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
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
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

        public RTETemplatesControllerLogic CreateSUT()
        {
            this.versionLoader = new Mock<IWorkspaceVersionMetaDataDTODataLoader>();
            this.rteTemplateDataLoader = new Mock<IRteTemplateDataLoader>();
            this.retriever = new Mock<IRetriever>();
            this.boeDtoDataLoader = new Mock<IBoeDTODataLoader>();
            this.boeMediator = new Mock<IBoeMediator>();
            this.taskElementDtoDataLoader = new Mock<IBoeTaskElementDTODataLoader>();
            this.taskElementMediator = new Mock<IBoeTaskElementMediator>();

        Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            Mock<IPermissionsDTODataLoader> permissionDataLoader = new Mock<IPermissionsDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionDataLoader.Object);


            return new RTETemplatesControllerLogic(this.rteTemplateDataLoader.Object, this.versionLoader.Object, this.boeDtoDataLoader.Object, this.boeMediator.Object, 
                this.taskElementDtoDataLoader.Object, this.taskElementMediator.Object);
        }

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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), ws.Id), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), ws.Id), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Once());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), ws.Id), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Once());
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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), ws.Id), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
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

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Once());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Once());
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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Once());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Once());
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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
        }

        /// <summary>
        /// Deleting an in-use template's prompt, when WS is not in initialization
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
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Once());
            this.boeMediator.Verify(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>()), Times.Never());
            this.taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>()), Times.Never());
        }

        /// <summary>
        /// Code that repeats a lot.. It setups the mock objects & kicks off the Save Template test
        /// </summary>
        private void SetupMockObjectsAndRunSaveTest(RTETemplatesControllerLogic sut, FullWorkspace ws, ICollection<RteCustomTemplateModelView> templatesToSave, ICollection<RteCustomTemplateModelView> templatesFromDb)
        {
            this.rteTemplateDataLoader.Setup(x => x.Save(templatesToSave)).Verifiable();
            this.retriever.Setup(x => x.GetCurrentActiveUser()).Returns(new UserDTO() { UserID = 333 });
            this.versionLoader.Setup(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>())).Verifiable();
            this.rteTemplateDataLoader.Setup(x => x.GetTemplates(ws.Id)).Returns(templatesFromDb);
            this.boeMediator.Setup(x => x.SaveEditBoeHeader(It.IsAny<BoeDTO>())).Verifiable();
            this.taskElementMediator.Setup(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>())).Verifiable();

            ICollection<BoeDTO> boes = new Collection<BoeDTO>() { new BoeDTO() { Id = 1 } };
            this.boeDtoDataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>())).Returns(boes);

            ICollection<BoeTaskElementDTO> tasks = new Collection<BoeTaskElementDTO>() { new BoeTaskElementDTO() { Id = 1, BoeID = 1 } };
            this.taskElementDtoDataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>(), false, It.IsAny<int>(), It.IsAny<int>())).Returns(tasks);

            ICollection<RTECustomTemplateQuestionAnswerModelView> questionAnswerMV = new Collection<RTECustomTemplateQuestionAnswerModelView>()
            {
                new RTECustomTemplateQuestionAnswerModelView() { QuestionText = "Q", AnswerText = "A" }
            };
            this.rteTemplateDataLoader.Setup(x => x.GetByBoeId(It.IsAny<int>(), It.IsAny<int>())).Returns(questionAnswerMV);
            this.rteTemplateDataLoader.Setup(x => x.GetByBoeIdAndTaskId(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(questionAnswerMV);

            sut.SaveTemplates(templatesToSave, ws);
        }
    }
}
