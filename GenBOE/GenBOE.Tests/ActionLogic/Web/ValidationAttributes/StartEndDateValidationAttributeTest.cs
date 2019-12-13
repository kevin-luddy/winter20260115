// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Web.ValidationAttributes
{
    using System;
    using System.Collections.Generic;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StartEndDateValidationAttributeTest
    {
        private StartEndDateValidationAttribute CreateSystem()
        {
            return new StartEndDateValidationAttribute() { StartDate = "StartDate", EndDate = "EndDate" };
        }

        [TestMethod]
        public void IsValidTest()
        {
            var sut = CreateSystem();

            Dictionary<Tuple<string, string>, bool> expectedResults = new Dictionary<Tuple<string, string>, bool>()
            {
                { new Tuple<string, string>("01/2011", "12/2011"), true },
                { new Tuple<string, string>("11/2011", "12/2011"), true },
                { new Tuple<string, string>("12/2011", "01/2012"), true },
                { new Tuple<string, string>(null, "01/2012"), true },
                { new Tuple<string, string>("12/2011", null), true },
                { new Tuple<string, string>(string.Empty, "01/2012"), true },
                { new Tuple<string, string>("12/2011", string.Empty), true },
                { new Tuple<string, string>("12/2011", "12/2011"), false },
                { new Tuple<string, string>("12/2011", "11/2011"), false },
                { new Tuple<string, string>("01/2011", "12/2010"), false },
                { new Tuple<string, string>("06/2011", "06/2010"), false }
            };

            foreach (KeyValuePair<Tuple<string, string>, bool> testDates in expectedResults)
            {
                bool result = sut.IsValid(new StartEndDateValidationAttributeModelView() { StartDate = testDates.Key.Item1, EndDate = testDates.Key.Item2 });
                Assert.AreEqual(testDates.Value, result);
            }

            sut.ValidIfEitherDateIsNull = false;
            sut.CanBeEqual = true;

            expectedResults = new Dictionary<Tuple<string, string>, bool>()
            {
                { new Tuple<string, string>("12/2011", "12/2011"), true },
                { new Tuple<string, string>(null, "01/2012"), false },
                { new Tuple<string, string>("12/2011", null), false },
                { new Tuple<string, string>(string.Empty, "01/2012"), false },
                { new Tuple<string, string>("12/2011", string.Empty), false }
            };

            foreach (KeyValuePair<Tuple<string, string>, bool> testDates in expectedResults)
            {
                bool result = sut.IsValid(new StartEndDateValidationAttributeModelView() { StartDate = testDates.Key.Item1, EndDate = testDates.Key.Item2 });
                Assert.AreEqual(testDates.Value, result);
            }
        }

        #region Exception Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsValidExceptionTest1()
        {
            var sut = CreateSystem();

            sut.IsValid(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsValidExceptionTest2()
        {
            var sut = CreateSystem();
            sut.StartDate = "Invalid Property";

            sut.IsValid(new StartEndDateValidationAttributeModelView() { StartDate = "01/2011", EndDate = "12/2011" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsValidExceptionTest3()
        {
            var sut = CreateSystem();
            sut.EndDate = "Invalid Property";

            sut.IsValid(new StartEndDateValidationAttributeModelView() { StartDate = "01/2011", EndDate = "12/2011" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsValidExceptionTest4()
        {
            var sut = CreateSystem();
            sut.IsValid(new StartEndDateValidationAttributeModelView() { StartDate = "15/15/2011", EndDate = "12/2011" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsValidExceptionTest5()
        {
            var sut = CreateSystem();
            sut.StartDate = "InvalidDataType";
            sut.IsValid(new StartEndDateValidationAttributeModelView() { InvalidDataType = new List<int>(), EndDate = "12/2011" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsValidExceptionTest6()
        {
            var sut = CreateSystem();
            sut.IsValid(new StartEndDateValidationAttributeModelView() { StartDate = "01/2011", EndDate = "15/15/2011" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsValidExceptionTest7()
        {
            var sut = CreateSystem();
            sut.EndDate = "InvalidDataType";
            sut.IsValid(new StartEndDateValidationAttributeModelView() { StartDate = "01/2011", InvalidDataType = new List<int>() });
        }

        #endregion Exception Tests
    }

    internal class StartEndDateValidationAttributeModelView
    {
        // These are not uncalled, they're just called through reflection (type.GetProperty(string)) from the attribute.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string StartDate { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string EndDate { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public List<int> InvalidDataType { get; set; }
    }
}
