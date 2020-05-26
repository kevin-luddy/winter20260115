// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RTETemplatesControllerLogicTest
    {
        private Mock<IWorkspaceVersionMetaDataDTODataLoader> versionLoader;
        private Mock<IRteTemplateDataLoader> rteTemplateDataLoader;
        private Mock<IBoeDTODataLoader> boeDtoDataLoader;
        private Mock<IBoeMediator> boeMediator;
        private Mock<IBoeTaskElementDTODataLoader> taskElementDtoDataLoader;
        private Mock<IBoeTaskElementMediator> taskElementMediator;
        private Mock<IRetriever> retriever;
        private Mock<IBoeEmailer> emailer;
        private Mock<IBOEStateMachine> stateMachine;

        /// <summary>
        /// Creates System Under Test
        /// </summary>
        public RTETemplatesControllerLogic CreateSUT()
        {
            this.versionLoader = new Mock<IWorkspaceVersionMetaDataDTODataLoader>();
            this.rteTemplateDataLoader = new Mock<IRteTemplateDataLoader>();
            this.retriever = new Mock<IRetriever>();
            this.boeDtoDataLoader = new Mock<IBoeDTODataLoader>();
            this.boeMediator = new Mock<IBoeMediator>();
            this.taskElementDtoDataLoader = new Mock<IBoeTaskElementDTODataLoader>();
            this.taskElementMediator = new Mock<IBoeTaskElementMediator>();
            this.emailer = new Mock<IBoeEmailer>();
            this.stateMachine = new Mock<IBOEStateMachine>();

            Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            Mock<IPermissionsDTODataLoader> permissionDataLoader = new Mock<IPermissionsDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionDataLoader.Object);

            return new RTETemplatesControllerLogic(this.rteTemplateDataLoader.Object, this.versionLoader.Object, this.boeDtoDataLoader.Object, this.boeMediator.Object, 
                this.taskElementDtoDataLoader.Object, this.taskElementMediator.Object, this.emailer.Object, this.stateMachine.Object);
        }

        #region SaveTemplates

        /// <summary>
        /// Creating a new template, when WS is in initialization
        /// 
        /// Template should be saved, no backup should be executed.
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test1()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Initialization };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Never());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());
        }

        /// <summary>
        /// Creating a new template, when WS is not in initialization
        /// 
        /// Template should be saved, no backup should be executed.
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test2()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Never());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());
        }

        /// <summary>
        /// Assigning a template, when WS is in initialization
        /// 
        /// Template should be saved, no backup should be executed.
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test3()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Initialization };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1, 2 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Never());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());
        }

        /// <summary>
        /// Assigning an in-use template, when WS is not in initialization
        /// 
        /// Template should be saved, backup should be executed.
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test4()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    WorkspaceId = ws.Id,
                    Id = 22,
                    Assigned = new List<int>() { 1, 2 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    WorkspaceId = ws.Id,
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), ws.Id), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Once());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Once());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());
        }

        /// <summary>
        /// Assigning an in-use template, when WS is not in initialization; changing assignment, so the same number of places is assigned
        /// 
        /// Template should be saved, backup should be executed.
        /// A BOE Level Assignment is removed, so BOEs should be updated
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test5()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    WorkspaceId = ws.Id,
                    Id = 22,
                    Assigned = new List<int>() { 2 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    WorkspaceId = ws.Id,
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), ws.Id), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Once());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Once());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Once());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Once());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());
        }

        /// <summary>
        /// Assigning an in-use template, when WS is not in initialization; changing assignment, so the same number of places is assigned
        /// 
        /// Template should be saved, backup should be executed.
        /// A Task Level Assignment is removed, so Tasks should be updated
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test6()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    WorkspaceId = ws.Id,
                    Id = 22,
                    Assigned = new List<int>() { 4 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    WorkspaceId = ws.Id,
                    Id = 22,
                    Assigned = new List<int>() { 3 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), ws.Id), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Once());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Once());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Once());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());
        }

        /// <summary>
        /// Assigning an in-use template, when WS is not in initialization. Multiple Templates, mix of changed & unchanged
        /// 
        /// Template should be saved, backup should be executed.
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test7()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    WorkspaceId = ws.Id,
                    Id = 22,
                    Assigned = new List<int>() { 1, 2 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                },
                new RteCustomTemplateModelView()
                {
                    WorkspaceId = ws.Id,
                    Id = 77,
                    Assigned = new List<int>() { },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 55, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    WorkspaceId = ws.Id,
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                },
                new RteCustomTemplateModelView()
                {
                    WorkspaceId = ws.Id,
                    Id = 77,
                    Assigned = new List<int>() { },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 55, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Exactly(2));
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), ws.Id), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Once());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Once());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());
        }

        /// <summary>
        /// Unassigning a template, when WS is in initialization
        /// 
        /// Template should be saved, no backup should be executed.
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test10()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Initialization };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1, 2, 3 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Never());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());
        }

        /// <summary>
        /// Unassigning a not in-use template, when WS is not in initialization
        /// 
        /// Template should be saved, no backup should be executed.
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test11()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1, 2 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Never());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());
        }

        /// <summary>
        /// Unassigning an in-use template, when WS is not in initialization
        /// 
        /// Template should be saved, backup should be executed.
        /// Unassigning both boe and task level sources, boes and tasks should be updated
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test12()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1, 2, 3 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Once());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Once());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Never());
        }

        /// <summary>
        /// Deleting an in-use template, with an assignment, when WS is not in initialization
        /// 
        /// Template should be saved, backup should be executed.
        /// Template had both boe and task level sources, boes and tasks should be updated
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test13()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1, 3 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Deleted
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1, 3 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Never());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Once());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Never());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Once());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());
        }

        /// <summary>
        /// Deleting an in-use template's prompt, when WS is in initialization
        /// 
        /// Template should be saved, backup should not be executed.
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test20()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Initialization };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Deleted }  },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Never());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());
        }

        /// <summary>
        /// Deleting a non in-use template's prompt, when WS is not in initialization
        /// 
        /// Template should be saved, backup should not be executed.
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test21()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Deleted } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Never());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Never());
        }

        /// <summary>
        /// Deleting an in-use template's prompt, when WS is not in initialization, and delete prompt data has been selected
        /// 
        /// Template should be saved, backup should not be executed.
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test22()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Deleted } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Never());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Once());
        }

        /// <summary>
        /// Deleting an in-use template's prompt, when WS is not in initialization, and move prompt data has been selected with an existing prompt selected
        /// 
        /// Template should be saved, backup should not be executed.
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test23()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Deleted }, new RteCustomTemplateQuestionModelView() { Id = 45, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert }, new RteCustomTemplateQuestionModelView() { Id = 45, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb, true, 45);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Once());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Once());
        }

        /// <summary>
        /// Deleting an in-use template's prompt, when WS is not in initialization, and move prompt data has been selected with a new prompt selected
        /// 
        /// Template should be saved, backup should not be executed.
        /// </summary>
        [TestMethod]
        public void SaveTemplates_Test24()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToSave = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Deleted }, new RteCustomTemplateQuestionModelView() { Id = -1, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb, true, -1);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.rteTemplateDataLoader.Verify(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>()), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
            this.rteTemplateDataLoader.Verify(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>()), Times.Once());

            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned), Times.Never());
            this.emailer.Verify(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted), Times.Once());
        }

        /// <summary>
        /// Code that repeats a lot.. It setups the mock objects & kicks off the Save Template test
        /// </summary>
        private void SetupMockObjectsAndRunSaveTest(RTETemplatesControllerLogic sut, FullWorkspace ws, ICollection<RteCustomTemplateModelView> templatesToSave, ICollection<RteCustomTemplateModelView> templatesFromDb,
            bool moveDeletedPromptData = false, int? moveToPrompt = null)
        {
            this.rteTemplateDataLoader.Setup(x => x.Save(templatesToSave)).Verifiable();
            this.rteTemplateDataLoader.Setup(x => x.SaveAnswers(It.IsAny<ICollection<RTECustomTemplateQuestionAnswerModelView>>())).Verifiable();
            this.rteTemplateDataLoader.Setup(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), It.IsAny<int>())).Verifiable();
            this.retriever.Setup(x => x.GetCurrentActiveUser()).Returns(new UserDTO() { UserID = 333 });
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new List<FullBoe>());
            this.versionLoader.Setup(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>())).Verifiable();
            this.rteTemplateDataLoader.Setup(x => x.GetTemplates(ws.Id)).Returns(templatesFromDb);
            this.boeMediator.Setup(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>())).Verifiable();
            this.taskElementMediator.Setup(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>())).Verifiable();

            this.emailer.Setup(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateUnassigned)).Verifiable();
            this.emailer.Setup(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplateAssigned)).Verifiable();
            this.emailer.Setup(x => x.SendRteTemplateEmail(ws, EmailTypes.TemplatePromptDeleted)).Verifiable();

            ICollection<BoeDTO> boes = new Collection<BoeDTO>() { new BoeDTO() { Id = 1 } };
            this.boeDtoDataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>())).Returns(boes);

            ICollection<BoeTaskElementDTO> tasks = new Collection<BoeTaskElementDTO>() { new BoeTaskElementDTO() { Id = 1, BoeID = 1 } };
            this.taskElementDtoDataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(tasks);

            ICollection<RTECustomTemplateQuestionAnswerModelView> questionAnswerMV = new Collection<RTECustomTemplateQuestionAnswerModelView>()
            {
                new RTECustomTemplateQuestionAnswerModelView() { QuestionText = "Q", AnswerText = "A", SourceId = 1, QuestionId = 44 },
                new RTECustomTemplateQuestionAnswerModelView() { QuestionText = "Q", AnswerText = "A", SourceId = 1, QuestionId = 45 },
                new RTECustomTemplateQuestionAnswerModelView() { QuestionText = "Q", AnswerText = "A", SourceId = 2 },
                new RTECustomTemplateQuestionAnswerModelView() { QuestionText = "Q", AnswerText = "A", SourceId = 3 },
                new RTECustomTemplateQuestionAnswerModelView() { QuestionText = "Q", AnswerText = "A", SourceId = 4 }
            };
            this.rteTemplateDataLoader.Setup(x => x.GetByBoeId(It.IsAny<int>(), It.IsAny<int>())).Returns(questionAnswerMV);
            this.rteTemplateDataLoader.Setup(x => x.GetByBoeIdAndTaskId(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(questionAnswerMV);

            
            foreach(RteCustomTemplateModelView template in templatesToSave)
            {
                Dictionary<int, int> questionDictionary = new Dictionary<int, int>();
                foreach (RteCustomTemplateQuestionModelView question in template.Questions)
                {
                    if (question.Id < 0)
                    {
                        questionDictionary.Add(question.Id, 45);
                    }
                    else
                    {
                        questionDictionary.Add(question.Id, question.Id);
                    }
                }

                this.rteTemplateDataLoader.Setup(x => x.SaveQuestions(It.IsAny<ICollection<RteCustomTemplateQuestionModelView>>(), template.Id)).Returns(questionDictionary);
            }

            sut.SaveTemplates(templatesToSave, ws, moveDeletedPromptData, moveToPrompt);
        }

        #endregion

        /// <summary>
        /// Tests template description being unique. Pass.
        /// </summary>
        [TestMethod]
        public void Validate_UniqueDescription1()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToValidate = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Updateable = UpdateType.Upsert,
                    Description = "Test Description Pass"
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Description = "Test Description Pass"
                }
            };

            #endregion

            this.SetupValidationAndExecuteTestMock(sut, ws, templatesToValidate, templatesFromDb);
        }

        /// <summary>
        /// Tests template description being unique. Pass, creation.
        /// </summary>
        [TestMethod]
        public void Validate_UniqueDescription2()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToValidate = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Updateable = UpdateType.Upsert,
                    Description = "Test Description Pass"
                },
                new RteCustomTemplateModelView()
                {
                    Id = -1,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Updateable = UpdateType.Upsert,
                    Description = "Test Description Another One"
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Description = "Test Description Pass"
                }
            };

            #endregion

            this.SetupValidationAndExecuteTestMock(sut, ws, templatesToValidate, templatesFromDb);
        }

        /// <summary>
        /// Tests template description being unique. Pass, deletion.
        /// 
        /// This is a weird setup, the goal is to get it to ignore failure in items that are being deleted
        /// </summary>
        [TestMethod]
        public void Validate_UniqueDescription3()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToValidate = new List<RteCustomTemplateModelView>()
            {

                new RteCustomTemplateModelView()
                {
                    Id = -1,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Updateable = UpdateType.Deleted,
                    Description = "Test Description Pass"
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Description = "Test Description Pass"
                }
            };

            #endregion

            this.SetupValidationAndExecuteTestMock(sut, ws, templatesToValidate, templatesFromDb);
        }

        /// <summary>
        /// Tests template description being unique. Failure due to duplication on changing existing
        /// </summary>
        [TestMethod, ExpectedException(typeof(GenValidationException))]
        public void Validate_UniqueDescription4()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToValidate = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Updateable = UpdateType.Upsert,
                    Description = "Test Description Fail"
                },
                new RteCustomTemplateModelView()
                {
                    Id = 23,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Updateable = UpdateType.Upsert,
                    Description = "Test Description Fail"
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Description = "Test Description Fail"
                }
            };

            #endregion

            this.SetupValidationAndExecuteTestMock(sut, ws, templatesToValidate, templatesFromDb);
        }

        /// <summary>
        /// Tests template description being unique. Failure due to duplication on creation
        /// </summary>
        [TestMethod, ExpectedException(typeof(GenValidationException))]
        public void Validate_UniqueDescription5()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToValidate = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Updateable = UpdateType.Upsert,
                    Description = "Test Description Fail"
                },
                new RteCustomTemplateModelView()
                {
                    Id = -1,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Updateable = UpdateType.Upsert,
                    Description = "Test Description Fail"
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Description = "Test Description Fail"
                }
            };

            #endregion

            this.SetupValidationAndExecuteTestMock(sut, ws, templatesToValidate, templatesFromDb);
        }

        /// <summary>
        /// Tests that a template is selected if deleting a prompt and selecting to move the data
        /// </summary>
        [TestMethod, ExpectedException(typeof(GenValidationException))]
        public void Validate_MissingMoveToPrompt()
        {
            RTETemplatesControllerLogic sut = this.CreateSUT();

            #region Data Setup

            FullWorkspace ws = new FullWorkspace() { Id = 1, WorkspaceState = WorkspaceState.Working };
            ICollection<RteCustomTemplateModelView> templatesToValidate = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Updateable = UpdateType.Upsert,
                    Description = "Test Description Pass"
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    Assigned = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert, Text = "hello" } },
                    Description = "Test Description Pass"
                }
            };

            #endregion

            this.SetupValidationAndExecuteTestMock(sut, ws, templatesToValidate, templatesFromDb, true, null);
        }

        /// <summary>
        /// Sets up Mock objects & executes validation test
        /// </summary>
        /// <param name="sut">System Under Test</param>
        /// <param name="ws">Workspace</param>
        /// <param name="templatesToValidate">Templates being validated</param>
        /// <param name="templatesFromDb">Templates in DB</param>
        private void SetupValidationAndExecuteTestMock(RTETemplatesControllerLogic sut, FullWorkspace ws, ICollection<RteCustomTemplateModelView> templatesToValidate, ICollection<RteCustomTemplateModelView> templatesFromDb,
            bool moveDeletedPromptData = false, int? moveToPrompt = null)
        {
            this.rteTemplateDataLoader.Setup(x => x.GetTemplates(ws.Id)).Returns(templatesFromDb);

            sut.ValidateTemplates(templatesToValidate, ws.Id, moveDeletedPromptData, moveToPrompt);
        }
    }
}
