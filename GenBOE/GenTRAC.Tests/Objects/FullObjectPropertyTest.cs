// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.Objects
{
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using GenTRAC.Tests.DAL;
    using IES.Common;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test the properites in each full object
    /// </summary>
    [TestClass]
    public class FullObjectPropertyTest
    {
        /// <summary>
        /// Retriever
        /// </summary>
        private Mock<IRetriever> retriever;

        /// <summary>
        /// Automatically add a new retriever to the unity container
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.retriever = new Mock<IRetriever>();

            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), this.retriever.Object);
        }

        /// <summary>
        /// Test the proposal properties
        /// </summary>
        [TestMethod]
        public void TestFullProposal()
        {
            ProposalDto proposalDto = new ProposalDto() { Id = 4 };
            FullProposal proposal = new FullProposal(proposalDto);

            UserDTO currentUserDto = new UserDTO() { Id = 34 };
            List<ProposalPermissionDto> permissionDtos = new List<ProposalPermissionDto>()
            {
                new ProposalPermissionDto() { Role = PtmRole.CaptureManager, Id = 5, UserId = 52 },
                new ProposalPermissionDto() { Role = PtmRole.Pricer, Id = 6, UserId = 55 }
            };

            List<ProposalChecklistDto> proposalChecklists = new List<ProposalChecklistDto>()
                {
                    new ProposalChecklistDto() { ProposalID = proposalDto.Id, Id = 1 }
                };

            ChecklistContentDto checklistPPRContent = new ChecklistContentDto();
            ChecklistContentItem pprContent1 = new ChecklistContentItem { Id = 1, ChecklistId = 1, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 1, Text = "All proposal pricing data is required", TextType = ChecklistTextType.Text };
            ChecklistContentItem pprContent2 = new ChecklistContentItem { Id = 2, ChecklistId = 1, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 2, Text = "Did you do this?", TextType = ChecklistTextType.Question };
            ChecklistContentItem pprContent3 = new ChecklistContentItem { Id = 3, ChecklistId = 1, ChecklistType = ChecklistType.ProposalPricingReview, SortOrder = 3, Text = "Review Items", TextType = ChecklistTextType.Header };
            checklistPPRContent.Content.Add(pprContent1);
            checklistPPRContent.Content.Add(pprContent2);
            checklistPPRContent.Content.Add(pprContent3);

            ChecklistContentDto checklistPARContent = new ChecklistContentDto();
            ChecklistContentItem parContent4 = new ChecklistContentItem { Id = 4, ChecklistId = 1, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 1, Text = "This is a test", TextType = ChecklistTextType.Text };
            ChecklistContentItem parContent5 = new ChecklistContentItem { Id = 5, ChecklistId = 1, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 2, Text = "Did you do this on purpose?", TextType = ChecklistTextType.Question };
            ChecklistContentItem parContent6 = new ChecklistContentItem { Id = 6, ChecklistId = 1, ChecklistType = ChecklistType.ProposalAdequacyReview, SortOrder = 3, Text = "Header", TextType = ChecklistTextType.Header };
            checklistPARContent.Content.Add(parContent4);
            checklistPARContent.Content.Add(parContent5);
            checklistPARContent.Content.Add(parContent6);

            this.retriever.Setup(x => x.GetCurrentUser()).Returns(currentUserDto);
            this.retriever.Setup(x => x.GetProposalPermissions(proposal.Id)).Returns(permissionDtos);
            this.retriever.Setup(x => x.GetProposalChecklists(proposal.Id)).Returns(proposalChecklists);
            this.retriever.Setup(x => x.GetPPRChecklistContent(proposal.Id)).Returns(checklistPPRContent);
            this.retriever.Setup(x => x.GetPARChecklistContent(proposal.Id)).Returns(checklistPARContent);

            var currentUser = proposal.CurrentUser;
            currentUser = proposal.CurrentUser;

            var permissions = proposal.Permissions;
            permissions = proposal.Permissions;

            DtoAssertHelpers.AssertDtos(currentUserDto, currentUser);

            for (int i = 0; i < permissions.Count; i++)
            {
                DtoAssertHelpers.AssertDtos(permissionDtos[i], permissions.ElementAt(i));
            }

            // checklist tests
            var checklists = proposal.ProposalChecklistData;
            checklists = proposal.ProposalChecklistData;

            for (int i = 0; i < checklists.Count; i++)
            {
                DtoAssertHelpers.AssertDtos(proposalChecklists[i], checklists.ElementAt(i));
            }

            var pprChecklistData = proposal.ProposalChecklistPPRData;
            pprChecklistData = proposal.ProposalChecklistPPRData;

            DtoAssertHelpers.AssertDtos(checklistPPRContent, pprChecklistData);

            var parChecklistData = proposal.ProposalChecklistPARData;
            parChecklistData = proposal.ProposalChecklistPARData;

            DtoAssertHelpers.AssertDtos(checklistPARContent, parChecklistData);

            this.retriever.Verify(x => x.GetCurrentUser(), Times.Once());
            this.retriever.Verify(x => x.GetProposalPermissions(proposal.Id), Times.Once());
            this.retriever.Verify(x => x.GetProposalChecklists(proposal.Id), Times.Once());
        }
    }
}
