// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;
    using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;
	using Newtonsoft.Json;

    /// <summary>
    /// The Model View used for Rate Year Values.
    /// </summary>
    [Serializable]
    public class RateYearModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RateYearModelView"/> class.
        /// </summary>
        public RateYearModelView()
        {
            this.Id = -1;
            this.RateCodeId = -1;
            this.Value = null;
        }

        /// <summary>
        /// Gets or sets the RateCode.
        /// </summary>
        [JsonProperty(PropertyName = "RCId")]
        public int RateCodeId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [JsonProperty(PropertyName = "Val")]
        public decimal? Value { get; set; }

        /// <summary>
        /// Used in the compare page.
        /// </summary>
        [JsonProperty(PropertyName = "PC")]
        public decimal? PercentChange { get; set; }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        [JsonProperty(PropertyName = "Yr")]
        public int Year { get; set; }

        /// <summary>
        /// Last Update Date
        /// </summary>
        [JsonIgnore]
        public override DateTime UpdateDate { get; set; }

        /// <summary>
        /// Update Type
        /// </summary>
        [JsonIgnore]
        public override UpdateType Updateable { get; set; }

        /// <summary>
        /// Propagates the new 'parent' DTO Id to all first level 'child' DTOs in collections.
        /// </summary>
        /// <param name="newParentId">new id of the parent DTO</param>
        protected override void PropagateNewParentIdToChildDTOs(int newParentId)
        {
            //// There are no child collections in this dto so there is no work to do.
        }
    }
}
