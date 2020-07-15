// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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
    public class ResourceListDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_GetResourceList()
        {
            var sut = new ResourceListDTODataLoader();
            ResourceListDTO resourceList = sut.GetResourceList(this.ResourceList.ResourceListID);
            Assert.IsTrue(resourceList != null, "The resource list was null");

        }


        [TestMethod]
        public void L_UpsertResourceList()
        {
            var sut = new ResourceListDTODataLoader();
            string resourceListName = Guid.NewGuid().ToString().Substring(0, 15);
            ResourceListDTO resourceList = new ResourceListDTO { ResourceListID = -1, ResourceListName = resourceListName, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now };
            sut.SaveResourceList(resourceList);


            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                var UpsertResourceList = (from r in gbe.ResourceLists
                                          where r.ResourceListName == resourceListName
                                          select new ResourceListDTO
                                          {
                                              ResourceListID = r.ResourceListID,
                                              ResourceListName = r.ResourceListName,
                                              UpdateDate = r.UpdateDT
                                          }).FirstOrDefault();
                resourceList = UpsertResourceList as ResourceListDTO;
            }

            Assert.AreEqual(resourceList.ResourceListName, resourceListName, "Resource List Name's do not match");
            Assert.IsTrue(resourceList.ResourceListID > 0, "Resource List ID was not > 0 ");

            // now edit the name
            ResourceListDTO updatedResourceList = resourceList;
            int ResourceListID = resourceList.ResourceListID;
            resourceListName = Guid.NewGuid().ToString().Substring(0, 15);
            updatedResourceList.ResourceListName = resourceListName;
            updatedResourceList.Updateable = UpdateType.Upsert;

            sut.SaveResourceList(updatedResourceList);

            updatedResourceList = sut.GetResourceList(ResourceListID);
            Assert.IsTrue(updatedResourceList.ResourceListID == ResourceListID, "Resource List ID's didn't match");
            Assert.AreEqual(updatedResourceList.ResourceListName, resourceListName, "Resource List Names do not match");

            ResetTestData();
        }

    }
}
