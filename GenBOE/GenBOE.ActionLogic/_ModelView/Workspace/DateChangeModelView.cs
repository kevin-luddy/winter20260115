using System;
using System.Collections.Generic;
using System.Linq;

namespace GenBOE.ActionLogic.ModelView.Workspace
{
    /// <summary>
    /// Date change modelview to be extended
    /// </summary>
    public class DateChangeModelView
    {
        /// <summary>
        /// Previous start date
        /// </summary>
        public string OldStartDate { get; set; }

        /// <summary>
        /// Previous end date
        /// </summary>
        public string OldEndDate { get; set; }

        /// <summary>
        /// New start date
        /// </summary>
        public string NewStartDate { get; set; }

        /// <summary>
        /// New end date
        /// </summary>
        public string NewEndDate { get; set; }
    }
}