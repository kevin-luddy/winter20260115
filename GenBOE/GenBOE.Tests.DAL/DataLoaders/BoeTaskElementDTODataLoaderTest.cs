// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core;
    using System.Linq;
    using System.Transactions;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using BOELaborType = GenBOE.Dtos.ResourceTypeDto;

    [TestClass]
    public class BoeTaskElementDTODataLoaderTest
    {
        private static void Initialize()
        {
            GlobalTestCaseSetup.ResetGlobalWorkspaceID();
            GlobalTestCaseSetup.CreateBOE(GlobalTestCaseSetup.GlobalWorkspaceID);
            GlobalTestCaseSetup.CreateGlobalTaskElementID();
            GlobalTestCaseSetup.CreateGlobalBOELaborTypeID();
        }

        [TestInitialize]
        public void CreateCustomFieldData()
        {
            Initialize();

            GlobalTestCaseSetup.CreateCustomField(GlobalTestCaseSetup.GlobalWorkspaceID);
            GlobalTestCaseSetup.CreateCustomFieldValue();
            GlobalTestCaseSetup.CreateBOECustomFieldValueXref();
            GlobalTestCaseSetup.CreateTaskElementCustomFieldValueXref();
            GlobalTestCaseSetup.CreateLaborTypeCustomFieldValueXref();
        }

        /// <summary>
        /// Create a loader for testing
        /// </summary>
        /// <returns>Test loader</returns>
        private BoeTaskElementDTODataLoader CreateTestLoader()
        {
            //Arrange            
            IResourceTypeLoader resourceTypeLoader = new ResourceTypeLoader();
            IResourceSpreadLoader resourceSpreadLoader = new ResourceSpreadLoader();
            IOrdinaryVariableLoader ordinaryVariableLoader = new OrdinaryVariableLoader();
            IBoeTaskElementCustomFieldValueXREFLoader taskElementCustomFieldLoader = new BoeTaskElementCustomFieldValueXREFLoader();
            ILaborTypeCustomFieldValueXREFLoader laborTypeCustomFieldLoader = new LaborTypeCustomFieldValueXREFLoader();

            return new BoeTaskElementDTODataLoader(resourceTypeLoader, resourceSpreadLoader, ordinaryVariableLoader, taskElementCustomFieldLoader, laborTypeCustomFieldLoader);
        }
           
        [TestMethod]
        public void L_GetTaskElementIDsByBoeID()
        {
            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();
            ICollection<BoeTaskElementDTO> taskElements = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2);

            Assert.IsTrue(taskElements.Count() > 0);
        }

        /// This is the individual BOE Task Element delete
        ///
        //[TestMethod]
        //public void L_DeleteBOETaskElement()
        //{

        //    //Arrange
        //    var sut = new BoeTaskElementDTODataLoader();
        //    bool GlobalTaskElementReset = false;
        //    BoeTaskElementDTO taskElement = new BoeTaskElementDTO();
        //    int newID = -1;


        //    Collection<BoeTaskElementDTO> lotsOfTaskElements = new Collection<BoeTaskElementDTO>();

        //    // create a task element detail so we always knows there is at least one task element detail to delete
        //    for (int x = 0; x < 4; x++)
        //    {
        //        BoeTaskElementDTO newElement = new BoeTaskElementDTO();

        //        newElement.BOETaskElementID = newID--;
        //        newElement.BOETaskID = "B48";
        //        newElement.Updateable = UpdateType.Upsert;
        //        newElement.Description = "thursdays are terrific";
        //        newElement.StartDate = Convert.ToDateTime("07/01/2010");
        //        newElement.EndDate = Convert.ToDateTime("07/01/2012");
        //        newElement.MOQHoursEquation = "14500 Hours * 75% ProFactor";
        //        newElement.MOQType = MOQType.Standard;
        //        newElement.MOQText = "this is my caclculation";
        //        newElement.UpdateDate = DateTime.Now;
        //        newElement.TaskTitle = Guid.NewGuid().ToString();
        //        newElement.BoeID = GlobalTestCaseSetup.GlobalBOEID;
        //        lotsOfTaskElements.Add(newElement);
        //    }


        //    sut.SaveBoeTaskElements(lotsOfTaskElements);

        //    Collection<int> taskElementIDs = sut.GetTaskElementIDsByBoeID(GlobalTestCaseSetup.GlobalBOEID);
        //    int randomTaskElementID = taskElementIDs[Math.Abs(MOQObject.randomNumberGenerator.Next(taskElementIDs.Count - 1))];
        //    int count = taskElementIDs.Count;
        //    // get random task element to delete
        //    taskElement = sut.GetBoeTaskElementByTaskElementID(randomTaskElementID);

        //    // if the task element ID selected for delete happens to match the global test task element id,
        //    // set the global value to 0 so it will be repopulated
        //    if (taskElement.BOETaskElementID == GlobalTestCaseSetup.GlobalTaskElementID)
        //    {
        //        GlobalTestCaseSetup.ResetGlobalTaskElementID();
        //        GlobalTestCaseSetup.CreateGlobalTaskElementID();
        //        GlobalTaskElementReset = true;
        //    }

        //    // Since the selected task element id and the global task element were equal and the global
        //    // task element had to be redefined, that means the GetAllTaskElementbyBOE for the global task
        //    // element ID would have increased so the assert would never work. If the global task element id
        //    // had to be redefined, go ahead and get another get before a delete
        //    if (GlobalTaskElementReset)
        //    {
        //        count = sut.GetTaskElementIDsByBoeID(GlobalTestCaseSetup.GlobalBOEID).Count;

        //    }
        //    sut.DeleteBOETaskElement(taskElement);

        //    int afterCount = sut.GetTaskElementIDsByBoeID(GlobalTestCaseSetup.GlobalBOEID).Count;
        //    Assert.AreEqual(count - 1, afterCount);

        //}

        [TestMethod]
        public void L_GetOrdinaryVariablesByTaskElement()
        {
            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            string guidstring = string.Empty;

            Collection<BoeTaskElementDTO> lotsOfTaskElements = new Collection<BoeTaskElementDTO>();

            // Act
            // create a task element detail so we always know there is something to get on in the db

            BoeTaskElementDTO newElement = new BoeTaskElementDTO();
            newElement.Id = -1;
            newElement.BOETaskID = "B48";
            newElement.Updateable = UpdateType.Upsert;
            newElement.Description = "thursdays are terrific";
            newElement.StartDate = Convert.ToDateTime("07/01/2010");
            newElement.EndDate = Convert.ToDateTime("07/01/2012");
            newElement.MOQHoursEquation = "14500 Hours * 75% ProFactor";
            newElement.MOQText = "this is my caclculation";
            newElement.MOQType = MOQType.SSCHistoricalExperienceFactor;
            newElement.UpdateDate = DateTime.Now;
            guidstring = Guid.NewGuid().ToString();
            newElement.TaskTitle = guidstring;
            newElement.BoeID = GlobalTestCaseSetup.GlobalBOEID;

            // create new ordinary variables
            Collection<OrdinaryVariableDto> OrdinaryVars = new Collection<OrdinaryVariableDto>();

            OrdinaryVariableDto OrdinaryVar = new OrdinaryVariableDto();
            OrdinaryVar.Updateable = UpdateType.Upsert;
            OrdinaryVar.Id = -1;
            OrdinaryVar.OrdinaryVariableName = "WorkHours";
            OrdinaryVar.OrdinaryVariableValue = 245000m;
            OrdinaryVar.UpdateDate = DateTime.Now;

            OrdinaryVariableDto OrdinaryVar2 = new OrdinaryVariableDto();
            OrdinaryVar2.Updateable = UpdateType.Upsert;
            OrdinaryVar2.Id = -1;
            OrdinaryVar2.OrdinaryVariableName = "ComplexityFactor";
            OrdinaryVar2.OrdinaryVariableValue = 12m;
            OrdinaryVar2.UpdateDate = DateTime.Now;

            OrdinaryVars.Add(OrdinaryVar2);

            // add the ordinary variables to the task element
            newElement.OrdinaryVariables = OrdinaryVars;

            lotsOfTaskElements.Add(newElement);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.SaveBoeTaskElements(lotsOfTaskElements);
                scope.Complete();
            }

            ICollection<BoeTaskElementDTO> boeTaskElements = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2);

            BoeTaskElementDTO assertTaskElement = boeTaskElements.FirstOrDefault(b => b.TaskTitle == guidstring);

            // Assert
            Assert.IsTrue(assertTaskElement.OrdinaryVariables.Count > 0, "No results found for Ordinary Variables");

        }

        [TestMethod]
        public void L_CreateSaveLMLaborType()
        {
            // Initialize();

            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            int initialResults = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).Count;

            string guidstring = string.Empty;

            // create a task element to associate the new labor type to
            var TaskElement = new BoeTaskElementDTO();
            TaskElement.Id = -1;
            TaskElement.StartDate = Convert.ToDateTime("09/01/2010");
            TaskElement.EndDate = Convert.ToDateTime("11/01/2010");
            TaskElement.MOQHoursEquation = "14200 LOCO * 45% PercentFactorCool";
            TaskElement.MOQText = "moq test";
            TaskElement.MOQType = MOQType.SSCBottomUp;
            TaskElement.Updateable = UpdateType.Upsert;
            guidstring = Guid.NewGuid().ToString();
            TaskElement.TaskTitle = guidstring;
            TaskElement.BoeID = GlobalTestCaseSetup.GlobalBOEID;

            // create labor type
            ResourceTypeDto createLaborType = new BOELaborType();
            createLaborType.Id = -1;
            createLaborType.ResourceID = GlobalTestCaseSetup.GlobalResourceID;
            createLaborType.PerformingOrgID = GlobalTestCaseSetup.GlobalPerfOrgID;
            createLaborType.SpreadCurveID = SpreadCurves.SpreadCurve17;
            createLaborType.PercentSpread = 11;
            createLaborType.ValueSpread = 10;
            createLaborType.SpreadType = SpreadType.Hours;
            createLaborType.StartDateValue = Convert.ToDateTime("09/01/2010");
            createLaborType.EndDateValue = Convert.ToDateTime("11/01/2010");
            createLaborType.PercentSpreadLocked = true;
            createLaborType.HourSpreadLocked = false;
            createLaborType.UpdateDate = DateTime.Now;
            createLaborType.Updateable = UpdateType.Upsert;
            createLaborType.CanOffload = true;
            createLaborType.TieredPercentage = 1.2m;
            createLaborType.AddOrDelete = "A";

            // add labor type to task element
            TaskElement.taskElementLabors.Add(createLaborType);

            // Act
            using (TransactionScope scope = new TransactionScope())
            {
                sut.SaveBoeTaskElements(new Collection<BoeTaskElementDTO> { TaskElement });
                scope.Complete();
            }

            int postResults = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).Count;

            // Assert that the task element was saved
            Assert.IsTrue(initialResults == postResults - 1);

            // Assert that the task elemnent had a labor type saved too
            ICollection<BoeTaskElementDTO> boeTaskElements = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2);

            BoeTaskElementDTO taskElementToRedit = boeTaskElements.FirstOrDefault(b => b.TaskTitle == guidstring);
            Assert.IsTrue(taskElementToRedit.taskElementLabors.Count > 0, "The labor type didn't save correctly");
            Assert.IsTrue(taskElementToRedit.taskElementLabors.First().LaborSpreads.Any(), "The labor spreads did not get automatically created");

            //Now, test a save of a current LM Labor type 
            // Act
            Collection<BoeTaskElementDTO> updatedTaskElements = new Collection<BoeTaskElementDTO>();

            taskElementToRedit.Description = "LA LA LA";
            taskElementToRedit.Updateable = UpdateType.Upsert;


            if (taskElementToRedit.taskElementLabors.Count > 0)
            {
                Collection<BOELaborType> labors = new Collection<BOELaborType>();
                // adjust all the labors
                foreach (ResourceTypeDto boeLabor in taskElementToRedit.taskElementLabors)
                {
                    ResourceTypeDto LT = new BOELaborType();
                    LT = boeLabor;

                    // adjust a field
                    decimal changePercent = 11.0m;
                    LT.PercentSpread = changePercent;
                    LT.ValueSpread = 5;
                    LT.Updateable = UpdateType.Upsert;
                    labors.Add(LT);

                }
                taskElementToRedit.taskElementLabors = labors;
                updatedTaskElements.Add(taskElementToRedit);
            }

            using (TransactionScope scope = new TransactionScope())
            {
                sut.SaveBoeTaskElements(updatedTaskElements);
                scope.Complete();
            }

            BoeTaskElementDTO task = sut.GetById(taskElementToRedit.Id, 0, 2);
            Assert.AreEqual(task.Description, "LA LA LA", "Description not LA LA LA");
            Assert.AreEqual(task.taskElementLabors[0].PercentSpread, 11.0m, "Percent spread was not 11.0");

        }

        [TestMethod]
        public void L_CreateorSaveTaskElementDetail()
        {
            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            // Act

            // Create a new task element
            var TaskElement = new BoeTaskElementDTO();
            TaskElement.Id = -1;
            TaskElement.StartDate = Convert.ToDateTime("09/01/2010");
            TaskElement.EndDate = Convert.ToDateTime("11/01/2010");
            TaskElement.MOQHoursEquation = "14200 LOCO * 45% PercentFactorCool";
            TaskElement.MOQText = "factor cool ";
            TaskElement.MOQType = MOQType.SSCBottomUp;
            TaskElement.Updateable = UpdateType.Upsert;
            string guidstring = Guid.NewGuid().ToString();
            TaskElement.TaskTitle = guidstring;
            TaskElement.UpdateDate = DateTime.Now;
            TaskElement.BoeID = GlobalTestCaseSetup.GlobalBOEID;

            // create new ordinary variables
            Collection<OrdinaryVariableDto> OrdinaryVars = new Collection<OrdinaryVariableDto>();

            OrdinaryVariableDto OrdinaryVar = new OrdinaryVariableDto();
            OrdinaryVar.Updateable = UpdateType.Upsert;
            OrdinaryVar.Id = -1;
            OrdinaryVar.OrdinaryVariableName = "WorkHours";
            OrdinaryVar.OrdinaryVariableValue = 24502m;
            OrdinaryVar.UpdateDate = DateTime.Now;

            OrdinaryVars.Add(OrdinaryVar);
            OrdinaryVariableDto OrdinaryVar2 = new OrdinaryVariableDto();
            OrdinaryVar2.Updateable = UpdateType.Upsert;
            OrdinaryVar2.Id = -2;
            OrdinaryVar2.OrdinaryVariableName = "PercentTime";
            OrdinaryVar2.OrdinaryVariableValue = 10m;
            OrdinaryVar2.UpdateDate = DateTime.Now;

            OrdinaryVars.Add(OrdinaryVar2);

            TaskElement.OrdinaryVariables = OrdinaryVars;

            int beforeSaveCount = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).Count;
            using (TransactionScope scope = new TransactionScope())
            {
                int newTaskElementID = sut.CreateOrSaveTaskElementDetail(GlobalTestCaseSetup.GlobalBOEID, TaskElement);
                Assert.IsTrue(newTaskElementID > 0, "The Task Element Detail Save was not successful");
                scope.Complete();
            }

            int AfterSaveCount = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).Count;

            // we added a new task element details so the count should be increased by 1
            Assert.AreEqual(beforeSaveCount + 1, AfterSaveCount, "Task Detail count does not match");


            ICollection<BoeTaskElementDTO> boeTaskElements = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2);

            BoeTaskElementDTO boeTask2 = boeTaskElements.FirstOrDefault(b => b.TaskTitle == guidstring);

            BoeTaskElementDTO boeTask = boeTask2;
                    boeTask.MOQText = guidstring;
                    boeTask.Description = guidstring;
                    boeTask.TaskTitle = guidstring;
                    boeTask.Updateable = UpdateType.Upsert;

            foreach (OrdinaryVariableDto var in boeTask.OrdinaryVariables)
            {
                var.Updateable = UpdateType.Upsert;
            }

            using (TransactionScope scope = new TransactionScope())
            {
                int sameTaskElementID = sut.CreateOrSaveTaskElementDetail(GlobalTestCaseSetup.GlobalBOEID, boeTask);
                scope.Complete();
                // Assert
                Assert.IsTrue(sameTaskElementID > 0, "The Task Element Detail returned an error");
            }

            BoeTaskElementDTO checkTask = sut.GetById(boeTask.Id, 0, 2);

            // verify that the save worked
            Assert.AreEqual(checkTask.MOQText, guidstring, "The MOQ Text is incorrect");
            Assert.AreEqual(checkTask.Description, guidstring, "The desc is incorrect");
            Assert.AreEqual(checkTask.TaskTitle, guidstring, "The task title is incorrect");

        }

        [TestMethod]
        public void L_DeleteLMLaborType()
        {
            // Initialize();
            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            int BeforeSaveCount = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).Count;
            string guidstring = string.Empty;

            // create a task element to associate the new labor type to
            Collection<BoeTaskElementDTO> taskElements = new Collection<BoeTaskElementDTO>();
            BoeTaskElementDTO TaskElement = new BoeTaskElementDTO();
            TaskElement.Id = -1;
            TaskElement.BoeID = GlobalTestCaseSetup.GlobalBOEID;
            TaskElement.StartDate = Convert.ToDateTime("09/01/2010");
            TaskElement.EndDate = Convert.ToDateTime("11/01/2010");
            TaskElement.MOQHoursEquation = "14200 LOCO * 45% PercentFactorCool";
            TaskElement.MOQText = guidstring;
            TaskElement.MOQType = MOQType.SSCBottomUp;
            TaskElement.Updateable = UpdateType.Upsert;
            guidstring = Guid.NewGuid().ToString();
            TaskElement.TaskTitle = guidstring;

            // create labor type
            ResourceTypeDto createLaborType = new BOELaborType();
            createLaborType.Id = -1;
            createLaborType.ResourceID = GlobalTestCaseSetup.GlobalResourceID;
            createLaborType.PerformingOrgID = GlobalTestCaseSetup.GlobalPerfOrgID;
            createLaborType.SpreadCurveID = SpreadCurves.SpreadCurve17;
            createLaborType.PercentSpread = 11;
            createLaborType.ValueSpread = 10;
            createLaborType.SpreadType = SpreadType.Hours;
            createLaborType.StartDateValue = Convert.ToDateTime("09/01/2010");
            createLaborType.EndDateValue = Convert.ToDateTime("11/01/2010");
            createLaborType.HourSpreadLocked = false;
            createLaborType.PercentSpreadLocked = false;
            createLaborType.UpdateDate = DateTime.Now;
            createLaborType.Updateable = UpdateType.Upsert;
            createLaborType.BoeID = GlobalTestCaseSetup.GlobalBOEID;


            // add labor type to task element
            TaskElement.taskElementLabors.Add(createLaborType);
            taskElements.Add(TaskElement);
            // Act

            using (TransactionScope scope = new TransactionScope())
            {
                sut.SaveBoeTaskElements(taskElements);
                scope.Complete();
            }

            int postResults = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).Count;

            // Assert that the task element was saved
            Assert.IsTrue(BeforeSaveCount == postResults - 1);

            //Now, test a save of a current LM Labor type 
            // Act
            ICollection<BoeTaskElementDTO> Tasks = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2);

            int TaskElementIDWithLabor = 0;

            using (TransactionScope scope = new TransactionScope())
            {
                foreach (BoeTaskElementDTO BoeTaskElement in Tasks)
                {
                    // delete all LM labor types for this BOE
                    if (BoeTaskElement.taskElementLabors.Count > 0)
                    {
                        TaskElementIDWithLabor = BoeTaskElement.Id;
                        // adjust all the labors


                        foreach (ResourceTypeDto boeLabor in BoeTaskElement.taskElementLabors)
                        {
                            ResourceTypeDto LT = new BOELaborType();
                            LT = boeLabor;
                            LT.Updateable = UpdateType.Deleted;
                            sut.DeleteLMLaborType(LT);  //Delete Labor Type
                        }
                    }
                }
                scope.Complete();
            }

            BoeTaskElementDTO checkTask = sut.GetById(TaskElementIDWithLabor, 0, 2);

            Assert.IsTrue(checkTask.taskElementLabors.Count == 0, "The delete labor type did not work");
        }

        [TestMethod]
        public void L_SaveOrdinaryVariables()
        {
            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            // Create 3 new Ordinary Variables and associate them to a BOE Task Element Detail 
            // Create a new task element

            BoeTaskElementDTO newElement = new BoeTaskElementDTO();
            newElement.Id = -1;
            newElement.BOETaskID = "Task X";
            newElement.Updateable = UpdateType.Upsert;
            newElement.Description = "This is Task X. Yay";
            newElement.StartDate = Convert.ToDateTime("10/01/2010");
            newElement.EndDate = Convert.ToDateTime("10/01/2013");
            newElement.MOQHoursEquation = "Employee * SourceCode + Vacation";
            newElement.MOQText = "employees, sourcecode, vacation";
            newElement.MOQType = MOQType.SSCAnalogySimilarTo;
            newElement.UpdateDate = DateTime.Now;
            string guidstring = Guid.NewGuid().ToString();
            newElement.TaskTitle = guidstring;
            newElement.BoeID = GlobalTestCaseSetup.GlobalBOEID;

            // create new ordinary variables
            Collection<OrdinaryVariableDto> OrdinaryVars = new Collection<OrdinaryVariableDto>();

            OrdinaryVariableDto OrdinaryVar = new OrdinaryVariableDto();
            OrdinaryVar.Updateable = UpdateType.Upsert;
            OrdinaryVar.Id = -1;
            OrdinaryVar.OrdinaryVariableName = "Employee";
            OrdinaryVar.OrdinaryVariableValue = 245m;
            OrdinaryVar.UpdateDate = DateTime.Now;
            OrdinaryVars.Add(OrdinaryVar);

            OrdinaryVariableDto OrdinaryVar2 = new OrdinaryVariableDto();
            OrdinaryVar2.Updateable = UpdateType.Upsert;
            OrdinaryVar2.Id = -2;
            OrdinaryVar2.OrdinaryVariableName = "SourceCode";
            OrdinaryVar2.OrdinaryVariableValue = 10m;
            OrdinaryVar2.UpdateDate = DateTime.Now;
            OrdinaryVars.Add(OrdinaryVar2);

            OrdinaryVariableDto OrdinaryVar3 = new OrdinaryVariableDto();
            OrdinaryVar3.Updateable = UpdateType.Upsert;
            OrdinaryVar3.Id = -3;
            OrdinaryVar3.OrdinaryVariableName = "Vacation";
            OrdinaryVar3.OrdinaryVariableValue = 40m;
            OrdinaryVar3.UpdateDate = DateTime.Now;
            OrdinaryVars.Add(OrdinaryVar3);


            newElement.OrdinaryVariables = OrdinaryVars;

            int newTaskElementID;
            using (TransactionScope scope = new TransactionScope())
            {
                newTaskElementID = sut.CreateOrSaveTaskElementDetail(GlobalTestCaseSetup.GlobalBOEID, newElement);
                scope.Complete();
            }

            // Then, delete one of the ordinary variables, edit one ordinary variable, and leave the third alone
            ICollection<BoeTaskElementDTO> boeTaskElements = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2);

            // get the task ordinary variables associated with the task element we just created
            BoeTaskElementDTO editTaskResults = boeTaskElements.FirstOrDefault(b => b.Id == newTaskElementID);

            int BeforeChangesCount = editTaskResults.OrdinaryVariables.Count;

            // set all variables to upsert and then customize a couple others below
            foreach (OrdinaryVariableDto var in editTaskResults.OrdinaryVariables)
            {
                var.Updateable = UpdateType.Upsert;
            }

            //delete the first ordinary var
            editTaskResults.OrdinaryVariables[0].Updateable = UpdateType.Deleted;

            // change the second ordinary var value
            editTaskResults.OrdinaryVariables[1].OrdinaryVariableValue = 67.4m;
            editTaskResults.OrdinaryVariables[1].Updateable = UpdateType.Upsert;

            using (TransactionScope scope = new TransactionScope())
            {
                newTaskElementID = sut.CreateOrSaveTaskElementDetail(GlobalTestCaseSetup.GlobalBOEID, editTaskResults);
                scope.Complete();
            }

            // Now that we have saved, check the count
            boeTaskElements = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2);
            BoeTaskElementDTO assertTaskElement = boeTaskElements.FirstOrDefault(b => b.Id == newTaskElementID);
            Assert.AreEqual(BeforeChangesCount - 1, assertTaskElement.OrdinaryVariables.Count, "The ordinary variable delete did not occur");            
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullSaveOrdinaryVar()
        {
            // Arrange
            // Initialize();
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            BoeTaskElementDTO singleTaskElement = new BoeTaskElementDTO();

            //Act

            // add a task element
            BoeTaskElementDTO newElement = new BoeTaskElementDTO();
            newElement.Id = -1;
            newElement.BOETaskID = "Task X";
            newElement.Updateable = UpdateType.Upsert;
            newElement.Description = "This is Task X. Yay";
            newElement.StartDate = Convert.ToDateTime("10/01/2010");
            newElement.EndDate = Convert.ToDateTime("10/01/2013");
            newElement.MOQHoursEquation = "Employee * SourceCode + Vacation";
            newElement.MOQText = "employees, sourcecode, vacation";
            newElement.MOQType = MOQType.SSCBottomUp;
            newElement.UpdateDate = DateTime.Now;
            string guidstring = Guid.NewGuid().ToString();
            newElement.TaskTitle = guidstring;
            newElement.BoeID = GlobalTestCaseSetup.GlobalBOEID;

            sut.SaveBoeTaskElements(new Collection<BoeTaskElementDTO> { newElement });


            // get the first task element
            singleTaskElement = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).FirstOrDefault();
            Assert.IsNotNull(singleTaskElement);
            singleTaskElement.OrdinaryVariables = null;

            sut.SaveOrdinaryVariables(singleTaskElement.OrdinaryVariables);

        }

        /// <summary>
        /// This test case will create a task element detail including the creation of an ordinary variable.
        /// Once the task element detail has been saved, the test case will re-call the Save but this time
        /// with an incorrect UpdateDate of the orindary variable which will cause an exception to be thrown
        /// from the database. Since this function expects an exception, no asserts are needed.
        /// </summary>
        [ExpectedException(typeof(EntityCommandExecutionException))]
        [TestMethod]
        public void L_BadDateSaveOrdinaryVar()
        {            
            //Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            // Create a new task element
            BoeTaskElementDTO newElement = new BoeTaskElementDTO();
            newElement.Id = -1;
            newElement.BOETaskID = "1.2.3.4.5.6";
            newElement.Updateable = UpdateType.Upsert;
            newElement.Description = "This is Task X. Yay";
            newElement.StartDate = Convert.ToDateTime("10/01/2010");
            newElement.EndDate = Convert.ToDateTime("10/01/2013");
            newElement.MOQHoursEquation = "Employee + 5";
            newElement.MOQText = "employees plus five";
            newElement.MOQType = MOQType.Standard;
            newElement.UpdateDate = DateTime.Now;
            newElement.TaskTitle = Guid.NewGuid().ToString();
            newElement.Updateable = UpdateType.Upsert;
            newElement.BoeID = GlobalTestCaseSetup.GlobalBOEID;

            // create a new ordinary variable to be tied to this task element detail
            Collection<OrdinaryVariableDto> OrdinaryVars = new Collection<OrdinaryVariableDto>();

            OrdinaryVariableDto OrdinaryVar = new OrdinaryVariableDto();
            OrdinaryVar.Updateable = UpdateType.Upsert;
            OrdinaryVar.Id = -1;
            OrdinaryVar.OrdinaryVariableName = "Employee";
            OrdinaryVar.OrdinaryVariableValue = 245m;
            OrdinaryVar.UpdateDate = DateTime.Now;
            OrdinaryVars.Add(OrdinaryVar);

            newElement.OrdinaryVariables = OrdinaryVars;

            // Save the boe, task elements, and ordinary vars
            int newTaskElementID = sut.CreateOrSaveTaskElementDetail(GlobalTestCaseSetup.GlobalBOEID, newElement);

            // Now, we want to get one of the ordinary variables and set it with DateTime.Now which will not match
            // the UpdateDate in the database. When dates do not match, an EntityCommandExecutionException is thrown
            ICollection<BoeTaskElementDTO> boeTaskElements = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2);
            BoeTaskElementDTO editTaskResults = boeTaskElements.FirstOrDefault(b => b.Id == newTaskElementID);
            Assert.IsNotNull(editTaskResults);

            editTaskResults.OrdinaryVariables[0].UpdateDate = DateTime.Now;
            editTaskResults.OrdinaryVariables[0].Updateable = UpdateType.Upsert;

            // Save the task element with the ordinary variable that has an incorrect date. This will throw the exception
            sut.SaveOrdinaryVariables(editTaskResults.OrdinaryVariables);
        }

        [TestMethod]
        public void L_SaveOrdinarySelectBoeToSumVariables()
        {
            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            // Create 3 new Ordinary Variables and associate them to a BOE Task Element Detail 
            // Create a new task element

            BoeTaskElementDTO newElement = new BoeTaskElementDTO();
            newElement.Id = -1;
            newElement.BOETaskID = "Task X";
            newElement.Updateable = UpdateType.Upsert;
            newElement.Description = "This is Task X. Yay";
            newElement.StartDate = Convert.ToDateTime("10/01/2010");
            newElement.EndDate = Convert.ToDateTime("10/01/2013");
            newElement.MOQHoursEquation = "Employee * SourceCode + Vacation";
            newElement.MOQText = "employees, sourcecode, vacation";
            newElement.MOQType = MOQType.SSCBottomUp;
            newElement.UpdateDate = DateTime.Now;
            string guidstring = Guid.NewGuid().ToString();
            newElement.TaskTitle = guidstring;
            newElement.Updateable = UpdateType.Upsert;
            newElement.BoeID = GlobalTestCaseSetup.GlobalBOEID;

            // create new ordinary variables
            Collection<OrdinaryVariableDto> OrdinaryVars = new Collection<OrdinaryVariableDto>();

            // create an ordinary var that is a sum of boe
            OrdinaryVariableDto OrdinaryVar = new OrdinaryVariableDto();
            OrdinaryVar.Updateable = UpdateType.Upsert;
            OrdinaryVar.Id = -1;
            OrdinaryVar.OrdinaryVariableName = "Employee";
            OrdinaryVar.OrdinaryVariableValue = 245m;
            OrdinaryVar.UpdateDate = DateTime.Now;
            OrdinaryVar.SortBOEBy = VarSortBOEBy.WBS;
            OrdinaryVar.ValueType = VarValueType.SumOfBOEs;
            SelectBOEsToSum selectBoe = new SelectBOEsToSum();
            selectBoe.WBSID = null;
            selectBoe.BoeID = GlobalTestCaseSetup.GlobalBOEID;
            selectBoe.CLINID = null;
            OrdinaryVar.SelectedBOEsToSum.Add(selectBoe);
            OrdinaryVars.Add(OrdinaryVar);

            OrdinaryVariableDto OrdinaryVar2 = new OrdinaryVariableDto();
            OrdinaryVar2.Updateable = UpdateType.Upsert;
            OrdinaryVar2.Id = -2;
            OrdinaryVar2.OrdinaryVariableName = "SourceCode";
            OrdinaryVar2.OrdinaryVariableValue = 10m;
            OrdinaryVar2.UpdateDate = DateTime.Now;
            OrdinaryVars.Add(OrdinaryVar2);

            OrdinaryVariableDto OrdinaryVar3 = new OrdinaryVariableDto();
            OrdinaryVar3.Updateable = UpdateType.Upsert;
            OrdinaryVar3.Id = -3;
            OrdinaryVar3.OrdinaryVariableName = "Vacation";
            OrdinaryVar3.OrdinaryVariableValue = 40m;
            OrdinaryVar3.UpdateDate = DateTime.Now;
            OrdinaryVars.Add(OrdinaryVar3);


            newElement.OrdinaryVariables = OrdinaryVars;

            int newTaskElementID;
            using (TransactionScope scope = new TransactionScope())
            {
                newTaskElementID = sut.CreateOrSaveTaskElementDetail(GlobalTestCaseSetup.GlobalBOEID, newElement);
                scope.Complete();
            }

            ICollection<BoeTaskElementDTO> boeTaskElements = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2);

            // get the task ordinary variables associated with the task element we just created
            BoeTaskElementDTO editTaskResults = boeTaskElements.FirstOrDefault(b => b.Id == newTaskElementID);
            Assert.IsNotNull(editTaskResults);

            Assert.IsTrue(editTaskResults.OrdinaryVariables.Count == 3, "There weren't 3 ordinary vars");
            var selectBoeVar = (from t in editTaskResults.OrdinaryVariables
                                where t.OrdinaryVariableName == "Employee"
                                select t).First();
            var boeID = (from s in selectBoeVar.SelectedBOEsToSum
                         select s.BoeID).First();

            Assert.IsTrue(selectBoeVar.ValueType == VarValueType.SumOfBOEs, "Value Type was not sum of boe");
            Assert.IsTrue(boeID == GlobalTestCaseSetup.GlobalBOEID, "The selected sum BOE ID did not match Global BOE ID");



        }

        [TestMethod]
        public void L_SaveBoeTaskElements()
        {
            // Initialize();
            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();

            int BeforeSaveCount = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).Count;
            BoeTaskElementDTO boeTask = new BoeTaskElementDTO();
            boeTask.Id = -1;
            boeTask.BoeID = GlobalTestCaseSetup.GlobalBOEID;
            boeTask.StartDate = Convert.ToDateTime("09/01/2010");
            boeTask.EndDate = Convert.ToDateTime("11/01/2010");
            boeTask.MOQHoursEquation = "14200 LOCO * 45% PercentFactorCool";
            boeTask.MOQText = "MOQ";
            boeTask.MOQType = MOQType.SSCAnalogySimilarTo;
            boeTask.Updateable = UpdateType.Upsert;
            string guidstring = Guid.NewGuid().ToString();
            boeTask.TaskTitle = guidstring;
            boeTaskElements.Add(boeTask);
            sut.SaveBoeTaskElements(boeTaskElements);

            int AfterSaveCount = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).Count;

            Assert.AreEqual(BeforeSaveCount + 1, AfterSaveCount, "The counts do not match");
        }

        //[TestMethod]
        //public void L_DeleteBOETaskElements()
        //{

        //    //  Initialize();
        //    var sut = new BoeTaskElementDTODataLoader();
        //    Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();


        //    // save a task element so we always know there is one to delete
        //    BoeTaskElementDTO boeTask = new BoeTaskElementDTO();
        //    boeTask.BOETaskElementID = -1;
        //    boeTask.BoeID = GlobalTestCaseSetup.GlobalBOEID;
        //    boeTask.StartDate = Convert.ToDateTime("09/01/2010");
        //    boeTask.EndDate = Convert.ToDateTime("11/01/2010");
        //    boeTask.MOQHoursEquation = "14200 LOCO * 45% PercentFactorCool";
        //    boeTask.MOQText = "MOQ";
        //    boeTask.MOQType = MOQType.Standard;
        //    boeTask.Updateable = UpdateType.Upsert;
        //    string guidstring = Guid.NewGuid().ToString();
        //    boeTask.TaskTitle = guidstring;
        //    boeTaskElements.Add(boeTask);
        //    sut.SaveBoeTaskElements(boeTaskElements);

        //    Collection<int> Ids = sut.GetTaskElementIDsByBoeID(GlobalTestCaseSetup.GlobalBOEID);

        //    int BeforeDeleteCount = sut.GetTaskElementIDsByBoeID(GlobalTestCaseSetup.GlobalBOEID).Count;
        //    boeTaskElements = new Collection<BoeTaskElementDTO>();
        //    BoeTaskElementDTO deleteTask = sut.GetBoeTaskElementByTaskElementID(Ids[0]);
        //    deleteTask.Updateable = UpdateType.Deleted;
        //    boeTaskElements.Add(deleteTask);
        //    sut.DeleteBOETaskElements(boeTaskElements);
        //    int AfterCount = sut.GetTaskElementIDsByBoeID(GlobalTestCaseSetup.GlobalBOEID).Count;

        //    Assert.AreEqual(BeforeDeleteCount - 1, AfterCount, "Delete did not work");

        //}

        [TestMethod]
        public void L_DeleteTaskElementDetail()
        {

            // Initialize();
            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();

            // save a task element so we always know there is one to delete
            BoeTaskElementDTO boeTask = new BoeTaskElementDTO();
            boeTask.Id = -1;
            boeTask.BoeID = GlobalTestCaseSetup.GlobalBOEID;
            boeTask.StartDate = Convert.ToDateTime("09/01/2010");
            boeTask.EndDate = Convert.ToDateTime("11/01/2010");
            boeTask.MOQHoursEquation = "14200 LOCO * 45% PercentFactorCool";
            boeTask.MOQText = "MOQ";
            boeTask.MOQType = MOQType.SSCBottomUp;
            boeTask.Updateable = UpdateType.Upsert;
            string guidstring = Guid.NewGuid().ToString();
            boeTask.TaskTitle = guidstring;
            boeTaskElements.Add(boeTask);
            sut.SaveBoeTaskElements(boeTaskElements);

            ICollection<BoeTaskElementDTO> TaskElements = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2);

            int BeforeDeleteCount = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).Count;
            boeTaskElements = new Collection<BoeTaskElementDTO>();
            BoeTaskElementDTO deleteTask = TaskElements.FirstOrDefault();
            Assert.IsNotNull(deleteTask);
            deleteTask.Updateable = UpdateType.Deleted;

            sut.DeleteTaskElementDetail(deleteTask);
            int AfterCount = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).Count;

            Assert.AreEqual(BeforeDeleteCount - 1, AfterCount, "Delete did not work");

        }

        [TestMethod]
        public void L_DeleteLMLaborSpreadsByLaborTypeID()
        {

            // Initialize();
            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();

            int BeforeSaveCount = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).Count;
            BoeTaskElementDTO boeTask = new BoeTaskElementDTO();
            boeTask.Id = -1;
            boeTask.BoeID = GlobalTestCaseSetup.GlobalBOEID;
            boeTask.StartDate = Convert.ToDateTime("09/01/2010");
            boeTask.EndDate = Convert.ToDateTime("11/01/2010");
            boeTask.MOQHoursEquation = "14200 LOCO * 45% PercentFactorCool";
            boeTask.MOQText = "MOQ";
            boeTask.MOQType = MOQType.SSCBottomUp;
            boeTask.Updateable = UpdateType.Upsert;
            string guidstring = Guid.NewGuid().ToString();
            boeTask.TaskTitle = guidstring;

            Collection<BOELaborType> boeLabors = new Collection<BOELaborType>();
            ResourceTypeDto boeLabor = new BOELaborType();
            boeLabor.BoeID = GlobalTestCaseSetup.GlobalBOEID;
            boeLabor.Id = -1;
            boeLabor.ValueSpread = 5000;
            boeLabor.SpreadType = SpreadType.Hours;
            boeLabor.ResourceID = GlobalTestCaseSetup.GlobalResourceID;
            boeLabor.PerformingOrgID = GlobalTestCaseSetup.GlobalPerfOrgID;
            boeLabor.PercentSpread = 100m;
            boeLabor.SpreadCurveID = SpreadCurves.DiscreteHours;
            boeLabor.StartDateValue = Convert.ToDateTime("09/01/2010");
            boeLabor.PercentSpreadLocked = true;
            boeLabor.HourSpreadLocked = false;
            boeLabor.EndDateValue = Convert.ToDateTime("09/01/2010");
            boeLabor.Updateable = UpdateType.Upsert;

            Collection<ResourceSpreadDto> laborSpreads = new Collection<ResourceSpreadDto>();
            ResourceSpreadDto LS = new ResourceSpreadDto();
            LS.BoeID = GlobalTestCaseSetup.GlobalBOEID;
            LS.Id = boeLabor.Id;
            LS.LaborSpreadDate = Convert.ToDateTime("09/01/2010");
            LS.LaborSpreadValue = 5000;
            LS.Updateable = UpdateType.Upsert;
            laborSpreads.Add(LS);
            boeLabor.LaborSpreads = laborSpreads;

            boeLabors.Add(boeLabor);
            boeTask.taskElementLabors = boeLabors;

            boeTaskElements.Add(boeTask);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.SaveBoeTaskElements(boeTaskElements);
                scope.Complete();
            }

            int AfterSaveCount = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).Count;

            Assert.AreEqual(BeforeSaveCount + 1, AfterSaveCount, "Saves did not match");

            // now delete labor spreads

            ICollection<BoeTaskElementDTO> boeTasks = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2);

            BoeTaskElementDTO toEditTask = boeTasks.FirstOrDefault(b => b.TaskTitle == guidstring);

            Assert.IsNotNull(toEditTask);
            Assert.IsTrue(toEditTask.taskElementLabors.First().LaborSpreads.Any(), "The labor spreads did not get saved and retrieved correctly");
            // get labor type ID to delete
            var laborTypeID = (from t in toEditTask.taskElementLabors
                               select t.Id).FirstOrDefault();

            sut.DeleteLMLaborSpreadsByLaborTypeID(laborTypeID);

            toEditTask = sut.GetById(toEditTask.Id, 0, 2);

            // assert that there is no labor spread
            foreach (ResourceTypeDto LT in toEditTask.taskElementLabors)
            {
                Assert.AreEqual(LT.LaborSpreads.Count, 0, "No Labor Spreads");
                break;
            }
        }

        [TestMethod]
        public void L_SaveBOETaskElementLaborTypeWarning()
        {
            //  Initialize();
            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            BoeTaskElementDTO TaskElement = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2).FirstOrDefault();
            Assert.IsNotNull(TaskElement);
            sut.SaveBOETaskElementLaborTypeWarning(TaskElement.Id, false);
            BoeTaskElementDTO boeTask = sut.GetById(TaskElement.Id, 0, 2);
            Assert.AreEqual(boeTask.LaborTypeWarningFlag, false, "The flag was not flag");
        }

        [TestMethod]
        public void L_SaveBOELaborTypeCustomFieldValueXrefs()
        {
            Initialize();

            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            Collection<CustomFieldValueContainer> customFieldContainers = new Collection<CustomFieldValueContainer>();
            BoeTaskElementDTO task = sut.GetById(GlobalTestCaseSetup.GlobalTaskElementID, 0, 2);

            // get one of the labor types to add custom field value contaniers too
            ResourceTypeDto laborType1 = task.taskElementLabors[0];

            int LaborTypeContainercount = laborType1.CustomFieldValueContainers.Count;
            foreach (CustomFieldValueContainer customfield in laborType1.CustomFieldValueContainers)
            {
                customFieldContainers.Add(customfield);
            }


            // create a new custom field value container

            CustomFieldValueContainer boeLaborTypeCustomFieldValueXrefDTO = new CustomFieldValueContainer();
            boeLaborTypeCustomFieldValueXrefDTO.ContainerID = -1;
            boeLaborTypeCustomFieldValueXrefDTO.CustomFieldValueID = GlobalTestCaseSetup.CreateCustomFieldValue(); // need to get a new custom field value ID so we don't have dups
            boeLaborTypeCustomFieldValueXrefDTO.Updateable = UpdateType.Upsert;
            customFieldContainers.Add(boeLaborTypeCustomFieldValueXrefDTO);

            laborType1.CustomFieldValueContainers = customFieldContainers;


            task.Updateable = UpdateType.Upsert;
            foreach (OrdinaryVariableDto var in task.OrdinaryVariables)
            {
                var.Updateable = UpdateType.Upsert;
            }

            using (TransactionScope scope = new TransactionScope())
            {
                sut.SaveBoeTaskElements(new Collection<BoeTaskElementDTO> { task });
                scope.Complete();
            }

            task = sut.GetById(GlobalTestCaseSetup.GlobalTaskElementID, 0, 2);
            int aftercount = (from t in task.taskElementLabors
                              where t.Id == laborType1.Id
                              select t.CustomFieldValueContainers.Count).First();


            Assert.AreEqual(LaborTypeContainercount + 1, aftercount, "The save did not work");

            // now Delete a custom field container

            foreach (ResourceTypeDto labortype in task.taskElementLabors)
            {
                if (labortype.Id == laborType1.Id)
                {
                    laborType1 = labortype;
                    break;
                }
            }

            using (TransactionScope scope = new TransactionScope())
            {
                sut.DeleteBoeLaborTypeCustomFieldValueContainer(laborType1.CustomFieldValueContainers[0], laborType1.Id);
                scope.Complete();
            }

            task = sut.GetById(GlobalTestCaseSetup.GlobalTaskElementID, 0, 2);
            //int afterDeletecount = (from t in task.taskElementLabors
            //                        where t.Id == laborType1.Id
            //                        select t.CustomFieldValueContainers.Count).First();
            //TODO: Sent Don a question about the Delete for this task. It seems to be deleting everything in the table associated with the passed in LaborTypeID which
            // doesn't seem right. Assert will be commented until discussed with Don
            // Assert.AreEqual(aftercount - 1, afterDeletecount, "The Delete did not work");
        }

        [TestMethod]
        public void L_SaveBOETaskElementCustomFieldValueXrefs()
        {
            // Initialize();
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            BoeTaskElementDTO task = sut.GetById(GlobalTestCaseSetup.GlobalTaskElementID, 0, 2);
            Collection<CustomFieldValueContainer> customFieldContainers = new Collection<CustomFieldValueContainer>();

            int BeforeSaveCount = task.CustomFieldValueContainers.Count;
            foreach (CustomFieldValueContainer customfield in task.CustomFieldValueContainers)
            {
                customfield.Updateable = UpdateType.Upsert;
                customFieldContainers.Add(customfield);
            }

            CustomFieldValueContainer boeLaborTypeCustomFieldValueXrefDTO = new CustomFieldValueContainer();
            boeLaborTypeCustomFieldValueXrefDTO.ContainerID = -1;
            boeLaborTypeCustomFieldValueXrefDTO.CustomFieldValueID = GlobalTestCaseSetup.CreateCustomFieldValue(); // need to get a new custom field value ID so we don't have dups
            boeLaborTypeCustomFieldValueXrefDTO.Updateable = UpdateType.Upsert;

            customFieldContainers.Add(boeLaborTypeCustomFieldValueXrefDTO);

            task.CustomFieldValueContainers = customFieldContainers;
            task.Updateable = UpdateType.Upsert;
            foreach (OrdinaryVariableDto var in task.OrdinaryVariables)
            {
                var.Updateable = UpdateType.Upsert;
            }

            foreach (ResourceTypeDto labor in task.taskElementLabors)
            {
                foreach (CustomFieldValueContainer container in labor.CustomFieldValueContainers)
                {
                    container.Updateable = UpdateType.Upsert;
                }
                labor.Updateable = UpdateType.Upsert;
            }

            using (TransactionScope scope = new TransactionScope())
            {
                sut.SaveBoeTaskElements(new Collection<BoeTaskElementDTO> { task });
                scope.Complete();
            }

            task = sut.GetById(GlobalTestCaseSetup.GlobalTaskElementID, 0, 2);
            foreach (OrdinaryVariableDto var in task.OrdinaryVariables)
            {
                var.Updateable = UpdateType.Upsert;
            }

            foreach (ResourceTypeDto labor in task.taskElementLabors)
            {
                foreach (CustomFieldValueContainer container in labor.CustomFieldValueContainers)
                {
                    container.Updateable = UpdateType.Upsert;
                }
                labor.Updateable = UpdateType.Upsert;
            }

            int AfterSaveCount = task.CustomFieldValueContainers.Count;

            Assert.AreEqual(BeforeSaveCount + 1, AfterSaveCount, "The Save did not work");


            // Now Save a Delete
            task.CustomFieldValueContainers[0].Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new TransactionScope())
            {
                sut.SaveBoeTaskElements(new Collection<BoeTaskElementDTO> { task });
                scope.Complete();
            }

            task = sut.GetById(GlobalTestCaseSetup.GlobalTaskElementID, 0, 2);

            Assert.AreEqual(AfterSaveCount - 1, task.CustomFieldValueContainers.Count, "The delete did not work");
        }

        [TestMethod]
        public void L_GetBoeTaskElementIdsByWorkspaceVariableID()
        {
            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            WorkspaceVariableDTODataLoader workspaceVarLoader = new WorkspaceVariableDTODataLoader();
            Collection<int> workspaceVarIDs = workspaceVarLoader.GetByWorkspaceID(GlobalTestCaseSetup.GlobalWorkspaceID).Select(x => x.Id).ToCollection();

            // Create a new task element given a workspace variable ID

            BoeTaskElementDTO newElement = new BoeTaskElementDTO();
            newElement.Id = -1;
            newElement.BOETaskID = "Task X";
            newElement.Updateable = UpdateType.Upsert;
            newElement.Description = "This is Task X. Yay";
            newElement.StartDate = Convert.ToDateTime("10/01/2010");
            newElement.EndDate = Convert.ToDateTime("10/01/2013");
            newElement.MOQHoursEquation = "GlobalMOQVar";
            newElement.MOQText = "GlobalMOQVar only";
            newElement.MOQType = MOQType.SSCActual;
            newElement.UpdateDate = DateTime.Now;
            string guidstring = Guid.NewGuid().ToString();
            newElement.TaskTitle = guidstring;
            newElement.BoeID = GlobalTestCaseSetup.GlobalBOEID;

            // associate workspace variable from above
            Collection<int> WorkspaceVars = new Collection<int>();
            WorkspaceVars.Add(workspaceVarIDs[0]);

            newElement.WorkspaceVariableIDs = WorkspaceVars;

            sut.CreateOrSaveTaskElementDetail(GlobalTestCaseSetup.GlobalBOEID, newElement);

            //another quick test to verify that the ID that we just created is the same one
            ////BoeTaskElementDTO taskElement = sut.GetById(taskElementIDs[0]);
            ////Assert.AreEqual(taskElement.MOQText, newElement.MOQText, "The MOQ Text didn't match");
            ////Assert.AreEqual(taskElement.MOQHoursEquation, newElement.MOQHoursEquation, "The MOQ Eq didn't match");
            ////Assert.AreEqual(taskElement.TaskTitle, newElement.TaskTitle, "The task title didn't match");
        }

        [TestMethod]
        public void L_TaskVarsWithResourceTypes()
        {
            // Arrange
            BoeTaskElementDTODataLoader sut = this.CreateTestLoader();

            // Create 3 new Ordinary Variables and associate them to a BOE Task Element Detail 
            // Create a new task element

            BoeTaskElementDTO newElement = new BoeTaskElementDTO();
            newElement.Id = -1;
            newElement.BOETaskID = "Task X";
            newElement.Updateable = UpdateType.Upsert;
            newElement.Description = "This is Task X. Yay";
            newElement.StartDate = Convert.ToDateTime("10/01/2010");
            newElement.EndDate = Convert.ToDateTime("10/01/2013");
            newElement.MOQHoursEquation = "Employee * SourceCode + Vacation";
            newElement.MOQType = MOQType.SSCLevelOfEffortSupport;
            newElement.MOQText = "employees, sourcecode, vacation";
            newElement.UpdateDate = DateTime.Now;
            string guidstring = Guid.NewGuid().ToString();
            newElement.TaskTitle = guidstring;
            newElement.Updateable = UpdateType.Upsert;
            newElement.BoeID = GlobalTestCaseSetup.GlobalBOEID;

            // create new ordinary variables
            Collection<OrdinaryVariableDto> OrdinaryVars = new Collection<OrdinaryVariableDto>();

            // create an ordinary var that is a sum of boe
            OrdinaryVariableDto OrdinaryVar = new OrdinaryVariableDto();
            OrdinaryVar.Updateable = UpdateType.Upsert;
            OrdinaryVar.Id = -1;
            OrdinaryVar.OrdinaryVariableName = "Employee";
            OrdinaryVar.OrdinaryVariableValue = 245m;
            OrdinaryVar.UpdateDate = DateTime.Now;
            OrdinaryVar.SortBOEBy = VarSortBOEBy.WBS;
            OrdinaryVar.ValueType = VarValueType.SumOfBOEs;
            SelectBOEsToSum selectBoe = new SelectBOEsToSum();
            selectBoe.WBSID = null;
            selectBoe.BoeID = GlobalTestCaseSetup.GlobalBOEID;
            selectBoe.CLINID = null;
            OrdinaryVar.SelectedBOEsToSum.Add(selectBoe);
            Collection<int> ResourceTypeIDs = new Collection<int> {  (int)SumVariableResourceType.SSCLOESub, (int)SumVariableResourceType.SSCLMLabor, (int)SumVariableResourceType.SSCLOEIWTA };
            OrdinaryVar.SumVariableResourceTypeIDs = ResourceTypeIDs;
            OrdinaryVars.Add(OrdinaryVar);

            OrdinaryVariableDto OrdinaryVar2 = new OrdinaryVariableDto();
            OrdinaryVar2.Updateable = UpdateType.Upsert;
            OrdinaryVar2.Id = -2;
            OrdinaryVar2.OrdinaryVariableName = "SourceCode";
            OrdinaryVar2.OrdinaryVariableValue = 10m;
            OrdinaryVar2.UpdateDate = DateTime.Now;
            OrdinaryVars.Add(OrdinaryVar2);

            OrdinaryVariableDto OrdinaryVar3 = new OrdinaryVariableDto();
            OrdinaryVar3.Updateable = UpdateType.Upsert;
            OrdinaryVar3.Id = -3;
            OrdinaryVar3.OrdinaryVariableName = "Vacation";
            OrdinaryVar3.OrdinaryVariableValue = 40m;
            OrdinaryVar3.UpdateDate = DateTime.Now;
            OrdinaryVars.Add(OrdinaryVar3);

            newElement.OrdinaryVariables = OrdinaryVars;

            int newTaskElementID;
            using (TransactionScope scope = new TransactionScope())
            {
                newTaskElementID = sut.CreateOrSaveTaskElementDetail(GlobalTestCaseSetup.GlobalBOEID, newElement);
                scope.Complete();
            }

            ICollection<BoeTaskElementDTO> boeTaskElements = sut.GetByBoeId(GlobalTestCaseSetup.GlobalBOEID, 0, 2);

            // get the task ordinary variables associated with the task element we just created
            BoeTaskElementDTO editTaskResults = boeTaskElements.FirstOrDefault(b => b.Id == newTaskElementID);

            Assert.IsNotNull(editTaskResults);
            Assert.IsTrue(editTaskResults.OrdinaryVariables[0].TaskElementId == editTaskResults.Id, "Task Variable did not match its parent");
            Assert.IsTrue(editTaskResults.OrdinaryVariables[0].SumVariableResourceTypeIDs.Contains((int)SumVariableResourceType.SSCLOESub), "Resource Type ID Array did not contain expected ID");
            Assert.IsTrue(editTaskResults.OrdinaryVariables[0].SumVariableResourceTypeIDs.Contains((int)SumVariableResourceType.SSCLMLabor), "Resource Type ID Array did not contain expected ID");
            Assert.IsTrue(editTaskResults.OrdinaryVariables[0].SumVariableResourceTypeIDs.Contains((int)SumVariableResourceType.SSCLOEIWTA), "Resource Type ID Array did not contain expected ID");
        }

        /// <summary>
        /// Tests the AddDelete property to make sure we are handling the conversion between strings & bools properly
        /// </summary>
        [TestMethod]
        public void TestAddDeleteProperties()
        {
            ResourceTypeDto sut = new ResourceTypeDto();

            sut.AddOrDelete = null;
            Assert.AreEqual(null, sut.IsAddOrDelete);

            sut.AddOrDelete = string.Empty;
            Assert.AreEqual(true, sut.IsAddOrDelete);

            sut.AddOrDelete = "A";
            Assert.AreEqual(true, sut.IsAddOrDelete);

            sut.AddOrDelete = "a";
            Assert.AreEqual(true, sut.IsAddOrDelete);

            sut.AddOrDelete = "ajflkdfjslkjfds";
            Assert.AreEqual(true, sut.IsAddOrDelete);

            sut.AddOrDelete = "D";
            Assert.AreEqual(false, sut.IsAddOrDelete);

            sut.AddOrDelete = "d";
            Assert.AreEqual(false, sut.IsAddOrDelete);


            sut = new ResourceTypeDto();

            sut.IsAddOrDelete = null;
            Assert.AreEqual(null, sut.AddOrDelete);

            sut.IsAddOrDelete = true;
            Assert.AreEqual("A", sut.AddOrDelete);

            sut.IsAddOrDelete = false;
            Assert.AreEqual("D", sut.AddOrDelete);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "newAvg"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "originalAvg")]
        //[TestMethod]
        public void TestOldVsNew()
        {
            //BoeTaskElementDTODataLoader loader = new BoeTaskElementDTODataLoader(new ResourceTypeLoader(), new ResourceSpreadLoader(), new OrdinaryVariableLoader(),
            //                                        new BoeTaskElementCustomFieldValueXREFLoader(), new LaborTypeCustomFieldValueXREFLoader());

            //Stopwatch sw = new Stopwatch();
            //List<BoeTaskElementDTO> originalData = new List<BoeTaskElementDTO>();
            //List<BoeTaskElementDTO> newData = new List<BoeTaskElementDTO>();
            //List<long> originalTimes = new List<long>();
            //List<long> newTimes = new List<long>();

            //List<int> ids = new List<int>();

            //{
            //    int maxItems = 50;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.BOETaskElements.OrderBy(x => new Guid()).Select(x => x.BOETaskElementID).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < 5; i++) { sw.Restart(); originalData.AddRange(loader.GetByIds_OLD(ids)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < 5; i++) { sw.Restart(); newData.AddRange(loader.GetByIds(ids, true)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<BoeTaskElementDTO>(); newData = new List<BoeTaskElementDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();

            //    int maxItems = 20;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.BOETaskElements.Select(x => x.BOEID).Distinct().OrderBy(x => new Guid()).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < 5; i++) { sw.Restart(); originalData.AddRange(loader.GetByBoeIds_OLD(ids.ToCollection()).ToList()); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < 5; i++) { sw.Restart(); newData.AddRange(loader.GetByBoeIds(ids.ToCollection(), true).ToList()); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<BoeTaskElementDTO>(); newData = new List<BoeTaskElementDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();

            //    int maxItems = 15;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.BOEs.Where(x => x.BOETaskElements.Any()).Select(x => x.WorkspaceID).Distinct().OrderBy(x => new Guid()).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < ids.Count; i++) { sw.Restart(); originalData.AddRange(loader.GetByWorkspaceId_OLD(ids.ElementAt(i)).ToList()); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < ids.Count; i++) { sw.Restart(); newData.AddRange(loader.GetByWorkspaceId(ids.ElementAt(i), true).ToList()); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    originalData.SelectMany(x => x.taskElementLabors).ToList().ForEach(x => { x.LaborSpreads = x.LaborSpreads.OrderBy(z => z.Id).ToCollection(); });
            //    newData.SelectMany(x => x.taskElementLabors).ToList().ForEach(x => { x.LaborSpreads = x.LaborSpreads.OrderBy(z => z.Id).ToCollection(); });

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}
        }

        /// <summary>
        /// Tests that RTE loading works as expected
        /// </summary>
        [TestMethod]
        public void TestingRteLoadChanges()
        {
            BoeTaskElementDTODataLoader loader = new BoeTaskElementDTODataLoader(new ResourceTypeLoader(), new ResourceSpreadLoader(), new OrdinaryVariableLoader(),
                                                    new BoeTaskElementCustomFieldValueXREFLoader(), new LaborTypeCustomFieldValueXREFLoader());

            int id = -1;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                id = gbe.BOETaskElements.First(x => x.TaskDescription.Length > 0).BOETaskElementID;
            }

            // A first test:
            // Check the RTE data being correctly loaded/not loaded..
            BoeTaskElementDTO nonRteLoadedBoe = loader.GetByIds(new List<int>() { id }, false, 0, 2).First();

            // make sure RTE data was not loaded by default
            Assert.IsFalse(nonRteLoadedBoe.WasDescriptionSet);
            Assert.IsFalse(nonRteLoadedBoe.WasMoqTextSet);

            // make sure Description is handled correctly
            nonRteLoadedBoe.Description = "blah blah";
            Assert.IsTrue(nonRteLoadedBoe.WasDescriptionSet);
            Assert.IsFalse(nonRteLoadedBoe.WasMoqTextSet);

            // load RTE data, make sure it loads correctly, leaving description alone
            loader.LoadRTEFields(new List<BoeTaskElementDTO>() { nonRteLoadedBoe });
            Assert.IsTrue(nonRteLoadedBoe.WasDescriptionSet);
            Assert.IsTrue(nonRteLoadedBoe.Description == "blah blah");
            Assert.IsTrue(nonRteLoadedBoe.WasMoqTextSet);


            // A second test:
            // do a manual, non RTE load first, then load RTE data after, then do a full RTE load, and compare the two, to make sure it's all the same
            nonRteLoadedBoe = loader.GetByIds(new List<int>() { id }, false, 0, 2).First();
            Assert.IsFalse(nonRteLoadedBoe.WasDescriptionSet);
            loader.LoadRTEFields(new List<BoeTaskElementDTO>() { nonRteLoadedBoe });
            Assert.IsTrue(nonRteLoadedBoe.WasDescriptionSet);
            Assert.IsTrue(nonRteLoadedBoe.WasMoqTextSet);

            BoeTaskElementDTO rteLoadedBoe = loader.GetByIds(new List<int>() { id }, true, 0, 2).First();
            Assert.IsTrue(rteLoadedBoe.WasDescriptionSet);
            Assert.IsTrue(nonRteLoadedBoe.WasMoqTextSet);

            VerifyDtos(nonRteLoadedBoe, rteLoadedBoe);
        }

        public static void VerifyCollections(ICollection<BoeTaskElementDTO> collection1, ICollection<BoeTaskElementDTO> collection2, bool skipFieldsNotRestoredFromBackup = false)
        {
            Assert.AreEqual(collection1.Count, collection2.Count);
            for (int i = 0; i < collection1.Count; i++)
            {
                VerifyDtos(collection1.ElementAt(i), collection2.ElementAt(i), skipFieldsNotRestoredFromBackup);
            }
        }

        public static void VerifyDtos(BoeTaskElementDTO dto1, BoeTaskElementDTO dto2, bool skipFieldsNotRestoredFromBackup = false)
        {
            if(!skipFieldsNotRestoredFromBackup)
            {
                Assert.AreEqual(dto1.BoeID, dto2.BoeID);
                Assert.AreEqual(dto1.Id, dto2.Id);

                // the equation contains variable IDs, so we cannot check it
                Assert.AreEqual(dto1.MOQHoursEquation, dto2.MOQHoursEquation);
            }

            Assert.AreEqual(dto1.BOETaskElementOrder, dto2.BOETaskElementOrder);
            Assert.AreEqual(dto1.BOETaskID, dto2.BOETaskID);
            Assert.AreEqual(dto1.EndDate, dto2.EndDate);
            Assert.AreEqual(dto1.IMS_ID, dto2.IMS_ID);
            Assert.AreEqual(dto1.LaborTypeWarningFlag, dto2.LaborTypeWarningFlag);
            Assert.AreEqual(dto1.MOQType, dto2.MOQType);
            Assert.AreEqual(dto1.MOQTypeName, dto2.MOQTypeName);
            Assert.AreEqual(dto1.StartDate, dto2.StartDate);
            Assert.AreEqual(dto1.TaskElementType, dto2.TaskElementType);
            Assert.AreEqual(dto1.TaskTitle, dto2.TaskTitle);
            Assert.AreEqual(dto1.TotalCost, dto2.TotalCost);
            Assert.AreEqual(dto1.TotalHours, dto2.TotalHours);
            Assert.AreEqual(dto1.UpdateDate, dto2.UpdateDate);
            Assert.AreEqual(dto1.WasDescriptionSet, dto2.WasDescriptionSet);
            Assert.AreEqual(dto1.WasMoqTextSet, dto2.WasMoqTextSet);

            if (dto1.WasDescriptionSet && dto2.WasDescriptionSet)
            {
                Assert.AreEqual(dto1.Description, dto2.Description);
            }

            if (dto1.WasMoqTextSet && dto2.WasMoqTextSet)
            {
                Assert.AreEqual(dto1.MOQText, dto2.MOQText);
            }

            Assert.AreEqual(dto1.WorkspaceVariableIDs.Count, dto2.WorkspaceVariableIDs.Count);
            if (!skipFieldsNotRestoredFromBackup)
            {
                for (int i = 0; i < dto1.WorkspaceVariableIDs.Count; i++)
                {
                    Assert.AreEqual(dto1.WorkspaceVariableIDs.ElementAt(i), dto2.WorkspaceVariableIDs.ElementAt(i));
                }
            }

            Assert.AreEqual(dto1.CustomFieldValueContainers.Count, dto2.CustomFieldValueContainers.Count);
            for (int i = 0; i < dto1.CustomFieldValueContainers.Count; i++)
            {
                if (!skipFieldsNotRestoredFromBackup)
                {
                    Assert.AreEqual(dto1.CustomFieldValueContainers.ElementAt(i).ContainerID, dto2.CustomFieldValueContainers.ElementAt(i).ContainerID);
                    Assert.AreEqual(dto1.CustomFieldValueContainers.ElementAt(i).CustomFieldValueID, dto2.CustomFieldValueContainers.ElementAt(i).CustomFieldValueID);
                    Assert.AreEqual(dto1.CustomFieldValueContainers.ElementAt(i).CustomFieldID, dto2.CustomFieldValueContainers.ElementAt(i).CustomFieldID);
                }

                Assert.AreEqual(dto1.CustomFieldValueContainers.ElementAt(i).UpdateDate, dto2.CustomFieldValueContainers.ElementAt(i).UpdateDate);
                Assert.AreEqual(dto1.CustomFieldValueContainers.ElementAt(i).IsOpenEnded, dto2.CustomFieldValueContainers.ElementAt(i).IsOpenEnded);
                Assert.AreEqual(dto1.CustomFieldValueContainers.ElementAt(i).OpenEndedValue, dto2.CustomFieldValueContainers.ElementAt(i).OpenEndedValue);
            }

            Assert.AreEqual(dto1.OrdinaryVariables.Count, dto2.OrdinaryVariables.Count);
            for (int i = 0; i < dto1.OrdinaryVariables.Count; i++)
            {
                if (!skipFieldsNotRestoredFromBackup)
                {
                    Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).Id, dto2.OrdinaryVariables.ElementAt(i).Id);
                    Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).BoeID, dto2.OrdinaryVariables.ElementAt(i).BoeID);
                    Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).TaskElementId, dto2.OrdinaryVariables.ElementAt(i).TaskElementId);
                }

                Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).DefaultSize, dto2.OrdinaryVariables.ElementAt(i).DefaultSize);
                Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).IsPercentage, dto2.OrdinaryVariables.ElementAt(i).IsPercentage);
                Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).OrdinaryVariableName, dto2.OrdinaryVariables.ElementAt(i).OrdinaryVariableName);
                Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).OrdinaryVariableValue, dto2.OrdinaryVariables.ElementAt(i).OrdinaryVariableValue);
                Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SortBOEBy, dto2.OrdinaryVariables.ElementAt(i).SortBOEBy);
                Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).UpdateDate, dto2.OrdinaryVariables.ElementAt(i).UpdateDate);
                Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).ValueType, dto2.OrdinaryVariables.ElementAt(i).ValueType);
                Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).VariableType, dto2.OrdinaryVariables.ElementAt(i).VariableType);

                Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).TaskElementIds.Count, dto2.OrdinaryVariables.ElementAt(i).TaskElementIds.Count);
                for (int j = 0; j < dto1.OrdinaryVariables.ElementAt(i).TaskElementIds.Count; j++)
                {
                    if (!skipFieldsNotRestoredFromBackup)
                    {

                        Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).TaskElementIds.ElementAt(j),
                                    dto2.OrdinaryVariables.ElementAt(i).TaskElementIds.ElementAt(j));
                    }
                }

                Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SumVariableResourceTypeIDs.Count, dto2.OrdinaryVariables.ElementAt(i).SumVariableResourceTypeIDs.Count);
                if (!skipFieldsNotRestoredFromBackup)
                {
                    for (int j = 0; j < dto1.OrdinaryVariables.ElementAt(i).SumVariableResourceTypeIDs.Count; j++)
                    {
                        Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SumVariableResourceTypeIDs.ElementAt(j),
                                        dto2.OrdinaryVariables.ElementAt(i).SumVariableResourceTypeIDs.ElementAt(j));
                    }
                }

                Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.Count, dto2.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.Count);
                for (int j = 0; j < dto1.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.Count; j++)
                {
                    if (!skipFieldsNotRestoredFromBackup)
                    {
                        Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).BoeID,
                                    dto2.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).BoeID);
                        Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).CLINID,
                                        dto2.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).CLINID);
                        Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).Id,
                                        dto2.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).Id);
                        Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).OrdinaryVariableID,
                                        dto2.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).OrdinaryVariableID);
                        Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).OVSumID,
                                        dto2.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).OVSumID);
                        Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).UpdateDate,
                                        dto2.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).UpdateDate);
                        Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).WBSID,
                                        dto2.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).WBSID);
                    }

                    Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).ChildBoeIDs.Count,
                        dto2.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).ChildBoeIDs.Count);
                    for (int k = 0; k < dto1.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).ChildBoeIDs.Count; k++)
                    {
                        if (!skipFieldsNotRestoredFromBackup)
                        {

                            Assert.AreEqual(dto1.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).ChildBoeIDs.ElementAt(k),
                                        dto2.OrdinaryVariables.ElementAt(i).SelectedBOEsToSum.ElementAt(j).ChildBoeIDs.ElementAt(k));
                        }
                    }
                }
            }

            Assert.AreEqual(dto1.taskElementLabors.Count, dto2.taskElementLabors.Count);
            for (int i = 0; i < dto1.taskElementLabors.Count; i++)
            {
                if (!skipFieldsNotRestoredFromBackup)
                {
                    Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).BoeID, dto2.taskElementLabors.ElementAt(i).BoeID);
                    Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).Id, dto2.taskElementLabors.ElementAt(i).Id);
                    Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).PerformingOrgID, dto2.taskElementLabors.ElementAt(i).PerformingOrgID);
                    Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).ResourceID, dto2.taskElementLabors.ElementAt(i).ResourceID);
                    Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).TaskElementId, dto2.taskElementLabors.ElementAt(i).TaskElementId);
                    Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).LaborSortID, dto2.taskElementLabors.ElementAt(i).LaborSortID);
                }

                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).EndDate, dto2.taskElementLabors.ElementAt(i).EndDate);
                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).EndDateValue, dto2.taskElementLabors.ElementAt(i).EndDateValue);
                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).HourSpreadLocked, dto2.taskElementLabors.ElementAt(i).HourSpreadLocked);
                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).PercentSpread, dto2.taskElementLabors.ElementAt(i).PercentSpread);
                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).PercentSpreadLocked, dto2.taskElementLabors.ElementAt(i).PercentSpreadLocked);
                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).SpreadCurveID, dto2.taskElementLabors.ElementAt(i).SpreadCurveID);
                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).SpreadType, dto2.taskElementLabors.ElementAt(i).SpreadType);
                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).StartDate, dto2.taskElementLabors.ElementAt(i).StartDate);
                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).StartDateValue, dto2.taskElementLabors.ElementAt(i).StartDateValue);
                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).UpdateDate, dto2.taskElementLabors.ElementAt(i).UpdateDate);
                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).ValueSpread, dto2.taskElementLabors.ElementAt(i).ValueSpread);
                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).CanOffload, dto2.taskElementLabors.ElementAt(i).CanOffload);
                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).TieredPercentage, dto2.taskElementLabors.ElementAt(i).TieredPercentage);

                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).CustomFieldValueContainers.Count, dto2.taskElementLabors.ElementAt(i).CustomFieldValueContainers.Count);
                for (int j = 0; j < dto1.taskElementLabors.ElementAt(i).CustomFieldValueContainers.Count; j++)
                {
                    if (!skipFieldsNotRestoredFromBackup)
                    {
                        Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).CustomFieldValueContainers.ElementAt(j).ContainerID,
                                    dto2.taskElementLabors.ElementAt(i).CustomFieldValueContainers.ElementAt(j).ContainerID);
                        Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).CustomFieldValueContainers.ElementAt(j).CustomFieldValueID,
                                        dto2.taskElementLabors.ElementAt(i).CustomFieldValueContainers.ElementAt(j).CustomFieldValueID);
                        Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).CustomFieldValueContainers.ElementAt(j).CustomFieldID,
                                        dto2.taskElementLabors.ElementAt(i).CustomFieldValueContainers.ElementAt(j).CustomFieldID);
                    }

                    Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).CustomFieldValueContainers.ElementAt(j).UpdateDate,
                                    dto2.taskElementLabors.ElementAt(i).CustomFieldValueContainers.ElementAt(j).UpdateDate);
                    Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).CustomFieldValueContainers.ElementAt(j).IsOpenEnded,
                                    dto2.taskElementLabors.ElementAt(i).CustomFieldValueContainers.ElementAt(j).IsOpenEnded);
                    Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).CustomFieldValueContainers.ElementAt(j).OpenEndedValue,
                                    dto2.taskElementLabors.ElementAt(i).CustomFieldValueContainers.ElementAt(j).OpenEndedValue);
                }

                Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).LaborSpreads.Count, dto2.taskElementLabors.ElementAt(i).LaborSpreads.Count);
                for (int j = 0; j < dto1.taskElementLabors.ElementAt(i).LaborSpreads.Count; j++)
                {
                    if (!skipFieldsNotRestoredFromBackup)
                    {
                        Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).LaborSpreads.ElementAt(j).BoeID,
                                    dto2.taskElementLabors.ElementAt(i).LaborSpreads.ElementAt(j).BoeID);
                        Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).LaborSpreads.ElementAt(j).Id,
                                        dto2.taskElementLabors.ElementAt(i).LaborSpreads.ElementAt(j).Id);
                        Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).LaborSpreads.ElementAt(j).LaborTypeId,
                                        dto2.taskElementLabors.ElementAt(i).LaborSpreads.ElementAt(j).LaborTypeId);
                    }

                    Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).LaborSpreads.ElementAt(j).LaborSpreadDate,
                                    dto2.taskElementLabors.ElementAt(i).LaborSpreads.ElementAt(j).LaborSpreadDate);
                    Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).LaborSpreads.ElementAt(j).LaborSpreadValue,
                                    dto2.taskElementLabors.ElementAt(i).LaborSpreads.ElementAt(j).LaborSpreadValue);
                    Assert.AreEqual(dto1.taskElementLabors.ElementAt(i).LaborSpreads.ElementAt(j).UpdateDate,
                                    dto2.taskElementLabors.ElementAt(i).LaborSpreads.ElementAt(j).UpdateDate);
                }
            }
        }
    }
}