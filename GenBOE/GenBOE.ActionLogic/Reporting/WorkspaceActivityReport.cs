// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    public class WorkspaceActivityReport
    {
        private readonly string ONE_DECIMAL = "N1";
        
        private IPermissionsDTODataLoader _PermissionsDTOLoader;

        public WorkspaceActivityReport(IPermissionsDTODataLoader inPermissionsDTOLoader)
        {
            this._PermissionsDTOLoader = inPermissionsDTOLoader;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public WorkspaceActivityReportModelView GenerateReport(FullWorkspace inWorkspace)
        {
            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }

            WorkspaceActivityReportModelView toReturn = new WorkspaceActivityReportModelView();

            // number of days left until proposal submittal
            DateTime? submittalDate = inWorkspace.ProposalSubmittalDate;
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

            // number of days in initialization
            var historyDTOs = inWorkspace.WorkspaceHistory; 

            TimeSpan timeInState = this.ComputeTimeSpan(historyDTOs, WorkspaceState.Initialization);
            toReturn.DaysInInitialization = timeInState.TotalDays.ToString(this.ONE_DECIMAL);


            // number of days in working
            timeInState = this.ComputeTimeSpan(historyDTOs, WorkspaceState.Working);
            toReturn.DaysInWorking = timeInState.TotalDays.ToString(this.ONE_DECIMAL);


            // number of days in locked
            timeInState = this.ComputeTimeSpan(historyDTOs, WorkspaceState.Locked);
            toReturn.DaysInLocked = timeInState.TotalDays.ToString(this.ONE_DECIMAL);

            // days from init to complete
            WorkspaceState startState = WorkspaceState.Initialization;
            WorkspaceState endState = WorkspaceState.Complete;
            if (inWorkspace.WorkspaceState != endState)
            {
                toReturn.DaysFromInitializationToComplete = "-";
            }
            else
            {
                DateTime firstDayInStartState = inWorkspace.WorkspaceHistory.Where(x => x.NewValue == startState).Select(x => x.Date).First();
                DateTime lastDayInEndState = inWorkspace.WorkspaceHistory.Where(x => x.NewValue == endState).Select(x => x.Date).Last();
                toReturn.DaysFromInitializationToComplete = (lastDayInEndState - firstDayInStartState).TotalDays.ToString(this.ONE_DECIMAL);
            }

            // days from working to complete
            startState = WorkspaceState.Working;
            endState = WorkspaceState.Complete;
            if (inWorkspace.WorkspaceState != endState)
            {
                toReturn.DaysFromWorkingToComplete = "-";
            }
            else
            {
                DateTime firstDayInStartState = inWorkspace.WorkspaceHistory.Where(x => x.NewValue == startState).Select(x => x.Date).First();
                DateTime lastDayInEndState = inWorkspace.WorkspaceHistory.Where(x => x.NewValue == endState).Select(x => x.Date).Last();
                toReturn.DaysFromWorkingToComplete = (lastDayInEndState - firstDayInStartState).TotalDays.ToString(this.ONE_DECIMAL);
            }

            // days to closed            
            endState = WorkspaceState.Closed;
            if (inWorkspace.WorkspaceState != endState)
            {
                toReturn.DaysToClosed = "-";
            }
            else
            {
                DateTime creationDate = inWorkspace.WorkspaceHistory.Select(x => x.Date).First();
                DateTime lastDayInEndState = inWorkspace.WorkspaceHistory.Where(x => x.NewValue == endState).Select(x => x.Date).Last();
                toReturn.DaysToClosed = (lastDayInEndState - creationDate).TotalDays.ToString(this.ONE_DECIMAL);
            }

            // number of days in initialization
            toReturn.NumberOfTimesInInitialization = this.ComputeNumberOfTimesInState(inWorkspace.WorkspaceHistory, WorkspaceState.Initialization).ToString();

            // number of days in working
            toReturn.NumberOfTimesInWorking = this.ComputeNumberOfTimesInState(inWorkspace.WorkspaceHistory, WorkspaceState.Working).ToString();

            // number of days in locked
            toReturn.NumberOfTimesInLocked = this.ComputeNumberOfTimesInState(inWorkspace.WorkspaceHistory, WorkspaceState.Locked).ToString();

            // number of days in complete
            toReturn.NumberOfTimesInComplete = this.ComputeNumberOfTimesInState(inWorkspace.WorkspaceHistory, WorkspaceState.Complete).ToString();

            // number of days in closed
            toReturn.NumberOfTimesInClosed = this.ComputeNumberOfTimesInState(inWorkspace.WorkspaceHistory, WorkspaceState.Closed).ToString();

            // number of times export to ProPricer
            toReturn.NumberOfTimesExportedToProPricer = inWorkspace.NumberOfTimesExportedToProPricer.ToString();

            // Number of BOEs
            var boes = inWorkspace.Boes;
            toReturn.NumberOfBOEs = boes.Count.ToString();

            // Number of BOEs in Unassigned state
            toReturn.NumberOfBOEsUnassigned = boes.Where(x => x.State == BOEState.Unassigned).Select(x => x).Count().ToString();

            // Number of BOEs in Draft state
            int numberOfBOEsDraft = boes.Where(x => x.State == BOEState.Draft).Select(x => x).Count() + boes.Where(x => x.State == BOEState.DraftLocked).Select(x => x).Count();
            toReturn.NumberOfBOEsDraft = numberOfBOEsDraft.ToString();

            // Number of BOEs in Awaiting Approval state
            toReturn.NumberOfBOEsAwaitingApproval = boes.Where(x => x.State == BOEState.AwaitingApproval).Select(x => x).Count().ToString();

            // Number of BOEs in Approved state
            toReturn.NumberOfBOEsApproved = boes.Where(x => x.State == BOEState.Approved).Select(x => x).Count().ToString();

            // number of admins
            //    - get the total number of users in the workspace admin role
            List<PermissionsDTO> workspacePermissions = this._PermissionsDTOLoader.GetWorkspacePermissions(inWorkspace.Id).ToList();

            var admins = (from p in workspacePermissions
                          where p.Role == Role.WorkspaceAdmin
                          select p.ETIUserId).Distinct();

            toReturn.NumberOfAdministrators = admins.Count().ToString();

            var boeIds = boes.Select(x => x.Id).ToList();
            
            List<PermissionsDTO> boePermissions = this._PermissionsDTOLoader.GetBOEPermissions(boeIds.ToCollection()).ToList();
            List<PermissionsDTO> allAuthorInfo = (from auth in boePermissions 
                                                 where auth.Role == Role.Author
                                                 select auth).ToList();

            List<PermissionsDTO> allSubcontractorAuthorInfo = (from sub in boePermissions 
                                                 where sub.Role == Role.SubcontractorAuthor
                                                 select sub).ToList();

            List<PermissionsDTO> allApproverInfo = (from app in boePermissions
                                                    where app.Role == Role.Approver
                                                    select app).ToList();

            // number of approvers
            //    - get each collection of approvers for each BOE
            //    - get the distinct userIds across all BOE approver lists

            toReturn.NumberOfApprovers = allApproverInfo.Select(x => x.ETIUserId).Distinct().Count().ToString();

            int numberOfAuthors = allAuthorInfo.Select(x => x.ETIUserId).Distinct().Count();
            int numberOfSubcontractorAuthors = allSubcontractorAuthorInfo.Select(x => x.ETIUserId).Distinct().Count();
            toReturn.NumberOfAuthors = (numberOfAuthors + numberOfSubcontractorAuthors).ToString();

            // number of reviewers
            //    - get the total number of users in the workspace reviewer role
            var reviewers = (from r in workspacePermissions where r.Role == Role.WorkspaceReviewer select r).Select(x => x.ETIUserId).Distinct();
            
            toReturn.NumberOfReviewers = reviewers.Count().ToString();

            // average number of BOEs/Author
            //    - get total # of BOEs (BOES)
            //    - get total # of unique Authors in workspace (AUTHORS)
            //
            //    Average = BOES/AUTHORS
            int boesCount = boeIds.Count();
            int authorsCount = numberOfAuthors + numberOfSubcontractorAuthors;
            if (authorsCount > 0)
            {
                decimal average = Decimal.Divide(boesCount, authorsCount);
                toReturn.AverageNumberOfBOEsPerAuthor = average.ToString();
            }
            else
            {
                toReturn.AverageNumberOfBOEsPerAuthor = "-";
            }
                

            return toReturn;
        }

        /// <summary>
        /// Compute the number of times a workspace has entered into a state (accumulating multiple entries)
        /// </summary>
        /// <param name="historyDTOs">The history DTOs for the workspace</param>
        /// <param name="stateOfInterest">The state computing for</param>
        /// <returns>the number of times in the stateOfInterest</returns>
        public int ComputeNumberOfTimesInState(IReadOnlyCollection<WorkspaceHistoryDTO> historyDTOs, WorkspaceState stateOfInterest)
        {
            return historyDTOs.Where(x => x.NewValue == stateOfInterest).Select(x => x).Count();
        }

        /// <summary>
        /// Compute the accumulated time span that a workspace has been in the stateOfInterest
        /// </summary>
        /// <param name="historyDTOs">the history DTOs for the workspace</param>
        /// <param name="stateOfInterest">The state computing for</param>
        /// <returns>the timespan representing the aggregate time spend in a given stateOfInterest</returns>
        public TimeSpan ComputeTimeSpan(IReadOnlyCollection<WorkspaceHistoryDTO> historyDTOs, WorkspaceState stateOfInterest)
        {
            List<WorkspaceHistoryDTO> allTransitionsInvolvingState =
                historyDTOs.Where(x => x.OldValue == stateOfInterest || x.NewValue == stateOfInterest).OrderBy(x => x.Date).Select(x => x).ToList();

            TimeSpan hoursInState = new TimeSpan();
            if (allTransitionsInvolvingState.Count > 1)
            {
                for (int x = 0; x < allTransitionsInvolvingState.Count-1; x = x + 2)
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
