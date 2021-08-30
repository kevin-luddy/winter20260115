// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.BOETransitions
{
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DefaultBOETransitionTest : MOQObject
    {
        Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

        [TestMethod]
        public void DefaultWorkspaceTransitionActionTest()
        {
            var emailer = new Mock<IBoeEmailer>();
            var boeApproverLoader = new Mock<IBoeApproverResponseDTODataLoader>();

            DefaultBOETransition sut = new DefaultBOETransition(emailer.Object, boeApproverLoader.Object);
            sut.Action(this.factory.Object.CreateFullBoe(1), new FullWorkspace(this.Workspace), BOEState.None, BOEState.Unassigned);
        }

        [TestMethod]
        public void DefaultWorkspaceTransitionValidationTest()
        {
            Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();   
            var emailer = new Mock<IBoeEmailer>();
            var boeApproverLoader = new Mock<IBoeApproverResponseDTODataLoader>();
            var retriever = new Mock<IRetriever>();
            var permloader = new Mock<IPermissionsDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permloader.Object);

            DefaultBOETransition sut = new DefaultBOETransition(emailer.Object, boeApproverLoader.Object);
            string errorMessage = string.Empty;
            Assert.IsTrue(sut.Validate(this.factory.Object.CreateFullBoe(1), new FullWorkspace(this.Workspace), out errorMessage), "Validation message " + errorMessage);
        }
    }
}
