// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Transactions;
	using System.Web.Configuration;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.IESSAPClient;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.Workspace;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.DataBridge.Reference;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using IES.Common.PickList;
	using MoreLinq;
	using static IES.Common.Constants;

	public abstract class WorkspaceControllerLogic : IWorkspaceControllerLogic
	{
		#region Protected Properties and Constructor

		private readonly IProjectMapDataLoader projectMapDataLoader;
		private readonly ITMResourceRateDTODataLoader tmResourceRateLoader;
		private readonly BoeTaskElementRecalculation boeTaskElementRecalculation;
		private readonly IInUseDataLoader inUseDataLoader;
		private readonly IFullObjectFactory factory;
		private readonly ICommonDataMapper commonDataMapper;
		private readonly FullBoeDataImporter fullBoeImporter;
		private readonly IBOELaborControllerLogic boeLaborControllerLogic;
		private readonly IWorkspaceVariableDTODataLoader workspaceVariableLoader;
		private readonly ICustomFieldValueDTODataLoader customFieldValueLoader;
		private readonly ICustomFieldDTODataLoader customFieldLoader = null;
		private readonly IPickListMapper boePickListMapper;
		private readonly IPickListMapper ptmPickListMapper;
		private readonly IMoqTypeDataLoader moqTypeLoader;
		private readonly IBoeMediator boeMediator;

		/// <summary>
		/// Boe State Machine
		/// </summary>
		private readonly IBOEStateMachine boeStateMachine;

		/// <summary>
		/// Permission Loader
		/// </summary>
		private IPermissionsDTODataLoader PermissionLoader { get; }

		/// <summary>
		/// Full WS Recalculation
		/// </summary>
		private IFullWorkspaceRecalculation FullWsRecalc { get; }

		/// <summary>
		/// Workspace Loader
		/// </summary>
		private IWorkspaceDTODataLoader WorkspaceLoader { get; }

		/// <summary>
		/// Exposes the <see cref="IUserDTODataLoader"/> to derived classes
		/// </summary>
		protected IUserDTODataLoader UserLoader { get; }

		/// <summary>
		/// Exposes the <see cref="IResourceDTODataLoader"/> to derived classes
		/// </summary>
		protected IResourceDTODataLoader ResourceLoader { get; }

		/// <summary>
		/// Contact Type Loader
		/// </summary>
		private readonly ContractTypeLoader contractTypeLoader;

		/// <summary>
		/// Workspace Exporter
		/// </summary>
		private readonly WorkspaceExporter workspaceExporter;

		/// <summary>
		/// The PTM LOB conversion error.
		/// </summary>
		private const string PTM_LOB_CONVERSION_ERROR = "The LOB in PTM is invalid, update the Proposal in PTM to use a valid LOB.";

		/// <summary>
		/// The PTM contract type conversion error
		/// </summary>
		private const string PTM_CONTRACT_TYPE_CONVERSION_ERROR = "The Contract Type in PTM is invalid, update the Proposal in PTM to use a valid Contract Type.";

		/// <summary>
		/// The PTM proposal class conversion error
		/// </summary>
		private const string PTM_PROPOSAL_CLASS_CONVERSION_ERROR = "The Proposal Class in PTM is invalid, update the Proposal in PTM to use a valid Proposal Class.";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="workspaceLoader">The workspace loader.</param>
		/// <param name="inuserLoader">The inuser loader.</param>
		/// <param name="inResourceLoader">The resource loader.</param>
		/// <param name="inTMResourceRateDTODataLoader">The in tm resource rate dto data loader.</param>
		/// <param name="inBoeTaskElementRecalc">The boe task element recalc.</param>
		/// <param name="inUseDataLoader">The use data loader.</param>
		/// <param name="factory">The factory.</param>
		/// <param name="inCommonDataMapper">The common data mapper.</param>
		/// <param name="inPermissionLoader">The permission loader.</param>
		/// <param name="inBoeImporter">The boe importer.</param>
		/// <param name="inBOELaborControllerLogic">The boe labor controller logic.</param>
		/// <param name="fullWsRecalc">The full ws recalc.</param>
		/// <param name="inWorkspaceVariableLoader">The workspace variable loader.</param>
		/// <param name="inCustomFieldValueLoader">The custom field value loader.</param>
		/// <param name="customFieldLoader">The custom field loader.</permission>
		/// <param name="projectMapDataLoader">The project map data loader.</param>
		/// <param name="boePickListMapper">The BOE pick list mapper.</param>
		/// <param name="ptmPickListMapper">The PTM pick list mapper.</param>
		/// <param name="contractTypeLoader">Contract Type Loader</param>
		/// <param name="workspaceExporter">WS Exporter</param>
		/// <param name="moqTypeLoader">Moq Type Loader</param>
		/// <param name="boeStateMachine">Boe State Machine</param>
		/// <param name="boeMediator">The BOE Mediator</param>
		protected WorkspaceControllerLogic(
			IWorkspaceDTODataLoader workspaceLoader,
			IUserDTODataLoader inuserLoader,
			IResourceDTODataLoader inResourceLoader,
			ITMResourceRateDTODataLoader inTMResourceRateDTODataLoader,
			BoeTaskElementRecalculation inBoeTaskElementRecalc, IInUseDataLoader inUseDataLoader,
			IFullObjectFactory factory,
			ICommonDataMapper inCommonDataMapper, IPermissionsDTODataLoader inPermissionLoader,
			FullBoeDataImporter inBoeImporter,
			IBOELaborControllerLogic inBOELaborControllerLogic, IFullWorkspaceRecalculation fullWsRecalc,
			IWorkspaceVariableDTODataLoader inWorkspaceVariableLoader,
			ICustomFieldValueDTODataLoader inCustomFieldValueLoader,
			ICustomFieldDTODataLoader customFieldLoader,
			IProjectMapDataLoader projectMapDataLoader,
			IPickListMapper boePickListMapper,
			IPickListMapper ptmPickListMapper,
			ContractTypeLoader contractTypeLoader,
			WorkspaceExporter workspaceExporter,
			IMoqTypeDataLoader moqTypeLoader,
			IBOEStateMachine boeStateMachine,
			IBoeMediator boeMediator)
		{
			this.WorkspaceLoader = workspaceLoader;
			this.UserLoader = inuserLoader;
			this.ResourceLoader = inResourceLoader;
			this.tmResourceRateLoader = inTMResourceRateDTODataLoader;
			this.boeTaskElementRecalculation = inBoeTaskElementRecalc;
			this.inUseDataLoader = inUseDataLoader;
			this.factory = factory;
			this.commonDataMapper = inCommonDataMapper;
			this.PermissionLoader = inPermissionLoader;
			this.fullBoeImporter = inBoeImporter;
			this.boeLaborControllerLogic = inBOELaborControllerLogic;
			this.FullWsRecalc = fullWsRecalc;
			this.workspaceVariableLoader = inWorkspaceVariableLoader;
			this.customFieldValueLoader = inCustomFieldValueLoader;
			this.customFieldLoader = customFieldLoader;
			this.projectMapDataLoader = projectMapDataLoader;
			this.boePickListMapper = boePickListMapper;
			this.ptmPickListMapper = ptmPickListMapper;
			this.contractTypeLoader = contractTypeLoader;
			this.workspaceExporter = workspaceExporter;
			this.moqTypeLoader = moqTypeLoader;
			this.boeStateMachine = boeStateMachine;
			this.boeMediator = boeMediator;
		}

		#endregion

		#region Get Actions

		/// <summary>
		/// Finds the Adjacent BOEs based on the sort column/order
		/// </summary>
		/// <param name="modelView">The workspace homepage modelview</param>
		/// <param name="boeId">The boe to find adjacent boes for.</param>
		/// <param name="sortColumn">The current sort column.</param>
		/// <param name="sortOrder">The current sort order.</param>
		/// <returns></returns>
		public AdjacentItems FindAdjacentBoes(HomeWorkspaceGridModelView modelView, int boeId, string sortColumn, SortOrder sortOrder)
		{
			if (modelView == null)
			{
				throw new ArgumentNullException(nameof(modelView));
			}

			if (boeId <= 0)
			{
				throw new ArgumentException("BoeId must be valid");
			}

			int?[] orderedIds;

			switch (sortColumn)
			{
				case "Recent":
					orderedIds = modelView.items.OrderByDescending(i => i.UpdateDate).ThenBy(i => i.WBSText).Select(i => i.BOEID).Reverse().ToArray();
					break;
				case "BOETitle":
					orderedIds = modelView.items.OrderBy(i => i.BOETitle).Select(i => i.BOEID).ToArray();
					break;
				case "CLINText":
					orderedIds = modelView.items.OrderBy(i => i.CLINText).ThenBy(i => i.WBSText).ThenBy(i => i.AuthorOrderName).Select(i => i.BOEID).ToArray();
					break;
				case "isMaterial":
					orderedIds = modelView.items.OrderBy(i => i.isMaterial).Select(i => i.BOEID).ToArray();
					break;
				case "AuthorOrderName":
					orderedIds = modelView.items.OrderBy(i => i.AuthorOrderName).Select(i => i.BOEID).ToArray();
					break;
				case "ApproverOrderName":
					orderedIds = modelView.items.OrderBy(i => i.ApproverOrderName).Select(i => i.BOEID).ToArray();
					break;
				case "Status":
					orderedIds = modelView.items.OrderBy(i => i.Status).Select(i => i.BOEID).ToArray();
					break;
				case "UpdateDate":
					orderedIds = modelView.items.OrderByDescending(i => i.UpdateDate).Select(i => i.BOEID).ToArray();
					break;
				case "WBSText":
				default:
					orderedIds = modelView.items.OrderBy(i => i.WBSText).ThenBy(i => i.CLINText).ThenBy(i => i.AuthorOrderName).Select(i => i.BOEID).ToArray();
					break;
			}

			if (sortOrder == SortOrder.Descending)
			{
				orderedIds = orderedIds.Reverse().ToArray();
			}

			AdjacentItems adjacentItems = new AdjacentItems();
			int? previousId = null;

			for (int i = 0; i < orderedIds.Length; i++)
			{
				int? currentId = orderedIds[i];

				if (currentId == boeId)
				{
					adjacentItems.PreviousId = previousId;

					if (orderedIds.Length > i + 1)
					{
						adjacentItems.NextId = orderedIds[i + 1];
					}
				}
				else
				{
					previousId = currentId;
				}
			}

			return adjacentItems;
		}

		/// <summary>
		/// Creates the <see cref="IWorkspaceIdentificationModelView"/> for the IS&amp;GS mode
		/// </summary>
		/// <param name="workspace">The <see cref="FullWorkspace"/> used to populate the model view</param>
		/// <returns>A populated <see cref="IWorkspaceIdentificationModelView"/> for the IS&amp;GS mode</returns>
		public abstract IWorkspaceIdentificationModelView GetWorkspaceIdentificationModelView(FullWorkspace workspace);

		/// <summary>
		/// Creates the Workspace Model View.
		/// </summary>
		/// <returns>A new Create Workspace Model View</returns>
		public abstract ICreateWorkspaceModelView GetCreateWorkspaceModelView();

		/// <summary>
		///  Gets all Workspace Resource Rate DTOs and converts them to Model Views
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="modelView"></param>
		/// <returns>WorkspaceResourceRateGridTMModelView</returns>
		public WorkspaceResourceRateGridTMModelView GetWorkspaceResourceRateGridTMModelView(FullWorkspace workspace, WorkspaceResourceRateGridTMModelView modelView)
		{
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			if (modelView == null)
			{
				modelView = new WorkspaceResourceRateGridTMModelView();
			}

			Collection<WorkspaceResourceRateTMModelView> matchingModelViews = new Collection<WorkspaceResourceRateTMModelView>();

			ICollection<TMResourceRateDTO> allTMResourceRates = this.tmResourceRateLoader.GetByWorkspaceId(workspace.Id);

			ICollection<int> resourceIDs = (from r in allTMResourceRates
											select r.ResourceID).Distinct().ToList();

			//Retrieve all the Workspace Resource DTOs at once.
			ICollection<ResourceDTO> workspaceResourceDTOs = this.ResourceLoader.GetByIds(resourceIDs);

			if (workspaceResourceDTOs != null)
			{
				//Determine what Rates are in use all at once.
				ICollection<int> resourcesInUse = this.inUseDataLoader.GetWorkspaceResourceInUseByMultipleResources(resourceIDs, workspace.ResourceListID);

				foreach (TMResourceRateDTO resourceRateDTO in allTMResourceRates)
				{
					ResourceDTO workspaceResourceDTO = (from r in workspaceResourceDTOs
														where r.Id == resourceRateDTO.ResourceID
														select r).FirstOrDefault();

					if (workspaceResourceDTO != null)
					{
						// only grab ones that pass the current filter
						if (!string.IsNullOrEmpty(modelView.SearchFilter) && workspaceResourceDTO.ResourceName.ContainsEquivalent(modelView.SearchFilter) ||
							string.IsNullOrEmpty(modelView.SearchFilter))
						{
							//Determine if resource is in use.
							int? tempResource = (from r in resourcesInUse
												 where r == resourceRateDTO.ResourceID
												 select r).FirstOrDefault();

							bool inUse = tempResource.Value > 0;
							WorkspaceResourceRateTMModelView resourceRateTMModelView = this.CreateWorkspaceResourceRateTMModelView(resourceRateDTO, workspaceResourceDTO, inUse);
							matchingModelViews.Add(resourceRateTMModelView);
						}
					}
				}

				// Sort
				if (modelView.isSortingRequested())
				{
					switch (modelView.SortField)
					{
						case WorkspaceResourceRateTMModelView.SORT_ID_RESOURCE_RATE:
							matchingModelViews =
								new Collection<WorkspaceResourceRateTMModelView>(matchingModelViews.OrderBy(w => Convert.ToDecimal(w.ResourceRate), modelView.Order).ThenBy(w => w.StartDate.ToDateTime(), modelView.Order).ToArray());
							break;
						case WorkspaceResourceRateTMModelView.SORT_ID_START_DATE:
							matchingModelViews =
								new Collection<WorkspaceResourceRateTMModelView>(matchingModelViews.OrderBy(w => w.StartDate.ToDateTime(), modelView.Order).ThenBy(w => w.ResourceName, modelView.Order).ToArray());
							break;
						case WorkspaceResourceRateTMModelView.SORT_ID_END_DATE:
							matchingModelViews =
								new Collection<WorkspaceResourceRateTMModelView>(matchingModelViews.OrderBy(w => w.EndDate.ToDateTime(), modelView.Order).ThenBy(w => w.ResourceName, modelView.Order).ToArray());
							break;
						case WorkspaceResourceRateTMModelView.SORT_ID_RESOURCE_DESCRIPTION:
							matchingModelViews =
								new Collection<WorkspaceResourceRateTMModelView>(matchingModelViews.OrderBy(w => w.ResourceDescription, modelView.Order).ThenBy(w => w.StartDate.ToDateTime(), modelView.Order).ToArray());
							break;
						default:
							matchingModelViews =
								new Collection<WorkspaceResourceRateTMModelView>(matchingModelViews.OrderBy(w => w.ResourceName, modelView.Order).ThenBy(w => w.StartDate.ToDateTime(), modelView.Order).ToArray());
							break;
					}
				}

				// store the paged results according to sorted order
				modelView.PagedIndexes = new Collection<int>(matchingModelViews.Select(x => x.ResourceRateID).ToArray());

				modelView.WorkspaceResourceRateTMResults = new Collection<WorkspaceResourceRateTMModelView>();

				//grab the correct set of data using the indexes given the current page you are on.
				for (int i = modelView.StartArrayIndex; i <= modelView.EndArrayIndex; i++)
				{
					if (i > -1 && i < matchingModelViews.Count)
					{
						modelView.WorkspaceResourceRateTMResults.Add(matchingModelViews[i]);
					}
				}
			}

			return modelView;
		}

		/// <summary>
		/// GetWorkspaceResourceRateTMModelView
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="workspaceResourceRateID"></param>
		/// <returns>WorkspaceResourceRateTMModelView</returns>
		public WorkspaceResourceRateTMModelView GetWorkspaceResourceRateTMModelView(FullWorkspace workspace, int workspaceResourceRateID)
		{
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}
			TMResourceRateDTO rateDTO = this.tmResourceRateLoader.GetById(workspaceResourceRateID);

			WorkspaceResourceRateTMModelView modelView = this.CreateWorkspaceResourceRateTMModelView();
			modelView.ResourceRateID = rateDTO.ResourceRateID;
			modelView.UpdateDate = rateDTO.UpdateDate;
			modelView.ResourceID = rateDTO.ResourceID;
			if (!rateDTO.StartDate.HasValue || !rateDTO.EndDate.HasValue || !rateDTO.ResourceRate.HasValue)
			{
				throw new GeneralAppException("An invalid workspace resource rate has been entered into the database.");
			}
			modelView.StartDate = rateDTO.StartDate.Value.ToMonthString();
			modelView.EndDate = rateDTO.EndDate.Value.ToMonthString();
			modelView.ResourceRate = rateDTO.ResourceRate.Value.ToString();

			return modelView;
		}

		/// <summary>
		/// Returns a dictionary of resource primary keys and ids for the workspace.  This is specific subset for the workspace resource rate page.
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public abstract IDictionary<string, Tuple<bool, int>> GetWorkspaceResourcesForWorkspaceResourceRateTM(FullWorkspace workspace);

		/// <summary>
		/// Creates a new model view for Space specific WorkspaceResourceRateTMModelView 
		/// </summary>
		/// <returns></returns>
		public abstract WorkspaceResourceRateTMModelView CreateWorkspaceResourceRateTMModelView();

		/// <summary>
		/// Creates a new model view for Space specific WorkspaceResourceRateTMModelView 
		/// </summary>
		/// <returns></returns>
		public abstract WorkspaceResourceRateTMModelView CreateWorkspaceResourceRateTMModelView(TMResourceRateDTO resourceRateDTO, ResourceDTO workspaceResourceDTO, bool inUse);

		/// <summary>
		/// Get the Model Views for the Custom Fields grid
		/// </summary>
		/// <param name="customFields">Custom Fields</param>
		/// <param name="workspaceId">The workspace Id.</param>
		/// <returns>Model Views for the Custom Fields grid</returns>
		public virtual ICollection<BOECustomFieldsGridModelView> GetCustomFieldsGridModelViews(ICollection<CustomFieldDTO> customFields, int workspaceId)
		{
			if (customFields == null)
			{
				throw new ArgumentNullException(nameof(customFields));
			}

			Collection<BOECustomFieldsGridModelView> toReturn = new Collection<BOECustomFieldsGridModelView>();

			if (customFields.Any())
			{
				// refresh in use flag
				this.customFieldValueLoader.RefreshCustomFieldInUseByWorkspaceID(workspaceId);

				ICollection<CustomFieldValueDTO> allCustomFieldValues = this.customFieldValueLoader.GetCustomFieldValueDTOsByCustomFieldIds(customFields.Select(i => i.Id).ToCollection<int>());
				foreach (CustomFieldDTO customField in customFields)
				{
					BOECustomFieldsGridModelView newModelView = new BOECustomFieldsGridModelView(customField)
					{
						inUse = allCustomFieldValues.Any(c => c.CustomFieldID == customField.Id && c.CustomFieldValueInUseFlag)
					};
					toReturn.Add(newModelView);
				}
			}

			return toReturn;
		}

		#endregion Get Actions

		#region Extra Validation

		/// <summary>
		/// Determines whether [is workspace resource date within contract] [the specified workspace].
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <param name="dateString">The date string.</param>
		/// <returns>
		///   <c>true</c> if [is workspace resource date within contract] [the specified workspace]; otherwise, <c>false</c>.
		/// </returns>
		public bool IsWorkspaceResourceDateWithinContract(FullWorkspace workspace, string dateString)
		{
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			DateTime date;

			if (!DateTime.TryParse(dateString, out date))
			{
				return true; // returning true because this error is displayed in a different message.
			}

			date = GenBOEUtilities.AdjustDateTimePrecision(date, DateTimePrecision.Month);

			if (date >= GenBOEUtilities.AdjustDateTimePrecision(workspace.ContractStartDate, DateTimePrecision.Month) && date <= GenBOEUtilities.AdjustDateTimePrecision(workspace.ContractEndDate, DateTimePrecision.Month))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Checks whether the proposed T M resource rate date window overlaps any existing rate windows.
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="currentResourceRateID">Rate schedule entry PK</param>
		/// <param name="resourceID">Resource ID</param>
		/// <param name="startDateString">Proposed start date</param>
		/// <param name="endDateString">Proposed end date</param>
		/// <returns>True if OK; false if overlap exists</returns>
		public bool IsWorkspaceResourceRateDateValidWithoutOverlapTM(FullWorkspace workspace, int currentResourceRateID, int resourceID, string startDateString, string endDateString)
		{
			bool valid = true;

			DateTime proposedStartDate;
			DateTime proposedEndDate;

			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			// if parsing error, then return true because those errors are displayed in a different message.
			if (!DateTime.TryParse(startDateString, out proposedStartDate) || !DateTime.TryParse(endDateString, out proposedEndDate))
			{
				return true;
			}

			proposedStartDate = proposedStartDate.Normalize();
			proposedEndDate = proposedEndDate.Normalize();

			ICollection<TMResourceRateDTO> currentWorkspaceRates = this.tmResourceRateLoader.GetByWorkspaceId(workspace.Id)
				.Where(x => x.ResourceRateID != currentResourceRateID && x.ResourceID == resourceID).ToArray();

			foreach (TMResourceRateDTO resourceRate in currentWorkspaceRates)
			{
				if (resourceRate.StartDate.HasValue && resourceRate.EndDate.HasValue)
				{
					DateTime existingStartDate = resourceRate.StartDate.Value.Normalize();
					DateTime existingEndDate = resourceRate.EndDate.Value.Normalize();

					if ((proposedStartDate < existingStartDate && proposedEndDate < existingStartDate) ||
						(proposedStartDate > existingEndDate && proposedEndDate > existingEndDate))
					{
						// NO overlap
					}
					else
					{
						valid = false;
						break;
					}
				}
			}
			return valid;
		}


		#endregion Extra Validation

		#region Save Actions

		/// <summary>
		/// SaveWorkspaceResourceRatesTM
		/// </summary>
		/// <param name="workspace"></param>
		/// <param name="inWorkspaceResourceRateTMModelViews"></param>
		/// <returns>bool</returns>
		public bool SaveWorkspaceResourceRatesTM(FullWorkspace workspace, Collection<WorkspaceResourceRateTMModelView> inWorkspaceResourceRateTMModelViews)
		{
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			if (inWorkspaceResourceRateTMModelViews == null)
			{
				throw new ArgumentNullException(nameof(inWorkspaceResourceRateTMModelViews));
			}

			int seedResourceRateID = -1;

			Collection<TMResourceRateDTO> toSave = new Collection<TMResourceRateDTO>();
			ICollection<TMResourceRateDTO> allResourceRates = this.tmResourceRateLoader.GetByWorkspaceId(workspace.Id);

			foreach (WorkspaceResourceRateTMModelView workspaceResourceRateModelView in inWorkspaceResourceRateTMModelViews)
			{
				TMResourceRateDTO tmResourceRateDTO;

				if (workspaceResourceRateModelView.ResourceRateID > 0)
				{
					tmResourceRateDTO = allResourceRates.FirstOrDefault(r => r.ResourceRateID == workspaceResourceRateModelView.ResourceRateID);
					DataRelationshipVerifier.VerifyDataRelation(tmResourceRateDTO, workspace.Id);
				}
				else
				{
					tmResourceRateDTO = new TMResourceRateDTO
					{
						ResourceRateID = seedResourceRateID--,
						ResourceID = workspaceResourceRateModelView.ResourceID,
						WorkspaceID = workspace.Id
					};
				}

				tmResourceRateDTO.UpdateDate = workspaceResourceRateModelView.UpdateDate;

				if (workspaceResourceRateModelView.ToDelete)
				{
					// can't delete ones that don't already exist.
					if (workspaceResourceRateModelView.ResourceRateID > 0 && !toSave.Select(x => x.ResourceRateID).Contains(tmResourceRateDTO.ResourceRateID))
					{
						tmResourceRateDTO.Updateable = UpdateType.Deleted;
						toSave.Add(tmResourceRateDTO);
					}
				}
				else
				{
					tmResourceRateDTO.StartDate = Convert.ToDateTime(workspaceResourceRateModelView.StartDate);
					tmResourceRateDTO.EndDate = Convert.ToDateTime(workspaceResourceRateModelView.EndDate);
					tmResourceRateDTO.ResourceRate = Convert.ToDecimal(workspaceResourceRateModelView.ResourceRate);

					tmResourceRateDTO.Updateable = UpdateType.Upsert;
					toSave.Add(tmResourceRateDTO);
				}
			}
			this.tmResourceRateLoader.SaveTMResourceRates(toSave);
			return true;
		}

		#endregion Save Actions

		#region Workspace Var Additional Checks

		/// <summary>
		/// Calculate any linked task elements that need to be updated based on the workspace variable
		/// </summary>
		/// <param name="inTaskElementsToSave">task elements to save</param>
		/// <param name="inWorkspace">Full Workspace</param>
		public void CalculateLinkedTaskElements(Collection<BoeTaskElementDTO> inTaskElementsToSave, FullWorkspace inWorkspace)
		{
			// Note: This method modifies the ReadOnly Collections, but the caller does not use a cached FullWorkspace

			if (inWorkspace == null)
			{
				throw new ArgumentNullException(nameof(inWorkspace));
			}

			if (inTaskElementsToSave == null)
			{
				throw new ArgumentNullException(nameof(inTaskElementsToSave));
			}

			// Setup hashsets that are queried a lot, to cut down on time in the recursive calls
			HashSet<FullBoe> wsBoes = new HashSet<FullBoe>(inWorkspace.Boes);
			HashSet<BoeDTO> originalWsBoes = new HashSet<BoeDTO>(inWorkspace.Boes.DeepClone());
			HashSet<WorkspaceVariableDTO> wsVarsHash = new HashSet<WorkspaceVariableDTO>(inWorkspace.WorkspaceVariables);
			Collection<BoeTaskElementDTO> inOtherBoeTaskElementToSave = new Collection<BoeTaskElementDTO>();

			// Setup hashsets to keep track of data from recalculation; this will need to be saved in the transaction
			HashSet<BoeTaskElementDTO> tasksToSave = new HashSet<BoeTaskElementDTO>();
			HashSet<WorkspaceVariableDTO> workspaceVariablesToSave = new HashSet<WorkspaceVariableDTO>();
			HashSet<FullBoe> boesToTransition = new HashSet<FullBoe>();

			foreach (BoeTaskElementDTO taskElement in inTaskElementsToSave)
			{
				// If we are out of date, then the previous run through this loop updated the value of the task element and we can skip it
				BoeTaskElementDTO boeTaskElementFromDB = inWorkspace.TaskElements.FirstOrDefault(i => i.Id == taskElement.Id);

				if (boeTaskElementFromDB == null || boeTaskElementFromDB.UpdateDate == taskElement.UpdateDate)
				{
					// need to save this task element
					tasksToSave.RemoveWhere(x => x.Id == taskElement.Id);
					tasksToSave.Add(taskElement);

					FullBoe boe = inWorkspace.Boes.First(i => i.Id == taskElement.BoeID);
					Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();
					Collection<WorkspaceVariableDTO> workspaceVariables = new Collection<WorkspaceVariableDTO>();

					// check if labor recalculation is needed given a task variable. if it is, it will be saved
					this.FullWsRecalc.RecalculateBasedOnSumToBOEVariable(inWorkspace, inOtherBoeTaskElementToSave,
						this.boeTaskElementRecalculation.RecalculateLaborWithBoe(boe, VariableType.Task, inWorkspace, boeTaskElements, workspaceVariables),
						ref tasksToSave, ref workspaceVariablesToSave, ref boesToTransition, wsVarsHash, wsBoes, boeTaskElements, workspaceVariables);

					// check if labor recalculation is needed given a workspace variable. if it is, it will be saved
					this.FullWsRecalc.RecalculateBasedOnSumToBOEVariable(inWorkspace, inOtherBoeTaskElementToSave,
						this.boeTaskElementRecalculation.RecalculateLaborWithBoe(boe, VariableType.Workspace, inWorkspace, boeTaskElements, workspaceVariables),
						ref tasksToSave, ref workspaceVariablesToSave, ref boesToTransition, wsVarsHash, wsBoes, boeTaskElements, workspaceVariables);
				}
			}

			#region Save the data

			this.FullWsRecalc.ValidateStateTransitionForBoesEffectedByRecalculation(inWorkspace, boesToTransition, originalWsBoes);
			this.FullWsRecalc.SaveDataEffectedByRecalculation(inWorkspace, tasksToSave, workspaceVariablesToSave, boesToTransition);
			this.FullWsRecalc.PerformStateTransitionActionsForBoesEffectedByRecalculation(inWorkspace, boesToTransition, originalWsBoes);

			#endregion
		}

		/// <summary>
		/// Takes a ModelView object and persists the data contained
		/// </summary>
		/// <param name="importedData">ModelView containing the import data</param>
		/// <param name="workspace">Workspace to import data into</param>
		/// <returns></returns>
		public bool CompleteImportWorkspaceFromExcel(ImportWorkofflineResultsModelView importedData, FullWorkspace workspace)
		{
			WorkofflineImport convertedData = this.ConvertToOfflineImportToDTO(importedData, workspace);

			IList<int> affectedBoeIds = this.fullBoeImporter.ImportAndMergeOfflineImportData(convertedData, workspace);

			bool result;
			if (affectedBoeIds == null)
			{
				result = false;
			}
			else
			{
				result = true;

				// trigger task variable recalculation (includes spread recalculation)
				foreach (int boeID in affectedBoeIds)
				{
					this.ProcessAllVariableDependencies(boeID, workspace);
				}
			}

			return result;

		}
		#region ImportWorkofflineResultsModelView mediator methods
		/// <summary>
		/// Primary mediation method for taking a WorkOfflineResultsModelView and turning it into a WorkofflineImport object
		/// usable in the Business library for persisting to database
		/// </summary>
		/// <param name="importData">Data imported from the WorkOffline spreadsheet.</param>
		/// <param name="workspace">Full workspace object.</param>
		/// <returns>Converted import data.</returns>
		private WorkofflineImport ConvertToOfflineImportToDTO(ImportWorkofflineResultsModelView importData, FullWorkspace workspace)
		{
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace), "workspace cannot be null");
			}
			if (importData == null)
			{
				throw new ArgumentNullException(nameof(importData), "importData cannot be null");
			}

			WorkofflineImport convertedData =
				new WorkofflineImport
				{
					ImportTypes = importData.ImportTypes.Cast<WorkofflineImportResult>().ToCollection()
				};

			foreach (ImportWorkofflineBOEResultsModelView boe in importData.Boes)
			{
				convertedData.ImportedBoes.Add(this.ConvertToOfflineImportBoeToDTO(boe, workspace));
			}
			return convertedData;
		}

		/// <summary>
		/// Converts imported BOE data to objects suitable for import buisness logic.
		/// </summary>
		/// <param name="boe">Imported BOE data.</param>
		/// <param name="workspace">Full workspace object.</param>
		/// <returns>Converted BOE data.</returns>
		private WorkofflineImportedBoe ConvertToOfflineImportBoeToDTO(ImportWorkofflineBOEResultsModelView boe, FullWorkspace workspace)
		{
			WorkofflineImportedBoe convertedBoe = new WorkofflineImportedBoe
			{
				Id = boe.BOEID,
				Description = boe.Description,
				DataSource = boe.SourcesOfData,
				Title = boe.Title,
				ImportTypes = new Collection<BoeImportResult>() { (BoeImportResult)boe.ImportType },
				ImportedTaskElements = this.ConvertOfflineImportTaskElementsCollectionToDTO(boe.TaskElements, workspace),
				ImportedCustomFields = this.ConvertImportWorkofflineCustomFieldModelViewCollectionToDTOs(boe.CustomFields)
			};

			return convertedBoe;
		}

		/// <summary>
		/// Converts imported task element data to objects suitable for import buisness logic.
		/// </summary>
		/// <param name="taskElementMVs">List of imported task elements data.</param>
		/// <param name="workspace">Full workspace object.</param>
		/// <returns>List of converted task element data.</returns>
		private Collection<WorkofflineImportedTaskElement> ConvertOfflineImportTaskElementsCollectionToDTO(Collection<ImportWorkofflineTaskElementResultsModelView> taskElementMVs, FullWorkspace workspace)
		{

			Collection<WorkofflineImportedTaskElement> convertedTaskElements = new Collection<WorkofflineImportedTaskElement>();
			if (taskElementMVs == null)
			{
				return convertedTaskElements;
			}

			foreach (ImportWorkofflineTaskElementResultsModelView mv in taskElementMVs)
			{
				convertedTaskElements.Add(this.ConvertImportWorkofflineTaskElementResultsModelViewToDTO(mv, workspace));
			}

			return convertedTaskElements;
		}

		/// <summary>
		/// Converts an individual imported task element data to a task element object suitable for import buisness logic.
		/// </summary>
		/// <param name="taskElement">Imported task element data.</param>
		/// <param name="workspace">Full workspace object.</param>
		/// <returns>Converted task element data.</returns>
		private WorkofflineImportedTaskElement ConvertImportWorkofflineTaskElementResultsModelViewToDTO(ImportWorkofflineTaskElementResultsModelView taskElement, FullWorkspace workspace)
		{
			if (taskElement == null)
			{
				return null;
			}

			WorkofflineImportedTaskElement te = new WorkofflineImportedTaskElement
			{
				Id = taskElement.BOETaskElementID,
				Description = taskElement.Description,
				StartDate = this.ImportedDateParser(taskElement.StartDate),
				EndDate = this.ImportedDateParser(taskElement.EndDate),
				ImportTypes = new Collection<TaskElementImportResult> { (TaskElementImportResult)taskElement.ImportType },
				MOQHoursEquation = taskElement.MOQHoursEquation,
				MOQType = (MOQType)taskElement.MOQTypeID,
				MOQText = taskElement.MOQText,
				BOETaskID = taskElement.TaskID,
				TaskTitle = taskElement.TaskTitle,
				WorkspaceVariableIDs = taskElement.WorkspaceVariableIDs,
				ImportedCustomFields = this.ConvertImportWorkofflineCustomFieldModelViewCollectionToDTOs(taskElement.CustomFields),
				ImportedResourceTypes = this.ConvertImportWorkofflineResourceCollectionToDTOs(taskElement.ResourceTypes, workspace)
			};

			return te;
		}

		/// <summary>
		/// Converts imported task element resources data to objects suitable for import buisness logic.
		/// </summary>
		/// <param name="resourceMVs">List of imported task element resources data.</param>
		/// <param name="workspace">Full workspace object.</param>
		/// <returns>List of converted task element resources data.</returns>
		private Collection<WorkofflineImportedResourceType> ConvertImportWorkofflineResourceCollectionToDTOs(Collection<ImportWorkofflineResourceTypeResultsModelView> resourceMVs, FullWorkspace workspace)
		{
			Collection<WorkofflineImportedResourceType> importedResources = new Collection<WorkofflineImportedResourceType>();
			if (resourceMVs == null)
			{
				return importedResources;
			}

			foreach (ImportWorkofflineResourceTypeResultsModelView mv in resourceMVs)
			{
				importedResources.Add(this.ConvertWorkOfflineResourceModelViewToDTO(mv, workspace));
			}

			return importedResources;
		}

		/// <summary>
		/// Converts an individual imported task element resource data to  an object suitable for import buisness logic.
		/// </summary>
		/// <param name="mv">Imported task element resource data.</param>
		/// <param name="workspace">Full workspace object.</param>
		/// <returns>Converted task element resource data.</returns>
		private WorkofflineImportedResourceType ConvertWorkOfflineResourceModelViewToDTO(ImportWorkofflineResourceTypeResultsModelView mv, FullWorkspace workspace)
		{
			if (mv == null)
			{
				return null;
			}

			WorkofflineImportedResourceType resource = new WorkofflineImportedResourceType();
			ResourceDTO resourceDto = workspace.ResourcesForWsResourceListId.FirstOrDefault(r => r.Id == mv.ResourceID);
			if (resourceDto != null)
			{
				resource.SpreadType = resourceDto.RateType == RateType.Cost ? SpreadType.Cost : SpreadType.Hours;
			}

			resource.Id = mv.BOELaborTypeID;
			resource.EndDateValue = this.ImportedDateParser(mv.EndDate);
			resource.ImportTypes = new Collection<LaborTypeImportResult>() { (LaborTypeImportResult)mv.ImportType };
			resource.PercentSpread = mv.PercentSpread;
			resource.PerformingOrgID = mv.PerformingOrgID;
			resource.ResourceID = mv.ResourceID;
			resource.SpreadCurveID = (SpreadCurves)mv.SpreadCurveID;
			resource.StartDateValue = this.ImportedDateParser(mv.StartDate);
			resource.ValueSpread = mv.ValueSpread;
			resource.HourSpreadLocked = mv.HourSpreadLocked;
			resource.PercentSpreadLocked = mv.PercentSpreadLocked;
			resource.WBSID = mv.WbsID;
			resource.CLINID = mv.ClinID;

			resource.ImportedResourceSpreads = this.ConvertImportWorkofflineResourceSpreadResultsModelViewCollectionToDTOs(mv.ResourceSpreads);
			resource.ImportedResourceSpreads.ImportTypes = mv.ResourceSpreads.ImportTypes.Cast<LaborSpreadImportResult>().ToCollection();

			resource.ImportedCustomFields = this.ConvertImportWorkofflineCustomFieldModelViewCollectionToDTOs(mv.CustomFields);

			return resource;
		}

		/// <summary>
		/// Converts the import workoffline resource spread results model view collection to dtos.
		/// </summary>
		/// <param name="resourceSpreadMvs">The resource spread MVS.</param>
		/// <returns>A collection of the converted Dtos.</returns>
		private WorkofflineImportedResourceSpreadsCollection ConvertImportWorkofflineResourceSpreadResultsModelViewCollectionToDTOs(ImportWorkofflineResourceSpreadResultsModelViewCollection resourceSpreadMvs)
		{
			WorkofflineImportedResourceSpreadsCollection resourceSpreadCollection = new WorkofflineImportedResourceSpreadsCollection();
			if (resourceSpreadMvs == null)
			{
				return resourceSpreadCollection;
			}

			foreach (ImportWorkofflineResourceSpreadResultsModelView mv in resourceSpreadMvs)
			{
				resourceSpreadCollection.Add(this.ConvertImportWorkofflineResourceSpreadResultsModelViewToDTO(mv));
			}

			return resourceSpreadCollection;
		}

		/// <summary>
		/// Converts the import workoffline resource spread results model view to dto.
		/// </summary>
		/// <param name="mv">The modelview.</param>
		/// <returns>The converted dto.</returns>
		private WorkofflineImportedResourceSpread ConvertImportWorkofflineResourceSpreadResultsModelViewToDTO(ImportWorkofflineResourceSpreadResultsModelView mv)
		{
			if (mv == null)
			{
				return null;
			}

			WorkofflineImportedResourceSpread resourceSpread =
				new WorkofflineImportedResourceSpread
				{
					Id = mv.BOELaborSpreadID,
					ImportTypes = mv.ImportTypes.Cast<LaborSpreadImportResult>().ToCollection(),
					LaborSpreadDate = this.ImportedDateParser(mv.LaborSpreadDate),
					LaborSpreadValue = mv.LaborSpreadValue
				};

			return resourceSpread;
		}

		/// <summary>
		/// Converts the import workoffline custom field model view collection to dtos.
		/// </summary>
		/// <param name="customFieldMvs">The custom field Modelviews.</param>
		/// <returns>Collection of converted DTOs</returns>
		private Collection<WorkofflineImportedCustomField> ConvertImportWorkofflineCustomFieldModelViewCollectionToDTOs(Collection<ImportWorkofflineCustomFieldModelView> customFieldMvs)
		{
			Collection<WorkofflineImportedCustomField> customFields = new Collection<WorkofflineImportedCustomField>();
			if (customFieldMvs == null)
			{
				return customFields;
			}
			foreach (ImportWorkofflineCustomFieldModelView mv in customFieldMvs)
			{
				customFields.Add(this.ConvertImportWorkofflineCustomFieldModelViewToDto(mv));
			}

			return customFields;
		}

		/// <summary>
		/// Converts the import workoffline custom field model view to dto.
		/// </summary>
		/// <param name="mv">The modelview.</param>
		/// <returns>The converted dto.</returns>
		private WorkofflineImportedCustomField ConvertImportWorkofflineCustomFieldModelViewToDto(ImportWorkofflineCustomFieldModelView mv)
		{
			if (mv == null)
			{
				return null;
			}

			WorkofflineImportedCustomField customField = new WorkofflineImportedCustomField();
			customField.ImportedCustomFieldID = mv.CustomFieldID;
			customField.ImportedCustomFieldIDDecription = mv.CustomFieldIDDecription;
			customField.ImportedCustomFieldName = mv.CustomFieldName;
			customField.ImportedCustomFieldValueID = mv.CustomFieldValueID;
			customField.IsOpenEnded = mv.IsOpenEnded;

			return customField;
		}

		#endregion

		#endregion Workspace Var Additional Checks

		#region Export Actions

		/// <summary>
		/// Gets a filtered list of Resources
		/// </summary>
		/// <param name="inWorkspaceResources">The list of resources to filter</param>
		/// <returns></returns>
		public abstract ICollection<ResourceDTO> GetFilteredWorkspaceResources(IReadOnlyCollection<ResourceDTO> inWorkspaceResources);

		/// <summary>
		/// Gets a filtered list of T&amp;M Resources
		/// </summary>
		/// <param name="workspaceResources">The list of resources to filter</param>
		/// <returns>list of T&amp;M Resources</returns>
		public ICollection<ResourceDTO> GetFilteredWorkspaceResourcesTM(IReadOnlyCollection<ResourceDTO> workspaceResources)
		{
			if (workspaceResources == null)
			{
				throw new ArgumentNullException(nameof(workspaceResources), "workspaceResources cannot be null");
			}

			ICollection<ResourceDTO> workspaceResourcesTM = (from x in workspaceResources
															 where
															   ((x.ElementOfCost == ElementOfCostType.Sub ||
																 x.ElementOfCost == ElementOfCostType.IWTA) &&
																(x.RateType == RateType.Hours))
															 select x).ToArray();
			return workspaceResourcesTM;
		}

		/// <summary>
		/// Gets a filtered list of Resources
		/// </summary>
		/// <param name="inWorkspaceResources">The list of resources to filter</param>
		/// <returns></returns>
		public abstract ICollection<ResourceDTO> GetFilteredOtherWorkspaceResources(IReadOnlyCollection<ResourceDTO> inWorkspaceResources);

		/// <summary>
		/// Get a collection of ModelView objects for BOEs that are potential candidates for Workoffline export.
		/// </summary>
		/// <param name="workspace"></param>
		/// <returns></returns>
		public ICollection<ExportBOEModelView> GetExportBOEModelData(FullWorkspace workspace)
		{
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			ICollection<ExportBOEModelView> theModelViews = new Collection<ExportBOEModelView>();

			#region Load Data from the DB

			UserDTO currentUser = workspace.CurrentActiveUser;

			HashSet<PermissionsDTO> permissions = new HashSet<PermissionsDTO>(this.PermissionLoader.GetWorkspacePermissions(workspace.Id)
				.Where(x => x.ETIUserId == currentUser.UserID && x.Role == Role.WorkspaceAdmin).ToCollection());

			bool isWorkspaceAdmin = (permissions.Any());

			ICollection<FullBoe> allBOEs = workspace.Boes.ToCollection();

			List<int> boeIds = allBOEs.Select(x => x.Id).ToList();
			HashSet<PermissionsDTO> rolesForBoes = new HashSet<PermissionsDTO>(this.PermissionLoader.GetBOEPermissions(boeIds));
			HashSet<BOEStateModelView> allBoeStates = new HashSet<BOEStateModelView>(this.commonDataMapper.getBOEStates());

			#endregion
			foreach (BoeDTO boe in allBOEs)
			{
				WbsDTO wbsDTO = boe.WBSID.HasValue ? workspace.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID.Value) : null;
				ClinDTO clinDTO = boe.CLINID.HasValue ? workspace.Clins.FirstOrDefault(x => x.Id == boe.CLINID.Value) : null;
				string status = allBoeStates.Where(x => x.BOEStateID == (int)boe.State).Select(x => x.BOEState).First();

				HashSet<PermissionsDTO> boeRoles = new HashSet<PermissionsDTO>(rolesForBoes.Where(x => x.BOEId == boe.Id).ToCollection());

				// Is current user the author
				bool isAuthor = (from r in boeRoles
								 where (r.Role == Role.Author || r.Role == Role.SubcontractorAuthor) && r.ETIUserId == currentUser.UserID
								 select r.ETIUserId).Any();

				// Only show BOEs assigned to the current user (Author).
				// Or if we are in Space mode and current user is Workspace Administrator.
				if (this.CanExportBoeForWorkoffline(isAuthor, isWorkspaceAdmin))
				{
					// add all values now so we can sort .. we'll trim later
					theModelViews.Add(new ExportBOEModelView(boe, wbsDTO, clinDTO, status, boe.isMaterial));
				}
			}

			return theModelViews;
		}

		/// <summary>
		/// Return true, if user is BOE Author.
		/// </summary>
		/// <param name="isAuthor">Is the user an author.</param>
		/// <param name="isWorkspaceAdmin">Is the user a workspace admin.</param>
		/// <returns><c>true</c> if the the author/workspace admin can export BOE for work offline; otherwise, <c>false</c>.
		/// </returns>
		public abstract bool CanExportBoeForWorkoffline(bool isAuthor, bool isWorkspaceAdmin);

		/// <summary>
		/// Gets the default ExcelReportTemplateType for a new workspace
		/// </summary>
		/// <returns>Default ExcelReportTemplateType</returns>
		public abstract ExcelReportTemplateType GetDefaultReportTemplateType();

		/// <summary>
		/// Gets the default picklist values for template types
		/// </summary>
		/// <param name="ws">Workspace</param>
		/// <returns>Collection of template types</returns>
		public abstract ICollection<ExcelReportTemplateType> GetPicklistReportTemplateTypes(FullWorkspace ws);
		#endregion

		#region Backup Export Actions

		/// <summary>
		/// Create a copy of a previous verison of the workspace
		/// </summary>
		/// <param name="ws">The Workspace</param>
		/// <param name="versionId">ID of the version</param>
		/// <param name="exportAllBoes">bool noting if all boes should be copied</param>
		/// <param name="boesToExport">list of BOE IDs if copying select BOEs</param>
		/// <returns>ID of the new temporary Workspace</returns>
		public int CopyWorkspaceVersion(FullWorkspace ws, int versionId, bool exportAllBoes, ICollection<int> boesToExport)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			// Workspace will only be around for 1 day, so just time will be unique enough for naming
			string tempWsName = ws.WorkspaceName + "-Backup-Version_" + versionId + "-" + DateTime.Now.ToString("HH:mm:ss.fff");
			string tempWsShortName = DateTime.Now.ToString("HH:mm:ss.fff-") + ws.Shortname;

			string boeList = exportAllBoes ? string.Empty : string.Join(",", boesToExport);

			int tempWorkspaceId = WorkspaceLoader.CopyWorkspaceVersion(ws.Id, tempWsName.Substring(0, Math.Min(tempWsName.Length, 115)), tempWsShortName.Substring(0, Math.Min(tempWsName.Length, 21)), versionId, boeList);

			return tempWorkspaceId;
		}

		/// <summary>
		/// Create the Workspace Data Report for a previous version of a Workspace
		/// </summary>
		/// <param name="ws">The temporary copy of the previous workspace version</param>
		/// <param name="templateFileLocation">template file location</param>
		/// <param name="originalWorkspaceName">Original Workspace name</param>
		/// <param name="versionId">Version ID</param>
		/// <returns>File location of Workspace Data Report for the previous version</returns>
		public string CreateWorkspaceDataReportForVersion(FullWorkspace ws, string templateFileLocation, MetricNameTaskElementMappingDTO metricTaskElementMappings, string originalWorkspaceName, int versionId)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			// All BOEs for the workspace as a default
			List<FullBoe> boes = ws.Boes.ToList();
			List<BoeTaskElementDTO> tasks = ws.TaskElements.OrderBy(t => t.BOETaskElementOrder).ToList();

			// Get RTE overrides
			ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplateOverrides = ws.TemplateQuestionsAndAnswers.ToList();

			bool isOffloading = ws.ProjectMapType != ProjectMapType.StandardWithoutOffload;
			if (isOffloading)
			{
				OffloadLaborRates offloader = new OffloadLaborRates();
				List<int> selectedBoeIds = boes.Select(b => b.Id).ToList();
				OffloadLaborRatesResults results = offloader.OffloadWorkspace(boes.Where(b => selectedBoeIds.Contains(b.Id)).ToList(), ws);

				boes = results.Boes.ToList();
				tasks = boes.SelectMany(b => b.TaskElements).OrderBy(t => t.BOETaskElementOrder).ToList();
				rteTemplateOverrides = boes.SelectMany(x => x.TemplateQuestionsAndAnswers).ToList();
			}

			BOEExportInputs exportInputs = new BOEExportInputs(boes, ws.Boes.ToList(), tasks, ws, rteTemplateOverrides, ws.MoqTypeSelections.ToList());
			// Need picklist values for contract type for Workspace Identification sheet
			exportInputs.ContractTypes = this.contractTypeLoader.GetPickListValues();
			ICollection<PickListDto> contractTypes = this.contractTypeLoader.GetPickListValues();

			return this.workspaceExporter.ExportToExcelFile(templateFileLocation, exportInputs, metricTaskElementMappings, contractTypes);
		}

		#endregion

		/// <summary>
		/// This method provides a basic DateTime parser that is more forgiving than those provided as exentsion methods.  
		/// It will also set the day of month to the 15th
		/// This method won't throw, but will return a DateTime.MinValue if parse fails
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public DateTime ImportedDateParser(string date)
		{
			DateTime parsedDate;

			if (date == null)
			{
				return DateTime.MinValue;
			}

			if (DateTime.TryParse(date, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedDate))
			{
				parsedDate = parsedDate.AddDays(15 - parsedDate.Day);
				parsedDate = parsedDate.AddHours(12 - parsedDate.Hour);
			}
			else
			{
				parsedDate = DateTime.MinValue;
			}

			return parsedDate;
		}

		/// <summary>
		/// Gets the Workspace Identification view name.
		/// </summary>
		public abstract string WorkspaceIdentificationViewName { get; }

		/// <summary>
		/// Populates properties with company specific data
		/// </summary>
		/// <param name="theModel">The <see cref="ICreateWorkspaceModelView"/> to populate</param>
		/// <param name="workspace">The <see cref="WorkspaceDTO"/> containing data used to populate the model view</param>
		public abstract void PopulateCompanySpecificWorkspaceProperties(ICreateWorkspaceModelView theModel, WorkspaceDTO workspace);

		/// <summary>
		/// Populates properties with company specific data
		/// </summary>
		/// <param name="theModel">The <see cref="IWorkspaceIdentificationModelView"/> to populate</param>
		/// <param name="workspace">The <see cref="WorkspaceDTO"/> containing data used to populate the model view</param>
		public abstract void PopulateCompanySpecificWorkspaceProperties(IWorkspaceIdentificationModelView theModel, WorkspaceDTO workspace);

		/// <summary>
		/// Populates properties with company specific data
		/// </summary>
		/// <param name="workspaceSearchResults">The <see cref="WorkspaceSearchResultModelView"/> to populate</param>
		public virtual void PopulateCompanySpecificProperties(WorkspaceSearchResultModelView workspaceSearchResults)
		{
			if (workspaceSearchResults == null)
			{
				throw new ArgumentNullException(nameof(workspaceSearchResults));
			}

			workspaceSearchResults.LabelLeadPricer = CommonConstants.LABEL_TEXT_LEAD_PRICER_SSC;
		}

		#region All variable dependencies

		/// <summary>
		/// Identify all task and workspace variables that reference the designated BOE.  Recalculate the values of each such variable as well as the
		/// hourly totals (and spread distributions) for any task elements that USE them.  Continue (recursively) resolving references to any
		/// NESTED variables until the process is complete.
		/// </summary>
		/// <param name="boeID">BOE PK identifier</param>
		/// <param name="ws">Workspace</param>
		public void ProcessAllVariableDependencies(int boeID, FullWorkspace ws)
		{
			// resolve the complete task-variable dependency chain (in breadth-first order)
			VariableDependencyResolutionData dependencies = this.boeLaborControllerLogic.ResolveAllVariableDependencies(boeID, ws);

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
			{
				this.boeLaborControllerLogic.ProcessAllVariableDependencies(boeID, ws, dependencies);

				scope.Complete();
			}
		}

		#endregion

		/// <summary>
		/// Full Workspace Recalculation - step 1 -> prep and recalculation, without saving data
		/// </summary>
		/// <param name="ws">WS to recalculate</param>
		/// <param name="tasksToSave">Tasks to Save will be loaded here</param>
		/// <param name="workspaceVariablesToSave">WS Variables to save will be loaded here</param>
		/// <param name="boesToTransition">Boes that needs to be transitioned to a new state will be loaded here</param>
		/// <param name="originalWsBoes">Original BOE data</param>
		/// <param name="decimalPrecisionChanged">When true, resource hours decimal precision has changed.</param>
		/// <param name="costDecimalPrecisionChanged">When true, resource cost decimal precision has changed.</param>
		/// <param name="costPrecision">The current cost decimal precision.</param>
		public void WsRecalculationStep1(FullWorkspace ws, ref HashSet<BoeTaskElementDTO> tasksToSave, ref HashSet<WorkspaceVariableDTO> workspaceVariablesToSave, ref HashSet<FullBoe> boesToTransition,
			HashSet<BoeDTO> originalWsBoes, bool decimalPrecisionChanged, bool costDecimalPrecisionChanged, int costPrecision)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			if (ws.IsProjectMapWorkspace)
			{
				this.ProjectMapAdjustPrecision(ws, decimalPrecisionChanged, costDecimalPrecisionChanged);
			}
			else
			{
				// Preload ws data so that it does not have to be done inside of a transaction
				ws.LoadClinsAndBoes();
				ws.WbsElements.Any();
				ws.ResourcesForWsResourceListId.Any();
				ws.PerformingOrgsForWsList.Any();

				if (decimalPrecisionChanged)
				{
					// Run recalculation, WITHOUT A SAVE
					// This is done first because it can take a long time and could cause the transaction to time out; new data is placed into the hashsets which need to be saved
					this.FullWsRecalc.RecalculateLaborInAWorkspaceWithoutSaving(ws, ref tasksToSave, ref workspaceVariablesToSave, ref boesToTransition);
				}

				if (costDecimalPrecisionChanged)
				{
					this.FullWsRecalc.RecalculateCostInAWorkspaceWithoutSaving(ws, ref tasksToSave, ref boesToTransition, costPrecision);
				}

				// Perform Boe state transition validation, WITHOUT ANY DATA MODIFICATION
				// If the validation fails, this method will throw an exception and will not proceed into the transaction
				this.FullWsRecalc.ValidateStateTransitionForBoesEffectedByRecalculation(ws, boesToTransition, originalWsBoes);
			}
		}

		/// <summary>
		/// Adjust precision for Project Map.
		/// </summary>
		/// <param name="ws">The ws.</param>
		/// <param name="decimalPrecisionChanged">if set to <c>true</c> [decimal precision changed].</param>
		/// <param name="costDecimalPrecisionChanged">if set to <c>true</c> [cost decimal precision changed].</param>
		public void ProjectMapAdjustPrecision(FullWorkspace ws, bool decimalPrecisionChanged, bool costDecimalPrecisionChanged)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			FullProjectMapWorkspace workspace = ws as FullProjectMapWorkspace;
			if (workspace == null)
			{
				throw new ArgumentException($"Workspace passed in was not a project map workspace with id {ws.Id}");
			}

			int id = -1;
			foreach (ProjectMapModelView model in workspace.ProjectMapData)
			{
				if (decimalPrecisionChanged && model.Hours.HasValue)
				{
					model.Hours = Utilities.AdjustPrecision(model.Hours.Value, ws.DecimalPrecision);
					if (ws.ProjectMapType == ProjectMapType.TimePhasedProjectMap && model.DiscreteMonths != null)
					{
						decimal sum = 0;
						for (int j = 0; j < model.DiscreteMonths.Length; j++)
						{
							decimal? monthValue = model.DiscreteMonths[j];
							if (monthValue.HasValue)
							{
								model.DiscreteMonths[j] = Utilities.AdjustPrecision(monthValue.Value, ws.DecimalPrecision);
								sum += model.DiscreteMonths[j].Value;
							}
						}

						model.Hours = sum;
					}
				}
				else if (costDecimalPrecisionChanged && model.Dollars.HasValue)
				{
					model.Dollars = Utilities.AdjustPrecision(model.Dollars.Value, ws.CostDecimalPrecision);
					if (ws.ProjectMapType == ProjectMapType.TimePhasedProjectMap && model.DiscreteMonths != null)
					{
						decimal sum = 0;
						for (int j = 0; j < model.DiscreteMonths.Length; j++)
						{
							decimal? monthValue = model.DiscreteMonths[j];
							if (monthValue.HasValue)
							{
								model.DiscreteMonths[j] = Utilities.AdjustPrecision(monthValue.Value, ws.CostDecimalPrecision);
								sum += model.DiscreteMonths[j].Value;
							}
						}
						model.Dollars = sum;
					}
				}

				model.Id = id--;
				model.Updateable = UpdateType.Upsert;
			}

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				this.projectMapDataLoader.DeleteAllInWs(ws.Id, ws.UpdateDate);
				this.projectMapDataLoader.BulkSave(workspace.ProjectMapData.ToList());

				scope.Complete();
			}
		}

		/// <summary>
		/// !!! This method does not do any other transitions based on the new state, it is purely for recalculation state changes !!!
		/// Changes the workspace state -> this is used because during recalculation we want to lock the workspace; 
		/// Once the recalculation is done, we want to return it back to the original state;
		/// </summary>
		/// <param name="ws">Workspace to change; The LastUpdateDate gets updated after the save so that way it can be modified and saved again, if needed</param>
		/// <param name="currentUserID">Current User Id</param>
		/// <param name="newState">State to which the WS will be changed to</param>
		/// <param name="recalculationDate">Date/Time when the recalculation started; Null if it's finished</param>
		public void ChangeTheWorkspaceStateDuringRecalculation(FullWorkspace ws, int currentUserID, WorkspaceState newState, DateTime? recalculationDate)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			// LOCK THE WS; update last saved date.. 
			// Have to do it this way because of the way the SP is written. If I try to save the new precision the state blows up..
			WorkspaceDTO oldWs = this.WorkspaceLoader.GetById(ws.Id);
			oldWs.WorkspaceState = newState;
			oldWs.DateRecalculationStarted = recalculationDate;
			oldWs.Updateable = UpdateType.Upsert;

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				this.WorkspaceLoader.SaveWorkspaceSettings(currentUserID, oldWs);

				scope.Complete();
			}

			WorkspaceDTO updatedWs = this.WorkspaceLoader.GetById(ws.Id);
			ws.UpdateDate = updatedWs.UpdateDate;
		}

		/// <summary>
		/// Updates the Zone Travel Rates to be the System default rates.
		/// </summary>
		/// <param name="workspaceId">The id for workspace to update.</param>
		public virtual void CopySystemZoneTravelRates(int workspaceId)
		{
			// DO Nothing for ISGS/Space
		}

		/// <summary>
		/// Gets the last updated time the Zone Travel was updated; Null if it is latest.
		/// </summary>
		/// <param name="workspaceId">The id for workspace to update.</param>
		public virtual DateTime? GetLastupdatedTimeZoneTravel(int workspaceId)
		{
			// Return null for ISGS/Space
			return null;
		}

		/// <summary>
		/// Gets the last updated time the Offload was updated; Null if it is latest.
		/// </summary>
		/// <param name="workspaceId">The id for workspace to update.</param>
		public virtual DateTime? GetLastupdatedTimeOffload(int workspaceId)
		{
			// Return null for Space
			return null;
		}

		/// <summary>
		/// Performs Validation for SaveWorkspaceIdentification
		/// </summary>
		/// <param name="ws">FullWorkspace</param>
		/// <param name="workspaceDetails">IWorkspaceIdentificationModelView</param>
		/// <param name="isAdmin">Is System Admin?</param>
		/// <param name="ptmTrackingNumberNotRequired">Is PTM Tracking Number Not Required</param>
		/// <returns>Collection of Validation Messages.</returns>
		public virtual ICollection<ValidationMessage> SaveWorkspaceIdentificationValidation(FullWorkspace ws, IWorkspaceIdentificationModelView workspaceDetails, bool isAdmin, bool ptmTrackingNumberNotRequired = false)
		{
			_ = ws ?? throw new ArgumentNullException(nameof(ws));
			_ = workspaceDetails ?? throw new ArgumentNullException(nameof(workspaceDetails));

			List<ValidationMessage> validationErrors = new List<ValidationMessage>();

			// validate workspace unique name
			Collection<Dictionary<string, string>> validationData = new Collection<Dictionary<string, string>> { new Dictionary<string, string>() };
			validationData.First().Add("WorkspaceID", ws.Id.ToString());
			Validator validator = ValidationFactory.Instance.getValidator(ValidationType.WorkspaceUniqueName);
			if (validator.validation(workspaceDetails.WorkspaceName, validationData).Count > 0)
			{
				validationErrors.Add(new ValidationMessage("WorkspaceNameUnique", "The Workspace Name must be unique."));
			}

			// Perform Cost Volume Lead/Pricer validation - Must be an individual (not a group), and not a subcontractor.
			validationErrors.AddRange(this.CostVolumeLeadPricerValidation(workspaceDetails.CostVolumeLeadPricerNTID));

			// Boe Template (MOQ Type) change validation
			if (ws.UsingTemplateBOE && !workspaceDetails.UsingTemplateBoe)
			{
				validationErrors.Add(new ValidationMessage("Using Template BOEs cannot be changed from 'Yes' to 'No'. The only allowed changed to this field is from 'No' to 'Yes'."));
			}

			// Validate the estimating lead/pricer's permissions
			// Edge case - if the selected user is already a lead/pricer for the WS and had create WS permissions removed afterwards, and they were not unselected during this step,
			// leave them as is
			WorkspaceDTO wsInDatabase = WorkspaceLoader.GetById(ws.Id);
			if (wsInDatabase.CostVolumeLeadPricerUserID != ws.CostVolumeLeadPricerUserID)
			{
				UserDTO costVolumeLeadDTO = this.UserLoader.GetUserByID(ws.CostVolumeLeadPricerUserID);
				// Need to send in an empty string for NTID and display name
				// The parameters below are already assuming that someone we select had permissions and then got them revoked, but are still selected; we want to restrict this
				ICollection<KeyValuePair<string, string>> createWSUsers = this.PermissionLoader.GetCreateWorkspaceRolesForPtm(string.Empty, string.Empty);
				KeyValuePair<string, string> estimatingLeadPricerKVP = new KeyValuePair<string, string>(costVolumeLeadDTO.NTID, costVolumeLeadDTO.DisplayName);
				if (!createWSUsers.Contains(estimatingLeadPricerKVP))
				{
					validationErrors.Add(new ValidationMessage("CostVolumeLeadPricerDisplayName", "Cannot add a user as the Estimating Lead/Pricer if they are not also a Workspace Creator."));
				}
			}

			return validationErrors;
		}

		/// <summary>
		/// Gets MOQ Type data when WS is changing from not using BOE Templates to using BOE Templates. This data still needs to be saved later
		/// </summary>
		/// <param name="ws">Workspace which is being saved</param>
		/// <returns>MOQ Type Data to save</returns>
		public ICollection<MoqTypeSelection> GetMoqTypesDataForBoeTemplateSettingChange(FullWorkspace ws)
		{
			_ = ws ?? throw new ArgumentNullException(nameof(ws));

			int i = -1;
			return ws.TaskElements.Where(x => x.MOQType.MapToNew(ws.CreationDate) != MOQType.None || !string.IsNullOrEmpty(x.MOQText)).Select(taskElement => new MoqTypeSelection()
			{
				Id = i--,
				Updateable = UpdateType.Upsert,
				TaskId = taskElement.Id,

				SelectedMOQType = taskElement.MOQType.MapToNew(ws.CreationDate, MOQType.Historical),
				SmeReason = taskElement.MOQType.MapToNew(ws.CreationDate, MOQType.Historical) == MOQType.SME ? taskElement.MOQText : string.Empty,
				Rationale = taskElement.MOQType.MapToNew(ws.CreationDate, MOQType.Historical) == MOQType.SME ? string.Empty : taskElement.MOQText
			}).ToList();
		}

		/// <summary>
		/// Save MOQ Types
		/// </summary>
		/// <param name="moqTypesToSave">Moq Types To Save</param>
		public void SaveMoqTypes(ICollection<MoqTypeSelection> moqTypesToSave)
		{
			_ = moqTypesToSave ?? throw new ArgumentNullException(nameof(moqTypesToSave));

			if (moqTypesToSave.Any())
			{
				this.moqTypeLoader.Save(moqTypesToSave);
			}
		}

		/// <summary>
		/// Validate the Cost Volume Lead Pricer ID is an individual and not a group.
		/// If they are an individual, verify they are not a subcontractor.
		/// Also need to ensure that the individual has Create WS permissions.
		/// </summary>
		/// <param name="inCostVolumeLeadPricerNTID">Cost Volume Lead Pricer NTID</param>
		/// <returns>Collection of Validation Messages.</returns>
		public ICollection<ValidationMessage> CostVolumeLeadPricerValidation(string inCostVolumeLeadPricerNTID)
		{
			List<ValidationMessage> errors = new List<ValidationMessage>();

			// Cost Volume Lead/Pricer must be an individual and not a group.
			Collection<Dictionary<string, string>> validationData = new Collection<Dictionary<string, string>> { new Dictionary<string, string>() };
			Validator isUserNotGroupValidator = ValidationFactory.Instance.getValidator(ValidationType.IsUserNotGroup);
			Collection<string> validationErrors = isUserNotGroupValidator.validation(inCostVolumeLeadPricerNTID, validationData);
			foreach (string ve in validationErrors)
			{
				errors.Add(new ValidationMessage("CostVolumeLeadPricerNTID", ve));
			}

			// If the Cost Volume Lead/Pricer is an individual, verify they are not a subcontractor
			if (!validationErrors.Any())
			{
				// Subcontractor users are restricted from Cost Volume Lead/Pricer permissions.
				validationData = new Collection<Dictionary<string, string>> { new Dictionary<string, string>() };
				Validator isUserNotSubcontractor = ValidationFactory.Instance.getValidator(ValidationType.IsUserNotSubcontractor);

				try
				{
					validationErrors = isUserNotSubcontractor.validation(inCostVolumeLeadPricerNTID, validationData);
				}
				catch (ArgumentNullException) { } // if the user is no longer in the system, we'll ignore the error.. the assumption is that this will be fixed by the admins.. Per Scott, 11/2015

				foreach (string ve in validationErrors)
				{
					errors.Add(new ValidationMessage("CostVolumeLeadPricerNTID", ve));
				}
			}

			// Verify that the user has create WS permissions
			UserDTO costVolumeLeadDTO = this.UserLoader.GetOrCreateUserByNtid(inCostVolumeLeadPricerNTID);
			ICollection<KeyValuePair<string, string>> createWSUsers = this.PermissionLoader.GetCreateWorkspaceRolesForPtm(string.Empty, string.Empty);
			KeyValuePair<string, string> estimatingLeadPricerKVP = new KeyValuePair<string, string>(costVolumeLeadDTO.NTID, costVolumeLeadDTO.DisplayName);
			if (!createWSUsers.Contains(estimatingLeadPricerKVP))
			{
				errors.Add(new ValidationMessage("CostVolumeLeadPricerNTID", "Cannot add a user as the Estimating Lead/Pricer if they are not also a Workspace Creator."));
			}

			return errors;
		}

		/// <summary>
		/// Deletes the given workspace variables
		/// </summary>
		/// <param name="variables">Variables to delete</param>
		public void DeleteWorkspaceVariables(Collection<WorkspaceVariableDTO> variables)
		{
			if (variables == null)
			{
				throw new ArgumentNullException(nameof(variables));
			}

			foreach (WorkspaceVariableDTO varToDelete in variables)
			{
				if (!varToDelete.InUse)
				{
					varToDelete.Updateable = UpdateType.Deleted;
				}
			}

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				this.workspaceVariableLoader.SaveWorkspaceVariables(variables.Where(x => x.Updateable != UpdateType.None).ToCollection());
				scope.Complete();
			}
		}

		/// <summary>
		/// Saves the project map data.
		/// </summary>
		/// <param name="projectMapData">The project map data.</param>
		/// <param name="workspace">The full workspace.</param>
		/// <exception cref="ArgumentNullException">
		/// workspace or projectMapData</exception>
		public void SaveProjectMapData(ICollection<ProjectMapModelView> projectMapData, FullWorkspace workspace)
		{
			if (ReferenceEquals(workspace, null))
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			if (ReferenceEquals(projectMapData, null))
			{
				throw new ArgumentNullException(nameof(projectMapData));
			}

			// Update dates on workspace
			if (projectMapData.Any())
			{
				DateTime earliestStart = projectMapData.Where(d => d.StartDate.HasValue).Select(d => d.StartDate.Value).Min().Normalize();
				DateTime latestEnd = projectMapData.Where(d => d.EndDate.HasValue).Select(d => d.EndDate.Value).Max().Normalize();
				workspace.ContractStartDate = earliestStart;
				workspace.ContractEndDate = latestEnd;
			}

			int currentUserId = workspace.CurrentActiveUser.UserID;

			Parallel.ForEach(projectMapData, (row) =>
			{
				row.Id = -1;
				row.Updateable = UpdateType.Upsert;
				row.WorkspaceId = workspace.Id;
			});

			// run the deletion and save in transaction
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("CopyWorkspaceTransactionTimeout", Constants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				// Delete old data first
				this.projectMapDataLoader.DeleteAllInWs(workspace.Id, workspace.UpdateDate);

				// Save the Project Map data
				this.projectMapDataLoader.BulkSave(projectMapData);

				// save Workspace (dates updated)
				this.WorkspaceLoader.SaveWorkspaceSettings(currentUserId, workspace);

				scope.Complete();
			}

			// Clear the cache after the project map has been saved since it is a kill/fill
			this.factory.ClearWorkspaceCache(workspace.Shortname);
		}

		/// <summary>
		/// Gets the workspace emails.
		/// </summary>
		/// <param name="workspaceId">The workspace identifier.</param>
		/// <returns>A collection of Workspace Emails.</returns>
		public ICollection<WorkspaceEmailOverrideModelView> GetWorkspaceEmails(int workspaceId)
		{
			ICollection<WorkspaceEmailOverrideDTO> workspaceDtos = this.WorkspaceLoader.GetWorkspaceEmailOverrides(workspaceId);
			ICollection<EmailModelDomain> systemEmails = this.commonDataMapper.GetEmails();
			ICollection<WorkspaceEmailOverrideModelView> modelViews = this.ConvertDtos(workspaceDtos, systemEmails);

			return modelViews;
		}

		/// <summary>
		/// Converts the email dtos into the modelview.
		/// </summary>
		/// <param name="workspaceDtos">The workspace dtos.</param>
		/// <param name="systemEmails">The system emails.</param>
		/// <returns>A merged model view of the emails from system and workspace</returns>
		private ICollection<WorkspaceEmailOverrideModelView> ConvertDtos(ICollection<WorkspaceEmailOverrideDTO> workspaceDtos, ICollection<EmailModelDomain> systemEmails)
		{
			List<WorkspaceEmailOverrideModelView> modelviews = new List<WorkspaceEmailOverrideModelView>();
			foreach (EmailModelDomain systemEmail in systemEmails)
			{
				WorkspaceEmailOverrideDTO workspaceOverride = workspaceDtos.FirstOrDefault(w => w.EmailType == systemEmail.EmailType);

				WorkspaceEmailOverrideModelView modelview = new WorkspaceEmailOverrideModelView
				{
					EmailType = systemEmail.EmailType,
					SystemDefaultOn = systemEmail.DefaultOn,
					SystemForced = systemEmail.Forced,
					Body = systemEmail.Body,
					Category = systemEmail.Category,
					Recipient = systemEmail.Recipient,
					Trigger = systemEmail.Trigger,
					Subject = systemEmail.Subject,
					Id = workspaceOverride?.Id ?? -1,
					WorkspaceOverrideOn = workspaceOverride?.TurnOn
				};

				modelviews.Add(modelview);
			}

			return modelviews;
		}

		/// <summary>
		/// Saves the workspace emails.
		/// </summary>
		/// <param name="emails">The emails.</param>
		/// <param name="workspaceId">The workspace identifier.</param>
		public void SaveWorkspaceEmails(ICollection<WorkspaceEmailOverrideModelView> emails, int workspaceId)
		{
			ICollection<WorkspaceEmailOverrideDTO> workspaceDtos = this.WorkspaceLoader.GetWorkspaceEmailOverrides(workspaceId);
			ICollection<WorkspaceEmailOverrideDTO> emailOverrides = this.ConvertModelView(emails, workspaceDtos);
			this.WorkspaceLoader.SaveWorkspaceEmailOverrides(emailOverrides, workspaceId);
		}

		/// <summary>
		/// Converts the model view.
		/// </summary>
		/// <param name="emails">The emails.</param>
		/// <param name="workspaceDtos">The workspace overrides from DB.</param>
		/// <returns>The converted Model views merged with the current workspace overrides</returns>
		private ICollection<WorkspaceEmailOverrideDTO> ConvertModelView(ICollection<WorkspaceEmailOverrideModelView> emails, ICollection<WorkspaceEmailOverrideDTO> workspaceDtos)
		{
			List<WorkspaceEmailOverrideDTO> overrides = new List<WorkspaceEmailOverrideDTO>();
			foreach (WorkspaceEmailOverrideModelView email in emails)
			{
				WorkspaceEmailOverrideDTO overrideEmail = workspaceDtos.FirstOrDefault(w => w.EmailType == email.EmailType);
				if (overrideEmail != null)
				{
					// editing an existing dto
					if (email.WorkspaceOverrideOn.HasValue)
					{
						// this is an update
						overrideEmail.Updateable = UpdateType.Upsert;
						overrideEmail.TurnOn = email.WorkspaceOverrideOn.Value;
					}
					else
					{
						// this is a deletion
						overrideEmail.Updateable = UpdateType.Deleted;
					}

					overrides.Add(overrideEmail);
				}
				else if (email.WorkspaceOverrideOn.HasValue)
				{
					// adding a new dto
					overrideEmail = new WorkspaceEmailOverrideDTO
					{
						EmailType = email.EmailType,
						TurnOn = email.WorkspaceOverrideOn.Value,
						Updateable = UpdateType.Upsert
					};

					overrides.Add(overrideEmail);
				}
			}

			return overrides;
		}

		/// <summary>
		/// Converts the PTM line of business.
		/// </summary>
		/// <param name="lineOfBusinessID">The line of business identifier.</param>
		/// <returns>The BOE id for a LOB.</returns>
		public int ConvertPTMLineOfBusiness(int lineOfBusinessID)
		{
			return this.ConvertPtmPickList(PickListEnum.LineOfBusiness, lineOfBusinessID, PTM_LOB_CONVERSION_ERROR);
		}

		/// <summary>
		/// Converts the PTM pick list.
		/// </summary>
		/// <param name="pickListEnum">The pick list enum.</param>
		/// <param name="ptmPickListId">The PTM pick list identifier.</param>
		/// <param name="exceptionText">The exception text.</param>
		/// <returns>The converted id</returns>
		public int ConvertPtmPickList(PickListEnum pickListEnum, int ptmPickListId, string exceptionText)
		{
			int newId;
			PickListDto dto = this.ptmPickListMapper.GetById(pickListEnum, ptmPickListId);
			if (dto != null)
			{
				string toMatch = dto.Text.ToLower();

				ICollection<PickListDto> pickValues = this.boePickListMapper.GetPickListValues(pickListEnum).PickLists;
				PickListDto match = pickValues.FirstOrDefault(l => l.Text.ToLower() == toMatch);
				if (match != null)
				{
					newId = match.Id;
				}
				else
				{
					throw new GenValidationException(exceptionText);
				}
			}
			else
			{
				throw new GenValidationException(exceptionText);
			}

			return newId;
		}

		/// <summary>
		/// Converts the PTM contract type id to BOE id.
		/// </summary>
		/// <param name="contractTypeId">The contract type identifier.</param>
		/// <returns>BOE id or -1</returns>
		public int ConvertPTMContractTypeId(int contractTypeId)
		{
			return this.ConvertPtmPickList(PickListEnum.ContractType, contractTypeId, PTM_CONTRACT_TYPE_CONVERSION_ERROR);
		}

		/// <summary>
		/// Converts the PTM proposal class id to BOE id.
		/// </summary>
		/// <param name="proposalClassId">The proposal class identifier.</param>
		/// <returns>BOE id or -1</returns>
		public int ConvertPTMProposalClassId(int proposalClassId)
		{
			// if value is NotSet, then set to -1 for dropdown in UI
			if (proposalClassId == 0)
			{
				proposalClassId = -1;
			}

			return this.ConvertPtmPickList(PickListEnum.ProposalClass, proposalClassId, PTM_PROPOSAL_CLASS_CONVERSION_ERROR);
		}

		/// <summary>
		/// Creates the default Sikorsky Custom Fields
		/// </summary>
		public void CreateSikorskyCustomFields(int wsId)
		{
			// Custom fields(all regular custom fields, none are open ended):
			// SOW-> default option SOW1
			// Category-> default option Category1
			// Cam Name-> default option CAM1
			// Class Of Cost->also put in options(text): REC, NRE, DNR
			// Add / Delete->also put in options(text): A, D

			ICollection<CustomFieldDTO> existingCustomFields = this.customFieldLoader.GetByWorkspaceId(wsId);
			ICollection<CustomFieldValueDTO> customFieldValues = new Collection<CustomFieldValueDTO>();

			if (!this.CustomFieldAlreadyExists(SikorskyConstants.SIKORSKY_CF_SOW, existingCustomFields))
			{
				CustomFieldDTO sow = new CustomFieldDTO
				{
					Id = -1,
					WorkspaceID = wsId,
					CustomFieldName = SikorskyConstants.SIKORSKY_CF_SOW,
					CustomFieldDisplayID = CustomFieldType.BoeDisplay,
					CustomFieldRequired = false,
					IsOpenEnded = false,
					Updateable = UpdateType.Upsert
				};

				int? sowId = this.customFieldLoader.Save(sow);

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = sowId.Value,
					CustomFieldValueName = "SOW1",
					CustomFieldValueDescription = "SOW1",
					Id = -6,
					CustomFieldValueID = -6
				});
			}

			if (!this.CustomFieldAlreadyExists(SikorskyConstants.SIKORSKY_CF_CATEGORY, existingCustomFields))
			{
				CustomFieldDTO category = new CustomFieldDTO
				{
					Id = -2,
					WorkspaceID = wsId,
					CustomFieldName = SikorskyConstants.SIKORSKY_CF_CATEGORY,
					CustomFieldDisplayID = CustomFieldType.BoeDisplay,
					CustomFieldRequired = false,
					IsOpenEnded = false,
					Updateable = UpdateType.Upsert
				};

				int? categoryId = this.customFieldLoader.Save(category);

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = categoryId.Value,
					CustomFieldValueName = "Category1",
					CustomFieldValueDescription = "Category1",
					Id = -7,
					CustomFieldValueID = -7
				});
			}

			if (!this.CustomFieldAlreadyExists(SikorskyConstants.SIKORSKY_CF_CAMNAME, existingCustomFields))
			{
				CustomFieldDTO camName = new CustomFieldDTO
				{
					Id = -3,
					WorkspaceID = wsId,
					CustomFieldName = SikorskyConstants.SIKORSKY_CF_CAMNAME,
					CustomFieldDisplayID = CustomFieldType.BoeDisplay,
					CustomFieldRequired = false,
					IsOpenEnded = false,
					Updateable = UpdateType.Upsert
				};

				int? camNameId = this.customFieldLoader.Save(camName);

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = camNameId.Value,
					CustomFieldValueName = "CAM1",
					CustomFieldValueDescription = "CAM1",
					Id = -8,
					CustomFieldValueID = -8
				});
			}

			if (!this.CustomFieldAlreadyExists(SikorskyConstants.SIKORSKY_CF_CLASSOFCOST, existingCustomFields))
			{
				CustomFieldDTO classOfCost = new CustomFieldDTO
				{
					Id = -4,
					WorkspaceID = wsId,
					CustomFieldName = SikorskyConstants.SIKORSKY_CF_CLASSOFCOST,
					CustomFieldDisplayID = CustomFieldType.BoeDisplay,
					CustomFieldRequired = false,
					IsOpenEnded = false,
					Updateable = UpdateType.Upsert
				};

				int? classOfCostId = this.customFieldLoader.Save(classOfCost);

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = classOfCostId.Value,
					CustomFieldValueName = "REC",
					CustomFieldValueDescription = "Recurring",
					Id = -1,
					CustomFieldValueID = -1
				});

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = classOfCostId.Value,
					CustomFieldValueName = "NRE",
					CustomFieldValueDescription = "Non Recurring",
					Id = -2,
					CustomFieldValueID = -2
				});
			}

			if (!this.CustomFieldAlreadyExists(SikorskyConstants.SIKORSKY_CF_ADDDELETE, existingCustomFields))
			{
				CustomFieldDTO addDelete = new CustomFieldDTO
				{
					Id = -5,
					WorkspaceID = wsId,
					CustomFieldName = SikorskyConstants.SIKORSKY_CF_ADDDELETE,
					CustomFieldDisplayID = CustomFieldType.LaborTypeDisplay,
					CustomFieldRequired = false,
					IsOpenEnded = false,
					Updateable = UpdateType.Upsert
				};

				int? addDeleteId = this.customFieldLoader.Save(addDelete);

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = addDeleteId.Value,
					CustomFieldValueName = "A",
					CustomFieldValueDescription = "A",
					Id = -4,
					CustomFieldValueID = -4
				});

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = addDeleteId.Value,
					CustomFieldValueName = "D",
					CustomFieldValueDescription = "D",
					Id = -5,
					CustomFieldValueID = -5
				});
			}


			this.customFieldValueLoader.Save(customFieldValues);
		}

		/// <summary>
		/// Creates the default ProPricer Custom FIelds
		/// </summary>
		/// <param name="wsId">Workspace ID</param>
		public void CreateProPricerCustomFields(int wsId)
		{
			// Custom fields(all regular custom fields, none are open ended, none are required):
			// SOW -> default: SOW1
			// Location -> default: Moorestown
			// Class Of Cost-> defaults: REC, NRE
			// Project -> default: USER1
			// FIELD-A -> default: USER2
			
			ICollection<CustomFieldDTO> existingCustomFields = this.customFieldLoader.GetByWorkspaceId(wsId);
			ICollection<CustomFieldValueDTO> customFieldValues = new Collection<CustomFieldValueDTO>();

			if (!this.CustomFieldAlreadyExists(ProPricerCFConstants.PROPRICER_CF_SOW, existingCustomFields))
			{
				CustomFieldDTO sow = new CustomFieldDTO
				{
					Id = -2,
					WorkspaceID = wsId,
					CustomFieldName = ProPricerCFConstants.PROPRICER_CF_SOW,
					CustomFieldDisplayID = CustomFieldType.BoeDisplay,
					CustomFieldRequired = false,
					IsOpenEnded = false,
					Updateable = UpdateType.Upsert
				};

				int? sowId = this.customFieldLoader.Save(sow);

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = sowId.Value,
					CustomFieldValueName = "SOW1",
					CustomFieldValueDescription = "SOW1",
					Id = -2,
					CustomFieldValueID = -2
				});
			}

			if (!this.CustomFieldAlreadyExists(ProPricerCFConstants.PROPRICER_CF_LOCATION, existingCustomFields))
			{
				CustomFieldDTO location = new CustomFieldDTO
				{
					Id = -3,
					WorkspaceID = wsId,
					CustomFieldName = ProPricerCFConstants.PROPRICER_CF_LOCATION,
					CustomFieldDisplayID = CustomFieldType.BoeDisplay,
					CustomFieldRequired = false,
					IsOpenEnded = false,
					Updateable = UpdateType.Upsert
				};

				int? locationId = this.customFieldLoader.Save(location);

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = locationId.Value,
					CustomFieldValueName = "Moorestown",
					CustomFieldValueDescription = "Moorestown",
					Id = -3,
					CustomFieldValueID = -3
				});
			}

			if (!this.CustomFieldAlreadyExists(ProPricerCFConstants.PROPRICER_CF_CLASSOFCOST, existingCustomFields))
			{
				CustomFieldDTO classOfCost = new CustomFieldDTO
				{
					Id = -4,
					WorkspaceID = wsId,
					CustomFieldName = ProPricerCFConstants.PROPRICER_CF_CLASSOFCOST,
					CustomFieldDisplayID = CustomFieldType.BoeDisplay,
					CustomFieldRequired = false,
					IsOpenEnded = false,
					Updateable = UpdateType.Upsert
				};

				int? classOfCostId = this.customFieldLoader.Save(classOfCost);

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = classOfCostId.Value,
					CustomFieldValueName = "NRE",
					CustomFieldValueDescription = "Non Recurring",
					Id = -4,
					CustomFieldValueID = -4
				});

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = classOfCostId.Value,
					CustomFieldValueName = "REC",
					CustomFieldValueDescription = "Recurring",
					Id = -5,
					CustomFieldValueID = -5
				});
			}

			if (!this.CustomFieldAlreadyExists(ProPricerCFConstants.PROPRICER_CF_PROJECT, existingCustomFields))
			{
				CustomFieldDTO project = new CustomFieldDTO
				{
					Id = -5,
					WorkspaceID = wsId,
					CustomFieldName = ProPricerCFConstants.PROPRICER_CF_PROJECT,
					CustomFieldDisplayID = CustomFieldType.BoeDisplay,
					CustomFieldRequired = false,
					IsOpenEnded = false,
					Updateable = UpdateType.Upsert
				};

				int? projectId = this.customFieldLoader.Save(project);

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = projectId.Value,
					CustomFieldValueName = "USER1",
					CustomFieldValueDescription = "User Defined",
					Id = -6,
					CustomFieldValueID = -6
				});
			}

			if (!this.CustomFieldAlreadyExists(ProPricerCFConstants.PROPRICER_CF_FIELDA, existingCustomFields))
			{
				CustomFieldDTO fieldA = new CustomFieldDTO
				{
					Id = -6,
					WorkspaceID = wsId,
					CustomFieldName = ProPricerCFConstants.PROPRICER_CF_FIELDA,
					CustomFieldDisplayID = CustomFieldType.BoeDisplay,
					CustomFieldRequired = false,
					IsOpenEnded = false,
					Updateable = UpdateType.Upsert
				};

				int? fieldAId = this.customFieldLoader.Save(fieldA);

				customFieldValues.Add(new CustomFieldValueDTO
				{
					Updateable = UpdateType.Upsert,
					CustomFieldID = fieldAId.Value,
					CustomFieldValueName = "USER2",
					CustomFieldValueDescription = "User Defined 2",
					Id = -7,
					CustomFieldValueID = -7
				});
			}

			this.customFieldValueLoader.Save(customFieldValues);
		}

		/// <summary>
		/// Calculates (SAP) Actuals for MOQ Types inside a Workspace, then saves the changes to the database, 
		/// and sets BOE State to Draft and sends out emails (if BOE State needed changed)
		/// </summary>
		/// <param name="ws">The workspace</param>
		/// <returns>Updated list of Calculated Actuals model views</returns>
		public async Task<ICollection<WorkspaceCalculateActualsModelView>> RecalculateActuals(FullWorkspace ws)
		{
			List<WorkspaceCalculateActualsModelView> result = new List<WorkspaceCalculateActualsModelView>();

			bool isRMS = SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST;
			string sapRepo = RepositoryName.SapWebi.GetDescription();
			Dictionary<int, FullBoe> boes = ws.Boes.ToDictionary(b => b.Id);
			Dictionary<int, MoqTableData> tables = new Dictionary<int, MoqTableData>();
			Dictionary<int, MoqTypeSelection> tableIdToMoqType = new Dictionary<int, MoqTypeSelection>();
			List<MoqTypeSelection> moqTypesToSave = new List<MoqTypeSelection>();
			foreach (MoqTypeSelection moqType in ws.MoqTypeSelections)
			{
				if ((moqType.SelectedMOQType == MOQType.Historical || moqType.SelectedMOQType == MOQType.Comparative) &&
					moqType.TableData != null && moqType.TableData.Any())
				{
					moqTypesToSave.Add(moqType);
					moqType.Updateable = UpdateType.Upsert;

					foreach (MoqTableData table in moqType.TableData)
					{
						// only recalculate if RMS or SAP/WEBI set as Repo
						if (isRMS || table.RepositoryName == sapRepo)
						{
							tables.Add(table.Id, table);
							tableIdToMoqType.Add(table.Id, moqType);
						}
					}
				}
			}

			if (tables.Any())
			{
				Dictionary<int, string> tasks = ws.TaskElements.ToDictionary(t => t.Id, x => x.TaskTitle);
				List<int> boesUpdated = await RecalculateActualsAcrossWorkspace(result, boes, tasks, tables, tableIdToMoqType, ws.CreationDate, moqTypesToSave);
				SaveRecalculateActuals(ws, boes, moqTypesToSave, boesUpdated);
			}

			// Reorder and reset the Order property on the results
			result = result.OrderBy(r => r.BoeTitle).ThenBy(t => t.Task).ThenBy(o => o.Order).ToList();
			int order = 0;
			result.ForEach(r => r.Order = order++);

			return result;
		}

		/// <summary>
		/// Recalculates the SAP Actuals across a Workspace, and finds any BOEs that were updated
		/// ToDo Thomas: Look here to add CLIN
		/// </summary>
		/// <param name="result">The list of Models that were updated</param>
		/// <param name="boes">Dictionary of BOEs</param>
		/// <param name="tasks">Dictionary of Tasks</param>
		/// <param name="tables">Dictionary of MOQ Tables</param>
		/// <param name="tableIdToMoqType">Dictionary of MOQ Types keyed by Table Id</param>
		/// <param name="workspaceCreationDate">Workspace Creation Date</param>
		/// <param name="moqTypes">The MOQ Types</param>
		/// <returns></returns>
		private async Task<List<int>> RecalculateActualsAcrossWorkspace(List<WorkspaceCalculateActualsModelView> result, Dictionary<int, FullBoe> boes, Dictionary<int, string> tasks, Dictionary<int, MoqTableData> tables, Dictionary<int, MoqTypeSelection> tableIdToMoqType, DateTime? workspaceCreationDate, ICollection<MoqTypeSelection> moqTypes)
		{
			ICollection<MoqTableDataModelView> tableData = tables.Values.Select(t =>
				new MoqTableDataModelView()
				{
					Filters = t.AdditionalQueryFilters,
					PoPStart = t.PoPStart,
					PoPEnd = t.PoPEnd,
					QueryType = t.QueryType,
					TableId = t.Id,
					WbsElement = t.WbsElement
				}
			).ToList();


			List<int> boesUpdated = new List<int>();
			// Make one bulk call to SAP
			if (Utilities.IsSkillMixEnabledForSystem && Utilities.ShowSkillMixForWorkspace(workspaceCreationDate))
			{
				ICollection<IESResponse<CalculateActualsWithSkillMixViewModel>> responses = await this.boeLaborControllerLogic.CalculateAllActualsSapWithSkillMix(tableData);
				foreach (IESResponse<CalculateActualsWithSkillMixViewModel> response in responses)
				{
					WorkspaceCalculateActualsModelView resultModel = new WorkspaceCalculateActualsModelView();
					CalculateActualsWithSkillMixViewModel model = response.Data.First();

					// get the original table and moqType
					MoqTableData table = tables[model.TableId];
					MoqTypeSelection moqType = tableIdToMoqType[model.TableId];
					FullBoe boe = boes[moqType.BoeId];

					resultModel.TableName = table.TableName;
					resultModel.WbsHoursPrevious = table.TotalWbsHours;
					resultModel.TotalRelevantHoursPrevious = table.TotalRelevantHours;
					resultModel.BoeStatePrevious = boe.State.GetDescription();
					resultModel.ClinString = boe.Clin?.ClinString;
					resultModel.WbsString = boe.Wbs?.WbsString;
					resultModel.BoeTitle = boe.Title;
					resultModel.Task = tasks[moqType.TaskId];
					resultModel.Order = table.Order;
					resultModel.IsSuccessful = response.IsSuccessful;
					
					if (response.IsSuccessful)
					{
						if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST)
						{
							double? wbsHoursSum = model.SkillMixDataTable.Sum(s => s.WbsHours);
							resultModel.WbsHoursPrevious = table.TotalWbsHours;
							table.TotalWbsHours = wbsHoursSum.HasValue ? Convert.ToDecimal(wbsHoursSum.Value) : default(decimal);
							resultModel.WbsHours = table.TotalWbsHours;

							if (string.IsNullOrWhiteSpace(table.ContractNumber) && !string.IsNullOrWhiteSpace(model.ContractNumber))
							{
								table.ContractNumber = model.ContractNumber;
							}

							table.RepositoryName = RepositoryName.SAP.GetDescription();
						}

						double totalHoursSum = model.SkillMixDataTable.Sum(s => s.TotalHours);
						resultModel.TotalRelevantHoursPrevious = table.TotalRelevantHours;
						table.DateOfReport = DateTime.Now;
						table.TotalRelevantHours = Convert.ToDecimal(totalHoursSum);
						resultModel.TotalRelevantHours = table.TotalRelevantHours;

						table.ResourceHours = model.SkillMixDataTable.Select(skillMix => new MOQTypeSelectionTableDataResourceHoursDTO
							{ 
								ResourceName= skillMix.ResourceID, 
								WbsHours= skillMix.WbsHours.HasValue ? Convert.ToDecimal(skillMix.WbsHours.Value) : default(decimal), 
								TotalHours= Convert.ToDecimal(skillMix.TotalHours) 
							}).ToArray();

						// only return to UI if Total Relevant Hours changes
						if (resultModel.TotalRelevantHours != resultModel.TotalRelevantHoursPrevious)
						{
							result.Add(resultModel);
							boesUpdated.Add(moqType.BoeId);
						}
					}
					else
					{
						resultModel.Messages = response.Messages;
						result.Add(resultModel);
					}
				}

				foreach (MoqTypeSelection moqType in moqTypes)
				{
					moqType.SkillMixTable = this.boeLaborControllerLogic.RefreshSkillMixTable(moqType.TableData.SelectMany(t => t.ResourceHours).ToArray(), moqType.SkillMixTable);
				}
			}
			else
			{
				ICollection<IESResponse<CalculateActualsViewModel>> responses = await this.boeLaborControllerLogic.CalculateAllActualsSap(tableData);

				foreach (IESResponse<CalculateActualsViewModel> response in responses)
				{
					WorkspaceCalculateActualsModelView resultModel = new WorkspaceCalculateActualsModelView();
					CalculateActualsViewModel model = response.Data.First();

					// get the original table and moqType
					MoqTableData table = tables[model.TableId];
					MoqTypeSelection moqType = tableIdToMoqType[model.TableId];
					FullBoe boe = boes[moqType.BoeId];

					resultModel.TableName = table.TableName;
					resultModel.WbsHoursPrevious = table.TotalWbsHours;
					resultModel.TotalRelevantHoursPrevious = table.TotalRelevantHours;
					resultModel.BoeStatePrevious = boe.State.GetDescription();
					resultModel.ClinString = boe.Clin?.ClinString;
					resultModel.WbsString = boe.Wbs?.WbsString;
					resultModel.BoeTitle = boe.Title;
					resultModel.Task = tasks[moqType.TaskId];
					resultModel.Order = table.Order;
					resultModel.IsSuccessful = response.IsSuccessful;

					if (response.IsSuccessful)
					{
						if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST)
						{
							resultModel.WbsHoursPrevious = table.TotalWbsHours;
							table.TotalWbsHours = model.WbsHours.HasValue ? Convert.ToDecimal(model.WbsHours.Value) : default(decimal);
							resultModel.WbsHours = table.TotalWbsHours;

							if (string.IsNullOrWhiteSpace(table.ContractNumber) && !string.IsNullOrWhiteSpace(model.ContractNumber))
							{
								table.ContractNumber = model.ContractNumber;
							}

							table.RepositoryName = RepositoryName.SAP.GetDescription();
						}

						resultModel.TotalRelevantHoursPrevious = table.TotalRelevantHours;
						table.DateOfReport = DateTime.Now;
						table.TotalRelevantHours = Convert.ToDecimal(model.TotalHours);
						resultModel.TotalRelevantHours = table.TotalRelevantHours;

						// only return to UI if Total Relevant Hours changes
						if (resultModel.TotalRelevantHours != resultModel.TotalRelevantHoursPrevious)
						{
							result.Add(resultModel);
							boesUpdated.Add(moqType.BoeId);
						}
					}
					else
					{
						resultModel.Messages = response.Messages;
						result.Add(resultModel);
					}
				}
			}

			return boesUpdated;
		}

		/// <summary>
		/// Saves the output from Recalculating the Actuals on MOQ Types for a Workspace
		/// </summary>
		/// <param name="ws">The workspace</param>
		/// <param name="boes">Dictionary of BOEs for a Workspace keyed by BoeId</param>
		/// <param name="moqTypesToSave">The Moq Types to save</param>
		/// <param name="boesUpdated">The list of BOE Ids that were updated</param>
		/// <exception cref="ValidationException">Thrown when a BOE tries to move to an invalid BOE State</exception>
		private void SaveRecalculateActuals(FullWorkspace ws, Dictionary<int, FullBoe> boes, List<MoqTypeSelection> moqTypesToSave, List<int> boesUpdated)
		{
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				// Save the moq types (updated hours and report date)
				this.moqTypeLoader.Save(moqTypesToSave);

				// run boes through boe state machine to set back to draft
				boesUpdated = boesUpdated.Distinct().ToList();
				List<FullBoe> updatedBoes = boes.Values.Where(b => boesUpdated.Contains(b.Id)).ToList();

				foreach (FullBoe boe in updatedBoes)
				{
					FullBoe readjustBoe = this.factory.CreateFullBoe(boe);
					BOEState oldBOEState = readjustBoe.State;
					BOEState newBOEState = BOEState.Draft;

					// Validate the Awaiting Approval or Approved to Draft state transition
					string validationMessage;
					if (!this.boeStateMachine.PerformStateTransitionValidation(readjustBoe, ws, readjustBoe.State, newBOEState, out validationMessage))
					{
						// not valid ... communicate to user
						throw new ValidationException(validationMessage);
					}

					// If the transition is valid, set the BOE to Draft and save it
					readjustBoe.Updateable = UpdateType.Upsert;
					readjustBoe.State = newBOEState;
					readjustBoe.UpdatedByUserId = ws.CurrentActiveUser.UserID;
					this.boeMediator.MediatedSave(ws, readjustBoe);

					// Perform common state transition actions
					this.boeStateMachine.PerformStateTransitionAction(readjustBoe, ws, oldBOEState, readjustBoe.State);
				}

				scope.Complete();
			}

			ws.RefreshBoes();
		}

		/// <summary>
		/// Check if a custom field exists for the given name
		/// </summary>
		/// <param name="customFieldName">Name to check</param>
		/// <param name="existingCustomFields">Existing custom fields</param>
		/// <returns>True if custom field already exists, otherwise false</returns>
		private bool CustomFieldAlreadyExists(string customFieldName, ICollection<CustomFieldDTO> existingCustomFields)
		{
			return existingCustomFields.Any(x => x.CustomFieldName.ToLower() == customFieldName.ToLower());
		}
	}
}
