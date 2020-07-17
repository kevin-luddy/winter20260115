// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using DataBridge.Loaders;
    using DataBridge.ModelViews;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Class for RateConfig Loader
    /// </summary>
    [TestClass]
    public class RateConfigTest
    {
        /// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Test retrieve
        /// </summary>
        [TestMethod]
        public void TestRateConfigLoader()
        {
            RateConfigLoader sut = this.testData.RateConfigLoader;

            ICollection<RateConfigModelView> results = sut.GetAll();
            ICollection<RateConfigModelView> rateConfigs = results.Where(x => x.RateTarget.Equals(RateTarget.Rate.GetDescription())).ToList();
            ICollection<RateConfigModelView> pprdConfigs = results.Where(x => x.RateTarget.Equals(RateTarget.PPRD.GetDescription())).ToList();
            Assert.IsTrue(rateConfigs.Any());
            Assert.IsTrue(pprdConfigs.Any());

            // verify default entries for Rate and PPR&D
            Assert.IsTrue(rateConfigs.Any(x => x.Precision == 6 && !x.RateCategory.HasValue && string.IsNullOrEmpty(x.Prefix) && string.IsNullOrEmpty(x.Suffix) && !x.Multiplier.HasValue));
            Assert.IsTrue(pprdConfigs.Any(x => x.Precision == 6 && !x.RateCategory.HasValue && string.IsNullOrEmpty(x.Prefix) && string.IsNullOrEmpty(x.Suffix) && !x.Multiplier.HasValue));
        }
    }
}
