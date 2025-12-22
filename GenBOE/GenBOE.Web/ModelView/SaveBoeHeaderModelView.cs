// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.ModelView
{
	using System;
	using GenBOE.ActionLogic._ModelView.Backend;

	/// <summary>
	/// Payload for Http POST SaveEditBoeHeader 
	/// </summary>
	[Serializable]
	public class SaveBoeHeaderModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public SaveBoeHeaderModelView()
		{
			this.workspaceShortName = String.Empty;
			this.boeHeader = new BOEHeaderViewModel();
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="workspaceShortName">Workspace Short name</param>
		/// <param name="sortedTaskElements">Boe header data</param>
		public SaveBoeHeaderModelView(string workspaceShortName, BOEHeaderViewModel boeHeader)
		{
			this.workspaceShortName = workspaceShortName;
			this.boeHeader = boeHeader;
		}

		/// <summary>
		/// Workspace short name
		/// </summary>
		public string workspaceShortName { get; set; }

		/// <summary>
		/// Boe header data
		/// </summary>
		public BOEHeaderViewModel boeHeader { get; set; }
	}
}