// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WorkspaceVariableUniqueNameValidatorTest
    {
        int boeid = 99;
        int wsid = 100;

        [TestMethod]
        public void WorkspaceVariableUniqueNameValidator_IsValid()
        {

            var factory = new Mock<IFullObjectFactory>();
            var retriever = new Mock<IRetriever>();
            var dataMapper = new Mock<ICommonDataMapper>();
            var permissions = new Mock<IPermissionsDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), dataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissions.Object);

            Collection<BoeDTO> boes = new Collection<BoeDTO>();
            BoeDTO boe1 = new BoeDTO { Id = boeid, WorkspaceID = wsid };
            boes.Add(boe1);

            BoeTaskElementDTO taskElement1 = new BoeTaskElementDTO { BoeID = boe1.Id, Id = 1, OrdinaryVariables = new Collection<OrdinaryVariableDto> {new OrdinaryVariableDto{Id=1, OrdinaryVariableName="Testy", OrdinaryVariableValue=25m} } };
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(wsid, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { taskElement1 });

            FullWorkspace ws = new FullWorkspace() { Id = wsid };
            factory.Setup(x => x.CreateFullWorkspace(wsid)).Returns(ws);
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(wsid)).Returns(new Collection<FullBoe>() { new FullBoe(boe1) });

            ICollection<WorkspaceVariableDTO> workspaceVariables = new Collection<WorkspaceVariableDTO>()
            {
                new WorkspaceVariableDTO { Id = 1, WorkspaceVariableName = "Bags", WorkspaceVariableValue = 25 }
            };
            retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(wsid)).Returns(workspaceVariables);

            WorkspaceVariableUniqueNameValidator sut = new WorkspaceVariableUniqueNameValidator();
            Collection<ValidationMessage> errors = sut.Validate("Test1", workspaceVariables, ws);
            Assert.IsTrue(!errors.Any(), "Test1 failed"); // will pass;not used yet

            errors = sut.Validate("Testy", workspaceVariables, ws);
            Assert.IsTrue(errors.Any(), "Testy worked");  // should fail because it's in use by a task variable

            errors = sut.Validate("TGIF", workspaceVariables, ws);
            Assert.IsTrue(!errors.Any(), "TGIF failed"); // will pass; not used yet

        }
    }
}
