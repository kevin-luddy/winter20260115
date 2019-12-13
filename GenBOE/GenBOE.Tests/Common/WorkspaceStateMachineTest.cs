// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.Common
{
    using System;
    using GenBOE.ActionLogic.WorkspaceTransitions;
    using GenBOE.Objects;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WorkspaceStateMachineTest
    {
        private Mock<IFullObjectFactory> factory;

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidTransition()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            factory = new Mock<IFullObjectFactory>();
            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Closed, WorkspaceState.Working);
        }

        [TestMethod]
        public void NoChangeTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Initialization, WorkspaceState.Initialization);
        }


        [TestMethod]
        public void InitializationToWorkingTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Initialization, WorkspaceState.Working);
        }

        [TestMethod]
        public void WorkingToInitializationTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Working, WorkspaceState.Initialization);
        }

        [TestMethod]
        public void InitializationToClosedTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Initialization, WorkspaceState.Closed);
        }

        [TestMethod]
        public void ClosedToInitializationTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Closed, WorkspaceState.Initialization);
        }

        [TestMethod]
        public void WorkingToLockedTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Working, WorkspaceState.Locked);
        }

        [TestMethod]
        public void LockedToWorkingTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Locked, WorkspaceState.Working);
        }

        [TestMethod]
        public void WorkingToClosedTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Working, WorkspaceState.Closed);
        }

        [TestMethod]
        public void LockedToCompleteTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Locked, WorkspaceState.Complete);
        }

        [TestMethod]
        public void CompleteToLockedTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Complete, WorkspaceState.Locked);
        }

        [TestMethod]
        public void LockedToClosedTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Locked, WorkspaceState.Closed);
        }

        [TestMethod]
        public void CompleteToWorkingTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.Complete, WorkspaceState.Working);
        }

        [TestMethod]
        public void NoneToInitializationTest()
        {
            var emptyTransition = new Mock<IWorkspaceStateTransition>();
            this.factory = new Mock<IFullObjectFactory>();

            WorkspaceStateMachine sut = new WorkspaceStateMachine(emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object,
                                                                    emptyTransition.Object, emptyTransition.Object, emptyTransition.Object);

            sut.PerformStateTransitionAction(this.factory.Object.CreateFullWorkspace(1), WorkspaceState.None, WorkspaceState.Initialization);
        }

    }

}
