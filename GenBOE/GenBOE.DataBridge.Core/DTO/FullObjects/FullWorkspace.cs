// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO.FullObjects
{
	using System;
	using System.Collections.Generic;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Interfaces;

	/// <summary>
	/// Workspace
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]
	[Serializable()]
	public class FullWorkspace : WorkspaceDTO, IDateShiftable
	{
		//	#region Private Properties

		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		//	[NonSerialized]
		//	protected IRetriever retriever;

		//	[NonSerialized]
		//	private ICommonDataMapper commonDataMapper;

		//	[NonSerialized]
		//	private IPermissionsDTODataLoader _PermissionsLoader;

		//	private string _WorkspaceStateName = null;
		//	private ReadOnlyCollection<PermissionsDTO> _WorkspacePermissions = null;
		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		//	protected ReadOnlyCollection<FullWbs> wbsElements;
		//	private ReadOnlyCollection<FullWbs> wbsElementsNoMulti;
		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		//	protected ReadOnlyCollection<FullClin> clins;
		//	private ReadOnlyCollection<FullClin> clinsNoMulti;
		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		//	protected ReadOnlyCollection<FullBoe> boes;
		//	private ReadOnlyCollection<WorkspaceVariableDTO> workspaceVariables;
		//	private ReadOnlyCollection<WorkspaceHistoryDTO> workspaceHistory;
		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		//	protected ReadOnlyCollection<BoeTaskElementDTO> taskElements;
		//	private ReadOnlyCollection<ResourceDTO> resourcesByListId;
		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		//	protected ReadOnlyCollection<ResourceDTO> resourcesUsedInBoes;
		//	private ReadOnlyCollection<ResourceDTO> systemLmLaborResources;
		//	private ReadOnlyCollection<TravelDTO> travels;
		//	private ReadOnlyCollection<CustomFieldDTO> customFields;
		//	private ReadOnlyCollection<CustomFieldValueDTO> customFieldValues;
		//	private ReadOnlyCollection<TMResourceRateDTO> tmResourceRates;
		//	private ReadOnlyCollection<WorkspaceExportFormatDTO> workspaceExportFormats;
		//	private string selectedWorkspaceExportFormatName;
		//	private ReadOnlyCollection<WorkspaceVersionMetaDataDTO> workspaceVersionMetaData;
		//	private ReadOnlyCollection<ProPricerDTO> proPricerExports;
		//	private ReadOnlyCollection<PerformingOrgDTO> performingOrgsForWorkspaceList;
		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		//	protected ReadOnlyCollection<PerformingOrgDTO> performingOrgsUsedInBoes;
		//	private PerformingOrgListDTO performingOrgList;
		//	private ReadOnlyCollection<MaterialDTO> materials;
		//	private ReadOnlyCollection<OtherDirectCostDTO> odcs;
		//	private ReadOnlyCollection<EscalationRatesDTO> escalationRates;
		//	private ReadOnlyCollection<TripDTO> travelTrips;
		//	private ReadOnlyCollection<PerDiemDTO> perDiemsForTravelTrips;
		//	private ReadOnlyCollection<MiscTravelRateDTO> miscTravelRatesForTravelTrips;
		//	private UserDTO currentActiveUser;
		//	private ReadOnlyCollection<LocationDTO> locationsInWs;
		//	private Dictionary<int, ICollection<KeyValuePair<int, int>>> taskElementsMappingWithCustomFieldsValuesAndContainerIds;
		//	private Dictionary<int, ICollection<KeyValuePair<int, int>>> moqTypeTableMappingWithCustomFieldsValuesAndContainerIds;
		//	private Dictionary<int, ICollection<KeyValuePair<int, int>>> travelElementsMappingWithCustomFieldsValuesAndContainerIds;
		//	private Dictionary<int, ICollection<KeyValuePair<int, int>>> travelTripMappingWithCustomFieldsValuesAndContainerIds;
		//	private Dictionary<int, ICollection<KeyValuePair<int, int>>> laborResourcesMappingWithCustomFieldsValuesAndContainerIds;
		//	private Dictionary<int, UserDTO> boeIdsAndLastUserToSubmitThemForApprovalMapping;
		//	private ReadOnlyCollection<UserDTO> usersAssociatedWithBoesInWs;
		//	private IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> assignedBoeIdsAndCustomFieldValuesMapping;
		//	private ILookup<ElementOfCostType, ResourceDTO> workspaceResourcesLookup = null;
		//	private IDictionary<int, ICollection<BoeApproverResponseDTO>> boeMappingWithApproverResponses;
		//	private ReadOnlyCollection<RteTemplateSource> rteOverrides;
		//	private ReadOnlyCollection<RTECustomTemplateQuestionAnswerModelView> templateQuestionsAndAnswers;
		//	private ReadOnlyCollection<MoqTypeSelection> moqTypeSelections;

		//	#endregion

		//	/// <summary>
		//	/// Gets the currently active user.
		//	/// </summary>
		//	public UserDTO CurrentActiveUser
		//	{
		//		get
		//		{
		//			return currentActiveUser;
		//		}
		//	}

		//	/// <summary>
		//	/// Refreshes the currently active user.
		//	/// </summary>
		//	public void RefreshCurrentActiveUser()
		//	{
		//		currentActiveUser = null;
		//	}

		//	/// <summary>
		//	/// Gets the workspace state name.
		//	/// </summary>
		//	/// <returns></returns>
		//	public string WorkspaceStateName
		//	{
		//		get
		//		{
		//			if (string.IsNullOrEmpty(_WorkspaceStateName))
		//			{
		//				_WorkspaceStateName = commonDataMapper.getWorkspaceStateName(WorkspaceState);
		//			}
		//			return _WorkspaceStateName;
		//		}
		//	}

		//	/// <summary>
		//	/// Gets the workspace permissions.
		//	/// </summary>
		//	/// <returns></returns>
		//	public IReadOnlyCollection<PermissionsDTO> WorkspacePermissions
		//	{
		//		get
		//		{
		//			if (_WorkspacePermissions == null)
		//			{
		//				_WorkspacePermissions = _PermissionsLoader.GetWorkspacePermissions(Id).ToList().AsReadOnly();
		//			}
		//			return _WorkspacePermissions;
		//		}
		//	}

		//	/// <summary>
		//	/// Clins belonging to the Workspace
		//	/// </summary>
		//	public virtual IReadOnlyCollection<FullClin> Clins
		//	{
		//		get
		//		{
		//			if (clins == null)
		//			{
		//				clins = retriever.GetClinsByWorkspaceId(Id).ToList().AsReadOnly();
		//			}

		//			return clins;
		//		}
		//	}

		//	/// <summary>
		//	/// Clins belonging to the Workspace without Multi Clin
		//	/// </summary>
		//	public virtual IReadOnlyCollection<FullClin> ClinsNoMultiClin
		//	{
		//		get
		//		{
		//			if (clinsNoMulti == null)
		//			{
		//				clinsNoMulti = Clins.Where(c => c.ClinNumber != Constants.UNIQUE_MULTI_NUMBER).ToList().AsReadOnly();
		//			}

		//			return clinsNoMulti;
		//		}
		//	}

		//	/// <summary>
		//	/// Wbs Elements belonging to the Workspace
		//	/// </summary>
		//	public virtual IReadOnlyCollection<FullWbs> WbsElements
		//	{
		//		get
		//		{
		//			if (wbsElements == null)
		//			{
		//				wbsElements = retriever.GetFullWbsElementsByWorkspaceId(Id).OrderBy(x => x.WbsPaddedNumber).ToList().AsReadOnly();
		//			}

		//			return wbsElements;
		//		}
		//	}

		//	/// <summary>
		//	/// Wbs Elements belonging to the Workspace without Multi
		//	/// </summary>
		//	public virtual IReadOnlyCollection<FullWbs> WbsElementsNoMultiWbs
		//	{
		//		get
		//		{
		//			if (wbsElementsNoMulti == null)
		//			{
		//				wbsElementsNoMulti = WbsElements.Where(w => w.WbsNumber != Constants.UNIQUE_MULTI_NUMBER).OrderBy(x => x.WbsPaddedNumber).ToList().AsReadOnly();
		//			}

		//			return wbsElementsNoMulti;
		//		}
		//	}


		//	/// <summary>
		//	/// Boes belonging to the Workspace
		//	/// </summary>
		//	public virtual IReadOnlyCollection<FullBoe> Boes
		//	{
		//		get
		//		{
		//			LoadBoes();

		//			return boes;
		//		}
		//	}

		//	/// <summary>
		//	/// Loads BOEs into the property.. Used if you want to preload the BOEs ahead of time
		//	/// </summary>
		//	public virtual void LoadBoes()
		//	{
		//		if (boes == null)
		//		{
		//			boes = retriever.GetFullBoesByWorkspaceId(Id, false, null).ToList().AsReadOnly();
		//		}
		//	}

		//	/// <summary>
		//	/// Populates RTE data for all Boes
		//	/// </summary>
		//	public virtual void LoadBoesAndTaskElementsRTEData()
		//	{
		//		if (boes == null)
		//		{
		//			// Confirming that Task Elements are loaded with RTE data
		//			LoadTaskElementRTEData();

		//			// Data has not been pulled yet, so we can do a full retrieval, including the RTE data
		//			boes = retriever.GetFullBoesByWorkspaceId(Id, true, TaskElements).ToList().AsReadOnly();
		//		}
		//		else
		//		{
		//			// Data was already retrieved, so we are only missing the RTE data, which we'll now load
		//			retriever.PopulateRTEData(boes);
		//		}
		//	}

		//	/// <summary>
		//	/// Loads BOEs and Clins into the properties.. Used if you want to preload the BOEs and Clins ahead of time.
		//	/// </summary>
		//	public virtual void LoadClinsAndBoes(bool loadBoeRteData = false)
		//	{
		//		if (clins == null)
		//		{
		//			clins = retriever.GetClinsByWorkspaceId(Id).ToList().AsReadOnly();
		//		}

		//		if (boes == null)
		//		{
		//			// passing in this.taskElements here to reuse Task Elements only if they are already loaded
		//			boes = retriever.GetFullBoesByWorkspaceId(Id, loadBoeRteData, taskElements).ToList().AsReadOnly();
		//		}

		//		foreach (FullClin clin in clins)
		//		{
		//			clin.SetBoes(boes);
		//		}
		//	}

		//	/// <summary>
		//	/// Workspace variables belonging to the workspace.
		//	/// </summary>
		//	public IReadOnlyCollection<WorkspaceVariableDTO> WorkspaceVariables
		//	{
		//		get
		//		{
		//			if (workspaceVariables == null)
		//			{
		//				workspaceVariables = retriever.GetWorkspaceVariableDTOsByWorkspaceId(Id).ToList().AsReadOnly();
		//			}

		//			return workspaceVariables;
		//		}
		//	}

		//	/// <summary>
		//	/// All task elements belonging to the workspace.
		//	/// </summary>
		//	public virtual IReadOnlyCollection<BoeTaskElementDTO> TaskElements
		//	{
		//		get
		//		{
		//			if (taskElements == null)
		//			{
		//				taskElements = retriever.GetBoeTaskElementCollectionByWorkspaceId(Id, false, DecimalPrecision, CostDecimalPrecision).ToList().AsReadOnly();
		//			}

		//			return taskElements;
		//		}
		//	}

		//	/// <summary>
		//	/// Populates RTE data for Task Elements
		//	/// </summary>
		//	public virtual void LoadTaskElementRTEData()
		//	{
		//		if (taskElements == null)
		//		{
		//			// Data has not been pulled yet, so we can do a full retrieval, including the RTE data
		//			taskElements = retriever.GetBoeTaskElementCollectionByWorkspaceId(Id, true, DecimalPrecision, CostDecimalPrecision).ToList().AsReadOnly();
		//		}
		//		else
		//		{
		//			// Data was already retrieved, so we are only missing the RTE data, which we'll now load
		//			retriever.PopulateRTEData(taskElements);
		//		}
		//	}

		//	#region Travel And Related Data (trips, per diems and misc rates)

		//	/*********
		//	 * So here's an issue. Trips/PerDiems/MiscRates are all tied to Travels. So once we have a Travel object, we can get the rest of the objects related to that travel.
		//	 * The issue is that travel can exist in the following states:
		//	 * 1. Travel is saved as a travel task element. In which case it belongs to a BOE, and then WS. So the element can be retrieved based on WS id.
		//	 * 2. There are system travels (trips, per diems...). In this case we can retrieve objects via loader based on Ids, but they are not tied to the WS.
		//	 * 3. The WS is locked/completed/, when all of these objects are moved into a different table and locked. In this case the loader does the work, so we don't have to worry about it.
		//	 * 
		//	 * Because of issue 1 & 2, we had to implement a GetTravel/... by Id methods inside of workspace, and these will do the following:
		//	 * a first look if the WS objects contain the object, if yes, done
		//	 * b if the object is not tied to the WS, then grab it from the loader
		//	 *     
		//	 *********/

		//	/// <summary>
		//	/// All travel elements belonging to the workspace.
		//	/// </summary>
		//	public IReadOnlyCollection<TravelDTO> Travels
		//	{
		//		get
		//		{
		//			if (travels == null)
		//			{
		//				travels = retriever.GetTravelByWorkspaceId(Id, false).ToList().AsReadOnly();
		//			}

		//			return travels;
		//		}
		//	}

		//	/// <summary>
		//	/// Populates RTE data for all Travels
		//	/// </summary>
		//	public void LoadTravelRTEData()
		//	{
		//		if (travels == null)
		//		{
		//			// Data has not been pulled yet, so we can do a full retrieval, including the RTE data
		//			travels = retriever.GetTravelByWorkspaceId(Id, true).ToList().AsReadOnly();
		//		}
		//		else
		//		{
		//			// Data was already retrieved, so we are only missing the RTE data, which we'll now load
		//			retriever.PopulateRTEData(travels);
		//		}
		//	}

		//	/// <summary>
		//	/// Gets all Travel Trips for the workspace.
		//	/// </summary>
		//	public IReadOnlyCollection<TripDTO> TravelTrips
		//	{
		//		get
		//		{
		//			if (travelTrips == null)
		//			{
		//				travelTrips = new ReadOnlyCollection<TripDTO>(new List<TripDTO>());
		//				if (Travels.Any())
		//				{
		//					travelTrips = retriever.GetTravelTripsForWorkspace(Travels.SelectMany(i => i.TravelTrips).Select(i => i.SystemTripID).ToCollection<int>(), this).ToList().AsReadOnly();
		//				}
		//			}

		//			return travelTrips;
		//		}
		//	}

		//	/// <summary>
		//	/// Gets trip by id
		//	/// </summary>
		//	/// <param name="id">Id</param>
		//	/// <returns>Trip Dto</returns>
		//	public TripDTO GetTripById(int id)
		//	{
		//		TripDTO trip = TravelTrips.FirstOrDefault(x => x.TripID == id);

		//		if (trip == null)
		//		{
		//			trip = retriever.GetTravelTripById(id, this);
		//		}

		//		return trip;
		//	}

		//	/// <summary>
		//	/// Gets all Per Diems for all Travel Trips for the workspace.
		//	/// </summary>
		//	public IReadOnlyCollection<PerDiemDTO> PerDiemsForTravelTrips
		//	{
		//		get
		//		{
		//			if (perDiemsForTravelTrips == null)
		//			{
		//				perDiemsForTravelTrips = new ReadOnlyCollection<PerDiemDTO>(new List<PerDiemDTO>());
		//				if (Travels.Any() && TravelTrips.Any())
		//				{
		//					perDiemsForTravelTrips = retriever.GetTravelTripPerDiemsForWorkspace(travelTrips.Select(i => i.PerDiemID).ToCollection<int>(), this).ToList().AsReadOnly();
		//				}
		//			}

		//			return perDiemsForTravelTrips;
		//		}
		//	}

		//	/// <summary>
		//	/// Gets perDiem by Id
		//	/// </summary>
		//	/// <param name="id">Id</param>
		//	/// <returns>Per Diem</returns>
		//	public PerDiemDTO GetPerDiemById(int id)
		//	{
		//		PerDiemDTO perDiem = PerDiemsForTravelTrips.FirstOrDefault(x => x.Id == id);
		//		if (perDiem == null)
		//		{
		//			perDiem = retriever.GetTravelTripPerDiemsForWorkspace(new List<int>() { id }, this).FirstOrDefault();
		//		}

		//		return perDiem;
		//	}

		//	/// <summary>
		//	/// Gets all Miscellaneous travel rates for all Travel Trips for the workspace.
		//	/// </summary>
		//	public IReadOnlyCollection<MiscTravelRateDTO> MiscTravelRatesForTravelTrips
		//	{
		//		get
		//		{
		//			if (miscTravelRatesForTravelTrips == null)
		//			{
		//				miscTravelRatesForTravelTrips = new ReadOnlyCollection<MiscTravelRateDTO>(new List<MiscTravelRateDTO>());
		//				if (Travels.Any() && TravelTrips.Any())
		//				{
		//					miscTravelRatesForTravelTrips = retriever.GetTravelTripMiscTravelRatesForWorkspace(travelTrips.Select(i => i.MiscTravelRateID).ToCollection<int>(), this).ToList().AsReadOnly();
		//				}
		//			}

		//			return miscTravelRatesForTravelTrips;
		//		}
		//	}

		//	/// <summary>
		//	/// Gets Misc Travel Dto by Id
		//	/// </summary>
		//	/// <param name="id">Id</param>
		//	/// <returns>Misc Travel Dto</returns>
		//	public MiscTravelRateDTO GetMiscTravelRateById(int id)
		//	{
		//		MiscTravelRateDTO dto = MiscTravelRatesForTravelTrips.FirstOrDefault(x => x.Id == id);

		//		if (dto == null)
		//		{
		//			dto = retriever.GetTravelTripMiscTravelRatesForWorkspace(new List<int>() { id }, this).FirstOrDefault();
		//		}

		//		return dto;
		//	}

		//	/// <summary>
		//	/// All location data used by trips in the WS
		//	/// </summary>
		//	public IReadOnlyCollection<LocationDTO> LocationsUsedByTrips
		//	{
		//		get
		//		{
		//			if (locationsInWs == null)
		//			{
		//				ICollection<int> locationIds = TravelTrips.Select(x => x.DepartureLocationID)
		//													.Union(travelTrips.Select(y => y.DestinationLocationID)).Distinct().ToList();

		//				locationsInWs = retriever.GetLocationDataByIds(locationIds).ToList().AsReadOnly();
		//			}

		//			return locationsInWs;
		//		}
		//	}

		//	/// <summary>
		//	/// Gets Location Dto by Id
		//	/// </summary>
		//	/// <param name="id">Id</param>
		//	/// <returns>Location Dto</returns>
		//	public LocationDTO GetLocationById(int id)
		//	{
		//		LocationDTO dto = LocationsUsedByTrips.FirstOrDefault(x => x.Id == id);

		//		if (dto == null)
		//		{
		//			dto = retriever.GetLocationDataByIds(new List<int>() { id }).FirstOrDefault();
		//		}

		//		return dto;

		//	}

		//	#endregion

		//	/// <summary>
		//	/// Workspace History belonging to the workspace.
		//	/// </summary>
		//	public IReadOnlyCollection<WorkspaceHistoryDTO> WorkspaceHistory
		//	{
		//		get
		//		{
		//			if (workspaceHistory == null)
		//			{
		//				workspaceHistory = retriever.GetWorkspaceHistoryByWorkspaceId(Id).ToList().AsReadOnly();
		//			}

		//			return workspaceHistory;
		//		}
		//	}

		//	/// <summary>
		//	/// Gets all Escalation Rates for the workspace.
		//	/// </summary>
		//	public IReadOnlyCollection<EscalationRatesDTO> EscalationRates
		//	{
		//		get
		//		{
		//			if (escalationRates == null)
		//			{
		//				escalationRates = retriever.GetEscalationRatesByWorkspace(this).ToList().AsReadOnly();
		//			}

		//			return escalationRates;
		//		}
		//	}

		//	/// <summary>
		//	/// Workspace resources.
		//	/// </summary>
		//	public IReadOnlyCollection<ResourceDTO> ResourcesForWsResourceListId
		//	{
		//		get
		//		{
		//			if (resourcesByListId == null)
		//			{
		//				if (ResourceSorting == CustomFieldSorting.ID)
		//				{
		//					resourcesByListId = retriever.GetResourcesByResourceListId(ResourceListID).OrderBy(r => r.ResourceName).ToList().AsReadOnly();
		//				}
		//				else
		//				{
		//					resourcesByListId = retriever.GetResourcesByResourceListId(ResourceListID).OrderBy(r => r.ResourceDesc).ToList().AsReadOnly();
		//				}
		//			}

		//			return resourcesByListId;
		//		}
		//	}

		//	/// <summary>
		//	/// Returns back a collection of resources used by the Boes in the entire WS
		//	/// </summary>
		//	public virtual IReadOnlyCollection<ResourceDTO> ResourcesUsedInWsBoes
		//	{
		//		get
		//		{
		//			if (resourcesUsedInBoes == null)
		//			{
		//				ICollection<int> resourceIds = TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value)
		//					.Union(TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.BusinessResourceCodeID.HasValue).Select(x => x.BusinessResourceCodeID.Value))
		//					.Union(Odcs.SelectMany(x => x.ODCTypes).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value))
		//					.Distinct().ToList();

		//				if (ResourceSorting == CustomFieldSorting.ID)
		//				{
		//					resourcesUsedInBoes = retriever.GetResourcesByIds(resourceIds).OrderBy(r => r.ResourceName).ToList().AsReadOnly();
		//				}
		//				else
		//				{
		//					resourcesUsedInBoes = retriever.GetResourcesByIds(resourceIds).OrderBy(r => r.ResourceDesc).ToList().AsReadOnly();
		//				}
		//			}

		//			return resourcesUsedInBoes;
		//		}
		//	}

		//	/// <summary>
		//	/// System level LM labor resources.
		//	/// </summary>
		//	public IReadOnlyCollection<ResourceDTO> ResourcesForSystemResourceListId
		//	{
		//		get
		//		{
		//			if (systemLmLaborResources == null)
		//			{
		//				systemLmLaborResources = retriever.GetSystemLmLaborResources().ToList().AsReadOnly();
		//			}

		//			return systemLmLaborResources;
		//		}
		//	}

		//	/// <summary>
		//	/// Workspace resources.
		//	/// </summary>
		//	public ILookup<ElementOfCostType, ResourceDTO> WorkspaceResourcesAsLookup
		//	{
		//		get
		//		{
		//			if (workspaceResourcesLookup == null)
		//			{
		//				workspaceResourcesLookup = ResourcesForWsResourceListId.ToLookup(x => x.ElementOfCost);
		//			}

		//			return workspaceResourcesLookup;
		//		}
		//	}

		//	/// <summary>
		//	/// Workspace resource rates for T&amp;M Resources.  
		//	/// </summary>
		//	public IReadOnlyCollection<TMResourceRateDTO> TMResourceRatesForWorkspace
		//	{
		//		get
		//		{
		//			if (tmResourceRates == null)
		//			{
		//				tmResourceRates = retriever.GetTMResourceRates(Id).ToList().AsReadOnly();
		//			}

		//			// Return T&M Rates for workspace.
		//			return tmResourceRates;
		//		}
		//	}

		//	/// <summary>
		//	/// Get Selected Workspace Export Format Name
		//	/// </summary>
		//	public string SelectedWorkspaceExportFormatName
		//	{
		//		get
		//		{
		//			if (selectedWorkspaceExportFormatName == null)
		//			{
		//				if (workspaceExportFormats == null)
		//				{
		//					// retrieve from the database directly
		//					selectedWorkspaceExportFormatName = retriever.GetWorkspaceExportFormatNameByTemplateId(TemplateID);
		//				}
		//				else
		//				{
		//					// retrieve from the property
		//					selectedWorkspaceExportFormatName = WorkspaceExportFormats.FirstOrDefault(f => f.Id == TemplateID)?.ExportFormatName;
		//				}
		//			}

		//			return selectedWorkspaceExportFormatName;
		//		}
		//	}

		//	/// <summary>
		//	/// Workspace Export Format
		//	/// </summary>
		//	public IReadOnlyCollection<WorkspaceExportFormatDTO> WorkspaceExportFormats
		//	{
		//		get
		//		{
		//			if (workspaceExportFormats == null)
		//			{
		//				List<WorkspaceExportFormatDTO> tempWorkspaceExportFormats = retriever.GetWorkspaceExportFormatsByWorkspaceId(Id).ToList();

		//				// Add the template for the Workspace's selected Template ID so a template that has been archived or
		//				// switched off available for all will still be available
		//				if (!tempWorkspaceExportFormats.Select(x => x.Id).Contains(TemplateID))
		//				{
		//					WorkspaceExportFormatDTO workspaceTemplate = retriever.GetWorkspaceExportFormatByTemplateId(TemplateID);

		//					if (workspaceTemplate != null)
		//					{
		//						tempWorkspaceExportFormats.Add(workspaceTemplate);
		//					}
		//				}

		//				workspaceExportFormats = tempWorkspaceExportFormats.AsReadOnly();
		//			}

		//			return workspaceExportFormats;
		//		}
		//	}

		//	/// <summary>
		//	/// Workspace Version Meta Data
		//	/// </summary>
		//	public IReadOnlyCollection<WorkspaceVersionMetaDataDTO> WorkspaceVersionMetaData
		//	{
		//		get
		//		{
		//			if (workspaceVersionMetaData == null)
		//			{
		//				workspaceVersionMetaData = retriever.GetWorkspaceVersionMetaDataByWorkspaceId(Id).ToList().AsReadOnly();
		//			}

		//			return workspaceVersionMetaData;
		//		}
		//	}

		//	/// <summary>
		//	/// Custom Fields for the workspace.
		//	/// </summary>
		//	public IReadOnlyCollection<CustomFieldDTO> CustomFields
		//	{
		//		get
		//		{
		//			if (customFields == null)
		//			{
		//				customFields = retriever.GetCustomFieldsByWorkspaceId(Id).ToList().AsReadOnly();
		//			}

		//			return customFields;
		//		}
		//	}

		//	/// <summary>
		//	/// Gets all custom field values for custom fields in the WS
		//	/// </summary>
		//	public IReadOnlyCollection<CustomFieldValueDTO> CustomFieldValues
		//	{
		//		get
		//		{
		//			if (customFieldValues == null)
		//			{
		//				if (CustomFieldSorting == CustomFieldSorting.ID)
		//				{
		//					customFieldValues = retriever.GetCustomFieldValuesByFieldIds(CustomFields.Select(x => x.Id).ToList(), Id).OrderBy(c => c.CustomFieldValueName).ToList().AsReadOnly();
		//				}
		//				else
		//				{
		//					customFieldValues = retriever.GetCustomFieldValuesByFieldIds(CustomFields.Select(x => x.Id).ToList(), Id).OrderBy(c => c.CustomFieldValueDescription).ToList().AsReadOnly();
		//				}
		//			}

		//			return customFieldValues;
		//		}
		//	}

		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		//	public Dictionary<int, ICollection<KeyValuePair<int, int>>> TaskElementsMappingWithCustomFieldsValuesAndContainerIds
		//	{
		//		get
		//		{
		//			if (taskElementsMappingWithCustomFieldsValuesAndContainerIds == null)
		//			{
		//				taskElementsMappingWithCustomFieldsValuesAndContainerIds = retriever.GetCustomFieldValueIDsContainerIdsByTaskElementIds(TaskElements.Select(x => x.Id).ToCollection());
		//			}

		//			return taskElementsMappingWithCustomFieldsValuesAndContainerIds;
		//		}
		//	}

		//	/// <summary>
		//	/// MOQ Type Table Mapping with Custom Field Values and Container IDs
		//	/// </summary>
		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		//	public Dictionary<int, ICollection<KeyValuePair<int, int>>> MoqTypeTableMappingWithCustomFieldsValuesAndContainerIds
		//	{
		//		get
		//		{
		//			if (moqTypeTableMappingWithCustomFieldsValuesAndContainerIds == null)
		//			{
		//				moqTypeTableMappingWithCustomFieldsValuesAndContainerIds = retriever.GetCustomFieldValueIDsContainerIdsByMoqTypeTableIds(MoqTypeSelections.SelectMany(x => x.TableData).Select(x => x.Id).ToCollection());
		//			}

		//			return moqTypeTableMappingWithCustomFieldsValuesAndContainerIds;
		//		}
		//	}

		//	/// <summary>
		//	/// gets all the custom field values that are associated with labor types within the workspace
		//	/// </summary>
		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		//	public Dictionary<int, ICollection<KeyValuePair<int, int>>> LaborTypesMappingWithCustomFieldsValuesAndContainerIds
		//	{
		//		get
		//		{
		//			if (laborResourcesMappingWithCustomFieldsValuesAndContainerIds == null)
		//			{
		//				laborResourcesMappingWithCustomFieldsValuesAndContainerIds = retriever.GetCustomFieldValueIDsContainerIdsByLaborTypeIds(TaskElements.SelectMany(x => x.taskElementLabors).Select(y => y.Id).ToCollection());
		//			}

		//			return laborResourcesMappingWithCustomFieldsValuesAndContainerIds;
		//		}
		//	}

		//	/// <summary>
		//	/// gets all custom field values associated with travel elements in the workspace
		//	/// </summary>
		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		//	public Dictionary<int, ICollection<KeyValuePair<int, int>>> TravelElementsMappingWithCustomFieldsValuesAndContainerIds
		//	{

		//		get
		//		{
		//			if (travelElementsMappingWithCustomFieldsValuesAndContainerIds == null)
		//			{
		//				travelElementsMappingWithCustomFieldsValuesAndContainerIds = retriever.GetCustomFieldValueIDsContainerIdsByTravelElementIds(Travels.Select(x => x.Id).ToCollection());
		//			}

		//			return travelElementsMappingWithCustomFieldsValuesAndContainerIds;
		//		}
		//	}

		//	/// <summary>
		//	/// gets all custom fields associated with travel trips in the workspace. 
		//	/// </summary>
		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		//	public Dictionary<int, ICollection<KeyValuePair<int, int>>> TravelTripsMappingWithCustomFieldsValuesAndContainerIds
		//	{
		//		get
		//		{
		//			if (travelTripMappingWithCustomFieldsValuesAndContainerIds == null)
		//			{
		//				travelTripMappingWithCustomFieldsValuesAndContainerIds = retriever.GetCustomFieldValueIDsContainerIdsByTravelTripsIds((from t in Travels
		//																																	   from s in t.TravelTrips
		//																																	   select s.Id).ToCollection());
		//			}

		//			return travelTripMappingWithCustomFieldsValuesAndContainerIds;
		//		}
		//	}

		//	/// <summary>
		//	/// Get ProPricer Exports by workspace Id.  This includes both system and workspace type exports.
		//	/// </summary>
		//	public IReadOnlyCollection<ProPricerDTO> ProPricerExports
		//	{
		//		get
		//		{
		//			if (proPricerExports == null)
		//			{
		//				proPricerExports = retriever.GetProPricerExportsByWorkspaceId(Id).ToList().AsReadOnly();
		//			}

		//			return proPricerExports;
		//		}
		//	}

		//	/// <summary>
		//	/// Performing Orgs based on Workspace's List Id
		//	/// </summary>
		//	public IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsForWsList
		//	{
		//		get
		//		{
		//			if (performingOrgsForWorkspaceList == null)
		//			{
		//				if (PerfOrgSorting == CustomFieldSorting.ID)
		//				{
		//					performingOrgsForWorkspaceList = retriever.GetPerformingOrgsByListId(PerfOrgListID).OrderBy(p => p.PerformingOrgName).ToList().AsReadOnly();
		//				}
		//				else
		//				{
		//					performingOrgsForWorkspaceList = retriever.GetPerformingOrgsByListId(PerfOrgListID).OrderBy(p => p.PerformingOrgDesc).ToList().AsReadOnly();
		//				}
		//			}

		//			return performingOrgsForWorkspaceList;
		//		}
		//	}

		//	/// <summary>
		//	/// Performing Orgs based on Boes's => Tasks/Odc/Travel
		//	/// </summary>
		//	public virtual IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsUsedInBoes
		//	{
		//		get
		//		{
		//			if (performingOrgsUsedInBoes == null)
		//			{
		//				ICollection<int> performingOrgIds = TaskElements.AsParallel().SelectMany(x => x.taskElementLabors).Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value)
		//						.Union(Odcs.AsParallel().SelectMany(x => x.ODCTypes).Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value))
		//						.Union(Travels.AsParallel().SelectMany(x => x.TravelTrips).Select(x => x.PerfOrgID))
		//						.Union(Travels.AsParallel().SelectMany(x => x.MSTTravelTrips).Select(x => x.PerfOrgID))
		//						.Distinct().ToList();

		//				if (PerfOrgSorting == CustomFieldSorting.ID)
		//				{
		//					performingOrgsUsedInBoes = retriever.GetPerformingOrgsByIds(performingOrgIds).OrderBy(p => p.PerformingOrgName).ToList().AsReadOnly();
		//				}
		//				else
		//				{
		//					performingOrgsUsedInBoes = retriever.GetPerformingOrgsByIds(performingOrgIds).OrderBy(p => p.PerformingOrgDesc).ToList().AsReadOnly();
		//				}
		//			}

		//			return performingOrgsUsedInBoes;
		//		}
		//	}

		//	/// <summary>
		//	/// Gets the global performing organization list.
		//	/// </summary>
		//	/// <returns>Global performing organization list name.</returns>
		//	public PerformingOrgListDTO PerformingOrgList
		//	{
		//		get
		//		{
		//			if (performingOrgList == null)
		//			{
		//				performingOrgList = retriever.GetPerfOrgList();
		//			}

		//			return performingOrgList;
		//		}
		//	}

		//	/// <summary>
		//	/// Materials for the WS
		//	/// </summary>
		//	public IReadOnlyCollection<MaterialDTO> Materials
		//	{
		//		get
		//		{
		//			if (materials == null)
		//			{
		//				materials = retriever.GetMaterialsByBoeIds(Boes.Select(x => x.Id).ToCollection(), false).ToList().AsReadOnly();
		//			}

		//			return materials;
		//		}
		//	}

		//	/// <summary>
		//	/// Populates RTE data for Materials
		//	/// </summary>
		//	public void LoadMaterialsRTEData()
		//	{
		//		if (materials == null)
		//		{
		//			// Data has not been pulled yet, so we can do a full retrieval, including the RTE data
		//			materials = retriever.GetMaterialsByBoeIds(Boes.Select(x => x.Id).ToCollection(), true).ToList().AsReadOnly();
		//		}
		//		else
		//		{
		//			// Data was already retrieved, so we are only missing the RTE data, which we'll now load
		//			retriever.PopulateRTEData(materials);
		//		}
		//	}

		//	/// <summary>
		//	/// ODCs for the workspace
		//	/// </summary>
		//	public IReadOnlyCollection<OtherDirectCostDTO> Odcs
		//	{
		//		get
		//		{
		//			if (odcs == null)
		//			{
		//				odcs = retriever.GetOdcCollectionByBoeIds(Boes.Select(x => x.Id).ToList(), false).ToList().AsReadOnly();
		//			}

		//			return odcs;
		//		}
		//	}

		//	/// <summary>
		//	/// Populates RTE data for all ODCs
		//	/// </summary>
		//	public void LoadODCsRTEData()
		//	{
		//		if (odcs == null)
		//		{
		//			// Data has not been pulled yet, so we can do a full retrieval, including the RTE data
		//			odcs = retriever.GetOdcCollectionByBoeIds(Boes.Select(x => x.Id).ToList(), true).ToList().AsReadOnly();
		//		}
		//		else
		//		{
		//			// Data was already retrieved, so we are only missing the RTE data, which we'll now load
		//			retriever.PopulateRTEData(odcs);
		//		}
		//	}

		//	/// <summary>
		//	/// A mapping of Boe Ids in the WS to the last user (id) that submitted them for approval
		//	/// </summary>
		//	public Dictionary<int, UserDTO> BoeIdsAndLastUserToSubmitThemForApprovalMapping
		//	{
		//		get
		//		{
		//			if (boeIdsAndLastUserToSubmitThemForApprovalMapping == null)
		//			{
		//				boeIdsAndLastUserToSubmitThemForApprovalMapping = retriever.GetBoeIdsAndAuthorsThatLastSubmittedItForApprovalForWs(Id);
		//			}

		//			return boeIdsAndLastUserToSubmitThemForApprovalMapping;
		//		}
		//	}

		//	/// <summary>
		//	/// For All Boes in the workspace, we retrieve the following users:
		//	/// - all authors
		//	/// - all subcontractor authors
		//	/// - all approvers with records in the approver responses (in Boe)
		//	/// - the user that updated the Boe
		//	/// </summary>
		//	public IReadOnlyCollection<UserDTO> GetUserDataForBoesForWs
		//	{
		//		get
		//		{
		//			if (usersAssociatedWithBoesInWs == null)
		//			{
		//				List<int> userIds = retriever.GetApprovalUserIdsByWorkspaceId(Id)
		//					.Union(Boes.SelectMany(x => x.SubcontractorAuthorIDs))
		//					.Union(Boes.SelectMany(x => x.AuthorIDs))
		//					.Union(boes.Select(x => x.UpdatedByUserId))
		//					.Distinct().ToList();

		//				usersAssociatedWithBoesInWs = retriever.GetUsersByIds(userIds).ToList().AsReadOnly();
		//			}

		//			return usersAssociatedWithBoesInWs;
		//		}
		//	}

		//	/// <summary>
		//	/// Refresh the boes.
		//	/// </summary>
		//	public virtual void RefreshBoes()
		//	{
		//		boes = null;
		//		RefreshTaskElements();  // task elements are children of BOEs, so they need to be refreshed as well
		//	}

		//	/// <summary>
		//	/// Refresh the task elements.
		//	/// </summary>
		//	public virtual void RefreshTaskElements()
		//	{
		//		taskElements = null;
		//	}

		//	/// <summary>
		//	/// Refresh the workspace variables.
		//	/// </summary>
		//	public void RefreshWorkspaceVariables()
		//	{
		//		workspaceVariables = null;
		//	}

		//	/// <summary>
		//	/// Gets a mapping of all custom field values and custom fields to Boes that use them, for the entire WS
		//	/// </summary>
		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		//	public IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> AssignedBoeIdsAndCustomFieldValuesMapping
		//	{
		//		get
		//		{
		//			if (assignedBoeIdsAndCustomFieldValuesMapping == null)
		//			{
		//				assignedBoeIdsAndCustomFieldValuesMapping = retriever.GetAllAssignedBOECustomFieldValuesForBoeIds(Boes.Select(x => x.Id).ToList());
		//			}

		//			return assignedBoeIdsAndCustomFieldValuesMapping;
		//		}
		//	}

		//	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		//	public IDictionary<int, ICollection<BoeApproverResponseDTO>> BoeMappingWithApproverResponses
		//	{
		//		get
		//		{
		//			if (boeMappingWithApproverResponses == null)
		//			{
		//				boeMappingWithApproverResponses = retriever.GetApproverResponseCollectionByWorkspaceId(Id);
		//			}

		//			return boeMappingWithApproverResponses;
		//		}
		//	}

		//	/// <summary>
		//	/// Gets the Multi WBS for this workspace.
		//	/// </summary>
		//	public WbsDTO MultiBOEWbs
		//	{
		//		get
		//		{
		//			return WbsElements.FirstOrDefault(w => w.WbsNumber == Constants.UNIQUE_MULTI_NUMBER);
		//		}
		//	}
		//	/// <summary>
		//	/// Gets the Multi Clin for this workspace.
		//	/// </summary>
		//	public ClinDTO MultiBOEClin
		//	{
		//		get
		//		{
		//			return Clins.FirstOrDefault(w => w.ClinNumber == Constants.UNIQUE_MULTI_NUMBER);
		//		}
		//	}

		/// <summary>
		/// Gets the children that can be shifted.
		/// </summary>
		public ICollection<IDateShiftable> Children
		{
			get
			{
				List<IDateShiftable> children = new List<IDateShiftable>();
				// TODO TIW
				//children.AddRange(Clins);
				//children.AddRange(Boes.Where(b => !b.CLINID.HasValue));

				return children;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has a spread of values.
		/// </summary>
		public bool HasSpread
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the date shift level.
		/// </summary>
		public Level DateShiftLevel
		{
			get
			{
				return Level.Workspace;
			}
		}

		/// <summary>
		/// Gets or sets the start date.
		/// </summary>
		public DateTime? StartDate
		{
			get
			{
				return ContractStartDate;
			}

			set
			{
				ContractStartDate = value ?? DateTime.MaxValue;
			}
		}

		/// <summary>
		/// Gets or sets the end date.
		/// </summary>
		public DateTime? EndDate
		{
			get
			{
				return ContractEndDate;
			}

			set
			{
				ContractEndDate = value ?? DateTime.MinValue;
			}
		}

		//	/// <summary>
		//	/// Updates the workspace variables.
		//	/// </summary>
		//	/// <param name="variables">The variables.</param>
		//	public void UpdateWorkspaceVariables(List<WorkspaceVariableDTO> variables)
		//	{
		//		if (variables == null)
		//		{
		//			throw new ArgumentNullException(nameof(variables));
		//		}

		//		workspaceVariables = variables.AsReadOnly();
		//	}

		//	/// <summary>
		//	/// Returns list of RTE fields which are being over-written w/ RTE templates 
		//	/// </summary>
		//	public IReadOnlyCollection<RteTemplateSource> RteOverrides
		//	{
		//		get
		//		{
		//			if (rteOverrides == null)
		//			{
		//				rteOverrides = retriever.GetWsRteOverrides(Id).ToList().AsReadOnly();
		//			}

		//			return rteOverrides;
		//		}
		//	}

		//	/// <summary>
		//	/// Template Questions & Answers
		//	/// </summary>
		//	public IReadOnlyCollection<RTECustomTemplateQuestionAnswerModelView> TemplateQuestionsAndAnswers
		//	{
		//		get
		//		{
		//			if (templateQuestionsAndAnswers == null)
		//			{
		//				templateQuestionsAndAnswers = retriever.GetQuestionsAndAnswersByWorkspaceId(Id).ToList().AsReadOnly();
		//			}

		//			return templateQuestionsAndAnswers;
		//		}
		//	}

		//	/// <summary>
		//	/// MOQ Type Selections for the entire workspace
		//	/// </summary>
		//	public IReadOnlyCollection<MoqTypeSelection> MoqTypeSelections
		//	{
		//		get
		//		{
		//			if (moqTypeSelections == null)
		//			{
		//				moqTypeSelections = retriever.GetMoqTypeSelectionsByWorkspaceId(Id).ToList().AsReadOnly();
		//			}

		//			return moqTypeSelections;
		//		}
		//	}
		}
	}