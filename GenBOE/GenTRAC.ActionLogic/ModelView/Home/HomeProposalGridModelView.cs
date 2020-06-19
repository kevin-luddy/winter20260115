// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Home
{
    using System;
    using System.Collections.Generic;
    using GeneralHelper;
    using IES.Common;

    /// <summary>
    /// The My Proposals Grid row that is shown on the home page
    /// </summary>
    public class HomeProposalGridModelView : SortableModelView<HomeProposalGridModelView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeProposalGridModelView"/> class.
        /// </summary>
        public HomeProposalGridModelView()
        {
            this.Workspaces = new List<string>();
            this.HasWriteAccessToLinkedDocument = false;
        }

        /// <summary>
        /// Proposal ID
        /// </summary>
        public int ProposalId { get; set; }

        /// <summary>
        /// Tracking Number
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets the forecasted tracking number.
        /// </summary>
        public string ForecastedTrackingNumber { get; set; }

        /// <summary>
        /// Proposal title
        /// </summary>
        public string ProposalTitle { get; set; }

        /// <summary>
        /// Program Area
        /// </summary>
        public string ProgramArea { get; set; }

        /// <summary>
        /// Customer
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// Is Commecial Customer
        /// </summary>
        public bool IsCommercialCustomer { get; set; }

        /// <summary>
        /// estimate value
        /// </summary>
        public long? EstValue { get; set; }

        /// <summary>
        /// Gets the string value of the Estimated Value
        /// </summary>
        public string EstValueString
        {
            get
            {
                if (this.EstValue.HasValue)
                {
                    if (this.EstValue.Value < 0)
                    {
                        return string.Format("({0:#,###0})", Math.Abs(this.EstValue.Value));
                    }
                    else
                    {
                        return string.Format("{0:#,###0}", this.EstValue);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Proposal date assigned
        /// </summary>
        public DateTime? ProposalDateAssigned { get; set; }

        /// <summary>
        /// Gets the string value of the Proposal Date Assigned
        /// </summary>
        public string ProposalDateAssignedString
        {
            get
            {
                return this.ProposalDateAssigned.HasValue ? this.ProposalDateAssigned.Value.ToString("MM/dd/yyyy") : string.Empty;
            }
        }

        /// <summary>
        /// proposal due date
        /// </summary>
        public DateTime ProposalDueDate { get; set; }

        /// <summary>
        /// Gets the string value of the Proposal Due Date
        /// </summary>
        public string ProposalDueDateString
        {
            get
            {
                return this.ProposalDueDate.ToString("MM/dd/yyyy");
            }
        }

        /// <summary>
        /// proposal checklist completed date
        /// </summary>
        public DateTime? ProposalCompletedDate { get; set; }

        /// <summary>
        /// Gets the string value of the Proposal Completed Date
        /// </summary>
        public string ProposalCompletedDateString
        {
            get
            {
                if (this.ProposalCompletedDate.HasValue)
                {
                    return this.ProposalCompletedDate.Value.ToString("MM/dd/yyyy");
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// checklist complete date
        /// </summary>
        public DateTime? ChecklistCompleteDate { get; set; }

        /// <summary>
        /// Gets the string value of the Checklist Complete Date
        /// </summary>
        public string ChecklistCompleteDateString
        {
            get
            {
                if (this.ChecklistCompleteDate.HasValue)
                {
                    return this.ChecklistCompleteDate.Value.ToString("MM/dd/yyyy");
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// proposal status 
        /// </summary>
        public ProposalStatus Status { get; set; }

        /// <summary>
        /// Gets or sets Pricers's account display name
        /// </summary>
        public string PricerDisplayName { get; set; }

        /// <summary>
        /// Peer Reviewer's Display Name
        /// </summary>
        public string PeerReviewerDisplayName { get; set; }

        /// <summary>
        /// Cost Volume Lead's Display Name.
        /// </summary>
        public string CostVolumeLeadDisplayName { get; set; }

        /// <summary>
        /// Capture Manager's Display Name
        /// </summary>
        public string CaptureManagerDisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has linked document.
        /// </summary>
        public bool HasLinkedDocument { get; set; }

        /// <summary>
        /// Indicates whether the current user has write access to the linked document
        /// </summary>
        public bool HasWriteAccessToLinkedDocument { get; set; }

        /// <summary>
        /// Indicates whether the current user has delete access (admin or lead estimator) for the proposal, and the proposal is in a deletable state.
        /// </summary>
        public bool IsDeleteAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is forecast proposal.
        /// </summary>
        public bool IsForecastProposal { get; set; }

        /// <summary>
        /// CSS class, indicating what color background the Due Date column should be
        /// Grey, If proposal status is set to Completed.
        /// Red, If due date is less than current date, highlight the cell red with white text
        /// Green,  If due date is greater than current date + 14, then highlight the cell green.
        /// Yellow, If due date is in the range current date + 14 then highlight the cell yellow
        /// </summary>
        public string DueDateBackground
        {
            get
            {
                if (this.IsForecastProposal)
                {
                    int forecastedDaysOut = IES.Common.ConfigurationUtilities.GetAppSetting<int>("ForecastedDaysOut");
                    if (this.ProposalDueDate < DateTime.Now.AddDays(forecastedDaysOut))
                    {
                        return Constants.FORECASTED_NEAR_DATE_CSS_CLASS_STRING;
                    }
                    else
                    {
                        return Constants.FORECASTED_FAR_DATE_CSS_CLASS_STRING;
                    }
                }
                else
                {
                    if (this.Status.Equals(ProposalStatus.Completed) || this.Status.Equals(ProposalStatus.Revised))
                    {
                        return Constants.GREY_BACKGROUND_CSS_CLASS_STRING;
                    }
                    else if (Helpers.IsProposalCertificationLate(this.Status, this.ProposalCompletedDate))
                    {
                        return Constants.SUBMITTED_LATE_BACKGROUND_CSS_CLASS_STRING;
                    }
                    else if (this.Status.Equals(ProposalStatus.Submitted))
                    {
                        return Constants.SUBMITTED_NOT_LATE_BACKGROUND_CSS_CLASS_STRING;
                    }
                    else
                    {
                        DateTime today = DateTime.Now.Date;

                        if (today > this.ProposalDueDate)
                        {
                            return Constants.RED_BACKGROUND_CSS_CLASS_STRING;
                        }
                        else if (this.ProposalDueDate > today.AddDays(14))
                        {
                            return Constants.GREEN_BACKGROUND_CSS_CLASS_STRING;
                        }
                        else
                        {
                            return Constants.YELLOW_BACKGROUND_CSS_CLASS_STRING;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the workspaces linked to this proposal.
        /// </summary>
        public ICollection<string> Workspaces { get; set; }
    } 
}
