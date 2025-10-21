// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.ModelView;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	/// <summary>
	/// Wrapper for finalizing the BOE import
	/// </summary>
	[Serializable]
	public class ImportBOEModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public ImportBOEModelView()
		{
			this.workspaceShortName = string.Empty;
			this.importedBOEs = new List<ImportBoeResultsModelView>();
		}

		/// <summary>
		/// Workspace shortname
		/// </summary>
		public string workspaceShortName { get; set; }

		/// <summary>
		/// All of the imported BOE results
		/// </summary>
		public ICollection<ImportBoeResultsModelView> importedBOEs { get; set; }
	}
}