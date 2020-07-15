// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Web.ValidationAttributes
{
    using System;
    using System.Collections.Generic;
    using GenBOE.ActionLogic.ValidationAttributes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LaborSpreadCostValidationAttributeTest
    {
        private LaborSpreadCostValidationAttribute CreateSystem()
        {
            return new LaborSpreadCostValidationAttribute() { LaborSpreadValue = "LaborSpreadValue", IsSpreadTypeCost = "IsSpreadTypeCost", RateTypeChanged = "RateTypeChanged" };
        }

        [TestMethod]
        public void LSCV_IsValidTest()
        {
            var sut = CreateSystem();

            Dictionary<Tuple<string, bool>, bool> expectedResults = new Dictionary<Tuple<string, bool>, bool>()
            {
                // If SpreadType is "Hours", skip validation (return true).
                { new Tuple<string, bool>("123", false), true },
                { new Tuple<string, bool>("1234567890", false), true },
                { new Tuple<string, bool>("-1234567890", false), true },
                { new Tuple<string, bool>("12345678901", false), true },
                { new Tuple<string, bool>("-12345678901", false), true },
                { new Tuple<string, bool>("1234567890.12", false), true },
                { new Tuple<string, bool>("-1234567890.12", false), true },
                { new Tuple<string, bool>("1234567890.123", false), true },
                { new Tuple<string, bool>("-1234567890.123", false), true },
                // If SpreadType is "Cost", the spread value must be between -9,999,999,999.99 and 9,999,999,999.99.
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
                bool result = sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { LaborSpreadValue = testSpread.Key.Item1, IsSpreadTypeCost = testSpread.Key.Item2, ResourceDecimalPrecision = "1", CostDecimalPrecision = "2" });
                Assert.AreEqual(testSpread.Value, result);
            }
        }

        #region Exception Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LSCV_IsValidExceptionTest1()
        {
            var sut = CreateSystem();

            sut.IsValid(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LSCV_IsValidExceptionTest2()
        {
            var sut = CreateSystem();
            sut.LaborSpreadValue = "Invalid Property";
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { LaborSpreadValue = "1234", IsSpreadTypeCost = true });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LSCV_IsValidExceptionTest3()
        {
            var sut = CreateSystem();
            sut.IsSpreadTypeCost = "Invalid Property";
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { LaborSpreadValue = "1234", IsSpreadTypeCost = true });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LSCV_IsValidExceptionTest4()
        {
            var sut = CreateSystem();
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { LaborSpreadValue = "1234ab", IsSpreadTypeCost = true });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LSCV_IsValidExceptionTest5()
        {
            var sut = CreateSystem();
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { LaborSpreadValue = string.Empty, IsSpreadTypeCost = true });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LSCV_IsValidExceptionTest6()
        {
            var sut = CreateSystem();
            sut.LaborSpreadValue = "InvalidDataType";
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { InvalidDataType = new List<int>(), IsSpreadTypeCost = true });
        }

        [TestMethod]
        public void LSCV_IsValidNoExceptionTest7()
        {
            var sut = CreateSystem();
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() {  });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LSCV_IsValidExceptionTest8()
        {
            var sut = CreateSystem();
            sut.IsSpreadTypeCost = "InvalidDataType";
            sut.IsValid(new LaborSpreadCostValidationAttributeModelView() { LaborSpreadValue = "1234", InvalidDataType = new List<int>() });
        }

        #endregion Exception Tests

    }

    internal class LaborSpreadCostValidationAttributeModelView
    {
        // These are not uncalled, they're just called through reflection (type.GetProperty(string)) from the attribute.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string LaborSpreadValue { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool IsSpreadTypeCost { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool RateTypeChanged { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string ResourceDecimalPrecision { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string CostDecimalPrecision { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public List<int> InvalidDataType { get; set; }
    }
}
