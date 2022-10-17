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
	using System.Linq;
	using System.Threading.Tasks;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.Workspace;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.DataBridge.Reference;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.PickList;
	using Microsoft.Practices.Unity;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;
	using UserDTO = GenBOE.Dtos.UserDTO;
	using IESSAPClient = GenBOE.ActionLogic.IESSAPClient;

	[TestClass]
    public class WorkspaceControllerLogicTest : MOQObject
    {
        #region Private members

        private Mock<IUserDTODataLoader> _userLoader = null;
        private Mock<IResourceDTODataLoader> _ResourceLoader = null;
        private Mock<ITMResourceRateDTODataLoader> _tmResourceRateLoader = null;
        private Mock<ICustomFieldValueDTODataLoader> _customFieldValueLoader = null;
        private Mock<IBOEStateMachine> _boeStateMachine = null;
        private Mock<IBoeMediator> _BoeMediator = null;
        private Mock<IBoeTaskElementMediator> _BoeTaskElementMediator = null;
        private Mock<IVariableSelectBOEtoSumCalculation> _VariableSelectBoeToSum = null;
        private Mock<BoeTaskElementRecalculation> _BoeTaskElementRecalculation = null;
        private Mock<IInUseDataLoader> _InUseDataLoader = null;
        private Mock<IRetriever> retriever = null;
        private Mock<IFullObjectFactory> factory;
        private Mock<ICommonDataMapper> _commonDatamapper = null;
        private Mock<IPermissionsDTODataLoader> _permissionLoader = null;
        private Mock<BOEImportMerge> _boeMergeMock = null;
        private Mock<BoeTaskElementImportMerge> _taskElementMergeMock = null;
        private Mock<ResourceTypeImportMerge> _resourceMergeMock = null;
        private Mock<FullBoeDataImporter> _boeImporterMock = null;
        private Mock<IBoeDTODataLoader> boeLoader = null;
        private Mock<IBOELaborControllerLogic> _BOELaborControllerLogic = null;
        private Mock<IFullWorkspaceRecalculation> fullWsRecalc = null;
        private Mock<IWorkspaceDTODataLoader> wsLoader = null;
        private Mock<RMSZoneTravelRatesFeesDataLoader> zoneTravelRatesFeesLoader = null;
        private Mock<IEscalationRatesDTOLoader> systemEscalationRatesLoader;
        private Mock<IMSTTravelNonzoneFeesAndCostsDTODataLoader> systemFeesLoader;
        private Mock<IWorkspaceVariableDTODataLoader> workspaceVariableLoader = null;
        private Mock<IOffloadRatesDTOLoader> offloadRatesLoader;
        private Mock<IPickListMapper> ptmPickListMapper;
        private Mock<IPickListMapper> boePickListMapper;
        private Mock<ContractTypeLoader> contractTypeLoader;
		private Mock<IMoqTypeDataLoader> moqTypeDataLoader;

        private WorkspaceControllerLogicSpaceSystems CreateSystemSpaceSystems()
        {
            this.CreateCommonSystem();

            return new WorkspaceControllerLogicSpaceSystems(this.wsLoader.Object,
                this._userLoader.Object,
                this._ResourceLoader.Object,
                this._tmResourceRateLoader.Object,
                this._BoeTaskElementRecalculation.Object,
                this._InUseDataLoader.Object,
                this.factory.Object,
                this._commonDatamapper.Object,
                this._permissionLoader.Object,
                this._boeImporterMock.Object,
                this._BOELaborControllerLogic.Object,
                this.fullWsRecalc.Object,
                this.workspaceVariableLoader.Object,
                this._customFieldValueLoader.Object,
                null,
                null,
                this.boePickListMapper.Object,
                this.ptmPickListMapper.Object,
                this.contractTypeLoader.Object,
                null,
				this.moqTypeDataLoader.Object,
				this._boeStateMachine.Object,
				this._BoeMediator.Object);
        }

        private WorkspaceControllerLogicMST CreateSystemMST()
        {
            this.CreateCommonSystem();

            return new WorkspaceControllerLogicMST(this.wsLoader.Object,
                this._userLoader.Object,
                this._ResourceLoader.Object,
                this._tmResourceRateLoader.Object,
                this._BoeTaskElementRecalculation.Object,
                this._InUseDataLoader.Object,
                this.factory.Object,
                this._commonDatamapper.Object,
                this._permissionLoader.Object,
                this._boeImporterMock.Object,
                this._BOELaborControllerLogic.Object,
                this.fullWsRecalc.Object,
                this.zoneTravelRatesFeesLoader.Object,
                this.systemEscalationRatesLoader.Object,
                this.systemFeesLoader.Object,
                this.workspaceVariableLoader.Object,
                this._customFieldValueLoader.Object,
                null,
                this.offloadRatesLoader.Object, null, 
                this.boePickListMapper.Object, 
                this.ptmPickListMapper.Object,
                this.contractTypeLoader.Object,
                null,
                this.moqTypeDataLoader.Object,
				this._boeStateMachine.Object,
				this._BoeMediator.Object);
		}

        /// <summary>
        /// Sets up the common components of the system needed for tests
        /// </summary>
        private void CreateCommonSystem()
        {
            this.wsLoader = new Mock<IWorkspaceDTODataLoader>();
            this.zoneTravelRatesFeesLoader = new Mock<RMSZoneTravelRatesFeesDataLoader>();
            this.offloadRatesLoader = new Mock<IOffloadRatesDTOLoader>();
            this.systemEscalationRatesLoader = new Mock<IEscalationRatesDTOLoader>();
            this.systemFeesLoader = new Mock<IMSTTravelNonzoneFeesAndCostsDTODataLoader>();
            this._userLoader = new Mock<IUserDTODataLoader>();
            this._ResourceLoader = new Mock<IResourceDTODataLoader>();
            this._tmResourceRateLoader = new Mock<ITMResourceRateDTODataLoader>();

            this._customFieldValueLoader = new Mock<ICustomFieldValueDTODataLoader>();
            this._customFieldValueLoader.Setup(s => s.GetBOECustomFieldContainerFieldValueMappings(It.IsAny<int>())).Returns(new List<CustomFieldContainerFieldValueMapping>());
            this._customFieldValueLoader.Setup(s => s.GetTaskElementCustomFieldContainerFieldValueMappings(It.IsAny<int>())).Returns(new List<CustomFieldContainerFieldValueMapping>());
            this._customFieldValueLoader.Setup(s => s.GetResourceCustomFieldContainerFieldValueMappings(It.IsAny<int>())).Returns(new List<CustomFieldContainerFieldValueMapping>());

            this._boeStateMachine = new Mock<IBOEStateMachine>();
            this._BoeMediator = new Mock<IBoeMediator>();
            this._BoeTaskElementMediator = new Mock<IBoeTaskElementMediator>();
            this._VariableSelectBoeToSum = new Mock<IVariableSelectBOEtoSumCalculation>();

            this.factory = new Mock<IFullObjectFactory>();
            this._BoeTaskElementRecalculation = new Mock<BoeTaskElementRecalculation>(this._VariableSelectBoeToSum.Object, this.factory.Object);
            this._InUseDataLoader = new Mock<IInUseDataLoader>();
            this.retriever = new Mock<IRetriever>();
            this._commonDatamapper = new Mock<ICommonDataMapper>();
            this._permissionLoader = new Mock<IPermissionsDTODataLoader>();

            this.boeLoader = new Mock<IBoeDTODataLoader>();
            this._boeMergeMock = new Mock<BOEImportMerge>(this.boeLoader.Object, this._customFieldValueLoader.Object);
            this._taskElementMergeMock = new Mock<BoeTaskElementImportMerge>(this.factory.Object, this._customFieldValueLoader.Object);
            this._resourceMergeMock = new Mock<ResourceTypeImportMerge>(this._VariableSelectBoeToSum.Object, this._customFieldValueLoader.Object);
            this._boeImporterMock = new Mock<FullBoeDataImporter>(this._BoeMediator.Object, this._BoeTaskElementMediator.Object, this._boeMergeMock.Object,
                this._taskElementMergeMock.Object, this._resourceMergeMock.Object);

            this._BOELaborControllerLogic = new Mock<IBOELaborControllerLogic>();

            this.fullWsRecalc = new Mock<IFullWorkspaceRecalculation>();

            this.workspaceVariableLoader = new Mock<IWorkspaceVariableDTODataLoader>();

            this.boePickListMapper = new Mock<IPickListMapper>();
            this.ptmPickListMapper = new Mock<IPickListMapper>();

            this.contractTypeLoader = new Mock<ContractTypeLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), this.retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), this.factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), this._commonDatamapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), this._permissionLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), CreateValidationFactoryMock().Object);

			this.moqTypeDataLoader = new Mock<IMoqTypeDataLoader>();
        }

        private void DoGetWorkspaceIdentificationTest(IWorkspaceControllerLogic sut, CompanyConfiguration config)
        {
            UserDTO costVolumeLead = new UserDTO()
            {
                UserID = 15,
                DisplayName = "Cost Volume Lead Dude",
                NTID = "cvld"
            };

            WorkspaceDTO workspace = new WorkspaceDTO()
            {
                CostVolumeLeadPricerUserID = costVolumeLead.UserID,
                Shortname = "testWorkspace",
                ContainsOCI = false,
                ContractEndDate = new DateTime(2012, 4, 25),
                ContractStartDate = new DateTime(2012, 5, 30),
                Description = "description",
                LineOfBusiness = new PickListDto() { Id = 1002, Text = "Commercial Ventures (COM)" }, //"Commercial Ventures (COM)"
                ProposalStatus = ProposalStatusType.None,
                ProposalSubmittalDate = new DateTime(2012, 12, 12),
                RFPNumber = "324134123",
                Segment = SegmentType.DS,
                StatusComment = "comment",
                TrackingNumber = "LDSKJF30241",
                UpdateDate = DateTime.Now,
                Id = 34,
                WorkspaceName = "test workspace"
            };
            FullWorkspace ws = new FullWorkspace(workspace);
            this.factory.Setup(x => x.CreateFullWorkspace(ws.Shortname, It.IsAny<bool>())).Returns(ws);

            //this._workspaceMapper.Setup(x => x.GetWorkspaceByShortName(workspace.Shortname, It.IsAny<bool>())).Returns(workspace);
            this._userLoader.Setup(x => x.GetUserByID(workspace.CostVolumeLeadPricerUserID)).Returns(costVolumeLead);

            this.retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(34)).Returns(new Collection<CustomFieldDTO>());

            if (config == CompanyConfiguration.SpaceSystems)
            {
                WorkspaceIdentificationSpaceModelView result = sut.GetWorkspaceIdentificationModelView(ws) as WorkspaceIdentificationSpaceModelView;
                Assert.AreEqual(result.ContainsOCI, workspace.ContainsOCI);
                Assert.AreEqual(result.ContractEndDate, workspace.ContractEndDate.ToMonthString());
                Assert.AreEqual(result.ContractStartDate, workspace.ContractStartDate.ToMonthString());
                Assert.AreEqual(result.CostVolumeLeadPricerDisplayName, costVolumeLead.DisplayName);
                Assert.AreEqual(result.CostVolumeLeadPricerNTID, costVolumeLead.NTID);
                Assert.AreEqual(result.Description, workspace.Description);
                Assert.AreEqual(result.LineOfBusinessTypeID, workspace.LineOfBusiness.Id);
                Assert.AreEqual(result.ProposalStatus, result.ProposalStatus);
                Assert.AreEqual(result.ProposalSubmittalDate, workspace.ProposalSubmittalDate.Value.ToString("MM/dd/yyyy"));
                Assert.AreEqual(result.RFPNumber, workspace.RFPNumber);
                Assert.AreEqual(result.StatusComments, workspace.StatusComment);
                Assert.AreEqual(result.TrackingNumber, workspace.TrackingNumber);
                Assert.AreEqual(result.UpdateDate, workspace.UpdateDate);
                Assert.AreEqual(result.WorkspaceID, workspace.Id);
                Assert.AreEqual(result.WorkspaceName, workspace.WorkspaceName);
                Assert.AreEqual(result.IsUsingTM, workspace.IsUsingTM);
            }

            if (config == CompanyConfiguration.MST)
        {
                WorkspaceIdentificationMSTModelView result = sut.GetWorkspaceIdentificationModelView(ws) as WorkspaceIdentificationMSTModelView;
                Assert.AreEqual(result.ContainsOCI, workspace.ContainsOCI);
                Assert.AreEqual(result.ContractEndDate, workspace.ContractEndDate.ToMonthString());
                Assert.AreEqual(result.ContractStartDate, workspace.ContractStartDate.ToMonthString());
                Assert.AreEqual(result.CostVolumeLeadPricerDisplayName, costVolumeLead.DisplayName);
                Assert.AreEqual(result.CostVolumeLeadPricerNTID, costVolumeLead.NTID);
                Assert.AreEqual(result.Description, workspace.Description);
                Assert.AreEqual(result.LineOfBusinessTypeID, workspace.LineOfBusiness.Id);
                Assert.AreEqual(result.ProposalStatus, result.ProposalStatus);
                Assert.AreEqual(result.ProposalSubmittalDate, workspace.ProposalSubmittalDate.Value.ToString("MM/dd/yyyy"));
                Assert.AreEqual(result.RFPNumber, workspace.RFPNumber);
                Assert.AreEqual(result.StatusComments, workspace.StatusComment);
                Assert.AreEqual(result.UpdateDate, workspace.UpdateDate);
                Assert.AreEqual(result.WorkspaceID, workspace.Id);
                Assert.AreEqual(result.WorkspaceName, workspace.WorkspaceName);
                Assert.AreEqual(result.IsProjectMapWorkspace, workspace.IsProjectMapWorkspace);
        }

        }

        private void DoCreateResourceRateTMModelView(WorkspaceResourceRateTMModelView result, IWorkspaceControllerLogic sut)
        {
            TMResourceRateDTO tmdto = new TMResourceRateDTO();
            DateTime TestDate = DateTime.Now;
            tmdto.ResourceRateID = 1;
            tmdto.UpdateDate = TestDate;
            tmdto.ResourceID = 1;
            tmdto.ResourceRate = 1.5m;
            tmdto.StartDate = TestDate.AddMonths(1);
            tmdto.EndDate = TestDate.AddMonths(2);

            ResourceDTO rdto = new ResourceDTO();
            rdto.ResourceName = "My Resource";
            rdto.ResourceDesc = "My Resource Description";

            result = sut.CreateWorkspaceResourceRateTMModelView(tmdto, rdto, true);
            Assert.AreEqual(1, result.ResourceRateID, "The ResourceRateID is incorrect.");
            Assert.AreEqual(TestDate, result.UpdateDate, "The UpdateDate is incorrect.");
            Assert.AreEqual(1, result.ResourceID, "The ResourceID is incorrect.");
            Assert.AreEqual((1.5m).ToString(), result.ResourceRate, "The ResourceRate is incorrect.");
            Assert.AreEqual(TestDate.AddMonths(1).ToMonthString(), result.StartDate, "The StartDate is incorrect.");
            Assert.AreEqual(TestDate.AddMonths(2).ToMonthString(), result.EndDate, "The EndDate is incorrect.");
            Assert.AreEqual("My Resource", result.ResourceName, "The ResourceName is incorrect.");
            Assert.AreEqual("My Resource Description", result.ResourceDescription, "The ResourceDescription is incorrect.");
        }

        #endregion

        #region GetWorkspaceIdentification Tests
        [TestMethod]
        public void GetWorkspaceIdentificationTestSpaceSystems()
        {
            WorkspaceControllerLogicSpaceSystems sut = this.CreateSystemSpaceSystems();
            DoGetWorkspaceIdentificationTest(sut, CompanyConfiguration.SpaceSystems);
        }

        [TestMethod]
        public void GetWorkspaceIdentificationTestMST()
        {
            WorkspaceControllerLogicMST sut = this.CreateSystemMST();
            DoGetWorkspaceIdentificationTest(sut, CompanyConfiguration.MST);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetWorkspaceIdentificationTestExceptionSpaceSystems()
        {
            WorkspaceControllerLogicSpaceSystems sut = this.CreateSystemSpaceSystems();
            WorkspaceIdentificationSpaceModelView result = sut.GetWorkspaceIdentificationModelView(null) as WorkspaceIdentificationSpaceModelView;
            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetWorkspaceIdentificationTestExceptionMST()
        {
            WorkspaceControllerLogicMST sut = this.CreateSystemMST();
            WorkspaceIdentificationMSTModelView result = sut.GetWorkspaceIdentificationModelView(null) as WorkspaceIdentificationMSTModelView;
            Assert.IsNull(result);
        }
        #endregion

        #region GetCreateWorkspaceModelView Tests
        [TestMethod]
        public void GetCreateWorkspaceModelViewSSC()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            CreateWorkspaceSpaceModelView result = sut.GetCreateWorkspaceModelView() as CreateWorkspaceSpaceModelView;

            Assert.IsNotNull(result, "The model view is not the required type.");
        }

        [TestMethod]
        public void GetCreateWorkspaceModelViewMST()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();
            CreateWorkspaceMSTModelView result = sut.GetCreateWorkspaceModelView() as CreateWorkspaceMSTModelView;

            Assert.IsNotNull(result, "The model view is not the required type.");
        }
        #endregion

        [TestMethod]
        public void CanExportBoeForWorkofflineMST()
        {
            WorkspaceControllerLogicMST sut = this.CreateSystemMST();
            bool result = sut.CanExportBoeForWorkoffline(true, true);
            Assert.AreEqual(true, result, "The result for the CanEportBoeForWorkofflineMST test should have been true.");
            result = sut.CanExportBoeForWorkoffline(false, true);
            Assert.AreEqual(true, result, "The result for the CanEportBoeForWorkofflineMST test should have been true.");
            result = sut.CanExportBoeForWorkoffline(true, false);
            Assert.AreEqual(true, result, "The result for the CanEportBoeForWorkofflineMST test should have been true.");
            result = sut.CanExportBoeForWorkoffline(false, false);
            Assert.AreEqual(false, result, "The result for the CanEportBoeForWorkofflineMST test should have been false.");
        }

        #region CreateWorkspaceResourceRateTMModelView Tests
        [TestMethod]
        public void CreateworkspaceResourceRateTMModelViewSSC()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            WorkspaceResourceRateTMModelView result = sut.CreateWorkspaceResourceRateTMModelView();
            Assert.AreEqual(WebConstants.WORKSPACE_RESOURCE_RATE_TM_SPACE_HEADING_TEXT, result.WorkspaceResourceRateTMHeadingText, "The heading text is not correct.");
            Assert.AreEqual(WebConstants.WORKSPACE_RESOURCE_RATE_TM_SPACE_JUMP_DESCRIPTION_TEXT, result.WorkspaceResourceRateTMJumpDescription, "The jump description text is not correct.");
            Assert.AreEqual(WebConstants.WORKSPACE_RESOURCE_RATE_TM_SPACE_CONTROL_DESCRIPTION_TEXT, result.WorkspaceResourceRateTMControlDescription, "The control descripton text is not correct.");
            this.DoCreateResourceRateTMModelView(result, sut);
        }

        [TestMethod]
        public void CreateworkspaceResourceRateTMModelViewMST()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();
            WorkspaceResourceRateTMModelView result = sut.CreateWorkspaceResourceRateTMModelView();
            Assert.AreEqual(WebConstants.WORKSPACE_RESOURCE_RATE_TM_SPACE_HEADING_TEXT, result.WorkspaceResourceRateTMHeadingText, "The heading text is not correct.");
            Assert.AreEqual(WebConstants.WORKSPACE_RESOURCE_RATE_TM_SPACE_JUMP_DESCRIPTION_TEXT, result.WorkspaceResourceRateTMJumpDescription, "The jump description text is not correct.");
            Assert.AreEqual(WebConstants.WORKSPACE_RESOURCE_RATE_TM_SPACE_CONTROL_DESCRIPTION_TEXT, result.WorkspaceResourceRateTMControlDescription, "The control descripton text is not correct.");
            this.DoCreateResourceRateTMModelView(result, sut);
        }
        #endregion

        #region GetDefaultReportTemplate Tests
        [TestMethod]
        public void GetDefaultReportTemplateSSC()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            ExcelReportTemplateType result = sut.GetDefaultReportTemplateType();
            Assert.AreEqual(ExcelReportTemplateType.MASTER, result, "The type of ExcelReportTemplateType returned in incorrect.");
        }

        [TestMethod]
        public void GetDefaultReportTemplateMST()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();
            ExcelReportTemplateType result = sut.GetDefaultReportTemplateType();
            Assert.AreEqual(ExcelReportTemplateType.MASTER, result, "The type of ExcelReportTemplateType returned in incorrect.");
        }
        #endregion

        #region GetPicklistReportTemplateTypes Tests
        [TestMethod]
        public void GetPicklistReportTemplateTypesSSC()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            ICollection<ExcelReportTemplateType> result = sut.GetPicklistReportTemplateTypes(new FullWorkspace());
            Assert.AreEqual(1, result.Count, "The number of objects returned in incorrect.");
            Assert.AreEqual(ExcelReportTemplateType.MASTER, result.ToCollection()[0]);
        }

        [TestMethod]
        public void GetPicklistReportTemplateTypesMST()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();
            ICollection<ExcelReportTemplateType> result = sut.GetPicklistReportTemplateTypes(new FullWorkspace());
            Assert.AreEqual(11, result.Count, "The number of objects returned in incorrect.");
            Assert.AreEqual(ExcelReportTemplateType.MASTER, result.ToCollection()[0]);
            Assert.AreEqual(ExcelReportTemplateType.MST_DEEPWATER_PORTRAIT, result.ToCollection()[1]);
            Assert.AreEqual(ExcelReportTemplateType.MST_PORTRAIT_12_POINT_1_INCH_MARGINS, result.ToCollection()[2]);
            Assert.AreEqual(ExcelReportTemplateType.MST_SPACE_FENCE_PORTRAIT, result.ToCollection()[3]);
            Assert.AreEqual(ExcelReportTemplateType.MST_STANDARD_PORTRAIT, result.ToCollection()[4]);
            Assert.AreEqual(ExcelReportTemplateType.MST_PORTRAIT_12_POINT_1_INCH_MARGINS_GENBOE_TASK_IDS, result.ToCollection()[5]);
            Assert.AreEqual(ExcelReportTemplateType.MST_PORTRAIT_12_POINT_1_INCH_MARGINS_GENBOE_BOE_TASK_RESOURCE_IDS, result.ToCollection()[6]);
            Assert.AreEqual(ExcelReportTemplateType.MST_PORTRAIT_12_POINT_1_INCH_MARGINS_CUSTOM_FIELDS, result.ToCollection()[7]);
            Assert.AreEqual(ExcelReportTemplateType.RMS_PORTRAIT_WBS_CLIN_SOW, result.ToCollection()[8]);
            Assert.AreEqual(ExcelReportTemplateType.RMS_SIKORSKY_PROJECT_MAP, result.ToCollection()[9]);
            Assert.AreEqual(ExcelReportTemplateType.RMS_RMS_PROJECT_MAP, result.ToCollection()[10]);
        }

        [TestMethod]
        public void GetPicklistReportTemplateTypesMST_ProjectMapWorkspace()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();
            ICollection<ExcelReportTemplateType> result = sut.GetPicklistReportTemplateTypes(new FullWorkspace() { ProjectMapType = ProjectMapType.TimePhasedProjectMap });
            Assert.AreEqual(2, result.Count, "The number of objects returned in incorrect.");
            Assert.AreEqual(ExcelReportTemplateType.RMS_SIKORSKY_PROJECT_MAP, result.ToCollection()[0]);
            Assert.AreEqual(ExcelReportTemplateType.RMS_RMS_PROJECT_MAP, result.ToCollection()[1]);
        }
        #endregion

        #region GetCustomFieldsGridModelViews Tests
        [TestMethod]
        public void GetCustomFieldsGridModelViewsTest()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();

            CustomFieldDTO customFieldDTO = new CustomFieldDTO() { Id = 1, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldName = "test", CustomFieldRequired = false, IsOpenEnded = false, WorkspaceID = 1 };
            CustomFieldValueDTO customFieldValueDTO = new CustomFieldValueDTO() { Id = 1, CustomFieldID = customFieldDTO.Id, CustomFieldValueID = 1, CustomFieldValueName = "test name", CustomFieldValueDescription = "test desc", CustomFieldValueInUseFlag = true };
            BOECustomFieldsGridModelView customFieldMV = new BOECustomFieldsGridModelView() { CustomFieldID = customFieldDTO.Id, CustomFieldDisplayID = customFieldDTO.CustomFieldDisplayID, FieldName = customFieldDTO.CustomFieldName, isRequired = customFieldDTO.CustomFieldRequired, isOpenEnded = customFieldDTO.IsOpenEnded };

            CustomFieldDTO openEndedDTO = new CustomFieldDTO() { Id = 2, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldName = "open ended", CustomFieldRequired = false, IsOpenEnded = true, WorkspaceID = 1 };
            CustomFieldValueDTO openEndedValueDTO = new CustomFieldValueDTO() { Id = 2, CustomFieldID = openEndedDTO.Id, CustomFieldValueDescription = "test open ended", CustomFieldValueInUseFlag = true };
            BOECustomFieldsGridModelView openEndedMV = new BOECustomFieldsGridModelView() { CustomFieldID = openEndedDTO.Id, CustomFieldDisplayID = openEndedDTO.CustomFieldDisplayID, FieldName = openEndedDTO.CustomFieldName, isRequired = openEndedDTO.CustomFieldRequired, isOpenEnded = openEndedDTO.IsOpenEnded };

            _customFieldValueLoader.Setup(x => x.GetCustomFieldValueDTOsByCustomFieldIds(new Collection<int>() { customFieldDTO.Id })).Returns(new Collection<CustomFieldValueDTO>() { customFieldValueDTO });

            ICollection<BOECustomFieldsGridModelView> result = sut.GetCustomFieldsGridModelViews(new Collection<CustomFieldDTO>() { customFieldDTO }, 0);

            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(customFieldMV.CustomFieldID, result.First().CustomFieldID);
            Assert.AreEqual(customFieldMV.FieldName, result.First().FieldName);
            Assert.AreEqual(customFieldMV.CustomFieldDisplayID, result.First().CustomFieldDisplayID);
            Assert.AreEqual(customFieldMV.FieldName, result.First().FieldName);
            Assert.AreEqual(customFieldMV.isRequired, result.First().isRequired);
            Assert.AreEqual(customFieldMV.isOpenEnded, result.First().isOpenEnded);

            _customFieldValueLoader.Setup(x => x.GetCustomFieldValueDTOsByCustomFieldIds(new Collection<int>() { openEndedDTO.Id })).Returns(new Collection<CustomFieldValueDTO>() { openEndedValueDTO });

             result = sut.GetCustomFieldsGridModelViews(new Collection<CustomFieldDTO>() { openEndedDTO }, 0);

            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(openEndedMV.CustomFieldID, result.First().CustomFieldID);
            Assert.AreEqual(openEndedMV.FieldName, result.First().FieldName);
            Assert.AreEqual(openEndedMV.CustomFieldDisplayID, result.First().CustomFieldDisplayID);
            Assert.AreEqual(openEndedMV.FieldName, result.First().FieldName);
            Assert.AreEqual(openEndedMV.isRequired, result.First().isRequired);
            Assert.AreEqual(openEndedMV.isOpenEnded, result.First().isOpenEnded);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCustomFieldsGridModelViewsTest_NullException()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            sut.GetCustomFieldsGridModelViews(null, 0);
        }
        #endregion

        #region WorkspaceIdentificationViewName Tests
        [TestMethod]
        public void WorkspaceIdentificationViewNameSSC()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            String result = sut.WorkspaceIdentificationViewName;
            Assert.AreEqual(WebConstants.VIEW_WORKSPACE_IDENTIFICATION_SPACE, result, "The Workspace Identification view name is incorrect.");
        }

        [TestMethod]
        public void WorkspaceIdentificationViewNameMST()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();
            String result = sut.WorkspaceIdentificationViewName;
            Assert.AreEqual(WebConstants.VIEW_WORKSPACE_IDENTIFICATION_MST, result, "The Workspace Identification view name is incorrect.");
        }
        #endregion

        # region PopulateCompanySpecificProperties
        [TestMethod]
        public void PopulateCompanySpecificPropertiesSSC()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            WorkspaceSearchResultModelView result = new WorkspaceSearchResultModelView();
            sut.PopulateCompanySpecificProperties(result);
            Assert.AreEqual(CommonConstants.LABEL_TEXT_LEAD_PRICER_SSC, result.LabelLeadPricer, "The LabelLeadPricer property is incorrect.");
        }

        [TestMethod]
        public void PopulateCompanySpecificPropertiesMST()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();
            WorkspaceSearchResultModelView result = new WorkspaceSearchResultModelView();
            sut.PopulateCompanySpecificProperties(result);
            Assert.AreEqual(CommonConstants.LABEL_TEXT_LEAD_PRICER_SSC, result.LabelLeadPricer, "The LabelLeadPricer property is incorrect.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PopulateCompanySpecificPropertiesExceptionSSC()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            sut.PopulateCompanySpecificProperties(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PopulateCompanySpecificPropertiesExceptionMST()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();
            sut.PopulateCompanySpecificProperties(null);
        }
        #endregion

        #region PopulateCompanySpecificWorkspaceProperties Tests

        /// <summary>
        /// Test the Workspace Identification version of PopulateCompanySpecificWorkspaceProperties for SSC
        /// </summary>
        [TestMethod]
        public void PopulateCompanySpecificWorkspacePropertiesSSC_WorkspaceIdentification()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            WorkspaceIdentificationSpaceModelView theModel = new WorkspaceIdentificationSpaceModelView();
            WorkspaceDTO wsDto = new WorkspaceDTO();
            theModel.ProposalClassType = 3;
            theModel.SelectedContractTypes = new Collection<int>() { 1001, 1002, 1004 };
            theModel.TrackingNumber = "The tracking number";
            theModel.RevisedSubmittalDate = "01/01/2019";
            sut.PopulateCompanySpecificWorkspaceProperties(theModel, wsDto);

            Assert.AreEqual(theModel.ProposalClassType, wsDto.ProposalClass.Id, "the ProposalClass property is incorrect.");
            Assert.AreEqual(theModel.SelectedContractTypes.Count, wsDto.SelectedContractTypes.Count, "The numbers of items in the SelectedContractTypes property is incorrect.");
            Assert.IsTrue(wsDto.SelectedContractTypes.Contains(1001), "The SelectedContractTypes property doesn't contain 1001.");
            Assert.IsTrue(wsDto.SelectedContractTypes.Contains(1002), "The SelectedContractTypes property doesn't contain 1002.");
            Assert.IsTrue(wsDto.SelectedContractTypes.Contains(1004), "The SelectedContractTypes property doesn't contain 1004.");
            Assert.AreEqual(theModel.TrackingNumber, wsDto.TrackingNumber, "the TrackingNumber property is incorrect.");
            Assert.AreEqual(theModel.RevisedSubmittalDate, wsDto.RevisedSubmittalDate.Value.ToString("MM/dd/yyyy"), "The RevisedSubmittalDate is incorrect");
        }

        /// <summary>
        /// Test the Create Workspace version of PopulateCompanySpecificWorkspaceProperties for SSC
        /// </summary>
        [TestMethod]
        public void PopulateCompanySpecificWorkspacePropertiesSSC_CreateWorkspace()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            CreateWorkspaceSpaceModelView theModel = new CreateWorkspaceSpaceModelView();
            WorkspaceDTO wsDto = new WorkspaceDTO();
            theModel.ProposalClass = 3;
            theModel.SelectedContractTypes = new Collection<int>() { 1001, 1002, 1004 };
            theModel.RevisedSubmittalDate = "01/01/2019";
            sut.PopulateCompanySpecificWorkspaceProperties(theModel, wsDto);

            Assert.AreEqual(theModel.ProposalClass, wsDto.ProposalClass.Id, "the ProposalClass property is incorrect.");
            Assert.AreEqual(theModel.SelectedContractTypes.Count, wsDto.SelectedContractTypes.Count, "The numbers of items in the SelectedContractTypes property is incorrect.");
            Assert.IsTrue(wsDto.SelectedContractTypes.Contains(1001), "The SelectedContractTypes property doesn't contain 1001.");
            Assert.IsTrue(wsDto.SelectedContractTypes.Contains(1002), "The SelectedContractTypes property doesn't contain 1002.");
            Assert.IsTrue(wsDto.SelectedContractTypes.Contains(1004), "The SelectedContractTypes property doesn't contain 1004.");
            Assert.AreEqual(theModel.RevisedSubmittalDate, wsDto.RevisedSubmittalDate.Value.ToString("MM/dd/yyyy"), "The RevisedSubmittalDate is incorrect");
        }

        /// <summary>
        /// Test the Workspace Identification version of PopulateCompanySpecificWorkspaceProperties throws an exception when the workspace is null for SSC
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PopulateCompanySpecificWorkspacePropertiesExceptionSSC_WorkspaceIdentification()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            WorkspaceIdentificationSpaceModelView theModel = new WorkspaceIdentificationSpaceModelView();
            sut.PopulateCompanySpecificWorkspaceProperties(theModel, null);
        }

        /// <summary>
        /// Test the Create Workspace version of PopulateCompanySpecificWorkspaceProperties throws an exception when the workspace is null for SSC
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PopulateCompanySpecificWorkspacePropertiesExceptionSSC_CreateWorkspace()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            CreateWorkspaceSpaceModelView theModel = new CreateWorkspaceSpaceModelView();
            sut.PopulateCompanySpecificWorkspaceProperties(theModel, null);
        }

        /// <summary>
        /// Test the Workspace Identification version of PopulateCompanySpecificWorkspaceProperties throws an exception when the workspace is null for RMS
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PopulateCompanySpecificWorkspacePropertiesExceptionMST()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();
            WorkspaceIdentificationMSTModelView theModel = new WorkspaceIdentificationMSTModelView();
            sut.PopulateCompanySpecificWorkspaceProperties(theModel, null);
        }

        /// <summary>
        /// Test the Create Workspace version of PopulateCompanySpecificWorkspaceProperties throws an exception when the workspace is null for RMS
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PopulateCompanySpecificWorkspacePropertiesException2MST()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();
            ICreateWorkspaceModelView theModel2 = new CreateWorkspaceMSTModelView();
            sut.PopulateCompanySpecificWorkspaceProperties(theModel2, null);
        }
        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void DoGetWorkspaceResourceRateGridTMModelViewsTest(IWorkspaceControllerLogic sut)
        {
            WorkspaceDTO workspaceDTO = new WorkspaceDTO()
            {
                Id = 4,
                ContractStartDate = new DateTime(2011, 1, 1),
                ContractEndDate = new DateTime(2012, 12, 1),
                Shortname = "testWorkspace",
                ResourceListID = 5
            };
            FullWorkspace ws = new FullWorkspace(workspaceDTO);
            this.factory.Setup(x => x.CreateFullWorkspace(ws.Shortname, It.IsAny<bool>())).Returns(ws);

            TMResourceRateDTO tmResourceRateDTO1 = new TMResourceRateDTO
            {
                ResourceRateID = 15,
                ResourceID = 5,
                StartDate = new DateTime(2012, 1, 1),
                EndDate = new DateTime(2012, 6, 1),
                ResourceRate = .53m,
                WorkspaceID = 4,
                UpdateDate = new DateTime(2011, 12, 19)
            };

            TMResourceRateDTO tmResourceRateDTO2 = new TMResourceRateDTO
            {
                ResourceRateID = 16,
                ResourceID = 6,
                StartDate = null,
                EndDate = null,
                ResourceRate = null ,
                WorkspaceID = 4,
                UpdateDate = new DateTime(2011, 12, 18)
            };

            TMResourceRateDTO tmResourceRateDTO3 = new TMResourceRateDTO
            {
                ResourceRateID = 17,
                ResourceID = 7,
                StartDate = new DateTime(2012, 1, 1),
                EndDate = new DateTime(2012, 12, 1),
                ResourceRate = 15.53m,
                WorkspaceID = 4,
                UpdateDate = new DateTime(2011, 12, 18)
            };

            TMResourceRateDTO tmResourceRateDTO4 = new TMResourceRateDTO
            {
                ResourceRateID = 18,
                ResourceID = 8,
                StartDate = null,
                EndDate = null,
                ResourceRate = null,
                WorkspaceID = 4,
                UpdateDate = new DateTime(2011, 12, 18)
            };

            TMResourceRateDTO systemResourceRate1 = new TMResourceRateDTO
            {
                ResourceRateID = 1,
                ResourceID = 34,
                StartDate = new DateTime(2011, 6, 1),
                EndDate = new DateTime(2011, 8, 1),
                ResourceRate = 32.2m
            };

            TMResourceRateDTO systemResourceRate2 = new TMResourceRateDTO
            {
                ResourceRateID = 2,
                ResourceID = 34,
                StartDate = new DateTime(2011, 4, 1),
                EndDate = new DateTime(2011, 6, 1),
                ResourceRate = 64.4m
            };

            TMResourceRateDTO systemResourceRate3 = new TMResourceRateDTO
            {
                ResourceRateID = 3,
                ResourceID = 35,
                StartDate = new DateTime(2011, 1, 1),
                EndDate = new DateTime(2011, 12, 1),
                ResourceRate = 15.1m
            };

            ResourceDTO workspaceResource1 = new ResourceDTO()
            {
                Id = 5,
                ResourceName = "ABCD",
                ResourceDesc = "DescriptionOne"
            };

            ResourceDTO workspaceResource2 = new ResourceDTO()
            {
                Id = 6,
                ResourceName = "EFGH",
                ResourceDesc = "DescriptionTwo"
            };

            ResourceDTO workspaceResource3 = new ResourceDTO()
            {
                Id = 7,
                ResourceName = "IJKL",
                ResourceDesc = "DescriptionThree"
            };

            ResourceDTO workspaceResource4 = new ResourceDTO()
            {
                Id = 8,
                ResourceName = "MNOP",
                ResourceDesc = "DescriptionFour"
            };

            ResourceDTO systemResource1 = new ResourceDTO()
            {
                Id = 34,
                ResourceName = "System1"
            };

            ResourceDTO systemResource2 = new ResourceDTO()
            {
                Id = 35,
                ResourceName = "System2"
            };

            List<TMResourceRateDTO> tmResourceRateDTOs = new List<TMResourceRateDTO>() { tmResourceRateDTO1, tmResourceRateDTO2, tmResourceRateDTO3, tmResourceRateDTO4 };
            List<TMResourceRateDTO> systemRateDTOs = new List<TMResourceRateDTO>() { systemResourceRate1, systemResourceRate2, systemResourceRate3 };

            List<TMResourceRateDTO> expandedWorkspaceRates = new List<TMResourceRateDTO>()
            {
                tmResourceRateDTO1,
                tmResourceRateDTO3,
                new TMResourceRateDTO() {
                    ResourceRateID = tmResourceRateDTO2.ResourceRateID,
                    ResourceID = tmResourceRateDTO2.ResourceID,
                    WorkspaceID = tmResourceRateDTO2.WorkspaceID,
                    StartDate = systemResourceRate1.StartDate,
                    EndDate = systemResourceRate1.EndDate,
                    ResourceRate = systemResourceRate1.ResourceRate
                },
                new TMResourceRateDTO() {
                    ResourceRateID = tmResourceRateDTO2.ResourceRateID,
                    ResourceID = tmResourceRateDTO2.ResourceID,
                    WorkspaceID = tmResourceRateDTO2.WorkspaceID,
                    StartDate = systemResourceRate2.StartDate,
                    EndDate = systemResourceRate2.EndDate,
                    ResourceRate = systemResourceRate2.ResourceRate
                },
                new TMResourceRateDTO() {
                    ResourceRateID = tmResourceRateDTO4.ResourceRateID,
                    ResourceID = tmResourceRateDTO4.ResourceID,
                    WorkspaceID = tmResourceRateDTO4.WorkspaceID,
                    StartDate = systemResourceRate3.StartDate,
                    EndDate = systemResourceRate3.EndDate,
                    ResourceRate = systemResourceRate3.ResourceRate
                }
            };
            _tmResourceRateLoader.Setup(x => x.GetByWorkspaceId(ws.Id)).Returns(tmResourceRateDTOs);
            _tmResourceRateLoader.Setup(x => x.GetByWorkspaceId(ws.Id)).Returns(expandedWorkspaceRates);
            int ResourceListID = 5;

            ICollection<ResourceDTO> workspaceResourceDTOs = new Collection<ResourceDTO>();
            workspaceResourceDTOs.Add(workspaceResource1);
            workspaceResourceDTOs.Add(workspaceResource2);
            workspaceResourceDTOs.Add(workspaceResource3);
            workspaceResourceDTOs.Add(workspaceResource4);

            ICollection<int> recIDs = new Collection<int>();
            recIDs.Add(workspaceResource1.Id);
            recIDs.Add(workspaceResource2.Id);
            recIDs.Add(workspaceResource3.Id);
            recIDs.Add(workspaceResource4.Id);

            ICollection<int> recIDsOddOrder = new Collection<int>() { 5, 7, 6, 8 };

            _ResourceLoader.Setup(x => x.GetById(workspaceResource1.Id)).Returns(workspaceResource1);
            _ResourceLoader.Setup(x => x.GetById(workspaceResource2.Id)).Returns(workspaceResource2);
            _ResourceLoader.Setup(x => x.GetById(workspaceResource3.Id)).Returns(workspaceResource3);
            _ResourceLoader.Setup(x => x.GetById(workspaceResource4.Id)).Returns(workspaceResource4);
            _ResourceLoader.Setup(x => x.GetById(systemResource1.Id)).Returns(systemResource1);
            _ResourceLoader.Setup(x => x.GetById(systemResource2.Id)).Returns(systemResource2);
            _ResourceLoader.Setup(x => x.GetById(systemResource1.Id)).Returns(systemResource1);
            _ResourceLoader.Setup(x => x.GetById(systemResource2.Id)).Returns(systemResource2);

            _ResourceLoader.Setup(x => x.GetByIds(recIDs)).Returns(workspaceResourceDTOs);
            _ResourceLoader.Setup(x => x.GetByIds(recIDsOddOrder)).Returns(workspaceResourceDTOs);
            _InUseDataLoader.Setup(x => x.GetWorkspaceResourceInUseByMultipleResources(recIDsOddOrder, ResourceListID)).Returns(new HashSet<int>() { 5, 7, 6, 8 });
            _InUseDataLoader.Setup(x => x.GetWorkspaceResourceInUseByMultipleResources(recIDs, ResourceListID)).Returns(new HashSet<int>() { 5, 6, 7, 8 });
            this._ResourceLoader.Setup(x => x.GetByIds(It.IsAny<ICollection<int>>())).Returns(new List<ResourceDTO>() { workspaceResource1, workspaceResource2, workspaceResource3, workspaceResource4, systemResource1, systemResource2 });

            WorkspaceResourceRateGridTMModelView result = sut.GetWorkspaceResourceRateGridTMModelView(ws, null);

            WorkspaceResourceRateTMModelView result1 = result.WorkspaceResourceRateTMResults.Where(x => x.ResourceRateID == tmResourceRateDTOs[0].ResourceRateID).First();
            WorkspaceResourceRateTMModelView result2 = result.WorkspaceResourceRateTMResults.Where(x => x.ResourceRate.IsEquivalentTo(systemRateDTOs[0].ResourceRate.ToString())).First();
            WorkspaceResourceRateTMModelView result3 = result.WorkspaceResourceRateTMResults.Where(x => x.ResourceRate.IsEquivalentTo(systemRateDTOs[1].ResourceRate.ToString())).First();
            WorkspaceResourceRateTMModelView result4 = result.WorkspaceResourceRateTMResults.Where(x => x.ResourceRateID == tmResourceRateDTOs[2].ResourceRateID).First();

            Assert.AreEqual(5, result.WorkspaceResourceRateTMResults.Count);

            Assert.AreEqual(tmResourceRateDTOs[0].ResourceRateID, result1.ResourceRateID);
            Assert.AreEqual(tmResourceRateDTOs[0].ResourceID, result1.ResourceID);
            Assert.AreEqual(workspaceResource1.ResourceName, result1.ResourceName);
            Assert.AreEqual(workspaceResource1.ResourceDesc, result1.ResourceDescription);
            Assert.AreEqual(tmResourceRateDTOs[0].StartDate.Value.ToMonthString(), result1.StartDate);
            Assert.AreEqual(tmResourceRateDTOs[0].EndDate.Value.ToMonthString(), result1.EndDate);
            Assert.AreEqual(tmResourceRateDTOs[0].ResourceRate.ToString(), result1.ResourceRate);
            Assert.AreEqual(tmResourceRateDTOs[0].UpdateDate, result1.UpdateDate);

            Assert.AreEqual(tmResourceRateDTOs[1].ResourceRateID, result2.ResourceRateID);
            Assert.AreEqual(tmResourceRateDTOs[1].ResourceID, result2.ResourceID);
            Assert.AreEqual(workspaceResource2.ResourceName, result2.ResourceName);
            Assert.AreEqual(workspaceResource2.ResourceDesc, result2.ResourceDescription);
            Assert.AreEqual(systemRateDTOs[0].StartDate.Value.ToMonthString(), result2.StartDate);
            Assert.AreEqual(systemRateDTOs[0].EndDate.Value.ToMonthString(), result2.EndDate);
            Assert.AreEqual(systemRateDTOs[0].ResourceRate.ToString(), result2.ResourceRate);

            Assert.AreEqual(tmResourceRateDTOs[1].ResourceRateID, result3.ResourceRateID);
            Assert.AreEqual(tmResourceRateDTOs[1].ResourceID, result3.ResourceID);
            Assert.AreEqual(workspaceResource2.ResourceName, result3.ResourceName);
            Assert.AreEqual(workspaceResource2.ResourceDesc, result3.ResourceDescription);
            Assert.AreEqual(systemRateDTOs[1].StartDate.Value.ToMonthString(), result3.StartDate);
            Assert.AreEqual(systemRateDTOs[1].EndDate.Value.ToMonthString(), result3.EndDate);
            Assert.AreEqual(systemRateDTOs[1].ResourceRate.ToString(), result3.ResourceRate);

            Assert.AreEqual(tmResourceRateDTOs[2].ResourceRateID, result4.ResourceRateID);
            Assert.AreEqual(tmResourceRateDTOs[2].ResourceID, result4.ResourceID);
            Assert.AreEqual(workspaceResource3.ResourceName, result4.ResourceName);
            Assert.AreEqual(workspaceResource3.ResourceDesc, result4.ResourceDescription);
            Assert.AreEqual(tmResourceRateDTOs[2].StartDate.Value.ToMonthString(), result4.StartDate);
            Assert.AreEqual(tmResourceRateDTOs[2].EndDate.Value.ToMonthString(), result4.EndDate);
            Assert.AreEqual(tmResourceRateDTOs[2].ResourceRate.ToString(), result4.ResourceRate);

            // test sorting and filtering
            WorkspaceResourceRateGridTMModelView modelView = new WorkspaceResourceRateGridTMModelView()
            {
                SearchFilter = workspaceResource2.ResourceName,
                SortField = WorkspaceResourceRateTMModelView.SORT_ID_START_DATE,
                OldSortField = WorkspaceResourceRateTMModelView.SORT_ID_START_DATE,
                Order = SortOrder.Descending,
                SortChanged = true
            };
            result = sut.GetWorkspaceResourceRateGridTMModelView(ws, modelView);
            Assert.AreEqual(2, result.WorkspaceResourceRateTMResults.Count);
            Assert.AreEqual(workspaceResource2.ResourceName, result.WorkspaceResourceRateTMResults.ElementAt(0).ResourceName);
            Assert.AreEqual(workspaceResource2.ResourceName, result.WorkspaceResourceRateTMResults.ElementAt(1).ResourceName);
            Assert.IsTrue(result.WorkspaceResourceRateTMResults.ElementAt(0).StartDate.ToDateTime() < result.WorkspaceResourceRateTMResults.ElementAt(1).StartDate.ToDateTime());

            modelView = new WorkspaceResourceRateGridTMModelView()
            {
                SearchFilter = null,
                SortField = WorkspaceResourceRateTMModelView.SORT_ID_START_DATE,
                OldSortField = WorkspaceResourceRateTMModelView.SORT_ID_START_DATE,
                Order = SortOrder.Ascending,
                SortChanged = true
            };
            result = sut.GetWorkspaceResourceRateGridTMModelView(ws, modelView);
            Assert.IsTrue(result.WorkspaceResourceRateTMResults.ElementAt(0).StartDate.ToDateTime() >= result.WorkspaceResourceRateTMResults.ElementAt(1).StartDate.ToDateTime() &&
                result.WorkspaceResourceRateTMResults.ElementAt(1).StartDate.ToDateTime() >= result.WorkspaceResourceRateTMResults.ElementAt(2).StartDate.ToDateTime() &&
                result.WorkspaceResourceRateTMResults.ElementAt(2).StartDate.ToDateTime() >= result.WorkspaceResourceRateTMResults.ElementAt(3).StartDate.ToDateTime() &&
                result.WorkspaceResourceRateTMResults.ElementAt(3).StartDate.ToDateTime() >= result.WorkspaceResourceRateTMResults.ElementAt(4).StartDate.ToDateTime());

            modelView = new WorkspaceResourceRateGridTMModelView()
            {
                SearchFilter = null,
                SortField = WorkspaceResourceRateTMModelView.SORT_ID_RESOURCE_DESCRIPTION,
                OldSortField = WorkspaceResourceRateTMModelView.SORT_ID_RESOURCE_DESCRIPTION,
                Order = SortOrder.Descending,
                SortChanged = true
            };
            result = sut.GetWorkspaceResourceRateGridTMModelView(ws, modelView);
            Assert.IsTrue(result.WorkspaceResourceRateTMResults.ElementAt(0).ResourceDescription.CompareTo(result.WorkspaceResourceRateTMResults.ElementAt(1).ResourceDescription) <= 0);
            Assert.IsTrue(result.WorkspaceResourceRateTMResults.ElementAt(1).ResourceDescription.CompareTo(result.WorkspaceResourceRateTMResults.ElementAt(2).ResourceDescription) <= 0);
            Assert.IsTrue(result.WorkspaceResourceRateTMResults.ElementAt(3).ResourceDescription.CompareTo(result.WorkspaceResourceRateTMResults.ElementAt(3).ResourceDescription) <= 0);
            Assert.IsTrue(result.WorkspaceResourceRateTMResults.ElementAt(4).ResourceDescription.CompareTo(result.WorkspaceResourceRateTMResults.ElementAt(4).ResourceDescription) <= 0);

            modelView = new WorkspaceResourceRateGridTMModelView()
            {
                SearchFilter = null,
                SortField = WorkspaceResourceRateTMModelView.SORT_ID_RESOURCE_DESCRIPTION,
                OldSortField = WorkspaceResourceRateTMModelView.SORT_ID_RESOURCE_DESCRIPTION,
                Order = SortOrder.Ascending,
                SortChanged = true
            };
        }

        #region GetWorkspaceResourceRateTMGridModelViews Tests

        [TestMethod]
        public void GetWorkspaceResourceRateGridTMModelViewsTest()
        {
            WorkspaceControllerLogic sut = CreateSystemMST();
            DoGetWorkspaceResourceRateGridTMModelViewsTest(sut);
        }

        [TestMethod]
        public void GetWorkspaceResourceRateGridTMModelViewsTestSpaceSystems()
        {
            WorkspaceControllerLogic sut = CreateSystemSpaceSystems();
            DoGetWorkspaceResourceRateGridTMModelViewsTest(sut);
        }
        #endregion


        #region IsWorkspaceResourceDateWithinContract Tests
        [TestMethod]
        public void IsWorkspaceResourceDateWithinContractTest()
        {
            WorkspaceControllerLogic sut = CreateSystemMST();
            DoIsWorkspaceResourceDateWithinContractTest(sut);
        }

        [TestMethod]
        public void IsWorkspaceResourceDateWithinContractTestSpaceSystems()
        {
            WorkspaceControllerLogic sut = CreateSystemSpaceSystems();
            DoIsWorkspaceResourceDateWithinContractTest(sut);
        }
        #endregion

        private void DoIsWorkspaceResourceDateWithinContractTest(IWorkspaceControllerLogic sut)
        {

            WorkspaceDTO workspaceDTO = new WorkspaceDTO()
            {
                Id = 4,
                ContractStartDate = new DateTime(2011, 1, 1),
                ContractEndDate = new DateTime(2012, 12, 1),
                Shortname = "testWorkspace"
            };
            FullWorkspace ws = new FullWorkspace(workspaceDTO);

            //_workspaceMapper.Setup(x => x.GetWorkspaceByShortName(workspaceDTO.Shortname, false)).Returns(workspaceDTO);

            Assert.IsTrue(sut.IsWorkspaceResourceDateWithinContract(ws, "abcd"));
            Assert.IsTrue(sut.IsWorkspaceResourceDateWithinContract(ws, "01/2011"));
            Assert.IsTrue(sut.IsWorkspaceResourceDateWithinContract(ws, "12/2012"));
            Assert.IsTrue(sut.IsWorkspaceResourceDateWithinContract(ws, "01/2012"));
            Assert.IsFalse(sut.IsWorkspaceResourceDateWithinContract(ws, "01/2010"));
            Assert.IsFalse(sut.IsWorkspaceResourceDateWithinContract(ws, "01/2015"));
        }
        
        #region CalculateLinkedTaskElements Tests
        [TestMethod]
        public void CalculateLinkedTaskElementsTest()
        {
            WorkspaceControllerLogic sut = CreateSystemMST();
            DoCalculateLinkedTaskElementsTest(sut);
        }

        [TestMethod]
        public void CalculateLinkedTaskElementsTestSpaceSystems()
        {
            WorkspaceControllerLogic sut = CreateSystemSpaceSystems();
            DoCalculateLinkedTaskElementsTest(sut);
        }
        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void DoCalculateLinkedTaskElementsTest(IWorkspaceControllerLogic sut)
        {
            //test
            WorkspaceDTO workspace = new WorkspaceDTO { Id = 2 };
            ResourceDTO resource = new ResourceDTO { Id = 6 };
            PerformingOrgDTO perfOrg = new PerformingOrgDTO { Id = 7 };

            FullWorkspace ws = new FullWorkspace(workspace);

            // For bug 9534. We have a discrete workspace variable that is being updated. However, this workspace variable is in use by boe c. boe c should be directly updated. boe b uses a task variable
            // that uses boe c so boe b should also be updated. if boe b is being updated, then so should boe a because it's using a task variable of boe b
            WorkspaceVariableDTO workspaceVarB = new WorkspaceVariableDTO { WorkspaceID = workspace.Id, Id = 3, WorkspaceVariableName = "BAGS", WorkspaceVariableValue = 200m, ValueType = VarValueType.Discrete };

            BoeDTO boeC = new BoeDTO { Id = 1, WorkspaceID = workspace.Id };
            ResourceSpreadDto boeLSC = new ResourceSpreadDto { BoeID = boeC.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 200 };
            ResourceTypeDto boeLaborC = new ResourceTypeDto { BoeID = boeC.Id, Id = 1, SpreadType = IES.Common.SpreadType.Hours, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 200, PercentSpread = 77, PerformingOrgID = perfOrg.Id, ResourceID = resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSC } };
            BoeTaskElementDTO boeTaskElementC = new BoeTaskElementDTO { Id = 1, BoeID = boeC.Id, MOQHoursEquation = "BAGS", LaborTypeWarningFlag = false, WorkspaceVariableIDs = new Collection<int> { workspaceVarB.Id }, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborC }, TaskElementType = TaskElementType.Labor };

            BoeDTO boeB = new BoeDTO { Id = 2, WorkspaceID = workspace.Id };
            OrdinaryVariableDto taskOrdinaryVarB = new OrdinaryVariableDto { BoeID = boeB.Id, Id = 2, OrdinaryVariableName = "BLAH", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = boeC.Id, CLINID = null, WBSID = null } } };
            ResourceSpreadDto boeLSB = new ResourceSpreadDto { BoeID = boeB.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLaborB = new ResourceTypeDto { BoeID = boeB.Id, Id = 2, SpreadType = IES.Common.SpreadType.Hours, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = perfOrg.Id, ResourceID = resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSB } };
            BoeTaskElementDTO boeTaskElementB = new BoeTaskElementDTO { Id = 2, BoeID = boeB.Id, MOQHoursEquation = "15000 + BLAH", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVarB }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborB }, TaskElementType = TaskElementType.Labor };

            BoeDTO boeA = new BoeDTO { Id = 3, WorkspaceID = workspace.Id, State = BOEState.AwaitingApproval };
            OrdinaryVariableDto taskOrdinaryVarA = new OrdinaryVariableDto { BoeID = boeA.Id, Id = 3, OrdinaryVariableName = "BLAH3", OrdinaryVariableValue = 500m, ValueType = VarValueType.SumOfBOEs, SortBOEBy = VarSortBOEBy.WBS, SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = boeB.Id, CLINID = null, WBSID = null } } };
            ResourceSpreadDto boeLSA = new ResourceSpreadDto { BoeID = boeA.Id, Id = 1, LaborSpreadDate = Convert.ToDateTime("04/20/2011"), LaborSpreadValue = 6000 };
            ResourceTypeDto boeLaborA = new ResourceTypeDto { BoeID = boeA.Id, Id = 3, SpreadType = IES.Common.SpreadType.Hours, SpreadCurveID = SpreadCurves.SpreadCurve13, ValueSpread = 25000, PercentSpread = 77, PerformingOrgID = perfOrg.Id, ResourceID = resource.Id, StartDateValue = Convert.ToDateTime("04/20/2011"), EndDateValue = Convert.ToDateTime("04/20/2011"), LaborSpreads = new Collection<ResourceSpreadDto> { boeLSA } };
            BoeTaskElementDTO boeTaskElementA = new BoeTaskElementDTO { Id = 3, BoeID = boeA.Id, MOQHoursEquation = "15000 + BLAH3", OrdinaryVariables = new Collection<OrdinaryVariableDto> { taskOrdinaryVarA }, LaborTypeWarningFlag = false, taskElementLabors = new Collection<ResourceTypeDto> { boeLaborA }, TaskElementType = TaskElementType.Labor };

            FullBoe boeAObject = new FullBoe(boeA);
            FullBoe boeBObject = new FullBoe(boeB);
            FullBoe boeCObject = new FullBoe(boeC);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new List<FullBoe>() { boeAObject, boeBObject, boeCObject });
            this.retriever.Setup(x => x.GetWorkspaceById(ws.Id)).Returns(workspace);
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementC, boeTaskElementB, boeTaskElementA });

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
            this.retriever.Setup(x => x.GetFullWorkspaceById(workspace.Id)).Returns(ws);
            this.factory.Setup(x => x.CreateFullBoes(It.IsAny<Collection<int>>())).Returns(new Collection<FullBoe> { boeAObject, boeBObject, boeCObject });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new Collection<WorkspaceVariableDTO> { });

            _BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeCObject, VariableType.Workspace, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementB });
            _BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeCObject, VariableType.Task, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });
            _BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeBObject, VariableType.Task, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { boeTaskElementA });
            _BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeBObject, VariableType.Workspace, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });
            _BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeAObject, VariableType.Task, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });
            _BoeTaskElementRecalculation.Setup(x => x.RecalculateLaborWithBoe(boeAObject, VariableType.Workspace, ws, It.IsAny<Collection<BoeTaskElementDTO>>(), It.IsAny<Collection<WorkspaceVariableDTO>>())).Returns(new Collection<BoeTaskElementDTO> { });

            string state = "";
            _boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boeAObject, ws, BOEState.AwaitingApproval, BOEState.Draft, out state)).Returns(true);

            // for this test case, we are updating C which kick off direct changes to B and then indirect changes to A
            sut.CalculateLinkedTaskElements(new Collection<BoeTaskElementDTO> { boeTaskElementC }, ws);

            this.fullWsRecalc.Verify(x => x.ValidateStateTransitionForBoesEffectedByRecalculation(ws, It.IsAny<HashSet<FullBoe>>(), It.IsAny<HashSet<BoeDTO>>()), Times.Once());
            this.fullWsRecalc.Verify(x => x.SaveDataEffectedByRecalculation(ws, It.IsAny<HashSet<BoeTaskElementDTO>>(), It.IsAny<HashSet<WorkspaceVariableDTO>>(), It.IsAny<HashSet<FullBoe>>()), Times.Once());
            this.fullWsRecalc.Verify(x => x.PerformStateTransitionActionsForBoesEffectedByRecalculation(ws, It.IsAny<HashSet<FullBoe>>(), It.IsAny<HashSet<BoeDTO>>()), Times.Once());
        }

        #region GetFilteredWorkspaceResources Tests
        [TestMethod]
        public void GetFilteredWorkspaceResourcesSpaceSystems()
        {
            WorkspaceControllerLogic sut = CreateSystemSpaceSystems();

            ResourceDTO workspaceResource1 = new ResourceDTO()
            {
                Id = 5,
                ResourceName = "ABCD",
                ResourceDesc = "DescriptionOne",
                Segment = SegmentType.SSC,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Cost
            };

            ResourceDTO workspaceResource2 = new ResourceDTO()
            {
                Id = 6,
                ResourceName = "EFGH",
                ResourceDesc = "DescriptionTwo",
                Segment = SegmentType.SSC,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Hours
            };

            ResourceDTO workspaceResource3 = new ResourceDTO()
            {
                Id = 7,
                ResourceName = "IJKL",
                ResourceDesc = "DescriptionThree",
                Segment = SegmentType.SSC,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Cost
            };

            ResourceDTO workspaceResource4 = new ResourceDTO()
            {
                Id = 8,
                ResourceName = "MNOP",
                ResourceDesc = "DescriptionFour",
                Segment = SegmentType.SSC,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Hours
            };

            ResourceDTO workspaceResource5 = new ResourceDTO()
            {
                Id = 9,
                ResourceName = "QRST",
                ResourceDesc = "DescriptionFour",
                Segment = SegmentType.SSC,
                ElementOfCost = ElementOfCostType.Sub,
                RateType = RateType.Cost
            };

            ResourceDTO workspaceResource6 = new ResourceDTO()
            {
                Id = 10,
                ResourceName = "UVWX",
                ResourceDesc = "DescriptionFour",
                Segment = SegmentType.SSC,
                ElementOfCost = ElementOfCostType.Sub,
                RateType = RateType.Hours
            };

            ResourceDTO workspaceResource7 = new ResourceDTO()
            {
                Id = 11,
                ResourceName = "YZ",
                ResourceDesc = "DescriptionFour",
                Segment = SegmentType.SSC,
                ElementOfCost = ElementOfCostType.IWTA,
                RateType = RateType.Cost
            };

            ResourceDTO workspaceResource8 = new ResourceDTO()
            {
                Id = 12,
                ResourceName = "A",
                ResourceDesc = "DescriptionFour",
                Segment = SegmentType.SSC,
                ElementOfCost = ElementOfCostType.IWTA,
                RateType = RateType.Hours
            };

            ResourceDTO workspaceResource9 = new ResourceDTO()
            {
                Id = 13,
                ResourceName = "System1",
                ResourceDesc = "DescriptionFour",
                Segment = SegmentType.SSC,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Cost
            };

            Collection<ResourceDTO> workspaceResourceCollection = new Collection<ResourceDTO>() { workspaceResource1, workspaceResource2, workspaceResource3, workspaceResource4, workspaceResource5, workspaceResource6, workspaceResource7, workspaceResource8, workspaceResource9 };

            ICollection<ResourceDTO> result = sut.GetFilteredWorkspaceResources(workspaceResourceCollection);
            Assert.AreEqual(9, result.Count);

            result = sut.GetFilteredOtherWorkspaceResources(workspaceResourceCollection);
            Assert.AreEqual(9, result.Count);

            ICollection<ResourceDTO> resultTM = sut.GetFilteredWorkspaceResourcesTM(workspaceResourceCollection);
            Assert.AreEqual(2, resultTM.Count);
        }

        [TestMethod]
        public void GetFilteredWorkspaceResourcesMST()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();

            ResourceDTO workspaceResource1 = new ResourceDTO()
            {
                Id = 5,
                ResourceName = "ABCD",
                ResourceDesc = "DescriptionOne",
                Segment = SegmentType.RMS,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Cost
            };

            ResourceDTO workspaceResource2 = new ResourceDTO()
            {
                Id = 6,
                ResourceName = "EFGH",
                ResourceDesc = "DescriptionTwo",
                Segment = SegmentType.RMS,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Hours
            };

            ResourceDTO workspaceResource3 = new ResourceDTO()
            {
                Id = 7,
                ResourceName = "IJKL",
                ResourceDesc = "DescriptionThree",
                Segment = SegmentType.RMS,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Cost
            };

            ResourceDTO workspaceResource4 = new ResourceDTO()
            {
                Id = 8,
                ResourceName = "MNOP",
                ResourceDesc = "DescriptionFour",
                Segment = SegmentType.RMS,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Hours
            };

            ResourceDTO workspaceResource5 = new ResourceDTO()
            {
                Id = 9,
                ResourceName = "QRST",
                ResourceDesc = "DescriptionFour",
                Segment = SegmentType.RMS,
                ElementOfCost = ElementOfCostType.Sub,
                RateType = RateType.Cost
            };

            ResourceDTO workspaceResource6 = new ResourceDTO()
            {
                Id = 10,
                ResourceName = "UVWX",
                ResourceDesc = "DescriptionFour",
                Segment = SegmentType.RMS,
                ElementOfCost = ElementOfCostType.Sub,
                RateType = RateType.Hours
            };

            ResourceDTO workspaceResource7 = new ResourceDTO()
            {
                Id = 11,
                ResourceName = "YZ",
                ResourceDesc = "DescriptionFour",
                Segment = SegmentType.RMS,
                ElementOfCost = ElementOfCostType.IWTA,
                RateType = RateType.Cost
            };

            ResourceDTO workspaceResource8 = new ResourceDTO()
            {
                Id = 12,
                ResourceName = "A",
                ResourceDesc = "DescriptionFour",
                Segment = SegmentType.RMS,
                ElementOfCost = ElementOfCostType.IWTA,
                RateType = RateType.Hours
            };

            ResourceDTO workspaceResource9 = new ResourceDTO()
            {
                Id = 13,
                ResourceName = "System1",
                ResourceDesc = "DescriptionFour",
                Segment = SegmentType.RMS,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Cost
            };

            Collection<ResourceDTO> workspaceResourceCollection = new Collection<ResourceDTO>() { workspaceResource1, workspaceResource2, workspaceResource3, workspaceResource4, workspaceResource5, workspaceResource6, workspaceResource7, workspaceResource8, workspaceResource9 };

            ICollection<ResourceDTO> result = sut.GetFilteredWorkspaceResources(workspaceResourceCollection);
            Assert.AreEqual(9, result.Count);

            result = sut.GetFilteredOtherWorkspaceResources(workspaceResourceCollection);
            Assert.AreEqual(9, result.Count);

            ICollection<ResourceDTO> resultTM = sut.GetFilteredWorkspaceResourcesTM(workspaceResourceCollection);
            Assert.AreEqual(2, resultTM.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFilteredWorkspaceResourcesExceptionSSC()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            sut.GetFilteredWorkspaceResources(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFilteredWorkspaceResourcesExceptionMST()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();
            sut.GetFilteredWorkspaceResources(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFilteredOtherWorkspaceResourcesExceptionSSC()
        {
            WorkspaceControllerLogicSpaceSystems sut = CreateSystemSpaceSystems();
            sut.GetFilteredOtherWorkspaceResources(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFilteredOtherWorkspaceResourcesExceptionMST()
        {
            WorkspaceControllerLogicMST sut = CreateSystemMST();
            sut.GetFilteredOtherWorkspaceResources(null);
        }
        #endregion

        #region GetExportBOEGridData Tests
        [TestMethod]
        public void GetExportBOEGridDataTest()
        {
            IWorkspaceControllerLogic sut = CreateSystemMST();
            DoGetExportBOEGridDataTest(sut);

        }

        [TestMethod]
        public void GetExportBOEGridDataTestSpaceSystems()
        {
            IWorkspaceControllerLogic sut = CreateSystemSpaceSystems();
            DoGetExportBOEGridDataTest(sut);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetExportBOEGridDataExceptionTest()
        {
            WorkspaceControllerLogic sut = CreateSystemMST();

            sut.GetExportBOEModelData(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetExportBOEGridDataExceptionTestSpaceSystems()
        {
            WorkspaceControllerLogic sut = CreateSystemSpaceSystems();

            sut.GetExportBOEModelData(null);
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void DoGetExportBOEGridDataTest(IWorkspaceControllerLogic sut)
        {
            WorkspaceDTO workspace = new WorkspaceDTO()
            {
                Id = 1,
                ContractStartDate = new DateTime(2011, 1, 1),
                ContractEndDate = new DateTime(2012, 12, 1),
                Shortname = "testWorkspace"
            };

            WorkspaceDTO inSummaryWorkspace = new WorkspaceDTO()
            {
                Id = 1,
                ContractStartDate = new DateTime(2011, 1, 1),
                ContractEndDate = new DateTime(2012, 12, 1),
                Shortname = "testInSummaryWorkspace"
            };

            FullClin clin1 = new FullClin()
            {
                Id = 1,
                ClinNumber = "Clin1",
                ClinTitle = "Clin1 Title",
                ClinPaddedNumber = "Clin1",
                WorkspaceID = workspace.Id
            };

            WbsDTO wbs1 = new WbsDTO()
            {
                 Id = 1,
                 WbsNumber = "1.0",
                 WbsTitle = "WBS 1.0",
                 WorkspaceID = workspace.Id
            };

            UserDTO author = new UserDTO()
            {
                UserID = 1,
                DisplayName = "Author",
                NTID = "author"
            };
            UserDTO subAuthor = new UserDTO()
            {
                UserID = 3,
                DisplayName = "SubAuthor",
                NTID = "subauthor"
            };
            UserDTO approver = new UserDTO()
            {
                UserID = 5,
                DisplayName = "Approver",
                NTID = "approver"
            };
            UserDTO wsadmin = new UserDTO()
            {
                UserID = 7,
                DisplayName = "WorkspaceAdmin",
                NTID = "wsadmin"
            };

            BoeDTO boe1 = new BoeDTO()
            {
                AuthorIDs = { author.UserID },
                CLINID = clin1.Id,
                Id = 1,
                SubcontractorAuthorIDs = {  },
                isMaterial = false,
                State = BOEState.Draft,
                WorkspaceID = workspace.Id, 
                WBSID = wbs1.Id
            };
            BoeDTO boe2 = new BoeDTO()
            {
                AuthorIDs = {  },
                CLINID = clin1.Id,
                Id = 2,
                SubcontractorAuthorIDs = { subAuthor.UserID },
                isMaterial = false,
                State = BOEState.Draft,
                WorkspaceID = workspace.Id,
                WBSID = wbs1.Id
            };
            BoeDTO boe3 = new BoeDTO()
            {
                AuthorIDs = { 999 },
                CLINID = clin1.Id,
                Id = 3,
                SubcontractorAuthorIDs = { 9999 },
                isMaterial = false,
                State = BOEState.Draft,
                WorkspaceID = workspace.Id,
                WBSID = wbs1.Id
            };
            BoeDTO boe4 = new BoeDTO()
            {
                AuthorIDs = { author.UserID },
                CLINID = clin1.Id,
                Id = 4,
                SubcontractorAuthorIDs = {  },
                isMaterial = false,
                State = BOEState.Draft,
                WorkspaceID = workspace.Id,
                WBSID = wbs1.Id
            };
            BoeDTO boe5 = new BoeDTO()
            {
                AuthorIDs = {author.UserID },
                CLINID = clin1.Id,
                Id = 5,
                SubcontractorAuthorIDs = {  },
                isMaterial = true,
                State = BOEState.Draft,
                WorkspaceID = workspace.Id,
                WBSID = wbs1.Id
            };

            BoeDTO boe6 = new BoeDTO()
            {
                AuthorIDs = { author.UserID },
                CLINID = clin1.Id,
                Id = 6,
                SubcontractorAuthorIDs = {  },
                isMaterial = false,
                State = BOEState.Approved,
                WorkspaceID = workspace.Id,
                WBSID = wbs1.Id
            };
            BoeDTO boe7 = new BoeDTO()
            {
                AuthorIDs = { author.UserID },
                CLINID = clin1.Id,
                Id = 7,
                SubcontractorAuthorIDs = {  },
                isMaterial = false,
                State = BOEState.AwaitingApproval,
                WorkspaceID = workspace.Id,
                WBSID = wbs1.Id
            };
            BoeDTO boe8 = new BoeDTO()
            {
                AuthorIDs = { author.UserID },
                CLINID = clin1.Id,
                Id = 8,
                SubcontractorAuthorIDs = {  },
                isMaterial = false,
                State = BOEState.None,
                WorkspaceID = workspace.Id,
                WBSID = wbs1.Id
            };
            BoeDTO boe9 = new BoeDTO()
            {
                AuthorIDs = { author.UserID },
                CLINID = clin1.Id,
                Id = 9,
                SubcontractorAuthorIDs = {  },
                isMaterial = false,
                State = BOEState.DateShiftDraft,
                WorkspaceID = workspace.Id,
                WBSID = wbs1.Id
            };
            
            retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id)).Returns(new List<FullBoe>() { new FullBoe(boe1), new FullBoe(boe2), new FullBoe(boe3), new FullBoe(boe4), new FullBoe(boe5), new FullBoe(boe6), new FullBoe(boe7), new FullBoe(boe8), new FullBoe(boe9)});
            retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(workspace.Id)).Returns(new List<FullWbs>() { new FullWbs(wbs1) });
            retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new List<FullClin>() { clin1 });

            Collection<PermissionsDTO> authorPermissions = new Collection<PermissionsDTO>() {
                new PermissionsDTO() {
                     Role = Role.Author,
                     ETIUserId = author.UserID
                }
            };

            List<PermissionsDTO> subAuthorPermissions = new List<PermissionsDTO>() {
                new PermissionsDTO() {
                     Role = Role.SubcontractorAuthor,
                     ETIUserId = subAuthor.UserID
                }
            };

            List<PermissionsDTO> wsAdminPermissions = new List<PermissionsDTO>() {
                new PermissionsDTO() {
                     Role = Role.WorkspaceAdmin,
                     ETIUserId = wsadmin.UserID
                }
            };

            List<PermissionsDTO> noPermissions = new List<PermissionsDTO>();

            Collection<PermissionsDTO> boe1Permissions = new Collection<PermissionsDTO>()
            {
                new PermissionsDTO()
                {
                    Role = Role.Author,
                    ETIUserId = author.UserID,
                    BOEId = boe1.Id
                }
            };

            Collection<PermissionsDTO> boe2Permissions = new Collection<PermissionsDTO>()
            {
                new PermissionsDTO()
                {
                Role = Role.SubcontractorAuthor,
                ETIUserId = subAuthor.UserID,
                BOEId = boe2.Id
                }
            };
            Collection<PermissionsDTO> boe3Permissions = new Collection<PermissionsDTO>()
            {
                new PermissionsDTO()
                {
                    Role = Role.Author,
                    ETIUserId = 999,
                    BOEId = boe3.Id
                },
                new PermissionsDTO()
                {
                    Role = Role.SubcontractorAuthor,
                    ETIUserId = 9999,
                    BOEId = boe3.Id
                },
                new PermissionsDTO()
                {
                    Role = Role.WorkspaceAdmin,
                    ETIUserId = 99999,
                    BOEId = boe3.Id
                }
            };
            Collection<PermissionsDTO> boe4Permissions = new Collection<PermissionsDTO>()
            {
                new PermissionsDTO()
                {
                    Role = Role.Author,
                    ETIUserId = author.UserID,
                    BOEId = boe4.Id
                }
            };
            Collection<PermissionsDTO> boe5Permissions = new Collection<PermissionsDTO>()
            {
                new PermissionsDTO()
                {
                Role = Role.Author,
                ETIUserId = author.UserID,
                BOEId = boe5.Id
                }
            };
            Collection<PermissionsDTO> boe6Permissions = new Collection<PermissionsDTO>()
            {
                new PermissionsDTO()
                {
                Role = Role.Author,
                ETIUserId = author.UserID,
                BOEId = boe6.Id
                }
            };
            Collection<PermissionsDTO> boe7Permissions = new Collection<PermissionsDTO>()
            {
                new PermissionsDTO()
                {
                Role = Role.Author,
                ETIUserId = author.UserID,
                BOEId = boe7.Id
                }
            };
            Collection<PermissionsDTO> boe8Permissions = new Collection<PermissionsDTO>()
            {
                new PermissionsDTO()
                {
                Role = Role.Author,
                ETIUserId = author.UserID,
                BOEId = boe8.Id
                }
            };
            Collection<PermissionsDTO> boe9Permissions = new Collection<PermissionsDTO>()
            {
                new PermissionsDTO()
                {
                Role = Role.Author,
                ETIUserId = author.UserID,
                BOEId = boe9.Id
                }
            };

            List<PermissionsDTO> boePermissions = new List<PermissionsDTO>();
                boePermissions.AddRange(boe1Permissions.ToList());
                boePermissions.AddRange(boe2Permissions.ToList());
                boePermissions.AddRange(boe3Permissions.ToList());
                boePermissions.AddRange(boe4Permissions.ToList());
                boePermissions.AddRange(boe5Permissions.ToList());
                boePermissions.AddRange(boe6Permissions.ToList());
                boePermissions.AddRange(boe7Permissions.ToList());
                boePermissions.AddRange(boe8Permissions.ToList());
                boePermissions.AddRange(boe9Permissions.ToList());


            List<PermissionsDTO> wsPermissions = new List<PermissionsDTO>();
            wsPermissions.AddRange(authorPermissions);
            wsPermissions.AddRange(subAuthorPermissions);
            wsPermissions.AddRange(wsAdminPermissions);

            Collection<PermissionsDTO> wsPermissionsCollection = new Collection<PermissionsDTO>(wsPermissions);

            _permissionLoader.Setup(x => x.GetWorkspacePermissions(It.IsAny<int>())).Returns(wsPermissionsCollection);
            _permissionLoader.Setup(x => x.GetBOEPermissions(It.IsAny<ICollection<int>>())).Returns(boePermissions.ToCollection());
            _commonDatamapper.Setup(x => x.getBOEStates()).Returns(new Collection<BOEStateModelView>() { new BOEStateModelView() { BOEStateID = (int)BOEState.Draft, BOEState = "Draft" },
                                                                                                        new BOEStateModelView() { BOEStateID = (int)BOEState.AwaitingApproval, BOEState = "Awaiting Approval" },
                                                                                                        new BOEStateModelView() { BOEStateID = (int)BOEState.None, BOEState = "None" },
                                                                                                        new BOEStateModelView() { BOEStateID = (int)BOEState.Unassigned, BOEState = "Unassigned" },
                                                                                                        new BOEStateModelView() { BOEStateID = (int)BOEState.DateShiftDraft, BOEState = "Date Shift Draft" },
                                                                                                        new BOEStateModelView() { BOEStateID = (int)BOEState.Approved, BOEState = "Approved" }});

            CollectionAssert.AreEqual((from p in _permissionLoader.Object.GetWorkspacePermissions(1)
                                                    where p.Role == Role.Author && p.ETIUserId == author.UserID
                                                    select p).ToList(), authorPermissions);

            CollectionAssert.AreEqual((from p in _permissionLoader.Object.GetWorkspacePermissions(1)
                             where p.Role == Role.SubcontractorAuthor && p.ETIUserId == author.UserID
                             select p).ToList(), noPermissions);

            CollectionAssert.AreEqual((from p in _permissionLoader.Object.GetWorkspacePermissions(1)
                             where p.Role == Role.Author && p.ETIUserId == subAuthor.UserID
                             select p).ToList(), noPermissions);
            
            CollectionAssert.AreEqual((from p in _permissionLoader.Object.GetWorkspacePermissions(1)
                             where p.Role == Role.SubcontractorAuthor && p.ETIUserId == subAuthor.UserID
                             select p).ToList(), subAuthorPermissions);

            CollectionAssert.AreEqual((from p in _permissionLoader.Object.GetWorkspacePermissions(1)
                             where p.Role == Role.Author && p.ETIUserId == approver.UserID
                             select p).ToList(), noPermissions);

            CollectionAssert.AreEqual((from p in _permissionLoader.Object.GetWorkspacePermissions(1)
                             where p.Role == Role.SubcontractorAuthor && p.ETIUserId == approver.UserID
                             select p).ToList(), noPermissions);

            CollectionAssert.AreEqual((from p in _permissionLoader.Object.GetWorkspacePermissions(1)
                             where p.Role == Role.Author && p.ETIUserId == wsadmin.UserID
                             select p).ToList(), noPermissions);

            CollectionAssert.AreEqual((from p in _permissionLoader.Object.GetWorkspacePermissions(1)
                             where p.Role == Role.SubcontractorAuthor && p.ETIUserId == wsadmin.UserID
                             select p).ToList(), noPermissions);

            CollectionAssert.AreEqual((from p in _permissionLoader.Object.GetWorkspacePermissions(1)
                             where p.Role == Role.WorkspaceAdmin && p.ETIUserId == author.UserID
                             select p).ToList(), noPermissions);
            
            CollectionAssert.AreEqual((from p in _permissionLoader.Object.GetWorkspacePermissions(1)
                             where p.Role == Role.WorkspaceAdmin && p.ETIUserId == subAuthor.UserID
                             select p).ToList(), noPermissions);

            CollectionAssert.AreEqual((from p in _permissionLoader.Object.GetWorkspacePermissions(1)
                             where p.Role == Role.WorkspaceAdmin && p.ETIUserId == approver.UserID
                             select p).ToList(), noPermissions);

            CollectionAssert.AreEqual((from p in _permissionLoader.Object.GetWorkspacePermissions(1)
                             where p.Role == Role.WorkspaceAdmin && p.ETIUserId == wsadmin.UserID
                             select p).ToList(), wsAdminPermissions);

            factory.Setup(x => x.CreateFullBoe(boe1.Id)).Returns(new FullBoe(boe1));
            factory.Setup(x => x.CreateFullBoe(boe2.Id)).Returns(new FullBoe(boe2));
            factory.Setup(x => x.CreateFullBoe(boe3.Id)).Returns(new FullBoe(boe3));
            factory.Setup(x => x.CreateFullBoe(boe4.Id)).Returns(new FullBoe(boe4));
            factory.Setup(x => x.CreateFullBoe(boe5.Id)).Returns(new FullBoe(boe5));
            factory.Setup(x => x.CreateFullBoe(boe6.Id)).Returns(new FullBoe(boe6));
            factory.Setup(x => x.CreateFullBoe(boe7.Id)).Returns(new FullBoe(boe7));
            factory.Setup(x => x.CreateFullBoe(boe8.Id)).Returns(new FullBoe(boe8));
            factory.Setup(x => x.CreateFullBoe(boe9.Id)).Returns(new FullBoe(boe9));

            retriever.Setup(x => x.GetFullBoesByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullBoe>() { new FullBoe(boe1), new FullBoe(boe2), new FullBoe(boe3),
                new FullBoe(boe4), new FullBoe(boe5), new FullBoe(boe6), new FullBoe(boe7), new FullBoe(boe8), new FullBoe(boe9)});

            // ===================================================================================================================
            // Tests for Author user
            // ===================================================================================================================
            retriever.Setup(x => x.GetCurrentActiveUser()).Returns(author);

            FullWorkspace ws = new FullWorkspace(workspace);
            FullWorkspace wsinSummaryWorkspace = new FullWorkspace(inSummaryWorkspace);

            // -- regular workspace
            ICollection<ExportBOEModelView> modelViews = sut.GetExportBOEModelData(ws);
            ICollection<ExportBOEModelView> isMaterialModelViews = new Collection<ExportBOEModelView>((from x in modelViews
                                                                                                      where x.isMaterial == true
                                                                                                      select x).ToList());

            Assert.AreEqual(7, modelViews.Count);
            Assert.AreEqual(1, isMaterialModelViews.Count);
            // -- "In Summary" workspace
            modelViews = sut.GetExportBOEModelData(wsinSummaryWorkspace);
            isMaterialModelViews = new Collection<ExportBOEModelView>((from x in modelViews
                                                                       where x.isMaterial == true
                                                                      select x).ToList());
            Assert.AreEqual(7, modelViews.Count);
            Assert.AreEqual(1, isMaterialModelViews.Count);

            // ===================================================================================================================
            // Tests for SubcontractorAuthor user
            // ===================================================================================================================
            
            retriever.Setup(x => x.GetCurrentActiveUser()).Returns(subAuthor);
            ws.RefreshCurrentActiveUser();

            // -- regular workspace
            modelViews = sut.GetExportBOEModelData(ws);
            isMaterialModelViews = new Collection<ExportBOEModelView>((from x in modelViews
                                                                       where x.isMaterial == true
                                                                       select x).ToList());
            Assert.AreEqual(1, modelViews.Count);
            Assert.AreEqual(0, isMaterialModelViews.Count);
            // -- "In Summary" workspace
            wsinSummaryWorkspace.RefreshCurrentActiveUser();
            modelViews = sut.GetExportBOEModelData(wsinSummaryWorkspace);
            isMaterialModelViews = new Collection<ExportBOEModelView>((from x in modelViews
                                                                       where x.isMaterial == true
                                                                       select x).ToList());
            Assert.AreEqual(1, modelViews.Count);
            Assert.AreEqual(0, isMaterialModelViews.Count);

            // ===================================================================================================================
            // Tests for Approver user
            // ===================================================================================================================
            retriever.Setup(x => x.GetCurrentActiveUser()).Returns(approver);
            ws.RefreshCurrentActiveUser();

            // -- regular workspace
            modelViews = sut.GetExportBOEModelData(ws);
            isMaterialModelViews = new Collection<ExportBOEModelView>((from x in modelViews
                                                                       where x.isMaterial == true
                                                                       select x).ToList());
            Assert.AreEqual(0, modelViews.Count);
            Assert.AreEqual(0, isMaterialModelViews.Count);
            // -- "In Summary" workspace
            wsinSummaryWorkspace.RefreshCurrentActiveUser();
            modelViews = sut.GetExportBOEModelData(wsinSummaryWorkspace);
            isMaterialModelViews = new Collection<ExportBOEModelView>((from x in modelViews
                                                                       where x.isMaterial == true
                                                                       select x).ToList());
            Assert.AreEqual(0, modelViews.Count);
            Assert.AreEqual(0, isMaterialModelViews.Count);


            // ===================================================================================================================
            // Tests for WorkspaceAdmin user
            // ===================================================================================================================
            retriever.Setup(x => x.GetCurrentActiveUser()).Returns(wsadmin);
            ws.RefreshCurrentActiveUser();

            // -- regular workspace
            modelViews = sut.GetExportBOEModelData(ws);
            isMaterialModelViews = new Collection<ExportBOEModelView>((from x in modelViews
                                                                       where x.isMaterial == true
                                                                       select x).ToList());
            Assert.AreEqual(9, modelViews.Count);
            Assert.AreEqual(1, isMaterialModelViews.Count);
            
            // -- "In Summary" workspace
            wsinSummaryWorkspace.RefreshCurrentActiveUser();
            modelViews = sut.GetExportBOEModelData(wsinSummaryWorkspace);
            isMaterialModelViews = new Collection<ExportBOEModelView>((from x in modelViews
                                                                       where x.isMaterial == true
                                                                       select x).ToList());
            Assert.AreEqual(9, modelViews.Count);
            Assert.AreEqual(1, isMaterialModelViews.Count);
        }
        
        [TestMethod]
        public void TestDeleteWorkspaceVariables()
        {
            IWorkspaceControllerLogic sut = CreateSystemSpaceSystems();
            WorkspaceVariableDTO variableDTO = new WorkspaceVariableDTO();

            workspaceVariableLoader.Setup(x => x.SaveWorkspaceVariables(It.IsAny<Collection<WorkspaceVariableDTO>>())).Verifiable();

            sut.DeleteWorkspaceVariables(new Collection<WorkspaceVariableDTO>() { variableDTO });

            workspaceVariableLoader.Verify(x => x.SaveWorkspaceVariables(It.IsAny<Collection<WorkspaceVariableDTO>>()), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestDeleteWorkspaceVariables_NullException()
        {
            IWorkspaceControllerLogic sut = CreateSystemSpaceSystems();
            sut.DeleteWorkspaceVariables(null);
        }

        /// <summary>
        /// Test GetLastUpdatedTimeOffload for SSC, always returns null
        /// </summary>
        [TestMethod]
        public void TestGetLastUpdatedTimeOffload()
        {
            IWorkspaceControllerLogic sut = this.CreateSystemSpaceSystems();
            DateTime? result = sut.GetLastupdatedTimeOffload(1);
            Assert.IsNull(result);
        }

        /// <summary>
        /// Test GetLastUpdatedTimeOffload for RMS when dates are out of date
        /// </summary>
        [TestMethod]
        public void TestGetLastUpdatedTimeOffload_RMS()
        {
            IWorkspaceControllerLogic sut = this.CreateSystemMST();

            ICollection<OffloadRatesDTO> rates = new Collection<OffloadRatesDTO>();
            rates.Add( new OffloadRatesDTO() {UpdateDate = DateTime.Now});

            this.offloadRatesLoader.Setup(x => x.AreCurrentOffloadRatesOutOfDate(It.IsAny<int>())).Returns(true);
            this.offloadRatesLoader.Setup(x => x.GetAllSystemRates()).Returns(rates);

            DateTime? result = sut.GetLastupdatedTimeOffload(1);

            Assert.AreEqual(rates.First().UpdateDate, result);
        }

        /// <summary>
        /// Test GetLastUpdatedTimeOffload for RMS when dates are up to date
        /// </summary>
        [TestMethod]
        public void TestGetLastUpdatedTimeOffload_RMS_UpToDate()
        {
            IWorkspaceControllerLogic sut = this.CreateSystemMST();

            this.offloadRatesLoader.Setup(x => x.AreCurrentOffloadRatesOutOfDate(It.IsAny<int>())).Returns(false);

            DateTime? result = sut.GetLastupdatedTimeOffload(1);

            Assert.IsNull(result);
        }

        /// <summary>
        /// Test that Total Cost of Travel is calculated appropriately for both domestic and international.
        /// </summary>
        [TestMethod]
        public void FindAdjacentBoes_Test()
        {
            HomeWorkspaceGridModelView model = new HomeWorkspaceGridModelView
            {
                items = new List<HomeWorkspaceModelView>
                {
                    new HomeWorkspaceModelView
                    {
                        ApproverOrderName = "aa",
                        AuthorOrderName = "aa",
                        BOEID = 1,
                        BOETitle = "1a",
                        WBSText = "1a",
                        CLINText = "1a",
                        isMaterial = "No",
                        UpdateDate = DateTime.Now.ToString(),
                        State = BOEState.Draft
                    },
                    new HomeWorkspaceModelView
                    {
                        ApproverOrderName = "bb",
                        AuthorOrderName = "bb",
                        BOEID = 2,
                        BOETitle = "2a",
                        WBSText = "2a",
                        CLINText = "2a",
                        isMaterial = "No",
                        UpdateDate = DateTime.Now.AddMinutes(5).ToString(),
                        State = BOEState.Draft
                    },
                    new HomeWorkspaceModelView
                    {
                        ApproverOrderName = "cc",
                        AuthorOrderName = "cc",
                        BOEID = 3,
                        BOETitle = "3a",
                        WBSText = "3a",
                        CLINText = "3a",
                        isMaterial = "Yes",
                        UpdateDate = DateTime.Now.AddMinutes(15).ToString(),
                        State = BOEState.AwaitingApproval
                    },
                    new HomeWorkspaceModelView
                    {
                        ApproverOrderName = "dd",
                        AuthorOrderName = "dd",
                        BOEID = 4,
                        BOETitle = "4a",
                        WBSText = "4a",
                        CLINText = "4a",
                        isMaterial = "No",
                        UpdateDate = DateTime.Now.AddMinutes(25).ToString(),
                        State = BOEState.Approved
                    }
                }
            };

            WorkspaceControllerLogic sut = this.CreateSystemMST();

            #region Test WBSText
            AdjacentItems items = sut.FindAdjacentBoes(model, 1, "WBSText", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.IsNull(items.PreviousId);
            Assert.AreEqual(2, items.NextId);

            items = sut.FindAdjacentBoes(model, 2, "WBSText", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.PreviousId);
            Assert.AreEqual(3, items.NextId);

            items = sut.FindAdjacentBoes(model, 3, "WBSText", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.PreviousId);
            Assert.AreEqual(4, items.NextId);

            items = sut.FindAdjacentBoes(model, 4, "WBSText", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.PreviousId);
            Assert.IsNull(items.NextId);

            items = sut.FindAdjacentBoes(model, 1, "WBSText", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.IsNull(items.NextId);
            Assert.AreEqual(2, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 2, "WBSText", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.NextId);
            Assert.AreEqual(3, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 3, "WBSText", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.NextId);
            Assert.AreEqual(4, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 4, "WBSText", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.NextId);
            Assert.IsNull(items.PreviousId);
            #endregion Test WBSText

            #region Test BOETitle
            items = sut.FindAdjacentBoes(model, 1, "BOETitle", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.IsNull(items.PreviousId);
            Assert.AreEqual(2, items.NextId);

            items = sut.FindAdjacentBoes(model, 2, "BOETitle", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.PreviousId);
            Assert.AreEqual(3, items.NextId);

            items = sut.FindAdjacentBoes(model, 3, "BOETitle", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.PreviousId);
            Assert.AreEqual(4, items.NextId);

            items = sut.FindAdjacentBoes(model, 4, "BOETitle", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.PreviousId);
            Assert.IsNull(items.NextId);

            items = sut.FindAdjacentBoes(model, 1, "BOETitle", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.IsNull(items.NextId);
            Assert.AreEqual(2, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 2, "BOETitle", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.NextId);
            Assert.AreEqual(3, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 3, "BOETitle", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.NextId);
            Assert.AreEqual(4, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 4, "BOETitle", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.NextId);
            Assert.IsNull(items.PreviousId);
            #endregion Test WBSText

            #region Test CLINText
            items = sut.FindAdjacentBoes(model, 1, "CLINText", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.IsNull(items.PreviousId);
            Assert.AreEqual(2, items.NextId);

            items = sut.FindAdjacentBoes(model, 2, "CLINText", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.PreviousId);
            Assert.AreEqual(3, items.NextId);

            items = sut.FindAdjacentBoes(model, 3, "CLINText", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.PreviousId);
            Assert.AreEqual(4, items.NextId);

            items = sut.FindAdjacentBoes(model, 4, "CLINText", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.PreviousId);
            Assert.IsNull(items.NextId);

            items = sut.FindAdjacentBoes(model, 1, "CLINText", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.IsNull(items.NextId);
            Assert.AreEqual(2, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 2, "CLINText", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.NextId);
            Assert.AreEqual(3, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 3, "CLINText", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.NextId);
            Assert.AreEqual(4, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 4, "CLINText", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.NextId);
            Assert.IsNull(items.PreviousId);
            #endregion Test CLINText

            #region Test AuthorOrderName
            items = sut.FindAdjacentBoes(model, 1, "AuthorOrderName", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.IsNull(items.PreviousId);
            Assert.AreEqual(2, items.NextId);

            items = sut.FindAdjacentBoes(model, 2, "AuthorOrderName", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.PreviousId);
            Assert.AreEqual(3, items.NextId);

            items = sut.FindAdjacentBoes(model, 3, "AuthorOrderName", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.PreviousId);
            Assert.AreEqual(4, items.NextId);

            items = sut.FindAdjacentBoes(model, 4, "AuthorOrderName", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.PreviousId);
            Assert.IsNull(items.NextId);

            items = sut.FindAdjacentBoes(model, 1, "AuthorOrderName", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.IsNull(items.NextId);
            Assert.AreEqual(2, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 2, "AuthorOrderName", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.NextId);
            Assert.AreEqual(3, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 3, "AuthorOrderName", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.NextId);
            Assert.AreEqual(4, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 4, "AuthorOrderName", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.NextId);
            Assert.IsNull(items.PreviousId);
            #endregion Test AuthorOrderName

            #region Test ApproverOrderName
            items = sut.FindAdjacentBoes(model, 1, "ApproverOrderName", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.IsNull(items.PreviousId);
            Assert.AreEqual(2, items.NextId);

            items = sut.FindAdjacentBoes(model, 2, "ApproverOrderName", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.PreviousId);
            Assert.AreEqual(3, items.NextId);

            items = sut.FindAdjacentBoes(model, 3, "ApproverOrderName", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.PreviousId);
            Assert.AreEqual(4, items.NextId);

            items = sut.FindAdjacentBoes(model, 4, "ApproverOrderName", SortOrder.Ascending);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.PreviousId);
            Assert.IsNull(items.NextId);

            items = sut.FindAdjacentBoes(model, 1, "ApproverOrderName", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.IsNull(items.NextId);
            Assert.AreEqual(2, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 2, "ApproverOrderName", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.NextId);
            Assert.AreEqual(3, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 3, "ApproverOrderName", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.NextId);
            Assert.AreEqual(4, items.PreviousId);

            items = sut.FindAdjacentBoes(model, 4, "ApproverOrderName", SortOrder.Descending);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.NextId);
            Assert.IsNull(items.PreviousId);
            #endregion Test ApproverOrderName

            items = sut.FindAdjacentBoes(model, 3, "isMaterial", SortOrder.Ascending);
            Assert.IsNull(items.NextId);
            Assert.IsNotNull(items.PreviousId);

            items = sut.FindAdjacentBoes(model, 3, "isMaterial", SortOrder.Descending);
            Assert.IsNull(items.PreviousId);
            Assert.IsNotNull(items.NextId);

            items = sut.FindAdjacentBoes(model, 4, "Status", SortOrder.Ascending);
            Assert.IsNull(items.PreviousId);
            Assert.AreEqual(3, items.NextId);

            items = sut.FindAdjacentBoes(model, 3, "Status", SortOrder.Ascending);
            Assert.AreEqual(4, items.PreviousId);
            Assert.AreEqual(1, items.NextId);
        }

        #region Backup Export Tests

        /// <summary>
        /// Test CopyWorkspaceVersion for copying all BOEs
        /// </summary>
        [TestMethod]
        public void CopyWorkspaceVersionTest()
        {
            WorkspaceControllerLogic sut = this.CreateSystemSpaceSystems();

            WorkspaceDTO workspace = new WorkspaceDTO()
            {
                Shortname = "testWorkspace",
                Id = 1
            };
            FullWorkspace ws = new FullWorkspace(workspace);

            int versionId = 2;
            int copyWsId = 3;

            this.wsLoader.Setup(w => w.CopyWorkspaceVersion(ws.Id, It.IsAny<string>(), It.IsAny<string>(), versionId, string.Empty)).Returns(copyWsId);

            int result = sut.CopyWorkspaceVersion(ws, versionId, true, new Collection<int>());

            Assert.AreEqual(copyWsId, result);
        }

        /// <summary>
        /// Test CopyWorkspaceVersion for copying select BOEs
        /// </summary>
        [TestMethod]
        public void CopyWorkspaceVersionTest_SelectBoes()
        {
            WorkspaceControllerLogic sut = this.CreateSystemSpaceSystems();

            WorkspaceDTO workspace = new WorkspaceDTO()
            {
                Shortname = "testWorkspace",
                Id = 1
            };
            FullWorkspace ws = new FullWorkspace(workspace);

            ICollection<int> selectBoes = new Collection<int>() { 1, 2, 3 };

            int versionId = 2;
            int copyWsId = 3;

            this.wsLoader.Setup(w => w.CopyWorkspaceVersion(ws.Id, It.IsAny<string>(), It.IsAny<string>(), versionId, string.Join(",", selectBoes))).Returns(copyWsId);

            int result = sut.CopyWorkspaceVersion(ws, versionId, false, selectBoes);

            Assert.AreEqual(copyWsId, result);
        }

        /// <summary>
        /// Test CopyWorkspaceVersion for throwing an exceptino for null Workspace param
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyWorkspaceVersionTest_Ex()
        {
            WorkspaceControllerLogic sut = this.CreateSystemSpaceSystems();
            int result = sut.CopyWorkspaceVersion(null, 1, false, null);
        }

        /// <summary>
        /// Tests that CreateWorkspaceDataReportForVersion throws an exception when ws param is null
        /// Note - rest of method cannot be tested since we cannot mock WorkspaceExporter
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateWorkspaceDataReportForVersionTest_EX()
        {
            WorkspaceControllerLogic sut = this.CreateSystemSpaceSystems();

            string result = sut.CreateWorkspaceDataReportForVersion(null, string.Empty, null, string.Empty, 1);
        }

        #endregion

        /// <summary>
        /// Tests changing of MOQ Types in SSC
        /// </summary>
        [TestMethod]
        public void SaveWorkspaceIdentificationValidation_MoqChangeTestSSC()
        {
            WorkspaceControllerLogicSpaceSystems sut = this.CreateSystemSpaceSystems();

            FullWorkspace ws = new FullWorkspace() { Id = 100 };
            IWorkspaceIdentificationModelView wsDetails = new WorkspaceIdentificationSpaceModelView() { WorkspaceID = 100, WorkspaceName = DateTime.Now.Ticks.ToString(), CostVolumeLeadPricerNTID = "paliderd" };
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());

            // No -> No -- Valid
            ws.UsingTemplateBOE = false;
            wsDetails.UsingTemplateBoe = false;
            Assert.IsFalse(sut.SaveWorkspaceIdentificationValidation(ws, wsDetails, false, false).Any());

            // No -> Yes -- Valid
            ws.UsingTemplateBOE = false;
            wsDetails.UsingTemplateBoe = true;
            Assert.IsFalse(sut.SaveWorkspaceIdentificationValidation(ws, wsDetails, false, false).Any());

            // Yes -> Yes -- Valid
            ws.UsingTemplateBOE = true;
            wsDetails.UsingTemplateBoe = true;
            Assert.IsFalse(sut.SaveWorkspaceIdentificationValidation(ws, wsDetails, false, false).Any());

            // Yes -> No -- InValid
            ws.UsingTemplateBOE = true;
            wsDetails.UsingTemplateBoe = false;
            Assert.IsTrue(sut.SaveWorkspaceIdentificationValidation(ws, wsDetails, false, false).Any());
        }

        /// <summary>
        /// Tests changing of MOQ Types in RMS
        /// </summary>
        [TestMethod]
        public void SaveWorkspaceIdentificationValidation_MoqChangeTestRMS()
        {
            WorkspaceControllerLogicMST sut = this.CreateSystemMST();

            FullWorkspace ws = new FullWorkspace() { Id = 100 };
            IWorkspaceIdentificationModelView wsDetails = new WorkspaceIdentificationMSTModelView() { WorkspaceID = 100, WorkspaceName = DateTime.Now.Ticks.ToString(), ShortName = DateTime.Now.ToShortTimeString(), CostVolumeLeadPricerNTID = "paliderd" };
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new List<FullClin>());
            
            // No -> No -- Valid
            ws.UsingTemplateBOE = false;
            wsDetails.UsingTemplateBoe = false;
            Assert.IsFalse(sut.SaveWorkspaceIdentificationValidation(ws, wsDetails, false, false).Any());

            // No -> Yes -- Valid
            ws.UsingTemplateBOE = false;
            wsDetails.UsingTemplateBoe = true;
            Assert.IsFalse(sut.SaveWorkspaceIdentificationValidation(ws, wsDetails, false, false).Any());

            // Yes -> Yes -- Valid
            ws.UsingTemplateBOE = true;
            wsDetails.UsingTemplateBoe = true;
            Assert.IsFalse(sut.SaveWorkspaceIdentificationValidation(ws, wsDetails, false, false).Any());

            // Yes -> No -- InValid
            ws.UsingTemplateBOE = true;
            wsDetails.UsingTemplateBoe = false;
            Assert.IsTrue(sut.SaveWorkspaceIdentificationValidation(ws, wsDetails, false, false).Any());
        }

        /// <summary>
        /// Tests Transforming MOQ Data for save
        /// </summary>
        [TestMethod]
        public void GetMoqTypesDataForBoeTemplateSettingChange_Test()
        {
            WorkspaceControllerLogicSpaceSystems sut = this.CreateSystemSpaceSystems();

            FullWorkspace ws = new FullWorkspace() { Id = 100, CostDecimalPrecision = 0, ResourceDecimalPrecision = 0, CreationDate = DateTime.Now };

            List<BoeTaskElementDTO> input = new List<BoeTaskElementDTO>()
            {
                new BoeTaskElementDTO()
                {
                    Id = 1,
                    MOQType = MOQType.SSCActual,
                    MOQText = "Text 1"
                },
                new BoeTaskElementDTO()
                {
                    Id = 2,
                    MOQType = MOQType.SSCBottomUp,
                    MOQText = "Text 2"
                }
            };

            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, false, ws.DecimalPrecision, ws.CostDecimalPrecision)).Returns(input);
			
            ICollection<MoqTypeSelection> result = sut.GetMoqTypesDataForBoeTemplateSettingChange(ws);

            Assert.AreEqual(input[0].Id, result.ElementAt(0).TaskId);
            Assert.AreEqual(input[0].MOQType.MapToNew(ws.CreationDate), result.ElementAt(0).SelectedMOQType);
            Assert.AreEqual(input[0].MOQText, result.ElementAt(0).Rationale);
            Assert.AreEqual(UpdateType.Upsert, result.ElementAt(0).Updateable);

            Assert.AreEqual(input[1].Id, result.ElementAt(1).TaskId);
            Assert.AreEqual(input[1].MOQType.MapToNew(ws.CreationDate), result.ElementAt(1).SelectedMOQType);
            Assert.AreEqual(input[1].MOQText, result.ElementAt(1).SmeReason);
            Assert.AreEqual(UpdateType.Upsert, result.ElementAt(1).Updateable);
        }

		/// <summary>
		/// Test Recalculating SAP Actuals
		/// </summary>
		[TestMethod]
		public async Task TestRecalculateActuals()
		{
			WorkspaceControllerLogicSpaceSystems sut = this.CreateSystemSpaceSystems();

			FullWorkspace ws = new FullWorkspace() { Id = 101, CostDecimalPrecision = 0, ResourceDecimalPrecision = 0, CreationDate = DateTime.Now };

			FullBoe boe1 = new FullBoe
			{
				Id = 1,
				State = BOEState.Draft,
				Title = "First Boe",
			};

			FullBoe boe2 = new FullBoe
			{
				Id = 2,
				State = BOEState.AwaitingApproval,
				Title = "Second Boe"
			};

			List<FullBoe> boes = new List<FullBoe>()
			{
				boe1,
				boe2
			};

			BoeTaskElementDTO task1 = new BoeTaskElementDTO()
			{
				Id = 1,
				BoeID = boe1.Id,
				MOQType = MOQType.SSCActual,
				MOQText = "Text 1"
			};

			BoeTaskElementDTO task2 = new BoeTaskElementDTO()
			{
				Id = 2,
				BoeID = boe2.Id,
				MOQType = MOQType.SSCBottomUp,
				MOQText = "Text 2"
			};

			List<BoeTaskElementDTO> tasks = new List<BoeTaskElementDTO>()
			{
				task1,
				task2	
			};

			List<MoqTypeSelection> moqs = new List<MoqTypeSelection>()
			{
				new MoqTypeSelection()
				{
					BoeId = boe1.Id,
					TaskId = task1.Id,
					Id = 1,
					SelectedMOQType = MOQType.Historical,
					TableData = new List<MoqTableData>
					{
						new MoqTableData()
						{
							TableName = "First table",
							Id = 1,
							DateOfReport = DateTime.Today.AddDays(-1),
							TotalRelevantHours = 10
						},
						new MoqTableData()
						{
							TableName = "Second table",
							Id = 2,
							DateOfReport = DateTime.Today.AddDays(-1),
							TotalRelevantHours = 15
						}
					}
				},
				new MoqTypeSelection()
				{
					BoeId = boe1.Id,
					TaskId = task1.Id,
					Id = 2,
					SelectedMOQType = MOQType.Comparative,
					TableData = new List<MoqTableData>
					{
						new MoqTableData()
						{
							TableName = "Third table",
							Id = 3,
							DateOfReport = DateTime.Today.AddDays(-1),
							TotalRelevantHours = 20
						}
					}
				},
				new MoqTypeSelection()
				{
					BoeId = boe2.Id,
					TaskId = task2.Id,
					Id = 3,
					SelectedMOQType = MOQType.Comparative,
					TableData = new List<MoqTableData>
					{
						new MoqTableData()
						{
							TableName = "Fourth table",
							Id = 4,
							DateOfReport = DateTime.Today.AddDays(-1),
							TotalRelevantHours = 25
						}
					}
				}
			};

			ICollection<IESResponse<IESSAPClient.CalculateActualsViewModel>> sapResponse = new List<IESResponse<IESSAPClient.CalculateActualsViewModel>>
			{
				new IESResponse<IESSAPClient.CalculateActualsViewModel>
				{
					IsSuccessful = true,
					Data = new List<IESSAPClient.CalculateActualsViewModel> ()
					{
						new IESSAPClient.CalculateActualsViewModel()
						{
							TableId = 1,
							TotalHours = 30
						}
					}
				},
				new IESResponse<IESSAPClient.CalculateActualsViewModel>
				{
					IsSuccessful = false,
					Data = new List<IESSAPClient.CalculateActualsViewModel> ()
					{
						new IESSAPClient.CalculateActualsViewModel()
						{
							TableId = 2
						}
					},
					Messages = new List<string>()
					{
						"Error happened"
					}
				},
				new IESResponse<IESSAPClient.CalculateActualsViewModel>
				{
					IsSuccessful = true,
					Data = new List<IESSAPClient.CalculateActualsViewModel> ()
					{
						new IESSAPClient.CalculateActualsViewModel()
						{
							TableId = 3,
							TotalHours = 20  // same total hours
						}
					}
				},
				new IESResponse<IESSAPClient.CalculateActualsViewModel>
				{
					IsSuccessful = true,
					Data = new List<IESSAPClient.CalculateActualsViewModel> ()
					{
						new IESSAPClient.CalculateActualsViewModel()
						{
							TableId = 4,
							TotalHours = 50
						}
					}
				},
			};

			this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, false, ws.DecimalPrecision, ws.CostDecimalPrecision)).Returns(tasks);
			this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(boes);
			this.retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(ws.Id)).Returns(moqs);
			this.retriever.Setup(x => x.GetCurrentActiveUser()).Returns(new UserDTO());
			this._BOELaborControllerLogic.Setup(x => x.CalculateAllActualsSap(It.IsAny<ICollection<MoqTableDataModelView>>())).Returns(Task.FromResult(sapResponse));
			this.factory.Setup(x => x.CreateFullBoe(boe1)).Returns(boe1);
			this.factory.Setup(x => x.CreateFullBoe(boe2)).Returns(boe2);
			string validationMessage;
			this._boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe1, It.IsAny<FullWorkspace>(), BOEState.Draft, BOEState.Draft, out validationMessage)).Returns(true);
			this._boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe2, It.IsAny<FullWorkspace>(), BOEState.AwaitingApproval, BOEState.Draft, out validationMessage)).Returns(true);

			ICollection<WorkspaceCalculateActualsModelView> models = await sut.RecalculateActuals(ws);

			Assert.IsNotNull(models);
			Assert.AreEqual(3, models.Count); // only getting 3 models because one had same hours as previous

			WorkspaceCalculateActualsModelView first = models.First();
			WorkspaceCalculateActualsModelView second = models.Skip(1).First();
			WorkspaceCalculateActualsModelView fourth = models.Last(); // the fourth table

			Assert.IsTrue(first.IsSuccessful);
			Assert.IsFalse(second.IsSuccessful);
			Assert.IsTrue(second.Messages.Any());
			Assert.AreEqual(BOEState.Draft.GetDescription(), first.BoeStatePrevious);
			Assert.AreEqual(BOEState.AwaitingApproval.GetDescription(), fourth.BoeStatePrevious);
			Assert.AreEqual(30, first.TotalRelevantHours);
			Assert.AreEqual(10, first.TotalRelevantHoursPrevious);
			Assert.AreEqual(50, fourth.TotalRelevantHours);
			Assert.AreEqual(25, fourth.TotalRelevantHoursPrevious);
			Assert.AreEqual(BOEState.Draft, boe1.State);
			Assert.AreEqual(BOEState.Draft, boe2.State);
		}

		/// <summary>
		/// Creates a mocked validation factory which returns all valid
		/// </summary>
		/// <returns>Mocked out validation factory</returns>
		public Mock<ValidationFactory> CreateValidationFactoryMock()
        {
            Mock<Validator> validator = new Mock<Validator>();

            validator.Setup(x => x.validation(It.IsAny<object>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>());

            Mock<ValidationFactory> validationFactoryMock = new Mock<ValidationFactory>(validator.Object,
                 validator.Object,
                 validator.Object,
                 validator.Object,
                 validator.Object,
                 validator.Object,
                 validator.Object,
                 validator.Object,
                 validator.Object,
                 validator.Object,
                 validator.Object,
                 validator.Object,
                 validator.Object,
                 validator.Object,
                 validator.Object);

            validationFactoryMock.Setup(x => x.getValidator(ValidationType.WorkspaceUniqueName)).Returns(validator.Object);
            validationFactoryMock.Setup(x => x.getValidator(ValidationType.IsUserNotGroup)).Returns(validator.Object);
            validationFactoryMock.Setup(x => x.getValidator(ValidationType.IsUserNotSubcontractor)).Returns(validator.Object);

            return validationFactoryMock;
        }
    }
}