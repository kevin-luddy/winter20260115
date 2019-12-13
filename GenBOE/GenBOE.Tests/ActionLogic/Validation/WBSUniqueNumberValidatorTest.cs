// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WBSUniqueNumberValidatorTest
    {
        [TestMethod]
        public void WBSUniqueNumberValidator_IsValid()
        {
            var wbsLoader = new Mock<IWbsDTODataLoader>();
            WBSUniqueNumberValidator sut = new WBSUniqueNumberValidator(wbsLoader.Object);

            wbsLoader.Setup(x => x.IsWbsNumberUnique("1", 1, -1)).Returns(false);
            wbsLoader.Setup(x => x.IsWbsNumberUnique("1", 1, 1)).Returns(true);
            wbsLoader.Setup(x => x.IsWbsNumberUnique("1", 1, null)).Returns(true);
            wbsLoader.Setup(x => x.IsWbsNumberUnique("2", 1, null)).Returns(false);

            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"WorkspaceID", "1"},
                {"WbsID", "1"}
            });
            bool returnValue = sut.isValid("1", validationDictionary);
            Assert.IsTrue(returnValue);

            validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"WorkspaceID", "1"},
                {"WbsID", "-1"}
            });

            returnValue = sut.isValid("1", validationDictionary);
            Assert.IsFalse(returnValue);

            validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"WorkspaceID", "1"}
            });

            returnValue = sut.isValid("1", validationDictionary);
            Assert.IsTrue(returnValue);

            validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"WorkspaceID", "1"}
            });

            returnValue = sut.isValid("2", validationDictionary);
            Assert.IsFalse(returnValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WBSUniqueNumberValidator_NullValueTest()
        {
            var wbsLoader = new Mock<IWbsDTODataLoader>();
            WBSUniqueNumberValidator sut = new WBSUniqueNumberValidator(wbsLoader.Object);

            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"WorkspaceID", "1"},
                {"WbsID", "1"}
            });

            Collection<string> returnValue = sut.validation("one", (Collection<Dictionary<string, string>>)null);
            Assert.IsTrue(returnValue.Contains("Invalid data"));

            sut.validation(null, validationDictionary);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void WBSUniqueNumberValidator_InvalidCastTest()
        {
            var wbsLoader = new Mock<IWbsDTODataLoader>();
            WBSUniqueNumberValidator sut = new WBSUniqueNumberValidator(wbsLoader.Object);

            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"WorkspaceID", "1"},
                {"WbsID", "1"}
            });
            sut.validation(validationDictionary, validationDictionary);
        }
    }
}
