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
    using System.Web;
    using IES.ActionLogic.ControllerLogic;
    using IES.ActionLogic.IO.Export;
    using IES.ActionLogic.Mediator;
    using IES.Common;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

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
        private Mock<IRevisionMediator> revisionMediator = new Mock<IRevisionMediator>();

        /// <summary>
        /// The area locking loader
        /// </summary>
        private Mock<IAreaLockingLoader> areaLockingLoader = new Mock<IAreaLockingLoader>();

        /// <summary>
        /// AD Utils
        /// </summary>
        private Mock<ActiveDirectoryUtilities> adUtils = new Mock<ActiveDirectoryUtilities>();

        /// <summary>
        /// Security Info
        /// </summary>
        private Mock<SecurityInformation> securityInfo;

        [TestInitialize]
        public void Init()
        {
            this.securityInfo = new Mock<SecurityInformation>(this.adUtils.Object, null);
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
                this.burdenPoolLoader.Object, this.areaLockingLoader.Object, this.revisionMediator.Object, this.adUtils.Object, this.securityInfo.Object);
        }

        /// <summary>
        /// Test GenerateFullPPRD for the WIP revision
        /// </summary>
        [TestMethod]
        public void TestGenerateFullPPRD_WIP()
        {
            ReportsControllerLogic sut = this.CreateSut();

            string id = "WIP";
            bool portionMarkingRequired = false;
            string serverFileName = "TestFile";
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();

            RevisionModelView wipRevision = new RevisionModelView()
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

            this.sectionLoader.Setup(x => x.GetAll(wipRevision, false, null, null)).Returns(new Collection<SectionModelView>());

            sut.GenerateFullPPRD(id, serverFileName, httpResponse.Object, portionMarkingRequired);

            this.pprdExporter.Verify(x => x.ExportFullPPRDToWordFile(It.IsAny<ICollection<SectionModelView>>(), It.IsAny<ICollection<RateDetailModelView>>(), It.IsAny<ICollection<FileAttachmentRowModelView>>(), serverFileName, It.IsAny<string>(), wipRevision, It.IsAny<int>(), httpResponse.Object, It.IsAny<int>()), Times.Exactly(1));
        }

        /// <summary>
        /// Test GenerateFullPPRD for a previous revision
        /// </summary>
        [TestMethod]
        public void TestGenerateFullPPRD_PreviousRevision()
        {
            ReportsControllerLogic sut = this.CreateSut();

            string id = "1";
            bool portionMarkingRequired = false;
            string serverFileName = "TestFile";
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();

            RevisionModelView previousRevision = new RevisionModelView()
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

            this.sectionLoader.Setup(x => x.GetAll(previousRevision, false, null, null)).Returns(new Collection<SectionModelView>());

            sut.GenerateFullPPRD(id, serverFileName, httpResponse.Object, portionMarkingRequired);

            this.pprdExporter.Verify(x => x.ExportFullPPRDToWordFile(It.IsAny<ICollection<SectionModelView>>(), It.IsAny<ICollection<RateDetailModelView>>(), It.IsAny<ICollection<FileAttachmentRowModelView>>(), serverFileName, It.IsAny<string>(), previousRevision, It.IsAny<int>(), httpResponse.Object, It.IsAny<int>()), Times.Exactly(1));
        }

        /// <summary>
        /// Test GenerateFullPPRD for an invalid Id string
        /// </summary>
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestGenerateFullPPRD_InvalidId()
        {
            ReportsControllerLogic sut = this.CreateSut();

            string id = "invalid";
            bool portionMarkingRequired = false;
            string serverFileName = "TestFile";
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();

            sut.GenerateFullPPRD(id, serverFileName, httpResponse.Object, portionMarkingRequired);
        }

        /// <summary>
        /// Test GenerateFullPPRD for a null Id
        /// </summary>
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void TestGenerateFullPPRD_NullId()
        {
            ReportsControllerLogic sut = this.CreateSut();

            bool portionMarkingRequired = false;
            string serverFileName = "TestFile";
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();

            sut.GenerateFullPPRD(null, serverFileName, httpResponse.Object, portionMarkingRequired);
        }

        /// <summary>
        /// Test ExportRevisionAsJson for a revision
        /// </summary>
        [TestMethod]
        public void TestExportRevisionAsJson()
        {
            ReportsControllerLogic sut = this.CreateSut();

            string id = "1";
            string serverFileName = "TestFile";
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();

            RevisionModelView revision = new RevisionModelView()
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

            UserData userData = new UserData() { DisplayName = "Test User", Ntid = "test1" };
            this.securityInfo.Setup(x => x.ActiveUserNTID).Returns(userData.Ntid);
            this.adUtils.Setup(x => x.GetUserByQualifiedAccount(userData.Ntid, false)).Returns(userData);
            this.revisionMediator.Setup(x => x.GetById(1)).Returns(revision);
            this.sectionLoader.Setup(x => x.GetAll(revision, false, null, It.IsAny<string>())).Returns(new Collection<SectionModelView>());
            this.rateDetailLoader.Setup(x => x.GetRatesByRevision(revision)).Returns(new Collection<RateDetailModelView>());
            this.cobraDetailLoader.Setup(x => x.GetCobraDetailsByRevision(revision)).Returns(new Collection<CobraDetailModelView>());
            this.burdenPoolLoader.Setup(x => x.GetByRevision(revision.Id)).Returns(new BurdenPoolGridModelView());

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
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();

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
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();

            sut.ExportRevisionAsJson(null, serverFileName);
        }
    }
}
