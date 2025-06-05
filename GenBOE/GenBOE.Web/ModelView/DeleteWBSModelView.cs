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
			this.workspaceShortName = String.Empty;
			this.wbs = new List<ManageWBSModelView>();
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="workspaceShortName"></param>
		/// <param name="clin"></param>
		public DeleteWBSModelView(string workspaceShortName, List<ManageWBSModelView> wbs)
		{
			this.workspaceShortName = workspaceShortName;
			this.wbs = wbs;
		}

		/// <summary>
		/// workspace shortname
		/// </summary>
		public string workspaceShortName { get; set; }

		/// <summary>
		/// The WBS to delete
		/// </summary>
		public List<ManageWBSModelView> wbs { get; set; }

	}
}