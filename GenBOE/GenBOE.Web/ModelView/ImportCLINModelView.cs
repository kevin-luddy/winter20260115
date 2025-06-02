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

		public string workspaceShortName { get; set; }

		public ICollection<ImportedClin> importedCLINs { get; set; }
	}
}