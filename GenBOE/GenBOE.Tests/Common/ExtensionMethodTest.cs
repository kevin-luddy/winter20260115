using System;
using System.Collections.Generic;
using System.Linq;
using IES.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;

namespace GenBOE.Tests.Common
{
    [TestClass]
    public class ExtensionMethodTest
    {
        [TestMethod]
        public void String_IsEquivalentToTest()
        {
            string baseString = "the quick brown fox jumped over the lazy dog";

            Dictionary<string, bool> expectedResults = new Dictionary<string, bool>()
            {
                { "", false},
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
                Assert.AreEqual(result, testString.Value);
            }

            Assert.IsFalse(baseString.IsEquivalentTo(null));

            baseString = null;
            Assert.IsTrue(baseString.IsEquivalentTo(null));
            Assert.IsTrue(baseString.IsEquivalentTo(""));

            baseString = "";
            Assert.IsTrue(baseString.IsEquivalentTo(null));
            Assert.IsTrue(baseString.IsEquivalentTo(""));
        }

        [TestMethod]
        public void String_ContainsEquivalentTest()
        {
            string baseString = "the quick brown fox jumped over the lazy dog";

            Dictionary<string, bool> expectedResults = new Dictionary<string, bool>()
            {
                { "", true},
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
            Assert.IsFalse(baseString.ContainsEquivalent(""));

            baseString = "";
            Assert.IsFalse(baseString.ContainsEquivalent(null));
            Assert.IsTrue(baseString.ContainsEquivalent(""));
        }

        [TestMethod]
        public void DateTime_ToMonthStringTest()
        {
            string date = "01/2011";

            DateTime toTest = new DateTime(2011, 1, 15);

            Assert.AreEqual(date, toTest.ToMonthString());
        }

        [TestMethod]
        public void DateTime_ToDateTimeMidMonthTest()
        {
            DateTime date = new DateTime(2011, 1, 15, 12, 0, 0);

            string toTest = "01/2011";

            Assert.AreEqual(date, toTest.ToDateTimeMidMonth());
        }

        [TestMethod]
        public void String_ToDateTimeTest()
        {
            DateTime date = new DateTime(2011, 1, 1);
            
            string toTest = "01/2011";           

            Assert.AreEqual(date, toTest.ToDateTime());
        }

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

        [TestMethod]
        public void IEnumerable_ThenByTest()
        {
            var dates = new DateTime[]
            {
                new DateTime(2012, 1, 1),
                new DateTime(2012, 2, 1),
                new DateTime(2012, 3, 1)
            };

            var objects = new[]
            {
                new { Date = dates[0], Value = 10 },
                new { Date = dates[2], Value = 0 },
                new { Date = dates[0], Value = 20 },
                new { Date = dates[1], Value = 15 },
                new { Date = dates[0], Value = 5 },
                new { Date = dates[1], Value = 25 }
            };

            var sorted = objects.OrderBy(o => o.Date, SortOrder.Ascending).ThenBy(o => o.Value, SortOrder.Ascending).ToList();

            Assert.IsTrue(sorted[0].Date == dates[0] && sorted[1].Date == dates[0] && sorted[2].Date == dates[0]);
            Assert.IsTrue(sorted[3].Date == dates[1] && sorted[4].Date == dates[1]);
            Assert.IsTrue(sorted[5].Date == dates[2]);
            Assert.IsTrue(sorted[0].Value < sorted[1].Value && sorted[1].Value < sorted[2].Value);
            Assert.IsTrue(sorted[3].Value < sorted[4].Value);

            sorted = objects.OrderBy(o => o.Date, SortOrder.Descending).ThenBy(o => o.Value, SortOrder.Descending).ToList();

            Assert.IsTrue(sorted[3].Date == dates[0] && sorted[4].Date == dates[0] && sorted[5].Date == dates[0]);
            Assert.IsTrue(sorted[1].Date == dates[1] && sorted[2].Date == dates[1]);
            Assert.IsTrue(sorted[0].Date == dates[2]);
            Assert.IsTrue(sorted[3].Value > sorted[4].Value && sorted[4].Value > sorted[5].Value);
            Assert.IsTrue(sorted[1].Value > sorted[2].Value);
        }

        /// <summary>
        /// Basic test for MonthDifference, small positive number
        /// </summary>
        [TestMethod]
        public void DateTime_MonthCount_Test1()
        {
            DateTime start = new DateTime(2010, 1, 1);
            DateTime end = new DateTime(2010, 5, 1);
            int expectedResult = 4;

            int actualResult = start.MonthDifference(end);

            Assert.IsTrue(expectedResult == actualResult);
        }

        /// <summary>
        /// Test for MonthDifference, negative result, small number
        /// </summary>
        [TestMethod]
        public void DateTime_MonthCount_Test2()
        {
            DateTime start = new DateTime(2010, 5, 1);
            DateTime end = new DateTime(2010, 1, 1);
            int expectedResult = -4;

            int actualResult = start.MonthDifference(end);

            Assert.IsTrue(expectedResult == actualResult);
        }

        /// <summary>
        /// Test for MonthDifference, positive result, large number
        /// </summary>
        [TestMethod]
        public void DateTime_MonthCount_Test3()
        {
            DateTime start = new DateTime(2003, 5, 1);
            DateTime end = new DateTime(2010, 1, 1);
            // 2004, 5, 6, 7, 8, 9 -> 6x12
            // + 7(for 2003)
            // + 1(2010) => 80
            int expectedResult = 80;

            int actualResult = start.MonthDifference(end);

            Assert.IsTrue(expectedResult == actualResult);
        }

        /// <summary>
        /// Test for MonthDifference, negative result, large number
        /// </summary>
        [TestMethod]
        public void DateTime_MonthCount_Test4()
        {
            DateTime end = new DateTime(2003, 5, 1);
            DateTime start = new DateTime(2010, 1, 1);
            // 2004, 5, 6, 7, 8, 9 -> 6x12
            // + 7(for 2003)
            // + 1(2010) => -80
            int expectedResult = -80;

            int actualResult = start.MonthDifference(end);

            Assert.IsTrue(expectedResult == actualResult);
        }

        [TestMethod]
        public void GetDescription_Test()
        {
            String Text = IES.Common.ExtensionMethods.GetDescription((SegmentType)(1));
            Assert.AreEqual("Development Segment (DS)", Text);

            Text = IES.Common.ExtensionMethods.GetDescription((SegmentType)(3));
            Assert.AreEqual("LM Services Segment (LS)", Text);
        }

        [TestMethod]
        public void IsValidDate_Test()
        {
            bool isValid = IES.Common.ExtensionMethods.IsValidDate("AA/BB/CC");
            Assert.IsFalse(isValid);

            isValid = IES.Common.ExtensionMethods.IsValidDate("04/15/20156");
            Assert.IsFalse(isValid);

            isValid = IES.Common.ExtensionMethods.IsValidDate("nodate");
            Assert.IsFalse(isValid);

            isValid = IES.Common.ExtensionMethods.IsValidDate("04/15/15");
            Assert.IsTrue(isValid);

            isValid = IES.Common.ExtensionMethods.IsValidDate("04/15");
            Assert.IsTrue(isValid);

            isValid = IES.Common.ExtensionMethods.IsValidDate("7/1/2015");
            Assert.IsTrue(isValid);

            //added for bug 16078
            isValid = IES.Common.ExtensionMethods.IsValidDate(null);
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void String_IsNotEquivalentToTest()
        {
            string baseString = "the quick brown fox jumped over the lazy dog";

            Dictionary<string, bool> expectedResults = new Dictionary<string, bool>()
            {
                { "", true},
                { "the quick brown fox jumped over the lazy dog", false },
                { "tHe qUiCk bRoWn FoX jUmPeD oVeR tHe LaZy DoG", false },
                { "             the quick brown fox jumped over the lazy dog              ", false },
                { "                    THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG", false },
                { "THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG                ", false },
                { "the quick brown cow jumped over the lazy dog", true },
                { "the quick brown fox jumped over the lazy cat", true },
                { "gone", true },
                { "the.quick.brown.fox.jumped.over.the.lazy.dog", true }
            };

            foreach (KeyValuePair<string, bool> testString in expectedResults)
            {
                bool result = baseString.IsNotEquivalentTo(testString.Key);
                Assert.AreEqual(result, testString.Value);
            }

            Assert.IsTrue(baseString.IsNotEquivalentTo(null));

            baseString = null;
            Assert.IsFalse(baseString.IsNotEquivalentTo(null));
            Assert.IsFalse(baseString.IsNotEquivalentTo(""));

            baseString = "";
            Assert.IsFalse(baseString.IsNotEquivalentTo(null));
            Assert.IsFalse(baseString.IsNotEquivalentTo(""));
        }

        [TestMethod]
        public void ToCollection_Test()
        {
            List<string> toConvert = new List<string> {"Apple", "Orange", "Cherry" };

            Collection<string> converted = IES.Common.ExtensionMethods.ToCollection(toConvert);
            Assert.IsTrue(converted.Contains("Apple"));
            Assert.IsTrue(converted.Contains("Orange"));
            Assert.IsTrue(converted.Contains("Cherry"));
           
        }

        [TestMethod]
        public void TestEnumExtensionMethods()
        {
            string description;
            string name;
            int? intValue;
            string enumStringValue;
            MOQType enumValue;
            MOQType? nullableEnumValue;
            ICollection<MOQType> allEnumValues;

            const string COMPARISON_DESCRIPTION = "Comparison";

            description = MOQType.Comparison.GetDescription<MOQType>();
            Assert.AreEqual(COMPARISON_DESCRIPTION, description);
            
            description = MOQType.Comparison.ToDescription<MOQType>();
            Assert.AreEqual(COMPARISON_DESCRIPTION, description);
            
            name = MOQType.Comparison.GetName();
            Assert.AreEqual(COMPARISON_DESCRIPTION, name);
            
            enumStringValue = MOQType.Comparison.ToString();
            enumValue = enumStringValue.GetEnumeratedValue<MOQType>(MOQType.Unit);
            Assert.AreEqual(MOQType.Comparison, enumValue);

            enumStringValue = string.Empty;
            enumValue = enumStringValue.GetEnumeratedValue<MOQType>(MOQType.Unit);
            Assert.AreEqual(MOQType.Unit, enumValue);
            nullableEnumValue = enumStringValue.GetEnumeratedValueNullable<MOQType>();
            Assert.IsNull(nullableEnumValue);

            intValue = MOQType.Comparison.GetNullableIntValue<MOQType>();
            Assert.AreEqual((int)MOQType.Comparison, intValue);
            
            intValue = MOQType.Comparison.GetNullableIntValue<MOQType>(MOQType.Comparison, MOQType.Factor, MOQType.Judgment);
            Assert.IsNull(intValue);
            
            intValue = MOQType.Comparison.GetNullableIntValue<MOQType>(MOQType.Factor, MOQType.Judgment);
            Assert.AreEqual((int)MOQType.Comparison, intValue);
            
            allEnumValues = IES.Common.ExtensionMethods.GetEnumValues<MOQType>();
            Assert.IsTrue(allEnumValues.Any());

            return;
        }
    }
}
