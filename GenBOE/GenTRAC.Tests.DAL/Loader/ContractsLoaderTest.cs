// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Loader
{
	using System;
	using GenTRAC.DataBridge.DTO;
	using IES.Common;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Tests for the Contracts loader
	/// </summary>
	[TestClass]
	public class ContractsLoaderTest
	{
		/// <summary>
		/// Handle to the test data
		/// </summary>
		private TestData testData = TestData.GetInstance();

		/// <summary>
		/// Test the GetByProposalIds method. 
		/// </summary>
		[TestMethod]
		public void GetByIdsTest()
		{
			this.TestContractsLoader(false);
		}

		/// <summary>
		/// Test the GetByProposalIds method. 
		/// </summary>
		[TestMethod]
		public void GetByProposalIdsTest()
		{
			this.TestContractsLoader(true);
		}

		/// <summary>
		/// Test the Contracts Loader
		/// </summary>
		/// <param name="useProposalID">If true use GetContractForProposal method, else use GetById method</param>
		private void TestContractsLoader(bool useProposalID)
		{
			// Create new ContractDTO 
			ContractsLoader sut = this.CreateSystem();

			ProposalDto previouslySubmittedProposal = this.testData.GetProposal(true);

			ProposalDto proposal = this.testData.GetProposal(true);

			ContractsDto testContractExpected = this.CreateContractsDto(proposal.Id, previouslySubmittedProposal.Id);

			int? contractId = sut.Save(testContractExpected);
			Assert.IsNotNull(contractId);

			ContractsDto testContractActual = useProposalID ? sut.GetContractForProposal(proposal.Id) : sut.GetById(contractId.Value);

			Assert.IsTrue(testContractActual.Id > 0);

			this.CompareContractData(testContractExpected, testContractActual);

			// Update Contract
			testContractActual.Updateable = UpdateType.Upsert;
			testContractActual.CustomerDueDate = DateTime.Now.AddDays(1);
			testContractActual.CustomerSubmittalDate = DateTime.Now.AddDays(1);
			testContractActual.ContractsCorrespondenceLogNumber = "TestLogNumber2";
			testContractActual.FinalNegotiatedValue = 201;
			testContractActual.NegotiationsSubmitted = DateTime.Now.AddDays(1);
			testContractActual.EppDelegationAuthority = (int)EppDelegationAuthority.MissionSegment;
			testContractActual.ScheduledActualProgramEppDate = DateTime.Now.AddDays(1);
			testContractActual.ScheduledActualLobEppDate = DateTime.Now.AddDays(1);
			testContractActual.ScheduledActualPreSpaceEppDate = DateTime.Now.AddDays(1);
			testContractActual.ScheduledActualSpaceEppDate = DateTime.Now.AddDays(1);
			testContractActual.ScheduledActualPreCorporateEppDate = DateTime.Now.AddDays(1);
			testContractActual.ScheduledActualCorporateEppDate = DateTime.Now.AddDays(1);
			testContractActual.EppRosDelegationNotes = "Test EppRosDelegationNotes2";
			testContractActual.LmWon = true;
			testContractActual.ModCompletedDate = DateTime.Now.AddDays(1);
			testContractActual.ScheduledActualBidEppDate =	DateTime.Now.AddDays(1);
			testContractActual.ScheduledActualMissionSegmentEppDate = DateTime.Now.AddDays(1);

			contractId = sut.Save(testContractActual);
			Assert.IsNotNull(contractId);
			Assert.AreEqual(contractId, testContractActual.Id);

			ContractsDto testContractActual2 = useProposalID ? sut.GetContractForProposal(proposal.Id) : sut.GetById(contractId.Value);

			this.CompareContractData(testContractActual, testContractActual2);
		}

		/// <summary>
		/// Creates a Contracts DTO based on a Propsal ID
		/// </summary>
		/// <param name="proposalId"> The proposal ID</param>
		/// <param name="previouslySubmittedProposalId"> The previously submitted proposal ID</param>
		/// <returns>A contracts dto object</returns>
		private ContractsDto CreateContractsDto(int proposalId, int previouslySubmittedProposalId)
		{
			return new ContractsDto
			{
				Updateable = UpdateType.Upsert,
				ProposalId = proposalId,
				PreviouslySubmittedROM = previouslySubmittedProposalId,
				CustomerDueDate = DateTime.Now,
				CustomerSubmittalDate = DateTime.Now,
				ContractsCorrespondenceLogNumber = "Test LogNumber",
				FinalNegotiatedValue = 222L,
				NegotiationsSubmitted = DateTime.Now,
				EppDelegationAuthority = (int)EppDelegationAuthority.Space,
				ScheduledActualProgramEppDate = DateTime.Now,
				ScheduledActualLobEppDate = DateTime.Now,
				ScheduledActualPreSpaceEppDate = DateTime.Now,
				ScheduledActualSpaceEppDate = DateTime.Now,
				ScheduledActualPreCorporateEppDate = DateTime.Now,
				ScheduledActualCorporateEppDate = DateTime.Now,
				EppRosDelegationNotes = "Test EppRosDelegationNotes",
				LmWon = false,
				ModCompletedDate = DateTime.Now,
				ScheduledActualBidEppDate = DateTime.Now,
				ScheduledActualMissionSegmentEppDate = DateTime.Now
			};
		}

		/// <summary>
		/// Asserts that the two Contracts dtos are equal.
		/// </summary>
		/// <param name="expected">The expected.</param>
		/// <param name="actual">The actual.</param>
		private void CompareContractData(ContractsDto expected, ContractsDto actual)
		{
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.ProposalId, actual.ProposalId);
			Assert.AreEqual(expected.PreviouslySubmittedROM, actual.PreviouslySubmittedROM);
			Assert.AreEqual(expected.CustomerDueDate.Value.Date, actual.CustomerDueDate.Value.Date);
			Assert.AreEqual(expected.CustomerSubmittalDate.Value.Date, actual.CustomerSubmittalDate.Value.Date);
			Assert.AreEqual(expected.ContractsCorrespondenceLogNumber, actual.ContractsCorrespondenceLogNumber);
			Assert.AreEqual(expected.FinalNegotiatedValue, actual.FinalNegotiatedValue);
			Assert.AreEqual(expected.NegotiationsSubmitted.Value.Date, actual.NegotiationsSubmitted.Value.Date);
			Assert.AreEqual(expected.EppDelegationAuthority, actual.EppDelegationAuthority);
			Assert.AreEqual(expected.ScheduledActualProgramEppDate.Value.Date, actual.ScheduledActualProgramEppDate.Value.Date);
			Assert.AreEqual(expected.ScheduledActualLobEppDate.Value.Date, actual.ScheduledActualLobEppDate.Value.Date);
			Assert.AreEqual(expected.ScheduledActualPreSpaceEppDate.Value.Date, actual.ScheduledActualPreSpaceEppDate.Value.Date);
			Assert.AreEqual(expected.ScheduledActualSpaceEppDate.Value.Date, actual.ScheduledActualSpaceEppDate.Value.Date);
			Assert.AreEqual(expected.ScheduledActualPreCorporateEppDate.Value.Date, actual.ScheduledActualPreCorporateEppDate.Value.Date);
			Assert.AreEqual(expected.ScheduledActualCorporateEppDate.Value.Date, actual.ScheduledActualCorporateEppDate.Value.Date);
			Assert.AreEqual(expected.EppRosDelegationNotes, actual.EppRosDelegationNotes);
			Assert.AreEqual(expected.LmWon, actual.LmWon);
			Assert.AreEqual(expected.ModCompletedDate.Value.Date, actual.ModCompletedDate.Value.Date);
			Assert.AreEqual(expected.ScheduledActualBidEppDate.Value.Date, actual.ScheduledActualBidEppDate.Value.Date);
			Assert.AreEqual(expected.ScheduledActualMissionSegmentEppDate.Value.Date, actual.ScheduledActualMissionSegmentEppDate.Value.Date);
		}

		/// <summary>
		/// Create the system
		/// </summary>
		/// <returns>A ContractsLoader loader</returns>
		private ContractsLoader CreateSystem()
		{
			return new ContractsLoader();
		}
	}
}
