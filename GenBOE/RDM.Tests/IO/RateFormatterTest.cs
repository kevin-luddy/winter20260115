// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Tests.IO
{
    using System;
    using System.Collections.Generic;
    using IES.ActionLogic.Common;
    using IES.Common;
    using IES.Common.classes;
    using IES.DataBridge.Loaders;
    using IES.Tests;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Rate Formatter tests.
    /// </summary>
    [TestClass]
    public class RateFormatterTest
    {
        /// <summary>
        /// Mock Rate Config Loader
        /// </summary>
        private Mock<IRateConfigLoader> rateConfigLoader = new Mock<IRateConfigLoader>();

        /// <summary>
        /// Initializes the test data.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            rateConfigLoader = new Mock<IRateConfigLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRateConfigLoader), rateConfigLoader.Object);
            this.rateConfigLoader.Setup(x => x.GetAll()).Returns(TestData.GetRateConfigTestData());
        }

        /// <summary>
        /// Test rate formatting for various rate categories when value is null.
        /// </summary>
        [TestMethod]
        public void TestFormatRateForExport_NullValue()
        {
            decimal? value = null;
            Dictionary<RateCategory, string[]> expectedResults = new Dictionary<RateCategory, string[]>()
            {
                { RateCategory.DirectLabor, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.Fccom, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.Fringe, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.GA, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.LaborEscalationFactor, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.LaborEscalationPercentage, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.NonLaborEscalationFactor, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.NonLaborEscalationPercentage, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.Overhead, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.SMConv, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.ServiceCenter, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.TravelFee, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.TravelMlge, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.TravelOtc, new[] { Constants.NOT_APPLICABLE, string.Empty } },
                { RateCategory.TravelRc, new[] { Constants.NOT_APPLICABLE, string.Empty } }
            };

            foreach (KeyValuePair<RateCategory, string[]> entry in expectedResults)
            {
                string actualPprd = RateFormatter.FormatRate(RateTarget.PPRD, entry.Key.GetDescription(), value);
                string actualRate = RateFormatter.FormatRate(RateTarget.Rate, entry.Key.GetDescription(), value);
                Assert.AreEqual(entry.Value[(int)RateTarget.PPRD], actualPprd);
                Assert.AreEqual(entry.Value[(int)RateTarget.Rate], actualRate);
            }
        }
        
        /// <summary>
        /// Test rate formatting for various rate categories for value with extra precision.
        /// </summary>
        [TestMethod]
        public void TestFormatRateForExport_ExtraPrecision()
        {
            decimal? value = 0.123456789m;   // rate value with extra precision.
            Dictionary<RateCategory, string[]> expectedResults = new Dictionary<RateCategory, string[]>()
            {
                { RateCategory.DirectLabor, new[] { "$0.12", "$0.12" } },
                { RateCategory.Fccom, new[] { "12.3457%", "0.123457" } },
                { RateCategory.Fringe, new[] { "12.3457%", "0.123457" } },
                { RateCategory.GA, new[] { "12.3457%", "0.123457" } },
                { RateCategory.LaborEscalationFactor, new[] { "0.1235", "0.1235" } },
                { RateCategory.LaborEscalationPercentage, new[] { "12.35%", "0.1235" } },
                { RateCategory.NonLaborEscalationFactor, new[] { "0.1235", "0.1235" } },
                { RateCategory.NonLaborEscalationPercentage, new[] { "12.35%", "0.1235" } },
                { RateCategory.Overhead, new[] { "12.3457%", "0.123457" } },
                { RateCategory.SMConv, new[] { "0", "0" } },
                { RateCategory.ServiceCenter, new[] { "$0.12", "$0.12" } },
                { RateCategory.TravelFee, new[] { "$0.12", "$0.12" } },
                { RateCategory.TravelMlge, new[] { "$0.123", "$0.123" } },
                { RateCategory.TravelOtc, new[] { "$0.12", "$0.12" } },
                { RateCategory.TravelRc, new[] { "$0.12", "$0.12" } }
            };

            foreach (KeyValuePair<RateCategory, string[]> entry in expectedResults)
            {
                string actualPprd = RateFormatter.FormatRate(RateTarget.PPRD, entry.Key.GetDescription(), value);
                string actualRate = RateFormatter.FormatRate(RateTarget.Rate, entry.Key.GetDescription(), value);
                Assert.AreEqual(entry.Value[(int)RateTarget.PPRD], actualPprd);
                Assert.AreEqual(entry.Value[(int)RateTarget.Rate], actualRate);
            }
        }

        /// <summary>
        /// Test rate formatting for various rate categories when value has extra zeros after decimal part.
        /// </summary>
        [TestMethod]
        public void TestFormatRateForExport_TrailingZeros()
        {
            decimal? value = 0.12000000000m;   // rate value with extra zeros after decimal part
            Dictionary<RateCategory, string[]> expectedResults = new Dictionary<RateCategory, string[]>()
            {
                { RateCategory.DirectLabor, new[] { "$0.12", "$0.12" } },
                { RateCategory.Fccom, new[] { "12%", "0.12" } },
                { RateCategory.Fringe, new[] { "12%", "0.12" } },
                { RateCategory.GA, new[] { "12%", "0.12" } },
                { RateCategory.LaborEscalationFactor, new[] { "0.12", "0.12" } },
                { RateCategory.LaborEscalationPercentage, new[] { "12%", "0.12" } },
                { RateCategory.NonLaborEscalationFactor, new[] { "0.12", "0.12" } },
                { RateCategory.NonLaborEscalationPercentage, new[] { "12%", "0.12" } },
                { RateCategory.Overhead, new[] { "12%", "0.12" } },
                { RateCategory.SMConv, new[] { "0", "0" } },
                { RateCategory.ServiceCenter, new[] { "$0.12", "$0.12" } },
                { RateCategory.TravelFee, new[] { "$0.12", "$0.12" } },
                { RateCategory.TravelMlge, new[] { "$0.12", "$0.12" } },
                { RateCategory.TravelOtc, new[] { "$0.12", "$0.12" } },
                { RateCategory.TravelRc, new[] { "$0.12", "$0.12" } }
            };

            foreach (KeyValuePair<RateCategory, string[]> entry in expectedResults)
            {
                string actualPprd = RateFormatter.FormatRate(RateTarget.PPRD, entry.Key.GetDescription(), value);
                string actualRate = RateFormatter.FormatRate(RateTarget.Rate, entry.Key.GetDescription(), value);
                Assert.AreEqual(entry.Value[(int)RateTarget.PPRD], actualPprd);
                Assert.AreEqual(entry.Value[(int)RateTarget.Rate], actualRate);
            }

            value = 0.00120000000m;   // rate value with extra zeros after decimal part
            expectedResults = new Dictionary<RateCategory, string[]>()
            {
                { RateCategory.DirectLabor, new[] { "$0.00", "$0.00" } },
                { RateCategory.Fccom, new[] { "0.12%", "0.0012" } },
                { RateCategory.Fringe, new[] { "0.12%", "0.0012" } },
                { RateCategory.GA, new[] { "0.12%", "0.0012" } },
                { RateCategory.LaborEscalationFactor, new[] { "0.0012", "0.0012" } },
                { RateCategory.LaborEscalationPercentage, new[] { "0.12%", "0.0012" } },
                { RateCategory.NonLaborEscalationFactor, new[] { "0.0012", "0.0012" } },
                { RateCategory.NonLaborEscalationPercentage, new[] { "0.12%", "0.0012" } },
                { RateCategory.Overhead, new[] { "0.12%", "0.0012" } },
                { RateCategory.SMConv, new[] { "0", "0" } },
                { RateCategory.ServiceCenter, new[] { "$0.00", "$0.00" } },
                { RateCategory.TravelFee, new[] { "$0.00", "$0.00" } },
                { RateCategory.TravelMlge, new[] { "$0.001", "$0.001" } },
                { RateCategory.TravelOtc, new[] { "$0.00", "$0.00" } },
                { RateCategory.TravelRc, new[] { "$0.00", "$0.00" } }
            };

            foreach (KeyValuePair<RateCategory, string[]> entry in expectedResults)
            {
                string actualPprd = RateFormatter.FormatRate(RateTarget.PPRD, entry.Key.GetDescription(), value);
                string actualRate = RateFormatter.FormatRate(RateTarget.Rate, entry.Key.GetDescription(), value);
                Assert.AreEqual(entry.Value[(int)RateTarget.PPRD], actualPprd);
                Assert.AreEqual(entry.Value[(int)RateTarget.Rate], actualRate);
            }

            value = 0.10000000000m;   // rate value with extra zeros after decimal part
            expectedResults = new Dictionary<RateCategory, string[]>()
            {
                { RateCategory.DirectLabor, new[] { "$0.10", "$0.10" } },
                { RateCategory.Fccom, new[] { "10%", "0.1" } },
                { RateCategory.Fringe, new[] { "10%", "0.1" } },
                { RateCategory.GA, new[] { "10%", "0.1" } },
                { RateCategory.LaborEscalationFactor, new[] { "0.1", "0.1" } },
                { RateCategory.LaborEscalationPercentage, new[] { "10%", "0.1" } },
                { RateCategory.NonLaborEscalationFactor, new[] { "0.1", "0.1" } },
                { RateCategory.NonLaborEscalationPercentage, new[] { "10%", "0.1" } },
                { RateCategory.Overhead, new[] { "10%", "0.1" } },
                { RateCategory.SMConv, new[] { "0", "0" } },
                { RateCategory.ServiceCenter, new[] { "$0.10", "$0.10" } },
                { RateCategory.TravelFee, new[] { "$0.10", "$0.10" } },
                { RateCategory.TravelMlge, new[] { "$0.1", "$0.1" } },
                { RateCategory.TravelOtc, new[] { "$0.10", "$0.10" } },
                { RateCategory.TravelRc, new[] { "$0.10", "$0.10" } }
            };

            foreach (KeyValuePair<RateCategory, string[]> entry in expectedResults)
            {
                string actualPprd = RateFormatter.FormatRate(RateTarget.PPRD, entry.Key.GetDescription(), value);
                string actualRate = RateFormatter.FormatRate(RateTarget.Rate, entry.Key.GetDescription(), value);
                Assert.AreEqual(entry.Value[(int)RateTarget.PPRD], actualPprd);
                Assert.AreEqual(entry.Value[(int)RateTarget.Rate], actualRate);
            }

            value = 99.90000000000m;   // rate value with extra zeros after decimal part
            expectedResults = new Dictionary<RateCategory, string[]>()
            {
                { RateCategory.DirectLabor, new[] { "$99.90", "$99.90" } },
                { RateCategory.Fccom, new[] { "9990%", "99.9" } },
                { RateCategory.Fringe, new[] { "9990%", "99.9" } },
                { RateCategory.GA, new[] { "9990%", "99.9" } },
                { RateCategory.LaborEscalationFactor, new[] { "99.9", "99.9" } },
                { RateCategory.LaborEscalationPercentage, new[] { "9990%", "99.9" } },
                { RateCategory.NonLaborEscalationFactor, new[] { "99.9", "99.9" } },
                { RateCategory.NonLaborEscalationPercentage, new[] { "9990%", "99.9" } },
                { RateCategory.Overhead, new[] { "9990%", "99.9" } },
                { RateCategory.SMConv, new[] { "100", "100" } },
                { RateCategory.ServiceCenter, new[] { "$99.90", "$99.90" } },
                { RateCategory.TravelFee, new[] { "$99.90", "$99.90" } },
                { RateCategory.TravelMlge, new[] { "$99.9", "$99.9" } },
                { RateCategory.TravelOtc, new[] { "$99.90", "$99.90" } },
                { RateCategory.TravelRc, new[] { "$99.90", "$99.90" } }
            };

            foreach (KeyValuePair<RateCategory, string[]> entry in expectedResults)
            {
                string actualPprd = RateFormatter.FormatRate(RateTarget.PPRD, entry.Key.GetDescription(), value);
                string actualRate = RateFormatter.FormatRate(RateTarget.Rate, entry.Key.GetDescription(), value);
                Assert.AreEqual(entry.Value[(int)RateTarget.PPRD], actualPprd);
                Assert.AreEqual(entry.Value[(int)RateTarget.Rate], actualRate);
            }
        }

        /// <summary>
        /// Test that an exception is thrown if the category isn't in the format dictionary
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestFormatRateForExport_NotInDictionary_EX()
        {
            decimal value = 0.12345m;
            RateFormatter.FormatRate(RateTarget.Rate, "Not a real category description", value);
        }
    }
}
