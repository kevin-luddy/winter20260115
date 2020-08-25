// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Loader
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test class for the Manage Proposal Info Loader
    /// </summary>
    [TestClass]
    public class ManageProposalInfoLoaderTest
    {
        /// <summary>
        /// Test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Create System
        /// </summary>
        /// <returns>Manage Proposal Info Loader</returns>
        private IManageProposalInfoLoader CreateSystem()
        {
            return new ManageProposalInfoLoader();
        }

        /// <summary>
        /// Save Proposal Info
        /// </summary>
        [TestMethod]
        public void L_SaveProposalInfo()
        {
            var sut = this.CreateSystem();

            IProposalLoader proposalLoader = new ProposalLoader();
            IProposalChecklistLoader proposalChecklistLoader = new ProposalChecklistLoader();

            ProposalDto proposal = this.testData.GetProposal(true);

            // proposal initially InProgress
            Assert.AreEqual(ProposalStatus.InProgress, proposal.ProposalStatus);

            string comments = "Test archived";

            // change proposal to Archived
            ManageProposalInfoDto manageProposalInfo = new ManageProposalInfoDto()
            {
                ProposalId = proposal.Id,
                UpdateDate = proposal.UpdateDate,
                NewProposalStatus = ProposalStatus.Archived,
                Comments = comments
            };

            int? resultProposalId = sut.SaveProposalInfo(manageProposalInfo);
            Assert.IsTrue(resultProposalId.HasValue);

            proposal = proposalLoader.GetById(resultProposalId.Value);
            Assert.AreEqual(ProposalStatus.Archived, proposal.ProposalStatus);
            Assert.AreEqual(comments, proposal.ManageProposalInfoComments);

            comments = "Test deleted";

            // change proposal to Deleted
            manageProposalInfo = new ManageProposalInfoDto()
            {
                ProposalId = proposal.Id,
                UpdateDate = proposal.UpdateDate,
                NewProposalStatus = ProposalStatus.Deleted,
                Comments = comments
            };

            resultProposalId = sut.SaveProposalInfo(manageProposalInfo);
            Assert.IsTrue(resultProposalId.HasValue);

            proposal = proposalLoader.GetById(resultProposalId.Value);
            Assert.AreEqual(ProposalStatus.Deleted, proposal.ProposalStatus);
            Assert.AreEqual(comments, proposal.ManageProposalInfoComments);

            // setup complete checklist
            ProposalChecklistDto checklist = this.testData.SaveChecklistAsPricer(proposal.Id);
            checklist.IsSubmit = true;
            checklist.Updateable = UpdateType.Upsert;
            checklist.ResponseType = ChecklistResponseType.Pricer;
            this.testData.SaveChecklistAsPricer(proposal.Id, checklist);
            checklist = this.testData.SaveChecklistAsPeer(proposal.Id);
            checklist.IsSubmit = true;
            checklist.Updateable = UpdateType.Upsert;
            checklist.ResponseType = ChecklistResponseType.Peer;
            checklist = this.testData.SaveChecklistAsPeer(proposal.Id, checklist);
            this.testData.SetProposalStatus(proposal.Id, ProposalStatus.Completed);

            proposal = proposalLoader.GetById(resultProposalId.Value);
            Assert.AreEqual(ProposalStatus.Completed, proposal.ProposalStatus);

            comments = "Test completed";

            // change fields on Completed proposal
            manageProposalInfo = new ManageProposalInfoDto()
            {
                ProposalId = proposal.Id,
                UpdateDate = proposal.UpdateDate,
                NewProposalStatus = ProposalStatus.Completed,
                TotalPrice = 555555,
                ProposalSubmittalDate = new DateTime(2015, 1, 1),
                ChecklistSubmittedDatePricer = new DateTime(2015, 2, 2),
                ChecklistSubmittedDatePeer = new DateTime(2015, 3, 3),
                Comments = comments
            };

            resultProposalId = sut.SaveProposalInfo(manageProposalInfo);
            Assert.IsTrue(resultProposalId.HasValue);

            proposal = proposalLoader.GetById(resultProposalId.Value);
            Assert.AreEqual(ProposalStatus.Completed, proposal.ProposalStatus);
            Assert.AreEqual(comments, proposal.ManageProposalInfoComments);

            checklist = proposalChecklistLoader.GetByProposalIds(new Collection<int>() { proposal.Id }).FirstOrDefault();
            Assert.AreEqual(manageProposalInfo.TotalPrice, checklist.SubmittedValue);
            Assert.AreEqual(manageProposalInfo.ProposalSubmittalDate, checklist.ProposalSubmittalDate);

            ICollection<ProposalChecklistSaveInfo> saveInfo = proposalChecklistLoader.GetAllChecklistSaveInfo(proposal.Id);
            saveInfo.Where(x => x.ResponseType == ChecklistResponseType.Pricer).ToList().ForEach(x => Assert.AreEqual(manageProposalInfo.ChecklistSubmittedDatePricer, x.SubmitDate));
            saveInfo.Where(x => x.ResponseType == ChecklistResponseType.Peer).ToList().ForEach(x => Assert.AreEqual(manageProposalInfo.ChecklistSubmittedDatePeer, x.SubmitDate));
        }
    }
}
