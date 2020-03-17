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
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WorkspaceVersionMetaDataDTODataLoaderTest : MOQLoaderObject
    {
        [TestMethod]
        public void L_DeleteWorkspaceVersion()
        {
            var sut = new WorkspaceVersionMetaDataDTODataLoader();

            // create a new workspace version so we can then delete it
            string guid = Guid.NewGuid().ToString();

            WorkspaceVersionMetaDataDTO version = new WorkspaceVersionMetaDataDTO { VersionID = -1, CreatedByID = this.Author.UserID, VersionName = guid, DateCreated = DateTime.Now, Updateable = UpdateType.Upsert };
            sut.Save(new Collection<WorkspaceVersionMetaDataDTO> { version }, this.Workspace.Id);

            int AfterSaveCount = sut.GetByWorkspaceID(this.Workspace.Id).Count();

            WorkspaceVersionMetaDataDTO versionToDelete = new WorkspaceVersionMetaDataDTO();

            Collection<WorkspaceVersionMetaDataDTO> workspaceVersions = sut.GetByWorkspaceID(this.Workspace.Id);

            foreach (WorkspaceVersionMetaDataDTO versionToCheck in workspaceVersions)
            {
                if (versionToCheck.VersionName == guid)
                {
                    versionToDelete = versionToCheck;
                    break;
                }

            }

            // now that we have found the version we just created, delete it
            sut.Delete(versionToDelete, this.Workspace.Id);

            int AfterDeleteCount = sut.GetByWorkspaceID(this.Workspace.Id).Count();
            Assert.AreEqual(AfterSaveCount - 1, AfterDeleteCount, "The delete didn't work");

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetAllWorkspaceVersions()
        {
            var sut = new WorkspaceVersionMetaDataDTODataLoader();

            // create a new workspace version so we can make sure there is at least one in the system
            string guid = Guid.NewGuid().ToString();

            WorkspaceVersionMetaDataDTO version = new WorkspaceVersionMetaDataDTO { VersionID = -1, CreatedByID = this.Author.UserID, VersionName = guid, DateCreated = DateTime.Now, Updateable = UpdateType.Upsert };
            sut.Save(new Collection<WorkspaceVersionMetaDataDTO> { version }, this.Workspace.Id);

            Collection<WorkspaceVersionMetaDataDTO> allVersions = sut.GetAllWorkspaceVersions();
            Assert.IsTrue(allVersions.Count > 0, "No versions were found");
        }

        [TestMethod]
        public void L_GetWorkspaceVersionByVersionID()
        {
            var sut = new WorkspaceVersionMetaDataDTODataLoader();

            // create a new workspace version so we can then look for it
            string guid = Guid.NewGuid().ToString().Substring(0, 20);

            WorkspaceVersionMetaDataDTO version = new WorkspaceVersionMetaDataDTO { VersionID = -1, CreatedByID = this.Author.UserID, VersionName = guid, DateCreated = DateTime.Now, Updateable = UpdateType.Upsert };
            sut.Save(new Collection<WorkspaceVersionMetaDataDTO> { version }, this.Workspace.Id);

            WorkspaceVersionMetaDataDTO versionToGet = new WorkspaceVersionMetaDataDTO();

            Collection<WorkspaceVersionMetaDataDTO> workspaceVersions = sut.GetByWorkspaceID(this.Workspace.Id);

            foreach (WorkspaceVersionMetaDataDTO versionToCheck in workspaceVersions)
            {
                if (versionToCheck.VersionName == guid)
                {
                    versionToGet = versionToCheck;
                    break;
                }

            }

            Assert.IsTrue(versionToGet.VersionID > 0, "Version ID isn't positive");
            Assert.IsTrue(versionToGet.VersionName == guid, "The name doesn't match");
            Assert.IsTrue(versionToGet.CreatedByID == this.Author.UserID, "Created by ID didn't match");
        }

        [TestMethod]
        public void L_GetWorkspaceVersionIDsByWorkspaceID()
        {
            var sut = new WorkspaceVersionMetaDataDTODataLoader();

            // create a new workspace version so we can then look for it
            string guid = Guid.NewGuid().ToString().Substring(0, 20);

            WorkspaceVersionMetaDataDTO version = new WorkspaceVersionMetaDataDTO { VersionID = -1, CreatedByID = this.Author.UserID, VersionName = guid, DateCreated = DateTime.Now, Updateable = UpdateType.Upsert };
            sut.Save(new Collection<WorkspaceVersionMetaDataDTO> { version }, this.Workspace.Id);

            Collection<WorkspaceVersionMetaDataDTO> workspaceVersions = sut.GetByWorkspaceID(this.Workspace.Id);

            Assert.IsTrue(workspaceVersions.Count > 0, "Workspace Versions don't exist");

        }

        [TestMethod]
        public void L_SaveWorkspaceVersions()
        {
            var sut = new WorkspaceVersionMetaDataDTODataLoader();

            // create a new workspace version so we can then look for it
            string guid = Guid.NewGuid().ToString().Substring(0, 20);

            WorkspaceVersionMetaDataDTO version = new WorkspaceVersionMetaDataDTO { VersionID = -1, CreatedByID = this.Author.UserID, VersionName = guid, DateCreated = DateTime.Now, Updateable = UpdateType.Upsert };

            int BeforeSaveCount = sut.GetByWorkspaceID(this.Workspace.Id).Count;
            sut.Save(new Collection<WorkspaceVersionMetaDataDTO> { version }, this.Workspace.Id);

            int AfterSaveCount = sut.GetByWorkspaceID(this.Workspace.Id).Count;

            Assert.AreEqual(BeforeSaveCount + 1, AfterSaveCount, "The save worked");

            // now do a delete through the save method
            int FirstID = sut.GetByWorkspaceID(this.Workspace.Id).First().Id;
            WorkspaceVersionMetaDataDTO toDeleteVersion = sut.GetByIds(new Collection<int>(){FirstID}).FirstOrDefault();
            toDeleteVersion.Updateable = UpdateType.Deleted;
            sut.Save(new Collection<WorkspaceVersionMetaDataDTO> { toDeleteVersion }, this.Workspace.Id);

            int AfterDeleteCount = sut.GetByWorkspaceID(this.Workspace.Id).Count;
            Assert.AreEqual(AfterSaveCount - 1, AfterDeleteCount, "the delete didn't work");
        }

        [TestMethod]
        public void L_UpsertWorkspaceVersion()
        {
            var sut = new WorkspaceVersionMetaDataDTODataLoader();

            // create a new workspace version so we can then look for it
            string guid = Guid.NewGuid().ToString().Substring(0, 20);

            WorkspaceVersionMetaDataDTO version = new WorkspaceVersionMetaDataDTO { VersionID = -1, CreatedByID = this.Author.UserID, VersionName = guid, DateCreated = DateTime.Now, Updateable = UpdateType.Upsert };

            int BeforeSaveCount = sut.GetByWorkspaceID(this.Workspace.Id).Count;

            sut.Upsert(version, this.Workspace.Id);
            int AfterSaveCount = sut.GetByWorkspaceID(this.Workspace.Id).Count;

            Assert.AreEqual(BeforeSaveCount + 1, AfterSaveCount, "The save worked");


           
        }

        [TestMethod]
        public void L_RestoreVersionSuccessful()
        {

            var sut = new WorkspaceVersionMetaDataDTODataLoader();

            // create a new workspace version so we can then look for it
            string guid = Guid.NewGuid().ToString().Substring(0, 20);

            WorkspaceVersionMetaDataDTO version = new WorkspaceVersionMetaDataDTO { VersionID = -1, CreatedByID = this.Author.UserID, VersionName = guid, DateCreated = DateTime.Now, Updateable = UpdateType.Upsert, WorkspaceID = this.Workspace.Id };

            int BeforeSaveCount = sut.GetByWorkspaceID(this.Workspace.Id).Count;
            sut.Save(new Collection<WorkspaceVersionMetaDataDTO> { version }, this.Workspace.Id);

            int AfterSaveCount = sut.GetByWorkspaceID(this.Workspace.Id).Count;

            Assert.AreEqual(BeforeSaveCount + 1, AfterSaveCount, "The save worked");

            WorkspaceVersionMetaDataDTO restoreVersion = sut.GetAllWorkspaceVersions().Where(x => x.VersionName == guid && x.WorkspaceID == this.Workspace.Id).First();

            string errors = sut.Restore(restoreVersion, this.Author.UserID);

            Assert.IsTrue(string.IsNullOrEmpty(errors), "not empty string");

            // reset data so that we don't get 2 restore attempts on 1 WS within 1 minute of each other which will cause a db error
            this.ResetTestData();
        }

        [TestMethod]
        public void L_RestoreVersionSuccessfulButWithMessages()
        {
            //TODO: Created Bug 15736 to change this mock test to use historical metrics
           //  var sut = new WorkspaceVersionMetaDataDTODataLoader();

           // // create a new workspace version so we can then look for it
           // string guid = Guid.NewGuid().ToString().Substring(0, 20);

           // // create new resource to use for this method, that way we can guarantee it's not "in use"
           // var resourceDataLoader = new ResourceDTODataLoader();
           // string resourceName1 = Guid.NewGuid().ToString().Substring(0, 5);
           // ResourceDTO resource1 = new ResourceDTO { ResourceID = -1, ResourceName = resourceName1, RateType = RateType.Hours, ResourceDesc = "mock resource", SegRegion = "E1", LaborType = "E3", ElementOfCost = ElementOfCostType.LMLabor, UpdateDate = DateTime.Now, Updateable = UpdateType.Upsert };
           // Dictionary<int, int> resourceDictionary = resourceDataLoader.SaveWorkspaceResources(new Collection<ResourceDTO> { resource1 }, this.Workspace.ResourceListID);
           // Assert.IsTrue(resourceDictionary.Count > 0, "save didn't work");


           // //create new perf org to use for this method, that way we can guarantee it's not "in use"
           // var perfOrgDataLoader = new PerformingOrgDTODataLoader();
           // string perfOrgName = Guid.NewGuid().ToString().Substring(0, 5);

           // PerformingOrgDTO perfOrg = new PerformingOrgDTO { PerformingOrgID = -1, PerformingOrgName = perfOrgName, PerformingOrgDesc = "First Perf Org", Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now };
           // perfOrgDataLoader.SaveSystemPerformingOrgs(new Collection<PerformingOrgDTO> { perfOrg });
           // int PerfOrgIDToUse = perfOrgDataLoader.GetPerformingOrgIDByPerformingOrgName(perfOrgName, 1);


           // //create task element with labor. the idea is to save this labor, create version, edit boe by changing the resource and perf org,
           // // delete those previously used resource and perf org, and then restore. the perf org and resource should be restored with no issue. 
           // // will end up with one warning about the historic metric
           // var boeLoader = new BoeTaskElementDTODataLoader();
           // BoeTaskElementDTO boeTask = boeLoader.GetBoeTaskElementByTaskElementID(this.TaskElement.BOETaskElementID);

           // int boeTaskElementID = boeTask.BOETaskElementID;
           // // delete any labor types now
           // foreach (GenBOE.ModelView.DTO.BOELaborType bl in boeTask.taskElementLabors)
           // {
           //     bl.Updateable = UpdateType.Deleted;
           //     boeLoader.DeleteLMLaborType(bl);
           // }

           // boeTask = boeLoader.GetBoeTaskElementByTaskElementID(this.TaskElement.BOETaskElementID);
           // Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();
           // boeTask.BoeID = this.Boe1.ID;
           // boeTask.StartDate = Convert.ToDateTime("09/01/2010");
           // boeTask.EndDate = Convert.ToDateTime("09/01/2010");
           // boeTask.MOQHoursEquation = "5000";
           // boeTask.MOQText = "MOQ";
           // boeTask.Updateable = UpdateType.Upsert;
           // string guidstring = Guid.NewGuid().ToString();
           // boeTask.TaskTitle = guidstring;

           // Dictionary<string, int> resourceNameDict = resourceDataLoader.GetResourceNameDictionaryByResourceListID(this.Workspace.ResourceListID);
           // int resourceIDToUse = resourceNameDict[resourceName1];

           // Collection<GenBOE.ModelView.DTO.BOELaborType> boeLabors = new Collection<GenBOE.ModelView.DTO.BOELaborType>();
           // GenBOE.ModelView.DTO.BOELaborType boeLabor = new GenBOE.ModelView.DTO.BOELaborType();
           // boeLabor.BoeID = this.Boe1.ID;
           // boeLabor.BOELaborTypeID = -1;
           // boeLabor.ValueSpread = 5000;
           // boeLabor.ResourceID = resourceIDToUse;
           // boeLabor.PerformingOrgID = PerfOrgIDToUse;
           // boeLabor.PercentSpread = 100m;
           // boeLabor.SpreadCurveID = SpreadCurves.SpreadCurve10;
           // boeLabor.StartDateValue = Convert.ToDateTime("09/01/2010");
           // boeLabor.EndDateValue = Convert.ToDateTime("09/01/2010");
           // boeLabor.Updateable = UpdateType.Upsert;

           // Collection<BoeLaborSpread> laborSpreads = new Collection<BoeLaborSpread>();
           // BoeLaborSpread LS = new BoeLaborSpread();
           // LS.BoeID = this.Boe1.ID;
           // LS.BOELaborSpreadID = -1;
           // LS.LaborSpreadDate = Convert.ToDateTime("09/01/2010");
           // LS.LaborSpreadValue = 5000;
           // LS.Updateable = UpdateType.Upsert;
           // laborSpreads.Add(LS);
           // boeLabor.LaborSpreads = laborSpreads;

           // boeLabors.Add(boeLabor);
           // boeTask.taskElementLabors = boeLabors;

           // foreach (BoeTaskOrdinaryVariable var in boeTask.OrdinaryVariables)
           // {
           //     var.Updateable = UpdateType.Upsert;
           // }

           // boeTaskElements.Add(boeTask);

           // boeLoader.SaveBoeTaskElements(boeTaskElements);

           // // now that we have saved the boe, create the workspace version

           // WorkspaceVersionMetaDataDTO version = new WorkspaceVersionMetaDataDTO { VersionID = -1, CreatedByID = this.Author.UserID, VersionName = guid, DateCreated = DateTime.Now, Updateable = UpdateType.Upsert, WorkspaceID = this.Workspace.WorkspaceID };


           // int BeforeSaveCount = sut.GetWorkspaceVersionIDsByWorkspaceID(this.Workspace.WorkspaceID).Count;
           // sut.SaveWorkspaceVersions(new Collection<WorkspaceVersionMetaDataDTO> { version }, this.Workspace.WorkspaceID);

           // int AfterSaveCount = sut.GetWorkspaceVersionIDsByWorkspaceID(this.Workspace.WorkspaceID).Count;

           // Assert.AreEqual(BeforeSaveCount + 1, AfterSaveCount, "The save worked");

           // // now that the version save worked. Edit the BOE and Delete the Global Perf and Resource ID
           // boeTask = boeLoader.GetBoeTaskElementByTaskElementID(boeTaskElementID);
           // int ResourceIDToDelete = boeTask.taskElementLabors[0].ResourceID.Value;
           // Assert.AreEqual(ResourceIDToDelete, resourceIDToUse, "reosurce IDs didn't match");
           // Assert.IsTrue(ResourceIDToDelete > 0, "no resource id ");
           // int PerfOrgIDToDelete = boeTask.taskElementLabors[0].PerformingOrgID.Value;

           // boeTask.taskElementLabors[0].Updateable = UpdateType.Upsert;

           // //create new resource to save the labor type to use
           // resourceName1 = Guid.NewGuid().ToString().Substring(0, 5);
           // ResourceDTO resource2 = new ResourceDTO { ResourceID = -1, ResourceName = resourceName1, RateType = RateType.Hours, ResourceDesc = "mock resource " + resourceName1, SegRegion = "E1", LaborType = "E3", ElementOfCost = ElementOfCostType.LMLabor, UpdateDate = DateTime.Now, Updateable = UpdateType.Upsert };
           // resourceDataLoader.SaveWorkspaceResources(new Collection<ResourceDTO> { resource2 }, this.Workspace.ResourceListID);
           // resourceNameDict = resourceDataLoader.GetResourceNameDictionaryByResourceListID(this.Workspace.ResourceListID);
           // int resourceID = resourceNameDict[resourceName1];

           // // create new performing org to save the labor type to use
           // perfOrgName = Guid.NewGuid().ToString().Substring(0, 5);
           // PerformingOrgDTO perfOrg2 = new PerformingOrgDTO { PerformingOrgID = -1, PerformingOrgName = perfOrgName, PerformingOrgDesc = "First Perf Org " + perfOrgName, Updateable = UpdateType.Upsert, UpdateDate = DateTime.Now };
           // perfOrgDataLoader.SaveWorkspacePerformingOrgs(new Collection<PerformingOrgDTO> { perfOrg2 }, this.PerfOrgList.PerformingOrgListID);
           // int PerfOrgID = perfOrgDataLoader.GetPerformingOrgIDByPerformingOrgName(perfOrgName, this.PerfOrgList.PerformingOrgListID);

           // // save boe labor with new resource and perf org
           // boeTask.taskElementLabors[0].ResourceID = resourceID;
           // boeTask.taskElementLabors[0].PerformingOrgID = PerfOrgID;
           // boeTask.Updateable = UpdateType.Upsert;
           // foreach (BoeTaskOrdinaryVariable var in boeTask.OrdinaryVariables)
           // {
           //     var.Updateable = UpdateType.Upsert;
           // }
           // boeLoader.SaveBoeTaskElements(new Collection<BoeTaskElementDTO> { boeTask });

           // // delete resource
           // resource1 = resourceDataLoader.GetResourcesByResourceIDsAndResourceListID(new Collection<int> { ResourceIDToDelete }, this.ResourceList.ResourceListID).FirstOrDefault();
           // resource1.Updateable = UpdateType.Deleted;
           // resourceDataLoader.SaveWorkspaceResources(new Collection<ResourceDTO> { resource1 }, this.Workspace.ResourceListID);

           // //delete perf org
           // perfOrg = perfOrgDataLoader.GetAllPerformingOrgsByListId(1).Where(x => x.PerformingOrgID == PerfOrgIDToDelete).FirstOrDefault();
           // perfOrg.Updateable = UpdateType.Deleted;
           // perfOrgDataLoader.SaveSystemPerformingOrgs(new Collection<PerformingOrgDTO> { perfOrg });

           // //restore workspace version
           // WorkspaceVersionMetaDataDTO restoreVersion = sut.GetAllWorkspaceVersions().Where(x => x.VersionName == guid && x.WorkspaceID == this.Workspace.WorkspaceID).First();

           // string errors = sut.RestoreWorkspaceVersion(restoreVersion, this.Author.UserID);

           // Assert.IsFalse(string.IsNullOrEmpty(errors), "not empty string");

           // // error message:
           // //PerformingOrganizationID: Y Performing Organization Name: Y Name no longer exists.
           //// Assert.IsTrue(errors.Contains("PerformingOrganizationID:"), "no perf org in error message <" + errors + ">");
           //// Assert.IsTrue(errors.Contains(PerfOrgIDToDelete.ToString()), "perf org id not in message <" + errors + ">");

           // this.ResetTestData();
        }

        /// <summary>
        /// Test GetBoesByVersionID
        /// </summary>
        [TestMethod]
        public void L_GetBoesByVersionID()
        {
            WorkspaceVersionMetaDataDTODataLoader sut = new WorkspaceVersionMetaDataDTODataLoader();

            // Create version to test for BOEs
            string guid = Guid.NewGuid().ToString();
            WorkspaceVersionMetaDataDTO version = new WorkspaceVersionMetaDataDTO { VersionID = -1, CreatedByID = this.Author.UserID, VersionName = guid, DateCreated = DateTime.Now, Updateable = UpdateType.Upsert };
            sut.Save(new Collection<WorkspaceVersionMetaDataDTO> { version }, this.Workspace.Id);
            ICollection<WorkspaceVersionMetaDataDTO> workspaceVersions = sut.GetByWorkspaceID(this.Workspace.Id);

            // Get the BOEs
            ICollection<BoeVersionDTO> result = sut.GetBoesByVersionID(workspaceVersions.First().VersionID, workspaceVersions.First().WorkspaceID);

            // Assert the BOEs are returned
            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.Any(x => x.BoeId == this.Boe1.Id));
            Assert.IsTrue(result.Any(x => x.BoeId == this.Boe2.Id));
            Assert.IsTrue(result.Any(x => x.BoeId == this.Boe3.Id));
        }
    }
}
