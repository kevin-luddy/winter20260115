// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Linq;
    using System.Transactions;
    using Common;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Class for Section Loader
    /// </summary>
    [TestClass]
    public class SectionLoaderTest
    {
        /// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Test UpdateSectionsAndContent with null revision.
        /// </summary>
        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void TestUpdateSectionsAndContent_EX1()
        {
            SectionLoader sut = this.testData.SectionLoader;
            sut.UpdateSectionsAndContent(null, null);
        }

        /// <summary>
        /// Test UpdateSectionsAndContent with null sections.
        /// </summary>
        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void TestUpdateSectionsAndContent_EX2()
        {
            SectionLoader sut = this.testData.SectionLoader;
            sut.UpdateSectionsAndContent(new RevisionModelView(), null);
        }

        /// <summary>
        /// Tests adding a section
        /// </summary>
        [TestMethod]
        public void ValidateSectionLoaderUpdateSectionsAndContentAddSection()
        {
            SectionLoader sut = this.testData.SectionLoader;
            RevisionModelView revision = this.testData.GetRevision(true);

            // Start with sections and content like:
            //           1                       2
            //           |                     /   \
            //          1.1                  2.1   2.2
            //       /       \
            //  1.1.1         1.1.2
            //           /    /    \    \
            //    1.1.2.1  1.1.2.2 Text  Table
            this.testData.AddBaselineSectionsAndRatesData(revision);

            SectionModelView sectionFromGui = new SectionModelView
            {
                Id = -1,
                ChildNodes = sut.GetAll(revision)
            };

            SectionModelView subSubSectionFromGui = sectionFromGui.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0);
            this.testData.MoveSectionNode(0, subSubSectionFromGui.ChildNodes, sectionFromGui.ChildNodes);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.UpdateSectionsAndContent(revision, sectionFromGui.ChildNodes);
                scope.Complete();
            }

            ICollection<SectionModelView> actual = sut.GetAll(revision);
            this.testData.AssertSectionModelViewCollectionsAreEquivalent(sectionFromGui.ChildNodes, actual, false);
        }

        /// <summary>
        /// Tests the retrieving of sub set of sections.
        /// </summary>
        [TestMethod]
        public void TestRetrievingSubSetOfSections()
        {
            SectionLoader sut = this.testData.SectionLoader;
            RevisionModelView revision = this.testData.GetRevision(true);

            // Start with sections and content like:
            //           1                       2
            //           |                     /   \
            //          1.1                  2.1   2.2
            //       /       \
            //  1.1.1         1.1.2
            //           /    /    \    \
            //    1.1.2.1  1.1.2.2 Text  Table
            this.testData.AddBaselineSectionsAndRatesData(revision);

            ICollection<SectionModelView> allsections = sut.GetAll(revision);
            SectionModelView section1 = allsections.First();
            SectionModelView section11 = section1.ChildNodes.First();
            SectionModelView section112 = section11.ChildNodes.Last();

            Assert.AreEqual("1.1.2", section112.ReferenceNumber);
            ICollection<SectionModelView> subset = sut.GetAll(revision, false, new int[] { section1.Id, section11.Id, section112.Id });

            Assert.AreEqual(1, subset.Count);
            Assert.AreEqual(1, subset.First().ChildNodes.Count);
            Assert.AreEqual(1, subset.First().ChildNodes.First().ChildNodes.Count);
            
            // 1.1.2 is now renumbered as 1.1.1
            Assert.AreEqual("1.1.1", subset.First().ChildNodes.First().ChildNodes.First().ReferenceNumber);
            
            // Make sure section 1.1.2 still has it's text and table child nodes
            Assert.IsTrue(subset.First().ChildNodes.First().ChildNodes.First().ChildNodes.Any(c => c.ContentType == SectionContentType.RateTable));
            Assert.IsTrue(subset.First().ChildNodes.First().ChildNodes.First().ChildNodes.Any(c => c.ContentType == SectionContentType.Text));
        }

        /// <summary>
        /// Tests insert, update, and delete. Make sure can not delete a section with existing rate code.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EntityCommandExecutionException))]
        public void TestDeleteSectionWithAssociatedRateCode_EX1()
        {
            SectionLoader sut = this.testData.SectionLoader;
            RevisionModelView revision = this.testData.GetRevision(true);
            this.testData.AddBaselineSectionsAndRatesData(revision);
            ICollection<SectionModelView> sections = sut.GetAll(revision);
            SectionModelView parent = sections.First(x => x.Title.Equals("Title 1"));
            Assert.IsNotNull(parent);
            parent.Updateable = UpdateType.Upsert;
            parent.ChildNodes.Clear();  // remove all children, some of which are associated with rate codes           

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                this.testData.SectionLoader.UpdateSectionsAndContent(revision, sections);
                // This scope . Complete() below is here so that the number of using scope == the number of scope completes for easy searching in the entire solution.
                // And yes, this did come about because we were 2 off (missing a "real" complete somewhere else)
                // scope.Complete();
            }
        }

        /// <summary>
        /// Tests UpdateSectionsAndContent method when saving sections without any modifications.
        /// </summary>
        [TestMethod]
        public void TestUpdateSectionsAndContent_0()
        {
            SectionLoader sut = this.testData.SectionLoader;
            RevisionModelView revision = this.testData.GetRevision(true);

            // Start with sections and content like:
            //           1                       2
            //           |                     /   \
            //          1.1                  2.1   2.2
            //       /       \
            //  1.1.1         1.1.2
            //           /    /    \    \
            //    1.1.2.1  1.1.2.2 Text  Table
            this.testData.AddBaselineSectionsAndRatesData(revision);

            ICollection<SectionModelView> expected = sut.GetAll(revision);

            // perform update with no changes to the sections
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.UpdateSectionsAndContent(revision, expected);
                scope.Complete();
            }

            // verify expected and actual are identical (including IDs)
            ICollection<SectionModelView> actual = sut.GetAll(revision);
            this.testData.AssertSectionModelViewCollectionsAreEquivalent(expected, actual, true);

            // Verify section options list
            ICollection<OptionModelView> options = sut.RetrieveSectionsAsOptions(revision);
            Assert.IsNotNull(options);
            Assert.AreEqual(10, options.Count); // 9 sections + blank option
            Assert.AreEqual(1, options.Count(x => x.Id == 0 && string.IsNullOrEmpty(x.Label)));
            SectionModelView subSection = expected.ElementAt(0).ChildNodes.ElementAt(0);    // section 1.1
            Assert.AreEqual(1, options.Count(x => x.Id == subSection.Id && x.Label.Equals(string.Format("{0} - {1}", subSection.ReferenceNumber, subSection.Title))));
        }

        /// <summary>
        /// Tests UpdateSectionsAndContent method when saving sections with a simple title modification.
        /// </summary>
        [TestMethod]
        public void TestUpdateSectionsAndContent_0a()
        {
            SectionLoader sut = this.testData.SectionLoader;
            RevisionModelView revision = this.testData.GetRevision(true);

            // Start with sections and content like:
            //           1                       2
            //           |                     /   \
            //          1.1                  2.1   2.2
            //       /       \
            //  1.1.1         1.1.2
            //           /    /    \    \
            //    1.1.2.1  1.1.2.2 Text  Table
            this.testData.AddBaselineSectionsAndRatesData(revision);

            ICollection<SectionModelView> expected = sut.GetAll(revision);
            expected.ElementAt(0).Title = "Modified title1";

            // perform update with no changes to the sections
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.UpdateSectionsAndContent(revision, expected);
                scope.Complete();
            }

            // verify expected and actual are identical (including IDs)
            ICollection<SectionModelView> actual = sut.GetAll(revision);
            this.testData.AssertSectionModelViewCollectionsAreEquivalent(expected, actual, true);
        }

        /// <summary>
        /// Tests UpdateSectionsAndContent method after adding a child section.
        /// </summary>
        [TestMethod]
        public void TestUpdateSectionsAndContent_1()
        {
            SectionLoader sut = this.testData.SectionLoader;
            RevisionModelView revision = this.testData.GetRevision(true);

            // Start with sections and content like:
            //           1                       2
            //           |                     /   \
            //          1.1                  2.1   2.2
            //       /       \
            //  1.1.1         1.1.2
            //           /    /    \    \
            //    1.1.2.1  1.1.2.2 Text  Table
            this.testData.AddBaselineSectionsAndRatesData(revision);

            // start with initial data and make some changes
            ICollection<SectionModelView> expected = sut.GetAll(revision);
            SectionModelView subSection = expected.ElementAt(0).ChildNodes.ElementAt(0);    // section 1.1

            // Add new child section to 1.1 (i.e. 1.1.2)
            subSection.ChildNodes.Add(new SectionModelView()
            {
                Id = -1,
                DisplayOrder = 2,
                ParentId = subSection.Id,
                Updateable = UpdateType.Upsert,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "SubSubTitle2",
                TextContent = string.Empty,
                DisplayRateCode = false,
                ContentType = SectionContentType.Section,
                ReferenceNumber = "1.1.2"
            });

            // perform update
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.UpdateSectionsAndContent(revision, expected);
                scope.Complete();
            }

            // verify expected and actual are identical
            ICollection<SectionModelView> actual = sut.GetAll(revision);
            this.testData.AssertSectionModelViewCollectionsAreEquivalent(expected, actual, false);
        }

        /// <summary>
        /// Tests UpdateSectionsAndContent method after adding, deleting, and moving child sections.
        /// </summary>
        [TestMethod]
        public void TestUpdateSectionsAndContent_2()
        {
            SectionLoader sut = this.testData.SectionLoader;
            RevisionModelView revision = this.testData.GetRevision(true);

            // Start with sections and content like:
            //           1                       2
            //           |                     /   \
            //          1.1                  2.1   2.2
            //       /       \
            //  1.1.1         1.1.2
            //           /    /    \    \
            //    1.1.2.1  1.1.2.2 Text  Table
            this.testData.AddBaselineSectionsAndRatesData(revision);
            ICollection<SectionModelView> expected = sut.GetAll(revision);

            // Move 1.1.2 nodes beneath 2.1 and delete node 2.2 to make sections like:
            //         1                             2
            //       /                             /   
            //     1.1                           2.1 
            //    /                              /
            // 1.1.1                          2.1.1
            //                           /    /    \    \
            //                    2.1.1.1  2.1.1.2 Text  Table

            this.testData.MoveSectionNode(1, expected.ElementAt(0).ChildNodes.ElementAt(0).ChildNodes, expected.ElementAt(1).ChildNodes.ElementAt(0).ChildNodes);

            SectionModelView subSection22 = expected.ElementAt(1).ChildNodes.ElementAt(1);    // section 2.2
            expected.ElementAt(1).ChildNodes.Remove(subSection22);

            // perform update
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.UpdateSectionsAndContent(revision, expected);
                scope.Complete();
            }

            // verify expected and actual are identical
            ICollection<SectionModelView> actual = sut.GetAll(revision);
            this.testData.AssertSectionModelViewCollectionsAreEquivalent(expected, actual, false);

            // Move 2.0 nodes beneath 1.0 to make sections like:
            //                 1        
            //               /   \          
            //             1.1   1.2  (formerly 2)
            //            /       /        
            //         1.1.1   1.2.1 (formerly 2.1) 
            //                   /
            //                1.2.1.1   
            //           /    /    \    \
            //    2.1.1.1  2.1.1.2 Text  Table
            expected = sut.GetAll(revision);
            this.testData.MoveSectionNode(1, expected, expected.ElementAt(0).ChildNodes);

            // perform update
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.UpdateSectionsAndContent(revision, expected);
                scope.Complete();
            }

            // verify expected and actual are identical
            actual = sut.GetAll(revision);
            this.testData.AssertSectionModelViewCollectionsAreEquivalent(expected, actual, false);
        }

        /// <summary>
        /// Tests UpdateSectionsAndContent method after adding, deleting, and moving child sections.
        /// </summary>
        [TestMethod]
        public void TestUpdateSectionsAndContent_3()
        {
            SectionLoader sut = this.testData.SectionLoader;
            RevisionModelView revision = this.testData.GetRevision(true);

            // Start with sections and content like:
            //           1                       2
            //           |                     /   \
            //          1.1                  2.1   2.2
            //       /       \
            //  1.1.1         1.1.2
            //           /    /    \    \
            //    1.1.2.1  1.1.2.2 Text  Table
            this.testData.AddBaselineSectionsAndRatesData(revision);

            ICollection<SectionModelView> expected = sut.GetAll(revision);

            // Add sections under 1.1.2.2 so we have sections like:
            //           1                       2
            //           |                     /   \
            //          1.1                  2.1   2.2
            //       /       \
            //  1.1.1         1.1.2
            //           /    /    \    \
            //    1.1.2.1  1.1.2.2 Text  Table
            //              /    \
            //      1.1.2.2.1    1.1.2.2.2
            SectionModelView section11 = expected.ElementAt(0).ChildNodes.ElementAt(0);
            SectionModelView section112 = section11.ChildNodes.ElementAt(1);
            SectionModelView section1122 = section112.ChildNodes.ElementAt(1);
            section1122.ChildNodes.Add(new SectionModelView()
            {
                Id = -1,
                DisplayOrder = 1,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                ContentType = SectionContentType.Section,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "Title 1.1.2.2.1",
                TextContent = string.Empty,
                DisplayRateCode = false
            });

            section1122.ChildNodes.Add(new SectionModelView()
            {
                Id = -1,
                DisplayOrder = 2,
                ParentId = null,
                Updateable = UpdateType.Upsert,
                ContentType = SectionContentType.Section,
                RevisionId = revision.Id,
                IsInternalSection = false,
                Title = "Title 1.1.2.2.2",
                TextContent = string.Empty,
                DisplayRateCode = false
            });

            // Move 1.1.2.2.1 under 2.1, so we have sections like:
            //           1                       2
            //           |                     /   \
            //          1.1                  2.1   2.2
            //       /       \                |
            //  1.1.1         1.1.2        2.1.1 (formerly 1.1.2.2.1)
            //           /    /    \    \
            //    1.1.2.1  1.1.2.2 Text  Table
            //                 \
            //                 1.1.2.2.2
            this.testData.MoveSectionNode(0, section1122.ChildNodes, expected.ElementAt(1).ChildNodes.ElementAt(0).ChildNodes);

            // Delete 1.1.2.2, So we have sections like:
            //           1                       2
            //           |                     /   \
            //          1.1                  2.1   2.2
            //       /       \                |
            //  1.1.1         1.1.2        2.1.1 (formerly 1.1.2.2.1)
            //              /   |   \
            //       1.1.2.1  Text  Table
            section112.ChildNodes.Remove(section1122);

            // perform update
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
            {
                sut.UpdateSectionsAndContent(revision, expected);
                scope.Complete();
            }

            // verify expected and actual are identical
            ICollection<SectionModelView> actual = sut.GetAll(revision);
            this.testData.AssertSectionModelViewCollectionsAreEquivalent(expected, actual, false);
        }

        /// <summary>
        /// Test RetrieveAllSections (and GetAll) with OnlySections set to true
        /// </summary>
        [TestMethod]
        public void TestRetrieveAllSections_OnlySections()
        {
            SectionLoader sut = this.testData.SectionLoader;
            RevisionModelView revision = this.testData.GetRevision(true);

            // Start with sections and content like:
            //           1                       2
            //           |                     /   \
            //          1.1                  2.1   2.2
            //       /       \
            //  1.1.1         1.1.2
            //           /    /    \    \
            //    1.1.2.1  1.1.2.2 Text  Table
            this.testData.AddBaselineSectionsAndRatesData(revision);

            ICollection<SectionModelView> result = sut.RetrieveAllSections(revision, true);

            // Only Sections with a Content Type of Sections should be returned
            Assert.IsTrue(result.Any());
            this.AssertOnlySectionsReturned(result);

            Assert.IsFalse(result.First().ReferenceNumber.EndsWith(".0"));
        }

        /// <summary>
        /// Recursively assert that only Sections with a Content Type of Sections are in the collection
        /// </summary>
        /// <param name="sections">Collection of sections to check</param>
        private void AssertOnlySectionsReturned(ICollection<SectionModelView> sections)
        {
            Assert.IsFalse(sections.Any(x => x.ContentType != SectionContentType.Section));
            foreach (SectionModelView section in sections)
            {
                if (section.ChildNodes.Any())
                {
                    this.AssertOnlySectionsReturned(section.ChildNodes);
                }
            }
        }
    }
}