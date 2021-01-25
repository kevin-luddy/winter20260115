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
        /// URL Suffix for Table Name for Historical
        /// </summary>
        public string TableNameHistoricalSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Table Name for Comparative
        /// </summary>
        public string TableNameComparativeSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Repository Name for Historical
        /// </summary>
        public string RepositoryNameHistoricalSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Repository Name for Comparative
        /// </summary>
        public string RepositoryNameComparativeSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Query Type for Historical
        /// </summary>
        public string QueryTypeHistoricalSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Query Type for Comparative
        /// </summary>
        public string QueryTypeComparativeSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Date of Report for Historical
        /// </summary>
        public string DateOfReportHistoricalSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Date of Report for Comparative
        /// </summary>
        public string DateOfReportComparativeSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Historical Program Name for Historical
        /// </summary>
        public string HistoricalProgramNameHistoricalSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Historical Program Name for Comparative
        /// </summary>
        public string HistoricalProgramNameComparativeSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Contract Number
        /// </summary>
        public string ContractNumberSuffix { get; set; }

        /// <summary>
        /// URL Suffix for WBS/WBS Element for Historical
        /// </summary>
        public string WBSElementHistoricalSuffix { get; set; }

        /// <summary>
        /// URL Suffix for WBS/WBS Element for Comparative
        /// </summary>
        public string WBSElementComparativeSuffix { get; set; }

        /// <summary>
        /// URL Suffix for PoP Start Date for Historical
        /// </summary>
        public string PoPStartHistoricalSuffix { get; set; }

        /// <summary>
        /// URL Suffix for PoP Start Date for Comparative
        /// </summary>
        public string PoPStartComparativeSuffix { get; set; }

        /// <summary>
        /// URL Suffix for PoP End Date for Historical
        /// </summary>
        public string PoPEndHistoricalSuffix { get; set; }

        /// <summary>
        /// URL Suffix for PoP End Date for Comparative
        /// </summary>
        public string PoPEndComparativeSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Total WBS hours for Historical
        /// </summary>
        public string TotalWBSHoursSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Additional Query Filters for Historical
        /// </summary>
        public string AdditionalQueryFiltersHistoricalSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Additional Query Filters for Comparative
        /// </summary>
        public string AdditionalQueryFiltersComparativeSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Total Relevant Hours for Historical
        /// </summary>
        public string TotalRelevantHoursHistoricalSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Total Relevant Hours for Comparative
        /// </summary>
        public string TotalRelevantHoursComparativeSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Historical Rationale
        /// </summary>
        public string HistoricalRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Comparative Rationale
        /// </summary>
        public string ComparativeRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for CER Rationale
        /// </summary>
        public string CerRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for PE Rationale
        /// </summary>
        public string PeRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for AR Rationale
        /// </summary>
        public string ArRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for SOW Rationale
        /// </summary>
        public string SowRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for LOE Rationale
        /// </summary>
        public string LoeRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Non-Labor Rationale
        /// </summary>
        public string NonLaborRationaleSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Historical Skill Mix Rationale
        /// </summary>
        public string HistoricalSkillMixSuffix { get; set; }

        /// <summary>
        /// URL Suffix for Comparative Skill Mix Rationale
        /// </summary>
        public string ComparativeSkillMixSuffix { get; set; }

        /// <summary>
        /// URL Suffix for CER Skill Mix Rationale
        /// </summary>
        public string CerSkillMixSuffix { get; set; }

        /// <summary>
        /// URL Suffix for PE Skill Mix Rationale
        /// </summary>
        public string PeSkillMixSuffix { get; set; }

        /// <summary>
        /// URL Suffix for AR Skill Mix Rationale
        /// </summary>
        public string ArSkillMixSuffix { get; set; }

        /// <summary>
        /// URL Suffix for SOW Skill Mix Rationale
        /// </summary>
        public string SowSkillMixSuffix { get; set; }

        /// <summary>
        /// URL Suffix for LOE Skill Mix Rationale
        /// </summary>
        public string LoeSkillMixSuffix { get; set; }

        /// <summary>
        /// URL Suffix for SME Skill Mix Rationale
        /// </summary>
        public string SmeSkillMixSuffix { get; set; }

        /// <summary>
        /// URL Suffix for CER Name
        /// </summary>
        public string CERNameSuffix { get; set; }
        
        /// <summary>
        /// URL Suffix for PE Name
        /// </summary>
        public string PENameSuffix { get; set; }
        
        /// <summary>
        /// URL Suffix for AR Name
        /// </summary>
        public string ARNameSuffix { get; set; }
        
        /// <summary>
        /// URL Suffix for SOW/LOE Description
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
