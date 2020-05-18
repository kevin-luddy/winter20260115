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
    using System.Linq;
    using System.Transactions;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.Dtos;
    using IES.Common;

    [TestClass]
    public class WbsDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_GetAllWbsIDsByWorkspaceID()
        {
            var sut = new WbsDTODataLoader();

            ICollection<int> results = sut.GetByWorkspaceId(this.Workspace.Id).Select(x => x.Id).ToList();

            //Assert
            Assert.IsTrue(results.Count > 0, "No WBS records found when trying to load ALL WBS");

            var wbsResult = (from w in results
                             where w == this.Wbs.Id
                             select w).FirstOrDefault();

            //Assert
            Assert.IsNotNull(wbsResult, "WBS with correct id [id=" + wbsResult + "] was not created.");

        }

        [TestMethod]
        public void L_GetWbsByWbsID()
        {
            //SUT
            var sut = new WbsDTODataLoader();

            WbsDTO results = sut.GetById(this.Wbs.Id);

            //Assert
            Assert.IsNotNull(results, "Did not return WBS inserted by test case setup");
            Assert.AreEqual(this.Wbs.Id, results.Id, "returned WbsDTO.Id does not match the expected");
        }

        [TestMethod]
        public void L_GetAllParentWbs()
        {
            var sut = new WbsDTODataLoader();

            WbsDTO parent1 = new WbsDTO()
            {
                Id = -1,
                WbsNumber = "@tst1",
                WbsTitle = "First Node",
                WorkspaceID = this.Workspace.Id,
                UpdateDate = DateTime.Now,
                Updateable = UpdateType.Upsert
            };

            WbsDTO parent2 = new WbsDTO()
            {
                Id = -2,
                WbsNumber = "@tst1.@tst2",
                WbsTitle = "Second Node",
                WorkspaceID = this.Workspace.Id,
                UpdateDate = DateTime.Now,
                Updateable = UpdateType.Upsert
            };

            WbsDTO parent3 = new WbsDTO()
            {
                Id = -3,
                WbsNumber = "@tst1.@tst2.@tst3",
                WbsTitle = "Third Node",
                WorkspaceID = this.Workspace.Id,
                UpdateDate = DateTime.Now,
                Updateable = UpdateType.Upsert
            };

            WbsDTO wbsToTest = new WbsDTO()
            {
                Id = -4,
                WbsNumber = "@tst1.@tst2.@tst3.@tst4",
                WbsTitle = "Node to test",
                WorkspaceID = this.Workspace.Id,
                UpdateDate = DateTime.Now,
                Updateable = UpdateType.Upsert
            };

            WbsDTO child1 = new WbsDTO()
            {
                Id = -5,
                WbsNumber = "@tst1.@tst2.@tst3.@tst4.@tst5",
                WbsTitle = "First Child",
                WorkspaceID = this.Workspace.Id,
                UpdateDate = DateTime.Now,
                Updateable = UpdateType.Upsert
            };

            WbsDTO child2 = new WbsDTO()
            {
                Id = -6,
                WbsNumber = "@tst1.@tst2.@tst3.@tst4.@tst5.@tst6",
                WbsTitle = "Second Child",
                WorkspaceID = this.Workspace.Id,
                UpdateDate = DateTime.Now,
                Updateable = UpdateType.Upsert
            };

            Collection<WbsDTO> toSave = new Collection<WbsDTO>() { parent1, parent2, parent3, wbsToTest, child1, child2 };
            Dictionary<int, int> results;
            using (TransactionScope scope = new TransactionScope())
            {
                results = sut.Save(toSave);
                scope.Complete();
            }

            List<int> parentIds = new List<int>() { results[-1], results[-2], results[-3] };

            ICollection<WbsDTO> parentWbs = sut.GetAllParentWbs(this.Workspace.Id, wbsToTest.WbsNumber);

            Assert.AreEqual(3, parentWbs.Count);
            
            foreach(WbsDTO parent in parentWbs)
            {
                Assert.IsTrue(parentIds.Contains(parent.Id));
            }

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetAllChildWbs()
        {
            var sut = new WbsDTODataLoader();

            WbsDTO parent1 = new WbsDTO()
            {
                Id = -1,
                WbsNumber = "@tst1",
                WbsTitle = "First Node",
                WorkspaceID = this.Workspace.Id,
                UpdateDate = DateTime.Now,
                Updateable = UpdateType.Upsert
            };

            WbsDTO parent2 = new WbsDTO()
            {
                Id = -2,
                WbsNumber = "@tst1.@tst2",
                WbsTitle = "Second Node",
                WorkspaceID = this.Workspace.Id,
                UpdateDate = DateTime.Now,
                Updateable = UpdateType.Upsert
            };

            WbsDTO parent3 = new WbsDTO()
            {
                Id = -3,
                WbsNumber = "@tst1.@tst2.@tst3",
                WbsTitle = "Third Node",
                WorkspaceID = this.Workspace.Id,
                UpdateDate = DateTime.Now,
                Updateable = UpdateType.Upsert
            };

            WbsDTO wbsToTest = new WbsDTO()
            {
                Id = -4,
                WbsNumber = "@tst1.@tst2.@tst3.@tst4",
                WbsTitle = "Node to test",
                WorkspaceID = this.Workspace.Id,
                UpdateDate = DateTime.Now,
                Updateable = UpdateType.Upsert
            };

            WbsDTO child1 = new WbsDTO()
            {
                Id = -5,
                WbsNumber = "@tst1.@tst2.@tst3.@tst4.@tst5",
                WbsTitle = "First Child",
                WorkspaceID = this.Workspace.Id,
                UpdateDate = DateTime.Now,
                Updateable = UpdateType.Upsert
            };

            WbsDTO child2 = new WbsDTO()
            {
                Id = -6,
                WbsNumber = "@tst1.@tst2.@tst3.@tst4.@tst5.@tst6",
                WbsTitle = "Second Child",
                WorkspaceID = this.Workspace.Id,
                UpdateDate = DateTime.Now,
                Updateable = UpdateType.Upsert
            };

            Collection<WbsDTO> toSave = new Collection<WbsDTO>() { parent1, parent2, parent3, wbsToTest, child1, child2 };
            Dictionary<int, int> results;
            using (TransactionScope scope = new TransactionScope())
            {
                results = sut.Save(toSave);
                scope.Complete();
            }

            List<int> childIds = new List<int>() { results[-5], results[-6] };

            ICollection<WbsDTO> childWbs = sut.GetAllChildWbs(this.Workspace.Id, wbsToTest.WbsNumber);

            Assert.AreEqual(2, childWbs.Count);

            foreach (WbsDTO child in childWbs)
            {
                Assert.IsTrue(childIds.Contains(child.Id));
            }

            this.ResetTestData();
        }

        [TestMethod]
        public void L_IsWbsNumberUnique()
        {
            var sut = new WbsDTODataLoader();
            string getWBSNumber;
            
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                getWBSNumber = (from w in gbe.WorkBreakdownStructures
                                where w.WorkspaceID == this.Workspace.Id
                                where w.WBSNumber != "MULTI"
                                select w.DisplayedWBSNumber).FirstOrDefault();
                
            }

            bool uniqueFlag = sut.IsWbsNumberUnique(getWBSNumber, this.Workspace.Id, null);

            Assert.IsFalse(uniqueFlag);

            uniqueFlag = sut.IsWbsNumberUnique(getWBSNumber, this.Workspace.Id, Wbs.Id);

            Assert.IsTrue(uniqueFlag);
        }

        [TestMethod]
        public void L_SaveWBS()
        {
            //SUT
            var sut = new WbsDTODataLoader();
            ClinDTODataLoader manageCLINDataLoader = new ClinDTODataLoader();
            int singleClin = int.MinValue;

            //Get Count of Manage WBS 
            int manageWBSCountBeforeSave = sut.GetByWorkspaceId(this.Workspace.Id).Count;

            // Get a CLIN
            Collection<int> getClins = new Collection<int>(manageCLINDataLoader.GetByWorkspaceId(this.Workspace.Id).Select(x => x.Id).ToArray());

            // if there are no valid CLINs for this workspace, create some new ones
            // else, get a random clin id to associate the boe id with
            if (getClins.Count == 0)
            {
                Collection<int> getClins2 = new Collection<int>(manageCLINDataLoader.GetByWorkspaceId(this.Workspace.Id).Select(x => x.Id).ToArray());
                singleClin = getClins2.ToArray()[Math.Abs(new Random((int)DateTime.Now.Ticks).Next(getClins2.Count - 1))];
            }
            else
            {
                singleClin = getClins.ToArray()[Math.Abs(new Random((int)DateTime.Now.Ticks).Next(getClins.Count - 1))];
            }

            var manageWBSModelViewEntry = new WbsDTO();
            Collection<WbsDTO> multipleManageWBSModelView = new Collection<WbsDTO>();

            manageWBSModelViewEntry.Id = -1;
            manageWBSModelViewEntry.WbsNumber = "11.13.1";
            manageWBSModelViewEntry.WbsTitle = "Chris WORK ELEMENT TEST Null CLINID";
            manageWBSModelViewEntry.WorkspaceID = this.Workspace.Id;
            manageWBSModelViewEntry.ClinIDs = new Collection<int> { singleClin };
            manageWBSModelViewEntry.UpdateDate = DateTime.Now;
            manageWBSModelViewEntry.Updateable = UpdateType.Upsert;

            multipleManageWBSModelView.Add(manageWBSModelViewEntry);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(multipleManageWBSModelView);
                scope.Complete();
            }

            int manageWBSCountAfterSave = sut.GetByWorkspaceId(this.Workspace.Id).Count;

            Assert.IsTrue(manageWBSCountAfterSave > manageWBSCountBeforeSave);

            ICollection<int> ids = new Collection<int>();
            ids = sut.GetByWorkspaceId(this.Workspace.Id).Select(x => x.Id).ToList();

            foreach (int id in ids)
            {
                if (sut.GetById(id).WbsNumber == "11.13.1")
                {
                    Assert.IsTrue(sut.GetById(id).WbsPaddedNumber == "00011??.00013??.00001??");
                }
            }

            this.ResetTestData();
        }

        [TestMethod]
        public void L_DeleteWBS()
        {
            //SUT
            var sut = new WbsDTODataLoader();

            ClinDTODataLoader manageCLINDataLoader = new ClinDTODataLoader();
            int singleClin = int.MinValue;
            // Get a CLIN
            Collection<int> getClins = new Collection<int>(manageCLINDataLoader.GetByWorkspaceId(this.Workspace.Id).Select(x => x.Id).ToArray());
            // if there are no valid CLINs for this workspace, create some new ones
            // else, get a random clin id to associate the boe id with
            if (getClins.Count == 0)
            {
                Collection<int> getClins2 = new Collection<int>(manageCLINDataLoader.GetByWorkspaceId(this.Workspace.Id).Select(x => x.Id).ToArray());
                singleClin = getClins2.ToArray()[Math.Abs(new Random((int)DateTime.Now.Ticks).Next(getClins2.Count - 1))];
            }
            else
            {
                singleClin = getClins.ToArray()[Math.Abs(new Random((int)DateTime.Now.Ticks).Next(getClins.Count - 1))];
            }
            // add a WBS DTO to the workspace so we'll know we always have one to delete
            var manageWBSModelViewEntry = new WbsDTO();
            Collection<WbsDTO> multipleManageWBSModelView = new Collection<WbsDTO>();

            string wbstitle = Guid.NewGuid().ToString();
            manageWBSModelViewEntry.Id = -1;
            manageWBSModelViewEntry.WbsNumber = "11131";
            manageWBSModelViewEntry.WbsTitle = wbstitle;
            manageWBSModelViewEntry.WorkspaceID = this.Workspace.Id;
            manageWBSModelViewEntry.ClinIDs = new Collection<int> { singleClin };
            manageWBSModelViewEntry.UpdateDate = DateTime.Now;
            manageWBSModelViewEntry.Updateable = UpdateType.Upsert;

            multipleManageWBSModelView.Add(manageWBSModelViewEntry);

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(multipleManageWBSModelView);
                scope.Complete();
            }

            //Get Count of Manage WBS 
            int manageWBSCountBeforeDelete = sut.GetByWorkspaceId(this.Workspace.Id).Count;

            WbsDTO singleManageWBSModelView = new WbsDTO();

            Collection<WbsDTO> ManageWBSList = new Collection<WbsDTO>();
            ICollection<int> WBSIDs = sut.GetByWorkspaceId(this.Workspace.Id).Select(x => x.Id).ToList();

            // get All WBS DTOs
            foreach (int x in WBSIDs)
            {
                singleManageWBSModelView = new WbsDTO();
                singleManageWBSModelView = sut.GetById(x);
                singleManageWBSModelView.Updateable = UpdateType.Upsert;
                ManageWBSList.Add(singleManageWBSModelView);
            }

            // mark the first DTO for deletion
            foreach (WbsDTO wbs in ManageWBSList)
            {
                // delete the WBS we just created
                if (wbs.WbsTitle == wbstitle)
                {
                    wbs.Updateable = UpdateType.Deleted;
                    break;
                }
            }

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(ManageWBSList);
                scope.Complete();
            }

            //Get Count of Manage WBS 
            int manageWBSCountAfterDelete = sut.GetByWorkspaceId(this.Workspace.Id).Count;

            Assert.IsTrue(manageWBSCountAfterDelete == manageWBSCountBeforeDelete - 1);
            this.ResetTestData();
        }

        /// <summary>
        /// GetWorkspaceVariableIDsByWbsID calls GetWorkspaceVariableIDSByWbsIDs so it tests both methods
        /// </summary>
        [TestMethod]
        public void L_GetWorkspaceVariableIDsByWbsID()
        {
            this.ResetTestData();
            this.Setup();

            var sut = new WbsDTODataLoader();
            ICollection<int> workspaceVarIDs = sut.GetWorkspaceVariableIdsById(this.Wbs.Id);

            Assert.IsTrue(workspaceVarIDs.Count == 1, "No workspace variables found associated with WBS ID");
            Assert.AreEqual(this.SummaryWbsWorkspaceVariable.Id, workspaceVarIDs.ElementAt(0), "actual workspace Var id does not match expected");
        }

        [TestMethod]
        public void L_GetBoeIdsForWbsIdWithNesting()
        {
            var sut = new WbsDTODataLoader();

            ICollection<int> results = sut.GetBoeIdsForWbsIdWithNesting(this.Wbs.Id);

            //Assert
            Assert.IsNotNull(results, "Did not return WBS inserted by test case setup");
            Assert.AreEqual(4, results.Count(), "didn't get the expected number of ids");
        }

        [TestMethod]
        public void L_RemapTaskAndWorkspaceVariablesFromWbsToBoe()
        {
            this.ResetTestData();
            this.Setup();

            WbsDTO altWbs = _CreateWBS(-1, new Collection<int> { Clin1.Id, Clin2.Id }, "1.2.3.4.5.6");
            Dictionary<int, int> wbsids;
            using (TransactionScope scope = new TransactionScope())
            {
                wbsids = _wbsDL.Save(new Collection<WbsDTO> { altWbs });
                scope.Complete();
            }

            OrdinaryVariableDto ordinaryVar = _CreateOrdinaryTaskVariable(-1, "remap", 24m, IES.Common.VarSortBOEBy.WBS, IES.Common.VarValueType.SumOfBOEs,
                new Collection<SelectBOEsToSum> { new SelectBOEsToSum { WBSID = altWbs.Id, BoeID = null, CLINID = null } }, this.TaskElement.Id);
            IDictionary<int, int> varIds;
            using (TransactionScope scope = new TransactionScope())
            {
                varIds = _taskElementDL.SaveOrdinaryVariables(new Collection<OrdinaryVariableDto> { ordinaryVar });
                scope.Complete();
            }

            ordinaryVar.Id = varIds.First().Value;

            WorkspaceVariableDTO workspaceVar = _CreateWorkspaceVariable(-1, "remap", 24m, IES.Common.VarSortBOEBy.WBS, IES.Common.VarValueType.SumOfBOEs,
                new Collection<SelectBOEsToSum> { new SelectBOEsToSum { WBSID = altWbs.Id, BoeID = null, CLINID = Clin1.Id } });
            IDictionary<int, int> workspaceIds;
            using (TransactionScope scope = new TransactionScope())
            {
                workspaceIds = this._workspaceVariableDL.SaveWorkspaceVariables(new Collection<WorkspaceVariableDTO> { workspaceVar });
                scope.Complete();
            }

            workspaceVar.Id = workspaceIds.First().Value;

            int key = wbsids.First().Key;
            altWbs = _wbsDL.GetById(wbsids[key]);
            Assert.IsNotNull(altWbs);

            // check the before state
            OrdinaryVariableDto ordinaryVarBefore = _taskElementDL.GetTaskVariableByTaskVariableID(ordinaryVar.Id);
            Assert.IsNotNull(ordinaryVarBefore);
            Assert.IsNotNull(ordinaryVarBefore.SelectedBOEsToSum);
            Assert.IsNull(ordinaryVarBefore.SelectedBOEsToSum.First().BoeID);
            Assert.AreEqual(altWbs.Id, ordinaryVarBefore.SelectedBOEsToSum.First().WBSID);

            WorkspaceVariableDTO workspaceVarBefore = _workspaceVariableDL.GetById(workspaceVar.Id);
            Assert.IsNotNull(workspaceVarBefore);
            Assert.IsNotNull(workspaceVarBefore.SelectedBOEsToSum);
            Assert.IsNull(workspaceVarBefore.SelectedBOEsToSum.First().BoeID);
            Assert.AreEqual(altWbs.Id, workspaceVarBefore.SelectedBOEsToSum.First().WBSID);

            var sut = new WbsDTODataLoader();

            using (TransactionScope scope = new TransactionScope())
            {
                sut.RemapTaskAndWorkspaceVariablesFromWbsToBoe(altWbs.Id, this.Boe1.Id);
                scope.Complete();
            }

            //Assert - get the wbs and the boe1 dtos and verify that the variables are remapped
            OrdinaryVariableDto ordinaryVarAfter = _taskElementDL.GetTaskVariableByTaskVariableID(ordinaryVar.Id);
            Assert.IsNotNull(ordinaryVarAfter);
            Assert.IsNotNull(ordinaryVarAfter.SelectedBOEsToSum);
            Assert.IsNull(ordinaryVarAfter.SelectedBOEsToSum.First().WBSID);
            Assert.AreEqual(this.Boe1.Id, ordinaryVarAfter.SelectedBOEsToSum.First().BoeID);

            WorkspaceVariableDTO workspaceVarAfter = _workspaceVariableDL.GetById(workspaceVar.Id);
            Assert.IsNotNull(workspaceVarAfter);
            Assert.IsNotNull(workspaceVarAfter.SelectedBOEsToSum);
            Assert.IsNull(workspaceVarAfter.SelectedBOEsToSum.First().WBSID);
            Assert.AreEqual(this.Boe1.Id, workspaceVarAfter.SelectedBOEsToSum.First().BoeID);
        }

        [TestMethod]
        public void L_IsWbsTiedToMaterialBoeTRUE()
        {
            var sut = new WbsDTODataLoader();

            bool isTied = sut.IsWbsTiedToMaterialBoe(this.Boe1.Id, this.Wbs.Id);

            Assert.IsTrue(isTied, "the WBS should have been tied to a material boe");

            this.ResetTestData(); // reset for next caller
        }

        [TestMethod]
        public void L_IsWbsTiedToMaterialBoeFALSE()
        {
            this.ResetTestData();
            this.Setup();

            // delete the material boe, we're going to create a new one that doesn't use the WBS
            this.DeleteBOEs(new Collection<BoeDTO> { BOEMaterial });

            WbsDTO altWbs = _CreateWBS(-1, new Collection<int> { Clin1.Id, Clin2.Id });
            Dictionary<int, int> wbsids;
            using (TransactionScope scope = new TransactionScope())
            {
                wbsids = _wbsDL.Save(new Collection<WbsDTO> { altWbs });
                scope.Complete();
            }

            int key = wbsids.First().Key;
            altWbs = _wbsDL.GetById(wbsids[key]);
            Assert.IsNotNull(altWbs);

            // reset with a new material BOE passed in
            BOEMaterial = this._CreateBOE(-1, Clin1.Id, altWbs.Id, false);
            BOEMaterial.UpdatedByUserId = Author.UserID;

            int? boematerialid;
            using (TransactionScope scope = new TransactionScope())
            {
                boematerialid = _boeDL.Save(BOEMaterial);
                scope.Complete();
            }
            BOEMaterial = _boeDL.GetByIds(new Collection<int> { boematerialid.Value }).First();


            var sut = new WbsDTODataLoader();

            bool isTied = sut.IsWbsTiedToMaterialBoe(this.Boe2.Id, this.Wbs.Id);

            Assert.IsTrue(!isTied, "the WBS should NOT have been tied to a material boe");
            this.ResetTestData();
        }

        [TestMethod]
        public void L_IsWbsTiedToMaterialBoeAndClin_TRUE()
        {
            var sut = new WbsDTODataLoader();

            bool isTied = sut.IsWbsTiedToMaterialBoeAndClin(this.Boe1.Id, this.Wbs.Id, this.Clin1.Id);

            Assert.IsTrue(isTied, "the WBS should have been tied to a material boe and Clin");

            this.ResetTestData(); // reset for next caller
        }

        [TestMethod]
        public void L_IsWbsTiedToMaterialBoeAndClin_FALSE()
        {
            this.ResetTestData();
            this.Setup();

            // delete the material boe, we're going to create a new one that doesn't use the WBS
            using (TransactionScope scope = new TransactionScope())
            {
                this.DeleteBOEs(new Collection<BoeDTO> { BOEMaterial });
                scope.Complete();
            }

            WbsDTO altWbs = _CreateWBS(-1, new Collection<int> { Clin1.Id, Clin2.Id });
            Dictionary<int, int> wbsids;

            using (TransactionScope scope = new TransactionScope())
            {
                wbsids = _wbsDL.Save(new Collection<WbsDTO> { altWbs });
                scope.Complete();
            }

            int key = wbsids.First().Key;
            altWbs = _wbsDL.GetById(wbsids[key]);
            Assert.IsNotNull(altWbs);

            // reset with a new material BOE passed in
            BOEMaterial = this._CreateBOE(-1, Clin1.Id, altWbs.Id, false);
            BOEMaterial.UpdatedByUserId = Author.UserID;

            int? boematerialid;
            using (TransactionScope scope = new TransactionScope())
            {
                boematerialid = _boeDL.Save(BOEMaterial);
                scope.Complete();
            }

            BOEMaterial = _boeDL.GetByIds(new Collection<int> { boematerialid.Value }).First();


            var sut = new WbsDTODataLoader();

            bool isTied = sut.IsWbsTiedToMaterialBoeAndClin(this.Boe2.Id, this.Wbs.Id, this.Clin1.Id);

            Assert.IsTrue(!isTied, "the WBS should NOT have been tied to a material boe and Clin");
            this.ResetTestData();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "newAvg"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "originalAvg")]
        //[TestMethod]
        public void TestOldVsNew()
        {
            //WbsDTODataLoader loader = new WbsDTODataLoader();

            //Stopwatch sw = new Stopwatch();
            //List<WbsDTO> originalData = new List<WbsDTO>();
            //List<WbsDTO> newData = new List<WbsDTO>();
            //List<long> originalTimes = new List<long>();
            //List<long> newTimes = new List<long>();

            //List<int> ids = new List<int>();

            //{
            //    int maxItems = 30;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.WorkBreakdownStructures.OrderBy(x => new Guid()).Select(x => x.WBSID).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < 5; i++) { sw.Restart(); originalData.AddRange(loader.GetByIds_OLD(ids)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < 5; i++) { sw.Restart(); newData.AddRange(loader.GetByIds(ids)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<WbsDTO>(); newData = new List<WbsDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();

            //    int maxItems = 20;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.Workspaces.Where(x => x.WorkBreakdownStructures.Any()).OrderBy(x => new Guid()).Select(x => x.WorkspaceID).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); originalData.AddRange(loader.GetByWorkspaceId_OLD(ids.ElementAt(i))); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); newData.AddRange(loader.GetByWorkspaceId(ids.ElementAt(i))); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    originalData.ForEach(x => 
            //    {
            //        x.ClinIDs = new Collection<int>(x.ClinIDs.OrderBy(z => z).ToList());
            //        x.ClinsInUse = new Collection<int>(x.ClinsInUse.OrderBy(z => z).ToList());
            //    });

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<WbsDTO>(); newData = new List<WbsDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();
            //    List<dynamic> data = new List<dynamic>();

            //    int maxItems = 50;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        data = gbe.WorkBreakdownStructures.Where(x => x.WorkspaceID.HasValue).Select(x => new { wsId = x.WorkspaceID.Value, number = x.WBSNumber })
            //            .OrderBy(x => new Guid()).Take(maxItems).ToList<dynamic>();
            //    }

            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); originalData.AddRange(loader.GetAllParentWbs_OLD
            //        (data.ElementAt(i).wsId, data.ElementAt(i).number)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); newData.AddRange(loader.GetAllParentWbs
            //        (data.ElementAt(i).wsId, data.ElementAt(i).number)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    originalData.ForEach(x =>
            //    {
            //        x.ClinIDs = new Collection<int>(x.ClinIDs.OrderBy(z => z).ToList());
            //        x.ClinsInUse = new Collection<int>(x.ClinsInUse.OrderBy(z => z).ToList());
            //    });

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<WbsDTO>(); newData = new List<WbsDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();
            //    List<dynamic> data = new List<dynamic>();

            //    int maxItems = 50;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        data = gbe.WorkBreakdownStructures.Where(x => x.WorkspaceID.HasValue).Select(x => new { wsId = x.WorkspaceID.Value, number = x.WBSNumber })
            //            .OrderBy(x => new Guid()).Take(maxItems).ToList<dynamic>();
            //    }

            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); originalData.AddRange(loader.GetAllChildWbs_OLD
            //        (data.ElementAt(i).wsId, data.ElementAt(i).number)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); newData.AddRange(loader.GetAllChildWbs
            //        (data.ElementAt(i).wsId, data.ElementAt(i).number)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    originalData.ForEach(x =>
            //    {
            //        x.ClinIDs = new Collection<int>(x.ClinIDs.OrderBy(z => z).ToList());
            //        x.ClinsInUse = new Collection<int>(x.ClinsInUse.OrderBy(z => z).ToList());
            //    });

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<WbsDTO>(); newData = new List<WbsDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();
            //    List<dynamic> data = new List<dynamic>();

            //    int maxItems = 50;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        data = gbe.WorkBreakdownStructures.Where(x => x.WorkspaceID.HasValue).Select(x => new { wsId = x.WorkspaceID.Value, number = x.WBSNumber })
            //            .OrderBy(x => new Guid()).Take(maxItems).ToList<dynamic>();
            //    }

            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); originalData.AddRange(loader.GetAllChildWbs_OLD
            //        (data.ElementAt(i).wsId, data.ElementAt(i).number)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); newData.AddRange(loader.GetAllParentWbs_OLD
            //        (data.ElementAt(i).wsId, data.ElementAt(i).number)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    originalData.ForEach(x =>
            //    {
            //        x.ClinIDs = new Collection<int>(x.ClinIDs.OrderBy(z => z).ToList());
            //        x.ClinsInUse = new Collection<int>(x.ClinsInUse.OrderBy(z => z).ToList());
            //    });

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}
        }

        public static void VerifyCollections(ICollection<WbsDTO> collection1, ICollection<WbsDTO> collection2, bool skipFieldsNotRestoredFromBackup = false)
        {
            Assert.AreEqual(collection1.Count, collection2.Count);
            for (int i = 0; i < collection1.Count; i++)
            {
                VerifyDtos(collection1.ElementAt(i), collection2.ElementAt(i), skipFieldsNotRestoredFromBackup);
            }
        }

        public static void VerifyDtos(WbsDTO dto1, WbsDTO dto2, bool skipFieldsNotRestoredFromBackup = false)
        {
            if (!skipFieldsNotRestoredFromBackup)
            {
                Assert.AreEqual(dto1.Id, dto2.Id);
                Assert.AreEqual(dto1.WorkspaceID, dto2.WorkspaceID);
            }

            Assert.AreEqual(dto1.inUse, dto2.inUse);
            Assert.AreEqual(dto1.Level, dto2.Level);
            Assert.AreEqual(dto1.Updateable, dto2.Updateable);
            Assert.AreEqual(dto1.UpdateDate, dto2.UpdateDate);
            Assert.AreEqual(dto1.WbsNumber, dto2.WbsNumber);
            Assert.AreEqual(dto1.WbsPaddedNumber, dto2.WbsPaddedNumber);
            Assert.AreEqual(dto1.WbsString, dto2.WbsString);
            Assert.AreEqual(dto1.WbsTitle, dto2.WbsTitle);

            Assert.AreEqual(dto1.ClinIDs.Count, dto2.ClinIDs.Count);
            if (!skipFieldsNotRestoredFromBackup)
            {
                for (int i = 0; i < dto1.ClinIDs.Count; i++)
                {
                    Assert.AreEqual(dto1.ClinIDs.ElementAt(i), dto2.ClinIDs.ElementAt(i));
                }
            }

            Assert.AreEqual(dto1.ClinsInUse.Count, dto2.ClinsInUse.Count);
            if (!skipFieldsNotRestoredFromBackup)
            {
                for (int i = 0; i < dto1.ClinsInUse.Count; i++)
                {
                    Assert.AreEqual(dto1.ClinsInUse.ElementAt(i), dto2.ClinsInUse.ElementAt(i));
                }
            }
        }
    }
}