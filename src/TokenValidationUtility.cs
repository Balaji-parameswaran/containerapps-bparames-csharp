/// -------------------------------------------------------------
/// <copyright file="TokenValidationUtility.cs" company="Microsoft">
///     Copyright (c) Microsoft Corporation. All rights reserved.
/// </copyright>
/// -------------------------------------------------------------

namespace Microsoft.AzureData.DataProcessing.Security.AzureAD
{
    using Microsoft.AzureData.DataProcessing.Logging;
    using Microsoft.AzureData.Tracing.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Identity.ServiceEssentials;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class TokenValidationUtility : ITokenValidationUtility
    {
        private readonly ILoggerExtendedFactory loggerExtendedFactory;

        private static TokenValidationUtility tokenValidationUtilityMiseInstance = null;
        private const string TokenValidationUtilityLogIdentifier = nameof(TokenValidationUtility);
        private readonly MiseHost<MiseHttpContext> MiseHost;

        public TokenValidationUtility(ILoggerExtendedFactory loggerExtendedFactory, MiseHost<MiseHttpContext> miseHost)
        {
            this.loggerExtendedFactory = loggerExtendedFactory ?? throw new ArgumentNullException(nameof(loggerExtendedFactory));
            this.MiseHost = miseHost;
        }

        public async Task<ClaimsPrincipal> GetClaimsAsync(TrackingContext trackingContext, IDictionary<string, StringValues> requestHeaders, CancellationToken cancellationToken = default)
        {
            var logger = this.loggerExtendedFactory.CreateScopedLogger<JwtTokenValidator>(trackingContext);
            ClaimsPrincipal claims = null;

            StringValues authorizationHeader;
            if (requestHeaders.TryGetValue("Authorization", out authorizationHeader))
            {
                string authorizationHeaderContent = authorizationHeader.FirstOrDefault();
                if (!string.IsNullOrEmpty(authorizationHeaderContent) && authorizationHeaderContent.Contains("Bearer", StringComparison.InvariantCultureIgnoreCase))
                {

                    // Obtain http request data from your stack
                    var httpRequestData = new HttpRequestData();
                    httpRequestData.Headers.Add("Authorization", authorizationHeaderContent);

                    /*** 1. create mise http context object (for each request) ***/
                    var context = new MiseHttpContext(httpRequestData) { };

                    /*** 2. execute mise (for each request) ***/
                    var miseResult = await this.MiseHost.HandleAsync(context, cancellationToken).ConfigureAwait(false);

                    /*** 3. examine results (for each request) ***/
                    if (miseResult.Succeeded)
                    {
                        logger.LogInformation($"Request {trackingContext.RequestId} was validated successfully.");
                        claims = new ClaimsPrincipal(miseResult.AuthenticationTicket.SubjectIdentity ?? miseResult.AuthenticationTicket.ActorIdentity);
                    }
                    else
                    {
                        logger.LogInformation($"Validation for request {trackingContext.RequestId} failed with Exception: {miseResult.Failure}");

                        /*** 3.2 examine failure, and/or http response produced by a module that failed to handle the request ***/
                        var moduleCreatedFailureResponse = miseResult.MiseContext.ModuleFailureResponse;
                        if (moduleCreatedFailureResponse != null)
                        {
                            logger.LogInformation($"Request {trackingContext.RequestId} returned HTTP status code: {moduleCreatedFailureResponse.StatusCode}");

                        }
                    }
                }
            }
            else
            {
                logger.LogError($"Failed to get the authorization headers for request {trackingContext.RequestId}");
            }
            return claims;
        }
    }
}
