// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Loader
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the Attachment dto data loader
    /// </summary>
    [TestClass]
    public class AttachmentLoaderTest
    {
        /// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Get All User Ids Test
        /// </summary>
        [TestMethod]
        public void L_AttachmentsTest()
        {
            var sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);
            ICollection<AttachmentDto> attachments = sut.GetAttachmentsForProposal(proposal.Id);

            Assert.AreEqual(0, attachments.Count);

            AttachmentDto dto1 = new AttachmentDto
            {
                AttachmentType = AttachmentType.DelegationOfAuthority,
                Contents = new byte[10],
                Name = "dto1",
                UploadedBy = "Tim W.",
                ProposalId = proposal.Id,
                Updateable = UpdateType.Upsert
            };

            AttachmentDto dto2 = new AttachmentDto
            {
                AttachmentType = AttachmentType.ResponsibilityAssignmentsMatrix,
                Contents = new byte[5],
                Name = "dto2",
                UploadedBy = "Timothy W.",
                ProposalId = proposal.Id,
                Updateable = UpdateType.Upsert
            };

            using (TransactionScope scope = new TransactionScope())
            {
                dto1.Id = sut.Save(dto1).Value;
                dto2.Id = sut.Save(dto2).Value;
                scope.Complete();
            }

            // Test that insertion and retrieval work
            attachments = sut.GetAttachmentsForProposal(proposal.Id);
            Assert.AreEqual(2, attachments.Count);

            this.AssertAreEqual(dto1, attachments.FirstOrDefault(a => a.Id == dto1.Id));
            this.AssertAreEqual(dto2, attachments.FirstOrDefault(a => a.Id == dto2.Id));
            byte[] firstBytes = sut.GetById(dto1.Id).Contents;
            byte[] secondBytes = sut.GetById(dto2.Id).Contents;

            Assert.AreEqual(dto1.Contents.Length, firstBytes.Length);
            Assert.AreEqual(dto2.Contents.Length, secondBytes.Length);

            dto2 = attachments.FirstOrDefault(a => a.Id == dto2.Id);

            // Test that update and retrieval work
            dto2.Contents = new byte[15];
            dto2.Name = "new name";
            dto2.UploadedBy = "second person";
            dto2.Updateable = UpdateType.Upsert;

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(dto2);
                scope.Complete();
            }

            attachments = sut.GetAttachmentsForProposal(proposal.Id);
            this.AssertAreEqual(dto2, attachments.FirstOrDefault(a => a.Id == dto2.Id));

            secondBytes = sut.GetById(dto2.Id).Contents;
            Assert.AreEqual(dto2.Contents.Length, secondBytes.Length);

            // Test that deletion works
            dto2 = attachments.FirstOrDefault(a => a.Id == dto2.Id);
            dto2.Updateable = UpdateType.Deleted;
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(dto2);
                scope.Complete();
            }
            
            attachments = sut.GetAttachmentsForProposal(proposal.Id);
            Assert.AreEqual(1, attachments.Count);
        }

        /// <summary>
        /// Tests bad proposalId
        /// </summary>
        [TestMethod]
        public void L_GetAttachments_BadProposalId()
        {
            var sut = this.CreateSystem();
            ICollection<AttachmentDto> attachments = sut.GetAttachmentsForProposal(-50);

            Assert.AreEqual(0, attachments.Count);
        }

        /// <summary>
        /// Asserts that the two attachment dtos are equal.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        private void AssertAreEqual(AttachmentDto expected, AttachmentDto actual)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.AttachmentType, actual.AttachmentType);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.UploadedBy, actual.UploadedBy);
            Assert.AreEqual(expected.ProposalId, actual.ProposalId);
            Assert.AreNotEqual(expected.UpdateDate, actual.UpdateDate);
            Assert.IsNull(actual.Contents);
        }

        /// <summary>
        /// Create the system
        /// </summary>
        /// <returns>An AttachmentLoader loader</returns>
        private AttachmentLoader CreateSystem()
        {
            return new AttachmentLoader();
        }
    }
}
