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
            this.proposalLogic = new Mock<ProposalControllerLogic>(this.securityAccess.Object, this.proposalLoader.Object, It.IsAny<IValidationMethods>(), this.proposalMediator.Object, this.userMapper.Object,
                this.objectFactory.Object, It.IsAny<IOrgStructureDataMapper>(), It.IsAny<IProposalPermissionMediator>(), It.IsAny<ISecurityInformation>(), It.IsAny<ICacheDataLoader>(),
                It.IsAny<IPickListMapper>(), It.IsAny<IUserLoader>(), this.approvalsLoader.Object, this.proposalChecklistLoader.Object, this.checklistMediator.Object, It.IsAny<IWorkspaceDTODataLoader>(),
                It.IsAny<IPermissionsDTODataLoader>(), this.emailer.Object);
            this.approvalsLogic = new Mock<ApprovalsControllerLogic>(this.securityAccess.Object, this.proposalLoader.Object, this.userMapper.Object, It.IsAny<IUserLoader>(), this.emailer.Object,
                this.objectFactory.Object, this.approvalsLoader.Object, this.proposalMediator.Object, this.proposalChecklistLoader.Object, this.checklistMediator.Object, It.IsAny<ApprovalEmailer>(),
                It.IsAny<IAttachmentLoader>(), It.IsAny<IActiveDirectoryUtilities>());

            ContractsControllerLogic logic;

            logic = new ContractsControllerLogic(this.securityAccess.Object, this.proposalLoader.Object, this.userMapper.Object,
                        this.objectFactory.Object, this.approvalsLoader.Object, this.proposalChecklistLoader.Object, this.checklistMediator.Object,
                        this.proposalMediator.Object, this.contractsLoader.Object, this.cageCodesLoader.Object, this.emailer.Object, this.approvalsLogic.Object);

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
                CustomerSubmittalDate = DateTime.Now,
                ContractsCorrespondenceLogNumber = "Test_Log_Number",
                FinalNegotiatedValue = 3,
                NegotiationsSubmitted = DateTime.Now,
                EppDelegationAuthority = 3,
                ProgramEppDate = DateTime.Now,
                LobEppDate = DateTime.Now,
                PreSpaceEppDate = DateTime.Now,
                SpaceEppDate = DateTime.Now,
                PreCorporateEppDate = DateTime.Now,
                CorporateEppDate = DateTime.Now,
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
            Assert.AreEqual(contractDto.CustomerSubmittalDate, contractsModelView.CustomerSubmittalDt);
            Assert.AreEqual(contractDto.ContractsCorrespondenceLogNumber, contractsModelView.ContractsCorrespondenceLogNumber);
            Assert.AreEqual(contractDto.FinalNegotiatedValue, contractsModelView.FinalNegotiatedValueLong);
            Assert.AreEqual(contractDto.NegotiationsSubmitted, contractsModelView.NegotiationsSubmittedDt);
            Assert.AreEqual(contractDto.PreviouslySubmittedROM.ToString(), contractsModelView.PreviouslySubmittedRoms.ElementAtOrDefault(0).Value);
            Assert.AreEqual(contractDto.PreviouslySubmittedROM, contractsModelView.PreviouslySubmittedROM);
            Assert.AreEqual(contractDto.EppDelegationAuthority, (int)contractsModelView.EppDelegationAuthority);
            Assert.AreEqual(contractDto.ProgramEppDate, contractsModelView.ProgramEppDate);
            Assert.AreEqual(contractDto.LobEppDate, contractsModelView.LobEppDate);
            Assert.AreEqual(contractDto.PreSpaceEppDate, contractsModelView.PreSpaceEppDate);
            Assert.AreEqual(contractDto.SpaceEppDate, contractsModelView.SpaceEppDate);
            Assert.AreEqual(contractDto.PreCorporateEppDate, contractsModelView.PreCorporateEppDate);
            Assert.AreEqual(contractDto.CorporateEppDate, contractsModelView.CorporateEppDate);
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

            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(TestProposalHelper.GetPermissionsForMocks());
            this.retriever.Setup(x => x.GetCurrentUser()).Returns(TestProposalHelper.GetLeadContractsUserForMocks());
            this.proposalLoader.Setup(x => x.GetById(1)).Returns(TestProposalHelper.GetProposalDtoForMocks(1));
            this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<FullProposal>()));
            this.proposalLogic.Setup(x => x.GetDataForProposalApprovals(fp.Id, false)).Returns(new ProposalApprovalsModelView()); // intercept and don't return data
            this.proposalLogic.Setup(x => x.GetDataForProposalUserInformation(fp.Id)).Returns(new ProposalUserInformationModelView());
            this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(fp.CurrentUser);
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

            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(TestProposalHelper.GetPermissionsForMocks());
            this.retriever.Setup(x => x.GetCurrentUser()).Returns(TestProposalHelper.GetLeadEstimatorUserForMocks());
            this.proposalLoader.Setup(x => x.GetById(1)).Returns(TestProposalHelper.GetProposalDtoForMocks(1));
            this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<FullProposal>()));
            this.proposalLogic.Setup(x => x.GetDataForProposalApprovals(fp.Id, false)).Returns(new ProposalApprovalsModelView()); // intercept and don't return data
            this.proposalLogic.Setup(x => x.GetDataForProposalUserInformation(fp.Id)).Returns(new ProposalUserInformationModelView());
            this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(fp.CurrentUser);
            this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(fp);

            await sut.SetProposalLost(fp.Id, errors);

            Assert.AreEqual(errors.First(), "Insufficient permissions to set proposal as Lost.");
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

            this.retriever.Setup(x => x.GetProposalPermissions(It.IsAny<int>())).Returns(TestProposalHelper.GetPermissionsForMocks());
            this.retriever.Setup(x => x.GetCurrentUser()).Returns(TestProposalHelper.GetLeadContractsUserForMocks());
            this.proposalLoader.Setup(x => x.GetById(1)).Returns(TestProposalHelper.GetProposalDtoForMocks(1));
            this.proposalMediator.Setup(x => x.SaveProposal(It.IsAny<FullProposal>()));
            this.proposalLogic.Setup(x => x.GetDataForProposalApprovals(fp.Id, false)).Returns(new ProposalApprovalsModelView()); // intercept and don't return data
            this.proposalLogic.Setup(x => x.GetDataForProposalUserInformation(fp.Id)).Returns(new ProposalUserInformationModelView());
            this.userMapper.Setup(x => x.GetByNtid(It.IsAny<string>())).Returns(fp.CurrentUser);
            this.objectFactory.Setup(x => x.CreateFullProposal(It.IsAny<ProposalDto>())).Returns(fp);

            await sut.SetProposalLost(fp.Id, errors);

            Assert.AreEqual(errors.First(), "The proposal status must be in 'Pending Certification' or 'Pending Contractual Award' in order to set it to 'Proposal Lost'");
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
            this.proposalLogic.Setup(x => x.GetDataForProposalUserInformation(fp.Id)).Returns(new ProposalUserInformationModelView());
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
            this.proposalLogic.Setup(x => x.GetDataForProposalUserInformation(fp.Id)).Returns(new ProposalUserInformationModelView());
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

            Assert.AreEqual("ProgramEppDate", dates.First());
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

            Assert.AreEqual("CorporateEppDate", dates.First());
            Assert.AreEqual("PreCorporateEppDate", dates.ElementAt(1));
            Assert.AreEqual("SpaceEppDate", dates.ElementAt(2));
            Assert.AreEqual("PreSpaceEppDate", dates.ElementAt(3));
            Assert.AreEqual("LobEppDate", dates.ElementAt(4));
            Assert.AreEqual("ProgramEppDate", dates.Last());
            Assert.AreEqual(6, dates.Count);
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
                SpaceEppDate = DateTime.Now,
                PreSpaceEppDate = DateTime.Now,
                ProgramEppDate = DateTime.Now,
                LobEppDate = DateTime.Now
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
                SpaceEppDate = DateTime.Now,
                PreSpaceEppDate = DateTime.Now,
                ProgramEppDate = DateTime.Now
            };

            bool isValid = edh.AreRequiredDatesPopulated(dto, errors);

            Assert.IsFalse(isValid);
            Assert.AreEqual("Line of Business EPP Date required.", errors.First());
        }

        /// <summary>
        /// Ensures that dates that are equal, or higher, by authority level pass
        /// </summary>
        [TestMethod]
        public void EppDatesForSpaceDelegationValidSequenceTest()
        {
            EppDelegationDatesHelper edh = new EppDelegationDatesHelper();
            List<string> errors = new List<string>();

            ContractsDto dto = new ContractsDto
            {
                PreviouslySubmittedROM = 12345,
                ContractsCorrespondenceLogNumber = "XYZ123",
                EppDelegationAuthority = (int)EppDelegationAuthority.Space,
                SpaceEppDate = new DateTime(2022, 03, 30),
                PreSpaceEppDate = new DateTime(2022, 03, 30),
                ProgramEppDate = new DateTime(2022, 03, 30),
                LobEppDate = new DateTime(2022, 03, 30)
            };

            bool pass = edh.AreRequiredDatesSequential(dto, errors);

            Assert.IsTrue(pass);
        }

        /// <summary>
        /// Ensures that dates that are not equal, or higher, by authority level fails
        /// </summary>
        [TestMethod]
        public void EppDatesForSpaceDelegationInvalidSequenceTest()
        {
            EppDelegationDatesHelper edh = new EppDelegationDatesHelper();
            List<string> errors = new List<string>();

            ContractsDto dto = new ContractsDto
            {
                PreviouslySubmittedROM = 12345,
                ContractsCorrespondenceLogNumber = "XYZ123",
                EppDelegationAuthority = (int)EppDelegationAuthority.Space,
                SpaceEppDate = new DateTime(2022, 03, 30),
                PreSpaceEppDate = new DateTime(2022, 03, 29),
                ProgramEppDate = new DateTime(2022, 03, 30),
                LobEppDate = new DateTime(2022, 03, 30)
            };

            bool pass = edh.AreRequiredDatesSequential(dto, errors);

            Assert.IsFalse(pass);
            Assert.IsTrue(errors.First().Contains("Line of Business EPP Date must be before Pre-Space EPP Date"));
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
                PreCorporateEppDate = new DateTime(2022, 04, 06),
                SpaceEppDate = new DateTime(2022, 04, 8),
                PreSpaceEppDate = new DateTime(2022, 04, 07),
                LobEppDate = new DateTime(2022, 04, 06),
                ProgramEppDate = new DateTime(2022, 04, 05),
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
                PreviouslySubmittedROM = 12345,
                ContractsCorrespondenceLogNumber = "XYZ123",
                EppDelegationAuthority = (int)EppDelegationAuthority.Space,
                SpaceEppDate = DateTime.Now,
                PreSpaceEppDate = DateTime.Now,
                ProgramEppDate = DateTime.Now,
                LobEppDate = DateTime.Now
            };

            bool isValid = sut.ContractDataValidForCompleteProposalSave(dto, messages);

            Assert.IsFalse(isValid);
            Assert.AreEqual(4, messages.Count);
            Assert.IsTrue(messages.Contains(Constants.INVALID_MOD_COMPLETION_DATE));
            Assert.IsTrue(messages.Contains(Constants.INVALID_LM_WIN_LOSS));
            Assert.IsTrue(messages.Contains(Constants.INVALID_NEGOTIATIONS_SUBMITTED_DATE));
            Assert.IsTrue(messages.Contains(Constants.INVALID_FINAL_NEGOTIATED_VALUE));
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
    }
}
