// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.Common
{
    using System;
    using System.Collections.Generic;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UtilitiesTest
    {
        /// <summary>
        /// Test the Cutoff Date
        /// </summary>
        [TestMethod]
        public void CutoffDateTest()
        {
            int numHoursCutoffOffset = 4;

            // working hours are 9am-8pm
            // Dec 16, 2016 is a friday
            DateTime currentTime = new DateTime(2016, 12, 16, 13, 0, 0);
            DateTime cutoffTime = Utilities.GetWorkflowCutoffDate(numHoursCutoffOffset, currentTime);
            Assert.AreEqual(new DateTime(2016, 12, 16, 9, 0, 0), cutoffTime);

            // Test after hours on friday
            currentTime = new DateTime(2016, 12, 16, 20, 0, 0);
            cutoffTime = Utilities.GetWorkflowCutoffDate(numHoursCutoffOffset, currentTime);
            Assert.AreEqual(new DateTime(2016, 12, 16, 16, 0, 0), cutoffTime);

            // Test Saturday hours
            currentTime = new DateTime(2016, 12, 17, 20, 0, 0);
            cutoffTime = Utilities.GetWorkflowCutoffDate(numHoursCutoffOffset, currentTime);
            Assert.AreEqual(new DateTime(2016, 12, 16, 16, 0, 0), cutoffTime);

            // Test after hours on friday
            currentTime = new DateTime(2016, 12, 18, 4, 0, 0);
            cutoffTime = Utilities.GetWorkflowCutoffDate(numHoursCutoffOffset, currentTime);
            Assert.AreEqual(new DateTime(2016, 12, 16, 16, 0, 0), cutoffTime);

            // Test before hours on Monday
            currentTime = new DateTime(2016, 12, 19, 4, 0, 0);
            cutoffTime = Utilities.GetWorkflowCutoffDate(numHoursCutoffOffset, currentTime);
            Assert.AreEqual(new DateTime(2016, 12, 16, 16, 0, 0), cutoffTime);

            // Test after starting hours on Monday, but wrapping around to Friday with offset
            currentTime = new DateTime(2016, 12, 19, 10, 0, 0);
            cutoffTime = Utilities.GetWorkflowCutoffDate(numHoursCutoffOffset, currentTime);
            Assert.AreEqual(new DateTime(2016, 12, 16, 17, 0, 0), cutoffTime);

            // Test during hours on Monday, no wrap
            currentTime = new DateTime(2016, 12, 19, 14, 0, 0);
            cutoffTime = Utilities.GetWorkflowCutoffDate(numHoursCutoffOffset, currentTime);
            Assert.AreEqual(new DateTime(2016, 12, 19, 10, 0, 0), cutoffTime);

            // Test on Tuesday with wrap
            currentTime = new DateTime(2016, 12, 20, 11, 0, 0);
            cutoffTime = Utilities.GetWorkflowCutoffDate(numHoursCutoffOffset, currentTime);
            Assert.AreEqual(new DateTime(2016, 12, 19, 18, 0, 0), cutoffTime);

            // Test during hours on Tuesday, no wrap
            currentTime = new DateTime(2016, 12, 20, 17, 0, 0);
            cutoffTime = Utilities.GetWorkflowCutoffDate(numHoursCutoffOffset, currentTime);
            Assert.AreEqual(new DateTime(2016, 12, 20, 13, 0, 0), cutoffTime);
        }

        [TestMethod]
        public void Test_RemoveBrTagsFromText()
        {
            List<string> inputs = new List<string>()
            {
                null,
                string.Empty,
                "a<br />a",
                "a<br/>a",
                "a<BR />a",
                "a<BR/>a",
                "a<br>a",
                "a<br >a",
                "a<BR >a",
                "<BRa"

            };

            List<string> expectedResults = new List<string>() 
            { 
                null,
                string.Empty,
                "aa",
                "aa",
                "aa",
                "aa",
                "aa",
                "aa",
                "aa",
                "<BRa"
            };

            for (int i = 0; i < inputs.Count; i++)
            {
                string actualResult = Utilities.RemoveBrTagsFromText(inputs[i]);

                Assert.AreEqual(expectedResults[i], actualResult);
            }
        }

        [TestMethod]
        public void Test_FormatNumberTitleString()
        {
            Assert.AreEqual("MULTI", IES.Common.Utilities.FormatNumberTitleString("MULTI", "MULTI", " "));
            Assert.AreEqual("Number MULTI", IES.Common.Utilities.FormatNumberTitleString("Number", "MULTI", " "));
            Assert.AreEqual("MULTI Title", IES.Common.Utilities.FormatNumberTitleString("MULTI", "Title", " "));
            Assert.AreEqual("Number Title", IES.Common.Utilities.FormatNumberTitleString("Number", "Title", " "));
            Assert.AreEqual("Number ", IES.Common.Utilities.FormatNumberTitleString("Number", null, " "));
            Assert.AreEqual(" Title", IES.Common.Utilities.FormatNumberTitleString(null, "Title", " "));
        }

        /// <summary>
        /// Test that PrecisionFormattingStringWithTrailingZeros returns the proper formatting string
        /// </summary>
        [TestMethod]
        public void Test_PrecisionFormattingStringWithTrailingZeros()
        {
            Assert.AreEqual("#,##0.", Utilities.PrecisionFormattingStringWithTrailingZeros(0));
            Assert.AreEqual("#,##0.", Utilities.PrecisionFormattingStringWithTrailingZeros(null));
            Assert.AreEqual("#,##0.00", Utilities.PrecisionFormattingStringWithTrailingZeros(2));

            Assert.AreEqual("0.00", (0).ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(2)));
            Assert.AreEqual("0.10", (0.1).ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(2)));
            Assert.AreEqual("0.00", (0.001).ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(2)));
            Assert.AreEqual("1,234.568", (1234.5678).ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(3)));
            Assert.AreEqual("1,234,567", (1234567).ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(0)));
            Assert.AreEqual("1,234,567", (1234567).ToString(Utilities.PrecisionFormattingStringWithTrailingZeros(null)));
        }

        [TestMethod]
        public void Test_NonceGeneration()
        {
            string nonce = NonceUtility.GenerateNonce(4, "action");
            Assert.IsNotNull(nonce);
            Assert.IsFalse(NonceUtility.IsNonceValid(nonce, 5, "action"));
            Assert.IsFalse(NonceUtility.IsNonceValid(nonce, 4, "action2"));
            Assert.IsFalse(NonceUtility.IsNonceValid(nonce + "2", 4, "action"));
            Assert.IsTrue(NonceUtility.IsNonceValid(nonce, 4, "action"));
            
            // Nonce is only valid once, so it should be invalid now
            Assert.IsFalse(NonceUtility.IsNonceValid(nonce, 4, "action"));
        }

        /// <summary>
        /// Tests the format resource names method for null Old Resource.
        /// </summary>
        [TestMethod]
        public void Test_FormatResourceNames_NullOldResource()
        {
            string resourceName = "r1";
            string oldResource = null;
            string formattedName = Utilities.FormatResourceNames(resourceName, oldResource, false);

            Assert.AreEqual("r1", formattedName);
        }

        /// <summary>
        /// Tests the format resource names method for null Old Resource.
        /// </summary>
        [TestMethod]
        public void Test_FormatResourceNames()
        {
            string resourceName = "r1";
            string oldResource = "aa";
            string formattedName = Utilities.FormatResourceNames(resourceName, oldResource, false);

            Assert.AreEqual("r1 (AA)", formattedName);
        }

        /// <summary>
        /// Tests the format resource names method for null Old Resource.
        /// </summary>
        [TestMethod]
        public void Test_FormatResourceNames_Sub()
        {
            string resourceName = "r1";
            string oldResource = "aa";
            string formattedName = Utilities.FormatResourceNames(resourceName, oldResource, true);

            Assert.AreEqual("r1", formattedName);
        }

        /// <summary>
        /// Tests that CleanFileName successfully removes invalid characters and converts spaces to underscores
        /// </summary>
        [TestMethod]
        public void Test_CleanFileName()
        {
            string testString = "Test? st/ri*ng with: in\"va><lid c|ha\\r.acters";
            string validString = "Test_string_with_invalid_char.acters";

            string result = Utilities.CleanFileName(testString);

            Assert.AreEqual(validString, result);
        }
    }
}