// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PerformingOrgUniqueIDValidatorTest
    {
        Mock<IPerformingOrgDTODataLoader> perfLoader = new Mock<IPerformingOrgDTODataLoader>();

        [TestMethod]
        public void WBSUniqueNumberValidator_IsValid()
        {
            PerformingOrgUniqueIDValidator sut = new PerformingOrgUniqueIDValidator(perfLoader.Object);

            perfLoader.Setup(x => x.GetByListIdAndName(1, "2")).Returns(new PerformingOrgDTO());
            perfLoader.Setup(x => x.GetByListId(1)).Returns(new Collection<PerformingOrgDTO>() { new PerformingOrgDTO() { Id = 1, PerformingOrgName = "1" }});

            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"PerformingOrgListID", "1"},
                {"PerformingOrgID", "1"}
            });
            bool returnValue = sut.isValid("1", validationDictionary);
            Assert.IsTrue(returnValue);

            validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"PerformingOrgListID", "1"},
                {"PerformingOrgID", "-1"}
            });

            returnValue = sut.isValid("1", validationDictionary);
            Assert.IsFalse(returnValue);

            validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"PerformingOrgListID", "1"}
            });

            returnValue = sut.isValid("1", validationDictionary);
            Assert.IsTrue(returnValue);

            validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"PerformingOrgListID", "1"}
            });

            returnValue = sut.isValid("2", validationDictionary);
            Assert.IsFalse(returnValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WBSUniqueNumberValidator_NullValueTest()
        {
            PerformingOrgUniqueIDValidator sut = new PerformingOrgUniqueIDValidator(perfLoader.Object);

            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"PerformingOrgListID", "1"},
                {"PerformingOrgID", "1"}
            });

            Collection<string> returnValue = sut.validation("one", (Collection<Dictionary<string, string>>)null);
            Assert.IsTrue(returnValue.Contains("Invalid data"));

            sut.validation(null, validationDictionary);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void WBSUniqueNumberValidator_InvalidCastTest()
        {
            PerformingOrgUniqueIDValidator sut = new PerformingOrgUniqueIDValidator(perfLoader.Object);

            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"PerformingOrgListID", "1"},
                {"PerformingOrgID", "1"}
            });
            sut.validation(validationDictionary, validationDictionary);
        }
    }
}
