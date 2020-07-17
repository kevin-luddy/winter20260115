// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.ActionLogic
{
    using GenTRAC.ActionLogic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the URL validator
    /// </summary>
    [TestClass]
    public class UrlValidatorTest
    {
        #region Test IsAbsoluteLockheedUrl

        /// <summary>
        /// Tests the IsAbsoluteLockheedUrl
        /// </summary>
        [TestMethod]
        public void IsAbsoluteLockheedUrlTest_1()
        {
            string toTest = "http://www.google.com";
            bool expectedResult = false;

            bool actual = UrlValidator.IsAbsoluteLockheedUrl(toTest);

            Assert.AreEqual(expectedResult, actual);
        }

        /// <summary>
        /// Tests the IsAbsoluteLockheedUrl
        /// </summary>
        [TestMethod]
        public void IsAbsoluteLockheedUrlTest_2()
        {
            string toTest = "~/helloworld";
            bool expectedResult = false;

            bool actual = UrlValidator.IsAbsoluteLockheedUrl(toTest);

            Assert.AreEqual(expectedResult, actual);
        }

        /// <summary>
        /// Tests the IsAbsoluteLockheedUrl
        /// </summary>
        [TestMethod]
        public void IsAbsoluteLockheedUrlTest_3()
        {
            string toTest = "http://www.google.com/lmco.com";
            bool expectedResult = false;

            bool actual = UrlValidator.IsAbsoluteLockheedUrl(toTest);

            Assert.AreEqual(expectedResult, actual);
        }

        /// <summary>
        /// Tests the IsAbsoluteLockheedUrl
        /// </summary>
        [TestMethod]
        public void IsAbsoluteLockheedUrlTest_4()
        {
            string toTest = "https://isgs-gen.external.lmco.com/sites/Estimating_Init/doclib2/Forms/AllItems.aspx";
            bool expectedResult = true;

            bool actual = UrlValidator.IsAbsoluteLockheedUrl(toTest);

            Assert.AreEqual(expectedResult, actual);
        }

        /// <summary>
        /// Tests the IsAbsoluteLockheedUrl
        /// </summary>
        [TestMethod]
        public void IsAbsoluteLockheedUrlTest_5()
        {
            string toTest = "aaaa";
            bool expectedResult = false;

            bool actual = UrlValidator.IsAbsoluteLockheedUrl(toTest);

            Assert.AreEqual(expectedResult, actual);
        }

        /// <summary>
        /// Tests the IsAbsoluteLockheedUrl
        /// </summary>
        [TestMethod]
        public void IsAbsoluteLockheedUrlTest_6()
        {
            string toTest = null;
            bool expectedResult = false;

            bool actual = UrlValidator.IsAbsoluteLockheedUrl(toTest);

            Assert.AreEqual(expectedResult, actual);
        }

        #endregion

        #region Test DoesPageExist

        /// <summary>
        /// Tests the DoesPageExist
        /// </summary>
        [TestMethod]
        public void DoesPageExist_1()
        {
            string toTest = "http://www.dusanPaliderThisPageDoesNotExistIhope.google.msn.cnn.com";
            bool expectedResult = false;

            bool actual = UrlValidator.DoesPageExist(toTest);

            Assert.AreEqual(expectedResult, actual);
        }

        #endregion
    }
}
