// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// The Categories of Quoting Methods in the system. Each enum maps to a value in the MOQTypeLU table.
    /// </summary>
    public enum MOQType
    {
        /*
         * IS&GS-only - LEGACY ONLY
         */
        [Description("")]
        None = 0,
        [Description("Standard")]
        Standard = 1,
        [Description("Estimating Relationships")]
        EstimatingRelationships = 2,
        [Description("Probability")]
        Probability = 3,
        [Description("Factor")]
        Factor = 4,
        [Description("Unit")]
        Unit = 5,
        [Description("Comparison")]
        Comparison = 6,
        [Description("Judgment")]
        Judgment = 7,
        [Description("Level of Effort")]
        LevelOfEffort = 8,
        [Description("Vendor Quote")]
        VendorQuote = 9,
        /*
         * SSC-only - Moving away from these for new workspaces starting in 2020.6
         */
        [Description("Analogy/similar to")]
        SSCAnalogySimilarTo = 1001,
        [Description("Bottom-up")]
        SSCBottomUp = 1002,
        [Description("Cost estimating relationships (CERs)")]
        SSCCostEstimatingRelationships = 1003,
        [Description("Historical experience factor (HEF)")]
        SSCHistoricalExperienceFactor = 1004,
        [Description("Labor standards and realization/performance factors")]
        SSCLaborStandardsAndRealizationPerformanceFactors = 1005,
        [Description("Level-of-effort (LOE)/support")]
        SSCLevelOfEffortSupport = 1006,
        [Description("Data-driven cost models/equations")]
        SSCDataDrivenCostModelsEquations = 1007,
        [Description("Actual")]
        SSCActual = 1008,
        [Description("Quote")]
        SSCQuote = 1009,
        /*
         * MST-only - Moving away from these for new workspaces starting in 2020.6
         */
        [Description("Historical Performance")]
        MSTHistoricalPerformance = 2001,
        [Description("Comparison/Analogy Method")]
        MSTComparisonAnalogyMethod = 2002,
        [Description("Cost Estimating Relationships (CERs)/Historical Factors")]
        MSTCostEstimatingRelationships = 2003,
        [Description("Parametric Cost Models")]
        MSTParametricCostModels = 2004,
        [Description("Standard Time Estimating")]
        MSTStandardTimeEstimating = 2005,
        [Description("Factor/Unit Method")]
        MSTFactorUnitMethod = 2006,
        [Description("Level-of-Effort/Support")]
        MSTLevelOfEffortSupport = 2007,
        [Description("Engineering/Judgmental Estimates")]
        MSTEngineeringJudgmentalEstimates = 2008,
        /*
         * New MOQ Types as of 2020.6
         */
        [Description("Actual Program or Task Cost Data (Historical)")]
        Historical = 5001,
        [Description("Comparative Analysis")]
        Comparative = 5002,
        [Description("Cost Estimating Relationships (CERs) R2")]
        CostEstimatingRelationships = 5003,
        [Description("Parametric Estimates")]
        ParametricEstimates = 5004,
        [Description("Analogous Relationships (ARs)")]
        AnalogousRelationships = 5005,
        [Description("Statement of Work (SOW)")]
        SOW = 5006,
        [Description("Level of Effort (LOE)")]
        LOE = 5007,
        [Description("Subject Matter Expert (SME) Judgment")]
        SME = 5008,
        [Description("Non-Labor")]
        NonLabor = 5009
    }

    /// <summary>
    /// An extension class for the MOQ Types enum
    /// </summary>
    public static class MoqExtensionMethods
    {
        /// <summary>
        /// Starting date for MOQ Templates. WS created after this date will be using new MOQ Types.
        /// </summary>
        private static readonly DateTime moqTemplateUsageStartDate = DateTime.Parse(ConfigurationUtilities.GetAppSetting("MoqTemplateStartDate"));

        /// <summary>
        /// Translates old MOQ Types to new ones, for MOQ Type Changes as of 2020.6
        /// </summary>
        /// <param name="originalValue">Original Value</param>
        /// <param name="wsCreationDate">WS Creation Date</param>
        /// <param name="defaultValue">Default value to set the MOQ Type to</param>
        /// <returns>New Value</returns>
        public static MOQType MapToNew(this MOQType originalValue, DateTime? wsCreationDate, MOQType defaultValue = MOQType.None)
        {
            MOQType result = originalValue;

            if (wsCreationDate >= moqTemplateUsageStartDate)
            {
                switch (originalValue)
                {
                    case MOQType.Historical:
                    case MOQType.MSTHistoricalPerformance:
                    case MOQType.SSCActual:
                    case MOQType.SSCAnalogySimilarTo:
                        result = MOQType.Historical;
                        break;
                    case MOQType.Comparative:
                    case MOQType.MSTComparisonAnalogyMethod:
                        result = MOQType.Comparative;
                        break;
                    case MOQType.CostEstimatingRelationships:
                    case MOQType.MSTCostEstimatingRelationships:
                    case MOQType.SSCCostEstimatingRelationships:
                        result = MOQType.CostEstimatingRelationships;
                        break;
                    case MOQType.ParametricEstimates:
                    case MOQType.MSTParametricCostModels:
                    case MOQType.MSTStandardTimeEstimating:
                    case MOQType.MSTFactorUnitMethod:
                    case MOQType.SSCDataDrivenCostModelsEquations:
                    case MOQType.SSCLaborStandardsAndRealizationPerformanceFactors:
                        result = MOQType.ParametricEstimates;
                        break;
                    case MOQType.AnalogousRelationships:
                    case MOQType.SSCHistoricalExperienceFactor:
                        result = MOQType.AnalogousRelationships;
                        break;
                    case MOQType.SOW:
                        result = MOQType.SOW;
                        break;
                    case MOQType.LOE:
                    case MOQType.MSTLevelOfEffortSupport:
                    case MOQType.SSCLevelOfEffortSupport:
                        result = MOQType.LOE;
                        break;
                    case MOQType.SME:
                    case MOQType.MSTEngineeringJudgmentalEstimates:
                    case MOQType.SSCBottomUp:
                        result = MOQType.SME;
                        break;
                    case MOQType.NonLabor:
                    case MOQType.SSCQuote:
                        result = MOQType.NonLabor;
                        break;
                    default:
                        result = defaultValue;
                        break;
                }
            }

            return result;
        }
    }
}