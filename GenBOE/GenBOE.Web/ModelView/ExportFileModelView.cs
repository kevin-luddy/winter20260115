// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.Web.ModelView
{
	using System;

	/// <summary>
	/// Payload model for Export File POST request
	/// </summary>
	[Serializable]
	public class ExportFileModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public ExportFileModelView()
		{
			this.workspaceShortName = String.Empty;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="workspaceShortName"></param>
		public ExportFileModelView(string workspaceShortName)
		{
			this.workspaceShortName = workspaceShortName;
		}

		/// <summary>
		/// workspace shortname
		/// </summary>
		public string workspaceShortName { get; set; }

		/// <summary>
		/// Should the downloadable template be a blank copy?
		/// </summary>
		public bool isBlankTemplate { get; set; } = false;
	}
}