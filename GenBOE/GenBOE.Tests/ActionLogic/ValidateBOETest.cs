// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.NewValidation;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.ActionLogic.WBS.BOE;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using Microsoft.Practices.Unity;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	/// <summary>
	/// ValidateBOE Tests
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestClass]
	public class ValidateBOETest : MOQObject
	{
		private Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
		private Mock<IRetriever> retriever = new Mock<IRetriever>();
		private bool _SpaceEnabled = SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems;
		private Mock<ICommonDataMapper> _CommonDataMapper = new Mock<ICommonDataMapper>();
		private Mock<IPermissionsDTODataLoader> _permissions = new Mock<IPermissionsDTODataLoader>();
		private Mock<IMSTZoneTravelValidator> mstZoneTravelValidator = new Mock<IMSTZoneTravelValidator>();
		private Mock<IActiveDirectoryUtilities> activeDirectoryUtilities = new Mock<IActiveDirectoryUtilities>();
		private Mock<IOffloadRatesDTOLoader> offloadRatesLoader = new Mock<IOffloadRatesDTOLoader>();
		private Mock<IRteTemplateDataLoader> rteTemplateLoader = new Mock<IRteTemplateDataLoader>();

		[TestInitialize]
		public override void Setup()
		{
			base.Setup();
			retriever.Setup(x => x.GetTMResourceRates(It.IsAny<int>())).Returns(new List<TMResourceRateDTO>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IActiveDirectoryUtilities), activeDirectoryUtilities.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IMSTZoneTravelValidator), mstZoneTravelValidator.Object);

			this.offloadRatesLoader = new Mock<IOffloadRatesDTOLoader>();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IOffloadRatesDTOLoader), offloadRatesLoader.Object);

			this.offloadRatesLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(new Collection<OffloadRatesDTO>()
			{
				new OffloadRatesDTO()
				{
					HourlyRate = 35.00m,
					Percent = 0.5m,
					PerformingOrg = "P",
					SubResource = "S",
					Resource = "R",
					Year = 2017
				},
				new OffloadRatesDTO()
				{
					HourlyRate = 25.00m,
					Percent = 0.15m,
					PerformingOrg = "P2",
					SubResource = "S",
					Resource = "R",
					Year = 2017
				},new OffloadRatesDTO()
				{
					HourlyRate = 135.00m,
					Percent = 0.4m,
					PerformingOrg = "P",
					SubResource = "S",
					Resource = "R2",
					Year = 2017
				},new OffloadRatesDTO()
				{
					HourlyRate = 20.00m,
					Percent = 0.5m,
					PerformingOrg = "P",
					SubResource = "S",
					Resource = "R",
					Year = 2018
				}
			});

			this.rteTemplateLoader.Setup(x => x.GetByBoeId(It.IsAny<int>(), It.IsAny<int>())).Returns(new List<RTECustomTemplateQuestionAnswerModelView>());
			this.rteTemplateLoader.Setup(x => x.GetByBoeIdAndTaskId(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<RTECustomTemplateQuestionAnswerModelView>());
		}

		/// <summary>
		/// Create ValidateBOE sut
		/// </summary>
		/// <returns>sut</returns>
		private ValidateBOE CreateSystem()
		{
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			Mock<IPermissionsDTODataLoader> permloader = new Mock<IPermissionsDTODataLoader>();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permloader.Object);

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			return sut;
		}

		/// <summary>
		/// This is a basic test using the global boe to validate
		/// </summary>
		[TestMethod]
		public void BL_ValidateBOE()
		{
			// Set up
			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			Mock<IResourceDTODataLoader> ResourceLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();
			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName };

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, WasDescriptionSet = true, WasDataSourceSet = true };
			FullBoe boeObject = new FullBoe(boe);
			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO>());
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> { boeObject });
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);

			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			ResourceLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>());

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			//Assert
			Assert.IsNotNull(validationBOE, "The BOE to validate was null");

			//Act
			boeObject.Description = null;
			boeObject.DataSource = null;
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			//Assert
			Assert.IsNotNull(validationBOE, "The BOE to validate was null");
		}

		/// <summary>
		/// This test case will ensure that a BOE is created with lots of missing fields
		/// so we can be sure that each required message needed is populated
		/// </summary>
		[TestMethod]
		public void BL_MoreInvalidBOEsToValidate()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, UsingTemplateBOE = false };

			//setup custom fields
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 20 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor,
				WasDescriptionSet = true,
				WasMoqTextSet = true
			};
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("12/01/2011"), WorkspaceID = workspace.Id, HistoricMetricDisclosureChecked = false, WasDescriptionSet = true, WasDataSourceSet = true };
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { boeTE });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO>());
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> { boeObject });
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);

			FullWorkspace workspaceObject = new FullWorkspace(workspace);

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));

			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>());

			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { });


			ICollection<int> ids = new Collection<int>();
			ids.Add(boeTE.Id);

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });

			IValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;

			// check that the boe description was missing

			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Contains(BoeDTO.BOE_DESC_REQUIRED), "The BOE Header was  empty");

			// verify task error messages
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				Assert.IsTrue(x.TaskElementDetails.TaskElementDetailValidationMessages.Contains(BoeDTO.MOQ_EQ_REQUIRED));
				Assert.IsTrue(x.TaskElementDetails.TaskElementDetailValidationMessages.Contains(String.Format(BoeDTO.MOQ_TEXT_REQUIRED, CommonConstants.BOE_MOQ_TEXT_LABEL)));
				Assert.IsTrue(x.TaskElementDetails.TaskElementDetailValidationMessages.Contains(BoeDTO.TOTAL_LABOR_SPREAD_INVALID));

				// verify labor error messages 
				foreach (ValidationBOELaborType y in x.LaborTypes)
				{
					Assert.IsTrue(y.LaborTypeValidationMsgs.Count == 2, "There weren't 2 labor type messages");
				}
			}

			//now test it in Space Systems mode
			sut = new ValidateBOESpaceSystems(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;

			// verify task error messages - in space sytems we only need to verify the MOQ Text difference
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				Assert.IsTrue(x.TaskElementDetails.TaskElementDetailValidationMessages.Contains(String.Format(BoeDTO.MOQ_TEXT_REQUIRED, CommonConstants.BOE_MOQ_TEXT_LABEL_SPACE_SYSTEMS)));
			}
		}

		[TestMethod]
		public void BL_ValidateBOE_GoodMOQ_Eq()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();
			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, UsingTemplateBOE = false };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, DataSource = "validate data" };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			FullBoe boeObject = new FullBoe(boe);

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);

			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());
			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;
			Collection<ValidationBOELaborType> toAssertLabors = new Collection<ValidationBOELaborType>();

			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Count == 0, string.Empty, "The BOE Header was not empty");
			Assert.IsTrue(toAssertTasks.Count == 0, "Task messages were not empty");
			Assert.IsTrue(toAssertLabors.Count == 0, "Task labor messages were not empty");
		}

		// This test case will test that if a BOE and CLIN don't have a start/end date, the contract start/end date are used
		public void BL_ValidateLaborTypeContractDates()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();
			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ContractStartDate = Convert.ToDateTime("03/01/2010"), ContractEndDate = Convert.ToDateTime("03/01/2014"), ResourceListID = 2 };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });

			//setup clin
			ClinDTO clin = new ClinDTO { Id = 8, WorkspaceID = 1, StartDate = null, EndDate = null };

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);

			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				//ResourceName = resource.ResourceName,
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", WorkspaceID = 1, CLINID = 8, DataSource = "validate data", StartDate = Convert.ToDateTime("03/01/2010"), EndDate = Convert.ToDateTime("03/01/2014") };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullBoe boeObject = new FullBoe(boe);
			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });


			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetClinById(boe.CLINID.Value)).Returns(clin);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullClin(clin.Id)).Returns(new FullClin(clin));
			factory.Setup(x => x.CreateFullClin(clin)).Returns(new FullClin(clin));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;
			Collection<ValidationBOELaborType> toAssertLabors = new Collection<ValidationBOELaborType>();

			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Count == 0, string.Empty, "The BOE Header was not empty");
			Assert.IsTrue(toAssertTasks.Count == 0, "Task messages were not empty");
			Assert.IsTrue(toAssertLabors.Count == 0, "Task labor messages were not empty");

		}

		// This test case will test that if a BOE and CLIN don't have a start/end date, the contract start/end date are used
		[TestMethod]
		public void BL_ValidateLaborTypeBadContractDates()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();
			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ContractEndDate = Convert.ToDateTime("01/01/2011"), ContractStartDate = Convert.ToDateTime("04/01/2011"), ResourceListID = 1 };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup clin
			ClinDTO clin = new ClinDTO { Id = 8, WorkspaceID = 1, StartDate = null, EndDate = null };

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", WorkspaceID = 1, CLINID = 8, DataSource = "validate data", StartDate = Convert.ToDateTime("01/01/2011"), EndDate = Convert.ToDateTime("04/01/2011") };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });

			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetClinById(boe.CLINID.Value)).Returns(clin);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullClin(clin.Id)).Returns(new FullClin(clin));
			factory.Setup(x => x.CreateFullClin(clin)).Returns(new FullClin(clin));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title",
				ODCTypes = new Collection<OtherDirectCostType> {
					new OtherDirectCostType { ODCTypeID = 1,
												BoeID = boe.Id,
												PerformingOrgID = this.Perforg.Id,
												ResourceID = this.Resource.Id,
												StartDate = Convert.ToDateTime("02/01/2011"),
												EndDate = Convert.ToDateTime("06/01/2011")
					} } } });

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());


			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			Collection<ValidationBOETasks> toAssertCosts = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;
			toAssertCosts = validationBOE.Costs;

			// want to extract the validation contract message
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				// verify labor error messages 
				foreach (ValidationBOELaborType y in x.LaborTypes)
				{
					Assert.IsTrue(y.LaborTypeValidationMsgs.Count == 2, "Labor type messages did not return 2 msgs");
					Assert.IsTrue(y.LaborTypeValidationMsgs[0].Contains("Start Date must be on or after"), "The validation msg was correct");
					Assert.IsTrue(y.LaborTypeValidationMsgs[1].Contains("End Date must be on or before"), "The validation msg was correct");

				}
			}

			foreach (ValidationBOETasks x in toAssertCosts)
			{
				// very ODC error message
				foreach (ValidationBOELaborType y in x.LaborTypes)
				{
					Assert.IsTrue(y.LaborTypeValidationMsgs.Count == 3, "ODC type messages did not return 3 messages");
					Assert.IsTrue(y.LaborTypeValidationMsgs[0].Contains("Start Date must be on or after"), "The validation msg was correct");
					Assert.IsTrue(y.LaborTypeValidationMsgs[1].Contains("End Date must be on or before"), "The validation msg was correct");
				}
			}
		}

		[TestMethod]
		public void BL_ValidateBadBOEContractDates()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();
			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ContractEndDate = Convert.ToDateTime("01/01/2011"), ContractStartDate = Convert.ToDateTime("04/01/2011"), ResourceListID = 1 };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });

			//setup clin
			ClinDTO clin = new ClinDTO { Id = 8, WorkspaceID = 1, StartDate = null, EndDate = null };

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};
			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", WorkspaceID = 1, CLINID = 8, DataSource = "validate data", StartDate = Convert.ToDateTime("01/01/2011"), EndDate = Convert.ToDateTime("04/01/2011") };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });

			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetClinById(boe.CLINID.Value)).Returns(clin);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullClin(clin.Id)).Returns(new FullClin(clin));
			factory.Setup(x => x.CreateFullClin(clin)).Returns(new FullClin(clin));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title",
				ODCTypes = new Collection<OtherDirectCostType> {
					new OtherDirectCostType { ODCTypeID = 1,
												BoeID = boe.Id,
												PerformingOrgID = this.Perforg.Id,
												ResourceID = this.Resource.Id,
												StartDate = Convert.ToDateTime("02/01/2011"),
												EndDate = Convert.ToDateTime("06/01/2011")
					} } } });

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Count == 2, "BOE Header messages did not return 2 msgs");
			Assert.IsTrue(validationBOE.BOEHeaderMsgs[0].Contains("Start Date must be on or after the Contract start date (04/2011)"), "The validation msg was correct");
			Assert.IsTrue(validationBOE.BOEHeaderMsgs[1].Contains("End Date must be on or before the Contract end date (01/2011)"), "The validation msg was correct");
		}

		// This test case will test that if a BOE doesn't have a start/end date, the clin start/end date are used
		[TestMethod]
		public void BL_ValidateLaborTypeClinDates()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();

			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ContractStartDate = Convert.ToDateTime("03/01/2010"), ContractEndDate = Convert.ToDateTime("03/01/2014"), ResourceListID = 3, UsingTemplateBOE = false };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				//ResourceName = resource.ResourceName,
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};
			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", WorkspaceID = 1, CLINID = 8, DataSource = "validate data", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("06/01/2011") };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			#region Setup Unity references

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			#endregion
			FullWorkspace workspaceObject = new FullWorkspace(workspace);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;
			Collection<ValidationBOELaborType> toAssertLabors = new Collection<ValidationBOELaborType>();

			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Count == 0, "The BOE Header was not empty");
			Assert.IsTrue(toAssertTasks.Count == 0, "Task messages were not empty");
			Assert.IsTrue(toAssertLabors.Count == 0, "Task labor messages were not empty");

		}

		// This test case will test that if a BOE doesn't have a start/end date, the clin start/end date are used
		[TestMethod]
		public void BL_ValidateLaborTypeBadClinDates()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 2 };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup clin
			ClinDTO clin = new ClinDTO { Id = 8, WorkspaceID = 1, StartDate = Convert.ToDateTime("04/01/2011"), EndDate = Convert.ToDateTime("02/01/2011") };

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				//ResourceName = resource.ResourceName,
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};
			BoeDTO boe = new BoeDTO
			{
				Id = 4,
				Title = "My BOE Title",
				Description = "Validate BOE",
				WorkspaceID = 1,
				CLINID = 8,
				DataSource = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011")
			};

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullBoe boeObject = new FullBoe(boe);


			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetClinById(boe.CLINID.Value)).Returns(clin);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullClin(clin.Id)).Returns(new FullClin(clin));
			factory.Setup(x => x.CreateFullClin(clin)).Returns(new FullClin(clin));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());


			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			FullWorkspace workspaceObject = new FullWorkspace(workspace);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;

			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Count == 2, "The BOE Header was  empty");
			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Contains(String.Format(BoeDTO.BOE_START_DATE_INVALID, "CLIN", clin.StartDate.Value.ToString("MM/yyyy"))), "BOE start date was valid");
			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Contains(String.Format(BoeDTO.BOE_END_DATE_INVALID, "CLIN", clin.EndDate.Value.ToString("MM/yyyy"))), "BOE end date was valid");

			// want to extract the validation contract message
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				// verify labor error messages 
				foreach (ValidationBOELaborType y in x.LaborTypes)
				{
					Assert.IsTrue(y.LaborTypeValidationMsgs.Count == 2, "There weren't 2 labor type messages");
					Assert.IsTrue(y.LaborTypeValidationMsgs[1].Contains("End Date must be on or before"), "The validation msg was correct");
					Assert.IsTrue(y.LaborTypeValidationMsgs[0].Contains("Start Date must be on or after"), "The validation msg was correct");

				}
			}
		}

		[TestMethod]
		// This test case will test the BOE Start/End Date against the Labor Type Start/End Date
		public void BL_ValidateLaborTypeBOEDates()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ContractStartDate = Convert.ToDateTime("03/01/2010"), ContractEndDate = Convert.ToDateTime("03/01/2014"), ResourceListID = 1, UsingTemplateBOE = false };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				//ResourceName = resource.ResourceName,
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};

			// the BOE End date is past the CLIN end date so this should fail validation
			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", WorkspaceID = 1, CLINID = 8, StartDate = Convert.ToDateTime("01/10/2011"), EndDate = Convert.ToDateTime("02/03/2012"), DataSource = "validate data" };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			FullWorkspace workspaceObject = new FullWorkspace(workspace);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;
			Collection<ValidationBOELaborType> toAssertLabors = new Collection<ValidationBOELaborType>();

			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Count == 0, "The BOE Header was empty");
			Assert.IsTrue(toAssertTasks.Count == 0, "Task messages were not empty");
			Assert.IsTrue(toAssertLabors.Count == 0, "Task labor messages were not empty");
		}

		// This test case will test that if a BOE doesn't have a start/end date, the clin start/end date are used
		[TestMethod]
		// This test case will test the BOE Start/End Date against the Labor Type Start/End Date
		public void BL_ValidateLaborTypeBadBOEDates()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 3 };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", WorkspaceID = 1, CLINID = 8, StartDate = Convert.ToDateTime("05/01/2011"), EndDate = Convert.ToDateTime("01/01/2011"), DataSource = "validate data" };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);

			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());
			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			#region Setup Unity references

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			#endregion
			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;

			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Count == 0, "The BOE Header was not empty");

			// want to extract the validation contract message
			foreach (ValidationBOETasks x in toAssertTasks)
			{

				// verify labor error messages 
				foreach (ValidationBOELaborType y in x.LaborTypes)
				{
					Assert.IsTrue(y.LaborTypeValidationMsgs.Count == 2, "There weren't 2 labor type messages");
					Assert.IsTrue(y.LaborTypeValidationMsgs[1].Contains("End Date must be on or before"), "The validation msg was correct");
					Assert.IsTrue(y.LaborTypeValidationMsgs[0].Contains("Start Date must be on or after"), "The validation msg was correct");
				}
			}
		}

		[TestMethod]
		public void BL_ValidateTaskCustomFields()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 1, UsingTemplateBOE = false };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				//ResourceName = resource.ResourceName,
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor,
				CustomFieldValueContainers = new Collection<CustomFieldValueContainer>() { new CustomFieldValueContainer { CustomFieldID = 2, IsOpenEnded = true, OpenEndedValue = "Test Value" } }
			};

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, DataSource = "validate data" };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.TaskDisplay, CustomFieldRequired = true, IsOpenEnded = false };
			CustomFieldValueDTO customValue = new CustomFieldValueDTO { CustomFieldID = 1, CustomFieldValueID = 1, CustomFieldValueInUseFlag = true, Id = 1 };

			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByTaskElementIds(It.IsAny<Collection<int>>())).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>() { });
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });

			_TripDTODataLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<TripDTO>());
			_MiscTravelRateDTOLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<MiscTravelRateDTO>());
			_LocationDTODataLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<LocationDTO>());

			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>() { customValue });

			Dictionary<int, ICollection<KeyValuePair<int, int>>> toReturn = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
			toReturn[boeTE.Id] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>(customField.Id, customValue.Id) };
			retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByTaskElementIds(It.IsAny<Collection<int>>())).Returns(toReturn);


			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;

			// want to extract the validation contract message
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				ValidationBOETaskElementDetails taskElementDetails = x.TaskElementDetails;

				Assert.IsTrue(taskElementDetails.TaskElementDetailValidationMessages.Count == 0, "Validation Msgs exist");
			}

			// Set up to test open ended custom field validation
			CustomFieldDTO customField_OpenEnded = new CustomFieldDTO { Id = 2, CustomFieldName = "BABBA", CustomFieldDisplayID = CustomFieldType.TaskDisplay, CustomFieldRequired = true, IsOpenEnded = true };
			customFields = new Collection<CustomFieldDTO>() { customField_OpenEnded };
			customValue.CustomFieldID = 2;
			toReturn[boeTE.Id] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>(customField_OpenEnded.Id, customValue.Id) };
			retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByTaskElementIds(It.IsAny<Collection<int>>())).Returns(toReturn);
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>() { customValue });
			workspaceObject = new FullWorkspace(workspace);

			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			toAssertTasks = validationBOE.Tasks;

			foreach (ValidationBOETasks x in toAssertTasks)
			{
				ValidationBOETaskElementDetails taskElementDetails = x.TaskElementDetails;

				Assert.IsTrue(taskElementDetails.TaskElementDetailValidationMessages.Count == 0, "Validation Msgs exist");
			}
		}

		[TestMethod]
		public void BL_ValidateTaskCustomFields_Invalid()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 4, UsingTemplateBOE = false };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				//ResourceName = resource.ResourceName,
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor,
				CustomFieldValueContainers = new Collection<CustomFieldValueContainer>() { new CustomFieldValueContainer { CustomFieldID = 2, IsOpenEnded = true, OpenEndedValue = string.Empty } }
			};

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, DataSource = "validate data" };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.TaskDisplay, CustomFieldRequired = true, CustomFieldName = "BABBA" };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });

			_TripDTODataLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<TripDTO>());
			_MiscTravelRateDTOLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<MiscTravelRateDTO>());
			_LocationDTODataLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<LocationDTO>());

			CustomFieldValueDTO customFieldValue = new CustomFieldValueDTO { Id = 1, CustomFieldID = 1, CustomFieldValueID = 1, CustomFieldValueDescription = "TestDesc", CustomFieldValueName = "TestName" };
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>() { customFieldValue });
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByTaskElementIds(It.IsAny<Collection<int>>())).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>() { });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);
			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;


			// want to extract the validation contract message
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				ValidationBOETaskElementDetails taskElementDetails = x.TaskElementDetails;

				Assert.IsTrue(taskElementDetails.TaskElementDetailValidationMessages.Count == 1, "Validation Msgs did not equal 1");
				Assert.IsTrue(taskElementDetails.TaskElementDetailValidationMessages.Contains("Custom Field BABBA is required."), "The validation msg was correct");
			}

			// Set up to test open ended custom field validation
			CustomFieldDTO customField_OpenEnded = new CustomFieldDTO { Id = 2, CustomFieldName = "BABBA", CustomFieldDisplayID = CustomFieldType.TaskDisplay, CustomFieldRequired = true };
			customFields = new Collection<CustomFieldDTO>() { customField_OpenEnded };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>() { });
			workspaceObject = new FullWorkspace(workspace);

			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			toAssertTasks = validationBOE.Tasks;

			// want to extract the validation contract message
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				ValidationBOETaskElementDetails taskElementDetails = x.TaskElementDetails;

				Assert.IsTrue(taskElementDetails.TaskElementDetailValidationMessages.Count == 1, "Validation Msgs did not equal 1");
				Assert.IsTrue(taskElementDetails.TaskElementDetailValidationMessages.Contains("Custom Field BABBA is required."), "The validation msg was correct");
			}
		}

		/// <summary>
		/// Validate MOQ Type Table Custom Fields - both valid and invalid
		/// </summary>
		[TestMethod]
		public void BL_ValidateMoqTypeTableCustomFields()
		{
			Utilities.IsSAPEnabledForSystem = false;

			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();
			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();
			Mock<IMoqTypeDataLoader> moqTypeLoader = new Mock<IMoqTypeDataLoader>();
			Mock<IMoqTypeTableCustomFieldValueXREFLoader> moqTypeTableCustomFieldXrefLoader = new Mock<IMoqTypeTableCustomFieldValueXREFLoader>();

			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 1, UsingTemplateBOE = true };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};

			MoqTableData moqTableData = new MoqTableData()
			{
				Id = 1,
				TableName = "test table",
				RepositoryName = "test repo",
				QueryType = "query type",
				DateOfReport = DateTime.Now.AddDays(-1),
				HistoricalProgramName = "test name",
				ContractNumber = "test contract",
				WbsElement = "test wbs",
				PoPStart = DateTime.Now.AddDays(-2),
				PoPEnd = DateTime.Now,
				TotalWbsHours = 100,
				AdditionalQueryFilters = "test filters",
				TotalRelevantHours = 50,
				CustomFieldValueContainers = new Collection<CustomFieldValueContainer>()
				{
					new CustomFieldValueContainer()
						{
							CustomFieldID = 1,
							IsOpenEnded = true,
							OpenEndedValue = "TEST"
						}
				}
			};

			MoqTypeSelection moqTypeSelection = new MoqTypeSelection()
			{
				Id = 1,
				TaskId = 1,
				SelectedMOQType = MOQType.Historical,
				CerName = "test name",
				DescriptionHoursRequired = "test desc",
				SmeReason = "test reason",
				SmeHoursLogic = "test hours logic",
				SmeDurationLogic = "test duration logic",
				SmeTaskEstimates = "test task estimates",
				Rationale = "test rationale",
				SkillMixRationale = "test skill mix",
				HistoricalReferenceExplanation = "test historical reference explanation",
				BoeId = 4,
				TableData = new Collection<MoqTableData>()
				{
					moqTableData
				}
			};

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, DataSource = "validate data" };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { moqTypeSelection });
			retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { moqTypeSelection });

			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldName = "TEST", CustomFieldDisplayID = CustomFieldType.MoqTypeTableDataDisplay, CustomFieldRequired = true, IsOpenEnded = true };
			CustomFieldValueDTO customValue = new CustomFieldValueDTO { CustomFieldID = 1, CustomFieldValueID = 1, CustomFieldValueInUseFlag = true, Id = 1 };

			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });
			_TripDTODataLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<TripDTO>());
			_MiscTravelRateDTOLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<MiscTravelRateDTO>());
			_LocationDTODataLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<LocationDTO>());

			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>() { customValue });

			Dictionary<int, ICollection<KeyValuePair<int, int>>> toReturn = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
			toReturn[boeTE.Id] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>(customField.Id, customValue.Id) };
			retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByMoqTypeTableIds(It.IsAny<Collection<int>>())).Returns(toReturn);
			retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByTaskElementIds(It.IsAny<Collection<int>>())).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>() { });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = validationBOE.Tasks;

			foreach (ValidationBOETasks x in toAssertTasks)
			{
				ValidationBOETaskElementDetails taskElementDetails = x.TaskElementDetails;

				Assert.IsTrue(taskElementDetails.TaskElementDetailValidationMessages.Count == 0, "Validation Msgs exist");
			}

			// Now test for invalid
			moqTypeSelection.TableData.First().CustomFieldValueContainers.First().OpenEndedValue = string.Empty;
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { moqTypeSelection });
			retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { moqTypeSelection });

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			toAssertTasks = validationBOE.Tasks;

			foreach (ValidationBOETasks x in toAssertTasks)
			{
				ValidationBOETaskElementDetails taskElementDetails = x.TaskElementDetails;

				Assert.IsTrue(taskElementDetails.TaskElementDetailValidationMessages.Count == 1, "Validation Msgs did not equal 1");
				Assert.IsTrue(taskElementDetails.TaskElementDetailValidationMessages.Contains("Custom Field TEST is required."), "The validation msg was correct");
			}
		}

		[TestMethod]
		public void BL_ValidateLaborTypeCustomFields()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 3, UsingTemplateBOE = false };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers = new Collection<CustomFieldValueContainer>() { new CustomFieldValueContainer { CustomFieldID = 2, IsOpenEnded = true, OpenEndedValue = "Test Value" } } };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				//ResourceName = resource.ResourceName,
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};
			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, DataSource = "validate data" };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullBoe boeObject = new FullBoe(boe);
			FullWorkspace workspaceObject = new FullWorkspace(workspace);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = true, IsOpenEnded = false };
			CustomFieldValueDTO customValue = new CustomFieldValueDTO { CustomFieldID = 1, CustomFieldValueID = 1, CustomFieldValueInUseFlag = true, Id = 1 };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>() { customValue });

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });
			Dictionary<int, ICollection<KeyValuePair<int, int>>> toReturn = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
			toReturn[boeLT.Id] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>(customField.Id, customValue.Id) };
			retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByLaborTypeIds(It.IsAny<Collection<int>>())).Returns(toReturn);

			_TripDTODataLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<TripDTO>());
			_MiscTravelRateDTOLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<MiscTravelRateDTO>());
			_LocationDTODataLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<LocationDTO>());

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			// only need to assert if task count = 0, because if there was a labor type validation msg,
			// it would automatically create a task so the labor type validation msg would be put underneath it
			Assert.IsTrue(validationBOE.Tasks.Count == 0, "There were task msgs");

			// Set up to test open ended custom field validation
			CustomFieldDTO customField_OpenEnded = new CustomFieldDTO { Id = 2, CustomFieldName = "BABBA", CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = true, IsOpenEnded = true };
			customFields = new Collection<CustomFieldDTO>() { customField_OpenEnded };
			customValue.CustomFieldID = 2;
			toReturn[boeLT.Id] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>(customField_OpenEnded.Id, customValue.Id) };
			retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByTaskElementIds(It.IsAny<Collection<int>>())).Returns(toReturn);
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>() { customValue });
			workspaceObject = new FullWorkspace(workspace);

			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			Assert.IsTrue(validationBOE.Tasks.Count == 0, "There were task msgs");
		}

		[TestMethod]
		public void BL_ValidateLaborTypeCustomFields_Invalid()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 3 };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours, CustomFieldValueContainers = new Collection<CustomFieldValueContainer>() { new CustomFieldValueContainer { CustomFieldID = 2, IsOpenEnded = true, OpenEndedValue = string.Empty } } };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				//ResourceName = resource.ResourceName,
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};
			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, DataSource = "validate data" };

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullBoe boeObject = new FullBoe(boe);
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = true, CustomFieldName = "BABBA" };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });

			_TripDTODataLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<TripDTO>());
			_MiscTravelRateDTOLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<MiscTravelRateDTO>());
			_LocationDTODataLoader.Setup(x => x.GetByIds(new Collection<int>())).Returns(new Collection<LocationDTO>());

			CustomFieldValueDTO customFieldValue = new CustomFieldValueDTO { Id = 1, CustomFieldID = 1, CustomFieldValueID = 1, CustomFieldValueDescription = "TestDesc", CustomFieldValueName = "TestName" };
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>() { customFieldValue });
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByLaborTypeIds(It.IsAny<Collection<int>>())).Returns(new Dictionary<int, ICollection<KeyValuePair<int, int>>>() { });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;


			//want to extract the validation contract message
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				foreach (ValidationBOELaborType y in x.LaborTypes)
				{
					Assert.IsTrue(y.LaborTypeValidationMsgs.Count == 1, "Validation Msgs did not equal 1");
					Assert.IsTrue(y.LaborTypeValidationMsgs.Contains("Custom Field BABBA is required."), "The validation msg was correct");
				}
			}

			// Set up to test open ended custom field validation
			CustomFieldDTO customField_OpenEnded = new CustomFieldDTO { Id = 2, CustomFieldName = "BABBA", CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = true };
			customFields = new Collection<CustomFieldDTO>() { customField_OpenEnded };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>() { });
			workspaceObject = new FullWorkspace(workspace);

			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			foreach (ValidationBOETasks x in toAssertTasks)
			{
				foreach (ValidationBOELaborType y in x.LaborTypes)
				{
					Assert.IsTrue(y.LaborTypeValidationMsgs.Count == 1, "Validation Msgs did not equal 1");
					Assert.IsTrue(y.LaborTypeValidationMsgs.Contains("Custom Field BABBA is required."), "The validation msg was correct");
				}
			}
		}

		[TestMethod]
		public void BL_ValidateBOECustomFields()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 3 };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				//ResourceName = resource.ResourceName,
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};
			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, DataSource = "validate data" };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);

			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = true };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			this.retriever.Setup(x => x.CheckIfBoeExistsGivenCustomFieldID(boe.Id, customField.Id)).Returns(true);
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Assert.IsTrue(validationBOE.BOECustomFieldValidationMessages.Count == 0, "There were BOE Custom msgs");
		}

		[TestMethod]
		public void BL_ValidateBOECustomFields_Invalid()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 3 };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				//ResourceName = resource.ResourceName,
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, DataSource = "validate data" };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = true, CustomFieldName = "BOE Apple" };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			//Assert
			Assert.IsTrue(validationBOE.BOECustomFieldValidationMessages.Count == 1, "Validation Msgs did not equal 1");
			Assert.IsTrue(validationBOE.BOECustomFieldValidationMessages.Contains("Custom Field BOE Apple is required."), "The validation msg was correct");
		}

		[TestMethod]
		public void BL_ValidateBOE_MissingHistoricMetrics()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ContractStartDate = Convert.ToDateTime("03/01/2010"), ContractEndDate = Convert.ToDateTime("03/01/2014"), ResourceListID = 1, UsingTemplateBOE = false };
			FullWorkspace workspaceObject = new FullWorkspace(workspace);

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = 1, PerformingOrgID = 1, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				//ResourceName = resource.ResourceName,
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};
			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", WorkspaceID = 1, CLINID = 8, DataSource = "validate data", HistoricMetricDisclosureChecked = false };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);

			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);

			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });

			ICollection<int> ids = new Collection<int>();
			ids.Add(boeTE.Id);

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;
			Collection<ValidationBOELaborType> toAssertLabors = new Collection<ValidationBOELaborType>();

			if (_SpaceEnabled)
			{
				// Metrics deprecated for SSC, should not contain message
				Assert.IsFalse(validationBOE.BOEHeaderMsgs.Contains(BoeDTO.HISTORIC_METRIC_DISCLOSURE_REQUIRED), "The Historic Metric was not required");
			}

			Assert.IsTrue(toAssertTasks.Count == 0, "Task messages were not empty");
			Assert.IsTrue(toAssertLabors.Count == 0, "Task labor messages were not empty");
		}

		[TestMethod]
		public void BL_ValidateBOECustomFields_MissingPerfOrgAndResource()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);
			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ContractStartDate = Convert.ToDateTime("03/01/2010"), ContractEndDate = Convert.ToDateTime("03/01/2014"), ResourceListID = 3 };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { resource });
			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });
			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			//setup a BOE
			ResourceSpreadDto boeLS = new ResourceSpreadDto { Id = 1, BoeID = 4, LaborSpreadDate = Convert.ToDateTime("03/01/2011"), LaborSpreadValue = 100 };
			ResourceTypeDto boeLT = new ResourceTypeDto { Id = 1, BoeID = 4, SpreadType = IES.Common.SpreadType.Hours, LaborSpreads = new Collection<ResourceSpreadDto> { boeLS }, ResourceID = null, PerformingOrgID = null, StartDateValue = Convert.ToDateTime("03/01/2011"), EndDateValue = Convert.ToDateTime("03/01/2011"), PercentSpread = 100, ValueSpread = 100, SpreadCurveID = SpreadCurves.DiscreteHours };
			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = "T56",
				Description = "validate data",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQHoursEquation = "100",
				MOQText = "validate moq",
				MOQType = MOQType.Comparison,
				MOQTypeName = "validate",
				taskElementLabors = new Collection<ResourceTypeDto> { boeLT },
				TaskTitle = "Validate Task",
				TotalHours = 100,
				WorkspaceVariableIDs = new Collection<int> { 2 },
				TaskElementType = TaskElementType.Labor
			};
			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", WorkspaceID = 1, CLINID = 8, StartDate = Convert.ToDateTime("01/10/2011"), EndDate = Convert.ToDateTime("02/03/2012"), DataSource = "validate data" };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);

			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());


			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;


			// verify task error messages
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				// verify labor error messages 
				foreach (ValidationBOELaborType y in x.LaborTypes)
				{
					Assert.IsTrue(y.LaborTypeValidationMsgs.Count == 2, "There weren't 2 labor type messages");
					Assert.IsTrue(y.LaborTypeValidationMsgs.Contains(BoeDTO.RESOURCE_CODE_REQUIRED), "The validation msg was correct");
					Assert.IsTrue(y.LaborTypeValidationMsgs.Contains(BoeDTO.PERFORM_ORG_REQUIRED), "The validation msg was correct");
				}
			}
		}

		[TestMethod]
		public void BL_ValidateBOE_MissingODCFields()
		{
			// Set up
			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permloader = new Mock<IPermissionsDTODataLoader>();


			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName };

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, WasDescriptionSet = true, WasDataSourceSet = true };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permloader.Object);
			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> { boeObject });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, EndDate = Convert.ToDateTime("07/01/2011"), StartDate = Convert.ToDateTime("06/01/2011"), PerformingOrgID = null, ResourceID = null } }, WasDescriptionSet = true, WasMoqTextSet = true } });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertCosts = new Collection<ValidationBOETasks>();
			toAssertCosts = validationBOE.Costs;


			// verify task error messages
			foreach (ValidationBOETasks x in toAssertCosts)
			{
				// verify labor error messages 
				foreach (ValidationBOELaborType y in x.LaborTypes)
				{
					Assert.IsTrue(y.LaborTypeValidationMsgs.Count == 3, "There weren't 3 labor type messages");
					Assert.IsTrue(y.LaborTypeValidationMsgs.Contains(OtherDirectCostDTO.RESOURCE_CODE_REQUIRED), "The validation msg was correct");
					Assert.IsTrue(y.LaborTypeValidationMsgs.Contains(OtherDirectCostDTO.PERFORM_ORG_REQUIRED), "The validation msg was correct");
					Assert.IsTrue(y.LaborTypeValidationMsgs.Contains(OtherDirectCostDTO.SPREAD_REQUIRED), "The valdiating msg was correct");
				}
			}
		}

		[TestMethod]
		public void BL_ValidateBOE_MissingTravelFields()
		{
			// Set up
			ValidationBOEModelView validationBOE = new ValidationBOEModelView();
			Mock<IResourceDTODataLoader> ResourceLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();



			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 1 };

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, WasDescriptionSet = true, WasDataSourceSet = true };
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { });

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { });
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> { boeObject });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO>());
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			int idToUse = 1;
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> {
				new TravelDTO{
					Id = 1,
					BoeID = boe.Id,
								Description = "moq travel1",
								TaskID="T01",
								TaskTitle="title1",
								StartDate = Convert.ToDateTime("01/01/2011"),
								EndDate = Convert.ToDateTime("12/01/2011"),
								TravelTrips = new Collection<TravelTripType> {
															new TravelTripType {
																			BoeID = boe.Id,
																			TripDate = Convert.ToDateTime("01/01/2030"),
																			SystemTripID=idToUse,
																			PerfOrgID = 1}}},
                // travelDTO with no traveltrips to create 1 error message
                new TravelDTO{
								Id = 2,
								BoeID = boe.Id,
								Description = "moq travel2",
								TaskID="T02",
								TaskTitle="title2",
								StartDate = Convert.ToDateTime("01/01/2011"),
								EndDate = Convert.ToDateTime("12/01/2011"),

								TravelTrips = new Collection<TravelTripType> {}}});
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> {
				new TravelDTO{
					Id = 1,
					BoeID = boe.Id,
								Description = "moq travel1",
								TaskID="T01",
								TaskTitle="title1",
								StartDate = Convert.ToDateTime("01/01/2011"),
								EndDate = Convert.ToDateTime("12/01/2011"),
								TravelTrips = new Collection<TravelTripType> {
															new TravelTripType {
																			BoeID = boe.Id,
																			TripDate = Convert.ToDateTime("01/01/2030"),
																			SystemTripID=idToUse,
																			PerfOrgID = 1}}},
                // travelDTO with no traveltrips to create 1 error message
                new TravelDTO{
								Id = 2,
								BoeID = boe.Id,
								Description = "moq travel2",
								TaskID="T02",
								TaskTitle="title2",
								StartDate = Convert.ToDateTime("01/01/2011"),
								EndDate = Convert.ToDateTime("12/01/2011"),

								TravelTrips = new Collection<TravelTripType> {}}});

			_TripDTODataLoader.Setup(x => x.GetByIds(new Collection<int> { idToUse })).Returns(new Collection<TripDTO> { new TripDTO { TripID = idToUse, MiscTravelRateID = idToUse, DepartureLocationID = idToUse, DestinationLocationID = idToUse } });
			_MiscTravelRateDTOLoader.Setup(x => x.GetByIds(new Collection<int> { idToUse })).Returns(new Collection<MiscTravelRateDTO> { new MiscTravelRateDTO { Id = idToUse, MiscTravelRateMode = "moqmode" } });
			_LocationDTODataLoader.Setup(x => x.GetByIds(new Collection<int> { idToUse })).Returns(new Collection<LocationDTO> { new LocationDTO { LocationName = "moq location", Id = idToUse } });

			ResourceLoader.Setup(x => x.GetByListIdAndElementOfCost(workspace.ResourceListID, ElementOfCostType.Travel)).Returns(new Collection<ResourceDTO> { new ResourceDTO { Id = 1, ResourceName = "test" } });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTravels = new Collection<ValidationBOETasks>();
			toAssertTravels = validationBOE.Travels;

			// verify task error messages
			Assert.AreEqual(2, toAssertTravels.Count);

			Assert.AreEqual("Trip: 1 moqmode moq location to moq location", toAssertTravels[0].LaborTypes[0].LaborTypeHeader);
			// since the mock travel task have a start and end date later than the boe date, we are expecting the invalid msgs to show up
			Assert.IsTrue(toAssertTravels[0].TaskElementDetails.TaskElementDetailValidationMessages.Contains(string.Format(TravelDTO.TRAVEL_TASK_START_DATE_INVALID, boe.StartDate.ToString("MM/yyyy"))), "Travel start task was valid");
			Assert.IsTrue(toAssertTravels[0].TaskElementDetails.TaskElementDetailValidationMessages.Contains(string.Format(TravelDTO.TRAVEL_TASK_END_DATE_INVALID, boe.EndDate.ToString("MM/yyyy"))), "Travel end task was valid");
			Assert.AreEqual(toAssertTravels[0].LaborTypes[0].LaborTypeValidationMsgs[0], string.Format(TravelDTO.TRIP_END_DATE_INVALID, "Task", "12/2011")); // the trip dates should be within the travel start/end task
			Assert.AreEqual("Task: T01 title1", toAssertTravels[0].TaskMessage);

			Assert.AreEqual("Task: T02 title2", toAssertTravels[1].TaskMessage);
			Assert.IsTrue(toAssertTravels[1].TaskElementDetails.TaskElementDetailValidationMessages.Contains(string.Format(TravelDTO.TRAVEL_TASK_START_DATE_INVALID, boe.StartDate.ToString("MM/yyyy"))), "Travel start task was valid");
			Assert.IsTrue(toAssertTravels[1].TaskElementDetails.TaskElementDetailValidationMessages.Contains(string.Format(TravelDTO.TRAVEL_TASK_END_DATE_INVALID, boe.EndDate.ToString("MM/yyyy"))), "Travel end task was valid");
		}

		[TestMethod]
		public void BL_ValidateBOE_MissingTravelFields_RMS()
		{
			// Set up
			ValidationBOEModelView validationBOE = new ValidationBOEModelView();
			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<RMSZoneTravelRatesFeesDataLoader> zoneTravelLoader = new Mock<RMSZoneTravelRatesFeesDataLoader>();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 1 };

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, WasDescriptionSet = true, WasDataSourceSet = true };
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { });

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { });
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(new Collection<CustomFieldDTO>() { new CustomFieldDTO() });
			retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> { boeObject });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO>());
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			zoneTravelLoader.Setup(x => x.getAllEscalationRatesByWorkspace(workspace.Id)).Returns(new List<WorkspaceRMSEscalationRatesDTO>() { new WorkspaceRMSEscalationRatesDTO() { Year = DateTime.Today.Year - 1 }, new WorkspaceRMSEscalationRatesDTO() { Year = DateTime.Today.Year }, new WorkspaceRMSEscalationRatesDTO() { Year = DateTime.Today.Year + 1 } });

			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> {
			new TravelDTO{
				Id = 1,
				BoeID = boe.Id,
							Description = "moq travel1",
							TaskID="T01",
							TaskTitle="title1",
							StartDate = Convert.ToDateTime("01/01/2011"),
							EndDate = Convert.ToDateTime("12/01/2011"),
							MSTTravelTrips = new Collection<MSTTravelTripType> {
														new MSTTravelTripType {
																		BoeID = boe.Id,
																		TripDate = Convert.ToDateTime("01/01/2030"),
																		PerfOrgID = 1,
																		Id = 1,
																		ModeID = MSTTravelMode.ZoneNoAirfare,
																		ZoneOriginName = "Test Origin",
																		ZoneDestinationName = "Test Destination"}}}});

			ValidateBOE sut = new ValidateBOEMst(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, mstZoneTravelValidator.Object, zoneTravelLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);
			mstZoneTravelValidator.Setup(x => x.ValidateTravelTaskDetails(It.IsAny<TravelDTO>(), It.IsAny<int>())).Returns(new Collection<ValidationMessage>() { new ValidationMessage() { ValidationIssue = "Test Travel Details Issue" } });
			mstZoneTravelValidator.Setup(x => x.ValidateTravelTrips(It.IsAny<ICollection<MSTTravelTripType>>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<ICollection<int>>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(new Collection<ValidationMessage>() { new ValidationMessage() { ValidationIssue = "Test Travel Trip Issue" } });

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTravels = new Collection<ValidationBOETasks>();
			toAssertTravels = validationBOE.Travels;

			// verify task error messages
			Assert.AreEqual(1, toAssertTravels.Count);
			Assert.AreEqual("Trip: 1 Domestic – Zone - No Airfare Test Origin to Test Destination", toAssertTravels[0].LaborTypes[0].LaborTypeHeader);
			Assert.AreEqual("Test Travel Details Issue", toAssertTravels[0].TaskElementDetails.TaskElementDetailValidationMessages[0]);
			Assert.AreEqual("Test Travel Trip Issue", toAssertTravels[0].LaborTypes[0].LaborTypeValidationMsgs[0]);
			Assert.AreEqual("Task: T01 title1", toAssertTravels[0].TaskMessage);
		}

		[TestMethod]
		public void BL_ValidateTripCustomFields_Invalid()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permissionLoader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 1 };

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, WasDescriptionSet = true, WasDataSourceSet = true };
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { });
			int idToUse = 1;
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);
			FullBoe boeObject = new FullBoe(boe);

			Collection<TravelDTO> travelCollection = new Collection<TravelDTO> {
			new TravelDTO{
				Id = 1,
				BoeID = boe.Id,
							Description = "moq travel1",
							TaskID="T01",
							TaskTitle="title1",
							StartDate = Convert.ToDateTime("01/01/2011"),
							EndDate = Convert.ToDateTime("12/01/2011"),
							TravelTrips = new Collection<TravelTripType> {
														new TravelTripType {
															Id = 12,
																		BoeID = boe.Id,
																		TripDate = Convert.ToDateTime("02/01/2011"),
																		SystemTripID=idToUse,
																		Segment = SegmentType.SSC,
																		PerfOrgID = 1}
							}
			}
		};
			_TripDTODataLoader.Setup(x => x.GetByIds(new Collection<int> { idToUse })).Returns(new Collection<TripDTO> { new TripDTO { TripID = idToUse, MiscTravelRateID = idToUse, DepartureLocationID = idToUse, DestinationLocationID = idToUse } });
			_MiscTravelRateDTOLoader.Setup(x => x.GetByIds(new Collection<int> { idToUse })).Returns(new Collection<MiscTravelRateDTO> { new MiscTravelRateDTO { Id = idToUse, MiscTravelRateMode = "moqmode" } });
			_LocationDTODataLoader.Setup(x => x.GetByIds(new Collection<int> { idToUse })).Returns(new Collection<LocationDTO> { new LocationDTO { LocationName = "moq location", Id = idToUse } });

			resourceDTOLoader.Setup(x => x.GetByListIdAndElementOfCost(workspace.ResourceListID, ElementOfCostType.Travel)).Returns(new Collection<ResourceDTO> { new ResourceDTO { Id = 1, ResourceName = "test" } });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { });
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			//setup custom fields to be false, we'll check them in another test
			retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> { boeObject });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO>());
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });

			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(travelCollection);

			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(travelCollection);
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionLoader.Object);

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(boe.WorkspaceID)).Returns(new Collection<FullWbs>());
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeObject });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldName = "LaborCustom", CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = true };
			CustomFieldValueDTO customValue = new CustomFieldValueDTO { CustomFieldID = 1, CustomFieldValueID = 1, CustomFieldValueInUseFlag = true, Id = 1 };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>() { customValue });

			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });
			Dictionary<int, ICollection<KeyValuePair<int, int>>> toReturn = new Dictionary<int, ICollection<KeyValuePair<int, int>>>();
			toReturn[12] = new Collection<KeyValuePair<int, int>> { new KeyValuePair<int, int>() };
			retriever.Setup(x => x.GetCustomFieldValueIDsContainerIdsByTravelTripsIds(It.IsAny<Collection<int>>())).Returns(toReturn);
			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Travels;


			//want to extract the validation contract message
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				foreach (ValidationBOELaborType y in x.LaborTypes)
				{

					Assert.IsTrue(y.LaborTypeValidationMsgs.Contains("Custom Field LaborCustom is required."), "The validation msg was correct");
				}
			}
		}

		[TestMethod]
		public void BL_ValiateBOE_MissingTasks()
		{
			// Set up
			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();



			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName };

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, WasDescriptionSet = true, WasDataSourceSet = true };

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissions.Object);

			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> { boeObject });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Tasks;


			// Assert
			// verify task error messages
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				Assert.IsTrue(x.TaskElementDetails.TaskElementDetailValidationMessages.Contains(BoeDTO.ONE_TASK_ELEMENT_REQUIRED), "Task element is required.");
			}
		}

		[TestMethod]
		public void BL_InvalidMaterialBOEsToValidate()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permloader = new Mock<IPermissionsDTODataLoader>();


			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName };

			// set up resource

			//setup a BOE
			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("12/01/2011"), WorkspaceID = workspace.Id, HistoricMetricDisclosureChecked = false, WasDescriptionSet = true, WasDataSourceSet = true };
			MaterialDTO materialDTO = new MaterialDTO
			{
				Id = 1,
				BoeID = boe.Id
			};

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permloader.Object);
			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			FullBoe boeObject = new FullBoe(boe);
			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { materialDTO });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> { boeObject });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(new Collection<CustomFieldDTO>());
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, MoqText = "something", TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id } } } });
			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>());

			IValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Materials;

			// verify material error messages
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				Assert.IsTrue(x.TaskElementDetails.TaskElementDetailValidationMessages.Contains(String.Format(BoeDTO.MOQ_TEXT_REQUIRED, CommonConstants.BOE_MOQ_TEXT_LABEL)));

				// verify labor error messages 
				foreach (ValidationBOELaborType y in x.LaborTypes)
				{
					Assert.IsTrue(y.LaborTypeValidationMsgs.Count == 1, "There wasn't 1 labor type messages (resource type required)");
				}
			}

			// now test in Space Systems mode - only the MOQ Text difference needs to be verified
			sut = new ValidateBOESpaceSystems(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Materials;

			// verify task error messages - in space sytems we only need to verify the MOQ Text difference
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				Assert.IsTrue(x.TaskElementDetails.TaskElementDetailValidationMessages.Contains(String.Format(BoeDTO.MOQ_TEXT_REQUIRED, CommonConstants.BOE_MOQ_TEXT_LABEL_SPACE_SYSTEMS)));
			}
		}

		[TestMethod]
		public void BL_InvalidMaterialBOEsDateToValidate()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permloader = new Mock<IPermissionsDTODataLoader>();


			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permloader.Object);

			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName };

			// set up resource

			//setup a BOE
			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("12/01/2011"), WorkspaceID = workspace.Id, HistoricMetricDisclosureChecked = false, WasDescriptionSet = true, WasDataSourceSet = true };
			MaterialDTO materialDTO = new MaterialDTO
			{
				Id = 1,
				BoeID = boe.Id
			};

			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			FullBoe boeObject = new FullBoe(boe);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { materialDTO });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });
			retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe> { boeObject });

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));

			resourceDTOLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(new Collection<CustomFieldDTO>());

			IValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Collection<ValidationBOETasks> toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Materials;

			// verify material error messages
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				Assert.IsTrue(x.TaskElementDetails.TaskElementDetailValidationMessages.Contains(String.Format(BoeDTO.MOQ_TEXT_REQUIRED, CommonConstants.BOE_MOQ_TEXT_LABEL)));

				// verify labor error messages 
				foreach (ValidationBOELaborType y in x.LaborTypes)
				{
					Assert.IsTrue(y.LaborTypeValidationMsgs.Count == 1, "There wasn't 1 labor type messages (expend date not in range)");
				}
			}

			// now test in Space Systems mode - only the MOQ Text difference needs to be verified
			sut = new ValidateBOESpaceSystems(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			toAssertTasks = new Collection<ValidationBOETasks>();
			toAssertTasks = validationBOE.Materials;

			// verify task error messages - in space sytems we only need to verify the MOQ Text difference
			foreach (ValidationBOETasks x in toAssertTasks)
			{
				Assert.IsTrue(x.TaskElementDetails.TaskElementDetailValidationMessages.Contains(String.Format(BoeDTO.MOQ_TEXT_REQUIRED, CommonConstants.BOE_MOQ_TEXT_LABEL_SPACE_SYSTEMS)));
			}
		}

		[TestMethod]
		public void BL_ValiateBOE_BoeDatesWithinClinContract()
		{
			Mock<IResourceDTODataLoader> resourceDTOLoader = new Mock<IResourceDTODataLoader>();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permLoader = new Mock<IPermissionsDTODataLoader>();


			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

			ValidationBOEModelView validationBOE = new ValidationBOEModelView();

			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName, ResourceListID = 3, ContractStartDate = Convert.ToDateTime("10/01/2012"), ContractEndDate = Convert.ToDateTime("12/01/2013") };

			// set up resource
			ResourceDTO resource = new ResourceDTO { Id = 1, ResourceName = "ResourceValidate", ResourceDesc = "validate resource" };
			resourceDTOLoader.Setup(x => x.GetById(resource.Id)).Returns(resource);

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

			BoeTaskElementDTO boeTE = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = 4,
				BOETaskID = string.Empty,
				Description = "The following hours represent the estimated hours.",
				StartDate = Convert.ToDateTime("02/01/2011"),
				EndDate = Convert.ToDateTime("06/01/2011"),
				MOQText = "Test",
				MOQType = MOQType.Factor,
				MOQTypeName = "Test.",
				TaskTitle = "Labor Task",
				TotalHours = 4,
				TaskElementType = TaskElementType.Labor
			};

			ClinDTO clin = new ClinDTO { Id = 1, ClinTitle = "2", ClinNumber = "2", StartDate = null, EndDate = null };
			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "DooDah.", StartDate = Convert.ToDateTime("08/01/2012"), EndDate = Convert.ToDateTime("03/01/2014"), WorkspaceID = workspace.Id, DataSource = "validate data", CLINID = clin.Id };
			FullWorkspace workspaceObject = new FullWorkspace(workspace);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			FullBoe boeObject = new FullBoe(boe);
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { boeTE });
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { boeTE });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO>() { });
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			retriever.Setup(x => x.GetFullBoesByWorkspaceId(boe.WorkspaceID, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boeObject });
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO>() { });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTE });
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);
			this.factory.Setup(x => x.CreateFullWbses(It.IsAny<ICollection<int>>())).Returns(new List<FullWbs>());

			#region Setup Unity references

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), resourceDTOLoader.Object);

			#endregion

			this.retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO> { resource });


			//Act
			validationBOE = sut.ValidateBOE_OnValidateBtnClick(boeObject, workspaceObject);

			// Assert
			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Count == 2, "The BOE Header was empty");
			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Contains(string.Format(BoeDTO.BOE_START_DATE_INVALID, "Contract", workspace.ContractStartDate.ToString("MM/yyyy"))));
			Assert.IsTrue(validationBOE.BOEHeaderMsgs.Contains(string.Format(BoeDTO.BOE_END_DATE_INVALID, "Contract", workspace.ContractEndDate.ToString("MM/yyyy"))));

		}

		[TestMethod]
		public void BL_ValidateAllBOEs()
		{
			// Set up
			ValidationAllBOEModelView validationBOE = new ValidationAllBOEModelView();

			Mock<IPerformingOrgDTODataLoader> perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			Mock<VariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation = new Mock<VariableSelectBOEtoSumCalculation>(perfOrgLoader.Object);
			Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
			Mock<BOECommentsResponsesValidator> _BOECommentsResponsesValidator = new Mock<BOECommentsResponsesValidator>(_boeCommentDTODataLoader.Object);
			Mock<ITripDTODataLoader> _TripDTODataLoader = new Mock<ITripDTODataLoader>();
			Mock<IMiscTravelRateDTOLoader> _MiscTravelRateDTOLoader = new Mock<IMiscTravelRateDTOLoader>();
			Mock<ILocationDTODataLoader> _LocationDTODataLoader = new Mock<ILocationDTODataLoader>();

			Mock<IPermissionsDTODataLoader> permloader = new Mock<IPermissionsDTODataLoader>();


			// set up workspace
			string WorkspaceName = "ValidateWorkspace";
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = WorkspaceName };

			BoeDTO boe = new BoeDTO { Id = 4, Title = "My BOE Title", Description = "Validate BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, WasDescriptionSet = true, WasDataSourceSet = true };
			BoeDTO boe2 = new BoeDTO { Id = 5, Title = "BOE Title", Description = "Validate second BOE", StartDate = Convert.ToDateTime("02/01/2011"), EndDate = Convert.ToDateTime("10/01/2011"), WorkspaceID = workspace.Id, WasDescriptionSet = true, WasDataSourceSet = true };
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permloader.Object);
			FullWorkspace ws = new FullWorkspace(workspace);
			FullBoe boeObject = new FullBoe(boe);
			FullBoe boeObject2 = new FullBoe(boe2);

			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO> { new WorkspaceVariableDTO { WorkspaceID = 1, Id = 2, WorkspaceVariableName = "Validate1", WorkspaceVariableValue = 64.0m } };
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(boe.WorkspaceID)).Returns(workspaceVars);
			this.retriever.Setup(x => x.GetWorkspaceById(boe.WorkspaceID)).Returns(workspace);
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(new FullWorkspace(workspace));
			factory.Setup(x => x.CreateFullWorkspace(boe.WorkspaceID)).Returns(new FullWorkspace(workspace));

			//setup custom fields to be false, we'll check them in another test
			CustomFieldDTO customField = new CustomFieldDTO { Id = 1, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = false };
			ICollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>() { customField };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(customFields);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<OtherDirectCostDTO> { new OtherDirectCostDTO { Id = 1, BoeID = boe.Id, TaskDescription = "mock odc", TaskTitle = "task title", ODCTypes = new Collection<OtherDirectCostType> { new OtherDirectCostType { ODCTypeID = 1, BoeID = boe.Id, EndDate = Convert.ToDateTime("07/01/2011"), StartDate = Convert.ToDateTime("06/01/2011"), PerformingOrgID = null, ResourceID = null } }, WasDescriptionSet = true, WasMoqTextSet = true } });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe2.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			retriever.Setup(x => x.GetNumberOfMaterialsForBoeId(boe2.Id)).Returns(0);
			retriever.Setup(x => x.GetMaterialCollectionByBoeID(boe2.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe2.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { });

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { });
			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO> { });
			retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<MaterialDTO> { });
			retriever.Setup(x => x.GetResourcesByResourceListId(workspace.ResourceListID)).Returns(new Collection<ResourceDTO> { });

			ValidateBOE sut = new ValidateBOE(_VariableSelectBOEtoSumCalculation.Object, _BOECommentsResponsesValidator.Object, _TripDTODataLoader.Object, _MiscTravelRateDTOLoader.Object, _LocationDTODataLoader.Object, offloadRatesLoader.Object, rteTemplateLoader.Object);

			Collection<FullBoe> boesToValidate = new Collection<FullBoe>();
			boesToValidate.Add(boeObject);
			boesToValidate.Add(boeObject2);
			retriever.Setup(i => i.GetFullBoesByWorkspaceId(workspace.Id, true, It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(boesToValidate);

			//Act
			validationBOE = sut.ValidateAllBOEs(ws);


			// verify that we have mulitple boe errors
			foreach (ValidationBOEModelView z in validationBOE.AllBOEs)
			{
				//check the cost.
				foreach (ValidationBOETasks x in z.Costs)
				{
					// verify labor error messages 
					foreach (ValidationBOELaborType y in x.LaborTypes)
					{
						Assert.IsTrue(y.LaborTypeValidationMsgs.Count == 3, "There weren't 3 labor type messages");
						Assert.IsTrue(y.LaborTypeValidationMsgs.Contains(OtherDirectCostDTO.RESOURCE_CODE_REQUIRED), "The validation msg was correct");
						Assert.IsTrue(y.LaborTypeValidationMsgs.Contains(OtherDirectCostDTO.PERFORM_ORG_REQUIRED), "The validation msg was correct");
						Assert.IsTrue(y.LaborTypeValidationMsgs.Contains(OtherDirectCostDTO.SPREAD_REQUIRED), "The valdiating msg was correct");
					}
				}
			}
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask for validation that PoPStart is on a Monday
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_PoPStartMonday_RmsMode()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.MST;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();
			Utilities.IsSAPEnabledForSystem = true;

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical, // Historical so we are using the MOQ Table
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 3), // Monday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "TestFilter",
							TotalRelevantHours = 1000,
							TotalWbsHours = 2000
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = "Test historical reference explanation"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsFalse(result.Any());

			// Add a day so PoP start is no longer on a Monday
			moqType.TableData.First().PoPStart = new DateTime(2022, 1, 4);

			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().Contains("Monday"));

			// Lastly clear the date to make sure we get the required field validation, but not the Monday validation
			moqType.TableData.First().PoPStart = new DateTime(1, 1, 1);

			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Contains("required")));
			Assert.IsFalse(result.Any(x => x.Contains("Monday")));
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask for validation that PoPStart is on a Sunday for SAP FW; Space Mode with Weekly query type, validation should happen
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_PoPStartSunday_SpaceMode_Weekly()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.SpaceSystems;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical, // Historical so we are using the MOQ Table
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							RepositoryName = RepositoryName.SapWebi.GetDescription(),
							QueryType = MoqTableData.WEEKLY,
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 2), // Sunday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "TestFilter",
							TotalRelevantHours = 1000
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = "Test historical reference explanation"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsFalse(result.Any());

			// Add a day so PoP start is no longer on a Sunday
			moqType.TableData.First().PoPStart = new DateTime(2022, 1, 3);

			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Contains("Sunday")));
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask for validation that PoPStart is on a Sunday for SAP FW; Space Mode with Monthly query type, no validation should happen
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_PoPStartSunday_SpaceMode_Monthly()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.SpaceSystems;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical, // Historical so we are using the MOQ Table
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							RepositoryName = RepositoryName.SapWebi.GetDescription(),
							QueryType = MoqTableData.MONTHLY,
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 2), // Sunday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "TestFilter",
							TotalRelevantHours = 1000
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = "Test historical reference explanation"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsFalse(result.Any());

			// Add a day so PoP start is no longer on a Sunday
			moqType.TableData.First().PoPStart = new DateTime(2022, 1, 3);

			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsFalse(result.Any());
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask for validation that PoPEnd is on a Sunday.. Testing for RMS, validation should happen
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_PoPEndSunday_RmsMode()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.MST;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical, // Historical so we are using the MOQ Table
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 3), // Monday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "TestFilter",
							TotalRelevantHours = 1000,
							TotalWbsHours = 2000
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = "Test historical reference explanation"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsFalse(result.Any());

			// Add a day so PoP start is no longer on a Sunday
			moqType.TableData.First().PoPEnd = new DateTime(2022, 1, 10);

			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().Contains("Sunday"));

			// Lastly clear the date to make sure we get the required field validation, but not the Monday validation
			moqType.TableData.First().PoPEnd = new DateTime(1, 1, 1);

			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Contains("required")));
			Assert.IsFalse(result.Any(x => x.Contains("Sunday")));
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask for validation that PoPEnd is on a Sunday.. Testing for Space with Monthly Weekly Type, validation should happen
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_PoPEndSunday_SpaceMode_Weekly()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.SpaceSystems;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical, // Historical so we are using the MOQ Table
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							RepositoryName = RepositoryName.SapWebi.GetDescription(),
							QueryType = MoqTableData.WEEKLY,
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 2), // Sunday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "TestFilter",
							TotalRelevantHours = 1000
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = "Test historical reference explanation"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsFalse(result.Any());

			// Add a day so PoP start is no longer on a Sunday
			moqType.TableData.First().PoPEnd = new DateTime(2022, 1, 10);

			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Contains("Sunday")));
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask for validation that PoPEnd is on a Sunday.. Testing for Space with Monthly Query Type, no validation should happen
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_PoPEndSunday_SpaceMode_Monthly()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.SpaceSystems;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical, // Historical so we are using the MOQ Table
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							RepositoryName = RepositoryName.SapWebi.GetDescription(),
							QueryType = MoqTableData.MONTHLY,
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 3), // Sunday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "TestFilter",
							TotalRelevantHours = 1000
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = "Test historical reference explanation"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsFalse(result.Any());

			// Add a day so PoP start is no longer on a Sunday
			moqType.TableData.First().PoPEnd = new DateTime(2022, 1, 10);

			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsFalse(result.Any());
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask for additional PoP date and Date of Report validation
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_AdditionalDateValidation()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.SpaceSystems;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();
			MoqTypeTableDataLabels labels = new MoqTypeTableDataLabels();

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical, // Historical so we are using the MOQ Table
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							RepositoryName = RepositoryName.SapWebi.GetDescription(),
							QueryType = MoqTableData.MONTHLY,
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 3), // Sunday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "TestFilter",
							TotalRelevantHours = 1000
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = "Test historical reference explanation"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsFalse(result.Any());

			// 'Remove' date of report to test required validation
			moqType.TableData.First().DateOfReport = new DateTime(1, 1, 1);
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Contains(labels.DateOfReport) && x.Contains("required")));

			// adjust date of report to after today to test on/before today validation
			moqType.TableData.First().DateOfReport = DateTime.Now.AddDays(1);
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Contains($"{labels.DateOfReport} must be on or before today's date.")));

			// 'Remove' start date to test required validation
			moqType.TableData.First().DateOfReport = DateTime.Now;
			moqType.TableData.First().PoPStart = new DateTime(1, 1, 1);
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Contains(labels.PoPStart) && x.Contains("required")));

			// add start date back and 'remove' end date to test required validation
			moqType.TableData.First().PoPStart = new DateTime(2022, 1, 3);
			moqType.TableData.First().PoPEnd = new DateTime(1, 1, 1);
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Contains(labels.PoPEnd) && x.Contains("required")));

			// add end date back and adjust start date to after today (this also sets start date after end date, so test that too)
			moqType.TableData.First().PoPEnd = new DateTime(2022, 1, 9);
			moqType.TableData.First().PoPStart = DateTime.Now.AddDays(1);
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Contains($"{labels.PoPStart} must be on or before today's date")));
			Assert.IsTrue(result.Any(x => x.Contains($"{labels.PoPStart} must be on or before {labels.PoPEnd}")));

			// reset start date and adjust end date to after today
			moqType.TableData.First().PoPStart = new DateTime(2022, 1, 3);
			moqType.TableData.First().PoPEnd = DateTime.Now.AddDays(1);
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Contains($"{labels.PoPEnd} must be on or before today's date")));
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask, specifically rules for Additional Query Field.. RMS Company
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_AdditionalQueryFields_RmsMode()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.MST;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical,
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 3), // Monday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "aaa",
							TotalWbsHours = 2000,
							TotalRelevantHours = 1000
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = "Test historical reference explanation"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			// Valid data to start
			Assert.IsFalse(result.Any());

			// Make the additional query filters null
			moqType.TableData.First().AdditionalQueryFilters = null;

			// RMS && SAP = true -> Not Required - False (IsAny)
			Utilities.IsSAPEnabledForSystem = true;
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);
			Assert.IsFalse(result.Any());

			// RMS && SAP = false -> Required - True (IsAny)
			Utilities.IsSAPEnabledForSystem = false;
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);
			Assert.IsTrue(result.Any());
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask, specifically rules for Additional Query Field.. Space Company
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_AdditionalQueryFields_SpaceMode()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.SpaceSystems;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical,
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							RepositoryName = RepositoryName.SapWebi.GetDescription(),
							QueryType = MoqTableData.WEEKLY,
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 2), // Sunday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "aaa",
							TotalRelevantHours = 1000
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = "Test historical reference explanation"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			// Valid data to start
			Assert.IsFalse(result.Any());

			// Make the additional query filters N/A
			moqType.TableData.First().AdditionalQueryFilters = "N/A";

			// SSC && SAP = True && Repo = SAP -> Not Required - False (IsAny)
			moqType.TableData.First().RepositoryName = RepositoryName.SapWebi.GetDescription();
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);
			Assert.IsFalse(result.Any());

			// SSC && SAP = True && Repo = Other -> Required - False (IsAny)
			moqType.TableData.First().RepositoryName = RepositoryName.Other.GetDescription();
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);
			Assert.IsFalse(result.Any());

			// SSC && SAP = false -> Required - False (IsAny)
			Utilities.IsSAPEnabledForSystem = false;
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);
			Assert.IsFalse(result.Any());

		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask, specifically rules for Total Relevant Hours - Space Company
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_Hours_SpaceMode()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.SpaceSystems;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical,
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							RepositoryName = RepositoryName.SapWebi.GetDescription(),
							QueryType = MoqTableData.WEEKLY,
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 2), // Sunday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "aaa",
							TotalRelevantHours = 1000
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = "Test historical reference explanation"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			// Valid data to start
			Assert.IsFalse(result.Any());

			// Relevant Hours less than 0
			moqType.TableData.First().TotalRelevantHours = -1;
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());

			// Relevant Hours over 10 digits
			moqType.TableData.First().TotalRelevantHours = 1000000000;
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask, specifically rules for Total Relevant Hours - RMS
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_Hours_RmsMode()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.MST;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical,
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 3), // Monday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "aaa",
							TotalWbsHours = 2000,
							TotalRelevantHours = 1000,
							RepositoryName = "aa",
							QueryType = "aa"
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = "Test historical reference explanation"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			// Valid data to start
			Assert.IsFalse(result.Any());

			// SAP disabled to test invalid WBS Hours
			Utilities.IsSAPEnabledForSystem = false;

			// WBS Hours less than 0
			moqType.TableData.First().TotalWbsHours = -1;
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());

			// WBS Hours over 10 digits
			moqType.TableData.First().TotalWbsHours = 1000000000;
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());

			// WBS Hours not validated when SAP enabled
			Utilities.IsSAPEnabledForSystem = true;
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsFalse(result.Any());

			// Relevant Hours less than 0
			moqType.TableData.First().TotalWbsHours = 1000;
			moqType.TableData.First().TotalRelevantHours = -1;
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());

			// Relevant Hours over 10 digits
			moqType.TableData.First().TotalRelevantHours = 1000000000;
			result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask, specifically rules for HistoricalReferenceExplanation
		/// Note: Assumes start date of feature of 1/1/2024 (in GenBOE.Tests app.config)
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_HistoricalReferenceExplanation()
		{
			// SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.SpaceSystems;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();

			// Create MOQ Table with blank HistoricalReferenceExplanation (blank after the start date)
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical,
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							RepositoryName = RepositoryName.SapWebi.GetDescription(),
							QueryType = MoqTableData.WEEKLY,
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 2), // Sunday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "aaa",
							TotalRelevantHours = 1000
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = string.Empty
			};

			// create ws with creation date after the feature start date
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = new DateTime(2024, 1, 2) };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			Assert.IsTrue(result.Any());
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask, specifically rules for HistoricalReferenceExplanation before the feature start date
		/// Note: Assumes start date of feature of 1/1/2024 (in GenBOE.Tests app.config)
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_HistoricalReferenceExplanation_BeforeFeatureStart()
		{
			// SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.SpaceSystems;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();

			// Create MOQ Table with blank HistoricalReferenceExplanation
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.Historical,
				TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							TableName = "Test Table",
							RepositoryName = RepositoryName.SapWebi.GetDescription(),
							QueryType = MoqTableData.WEEKLY,
							ContractNumber = "1",
							DateOfReport = DateTime.Now,
							HistoricalProgramName = "Test Name",
							WbsElement = "Test WBS",
							PoPStart = new DateTime(2022, 1, 2), // Sunday
                            PoPEnd = new DateTime(2022, 1, 9), // Sunday
                            AdditionalQueryFilters = "aaa",
							TotalRelevantHours = 1000
						}
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				HistoricalReferenceExplanation = string.Empty
			};

			// create ws with creation date before the feature start date
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = new DateTime(2023, 1, 2) };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			// Assert no results because field is not required before the start date
			Assert.IsFalse(result.Any());
		}

		/// <summary>
		/// Test Null Current Resource with included set to 'Yes'
		/// </summary>
		[TestMethod]
		public void ValidateSkillMix_NullCurrentResource()
		{
			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = "HISTORICAL_R",
					HistoricalHours = 100,
					ResourceNew = null,
					Included = true,
					IsUserInput = true,
					Rationale = "Rationale1",
					BOESkillMix = 100
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateSkillMixTable(skillmix, false);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("Included cannot be set to 'Yes' for an empty/null Current Resource"));
		}

		/// <summary>
		/// Test Invalid BOE Skill Mix (null)
		/// </summary>
		[TestMethod]
		public void ValidateSkillMix_NullBoeSkillMix()
		{
			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = "HISTORICAL_R",
					HistoricalHours = 100,
					ResourceNew = "NEW",
					Included = true,
					IsUserInput = true,
					Rationale = "Rationale1",
					BOESkillMix = null
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateSkillMixTable(skillmix, false);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("BOE Skill Mix is missing"));
		}

		/// <summary>
		/// Test Invalid BOE Skill Mix (<=0)
		/// </summary>
		[TestMethod]
		public void ValidateSkillMix_InvalidBoeSkillMix()
		{
			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = "HISTORICAL_R",
					HistoricalHours = 100,
					ResourceNew = "NEW",
					Included = true,
					IsUserInput = true,
					Rationale = "Rationale1",
					BOESkillMix = 0
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateSkillMixTable(skillmix, false);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("BOE Skill Mix has invalid value"));
		}

		/// <summary>
		/// Test Invalid BOE Skill Mix Total
		/// </summary>
		[TestMethod]
		public void ValidateSkillMix_InvalidSkillMixTotal()
		{
			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = "HISTORICAL_R",
					HistoricalHours = 100,
					ResourceNew = "NEW",
					Included = true,
					IsUserInput = true,
					Rationale = "Rationale1",
					BOESkillMix = 50
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateSkillMixTable(skillmix, false);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("BOE Skill Mix total must be either 0% or 100%"));
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask does not run MOQ Table Validation for Analogous Relationships for RMS
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_ARMoqTable_RmsMode()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.MST;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();
			Utilities.IsSAPEnabledForSystem = true;

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.AnalogousRelationships,
				TableData = new Collection<MoqTableData>()
					{
						// blank table so we know it would be invalid if checked
						new MoqTableData() { }
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				CerName = "AR Name"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			// RMS does not have tables for AR, so table validation should not run and 0 errors should be returned
			Assert.IsFalse(result.Any());
		}

		/// <summary>
		/// Test ValidateTemplateMoqForTask does run MOQ Table Validation for Analogous Relationships for Space
		/// </summary>
		[TestMethod]
		public void BL_ValidateTemplateMoqForTask_ARMoqTable_SscMode()
		{
			SystemConfiguration.Instance().CompanyMode = CompanyConfiguration.SpaceSystems;
			Utilities.IsSAPEnabledForSystem = true;

			ValidateBOE sut = CreateSystem();
			Utilities.IsSAPEnabledForSystem = true;

			// Create a valid MOQ Table
			MoqTypeSelection moqType = new MoqTypeSelection()
			{
				SelectedMOQType = MOQType.AnalogousRelationships,
				TableData = new Collection<MoqTableData>()
					{
						// blank table so we know it would be invalid if checked
						new MoqTableData() { }
					},
				Rationale = "Test Rationale",
				SkillMixRationale = "Test Skill Mix",
				CerName = "AR Name"
			};

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1, WorkspaceName = "Test WS", CreationDate = DateTime.Now };
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<string> result = sut.ValidateTemplateMoqForTask(new Collection<MoqTypeSelection>() { moqType }, ws, false);

			// The specifics of MOQ Table validation are checked in other tests, this test just wants to make sure the validation is run in space for AR
			// With a blank table, we expect 10 errors - one for each field
			Assert.IsTrue(result.Any());
			Assert.AreEqual(10, result.Count);
			Assert.IsTrue(result.All(x => x.StartsWith(MOQType.AnalogousRelationships.GetDescription())));
		}

		/// <summary>
		/// Test Rationale Length
		/// </summary>
		[TestMethod]
		public void ValidateSkillMix_RationaleLength()
		{
			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = "HISTORICAL_R",
					HistoricalHours = 100,
					ResourceNew = null,
					Included = false,
					IsUserInput = true,
					Rationale = "Rationale1"
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateSkillMixTable(skillmix, false);
			Assert.IsNotNull(messages);
			Assert.IsTrue(messages.None());
		}

		/// <summary>
		/// Test Rationale Length
		/// </summary>
		[TestMethod]
		public void ValidateSkillMix_RationaleLength_Bad()
		{
			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = "HISTORICAL_Res",
					HistoricalHours = 100,
					ResourceNew = null,
					Included = false,
					IsUserInput = true,
					Rationale = "Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1"
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateSkillMixTable(skillmix, false);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("The maximum length of the Rationale field"));
		}

		/// <summary>
		/// Test Historical Resource Length
		/// </summary>
		[TestMethod]
		public void ValidateSkillMix_HistoricalResourceLength_Bad()
		{
			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = "HISTORICAL_Res1234567", // 21 characters, max is 20
					HistoricalHours = 100,
					ResourceNew = null,
					Included = false,
					IsUserInput = true,
					Rationale = "Rationale1"
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateSkillMixTable(skillmix, false);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("The maximum length of the Historical Resource field"));
		}

		/// <summary>
		/// Test Rationale being empty on save (valid)
		/// </summary>
		[TestMethod]
		public void ValidateSkillMix_RationaleEmptyOnSave()
		{
			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = "HISTORICAL_R",
					HistoricalHours = 100,
					ResourceNew = null,
					Included = false,
					IsUserInput = true,
					Rationale = string.Empty
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateSkillMixTable(skillmix, false);
			Assert.IsNotNull(messages);
			Assert.IsTrue(messages.None());
		}

		/// <summary>
		/// Test Rationale being empty on validate button click (invalid)
		/// </summary>
		[TestMethod]
		public void ValidateSkillMix_RationaleEmptyOnValidate()
		{
			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = "HISTORICAL_R",
					HistoricalHours = 100,
					ResourceNew = null,
					Included = false,
					IsUserInput = true,
					Rationale = string.Empty
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateSkillMixTable(skillmix, true);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("The Rationale field") && messages.First().Contains("required"));
		}

		/// <summary>
		/// Test Historical Old Length
		/// </summary>
		[TestMethod]
		public void ValidateSkillMix_HistoricalOldLength_Bad()
		{
			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = "HISTORICAL_RESOURCE_NAME1",
					HistoricalHours = 100,
					ResourceNew = null,
					Included = false,
					IsUserInput = true,
					Rationale = "Rationale1"
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateSkillMixTable(skillmix, false);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("The maximum length of the Historical Resource field"));
		}

		/// <summary>
		/// Test Rationale length too long
		/// </summary>
		[TestMethod]
		public void ValidateCommonDisclosureSkillMix_RationaleLength()
		{
			List<CommonDisclosureModelView> skillmix = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = "HISTORICAL_R",
					HistoricalHours = 100,
					BusinessResourceID = null,
					Included = true,
					IsUserInput = true,
					Rationale = "Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1Rationale1",
					BOESkillMix = 100
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateCommonDisclosureSkillMixTable(skillmix, true);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("The maximum length of the Rationale field"));
		}

		/// <summary>
		/// Test BOE Skill Mix missing
		/// </summary>
		[TestMethod]
		public void ValidateCommonDisclosureSkillMix_BoeSkillMixMissing()
		{
			List<CommonDisclosureModelView> skillmix = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = "HISTORICAL_R",
					HistoricalHours = 100,
					BusinessResourceID = null,
					Included = true,
					IsUserInput = true,
					Rationale = "Rationale1",
					BOESkillMix = null
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateCommonDisclosureSkillMixTable(skillmix, true);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("BOE Skill Mix is missing"));
		}

		/// <summary>
		/// Test BOE Skill Mix <= 0
		/// </summary>
		[TestMethod]
		public void ValidateCommonDisclosureSkillMix_BoeSkillMixInvalid()
		{
			List<CommonDisclosureModelView> skillmix = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = "HISTORICAL_R",
					HistoricalHours = 100,
					BusinessResourceID = null,
					Included = true,
					IsUserInput = true,
					Rationale = "Rationale1",
					BOESkillMix = 0
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateCommonDisclosureSkillMixTable(skillmix, true);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("BOE skill Mix has invalid value"));
		}

		/// <summary>
		/// Test at least one resource included
		/// </summary>
		[TestMethod]
		public void ValidateCommonDisclosureSkillMix_MissingResource()
		{
			List<CommonDisclosureModelView> skillmix = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = "HISTORICAL_R",
					HistoricalHours = 100,
					BusinessResourceID = null,
					Included = false,
					IsUserInput = true,
					Rationale = "Rationale1",
					BOESkillMix = 0
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateCommonDisclosureSkillMixTable(skillmix, true);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("At least one Resource has to be included"));
		}

		/// <summary>
		/// Test Boe Skill Mix total must be 0 or 100
		/// </summary>
		[TestMethod]
		public void ValidateCommonDisclosureSkillMix_BoeSkillMixTotal()
		{
			List<CommonDisclosureModelView> skillmix = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = "HISTORICAL_R",
					HistoricalHours = 100,
					BusinessResourceID = null,
					Included = true,
					IsUserInput = true,
					Rationale = "Rationale1",
					BOESkillMix = 50
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateCommonDisclosureSkillMixTable(skillmix, true);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("BOE Skill Mix total must be either 0% or 100%"));
		}

		/// <summary>
		/// Test each BRC unique for each resource
		/// </summary>
		[TestMethod]
		public void ValidateCommonDisclosureSkillMix_EachBrcUniqueForResource()
		{
			List<CommonDisclosureModelView> skillmix = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = "HISTORICAL_R",
					HistoricalHours = 100,
					BusinessResourceID = "NEW",
					Included = true,
					IsUserInput = true,
					Rationale = "Rationale1",
					BOESkillMix = 50
				},
				new CommonDisclosureModelView {
					ResourceID = "HISTORICAL_R",
					HistoricalHours = 100,
					BusinessResourceID = "NEW",
					Included = true,
					IsUserInput = true,
					Rationale = "Rationale2",
					BOESkillMix = 50
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateCommonDisclosureSkillMixTable(skillmix, true);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("Each BRC must be unique for Resource"));
		}

		/// <summary>
		/// Test Rationale being empty on save (valid)
		/// </summary>
		[TestMethod]
		public void ValidateCommonDisclosureSkillMix_RationaleEmptyOnSave()
		{
			List<CommonDisclosureModelView> skillmix = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = "HISTORICAL_R",
					HistoricalHours = 100,
					BusinessResourceID = null,
					Included = true,
					IsUserInput = true,
					Rationale = string.Empty,
					BOESkillMix = 100
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateCommonDisclosureSkillMixTable(skillmix, false);
			Assert.IsNotNull(messages);
			Assert.IsTrue(messages.None());
		}

		/// <summary>
		/// Test Rationale being empty on validate button click (invalid)
		/// </summary>
		[TestMethod]
		public void ValidateCommonDisclosureSkillMix_RationaleEmptyOnValidate()
		{
			List<CommonDisclosureModelView> skillmix = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = "HISTORICAL_R",
					HistoricalHours = 100,
					BusinessResourceID = null,
					Included = true,
					IsUserInput = true,
					Rationale = string.Empty,
					BOESkillMix = 100
				}
			};

			ICollection<string> messages = ActionLogicUtility.ValidateCommonDisclosureSkillMixTable(skillmix, true);
			Assert.IsNotNull(messages);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.First().Contains("The Rationale field") && messages.First().Contains("required"));
		}
	}
}