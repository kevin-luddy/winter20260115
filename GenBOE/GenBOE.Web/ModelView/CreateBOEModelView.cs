// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.ModelView
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Payload model for creating BOEs for WBS
	/// </summary>
	[Serializable]
	public class CreateBOEModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public CreateBOEModelView()
		{
			this.WorkspaceShortName = String.Empty;
			this.WbsIDs = new List<int>();
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="WorkspaceShortName"></param>
		/// <param name="Wbs IDs"></param>
		public CreateBOEModelView(string WorkspaceShortName, List<int> WbsIDs)
		{
			this.WorkspaceShortName = WorkspaceShortName;
			this.WbsIDs = WbsIDs;
		}

		/// <summary>
		/// workspace shortname
		/// </summary>
		public string WorkspaceShortName { get; set; }

		/// <summary>
		/// The WBS IDs to create BOE for
		/// </summary>
		public List<int> WbsIDs { get; set; }
	}
}