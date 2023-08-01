/// -------------------------------------------------------------
/// <copyright file="JwtTokenValidator.cs" company="Microsoft">
///     Copyright (c) Microsoft Corporation. All rights reserved.
/// </copyright>
/// -------------------------------------------------------------

namespace Microsoft.AzureData.DataProcessing.Security.AzureAD
{
    using Microsoft.AzureData.DataProcessing.Logging;
    using Microsoft.AzureData.DataProcessing.Logging.Models;
    using Microsoft.AzureData.Tracing.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// This <see cref="ITokenValidator"/> that will be responsible for validating JWT tokens.
    /// </summary>
    public class JwtTokenValidator : ITokenValidator
    {
        /// <summary>
        /// The Qos component name for <see cref="JwtTokenValidator"/> class.
        /// </summary>
        private static readonly string QosOperationComponentName = "JwtTokenValidationQos";

        /// <summary>
        /// In memory cache of valid issuers per tenant id.
        /// </summary>
        private ConcurrentDictionary<string, IList<string>> validIssuersCache = new ConcurrentDictionary<string, IList<string>>();

        /// <summary>
        /// The current <see cref="ILoggerExtendedFactory"/> instance to create <see cref="ILogger"/> instance for this class.
        /// </summary>
        private readonly ILoggerExtendedFactory loggerExtendedFactory;

        /// <summary>
        /// The current associated <see cref="AzureADSettings"/> for this <see cref="JwtTokenValidator"/>.
        /// </summary>
        private readonly AzureADSettings azureADSettings;

        private readonly ITokenValidationUtility tokenValidationUtility;

        /// <summary>
        /// Initializes new instance of type <see cref="JwtTokenValidator"/>.
        /// </summary>
        /// <param name="loggerExtendedFactory">The <see cref="ILoggerExtendedFactory"/> to create logger for <see cref="JwtTokenValidator"/> to use.</param>
        /// <param name="azureADSettings">The current <see cref="AzureADSettings"/> to be associated with this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the parameters is null.</exception>
        public JwtTokenValidator(ILoggerExtendedFactory loggerExtendedFactory, AzureADSettings azureADSettings, ITokenValidationUtility tokenValidationUtility)
        {
            this.loggerExtendedFactory = loggerExtendedFactory ?? throw new ArgumentNullException(nameof(loggerExtendedFactory));
            this.azureADSettings = azureADSettings ?? throw new ArgumentNullException(nameof(azureADSettings));
            this.tokenValidationUtility = tokenValidationUtility ?? throw new ArgumentNullException(nameof(tokenValidationUtility));
        }

        #region ITokenValidator Members

        public async Task<TokenValidationResult> ValidateAsync(TrackingContext trackingContext, IDictionary<string, StringValues> requestHeaders, CancellationToken cancellationToken)
        {
            using (var logger = this.loggerExtendedFactory.CreateScopedLogger<JwtTokenValidator>(trackingContext))
            {
                try
                {
                    var claims = await this.ValidateTokenAsync(trackingContext, requestHeaders, cancellationToken);
                    this.CustomValidations(claims);
                    logger.LogTelemetry(QosResult.Success(QosOperationComponentName));
                    return new TokenValidationResult(claims);
                }
                catch (SecurityTokenException securityTokenException)
                {
                    string errorMessage = "Failed to validate token. Invalid token provided.";
                    logger.LogTelemetry(QosResult.BadRequest(QosOperationComponentName), errorMessage, securityTokenException);
                    logger.LogError(securityTokenException, errorMessage);
                    TokenValidationResult result = new TokenValidationResult(400, securityTokenException.ToString());
                    return result;
                }
                catch (Exception exception)
                {
                    string errorMessage = "Failed to validate accessToken with general exception.";
                    logger.LogTelemetry(QosResult.Failure(QosOperationComponentName), errorMessage, exception);
                    logger.LogError(exception, errorMessage);
                    TokenValidationResult result = new TokenValidationResult(500, exception.ToString());
                    return result;
                }
            }
        }

        #endregion

        #region Private Members
        /// <summary>
        /// Performs additional validations.
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        private void CustomValidations(ClaimsPrincipal claims)
        {
            this.ValidateTenantId(claims);
            this.ValidateIssuer(claims);
            this.ValidateAppId(claims);
            this.ValidateAppRoles(claims);
        }

