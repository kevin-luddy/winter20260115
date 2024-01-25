// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.ActionLogic
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Web.Common;
    using IES.Common;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests PostSubmittalAttachment Controller Logic
    /// </summary>
    [TestClass]
    public class PostSubmittalAttachmentControllerLogicTest
    {
        #region Properties

        /// <summary>
        /// Security Access
        /// </summary>
        private Mock<ISecurityAccess> securityAccess;

        /// <summary>
        /// Proposal Loader
        /// </summary>
        private Mock<IProposalLoader> proposalLoader;

        /// <summary>
        /// User Mapper
        /// </summary>
        private Mock<IUserMapper> userMapper;

        /// <summary>
        /// Object Factory
        /// </summary>
        private Mock<IFullObjectFactory> objectFactory;

        /// <summary>
        /// Proposal Mediator
        /// </summary>
        private Mock<IProposalMediator> proposalMediator;

        /// <summary>
        /// Proposal Checklist Loader
        /// </summary>
        private Mock<IProposalChecklistLoader> proposalChecklistLoader;

        /// <summary>
        /// Checklist Mediator
        /// </summary>
        private Mock<IChecklistMediator> checklistMediator;

        /// <summary>
        /// Retriever
        /// </summary>
        private Mock<IRetriever> retriever;

        /// <summary>
        /// Approvals Loader
        /// </summary>
        private Mock<ApprovalsLoader> approvalsLoader;

        /// <summary>
        /// The approval emailer.
        /// </summary>
        private Mock<IAttachmentLoader> attachmentLoader = null;

        /// <summary>
        /// Creates ApprovalsControllerLogic for testing
        /// </summary>
        /// <returns>ApprovalsControllerLogic</returns>
        private PostSubmittalAttachmentsControllerLogic CreateSystem()
        {
            this.securityAccess = new Mock<ISecurityAccess>();
            this.proposalLoader = new Mock<IProposalLoader>();
            this.userMapper = new Mock<IUserMapper>();
            this.objectFactory = new Mock<IFullObjectFactory>();
            this.approvalsLoader = new Mock<ApprovalsLoader>();
            this.proposalMediator = new Mock<IProposalMediator>();
            this.proposalChecklistLoader = new Mock<IProposalChecklistLoader>();
            this.checklistMediator = new Mock<IChecklistMediator>();
            this.attachmentLoader = new Mock<IAttachmentLoader>();

            this.retriever = new Mock<IRetriever>();
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);

            return new PostSubmittalAttachmentsControllerLogic(this.securityAccess.Object, this.proposalLoader.Object, this.userMapper.Object, this.objectFactory.Object, this.approvalsLoader.Object, this.proposalChecklistLoader.Object, this.checklistMediator.Object, this.proposalMediator.Object, this.attachmentLoader.Object);
        }

        /// <summary>
        /// Creates the specified number of 'Other' attachments and sets the attachmentLoader up to return them.
        /// </summary>
        /// <param name="numAttachmentsToCreate">The number of attachments to create.</param>
        private void CreateOtherAttachments(int? numAttachmentsToCreate)
        {
            Collection<AttachmentDto> psaFiles = new Collection<AttachmentDto>();

            for (int i = 0; i < numAttachmentsToCreate; i++)
            {
                psaFiles.Add(new AttachmentDto() { AttachmentType = AttachmentType.Other, Id = i });
            }

            this.attachmentLoader.Setup(x => x.GetAttachmentsForProposal(It.IsAny<int>())).Returns(psaFiles);
        }

        #endregion

        /// <summary>
        /// Test that 3 distinct attachments are always returned
        /// </summary>
        [TestMethod]
        public void GetPostSubmittalAttachments_Test()
        {
            PostSubmittalAttachmentsControllerLogic sut = this.CreateSystem();

            this.attachmentLoader.Setup(x => x.GetAttachmentsForProposal(It.IsAny<int>())).Returns(new Collection<AttachmentDto>());

            ICollection<AttachmentDto> attachments = sut.GetPostSubmittalAttachments(1);

            // Confirm that method returns all 3 distinct attachment types:
            // CostKickOffPackage, ResponsibilityAssignmentsMatrix, DelegationOfAuthority
            Assert.AreEqual(3, attachments.GroupBy(g => g.AttachmentType).Select(a => a.First()).Count(),
                "GetPostSubmittalAttachments should always return all 3 distinct AttachmentTypes");

            this.attachmentLoader.Setup(x => x.GetAttachmentsForProposal(It.IsAny<int>()))
                .Returns(new Collection<AttachmentDto>
                            {
                                new AttachmentDto
                                {
                                    AttachmentType = AttachmentType.CostKickOffPackage
                                }
                            });

            attachments = sut.GetPostSubmittalAttachments(1);

            Assert.AreEqual(3, attachments.GroupBy(g => g.AttachmentType).Select(a => a.First()).Count(),
                "GetPostSubmittalAttachments should always return all 3 distinct AttachmentTypes");
        }

        /// <summary>
        /// Test file validation
        /// </summary>
        [TestMethod]
        public void ValidateFileTypeAndSize_Test()
        {
            PostSubmittalAttachmentsControllerLogic sut = this.CreateSystem();

            Mock<HttpPostedFileBase> mockFile = new Mock<HttpPostedFileBase>();
            mockFile.Setup(x => x.FileName).Returns("UnallowableFileType.exe");
            mockFile.Setup(x => x.ContentLength).Returns(SiteMasterUtilities.MaxFileSize.Value + 1);

            ICollection<string> errors = sut.ValidateFileTypeAndSize(mockFile.Object, SiteMasterUtilities.AllowedFileTypes, SiteMasterUtilities.MaxFileSize);

            Assert.AreEqual(2, errors.Count, 
                "File is larger than MaxFileSize and the extension is not in AllowedFileTypes 2 errors should be returned");
        }

        /// <summary>
        /// Test file count validation by exceeding the maximum number of allowed 'Other' files.
        /// </summary>
        [TestMethod]
        public void ValidateOtherFileCount_Test()
        {
            PostSubmittalAttachmentsControllerLogic sut = this.CreateSystem();
            this.CreateOtherAttachments(SiteMasterUtilities.MaxOtherFileCount);

            ICollection<string> errors = sut.ValidateOtherFileCount(1, new AttachmentDto() { AttachmentType = AttachmentType.Other, Id = -1 }, SiteMasterUtilities.MaxOtherFileCount);

            Assert.AreEqual(1, errors.Count, "MaxOtherFileCount exceeded, but validation did not fail for the new 'Other' attachment.");
        }

        /// <summary>
        /// Test file count validation wihtout exceeding the maximum number of allowed 'Other' files.
        /// </summary>
        [TestMethod]
        public void ValidateOtherFileCountNotMaxNumber_Test()
        {
            PostSubmittalAttachmentsControllerLogic sut = this.CreateSystem();
            this.CreateOtherAttachments(SiteMasterUtilities.MaxOtherFileCount - 1);

            ICollection<string> errors = sut.ValidateOtherFileCount(1, new AttachmentDto() { AttachmentType = AttachmentType.Other, Id = -1 }, SiteMasterUtilities.MaxOtherFileCount);

            Assert.AreEqual(0, errors.Count, "Validation failed for a new 'Other' file without exceeding the MaxOtherFileCount.");
        }

        /// <summary>
        /// Test that the file count validation does not apply to standard file types.
        /// </summary>
        [TestMethod]
        public void ValidateOtherFileCountStandardFile_Test()
        {
            PostSubmittalAttachmentsControllerLogic sut = this.CreateSystem();
            this.CreateOtherAttachments(SiteMasterUtilities.MaxOtherFileCount);

            ICollection<string> errors = sut.ValidateOtherFileCount(1, new AttachmentDto() { AttachmentType = AttachmentType.CostKickOffPackage, Id = -1 }, SiteMasterUtilities.MaxOtherFileCount);

            Assert.AreEqual(0, errors.Count, "Validation failed for a standard attachment type due to exceeding the MaxOtherFileCount.  Validation should only fail for this reason for files with a type of 'Other'.");
        }
    }
}
