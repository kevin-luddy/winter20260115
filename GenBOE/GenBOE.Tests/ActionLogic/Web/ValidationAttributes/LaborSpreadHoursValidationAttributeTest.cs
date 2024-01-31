// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.Tests.ActionLogic.Web.ValidationAttributes
{
    using System;
    using System.Collections.Generic;
    using GenBOE.ActionLogic.ValidationAttributes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LaborSpreadHoursValidationAttributeTest
    {
        private LaborSpreadHoursValidationAttribute CreateSystem()
        {
            return new LaborSpreadHoursValidationAttribute() { RateTypeChanged = "RateTypeChanged", LaborSpreadValue = "LaborSpreadValue", IsSpreadTypeCost = "IsSpreadTypeCost", ResourceDecimalPrecision = "ResourceDecimalPrecision", CostDecimalPrecision = "CostDecimalPrecision" };
        }

        /// <summary>
        /// Tests maximum lenghts of spread values with default resource hours precision of 0 and cost precision of 2.
        /// </summary>
        [TestMethod]
        public void LSHV_IsValidTestWithDefaultPrecisions()
        {
			LaborSpreadHoursValidationAttribute sut = CreateSystem();

            Dictionary<Tuple<string, bool>, bool> expectedResults = new Dictionary<Tuple<string, bool>, bool>()
            {
                // If SpreadType is "Hours", the spread value must be between -9,999,999,999 and 9,999,999,999.
                { new Tuple<string, bool>("123", false), true },
                { new Tuple<string, bool>("1234567890", false), true },
                { new Tuple<string, bool>("-1234567890", false), true },
                { new Tuple<string, bool>("12345678901", false), false },
                { new Tuple<string, bool>("-12345678901", false), false },
                { new Tuple<string, bool>("1234567890.12", false), false },
                { new Tuple<string, bool>("-1234567890.12", false), false },
                { new Tuple<string, bool>("1234567890.123", false), false },
                { new Tuple<string, bool>("-1234567890.123", false), false },
                
                // If SpreadType is "Cost" with a cost precision of 2 the spread value must be between -9,999,999,999.99 and 
                // 9,999,999,999.99.
                { new Tuple<string, bool>("123", true), true },
                { new Tuple<string, bool>("1234567890", true), true },
                { new Tuple<string, bool>("-1234567890", true), true },
                { new Tuple<string, bool>("12345678901", true), false },
                { new Tuple<string, bool>("-12345678901", true), false },
                { new Tuple<string, bool>("1234567890.12", true), true },
                { new Tuple<string, bool>("-1234567890.12", true), true },
                { new Tuple<string, bool>("1234567890.123", true), false },
                { new Tuple<string, bool>("-1234567890.123", true), false }
            };

            foreach (KeyValuePair<Tuple<string, bool>, bool> testSpread in expectedResults)
            {
                bool result = sut.IsValid(new LaborSpreadHoursValidationAttributeModelView() { LaborSpreadValue = testSpread.Key.Item1, IsSpreadTypeCost = testSpread.Key.Item2, ResourceDecimalPrecision = 0 , CostDecimalPrecision = 2 });
                Assert.AreEqual(testSpread.Value, result);
            }
        }

        /// <summary>
        /// Tests spread values for resource hours with a decimal precision of 2.
        /// </summary>
        [TestMethod]
        public void LSHV_IsHoursValidWithPrecision()
        {
			LaborSpreadHoursValidationAttribute sut = CreateSystem();

            Dictionary<Tuple<string, bool>, bool> expectedResults = new Dictionary<Tuple<string, bool>, bool>()
            {
                // If SpreadType is "Hours", the spread value must be between -9,999,999,999 and 9,999,999,999.
                { new Tuple<string, bool>("123", false), true },
                { new Tuple<string, bool>("1234567890", false), true },
                { new Tuple<string, bool>("-1234567890", false), true },
                { new Tuple<string, bool>("12345678901", false), false },
                { new Tuple<string, bool>("-12345678901", false), false },
                { new Tuple<string, bool>("1234567890.12", false), true },
                { new Tuple<string, bool>("-1234567890.12", false), true },
                { new Tuple<string, bool>("1234567890.123", false), false },
                { new Tuple<string, bool>("-1234567890.123", false), false }
            };

            foreach (KeyValuePair<Tuple<string, bool>, bool> testSpread in expectedResults)
            {
                bool result = sut.IsValid(new LaborSpreadHoursValidationAttributeModelView() { LaborSpreadValue = testSpread.Key.Item1, IsSpreadTypeCost = testSpread.Key.Item2, ResourceDecimalPrecision = 2 });
                Assert.AreEqual(testSpread.Value, result);
            }
        }

        /// <summary>
        /// Tests spread values for resource costs with a decimal precision of 0.
        /// </summary>
        [TestMethod]
        public void LSHV_IsCostsValidWithPrecision()
        {
			LaborSpreadHoursValidationAttribute sut = CreateSystem();

            Dictionary<Tuple<string, bool>, bool> expectedResults = new Dictionary<Tuple<string, bool>, bool>()
            {
                // If SpreadType is "Hours", the spread value must be between -9,999,999,999 and 9,999,999,999.
                { new Tuple<string, bool>("123", true), true },
                { new Tuple<string, bool>("1234567890", true), true },
                { new Tuple<string, bool>("-1234567890", true), true },
                { new Tuple<string, bool>("1234567890.00", true), false },
                { new Tuple<string, bool>("12345678901", true), false },
                { new Tuple<string, bool>("-12345678901", true), false },
                { new Tuple<string, bool>("1234567890.12", true), false },
                { new Tuple<string, bool>("-1234567890.12", true), false },
                { new Tuple<string, bool>("1234567890.123", true), false },
                { new Tuple<string, bool>("-1234567890.123", true), false }
            };

            foreach (KeyValuePair<Tuple<string, bool>, bool> testSpread in expectedResults)
            {
                bool result = sut.IsValid(new LaborSpreadHoursValidationAttributeModelView() { LaborSpreadValue = testSpread.Key.Item1, IsSpreadTypeCost = testSpread.Key.Item2, ResourceDecimalPrecision = 2, CostDecimalPrecision = 0 });
                Assert.AreEqual(testSpread.Value, result);
            }
        }

        #region Exception Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LSHV_IsValidExceptionTest1()
        {
			LaborSpreadHoursValidationAttribute sut = CreateSystem();

            sut.IsValid(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LSHV_IsValidExceptionTest2()
        {
			LaborSpreadHoursValidationAttribute sut = CreateSystem();
            sut.LaborSpreadValue = "Invalid Property";
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { LaborSpreadValue = "1234", IsSpreadTypeCost = false });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LSHV_IsValidExceptionTest3()
        {
			LaborSpreadHoursValidationAttribute sut = CreateSystem();
            sut.IsSpreadTypeCost = "Invalid Property";
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { LaborSpreadValue = "1234", IsSpreadTypeCost = false });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LSHV_IsValidExceptionTest4()
        {
			LaborSpreadHoursValidationAttribute sut = CreateSystem();
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { LaborSpreadValue = "1234ab", IsSpreadTypeCost = false, ResourceDecimalPrecision = "1" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LSHV_IsValidExceptionTest5()
        {
			LaborSpreadHoursValidationAttribute sut = CreateSystem();
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { LaborSpreadValue = string.Empty, IsSpreadTypeCost = false });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LSHV_IsValidExceptionTest6()
        {
			LaborSpreadHoursValidationAttribute sut = CreateSystem();
            sut.LaborSpreadValue = "InvalidDataType";
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { InvalidDataType = new List<int>(), IsSpreadTypeCost = false });
        }

        [TestMethod]
        public void LSHV_IsValidNoExceptionTest7()
        {
			LaborSpreadHoursValidationAttribute sut = CreateSystem();
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() {  });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LSHV_IsValidExceptionTest8()
        {
			LaborSpreadHoursValidationAttribute sut = CreateSystem();
            sut.IsSpreadTypeCost = "InvalidDataType";
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { LaborSpreadValue = "1234", InvalidDataType = new List<int>() });
        }

        #endregion Exception Tests

    }

    internal class LaborSpreadHoursValidationAttributeModelView
    {
        // These are not uncalled, they're just called through reflection (type.GetProperty(string)) from the attribute.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string LaborSpreadValue { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool IsSpreadTypeCost { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool RateTypeChanged { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public int ResourceDecimalPrecision { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public int CostDecimalPrecision { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public List<int> InvalidDataType { get; set; }
    }
}
