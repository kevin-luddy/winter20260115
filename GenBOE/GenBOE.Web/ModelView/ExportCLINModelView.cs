namespace GenBOE.Web.ModelView
{
	using System;

	[Serializable]
	public class ExportCLINModelView
	{
		public ExportCLINModelView() 
		{
			this.workspaceShortName = String.Empty;
		}

		public ExportCLINModelView(string workspaceShortName)
		{
			this.workspaceShortName = workspaceShortName;
		}

		/// <summary>
		/// workspace shortname
		/// </summary>
		public string workspaceShortName { get; set; }
	}
}