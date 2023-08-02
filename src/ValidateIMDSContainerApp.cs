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
            Console.WriteLine(" token retrieved ");
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
