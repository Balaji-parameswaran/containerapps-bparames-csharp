using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace albumapi_csharp.Controllers
{
    public class ValidateIMDSnet6 : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Get the IMDS token from the header
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

        }
    }
}
