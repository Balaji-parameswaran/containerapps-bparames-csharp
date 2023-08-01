/// -------------------------------------------------------------
/// <copyright file="TokenValidationResult.cs" company="Microsoft">
///     Copyright (c) Microsoft Corporation. All rights reserved.
/// </copyright>
/// -------------------------------------------------------------

namespace Microsoft.AzureData.DataProcessing.Security.AzureAD
{
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using System;
    using System.Security.Claims;

    public class TokenValidationResult
    {
        /// <summary>
        /// Initializes new instance of type <see cref="TokenValidationResult"/>.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The error message.</param>
        public TokenValidationResult(int errorCode, string message)
        {
            this.ErrorCode = errorCode;
            this.Message = message;
            this.IsSuccessful = false;
        }

        /// <summary>
        /// Initializes new instance of type <see cref="TokenValidationResult"/>.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="claimsPrincipal">The claims principal containing claims identity for tests.</param>
        public TokenValidationResult(int errorCode, string message, ClaimsPrincipal claimsPrincipal)
        {
            this.ErrorCode = errorCode;
            this.Message = message;
            this.ClaimsPrincipal = claimsPrincipal;
            this.IsSuccessful = false;
        }

        /// <summary>
        /// Initializes new instance of type <see cref="TokenValidationResult"/>.
        /// </summary>
        /// <param name="claimsPrincipal">The successful extracted <see cref="System.Security.Claims.ClaimsPrincipal"/> instance.</param>
        /// <param name="securityToken">The successful extracted <see cref="Microsoft.IdentityModel.Tokens.SecurityToken"/> instance.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the parameters is null.</exception>
        public TokenValidationResult(ClaimsPrincipal claimsPrincipal, SecurityToken securityToken)
        {
            this.ClaimsPrincipal = claimsPrincipal ?? throw new ArgumentNullException(nameof(claimsPrincipal));
            this.SecurityToken = securityToken ?? throw new ArgumentNullException(nameof(securityToken));
            this.IsSuccessful = true;
        }

        /// <summary>
        /// Initializes new instance of type <see cref="TokenValidationResult"/>.
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public TokenValidationResult(ClaimsPrincipal claimsPrincipal)
        {
            this.ClaimsPrincipal = claimsPrincipal ?? throw new ArgumentNullException(nameof(claimsPrincipal));
            this.IsSuccessful = true;
        }

        /// <summary>
        /// Value indicating whether the token validation was successful.
        /// </summary>
        public bool IsSuccessful { get; private set; }

        /// <summary>
        /// Error code of token validation result.
        /// </summary>
        public int? ErrorCode { get; private set; }

        /// <summary>
        /// The messsage field in the token validation result.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// ClaimsPrincipal has the set of claims.
        /// </summary>
        [JsonIgnore]
        public ClaimsPrincipal ClaimsPrincipal { get; private set; }

        /// <summary>
        /// Gets the security token.
        /// </summary>
        [JsonIgnore]
        public SecurityToken SecurityToken { get; private set; }
    }
}
