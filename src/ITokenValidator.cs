/// -------------------------------------------------------------
/// <copyright file="ITokenValidator.cs" company="Microsoft">
///     Copyright (c) Microsoft Corporation. All rights reserved.
/// </copyright>
/// -------------------------------------------------------------

namespace Microsoft.AzureData.DataProcessing.Security.AzureAD
{
    using Microsoft.AzureData.Tracing.Models;
    using Microsoft.Extensions.Primitives;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a authentication token validator.
    /// </summary>
    public interface ITokenValidator
    {
        /// <summary>
        /// Validates the provided token for valid authentication information.
        /// </summary>
        /// <param name="trackingContext">The current <see cref="TrackingContext"/> to be used for the current call.</param>
        /// <param name="requestHeaders">The request headers.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the async operation.</param>
        /// <returns>The <see cref="TokenValidationResult"/> result from the operation.</returns>
        Task<TokenValidationResult> ValidateAsync(TrackingContext trackingContext, IDictionary<string, StringValues> requestHeaders, CancellationToken cancellationToken);
    }
}
