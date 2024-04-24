// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.CustomFields
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.CustomFields;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
    using GenBOE.DataBridge.Reference;
    using GenBOE.Dtos;
    using GenBOE.Objects;
	using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RestoreDefaultOptionsTestCase : MOQObject
    {
        Mock<IPerformingOrgDTODataLoader> perfOrgLoader;
		Mock<IResourceDTODataLoader> resourceLoader;
		Mock<IWorkspaceDTODataLoader> workspaceLoader;
		Mock<IInUseDataLoader> inUseLoader;
		Mock<IFullObjectFactory> factory;
		Mock<IRetriever> retriever;

		private RestoreDefaultOptions CreateSUT()
		{
			perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			resourceLoader = new Mock<IResourceDTODataLoader>();
			workspaceLoader = new Mock<IWorkspaceDTODataLoader>();
			inUseLoader = new Mock<IInUseDataLoader>();
			factory = new Mock<IFullObjectFactory>();
			retriever = new Mock<IRetriever>();

			Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
			Mock<IPermissionsDTODataLoader> _permissions = new Mock<IPermissionsDTODataLoader>();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

			return new RestoreDefaultOptions(inUseLoader.Object, workspaceLoader.Object, perfOrgLoader.Object, resourceLoader.Object);
		}

		[TestMethod]
        public void RestorePerfOrg()
		{
			RestoreDefaultOptions sut = CreateSUT();

			// the default list to use
			Collection<PerformingOrgDTO> DefaultPerfOrgs = new Collection<PerformingOrgDTO> {
                new PerformingOrgDTO{Id=1, PerformingOrgName="PerfOrg1", PerformingOrgDesc="Perf Org 1"},
                new PerformingOrgDTO{Id=2, PerformingOrgName="PerfOrg2", PerformingOrgDesc="Perf Org 2 GLOBAL"},
                new PerformingOrgDTO{Id=3, PerformingOrgName="PerfOrg3", PerformingOrgDesc="Perf Org 3"},
                new PerformingOrgDTO{Id=4, PerformingOrgName="PerfOrg4", PerformingOrgDesc="Perf Org 4"},
                new PerformingOrgDTO{Id=5, PerformingOrgName="PerfOrg5", PerformingOrgDesc="Perf Org 5"},
                new PerformingOrgDTO{Id=6, PerformingOrgName="PerfOrg6", PerformingOrgDesc="Perf Org 6"}
            };

            // represents the current list to compare with the default one
            Collection<PerformingOrgDTO> perfOrgs = new Collection<PerformingOrgDTO> {
                new PerformingOrgDTO{Id=1, PerformingOrgName="PerfOrg1", PerformingOrgDesc="Perf Org 1"},
                new PerformingOrgDTO{Id=2, PerformingOrgName="PerfOrg2", PerformingOrgDesc="Perf Org 2"},
                new PerformingOrgDTO{Id=3, PerformingOrgName="PerfOrg3", PerformingOrgDesc="Perf Org 3 NOT GLOBAL"},
                new PerformingOrgDTO{Id=4, PerformingOrgName="PerfOrg4", PerformingOrgDesc="Perf Org 4"},
                new PerformingOrgDTO{Id=7, PerformingOrgName="PerfOrg7", PerformingOrgDesc="Perf Org 7"}
            };

            WorkspaceDTO workspace = new WorkspaceDTO {Id=this.Workspace.Id, PerfOrgListID=this.PerfOrgList.PerformingOrgListID };
            FullWorkspace ws = new FullWorkspace(workspace);

            retriever.Setup(x => x.GetPerformingOrgsByListId(this.PerfOrgList.PerformingOrgListID)).Returns(perfOrgs);
            perfOrgLoader.Setup(x => x.GetGlobalPerformingOrgs()).Returns(DefaultPerfOrgs);

            // performingOrgID of 1 is in use in this case
            HashSet<int> returnedIDs = new HashSet<int> { 1 };
            inUseLoader.Setup(x => x.GetWorkspacePerfOrgIDsInUseByPerfOrgListID(workspace.PerfOrgListID)).Returns(returnedIDs);

            RestoreOptionData optionsRestored = sut.RestorePerfOrg(ws);

            // there should be 2 options added, PerfOrg5 and PerfOrg6
            Assert.IsTrue(optionsRestored.OptionAdded.Count == 2, "Two options were not added");

            // there should be 2 options changed, PerfOrg2 and PerfOrg3. 
            // the result should have desc Perf Org 2 to Perf Org 2 GLOBAL
            // the result should have desc Perf Org 3 NOT GLOBAL to Perf Org
            Assert.IsTrue(optionsRestored.OptionChanged.Count == 2, "Two options were not changed");

            // there should be 1 option deleted, PerfOrg7
            Assert.IsTrue(optionsRestored.OptionDeleted.Count == 1, "Option was not deleted");
            Assert.IsTrue(optionsRestored.OptionDeleted[0].Name == "PerfOrg7", "PerfOrg7 was not the deleted one");

            // there should be 1 option not changed, PerfOrg1
            Assert.IsTrue(optionsRestored.OptionNotChanged.Count == 1, "OPtion was not changed");
            Assert.IsTrue(optionsRestored.OptionNotChanged[0].Name == "PerfOrg1", "PergOrg1 was not marked as in use");
        }

		/// <summary>
		/// Test RestoreSystemResources
		/// </summary>
		[TestMethod]
		public void TestRestoreSystemResources()
		{
			RestoreDefaultOptions sut = CreateSUT();

			// Set up resources so that resources 1 and 7 are unchanged, 2 and 3 were updated in the ws, 4 and 5 were deleted from the system, and 6 was added to the system
			string resourceName1 = "Res1";
			string resourceName2 = "Res2";
			string resourceName3 = "Res3";
			string resourceName4 = "Res4";
			string resourceName5 = "Res5";
			string resourceName6 = "Res6";
			string resourceName7 = "Res7";

			ICollection<ResourceDTO> systemResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = resourceName1, ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = resourceName2, ResourceDesc = "System Resource 2", SegRegion = "Region 2", LaborType = "Labor" },
				new ResourceDTO() { Id = 3, ResourceName = resourceName3, ResourceDesc = "System Resource 3", SegRegion = "Region 3", LaborType = "Labor" },
				new ResourceDTO() { Id = 6, ResourceName = resourceName6, ResourceDesc = "System Resource 6", SegRegion = "Region 6", LaborType = "Labor" },
				new ResourceDTO() { Id = 7, ResourceName = resourceName7, ResourceDesc = "System Resource 7", SegRegion = "Region 7", LaborType = "Labor" }
			};

			ICollection<ResourceDTO> workspaceResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = resourceName1, ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = resourceName2, ResourceDesc = "CHANGED Workspace Resource 2", SegRegion = "Region 2", LaborType = "Labor" },
				new ResourceDTO() { Id = 3, ResourceName = resourceName3, ResourceDesc = "System Resource 3", SegRegion = "CHANGED Region 3", LaborType = "Labor" },
				new ResourceDTO() { Id = 4, ResourceName = resourceName4, ResourceDesc = "System Resource 4", SegRegion = "Region 4", LaborType = "Labor" },
				new ResourceDTO() { Id = 5, ResourceName = resourceName5, ResourceDesc = "System Resource 5", SegRegion = "Region 5", LaborType = "Labor" },
				new ResourceDTO() { Id = 7, ResourceName = resourceName7, ResourceDesc = "System Resource 7", SegRegion = "Region 7", LaborType = "Labor" }
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = this.Workspace.Id, ResourceListID = this.ResourceList.ResourceListID };
			FullWorkspace ws = new FullWorkspace(workspace);

			retriever.Setup(x => x.GetResourcesByResourceListId(this.ResourceList.ResourceListID)).Returns(workspaceResources);
			resourceLoader.Setup(x => x.GetGlobalResources()).Returns(systemResources);
			resourceLoader.Setup(x => x.SaveWorkspaceResources(workspace, It.IsAny<List<ResourceDTO>>())).Returns(new Dictionary<int, int>());
			
			// set resources 3 and 5 as in-use to not be affected
			inUseLoader.Setup(x => x.GetWorkspaceResourceIDsInUseByListID(this.ResourceList.ResourceListID)).Returns(new HashSet<int>() { 3, 5, 7 });

			RestoreOptionData result = sut.RestoreSystemResources(ws);

			Assert.IsTrue(result.OptionAdded.Any(x => x.Name == resourceName6));
			Assert.IsTrue(result.OptionChanged.Any(x => x.Name == resourceName2));
			Assert.IsFalse(result.OptionChanged.Any(x => x.Name == resourceName3));
			Assert.IsTrue(result.OptionDeleted.Any(x => x.Name == resourceName4));
			Assert.IsFalse(result.OptionDeleted.Any(x => x.Name == resourceName5));
			Assert.IsTrue(result.OptionNotChanged.Any(x => x.Name == resourceName3));
			Assert.IsTrue(result.OptionNotChanged.Any(x => x.Name == resourceName5));
			Assert.IsTrue(result.OptionNotChanged.Any(x => x.Name == resourceName7));
		}

		/// <summary>
		/// Test ResourcesHaveChanged returns true when a system resource has been added
		/// </summary>
		[TestMethod]
		public void TestResourcesHaveChangedAddedResources()
		{
			RestoreDefaultOptions sut = CreateSUT();

			ICollection<ResourceDTO> systemResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = "Res1", ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = "Res2", ResourceDesc = "System Resource 2", SegRegion = "Region 2", LaborType = "Labor" },
				new ResourceDTO() { Id = 3, ResourceName = "Res3", ResourceDesc = "System Resource 3", SegRegion = "Region 3", LaborType = "Labor" },
				new ResourceDTO() { Id = 4, ResourceName = "Res4", ResourceDesc = "System Resource 4", SegRegion = "Region 4", LaborType = "Labor" }
			};

			ICollection<ResourceDTO> workspaceResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = "Res1", ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = "Res2", ResourceDesc = "System Resource 2", SegRegion = "Region 2", LaborType = "Labor" },
				new ResourceDTO() { Id = 3, ResourceName = "Res3", ResourceDesc = "System Resource 3", SegRegion = "Region 3", LaborType = "Labor" }
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = this.Workspace.Id, ResourceListID = this.ResourceList.ResourceListID };
			FullWorkspace ws = new FullWorkspace(workspace);

			retriever.Setup(x => x.GetResourcesByResourceListId(this.ResourceList.ResourceListID)).Returns(workspaceResources);
			resourceLoader.Setup(x => x.GetGlobalResources()).Returns(systemResources);
			inUseLoader.Setup(x => x.GetWorkspaceResourceIDsInUseByListID(this.ResourceList.ResourceListID)).Returns(new HashSet<int>());

			bool result = sut.ResourcesHaveChanged(ws);

			Assert.IsTrue(result);
		}

		/// <summary>
		/// Test ResourcesHaveChanged returns true when a workspace resource has been changed
		/// </summary>
		[TestMethod]
		public void TestResourcesHaveChangedChangedResources()
		{
			RestoreDefaultOptions sut = CreateSUT();

			ICollection<ResourceDTO> systemResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = "Res1", ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = "Res2", ResourceDesc = "System Resource 2", SegRegion = "Region 2", LaborType = "Labor" },
				new ResourceDTO() { Id = 3, ResourceName = "Res3", ResourceDesc = "System Resource 3", SegRegion = "Region 3", LaborType = "Labor" }
			};

			ICollection<ResourceDTO> workspaceResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = "Res1", ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = "Res2", ResourceDesc = "CHANGED Workspace Resource 2", SegRegion = "Region 2", LaborType = "Labor" },
				new ResourceDTO() { Id = 3, ResourceName = "Res3", ResourceDesc = "System Resource 3", SegRegion = "Region 3", LaborType = "Labor" }
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = this.Workspace.Id, ResourceListID = this.ResourceList.ResourceListID };
			FullWorkspace ws = new FullWorkspace(workspace);

			retriever.Setup(x => x.GetResourcesByResourceListId(this.ResourceList.ResourceListID)).Returns(workspaceResources);
			resourceLoader.Setup(x => x.GetGlobalResources()).Returns(systemResources);
			inUseLoader.Setup(x => x.GetWorkspaceResourceIDsInUseByListID(this.ResourceList.ResourceListID)).Returns(new HashSet<int>());

			bool result = sut.ResourcesHaveChanged(ws);

			Assert.IsTrue(result);
		}

		/// <summary>
		/// Test ResourcesHaveChanged returns false when a workspace resource has been changed but it's in use
		/// </summary>
		[TestMethod]
		public void TestResourcesHaveChangedChangedResourcesInUse()
		{
			RestoreDefaultOptions sut = CreateSUT();

			ICollection<ResourceDTO> systemResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = "Res1", ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = "Res2", ResourceDesc = "System Resource 2", SegRegion = "Region 2", LaborType = "Labor" },
				new ResourceDTO() { Id = 3, ResourceName = "Res3", ResourceDesc = "System Resource 3", SegRegion = "Region 3", LaborType = "Labor" }
			};

			ICollection<ResourceDTO> workspaceResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = "Res1", ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = "Res2", ResourceDesc = "CHANGED Workspace Resource 2", SegRegion = "Region 2", LaborType = "Labor" },
				new ResourceDTO() { Id = 3, ResourceName = "Res3", ResourceDesc = "System Resource 3", SegRegion = "Region 3", LaborType = "Labor" }
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = this.Workspace.Id, ResourceListID = this.ResourceList.ResourceListID };
			FullWorkspace ws = new FullWorkspace(workspace);

			retriever.Setup(x => x.GetResourcesByResourceListId(this.ResourceList.ResourceListID)).Returns(workspaceResources);
			resourceLoader.Setup(x => x.GetGlobalResources()).Returns(systemResources);
			inUseLoader.Setup(x => x.GetWorkspaceResourceIDsInUseByListID(this.ResourceList.ResourceListID)).Returns(new HashSet<int>() { 2 });

			bool result = sut.ResourcesHaveChanged(ws);

			Assert.IsFalse(result);
		}

		/// <summary>
		/// Test ResourcesHaveChanged returns true when a system resource has been deleted
		/// </summary>
		[TestMethod]
		public void TestResourcesHaveChangedDeletedResources()
		{
			RestoreDefaultOptions sut = CreateSUT();

			ICollection<ResourceDTO> systemResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = "Res1", ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = "Res2", ResourceDesc = "System Resource 2", SegRegion = "Region 2", LaborType = "Labor" }
			};

			ICollection<ResourceDTO> workspaceResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = "Res1", ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = "Res2", ResourceDesc = "System Resource 2", SegRegion = "Region 2", LaborType = "Labor" },
				new ResourceDTO() { Id = 3, ResourceName = "Res3", ResourceDesc = "System Resource 3", SegRegion = "Region 3", LaborType = "Labor" }
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = this.Workspace.Id, ResourceListID = this.ResourceList.ResourceListID };
			FullWorkspace ws = new FullWorkspace(workspace);

			retriever.Setup(x => x.GetResourcesByResourceListId(this.ResourceList.ResourceListID)).Returns(workspaceResources);
			resourceLoader.Setup(x => x.GetGlobalResources()).Returns(systemResources);
			inUseLoader.Setup(x => x.GetWorkspaceResourceIDsInUseByListID(this.ResourceList.ResourceListID)).Returns(new HashSet<int>());

			bool result = sut.ResourcesHaveChanged(ws);

			Assert.IsTrue(result);
		}

		/// <summary>
		/// Test ResourcesHaveChanged returns false when a system resource has been deleted but it's in use
		/// </summary>
		[TestMethod]
		public void TestResourcesHaveChangedDeletedResourcesInUse()
		{
			RestoreDefaultOptions sut = CreateSUT();

			ICollection<ResourceDTO> systemResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = "Res1", ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = "Res2", ResourceDesc = "System Resource 2", SegRegion = "Region 2", LaborType = "Labor" }
			};

			ICollection<ResourceDTO> workspaceResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = "Res1", ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = "Res2", ResourceDesc = "System Resource 2", SegRegion = "Region 2", LaborType = "Labor" },
				new ResourceDTO() { Id = 3, ResourceName = "Res3", ResourceDesc = "System Resource 3", SegRegion = "Region 3", LaborType = "Labor" }
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = this.Workspace.Id, ResourceListID = this.ResourceList.ResourceListID };
			FullWorkspace ws = new FullWorkspace(workspace);

			retriever.Setup(x => x.GetResourcesByResourceListId(this.ResourceList.ResourceListID)).Returns(workspaceResources);
			resourceLoader.Setup(x => x.GetGlobalResources()).Returns(systemResources);
			inUseLoader.Setup(x => x.GetWorkspaceResourceIDsInUseByListID(this.ResourceList.ResourceListID)).Returns(new HashSet<int>() { 3 });

			bool result = sut.ResourcesHaveChanged(ws);

			Assert.IsFalse(result);
		}

		/// <summary>
		/// Test ResourcesHaveChanged returns false when there are no changes
		/// </summary>
		[TestMethod]
		public void TestResourcesHaveChangedNoChanges()
		{
			RestoreDefaultOptions sut = CreateSUT();

			ICollection<ResourceDTO> systemResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = "Res1", ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = "Res2", ResourceDesc = "System Resource 2", SegRegion = "Region 2", LaborType = "Labor" },
				new ResourceDTO() { Id = 3, ResourceName = "Res3", ResourceDesc = "System Resource 3", SegRegion = "Region 3", LaborType = "Labor" }
			};

			ICollection<ResourceDTO> workspaceResources = new Collection<ResourceDTO>()
			{
				new ResourceDTO() { Id = 1, ResourceName = "Res1", ResourceDesc = "System Resource 1", SegRegion = "Region 1", LaborType = "Labor" },
				new ResourceDTO() { Id = 2, ResourceName = "Res2", ResourceDesc = "System Resource 2", SegRegion = "Region 2", LaborType = "Labor" },
				new ResourceDTO() { Id = 3, ResourceName = "Res3", ResourceDesc = "System Resource 3", SegRegion = "Region 3", LaborType = "Labor" }
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = this.Workspace.Id, ResourceListID = this.ResourceList.ResourceListID };
			FullWorkspace ws = new FullWorkspace(workspace);

			retriever.Setup(x => x.GetResourcesByResourceListId(this.ResourceList.ResourceListID)).Returns(workspaceResources);
			resourceLoader.Setup(x => x.GetGlobalResources()).Returns(systemResources);
			inUseLoader.Setup(x => x.GetWorkspaceResourceIDsInUseByListID(this.ResourceList.ResourceListID)).Returns(new HashSet<int>());

			bool result = sut.ResourcesHaveChanged(ws);

			Assert.IsFalse(result);
		}
	}
}
