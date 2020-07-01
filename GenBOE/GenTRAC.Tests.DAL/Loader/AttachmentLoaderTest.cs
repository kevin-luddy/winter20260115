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
                IsRevisionReference = false,
                Updateable = UpdateType.Upsert
            };

            AttachmentDto dto2 = new AttachmentDto
            {
                AttachmentType = AttachmentType.ResponsibilityAssignmentsMatrix,
                Contents = new byte[5],
                Name = "dto2",
                UploadedBy = "Timothy W.",
                ProposalId = proposal.Id,
                IsRevisionReference = false,
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

            // testing revision handling
            ProposalDto proposal2 = this.testData.GetProposal(true);
            dto1.IsRevisionReference = true;
            dto1.ProposalId = proposal2.Id;

            using (TransactionScope scope = new TransactionScope())
            {
                dto1.Id = sut.Save(dto1).Value;
                scope.Complete();
            }

            attachments = sut.GetAttachmentsForProposal(proposal.Id);
        }

        /// <summary>
        /// Tests revisioning and attachments
        /// </summary>
        [TestMethod]
        public void L_AttachmentsTestRevision()
        {
            var sut = this.CreateSystem();

            ProposalDto proposal1 = this.testData.GetProposal(true);
            AttachmentDto dto1 = new AttachmentDto
            {
                AttachmentType = AttachmentType.DelegationOfAuthority,
                Contents = new byte[10],
                Name = "dto1",
                UploadedBy = "Tim W.",
                ProposalId = proposal1.Id,
                IsRevisionReference = false,
                Updateable = UpdateType.Upsert
            };

            using (TransactionScope scope = new TransactionScope())
            {
                dto1.Id = sut.Save(dto1).Value;
                scope.Complete();
            }

            dto1.UpdateDate = sut.GetAttachmentsForProposal(proposal1.Id).ToList()[0].UpdateDate;

            ProposalDto proposal2 = this.testData.GetProposal(true);
            AttachmentDto dto2 = new AttachmentDto
            {
                Id = dto1.Id,
                AttachmentType = dto1.AttachmentType,
                Contents = dto1.Contents,
                Name = dto1.Name,
                UploadedBy = dto1.UploadedBy,
                ProposalId = proposal2.Id,
                IsRevisionReference = true,
                Updateable = UpdateType.Upsert
            };

            using (TransactionScope scope = new TransactionScope())
            {
                dto2.Id = sut.Save(dto2).Value;
                scope.Complete();
            }

            dto2.UpdateDate = sut.GetAttachmentsForProposal(proposal2.Id).ToList()[0].UpdateDate;

            // revision link created
            List<AttachmentDto> attachments = sut.GetAttachmentsForProposal(proposal2.Id).ToList();
            Assert.AreEqual(1, attachments.Count);
            this.AssertAreEqual(dto2, attachments[0]);

            // verify that the original is still the same
            attachments = sut.GetAttachmentsForProposal(proposal1.Id).ToList();
            Assert.AreEqual(1, attachments.Count);
            this.AssertAreEqual(dto1, attachments[0]);

            // modify underlying file
            dto1.Name = "NEW NAME";
            dto2.Name = dto1.Name;
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(dto1);
                scope.Complete();
            }

            dto1.UpdateDate = sut.GetAttachmentsForProposal(proposal1.Id).ToList()[0].UpdateDate;
            dto2.UpdateDate = dto1.UpdateDate;

            // check that things look correct
            attachments = sut.GetAttachmentsForProposal(proposal1.Id).ToList();
            Assert.AreEqual(1, attachments.Count);
            this.AssertAreEqual(dto1, attachments[0]);

            attachments = sut.GetAttachmentsForProposal(proposal2.Id).ToList();
            Assert.AreEqual(1, attachments.Count);
            this.AssertAreEqual(dto2, attachments[0]);

            // delete the reference
            dto2.Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(dto2);
                scope.Complete();
            }

            // verify revision was deleted
            attachments = sut.GetAttachmentsForProposal(proposal2.Id).ToList();
            Assert.AreEqual(0, attachments.Count);

            // verify that the original is still the same
            attachments = sut.GetAttachmentsForProposal(proposal1.Id).ToList();
            Assert.AreEqual(1, attachments.Count);
            this.AssertAreEqual(dto1, attachments[0]);

            // delete the full attachment
            // delete the reference
            dto1.Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(dto1);
                scope.Complete();
            }

            // verify all was deleted
            attachments = sut.GetAttachmentsForProposal(proposal1.Id).ToList();
            Assert.AreEqual(0, attachments.Count);
            attachments = sut.GetAttachmentsForProposal(proposal2.Id).ToList();
            Assert.AreEqual(0, attachments.Count);
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
        /// Test AllRequiredAttachmentsHaveBeenUploaded
        /// </summary>
        [TestMethod]
        public void L_AllRequiredAttachmentsHaveBeenUploaded()
        {
            AttachmentLoader sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);

            bool result = sut.AllRequiredAttachmentsHaveBeenUploaded(proposal.Id);

            // Assert is false when no attachements are added
            Assert.IsFalse(result);

            // Add the required attachments
            AttachmentDto dto1 = new AttachmentDto
            {
                AttachmentType = AttachmentType.CostKickOffPackage,
                Contents = new byte[10],
                Name = "dto1",
                UploadedBy = "test",
                ProposalId = proposal.Id,
                IsRevisionReference = false,
                Updateable = UpdateType.Upsert                
            };

            AttachmentDto dto2 = new AttachmentDto
            {
                AttachmentType = AttachmentType.ResponsibilityAssignmentsMatrix,
                Contents = new byte[5],
                Name = "dto2",
                UploadedBy = "test",
                ProposalId = proposal.Id,
                IsRevisionReference = false,
                Updateable = UpdateType.Upsert
            };

            using (TransactionScope scope = new TransactionScope())
            {
                dto1.Id = sut.Save(dto1).Value;
                dto2.Id = sut.Save(dto2).Value;
                scope.Complete();
            }

            result = sut.AllRequiredAttachmentsHaveBeenUploaded(proposal.Id);

            // Assert is true when required attachements are added
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test OptionalAttachmentHasBeenUploaded
        /// </summary>
        [TestMethod]
        public void L_OptionalAttachmentHasBeenUploaded()
        {
            AttachmentLoader sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);

            bool result = sut.OptionalAttachmentHasBeenUploaded(proposal.Id);

            // Assert is false when attachement is not added
            Assert.IsFalse(result);

            // Add the optional attachment
            AttachmentDto dto1 = new AttachmentDto
            {
                AttachmentType = AttachmentType.DelegationOfAuthority,
                Contents = new byte[10],
                Name = "dto1",
                UploadedBy = "test",
                ProposalId = proposal.Id,
                IsRevisionReference = false,
                Updateable = UpdateType.Upsert
            };
            
            using (TransactionScope scope = new TransactionScope())
            {
                dto1.Id = sut.Save(dto1).Value;
                scope.Complete();
            }

            result = sut.OptionalAttachmentHasBeenUploaded(proposal.Id);

            // Assert is true when required attachements are added
            Assert.IsTrue(result);
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
            Assert.AreEqual(expected.IsRevisionReference, actual.IsRevisionReference);
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
