// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Admin
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    /// <summary>
    /// Bulk Archive view
    /// </summary>
    public class BulkArchiveModelView
    {
        /// <summary>
        /// Constructor for BulkArchiveModelView
        /// </summary>
        public BulkArchiveModelView()
        {
            this.LineOfBusinessOptions = new List<SelectListItem>();
            this.ProgramAreaOptions = new List<SelectListItem>();
        }

        /// <summary>
        /// Start Date
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// End Date
        /// </summary>
        public string EndDate { get; set; }
        
        /// <summary>
        /// Line Of Business
        /// </summary>
        public string LineOfBusiness { get; set; }

        /// <summary>
        /// Line Of Business select Options
        /// </summary>
        public ICollection<SelectListItem> LineOfBusinessOptions { get; set; }

        /// <summary>
        /// Program Area
        /// </summary>
        public string ProgramArea { get; set; }

        /// <summary>
        /// Program Area Options
        /// </summary>
        public ICollection<SelectListItem> ProgramAreaOptions { get; set; }
    }
}
