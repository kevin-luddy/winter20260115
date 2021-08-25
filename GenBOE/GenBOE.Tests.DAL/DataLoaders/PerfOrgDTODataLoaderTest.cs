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
    using GenBOE.Models;
    using GenBOE.DataBridge.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.Dtos;
    using IES.Common;

    [TestClass]
    public class PerfOrgDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_GetAllPerformingOrgsByListId()
        {
            var sut = new PerformingOrgDTODataLoader();

            // search on global
            Collection<PerformingOrgDTO> results = sut.GetByListId(1);

            int totalFromDB;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                totalFromDB = (from r in gbe.PerformingOrganizations
                                        where r.PerformingOrganizationListID == 1 && r.DeletedFlag != true
                                        select r).Count();
            }

            Assert.AreEqual(totalFromDB, results.Count, "The number of system perf orgs returned did not match the number in the database.");

            //search for workspace perf org list id
            results = sut.GetByListId(this.PerfOrgList.PerformingOrgListID);

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                totalFromDB = (from r in gbe.WorkspacePerformingOrganizations
                               where r.PerformingOrganizationListID == this.PerfOrgList.PerformingOrgListID
                               select r).Count();
            }

            Assert.AreEqual(totalFromDB, results.Count, "The number of workspace perf orgs returned did not match the number in the database.");

        }

        [TestMethod]
        public void L_UpsertSystemPerformingOrg()
        {
            var sut = new PerformingOrgDTODataLoader();
            Collection<PerformingOrgDTO> NewPerformingOrgs = new Collection<PerformingOrgDTO>();
            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = -1,  PerformingOrgName = Guid.NewGuid().ToString(), PerformingOrgDesc = "First Perf Org", Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now };
            PerformingOrgDTO perfOrg2 = new PerformingOrgDTO { Id = -2,  PerformingOrgName = Guid.NewGuid().ToString(), PerformingOrgDesc = "Second Perf Org", Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now };
            NewPerformingOrgs.Add(perfOrg);
            NewPerformingOrgs.Add(perfOrg2);

            // get the list before we save so we can compare counts after the save
            int Count1 = sut.GetByListId(1).Count;
            Dictionary<int, int> newids = sut.SaveSystemPerformingOrgs(NewPerformingOrgs);
            int Count2 = sut.GetByListId(1).Count;
            Assert.AreEqual(Count1 + 2, Count2, "Two Performing Orgs were not saved");

            // now edit a performing org
            perfOrg = sut.GetById(newids[-1]);
            perfOrg.PerformingOrgDesc = " PERFORG 1 ";
            perfOrg.Updateable = UpdateType.Upsert;
            newids = sut.SaveSystemPerformingOrgs(new Collection<PerformingOrgDTO> { perfOrg });

            PerformingOrgDTO updated = sut.GetById(newids[perfOrg.Id]);

            Assert.AreEqual(updated.PerformingOrgDesc, perfOrg.PerformingOrgDesc.Trim(), "Update was not successful");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_DeleteSystemPerformingOrg()
        {
            int globalListID = 1;
            var sut = new PerformingOrgDTODataLoader();
            Collection<PerformingOrgDTO> PerformingOrgs = new Collection<PerformingOrgDTO>();
            Collection<PerformingOrgDTO> NewPerformingOrgs = new Collection<PerformingOrgDTO>();

            // get the list before we save so we can compare counts after the save
            int Count1 = sut.GetByListId(globalListID).Count;

            // Set up a new DTO
            string newName = Guid.NewGuid().ToString().Substring(0, 4);

            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = -1, PerformingOrgName = newName, PerformingOrgDesc = "Delete Perf Org", Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now };
            NewPerformingOrgs.Add(perfOrg);

            sut.SaveSystemPerformingOrgs(NewPerformingOrgs);
            int AfterSaveCount = sut.GetByListId(globalListID).Count;
            Assert.AreEqual(Count1 + 1, AfterSaveCount, "The Performing Org was not saved");

            ICollection<int> PerformingOrgIDs = sut.GetByListId(globalListID).Select(x => x.Id).ToList();
            PerformingOrgs = new Collection<PerformingOrgDTO>();
            foreach (int PerformingOrgID in PerformingOrgIDs)
            {
                PerformingOrgs.Add(sut.GetById(PerformingOrgID));
            }

            // now delete the org we just saved
            var updatedPO = (from p in PerformingOrgs
                             where p.PerformingOrgName == newName 
                             select p).First();

            // clear the list
            PerformingOrgs = new Collection<PerformingOrgDTO>();
            perfOrg = updatedPO;
            perfOrg.Updateable = UpdateType.Deleted;
            PerformingOrgs.Add(perfOrg);
            sut.SaveSystemPerformingOrgs(PerformingOrgs);
            int FinalPerformingOrgsCount = sut.GetByListId(globalListID).Count;

            Assert.AreEqual(FinalPerformingOrgsCount, AfterSaveCount - 1, "The delete did not work");
            this.ResetTestData();
        }

        [TestMethod]
        public void L_UpsertWorkspacePerformingOrg()
        {
            var sut = new PerformingOrgDTODataLoader();
            Collection<PerformingOrgDTO> NewPerformingOrgs = new Collection<PerformingOrgDTO>();
            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = -1,  PerformingOrgName = Guid.NewGuid().ToString(), PerformingOrgDesc = "First Perf Org", Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now };
            PerformingOrgDTO perfOrg2 = new PerformingOrgDTO { Id = -2, PerformingOrgName = Guid.NewGuid().ToString(), PerformingOrgDesc = "Second Perf Org", Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now };
            NewPerformingOrgs.Add(perfOrg);
            NewPerformingOrgs.Add(perfOrg2);

            // get the list before we save so we can compare counts after the save
            int Count1 = sut.GetByListId(this.PerfOrgList.PerformingOrgListID).Count;
            Dictionary<int, int> newids = sut.SaveWorkspacePerformingOrgs(NewPerformingOrgs, this.PerfOrgList.PerformingOrgListID);
            int Count2 = sut.GetByListId(this.PerfOrgList.PerformingOrgListID).Count;
            Assert.AreEqual(Count1 + 2, Count2, "Two Performing Orgs were not saved");

            // now edit a performing org
            perfOrg = sut.GetById(newids[-1]);
            perfOrg.PerformingOrgDesc = "PERFORG1";
            perfOrg.Updateable = UpdateType.Upsert;
            newids = sut.SaveWorkspacePerformingOrgs(new Collection<PerformingOrgDTO> { perfOrg }, this.PerfOrgList.PerformingOrgListID);
            PerformingOrgDTO updated = sut.GetById(newids[perfOrg.Id]);

            Assert.AreEqual(updated.PerformingOrgDesc, perfOrg.PerformingOrgDesc, "Update was not successful");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_DeleteWorkspacePerformingOrg()
        {
            var sut = new PerformingOrgDTODataLoader();
            Collection<PerformingOrgDTO> PerformingOrgs = new Collection<PerformingOrgDTO>();
            Collection<PerformingOrgDTO> NewPerformingOrgs = new Collection<PerformingOrgDTO>();

            // get the list before we save so we can compare counts after the save
            int Count1 = sut.GetByListId(this.PerfOrgList.PerformingOrgListID).Count;

            // Set up a new DTO
            string newName = Guid.NewGuid().ToString().Substring(0, 4);

            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = -1,  PerformingOrgName = newName, PerformingOrgDesc = "Delete Perf Org", Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now };
            NewPerformingOrgs.Add(perfOrg);

            sut.SaveWorkspacePerformingOrgs(NewPerformingOrgs, this.PerfOrgList.PerformingOrgListID);
            int AfterSaveCount = sut.GetByListId(this.PerfOrgList.PerformingOrgListID).Count;
            Assert.AreEqual(Count1 + 1, AfterSaveCount, "The Performing Org was not saved");

            ICollection<int> PerformingOrgIDs = sut.GetByListId(this.PerfOrgList.PerformingOrgListID).Select(x => x.Id).ToList();
            PerformingOrgs = new Collection<PerformingOrgDTO>();
            foreach (int PerformingOrgID in PerformingOrgIDs)
            {
                PerformingOrgs.Add(sut.GetById(PerformingOrgID));
            }

            // now delete the org we just saved
            var updatedPO = (from p in PerformingOrgs
                             where p.PerformingOrgName == newName 
                             select p).First();

            // clear the list
            PerformingOrgs = new Collection<PerformingOrgDTO>();
            perfOrg = updatedPO;
            perfOrg.Updateable = UpdateType.Deleted;
            PerformingOrgs.Add(perfOrg);
            sut.SaveWorkspacePerformingOrgs(PerformingOrgs, this.PerfOrgList.PerformingOrgListID);
            int FinalPerformingOrgsCount = sut.GetByListId(this.PerfOrgList.PerformingOrgListID).Count;

            Assert.AreEqual(FinalPerformingOrgsCount, AfterSaveCount - 1, "The delete did not work");
            this.ResetTestData();
        }
    }
}
