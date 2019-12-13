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
    using GenBOE.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WorkspaceUniqueShortnameValidatorTest
    {
        [TestMethod]
        public void WorkspaceUniqueShortnameValidator_IsValid()
        {
            var wsLoader = new Mock<IWorkspaceDTODataLoader>();

            WorkspaceUniqueShortnameValidator sut = new WorkspaceUniqueShortnameValidator(wsLoader.Object);

            // setup workspaces
            Collection<WorkspaceDTO> workspaces = new Collection<WorkspaceDTO>();
            
            WorkspaceDTO wsOne = new WorkspaceDTO();
            wsOne.Shortname = "shortname";

            workspaces.Add(wsOne);

            wsLoader.Setup(x => x.GetAllWsNamesAndTrackingNumberInfo()).Returns(workspaces);

            bool returnValue = sut.isValid("shortname", null);
            Assert.IsFalse(returnValue);

            returnValue = sut.isValid("Workspace Two", null);
            Assert.IsTrue(returnValue);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void WorkspaceUniqueShortnameValidator_InvalidParameterTest()
        {
            var wsLoader = new Mock<IWorkspaceDTODataLoader>();
            WorkspaceUniqueShortnameValidator sut = new WorkspaceUniqueShortnameValidator(wsLoader.Object);

            decimal[] test = { 3.14m, 6.77m };
            bool returnValue = sut.isValid(test, null);
            Assert.IsFalse(returnValue);

            sut.isValid(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkspaceUniqueShortnameValidator_NullParameterTest()
        {
            var wsLoader = new Mock<IWorkspaceDTODataLoader>();
            WorkspaceUniqueShortnameValidator sut = new WorkspaceUniqueShortnameValidator(wsLoader.Object);
            sut.validation(null, new Collection<Dictionary<string, string>>());
        }
    }
}
