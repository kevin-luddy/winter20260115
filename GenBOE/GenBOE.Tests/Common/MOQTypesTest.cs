// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.Common
{
    using System;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests MOQ Types
    /// </summary>
    [TestClass]
    public class MOQTypesTest
    {
        /// <summary>
        /// Test MOQ Type Translations, workspace created prior to new MOQ Types Usage Start Date
        /// </summary>
        [TestMethod]
        public void TestTranslation_WsCreatedPriorToNewMoqTypeStart()
        {
            DateTime? wsCreationDate = new DateTime(2020, 10, 1);

            // IS&GS.. we aren't translating -> None
            Assert.AreEqual(MOQType.Comparison, MOQType.Comparison.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.EstimatingRelationships, MOQType.EstimatingRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Factor, MOQType.Factor.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Judgment, MOQType.Judgment.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.LevelOfEffort, MOQType.LevelOfEffort.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.None, MOQType.None.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Probability, MOQType.Probability.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Standard, MOQType.Standard.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Unit, MOQType.Unit.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.VendorQuote, MOQType.VendorQuote.MapToNew(wsCreationDate));

            // RMS Types
            Assert.AreEqual(MOQType.MSTComparisonAnalogyMethod, MOQType.MSTComparisonAnalogyMethod.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTCostEstimatingRelationships, MOQType.MSTCostEstimatingRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTEngineeringJudgmentalEstimates, MOQType.MSTEngineeringJudgmentalEstimates.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTFactorUnitMethod, MOQType.MSTFactorUnitMethod.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTHistoricalPerformance, MOQType.MSTHistoricalPerformance.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTLevelOfEffortSupport, MOQType.MSTLevelOfEffortSupport.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTParametricCostModels, MOQType.MSTParametricCostModels.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTStandardTimeEstimating, MOQType.MSTStandardTimeEstimating.MapToNew(wsCreationDate));

            // SSC Types
            Assert.AreEqual(MOQType.SSCActual, MOQType.SSCActual.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCAnalogySimilarTo, MOQType.SSCAnalogySimilarTo.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCBottomUp, MOQType.SSCBottomUp.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCCostEstimatingRelationships, MOQType.SSCCostEstimatingRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCDataDrivenCostModelsEquations, MOQType.SSCDataDrivenCostModelsEquations.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCHistoricalExperienceFactor, MOQType.SSCHistoricalExperienceFactor.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCLaborStandardsAndRealizationPerformanceFactors, MOQType.SSCLaborStandardsAndRealizationPerformanceFactors.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCLevelOfEffortSupport, MOQType.SSCLevelOfEffortSupport.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCQuote, MOQType.SSCQuote.MapToNew(wsCreationDate));

            // New Types
            Assert.AreEqual(MOQType.AnalogousRelationships, MOQType.AnalogousRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Comparative, MOQType.Comparative.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.CostEstimatingRelationships, MOQType.CostEstimatingRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Historical, MOQType.Historical.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.LOE, MOQType.LOE.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.NonLabor, MOQType.NonLabor.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.ParametricEstimates.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SME, MOQType.SME.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SOW, MOQType.SOW.MapToNew(wsCreationDate));
        }

        /// <summary>
        /// Test MOQ Type Translations, workspace created date being null (i.e. considerably older)
        /// </summary>
        [TestMethod]
        public void TestTranslation_WsCreatedNullDate()
        {
            DateTime? wsCreationDate = null;

            // IS&GS.. we aren't translating -> None
            Assert.AreEqual(MOQType.Comparison, MOQType.Comparison.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.EstimatingRelationships, MOQType.EstimatingRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Factor, MOQType.Factor.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Judgment, MOQType.Judgment.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.LevelOfEffort, MOQType.LevelOfEffort.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.None, MOQType.None.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Probability, MOQType.Probability.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Standard, MOQType.Standard.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Unit, MOQType.Unit.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.VendorQuote, MOQType.VendorQuote.MapToNew(wsCreationDate));

            // RMS Types
            Assert.AreEqual(MOQType.MSTComparisonAnalogyMethod, MOQType.MSTComparisonAnalogyMethod.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTCostEstimatingRelationships, MOQType.MSTCostEstimatingRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTEngineeringJudgmentalEstimates, MOQType.MSTEngineeringJudgmentalEstimates.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTFactorUnitMethod, MOQType.MSTFactorUnitMethod.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTHistoricalPerformance, MOQType.MSTHistoricalPerformance.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTLevelOfEffortSupport, MOQType.MSTLevelOfEffortSupport.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTParametricCostModels, MOQType.MSTParametricCostModels.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.MSTStandardTimeEstimating, MOQType.MSTStandardTimeEstimating.MapToNew(wsCreationDate));

            // SSC Types
            Assert.AreEqual(MOQType.SSCActual, MOQType.SSCActual.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCAnalogySimilarTo, MOQType.SSCAnalogySimilarTo.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCBottomUp, MOQType.SSCBottomUp.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCCostEstimatingRelationships, MOQType.SSCCostEstimatingRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCDataDrivenCostModelsEquations, MOQType.SSCDataDrivenCostModelsEquations.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCHistoricalExperienceFactor, MOQType.SSCHistoricalExperienceFactor.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCLaborStandardsAndRealizationPerformanceFactors, MOQType.SSCLaborStandardsAndRealizationPerformanceFactors.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCLevelOfEffortSupport, MOQType.SSCLevelOfEffortSupport.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SSCQuote, MOQType.SSCQuote.MapToNew(wsCreationDate));

            // New Types
            Assert.AreEqual(MOQType.AnalogousRelationships, MOQType.AnalogousRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Comparative, MOQType.Comparative.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.CostEstimatingRelationships, MOQType.CostEstimatingRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Historical, MOQType.Historical.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.LOE, MOQType.LOE.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.NonLabor, MOQType.NonLabor.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.ParametricEstimates.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SME, MOQType.SME.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SOW, MOQType.SOW.MapToNew(wsCreationDate));
        }

        /// <summary>
        /// Test MOQ Type Translations
        /// </summary>
        [TestMethod]
        public void TestTranslation_WsCreatedAfterNewMoqTypeStart()
        {
            DateTime? wsCreationDate = new DateTime(2021, 10, 1);

            // IS&GS.. we aren't translating -> None
            Assert.AreEqual(MOQType.None, MOQType.Comparison.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.None, MOQType.EstimatingRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.None, MOQType.Factor.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.None, MOQType.Judgment.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.None, MOQType.LevelOfEffort.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.None, MOQType.None.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.None, MOQType.Probability.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.None, MOQType.Standard.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.None, MOQType.Unit.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.None, MOQType.VendorQuote.MapToNew(wsCreationDate));

            // RMS Types
            Assert.AreEqual(MOQType.Comparative, MOQType.MSTComparisonAnalogyMethod.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.CostEstimatingRelationships, MOQType.MSTCostEstimatingRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SME, MOQType.MSTEngineeringJudgmentalEstimates.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.MSTFactorUnitMethod.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Historical, MOQType.MSTHistoricalPerformance.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.LOE, MOQType.MSTLevelOfEffortSupport.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.MSTParametricCostModels.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.MSTStandardTimeEstimating.MapToNew(wsCreationDate));

            // SSC Types
            Assert.AreEqual(MOQType.Historical, MOQType.SSCActual.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Historical, MOQType.SSCAnalogySimilarTo.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SME, MOQType.SSCBottomUp.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.CostEstimatingRelationships, MOQType.SSCCostEstimatingRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.SSCDataDrivenCostModelsEquations.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.AnalogousRelationships, MOQType.SSCHistoricalExperienceFactor.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.SSCLaborStandardsAndRealizationPerformanceFactors.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.LOE, MOQType.SSCLevelOfEffortSupport.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.NonLabor, MOQType.SSCQuote.MapToNew(wsCreationDate));

            // New Types
            Assert.AreEqual(MOQType.AnalogousRelationships, MOQType.AnalogousRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Comparative, MOQType.Comparative.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.CostEstimatingRelationships, MOQType.CostEstimatingRelationships.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.Historical, MOQType.Historical.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.LOE, MOQType.LOE.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.NonLabor, MOQType.NonLabor.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.ParametricEstimates.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SME, MOQType.SME.MapToNew(wsCreationDate));
            Assert.AreEqual(MOQType.SOW, MOQType.SOW.MapToNew(wsCreationDate));
        }
    }
}
