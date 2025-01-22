// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.Other
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenBOE.Tests.ActionLogic;
	using GenBOE.Tests.DAL.DataLoaders;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using IES.Common.PickList;
	using Microsoft.Practices.Unity;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	[TestClass]
	public class WorkspaceControllerLogicTest
	{
		private Mock<IResourceDTODataLoader> resourceLoader;
		private IFullObjectFactory fullObjectFactory;
		private Mock<ICommonDataMapper> commonDataMapper;
		private Mock<IPermissionsDTODataLoader> permissionsLoader;
		private Mock<IUserDTODataLoader> userLoader;
		private Retriever retriever;
		private Mock<IPerformingOrgDTODataLoader> perfLoader;
		private Mock<IPerformingOrgListDTODataLoader> perfListLoader;
		private BoeTaskElementDTODataLoader boeTaskElementDTODataLoader;
		private Mock<IOffloadRatesDTOLoader> offloadRatesLoader;
		private Mock<IRteTemplateDataLoader> rteTemplateDataLoader;
		private readonly Mock<IActiveDirectoryUtilities> _ADUtils = new Mock<IActiveDirectoryUtilities>();

		/// <summary>
		/// Initializes the test data.
		/// </summary>
		[TestInitialize]
		public void Init()
		{
			boeTaskElementDTODataLoader = new BoeTaskElementDTODataLoader(new ResourceTypeLoader(), new ResourceSpreadLoader(),
				new OrdinaryVariableLoader(), new BoeTaskElementCustomFieldValueXREFLoader(), new LaborTypeCustomFieldValueXREFLoader(), new SkillMixDTOLoader(), new CommonDisclosureSMDTODataLoader());
			resourceLoader = new Mock<IResourceDTODataLoader>();
			perfLoader = new Mock<IPerformingOrgDTODataLoader>();
			perfListLoader = new Mock<IPerformingOrgListDTODataLoader>();
			offloadRatesLoader = new Mock<IOffloadRatesDTOLoader>();
			fullObjectFactory = new FullObjectFactory(new WorkspaceDTODataLoader(), new BoeDTODataLoader(), null, null, null, null, null, null, null, null, null, null);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), fullObjectFactory);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IActiveDirectoryUtilities), _ADUtils.Object);

			userLoader = new Mock<IUserDTODataLoader>();

			UserDTO userDto = new UserDTO();
			userDto.UserID = 1;

			userLoader.Setup(x => x.GetUserForActiveUser()).Returns(userDto);
			commonDataMapper = new Mock<ICommonDataMapper>();
			permissionsLoader = new Mock<IPermissionsDTODataLoader>();

			PerformingOrgDTO perfOrg = new PerformingOrgDTO()
			{
				IsSystemPerfOrg = false,
				PerformingOrgDesc = "P",
				PerformingOrgName = "P",
				Id = 15
			};
			perfLoader.Setup(x => x.GetByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>() { perfOrg });

			ResourceDTO resource1 = new ResourceDTO()
			{
				ResourceDesc = "R",
				ResourceName = "R",
				Id = 20,
				ElementOfCost = ElementOfCostType.LMLabor,
				RateType = RateType.Hours
			};


			resourceLoader.Setup(x => x.GetByListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO>() { resource1 });

			retriever = new Retriever(new ClinDTODataLoader(), new WbsDTODataLoader(), new WorkspaceDTODataLoader(), new BoeDTODataLoader(), null, null, null, null,
				new ResourceTypeLoader(), boeTaskElementDTODataLoader, null, resourceLoader.Object, null, null, null, null, null, null,
				perfLoader.Object, perfListLoader.Object, null, null, null, null, userLoader.Object, null, null, null, null, new ProjectMapDataLoader(new ProjectMapSpreadLoader()), null, null);

			permissionsLoader.Setup(x => x.GetBOEPermissions(It.IsAny<ICollection<int>>())).Returns(new Collection<PermissionsDTO>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceTypeLoader), new ResourceTypeLoader());
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IOffloadRatesDTOLoader), offloadRatesLoader.Object);

			rteTemplateDataLoader = new Mock<IRteTemplateDataLoader>();

			GlobalTestCaseSetup.ResetGlobalWorkspaceID();
		}

		/// <summary>
		/// Creates the system.
		/// </summary>
		/// <returns>A Workspace controller logic.</returns>
		public IWorkspaceControllerLogic CreateSystem()
		{
			BoeTaskElementMediator mediator = new BoeTaskElementMediator(this.boeTaskElementDTODataLoader);
			return new WorkspaceControllerLogicMST(new WorkspaceDTODataLoader(), null, new ResourceDTODataLoader(),
				null, null, null, this.fullObjectFactory,
				this.commonDataMapper.Object, this.permissionsLoader.Object, null, null, null, null, null, null, null,
				null, null, new ProjectMapDataLoader(new ProjectMapSpreadLoader()), new BoePickListMapper(new LineOfBusinessDataLoader(), new ProposalClassLoader(),
				new ContractTypeLoader()), new GenTRAC.DataBridge.DTO.PtmPickListMapper(new GenTRAC.DataBridge.DTO.ProposalTypeLULoader(), new GenTRAC.DataBridge.DTO.ProposalClassLULoader(),
				new GenTRAC.DataBridge.DTO.TypeOfRequestLULoader(), new GenTRAC.DataBridge.DTO.LineOfBusinessDataLoader(), new GenTRAC.DataBridge.DTO.ProgramAreaDataLoader(),
				new GenTRAC.DataBridge.DTO.ContractTypeLULoader(), new GenTRAC.DataBridge.DTO.ContractTypeGroupLULoader()), null, null, null, null, null, null);
		}


		/// <summary>
		/// Tests the project map save.
		/// </summary>
		[TestMethod]
		public void Test_ProjectMapSave()
		{
			FullWorkspace ws = this.fullObjectFactory.CreateFullWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);
			ws.ProjectMapType = ProjectMapType.NonTimePhasedProjectMap;
			TestSaving(ws);
		}

		/// <summary>
		/// Tests the project map save for time phased.
		/// </summary>
		[TestMethod]
		public void Test_ProjectMapSave_TimePhased()
		{
			FullWorkspace ws = this.fullObjectFactory.CreateFullWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);
			ws.ProjectMapType = ProjectMapType.TimePhasedProjectMap;
			TestSaving(ws);
		}

		/// <summary>
		/// Tests saving a workspace based on project map
		/// </summary>
		/// <param name="ws">The workspace to save.</param>
		private void TestSaving(FullWorkspace ws)
		{
			ProjectMapModelView[] data = ProjectMapConverterTest.GetProjectMapModelViews();
			var sut = this.CreateSystem();
			sut.SaveProjectMapData(data, ws);

			// shove the data pulled from the database into a ConvertedProjectMapDTO to re-use assert equality method
			ConvertedProjectMapDTO savedData = new ConvertedProjectMapDTO();

			// create a new workspace that doesn't have the properties cached
			ws = this.fullObjectFactory.CreateFullWorkspace(ws.Id);

			foreach (FullBoe boe in ws.Boes)
			{
				savedData.Boes.Add(boe);
			}

			foreach (FullClin clin in ws.Clins)
			{
				savedData.Clins.Add(clin);
			}

			foreach (FullWbs Wbs in ws.WbsElements)
			{
				savedData.Wbs.Add(Wbs);
			}

			foreach (BoeTaskElementDTO task in ws.TaskElements)
			{
				savedData.Tasks.Add(task);
			}

			ProjectMapConverterTest.AssertEquality(data, savedData);
		}

		[TestMethod]
		public void TestProjectMapAdjustPrecision()
		{
			ICollection<ProjectMapModelView> models = new List<ProjectMapModelView> {
				new ProjectMapModelView
				{
					Updateable = UpdateType.Upsert,
					EndDate = DateTime.Parse("2/2/2019"),
					Hours = 10.67m,
					StartDate = DateTime.Parse("2/1/2019"),
					WbsNumber = "123",
					ActivityID = "1",
					ActivityName = "123",
					WbsElementTitle = "123",
					InitialResource = "123",
					CostCenter = "123",
					Clin = "123",
					ClassOfCost = ClassOfCost.NonRecurring.ToDescription(),
					TieredPercentage = 12.3m,
					DiscreteMonths = new decimal?[] { 10.67m }
				},
				new ProjectMapModelView
				{
					Updateable = UpdateType.Upsert,
					EndDate = DateTime.Parse("2/2/2019"),
					Dollars = 12.01m,
					StartDate = DateTime.Parse("2/1/2019"),
					WbsNumber = "123",
					ActivityID = "2",
					ActivityName = "123",
					WbsElementTitle = "123",
					InitialResource = "123",
					CostCenter = "123",
					Clin = "123",
					ClassOfCost = ClassOfCost.NonRecurring.ToDescription(),
					TieredPercentage = 21.0m,
					DiscreteMonths = new decimal?[] { 12.01m }
				},
				new ProjectMapModelView
				{
					Updateable = UpdateType.Upsert,
					EndDate = DateTime.Parse("2/2/2019"),
					Hours = 10.0m,
					StartDate = DateTime.Parse("2/1/2019"),
					WbsNumber = "123",
					ActivityID = "3",
					ActivityName = "123",
					WbsElementTitle = "123",
					InitialResource = "123",
					CostCenter = "123",
					Clin = "123",
					ClassOfCost = ClassOfCost.NonRecurring.ToDescription(),
					DiscreteMonths = new decimal?[] { 2.2m, 2.4m, 5.4m }
				}
			};

			// save the workspace
			FullWorkspace ws = this.fullObjectFactory.CreateFullWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);
			ws.ProjectMapType = ProjectMapType.TimePhasedProjectMap;
			ws.CostDecimalPrecision = 2;
			ws.ResourceDecimalPrecision = 2;
			WorkspaceDTODataLoader loader = new WorkspaceDTODataLoader();
			int id = GlobalTestCaseSetup.GetEtiUserID();
			loader.SaveIdentificationAndExportFormat(id, ws);

			// reload the workspace
			ws = this.fullObjectFactory.CreateFullWorkspace(ws.Shortname, true);

			var sut = this.CreateSystem();
			sut.SaveProjectMapData(models, ws);

			// reload the workspace
			ws = this.fullObjectFactory.CreateFullWorkspace(ws.Shortname, true);

			// Adjust the precision in Workspace DTO
			ws.CostDecimalPrecision = 0;
			ws.ResourceDecimalPrecision = 0;
			sut.ProjectMapAdjustPrecision(ws, true, true);

			// reload the workspace
			ws = this.fullObjectFactory.CreateFullWorkspace(ws.Shortname, true);
			FullProjectMapWorkspace projectWorkspace = ws as FullProjectMapWorkspace;

			Assert.AreEqual(11m, projectWorkspace.ProjectMapData.First(m => m.ActivityID == "1").Hours);
			Assert.AreEqual(12m, projectWorkspace.ProjectMapData.First(m => m.ActivityID == "2").Dollars);
			Assert.AreEqual(9m, projectWorkspace.ProjectMapData.First(m => m.ActivityID == "3").Hours);
			Assert.AreEqual(12.3m, projectWorkspace.ProjectMapData.First(m => m.ActivityID == "1").TieredPercentage);
			Assert.AreEqual(21.0m, projectWorkspace.ProjectMapData.First(m => m.ActivityID == "2").TieredPercentage);
			Assert.AreEqual(2m, projectWorkspace.ProjectMapData.First(m => m.ActivityID == "3").DiscreteMonths[0].Value);
			Assert.AreEqual(2m, projectWorkspace.ProjectMapData.First(m => m.ActivityID == "3").DiscreteMonths[1].Value);
			Assert.AreEqual(5m, projectWorkspace.ProjectMapData.First(m => m.ActivityID == "3").DiscreteMonths[2].Value);
		}

		#region PTM Conversion Tests

		/// <summary>
		/// Tests the contract type conversion.
		/// </summary>
		[TestMethod]
		public void TestContractTypeConversion()
		{
			var sut = this.CreateSystem();

			GenTRAC.DataBridge.DTO.PtmPickListMapper mapper = new GenTRAC.DataBridge.DTO.PtmPickListMapper(null, null, null, null, null, new GenTRAC.DataBridge.DTO.ContractTypeLULoader(), new GenTRAC.DataBridge.DTO.ContractTypeGroupLULoader());

			foreach (PickListDto contractType in mapper.GetPickListValues(PickListEnum.ContractType).PickLists)
			{
				if (contractType.Text != "Other" && contractType.Text != "Time and Material Level of Effort")
				{
					int boeContractType = sut.ConvertPTMContractTypeId(contractType.Id);
					Assert.IsTrue(boeContractType > 0);
				}
			}
		}

		/// <summary>
		/// Test ConvertPTMLineOfBusiness
		/// </summary>
		[TestMethod]
		public void TestConvertPTMLineOfBusiness()
		{
			IWorkspaceControllerLogic sut = this.CreateSystem();

			int result = sut.ConvertPTMLineOfBusiness(18);

			Assert.IsTrue(result > 0);
		}

		/// <summary>
		/// Test ConvertPTMLineOfBusiness for LoB DTO being null
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(GenValidationException))]
		public void TestConvertPTMLineOfBusiness_EX1()
		{
			IWorkspaceControllerLogic sut = this.CreateSystem();

			int result = sut.ConvertPTMLineOfBusiness(111111);
		}

		/// <summary>
		/// Test ConvertPTMContractTypeId
		/// </summary>
		[TestMethod]
		public void TestConvertPTMContractTypeId()
		{
			IWorkspaceControllerLogic sut = this.CreateSystem();

			int result = sut.ConvertPTMContractTypeId(1); // PtmContractType.CostPlusAwardFee

			Assert.AreEqual(1001, result);
		}

		/// <summary>
		/// Test ConvertPTMContractTypeId for no match
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(GenValidationException))]
		public void TestConvertPTMContractTypeId_EX1()
		{
			IWorkspaceControllerLogic sut = this.CreateSystem();

			int result = sut.ConvertPTMContractTypeId(99999);
		}

		/// <summary>
		/// Test ConvertPTMProposalClassId
		/// </summary>
		[TestMethod]
		public void TestConvertPTMProposalClassId()
		{
			IWorkspaceControllerLogic sut = this.CreateSystem();

			PickListDto dto = new PickListDto() { Id = 1, Text = "Firm" };

			int result = sut.ConvertPTMProposalClassId(1);

			Assert.AreEqual(1001, result);
		}

		/// <summary>
		/// Test ConvertPTMProposalClassId for no match
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(GenValidationException))]
		public void TestConvertPTMProposalClassId_EX1()
		{
			IWorkspaceControllerLogic sut = this.CreateSystem();

			int result = sut.ConvertPTMProposalClassId(33333);
		}

		#endregion
	}
}
