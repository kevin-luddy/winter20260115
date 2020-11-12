// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using IES.Common;

    /// <summary>
    /// Class for MOQ Help URLs
    /// </summary>
    public class MoqTypeHelpUrls
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MoqTypeHelpUrls()
        {
            this.BaseUrl = ConfigurationUtilities.GetAppSetting("MOQHelpBaseUrl");
        }

        /// <summary>
        /// Base URL from web.config
        /// </summary>
        public string BaseUrl { get; set; }
        
        /// <summary>
        /// URL Suffix for Table Name
        /// </summary>
        public string TableNameSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Repository Name
        /// </summary>
        public string RepositoryNameSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Query Type
        /// </summary>
        public string QueryTypeSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Date of Report
        /// </summary>
        public string DateOfReportSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Historical Program Name
        /// </summary>
        public string HistoricalProgramNameSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Contract Number
        /// </summary>
        public string ContractNumberSuffix { get; set; }

        /// <summary>
        /// URL Suffix for WBS/WBS Element
        /// </summary>
        public string WBSElementSuffix { get; set; }

        /// <summary>
        /// URL Suffix for PoP Start Date
        /// </summary>
        public string PoPStartSuffix { get; set; }

        /// <summary>
        /// URL Suffix for PoP End Date
        /// </summary>
        public string PoPEndSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Total WBS hours
        /// </summary>
        public string TotalWBSHoursSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Additional Query Filters
        /// </summary>
        public string AdditionalQueryFiltersSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Total Relevant Hours
        /// </summary>
        public string TotalRelevantHoursSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Historical Rationale
        /// </summary>
        public string HistoricalRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Comparative Rationale
        /// </summary>
        public string ComparativeRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for CER/PE/AR Rationale
        /// </summary>
        public string CerPeArRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for SOW/LOE Rationale
        /// </summary>
        public string SowLoeRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Non-Labor Rationale
        /// </summary>
        public string NonLaborRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Skill Mix Rationale
        /// </summary>
        public string SkillMixSuffix { get; set; }

        /// <summary>
        /// URL Suffix for CER Name
        /// </summary>
        public string CERNameSuffix { get; set; }

        /// <summary>
        /// URL Suffix for CER Location
        /// </summary>
        public string CERLocationSuffix { get; set; }

        /// <summary>
        /// URL Suffix for PE Name
        /// </summary>
        public string PENameSuffix { get; set; }

        /// <summary>
        /// URL Suffix for PE Location
        /// </summary>
        public string PELocationSuffix { get; set; }

        /// <summary>
        /// URL Suffix for AR Name
        /// </summary>
        public string ARNameSuffix { get; set; }

        /// <summary>
        /// URL Suffix for AR Location
        /// </summary>
        public string ARLocationSuffix { get; set; }

        /// <summary>
        /// URL Suffix for SOW Description
        /// </summary>
        public string SOWDescriptionSuffix { get; set; }

        /// <summary>
        /// URL Suffix for LOE Description
        /// </summary>
        public string LOEDescriptionSuffix { get; set; }

        /// <summary>
        /// URL Suffix for SME Reasons
        /// </summary>
        public string SMEReasonsSuffix { get; set; }

        /// <summary>
        /// URL Suffix for SME Hours Logic
        /// </summary>
        public string SMEHoursLogicSuffix { get; set; }

        /// <summary>
        /// URL Suffix for SME Duration Logic
        /// </summary>
        public string SMEDurationLogicSuffix { get; set; }

        /// <summary>
        /// URL Suffix for SME Tasks
        /// </summary>
        public string SMETasksSuffix { get; set; }
    }
}
