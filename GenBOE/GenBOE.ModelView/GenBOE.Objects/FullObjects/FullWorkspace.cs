// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;

    /// <summary>
    /// Workspace
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]
    [Serializable()]
    public class FullWorkspace : WorkspaceDTO, IDateShiftable
    {
        #region Private Properties

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        [NonSerialized]
        protected IRetriever retriever;
        
        [NonSerialized]
        private ICommonDataMapper commonDataMapper;

        [NonSerialized]
        private IPermissionsDTODataLoader _PermissionsLoader;
        
        private string _WorkspaceStateName = null;
        private ReadOnlyCollection<PermissionsDTO> _WorkspacePermissions = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected ReadOnlyCollection<FullWbs> wbsElements;
        private ReadOnlyCollection<FullWbs> wbsElementsNoMulti;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected ReadOnlyCollection<FullClin> clins;
        private ReadOnlyCollection<FullClin> clinsNoMulti;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected ReadOnlyCollection<FullBoe> boes;
        private ReadOnlyCollection<WorkspaceVariableDTO> workspaceVariables;
        private ReadOnlyCollection<WorkspaceHistoryDTO> workspaceHistory;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected ReadOnlyCollection<BoeTaskElementDTO> taskElements;
        private ReadOnlyCollection<ResourceDTO> resourcesByListId;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected ReadOnlyCollection<ResourceDTO> resourcesUsedInBoes;
        private ReadOnlyCollection<ResourceDTO> systemLmLaborResources;
        private ReadOnlyCollection<TravelDTO> travels;
        private ReadOnlyCollection<CustomFieldDTO> customFields;
        private ReadOnlyCollection<CustomFieldValueDTO> customFieldValues;
        private ReadOnlyCollection<TMResourceRateDTO> tmResourceRates;
        private ReadOnlyCollection<WorkspaceExportFormatDTO> workspaceExportFormats;
        private ReadOnlyCollection<WorkspaceVersionMetaDataDTO> workspaceVersionMetaData;
        private ReadOnlyCollection<ProPricerDTO> proPricerExports;
        private ReadOnlyCollection<PerformingOrgDTO> performingOrgsForWorkspaceList;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected ReadOnlyCollection<PerformingOrgDTO> performingOrgsUsedInBoes;
        private PerformingOrgListDTO performingOrgList;
        private ReadOnlyCollection<MaterialDTO> materials;
        private ReadOnlyCollection<OtherDirectCostDTO> odcs;
        private ReadOnlyCollection<EscalationRatesDTO> escalationRates;
        private ReadOnlyCollection<TripDTO> travelTrips;
        private ReadOnlyCollection<PerDiemDTO> perDiemsForTravelTrips;
        private ReadOnlyCollection<MiscTravelRateDTO> miscTravelRatesForTravelTrips;
        private UserDTO currentActiveUser;
        private ReadOnlyCollection<LocationDTO> locationsInWs;
        private Dictionary<int, ICollection<KeyValuePair<int, int>>> taskElementsMappingWithCustomFieldsValuesAndContainerIds;
        private Dictionary<int, ICollection<KeyValuePair<int, int>>> travelElementsMappingWithCustomFieldsValuesAndContainerIds;
        private Dictionary<int, ICollection<KeyValuePair<int, int>>> travelTripMappingWithCustomFieldsValuesAndContainerIds;
        private Dictionary<int, ICollection<KeyValuePair<int, int>>> laborResourcesMappingWithCustomFieldsValuesAndContainerIds;
        private Dictionary<int, UserDTO> boeIdsAndLastUserToSubmitThemForApprovalMapping;
        private ReadOnlyCollection<UserDTO> usersAssociatedWithBoesInWs;
        private IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> assignedBoeIdsAndCustomFieldValuesMapping;
        private ILookup<ElementOfCostType, ResourceDTO> workspaceResourcesLookup = null;
        private IDictionary<int, ICollection<BoeApproverResponseDTO>> boeMappingWithApproverResponses;
        
        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        internal FullWorkspace() : base()
        {
            this.retriever = GenBOEUnityContainer.Resolve<IRetriever>();
            this.commonDataMapper = GenBOEUnityContainer.Resolve<ICommonDataMapper>();
            this._PermissionsLoader = GenBOEUnityContainer.Resolve<IPermissionsDTODataLoader>();
        }

        /// <summary>
        /// Constructor from workspace Dto
        /// </summary>
        /// <param name="workspace">Workspace</param>
        internal FullWorkspace(WorkspaceDTO workspace) : this()
        {
            if (workspace != null)
            {
                foreach (PropertyInfo prop in typeof(WorkspaceDTO).GetProperties())
                {
                    if (prop.CanRead && prop.CanWrite)
                    {
                        this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(workspace, null), null);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the currently active user.
        /// </summary>
        public UserDTO CurrentActiveUser
        {
            get
            {
                if (currentActiveUser == null)
                {
                    currentActiveUser = retriever.GetCurrentActiveUser();
                }
                return currentActiveUser;
            }
        }

        /// <summary>
        /// Refreshes the currently active user.
        /// </summary>
        public void RefreshCurrentActiveUser()
        {
            currentActiveUser = null;
        }

        /// <summary>
        /// Gets the workspace state name.
        /// </summary>
        /// <returns></returns>
        public string WorkspaceStateName
        {
            get
            {
                if (string.IsNullOrEmpty(this._WorkspaceStateName))
                {
                    this._WorkspaceStateName = this.commonDataMapper.getWorkspaceStateName(this.WorkspaceState);
                }
                return this._WorkspaceStateName;
            }
        }

        /// <summary>
        /// Gets the workspace permissions.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<PermissionsDTO> WorkspacePermissions
        {
            get
            {
                if (this._WorkspacePermissions == null)
                {
                    this._WorkspacePermissions = this._PermissionsLoader.GetWorkspacePermissions(this.Id).ToList().AsReadOnly();
                }
                return this._WorkspacePermissions;
            }
        }

        /// <summary>
        /// Clins belonging to the Workspace
        /// </summary>
        public virtual IReadOnlyCollection<FullClin> Clins
        {
            get
            {
                if (this.clins == null)
                {
                    this.clins = this.retriever.GetClinsByWorkspaceId(this.Id).ToList().AsReadOnly();
                }

                return this.clins;
            }
        }
        
        /// <summary>
        /// Clins belonging to the Workspace without Multi Clin
        /// </summary>
        public virtual IReadOnlyCollection<FullClin> ClinsNoMultiClin
        {
            get
            {
                if (this.clinsNoMulti == null)
                {
                    this.clinsNoMulti = this.Clins.Where(c => c.ClinNumber != Constants.UNIQUE_MULTI_NUMBER).ToList().AsReadOnly();
                }

                return this.clinsNoMulti;
            }
        }

        /// <summary>
        /// Wbs Elements belonging to the Workspace
        /// </summary>
        public virtual IReadOnlyCollection<FullWbs> WbsElements
        {
            get
            {
                if (this.wbsElements == null)
                {
                    this.wbsElements = this.retriever.GetFullWbsElementsByWorkspaceId(this.Id).OrderBy(x => x.WbsPaddedNumber).ToList().AsReadOnly();
                }

                return this.wbsElements;
            }
        }
        
        /// <summary>
        /// Wbs Elements belonging to the Workspace without Multi
        /// </summary>
        public virtual IReadOnlyCollection<FullWbs> WbsElementsNoMultiWbs
        {
            get
            {
                if (this.wbsElementsNoMulti == null)
                {
                    this.wbsElementsNoMulti = WbsElements.Where(w => w.WbsNumber != Constants.UNIQUE_MULTI_NUMBER).OrderBy(x => x.WbsPaddedNumber).ToList().AsReadOnly();
                }

                return this.wbsElementsNoMulti;
            }
        }


        /// <summary>
        /// Boes belonging to the Workspace
        /// </summary>
        public virtual IReadOnlyCollection<FullBoe> Boes
        {
            get
            {
                this.LoadBoes();

                return this.boes;
            }
        }
        
        /// <summary>
        /// Loads BOEs into the property.. Used if you want to preload the BOEs ahead of time
        /// </summary>
        public virtual void LoadBoes()
        {
            if (this.boes == null)
            {
                this.boes = this.retriever.GetFullBoesByWorkspaceId(this.Id).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Populates RTE data for all Boes
        /// </summary>
        public virtual void LoadBoesRTEData()
        { 
            if(this.boes == null)
            { 
                // Data has not been pulled yet, so we can do a full retrieval, including the RTE data
                this.boes = this.retriever.GetFullBoesByWorkspaceId(this.Id, true).ToList().AsReadOnly();
            }
            else
            { 
                // Data was already retrieved, so we are only missing the RTE data, which we'll now load
                this.retriever.PopulateRTEData(this.boes);
            }
        }

        /// <summary>
        /// Loads BOEs and Clins into the properties.. Used if you want to preload the BOEs and Clins ahead of time.
        /// </summary>
        public virtual void LoadClinsAndBoes()
        {
            if (this.clins == null)
            {
                this.clins = this.retriever.GetClinsByWorkspaceId(this.Id).ToList().AsReadOnly();
            }

            if (this.boes == null)
            {
                this.boes = this.retriever.GetFullBoesByWorkspaceId(this.Id).ToList().AsReadOnly();
            }

            foreach (FullClin clin in this.clins)
            {
                clin.SetBoes(this.boes);
            }
        }

        /// <summary>
        /// Workspace variables belonging to the workspace.
        /// </summary>
        public IReadOnlyCollection<WorkspaceVariableDTO> WorkspaceVariables
        {
            get
            {
                if (this.workspaceVariables == null)
                {
                    this.workspaceVariables = this.retriever.GetWorkspaceVariableDTOsByWorkspaceId(this.Id).ToList().AsReadOnly();
                }

                return this.workspaceVariables;
            }
        }

        /// <summary>
        /// All task elements belonging to the workspace.
        /// </summary>
        public virtual IReadOnlyCollection<BoeTaskElementDTO> TaskElements
        {
            get
            {
                if (this.taskElements == null)
                {
                    this.taskElements = this.retriever.GetBoeTaskElementCollectionByWorkspaceId(this.Id, false, this.DecimalPrecision, this.CostDecimalPrecision).ToList().AsReadOnly();
                }

                return this.taskElements;
            }
        }

        /// <summary>
        /// Populates RTE data for Task Elements
        /// </summary>
        public virtual void LoadTaskElementRTEData()
        {
            if (this.taskElements == null)
            {
                // Data has not been pulled yet, so we can do a full retrieval, including the RTE data
                this.taskElements = this.retriever.GetBoeTaskElementCollectionByWorkspaceId(this.Id, true, this.DecimalPrecision, this.CostDecimalPrecision).ToList().AsReadOnly();
            }
            else
            {
                // Data was already retrieved, so we are only missing the RTE data, which we'll now load
                this.retriever.PopulateRTEData(this.taskElements);
            }
        }

        #region Travel And Related Data (trips, per diems and misc rates)

        /*********
         * So here's an issue. Trips/PerDiems/MiscRates are all tied to Travels. So once we have a Travel object, we can get the rest of the objects related to that travel.
         * The issue is that travel can exist in the following states:
         * 1. Travel is saved as a travel task element. In which case it belongs to a BOE, and then WS. So the element can be retrieved based on WS id.
         * 2. There are system travels (trips, per diems...). In this case we can retrieve objects via loader based on Ids, but they are not tied to the WS.
         * 3. The WS is locked/completed/, when all of these objects are moved into a different table and locked. In this case the loader does the work, so we don't have to worry about it.
         * 
         * Because of issue 1 & 2, we had to implement a GetTravel/... by Id methods inside of workspace, and these will do the following:
         * a first look if the WS objects contain the object, if yes, done
         * b if the object is not tied to the WS, then grab it from the loader
         *     
         *********/

        /// <summary>
        /// All travel elements belonging to the workspace.
        /// </summary>
        public IReadOnlyCollection<TravelDTO> Travels
        {
            get
            {
                if (this.travels == null)
                {
                    this.travels = this.retriever.GetTravelByWorkspaceId(this.Id, false).ToList().AsReadOnly();
                }

                return this.travels;
            }
        }

        /// <summary>
        /// Populates RTE data for all Travels
        /// </summary>
        public void LoadTravelRTEData()
        {
            if (this.travels == null)
            {
                // Data has not been pulled yet, so we can do a full retrieval, including the RTE data
                this.travels = this.retriever.GetTravelByWorkspaceId(this.Id, true).ToList().AsReadOnly();
            }
            else
            {
                // Data was already retrieved, so we are only missing the RTE data, which we'll now load
                this.retriever.PopulateRTEData(this.travels);
            }
        }

        /// <summary>
        /// Gets all Travel Trips for the workspace.
        /// </summary>
        public IReadOnlyCollection<TripDTO> TravelTrips
        {
            get
            {
                if (this.travelTrips == null)
                {
                    this.travelTrips = new ReadOnlyCollection<TripDTO>(new List<TripDTO>());
                    if (this.Travels.Any())
                    {
                        this.travelTrips = this.retriever.GetTravelTripsForWorkspace(Travels.SelectMany(i => i.TravelTrips).Select(i => i.SystemTripID).ToCollection<int>(), this).ToList().AsReadOnly();
                    }
                }

                return this.travelTrips;
            }
        }

        /// <summary>
        /// Gets trip by id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Trip Dto</returns>
        public TripDTO GetTripById(int id)
        {
            TripDTO trip = this.TravelTrips.FirstOrDefault(x => x.TripID == id);

            if (trip == null)
            {
               trip = this.retriever.GetTravelTripById(id, this);
            }

            return trip;
        }

        /// <summary>
        /// Gets all Per Diems for all Travel Trips for the workspace.
        /// </summary>
        public IReadOnlyCollection<PerDiemDTO> PerDiemsForTravelTrips
        {
            get
            {
                if (this.perDiemsForTravelTrips == null)
                {
                    perDiemsForTravelTrips = new ReadOnlyCollection<PerDiemDTO>(new List<PerDiemDTO>());
                    if (Travels.Any() && TravelTrips.Any())
                    {
                        this.perDiemsForTravelTrips = this.retriever.GetTravelTripPerDiemsForWorkspace(travelTrips.Select(i => i.PerDiemID).ToCollection<int>(), this).ToList().AsReadOnly();
                    }
                }

                return this.perDiemsForTravelTrips;
            }
        }

        /// <summary>
        /// Gets perDiem by Id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Per Diem</returns>
        public PerDiemDTO GetPerDiemById(int id)
        {
            PerDiemDTO perDiem = this.PerDiemsForTravelTrips.FirstOrDefault(x => x.Id == id);
            if (perDiem == null)
            {
                perDiem = this.retriever.GetTravelTripPerDiemsForWorkspace(new List<int>() { id }, this).FirstOrDefault();
            }

            return perDiem;
        }

        /// <summary>
        /// Gets all Miscellaneous travel rates for all Travel Trips for the workspace.
        /// </summary>
        public IReadOnlyCollection<MiscTravelRateDTO> MiscTravelRatesForTravelTrips
        {
            get
            {
                if (this.miscTravelRatesForTravelTrips == null)
                {
                    miscTravelRatesForTravelTrips = new ReadOnlyCollection<MiscTravelRateDTO>(new List<MiscTravelRateDTO>());
                    if (Travels.Any() && TravelTrips.Any())
                    {
                        this.miscTravelRatesForTravelTrips = this.retriever.GetTravelTripMiscTravelRatesForWorkspace(travelTrips.Select(i => i.MiscTravelRateID).ToCollection<int>(), this).ToList().AsReadOnly();
                    }
                }

                return this.miscTravelRatesForTravelTrips;
            }
        }

        /// <summary>
        /// Gets Misc Travel Dto by Id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Misc Travel Dto</returns>
        public MiscTravelRateDTO GetMiscTravelRateById(int id)
        {
            MiscTravelRateDTO dto = this.MiscTravelRatesForTravelTrips.FirstOrDefault(x => x.Id == id);

            if (dto == null)
            {
                dto = this.retriever.GetTravelTripMiscTravelRatesForWorkspace(new List<int>() { id }, this).FirstOrDefault();
            }

            return dto;
        }

        /// <summary>
        /// All location data used by trips in the WS
        /// </summary>
        public IReadOnlyCollection<LocationDTO> LocationsUsedByTrips
        {
            get
            {
                if (this.locationsInWs == null)
                {
                    ICollection<int> locationIds = this.TravelTrips.Select(x => x.DepartureLocationID)
                                                        .Union(this.travelTrips.Select(y => y.DestinationLocationID)).Distinct().ToList();

                    this.locationsInWs = this.retriever.GetLocationDataByIds(locationIds).ToList().AsReadOnly();
                }

                return this.locationsInWs;
            }
        }

        /// <summary>
        /// Gets Location Dto by Id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Location Dto</returns>
        public LocationDTO GetLocationById(int id)
        {
            LocationDTO dto = this.LocationsUsedByTrips.FirstOrDefault(x => x.Id == id);

            if (dto == null)
            {
                dto = this.retriever.GetLocationDataByIds(new List<int>() { id }).FirstOrDefault();
            }

            return dto;

        }

        #endregion

        /// <summary>
        /// Workspace History belonging to the workspace.
        /// </summary>
        public IReadOnlyCollection<WorkspaceHistoryDTO> WorkspaceHistory
        {
            get
            {
                if (this.workspaceHistory == null)
                {
                    this.workspaceHistory = this.retriever.GetWorkspaceHistoryByWorkspaceId(this.Id).ToList().AsReadOnly();
                }

                return this.workspaceHistory;
            }
        }

        /// <summary>
        /// Gets all Escalation Rates for the workspace.
        /// </summary>
        public IReadOnlyCollection<EscalationRatesDTO> EscalationRates
        {
            get
            {
                if (this.escalationRates == null)
                {
                    this.escalationRates = this.retriever.GetEscalationRatesByWorkspace(this).ToList().AsReadOnly();
                }

                return this.escalationRates;
            }
        }

        /// <summary>
        /// Workspace resources.
        /// </summary>
        public IReadOnlyCollection<ResourceDTO> ResourcesForWsResourceListId
        {
            get
            {
                if (this.resourcesByListId == null)
                {
                    if (this.ResourceSorting == CustomFieldSorting.ID)
                    {
                        this.resourcesByListId = this.retriever.GetResourcesByResourceListId(this.ResourceListID).OrderBy(r => r.ResourceName).ToList().AsReadOnly();
                    }
                    else
                    {
                        this.resourcesByListId = this.retriever.GetResourcesByResourceListId(this.ResourceListID).OrderBy(r => r.ResourceDesc).ToList().AsReadOnly();
                    }
                }

                return this.resourcesByListId;
            }
        }

        /// <summary>
        /// Returns back a collection of resources used by the Boes in the entire WS
        /// </summary>
        public virtual IReadOnlyCollection<ResourceDTO> ResourcesUsedInWsBoes
        {
            get
            {
                if (this.resourcesUsedInBoes == null)
                {
                    ICollection<int> resourceIds = this.TaskElements.SelectMany(x => x.taskElementLabors).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value)
                        .Union(this.Odcs.SelectMany(x => x.ODCTypes).Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value))
                        .Distinct().ToList();

                    if (this.ResourceSorting == CustomFieldSorting.ID)
                    {
                        this.resourcesUsedInBoes = this.retriever.GetResourcesByIds(resourceIds).OrderBy(r => r.ResourceName).ToList().AsReadOnly();
                    }
                    else
                    {
                        this.resourcesUsedInBoes = this.retriever.GetResourcesByIds(resourceIds).OrderBy(r => r.ResourceDesc).ToList().AsReadOnly();
                    }
                }

                return this.resourcesUsedInBoes;
            }
        }

        /// <summary>
        /// System level LM labor resources.
        /// </summary>
        public IReadOnlyCollection<ResourceDTO> ResourcesForSystemResourceListId
        {
            get
            {
                if (this.systemLmLaborResources == null)
                {
                    this.systemLmLaborResources = this.retriever.GetSystemLmLaborResources().ToList().AsReadOnly();
                }

                return this.systemLmLaborResources;
            }
        }

        /// <summary>
        /// Workspace resources.
        /// </summary>
        public ILookup<ElementOfCostType, ResourceDTO> WorkspaceResourcesAsLookup
        {
            get
            {
                if (this.workspaceResourcesLookup == null)
                {
                    this.workspaceResourcesLookup = ResourcesForWsResourceListId.ToLookup(x => x.ElementOfCost);
                }

                return this.workspaceResourcesLookup;
            }
        }

        /// <summary>
        /// Workspace resource rates for T&amp;M Resources.  
        /// </summary>
        public IReadOnlyCollection<TMResourceRateDTO> TMResourceRatesForWorkspace
        {
            get
            {
                if (tmResourceRates == null)
                {
                    this.tmResourceRates = this.retriever.GetTMResourceRates(this.Id).ToList().AsReadOnly();
                }

                // Return T&M Rates for workspace.
                return this.tmResourceRates;
            }
        }

        /// <summary>
        /// Workspace Export Format
        /// </summary>
        public IReadOnlyCollection<WorkspaceExportFormatDTO> WorkspaceExportFormats
        {
            get
            {
                if (this.workspaceExportFormats == null)
                {
                    List<WorkspaceExportFormatDTO> tempWorkspaceExportFormats = this.retriever.GetWorkspaceExportFormatsByWorkspaceId(this.Id).ToList();

                    // Add the template for the Workspace's selected Template ID so a template that has been archived or
                    // switched off available for all will still be available
                    if (!tempWorkspaceExportFormats.Select(x => x.Id).Contains(this.TemplateID))
                    {
                        WorkspaceExportFormatDTO workspaceTemplate = this.retriever.GetWorkspaceExportFormatByTemplateId(this.TemplateID);

                        if (workspaceTemplate != null)
                        {
                            tempWorkspaceExportFormats.Add(workspaceTemplate);
                        }
                    }

                    this.workspaceExportFormats = tempWorkspaceExportFormats.AsReadOnly();
                }

                return this.workspaceExportFormats;
            }
        }

        /// <summary>
        /// Workspace Version Meta Data
        /// </summary>
        public IReadOnlyCollection<WorkspaceVersionMetaDataDTO> WorkspaceVersionMetaData
        {
            get
            {
                if (this.workspaceVersionMetaData == null)
                {
                    this.workspaceVersionMetaData = this.retriever.GetWorkspaceVersionMetaDataByWorkspaceId(this.Id).ToList().AsReadOnly();
                }

                return this.workspaceVersionMetaData;
            }
        }

        /// <summary>
        /// Custom Fields for the workspace.
        /// </summary>
        public IReadOnlyCollection<CustomFieldDTO> CustomFields
        {
            get
            {
                if (this.customFields == null)
                {
                    this.customFields = this.retriever.GetCustomFieldsByWorkspaceId(this.Id).ToList().AsReadOnly();
                }

                return this.customFields;
            }
        }

        /// <summary>
        /// Gets all custom field values for custom fields in the WS
        /// </summary>
        public IReadOnlyCollection<CustomFieldValueDTO> CustomFieldValues
        {
            get
            {
                if (this.customFieldValues == null)
                {
                    if (this.CustomFieldSorting == CustomFieldSorting.ID)
                    {
                        this.customFieldValues = this.retriever.GetCustomFieldValuesByFieldIds(this.CustomFields.Select(x => x.Id).ToList(), this.Id).OrderBy(c => c.CustomFieldValueName).ToList().AsReadOnly();
                    }
                    else
                    {
                        this.customFieldValues = this.retriever.GetCustomFieldValuesByFieldIds(this.CustomFields.Select(x => x.Id).ToList(), this.Id).OrderBy(c => c.CustomFieldValueDescription).ToList().AsReadOnly();
                    }
                }

                return this.customFieldValues;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<int, ICollection<KeyValuePair<int, int>>> TaskElementsMappingWithCustomFieldsValuesAndContainerIds
        {
            get
            {
                if (this.taskElementsMappingWithCustomFieldsValuesAndContainerIds == null)
                {
                    this.taskElementsMappingWithCustomFieldsValuesAndContainerIds = this.retriever.GetCustomFieldValueIDsContainerIdsByTaskElementIds(this.TaskElements.Select(x => x.Id).ToCollection());
                }

                return this.taskElementsMappingWithCustomFieldsValuesAndContainerIds;
            }
        }

        /// <summary>
        /// gets all the custom field values that are associated with labor types within the workspace
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<int, ICollection<KeyValuePair<int, int>>> LaborTypesMappingWithCustomFieldsValuesAndContainerIds
        {
            get
            {
                if (this.laborResourcesMappingWithCustomFieldsValuesAndContainerIds == null)
                {
                    this.laborResourcesMappingWithCustomFieldsValuesAndContainerIds = this.retriever.GetCustomFieldValueIDsContainerIdsByLaborTypeIds(this.TaskElements.SelectMany(x => x.taskElementLabors).Select(y=>y.Id).ToCollection());
                }

                return this.laborResourcesMappingWithCustomFieldsValuesAndContainerIds;
            }
        }

        /// <summary>
        /// gets all custom field values associated with travel elements in the workspace
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<int, ICollection<KeyValuePair<int, int>>> TravelElementsMappingWithCustomFieldsValuesAndContainerIds
        {
           
            get
            {
                if (this.travelElementsMappingWithCustomFieldsValuesAndContainerIds == null)
                {
                    this.travelElementsMappingWithCustomFieldsValuesAndContainerIds = this.retriever.GetCustomFieldValueIDsContainerIdsByTravelElementIds(this.Travels.Select(x => x.Id).ToCollection());
                }

                return this.travelElementsMappingWithCustomFieldsValuesAndContainerIds;
            }
        }

        /// <summary>
        /// gets all custom fields associated with travel trips in the workspace. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<int, ICollection<KeyValuePair<int, int>>> TravelTripsMappingWithCustomFieldsValuesAndContainerIds
        {
            get
            {
                if (this.travelTripMappingWithCustomFieldsValuesAndContainerIds == null)
                {
                    this.travelTripMappingWithCustomFieldsValuesAndContainerIds = this.retriever.GetCustomFieldValueIDsContainerIdsByTravelTripsIds((from t in this.Travels
                                                                                                                                                     from s in t.TravelTrips
                                                                                                                                                     select s.Id).ToCollection());
                }

                return this.travelTripMappingWithCustomFieldsValuesAndContainerIds;
            }
        }

        /// <summary>
        /// Get ProPricer Exports by workspace Id.  This includes both system and workspace type exports.
        /// </summary>
        public IReadOnlyCollection<ProPricerDTO> ProPricerExports
        {
            get
            {
                if (this.proPricerExports == null)
                {
                    this.proPricerExports = this.retriever.GetProPricerExportsByWorkspaceId(this.Id).ToList().AsReadOnly();
                }

                return this.proPricerExports;
            }
        }

        /// <summary>
        /// Performing Orgs based on Workspace's List Id
        /// </summary>
        public IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsForWsList
        {
            get
            {
                if (this.performingOrgsForWorkspaceList == null)
                {
                    if (this.PerfOrgSorting == CustomFieldSorting.ID)
                    {
                        this.performingOrgsForWorkspaceList = this.retriever.GetPerformingOrgsByListId(this.PerfOrgListID).OrderBy(p => p.PerformingOrgName).ToList().AsReadOnly();
                    }
                    else
                    {
                        this.performingOrgsForWorkspaceList = this.retriever.GetPerformingOrgsByListId(this.PerfOrgListID).OrderBy(p => p.PerformingOrgDesc).ToList().AsReadOnly();
                    }
                }

                return this.performingOrgsForWorkspaceList;
            }
        }

        /// <summary>
        /// Performing Orgs based on Boes's => Tasks/Odc/Travel
        /// </summary>
        public virtual IReadOnlyCollection<PerformingOrgDTO> PerformingOrgsUsedInBoes
        {
            get
            {
                if (this.performingOrgsUsedInBoes == null)
                {
                    ICollection<int> performingOrgIds = this.TaskElements.AsParallel().SelectMany(x => x.taskElementLabors).Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value)
                            .Union(this.Odcs.AsParallel().SelectMany(x => x.ODCTypes).Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value))
                            .Union(this.Travels.AsParallel().SelectMany(x => x.TravelTrips).Select(x => x.PerfOrgID))
                            .Union(this.Travels.AsParallel().SelectMany(x => x.MSTTravelTrips).Select(x => x.PerfOrgID))
                            .Distinct().ToList();

                    if (this.PerfOrgSorting == CustomFieldSorting.ID)
                    {
                        this.performingOrgsUsedInBoes = this.retriever.GetPerformingOrgsByIds(performingOrgIds).OrderBy(p => p.PerformingOrgName).ToList().AsReadOnly();
                    }
                    else
                    {
                        this.performingOrgsUsedInBoes = this.retriever.GetPerformingOrgsByIds(performingOrgIds).OrderBy(p => p.PerformingOrgDesc).ToList().AsReadOnly();
                    }
                }

                return this.performingOrgsUsedInBoes;
            }
        }

        /// <summary>
        /// Gets the global performing organization list.
        /// </summary>
        /// <returns>Global performing organization list name.</returns>
        public PerformingOrgListDTO PerformingOrgList
        {
            get
            {
                if (this.performingOrgList == null)
                {
                    this.performingOrgList = this.retriever.GetPerfOrgList();
                }

                return this.performingOrgList;
            }
        }

        /// <summary>
        /// Materials for the WS
        /// </summary>
        public IReadOnlyCollection<MaterialDTO> Materials
        {
            get
            {
                if (this.materials == null)
                {
                    this.materials = this.retriever.GetMaterialsByBoeIds(this.Boes.Select(x => x.Id).ToCollection(), false).ToList().AsReadOnly();
                }

                return this.materials;
            }
        }

        /// <summary>
        /// Populates RTE data for Materials
        /// </summary>
        public void LoadMaterialsRTEData()
        {
            if (this.materials == null)
            {
                // Data has not been pulled yet, so we can do a full retrieval, including the RTE data
                this.materials = this.retriever.GetMaterialsByBoeIds(this.Boes.Select(x => x.Id).ToCollection(), true).ToList().AsReadOnly();
            }
            else
            {
                // Data was already retrieved, so we are only missing the RTE data, which we'll now load
                this.retriever.PopulateRTEData(this.materials);
            }
        }

        /// <summary>
        /// ODCs for the workspace
        /// </summary>
        public IReadOnlyCollection<OtherDirectCostDTO> Odcs
        {
            get
            {
                if (this.odcs == null)
                {
                    this.odcs = this.retriever.GetOdcCollectionByBoeIds(this.Boes.Select(x => x.Id).ToList(), false).ToList().AsReadOnly();
                }

                return this.odcs;
            }
        }

        /// <summary>
        /// Populates RTE data for all ODCs
        /// </summary>
        public void LoadODCsRTEData()
        {
            if (this.odcs == null)
            {
                // Data has not been pulled yet, so we can do a full retrieval, including the RTE data
                this.odcs = this.retriever.GetOdcCollectionByBoeIds(this.Boes.Select(x => x.Id).ToList(), true).ToList().AsReadOnly();
            }
            else
            {
                // Data was already retrieved, so we are only missing the RTE data, which we'll now load
                this.retriever.PopulateRTEData(this.odcs);
            }
        }

        /// <summary>
        /// A mapping of Boe Ids in the WS to the last user (id) that submitted them for approval
        /// </summary>
        public Dictionary<int, UserDTO> BoeIdsAndLastUserToSubmitThemForApprovalMapping
        {
            get
            {
                if (this.boeIdsAndLastUserToSubmitThemForApprovalMapping == null)
                {
                    this.boeIdsAndLastUserToSubmitThemForApprovalMapping = this.retriever.GetBoeIdsAndAuthorsThatLastSubmittedItForApprovalForWs(this.Id);
                }

                return this.boeIdsAndLastUserToSubmitThemForApprovalMapping;
            }
        }

        /// <summary>
        /// For All Boes in the workspace, we retrieve the following users:
        /// - all authors
        /// - all subcontractor authors
        /// - all approvers with records in the approver responses (in Boe)
        /// - the user that updated the Boe
        /// </summary>
        public IReadOnlyCollection<UserDTO> GetUserDataForBoesForWs
        {
            get
            {
                if (this.usersAssociatedWithBoesInWs == null)
                {
                    List<int> userIds = this.retriever.GetApprovalUserIdsByWorkspaceId(this.Id)
                        .Union(this.Boes.SelectMany(x => x.SubcontractorAuthorIDs))
                        .Union(this.Boes.SelectMany(x => x.AuthorIDs))
                        .Union(this.boes.Select(x => x.UpdatedByUserId))
                        .Distinct().ToList();

                    this.usersAssociatedWithBoesInWs = this.retriever.GetUsersByIds(userIds).ToList().AsReadOnly();
                }

                return this.usersAssociatedWithBoesInWs;
            }
        }

        /// <summary>
        /// Refresh the boes.
        /// </summary>
        public virtual void RefreshBoes()
        {
            this.boes = null;
            this.RefreshTaskElements();  // task elements are children of BOEs, so they need to be refreshed as well
        }

        /// <summary>
        /// Refresh the task elements.
        /// </summary>
        public virtual void RefreshTaskElements()
        {
            this.taskElements = null;
        }

        /// <summary>
        /// Refresh the workspace variables.
        /// </summary>
        public void RefreshWorkspaceVariables()
        {
            this.workspaceVariables = null;
        }

        /// <summary>
        /// Gets a mapping of all custom field values and custom fields to Boes that use them, for the entire WS
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> AssignedBoeIdsAndCustomFieldValuesMapping
        {
            get
            {
                if (this.assignedBoeIdsAndCustomFieldValuesMapping == null)
                {
                    this.assignedBoeIdsAndCustomFieldValuesMapping = this.retriever.GetAllAssignedBOECustomFieldValuesForBoeIds(this.Boes.Select(x => x.Id).ToList());
                }

                return this.assignedBoeIdsAndCustomFieldValuesMapping;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IDictionary<int, ICollection<BoeApproverResponseDTO>> BoeMappingWithApproverResponses
        {
            get
            {
                if (this.boeMappingWithApproverResponses == null)
                {
                    this.boeMappingWithApproverResponses = this.retriever.GetApproverResponseCollectionByWorkspaceId(this.Id);
                }

                return this.boeMappingWithApproverResponses;
            }
        }

        /// <summary>
        /// Gets the Multi WBS for this workspace.
        /// </summary>
        public WbsDTO MultiBOEWbs
        {
            get
            {
                return WbsElements.FirstOrDefault(w => w.WbsNumber == Constants.UNIQUE_MULTI_NUMBER);
            }
        } 
        /// <summary>
        /// Gets the Multi Clin for this workspace.
        /// </summary>
        public ClinDTO MultiBOEClin
        {
            get
            {
                return Clins.FirstOrDefault(w => w.ClinNumber == Constants.UNIQUE_MULTI_NUMBER);
            }
        }

        /// <summary>
        /// Gets the children that can be shifted.
        /// </summary>
        public ICollection<IDateShiftable> Children
        {
            get
            {
                List<IDateShiftable> children = new List<IDateShiftable>();
                children.AddRange(this.Clins);
                children.AddRange(this.Boes.Where(b => !b.CLINID.HasValue));

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
                return this.ContractStartDate;
            }

            set
            {
                this.ContractStartDate = value ?? DateTime.MaxValue;
            }
        }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime? EndDate
        {
            get
            {
                return this.ContractEndDate;
            }

            set
            {
                this.ContractEndDate = value ?? DateTime.MinValue;
            }
        }

        /// <summary>
        /// Updates the workspace variables.
        /// </summary>
        /// <param name="variables">The variables.</param>
        public void UpdateWorkspaceVariables(List<WorkspaceVariableDTO> variables)
        {
            if (variables == null)
            {
                throw new ArgumentNullException(nameof(variables));
            }

            this.workspaceVariables = variables.AsReadOnly();
        }
    }
}