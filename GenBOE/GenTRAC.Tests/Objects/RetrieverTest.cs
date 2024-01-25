// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Tests.DAL;
    using IES.Common;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests the Retriever Class
    /// </summary>
    [TestClass]
    public class RetrieverTest
    {
        #region Setup

        /// <summary>
        /// User Mapper
        /// </summary>
        private Mock<IUserMapper> userMapper;

        /// <summary>
        /// Permissions Mapper
        /// </summary>
        private Mock<IProposalPermissionMapper> proposalPermissionsMapper;

        /// <summary>
        /// Proposal checklist loader
        /// </summary>
        private Mock<IProposalChecklistLoader> proposalChecklistLoader;

        /// <summary>
        /// PPR checklist content mapper
        /// </summary>
        private Mock<IChecklistContentMapper> pprChecklistContentMapper;

        /// <summary>
        /// PAR checklist content mapper
        /// </summary>
        private Mock<IChecklistContentMapper> parChecklistConteMapper;

        /// <summary>
        /// Creates the system under test
        /// </summary>
        /// <returns>Retriever</returns>
        private Retriever CreateSystem()
        {
            this.userMapper = new Mock<IUserMapper>();
            this.proposalPermissionsMapper = new Mock<IProposalPermissionMapper>();
            this.proposalChecklistLoader = new Mock<IProposalChecklistLoader>();
            this.parChecklistConteMapper = new Mock<IChecklistContentMapper>();
            this.pprChecklistContentMapper = new Mock<IChecklistContentMapper>();

            Retriever retriever = new Retriever(
                this.userMapper.Object,
                this.proposalPermissionsMapper.Object,
                this.proposalChecklistLoader.Object,
                this.pprChecklistContentMapper.Object,
                this.parChecklistConteMapper.Object);

            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever);

            return retriever;
        }

        #endregion Setup

        #region Proposal

        /// <summary>
        /// Test GetCurrentUser
        /// </summary>
        [TestMethod]
        public void O_GetCurrentUserTest()
        {
            Retriever sut = this.CreateSystem();

            UserDTO currentUser = new UserDTO()
            {
                Id = 5,
                DisplayName = "test",
                Ntid = "testntid",
                UserType = UserType.User,
                IsGroup = false
            };

            this.userMapper.Setup(x => x.GetActiveUser()).Returns(currentUser);

            UserDTO result = sut.GetCurrentUser();

            DtoAssertHelpers.AssertDtos(currentUser, result);
        }

        /// <summary>
        /// Get Proposal Permissions Test
        /// </summary>
        [TestMethod]
        public void O_GetProposalPermissions()
        {
            Retriever sut = this.CreateSystem();

            int proposalID = 515;

            ProposalPermissionDto permissions = new ProposalPermissionDto
            {
                Id = 5,
                Role = PtmRole.PeerReviewer,
                UserId = 50,
                ProposalID = proposalID
            };

            this.proposalPermissionsMapper.Setup(x => x.GetProposalPermissionsByProposalId(proposalID)).Returns(new Collection<ProposalPermissionDto> { permissions });

            ICollection<ProposalPermissionDto> result = sut.GetProposalPermissions(proposalID);

            DtoAssertHelpers.AssertDtos(result.First(), permissions);
        }
        #endregion Proposal

        #region Checklist
        /// <summary>
        /// Test get proposal checklist
        /// </summary>
        [TestMethod]
        public void O_GetProposalChecklistTest()
        {
            Retriever sut = this.CreateSystem();

            int proposalID = 44;
            List<ProposalChecklistDto> testProposalChecklists = new List<ProposalChecklistDto>
            {
                new ProposalChecklistDto()
                {
                    Id = 1,
                    ProposalID = proposalID,
                    SubmittedValue = 100
                }
            };

            this.proposalChecklistLoader.Setup(x => x.GetByProposalIds(new Collection<int> { proposalID })).Returns(testProposalChecklists);
            ICollection<ProposalChecklistDto> result = sut.GetProposalChecklists(proposalID);

            DtoAssertHelpers.AssertDtos(testProposalChecklists.First(), result.First());
        }

        /// <summary>
        /// Test get PPR checklist content
        /// </summary>
        [TestMethod]
        public void O_GetPPRChecklistContentTest()
        {
            Retriever sut = this.CreateSystem();

            int proposalID = 44;
            int checklistID = 32;
            ChecklistContentDto checklistContent = new ChecklistContentDto();
            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Review Items", TextType = ChecklistTextType.Header };
            checklistContent.Content.Add(content1);
            checklistContent.Content.Add(content2);
            checklistContent.Content.Add(content3);

            this.pprChecklistContentMapper.Setup(x => x.GetChecklistByProposalId(proposalID)).Returns(checklistContent);
            ChecklistContentDto result = sut.GetPPRChecklistContent(proposalID);

            DtoAssertHelpers.AssertDtos(checklistContent, result);
        }

        /// <summary>
        /// Test get PAR checklist content
        /// </summary>
        [TestMethod]
        public void O_GetPARChecklistContentTest()
        {
            Retriever sut = this.CreateSystem();

            int proposalID = 44;
            int checklistID = 32;
            ChecklistContentDto checklistContent = new ChecklistContentDto();
            ChecklistContentItem content1 = new ChecklistContentItem { Id = 1, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem content2 = new ChecklistContentItem { Id = 2, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem content3 = new ChecklistContentItem { Id = 3, ChecklistId = checklistID, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 3, Text = "Review Items", TextType = ChecklistTextType.Header };
            checklistContent.Content.Add(content1);
            checklistContent.Content.Add(content2);
            checklistContent.Content.Add(content3);

            this.parChecklistConteMapper.Setup(x => x.GetChecklistByProposalId(proposalID)).Returns(checklistContent);
            ChecklistContentDto result = sut.GetPARChecklistContent(proposalID);

            DtoAssertHelpers.AssertDtos(checklistContent, result);
        }

        /// <summary>
        /// Get Checklist Save Info Test
        /// </summary>
        [TestMethod]
        public void O_GetAllChecklistSaveInfo()
        {
            Retriever sut = this.CreateSystem();

            int proposalID = 515;

            ProposalChecklistSaveInfo saveEntry = new ProposalChecklistSaveInfo()
            {
                ChecklistType = ChecklistType.ProposalPricingReview,
                Comment = "comment",
                LastSaveDate = DateTime.Now,
                ResponseType = ChecklistResponseType.Pricer,
                SubmitDate = DateTime.Now,
                UserID = 3
            };

            ICollection<ProposalChecklistSaveInfo> saveInfo = new Collection<ProposalChecklistSaveInfo>() { saveEntry };

            this.proposalChecklistLoader.Setup(x => x.GetAllChecklistSaveInfo(proposalID)).Returns(saveInfo);

            ICollection<ProposalChecklistSaveInfo> result = sut.GetAllChecklistSaveInfo(proposalID);

            DtoAssertHelpers.AssertDtos(saveEntry, result.First());
        }

        #endregion Checklist
    }
}
