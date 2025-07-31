// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Tests.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
	using System.Threading.Tasks;
	using System.Web;
	using IES.ActionLogic.Core.ControllerLogic;
	using IES.ActionLogic.Core.Mediator;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Security;
	using IES.Common.Core;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
	using IES.ActionLogic.Core.IO.Export;
	using IES.Common.Core.Models;

	/// <summary>
	/// Test ReportsControllerLogic
	/// </summary>
	[TestClass]
    public class ReportsControllerLogicTest
    {
        /// <summary>
        /// PPRD Exporter
        /// </summary>
        private Mock<IPPRDExporter> pprdExporter;

        /// <summary>
        /// RDM Revision Exporter
        /// </summary>
        private Mock<IRdmRevisionExporter> rdmRevisionExporter;

        /// <summary>
        /// Rate Detail Loader
        /// </summary>
        private Mock<IRateDetailLoader> rateDetailLoader;

        /// <summary>
        /// COBRA Detail Loader
        /// </summary>
        private Mock<ICobraDetailLoader> cobraDetailLoader;

        /// <summary>
        /// The section loader
        /// </summary>
        private Mock<ISectionLoader> sectionLoader;

        /// <summary>
        /// The file attachment loader
        /// </summary>
        private Mock<IFileAttachmentLoader> fileAttachmentLoader;

        /// <summary>
        /// The burden pool loader
        /// </summary>
        private Mock<IBurdenPoolLoader> burdenPoolLoader;

        /// <summary>
        /// Mock Revision Loader
        /// </summary>
        private Mock<IRevisionMediator> revisionMediator = new();

        /// <summary>
        /// The area locking loader
        /// </summary>
        private Mock<IAreaLockingLoader> areaLockingLoader = new();

        /// <summary>
        /// AD Utils
        /// </summary>
        private Mock<IActiveDirectoryService> adUtils = new();

        /// <summary>
        /// Security Info
        /// </summary>
        private Mock<SecurityInformation> securityInfo;

        [TestInitialize]
        public void Init()
        {
            this.securityInfo = new Mock<SecurityInformation>(this.adUtils.Object, null, null);
            this.pprdExporter = new Mock<IPPRDExporter>();
            this.rdmRevisionExporter = new Mock<IRdmRevisionExporter>();
            this.revisionMediator = new Mock<IRevisionMediator>();
            this.rateDetailLoader = new Mock<IRateDetailLoader>();
            this.cobraDetailLoader = new Mock<ICobraDetailLoader>();
            this.sectionLoader = new Mock<ISectionLoader>();
            this.fileAttachmentLoader = new Mock<IFileAttachmentLoader>();
            this.burdenPoolLoader = new Mock<IBurdenPoolLoader>();
        }

        /// <summary>
        /// Create sut
        /// </summary>
        /// <returns>sut</returns>
        private ReportsControllerLogic CreateSut()
        {
            return new ReportsControllerLogic(this.pprdExporter.Object, this.rdmRevisionExporter.Object, this.rateDetailLoader.Object, this.cobraDetailLoader.Object, this.sectionLoader.Object, this.fileAttachmentLoader.Object,
                this.burdenPoolLoader.Object, this.areaLockingLoader.Object, this.revisionMediator.Object, this.adUtils.Object, this.securityInfo.Object, null);
        }

        /// <summary>
        /// Test GenerateFullPPRD for the WIP revision
        /// </summary>
        [TestMethod]
        public async Task TestGenerateFullPPRD_WIP()
        {
            ReportsControllerLogic sut = this.CreateSut();

            string id = "WIP";
            bool portionMarkingRequired = false;
            string serverFileName = "TestFile";
            
            RevisionModelView wipRevision = new()
            {
                CreatedBy = "test user",
                DateCreated = DateTime.Now.AddDays(-1),
                DatePublished = null,
                History = "test revision",
                Dirty = false,
                EndYear = 2018,
                Id = 1,
                PublishedBy = "test user2",
                Revision = "1",
                StartYear = 2017,
                ReleaseNotes = "Test Notes"
            };

            this.revisionMediator.Setup(x => x.GetWipRevision()).Returns(wipRevision);

            this.rateDetailLoader.Setup(x => x.GetRatesByRevision(wipRevision))
                .Returns(new Collection<RateDetailModelView>());

            this.sectionLoader.Setup(x => x.GetAll(wipRevision, false, It.IsAny<bool>(), null, null)).Returns(new Collection<SectionModelView>());

            await sut.GenerateFullPPRD(id, serverFileName, portionMarkingRequired);

            this.pprdExporter.Verify(x => x.ExportFullPPRDToWordFile(It.IsAny<ICollection<SectionModelView>>(), It.IsAny<ICollection<RateDetailModelView>>(), It.IsAny<ICollection<FileAttachmentRowModelView>>(), serverFileName, It.IsAny<string>(), wipRevision, It.IsAny<int>(), It.IsAny<int>(), portionMarkingRequired), Times.Exactly(1));
        }

        /// <summary>
        /// Test GenerateFullPPRD for a previous revision
        /// </summary>
        [TestMethod]
        public async Task TestGenerateFullPPRD_PreviousRevision()
        {
            ReportsControllerLogic sut = this.CreateSut();

            string id = "1";
            bool portionMarkingRequired = false;
            string serverFileName = "TestFile";
            
            RevisionModelView previousRevision = new()
            {
                CreatedBy = "test user",
                DateCreated = DateTime.Now.AddDays(-1),
                DatePublished = DateTime.Now,
                History = "test revision",
                Dirty = false,
                EndYear = 2018,
                Id = 1,
                PublishedBy = "test user2",
                Revision = "1",
                StartYear = 2017,
                ReleaseNotes = "Test Notes"
            };

            this.revisionMediator.Setup(x => x.GetById(1)).Returns(previousRevision);

            this.rateDetailLoader.Setup(x => x.GetRatesByRevision(previousRevision))
                .Returns(new Collection<RateDetailModelView>());

            this.sectionLoader.Setup(x => x.GetAll(previousRevision, false, It.IsAny<bool>(), null, null)).Returns(new Collection<SectionModelView>());

            await sut.GenerateFullPPRD(id, serverFileName, portionMarkingRequired);

            this.pprdExporter.Verify(x => x.ExportFullPPRDToWordFile(It.IsAny<ICollection<SectionModelView>>(), It.IsAny<ICollection<RateDetailModelView>>(), It.IsAny<ICollection<FileAttachmentRowModelView>>(), serverFileName, It.IsAny<string>(), previousRevision, It.IsAny<int>(), It.IsAny<int>(), portionMarkingRequired), Times.Exactly(1));
        }

        /// <summary>
        /// Test GenerateFullPPRD for an invalid Id string
        /// </summary>
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public async Task TestGenerateFullPPRD_InvalidId()
        {
            ReportsControllerLogic sut = this.CreateSut();

            string id = "invalid";
            bool portionMarkingRequired = false;
            string serverFileName = "TestFile";
            
            await sut.GenerateFullPPRD(id, serverFileName, portionMarkingRequired);
        }

        /// <summary>
        /// Test GenerateFullPPRD for a null Id
        /// </summary>
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public async Task TestGenerateFullPPRD_NullId()
        {
            ReportsControllerLogic sut = this.CreateSut();

            bool portionMarkingRequired = false;
            string serverFileName = "TestFile";
            
            await sut.GenerateFullPPRD(null, serverFileName, portionMarkingRequired);
        }

        /// <summary>
        /// Test ExportRevisionAsJson for a revision
        /// </summary>
        [TestMethod]
        public void TestExportRevisionAsJson()
        {
            ReportsControllerLogic sut = this.CreateSut();

            string id = "1";
            string serverFileName = "TestFile.txt";
			File.WriteAllText(serverFileName, "");
            
            RevisionModelView revision = new()
            {
                CreatedBy = "test user",
                DateCreated = DateTime.Now.AddDays(-1),
                DatePublished = DateTime.Now,
                History = "test revision",
                Dirty = false,
                EndYear = 2018,
                Id = 1,
                PublishedBy = "test user2",
                Revision = "1",
                StartYear = 2017,
                ReleaseNotes = "Test Notes"
            };

            UserData userData = new () { DisplayName = "Test User", Ntid = "test1" };
            this.securityInfo.Setup(x => x.ActiveUserNTID).Returns(userData.Ntid);
            this.adUtils.Setup(x => x.GetUserByQualifiedAccount(userData.Ntid, false)).Returns(userData);
            this.revisionMediator.Setup(x => x.GetById(1)).Returns(revision);
            this.sectionLoader.Setup(x => x.GetAll(revision, false, It.IsAny<bool>(), null, It.IsAny<string>())).Returns(new Collection<SectionModelView>());
            this.rateDetailLoader.Setup(x => x.GetRatesByRevision(revision)).Returns(new Collection<RateDetailModelView>());
            this.cobraDetailLoader.Setup(x => x.GetCobraDetailsByRevision(revision)).Returns(new Collection<CobraDetailModelView>());
            this.burdenPoolLoader.Setup(x => x.GetByRevision(revision.Id, It.IsAny<bool>())).Returns(new BurdenPoolGridModelView());

            sut.ExportRevisionAsJson(id, serverFileName);

            this.rdmRevisionExporter.Verify(x => x.ExportRevisionAsJson(It.IsAny<string>(), revision, It.IsAny<ICollection<SectionModelView>>(), It.IsAny<ICollection<RateDetailModelView>>(), It.IsAny<ICollection<CobraDetailModelView>>(), It.IsAny<BurdenPoolGridModelView>(), It.IsAny<string>()), Times.Exactly(1));
        }

        /// <summary>
        /// Test ExportRevisionAsJson for an invalid Id string
        /// </summary>
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestExportRevisionAsJson_InvalidId()
        {
            ReportsControllerLogic sut = this.CreateSut();

            string id = "invalid";
            string serverFileName = "TestFile";
            
            sut.ExportRevisionAsJson(id, serverFileName);
        }

        /// <summary>
        /// Test ExportRevisionAsJson for a null Id
        /// </summary>
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void TestExportRevisionAsJson_NullId()
        {
            ReportsControllerLogic sut = this.CreateSut();

            string serverFileName = "TestFile";
            
            sut.ExportRevisionAsJson(null, serverFileName);
        }
    }
}
