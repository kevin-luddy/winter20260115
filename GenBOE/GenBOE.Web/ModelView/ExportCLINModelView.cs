namespace GenBOE.Web.ModelView
{
	using System;

	/// <summary>
	/// Payload model for Export CLIN POST request
	/// </summary>
	[Serializable]
	public class ExportCLINModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public ExportCLINModelView() 
		{
			this.workspaceShortName = String.Empty;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="workspaceShortName"></param>
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