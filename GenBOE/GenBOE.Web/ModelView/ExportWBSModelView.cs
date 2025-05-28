namespace GenBOE.Web.ModelView
{
	using System;

	/// <summary>
	/// Payload model for Export WBS POST request
	/// </summary>
	[Serializable]
	public class ExportWBSModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public ExportWBSModelView() 
		{
			this.workspaceShortName = String.Empty;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="workspaceShortName"></param>
		public ExportWBSModelView(string workspaceShortName)
		{
			this.workspaceShortName = workspaceShortName;
		}

		/// <summary>
		/// workspace shortname
		/// </summary>
		public string workspaceShortName { get; set; }
	}
}