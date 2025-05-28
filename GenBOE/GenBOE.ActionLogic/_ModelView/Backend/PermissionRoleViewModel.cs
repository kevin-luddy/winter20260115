// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Backend
{
	using IES.Common;

	/// <summary>
	/// BOE Configuration Class (data returned from GenBOE's Web.config going to the new GenBOE UI)
	/// 
	/// THIS MUST MATCH THE BOE SERVICE, DO NOT CHANGE UNLESS CHANGES ARE MADE TO BOTH CLASSES.
	/// </summary>
	public class PermissionRoleViewModel
	{
		public PermissionRoleViewModel()
		{
			RoleID = (int)Role.WorkspaceUser;
			RoleName = string.Empty;
		}

		/// <summary>
		/// Role name
		/// </summary>
		public string RoleName { get; set; }

		/// <summary>
		/// ID of the role
		/// </summary>
		public int RoleID { get; set; }
	}
}
