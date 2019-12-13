// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using Common;
    using DataBridge.Loaders;
    using DataBridge.ModelViews;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Class for File Attachment Loader
    /// </summary>
    [TestClass]
    public class FileAttachmentLoaderTest
    {
        /// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Tests insert, update, retrieve and delete
        /// </summary>
        [TestMethod]
        public void TestFileAttachmentLoader()
        {
            FileAttachmentLoader sut = this.testData.FileAttachmentLoader;
            RevisionModelView wip = this.testData.RevisionMediator.GetWipRevision();
            ICollection<OptionModelView> sections = this.testData.SectionLoader.RetrieveSectionsAsOptions(wip);
            OptionModelView firstSection = sections.ElementAt(1);
            OptionModelView secondSection = sections.Last();
            FileAttachmentRowModelView item = new FileAttachmentRowModelView
            {
                Id = -3,
                Link = "https://www.google.com",
                Name = "Google",
                RevisionID = wip.Id,
                SectionId = firstSection.Id,
                Updateable = UpdateType.Upsert
            };

            int? id = null;

            // insert
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                id = sut.Save(item);
                scope.Complete();
            }

            ICollection<FileAttachmentRowModelView> results = sut.GetByRevision(wip.Id);
            Assert.AreEqual(1, results.Count(x => x.Id == id && x.Name == item.Name && x.Link == item.Link && x.RevisionID == item.RevisionID && x.SectionId == item.SectionId));

            item = results.First(x => x.Id == id);
            item.Link = "Bubbly";
            item.Name = "Troubly";
            item.SectionId = secondSection.Id;
            item.Updateable = UpdateType.Upsert;

            // update
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                id = sut.Save(item);
                scope.Complete();
            }

            results = sut.GetByRevision(wip.Id);
            Assert.AreEqual(1, results.Count(x => x.Id == id && x.Name == item.Name && x.Link == item.Link && x.RevisionID == item.RevisionID && x.SectionId == item.SectionId));

            // delete
            item = results.First(x => x.Id == id);
            item.Updateable = UpdateType.Deleted;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                id = sut.Save(item);
                scope.Complete();
            }

            results = sut.GetByRevision(wip.Id);
            Assert.AreEqual(0, results.Count(x => x.Id == id));
        }
    }
}
