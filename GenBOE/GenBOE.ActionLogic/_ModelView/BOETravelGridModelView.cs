// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    public class BOETravelGridModelView
    {
        public string DisplayEvent { get; set; }

        /// <summary>
        /// The value of the order in which the task will appear in the boe listing
        /// </summary>
        public int BOETaskElementOrder { get; set; }

        /// <summary>
        /// Unique Travel ID
        /// </summary>
        public int TravelID { get; set; }
        /// <summary>
        /// User-entered Task ID
        /// </summary>
        public string TaskID { get; set; }
        /// <summary>
        /// Task Title
        /// </summary>
        public string TaskTitle { get; set; }
        /// <summary>
        /// Date Task starts
        /// </summary>
        public string TaskStartDate { get; set; }
        /// <summary>
        /// Date Task ends
        /// </summary>
        public string TaskEndDate { get; set; }
        /// <summary>
        /// Total Cost of the Task
        /// </summary>
        public string TotalCost { get; set; }

    }
}
