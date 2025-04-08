// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export.BOE
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;

	public class BOEExcelExportInputs
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="ws">Workspace containing BOEs</param>
        /// <param name="templateFileName">file name of the template used for export</param>
        /// <param name="blankTemplate">Bool to determine if template should be blank or contain all BOEs</param>
        public BOEExcelExportInputs(FullWorkspace ws, string templateFileName, bool blankTemplate, 
			IUserDTODataLoader userDTODataLoader, IActiveDirectoryUtilities activeDirectoryUtilities, 
			IPermissionsDTODataLoader permissionsDTODataLoader) 
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			if (userDTODataLoader == null)
			{
				throw new ArgumentNullException(nameof(userDTODataLoader));
			}

			if (activeDirectoryUtilities == null)
			{
				throw new ArgumentNullException(nameof(activeDirectoryUtilities));
			}

			if (permissionsDTODataLoader == null)
			{
				throw new ArgumentNullException(nameof(permissionsDTODataLoader));
			}

			this.IsBlankTemplate = blankTemplate;
			this.TemplateFile = System.IO.File.ReadAllBytes(templateFileName);
			this.WorkspaceName = ws.WorkspaceName;
			this.Boes = ws.Boes.Select(b => b.ToDTO()).ToList();
			this.Clins = ws.Clins.Select(c => c.ToDTO()).ToList();
			this.WbsElements = ws.WbsElements.Select(x => new WbsDTO(x)).ToList();
			Collection<PermissionsDTO> permissionsForWorkspace = permissionsDTODataLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id);
			HashSet<UserDTO> allUsersWithPotentialPermissions = new HashSet<UserDTO>(userDTODataLoader.GetByIds(permissionsForWorkspace.Select(x => x.ETIUserId).Distinct().ToList()).ToCollection());

			List<int> boeIds = Boes.Select(x => x.Id).ToList();
			this.RolesWithBoes = new HashSet<PermissionsDTO>(permissionsDTODataLoader.GetBOEPermissions(boeIds).Where(x => x.BOEId.HasValue).ToCollection());

			// Get users broken down by roles, for the boes
			this.AllBOEUsers = new HashSet<UserDTO>(userDTODataLoader.GetByIds(this.RolesWithBoes.Select(x => x.ETIUserId).Distinct().ToList()).ToCollection());

			HashSet<int> authorIds = new HashSet<int>(permissionsForWorkspace.Where(x => x.Role == Role.Author).Select(x => x.ETIUserId).ToCollection());
			Collection<UserDTO> allAuthors = allUsersWithPotentialPermissions.Where(u => authorIds.Contains(u.UserID)).OrderBy(x => x.DisplayName).ToCollection<UserDTO>();

			HashSet<int> subcontractorAuthorIds = new HashSet<int>(permissionsForWorkspace.Where(x => x.Role == Role.SubcontractorAuthor).Select(x => x.ETIUserId).ToCollection());
			Collection<UserDTO> allSubcontractorAuthors = allUsersWithPotentialPermissions.Where(x => subcontractorAuthorIds.Contains(x.UserID)).OrderBy(x => x.DisplayName).ToCollection();

			HashSet<int> approverIds = new HashSet<int>(permissionsForWorkspace.Where(x => x.Role == Role.Approver).Select(x => x.ETIUserId).ToCollection());
			Collection<UserDTO> allApprovers = allUsersWithPotentialPermissions.Where(x => approverIds.Contains(x.UserID)).OrderBy(x => x.DisplayName).ToCollection();

			#region Remove AD Groups from users

			Authors = CleanupAdGroupsFromUsers(allAuthors, userDTODataLoader, activeDirectoryUtilities);
			SubcontractorAuthors = CleanupAdGroupsFromUsers(allSubcontractorAuthors, userDTODataLoader, activeDirectoryUtilities);
			Approvers = CleanupAdGroupsFromUsers(allApprovers, userDTODataLoader, activeDirectoryUtilities);

			#endregion 
		}

		/// <summary>
		/// Gets all of the boes in the workspace.
		/// </summary>
		public ICollection<BoeDTO> Boes { get; }

		/// <summary>
		/// Gets the clins.
		/// </summary>
		public ICollection<ClinDTO> Clins { get; set; }

		/// <summary>
		/// Gets the WBS elements.
		/// </summary>
		public ICollection<WbsDTO> WbsElements { get; set; }

		/// <summary>
		/// Authors for BOEs
		/// </summary>
		public ICollection<UserDTO> Authors { get; set; }

		/// <summary>
		/// Subcontractor Authors for BOEs
		/// </summary>
		public ICollection<UserDTO> SubcontractorAuthors { get; set; }

		/// <summary>
		/// Approvers for BOEs
		/// </summary>
		public ICollection<UserDTO> Approvers { get; set; }

		/// <summary>
		/// BOE Permissions
		/// </summary>
		public ICollection<PermissionsDTO> RolesWithBoes { get; set; }

		/// <summary>
		/// BOE Users based off BOE Permissions
		/// </summary>
		public ICollection<UserDTO> AllBOEUsers { get; set; }

		/// <summary>
		/// Should this export to a blank template
		/// </summary>
		public bool IsBlankTemplate { get; set; }

		/// <summary>
		/// The BOE Template File Location
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public byte[] TemplateFile { get; set; }

		/// <summary>
		/// The Workspace Name
		/// </summary>
		public string WorkspaceName { get; set; }

		/// <summary>
		/// Breaks down AD Groups into individual users
		/// </summary>
		/// <param name="incomingUsers">incoming users potentially containing AD groups</param>
		/// <returns>individual users</returns>
		private Collection<UserDTO> CleanupAdGroupsFromUsers(Collection<UserDTO> incomingUsers,
			IUserDTODataLoader userDTODataLoader, IActiveDirectoryUtilities activeDirectoryUtilities)
		{
			List<UserDTO> tempUsers = new List<UserDTO>();

			// all users can contain groups. need to break those down.
			foreach (UserDTO user in incomingUsers)
			{
				if (user.NTID.Contains('.')) // AD group name
				{
					ICollection<UserData> members = activeDirectoryUtilities.GetAdGroupUsers(user.DisplayName);

					ICollection<UserData> orderedMembers = members.OrderBy(m => m.DisplayName).ToList();

					List<int> userIds = new List<int>();

					foreach (UserData member in orderedMembers)
					{
						int userId;
						bool userExists = userDTODataLoader.UserExists(member.Ntid, out userId);

						if (userExists)
						{
							userIds.Add(userId);
						}
					}

					tempUsers.AddRange(userDTODataLoader.GetByIds(userIds));
				}
				else // just a regular user
				{
					tempUsers.Add(user);
				}
			}

			return new Collection<UserDTO>(tempUsers);
		}
	}
}
