// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data.SqlClient;
	using System.Linq;
	using GenBOE.Dtos;
	using GenBOE.Models;
	using GenTRAC.Models;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using IES.Common.PickList;

	public class WorkspaceDTODataLoader : DataLoader<WorkspaceDTO>, IWorkspaceDTODataLoader
	{
		private static object userLock = new object();

		/// <summary>
		/// The activity cutoff days
		/// </summary>
		private static readonly int ACTIVITY_CUTOFF_DAYS = 7;


		/// <summary>
		/// Default Constructor
		/// </summary>
		public WorkspaceDTODataLoader()
		{
			this.Log = new Logger(typeof(WorkspaceDTODataLoader));
		}

		#region Retrieves

		/// <summary>
		/// This method retrieves PARTIAL workspace data: Short and Long names, Ws Id, State, and the Tracking Number
		/// </summary>
		/// <returns>Partially filled wsDtos</returns>
		[DbQuery]
		public virtual Collection<WorkspaceDTO> GetAllWsNamesAndTrackingNumberInfo()
		{
			Collection<WorkspaceDTO> toReturn;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn = (from w in gbe.Workspaces
								select new WorkspaceDTO
								{
									WorkspaceName = w.WorkspaceName,
									Shortname = w.WorkspaceShortName,
									Id = w.WorkspaceID,
									TrackingNumber = w.TrackingNumber,
									WorkspaceState = (WorkspaceState)w.WorkspaceStateID,
									CurrentPTMWorkspace = w.CurrentPTMWorkspace,
									UCOTFactor = w.UCOTFactor,
									EnableAssignTaskAuthor = w.EnableAssignTaskAuthor
								}).ToCollection();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// This method retrieves PARTIAL workspace data: Short and Long names, Ws Id, State, and the Tracking Number
		/// </summary>
		/// <param name="trackingNumber">The tracking number to search against.</param>
		/// <returns>Partially filled wsDtos</returns>
		[DbQuery]
		public virtual ICollection<WorkspaceDTO> GetAllWsNamesForTrackingNumber(string trackingNumber)
		{
			Collection<WorkspaceDTO> toReturn;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn = (from w in gbe.Workspaces
								where w.TrackingNumber.ToLower() == trackingNumber.ToLower()
								select new WorkspaceDTO
								{
									WorkspaceName = w.WorkspaceName,
									Shortname = w.WorkspaceShortName,
									Id = w.WorkspaceID,
									TrackingNumber = w.TrackingNumber,
									WorkspaceState = (WorkspaceState)w.WorkspaceStateID,
									CurrentPTMWorkspace = w.CurrentPTMWorkspace,
									UCOTFactor = w.UCOTFactor,
									EnableAssignTaskAuthor = w.EnableAssignTaskAuthor
								}).ToCollection();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// This method retrieves PARTIAL workspace data - everything that is needed for homepage, but nothing more
		/// </summary>
		/// <param name="userId">The user id.</param>
		/// <returns>workspaces</returns>
		[DbQuery]
		public virtual ICollection<GenBOEHomepageWorkspaceRowModelView> GetAllWsForHomepageGrid(int userId)
		{
			Collection<GenBOEHomepageWorkspaceRowModelView> toReturn = null;

			int boeApprovedState = (int)BOEState.Approved;
			int boeAwaitingState = (int)BOEState.AwaitingApproval;
			int boeDraftState = (int)BOEState.Draft;
			int boeDraftLockedState = (int)BOEState.DraftLocked;
			int boeUnassignedState = (int)BOEState.Unassigned;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn = (from w in gbe.Workspaces
								join cv in gbe.ETIusers on w.CostVolumeLeadPricerUserID equals cv.ETIUserID
								join uXref in gbe.WorkspaceUserXREFs on new { a = w.WorkspaceID, b = userId } equals new { a = uXref.WorkspaceID, b = uXref.ETIUserId }
								into wsJoin
								from ws in wsJoin.DefaultIfEmpty()
								select new GenBOEHomepageWorkspaceRowModelView
								{
									WorkspaceId = w.WorkspaceID,
									CostVolumeLeadPricer = cv != null ? cv.DisplayName : string.Empty,
									CostVolumeLeadPricerNtId = cv != null ? cv.NTID : string.Empty,
									CurrentUserHasAdmin = true,
									LineOfBusiness = w.LineOfBusiness.LineOfBusinessName,
									ps = w.ProposalStatusID,
									sub = w.RevisedSubmittalDate.HasValue ? w.RevisedSubmittalDate : w.ProposalSubmitDate,
									TrackingNumber = w.TrackingNumber,
									st = w.WorkspaceStateID,
									WorkspaceName = w.WorkspaceName,
									WorkspaceShortName = w.WorkspaceShortName,
									NumBOEsApproved = w.BOEs.Count(t => t != null && t.BOEStateID == boeApprovedState),
									NumBOEsInAwaitingApproval = w.BOEs.Count(t => t != null && t.BOEStateID == boeAwaitingState),
									NumBOEsInDraft = w.BOEs.Count(t => t != null && (t.BOEStateID == boeDraftState || t.BOEStateID == boeDraftLockedState)),
									NumBOEsUnassigned = w.BOEs.Count(t => t != null && t.BOEStateID == boeUnassignedState),
									HasBeenDeleted = w.IsDeleted ?? false,
									up = w.UpdateDT,
									IsFavorite = ws.IsFavorite ?? false,
									la = ws.LastAccessed
								}).ToCollection();
				}


			}

			return toReturn;
		}

		/// <summary>
		/// Gets the Workspaces for homepage grid.
		/// </summary>
		/// <param name="workspaceIds">The workspace ids.</param>
		/// <param name="workspaceIdsWithAdmin">The workspace ids with admin.</param>
		/// <param name="userId">The user Id.</param>
		/// <returns>workspaces</returns>
		[DbQuery]
		public virtual ICollection<GenBOEHomepageWorkspaceRowModelView> GetWsForHomepageGrid(ICollection<int> workspaceIds, ICollection<int> workspaceIdsWithAdmin, int userId)
		{
			Collection<GenBOEHomepageWorkspaceRowModelView> toReturn = null;

			int boeApprovedState = (int)BOEState.Approved;
			int boeAwaitingState = (int)BOEState.AwaitingApproval;
			int boeDraftState = (int)BOEState.Draft;
			int boeDraftLockedState = (int)BOEState.DraftLocked;
			int boeUnassignedState = (int)BOEState.Unassigned;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn = (from w in gbe.Workspaces
								join cv in gbe.ETIusers on w.CostVolumeLeadPricerUserID equals cv.ETIUserID
								join uXref in gbe.WorkspaceUserXREFs on new { a = w.WorkspaceID, b = userId } equals new { a = uXref.WorkspaceID, b = uXref.ETIUserId }
								into wsJoin
								where workspaceIds.Contains(w.WorkspaceID) && !(w.IsDeleted ?? false)
								from ws in wsJoin.DefaultIfEmpty()
								select new GenBOEHomepageWorkspaceRowModelView
								{
									WorkspaceId = w.WorkspaceID,
									CostVolumeLeadPricer = cv != null ? cv.DisplayName : string.Empty,
									CostVolumeLeadPricerNtId = cv != null ? cv.NTID : string.Empty,
									LineOfBusiness = w.LineOfBusiness.LineOfBusinessName,
									ps = w.ProposalStatusID,
									sub = w.RevisedSubmittalDate.HasValue ? w.RevisedSubmittalDate : w.ProposalSubmitDate,
									TrackingNumber = w.TrackingNumber,
									st = w.WorkspaceStateID,
									WorkspaceName = w.WorkspaceName,
									WorkspaceShortName = w.WorkspaceShortName,
									NumBOEsApproved = w.BOEs.Count(t => t != null && t.BOEStateID == boeApprovedState),
									NumBOEsInAwaitingApproval = w.BOEs.Count(t => t != null && t.BOEStateID == boeAwaitingState),
									NumBOEsInDraft = w.BOEs.Count(t => t != null && (t.BOEStateID == boeDraftState || t.BOEStateID == boeDraftLockedState)),
									NumBOEsUnassigned = w.BOEs.Count(t => t != null && t.BOEStateID == boeUnassignedState),
									HasBeenDeleted = w.IsDeleted ?? false,
									up = w.UpdateDT,
									IsFavorite = ws.IsFavorite ?? false,
									la = ws.LastAccessed
								}).ToCollection();
				}

				toReturn.AsParallel().ForAll(w =>
				{
					w.CurrentUserHasAdmin = workspaceIdsWithAdmin.Contains(w.WorkspaceId);
				});

			}

			return toReturn;
		}


		/// <summary>
		/// Gets a collection of workspaces by ids
		/// </summary>
		/// <param name="ids">ids</param>
		/// <returns>workspaces</returns>
		[DbQuery]
		public override ICollection<WorkspaceDTO> GetByIds(ICollection<int> ids)
		{
			ICollection<WorkspaceDTO> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn = (from w in gbe.Workspaces.Where(w => ids.Contains(w.WorkspaceID))
								select new WorkspaceDTO
								{
									Id = w.WorkspaceID,
									CostVolumeLeadPricerUserID = w.CostVolumeLeadPricerUserID,
									ProposalSubmittalDate = w.ProposalSubmitDate,
									WorkspaceName = w.WorkspaceName,
									Shortname = w.WorkspaceShortName,
									TrackingNumber = w.TrackingNumber,
									HasBeenDeleted = w.IsDeleted ?? false,
									UpdateDate = w.UpdateDT,

									ContainsOCI = w.ContainsOCI,
									AllowSearch = w.AllowSearch,
									BOEExportSortByID = w.BOEExportSortByID,

									Description = w.WorkspaceDescription,
									ContractStartDate = w.ContractStartDate,
									ContractEndDate = w.ContractEndDate,
									RFPNumber = w.RFPNumber,
									TemplateID = w.TemplateID,
									CreatedByUserID = w.CreatedByETIUserID,
									ResourceListID = w.ResourceListID ?? 0,
									PerfOrgListID = w.PerformingOrganizationListID ?? 0,
									PerfOrgsChanged = w.PerformingOrganizationChangeFlag,
									ContainsTemplate = w.ContainsTemplate,
									NumberOfTimesExportedToProPricer = w.NumProPricerExport,
									StatusComment = w.StatusComment,
									ProposalTitle = w.ProposalTitle,
									ResourceDecimalPrecision = w.ResourcePrecision,
									DateDeleted = w.DateDeleted,
									DateRecalculationStarted = w.RecalculationStartedDate,
									CostDecimalPrecision = ((int?)w.CostPrecision) ?? 2,

									ProposalStatus = (ProposalStatusType)w.ProposalStatusID,
									WorkspaceState = (WorkspaceState)w.WorkspaceStateID,
									Segment = (SegmentType)(w.SegmentID ?? 0),
									ProposalClass = new PickListDto
									{
										Id = w.ProposalClassLU == null ? -1 : w.ProposalClassLU.ProposalClassID,
										Text = w.ProposalClassLU == null ? string.Empty : w.ProposalClassLU.ProposalClass
									},

									LineOfBusiness = new PickListDto
									{
										Id = w.LineOfBusiness == null ? -1 : w.LineOfBusiness.LineOfBusinessID,
										Text = w.LineOfBusiness == null ? string.Empty : w.LineOfBusiness.LineOfBusinessName
									},

									SelectedContractTypeIEnum = w.WorkspaceContractTypeXREFs.Select(wCt => wCt.ContractTypeID),
									IsUsingEquivalentPerson = w.IsUsingEquivalentPerson,
									IsUsingTM = w.IsUsingTM,
									ProjectMapType = (ProjectMapType)w.ProjectMapTypeID,
									AllowGridEdit = w.AllowGridEdit,
									CustomFieldSorting = (CustomFieldSorting)(w.CustomSorting),
									ResourceSorting = (CustomFieldSorting)(w.ResourceSorting),
									PerfOrgSorting = (CustomFieldSorting)(w.PerfOrgSorting),
									LastProPricerInstance = w.LastProPricerInstance,
									LastProPricerProposal = w.LastProPricerProposal,
									RteSizeLimit = w.RteSizeLimit,
									RevisedSubmittalDate = w.RevisedSubmittalDate,
									UsingTemplateBOE = w.TemplateBoe,
									EnableSAPConnection = w.EnableSAPConnection,
									CurrentPTMWorkspace = w.CurrentPTMWorkspace,
									CreationDate = w.WorkspaceCreationDate,
									UCOTFactor = w.UCOTFactor,
									EnableAssignTaskAuthor = w.EnableAssignTaskAuthor,
									EnableLmNavigator = w.EnableLmNavigator
								}).ToCollection();

					toReturn.ToList().ForEach(w =>
					{
						w.ContractStartDate = w.ContractStartDate.Normalize();
						w.ContractEndDate = w.ContractEndDate.Normalize();

						w.SelectedContractTypes = w.SelectedContractTypeIEnum.ToCollection(); w.SelectedContractTypeIEnum = null;
					});
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Get Workspace Data for a given workspace
		/// </summary>
		/// <param name="shortName">shortname</param>
		/// <returns>workspace DTO</returns>
		[DbQuery]
		public virtual WorkspaceDTO GetByShortname(string shortName)
		{
			WorkspaceDTO toReturn = null;

			if (!string.IsNullOrWhiteSpace(shortName))
			{
				using (StopwatchTimer sw = new StopwatchTimer(this.Log))
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						toReturn = (from w in gbe.Workspaces.Where(w => w.WorkspaceShortName == shortName)
									select new WorkspaceDTO
									{
										Id = w.WorkspaceID,
										CostVolumeLeadPricerUserID = w.CostVolumeLeadPricerUserID,
										ProposalSubmittalDate = w.ProposalSubmitDate,
										WorkspaceName = w.WorkspaceName,
										Shortname = w.WorkspaceShortName,
										TrackingNumber = w.TrackingNumber,
										HasBeenDeleted = w.IsDeleted ?? false,
										UpdateDate = w.UpdateDT,

										ContainsOCI = w.ContainsOCI,
										AllowSearch = w.AllowSearch,
										BOEExportSortByID = w.BOEExportSortByID,

										Description = w.WorkspaceDescription,
										ContractStartDate = w.ContractStartDate,
										ContractEndDate = w.ContractEndDate,
										RFPNumber = w.RFPNumber,
										TemplateID = w.TemplateID,
										CreatedByUserID = w.CreatedByETIUserID,
										ResourceListID = w.ResourceListID ?? 0,
										PerfOrgListID = w.PerformingOrganizationListID ?? 0,
										PerfOrgsChanged = w.PerformingOrganizationChangeFlag,
										ContainsTemplate = w.ContainsTemplate,
										NumberOfTimesExportedToProPricer = w.NumProPricerExport,
										StatusComment = w.StatusComment,
										ProposalTitle = w.ProposalTitle,
										ResourceDecimalPrecision = w.ResourcePrecision,
										DateDeleted = w.DateDeleted,
										DateRecalculationStarted = w.RecalculationStartedDate,
										CostDecimalPrecision = ((int?)w.CostPrecision) ?? 2,

										ProposalStatus = (ProposalStatusType)w.ProposalStatusID,
										WorkspaceState = (WorkspaceState)w.WorkspaceStateID,
										Segment = (SegmentType)(w.SegmentID ?? 0),
										ProposalClass = new PickListDto
										{
											Id = w.ProposalClassLU == null ? -1 : w.ProposalClassLU.ProposalClassID,
											Text = w.ProposalClassLU == null ? string.Empty : w.ProposalClassLU.ProposalClass
										},
										LineOfBusiness = new PickListDto
										{
											Id = w.LineOfBusiness == null ? -1 : w.LineOfBusiness.LineOfBusinessID,
											Text = w.LineOfBusiness == null ? string.Empty : w.LineOfBusiness.LineOfBusinessName
										},

										SelectedContractTypeIEnum = w.WorkspaceContractTypeXREFs.Select(wCt => wCt.ContractTypeID),
										IsUsingEquivalentPerson = w.IsUsingEquivalentPerson,
										IsUsingTM = w.IsUsingTM,
										ProjectMapType = (ProjectMapType)w.ProjectMapTypeID,
										AllowGridEdit = w.AllowGridEdit,
										CustomFieldSorting = (CustomFieldSorting)(w.CustomSorting),
										ResourceSorting = (CustomFieldSorting)(w.ResourceSorting),
										PerfOrgSorting = (CustomFieldSorting)(w.PerfOrgSorting),
										LastProPricerInstance = w.LastProPricerInstance,
										LastProPricerProposal = w.LastProPricerProposal,
										RteSizeLimit = w.RteSizeLimit,
										RevisedSubmittalDate = w.RevisedSubmittalDate,
										UsingTemplateBOE = w.TemplateBoe,
										EnableSAPConnection = w.EnableSAPConnection,
										CurrentPTMWorkspace = w.CurrentPTMWorkspace,
										CreationDate = w.WorkspaceCreationDate,
										UCOTFactor = w.UCOTFactor,
										EnableAssignTaskAuthor = w.EnableAssignTaskAuthor,
										EnableLmNavigator = w.EnableLmNavigator
									}).FirstOrDefault();

						if (toReturn != null)
						{
							toReturn.ContractStartDate = toReturn.ContractStartDate.Normalize();
							toReturn.ContractEndDate = toReturn.ContractEndDate.Normalize();

							toReturn.SelectedContractTypes = toReturn.SelectedContractTypeIEnum.ToCollection(); toReturn.SelectedContractTypeIEnum = null;
						}
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Gets the rte fields exceeding limit.
		/// </summary>
		/// <param name="wsId">The ws id.</param>
		/// <returns>A list of fields that exceed the RTE limit</returns>
		[DbQuery]
		public virtual ICollection<RTEValidationMV> GetRteFieldsExceedingLimit(int wsId)
		{
			List<RTEValidationMV> data = null;
			int? rteSizeLimit;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					Workspace tempWs = gbe.Workspaces.FirstOrDefault(x => x.WorkspaceID == wsId && x.RteSizeLimit.HasValue);
					rteSizeLimit = tempWs.RteSizeLimit;

					data = tempWs.BOEs.Where(b => b.BOEDescription != null)
								.Select(b => new RTEValidationMV
								{
									Field = "BOE Description",
									BoeId = b.BOEID,
									TaskId = null,
									Value = b.BOEDescription
								}).Union(
							tempWs.BOEs.Where(b => b.DataSource != null)
								.Select(b => new RTEValidationMV
								{
									Field = "BOE Sources Of Data",
									BoeId = b.BOEID,
									TaskId = null,
									Value = b.DataSource
								})).Union(
							tempWs.BOEs.SelectMany(b => b.BOETaskElements).Where(t => t.TaskDescription != null)
								.Select(t => new RTEValidationMV
								{
									Field = "Task Description",
									BoeId = t.BOEID,
									TaskId = t.BOETaskElementID,
									Value = t.TaskDescription
								})).Union(
							tempWs.BOEs.SelectMany(b => b.BOETaskElements).Where(t => t.MOQText != null)
								.Select(t => new RTEValidationMV
								{
									Field = "Task MOQ Rationale",
									BoeId = t.BOEID,
									TaskId = t.BOETaskElementID,
									Value = t.MOQText
								}))
						.ToList();
				}

				data = data.Where(x => GenBOEUtilities.ConvertHtmlToText(x.Value).Length > rteSizeLimit).ToList();
			}

			return data;
		}

		/// <summary>
		/// Gets the ws for backup generation.
		/// </summary>
		/// <returns></returns>
		[DbQuery, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public List<Tuple<int, WorkspaceState>> GetWsForBackupGeneration()
		{
			List<Tuple<int, WorkspaceState>> result;

			DateTime dateCutoff = DateTime.Now.Date.AddDays(-1 * ACTIVITY_CUTOFF_DAYS);

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.Database.CommandTimeout = 900; // 15 minutes

					result = gbe.Workspaces.Where(x =>
						x.WorkspaceStateID != (int)WorkspaceState.Complete && x.WorkspaceStateID != (int)WorkspaceState.Closed
						&& !(x.IsDeleted == true)
						&& !x.WorkspaceName.StartsWith("Mock")
						&& dateCutoff < (new List<DateTime>() { // Selecting all dates, then doing a max after, doubled the execution time
							x.BOEs.Select(z => z.UpdateDT).Max(),
							x.BOEs.SelectMany(z => z.BOETaskElements).Select(z => z.UpdateDT).Max(),
							x.BOEs.SelectMany(z => z.BOETaskElements).SelectMany(z => z.OrdinaryVariables).Select(z => z.UpdateDT).Max(),
							x.BOEs.SelectMany(z => z.BOETaskElements).SelectMany(z => z.BOELaborTypes).Select(z => z.UpdateDT).Max(),
							x.BOEs.SelectMany(z => z.BOEApprovals).Select(z => z.UpdateDT).Max(),
							x.BOEs.SelectMany(z => z.BOEApprovalHistories).Select(z => z.UpdateDT).Max(),
							x.BOEs.SelectMany(z => z.BOEComments).Select(z => z.UpdateDT).Max(),
							x.BOEs.SelectMany(z => z.BOECommentHistories).Select(z => z.UpdateDT).Max(),
							x.BOEs.SelectMany(z => z.BOEUserRoles).Select(z => z.UpdateDT).Max(),
							x.BOEs.SelectMany(z => z.BOEUserRoleHistories).Select(z => z.UpdateDT).Max(),
							x.BOEs.SelectMany(z => z.BOEStateHistories).Select(z => z.UpdateDT).Max(),
							x.CLINs.Select(z => z.UpdateDT).Max(),
							x.WorkBreakdownStructures.Select(z => z.UpdateDT).Max(),
							x.CustomFields.Select(z => z.UpdateDT).Max(),
							x.CustomFields.SelectMany(z => z.CustomFieldValues).Select(z => z.UpdateDT).Max(),
							x.TMResourceRates.Select(z => z.UpdateDT).Max(),
							x.WorkspaceStateHistories.Select(z => z.UpdateDT).Max(),
							x.WorkspaceUserRoles.Select(z => z.UpdateDT).Max(),
							x.WorkspaceVariables.Select(z => z.UpdateDT).Max(),
							x.UpdateDT }).Max()
					).Select(x => new { Item1 = x.WorkspaceID, Item2 = (WorkspaceState)x.WorkspaceStateID }).ToList()
					.Select(x => new Tuple<int, WorkspaceState>(x.Item1, x.Item2)).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get Current Workspace Data For Proposal
		/// 
		/// Used by ACV
		/// </summary>
		/// <param name="ptmTrackingNumber">PTM Tracking Number</param>
		/// <returns>Current Workspace Data List or single item when PTM Tracking Number is provided </returns>
		[DbQuery]
		public virtual ICollection<(int Id, string shortName, string longName, bool containsOCI, string ptmTrackingNumber)> GetWorkspaceDataForProposal(string ptmTrackingNumber)
		{
			ICollection<(int Id, string shortName, string longName, bool containsOCI, string ptmTrackingNumber)> result;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = gbe.Workspaces.Where(x => x.TrackingNumber == ptmTrackingNumber && x.IsDeleted != true && x.CurrentPTMWorkspace)
									.Select(x => new { Id = x.WorkspaceID, shortName = x.WorkspaceShortName, longName = x.WorkspaceName, containsOCI = x.ContainsOCI, ptmTrackingNumber = x.TrackingNumber }).ToList()
									.Select(x => (Id: x.Id, shortName: x.shortName, longName: x.longName, containsOCI: x.containsOCI, ptmTrackingNumber = x.ptmTrackingNumber)).ToList();

				}
			}

			return result;
		}

		/// <summary>
		/// Get All Current Workspace Data
		/// 
		/// Used by ACV
		/// </summary>
		/// <returns>Current Workspace Data List</returns>
		[DbQuery]
		public virtual ICollection<(int Id, string shortName, string longName, bool containsOCI, string ptmTrackingNumber)> GetWorkspaceData()
		{
			ICollection<(int Id, string shortName, string longName, bool containsOCI, string ptmTrackingNumber)> result;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = gbe.Workspaces.Where(x => x.IsDeleted != true && x.CurrentPTMWorkspace)
									.Select(x => new { Id = x.WorkspaceID, shortName = x.WorkspaceShortName, longName = x.WorkspaceName, containsOCI = x.ContainsOCI, ptmTrackingNumber = x.TrackingNumber }).ToList()
									.Select(x => (Id: x.Id, shortName: x.shortName, longName: x.longName, containsOCI: x.containsOCI, ptmTrackingNumber: x.ptmTrackingNumber)).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get Workspace data by NTID to be used in NLF home grid
		/// </summary>
		/// <param name="ntid">user NTID</param>
		/// <returns>Collection of Workspace IDs, URLs, and Names where user is WS or GSCO admin</returns>
		[DbQuery]
		public ICollection<NlfWorkspaceDataDTO> GetWorkspaceDataByNtidForNlf(string ntid)
		{
			ICollection<NlfWorkspaceDataDTO> result;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from w in gbe.Workspaces
							  join wur in gbe.WorkspaceUserRoles on w.WorkspaceID equals wur.WorkspaceID
							  join eu in gbe.ETIusers on wur.ETIUserID equals eu.ETIUserID
							  where eu.NTID == ntid
							  && (wur.RoleID == (int)Role.WorkspaceAdmin || wur.RoleID == (int)Role.SubcontractAdmin)
							  && w.IsDeleted == false
							  select new NlfWorkspaceDataDTO
							  {
								  WorkspaceId = w.WorkspaceID,
								  WorkspaceUrl = w.WorkspaceShortName,
								  WorkspaceName = w.WorkspaceName
							  }).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get all Workspace data for a system admin to be used in NLF home grid
		/// </summary>
		/// <returns>Collection of Workspace IDs, URLs, and Names</returns>
		[DbQuery]
		public ICollection<NlfWorkspaceDataDTO> GetAllWorkspaceDataForNlf()
		{
			ICollection<NlfWorkspaceDataDTO> result;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from w in gbe.Workspaces
							  where w.IsDeleted == false
							  select new NlfWorkspaceDataDTO
							  {
								  WorkspaceId = w.WorkspaceID,
								  WorkspaceUrl = w.WorkspaceShortName,
								  WorkspaceName = w.WorkspaceName
							  }).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get Workspace data by NTID to be used in NLF home grid
		/// </summary>
		/// <param name="ntid">user NTID</param>
		/// <param name="trackingNumbers">list of all tracking numbers tied to a user</param>
		/// <returns>Collection of Workspace IDs, URLs, and Names where user is WS or GSCO admin</returns>
		[DbQuery]
		public ICollection<NlfWorkspaceInnerDataDTO> GetWorkspaceInnerDataByNtidForNlf(string ntid, ICollection<string> trackingNumbers)
		{
			ICollection<NlfWorkspaceInnerDataDTO> result;
			ICollection<string> filteredTrackingNumbers;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (genTRACEntities gte = new genTRACEntities())
				{
					filteredTrackingNumbers = (from p in gte.Proposals
											   join pur in gte.ProposalUserRoles on p.ProposalID equals pur.ProposalID
											   where (pur.RoleID == (int)PtmRole.SupplyChainPOCSubs
												 || pur.RoleID == (int)PtmRole.BackupSubcontractsLead
												 || pur.RoleID == (int)PtmRole.SupplyChainPOCMatl
												 || pur.RoleID == (int)PtmRole.BackupMaterialLead
												 || pur.RoleID == (int)PtmRole.Pricer
												 || pur.RoleID == (int)PtmRole.BackupPricer
												 || pur.RoleID == (int)PtmRole.CostVolumeLead)
												 && pur.genTRACUser.NTID == ntid
											   select p.ProposalTrackingID).Distinct().ToList();
				}

				filteredTrackingNumbers = filteredTrackingNumbers.Where(x => trackingNumbers.Contains(x)).ToList();

				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from w in gbe.Workspaces
							  join eu in gbe.ETIusers on w.CostVolumeLeadPricerUserID equals eu.ETIUserID
							  where w.CurrentPTMWorkspace
						&& filteredTrackingNumbers.Contains(w.TrackingNumber)
						&& w.IsDeleted == false
							  select new NlfWorkspaceInnerDataDTO
							  {
								  WorkspaceId = w.WorkspaceID,
								  WorkspaceUrl = w.WorkspaceShortName,
								  WorkspaceName = w.WorkspaceName,
								  LineOfBusiness = w.LineOfBusiness,
								  PTMTrackingNumber = w.TrackingNumber,
								  WorkspaceCreationDate = w.WorkspaceCreationDate,
								  EstimatingLead = eu.DisplayName
							  }).Distinct().ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get Workspace data to be used in NLF home grid
		/// </summary>
		/// <param name="trackingNumbers">list of all tracking numbers tied to a user</param>
		/// <returns>Collection of Workspace IDs, URLs, and Names where user is WS or GSCO admin</returns>
		[DbQuery]
		public ICollection<NlfWorkspaceInnerDataDTO> GetWorkspaceInnerDataForNlf(ICollection<string> trackingNumbers)
		{
			ICollection<NlfWorkspaceInnerDataDTO> result;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from w in gbe.Workspaces
							  join eu in gbe.ETIusers on w.CostVolumeLeadPricerUserID equals eu.ETIUserID
							  where w.CurrentPTMWorkspace
						&& trackingNumbers.Contains(w.TrackingNumber)
						&& w.IsDeleted == false
							  select new NlfWorkspaceInnerDataDTO
							  {
								  WorkspaceId = w.WorkspaceID,
								  WorkspaceUrl = w.WorkspaceShortName,
								  WorkspaceName = w.WorkspaceName,
								  LineOfBusiness = w.LineOfBusiness,
								  PTMTrackingNumber = w.TrackingNumber,
								  WorkspaceCreationDate = w.WorkspaceCreationDate,
								  EstimatingLead = eu.DisplayName
							  }).Distinct().ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get all Workspace Inner Data for a System Admin to be used in the NLF Home Grid
		/// </summary>
		/// <returns>Collection of Workspace Inner Data</returns>
		[DbQuery]
		public ICollection<NlfWorkspaceInnerDataDTO> GetAllWorkspaceInnerDataForNlf()
		{
			ICollection<NlfWorkspaceInnerDataDTO> result;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from w in gbe.Workspaces
							  join eti in gbe.ETIusers on w.CostVolumeLeadPricerUserID equals eti.ETIUserID
							  where w.IsDeleted == false
							  && w.CurrentPTMWorkspace
							  select new NlfWorkspaceInnerDataDTO
							  {
								  WorkspaceId = w.WorkspaceID,
								  WorkspaceUrl = w.WorkspaceShortName,
								  WorkspaceName = w.WorkspaceName,
								  LineOfBusiness = w.LineOfBusiness,
								  PTMTrackingNumber = w.TrackingNumber,
								  WorkspaceCreationDate = w.WorkspaceCreationDate,
								  EstimatingLead = eti.DisplayName
							  }).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get Workspace Inner Data for a System Admin
		/// </summary>
		/// <param name="workspaceId">Workspace Id to search</param>
		/// <returns>Collection of Workspace Inner Data</returns>
		[DbQuery]
		public ICollection<NlfWorkspaceInnerDataDTO> GetWorkspaceInnerDataForNlf(int workspaceId)
		{
			ICollection<NlfWorkspaceInnerDataDTO> result;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from w in gbe.Workspaces
							  join eti in gbe.ETIusers on w.CostVolumeLeadPricerUserID equals eti.ETIUserID
							  where w.WorkspaceID == workspaceId && w.IsDeleted == false
							  select new NlfWorkspaceInnerDataDTO
							  {
								  WorkspaceId = w.WorkspaceID,
								  WorkspaceUrl = w.WorkspaceShortName,
								  WorkspaceName = w.WorkspaceName,
								  LineOfBusiness = w.LineOfBusiness,
								  PTMTrackingNumber = w.TrackingNumber,
								  WorkspaceCreationDate = w.WorkspaceCreationDate,
								  EstimatingLead = eti.DisplayName
							  }).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get Workspace Inner Data for a System Admin
		/// </summary>
		/// <param name="trackingNumber">Tracking Number</param>
		/// <returns>Collection of Workspace Inner Data</returns>
		[DbQuery]
		public ICollection<NlfWorkspaceInnerDataDTO> GetWorkspaceInnerDataForNlf(string trackingNumber)
		{
			ICollection<NlfWorkspaceInnerDataDTO> result;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from w in gbe.Workspaces
							  join eti in gbe.ETIusers on w.CostVolumeLeadPricerUserID equals eti.ETIUserID
							  where w.TrackingNumber == trackingNumber && w.IsDeleted == false
							  && w.CurrentPTMWorkspace
							  select new NlfWorkspaceInnerDataDTO
							  {
								  WorkspaceId = w.WorkspaceID,
								  WorkspaceUrl = w.WorkspaceShortName,
								  WorkspaceName = w.WorkspaceName,
								  LineOfBusiness = w.LineOfBusiness,
								  PTMTrackingNumber = w.TrackingNumber,
								  WorkspaceCreationDate = w.WorkspaceCreationDate,
								  EstimatingLead = eti.DisplayName
							  }).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get Material PBoe Data for a given Workspace
		/// </summary>
		/// <param name="workspaceID"></param>
		/// <returns>Collection of Material PBoe</returns>
		[DbQuery]
		public ICollection<MPBoeDataDTO> GetMaterialPBoeForWorkspace(int workspaceID)
		{
			ICollection<MPBoeDataDTO> result;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from w in gbe.Workspaces
							  where w.WorkspaceID == workspaceID
							  select new MPBoeDataDTO
							  {
								  PTMProposalTitle = w.ProposalTitle,
								  RFPNumber = w.RFPNumber,
								  CLINNumbers = w.CLINs.Select(x => x.DisplayedCLINNumber).ToList(),
								  WBSNumbers = w.WorkBreakdownStructures.Select(x => x.DisplayedWBSNumber).ToList(),
								  WorkspaceName = w.WorkspaceName,
								  ShortName = w.WorkspaceShortName,
								  TrackingNumber = w.TrackingNumber
							  }).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get Material PBoe Data for a given tracking number
		/// </summary>
		/// <param name="trackingNumber">The tracking number</param>
		/// <returns>Collection of Material PBoe</returns>
		[DbQuery]
		public ICollection<MPBoeDataDTO> GetMaterialPBoeForWorkspace(string trackingNumber)
		{
			ICollection<MPBoeDataDTO> result;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from w in gbe.Workspaces
							  where w.TrackingNumber == trackingNumber
							  && w.CurrentPTMWorkspace
							  select new MPBoeDataDTO
							  {
								  PTMProposalTitle = w.ProposalTitle,
								  RFPNumber = w.RFPNumber,
								  CLINNumbers = w.CLINs.Select(x => x.DisplayedCLINNumber).ToList(),
								  WBSNumbers = w.WorkBreakdownStructures.Select(x => x.DisplayedWBSNumber).ToList(),
								  WorkspaceName = w.WorkspaceName,
								  ShortName = w.WorkspaceShortName,
								  TrackingNumber = w.TrackingNumber
							  }).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get all workspaces given a tracking number
		/// </summary>
		/// <param name="trackingNumber">Tracking number</param>
		/// <returns>Collection of matching workspaces</returns>
		[DbQuery]
		public ICollection<WorkspaceDTO> GetWorkspacesByTrackingNumber(string trackingNumber)
		{
			ICollection<WorkspaceDTO> result;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from w in gbe.Workspaces
							  where w.TrackingNumber == trackingNumber
							  select new WorkspaceDTO
							  {
								  TrackingNumber = w.TrackingNumber,
								  RFPNumber = w.RFPNumber,
								  WorkspaceName = w.WorkspaceName,
								  Id = w.WorkspaceID,
								  CurrentPTMWorkspace = w.CurrentPTMWorkspace,
								  ResourceListID = w.ResourceListID.Value,
								  IsUsingTM = w.IsUsingTM,
								  IsUsingEquivalentPerson = w.IsUsingEquivalentPerson,
								  ResourceDecimalPrecision = w.ResourcePrecision,
								  CostDecimalPrecision = w.CostPrecision
							  }).ToList();
				}
			}

			return result;
		}

		/// <summary>
		/// Get All Workspaces that contain a tracking number		/// 
		/// Typically used for PLD Status Sync, needs to grab all data needed to call UpsertWorkspace
		/// </summary>
		/// <returns>Workspaces containing a tracking number</returns>
		[DbQuery]
		public ICollection<WorkspaceDTO> GetAllWorkspacesWithTrackingNumbers()
		{
			ICollection<WorkspaceDTO> toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					toReturn = (from w in gbe.Workspaces.Where(w => !string.IsNullOrEmpty(w.TrackingNumber))
								select new WorkspaceDTO
								{
									Id = w.WorkspaceID,
									CostVolumeLeadPricerUserID = w.CostVolumeLeadPricerUserID,
									ProposalSubmittalDate = w.ProposalSubmitDate,
									WorkspaceName = w.WorkspaceName,
									Shortname = w.WorkspaceShortName,
									TrackingNumber = w.TrackingNumber,
									UpdateDate = w.UpdateDT,
									ContainsOCI = w.ContainsOCI,
									AllowSearch = w.AllowSearch,
									BOEExportSortByID = w.BOEExportSortByID,
									Description = w.WorkspaceDescription,
									ContractStartDate = w.ContractStartDate,
									ContractEndDate = w.ContractEndDate,
									RFPNumber = w.RFPNumber,
									TemplateID = w.TemplateID,
									CreatedByUserID = w.CreatedByETIUserID,
									ResourceListID = w.ResourceListID ?? 0,
									PerfOrgListID = w.PerformingOrganizationListID ?? 0,
									ContainsTemplate = w.ContainsTemplate,
									NumberOfTimesExportedToProPricer = w.NumProPricerExport,
									StatusComment = w.StatusComment,
									ProposalTitle = w.ProposalTitle,
									ResourceDecimalPrecision = w.ResourcePrecision,
									DateRecalculationStarted = w.RecalculationStartedDate,
									CostDecimalPrecision = ((int?)w.CostPrecision) ?? 2,
									ProposalStatus = (ProposalStatusType)w.ProposalStatusID,
									WorkspaceState = (WorkspaceState)w.WorkspaceStateID,
									Segment = (SegmentType)(w.SegmentID ?? 0),
									ProposalClass = new PickListDto
									{
										Id = w.ProposalClassLU == null ? -1 : w.ProposalClassLU.ProposalClassID,
										Text = w.ProposalClassLU == null ? string.Empty : w.ProposalClassLU.ProposalClass
									},
									LineOfBusiness = new PickListDto
									{
										Id = w.LineOfBusiness == null ? -1 : w.LineOfBusiness.LineOfBusinessID,
										Text = w.LineOfBusiness == null ? string.Empty : w.LineOfBusiness.LineOfBusinessName
									},
									SelectedContractTypeIEnum = w.WorkspaceContractTypeXREFs.Select(wCt => wCt.ContractTypeID),
									IsUsingEquivalentPerson = w.IsUsingEquivalentPerson,
									IsUsingTM = w.IsUsingTM,
									ProjectMapType = (ProjectMapType)w.ProjectMapTypeID,
									AllowGridEdit = w.AllowGridEdit,
									CustomFieldSorting = (CustomFieldSorting)(w.CustomSorting),
									ResourceSorting = (CustomFieldSorting)(w.ResourceSorting),
									PerfOrgSorting = (CustomFieldSorting)(w.PerfOrgSorting),
									LastProPricerInstance = w.LastProPricerInstance,
									LastProPricerProposal = w.LastProPricerProposal,
									RteSizeLimit = w.RteSizeLimit,
									RevisedSubmittalDate = w.RevisedSubmittalDate,
									UsingTemplateBOE = w.TemplateBoe,
									EnableSAPConnection = w.EnableSAPConnection,
									CurrentPTMWorkspace = w.CurrentPTMWorkspace,
									CreationDate = w.WorkspaceCreationDate,
									UCOTFactor = w.UCOTFactor,
									EnableAssignTaskAuthor = w.EnableAssignTaskAuthor,
									EnableLmNavigator = w.EnableLmNavigator
								}).ToList();

					foreach (WorkspaceDTO ws in toReturn)
					{
						ws.ContractStartDate = ws.ContractStartDate.Normalize();
						ws.ContractEndDate = ws.ContractEndDate.Normalize();
						ws.SelectedContractTypes = ws.SelectedContractTypeIEnum.ToCollection();
					}
				}
			}

			return toReturn;
		}

		#endregion

		#region Restores and Copies

		/// <summary>
		/// Determine whether there is currently locked travel data stored for a workspace.
		/// </summary>
		/// <param name="workspaceId">PKID for the workspace</param>
		/// <returns>True if there is locked data in the database; false if not.</returns>
		[DbQuery]
		public bool LockedDataExists(int workspaceId)
		{
			bool isLocked = false;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities db = new GenBoeEntities())
				{
					isLocked =
						(from W in db.Workspaces
						 join B in db.BOEs on W.WorkspaceID equals B.WorkspaceID
						 join TASK in db.TravelTripTaskElements on B.BOEID equals TASK.BOEID
						 join TRIP in db.TravelTrips on TASK.TravelTripTaskElementID equals TRIP.TravelTripTaskElementID
						 where TRIP.TripLockedDT != null && W.WorkspaceID == workspaceId
						 select TRIP.TravelTripID).Any();
				}
			}

			return isLocked;
		}

		public virtual void RestoreTravelForWorkspace(int inWorkspaceId)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.restoreWorkspaceRate(inWorkspaceId);
				}
			}
		}

		public virtual void LockTravelAndResourceRatesForWorkspace(int inWorkspaceId)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.lockWorkspaceRate(inWorkspaceId);
				}
			}
		}

		/// <summary>
		/// Check states for all BOEs in the workspace.  If all draft BOEs are locked, then return the BOEID for the last-locked BOE.
		/// 
		/// Rates should not be locked-down until ALL of the BOEs have been re-locked.  To accomplish this, we need to:
		/// 
		///   - Determine whether there are any BOEs still in (unlocked) Draft state.  If not, then:
		///       - Identify the last BOE locked (by comparing timestamps from [BOE].[UpdateDT])
		///       - Have that last BOE (and ONLY that BOE) trigger the rate lockdown (to avoid concurrent lockdowns)
		/// </summary>
		/// <param name="workspaceId">Workspace ID</param>
		/// <returns>BOEID for the last-locked BOE, or null if there are BOEs still in (unlocked) draft</returns>
		[DbQuery]
		public int? GetLastLockedBOEID(int workspaceId)
		{
			int? boeID = null;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					List<int> item = gbe.BOEs.Where(
												b => b.WorkspaceID == workspaceId
												&& b.Workspace.WorkspaceStateID == (int)WorkspaceState.Locked // WS is locked
												&& !b.Workspace.BOEs.Any(wB => wB.BOEStateID == (int)BOEState.Draft) // WS does not contain any BOEs in Draft
												&& b.BOEStateID == (int)BOEState.DraftLocked) // find all locked BOEs
											.OrderByDescending(b => b.UpdateDT) // Put the last one on top
											.Select(b => b.BOEID).ToList(); // and get the ids

					if (item.Any())
					{
						boeID = item.ElementAt(0);
					}
				}
			}

			return boeID;
		}

		/// <summary>
		/// Gets the workspace email overrides.
		/// </summary>
		/// <param name="workspaceId">The workspace identifier.</param>
		/// <returns>
		/// A collection of workspace email overrides
		/// </returns>
		[DbQuery]
		public ICollection<WorkspaceEmailOverrideDTO> GetWorkspaceEmailOverrides(int workspaceId)
		{
			ICollection<WorkspaceEmailOverrideDTO> overrides;
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				overrides = (from e in gbe.WorkspaceEmailXREFs.Where(e => e.WorkspaceID == workspaceId)
							 select new WorkspaceEmailOverrideDTO
							 {
								 Id = e.WorkspaceEmailID,
								 UpdateDate = e.UpdateDT,
								 TurnOn = e.TurnOn,
								 EmailType = (EmailTypes)e.EmailID
							 }).ToList();
			}
			return overrides;
		}

		/// <summary>
		/// The ExactCopyWorkspace will copy every aspect of a workspace.
		/// </summary>
		/// <param name="inWorkspaceIDtoCopy">Id of workspace to exact copy</param>
		/// <param name="newWorkspaceName">New unique workspace name.</param>
		/// <param name="newShortName">New unique short name for the workspace.</param>
		/// <param name="costVolumeLeadPricerId">Cost Volume Lead Pricer / Estimator Id</param>
		public virtual int ExactCopyWorkspace(int inWorkspaceIDtoCopy, string newWorkspaceName, string newShortName, int costVolumeLeadPricerId)
		{
			if (inWorkspaceIDtoCopy <= 0) { throw new ArgumentNullException(nameof(inWorkspaceIDtoCopy)); }

			int outNewWorkspaceID = 0;

			try
			{
				using (StopwatchTimer sw = new StopwatchTimer(this.Log))
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						gbe.Database.CommandTimeout = 300;  // give the SP enough time to execute

						// Save the workspace identification and output format template
						DateTime? spaceSAPCutoff = null;

						if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems &&
							Utilities.IsSAPEnabledForSystem)
						{
							spaceSAPCutoff = Utilities.SAPSpaceStartDate;
						}


						outNewWorkspaceID = gbe.copyWorkspace(inWorkspaceIDtoCopy, newWorkspaceName, newShortName, costVolumeLeadPricerId, spaceSAPCutoff).FirstOrDefault().GetValueOrDefault();
					}
				}
			}
			catch (SqlException ex)
			{
				Log.Error(ex);
				throw new GeneralAppException("There was an error copying the workspace.  Contact a system administrator for assistance.");
			}

			return outNewWorkspaceID;
		}

		/// <summary>
		/// Create a copy of a previous version of a workspace
		/// </summary>
		/// <param name="workspaceId">Workspace ID</param>
		/// <param name="tempWorkspaceName">Name for the temp Workspace</param>
		/// <param name="tempWorkspaceShortName">Short Name for the temp Workspace</param>
		/// <param name="versionId">Id of the version</param>
		/// <param name="boesToCopy">Comma separated list of BOE IDs if copying select BOEs</param>
		/// <returns>ID of the temporary Workspace</returns>
		public virtual int CopyWorkspaceVersion(int workspaceId, string tempWorkspaceName, string tempWorkspaceShortName, int versionId, string boesToCopy)
		{
			int tempWorkspaceId = 0;

			try
			{
				using (StopwatchTimer sw = new StopwatchTimer(this.Log))
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						gbe.Database.CommandTimeout = 300;  // give the SP enough time to execute

						tempWorkspaceId = gbe.copyWorkspaceVersion(workspaceId, tempWorkspaceName, tempWorkspaceShortName, versionId, boesToCopy).FirstOrDefault().GetValueOrDefault();
					}
				}
			}
			catch (SqlException ex)
			{
				Log.Error(ex);
				throw new GeneralAppException("There was an error copying the workspace version.  Contact a system administrator for assistance.");
			}

			return tempWorkspaceId;
		}

		/// <summary>
		/// Get the ContainsOCI setting for the given workspace
		/// </summary>
		/// <param name="shortName">workspace shortname</param>
		/// <returns>ContainsOCI setting</returns>
		[DbQuery]
		public bool? GetWorkspaceOciSettingByShortname(string shortName)
		{
			bool? containsOci = null;

			if (!string.IsNullOrWhiteSpace(shortName))
			{
				using (StopwatchTimer sw = new StopwatchTimer(this.Log))
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						containsOci = (from w in gbe.Workspaces.Where(w => w.WorkspaceShortName == shortName)
									   select w.ContainsOCI).FirstOrDefault();
					}
				}
			}

			return containsOci;
		}

		#endregion

		#region Commits

		/// <summary>
		/// Upserts a Workspace DTO
		/// </summary>
		/// <param name="dtoToUpsert">Dto to upsert</param>
		/// <returns>Id of the saved capture</returns>
		protected override int? Upsert(WorkspaceDTO dtoToUpsert)
		{
			throw new NotImplementedException("You should not be calling Save on Workspace, use other methods");
		}

		/// <summary>
		/// Deletes a workspace dto
		/// </summary>
		/// <param name="dtoToDelete">Capture to delete.</param>
		/// <returns>Id of the deleted item.</returns>
		protected override int? Delete(WorkspaceDTO dtoToDelete)
		{
			throw new NotImplementedException("You should not be calling Save on Workspace, use other methods");
		}

		/// <summary>
		/// Soft Deletes or Restores Workspace
		/// </summary>
		/// <param name="inWorkspaceID">workspace id</param>
		/// <param name="inUpdateDT">Update Date</param>
		/// <param name="inSoftDelete">soft delete workspace</param>
		/// <param name="userID">The User Id</param>
		public virtual void UpdateDeletedStatus(int inWorkspaceID, DateTime inUpdateDT, bool inSoftDelete, int userID)
		{
			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.updateWorkspaceProcessSoftDelete(inWorkspaceID, inUpdateDT, inSoftDelete);
				}
			}

			if (Utilities.IsPTMIntegrated)
			{
				if (inSoftDelete)
				{
					// clear out ptm tracking number
					WorkspaceDTO dto = this.GetById(inWorkspaceID);
					dto.TrackingNumber = null;
					dto.Updateable = UpdateType.Upsert;
					this.SaveIdentificationAndExportFormat(userID, dto);
				}
			}
		}

		/// <summary>
		/// The SaveWorkspaceSettings will save the following items:
		/// - Inserts/Edits/Deletes to Workspace variables
		/// - Inserts/Edits Workspace Identification and Output Format Template
		/// </summary>
		/// <param name="userID">Curent user ID</param>
		/// <param name="wsToSave">workspace to save</param>
		public virtual int SaveWorkspaceSettings(int userID, WorkspaceDTO wsToSave)
		{
			if (wsToSave == null) { throw new ArgumentNullException(nameof(wsToSave)); }

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				// Save the workspace identification and output format template
				this.SaveIdentificationAndExportFormat(userID, wsToSave);

				// Save the workspace allow search bit
				this.SaveAllowSearch(wsToSave);
			}

			return wsToSave.Id;
		}

		/// <summary>
		/// Inserts a Multi Clin and WBS into the workspace if they dont already have one. 
		/// </summary>
		/// <param name="workspaceID">Workspace ID</param>
		public virtual void InsertDefaultMutliValues(int workspaceID)
		{
			if (workspaceID <= 0)
			{
				this.Log.Error("The workspace id was negative");
			}
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.insertDefaultMultiClinWBS(workspaceID);
			}
		}

		/// <summary>
		/// Inserts the report XML.
		/// </summary>
		/// <param name="nonce">The nonce.</param>
		/// <param name="reportXml">The report XML.</param>
		public virtual void InsertReportXml(string nonce, string reportXml)
		{
			if (nonce == null)
			{
				throw new ArgumentNullException(nameof(nonce));
			}

			if (reportXml == null)
			{
				throw new ArgumentNullException(nameof(reportXml));
			}

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.insertReportXml(nonce, reportXml);
			}
		}

		/// <summary>
		/// Saves the workspace email overrides.
		/// </summary>
		/// <param name="emailOverrides">The email overrides.</param>
		/// <param name="workspaceId">The workspace identifier.</param>
		/// <exception cref="GenValidationException">UpdateType was not set on the Email Override.</exception>
		public void SaveWorkspaceEmailOverrides(ICollection<WorkspaceEmailOverrideDTO> emailOverrides, int workspaceId)
		{
			if (emailOverrides == null)
			{
				throw new ArgumentNullException(nameof(emailOverrides));
			}

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				foreach (WorkspaceEmailOverrideDTO emailOverride in emailOverrides)
				{
					switch (emailOverride.Updateable)
					{
						case UpdateType.Deleted:
							gbe.deleteWorkspaceEmail(emailOverride.Id, emailOverride.UpdateDate);
							break;
						case UpdateType.Upsert:
							gbe.upsertWorkspaceEmail(emailOverride.Id, (int)emailOverride.EmailType, workspaceId, emailOverride.TurnOn, emailOverride.UpdateDate);
							break;
						default:
							throw new GenValidationException("UpdateType was not set on the Email Override.");
					}
				}
			}
		}

		/// <summary>
		/// Save the data within Workspace Identification and the Output Format Template
		/// </summary>
		/// <param name="userID">Curent user ID</param>
		/// <param name="wsToSave">the workspace identification and output format to save</param>
		public virtual void SaveIdentificationAndExportFormat(int userID, WorkspaceDTO wsToSave)
		{
			// check if the input is null
			if (wsToSave == null) { throw new ArgumentNullException(nameof(wsToSave)); }

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				// assemble comma separated string for Contract Types
				string contractTypesList = null;
				if (wsToSave.SelectedContractTypes != null)
				{
					contractTypesList = string.Join(",", wsToSave.SelectedContractTypes.Select(i => i));
				}

				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					// Save the workspace variable
					int resultID = Convert.ToInt32(gbe.upsertWorkspace(
					   wsToSave.Id,
					   wsToSave.WorkspaceName,
					   wsToSave.Shortname.Trim(),
					   Convert.ToInt32(wsToSave.WorkspaceState),
					   wsToSave.ContractStartDate,
					   wsToSave.ContractEndDate,
					   wsToSave.ProposalSubmittalDate,
					   wsToSave.Description,
					   wsToSave.CostVolumeLeadPricerUserID,
					   wsToSave.RFPNumber,
					   wsToSave.TemplateID,
					   wsToSave.ContainsOCI,
					   wsToSave.TrackingNumber,
					   wsToSave.ContainsTemplate,
					   wsToSave.NumberOfTimesExportedToProPricer,
					   userID,
					   Convert.ToInt32(wsToSave.ProposalStatus),
					   wsToSave.StatusComment,
					   wsToSave.PerfOrgListID,
					   wsToSave.ResourceListID,
					   wsToSave.UpdateDate,
					   wsToSave.BOEExportSortByID,
					   Convert.ToInt32(wsToSave.Segment),
					   // Note: When running with CompanyConfiguration set to "SpaceSystems", LineOfBusiness may not be set.  In this case, store null in the field.
					   wsToSave.LineOfBusiness == null || wsToSave.LineOfBusiness.Id == -1 ? (int?)null : wsToSave.LineOfBusiness.Id,
					   // Note: When running with CompanyConfiguration set to "SpaceSystems", ContractType may not be set.  In this case, store null in the field.
					   contractTypesList,
					   // Note: When running with CompanyConfiguration set to "SpaceSystems", ProposalClass may not be set.  In this case, store null in the field.
					   wsToSave.ProposalClass == null || wsToSave.ProposalClass.Id == Constants.PROPOSAL_CLASS_TYPE_NOT_SET ? (int?)null : wsToSave.ProposalClass.Id,
					   wsToSave.ProposalTitle,
					   wsToSave.ResourceDecimalPrecision,
					   wsToSave.DateRecalculationStarted,
					   (byte)wsToSave.CostDecimalPrecision,
					   wsToSave.IsUsingEquivalentPerson,
					   wsToSave.IsUsingTM,
					   (int)wsToSave.ProjectMapType,
					   wsToSave.AllowGridEdit,
					   (int)wsToSave.CustomFieldSorting,
					   (int)wsToSave.ResourceSorting,
					   (int)wsToSave.PerfOrgSorting,
						wsToSave.LastProPricerInstance,
						wsToSave.LastProPricerProposal,
						wsToSave.RteSizeLimit,
						wsToSave.RevisedSubmittalDate,
						wsToSave.UsingTemplateBOE,
						wsToSave.EnableSAPConnection,
						wsToSave.CurrentPTMWorkspace,
						wsToSave.UCOTFactor,
						wsToSave.EnableAssignTaskAuthor,
						wsToSave.EnableLmNavigator).FirstOrDefault());

					// if the result ID is not a positive number, something bad went wrong so Log it
					if (resultID <= 0)
					{
						this.Log.Error("The returned ID from upsertWorkspace SP was negative");
					}
					else
					{
						// Set the new workspace Id
						wsToSave.Id = resultID;
					}
				}

				if (wsToSave.ContainsOCI == true)
				{
					wsToSave.AllowSearch = false;
					this.SaveAllowSearch(wsToSave);
				}
			}
		}

		/// <summary>
		/// Save Allow Search
		/// </summary>
		/// <param name="inWorkspace">workspace to save</param>
		public virtual void SaveAllowSearch(WorkspaceDTO inWorkspace)
		{
			if (inWorkspace == null) { throw new ArgumentNullException(nameof(inWorkspace)); }

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.updateWorkspaceAllowSearch(inWorkspace.Id, inWorkspace.AllowSearch, inWorkspace.ContainsTemplate);
				}
			}
		}

		/// <summary>
		/// If the workspace performing organization list has been restored, set performing organization change flag to false
		/// </summary>
		/// <param name="inWorkspace"></param>
		/// <param name="inPerfOrgChangeFlag"></param>
		public virtual void UpdatePerfOrgChangeFlag(WorkspaceDTO inWorkspace, bool inPerfOrgChangeFlag)
		{
			if (inWorkspace == null) { throw new ArgumentNullException(nameof(inWorkspace)); }

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					gbe.updateWorkspacePerformingOrganizationFlag(inWorkspace.Id, inPerfOrgChangeFlag, inWorkspace.UpdateDate);
				}
			}
		}

		/// <summary>
		/// Updates the favorite.
		/// </summary>
		/// <param name="workspaceId">The workspace identifier.</param>
		/// <param name="userId">The user identifier.</param>
		/// <param name="isFavorite">if set to <c>true</c> [is favorite].</param>
		public void UpdateFavorite(int workspaceId, int userId, bool isFavorite)
		{
			lock (userLock)
			{
				using (StopwatchTimer sw = new StopwatchTimer(this.Log))
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						gbe.upsertWorkspaceFavorite(workspaceId, userId, isFavorite);
					}
				}
			}
		}

		/// <summary>
		/// Updates the last accessed.
		/// </summary>
		/// <param name="workspaceId">The workspace identifier.</param>
		/// <param name="userId">The user identifier.</param>
		public void UpdateLastAccessed(int workspaceId, int userId)
		{
			lock (userLock)
			{
				using (StopwatchTimer sw = new StopwatchTimer(this.Log))
				{
					using (GenBoeEntities gbe = new GenBoeEntities())
					{
						DateTime now = DateTime.Now;
						DateTime updateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local);

						gbe.upsertWorkspaceLastAccessed(workspaceId, userId, updateTime);
					}
				}
			}
		}

		#endregion
	}
}