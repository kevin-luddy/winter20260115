// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests
{
    using System;
    using System.Collections.Generic;
    using GenTRAC.ActionLogic.Validation;
    using GenTRAC.Objects;
    using IES.Common;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests for the Validation Object
    /// </summary>
    [TestClass]
    public class ValidationMethodsTest
    {
        /// <summary>
        /// Mock AD Utilities
        /// </summary>
        private Mock<IActiveDirectoryUtilities> adUtilities = null;

        /// <summary>
        /// Mock wbs mapper
        /// </summary>
        private Mock<IRetriever> retriever = null;

        /// <summary>
        /// Creates the system under test
        /// </summary>
        /// <returns>Validation object</returns>
        private ValidationMethods CreateSystem()
        {
            this.adUtilities = new Mock<IActiveDirectoryUtilities>();
            this.retriever = new Mock<IRetriever>();

            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);

            return new ValidationMethods(this.adUtilities.Object);
        }

        /// <summary>
        /// Tests start and end date 
        /// </summary>
        [TestMethod]
        public void IsStartEndDateValidTest()
        {
            var sut = this.CreateSystem();

            bool validWhenNull = true;
            bool canBeEqual = false;
            bool validWhenOneIsNullAndTheOtherIsNot = true;

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
                { new Tuple<string, string>(string.Empty, string.Empty), true },
                { new Tuple<string, string>(null, null), true },
                { new Tuple<string, string>("06/2011", "06/2010"), false }
            };

            string dateFormat = "MM/yyyy";

            foreach (KeyValuePair<Tuple<string, string>, bool> testDates in expectedResults)
            {
                string errorMessage;
                bool result = sut.IsStartEndDateValid(testDates.Key.Item1, null, testDates.Key.Item2, null, dateFormat, validWhenNull, canBeEqual, validWhenOneIsNullAndTheOtherIsNot, out errorMessage);
                Assert.AreEqual(testDates.Value, result);
            }

            validWhenNull = false;
            canBeEqual = true;

            expectedResults = new Dictionary<Tuple<string, string>, bool>()
            {
                { new Tuple<string, string>("12/2011", "12/2011"), true },
                { new Tuple<string, string>(null, "01/2012"), true },
                { new Tuple<string, string>("12/2011", null), true },
                { new Tuple<string, string>(string.Empty, "01/2012"), true },
                { new Tuple<string, string>(string.Empty, string.Empty), false },
                { new Tuple<string, string>(null, null), false },
                { new Tuple<string, string>("12/2011", string.Empty), true }
            };

            foreach (KeyValuePair<Tuple<string, string>, bool> testDates in expectedResults)
            {
                string errorMessage;
                bool result = sut.IsStartEndDateValid(testDates.Key.Item1, null, testDates.Key.Item2, null, dateFormat, validWhenNull, canBeEqual, validWhenOneIsNullAndTheOtherIsNot, out errorMessage);
                Assert.AreEqual(testDates.Value, result);
            }
        }

        /// <summary>
        /// Tests user type
        /// </summary>
        [TestMethod]
        public void IsUserTypeValidTest()
        {
            var sut = this.CreateSystem();

            bool validWhenNull = true;
            UserType requiredType = UserType.NotSet;

            this.adUtilities.Setup(x => x.IsValidADGroup("jbasilio")).Returns(false);
            this.adUtilities.Setup(x => x.IsValidADGroup("ipe.agile.eti.dev")).Returns(true);
            this.adUtilities.Setup(x => x.GetUserByQualifiedAccount("jbasilio", false)).Returns(new UserData());

            Assert.IsTrue(sut.IsUserTypeValid("jbasilio", requiredType, validWhenNull, false, false));
            Assert.IsTrue(sut.IsUserTypeValid("ipe.agile.eti.dev", requiredType, validWhenNull, false, false));
            Assert.IsFalse(sut.IsUserTypeValid("invalidntid", requiredType, validWhenNull, false, false));
            Assert.AreEqual(validWhenNull, sut.IsUserTypeValid(string.Empty, requiredType, validWhenNull, false, false));
            Assert.AreEqual(validWhenNull, sut.IsUserTypeValid(null, requiredType, validWhenNull, false, false));

            requiredType = UserType.User;
            Assert.IsTrue(sut.IsUserTypeValid("jbasilio", requiredType, validWhenNull, false, false));
            Assert.IsFalse(sut.IsUserTypeValid("ipe.agile.eti.dev", requiredType, validWhenNull, false, false));
            Assert.IsFalse(sut.IsUserTypeValid("invalidntid", requiredType, validWhenNull, false, false));

            requiredType = UserType.Group;
            Assert.IsFalse(sut.IsUserTypeValid("jbasilio", requiredType, validWhenNull, false, false));
            Assert.IsTrue(sut.IsUserTypeValid("ipe.agile.eti.dev", requiredType, validWhenNull, false, false));
            Assert.IsFalse(sut.IsUserTypeValid("invalidntid", requiredType, validWhenNull, false, false));
        }

        /// <summary>
        /// Tests Number Validation
        /// </summary>
        [TestMethod]
        public void ValidateMoney_Test()
        {
            ValidationMessage result = ValidationMethods.ValidateMoneyItem("hello", "a", 2, true, null, null);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem("$100.1", "a", 5, true, null, null);
            Assert.IsNull(result);

            result = ValidationMethods.ValidateMoneyItem("$100.3", "a", 2, true, null, null);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem("$100.1", "a", 5, false, null, null);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem("$100.3", "a", 2, false, null, null);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem("$100.1", "a", 5, true, null, 200);
            Assert.IsNull(result);

            result = ValidationMethods.ValidateMoneyItem("$100.3", "a", 2, true, null, 100);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem("$100.1", "a", 5, true, 100, null);
            Assert.IsNull(result);

            result = ValidationMethods.ValidateMoneyItem("$100.3", "a", 2, true, 101, null);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem(string.Empty, "a", 5, true, null, null);
            Assert.IsNull(result);

            result = ValidationMethods.ValidateMoneyItem("$10.0.0", "a", 5, true, null, null);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem("$100.3", "a", 5, true, null, 100);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem("$100.3", "a", 5, true, 200, null);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem("$150", "a", 5, false, null, 100);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem("$100", "a", 5, false, 200, null);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem("$100.1", "a", 5, false, 200, null);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem(long.MaxValue.ToString(), "a", 100, false, null, 100);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem(long.MinValue.ToString(), "a", 100, false, 200, null);
            Assert.IsNotNull(result);

            result = ValidationMethods.ValidateMoneyItem("-", "a", 100, false, null, 100);
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests start and end date 
        /// </summary>
        [TestMethod]
        public void IsStartEndDateValidTest_StartDateNullEndDateWithValue()
        {
            var sut = this.CreateSystem();

            bool validWhenNull = true;
            bool canBeEqual = true;
            bool validWhenOneIsNullAndTheOtherIsNot = false;

            Dictionary<Tuple<string, string>, bool> expectedResults = new Dictionary<Tuple<string, string>, bool>()
            {
                { new Tuple<string, string>("01/01/2011", "12/01/2011"), true },
                { new Tuple<string, string>("11/01/2011", "12/01/2011"), true },
                { new Tuple<string, string>("12/01/2011", "01/01/2012"), true },
                { new Tuple<string, string>(null, "01/01/2012"), false },
                { new Tuple<string, string>("12/01/2011", null), false },
                { new Tuple<string, string>(string.Empty, "01/01/2012"), false },
                { new Tuple<string, string>("12/01/2011", string.Empty), false },
                { new Tuple<string, string>("12/01/2011", "12/01/2011"), true },
                { new Tuple<string, string>("12/01/2011", "11/01/2011"), false },
                { new Tuple<string, string>("01/01/2011", "12/01/2010"), false },
                { new Tuple<string, string>("06/01/2011", "06/01/2010"), false }
            };

            string dateFormat = "MM/dd/yyyy";

            foreach (KeyValuePair<Tuple<string, string>, bool> testDates in expectedResults)
            {
                string errorMessage;
                bool result = sut.IsStartEndDateValid(testDates.Key.Item1, null, testDates.Key.Item2, null, dateFormat, validWhenNull, canBeEqual, validWhenOneIsNullAndTheOtherIsNot, out errorMessage);
                Assert.AreEqual(testDates.Value, result);
            }
        }
        #region Exception Tests

        /// <summary>
        /// Test Exception invalid date
        /// </summary>
        [TestMethod]
        public void IsStartEndDateValidExceptionTest1()
        {
            var sut = this.CreateSystem();
            string errorMessage;

            Assert.IsFalse(sut.IsStartEndDateValid("15/15/2011", null, "12/2011", null, null, true, false, true, out errorMessage));
        }

        /// <summary>
        /// Test Exception invalid type
        /// </summary>
        [TestMethod]
        public void IsStartEndDateValidExceptionTest2()
        {
            var sut = this.CreateSystem();
            string errorMessage;
            Assert.IsFalse(sut.IsStartEndDateValid(new List<int>(), null, "12/2011", null, null, true, false, true, out errorMessage));
        }

        /// <summary>
        /// Test Exception invalid date
        /// </summary>
        [TestMethod]
        public void IsStartEndDateValidExceptionTest3()
        {
            string errorMessage;
            var sut = this.CreateSystem();
            Assert.IsFalse(sut.IsStartEndDateValid("01/2011", null, "15/15/2011", null, null, true, false, true, out errorMessage));
        }

        /// <summary>
        /// Test Exception invalid type
        /// </summary>
        [TestMethod]
        public void IsStartEndDateValidExceptionTest4()
        {
            var sut = this.CreateSystem();
            string errorMessage;
            Assert.IsFalse(sut.IsStartEndDateValid("01/2011", null, new List<int>(), null, null, true, false, true, out errorMessage));
        }

        #endregion Exception Tests
    }
}
