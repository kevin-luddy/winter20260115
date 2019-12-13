// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.ObjectModel;

namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// A collection of TaskElementOrders that come in from the UI. 
    /// </summary>
    public class TaskElementOrderCollection
    {
        /// <summary>
        /// Collection of TaskElements that contain orders.
        /// </summary>
        public Collection<TaskElementOrder> BOETaskElements { get; set; }
    }
}
