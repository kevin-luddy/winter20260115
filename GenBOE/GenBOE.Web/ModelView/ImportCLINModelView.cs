namespace GenBOE.Web.ModelView
{
	using System;
	using System.IO;

	/// <summary>
	/// POST Payload for Import CLINs
	/// </summary>
	[Serializable]
	public class ImportCLINModelView
	{
		public ImportCLINModelView() { }

		/// <summary>
		/// Workspace ShortName
		/// </summary>
		public string workspaceShortName { get; set; }

		public Stream file { get; set; }
	}
}