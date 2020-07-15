// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// Class that collects Labor Type IDs and Orders
    /// </summary>
    public class LaborTypeOrder
    {
        /// <summary>
        /// ID related to each Labor Type
        /// </summary>
        public int LaborTypeID { get; set; }

        /// <summary>
        /// The Order of the Labor Type in the Task Element
        /// </summary>
        public int ListOrder { get; set; }
    }
}
