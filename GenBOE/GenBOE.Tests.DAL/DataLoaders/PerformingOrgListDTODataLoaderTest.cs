// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Linq;
    using GenBOE.Models;
    using GenBOE.DataBridge.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.Dtos;
    using IES.Common;

    [TestClass]
    public class PerformingOrgListDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_GetPerfOrgList()
        {
            var sut = new PerformingOrgListDTODataLoader();
            PerformingOrgListDTO perfOrgList = sut.GetPerfOrgList(PerfOrgList.PerformingOrgListID);
            Assert.IsTrue(perfOrgList != null, "The performng org list was null");
        }

        [TestMethod]
        public void L_UpsertPerfOrgList()
        {
            var sut = new PerformingOrgListDTODataLoader();
            string perfOrgListName = Guid.NewGuid().ToString().Substring(0, 15);
            PerformingOrgListDTO perfOrgList = new PerformingOrgListDTO { PerformingOrgListID = -1, PerformingOrgListName = perfOrgListName, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now };
            sut.SavePerformingOrgList(perfOrgList);

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                var UpsertResourceList = (from r in gbe.PerformingOrganizationLists
                                          where r.PerformingOrganizationListName == perfOrgListName
                                          select new PerformingOrgListDTO
                                          {
                                              PerformingOrgListID = r.PerformingOrganizationListID,
                                              PerformingOrgListName = r.PerformingOrganizationListName,
                                              UpdateDate = r.UpdateDT
                                          }).FirstOrDefault();
                perfOrgList = UpsertResourceList as PerformingOrgListDTO;
            }

            Assert.AreEqual(perfOrgList.PerformingOrgListName, perfOrgListName, "Perf Org List Name's do not match");
            Assert.IsTrue(perfOrgList.PerformingOrgListID > 0, "Perf Org List ID was not > 0 ");

            // now edit the name
            PerformingOrgListDTO updatedPerfOrgList = perfOrgList;
            int ResourceListID = perfOrgList.PerformingOrgListID;
            perfOrgListName = Guid.NewGuid().ToString().Substring(0, 15);
            updatedPerfOrgList.PerformingOrgListName = perfOrgListName;
            updatedPerfOrgList.Updateable = UpdateType.Upsert;

            sut.SavePerformingOrgList(updatedPerfOrgList);

            updatedPerfOrgList = sut.GetPerfOrgList(ResourceListID);
            Assert.IsTrue(updatedPerfOrgList.PerformingOrgListID == ResourceListID, "Resource List ID's didn't match");
            Assert.AreEqual(updatedPerfOrgList.PerformingOrgListName, perfOrgListName, "Resource List Names do not match");

            this.ResetTestData();
        }
    }
}
