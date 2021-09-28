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
    using System.Linq;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WorkspaceUniqueNameValidatorTest
    {
        [TestMethod]
        public void WorkspaceUniqueNameValidator_IsValid()
        {
            var wsLoader = new Mock<IWorkspaceDTODataLoader>();
            WorkspaceUniqueNameValidator sut = new WorkspaceUniqueNameValidator(wsLoader.Object);

            // setup workspaces
            Collection<WorkspaceDTO> workspaces = new Collection<WorkspaceDTO>();            
            WorkspaceDTO wsOne = new WorkspaceDTO();
            wsOne.WorkspaceName = "Workspace One";
            wsOne.Id = 1;
            workspaces.Add(wsOne);

            // Setup Method
            wsLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(workspaces);

            // Act and Assert
            bool returnValue = sut.isValid("Workspace One", null);
            Assert.IsFalse(returnValue);

            returnValue = sut.isValid("Workspace Two", null);
            Assert.IsTrue(returnValue);

            // Test again excluding "current" workspace
            Collection<Dictionary<string, string>> validationDictionary = new Collection<Dictionary<string, string>>();
            validationDictionary.Add(new Dictionary<string, string>());
            validationDictionary.First<Dictionary<string, string>>().Add("WorkspaceID", "1");
            returnValue = sut.isValid("Workspace One", validationDictionary);
            Assert.IsTrue(returnValue);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void WorkspaceUniqueNameValidator_InvalidParameterTest()
        {
            var wsLoader = new Mock<IWorkspaceDTODataLoader>();
            WorkspaceUniqueNameValidator sut = new WorkspaceUniqueNameValidator(wsLoader.Object);

            decimal[] test = { 3.14m, 6.77m };

            bool returnValue = sut.isValid(test, null);
            Assert.IsFalse(returnValue);

            sut.isValid(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkspaceUniqueNameValidator_NullParameterTest()
        {
            var wsLoader = new Mock<IWorkspaceDTODataLoader>();
            WorkspaceUniqueNameValidator sut = new WorkspaceUniqueNameValidator(wsLoader.Object);
            sut.validation(null, new Collection<Dictionary<string, string>>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkspaceUniqueNameValidator_NullIsValidTest()
        {
            var wsLoader = new Mock<IWorkspaceDTODataLoader>();
            WorkspaceUniqueNameValidator sut = new WorkspaceUniqueNameValidator(wsLoader.Object);
            sut.isValid(null, new Collection<Dictionary<string, string>>());
        }
    }
}
