// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.ActionLogic
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web.Mvc;
	using GenBOE.DataBridge.DTO;
	using GenTRAC.ActionLogic;
	using GenTRAC.ActionLogic.Email;
	using GenTRAC.ActionLogic.GeneralHelper;
	using GenTRAC.ActionLogic.Mediator;
	using GenTRAC.ActionLogic.ModelView;
	using GenTRAC.ActionLogic.ModelView.Contracts;
	using GenTRAC.ActionLogic.ModelView.Proposals;
	using GenTRAC.ActionLogic.Validation;
	using GenTRAC.DataBridge.Common.Security;
	using GenTRAC.DataBridge.DTO;
	using GenTRAC.DataBridge.DTO.Contracts;
	using GenTRAC.Objects;
	using GenTRAC.Objects.FullObject;
	using GenTRAC.Tests.Helper;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.PickList;
	using Microsoft.Practices.Unity;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	/// <summary>
	/// Tests for the Contracts Controller Logic
	/// </summary>
	[TestClass]
	public class ContractsControllerLogicTest
	{
		#region Properties

		/// <summary>
		/// Security Access
		/// </summary>
		private Mock<ISecurityAccess> securityAccess = null;

		/// <summary>
		/// Proposal Loader
		/// </summary>
		private Mock<IProposalLoader> proposalLoader = null;

		/// <summary>
		/// The user mapper
		/// </summary>
		private Mock<IUserMapper> userMapper = null;

		/// <summary>
		/// Retriever
		/// </summary>
		private Mock<IRetriever> retriever = null;

		/// <summary>
		/// Object Factory
		/// </summary>
		private Mock<IFullObjectFactory> objectFactory = null;

		/// <summary>
		/// Approvals Loader
		/// </summary>
		private Mock<IApprovalsLoader> approvalsLoader;

		/// <summary>
		/// proposal checklist loader
		/// </summary>
		private Mock<IProposalChecklistLoader> proposalChecklistLoader = null;

		/// <summary>
		/// checklist mediator
		/// </summary>
		private Mock<IChecklistMediator> checklistMediator = null;

		/// <summary>
		/// propsoal mediator
		/// </summary>
		private Mock<IProposalMediator> proposalMediator = null;

		/// <summary>
		/// Contracts Loader
		/// </summary>
		private Mock<IContractsLoader> contractsLoader = null;

		/// <summary>
		/// Cage Codes Loader
		/// </summary>
		private Mock<ICageCodesLoader> cageCodesLoader = null;

		/// <summary>
		/// Emailer
		/// </summary>
		private Mock<IPtmEmailer> emailer;

		/// <summary>
		/// Proposal Logic
		/// </summary>
		private Mock<ProposalControllerLogic> proposalLogic;

		/// <summary>
		/// Approvals Logic
		/// </summary>
		private Mock<ApprovalsControllerLogic> approvalsLogic;

		/// <summary>
		/// AD utilities
		/// </summary>
		private Mock<IActiveDirectoryUtilities> activeDirectoryUtilities;

		/// <summary>
		/// Picklist Mapper
		/// </summary>
		private Mock<IPickListMapper> pickListMapper;

		#endregion

		/// <summary>
		/// Create the system
		/// </summary>
		/// <returns>A ContractsControllerLogic object</returns>
		private ContractsControllerLogic CreateSystem()
		{
			this.securityAccess = new Mock<ISecurityAccess>();
			this.proposalLoader = new Mock<IProposalLoader>();
			this.userMapper = new Mock<IUserMapper>();
			this.objectFactory = new Mock<IFullObjectFactory>();
			this.checklistMediator = new Mock<IChecklistMediator>();
			this.proposalChecklistLoader = new Mock<IProposalChecklistLoader>();
			this.proposalMediator = new Mock<IProposalMediator>();
			this.approvalsLoader = new Mock<IApprovalsLoader>();
			this.contractsLoader = new Mock<IContractsLoader>();
			this.cageCodesLoader = new Mock<ICageCodesLoader>();
			this.emailer = new Mock<IPtmEmailer>();
			IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.emailer.Object);
			this.retriever = new Mock<IRetriever>();
			IES.Common.classes.GenBOEUnityContainer.Container.RegisterInstance(this.retriever.Object);
			this.activeDirectoryUtilities = new Mock<IActiveDirectoryUtilities>();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IActiveDirectoryUtilities), activeDirectoryUtilities.Object);
			this.proposalLogic = new Mock<ProposalControllerLogic>(this.securityAccess.Object, this.proposalLoader.Object, It.IsAny<IValidationMethods>(), this.proposalMediator.Object, this.userMapper.Object,
				this.objectFactory.Object, It.IsAny<IOrgStructureDataMapper>(), It.IsAny<IProposalPermissionMediator>(), It.IsAny<ISecurityInformation>(), It.IsAny<ICacheDataLoader>(),
				It.IsAny<IPickListMapper>(), It.IsAny<IUserLoader>(), this.approvalsLoader.Object, this.proposalChecklistLoader.Object, this.checklistMediator.Object, It.IsAny<IWorkspaceDTODataLoader>(),
				It.IsAny<IPermissionsDTODataLoader>(), this.emailer.Object);
			this.approvalsLogic = new Mock<ApprovalsControllerLogic>(this.securityAccess.Object, this.proposalLoader.Object, this.userMapper.Object, It.IsAny<IUserLoader>(), this.emailer.Object,
				this.objectFactory.Object, this.approvalsLoader.Object, this.proposalMediator.Object, this.proposalChecklistLoader.Object, this.checklistMediator.Object, It.IsAny<ApprovalEmailer>(),
				It.IsAny<IAttachmentLoader>(), It.IsAny<IActiveDirectoryUtilities>());
			pickListMapper = new Mock<IPickListMapper>();

			pickListMapper.Setup(x => x.GetSelectListPickList(PickListEnum.LineOfBusiness, null, true, false))
				.Returns(new Collection<SelectListItem>());

			ContractsControllerLogic logic = new ContractsControllerLogic(this.securityAccess.Object, this.proposalLoader.Object, this.userMapper.Object,
						this.objectFactory.Object, this.approvalsLoader.Object, this.proposalChecklistLoader.Object, this.checklistMediator.Object,
						this.proposalMediator.Object, this.contractsLoader.Object, this.cageCodesLoader.Object, this.emailer.Object, this.approvalsLogic.Object,
						pickListMapper.Object);

			return logic;
		}

		/// <summary>
		/// Test the GetDataForProposalContracts method. 
		/// </summary>
		[TestMethod]
		public void GetDataForProposalContractsTest()
		{
			// Create new Contracts Controller Logic object
			ContractsControllerLogic sut = this.CreateSystem();

			int proposalId = 5;

			ICollection<CageCodeDTO> cageCodesDTOsSetup = new List<CageCodeDTO>();

			cageCodesDTOsSetup.Add(new CageCodeDTO()
			{
				CageCode = "ABC123",
				Address1 = "Test1",
				Address2 = "Test2",
				City = "TestCity",
				State = "TestState",
				Zip = "TestZip"
			});
			cageCodesDTOsSetup.Add(new CageCodeDTO()
			{
				CageCode = "ABC1234",
				Address1 = "Test11",
				Address2 = "Test22",
				City = "TestCityy",
				State = "TestStatee",
				Zip = "TestZipp"
			});
			cageCodesDTOsSetup.Add(new CageCodeDTO()
			{
				CageCode = "ABC12345",
				Address1 = "Test111",
				Address2 = "Test222",
				City = "TestCityyy",
				State = "TestStateee",
				Zip = "TestZippp"
			});

			ContractsDto contractDto = new ContractsDto()
			{
				Updateable = UpdateType.Upsert,
				ProposalId = proposalId,
				PreviouslySubmittedROM = 2,
				CustomerDueDate = DateTime.Now,
				CustomerSubmittalDate = DateTime.Now,
				ContractsCorrespondenceLogNumber = "Test_Log_Number",
				FinalNegotiatedValue = 3,
				NegotiationsSubmitted = DateTime.Now,
				EppDelegationAuthority = 3,
				ScheduledActualBidEppDate = DateTime.Now,
				ScheduledActualProgramEppDate = DateTime.Now,
				ScheduledActualLobEppDate = DateTime.Now,
				ScheduledActualPreSpaceEppDate = DateTime.Now,
				ScheduledActualSpaceEppDate = DateTime.Now,
				ScheduledActualPreCorporateEppDate = DateTime.Now,
				ScheduledActualCorporateEppDate = DateTime.Now,
				ScheduledActualMissionSegmentEppDate = DateTime.Now,
				PlannedBidEppDate = DateTime.Now,
				PlannedProgramEppDate = DateTime.Now,
				PlannedLobEppDate = DateTime.Now,
				PlannedPreSpaceEppDate = DateTime.Now,
				PlannedSpaceEppDate = DateTime.Now,
				PlannedPreCorporateEppDate = DateTime.Now,
				PlannedCorporateEppDate = DateTime.Now,
				PlannedMissionSegmentEppDate = DateTime.Now,
				EppRosDelegationNotes = "Test EppRosDelegationNotes",
				LmWon = false,
				ModCompletedDate = DateTime.Now,
				CageCode = "Test"
			};

			ICollection<SelectListItem> getRomProposalOptions = new Collection<SelectListItem>()
			{
				new SelectListItem()
				{
					Selected = true,
					Text = "TestValue",
					Value = contractDto.PreviouslySubmittedROM.ToString()
				},
				new SelectListItem()
				{
					Selected = true,
					Text = "TestValue2",
					Value = (contractDto.PreviouslySubmittedROM + 1).ToString()
				}
			};

			Tuple<DateTime?, decimal?> getRomDateAndValue = new Tuple<DateTime?, decimal?>(DateTime.Now, 0);
			this.proposalLoader.Setup(x => x.GetRomDateAndValue(contractDto.ProposalId)).Returns(getRomDateAndValue);

			this.proposalLoader.Setup(x => x.GetRomProposalOptions(contractDto.PreviouslySubmittedROM)).Returns(getRomProposalOptions);

			this.contractsLoader.Setup(x => x.GetContractForProposal(proposalId)).Returns(contractDto);

			this.cageCodesLoader.Setup(x => x.GetAllCageCodesData()).Returns(cageCodesDTOsSetup);

			ProposalDto proposal = TestProposalHelper.GetProposalDtoForMocks(proposalId);
			this.proposalLoader.Setup(x => x.GetById(proposalId)).Returns(proposal);
			this.objectFactory.Setup(x => x.CreateFullProposal(proposal)).Returns(TestProposalHelper.GetFullProposalForMocks(proposalId, ProposalStatus.InProgress));
			this.retriever.Setup(x => x.GetCurrentUser()).Returns(new UserDTO() { Id = 1 });
			this.retriever.Setup(x => x.GetProposalPermissions(proposalId)).Returns(TestProposalHelper.GetPermissionsForMocks());

			ContractsModelView contractsModelView = null;
			contractsModelView = sut.GetDataForProposalContracts(proposalId).Result;

			Assert.IsNotNull(contractsModelView);
			Assert.IsTrue(contractsModelView.ProposalId > 0);
			Assert.AreEqual(contractDto.ProposalId, contractsModelView.ProposalId);
			Assert.AreEqual(contractDto.CustomerDueDate, contractsModelView.CustomerDueDt);
			Assert.AreEqual(contractDto.CustomerSubmittalDate, contractsModelView.CustomerSubmittalDt);
			Assert.AreEqual(contractDto.ContractsCorrespondenceLogNumber, contractsModelView.ContractsCorrespondenceLogNumber);
			Assert.AreEqual(contractDto.FinalNegotiatedValue, contractsModelView.FinalNegotiatedValueLong);
			Assert.AreEqual(contractDto.NegotiationsSubmitted, contractsModelView.NegotiationsSubmittedDt);
			Assert.AreEqual(contractDto.PreviouslySubmittedROM.ToString(), contractsModelView.PreviouslySubmittedRoms.ElementAtOrDefault(0).Value);
			Assert.AreEqual(contractDto.PreviouslySubmittedROM, contractsModelView.PreviouslySubmittedROM);
			Assert.AreEqual(contractDto.EppDelegationAuthority, (int)contractsModelView.EppDelegationAuthority);
			Assert.AreEqual(contractDto.ScheduledActualBidEppDate, contractsModelView.ScheduledActualBidEppDate);
			Assert.AreEqual(contractDto.ScheduledActualProgramEppDate, contractsModelView.ScheduledActualProgramEppDate);
			Assert.AreEqual(contractDto.ScheduledActualLobEppDate, contractsModelView.ScheduledActualLobEppDate);
			Assert.AreEqual(contractDto.ScheduledActualPreSpaceEppDate, contractsModelView.ScheduledActualPreSpaceEppDate);
			Assert.AreEqual(contractDto.ScheduledActualSpaceEppDate, contractsModelView.ScheduledActualSpaceEppDate);
			Assert.AreEqual(contractDto.ScheduledActualPreCorporateEppDate, contractsModelView.ScheduledActualPreCorporateEppDate);
			Assert.AreEqual(contractDto.ScheduledActualCorporateEppDate, contractsModelView.ScheduledActualCorporateEppDate);
			Assert.AreEqual(contractDto.ScheduledActualMissionSegmentEppDate, contractsModelView.ScheduledActualMissionSegmentEppDate);
			Assert.AreEqual(contractDto.PlannedBidEppDate, contractsModelView.PlannedBidEppDate);
			Assert.AreEqual(contractDto.PlannedProgramEppDate, contractsModelView.PlannedProgramEppDate);
			Assert.AreEqual(contractDto.PlannedLobEppDate, contractsModelView.PlannedLobEppDate);
			Assert.AreEqual(contractDto.PlannedPreSpaceEppDate, contractsModelView.PlannedPreSpaceEppDate);
			Assert.AreEqual(contractDto.PlannedSpaceEppDate, contractsModelView.PlannedSpaceEppDate);
			Assert.AreEqual(contractDto.PlannedPreCorporateEppDate, contractsModelView.PlannedPreCorporateEppDate);
			Assert.AreEqual(contractDto.PlannedCorporateEppDate, contractsModelView.PlannedCorporateEppDate);
			Assert.AreEqual(contractDto.PlannedMissionSegmentEppDate, contractsModelView.PlannedMissionSegmentEppDate);
			Assert.AreEqual(contractDto.EppRosDelegationNotes, contractsModelView.EppRosDelegationNotes);
			Assert.AreEqual(contractDto.LmWon, contractsModelView.LmWon);
			Assert.AreEqual(contractDto.ModCompletedDate, contractsModelView.ModCompletedDate);
		}

		/// <summary>
		/// Test the GetRomDateAndValue method. 
		/// </summary>
		[TestMethod]
		public void GetRomDateAndValueTest()
		{
			// Create new Contracts Controller Logic object
			ContractsControllerLogic sut = this.CreateSystem();

			int proposalId = 5;

			Tuple<DateTime?, decimal?> testDateAndValue = new Tuple<DateTime?, decimal?>(DateTime.Now, 0);
			this.proposalLoader.Setup(x => x.GetRomDateAndValue(proposalId)).Returns(testDateAndValue);

			Tuple<DateTime?, decimal?> getRomDateAndValue = null;

			getRomDateAndValue = sut.GetRomDateAndValue(proposalId);

			Assert.IsNotNull(getRomDateAndValue);
			Assert.AreEqual(getRomDateAndValue, testDateAndValue);
		}

		/// <summary>
		/// Test throw execption for the GetRomDateAndValue method. 
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException), "Invalid Proposal ID, is less than 0")]
		public void GetRomDateAndValueTestNull()
		{
			// Create new Contracts Controller Logic object
			ContractsControllerLogic sut = this.CreateSystem();

			_ = sut.GetRomDateAndValue(-1);
		}

		/// <summary>
		/// Test for setting the proposal status to Lost
		/// </summary>
		/// <returns>Async Task</returns>
		[TestMethod]
		public async Task SetProposalLostTest()
		{
			ContractsControllerLogic sut = this.CreateSystem();
			FullProposal fp = TestProposalHelper.GetFullProposalForMocks(1, ProposalStatus.PendingCertification);
			List<string> errors = new List<string>();

			ContractsDto contractDto = new ContractsDto()
			{
				CustomerDueDate = DateTime.Now,
				CustomerSubmittalDate = DateTime.Now
			};

			this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(TestProposalHelper.GetPermissionsForMocks());
			this.retriever.Setup(x => x.GetCurrentUser()).Returns(TestProposalHelper.GetLeadContractsUserForMocks());
			this.proposalLoader.Setup(x => x.GetById(1)).Returns(TestProposalHelper.GetProposalDtoForMocks(1));
			this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<FullProposal>()));
			this.proposalLogic.Setup(x => x.GetDataForProposalApprovals(fp.Id, false)).Returns(new ProposalApprovalsModelView()); // intercept and don't return data
			this.proposalLogic.Setup(x => x.GetDataForProposalUserInformation(fp.Id, It.IsAny<bool>())).Returns(new ProposalUserInformationModelView());
			this.contractsLoader.Setup(x => x.GetContractForProposal(It.IsAny<int>())).Returns(contractDto);
			this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(fp.CurrentUser);
			this.userMapper.Setup(x => x.GetById(It.IsAny<int>())).Returns(fp.CurrentUser);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(fp);

			await sut.SetProposalLost(fp.Id, errors);

			Assert.IsTrue(fp.ProposalStatus == ProposalStatus.Lost);
		}

		/// <summary>
		/// Tests to ensure setting "Lost" fails when not a Contracts PoC (or Backup)
		/// </summary>
		/// <returns>Async Task</returns>
		[TestMethod]
		public async Task SetProposalLostInsufficientAccessTest()
		{
			ContractsControllerLogic sut = this.CreateSystem();
			FullProposal fp = TestProposalHelper.GetFullProposalForMocks(1, ProposalStatus.PendingCertification);
			List<string> errors = new List<string>();

			ContractsDto contractDto = new ContractsDto()
			{
				CustomerDueDate = DateTime.Now,
				CustomerSubmittalDate = DateTime.Now
			};

			this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(TestProposalHelper.GetPermissionsForMocks());
			this.retriever.Setup(x => x.GetCurrentUser()).Returns(TestProposalHelper.GetLeadEstimatorUserForMocks());
			this.proposalLoader.Setup(x => x.GetById(1)).Returns(TestProposalHelper.GetProposalDtoForMocks(1));
			this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<FullProposal>()));
			this.proposalLogic.Setup(x => x.GetDataForProposalApprovals(fp.Id, false)).Returns(new ProposalApprovalsModelView()); // intercept and don't return data
			this.proposalLogic.Setup(x => x.GetDataForProposalUserInformation(fp.Id, It.IsAny<bool>())).Returns(new ProposalUserInformationModelView());
			this.contractsLoader.Setup(x => x.GetContractForProposal(It.IsAny<int>())).Returns(contractDto);
			this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(fp.CurrentUser);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(fp);

			await sut.SetProposalLost(fp.Id, errors);

			Assert.AreEqual(errors.First(), Constants.INSUFFICIENT_PERMISSIONS_FOR_LOST);
		}

		/// <summary>
		/// Tests to ensure setting "Lost" fails when in incorrect status
		/// </summary>
		/// <returns>Async Task</returns>
		[TestMethod]
		public async Task SetProposalLostWrongStatusTest()
		{
			ContractsControllerLogic sut = this.CreateSystem();
			FullProposal fp = TestProposalHelper.GetFullProposalForMocks(1, ProposalStatus.NoBid);
			List<string> errors = new List<string>();

			ContractsDto contractDto = new ContractsDto()
			{
				CustomerDueDate = DateTime.Now,
				CustomerSubmittalDate = DateTime.Now
			};

			this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(TestProposalHelper.GetPermissionsForMocks());
			this.retriever.Setup(x => x.GetCurrentUser()).Returns(TestProposalHelper.GetLeadContractsUserForMocks());
			this.proposalLoader.Setup(x => x.GetById(1)).Returns(TestProposalHelper.GetProposalDtoForMocks(1));
			this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<FullProposal>()));
			this.proposalLogic.Setup(x => x.GetDataForProposalApprovals(fp.Id, false)).Returns(new ProposalApprovalsModelView()); // intercept and don't return data
			this.proposalLogic.Setup(x => x.GetDataForProposalUserInformation(fp.Id, It.IsAny<bool>())).Returns(new ProposalUserInformationModelView());
			this.contractsLoader.Setup(x => x.GetContractForProposal(It.IsAny<int>())).Returns(contractDto);
			this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(fp.CurrentUser);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(fp);

			await sut.SetProposalLost(fp.Id, errors);

			Assert.AreEqual(errors.First(), Constants.INVALID_STATUS_FOR_LOST);
		}

		/// <summary>
		/// Test for setting the proposal status to Lost
		/// </summary>
		/// <returns>Async Task</returns>
		[TestMethod]
		public async Task SetProposalLostMissingDueDate()
		{
			ContractsControllerLogic sut = this.CreateSystem();
			FullProposal fp = TestProposalHelper.GetFullProposalForMocks(1, ProposalStatus.PendingCertification);
			List<string> errors = new List<string>();

			ContractsDto contractDto = new ContractsDto()
			{
				CustomerSubmittalDate = DateTime.Now
			};

			this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(TestProposalHelper.GetPermissionsForMocks());
			this.retriever.Setup(x => x.GetCurrentUser()).Returns(TestProposalHelper.GetLeadContractsUserForMocks());
			this.proposalLoader.Setup(x => x.GetById(1)).Returns(TestProposalHelper.GetProposalDtoForMocks(1));
			this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<FullProposal>()));
			this.proposalLogic.Setup(x => x.GetDataForProposalApprovals(fp.Id, false)).Returns(new ProposalApprovalsModelView()); // intercept and don't return data
			this.proposalLogic.Setup(x => x.GetDataForProposalUserInformation(fp.Id, It.IsAny<bool>())).Returns(new ProposalUserInformationModelView());
			this.contractsLoader.Setup(x => x.GetContractForProposal(It.IsAny<int>())).Returns(contractDto);
			this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(fp.CurrentUser);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(fp);

			await sut.SetProposalLost(fp.Id, errors);

			Assert.AreEqual(errors.First(), Constants.DUE_DATE_REQUIRED_FOR_LOST);
		}

		/// <summary>
		/// Test for setting the proposal status to Lost
		/// </summary>
		/// <returns>Async Task</returns>
		[TestMethod]
		public async Task SetProposalLostTestMissingSubmittalDate()
		{
			ContractsControllerLogic sut = this.CreateSystem();
			FullProposal fp = TestProposalHelper.GetFullProposalForMocks(1, ProposalStatus.PendingCertification);
			List<string> errors = new List<string>();

			ContractsDto contractDto = new ContractsDto()
			{
				CustomerDueDate = DateTime.Now
			};

			this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(TestProposalHelper.GetPermissionsForMocks());
			this.retriever.Setup(x => x.GetCurrentUser()).Returns(TestProposalHelper.GetLeadContractsUserForMocks());
			this.proposalLoader.Setup(x => x.GetById(1)).Returns(TestProposalHelper.GetProposalDtoForMocks(1));
			this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<FullProposal>()));
			this.proposalLogic.Setup(x => x.GetDataForProposalApprovals(fp.Id, false)).Returns(new ProposalApprovalsModelView()); // intercept and don't return data
			this.proposalLogic.Setup(x => x.GetDataForProposalUserInformation(fp.Id, It.IsAny<bool>())).Returns(new ProposalUserInformationModelView());
			this.contractsLoader.Setup(x => x.GetContractForProposal(It.IsAny<int>())).Returns(contractDto);
			this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(fp.CurrentUser);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(fp);

			await sut.SetProposalLost(fp.Id, errors);

			Assert.AreEqual(errors.First(), Constants.SUBMITTAL_DATE_REQUIRED_FOR_LOST);
		}

		/// <summary>
		/// Test for setting the proposal status to No Bid
		/// </summary>
		[TestMethod]
		public void SetProposalNoBidTest()
		{
			ContractsControllerLogic sut = this.CreateSystem();
			FullProposal fp = TestProposalHelper.GetFullProposalForMocks(1, ProposalStatus.PendingCertification);

			this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(TestProposalHelper.GetPermissionsForMocks());
			this.retriever.Setup(x => x.GetCurrentUser()).Returns(TestProposalHelper.GetLeadContractsUserForMocks());
			this.proposalLoader.Setup(x => x.GetById(1)).Returns(TestProposalHelper.GetProposalDtoForMocks(1));
			this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<FullProposal>()));
			this.proposalLogic.Setup(x => x.GetDataForProposalApprovals(fp.Id, false)).Returns(new ProposalApprovalsModelView()); // intercept and don't return data
			this.proposalLogic.Setup(x => x.GetDataForProposalUserInformation(fp.Id, It.IsAny<bool>())).Returns(new ProposalUserInformationModelView());
			this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(fp.CurrentUser);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(fp);

			sut.SetProposalToNoBid(fp.Id);

			Assert.IsTrue(fp.ProposalStatus == ProposalStatus.NoBid);
		}

		/// <summary>
		/// Test for reverting the proposal status from No Bid
		/// </summary>
		[TestMethod]
		public void SetProposalRevertNoBidTest()
		{
			ContractsControllerLogic sut = this.CreateSystem();
			FullProposal fp = TestProposalHelper.GetFullProposalForMocks(1, ProposalStatus.NoBid);

			this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(TestProposalHelper.GetPermissionsForMocks());
			this.retriever.Setup(x => x.GetCurrentUser()).Returns(TestProposalHelper.GetLeadContractsUserForMocks());
			this.proposalLoader.Setup(x => x.GetById(1)).Returns(TestProposalHelper.GetProposalDtoForMocks(1));
			this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<FullProposal>()));
			this.proposalLogic.Setup(x => x.GetDataForProposalApprovals(fp.Id, false)).Returns(new ProposalApprovalsModelView()); // intercept and don't return data
			this.proposalLogic.Setup(x => x.GetDataForProposalUserInformation(fp.Id, It.IsAny<bool>())).Returns(new ProposalUserInformationModelView());
			this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(fp.CurrentUser);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(fp);

			sut.RevertProposalFromNoBid(fp.Id);

			Assert.IsTrue(fp.ProposalStatus == ProposalStatus.InProgress);
		}

		/// <summary>
		/// Test the GetByProposalIds method. 
		/// </summary>
		[TestMethod]
		public void SaveContractTest()
		{
			// Create new Contracts Controller Logic object
			// ContractsControllerLogic sut = this.CreateSystem();

			// TODO: SaveContract unit test.

			// Assert that the id return from the save is the one we expect.
			// Verify method - that it ran, but if we get an Id, may not be needed) 
		}

		/// <summary>
		/// Test the ValidateContract method. 
		/// </summary>
		[TestMethod]
		public void ValidateContractTest()
		{
			// TODO: After Validation methods are implemented
		}

		/// <summary>
		/// Specifying Delegation Authority of "Program", requires ProgramEppDate
		/// </summary>
		[TestMethod]
		public void EppRequiredDatesListProgramTest()
		{
			EppDelegationDatesHelper edh = new EppDelegationDatesHelper();
			List<string> dates = new List<string>();

			dates = edh.GetRequiredEppDatesForDelegation(EppDelegationAuthority.Program);

			Assert.AreEqual("ScheduledActualProgramEppDate", dates.First());
			Assert.AreEqual(1, dates.Count);
		}

		/// <summary>
		/// Speicifying Delegation Authority of "Corporate" requires all dates, and they are in correct order
		/// </summary>
		[TestMethod]
		public void EppRequiredDatesListCorporateTest()
		{
			EppDelegationDatesHelper edh = new EppDelegationDatesHelper();
			List<string> dates = new List<string>();

			dates = edh.GetRequiredEppDatesForDelegation(EppDelegationAuthority.Corporate);

			Assert.AreEqual(nameof(ContractsDto.ScheduledActualCorporateEppDate), dates.First());
			Assert.AreEqual(nameof(ContractsDto.ScheduledActualPreCorporateEppDate), dates.ElementAt(1));
			Assert.AreEqual(nameof(ContractsDto.ScheduledActualSpaceEppDate), dates.ElementAt(2));
			Assert.AreEqual(nameof(ContractsDto.ScheduledActualPreSpaceEppDate), dates.ElementAt(3));
			Assert.AreEqual(nameof(ContractsDto.ScheduledActualLobEppDate), dates.ElementAt(4));
			Assert.AreEqual(nameof(ContractsDto.ScheduledActualMissionSegmentEppDate), dates.ElementAt(5));
			Assert.AreEqual(nameof(ContractsDto.ScheduledActualProgramEppDate), dates.Last());
			Assert.AreEqual(7, dates.Count);
		}

		/// <summary>
		/// Adding only those dates that are required, ensure validation passes
		/// </summary>
		[TestMethod]
		public void EppDatesForSpaceDelegationPassTest()
		{
			EppDelegationDatesHelper edh = new EppDelegationDatesHelper();
			List<string> errors = new List<string>();

			ContractsDto dto = new ContractsDto
			{
				PreviouslySubmittedROM = 12345,
				ContractsCorrespondenceLogNumber = "XYZ123",
				EppDelegationAuthority = (int)EppDelegationAuthority.Space,
				ScheduledActualSpaceEppDate = DateTime.Now,
				ScheduledActualPreSpaceEppDate = DateTime.Now,
				ScheduledActualProgramEppDate = DateTime.Now,
				ScheduledActualLobEppDate = DateTime.Now
			};

			bool isValid = edh.AreRequiredDatesPopulated(dto, errors);

			Assert.IsTrue(isValid);
			Assert.AreEqual(0, errors.Count);
		}

		/// <summary>
		/// Missing Line of Business date fails validation
		/// </summary>
		[TestMethod]
		public void EppDatesForSpaceDelegationMissingLoBTest()
		{
			EppDelegationDatesHelper edh = new EppDelegationDatesHelper();
			List<string> errors = new List<string>();

			ContractsDto dto = new ContractsDto
			{
				PreviouslySubmittedROM = 12345,
				ContractsCorrespondenceLogNumber = "XYZ123",
				EppDelegationAuthority = (int)EppDelegationAuthority.Space,
				ScheduledActualSpaceEppDate = DateTime.Now,
				ScheduledActualPreSpaceEppDate = DateTime.Now,
				ScheduledActualProgramEppDate = DateTime.Now
			};

			bool isValid = edh.AreRequiredDatesPopulated(dto, errors);

			Assert.IsFalse(isValid);
			Assert.AreEqual("Line of Business EPP Date required.", errors.First());
		}

		/// <summary>
		/// Ensure that all provide dates are sequential - whether they are required or not
		/// </summary>
		[TestMethod]
		public void EppDatesProvidedAreSequentialFailTest()
		{
			EppDelegationDatesHelper edh = new EppDelegationDatesHelper();
			List<string> errors = new List<string>();

			ContractsDto dto = new ContractsDto
			{
				PreviouslySubmittedROM = 12345,
				ContractsCorrespondenceLogNumber = "XYZ123",
				EppDelegationAuthority = (int)EppDelegationAuthority.Space,
				ScheduledActualPreCorporateEppDate = new DateTime(2022, 04, 06),
				ScheduledActualSpaceEppDate = new DateTime(2022, 04, 8),
				ScheduledActualPreSpaceEppDate = new DateTime(2022, 04, 07),
				ScheduledActualLobEppDate = new DateTime(2022, 04, 06),
				ScheduledActualProgramEppDate = new DateTime(2022, 04, 05),
			};

			bool pass = edh.AreEnteredDatesSequential(dto, errors);

			Assert.IsFalse(pass);
			Assert.IsTrue(errors.First().Contains("Space EPP Date must be before Pre-Corporate EPP Date"));
		}

		/// <summary>
		/// Get list of non-epp date related validation failures
		/// </summary>
		[TestMethod]
		public void ValidationFinalAllNonEppMissingTest()
		{
			ContractsControllerLogic sut = this.CreateSystem();
			List<string> messages = new List<string>();

			ContractsDto dto = new ContractsDto
			{
				ProposalId = 1,
				PreviouslySubmittedROM = 12345,
				ContractsCorrespondenceLogNumber = "XYZ123",
				EppDelegationAuthority = (int)EppDelegationAuthority.Space,
				ScheduledActualSpaceEppDate = DateTime.Now,
				ScheduledActualPreSpaceEppDate = DateTime.Now,
				ScheduledActualProgramEppDate = DateTime.Now.AddHours(-2),
				CageCode = "ABC123",
				ScheduledActualLobEppDate = DateTime.Now.AddHours(-1),
				CustomerDueDate = DateTime.Now,
				IsInsuranceDirect = TripleBooleanState.Yes,
				InsuranceType = InsuranceType.Space,
				ProposedInsurance = 1,
				NegotiatedInsurance = 1
			};

			ProposalDto proposal = new ProposalDto() { AgreementDate = DateTime.Now, IsRomNte = false };
			this.proposalLoader.Setup(x => x.GetById(dto.ProposalId)).Returns(proposal);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(new FullProposal(proposal));

			bool isValid = sut.ContractDataValidForCompleteProposalSave(dto, new FullProposal(proposal), messages);

			Assert.IsFalse(isValid);
			Assert.AreEqual(1, messages.Count);
			Assert.IsTrue(messages.Contains(Constants.INVALID_LM_WIN_LOSS));

			dto.LmWon = true;
			messages = new List<string>();
			isValid = sut.ContractDataValidForCompleteProposalSave(dto, new FullProposal(proposal), messages);

			Assert.IsFalse(isValid);
			Assert.AreEqual(3, messages.Count);
			Assert.IsTrue(messages.Contains(Constants.INVALID_MOD_COMPLETION_DATE));
			Assert.IsTrue(messages.Contains(Constants.INVALID_NEGOTIATIONS_SUBMITTED_DATE));
			Assert.IsTrue(messages.Contains(Constants.INVALID_FINAL_NEGOTIATED_VALUE));
		}

		/// <summary>
		/// Get list of non-epp date related validation failures; zero final negotiated value is valid
		/// </summary>
		[TestMethod]
		public void ValidationFinalAllNonEppZeroFinalNegotiatedValue()
		{
			ContractsControllerLogic sut = this.CreateSystem();
			List<string> messages = new List<string>();

			ContractsDto dto = new ContractsDto
			{
				ProposalId = 1,
				PreviouslySubmittedROM = 12345,
				ContractsCorrespondenceLogNumber = "XYZ123",
				EppDelegationAuthority = (int)EppDelegationAuthority.Space,
				ScheduledActualSpaceEppDate = DateTime.Now,
				ScheduledActualPreSpaceEppDate = DateTime.Now,
				ScheduledActualProgramEppDate = DateTime.Now,
				CageCode = "ABC123",
				ScheduledActualLobEppDate = DateTime.Now,
				FinalNegotiatedValue = 0,
				ModCompletedDate = DateTime.Now,
				NegotiationsSubmitted = DateTime.Now,
				LmWon = true,
				CustomerDueDate = DateTime.Now,
				IsInsuranceDirect = TripleBooleanState.Yes,
				InsuranceType = InsuranceType.Space,
				ProposedInsurance = 1,
				NegotiatedInsurance = 1
			};

			ProposalDto proposal = new ProposalDto() { AgreementDate = DateTime.Now.AddDays(-1) };
			this.proposalLoader.Setup(x => x.GetById(dto.ProposalId)).Returns(proposal);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(new FullProposal(proposal));

			bool isValid = sut.ContractDataValidForCompleteProposalSave(dto, new FullProposal(proposal), messages);

			Assert.IsTrue(isValid);
			Assert.AreEqual(0, messages.Count);
		}

		/// <summary>
		/// Runs through a valid test of getting all cage code data
		/// </summary>
		[TestMethod]
		public void GetAllCageCodesDataTest()
		{
			// Create new Contracts Controller Logic object
			ContractsControllerLogic sut = this.CreateSystem();

			ICollection<CageCodeDTO> cageCodesDTOsSetup = new List<CageCodeDTO>();

			cageCodesDTOsSetup.Add(new CageCodeDTO()
			{
				CageCode = "ABC123",
				Address1 = "Test1",
				Address2 = "Test2",
				City = "TestCity",
				State = "TestState",
				Zip = "TestZip"
			});
			cageCodesDTOsSetup.Add(new CageCodeDTO()
			{
				CageCode = "ABC1234",
				Address1 = "Test11",
				Address2 = "Test22",
				City = "TestCityy",
				State = "TestStatee",
				Zip = "TestZipp"
			});
			cageCodesDTOsSetup.Add(new CageCodeDTO()
			{
				CageCode = "ABC12345",
				Address1 = "Test111",
				Address2 = "Test222",
				City = "TestCityyy",
				State = "TestStateee",
				Zip = "TestZippp"
			});

			// When this method is called, return this instead:
			this.cageCodesLoader.Setup(x => x.GetAllCageCodesData()).Returns(cageCodesDTOsSetup);

			ICollection<CageCodeModelView> cageCodesDTOsActuals = sut.GetAllCageCodesData();

			Assert.IsTrue(cageCodesDTOsActuals.Any());
		}

		/// <summary>
		/// Runs through a valid test of getting data by a cage code
		/// </summary>
		[TestMethod]
		public void GetDataForCageCodeTest()
		{
			// Create new Contracts Controller Logic object
			ContractsControllerLogic sut = this.CreateSystem();

			CageCodeDTO cageCodesDTOSetup = new CageCodeDTO()
			{
				CageCode = "XYZ098",
				Address1 = "Address 1 Test",
				Address2 = "Address 2 Test",
				City = "City Test",
				State = "State Test",
				Zip = "Zip Test"
			};

			string matchingFakeCageCode = "XYZ098";
			string notMatchingFakeCageCode = "ABC123";

			// When this method is called, return this instead:
			this.cageCodesLoader.Setup(x => x.GetDataByCageCode(matchingFakeCageCode)).Returns(cageCodesDTOSetup);

			CageCodeModelView cageCodesDTOActual = sut.GetDataByCageCode(matchingFakeCageCode);

			Assert.IsNotNull(cageCodesDTOActual);
			Assert.AreEqual(cageCodesDTOSetup.CageCode, cageCodesDTOActual.CageCode);
			Assert.AreEqual(cageCodesDTOSetup.Address1, cageCodesDTOActual.Address1);
			Assert.AreEqual(cageCodesDTOSetup.Address2, cageCodesDTOActual.Address2);
			Assert.AreEqual(cageCodesDTOSetup.City, cageCodesDTOActual.City);
			Assert.AreEqual(cageCodesDTOSetup.State, cageCodesDTOActual.State);
			Assert.AreEqual(cageCodesDTOSetup.Zip, cageCodesDTOActual.Zip);

			Assert.AreNotEqual(notMatchingFakeCageCode, cageCodesDTOActual.CageCode);
		}

		/// <summary>
		/// Test ValidateContractModelView for a valid modelview
		/// </summary>
		[TestMethod]
		public void ValidateContractModelView()
		{
			ContractsControllerLogic sut = this.CreateSystem();

			ContractsModelView mv = new ContractsModelView
			{
				ProposalId = 1,
				CustomerSubmittalDt = DateTime.Now.AddDays(1),
				NegotiationsSubmittedDt = DateTime.Now
			};

			ProposalDto proposal = new ProposalDto() { AgreementDate = DateTime.Now.AddDays(-1) };
			this.proposalLoader.Setup(x => x.GetById(mv.ProposalId)).Returns(proposal);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(new FullProposal(proposal));

			ICollection<string> result = sut.ValidateContractModelView(mv, new FullProposal(proposal));

			Assert.IsFalse(result.Any());
		}

		/// <summary>
		/// Test ValidateContractModelView for an invalid Proposal Submittal Date to the Customer
		/// </summary>
		[TestMethod]
		public void ValidateContractModelView_InvalidProposalSubmittalDate()
		{
			ContractsControllerLogic sut = this.CreateSystem();

			ContractsModelView mv = new ContractsModelView
			{
				ProposalId = 1,
				CustomerSubmittalDt = DateTime.Now.AddDays(-1)
			};

			ProposalDto proposal = new ProposalDto() { AgreementDate = DateTime.Now.AddDays(-1) };
			this.proposalLoader.Setup(x => x.GetById(mv.ProposalId)).Returns(proposal);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(new FullProposal(proposal));
			this.retriever.Setup(x => x.GetProposalChecklists(proposal.Id)).Returns(new List<ProposalChecklistDto>() { new ProposalChecklistDto() { EstimatingSubmitsToContractsDate = DateTime.Now } });

			ICollection<string> result = sut.ValidateContractModelView(mv, new FullProposal(proposal));

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(Constants.INVALID_PROPOSAL_SUBMITTAL_DATE, result.First());
		}

		/// <summary>
		/// Test ValidateContractModelView for a valid Proposal Submittal Date to the Customer (w/ time being different)
		/// </summary>
		[TestMethod]
		public void ValidateContractModelView_ValidProposalSubmittalDate()
		{
			ContractsControllerLogic sut = this.CreateSystem();

			ContractsModelView mv = new ContractsModelView
			{
				ProposalId = 1,
				CustomerSubmittalDt = DateTime.Now.AddHours(-1)
			};

			ProposalDto proposal = new ProposalDto() { AgreementDate = DateTime.Now.AddDays(5) };
			this.proposalLoader.Setup(x => x.GetById(mv.ProposalId)).Returns(proposal);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(new FullProposal(proposal));
			this.retriever.Setup(x => x.GetProposalChecklists(proposal.Id)).Returns(new List<ProposalChecklistDto>() { new ProposalChecklistDto() { EstimatingSubmitsToContractsDate = DateTime.Now } });

			ICollection<string> result = sut.ValidateContractModelView(mv, new FullProposal(proposal));

			Assert.IsFalse(result.Any());
		}

		/// <summary>
		/// Test ValidateContractModelView for an invalid Date Confirmation of Negotiations Submitted
		/// </summary>
		[TestMethod]
		public void ValidateContractModelView_InvalidNegotiationsSubmitted()
		{
			ContractsControllerLogic sut = this.CreateSystem();

			ContractsModelView mv = new ContractsModelView
			{
				ProposalId = 1,
				CustomerSubmittalDt = DateTime.Now.AddDays(1),
				NegotiationsSubmittedDt = DateTime.Now
			};

			ProposalDto proposal = new ProposalDto() { AgreementDate = DateTime.Now.AddDays(1) };
			this.proposalLoader.Setup(x => x.GetById(mv.ProposalId)).Returns(proposal);
			this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(new FullProposal(proposal));

			ICollection<string> result = sut.ValidateContractModelView(mv, new FullProposal(proposal));

			Assert.IsTrue(result.Any());
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(Constants.INVALID_NEGOTIATIONS_SUBMITTED, result.First());
		}

		/// <summary>
		/// Test ContractDataValidForCompleteProposalSave sees Mission Segment EPP Delegation Authority as a valid selection
		/// </summary>
		[TestMethod]
		public void ContractDataValidForCompleteProposalSave_MissionSegmentDelegationValid()
		{
			ContractsControllerLogic sut = this.CreateSystem();

			ContractsDto dto = new ContractsDto
			{
				ProposalId = 1,
				EppDelegationAuthority = (int)EppDelegationAuthority.MissionSegment,
			};

			ProposalDto proposalDto = new ProposalDto() { AgreementDate = DateTime.Now.AddDays(-1) };
			FullProposal proposal = new FullProposal(proposalDto);
			List<string> messages = new List<string>();

			sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsFalse(messages.Contains(Constants.EPP_DELEGATION_AUTHORITY_INVALID));
		}

		/// <summary>
		/// Test ContractDataValidForCompleteProposalSave for validation of Mission Segment EPP Date
		/// </summary>
		[TestMethod]
		public void ContractDataValidForCompleteProposalSave_MissionSegmentEppDateValidation()
		{
			ContractsControllerLogic sut = this.CreateSystem();

			// With the LOB not set to NSS, a null Mission Segment EPP date is valid for all delegation authorities
			// First test Corporate
			ContractsDto dto = new ContractsDto
			{
				ProposalId = 1,
				CustomerSubmittalDate = DateTime.Now.AddDays(1),
				NegotiationsSubmitted = DateTime.Now,
				EppDelegationAuthority = (int)EppDelegationAuthority.Corporate,
				ScheduledActualProgramEppDate = DateTime.Now,
				ScheduledActualMissionSegmentEppDate = null,
				ScheduledActualLobEppDate = DateTime.Now,
				ScheduledActualPreSpaceEppDate = DateTime.Now,
				ScheduledActualSpaceEppDate = DateTime.Now,
				ScheduledActualPreCorporateEppDate = DateTime.Now,
				ScheduledActualCorporateEppDate = DateTime.Now,
				PreviouslySubmittedROM = 12345,
				ContractsCorrespondenceLogNumber = "XYZ123",
				CageCode = "ABC123",
				FinalNegotiatedValue = 0,
				ModCompletedDate = DateTime.Now,
				LmWon = true,
				CustomerDueDate = DateTime.Now,
				IsInsuranceDirect = TripleBooleanState.Yes,
				InsuranceType = InsuranceType.Space,
				ProposedInsurance = 1,
				NegotiatedInsurance = 1
			};

			ProposalDto proposalDto = new ProposalDto() { AgreementDate = DateTime.Now.AddDays(-1), LineOfBusinessID = 5 };
			FullProposal proposal = new FullProposal(proposalDto);
			List<string> messages = new List<string>();

			bool result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsTrue(result);
			Assert.IsFalse(messages.Any());

			// Space, not NSS
			dto.EppDelegationAuthority = (int)EppDelegationAuthority.Space;
			result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsTrue(result);
			Assert.IsFalse(messages.Any());

			// LoB, not NSS
			dto.EppDelegationAuthority = (int)EppDelegationAuthority.LoB;
			result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsTrue(result);
			Assert.IsFalse(messages.Any());

			// Mission Segment, not NSS
			dto.EppDelegationAuthority = (int)EppDelegationAuthority.MissionSegment;
			result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsTrue(result);
			Assert.IsFalse(messages.Any());

			// Never required for Program
			dto.EppDelegationAuthority = (int)EppDelegationAuthority.Program;
			result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsTrue(result);
			Assert.IsFalse(messages.Any());

			// Now set the Proposal's LoB to NSS - Mission Segment EPP date should be required for all but Program
			pickListMapper.Setup(x => x.GetSelectListPickList(PickListEnum.LineOfBusiness, null, true, false))
				.Returns(new Collection<SelectListItem>()
				{
					new SelectListItem() {
						Text = Constants.NSS_LOB_NAME,
						Value = proposal.LineOfBusinessID.ToString()
					}
				});

			// Corporate, is NSS
			dto.EppDelegationAuthority = (int)EppDelegationAuthority.Corporate;
			result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsFalse(result);
			Assert.AreEqual(1, messages.Count);
			Assert.AreEqual("Mission Segment EPP Date required.", messages.First());

			// Space, not NSS
			messages.Clear();
			dto.EppDelegationAuthority = (int)EppDelegationAuthority.Space;
			result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsFalse(result);
			Assert.AreEqual(1, messages.Count);
			Assert.AreEqual("Mission Segment EPP Date required.", messages.First());

			// LoB, not NSS
			messages.Clear();
			dto.EppDelegationAuthority = (int)EppDelegationAuthority.LoB;
			result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsFalse(result);
			Assert.AreEqual(1, messages.Count);
			Assert.AreEqual("Mission Segment EPP Date required.", messages.First());

			// Mission Segment, not NSS
			messages.Clear();
			dto.EppDelegationAuthority = (int)EppDelegationAuthority.MissionSegment;
			result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsFalse(result);
			Assert.AreEqual(1, messages.Count);
			Assert.AreEqual("Mission Segment EPP Date required.", messages.First());

			// Never required for Program
			messages.Clear();
			dto.EppDelegationAuthority = (int)EppDelegationAuthority.Program;
			result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsTrue(result);
			Assert.IsFalse(messages.Any());
		}

		/// <summary>
		/// Test AreRequiredDatesPopulated for valid Mission Segment dates
		/// </summary>
		[TestMethod]
		public void TestAreRequiredDatesPopulatedMissionSegment()
		{
			EppDelegationDatesHelper edh = new EppDelegationDatesHelper();
			List<string> errors = new List<string>();

			ContractsDto dto = new ContractsDto
			{
				PreviouslySubmittedROM = 12345,
				ContractsCorrespondenceLogNumber = "XYZ123",
				EppDelegationAuthority = (int)EppDelegationAuthority.MissionSegment,
				ScheduledActualMissionSegmentEppDate = DateTime.Now,
				ScheduledActualProgramEppDate = DateTime.Now
			};

			// With the date populated, this should be valid wether or not the LOB is NSS
			bool isValid = edh.AreRequiredDatesPopulated(dto, errors, false);

			Assert.IsTrue(isValid);
			Assert.IsFalse(errors.Any());

			isValid = edh.AreRequiredDatesPopulated(dto, errors, true);

			Assert.IsTrue(isValid);
			Assert.IsFalse(errors.Any());
		}

		/// <summary>
		/// Test AreRequiredDatesPopulated for missing Mission Segment EPP Date
		/// </summary>
		[TestMethod]
		public void TestAreRequiredDatesPopulatedMissionSegmentMissingDate()
		{
			EppDelegationDatesHelper edh = new EppDelegationDatesHelper();
			List<string> errors = new List<string>();

			ContractsDto dto = new ContractsDto
			{
				PreviouslySubmittedROM = 12345,
				ContractsCorrespondenceLogNumber = "XYZ123",
				EppDelegationAuthority = (int)EppDelegationAuthority.MissionSegment,
				ScheduledActualMissionSegmentEppDate = null,
				ScheduledActualProgramEppDate = DateTime.Now
			};

			bool isValid = edh.AreRequiredDatesPopulated(dto, errors, false);

			// LOB is not NSS, so this should be valid
			Assert.IsTrue(isValid);
			Assert.IsFalse(errors.Any());

			isValid = edh.AreRequiredDatesPopulated(dto, errors, true);

			// Now that the LOB is NSS, there should be a validation error
			Assert.IsFalse(isValid);
			Assert.AreEqual(1, errors.Count);
			Assert.AreEqual("Mission Segment EPP Date required.", errors.First());
		}

		/// <summary>
		/// Test AreEnteredDatesSequential for EPP Bid Date
		/// </summary>
		[TestMethod]
		public void TestAreEnteredDatesSequentialBidEppDate()
		{
			EppDelegationDatesHelper edh = new EppDelegationDatesHelper();
			List<string> errors = new List<string>();

			ContractsDto dto = new ContractsDto
			{
				PreviouslySubmittedROM = 12345,
				ContractsCorrespondenceLogNumber = "XYZ123",
				EppDelegationAuthority = (int)EppDelegationAuthority.Space,
				ScheduledActualBidEppDate = DateTime.Now,
				ScheduledActualProgramEppDate = DateTime.Now.AddDays(1)
			};

			bool pass = edh.AreEnteredDatesSequential(dto, errors);

			Assert.IsTrue(pass);
			Assert.IsFalse(errors.Any());

			dto.ScheduledActualBidEppDate = DateTime.Now.AddDays(2);
			pass = edh.AreEnteredDatesSequential(dto, errors);

			Assert.IsFalse(pass);
			Assert.IsTrue(errors.First().Contains("Bid EPP Date must be before Program EPP Date"));

			// Test once more with several blank dates between Bid Epp Date and the next entered date
			dto.ScheduledActualProgramEppDate = null;
			dto.ScheduledActualCorporateEppDate = DateTime.Now;
			errors.Clear();

			pass = edh.AreEnteredDatesSequential(dto, errors);

			Assert.IsFalse(pass);
			Assert.IsTrue(errors.First().Contains("Bid EPP Date must be before Corporate EPP Date"));
		}

		/// <summary>
		/// Test AreEnteredDatesSequential for EPP Bid Date
		/// </summary>
		[TestMethod]
		public void TestAreEnteredDatesSequentialMissionSegmentEppDate()
		{
			EppDelegationDatesHelper edh = new EppDelegationDatesHelper();
			List<string> errors = new List<string>();

			ContractsDto dto = new ContractsDto
			{
				PreviouslySubmittedROM = 12345,
				ContractsCorrespondenceLogNumber = "XYZ123",
				EppDelegationAuthority = (int)EppDelegationAuthority.Space,
				ScheduledActualMissionSegmentEppDate = DateTime.Now,
				ScheduledActualLobEppDate = DateTime.Now.AddDays(1)
			};

			bool pass = edh.AreEnteredDatesSequential(dto, errors);

			Assert.IsTrue(pass);
			Assert.IsFalse(errors.Any());

			dto.ScheduledActualMissionSegmentEppDate = DateTime.Now.AddDays(2);
			pass = edh.AreEnteredDatesSequential(dto, errors);

			Assert.IsFalse(pass);
			Assert.IsTrue(errors.First().Contains("Mission Segment EPP Date must be before Line of Business EPP Date"));
		}

		/// <summary>
		/// Test ContractDataValidForCompleteProposalSave for validation of the proposed and negotiatied insurances
		/// </summary>
		[TestMethod]
		public void ContractDataValidForCompleteProposalSave_IsInsuranceDirectValidation()
		{
			ContractsControllerLogic sut = this.CreateSystem();

			// Initial test - Proposed and Negotiated insurance required when IsInsuranceDirect is Yes
			ContractsDto dto = new ContractsDto
			{
				IsInsuranceDirect = TripleBooleanState.Yes,
			};

			ProposalDto proposalDto = new ProposalDto();
			FullProposal proposal = new FullProposal(proposalDto);
			List<string> messages = new List<string>();

			bool result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsFalse(result);
			Assert.IsTrue(messages.Contains(Constants.INVALID_PROPOSED_INSURANCE));
			Assert.IsTrue(messages.Contains(Constants.INVALID_NEGOTIATED_INSURANCE));

			// Proposed and Negotiated insurance should be "blank" when IsInsuranceDirect is NA
			messages.Clear();
			dto.IsInsuranceDirect = TripleBooleanState.NA;
			dto.ProposedInsurance = 1;
			dto.NegotiatedInsurance = 1;
			result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsFalse(result);
			Assert.IsTrue(messages.Contains(Constants.INVALID_PROPOSED_INSURANCE_BLANK));
			Assert.IsTrue(messages.Contains(Constants.INVALID_NEGOTIATED_INSURANCE_BLANK));

			// Proposed and Negotiated insurance should be "blank" when IsInsuranceDirect is No
			messages.Clear();
			dto.IsInsuranceDirect = TripleBooleanState.No;
			result = sut.ContractDataValidForCompleteProposalSave(dto, proposal, messages);

			Assert.IsFalse(result);
			Assert.IsTrue(messages.Contains(Constants.INVALID_PROPOSED_INSURANCE_BLANK));
			Assert.IsTrue(messages.Contains(Constants.INVALID_NEGOTIATED_INSURANCE_BLANK));
		}

		/// <summary>
		/// Test EppDateValidations Method in the ValidateContractViewModel
		/// </summary>
		[TestMethod]
		public void EppDateValidationsViaValidateContractViewModelTest()
		{
			ICollection<string> validationMessages = new HashSet<string>();

			ContractsControllerLogic sut = this.CreateSystem();
			ProposalDto proposalDto = new ProposalDto();
			FullProposal proposal = new FullProposal(proposalDto);

			// Set arbitrary ID for the Line of Business
			proposal.LineOfBusinessID = 15;

			// Now set the Proposal's LoB to NSS - This will allow our validation to run
			pickListMapper.Setup(x => x.GetSelectListPickList(PickListEnum.LineOfBusiness, null, true, false))
				.Returns(new Collection<SelectListItem>()
				{
					new SelectListItem() {
						Text = Constants.NSS_LOB_NAME,
						Value = proposal.LineOfBusinessID.ToString()
					}
				});

			ContractsModelView contractsModelView = new ContractsModelView();

			contractsModelView.PlannedBidEppDate = DateTime.Now;
			contractsModelView.PlannedMissionSegmentEppDate = DateTime.Now;
			contractsModelView.PlannedLobEppDate = DateTime.Now;
			contractsModelView.PlannedProgramEppDate = DateTime.Now;
			contractsModelView.PlannedPreSpaceEppDate = DateTime.Now;
			contractsModelView.PlannedSpaceEppDate = DateTime.Now;
			contractsModelView.PlannedPreCorporateEppDate = DateTime.Now;
			contractsModelView.PlannedCorporateEppDate = DateTime.Now;

			validationMessages = sut.ValidateContractModelView(contractsModelView, proposal);

			Assert.IsTrue(validationMessages.Count == 8);
			Assert.IsTrue(validationMessages.Contains(Constants.INVALID_SCHEDULED_ACTUAL_BID_EPP_DATE));
			Assert.IsTrue(validationMessages.Contains(Constants.INVALID_SCHEDULED_ACTUAL_MISSION_SEGMENT_EPP_DATE));
			Assert.IsTrue(validationMessages.Contains(Constants.INVALID_SCHEDULED_ACTUAL_LOB_EPP_DATE));
			Assert.IsTrue(validationMessages.Contains(Constants.INVALID_SCHEDULED_ACTUAL_PROGRAM_EPP_DATE));
			Assert.IsTrue(validationMessages.Contains(Constants.INVALID_SCHEDULED_ACTUAL_PRE_SPACE_EPP_DATE));
			Assert.IsTrue(validationMessages.Contains(Constants.INVALID_SCHEDULED_ACTUAL_SPACE_EPP_DATE));
			Assert.IsTrue(validationMessages.Contains(Constants.INVALID_SCHEDULED_ACTUAL_PRE_CORPORATE_EPP_DATE));
			Assert.IsTrue(validationMessages.Contains(Constants.INVALID_SCHEDULED_ACTUAL_CORPORATE_EPP_DATE));

			// Now lets one by one add the planned dates and see that the messages should dwindle to 0
			contractsModelView.ScheduledActualBidEppDate = DateTime.Now;
			validationMessages = sut.ValidateContractModelView(contractsModelView, proposal);
			Assert.IsTrue(validationMessages.Count == 7);

			contractsModelView.ScheduledActualMissionSegmentEppDate = DateTime.Now;
			validationMessages = sut.ValidateContractModelView(contractsModelView, proposal);
			Assert.IsTrue(validationMessages.Count == 6);

			contractsModelView.ScheduledActualLobEppDate = DateTime.Now;
			validationMessages = sut.ValidateContractModelView(contractsModelView, proposal);
			Assert.IsTrue(validationMessages.Count == 5);

			contractsModelView.ScheduledActualProgramEppDate = DateTime.Now;
			validationMessages = sut.ValidateContractModelView(contractsModelView, proposal);
			Assert.IsTrue(validationMessages.Count == 4);

			contractsModelView.ScheduledActualPreSpaceEppDate = DateTime.Now;
			validationMessages = sut.ValidateContractModelView(contractsModelView, proposal);
			Assert.IsTrue(validationMessages.Count == 3);

			contractsModelView.ScheduledActualSpaceEppDate = DateTime.Now;
			validationMessages = sut.ValidateContractModelView(contractsModelView, proposal);
			Assert.IsTrue(validationMessages.Count == 2);

			contractsModelView.ScheduledActualPreCorporateEppDate = DateTime.Now;
			validationMessages = sut.ValidateContractModelView(contractsModelView, proposal);
			Assert.IsTrue(validationMessages.Count == 1);

			contractsModelView.ScheduledActualCorporateEppDate = DateTime.Now;
			validationMessages = sut.ValidateContractModelView(contractsModelView, proposal);
			Assert.IsTrue(validationMessages.Count == 0);

			// Now set NSS LoB to something - This will skip validation of the EPPDateValidations Method
			pickListMapper.Setup(x => x.GetSelectListPickList(PickListEnum.LineOfBusiness, null, true, false))
				.Returns(new Collection<SelectListItem>()
				{
					new SelectListItem() {
						Text = Constants.NSS_LOB_NAME,
						Value = "1"
					}
				});

			// Going to unset all the planned dates and because the LoB is now not NSS, the Scheduled dates that are set, should not matter
			contractsModelView.ScheduledActualBidEppDate = null;
			contractsModelView.ScheduledActualMissionSegmentEppDate = null;
			contractsModelView.ScheduledActualLobEppDate = null;
			contractsModelView.ScheduledActualProgramEppDate = null;
			contractsModelView.ScheduledActualPreSpaceEppDate = null;
			contractsModelView.ScheduledActualSpaceEppDate = null;
			contractsModelView.ScheduledActualPreCorporateEppDate = null;
			contractsModelView.ScheduledActualCorporateEppDate = null;

			// Lets run the validation again
			validationMessages = sut.ValidateContractModelView(contractsModelView, proposal);

			Assert.IsTrue(validationMessages.Count == 0);
		}
	}
}
