namespace GenBOE.Web.ModelView
{
	using System;
	using System.Collections.Generic;
	using IES.Common;

	/// <summary>
	/// Payload model for Edit Permissions POST request
	/// </summary>
	[Serializable]
	public class EditPermissionsModelView
	{
		/// <summary>
		/// Ctor
		/// </summary>
		public EditPermissionsModelView()
		{
			this.workspaceShortName = String.Empty;
			this.entityId = -1;
			this.roles = null;
			this.entityType = 0;
		}

		/// <summary>
		/// workspace short name
		/// </summary>
		public string workspaceShortName { get; set; }

		/// <summary>
		/// Roles being assigned
		/// </summary>
		public ICollection<Role> roles { get; set; }

		/// <summary>
		/// Entity Type 
		/// </summary>
		public EntityType entityType { get; set; }

		/// <summary>
		/// Entity Id
		/// </summary>
		public int entityId { get; set; }
	}
}