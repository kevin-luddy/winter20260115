namespace GenBOE.Web.ModelView
{
	using System;

	/// <summary>
	/// Payload model for Export CLIN POST request
	/// </summary>
	[Serializable]
	public class ExportPermissionModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public ExportPermissionModelView()
		{
			this.workspaceShortName = String.Empty;
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="workspaceShortName"></param>
		public ExportPermissionModelView(string workspaceShortName)
		{
			this.workspaceShortName = workspaceShortName;
		}

		/// <summary>
		/// workspace shortname
		/// </summary>
		public string workspaceShortName { get; set; }
	}
}