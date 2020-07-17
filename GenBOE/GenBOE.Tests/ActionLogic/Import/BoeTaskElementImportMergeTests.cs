// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BoeTaskElementImportMergeTests
    {
        private BoeTaskElementImportMerge taskElementImportMerge = null;
        private Mock<IFullObjectFactory> fullFactoryMock = null;
        private Mock<IBoeTaskElementDTODataLoader> taskLoaderMock = null;
        private Mock<ICustomFieldValueDTODataLoader> customFieldValueLoader = null;
        private WorkofflineImportedTaskElement workOfflineTask = null;
        private BoeTaskElementDTO expectedResult = null;
        private BoeTaskElementDTO dtoLookupResult = null;

        private const int TASK_ELEMENT_ID = 123;
        private const int BOE_ID = 456;
        private const string TASK_ID = "1.1";
        private const string DESCRIPTION = "Description";
        private DateTime END_TIME = new DateTime(2013, 12, 1).AddDays(15).AddHours(12);
        private DateTime START_TIME = new DateTime(2014, 12, 1).AddDays(15).AddHours(12);
        private Collection<int> WorkspaceVars = new Collection<int>() { 1001, 1002, 1003 };
        private Collection<int> WorkspaceVars_B = new Collection<int>() { 2001, 1002, 2003 };
        private Collection<int> WorkspaceVars_None = new Collection<int>();

        [TestInitialize]
        public void Init()
        {
            fullFactoryMock = new Mock<IFullObjectFactory>();
            taskLoaderMock = new Mock<IBoeTaskElementDTODataLoader>();
            customFieldValueLoader = new Mock<ICustomFieldValueDTODataLoader>();
            workOfflineTask = new WorkofflineImportedTaskElement();
            expectedResult = new BoeTaskElementDTO();
            dtoLookupResult = new BoeTaskElementDTO();

            taskLoaderMock.Setup(s => s.GetById(TASK_ELEMENT_ID, It.IsAny<int>(), It.IsAny<int>())).Returns(dtoLookupResult);
            fullFactoryMock.Setup(s => s.CreateTaskElement(TASK_ELEMENT_ID, It.IsAny<int>(), It.IsAny<int>())).Returns(dtoLookupResult);
            customFieldValueLoader.Setup(s => s.GetTaskElementCustomFieldContainerFieldValueMappings(It.IsAny<int>())).Returns(new List<CustomFieldContainerFieldValueMapping>());

            taskElementImportMerge = new BoeTaskElementImportMerge(fullFactoryMock.Object, customFieldValueLoader.Object);

            //import object setup
            workOfflineTask.Id = TASK_ELEMENT_ID;
            workOfflineTask.BOETaskID = TASK_ID;
            workOfflineTask.BoeID = BOE_ID;
            workOfflineTask.Description = DESCRIPTION;
            workOfflineTask.StartDate = START_TIME;
            workOfflineTask.EndDate = END_TIME;

            //expected result setup
            expectedResult.Id = TASK_ELEMENT_ID;
            expectedResult.BOETaskID = TASK_ID;
            expectedResult.BoeID = BOE_ID;
            expectedResult.Description = DESCRIPTION;
            expectedResult.StartDate = START_TIME;
            expectedResult.EndDate = END_TIME;

            //DTO object setup
            dtoLookupResult.Id = TASK_ELEMENT_ID;
            dtoLookupResult.BOETaskID = TASK_ID;
            dtoLookupResult.BoeID = BOE_ID;
            dtoLookupResult.Description = DESCRIPTION;
            dtoLookupResult.StartDate = START_TIME;
            dtoLookupResult.EndDate = END_TIME;
            dtoLookupResult.WorkspaceVariableIDs = WorkspaceVars;
        }

        [TestMethod]
        public void BoeTaskElementImportMergeTests_ExtractValidTaskElementsFromOfflineImport()
        {
            //act
            BoeTaskElementDTO result = taskElementImportMerge.ExtractValidTaskElementsFromOfflineImport(workOfflineTask);

            //assert
            Assert.AreEqual(expectedResult.Id, result.Id);
            Assert.AreEqual(expectedResult.BOETaskID, result.BOETaskID);
            Assert.AreEqual(expectedResult.BoeID, result.BoeID);
            Assert.AreEqual(expectedResult.Description, result.Description);
            Assert.AreEqual(expectedResult.StartDate, result.StartDate);
            Assert.AreEqual(expectedResult.EndDate, result.EndDate);
        }


        [TestMethod]
        public void BoeTaskElementImportMergeTests_MergeTaskWithData_UpdateAll()
        {
            //arrange
            BoeTaskElementDTO temp = taskElementImportMerge.ExtractValidTaskElementsFromOfflineImport(workOfflineTask);
            temp.WasMoqTextSet = true; temp.WasDescriptionSet = true;
            temp.WorkspaceVariableIDs = WorkspaceVars_B;
            //act
            WorkspaceDTO workspace = new WorkspaceDTO();
            BoeTaskElementDTO result = taskElementImportMerge.MergeTaskWithData(temp, true, workspace);

            //assert
            Assert.AreEqual(expectedResult.Id, result.Id);
            Assert.AreEqual(expectedResult.BOETaskID, result.BOETaskID);
            Assert.AreEqual(expectedResult.BoeID, result.BoeID);
            Assert.AreEqual(expectedResult.Description, result.Description);
            Assert.AreEqual(expectedResult.StartDate, result.StartDate);
            Assert.AreEqual(expectedResult.EndDate, result.EndDate);
            Assert.AreEqual(WorkspaceVars_B, result.WorkspaceVariableIDs);

        }

        [TestMethod]
        public void BoeTaskElementImportMergeTests_MergeTaskWithData_UpdateAll_RemoveVars()
        {
            //arrange
            BoeTaskElementDTO temp = taskElementImportMerge.ExtractValidTaskElementsFromOfflineImport(workOfflineTask);
            temp.WasMoqTextSet = true; temp.WasDescriptionSet = true;
            temp.WorkspaceVariableIDs = WorkspaceVars_None;


            //act
            WorkspaceDTO workspace = new WorkspaceDTO();
            BoeTaskElementDTO result = taskElementImportMerge.MergeTaskWithData(temp, true, workspace);

            //assert
            Assert.AreEqual(expectedResult.Id, result.Id);
            Assert.AreEqual(expectedResult.BOETaskID, result.BOETaskID);
            Assert.AreEqual(expectedResult.BoeID, result.BoeID);
            Assert.AreEqual(expectedResult.Description, result.Description);
            Assert.AreEqual(expectedResult.StartDate, result.StartDate);
            Assert.AreEqual(expectedResult.EndDate, result.EndDate);
            Assert.AreEqual(WorkspaceVars_None, result.WorkspaceVariableIDs);

        }

        [TestMethod]
        public void BoeTaskElementImportMergeTests_MergeTaskWithData_SkipUpdates()
        {
            //arrange
            BoeTaskElementDTO temp = taskElementImportMerge.ExtractValidTaskElementsFromOfflineImport(workOfflineTask);
            temp.WasMoqTextSet = true; temp.WasDescriptionSet = true;

            //act
            WorkspaceDTO workspace = new WorkspaceDTO();
            BoeTaskElementDTO result = taskElementImportMerge.MergeTaskWithData(temp, false, workspace);

            //assert
            Assert.AreEqual(expectedResult.Id, result.Id);
            Assert.AreEqual(expectedResult.BOETaskID, result.BOETaskID);
            Assert.AreEqual(expectedResult.BoeID, result.BoeID);
            Assert.AreEqual(expectedResult.Description, result.Description);
            Assert.AreEqual(expectedResult.StartDate, result.StartDate);
            Assert.AreEqual(expectedResult.EndDate, result.EndDate);

        }

        [TestMethod]
        public void BoeTaskElementImportMergeTests_MergeTaskWithData_NullDTO()
        {
            //arrange
            BoeTaskElementDTO temp = taskElementImportMerge.ExtractValidTaskElementsFromOfflineImport(workOfflineTask);
            temp.WasMoqTextSet = true; temp.WasDescriptionSet = true;
            taskLoaderMock.Setup(s => s.GetById(TASK_ELEMENT_ID, It.IsAny<int>(), It.IsAny<int>())).Returns<BoeTaskElementDTO>(null);

            //act
            WorkspaceDTO workspace = new WorkspaceDTO();
            BoeTaskElementDTO result = taskElementImportMerge.MergeTaskWithData(temp, true, workspace);

            //assert
            Assert.AreEqual(expectedResult.Id, result.Id);
            Assert.AreEqual(expectedResult.BOETaskID, result.BOETaskID);
            Assert.AreEqual(expectedResult.Description, result.Description);
            Assert.AreEqual(expectedResult.StartDate, result.StartDate);
            Assert.AreEqual(expectedResult.EndDate, result.EndDate);

        }

        [TestMethod]
        public void BoeTaskElementImportMergeTests_MergeTaskWithData_ExistingCustomField()
        {
            //arrange
            BoeTaskElementDTO temp = taskElementImportMerge.ExtractValidTaskElementsFromOfflineImport(workOfflineTask);
            temp.WasMoqTextSet = true; temp.WasDescriptionSet = true;
            temp.CustomFieldValueContainers.Add(new CustomFieldValueContainer()
                {
                    CustomFieldValueID = 789, 
                    ContainerID = 456,
                    Updateable = UpdateType.Upsert
                });

            //act
            WorkspaceDTO workspace = new WorkspaceDTO();
            BoeTaskElementDTO result = taskElementImportMerge.MergeTaskWithData(temp, true, workspace);

            //assert
            Assert.AreEqual(expectedResult.Id, result.Id);
            Assert.AreEqual(expectedResult.BOETaskID, result.BOETaskID);
            Assert.AreEqual(expectedResult.BoeID, result.BoeID);
            Assert.AreEqual(expectedResult.Description, result.Description);
            Assert.AreEqual(expectedResult.StartDate, result.StartDate);
            Assert.AreEqual(expectedResult.EndDate, result.EndDate);
            Assert.AreEqual(temp.CustomFieldValueContainers[0].CustomFieldValueID, 789);
        }
        
        [TestMethod]
        public void BoeTaskElementImportMergeTests_MergeTaskWithData_NewCustomField()
        {
            //arrange
            BoeTaskElementDTO temp = taskElementImportMerge.ExtractValidTaskElementsFromOfflineImport(workOfflineTask);
            temp.WasMoqTextSet = true; temp.WasDescriptionSet = true;
            temp.CustomFieldValueContainers.Add(new CustomFieldValueContainer()
            {
                CustomFieldValueID = -1,
                ContainerID = -1,
                Updateable = UpdateType.Upsert
            });

            //act
            WorkspaceDTO workspace = new WorkspaceDTO();
            BoeTaskElementDTO result = taskElementImportMerge.MergeTaskWithData(temp, true, workspace);

            //assert
            Assert.AreEqual(expectedResult.Id, result.Id);
            Assert.AreEqual(expectedResult.BOETaskID, result.BOETaskID);
            Assert.AreEqual(expectedResult.BoeID, result.BoeID);
            Assert.AreEqual(expectedResult.Description, result.Description);
            Assert.AreEqual(expectedResult.StartDate, result.StartDate);
            Assert.AreEqual(expectedResult.EndDate, result.EndDate);
            Assert.AreEqual(temp.CustomFieldValueContainers[0].CustomFieldValueID, -1);

        }
    }
}
