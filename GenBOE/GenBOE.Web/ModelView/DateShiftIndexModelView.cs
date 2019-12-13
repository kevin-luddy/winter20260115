// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using IES.Common;
    
    /// <summary>
    /// Index Model View for Date Shift
    /// </summary>
    public class DateShiftIndexModelView : GenBOEMasterModelView
    {
        public ICollection<SelectListItem> SpreadCurves { get; set; }

        /// <summary>
        /// Gets or sets the date shift level.
        /// </summary>
        public Level DateShiftLevel { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Gets or sets the parent level.
        /// </summary>
        public Level ParentLevel { get; set; }

        /// <summary>
        /// Gets or sets the parent start date.
        /// </summary>
        public string ParentStartDate { get; set; }

        /// <summary>
        /// Gets or sets the parent end date.
        /// </summary>
        public string ParentEndDate { get; set; }
        
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [contains discrete].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [contains discrete]; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsDiscrete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if there are any Discrete month values outside of POP.
        /// </summary>
        public bool DiscreteOutsidePop { get; set; }

        public string ReturnUrl { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateShiftIndexModelView" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="dateShiftLevel">The date shift level.</param>
        /// <param name="containsDiscrete">if set to <c>true</c> [contains discrete].</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="spreadCurves">The spread curves.</param>
        /// <param name="returnUrl">The return url</param>
        /// <param name="discreteOutsidePOP">Is there any current problems with Outside POP when there is discrete.</param>
        /// <param name="title">Title</param>
        /// <param name="parentLevel">The parent dateshift level.</param>
        /// <param name="parentStart">The parent start date.</param>
        /// <param name="parentEnd">The parent end date.</param>
        /// <param name="proposalName">Proposal Name</param>
        /// <param name="workspaceState">Workspace State</param>
        public DateShiftIndexModelView(int id, Level dateShiftLevel, bool containsDiscrete, bool discreteOutsidePOP, string startDate, string endDate, ICollection<SelectListItem> spreadCurves, string returnUrl,
            string title, Level parentLevel, string parentStart, string parentEnd, string proposalName, string workspaceState)
        {
            this.Id = id;
            this.DateShiftLevel = dateShiftLevel;
            this.ContainsDiscrete = containsDiscrete;
            this.DiscreteOutsidePop = discreteOutsidePOP;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.SpreadCurves = spreadCurves;
            this.ReturnUrl = returnUrl;
            this.Title = title;
            this.ParentLevel = parentLevel;
            this.ParentStartDate = parentStart;
            this.ParentEndDate = parentEnd;
            this.ProposalName = proposalName;
            this.WorkspaceState = workspaceState;
        }
    }
}