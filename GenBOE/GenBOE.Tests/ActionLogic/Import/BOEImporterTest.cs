// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.PickList;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOEImporterTest
    {
        private Mock<IPermissionsDTODataLoader> permissionsDTOLoader = null;
        private Mock<IUserDTODataLoader> UserDTODataLoader = null;
        Mock<IFullObjectFactory> factory;
        Mock<IRetriever> retriever;
        Mock<IActiveDirectoryUtilities> adUtils;

        private BOEImporter CreateSystem()
        {
            permissionsDTOLoader = new Mock<IPermissionsDTODataLoader>();
            UserDTODataLoader = new Mock<IUserDTODataLoader>();
            this.factory = new Mock<IFullObjectFactory>();
            this.retriever = new Mock<IRetriever>();
            this.adUtils = new Mock<IActiveDirectoryUtilities>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

            return new BOEImporter(
                permissionsDTOLoader.Object,
                UserDTODataLoader.Object,
                factory.Object,
                adUtils.Object);
        }

        private WorkspaceDTO CreateWorkspaceTestData()
        {
            DateTime contractStartDate = new DateTime(2013, 4, 15);
            DateTime contractEndDate = new DateTime(2015, 8, 15);
            DateTime proposalSubmittalDate = new DateTime(2013, 2, 15);

            WorkspaceDTO workspace = new WorkspaceDTO
            {
                AllowSearch = false,
                BOEExportSortByID = 1,
                ContainsOCI = false,
                ContainsTemplate = false,
                ContractEndDate = contractEndDate,
                ContractStartDate = contractStartDate,
                CostVolumeLeadPricerUserID = 1,
                CreatedByUserID = 1,
                Description = "Description",
                TemplateID = (int)ExcelReportTemplateType.DS_ES_STANDARD_PORTRAIT_WITH_COST,
                NumberOfTimesExportedToProPricer = 0,
                PerfOrgListID = 1,
                PerfOrgsChanged = false,
                LineOfBusiness = new PickListDto() { Id = 1002, Text = "Commercial Ventures (COM)" }, //"Commercial Ventures (COM)"
                ProposalStatus = ProposalStatusType.None,
                ProposalSubmittalDate = proposalSubmittalDate,
                ProposalClass = new IES.Common.PickList.PickListDto { Id = Constants.PROPOSAL_CLASS_TYPE_NOT_SET, Text = "Not Set" },//not used by ISGS
                SelectedContractTypes = null,//not used by ISGS
                ResourceListID = 1,
                RFPNumber = "3.4.5.6",
                Segment = SegmentType.SSC,
                Shortname = "myworkspace",
                StatusComment = "Status comment",
                TrackingNumber = "11234",
                UpdateDate = DateTime.Now.AddDays(-1D),
                Id = 1,
                WorkspaceName = "myworkspace",
                WorkspaceState = WorkspaceState.Working
            };

            return workspace;
        }

        private Collection<BoeDTO> CreateBOETestData()
        {
            Collection<BoeDTO> allBoes = new Collection<BoeDTO>
            {
                new BoeDTO
                {
                    Id = 1,
                    Title = "BOE 1.1",
                    CustomFieldValueContainers = new Collection<CustomFieldValueContainer>(),
                    AuthorIDs = new Collection<int>{1},
                    SubcontractorAuthorIDs = new Collection<int>{3},
                    WBSID = 1,
                    CLINID = 1,
                    isMaterial = false,
                    State = BOEState.Draft,
                    WorkspaceID = 1,
                    DataSource = string.Empty,
                    Description = string.Empty,
                    EndDate = DateTime.MinValue,
                    HistoricMetricDisclosureChecked = false,
                    NumAuthorReassigned = 0,
                    StartDate = DateTime.MinValue,
                    SubmitForApprovalDate = DateTime.MinValue,
                    WCBID = -1
                }
            };

            return allBoes;
        }

        private Collection<WbsDTO> CreateWBSTestData(int workspaceId)
        {
            workspaceId = 1;
            Collection<WbsDTO> allWbs = new Collection<WbsDTO>
            {
                new WbsDTO
                {
                    Id = 1,
                    WbsNumber = "123",
                    WbsTitle = "WBS One",
                    ClinIDs = new Collection<int>{ 1 },
                    WorkspaceID = workspaceId
                },
                new WbsDTO
                {
                    Id = 2,
                    WbsNumber = "456",
                    WbsTitle = "WBS Two",
                    ClinIDs = new Collection<int>{ 1, 2 },
                    WorkspaceID = workspaceId
                },
                new WbsDTO
                {
                    Id = 3,
                    WbsNumber = "789",
                    WbsTitle = "WBS Three",
                    ClinIDs = new Collection<int>{ 3, 4 },
                    WorkspaceID = workspaceId
                }
            };

            return allWbs;
        }

        private Collection<FullClin> CreateCLINTestData()
        {
            Collection<FullClin> allClins = new Collection<FullClin>
            {
                new FullClin
                {
                    Id = 1,
                    ClinNumber = "135",
                    ClinTitle = "CLIN One",
                    WorkspaceID = 1
                },
                new FullClin
                {
                    Id = 2,
                    ClinNumber = "579",
                    ClinTitle = "CLIN Two",
                    WorkspaceID = 1
                },
                new FullClin
                {
                    Id = 3,
                    ClinNumber = "246",
                    ClinTitle = "CLIN Three",
                    WorkspaceID = 1
                },
                new FullClin
                {
                    Id = 4,
                    ClinNumber = "468",
                    ClinTitle = "CLIN Four",
                    WorkspaceID = 1
                }
            };

            return allClins;
        }

        private Collection<ImportedBoe> GetExpectedImportData()
        {
            Collection<ImportedBoe> importedBoes = new Collection<ImportedBoe>
            {
                new ImportedBoe
                {
                    Id = -1,
                    Title = string.Empty,
                    CustomFieldValueContainers = new Collection<CustomFieldValueContainer>(),
                    AuthorIDs = new Collection<int>{1},
                    ApproverIDs = new Collection<int> { 2 },
                    SubcontractorAuthorIDs = new Collection<int>{3},
                    WBSID = 1,
                    WbsString = "123 WBS One",
                    CLINID = 1,
                    ClinString = "135 CLIN One",
                    isMaterial = false,
                    State = BOEState.Draft,
                    WorkspaceID = 1,
                    ImportTypes = new Collection<BoeImportResult>(new List<BoeImportResult> { BoeImportResult.CreateBoe }),
                    DataSource = string.Empty,
                    Description = string.Empty,
                    EndDate = DateTime.MinValue,
                    HistoricMetricDisclosureChecked = false,
                    NumAuthorReassigned = 0,
                    StartDate = DateTime.MinValue,
                    SubmitForApprovalDate = DateTime.MinValue,
                    WCBID = -1
                },
                new ImportedBoe
                {
                    Id = -2,
                    Title = string.Empty,
                    AuthorIDs = new Collection<int>{1},
                    ApproverIDs = new Collection<int> { 2 },
                    SubcontractorAuthorIDs = new Collection<int>{3},
                    WBSID = 2,
                    WbsString = "456 WBS Two",
                    CLINID = 2,
                    ClinString = "579 CLIN Two",
                    isMaterial = true,
                    State = BOEState.Draft,
                    WorkspaceID = 1,
                    ImportTypes = new Collection<BoeImportResult>(new List<BoeImportResult> { BoeImportResult.CreateBoe }),
                    CustomFieldValueContainers = new Collection<CustomFieldValueContainer>(),
                    DataSource = string.Empty,
                    Description = string.Empty,
                    EndDate = DateTime.MinValue,
                    HistoricMetricDisclosureChecked = false,
                    NumAuthorReassigned = 0,
                    StartDate = DateTime.MinValue,
                    SubmitForApprovalDate = DateTime.MinValue,
                    WCBID = -1
                },
                new ImportedBoe
                {
                    Id = -3,
                    Title = string.Empty,
                    CustomFieldValueContainers = new Collection<CustomFieldValueContainer>(),
                    AuthorIDs = new Collection<int>{1},
                    ApproverIDs = new Collection<int> { 2 },
                    SubcontractorAuthorIDs = new Collection<int>{3},
                    WBSID = 2,
                    WbsString = "456 WBS Two",
                    CLINID = 1,
                    ClinString = "135 CLIN One",
                    isMaterial = true,
                    State = BOEState.Draft,
                    WorkspaceID = 1,
                    ImportTypes = new Collection<BoeImportResult>(new List<BoeImportResult> { BoeImportResult.CreateBoe }),
                    DataSource = string.Empty,
                    Description = string.Empty,
                    EndDate = DateTime.MinValue,
                    HistoricMetricDisclosureChecked = false,
                    NumAuthorReassigned = 0,
                    StartDate = DateTime.MinValue,
                    SubmitForApprovalDate = DateTime.MinValue,
                    WCBID = -1
                },
                new ImportedBoe
                {
                    Id = -4,
                    Title = string.Empty,
                    CustomFieldValueContainers = new Collection<CustomFieldValueContainer>(),
                    AuthorIDs = new Collection<int>{1},
                    ApproverIDs = new Collection<int> { 2 },
                    SubcontractorAuthorIDs = new Collection<int>{3},
                    WBSID = 3,
                    WbsString = "789 WBS Three",
                    CLINID = 3,
                    ClinString = "246 CLIN Three",
                    isMaterial = true,
                    State = BOEState.Draft,
                    WorkspaceID = 1,
                    ImportTypes = new Collection<BoeImportResult>(new List<BoeImportResult> { BoeImportResult.CreateBoe }),
                    DataSource = string.Empty,
                    Description = string.Empty,
                    EndDate = DateTime.MinValue,
                    HistoricMetricDisclosureChecked = false,
                    NumAuthorReassigned = 0,
                    StartDate = DateTime.MinValue,
                    SubmitForApprovalDate = DateTime.MinValue,
                    WCBID = -1
                },
                new ImportedBoe
                {
                    Id = -5,
                    Title = string.Empty,
                    CustomFieldValueContainers = new Collection<CustomFieldValueContainer>(),
                    AuthorIDs = new Collection<int>{1},
                    ApproverIDs = new Collection<int> { 2 },
                    SubcontractorAuthorIDs = new Collection<int>{3},
                    WBSID = 3,
                    WbsString = "789 WBS Three",
                    CLINID = 4,
                    ClinString = "468 CLIN Four",
                    isMaterial = true,
                    State = BOEState.Draft,
                    WorkspaceID = 1,
                    ImportTypes = new Collection<BoeImportResult>(new List<BoeImportResult> { BoeImportResult.CreateBoe }),
                    DataSource = string.Empty,
                    Description = string.Empty,
                    EndDate = DateTime.MinValue,
                    HistoricMetricDisclosureChecked = false,
                    NumAuthorReassigned = 0,
                    StartDate = DateTime.MinValue,
                    SubmitForApprovalDate = DateTime.MinValue,
                    WCBID = -1
                }
            };

            return importedBoes;
        }

        private void AssertAreEqual(ImportedBoe a, ImportedBoe b)
        {
            Assert.AreEqual(a.ApproverIDs.Count, b.ApproverIDs.Count);
            Assert.AreEqual(a.AuthorIDs.Count, b.AuthorIDs.Count);
            Assert.AreEqual(a.CLINID, b.CLINID);
            Assert.AreEqual(a.ClinString, b.ClinString);
            Assert.AreEqual(a.Id, b.Id);
            AssertHelpers.AssertAreEqual<BoeImportResult>(a.ImportTypes, b.ImportTypes);
            Assert.AreEqual(a.isMaterial, b.isMaterial);
            Assert.AreEqual(a.NumAuthorReassigned, b.NumAuthorReassigned);
            Assert.AreEqual(a.StartDate, b.StartDate);
            Assert.AreEqual(a.State, b.State);
            Assert.AreEqual(a.SubmitForApprovalDate, b.SubmitForApprovalDate);
            Assert.AreEqual(a.Title, b.Title);
            Assert.AreEqual(a.WBSID, b.WBSID);
            Assert.AreEqual(a.WbsString, b.WbsString);
            Assert.AreEqual(a.WCBID, b.WCBID);
            Assert.AreEqual(a.WorkspaceID, b.WorkspaceID);
        }

        //ToDo: Fix this test to run again
        public void ImportBoeFromExcelFile()
        {
            BOEImporter importer = this.CreateSystem();

            WorkspaceDTO workspace = this.CreateWorkspaceTestData();
            Collection<BoeDTO> boeTestData = CreateBOETestData();
            Collection<WbsDTO> wbsTestData = this.CreateWBSTestData(workspace.Id);
            Collection<FullClin> clinTestData = this.CreateCLINTestData();

            Collection<ImportedBoe> expectedImportData = GetExpectedImportData();

            Collection<ImportedBoe> actualImportData;

            Collection<FullWbs> wbsObjects = new Collection<FullWbs>();
            Collection<FullBoe> boeObjects = new Collection<FullBoe>();

            foreach (BoeDTO boe in boeTestData)
            {
                FullBoe boeObj = new FullBoe(boe);
                boeObjects.Add(boeObj);
                this.factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(boeObj);
            }
            foreach (WbsDTO wbs in wbsTestData)
            {
                wbsObjects.Add(new FullWbs(wbs));
            }

            FullWorkspace ws = new FullWorkspace(workspace);
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(wbsObjects);
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(boeObjects);
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(clinTestData);

            using (MemoryStream file = new MemoryStream(Properties.Resources.BOEImporterTest))
            {
                actualImportData = importer.ImportBoeFromExcelFile(file, ws);
            }

            Assert.AreEqual(expectedImportData.Count, actualImportData.Count);

            for (int i = 0; i < expectedImportData.Count; i++)
            {
                this.AssertAreEqual(expectedImportData[i], actualImportData[i]);
            }
        }
    }
}
