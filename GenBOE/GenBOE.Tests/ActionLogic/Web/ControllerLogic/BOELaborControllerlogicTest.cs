// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Web.ControllerLogic
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Web;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
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

	[TestClass]
	public class BOELaborControllerlogicTest : MOQObject
	{
		private Mock<BoeTaskElementRecalculation> _BoeTaskElementRecalculation = null;
		private Mock<IBOEStateMachine> _boeStateMachine = null;
		private Mock<IBoeMediator> _BoeMediator = null;
		private Mock<IBoeTaskElementMediator> _BoeTaskElementMediator = null;
		private Mock<IWorkspaceVariableDTODataLoader> _WorkspaceVarLoader = null;
		private Mock<IVariableSelectBOEtoSumCalculation> _VariableSelectBoeToSum = null;
		private Mock<IResourceDTODataLoader> _ResourceLoader = null;
		private Mock<IFullObjectFactory> factory;
		private Mock<IRetriever> retriever;
		private Mock<IBoeDTODataLoader> _BoeLoader = null;
		private Mock<IBoeTaskElementDTODataLoader> _TaskElementDataLoader = null;
		private Mock<IUserDTODataLoader> _UserLoader = null;
		private Mock<IPermissionsDTODataLoader> _PermissionDataLoader = null;
		private Mock<IPerformingOrgDTODataLoader> perfOrgLoader;
		private Mock<ICommonDataMapper> _CommonDataMapper = null;
		private Mock<IOrdinaryVariableLoader> _TaskVariableLoader = null;
		private Mock<TaskElementValidation> _TaskElementValidation = null;
		private Mock<IVariableCircularReferenceChecker> circularReferenceChecker = null;
		private Mock<ICommonDataMapper> commonDataMapper = null;
		private Mock<IRteTemplateDataLoader> rteTemplateDataLoader = null;
		private Mock<IMoqTypeDataLoader> moqTypeDataLoader = null;
		private Mock<ITMResourceRateDTODataLoader> tmResourceRateDTODataLoader = null;
		private Mock<IValidateBOE> validateBOE = null;
		private Mock<IMoqTableExporter> moqTableExporter = null;
		private Mock<IMoqTableImporter> moqTableImporter = null;
		private Mock<ITokenService> tokenservice = new Mock<ITokenService>();
		private Mock<ICache> cache = new Mock<ICache>();
		private Mock<GenBOE.ActionLogic.IESSAPClient.IESSAPClient> iesSapClient = new Mock<GenBOE.ActionLogic.IESSAPClient.IESSAPClient>();

		private const string RESOURCE_NAME1 = "Resource1";
		private const string RESOURCE_NAME2 = "Resource2";
		private const string RESOURCE_NAME3 = "Resource3";
		private const string BRC_RESOURCE_NAME1 = "BRCResource1";
		private const string BRC_RESOURCE_NAME2 = "BRCResource2";
		private const string BRC_RESOURCE_NAME3 = "BRCResource3";
		private const string HISTORICAL_RESOURCE_NAME1 = "HistoricalResource1";
		private const string HISTORICAL_RESOURCE_NAME2 = "HistoricalResource2";
		private const string HISTORICAL_RESOURCE_NAME3 = "HistoricalResource3";
		private const string Rationale1 = "Rationale1";
		private const string Rationale2 = "Rationale2";
		private const string Rationale3 = "Rationale3";

		#region Private members

		// Test
		private BOELaborControllerLogic CreateSystem()
		{
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IActiveDirectoryUtilities), new ActiveDirectoryUtilities());

			this.CreateCommonSystem();

			return new BOELaborControllerLogic(
				   _BoeTaskElementRecalculation.Object,
				   _boeStateMachine.Object,
				   _BoeMediator.Object,
				   _BoeTaskElementMediator.Object,
				   _VariableSelectBoeToSum.Object,
				   _ResourceLoader.Object,
				   this.factory.Object,
				   _WorkspaceVarLoader.Object,
				   _BoeLoader.Object,
				   _TaskElementDataLoader.Object,
				   _UserLoader.Object,
				   _PermissionDataLoader.Object,
				   perfOrgLoader.Object,
				   _TaskVariableLoader.Object,
				   _TaskElementValidation.Object,
				   circularReferenceChecker.Object,
				   commonDataMapper.Object,
				   this.rteTemplateDataLoader.Object,
				   this.moqTypeDataLoader.Object,
				   this.tmResourceRateDTODataLoader.Object,
				   this.validateBOE.Object,
				   this.moqTableExporter.Object,
				   this.moqTableImporter.Object,
				   iesSapClient.Object,
				   tokenservice.Object,
				   cache.Object
			);
		}

		private BOELaborControllerLogic CreateSystemSSC()
		{
			this.CreateCommonSystem();

			return new BOELaborControllerLogicSpace(
				   _BoeTaskElementRecalculation.Object,
				   _boeStateMachine.Object,
				   _BoeMediator.Object,
				   _BoeTaskElementMediator.Object,
				   _VariableSelectBoeToSum.Object,
				   _ResourceLoader.Object,
				   this.factory.Object,
				   _WorkspaceVarLoader.Object,
				   _UserLoader.Object,
				   _BoeLoader.Object,
				   _TaskElementDataLoader.Object,
				   _PermissionDataLoader.Object,
				   perfOrgLoader.Object,
				   _TaskVariableLoader.Object,
				   _TaskElementValidation.Object,
				   circularReferenceChecker.Object,
				   commonDataMapper.Object,
				   rteTemplateDataLoader.Object,
				   this.moqTypeDataLoader.Object,
				   this.tmResourceRateDTODataLoader.Object,
				   this.validateBOE.Object,
				   this.moqTableExporter.Object,
				   this.moqTableImporter.Object,
				   iesSapClient.Object,
				   tokenservice.Object,
				   cache.Object
			);
		}

		private BOELaborControllerLogic CreateSystemMST()
		{
			this.CreateCommonSystem();

			return new BOELaborControllerLogicMST(
				   _BoeTaskElementRecalculation.Object,
				   _boeStateMachine.Object,
				   _BoeMediator.Object,
				   _BoeTaskElementMediator.Object,
				   _VariableSelectBoeToSum.Object,
				   _ResourceLoader.Object,
				   this.factory.Object,
				   _WorkspaceVarLoader.Object,
				   _UserLoader.Object,
				   _BoeLoader.Object,
				   _TaskElementDataLoader.Object,
				   _PermissionDataLoader.Object,
				   perfOrgLoader.Object,
				   _TaskVariableLoader.Object,
				   _TaskElementValidation.Object,
				   circularReferenceChecker.Object,
				   commonDataMapper.Object,
				   rteTemplateDataLoader.Object,
				   this.moqTypeDataLoader.Object,
				   this.tmResourceRateDTODataLoader.Object,
				   this.validateBOE.Object,
				   this.moqTableExporter.Object,
				   this.moqTableImporter.Object,
				   iesSapClient.Object,
				   tokenservice.Object,
				   cache.Object
			);
		}

		/// <summary>
		/// Sets up the common components of the system needed for tests
		/// </summary>
		private void CreateCommonSystem()
		{
			this.rteTemplateDataLoader = new Mock<IRteTemplateDataLoader>();
			_WorkspaceVarLoader = new Mock<IWorkspaceVariableDTODataLoader>();
			_boeStateMachine = new Mock<IBOEStateMachine>();
			_BoeMediator = new Mock<IBoeMediator>();
			_BoeTaskElementMediator = new Mock<IBoeTaskElementMediator>();
			_ResourceLoader = new Mock<IResourceDTODataLoader>();
			this.perfOrgLoader = new Mock<IPerformingOrgDTODataLoader>();
			_VariableSelectBoeToSum = new Mock<IVariableSelectBOEtoSumCalculation>();
			this.factory = new Mock<IFullObjectFactory>();
			this.retriever = new Mock<IRetriever>();
			_CommonDataMapper = new Mock<ICommonDataMapper>();
			_BoeLoader = new Mock<IBoeDTODataLoader>();
			_TaskElementDataLoader = new Mock<IBoeTaskElementDTODataLoader>();
			_UserLoader = new Mock<IUserDTODataLoader>();
			_PermissionDataLoader = new Mock<IPermissionsDTODataLoader>();
			_CommonDataMapper = new Mock<ICommonDataMapper>();
			this._TaskVariableLoader = new Mock<IOrdinaryVariableLoader>();
			_TaskElementValidation = new Mock<TaskElementValidation>();
			circularReferenceChecker = new Mock<IVariableCircularReferenceChecker>();
			commonDataMapper = new Mock<ICommonDataMapper>();
			this.moqTypeDataLoader = new Mock<IMoqTypeDataLoader>();
			this.validateBOE = new Mock<IValidateBOE>();
			this.moqTableExporter = new Mock<IMoqTableExporter>();
			this.moqTableImporter = new Mock<IMoqTableImporter>();
			this.tmResourceRateDTODataLoader = new Mock<ITMResourceRateDTODataLoader>();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _PermissionDataLoader.Object);

			_BoeTaskElementRecalculation = new Mock<BoeTaskElementRecalculation>(_VariableSelectBoeToSum.Object, factory.Object);
			this.rteTemplateDataLoader.Setup(x => x.GetByBoeIdAndTaskId(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).Returns(new List<RTECustomTemplateQuestionAnswerModelView>());
			this.retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });
		}

		/// <summary>
		/// Create ModelView for testing and set up necessary methods
		/// </summary>
		/// <param name="boe">boe</param>
		/// <param name="ws">workspace</param>
		/// <returns>Modelview for testing</returns>
		private LaborTaskDataModelView CreateModelView(FullBoe boe, FullWorkspace ws)
		{
			LaborTaskDataModelView toReturn = new LaborTaskDataModelView();

			// setup custom fields at the task level
			CustomFieldDTO taskCustomField = new CustomFieldDTO { Id = 2, CustomFieldName = "TaskColor", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.TaskDisplay, CustomFieldRequired = true };
			CustomFieldValueDTO taskCustomFieldValue_Red = new CustomFieldValueDTO { CustomFieldID = taskCustomField.Id, CustomFieldValueID = 2, CustomFieldValueName = "Red", CustomFieldValueDescription = "the color red", CustomFieldValueInUseFlag = true };
			CustomFieldValueDTO taskCustomFieldValue_Blue = new CustomFieldValueDTO { CustomFieldID = taskCustomField.Id, CustomFieldValueID = 3, CustomFieldValueName = "Blue", CustomFieldValueDescription = "the color blue", CustomFieldValueInUseFlag = true };
			Collection<CustomFieldValueDTO> colorOptions = new Collection<CustomFieldValueDTO> { taskCustomFieldValue_Red, taskCustomFieldValue_Blue };

			// setup custom fields at the labor type level
			CustomFieldDTO laborCustomField = new CustomFieldDTO { Id = 2, CustomFieldName = "LaborColor", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = true };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<CustomFieldDTO> { laborCustomField, taskCustomField });
			CustomFieldValueDTO laborCustomFieldValue_Red = new CustomFieldValueDTO { CustomFieldID = laborCustomField.Id, CustomFieldValueID = 2, CustomFieldValueName = "Red", CustomFieldValueDescription = "the color red", CustomFieldValueInUseFlag = true };
			CustomFieldValueDTO laborCustomFieldValue_Blue = new CustomFieldValueDTO { CustomFieldID = laborCustomField.Id, CustomFieldValueID = 3, CustomFieldValueName = "Blue", CustomFieldValueDescription = "the color blue", CustomFieldValueInUseFlag = true };
			Collection<CustomFieldValueDTO> laborColorOptions = new Collection<CustomFieldValueDTO> { laborCustomFieldValue_Red, laborCustomFieldValue_Blue };
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(new Collection<int> { taskCustomField.Id, laborCustomField.Id }, It.IsAny<int>())).Returns(colorOptions.Concat(laborColorOptions).ToCollection());

			WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { WorkspaceID = ws.Id, ValueType = VarValueType.Discrete, WorkspaceVariableValue = 300, WorkspaceVariableName = "TestWorkspaceVar" };
			// Resource Types only used for SumOfBoes, so empty in taskVar below
			BoeTaskOrdinaryVariableModelView taskVar = new BoeTaskOrdinaryVariableModelView { OrdinaryVariableID = 8, OrdinaryVariableName = "TestTaskVar", OrdinaryVariableValueType = VarValueType.Discrete, OrdinaryVariableValue = "600", ResourceTypes = new Collection<int> { } };
			CustomFieldSelectionModelView taskCustomFieldSelection = new CustomFieldSelectionModelView { CustomFieldValueID = taskCustomField.Id, SelectionID = taskCustomFieldValue_Blue.CustomFieldID };

			// task data
			toReturn.TaskElementData = new TaskElementDetailModelView
			{
				TaskElementDetailID = 3,
				BOEID = boe.Id,
				TaskID = "A55",
				MOQHoursEquation = "(200 * 3) + 4 + TestTaskVar + TestWorkspaceVar",
				WorkspaceVariableIDs = new Collection<int> { workspaceVar.Id },
				TaskOrdinaryVariables = new Collection<BoeTaskOrdinaryVariableModelView> { taskVar },
				StartDate = "12/2012",
				EndDate = "12/2012",
				CustomFieldValues = new Collection<CustomFieldSelectionModelView> { taskCustomFieldSelection },
				MetricIds = new Collection<int>() { 1 }
			};

			CustomFieldSelectionModelView laborCustomFieldSelection = new CustomFieldSelectionModelView { CustomFieldValueID = laborCustomField.Id, SelectionID = laborCustomFieldValue_Blue.CustomFieldID };
			LaborSpreadDataModelView spread = new LaborSpreadDataModelView { LaborSpreadDate = "12/2012", LaborSpreadValue = 1504 };
			LaborTypeDataModelView labor = new LaborTypeDataModelView
			{
				BOELaborTypeID = 3,
				BOETaskElementID = toReturn.TaskElementData.TaskElementDetailID,
				StartDate = "12/2012",
				EndDate = "12/2012",
				ResourceID = this.Resource.Id,
				PerformingOrgID = this.Perforg.Id,
				ElementOfCost = (int)ElementOfCostType.LMLabor,
				SpreadCurveID = SpreadCurves.SpreadCurve1,
				RateType = RateType.Hours,
				HourSpread = 1504,
				HourSpreadLocked = true,
				PercentSpread = 100m,
				PercentSpreadLocked = false,
				CustomFieldValues = new Collection<CustomFieldSelectionModelView> { laborCustomFieldSelection },
				Spreads = new Collection<LaborSpreadDataModelView>() { spread },
				LaborTypeOrder = 1
			};

			toReturn.LaborTypesData = new Collection<LaborTypeDataModelView> { labor };

			toReturn.MOQTypes = new List<MoqTypeSelection>() {
				new MoqTypeSelection() {
					Id = 1,
					BoeId = 1,
					TaskId = 1,
					SelectedMOQType = MOQType.Historical,
					Rationale = "test",
					SkillMixRationale = "test",
					HistoricalReferenceExplanation = "test",
					TableData = new Collection<MoqTableData>()
					{
						new MoqTableData()
						{
							Id = 1,
							MOQTypeSelectionId = 1,
							TableName = "test",
							ContractNumber = "test",
							HistoricalProgramName = "test",
							RepositoryName = "test",
							DateOfReport = DateTime.Today,
							PoPStart = DateTime.Today,
							PoPEnd = DateTime.Today.AddDays(-1),
							QueryType = "test",
							WbsElement = "test",
							TotalWbsHours = 10,
							AdditionalQueryFilters = "test",
							TotalRelevantHours = 1,
							CustomFieldValueContainers = new Collection<CustomFieldValueContainer>()
							{
								new CustomFieldValueContainer()
								{
									Id = 1,
									ContainerID = 1,
									OwnerID = 1,
									CustomFieldID = 1,
									CustomFieldValueID = 1,
									OpenEndedValue = "test",
									IsOpenEnded = true
								}
							}
						}
					}
				}
			};

			_WorkspaceVarLoader.Setup(x => x.GetByWorkspaceID(ws.Id)).Returns(new Collection<WorkspaceVariableDTO> { workspaceVar });
			_VariableSelectBoeToSum.Setup(x => x.GetTaskVarLabelTotal(It.IsAny<OrdinaryVariableDto>(), It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(decimal.Parse(taskVar.OrdinaryVariableValue));
			_VariableSelectBoeToSum.Setup(x => x.GetWorkspaceVarLabelTotal(It.IsAny<WorkspaceVariableDTO>(), It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(workspaceVar.WorkspaceVariableValue);

			BoeTaskElementDTO boeTask = new BoeTaskElementDTO
			{
				Id = toReturn.TaskElementData.TaskElementDetailID.Value,
				OrdinaryVariables = new Collection<OrdinaryVariableDto> { new OrdinaryVariableDto { Id = taskVar.OrdinaryVariableID,
					OrdinaryVariableName = taskVar.OrdinaryVariableName, BoeID = boe.Id } },
				BoeID = boe.Id,
				BOETaskID = "A55",
				StartDate = boe.StartDate,
				EndDate = boe.EndDate
			};

			factory.Setup(x => x.CreateTaskElement(boeTask.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTask);
			factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(boe);
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTask });
			retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, false, ws.DecimalPrecision, ws.CostDecimalPrecision)).Returns(new Collection<BoeTaskElementDTO>() { boeTask });
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(ws.Id)).Returns(new Collection<CustomFieldDTO>() { taskCustomField, laborCustomField });
			retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new Collection<WorkspaceVariableDTO> { workspaceVar });
			retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>());
			retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>());
			retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			retriever.Setup(x => x.GetResourcesByResourceListId(ws.ResourceListID)).Returns(new Collection<ResourceDTO>() { });

			return toReturn;
		}

		/// <summary>
		/// Create DTO for testing, based on the modelview
		/// </summary>
		/// <param name="boe">boe</param>
		/// <param name="ws">workspace</param>
		/// <param name="sut">controller logic</param>
		/// <returns>dto for testing</returns>
		private BoeTaskElementDTO CreateDto(FullBoe boe, FullWorkspace ws, BOELaborControllerLogic sut)
		{
			LaborTaskDataModelView modelView = this.CreateModelView(boe, ws);
			BoeTaskElementDTO toReturn = sut.ConvertModelViewToDto(modelView, ws);
			return toReturn;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
		public void CalculateLinkedTaskElementsTest_WithTaskVariables()
		{
			BOELaborControllerLogic sut = CreateSystem();
			Collection<ValidationMessage> validations = new Collection<ValidationMessage>();

			//test
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2 };

			FullWorkspace ws = new FullWorkspace(workspace);

			// BOE C is the first task element we're saving. BOE B uses BOE C as part of it's boe to sum task variable so that should be identified to be updated/saved. Since BOE B is being updated, BOE A should
			// also be updated since A uses B in it's boe to sum task variable

			BoeDTO boeC = new BoeDTO { Id = 1, WorkspaceID = this.Workspace.Id };
			ResourceSpreadDto boeLSC = new ResourceSpreadDto { BoeID = boeC.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 200 };
			ResourceTypeDto boeLaborC = new ResourceTypeDto { BoeID = boeC.Id, Id = 1, SpreadType = IES.Common.SpreadType.Hours, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 200, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSC } };
			BoeTaskElementDTO boeTaskElementC = new BoeTaskElementDTO { Id = 1, BoeID = boeC.Id, MOQHoursEquation = "200", LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborC }, TaskElementType = TaskElementType.Labor };

			BoeDTO boeB = new BoeDTO { Id = 2, WorkspaceID = this.Workspace.Id };
			OrdinaryVariableDto taskOrdinaryVarB = new OrdinaryVariableDto { BoeID = boeB.Id, Id = 2, OrdinaryVariableName = "BLAH", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = boeC.Id, CLINID = null, WBSID = null } } };
			WorkspaceVariableDTO workspaceVarB = new WorkspaceVariableDTO { WorkspaceID = this.Workspace.Id, Id = 3, WorkspaceVariableName = "BAGS", WorkspaceVariableValue = 200m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { } };
			ResourceSpreadDto boeLSB = new ResourceSpreadDto { BoeID = boeB.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
			ResourceTypeDto boeLaborB = new ResourceTypeDto { BoeID = boeB.Id, Id = 2, SpreadType = IES.Common.SpreadType.Hours, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSB } };
			BoeTaskElementDTO boeTaskElementB = new BoeTaskElementDTO { Id = 2, BoeID = boeB.Id, MOQHoursEquation = "15000 + BLAH + 2 + BAGS", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVarB }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborB }, WorkspaceVariableIDs = new Collection<int> { workspaceVarB.Id }, TaskElementType = TaskElementType.Labor };

			BoeDTO boeA = new BoeDTO { Id = 3, WorkspaceID = this.Workspace.Id, State = BOEState.AwaitingApproval };
			OrdinaryVariableDto taskOrdinaryVarA = new OrdinaryVariableDto { BoeID = boeA.Id, Id = 3, OrdinaryVariableName = "BLAH3", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = boeB.Id, CLINID = null, WBSID = null } } };
			ResourceSpreadDto boeLSA = new ResourceSpreadDto { BoeID = boeA.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
			ResourceTypeDto boeLaborA = new ResourceTypeDto { BoeID = boeA.Id, Id = 3, SpreadType = IES.Common.SpreadType.Hours, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSA } };
			BoeTaskElementDTO boeTaskElementA = new BoeTaskElementDTO { Id = 3, BoeID = boeA.Id, MOQHoursEquation = "15000 + BLAH3 + 2 + BAGS", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVarA }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborA }, TaskElementType = TaskElementType.Labor };

			FullBoe boeAObject = new FullBoe(boeA);
			FullBoe boeBObject = new FullBoe(boeB);
			FullBoe boeCObject = new FullBoe(boeC);

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeAObject, boeBObject, boeCObject });
			this.retriever.Setup(x => x.GetWorkspaceById(this.Workspace.Id)).Returns(workspace);

			this.factory.Setup(x => x.CreateFullWorkspace(workspace.Id)).Returns(ws);
			this.factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(ws);
			this.factory.Setup(x => x.CreateFullWorkspace(ws)).Returns(ws);

			this.factory.Setup(x => x.CreateFullBoe(boeA.Id)).Returns(boeAObject);
			this.factory.Setup(x => x.CreateFullBoe(boeB.Id)).Returns(boeBObject);
			this.factory.Setup(x => x.CreateFullBoe(boeC.Id)).Returns(boeCObject);
			this.factory.Setup(x => x.CreateFullBoe(boeA)).Returns(boeAObject);
			this.factory.Setup(x => x.CreateFullBoe(boeB)).Returns(boeBObject);
			this.factory.Setup(x => x.CreateFullBoe(boeC)).Returns(boeCObject);
			this.factory.Setup(x => x.CreateFullBoe(boeAObject)).Returns(boeAObject);
			this.factory.Setup(x => x.CreateFullBoe(boeBObject)).Returns(boeBObject);
			this.factory.Setup(x => x.CreateFullBoe(boeCObject)).Returns(boeCObject);

			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeCObject, VariableType.Task, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementB });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeCObject, VariableType.Workspace, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeBObject, VariableType.Task, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementA });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeBObject, VariableType.Workspace, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeAObject, VariableType.Task, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeAObject, VariableType.Workspace, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });

			this.retriever.Setup(x => x.GetFullWorkspaceById(It.IsAny<int>())).Returns(ws);
			this.factory.Setup(x => x.CreateFullBoes(It.IsAny<Collection<int>>())).Returns(new Collection<FullBoe> { boeAObject, boeBObject, boeCObject });
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<WorkspaceVariableDTO> { });
			this.retriever.Setup(x => x.GetWorkspaceVariableIdsForBoe(It.IsAny<int>())).Returns(new Collection<int> { });

			// for this test case, we are updating C which kick off direct changes to B and then indirect changes to A
			sut.CalculateLinkedTaskElements(validations, new Collection<BoeTaskElementDTO> { boeTaskElementC }, new Collection<BoeTaskElementDTO> { }, ws);
			_BoeTaskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO> { boeTaskElementB }, ws), Times.Once());
			_BoeTaskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO> { boeTaskElementA }, ws), Times.Once());

			// since boeA was in the awaiting approval state, it goes back to draft. verify the calls happened
			_BoeMediator.Verify(x => x.MediatedSave(ws, boeAObject), Times.Once());
			_boeStateMachine.Verify(x => x.PerformStateTransitionAction(boeAObject, ws, BOEState.AwaitingApproval, BOEState.Draft), Times.Once());

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
		public void CalculateLinkedTaskElementsTest_WithTaskAndWorkspaceVariables()
		{
			BOELaborControllerLogic sut = CreateSystem();
			Collection<ValidationMessage> validations = new Collection<ValidationMessage>();

			//test
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2 };
			FullWorkspace ws = new FullWorkspace(workspace);

			// BOE C is the first task element we're saving. BOE B uses BOE C as part of a boe to sum workspace variable so that should be identified to be updated/saved. Since BOE B is being updated, BOE A should
			// also be updated since A uses B in it's boe to sum task variable

			BoeDTO boeC = new BoeDTO { Id = 1, WorkspaceID = this.Workspace.Id };
			ResourceSpreadDto boeLSC = new ResourceSpreadDto { BoeID = boeC.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 200 };
			ResourceTypeDto boeLaborC = new ResourceTypeDto { BoeID = boeC.Id, Id = 1, SpreadType = IES.Common.SpreadType.Hours, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 200, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSC } };
			BoeTaskElementDTO boeTaskElementC = new BoeTaskElementDTO { Id = 1, BoeID = boeC.Id, MOQHoursEquation = "200", LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborC }, TaskElementType = TaskElementType.Labor };

			BoeDTO boeB = new BoeDTO { Id = 2, WorkspaceID = this.Workspace.Id };
			// using a boe to sum workspace variable in this task element
			WorkspaceVariableDTO workspaceVarB = new WorkspaceVariableDTO { WorkspaceID = this.Workspace.Id, Id = 3, WorkspaceVariableName = "BAGS", WorkspaceVariableValue = 200m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = boeC.Id, CLINID = null, WBSID = null } } };
			ResourceSpreadDto boeLSB = new ResourceSpreadDto { BoeID = boeB.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
			ResourceTypeDto boeLaborB = new ResourceTypeDto { BoeID = boeB.Id, Id = 2, SpreadType = IES.Common.SpreadType.Hours, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSB } };
			BoeTaskElementDTO boeTaskElementB = new BoeTaskElementDTO { Id = 2, BoeID = boeB.Id, MOQHoursEquation = "15000 + 2 + BAGS", LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborB }, WorkspaceVariableIDs = new Collection<int> { workspaceVarB.Id }, TaskElementType = TaskElementType.Labor };


			BoeDTO boeA = new BoeDTO { Id = 3, WorkspaceID = this.Workspace.Id, State = BOEState.AwaitingApproval };
			OrdinaryVariableDto taskOrdinaryVarA = new OrdinaryVariableDto { BoeID = boeA.Id, Id = 3, OrdinaryVariableName = "BLAH3", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = boeB.Id, CLINID = null, WBSID = null } } };
			ResourceSpreadDto boeLSA = new ResourceSpreadDto { BoeID = boeA.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
			ResourceTypeDto boeLaborA = new ResourceTypeDto { BoeID = boeA.Id, Id = 3, SpreadType = IES.Common.SpreadType.Hours, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSA } };
			BoeTaskElementDTO boeTaskElementA = new BoeTaskElementDTO { Id = 3, BoeID = boeA.Id, MOQHoursEquation = "15000 + BLAH3", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVarA }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborA }, TaskElementType = TaskElementType.Labor };

			FullBoe boeAObject = new FullBoe(boeA);
			FullBoe boeBObject = new FullBoe(boeB);
			FullBoe boeCObject = new FullBoe(boeC);

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeAObject, boeBObject, boeCObject });
			this.retriever.Setup(x => x.GetWorkspaceById(this.Workspace.Id)).Returns(workspace);

			this.factory.Setup(x => x.CreateFullWorkspace(workspace.Id)).Returns(ws);
			this.factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(ws);
			this.factory.Setup(x => x.CreateFullWorkspace(ws)).Returns(ws);

			this.factory.Setup(x => x.CreateFullBoe(boeA.Id)).Returns(boeAObject);
			this.factory.Setup(x => x.CreateFullBoe(boeB.Id)).Returns(boeBObject);
			this.factory.Setup(x => x.CreateFullBoe(boeC.Id)).Returns(boeCObject);
			this.factory.Setup(x => x.CreateFullBoe(boeA)).Returns(boeAObject);
			this.factory.Setup(x => x.CreateFullBoe(boeB)).Returns(boeBObject);
			this.factory.Setup(x => x.CreateFullBoe(boeC)).Returns(boeCObject);
			this.factory.Setup(x => x.CreateFullBoe(boeAObject)).Returns(boeAObject);
			this.factory.Setup(x => x.CreateFullBoe(boeBObject)).Returns(boeBObject);
			this.factory.Setup(x => x.CreateFullBoe(boeCObject)).Returns(boeCObject);

			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeCObject, VariableType.Task, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementB });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeCObject, VariableType.Workspace, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeBObject, VariableType.Task, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementA });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeBObject, VariableType.Workspace, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeAObject, VariableType.Task, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeAObject, VariableType.Workspace, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });

			this.retriever.Setup(x => x.GetFullWorkspaceById(It.IsAny<int>())).Returns(ws);
			this.factory.Setup(x => x.CreateFullBoes(It.IsAny<Collection<int>>())).Returns(new Collection<FullBoe> { boeAObject, boeBObject, boeCObject });
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<WorkspaceVariableDTO> { });
			this.retriever.Setup(x => x.GetWorkspaceVariableIdsForBoe(It.IsAny<int>())).Returns(new Collection<int> { });
			this._WorkspaceVarLoader.Setup(x => x.GetByIds(new Collection<int> { workspaceVarB.Id })).Returns(new Collection<WorkspaceVariableDTO> { workspaceVarB });

			// for this test case, we are updating C which kick off direct changes to B and then indirect changes to A
			sut.CalculateLinkedTaskElements(validations, new Collection<BoeTaskElementDTO> { boeTaskElementC }, new Collection<BoeTaskElementDTO> { }, ws);
			_BoeTaskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO> { boeTaskElementB }, ws), Times.Once());
			_BoeTaskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO> { boeTaskElementA }, ws), Times.Once());

			// since boeA was in the awaiting approval state, it goes back to draft. verify the calls happened
			_BoeMediator.Verify(x => x.MediatedSave(ws, boeAObject), Times.Once());
			_boeStateMachine.Verify(x => x.PerformStateTransitionAction(boeAObject, ws, BOEState.AwaitingApproval, BOEState.Draft), Times.Once());

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Collections.Generic.Dictionary`2<System.Int32,System.Collections.Generic.ICollection`1<System.Int32>>"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
		public void CalculateLinkedTaskElementsTest_WithUpdateToNonInUseWorkspaceVariable()
		{
			BOELaborControllerLogic sut = CreateSystem();
			Collection<ValidationMessage> validations = new Collection<ValidationMessage>();

			//test
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2 };
			FullWorkspace ws = new FullWorkspace(workspace);

			// BOE C is the first task element we're saving. BOE B uses BOE C as part of a boe to sum workspace variable so that should be identified to be updated/saved. Since BOE B is being updated, BOE A should
			// also be updated since A uses B in it's boe to sum task variable

			BoeDTO boeC = new BoeDTO { Id = 1, WorkspaceID = this.Workspace.Id };
			ResourceSpreadDto boeLSC = new ResourceSpreadDto { BoeID = boeC.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 200 };
			ResourceTypeDto boeLaborC = new ResourceTypeDto { BoeID = boeC.Id, Id = 1, SpreadType = IES.Common.SpreadType.Hours, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 200, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSC } };
			BoeTaskElementDTO boeTaskElementC = new BoeTaskElementDTO { Id = 1, BoeID = boeC.Id, MOQHoursEquation = "200", LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborC }, TaskElementType = TaskElementType.Labor, WorkspaceVariableIDs = new Collection<int> { 3 } };

			BoeDTO boeB = new BoeDTO { Id = 2, WorkspaceID = this.Workspace.Id };
			ResourceSpreadDto boeLSB = new ResourceSpreadDto { BoeID = boeB.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
			ResourceTypeDto boeLaborB = new ResourceTypeDto { BoeID = boeB.Id, Id = 2, SpreadType = IES.Common.SpreadType.Hours, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSB } };
			BoeTaskElementDTO boeTaskElementB = new BoeTaskElementDTO { Id = 2, BoeID = boeB.Id, MOQHoursEquation = "15000 + 2", LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborB }, TaskElementType = TaskElementType.Labor };

			BoeDTO boeA = new BoeDTO { Id = 3, WorkspaceID = this.Workspace.Id, State = BOEState.AwaitingApproval };
			OrdinaryVariableDto taskOrdinaryVarA = new OrdinaryVariableDto { BoeID = boeA.Id, Id = 3, OrdinaryVariableName = "BLAH3", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = boeB.Id, CLINID = null, WBSID = null } } };
			ResourceSpreadDto boeLSA = new ResourceSpreadDto { BoeID = boeA.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
			ResourceTypeDto boeLaborA = new ResourceTypeDto { BoeID = boeA.Id, Id = 3, SpreadType = IES.Common.SpreadType.Hours, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = this.Perforg.Id, ResourceID = this.Resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSA } };
			BoeTaskElementDTO boeTaskElementA = new BoeTaskElementDTO { Id = 3, BoeID = boeA.Id, MOQHoursEquation = "15000 + BLAH3", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVarA }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborA }, TaskElementType = TaskElementType.Labor };

			// this is a boe to sum workspace variable that is not in use by a task element, but we still need to make sure it's being updated
			WorkspaceVariableDTO workspaceVarB = new WorkspaceVariableDTO { WorkspaceID = this.Workspace.Id, Id = 3, WorkspaceVariableName = "BAGS", WorkspaceVariableValue = 15002m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = boeB.Id, CLINID = null, WBSID = null } } };

			FullBoe boeAObject = new FullBoe(boeA);
			FullBoe boeBObject = new FullBoe(boeB);
			FullBoe boeCObject = new FullBoe(boeC);

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { boeAObject, boeBObject, boeCObject });
			this.retriever.Setup(x => x.GetWorkspaceById(this.Workspace.Id)).Returns(workspace);

			this.factory.Setup(x => x.CreateFullWorkspace(workspace.Id)).Returns(ws);
			this.factory.Setup(x => x.CreateFullWorkspace(workspace)).Returns(ws);
			this.factory.Setup(x => x.CreateFullWorkspace(ws)).Returns(ws);

			this.factory.Setup(x => x.CreateFullBoe(boeA.Id)).Returns(boeAObject);
			this.factory.Setup(x => x.CreateFullBoe(boeB.Id)).Returns(boeBObject);
			this.factory.Setup(x => x.CreateFullBoe(boeC.Id)).Returns(boeCObject);
			this.factory.Setup(x => x.CreateFullBoe(boeA)).Returns(boeAObject);
			this.factory.Setup(x => x.CreateFullBoe(boeB)).Returns(boeBObject);
			this.factory.Setup(x => x.CreateFullBoe(boeC)).Returns(boeCObject);
			this.factory.Setup(x => x.CreateFullBoe(boeAObject)).Returns(boeAObject);
			this.factory.Setup(x => x.CreateFullBoe(boeBObject)).Returns(boeBObject);
			this.factory.Setup(x => x.CreateFullBoe(boeCObject)).Returns(boeCObject);

			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeCObject, VariableType.Task, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementB });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeCObject, VariableType.Workspace, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeBObject, VariableType.Task, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementA });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeBObject, VariableType.Workspace, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeAObject, VariableType.Task, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeAObject, VariableType.Workspace, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });

			this.retriever.Setup(x => x.GetFullWorkspaceById(It.IsAny<int>())).Returns(ws);
			this.factory.Setup(x => x.CreateFullBoes(It.IsAny<Collection<int>>())).Returns(new Collection<FullBoe> { boeAObject, boeBObject, boeCObject });
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<WorkspaceVariableDTO> { });
			this.retriever.Setup(x => x.GetWorkspaceVariableIdsForBoe(It.IsAny<int>())).Returns(new Collection<int> { });

			_WorkspaceVarLoader.Setup(x => x.GetById(workspaceVarB.Id)).Returns(workspaceVarB);
			_VariableSelectBoeToSum.Setup(x => x.GetWorkspaceVarLabelTotal(workspaceVarB, It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(15002m);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boeB.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);

			// for this test case, we are updating C which kick off direct changes to B and then indirect changes to A
			sut.CalculateLinkedTaskElements(validations, new Collection<BoeTaskElementDTO> { boeTaskElementC }, new Collection<BoeTaskElementDTO> { }, ws);
			_BoeTaskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO> { boeTaskElementB }, ws), Times.Once());
			_BoeTaskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO> { boeTaskElementA }, ws), Times.Once());

			// since boeA was in the awaiting approval state, it goes back to draft. verify the calls happened
			_BoeMediator.Verify(x => x.MediatedSave(ws, boeAObject), Times.Once());
			_boeStateMachine.Verify(x => x.PerformStateTransitionAction(boeAObject, ws, BOEState.AwaitingApproval, BOEState.Draft), Times.Once());
		}

		[TestMethod]
		public void ConvertTaskVariableModelViewCollectionToTaskOrdinaryVariableCollectionTest()
		{

			BOELaborControllerLogic sut = CreateSystem();

			BoeDTO boe = new BoeDTO { Id = 1 };
			// this is the task var model view
			BoeTaskOrdinaryVariableModelView existingTaskVarMV = new BoeTaskOrdinaryVariableModelView { OrdinaryVariableID = 5, OrdinaryVariableName = "TestExistVar", IsPercentage = false, DefaultSize = "111.00", OrdinaryVariableValueType = VarValueType.SumOfBOEs, ResourceTypes = new Collection<int> { 1 }, BOEToSum = new Collection<int> { 2 } };

			// this is the task var. this should match existingTaskVarMV 
			OrdinaryVariableDto existingTaskVar = new OrdinaryVariableDto { Id = 5, OrdinaryVariableName = "TestExistVar", IsPercentage = false, DefaultSize = "111.00", BoeID = boe.Id, SortBOEBy = VarSortBOEBy.CLIN, SumVariableResourceTypeIDs = new Collection<int> { 1 }, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = 2, CLINID = null, WBSID = null } } };

			Collection<BoeTaskOrdinaryVariableModelView> taskVars = new Collection<BoeTaskOrdinaryVariableModelView>();
			taskVars.Add(existingTaskVarMV);
			WorkspaceDTO workspaceDto = new WorkspaceDTO() { WorkspaceName = "workspace" };


			BoeTaskElementDTO boeTask = new BoeTaskElementDTO { Id = 5, OrdinaryVariables = new Collection<OrdinaryVariableDto> { existingTaskVar }, BoeID = boe.Id };
			factory.Setup(x => x.CreateTaskElement(boeTask.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTask);

			Collection<OrdinaryVariableDto> taskVariables = sut.ConvertTaskVariableModelViewCollectionToTaskOrdinaryVariableCollection(taskVars, boeTask.Id, boe.Id, workspaceDto);
			Assert.IsTrue(taskVariables.Contains(existingTaskVar), "Existing task variable did not come back");

		}

		/// <summary>
		/// Test ValidateMOQEquation without variables
		/// </summary>
		[TestMethod]
		public void Test_ValidateMOQEquation_WithoutVariables()
		{
			BOELaborControllerLogic sut = CreateSystem();

			//test
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2 };
			BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = workspace.Id };
			FullWorkspace ws = new FullWorkspace(workspace);

			TaskElementDetailModelView taskDetail = new TaskElementDetailModelView { TaskElementDetailID = 3, BOEID = boe.Id, MOQHoursEquation = "(200*3)+4" };
			LaborSpreadDataModelView spread = new LaborSpreadDataModelView { LaborSpreadDate = "12/01/2012", LaborSpreadValue = 604 };
			LaborTypeDataModelView labor = new LaborTypeDataModelView
			{
				BOELaborTypeID = 3,
				BOETaskElementID = taskDetail.TaskElementDetailID,
				ResourceID = this.Resource.Id,
				PerformingOrgID = this.Perforg.Id,
				ElementOfCost = (int)ElementOfCostType.LMLabor,
				SpreadCurveID = SpreadCurves.SpreadCurve1,
				HourSpread = 604,
				HourSpreadLocked = true,
				PercentSpread = 100m,
				PercentSpreadLocked = false,
				Spreads = new Collection<LaborSpreadDataModelView>() { spread }
			};
			LaborTaskDataModelView task = new LaborTaskDataModelView { TaskElementData = taskDetail, LaborTypesData = new Collection<LaborTypeDataModelView> { labor } };
			Collection<ValidationMessage> validations = new Collection<ValidationMessage>();
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>());
			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { new FullBoe(boe) });
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(workspace.Id)).Returns(new Collection<WorkspaceVariableDTO> { });
			this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO>());
			this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(It.IsAny<int>(), false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);

			sut.ValidateMOQEquation(boe.Id, task, validations, ws);
			Assert.IsTrue(validations.Count == 0, "validation errors occured");
		}

		/// <summary>
		/// Test ValidateMOQEquation with invalid MOQ
		/// </summary>
		[TestMethod]
		public void Test_ValidateMOQEquation_InvalidMOQ()
		{
			BOELaborControllerLogic sut = CreateSystem();

			//test
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2 };
			BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = workspace.Id };
			FullWorkspace ws = new FullWorkspace(workspace);

			TaskElementDetailModelView taskDetail = new TaskElementDetailModelView { TaskElementDetailID = 3, BOEID = boe.Id, MOQHoursEquation = "(200*3+4" };
			LaborSpreadDataModelView spread = new LaborSpreadDataModelView { LaborSpreadDate = "12/01/2012", LaborSpreadValue = 604 };
			LaborTypeDataModelView labor = new LaborTypeDataModelView
			{
				BOELaborTypeID = 3,
				BOETaskElementID = taskDetail.TaskElementDetailID,
				ResourceID = this.Resource.Id,
				PerformingOrgID = this.Perforg.Id,
				ElementOfCost = (int)ElementOfCostType.LMLabor,
				SpreadCurveID = SpreadCurves.SpreadCurve1,
				HourSpread = 604,
				HourSpreadLocked = true,
				PercentSpread = 100m,
				PercentSpreadLocked = false,
				Spreads = new Collection<LaborSpreadDataModelView>() { spread }
			};
			LaborTaskDataModelView task = new LaborTaskDataModelView { TaskElementData = taskDetail, LaborTypesData = new Collection<LaborTypeDataModelView> { labor } };
			Collection<ValidationMessage> validations = new Collection<ValidationMessage>();

			sut.ValidateMOQEquation(boe.Id, task, validations, ws);
			Assert.IsTrue(validations.Count == 1, " no validation errors occured");
		}

		[TestMethod]
		public void Test_ValidateMOQEquation_WithVariables()
		{
			BOELaborControllerLogic sut = CreateSystem();

			//test
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2 };
			BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = workspace.Id };

			Collection<ValidationMessage> validations = new Collection<ValidationMessage>();

			// test with a task and workspace variable
			WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO { WorkspaceID = 7, ValueType = VarValueType.Discrete, WorkspaceVariableValue = 300, WorkspaceVariableName = "TestWorkspaceVar" };
			BoeTaskOrdinaryVariableModelView taskVar = new BoeTaskOrdinaryVariableModelView { OrdinaryVariableID = 8, OrdinaryVariableName = "TestTaskVar", OrdinaryVariableValueType = VarValueType.Discrete, OrdinaryVariableValue = "600", ResourceTypes = new Collection<int> { this.Resource.Id } };
			TaskElementDetailModelView taskDetail2 = new TaskElementDetailModelView { TaskElementDetailID = 3, BOEID = boe.Id, MOQHoursEquation = "(200*3)+4 + TestTaskVar + TestWorkspaceVar", WorkspaceVariableIDs = new Collection<int> { workspaceVar.Id }, TaskOrdinaryVariables = new Collection<BoeTaskOrdinaryVariableModelView> { taskVar } };
			LaborSpreadDataModelView spread2 = new LaborSpreadDataModelView { LaborSpreadDate = "12/01/2012", LaborSpreadValue = 1504 };
			LaborTypeDataModelView labor2 = new LaborTypeDataModelView
			{
				BOELaborTypeID = 3,
				BOETaskElementID = taskDetail2.TaskElementDetailID,
				ResourceID = this.Resource.Id,
				PerformingOrgID = this.Perforg.Id,
				ElementOfCost = (int)ElementOfCostType.LMLabor,
				SpreadCurveID = SpreadCurves.SpreadCurve1,
				HourSpread = 1504,
				HourSpreadLocked = true,
				PercentSpread = 100m,
				PercentSpreadLocked = false,
				Spreads = new Collection<LaborSpreadDataModelView>() { spread2 }
			};
			LaborTaskDataModelView task2 = new LaborTaskDataModelView { TaskElementData = taskDetail2, LaborTypesData = new Collection<LaborTypeDataModelView> { labor2 } };

			_WorkspaceVarLoader.Setup(x => x.GetByWorkspaceID(workspace.Id)).Returns(new Collection<WorkspaceVariableDTO> { workspaceVar });

			BoeTaskElementDTO boeTask = new BoeTaskElementDTO { Id = taskDetail2.TaskElementDetailID.Value, OrdinaryVariables = new Collection<OrdinaryVariableDto> { new OrdinaryVariableDto { Id = taskVar.OrdinaryVariableID, OrdinaryVariableName = taskVar.OrdinaryVariableName, BoeID = boe.Id } }, BoeID = boe.Id };
			factory.Setup(x => x.CreateTaskElement(boeTask.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTask);
			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { new FullBoe(boe) });
			this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(workspace.Id)).Returns(new Collection<WorkspaceVariableDTO> { workspaceVar });
			this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(workspace.Id)).Returns(new Collection<FullWbs>());
			this.retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>());
			this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO>());
			this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(It.IsAny<int>(), false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IResourceDTODataLoader), _ResourceLoader.Object);
			Mock<IPermissionsDTODataLoader> permLoader = new Mock<IPermissionsDTODataLoader>();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permLoader.Object);

			FullWorkspace ws = new FullWorkspace(workspace);

			sut.ValidateMOQEquation(boe.Id, task2, validations, ws);
			Assert.IsTrue(validations.Count == 0, "validation errors occured");
		}


		[TestMethod]
		public void Test_ValidateLaborTaskData_ValidTask()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016") });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.IsFalse(validations.Any(), "There were validation errors");
		}

		[TestMethod]
		public void Test_ValidateLaborTaskData_MissingResourceId()
		{
			BOELaborControllerLogic sut = CreateSystem();

			//test
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016") });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			task.LaborTypesData.First().ResourceID = null;

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(1, validations.Count());
		}

		[TestMethod]
		public void Test_ValidateLaborTaskData_MissingPerfOrgId()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016") });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			task.LaborTypesData.First().PerformingOrgID = null;

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(1, validations.Count());
		}

		[TestMethod]
		public void Test_ValidateLaborTaskData_MissingSpreadCurveId()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016") });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.LaborTypesData.First().SpreadCurveID = null;

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(1, validations.Count());

			// Also test for it being set to none
			task.LaborTypesData.First().SpreadCurveID = SpreadCurves.None;

			validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(1, validations.Count());
		}

		[TestMethod]
		public void Test_ValidateLaborTaskData_MissingPercentSpread()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016") });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			task.LaborTypesData.First().PercentSpread = null;

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(1, validations.Count());
		}

		[TestMethod]
		public void Test_ValidateLaborTaskData_MissingHoursSpread()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016") });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			task.LaborTypesData.First().HourSpread = null;

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(2, validations.Count());
		}

		[TestMethod]
		public void Test_ValidateLaborTaskData_MissingStartDate()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016") });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.LaborTypesData.First().StartDate = null;

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(2, validations.Count());
		}

		[TestMethod]
		public void Test_ValidateLaborTaskData_MissingEndDate()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016") });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			task.LaborTypesData.First().EndDate = null;

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(1, validations.Count());
		}

		[TestMethod]
		public void Test_ValidateLaborTaskData_MissingMultiWbsClin()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			task.LaborTypesData.First().WBSID = null;
			task.LaborTypesData.First().CLINID = null;

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(1, validations.Count());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void Test_ValidateLaborTaskData_ArgumentNull()
		{
			BOELaborControllerLogic sut = CreateSystem();
			sut.ValidateLaborTaskDataWithDataModification(null, null);
		}

		[TestMethod]
		public void TestValidateTaskDetails_Valid()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.IsFalse(results.Any());
		}

		[TestMethod]
		public void TestValidateTaskDetails_LongTaskDesc()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, RteSizeLimit = 50, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.TaskElementData.TaskDescription = "This is a long sentence that goes over the rte size limit of 50 characters";
			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_LongMoqText()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, RteSizeLimit = 50, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.TaskElementData.MOQText = "This is a long sentence that goes over the rte size limit of 50 characters";
			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_TaskStartDateBeforeBoe()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.TaskElementData.StartDate = boe.StartDate.AddMonths(-1).ToMonthString();
			task.TaskElementData.TaskElementDetailID = -1;
			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_TaskEndDateAfterBoe()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.TaskElementData.EndDate = boe.EndDate.AddMonths(1).ToMonthString();
			task.TaskElementData.TaskElementDetailID = -1;
			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_TaskStartDateAfterEndDate()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2018"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.TaskElementData.StartDate = Convert.ToDateTime(task.TaskElementData.EndDate).AddMonths(1).ToMonthString();
			task.TaskElementData.TaskElementDetailID = -1;
			task.LaborTypesData = new Collection<LaborTypeDataModelView>(); // Clear to avoid issues with resource type dates
			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_LaborStartDateBeforeBoe()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.LaborTypesData.First().StartDate = boe.StartDate.AddMonths(-1).ToMonthString();

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_LaborEndDateAfterBoe()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.LaborTypesData.First().EndDate = boe.EndDate.AddMonths(1).ToMonthString();

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_LaborStartDateBeforeBoe_NoEdit()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = false });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.LaborTypesData = new Collection<LaborTypeDataModelView>() { new LaborTypeDataModelView { StartDate = boe.StartDate.AddMonths(-1).ToMonthString(), EndDate = boe.EndDate.ToMonthString() } };

			BoeTaskElementDTO boeTask = new BoeTaskElementDTO
			{
				Id = task.TaskElementData.TaskElementDetailID.Value,
				BoeID = boe.Id,
				BOETaskID = "2",
				StartDate = boe.StartDate,
				EndDate = boe.EndDate,
				taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto() { StartDateValue = boe.StartDate.AddMonths(-1), EndDateValue = boe.EndDate } }
			};

			factory.Setup(x => x.CreateTaskElement(boeTask.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTask);
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<CustomFieldDTO>());
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>());

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_LaborEndDateAfterBoe_NoEdit()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = false });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.LaborTypesData = new Collection<LaborTypeDataModelView>() { new LaborTypeDataModelView { StartDate = boe.StartDate.ToMonthString(), EndDate = boe.EndDate.AddMonths(1).ToMonthString() } };

			BoeTaskElementDTO boeTask = new BoeTaskElementDTO
			{
				Id = task.TaskElementData.TaskElementDetailID.Value,
				BoeID = boe.Id,
				BOETaskID = "2",
				StartDate = boe.StartDate,
				EndDate = boe.EndDate,
				taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto() { StartDateValue = boe.StartDate, EndDateValue = boe.EndDate.AddMonths(1) } }
			};

			factory.Setup(x => x.CreateTaskElement(boeTask.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTask);
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<CustomFieldDTO>());
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>());

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_MissingRequiredLaborCF_OpenEnded()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			CustomFieldDTO laborCustomField = new CustomFieldDTO { Id = 2, CustomFieldName = "LaborColor", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = true, IsOpenEnded = true };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<CustomFieldDTO> { laborCustomField });
			Collection<CustomFieldValueDTO> laborColorOptions = new Collection<CustomFieldValueDTO>();
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(new Collection<int> { laborCustomField.Id }, It.IsAny<int>())).Returns(laborColorOptions);

			task.LaborTypesData.First().CustomFieldValues.Add(new CustomFieldSelectionModelView() { IsOpenEnded = true, CustomFieldID = laborCustomField.Id, OpenEndedValue = string.Empty });

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_MissingRequiredLaborCF_Standard()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.LaborTypesData.First().CustomFieldValues = new Collection<CustomFieldSelectionModelView>();

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_MissingRequiredLaborCF_Standard_NoEdit()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.LaborTypesData = new Collection<LaborTypeDataModelView>();

			BoeTaskElementDTO boeTask = new BoeTaskElementDTO
			{
				Id = task.TaskElementData.TaskElementDetailID.Value,
				BoeID = boe.Id,
				BOETaskID = "2",
				StartDate = boe.StartDate,
				EndDate = boe.EndDate,
				taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto() { StartDateValue = boe.StartDate, EndDateValue = boe.EndDate } }
			};

			factory.Setup(x => x.CreateTaskElement(boeTask.Id, It.IsAny<int>(), It.IsAny<int>())).Returns(boeTask);

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_MissingRequiredTaskCF_Standard()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			task.TaskElementData.CustomFieldValues = new Collection<CustomFieldSelectionModelView>();

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count());
		}

		[TestMethod]
		public void TestValidateTaskDetails_MissingRequiredTaskCF_OpenEnded()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			CustomFieldDTO taskCustomField = new CustomFieldDTO { Id = 2, CustomFieldName = "TaskColor", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.TaskDisplay, CustomFieldRequired = true, IsOpenEnded = true };
			Collection<CustomFieldValueDTO> colorOptions = new Collection<CustomFieldValueDTO>();
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<CustomFieldDTO> { taskCustomField });
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(new Collection<int> { taskCustomField.Id }, It.IsAny<int>())).Returns(colorOptions);

			task.TaskElementData.CustomFieldValues.Add(new CustomFieldSelectionModelView() { IsOpenEnded = true, CustomFieldID = taskCustomField.Id, OpenEndedValue = string.Empty });

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count);
		}

		/// <summary>
		/// Test ValidateTaskDetails for a missing required MOQ Type Table Custom Field
		/// </summary>
		[TestMethod]
		public void TestValidateTaskDetails_MissingRequiredMoqTypeTableCF()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			CustomFieldDTO moqTypeTableCustomField = new CustomFieldDTO { Id = 2, CustomFieldName = "MoqColor", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.MoqTypeTableDataDisplay, CustomFieldRequired = true, IsOpenEnded = true };
			Collection<CustomFieldValueDTO> colorOptions = new Collection<CustomFieldValueDTO>();
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<CustomFieldDTO> { moqTypeTableCustomField });
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(new Collection<int> { moqTypeTableCustomField.Id }, It.IsAny<int>())).Returns(colorOptions);

			task.MOQTypes.First().TableData.First().CustomFieldValueContainers.Add(new CustomFieldValueContainer()
			{
				Id = 1,
				CustomFieldID = moqTypeTableCustomField.Id,
				IsOpenEnded = true,
				OpenEndedValue = string.Empty
			});

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(1, results.Count);
		}

		/// <summary>
		/// Test Custom Fields Validation for MOQ Type Tables when Repo is set to SAP/WEBI
		/// </summary>
		[TestMethod]
		public void TestValidateTaskDetails_SAPWEBI_REPO_ValueInMoqTypeTableCF()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			CustomFieldDTO moqTypeTableCustomField = new CustomFieldDTO { Id = 2, CustomFieldName = "MoqColor", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.MoqTypeTableDataDisplay, CustomFieldRequired = true, IsOpenEnded = true };
			Collection<CustomFieldValueDTO> colorOptions = new Collection<CustomFieldValueDTO>();
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<CustomFieldDTO> { moqTypeTableCustomField });
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(new Collection<int> { moqTypeTableCustomField.Id }, It.IsAny<int>())).Returns(colorOptions);

			task.MOQTypes.First().TableData.First().RepositoryName = RepositoryName.SapWebi.GetDescription();

			task.MOQTypes.First().TableData.First().CustomFieldValueContainers.Add(new CustomFieldValueContainer()
			{
				Id = 1,
				CustomFieldID = moqTypeTableCustomField.Id,
				IsOpenEnded = true,
				OpenEndedValue = string.Empty
			});

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, ws);
			Assert.AreEqual(0, results.Count);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestValidateTaskDetails_ExBoe()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(null, task, results, ws);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestValidateTaskDetails_ExTask()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, null, results, ws);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestValidateTaskDetails_ExValidation()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			sut.ValidateTaskDetails(boe, task, null, ws);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void TestValidateTaskDetails_ExWs()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			ICollection<ValidationMessage> results = new Collection<ValidationMessage>();

			sut.ValidateTaskDetails(boe, task, results, null);
		}

		[TestMethod]
		public void TestValidateLaborTaskData_NullVariable()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = false });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.TaskElementData.TaskOrdinaryVariables.First().OrdinaryVariableValue = null;

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(3, validations.Count());
		}

		[TestMethod]
		public void TestValidateLaborTaskData_InvalidPerfOrg()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = false });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.LaborTypesData.First().PerformingOrgID = -1;

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(1, validations.Count());
		}

		[TestMethod]
		public void TestValidateLaborTaskData_InvalidResource()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = false });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			task.LaborTypesData.First().ResourceID = -1;

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(1, validations.Count());
		}

		[TestMethod]
		public void TestValidateLaborTaskData_UnqualMoqLaborSpreads()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = false });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			// Make MOQ not equal Labor Spread
			task.TaskElementData.MOQHoursEquation = (task.LaborTypesData.First().HourSpread - 1).ToString();

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(1, validations.Count());
		}

		[TestMethod]
		public void TestValidateLaborTaskData_UnqualLaborSpreadsResourceHourSpreads()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = false });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			task.LaborTypesData.First().Spreads.First().LaborSpreadValue -= 1;

			ICollection<ValidationMessage> validations = sut.ValidateLaborTaskDataWithDataModification(ws, task);
			Assert.AreEqual(1, validations.Count());
		}

		/// <summary>
		/// Test ValidateLockedLaborTaskData returns no errors when no changes made
		/// </summary>
		[TestMethod]
		public void TestValidateLockedLaborTaskData()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullWorkspace ws = new FullWorkspace(workspace);

			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2012"), IsMultiClinWbs = false });

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			BoeTaskElementDTO originalTask = CreateDto(boe, ws, sut);

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			this.retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, false, workspace.DecimalPrecision, workspace.CostDecimalPrecision)).Returns(new Collection<BoeTaskElementDTO>() { originalTask });
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(ws);

			ICollection<ValidationMessage> result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsFalse(result.Any());
		}

		/// <summary>
		/// Test ValidateLockedLaborTaskData returns proper error when Task ID is changed
		/// </summary>
		[TestMethod]
		public void TestValidateLockedLaborTaskData_TaskId()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullWorkspace ws = new FullWorkspace(workspace);

			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2012"), IsMultiClinWbs = false });

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			BoeTaskElementDTO originalTask = CreateDto(boe, ws, sut);

			// Change Task ID
			task.TaskElementData.TaskID = "2";

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			this.retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, false, workspace.DecimalPrecision, workspace.CostDecimalPrecision)).Returns(new Collection<BoeTaskElementDTO>() { originalTask });
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(ws);

			ICollection<ValidationMessage> result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains("Task ID"));
		}

		/// <summary>
		/// Test ValidateLockedLaborTaskData returns proper error when Task Title is changed
		/// </summary>
		[TestMethod]
		public void TestValidateLockedLaborTaskData_TaskTitle()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullWorkspace ws = new FullWorkspace(workspace);

			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2012"), IsMultiClinWbs = false });

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			BoeTaskElementDTO originalTask = CreateDto(boe, ws, sut);

			// Change Task Title
			task.TaskElementData.Title = "Wrong title";

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			this.retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, false, workspace.DecimalPrecision, workspace.CostDecimalPrecision)).Returns(new Collection<BoeTaskElementDTO>() { originalTask });
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(ws);

			ICollection<ValidationMessage> result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains("Task Title"));
		}


		/// <summary>
		/// Test ValidateLockedLaborTaskData returns proper error when Task dates are changed
		/// </summary>
		[TestMethod]
		public void TestValidateLockedLaborTaskData_TaskDates()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullWorkspace ws = new FullWorkspace(workspace);

			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2012"), IsMultiClinWbs = false });

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			BoeTaskElementDTO originalTask = CreateDto(boe, ws, sut);

			// Change Task Start Date
			task.TaskElementData.StartDate = DateTime.Now.AddDays(1).ToMonthString();

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			this.retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, false, workspace.DecimalPrecision, workspace.CostDecimalPrecision)).Returns(new Collection<BoeTaskElementDTO>() { originalTask });
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(ws);

			ICollection<ValidationMessage> result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains("Task Start and End Dates"));

			// Change Task Start Date
			task.TaskElementData.StartDate = DateTime.Now.ToMonthString();
			task.TaskElementData.EndDate = DateTime.Now.AddDays(1).ToMonthString();

			result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains("Task Start and End Dates"));
		}

		/// <summary>
		/// Test ValidateLockedLaborTaskData returns proper error when the MOQ Equation is changed
		/// </summary>
		[TestMethod]
		public void TestValidateLockedLaborTaskData_Moq()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullWorkspace ws = new FullWorkspace(workspace);

			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2012"), IsMultiClinWbs = false });

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			BoeTaskElementDTO originalTask = CreateDto(boe, ws, sut);

			// Change MOQ text
			task.TaskElementData.MOQHoursEquation = "Wrong text";

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			this.retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, false, workspace.DecimalPrecision, workspace.CostDecimalPrecision)).Returns(new Collection<BoeTaskElementDTO>() { originalTask });
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(ws);

			ICollection<ValidationMessage> result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains("MOQ Equation"));
		}

		/// <summary>
		/// Test ValidateLockedLaborTaskData returns proper error when Labor fields are changed
		/// </summary>
		[TestMethod]
		public void TestValidateLockedLaborTaskData_Labor()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullWorkspace ws = new FullWorkspace(workspace);

			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2012"), IsMultiClinWbs = false });

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			BoeTaskElementDTO originalTask = CreateDto(boe, ws, sut);

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			this.retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, false, workspace.DecimalPrecision, workspace.CostDecimalPrecision)).Returns(new Collection<BoeTaskElementDTO>() { originalTask });
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(ws);

			// Make a copy of the MV so values can easily be reverted. Testing one field at a time.
			LaborTaskDataModelView unchangedTask = new LaborTaskDataModelView() { TaskElementData = task.TaskElementData, LaborTypesData = task.LaborTypesData };

			// Change Labor Resource ID
			task.LaborTypesData.First().ResourceID = unchangedTask.LaborTypesData.First().ResourceID + 1;

			ICollection<ValidationMessage> result = sut.ValidateLockedLaborTaskData(ws, task);

			string validationContainsString = "Resource Types";

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains(validationContainsString));

			// Change Labor Perf Org ID
			task.LaborTypesData.First().PerformingOrgID = unchangedTask.LaborTypesData.First().PerformingOrgID + 1;
			task.LaborTypesData.First().ResourceID = unchangedTask.LaborTypesData.First().ResourceID;

			result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains(validationContainsString));

			// Change Labor Start Date
			task.LaborTypesData.First().StartDate = DateTime.Now.AddDays(1).ToMonthString();
			task.LaborTypesData.First().PerformingOrgID = unchangedTask.LaborTypesData.First().PerformingOrgID;

			result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains(validationContainsString));

			// Change Labor End Date
			task.LaborTypesData.First().EndDate = DateTime.Now.AddDays(1).ToMonthString();
			task.LaborTypesData.First().StartDate = unchangedTask.LaborTypesData.First().StartDate;

			result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains(validationContainsString));

			// Change Labor Spread Curve
			task.LaborTypesData.First().SpreadCurveID = unchangedTask.LaborTypesData.First().SpreadCurveID + 1;
			task.LaborTypesData.First().EndDate = unchangedTask.LaborTypesData.First().EndDate;

			result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains(validationContainsString));

			// Change Labor Percent Spread
			task.LaborTypesData.First().PercentSpread = unchangedTask.LaborTypesData.First().PercentSpread + 1;
			task.LaborTypesData.First().SpreadCurveID = unchangedTask.LaborTypesData.First().SpreadCurveID;

			result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains(validationContainsString));

			// Change Labor hours spread value
			task.LaborTypesData.First().HourSpread = unchangedTask.LaborTypesData.First().HourSpread + 1;
			task.LaborTypesData.First().PercentSpread = unchangedTask.LaborTypesData.First().PercentSpread;

			result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains(validationContainsString));

			// Change Labor cost spread value
			task.LaborTypesData.First().CostSpread = unchangedTask.LaborTypesData.First().CostSpread + 1;
			task.LaborTypesData.First().HourSpread = unchangedTask.LaborTypesData.First().HourSpread;
			originalTask.taskElementLabors.First().SpreadType = SpreadType.Cost;
			this.retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, false, workspace.DecimalPrecision, workspace.CostDecimalPrecision)).Returns(new Collection<BoeTaskElementDTO>() { originalTask });

			result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains(validationContainsString));
		}

		/// <summary>
		/// Test ValidateLockedLaborTaskData returns proper error when Spread Values or Dates are changed
		/// </summary>
		[TestMethod]
		public void TestValidateLockedLaborTaskData_Spread()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullWorkspace ws = new FullWorkspace(workspace);

			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2012"), IsMultiClinWbs = false });

			LaborTaskDataModelView task = CreateModelView(boe, ws);
			BoeTaskElementDTO originalTask = CreateDto(boe, ws, sut);

			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			this.retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, false, workspace.DecimalPrecision, workspace.CostDecimalPrecision)).Returns(new Collection<BoeTaskElementDTO>() { originalTask });
			this.retriever.Setup(x => x.GetFullWorkspaceById(boe.WorkspaceID)).Returns(ws);

			// Change Spread Value
			decimal? originalValue = task.LaborTypesData.First().Spreads.First().LaborSpreadValue;
			task.LaborTypesData.First().Spreads.First().LaborSpreadValue = originalValue + 1;

			ICollection<ValidationMessage> result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains("Resource Spreads"));

			// Change Spread Date
			task.LaborTypesData.First().Spreads.First().LaborSpreadDate = DateTime.Now.AddDays(1).ToMonthString();
			task.LaborTypesData.First().Spreads.First().LaborSpreadValue = originalValue;

			result = sut.ValidateLockedLaborTaskData(ws, task);

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.First().ValidationIssue.Contains("Resource Spreads"));
		}

		[TestMethod]
		public void Test_ConvertModelViewToDto()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView task = CreateModelView(boe, ws);

			BoeTaskElementDTO result = sut.ConvertModelViewToDto(task, ws);

			// Assert Task Element
			Assert.AreEqual(task.TaskElementData.TaskElementDetailID, result.Id);
			Assert.AreEqual(task.TaskElementData.BOEID, result.BoeID);
			Assert.AreEqual(task.TaskElementData.TaskID, result.BOETaskID);
			Assert.AreEqual(task.TaskElementData.Title, result.TaskTitle);
			Assert.AreEqual(task.TaskElementData.TaskDescription, result.Description);
			Assert.AreEqual(task.TaskElementData.MOQType, result.MOQType);
			Assert.AreEqual(task.TaskElementData.MOQText, result.MOQText);
			Assert.AreEqual("(200 * 3) + 4 + TestTaskVar + <WSVAR:-1>", result.MOQHoursEquation);
			Assert.AreEqual(task.TaskElementData.UpdateDate, result.UpdateDate);
			Assert.AreEqual(task.TaskElementData.LaborTypeWarning, result.LaborTypeWarningFlag);
			Assert.AreEqual(task.TaskElementData.WorkspaceVariableIDs, result.WorkspaceVariableIDs);
			Assert.AreEqual(task.TaskElementData.StartDate, result.StartDate.Value.ToString("MM/yyyy"));
			Assert.AreEqual(task.TaskElementData.EndDate, result.EndDate.Value.ToString("MM/yyyy"));

			// Assert Variables
			Assert.IsTrue(result.OrdinaryVariables.Any());
			Assert.AreEqual(task.TaskElementData.TaskOrdinaryVariables.Count(), result.OrdinaryVariables.Count());
			BoeTaskOrdinaryVariableModelView expectedVar = task.TaskElementData.TaskOrdinaryVariables.First();
			OrdinaryVariableDto resultVar = result.OrdinaryVariables.First();
			Assert.AreEqual(expectedVar.OrdinaryVariableID, resultVar.Id);
			Assert.AreEqual(boe.Id, resultVar.BoeID);
			Assert.AreEqual(expectedVar.DefaultSize, resultVar.DefaultSize);
			Assert.AreEqual(expectedVar.IsPercentage, resultVar.IsPercentage);
			Assert.AreEqual(expectedVar.OrdinaryVariableName, resultVar.OrdinaryVariableName);
			Assert.AreEqual(expectedVar.OrdinaryVariableValue, resultVar.OrdinaryVariableValue.Value.ToString());
			Assert.AreEqual(expectedVar.OrdinaryVariableValueType, resultVar.ValueType);
			Assert.AreEqual(expectedVar.SortBOEBy, resultVar.SortBOEBy);
			Assert.AreEqual(expectedVar.ResourceTypes.Count, resultVar.SumVariableResourceTypeIDs.Count);
			// TaskElementId is set during the save (propagate Id) of the BoeTaskElement, not during conversion
			// Assert.AreEqual(task.TaskElementData.TaskElementDetailID, resultVar.TaskElementId);
			Assert.AreEqual(UpdateType.Upsert, resultVar.Updateable);
			Assert.AreEqual(expectedVar.UpdateDate, resultVar.UpdateDate);
			Assert.AreEqual(expectedVar.UpdateDateLong, resultVar.UpdateDateLong);
			Assert.AreEqual(expectedVar.BOEToSum.Count(), resultVar.SelectedBOEsToSum.Count());

			// Assert Task Custom Fields
			Assert.IsTrue(result.CustomFieldValueContainers.Any());
			CustomFieldSelectionModelView expectedCf = task.TaskElementData.CustomFieldValues.First();
			CustomFieldValueContainer resultCf = result.CustomFieldValueContainers.First();
			Assert.AreEqual(expectedCf.CustomFieldValueID, resultCf.CustomFieldValueID);
			Assert.AreEqual(expectedCf.SelectionID, resultCf.ContainerID);
			Assert.AreEqual(expectedCf.CustomFieldID, resultCf.CustomFieldID);
			Assert.AreEqual(expectedCf.IsOpenEnded, resultCf.IsOpenEnded);
			Assert.AreEqual(expectedCf.OpenEndedValue, resultCf.OpenEndedValue);
			Assert.AreEqual(expectedCf.UpdateDate, resultCf.UpdateDate);
			Assert.AreEqual(UpdateType.Upsert, resultCf.Updateable);

			// Assert Labors
			Assert.IsTrue(result.taskElementLabors.Any());
			for (int i = 0; i < task.LaborTypesData.Count(); i++)
			{
				LaborTypeDataModelView expectedLabor = task.LaborTypesData.ElementAt(i);
				ResourceTypeDto resultLabor = result.taskElementLabors.Where(x => x.Id == expectedLabor.BOELaborTypeID).First();

				Assert.AreEqual(expectedLabor.BOELaborTypeID, resultLabor.Id);
				if (resultLabor.Updateable == UpdateType.Upsert)
				{
					Assert.AreEqual(expectedLabor.ResourceID, resultLabor.ResourceID);
					Assert.AreEqual(expectedLabor.PerformingOrgID, resultLabor.PerformingOrgID);
					Assert.AreEqual(expectedLabor.SpreadCurveID, resultLabor.SpreadCurveID);
					Assert.AreEqual(expectedLabor.PercentSpread, resultLabor.PercentSpread);
					Assert.AreEqual(expectedLabor.CanOffload, resultLabor.CanOffload);
					Assert.AreEqual(expectedLabor.TieredPercentage, resultLabor.TieredPercentage);
					Assert.AreEqual(expectedLabor.RateType == RateType.Hours ? SpreadType.Hours : SpreadType.Cost, resultLabor.SpreadType);
					Assert.AreEqual(expectedLabor.RateType == RateType.Hours ? expectedLabor.HourSpread : expectedLabor.CostSpread, resultLabor.ValueSpread);
					Assert.AreEqual(expectedLabor.UpdateDate, resultLabor.UpdateDate);
					Assert.AreEqual(expectedLabor.PercentSpreadLocked, resultLabor.PercentSpreadLocked);
					Assert.AreEqual(expectedLabor.HourSpreadLocked, resultLabor.HourSpreadLocked);
					Assert.AreEqual(expectedLabor.WBSID > 0 ? expectedLabor.WBSID : null, resultLabor.WBSID);
					Assert.AreEqual(expectedLabor.CLINID > 0 ? expectedLabor.CLINID : null, resultLabor.CLINID);
					Assert.AreEqual(UpdateType.Upsert, resultLabor.Updateable);
					Assert.AreEqual(expectedLabor.StartDate, resultLabor.StartDate.Value.ToString("MM/yyyy"));
					Assert.AreEqual(expectedLabor.EndDate, resultLabor.EndDate.Value.ToString("MM/yyyy"));
					Assert.AreEqual(expectedLabor.LaborTypeOrder, resultLabor.LaborTypeOrder);

					// Assert Labor Custom Fields
					Assert.IsTrue(resultLabor.CustomFieldValueContainers.Any());
					Assert.AreEqual(expectedLabor.CustomFieldValues.First().CustomFieldValueID, resultLabor.CustomFieldValueContainers.First().CustomFieldValueID);
					Assert.AreEqual(expectedLabor.CustomFieldValues.First().SelectionID, resultLabor.CustomFieldValueContainers.First().ContainerID);
					Assert.AreEqual(expectedLabor.CustomFieldValues.First().CustomFieldID, resultLabor.CustomFieldValueContainers.First().CustomFieldID);
					Assert.AreEqual(expectedLabor.CustomFieldValues.First().IsOpenEnded, resultLabor.CustomFieldValueContainers.First().IsOpenEnded);
					Assert.AreEqual(expectedLabor.CustomFieldValues.First().OpenEndedValue, resultLabor.CustomFieldValueContainers.First().OpenEndedValue);
					Assert.AreEqual(expectedLabor.CustomFieldValues.First().UpdateDate, resultLabor.CustomFieldValueContainers.First().UpdateDate);
					Assert.AreEqual(UpdateType.Upsert, resultLabor.CustomFieldValueContainers.First().Updateable);

					// Assert Spreads
					Assert.IsTrue(resultLabor.LaborSpreads.Any());
					for (int j = 0; j < resultLabor.LaborSpreads.Count(); j++)
					{
						LaborSpreadDataModelView expectedSpread = expectedLabor.Spreads.ElementAt(j);
						ResourceSpreadDto resultSpread = resultLabor.LaborSpreads.ElementAt(j);

						Assert.AreEqual(expectedSpread.LaborSpreadDate, resultSpread.LaborSpreadDate.ToString("MM/yyyy"));
						Assert.AreEqual(expectedSpread.LaborSpreadValue, resultSpread.LaborSpreadValue);
						Assert.AreEqual(expectedSpread.UpdateDate, resultSpread.UpdateDate);
						Assert.AreEqual(expectedSpread.UpdateDateLong, resultSpread.UpdateDateLong);
						Assert.AreEqual(UpdateType.Upsert, resultSpread.Updateable);
					}
				}
			}
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void Test_ConvertModelViewToDto_EX()
		{
			BOELaborControllerLogic sut = CreateSystem();
			sut.ConvertModelViewToDto(null, null);
		}

		[TestMethod]
		public void Test_ValidateTaskElementDto()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			BoeTaskElementDTO task = CreateDto(boe, ws, sut);

			circularReferenceChecker.Setup(x => x.OrdinaryVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<OrdinaryVariableDto>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<OrdinaryVariableDto>());
			circularReferenceChecker.Setup(x => x.WorkspaceVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<WorkspaceVariableDTO>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<WorkspaceVariableDTO>());
			_ResourceLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<ResourceDTO>());

			ICollection<ValidationMessage> result = sut.ValidateTaskElementDto(ws, task, new Collection<MoqTypeSelection>());

			Assert.IsFalse(result.Any());
		}

		[TestMethod]
		public void Test_ValidateTaskElementDto_InvalidOrdinaryVariable()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			BoeTaskElementDTO task = CreateDto(boe, ws, sut);

			circularReferenceChecker.Setup(x => x.OrdinaryVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<OrdinaryVariableDto>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<OrdinaryVariableDto>() { new OrdinaryVariableDto() { OrdinaryVariableName = "Test" } });
			circularReferenceChecker.Setup(x => x.WorkspaceVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<WorkspaceVariableDTO>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<WorkspaceVariableDTO>());
			_ResourceLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<ResourceDTO>());

			ICollection<ValidationMessage> result = sut.ValidateTaskElementDto(ws, task, new Collection<MoqTypeSelection>());

			Assert.AreEqual(1, result.Count());
		}

		[TestMethod]
		public void Test_ValidateTaskElementDto_InvalidWorkspaceVariable()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			BoeTaskElementDTO task = CreateDto(boe, ws, sut);

			circularReferenceChecker.Setup(x => x.OrdinaryVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<OrdinaryVariableDto>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<OrdinaryVariableDto>());
			circularReferenceChecker.Setup(x => x.WorkspaceVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<WorkspaceVariableDTO>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<WorkspaceVariableDTO>() { new WorkspaceVariableDTO() { WorkspaceVariableName = "Test" } });
			_ResourceLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<ResourceDTO>());

			ICollection<ValidationMessage> result = sut.ValidateTaskElementDto(ws, task, new Collection<MoqTypeSelection>());

			Assert.AreEqual(1, result.Count());
		}

		[TestMethod]
		public void Test_ValidateTaskElementDto_InvalidSpreadStartDate()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			BoeTaskElementDTO task = CreateDto(boe, ws, sut);

			circularReferenceChecker.Setup(x => x.OrdinaryVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<OrdinaryVariableDto>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<OrdinaryVariableDto>());
			circularReferenceChecker.Setup(x => x.WorkspaceVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<WorkspaceVariableDTO>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<WorkspaceVariableDTO>());
			_ResourceLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<ResourceDTO>());

			task.taskElementLabors.First().LaborSpreads.First().LaborSpreadDate = DateTime.MinValue;

			ICollection<ValidationMessage> result = sut.ValidateTaskElementDto(ws, task, new Collection<MoqTypeSelection>());

			Assert.AreEqual(1, result.Count());
		}

		[TestMethod]
		public void Test_ValidateTaskElementDto_InvalidSpreadEndDate()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			BoeTaskElementDTO task = CreateDto(boe, ws, sut);

			circularReferenceChecker.Setup(x => x.OrdinaryVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<OrdinaryVariableDto>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<OrdinaryVariableDto>());
			circularReferenceChecker.Setup(x => x.WorkspaceVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<WorkspaceVariableDTO>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<WorkspaceVariableDTO>());
			_ResourceLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<ResourceDTO>());

			task.taskElementLabors.First().LaborSpreads.First().LaborSpreadDate = DateTime.MaxValue;

			ICollection<ValidationMessage> result = sut.ValidateTaskElementDto(ws, task, new Collection<MoqTypeSelection>());

			Assert.AreEqual(1, result.Count());
		}

		[TestMethod]
		public void Test_ValidateTaskElementDto_InvalidTaskStartDate()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			BoeTaskElementDTO task = CreateDto(boe, ws, sut);

			circularReferenceChecker.Setup(x => x.OrdinaryVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<OrdinaryVariableDto>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<OrdinaryVariableDto>());
			circularReferenceChecker.Setup(x => x.WorkspaceVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<WorkspaceVariableDTO>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<WorkspaceVariableDTO>());
			_ResourceLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<ResourceDTO>());

			task.StartDate = DateTime.MinValue;

			ICollection<ValidationMessage> result = sut.ValidateTaskElementDto(ws, task, new Collection<MoqTypeSelection>());

			Assert.AreEqual(1, result.Count());
		}

		[TestMethod]
		public void Test_ValidateTaskElementDto_InvalidTaskEndDate()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			BoeTaskElementDTO task = CreateDto(boe, ws, sut);

			circularReferenceChecker.Setup(x => x.OrdinaryVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<OrdinaryVariableDto>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<OrdinaryVariableDto>());
			circularReferenceChecker.Setup(x => x.WorkspaceVariablesCreateCircularReference(It.IsAny<VariableCircularReferenceCheckerCache>(), It.IsAny<int>(), It.IsAny<ICollection<WorkspaceVariableDTO>>(), It.IsAny<FullWorkspace>())).Returns(new Collection<WorkspaceVariableDTO>());
			_ResourceLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<ResourceDTO>());

			task.EndDate = DateTime.MaxValue;

			ICollection<ValidationMessage> result = sut.ValidateTaskElementDto(ws, task, new Collection<MoqTypeSelection>());

			Assert.AreEqual(1, result.Count());
		}

		/// <summary>
		/// Test that ValidateTaskElementDto throws an exception when ws is null 
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void Test_ValidateTaskElementDto_ExWs()
		{
			BOELaborControllerLogic sut = CreateSystem();
			sut.ValidateTaskElementDto(null, new BoeTaskElementDTO(), new Collection<MoqTypeSelection>());
		}

		/// <summary>
		/// Test that ValidateTaskElementDto throws an exception when taskElement is null 
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void Test_ValidateTaskElementDto_ExTaskElement()
		{
			BOELaborControllerLogic sut = CreateSystem();
			sut.ValidateTaskElementDto(new FullWorkspace(), null, new Collection<MoqTypeSelection>());
		}

		/// <summary>
		/// Test that ValidateTaskElementDto throws an exception when moqTypes is null 
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void Test_ValidateTaskElementDto_ExMoqTypes()
		{
			BOELaborControllerLogic sut = CreateSystem();
			sut.ValidateTaskElementDto(new FullWorkspace(), new BoeTaskElementDTO(), null);
		}

		/// <summary>
		/// Test SaveLaborTaskData for an existing task
		/// </summary>
		[TestMethod]
		public void Test_SaveLaborTaskData()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView taskMV = CreateModelView(boe, ws);
			BoeTaskElementDTO task = CreateDto(boe, ws, sut);
			_BoeTaskElementMediator.Setup(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO>() { task }, ws)).Returns(new Dictionary<int, int>() { { task.Id, task.Id } });

			// pass over code that does OtherBOERecalculationsNeeded
			task.TotalHours = null;
			sut.SaveLaborTaskData(ws, task, taskMV.TaskElementData.MetricIds, null, new Collection<MoqTypeSelection>());

			_BoeTaskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO>() { task }, ws), Times.Once());
			_VariableSelectBoeToSum.Verify(x => x.GetWorkspaceVarLabelTotal(It.IsAny<WorkspaceVariableDTO>(), It.IsAny<DataClassForSumOfBOEsCalculation>()), Times.Never());
			_WorkspaceVarLoader.Verify(x => x.SaveWorkspaceVariables(It.IsAny<Collection<WorkspaceVariableDTO>>()), Times.Never());
			moqTypeDataLoader.Verify(x => x.Save(It.IsAny<ICollection<MoqTypeSelection>>()), Times.Never());
		}


		/// <summary>
		/// Test SaveLaborTaskData for a new task
		/// </summary>
		[TestMethod]
		public void Test_SaveLaborTaskData_New()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = false };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView taskMV = CreateModelView(boe, ws);
			BoeTaskElementDTO task = CreateDto(boe, ws, sut);
			task.Id = -1;

			_BoeTaskElementMediator.Setup(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO>() { task }, ws)).Returns(new Dictionary<int, int>() { { task.Id, 1 } });
			_BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(It.IsAny<FullBoe>(), It.IsAny<VariableType>(), It.IsAny<FullWorkspace>(), It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO>());

			sut.SaveLaborTaskData(ws, task, taskMV.TaskElementData.MetricIds, null, new Collection<MoqTypeSelection>());

			_BoeTaskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO>() { task }, ws), Times.Once());
			_VariableSelectBoeToSum.Verify(x => x.GetWorkspaceVarLabelTotal(It.IsAny<WorkspaceVariableDTO>(), It.IsAny<DataClassForSumOfBOEsCalculation>()), Times.Never());
			_WorkspaceVarLoader.Verify(x => x.SaveWorkspaceVariables(It.IsAny<Collection<WorkspaceVariableDTO>>()), Times.Once());
			moqTypeDataLoader.Verify(x => x.Save(It.IsAny<ICollection<MoqTypeSelection>>()), Times.Never());
		}


		/// <summary>
		/// Test SaveLaborTaskData throws an exception when ws is null
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void Test_SaveLaborTaskData_EX()
		{
			BOELaborControllerLogic sut = CreateSystem();
			sut.SaveLaborTaskData(null, new BoeTaskElementDTO(), new Collection<int>(), null, new Collection<MoqTypeSelection>());
		}

		/// <summary>
		/// Test SaveLaborTaskData throws an exception when moqTypes is null
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void Test_SaveLaborTaskData_EX2()
		{
			BOELaborControllerLogic sut = CreateSystem();
			sut.SaveLaborTaskData(new FullWorkspace(), new BoeTaskElementDTO(), new Collection<int>(), null, null);
		}


		/// <summary>
		/// Test SaveLaborTaskData whan saving moq types
		/// </summary>
		[TestMethod]
		public void Test_SaveLaborTaskData_MOQTypes()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2, CostDecimalPrecision = 2, ResourceDecimalPrecision = 3, ResourceListID = 1, UsingTemplateBOE = true };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id, StartDate = Convert.ToDateTime("12/01/2012"), EndDate = Convert.ToDateTime("12/01/2016"), IsMultiClinWbs = true });
			FullWorkspace ws = new FullWorkspace(workspace);

			LaborTaskDataModelView taskMV = CreateModelView(boe, ws);
			BoeTaskElementDTO task = CreateDto(boe, ws, sut);
			_BoeTaskElementMediator.Setup(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO>() { task }, ws)).Returns(new Dictionary<int, int>() { { task.Id, task.Id } });

			MoqTypeSelection moqTypeSelectionToSave = new MoqTypeSelection()
			{
				Id = -1,
				TaskId = task.Id,
				SelectedMOQType = MOQType.Comparative,
				CerName = "test name",
				DescriptionHoursRequired = "test desc",
				SmeReason = "test reason",
				SmeHoursLogic = "test hours logic",
				SmeDurationLogic = "test duration logic",
				SmeTaskEstimates = "test task estimates",
				Rationale = "test rationale",
				SkillMixRationale = "test skill mix",
				HistoricalReferenceExplanation = "test historical reference explanation",
				BoeId = boe.Id
			};

			MoqTableData moqTableData1 = new MoqTableData()
			{
				Id = -1,
				TableName = "test table 1",
				RepositoryName = "test repo 1",
				QueryType = "query type 1",
				DateOfReport = DateTime.Now,
				HistoricalProgramName = "test name 1",
				ContractNumber = "test contract 1",
				WbsElement = "test wbs 1",
				PoPStart = DateTime.Now.AddDays(-1),
				PoPEnd = DateTime.Now.AddDays(1),
				TotalWbsHours = 100,
				AdditionalQueryFilters = "test filters 1",
				TotalRelevantHours = 50,
				CustomFieldValueContainers = new Collection<CustomFieldValueContainer>()
				{
					new CustomFieldValueContainer()
					{
						Id = 1,
						OpenEndedValue = "test 1"
					},
					new CustomFieldValueContainer()
					{
						Id = 2,
						OpenEndedValue = "test 2"
					}
				}
			};

			MoqTableData moqTableData2 = new MoqTableData()
			{
				Id = -1,
				TableName = "test table 2",
				RepositoryName = "test repo 2",
				QueryType = "query type 2",
				DateOfReport = DateTime.Now.AddDays(2),
				HistoricalProgramName = "test name 2",
				ContractNumber = "test contract 2",
				WbsElement = "test wbs 2",
				PoPStart = DateTime.Now.AddDays(-3),
				PoPEnd = DateTime.Now.AddDays(3),
				TotalWbsHours = 200,
				AdditionalQueryFilters = "test filters 2",
				TotalRelevantHours = 150,
				CustomFieldValueContainers = new Collection<CustomFieldValueContainer>()
				{
					new CustomFieldValueContainer()
					{
						Id = 3,
						OpenEndedValue = "test 3"
					},
					new CustomFieldValueContainer()
					{
						Id = 4,
						OpenEndedValue = "test 4"
					}
				}
			};

			moqTypeSelectionToSave.TableData.Add(moqTableData1);
			moqTypeSelectionToSave.TableData.Add(moqTableData2);
			taskMV.MOQTypes = new Collection<MoqTypeSelection>();
			taskMV.MOQTypes.Add(moqTypeSelectionToSave);

			// Get By BOE Id should return the existing MOQ Type plus one not included in the save to be deleted
			moqTypeDataLoader.Setup(x => x.GetByBoeId(boe.Id)).Returns(new Collection<MoqTypeSelection>() { moqTypeSelectionToSave, new MoqTypeSelection() { Id = 2 } });

			// pass over code that does OtherBOERecalculationsNeeded
			task.TotalHours = null;
			sut.SaveLaborTaskData(ws, task, taskMV.TaskElementData.MetricIds, null, taskMV.MOQTypes);

			_BoeTaskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO>() { task }, ws), Times.Once());
			_VariableSelectBoeToSum.Verify(x => x.GetWorkspaceVarLabelTotal(It.IsAny<WorkspaceVariableDTO>(), It.IsAny<DataClassForSumOfBOEsCalculation>()), Times.Never());
			_WorkspaceVarLoader.Verify(x => x.SaveWorkspaceVariables(It.IsAny<Collection<WorkspaceVariableDTO>>()), Times.Never());
			moqTypeDataLoader.Verify(x => x.Save(It.IsAny<ICollection<MoqTypeSelection>>()), Times.Exactly(2)); // Twice - once for delete of existing, once for save of new
		}

		[TestMethod]
		public void Test_GetCustomFieldOptionModelViews_TaskLevel()
		{
			BOELaborControllerLogic sut = CreateSystem();

			//test
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2 };
			FullWorkspace ws = new FullWorkspace(workspace);
			CustomFieldDTO taskCustomField = new CustomFieldDTO { Id = 1, CustomFieldName = "Color", WorkspaceID = workspace.Id, CustomFieldDisplayID = CustomFieldType.TaskDisplay, CustomFieldRequired = true, IsOpenEnded = false };
			CustomFieldDTO openEndedCustomField = new CustomFieldDTO { Id = 2, CustomFieldName = "Custom", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.TaskDisplay, CustomFieldRequired = true, IsOpenEnded = true };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(new Collection<CustomFieldDTO> { taskCustomField, openEndedCustomField });

			CustomFieldValueDTO taskCustomFieldValue_Red = new CustomFieldValueDTO { CustomFieldID = taskCustomField.Id, CustomFieldValueID = 2, CustomFieldValueName = "Red", CustomFieldValueDescription = "the color red", CustomFieldValueInUseFlag = true };
			CustomFieldValueDTO taskCustomFieldValue_Blue = new CustomFieldValueDTO { CustomFieldID = taskCustomField.Id, CustomFieldValueID = 3, CustomFieldValueName = "Blue", CustomFieldValueDescription = "the color blue", CustomFieldValueInUseFlag = false };
			CustomFieldValueDTO openEndedCustomFieldValue = new CustomFieldValueDTO { CustomFieldID = openEndedCustomField.Id, CustomFieldValueID = 4, CustomFieldValueDescription = "Test", CustomFieldValueInUseFlag = true };
			ICollection<CustomFieldValueDTO> customFieldOptions = new Collection<CustomFieldValueDTO> { taskCustomFieldValue_Blue, taskCustomFieldValue_Red, openEndedCustomFieldValue };
			List<int> customFieldIds = new List<int> { taskCustomField.Id, openEndedCustomField.Id };
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(customFieldIds, It.IsAny<int>())).Returns(customFieldOptions);

			Collection<BOECustomFieldModelView> result = sut.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.Task);
			retriever.Verify(x => x.GetCustomFieldsByWorkspaceId(workspace.Id), Times.Once());
			retriever.Verify(x => x.GetCustomFieldValuesByFieldIds(customFieldIds, It.IsAny<int>()), Times.Once());

			Assert.IsFalse(result[0].CustomFieldMetaData.isOpenEnded);
			Assert.IsTrue(result[0].CustomFieldOptions[0].ID == "Blue");
			Assert.IsTrue(result[0].CustomFieldOptions[1].ID == "Red");

			Assert.IsTrue(result[1].CustomFieldMetaData.isOpenEnded);
			Assert.AreEqual(1, result[1].CustomFieldOptions.Count);
			Assert.AreEqual("Test", result[1].CustomFieldOptions[0].Description);
		}

		/// <summary>
		/// Test GetCustomFieldOptionModelViews for MOQ Type Table level custom fields
		/// </summary>
		[TestMethod]
		public void Test_GetCustomFieldOptionModelViews_MoqTypeTableLevel()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2 };
			FullWorkspace ws = new FullWorkspace(workspace);
			CustomFieldDTO moqTypeTableCustomField = new CustomFieldDTO { Id = 1, CustomFieldName = "Color", WorkspaceID = workspace.Id, CustomFieldDisplayID = CustomFieldType.MoqTypeTableDataDisplay, CustomFieldRequired = true, IsOpenEnded = true };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(new Collection<CustomFieldDTO> { moqTypeTableCustomField });

			CustomFieldValueDTO openEndedCustomFieldValue = new CustomFieldValueDTO { CustomFieldID = moqTypeTableCustomField.Id, CustomFieldValueID = 4, CustomFieldValueDescription = "Test", CustomFieldValueInUseFlag = true };
			ICollection<CustomFieldValueDTO> customFieldOptions = new Collection<CustomFieldValueDTO> { openEndedCustomFieldValue };
			List<int> customFieldIds = new List<int> { moqTypeTableCustomField.Id };
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(customFieldIds, It.IsAny<int>())).Returns(customFieldOptions);

			Collection<BOECustomFieldModelView> result = sut.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.MoqTypeTable);
			retriever.Verify(x => x.GetCustomFieldsByWorkspaceId(workspace.Id), Times.Once());
			retriever.Verify(x => x.GetCustomFieldValuesByFieldIds(customFieldIds, It.IsAny<int>()), Times.Once());

			Assert.IsTrue(result[0].CustomFieldMetaData.isOpenEnded);
			Assert.AreEqual(1, result[0].CustomFieldOptions.Count);
			Assert.AreEqual("Test", result[0].CustomFieldOptions[0].Description);
		}

		[TestMethod]
		public void Test_GetCustomFieldOptionModelViews_LaborTypeLevel()
		{
			BOELaborControllerLogic sut = CreateSystem();

			//test
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2 };
			FullWorkspace ws = new FullWorkspace(workspace);
			CustomFieldDTO laborCustomField = new CustomFieldDTO { Id = 1, CustomFieldName = "Color", WorkspaceID = workspace.Id, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = true, IsOpenEnded = false };
			CustomFieldDTO openEndedCustomField = new CustomFieldDTO { Id = 2, CustomFieldName = "Custom", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldRequired = true, IsOpenEnded = true };
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(workspace.Id)).Returns(new Collection<CustomFieldDTO> { laborCustomField, openEndedCustomField });

			CustomFieldValueDTO laborCustomFieldValue_Red = new CustomFieldValueDTO { CustomFieldID = laborCustomField.Id, CustomFieldValueID = 2, CustomFieldValueName = "Red", CustomFieldValueDescription = "the color red", CustomFieldValueInUseFlag = true };
			CustomFieldValueDTO laborCustomFieldValue_Blue = new CustomFieldValueDTO { CustomFieldID = laborCustomField.Id, CustomFieldValueID = 3, CustomFieldValueName = "Blue", CustomFieldValueDescription = "the color blue", CustomFieldValueInUseFlag = false };
			CustomFieldValueDTO openEndedCustomFieldValue = new CustomFieldValueDTO { CustomFieldID = openEndedCustomField.Id, CustomFieldValueID = 4, CustomFieldValueDescription = "Test", CustomFieldValueInUseFlag = true };
			ICollection<CustomFieldValueDTO> customFieldOptions = new Collection<CustomFieldValueDTO> { laborCustomFieldValue_Blue, laborCustomFieldValue_Red, openEndedCustomFieldValue };
			List<int> customFieldIds = new List<int> { laborCustomField.Id, openEndedCustomField.Id };
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(customFieldIds, workspace.Id)).Returns(customFieldOptions);

			Collection<BOECustomFieldModelView> result = sut.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.LaborTypes);
			retriever.Verify(x => x.GetCustomFieldsByWorkspaceId(workspace.Id), Times.Once());
			retriever.Verify(x => x.GetCustomFieldValuesByFieldIds(customFieldIds, workspace.Id), Times.Once());

			Assert.IsFalse(result[0].CustomFieldMetaData.isOpenEnded);
			Assert.IsTrue(result[0].CustomFieldOptions[0].ID == "Blue");
			Assert.IsTrue(result[0].CustomFieldOptions[1].ID == "Red");

			Assert.IsTrue(result[1].CustomFieldMetaData.isOpenEnded);
			Assert.AreEqual(1, result[1].CustomFieldOptions.Count);
			Assert.AreEqual("Test", result[1].CustomFieldOptions[0].Description);
		}

		#region GetMOQTypes Tests
		[TestMethod]
		public void GetMOQTypes()
		{
			BOELaborControllerLogic sut = CreateSystem();
			ICollection<MOQType> result = sut.GetMOQTypes();
			Assert.AreEqual(9, result.Count(), "The number of MOQTypes returned is incorrect.");
			Assert.IsTrue(result.Contains(MOQType.Standard), "The MOQType.Standard was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.EstimatingRelationships), "The EstimatingRelationships.SSCBottomUp was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.Probability), "The MOQType.Probability was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.Factor), "The MOQType.Factor was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.Unit), "The MOQType.Unit was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.Comparison), "The MOQType.Comparison was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.Judgment), "The MOQType.Judgment was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.LevelOfEffort), "The MOQType.LevelOfEffort was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.VendorQuote), "The MOQType.VendorQuote was not found in the collection.");
		}

		[TestMethod]
		public void GetMOQTypesSSC()
		{
			BOELaborControllerLogic sut = CreateSystemSSC();
			ICollection<MOQType> result = sut.GetMOQTypes();
			Assert.AreEqual(9, result.Count(), "The number of MOQTypes returned is incorrect.");
			Assert.IsTrue(result.Contains(MOQType.SSCAnalogySimilarTo), "The MOQType.SSCAnalogySimilarTo was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.SSCBottomUp), "The MOQType.SSCBottomUp was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.SSCCostEstimatingRelationships), "The MOQType.SSCCostEstimatingRelationships was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.SSCHistoricalExperienceFactor), "The MOQType.SSCHistoricalExperienceFactor was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.SSCLaborStandardsAndRealizationPerformanceFactors), "The MOQType.SSCLaborStandardsAndRealizationPerformanceFactors was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.SSCLevelOfEffortSupport), "The MOQType.SSCLevelOfEffortSupport was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.SSCDataDrivenCostModelsEquations), "The MOQType.SSCDataDrivenCostModelsEquations was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.SSCActual), "The MOQType.SSCActual was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.SSCQuote), "The MOQType.SSCQuote was not found in the collection.");
		}

		[TestMethod]
		public void GetMOQTypesMST()
		{
			BOELaborControllerLogic sut = CreateSystemMST();
			ICollection<MOQType> result = sut.GetMOQTypes();
			Assert.AreEqual(8, result.Count(), "The number of MOQTypes returned is incorrect.");
			Assert.IsTrue(result.Contains(MOQType.MSTHistoricalPerformance), "The MOQType.MSTHistoricalPerformance was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.MSTComparisonAnalogyMethod), "The MOQType.MSTComparisonAnalogyMethod was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.MSTCostEstimatingRelationships), "The MOQType.MSTCostEstimatingRelationships was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.MSTParametricCostModels), "The MOQType.MSTParametricCostModels was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.MSTStandardTimeEstimating), "The MOQType.MSTStandardTimeEstimating was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.MSTFactorUnitMethod), "The MOQType.MSTFactorUnitMethod was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.MSTLevelOfEffortSupport), "The MOQType.MSTLevelOfEffortSupport was not found in the collection.");
			Assert.IsTrue(result.Contains(MOQType.MSTEngineeringJudgmentalEstimates), "The MOQType.MSTEngineeringJudgmentalEstimates was not found in the collection.");
		}
		#endregion

		#region GetMOQTypesHelpText Tests
		[TestMethod]
		public void GetMOQTypesHelpText()
		{
			BOELaborControllerLogic sut = CreateSystem();
			String result = sut.GetMOQTypesHelpText();
			Assert.AreEqual(CommonConstants.BOE_MOQ_TYPES_HELP_TEXT_ISGS, result, "The MOQ Types help text for IS&GS is incorrect.");
		}

		[TestMethod]
		public void GetMOQTypesHelpTextSSC()
		{
			BOELaborControllerLogic sut = CreateSystemSSC();
			String result = sut.GetMOQTypesHelpText();
			Assert.AreEqual(CommonConstants.BOE_MOQ_TYPES_HELP_TEXT_SPACE_SYSTEMS, result, "The MOQ Types help text for SSC is incorrect.");
		}

		[TestMethod]
		public void GetMOQTypesHelpTextMST()
		{
			BOELaborControllerLogic sut = CreateSystemMST();
			String result = sut.GetMOQTypesHelpText();
			Assert.AreEqual(CommonConstants.BOE_MOQ_TYPES_HELP_TEXT_MST, result, "The MOQ Types help text for MST is incorrect.");
		}
		#endregion

		#region GetMOQEquationLabel Tests
		[TestMethod]
		public void GetMOQEquationLabel()
		{
			BOELaborControllerLogic sut = CreateSystem();
			String result = sut.GetMOQEquationLabel();
			Assert.AreEqual(CommonConstants.BOE_MOQ_EQUATION_LABEL, result, "The MOQ equation label for IS&GS is incorrect.");
		}

		[TestMethod]
		public void GetMOQEquationLabelSSC()
		{
			BOELaborControllerLogic sut = CreateSystemSSC();
			String result = sut.GetMOQEquationLabel();
			Assert.AreEqual(CommonConstants.BOE_MOQ_EQUATION_LABEL, result, "The MOQ equation label for SSC is incorrect.");
		}

		[TestMethod]
		public void GetMOQEquationLabelMST()
		{
			BOELaborControllerLogic sut = CreateSystemMST();
			String result = sut.GetMOQEquationLabel();
			Assert.AreEqual(CommonConstants.BOE_MOQ_EQUATION_LABEL, result, "The MOQ equation label for MST is incorrect.");
		}
		#endregion

		#region GetMOQTextLabel Tests
		[TestMethod]
		public void GetMOQTextLabel()
		{
			BOELaborControllerLogic sut = CreateSystem();
			String result = sut.GetMOQTextLabel();
			Assert.AreEqual(CommonConstants.BOE_MOQ_TEXT_LABEL, result, "The MOQ text label for IS&GS is incorrect.");
		}

		[TestMethod]
		public void GetMOQTextLabelSSC()
		{
			BOELaborControllerLogic sut = CreateSystemSSC();
			String result = sut.GetMOQTextLabel();
			Assert.AreEqual(CommonConstants.BOE_MOQ_TEXT_LABEL_SPACE_SYSTEMS, result, "The MOQ text label for SSC is incorrect.");
		}

		[TestMethod]
		public void GetMOQTextLabelMST()
		{
			BOELaborControllerLogic sut = CreateSystemMST();
			String result = sut.GetMOQTextLabel();
			Assert.AreEqual(CommonConstants.BOE_MOQ_TEXT_LABEL_SPACE_SYSTEMS, result, "The MOQ text label for MST is incorrect.");
		}
		#endregion

		/// <summary>
		/// Test to get metric search dialog parameters for IS&GS.
		/// </summary>
		[TestMethod]
		public void GetMetricSearchDialogParameters()
		{
			BOELaborControllerLogic sut = CreateSystem();
			LaborTaskModelView modelView = new LaborTaskModelView();
			sut.GetMetricSearchDialogParameters(modelView);
			Assert.AreEqual(modelView.MetricsSearchDialogParameters.DialogTitle, string.Empty, "The Metric Search Dialog title is incorrect for IS&GS.");
			Assert.AreEqual(modelView.MetricsSearchDialogParameters.SearchMetricsDialogIdSuffix, null, "The Metric dialog suffix Id is incorrect for IS&GS.");
			Assert.AreEqual(modelView.MetricsPagingActionName, string.Empty, "The Metric dialog paging action name is incorrect for IS&GS.");
		}

		#region GetMOQEquationModelView Tests
		[TestMethod]
		public void GetMOQEquationModelView()
		{
			BOELaborControllerLogic sut = CreateSystem();
			BoeTaskElementDTO te = new BoeTaskElementDTO();
			te.Id = 1;
			te.TaskTitle = "My Task Title";
			te.WasMoqTextSet = true; te.WasDescriptionSet = true;

			FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO());

			MOQEquationModelView result = sut.GetMOQModelView(te, workspace);
			Assert.IsFalse(result.ShowSearchMetricsLink);
		}

		[TestMethod]
		public void GetMOQEquationModelViewMST()
		{
			BOELaborControllerLogic sut = CreateSystemMST();
			BoeTaskElementDTO te = new BoeTaskElementDTO()
			{
				Id = 1,
				TaskTitle = "My Task Title",
				MOQText = "MOQ Text"
			};
			MSTMetricDetailsDTO dto = new MSTMetricDetailsDTO()
			{
				Id = 1,
				BusinessArea = "MST",
				DataSource = "PMM",
				DateAddedToTaskElement = DateTime.Now,
				Comment = "Test Comment",
				ContractNumber = "12345",
				DataSourceId = 1,
				EndDate = DateTime.Now.AddYears(1),
				Equation = "1 + 2",
				LineOfBusiness = "Civil",
				MeasureData = 0.98m,
				ProgramName = "JSF",
				MeasureName = "SoftwareEngineer",
				ProgramId = 1
			};

			FullWorkspace workspace = new FullWorkspace(new WorkspaceDTO());

			MOQEquationModelView result = sut.GetMOQModelView(te, workspace);
			Assert.AreEqual(1, result.PMMetricsUsed.Count, "The number of HistoricalMetricsUsed is incorrect.");
			Assert.AreEqual("SoftwareEngineer", result.PMMetricsUsed.ToCollection()[0].MeasureName, "The data in the returned model is not correct.");
			Assert.AreEqual(1, result.PMMetricsUsed.ToCollection()[0].Id, "The data in the returned model is not correct.");
			Assert.IsTrue(result.ShowSearchMetricsLink);
		}
		#endregion

		#region OverrideReadOnly Tests
		[TestMethod]
		public void OverrideReadOnlySSC()
		{
			BOELaborControllerLogic sut = CreateSystemSSC();

			WorkspaceDTO wsDto = new WorkspaceDTO();
			wsDto.WorkspaceState = WorkspaceState.Locked;

			FullWorkspace fws = new FullWorkspace(wsDto);

			BoeDTO boeDto = new BoeDTO();
			boeDto.Id = 1;
			boeDto.State = BOEState.Draft;

			FullBoe fboe = new FullBoe(boeDto);

			int EtiUserID = 1;

			UserDTO userDto = new UserDTO();
			userDto.UserID = 1;

			Collection<PermissionsDTO> perms = new Collection<PermissionsDTO>() { new PermissionsDTO() { BOEId = boeDto.Id, ETIUserId = EtiUserID, Role = Role.Author } };


			_UserLoader.Setup(x => x.GetUserForActiveUser()).Returns(userDto);
			_PermissionDataLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boeDto.Id })).Returns(perms);
			_PermissionDataLoader.Setup(x => x.GetWorkspacePermissions(fws.Id)).Returns(new Collection<PermissionsDTO>());

			// test when WS state is locked and user role is author
			Boolean result = sut.OverrideReadOnly(fws, fboe);
			Assert.IsTrue(result);

			// now set the WS to working and the users role to sub
			fws.WorkspaceState = WorkspaceState.Working;
			perms = new Collection<PermissionsDTO>() { new PermissionsDTO() { BOEId = boeDto.Id, ETIUserId = EtiUserID, Role = Role.SubcontractorAuthor } };
			_PermissionDataLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boeDto.Id })).Returns(perms);

			// retest
			result = sut.OverrideReadOnly(fws, fboe);
			Assert.IsTrue(result);

			// now set the users role to workspace admin
			perms = new Collection<PermissionsDTO>() { new PermissionsDTO() { BOEId = boeDto.Id, ETIUserId = EtiUserID, Role = Role.WorkspaceAdmin } };
			_PermissionDataLoader.Setup(x => x.GetWorkspacePermissions(fws.Id)).Returns(new Collection<PermissionsDTO>());

			// retest
			result = sut.OverrideReadOnly(fws, fboe);
			Assert.IsTrue(result);

			// now trigger false returns

			// now set the users role to approver
			perms = new Collection<PermissionsDTO>() { new PermissionsDTO() { BOEId = boeDto.Id, ETIUserId = EtiUserID, Role = Role.Approver } };
			_PermissionDataLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boeDto.Id })).Returns(perms);

			// retest
			result = sut.OverrideReadOnly(fws, fboe);
			Assert.IsFalse(result);

			// now set the users role back to author and set the ETIUserId to other than current user
			perms = new Collection<PermissionsDTO>() { new PermissionsDTO() { BOEId = boeDto.Id, ETIUserId = 2, Role = Role.Author } };
			_PermissionDataLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boeDto.Id })).Returns(perms);

			// retest
			result = sut.OverrideReadOnly(fws, fboe);
			Assert.IsFalse(result);

			// now set the workspace state to init
			fws.WorkspaceState = WorkspaceState.Initialization;

			// retest
			result = sut.OverrideReadOnly(fws, fboe);
			Assert.IsFalse(result);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void OverrideReadOnlyExceptionSSC()
		{
			BOELaborControllerLogic sut = CreateSystemSSC();

			BoeDTO boeDto = new BoeDTO();
			boeDto.Id = 1;
			boeDto.State = BOEState.Draft;
			FullBoe fboe = new FullBoe(boeDto);

			sut.OverrideReadOnly(null, fboe);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void OverrideReadOnlyException2SSC()
		{
			BOELaborControllerLogic sut = CreateSystemSSC();

			WorkspaceDTO wsDto = new WorkspaceDTO();
			wsDto.WorkspaceState = WorkspaceState.Locked;
			FullWorkspace fws = new FullWorkspace(wsDto);

			sut.OverrideReadOnly(fws, null);
		}

		[TestMethod]
		public void OverrideReadOnlyMST()
		{
			BOELaborControllerLogic sut = CreateSystemMST();

			WorkspaceDTO wsDto = new WorkspaceDTO();
			wsDto.WorkspaceState = WorkspaceState.Locked;

			FullWorkspace fws = new FullWorkspace(wsDto);

			BoeDTO boeDto = new BoeDTO();
			boeDto.Id = 1;
			boeDto.State = BOEState.Draft;

			FullBoe fboe = new FullBoe(boeDto);

			int EtiUserID = 1;

			UserDTO userDto = new UserDTO();
			userDto.UserID = 1;

			Collection<PermissionsDTO> perms = new Collection<PermissionsDTO>() { new PermissionsDTO() { BOEId = boeDto.Id, ETIUserId = EtiUserID, Role = Role.Author } };


			_UserLoader.Setup(x => x.GetUserForActiveUser()).Returns(userDto);
			_PermissionDataLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boeDto.Id })).Returns(perms);

			// test when WS state is locked and user role is author
			Boolean result = sut.OverrideReadOnly(fws, fboe);
			Assert.IsTrue(result);

			// now set the WS to working and the users role to sub
			fws.WorkspaceState = WorkspaceState.Working;
			perms = new Collection<PermissionsDTO>() { new PermissionsDTO() { BOEId = boeDto.Id, ETIUserId = EtiUserID, Role = Role.SubcontractorAuthor } };
			_PermissionDataLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boeDto.Id })).Returns(perms);

			// retest
			result = sut.OverrideReadOnly(fws, fboe);
			Assert.IsTrue(result);

			// now set the users role to workspace admin
			perms = new Collection<PermissionsDTO>() { new PermissionsDTO() { BOEId = boeDto.Id, ETIUserId = EtiUserID, Role = Role.WorkspaceAdmin } };
			_PermissionDataLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boeDto.Id })).Returns(perms);

			// retest
			result = sut.OverrideReadOnly(fws, fboe);
			Assert.IsTrue(result);

			// now trigger false returns

			// now set the users role to approver
			perms = new Collection<PermissionsDTO>() { new PermissionsDTO() { BOEId = boeDto.Id, ETIUserId = EtiUserID, Role = Role.Approver } };
			_PermissionDataLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boeDto.Id })).Returns(perms);

			// retest
			result = sut.OverrideReadOnly(fws, fboe);
			Assert.IsFalse(result);

			// now set the users role back to author and set the ETIUserId to other than current user
			perms = new Collection<PermissionsDTO>() { new PermissionsDTO() { BOEId = boeDto.Id, ETIUserId = 2, Role = Role.Author } };
			_PermissionDataLoader.Setup(x => x.GetBOEPermissions(new List<int>() { boeDto.Id })).Returns(perms);

			// retest
			result = sut.OverrideReadOnly(fws, fboe);
			Assert.IsFalse(result);

			// now set the workspace state to init
			fws.WorkspaceState = WorkspaceState.Initialization;

			// retest
			result = sut.OverrideReadOnly(fws, fboe);
			Assert.IsFalse(result);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void OverrideReadOnlyExceptionMST()
		{
			BOELaborControllerLogic sut = CreateSystemMST();

			BoeDTO boeDto = new BoeDTO();
			boeDto.Id = 1;
			boeDto.State = BOEState.Draft;
			FullBoe fboe = new FullBoe(boeDto);

			sut.OverrideReadOnly(null, fboe);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void OverrideReadOnlyException2MST()
		{
			BOELaborControllerLogic sut = CreateSystemMST();

			WorkspaceDTO wsDto = new WorkspaceDTO();
			wsDto.WorkspaceState = WorkspaceState.Locked;
			FullWorkspace fws = new FullWorkspace(wsDto);

			sut.OverrideReadOnly(fws, null);
		}
		#endregion

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetPerformingOrgsForLaborTaskElement_NullTest()
		{
			BOELaborControllerLogic sut = CreateSystem();
			FullWorkspace ws = null;
			sut.GetPerformingOrgs(ws);
		}

		[TestMethod]
		public void GetPerformingOrgsForLaborTaskElement_Pass()
		{
			BOELaborControllerLogic sut = CreateSystem();
			FullWorkspace ws = new FullWorkspace();
			ws.PerfOrgListID = 1;
			Collection<PerformingOrgDTO> toReturnOrgs = new Collection<PerformingOrgDTO>();
			toReturnOrgs.Add(new PerformingOrgDTO()
			{
				Id = 1,
				PerformingOrgName = "name1",
				PerformingOrgDesc = "des1"
			});
			toReturnOrgs.Add(new PerformingOrgDTO()
			{
				Id = 2,
				PerformingOrgName = "name2",
				PerformingOrgDesc = "des2"

			});
			toReturnOrgs.Add(new PerformingOrgDTO()
			{
				Id = 3,
				PerformingOrgName = "name3",
				PerformingOrgDesc = "des3"
			});


			Collection<PerformingOrgModelView> expectedReturn = new Collection<PerformingOrgModelView>();
			expectedReturn.Add(new PerformingOrgModelView()
			{
				PerformingOrgID = 1,
				PerformingOrgName = "name1",
				PerformingOrgDesc = "des1"
			});
			expectedReturn.Add(new PerformingOrgModelView()
			{
				PerformingOrgID = 2,
				PerformingOrgName = "name2",
				PerformingOrgDesc = "des2"

			});
			expectedReturn.Add(new PerformingOrgModelView()
			{
				PerformingOrgID = 3,
				PerformingOrgName = "name3",
				PerformingOrgDesc = "des3"
			});

			this.retriever.Setup(x => x.GetPerformingOrgsByListId(ws.PerfOrgListID)).Returns(toReturnOrgs);
			Collection<PerformingOrgModelView> toCheck = sut.GetPerformingOrgs(ws);


			Assert.AreEqual(expectedReturn.Count, toCheck.Count);
			for (int i = 0; i < toCheck.Count; i++)
			{
				Assert.AreEqual(expectedReturn[i].PerformingOrgID, toCheck[i].PerformingOrgID);
				Assert.AreEqual(expectedReturn[i].PerformingOrgName, toCheck[i].PerformingOrgName);
				Assert.AreEqual(expectedReturn[i].PerformingOrgDesc, toCheck[i].PerformingOrgDesc);

			}
		}

		/// <summary>
		/// Test GetLaborTaskData
		/// </summary>
		[TestMethod]
		public void GetLaborTaskDataTest()
		{
			BOELaborControllerLogic sut = CreateSystem();

			FullWorkspace testWorkspace = new FullWorkspace() { WorkspaceName = "Test Workspace", CostDecimalPrecision = 2, ResourceDecimalPrecision = 2 };

			ResourceSpreadDto testSpread = new ResourceSpreadDto { BoeID = this.Boe1.Id, Id = 1, LaborSpreadDate = DateTime.Now, LaborSpreadValue = 5000 };

			CustomFieldValueContainer testLaborCustomField = new CustomFieldValueContainer()
			{
				CustomFieldValueID = 2,
				ContainerID = 2,
				UpdateDate = DateTime.Now,
				CustomFieldID = 2,
				IsOpenEnded = false,
				OpenEndedValue = string.Empty
			};

			ResourceTypeDto testLabor = new ResourceTypeDto
			{
				SpreadType = IES.Common.SpreadType.Hours,
				BoeID = this.Boe1.Id,
				Id = 1,
				SpreadCurveID = SpreadCurves.SpreadCurve1,
				ValueSpread = 5000,
				PercentSpread = 100,
				PerformingOrgID = this.Perforg.Id,
				ResourceID = this.Resource.Id,
				StartDateValue = DateTime.Now,
				EndDateValue = DateTime.Now,
				LaborSpreads = new Collection<ResourceSpreadDto> { testSpread },
				CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { testLaborCustomField }
			};

			// Add discrete in order to test ContainsDiscrete
			ResourceTypeDto testLaborDiscrete = new ResourceTypeDto
			{
				SpreadType = IES.Common.SpreadType.Hours,
				BoeID = this.Boe1.Id,
				Id = 2,
				SpreadCurveID = SpreadCurves.DiscreteHours,
				ValueSpread = 5000,
				PerformingOrgID = this.Perforg.Id,
				ResourceID = this.Resource.Id,
				StartDateValue = DateTime.Now,
				EndDateValue = DateTime.Now,
				LaborSpreads = new Collection<ResourceSpreadDto> { testSpread }
			};

			this.perfOrgLoader.Setup(x => x.GetById(this.Perforg.Id)).Returns(this.Perforg);
			this.perfOrgLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<PerformingOrgDTO> { this.Perforg });

			CustomFieldValueContainer testTaskCustomField = new CustomFieldValueContainer()
			{
				CustomFieldValueID = 1,
				ContainerID = 1,
				UpdateDate = DateTime.Now,
				CustomFieldID = 1,
				IsOpenEnded = false,
				OpenEndedValue = string.Empty
			};

			BoeTaskElementDTO testTask = new BoeTaskElementDTO
			{
				Id = 1,
				BoeID = this.Boe1.Id,
				MOQHoursEquation = "10000",
				LaborTypeWarningFlag = false,
				taskElementLabors = new Collection<ResourceTypeDto> { testLabor, testLaborDiscrete },
				CustomFieldValueContainers = new Collection<CustomFieldValueContainer>() { testTaskCustomField }
			};

			_ResourceLoader.Setup(x => x.GetById(this.Resource.Id)).Returns(this.Resource);
			this._ResourceLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<ResourceDTO> { this.Resource });
			factory.Setup(x => x.CreateFullWorkspace(testWorkspace.WorkspaceName, It.IsAny<bool>())).Returns(testWorkspace);
			factory.Setup(x => x.CreateTaskElement(testTask.Id, testWorkspace.DecimalPrecision, testWorkspace.CostDecimalPrecision)).Returns(testTask);
			_TaskElementValidation.Setup(x => x.ValidateTaskElementsWithErrorMessages(It.IsAny<FullWorkspace>(), It.IsAny<ICollection<BoeTaskElementDTO>>())).Returns(new List<LaborValidationClass>());
			retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(testWorkspace.Id)).Returns(new List<CustomFieldDTO>());
			retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new List<CustomFieldValueDTO>());
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(this.Boe1.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO> { testTask });
			retriever.Setup(x => x.GetFullWorkspaceById(It.IsAny<int>())).Returns(testWorkspace);
			retriever.Setup(x => x.GetMoqTypeSelectionsByBoeId(this.Boe1.Id)).Returns(new List<MoqTypeSelection>());
			LaborTaskDataModelView result = sut.GetLaborTaskData(testWorkspace, new FullBoe(this.Boe1), testTask.Id);

			// Assert task element data
			Assert.AreEqual(testTask.Id, result.TaskElementData.TaskElementDetailID);
			Assert.AreEqual(testTask.BOETaskID, result.TaskElementData.TaskID);
			Assert.AreEqual(testTask.TaskTitle, result.TaskElementData.Title);
			Assert.AreEqual(testTask.Description, result.TaskElementData.TaskDescription);
			Assert.AreEqual(testTask.BoeID, result.TaskElementData.BOEID);
			Assert.AreEqual(testTask.StartDate.Value.ToString("MM/yyyy"), result.TaskElementData.StartDate);
			Assert.AreEqual(testTask.EndDate.Value.ToString("MM/yyyy"), result.TaskElementData.EndDate);
			Assert.AreEqual(testTask.MOQHoursEquation, result.TaskElementData.MOQHoursEquation);
			Assert.AreEqual(testTask.MOQType, result.TaskElementData.MOQType);
			Assert.AreEqual(testTask.MOQText, result.TaskElementData.MOQText);

			// Assert custom field values
			Assert.IsTrue(result.TaskElementData.CustomFieldValues.Any());
			Assert.AreEqual(testTaskCustomField.CustomFieldValueID, result.TaskElementData.CustomFieldValues.First().CustomFieldValueID);
			Assert.AreEqual(testTaskCustomField.ContainerID, result.TaskElementData.CustomFieldValues.First().SelectionID);
			Assert.AreEqual(testTaskCustomField.UpdateDate, result.TaskElementData.CustomFieldValues.First().UpdateDate);
			Assert.AreEqual(testTaskCustomField.CustomFieldID, result.TaskElementData.CustomFieldValues.First().CustomFieldID);
			Assert.AreEqual(testTaskCustomField.IsOpenEnded, result.TaskElementData.CustomFieldValues.First().IsOpenEnded);
			Assert.AreEqual(testTaskCustomField.OpenEndedValue, result.TaskElementData.CustomFieldValues.First().OpenEndedValue);

			// Assert Labor Types Data
			Assert.IsTrue(result.LaborTypesData.Any());
			Assert.AreEqual(testLabor.Id, result.LaborTypesData.First().BOELaborTypeID);
			Assert.AreEqual(testLabor.ResourceID, result.LaborTypesData.First().ResourceID);
			Assert.AreEqual(testLabor.PerformingOrgID, result.LaborTypesData.First().PerformingOrgID);
			Assert.AreEqual(testLabor.SpreadCurveID, result.LaborTypesData.First().SpreadCurveID);
			Assert.AreEqual(testLabor.PercentSpread, result.LaborTypesData.First().PercentSpread);
			Assert.AreEqual(this.Resource.ResourceName, result.LaborTypesData.First().ResourceName);
			Assert.AreEqual(this.Resource.ResourceDesc, result.LaborTypesData.First().ResourceDescription);
			Assert.AreEqual(this.Resource.RateType, result.LaborTypesData.First().RateType);
			Assert.AreEqual(testLabor.CanOffload, result.LaborTypesData.First().CanOffload);
			Assert.AreEqual(testLabor.TieredPercentage, result.LaborTypesData.First().TieredPercentage);
			Assert.AreEqual((int)this.Resource.ElementOfCost, result.LaborTypesData.First().ElementOfCost);
			Assert.AreEqual(testLabor.ValueSpread, result.LaborTypesData.First().HourSpread);
			Assert.AreNotEqual(testLabor.ValueSpread, result.LaborTypesData.First().CostSpread);
			Assert.AreEqual(testLabor.StartDate.Value.ToString("MM/yyyy"), result.LaborTypesData.First().StartDate);
			Assert.AreEqual(testLabor.EndDate.Value.ToString("MM/yyyy"), result.LaborTypesData.First().EndDate);
			Assert.AreEqual(testLabor.UpdateDate, result.LaborTypesData.First().UpdateDate);
			Assert.AreEqual(testLabor.PercentSpreadLocked, result.LaborTypesData.First().PercentSpreadLocked);
			Assert.AreEqual(testLabor.HourSpreadLocked, result.LaborTypesData.First().HourSpreadLocked);
			Assert.AreEqual(testLabor.SpreadType, result.LaborTypesData.First().RateType == RateType.Hours ? SpreadType.Hours : SpreadType.Cost);
			Assert.AreEqual(testLabor.WBSID, result.LaborTypesData.First().WBSID);
			Assert.AreEqual(testLabor.CLINID, result.LaborTypesData.First().CLINID);

			// Assert Labor Types Custom Fields
			Assert.IsTrue(result.LaborTypesData.First().CustomFieldValues.Any());
			Assert.AreEqual(testLaborCustomField.CustomFieldValueID, result.LaborTypesData.First().CustomFieldValues.First().CustomFieldValueID);
			Assert.AreEqual(testLaborCustomField.ContainerID, result.LaborTypesData.First().CustomFieldValues.First().SelectionID);
			Assert.AreEqual(testLaborCustomField.UpdateDate, result.LaborTypesData.First().CustomFieldValues.First().UpdateDate);
			Assert.AreEqual(testLaborCustomField.CustomFieldID, result.LaborTypesData.First().CustomFieldValues.First().CustomFieldID);
			Assert.AreEqual(testLaborCustomField.IsOpenEnded, result.LaborTypesData.First().CustomFieldValues.First().IsOpenEnded);
			Assert.AreEqual(testLaborCustomField.OpenEndedValue, result.LaborTypesData.First().CustomFieldValues.First().OpenEndedValue);

			// Assert Spreads
			Assert.IsTrue(result.LaborTypesData.First().Spreads.Any());
			Assert.AreEqual(testSpread.LaborSpreadDate.ToMonthString(), result.LaborTypesData.First().Spreads.First().LaborSpreadDate);
			Assert.AreEqual(testSpread.LaborSpreadDate.ToMonthString(), result.LaborTypesData.First().Spreads.First().LaborSpreadDate);
			Assert.AreEqual(testSpread.LaborSpreadValue, result.LaborTypesData.First().Spreads.First().LaborSpreadValue);

			// Assert Contains Discrete - true due to test setup
			Assert.IsTrue(result.ContainsDiscrete);
		}

		/// <summary>
		/// Test CalculateLaborSpreads
		/// </summary>
		[TestMethod]
		public void CalculateLaborSpreadsTest()
		{
			BOELaborControllerLogic sut = CreateSystem();

			decimal testValue = 10000;
			DateTime startDate = DateTime.Now.AddYears(-1);
			DateTime endDate = DateTime.Now;
			SpreadCurves testSpread = SpreadCurves.SpreadCurve3;
			int testPrecision = 3;

			// Get number of months to determine expected number of spread values
			int numberOfValues = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month + 1;

			ICollection<LaborSpreadDataModelView> results = sut.CalculateLaborSpreads(testValue, startDate, endDate, testSpread, testPrecision);

			Assert.IsTrue(results.Any());
			Assert.AreEqual(numberOfValues, results.Count);

			// Calculate expectedSpreads based on Spread Curve 3
			// A recreation of SpreadCurve.SpreadFlat, which is private
			decimal[] expectedSpreadValues = new decimal[numberOfValues];
			decimal valuePerMonth = testValue / numberOfValues;
			decimal resid = 0;
			for (int i = 0; i < numberOfValues; i++)
			{
				decimal temp = (valuePerMonth + resid) + 0.5001m;
				resid += valuePerMonth - temp;
				expectedSpreadValues[i] = temp;
			}
			expectedSpreadValues = SpreadCurve.Smooth(testValue, expectedSpreadValues, 0, numberOfValues, testPrecision);

			DateTime expectedDate = startDate;
			int index = 0;

			foreach (LaborSpreadDataModelView result in results)
			{
				Assert.AreEqual(expectedSpreadValues[index], result.LaborSpreadValue);
				Assert.AreEqual(expectedDate.ToMonthString(), result.LaborSpreadDate);

				expectedDate = expectedDate.AddMonths(1);
				index++;
			}
		}

		/// <summary>
		/// Test CalculateLaborSpreads for validation exception for start date being after end date
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(GenValidationException))]
		public void CalculateLaborSpreadsTest_Ex()
		{
			BOELaborControllerLogic sut = CreateSystem();

			decimal testValue = 10000;
			DateTime startDate = DateTime.Now.AddYears(1);
			DateTime endDate = DateTime.Now;
			SpreadCurves testSpread = SpreadCurves.SpreadCurve3;
			int testPrecision = 3;

			ICollection<LaborSpreadDataModelView> results = sut.CalculateLaborSpreads(testValue, startDate, endDate, testSpread, testPrecision);
		}

		/// <summary>
		/// Test ReOrderLaborTypeOrder
		/// </summary>
		[TestMethod]
		public void ReOrderLaborTypeOrderTest()
		{
			BOELaborControllerLogic sut = CreateSystem();
			WorkspaceDTO workspace = new WorkspaceDTO { Id = 2 };
			FullBoe boe = new FullBoe(new BoeDTO { Id = 1, WorkspaceID = workspace.Id });
			FullWorkspace ws = new FullWorkspace(workspace);

			BoeTaskElementDTO task = CreateDto(boe, ws, sut);
			task.taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto() { Id = 1, LaborTypeOrder = 1 } };

			_BoeTaskElementMediator.Setup(x => x.MediatedBulkSaveTaskElements(It.IsAny<List<BoeTaskElementDTO>>(), It.IsAny<FullWorkspace>())).Returns(new Dictionary<int, int>() { { task.Id, task.Id } });

			LaborTypeOrderCollection modelView = new LaborTypeOrderCollection();
			modelView.LaborTypes = new Collection<LaborTypeOrder>();
			modelView.LaborTypes.Add(new LaborTypeOrder() { LaborTypeID = 1, ListOrder = 1 });

			sut.ReOrderLaborTypeOrder(ws, task, modelView);

			_BoeTaskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO> { task }, ws), Times.Once());
		}

		/// <summary>
		/// Test ReOrderLaborTypeOrder for Argument Null Exception
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ReOrderLaborTypeOrderTest_EX()
		{
			BOELaborControllerLogic sut = CreateSystem();

			sut.ReOrderLaborTypeOrder(null, null, null);
		}

		/// <summary>
		/// Test that Total Cost of Travel is calculated appropriately for both domestic and international.
		/// </summary>
		[TestMethod]
		public void FindAdjacentTasks_Test()
		{
			BOELaborControllerLogic sut = CreateSystem();

			BoeDTO boeA = new BoeDTO { Id = 355778, WorkspaceID = this.Workspace.Id, State = BOEState.Draft };
			BoeTaskElementDTO boeTaskElementA = new BoeTaskElementDTO { Id = 366, BOETaskElementOrder = 1, BoeID = boeA.Id, MOQHoursEquation = "15000", LaborTypeWarningFlag = false, TaskElementType = TaskElementType.Labor };
			BoeTaskElementDTO boeTaskElementB = new BoeTaskElementDTO { Id = 367, BOETaskElementOrder = 2, BoeID = boeA.Id, MOQHoursEquation = "15000", LaborTypeWarningFlag = false, TaskElementType = TaskElementType.Labor };
			BoeTaskElementDTO boeTaskElementC = new BoeTaskElementDTO { Id = 368, BOETaskElementOrder = 3, BoeID = boeA.Id, MOQHoursEquation = "15000", LaborTypeWarningFlag = false, TaskElementType = TaskElementType.Labor };
			BoeTaskElementDTO boeTaskElementD = new BoeTaskElementDTO { Id = 369, BOETaskElementOrder = 4, BoeID = boeA.Id, MOQHoursEquation = "15000", LaborTypeWarningFlag = false, TaskElementType = TaskElementType.Labor };

			FullBoe boeAObject = new FullBoe(boeA);
			boeAObject.SetTaskElements(new BoeTaskElementDTO[] { boeTaskElementA, boeTaskElementB, boeTaskElementC, boeTaskElementD });

			this.factory.Setup(x => x.CreateFullBoe(boeA.Id)).Returns(boeAObject);
			this.factory.Setup(x => x.CreateFullBoe(boeA)).Returns(boeAObject);
			this.factory.Setup(x => x.CreateFullBoe(boeAObject)).Returns(boeAObject);
			this.factory.Setup(x => x.CreateFullBoes(It.IsAny<Collection<int>>())).Returns(new Collection<FullBoe> { boeAObject });

			AdjacentItems items = sut.FindAdjacentTasks(boeAObject, boeTaskElementA.Id);
			Assert.IsNull(items.PreviousId);
			Assert.AreEqual(boeTaskElementB.Id, items.NextId);

			items = sut.FindAdjacentTasks(boeAObject, boeTaskElementB.Id);
			Assert.AreEqual(boeTaskElementA.Id, items.PreviousId);
			Assert.AreEqual(boeTaskElementC.Id, items.NextId);

			items = sut.FindAdjacentTasks(boeAObject, boeTaskElementC.Id);
			Assert.AreEqual(boeTaskElementB.Id, items.PreviousId);
			Assert.AreEqual(boeTaskElementD.Id, items.NextId);

			items = sut.FindAdjacentTasks(boeAObject, boeTaskElementD.Id);
			Assert.AreEqual(boeTaskElementC.Id, items.PreviousId);
			Assert.AreEqual(null, items.NextId);
		}

		/// <summary>
		/// Test GetMoqTypeHelpUrls for SSC
		/// </summary>
		[TestMethod]
		public void GetMoqTypeHelpUrls_Test_SSC()
		{
			BOELaborControllerLogic sut = CreateSystemSSC();

			MoqTypeHelpUrls helpUrls = sut.GetMoqTypeHelpUrls();

			Assert.IsNotNull(helpUrls);

			// Assert base url matches web.config
			Assert.AreEqual(ConfigurationUtilities.GetAppSetting("MOQHelpBaseUrl"), helpUrls.BaseUrl);

			foreach (PropertyInfo property in helpUrls.GetType().GetProperties())
			{
				string value = property.GetValue(helpUrls).ToString();
				Assert.IsNotNull(value);

				if (property.Name == "ContractNumberSuffix" || property.Name == "TotalWBSHoursSuffix")
				{
					// This field isn't used in SSC
					Assert.IsTrue(string.IsNullOrEmpty(value));
				}
				else
				{
					// Make sure the rest have been populated
					Assert.IsFalse(string.IsNullOrEmpty(value));
				}
			}
		}

		/// <summary>
		/// Test GetMoqTypeHelpUrls for RMS
		/// </summary>
		[TestMethod]
		public void GetMoqTypeHelpUrls_Test_RMS()
		{
			BOELaborControllerLogic sut = CreateSystemMST();

			MoqTypeHelpUrls helpUrls = sut.GetMoqTypeHelpUrls();

			Assert.IsNotNull(helpUrls);

			// Assert base url matches web.config
			Assert.AreEqual(ConfigurationUtilities.GetAppSetting("MOQHelpBaseUrl"), helpUrls.BaseUrl);

			// Assert all fields set
			foreach (PropertyInfo property in helpUrls.GetType().GetProperties())
			{
				string value = property.GetValue(helpUrls).ToString();
				Assert.IsNotNull(value);

				if (property.Name == "RepositoryNameHistoricalSuffix" || property.Name == "RepositoryNameComparativeSuffix" || property.Name == "QueryTypeHistoricalSuffix" || property.Name == "QueryTypeComparativeSuffix")
				{
					// These fields aren't used in RMS
					Assert.IsTrue(string.IsNullOrEmpty(value));
				}
				else
				{
					// Make sure the rest have been populated
					Assert.IsFalse(string.IsNullOrEmpty(value));
				}
			}
		}

		/// <summary>
		/// Test ExportMoqTables
		/// </summary>
		[TestMethod]
		public void ExportMoqTables_Test()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1 };
			FullWorkspace ws = new FullWorkspace(workspace);

			MoqTypeSelection moqType = new MoqTypeSelection() { Id = 1 };
			moqType.TableData.Add(new MoqTableData() { Id = 1 });
			string expectedFileName = "exported_file_name.xlsx";

			this.moqTypeDataLoader.Setup(x => x.GetById(moqType.Id)).Returns(moqType);
			this.moqTableExporter.Setup(x => x.ExportToExcelFile(It.IsAny<string>(), moqType.TableData, It.IsAny<FullWorkspace>())).Returns(expectedFileName);

			string result = sut.ExportMoqTables(moqType.Id, ws, "tempfileloc.xlsx");

			Assert.AreEqual(expectedFileName, result);
		}

		/// <summary>
		/// Test ImportMoqTables
		/// </summary>
		[TestMethod]
		public async Task ImportMoqTables_Test()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1 };
			FullWorkspace ws = new FullWorkspace(workspace);

			Mock<HttpRequestBase> request = new Mock<HttpRequestBase>();
			request.SetupGet(x => x.Files.Count).Returns(2);
			request.SetupGet(x => x.Files[0].FileName).Returns("testfile.xlsx");

			ICollection<ImportedMoqTable> expected = new Collection<ImportedMoqTable>();
			expected.Add(new ImportedMoqTable() { Id = -1, TableName = "test", ImportTypes = new Collection<MoqTableImportType>() { MoqTableImportType.CreateMoqTable } });
			expected.Add(new ImportedMoqTable() { Id = -2, TableName = "test 2", ImportTypes = new Collection<MoqTableImportType>() { MoqTableImportType.MissingRequiredField } });
			this.moqTableImporter.Setup(x => x.ImportMoqTableFromExcelFile(It.IsAny<Stream>(), It.IsAny<FullWorkspace>())).Returns(expected);

			ImportMoqTableResultDataModelView results = await sut.ImportMoqTables(ws, request.Object);

			// Assert all results
			Assert.IsNotNull(results.Result);
			Assert.AreEqual(2, results.Result.Count);
			Assert.IsTrue(results.Result.Any(x => x.Id == -1));
			Assert.IsTrue(results.Result.Any(x => x.Id == -2));

			// Assert results to save (only those with CreateMoqTable Import Type)
			Assert.IsTrue(results.DataToSave().Any());
			Assert.AreEqual(1, results.DataToSave().Count);
			Assert.IsTrue(results.DataToSave().Any(x => x.Id == -1));
			Assert.IsFalse(results.DataToSave().Any(x => x.Id == -2));

			// Assert no errors
			Assert.IsFalse(results.ErrorsOccurred);
		}


		/// <summary>
		/// Test ImportMoqTables returning caught exception
		/// </summary>
		[TestMethod]
		public async Task ImportMoqTables_Test_CaughtException()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1 };
			FullWorkspace ws = new FullWorkspace(workspace);

			// Don't setup request further so it causes an exception
			Mock<HttpRequestBase> request = new Mock<HttpRequestBase>();

			ImportMoqTableResultDataModelView results = await sut.ImportMoqTables(ws, request.Object);

			Assert.IsTrue(results.ErrorsOccurred);
		}

		/// <summary>
		/// Test ImportMoqTables throws exception when ws is null
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task ImportMoqTables_Test_CaughtException_NullWs()
		{
			BOELaborControllerLogic sut = CreateSystem();

			Mock<HttpRequestBase> request = new Mock<HttpRequestBase>();

			ImportMoqTableResultDataModelView results = await sut.ImportMoqTables(null, request.Object);
		}

		/// <summary>
		/// Test ImportMoqTables throws exception when request is null
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public async Task ImportMoqTables_Test_CaughtException_NullRequest()
		{
			BOELaborControllerLogic sut = CreateSystem();

			WorkspaceDTO workspace = new WorkspaceDTO { Id = 1 };
			FullWorkspace ws = new FullWorkspace(workspace);

			ImportMoqTableResultDataModelView results = await sut.ImportMoqTables(ws, null);
		}

		/// <summary>
		/// Test CompleteImportMoqTables
		/// </summary>
		[TestMethod]
		public void CompleteImportMoqTables_Test()
		{
			BOELaborControllerLogic sut = CreateSystem();

			MoqTypeSelection moqType = new MoqTypeSelection() { Id = 1 };
			moqType.TableData.Add(new MoqTableData() { Id = 1 });
			this.moqTypeDataLoader.Setup(x => x.GetById(moqType.Id)).Returns(moqType);

			ICollection<ImportMoqTableResultsModelView> importResults = new Collection<ImportMoqTableResultsModelView>();
			importResults.Add(new ImportMoqTableResultsModelView() { Id = -1, ImportType = 1 });
			importResults.Add(new ImportMoqTableResultsModelView() { Id = -2, ImportType = 3 });

			this.moqTypeDataLoader.Setup(x => x.SaveImportedMoqTypeTables(It.IsAny<MoqTypeSelection>())).Verifiable();

			sut.CompleteImportMoqTables(importResults, moqType.Id);

			this.moqTypeDataLoader.Verify(x => x.SaveImportedMoqTypeTables(It.IsAny<MoqTypeSelection>()), Times.Once());
		}

		/// <summary>
		/// Test CompleteImportMoqTables throws exception when Import Results is null
		/// </summary>
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CompleteImportMoqTables_Test_NullImportResults()
		{
			BOELaborControllerLogic sut = CreateSystem();

			sut.CompleteImportMoqTables(null, 1);
		}

		/// <summary>
		/// Test Refresh SkillMix Calculation with Empty hours
		/// </summary>
		[TestMethod]
		public void RefreshSkillMix_Empty_Test()
		{
			BOELaborControllerLogic sut = CreateSystem();
			RefreshSkillMixModelView result = sut.RefreshSkillMixTables(null, null, null, null, true);
			Assert.IsNotNull(result);
		}

		/// <summary>
		/// Test Refresh SkillMix Calculation with only Historical Hours
		/// </summary>
		[TestMethod]
		public void RefreshSkillMix_HoursOnly_Test()
		{
			BOELaborControllerLogic sut = CreateSystem();
			List<MOQTypeSelectionTableDataResourceHoursDTO> hours = new List<MOQTypeSelectionTableDataResourceHoursDTO>
			{
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 2,
					ResourceName = HISTORICAL_RESOURCE_NAME2,
					TotalHours = 40.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 1,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 100.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 3,
					ResourceName = HISTORICAL_RESOURCE_NAME3,
					TotalHours = 110.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 2,
					MOQTypeSelectionTableDataResourceHoursId = 4,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 50.0m
				}
			};

			RefreshSkillMixModelView result = sut.RefreshSkillMixTables(hours, null, null, null, true);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.SkillMixRows);
			Assert.IsTrue(result.SkillMixRows.Any());
			Assert.AreEqual(4, result.SkillMixRows.Count);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.First().ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME2, result.SkillMixRows.ElementAt(1).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME3, result.SkillMixRows.ElementAt(2).ResourceOld);
			Assert.AreEqual(150.0m, result.SkillMixRows.ElementAt(0).HistoricalHours);
			Assert.AreEqual(40.0m, result.SkillMixRows.ElementAt(1).HistoricalHours);
			Assert.AreEqual(110.0m, result.SkillMixRows.ElementAt(2).HistoricalHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(50.0m, result.SkillMixRows.ElementAt(0).LaborSkillMix);
			Assert.AreEqual(4000.0m / 300m, result.SkillMixRows.ElementAt(1).LaborSkillMix);
			Assert.AreEqual(11000.0m / 300m, result.SkillMixRows.ElementAt(2).LaborSkillMix);
			Assert.IsFalse(result.SkillMixRows.ElementAt(0).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(1).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(2).Included);
		}

		/// <summary>
		/// Test Refresh SkillMix Calculation with Historical Hours and Labor Type info only
		/// </summary>
		[TestMethod]
		public void RefreshSkillMix_HoursLaborOnly_Test()
		{
			BOELaborControllerLogic sut = CreateSystem();
			List<MOQTypeSelectionTableDataResourceHoursDTO> hours = new List<MOQTypeSelectionTableDataResourceHoursDTO>
			{
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 2,
					ResourceName = HISTORICAL_RESOURCE_NAME2,
					TotalHours = 40.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 1,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 100.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 3,
					ResourceName = HISTORICAL_RESOURCE_NAME3,
					TotalHours = 110.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 2,
					MOQTypeSelectionTableDataResourceHoursId = 4,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 50.0m
				}
			};

			List<LaborTypeDataModelView> laborTypes = new List<LaborTypeDataModelView>
			{
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME1,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 60.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "02/2024",
							LaborSpreadValue = 15.0m
						}
					},
					Deleted = false,
					HourSpread = 75.0m,
					RateType = RateType.Hours
				}
			};

			RefreshSkillMixModelView result = sut.RefreshSkillMixTables(hours, laborTypes, null, null, true);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.SkillMixRows);
			Assert.IsTrue(result.SkillMixRows.Any());
			Assert.AreEqual(4, result.SkillMixRows.Count);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.First().ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME2, result.SkillMixRows.ElementAt(1).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME3, result.SkillMixRows.ElementAt(2).ResourceOld);
			Assert.AreEqual(150.0m, result.SkillMixRows.ElementAt(0).HistoricalHours);
			Assert.AreEqual(40.0m, result.SkillMixRows.ElementAt(1).HistoricalHours);
			Assert.AreEqual(110.0m, result.SkillMixRows.ElementAt(2).HistoricalHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(50.0m, result.SkillMixRows.ElementAt(0).LaborSkillMix);
			Assert.AreEqual(4000.0m / 300m, result.SkillMixRows.ElementAt(1).LaborSkillMix);
			Assert.AreEqual(11000.0m / 300m, result.SkillMixRows.ElementAt(2).LaborSkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(0).BOESkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(1).BOESkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(2).BOESkillMix);
			Assert.IsFalse(result.SkillMixRows.ElementAt(0).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(1).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(2).Included);
		}

		/// <summary>
		/// Test Refresh SkillMix Calculation with Historical Hours, Labor Types, and Skill Mix Rows
		/// </summary>
		[TestMethod]
		public void RefreshSkillMix_HoursLaborSkillMixOnly_Test()
		{
			BOELaborControllerLogic sut = CreateSystem();
			List<MOQTypeSelectionTableDataResourceHoursDTO> hours = new List<MOQTypeSelectionTableDataResourceHoursDTO>
			{
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 2,
					ResourceName = HISTORICAL_RESOURCE_NAME2,
					TotalHours = 40.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 1,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 100.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 3,
					ResourceName = HISTORICAL_RESOURCE_NAME3,
					TotalHours = 110.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 2,
					MOQTypeSelectionTableDataResourceHoursId = 4,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 50.0m
				}
			};

			List<LaborTypeDataModelView> laborTypes = new List<LaborTypeDataModelView>
			{
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME1,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 60.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "02/2024",
							LaborSpreadValue = 15.0m
						}
					},
					Deleted = false,
					HourSpread = 75.0m,
					RateType = RateType.Hours
				}
			};

			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = HISTORICAL_RESOURCE_NAME1,
					ResourceNew = RESOURCE_NAME1,
					Included = true
				}
			};

			RefreshSkillMixModelView result = sut.RefreshSkillMixTables(hours, laborTypes, skillmix, null, true);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.SkillMixRows);
			Assert.IsTrue(result.SkillMixRows.Any());
			Assert.AreEqual(4, result.SkillMixRows.Count);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.First().ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME2, result.SkillMixRows.ElementAt(1).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME3, result.SkillMixRows.ElementAt(2).ResourceOld);
			Assert.AreEqual(150.0m, result.SkillMixRows.ElementAt(0).HistoricalHours);
			Assert.AreEqual(40.0m, result.SkillMixRows.ElementAt(1).HistoricalHours);
			Assert.AreEqual(110.0m, result.SkillMixRows.ElementAt(2).HistoricalHours);
			Assert.AreEqual(75.0m, result.SkillMixRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(50.0m, result.SkillMixRows.ElementAt(0).LaborSkillMix);
			Assert.AreEqual(4000.0m / 300m, result.SkillMixRows.ElementAt(1).LaborSkillMix);
			Assert.AreEqual(11000.0m / 300m, result.SkillMixRows.ElementAt(2).LaborSkillMix);
			Assert.AreEqual(100.0m, result.SkillMixRows.ElementAt(0).BOESkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(1).BOESkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(2).BOESkillMix);
			Assert.AreEqual(RESOURCE_NAME1, result.SkillMixRows.First().ResourceNew);
			Assert.IsTrue(result.SkillMixRows.ElementAt(0).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(1).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(2).Included);
		}

		/// <summary>
		/// Test Refresh SkillMix Calculation with Historical Hours, Labor Types, and INVALID Skill Mix Row
		/// </summary>
		[TestMethod]
		public void RefreshSkillMix_HoursLaborSkillMixOnly_Test2()
		{
			BOELaborControllerLogic sut = CreateSystem();
			List<MOQTypeSelectionTableDataResourceHoursDTO> hours = new List<MOQTypeSelectionTableDataResourceHoursDTO>
			{
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 2,
					ResourceName = HISTORICAL_RESOURCE_NAME2,
					TotalHours = 40.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 1,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 100.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 3,
					ResourceName = HISTORICAL_RESOURCE_NAME3,
					TotalHours = 110.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 2,
					MOQTypeSelectionTableDataResourceHoursId = 4,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 50.0m
				}
			};

			List<LaborTypeDataModelView> laborTypes = new List<LaborTypeDataModelView>
			{
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME3,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 60.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "02/2024",
							LaborSpreadValue = 15.0m
						}
					},
					Deleted = false,
					HourSpread = 75.0m,
					RateType = RateType.Hours
				}
			};

			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = HISTORICAL_RESOURCE_NAME1,
					ResourceNew = RESOURCE_NAME1,
					Included = true
				}
			};

			RefreshSkillMixModelView result = sut.RefreshSkillMixTables(hours, laborTypes, skillmix, null, true);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.SkillMixRows);
			Assert.IsTrue(result.SkillMixRows.Any());
			Assert.AreEqual(4, result.SkillMixRows.Count);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.First().ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME2, result.SkillMixRows.ElementAt(1).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME3, result.SkillMixRows.ElementAt(2).ResourceOld);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(0).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(1).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(2).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(3).ResourceNew);
			Assert.AreEqual(150.0m, result.SkillMixRows.ElementAt(0).HistoricalHours);
			Assert.AreEqual(40.0m, result.SkillMixRows.ElementAt(1).HistoricalHours);
			Assert.AreEqual(110.0m, result.SkillMixRows.ElementAt(2).HistoricalHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(50.0m, result.SkillMixRows.ElementAt(0).LaborSkillMix);
			Assert.AreEqual(4000.0m / 300m, result.SkillMixRows.ElementAt(1).LaborSkillMix);
			Assert.AreEqual(11000.0m / 300m, result.SkillMixRows.ElementAt(2).LaborSkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(0).BOESkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(1).BOESkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(2).BOESkillMix);
			Assert.AreEqual(string.Empty, result.SkillMixRows.First().ResourceNew);
			Assert.IsFalse(result.SkillMixRows.ElementAt(0).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(1).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(2).Included);
		}

		/// <summary>
		/// Test Refresh SkillMix Calculation with Historical Hours, Labor Types, Skill Mix Rows, and Common Disclosure Rows
		/// </summary>
		[TestMethod]
		public void RefreshSkillMix_All_Test()
		{
			BOELaborControllerLogic sut = CreateSystem();

			List<MOQTypeSelectionTableDataResourceHoursDTO> hours = new List<MOQTypeSelectionTableDataResourceHoursDTO>
			{
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 2,
					ResourceName = HISTORICAL_RESOURCE_NAME2,
					TotalHours = 40.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 1,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 100.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 3,
					ResourceName = HISTORICAL_RESOURCE_NAME3,
					TotalHours = 110.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 2,
					MOQTypeSelectionTableDataResourceHoursId = 4,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 50.0m
				}
			};

			List<LaborTypeDataModelView> laborTypes = new List<LaborTypeDataModelView>
			{
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME1,
					BusinessResourceCodeName = BRC_RESOURCE_NAME1,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 60.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 15.0m
						}
					},
					Deleted = false,
					HourSpread = 75.0m,
					RateType = RateType.Hours
				}
			};

			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = HISTORICAL_RESOURCE_NAME1,
					ResourceNew = RESOURCE_NAME1,
					Included = true,
					Rationale = Rationale1
				}
			};

			List<CommonDisclosureModelView> commonDisclosures = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = RESOURCE_NAME1,
					BusinessResourceID = BRC_RESOURCE_NAME1,
					Included = true,
					Rationale = Rationale2
				}
			};

			RefreshSkillMixModelView result = sut.RefreshSkillMixTables(hours, laborTypes, skillmix, commonDisclosures, true);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.SkillMixRows);
			Assert.IsTrue(result.SkillMixRows.Any());
			Assert.AreEqual(4, result.SkillMixRows.Count);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.ElementAt(0).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME2, result.SkillMixRows.ElementAt(1).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME3, result.SkillMixRows.ElementAt(2).ResourceOld);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(3).ResourceOld);
			Assert.AreEqual(150.0m, result.SkillMixRows.ElementAt(0).HistoricalHours);
			Assert.AreEqual(40.0m, result.SkillMixRows.ElementAt(1).HistoricalHours);
			Assert.AreEqual(110.0m, result.SkillMixRows.ElementAt(2).HistoricalHours);
			Assert.AreEqual(60.0m, result.SkillMixRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(50.0m, result.SkillMixRows.ElementAt(0).LaborSkillMix);
			Assert.AreEqual(4000.0m / 300m, result.SkillMixRows.ElementAt(1).LaborSkillMix);
			Assert.AreEqual(11000.0m / 300m, result.SkillMixRows.ElementAt(2).LaborSkillMix);
			Assert.AreEqual(100.0m, result.SkillMixRows.ElementAt(0).BOESkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(1).BOESkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(2).BOESkillMix);
			Assert.IsTrue(result.SkillMixRows.ElementAt(0).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(1).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(2).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(3).Included);
			Assert.AreEqual(Rationale1, result.SkillMixRows.ElementAt(0).Rationale);

			Assert.AreEqual(1, result.CommonDisclosureRows.Count);
			Assert.AreEqual(RESOURCE_NAME1, result.CommonDisclosureRows.First().ResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME1, result.CommonDisclosureRows.First().BusinessResourceID);
			Assert.AreEqual(true, result.CommonDisclosureRows.First().Included);
			Assert.AreEqual(15.0m, result.CommonDisclosureRows.First().ProposedHours);
			Assert.AreEqual(100.0m, result.CommonDisclosureRows.First().BOESkillMix);
			Assert.AreEqual(100.0m, result.CommonDisclosureRows.First().LaborSkillMix);
			Assert.AreEqual(Rationale2, result.CommonDisclosureRows.First().Rationale);
		}

		/// <summary>
		/// Test Refresh SkillMix Calculation with Historical Hours, Labor Types, Skill Mix Rows, and split BRC Common Disclosure Rows
		/// </summary>
		[TestMethod]
		public void RefreshSkillMix_All_Test2()
		{
			BOELaborControllerLogic sut = CreateSystem();

			List<MOQTypeSelectionTableDataResourceHoursDTO> hours = new List<MOQTypeSelectionTableDataResourceHoursDTO>
			{
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 2,
					ResourceName = HISTORICAL_RESOURCE_NAME2,
					TotalHours = 40.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 1,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 100.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 3,
					ResourceName = HISTORICAL_RESOURCE_NAME3,
					TotalHours = 110.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 2,
					MOQTypeSelectionTableDataResourceHoursId = 4,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 50.0m
				}
			};

			List<LaborTypeDataModelView> laborTypes = new List<LaborTypeDataModelView>
			{
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME1,
					BusinessResourceCodeName = BRC_RESOURCE_NAME1,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 60.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 15.0m
						}
					},
					Deleted = false,
					HourSpread = 75.0m,
					RateType = RateType.Hours
				},
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME1,
					BusinessResourceCodeName = BRC_RESOURCE_NAME2,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 30.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 30.0m
						}
					},
					Deleted = false,
					HourSpread = 60.0m,
					RateType = RateType.Hours
				},
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME3,
					BusinessResourceCodeName = BRC_RESOURCE_NAME3,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 10.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 20.0m
						}
					},
					Deleted = false,
					HourSpread = 30.0m,
					RateType = RateType.Hours
				}
			};

			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = HISTORICAL_RESOURCE_NAME1,
					ResourceNew = RESOURCE_NAME1,
					Included = true,
					Rationale = Rationale1
				}
			};

			List<CommonDisclosureModelView> commonDisclosures = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = RESOURCE_NAME1,
					BusinessResourceID = BRC_RESOURCE_NAME1,
					Included = true,
					Rationale = Rationale2
				},
				new CommonDisclosureModelView {
					ResourceID = RESOURCE_NAME1,
					BusinessResourceID = BRC_RESOURCE_NAME2,
					Included = true,
					Rationale = Rationale3
				}
			};

			RefreshSkillMixModelView result = sut.RefreshSkillMixTables(hours, laborTypes, skillmix, commonDisclosures, true);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.SkillMixRows);
			Assert.IsTrue(result.SkillMixRows.Any());
			Assert.AreEqual(4, result.SkillMixRows.Count);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.First().ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME2, result.SkillMixRows.ElementAt(1).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME3, result.SkillMixRows.ElementAt(2).ResourceOld);
			Assert.AreEqual(RESOURCE_NAME1, result.SkillMixRows.ElementAt(0).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(1).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(2).ResourceNew);
			Assert.AreEqual(150.0m, result.SkillMixRows.ElementAt(0).HistoricalHours);
			Assert.AreEqual(40.0m, result.SkillMixRows.ElementAt(1).HistoricalHours);
			Assert.AreEqual(110.0m, result.SkillMixRows.ElementAt(2).HistoricalHours);
			Assert.AreEqual(90.0m, result.SkillMixRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(1).ProposedHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(2).ProposedHours);
			Assert.AreEqual(50.0m, result.SkillMixRows.ElementAt(0).LaborSkillMix);
			Assert.AreEqual(4000.0m / 300m, result.SkillMixRows.ElementAt(1).LaborSkillMix);
			Assert.AreEqual(11000.0m / 300m, result.SkillMixRows.ElementAt(2).LaborSkillMix);
			Assert.AreEqual(100.0m, result.SkillMixRows.ElementAt(0).BOESkillMix);
			Assert.AreEqual(0m, result.SkillMixRows.ElementAt(1).BOESkillMix);
			Assert.AreEqual(0m, result.SkillMixRows.ElementAt(2).BOESkillMix);
			Assert.IsTrue(result.SkillMixRows.ElementAt(0).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(1).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(2).Included);
			Assert.AreEqual(Rationale1, result.SkillMixRows.ElementAt(0).Rationale);

			Assert.AreEqual(2, result.CommonDisclosureRows.Count);
			Assert.AreEqual(RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(0).ResourceID);
			Assert.AreEqual(RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(1).ResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(0).BusinessResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME2, result.CommonDisclosureRows.ElementAt(1).BusinessResourceID);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(0).Included);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(0).Included);
			Assert.AreEqual(15.0m, result.CommonDisclosureRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(30.0m, result.CommonDisclosureRows.ElementAt(1).ProposedHours);
			AssertHelpers.AssertAreEqualEpsilon(15.0m / 45.0m * 100m, result.CommonDisclosureRows.ElementAt(0).BOESkillMix.Value);
			AssertHelpers.AssertAreEqualEpsilon(30.0m / 45.0m * 100m, result.CommonDisclosureRows.ElementAt(1).BOESkillMix.Value);
			Assert.AreEqual(Rationale2, result.CommonDisclosureRows.ElementAt(0).Rationale);
			Assert.AreEqual(Rationale3, result.CommonDisclosureRows.ElementAt(1).Rationale);

			// Check CommonDisclosureTotals
			CheckSkillMixTotals(result);

			// labor skill mix is percentage the brc is linked to in the labor types for this resource multiplied by historical hours in skill mix for this resource
			AssertHelpers.AssertAreEqualEpsilon(75.0m / 135.0m * 150.0m * 100m / result.CommonDisclosureTotals.HistoricalHours, result.CommonDisclosureRows.ElementAt(0).LaborSkillMix);
			AssertHelpers.AssertAreEqualEpsilon(60.0m / 135.0m * 150.0m * 100m / result.CommonDisclosureTotals.HistoricalHours, result.CommonDisclosureRows.ElementAt(1).LaborSkillMix);


		}

		/// <summary>
		/// Test Refresh SkillMix Calculation with Hisorical Hours, labor types, and split Skill Mix Rows
		/// </summary>
		[TestMethod]
		public void RefreshSkillMix_SplitHistorical()
		{
			BOELaborControllerLogic sut = CreateSystem();

			List<MOQTypeSelectionTableDataResourceHoursDTO> hours = new List<MOQTypeSelectionTableDataResourceHoursDTO>
			{
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 2,
					ResourceName = HISTORICAL_RESOURCE_NAME2,
					TotalHours = 40.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 1,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 100.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 3,
					ResourceName = HISTORICAL_RESOURCE_NAME3,
					TotalHours = 110.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 2,
					MOQTypeSelectionTableDataResourceHoursId = 4,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 50.0m
				}
			};

			List<LaborTypeDataModelView> laborTypes = new List<LaborTypeDataModelView>
			{
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME1,
					BusinessResourceCodeName = BRC_RESOURCE_NAME1,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 60.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 15.0m
						}
					},
					Deleted = false,
					HourSpread = 75.0m,
					RateType = RateType.Hours
				},
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME1,
					BusinessResourceCodeName = BRC_RESOURCE_NAME2,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 30.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 30.0m
						}
					},
					Deleted = false,
					HourSpread = 60.0m,
					RateType = RateType.Hours
				},
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME3,
					BusinessResourceCodeName = BRC_RESOURCE_NAME3,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 10.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 20.0m
						}
					},
					Deleted = false,
					HourSpread = 30.0m,
					RateType = RateType.Hours
				}
			};

			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = HISTORICAL_RESOURCE_NAME1,
					ResourceNew = RESOURCE_NAME1,
					Included = true,
					Rationale = Rationale1
				},
				new SkillMixModelView {
					ResourceOld = HISTORICAL_RESOURCE_NAME1,
					ResourceNew = RESOURCE_NAME3,
					Included = true,
					Rationale = Rationale2
				}
			};

			List<CommonDisclosureModelView> commonDisclosures = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = RESOURCE_NAME1,
					BusinessResourceID = BRC_RESOURCE_NAME1,
					Included = true,
					Rationale = Rationale2
				},
				new CommonDisclosureModelView {
					ResourceID = RESOURCE_NAME1,
					BusinessResourceID = BRC_RESOURCE_NAME2,
					Included = true,
					Rationale = Rationale3
				}
			};

			RefreshSkillMixModelView result = sut.RefreshSkillMixTables(hours, laborTypes, skillmix, commonDisclosures, true);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.SkillMixRows);
			Assert.IsTrue(result.SkillMixRows.Any());
			Assert.AreEqual(5, result.SkillMixRows.Count);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.First().ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.ElementAt(1).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME2, result.SkillMixRows.ElementAt(2).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME3, result.SkillMixRows.ElementAt(3).ResourceOld);
			Assert.AreEqual(RESOURCE_NAME1, result.SkillMixRows.ElementAt(0).ResourceNew);
			Assert.AreEqual(RESOURCE_NAME3, result.SkillMixRows.ElementAt(1).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(2).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(3).ResourceNew);

			Assert.AreEqual(150.0m, result.SkillMixRows.ElementAt(0).HistoricalHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(1).HistoricalHours);
			Assert.AreEqual(40.0m, result.SkillMixRows.ElementAt(2).HistoricalHours);
			Assert.AreEqual(110.0m, result.SkillMixRows.ElementAt(3).HistoricalHours);
			Assert.AreEqual(90.0m, result.SkillMixRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(10.0m, result.SkillMixRows.ElementAt(1).ProposedHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(2).ProposedHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(3).ProposedHours);
			Assert.AreEqual(50.0m, result.SkillMixRows.ElementAt(0).LaborSkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(1).LaborSkillMix);
			Assert.AreEqual(4000.0m / 300m, result.SkillMixRows.ElementAt(2).LaborSkillMix);
			Assert.AreEqual(11000.0m / 300m, result.SkillMixRows.ElementAt(3).LaborSkillMix);
			Assert.AreEqual(90.0m, result.SkillMixRows.ElementAt(0).BOESkillMix);
			Assert.AreEqual(10m, result.SkillMixRows.ElementAt(1).BOESkillMix);
			Assert.AreEqual(0m, result.SkillMixRows.ElementAt(2).BOESkillMix);
			Assert.AreEqual(0m, result.SkillMixRows.ElementAt(3).BOESkillMix);
			Assert.IsTrue(result.SkillMixRows.ElementAt(0).Included);
			Assert.IsTrue(result.SkillMixRows.ElementAt(1).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(2).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(3).Included);

			Assert.AreEqual(Rationale1, result.SkillMixRows.ElementAt(0).Rationale);
			Assert.AreEqual(Rationale2, result.SkillMixRows.ElementAt(1).Rationale);

			Assert.AreEqual(3, result.CommonDisclosureRows.Count);
			Assert.AreEqual(RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(0).ResourceID);
			Assert.AreEqual(RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(1).ResourceID);
			Assert.AreEqual(RESOURCE_NAME3, result.CommonDisclosureRows.ElementAt(2).ResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(0).BusinessResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME2, result.CommonDisclosureRows.ElementAt(1).BusinessResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME3, result.CommonDisclosureRows.ElementAt(2).BusinessResourceID);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(0).Included);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(1).Included);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(2).Included);
			Assert.AreEqual(15.0m, result.CommonDisclosureRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(30.0m, result.CommonDisclosureRows.ElementAt(1).ProposedHours);
			Assert.AreEqual(20.0m, result.CommonDisclosureRows.ElementAt(2).ProposedHours);

			AssertHelpers.AssertAreEqualEpsilon(15.0m / 65.0m * 100m, result.CommonDisclosureRows.ElementAt(0).BOESkillMix.Value);
			AssertHelpers.AssertAreEqualEpsilon(30.0m / 65.0m * 100m, result.CommonDisclosureRows.ElementAt(1).BOESkillMix.Value);
			AssertHelpers.AssertAreEqualEpsilon(20.0m / 65.0m * 100m, result.CommonDisclosureRows.ElementAt(2).BOESkillMix.Value);
			Assert.AreEqual(Rationale2, result.CommonDisclosureRows.ElementAt(0).Rationale);
			Assert.AreEqual(Rationale3, result.CommonDisclosureRows.ElementAt(1).Rationale);
			Assert.AreEqual(string.Empty, result.CommonDisclosureRows.ElementAt(2).Rationale);

			// Check CommonDisclosureTotals
			CheckSkillMixTotals(result);

			// labor skill mix is percentage the brc is linked to in the labor types for this resource multiplied by historical hours in skill mix for this resource
			AssertHelpers.AssertAreEqualEpsilon(75.0m / 135.0m * 150.0m * 100m / result.CommonDisclosureTotals.HistoricalHours, result.CommonDisclosureRows.ElementAt(0).LaborSkillMix);
			AssertHelpers.AssertAreEqualEpsilon(60.0m / 135.0m * 150.0m * 100m / result.CommonDisclosureTotals.HistoricalHours, result.CommonDisclosureRows.ElementAt(1).LaborSkillMix);
			AssertHelpers.AssertAreEqualEpsilon(0m * 100m / result.CommonDisclosureTotals.HistoricalHours, result.CommonDisclosureRows.ElementAt(2).LaborSkillMix);
		}

		/// <summary>
		/// Test Refresh SkillMix Calculation with removing Invalid data from Common Disclosure row (resource)
		/// </summary>
		[TestMethod]
		public void RefreshSkillMix_RemoveFromCDInvalidResource()
		{
			BOELaborControllerLogic sut = CreateSystem();

			List<MOQTypeSelectionTableDataResourceHoursDTO> hours = new List<MOQTypeSelectionTableDataResourceHoursDTO>
			{
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 2,
					ResourceName = HISTORICAL_RESOURCE_NAME2,
					TotalHours = 40.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 1,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 100.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 3,
					ResourceName = HISTORICAL_RESOURCE_NAME3,
					TotalHours = 110.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 2,
					MOQTypeSelectionTableDataResourceHoursId = 4,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 50.0m
				}
			};

			List<LaborTypeDataModelView> laborTypes = new List<LaborTypeDataModelView>
			{
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME1,
					BusinessResourceCodeName = BRC_RESOURCE_NAME1,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 60.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 15.0m
						}
					},
					Deleted = false,
					HourSpread = 75.0m,
					RateType = RateType.Hours
				},
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME1,
					BusinessResourceCodeName = BRC_RESOURCE_NAME2,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 30.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 30.0m
						}
					},
					Deleted = false,
					HourSpread = 60.0m,
					RateType = RateType.Hours
				},
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME3,
					BusinessResourceCodeName = BRC_RESOURCE_NAME3,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 10.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 20.0m
						}
					},
					Deleted = false,
					HourSpread = 30.0m,
					RateType = RateType.Hours
				}
			};

			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = HISTORICAL_RESOURCE_NAME1,
					ResourceNew = RESOURCE_NAME1,
					Included = true,
					Rationale = Rationale1
				},
				new SkillMixModelView {
					ResourceOld = HISTORICAL_RESOURCE_NAME1,
					ResourceNew = RESOURCE_NAME3,
					Included = true,
					Rationale = Rationale2
				}
			};

			List<CommonDisclosureModelView> commonDisclosures = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = RESOURCE_NAME1,
					BusinessResourceID = BRC_RESOURCE_NAME1,
					Included = true,
					Rationale = Rationale2
				},
				new CommonDisclosureModelView {
					ResourceID = RESOURCE_NAME1,
					BusinessResourceID = BRC_RESOURCE_NAME2,
					Included = true,
					Rationale = Rationale3
				},
				new CommonDisclosureModelView {
					ResourceID = RESOURCE_NAME2,
					BusinessResourceID = BRC_RESOURCE_NAME2,
					Included = true,
					Rationale = Rationale3
				}
			};

			RefreshSkillMixModelView result = sut.RefreshSkillMixTables(hours, laborTypes, skillmix, commonDisclosures, true);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.SkillMixRows);
			Assert.IsTrue(result.SkillMixRows.Any());
			Assert.AreEqual(5, result.SkillMixRows.Count);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.First().ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.ElementAt(1).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME2, result.SkillMixRows.ElementAt(2).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME3, result.SkillMixRows.ElementAt(3).ResourceOld);
			Assert.AreEqual(RESOURCE_NAME1, result.SkillMixRows.ElementAt(0).ResourceNew);
			Assert.AreEqual(RESOURCE_NAME3, result.SkillMixRows.ElementAt(1).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(2).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(3).ResourceNew);

			Assert.AreEqual(150.0m, result.SkillMixRows.ElementAt(0).HistoricalHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(1).HistoricalHours);
			Assert.AreEqual(40.0m, result.SkillMixRows.ElementAt(2).HistoricalHours);
			Assert.AreEqual(110.0m, result.SkillMixRows.ElementAt(3).HistoricalHours);
			Assert.AreEqual(90.0m, result.SkillMixRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(10.0m, result.SkillMixRows.ElementAt(1).ProposedHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(2).ProposedHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(3).ProposedHours);
			Assert.AreEqual(50.0m, result.SkillMixRows.ElementAt(0).LaborSkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(1).LaborSkillMix);
			Assert.AreEqual(4000.0m / 300m, result.SkillMixRows.ElementAt(2).LaborSkillMix);
			Assert.AreEqual(11000.0m / 300m, result.SkillMixRows.ElementAt(3).LaborSkillMix);
			Assert.AreEqual(90.0m, result.SkillMixRows.ElementAt(0).BOESkillMix);
			Assert.AreEqual(10m, result.SkillMixRows.ElementAt(1).BOESkillMix);
			Assert.AreEqual(0m, result.SkillMixRows.ElementAt(2).BOESkillMix);
			Assert.AreEqual(0m, result.SkillMixRows.ElementAt(3).BOESkillMix);
			Assert.IsTrue(result.SkillMixRows.ElementAt(0).Included);
			Assert.IsTrue(result.SkillMixRows.ElementAt(1).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(2).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(3).Included);
			Assert.AreEqual(Rationale1, result.SkillMixRows.ElementAt(0).Rationale);
			Assert.AreEqual(Rationale2, result.SkillMixRows.ElementAt(1).Rationale);

			Assert.AreEqual(3, result.CommonDisclosureRows.Count);
			Assert.AreEqual(RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(0).ResourceID);
			Assert.AreEqual(RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(1).ResourceID);
			Assert.AreEqual(RESOURCE_NAME3, result.CommonDisclosureRows.ElementAt(2).ResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(0).BusinessResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME2, result.CommonDisclosureRows.ElementAt(1).BusinessResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME3, result.CommonDisclosureRows.ElementAt(2).BusinessResourceID);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(0).Included);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(1).Included);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(2).Included);
			Assert.AreEqual(15.0m, result.CommonDisclosureRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(30.0m, result.CommonDisclosureRows.ElementAt(1).ProposedHours);
			Assert.AreEqual(20.0m, result.CommonDisclosureRows.ElementAt(2).ProposedHours);

			AssertHelpers.AssertAreEqualEpsilon(15.0m / 65.0m * 100m, result.CommonDisclosureRows.ElementAt(0).BOESkillMix.Value);
			AssertHelpers.AssertAreEqualEpsilon(30.0m / 65.0m * 100m, result.CommonDisclosureRows.ElementAt(1).BOESkillMix.Value);
			AssertHelpers.AssertAreEqualEpsilon(20.0m / 65.0m * 100m, result.CommonDisclosureRows.ElementAt(2).BOESkillMix.Value);
			Assert.AreEqual(Rationale2, result.CommonDisclosureRows.ElementAt(0).Rationale);
			Assert.AreEqual(Rationale3, result.CommonDisclosureRows.ElementAt(1).Rationale);
			Assert.AreEqual(string.Empty, result.CommonDisclosureRows.ElementAt(2).Rationale);

			// Check CommonDisclosureTotals
			CheckSkillMixTotals(result);

			// labor skill mix is percentage the brc is linked to in the labor types for this resource multiplied by historical hours in skill mix for this resource
			AssertHelpers.AssertAreEqualEpsilon(75.0m / 135.0m * 150.0m * 100m / result.CommonDisclosureTotals.HistoricalHours, result.CommonDisclosureRows.ElementAt(0).LaborSkillMix);
			AssertHelpers.AssertAreEqualEpsilon(60.0m / 135.0m * 150.0m * 100m / result.CommonDisclosureTotals.HistoricalHours, result.CommonDisclosureRows.ElementAt(1).LaborSkillMix);
			AssertHelpers.AssertAreEqualEpsilon(0m * 100m / result.CommonDisclosureTotals.HistoricalHours, result.CommonDisclosureRows.ElementAt(2).LaborSkillMix);
		}

		/// <summary>
		/// Test Refresh SkillMix Calculation with removing Invalid data from Common Disclosure row (BRC)
		/// </summary>
		[TestMethod]
		public void RefreshSkillMix_RemoveFromCDInvalidBRC()
		{
			BOELaborControllerLogic sut = CreateSystem();

			List<MOQTypeSelectionTableDataResourceHoursDTO> hours = new List<MOQTypeSelectionTableDataResourceHoursDTO>
			{
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 2,
					ResourceName = HISTORICAL_RESOURCE_NAME2,
					TotalHours = 40.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 1,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 100.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 3,
					ResourceName = HISTORICAL_RESOURCE_NAME3,
					TotalHours = 110.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 2,
					MOQTypeSelectionTableDataResourceHoursId = 4,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 50.0m
				}
			};

			List<LaborTypeDataModelView> laborTypes = new List<LaborTypeDataModelView>
			{
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME1,
					BusinessResourceCodeName = BRC_RESOURCE_NAME1,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 60.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 15.0m
						}
					},
					Deleted = false,
					HourSpread = 75.0m,
					RateType = RateType.Hours
				},
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME1,
					BusinessResourceCodeName = BRC_RESOURCE_NAME2,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 30.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 30.0m
						}
					},
					Deleted = false,
					HourSpread = 60.0m,
					RateType = RateType.Hours
				},
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME3,
					BusinessResourceCodeName = BRC_RESOURCE_NAME3,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 10.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 20.0m
						}
					},
					Deleted = false,
					HourSpread = 30.0m,
					RateType = RateType.Hours
				}
			};

			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = HISTORICAL_RESOURCE_NAME1,
					ResourceNew = RESOURCE_NAME1,
					Included = true,
					Rationale = Rationale1
				},
				new SkillMixModelView {
					ResourceOld = HISTORICAL_RESOURCE_NAME1,
					ResourceNew = RESOURCE_NAME3,
					Included = true,
					Rationale = Rationale2
				}
			};

			List<CommonDisclosureModelView> commonDisclosures = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = RESOURCE_NAME1,
					BusinessResourceID = BRC_RESOURCE_NAME1,
					Included = true,
					Rationale = Rationale2
				},
				new CommonDisclosureModelView {
					ResourceID = RESOURCE_NAME1,
					BusinessResourceID = BRC_RESOURCE_NAME2,
					Included = true,
					Rationale = Rationale3
				},
				new CommonDisclosureModelView {
					ResourceID = RESOURCE_NAME3,
					BusinessResourceID = BRC_RESOURCE_NAME2,
					Included = true,
					Rationale = Rationale3
				}
			};

			RefreshSkillMixModelView result = sut.RefreshSkillMixTables(hours, laborTypes, skillmix, commonDisclosures, true);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.SkillMixRows);
			Assert.IsTrue(result.SkillMixRows.Any());
			Assert.AreEqual(5, result.SkillMixRows.Count);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.First().ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.ElementAt(1).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME2, result.SkillMixRows.ElementAt(2).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME3, result.SkillMixRows.ElementAt(3).ResourceOld);
			Assert.AreEqual(RESOURCE_NAME1, result.SkillMixRows.ElementAt(0).ResourceNew);
			Assert.AreEqual(RESOURCE_NAME3, result.SkillMixRows.ElementAt(1).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(2).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(3).ResourceNew);

			Assert.AreEqual(150.0m, result.SkillMixRows.ElementAt(0).HistoricalHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(1).HistoricalHours);
			Assert.AreEqual(40.0m, result.SkillMixRows.ElementAt(2).HistoricalHours);
			Assert.AreEqual(110.0m, result.SkillMixRows.ElementAt(3).HistoricalHours);
			Assert.AreEqual(90.0m, result.SkillMixRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(10.0m, result.SkillMixRows.ElementAt(1).ProposedHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(2).ProposedHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(3).ProposedHours);
			Assert.AreEqual(50.0m, result.SkillMixRows.ElementAt(0).LaborSkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(1).LaborSkillMix);
			Assert.AreEqual(4000.0m / 300m, result.SkillMixRows.ElementAt(2).LaborSkillMix);
			Assert.AreEqual(11000.0m / 300m, result.SkillMixRows.ElementAt(3).LaborSkillMix);
			Assert.AreEqual(90.0m, result.SkillMixRows.ElementAt(0).BOESkillMix);
			Assert.AreEqual(10m, result.SkillMixRows.ElementAt(1).BOESkillMix);
			Assert.AreEqual(0m, result.SkillMixRows.ElementAt(2).BOESkillMix);
			Assert.AreEqual(0m, result.SkillMixRows.ElementAt(3).BOESkillMix);
			Assert.IsTrue(result.SkillMixRows.ElementAt(0).Included);
			Assert.IsTrue(result.SkillMixRows.ElementAt(1).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(2).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(3).Included);
			Assert.AreEqual(Rationale1, result.SkillMixRows.ElementAt(0).Rationale);
			Assert.AreEqual(Rationale2, result.SkillMixRows.ElementAt(1).Rationale);

			Assert.AreEqual(3, result.CommonDisclosureRows.Count);
			Assert.AreEqual(RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(0).ResourceID);
			Assert.AreEqual(RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(1).ResourceID);
			Assert.AreEqual(RESOURCE_NAME3, result.CommonDisclosureRows.ElementAt(2).ResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(0).BusinessResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME2, result.CommonDisclosureRows.ElementAt(1).BusinessResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME3, result.CommonDisclosureRows.ElementAt(2).BusinessResourceID);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(0).Included);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(1).Included);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(2).Included);
			Assert.AreEqual(15.0m, result.CommonDisclosureRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(30.0m, result.CommonDisclosureRows.ElementAt(1).ProposedHours);
			Assert.AreEqual(20.0m, result.CommonDisclosureRows.ElementAt(2).ProposedHours);

			AssertHelpers.AssertAreEqualEpsilon(15.0m / 65.0m * 100m, result.CommonDisclosureRows.ElementAt(0).BOESkillMix.Value);
			AssertHelpers.AssertAreEqualEpsilon(30.0m / 65.0m * 100m, result.CommonDisclosureRows.ElementAt(1).BOESkillMix.Value);
			AssertHelpers.AssertAreEqualEpsilon(20.0m / 65.0m * 100m, result.CommonDisclosureRows.ElementAt(2).BOESkillMix.Value);
			Assert.AreEqual(Rationale2, result.CommonDisclosureRows.ElementAt(0).Rationale);
			Assert.AreEqual(Rationale3, result.CommonDisclosureRows.ElementAt(1).Rationale);
			Assert.AreEqual(string.Empty, result.CommonDisclosureRows.ElementAt(2).Rationale);

			// Check CommonDisclosureTotals
			CheckSkillMixTotals(result);

			// labor skill mix is percentage the brc is linked to in the labor types for this resource multiplied by historical hours in skill mix for this resource
			AssertHelpers.AssertAreEqualEpsilon(75.0m / 135.0m * 150.0m * 100m / result.CommonDisclosureTotals.HistoricalHours, result.CommonDisclosureRows.ElementAt(0).LaborSkillMix);
			AssertHelpers.AssertAreEqualEpsilon(60.0m / 135.0m * 150.0m * 100m / result.CommonDisclosureTotals.HistoricalHours, result.CommonDisclosureRows.ElementAt(1).LaborSkillMix);
			AssertHelpers.AssertAreEqualEpsilon(0m * 100m / result.CommonDisclosureTotals.HistoricalHours, result.CommonDisclosureRows.ElementAt(2).LaborSkillMix);
		}

		/// <summary>
		/// Test Refresh SkillMix Calculation with no resource set for BRC
		/// </summary>
		[TestMethod]
		public void RefreshSkillMix_BRC_NoResource_Test1()
		{
			BOELaborControllerLogic sut = CreateSystem();

			List<MOQTypeSelectionTableDataResourceHoursDTO> hours = new List<MOQTypeSelectionTableDataResourceHoursDTO>
			{
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 2,
					ResourceName = HISTORICAL_RESOURCE_NAME2,
					TotalHours = 40.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 1,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 100.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 1,
					MOQTypeSelectionTableDataResourceHoursId = 3,
					ResourceName = HISTORICAL_RESOURCE_NAME3,
					TotalHours = 110.0m
				},
				new MOQTypeSelectionTableDataResourceHoursDTO
				{
					MOQTypeSelectionTableDataId = 2,
					MOQTypeSelectionTableDataResourceHoursId = 4,
					ResourceName = HISTORICAL_RESOURCE_NAME1,
					TotalHours = 50.0m
				}
			};

			List<LaborTypeDataModelView> laborTypes = new List<LaborTypeDataModelView>
			{
				new LaborTypeDataModelView
				{
					ResourceName = string.Empty,
					BusinessResourceCodeName = BRC_RESOURCE_NAME1,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2033",
							LaborSpreadValue = 60.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 15.0m
						}
					},
					Deleted = false,
					HourSpread = 75.0m,
					RateType = RateType.Hours
				},
				new LaborTypeDataModelView
				{
					ResourceName = null,
					BusinessResourceCodeName = BRC_RESOURCE_NAME2,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2033",
							LaborSpreadValue = 30.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 30.0m
						}
					},
					Deleted = false,
					HourSpread = 60.0m,
					RateType = RateType.Hours
				},
				new LaborTypeDataModelView
				{
					ResourceName = RESOURCE_NAME3,
					BusinessResourceCodeName = BRC_RESOURCE_NAME3,
					Spreads = new List<LaborSpreadDataModelView> {
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "01/2024",
							LaborSpreadValue = 10.0m
						},
						new LaborSpreadDataModelView
						{
							LaborSpreadDate = "12/2032",
							LaborSpreadValue = 20.0m
						}
					},
					Deleted = false,
					HourSpread = 30.0m,
					RateType = RateType.Hours
				}
			};

			List<SkillMixModelView> skillmix = new List<SkillMixModelView>
			{
				new SkillMixModelView {
					ResourceOld = HISTORICAL_RESOURCE_NAME1,
					ResourceNew = null,
					Included = true,
					IsUserInput = true,
					Rationale = Rationale1
				},
				new SkillMixModelView {
					ResourceOld = HISTORICAL_RESOURCE_NAME2,
					ResourceNew = string.Empty,
					Included = true,
					IsUserInput = true,
					Rationale = Rationale2
				}
			};

			List<CommonDisclosureModelView> commonDisclosures = new List<CommonDisclosureModelView>
			{
				new CommonDisclosureModelView {
					ResourceID = null,
					BusinessResourceID = BRC_RESOURCE_NAME1,
					Included = true,
					Rationale = Rationale3
				},
				new CommonDisclosureModelView {
					ResourceID = string.Empty,
					BusinessResourceID = BRC_RESOURCE_NAME2,
					Included = true,
					Rationale = Rationale1
				}
			};

			RefreshSkillMixModelView result = sut.RefreshSkillMixTables(hours, laborTypes, skillmix, commonDisclosures, true);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.SkillMixRows);
			Assert.IsTrue(result.SkillMixRows.Any());
			Assert.AreEqual(4, result.SkillMixRows.Count);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME1, result.SkillMixRows.First().ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME2, result.SkillMixRows.ElementAt(1).ResourceOld);
			Assert.AreEqual(HISTORICAL_RESOURCE_NAME3, result.SkillMixRows.ElementAt(2).ResourceOld);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(0).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(1).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(2).ResourceNew);
			Assert.AreEqual(string.Empty, result.SkillMixRows.ElementAt(3).ResourceNew);
			Assert.AreEqual(150.0m, result.SkillMixRows.ElementAt(0).HistoricalHours);
			Assert.AreEqual(40.0m, result.SkillMixRows.ElementAt(1).HistoricalHours);
			Assert.AreEqual(110.0m, result.SkillMixRows.ElementAt(2).HistoricalHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(1).ProposedHours);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(2).ProposedHours);
			Assert.AreEqual(50.0m, result.SkillMixRows.ElementAt(0).LaborSkillMix);
			Assert.AreEqual(4000.0m / 300m, result.SkillMixRows.ElementAt(1).LaborSkillMix);
			Assert.AreEqual(11000.0m / 300m, result.SkillMixRows.ElementAt(2).LaborSkillMix);
			Assert.AreEqual(0.0m, result.SkillMixRows.ElementAt(0).BOESkillMix);
			Assert.AreEqual(0m, result.SkillMixRows.ElementAt(1).BOESkillMix);
			Assert.AreEqual(0m, result.SkillMixRows.ElementAt(2).BOESkillMix);
			Assert.IsTrue(result.SkillMixRows.ElementAt(0).Included);
			Assert.IsTrue(result.SkillMixRows.ElementAt(1).Included);
			Assert.IsFalse(result.SkillMixRows.ElementAt(2).Included);
			Assert.AreEqual(Rationale1, result.SkillMixRows.ElementAt(0).Rationale);
			Assert.AreEqual(Rationale2, result.SkillMixRows.ElementAt(1).Rationale);
			Assert.IsTrue(string.IsNullOrEmpty(result.SkillMixRows.ElementAt(2).Rationale));

			Assert.AreEqual(2, result.CommonDisclosureRows.Count);
			Assert.AreEqual(string.Empty, result.CommonDisclosureRows.ElementAt(0).ResourceID);
			Assert.AreEqual(string.Empty, result.CommonDisclosureRows.ElementAt(1).ResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME1, result.CommonDisclosureRows.ElementAt(0).BusinessResourceID);
			Assert.AreEqual(BRC_RESOURCE_NAME2, result.CommonDisclosureRows.ElementAt(1).BusinessResourceID);
			Assert.AreEqual(Rationale3, result.CommonDisclosureRows.ElementAt(0).Rationale);
			Assert.AreEqual(Rationale1, result.CommonDisclosureRows.ElementAt(1).Rationale);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(0).Included);
			Assert.AreEqual(true, result.CommonDisclosureRows.ElementAt(1).Included);
			Assert.AreEqual(75.0m, result.CommonDisclosureRows.ElementAt(0).ProposedHours);
			Assert.AreEqual(60.0m, result.CommonDisclosureRows.ElementAt(1).ProposedHours);
			AssertHelpers.AssertAreEqualEpsilon(75.0m / 135.0m * 100m, result.CommonDisclosureRows.ElementAt(0).BOESkillMix.Value);
			AssertHelpers.AssertAreEqualEpsilon(60.0m / 135.0m * 100m, result.CommonDisclosureRows.ElementAt(1).BOESkillMix.Value);

			// Check CommonDisclosureTotals
			CheckSkillMixTotals(result);

			// Historical Hours is percentage the brc is linked to in the labor types for this resource multiplied by historical hours in skill mix for this resource
			AssertHelpers.AssertAreEqualEpsilon(105.55555m, result.CommonDisclosureRows.ElementAt(0).HistoricalHours);
			AssertHelpers.AssertAreEqualEpsilon(84.44444m, result.CommonDisclosureRows.ElementAt(1).HistoricalHours);
			AssertHelpers.AssertAreEqualEpsilon(55.555555m, result.CommonDisclosureRows.ElementAt(0).LaborSkillMix);
			AssertHelpers.AssertAreEqualEpsilon(44.444444m, result.CommonDisclosureRows.ElementAt(1).LaborSkillMix);



		}

		/// <summary>
		/// Check the totals for SkillMix and CD 
		/// </summary>
		/// <param name="result"></param>
		public void CheckSkillMixTotals(RefreshSkillMixModelView result)
		{
			if (result.SkillMixRows.Any(r => r.LaborSkillMix != 0m))
			{
				AssertHelpers.AssertAreEqualEpsilon(100m, result.SkillMixTotals.LaborSkillMix);
			}
			else
			{
				AssertHelpers.AssertAreEqualEpsilon(0m, result.SkillMixTotals.LaborSkillMix);
			}
			if (result.SkillMixRows.Any(r => r.BOESkillMix != 0m))
			{
				AssertHelpers.AssertAreEqualEpsilon(100m, result.SkillMixTotals.BoeSkillMix);
			}
			else
			{
				AssertHelpers.AssertAreEqualEpsilon(0m, result.SkillMixTotals.BoeSkillMix);
			}

			if (result.CommonDisclosureRows.Any(r => r.BOESkillMix != 0m))
			{
				AssertHelpers.AssertAreEqualEpsilon(100m, result.CommonDisclosureTotals.LaborSkillMix);
			}
			else
			{
				AssertHelpers.AssertAreEqualEpsilon(0m, result.CommonDisclosureTotals.LaborSkillMix);
			}

			if (result.CommonDisclosureRows.Any(r => r.BOESkillMix != 0m))
			{
				AssertHelpers.AssertAreEqualEpsilon(100m, result.CommonDisclosureTotals.BoeSkillMix);
			}
			else
			{
				AssertHelpers.AssertAreEqualEpsilon(0m, result.CommonDisclosureTotals.BoeSkillMix);
			}
		}
	}
}
