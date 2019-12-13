// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Loader
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test class for the Email Information Loader
    /// </summary>
    [TestClass]
    public class EmailInformationLoaderTest
    {
        /// <summary>
        /// Test Data Utility Class
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Create System
        /// </summary>
        /// <returns>Email Information Loader</returns>
        private EmailInformationLoader CreateSystem()
        {
            return new EmailInformationLoader();
        }

        /// <summary>
        /// Test for initial approval emails.
        /// </summary>
        [TestMethod]
        public void L_GetAllEmailsToBeSentTest_InitialApprover()
        {
            var sut = this.CreateSystem();

            // Set the status to Started
            ProposalDto testProposal = this.testData.GetProposal();
            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.NotStarted);

            ICollection<EmailInformationDto> emails = sut.GetAllEmailsToBeSent(null);

            Assert.IsFalse(emails.Any(e => e.ProposalId == testProposal.Id));

            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.Started);
            emails = sut.GetAllEmailsToBeSent(null);
            Assert.IsTrue(emails.Any(e => e.ProposalEmailType == EmailType.InitialApprovalEmail && e.ProposalId == testProposal.Id));
        }

        /// <summary>
        /// Test for forecast proposals
        /// </summary>
        [TestMethod]
        public void L_GetAllEmailsToBeSentTest_Forecast()
        {
            var sut = this.CreateSystem();

            // Set the anticipated delivery date to be in the past, and save it
            ProposalDto testProposal = this.testData.GetProposal(true, null, DateTime.Now.AddDays(-1), true);
            ProposalPermissionDto permission = new ProposalPermissionDto()
            {
                Id = -1,
                ProposalID = testProposal.Id,
                ResourceType = ResourceType.NotSet,
                Role = PtmRole.LOBEstLead,
                Updateable = UpdateType.Upsert,
                UserId = this.testData.GetUser().Id
            };
            this.testData.GetProposalPermission(true, permission);
            ICollection<EmailInformationDto> emails = sut.GetAllEmailsToBeSent(null);

            Assert.IsTrue(emails.Any(e => e.ProposalEmailType == EmailType.ForecastAlertEmail && e.ProposalId == testProposal.Id));
        }

        /// <summary>
        /// Test for second approval emails.
        /// </summary>
        [TestMethod]
        public void L_GetAllEmailsToBeSentTest_SecondEmailApprover()
        {
            var sut = this.CreateSystem();

            // Set the status to Started
            ProposalDto testProposal = this.testData.GetProposal();
            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.NotStarted);

            ICollection<EmailInformationDto> emails = sut.GetAllEmailsToBeSent(null);

            Assert.IsFalse(emails.Any(e => e.ProposalId == testProposal.Id));

            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.InitialApproverEmail, DateTime.Now.AddDays(-10));
            emails = sut.GetAllEmailsToBeSent(null);
            Assert.IsTrue(emails.Any(e => e.ProposalEmailType == EmailType.SecondApprovalEmail && e.ProposalId == testProposal.Id));
        }

        /// <summary>
        /// Test for final approval emails.
        /// </summary>
        [TestMethod]
        public void L_GetAllEmailsToBeSentTest_FinalEmailApprover()
        {
            var sut = this.CreateSystem();

            // Set the status to Started
            ProposalDto testProposal = this.testData.GetProposal();
            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.NotStarted);

            ICollection<EmailInformationDto> emails = sut.GetAllEmailsToBeSent(null);

            Assert.IsFalse(emails.Any(e => e.ProposalId == testProposal.Id));

            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.SecondApproverEmail, DateTime.Now.AddDays(-10));
            emails = sut.GetAllEmailsToBeSent(null);
            Assert.IsTrue(emails.Any(e => e.ProposalEmailType == EmailType.FinalApprovalEmail && e.ProposalId == testProposal.Id));
        }

        /// <summary>
        /// Test for Lead Alert for Approver emails.
        /// </summary>
        [TestMethod]
        public void L_GetAllEmailsToBeSentTest_LeadAlertForApprover()
        {
            var sut = this.CreateSystem();

            // Set the status to Started
            ProposalDto testProposal = this.testData.GetProposal();
            UserDTO user = this.testData.GetUser(inCreateNew: true);
            this.testData.GetProposalPermission(true, new ProposalPermissionDto()
            {
                ProposalID = testProposal.Id,
                Role = PtmRole.Pricer,
                Id = -1,
                Updateable = UpdateType.Upsert,
                UserId = user.Id
            });

            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.NotStarted);

            ICollection<EmailInformationDto> emails = sut.GetAllEmailsToBeSent(null);

            Assert.IsFalse(emails.Any(e => e.ProposalId == testProposal.Id));

            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.ThirdApproverEmail, DateTime.Now.AddDays(-10));
            emails = sut.GetAllEmailsToBeSent(null);
            Assert.IsTrue(emails.Any(e => e.ProposalEmailType == EmailType.LeadAlertForApprovers && e.ProposalId == testProposal.Id));
        }

        /// <summary>
        /// Test for initial approval emails.
        /// </summary>
        [TestMethod]
        public void L_GetAllEmailsToBeSentTest_InitialLOBApprover()
        {
            var sut = this.CreateSystem();

            // Set the status to Started
            ProposalDto testProposal = this.testData.GetProposal();
            UserDTO user = this.testData.GetUser(inCreateNew: true);
            this.testData.GetProposalPermission(true, new ProposalPermissionDto()
            {
                ProposalID = testProposal.Id,
                Role = PtmRole.LOBEstLead,
                Id = -1,
                Updateable = UpdateType.Upsert,
                UserId = user.Id
            });
            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.NotStarted);

            ICollection<EmailInformationDto> emails = sut.GetAllEmailsToBeSent(null);

            Assert.IsFalse(emails.Any(e => e.ProposalId == testProposal.Id));

            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.AllApproved);
            emails = sut.GetAllEmailsToBeSent(null);
            Assert.IsTrue(emails.Any(e => e.ProposalEmailType == EmailType.InitialLOBApprovalEmail && e.ProposalId == testProposal.Id));
        }

        /// <summary>
        /// Test for second approval emails.
        /// </summary>
        [TestMethod]
        public void L_GetAllEmailsToBeSentTest_SecondEmailLOBApprover()
        {
            var sut = this.CreateSystem();

            // Set the status to Started
            ProposalDto testProposal = this.testData.GetProposal();
            UserDTO user = this.testData.GetUser(inCreateNew: true);
            this.testData.GetProposalPermission(true, new ProposalPermissionDto()
            {
                ProposalID = testProposal.Id,
                Role = PtmRole.LOBEstLead,
                Id = -1,
                Updateable = UpdateType.Upsert,
                UserId = user.Id
            });
            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.NotStarted);

            ICollection<EmailInformationDto> emails = sut.GetAllEmailsToBeSent(null);

            Assert.IsFalse(emails.Any(e => e.ProposalId == testProposal.Id));

            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.InitialLOBLeadEmail, DateTime.Now.AddDays(-10));
            emails = sut.GetAllEmailsToBeSent(null);
            Assert.IsTrue(emails.Any(e => e.ProposalEmailType == EmailType.SecondLOBApprovalEmail && e.ProposalId == testProposal.Id));
        }

        /// <summary>
        /// Test for final approval emails.
        /// </summary>
        [TestMethod]
        public void L_GetAllEmailsToBeSentTest_FinalEmailLOBApprover()
        {
            var sut = this.CreateSystem();

            // Set the status to Started
            ProposalDto testProposal = this.testData.GetProposal();
            UserDTO user = this.testData.GetUser(inCreateNew: true);
            this.testData.GetProposalPermission(true, new ProposalPermissionDto()
            {
                ProposalID = testProposal.Id,
                Role = PtmRole.LOBEstLead, 
                Id = -1,
                Updateable = UpdateType.Upsert,
                UserId = user.Id
            });
            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.NotStarted);
            
            ICollection<EmailInformationDto> emails = sut.GetAllEmailsToBeSent(null);

            Assert.IsFalse(emails.Any(e => e.ProposalId == testProposal.Id));

            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.SecondLOBLeadEmail, DateTime.Now.AddDays(-10));
            emails = sut.GetAllEmailsToBeSent(null);
            Assert.IsTrue(emails.Any(e => e.ProposalEmailType == EmailType.FinalLOBApprovalEmail && e.ProposalId == testProposal.Id));
        }

        /// <summary>
        /// Test for Lead Alert for Approver emails.
        /// </summary>
        [TestMethod]
        public void L_GetAllEmailsToBeSentTest_LeadAlertForLOBApprover()
        {
            var sut = this.CreateSystem();

            // Set the status to Started
            ProposalDto testProposal = this.testData.GetProposal();
            UserDTO user = this.testData.GetUser(inCreateNew: true);
            this.testData.GetProposalPermission(true, new ProposalPermissionDto()
            {
                ProposalID = testProposal.Id,
                Role = PtmRole.Pricer,
                Id = -1,
                Updateable = UpdateType.Upsert,
                UserId = user.Id
            });
            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.NotStarted);

            ICollection<EmailInformationDto> emails = sut.GetAllEmailsToBeSent(null);

            Assert.IsFalse(emails.Any(e => e.ProposalId == testProposal.Id));

            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.ThirdLOBLeadEmail, DateTime.Now.AddDays(-10));
            emails = sut.GetAllEmailsToBeSent(null);
            Assert.IsTrue(emails.Any(e => e.ProposalEmailType == EmailType.LeadAlertForLOBApprover && e.ProposalId == testProposal.Id));
        }

        /// <summary>
        /// Test for initial approval emails.
        /// </summary>
        [TestMethod]
        public void L_GetAllEmailsToBeSentTest_CertificationTimeline()
        {
            var sut = this.CreateSystem();

            ProposalDto testProposal = this.testData.GetProposal(true);

            testProposal.ProposalStatus = ProposalStatus.Submitted;
            
            this.testData.UpdateProposal(testProposal);

            this.testData.SetWorkflowStatus(testProposal.Id, WorkflowStatus.ProposalLocked);
            ProposalPermissionDto permission = new ProposalPermissionDto
            {
                ProposalID = testProposal.Id,
                Role = PtmRole.Pricer,
                UserId = this.testData.GetUser().Id,
                Updateable = UpdateType.Upsert
            };

            this.testData.GetProposalPermission(true, permission);

            ICollection<EmailInformationDto> emails = sut.GetAllEmailsToBeSent(null);

            Assert.IsTrue(emails.Any(e => e.ProposalId == testProposal.Id && e.ProposalEmailType == EmailType.CertificationTimelineEmail));
        }
    }
}