// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Web.Mvc;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView.Reports;
    using GenTRAC.ActionLogic.Validation;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.DataBridge.DTO.Reports;
    using GenTRAC.Objects;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.PickList;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test the Reports Controller Logic class
    /// </summary>
    [TestClass]
    public class ReportsControllerLogicTest
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
        /// AD Utils
        /// </summary>
        private Mock<IES.Common.IActiveDirectoryUtilities> adUtils = null;

        /// <summary>
        /// The user mapper
        /// </summary>
        private Mock<IUserMapper> userMapper = null;

        /// <summary>
        /// Object Factory
        /// </summary>
        private Mock<IFullObjectFactory> objectFactory = null;

        /// <summary>
        /// org structure data mapper
        /// </summary>
        private Mock<IOrgStructureDataMapper> orgStructureDataMapper = null;

        /// <summary>
        /// reports loader
        /// </summary>
        private Mock<IReportsLoader> reportsLoader = null;

        /// <summary>
        /// Approvals Loader
        /// </summary>
        private Mock<IApprovalsLoader> approvalsLoader;

        /// <summary>
        /// Proposal Checklist Loader
        /// </summary>
        private Mock<IProposalChecklistLoader> proposalChecklistLoader;

        /// <summary>
        /// Checklist Mediator
        /// </summary>
        private Mock<IChecklistMediator> checklistMediator;

        /// <summary>
        /// Proposal Mediator
        /// </summary>
        private Mock<IProposalMediator> proposalMediator;

        #endregion Properties

        /// <summary>
        /// Reports Controller logic under test
        /// </summary>
        /// <returns>home controller logic</returns>
        private ReportsControllerLogic CreateSystem()
        {
            this.securityAccess = new Mock<ISecurityAccess>();
            this.proposalLoader = new Mock<IProposalLoader>();
            this.adUtils = new Mock<IES.Common.IActiveDirectoryUtilities>();
            this.userMapper = new Mock<IUserMapper>();
            this.objectFactory = new Mock<IFullObjectFactory>();
            this.orgStructureDataMapper = new Mock<IOrgStructureDataMapper>();
            this.reportsLoader = new Mock<IReportsLoader>();
            this.approvalsLoader = new Mock<IApprovalsLoader>();
            this.proposalChecklistLoader = new Mock<IProposalChecklistLoader>();
            this.checklistMediator = new Mock<IChecklistMediator>();
            this.proposalMediator = new Mock<IProposalMediator>();

            Mock<IES.Common.SecurityInformation> security = new Mock<IES.Common.SecurityInformation>(this.adUtils.Object);
            IES.Common.UserData user = new IES.Common.UserData()
            {
                Email = "james.basilio@lmco.com"
            };
            security.Setup(x => x.ActiveUserData).Returns(user);

            return new ReportsControllerLogic(this.securityAccess.Object, this.proposalLoader.Object,
                 this.userMapper.Object, this.objectFactory.Object, this.reportsLoader.Object, this.orgStructureDataMapper.Object, 
                 this.adUtils.Object, this.approvalsLoader.Object, this.proposalChecklistLoader.Object, this.checklistMediator.Object, this.proposalMediator.Object);
        }

        /// <summary>
        /// Get data for proposal log report
        /// </summary>
        [TestMethod]
        public void C_GetDataForProposalLogReport()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            PickListDto lineOfBusiness = new PickListDto()
            {
                Text = "myLineOfBusiness",
                Id = 2,
                IsActive = true
            };

            PickListDto programArea = new PickListDto()
            {
                Text = "myProgramArea",
                Id = 1,
                ParentIds = new int[] { lineOfBusiness.Id },
                IsActive = true
            };

            // Pricer
            UserDTO userPricer1 = new UserDTO
            {
                Id = 1,
                FirstName = "Reed",
                LastName = "Derr",
                DisplayName = "Reed Derr",
                Ntid = "reader",
                UserType = IES.Common.UserType.User
            };

            UserDTO userPricer2 = new UserDTO
            {
                Id = 2,
                FirstName = "Bumpkin",
                LastName = "Pumpkin",
                DisplayName = "Pumpkin Bumpkin",
                Ntid = "bpumkin",
                UserType = IES.Common.UserType.User
            };

            UserDTO userPricer3 = new UserDTO
            {
                Id = 3,
                FirstName = "Orange",
                LastName = "Dew",
                DisplayName = "Dew, Orange",
                Ntid = "oranged",
                UserType = IES.Common.UserType.User
            };

            UserDTO userPricer4 = new UserDTO
            {
                Id = 4,
                FirstName = "Blah",
                LastName = "Friday",
                DisplayName = "Friday, Blah",
                Ntid = "blah",
                UserType = IES.Common.UserType.User
            };
            IES.Common.GroupData redShirters = new IES.Common.GroupData { DisplayName = "redshirts.isgs.lmco.com", Ntid = "redshirts.isgs.lmco.com" };

            this.orgStructureDataMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new List<PickListDto>() { lineOfBusiness });
            this.orgStructureDataMapper.Setup(x => x.GetAllProgramAreas()).Returns(new List<PickListDto>() { programArea });
            this.reportsLoader.Setup(x => x.GetPricerUserIds()).Returns(new Collection<int> { 1, 2, 3, 4 });
            this.reportsLoader.Setup(x => x.GetProposalYears()).Returns(new Collection<int> { 2012, 1013, 2027 });
            this.adUtils.Setup(x => x.GetGroupsForUser(userPricer1.Ntid)).Returns(new List<IES.Common.GroupData> { redShirters });
            this.userMapper.Setup(x => x.GetActiveUser()).Returns(userPricer1);
            this.userMapper.Setup(x => x.GetAllGroups()).Returns(new Collection<UserDTO> { userPricer1 });
            this.userMapper.Setup(x => x.GetUserDtosByUserIds(It.IsAny<ICollection<int>>())).Returns(new Collection<UserDTO> { userPricer1, userPricer2, userPricer3, userPricer4 });

            // return userPricer1 and userPricer2 the first time, userPricer3 and userPricer4 on callback (2nd time)
            Collection<UserDTO> toReturn = new Collection<UserDTO> { userPricer1, userPricer2 };
            Collection<UserDTO> toReturn2 = new Collection<UserDTO> { userPricer3, userPricer4 };
            this.userMapper.Setup(x => x.GetUserDtosByUserIds(new Collection<int> { 1, 2 })).Returns(toReturn);
            this.userMapper.Setup(x => x.GetUserDtosByUserIds(new Collection<int> { 3, 4 })).Returns(toReturn2);

            ProposalLogReportModelView proposalLogMv = sut.GetDataForProposalLogReport();

            Assert.IsTrue(proposalLogMv.LOBList.Any());
            Assert.IsTrue(proposalLogMv.ProposalLogStatusList.Any());
            Assert.IsTrue(proposalLogMv.LeadEstimatorList.Any());
            Assert.IsTrue(proposalLogMv.YearsList.Any());

            var programAreas = from p in proposalLogMv.LOBList
                       where p.Text.Contains(lineOfBusiness.Text)
                       select p;
            Assert.IsTrue(programAreas.Any());
            Assert.IsTrue(proposalLogMv.ExecutionUserIds.Contains(userPricer1.Id.ToString()));
        }

        /// <summary>
        /// Get line of business and program Area list
        /// </summary>
        [TestMethod]
        public void C_GetProgramAreasList()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            PickListDto programArea = new PickListDto()
            {
                Text = "myL",
                Id = 1,
                ParentIds = new int[] { 2 },
                IsActive = true
            };

            this.orgStructureDataMapper.Setup(x => x.GetAllProgramAreas()).Returns(new List<PickListDto>() { programArea });

            Collection<SelectListItem> list = sut.GetProgramAreasList();

            var programAreas = from p in list
                               where p.Text.Contains(programArea.Text)
                       select p;
            Assert.IsTrue(programAreas.Any());
        }

        /// <summary>
        /// Get Line of Business list
        /// </summary>
        [TestMethod]
        public void C_GetLOBList()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            PickListDto lob = new PickListDto()
            {
                Text = "testLOB",
                Id = 1,
                IsActive = true
            };

            this.orgStructureDataMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new List<PickListDto>() { lob });

            Collection<SelectListItem> list = sut.GetLOBList();

            var lobs = from l in list
                       where l.Text.Contains(lob.Text)
                       select l;

            Assert.IsTrue(lobs.Any());
        }

        /// <summary>
        /// validate proposal log input parameters
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalLogParameters()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            // test validation relating to Specific Proposals radio button selection
            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            ProposalLogReportModelView mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.SpecificProposalRadioFilter
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.SPECIFIC_PROPOSAL_SELECTION_REQUIRED)).Any());

            // as long as one specific filter is selected, we won't get the validation message
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.SpecificProposalRadioFilter,
                Years = new Collection<string> { "2012", "2013" }
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsFalse(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.SPECIFIC_PROPOSAL_SELECTION_REQUIRED)).Any());

            // test validation releating to Submit Date Range radio button selection
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.SubmitDateRangeFilter,
                SubmitStartDate = null,
                SubmitEndDate = null
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            // test that if both are null,  validation message is displayed
            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.SUBMIT_DATE_REQUIRED)).Any());

            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.SubmitDateRangeFilter,
                SubmitStartDate = "07/12/2013",
                SubmitEndDate = null
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            // assert validation message is displayed if one of the two dates is null
            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.SUBMIT_DATE_REQUIRED)).Any());

            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.SubmitDateRangeFilter,
                SubmitStartDate = "abc",
                SubmitEndDate = "08122013"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.SUBMIT_START_NOT_VALID_DATE_FORMAT)).Any());
            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.SUBMIT_END_NOT_VALID_DATE_FORMAT)).Any());

            // test the start date must be before the end date
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.SubmitDateRangeFilter,
                SubmitStartDate = "08/05/2013",
                SubmitEndDate = "06/24/2013"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.SUBMIT_START_DATE_RANGE)).Any());

            // same start and end date is valid
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.SubmitDateRangeFilter,
                SubmitStartDate = "08/05/2013",
                SubmitEndDate = "08/05/2013"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsFalse(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.SUBMIT_START_DATE_RANGE)).Any());

            // test when dates are valid
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.SubmitDateRangeFilter,
                SubmitStartDate = "07/12/2013",
                SubmitEndDate = "8/20/2013"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsFalse(validationMessages.Any());

            // test when tracking # is selected but the user didn't type anything
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.TrackingNumberFilter,
                TrackingNumber = string.Empty
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_REQUIRED)).Any());
        }

        /// <summary>
        /// populate the proposal log SSRS parameters. This will test All Proposals and Specific Proposals selection
        /// </summary>
        [TestMethod]
        public void C_PopulateProposalLogSSRSParameters_AllProposals_And_SpecificProposals()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            ProposalLogReportModelView mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.All, (int)ProposalReportStatus.Archived, (int)ProposalReportStatus.Deleted, (int)ProposalReportStatus.Revision },
                ProposalLogFilterOption = ProposalLogFilterOption.AllProposalsFilter,
                ExecutionUserIds = "1"
            };

            Uri url = sut.PopulateProposalLogSSRSParameters(mv);

            // the name of this report
            Assert.IsTrue(url.AbsoluteUri.Contains("Proposal%20Log%20Report"));

            // if All was selected as one of the proposal status options, the rest are filtered out of the url and just All is used
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=All"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=True"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.EXECUTION_USER_ID + "=1"));
            
            // Now select specific proposals with some of the options picked
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.SpecificProposalRadioFilter,
                Years = new Collection<string> { "2012", "2014" },
                CentralEstimators = new Collection<int> { 1, 233, 44 }
            };

            this.reportsLoader.Setup(x => x.GetProposalYears()).Returns(new Collection<int>() { 2011, 2012, 2013, 2014 });
            this.reportsLoader.Setup(x => x.GetPricerUserIds()).Returns(new Collection<int> { 1, 233, 44 });
            this.userMapper.Setup(x => x.GetUserDtosByUserIds(It.IsAny<ICollection<int>>())).Returns(new Collection<UserDTO> { new UserDTO(), new UserDTO(), new UserDTO(), new UserDTO() });

            url = sut.PopulateProposalLogSSRSParameters(mv);

            // if Active was selected as a proposal log status, this is converted to the proposal status of In Progess and Completed. any other are mapped to proposal log
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=" + ((int)ProposalReportStatus.Archived).ToString() + "," + ((int)ProposalStatus.InProgress).ToString() + "," + ((int)ProposalStatus.Completed).ToString()));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=False"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.YEAR + "=2012,2014"));
            Assert.IsFalse(url.AbsoluteUri.Contains(Constants.Report.LINE_OF_BUSINESS)); // Line Of Business was not selected so it's not a parmeter
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.CENTRAL_ESTIMATOR + "=1,233,44"));

            // Now select specific proposals with all options selected
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.SpecificProposalRadioFilter,
                Years = new Collection<string> { "2012", "2014" },
                CentralEstimators = new Collection<int> { 1, 233, 44, 66 },
                LOBIds = new Collection<int> { 667, 1234, 2344 }
            };

            this.reportsLoader.Setup(x => x.GetProposalYears()).Returns(new Collection<int>() { 2011, 2012, 2013, 2014 });
            this.reportsLoader.Setup(x => x.GetPricerUserIds()).Returns(new Collection<int> { 1, 233, 44, 66 });
            this.userMapper.Setup(x => x.GetUserDtosByUserIds(It.IsAny<ICollection<int>>())).Returns(new Collection<UserDTO> { new UserDTO(), new UserDTO(), new UserDTO(), new UserDTO() });            
            this.orgStructureDataMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new Collection<PickListDto> { new PickListDto { Id = 667 }, new PickListDto { Id = 1234 }, new PickListDto { Id = 2344 }, new PickListDto { Id = 5667 } });

            url = sut.PopulateProposalLogSSRSParameters(mv);

            // if Active was selected as a proposal log status, this is converted to the proposal status of In Progess and Completed. any other are mapped to proposal log
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=" + ((int)ProposalReportStatus.Archived).ToString() + "," + ((int)ProposalStatus.InProgress).ToString() + "," + ((int)ProposalStatus.Completed).ToString()));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=False"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.YEAR + "=2012,2014"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.LINE_OF_BUSINESS + "=667,1234,2344"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.CENTRAL_ESTIMATOR + "=All"));
        }

        /// <summary>
        /// populate the proposal log SSRS parameters. This will test Specific Proposals selection that will be converted to All
        /// </summary>
        [TestMethod]
        public void C_PopulateProposalLogSSRSParameters_SpecificProposalsConvertedToAll()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            // select specific proposals with some of the options picked
            ProposalLogReportModelView mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.SpecificProposalRadioFilter,
                Years = new Collection<string> { "2012", "2014" },
                CentralEstimators = new Collection<int>()
            };

            this.reportsLoader.Setup(x => x.GetProposalYears()).Returns(new Collection<int>() { 2012, 2014 });
            this.userMapper.Setup(x => x.GetUserDtosByUserIds(It.IsAny<ICollection<int>>())).Returns(new Collection<UserDTO> { new UserDTO(), new UserDTO(), new UserDTO(), new UserDTO() });

            Uri url = sut.PopulateProposalLogSSRSParameters(mv);

            // if Active was selected as a proposal log status, this is converted to the proposal status of In Progess and Completed. any other are mapped to proposal log
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=" + ((int)ProposalReportStatus.Archived).ToString() + "," + ((int)ProposalStatus.InProgress).ToString() + "," + ((int)ProposalStatus.Completed).ToString()));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=False"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.YEAR + "=All"));
            Assert.IsFalse(url.AbsoluteUri.Contains(Constants.Report.LINE_OF_BUSINESS)); // Line Of Business was not selected so it's not a parmeter
            
            // Now select specific proposals with all options selected
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.SpecificProposalRadioFilter,
                Years = new Collection<string> { "2012", "2014" },
                LOBIds = new Collection<int> { 667, 1234, 2344 },
                CentralEstimators = new Collection<int>()
            };

            this.reportsLoader.Setup(x => x.GetProposalYears()).Returns(new Collection<int>() { 2012, 2014 });
            this.orgStructureDataMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new Collection<PickListDto> { new PickListDto { Id = 667}, new PickListDto { Id = 1234 }, new PickListDto { Id = 2344 } });

            url = sut.PopulateProposalLogSSRSParameters(mv);

            // if Active was selected as a proposal log status, this is converted to the proposal status of In Progess and Completed. any other are mapped to proposal log
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=" + ((int)ProposalReportStatus.Archived).ToString() + "," + ((int)ProposalStatus.InProgress).ToString() + "," + ((int)ProposalStatus.Completed).ToString()));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=False"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.YEAR + "=All"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.LINE_OF_BUSINESS + "=All"));
        }

        /// <summary>
        /// populate the proposal log SSRS parameters. This will test Submit Date Range selection
        /// </summary>
        [TestMethod]
        public void C_PopulateProposalLogSSRSParameters_SubmitDateRange()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            ProposalLogReportModelView mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.All, (int)ProposalReportStatus.Archived, (int)ProposalReportStatus.Deleted, (int)ProposalReportStatus.Revision },
                ProposalLogFilterOption = ProposalLogFilterOption.SubmitDateRangeFilter,
                SubmitStartDate = "02/02/2012",
                SubmitEndDate = "12/24/2019",
                ExecutionUserIds = "1"
            };

            Uri url = sut.PopulateProposalLogSSRSParameters(mv);

            // the name of this report
            Assert.IsTrue(url.AbsoluteUri.Contains("Proposal%20Log%20Report"));

            // if All was selected as one of the proposal status options, the rest are filtered out of the url and just All is used
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=All"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=False"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.EXECUTION_USER_ID + "=1"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.SUBMIT_START_DATE + "=" + "02/02/2012"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.SUBMIT_END_DATE + "=" + "12/24/2019"));
        }

        /// <summary>
        /// populate the proposal log SSRS parameters. This will test Tracking Number selection
        /// </summary>
        [TestMethod]
        public void C_PopulateProposalLogSSRSParameters_TrackingNumber()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            ProposalLogReportModelView mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.All, (int)ProposalReportStatus.Archived, (int)ProposalReportStatus.Deleted, (int)ProposalReportStatus.Revision },
                ProposalLogFilterOption = ProposalLogFilterOption.TrackingNumberFilter,
                TrackingNumber = "2013",
                ExecutionUserIds = "1"
            };

            Uri url = sut.PopulateProposalLogSSRSParameters(mv);

            // the name of this report
            Assert.IsTrue(url.AbsoluteUri.Contains("Proposal%20Log%20Report"));

            // if All was selected as one of the proposal status options, the rest are filtered out of the url and just All is used
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=All"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=False"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.EXECUTION_USER_ID + "=1"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.TRACKING_NUMBER + "=2013"));
        }

        /// <summary>
        /// populate the proposal log SSRS parameters. This will test Include Org Mapping selection
        /// </summary>
        [TestMethod]
        public void C_PopulateProposalLogSSRSParameters_IncludeOrgMapping()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            ProposalLogReportModelView mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.All, (int)ProposalReportStatus.Archived, (int)ProposalReportStatus.Deleted, (int)ProposalReportStatus.Revision },
                ProposalLogFilterOption = ProposalLogFilterOption.TrackingNumberFilter,
                TrackingNumber = "2013",
                ExecutionUserIds = "1"
            };

            Uri url = sut.PopulateProposalLogSSRSParameters(mv);

            // the name of this report
            Assert.IsTrue(url.AbsoluteUri.Contains("Proposal%20Log%20Report"));

            // if All was selected as one of the proposal status options, the rest are filtered out of the url and just All is used
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=All"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=False"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.EXECUTION_USER_ID + "=1"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.TRACKING_NUMBER + "=2013"));
        }

        /// <summary>
        /// validate proposal log input parameters's tracking number regular expression
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalLogParameters_TRACKING_NUMBER_REGEX()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            // test that specical characters are not allowed in the tracking number 
            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            ProposalLogReportModelView mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.TrackingNumberFilter,
                TrackingNumber = "&"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_FORMAT)).Any());

            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.TrackingNumberFilter,
                TrackingNumber = "99#"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_FORMAT)).Any());

            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.TrackingNumberFilter,
                TrackingNumber = "2013&23"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_FORMAT)).Any());

            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.TrackingNumberFilter,
                TrackingNumber = "2013 23"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_FORMAT)).Any());

            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.TrackingNumberFilter,
                TrackingNumber = "2013*"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_FORMAT)).Any());

            // now ensure some valid tracking numbers
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.TrackingNumberFilter,
                TrackingNumber = "2013"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsFalse(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_FORMAT)).Any());

            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.TrackingNumberFilter,
                TrackingNumber = "2013-"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsFalse(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_FORMAT)).Any());

            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.TrackingNumberFilter,
                TrackingNumber = "rev"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsFalse(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_FORMAT)).Any());

            validationMessages = new List<ValidationMessage>();
            mv = new ProposalLogReportModelView
            {
                ProposalLogStatus = new Collection<int> { (int)ProposalReportStatus.Active, (int)ProposalReportStatus.Archived },
                ProposalLogFilterOption = ProposalLogFilterOption.TrackingNumberFilter,
                TrackingNumber = "2013-R0"
            };

            sut.ValidateProposalLogParameters(mv, validationMessages);

            Assert.IsFalse(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_FORMAT)).Any());
        }

        /// <summary>
        /// Get data for proposal activity report
        /// </summary>
        [TestMethod]
        public void C_GetDataForProposalActivityReport()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            PickListDto lineOfBusiness = new PickListDto()
            {
                Text = "myLineOfBusiness",
                Id = 2,
                IsActive = true
            };

            PickListDto programArea = new PickListDto()
            {
                Text = "myProgramArea",
                Id = 1,
                ParentIds = new int[] { lineOfBusiness.Id },
                IsActive = true
            };

            // Pricer
            UserDTO userPricer1 = new UserDTO
            {
                Id = 1,
                FirstName = "Reed",
                LastName = "Derr",
                DisplayName = "Reed Derr",
                Ntid = "reader",
                UserType = IES.Common.UserType.User
            };

            UserDTO userPricer2 = new UserDTO
            {
                Id = 2,
                FirstName = "Bumpkin",
                LastName = "Pumpkin",
                DisplayName = "Pumpkin Bumpkin",
                Ntid = "bpumkin",
                UserType = IES.Common.UserType.User
            };

            UserDTO userPricer3 = new UserDTO
            {
                Id = 3,
                FirstName = "Orange",
                LastName = "Dew",
                DisplayName = "Dew, Orange",
                Ntid = "oranged",
                UserType = IES.Common.UserType.User
            };

            UserDTO userPricer4 = new UserDTO
            {
                Id = 4,
                FirstName = "Blah",
                LastName = "Friday",
                DisplayName = "Friday, Blah",
                Ntid = "blah",
                UserType = IES.Common.UserType.User
            };
            IES.Common.GroupData redShirters = new IES.Common.GroupData { DisplayName = "redshirts.isgs.lmco.com", Ntid = "redshirts.isgs.lmco.com" };

            this.orgStructureDataMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new List<PickListDto>() { lineOfBusiness });
            this.orgStructureDataMapper.Setup(x => x.GetAllProgramAreas()).Returns(new List<PickListDto>() { programArea });
            this.adUtils.Setup(x => x.GetGroupsForUser(userPricer1.Ntid)).Returns(new List<IES.Common.GroupData> { redShirters });
            this.userMapper.Setup(x => x.GetActiveUser()).Returns(userPricer1);
            this.userMapper.Setup(x => x.GetAllGroups()).Returns(new Collection<UserDTO> { userPricer1 });

            // return userPricer1 and userPricer2 the first time, userPricer3 and userPricer4 on callback (2nd time)
            Collection<UserDTO> toReturn = new Collection<UserDTO> { userPricer1, userPricer2 };
            Collection<UserDTO> toReturn2 = new Collection<UserDTO> { userPricer3, userPricer4 };
            this.userMapper.Setup(x => x.GetUserDtosByUserIds(new Collection<int> { 1, 2 })).Returns(toReturn);
            this.userMapper.Setup(x => x.GetUserDtosByUserIds(new Collection<int> { 3, 4 })).Returns(toReturn2);

            ProposalActivityReportModelView proposalLogMv = sut.GetDataForProposalActivityReport();

            Assert.IsTrue(proposalLogMv.ProgramAreaList.Any());
            Assert.IsTrue(proposalLogMv.CustomerTypeList.Any());

            var programAreas = from p in proposalLogMv.ProgramAreaList
                       where p.Text.Contains(programArea.Text)
                       select p;
            Assert.IsTrue(programAreas.Any());
            Assert.IsTrue(proposalLogMv.ExecutionUserIds.Contains(userPricer1.Id.ToString()));
        }

        /// <summary>
        /// validate proposal activity input parameters
        /// </summary>
        [TestMethod]
        public void C_ValidateProposalActivityParameters()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            // test validation relating to Specific Customer Types radio button selection
            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            ProposalActivityReportModelView mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.Active,
                ProposalActivityFilterOption = ProposalActivityFilterOption.SpecificCustomerTypesFilter
            };

            sut.ValidateProposalActivityParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalActivityReportValidationConstants.SPECIFIC_CUSTOMER_TYPE_PROGRAM_AREA_SELECTION_REQUIRED)).Any());

            // as long as one specific filter is selected, we won't get the validation message
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.Active,
                ProposalActivityFilterOption = ProposalActivityFilterOption.AllCustomerTypesFilter,
                CreateStartDate = "abc",
                CreateEndDate = "08122013"
            };

            sut.ValidateProposalActivityParameters(mv, validationMessages);

            // assert validation message is displayed if the dates aren't in correct date time format
            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalActivityReportValidationConstants.CREATE_END_NOT_VALID_DATE_FORMAT)).Any());
            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalActivityReportValidationConstants.CREATE_START_NOT_VALID_DATE_FORMAT)).Any());

            // test the start date must be before the end date
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.Active,
                ProposalActivityFilterOption = ProposalActivityFilterOption.AllCustomerTypesFilter,
                CreateStartDate = "08/05/2013",
                CreateEndDate = "06/24/2013"
            };

            sut.ValidateProposalActivityParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalActivityReportValidationConstants.CREATE_START_DATE_RANGE)).Any());

            // same start and end date is valid
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.Active,
                ProposalActivityFilterOption = ProposalActivityFilterOption.AllCustomerTypesFilter,
                CreateStartDate = "08/05/2013",
                CreateEndDate = "08/05/2013"
            };

            sut.ValidateProposalActivityParameters(mv, validationMessages);

            Assert.IsFalse(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalActivityReportValidationConstants.CREATE_START_DATE_RANGE)).Any());

            // test when tracking # is selected but the user didn't type anything
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.Active,
                ProposalActivityFilterOption = ProposalActivityFilterOption.AllCustomerTypesFilter,
                TrackingNumber = "99#"
            };

            sut.ValidateProposalActivityParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_FORMAT)).Any());

            // test incomplete data range, missing end date
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.Active,
                ProposalActivityFilterOption = ProposalActivityFilterOption.AllCustomerTypesFilter,
                CreateStartDate = "08/05/2013",
                CreateEndDate = null
            };

            sut.ValidateProposalActivityParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalActivityReportValidationConstants.INCOMPLETE_CREATE_DATE_RANGE)).Any());

            // test incomplete data range, missing start date
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.Active,
                ProposalActivityFilterOption = ProposalActivityFilterOption.AllCustomerTypesFilter,
                CreateStartDate = null,
                CreateEndDate = "08/05/2013"
            };

            sut.ValidateProposalActivityParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalActivityReportValidationConstants.INCOMPLETE_CREATE_DATE_RANGE)).Any());

            // test specific customer types but no programArea or customers type selected
            validationMessages = new List<ValidationMessage>();
            mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.Active,
                ProposalActivityFilterOption = ProposalActivityFilterOption.SpecificCustomerTypesFilter
            };

            sut.ValidateProposalActivityParameters(mv, validationMessages);

            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.ProposalActivityReportValidationConstants.SPECIFIC_CUSTOMER_TYPE_PROGRAM_AREA_SELECTION_REQUIRED)).Any());
        }

        /// <summary>
        /// populate the proposal activity SSRS parameters. 
        /// </summary>
        [TestMethod]
        public void C_PopulateProposalActivitySSRSParameters()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            // test the base case, All
            ProposalActivityReportModelView mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.All,
                ProposalActivityFilterOption = ProposalActivityFilterOption.AllCustomerTypesFilter,
                ExecutionUserIds = "1"
            };

            Uri url = sut.PopulateProposalActivitySSRSParameters(mv);

            // the name of this report
            Assert.IsTrue(url.AbsoluteUri.Contains("Proposal%20Activity%20Report"));

            // if All was selected as one of the proposal status options, the rest are filtered out of the url and just All is used
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=All"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=True"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.EXECUTION_USER_ID + "=1"));

            // test Active proposal, All customer types/program Areas, and date range
            mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.Active,
                ProposalActivityFilterOption = ProposalActivityFilterOption.AllCustomerTypesFilter,
                CreateStartDate = "10/08/2008",
                CreateEndDate = "12/12/2013",
                ExecutionUserIds = "1"
            };

            url = sut.PopulateProposalActivitySSRSParameters(mv);
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=1,6,2")); // Active translates to 1, 6, 2 (in progress, submitted, complete)
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.CREATE_START_DATE + "=10/08/2008"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.CREATE_END_DATE + "=12/13/2013"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=True"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.EXECUTION_USER_ID + "=1"));

            // test Archived proposal, specific customer types/program Areas
            mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.Archived,
                ProposalActivityFilterOption = ProposalActivityFilterOption.SpecificCustomerTypesFilter,
                ProgramAreaIds = new Collection<int> { 667, 1234, 2344 },
                CustomerTypeIds = new Collection<int> { 1, 3 }
            };

            this.orgStructureDataMapper.Setup(x => x.GetAllProgramAreas()).Returns(new Collection<PickListDto> { new PickListDto { Id = 667 }, new PickListDto { Id = 1234 }, new PickListDto { Id = 2344 }, new PickListDto { Id = 5555 } });
            url = sut.PopulateProposalActivitySSRSParameters(mv);
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=" + ((int)ProposalReportStatus.Archived).ToString()));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROGRAM_AREA + "=667,1234,2344"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.CUSTOMER_TYPE + "=1,3"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=False"));

            // test Deleted proposal, specific customer types/program Areas, and pricer
            mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.Deleted,
                ProposalActivityFilterOption = ProposalActivityFilterOption.SpecificCustomerTypesFilter,
                ProgramAreaIds = new Collection<int> { 667, 1234, 2344 },
                CustomerTypeIds = new Collection<int> { 2, 4 },
                LeadEstimatorNtid = "testNtID"
            };

            url = sut.PopulateProposalActivitySSRSParameters(mv);
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=" + ((int)ProposalReportStatus.Deleted).ToString()));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROGRAM_AREA + "=667,1234,2344"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.CUSTOMER_TYPE + "=2,4"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=False"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PRICER_ID + "=testNtID"));

            // test Revision proposal, specific customer types/program Areas, and pricer
            mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.Revision,
                ProposalActivityFilterOption = ProposalActivityFilterOption.SpecificCustomerTypesFilter,
                ProgramAreaIds = new Collection<int> { 667, 1234, 2344 },
                CustomerTypeIds = new Collection<int> { 1 },
                TrackingNumber = "2013"
            };

            url = sut.PopulateProposalActivitySSRSParameters(mv);
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=" + ((int)ProposalReportStatus.Revision).ToString()));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROGRAM_AREA + "=667,1234,2344"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.CUSTOMER_TYPE + "=1"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=False"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.TRACKING_NUMBER + "=2013"));
        }

        /// <summary>
        /// populate the proposal activity SSRS parameters. 
        /// </summary>
        [TestMethod]
        public void C_PopulateProposalActivitySSRSParameters_AllProgramAreaAndAllCustomerTypes()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            // test the base case, All
            ProposalActivityReportModelView mv = new ProposalActivityReportModelView
            {
                ProposalStatus = ProposalReportStatus.All,
                ProposalActivityFilterOption = ProposalActivityFilterOption.SpecificCustomerTypesFilter,
                ExecutionUserIds = "1",
                ProgramAreaIds = new Collection<int> { 667, 1234, 2344 },
                CustomerTypeIds = new Collection<int> { 1, 2, 3, 4, 5, 6 }
            };

            this.orgStructureDataMapper.Setup(x => x.GetAllProgramAreas()).Returns(new Collection<PickListDto> { new PickListDto { Id = 667 }, new PickListDto { Id = 1234 }, new PickListDto { Id = 2344 } });
            Uri url = sut.PopulateProposalActivitySSRSParameters(mv);

            // the name of this report
            Assert.IsTrue(url.AbsoluteUri.Contains("Proposal%20Activity%20Report"));

            // if All was selected as one of the proposal status options, the rest are filtered out of the url and just All is used
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROPOSAL_STATUS + "=All"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.ALL_PROPOSALS + "=False"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.SPECIFIC_PROPOSALS + "=True"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROGRAM_AREA + "=All"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.CUSTOMER_TYPE + "=All"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.EXECUTION_USER_ID + "=1"));
        }

        /// <summary>
        /// Test the GetDataForDfarsReport method
        /// </summary>
        [TestMethod]
        public void C_GetDataForDfarsReport()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            PickListDto lineOfBusiness = new PickListDto()
            {
                Text = "myLineOfBusiness",
                Id = 2,
                IsActive = true
            };

            PickListDto programArea = new PickListDto()
            {
                Text = "myProgramArea",
                Id = 3,
                IsActive = true
            };

            this.orgStructureDataMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new List<PickListDto>() { lineOfBusiness });
            this.orgStructureDataMapper.Setup(x => x.GetAllProgramAreas()).Returns(new List<PickListDto>() { programArea });

            UserDTO user = new UserDTO
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                DisplayName = "Test User",
                Ntid = "testuser1",
                UserType = IES.Common.UserType.User
            };

            this.userMapper.Setup(x => x.GetActiveUser()).Returns(user);
            this.adUtils.Setup(x => x.GetGroupsForUser(user.Ntid)).Returns(new List<GroupData>());

            DfarsReportModelView result = sut.GetDataForDfarsReport();

            Assert.IsTrue(result.LobList.Any());
            Assert.IsTrue(result.ProgramAreaList.Any());
            Assert.IsTrue(result.ExecutionUserIds.Contains(user.Id.ToString()));
        }

        /// <summary>
        /// Test ValidateDfarsReportParameters when valid with paramters
        /// </summary>
        [TestMethod]
        public void C_ValidateDfarsReportParameters_Valid()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            DfarsReportModelView mv = new DfarsReportModelView()
            {
                Lobs = new Collection<int>() { 1 },
                ProgramAreas = new Collection<int>() { 1 },
                StartDate = "01/01/2019",
                EndDate = "12/31/2019",
                ExecutionUserIds = "1"
            };

            this.orgStructureDataMapper.Setup(x => x.GetProgramAreaById(It.IsAny<int>())).Returns(new PickListDto() { ParentIds = { 1 } });

            sut.ValidateDfarsReportParameters(mv, validationMessages);

            Assert.IsFalse(validationMessages.Any());
        }

        /// <summary>
        /// Test ValidateDfarsReportParameters when valid with no paramters
        /// </summary>
        [TestMethod]
        public void C_ValidateDfarsReportParameters_NoParamters()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            DfarsReportModelView mv = new DfarsReportModelView();

            sut.ValidateDfarsReportParameters(mv, validationMessages);

            // There should be no errors when no parameters are selected
            Assert.IsFalse(validationMessages.Any());
        }

        /// <summary>
        /// Test ValidateDfarsReportParameters for invalid start/end date formats
        /// </summary>
        [TestMethod]
        public void C_ValidateDfarsReportParameters_InvalidDateFormat()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            DfarsReportModelView mv = new DfarsReportModelView();

            mv.StartDate = "invalid date format";
            mv.EndDate = "February 31 2019";

            sut.ValidateDfarsReportParameters(mv, validationMessages);

            Assert.AreEqual(2, validationMessages.Count());
            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.DfarsReportValidationConstants.START_DATE_INVALID_FORMAT)).Any());
            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.DfarsReportValidationConstants.END_DATE_INVALID_FORMAT)).Any());
        }

        /// <summary>
        /// Test ValidateDfarsReportParameters for start date before end date
        /// </summary>
        [TestMethod]
        public void C_ValidateDfarsReportParameters_InvalidDateRange()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            DfarsReportModelView mv = new DfarsReportModelView();

            mv.StartDate = "12/31/2019";
            mv.EndDate = "01/01/2019";

            sut.ValidateDfarsReportParameters(mv, validationMessages);

            Assert.AreEqual(1, validationMessages.Count());
            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.DfarsReportValidationConstants.START_BEFORE_END_DATE)).Any());
        }

        /// <summary>
        /// Test ValidateDfarsReportParameters for missing start date or end date
        /// </summary>
        [TestMethod]
        public void C_ValidateDfarsReportParameters_IncompleteDateRange()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            DfarsReportModelView mv = new DfarsReportModelView();

            mv.StartDate = "01/01/2019";
            mv.EndDate = string.Empty;

            sut.ValidateDfarsReportParameters(mv, validationMessages);

            Assert.AreEqual(1, validationMessages.Count());
            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.DfarsReportValidationConstants.INCOMPLETE_DATE_RANGE)).Any());

            mv.StartDate = string.Empty; 
            mv.EndDate = "12/31/2019";
            validationMessages.Clear();

            sut.ValidateDfarsReportParameters(mv, validationMessages);

            Assert.AreEqual(1, validationMessages.Count());
            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(ValidationConstants.DfarsReportValidationConstants.INCOMPLETE_DATE_RANGE)).Any());
        }

        /// <summary>
        /// Test ValidateDfarsReportParameters for invalid Program Area selection
        /// </summary>
        [TestMethod]
        public void C_ValidateDfarsReportParameters_InvalidProgramArea()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            DfarsReportModelView mv = new DfarsReportModelView()
            {
                Lobs = new Collection<int>() { 1 },
                ProgramAreas = new Collection<int>() { 1 },
                StartDate = "01/01/2019",
                EndDate = "12/31/2019",
                ExecutionUserIds = "1"
            };

            this.orgStructureDataMapper.Setup(x => x.GetProgramAreaById(It.IsAny<int>())).Returns(new PickListDto() { ParentIds = { 2 }, Text = "TestPA" });
            this.orgStructureDataMapper.Setup(x => x.GetLineOfBusinessById(It.IsAny<int>())).Returns(new PickListDto() { Text = "TestLOB" });

            sut.ValidateDfarsReportParameters(mv, validationMessages);

            Assert.AreEqual(1, validationMessages.Count());
            Assert.IsTrue(validationMessages.Where(x => x.ValidationIssue.Contains(string.Format(ValidationConstants.DfarsReportValidationConstants.INVALID_PROGRAM_AREA, "TestLOB", "TestPA"))).Any());
        }

        /// <summary>
        /// Test PopulateDfarsSSRSParameters
        /// </summary>
        [TestMethod]
        public void C_PopulateDfarsSSRSParameters()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            DfarsReportModelView mv = new DfarsReportModelView()
            {
                Lobs = new Collection<int>() { 1, 2, 3 },
                ProgramAreas = new Collection<int>() { 1, 2, 3 },
                StartDate = "01/01/2019",
                EndDate = "12/31/2019",
                ExecutionUserIds = "1,2"
            };

            PickListDto lob1 = new PickListDto()
            {
                Text = "testLOB",
                Id = 1,
                IsActive = true
            };

            PickListDto lob2 = new PickListDto()
            {
                Text = "testLOB",
                Id = 2,
                IsActive = true
            };

            PickListDto lob3 = new PickListDto()
            {
                Text = "testLOB",
                Id = 3,
                IsActive = true
            };

            PickListDto lob4 = new PickListDto()
            {
                Text = "testLOB",
                Id = 4,
                IsActive = true
            };

            PickListDto pa1 = new PickListDto()
            {
                Text = "testPA",
                Id = 1,
                IsActive = true
            };

            PickListDto pa2 = new PickListDto()
            {
                Text = "testPA",
                Id = 2,
                IsActive = true
            };

            PickListDto pa3 = new PickListDto()
            {
                Text = "testPA",
                Id = 3,
                IsActive = true
            };

            PickListDto pa4 = new PickListDto()
            {
                Text = "testPA",
                Id = 4,
                IsActive = true
            };

            this.orgStructureDataMapper.Setup(x => x.GetAllLinesOfBusiness()).Returns(new List<PickListDto>() { lob1, lob2, lob3, lob4 });
            this.orgStructureDataMapper.Setup(x => x.GetAllProgramAreas()).Returns(new List<PickListDto>() { pa1, pa2, pa3, pa4 });

            Uri url = sut.PopulateDfarsSSRSParameters(mv);

            Assert.IsTrue(url.AbsoluteUri.Contains("DFARS%20Non-Standard%20Responses"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.NO_REPORT_PARAMETERS));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.LINE_OF_BUSINESS + "=1,2,3"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.PROGRAM_AREA + "=1,2,3"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.START_DATE + "=01/01/2019" ));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.END_DATE + "=01/01/2020"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.EXECUTION_USER_ID + "=1,2"));
        }

        /// <summary>
        /// Test PopulateDfarsSSRSParameters when no Parameters are selected
        /// </summary>
        [TestMethod]
        public void C_PopulateDfarsSSRSParameters_NoParameters()
        {
            ReportsControllerLogic sut = this.CreateSystem();

            DfarsReportModelView mv = new DfarsReportModelView()
            {
                // Execution User Ids still needed
                ExecutionUserIds = "1,2"
            };

            Uri url = sut.PopulateDfarsSSRSParameters(mv);

            Assert.IsTrue(url.AbsoluteUri.Contains("DFARS%20Non-Standard%20Responses"));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.NO_REPORT_PARAMETERS));
            Assert.IsFalse(url.AbsoluteUri.Contains(Constants.Report.LINE_OF_BUSINESS));
            Assert.IsFalse(url.AbsoluteUri.Contains(Constants.Report.PROGRAM_AREA));
            Assert.IsFalse(url.AbsoluteUri.Contains(Constants.Report.START_DATE));
            Assert.IsFalse(url.AbsoluteUri.Contains(Constants.Report.END_DATE));
            Assert.IsTrue(url.AbsoluteUri.Contains(Constants.Report.EXECUTION_USER_ID + "=1,2"));
        }

        #region Exception Tests
        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateProposalLogParameters_ExceptionTest1()
        {
            ReportsControllerLogic sut = this.CreateSystem();
            sut.ValidateProposalLogParameters(null, new List<ValidationMessage>());
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateProposalLogParameters_ExceptionTest2()
        {
            ReportsControllerLogic sut = this.CreateSystem();
            sut.ValidateProposalLogParameters(new ProposalLogReportModelView(), null);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_PopulateProposalLogSSRSParameters_ExceptionTest1()
        {
            ReportsControllerLogic sut = this.CreateSystem();
            sut.PopulateProposalLogSSRSParameters(null);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateProposalActivityParameters_ExceptionTest1()
        {
            ReportsControllerLogic sut = this.CreateSystem();
            sut.ValidateProposalActivityParameters(null, new List<ValidationMessage>());
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateProposalActivityParameters_ExceptionTest2()
        {
            ReportsControllerLogic sut = this.CreateSystem();
            sut.ValidateProposalActivityParameters(new ProposalActivityReportModelView(), null);
        }

        /// <summary>
        /// Exception Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_PopulateProposalActivitySSRSParameters_ExceptionTest1()
        {
            ReportsControllerLogic sut = this.CreateSystem();
            sut.PopulateProposalActivitySSRSParameters(null);
        }

        /// <summary>
        /// Null Exception Test for reportParameters for ValidateDfarsReportParameters
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateDfarsReportParameters_NullReportParameters()
        {
            ReportsControllerLogic sut = this.CreateSystem();
            ICollection<ValidationMessage> validationMessages = new List<ValidationMessage>();
            sut.ValidateDfarsReportParameters(null, validationMessages);
        }

        /// <summary>
        /// Null Exception Test for inValidationErrors for ValidateDfarsReportParameters
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_ValidateDfarsReportParameters_NullValidationErrors()
        {
            ReportsControllerLogic sut = this.CreateSystem();
            DfarsReportModelView mv = new DfarsReportModelView();
            sut.ValidateDfarsReportParameters(mv, null);
        }

        /// <summary>
        /// Null Exception Test for PopulateDfarsSSRSParameters
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void C_PopulateDfarsSSRSParameters_NullReportParameters()
        {
            ReportsControllerLogic sut = this.CreateSystem();
            sut.PopulateDfarsSSRSParameters(null);
        }

        #endregion Exception Tests
    }
}