// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Loader
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Transactions;
	using ActionLogic;
	using ActionLogic.Email;
	using ActionLogic.Mediator;
	using DataBridge.Common.Security;
	using GenTRAC.DataBridge.DTO;
	using GenTRAC.DataBridge.DTO.Permission;
	using GenTRAC.Models;
	using IES.Common;
	using Microsoft.Practices.Unity;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;
	using Objects;

	/// <summary>
	/// Test class for the ProposalLoaderTest
	/// </summary>
	[TestClass]
	public class ProposalLoaderTest
	{
		/// <summary>
		/// Test data
		/// </summary>
		private TestData testData = TestData.GetInstance();

		/// <summary>
		/// Proposal loader
		/// </summary>
		/// <returns>loader</returns>
		private ProposalLoader CreateSystem()
		{
			return new ProposalLoader();
		}

		/// <summary>
		/// Get all proposal IDs
		/// </summary>
		[TestMethod]
		public void L_GetAllProposalIDs()
		{
			ProposalLoader sut = this.CreateSystem();

			ICollection<int> beforeProposalIds = sut.GetAllIds();
			this.testData.GetProposal(true);
			ICollection<int> afterProposalIds = sut.GetAllIds();

			Assert.AreEqual(beforeProposalIds.Count + 1, afterProposalIds.Count);
		}

		/// <summary>
		/// Get Proposal Id by Tracking Id test - Forecast Version
		/// </summary>
		[TestMethod]
		public void L_GetProposalIdByTrackingIdForecastTest()
		{
			ProposalLoader sut = this.CreateSystem();
			ProposalDto p = TestData.GetInstance().GetProposal(true, null, null, true);
			int fid = sut.GetIdByTrackingNumber(p.ForecastedTrackingNumber);

			Assert.AreEqual(p.Id, fid);

			fid = sut.GetIdByTrackingNumber(p.ForecastedTrackingNumber.ToLower());

			Assert.AreEqual(p.Id, fid);

			fid = sut.GetIdByTrackingNumber(p.ForecastedTrackingNumber.ToUpper());

			Assert.AreEqual(p.Id, fid);
		}

		/// <summary>
		/// Save proposal, then get by id
		/// </summary>
		[TestMethod]
		public void L_SaveProposalAndGetProposalByID()
		{
			ProposalLoader sut = this.CreateSystem();

			string proposalIdentifier = TestData.CreateRandomWord(6);
			ProposalDto newProposal = new ProposalDto()
			{
				Id = -1,
				ProposalStatus = ProposalStatus.InProgress,
				ProposalTitle = "Mock" + proposalIdentifier,
				OTISOpportunityID = proposalIdentifier,
				UpdateDate = DateTime.Now,
				Updateable = UpdateType.Upsert,
				BoeTool = BOETool.Excel,
				ContractTypeGroup = 1, // ContractTypeGroup.CP,
				ContractTypeIds = new List<int>() { 1, 4 }, // CostPlusAwardFee, FirmFixedPrice
				CostElementTypeIds = new List<int> { (int)CostElementType.Labor },
				Customer = proposalIdentifier,
				CustomerType = CustomerType.InternationalForeignMilitarySaleUSGovt,
				DeliveryDate = new DateTime(2013, 6, 1),
				EstimatedProposalValue = 0,
				ISGSRole = ISGSRole.Prime,
				ProgramAreaId = 46,
				ProposalLocation = ProposalLocation.ValleyForgePA,
				PricingTool = PricingTool.Excel,
				LineOfBusinessID = 10,
				ProgramName = proposalIdentifier,
				ProposalType = 3,
				Request = 1,
				ProposalClass = 1,
				RFPNumber = proposalIdentifier,
				IsScheduleProposal = false,
				DateAssigned = DateTime.Now,
				DateCreated = DateTime.Now,
				RFPIssuedDate = new DateTime(2013, 4, 4),
				RFPReceivedDate = new DateTime(2013, 5, 5),
				Comments = "my comment",
				ChangeChecklist = false,
				ProgramProposalStatus = ProgramProposalStatus.UnderStrategicReviewISGS,
				ApprovalEmailText = "This is extra text for the approval email",
				IsCCPDRequired = true,
				IsCostVolumeClassified = false,
				DocumentId = 5,
				AgreementDate = new DateTime(2013, 6, 6),
				CutOffDateUtilization = CutOffDateUtilization.NoRequestDenied,
				CertificationDate = new DateTime(2013, 6, 7),
				CertificationLastEmailed = new DateTime(2013, 6, 8),
				CertificationTimelineCompleted = new DateTime(2013, 6, 9),
				RevisionOfId = null,
				ReasonCertificationNotRequired = ReasonCertificationNotRequired.Other,
				OtherReasonComment = "Other comment",
				ProposalSetupComments = "Setup comment",
				ModExecutedLastEmailed = DateTime.UtcNow,
				ContractActionType = ContractActionType.Other,
				ContractActionTypeOtherText = "Other text.",
				CostVolumeTool = CostVolumeTool.ACV
			};

			int? newProposalID;

			using (TransactionScope scope = new TransactionScope())
			{
				newProposalID = sut.Save(newProposal);
				scope.Complete();
			}

			ProposalDto toTest = sut.GetById(newProposalID.Value);

			DtoAssertHelpers.AssertDtos(newProposal, toTest);

			// Test update Forecast Email Sent

			using (TransactionScope scope = new TransactionScope())
			{
				sut.UpdateProposalForecastEmailSent(toTest.Id, toTest.UpdateDate);
				scope.Complete();
			}

			newProposal.ForecastEmailSent = true;
			toTest = sut.GetById(newProposalID.Value);
			DtoAssertHelpers.AssertDtos(newProposal, toTest);

			// test delete
			toTest.Updateable = UpdateType.Deleted;
			int? deletedProposalId;

			using (TransactionScope scope = new TransactionScope())
			{
				deletedProposalId = sut.Save(toTest);
				scope.Complete();
			}

			Assert.AreEqual(newProposalID, deletedProposalId);
		}

		/// <summary>
		/// Verify the correct behavior for ProposalLocationName, PricingToolName and BOEToolName fields during save and retrieve
		/// </summary>
		[TestMethod]
		public void L_SaveProposalTestOtherFieldHandling()
		{
			ProposalLoader sut = this.CreateSystem();
			string boeToolName = "Big BOE";
			string pricingToolName = "Big Pricer";
			string costVolumeToolName = "Big Cost Volume";
			string proposalLocationName = "Kansas City, MO";

			string proposalIdentifier = TestData.CreateRandomWord(6);
			ProposalDto newProposal = new ProposalDto()
			{
				Id = -1,
				ProposalStatus = ProposalStatus.InProgress,
				ProposalTitle = "Mock" + proposalIdentifier,
				OTISOpportunityID = proposalIdentifier,
				UpdateDate = DateTime.Now,
				Updateable = UpdateType.Upsert,
				BoeTool = BOETool.Other,
				BoeToolName = boeToolName,
				ContractTypeGroup = 1, // ContractTypeGroup.CP,
				ContractTypeIds = new List<int>() { 1, 4 }, // CostPlusAwardFee, FirmFixedPrice
				CostElementTypeIds = new List<int> { (int)CostElementType.Labor },
				Customer = proposalIdentifier,
				CustomerType = CustomerType.InternationalForeignMilitarySaleUSGovt,
				DeliveryDate = new DateTime(2013, 6, 1),
				EstimatedProposalValue = 0,
				ISGSRole = ISGSRole.Prime,
				ProgramAreaId = 46,
				ProposalLocation = ProposalLocation.Other,
				ProposalLocationName = proposalLocationName,
				PricingTool = PricingTool.Other,
				PricingToolName = pricingToolName,
				LineOfBusinessID = 10,
				ProgramName = proposalIdentifier,
				ProposalType = 3,
				Request = 1,
				ProposalClass = 2,
				RFPNumber = proposalIdentifier,
				IsScheduleProposal = false,
				DateAssigned = DateTime.Now,
				DateCreated = DateTime.Now,
				RFPIssuedDate = new DateTime(2013, 4, 4),
				RFPReceivedDate = new DateTime(2013, 5, 5),
				Comments = "my comment",
				ProgramProposalStatus = ProgramProposalStatus.UnderStrategicReviewISGS,
				IsCostVolumeClassified = false,
				CostVolumeTool = CostVolumeTool.Other,
				CostVolumeToolName = costVolumeToolName
			};

			int? newProposalID;

			// Save the proposal
			using (TransactionScope scope = new TransactionScope())
			{
				newProposalID = sut.Save(newProposal);
				scope.Complete();
			}

			// Verify that the Name fields are populated with the expected value
			Assert.AreEqual(ProposalLocation.Other, newProposal.ProposalLocation);
			Assert.AreEqual(proposalLocationName, newProposal.ProposalLocationName);
			Assert.AreEqual(PricingTool.Other, newProposal.PricingTool);
			Assert.AreEqual(pricingToolName, newProposal.PricingToolName);
			Assert.AreEqual(BOETool.Other, newProposal.BoeTool);
			Assert.AreEqual(boeToolName, newProposal.BoeToolName);
			Assert.AreEqual(CostVolumeTool.Other, newProposal.CostVolumeTool);
			Assert.AreEqual(costVolumeToolName, newProposal.CostVolumeToolName);

			// Make the proposal updateable
			newProposal.Id = (int)newProposalID;
			ProposalDto updatedProposal = sut.GetById((int)newProposalID);
			newProposal.UpdateDate = updatedProposal.UpdateDate;

			// Update the proposal to non-Other fields
			newProposal.ProposalLocation = ProposalLocation.ValleyForgePA;
			newProposal.PricingTool = PricingTool.Excel;
			newProposal.BoeTool = BOETool.ABE;
			newProposal.CostVolumeTool = CostVolumeTool.ACV;

			// Save the proposal
			using (TransactionScope scope = new TransactionScope())
			{
				newProposalID = sut.Save(newProposal);
				scope.Complete();
			}

			// Verify that the Name fields are now unpopulated
			Assert.AreEqual(ProposalLocation.ValleyForgePA, newProposal.ProposalLocation);
			Assert.IsTrue(string.IsNullOrEmpty(newProposal.ProposalLocationName));
			Assert.AreEqual(PricingTool.Excel, newProposal.PricingTool);
			Assert.IsTrue(string.IsNullOrEmpty(newProposal.PricingToolName));
			Assert.AreEqual(BOETool.ABE, newProposal.BoeTool);
			Assert.IsTrue(string.IsNullOrEmpty(newProposal.BoeToolName));
			Assert.AreEqual(CostVolumeTool.ACV, newProposal.CostVolumeTool);
			Assert.IsTrue(string.IsNullOrEmpty(newProposal.CostVolumeToolName));

			// add the created proposal to TestData so it is cleaned up
			this.testData.AddProposal(newProposal);
		}

		/// <summary>
		/// Get proposal ID by tracking number
		/// </summary>
		[TestMethod]
		public void L_GetProposalIDByTrackingNumber()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto testProposal = this.testData.GetProposal();

			int toTest = sut.GetIdByTrackingNumber(testProposal.TrackingNumber);

			Assert.AreEqual(testProposal.Id, toTest);

			// Test one that doesn't exist, and make sure that it doesn't start with F
			Assert.AreEqual(-1, sut.GetIdByTrackingNumber("1" + TestData.CreateRandomWord(9)));
		}

		/// <summary>
		/// Test Get Proposal Ids by user
		/// </summary>
		[TestMethod]
		public void L_GetProposalIdsByUser()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);
			UserDTO user = this.testData.GetUser(inCreateNew: true);
			this.testData.GetProposalPermission(true, new ProposalPermissionDto()
			{
				ProposalID = proposal.Id,
				Role = PtmRole.BackupPricer,
				UserId = user.Id,
				Updateable = UpdateType.Upsert
			});

			ICollection<int> proposalIds = sut.GetProposalIdsByUser(new List<int>() { user.Id });
			Assert.AreEqual(1, proposalIds.Count);
			Assert.IsTrue(proposalIds.Contains(proposal.Id));
		}

		/// <summary>
		/// Test Get Proposals by user
		/// </summary>
		[TestMethod]
		public void L_GetProposalsByUser()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);
			UserDTO user = this.testData.GetUser(inCreateNew: true);
			this.testData.GetProposalPermission(true, new ProposalPermissionDto()
			{
				ProposalID = proposal.Id,
				Role = PtmRole.BackupPricer,
				UserId = user.Id,
				Updateable = UpdateType.Upsert
			});

			ICollection<HomeProposalViewDto> proposals = sut.GetProposalsByUser(null, null, null, null, user.Ntid, false, null, null);
			Assert.AreEqual(1, proposals.Count);
			Assert.IsTrue(proposals.Any(p => p.ProposalId == proposal.Id));
		}

		/// <summary>
		/// Test Get Proposals by user with/without showProposalsForMyOrganization flag set
		/// </summary>
		[TestMethod]
		public void L_GetProposalsByUser_ShowProposalsForMyOrganization()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);

			UserDTO user0 = this.testData.GetUser(inCreateNew: true);
			this.testData.GetProposalPermission(true, new ProposalPermissionDto()
			{
				ProposalID = proposal.Id,
				Role = PtmRole.BackupPricer,
				UserId = user0.Id,
				Updateable = UpdateType.Upsert
			});

			UserDTO user1 = this.testData.GetUser(true, new UserDTO()
			{
				Id = -1,
				Ntid = "securityusertest" + TestData.CreateRandomWord(3),
				Updateable = UpdateType.Upsert
			});

			UserDTO user2 = this.testData.GetUser(true, new UserDTO()
			{
				Id = -1,
				Ntid = "securityusertest" + TestData.CreateRandomWord(3),
				Updateable = UpdateType.Upsert
			});

			this.testData.GetPermission(true, new SystemPermissionDto()
			{
				Id = -1,
				Role = PtmRole.Viewer,
				Updateable = UpdateType.Upsert,
				UserId = user0.Id,
				LineOfBusinessIDs = new Collection<int>() { 9 }
			});

			this.testData.GetPermission(true, new SystemPermissionDto()
			{
				Id = -1,
				Role = PtmRole.Viewer,
				Updateable = UpdateType.Upsert,
				UserId = user1.Id,
				LineOfBusinessIDs = new Collection<int>() { 9 }     // Give Viewer access to LOB 9 (does not match LOB in proposal)
			});

			this.testData.GetPermission(true, new SystemPermissionDto()
			{
				Id = -1,
				Role = PtmRole.Viewer,
				Updateable = UpdateType.Upsert,
				UserId = user2.Id,
				LineOfBusinessIDs = new Collection<int>() { 10 }    // Give Viewer access to LOB 10 (matches LOB in proposal)
			});

			string userAndGroupXml = "<ROOT><id>{0}</id></ROOT>";
			string userAndGroupIds0 = string.Format(userAndGroupXml, user0.Ntid);
			string userAndGroupIds1 = string.Format(userAndGroupXml, user1.Ntid);
			string userAndGroupIds2 = string.Format(userAndGroupXml, user2.Ntid);

			// Test user0 with ShowMyOwnProposals - Should find proposal because user is Backup Pricer
			ICollection<HomeProposalViewDto> proposals = sut.GetProposalsByUser(null, null, null, null, user0.Ntid, false, null, null);
			Assert.AreEqual(1, proposals.Count);
			Assert.IsTrue(proposals.Any(p => p.ProposalId == proposal.Id));

			// Test user0 with showProposalsForMyOrganization - Should find proposal because user is Backup Pricer
			proposals = sut.GetProposalsByUser(null, null, null, null, user0.Ntid, true, userAndGroupIds0, null);
			Assert.IsTrue(proposals.Count >= 1);
			Assert.IsTrue(proposals.Any(p => p.ProposalId == proposal.Id));

			// Test user1 with ShowMyOwnProposals - Should not find proposal
			proposals = sut.GetProposalsByUser(null, null, null, null, user1.Ntid, false, null, null);
			Assert.AreEqual(0, proposals.Count);

			// Test user1 with showProposalsForMyOrganization - Should not find proposal (user is Viewer, but wrong LOB)
			proposals = sut.GetProposalsByUser(null, null, null, null, user1.Ntid, true, userAndGroupIds1, null);
			Assert.AreEqual(0, proposals.Where(p => p.ProposalId == proposal.Id).ToCollection().Count);

			// Test user2 with ShowMyOwnProposals - Should not find proposal
			proposals = sut.GetProposalsByUser(null, null, null, null, user2.Ntid, false, null, null);
			Assert.AreEqual(0, proposals.Where(p => p.ProposalId == proposal.Id).ToCollection().Count);

			// Test user2 with showProposalsForMyOrganization - Should find proposal (user is Viewer for LOB 10)
			proposals = sut.GetProposalsByUser(null, null, null, null, user2.Ntid, true, userAndGroupIds2, null);
			Assert.IsTrue(proposals.Count >= 1);
			Assert.IsTrue(proposals.Any(p => p.ProposalId == proposal.Id));
		}

		/// <summary>
		/// Test the Proposal Class Filter option for Forecasted.
		/// </summary>
		[TestMethod]
		public void L_GetProposals_Forecasted()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true, isForecasted: true);
			UserDTO user = this.testData.GetUser(inCreateNew: true);
			this.testData.GetProposalPermission(true, new ProposalPermissionDto()
			{
				ProposalID = proposal.Id,
				Role = PtmRole.LOBEstLead,
				UserId = user.Id,
				Updateable = UpdateType.Upsert
			});

			// Ensure proposal comes back with 'Forecasted' filter.
			ICollection<HomeProposalViewDto> proposals = sut.GetProposalsByUser(null, null, null, null, user.Ntid, false, null, (int)ProposalClassFilterOption.Forecasted);
			Assert.AreEqual(proposals.Count, 1);
			Assert.IsTrue(proposals.Any(p => p.ProposalId == proposal.Id));

			// Ensure proposal comes back with 'All' filter.
			proposals = sut.GetProposalsByUser(null, null, null, null, user.Ntid, false, null, (int)ProposalClassFilterOption.All);
			Assert.AreEqual(proposals.Count, 1);
			Assert.IsTrue(proposals.Any(p => p.ProposalId == proposal.Id));

			// Ensure proposal comes back with null filter (which is treated as 'All').
			proposals = sut.GetProposalsByUser(null, null, null, null, user.Ntid, false, null, null);
			Assert.AreEqual(proposals.Count, 1);
			Assert.IsTrue(proposals.Any(p => p.ProposalId == proposal.Id));

			// Ensure proposal does NOT come back with 'NonForecasted' filter.
			proposals = sut.GetProposalsByUser(null, null, null, null, user.Ntid, false, null, (int)ProposalClassFilterOption.NonForecasted);
			Assert.AreEqual(proposals.Count, 0);
			Assert.IsFalse(proposals.Any(p => p.ProposalId == proposal.Id));
		}

		/// <summary>
		/// Test the Proposal Class Filter option for NonForecasted.
		/// </summary>
		[TestMethod]
		public void L_GetProposals_NonForecasted()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true, isForecasted: false);
			UserDTO user = this.testData.GetUser(inCreateNew: true);
			this.testData.GetProposalPermission(true, new ProposalPermissionDto()
			{
				ProposalID = proposal.Id,
				Role = PtmRole.LOBEstLead,
				UserId = user.Id,
				Updateable = UpdateType.Upsert
			});

			// Ensure proposal comes back with 'NonForecasted' filter.
			ICollection<HomeProposalViewDto> proposals = sut.GetProposalsByUser(null, null, null, null, user.Ntid, false, null, (int)ProposalClassFilterOption.NonForecasted);
			Assert.AreEqual(proposals.Count, 1);
			Assert.IsTrue(proposals.Any(p => p.ProposalId == proposal.Id));

			// Ensure proposal comes back with 'All' filter.
			proposals = sut.GetProposalsByUser(null, null, null, null, user.Ntid, false, null, (int)ProposalClassFilterOption.All);
			Assert.AreEqual(proposals.Count, 1);
			Assert.IsTrue(proposals.Any(p => p.ProposalId == proposal.Id));

			// Ensure proposal comes back with null filter (which is treated as 'All').
			proposals = sut.GetProposalsByUser(null, null, null, null, user.Ntid, false, null, null);
			Assert.AreEqual(proposals.Count, 1);
			Assert.IsTrue(proposals.Any(p => p.ProposalId == proposal.Id));

			// Ensure proposal does NOT come back with 'Forecasted' filter.
			proposals = sut.GetProposalsByUser(null, null, null, null, user.Ntid, false, null, (int)ProposalClassFilterOption.Forecasted);
			Assert.AreEqual(proposals.Count, 0);
			Assert.IsFalse(proposals.Any(p => p.ProposalId == proposal.Id));
		}

		/// <summary>
		/// Tests for Date Assigned behavior
		/// </summary>
		[TestMethod]
		public void L_DateAssignedTest()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);

			// get original date assigned
			ProposalDto toTest = sut.GetById(proposal.Id);
			DateTime originalTimestamp = toTest.DateAssigned.Value;

			// save with flag=false
			toTest.UpdateDateAssigned = false;
			toTest.Updateable = UpdateType.Upsert;
			using (TransactionScope scope = new TransactionScope())
			{
				sut.Save(toTest);
				scope.Complete();
			}

			// get proposal, verify Date Assigned hasn't changed
			toTest = sut.GetById(proposal.Id);
			Assert.AreEqual(originalTimestamp, toTest.DateAssigned);

			// save with flag=true
			toTest.UpdateDateAssigned = true;
			toTest.Updateable = UpdateType.Upsert;
			using (TransactionScope scope = new TransactionScope())
			{
				sut.Save(toTest);
				scope.Complete();
			}

			// get proposal, verify Date Assigned has been updated
			toTest = sut.GetById(proposal.Id);
			Assert.IsTrue(toTest.DateAssigned > originalTimestamp);
		}

		/// <summary>
		/// Tests for GetProposalCompletedDate 
		/// </summary>
		[TestMethod]
		public void L_GetProposalCompletedDateTest()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);

			// proposal is InProgress, so completed date should be null
			DateTime? completedDate = sut.GetProposalCompletedDate(proposal.Id);
			Assert.IsFalse(completedDate.HasValue);

			// save as pricer and move to completed
			this.testData.SaveChecklistAsPricer(proposal.Id, isSubmit: true);
			this.testData.SetProposalStatus(proposal.Id, ProposalStatus.Completed);

			// completed date should be pricer's max date
			ProposalChecklistLoader checklistLoader = new ProposalChecklistLoader();
			DateTime pricerDate = checklistLoader.GetAllChecklistSaveInfo(proposal.Id).Where(x => x.ResponseType == ChecklistResponseType.Pricer).Select(x => x.SubmitDate).Max().Value;
			completedDate = sut.GetProposalCompletedDate(proposal.Id);
			Assert.IsTrue(completedDate.HasValue);
			Assert.AreEqual(pricerDate, completedDate);

			// save as peer
			this.testData.SaveChecklistAsPeer(proposal.Id, isSubmit: true);

			// completed date should be peer's date
			DateTime peerDate = checklistLoader.GetAllChecklistSaveInfo(proposal.Id).Where(x => x.ResponseType == ChecklistResponseType.Peer).Select(x => x.SubmitDate).First().Value;
			completedDate = sut.GetProposalCompletedDate(proposal.Id);
			Assert.IsTrue(peerDate > pricerDate);
			Assert.IsTrue(completedDate.HasValue);
			Assert.AreEqual(peerDate, completedDate);
		}

		/// <summary>
		/// Tests for GetProposalStatus 
		/// </summary>
		[TestMethod]
		public void L_GetProposalStatusTest()
		{
			ProposalLoader sut = this.CreateSystem();

			// new proposal is In Progress
			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);
			ProposalStatus? result = sut.GetProposalStatus(proposal.Id);
			Assert.AreEqual(ProposalStatus.InProgress, result);

			// deleted
			this.testData.SetProposalStatus(proposal.Id, ProposalStatus.Deleted);
			result = sut.GetProposalStatus(proposal.Id);
			Assert.AreEqual(ProposalStatus.Deleted, result);
		}

		/// <summary>
		/// Tests for UpdateProposalStatus
		/// </summary>
		[TestMethod]
		public void L_UpdateProposalStatusTest()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);

			Assert.AreEqual(proposal.ProposalStatus, ProposalStatus.InProgress);

			// change the proposal status to completed
			int? proposalId = sut.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, ProposalStatus.Completed);

			Assert.IsTrue(proposalId.HasValue);
			proposal = sut.GetById(proposalId.Value);
			Assert.AreEqual(proposal.ProposalStatus, ProposalStatus.Completed);

			// change the proposal status to archived
			proposalId = sut.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, ProposalStatus.Archived);

			Assert.IsTrue(proposalId.HasValue);
			proposal = sut.GetById(proposalId.Value);
			Assert.AreEqual(proposal.ProposalStatus, ProposalStatus.Archived);

			// change the proposal status to deleted
			proposalId = sut.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, ProposalStatus.Deleted);

			Assert.IsTrue(proposalId.HasValue);
			proposal = sut.GetById(proposalId.Value);
			Assert.AreEqual(proposal.ProposalStatus, ProposalStatus.Deleted);
		}

		/// <summary>
		/// Tests for IsProposalTitleUnique
		/// </summary>
		[TestMethod]
		public void L_IsProposalTitleUnique()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);

			// Find a Proposal Title that isn't yet used
			string randomProposalTitleExpectedToBeUnique = "Mock" + Guid.NewGuid().ToString();

			Assert.IsTrue(sut.IsProposalTitleUnique(proposal.Id, randomProposalTitleExpectedToBeUnique));
			Assert.IsFalse(sut.IsProposalTitleUnique(proposal.Id + 1, proposal.ProposalTitle));
		}

		/// <summary>
		/// Test for retrieval of proposals by workflow status.
		/// </summary>
		[TestMethod]
		public void L_GetProposalsByWorkflowStatus()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);
			proposal.WorkflowStatus = WorkflowStatus.InitialLOBLeadEmail;
			proposal.WorkflowStatusLastUpdated = DateTime.Now;
			proposal.Updateable = UpdateType.Upsert;
			using (TransactionScope scope = new TransactionScope())
			{
				sut.Save(proposal);
				scope.Complete();
			}

			ICollection<ProposalDto> proposals = sut.GetProposalsByWorkflowStatus(WorkflowStatus.InitialLOBLeadEmail);
			Assert.IsTrue(proposals.Any(p => p.Id == proposal.Id));

			proposal = sut.GetById(proposal.Id);
			proposal.WorkflowStatus = WorkflowStatus.ProposalLocked;
			proposal.Updateable = UpdateType.Upsert;
			using (TransactionScope scope = new TransactionScope())
			{
				sut.Save(proposal);
				scope.Complete();
			}

			proposals = sut.GetProposalsByWorkflowStatus(WorkflowStatus.ProposalLocked);

			Assert.IsTrue(proposals.Any(p => p.Id == proposal.Id));
		}

		/// <summary>
		/// Test GetProposalsByProposalStatus
		/// </summary>
		[TestMethod]
		public void L_GetProposalsByProposalStatus()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);

			// the newly created test proposal should have a status of In Progress
			ICollection<ProposalDto> result = sut.GetProposalsByProposalStatus(ProposalStatus.InProgress);

			Assert.IsNotNull(result.FirstOrDefault(x => x.Id == proposal.Id));

			// Change the status and test getting proposals by that status
			sut.UpdateProposalStatus(proposal.Id, proposal.UpdateDate, ProposalStatus.PendingCertification);

			result = sut.GetProposalsByProposalStatus(ProposalStatus.PendingCertification);

			Assert.IsNotNull(result.FirstOrDefault(x => x.Id == proposal.Id));
		}

		/// <summary>
		/// Test GetAllCompletedProposalsAfterSubmitDate
		/// </summary>
		[TestMethod]
		public void L_GetAllCompletedProposalsAfterSubmitDate()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);
			this.testData.SaveChecklistAsPricer(proposal.Id, null, true);
			this.testData.SetCustomerSubmittalDate(proposal.Id, DateTime.Now.AddDays(1));
			this.testData.SetProposalStatus(proposal.Id, ProposalStatus.PendingCertification);

			ICollection<ProposalDto> result = sut.GetAllCompletedProposalsAfterSubmitDate(DateTime.Now);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Id == proposal.Id));
		}

		/// <summary>
		/// Test for retrieval of proposals by workflow status and cutoff date.
		/// </summary>
		[TestMethod]
		public void L_GetProposalsByWorkflowStatusAndCutoffDate()
		{
			ProposalLoader sut = this.CreateSystem();

			DateTime cutoffDate = DateTime.Now.AddHours(-1);

			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);
			proposal.WorkflowStatus = WorkflowStatus.ProposalLocked;
			proposal.WorkflowStatusLastUpdated = cutoffDate.AddHours(-2);
			proposal.Updateable = UpdateType.Upsert;
			using (TransactionScope scope = new TransactionScope())
			{
				sut.Save(proposal);
				scope.Complete();
			}

			ICollection<ProposalDto> proposals = sut.GetProposalsByWorkflowStatusAndCutoffDate(WorkflowStatus.ProposalLocked, cutoffDate);

			Assert.IsTrue(proposals.Any(p => p.Id == proposal.Id));

			proposal = sut.GetById(proposal.Id);
			proposal.WorkflowStatusLastUpdated = cutoffDate.AddHours(1);
			proposal.Updateable = UpdateType.Upsert;
			using (TransactionScope scope = new TransactionScope())
			{
				sut.Save(proposal);
				scope.Complete();
			}

			proposals = sut.GetProposalsByWorkflowStatusAndCutoffDate(WorkflowStatus.ProposalLocked, cutoffDate);

			Assert.IsFalse(proposals.Any(p => p.Id == proposal.Id));
		}

		/// <summary>
		/// Tests the ResetWorkflow method.
		/// </summary>
		[TestMethod]
		public void L_ResetWorkflowTest()
		{
			ProposalLoader sut = this.CreateSystem();
			ProposalDto proposal = this.testData.GetProposal(inCreateNew: true);
			proposal = sut.GetById(proposal.Id);
			proposal.WorkflowStatus = WorkflowStatus.ProposalLocked;
			proposal.WorkflowStatusLastUpdated = DateTime.Now;
			proposal.LeadEstimatorSignatureComment = "lead comment";
			proposal.LeadEstimatorSignedDate = DateTime.Now;
			proposal.ApprovalEmailText = "email text";
			proposal.CoverSheetApproverSignatureComment = "cv comment";
			proposal.CoverSheetApproverSignedDate = DateTime.Now;
			proposal.PricingVerifierSignatureComment = "pv comment";
			proposal.PricingVerifierSignedDate = DateTime.Now;
			proposal.IndependentReviewerSignatureComment = "indp comment";
			proposal.IndependentReviewerSignedDate = DateTime.Now;
			proposal.LOBEstimatingLeadSignatureComment = "lob comment";
			proposal.LOBEstimatingLeadSignedDate = DateTime.Now;

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
			{
				proposal.Updateable = UpdateType.Upsert;
				sut.Save(proposal);

				scope.Complete();
			}

			ProposalDto actual = sut.GetById(proposal.Id);
			Assert.AreEqual(proposal.WorkflowStatus, actual.WorkflowStatus);
			Assert.AreEqual(proposal.WorkflowStatusLastUpdated, actual.WorkflowStatusLastUpdated);
			Assert.AreEqual(proposal.LeadEstimatorSignatureComment, actual.LeadEstimatorSignatureComment);
			Assert.AreEqual(proposal.LeadEstimatorSignedDate, actual.LeadEstimatorSignedDate);
			Assert.AreEqual(proposal.ApprovalEmailText, actual.ApprovalEmailText);
			Assert.AreEqual(proposal.CoverSheetApproverSignatureComment, actual.CoverSheetApproverSignatureComment);
			Assert.AreEqual(proposal.CoverSheetApproverSignedDate, actual.CoverSheetApproverSignedDate);
			Assert.AreEqual(proposal.PricingVerifierSignatureComment, actual.PricingVerifierSignatureComment);
			Assert.AreEqual(proposal.PricingVerifierSignedDate, actual.PricingVerifierSignedDate);
			Assert.AreEqual(proposal.IndependentReviewerSignatureComment, actual.IndependentReviewerSignatureComment);
			Assert.AreEqual(proposal.IndependentReviewerSignedDate, actual.IndependentReviewerSignedDate);
			Assert.AreEqual(proposal.LOBEstimatingLeadSignatureComment, actual.LOBEstimatingLeadSignatureComment);
			Assert.AreEqual(proposal.LOBEstimatingLeadSignedDate, actual.LOBEstimatingLeadSignedDate);

			Mock<IChecklistMediator> checklistMediator;
			ApprovalsControllerLogic approvalsControllerLogic = this.CreateControllerLogicSystem(sut, out checklistMediator);
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
			{
				approvalsControllerLogic.ResetWorkflow(proposal.Id);
				scope.Complete();
			}

			actual = sut.GetById(proposal.Id);
			checklistMediator.Verify(x => x.UnlockChecklist(proposal.Id, It.IsAny<DateTime>(), UnlockChecklistOption.UnlockPricer), Times.Once());

			Assert.AreEqual(WorkflowStatus.NotStarted, actual.WorkflowStatus);
			Assert.IsNull(actual.WorkflowStatusLastUpdated);
			Assert.IsNotNull(actual.LeadEstimatorSignatureComment);
			Assert.IsNull(actual.LeadEstimatorSignedDate);
			Assert.IsNotNull(actual.ApprovalEmailText);
			Assert.IsNotNull(actual.CoverSheetApproverSignatureComment);
			Assert.IsNull(actual.CoverSheetApproverSignedDate);
			Assert.IsNotNull(actual.PricingVerifierSignatureComment);
			Assert.IsNull(actual.PricingVerifierSignedDate);
			Assert.IsNotNull(actual.IndependentReviewerSignatureComment);
			Assert.IsNull(actual.IndependentReviewerSignedDate);
			Assert.IsNotNull(actual.LOBEstimatingLeadSignatureComment);
			Assert.IsNull(actual.LOBEstimatingLeadSignedDate);
		}

		/// <summary>
		/// Creates ApprovalsControllerLogic for testing
		/// </summary>
		/// <param name="proposalLoader">The proposal loader.</param>
		/// <param name="checklistMediator">The mock object created for the checklist mediator.</param>
		/// <returns>ApprovalsControllerLogic</returns>
		private ApprovalsControllerLogic CreateControllerLogicSystem(ProposalLoader proposalLoader, out Mock<IChecklistMediator> checklistMediator)
		{
			Mock<ISecurityAccess> securityAccess = new Mock<ISecurityAccess>();
			Mock<IUserMapper> userMapper = new Mock<IUserMapper>();
			Mock<IUserLoader> userLoader = new Mock<IUserLoader>();
			Mock<IPtmEmailer> emailer = new Mock<IPtmEmailer>();
			Mock<ApprovalsLoader> approvalsLoader = new Mock<ApprovalsLoader>();
			Mock<IProposalChecklistLoader> proposalChecklistLoader = new Mock<IProposalChecklistLoader>();
			checklistMediator = new Mock<IChecklistMediator>();
			Mock<ApprovalEmailer> approvalEmailer = new Mock<ApprovalEmailer>();

			Mock<IRetriever> retriever = new Mock<IRetriever>();
			IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(retriever.Object);

			return new ApprovalsControllerLogic(securityAccess.Object, proposalLoader, userMapper.Object, userLoader.Object, emailer.Object, new FullObjectFactory(), approvalsLoader.Object, new ProposalMediator(proposalLoader), proposalChecklistLoader.Object, checklistMediator.Object, approvalEmailer.Object, null, null);
		}

		#region Exception Test

		/// <summary>
		/// Save proposal with exception (null argument)
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void L_SaveProposalException1()
		{
			ProposalLoader sut = this.CreateSystem();

			using (TransactionScope scope = new TransactionScope())
			{
				ProposalDto dto = null;
				sut.Save(dto);
				scope.Complete();
			}
		}

		/// <summary>
		/// Save proposal with exception (invalid argument)
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void L_SaveProposalException2()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto toSave = new ProposalDto();

			using (TransactionScope scope = new TransactionScope())
			{
				sut.Save(toSave);
				scope.Complete();
			}
		}

		/// <summary>
		/// Check if Proposal Title is Unique with null input (invalid argument)
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void L_IsProposalTitleUnique2()
		{
			ProposalLoader sut = this.CreateSystem();

			sut.IsProposalTitleUnique(1, null);
		}

		#endregion Exception Test

		/// <summary>
		/// Checks the retrieval of proposals with certification timeline past due.
		/// </summary>
		[TestMethod]
		public void GetProposalsCertificationTimelinePastDue()
		{
			ProposalLoader sut = this.CreateSystem();
			ICollection<ProposalDto> proposals = sut.GetProposalsCertificationTimelinePastDue();

			Assert.IsNotNull(proposals);
		}

		/// <summary>
		/// Determine whether to send ModExecutionDate email reminder
		/// Case: It has not been 15 days since setting CertificationDate, nothing to send
		/// </summary>
		[TestMethod]
		public void MissingModExecutionDateNoInitialNotificationsTest()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto previouslySubmittedRom = testData.GetProposal();
			ProposalDto proposal = testData.GetProposal();
			ContractsDto contractsDto = new ContractsDto();

			using (TransactionScope scope = new TransactionScope())
			{
				proposal.CertificationDate = DateTime.Now;
				proposal.Updateable = UpdateType.Upsert;

				sut.Save(proposal);

				ContractsLoader contractsLoader = new ContractsLoader();
				contractsDto.ProposalId = proposal.Id;
				contractsDto.PreviouslySubmittedROM = previouslySubmittedRom.Id; // FK, must exist
				contractsDto.ContractsCorrespondenceLogNumber = "ABC123ABC";
				contractsDto.Updateable = UpdateType.Upsert;

				contractsLoader.Save(contractsDto);

				scope.Complete();
			}

			ICollection<ProposalDto> proposals = sut.GetModExecutedDateMissingNotifications();

			Assert.IsNull(proposals.FirstOrDefault(x => x.Id == proposal.Id));
		}

		/// <summary>
		/// Determine whether to send ModExecutionDate email reminder
		/// Case: It has been 15 days since setting CertificationDate
		/// and we have not sent the first reminder, Send.
		/// </summary>
		[TestMethod]
		public void MissingModExecutionDateSendInitialNotificationsTest()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto previouslySubmittedRom = testData.GetProposal();
			ProposalDto proposal = testData.GetProposal(true);
			ContractsDto contractsDto = new ContractsDto();

			using (TransactionScope scope = new TransactionScope())
			{
				proposal.CertificationDate = DateTime.Now.AddDays(-30);
				proposal.Updateable = UpdateType.Upsert;

				sut.Save(proposal);

				ContractsLoader contractsLoader = new ContractsLoader();
				contractsDto.ProposalId = proposal.Id;
				contractsDto.PreviouslySubmittedROM = previouslySubmittedRom.Id;
				contractsDto.ContractsCorrespondenceLogNumber = "ABC123ABC";
				contractsDto.Updateable = UpdateType.Upsert;

				contractsLoader.Save(contractsDto);

				scope.Complete();
			}

			ICollection<ProposalDto> proposals = sut.GetModExecutedDateMissingNotifications();

			Assert.IsNotNull(proposals.FirstOrDefault(x => x.Id == proposal.Id));
		}

		/// <summary>
		/// Determine whether to send ModExecutionDate email reminder
		/// Case: It has been 15 days since setting CertificationDate,
		/// and the last emailed date is greater than 7 days.  Send.
		/// </summary>
		[TestMethod]
		public void MissingModExecutionDateReSendNotificationsTest()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto previouslySubmittedRom = testData.GetProposal();
			ProposalDto proposal = testData.GetProposal(true);
			ContractsDto contractsDto = new ContractsDto();

			using (TransactionScope scope = new TransactionScope())
			{
				proposal.CertificationDate = DateTime.Now.AddDays(-30);
				proposal.ModExecutedLastEmailed = DateTime.Now.AddDays(-8);
				proposal.Updateable = UpdateType.Upsert;

				sut.Save(proposal);

				ContractsLoader contractsLoader = new ContractsLoader();
				contractsDto.ProposalId = proposal.Id;
				contractsDto.PreviouslySubmittedROM = previouslySubmittedRom.Id;
				contractsDto.ContractsCorrespondenceLogNumber = "ABC123ABC";
				contractsDto.Updateable = UpdateType.Upsert;

				contractsLoader.Save(contractsDto);

				scope.Complete();
			}

			ICollection<ProposalDto> proposals = sut.GetModExecutedDateMissingNotifications();

			Assert.IsNotNull(proposals.FirstOrDefault(x => x.Id == proposal.Id));
		}

		/// <summary>
		/// Determine whether to send ModExecutionDate email reminder
		/// Case: It has been 15 days since setting CertificationDate,
		/// and the last emailed date is less than 7 days.  No send.
		/// </summary>
		[TestMethod]
		public void MissingModExecutionDateNoReSendNotificationTest()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto previouslySubmittedRom = testData.GetProposal();
			ProposalDto proposal = testData.GetProposal(true);
			ContractsDto contractsDto = new ContractsDto();

			using (TransactionScope scope = new TransactionScope())
			{
				proposal.CertificationDate = DateTime.Now.AddDays(-30);
				proposal.ModExecutedLastEmailed = DateTime.Now.AddDays(-6);
				proposal.Updateable = UpdateType.Upsert;

				sut.Save(proposal);

				ContractsLoader contractsLoader = new ContractsLoader();
				contractsDto.ProposalId = proposal.Id;
				contractsDto.PreviouslySubmittedROM = previouslySubmittedRom.Id;
				contractsDto.ContractsCorrespondenceLogNumber = "ABC123ABC";
				contractsDto.Updateable = UpdateType.Upsert;

				contractsLoader.Save(contractsDto);

				scope.Complete();
			}

			ICollection<ProposalDto> proposals = sut.GetModExecutedDateMissingNotifications();

			Assert.IsNull(proposals.FirstOrDefault(x => x.Id == proposal.Id));
		}

		/// <summary>
		/// Determine whether to send ModExecutionDate email reminder
		/// Case: The ModExecutionDate has been set.  No send.
		/// </summary>
		[TestMethod]
		public void ModExecutionDateSendNotNeededTest()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto previouslySubmittedRom = testData.GetProposal();
			ProposalDto proposal = testData.GetProposal(true);
			ContractsDto contractsDto = new ContractsDto();

			using (TransactionScope scope = new TransactionScope())
			{
				proposal.CertificationDate = DateTime.Now.AddDays(-30);
				proposal.ModExecutedLastEmailed = DateTime.Now.AddDays(-8);
				proposal.Updateable = UpdateType.Upsert;

				sut.Save(proposal);

				ContractsLoader contractsLoader = new ContractsLoader();
				contractsDto.ProposalId = proposal.Id;
				contractsDto.PreviouslySubmittedROM = previouslySubmittedRom.Id;
				contractsDto.ModCompletedDate = DateTime.Now;
				contractsDto.ContractsCorrespondenceLogNumber = "ABC123ABC";
				contractsDto.Updateable = UpdateType.Upsert;

				contractsLoader.Save(contractsDto);

				scope.Complete();
			}

			ICollection<ProposalDto> proposals = sut.GetModExecutedDateMissingNotifications();

			Assert.IsNull(proposals.FirstOrDefault(x => x.Id == proposal.Id));
		}

		/// <summary>
		/// Determine whether to send ModExecutionDate email reminder
		/// Case: The ModExecutionDate has not been set, but proposal is lost.  No send.
		/// </summary>
		[TestMethod]
		public void ModExecutionDate_LostTest()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto previouslySubmittedRom = testData.GetProposal();
			ProposalDto proposal = testData.GetProposal(true);
			ContractsDto contractsDto = new ContractsDto();

			using (TransactionScope scope = new TransactionScope())
			{
				proposal.CertificationDate = DateTime.Now.AddDays(-30);
				proposal.Updateable = UpdateType.Upsert;

				sut.Save(proposal);

				ContractsLoader contractsLoader = new ContractsLoader();
				contractsDto.ProposalId = proposal.Id;
				contractsDto.PreviouslySubmittedROM = previouslySubmittedRom.Id;
				contractsDto.ContractsCorrespondenceLogNumber = "ABC123ABC";
				contractsDto.LmWon = false;
				contractsDto.Updateable = UpdateType.Upsert;

				contractsLoader.Save(contractsDto);

				scope.Complete();
			}

			ICollection<ProposalDto> proposals = sut.GetModExecutedDateMissingNotifications();

			Assert.IsNull(proposals.FirstOrDefault(x => x.Id == proposal.Id));
		}

		/// <summary>
		/// Determine whether to send ModExecutionDate email reminder
		/// Case: The ModExecutionDate has not been set, but proposal was completed before the feature was deployed.  No send.
		/// </summary>
		[TestMethod]
		public void ModExecutionDate_OldTest()
		{
			ProposalLoader sut = this.CreateSystem();

			ProposalDto previouslySubmittedRom = testData.GetProposal();
			ProposalDto proposal = testData.GetProposal(true);
			ContractsDto contractsDto = new ContractsDto();

			using (TransactionScope scope = new TransactionScope())
			{
				proposal.CertificationDate = new DateTime(2021, 1, 1); // this works because PtmContractsStartDate is set to 5/16/21
				proposal.Updateable = UpdateType.Upsert;

				sut.Save(proposal);

				ContractsLoader contractsLoader = new ContractsLoader();
				contractsDto.ProposalId = proposal.Id;
				contractsDto.PreviouslySubmittedROM = previouslySubmittedRom.Id;
				contractsDto.ContractsCorrespondenceLogNumber = "ABC123ABC";
				contractsDto.Updateable = UpdateType.Upsert;

				contractsLoader.Save(contractsDto);

				scope.Complete();
			}

			ICollection<ProposalDto> proposals = sut.GetModExecutedDateMissingNotifications();

			Assert.IsNull(proposals.FirstOrDefault(x => x.Id == proposal.Id));
		}

		/// <summary>
		/// Tests GetWorkflowCompletedLineText for In Progress proposals
		/// </summary>
		[TestMethod]
		public void TestGetWorkflowCompletedLineText_InProgress()
		{
			DateTime anticipatedDeliveryDate = DateTime.Now;
			string result = ProposalLoader.GetWorkflowCompletedLineText(ProposalStatus.InProgress, anticipatedDeliveryDate, null, null);

			Assert.AreEqual("Due: " + anticipatedDeliveryDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR), result);
		}

		/// <summary>
		/// Tests GetWorkflowCompletedLineText for In Progress proposals with a revised delivery date
		/// </summary>
		[TestMethod]
		public void TestGetWorkflowCompletedLineText_InProgress_Revised()
		{
			DateTime anticipatedDeliveryDate = DateTime.Now;
			DateTime revisedDeliveryDate = DateTime.Now.AddDays(1);
			string result = ProposalLoader.GetWorkflowCompletedLineText(ProposalStatus.InProgress, anticipatedDeliveryDate, null, revisedDeliveryDate);

			Assert.AreEqual("Due: " + revisedDeliveryDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR), result);
		}

		/// <summary>
		/// Tests GetWorkflowCompletedLineText for Completed/Submitted/Revised proposals 
		/// </summary>
		[TestMethod]
		public void TestGetWorkflowCompletedLineText_CompletedSubmittedRevised()
		{
			DateTime anticipatedDeliveryDate = DateTime.Now;
			DateTime maxCompleteDate = DateTime.Now.AddDays(2);
			string expectedResultPrefix = "Approval Workflow Completed: ";

			// Test Completed
			string result = ProposalLoader.GetWorkflowCompletedLineText(ProposalStatus.Completed, anticipatedDeliveryDate, maxCompleteDate, null);
			Assert.AreEqual(expectedResultPrefix + maxCompleteDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR), result);

			// Test Submitted
			result = ProposalLoader.GetWorkflowCompletedLineText(ProposalStatus.PendingCertification, anticipatedDeliveryDate, maxCompleteDate, null);
			Assert.AreEqual(expectedResultPrefix + maxCompleteDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR), result);

			// Test Revised
			result = ProposalLoader.GetWorkflowCompletedLineText(ProposalStatus.Revised, anticipatedDeliveryDate, maxCompleteDate, null);
			Assert.AreEqual(expectedResultPrefix + maxCompleteDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR), result);
		}

		/// <summary>
		/// Tests GetWorkflowCompletedLineText for Completed/Submitted/Revised proposals with no max complete date
		/// </summary>
		[TestMethod]
		public void TestGetWorkflowCompletedLineText_CompletedSubmittedRevised_NA()
		{
			DateTime anticipatedDeliveryDate = DateTime.Now;
			string expectedResultPrefix = "Approval Workflow Completed: ";

			string result = ProposalLoader.GetWorkflowCompletedLineText(ProposalStatus.Revised, anticipatedDeliveryDate, null, null);
			Assert.AreEqual(expectedResultPrefix + "N/A", result);
		}

		/// <summary>
		/// Tests GetWorkflowCompletedLineText for Archived/Deleted/No Bid proposals 
		/// </summary>
		[TestMethod]
		public void TestGetWorkflowCompletedLineText_ArchivedDeletedNoBid()
		{
			DateTime anticipatedDeliveryDate = DateTime.Now;

			// Test Archived
			string result = ProposalLoader.GetWorkflowCompletedLineText(ProposalStatus.Archived, anticipatedDeliveryDate, null, null);
			Assert.AreEqual(ProposalStatus.Archived.GetDescription(), result);

			// Test Deleted
			result = ProposalLoader.GetWorkflowCompletedLineText(ProposalStatus.Deleted, anticipatedDeliveryDate, null, null);
			Assert.AreEqual(ProposalStatus.Deleted.GetDescription(), result);

			// Test No Bid
			result = ProposalLoader.GetWorkflowCompletedLineText(ProposalStatus.NoBid, anticipatedDeliveryDate, null, null);
			Assert.AreEqual(ProposalStatus.NoBid.GetDescription(), result);
		}

		#region GetEppProposalData

		/// <summary>
		/// Test non-admin, verify that retrieved proposals are pulled for the right role
		/// </summary>
		[TestMethod]
		public void GetEppProposalData_Test1()
		{
			bool isAdmin = false;
			string ntid = "paliderd";

			ProposalLoader sut = this.CreateSystem();
			UserLoader userLoader = new UserLoader();
			ProposalPermissionLoader permissionsLoader = new ProposalPermissionLoader();

			int userId = userLoader.GetByNtid(ntid).Id;
			ICollection<EppProposalData> result = sut.GetEppProposalData(ntid, isAdmin, null);

			foreach (int id in result.Select(x => x.ProposalId).ToList())
			{
				Assert.IsTrue(permissionsLoader.GetByIds(permissionsLoader.GetIdsByProposalId(id)).Any(x => x.UserId == userId && (x.Role == PtmRole.ContractsPOC || x.Role == PtmRole.BackupContractsPOC)));
			}
		}

		/// <summary>
		/// Test to verify that retrieved proposals are not forecasted
		/// </summary>
		[TestMethod]
		public void GetEppProposalData_Test2()
		{
			bool isAdmin = true;
			string ntid = "paliderd";

			ProposalLoader sut = this.CreateSystem();

			ICollection<EppProposalData> result = sut.GetEppProposalData(ntid, isAdmin, string.Empty);

			ICollection<ProposalDto> proposals = sut.GetByIds(result.Select(x => x.ProposalId).ToList());

			foreach (ProposalDto prop in proposals)
			{
				Assert.IsFalse(prop.IsForecastProposal);
			}
		}

		/// <summary>
		/// Test to verify that we retrieve max of 50 records
		/// </summary>
		[TestMethod]
		public void GetEppProposalData_Test4()
		{
			bool isAdmin = true;
			string ntid = "paliderd";

			ProposalLoader sut = this.CreateSystem();

			ICollection<EppProposalData> result = sut.GetEppProposalData(ntid, isAdmin, string.Empty);

			Assert.IsTrue(result.Count >= 50);
		}

		/// <summary>
		/// Test to verify search by LOB Name, full string
		/// </summary>
		[TestMethod]
		public void GetEppProposalData_Test5()
		{
			bool isAdmin = false;
			string ntid = "paliderd";
			string searchString = "Comm Space".ToLower();

			ProposalLoader sut = this.CreateSystem();

			EppProposalData result = sut.GetEppProposalData(ntid, isAdmin, searchString).First();

			Assert.IsTrue(result.LobDescription.ToLower().Contains(searchString));
		}

		/// <summary>
		/// Test to verify search by LOB Name, partial string
		/// </summary>
		[TestMethod]
		public void GetEppProposalData_Test6()
		{
			bool isAdmin = false;
			string ntid = "paliderd";
			string searchString = "Space".ToLower();

			ProposalLoader sut = this.CreateSystem();

			EppProposalData result = sut.GetEppProposalData(ntid, isAdmin, searchString).First();

			Assert.IsTrue(result.LobDescription.ToLower().Contains(searchString));
		}

		/// <summary>
		/// Test to verify search by title, full string
		/// </summary>
		[TestMethod]
		public void GetEppProposalData_Test7()
		{
			bool isAdmin = false;
			string ntid = "paliderd";
			string searchString = "Patton Regular Proposal 2".ToLower();

			ProposalLoader sut = this.CreateSystem();

			EppProposalData result = sut.GetEppProposalData(ntid, isAdmin, searchString).First();

			Assert.IsTrue(result.ProposalTitle.ToLower().Contains(searchString));
		}

		/// <summary>
		/// Test to verify search by title, partial string
		/// </summary>
		[TestMethod]
		public void GetEppProposalData_Test8()
		{
			bool isAdmin = false;
			string ntid = "paliderd";
			string searchString = "Regular Proposal".ToLower();

			ProposalLoader sut = this.CreateSystem();

			EppProposalData result = sut.GetEppProposalData(ntid, isAdmin, searchString).First();

			Assert.IsTrue(result.ProposalTitle.ToLower().Contains(searchString));
		}

		/// <summary>
		/// Test to verify search by tracking number, full string
		/// </summary>
		[TestMethod]
		public void GetEppProposalData_Test9()
		{
			bool isAdmin = false;
			string ntid = "paliderd";
			string searchString = "18-00008".ToLower();

			ProposalLoader sut = this.CreateSystem();

			EppProposalData result = sut.GetEppProposalData(ntid, isAdmin, searchString).First();

			Assert.IsTrue(result.PTMTrackingNumber.ToLower().Contains(searchString));
		}

		/// <summary>
		/// Test to verify search by tracking number, partial string
		/// </summary>
		[TestMethod]
		public void GetEppProposalData_Test10()
		{
			bool isAdmin = false;
			string ntid = "paliderd";
			string searchString = "00008".ToLower();

			ProposalLoader sut = this.CreateSystem();

			EppProposalData result = sut.GetEppProposalData(ntid, isAdmin, searchString).First();

			Assert.IsTrue(result.PTMTrackingNumber.ToLower().Contains(searchString));
		}

		/// <summary>
		/// Test to verify search by Contracts POC, full string
		/// </summary>
		[TestMethod]
		public void GetEppProposalData_Test11()
		{
			bool isAdmin = false;
			string ntid = "paliderd";
			string searchString = "Palider, Dusan".ToLower();

			ProposalLoader sut = this.CreateSystem();
			UserLoader userLoader = new UserLoader();
			ProposalPermissionLoader permissionsLoader = new ProposalPermissionLoader();
			int userId = userLoader.GetByNtid(ntid).Id;

			EppProposalData result = sut.GetEppProposalData(ntid, isAdmin, searchString).First();

			Assert.IsTrue(permissionsLoader.GetByIds(permissionsLoader.GetIdsByProposalId(result.ProposalId)).Any(x => x.UserId == userId && x.Role == PtmRole.ContractsPOC));
		}

		/// <summary>
		/// Test to verify search by Contracts POC, partial string
		/// </summary>
		[TestMethod]
		public void GetEppProposalData_Test12()
		{
			bool isAdmin = false;
			string ntid = "paliderd";
			string searchString = "Palider".ToLower();

			ProposalLoader sut = this.CreateSystem();
			UserLoader userLoader = new UserLoader();
			ProposalPermissionLoader permissionsLoader = new ProposalPermissionLoader();
			int userId = userLoader.GetByNtid(ntid).Id;

			EppProposalData result = sut.GetEppProposalData(ntid, isAdmin, searchString).First();

			Assert.IsTrue(permissionsLoader.GetByIds(permissionsLoader.GetIdsByProposalId(result.ProposalId)).Any(x => x.UserId == userId && x.Role == PtmRole.ContractsPOC));
		}

		/// <summary>
		/// Test to verify get by tracking number, correct search, correct result
		/// </summary>
		[TestMethod]
		public void GetEppProposalDataByProposalTrackingNumber_Test1()
		{
			bool isAdmin = true;
			string ntid = "paliderd";
			string trackingNumber = "21-00008";

			ProposalLoader sut = this.CreateSystem();

			EppProposalData result = sut.GetEppProposalDataByProposalTrackingNumber(ntid, isAdmin, trackingNumber);

			Assert.AreEqual(trackingNumber, result.PTMTrackingNumber);
		}

		/// <summary>
		/// Test to verify get by tracking number, incorrect search (invalid permissions), null result
		/// </summary>
		[TestMethod]
		public void GetEppProposalDataByProposalTrackingNumber_Test2()
		{
			bool isAdmin = false;
			string ntid = "paliderd";
			string trackingNumber = "21-00008";

			ProposalLoader sut = this.CreateSystem();

			EppProposalData result = sut.GetEppProposalDataByProposalTrackingNumber(ntid, isAdmin, trackingNumber);

			Assert.IsNull(result);
		}

		#endregion

		/// <summary>
		/// Tests non-admin search
		/// </summary>
		[TestMethod]
		public void GetCostVolumeProposalData_Test1()
		{
			bool isAdmin = false;
			string ntid = "paliderd";
			string searchString = "00016";

			ProposalLoader sut = this.CreateSystem();

			ICollection<(string PtmTrackingNumber, string ProposalTitle, int ProposalId)> result = sut.GetCostVolumeProposalData(ntid, isAdmin, searchString);

			Assert.IsTrue(result.Any(x => x.PtmTrackingNumber.Contains("17-00016")));
		}

		/// <summary>
		/// Tests admin search and fail due to not in progress
		/// </summary>
		[TestMethod]
		public void GetCostVolumeProposalData_Test2()
		{
			bool isAdmin = true;
			string ntid = "paliderd";
			string searchString = "20-00017-PR1";

			ProposalLoader sut = this.CreateSystem();

			ICollection<(string PtmTrackingNumber, string ProposalTitle, int ProposalId)> result = sut.GetCostVolumeProposalData(ntid, isAdmin, searchString);

			Assert.IsFalse(result.Any(x => x.PtmTrackingNumber.Contains("20-00017-PR1")));
		}

		/// <summary>
		/// Test GetAcvHeaderDataByProposalId for when CCoPD is true
		/// </summary>
		[TestMethod]
		public void GetAcvHeaderDataByProposalId_CcopdTrue()
		{
			Proposal testProposal;

			// get a proposal with CCoPD set to true to test with
			using (genTRACEntities dbModel = new genTRACEntities())
			{
				testProposal = dbModel.Proposals.Include("ProposalContractsDatas").Where(x => x.CCPDRequired == true && x.ProposalContractsDatas.FirstOrDefault().ContractsCorrespondLogNumber != null).OrderByDescending(x => x.ProposalID).FirstOrDefault();
			}

			ProposalLoader sut = this.CreateSystem();

			AcvHeaderDataDto result = sut.GetAcvHeaderDataByProposalId(testProposal.ProposalID);

			Assert.IsNotNull(result);
			Assert.AreEqual(testProposal.ProposalTrackingID, result.PtmTrackingNumber);
			Assert.AreEqual(testProposal.ProposalTitle, result.ProposalTitle);
			Assert.AreEqual(testProposal.RFPNumber, result.RfpNumber);
			Assert.IsNull(result.CostVolumeSubmittalDate);
			Assert.AreEqual(testProposal.ProposalContractsDatas.FirstOrDefault().ContractsCorrespondLogNumber, result.CCLogNumber);
		}

		/// <summary>
		/// Test GetAcvHeaderDataByProposalId for when CCoPD is true
		/// </summary>
		[TestMethod]
		public void GetAcvHeaderDataByProposalId_NullContracts()
		{
			Proposal testProposal;

			// get a proposal with CCoPD set to true to test with
			using (genTRACEntities dbModel = new genTRACEntities())
			{
				testProposal = dbModel.Proposals.Include("ProposalContractsDatas").Where(x => !x.ProposalContractsDatas.Any()).FirstOrDefault();
			}

			ProposalLoader sut = this.CreateSystem();

			AcvHeaderDataDto result = sut.GetAcvHeaderDataByProposalId(testProposal.ProposalID);

			Assert.AreEqual(string.Empty, result.CCLogNumber);
		}

		/// <summary>
		/// Test GetAcvHeaderDataByProposalId for when CCoPD is false and Revised Anticipated Delivery Date is available
		/// </summary>
		[TestMethod]
		public void GetAcvHeaderDataByProposalId_CcopdFalse_Revised()
		{
			Proposal testProposal;

			// get a proposal with CCoPD set to true to test with
			using (genTRACEntities dbModel = new genTRACEntities())
			{
				testProposal = dbModel.Proposals.Where(x => x.CCPDRequired == false && x.RevisedSubmittalDate.HasValue && x.RevisedSubmittalDate != x.AnticipatedDeliveryDate).OrderByDescending(x => x.ProposalID).FirstOrDefault();
			}

			ProposalLoader sut = this.CreateSystem();

			AcvHeaderDataDto result = sut.GetAcvHeaderDataByProposalId(testProposal.ProposalID);

			Assert.IsNotNull(result);
			Assert.AreEqual(testProposal.ProposalTrackingID, result.PtmTrackingNumber);
			Assert.AreEqual(testProposal.ProposalTitle, result.ProposalTitle);
			Assert.AreEqual(testProposal.RFPNumber, result.RfpNumber);
			Assert.AreEqual(testProposal.RevisedSubmittalDate, result.CostVolumeSubmittalDate);
		}

		/// <summary>
		/// Test GetAcvHeaderDataByProposalId for when CCoPD is false and Revised Anticipated Delivery Date is not available
		/// </summary>
		[TestMethod]
		public void GetAcvHeaderDataByProposalId_CcopdFalse_NotRevised()
		{
			Proposal testProposal;

			// get a proposal with CCoPD set to true to test with
			using (genTRACEntities dbModel = new genTRACEntities())
			{
				testProposal = dbModel.Proposals.Where(x => x.CCPDRequired == false && !x.RevisedSubmittalDate.HasValue).OrderByDescending(x => x.ProposalID).FirstOrDefault();
			}

			ProposalLoader sut = this.CreateSystem();

			AcvHeaderDataDto result = sut.GetAcvHeaderDataByProposalId(testProposal.ProposalID);

			Assert.IsNotNull(result);
			Assert.AreEqual(testProposal.ProposalTrackingID, result.PtmTrackingNumber);
			Assert.AreEqual(testProposal.ProposalTitle, result.ProposalTitle);
			Assert.AreEqual(testProposal.RFPNumber, result.RfpNumber);
			Assert.AreEqual(testProposal.AnticipatedDeliveryDate, result.CostVolumeSubmittalDate);
		}

		/// <summary>
		/// Test that GetProposalRolesForNlfByNtid returns results for each role
		/// </summary>
		[TestMethod]
		public void GetProposalRolesForNlfByNtid_NonAdmin()
		{
			ProposalLoader sut = CreateSystem();

			string leadEstimatorNtid;
			string backupEstimatorNtid;
			string costVolumeLeadNtid;
			string materialLeadNtid;
			string backupMaterialLeadNtid;
			string subcontractsLeadNtid;
			string backupSubcontractsLeadNtid;

			using (genTRACEntities dbModel = new genTRACEntities())
			{
				leadEstimatorNtid = GetNtidWithRole(PtmRole.Pricer, dbModel);
				backupEstimatorNtid = GetNtidWithRole(PtmRole.BackupPricer, dbModel);
				costVolumeLeadNtid = GetNtidWithRole(PtmRole.CostVolumeLead, dbModel);
				materialLeadNtid = GetNtidWithRole(PtmRole.SupplyChainPOCMatl, dbModel);
				backupMaterialLeadNtid = GetNtidWithRole(PtmRole.BackupMaterialLead, dbModel);
				subcontractsLeadNtid = GetNtidWithRole(PtmRole.SupplyChainPOCSubs, dbModel);
				backupSubcontractsLeadNtid = GetNtidWithRole(PtmRole.BackupSubcontractsLead, dbModel);
			}

			ICollection<ProposalRoleDto> result = sut.GetProposalRolesForNlfByNtid(leadEstimatorNtid);
			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Role == PtmRole.Pricer));

			result = sut.GetProposalRolesForNlfByNtid(backupEstimatorNtid);
			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Role == PtmRole.BackupPricer));

			result = sut.GetProposalRolesForNlfByNtid(costVolumeLeadNtid);
			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.Role == PtmRole.CostVolumeLead));

			if (!string.IsNullOrEmpty(materialLeadNtid))
			{
				result = sut.GetProposalRolesForNlfByNtid(materialLeadNtid);
				Assert.IsTrue(result.Any());
				Assert.IsTrue(result.Any(x => x.Role == PtmRole.SupplyChainPOCMatl));
			}

			if (!string.IsNullOrEmpty(backupMaterialLeadNtid))
			{
				result = sut.GetProposalRolesForNlfByNtid(backupMaterialLeadNtid);
				Assert.IsTrue(result.Any());
				Assert.IsTrue(result.Any(x => x.Role == PtmRole.BackupMaterialLead));
			}

			if (!string.IsNullOrEmpty(subcontractsLeadNtid))
			{
				result = sut.GetProposalRolesForNlfByNtid(subcontractsLeadNtid);
				Assert.IsTrue(result.Any());
				Assert.IsTrue(result.Any(x => x.Role == PtmRole.SupplyChainPOCSubs));
			}

			if (!string.IsNullOrEmpty(backupSubcontractsLeadNtid))
			{
				result = sut.GetProposalRolesForNlfByNtid(backupSubcontractsLeadNtid);
				Assert.IsTrue(result.Any());
				Assert.IsTrue(result.Any(x => x.Role == PtmRole.BackupSubcontractsLead));
			}
		}

		/// <summary>
		/// Get an NTID with the given role from a proposal
		/// </summary>
		/// <param name="ptmRole">Role to get NTID for</param>
		/// <param name="dbModel">genTrac Entities</param>
		/// <returns>NTID</returns>
		private string GetNtidWithRole(PtmRole ptmRole, genTRACEntities dbModel)
		{
			return dbModel.Proposals.Where(x => x.ProposalClassLU.ProposalClass != Constants.PROPOSAL_CLASS_FORECASTED
					&& x.ProposalUserRoles.Any(role => role.RoleID == (int)ptmRole)).FirstOrDefault()?
					.ProposalUserRoles.FirstOrDefault(y => y.RoleID == (int)ptmRole)?.genTRACUser.NTID.ToString();
		}
	}
}
