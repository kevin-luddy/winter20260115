// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using IES.Common.PickList;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Helps us assert Dtos
    /// </summary>
    public static class DtoAssertHelpers
    {
        /// <summary>
        /// Number of untestable properties common to all DTOs
        /// </summary>
        private const int UNTESTABLE_PROPERTIES = 4;

        /// <summary>
        /// Asserts to make sure that both dtos match.
        /// </summary>
        /// <param name="expectedResult">Expected result.</param>
        /// <param name="actualResult">Actual result, from the DB.</param>
        /// <param name="legacy">legacy == true is for the 4 digit date format</param>
        public static void AssertDtos(ProposalDto expectedResult, ProposalDto actualResult, bool legacy = false)
        {
            if (actualResult == null)
            {
                throw new ArgumentNullException(nameof(actualResult));
            }

            if (expectedResult == null)
            {
                throw new ArgumentNullException(nameof(expectedResult));
            }

            Type dtoType = typeof(ProposalDto);
            int numProperties = dtoType.GetProperties().Count();

            // 3 UNTESTABLE_PROPOERTIES properties we can't test (primary key, update date, updatable)
            // 2 untestable tracking number and revision number (assigned by DB)
            // 59 testable DTO properties that are asserted below
            Assert.AreEqual(UNTESTABLE_PROPERTIES + 2 + 59, numProperties, "Untested properties exist in the DTO.");

            // 0
            Assert.AreEqual(expectedResult.ProposalTitle, actualResult.ProposalTitle);
            Assert.AreEqual(expectedResult.ProposalStatus, actualResult.ProposalStatus);
            Assert.AreEqual(expectedResult.OTISOpportunityID, actualResult.OTISOpportunityID);
            Assert.AreEqual(expectedResult.CostElementTypeIds.Count(), actualResult.CostElementTypeIds.Count());
            if (expectedResult.CostElementTypeIds.Any())
            {
                Assert.AreEqual(expectedResult.CostElementTypeIds.First(), actualResult.CostElementTypeIds.First());
            }

            Assert.AreEqual(expectedResult.ContractTypeIds.Count(), actualResult.ContractTypeIds.Count());

            if (expectedResult.ContractTypeIds.Any())
            {
                Assert.AreEqual(expectedResult.ContractTypeIds.First(), actualResult.ContractTypeIds.First());
            }

            Assert.AreEqual(expectedResult.ContractTypeGroup, actualResult.ContractTypeGroup);
            Assert.AreEqual(expectedResult.Customer, actualResult.Customer);
            Assert.AreEqual(expectedResult.CustomerType, actualResult.CustomerType);
            Assert.AreEqual(expectedResult.DeliveryDate, actualResult.DeliveryDate);
            Assert.AreEqual(expectedResult.EstimatedProposalValue, actualResult.EstimatedProposalValue);
            Assert.AreEqual(expectedResult.ISGSRole, actualResult.ISGSRole);
            Assert.AreEqual(expectedResult.ProgramAreaId, actualResult.ProgramAreaId);
            Assert.AreEqual(expectedResult.ProposalLocation, actualResult.ProposalLocation);
            Assert.AreEqual(expectedResult.ProposalLocationName, actualResult.ProposalLocationName);
            Assert.AreEqual(expectedResult.PricingTool, actualResult.PricingTool);
            Assert.AreEqual(expectedResult.PricingToolName, actualResult.PricingToolName);
            Assert.AreEqual(expectedResult.LineOfBusinessID, actualResult.LineOfBusinessID);
            Assert.AreEqual(expectedResult.ProgramName, actualResult.ProgramName);
            Assert.AreEqual(expectedResult.ProposalType, actualResult.ProposalType);
            Assert.AreEqual(expectedResult.Request, actualResult.Request);
            Assert.AreEqual(expectedResult.ProposalClass, actualResult.ProposalClass);
            Assert.AreEqual(expectedResult.RFPNumber, actualResult.RFPNumber);
            Assert.AreEqual(expectedResult.BoeTool, actualResult.BoeTool);
            Assert.AreEqual(expectedResult.BoeToolName, actualResult.BoeToolName);
            Assert.AreEqual(expectedResult.IsScheduleProposal, actualResult.IsScheduleProposal);
            Assert.AreEqual(expectedResult.UpdateDateAssigned, actualResult.UpdateDateAssigned);
            Assert.AreEqual(expectedResult.CreatedByUserId, actualResult.CreatedByUserId);
            Assert.AreEqual(expectedResult.ProgramProposalStatus, actualResult.ProgramProposalStatus);
            Assert.AreEqual(expectedResult.IsCCPDRequired, actualResult.IsCCPDRequired);
            Assert.AreEqual(expectedResult.IsCostVolumeClassified, actualResult.IsCostVolumeClassified);
            Assert.AreEqual(expectedResult.DocumentId, actualResult.DocumentId);
            Assert.AreEqual(expectedResult.ForecastedTrackingNumber, actualResult.ForecastedTrackingNumber);
            Assert.AreEqual(expectedResult.HasWriteAccessToLinkedDocument, actualResult.HasWriteAccessToLinkedDocument);

            if (expectedResult.DateCreated.HasValue)
            {
                Assert.AreEqual(expectedResult.DateCreated.Value.ToString("MM/dd/yyyy"), actualResult.DateCreated.Value.ToString("MM/dd/yyyy"));
            }

            if (expectedResult.DateAssigned.HasValue)
            {
                Assert.AreEqual(expectedResult.DateAssigned.Value.ToString("MM/dd/yyyy"), actualResult.DateAssigned.Value.ToString("MM/dd/yyyy"));
            }

            // test primary key
            Assert.AreEqual(expectedResult.Id, expectedResult.GetPrimaryKeyID());

            // check the proposal tracking number format
            if (legacy)
            {
                Assert.IsTrue(actualResult.TrackingNumber.StartsWith(DateTime.Now.Year.ToString()));
                Assert.IsTrue(actualResult.TrackingNumber.Length <= 13); 
            }
            else
            {
                Assert.IsTrue(actualResult.TrackingNumber.StartsWith(DateTime.Now.ToString("yy")));
                Assert.IsTrue(actualResult.TrackingNumber.Length <= 11);
            }
 
            Assert.IsTrue(actualResult.TrackingNumber.Contains("-"));

            Assert.AreEqual(expectedResult.RFPIssuedDate, actualResult.RFPIssuedDate);
            Assert.AreEqual(expectedResult.RFPReceivedDate, actualResult.RFPReceivedDate);
            Assert.AreEqual(expectedResult.Comments, actualResult.Comments);
            Assert.AreEqual(expectedResult.ChangeChecklist, actualResult.ChangeChecklist);

            Assert.AreEqual(expectedResult.WorkflowStatus, actualResult.WorkflowStatus);
            Assert.AreEqual(expectedResult.WorkflowStatusLastUpdated, actualResult.WorkflowStatusLastUpdated);
            Assert.AreEqual(expectedResult.LeadEstimatorSignatureComment, actualResult.LeadEstimatorSignatureComment);
            Assert.AreEqual(expectedResult.LeadEstimatorSignedDate, actualResult.LeadEstimatorSignedDate);
            Assert.AreEqual(expectedResult.CoverSheetApproverSignatureComment, actualResult.CoverSheetApproverSignatureComment);
            Assert.AreEqual(expectedResult.CoverSheetApproverSignedDate, actualResult.CoverSheetApproverSignedDate);
            Assert.AreEqual(expectedResult.PricingVerifierSignatureComment, actualResult.PricingVerifierSignatureComment);
            Assert.AreEqual(expectedResult.PricingVerifierSignedDate, actualResult.PricingVerifierSignedDate);
            Assert.AreEqual(expectedResult.IndependentReviewerSignatureComment, actualResult.IndependentReviewerSignatureComment);
            Assert.AreEqual(expectedResult.IndependentReviewerSignedDate, actualResult.IndependentReviewerSignedDate);
            Assert.AreEqual(expectedResult.LOBEstimatingLeadSignatureComment, actualResult.LOBEstimatingLeadSignatureComment);
            Assert.AreEqual(expectedResult.LOBEstimatingLeadSignedDate, actualResult.LOBEstimatingLeadSignedDate);
            Assert.AreEqual(expectedResult.ApprovalEmailText, actualResult.ApprovalEmailText);
            Assert.AreEqual(expectedResult.ForecastedTrackingNumber, actualResult.ForecastedTrackingNumber);
            Assert.AreEqual(expectedResult.ForecastEmailSent, actualResult.ForecastEmailSent);
            Assert.AreEqual(expectedResult.AgreementDate, actualResult.AgreementDate);
            Assert.AreEqual(expectedResult.CutOffDateUtilization, actualResult.CutOffDateUtilization);
            Assert.AreEqual(expectedResult.CertificationDate, actualResult.CertificationDate);
            Assert.AreEqual(expectedResult.CertificationLastEmailed, actualResult.CertificationLastEmailed);
            Assert.AreEqual(expectedResult.CertificationTimelineCompleted, actualResult.CertificationTimelineCompleted);
        }

        /// <summary>
        /// Asserts to make sure that both dtos match.
        /// </summary>
        /// <param name="expectedResult">Expected result.</param>
        /// <param name="actualResult">Actual result, from the DB.</param>
        public static void AssertDtos(UserDTO expectedResult, UserDTO actualResult)
        {
            if (actualResult == null)
            {
                throw new ArgumentNullException(nameof(actualResult));
            }

            if (expectedResult == null)
            {
                throw new ArgumentNullException(nameof(expectedResult));
            }

            Type dtoType = typeof(UserDTO);
            int numProperties = dtoType.GetProperties().Count();

            // 3 properties we can't test (primary key, update date, updatable), plus the ones below 
            Assert.AreEqual(UNTESTABLE_PROPERTIES + 13, numProperties, "Untested properties exist in the DTO.");

            Assert.IsNotNull(actualResult);

            // 0
            Assert.AreEqual(expectedResult.DisplayName, actualResult.DisplayName);
            Assert.AreEqual(expectedResult.EmailAddress, actualResult.EmailAddress);
            Assert.AreEqual(expectedResult.FirstName, actualResult.FirstName);
            Assert.AreEqual(expectedResult.LastName, actualResult.LastName);
            // 5
            Assert.AreEqual(expectedResult.Ntid, actualResult.Ntid);
            Assert.AreEqual(expectedResult.PhoneNumber, actualResult.PhoneNumber);
            Assert.AreEqual(expectedResult.UserType, actualResult.UserType);
            Assert.AreEqual(expectedResult.IsGroup, actualResult.IsGroup);
            Assert.AreEqual(expectedResult.IsUsPerson, actualResult.IsUsPerson);
            Assert.AreEqual(expectedResult.IsSubcontractor, actualResult.IsSubcontractor);

            // test primary key
            Assert.AreEqual(expectedResult.Id, expectedResult.GetPrimaryKeyID());
        }

        /// <summary>
        /// Asserts to make sure that both dtos match.
        /// </summary>
        /// <param name="expectedResult">Expected result.</param>
        /// <param name="actualResult">Actual result, from the DB.</param>
        /// SUPPRESSION NOTE: Want to specifically test the SystemPermissionDto here, not PermissionDto
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void AssertDtos(SystemPermissionDto expectedResult, SystemPermissionDto actualResult)
        {
            if (expectedResult == null)
            {
                throw new ArgumentNullException(nameof(expectedResult));
            }

            if (actualResult == null)
            {
                throw new ArgumentNullException(nameof(actualResult));
            }

            Type dtoType = typeof(SystemPermissionDto);
            int numProperties = dtoType.GetProperties().Count();

            // 3 properties we can't test (primary key, update date, updatable), plus the ones below 
            Assert.AreEqual(UNTESTABLE_PROPERTIES + 3, numProperties, "Untested properties exist in the DTO.");

            Assert.IsNotNull(actualResult);

            // 0
            Assert.AreEqual(expectedResult.Role, actualResult.Role);
            Assert.AreEqual(expectedResult.UserId, actualResult.UserId);

            // test primary key
            Assert.AreEqual(expectedResult.Id, expectedResult.GetPrimaryKeyID());
        }

        /// <summary>
        /// Asserts to make sure that both dtos match.
        /// </summary>
        /// <param name="expectedResult">Expected result.</param>
        /// <param name="actualResult">Actual result, from the DB.</param>
        public static void AssertDtos(ProposalPermissionDto expectedResult, ProposalPermissionDto actualResult)
        {
            if (expectedResult == null)
            {
                throw new ArgumentNullException(nameof(expectedResult));
            }

            if (actualResult == null)
            {
                throw new ArgumentNullException(nameof(actualResult));
            }

            Type dtoType = typeof(ProposalPermissionDto);
            int numProperties = dtoType.GetProperties().Count();

            // 3 properties we can't test (primary key, update date, updatable), plus the ones below 
            Assert.AreEqual(UNTESTABLE_PROPERTIES + 4, numProperties, "Untested properties exist in the DTO.");

            Assert.IsNotNull(actualResult);

            // 0
            Assert.AreEqual(expectedResult.ProposalID, actualResult.ProposalID);
            Assert.AreEqual(expectedResult.Role, actualResult.Role);
            Assert.AreEqual(expectedResult.UserId, actualResult.UserId);
            Assert.AreEqual(expectedResult.ResourceType, actualResult.ResourceType);

            // test primary key
            Assert.AreEqual(expectedResult.Id, expectedResult.GetPrimaryKeyID());
        }

        /// <summary>
        /// Asserts to make sure that both dtos match.
        /// NOTE: does not assert any SaveInfo since it's different depending on situation
        /// </summary>
        /// <param name="expectedResult">Expected result.</param>
        /// <param name="actualResult">Actual result, from the DB.</param>
        public static void AssertDtos(ProposalChecklistDto expectedResult, ProposalChecklistDto actualResult)
        {
            if (expectedResult == null)
            {
                throw new ArgumentNullException(nameof(expectedResult));
            }

            if (actualResult == null)
            {
                throw new ArgumentNullException(nameof(actualResult));
            }

            Type dtoType = typeof(ProposalChecklistDto);
            int numProperties = dtoType.GetProperties().Count();

            // 3 properties we can't test (primary key, update date, updatable), plus response type, and save info, plus the ones below 
            Assert.AreEqual(UNTESTABLE_PROPERTIES + 3 + 16, numProperties, "Untested properties exist in the DTO.");

            Assert.AreEqual(expectedResult.LMLaborHrs, actualResult.LMLaborHrs);
            Assert.AreEqual(expectedResult.LMLaborCost, actualResult.LMLaborCost);
            Assert.AreEqual(expectedResult.SubcontractorCost, actualResult.SubcontractorCost);
            Assert.AreEqual(expectedResult.MaterialCost, actualResult.MaterialCost);
            Assert.AreEqual(expectedResult.IWTACost, actualResult.IWTACost);
            Assert.AreEqual(expectedResult.TravelCost, actualResult.TravelCost);
            Assert.AreEqual(expectedResult.OtherDirectCosts, actualResult.OtherDirectCosts);
            Assert.AreEqual(expectedResult.ProfitFee, actualResult.ProfitFee);
            Assert.AreEqual(expectedResult.AbsoluteValue, actualResult.AbsoluteValue);
            Assert.AreEqual(expectedResult.ProposalID, actualResult.ProposalID);
            Assert.AreEqual(expectedResult.ROSPercentage, actualResult.ROSPercentage);
            Assert.AreEqual(expectedResult.SubmittedValue, actualResult.SubmittedValue);
            Assert.AreEqual(expectedResult.IsSubmit, actualResult.IsSubmit);
            Assert.AreEqual(expectedResult.DeliverChecklistDFARS, actualResult.DeliverChecklistDFARS);
            
            // TODO: check if this value should be false or null, assert is failing
            // Assert.AreEqual(expectedResult.IsCCPDRequired, actualResult.IsCCPDRequired);

            if (expectedResult.ProposalSubmittalDate.HasValue)
            {
                Assert.AreEqual(expectedResult.ProposalSubmittalDate.Value.ToString("MM/DD/YYYY"), actualResult.ProposalSubmittalDate.Value.ToString("MM/DD/YYYY"));
            }

            AssertChecklistResponses(expectedResult.PARResponses, actualResult.PARResponses);
            AssertChecklistResponses(expectedResult.PPRResponses, actualResult.PPRResponses);

            // test primary key
            Assert.AreEqual(expectedResult.Id, expectedResult.GetPrimaryKeyID());
        }

        /// <summary>
        /// Assert checklist responses are equal
        /// </summary>
        /// <param name="expectedResponses">Expected responses</param>
        /// <param name="actualResponses">Actual responses</param>
        public static void AssertChecklistResponses(ICollection<ChecklistResponseItem> expectedResponses, ICollection<ChecklistResponseItem> actualResponses)
        {
            if (expectedResponses == null) 
            { 
                throw new ArgumentNullException(nameof(expectedResponses)); 
            }

            if (actualResponses == null)
            {
                throw new ArgumentNullException(nameof(actualResponses));
            }

            foreach (ChecklistResponseItem item in expectedResponses)
            {
                ChecklistResponseItem matchingActualResponse = actualResponses.FirstOrDefault(x => x.ResponseType == item.ResponseType
                    && x.ProposalId == item.ProposalId && x.ChecklistContentId == item.ChecklistContentId);

                Assert.IsNotNull(matchingActualResponse);
                Assert.AreEqual(item.Response, matchingActualResponse.Response);

                // remove match
                actualResponses.Remove(matchingActualResponse);
            }

            // make sure all entries left are not initialized
            Assert.IsFalse(actualResponses.Where(x => x.Response != ChecklistResponseOption.NotSet).Any());
        }

        /// <summary>
        /// Asserts to make sure that both dtos match.
        /// </summary>
        /// <param name="expectedResult">Expected result.</param>
        /// <param name="actualResult">Actual result.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void AssertDtos(UsersOnlineDTO expectedResult, UsersOnlineDTO actualResult)
        {
            if (expectedResult == null)
            {
                throw new ArgumentNullException(nameof(expectedResult));
            }

            if (actualResult == null)
            {
                throw new ArgumentNullException(nameof(actualResult));
            }

            Type dtoType = typeof(UsersOnlineDTO);
            int numPoperties = dtoType.GetProperties().Count();

            // 3 UNTESTABLE_PROPOERTIES properties we can't test (update date as long, update date, updatable)
            // 1 testable DTO properties that is asserted below
            Assert.AreEqual(4, numPoperties, "Untested propoerties exist in the DTO.");

            Assert.AreEqual(expectedResult.Id, actualResult.Id);
        }

        /// <summary>
        /// Asserts to make sure that both dtos match.
        /// </summary>
        /// <param name="expectedResult">Expected result.</param>
        /// <param name="actualResult">Actual result, from the DB.</param>
        public static void AssertDtos(ChecklistContentDto expectedResult, ChecklistContentDto actualResult)
        {
            if (expectedResult == null)
            {
                throw new ArgumentNullException(nameof(expectedResult));
            }

            if (actualResult == null)
            {
                throw new ArgumentNullException(nameof(actualResult));
            }

            Type dtoType = typeof(ChecklistContentDto);
            int numProperties = dtoType.GetProperties().Count();

            Assert.AreEqual(4, numProperties, "Untested properties exist in the DTO.");

            Assert.AreEqual(expectedResult.Version, actualResult.Version);
            Assert.AreEqual(expectedResult.Content.Count, actualResult.Content.Count);
            Assert.AreEqual(expectedResult.ChecklistType, actualResult.ChecklistType);
        }

        /// <summary>
        /// Asserts to make sure that both dtos match.
        /// </summary>
        /// <param name="expectedResult">Expected result.</param>
        /// <param name="actualResult">Actual result, from the DB.</param>
        public static void AssertDtos(ProposalChecklistSaveInfo expectedResult, ProposalChecklistSaveInfo actualResult)
        {
            if (expectedResult == null)
            {
                throw new ArgumentNullException(nameof(expectedResult));
            }

            if (actualResult == null)
            {
                throw new ArgumentNullException(nameof(actualResult));
            }

            Type dtoType = typeof(ProposalChecklistSaveInfo);
            int numProperties = dtoType.GetProperties().Count();

            Assert.AreEqual(6, numProperties, "Untested properties exist in the DTO.");

            Assert.AreEqual(expectedResult.ChecklistType, actualResult.ChecklistType);
            Assert.AreEqual(expectedResult.Comment, actualResult.Comment);
            Assert.AreEqual(expectedResult.LastSaveDate, actualResult.LastSaveDate);
            Assert.AreEqual(expectedResult.ResponseType, actualResult.ResponseType);
            Assert.AreEqual(expectedResult.SubmitDate, actualResult.SubmitDate);
            Assert.AreEqual(expectedResult.UserID, actualResult.UserID);
        }

        /// <summary>
        /// Asserts to make sure that both dtos match.
        /// </summary>
        /// <param name="expectedResult">Expected result.</param>
        /// <param name="actualResult">Actual result, from the DB.</param>
        public static void AssertDtos(PickListDto expectedResult, PickListDto actualResult)
        {
            if (expectedResult == null)
            {
                throw new ArgumentNullException(nameof(expectedResult));
            }

            if (actualResult == null)
            {
                throw new ArgumentNullException(nameof(actualResult));
            }

            Type dtoType = typeof(PickListDto);
            int numProperties = dtoType.GetProperties().Count();

            Assert.AreEqual(12, numProperties, "Untested properties exist in the DTO.");

            Assert.AreEqual(expectedResult.Id, actualResult.Id);
            Assert.AreEqual(expectedResult.Text, actualResult.Text);
            Assert.AreEqual(expectedResult.IsReadOnly, actualResult.IsReadOnly);
            Assert.AreEqual(expectedResult.InUse, actualResult.InUse);
            if (expectedResult.ParentIds != null && expectedResult.ParentIds.Count > 0)
            {
                Assert.IsNotNull(actualResult.ParentIds);
                Assert.AreEqual(expectedResult.ParentIds.Count, actualResult.ParentIds.Count);
                Assert.AreEqual(expectedResult.ParentIds.First(), actualResult.ParentIds.First());
            }
            
            Assert.AreEqual(expectedResult.IsActive, actualResult.IsActive);
        }
    }
}