// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
	using GenBOE.DataBridge.DTO.SkillMixSummary;

	[TestClass]
    public class WorkspaceVariableDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_GetWorkspaceVarIDsByWorkspaceID()
        {
            var sut = new WorkspaceVariableDTODataLoader();

            // create a workspace variable so we always have one
            string workspaceVarName = Guid.NewGuid().ToString().Substring(0, 20);
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = -1, WorkspaceID = Workspace.Id, Updateable = UpdateType.Upsert, WorkspaceVariableName = workspaceVarName, WorkspaceVariableValue = 5.0m, UpdateDate = DateTime.Now };
            Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { workspaceVar };
            sut.SaveWorkspaceVariables(workspaceVars);

            Collection<int> workspaceVarIDs = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection();
            Assert.IsTrue(workspaceVarIDs.Count > 0, "No workspace variables exist");
        }
        [TestMethod]
        public void L_GetWorkspaceVariableByWorkspaceVarID()
        {
            var sut = new WorkspaceVariableDTODataLoader();

            Collection<int> workspaceVarIDs = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection();

            WorkspaceVariableDTO workspaceVariable = sut.GetById(workspaceVarIDs[0]);
            Assert.IsTrue(workspaceVariable != null, "Workspace variable is null");
        }

        [TestMethod]
        public void L_GetBOEIDsUsingWorkspaceVarID()
        {
            IResourceTypeLoader resourceTypeLoader = new ResourceTypeLoader();
            IResourceSpreadLoader resourceSpreadLoader = new ResourceSpreadLoader();
            IOrdinaryVariableLoader ordinaryVariableLoader = new OrdinaryVariableLoader();
            IBoeTaskElementCustomFieldValueXREFLoader taskElementCustomFieldLoader = new BoeTaskElementCustomFieldValueXREFLoader();
            ILaborTypeCustomFieldValueXREFLoader laborTypeCustomFieldLoader = new LaborTypeCustomFieldValueXREFLoader();
			ISkillMixDTOLoader skillMixDTOLoader = new SkillMixDTOLoader();
			ICommonDisclosureSMDTODataLoader commonDisclosureSMDTODataLoader = new CommonDisclosureSMDTODataLoader();
			ISkillMixSummaryDTOLoader skillMixSummaryDTOLoader = new SkillMixSummaryDTOLoader();

			BoeTaskElementDTODataLoader _BoeTaskElementDTODataLoader = new BoeTaskElementDTODataLoader(resourceTypeLoader, resourceSpreadLoader, ordinaryVariableLoader, taskElementCustomFieldLoader, laborTypeCustomFieldLoader, skillMixDTOLoader, skillMixSummaryDTOLoader, commonDisclosureSMDTODataLoader);

			WorkspaceVariableDTODataLoader sut = new WorkspaceVariableDTODataLoader();

            Collection<int> workspaceVarIDs = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection();

            // Create a new task element

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
            newElement.BoeID = Boe1.Id;

            // associate workspace variable from above
            Collection<int> WorkspaceVars = new Collection<int>();
            WorkspaceVars.Add(workspaceVarIDs[0]);

            newElement.WorkspaceVariableIDs = WorkspaceVars;

            _BoeTaskElementDTODataLoader.CreateOrSaveTaskElementDetail(Boe1.Id, newElement);

            Collection<int> boes = sut.GetBOEIDsUsingWorkspaceVarID(workspaceVarIDs[0]);
            Assert.IsTrue(boes.Count > 0, "Workspace variable is not referenced in any BOEs");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_SaveWorkspaceVariables()
        {
            var sut = new WorkspaceVariableDTODataLoader();

            // workspace variable name has to be unique
            string workspaceVarName = Guid.NewGuid().ToString().Substring(0, 20);
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = -1, WorkspaceID = Workspace.Id, Updateable = UpdateType.Upsert, WorkspaceVariableName = workspaceVarName, WorkspaceVariableValue = 5.0m, UpdateDate = DateTime.Now };
            Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { workspaceVar };

            // before the Save
            int BeforeCount = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection().Count;
            sut.SaveWorkspaceVariables(workspaceVars);
            int AfterCount = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection().Count;

            Assert.AreEqual(BeforeCount + 1, AfterCount, "The Save did not work");

            this.ResetTestData();

        }

        [TestMethod]
        public void L_DeleteWorkspaceVariable()
        {
            var sut = new WorkspaceVariableDTODataLoader();

            // Save a workspace variable so we're always sure there is one to delete
            // workspace variable name has to be unique
            string workspaceVarName = Guid.NewGuid().ToString().Substring(0, 20);
            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = -1, WorkspaceID = Workspace.Id, Updateable = UpdateType.Upsert, WorkspaceVariableName = workspaceVarName, WorkspaceVariableValue = 5.0m, UpdateDate = DateTime.Now };
            Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { workspaceVar };

            sut.SaveWorkspaceVariables(workspaceVars);

            // clear out the above workspace variables collection
            workspaceVars = new Collection<WorkspaceVariableDTO>();
            Collection<int> workspaceVarIDs = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection();
            int BeforeCount = workspaceVarIDs.Count;
            workspaceVar = sut.GetById(workspaceVarIDs[0]);
            workspaceVar.Updateable = UpdateType.Deleted;
            workspaceVars.Add(workspaceVar);

            sut.SaveWorkspaceVariables(workspaceVars);

            int AfterCount = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection().Count;

            Assert.AreEqual(BeforeCount - 1, AfterCount, "The Delete did not work");

            this.ResetTestData();


        }

        [TestMethod]
        public void L_SaveWorkspaceVariablesWithSelectOfSumWBS()
        {
            var sut = new WorkspaceVariableDTODataLoader();


            // workspace variable name has to be unique
            string workspaceVarName = Guid.NewGuid().ToString().Substring(0, 20);

            // set up a select boes to sum workspace variable
            Collection<SelectBOEsToSum> boesToSum = new Collection<SelectBOEsToSum>();
            SelectBOEsToSum boeToSum = new SelectBOEsToSum();
            boeToSum.WBSID = Wbs.Id;
            boeToSum.BoeID = null;
            boeToSum.CLINID = null;

            boesToSum.Add(boeToSum);

            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = -1, WorkspaceID = Workspace.Id, Updateable = UpdateType.Upsert, WorkspaceVariableName = workspaceVarName, WorkspaceVariableValue = 5.0m, UpdateDate = DateTime.Now, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = boesToSum };
            Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { workspaceVar };

            // before the Save
            int BeforeCount = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection().Count;
            sut.SaveWorkspaceVariables(workspaceVars);
            int AfterCount = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection().Count;

            Assert.AreEqual(BeforeCount + 1, AfterCount, "The Save did not work");

            this.ResetTestData();

        }

        [TestMethod]
        public void L_SaveWorkspaceVariablesWithSelectOfSumCLIN()
        {
            var sut = new WorkspaceVariableDTODataLoader();
            var clinDL = new ClinDTODataLoader();

            // workspace variable name has to be unique
            string workspaceVarName = Guid.NewGuid().ToString().Substring(0, 20);

            // set up a select boes to sum workspace variable
            Collection<SelectBOEsToSum> boesToSum = new Collection<SelectBOEsToSum>();
            SelectBOEsToSum boeToSum = new SelectBOEsToSum();
            boeToSum.CLINID = clinDL.GetByWorkspaceId(Workspace.Id).Select(x => x.Id).First(); // CLIN will be the summary ID

            boeToSum.BoeID = null;
            boeToSum.WBSID = null;
            boesToSum.Add(boeToSum);

            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = -1, WorkspaceID = Workspace.Id, Updateable = UpdateType.Upsert, WorkspaceVariableName = workspaceVarName, WorkspaceVariableValue = 5.0m, UpdateDate = DateTime.Now, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.CLIN, SelectedBOEsToSum = boesToSum };
            Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { workspaceVar };

            // before the Save
            int BeforeCount = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection().Count;
            sut.SaveWorkspaceVariables(workspaceVars);
            int AfterCount = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection().Count;

            Assert.AreEqual(BeforeCount + 1, AfterCount, "The Save did not work");
            this.ResetTestData();
        }

        [TestMethod]
        public void L_DeleteWorkspaceVariableWithSelectOfSum()
        {
            var sut = new WorkspaceVariableDTODataLoader();
            var clinDL = new ClinDTODataLoader();

            // Save a workspace variable so we're always sure there is one to delete
            // workspace variable name has to be unique
            string workspaceVarName = Guid.NewGuid().ToString().Substring(0, 20);
            // set up a select boes to sum workspace variable
            Collection<SelectBOEsToSum> boesToSum = new Collection<SelectBOEsToSum>();
            SelectBOEsToSum boeToSum = new SelectBOEsToSum();
            boeToSum.WBSID = Wbs.Id; // WBS will be the summary ID
            boeToSum.BoeID = Boe1.Id;
            boeToSum.CLINID = clinDL.GetByWorkspaceId(Workspace.Id).Select(x => x.Id).First(); // get a CLIN ID based off of the global
            boesToSum.Add(boeToSum);

            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = -1, WorkspaceID = Workspace.Id, Updateable = UpdateType.Upsert, WorkspaceVariableName = workspaceVarName, WorkspaceVariableValue = 5.0m, UpdateDate = DateTime.Now, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = boesToSum };
            Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { workspaceVar };

            sut.SaveWorkspaceVariables(workspaceVars);

            // clear out the above workspace variables collection
            workspaceVars = new Collection<WorkspaceVariableDTO>();
            Collection<int> workspaceVarIDs = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection();

            foreach (int x in workspaceVarIDs)
            {
                WorkspaceVariableDTO wv = sut.GetById(x);
                if (wv.ValueType.Equals(VarValueType.SumOfBOEs))
                {
                    workspaceVar = wv;
                    break;
                }
            }

            // test that a workspace variable can change from SumOfBoe to Discrete
            int workspaceVarID = workspaceVar.Id;
            Collection<SelectBOEsToSum> updatedboesToSumDelete = new Collection<SelectBOEsToSum>();
            workspaceVar.ValueType = VarValueType.Discrete;
            workspaceVar.SelectedBOEsToSum = updatedboesToSumDelete;
            workspaceVar.Updateable = UpdateType.Upsert;
            workspaceVars.Add(workspaceVar);

            sut.SaveWorkspaceVariables(workspaceVars);

            WorkspaceVariableDTO updatedVar = sut.GetById(workspaceVarID);
            // all the selected sums have been deleted
            Assert.AreEqual(updatedVar.SelectedBOEsToSum.Count, 0, "The update did not work");
            Assert.AreEqual(updatedVar.ValueType, VarValueType.Discrete, "The update did not work");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_WorkspaceVariableWithVariableResourceTypes()
        {
            var sut = new WorkspaceVariableDTODataLoader();

            // Save a workspace variable so we're always sure there is one to delete
            // workspace variable name has to be unique
            string workspaceVarName = Guid.NewGuid().ToString().Substring(0, 20);


            Collection<int> resourceTypeIDs = new Collection<int> { (int)SumVariableResourceType.SSCLMLabor, (int)SumVariableResourceType.SSCLOESub };


            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = -1, WorkspaceID = Workspace.Id, Updateable = UpdateType.Upsert, WorkspaceVariableName = workspaceVarName, WorkspaceVariableValue = 5.0m, UpdateDate = DateTime.Now, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SumVariableResourceTypeIDs = resourceTypeIDs };
            Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { workspaceVar };

            sut.SaveWorkspaceVariables(workspaceVars);

            // clear out the above workspace variables collection
            workspaceVars = new Collection<WorkspaceVariableDTO>();
            Collection<int> workspaceVarIDs = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection();

            foreach (int x in workspaceVarIDs)
            {
                WorkspaceVariableDTO wv = sut.GetById(x);
                if (wv.ValueType.Equals(VarValueType.SumOfBOEs))
                {
                    workspaceVar = wv;
                    break;
                }
            }

            // test that a workspace variable can change from SumOfBoe to Discrete
            int workspaceVarID = workspaceVar.Id;
            Collection<SelectBOEsToSum> updatedboesToSumDelete = new Collection<SelectBOEsToSum>();
            workspaceVar.ValueType = VarValueType.Discrete;
            workspaceVar.SelectedBOEsToSum = updatedboesToSumDelete;
            workspaceVar.SumVariableResourceTypeIDs = new Collection<int>(); // the loader just saves whatever we give it, so we need to clear these out..
            workspaceVar.Updateable = UpdateType.Upsert;
            workspaceVars.Add(workspaceVar);

            sut.SaveWorkspaceVariables(workspaceVars);

            WorkspaceVariableDTO updatedVar = sut.GetById(workspaceVarID);
            // all the selected sums have been deleted
            Assert.AreEqual(updatedVar.SumVariableResourceTypeIDs.Count, 0, "The update did not work");
            Assert.AreEqual(updatedVar.ValueType, VarValueType.Discrete, "The update did not work");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_DeleteVariableWithSelectOfSum()
        {
            var sut = new WorkspaceVariableDTODataLoader();
            var clinDL = new ClinDTODataLoader();

            // Save a workspace variable so we're always sure there is one to delete
            // workspace variable name has to be unique
            string workspaceVarName = Guid.NewGuid().ToString().Substring(0, 20);
            // set up a select boes to sum workspace variable
            Collection<SelectBOEsToSum> boesToSum = new Collection<SelectBOEsToSum>();
            SelectBOEsToSum boeToSum = new SelectBOEsToSum();
            boeToSum.WBSID = Wbs.Id; // WBS will be the summary ID
            boeToSum.BoeID = Boe1.Id;
            boeToSum.CLINID = clinDL.GetByWorkspaceId(Workspace.Id).Select(x => x.Id).First(); // get a CLIN ID based off of the global
            boesToSum.Add(boeToSum);
            Collection<int> resourceTypeIDs = new Collection<int> { (int)SumVariableResourceType.SSCLMLabor, (int)SumVariableResourceType.SSCLOESub };

            WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { Id = -1, WorkspaceID = Workspace.Id, Updateable = UpdateType.Upsert, WorkspaceVariableName = workspaceVarName, WorkspaceVariableValue = 5.0m, UpdateDate = DateTime.Now, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = boesToSum, SumVariableResourceTypeIDs = resourceTypeIDs };
            Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { workspaceVar };

            sut.SaveWorkspaceVariables(workspaceVars);

            // clear out the above workspace variables collection
            workspaceVars = new Collection<WorkspaceVariableDTO>();
            Collection<int> workspaceVarIDs = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection();

            foreach (int x in workspaceVarIDs)
            {
                WorkspaceVariableDTO wv = sut.GetById(x);
                if (wv.ValueType.Equals(VarValueType.SumOfBOEs) && wv.SumVariableResourceTypeIDs.Count > 0)
                {
                    workspaceVar = wv;
                    break;
                }
            }

            int beforeDelete = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection().Count;
            workspaceVar.Updateable = UpdateType.Deleted;
            workspaceVars.Add(workspaceVar);

            sut.SaveWorkspaceVariables(workspaceVars);

            int AfterDelete = sut.GetByWorkspaceID(Workspace.Id).Select(x => x.Id).ToCollection().Count;

            Assert.AreEqual(beforeDelete - 1, AfterDelete, "The delete didn't work");

            this.ResetTestData();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "newAvg"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "originalAvg")]
        //[TestMethod]
        public void TestOldVsNew()
        {
            //WorkspaceVariableDTODataLoader loader = new WorkspaceVariableDTODataLoader();

            //Stopwatch sw = new Stopwatch();
            //List<WorkspaceVariableDTO> originalData = new List<WorkspaceVariableDTO>();
            //List<WorkspaceVariableDTO> newData = new List<WorkspaceVariableDTO>();
            //List<long> originalTimes = new List<long>();
            //List<long> newTimes = new List<long>();

            //List<int> ids = new List<int>();

            //{
            //    int maxItems = 50;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.Workspaces.Where(w => w.WorkspaceVariables.Any()).OrderBy(x => new Guid()).Select(x => x.WorkspaceID).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); originalData.AddRange(loader.GetByWorkspaceID_OLD(ids.ElementAt(i))); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); newData.AddRange(loader.GetByWorkspaceID(ids.ElementAt(i))); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<WorkspaceVariableDTO>(); newData = new List<WorkspaceVariableDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();

            //    int maxItems = 15;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.WorkspaceVariables.OrderBy(x => new Guid()).Select(x => x.WorkspaceVariableID).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < 5; i++) { sw.Restart(); originalData.AddRange(loader.GetByIds_OLD(ids)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < 5; i++) { sw.Restart(); newData.AddRange(loader.GetByIds(ids)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}
        }

        public static void VerifyCollections(ICollection<WorkspaceVariableDTO> collection1, ICollection<WorkspaceVariableDTO> collection2, bool skipFieldsNotRestoredFromBackup = false)
        {
            Assert.AreEqual(collection1.Count, collection2.Count);
            for (int i = 0; i < collection1.Count; i++)
            {
                collection1.ElementAt(i).TaskElementIds = collection1.ElementAt(i).TaskElementIds.OrderBy(x => x).ToCollection();
                collection2.ElementAt(i).TaskElementIds = collection2.ElementAt(i).TaskElementIds.OrderBy(x => x).ToCollection();

                collection1.ElementAt(i).SumVariableResourceTypeIDs = collection1.ElementAt(i).SumVariableResourceTypeIDs.OrderBy(x => x).ToCollection();
                collection2.ElementAt(i).SumVariableResourceTypeIDs = collection2.ElementAt(i).SumVariableResourceTypeIDs.OrderBy(x => x).ToCollection();

                VerifyDtos(collection1.ElementAt(i), collection2.ElementAt(i), skipFieldsNotRestoredFromBackup);
            }
        }

        public static void VerifyDtos(WorkspaceVariableDTO dto1, WorkspaceVariableDTO dto2, bool skipFieldsNotRestoredFromBackup = false)
        {
            if (!skipFieldsNotRestoredFromBackup)
            {
                Assert.AreEqual(dto1.Id, dto2.Id);
                Assert.AreEqual(dto1.WorkspaceID, dto2.WorkspaceID);
            }

            Assert.AreEqual(dto1.InUse, dto2.InUse);
            Assert.AreEqual(dto1.IsPercentage, dto2.IsPercentage);
            Assert.AreEqual(dto1.SortBOEBy, dto2.SortBOEBy);
            Assert.AreEqual(dto1.Updateable, dto2.Updateable);
            Assert.AreEqual(dto1.UpdateDate, dto2.UpdateDate);
            Assert.AreEqual(dto1.ValueType, dto2.ValueType);
            Assert.AreEqual(dto1.WorkspaceVariableName, dto2.WorkspaceVariableName);
            Assert.AreEqual(dto1.WorkspaceVariableValue, dto2.WorkspaceVariableValue);

            Assert.AreEqual(dto1.SumVariableResourceTypeIDs.Count, dto2.SumVariableResourceTypeIDs.Count);
            if (!skipFieldsNotRestoredFromBackup)
            {
                for (int i = 0; i < dto1.SumVariableResourceTypeIDs.Count; i++)
                {
                    Assert.AreEqual(dto1.SumVariableResourceTypeIDs.ElementAt(i), dto2.SumVariableResourceTypeIDs.ElementAt(i));
                }
            }

            Assert.AreEqual(dto1.TaskElementIds.Count, dto2.TaskElementIds.Count);
            if (!skipFieldsNotRestoredFromBackup)
            {
                for (int i = 0; i < dto1.TaskElementIds.Count; i++)
                {
                    Assert.AreEqual(dto1.TaskElementIds.ElementAt(i), dto2.TaskElementIds.ElementAt(i));
                }
            }

            Assert.AreEqual(dto1.SelectedBOEsToSum.Count, dto2.SelectedBOEsToSum.Count);
            for (int i = 0; i < dto1.SelectedBOEsToSum.Count; i++)
            {
                if (!skipFieldsNotRestoredFromBackup)
                {
                    Assert.AreEqual(dto1.SelectedBOEsToSum.ElementAt(i).BoeID, dto2.SelectedBOEsToSum.ElementAt(i).BoeID);
                    Assert.AreEqual(dto1.SelectedBOEsToSum.ElementAt(i).CLINID, dto2.SelectedBOEsToSum.ElementAt(i).CLINID);
                    Assert.AreEqual(dto1.SelectedBOEsToSum.ElementAt(i).Id, dto2.SelectedBOEsToSum.ElementAt(i).Id);
                    Assert.AreEqual(dto1.SelectedBOEsToSum.ElementAt(i).OrdinaryVariableID, dto2.SelectedBOEsToSum.ElementAt(i).OrdinaryVariableID);
                    Assert.AreEqual(dto1.SelectedBOEsToSum.ElementAt(i).OVSumID, dto2.SelectedBOEsToSum.ElementAt(i).OVSumID);
                    Assert.AreEqual(dto1.SelectedBOEsToSum.ElementAt(i).WBSID, dto2.SelectedBOEsToSum.ElementAt(i).WBSID);
                }

                Assert.AreEqual(dto1.SelectedBOEsToSum.ElementAt(i).Updateable, dto2.SelectedBOEsToSum.ElementAt(i).Updateable);
                Assert.AreEqual(dto1.SelectedBOEsToSum.ElementAt(i).UpdateDate, dto2.SelectedBOEsToSum.ElementAt(i).UpdateDate);

                Assert.AreEqual(dto1.SelectedBOEsToSum.ElementAt(i).ChildBoeIDs.Count, dto2.SelectedBOEsToSum.ElementAt(i).ChildBoeIDs.Count);
                if (!skipFieldsNotRestoredFromBackup)
                {
                    for (int j = 0; j < dto1.SelectedBOEsToSum.ElementAt(i).ChildBoeIDs.Count; j++)
                    {
                        Assert.AreEqual(dto1.SelectedBOEsToSum.ElementAt(i).ChildBoeIDs.ElementAt(j), dto2.SelectedBOEsToSum.ElementAt(i).ChildBoeIDs.ElementAt(j));
                    }
                }
            }
        }
    }
}
