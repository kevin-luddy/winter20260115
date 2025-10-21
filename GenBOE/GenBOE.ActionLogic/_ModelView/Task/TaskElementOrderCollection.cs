// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.ActionLogic.ModelView
{
	using System.Collections.ObjectModel;

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
