// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Reports
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Web.Mvc;

    /// <summary>
    /// Modelview for the DFARS Report
    /// </summary>
    public class DfarsReportModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DfarsReportModelView()
        {
            this.Lobs = new Collection<int>() { };
            this.LobList = new Collection<SelectListItem>() { };
            this.ProgramAreas = new Collection<int>() { };
            this.ProgramAreaList = new Collection<SelectListItem>() { };
        }

        /// <summary>
        /// get/set list of LOBs
        /// </summary>
        public ICollection<SelectListItem> LobList { get; set; }

        /// <summary>
        /// get/set selected LOBs
        /// </summary>
        public ICollection<int> Lobs { get; set; }

        /// <summary>
        /// get/set list of Program Areas
        /// </summary>
        public ICollection<SelectListItem> ProgramAreaList { get; set; }

        /// <summary>
        /// get/set selected Program Areas
        /// </summary>
        public ICollection<int> ProgramAreas { get; set; }
                
        /// <summary>
        /// get/set Start Date
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// get/set End Date
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// execution user ids..the user id (including groups) that the the user is running the report as
        /// </summary>
        public string ExecutionUserIds { get; set; }
    }
}
