// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
    /// Test class for the Bulk Archive Loader
    /// </summary>
    [TestClass]
    public class BulkArchiveLoaderTest
    {
        /// <summary>
        /// Create System
        /// </summary>
        /// <returns>Bulk Archive Loader</returns>
        private IBulkArchiveLoader CreateSystem()
        {
            return new BulkArchiveLoader();
        }

        /// <summary>
        /// Test Data Utility Class
        /// </summary>
        private TestData testData = TestData.GetInstance();

       /// <summary>
       /// Search for a list of proposals, add 2 more that match the filter, 
       /// and then verify that the final list of proposals is +2
       /// </summary>
        [TestMethod]
        public void L_SearchBulkArchiveTest()
        {
            var sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);

            DateTime startDate = DateTime.Today.AddDays(-1);
            DateTime endDate = DateTime.Today.AddDays(1);
            string lineOfBusiness = proposal.LineOfBusinessID.ToString();
            string programArea = proposal.ProgramAreaId.ToString();

            BulkArchiveDto bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = startDate,
                EndDate = endDate,
                LineOfBusiness = lineOfBusiness,
                ProgramArea = programArea
            };

            int? initialResults = sut.SearchBulkArchive(bulkArchiveDto);

            // add 2 more proposals

            this.testData.GetProposal(true);
            this.testData.GetProposal(true);

            int? results = sut.SearchBulkArchive(bulkArchiveDto);

            Assert.AreEqual(results, initialResults + 2);
        }

        /// <summary>
        /// Perform the Search Bulk Archive query based on various search criteria, comparing the expected count from
        /// a manual query to the count returned from the SP
        /// </summary>
        [TestMethod]
        public void L_SearchBulkArchiveTest_SearchCriteria()
        {
            var sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);

            DateTime startDate = DateTime.Today;
            DateTime endDate = DateTime.Today.AddDays(1);
            string lineOfBusiness = proposal.LineOfBusinessID.ToString();
            string programArea = proposal.ProgramAreaId.ToString();
            BulkArchiveDto bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = startDate,
                EndDate = endDate,
                LineOfBusiness = lineOfBusiness,
                ProgramArea = programArea
            };

            // start and end date, specific Line of Business and Program Area
            int? queryResults = sut.SearchBulkArchive(bulkArchiveDto);

            ICollection<ProposalDto> allProposals = this.testData.ProposalLoader.GetAllSlim();

            int manualCount = 0;
            foreach (ProposalDto prop in allProposals) 
            {
                if (startDate <= prop.DateCreated && prop.DateCreated <= endDate &&
                            prop.LineOfBusinessID == proposal.LineOfBusinessID &&
                                prop.ProgramAreaId == proposal.ProgramAreaId && 
                                    (prop.ProposalStatus == ProposalStatus.InProgress || 
                                        prop.ProposalStatus == ProposalStatus.Completed ||
                                        prop.ProposalStatus == ProposalStatus.Submitted))
                {
                    ++manualCount;
                }
            }

            Assert.AreEqual(queryResults, manualCount);

            programArea = "All";
            startDate = DateTime.Today.AddDays(-5);
            bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = startDate,
                EndDate = null,
                LineOfBusiness = lineOfBusiness.ToString(),
                ProgramArea = programArea
            };

            // start date only, specific Line of Business, All Program Area
            queryResults = sut.SearchBulkArchive(bulkArchiveDto);

            manualCount = 0;
            foreach (ProposalDto prop in allProposals)
            {
                if (prop.DateCreated >= startDate && prop.LineOfBusinessID == int.Parse(lineOfBusiness) &&
                    (prop.ProposalStatus == ProposalStatus.InProgress || prop.ProposalStatus == ProposalStatus.Completed || prop.ProposalStatus == ProposalStatus.Submitted))
                {
                    ++manualCount;
                }
            }

            Assert.AreEqual(queryResults, manualCount);

            startDate = DateTime.Today.AddDays(-1);
            endDate = DateTime.Today.AddDays(1);
            lineOfBusiness = "All";
            programArea = "All"; 
            bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = startDate,
                EndDate = endDate,
                LineOfBusiness = lineOfBusiness,
                ProgramArea = programArea
            };

            // start and end dates, All Line of Business and Program Area
            queryResults = sut.SearchBulkArchive(bulkArchiveDto);

            manualCount = 0;
            foreach (ProposalDto prop in allProposals)
            {
                if (prop.DateCreated >= startDate && prop.DateCreated <= endDate &&
                        (prop.ProposalStatus == ProposalStatus.InProgress || prop.ProposalStatus == ProposalStatus.Completed || prop.ProposalStatus == ProposalStatus.Submitted))
                {
                    ++manualCount;
                }
            }

            Assert.AreEqual(queryResults, manualCount);

            bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = null,
                EndDate = null,
                LineOfBusiness = lineOfBusiness,
                ProgramArea = programArea
            };

            // no date, All Line of Business and Program Area
            queryResults = sut.SearchBulkArchive(bulkArchiveDto);

            manualCount = 0;
            foreach (ProposalDto prop in allProposals)
            {
                if (prop.ProposalStatus == ProposalStatus.InProgress || prop.ProposalStatus == ProposalStatus.Completed || prop.ProposalStatus == ProposalStatus.Submitted)
                {
                    ++manualCount;
                }
            }

            Assert.AreEqual(queryResults, manualCount);

            startDate = DateTime.Today.AddDays(-5);
            bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = startDate,
                EndDate = null,
                LineOfBusiness = lineOfBusiness,
                ProgramArea = programArea
            };

            // start date only, All Line of Business and Program Area
            queryResults = sut.SearchBulkArchive(bulkArchiveDto);

            manualCount = 0;
            foreach (ProposalDto prop in allProposals)
            {
                if (prop.DateCreated >= startDate &&
                    (prop.ProposalStatus == ProposalStatus.InProgress || prop.ProposalStatus == ProposalStatus.Completed || prop.ProposalStatus == ProposalStatus.Submitted))
                {
                    ++manualCount;
                }
            }

            Assert.AreEqual(queryResults, manualCount);

            endDate = DateTime.Today.AddDays(5);
            bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = null,
                EndDate = endDate,
                LineOfBusiness = lineOfBusiness,
                ProgramArea = programArea
            };

            // end date only, All Line of Business and Program Area
            queryResults = sut.SearchBulkArchive(bulkArchiveDto);

            manualCount = 0;
            foreach (ProposalDto prop in allProposals)
            {
                if (prop.DateCreated <= endDate &&
                    (prop.ProposalStatus == ProposalStatus.InProgress || prop.ProposalStatus == ProposalStatus.Completed || prop.ProposalStatus == ProposalStatus.Submitted))
                {
                    ++manualCount;
                }
            }

            Assert.AreEqual(queryResults, manualCount);
        }

        /// <summary>
        /// Get the results of performing a "Search Bulk Archive." Then, add 2 proposals (1 "In Progress" and 1 "Archived").
        /// Verify that calling "Search Bulk Archive" again results in the previous count + 1. The "Archived" Proposal
        /// should not be counted.
        /// </summary>
        [TestMethod]
        public void L_SearchBulkArchiveTest_VerifyCanceledProposalsNotCounted()
        {
            var sut = this.CreateSystem();

            DateTime startDate = DateTime.Today.AddDays(-5);
            DateTime endDate = DateTime.Today.AddDays(5);
            string lineOfBusiness = "All";
            string programArea = "All";

            BulkArchiveDto bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = startDate,
                EndDate = endDate,
                LineOfBusiness = lineOfBusiness,
                ProgramArea = programArea
            };

            int? initialResults = sut.SearchBulkArchive(bulkArchiveDto);

            ProposalDto prop = this.testData.GetProposal(true);
            prop.ProposalStatus = ProposalStatus.Archived;
            prop.Updateable = UpdateType.Upsert;
            this.testData.GetProposal(true, prop);

            this.testData.GetProposal(true);

            int? results = sut.SearchBulkArchive(bulkArchiveDto);

            Assert.AreEqual(results, initialResults + 1);
        }

        /// <summary>
        /// Get the results of performing a "Search Bulk Archive." Then, add 2 proposals to a 
        /// particular Line of Business. Then, run a search on the Line of Business with all Lines of Business.
        /// </summary>
        [TestMethod]
        public void L_SearchBulkArchiveTest_LineOfBusinessWithAllProgramArea()
        {
            var sut = this.CreateSystem();

            DateTime startDate = DateTime.Today.AddDays(-5);
            DateTime endDate = DateTime.Today.AddDays(5);
            int lineOfBusiness = 10;
            string programArea = "All";

            BulkArchiveDto bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = startDate,
                EndDate = endDate,
                LineOfBusiness = lineOfBusiness.ToString(),
                ProgramArea = programArea
            };

            int? initialResults = sut.SearchBulkArchive(bulkArchiveDto);

            ProposalDto prop = this.testData.GetProposal(true);
            prop.ProposalStatus = ProposalStatus.InProgress;
            prop.Updateable = UpdateType.Upsert;
            prop.LineOfBusinessID = lineOfBusiness;
            this.testData.GetProposal(true, prop);

            ProposalDto prop2 = this.testData.GetProposal(true);
            prop2.ProposalStatus = ProposalStatus.InProgress;
            prop2.Updateable = UpdateType.Upsert;
            prop2.LineOfBusinessID = lineOfBusiness; 
            this.testData.GetProposal(true, prop2);

            int? results = sut.SearchBulkArchive(bulkArchiveDto);

            Assert.AreEqual(results, initialResults + 2);
        }

        /// <summary>
        /// Add 2 proposals, perform a Search Bulk Archive, and then do a Apply Bulk Archive. 
        /// Manually, verify the results of the Search and Apply.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [TestMethod]
        public void L_ApplyBulkArchiveTest()
        {
            var sut = this.CreateSystem();

            // both dates
            DateTime startDate = DateTime.Today.AddDays(-5);
            DateTime endDate = DateTime.Today.AddDays(5);
            int lineOfBusiness = 10;
            int programArea = 47;

            BulkArchiveDto bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = startDate,
                EndDate = endDate,
                LineOfBusiness = lineOfBusiness.ToString(),
                ProgramArea = programArea.ToString()
            };

            // Save 2 proposals with the Line of Business and program Area to match Bulk Archive DTO filters
            ProposalDto prop = this.testData.GetProposal(true);
            prop.ProposalStatus = ProposalStatus.InProgress;
            prop.Updateable = UpdateType.Upsert;
            prop.LineOfBusinessID = lineOfBusiness;
            prop.ProgramAreaId = programArea;
            this.testData.GetProposal(true, prop);

            ProposalDto prop2 = this.testData.GetProposal(true);
            prop2.ProposalStatus = ProposalStatus.InProgress;
            prop2.Updateable = UpdateType.Upsert;
            prop2.LineOfBusinessID = lineOfBusiness;
            prop2.ProgramAreaId = programArea;
            this.testData.GetProposal(true, prop2);

            // Call GetAllSlim() since 2 proposals have since been saved to the database
            ICollection<ProposalDto> allProposals = this.testData.ProposalLoader.GetAllSlim();
            Dictionary<int, ProposalStatus> initialStatuses = allProposals.ToDictionary(x => x.Id, x => x.ProposalStatus);

            int initialArchivedProposalsCount = allProposals.Where(x => x.DateCreated >= startDate && x.DateCreated <= endDate &&
                    x.LineOfBusinessID == lineOfBusiness && x.ProgramAreaId == programArea &&
                    x.ProposalStatus == ProposalStatus.Archived).Count();

            ICollection<ProposalDto> proposalsBeingArchived = allProposals.Where(x => x.DateCreated >= startDate && x.DateCreated <= endDate &&
                    x.LineOfBusinessID == lineOfBusiness && x.ProgramAreaId == programArea && 
                    (x.ProposalStatus == ProposalStatus.InProgress || x.ProposalStatus == ProposalStatus.Completed || x.ProposalStatus == ProposalStatus.Submitted)).ToList();
            int manualSearchCount = proposalsBeingArchived.Count();

            int? searchResults = sut.SearchBulkArchive(bulkArchiveDto);
            Assert.AreEqual(searchResults, manualSearchCount);

            int? applyArchiveResults = sut.ApplyBulkArchive(bulkArchiveDto);

            // get all proposals after archive (only done once to minimize duration of test)
            ICollection<ProposalDto> allProposalsAfterArchive = this.testData.ProposalLoader.GetAllSlim();
            int afterArchivedProposalsCount = allProposalsAfterArchive.Where(x => x.DateCreated >= startDate && x.DateCreated <= endDate &&
                    x.LineOfBusinessID == lineOfBusiness && x.ProgramAreaId == programArea &&
                    x.ProposalStatus == ProposalStatus.Archived).Count();

            // verify number archived
            Assert.AreEqual(initialArchivedProposalsCount + manualSearchCount, afterArchivedProposalsCount);
            Assert.AreEqual(manualSearchCount, applyArchiveResults);
            foreach (ProposalDto proposal in proposalsBeingArchived)
            {
                Assert.AreEqual(ProposalStatus.Archived, this.testData.ProposalLoader.GetById(proposal.Id).ProposalStatus);
            }

            // revert proposal statuses
            foreach (ProposalDto proposal in proposalsBeingArchived)
            {
                this.testData.SetProposalStatus(proposal.Id, initialStatuses[proposal.Id]);
            }

            // only start date
            startDate = DateTime.Today.AddDays(-50);

            bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = startDate,
                EndDate = null,
                LineOfBusiness = lineOfBusiness.ToString(),
                ProgramArea = programArea.ToString()
            };

            proposalsBeingArchived = allProposals.Where(x => x.DateCreated >= startDate &&
                    x.LineOfBusinessID == lineOfBusiness && x.ProgramAreaId == programArea &&
                    (x.ProposalStatus == ProposalStatus.InProgress || x.ProposalStatus == ProposalStatus.Completed || x.ProposalStatus == ProposalStatus.Submitted)).ToList();
            manualSearchCount = proposalsBeingArchived.Count();

            searchResults = sut.SearchBulkArchive(bulkArchiveDto);
            Assert.AreEqual(searchResults, manualSearchCount);

            applyArchiveResults = sut.ApplyBulkArchive(bulkArchiveDto);

            // verify number archived
            Assert.AreEqual(manualSearchCount, applyArchiveResults);
            foreach (ProposalDto proposal in proposalsBeingArchived)
            {
                Assert.AreEqual(ProposalStatus.Archived, this.testData.ProposalLoader.GetById(proposal.Id).ProposalStatus);
            }

            // revert proposal statuses
            foreach (ProposalDto proposal in proposalsBeingArchived)
            {
                this.testData.SetProposalStatus(proposal.Id, initialStatuses[proposal.Id]);
            }

            // only end date
            bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = null,
                EndDate = endDate,
                LineOfBusiness = lineOfBusiness.ToString(),
                ProgramArea = programArea.ToString()
            };

            proposalsBeingArchived = allProposals.Where(x => x.DateCreated <= endDate &&
                    x.LineOfBusinessID == lineOfBusiness && x.ProgramAreaId == programArea &&
                    (x.ProposalStatus == ProposalStatus.InProgress || x.ProposalStatus == ProposalStatus.Completed || x.ProposalStatus == ProposalStatus.Submitted)).ToList();

            manualSearchCount = proposalsBeingArchived.Count();

            searchResults = sut.SearchBulkArchive(bulkArchiveDto);
            Assert.AreEqual(searchResults, manualSearchCount);

            applyArchiveResults = sut.ApplyBulkArchive(bulkArchiveDto);

            // verify number archived
            Assert.AreEqual(manualSearchCount, applyArchiveResults);
            foreach (ProposalDto proposal in proposalsBeingArchived)
            {
                Assert.AreEqual(ProposalStatus.Archived, this.testData.ProposalLoader.GetById(proposal.Id).ProposalStatus);
            }

            // revert proposal statuses
            foreach (ProposalDto proposal in proposalsBeingArchived)
            {
                this.testData.SetProposalStatus(proposal.Id, initialStatuses[proposal.Id]);
            }
        }

        /// <summary>
        /// Add 2 proposals, perform a Search Bulk Archive, and then do a Apply Bulk Archive. 
        /// </summary>
        [TestMethod]
        public void L_ApplyBulkArchiveTest_NoneArchivedSinceAddingIneligibleStatusProposals()
        {
            var sut = this.CreateSystem();

            DateTime startDate = DateTime.Today.AddDays(-50);
            int lineOfBusiness = 10;
            int programArea = 47;

            BulkArchiveDto bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = startDate,
                EndDate = null,
                LineOfBusiness = lineOfBusiness.ToString(),
                ProgramArea = programArea.ToString()
            };

            // Save 2 proposals with the Line of Business and program Area as per the Bulk Archive DTO
            ProposalDto prop = this.testData.GetProposal(true);
            prop.ProposalStatus = ProposalStatus.Archived;
            prop.Updateable = UpdateType.Upsert;
            prop.LineOfBusinessID = lineOfBusiness;
            prop.ProgramAreaId = programArea;
            this.testData.GetProposal(true, prop);

            ProposalDto prop2 = this.testData.GetProposal(true);
            prop2.ProposalStatus = ProposalStatus.Deleted;
            prop2.Updateable = UpdateType.Upsert;
            prop2.LineOfBusinessID = lineOfBusiness;
            prop2.ProgramAreaId = programArea;
            this.testData.GetProposal(true, prop2);

            int? searchResults = sut.SearchBulkArchive(bulkArchiveDto);

            int? archiveResults = sut.ApplyBulkArchive(bulkArchiveDto);

            Assert.AreEqual(archiveResults, searchResults);
        }
    }
}