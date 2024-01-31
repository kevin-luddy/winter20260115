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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ResourceUniqueIDValidatorTest
    {
        [TestMethod]
        public void ResourceUniqueIDValidator_IsValid()
        {
			Mock<IResourceDTODataLoader> ResourceLoader = new Mock<IResourceDTODataLoader>();
            ResourceUniqueIDValidator sut = new ResourceUniqueIDValidator(ResourceLoader.Object);

            ResourceLoader.Setup(x => x.IsResourceNameUnique("1", 1, -1)).Returns(false);
            ResourceLoader.Setup(x => x.IsResourceNameUnique("1", 1, 1)).Returns(true);
            ResourceLoader.Setup(x => x.IsResourceNameUnique("1", 1, null)).Returns(true);
            ResourceLoader.Setup(x => x.IsResourceNameUnique("2", 1, null)).Returns(false);

            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"ResourceListID", "1"},
                {"ResourceID", "1"}
            });
            bool returnValue = sut.isValid("1", validationDictionary);
            Assert.IsTrue(returnValue);

            validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"ResourceListID", "1"},
                {"ResourceID", "-1"}
            });

            returnValue = sut.isValid("1", validationDictionary);
            Assert.IsFalse(returnValue);

            validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"ResourceListID", "1"}
            });

            returnValue = sut.isValid("1", validationDictionary);
            Assert.IsTrue(returnValue);

            validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"ResourceListID", "1"}
            });

            returnValue = sut.isValid("2", validationDictionary);
            Assert.IsFalse(returnValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WBSUniqueNumberValidator_NullValueTest()
        {
			Mock<IResourceDTODataLoader> ResourceLoader = new Mock<IResourceDTODataLoader>();
            ResourceUniqueIDValidator sut = new ResourceUniqueIDValidator(ResourceLoader.Object);

            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"ResourceListID", "1"},
                {"ResourceID", "-1"}
            });

            Collection<string> returnValue = sut.validation("one", (Collection<Dictionary<string, string>>)null);
            Assert.IsTrue(returnValue.Contains("Invalid data"));

            sut.validation(null, validationDictionary);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void WBSUniqueNumberValidator_InvalidCastTest()
        {
			Mock<IResourceDTODataLoader> ResourceLoader = new Mock<IResourceDTODataLoader>();
            ResourceUniqueIDValidator sut = new ResourceUniqueIDValidator(ResourceLoader.Object);

            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>() {
                {"ResourceListID", "1"},
                {"ResourceID", "-1"}
            });
            sut.validation(validationDictionary, validationDictionary);
        }
    }
}
