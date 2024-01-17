// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
	using Newtonsoft.Json;

	/// <summary>
	/// The Model View used for ProPricer mappings.
	/// </summary>
	[Serializable]
    public class ProPricerRateCodeXrefModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProPricerRateCodeXrefModelView"/> class.
        /// </summary>
        public ProPricerRateCodeXrefModelView()
        {
            this.RateCodeId = -1;
        }

        /// <summary>
        /// Gets or sets the mapping description.
        /// </summary>
        [JsonProperty(PropertyName = "Desc")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the RateCodeExtensionId.
        /// </summary>
        [JsonProperty(PropertyName = "RCE")]
        public int? RateCodeExtensionId { get; set; }

        /// <summary>
        /// Gets or sets the RateCodeId for this mapping.
        /// </summary>
        [JsonProperty(PropertyName = "RCI")]
        public int RateCodeId { get; set; }

        /// <summary>
        /// Gets or sets the ResourceClassId.
        /// </summary>
        [JsonProperty(PropertyName = "RCId")]
        public int? ResourceClassId { get; set; }

        /// <summary>
        /// Gets or sets the ResourceClass.
        /// </summary>
        [JsonProperty(PropertyName = "RC")]
        public string ResourceClass { get; set; }

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
