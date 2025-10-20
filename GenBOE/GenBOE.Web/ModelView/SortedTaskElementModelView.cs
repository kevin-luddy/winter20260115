// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.ModelView
{
	using System;
	using System.Collections.Generic;
	using GenBOE.ActionLogic.ModelView;

	/// <summary>
	/// Payload for Http POST Task Element Sorting
	/// </summary>
	[Serializable]
	public class SortedTaskElementModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public SortedTaskElementModelView() 
		{
			this.workspaceShortName = String.Empty;
			this.boeId = -1;
			this.taskElementOrders = new List<TaskElementOrder>();
		}

		/// <summary>
		/// Ctor
		/// </summary>
		public SortedTaskElementModelView(string workspaceShortName, int boeId, ICollection<TaskElementOrder> taskElementOrders)
		{
			this.workspaceShortName = workspaceShortName;
			this.boeId = boeId;
			this.taskElementOrders = taskElementOrders;
		}

		/// <summary>
		/// Workspace short name
		/// </summary>
		public string workspaceShortName { get; set; }

		/// <summary>
		/// boeId where the Task Element is being sorted
		/// </summary>
		public int boeId { get; set; }

		/// <summary>
		/// order of task elements
		/// </summary>
		public ICollection<TaskElementOrder> taskElementOrders { get; set; }
	}
}