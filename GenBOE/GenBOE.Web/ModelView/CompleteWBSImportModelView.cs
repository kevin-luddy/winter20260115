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
	/// Payload model for complete WBS import POST request
	/// </summary>
	[Serializable]
	public class CompleteWBSImportModelView
	{
		private ICollection<ActionLogic._ModelView.ImportWbsResultsModelView> wbs1;

		/// <summary>
		/// Ctor
		/// </summary>
		public CompleteWBSImportModelView()
		{
			this.workspaceShortName = String.Empty;
			this.dataToSave = new List<ActionLogic._ModelView.ImportWbsResultsModelView>();
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="workspaceShortName"></param>
		/// <param name="dataToSave"></param>
		public CompleteWBSImportModelView(string workspaceShortName, List<ActionLogic._ModelView.ImportWbsResultsModelView> dataToSave)
		{
			this.workspaceShortName = workspaceShortName;
			this.dataToSave = dataToSave;
		}

		/// <summary>
		/// workspace shortname
		/// </summary>
		public string workspaceShortName { get; set; }

		/// <summary>
		/// The WBS to import
		/// </summary>
		public ICollection<ActionLogic._ModelView.ImportWbsResultsModelView> dataToSave { get => wbs1; set => wbs1 = value; }
	}
}