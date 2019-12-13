// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test ExtensionMethods
    /// </summary>
    [TestClass]
    public class ExtensionMethodsTest
    {
        /// <summary>
        /// IsEquivalentTo
        /// </summary>
        [TestMethod]
        public void String_IsEquivalentToTest()
        {
            string baseString = "the quick brown fox jumped over the lazy dog";

            Dictionary<string, bool> expectedResults = new Dictionary<string, bool>()
            {
                { string.Empty, false},
                { "the quick brown fox jumped over the lazy dog", true },
                { "tHe qUiCk bRoWn FoX jUmPeD oVeR tHe LaZy DoG", true },
                { "             the quick brown fox jumped over the lazy dog              ", true },
                { "                    THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG", true },
                { "THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG                ", true },
                { "the quick brown cow jumped over the lazy dog", false },
                { "the quick brown fox jumped over the lazy cat", false },
                { "gone", false },
                { "the.quick.brown.fox.jumped.over.the.lazy.dog", false }
            };

            foreach (KeyValuePair<string, bool> testString in expectedResults)
            {
                bool result = baseString.IsEquivalentTo(testString.Key);
                Assert.AreEqual(testString.Value, result);
            }

            Assert.IsFalse(baseString.IsEquivalentTo(null));

            baseString = null;
            Assert.IsTrue(baseString.IsEquivalentTo(null));
            Assert.IsTrue(baseString.IsEquivalentTo(string.Empty));

            baseString = string.Empty;
            Assert.IsTrue(baseString.IsEquivalentTo(null));
            Assert.IsTrue(baseString.IsEquivalentTo(string.Empty));
        }

        /// <summary>
        /// ContainsEquivalent
        /// </summary>
        [TestMethod]
        public void String_ContainsEquivalentTest()
        {
            string baseString = "the quick brown fox jumped over the lazy dog";

            Dictionary<string, bool> expectedResults = new Dictionary<string, bool>()
            {
                { string.Empty, true},
                { "the quick brown fox jumped over the lazy dog", true },
                { "tHe qUiCk bRoWn FoX jUmPeD oVeR tHe LaZy DoG", true },
                { "             the quick brown fox jumped over the lazy dog              ", true },
                { "                    THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG", true },
                { "THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG                ", true },
                { "the quick brown cow jumped over the lazy dog", false },
                { "the quick brown fox jumped over the lazy cat", false },
                { "gone", false },
                { "the.quick.brown.fox.jumped.over.the.lazy.dog", false },
                { "FOX", true },
                { "   dog    ", true },
                { "wn f      ", true },
                { "           ", true },
                { "abc", false },
                { " q u i c k ", false },
                { "123", false },
                { "QuIcK bRoWn FoX     ", true }
            };

            foreach (KeyValuePair<string, bool> testString in expectedResults)
            {
                bool result = baseString.ContainsEquivalent(testString.Key);
                Assert.AreEqual(result, testString.Value);
            }

            Assert.IsFalse(baseString.ContainsEquivalent(null));

            baseString = null;
            Assert.IsFalse(baseString.ContainsEquivalent(null));
            Assert.IsFalse(baseString.ContainsEquivalent(string.Empty));

            baseString = string.Empty;
            Assert.IsFalse(baseString.ContainsEquivalent(null));
            Assert.IsTrue(baseString.ContainsEquivalent(string.Empty));
        }

        /// <summary>
        /// string.ToDateTime
        /// </summary>
        [TestMethod]
        public void String_ToDateTimeTest()
        {
            DateTime date = new DateTime(2011, 1, 1);

            List<Tuple<string, string>> toTest = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("01/2011", "MM/yyyy"),
                new Tuple<string, string>("1/2011", "MM/yyyy"),
                new Tuple<string, string>("1/11", "MM/yyyy"),
                new Tuple<string, string>("01/01/2011", "MM/dd/yyyy"),
                new Tuple<string, string>("1/01/2011", "MM/dd/yyyy"),
                new Tuple<string, string>("01/1/2011", "MM/dd/yyyy"),
                new Tuple<string, string>("01/01/11", "MM/dd/yyyy"),
                new Tuple<string, string>("1/1/2011", "MM/dd/yyyy"),
                new Tuple<string, string>("01/1/11", "MM/dd/yyyy"),
                new Tuple<string, string>("1/01/11", "MM/dd/yyyy"),
                new Tuple<string, string>("1/1/11", "MM/dd/yyyy")
            };

            foreach (Tuple<string, string> testString in toTest)
            {
                Assert.AreEqual(date, testString.Item1.ToDateTime(testString.Item2));
            }
        }

        /// <summary>
        /// PropertyNameSort
        /// </summary>
        [TestMethod]
        public void IEnumerable_PropertyNameSortTest()
        {
            var objects = new[]
            {
                new { Date = new DateTime(2012, 1, 1) },
                new { Date = new DateTime(2012, 2, 1) },
                new { Date = new DateTime(2012, 3, 1) }
            };

            var sorted = objects.PropertyNameSort("Date").ToList();

            Assert.IsTrue(sorted[0].Date < sorted[1].Date && sorted[1].Date < sorted[2].Date);

            sorted = objects.PropertyNameSort(SortOrder.Descending, "Date").ToList();

            Assert.IsTrue(sorted[0].Date > sorted[1].Date && sorted[1].Date > sorted[2].Date);
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        [TestMethod]
        public void IEnumerable_OrderByTest()
        {
            var objects = new[]
            {
                new { Date = new DateTime(2012, 1, 1) },
                new { Date = new DateTime(2012, 2, 1) },
                new { Date = new DateTime(2012, 3, 1) }
            };

            var sorted = objects.OrderBy(o => o.Date, SortOrder.Ascending).ToList();

            Assert.IsTrue(sorted[0].Date < sorted[1].Date && sorted[1].Date < sorted[2].Date);

            sorted = objects.OrderBy(o => o.Date, SortOrder.Descending).ToList();

            Assert.IsTrue(sorted[0].Date > sorted[1].Date && sorted[1].Date > sorted[2].Date);
        }

        /// <summary>
        /// Test enum for description
        /// </summary>
        private enum TestEnumDescription
        {
            /// <summary>
            /// Test enum value with description
            /// </summary>
            [System.ComponentModel.Description("Description")]
            WithDescription,

            /// <summary>
            /// Test enum value without description
            /// </summary>
            WithoutDescription,

            /// <summary>
            /// Test enum value with different description
            /// </summary>
            [System.ComponentModel.Description("Different Description")]
            WithDifferentDescription
        }

        /// <summary>
        /// Test for enum GetDescription
        /// </summary>
        [TestMethod]
        public void GetDescriptionTest()
        {
            string description1 = TestEnumDescription.WithDescription.GetDescription();
            Assert.AreEqual("Description", description1);
            string description2 = TestEnumDescription.WithoutDescription.GetDescription();
            Assert.AreEqual(TestEnumDescription.WithoutDescription.ToString(), description2);
        }

        /// <summary>
        /// Test for enum GetValueFromDescription
        /// </summary>
         [TestMethod]
        public void GetValueFromDescriptionTest()
        {
            string description = "Description";
            string differentDescription = "Different Description";

            Assert.AreEqual(TestEnumDescription.WithDescription, ExtensionMethods.GetValueFromDescription<TestEnumDescription>(description));
            Assert.AreEqual(TestEnumDescription.WithDifferentDescription, ExtensionMethods.GetValueFromDescription<TestEnumDescription>(differentDescription));         
        }

        /// <summary>
        /// Test enum for is active
        /// </summary>
        private enum TestEnumIsActive
        {
            /// <summary>
            /// Test enum value with is active = true
            /// </summary>
            [IES.Common.IsActive(true)]
            IsActiveTrue,

            /// <summary>
            /// Test enum value with is active = false
            /// </summary>
            [IES.Common.IsActive(false)]
            IsActiveFalse,

            /// <summary>
            /// Test enum value with is active not specified
            /// </summary>
            IsActiveNotSpecified
        }

        /// <summary>
        /// Test for enum IsActive
        /// </summary>
        [TestMethod]
        public void IsActiveTest()
        {
            Assert.IsTrue(TestEnumIsActive.IsActiveTrue.IsActive());
            Assert.IsFalse(TestEnumIsActive.IsActiveFalse.IsActive());
            Assert.IsTrue(TestEnumIsActive.IsActiveNotSpecified.IsActive());
        }

        #region Exception Tests

        /// <summary>
        /// string.ToDateTime with exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToDateTime_ExceptionTest()
        {
            string toTest = null;
            toTest.ToDateTime();
        }

        /// <summary>
        /// string.ToDateTime with format exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ToDateTime_FormatExceptionTest1()
        {
            string toTest = "invalid";
            toTest.ToDateTime("invalidFormat");
        }

        /// <summary>
        /// string.ToDateTime with format exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ToDateTime_FormatExceptionTest2()
        {
            string toTest = "invalid";
            toTest.ToDateTime("MM/yyyy");
        }

        /// <summary>
        /// The description does not exist in the enum.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetValueFromDescription_ExceptionTest()
        {
            ExtensionMethods.GetValueFromDescription<TestEnumDescription>("Description does not exist");
        }

        /// <summary>
        /// The type is not an enum.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetValueFromDescription_ExceptionTest2()
        {
            ExtensionMethods.GetValueFromDescription<TimeZone>("Description does not exist");
        }

        #endregion Exception Tests
    }
}
