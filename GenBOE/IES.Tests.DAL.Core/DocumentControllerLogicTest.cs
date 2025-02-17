// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests.Core
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data.Entity.Core;
	using System.Diagnostics;
	using System.Linq;
	using System.Security.Principal;
	using System.Threading;
	using System.Transactions;
	using DataBridge.Loaders;
	using DataBridge.ModelViews;
	using GenTRAC.DataBridge.Core.Common.Security;
	using GenTRAC.DataBridge.Core.DTO.OrgData;
	using GenTRAC.DataBridge.Core.DTO.Proposal;
	using GenTRAC.DataBridge.Core.DTO.User;
	using IES.ActionLogic.Core.ControllerLogic;
	using IES.ActionLogic.Core.IO.Export;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Loaders;
	using IES.Common.Core.Security;
	using IES.Common.Core.Services;
	using IES.Common.Core;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Logging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Models;
	using Moq;
	using IES.Common.Core.Constants;

	/// <summary>
	/// Test Class for Document Controller Logic and Document Loader.
	/// </summary>
	[TestClass]
	[Ignore("Ignoring until loaders are properly mocked as this is a logic test")]
    public class DocumentControllerLogicTest
    {
        private SecurityMapper securityMapper;
        private RevisionLoader revisionLoader;
        private Mock<ISectionLoader> sectionLoader;
        private RateDetailLoader rateDetailLoader;
        private IDocumentDetailLoader documentDetailLoader;
        private IRdsbRateCodeXrefLoader rdsbRateCodeXrefLoader;
        private IRdsbSectionXrefLoader rdsbSectionXrefLoader;
        private ISecurityInformation securityInformation;
        private IProposalLoader proposalLoader;
		// private IProposalPermissionLoader proposalPermissionLoader;
        private IUserLoader userLoader;
        private IActiveDirectoryService AD;

        private int testProposalId = -1;

        /// <summary>
        /// Performs setup initialization for the tests.
        /// </summary>
        [TestInitialize]
        public void TestSetup()
        {
			ICacheService c = Mock.Of<ICacheService>();
			IHttpContextAccessor httpContextAccessor = Mock.Of<HttpContextAccessor>();
			this.AD = new ActiveDirectoryService(Mock.Of<ILogger<ActiveDirectoryService>>(), c,300);
            this.securityInformation = new SecurityInformation(AD, c, httpContextAccessor);
            CacheDataLoader cache = new(Mock.Of<ILogger<CacheDataLoader>>(), c, 500);
            UserMapper um = new(new UserLoader(AD, Mock.Of<ILogger<UserLoader>>()), cache, this.securityInformation, AD, c, Mock.Of<ILogger<UserMapper>>());
            this.securityMapper = new SecurityMapper(new SecurityUserAuthorizationsDataLoader(AD, Mock.Of<ILogger<SecurityUserAuthorizationsDataLoader>>()), this.securityInformation, um, c, Mock.Of<ILogger<SecurityMapper>>());
            this.revisionLoader = new RevisionLoader(Mock.Of<ILogger<RevisionLoader>>());
			this.sectionLoader = new Mock<ISectionLoader>();
            this.rateDetailLoader = new RateDetailLoader(Mock.Of<ILogger<RateDetailLoader>>(), new RateCodeYearLoader(Mock.Of<ILogger<RateCodeYearLoader>>()), new ProPricerRateCodeXrefLoader(Mock.Of<ILogger<ProPricerRateCodeXrefLoader>>()), Mock.Of<ISecurityInformation>());
            this.rdsbRateCodeXrefLoader = new RdsbRateCodeXrefLoader(Mock.Of<ILogger<RdsbRateCodeXrefLoader>>());
            this.rdsbSectionXrefLoader = new RdsbSectionXrefLoader(Mock.Of<ILogger<RdsbSectionXrefLoader>>());
            this.documentDetailLoader = new DocumentDetailLoader(this.rdsbRateCodeXrefLoader, this.rdsbSectionXrefLoader, Mock.Of<ILogger<DocumentDetailLoader>>());
            this.proposalLoader = new ProposalLoader(Mock.Of<ILogger<ProposalLoader>>());
            this.userLoader = new UserLoader(this.AD, Mock.Of<ILogger<UserLoader>>());
            // this.proposalPermissionLoader = new ProposalPermissionLoader();
            // setup the initial Document
            IDocumentControllerLogic sut = CreateSut();

            // clear out any previous test docs
            ICollection<DocumentGridModelView> linkedDocuments = sut.RetrieveAllLinkedDocuments(this.GetNonAdminRoles(), "paliderd");
            if (linkedDocuments.Any())
            {
                foreach (DocumentGridModelView document in linkedDocuments)
                {
                    sut.DeleteDocument(document.ProposalId);
                }
            }

            ICollection<ProposalDto> nonAdminProposals = sut.RetrieveUnlinkedProposals(this.GetNonAdminRoles(), "paliderd");

            GenericIdentity identity = new("acct04\\paliderd");

            GenericPrincipal principal = new(identity, null);

            Thread.CurrentPrincipal = principal;

            if (nonAdminProposals.Count < 2)
            {
                int userId = this.userLoader.GetByNtid("paliderd").Id;
                LineOfBusinessDataLoader lobLoader = new(Mock.Of<ILogger<LineOfBusinessDataLoader>>());
                // we need to make two new ones
                this.CreateProposal(lobLoader, userId);
                this.CreateProposal(lobLoader, userId);
            }
            else
            {
                this.testProposalId = nonAdminProposals.First().Id;                
            }

			DocumentDetailModelView docDetails = sut.RetrieveDocumentDetailByProposalId(this.testProposalId, true); // create RDSB document if it doesn't already exist
			docDetails.SelectedRateCodeIds = new Collection<int>() { 1, 2, 3 };
			docDetails.SelectedSectionIds = new Collection<int>() { 4, 5, 6 };
			docDetails.StartYear = 2016;
			docDetails.EndYear = 2020;
			docDetails.ParentSection = "5";

			//try
			//{
			//    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
			//    {
			//        sut.SaveDocument(docDetails);
			//        scope.Complete();
			//    }
			//}
			//catch (Exception e)
			//{
			//    Assert.Fail("Document save failed: " + e.Message);
			//}
		}

        /// <summary>
        /// Creates a new proposal for the specified user using the first LOB found.
        /// </summary>
        /// <param name="lobLoader">The lob loader.</param>
        /// <param name="userId">The user identifier.</param>
        private void CreateProposal(LineOfBusinessDataLoader lobLoader, int userId)
        {
            string proposalIdentifier = TestData.CreateRandomWord(6);
            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                this.testProposalId = proposalLoader.Save(new ProposalDto()
                {
                    ProposalTitle = "Mock" + proposalIdentifier,
                    Updateable = UpdateType.Upsert,
                    CreatedByUserId = userId,
                    DateCreated = DateTime.Now,
                    Customer = "customer",
                    DeliveryDate = DateTime.Now,
                    RFPNumber = "12345",
                    ProgramProposalStatus = ProgramProposalStatus.LMRetainedSSC,
                    ProposalStatus = ProposalStatus.InProgress,
                    WorkflowStatus = WorkflowStatus.NotStarted,
                    ContractTypeGroup = 1, // ContractTypeGroup.CP,
                    BoeTool = BOETool.Excel,
                    CustomerType = CustomerType.FederalGovernment,
                    LineOfBusinessID = lobLoader.GetPickListValues().First().Id,
                    ISGSRole = ISGSRole.Prime,
                    ContractTypeIds = new List<int>() { 1, 4 }, // CostPlusAwardFee, FirmFixedPrice
                    CostElementTypeIds = new List<int> { (int)CostElementType.Labor },
                    OTISOpportunityID = "p",
                    EstimatedProposalValue = 0,
                    ProgramAreaId = 46,
                    ProposalLocation = ProposalLocation.ValleyForgePA,
                    PricingTool = PricingTool.Excel,
                    ProposalType = 3,
                    Request = 1,
                    ProposalClass = 1,
                    IsScheduleProposal = false,
					DateAssigned = DateTime.Now,
                    IsCCPDRequired = false
                }).Value;

                //this.proposalPermissionLoader.Save(new ProposalPermissionDto()
                //{
                //    ProposalID = this.testProposalId,
                //    ResourceType = ResourceType.Pricer,
                //    Role = PtmRole.Pricer,
                //    Updateable = UpdateType.Upsert,
                //    UserId = userId
                //});

                scope.Complete();
            }
        }

        /// <summary>
        /// Runs cleanup after the tests.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            IDocumentControllerLogic sut = this.CreateSut();

            // clear out any test docs
            ICollection<DocumentGridModelView> linkedDocuments = sut.RetrieveAllLinkedDocuments(this.GetNonAdminRoles(), "paliderd");
            if (linkedDocuments.Any())
            {
                foreach (DocumentGridModelView document in linkedDocuments)
                {
                    sut.DeleteDocument(document.ProposalId);
                }
            }
        }

        /// <summary>
        /// Creates the System under test.
        /// </summary>
        /// <returns></returns>
        private IDocumentControllerLogic CreateSut()
        {
            IDocumentControllerLogic sut = new DocumentControllerLogic(Mock.Of<ILogger<DocumentControllerLogic>>(),
				proposalLoader,
				CreateSutLoader(), 
				this.documentDetailLoader, 
				this.AD, 
				this.securityInformation, 
				this.revisionLoader, 
				this.sectionLoader.Object, 
				this.rateDetailLoader, 
				Mock.Of<IFileAttachmentLoader>(),
				Mock.Of<IPPRDExporter>());

            return sut;
        }

        /// <summary>
        /// Creates the sut loader.
        /// </summary>
        /// <returns></returns>
        private IDocumentLoader CreateSutLoader()
        {
			return new DocumentLoader(this.revisionLoader, Mock.Of<ILogger<DocumentLoader>>());
		}

        /// <summary>
        /// Gets the admin role.
        /// </summary>
        /// <returns></returns>
        private IReadOnlyCollection<SecurityPermissionsResponse> GetAdminRole()
        {
            List<SecurityPermissionsResponse> roles = new() { new SecurityPermissionsResponse(PtmRole.Admin, null) };

            return roles.AsReadOnly();
        }

        /// <summary>
        /// Gets the edit roles.
        /// </summary>
        /// <returns></returns>
        private IReadOnlyCollection<SecurityPermissionsResponse> GetNonAdminRoles()
        {
            List<SecurityPermissionsResponse> roles = this.securityMapper.GetRolesForUser("paliderd").Where(r => r.AuthorizedRole != PtmRole.Admin).ToList();
            
            return roles.AsReadOnly();
        }

        /// <summary>
        /// Helper method to get the IDs of the required sections from the given sections
        /// </summary>
        /// <param name="sections">The sections</param>
        /// <returns>Collection of IDs of the required sections</returns>
        private ICollection<int> GetRequiredSectionIds(ICollection<SectionModelView> sections)
        {
			ICollection<int> toReturn = new Collection<int>();

            foreach (SectionModelView section in sections)
            {
                if (section.IsRdsbRequired)
                {
                    toReturn.Add(section.Id);
                }

                if (section.ChildNodes != null && section.ChildNodes.Any())
                {
                    toReturn.AddRange(this.GetRequiredSectionIds(section.ChildNodes));
                }
            }

            return toReturn;
        }

        [TestMethod]
        public void TestDocumentValidation()
        {
            IDocumentControllerLogic sut = CreateSut();
            IList<RevisionModelView> revisions = this.revisionLoader.GetAll().Where(r => r.DatePublished.HasValue).OrderByDescending(r => r.DatePublished).ToList();

            RevisionModelView revision = revisions.First();
			ICollection<SectionModelView> sections = new Collection<SectionModelView>(); // TODO Mock this out - originally called this.sectionLoader.RetrieveAllSections(new RevisionModelView() { Id = revision.Id }, true);
            ICollection<int> requiredSectionIds = this.GetRequiredSectionIds(sections);
            ICollection<RdsbRateDetailModelView> rates = this.rateDetailLoader.GetRatesForRdsbDocument(revision.Id);

            // set up selected section ids with all require ids, plus the first section and subsection
            ICollection<int> selectedSectionIds = new Collection<int>();
            selectedSectionIds.AddRange(requiredSectionIds);

            if (!selectedSectionIds.Contains(sections.First().Id))
            {
                selectedSectionIds.Add(sections.First().Id);
            }

            if (!selectedSectionIds.Contains(sections.First().ChildNodes.First().Id))
            {
                selectedSectionIds.Add(sections.First().ChildNodes.First().Id);
            }

            DocumentDetailModelView detail = new()
			{
                Id = 1,
                SelectedRateCodeIds = new int[] { rates.First().Id },
                SelectedRevisionId = revision.Id,
                SelectedSectionIds = selectedSectionIds,
                StartYear = revision.StartYear,
                EndYear = revision.EndYear,
                ParentSection = "4"
                // ProposalId is validated inside the controller
            };

            ICollection<ValidationMessage> messages = sut.ValidateDocumentDetailModelView(detail);

            Assert.AreEqual(0, messages.Count);

            // still valid
            detail.StartYear = revision.EndYear;
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(0, messages.Count);

            // start year not set
            detail.StartYear = 0;
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(1, messages.Count);

            // end year and start year not set
            detail.EndYear = 0;
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(2, messages.Count);

            // start after end year
            detail.StartYear = 3;
            detail.EndYear = 2;
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(1, messages.Count);

            // start and end not within revision's years
            detail.EndYear = 4;
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(2, messages.Count);

            // start not within revision's years
            detail.StartYear = revision.StartYear - 1;
            detail.EndYear = revision.EndYear;
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(1, messages.Count);

            // end not within revision's years
            detail.StartYear = revision.StartYear;
            detail.EndYear = revision.EndYear + 1;
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(1, messages.Count);

            // Bad Revision Id
            detail.StartYear = revision.StartYear;
            detail.EndYear = revision.EndYear;
            detail.SelectedRevisionId = null;
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(1, messages.Count);

            // missing/bad rate codes
            detail.SelectedRevisionId = revision.Id;
            detail.SelectedRateCodeIds = null;
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(1, messages.Count);

            detail.SelectedRateCodeIds = new int[] { 0 };
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(1, messages.Count);

            // missing/bad sections
            detail.SelectedSectionIds = null;
            detail.SelectedRateCodeIds = new int[] { rates.First().Id, rates.Last().Id };
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(1, messages.Count);

            // section not found
            detail.SelectedSectionIds = new Collection<int>() { int.MaxValue };
            detail.SelectedSectionIds.AddRange(requiredSectionIds);
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(1, messages.Count);

            // missing required sections
            if (requiredSectionIds.Any())
            {
                detail.SelectedSectionIds = new Collection<int>() { sections.First().Id };
                messages = sut.ValidateDocumentDetailModelView(detail);
                Assert.AreEqual(1, messages.Count);
            }

            detail.SelectedSectionIds = new int[] { 0 };
            messages = sut.ValidateDocumentDetailModelView(detail);
            Assert.AreEqual(1, messages.Count);
        }

        /// <summary>
        /// Tests the retrieve unlinked proposals.
        /// </summary>
        [TestMethod]
        public void TestRetrieveUnlinkedProposals()
        {
            IDocumentControllerLogic sut = CreateSut();

            ICollection<ProposalDto> proposals = sut.RetrieveUnlinkedProposals(this.GetAdminRole(), "paliderd");
            Assert.IsTrue(proposals.Count > 0);

            IReadOnlyCollection<SecurityPermissionsResponse> editRoles = this.GetNonAdminRoles();
            ICollection<ProposalDto> nonAdminProposals = sut.RetrieveUnlinkedProposals(editRoles, "paliderd");

            Assert.IsTrue(nonAdminProposals.Count > 0);
            Assert.IsTrue(proposals.Count > nonAdminProposals.Count);

            proposals.ToList().ForEach(p => Debug.WriteLine(p.Id.ToString()));
            Debug.WriteLine("non-admin");

            // confirm the nonAdmin set is inside the admin set
            foreach (ProposalDto p in nonAdminProposals)
            {
                Debug.WriteLine(p.Id.ToString());
                Assert.IsTrue(proposals.Any(pr => pr.Id == p.Id));
            }

            Assert.IsFalse(proposals.Any(p => p.ProposalStatus != ProposalStatus.InProgress && p.ProposalStatus != ProposalStatus.PendingCertification && p.ProposalStatus != ProposalStatus.PendingAward));
            Assert.IsFalse(nonAdminProposals.Any(p => p.ProposalStatus != ProposalStatus.InProgress && p.ProposalStatus != ProposalStatus.PendingCertification && p.ProposalStatus != ProposalStatus.PendingAward));

            Assert.IsFalse(proposals.Any(p => p.DocumentId.HasValue));
            Assert.IsFalse(nonAdminProposals.Any(p => p.DocumentId.HasValue));

            Assert.IsFalse(proposals.Any(p => p.CustomerType == CustomerType.Commercial || p.CustomerType == CustomerType.InternationalCommercial));
            Assert.IsFalse(proposals.Any(p => p.ProposalStatus == ProposalStatus.Revised));
            Assert.IsFalse(nonAdminProposals.Any(p => p.CustomerType == CustomerType.Commercial || p.CustomerType == CustomerType.InternationalCommercial));

            Assert.IsTrue(nonAdminProposals.All(p => editRoles.Any(e => e.ProposalID == p.Id && CommonConstants.EDIT_ROLES.Contains(e.AuthorizedRole))));
        }

        /// <summary>
        /// Tests the retrieve all linked documents.
        /// </summary>
        [TestMethod]
        public void TestRetrieveAllLinkedDocuments()
        {
            IDocumentControllerLogic sut = CreateSut();

            ICollection<DocumentGridModelView> allDocuments = sut.RetrieveAllLinkedDocuments(this.GetAdminRole(), "paliderd");
            Assert.IsTrue(allDocuments.Count > 0);
            
            int totalCount;
            using (IESEntities context = new())
            {
                totalCount = context.RDSBDocumentInformations.Count();
            }

            Assert.AreEqual(totalCount, allDocuments.Count);

            ICollection<DocumentGridModelView> nonAdminDocuments = sut.RetrieveAllLinkedDocuments(this.GetNonAdminRoles(), "paliderd");

            Assert.IsTrue(nonAdminDocuments.Count > 0);
            Assert.IsTrue(allDocuments.Count >= nonAdminDocuments.Count);

            // confirm the nonAdmin set is inside the admin set
            foreach (DocumentGridModelView d in nonAdminDocuments)
            {
                Assert.IsTrue(allDocuments.Any(pr => pr.ProposalId == d.ProposalId));
            }
        }

        /// <summary>
        /// Test RetrieveDocumentDetailByProposalID
        /// </summary>
        [TestMethod]
        public void TestRetrieveDocumentDetailByProposalID()
        {
            IDocumentControllerLogic sut = this.CreateSut();

            ProposalDto proposal = this.proposalLoader.GetById(this.testProposalId);

            DocumentDetailModelView result = sut.RetrieveDocumentDetailByProposalId(this.testProposalId);

            int revisionId = this.revisionLoader.GetAll().Where(x => x.DatePublished.HasValue)
                .OrderByDescending(x => x.DatePublished).First().Id;

            Assert.IsNotNull(result);
            Assert.AreEqual(this.testProposalId, result.ProposalId);
            Assert.AreEqual(proposal.TrackingNumber, result.ProposalTrackingNumber);
            Assert.AreEqual(proposal.ProposalTitle, result.ProposalTitle);
            Assert.AreEqual(proposal.ProposalStatus.ToDescription(), result.ProposalStatus);
            Assert.AreEqual(this.AD.GetUserByQualifiedAccount(this.securityInformation.ActiveUserNTID, false).DisplayName, result.DocumentCreatedBy);
            Assert.AreEqual(revisionId, result.SelectedRevisionId);
            Assert.AreEqual(2016, result.StartYear);
            Assert.AreEqual(2020, result.EndYear);
            Assert.AreEqual(3, result.SelectedRateCodeIds.Count);
            Assert.AreEqual(3, result.SelectedSectionIds.Count);
            Assert.IsTrue(result.SelectedRateCodeIds.Any(x => x == 1 || x == 2 || x == 3));
            Assert.IsTrue(result.SelectedSectionIds.Any(x => x == 4 || x == 5 || x == 6 ));
            Assert.AreEqual(1, result.AvailableRevisions.Count); // Should only return the most recent revision since it's new
            Assert.AreEqual(revisionId, result.AvailableRevisions.First().Id);
        }

        /// <summary>
        /// Test that method fails when proposal id is less than 0
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GenValidationException))]
        public void TestRetrieveDocumentDetailByProposalID_EX1()
        {
            IDocumentControllerLogic sut = this.CreateSut();
            sut.RetrieveDocumentDetailByProposalId(-1);
        }

        /// <summary>
        /// Test that method fails when proposal id is invalid
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GenValidationException))]
        public void TestRetrieveDocumentDetailByProposalID_EX2()
        {
            IDocumentControllerLogic sut = this.CreateSut();
            // This code assumes no proposals with id 1
            // Current Proposals start at id 4435
            sut.RetrieveDocumentDetailByProposalId(1);
        }

        /// <summary>
        /// Test that method fails when proposal is not linked to a document
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(GenValidationException))]
        public void TestRetrieveDocumentDetailByProposalID_EX3()
        {
            IDocumentControllerLogic sut = this.CreateSut();
            ICollection<ProposalDto> proposals = sut.RetrieveUnlinkedProposals(this.GetNonAdminRoles(), "paliderd").Where(x => !x.DocumentId.HasValue).ToCollection();
            sut.RetrieveDocumentDetailByProposalId(proposals.First().Id);
        }

        /// <summary>
        /// Test that method fails when the document is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSaveDocument_EX()
        {
            IDocumentControllerLogic sut = this.CreateSut();
            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.SaveDocument(null);
                scope.Complete();
            }
        }

        [TestMethod]
        public void DocumentLoaderSave()
        {
			IDocumentLoader sut = CreateSutLoader();
            IList<RevisionModelView> revisions = this.revisionLoader.GetAll().Where(r => r.DatePublished.HasValue).OrderByDescending(r => r.DatePublished).ToList();
            RevisionModelView latestRevision = revisions.First();

            // delete the document if it is already there (from previous test if test failed)
            DocumentGridModelView actual = sut.GetByProposalIds(new int[] { 5000 }, latestRevision.Id).FirstOrDefault();
            if (actual != null)
            {
                actual.Updateable = UpdateType.Deleted;
                using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
                {
                    sut.Save(actual);
                    scope.Complete();
                }
            }

            DocumentGridModelView expected = new()
			{
                DocumentCreatedBy = "Doo, Scooby",
                RDMRevisionId = revisions.First().Id,
                ProposalId = 5000,
                Updateable = UpdateType.Upsert,
                StartYear = 2018,
                EndYear = 2023,
				RevisionSegment = RevisionSegment.Core,
				Id = -1
            };

            int? docId;
            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                docId = sut.Save(expected);
                scope.Complete();
            }

            actual = sut.GetByProposalIds(new int[] { 5000 }, latestRevision.Id).FirstOrDefault();

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.DocumentCreatedBy, actual.DocumentCreatedBy);
            Assert.AreEqual(revisions.First().DisplayRevision, actual.PPRDVersion);
            Assert.AreEqual(revisions.First().DatePublished, actual.PPRDVersionDate);
            Assert.AreEqual(expected.RDMRevisionId, actual.RDMRevisionId);
            Assert.AreEqual(expected.ProposalId, actual.ProposalId);
            Assert.AreEqual(expected.StartYear, actual.StartYear);
            Assert.AreEqual(expected.EndYear, actual.EndYear);
			Assert.AreEqual(expected.RevisionSegment, actual.RevisionSegment);
            Assert.IsNull(actual.ProposalStatus);
            Assert.IsNull(actual.ProposalTitle);
            Assert.IsNull(actual.ProposalTrackingNumber);
            Assert.AreEqual(expected.RDMRevisionId == latestRevision.Id, actual.IsUsingLatest);

            DocumentDetailModelView detail = this.documentDetailLoader.GetByProposalId(5000);
            // test updating the values
            detail.ParentSection = "55";
            detail.StartYear = 1990;
            detail.EndYear = 1999;
            detail.Updateable = UpdateType.Upsert;
            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                this.documentDetailLoader.Save(detail);
                scope.Complete();
            }

            detail = this.documentDetailLoader.GetByProposalId(5000);

            Assert.AreEqual("55", detail.ParentSection);
            Assert.AreEqual(1990, detail.StartYear);
            Assert.AreEqual(1999, detail.EndYear);

            actual = sut.GetByProposalIds(new int[] { 5000 }, latestRevision.Id).FirstOrDefault();
            actual.Updateable = UpdateType.Deleted;
            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                docId = sut.Save(actual);
                scope.Complete();
            }

            actual = sut.GetByProposalIds(new int[] { 5000 }, latestRevision.Id).FirstOrDefault();

            Assert.IsNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DocumentLoader_SaveEx1()
        {
            // missing ProposalId
            IList<RevisionModelView> revisions = this.revisionLoader.GetAll().Where(r => r.DatePublished.HasValue).ToList();
            DocumentGridModelView expected = new()
			{
                RDMRevisionId = revisions.First().Id,
                DocumentCreatedBy = "Doo, Scooby",
                Updateable = UpdateType.Upsert,
                Id = -1
            };

			IDocumentLoader sut = CreateSutLoader();
            int? docId;
            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                docId = sut.Save(expected);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(EntityCommandExecutionException))]
        public void DocumentLoader_SaveEx2()
        {
            // missing ProposalId
            IList<RevisionModelView> revisions = this.revisionLoader.GetAll().Where(r => r.DatePublished.HasValue).ToList();
            DocumentGridModelView expected = new()
			{
                RDMRevisionId = revisions.First().Id,
                ProposalId = 5000,
                Updateable = UpdateType.Upsert,
                Id = -1
            };

			IDocumentLoader sut = CreateSutLoader();
            int? docId;
            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                docId = sut.Save(expected);
            }
        }

        /// <summary>
        /// Test that save fails when dto is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DocumentDetailLoader_Save_EX()
        {
            this.documentDetailLoader.Save((DocumentDetailModelView)null);
        }

        /// <summary>
        /// Test that delete throws not implemented exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void DocumentDetailLoader_Delete_EX()
        {
            DocumentDetailModelView modelView = new();
            modelView.Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                this.documentDetailLoader.Save(modelView);
                scope.Complete();
            }
        }

        /// <summary>
        /// Test that upsert throws not implemented exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RdsbRateCodeXrefLoader_Upsert_EX()
        {
            RdsbRateCodeXrefModelView modelView = new();
            modelView.Id = 1; // needs an id > 0
            modelView.Updateable = UpdateType.Upsert;

            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                this.rdsbRateCodeXrefLoader.BulkSave(new Collection<RdsbRateCodeXrefModelView>() {modelView});
                scope.Complete();
            }
        }

        /// <summary>
        /// Test that delete throws not implemented exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RdsbRateCodeXrefLoader_Delete_EX()
        {
            RdsbRateCodeXrefModelView modelView = new();
            modelView.Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                this.rdsbRateCodeXrefLoader.BulkSave(new Collection<RdsbRateCodeXrefModelView>() { modelView });
                scope.Complete();
            }
        }

        /// <summary>
        /// Test that upsert throws not implemented exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RdsbSectionXrefLoader_Upsert_EX()
        {
            RdsbSectionXrefModelView modelView = new();
            modelView.Id = 1; // needs an id > 0
            modelView.Updateable = UpdateType.Upsert;

            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                this.rdsbSectionXrefLoader.BulkSave(new Collection<RdsbSectionXrefModelView>() { modelView });
                scope.Complete();
            }
        }

        /// <summary>
        /// Test that delete throws not implemented exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RdsbSectionXrefLoader_Delete_EX()
        {
            RdsbSectionXrefModelView modelView = new();
            modelView.Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                this.rdsbSectionXrefLoader.BulkSave(new Collection<RdsbSectionXrefModelView>() { modelView });
                scope.Complete();
            }
        }

        /// <summary>
        /// Test DoesRecordExist for an existing record
        /// </summary>
        [TestMethod]
        public void DoesRecordExist_True()
        {
            IDocumentLoader sut = CreateSutLoader();
            int existingId;

            using (IESEntities context = new())
            {
                RDSBDocumentInformation existingRecord = context.RDSBDocumentInformations.FirstOrDefault();
                existingId = existingRecord.PTMProposalID;
            }

            bool result = sut.DoesRecordExist(existingId);
            Assert.IsTrue(result);
        }


        /// <summary>
        /// Test DoesRecordExist for an existing record
        /// </summary>
        [TestMethod]
        public void DoesRecordExist_False()
        {
            IDocumentLoader sut = CreateSutLoader();
            bool result = sut.DoesRecordExist(int.MaxValue);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test getting Top Level Section number for Rate Codes
        /// </summary>
        [TestMethod]
        public void TestGetTopLevelSectionsForRateCodes()
        {
            IDocumentControllerLogic sut = CreateSut();

            // pulling from QAS, so assuming we are using Proposal tracking # 22-00013 (id 15297) that ties into PPRD Revision 299
            ICollection<string> rateCodes = new string[] { "FXDDAC1234", "XCZDPA1234" };
            ICollection<string> results = sut.GetTopLevelSectionsForRateCodes(rateCodes, 15297);

            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);
        }

        /// <summary>
        /// Test getting Top Level Section number for Rate Descriptions
        /// </summary>
        [TestMethod]
        public void TestGetTopLevelSectionsForRateDescriptions()
        {
            IDocumentControllerLogic sut = CreateSut();

            // pulling from QAS, so assuming we are using Proposal tracking # 22-00013 (id 15297) that ties into PPRD Revision 299
            ICollection<string> rateCodes = new string[] { "Denver FBM", "Titusville Development Lvl 1" };
            ICollection<string> results = sut.GetTopLevelSectionsForRateDescriptions(rateCodes, 15297);

            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);
        }

        /// <summary>
        /// Tests GetAddresses
        /// </summary>
        [TestMethod]
        public void TestGetAddresses()
        {
            IDocumentControllerLogic sut = CreateSut();
            int existingRevisionId;

            using (IESEntities context = new())
            {
                Revision existingRevision = context.Revisions.ToList().LastOrDefault();
                existingRevisionId = existingRevision.ID;
            }

            ICollection<SectionAddressModelView> result = sut.GetAddresses(existingRevisionId);

            Assert.IsNotNull(result);
            if (result.Any())
            {
                SectionAddressModelView oneAddress = result.FirstOrDefault();
                Assert.IsTrue(oneAddress.Id != 0);
            }
        }

		/// <summary>
		/// Tests GetSectionSelectionForRevisionChange
		/// </summary>
		[TestMethod]
		public void TestGetSectionSelectionForRevisionChange()
		{
			IDocumentControllerLogic sut = CreateSut();

			ICollection<SectionModelView> fromSections = new Collection<SectionModelView>()
			{
				new()
				{
					Id = 1,
					Title = "Test 1",
					ReferenceNumber = "1"
				},
				new()
				{
					Id = 2,
					Title = "Test 2",
					ReferenceNumber = "1.1"
				},
				new()
				{
					Id = 3,
					Title = "Test 3",
					ReferenceNumber = "1.1.1"
				},
				new()
				{
					Id = 4,
					Title = "Test 4",
					ReferenceNumber = "1.2"
				},
				new()
				{
					Id = 5,
					Title = "Test 5",
					ReferenceNumber = "2"
				},
				new()
				{
					Id = 6,
					Title = "Test 5",
					ReferenceNumber = "2.1"
				},
				new()
				{
					Id = 7,
					Title = "Test 5",
					ReferenceNumber = "3"
				}
			};

			ICollection<SectionModelView> toSections = new Collection<SectionModelView>()
			{
				new()
				{
					Id = 101,
					Title = "Test 1",
					ReferenceNumber = "1"
				},
				new()
				{
					Id = 102,
					Title = "Test 2.0",
					ReferenceNumber = "1.1"
				},
				new()
				{
					Id = 103,
					Title = "Test 3",
					ReferenceNumber = "1.2.1"
				},
				new()
				{
					Id = 104,
					Title = "Test 4",
					ReferenceNumber = "1.2"
				},
				new()
				{
					Id = 105,
					Title = "Test 5",
					ReferenceNumber = "2"
				},
				new()
				{
					Id = 106,
					Title = "Test 5",
					ReferenceNumber = "2.1"
				},
				new()
				{
					Id = 107,
					Title = "Test 5",
					ReferenceNumber = "3.1"
				}
			};

			int fromRevisionId = 1;
			int toRevisionId = 2;
			ICollection<int> selectedIds = new Collection<int> { 1, 2, 3, 5, 7 };

			sectionLoader.Setup(x => x.GetFlatSectionIdsTitlesAndRefNumbersByRevisionId(fromRevisionId)).Returns(fromSections);
			sectionLoader.Setup(x => x.GetFlatSectionIdsTitlesAndRefNumbersByRevisionId(toRevisionId)).Returns(toSections);

			SectionSelectionModelView result = sut.GetSectionSelectionForRevisionChange(fromRevisionId, toRevisionId, selectedIds);

			Assert.AreEqual(3, result.SelectedSections.Count);
			Assert.AreEqual(2, result.UnmappedSections.Count);
			Assert.IsTrue(result.SelectedSections.Contains(101)); // Simple Title map, same Ref Number
			Assert.IsTrue(result.SelectedSections.Contains(103)); // Simple Title map, different Ref Number
			Assert.IsTrue(result.SelectedSections.Contains(105)); // Duplicate Title but matching Ref Number
			Assert.IsTrue(result.UnmappedSections.Contains("Test 2")); // Title no longer there
			Assert.IsTrue(result.UnmappedSections.Contains("Test 5")); // Duplicate Title but no matching Ref Number
		}

	}
}
