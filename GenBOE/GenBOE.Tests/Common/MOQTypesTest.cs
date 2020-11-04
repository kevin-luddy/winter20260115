// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.Common
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using IES.Common;

    /// <summary>
    /// Tests MOQ Types
    /// </summary>
    [TestClass]
    public class MOQTypesTest
    {
        /// <summary>
        /// Test MOQ Type Translations
        /// </summary>
        [TestMethod]
        public void TestTranslation()
        {
            // IS&GS.. we aren't translating -> None
            Assert.AreEqual(MOQType.None, MOQType.Comparison.MapToNew());
            Assert.AreEqual(MOQType.None, MOQType.EstimatingRelationships.MapToNew());
            Assert.AreEqual(MOQType.None, MOQType.Factor.MapToNew());
            Assert.AreEqual(MOQType.None, MOQType.Judgment.MapToNew());
            Assert.AreEqual(MOQType.None, MOQType.LevelOfEffort.MapToNew());
            Assert.AreEqual(MOQType.None, MOQType.None.MapToNew());
            Assert.AreEqual(MOQType.None, MOQType.Probability.MapToNew());
            Assert.AreEqual(MOQType.None, MOQType.Standard.MapToNew());
            Assert.AreEqual(MOQType.None, MOQType.Unit.MapToNew());
            Assert.AreEqual(MOQType.None, MOQType.VendorQuote.MapToNew());

            // RMS Types
            Assert.AreEqual(MOQType.Comparative, MOQType.MSTComparisonAnalogyMethod.MapToNew());
            Assert.AreEqual(MOQType.CostEstimatingRelationships, MOQType.MSTCostEstimatingRelationships.MapToNew());
            Assert.AreEqual(MOQType.SME, MOQType.MSTEngineeringJudgmentalEstimates.MapToNew());
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.MSTFactorUnitMethod.MapToNew());
            Assert.AreEqual(MOQType.Historical, MOQType.MSTHistoricalPerformance.MapToNew());
            Assert.AreEqual(MOQType.LOE, MOQType.MSTLevelOfEffortSupport.MapToNew());
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.MSTParametricCostModels.MapToNew());
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.MSTStandardTimeEstimating.MapToNew());

            // SSC Types
            Assert.AreEqual(MOQType.Historical, MOQType.SSCActual.MapToNew());
            Assert.AreEqual(MOQType.AnalogousRelationships, MOQType.SSCAnalogySimilarTo.MapToNew());
            Assert.AreEqual(MOQType.SME, MOQType.SSCBottomUp.MapToNew());
            Assert.AreEqual(MOQType.CostEstimatingRelationships, MOQType.SSCCostEstimatingRelationships.MapToNew());
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.SSCDataDrivenCostModelsEquations.MapToNew());
            Assert.AreEqual(MOQType.CostEstimatingRelationships, MOQType.SSCHistoricalExperienceFactor.MapToNew());
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.SSCLaborStandardsAndRealizationPerformanceFactors.MapToNew());
            Assert.AreEqual(MOQType.LOE, MOQType.SSCLevelOfEffortSupport.MapToNew());
            Assert.AreEqual(MOQType.NonLabor, MOQType.SSCQuote.MapToNew());

            // New Types
            Assert.AreEqual(MOQType.AnalogousRelationships, MOQType.AnalogousRelationships.MapToNew());
            Assert.AreEqual(MOQType.Comparative, MOQType.Comparative.MapToNew());
            Assert.AreEqual(MOQType.CostEstimatingRelationships, MOQType.CostEstimatingRelationships.MapToNew());
            Assert.AreEqual(MOQType.Historical, MOQType.Historical.MapToNew());
            Assert.AreEqual(MOQType.LOE, MOQType.LOE.MapToNew());
            Assert.AreEqual(MOQType.NonLabor, MOQType.NonLabor.MapToNew());
            Assert.AreEqual(MOQType.ParametricEstimates, MOQType.ParametricEstimates.MapToNew());
            Assert.AreEqual(MOQType.SME, MOQType.SME.MapToNew());
            Assert.AreEqual(MOQType.SOW, MOQType.SOW.MapToNew());
        }
    }
}
