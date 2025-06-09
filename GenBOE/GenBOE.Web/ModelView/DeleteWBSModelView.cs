// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.ModelView
{
	using System;
	using System.Collections.Generic;
	using GenBOE.ActionLogic.ModelView;

	/// <summary>
	/// Payload model for Delete WBS request
	/// </summary>
	[Serializable]
	public class DeleteWBSModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public DeleteWBSModelView()
		{
			this.WorkspaceShortName = String.Empty;
			this.Wbs = new List<ManageWBSModelView>();
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="WorkspaceShortName"></param>
		/// <param name="Wbs"></param>
		public DeleteWBSModelView(string WorkspaceShortName, List<ManageWBSModelView> Wbs)
		{
			this.WorkspaceShortName = WorkspaceShortName;
			this.Wbs = Wbs;
		}

		/// <summary>
		/// workspace shortname
		/// </summary>
		public string WorkspaceShortName { get; set; }

		/// <summary>
		/// The WBS to delete
		/// </summary>
		public List<ManageWBSModelView> Wbs { get; set; }

	}
}