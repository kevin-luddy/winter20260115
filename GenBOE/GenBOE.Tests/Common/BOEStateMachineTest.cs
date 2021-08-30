// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.Common
{
    using System;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOEStateMachineTest : MOQObject
    {
        private Mock<ICommonDataMapper> _CommonDataMapper = null;
        private Mock<IRetriever> _Retriever = null;
        private Mock<IFullObjectFactory> _Factory = null;

        [TestInitialize]
        new public void Setup()
        {
            _CommonDataMapper = new Mock<ICommonDataMapper>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);

            _Retriever = new Mock<IRetriever>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _Retriever.Object);

            _Factory = new Mock<IFullObjectFactory>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), _Factory.Object);
        }

        private Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidTransition()
        {
            var permissionLoader = new Mock<IPermissionsDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);

            var emptyTransition = new Mock<IBOEStateTransition>();
            BOEStateMachine sut = new BOEStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object, null);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullBoe(1), new FullWorkspace(this.Workspace), BOEState.AwaitingApproval, BOEState.None);
        }

        [TestMethod]
        public void NoChangeTest()
        {
            var permissionLoader = new Mock<IPermissionsDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);

            var emptyTransition = new Mock<IBOEStateTransition>();
            BOEStateMachine sut = new BOEStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object, null);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullBoe(1), new FullWorkspace(this.Workspace), BOEState.Draft, BOEState.Draft);
        }


        [TestMethod]
        public void NoneToUnassignedTest()
        {
            var permissionLoader = new Mock<IPermissionsDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);

            var emptyTransition = new Mock<IBOEStateTransition>();
            BOEStateMachine sut = new BOEStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object, null);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullBoe(1), new FullWorkspace(this.Workspace), BOEState.None, BOEState.Unassigned);
        }

        [TestMethod]
        public void UnassignedToDraftTest()
        {
            var permissionLoader = new Mock<IPermissionsDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);

            var emptyTransition = new Mock<IBOEStateTransition>();
            BOEStateMachine sut = new BOEStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object, null);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullBoe(1), new FullWorkspace(this.Workspace), BOEState.Unassigned, BOEState.Draft);
        }

        [TestMethod]
        public void DraftToAwaitingApprovalTest()
        {
            var permissionLoader = new Mock<IPermissionsDTODataLoader>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
            var emptyTransition = new Mock<IBOEStateTransition>();
            BOEStateMachine sut = new BOEStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object, null);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullBoe(1), new FullWorkspace(this.Workspace), BOEState.Draft, BOEState.AwaitingApproval);
        }

        [TestMethod]
        public void AwaitingApprovalToDraftTest()
        {
            var emptyTransition = new Mock<IBOEStateTransition>();
            BOEStateMachine sut = new BOEStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object, null);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullBoe(1), new FullWorkspace(this.Workspace), BOEState.AwaitingApproval, BOEState.Draft);
        }

        [TestMethod]
        public void AwaitingApprovalToApprovedTest()
        {
            var emptyTransition = new Mock<IBOEStateTransition>();
            BOEStateMachine sut = new BOEStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object, null);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullBoe(1), new FullWorkspace(this.Workspace), BOEState.AwaitingApproval, BOEState.Approved);
        }

        [TestMethod]
        public void ApprovedToDraftTest()
        {
            var emptyTransition = new Mock<IBOEStateTransition>();
            BOEStateMachine sut = new BOEStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                emptyTransition.Object, emptyTransition.Object, emptyTransition.Object, null);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullBoe(1), new FullWorkspace(this.Workspace), BOEState.Approved, BOEState.Draft);
        }
    }

}
