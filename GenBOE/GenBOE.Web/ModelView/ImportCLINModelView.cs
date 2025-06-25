// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	using System;
	using System.Collections.Generic;
	using GenBOE.ActionLogic.IO.Import;

	/// <summary>
	/// Payload model for Complete Import CLIN
	/// </summary>
	[Serializable]
	public class ImportCLINModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public ImportCLINModelView() 
		{
			this.workspaceShortName = string.Empty;
			this.importedCLINs = new List<ImportedClin>();
		}

		/// <summary>
		/// workspace short name
		/// </summary>
		public string workspaceShortName { get; set; }

		/// <summary>
		/// All of the imported CLIN rfesults
		/// </summary>
		public ICollection<ImportedClin> importedCLINs { get; set; }
	}
}