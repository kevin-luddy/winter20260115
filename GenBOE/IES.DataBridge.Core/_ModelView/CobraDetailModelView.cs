// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using IES.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// The Model View used for COBRA Mapping Details.
    /// </summary>
    public class CobraDetailModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CobraDetailModelView"/> class.
        /// </summary>
        public CobraDetailModelView()
        {
            this.Id = -1;
            this.RevisionId = -1;
            this.RateCategoryDescription = string.Empty;
            this.Description = string.Empty;
            this.Dirty = false;
        }

        /// <summary>
        /// RevisionId for the Rate.
        /// </summary>
        [JsonProperty(PropertyName = "R")]
        public int RevisionId { get; set; }

        /// <summary>
        /// Gets or sets the rate code.
        /// </summary>
        [JsonProperty(PropertyName = "Co")]
        public string RateCode { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [JsonProperty(PropertyName = "De")]
        public string Description { get; set; }

        /// <summary>
        /// Rate Category Description
        /// </summary>
        [JsonProperty(PropertyName = "RCD")]
        public string RateCategoryDescription { get; set; }

        /// <summary>
        /// Gets or sets the Cobra rate set.
        /// </summary>
        [JsonProperty(PropertyName = "RS")]
        public string RateSet { get; set; }

        /// <summary>
        /// COBRA Code1.
        /// </summary>
        [JsonIgnore]
        private Code1? code1;

        /// <summary>
        /// Gets or sets the COBRA Code1 value.
        /// </summary>
        [JsonProperty(PropertyName = "C1")]
        public Code1? Code1
        {
            get
            {
                return this.code1;
            }

            set
            {
                this.Code1Description = value.HasValue ? value.GetDescription() : string.Empty;
                this.code1 = value;
            }
        }

        /// <summary>
        /// Client needs Description for presentation.
        /// </summary>
        [JsonProperty(PropertyName = "C1D")]
        public string Code1Description { get; set; }

        #region BulkLoader Methods

        /// <summary>
        /// Propagates the new 'parent' DTO Id to all first level 'child' DTOs in collections.
        /// </summary>
        /// <param name="newParentId">new id of the parent DTO</param>
        protected override void PropagateNewParentIdToChildDTOs(int newParentId)
        {
            //// There are no child collections in this dto so there is no work to do.
        }

        #endregion
    }
}
