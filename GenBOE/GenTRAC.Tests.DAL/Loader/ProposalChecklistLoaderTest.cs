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
    using System.Transactions;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test class for the ProposalChecklistLoaderTest
    /// </summary>
    [TestClass]
    public class ProposalChecklistLoaderTest
    {
        /// <summary>
        /// Test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Proposal loader
        /// </summary>
        /// <returns>loader</returns>
        private ProposalChecklistLoader CreateSystem()
        {
            return new ProposalChecklistLoader();
        }

        /// <summary>
        /// Save proposal as pricer, then get by id
        /// </summary>
        [TestMethod]
        public void L_SaveProposalChecklistAsPricerAndGetProposalByID()
        {
            var sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);

            UserDTO submitter = this.testData.GetUser();
            
            ICollection<ChecklistContentItem> pprQuestions =
                this.testData.GetPPRChecklistForProposal(proposal.Id).Content.Where(x => x.TextType == ChecklistTextType.Question).OrderBy(x => x.ChecklistId).ToList();

            List<ChecklistResponseItem> pprResponses = new List<ChecklistResponseItem>()
            {
                new ChecklistResponseItem()
                {
                    ChecklistContentId = pprQuestions.ElementAt(0).Id,
                    ChecklistType = ChecklistType.ProposalPricingReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.Yes,
                    ResponseType = ChecklistResponseType.Pricer
                },
                new ChecklistResponseItem()
                {
                    ChecklistContentId = pprQuestions.ElementAt(1).Id,
                    ChecklistType = ChecklistType.ProposalPricingReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.No,
                    ResponseType = ChecklistResponseType.Pricer
                },
                new ChecklistResponseItem()
                {
                    ChecklistContentId = pprQuestions.ElementAt(2).Id,
                    ChecklistType = ChecklistType.ProposalPricingReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.NA,
                    ResponseType = ChecklistResponseType.Pricer
                }
            };

            ICollection<ChecklistContentItem> parQuestions =
                this.testData.GetPARChecklistForProposal(proposal.Id).Content.Where(x => x.TextType == ChecklistTextType.Question).OrderBy(x => x.ChecklistId).ToList();

            List<ChecklistResponseItem> parResponses = new List<ChecklistResponseItem>()
            {
                new ChecklistResponseItem()
                {
                    ChecklistContentId = parQuestions.ElementAt(0).Id,
                    ChecklistType = ChecklistType.ProposalAdequacyReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.Yes,
                    ResponseType = ChecklistResponseType.Pricer
                },
                new ChecklistResponseItem()
                {
                    ChecklistContentId = parQuestions.ElementAt(1).Id,
                    ChecklistType = ChecklistType.ProposalAdequacyReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.No,
                    ResponseType = ChecklistResponseType.Pricer
                },
                new ChecklistResponseItem()
                {
                    ChecklistContentId = parQuestions.ElementAt(2).Id,
                    ChecklistType = ChecklistType.ProposalAdequacyReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.NA,
                    ResponseType = ChecklistResponseType.Pricer
                }
            };

            ProposalChecklistDto newChecklistDto = new ProposalChecklistDto()
            {
                Id = -1,
                ProposalID = proposal.Id,
                LMLaborHrs = new decimal(12.12),
                LMLaborCost = 2000,
                SubcontractorCost = 1000,
                MaterialCost = 1000,
                IWTACost = 1000,
                TravelCost = 1000,
                OtherDirectCosts = 100,
                ROSPercentage = (decimal)62.50,
                ProfitFeeCOM = 1000,
                AbsoluteValue = 1500,
                EstimatingSubmitsToContractsDate = DateTime.Now,
                ResponseType = ChecklistResponseType.Pricer,
                SubmittedValue = 2000,
                PPRResponses = pprResponses,
                PARResponses = parResponses,
                Updateable = UpdateType.Upsert,
                IsSubmit = false
            };

            string pprComment = "this is my par comment";
            newChecklistDto.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalPricingReview, new ProposalChecklistSaveInfo()
            {
                UserID = submitter.Id,
                Comment = pprComment
            });

            string parComment = "this is my par comment";
            newChecklistDto.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalAdequacyReview, new ProposalChecklistSaveInfo()
            {
                UserID = submitter.Id,
                Comment = parComment
            });

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(newChecklistDto);
                scope.Complete();
            }

            ProposalChecklistDto toTest = sut.GetByProposalIds(new Collection<int>() { proposal.Id }).FirstOrDefault();

            DtoAssertHelpers.AssertDtos(newChecklistDto, toTest);

            Assert.AreEqual(submitter.Id, toTest.UserSaveInfo[ChecklistResponseType.Pricer][ChecklistType.ProposalPricingReview].UserID);
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), toTest.UserSaveInfo[ChecklistResponseType.Pricer][ChecklistType.ProposalPricingReview].LastSaveDate.Value.ToString("MM/dd/yyyy"));
            Assert.IsNull(toTest.UserSaveInfo[ChecklistResponseType.Pricer][ChecklistType.ProposalPricingReview].SubmitDate);
            Assert.AreEqual(pprComment, toTest.UserSaveInfo[ChecklistResponseType.Pricer][ChecklistType.ProposalPricingReview].Comment);

            Assert.AreEqual(submitter.Id, toTest.UserSaveInfo[ChecklistResponseType.Pricer][ChecklistType.ProposalAdequacyReview].UserID);
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), toTest.UserSaveInfo[ChecklistResponseType.Pricer][ChecklistType.ProposalAdequacyReview].LastSaveDate.Value.ToString("MM/dd/yyyy"));
            Assert.IsNull(toTest.UserSaveInfo[ChecklistResponseType.Pricer][ChecklistType.ProposalAdequacyReview].SubmitDate);
            Assert.AreEqual(parComment, toTest.UserSaveInfo[ChecklistResponseType.Pricer][ChecklistType.ProposalAdequacyReview].Comment);
        }

        /// <summary>
        /// Save proposal as peer reviewer, then get by id
        /// </summary>
        [TestMethod]
        public void L_SaveProposalChecklistAsPeerReviewerAndGetProposalByID()
        {
            var sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);
            ProposalChecklistDto proposalChecklist = this.testData.SaveChecklistAsPricer(proposal.Id);
            string parComment = "this is my par comment";
            proposalChecklist.UserSaveInfo[ChecklistResponseType.Peer].Add(ChecklistType.ProposalAdequacyReview, new ProposalChecklistSaveInfo
            {
                Comment = parComment
            });

            UserDTO submitter = this.testData.GetUser();

            ICollection<ChecklistContentItem> parQuestions =
                this.testData.GetPARChecklistForProposal(proposal.Id).Content.Where(x => x.TextType == ChecklistTextType.Question).OrderBy(x => x.ChecklistId).ToList();

            List<ChecklistResponseItem> parResponses = new List<ChecklistResponseItem>()
            {
                new ChecklistResponseItem()
                {
                    ChecklistContentId = parQuestions.ElementAt(0).Id,
                    ChecklistType = ChecklistType.ProposalAdequacyReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.Yes,
                    ResponseType = ChecklistResponseType.Peer,
                    PricerPageNumber = string.Empty,
                    RowComment = string.Empty
                },
                new ChecklistResponseItem()
                {
                    ChecklistContentId = parQuestions.ElementAt(1).Id,
                    ChecklistType = ChecklistType.ProposalAdequacyReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.No,
                    ResponseType = ChecklistResponseType.Peer,
                    PricerPageNumber = string.Empty,
                    RowComment = string.Empty
                },
                new ChecklistResponseItem()
                {
                    ChecklistContentId = parQuestions.ElementAt(2).Id,
                    ChecklistType = ChecklistType.ProposalAdequacyReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.NA,
                    ResponseType = ChecklistResponseType.Peer,
                    PricerPageNumber = string.Empty,
                    RowComment = string.Empty
                }
            };

            // peer can only save PAR responses, so leave remaining fields as default
            proposalChecklist.ResponseType = ChecklistResponseType.Peer;
            proposalChecklist.PARResponses = parResponses;
            proposalChecklist.Updateable = UpdateType.Upsert;
            proposalChecklist.UserSaveInfo[ChecklistResponseType.Peer][ChecklistType.ProposalAdequacyReview].UserID = submitter.Id;

            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(proposalChecklist);
                scope.Complete();
            }

            ProposalChecklistDto toTest = sut.GetByProposalIds(new Collection<int>() { proposal.Id }).FirstOrDefault();

            DtoAssertHelpers.AssertDtos(proposalChecklist, toTest);

            Assert.AreEqual(submitter.Id, toTest.UserSaveInfo[ChecklistResponseType.Peer][ChecklistType.ProposalAdequacyReview].UserID);
            Assert.AreEqual(DateTime.Now.ToString("MM/dd/yyyy"), toTest.UserSaveInfo[ChecklistResponseType.Peer][ChecklistType.ProposalAdequacyReview].LastSaveDate.Value.ToString("MM/dd/yyyy"));
            Assert.IsNull(toTest.UserSaveInfo[ChecklistResponseType.Peer][ChecklistType.ProposalAdequacyReview].SubmitDate);
            Assert.AreEqual(parComment, toTest.UserSaveInfo[ChecklistResponseType.Peer][ChecklistType.ProposalAdequacyReview].Comment);
        }

        /// <summary>
        /// Get By IDs test
        /// </summary>
        [TestMethod]
        public void L_GetChecklistsByIds()
        {
            var sut = this.CreateSystem();

            ProposalDto proposal1 = this.testData.GetProposal();
            ProposalChecklistDto checklist1 = this.testData.SaveChecklistAsPricer(proposal1.Id);

            ProposalDto proposal2 = this.testData.GetProposal(inCreateNew: true);
            ProposalChecklistDto checklist2 = this.testData.SaveChecklistAsPricer(proposal2.Id);

            ICollection<ProposalChecklistDto> actualResults = sut.GetByIds(new Collection<int>() { checklist1.Id, checklist2.Id });

            DtoAssertHelpers.AssertDtos(checklist1, actualResults.FirstOrDefault(x => x.Id == checklist1.Id));
            DtoAssertHelpers.AssertDtos(checklist2, actualResults.FirstOrDefault(x => x.Id == checklist2.Id));
        }

        /// <summary>
        /// Get all checklist save info test
        /// </summary>
        [TestMethod]
        public void L_GetAllChecklistSaveInfoTest()
        {
            var sut = this.CreateSystem();
            ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);
            ICollection<ProposalChecklistSaveInfo> saveInfo = sut.GetAllChecklistSaveInfo(proposal.Id);

            // no save info
            Assert.IsFalse(saveInfo.Any());

            this.testData.SaveChecklistAsPricer(proposal.Id);
            saveInfo = sut.GetAllChecklistSaveInfo(proposal.Id);

            // save info for pricer but not peer
            Assert.IsTrue(saveInfo.Any());
            Assert.IsTrue(saveInfo.Where(x => x.ResponseType == ChecklistResponseType.Pricer && x.ChecklistType == ChecklistType.ProposalPricingReview).Any());
            Assert.IsTrue(saveInfo.Where(x => x.ResponseType == ChecklistResponseType.Pricer && x.ChecklistType == ChecklistType.ProposalAdequacyReview).Any());
            Assert.IsFalse(saveInfo.Where(x => x.ResponseType == ChecklistResponseType.Peer && x.ChecklistType == ChecklistType.ProposalAdequacyReview).Any());

            this.testData.SaveChecklistAsPeer(proposal.Id);
            saveInfo = sut.GetAllChecklistSaveInfo(proposal.Id);

            // save info for pricer and peer
            Assert.IsTrue(saveInfo.Any());
            Assert.IsTrue(saveInfo.Where(x => x.ResponseType == ChecklistResponseType.Pricer && x.ChecklistType == ChecklistType.ProposalPricingReview).Any());
            Assert.IsTrue(saveInfo.Where(x => x.ResponseType == ChecklistResponseType.Pricer && x.ChecklistType == ChecklistType.ProposalAdequacyReview).Any());
            Assert.IsTrue(saveInfo.Where(x => x.ResponseType == ChecklistResponseType.Peer && x.ChecklistType == ChecklistType.ProposalAdequacyReview).Any());
        }

        /// <summary>
        /// Unlock checklist test
        /// </summary>
        [TestMethod]
        public void L_UnlockChecklistTest()
        {
            var sut = this.CreateSystem();
            IProposalLoader proposalLoader = new ProposalLoader();
            ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);
            ProposalChecklistDto checklist = this.testData.SaveChecklistAsPricer(proposal.Id);

            // submit as pricer
            checklist.IsSubmit = true;
            checklist.Updateable = UpdateType.Upsert;
            checklist.ResponseType = ChecklistResponseType.Pricer;
            this.testData.SaveChecklistAsPricer(proposal.Id, checklist);

            // verify save data before pricer unlock
            ICollection<ProposalChecklistSaveInfo> pricerSaveInfo = sut.GetAllChecklistSaveInfo(proposal.Id).Where(x => x.ResponseType == ChecklistResponseType.Pricer).ToList();
            proposal = proposalLoader.GetById(proposal.Id);
            Assert.AreEqual(ProposalStatus.InProgress, proposal.ProposalStatus);
            Assert.AreEqual(2, pricerSaveInfo.Count);
            Assert.AreEqual(0, pricerSaveInfo.Where(x => x.SubmitDate == null).Count());
            DateTime pricerLastSaveDate = pricerSaveInfo.First().LastSaveDate.Value;

            // unlock pricer
            sut.UnlockChecklist(proposal.Id, proposal.UpdateDate, UnlockChecklistOption.UnlockPricer);

            // verify save data after pricer unlock
            pricerSaveInfo = sut.GetAllChecklistSaveInfo(proposal.Id).Where(x => x.ResponseType == ChecklistResponseType.Pricer).ToList();
            proposal = proposalLoader.GetById(proposal.Id);
            Assert.AreEqual(ProposalStatus.InProgress, proposal.ProposalStatus);
            Assert.AreEqual(2, pricerSaveInfo.Count);
            Assert.AreEqual(2, pricerSaveInfo.Where(x => x.SubmitDate == null).Count());
            Assert.AreEqual(pricerLastSaveDate, pricerSaveInfo.First().LastSaveDate.Value);

            // submit as peer
            checklist = this.testData.SaveChecklistAsPeer(proposal.Id);
            checklist.IsSubmit = true;
            checklist.Updateable = UpdateType.Upsert;
            checklist.ResponseType = ChecklistResponseType.Peer;
            this.testData.SaveChecklistAsPeer(proposal.Id, checklist);

            // verify save data before peer unlock
            ICollection<ProposalChecklistSaveInfo> peerSaveInfo = sut.GetAllChecklistSaveInfo(proposal.Id).Where(x => x.ResponseType == ChecklistResponseType.Peer).ToList();
            proposal = proposalLoader.GetById(proposal.Id);
            Assert.AreEqual(ProposalStatus.InProgress, proposal.ProposalStatus);
            Assert.AreEqual(1, peerSaveInfo.Count);
            Assert.AreEqual(0, peerSaveInfo.Where(x => x.SubmitDate == null).Count());
            DateTime peerLastSaveDate = peerSaveInfo.First().LastSaveDate.Value;

            // unlock peer
            sut.UnlockChecklist(proposal.Id, proposal.UpdateDate, UnlockChecklistOption.UnlockPeer);

            // verify save data after peer unlock
            peerSaveInfo = sut.GetAllChecklistSaveInfo(proposal.Id).Where(x => x.ResponseType == ChecklistResponseType.Peer).ToList();
            proposal = proposalLoader.GetById(proposal.Id);
            Assert.AreEqual(ProposalStatus.InProgress, proposal.ProposalStatus);
            Assert.AreEqual(1, peerSaveInfo.Count);
            Assert.AreEqual(1, peerSaveInfo.Where(x => x.SubmitDate == null).Count());
            Assert.AreEqual(peerLastSaveDate, peerSaveInfo.First().LastSaveDate.Value);

            // submit as both and set proposal to complete
            checklist = sut.GetById(checklist.Id);
            checklist.IsSubmit = true;
            checklist.Updateable = UpdateType.Upsert;
            checklist.ResponseType = ChecklistResponseType.Pricer;
            this.testData.SaveChecklistAsPricer(proposal.Id, checklist);
            checklist = sut.GetById(checklist.Id);
            checklist.IsSubmit = true;
            checklist.Updateable = UpdateType.Upsert;
            checklist.ResponseType = ChecklistResponseType.Peer;
            this.testData.SaveChecklistAsPeer(proposal.Id, checklist);
            this.testData.SetProposalStatus(proposal.Id, ProposalStatus.Completed);

            // verify save data before both unlock
            pricerSaveInfo = sut.GetAllChecklistSaveInfo(proposal.Id).Where(x => x.ResponseType == ChecklistResponseType.Pricer).ToList();
            peerSaveInfo = sut.GetAllChecklistSaveInfo(proposal.Id).Where(x => x.ResponseType == ChecklistResponseType.Peer).ToList();
            proposal = proposalLoader.GetById(proposal.Id);
            Assert.AreEqual(ProposalStatus.Completed, proposal.ProposalStatus);
            Assert.AreEqual(2, pricerSaveInfo.Count);
            Assert.AreEqual(0, pricerSaveInfo.Where(x => x.SubmitDate == null).Count());
            Assert.AreEqual(1, peerSaveInfo.Count);
            Assert.AreEqual(0, peerSaveInfo.Where(x => x.SubmitDate == null).Count());
            pricerLastSaveDate = pricerSaveInfo.First().LastSaveDate.Value; 
            peerLastSaveDate = peerSaveInfo.First().LastSaveDate.Value;

            // unlock both
            sut.UnlockChecklist(proposal.Id, proposal.UpdateDate, UnlockChecklistOption.UnlockBoth);

            // verify save data after peer unlock
            pricerSaveInfo = sut.GetAllChecklistSaveInfo(proposal.Id).Where(x => x.ResponseType == ChecklistResponseType.Pricer).ToList();
            peerSaveInfo = sut.GetAllChecklistSaveInfo(proposal.Id).Where(x => x.ResponseType == ChecklistResponseType.Peer).ToList();
            proposal = proposalLoader.GetById(proposal.Id);
            Assert.AreEqual(ProposalStatus.InProgress, proposal.ProposalStatus);
            Assert.AreEqual(2, pricerSaveInfo.Count);
            Assert.AreEqual(2, pricerSaveInfo.Where(x => x.SubmitDate == null).Count());
            Assert.AreEqual(1, peerSaveInfo.Count);
            Assert.AreEqual(1, peerSaveInfo.Where(x => x.SubmitDate == null).Count());
            Assert.AreEqual(pricerLastSaveDate, pricerSaveInfo.First().LastSaveDate.Value);
            Assert.AreEqual(peerLastSaveDate, peerSaveInfo.First().LastSaveDate.Value);
        }

        /// <summary>
        /// Get PPR Responses test
        /// </summary>
        [TestMethod]
        public void L_GetPPRResponsesTest()
        {
            var sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);
            List<ChecklistResponseItem> responses = sut.GetPPRResponses(proposal.Id).ToList();

            Assert.IsTrue(responses.Any());
            responses.ForEach(x => Assert.AreEqual(proposal.Id, x.ProposalId));
            responses.ForEach(x => Assert.AreEqual(ChecklistType.ProposalPricingReview, x.ChecklistType));
            responses.ForEach(x => Assert.AreEqual(ChecklistResponseType.Pricer, x.ResponseType));
            Assert.AreEqual(1, responses.Where(x => x.Response == ChecklistResponseOption.No).Count());
            Assert.AreEqual(responses.Count - 1, responses.Where(x => x.Response == ChecklistResponseOption.NotSet).Count());
        }

        /// <summary>
        /// Get PAR Responses test
        /// </summary>
        [TestMethod]
        public void L_GetPARResponsesTest()
        {
            var sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);
            List<ChecklistResponseItem> responses = sut.GetPARResponses(proposal.Id).ToList();

            Assert.IsTrue(responses.Any());
            responses.ForEach(x => Assert.AreEqual(proposal.Id, x.ProposalId));
            responses.ForEach(x => Assert.AreEqual(ChecklistType.ProposalAdequacyReview, x.ChecklistType));
            responses.ForEach(x => Assert.AreEqual(ChecklistResponseOption.NotSet, x.Response));
            Assert.AreEqual(responses.Count / 2, responses.Where(x => x.ResponseType == ChecklistResponseType.Pricer).Count());
            Assert.AreEqual(responses.Count / 2, responses.Where(x => x.ResponseType == ChecklistResponseType.Peer).Count());
        }

        /// <summary>
        /// Get Proposal Submit Date test
        /// </summary>
        [TestMethod]
        public void L_GetProposalSubmitDateTest()
        {
            var sut = this.CreateSystem();

            ProposalDto proposal1 = this.testData.GetProposal(true);
            ProposalDto proposal2 = this.testData.GetProposal(true);
            ICollection<int> ids = new Collection<int>() { proposal1.Id, proposal2.Id };
            UserDTO submitter = this.testData.GetUser();

            // verify empty values
            IDictionary<int, DateTime?> submitDates = sut.GetProposalSubmittalDate(ids);
            Assert.IsFalse(submitDates.Any());

            // save checklists
            DateTime submitDate1 = new DateTime(2014, 1, 1);
            DateTime submitDate2 = new DateTime(2015, 2, 2);
            ProposalChecklistDto checklist1 = new ProposalChecklistDto()
            {
                ProposalID = proposal1.Id,
                EstimatingSubmitsToContractsDate = submitDate1,
                ResponseType = ChecklistResponseType.Pricer,
                Updateable = UpdateType.Upsert
            };
            checklist1.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalPricingReview, new ProposalChecklistSaveInfo()
            {
                UserID = submitter.Id
            });
            ProposalChecklistDto checklist2 = new ProposalChecklistDto()
            {
                ProposalID = proposal2.Id,
                EstimatingSubmitsToContractsDate = submitDate2,
                ResponseType = ChecklistResponseType.Pricer,
                Updateable = UpdateType.Upsert
            };
            checklist2.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalPricingReview, new ProposalChecklistSaveInfo()
            {
                UserID = submitter.Id
            });
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(checklist1);
                sut.Save(checklist2);
                scope.Complete();
            }

            // verify dates
            submitDates = sut.GetProposalSubmittalDate(ids);
            Assert.IsTrue(submitDates.ContainsKey(proposal1.Id));
            Assert.IsTrue(submitDates.ContainsKey(proposal2.Id));
            Assert.AreEqual(submitDate1, submitDates[proposal1.Id]);
            Assert.AreEqual(submitDate2, submitDates[proposal2.Id]);
        }

        #region Exception Test

        /// <summary>
        /// Delete proposal checklist.
        /// </summary>
        [TestMethod]
        public void L_DeleteProposalChecklist()
        {
            var sut = this.CreateSystem();

            ProposalDto proposal = this.testData.GetProposal(true);

            // Create and save a new Checklist.
            ProposalChecklistDto newChecklistDto = this.CreateProposalChecklist(proposal);
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(newChecklistDto);
                scope.Complete();
            }

            // Ensure the Checklist saved.
            ProposalChecklistDto checklistBeforeDelete = sut.GetByProposalIds(new Collection<int>() { proposal.Id }).FirstOrDefault();
            Assert.IsNotNull(checklistBeforeDelete);
            DtoAssertHelpers.AssertDtos(newChecklistDto, checklistBeforeDelete);

            // Delete the Checklist.
            checklistBeforeDelete.Updateable = UpdateType.Deleted;
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(checklistBeforeDelete);
                scope.Complete();
            }

            // Ensure the Checklist was deleted.
            ProposalChecklistDto checklistAfterDelete = sut.GetByProposalIds(new Collection<int>() { proposal.Id }).FirstOrDefault();
            Assert.IsNull(checklistAfterDelete);
        }

        /// <summary>
        /// Creates a ProposalChecklistDto object for the given ProposalDto.
        /// </summary>
        /// <param name="proposal">The ProposalDto.</param>
        /// <returns>A new ProposalChecklistDto for the ProposalDto.</returns>
        private ProposalChecklistDto CreateProposalChecklist(ProposalDto proposal)
        {
            UserDTO submitter = this.testData.GetUser();

            ICollection<ChecklistContentItem> pprQuestions =
                this.testData.GetPPRChecklistForProposal(proposal.Id).Content.Where(x => x.TextType == ChecklistTextType.Question).OrderBy(x => x.ChecklistId).ToList();

            List<ChecklistResponseItem> pprResponses = new List<ChecklistResponseItem>()
            {
                new ChecklistResponseItem()
                {
                    ChecklistContentId = pprQuestions.ElementAt(0).Id,
                    ChecklistType = ChecklistType.ProposalPricingReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.Yes,
                    ResponseType = ChecklistResponseType.Pricer
                },
                new ChecklistResponseItem()
                {
                    ChecklistContentId = pprQuestions.ElementAt(1).Id,
                    ChecklistType = ChecklistType.ProposalPricingReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.No,
                    ResponseType = ChecklistResponseType.Pricer
                },
                new ChecklistResponseItem()
                {
                    ChecklistContentId = pprQuestions.ElementAt(2).Id,
                    ChecklistType = ChecklistType.ProposalPricingReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.NA,
                    ResponseType = ChecklistResponseType.Pricer
                }
            };

            ICollection<ChecklistContentItem> parQuestions =
                this.testData.GetPARChecklistForProposal(proposal.Id).Content.Where(x => x.TextType == ChecklistTextType.Question).OrderBy(x => x.ChecklistId).ToList();

            List<ChecklistResponseItem> parResponses = new List<ChecklistResponseItem>()
            {
                new ChecklistResponseItem()
                {
                    ChecklistContentId = parQuestions.ElementAt(0).Id,
                    ChecklistType = ChecklistType.ProposalAdequacyReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.Yes,
                    ResponseType = ChecklistResponseType.Pricer
                },
                new ChecklistResponseItem()
                {
                    ChecklistContentId = parQuestions.ElementAt(1).Id,
                    ChecklistType = ChecklistType.ProposalAdequacyReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.No,
                    ResponseType = ChecklistResponseType.Pricer
                },
                new ChecklistResponseItem()
                {
                    ChecklistContentId = parQuestions.ElementAt(2).Id,
                    ChecklistType = ChecklistType.ProposalAdequacyReview,
                    ProposalId = proposal.Id,
                    Response = ChecklistResponseOption.NA,
                    ResponseType = ChecklistResponseType.Pricer
                }
            };

            // Save a Checklist.
            ProposalChecklistDto newChecklistDto = new ProposalChecklistDto()
            {
                Id = -1,
                ProposalID = proposal.Id,
                LMLaborHrs = new decimal(12.12),
                LMLaborCost = 2000,
                SubcontractorCost = 1000,
                MaterialCost = 1000,
                IWTACost = 1000,
                TravelCost = 1000,
                OtherDirectCosts = 100,
                ROSPercentage = (decimal)62.50,
                ProfitFeeCOM = 1000,
                AbsoluteValue = 1500,
                EstimatingSubmitsToContractsDate = DateTime.Now,
                ResponseType = ChecklistResponseType.Pricer,
                SubmittedValue = 2000,
                PPRResponses = pprResponses,
                PARResponses = parResponses,
                Updateable = UpdateType.Upsert,
                IsSubmit = false
            };

            string pprComment = "this is my par comment";
            newChecklistDto.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalPricingReview, new ProposalChecklistSaveInfo()
            {
                UserID = submitter.Id,
                Comment = pprComment
            });

            string parComment = "this is my par comment";
            newChecklistDto.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalAdequacyReview, new ProposalChecklistSaveInfo()
            {
                UserID = submitter.Id,
                Comment = parComment
            });

            return newChecklistDto;
        }

        /// <summary>
        /// Save proposal with exception (invalid argument)
        /// </summary>
        ////[TestMethod]
        ////[ExpectedException(typeof(ArgumentException))]
        ////public void L_SaveProposalException2()
        ////{
        ////    var sut = this.CreateSystem();

        ////    ProposalDto toSave = new ProposalDto();

        ////    using (TransactionScope scope = new TransactionScope())
        ////    {
        ////        sut.Save(toSave);
        ////        scope.Complete();
        ////    }
        ////}

        #endregion Exception Test
    }
}
