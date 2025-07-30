// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.ActionLogic.ControllerLogic.Backend
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Web.Mvc;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.ActionLogic.ModelView.Clin;
	using GenBOE.ActionLogic.WBS;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using IES.Common.PickList;

	/// <summary>
	/// Workspace Admin Controller Logic
	/// </summary>
	public class WorkspaceAdminControllerLogic
	{
		/// <summary>
		/// Full object factory
		/// </summary>
		private readonly IFullObjectFactory factory;

		/// <summary>
		/// Common Data Loader
		/// </summary>
		private readonly ICommonDataLoader commonDataLoader;

		/// <summary>
		/// wbs Loader
		/// </summary>
		private readonly IWbsDTODataLoader wbsLoader;

		/// <summary>
		/// User Loader
		/// </summary>
		private readonly IUserDTODataLoader userLoader;

		/// <summary>
		/// nested wbs utilities
		/// </summary>
		private readonly NestedWBSUtilities nestedWbsUtilities;

		/// <summary>
		/// permissions Loader
		/// </summary>
		private readonly IPermissionsDTODataLoader permissionsLoader;

		/// <summary>
		/// security access
		/// </summary>
		private readonly ISecurityAccess securityAccess;

		/// <summary>
		/// AD utilities
		/// </summary>
		private readonly IActiveDirectoryUtilities adUtils;

		/// <summary>
		/// Zone travel rates fees loader
		/// </summary>
		private readonly RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader;

		public WorkspaceAdminControllerLogic(ICommonDataLoader commonDataLoader, NestedWBSUtilities nestedWbsUtilities, IWbsDTODataLoader wbsLoader, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, ISecurityAccess securityAccess, IActiveDirectoryUtilities adUtils, RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader)
		{
			this.commonDataLoader = commonDataLoader;
			this.nestedWbsUtilities = nestedWbsUtilities;
			this.wbsLoader = wbsLoader;
			this.factory = factory;
			this.userLoader = userLoader;
			this.permissionsLoader = permissionsLoader;
			this.securityAccess = securityAccess;
			this.adUtils = adUtils;
			this.zoneTravelRatesFeesLoader = zoneTravelRatesFeesLoader;
		}

		/// <summary>
		/// Builds sorted SelectList with only Contract Types selected at the workspace level and default option
		/// </summary>
		/// <param name="workspaceId"></param>
		/// <returns>sorted SelectList with workspace Contract Types only</returns>
		public ICollection<PickListDto> BuildContractTypeDropdownOptions(int workspaceId)
		{
			//Contract Type dropdown includes workspace contract types only
			List<PickListDto> selectedContractTypesList = this.commonDataLoader.GetSelectedContractTypes(workspaceId)
				.OrderBy(ct => ct.Text)
				.ToList();
			//Add 'Not Set' option for CLINs without contract type
			selectedContractTypesList.Insert(0, new PickListDto { Id = Constants.CONTRACT_TYPE_NOT_SET, Text = Constants.CONTRACT_TYPE_NOT_SET_STRING });
			return selectedContractTypesList;
		}

		/// <summary>
		/// Get the WBS Grid MV
		/// </summary>
		/// <param name="workspace">the workspace</param>
		/// <returns>The MV for the Manage WBS grid</returns>
		public ICollection<ManageWBSModelView> GetManageWBSModel(FullWorkspace workspace)
		{
			if(workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			// convert the collection of WBS DTO to a collection of ManageWBSModelView
			Collection<WbsDTO> wbsDTOs = workspace.WbsElementsNoMultiWbs.ToCollection<WbsDTO>();

			Collection<ManageWBSModelView> result = new Collection<ManageWBSModelView>();

			nestedWbsUtilities.AdjustLevels(wbsDTOs);

			Collection<int> wbsids = GetWBSUsedByWorkspaceVariables(workspace);
			ICollection<int> wbsInBoes = GetWbsUsedInBoes(workspace);

			foreach (WbsDTO wbs in wbsDTOs)
			{
				Collection<ClinDTO> clins = workspace.Clins.Where(x => wbs.ClinIDs.Contains(x.Id)).ToCollection<ClinDTO>();

				ManageWBSModelView wbsModelView = new ManageWBSModelView(wbs, clins);

				if (wbsids.Contains(wbs.Id))
				{
					wbsModelView.InUse = true;
				}

				if (wbsInBoes.Contains(wbs.Id))
				{
					wbsModelView.HasBOE = true;
				}

				wbsModelView.ParentOrChildHasBoe = this.ParentOrChildHasBoe(wbs, workspace);

				result.Add(wbsModelView);
			}

			return result;
        }

        /// <summary>
        /// Get IDs of WBSs that are used by workspace variables
        /// </summary>
        /// <param name="ws">The Workspace</param>
        /// <returns>The IDs of WBSs used by workspace variables</returns>
        private Collection<int> GetWBSUsedByWorkspaceVariables(FullWorkspace workspace)
		{
			IReadOnlyCollection<WorkspaceVariableDTO> workspaceVariables = workspace.WorkspaceVariables;
			Collection<int> wbsids = new Collection<int>();

			foreach (WorkspaceVariableDTO workspaceVariable in workspaceVariables)
			{
				foreach (SelectBOEsToSum SelectedBOEsToSum in workspaceVariable.SelectedBOEsToSum)
				{
					if (SelectedBOEsToSum.WBSID != null && !wbsids.Contains(SelectedBOEsToSum.WBSID.Value))
					{
						wbsids.Add(SelectedBOEsToSum.WBSID.Value);
					}
				}
			}

			return wbsids;
		}

		/// <summary>
		/// Get the WBSs that are used by BOEs
		/// </summary>
		/// <param name="ws">The Workspace</param>
		/// <returns>The IDs of WBSs used by BOEs</returns>
		private ICollection<int> GetWbsUsedInBoes(FullWorkspace ws)
		{
			ICollection<int> toReturn = new Collection<int>();
			foreach (FullBoe boe in ws.Boes)
			{
				if (boe.WBSID != null)
				{
					toReturn.Add((int)boe.WBSID);
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Check if a parent WBS has a BOE
		/// </summary>
		/// <param name="wbs">wbs to check</param>
		/// <param name="ws">ws containing wbs</param>
		/// <returns>True if parent WBS has a BOE, otherwise false</returns>
		private bool ParentOrChildHasBoe(WbsDTO wbs, FullWorkspace ws)
		{
			ICollection<string> parentWbs = nestedWbsUtilities.GetParentsWBSNumByChildWBS(wbs);
			foreach (string parentNumber in parentWbs)
			{
				WbsDTO parentDto = ws.WbsElements.FirstOrDefault(x => x.WbsNumber == parentNumber);
				if (parentDto != null && parentDto.inUse)
				{
					return true;
				}
			}

			ICollection<WbsDTO> childWbs = wbsLoader.GetAllChildWbs(ws.Id, wbs.WbsNumber);
			foreach (WbsDTO childDto in childWbs)
			{
				if (childDto.inUse)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Check permissions and return the authorization of the user
		/// </summary>
		/// <param name="inPage">The page to check</param>
		/// <param name="inWorkspace">The workspace id (optional, null if not required)</param>
		/// <param name="inBOEId">The BOE ID (optional, null if not required)</param>
		/// <returns>The view to redirect to if security error, NULL otherwise (i.e. NULL indicates the user is allowed to proceed</returns>
		public SecurityAuthorization CheckPermissions(SecurityPage inPage, WorkspaceDTO workspace, int? inBOEId)
		{
			Dictionary<SecurityPage, SecurityAuthorization> securityPermission = CheckPermissions(new Collection<SecurityPage>() { inPage }, workspace, inBOEId);

			// NOTE: If you want to turn security "off" uncomment the next line.  everyone accessing the system will get CRUD access...
			// authorizationForUser = SecurityAuthorization.CreateReadUpdateDelete;

			return securityPermission.Values.First();
		}

		/// <summary>
		/// Check permissions and return the authorization of the user
		/// </summary>
		/// <param name="inPage">The page to check</param>
		/// <param name="inWorkspace">The workspace id (optional, null if not required)</param>
		/// <param name="inBOEId">The BOE ID (optional, null if not required)</param>
		/// <returns>The view to redirect to if security error, NULL otherwise (i.e. NULL indicates the user is allowed to proceed</returns>
		protected Dictionary<SecurityPage, SecurityAuthorization> CheckPermissions(Collection<SecurityPage> inPages, WorkspaceDTO workspace, int? inBOEId)
		{
			if (inPages == null)
			{
				throw new ArgumentNullException(nameof(inPages));
			}
			if (inBOEId.HasValue)
			{
				if (workspace == null) { throw new ArgumentNullException(nameof(workspace), "If BOEId is specified, workspace must be specified as well"); }
				if (!factory.BoeLoader.DoesWorkspaceContainBoe(workspace.Id, inBOEId.Value))
				{ throw new InvalidDataRelationException("The requested BOE: " + inBOEId.Value + " does not belong to the current workspace: " + workspace.Id + "."); }
			}

			Dictionary<SecurityPage, SecurityAuthorization> securityDictionary = new Dictionary<SecurityPage, SecurityAuthorization>();
			int? wsId = workspace == null ? null : (int?)workspace.Id;

			UserDTO user = this.userLoader.GetUserForActiveUser();
			string overrideNonUsString = ConfigurationUtilities.GetAppSetting("OverrideSubNonUs");
			bool overrideNonUs = string.IsNullOrEmpty(overrideNonUsString) ? false : overrideNonUsString.ToLower() == "true";
			bool? isUsPerson = overrideNonUs ? true : user.IsUsPerson;

			if (isUsPerson == null)
			{
				throw new ValidationException("IsUsPerson cannot be null");
			}

			IReadOnlyCollection<SecurityPermissionsResponse> rolesForUser = factory.GetPermissionsForUser(user.NTID);

			foreach (SecurityPage page in inPages)
			{
				SecurityAuthorization authorizationForUser;

				if ((bool)isUsPerson)
				{
					authorizationForUser = securityAccess.IsAuthorized(
						new SecurityPermissionsRequested { PageToCheck = page, WorkspaceId = wsId, BOEId = inBOEId }, workspace, rolesForUser);
				}
				else
				{
					throw new UnauthorizedAccessException("Access is denied for non-US users.");
				}

				securityDictionary.Add(page, authorizationForUser);
			}

			return securityDictionary;
		}

		/// <summary>
		/// Gets company specific header information for the manage Boe page.
		/// </summary>
		/// <returns>Header Info for Manage Boe</returns>
		public virtual string GetCompanySpecificManageBoeHeaderInfo
		{
			get
			{
				return CommonConstants.MANAGE_BOE_HEADER_ISGS;
			}
		}

		/// <summary>
		/// Calculates the defaults for the Manage BOE page
		/// </summary>
		/// <param name="theModelView">modelview of the Manage BOE Grid</param>
		/// <param name="workspace">workspace containing Mamage BOE page</param>
		public void CalculateManageBOEDefaults(ManageBOEGridWidgetModelView theModelView, FullWorkspace workspace)
		{
			if (theModelView == null)
			{
				throw new ArgumentNullException(nameof(theModelView));
			}
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}
			// WBS Names
			Collection<WbsDTO> allWbsForWorkspace = workspace.WbsElementsNoMultiWbs.ToCollection<WbsDTO>();

			List<WbsDTO> availableWbs = allWbsForWorkspace.Where(x => x.inUse).ToList();
			this.nestedWbsUtilities.SetParentsAndChildrenInUse(allWbsForWorkspace);

			List<WbsDTO> otherAvailbleWBS = allWbsForWorkspace.Where(x => !x.inUse).ToList();

			availableWbs.AddRange(otherAvailbleWBS);

			// CLIN Names
			Collection<ManageCLINModelView> clinNames = new Collection<ManageCLINModelView>(
				(from c in workspace.ClinsNoMultiClin
				 orderby c.ClinNumber
				 select new ManageCLINModelView
				 {
					 ClinNumber = c.ClinNumber,
					 ClinTitle = c.ClinTitle,
					 ClinID = c.Id,
					 StartDate = c.StartDate.HasValue ? c.StartDate.Value.ToString("MM/yyyy") : workspace.ContractStartDate.ToString("MM/yyyy"),
					 EndDate = c.EndDate.HasValue ? c.EndDate.Value.ToString("MM/yyyy") : workspace.ContractEndDate.ToString("MM/yyyy")
				 }).ToList());

			Collection<PermissionsDTO> potentialBOEPermissions = permissionsLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id);
			// Approver Names
			IEnumerable<int> approverIDs = (from p in potentialBOEPermissions
											where p.Role == Role.Approver
											select p.ETIUserId).Distinct();
			Collection<SelectListItem> approvers = this.CreateUserSelectList(approverIDs.ToCollection(), false);

			// Author Names
			IEnumerable<int> authorIDs = (from p in potentialBOEPermissions
										  where p.Role == Role.Author
										  select p.ETIUserId).Distinct();
			Collection<SelectListItem> authors = this.CreateUserSelectList(authorIDs.ToCollection(), false);

			// Subcontractor Author Names
			IEnumerable<int> subcontractorAuthorIDs = (from p in potentialBOEPermissions
													   where p.Role == Role.SubcontractorAuthor
													   select p.ETIUserId).Distinct();
			Collection<SelectListItem> subcontractorAuthors = this.CreateUserSelectList(subcontractorAuthorIDs.ToCollection(), true);

			theModelView.DefaultStartDate = workspace.ContractStartDate.ToString("MM/yyyy");
			theModelView.DefaultEndDate = workspace.ContractEndDate.ToString("MM/yyyy");

			theModelView.PotentialWbs = new Collection<SelectListItem>((from x in availableWbs
																		orderby x.WbsPaddedNumber
																		select new SelectListItem()
																		{
																			Text = x.WbsString,
																			Value = x.Id.ToString()
																		}).ToList());

			theModelView.PotentialWbs.Insert(0, new SelectListItem() { Selected = true, Text = string.Empty, Value = "-1" });



			//Lets get the Multi WBS for this workspace. 
			theModelView.MultiWBSId = workspace.MultiBOEWbs.Id;
			theModelView.MultiWBSName = workspace.MultiBOEWbs.WbsString;


			//Lets get the Multi Clin for this workspace. 
			theModelView.MultiClin = (new ManageCLINModelView(workspace.MultiBOEClin, null));

			theModelView.PotentialClins = clinNames;
			theModelView.PotentialAuthors = new Collection<SelectListItem>(authors.OrderBy(x => x.Text).ToArray());
			theModelView.PotentialSubcontractorAuthors = new Collection<SelectListItem>(subcontractorAuthors.OrderBy(x => x.Text).ToArray());
			theModelView.PotentialApprovers = new Collection<SelectListItem>(approvers.OrderBy(x => x.Text).ToArray());

			theModelView.BoeResults = this.GetManageBOEGridData(workspace, workspace.Boes);
		}


		/// <summary>
		/// Gets the model view BOE properties for the Manage BOE Grid.
		/// </summary>
		/// <param name="ws">Workspace containing the BOEs</param>
		/// <param name="boes">The Boes to get data for</param>
		public ICollection<ManageBOEModelView> GetManageBOEGridData(FullWorkspace ws, IReadOnlyCollection<FullBoe> boes)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			if (boes == null)
			{
				throw new ArgumentNullException(nameof(boes));
			}

			ICollection<ManageBOEModelView> modelViews = new List<ManageBOEModelView>();

			#region Get the data

			// Get roles for the boes
			List<int> boeIds = boes.Select(x => x.Id).ToList();

			HashSet<PermissionsDTO> rolesWithBoes = new HashSet<PermissionsDTO>(permissionsLoader.GetBOEPermissions(boeIds).Where(x => x.BOEId.HasValue).ToCollection());

			// Get users broken down by roles, for the boes
			HashSet<UserDTO> allBoeUsers = new HashSet<UserDTO>(userLoader.GetByIds(rolesWithBoes.Select(x => x.ETIUserId).Distinct().ToList()).ToCollection());

			Dictionary<int, Collection<UserDTO>> boeAuthors = this.GetBoeUsersByRole(rolesWithBoes, allBoeUsers, Role.Author);
			Dictionary<int, Collection<UserDTO>> boeSubcontractorAuthors = this.GetBoeUsersByRole(rolesWithBoes, allBoeUsers, Role.SubcontractorAuthor);
			Dictionary<int, Collection<UserDTO>> boeApprovers = this.GetBoeUsersByRole(rolesWithBoes, allBoeUsers, Role.Approver);

			// Get potential WS users
			Collection<PermissionsDTO> wsPotentialPermissions = permissionsLoader.GetBOEPotentialPermissionsForWorkspace(ws.Id);

			Collection<UserDTO> potentialWsUsers = userLoader.GetByIds(
				wsPotentialPermissions.Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor || x.Role == Role.Approver).Select(x => x.ETIUserId).Distinct().ToList()
				).ToCollection();

			// add group users..
			List<UserDTO> tempUsers = new List<UserDTO>();
			foreach (UserDTO user in potentialWsUsers)
			{
				if (user.NTID.Contains('.')) // AD group name
				{
					ICollection<UserData> members = this.adUtils.GetAdGroupUsers(user.DisplayName);

					ICollection<UserData> orderedMembers = members.OrderBy(m => m.DisplayName).ToList();
					int userId;
					List<int> userIds = new List<int>();

					foreach (UserData member in orderedMembers)
					{
						bool userExists = this.userLoader.UserExists(member.Ntid, out userId);

						if (userExists)
						{
							if (potentialWsUsers.All(x => x.UserID != userId))
							{
								userIds.Add(userId);
							}
						}
					}

					tempUsers.AddRange(this.userLoader.GetByIds(userIds));
				}
			}

			tempUsers.AddRange(potentialWsUsers.ToList());
			potentialWsUsers = new Collection<UserDTO>(tempUsers);

			#endregion

			// Only get escalation rates and fees once since they exist at the Workspace level.
			ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates = this.zoneTravelRatesFeesLoader.getAllEscalationRatesByWorkspace(ws.Id);
			Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = this.zoneTravelRatesFeesLoader.getAllFeesAndCostsByWorkspace(ws.Id).ToDictionary(f => f.ModeID);

			foreach (FullBoe boe in boes)
			{
				WbsDTO wbsDto = ws.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
				ClinDTO clinDto = ws.Clins.FirstOrDefault(x => x.Id == boe.CLINID);

				//loop through BOE Authors and Subcontractor Authors to build these lists
				Collection<UserDTO> authorsAsUser;
				if (!boeAuthors.TryGetValue(boe.Id, out authorsAsUser))
				{
					authorsAsUser = new Collection<UserDTO>();
				}

				Collection<UserDTO> subcontractorAuthorsAsUser;
				if (!boeSubcontractorAuthors.TryGetValue(boe.Id, out subcontractorAuthorsAsUser))
				{
					subcontractorAuthorsAsUser = new Collection<UserDTO>();
				}

				Collection<UserDTO> approversAsUser;
				if (!boeApprovers.TryGetValue(boe.Id, out approversAsUser))
				{
					approversAsUser = new Collection<UserDTO>();
				}

				Collection<UserDTO> users = potentialWsUsers;

				PermissionsDTO[] authors = rolesWithBoes.Where(x => x.BOEId == boe.Id).Where(x => x.Role == Role.Author).Select(x => x).ToArray();
				PermissionsDTO[] subcontractorAuthors = rolesWithBoes.Where(x => x.BOEId == boe.Id).Where(x => x.Role == Role.SubcontractorAuthor).Select(x => x).ToArray();
				PermissionsDTO[] approvers = rolesWithBoes.Where(x => x.BOEId == boe.Id).Where(x => x.Role == Role.Approver).Select(x => x).ToArray();
				List<TravelDTO> travels = ws.Travels.Where(x => x.BoeID == boe.Id).ToList();


				decimal totalCostTravel = this.CalculateTotalCostTravel(travels, ws.DecimalPrecision, escalationRates, fees);

				ManageBOEModelView manageBoe = new ManageBOEModelView(boe, wbsDto, clinDto, users, authors.ToCollection(),
					subcontractorAuthors.ToCollection(), approvers.ToCollection(), ws.TaskElements.Where(t => t.BoeID == boe.Id).ToCollection(), totalCostTravel);

				modelViews.Add(manageBoe);
			}

			return modelViews;
		}

		/// <summary>
		/// Calculates the total cost of travel for all MSTTravelTrips for the BOE.  Overridden by the BOEControllerLogicMST.cs class.
		/// </summary>
		/// <param name="travels">The travel elements.</param>
		/// <param name="decimalPrecision">The decimal precision for the workspace.</param>
		/// <param name="escalationRates">The escalation rates.</param>
		/// <param name="fees">The fees.</param>
		/// <returns>0 for SSC</returns>
		internal virtual decimal CalculateTotalCostTravel(ICollection<TravelDTO> travels, int decimalPrecision, ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates, Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees)
		{
			return 0;  // Returns zero by default for SpaceSystems.  Method is overridden for RMS to calculate this appropriately.
		}

		/// <summary>
		/// Gets Users based on role in BOE
		/// </summary>
		/// <param name="boeRolesWithBOEID">Roles in the BOE</param>
		/// <param name="allUsers">Users with any role in the BOE</param>
		/// <param name="role">Role to get users by</param>
		/// <returns>Dictionary of uers mapped to the BOE ID</returns>
		private Dictionary<int, Collection<UserDTO>> GetBoeUsersByRole(HashSet<PermissionsDTO> boeRolesWithBOEID, HashSet<UserDTO> allUsers, Role role)
		{
			ICollection<int> userIdsForRole = boeRolesWithBOEID.Where(x => x.Role == role).Select(x => x.ETIUserId).Distinct().ToList();
			ICollection<UserDTO> usersForRole = allUsers.Where(x => userIdsForRole.Contains(x.UserID)).ToList();

			Dictionary<int, Collection<UserDTO>> usersMappedToBoeId = new Dictionary<int, Collection<UserDTO>>();

			foreach (int boeId in boeRolesWithBOEID.Select(x => x.BOEId).Distinct())
			{
				IEnumerable<int> matchingUserIds = boeRolesWithBOEID.Where(x => x.BOEId == boeId).Select(x => x.ETIUserId).Distinct();
				Collection<UserDTO> matchingUsers = usersForRole.Where(x => matchingUserIds.Contains(x.UserID)).ToCollection();

				usersMappedToBoeId.Add(boeId, matchingUsers);
			}

			return usersMappedToBoeId;
		}

		/// <summary>
		/// Creates the select list for authors/approvers when creating/managing a BOE
		/// </summary>
		/// <param name="inUserIds">UserIDs for the select list</param>
		/// <param name="isSubcontractors">True if the list is for subcontractors</param>
		/// <returns>select list for authors/approvers</returns>
		private Collection<SelectListItem> CreateUserSelectList(Collection<int> inUserIds, bool isSubcontractors)
		{
			if (inUserIds == null)
			{
				throw new ArgumentNullException(nameof(inUserIds));
			}

			Collection<SelectListItem> returnList = new Collection<SelectListItem>();

			ICollection<UserDTO> allUsers = this.userLoader.GetByIds(inUserIds);

			foreach (int user in inUserIds)
			{
				UserDTO selectedUser = allUsers.First(x => x.UserID == user);
				string userDisplayName = selectedUser.DisplayName;

				if (selectedUser.NTID.Contains('.')) // AD group name
				{
					ICollection<UserData> members = this.adUtils.GetAdGroupUsers(userDisplayName);

					List<UserData> orderedMembers = members.OrderBy(m => m.DisplayName).ToList();
					foreach (UserData member in orderedMembers)
					{
						// Load the user's information.  If the user does not currently exist in the database, create it and use its new ID.
						UserDTO userInfo = this.userLoader.GetOrCreateUserByNtid(member.Ntid);
						returnList.Add(new SelectListItem
						{
							Value = userInfo.UserID.ToString(),
							Text = userInfo.DisplayName
						});
					}
				}
				else
				{
					if (isSubcontractors)
					{
						returnList.Add(new SelectListItem
						{
							Text = selectedUser.DisplayName + CommonConstants.SUBCONTRACTOR_AUTHOR_SUFFIX,
							Value = selectedUser.UserID.ToString()
						});
					}
					else
					{
						returnList.Add(new SelectListItem
						{
							Text = selectedUser.DisplayName,
							Value = selectedUser.UserID.ToString()
						});
					}
				}

				returnList = (from a in returnList
							  group a by new { Selected = a.Selected, Value = a.Value, Text = a.Text } into g
							  select new SelectListItem
							  {
								  Value = g.Key.Value,
								  Text = g.Key.Text,
								  Selected = g.Key.Selected
							  }).ToCollection();
			}

			return returnList;
		}
	}
}
