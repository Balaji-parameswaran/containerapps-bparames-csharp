using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace IMDSValidation
{
    class ValidateIMDSContainerApp
    {
        public async Task<ClaimsPrincipal> ValidateIMDS(string token)
        {
            Console.WriteLine(" token retrieved " + token);
            // Get the OpenID Connect discovery document URL
            string discoveryUrl = "https://login.microsoftonline.com/common/.well-known/openid-configuration";

            // Get the configuration manager for the discovery document
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(discoveryUrl, new OpenIdConnectConfigurationRetriever());

            // Get the OpenID Connect configuration
            OpenIdConnectConfiguration openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);

            // Get the signing keys from the configuration
            var signingKeys = openIdConfig.SigningKeys;

            // Create a token validation parameters object
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                // Validate the token signature using the signing keys
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,

                // Validate the token issuer (optional)
                ValidateIssuer = true,
                ValidIssuer = "https://sts.windows.net/72f988bf-86f1-41af-91ab-2d7cd011db47/",

                // Validate the token audience (optional)
                ValidateAudience = true,
                ValidAudience = "https://storage.azure.com/",

                // Validate the token expiration
                ValidateLifetime = true,

                // Allow some clock skew
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            // Create a JWT security token handler
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            Console.WriteLine(" All declaration done ");
            try
            {
                // Validate the token using the validation parameters
                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Read the JWT security token
                var jwtToken = validatedToken as JwtSecurityToken;

                if (jwtToken == null)
                {
                    throw new Exception("The token is not a valid JWT token");
                }
                // Read the xms_mirid claim from the token
                var xmsMiridClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "xms_mirid");

                if (xmsMiridClaim == null)
                {
                    throw new Exception("The token does not contain the xms_mirid claim");
                }

                // Get the xms_mirid claim value
                var xmsMiridValue = xmsMiridClaim.Value;

                Console.WriteLine($"The xms_mirid claim value is: {xmsMiridValue}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return null;
            /* Create a JwtSecurityTokenHandler object
            var handler = new JwtSecurityTokenHandler();

            // Create a TokenValidationParameters object
            var parameters = new TokenValidationParameters()
            {
                // Specify the issuer of the token
                ValidIssuer = "http://example.com",

                // Specify the audience of the token
                ValidAudience = "http://example.com",

                // Specify the signing key of the token
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String("secret")),

                // Specify whether to validate the expiration of the token
                ValidateLifetime = true,

                // Specify the clock skew for expiration validation
                ClockSkew = TimeSpan.Zero
            };

            // Validate the token using the handler and parameters
            var principal = handler.ValidateToken(token, parameters, out _);

            // Return the ClaimsPrincipal object
            return principal;
            */
        }
    }
}
