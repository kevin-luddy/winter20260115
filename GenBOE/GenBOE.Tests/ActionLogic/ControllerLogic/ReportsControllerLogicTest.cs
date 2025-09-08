// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
	using System.Data.Entity.Core.Metadata.Edm;
	using System.IO;
    using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
    using System.Web.Mvc;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenTRAC.DataBridge.DTO;
	using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using UserDTO = GenBOE.Dtos.UserDTO;

    [TestClass]
    public class ReportsControllerLogicTest
    {
        Mock<BOESummary> boeSummary;
        Mock<IBOEExporter> boeExporter = new Mock<IBOEExporter>();
        Mock<IBOECustomExporter> boeCustomExporter = new Mock<IBOECustomExporter>();
        Mock<IWorkspaceExportFormatDTODataLoader> workspaceExportFormatDTOLoader = new Mock<IWorkspaceExportFormatDTODataLoader>();
        Mock<IResourceDTODataLoader> ResourceLoader = new Mock<IResourceDTODataLoader>();
        Mock<IFullObjectFactory> Factory = new Mock<IFullObjectFactory>();
        Mock<IRetriever> _retriever = new Mock<IRetriever>();
        Mock<IPermissionsDTODataLoader> _permissionsDtoDataLoader = new Mock<IPermissionsDTODataLoader>();
        Mock<ICommonDataMapper> _commonDataMapper = new Mock<ICommonDataMapper>();
        Mock<RMSZoneTravelRatesFeesDataLoader> _RMSZoneTravelRatesFeesDataLoader;
        Mock<TravelTripCostCalculation> _TravelTripCostCalculator;
        Mock<BOEDiscrepancyReport> boeDiscrepancyReport = new Mock<BOEDiscrepancyReport>(new Mock<IFullWorkspaceRecalculation>().Object, new Mock<IUserDTODataLoader>().Object);
        Mock<IProposalLoader> proposalLoader = new Mock<IProposalLoader>();
        Mock<IWorkspaceControllerLogic> workspaceControllerLogic = new Mock<IWorkspaceControllerLogic>();
        Mock<IRteTemplateDataLoader> rteTemplateLoader = new Mock<IRteTemplateDataLoader>();
        Mock<TravelTripCostCalculation> travelTripCostCalculation = new Mock<TravelTripCostCalculation>();
		Mock<IUserDTODataLoader> _userDtoDataLoader = new Mock<IUserDTODataLoader>();

		[TestInitialize]
        public void Init()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsDtoDataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IActiveDirectoryUtilities), new ActiveDirectoryUtilities());

            _TravelTripCostCalculator = new Mock<TravelTripCostCalculation>();
            _RMSZoneTravelRatesFeesDataLoader = new Mock<RMSZoneTravelRatesFeesDataLoader>();

            boeSummary = new Mock<BOESummary>(_TravelTripCostCalculator.Object, _RMSZoneTravelRatesFeesDataLoader.Object);
        }

        /// <summary>
        /// Create sut
        /// </summary>
        /// <returns>sut</returns>
        private ReportsControllerLogic CreateSut()
        {
			return new ReportsControllerLogic(this.boeExporter.Object, this.boeSummary.Object,
                this.boeCustomExporter.Object,
                this.workspaceExportFormatDTOLoader.Object, this.boeDiscrepancyReport.Object,
                this.proposalLoader.Object,
                this.workspaceControllerLogic.Object, this.travelTripCostCalculation.Object,
				this._commonDataMapper.Object,
				this._permissionsDtoDataLoader.Object, this._userDtoDataLoader.Object
				);
		}

        #region ExportAllBOEsReport
        /// <summary>
        /// This test case will test ExportAllBOEsReport. It will verify that 
        /// all BOEs will export properly to the word file stream using the
        /// Master template with the boe custom exporter.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public async Task ExportAllBOEsReportTest()
        {
            //Value Declarations
            FullWorkspace workspace = new FullWorkspace() { Id = 1, BOEExportSortByID = 2, TemplateID = 2, ProjectMapType = ProjectMapType.StandardWithoutOffload };
            bool isSubContractorUser = true;
            int? oftid2 = 2;
            List<int> selectBOEs = new List<int> { 1, 3, 4 };
            ICollection<BoeCustomReportComponent> selectedComponents = new Collection<BoeCustomReportComponent>();
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary();
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();

            wsExportFormatDTOCollection = new Collection<WorkspaceExportFormatDTO>(){
                new WorkspaceExportFormatDTO(){ Id = 1, ExportFormat = new ExcelReportTemplate() { TemplateId = 1, ParentTemplateId = 2001 }},
                new WorkspaceExportFormatDTO(){ Id = 2, ExportFormat = new ExcelReportTemplate() { TemplateId = 2, ParentTemplateId = 9001 }},
                new WorkspaceExportFormatDTO(){ Id = 3, ExportFormat = new ExcelReportTemplate() { TemplateId = 3, ParentTemplateId = 2002 }}};
            WorkspaceExportFormatDTO wsExportFormatDTO = wsExportFormatDTOCollection.FirstOrDefault(x => x.ExportFormat.TemplateId == oftid2.Value);
            boes = new Collection<FullBoe>(){
                new FullBoe() { Id = 1, CLINID = 1 },
                new FullBoe() { Id = 2 },
                new FullBoe() { Id = 3, CLINID = 2 },
                new FullBoe() { Id = 4, CLINID = 3 }};

            TaskElementsID = new Collection<BoeTaskElementDTO>(){
                new BoeTaskElementDTO() { Id = 1, taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto(){ ResourceID = 1}, new ResourceTypeDto() }},
                new BoeTaskElementDTO() { Id = 2, taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto(){ ResourceID = 2}, new ResourceTypeDto() }},
                new BoeTaskElementDTO() { Id = 3, taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto()}}};
            dtoID = new Collection<OtherDirectCostDTO>(){
                new OtherDirectCostDTO() { Id = 1, ODCTypes = new Collection<OtherDirectCostType>() { new OtherDirectCostType(){ ResourceID = 3}, new OtherDirectCostType() }},
                new OtherDirectCostDTO() { Id = 2, ODCTypes = new Collection<OtherDirectCostType>() { new OtherDirectCostType(), new OtherDirectCostType(){ ResourceID = 2} }},
                new OtherDirectCostDTO() { Id = 3, ODCTypes = new Collection<OtherDirectCostType>() { new OtherDirectCostType()}}};

            MaterialID = new Collection<MaterialDTO>(){
                new MaterialDTO() { Id = 1},
                new MaterialDTO() { Id = 2},
                new MaterialDTO() { Id = 3} };
            ICollection<int> resourceIds = new Collection<int>() { 1, 0, 2, 3 };

            BOEExportModelView boeModel1 = new BOEExportModelView() { BoeID = 1, PaddedClinName = "X", IsMultiClinWbs = false };
            BOEExportModelView boeModel3 = new BOEExportModelView() { BoeID = 3, PaddedClinName = "MULTI", IsMultiClinWbs = true };
            BOEExportModelView boeModel4 = new BOEExportModelView() { BoeID = 4, PaddedClinName = "A", IsMultiClinWbs = false };
            Collection<BOEExportModelView> boeModelCollection = new Collection<BOEExportModelView>() { boeModel1, boeModel3, boeModel4 };
            Collection<BOEExportModelView> sortedBoeModelCollection = new Collection<BOEExportModelView>() { boeModel4, boeModel1, boeModel3 };

            ICollection<ResourceDTO> resourcesFromDB = new Collection<ResourceDTO>(){
                new ResourceDTO(){ Id = 1 },
                new ResourceDTO(){ Id = 2 }};

            ICollection<BOESummaryGridModelView> boeSummaryGridModelViewsRange1 = new Collection<BOESummaryGridModelView>() { new BOESummaryGridModelView() { BOEID = 1 } };
            ICollection<BOESummaryGridModelView> boeSummaryGridModelViewsRange2 = new Collection<BOESummaryGridModelView>() { new BOESummaryGridModelView() { BOEID = 2 }, new BOESummaryGridModelView() { BOEID = 3 } };
            ICollection<BOESummaryGridModelView> boeSummaryGridModelViewsRange3 = new Collection<BOESummaryGridModelView>() { new BOESummaryGridModelView() { BOEID = 4 } };
            List<BOESummaryGridModelView> listOfBOEs = new List<BOESummaryGridModelView>();
            listOfBOEs.AddRange(boeSummaryGridModelViewsRange1);
            listOfBOEs.AddRange(boeSummaryGridModelViewsRange2);
            listOfBOEs.AddRange(boeSummaryGridModelViewsRange3);

            //Setup
            _retriever.Setup(x => x.GetWorkspaceExportFormatsByWorkspaceId(1)).Returns(wsExportFormatDTOCollection);
            _retriever.Setup(x => x.GetFullBoesByWorkspaceId(1, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(boes);
            _retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(1, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(TaskElementsID);
            _retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(2, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO>());
            _retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(3, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO>());
            _retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(4, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO>());
            _retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(TaskElementsID);
            _retriever.Setup(x => x.GetTravelByWorkspaceId(1, It.IsAny<bool>())).Returns(new Collection<TravelDTO>());
            _retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<CustomFieldDTO>());
            _retriever.Setup(x => x.GetClinsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullClin>());
            _retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullWbs>());
            _retriever.Setup(x => x.GetApprovalUserIdsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<int>());
            _retriever.Setup(x => x.GetUsersByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<UserDTO>());
            _retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<PerformingOrgDTO>());
            _retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>())).Returns(new Collection<PerformingOrgDTO>());
            _retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<WorkspaceVariableDTO>());
            _retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO>());
            _retriever.Setup(x => x.GetSystemLmLaborResources()).Returns(new Collection<ResourceDTO>());
            _retriever.Setup(x => x.GetQuestionsAndAnswersByWorkspaceId(workspace.Id)).Returns(new List<RTECustomTemplateQuestionAnswerModelView>());
            _retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>());

            _retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resourcesFromDB);
            _retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(dtoID);
            _retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(MaterialID);
            _retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(workspace.Id)).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });

            ResourceLoader.Setup(x => x.GetByIds(resourceIds)).Returns(resourcesFromDB);
            boeCustomExporter.Setup(x => x.SetWorkspacePrecisionVariables(workspace));
            httpResponse.Setup(x => x.Cookies).Returns(new HttpCookieCollection());

            boeSummary.Setup(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[0], It.IsAny<BOEExportInputs>(), isSubContractorUser)).Returns(boeSummaryGridModelViewsRange1);
            boeSummary.Setup(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[2], It.IsAny<BOEExportInputs>(), isSubContractorUser)).Returns(boeSummaryGridModelViewsRange2);
            boeSummary.Setup(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[3], It.IsAny<BOEExportInputs>(), isSubContractorUser)).Returns(boeSummaryGridModelViewsRange3);
			workspaceExportFormatDTOLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(wsExportFormatDTO);

			boeCustomExporter.Setup(x => x.ConvertBoeDTOsToExportMVs(It.IsAny<ICollection<FullBoe>>(), It.IsAny<BOEExportInputs>())).Returns(new List<BOEExportModelView> { boeModel1, boeModel3, boeModel4 });
            List<RTECustomTemplateQuestionAnswerModelView> answers = new List<RTECustomTemplateQuestionAnswerModelView>();
            answers.Add(new RTECustomTemplateQuestionAnswerModelView
            {
                AnswerText = "testing answer",
                QuestionText = "question:",
                SourceId = (int)RteTemplateSource.BoeDescription,
                BoeId = 1
            });

            rteTemplateLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>(), It.IsAny<ICollection<FullBoe>>())).Returns(answers);
            ReportsControllerLogic sut = CreateSut();

            bool isCustomExport;
            WorkspaceExportFormatDTO wsExportFormat;
            BOEExportInputs exportInputs;
            ICollection<BOEExportModelView> boeExportModelViews;
            List<BOESummaryGridModelView> boeSummaryGridModelViews;

            //Act
            sut.PrepareAllBOEsReport(workspace, isSubContractorUser, null, selectBOEs, viewDataDictionary, out isCustomExport, out wsExportFormat, out exportInputs,
                out boeExportModelViews, out boeSummaryGridModelViews, true);
            await sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, isCustomExport, wsExportFormat, exportInputs, boeExportModelViews, boeSummaryGridModelViews);
            sut.PrepareAllBOEsReport(workspace, isSubContractorUser, null, selectBOEs, viewDataDictionary, out isCustomExport, out wsExportFormat, out exportInputs,
                out boeExportModelViews, out boeSummaryGridModelViews, false);
            await sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, isCustomExport, wsExportFormat, exportInputs, boeExportModelViews, boeSummaryGridModelViews);
            sut.PrepareAllBOEsReport(workspace, isSubContractorUser, null, selectBOEs, viewDataDictionary, out isCustomExport, out wsExportFormat, out exportInputs,
                out boeExportModelViews, out boeSummaryGridModelViews, true);
            await sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, isCustomExport, wsExportFormat, exportInputs, boeExportModelViews, boeSummaryGridModelViews);

            //Assert
            boeSummary.Verify(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[0], It.IsAny<BOEExportInputs>(), isSubContractorUser), Times.Exactly(3));
            boeSummary.Verify(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[1], It.IsAny<BOEExportInputs>(), isSubContractorUser), Times.Never());
            boeSummary.Verify(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[2], It.IsAny<BOEExportInputs>(), isSubContractorUser), Times.Exactly(3));
            boeCustomExporter.Verify(x => x.ExportBOEToWordFile(It.IsAny<BOEExportInputs>(), boeModelCollection, listOfBOEs, workspace, selectedComponents, httpResponse.Object, string.Format("genBOECustomExport-{0}.docx", workspace.WorkspaceName), wsExportFormatDTO), Times.Exactly(2));
            boeCustomExporter.Verify(x => x.ExportBOEToWordFile(It.IsAny<BOEExportInputs>(), sortedBoeModelCollection, listOfBOEs, workspace, selectedComponents, httpResponse.Object, string.Format("genBOECustomExport-{0}.docx", workspace.WorkspaceName), wsExportFormatDTO), Times.Exactly(1));
        }

        /// <summary>
        /// This test case will test ExportAllBOEsReport. It will verify that 
        /// all BOEs will exports properly to the word file stream using the 
        /// template with the boe exporter. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [TestMethod]
        public async Task ExportAllBOEsReportTestCustomFalse()
        {
            //Value Declarations
            FullWorkspace workspace = new FullWorkspace() { Id = 1, BOEExportSortByID = 1, TemplateID = 2 };
            List<int> selectBOEs = new List<int> { 1, 3 };
            ICollection<BoeCustomReportComponent> selectedComponents = new Collection<BoeCustomReportComponent>();
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary();
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();

            wsExportFormatDTOCollection = new Collection<WorkspaceExportFormatDTO>(){
                new WorkspaceExportFormatDTO("Test"){ Id = 1, ExportFormat = new ExcelReportTemplate() { TemplateId = 1, ParentTemplateId = 9001 }, FileData = new byte[2]},
                new WorkspaceExportFormatDTO("Test"){ Id = 2, ExportFormat = new ExcelReportTemplate() { TemplateId = 2, ParentTemplateId = 2001 }, FileData = new byte[2]},
                new WorkspaceExportFormatDTO("Test"){ Id = 3, ExportFormat = new ExcelReportTemplate() { TemplateId = 3, ParentTemplateId = 7 }, FileData = new byte[2]}};
            WorkspaceExportFormatDTO wsExportFormatDTO = wsExportFormatDTOCollection.FirstOrDefault(x => x.ExportFormat.TemplateId == workspace.TemplateID);
            boes = new Collection<FullBoe>(){
                new FullBoe() { Id = 1 },
                new FullBoe() { Id = 2 },
                new FullBoe() { Id = 3 }};

            TaskElementsID = new Collection<BoeTaskElementDTO>(){
                new BoeTaskElementDTO() { Id = 1, taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto(){ ResourceID = 1}, new ResourceTypeDto() }},
                new BoeTaskElementDTO() { Id = 2, taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto(){ ResourceID = 2}, new ResourceTypeDto() }},
                new BoeTaskElementDTO() { Id = 3, taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto()}}};
            dtoID = new Collection<OtherDirectCostDTO>(){
                new OtherDirectCostDTO() { Id = 1, ODCTypes = new Collection<OtherDirectCostType>() { new OtherDirectCostType(){ ResourceID = 3}, new OtherDirectCostType() }},
                new OtherDirectCostDTO() { Id = 2, ODCTypes = new Collection<OtherDirectCostType>() { new OtherDirectCostType(), new OtherDirectCostType(){ ResourceID = 2} }},
                new OtherDirectCostDTO() { Id = 3, ODCTypes = new Collection<OtherDirectCostType>() { new OtherDirectCostType()}}};

            MaterialID = new Collection<MaterialDTO>(){
                new MaterialDTO() { Id = 1},
                new MaterialDTO() { Id = 2},
                new MaterialDTO() { Id = 3}};
            ICollection<int> resourceIds = new Collection<int>() { 1, 0, 2, 3 };

            BOEExportModelView boeModel1 = new BOEExportModelView() { BoeID = 1 };
            BOEExportModelView boeModel3 = new BOEExportModelView() { BoeID = 3 };
            ICollection<BOEExportModelView> boeModelCollection = new Collection<BOEExportModelView>() { boeModel1, boeModel3 };

            ICollection<ResourceDTO> resourcesFromDB = new Collection<ResourceDTO>(){
                new ResourceDTO(){ Id = 1 },
                new ResourceDTO(){ Id = 2 }};

            ICollection<BOESummaryGridModelView> boeSummaryGridModelViewsRange1 = new Collection<BOESummaryGridModelView>() { new BOESummaryGridModelView() { BOEID = 1 } };
            ICollection<BOESummaryGridModelView> boeSummaryGridModelViewsRange2 = new Collection<BOESummaryGridModelView>() { new BOESummaryGridModelView() { BOEID = 2 }, new BOESummaryGridModelView() { BOEID = 3 } };
            List<BOESummaryGridModelView> listOfBOEs = new List<BOESummaryGridModelView>();
            listOfBOEs.AddRange(boeSummaryGridModelViewsRange1);
            listOfBOEs.AddRange(boeSummaryGridModelViewsRange2);

            //Setup
            _retriever.Setup(x => x.GetWorkspaceExportFormatsByWorkspaceId(1)).Returns(wsExportFormatDTOCollection);
            _retriever.Setup(x => x.GetFullBoesByWorkspaceId(1, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(boes);
            _retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(TaskElementsID);
            _retriever.Setup(x => x.GetQuestionsAndAnswersByWorkspaceId(workspace.Id)).Returns(new List<RTECustomTemplateQuestionAnswerModelView>());
            _retriever.Setup(x => x.GetMoqTypeSelectionsByWorkspaceId(workspace.Id)).Returns(new List<MoqTypeSelection>() { new MoqTypeSelection() });
			_retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new List<TravelDTO>());
            _retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, false)).Returns(dtoID);
            _retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, false)).Returns(MaterialID);
			_retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new List<PerformingOrgDTO>());
			_retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new Collection<FullClin>());
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(workspace.Id)).Returns(new Collection<FullWbs>());
			_retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, true)).Returns(new ReadOnlyCollection<TravelDTO>(new List<TravelDTO>()));
            _retriever.Setup(x => x.GetMaterialsByWorkspaceId(workspace.Id, true)).Returns(new ReadOnlyCollection<MaterialDTO>(new List<MaterialDTO>()));

            this._retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resourcesFromDB);

            ResourceLoader.Setup(x => x.GetByIds(resourceIds)).Returns(resourcesFromDB);
            boeExporter.Setup(x => x.SetWorkspacePrecisionVariables(workspace));
            httpResponse.Setup(x => x.Cookies).Returns(new HttpCookieCollection());

            boeSummary.Setup(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[0], It.IsAny<BOEExportInputs>(), true)).Returns(boeSummaryGridModelViewsRange1);
            boeSummary.Setup(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[2], It.IsAny<BOEExportInputs>(), true)).Returns(boeSummaryGridModelViewsRange2);

            BOEExportInputs exportInputs = new BOEExportInputs(workspace.Boes.ToList(), workspace.Boes.ToList(), workspace.TaskElements.ToList(), workspace);

            boeExporter.Setup(x => x.ConvertBoeDTOsToExportMVs(It.IsAny<BOEExportInputs>())).Returns(new List<BOEExportModelView> { boeModel1, boeModel3 });
            boeExporter.Setup(x => x.ExportBOEToWordFile(exportInputs, boeModelCollection, listOfBOEs, workspace, httpResponse.Object, string.Format("genBOEExport-{0}.docx", workspace.WorkspaceName), wsExportFormatDTO.PhysicalFilePathCache, wsExportFormatDTO.ExportFormat.TemplateType));

			workspaceExportFormatDTOLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(wsExportFormatDTO);

			ReportsControllerLogic sut = CreateSut();

            bool isCustomExport;
            WorkspaceExportFormatDTO wsExportFormat;
            List<BOESummaryGridModelView> boeSummaryGridModelViews;

            //Act
            sut.PrepareAllBOEsReport(workspace, true, null, selectBOEs, viewDataDictionary, out isCustomExport, out wsExportFormat, out exportInputs,
                out boeModelCollection, out boeSummaryGridModelViews, false);
            await sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, isCustomExport, wsExportFormat, exportInputs, boeModelCollection, boeSummaryGridModelViews);
            sut.PrepareAllBOEsReport(workspace, true, null, selectBOEs, viewDataDictionary, out isCustomExport, out wsExportFormat, out exportInputs,
                out boeModelCollection, out boeSummaryGridModelViews, false);
            await sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, isCustomExport, wsExportFormat, exportInputs, boeModelCollection, boeSummaryGridModelViews);

            //Assert
            boeExporter.Verify(x => x.ExportBOEToWordFile(exportInputs, boeModelCollection, listOfBOEs, workspace, httpResponse.Object, string.Format("genBOEExport-{0}.docx", workspace.WorkspaceName), wsExportFormatDTO.PhysicalFilePathCache, wsExportFormatDTO.ExportFormat.TemplateType), Times.Once());
        }

        /// <summary>
        /// This will test the SummarizeByCustomFieldOptions method.
        /// </summary>
        [TestMethod]
        public void Test_SummarizeByCustomFieldOptions()
        {
            ReportsControllerLogic sut = CreateSut();
            ICollection<SelectListItem> result = sut.SummarizeByCustomFieldOptions(null);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());

            IReadOnlyCollection<CustomFieldDTO> customFields = new Collection<CustomFieldDTO>();
            result = sut.SummarizeByCustomFieldOptions(customFields);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());

            customFields = new Collection<CustomFieldDTO>() {
                new CustomFieldDTO { Id=1, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldName = "BoeField" },
                new CustomFieldDTO { Id=2, CustomFieldDisplayID = CustomFieldType.TaskDisplay, CustomFieldName = "TaskField" } };
            result = sut.SummarizeByCustomFieldOptions(customFields);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());

            customFields = new Collection<CustomFieldDTO>() {
                new CustomFieldDTO { Id=1, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldName = "BoeField" },
                new CustomFieldDTO { Id=2, CustomFieldDisplayID = CustomFieldType.TaskDisplay, CustomFieldName = "TaskField" },
                new CustomFieldDTO { Id=3, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldName = "LaborTypeField1" },
                new CustomFieldDTO { Id=4, CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay, CustomFieldName = "LaborTypeField2" } };
            result = sut.SummarizeByCustomFieldOptions(customFields);
            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(1, result.Where(x => x.Text.Equals("None") && x.Value.Equals("None")).Count());
            Assert.AreEqual(1, result.Where(x => x.Text.Equals("LaborTypeField1") && x.Value.Equals("LaborTypeField1")).Count());
            Assert.AreEqual(1, result.Where(x => x.Text.Equals("LaborTypeField2") && x.Value.Equals("LaborTypeField2")).Count());
        }

        /// <summary>
        /// This test case will test ExportAllBOEsReport. It will verify that 
        /// this function will properly throw a ArgumentNullException when the
        /// FullWorkspace is passed through as Null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PrepareAllBOEsReportTestWorkspaceNull()
        {
            //Value Declarations
            bool isSubContractorUser = true;
            List<int> selectBOEs = new List<int>();
            ICollection<BoeCustomReportComponent> selectedComponents = new Collection<BoeCustomReportComponent>();
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary();

            ReportsControllerLogic sut = CreateSut();

            bool isCustomExport;
            WorkspaceExportFormatDTO wsExportFormat;
            BOEExportInputs exportInputs;
            ICollection<BOEExportModelView> boeExportModelViews;
            List<BOESummaryGridModelView> boeSummaryGridModelViews;

            //Act
            sut.PrepareAllBOEsReport(null, isSubContractorUser, null, selectBOEs, viewDataDictionary, out isCustomExport, out wsExportFormat, out exportInputs, out boeExportModelViews, out boeSummaryGridModelViews);
        }

        /// <summary>
        /// This test case will test ExportAllBOEsReport. It will verify that 
        /// this function will properly throw a ArgumentNullException when the
        /// FullWorkspace is passed through as Null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ExportAllBOEsReportTestWorkspaceNull()
        {
            //Value Declarations
            FullWorkspace workspace = null;
            List<int> selectBOEs = new List<int>();
            ICollection<BoeCustomReportComponent> selectedComponents = new Collection<BoeCustomReportComponent>();
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary();
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();
            
            ReportsControllerLogic sut = CreateSut();

            //Act
            await sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, false, null, null, null, null);
        }

        /// <summary>
        /// This test case will test ExportAllBOEsReport. It will verify that
        /// this function will properly throw a ArgumentNullException when the
        /// HttpResponseBase is passed through as Null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ExportAllBOEsReportTesthttpResponseNull()
        {
            //Value Declarations
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };
            ICollection<BoeCustomReportComponent> selectedComponents = new Collection<BoeCustomReportComponent>();
            HttpResponseBase httpResponse = null;

            ReportsControllerLogic sut = CreateSut();

            //Act
            await sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse, true, null, null, null, null);
        }

        /// <summary>
        /// This test case will test ExportAllBOEsReport. It will verify that 
        /// this function will properly throw a ArgumentNullException when 
        /// wsExportFormatDTO is passed through as Null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ExportAllBOEsReportTestWsExportFormatDTONull()
        {
            //Value Declarations
            FullWorkspace workspace = new FullWorkspace();
            List<int> selectBOEs = new List<int>();
            ICollection<BoeCustomReportComponent> selectedComponents = new Collection<BoeCustomReportComponent>();
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary();
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();

            ReportsControllerLogic sut = CreateSut();

            //Act
            await sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, false, null, null, null, null);
        }
        #endregion

        #region Project Map Reports

        /// <summary>
        /// Test that GetSummaryReportsModelViews returns null for SSC/default
        /// </summary>
        [TestMethod]
        public void GetSummaryReportsModelViewsTest()
        {
            ReportsControllerLogic sut = this.CreateSut();
            ICollection<SSRSReportsModelView> result = sut.GetSummaryReportsModelViews();
            Assert.IsNull(result);
        }

        /// <summary>
        /// Test that GetCustomerReportsModelViews returns null for SSC/default
        /// </summary>
        [TestMethod]
        public void GetCustomerReportsModelViewsTest()
        {
            ReportsControllerLogic sut = this.CreateSut();
            ICollection<SSRSReportsModelView> result = sut.GetCustomerReportsModelViews();
            Assert.IsNull(result);
        }

        /// <summary>
        /// Test that GetFinanceReportsModelViews returns null for SSC/default
        /// </summary>
        [TestMethod]
        public void GetFinanceReportsModelViewsTest()
        {
            ReportsControllerLogic sut = this.CreateSut();
            ICollection<SSRSReportsModelView> result = sut.GetFinanceReportsModelViews();
            Assert.IsNull(result);
        }

        /// <summary>
        /// Test that GetAdditionalReportsModelViews returns null for SSC/default
        /// </summary>
        [TestMethod]
        public void GetAdditionalReportsModelViewsTest()
        {
            ReportsControllerLogic sut = this.CreateSut();
            ICollection<SSRSReportsModelView> result = sut.GetAdditionalReportsModelViews();
            Assert.IsNull(result);
        }

        #endregion

        #region PTM Out of Sync
        
        /// <summary>
        /// Test GetPtmDataOutOfSyncMessages with all fields up to date
        /// </summary>
        [TestMethod]
        public void TestGetPtmDataOutOfSyncMessages_UpToDate()
        {
            DateTime deliveryDate = DateTime.Now;
            FullWorkspace workspace = new FullWorkspace()
            {
                Id = 1,
                TrackingNumber = "1",
                LineOfBusiness = new IES.Common.PickList.PickListDto() { Id = 1 },
                ProposalClass = new IES.Common.PickList.PickListDto { Id = 1, Text = "Firm" },
                SelectedContractTypes =
                    new Collection<int>() { 1001, 1002, 1003 },
                ProposalSubmittalDate = deliveryDate,
                RFPNumber = "1",
                ProposalTitle = "Test"
            };
            int proposalID = 1;
            ProposalDto proposal = new ProposalDto()
            {
                LineOfBusinessID = 1,
                ProposalClass = 1,
                ContractTypeIds = new List<int>() { 1, 2, 3 },
                DeliveryDate = deliveryDate,
                RFPNumber = "1",
                ProposalTitle = "Test"
            };

            this.proposalLoader.Setup(x => x.GetIdByTrackingNumber(It.IsAny<string>())).Returns(proposalID);
            this.proposalLoader.Setup(x => x.GetById(proposalID)).Returns(proposal);
            this.workspaceControllerLogic.Setup(x => x.ConvertPTMLineOfBusiness(It.IsAny<int>())).Returns(1);
            this.workspaceControllerLogic.Setup(x => x.ConvertPTMProposalClassId(It.IsAny<int>())).Returns(1);
            this.workspaceControllerLogic.Setup(x => x.ConvertPTMContractTypeId(1)).Returns(1001);
            this.workspaceControllerLogic.Setup(x => x.ConvertPTMContractTypeId(2)).Returns(1002);
            this.workspaceControllerLogic.Setup(x => x.ConvertPTMContractTypeId(3)).Returns(1003);

            ReportsControllerLogic sut = this.CreateSut();
            ICollection<string> result = sut.GetPtmDataOutOfSyncMessages(workspace);

            // Assert no messages are returned
            Assert.IsFalse(result.Any());
        }

        /// <summary>
        /// Test GetPtmDataOutOfSyncMessages for PTM Integration not being on
        /// </summary>
        [TestMethod]
        public void TestGetPtmDataOutOfSyncMessages_NotIntegrated()
        {
            try
            {
                FullWorkspace workspace = new FullWorkspace() { Id = 1 };

                SystemConfiguration.Instance().CompanyConfigurationSettings.AppSettings["IsPTMIntegrated"] = "false";

                ReportsControllerLogic sut = this.CreateSut();
                ICollection<string> result = sut.GetPtmDataOutOfSyncMessages(workspace);

                // Assert no messages are returned
                Assert.IsFalse(result.Any());
            }
            finally
            {
                SystemConfiguration.Instance().CompanyConfigurationSettings.AppSettings["IsPTMIntegrated"] = "true";
            }
        }

        /// <summary>
        /// Test GetPtmDataOutOfSyncMessages for there being no linked proposal
        /// </summary>
        [TestMethod]
        public void TestGetPtmDataOutOfSyncMessages_NoProposal()
        {
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };

            ReportsControllerLogic sut = this.CreateSut();
            ICollection<string> result = sut.GetPtmDataOutOfSyncMessages(workspace);

            // Assert no messages are returned
            Assert.IsFalse(result.Any());
        }

        /// <summary>
        /// Test GetPtmDataOutOfSyncMessages for proposalLoader.GetById returning null
        /// </summary>
        [TestMethod]
        public void TestGetPtmDataOutOfSyncMessages_InvalidProposalId()
        {
            FullWorkspace workspace = new FullWorkspace() { Id = 1, TrackingNumber = "1" };

            int proposalID = 1;
            this.proposalLoader.Setup(x => x.GetIdByTrackingNumber(It.IsAny<string>())).Returns(proposalID);
            this.proposalLoader.Setup(x => x.GetById(proposalID)).Returns((ProposalDto)null);

            ReportsControllerLogic sut = this.CreateSut();
            ICollection<string> result = sut.GetPtmDataOutOfSyncMessages(workspace);

            // Assert no messages are returned
            Assert.IsFalse(result.Any());
        }

        /// <summary>
        /// Test GetPtmDataOutOfSyncMessages for Proposal Fields being out of date
        /// </summary>
        [TestMethod]
        public void TestGetPtmDataOutOfSyncMessages_OutOfDateFields()
        {
            DateTime deliveryDate = DateTime.Now;
            FullWorkspace workspace = new FullWorkspace()
            {
                Id = 1,
                TrackingNumber = "1",
                LineOfBusiness = new IES.Common.PickList.PickListDto() { Id = 1 },
                ProposalClass = new IES.Common.PickList.PickListDto { Id = 1, Text = "Firm" },
                SelectedContractTypes =
                    new Collection<int>() { 1001, 1002, 1003 },
                ProposalSubmittalDate = deliveryDate,
                RFPNumber = "1",
                ProposalTitle = "Test",
                RevisedSubmittalDate = DateTime.Now
            };
            int proposalID = 1;
            ProposalDto proposal = new ProposalDto()
            {
                LineOfBusinessID = 1,
                ProposalClass = 1,
                ContractTypeIds = new List<int>() { 1, 2, 3 },
                DeliveryDate = deliveryDate.AddDays(1),
                RFPNumber = "2",
                ProposalTitle = "Test2",
                RevisedSubmittalDate = DateTime.Now.AddDays(-1)
            };

            this.proposalLoader.Setup(x => x.GetIdByTrackingNumber(It.IsAny<string>())).Returns(proposalID);
            this.proposalLoader.Setup(x => x.GetById(proposalID)).Returns(proposal);
            this.workspaceControllerLogic.Setup(x => x.ConvertPTMLineOfBusiness(It.IsAny<int>())).Returns(2);
            this.workspaceControllerLogic.Setup(x => x.ConvertPTMProposalClassId(It.IsAny<int>())).Returns(2);
            this.workspaceControllerLogic.Setup(x => x.ConvertPTMContractTypeId(1)).Returns(1);
            this.workspaceControllerLogic.Setup(x => x.ConvertPTMContractTypeId(2)).Returns(1002);
            this.workspaceControllerLogic.Setup(x => x.ConvertPTMContractTypeId(3)).Returns(3);

            ReportsControllerLogic sut = this.CreateSut();
            ICollection<string> result = sut.GetPtmDataOutOfSyncMessages(workspace);

            // Assert the expected messages are returned
            Assert.AreEqual(7, result.Count());
            Assert.AreEqual("Line of Business", result.ElementAt(0));
            Assert.AreEqual("Proposal Class", result.ElementAt(1));
            Assert.AreEqual("Contract Types", result.ElementAt(2));
            Assert.AreEqual("Anticipated Delivery Date", result.ElementAt(3));
            Assert.AreEqual("Revised Anticipated Delivery Date", result.ElementAt(4));
            Assert.AreEqual("RFP Number", result.ElementAt(5));
            Assert.AreEqual("PTM Proposal Title", result.ElementAt(6));
        }

        /// <summary>
        /// Test GetPtmDataOutOfSyncMessages to make sure it throws the expected exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetPtmDataOutOfSyncMessages_EX()
        {
            ReportsControllerLogic sut = this.CreateSut();
            ICollection<string> result = sut.GetPtmDataOutOfSyncMessages(null);
        }

        #endregion

        #region WBS/BOE Report

        /// <summary>
        /// Test GetExportInputsForStatusAndWbsReports
        /// </summary>
        [TestMethod]
        public void TestGetExportInputsForStatusAndWbsReports()
        {
            ReportsControllerLogic sut = CreateSut();

            FullWorkspace workspace = new FullWorkspace() { Id = 1, BOEExportSortByID = 2, TemplateID = 2, ProjectMapType = ProjectMapType.StandardWithoutOffload };
            boes = new Collection<FullBoe>(){
                new FullBoe() { Id = 1 },
                new FullBoe() { Id = 2 },
                new FullBoe() { Id = 3 },
                new FullBoe() { Id = 4 }};
            ICollection<int> BoeIds = boes.Select(x => x.Id).ToCollection();
            TaskElementsID = new Collection<BoeTaskElementDTO>(){
                new BoeTaskElementDTO() { Id = 1, taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto(){ ResourceID = 1}, new ResourceTypeDto() }},
                new BoeTaskElementDTO() { Id = 2, taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto(){ ResourceID = 2}, new ResourceTypeDto() }},
                new BoeTaskElementDTO() { Id = 3, taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto()}}};
            dtoID = new Collection<OtherDirectCostDTO>(){
                new OtherDirectCostDTO() { Id = 1, ODCTypes = new Collection<OtherDirectCostType>() { new OtherDirectCostType(){ ResourceID = 3}, new OtherDirectCostType() }},
                new OtherDirectCostDTO() { Id = 2, ODCTypes = new Collection<OtherDirectCostType>() { new OtherDirectCostType(), new OtherDirectCostType(){ ResourceID = 2} }},
                new OtherDirectCostDTO() { Id = 3, ODCTypes = new Collection<OtherDirectCostType>() { new OtherDirectCostType()}}};
            ICollection<ResourceDTO> resourcesFromDB = new Collection<ResourceDTO>(){
                new ResourceDTO(){ Id = 1 },
                new ResourceDTO(){ Id = 2 }};

            _retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(boes);
            _retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(TaskElementsID);
            _retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, false)).Returns(dtoID);
            this._retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resourcesFromDB);
			_retriever.Setup(x => x.GetTravelByWorkspaceId(1, It.IsAny<bool>())).Returns(new Collection<TravelDTO>());
			_retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<PerformingOrgDTO>());
			_retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new Collection<FullClin>());
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(workspace.Id)).Returns(new Collection<FullWbs>());

			BOEExportInputs result = sut.GetExportInputsForStatusAndWbsReports(workspace);

            Assert.IsNotNull(result);
            Assert.AreEqual(workspace.Id, result.Workspace.Id);
            Assert.IsTrue(result.Boes.Any(x => x.Id == BoeIds.First()));
            Assert.IsTrue(result.TaskElements.Any(x => x.Id == TaskElementsID.First().Id));
            Assert.IsTrue(result.Odcs.Any(x => x.Id == dtoID.First().Id));
        }

        /// <summary>
        /// Test GenerateWbsBoeReport
        /// </summary>
        [TestMethod]
        public void TestGenerateWbsBoeReport()
        {
            ReportsControllerLogic sut = CreateSut();

            FullWorkspace workspace = new FullWorkspace() { Id = 1, BOEExportSortByID = 2, TemplateID = 2, ProjectMapType = ProjectMapType.StandardWithoutOffload };
            boes = new Collection<FullBoe>(){
                new FullBoe() { Id = 1, WBSID = 1, Title = "BOE1" },
                new FullBoe() { Id = 2, Title = "BOE2" },
                new FullBoe() { Id = 3, WBSID = 2, Title = "BOE3" },
                new FullBoe() { Id = 4, WBSID = 3, Title = "BOE4" }};

            TaskElementsID = new Collection<BoeTaskElementDTO>(){
                new BoeTaskElementDTO() { Id = 1, BoeID = 1, taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto(){ ResourceID = 1, ValueSpread = 10, SpreadType = SpreadType.Hours}, new ResourceTypeDto() { ResourceID = 1, ValueSpread = 20, SpreadType = SpreadType.Hours } } },
                new BoeTaskElementDTO() { Id = 2, BoeID = 2, taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto(){ ResourceID = 2, ValueSpread = 10, SpreadType = SpreadType.Cost }, new ResourceTypeDto(){ ResourceID = 2, ValueSpread = 20, SpreadType = SpreadType.Cost } }},
                new BoeTaskElementDTO() { Id = 3, BoeID = 3, taskElementLabors = new Collection<ResourceTypeDto>() { new ResourceTypeDto(){ ResourceID = 1, ValueSpread = 10, SpreadType = SpreadType.Hours}, new ResourceTypeDto() { ResourceID = 2, ValueSpread = 10, SpreadType = SpreadType.Cost }}}};
            dtoID = new Collection<OtherDirectCostDTO>(){
                new OtherDirectCostDTO() { Id = 1, ODCTypes = new Collection<OtherDirectCostType>() { new OtherDirectCostType(){ ResourceID = 3}, new OtherDirectCostType() }},
                new OtherDirectCostDTO() { Id = 2, ODCTypes = new Collection<OtherDirectCostType>() { new OtherDirectCostType(), new OtherDirectCostType(){ ResourceID = 2} }},
                new OtherDirectCostDTO() { Id = 3, ODCTypes = new Collection<OtherDirectCostType>() { new OtherDirectCostType()}}};
            ICollection<ResourceDTO> resourcesFromDB = new Collection<ResourceDTO>(){
                new ResourceDTO(){ Id = 1 },
                new ResourceDTO(){ Id = 2 }};
            ICollection<FullWbs> wbs = new Collection<FullWbs>()
            {
                new FullWbs() { Id = 1, WbsNumber = "1" },
                new FullWbs() { Id = 2, WbsNumber = "1.1" },
                new FullWbs() { Id = 3, WbsNumber = "2" }
            };

            _retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(boes);
            _retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(TaskElementsID);
            _retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, false)).Returns(dtoID);
            this._retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resourcesFromDB);
            _retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, It.IsAny<bool>())).Returns(new Collection<TravelDTO>());
            _retriever.Setup(x => x.GetEscalationRatesByWorkspace(workspace)).Returns(new Collection<EscalationRatesDTO>());
            _retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(workspace.Id)).Returns(wbs);
			_retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<PerformingOrgDTO>());
			_retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new Collection<FullClin>());

			BOEExportInputs inputs = new BOEExportInputs(boes, boes, TaskElementsID, workspace, null);

            ICollection<BoeWbsReportModelView> result = sut.GenerateWbsBoeReport(inputs);

            Assert.IsTrue(result.Any());
            Assert.AreEqual(boes.Count + 1, result.Count); // +1 for totals row

            // Assert each row for each boe
            foreach(FullBoe boe in boes)
            {
                BoeWbsReportModelView row = result.FirstOrDefault(x => x.BOETitle == boe.Title);
                string expectedWbsNumber = boe.WBSID != null ? wbs.First(x => x.Id == boe.WBSID).WbsNumber : string.Empty;
                BoeTaskElementDTO task = TaskElementsID.FirstOrDefault(x => x.BoeID == boe.Id);
                decimal? expectedTotalHours = task != null ? task.taskElementLabors.Where(x => x.SpreadType == SpreadType.Hours).Sum(x => x.ValueSpread) : 0;
                decimal? expectedTotalCost = task != null ? task.taskElementLabors.Where(x => x.SpreadType == SpreadType.Cost).Sum(x => x.ValueSpread) : 0;

                Assert.IsNotNull(row);
                Assert.AreEqual(expectedWbsNumber, row.WBSNumber);
                Assert.AreEqual(expectedTotalHours, row.TotalHours);
                Assert.AreEqual(expectedTotalCost, row.TotalCost);
            }
            
            // Assert totals row
            Assert.AreEqual(CommonConstants.SET_AS_BOLD_FOR_EXCEL + "Totals", result.Last().BOETitle);
            Assert.AreEqual(string.Empty, result.Last().WBSNumber);
            Assert.AreEqual(40, result.Last().TotalHours);
            Assert.AreEqual(40, result.Last().TotalCost);
        }

        /// <summary>
        /// Test ExportWbsBoeReport
        /// </summary>
        [TestMethod]
        public void TestExportWbsBoeReport()
        {
            ReportsControllerLogic sut = CreateSut();

            FullWorkspace workspace = new FullWorkspace() { Id = 1 };
            boes = new Collection<FullBoe>();
            ICollection<int> BoeIds = new Collection<int>();
            TaskElementsID = new Collection<BoeTaskElementDTO>();
            dtoID = new Collection<OtherDirectCostDTO>();
            ICollection<ResourceDTO> resourcesFromDB = new Collection<ResourceDTO>();

            _retriever.Setup(x => x.GetFullBoesByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(boes);
            _retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(workspace.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(TaskElementsID);
            _retriever.Setup(x => x.GetOdcCollectionByWorkspaceId(workspace.Id, false)).Returns(dtoID);
            this._retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resourcesFromDB);
			_retriever.Setup(x => x.GetTravelByWorkspaceId(1, It.IsAny<bool>())).Returns(new Collection<TravelDTO>());
			_retriever.Setup(x => x.GetPerformingOrgsByIds(It.IsAny<ICollection<int>>())).Returns(new Collection<PerformingOrgDTO>());
			_retriever.Setup(x => x.GetClinsByWorkspaceId(workspace.Id)).Returns(new Collection<FullClin>());
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(workspace.Id)).Returns(new Collection<FullWbs>());

			BOEExportInputs inputs = new BOEExportInputs(boes, boes, TaskElementsID, workspace, null);
            ICollection<BoeWbsReportModelView> reportModelView = new Collection<BoeWbsReportModelView>()
            {
                new BoeWbsReportModelView()
                {
                    BOETitle = "Test",
                    WBSNumber = "1",
                    TotalHours = 1000,
                    TotalCost = 2000
                }
            };

            string fileLocation = Path.Combine(System.Environment.CurrentDirectory, Path.GetRandomFileName() + ".xlsx");
            File.WriteAllBytes(fileLocation, Properties.Resources.WbsBoeReport);

            string result = sut.ExportWbsBoeReport(workspace, fileLocation, reportModelView, inputs);

            // Wrapping in try/finally to ensure file is deleted even if test fails
            try
            {
                // Assert file was created
                Assert.IsFalse(string.IsNullOrEmpty(result));
                Assert.IsTrue(File.Exists(result));

                ICollection<string> columns = new Collection<string>() { ImportExportConstants.WBS_NUMBER_COLUMN_HEADER, ImportExportConstants.BOE_TITLE_COLUMN_HEADER,
                "Total Hours", ImportExportConstants.TOTAL_COST_COLUMN_HEADER };
                string hoursFormatString = Utilities.PrecisionFormattingStringNoComma(workspace.DecimalPrecision);
                string costFormatString = Utilities.CostPrecisionFormattingString(workspace.CostDecimalPrecision).Replace(",", "");

                using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(result, false))
                {
                    ICollection<Dictionary<string, string>> rows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(spreadsheet, string.Empty, columns.ToArray(), columns.ToArray()).ToCollection();

                    // Assert that there is only one row that matches the modelview
                    Assert.IsTrue(rows.Count == 1);

                    BoeWbsReportModelView expected = reportModelView.First();
                    Dictionary<string, string> actual = rows.First();

                    // Assert column values for the row
                    Assert.AreEqual(expected.BOETitle, actual[ImportExportConstants.BOE_TITLE_COLUMN_HEADER]);
                    Assert.AreEqual(expected.WBSNumber, actual[ImportExportConstants.WBS_NUMBER_COLUMN_HEADER]);
                    Assert.AreEqual(expected.TotalHours.ToString(hoursFormatString), actual["Total Hours"]);
                    Assert.AreEqual(expected.TotalCost.ToString(costFormatString), actual[ImportExportConstants.TOTAL_COST_COLUMN_HEADER]);
                }
            }
            finally
            {
                // Delete the file now that testing is done
                File.Delete(result);
            }
        }

        #endregion

        //Global local Value Declarations 
        private ICollection<FullBoe> boes;
        private ICollection<WorkspaceExportFormatDTO> wsExportFormatDTOCollection;
        private ICollection<BoeTaskElementDTO> TaskElementsID;
        private ICollection<OtherDirectCostDTO> dtoID;
        private ICollection<MaterialDTO> MaterialID;
    }
}
