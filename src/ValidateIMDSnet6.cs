

namespace albumapi_csharp.Controllers
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

    public class ValidateIMDSnet6 : Attribute, IAsyncAuthorizationFilter
    {
        public async Task<ClaimsPrincipal> OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Get the IMDS token from the header
            ClaimsPrincipal claims = null;
            var imdsToken = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            StringValues authorizationHeader;
            if (requestHeaders.TryGetValue("Authorization", out authorizationHeader))
            {
                string authorizationHeaderContent = imdsToken;
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
            /*
            var imdsToken = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(imdsToken))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Validate the IMDS token format and signature
            var jwtHandler = new JwtSecurityTokenHandler();
            if (!jwtHandler.CanReadToken(imdsToken))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            var jwtToken = jwtHandler.ReadJwtToken(imdsToken);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "https://sts.windows.net/72f988bf-86f1-41af-91ab-2d7cd011db47/",
                ValidAudience = "https://management.azure.com/",
                IssuerSigningKey = new X509SecurityKey(jwtToken.Header.X5c.First())
            };
            try
            {
                jwtHandler.ValidateToken(imdsToken, validationParameters, out _);
            }
            catch (Exception ex)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get the xms_mirid claim from the IMDS token
            var xmsMiridClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "xms_mirid");
            if (xmsMiridClaim == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Validate the xms_mirid claim value
            var xmsMiridParts = xmsMiridClaim.Value.Split('/');
            if (xmsMiridParts.Length != 4 || xmsMiridParts[0] != "subscriptions" || xmsMiridParts[2] != "resourcegroups")
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get the subscription id, resource group name, and resource name from the xms_mirid claim value
            var subscriptionId = xmsMiridParts[1];
            var resourceGroupName = xmsMiridParts[3];
            var resourceName = xmsMiridParts[4];

            // Get the current VM's metadata using Azure Identity library and Managed Identity credential
            var credential = new ManagedIdentityCredential();
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await credential.GetTokenAsync(new TokenRequestContext(new[] { "https://management.azure.com/.default" })));

            // Compare the current VM's metadata with the xms_mirid claim value
            var response = await httpClient.GetAsync($"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Compute/virtualMachines/{resourceName}?api-version=2021-07-01");

            if (!response.IsSuccessStatusCode)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

        }*/
        }
}
