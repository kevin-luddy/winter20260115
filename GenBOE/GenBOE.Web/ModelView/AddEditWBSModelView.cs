// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.ModelView
{
	using System;
	using GenBOE.ActionLogic.ModelView;

	/// <summary>
	/// Payload model for Add/Edit WBS POST request
	/// </summary>
	[Serializable]
	public class AddEditWBSModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public AddEditWBSModelView()
		{
			this.workspaceShortName = String.Empty;
			this.wbs = null;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="workspaceShortName"></param>
		/// <param name="clin"></param>
		public AddEditWBSModelView(string workspaceShortName, ManageWBSModelView wbs)
		{
			this.workspaceShortName = workspaceShortName;
			this.wbs = wbs;
		}

		/// <summary>
		/// workspace shortname
		/// </summary>
		public string workspaceShortName { get; set; }

		/// <summary>
		/// The WBS being saved
		/// </summary>
		public ManageWBSModelView wbs { get; set; }

	}
}