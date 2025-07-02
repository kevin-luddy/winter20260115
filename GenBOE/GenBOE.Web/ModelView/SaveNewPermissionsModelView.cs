namespace GenBOE.Web.ModelView
{
	using IES.Common;
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Payload model for Save New Permissions POST requst
	/// </summary>
	[Serializable]
	public class SaveNewPermissionsModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public SaveNewPermissionsModelView()
		{
			this.workspaceShortName = String.Empty;
			this.entityIds = String.Empty;
			this.roles = null;
		}

		/// <summary>
		/// workspace short name
		/// </summary>
		public string workspaceShortName { get; set; }

		/// <summary>
		/// id or role as a single string value
		/// </summary>
		public string entityIds { get; set; }

		/// <summary>
		/// Roles being assigned
		/// </summary>
		public ICollection<Role> roles { get; set; }
	}
}