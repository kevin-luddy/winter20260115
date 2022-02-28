// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Configuration;
    using System.Web.Mvc;
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

    /// <summary>
    /// Business logic class for the Reports Controller
    /// </summary>
    public class ReportsControllerLogic : GenTRACControllerLogic
    {
        /// <summary>
        /// The name of the proposal log's report parameters section, needed for validation
        /// </summary>
        public const string PROPOSAL_LOG_REPORT_PARAMETER_FORM = "proposalLogReportParameterForm";

        /// <summary>
        /// Reports loader
        /// </summary>
        private IReportsLoader reportsLoader = null;

        /// <summary>
        /// Org structure mapper for Program Areas and Lines of Business
        /// </summary>
        private IOrgStructureDataMapper orgStructureMapper = null;

        /// <summary>
        /// The AD utils
        /// </summary>
        private IES.Common.IActiveDirectoryUtilities activeDirectoryUtils = null;

        /// <summary>
        /// Create static Regex object for ReportTrackingNumber.
        /// </summary>
        private static Regex regexReportTrackingNumber = new Regex(ValidationConstants.REPORT_TRACKING_NUMBER_FORMAT);

        /// <summary>
        /// The name of the proposal activity's report parameters section, needed for validation
        /// </summary>
        public const string PROPOSAL_ACTIVITY_REPORT_PARAMETER_FORM = "proposalActivityReportParameterForm";

        /// <summary>
        /// The name of the DFARS report paramters section, needed for validation
        /// </summary>
        public const string DFARS_REPORT_PARAMETER_FORM = "dfarsReportParameterForm";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityAccess">Security Access</param>
        /// <param name="inProposalLoader">Proposal Loader</param>
        /// <param name="inUserMapper">User Mapper</param>
        /// <param name="objectFactory">Object Factory</param>
        /// <param name="reportsLoader">reports loader</param>
        /// <param name="orgStructureDataMapper">org structure data mapper</param>
        /// <param name="adUtilities">active directory utilities</param>
        /// <param name="approvalsLoader">Approvals Loader</param>
        /// <param name="proposalChecklistLoader">Proposal Checklist Loader</param>
        /// <param name="inChecklistMediator">Checklist Mediator</param>
        /// <param name="inProposalMediator">Proposal Mediator</param>
        public ReportsControllerLogic(
            ISecurityAccess inSecurityAccess,
            IProposalLoader inProposalLoader,
            IUserMapper inUserMapper,
            IFullObjectFactory objectFactory,
            IReportsLoader reportsLoader,
            IOrgStructureDataMapper orgStructureDataMapper,
            IES.Common.IActiveDirectoryUtilities adUtilities,
            IApprovalsLoader approvalsLoader,
            IProposalChecklistLoader proposalChecklistLoader,
            IChecklistMediator inChecklistMediator,
            IProposalMediator inProposalMediator)
            : base(inSecurityAccess, inProposalLoader, inUserMapper, objectFactory, approvalsLoader, proposalChecklistLoader, inChecklistMediator, inProposalMediator)
        {
            this.reportsLoader = reportsLoader;
            this.orgStructureMapper = orgStructureDataMapper;
            this.activeDirectoryUtils = adUtilities;
        }

        /// <summary>
        /// Get data to show on the propsoal log report
        /// </summary>
        /// <returns>ProposalLogReportModelView</returns>
        public ProposalLogReportModelView GetDataForProposalLogReport()
        {
            ProposalLogReportModelView model = new ProposalLogReportModelView();

            // set up proposal log's proposal status
            model.ProposalLogStatusList = new Collection<SelectListItem>();
            foreach (ProposalReportStatus status in Enum.GetValues(typeof(ProposalReportStatus)))
            {
                model.ProposalLogStatusList.Add(new SelectListItem() { Value = ((int)status).ToString(), Text = status.GetDescription(), Selected = status.Equals(ProposalReportStatus.Active) ? true : false });
            }

            // set up years
            model.YearsList = new Collection<SelectListItem>();
            ICollection<int> years = this.reportsLoader.GetProposalYears();
            foreach (int x in years)
            {
                model.YearsList.Add(new SelectListItem() { Value = x.ToString(), Text = x.ToString() });
            }

            // set up Line Of Business
            model.LOBList = this.GetLOBList();

            // set up lead estimators 
            var pricerUserIDs = this.reportsLoader.GetPricerUserIds();
            ICollection<UserDTO> pricers = this.UserMapper.GetUserDtosByUserIds(pricerUserIDs);
            model.LeadEstimatorList = new Collection<SelectListItem>();

            if (pricers != null)
            {
                foreach (UserDTO pricer in pricers)
                {
                    if (!string.IsNullOrWhiteSpace(pricer.DisplayName))
                    {
                        model.LeadEstimatorList.Add(new SelectListItem() { Value = pricer.Id.ToString(), Text = pricer.DisplayName });
                    }
                }
            }

            // for the current user, need to determine if they are part of any groups. if they are, need to pass it to the SSRS
            model.ExecutionUserIds = this.GetExecutionUserIds();

            return model;
        }

        /// <summary>
        /// Return Program Area list
        /// </summary>
        /// <returns>list box items</returns>
        public Collection<SelectListItem> GetProgramAreasList()
        {
            Collection<SelectListItem> toReturn = new Collection<SelectListItem>();

            foreach (PickListDto programArea in this.orgStructureMapper.GetAllProgramAreas().OrderBy(x => x.Text))
            {
                toReturn.Add(new SelectListItem() { Value = programArea.Id.ToString(), Text = programArea.Text });
            }

            return toReturn;
        }

        /// <summary>
        /// Return Line of Business list
        /// </summary>
        /// <returns>list box items</returns>
        public Collection<SelectListItem> GetLOBList()
        {
            Collection<SelectListItem> toReturn = new Collection<SelectListItem>();

            foreach (PickListDto lob in this.orgStructureMapper.GetAllLinesOfBusiness().OrderBy(x => x.Text))
            {
                toReturn.Add(new SelectListItem() { Value = lob.Id.ToString(), Text = lob.Text });
            }

            return toReturn;
        }

        /// <summary>
        /// Validate proposal log report parameters
        /// </summary>
        /// <param name="reportParameters">report parameters</param>
        /// <param name="inValidationErrors">validation errors</param>
        public void ValidateProposalLogParameters(ProposalLogReportModelView reportParameters, ICollection<ValidationMessage> inValidationErrors)
        {
            if (reportParameters == null)
            {
                throw new ArgumentNullException(nameof(reportParameters));
            }

            if (inValidationErrors == null)
            {
                throw new ArgumentNullException(nameof(inValidationErrors));
            }

            if (reportParameters.ProposalLogFilterOption.HasValue)
            {
                // only validate the submit date range or specific propsoal
                if (reportParameters.ProposalLogFilterOption.Value == ProposalLogFilterOption.SpecificProposalRadioFilter)
                {
                    if (reportParameters.Years == null && reportParameters.LOBIds == null && reportParameters.CentralEstimators == null)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalLogReportValidationConstants.SPECIFIC_PROPOSAL_SELECTION_REQUIRED));
                    }
                }
                else if (reportParameters.ProposalLogFilterOption.Value == ProposalLogFilterOption.SubmitDateRangeFilter)
                {
                    if (string.IsNullOrEmpty(reportParameters.SubmitStartDate) || string.IsNullOrEmpty(reportParameters.SubmitEndDate))
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalLogReportValidationConstants.SUBMIT_DATE_REQUIRED));
                    }

                    if (!string.IsNullOrEmpty(reportParameters.SubmitStartDate))
                    {
                        try
                        {
                            reportParameters.SubmitStartDate.ToDateTime("MM/dd/yyyy");
                        }
                        catch (FormatException)
                        {
                            inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalLogReportValidationConstants.SUBMIT_START_NOT_VALID_DATE_FORMAT));
                        }
                    }

                    if (!string.IsNullOrEmpty(reportParameters.SubmitEndDate))
                    {
                        try
                        {
                            reportParameters.SubmitEndDate.ToDateTime("MM/dd/yyyy");
                        }
                        catch (FormatException)
                        {
                            inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalLogReportValidationConstants.SUBMIT_END_NOT_VALID_DATE_FORMAT));
                        }
                    }

                    if (!string.IsNullOrEmpty(reportParameters.SubmitStartDate) && !string.IsNullOrEmpty(reportParameters.SubmitEndDate))
                    {
                        var dateFormatError = from v in inValidationErrors
                                              where v.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.SUBMIT_END_NOT_VALID_DATE_FORMAT) ||
                                              v.ValidationIssue.Contains(ValidationConstants.ProposalLogReportValidationConstants.SUBMIT_START_NOT_VALID_DATE_FORMAT)
                                              select v;
                        // only compare the start date against the end date if they can both be converted to date time
                        if (!dateFormatError.Any())
                        {
                            DateTime startDate = reportParameters.SubmitStartDate.ToDateTime("MM/dd/yyyy");
                            DateTime endDate = reportParameters.SubmitEndDate.ToDateTime("MM/dd/yyyy");

                            if (startDate > endDate)
                            {
                                inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalLogReportValidationConstants.SUBMIT_START_DATE_RANGE));
                            }
                        }
                    }
                }
                else if (reportParameters.ProposalLogFilterOption.Value == ProposalLogFilterOption.TrackingNumberFilter)
                {
                    reportParameters.TrackingNumber = reportParameters.TrackingNumber.Trim();

                    if (string.IsNullOrEmpty(reportParameters.TrackingNumber))
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_REQUIRED));
                    }
                    else
                    {
                        bool trackingMatch = regexReportTrackingNumber.IsMatch(reportParameters.TrackingNumber);
                        if (trackingMatch == false)
                        {
                            inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_FORMAT));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Populate the SSRS Uri for the proposal log given the report parameters
        /// </summary>
        /// <param name="reportParameters">report parameters</param>
        /// <returns>Uri with parameters to open the SSRS report in</returns>
        public Uri PopulateProposalLogSSRSParameters(ProposalLogReportModelView reportParameters)
        {
            if (reportParameters == null)
            {
                throw new ArgumentNullException(nameof(reportParameters));
            }

            Uri toReturn = null;

            StringBuilder sb = new StringBuilder();

            // do not show report parameters
            sb.Append(Constants.Report.NO_REPORT_PARAMETERS);

            // get proposal status
            if (reportParameters.ProposalLogStatus.Contains((int)ProposalReportStatus.All))
            {
                sb.Append(string.Format("&{0}={1}", Constants.Report.PROPOSAL_STATUS, "All"));
            }
            else
            {
                ICollection<int> statusIds = new Collection<int>();
                statusIds = reportParameters.ProposalLogStatus;

                // if proposal log status is Active, this really represents Proposal Status of In Progress, Submitted, and Completed so grab the correct IDs to send
                if (statusIds.Contains((int)ProposalReportStatus.Active))
                {
                    statusIds.Remove((int)ProposalReportStatus.Active);
                    statusIds.Add((int)ProposalStatus.InProgress);
                    statusIds.Add((int)ProposalStatus.Completed);
                    statusIds.Add((int)ProposalStatus.PendingCertification);
                }

                sb.Append(string.Format("&{0}={1}", Constants.Report.PROPOSAL_STATUS, string.Join(",", statusIds)));
            }

            // get filter option
            switch (reportParameters.ProposalLogFilterOption.Value)
            {
                case ProposalLogFilterOption.AllProposalsFilter:
                    sb.Append(string.Format("&{0}={1}", Constants.Report.ALL_PROPOSALS, "True"));
                    break;
                case ProposalLogFilterOption.SpecificProposalRadioFilter:
                    sb.Append(string.Format("&{0}={1}", Constants.Report.ALL_PROPOSALS, "False"));
                    sb.Append(string.Format("&{0}={1}", Constants.Report.SPECIFIC_PROPOSALS, "True"));
                    if (reportParameters.Years != null)
                    {
                        int dbYearCount = this.reportsLoader.GetProposalYears().Count();

                        // if all years were selected, send the keyword All instead of each year
                        if (dbYearCount == reportParameters.Years.Count())
                        {
                            sb.Append(string.Format("&{0}={1}", Constants.Report.YEAR, "All"));
                        }
                        else
                        {
                            sb.Append(string.Format("&{0}={1}", Constants.Report.YEAR, string.Join(",", reportParameters.Years)));
                        }
                    }

                    if (reportParameters.LOBIds != null)
                    {
                        int dbLOBCount = this.orgStructureMapper.GetAllLinesOfBusiness().Count();

                        // if all program Areas were selected, send the keyword ALL instead of each program Area
                        if (dbLOBCount == reportParameters.LOBIds.Count())
                        {
                            sb.Append(string.Format("&{0}={1}", Constants.Report.LINE_OF_BUSINESS, "All"));
                        }
                        else
                        {
                            sb.Append(string.Format("&{0}={1}", Constants.Report.LINE_OF_BUSINESS, string.Join(",", reportParameters.LOBIds)));
                        }
                    }

                    if (reportParameters.CentralEstimators != null)
                    {
                        int centralFieldCount = this.UserMapper.GetUserDtosByUserIds(this.reportsLoader.GetPricerUserIds()).Count();

                        // if all central field pricers were selected, send the keyword ALL instead of each pricer
                        if (reportParameters.CentralEstimators.Count() == centralFieldCount)
                        {
                            sb.Append(string.Format("&{0}={1}", Constants.Report.CENTRAL_ESTIMATOR, "All"));
                        }
                        else
                        {
                            sb.Append(string.Format("&{0}={1}", Constants.Report.CENTRAL_ESTIMATOR, string.Join(",", reportParameters.CentralEstimators)));
                        }
                    }

                    break;
                case ProposalLogFilterOption.SubmitDateRangeFilter:
                    sb.Append(string.Format("&{0}={1}", Constants.Report.ALL_PROPOSALS, "False"));
                    sb.Append(string.Format("&{0}={1}", Constants.Report.SUBMIT_START_DATE, reportParameters.SubmitStartDate));
                    sb.Append(string.Format("&{0}={1}", Constants.Report.SUBMIT_END_DATE, reportParameters.SubmitEndDate));
                    break;
                case ProposalLogFilterOption.TrackingNumberFilter:
                    sb.Append(string.Format("&{0}={1}", Constants.Report.ALL_PROPOSALS, "False"));
                    sb.Append(string.Format("&{0}={1}", Constants.Report.TRACKING_NUMBER, reportParameters.TrackingNumber));
                    break;
                default:
                    break;
            }

            sb.Append(string.Format("&{0}={1}", Constants.Report.EXECUTION_USER_ID, reportParameters.ExecutionUserIds));

            toReturn = new Uri(string.Format("{0}/{1}/{2}{3}", WebConfigurationManager.AppSettings["ReportServerLocation"], WebConfigurationManager.AppSettings["ReportServerFolderName"], "Proposal Log Report", sb));
            return toReturn;
        }

        /// <summary>
        /// Get data to show on the propsoal activity report
        /// </summary>
        /// <returns>ProposalActivityReportModelView</returns>
        public ProposalActivityReportModelView GetDataForProposalActivityReport()
        {
            ProposalActivityReportModelView model = new ProposalActivityReportModelView();

            // set up proposal activity's proposal status
            model.ProposalStatusList = EnumUtilities.GetListItemsForEnumSorted(typeof(ProposalReportStatus));

            model.CustomerTypeList = EnumUtilities.GetListItemsForEnumSorted(typeof(CustomerType)).Where(x => !string.IsNullOrEmpty(x.Value)).ToList();

            // set up Program Area
            model.ProgramAreaList = this.GetProgramAreasList();

            string groupName = IES.Common.ConfigurationUtilities.GetAppSetting("LeadEstimators");
            model.LeadEstimatorList = this.activeDirectoryUtils.GetAdGroupUsers(groupName.GetObjectName());

            // for the current user, need to determine if they are part of any groups. if they are, need to pass it to the SSRS
            model.ExecutionUserIds = this.GetExecutionUserIds();

            return model;
        }

        /// <summary>
        /// Get execution User Ids of the current user
        /// </summary>
        /// <returns>user ID of current user and any groups they belong to</returns>
        public string GetExecutionUserIds()
        {
            string executionUserIds = string.Empty;

            // get the user table PK ids for ...
            List<int> permissionedUserIds = new List<int>();

            // get the current user
            UserDTO currentUser = this.UserMapper.GetActiveUser();
            if (currentUser != null)
            {
                permissionedUserIds.Add(currentUser.Id);

                // get the groups the current user may be in
                ICollection<IES.Common.GroupData> adUserGroups = this.activeDirectoryUtils.GetGroupsForUser(currentUser.Ntid);

                // get all groups in the system
                List<UserDTO> allApplicationGroupsAndCurrentUser = new List<UserDTO>();
                ICollection<UserDTO> groupUsers = this.UserMapper.GetAllGroups();

                if (groupUsers != null)
                {
                    allApplicationGroupsAndCurrentUser.AddRange(groupUsers);
                }

                // now add current user to the groups
                allApplicationGroupsAndCurrentUser.Add(currentUser);

                // get all the permission Ids
                ICollection<int> permissionedGroupIds = (from ad in adUserGroups
                                                         join g in allApplicationGroupsAndCurrentUser on ad.Ntid.ToLower() equals g.Ntid.ToLower()
                                                         select g.Id).ToList();

                permissionedUserIds.AddRange(permissionedGroupIds);

                executionUserIds = string.Join(",", permissionedUserIds);

                return executionUserIds;
            }
            else
            {
                throw new GeneralAppException("User information could not be found for current user. Cannot proceed with report processing");
            }
        }

        /// <summary>
        /// Validate proposal activity report parameters
        /// </summary>
        /// <param name="reportParameters">report parameters</param>
        /// <param name="inValidationErrors">validation errors</param>
        public void ValidateProposalActivityParameters(ProposalActivityReportModelView reportParameters, ICollection<ValidationMessage> inValidationErrors)
        {
            if (reportParameters == null)
            {
                throw new ArgumentNullException(nameof(reportParameters));
            }

            if (inValidationErrors == null)
            {
                throw new ArgumentNullException(nameof(inValidationErrors));
            }

            if (reportParameters.ProposalActivityFilterOption.HasValue)
            {
                // only validate if specific customer types is selected
                if (reportParameters.ProposalActivityFilterOption.Value == ProposalActivityFilterOption.SpecificCustomerTypesFilter)
                {
                    if (reportParameters.CustomerTypeIds == null && reportParameters.ProgramAreaIds == null)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalActivityReportValidationConstants.SPECIFIC_CUSTOMER_TYPE_PROGRAM_AREA_SELECTION_REQUIRED));
                    }
                }
            }

            // ensure create start date is in the correct date format if it's populated
            if (!string.IsNullOrEmpty(reportParameters.CreateStartDate))
            {
                try
                {
                    reportParameters.CreateStartDate.ToDateTime("MM/dd/yyyy");
                }
                catch (FormatException)
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalActivityReportValidationConstants.CREATE_START_NOT_VALID_DATE_FORMAT));
                }
            }

            // ensure create end date is in the correct date format if it's populated
            if (!string.IsNullOrEmpty(reportParameters.CreateEndDate))
            {
                try
                {
                    reportParameters.CreateEndDate.ToDateTime("MM/dd/yyyy");
                }
                catch (FormatException)
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalActivityReportValidationConstants.CREATE_END_NOT_VALID_DATE_FORMAT));
                }
            }

            // if start and end date are populated and are in correct date formats, check the range is correct
            if (!string.IsNullOrEmpty(reportParameters.CreateStartDate) && !string.IsNullOrEmpty(reportParameters.CreateEndDate))
            {
                var dateFormatError = from v in inValidationErrors
                                      where v.ValidationIssue.Contains(ValidationConstants.ProposalActivityReportValidationConstants.CREATE_END_NOT_VALID_DATE_FORMAT) ||
                                      v.ValidationIssue.Contains(ValidationConstants.ProposalActivityReportValidationConstants.CREATE_START_NOT_VALID_DATE_FORMAT)
                                      select v;
                // only compare the start date against the end date if they can both be converted to date time
                if (!dateFormatError.Any())
                {
                    DateTime startDate = reportParameters.CreateStartDate.ToDateTime("MM/dd/yyyy");
                    DateTime endDate = reportParameters.CreateEndDate.ToDateTime("MM/dd/yyyy");

                    if (startDate > endDate)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalActivityReportValidationConstants.CREATE_START_DATE_RANGE));
                    }
                }
            }

            // if one of the dates is filled out and the other isn't, throw incomplete date range
            if ((string.IsNullOrEmpty(reportParameters.CreateStartDate) && !string.IsNullOrEmpty(reportParameters.CreateEndDate)) || (!string.IsNullOrEmpty(reportParameters.CreateStartDate) && string.IsNullOrEmpty(reportParameters.CreateEndDate)))
            {
                inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalActivityReportValidationConstants.INCOMPLETE_CREATE_DATE_RANGE));
            }

            // if tracking number is populated make sure it's in the right format
            if (!string.IsNullOrEmpty(reportParameters.TrackingNumber))
            {
                reportParameters.TrackingNumber = reportParameters.TrackingNumber.Trim();

                bool trackingMatch = regexReportTrackingNumber.IsMatch(reportParameters.TrackingNumber);
                if (trackingMatch == false)
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.ProposalLogReportValidationConstants.TRACKING_NUMBER_FORMAT));
                }
            }
        }

        /// <summary>
        /// Populate the SSRS Uri for the proposal activity given the report parameters
        /// </summary>
        /// <param name="reportParameters">report parameters</param>
        /// <returns>Uri with parameters to open the SSRS report in</returns>
        public Uri PopulateProposalActivitySSRSParameters(ProposalActivityReportModelView reportParameters)
        {
            if (reportParameters == null)
            {
                throw new ArgumentNullException(nameof(reportParameters));
            }

            Uri toReturn = null;

            StringBuilder sb = new StringBuilder();

            // do not show report parameters
            sb.Append(Constants.Report.NO_REPORT_PARAMETERS);

            // get proposal status
            if (reportParameters.ProposalStatus == ProposalReportStatus.All)
            {
                sb.Append(string.Format("&{0}={1}", Constants.Report.PROPOSAL_STATUS, "All"));
            }
            else if (reportParameters.ProposalStatus == ProposalReportStatus.Active)
            {
                ICollection<int> statusIds = new Collection<int>();
                statusIds.Add((int)ProposalStatus.InProgress);
                statusIds.Add((int)ProposalStatus.PendingCertification);
                statusIds.Add((int)ProposalStatus.Completed);
                sb.Append(string.Format("&{0}={1}", Constants.Report.PROPOSAL_STATUS, string.Join(",", statusIds)));
            }
            else
            {
                sb.Append(string.Format("&{0}={1}", Constants.Report.PROPOSAL_STATUS, (int)reportParameters.ProposalStatus));
            }

            // get filter option
            switch (reportParameters.ProposalActivityFilterOption.Value)
            {
                case ProposalActivityFilterOption.AllCustomerTypesFilter:
                    sb.Append(string.Format("&{0}={1}", Constants.Report.ALL_PROPOSALS, "True"));
                    break;
                case ProposalActivityFilterOption.SpecificCustomerTypesFilter:
                    sb.Append(string.Format("&{0}={1}", Constants.Report.ALL_PROPOSALS, "False"));
                    sb.Append(string.Format("&{0}={1}", Constants.Report.SPECIFIC_PROPOSALS, "True"));
                    if (reportParameters.CustomerTypeIds != null)
                    {
                        int customerCount = EnumUtilities.GetListItemsForEnumSorted(typeof(CustomerType)).Count;

                        // if all customers were selected, send the keyword All instead of each year
                        if (customerCount == reportParameters.CustomerTypeIds.Count())
                        {
                            sb.Append(string.Format("&{0}={1}", Constants.Report.CUSTOMER_TYPE, "All"));
                        }
                        else
                        {
                            sb.Append(string.Format("&{0}={1}", Constants.Report.CUSTOMER_TYPE, string.Join(",", reportParameters.CustomerTypeIds)));
                        }
                    }

                    if (reportParameters.ProgramAreaIds != null)
                    {
                        int dbProgramAreaCount = this.orgStructureMapper.GetAllProgramAreas().Count();

                        // if all Program Areas were selected, send the keyword ALL instead of each ProgramArea
                        if (dbProgramAreaCount == reportParameters.ProgramAreaIds.Count())
                        {
                            sb.Append(string.Format("&{0}={1}", Constants.Report.PROGRAM_AREA, "All"));
                        }
                        else
                        {
                            sb.Append(string.Format("&{0}={1}", Constants.Report.PROGRAM_AREA, string.Join(",", reportParameters.ProgramAreaIds)));
                        }
                    }

                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(reportParameters.CreateStartDate))
            {
                sb.Append(string.Format("&{0}={1}", Constants.Report.CREATE_START_DATE, reportParameters.CreateStartDate));
            }

            if (!string.IsNullOrEmpty(reportParameters.CreateEndDate))
            {
                DateTime endDate = reportParameters.CreateEndDate.ToDateTime("MM/dd/yyyy").AddDays(1);
                sb.Append(string.Format("&{0}={1}", Constants.Report.CREATE_END_DATE, endDate.ToString("MM/dd/yyyy")));
            }

            if (!string.IsNullOrEmpty(reportParameters.TrackingNumber))
            {
                sb.Append(string.Format("&{0}={1}", Constants.Report.TRACKING_NUMBER, reportParameters.TrackingNumber));
            }

            if (!string.IsNullOrEmpty(reportParameters.LeadEstimatorNtid))
            {
                sb.Append(string.Format("&{0}={1}", Constants.Report.PRICER_ID, reportParameters.LeadEstimatorNtid));
            }

            sb.Append(string.Format("&{0}={1}", Constants.Report.EXECUTION_USER_ID, reportParameters.ExecutionUserIds));

            toReturn = new Uri(string.Format("{0}/{1}/{2}{3}", WebConfigurationManager.AppSettings["ReportServerLocation"], WebConfigurationManager.AppSettings["ReportServerFolderName"], "Proposal Activity Report", sb));
            return toReturn;
        }

        /// <summary>
        /// Get data to show on the DFARS report
        /// </summary>
        /// <returns>Dfars Report Modelview</returns>
        public DfarsReportModelView GetDataForDfarsReport()
        {
            DfarsReportModelView model = new DfarsReportModelView();

            model.LobList = this.GetLOBList();
            model.ProgramAreaList = this.GetProgramAreasList();

            // for the current user, need to determine if they are part of any groups. if they are, need to pass it to the SSRS
            model.ExecutionUserIds = this.GetExecutionUserIds();

            return model;
        }

        /// <summary>
        /// Validate DFARS report parameters
        /// </summary>
        /// <param name="reportParameters">report parameters</param>
        /// <param name="inValidationErrors">validation errors</param>
        public void ValidateDfarsReportParameters(DfarsReportModelView reportParameters, ICollection<ValidationMessage> inValidationErrors)
        {
            if (reportParameters == null)
            {
                throw new ArgumentNullException(nameof(reportParameters));
            }

            if (inValidationErrors == null)
            {
                throw new ArgumentNullException(nameof(inValidationErrors));
            }

            // ensure create start date is in the correct date format if it's populated
            if (!string.IsNullOrEmpty(reportParameters.StartDate))
            {
                try
                {
                    reportParameters.StartDate.ToDateTime("MM/dd/yyyy");
                }
                catch (FormatException)
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.DfarsReportValidationConstants.START_DATE_INVALID_FORMAT));
                }
            }

            // ensure create end date is in the correct date format if it's populated
            if (!string.IsNullOrEmpty(reportParameters.EndDate))
            {
                try
                {
                    reportParameters.EndDate.ToDateTime("MM/dd/yyyy");
                }
                catch (FormatException)
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.DfarsReportValidationConstants.END_DATE_INVALID_FORMAT));
                }
            }

            // if start and end date are populated and are in correct date formats, check the range is correct
            if (!string.IsNullOrEmpty(reportParameters.StartDate) && !string.IsNullOrEmpty(reportParameters.EndDate))
            {
                var dateFormatError = from v in inValidationErrors
                                      where v.ValidationIssue.Contains(ValidationConstants.DfarsReportValidationConstants.END_DATE_INVALID_FORMAT) ||
                                      v.ValidationIssue.Contains(ValidationConstants.DfarsReportValidationConstants.START_DATE_INVALID_FORMAT)
                                      select v;
                // only compare the start date against the end date if they can both be converted to date time
                if (!dateFormatError.Any())
                {
                    DateTime startDate = reportParameters.StartDate.ToDateTime("MM/dd/yyyy");
                    DateTime endDate = reportParameters.EndDate.ToDateTime("MM/dd/yyyy");

                    if (startDate > endDate)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.DfarsReportValidationConstants.START_BEFORE_END_DATE));
                    }
                }
            }

            // if one of the dates is filled out and the other isn't, throw incomplete date range
            if ((string.IsNullOrEmpty(reportParameters.StartDate) && !string.IsNullOrEmpty(reportParameters.EndDate)) || (!string.IsNullOrEmpty(reportParameters.StartDate) && string.IsNullOrEmpty(reportParameters.EndDate)))
            {
                inValidationErrors.Add(new ValidationMessage(ValidationConstants.DfarsReportValidationConstants.INCOMPLETE_DATE_RANGE));
            }

            // validate that selected Program Areas belong to the selected LOBs if both had selections
            if(reportParameters.ProgramAreas.Any() && reportParameters.Lobs.Any())
            {
                foreach(int programAreaId in reportParameters.ProgramAreas)
                {
                    PickListDto pa = this.orgStructureMapper.GetProgramAreaById(programAreaId);

                    if(!pa.ParentIds.Intersect(reportParameters.Lobs).Any())
                    {
                        ICollection<PickListDto> lobsForPA = new Collection<PickListDto>();
                        foreach (var lobId in pa.ParentIds)
                        {
                            lobsForPA.Add(this.orgStructureMapper.GetLineOfBusinessById(lobId));
                        }

                        string requiredLobs = string.Join(", ", lobsForPA.Select(x => x.Text));

                        inValidationErrors.Add(new ValidationMessage(string.Format(ValidationConstants.DfarsReportValidationConstants.INVALID_PROGRAM_AREA, requiredLobs, pa.Text)));
                    }
                }
            }
        }

        /// <summary>
        /// Populate the SSRS Uri for the DFARS report given the report parameters
        /// </summary>
        /// <param name="reportParameters">report parameters</param>
        /// <returns>Uri with parameters to open the SSRS report in</returns>
        public Uri PopulateDfarsSSRSParameters(DfarsReportModelView reportParameters)
        {
            if (reportParameters == null)
            {
                throw new ArgumentNullException(nameof(reportParameters));
            }

            Uri toReturn = null;

            StringBuilder sb = new StringBuilder();

            // do not show report parameters
            sb.Append(Constants.Report.NO_REPORT_PARAMETERS);

            // LOBs
            if (reportParameters.Lobs.Any())
            {
                int dbLOBCount = this.orgStructureMapper.GetAllLinesOfBusiness().Count();

                // if all LOBs were selected, send the keyword ALL instead of each LOB
                if (dbLOBCount == reportParameters.Lobs.Count())
                {
                    sb.Append(string.Format("&{0}={1}", Constants.Report.LINE_OF_BUSINESS, "All"));
                }
                else
                {
                    sb.Append(string.Format("&{0}={1}", Constants.Report.LINE_OF_BUSINESS, string.Join(",", reportParameters.Lobs)));
                }
            }

            // Program Areas
            if (reportParameters.ProgramAreas.Any())
            {
                int dbPACount = this.orgStructureMapper.GetAllProgramAreas().Count();

                // if all PAs were selected, send the keyword ALL instead of each PA
                if (dbPACount == reportParameters.ProgramAreas.Count())
                {
                    sb.Append(string.Format("&{0}={1}", Constants.Report.PROGRAM_AREA, "All"));
                }
                else
                {
                    sb.Append(string.Format("&{0}={1}", Constants.Report.PROGRAM_AREA, string.Join(",", reportParameters.ProgramAreas)));
                }
            }

            // date range
            if (!string.IsNullOrEmpty(reportParameters.StartDate))
            {
                sb.Append(string.Format("&{0}={1}", Constants.Report.START_DATE, reportParameters.StartDate));
            }

            if (!string.IsNullOrEmpty(reportParameters.EndDate))
            {
                DateTime endDate = reportParameters.EndDate.ToDateTime("MM/dd/yyyy").AddDays(1);
                sb.Append(string.Format("&{0}={1}", Constants.Report.END_DATE, endDate.ToString("MM/dd/yyyy")));
            }

            // Execution User IDs
            sb.Append(string.Format("&{0}={1}", Constants.Report.EXECUTION_USER_ID, reportParameters.ExecutionUserIds));

            // build URI
            toReturn = new Uri(string.Format("{0}/{1}/{2}{3}", WebConfigurationManager.AppSettings["ReportServerLocation"], WebConfigurationManager.AppSettings["ReportServerFolderName"], "DFARS Non-Standard Responses", sb));

            return toReturn;
        }

        /// <summary>
        /// Get the program areas for the selected lobs
        /// </summary>
        /// <param name="lobs">Selected LOBs</param>
        /// <returns>PAs for selected LOBs</returns>
        public string GetProgramAreasForLOBs (ICollection<int> lobs)
        {
            string toReturn = string.Empty;
            if (lobs != null && lobs.Any())
            {
                foreach(int lob in lobs)
                {
                    toReturn += this.orgStructureMapper.GetProgramAreaHtmlOptionsForLineOfBusiness(lob, null, false);
                }
            } 
            else
            {
                // get all
                toReturn = this.orgStructureMapper.GetProgramAreaHtmlOptionsForLineOfBusiness(null, null, false);
            }

            return toReturn;
        }
    }
}