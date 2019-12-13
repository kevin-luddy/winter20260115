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
    public class ResourceUniqueDescValidatorTest
    {
        [TestMethod]
        public void ResourceUniqueDescValidator_IsValid()
        {
            var ResourceLoader = new Mock<IResourceDTODataLoader>();
            ResourceUniqueDescValidator sut = new ResourceUniqueDescValidator(ResourceLoader.Object);

            ResourceLoader.Setup(x => x.IsResourceDescriptionUnique("1", 1, -1)).Returns(false);
            ResourceLoader.Setup(x => x.IsResourceDescriptionUnique("1", 1, 1)).Returns(true);
            ResourceLoader.Setup(x => x.IsResourceDescriptionUnique("1", 1, null)).Returns(true);
            ResourceLoader.Setup(x => x.IsResourceDescriptionUnique("2", 1, null)).Returns(false);

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
        public void ResourceUniqueDescValidator_NullValueTest()
        {
            var ResourceLoader = new Mock<IResourceDTODataLoader>();
            ResourceUniqueDescValidator sut = new ResourceUniqueDescValidator(ResourceLoader.Object);

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
        public void ResourceUniqueDescValidator_InvalidCastTest()
        {
            var ResourceLoader = new Mock<IResourceDTODataLoader>();
            ResourceUniqueDescValidator sut = new ResourceUniqueDescValidator(ResourceLoader.Object);

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
