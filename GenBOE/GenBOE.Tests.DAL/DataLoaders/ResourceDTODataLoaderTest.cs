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
    using IES.Common.classes;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ResourceDTODataLoaderTest : MOQLoaderObject
    {
    

        [TestMethod]
        public void L_TestUniqueResourceNameDesc()
        {
            var sut = new ResourceDTODataLoader();
            // add brand new resource
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();
            string resourceName1 = "TwentyCharactersDone";
            string resourceDesc = "TestForLengthIncreaseTestForLengthIncrease12345678";
            ResourceDTO resource1 = new ResourceDTO { Id = -1, ResourceName = resourceName1, RateType = RateType.Hours, ResourceDesc = resourceDesc, SegRegion = "E1", LaborType = "E3", ElementOfCost = ElementOfCostType.LMLabor, UpdateDate = DateTime.Now, Updateable = UpdateType.Upsert };
            resources.Add(resource1);

            WorkspaceDTO tempWorkspace = new WorkspaceDTO();
            tempWorkspace.ResourceListID = this.ResourceList.ResourceListID;

            sut.SaveWorkspaceResources(tempWorkspace, resources);

            Assert.IsFalse(sut.IsResourceNameUnique(resourceName1, this.ResourceList.ResourceListID, null));
            Assert.IsFalse(sut.IsResourceDescriptionUnique(resourceDesc, this.ResourceList.ResourceListID, null));
            
            this.ResetTestData();
        }

        [TestMethod]
        public void L_SaveUpsertResources()
        {
            var sut = new ResourceDTODataLoader();
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();

            // add brand new resources
            string resourceName1 = " " + Guid.NewGuid().ToString().Substring(0, 5) + " ";
            string resourceName2 = Guid.NewGuid().ToString().Substring(0, 5);
            ResourceDTO resource1 = new ResourceDTO { Id = -1, ResourceName = resourceName1, RateType = RateType.Hours, ResourceDesc = "The three e", SegRegion = "E1", LaborType = "E3", ElementOfCost = ElementOfCostType.LMLabor, UpdateDate = DateTime.Now, Updateable = UpdateType.Upsert };
            ResourceDTO resource2 = new ResourceDTO { Id = -2, ResourceName = resourceName2, RateType = RateType.Hours, ResourceDesc = "two four 6", SegRegion = "E6", LaborType = "G9", ElementOfCost = ElementOfCostType.LMLabor, UpdateDate = DateTime.Now, Updateable = UpdateType.Upsert };
            resources.Add(resource1);
            resources.Add(resource2);

            WorkspaceDTO tempWorkspace = new WorkspaceDTO();
            tempWorkspace.ResourceListID = this.ResourceList.ResourceListID;

            Dictionary<int, int> ids = sut.SaveWorkspaceResources(tempWorkspace, resources);
            Assert.AreEqual(2, ids.Count);

            ResourceDTO firstSaved = sut.GetByIds(new Collection<int> { ids[-1] }).First();
            Assert.AreEqual(resource1.ResourceDesc.Trim(), firstSaved.ResourceDesc);
            Assert.AreEqual(resource1.SegRegion.Trim(), firstSaved.SegRegion);
            Assert.AreEqual(resource1.ResourceName.Trim(), firstSaved.ResourceName);
            Assert.AreEqual(resource1.LaborType.Trim(), firstSaved.LaborType);

            ResourceDTO secondSaved = sut.GetByIds(new Collection<int> { ids[-2] }).First();
            Assert.AreEqual(resource2.ResourceDesc, secondSaved.ResourceDesc);
            Assert.AreEqual(resource2.SegRegion, secondSaved.SegRegion);
            Assert.IsTrue(resource1.isSystemResource == false, "the resource was a system one");
            Assert.IsTrue(resource2.isSystemResource == false, "the resource was a system one");

            // edit resource
            int changedResourceID = firstSaved.Id;
            string segRegionTemp = "TestForLengthIncreaseTestForLengthIncrease12345678";
            firstSaved.SegRegion = segRegionTemp;
            string laborTypeTemp = "TestForLengthIncreaseTestForLengthIncrease12345678";
            firstSaved.LaborType = laborTypeTemp;
            string resourceNameTemp = Guid.NewGuid().ToString().Substring(0, 15);
            firstSaved.ResourceName = resourceNameTemp;
            firstSaved.Updateable = UpdateType.Upsert;
            resources = new Collection<ResourceDTO>();
            resources.Add(firstSaved);

            ids = sut.SaveWorkspaceResources(tempWorkspace, resources);
            Assert.AreEqual(1, ids.Count);
            ResourceDTO firstEdited = sut.GetByIds(new Collection<int> { firstSaved.Id }).First();
            Assert.AreEqual(firstEdited.SegRegion, segRegionTemp, "Seg Region didn't save");
            Assert.IsTrue(firstEdited.SegRegion.Length == 50, "String wasn't 50");
            Assert.AreEqual(firstEdited.LaborType, laborTypeTemp, "Labor Type didn't save");
            Assert.IsTrue(firstEdited.LaborType.Length == 50, "String wasn't 50");
            Assert.AreEqual(firstEdited.ResourceName, resourceNameTemp, "Resource name didn't save");
            Assert.AreEqual(changedResourceID, firstEdited.Id, "Resource IDs didn't match");

            // clean up both resources, first refresh our copy
            ICollection<ResourceDTO> dtos = sut.GetByIds(new Collection<int> { resource1.Id, resource2.Id });
            resource1 = dtos.Where(x => x.Id == resource1.Id).First();
            resource2 = dtos.Where(x => x.Id == resource2.Id).First();

            resource1.Updateable = UpdateType.Deleted;
            resource2.Updateable = UpdateType.Deleted;

            sut.SaveWorkspaceResources(tempWorkspace, new Collection<ResourceDTO> { resource1, resource2 });

            // confirm deletes
            Assert.AreEqual(dtos.Count - 2, sut.GetByIds(new Collection<int> { resource1.Id, resource2.Id }).Count);

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetResourceIDsByListID()
        {
            var sut = new ResourceDTODataLoader();

            ICollection<int> resourceIDs = sut.GetByListId(this.Workspace.ResourceListID).Select(x => x.Id).ToList();
            Assert.IsTrue(resourceIDs.Count > 0, "no resource IDs were found for the given works'aces resource list id");

            // get system resource
            resourceIDs = sut.GetByListId(CommonConstants.GLOBAL_LIST_ID).Select(x => x.Id).ToList();
            Assert.IsTrue(resourceIDs.Count > 0, "no system resource IDs were found");
        }

        [TestMethod]
        public void L_GetResourceByNameAndListID()
        {
            var sut = new ResourceDTODataLoader();

            // should return a null resource and not an error
            ResourceDTO resource = sut.GetByNameAndListId("", CommonConstants.GLOBAL_LIST_ID);
            Assert.IsNull(resource, "Resource should not be found and return null");
        }

        [TestMethod]
        public void L_GetResourceIDByResourceDescription()
        {
            var sut = new ResourceDTODataLoader();

            int resourceID = sut.GetByDescriptionAndListId(this.Resource.ResourceDesc, this.Workspace.ResourceListID).Id;
            Assert.IsTrue(resourceID > 0, "no resource ID was found with that description");

            // get a system resource by description
            ResourceDTO moq = sut.GetByDescriptionAndListId("MOQ1 - System Test", CommonConstants.GLOBAL_LIST_ID);
            if (moq == null || moq.Id <= 0)
            {
                sut.SaveSystemResources(new Collection<ResourceDTO> { new ResourceDTO { ResourceName = "MOQ1", RateType = RateType.Hours, ResourceDesc = "MOQ1 - System Test", isSystemResource = true, Segment = SegmentType.SSC, Updateable = UpdateType.Upsert, ElementOfCost = ElementOfCostType.LMLabor } });
            }
            resourceID = sut.GetByDescriptionAndListId("MOQ1 - System Test", CommonConstants.GLOBAL_LIST_ID).Id;
            Assert.IsTrue(resourceID > 0, "no system resource ID was found with that description");

        }

        [TestMethod]
        public void L_SaveUpsertSystemResources()
        {
            var sut = new ResourceDTODataLoader();
            Collection<ResourceDTO> resources = new Collection<ResourceDTO>();

            // add brand new resources
            string resourceName1 = Guid.NewGuid().ToString().Substring(0, 5);
            string resourceName2 = Guid.NewGuid().ToString().Substring(0, 5);
            ResourceDTO resource1 = new ResourceDTO { Id = -1, ResourceName = resourceName1, RateType = RateType.Hours, ResourceDesc = "The three e", SegRegion = "E1", LaborType = "E3", ElementOfCost = ElementOfCostType.LMLabor, UpdateDate = DateTime.Now, Updateable = UpdateType.Upsert };
            ResourceDTO resource2 = new ResourceDTO { Id = -2, ResourceName = resourceName2, RateType = RateType.Hours, ResourceDesc = "two four 6", SegRegion = "E6", LaborType = "G9", ElementOfCost = ElementOfCostType.LMLabor, UpdateDate = DateTime.Now, Updateable = UpdateType.Upsert };
            resources.Add(resource1);
            resources.Add(resource2);
            Dictionary<int, int> ids = sut.SaveSystemResources(resources);

            //check that system resources saved
            Assert.AreEqual(2, ids.Count);
            ResourceDTO firstSaved = sut.GetByIds(new Collection<int> { ids[-1] }).First();
            Assert.AreEqual(resource1.ResourceDesc, firstSaved.ResourceDesc);
            Assert.AreEqual(resource1.SegRegion, firstSaved.SegRegion);
            ResourceDTO secondSaved = sut.GetByIds(new Collection<int> { ids[-2] }).First();
            Assert.AreEqual(resource2.ResourceDesc, secondSaved.ResourceDesc);
            Assert.AreEqual(resource2.SegRegion, secondSaved.SegRegion);
            Assert.IsTrue(firstSaved.isSystemResource == true, "resource was actually a workspace one");
            Assert.IsTrue(secondSaved.isSystemResource == true, "resource was actually a workspace one");

            // edit resource
            string segRegionTemp = "TestForLengthIncreaseTestForLengthIncrease12345678";
            firstSaved.SegRegion = segRegionTemp;
            string laborTypeTemp = "TestForLengthIncreaseTestForLengthIncrease12345678";
            firstSaved.LaborType = laborTypeTemp;
            string resourceNameTemp = Guid.NewGuid().ToString().Substring(0, 15);
            firstSaved.ResourceName = resourceNameTemp;
            firstSaved.Updateable = UpdateType.Upsert;
            resources = new Collection<ResourceDTO>();
            resources.Add(firstSaved);

            ids = sut.SaveSystemResources(resources);
            Assert.AreEqual(1, ids.Count);

            // if an edit is made to a system resource, it creates a new row so existing workspaces that point to that resource still keep a link to the unedited version
            ResourceDTO firstEdited = sut.GetByIds(new Collection<int> { ids[firstSaved.Id] }).First();
            Assert.AreEqual(firstEdited.SegRegion, segRegionTemp, "Seg Region didn't save");
            Assert.IsTrue(firstEdited.SegRegion.Length == 50, "String wasn't 50");
            Assert.AreEqual(firstEdited.LaborType, laborTypeTemp, "Labor Type didn't save");
            Assert.IsTrue(firstEdited.LaborType.Length == 50, "String wasn't 50");
            Assert.AreEqual(firstEdited.ResourceName, resourceNameTemp, "Resource name didn't save");
            Assert.AreEqual(ids[firstSaved.Id], firstEdited.Id, "Resource IDs didn't match");



            // clean up both resources, first refresh our copy
            ICollection<ResourceDTO> dtos = sut.GetByIds(new Collection<int> { firstEdited.Id, resource2.Id });
            resource1 = dtos.Where(x => x.Id == firstEdited.Id).First();
            resource2 = dtos.Where(x => x.Id == resource2.Id).First();

            resource1.Updateable = UpdateType.Deleted;
            resource2.Updateable = UpdateType.Deleted;

            sut.SaveSystemResources(new Collection<ResourceDTO> { resource1, resource2 });

            // confirm deletes but not by checking count decrease. check that calling an get on their ID returns no DTOs. this is because rows marked as deleted aren't returned
            dtos = sut.GetByIds(new Collection<int> { firstEdited.Id, resource2.Id });
            Assert.AreEqual(2, dtos.Count);

            this.ResetTestData();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "newAvg"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "originalAvg")]
        //[TestMethod]
        public void TestOldVsNew()
        {
            //ResourceDTODataLoader loader = new ResourceDTODataLoader();

            //Stopwatch sw = new Stopwatch();
            //List<ResourceDTO> originalData = new List<ResourceDTO>();
            //List<ResourceDTO> newData = new List<ResourceDTO>();
            //List<long> originalTimes = new List<long>();
            //List<long> newTimes = new List<long>();

            //List<int> ids = new List<int>();

            //{
            //    int maxItems = 30;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.Resources.OrderBy(x => new Guid()).Select(x => x.ResourceListID).Take(maxItems).ToList();
            //    }

            //    ids.Add(loader.GlobalListID);

            //    for (int i = 0; i < ids.Count; i++) { sw.Restart(); originalData.AddRange(loader.GetByListId_OLD(ids.ElementAt(i))); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < ids.Count; i++) { sw.Restart(); newData.AddRange(loader.GetByListId(ids.ElementAt(i))); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<ResourceDTO>(); newData = new List<ResourceDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();

            //    int maxItems = 50;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.Resources.OrderBy(x => new Guid()).Select(x => x.ResourceID).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < 5; i++) { sw.Restart(); originalData.AddRange(loader.GetByIds_OLD(ids)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < 5; i++) { sw.Restart(); newData.AddRange(loader.GetByIds(ids)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<ResourceDTO>(); newData = new List<ResourceDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();
            //    List<dynamic> data = new List<dynamic>();

            //    int maxItems = 30;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        data = gbe.Resources.OrderBy(x => new Guid()).Select(x => new { name = x.ResourceName, listId = x.ResourceListID }).Take(maxItems).ToList<dynamic>();
            //        data.Add(gbe.Resources.Where(x => x.ResourceListID == loader.GlobalListID).OrderBy(x => new Guid())
            //            .Select(x => new { name = x.ResourceName, listId = x.ResourceListID }).First());
            //    }

            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); originalData.Add(loader.GetByNameAndListId_OLD(data.ElementAt(i).name, data.ElementAt(i).listId)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); newData.Add(loader.GetByNameAndListId(data.ElementAt(i).name, data.ElementAt(i).listId)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<ResourceDTO>(); newData = new List<ResourceDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();
            //    List<dynamic> data = new List<dynamic>();

            //    int maxItems = 30;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        data = gbe.Resources.OrderBy(x => new Guid()).Select(x => new { desc = x.ResourceDescription, listId = x.ResourceListID }).Take(maxItems).ToList<dynamic>();
            //        data.Add(gbe.Resources.Where(x => x.ResourceListID == loader.GlobalListID).OrderBy(x => new Guid())
            //            .Select(x => new { desc = x.ResourceDescription, listId = x.ResourceListID }).First());
            //    }

            //    for (int i = 0; i < data.Count; i++) { sw.Restart(); originalData.Add(loader.GetByDescriptionAndListId_OLD(data.ElementAt(i).desc, data.ElementAt(i).listId)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < data.Count; i++) { sw.Restart(); newData.Add(loader.GetByDescriptionAndListId(data.ElementAt(i).desc, data.ElementAt(i).listId)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    List<bool> originalDataBool = new List<bool>(); List<bool> newDataBool = new List<bool>();
            //    List<dynamic> data;

            //    int maxItems = 20;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        data = gbe.Resources.OrderBy(x => new Guid()).Take(maxItems).Select(x => new { id = (int?)x.ResourceID, desc = x.ResourceDescription, listId = x.ResourceListID }).ToList<dynamic>();
            //    }
            //    data.Add(new { id = (int?)null, desc = data.ElementAt(0).desc, listId = data.ElementAt(0).listId });
            //    data.Add(new { id = (int?)(data.ElementAt(1).id + 12), desc = data.ElementAt(1).desc, listId = data.ElementAt(1).listId });
            //    data.Add(new { id = (int?)(data.ElementAt(3).id + 43), desc = data.ElementAt(3).desc, listId = data.ElementAt(3).listId });
            //    data.Add(new { id = (int?)(data.ElementAt(6).id + 21), desc = data.ElementAt(6).desc, listId = data.ElementAt(6).listId });

            //    for (int i = 0; i < data.Count; i++) { sw.Restart(); originalDataBool.Add(loader.IsResourceDescriptionUnique_OLD(data.ElementAt(i).desc, data.ElementAt(i).listId, data.ElementAt(i).id)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < data.Count; i++) { sw.Restart(); newDataBool.Add(loader.IsResourceDescriptionUnique(data.ElementAt(i).desc, data.ElementAt(i).listId, data.ElementAt(i).id)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    Assert.AreEqual(originalDataBool.Count, newDataBool.Count);
            //    for (int i = 0; i < originalDataBool.Count; i++)
            //    {
            //        Assert.AreEqual(originalDataBool.ElementAt(i), newDataBool.ElementAt(i));
            //    }
            //}

            //{
            //    List<bool> originalDataBool = new List<bool>(); List<bool> newDataBool = new List<bool>();
            //    List<dynamic> data;

            //    int maxItems = 20;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        data = gbe.Resources.OrderBy(x => new Guid()).Take(maxItems).Select(x => new { id = (int?)x.ResourceID, name = x.ResourceName, listId = x.ResourceListID }).ToList<dynamic>();
            //    }
            //    data.Add(new { id = (int?)null, name = data.ElementAt(0).name, listId = data.ElementAt(0).listId });
            //    data.Add(new { id = (int?)(data.ElementAt(1).id + 12), name = data.ElementAt(1).name, listId = data.ElementAt(1).listId });
            //    data.Add(new { id = (int?)(data.ElementAt(3).id + 43), name = data.ElementAt(3).name, listId = data.ElementAt(3).listId });
            //    data.Add(new { id = (int?)(data.ElementAt(6).id + 21), name = data.ElementAt(6).name, listId = data.ElementAt(6).listId });

            //    for (int i = 0; i < data.Count; i++) { sw.Restart(); originalDataBool.Add(loader.IsResourceNameUnique_OLD(data.ElementAt(i).name, data.ElementAt(i).listId, data.ElementAt(i).id)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < data.Count; i++) { sw.Restart(); newDataBool.Add(loader.IsResourceNameUnique(data.ElementAt(i).name, data.ElementAt(i).listId, data.ElementAt(i).id)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    Assert.AreEqual(originalDataBool.Count, newDataBool.Count);
            //    for (int i = 0; i < originalDataBool.Count; i++)
            //    {
            //        Assert.AreEqual(originalDataBool.ElementAt(i), newDataBool.ElementAt(i));
            //    }
            //}
        }

        public static void VerifyCollections(ICollection<ResourceDTO> collection1, ICollection<ResourceDTO> collection2, bool skipFieldsNotRestoredFromBackup = false)
        {
            Assert.AreEqual(collection1.Count, collection2.Count);
            for (int i = 0; i < collection1.Count; i++)
            {
                VerifyDtos(collection1.ElementAt(i), collection2.ElementAt(i), skipFieldsNotRestoredFromBackup);
            }
        }

        public static void VerifyDtos(ResourceDTO dto1, ResourceDTO dto2, bool skipFieldsNotRestoredFromBackup = false)
        {
            if(!skipFieldsNotRestoredFromBackup)
            {
                Assert.AreEqual(dto1.Id, dto2.Id);
            }

            Assert.AreEqual(dto1.BurdenPool, dto2.BurdenPool);
            Assert.AreEqual(dto1.CalculatedSegment, dto2.CalculatedSegment);
            Assert.AreEqual(dto1.ElementOfCost, dto2.ElementOfCost);
            Assert.AreEqual(dto1.isSystemResource, dto2.isSystemResource);
            Assert.AreEqual(dto1.LaborType, dto2.LaborType);
            Assert.AreEqual(dto1.RateType, dto2.RateType);
            Assert.AreEqual(dto1.RateTypeString, dto2.RateTypeString);
            Assert.AreEqual(dto1.ResourceDesc, dto2.ResourceDesc);
            Assert.AreEqual(dto1.ResourceName, dto2.ResourceName);
            Assert.AreEqual(dto1.ResourceTypeCategory, dto2.ResourceTypeCategory);
            Assert.AreEqual(dto1.Segment, dto2.Segment);
            Assert.AreEqual(dto1.SegRegion, dto2.SegRegion);
            Assert.AreEqual(dto1.UpdateDate, dto2.UpdateDate);
        }
    }
}