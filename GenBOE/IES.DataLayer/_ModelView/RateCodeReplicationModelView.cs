// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System.Collections.Generic;
    using IES.Common.Exceptions;

    /// <summary>
    /// Model View for the Rate Code Replication page in RDM.
    /// </summary>
    public class RateCodeReplicationModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RateCodeReplicationModelView"/> class.
        /// </summary>
        public RateCodeReplicationModelView()
        {
            this.RateCodes = new List<RateDto>();
            this.Replications = new List<RateCodeModelView>();
        }

        /// <summary>
        /// Gets or sets the rate codes.
        /// </summary>
        public ICollection<RateDto> RateCodes { get; set; }

        /// <summary>
        /// Gets or sets the replication configuration.
        /// </summary>
        public ICollection<RateCodeModelView> Replications { get; set; }

        /// <summary>
        /// Gets or sets lock information
        /// </summary>
        public LockModelView LockInfo { get; set; }

        /// <summary>
        /// Gets or sets the validation messages.
        /// </summary>
        public ICollection<ValidationMessage> ValidationMessages { get; set; }
    }
}
