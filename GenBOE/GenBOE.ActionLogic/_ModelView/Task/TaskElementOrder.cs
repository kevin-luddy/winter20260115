// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// Class that collects TaskElement's ID's and Orders
    /// </summary>
    public class TaskElementOrder
    {
        /// <summary>
        /// ID related to each TaskElement
        /// </summary>
        public int TaskID { get; set; }
        /// <summary>
        /// The Order of the TaskElement in the BOE
        /// </summary>
        public int ListOrder { get; set; }

    }
}
