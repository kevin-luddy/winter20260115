// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Backend
{
	using System.Collections.Generic;

	/// <summary>
	/// BOE Configuration Class (data returned from GenBOE's Web.config going to the new GenBOE UI)
	/// 
	/// THIS MUST MATCH THE BOE SERVICE, DO NOT CHANGE UNLESS CHANGES ARE MADE TO BOTH CLASSES.
	/// </summary>
	public class PermissionViewModel
	{
		/// <summary>
		/// Permissions
		/// </summary>
		public ICollection<PermissionsGridViewModel> Permissions { get; set; }

		/// <summary>
		/// Current User ID
		/// </summary>
		public int CurrentUserId { get; set; }

		/// <summary>
		/// Current User Display Name
		/// </summary>
		public string CurrentUserDisplayName { get; set; }
	}
}
