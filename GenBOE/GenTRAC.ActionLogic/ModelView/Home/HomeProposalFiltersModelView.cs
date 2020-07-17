// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Home
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using IES.Common;

    /// <summary>
    /// Home model view for proposal filters
    /// </summary>
    public class HomeProposalFiltersModelView
    {
        /// <summary>
        /// List of options for filtering proposals
        /// </summary>
        public ICollection<SelectListItem> FilterOptions { get; set; }

        /// <summary>
        /// Selected filter option
        /// </summary>
        public ProposalFilterOption FilterOption { get; set; }

        /// <summary>
        /// List of options for filtering by Proposal Class.
        /// </summary>
        public ICollection<SelectListItem> ProposalClassFilterOptions { get; set; }

        /// <summary>
        /// Selected Proposal Class filter option.
        /// </summary>
        public ProposalClassFilterOption ProposalClassFilterOption { get; set; }

        /// <summary>
        /// Filter start date
        /// </summary>
        [GenTRAC.ActionLogic.Validation.StartEndDateValidationAttribute(EndDate = "FilterEndDate", DateFormat = "MM/dd/yyyy", CanBeEqual = true, ValidIfOneDateIsNullAndTheOtherIsNot = false)]
        [Display(Name = "Filter Start Date")]
        public string FilterStartDate { get; set; }

        /// <summary>
        /// Filter end date
        /// </summary>
        [Display(Name = "Filter End Date")]
        public string FilterEndDate { get; set; }

        /// <summary>
        /// Flag for whether current user has the viewer role or is in a group that has the viewer role.
        /// </summary>
        public string IsViewer { get; set; }

        /// <summary>
        /// List of options for filtering proposals when user has the viewer role (Show only my proposals, Show my organization's proposals)
        /// </summary>
        public ICollection<SelectListItem> ViewerFilterOptions { get; set; }

        /// <summary>
        /// Selected viewer filter option
        /// </summary>
        public ViewerProposalFilterOption ViewerFilterOption { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public HomeProposalFiltersModelView()
        {
        }
    }
}
