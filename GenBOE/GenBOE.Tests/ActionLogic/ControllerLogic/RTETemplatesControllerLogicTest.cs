// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using DocumentFormat.OpenXml.Vml.Spreadsheet;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using IES.Common.PickList;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RTETemplatesControllerLogicTest
    {
        private Mock<IWorkspaceVersionMetaDataDTODataLoader> versionLoader;
        private Mock<IRteTemplateDataLoader> rteTemplateDataLoader;
        private Mock<IRetriever> retriever;

        public RTETemplatesControllerLogic CreateSUT()
        {
            this.versionLoader = new Mock<IWorkspaceVersionMetaDataDTODataLoader>();
            this.rteTemplateDataLoader = new Mock<IRteTemplateDataLoader>();
            this.retriever = new Mock<IRetriever>();
            Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
            Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
            Mock<IPermissionsDTODataLoader> permissionDataLoader = new Mock<IPermissionsDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionDataLoader.Object);

            return new RTETemplatesControllerLogic(this.rteTemplateDataLoader.Object, this.versionLoader.Object);
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
                    AssignedList = new List<int>() { 1 },
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
                    AssignedList = new List<int>() { 1 },
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
                    AssignedList = new List<int>() { 1, 2 },
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
                    AssignedList = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
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
                    AssignedList = new List<int>() { 1, 2 },
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
                    AssignedList = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), ws.Id), Times.Once());
        }

        /// <summary>
        /// Assigning an in-use template, when WS is not in initialization; changing assignment, so the same number of places is assigned
        /// 
        /// Template should be saved, backup should be executed.
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
                    AssignedList = new List<int>() { 2 },
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
                    AssignedList = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), ws.Id), Times.Once());
        }

        /// <summary>
        /// Assigning an in-use template, when WS is not in initialization. Multiple Templates, mix of changed & unchanged
        /// 
        /// Template should be saved, backup should be executed.
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
                    AssignedList = new List<int>() { 1, 2 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } },
                    Updateable = UpdateType.Upsert
                },
                new RteCustomTemplateModelView()
                {
                    WorkspaceId = ws.Id,
                    Id = 77,
                    AssignedList = new List<int>() { },
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
                    AssignedList = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                },
                new RteCustomTemplateModelView()
                {
                    WorkspaceId = ws.Id,
                    Id = 77,
                    AssignedList = new List<int>() { },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 55, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), ws.Id), Times.Once());
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
                    AssignedList = new List<int>() { 1 },
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
                    AssignedList = new List<int>() { 1, 2 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
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
                    AssignedList = new List<int>() { 1 },
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
                    AssignedList = new List<int>() { 1, 2 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
        }

        /// <summary>
        /// Unassigning an in-use template, when WS is not in initialization
        /// 
        /// Template should be saved, backup should be executed.
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
                    AssignedList = new List<int>() { 1 },
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
                    AssignedList = new List<int>() { 1, 2 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Once());
        }

        /// <summary>
        /// Deleting an in-use template, with an assignment, when WS is not in initialization
        /// 
        /// Template should be saved, backup should be executed.
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
                    AssignedList = new List<int>() { 1 },
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
                    AssignedList = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Once());
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
                    AssignedList = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { },
                    Updateable = UpdateType.Deleted
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    AssignedList = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
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
                    AssignedList = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { },
                    Updateable = UpdateType.Deleted
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    AssignedList = new List<int>() { 1 },
                    InUse = false,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Never());
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
                    AssignedList = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { },
                    Updateable = UpdateType.Deleted
                }
            };
            ICollection<RteCustomTemplateModelView> templatesFromDb = new List<RteCustomTemplateModelView>()
            {
                new RteCustomTemplateModelView()
                {
                    Id = 22,
                    AssignedList = new List<int>() { 1 },
                    InUse = true,
                    Questions = new List<RteCustomTemplateQuestionModelView>() { new RteCustomTemplateQuestionModelView() { Id = 44, Updateable = UpdateType.Upsert } }
                }
            };

            #endregion

            this.SetupMockObjectsAndRunSaveTest(sut, ws, templatesToSave, templatesFromDb);

            this.rteTemplateDataLoader.Verify(x => x.Save(templatesToSave), Times.Once());
            this.versionLoader.Verify(x => x.Upsert(It.IsAny<WorkspaceVersionMetaDataDTO>(), It.IsAny<int>()), Times.Once());
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

            sut.SaveTemplates(templatesToSave, ws);

        }
    }
}
