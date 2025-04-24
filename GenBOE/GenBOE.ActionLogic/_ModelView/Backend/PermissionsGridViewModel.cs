// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Backend
{
	using IES.Common;
	using System.Collections.ObjectModel;
	using System.Linq;

	/// <summary>
	/// BOE Configuration Class (data returned from GenBOE's Web.config going to the new GenBOE UI)
	/// 
	/// THIS MUST MATCH THE BOE SERVICE, DO NOT CHANGE UNLESS CHANGES ARE MADE TO BOTH CLASSES.
	/// </summary>
	public class PermissionsGridViewModel
	{
		private Collection<PermissionRoleViewModel> _Roles = new Collection<PermissionRoleViewModel>();

		public PermissionsGridViewModel()
		{
			Users = new Collection<PermissionUserViewModel>();
			_Roles = new Collection<PermissionRoleViewModel>();
			isGroup = false;
			groupMembers = new Collection<string>();
			genBOEAccess = "";
		}

		/// <summary>
		/// For this element, the users belonging to the group
		/// </summary>
		public Collection<PermissionUserViewModel> Users { get; set; }

		/// <summary>
		/// For this element, the roles belonging to the users contained in the element
		/// </summary>
		public Collection<PermissionRoleViewModel> Roles
		{
			get
			{
				if (_Roles != null)
				{
					// we don't ever want to show the user the 'workspace user' role .. 
					// it's too confusing.  the database will manage this role automatically
					// as other roles around it are inserted and deleted.
					return new Collection<PermissionRoleViewModel>(_Roles.Where(x => x.RoleID != (int)Role.WorkspaceUser).Select(y => y).ToArray());
				}
				else
				{
					return null;
				}
			}
			set
			{
				_Roles = value;
			}
		}

		/// <summary>
		/// If this element is a group or not
		/// </summary>
		public bool isGroup { get; set; }

		/// <summary>
		/// If it's a group, this will get the users in the group
		/// </summary>
		public Collection<string> groupMembers { get; set; }

		/// <summary>
		/// User/Group's access to GenBOE - "Yes" or "No" for users, "View Users" for groups
		/// </summary>
		public string genBOEAccess { get; set; }
	}
}
