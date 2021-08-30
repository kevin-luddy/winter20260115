// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace GenBOE.ActionLogic.ModelView.SSRS
{
    public class ProjectWbsCostSummaryByClinModelView
    {
        /// <summary>
        /// The Workspace Name.
        /// </summary>
        public string WorkspaceName { get; set; }

        /// <summary>
        /// The WBS Code.
        /// </summary>
        public string Wbs { get; set; }

        /// <summary>
        /// The WBS Description.
        /// </summary>
        public string WbsTitle { get; set; }

        /// <summary>
        /// The CLIN.
        /// </summary>
        public string Clin { get; set; }

        /// <summary>
        /// The Activity ID.
        /// </summary>
        public string ActivityID { get; set; }

        /// <summary>
        /// The Activity Name.
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// The Resource.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// The Resource Name.
        /// </summary>
        public string CostCenter { get; set; }

        /// <summary>
        /// The Start Date.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The End Date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The Labor Hours.
        /// </summary>
        public decimal? LaborHrs { get; set; }

        /// <summary>
        /// The Material Cost.
        /// </summary>
        public decimal? MatlCost { get; set; }
    }
}
