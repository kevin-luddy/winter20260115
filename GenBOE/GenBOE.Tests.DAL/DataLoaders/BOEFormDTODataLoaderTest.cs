using IES.Common;
using GenBOE.DataBridge.DTO;
using GenBOE.Dtos;
using GenBOE.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Transactions;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public partial class BOEFormPBOEDTODataLoaderTest
    {
        private static class Stubs
        {
            public static Collection<BOEFormPBOEDTO> BOEFormPBOEDTOs
            {
                get
                {
                    return new Collection<BOEFormPBOEDTO>{
                        new BOEFormPBOEDTO
                        {
                            Id = -1,
                            ResourceIds = new List<int> { 1, 2, 3 },
                            ClinContractTypes = new List<BoeFormClinContractTypeDTO>
                            {
                                new BoeFormClinContractTypeDTO { ClinId = 1, ContractType = 1020 },
                                new BoeFormClinContractTypeDTO { ClinId = 2, ContractType = 1003 }
                            },
                            Revision = 0,
                            FormName = "FormName",
                            Updateable = UpdateType.None,
                            Description = "Unit Test - This item was created and used for unit testing.",
                            ProposalTitle = "ProposalTitle",
                            ProposalDate = "02/22/2016",  // sql server will truncate to 10 chars!
                            Poc = "poc",
                            PocPhone = "pocphone",
                            Approver = "approver",
                            ApproverPhone = "aproverphone",
                            Version = 1,
                            CCoPDApplies = true,
                            CommercialItemExceptionApplies = false,
                            CompetitionExceptionApplies = false,
                            OtherExceptionApplies = true,
                            OtherText = "OtherText",
                            RFP = "RFP",
                            ProposalNumber = "ProposalNumber",
                            SupplierName = "SupplierName",
                            ValidityDate = "09/11/1199",  // sql server will truncate to 10 chars!
                            ShouldCostEstimate = ScheduleEvent.Actual,
                            ShouldCostEstimateDate = DateTime.Parse("09/09/2016"),
                            RFPRelease = ScheduleEvent.Actual,
                            RFPReleaseDate = DateTime.Parse("09/09/2016"),
                            FirmSupplierReceipt = ScheduleEvent.Actual,
                            FirmSupplierReceiptDate = DateTime.Parse("09/09/2016"),
                            SourceSelection = ScheduleEvent.Actual,
                            SourceSelectionDate = DateTime.Parse("09/09/2016"),
                            CID = ScheduleEvent.Actual,
                            CIDDate = DateTime.Parse("09/09/2016"),
                            PriceAnalysis = ScheduleEvent.Actual,
                            PriceAnalysisDate = DateTime.Parse("09/09/2016"),
                            TechnicalEvaluation = ScheduleEvent.Actual,
                            TechnicalEvaluationDate = DateTime.Parse("09/09/2016"),
                            FactFinding = ScheduleEvent.Actual,
                            FactFindingDate = DateTime.Parse("09/09/2016"),
                            CostAnalysis = ScheduleEvent.Actual,
                            CostAnalysisDate = DateTime.Parse("09/09/2016"),
                            GovtPricing = ScheduleEvent.Actual,
                            GovtPricingDate = DateTime.Parse("09/09/2016"),
                            SupplierNegotiations = ScheduleEvent.Actual,
                            SupplierNegotiationsDate = DateTime.Parse("09/09/2016"),
                            MOU = ScheduleEvent.Actual,
                            MOUDate = DateTime.Parse("09/09/2016"),
                            PlannedDate_WrittenApproval = DateTime.Parse("09/09/2016"),
                            PlannedDate_ApprovedSubmission = DateTime.Parse("09/09/2016"),
                            CIDText = "cid",
                            CostAnalysisText = "cost",
                            FactFindingText = "fact",
                            FirmSupplierReceiptText = "firm",
                            GovtPricingText = "gov",
                            MOUText = "mou",
                            PriceAnalysisText = "price",
                            RFPReleaseText = "rfp",
                            ShouldCostEstimateText = "should",
                            SourceSelectionText = "source",
                            SupplierNegotiationsText = "supplier",
                            TechnicalEvaluationText = "tech",
                            RationaleValueSummary = RandomString(25),
                            CommercialityDescription = RandomString(20),
                            CostAnalysisDescription = RandomString(15),
                            PriceAnalysisDescription = RandomString(10),
                            GovtPricingReceived = ScheduleEvent.NA,
                            GovtPricingReceivedDate = new DateTime(2022, 04, 20),
                            GovtPricingReceivedText = RandomString(18),
                            CostAnalysisUnqual = ScheduleEvent.Planned,
                            CostAnalysisUnqualDate = new DateTime(2022, 04, 20),
                            CostAnalysisUnqualText = RandomString(11),
                        }
                    };
                }
            }
        }

        private static IBOEFormPBOEDTODataLoader sut = new BOEFormPBOEDTODataLoader();

        [TestMethod]
        public void GetByWorkspaceIdTest()
        {
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                // retrieve some ids to test against
                Collection<int> ids = gbe.BOEFormPBOEs.Select(x => x.WorkspaceID).Take(5).ToCollection();
                Assert.IsTrue(ids.Count > 0, "no test data?");

                foreach (int workspaceId in ids)
                {
                    var item = sut.GetByWorkspaceId(workspaceId);
                    Assert.IsNotNull(item);
                }
            }
        }

        [TestMethod]
        public void GetCurrentFormVersionTest()
        {
            var version = sut.GetCurrentFormVersion();
            Assert.IsTrue(version > -1);
        }

        [TestMethod]
        public void GetByIdsTest()
        {
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                // retrieve some ids to test against
                Collection<int> ids = gbe.BOEFormPBOEs.Select(x => x.PBOEFormID).Take(5).ToCollection();
                Assert.IsTrue(ids.Count > 0, "no test data?");

                IEnumerable<BOEFormPBOEDTO> items = sut.GetByIds(ids);
                Assert.IsTrue(items.Count() > 0);
            }
        }

        [TestMethod]
        public void Upsert_TestCreate()
        {
            // get stub
            var stubDto = Stubs.BOEFormPBOEDTOs.Single();
            stubDto.Updateable = UpdateType.Upsert;

            // save item
            int createdItemId = 0;
            using (TransactionScope scope = new TransactionScope())
            {
                createdItemId = sut.Save(new BOEFormPBOEDTO[] { stubDto }).Values.Single();
                scope.Complete();
            }
            Assert.IsTrue(createdItemId > 0, "Failed to save the item!");

            // lets compare some values to make sure that
            // this is absolutely the right object!
            BOEFormPBOEDTO createdItem = sut.GetById(createdItemId);
            AssertEquality(stubDto, createdItem);

            createdItem.Updateable = UpdateType.Deleted;
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(new BOEFormPBOEDTO[] { createdItem });
                scope.Complete();
            }
        }

        [TestMethod]
        public void Upsert_TestUpdate()
        {
            // get item
            BOEFormPBOEDTO testDto;
            using (var gbe = new GenBoeEntities())
            {
                // This is dangerous taking the first PBOE in table :(
                BOEFormPBOE item = gbe.BOEFormPBOEs.First();
                testDto = new BOEFormPBOEDTO
                {
                    Id = item.PBOEFormID,
                    Updateable = UpdateType.Upsert,
                    UpdateDateLong = item.UpdateDT.Ticks.ToString(),
                    WorkspaceId = item.WorkspaceID,
                    Revision = item.Revision,
                    UpdateDate = item.UpdateDT,
                    FormName = "Updated Form Name",
                    Description = "Unit Test - This item was created and used to test updates for unit testing.",
                    ProposalTitle = "ProposalTitle -",
                    ProposalDate = "02/22/2016 -",  // sql server will truncate to 10 chars!
                    Poc = "poc -",
                    PocPhone = "pocphone -",
                    Approver = "approver -",
                    ApproverPhone = "aproverphone -",
                    Version = item.FormVersion,
                    CCoPDApplies = false,
                    CommercialItemExceptionApplies = true,
                    CompetitionExceptionApplies = true,
                    OtherExceptionApplies = true,
                    OtherText = "OtherText -",
                    RFP = "RFP -",
                    ProposalNumber = "ProposalNumber -",
                    SupplierName = "SupplierName -",
                    ValidityDate = "01/22/1099",  // sql server will truncate to 10 chars!
                    ShouldCostEstimate = ScheduleEvent.Actual,
                    ShouldCostEstimateDate = DateTime.Parse("01/02/2016"),
                    RFPRelease = ScheduleEvent.Actual,
                    RFPReleaseDate = DateTime.Parse("05/06/2016"),
                    FirmSupplierReceipt = ScheduleEvent.Actual,
                    FirmSupplierReceiptDate = DateTime.Parse("07/08/2016"),
                    SourceSelection = ScheduleEvent.Actual,
                    SourceSelectionDate = DateTime.Parse("09/10/2016"),
                    CID = ScheduleEvent.Planned,
                    CIDDate = DateTime.Parse("11/12/2016"),
                    PriceAnalysis = ScheduleEvent.Actual,
                    PriceAnalysisDate = DateTime.Parse("01/03/2016"),
                    TechnicalEvaluation = ScheduleEvent.Planned,
                    TechnicalEvaluationDate = DateTime.Parse("02/04/2016"),
                    FactFinding = ScheduleEvent.Actual,
                    FactFindingDate = DateTime.Parse("03/06/2016"),
                    CostAnalysis = ScheduleEvent.Actual,
                    CostAnalysisDate = DateTime.Parse("04/06/2016"),
                    GovtPricing = ScheduleEvent.Actual,
                    GovtPricingDate = DateTime.Parse("06/08/2016"),
                    SupplierNegotiations = ScheduleEvent.Actual,
                    SupplierNegotiationsDate = DateTime.Parse("08/10/2016"),
                    MOU = ScheduleEvent.NA,
                    MOUDate = DateTime.Parse("10/12/2016"),
                    PlannedDate_WrittenApproval = DateTime.Parse("01/05/2016"),
                    PlannedDate_ApprovedSubmission = DateTime.Parse("01/07/2016"),
                    CIDText = "new cid",
                    CostAnalysisText = "new cost",
                    FactFindingText = "new fact",
                    FirmSupplierReceiptText = "new firm",
                    GovtPricingText = "new gov",
                    MOUText = "new mou",
                    PriceAnalysisText = "new price",
                    RFPReleaseText = "new rfp",
                    ShouldCostEstimateText = "new should",
                    SourceSelectionText = "new source",
                    SupplierNegotiationsText = "new supplier",
                    TechnicalEvaluationText = "new tech",
                    RationaleValueSummary = RandomString(25),
                    CommercialityDescription = RandomString(20),
                    CostAnalysisDescription = RandomString(15),
                    PriceAnalysisDescription = RandomString(10),
                    GovtPricingReceived = ScheduleEvent.NA,
                    GovtPricingReceivedDate = new DateTime(2022, 04, 20),
                    GovtPricingReceivedText = RandomString(18),
                    CostAnalysisUnqual = ScheduleEvent.Planned,
                    CostAnalysisUnqualDate = new DateTime(2022, 04, 20),
                    CostAnalysisUnqualText = RandomString(11),
                };

                testDto.ResourceIds.Add(6);
                testDto.ClinContractTypes.Add(new BoeFormClinContractTypeDTO { ClinId = 55, ContractType = 1002 });
                testDto.ClinContractTypes.Add(new BoeFormClinContractTypeDTO { ClinId = 908, ContractType = 1009 });
            }
           
            // save item
            int updatedItemId = 0;
            using (TransactionScope scope = new TransactionScope())
            {
                updatedItemId = sut.Save(new BOEFormPBOEDTO[] { testDto }).Values.Single();
                scope.Complete();
            }
            Assert.IsTrue(updatedItemId > 0, "Failed to save the item!");

            // lets compare some values to make sure that
            // this is absolutely the right object!
            BOEFormPBOEDTO updatedItem = sut.GetById(updatedItemId);
            AssertEquality(testDto, updatedItem);

            updatedItem = sut.GetByWorkspaceId(testDto.WorkspaceId).FirstOrDefault(p => p.Id == updatedItemId);
            AssertEquality(testDto, updatedItem);
        }

        [TestMethod]
        public void DeleteTest()
        {
            int createdItemId = 0;

            // save a new item
            using (TransactionScope scope = new TransactionScope())
            {
                BOEFormPBOEDTO dto = Stubs.BOEFormPBOEDTOs.First();
                dto.Updateable = UpdateType.Upsert;
                createdItemId = sut.Save( new BOEFormPBOEDTO[] { dto }).Values.First();
                scope.Complete();
            }

            using (var gbe = new GenBoeEntities())
            {
                // read item
                BOEFormPBOE retrievedItem = gbe.BOEFormPBOEs.Single(x => x.PBOEFormID == createdItemId);

                // delete item
                int? deletedItemId;
                using (TransactionScope scope = new TransactionScope())
                {
                    deletedItemId = sut.Save(
                        new BOEFormPBOEDTO
                        {
                            Id = retrievedItem.PBOEFormID,
                            Updateable = UpdateType.Deleted,
                            UpdateDateLong = retrievedItem.UpdateDT.Ticks.ToString()
                        });
                    scope.Complete();
                }

                Assert.IsNotNull(retrievedItem); // it was there
                Assert.IsFalse(gbe.BOEFormPBOEs.Any(x => x.PBOEFormID == deletedItemId)); // now its gone
            }
        }

        [TestMethod]
        public void TestPartialSave_PBOE()
        {
            BOEFormPBOEDTO stubDto = new BOEFormPBOEDTO
            {
                FormName = "test",
                Revision = 0,
                Updateable = UpdateType.Upsert
            };

            // save item
            int createdItemId = 0;
            using (TransactionScope scope = new TransactionScope())
            {
                createdItemId = sut.Save(new BOEFormPBOEDTO[] { stubDto }).Values.Single();
                scope.Complete();
            }
            Assert.IsTrue(createdItemId > 0, "Failed to save the item!");

            // lets compare some values to make sure that
            // this is absolutely the right object!
            BOEFormPBOEDTO createdItem = sut.GetById(createdItemId);
            AssertEquality(stubDto, createdItem);
        }

        private static void AssertEquality(BOEFormPBOEDTO expected, BOEFormPBOEDTO actual)
        {
            Assert.AreEqual(expected.CCoPDApplies, actual.CCoPDApplies);
            Assert.AreEqual(expected.CommercialItemExceptionApplies, actual.CommercialItemExceptionApplies);
            Assert.AreEqual(expected.CompetitionExceptionApplies, actual.CompetitionExceptionApplies);
            Assert.AreEqual(expected.OtherExceptionApplies, actual.OtherExceptionApplies);
            Assert.AreEqual<string>(expected.OtherText, actual.OtherText);
            Assert.AreEqual<string>(expected.RFP, actual.RFP);
            Assert.AreEqual<string>(expected.ProposalNumber, actual.ProposalNumber);
            Assert.AreEqual<string>(expected.SupplierName, actual.SupplierName);
            Assert.AreEqual<string>(expected.ValidityDate, actual.ValidityDate);
            Assert.AreEqual<int>(expected.Revision, actual.Revision);
            Assert.AreEqual(expected.ShouldCostEstimate, actual.ShouldCostEstimate);
            Assert.AreEqual<DateTime?>(expected.ShouldCostEstimateDate, actual.ShouldCostEstimateDate);
            Assert.AreEqual(expected.RFPRelease, actual.RFPRelease);
            Assert.AreEqual<DateTime?>(expected.RFPReleaseDate, actual.RFPReleaseDate);
            Assert.AreEqual(expected.FirmSupplierReceipt, actual.FirmSupplierReceipt);
            Assert.AreEqual<DateTime?>(expected.FirmSupplierReceiptDate, actual.FirmSupplierReceiptDate);
            Assert.AreEqual(expected.SourceSelection, actual.SourceSelection);
            Assert.AreEqual<DateTime?>(expected.SourceSelectionDate, actual.SourceSelectionDate);
            Assert.AreEqual(expected.CID, actual.CID);
            Assert.AreEqual<DateTime?>(expected.CIDDate, actual.CIDDate);
            Assert.AreEqual(expected.PriceAnalysis, actual.PriceAnalysis);
            Assert.AreEqual<DateTime?>(expected.PriceAnalysisDate, actual.PriceAnalysisDate);
            Assert.AreEqual(expected.TechnicalEvaluation, actual.TechnicalEvaluation);
            Assert.AreEqual<DateTime?>(expected.TechnicalEvaluationDate, actual.TechnicalEvaluationDate);
            Assert.AreEqual(expected.FactFinding, actual.FactFinding);
            Assert.AreEqual<DateTime?>(expected.FactFindingDate, actual.FactFindingDate);
            Assert.AreEqual(expected.CostAnalysis, actual.CostAnalysis);
            Assert.AreEqual<DateTime?>(expected.CostAnalysisDate, actual.CostAnalysisDate);
            Assert.AreEqual(expected.GovtPricing, actual.GovtPricing);
            Assert.AreEqual<DateTime?>(expected.GovtPricingDate, actual.GovtPricingDate);
            Assert.AreEqual(expected.SupplierNegotiations, actual.SupplierNegotiations);
            Assert.AreEqual<DateTime?>(expected.SupplierNegotiationsDate, actual.SupplierNegotiationsDate);
            Assert.AreEqual(expected.MOU, actual.MOU);
            Assert.AreEqual<DateTime?>(expected.MOUDate, actual.MOUDate);
            Assert.AreEqual<DateTime?>(expected.PlannedDate_WrittenApproval, actual.PlannedDate_WrittenApproval);
            Assert.AreEqual<DateTime?>(expected.PlannedDate_ApprovedSubmission, actual.PlannedDate_ApprovedSubmission);
            BOEFormIBOEDTODataLoaderTest.AssertEquality(expected.ResourceIds, actual.ResourceIds);
            BOEFormIBOEDTODataLoaderTest.AssertEquality(expected.ClinContractTypes, actual.ClinContractTypes);
            Assert.AreEqual(expected.CIDText, actual.CIDText);
            Assert.AreEqual(expected.PriceAnalysisText, actual.PriceAnalysisText);
            Assert.AreEqual(expected.TechnicalEvaluationText, actual.TechnicalEvaluationText);
            Assert.AreEqual(expected.FactFindingText, actual.FactFindingText);
            Assert.AreEqual(expected.CostAnalysisText, actual.CostAnalysisText);
            Assert.AreEqual(expected.GovtPricingText, actual.GovtPricingText);
            Assert.AreEqual(expected.SupplierNegotiationsText, actual.SupplierNegotiationsText);
            Assert.AreEqual(expected.MOUText, actual.MOUText);
            Assert.AreEqual(expected.ShouldCostEstimateText, actual.ShouldCostEstimateText);
            Assert.AreEqual(expected.RFPReleaseText, actual.RFPReleaseText);
            Assert.AreEqual(expected.FirmSupplierReceiptText, actual.FirmSupplierReceiptText);
            Assert.AreEqual(expected.SourceSelectionText, actual.SourceSelectionText);
            Assert.AreEqual(expected.PriceAnalysisDescription, actual.PriceAnalysisDescription);
            Assert.AreEqual(expected.RationaleValueSummary, actual.RationaleValueSummary);
            Assert.AreEqual(expected.CostAnalysisDescription, actual.CostAnalysisDescription);
            Assert.AreEqual(expected.CommercialityDescription, actual.CommercialityDescription);
            Assert.AreEqual(expected.GovtPricingReceived, actual.GovtPricingReceived);
            Assert.AreEqual(expected.GovtPricingReceivedDate, actual.GovtPricingReceivedDate);
            Assert.AreEqual(expected.GovtPricingReceivedText, actual.GovtPricingReceivedText);
            Assert.AreEqual(expected.CostAnalysisUnqual, actual.CostAnalysisUnqual);
            Assert.AreEqual(expected.CostAnalysisUnqualDate, actual.CostAnalysisUnqualDate);
            Assert.AreEqual(expected.CostAnalysisUnqualText, actual.CostAnalysisUnqualText);
        }

        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }


    [TestClass]
    public class BOEFormIBOEDTODataLoaderTest
    {
        private static class Stubs
        {
            public static Collection<BOEFormIBOEDTO> BOEFormIBOEDTOs
            {
                get
                {
                    return new Collection<BOEFormIBOEDTO>
                    {
                        new BOEFormIBOEDTO
                        {
                            Id = -1,
                            ResourceIds = new List<int> { 1, 2, 3 },
                            ClinContractTypes = new List<BoeFormClinContractTypeDTO>
                            {
                                new BoeFormClinContractTypeDTO { ClinId = 1, ContractType = 1013 },
                                new BoeFormClinContractTypeDTO { ClinId = 2, ContractType = 1015 }
                            },
                            FormName = "FormName",
                            Updateable = UpdateType.None,
                            Description = "Unit Test Description",
                            ProposalTitle = "ProposalTitle",
                            ProposalDate = "02/22/2016", // sql server will truncate to 10 chars!
                            Poc = "Poc",
                            PocPhone = "PocPhone",
                            Approver = "Approver",
                            ApproverPhone = "ApproverPhone",
                            Version = 1,
                            BusinessArea = "BusinessArea",
                            Revision = 0, // set to 0 since the assert makes sure revision is 1 version higher, and create is set to 1
                            WorkspaceId = 1
                        }
                    };
                }
            }
        }

        private static IBOEFormIBOEDTODataLoader sut = new BOEFormIBOEDTODataLoader();

        [TestMethod]
        public void TestPartialSave_IBOE()
        {
            BOEFormIBOEDTO stubDto = new BOEFormIBOEDTO
            {
                FormName = "test",
                Revision = 0,
                Updateable = UpdateType.Upsert
            };

            // save item
            int createdItemId = 0;
            using (TransactionScope scope = new TransactionScope())
            {
                createdItemId = sut.Save(new BOEFormIBOEDTO[] { stubDto }).Values.Single();
                scope.Complete();
            }
            Assert.IsTrue(createdItemId > 0, "Failed to save the item!");

            // lets compare some values to make sure that
            // this is absolutely the right object!
            BOEFormIBOEDTO createdItem = sut.GetById(createdItemId);
            AssertAreEqual(stubDto, createdItem);
        }

        [TestMethod]
        public void GetByWorkspaceIdTest()
        {
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                // retrieve some ids to test against
                Collection<int> ids = gbe.BOEFormIBOEs.Select(x => x.WorkspaceID).Take(5).ToCollection();
                Assert.IsTrue(ids.Count > 0, "no test data?");

                foreach (int workspaceId in ids) // no, this wont break if theyre are less than 10
                {
                    var item = sut.GetByWorkspaceId(workspaceId);
                    Assert.IsNotNull(item);
                }
            }
        }

        [TestMethod]
        public void GetCurrentFormVersionTest()
        {
            var version = sut.GetCurrentFormVersion();
            Assert.IsTrue(version > -1);
        }

        [TestMethod]
        public void GetByIdsTest()
        {
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                // retrieve some ids to test against
                Collection<int> ids = gbe.BOEFormIBOEs.Select(x => x.IBOEFormID).Take(5).ToCollection();
                Assert.IsTrue(ids.Count > 0, "no test data?");

                IEnumerable<BOEFormIBOEDTO> items = sut.GetByIds(ids);
                Assert.IsTrue(items.Count() > 0);
            }
        }

        [TestMethod]
        public void Upsert_TestCreate()
        {
            // get stub
            var stubDto = Stubs.BOEFormIBOEDTOs.Single();
            stubDto.Updateable = UpdateType.Upsert;

            // save item
            int createdItemId = 0;
            using (TransactionScope scope = new TransactionScope())
            {
                createdItemId = sut.Save(new BOEFormIBOEDTO[] { stubDto }).Values.Single();
                scope.Complete();
            }
            Assert.IsTrue(createdItemId > 0, "Failed to save the item!");

            BOEFormIBOEDTO createdItem = sut.GetById(createdItemId);

            // lets compare some values to make sure that
            // this is absolutely the right object!
            AssertAreEqual(stubDto, createdItem);
            
        }

        [TestMethod]
        public void Upsert_TestUpdate()
        {
            // get item
            BOEFormIBOEDTO testDto;
            using (var gbe = new GenBoeEntities())
            {
                // This is dangerous just taking the first IBOE in the table :(
                BOEFormIBOE item = gbe.BOEFormIBOEs.First();
                testDto = new BOEFormIBOEDTO
                {
                    Id = item.IBOEFormID,
                    Updateable = UpdateType.Upsert,
                    UpdateDateLong = item.UpdateDT.Ticks.ToString(),
                    WorkspaceId = item.WorkspaceID,
                    Revision = item.Revision,
                    UpdateDate = item.UpdateDT,
                    Version = item.FormVersion,
                    BusinessArea = "Some business area",
                    FormName = "Updated Form name",
                    Description = "Unit Test - This item was created and used to test updates for unit testing.",
                    BasisAndRationale = "BasisAndRationale -",
                    ProposalTitle = "ProposalTitle -",
                    ProposalDate = "02/22/2015", // sql server will truncate to 10 chars!
                    Poc = "poc -",
                    PocPhone = "pocphone -",
                    Approver = "approver -",
                    ApproverPhone = "aproverphone -"
                };

                testDto.ResourceIds.Add(6);
                testDto.ClinContractTypes.Add(new BoeFormClinContractTypeDTO { ClinId = 55, ContractType = 1018 });
            }

            // save item
            int updatedItemId = 0;
            using (TransactionScope scope = new TransactionScope())
            {
                updatedItemId = sut.Save(new BOEFormIBOEDTO[] { testDto }).Values.Single();
                scope.Complete();
            }
            Assert.IsTrue(updatedItemId > 0, "Failed to save the item!");

            // get item
            BOEFormIBOEDTO updatedItem = sut.GetById(updatedItemId);
            AssertAreEqual(testDto, updatedItem);

            updatedItem = sut.GetByWorkspaceId(testDto.WorkspaceId).FirstOrDefault(p => p.Id == updatedItemId);
            AssertAreEqual(testDto, updatedItem);
        }

        [TestMethod]
        public void DeleteTest()
        {
            int createdItemId = 0;

            // save a new item
            using (TransactionScope scope = new TransactionScope())
            {
                BOEFormIBOEDTO dto = Stubs.BOEFormIBOEDTOs.First();
                dto.Updateable = UpdateType.Upsert;
                createdItemId = sut.Save(new BOEFormIBOEDTO[] { dto }).Values.First();
                scope.Complete();
            }

            using (var gbe = new GenBoeEntities())
            {
                // read item
                BOEFormIBOE retrievedItem = gbe.BOEFormIBOEs.Single(x => x.IBOEFormID == createdItemId);

                // delete item
                int? deletedItemId;
                using (TransactionScope scope = new TransactionScope())
                {
                    deletedItemId = sut.Save(
                        new BOEFormIBOEDTO
                        {
                            Id = retrievedItem.IBOEFormID,
                            Updateable = UpdateType.Deleted,
                            UpdateDateLong = retrievedItem.UpdateDT.Ticks.ToString()
                        });
                    scope.Complete();
                }

                Assert.IsNotNull(retrievedItem); // it was there
                Assert.IsFalse(gbe.BOEFormIBOEs.Any(x => x.IBOEFormID == deletedItemId)); // now its gone
            }
        }

        private void AssertAreEqual(BOEFormIBOEDTO expected, BOEFormIBOEDTO actual)
        {
            Assert.AreEqual(expected.FormName, actual.FormName);
            Assert.AreEqual(expected.Revision, actual.Revision);
            Assert.AreEqual(expected.BusinessArea, actual.BusinessArea);
            Assert.AreEqual(expected.FormName, actual.FormName);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.ProposalTitle, actual.ProposalTitle);
            Assert.AreEqual(expected.ProposalDate, actual.ProposalDate);
            Assert.AreEqual(expected.Poc, actual.Poc);
            Assert.AreEqual(expected.PocPhone, actual.PocPhone);
            Assert.AreEqual(expected.Approver, actual.Approver);
            Assert.AreEqual(expected.ApproverPhone, actual.ApproverPhone);
            Assert.AreEqual(expected.Version, actual.Version);
            Assert.AreEqual(expected.WorkspaceId, actual.WorkspaceId);
            AssertEquality(expected.ResourceIds, actual.ResourceIds);
            AssertEquality(expected.ClinContractTypes, actual.ClinContractTypes);
        }

        internal static void AssertEquality(ICollection<int> resourceIds1, ICollection<int> resourceIds2)
        {
            if (ReferenceEquals(resourceIds1, null))
            {
                throw new ArgumentNullException(nameof(resourceIds1));
            }
            if (ReferenceEquals(resourceIds2, null))
            {
                throw new ArgumentNullException(nameof(resourceIds2));
            }

            Assert.AreEqual(resourceIds1.Count, resourceIds2.Count);
            foreach (int resourceId in resourceIds2)
            {
                Assert.IsTrue(resourceIds1.Contains(resourceId));
            }
        }

        internal static void AssertEquality(ICollection<BoeFormClinContractTypeDTO> clinContractTypes1, ICollection<BoeFormClinContractTypeDTO> clinContractTypes2)
        {
            if (ReferenceEquals(clinContractTypes1, null))
            {
                throw new ArgumentNullException(nameof(clinContractTypes1));
            }
            if (ReferenceEquals(clinContractTypes2, null))
            {
                throw new ArgumentNullException(nameof(clinContractTypes2));
            }

            Assert.AreEqual(clinContractTypes1.Count, clinContractTypes2.Count);
            foreach (BoeFormClinContractTypeDTO clinContractType in clinContractTypes2)
            {
                Assert.IsTrue(clinContractTypes1.Any(c => c.ClinId == clinContractType.ClinId && c.ContractType == clinContractType.ContractType));
            }
        }


    }
}
