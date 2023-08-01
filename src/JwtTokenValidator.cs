/// -------------------------------------------------------------
/// <copyright file="JwtTokenValidator.cs" company="Microsoft">
///     Copyright (c) Microsoft Corporation. All rights reserved.
/// </copyright>
/// -------------------------------------------------------------

namespace Microsoft.AzureData.DataProcessing.Security.AzureAD
{
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

        private async Task<ClaimsPrincipal> ValidateTokenAsync(TrackingContext trackingContext, IDictionary<string, StringValues> requestHeaders, CancellationToken cancellationToken)
        {
            var claims = await this.tokenValidationUtility.GetClaimsAsync(trackingContext, requestHeaders, cancellationToken);
            if (claims == null)
            {
                throw new UnauthorizedAccessException(($"The request {trackingContext.RequestId} is unauthorized."));
            }
            return claims;

        }

        public async Task<TokenValidationResult> ValidateAsync(TrackingContext trackingContext, IDictionary<string, StringValues> requestHeaders, CancellationToken cancellationToken)
        {
            using (var logger = this.loggerExtendedFactory.CreateScopedLogger<JwtTokenValidator>(trackingContext))
            {
                try
                {
                    var claims = await this.ValidateTokenAsync(trackingContext, requestHeaders, cancellationToken);
                    //this.CustomValidations(claims);
                   // logger.LogTelemetry(QosResult.Success(QosOperationComponentName));
                    return new TokenValidationResult(claims);
                }
                catch (SecurityTokenException securityTokenException)
                {
                    string errorMessage = "Failed to validate token. Invalid token provided.";
                    
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

       
    }
}
