// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using IES.Common;
    using Newtonsoft.Json;

    /// <summary>
    /// The Model View used for Rate Details.
    /// </summary>
    [Serializable]
    public class RateDetailModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RateDetailModelView"/> class.
        /// </summary>
        public RateDetailModelView()
        {
            this.Id = -1;
            this.RevisionId = -1;
            this.RateCategoryDescription = string.Empty;
            this.RateDescription = string.Empty;
            this.RateDescription1 = string.Empty;
            this.RateDescription2 = string.Empty;
            this.RateDescription3 = string.Empty;
            this.RateDescription4 = string.Empty;
            this.RateDescription5 = string.Empty;
            this.RateDescription6 = string.Empty;
            this.RateDescription7 = string.Empty;
            this.ResourceClass = string.Empty;
            this.ResourceClass1 = string.Empty;
            this.ResourceClass2 = string.Empty;
            this.ResourceClass3 = string.Empty;
            this.ResourceClass4 = string.Empty;
            this.ResourceClass5 = string.Empty;
            this.ResourceClass6 = string.Empty;
            this.ResourceClass7 = string.Empty;
            this.ResourceClassId = 0;
            this.ResourceClassId1 = 0;
            this.ResourceClassId2 = 0;
            this.ResourceClassId3 = 0;
            this.ResourceClassId4 = 0;
            this.ResourceClassId5 = 0;
            this.ResourceClassId6 = 0;
            this.ResourceClassId7 = 0;
            this.RatePrecision = -1;
            this.GovernmentBurdenPool = string.Empty;
            this.CommercialBurdenPool = string.Empty;
            this.Dirty = false;
            this.Values = null;
            this.HasProPricerBurdenRateMappings = false;
        }

        /// <summary>
        /// RevisionId for the Rate.
        /// </summary>
        [JsonProperty(PropertyName = "R")]
        public int RevisionId { get; set; }

        /// <summary>
        /// Gets or sets the rate category.
        /// </summary>
        [JsonIgnore]
        private RateCategory rateCategory;

        /// <summary>
        /// Gets or sets the rate category.
        /// </summary>
        [JsonProperty(PropertyName = "RC")]
        [Required]
        public RateCategory RateCategory
        {
            get
            {
                return this.rateCategory;
            }

            set
            {
                this.RateCategoryDescription = value.GetDescription();
                this.rateCategory = value;
            }
        }

        /// <summary>
        /// Client needs Description for presentation and value for looking up precisions in the dictionary.
        /// </summary>
        [JsonProperty(PropertyName = "RCD")]
        [Required]
        public string RateCategoryDescription { get; set; }

        /// <summary>
        /// Rate precision read from WebConfig.
        /// </summary>
        [JsonIgnore]
        public int RatePrecision { get; set; }

        /// <summary>
        /// Gets or sets the section.
        /// </summary>
        [JsonProperty(PropertyName = "S")]
        [Required]
        public int? Section { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [JsonProperty(PropertyName = "De")]
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the rate code.
        /// </summary>
        [JsonProperty(PropertyName = "Co")]
        [Required]
        public string RateCode { get; set; }

        /// <summary>
        /// Gets or sets the CompareState.
        /// </summary>
        [JsonProperty(PropertyName = "CS")]
        public string CompareState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        [JsonProperty(PropertyName = "Del")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets all of the Values by Year.
        /// </summary>
        [Required]
        public ICollection<RateYearModelView> Values { get; set; }

        #region ProPricer Direct Rates
        /// <summary>
        /// ResourceTypeField
        /// </summary>
        [JsonProperty(PropertyName = "RT")]
        public DirectRateMappingResourceType? ResourceType { get; set; }

        /// <summary>
        /// RateType
        /// </summary>
        [JsonProperty(PropertyName = "Ra")]
        public RateType? RateType { get; set; }

        /// <summary>
        /// GenerateAdditionalDirectLaborRates
        /// </summary>
        [JsonProperty(PropertyName = "Gen")]
        public bool GenerateAdditionalDirectLaborRates { get; set; }

        /// <summary>
        /// description of rate for single (no additional rate codes)
        /// </summary>
        [JsonIgnore]
        public ICollection<ProPricerRateCodeXrefModelView> ProPricerMappings { get; set; }

        // Descriptions for rate codes 1-6 if generateAdditionalDirectLaborRates == true (all optional)
        // If these method names are changed from "RateDescription" you must modify the CommonConstants.RateDescriptionMethodBase.
        // It is used in the Loader.

        /// <summary>
        /// description of rate for single (no additional rate codes)
        /// </summary>
        [JsonProperty(PropertyName = "RD")]
        public string RateDescription { get; set; }

        /// <summary>
        /// ResourceClass
        /// </summary>
        [JsonIgnore]
        public string ResourceClass { get; set; }

        /// <summary>
        /// ResourceClassId
        /// </summary>
        [JsonProperty(PropertyName = "RCId")]
        public int? ResourceClassId { get; set; }

        /// <summary>
        /// RateDescription1
        /// </summary>
        [JsonProperty(PropertyName = "RD1")]
        public string RateDescription1 { get; set; }

        /// <summary>
        /// ResourceClass1
        /// </summary>
        [JsonIgnore]
        public string ResourceClass1 { get; set; }

        /// <summary>
        /// ResourceClassId1
        /// </summary>
        [JsonProperty(PropertyName = "RCId1")]
        public int? ResourceClassId1 { get; set; }

        /// <summary>
        /// RateDescription2
        /// </summary>
        [JsonProperty(PropertyName = "RD2")]
        public string RateDescription2 { get; set; }

        /// <summary>
        /// ResourceClass2
        /// </summary>
        [JsonIgnore]
        public string ResourceClass2 { get; set; }

        /// <summary>
        /// ResourceClassId2
        /// </summary>
        [JsonProperty(PropertyName = "RCId2")]
        public int? ResourceClassId2 { get; set; }

        /// <summary>
        /// RateDescription3
        /// </summary>
        [JsonProperty(PropertyName = "RD3")]
        public string RateDescription3 { get; set; }

        /// <summary>
        /// ResourceClass3
        /// </summary>
        [JsonIgnore]
        public string ResourceClass3 { get; set; }

        /// <summary>
        /// ResourceClassId3
        /// </summary>
        [JsonProperty(PropertyName = "RCId3")]
        public int? ResourceClassId3 { get; set; }

        /// <summary>
        /// RateDescription4
        /// </summary>
        [JsonProperty(PropertyName = "RD4")]
        public string RateDescription4 { get; set; }

        /// <summary>
        /// ResourceClass4
        /// </summary>
        [JsonIgnore]
        public string ResourceClass4 { get; set; }

        /// <summary>
        /// ResourceClassId4
        /// </summary>
        [JsonProperty(PropertyName = "RCId4")]
        public int? ResourceClassId4 { get; set; }

        /// <summary>
        /// RateDescription5
        /// </summary>
        [JsonProperty(PropertyName = "RD5")]
        public string RateDescription5 { get; set; }

        /// <summary>
        /// ResourceClass5
        /// </summary>
        [JsonIgnore]
        public string ResourceClass5 { get; set; }

        /// <summary>
        /// ResourceClassId5
        /// </summary>
        [JsonProperty(PropertyName = "RCId5")]
        public int? ResourceClassId5 { get; set; }

        /// <summary>
        /// RateDescription6
        /// </summary>
        [JsonProperty(PropertyName = "RD6")]
        public string RateDescription6 { get; set; }

        /// <summary>
        /// ResourceClass6
        /// </summary>
        [JsonIgnore]
        public string ResourceClass6 { get; set; }

        /// <summary>
        /// ResourceClassId6
        /// </summary>
        [JsonProperty(PropertyName = "RCId6")]
        public int? ResourceClassId6 { get; set; }

        /// <summary>
        /// RateDescription7
        /// </summary>
        [JsonProperty(PropertyName = "RD7")]
        public string RateDescription7 { get; set; }

        /// <summary>
        /// ResourceClass7
        /// </summary>
        [JsonIgnore]
        public string ResourceClass7 { get; set; }

        /// <summary>
        /// ResourceClassId7
        /// </summary>
        [JsonProperty(PropertyName = "RCId7")]
        public int? ResourceClassId7 { get; set; }

        /// <summary>
        /// GovernmentBurdenPool
        /// </summary>
        [JsonIgnore]
        public string GovernmentBurdenPool { get; set; }

        /// <summary>
        /// GovernmentBurdenPoolId
        /// </summary>
        [JsonProperty(PropertyName = "GBPId")]
        public int? GovernmentBurdenPoolId { get; set; }

        /// <summary>
        /// CommercialBurdenPool
        /// </summary>
        [JsonIgnore]
        public string CommercialBurdenPool { get; set; }

        /// <summary>
        /// CommercialBurdenPoolId
        /// </summary>
        [JsonProperty(PropertyName = "CBPId")]
        public int? CommercialBurdenPoolId { get; set; }

        /// <summary>
        /// Bool noting if there are ProPricer Burden Rate Mappings
        /// </summary>
        [JsonProperty(PropertyName = "HM")]
        public bool HasProPricerBurdenRateMappings { get; set; }

        #endregion

        #region BulkLoader Methods

        /// <summary>
        /// Propagates the new 'parent' DTO Id to all first level 'child' DTOs in collections.
        /// </summary>
        /// <param name="newParentId">new id of the parent DTO</param>
        protected override void PropagateNewParentIdToChildDTOs(int newParentId)
        {
            //// now update the child DTOs in each collection on this element
            if (this.ProPricerMappings != null && this.ProPricerMappings.Any())
            {
                foreach (ProPricerRateCodeXrefModelView proPricerMV in this.ProPricerMappings)
                {
                    proPricerMV.RateCodeId = newParentId;
                }
            }

            if (this.Values != null && this.Values.Any())
            {
                foreach (RateYearModelView rateYearMV in this.Values)
                {
                    rateYearMV.RateCodeId = newParentId;
                }
            }
        }

        #endregion
    }
}