        /// <summary>
        /// Validates the current <paramref name="claimsPrincipal"/>'s tenant id is allowed.
        /// </summary>
        /// <param name="claimsPrincipal">The current <see cref="ClaimsPrincipal"/> from successfully authenticated JWT token.</param>
        /// <exception cref="SecurityTokenException">Thrown if validation fails.</exception>
        private void ValidateTenantId(ClaimsPrincipal claimsPrincipal)
        {
            if (this.azureADSettings.AllowedTenantIds.Length > 0)
            {
                var tenantId = Guid.Parse(claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value ?? claimsPrincipal.FindFirst("tid").Value);
                if (!this.azureADSettings.AllowedTenantIds.Contains(tenantId))
                {
                    throw new SecurityTokenException($"The provided token is issued from tenant id = {tenantId} which is not allowed.");
                }
            }
        }

        /// <summary>
        /// Validates the current <paramref name="claimsPrincipal"/>'s issuer is supported.
        /// </summary>
        /// <param name="claimsPrincipal">The current <see cref="ClaimsPrincipal"/> from successfully authenticated JWT token.</param>
        /// <exception cref="SecurityTokenException">Thrown if validation fails.</exception>
        private void ValidateIssuer(ClaimsPrincipal claimsPrincipal)
        {
            var issuer = claimsPrincipal.FindFirst("iss").Value;
            var tenantId = claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value ?? claimsPrincipal.FindFirst("tid").Value;

            var validIssuers = this.validIssuersCache.GetOrAdd(tenantId, (tid) =>
            {
                return new List<string>()
                {
                    $"https://login.microsoftonline.com/{tid}/",
                    $"https://login.microsoftonline.com/{tid}/v2.0",
                    $"https://login.windows.net/{tid}/",
                    $"https://login.microsoft.com/{tid}/",
                    $"https://sts.windows.net/{tid}/",
                    $"https://login.windows-ppe.net/{tid}",
                    $"https://sts.windows-ppe.net/{tid}/"
                };
            });

            if (!validIssuers.Contains(issuer, StringComparer.OrdinalIgnoreCase))
            {
                throw new SecurityTokenException($"The provided token is issued from issuer = {issuer} which is not supported.");
            }
        }

        /// <summary>
        /// Validates the first party application id claim from JWT token.
        /// It helps to determine who generated the token
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        private void ValidateAppId(ClaimsPrincipal claimsPrincipal)
        {
            if (this.azureADSettings.AllowedAppIds.Length > 0)
            {
                var appId = Guid.Parse(claimsPrincipal.FindFirst("appid").Value);
                if (!this.azureADSettings.AllowedAppIds.Contains(appId))
                {
                    throw new SecurityTokenException($"The provided token is issued from app id = {appId} which is not allowed.");
                }
            }
        }

        /// <summary>
        /// Validates if the current <paramref name="claimsPrincipal"/> contains all the required application roles.
        /// </summary>
        /// <param name="claimsPrincipal">The current <see cref="ClaimsPrincipal"/> from successfully authenticated JWT token.</param>
        /// <exception cref="SecurityTokenException">Thrown if validation fails.</exception>
        private void ValidateAppRoles(ClaimsPrincipal claimsPrincipal)
        {
            if (this.azureADSettings.RequiredAppRoles != null && this.azureADSettings.RequiredAppRoles.Any())
            {
                var appRoles = claimsPrincipal.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                var missingRequiredRoles = this.azureADSettings.RequiredAppRoles
                    .Where(requiredRole => !appRoles.Any(tokenRole => tokenRole.Value.Equals(requiredRole, StringComparison.OrdinalIgnoreCase)));

                if (missingRequiredRoles != null && missingRequiredRoles.Any())
                {
                    throw new SecurityTokenException($"The provided token is missing the following required application roles = {JsonConvert.SerializeObject(missingRequiredRoles)}.");
                }
            }
        }

        private async Task<ClaimsPrincipal> ValidateTokenAsync(TrackingContext trackingContext, IDictionary<string, StringValues> requestHeaders, CancellationToken cancellationToken)
        {
            var claims = await this.tokenValidationUtility.GetClaimsAsync(trackingContext, requestHeaders, cancellationToken);
            if (claims == null)
            {
                throw new UnauthorizedAccessException(($"The request {trackingContext.RequestId} is unauthorized."));
            }
            return claims;

        }

        #endregion
    }
}
