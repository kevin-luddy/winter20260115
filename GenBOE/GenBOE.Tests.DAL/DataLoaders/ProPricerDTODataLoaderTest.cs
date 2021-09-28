// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.DataBridge.DTO;
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Common;

    [TestClass]
    public class ProPricerDTODataLoaderTest
    {
        [TestMethod]
        public void GetAllSystemProPricerExports()
        {
            var sut = new ProPricerDTODataLoader();

            ICollection<ProPricerDTO> all = sut.GetAllSystemExports();
            Assert.IsNotNull(all);
            Assert.IsTrue(all.Any());
        }

        [TestMethod]
        public void InsertDeleteSystemProPricerExport()
        {
            var sut = new ProPricerDTODataLoader();

            // create new system  pro pricer export
            ProPricerDTO pp = new ProPricerDTO();
            pp.ExportID = -1;
            pp.FormatName = "MOCK PP System" + DateTime.Now.ToString();
            pp.Updateable = UpdateType.Upsert;
            pp.UpdateDate = DateTime.Now;
            pp.Scope = IES.Common.ProPricerScope.System;
            pp.ProPricerTasks.Add(new ProPricerTasks
            {
                CustomFieldName = "Custom 1",
                ListOrder = 1,
                Selection = ProPricerCustomFieldSelection.CustomFieldID
            });
            pp.ProPricerTasks.Add(new ProPricerTasks
            {
                Task = ProPricerField_Task.BOETitle,
                ListOrder = 2
            });
            pp.ProPricerResources.Add(new ProPricerResources
            {
                CustomFieldName = "Custom 2",
                ListOrder = 2,
                Selection = ProPricerCustomFieldSelection.CustomFieldID
            });
            pp.ProPricerResources.Add(new ProPricerResources
            {
                Resource = ProPricerField_Resources.CLINNumber,
                ListOrder = 1
            });

            sut.SaveSystemProPricerExport(pp);

            ICollection<ProPricerDTO> all = sut.GetAllSystemExports();
            ProPricerDTO saved = all.FirstOrDefault(p => p.Scope == ProPricerScope.System && p.FormatName == pp.FormatName);
            Assert.IsNotNull(saved);
            Assert.AreEqual(2, saved.ProPricerResources.Count);
            Assert.AreEqual(2, saved.ProPricerTasks.Count);
            Assert.IsTrue(saved.ProPricerResources.Any(p => p.CustomFieldName == "Custom 2" && p.ListOrder == 2 && p.Selection == ProPricerCustomFieldSelection.CustomFieldID));
            Assert.IsTrue(saved.ProPricerTasks.Any(p => p.CustomFieldName == "Custom 1" && p.ListOrder == 1 && p.Selection == ProPricerCustomFieldSelection.CustomFieldID));
            Assert.IsTrue(saved.ProPricerResources.Any(p => p.Resource == ProPricerField_Resources.CLINNumber && p.ListOrder == 1));
            Assert.IsTrue(saved.ProPricerTasks.Any(p => p.Task == ProPricerField_Task.BOETitle && p.ListOrder == 2));

            // update tasks and resources
            saved.ProPricerResources.Clear();
            saved.ProPricerTasks.Clear();
            saved.ProPricerTasks.Add(new ProPricerTasks
            {
                CustomFieldName = "Custom 3",
                ListOrder = 1,
                Selection = ProPricerCustomFieldSelection.CustomFieldDescription
            });
            saved.ProPricerResources.Add(new ProPricerResources
            {
                Resource = ProPricerField_Resources.CLINTitle,
                ListOrder = 1
            });

            saved.FormatName = "MOCK PP System Updated";
            saved.Updateable = UpdateType.Upsert;
            sut.SaveSystemProPricerExport(saved);

            ProPricerDTO updated = sut.GetById(saved.ExportID, ProPricerScope.System);
            Assert.IsNotNull(updated);
            Assert.AreEqual(saved.FormatName, updated.FormatName);
            Assert.IsTrue(updated.ProPricerTasks.Any(p => p.CustomFieldName == "Custom 3" && p.ListOrder == 1 && p.Selection == ProPricerCustomFieldSelection.CustomFieldDescription));
            Assert.IsTrue(updated.ProPricerResources.Any(p => p.Resource == ProPricerField_Resources.CLINTitle && p.ListOrder == 1));

            updated.Updateable = UpdateType.Deleted;
            sut.SaveSystemProPricerExport(updated);

            all = sut.GetAllSystemExports();
            saved = all.FirstOrDefault(p => p.UpdateDate == pp.UpdateDate && p.Scope == ProPricerScope.System && p.FormatName == pp.FormatName);
            Assert.IsNull(saved);
        }

        [TestMethod]
        public void L_GetProPricerExportsByWorkspaceID()
        {
            var sut = new ProPricerDTODataLoader();

            // create new workspace pro pricer export
            ProPricerDTO pp = new ProPricerDTO();
            pp.ExportID = -1;
            pp.FormatName = "MOCK PP ";
            pp.Updateable = UpdateType.Upsert;
            pp.UpdateDate = DateTime.Now;
            pp.WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID;
            pp.Scope = IES.Common.ProPricerScope.Workspace;
            sut.SaveProPricerExport(pp);

            int count = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID).Count;
            Assert.IsTrue(count > 0, "No system exports could be found");
        }

        [TestMethod]
        public void L_GetProPricerExportByExportID()
        {
            var sut = new ProPricerDTODataLoader();

            // create new system  pro pricer export
            ProPricerDTO pp = new ProPricerDTO();
            string guidstring = "MOCK" + Guid.NewGuid().ToString();
            pp.ExportID = -1;
            pp.FormatName = guidstring;
            pp.Updateable = UpdateType.Upsert;
            pp.UpdateDate = DateTime.Now;
            pp.WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID;
            pp.Scope = IES.Common.ProPricerScope.Workspace;
            sut.SaveProPricerExport(pp);

            ICollection<ProPricerDTO> exports = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);
            ProPricerDTO pp2 = (from e in exports
                                where e.FormatName == guidstring
                                select e).First();
            ProPricerDTO export = sut.GetById(pp2.ExportID, ProPricerScope.Workspace);

            Assert.IsTrue(export.FormatName == guidstring, "The format name did not match");
            Assert.IsTrue(export.Scope == IES.Common.ProPricerScope.Workspace, "The scope was not workspace");
            Assert.IsTrue(export.ExportID > 0, "The export ID is valid");
        }

        [TestMethod]
        ///This method tests a brand new save, an edit, and a delete
        public void L_SaveProPricerExport()
        {
            var sut = new ProPricerDTODataLoader();

            // create new system  pro pricer export
            ProPricerDTO pp = new ProPricerDTO();
            string guidstring = "MOCK" + Guid.NewGuid().ToString();
            pp.ExportID = -1;
            pp.FormatName = guidstring;
            pp.Updateable = UpdateType.Upsert;
            pp.UpdateDate = DateTime.Now;
            pp.WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID;
            pp.Scope = IES.Common.ProPricerScope.Workspace;

            ProPricerTasks ppTask1 = new ProPricerTasks();
            ppTask1.ListOrder = 0;
            ppTask1.Task = IES.Common.ProPricerField_Task.BOEStartDate;

            ProPricerTasks ppTask2 = new ProPricerTasks();
            ppTask2.ListOrder = 1;
            ppTask2.Task = IES.Common.ProPricerField_Task.BLANK;

            ProPricerTasks ppTask3 = new ProPricerTasks();
            ppTask3.ListOrder = 2;
            ppTask3.Task = IES.Common.ProPricerField_Task.TOTAL;

            ProPricerTasks ppTask4 = new ProPricerTasks();
            ppTask4.ListOrder = 3;
            ppTask4.Task = IES.Common.ProPricerField_Task.CLINNumber;

            Collection<ProPricerTasks> tasks = new Collection<ProPricerTasks> { ppTask1, ppTask2, ppTask3, ppTask4 };

            pp.ProPricerTasks = tasks;

            ProPricerResources ppResource = new ProPricerResources();
            ppResource.ListOrder = 0;
            ppResource.Resource = IES.Common.ProPricerField_Resources.IMSCode;

            ProPricerResources ppResource2 = new ProPricerResources();
            ppResource2.ListOrder = 1;
            ppResource2.Resource = IES.Common.ProPricerField_Resources.ResourceID;

            Collection<ProPricerResources> resources = new Collection<ProPricerResources> { ppResource, ppResource2};
            pp.ProPricerResources = resources;

            sut.SaveProPricerExport(pp);

            ICollection<ProPricerDTO> exports = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);

            ProPricerDTO pp2 = (from e in exports
                                where e.FormatName == guidstring
                                select e).First();

            // Assert the save did work
            Assert.IsTrue(pp2.FormatName == guidstring, "The format name did not match");
            Assert.IsTrue(pp2.Scope == IES.Common.ProPricerScope.Workspace, "The scope was not workspace");
            Assert.IsTrue(pp2.ExportID > 0, "The export ID is valid");
            Assert.IsTrue(pp2.ProPricerResources.Count == 2, "Resources count was not right");
            Assert.IsTrue(pp2.ProPricerTasks.Count == 4, "Task count was not right");

            // Edit the saved export
            guidstring = Guid.NewGuid().ToString();
            pp2.Updateable = UpdateType.Upsert;
            pp2.FormatName = guidstring;
            sut.SaveProPricerExport(pp2);

           ProPricerDTO editedPP = sut.GetById(pp2.ExportID, ProPricerScope.Workspace);
           Assert.IsTrue(editedPP.FormatName == guidstring, "the format name did not match");

            // Now delete this PP
           editedPP.Updateable = UpdateType.Deleted;
           sut.SaveProPricerExport(editedPP);

           ICollection<ProPricerDTO> exportsAfterSave = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);
           Assert.AreEqual(exports.Count - 1, exportsAfterSave.Count, "The ID count doesn't match");

        }


        /// <summary>
        /// Test that a single quote (or an apostrophe) cannot be saved into the DB as a valid propricer name
        /// </summary>
        [TestMethod]
        public void L_SaveProPricerExport_BOEJ47()
        {
            var sut = new ProPricerDTODataLoader();

            // create new system  pro pricer export
            ProPricerDTO newRecord = new ProPricerDTO()
            {
                ExportID = -1,
                FormatName = "MOCK " + "Dusan's Te\"st",
                Updateable = UpdateType.Upsert,
                UpdateDate = DateTime.Now,
                WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID,
                Scope = IES.Common.ProPricerScope.Workspace
            };

            newRecord.ProPricerTasks = new List<ProPricerTasks>() 
            { 
                new ProPricerTasks()
                {
                    ListOrder = 0,
                    Task = IES.Common.ProPricerField_Task.BOEStartDate 
                }
            };

            newRecord.ProPricerResources = new Collection<ProPricerResources>() 
            { 
                new ProPricerResources()
                {
                    ListOrder = 0,
                    Resource = IES.Common.ProPricerField_Resources.IMSCode
                }
            };


            sut.SaveProPricerExport(newRecord);

            ICollection<ProPricerDTO> exports = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);

            ProPricerDTO recordFromDb = (from e in exports
                                where e.FormatName == newRecord.FormatName
                                select e).FirstOrDefault();

            // should not have saved it w/ the ' in the name
            Assert.IsNull(recordFromDb);

            recordFromDb = (from e in exports
                            where e.FormatName == newRecord.FormatName.Replace("'", string.Empty).Replace("\"", string.Empty)
                            select e).FirstOrDefault();

            // this should have retrieved it..
            Assert.IsNotNull(recordFromDb);

            // Double check to make sure it's correct..
            Assert.IsTrue(recordFromDb.FormatName == newRecord.FormatName.Replace("'", string.Empty).Replace("\"", string.Empty), "The format name did not match");
            Assert.IsTrue(recordFromDb.Scope == newRecord.Scope, "The scope was not workspace");
            Assert.IsTrue(recordFromDb.ExportID > 0, "The export ID is valid");
            Assert.IsTrue(recordFromDb.ProPricerResources.Count == newRecord.ProPricerResources.Count, "Resources count was not right");
            Assert.IsTrue(recordFromDb.ProPricerTasks.Count == newRecord.ProPricerTasks.Count, "Task count was not right");

            // Edit the saved export
            recordFromDb.Updateable = UpdateType.Upsert;
            recordFromDb.FormatName = "A second test with\" baaad data' like so";
            sut.SaveProPricerExport(recordFromDb);

            ProPricerDTO editedPP = sut.GetById(recordFromDb.ExportID, ProPricerScope.Workspace);
            Assert.IsFalse(editedPP.FormatName == recordFromDb.FormatName, "the format name did not match");
            Assert.IsTrue(editedPP.FormatName == recordFromDb.FormatName.Replace("'", string.Empty).Replace("\"", string.Empty), "the format name did not match");

            // Now delete this PP
            editedPP.Updateable = UpdateType.Deleted;
            sut.SaveProPricerExport(editedPP);

            ICollection<ProPricerDTO> exportsAfterSave = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);
            Assert.AreEqual(exports.Count - 1, exportsAfterSave.Count, "The ID count doesn't match");
        }


        [TestMethod]
        public void L_SavePPTaskBlanks()
        {
            var sut = new ProPricerDTODataLoader();

            // create new system  pro pricer export
            ProPricerDTO pp = new ProPricerDTO();
            string guidstring = "MOCK" + Guid.NewGuid().ToString();
            pp.ExportID = -1;
            pp.FormatName = guidstring;
            pp.Updateable = UpdateType.Upsert;
            pp.UpdateDate = DateTime.Now;
            pp.WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID;
            pp.Scope = IES.Common.ProPricerScope.Workspace;

            ProPricerTasks ppTask1 = new ProPricerTasks();
            ppTask1.ListOrder = 0;
            ppTask1.Task = IES.Common.ProPricerField_Task.BOEStartDate;

            ProPricerTasks ppTask2 = new ProPricerTasks();
            ppTask2.ListOrder = 1;
            ppTask2.Task = IES.Common.ProPricerField_Task.BLANK;

            ProPricerTasks ppTask3 = new ProPricerTasks();
            ppTask3.ListOrder = 2;
            ppTask3.Task = IES.Common.ProPricerField_Task.BLANK;

            ProPricerTasks ppTask4 = new ProPricerTasks();
            ppTask4.ListOrder = 3;
            ppTask4.Task = IES.Common.ProPricerField_Task.BLANK;

            Collection<ProPricerTasks> tasks = new Collection<ProPricerTasks> {ppTask1, ppTask2, ppTask3, ppTask4 };

            pp.ProPricerTasks = tasks;

            sut.SaveProPricerExport(pp);

            ICollection<ProPricerDTO> exports = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);

            ProPricerDTO pp2 = (from e in exports
                                where e.FormatName == guidstring
                                select e).First();

            // Assert the save did work
            Assert.IsTrue(pp2.FormatName == guidstring, "The format name did not match");
            Assert.IsTrue(pp2.Scope == IES.Common.ProPricerScope.Workspace, "The scope was not workspace");
            Assert.IsTrue(pp2.ExportID > 0, "The export ID is valid");
            Assert.IsTrue(pp2.ProPricerTasks.Count == 4, "Task count was not right"); 
     

            // Edit the saved export
            guidstring = Guid.NewGuid().ToString();
            pp2.Updateable = UpdateType.Upsert;
            pp2.FormatName = guidstring;
            sut.SaveProPricerExport(pp2);

            ProPricerDTO editedPP = sut.GetById(pp2.ExportID, ProPricerScope.Workspace);
            Assert.IsTrue(editedPP.FormatName == guidstring, "the format name did not match");

            // Now delete this PP
            editedPP.Updateable = UpdateType.Deleted;
            sut.SaveProPricerExport(editedPP);

            ICollection<ProPricerDTO> exportsAfterSave = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);
            Assert.AreEqual(exports.Count - 1, exportsAfterSave.Count, "The ID count doesn't match");
        }

        [TestMethod]
        public void L_SavePP_OneTaskBlank()
        {
            var sut = new ProPricerDTODataLoader();

            // create new system  pro pricer export
            ProPricerDTO pp = new ProPricerDTO();
            string guidstring = "MOCK" + Guid.NewGuid().ToString();
            pp.ExportID = -1;
            pp.FormatName = guidstring;
            pp.Updateable = UpdateType.Upsert;
            pp.UpdateDate = DateTime.Now;
            pp.WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID;
            pp.Scope = IES.Common.ProPricerScope.Workspace;

            ProPricerTasks ppTask1 = new ProPricerTasks();
            ppTask1.ListOrder = 0;
            ppTask1.Task = IES.Common.ProPricerField_Task.BOEStartDate;

            ProPricerTasks ppTask2 = new ProPricerTasks();
            ppTask2.ListOrder = 1;
            ppTask2.Task = IES.Common.ProPricerField_Task.BLANK;


            Collection<ProPricerTasks> tasks = new Collection<ProPricerTasks> { ppTask1, ppTask2};

            pp.ProPricerTasks = tasks;

            sut.SaveProPricerExport(pp);

            ICollection<ProPricerDTO> exports = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);

            ProPricerDTO pp2 = (from e in exports
                                where e.FormatName == guidstring
                                select e).First();

            // Assert the save did work
            Assert.IsTrue(pp2.FormatName == guidstring, "The format name did not match");
            Assert.IsTrue(pp2.Scope == IES.Common.ProPricerScope.Workspace, "The scope was not workspace");
            Assert.IsTrue(pp2.ExportID > 0, "The export ID is valid");
            Assert.IsTrue(pp2.ProPricerTasks.Count == 2, "Task count was not right");


            // Edit the saved export
            guidstring = Guid.NewGuid().ToString();
            pp2.Updateable = UpdateType.Upsert;
            pp2.FormatName = guidstring;
            sut.SaveProPricerExport(pp2);

            ProPricerDTO editedPP = sut.GetById(pp2.ExportID, ProPricerScope.Workspace);
            Assert.IsTrue(editedPP.FormatName == guidstring, "the format name did not match");

            // Now delete this PP
            editedPP.Updateable = UpdateType.Deleted;
            sut.SaveProPricerExport(editedPP);

            ICollection<ProPricerDTO> exportsAfterSave = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);
            Assert.AreEqual(exports.Count - 1, exportsAfterSave.Count, "The ID count doesn't match");
        }

        [TestMethod]
        public void L_SavePPWithCustomFields()
        {
            var sut = new ProPricerDTODataLoader();

            // create new system  pro pricer export
            ProPricerDTO pp = new ProPricerDTO();
            string guidstring = "MOCK" + Guid.NewGuid().ToString();
            pp.ExportID = -1;
            pp.FormatName = guidstring;
            pp.Updateable = UpdateType.Upsert;
            pp.UpdateDate = DateTime.Now;
            pp.WorkspaceID = GlobalTestCaseSetup.GlobalWorkspaceID;
            pp.Scope = IES.Common.ProPricerScope.Workspace;

            // Save Pro pricer tasks
            ProPricerTasks ppTask1 = new ProPricerTasks();
            ppTask1.ListOrder = 0;
            ppTask1.Task = IES.Common.ProPricerField_Task.BOEStartDate;

            ProPricerTasks ppTask2 = new ProPricerTasks();
            ppTask2.ListOrder = 1;
            ppTask2.Task = IES.Common.ProPricerField_Task.BLANK;

            ProPricerTasks ppTask3 = new ProPricerTasks();
            ppTask3.ListOrder = 2;
            ppTask3.Task = IES.Common.ProPricerField_Task.TOTAL;

            ProPricerTasks ppTask4 = new ProPricerTasks();
            ppTask4.ListOrder = 3;
            ppTask4.Task = IES.Common.ProPricerField_Task.CLINNumber;

            // save custom field tasks
            ProPricerTasks customTask = new ProPricerTasks();
            customTask.CustomFieldID = GlobalTestCaseSetup.GlobalCustomFieldID;
            customTask.ListOrder = 4;
            customTask.Selection = IES.Common.ProPricerCustomFieldSelection.CustomFieldID;


            Collection<ProPricerTasks> tasks = new Collection<ProPricerTasks> { ppTask1, ppTask2, ppTask3, ppTask4, customTask };

            pp.ProPricerTasks = tasks;

            // save pro pricer resources
            ProPricerResources ppResource = new ProPricerResources();
            ppResource.ListOrder = 0;
            ppResource.Resource = IES.Common.ProPricerField_Resources.IMSCode;

            ProPricerResources ppResource2 = new ProPricerResources();
            ppResource2.ListOrder = 1;
            ppResource2.Resource = IES.Common.ProPricerField_Resources.ResourceID;

            // save custom field resources
            ProPricerResources customResource = new ProPricerResources();
            customResource.CustomFieldID = GlobalTestCaseSetup.GlobalCustomFieldID;
            customResource.ListOrder = 3;
            customResource.Selection = IES.Common.ProPricerCustomFieldSelection.CustomFieldDescription;

            Collection<ProPricerResources> resources = new Collection<ProPricerResources> { ppResource, ppResource2, customResource };
            pp.ProPricerResources = resources;

            sut.SaveProPricerExport(pp);

            ICollection<ProPricerDTO> exports = sut.GetByWorkspaceId(GlobalTestCaseSetup.GlobalWorkspaceID);

            ProPricerDTO pp2 = (from e in exports
                                where e.FormatName == guidstring
                                select e).First();

            // Assert the save did work
            Assert.IsTrue(pp2.FormatName == guidstring, "The format name did not match");
            Assert.IsTrue(pp2.Scope == IES.Common.ProPricerScope.Workspace, "The scope was not workspace");
            Assert.IsTrue(pp2.ExportID > 0, "The export ID is valid");
            Assert.IsTrue(pp2.ProPricerResources.Count == 3, "Resources count was not right");
            Assert.IsTrue(pp2.ProPricerTasks.Count == 5, "Task count was not right");
            Assert.IsTrue(pp2.ProPricerResources.ToList()[2].CustomFieldID.HasValue, "the custom field was not resource");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "newAvg"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "originalAvg")]
        //[TestMethod]
        public void TestOldVsNew()
        {
            //ProPricerDTODataLoader loader = new ProPricerDTODataLoader();

            //Stopwatch sw = new Stopwatch();
            //List<ProPricerDTO> originalData = new List<ProPricerDTO>();
            //List<ProPricerDTO> newData = new List<ProPricerDTO>();
            //List<long> originalTimes = new List<long>();
            //List<long> newTimes = new List<long>();

            //List<int> ids = new List<int>();

            //{
            //    int maxItems = 60;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.ProPricerExports.OrderBy(x => new Guid()).Select(x => x.ProPricerExportID).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); originalData.Add(loader.GetById_OLD(ids.ElementAt(i))); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); newData.Add(loader.GetById(ids.ElementAt(i))); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<ProPricerDTO>(); newData = new List<ProPricerDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();

            //    int maxItems = 60;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.Workspaces.Where(x => x.ProPricerExports.Any()).OrderBy(x => new Guid()).Select(x => x.WorkspaceID).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); originalData.AddRange(loader.GetByWorkspaceId_OLD(ids.ElementAt(i))); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); newData.AddRange(loader.GetByWorkspaceId(ids.ElementAt(i))); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}
        }

        public static void VerifyCollections(ICollection<ProPricerDTO> collection1, ICollection<ProPricerDTO> collection2, bool skipFieldsNotRestoredFromBackup = false)
        {
            Assert.AreEqual(collection1.Count, collection2.Count);
            for (int i = 0; i < collection1.Count; i++)
            {
                VerifyDtos(collection1.ElementAt(i), collection2.ElementAt(i), skipFieldsNotRestoredFromBackup);
            }
        }

        public static void VerifyDtos(ProPricerDTO dto1, ProPricerDTO dto2, bool skipFieldsNotRestoredFromBackup = false)
        {
            if(!skipFieldsNotRestoredFromBackup)
            {
                Assert.AreEqual(dto1.ExportID, dto2.ExportID);
                Assert.AreEqual(dto1.WorkspaceID, dto2.WorkspaceID);
            }

            Assert.AreEqual(dto1.FormatName, dto2.FormatName);
            Assert.AreEqual(dto1.Scope, dto2.Scope);
            Assert.AreEqual(dto1.UpdateDate, dto2.UpdateDate);

            Assert.AreEqual(dto1.ProPricerResources.Count, dto2.ProPricerResources.Count);
            for (int i = 0; i < dto1.ProPricerResources.Count; i++)
            {
                if (!skipFieldsNotRestoredFromBackup)
                {
                    Assert.AreEqual(dto1.ProPricerResources.ElementAt(i).CustomFieldID, dto2.ProPricerResources.ElementAt(i).CustomFieldID);
                    Assert.AreEqual(dto1.ProPricerResources.ElementAt(i).Resource, dto2.ProPricerResources.ElementAt(i).Resource);
                }

                Assert.AreEqual(dto1.ProPricerResources.ElementAt(i).ListOrder, dto2.ProPricerResources.ElementAt(i).ListOrder);
                Assert.AreEqual(dto1.ProPricerResources.ElementAt(i).Selection, dto2.ProPricerResources.ElementAt(i).Selection);
            }

            Assert.AreEqual(dto1.ProPricerTasks.Count, dto2.ProPricerTasks.Count);
            for (int i = 0; i < dto1.ProPricerTasks.Count; i++)
            {
                if (!skipFieldsNotRestoredFromBackup)
                {
                    Assert.AreEqual(dto1.ProPricerTasks.ElementAt(i).CustomFieldID, dto2.ProPricerTasks.ElementAt(i).CustomFieldID);
                    Assert.AreEqual(dto1.ProPricerTasks.ElementAt(i).ListOrder, dto2.ProPricerTasks.ElementAt(i).ListOrder);
                }

                Assert.AreEqual(dto1.ProPricerTasks.ElementAt(i).Selection, dto2.ProPricerTasks.ElementAt(i).Selection);
                Assert.AreEqual(dto1.ProPricerTasks.ElementAt(i).Task, dto2.ProPricerTasks.ElementAt(i).Task);
            }
        }
    }
}
