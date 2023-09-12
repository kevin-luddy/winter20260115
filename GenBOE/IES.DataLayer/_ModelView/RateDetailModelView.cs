// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
		/// Disclosure Type
		/// </summary>
		[JsonProperty(PropertyName = "DT")]
		public DisclosureType? DisclosureType { get; set; }

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

		#region 1LMX Direct Rates

		/// <summary>
		/// RateDescription11
		/// </summary>
		[JsonProperty(PropertyName = "RD11")]
		public string RateDescription11 { get; set; }

		/// <summary>
		/// ResourceClass11
		/// </summary>
		[JsonIgnore]
		public string ResourceClass11 { get; set; }

		/// <summary>
		/// ResourceClassId11
		/// </summary>
		[JsonProperty(PropertyName = "RCId11")]
		public int? ResourceClassId11 { get; set; }

		/// <summary>
		/// RateDescription12
		/// </summary>
		[JsonProperty(PropertyName = "RD12")]
		public string RateDescription12 { get; set; }

		/// <summary>
		/// ResourceClass12
		/// </summary>
		[JsonIgnore]
		public string ResourceClass12 { get; set; }

		/// <summary>
		/// ResourceClassId12
		/// </summary>
		[JsonProperty(PropertyName = "RCId12")]
		public int? ResourceClassId12 { get; set; }

		/// <summary>
		/// RateDescription13
		/// </summary>
		[JsonProperty(PropertyName = "RD13")]
		public string RateDescription13 { get; set; }

		/// <summary>
		/// ResourceClass13
		/// </summary>
		[JsonIgnore]
		public string ResourceClass13 { get; set; }

		/// <summary>
		/// ResourceClassId13
		/// </summary>
		[JsonProperty(PropertyName = "RCId13")]
		public int? ResourceClassId13 { get; set; }

		/// <summary>
		/// RateDescription14
		/// </summary>
		[JsonProperty(PropertyName = "RD14")]
		public string RateDescription14 { get; set; }

		/// <summary>
		/// ResourceClass14
		/// </summary>
		[JsonIgnore]
		public string ResourceClass14 { get; set; }

		/// <summary>
		/// ResourceClassId14
		/// </summary>
		[JsonProperty(PropertyName = "RCId14")]
		public int? ResourceClassId14 { get; set; }

		/// <summary>
		/// RateDescription15
		/// </summary>
		[JsonProperty(PropertyName = "RD15")]
		public string RateDescription15 { get; set; }

		/// <summary>
		/// ResourceClass15
		/// </summary>
		[JsonIgnore]
		public string ResourceClass15 { get; set; }

		/// <summary>
		/// ResourceClassId15
		/// </summary>
		[JsonProperty(PropertyName = "RCId15")]
		public int? ResourceClassId15 { get; set; }

		/// <summary>
		/// RateDescription21
		/// </summary>
		[JsonProperty(PropertyName = "RD21")]
		public string RateDescription21 { get; set; }

		/// <summary>
		/// ResourceClass21
		/// </summary>
		[JsonIgnore]
		public string ResourceClass21 { get; set; }

		/// <summary>
		/// ResourceClassId21
		/// </summary>
		[JsonProperty(PropertyName = "RCId21")]
		public int? ResourceClassId21 { get; set; }

		/// <summary>
		/// RateDescription22
		/// </summary>
		[JsonProperty(PropertyName = "RD22")]
		public string RateDescription22 { get; set; }

		/// <summary>
		/// ResourceClass22
		/// </summary>
		[JsonIgnore]
		public string ResourceClass22 { get; set; }

		/// <summary>
		/// ResourceClassId22
		/// </summary>
		[JsonProperty(PropertyName = "RCId22")]
		public int? ResourceClassId22 { get; set; }

		/// <summary>
		/// RateDescription23
		/// </summary>
		[JsonProperty(PropertyName = "RD23")]
		public string RateDescription23 { get; set; }

		/// <summary>
		/// ResourceClass23
		/// </summary>
		[JsonIgnore]
		public string ResourceClass23 { get; set; }

		/// <summary>
		/// ResourceClassId23
		/// </summary>
		[JsonProperty(PropertyName = "RCId23")]
		public int? ResourceClassId23 { get; set; }

		/// <summary>
		/// RateDescription24
		/// </summary>
		[JsonProperty(PropertyName = "RD24")]
		public string RateDescription24 { get; set; }

		/// <summary>
		/// ResourceClass24
		/// </summary>
		[JsonIgnore]
		public string ResourceClass24 { get; set; }

		/// <summary>
		/// ResourceClassId24
		/// </summary>
		[JsonProperty(PropertyName = "RCId24")]
		public int? ResourceClassId24 { get; set; }

		/// <summary>
		/// RateDescription25
		/// </summary>
		[JsonProperty(PropertyName = "RD25")]
		public string RateDescription25 { get; set; }

		/// <summary>
		/// ResourceClass25
		/// </summary>
		[JsonIgnore]
		public string ResourceClass25 { get; set; }

		/// <summary>
		/// ResourceClassId25
		/// </summary>
		[JsonProperty(PropertyName = "RCId25")]
		public int? ResourceClassId25 { get; set; }

		/// <summary>
		/// RateDescription31
		/// </summary>
		[JsonProperty(PropertyName = "RD31")]
		public string RateDescription31 { get; set; }

		/// <summary>
		/// ResourceClass31
		/// </summary>
		[JsonIgnore]
		public string ResourceClass31 { get; set; }

		/// <summary>
		/// ResourceClassId31
		/// </summary>
		[JsonProperty(PropertyName = "RCId31")]
		public int? ResourceClassId31 { get; set; }

		/// <summary>
		/// RateDescription32
		/// </summary>
		[JsonProperty(PropertyName = "RD32")]
		public string RateDescription32 { get; set; }

		/// <summary>
		/// ResourceClass32
		/// </summary>
		[JsonIgnore]
		public string ResourceClass32 { get; set; }

		/// <summary>
		/// ResourceClassId32
		/// </summary>
		[JsonProperty(PropertyName = "RCId32")]
		public int? ResourceClassId32 { get; set; }

		/// <summary>
		/// RateDescription33
		/// </summary>
		[JsonProperty(PropertyName = "RD33")]
		public string RateDescription33 { get; set; }

		/// <summary>
		/// ResourceClass33
		/// </summary>
		[JsonIgnore]
		public string ResourceClass33 { get; set; }

		/// <summary>
		/// ResourceClassId33
		/// </summary>
		[JsonProperty(PropertyName = "RCId33")]
		public int? ResourceClassId33 { get; set; }

		/// <summary>
		/// RateDescription34
		/// </summary>
		[JsonProperty(PropertyName = "RD34")]
		public string RateDescription34 { get; set; }

		/// <summary>
		/// ResourceClass34
		/// </summary>
		[JsonIgnore]
		public string ResourceClass34 { get; set; }

		/// <summary>
		/// ResourceClassId34
		/// </summary>
		[JsonProperty(PropertyName = "RCId34")]
		public int? ResourceClassId34 { get; set; }

		/// <summary>
		/// RateDescription35
		/// </summary>
		[JsonProperty(PropertyName = "RD35")]
		public string RateDescription35 { get; set; }

		/// <summary>
		/// ResourceClass35
		/// </summary>
		[JsonIgnore]
		public string ResourceClass35 { get; set; }

		/// <summary>
		/// ResourceClassId35
		/// </summary>
		[JsonProperty(PropertyName = "RCId35")]
		public int? ResourceClassId35 { get; set; }

		/// <summary>
		/// RateDescription41
		/// </summary>
		[JsonProperty(PropertyName = "RD41")]
		public string RateDescription41 { get; set; }

		/// <summary>
		/// ResourceClass41
		/// </summary>
		[JsonIgnore]
		public string ResourceClass41 { get; set; }

		/// <summary>
		/// ResourceClassId41
		/// </summary>
		[JsonProperty(PropertyName = "RCId41")]
		public int? ResourceClassId41 { get; set; }

		/// <summary>
		/// RateDescription42
		/// </summary>
		[JsonProperty(PropertyName = "RD42")]
		public string RateDescription42 { get; set; }

		/// <summary>
		/// ResourceClass42
		/// </summary>
		[JsonIgnore]
		public string ResourceClass42 { get; set; }

		/// <summary>
		/// ResourceClassId42
		/// </summary>
		[JsonProperty(PropertyName = "RCId42")]
		public int? ResourceClassId42 { get; set; }

		/// <summary>
		/// RateDescription43
		/// </summary>
		[JsonProperty(PropertyName = "RD43")]
		public string RateDescription43 { get; set; }

		/// <summary>
		/// ResourceClass43
		/// </summary>
		[JsonIgnore]
		public string ResourceClass43 { get; set; }

		/// <summary>
		/// ResourceClassId43
		/// </summary>
		[JsonProperty(PropertyName = "RCId43")]
		public int? ResourceClassId43 { get; set; }

		/// <summary>
		/// RateDescription44
		/// </summary>
		[JsonProperty(PropertyName = "RD44")]
		public string RateDescription44 { get; set; }

		/// <summary>
		/// ResourceClass44
		/// </summary>
		[JsonIgnore]
		public string ResourceClass44 { get; set; }

		/// <summary>
		/// ResourceClassId44
		/// </summary>
		[JsonProperty(PropertyName = "RCId44")]
		public int? ResourceClassId44 { get; set; }

		/// <summary>
		/// RateDescription45
		/// </summary>
		[JsonProperty(PropertyName = "RD45")]
		public string RateDescription45 { get; set; }

		/// <summary>
		/// ResourceClass45
		/// </summary>
		[JsonIgnore]
		public string ResourceClass45 { get; set; }

		/// <summary>
		/// ResourceClassId45
		/// </summary>
		[JsonProperty(PropertyName = "RCId45")]
		public int? ResourceClassId45 { get; set; }

		/// <summary>
		/// RateDescription51
		/// </summary>
		[JsonProperty(PropertyName = "RD51")]
		public string RateDescription51 { get; set; }

		/// <summary>
		/// ResourceClass51
		/// </summary>
		[JsonIgnore]
		public string ResourceClass51 { get; set; }

		/// <summary>
		/// ResourceClassId51
		/// </summary>
		[JsonProperty(PropertyName = "RCId51")]
		public int? ResourceClassId51 { get; set; }

		/// <summary>
		/// RateDescription52
		/// </summary>
		[JsonProperty(PropertyName = "RD52")]
		public string RateDescription52 { get; set; }

		/// <summary>
		/// ResourceClass52
		/// </summary>
		[JsonIgnore]
		public string ResourceClass52 { get; set; }

		/// <summary>
		/// ResourceClassId52
		/// </summary>
		[JsonProperty(PropertyName = "RCId52")]
		public int? ResourceClassId52 { get; set; }

		/// <summary>
		/// RateDescription53
		/// </summary>
		[JsonProperty(PropertyName = "RD53")]
		public string RateDescription53 { get; set; }

		/// <summary>
		/// ResourceClass53
		/// </summary>
		[JsonIgnore]
		public string ResourceClass53 { get; set; }

		/// <summary>
		/// ResourceClassId53
		/// </summary>
		[JsonProperty(PropertyName = "RCId53")]
		public int? ResourceClassId53 { get; set; }

		/// <summary>
		/// RateDescription54
		/// </summary>
		[JsonProperty(PropertyName = "RD54")]
		public string RateDescription54 { get; set; }

		/// <summary>
		/// ResourceClass54
		/// </summary>
		[JsonIgnore]
		public string ResourceClass54 { get; set; }

		/// <summary>
		/// ResourceClassId54
		/// </summary>
		[JsonProperty(PropertyName = "RCId54")]
		public int? ResourceClassId54 { get; set; }

		/// <summary>
		/// RateDescription55
		/// </summary>
		[JsonProperty(PropertyName = "RD55")]
		public string RateDescription55 { get; set; }

		/// <summary>
		/// ResourceClass55
		/// </summary>
		[JsonIgnore]
		public string ResourceClass55 { get; set; }

		/// <summary>
		/// ResourceClassId55
		/// </summary>
		[JsonProperty(PropertyName = "RCId55")]
		public int? ResourceClassId55 { get; set; }

		/// <summary>
		/// RateDescription61
		/// </summary>
		[JsonProperty(PropertyName = "RD61")]
		public string RateDescription61 { get; set; }

		/// <summary>
		/// ResourceClass61
		/// </summary>
		[JsonIgnore]
		public string ResourceClass61 { get; set; }

		/// <summary>
		/// ResourceClassId61
		/// </summary>
		[JsonProperty(PropertyName = "RCId61")]
		public int? ResourceClassId61 { get; set; }

		/// <summary>
		/// RateDescription62
		/// </summary>
		[JsonProperty(PropertyName = "RD62")]
		public string RateDescription62 { get; set; }

		/// <summary>
		/// ResourceClass62
		/// </summary>
		[JsonIgnore]
		public string ResourceClass62 { get; set; }

		/// <summary>
		/// ResourceClassId62
		/// </summary>
		[JsonProperty(PropertyName = "RCId62")]
		public int? ResourceClassId62 { get; set; }

		/// <summary>
		/// RateDescription63
		/// </summary>
		[JsonProperty(PropertyName = "RD63")]
		public string RateDescription63 { get; set; }

		/// <summary>
		/// ResourceClass63
		/// </summary>
		[JsonIgnore]
		public string ResourceClass63 { get; set; }

		/// <summary>
		/// ResourceClassId63
		/// </summary>
		[JsonProperty(PropertyName = "RCId63")]
		public int? ResourceClassId63 { get; set; }

		/// <summary>
		/// RateDescription64
		/// </summary>
		[JsonProperty(PropertyName = "RD64")]
		public string RateDescription64 { get; set; }

		/// <summary>
		/// ResourceClass64
		/// </summary>
		[JsonIgnore]
		public string ResourceClass64 { get; set; }

		/// <summary>
		/// ResourceClassId64
		/// </summary>
		[JsonProperty(PropertyName = "RCId64")]
		public int? ResourceClassId64 { get; set; }

		/// <summary>
		/// RateDescription65
		/// </summary>
		[JsonProperty(PropertyName = "RD65")]
		public string RateDescription65 { get; set; }

		/// <summary>
		/// ResourceClass65
		/// </summary>
		[JsonIgnore]
		public string ResourceClass65 { get; set; }

		/// <summary>
		/// ResourceClassId65
		/// </summary>
		[JsonProperty(PropertyName = "RCId65")]
		public int? ResourceClassId65 { get; set; }

		/// <summary>
		/// RateDescription71
		/// </summary>
		[JsonProperty(PropertyName = "RD71")]
		public string RateDescription71 { get; set; }

		/// <summary>
		/// ResourceClass71
		/// </summary>
		[JsonIgnore]
		public string ResourceClass71 { get; set; }

		/// <summary>
		/// ResourceClassId71
		/// </summary>
		[JsonProperty(PropertyName = "RCId71")]
		public int? ResourceClassId71 { get; set; }

		/// <summary>
		/// RateDescription72
		/// </summary>
		[JsonProperty(PropertyName = "RD72")]
		public string RateDescription72 { get; set; }

		/// <summary>
		/// ResourceClass72
		/// </summary>
		[JsonIgnore]
		public string ResourceClass72 { get; set; }

		/// <summary>
		/// ResourceClassId72
		/// </summary>
		[JsonProperty(PropertyName = "RCId72")]
		public int? ResourceClassId72 { get; set; }

		/// <summary>
		/// RateDescription73
		/// </summary>
		[JsonProperty(PropertyName = "RD73")]
		public string RateDescription73 { get; set; }

		/// <summary>
		/// ResourceClass73
		/// </summary>
		[JsonIgnore]
		public string ResourceClass73 { get; set; }

		/// <summary>
		/// ResourceClassId73
		/// </summary>
		[JsonProperty(PropertyName = "RCId73")]
		public int? ResourceClassId73 { get; set; }

		/// <summary>
		/// RateDescription74
		/// </summary>
		[JsonProperty(PropertyName = "RD74")]
		public string RateDescription74 { get; set; }

		/// <summary>
		/// ResourceClass74
		/// </summary>
		[JsonIgnore]
		public string ResourceClass74 { get; set; }

		/// <summary>
		/// ResourceClassId74
		/// </summary>
		[JsonProperty(PropertyName = "RCId74")]
		public int? ResourceClassId74 { get; set; }

		/// <summary>
		/// RateDescription75
		/// </summary>
		[JsonProperty(PropertyName = "RD75")]
		public string RateDescription75 { get; set; }

		/// <summary>
		/// ResourceClass75
		/// </summary>
		[JsonIgnore]
		public string ResourceClass75 { get; set; }

		/// <summary>
		/// ResourceClassId75
		/// </summary>
		[JsonProperty(PropertyName = "RCId75")]
		public int? ResourceClassId75 { get; set; }

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
