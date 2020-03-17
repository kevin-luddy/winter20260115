// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
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
        Mock<IPermissionsDTODataLoader> _perissionsDtoDataLoader = new Mock<IPermissionsDTODataLoader>();
        Mock<ICommonDataMapper> _commonDataMapper = new Mock<ICommonDataMapper>();
        Mock<RMSZoneTravelRatesFeesDataLoader> _RMSZoneTravelRatesFeesDataLoader;
        Mock<TravelTripCostCalculation> _TravelTripCostCalculator;
        Mock<BOEDiscrepancyReport> boeDiscrepancyReport = new Mock<BOEDiscrepancyReport>(new Mock<IFullWorkspaceRecalculation>().Object, new Mock<IUserDTODataLoader>().Object);
        Mock<IProposalLoader> proposalLoader = new Mock<IProposalLoader>();
        Mock<IWorkspaceControllerLogic> workspaceControllerLogic = new Mock<IWorkspaceControllerLogic>();
        Mock<IRteTemplateDataLoader> rteTemplateLoader = new Mock<IRteTemplateDataLoader>();
        
        [TestInitialize]
        public void Init()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _perissionsDtoDataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);

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
                this.workspaceControllerLogic.Object, rteTemplateLoader.Object);
        }

        #region ExportAllBOEsReport
        /// <summary>
        /// This test case will test ExportAllBOEsReport. It will verify that 
        /// all BOEs will export properly to the word file stream using the
        /// Master template with the boe custom exporter.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
        public void ExportAllBOEsReportTest()
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
            _retriever.Setup(x => x.GetFullBoesByWorkspaceId(1)).Returns(boes);
            _retriever.Setup(x => x.GetFullBoesByWorkspaceId(1, It.IsAny<bool>())).Returns(boes);
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
            
            _retriever.Setup(x => x.GetCustomFieldValuesByFieldIds(It.IsAny<ICollection<int>>(), It.IsAny<int>())).Returns(new Collection<CustomFieldValueDTO>());

            this._retriever.Setup(x => x.GetMaterialsByBoeIds(It.IsAny<Collection<int>>(), It.IsAny<bool>())).Returns(MaterialID.ToList());
            this._retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resourcesFromDB);

            Collection<int> BoeIds = boes.Select(x => x.Id).ToCollection();
            _retriever.Setup(x => x.GetOdcCollectionByBoeIds(BoeIds, It.IsAny<bool>())).Returns(dtoID);
            _retriever.Setup(x => x.GetMaterialsByBoeIds(BoeIds, It.IsAny<bool>())).Returns(MaterialID);

            ResourceLoader.Setup(x => x.GetByIds(resourceIds)).Returns(resourcesFromDB);
            boeCustomExporter.Setup(x => x.SetWorkspacePrecisionVariables(workspace));
            httpResponse.Setup(x => x.Cookies).Returns(new HttpCookieCollection());

            boeSummary.Setup(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[0], It.IsAny<BOEExportInputs>(), isSubContractorUser)).Returns(boeSummaryGridModelViewsRange1);
            boeSummary.Setup(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[2], It.IsAny<BOEExportInputs>(), isSubContractorUser)).Returns(boeSummaryGridModelViewsRange2);
            boeSummary.Setup(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[3], It.IsAny<BOEExportInputs>(), isSubContractorUser)).Returns(boeSummaryGridModelViewsRange3);
            workspaceExportFormatDTOLoader.Setup(x => x.GetById((int)ExcelReportTemplateType.MASTER)).Returns(wsExportFormatDTO);

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
            sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, true, isCustomExport, wsExportFormat, exportInputs, boeExportModelViews, boeSummaryGridModelViews);
            sut.PrepareAllBOEsReport(workspace, isSubContractorUser, null, selectBOEs, viewDataDictionary, out isCustomExport, out wsExportFormat, out exportInputs,
                out boeExportModelViews, out boeSummaryGridModelViews, false);
            sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, true, isCustomExport, wsExportFormat, exportInputs, boeExportModelViews, boeSummaryGridModelViews);
            sut.PrepareAllBOEsReport(workspace, isSubContractorUser, null, selectBOEs, viewDataDictionary, out isCustomExport, out wsExportFormat, out exportInputs,
                out boeExportModelViews, out boeSummaryGridModelViews, true);
            sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, true, isCustomExport, wsExportFormat, exportInputs, boeExportModelViews, boeSummaryGridModelViews);

            //Assert
            rteTemplateLoader.Verify(x => x.GetByWorkspaceId(It.IsAny<int>(), It.IsAny<ICollection<FullBoe>>()), Times.Exactly(3));
            boeSummary.Verify(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[0], It.IsAny<BOEExportInputs>(), isSubContractorUser), Times.Exactly(3));
            boeSummary.Verify(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[1], It.IsAny<BOEExportInputs>(), isSubContractorUser), Times.Never());
            boeSummary.Verify(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[2], It.IsAny<BOEExportInputs>(), isSubContractorUser), Times.Exactly(3));
            boeCustomExporter.Verify(x => x.ExportBOEToWordFile(It.IsAny<BOEExportInputs>(), boeModelCollection, listOfBOEs, selectedComponents, httpResponse.Object, string.Format("genBOECustomExport-{0}.docx", workspace.WorkspaceName), wsExportFormatDTO), Times.Exactly(2));
            boeCustomExporter.Verify(x => x.ExportBOEToWordFile(It.IsAny<BOEExportInputs>(), sortedBoeModelCollection, listOfBOEs, selectedComponents, httpResponse.Object, string.Format("genBOECustomExport-{0}.docx", workspace.WorkspaceName), wsExportFormatDTO), Times.Exactly(1));
        }

        /// <summary>
        /// This test case will test ExportAllBOEsReport. It will verify that 
        /// all BOEs will exports properly to the word file stream using the 
        /// template with the boe exporter. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [TestMethod]
        public void ExportAllBOEsReportTestCustomFalse()
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
            _retriever.Setup(x => x.GetFullBoesByWorkspaceId(1)).Returns(boes);
            _retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(TaskElementsID);

            Collection<int> BoeIds = boes.Select(x => x.Id).ToCollection();
            _retriever.Setup(x => x.GetOdcCollectionByBoeIds(BoeIds, false)).Returns(dtoID);
            _retriever.Setup(x => x.GetMaterialsByBoeIds(BoeIds, false)).Returns(MaterialID);

            _retriever.Setup(x => x.GetTravelByWorkspaceId(workspace.Id, true)).Returns(new ReadOnlyCollection<TravelDTO>(new List<TravelDTO>()));
            _retriever.Setup(x => x.GetMaterialsByBoeIds(BoeIds, true)).Returns(new ReadOnlyCollection<MaterialDTO>(new List<MaterialDTO>()));

            this._retriever.Setup(x => x.GetResourcesByIds(It.IsAny<ICollection<int>>())).Returns(resourcesFromDB);

            ResourceLoader.Setup(x => x.GetByIds(resourceIds)).Returns(resourcesFromDB);
            boeExporter.Setup(x => x.SetWorkspacePrecisionVariables(workspace));
            httpResponse.Setup(x => x.Cookies).Returns(new HttpCookieCollection());

            boeSummary.Setup(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[0], It.IsAny<BOEExportInputs>(), true)).Returns(boeSummaryGridModelViewsRange1);
            boeSummary.Setup(x => x.GetBOESummaryGridModelViews(boes.ToCollection()[2], It.IsAny<BOEExportInputs>(), true)).Returns(boeSummaryGridModelViewsRange2);

            BOEExportInputs exportInputs = new BOEExportInputs(workspace.Boes.ToList(), workspace.Boes.ToList(), workspace.TaskElements.ToList(), workspace);

            boeExporter.Setup(x => x.ConvertBoeDTOsToExportMVs(It.IsAny<BOEExportInputs>())).Returns(new List<BOEExportModelView> { boeModel1, boeModel3 });
            boeExporter.Setup(x => x.ExportBOEToWordFile(exportInputs, boeModelCollection, listOfBOEs, httpResponse.Object, string.Format("genBOEExport-{0}.docx", workspace.WorkspaceName), wsExportFormatDTO.PhysicalFilePathCache, wsExportFormatDTO.ExportFormat.TemplateType));

            ReportsControllerLogic sut = CreateSut();

            bool isCustomExport;
            WorkspaceExportFormatDTO wsExportFormat;
            List<BOESummaryGridModelView> boeSummaryGridModelViews;

            //Act
            sut.PrepareAllBOEsReport(workspace, true, null, selectBOEs, viewDataDictionary, out isCustomExport, out wsExportFormat, out exportInputs,
                out boeModelCollection, out boeSummaryGridModelViews, false);
            sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, true, isCustomExport, wsExportFormat, exportInputs, boeModelCollection, boeSummaryGridModelViews);
            sut.PrepareAllBOEsReport(workspace, true, null, selectBOEs, viewDataDictionary, out isCustomExport, out wsExportFormat, out exportInputs,
                out boeModelCollection, out boeSummaryGridModelViews, false);
            sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, true, isCustomExport, wsExportFormat, exportInputs, boeModelCollection, boeSummaryGridModelViews);

            //Assert
            boeExporter.Verify(x => x.ExportBOEToWordFile(exportInputs, boeModelCollection, listOfBOEs, httpResponse.Object, string.Format("genBOEExport-{0}.docx", workspace.WorkspaceName), wsExportFormatDTO.PhysicalFilePathCache, wsExportFormatDTO.ExportFormat.TemplateType), Times.Once());
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
        public void ExportAllBOEsReportTestWorkspaceNull()
        {
            //Value Declarations
            FullWorkspace workspace = null;
            bool isSubContractorUser = true;
            List<int> selectBOEs = new List<int>();
            ICollection<BoeCustomReportComponent> selectedComponents = new Collection<BoeCustomReportComponent>();
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary();
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();
            bool custom = false;

            ReportsControllerLogic sut = CreateSut();

            //Act
            sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, true, false, null, null, null, null);
        }

        /// <summary>
        /// This test case will test ExportAllBOEsReport. It will verify that
        /// this function will properly throw a ArgumentNullException when the
        /// HttpResponseBase is passed through as Null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExportAllBOEsReportTesthttpResponseNull()
        {
            //Value Declarations
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };
            bool isSubContractorUser = true;
            ICollection<BoeCustomReportComponent> selectedComponents = new Collection<BoeCustomReportComponent>();
            HttpResponseBase httpResponse = null;
            bool custom = false;

            ReportsControllerLogic sut = CreateSut();

            //Act
            sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse, isSubContractorUser, custom, null, null, null, null);
        }

        /// <summary>
        /// This test case will test ExportAllBOEsReport. It will verify that 
        /// this function will properly throw a ArgumentNullException when 
        /// wsExportFormatDTO is passed through as Null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExportAllBOEsReportTestWsExportFormatDTONull()
        {
            //Value Declarations
            FullWorkspace workspace = new FullWorkspace();
            bool isSubContractorUser = true;
            List<int> selectBOEs = new List<int>();
            ICollection<BoeCustomReportComponent> selectedComponents = new Collection<BoeCustomReportComponent>();
            ViewDataDictionary viewDataDictionary = new ViewDataDictionary();
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();
            bool custom = false;

            ReportsControllerLogic sut = CreateSut();

            //Act
            sut.ExportAllBOEsReport(workspace, selectedComponents, httpResponse.Object, true, false, null, null, null, null);
        }
        #endregion

        #region GetMetricNameTaskElementMappingDTO
        /// <summary>
        /// This Test check GetMetricNameTaskElementmappingDTO. It will verify that the 
        /// mapping of dto task element to metric names is correct and that the value
        /// return is the proper mapping.
        /// </summary>
        [TestMethod]
        public void GetMetricNameTaskElementMappingDTOTest()
        {
            //Value Declarations
            FullWorkspace workspace = new FullWorkspace() { Id = 1 };

            ReportsControllerLogic sut = CreateSut();

            //Act
            MetricNameTaskElementMappingDTO returnValue = sut.GetMetricNameTaskElementMappingDTO(workspace);

            //Assert
            Assert.AreEqual(returnValue.GetMetricNamesByTaskElementId(20), string.Empty);
        }

        /// <summary>
        /// This test checks GetMetricNameTaskElementMappingDTO. It will verify that when
        /// FullWorkspace is passed through that it will throw a ArgumentNullException. 
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetMetricNameTaskElementMappingDTOWorkspaceNull()
        {
            //Value Declaration
            FullWorkspace workspace = null;

            ReportsControllerLogic sut = CreateSut();

            //Act
            sut.GetMetricNameTaskElementMappingDTO(workspace);
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
            Assert.AreEqual("Revised Submittal Date", result.ElementAt(4));
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

        //Global local Value Declarations 
        private ICollection<FullBoe> boes;
        private ICollection<WorkspaceExportFormatDTO> wsExportFormatDTOCollection;
        private ICollection<BoeTaskElementDTO> TaskElementsID;
        private ICollection<OtherDirectCostDTO> dtoID;
        private ICollection<MaterialDTO> MaterialID;
    }
}
