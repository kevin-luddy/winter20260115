// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.ModelView
{
	using System;
	using GenBOE.ActionLogic.ModelView.Clin;

	/// <summary>
	/// Pyaload model for Add/Edit CLIN POST request
	/// </summary>
	[Serializable]
	public class AddEditCLINModelView
	{
		public AddEditCLINModelView() 
		{
			this.workspaceShortName = String.Empty;
			this.clin = null;
		}

		public AddEditCLINModelView(string workspaceShortName, ManageCLINModelView clin)
		{
			this.workspaceShortName = workspaceShortName;
			this.clin = clin;
		}

		/// <summary>
		/// workspace shortname
		/// </summary>
		public string workspaceShortName { get; set; }

		/// <summary>
		/// The CLIN being saved
		/// </summary>
		public ManageCLINModelView clin { get; set; }

	}
}