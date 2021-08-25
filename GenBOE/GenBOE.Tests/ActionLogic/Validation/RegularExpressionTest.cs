// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.Tests.ActionLogic.Validation
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using GenBOE.ActionLogic.Validation;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RegularExpressionTest
    {
        /// <summary>
        /// Create static Regex object for WBS Number.
        /// </summary>
        private static Regex regexWbsNumber = new Regex(ValidationConstants.WBS_NUMBER, RegexOptions.None, Constants.REGEX_TIMEOUT);

        /// <summary>
        /// Create static Regex object for No Spaces.
        /// </summary>
        private static Regex regexNoSpaces = new Regex(ValidationConstants.NO_SPACES, RegexOptions.None, Constants.REGEX_TIMEOUT);

        /// <summary>
        /// Create static Regex object for Date Month Year.
        /// </summary>
        private static Regex regexDateMonthYear = new Regex(ValidationConstants.DATE_MONTH_YEAR, RegexOptions.None, Constants.REGEX_TIMEOUT);

        /// <summary>
        /// Create static Regex object for Date Full.
        /// </summary>
        private static Regex regexDateFull = new Regex(ValidationConstants.DATE_FULL, RegexOptions.None, Constants.REGEX_TIMEOUT);

        /// <summary>
        /// Create static Regex object for Rate Decimal.
        /// </summary>
        private static Regex regexRateDecimal = new Regex(ValidationConstants.RATE_DECIMAL, RegexOptions.None, Constants.REGEX_TIMEOUT);

        /// <summary>
        /// Create static Regex object for Resource Rate Decimal.
        /// </summary>
        private static Regex regexResourceRateDecimal = new Regex(ValidationConstants.RESOURCE_RATE_DECIMAL, RegexOptions.None, Constants.REGEX_TIMEOUT);

        /// <summary>
        /// Create static Regex object for TM Resource Rate Decimal.
        /// </summary>
        private static Regex regexTmResourceRateDecimal = new Regex(ValidationConstants.TM_RESOURCE_RATE_DECIMAL, RegexOptions.None, Constants.REGEX_TIMEOUT);

        [TestMethod]
        public void WbsNumberTest()
        {
            Dictionary<string, bool> expectedResults = new Dictionary<string, bool>() { 
                {"1", true},
                {"11", true},
                {"abcde", true},
                {"abcdef", false},
                {"123ad", true},
                {"1.2.3.4.5.6.7.8.9.0", true},
                {"a.b.c.d.e.f.g.h.i.j", true},
                {"a.b.c.d.e.f.g.h.i.j.k", false},
                {"0.1", true},
                {".1", false},
                {"9102.dasd.d908.d098a.da90s.fas09.df9.qpz.d.0", true},
                {"1.2.3.4.5.6.7.8.9.0.1", false},
                {"9..z", false},
                {"1.4.ad.98zerf", false}
            };

            foreach (KeyValuePair<string, bool> regexTest in expectedResults)
            {
                bool result = regexWbsNumber.IsMatch(regexTest.Key);
                Assert.IsTrue(result == regexTest.Value);
            }
        }

        [TestMethod]
        public void NoSpacesStringTest()
        {
            Dictionary<string, bool> expectedResults = new Dictionary<string, bool>() { 
                {"1", true},
                {"1 1", false},
                {" 2332sdfsdfdfsa", false},
                {"sdffaf7897893_+(()*<>?/.,;'{}", true},
                {"123ad ", false},
                {" 3234 ", false}
            };

            foreach (KeyValuePair<string, bool> regexTest in expectedResults)
            {
                bool result = regexNoSpaces.IsMatch(regexTest.Key);
                Assert.IsTrue(result == regexTest.Value);
            }
        }

        [TestMethod]
        public void MonthYearDateTest()
        {
            Dictionary<string, bool> expectedResults = new Dictionary<string, bool>() { 
                {"01/1000", true},
                {"01/2999", true},
                {"01/0999", false},
                {"01/3000", false},
                {"00/2000", false},
                {"12/2000", true},
                {"13/2000", false},
                {"0a/2000", false},
                {"03/200a", false},
                {"1/2000", true},
                {"9/2000", true},
                {"19/20001", false},
                {"a9/2000a", false}
            };

            foreach (KeyValuePair<string, bool> regexTest in expectedResults)
            {
                bool result = regexDateMonthYear.IsMatch(regexTest.Key);
                Assert.IsTrue(result == regexTest.Value);
            }
        }

        [TestMethod]
        public void FullDateTest()
        {
            Dictionary<string, bool> expectedResults = new Dictionary<string, bool>() { 
                {"01/15/1000", true},
                {"01/15/2999", true},
                {"01/15/0999", false},
                {"01/15/3000", false},
                {"00/15/2000", false},
                {"12/15/2000", true},
                {"13/15/2000", false},
                {"0a/15/2000", false},
                {"03/15/200a", false},
                {"1/15/2000", true},
                {"9/15/2000", true},
                {"19/15/20001", false},
                {"a9/15/2000a", false},
                {"12/515/2000", false},
                {"12/155/2000", false},
                {"12/a15/2000", false},
                {"12/1a/2000", false},
                {"12/00/2000", false},
                {"12/32/2000", false}
            };

            foreach (KeyValuePair<string, bool> regexTest in expectedResults)
            {
                bool result = regexDateFull.IsMatch(regexTest.Key);
                Assert.IsTrue(result == regexTest.Value);
            }
        }

        [TestMethod]
        public void RatesDecimalTest()
        {
            Dictionary<string, bool> expectedResults = new Dictionary<string, bool>() { 
                {"1", true},
                {"12", true},
                {"123", true},
                {"1234", true},
                {"1234.1", true},
                {"1234.12", true},
                {"1234.123", true},
                {"a", false},
                {"a.12", false},
                {"12345", false},
                {"123.1234", false},
                {"1.", false},
                {"1.a", false},
                {"12345.12", false}
            };

            foreach (KeyValuePair<string, bool> regexTest in expectedResults)
            {
                bool result = regexRateDecimal.IsMatch(regexTest.Key);
                Assert.IsTrue(result == regexTest.Value);
            }
        }

        [TestMethod]
        public void ResourceRateDecimalTest()
        {
            Dictionary<string, bool> expectedResults = new Dictionary<string, bool>() { 
                {"1", true},
                {"12", true},
                {"123", true},
                {"123.1", true},
                {"123.12", true},
                {".1", true},
                {".12", true},
                {"1234", false},
                {"1234.1", false},
                {"1234.12", false},
                {"1234.123", false},
                {"a", false},
                {"a.12", false},
                {"12345", false},
                {"123.1234", false},
                {"1.", false},
                {"1.a", false},
                {"12345.12", false}
            };

            foreach (KeyValuePair<string, bool> regexTest in expectedResults)
            {
                bool result = regexResourceRateDecimal.IsMatch(regexTest.Key);
                Assert.IsTrue(result == regexTest.Value);
            }

            expectedResults = new Dictionary<string, bool>() {
                {"1", true},
                {"12", true},
                {"123", true},
                {"1234", true},
                {"1234.1", true},
                {"1234.12", true},
                {".1", true},
                {".12", true},
                {"12345", false},
                {"12345.1", false},
                {"12345.12", false},
                {"12345.123", false},
                {"a", false},
                {"a.12", false},
                {"123456", false},
                {"123.12345", false},
                {"1.", false},
                {"1.a", false},
                {"123456.12", false}
            };

            foreach (KeyValuePair<string, bool> regexTest in expectedResults)
            {
                bool result = regexTmResourceRateDecimal.IsMatch(regexTest.Key);
                Assert.IsTrue(result == regexTest.Value);
            }

        }
    }
}
