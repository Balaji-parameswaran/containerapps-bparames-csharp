using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace IMDSValidation
{
    class ValidateIMDSContainerApp
    {
        // The Azure AD metadata endpoint
        private const string MetadataEndpoint = "https://login.microsoftonline.com/common/discovery/keys";

        // The IMDS token issuer
        private const string Issuer = "https://sts.windows.net/{tenant-id}/";

        // The IMDS token audience
        private const string Audience = "https://management.azure.com/";

        static void Main(string[] args)
        {
            // Get the IMDS token from the request header
            string imdsToken = GetIMDSTokenFromHeader();

            // Get the signing keys from the metadata endpoint
            List<SecurityKey> signingKeys = GetSigningKeys(MetadataEndpoint);

            // Validate the IMDS token
            ClaimsPrincipal principal = ValidateIMDSToken(imdsToken, signingKeys);

            // Validate the xms_mirid claim
            ValidateXmsMiridClaim(principal);
        }

        // Get the signing keys from the metadata endpoint
        private static List<SecurityKey> GetSigningKeys(string metadataEndpoint)
        {
            using (var client = new HttpClient())
            {
                // Get the JSON response from the endpoint
                var response = client.GetAsync(metadataEndpoint).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                // Parse the JSON response to get the keys
                var keySet = new JsonWebKeySet(content);
                return keySet.GetSigningKeys();
            }
        }

        // Validate the IMDS token
        private static ClaimsPrincipal ValidateIMDSToken(string imdsToken, List<SecurityKey> signingKeys)
        {
            // Create a token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // Create validation parameters
            var validationParameters = new TokenValidationParameters()
            {
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // Validate the token and return the principal
            return tokenHandler.ValidateToken(imdsToken, validationParameters, out _);
        }

        // Validate the xms_mirid claim
        private static void ValidateXmsMiridClaim(ClaimsPrincipal principal)
        {
            // Get the xms_mirid claim
            var claim = principal.FindFirst("xms_mirid");

            // Check if the claim exists
            if (claim == null)
            {
                throw new Exception("xms_mirid claim not found");
            }

            // Parse and validate the claim value according to your logic
            // The value format is /subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}/providers/{resourceProviderNamespace}/{resourceType}/{resourceName}
            var parts = claim.Value.Split('/');

            // For example, check if the resource type is virtualMachine or virtualMachineScaleSet
            if (parts.Length != 9 || (parts[6] != "virtualMachine" && parts[6] != "virtualMachineScaleSet"))
            {
                throw new Exception("Invalid xms_mirid claim value");
            }

            // Do other validations as needed
        }
    }
}
