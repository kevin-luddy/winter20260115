// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic._ControllerLogic.Backend
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Transactions;
	using System.Web.Mvc;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.Workspace;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using GenTRAC.DataBridge.DTO;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.PickList;

	public class WorkspaceSettingsControllerLogic
	{
		/// <summary>
		/// Security Information
		/// </summary>
		private ISecurityInformation _securityInformation { get; set; }

		/// <summary>
		/// Service for Workspace Controller
		/// </summary>
		private IWorkspaceControllerLogic workspaceControllerLogic { get; set; }

		/// <summary>
		/// WorkspaceDTODataLoader
		/// </summary>
		private IWorkspaceDTODataLoader workspaceDTODataLoader { get; set; }

		/// <summary>
		/// Proposal Loader
		/// </summary>
		private GenTRAC.DataBridge.DTO.IProposalLoader proposalLoader { get; set; }

		/// <summary>
		/// BOE Pick List Mapper
		/// </summary>
		private BoePickListMapper boePickListMapper { get; set; }

		/// <summary>
		/// PTM Security Mapper
		/// </summary>
		private GenTRAC.DataBridge.Common.Security.ISecurityMapper ptmSecurityMapper { get; set; }

		/// <summary>
		/// User Loader
		/// </summary>
		private IUserDTODataLoader userLoader { get; set; }

		/// <summary>
		/// Permissions Loader
		/// </summary>
		private IPermissionsDTODataLoader permissionsLoader { get; set; }

		/// <summary>
		/// Active Directory Utils
		/// </summary>
		private ActiveDirectoryUtilities adUtils { get; set; }

		/// <summary>
		/// Full WS Recalculation
		/// </summary>
		private IFullWorkspaceRecalculation fullWSRecalc { get; set; }

		/// <summary>
		/// BOE Mediator
		/// </summary>
		private BoeMediator boeMediator { get; set; }

		/// <summary>
		/// BOE Task Element Mediator
		/// </summary>
		private BoeTaskElementMediator boeTaskElementMediator { get; set; }

		/// <summary>
		/// BOE State Machine
		/// </summary>
		private BOEStateMachine boeStateMachine { get; set; }

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="workspaceControllerLogic"></param>
		/// <param name="workspaceDTODataLoader"></param>
		public WorkspaceSettingsControllerLogic(
			ISecurityInformation _securityInformation,
			IWorkspaceControllerLogic workspaceControllerLogic,
			IWorkspaceDTODataLoader workspaceDTODataLoader,
			BoePickListMapper boePickListMapper,
			GenTRAC.DataBridge.DTO.IProposalLoader proposalLoader,
			GenTRAC.DataBridge.Common.Security.ISecurityMapper ptmSecurityMapper,
			IUserDTODataLoader userLoader,
			IPermissionsDTODataLoader permissionsLoader,
			ActiveDirectoryUtilities adUtils,
			IFullWorkspaceRecalculation fullWSRecalc,
			BoeMediator boeMediator,
			BoeTaskElementMediator boeTaskElementMediator,
			BOEStateMachine boeStateMachine
		)
		{
			this._securityInformation = _securityInformation;
			this.workspaceControllerLogic = workspaceControllerLogic;
			this.workspaceDTODataLoader = workspaceDTODataLoader;
			this.boePickListMapper = boePickListMapper;
			this.proposalLoader = proposalLoader;
			this.ptmSecurityMapper = ptmSecurityMapper;
			this.userLoader = userLoader;
			this.permissionsLoader = permissionsLoader;
			this.adUtils = adUtils;
			this.fullWSRecalc = fullWSRecalc;
			this.boeMediator = boeMediator;
			this.boeTaskElementMediator = boeTaskElementMediator;
			this.boeStateMachine = boeStateMachine;
		}

		/// <summary>
		/// Logic to get Workspace Identification
		/// </summary>
		/// <param name="ws"></param>
		/// <returns></returns>
		public IWorkspaceIdentificationModelView GetWorkspaceIdentification(FullWorkspace ws)
		{
			IWorkspaceIdentificationModelView workspaceIdentificationModelView = this.workspaceControllerLogic.GetWorkspaceIdentificationModelView(ws);
			workspaceIdentificationModelView.ApplicationURL = new System.Uri(ConfigurationUtilities.GetAppSetting("ServerURL") + "/" + workspaceIdentificationModelView.ShortName);

			workspaceIdentificationModelView.HoursLabel = FullObjectHelper.HoursLabel(ws);
			workspaceIdentificationModelView.EnableSAP = Utilities.IsSAPEnabledForWorkspace(ws.EnableSAPConnection, ws.CreationDate);
			workspaceIdentificationModelView.ShowSAP = Utilities.ShowSAPForWorkspace(ws.CreationDate);

			ICollection<WorkspaceDTO> workspaceChecks = this.workspaceDTODataLoader.GetAllWsNamesForTrackingNumber(ws.TrackingNumber)
				.Where(x => x.CurrentPTMWorkspace).ToList();

			if (workspaceChecks.Count() == 1 && workspaceChecks.Any(x => x.Id != ws.Id))
			{
				workspaceIdentificationModelView.DoesPTMMultipleWorkspaces = true;
			}
			else if (workspaceChecks.Count() > 1)
			{
				workspaceIdentificationModelView.DoesPTMMultipleWorkspaces = false;
			}

			workspaceIdentificationModelView.ContractTypes = boePickListMapper.GetPickListValues(PickListEnum.ContractType).PickLists
				.Where(p => p.IsActive || ws.SelectedContractTypes.Contains(p.Id))
				.Select(p => p.Id)
				.ToList();

			workspaceIdentificationModelView.SelectedContractTypes = ws.SelectedContractTypes;

			return workspaceIdentificationModelView;
		}

		/// <summary>
		/// Get Pick List Values
		/// </summary>
		/// <returns></returns>
		public ICollection<PickListDto> GetLineOfBusinessType()
		{
			return boePickListMapper.GetPickListValues(PickListEnum.LineOfBusiness).PickLists;
		}

		/// <summary>
		/// Get Proposal Class Options
		/// </summary>
		/// <returns></returns>
		public ICollection<PickListDto> GetProposalClassOptionList()
		{
			return boePickListMapper.GetPickListValues(PickListEnum.ProposalClass).PickLists;
		}

		/// <summary>
		/// Get Contract Type List
		/// </summary>
		/// <returns></returns>
		public ICollection<PickListDto> GetContractTypeOptionList()
		{
			return boePickListMapper.GetPickListValues(PickListEnum.ContractType).PickLists;
		}

		/// <summary>
		/// Get Tracking Number List
		/// </summary>
		/// <param name="trackingNumber"></param>
		/// <returns></returns>
		public ICollection<SelectListItem> GetTrackingNumberOptionList(string trackingNumber)
		{
			Collection<SelectListItem> trackingNumbers = new Collection<SelectListItem>();
			IReadOnlyCollection<GenTRAC.DataBridge.Common.Security.SecurityPermissionsResponse> roles = this.ptmSecurityMapper.GetRolesForLoggedInUser();
			bool isAdmin = roles.Any(r => r.AuthorizedRole == PtmRole.Admin);

			ICollection<ProposalDto> proposals = (isAdmin ? this.proposalLoader.GetAllSlim() : this.proposalLoader.GetProposalsByUser(this._securityInformation.ActiveUserNTID, true))
																					.Where(p => !p.IsForecastProposal && p.ProposalStatus != ProposalStatus.NoBid && p.ProposalStatus != ProposalStatus.Revised).ToList();

			if (!string.IsNullOrWhiteSpace(trackingNumber) && !proposals.Any(p => p.TrackingNumber == trackingNumber))
			{
				// Currently has a bad Tracking Number saved in DB, but we will let that slide
				trackingNumbers.Add(new SelectListItem
				{
					Text = trackingNumber,
					Value = trackingNumber
				});
			}

			foreach (ProposalDto proposal in proposals)
			{
				trackingNumbers.Add(new SelectListItem
				{
					Text = proposal.TrackingNumber + " - " + proposal.ProposalTitle,
					Value = proposal.TrackingNumber
				});
			}

			return trackingNumbers;
		}

		/// <summary>
		/// Gets the next Tracking Number details when workspace tracking number is updated
		/// </summary>
		/// <param name="trackingNumber"></param>
		/// <returns></returns>
		public RefreshPTMResponseData GetNextTrackingNumberRevision(string trackingNumber)
		{
			RefreshPTMResponseData response = new RefreshPTMResponseData();

			string nextRevision = this.GetNextTrackingNumber(trackingNumber);

			int proposalId = this.proposalLoader.GetIdByTrackingNumber(trackingNumber);
			if (proposalId > 0)
			{
				response.TrackingNumberRevision = nextRevision;
				ProposalDto proposal = this.proposalLoader.GetById(proposalId);
				response.RFPNumber = proposal.RFPNumber;
				response.Title = proposal.ProposalTitle;
				if (proposal.ContractTypeIds.Any())
				{
					List<int> contractTypeIds = new List<int>();
					foreach (int contractTypeId in proposal.ContractTypeIds)
					{
						int convertedContractTypeId = workspaceControllerLogic.ConvertPTMContractTypeId(contractTypeId);
						if (convertedContractTypeId > 0)
						{
							contractTypeIds.Add(convertedContractTypeId);
						}
					}
					response.ContractTypes = contractTypeIds.ToArray();
				}

				response.ProposalClassId = workspaceControllerLogic.ConvertPTMProposalClassId(proposal.ProposalClass);

				response.LOBId = workspaceControllerLogic.ConvertPTMLineOfBusiness(proposal.LineOfBusinessID);

				response.AnticipatedDeliveryDate = proposal.DeliveryDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR);

				response.RevisedSubmittalDate = proposal.RevisedSubmittalDate.HasValue
					? proposal.RevisedSubmittalDate.Value.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR)
					: string.Empty;

				// Default Template BOE switch to Yes if CCoPD is set to true
				response.UsingTemplateBoe = proposal.IsCCPDRequired.HasValue ? proposal.IsCCPDRequired.Value : false;

				response.IsSAPEnabledConfig = Utilities.IsSAPEnabledForSystem;
			}
			else
			{
				throw new GenValidationException("PTM Tracking Number is invalid. Please delete the current Tracking Number and choose another from the dropdown list.");
			}

			return response;
		}

		/// <summary>
		/// Save Workspace Identification logic
		/// </summary>
		/// <param name="factory">Factory</param>
		/// <param name="ws">Full Workspace</param>
		/// <param name="workspaceDetails">Workspace details being saved</param>
		/// <returns>Warning/Error messages if any</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="GenValidationException"></exception>
		public string SaveWorkspaceIdentification(IFullObjectFactory factory, FullWorkspace ws, IWorkspaceIdentificationModelView workspaceDetails)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			if (workspaceDetails == null)
			{
				throw new ArgumentNullException(nameof(workspaceDetails));
			}

			string returnMessage = string.Empty;

			bool decimalPrecisionChanged = ws.DecimalPrecision != (workspaceDetails.ResourceDecimalPrecision ?? 0);
			bool costDecimalPrecisionChanged = ws.CostDecimalPrecision != workspaceDetails.CostDecimalPrecision;

			IReadOnlyCollection<SecurityPermissionsResponse> permissions = factory.GetPermissionsForUser(this._securityInformation.ActiveUserNTID);
			bool isAdmin = permissions.Any(p => p.AuthorizedRole == Role.SystemAdmin);

			bool ptmTrackingNumberNotRequired = string.IsNullOrEmpty(ConfigurationUtilities.GetAppSetting("CanCreateWorkspaceWithoutPtmTrackingNumber")) ?
								false : _securityInformation.IsMemberOfADGroupInAppSettingsList(this._securityInformation.ActiveUserNTID, "CanCreateWorkspaceWithoutPtmTrackingNumber");

			ICollection<ValidationMessage> ValidationErrors = workspaceControllerLogic.SaveWorkspaceIdentificationValidation(ws, workspaceDetails, isAdmin, ptmTrackingNumberNotRequired);
			if (ValidationErrors.Any())
			{
				throw new GenValidationException(ValidationErrors);
			}

			#region Setup all the data needed for the save, to minimize the time inside of a transaction

			// Workspace Identification
			Dtos.UserDTO costVolumeLeadDTO = null;
			PermissionsDTO workspaceAdmin = null;

			// Get the user who is saving the BOE(s)
			int currentUserID = ws.CurrentActiveUser.UserID;
			bool templateBoeUsageChanged = ws.UsingTemplateBOE != workspaceDetails.UsingTemplateBoe;

			// Create DTO and populate the common properties
			ws.ContainsOCI = workspaceDetails.ContainsOCI;
			ws.Description = workspaceDetails.Description;
			ws.ProposalSubmittalDate = workspaceDetails.ProposalSubmittalDate != null ? (DateTime?)Convert.ToDateTime(workspaceDetails.ProposalSubmittalDate) : null;
			ws.LineOfBusiness = new PickListDto() { Id = workspaceDetails.LineOfBusinessTypeID };
			ws.RFPNumber = workspaceDetails.RFPNumber;
			ws.UpdateDate = workspaceDetails.UpdateDate;
			ws.WorkspaceName = workspaceDetails.WorkspaceName;
			ws.ProposalStatus = workspaceDetails.ProposalStatus;
			ws.StatusComment = workspaceDetails.StatusComments;
			ws.ResourceDecimalPrecision = workspaceDetails.ResourceDecimalPrecision;
			ws.CostDecimalPrecision = workspaceDetails.CostDecimalPrecision;
			ws.CustomFieldSorting = workspaceDetails.CustomFieldSorting;
			ws.ResourceSorting = workspaceDetails.ResourceSorting;
			ws.PerfOrgSorting = workspaceDetails.PerfOrgSorting;
			ws.RteSizeLimit = workspaceDetails.RteSizeLimit;
			ws.UsingTemplateBOE = workspaceDetails.UsingTemplateBoe;
			ws.EnableSAPConnection = workspaceDetails.EnableSAPConnection;
			ws.CurrentPTMWorkspace = workspaceDetails.CurrentPTMWorkspace;

			// Keep track of the previous value of Enable Assign Task Author
			bool previousValueEnableAssignTaskAuthor = ws.EnableAssignTaskAuthor;
			ws.EnableAssignTaskAuthor = workspaceDetails.EnableAssignTaskAuthor;

			// Populate the company specific properties
			workspaceControllerLogic.PopulateCompanySpecificWorkspaceProperties(workspaceDetails, ws);

			// we only need to do the code below if the pricer/cost volume lead has changed..
			costVolumeLeadDTO = userLoader.GetByIds(new List<int>() { ws.CostVolumeLeadPricerUserID }).FirstOrDefault();

			if (costVolumeLeadDTO == null || costVolumeLeadDTO.NTID != workspaceDetails.CostVolumeLeadPricerNTID)
			{
				// New CostVolumeLead
				UserData costVolumeLeadData = adUtils.GetUserByQualifiedAccount(workspaceDetails.CostVolumeLeadPricerNTID, false);

				costVolumeLeadDTO = userLoader.GetOrCreateUserByNtid(costVolumeLeadData.Ntid);
			}

			HashSet<PermissionsDTO> wsPermissions = new HashSet<PermissionsDTO>(permissionsLoader.GetWorkspacePermissions(ws.Id));

			#endregion

			WorkspaceState originalWsState = ws.WorkspaceState;

			try
			{
				// Setup hashsets to keep track of data from recalculation; this will need to be saved in the transaction
				HashSet<BoeTaskElementDTO> tasksToSave = new HashSet<BoeTaskElementDTO>();
				HashSet<WorkspaceVariableDTO> workspaceVariablesToSave = new HashSet<WorkspaceVariableDTO>();
				HashSet<FullBoe> boesToTransition = new HashSet<FullBoe>();
				HashSet<BoeDTO> originalWsBoes = new HashSet<BoeDTO>(ws.Boes.ToList<BoeDTO>().DeepClone());

				#region Setup things needed for recalculation and execute it; Do not save any data though, that will be done in the transaction

				TimeSpan timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT));


				if (decimalPrecisionChanged || costDecimalPrecisionChanged)
				{
					workspaceControllerLogic.ChangeTheWorkspaceStateDuringRecalculation(ws, currentUserID, WorkspaceState.Initialization, DateTime.Now);

					// The timeout for the transaction needs to be longer, since the recalculation will save a lot more data..
					timeout = new TimeSpan(0, 10, 0);

					workspaceControllerLogic.WsRecalculationStep1(ws, ref tasksToSave, ref workspaceVariablesToSave, ref boesToTransition, originalWsBoes, decimalPrecisionChanged, costDecimalPrecisionChanged, workspaceDetails.CostDecimalPrecision);
				}

				#endregion

				#region Save data in the DB, in a transaction

				ICollection<MoqTypeSelection> moqTypes = templateBoeUsageChanged ? workspaceControllerLogic.GetMoqTypesDataForBoeTemplateSettingChange(ws) : new List<MoqTypeSelection>();

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = timeout }))
				{
					// Give CostVolumeLead Workspace Admin permissions
					Collection<PermissionsDTO> workspacePermissions = wsPermissions.Where(p => p.Role == Role.WorkspaceAdmin && p.ETIUserId == costVolumeLeadDTO.UserID).ToCollection();

					// if no results
					if (!workspacePermissions.Any())
					{
						workspaceAdmin = new PermissionsDTO();
						workspaceAdmin.WorkspaceId = ws.Id;
						workspaceAdmin.Role = Role.WorkspaceAdmin;
						workspaceAdmin.ETIUserId = costVolumeLeadDTO.UserID;
						workspaceAdmin.Updateable = UpdateType.Upsert;

						// Save permissions, if needed
						permissionsLoader.SavePermission(workspaceAdmin);
					}

					ws.CostVolumeLeadPricerUserID = costVolumeLeadDTO.UserID;

					// Save the workspace
					workspaceDTODataLoader.SaveWorkspaceSettings(currentUserID, ws);

					workspaceControllerLogic.SaveMoqTypes(moqTypes);

					if (!ws.IsProjectMapWorkspace)
					{
						if (decimalPrecisionChanged || costDecimalPrecisionChanged) // need to save the recalculations that we ran earlier
						{
							fullWSRecalc.SaveDataEffectedByRecalculation(ws, tasksToSave, workspaceVariablesToSave, boesToTransition);
						}

						// If we recalculated, we need to carry out the actions based on the state transition of Boes
						if (decimalPrecisionChanged)
						{
							fullWSRecalc.PerformStateTransitionActionsForBoesEffectedByRecalculation(ws, boesToTransition, originalWsBoes);
						}

						// If Template Boe usage changed, we need to reset all BOEs back to draft
						if (templateBoeUsageChanged)
						{
							ws.RefreshBoes();

							foreach (FullBoe boe in ws.Boes)
							{
								// apply the actual state-value update
								boe.State = BOEState.Draft;
								boe.Updateable = UpdateType.Upsert;

								boeMediator.MediatedSave(ws, boe);
								boeStateMachine.PerformStateTransitionAction(boe, ws, boe.State, BOEState.Draft);
							}
						}
					}

					// If Authors Assignable at Task Level is set to false and it was previously set to true,
					// change all of the BOEs to Draft and clear all authors from tasks
					if (Utilities.IsAssignTaskAuthorEnabledForSystem && !ws.EnableAssignTaskAuthor && previousValueEnableAssignTaskAuthor)
					{
						ws.RefreshBoes();

						foreach (FullBoe boe in ws.Boes)
						{
							boe.State = BOEState.Draft;
							boe.Updateable = UpdateType.Upsert;

							// Get the task and remove the author
							ICollection<BoeTaskElementDTO> editableTasks = (ICollection<BoeTaskElementDTO>)boe.TaskElements;
							foreach (BoeTaskElementDTO task in editableTasks)
							{
								task.AuthorUserId = null;
								task.Updateable = UpdateType.Upsert;
							}

							boeTaskElementMediator.MediatedBulkSaveTaskElements(editableTasks, ws);
							boeMediator.MediatedSave(ws, boe);
							boeStateMachine.PerformStateTransitionAction(boe, ws, boe.State, BOEState.Draft);
						}
					}

					scope.Complete();
				}

				#endregion
			}
			finally
			{
				if (decimalPrecisionChanged || costDecimalPrecisionChanged) // if precision changed, we locked the WS at the beginning so we need to unlock..
				{
					workspaceControllerLogic.ChangeTheWorkspaceStateDuringRecalculation(ws, currentUserID, originalWsState, null);
				}
			}

			// This checks to see if there are any task elements that contain discrete spreads which now have a delta other than 0
			if (decimalPrecisionChanged && !ws.IsProjectMapWorkspace)
			{
				returnMessage = fullWSRecalc.GenerateMsgIfWsContainsTaskElementsWithNonZeroDeltaLabor(ws);

				if (!string.IsNullOrEmpty(returnMessage))
				{
					// this causes the cache to fully blow out
					factory.ClearWorkspaceCache(ws.Shortname);
					return returnMessage;
				}
			}

			// this causes the cache to fully blow out
			factory.ClearWorkspaceCache(ws.Shortname);

			// This will check to see if any items are failing the new RTE length
			if (ws.RteSizeLimit.HasValue)
			{
				ICollection<RTEValidationMV> issues = workspaceDTODataLoader.GetRteFieldsExceedingLimit(ws.Id);

				if (issues.Any())
				{
					returnMessage = "<b>Your last action was successful.</b><br />The RTE limit is exceeded in at least one instance, please ask the authors to review the data.";
					return returnMessage;
				}
			}

			return returnMessage;
		}

		/// <summary>
		/// Gets the next tracking number
		/// </summary>
		/// <param name="trackingNumber"></param>
		/// <returns></returns>
		private string GetNextTrackingNumber(string trackingNumber)
		{
			string nextRevision = trackingNumber;
			ICollection<WorkspaceDTO> trackingNameData = this.workspaceDTODataLoader.GetAllWsNamesAndTrackingNumberInfo().Where(w => w.TrackingNumber == trackingNumber || w.Shortname.StartsWith(trackingNumber, StringComparison.InvariantCultureIgnoreCase)).ToList();
			if (trackingNameData.Any())
			{
				// extract revision numbers - short names should be of the format [TrackingNumber] or [TrackingNumber]_XX, where XX is the revision number
				IList<string> revisionStrings = trackingNameData.Where(x => x.Shortname.StartsWith(nextRevision + "_")).Select(x => x.Shortname.Substring(x.Shortname.IndexOf("_") + 1, 2)).ToList();
				IList<int> revisions = new List<int>();

				// confirm the extracted values are numbers and convert them to ints
				foreach (string revision in revisionStrings)
				{
					int revisionNumber;
					if (int.TryParse(revision, out revisionNumber))
					{
						revisions.Add(revisionNumber);
					}
				}

				// Get highest number or 0 if there are none
				int highestRevision = revisions.Any() ? revisions.OrderByDescending(x => x).First() : 0;

				nextRevision = nextRevision + "_" + (highestRevision + 1).ToString("00");
			}

			return nextRevision;
		}
	}
}
