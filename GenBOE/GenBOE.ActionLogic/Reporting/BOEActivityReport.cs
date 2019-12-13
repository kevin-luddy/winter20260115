// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.ActionLogic.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    public class BOEActivityReport
    {
        private readonly string ONE_DECIMAL = "N1";
        private readonly string sEmpty = string.Empty;

        private IBOEHistoryDTODataLoader _BOEHistoryDTODataLoader;
        private IUserDTODataLoader _UserDTODataLoader;
        private ICommonDataMapper _CommonDataMapper;
        private IPermissionsDTODataLoader _PermissionsLoader;

        public BOEActivityReport(IBOEHistoryDTODataLoader inBOEHistoryDTODataLoader,
                                       IUserDTODataLoader inUserDTODataLoader,
                                       ICommonDataMapper inCommonDataMapper,
                                       IPermissionsDTODataLoader inPermissionsLoader)
        {
            this._BOEHistoryDTODataLoader = inBOEHistoryDTODataLoader;
            this._UserDTODataLoader = inUserDTODataLoader;
            this._CommonDataMapper = inCommonDataMapper;
            this._PermissionsLoader = inPermissionsLoader;
        }

        public BOEActivityReport(IBOEHistoryDTODataLoader inBOEHistoryDTODataLoader,
                               IUserDTODataLoader inUserDTODataLoader,
                               ICommonDataMapper inCommonDataMapper)
        {
            this._BOEHistoryDTODataLoader = inBOEHistoryDTODataLoader;
            this._UserDTODataLoader = inUserDTODataLoader;
            this._CommonDataMapper = inCommonDataMapper;
        }

        /// <summary>
        /// Send a BOEActivityReport report to file and return the file name
        /// </summary>
        /// <param name="templateFileLocation">The template file location.</param>
        /// <param name="modelView">The model view.</param>
        /// <returns>
        /// The path of the generated file
        /// </returns>
        [ExcludeFromCodeCoverage]
        public string SendBOEActivityReportToFile(string templateFileLocation, BOEActivityReportModelView modelView)
        {
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }
            if (modelView == null)
            {
                throw new ArgumentNullException(nameof(modelView));
            }

            string toReturn = "";

            // Create all rows for the export file
            var worksheet = new ExcelExportWorksheet();
            worksheet.AddRange(from boe in modelView.Rows
                               select new Collection<string>
                               {
                                   boe.WBSNum,
                                   boe.WBSTitle,
                                   boe.BOETitle != null ? boe.BOETitle : this.sEmpty,
                                   boe.CLINNum,
                                   boe.CLINTitle,
                                   boe.Authors.Replace("<br/>","; "),
                                   boe.Status,
                                   boe.DaysInUnassigned,
                                   boe.DaysInDraft,
                                   boe.DaysInAwaitingApproval,
                                   boe.DaysFromCreatedToApproved,
                                   boe.DaysFromDraftToApproved,
                                   boe.NumTimesInDraft,
                                   boe.NumTimesInAwaitingApproval,
                                   boe.NumTimesInApproved,
                                   boe.NumTimesAuthorReassigned
                               });

            // Pass the rows to the generic Excel exporter
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);

            // Return the file name of the Export File
            return toReturn;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public BOEActivityReportModelView GenerateReport(FullWorkspace workspaceObject)
        {
            if (workspaceObject == null)
            {
                throw new ArgumentNullException(nameof(workspaceObject));
            }

            BOEActivityReportModelView toReturn = new BOEActivityReportModelView();

            IDictionary<int,BOEStateModelView> allBOEStates = this._CommonDataMapper.getBOEStatesDictionary();

            // number of days left until proposal submittal
            DateTime? submittalDate = workspaceObject.ProposalSubmittalDate;
            if (submittalDate.HasValue && submittalDate.Value > DateTime.Now)
            {
                int daysUntilSubmittalDue = (int)Math.Ceiling((submittalDate.Value - DateTime.Now).TotalDays);
                if (daysUntilSubmittalDue < 0)
                {
                    daysUntilSubmittalDue = 0;
                }

                toReturn.DaysLeftUntilProposalSubmittalDate =
                    daysUntilSubmittalDue.ToString() +
                    (daysUntilSubmittalDue == 1 ? " day" : " days") +
                    " left until proposal submittal (" + submittalDate.Value.ToString("MM/dd/yyyy") + ")";
            }
            else if (submittalDate.HasValue && submittalDate.Value < DateTime.Now)
            {
                int daysSinceSubmittalDue = (int)Math.Ceiling((DateTime.Now - submittalDate.Value).TotalDays);
                if (daysSinceSubmittalDue < 0)
                {
                    daysSinceSubmittalDue = 0;
                }

                toReturn.DaysLeftUntilProposalSubmittalDate =
                    daysSinceSubmittalDue.ToString() +
                    (daysSinceSubmittalDue == 1 ? " day" : " days") +
                    " since the scheduled proposal submittal (" + submittalDate.Value.ToString("MM/dd/yyyy") + ")";
            }

            // for each BOE generate a row in the model view
            toReturn.Rows = new Collection<BOEActivityReportRow>();

            HashSet<PermissionsDTO> permissionsAssociatedWithBoes = new HashSet<PermissionsDTO>(this._PermissionsLoader.GetBOEPermissions(workspaceObject.Boes.Select(x => x.Id).ToList()));
            HashSet<PermissionsDTO> authorsForBoes = new HashSet<PermissionsDTO>(permissionsAssociatedWithBoes.Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor).ToCollection());
            HashSet<UserDTO> usersForBoe = new HashSet<UserDTO>(this._UserDTODataLoader.GetByIds(authorsForBoes.Select(x => x.ETIUserId).Distinct().ToList()));

            foreach (BoeDTO boe in workspaceObject.Boes)
            {
                BOEActivityReportRow row = new BOEActivityReportRow();

                row.BOETitle = boe.Title;

                // wbs #
                WbsDTO wbs = workspaceObject.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
                if (wbs != null)
                {
                    row.WBSNum = wbs.WbsNumber;
                    row.WBSNumPadded = wbs.WbsPaddedNumber;
                    row.WBSTitle = wbs.WbsTitle;
                }
                else
                {
                    row.WBSNum = "-";
                    row.WBSTitle = "-";
                }

                ClinDTO clin = workspaceObject.Clins.FirstOrDefault(x => x.Id == boe.CLINID);
                if (clin != null)
                {
                    row.CLINNum = clin.ClinNumber;
                    row.CLINTitle = clin.ClinTitle;
                }
                else
                {
                    row.CLINNum = "-";
                    row.CLINTitle = "-";
                }

                // authors
                row.Authors = "-";

                var boeAuthors = authorsForBoes.Where(x => x.BOEId == boe.Id).ToCollection();
                if (boeAuthors.Any())
                {
                    var authors = from author in boeAuthors
                                  where author.Role == Role.Author
                                  select usersForBoe.First(x => x.UserID == author.ETIUserId).DisplayName;

                    var subAuthors = from author in boeAuthors
                                               where author.Role == Role.SubcontractorAuthor
                                               select usersForBoe.First(x => x.UserID == author.ETIUserId).DisplayName + " (Sub)";


                    var combinedAuthors = authors.Union(subAuthors);  // combine the authors lists

                    var allAuthors = combinedAuthors.OrderBy(x => x).Distinct().ToList();

                    row.Authors = allAuthors.Count > 0 ? string.Join("<br/>", allAuthors.Select(x => x)) : string.Empty;
                }

                // status
                row.Status = allBOEStates[(int)boe.State].BOEState;

                ICollection<BOEHistoryDTO> boeHistory = this._BOEHistoryDTODataLoader.GetBOEHistory(boe.Id); // this data comes from an SP. there isn't a good way to do a bulk load

                // days in unassigned
                string stateOfInterest = allBOEStates[(int)BOEState.Unassigned].BOEState;
                row.DaysInUnassigned = this.ComputeTimeSpan(boeHistory, stateOfInterest).TotalDays.ToString(this.ONE_DECIMAL);

                // days in draft
                stateOfInterest = allBOEStates[(int)BOEState.Draft].BOEState;
                double daysDraft = this.ComputeTimeSpan(boeHistory, stateOfInterest).TotalDays;
                stateOfInterest = allBOEStates[(int)BOEState.DraftLocked].BOEState;
                daysDraft += this.ComputeTimeSpan(boeHistory, stateOfInterest).TotalDays;
                row.DaysInDraft = daysDraft.ToString(this.ONE_DECIMAL);

                // days in awaiting approval
                stateOfInterest = allBOEStates[(int)BOEState.AwaitingApproval].BOEState;
                row.DaysInAwaitingApproval = this.ComputeTimeSpan(boeHistory, stateOfInterest).TotalDays.ToString(this.ONE_DECIMAL);

                // days from created to approved
                string startState = allBOEStates[(int)BOEState.None].BOEState;
                string endState = allBOEStates[(int)BOEState.Approved].BOEState;
                if (boe.State != BOEState.Approved)
                {
                    row.DaysFromCreatedToApproved = "-";
                }
                else
                {
                    var testIfNoneExists = boeHistory.Where(x => x.NewValue == startState).Select(x => x.Date).ToList();
                    if (testIfNoneExists.None())
                    {
                        startState = allBOEStates[(int)BOEState.Draft].BOEState; // some data doesn't start at 0 (None), but instead starts at state 1 (Draft)
                    }

                    DateTime firstDayInStartState = boeHistory.Where(x => x.NewValue == startState).Select(x => x.Date).First();
                    DateTime lastDayInEndState = boeHistory.Where(x => x.NewValue == endState).Select(x => x.Date).Last();
                    row.DaysFromCreatedToApproved = (lastDayInEndState - firstDayInStartState).TotalDays.ToString(this.ONE_DECIMAL);
                }

                // days from draft to approved
                startState = allBOEStates[(int)BOEState.Draft].BOEState;
                endState = allBOEStates[(int)BOEState.Approved].BOEState;
                if (boe.State != BOEState.Approved)
                {
                    row.DaysFromDraftToApproved = "-";
                }
                else
                {
                    // the getBoeHistoryDTO returns dates in descending order so we need to find the first day in Draft with the oldest date and the first day in Approved
                    // with the earliest date
                    DateTime firstDayInStartState = boeHistory.Where(x => x.NewValue == startState).Select(x => x.Date).Last();
                    DateTime lastDayInEndState = boeHistory.Where(x => x.NewValue == endState).Select(x => x.Date).First();
                    row.DaysFromDraftToApproved = (lastDayInEndState - firstDayInStartState).TotalDays.ToString(this.ONE_DECIMAL);
                }

                // # times in draftFieldType
                row.NumTimesInDraft = (this.ComputeNumberOfTimesInState(boeHistory, allBOEStates[(int)BOEState.Draft].BOEState)
                    + this.ComputeNumberOfTimesInState(boeHistory, allBOEStates[(int)BOEState.DraftLocked].BOEState))
                    .ToString();

                // # times in awaiting approval
                stateOfInterest = allBOEStates[(int)BOEState.AwaitingApproval].BOEState;
                row.NumTimesInAwaitingApproval = this.ComputeNumberOfTimesInState(boeHistory, stateOfInterest).ToString();

                // # times in approved
                // to get the actual number of times in approved, the SP would return an old value of empty/null and a new value of Approved
                // if you don't use this logic, you can get false positives with just counting "Approved"
                stateOfInterest = allBOEStates[(int)BOEState.Approved].BOEState;

                var approvedHistory = boeHistory.Where(x => x.OldValue == allBOEStates[(int)BOEState.AwaitingApproval].BOEState && x.NewValue == BOEState.Approved.ToString()).Select(x => x).ToList();
                row.NumTimesInApproved = this.ComputeNumberOfTimesInState(approvedHistory, stateOfInterest).ToString();

                // # times authors reassigned
                row.NumTimesAuthorReassigned = boe.NumAuthorReassigned.ToString();

                toReturn.Rows.Add(row);
            }

            return toReturn;
        }

        /// <summary>
        /// Compute the number of times a BOE has entered into a state (accumulating multiple entries)
        /// </summary>
        /// <param name="historyDTOs">The history DTOs for the workspace</param>
        /// <param name="stateOfInterest">The state computing for</param>
        /// <returns>the number of times in the stateOfInterest</returns>
        public int ComputeNumberOfTimesInState(ICollection<BOEHistoryDTO> historyDTOs, string stateOfInterest)
        {
            return historyDTOs.Count(x => x.NewValue == stateOfInterest);
        }

        /// <summary>
        /// Compute the accumulated time span that a BOE has been in the stateOfInterest
        /// </summary>
        /// <param name="historyDTOs">the history DTOs for the BOE</param>
        /// <param name="stateOfInterest">The state computing for</param>
        /// <returns>the timespan representing the aggregate time spend in a given stateOfInterest</returns>
        public TimeSpan ComputeTimeSpan(ICollection<BOEHistoryDTO> historyDTOs, string stateOfInterest)
        {
            List<BOEHistoryDTO> allTransitionsInvolvingState =
                historyDTOs.Where(x => x.OldValue == stateOfInterest || x.NewValue == stateOfInterest).OrderBy(x => x.Date).Select(x => x).ToList();

            TimeSpan hoursInState = new TimeSpan();
            if (allTransitionsInvolvingState.Count > 1)
            {
                for (int x = 0; x < allTransitionsInvolvingState.Count - 1; x = x + 2)
                {
                    // transitions that contain the state of interest should always be counted in pairs
                    hoursInState = hoursInState.Add(allTransitionsInvolvingState[x + 1].Date - allTransitionsInvolvingState[x].Date);
                }
            }

            // if we had an odd number of transitions AND the last history record we have has our state in it,
            // it means the last transition is still occurring .. so take 'now' - last
            if (allTransitionsInvolvingState.Count % 2 != 0 &&
                historyDTOs.Last().NewValue == stateOfInterest)
            {
                hoursInState = hoursInState.Add(DateTime.Now - allTransitionsInvolvingState.Last().Date);
            }

            return hoursInState;
        }
    }
}
