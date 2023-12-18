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
	using System.Transactions;
	using GenTRAC.DataBridge.DTO;
	using GenTRAC.DataBridge.DTO.Contracts;
	using IES.Common;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Tests for the Cover Sheet Data Loader
	/// </summary>
	[TestClass]
    public class CoverSheetDataLoaderTest
    {
		/// <summary>
        /// Handle to the test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

		/// <summary>
		/// Get Cover Sheet Data by Id - Without Checklist
		/// </summary>
		[TestMethod]
        public void GetCoverSheetDataByIdTest_WithoutChecklist()
        {
            CoverSheetDataLoader sut = new CoverSheetDataLoader();

			CageCodesLoader cageCodesLoader = new CageCodesLoader();
			ContractsLoader contractsLoader = new ContractsLoader();
			ProposalPermissionLoader proposalPermissionLoader = new ProposalPermissionLoader();
            UserLoader userLoader = new UserLoader();

			CageCodeDTO cageCode = cageCodesLoader.GetAllCageCodesData().First();
            int userId = userLoader.GetByNtid("paliderd").Id;

			// Proposal Dto
			ProposalDto prop = testData.GetProposal(true);

			// Proposal Contract Data
			ContractsDto contractsData = new ContractsDto()
			{
				Id = -1,
				Updateable = UpdateType.Upsert,
				CageCode = cageCode.CageCode,
				CustomerSubmittalDate = DateTime.Now.Date,
				ProposalId = prop.Id,
                ContractsCorrespondenceLogNumber = "booo hoo"
			};

            List<ProposalPermissionDto> permissions = new List<ProposalPermissionDto>()
            { 
                new ProposalPermissionDto()
                {
                    ProposalID = prop.Id,
                    Updateable = UpdateType.Upsert,
                    Id = -1,
                    Role = PtmRole.ContractsPOC,
                    UserId = userId
                },
				new ProposalPermissionDto()
				{
					ProposalID = prop.Id,
					Updateable = UpdateType.Upsert,
					Id = -2,
					Role = PtmRole.CoverSheetApprover,
					UserId = userId
				}
			};

			using (TransactionScope scope = new TransactionScope())
            {
				contractsLoader.Save(contractsData);
                proposalPermissionLoader.Save(permissions);

				scope.Complete();
            }

			CoverSheetDataDto actualData = sut.GetCoverSheetDataById(prop.Id);

            CoverSheetDataDto expectedData = new CoverSheetDataDto()
            {
                CageCode = cageCode.CageCode,
                OfferorAddress = new List<string>() { cageCode.Address1, cageCode.Address2, cageCode.City, cageCode.State, cageCode.Zip },
                CoverSheetApproverNtid = "paliderd",
                ContractActionType = prop.ContractActionType,
                ContractsLead = "paliderd",
                CoverSheetApproverSignedDate = prop.CoverSheetApproverSignedDate,
                OtherContractActionType = prop.ContractActionTypeOtherText,
                ContractTypeGroup = (int)prop.ContractTypeGroup,
                CustomerSubmittalDate = contractsData.CustomerSubmittalDate,
                IsCCPDRequired = prop.IsCCPDRequired,

				// missing checklist data..
				CostThroughCom = null,
				ProfitFee = null,
				LMSpaceTotalPrice = null
			};


			Assert.IsNotNull(actualData);
            DtoAssertHelpers.AssertDtos(expectedData, actualData);
        }

		/// <summary>
		/// Get Cover Sheet Data by Id - Without roles
		/// </summary>
		[TestMethod]
		public void GetCoverSheetDataByIdTest_WithoutRoles()
		{
			CoverSheetDataLoader sut = new CoverSheetDataLoader();

			CageCodesLoader cageCodesLoader = new CageCodesLoader();
			ContractsLoader contractsLoader = new ContractsLoader();

			CageCodeDTO cageCode = cageCodesLoader.GetAllCageCodesData().First();

			// Proposal Dto
			ProposalDto prop = testData.GetProposal(true);

			// Proposal Contract Data
			ContractsDto contractsData = new ContractsDto()
			{
				Id = -1,
				Updateable = UpdateType.Upsert,
				CageCode = cageCode.CageCode,
				CustomerSubmittalDate = DateTime.Now.Date,
				ProposalId = prop.Id,
				ContractsCorrespondenceLogNumber = "booo hoo"
			};

			using (TransactionScope scope = new TransactionScope())
			{
				contractsLoader.Save(contractsData);

				scope.Complete();
			}

			CoverSheetDataDto actualData = sut.GetCoverSheetDataById(prop.Id);

			CoverSheetDataDto expectedData = new CoverSheetDataDto()
			{
				CageCode = cageCode.CageCode,
				OfferorAddress = new List<string>() { cageCode.Address1, cageCode.Address2, cageCode.City, cageCode.State, cageCode.Zip },
				ContractActionType = prop.ContractActionType,
				CoverSheetApproverSignedDate = prop.CoverSheetApproverSignedDate,
				OtherContractActionType = prop.ContractActionTypeOtherText,
				ContractTypeGroup = (int)prop.ContractTypeGroup,
				CustomerSubmittalDate = contractsData.CustomerSubmittalDate,
				IsCCPDRequired = prop.IsCCPDRequired,

				// missing checklist data..
				CostThroughCom = null,
				ProfitFee = null,
				LMSpaceTotalPrice = null,

				// missing roles
				ContractsLead = null,
				CoverSheetApproverNtid = null
			};


			Assert.IsNotNull(actualData);
			DtoAssertHelpers.AssertDtos(expectedData, actualData);
		}

		/// <summary>
		/// Get Cover Sheet Data by Id - Without Cage Code
		/// </summary>
		[TestMethod]
		public void GetCoverSheetDataByIdTest_WithoutCageCode()
		{
			CoverSheetDataLoader sut = new CoverSheetDataLoader();

			ContractsLoader contractsLoader = new ContractsLoader();
			ProposalPermissionLoader proposalPermissionLoader = new ProposalPermissionLoader();
			UserLoader userLoader = new UserLoader();

			int userId = userLoader.GetByNtid("paliderd").Id;

			// Proposal Dto
			ProposalDto prop = testData.GetProposal(true);

			// Proposal Contract Data
			ContractsDto contractsData = new ContractsDto()
			{
				Id = -1,
				Updateable = UpdateType.Upsert,
				CustomerSubmittalDate = DateTime.Now.Date,
				ProposalId = prop.Id,
				ContractsCorrespondenceLogNumber = "booo hoo"
			};

			List<ProposalPermissionDto> permissions = new List<ProposalPermissionDto>()
			{
				new ProposalPermissionDto()
				{
					ProposalID = prop.Id,
					Updateable = UpdateType.Upsert,
					Id = -1,
					Role = PtmRole.ContractsPOC,
					UserId = userId
				},
				new ProposalPermissionDto()
				{
					ProposalID = prop.Id,
					Updateable = UpdateType.Upsert,
					Id = -2,
					Role = PtmRole.CoverSheetApprover,
					UserId = userId
				}
			};

			using (TransactionScope scope = new TransactionScope())
			{
				contractsLoader.Save(contractsData);
				proposalPermissionLoader.Save(permissions);

				scope.Complete();
			}

			CoverSheetDataDto actualData = sut.GetCoverSheetDataById(prop.Id);

			CoverSheetDataDto expectedData = new CoverSheetDataDto()
			{
				CoverSheetApproverNtid = "paliderd",
				ContractActionType = prop.ContractActionType,
				ContractsLead = "paliderd",
				CoverSheetApproverSignedDate = prop.CoverSheetApproverSignedDate,
				OtherContractActionType = prop.ContractActionTypeOtherText,
				ContractTypeGroup = (int)prop.ContractTypeGroup,
				CustomerSubmittalDate = contractsData.CustomerSubmittalDate,
				IsCCPDRequired = prop.IsCCPDRequired,

				// missing checklist data..
				CostThroughCom = null,
				ProfitFee = null,
				LMSpaceTotalPrice = null,

				// missing cage codes
				CageCode = null,
				OfferorAddress = null
			};


			Assert.IsNotNull(actualData);
			DtoAssertHelpers.AssertDtos(expectedData, actualData);
		}

		/// <summary>
		/// Get Cover Sheet Data by Id - Without Checklist
		/// </summary>
		[TestMethod]
		public void GetCoverSheetDataByIdTest_WithoutContractsData()
		{
			CoverSheetDataLoader sut = new CoverSheetDataLoader();

			ProposalPermissionLoader proposalPermissionLoader = new ProposalPermissionLoader();
			UserLoader userLoader = new UserLoader();

			int userId = userLoader.GetByNtid("paliderd").Id;

			// Proposal Dto
			ProposalDto prop = testData.GetProposal(true);

			List<ProposalPermissionDto> permissions = new List<ProposalPermissionDto>()
			{
				new ProposalPermissionDto()
				{
					ProposalID = prop.Id,
					Updateable = UpdateType.Upsert,
					Id = -1,
					Role = PtmRole.ContractsPOC,
					UserId = userId
				},
				new ProposalPermissionDto()
				{
					ProposalID = prop.Id,
					Updateable = UpdateType.Upsert,
					Id = -2,
					Role = PtmRole.CoverSheetApprover,
					UserId = userId
				}
			};

			using (TransactionScope scope = new TransactionScope())
			{
				proposalPermissionLoader.Save(permissions);

				scope.Complete();
			}

			CoverSheetDataDto actualData = sut.GetCoverSheetDataById(prop.Id);

			CoverSheetDataDto expectedData = new CoverSheetDataDto()
			{
				CoverSheetApproverNtid = "paliderd",
				ContractActionType = prop.ContractActionType,
				ContractsLead = "paliderd",
				CoverSheetApproverSignedDate = prop.CoverSheetApproverSignedDate,
				OtherContractActionType = prop.ContractActionTypeOtherText,
				ContractTypeGroup = (int)prop.ContractTypeGroup,
				IsCCPDRequired = prop.IsCCPDRequired,

				// missing checklist data..
				CostThroughCom = null,
				ProfitFee = null,
				LMSpaceTotalPrice = null,

				// missing contracts data
				CageCode = null,
				OfferorAddress = null,
				CustomerSubmittalDate = null,
			};

			Assert.IsNotNull(actualData);
			DtoAssertHelpers.AssertDtos(expectedData, actualData);
		}
	}
}