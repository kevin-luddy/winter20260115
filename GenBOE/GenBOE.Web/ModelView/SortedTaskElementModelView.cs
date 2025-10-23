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
			this.sortedTaskElements = new List<GenericTaskElementGridRow>();
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="workspaceShortName">Workspace Short name</param>
		/// <param name="boeId">BOE Id the boe labor tasks are being sorted</param>
		/// <param name="sortedTaskElements">list of sorted task elements</param>
		public SortedTaskElementModelView(string workspaceShortName, int boeId, ICollection<GenericTaskElementGridRow> sortedTaskElements)
		{
			this.workspaceShortName = workspaceShortName;
			this.boeId = boeId;
			this.sortedTaskElements = sortedTaskElements;
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
		public ICollection<GenericTaskElementGridRow> sortedTaskElements { get; set; }
	}
}