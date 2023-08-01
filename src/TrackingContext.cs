/// -------------------------------------------------------------
/// <copyright file="TrackingContext.cs" company="Microsoft">
///     Copyright (c) Microsoft Corporation. All rights reserved.
/// </copyright>
/// -------------------------------------------------------------

namespace Microsoft.AzureData.Tracing.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a context that is designed to be used for tracking various operations and calls.
    /// </summary>
    public class TrackingContext
    {
        public static TrackingContext Empty => new TrackingContext(Guid.Empty, Guid.Empty, Guid.Empty);

        /// <summary>
        /// Initializes new instance of type <see cref="TrackingContext"/>.
        /// </summary>
        /// <param name="requestId">The current client request id.</param>
        /// <param name="trackingId">The current generated id.</param>
        /// <param name="correlationVector">The current client correlation vector.</param>
        public TrackingContext(Guid requestId, Guid trackingId, Guid correlationVector)
        {
            this.TrackingId = trackingId;
            this.RequestId = requestId;
            this.CorrelationVector = correlationVector;
        }

        /// <summary>
        /// Initializes new instance of type <see cref="TrackingContext"/>.
        /// </summary>
        ///
        public TrackingContext()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public TrackingContext(TrackingContext t)
            : this(t.TrackingId, t.RequestId,t.CorrelationVector,t.ResourceUri,t.ApiVersion,t.TenantId)
        {
        }
        /// <summary>
        /// Initializes new instance of type <see cref="TrackingContext"/>.
        /// </summary>
        /// <param name="requestId">The current client request id.</param>
        /// <param name="trackingId">The current generated id.</param>
        /// <param name="correlationVector">The current client correlation vector.</param>
        /// <param name="resourceUri">The Azure resource <see cref="Uri"/> for which data is uploaded.</param>
        /// <param name="apiVersion">The current API version provided by clients.</param>
        [JsonConstructor]
        public TrackingContext(Guid requestId, Guid trackingId, Guid correlationVector, string resourceUri, string apiVersion)
        {
            this.TrackingId = trackingId;
            this.RequestId = requestId;
            this.CorrelationVector = correlationVector;
            this.ResourceUri = resourceUri;
            this.ApiVersion = apiVersion;
        }

        /// <summary>
        /// Initializes new instance of type <see cref="TrackingContext"/>.
        /// </summary>
        /// <param name="requestId">The current client request id.</param>
        /// <param name="trackingId">The current generated id.</param>
        /// <param name="correlationVector">The current client correlation vector.</param>
        /// <param name="resourceUri">The Azure resource <see cref="Uri"/> for which data is uploaded.</param>
        /// <param name="apiVersion">The current API version provided by clients.</param>
        /// <param name="tenantId">The tenant id provided by clients.</param>
        public TrackingContext(Guid requestId, Guid trackingId, Guid correlationVector, string resourceUri, string apiVersion, Guid tenantId)
        {
            this.TrackingId = trackingId;
            this.RequestId = requestId;
            this.CorrelationVector = correlationVector;
            this.ResourceUri = resourceUri;
            this.ApiVersion = apiVersion;
            this.TenantId = tenantId;
        }

        /// <summary>
        /// Gets the request id for this context.
        /// </summary>
        public Guid RequestId { get; private set; }

        /// <summary>
        /// Gets the tracking id for this context.
        /// </summary>
        public Guid TrackingId { get; private set; }

        /// <summary>
        /// Gets the correlation vector for this context.
        /// </summary>
        public Guid CorrelationVector { get; private set; }

        /// <summary>
        /// Gets the azure resource <see cref="Uri"/>.
        /// </summary>
        public string ResourceUri { get; private set; }

        /// <summary>
        /// Gets the current API version provided by clients.
        /// </summary>
        public string ApiVersion { get; private set; }

        /// <summary>
        /// Gets the tenant id for this context.
        /// </summary>
        public Guid TenantId { get; private set; }

        #region Overrides

        public override string ToString()
        {
            return $"RequestId={this.RequestId}; TrackingId={this.TrackingId}; CorrelationVector={this.CorrelationVector}; ResourceUri={this.ResourceUri}; ApiVersion={this.ApiVersion};";
        }

        public override bool Equals(object obj)
        {
            var trackingContext = obj as TrackingContext;
            if (obj == null)
            {
                return false;
            }

            return this.RequestId == trackingContext.RequestId &&
                this.TrackingId == trackingContext.TrackingId &&
                this.CorrelationVector == trackingContext.CorrelationVector &&
                string.Equals(this.ResourceUri, trackingContext.ResourceUri, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.ApiVersion, trackingContext.ApiVersion, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            // Automatically generated by Visual Studio
            var hashCode = -1264182446;
            hashCode = hashCode * -1521134295 + RequestId.GetHashCode();
            hashCode = hashCode * -1521134295 + TrackingId.GetHashCode();
            hashCode = hashCode * -1521134295 + CorrelationVector.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ResourceUri);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ApiVersion);
            return hashCode;
        }

        #endregion
    }
}
