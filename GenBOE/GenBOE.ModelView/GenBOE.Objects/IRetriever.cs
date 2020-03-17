// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Objects
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// Retrieves child full objects / dtos for the Full Objects.
    /// </summary>
    public interface IRetriever
    {
        #region Collections by Workspace Id

        /// <summary>
        /// Gets all the clins that belong to the workspace
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <returns>Clins</returns>
        ICollection<FullClin> GetClinsByWorkspaceId(int workspaceId);

        /// <summary>
        /// Gets all FullBoe objects that belong to the workspace (WITHOUT RTE DATA)
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <returns>FullBoe objects</returns>
        ICollection<FullBoe> GetFullBoesByWorkspaceId(int workspaceId);

        /// <summary>
        /// Gets all FullBoe objects that belong to the workspace
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="loadRteData">Indicate whether RTE data should be loaded automatically</param>
        /// <returns>FullBoe objects</returns>
        ICollection<FullBoe> GetFullBoesByWorkspaceId(int workspaceId, bool loadRteData);

        /// <summary>
        /// Gets all Wbs elements that belong to the workspace
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <returns>Wbs Elements</returns>
        ICollection<FullWbs> GetFullWbsElementsByWorkspaceId(int workspaceId);

        /// <summary>
        /// Gets Full Workspace variables by workspace Id.
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <returns>Workspace Variables</returns>
        ICollection<WorkspaceVariableDTO> GetWorkspaceVariableDTOsByWorkspaceId(int workspaceId);

        /// <summary>
        /// All workspace variable Ids where the BOE is part of the sum of BOEs for the variable.
        /// </summary>
        /// <param name="boeId">Boe Id.</param>
        /// <returns>All workspace variable ids for the Boe where it is summed.</returns>
        ICollection<int> GetWorkspaceVariableIdsForBoe(int boeId);

        /// <summary>
        /// Returns all travel IDs for a given workspace.
        /// </summary>
        /// <param name="inWorkspaceID">Workspace Id</param>
        /// <param name="loadRteData">Indicate whether RTE data should be loaded automatically</param>
        /// <returns>Collection of travel dtos by workspace Id.</returns>
        ICollection<TravelDTO> GetTravelByWorkspaceId(int inWorkspaceID, bool loadRteData);
        
        /// <summary>
        /// Gets all Custom Fields for the given workspace id.
        /// </summary>
        /// <param name="workspaceId">Workspace Id.</param>
        /// <returns>All CustomField Dto for a workspace.</returns>
        ICollection<CustomFieldDTO> GetCustomFieldsByWorkspaceId(int workspaceId);

        /// <summary>
        /// Gets custom field values based on the custom field ids
        /// </summary>
        /// <param name="customFieldIds"></param>
        /// <param name="workspaceId">The workspace Id.</param>
        /// <returns></returns>
        ICollection<CustomFieldValueDTO> GetCustomFieldValuesByFieldIds(ICollection<int> customFieldIds, int workspaceId);

        /// <summary>
        /// Gets a mapping of labor type ids and custom field ids/container ids
        /// </summary>
        /// <param name="laborTypeIds">Labor Types</param>
        /// <returns>Data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIdsByLaborTypeIds(Collection<int> laborTypeIds);

        /// <summary>
        /// Gets a mapping of task element ids and custom field ids/container ids
        /// </summary>
        /// <param name="taskElementIds">Task Elements</param>
        /// <returns>Data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIdsByTaskElementIds(Collection<int> taskElementIds);

        /// <summary>
        /// Gets a mapping of travel element ids and custom field ids/container ids
        /// </summary>
        /// <param name="taskElementIds">Travel Elements</param>
        /// <returns>Data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIdsByTravelElementIds(Collection<int> travelElementIds);
        /// <summary>
        /// Gets a mapping of travel trips ids and custom field ids/container ids
        /// </summary>
        /// <param name="travelTripIds">travel trips</param>
        /// <returns>Data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Dictionary<int, ICollection<KeyValuePair<int, int>>> GetCustomFieldValueIDsContainerIdsByTravelTripsIds(Collection<int> travelTripIds);
        /// <summary>
        /// Gets workspace history by workspace Id.
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <returns>Workspace History DTOs</returns>
        ICollection<WorkspaceHistoryDTO> GetWorkspaceHistoryByWorkspaceId(int workspaceId);

        // <summary>
        /// Get ProPricer Exports by workspace Id.  This includes both system and workspace type exports.
        /// </summary>
        /// <param name="workspaceId">Workspace Id.</param>
        /// <returns>ProPricer Export Dtos.</returns>
        ICollection<ProPricerDTO> GetProPricerExportsByWorkspaceId(int workspaceId);

        /// <summary>
        /// Gets workspace export format by workspace Id.
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>Workspace Export Format DTOs</returns>
        ICollection<WorkspaceExportFormatDTO> GetWorkspaceExportFormatsByWorkspaceId(int wsId);

        /// <summary>
        /// Get Workspace Export Format DTO by the template ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>Workspace Export Format DTO for the given template ID</returns>
        WorkspaceExportFormatDTO GetWorkspaceExportFormatByTemplateId(int id);

        /// <summary>
        /// Gets workspace version metadata by workspace Id.
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>Workspace Version Meta Data DTOs</returns>
        ICollection<WorkspaceVersionMetaDataDTO> GetWorkspaceVersionMetaDataByWorkspaceId(int wsId);

        /// <summary>
        /// Gets all travel element trips in the workspace.
        /// </summary>
        /// <param name="tripIds">Trip Ids.</param>
        /// <param name="workspace">Workspace.</param>
        /// <returns>Travel Trips for the workspace.</returns>
        ICollection<TripDTO> GetTravelTripsForWorkspace(ICollection<int> tripIds, WorkspaceDTO workspace);

        /// <summary>
        /// Gets travel trip data by trip Id
        /// </summary>
        /// <param name="id">Trip Id</param>
        /// <param name="workspace">Workspace</param>
        /// <returns>Trip Dto</returns>
        TripDTO GetTravelTripById(int id, WorkspaceDTO workspace);

        /// <summary>
        /// Gets all per diems for travel element trips in the workspace.
        /// </summary>
        /// <param name="perDiemIds">Per Diem Ids.</param>
        /// <param name="workspace">Workspace.</param>
        /// <returns>Travel Trip per diems for the workspace.</returns>
        ICollection<PerDiemDTO> GetTravelTripPerDiemsForWorkspace(ICollection<int> perDiemIds, WorkspaceDTO workspace);

        /// <summary>
        /// Gets all misc travel rates for travel element trips in the workspace.
        /// </summary>
        /// <param name="perDiemIds">Misc Travel Rate Ids.</param>
        /// <param name="workspace">Workspace.</param>
        /// <returns>Travel Trip per diems for the workspace.</returns>
        ICollection<MiscTravelRateDTO> GetTravelTripMiscTravelRatesForWorkspace(ICollection<int> rateIds, WorkspaceDTO workspace);

        /// <summary>
        /// Gets all Escalation rates for a workspace.
        /// </summary>
        /// <param name="workspace">Workspace.</param>
        /// <returns>Escalation rates for the workspace.</returns>
        ICollection<EscalationRatesDTO> GetEscalationRatesByWorkspace(WorkspaceDTO workspace);

        /// <summary>
        /// Gets all T&amp;M resource rates for the workspace.
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <returns>T&amp;M resource rates for a given workspace.</returns>
        ICollection<TMResourceRateDTO> GetTMResourceRates(int workspaceId);

        #endregion

        /// <summary>
        /// Gets the currently active user.
        /// </summary>
        UserDTO GetCurrentActiveUser();

        /// <summary>
        /// Get Boes By Clin Id
        /// </summary>
        /// <param name="clinId">Clin Id</param>
        /// <returns>Corresponding Boes</returns>
        ICollection<FullBoe> GetFullBoesByClinId(int clinId);

        /// <summary>
        /// Get Boe Comments By Boe Id
        /// </summary>
        /// <param name="boeId">Boe Id</param>
        /// <returns>Corresponding Boe Comments</returns>
        ICollection<BOECommentDTO> GetCommentsByBoeId(int boeId);

        /// <summary>
        /// Gets all Materials by BOE Id,
        /// </summary>
        /// <param name="inBoeID">BOE Id.</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>MaterialDTOs by BOE Id.</returns>
        ICollection<MaterialDTO> GetMaterialCollectionByBoeID(int inBoeID, bool includeRTEFields);

        /// <summary>
        /// Gets all Travels by BOE Id,
        /// </summary>
        /// <param name="inBoeID">BOE Id.</param>
        /// <param name="loadRteData">Indicate whether RTE data should be loaded automatically</param>
        /// <returns>TravelDTOs by BOE Id.</returns>
        ICollection<TravelDTO> GetTravelCollectionByBoeID(int inBoeID, bool loadRteData);

        #region Individual Dtos

        /// <summary>
        /// Gets Workspace Dto By Workspace Id
        /// </summary>
        /// <param name="workspaceId">Id</param>
        /// <returns>Workspace</returns>
        WorkspaceDTO GetWorkspaceById(int workspaceId);

        /// <summary>
        /// Gets Full Workspace Object By Workspace Id
        /// </summary>
        /// <param name="workspaceId">Id</param>
        /// <returns>Workspace</returns>
        FullWorkspace GetFullWorkspaceById(int workspaceId);

        /// <summary>
        /// Get Clin By Id
        /// </summary>
        /// <param name="clinId">Id</param>
        /// <returns>Clin</returns>
        ClinDTO GetClinById(int clinId);

        /// <summary>
        /// Get Clins By Ids
        /// </summary>
        /// <param name="clinIds">Ids</param>
        /// <returns>Clin</returns>
        ICollection<ClinDTO> GetClinsByIds(ICollection<int> clinIds);

        /// <summary>
        /// Get Wbs By Id
        /// </summary>
        /// <param name="wbsId">Id</param>
        /// <returns>Wbs</returns>
        WbsDTO GetWbsById(int wbsId);

        #endregion

        /// <summary>
        /// Gets Workspace Variables Ids by Clin Id
        /// </summary>
        /// <param name="clinId">Clin Id</param>
        /// <returns>Workspace Variable Ids</returns>
        ICollection<int> GetWorkspaceVariablesByClinId(int clinId);

        /// <summary>
        /// Gets Task Variable Ids by Clin Id
        /// </summary>
        /// <param name="clinId">Clin Id</param>
        /// <returns>Boe Task Variable Ids</returns>
        ICollection<int> GetTaskVariableIdsByClinId(int clinId);

        /// <summary>
        /// Returns resources based on Resource List Id
        /// </summary>
        /// <param name="resourceListId">Resource List Id</param>
        /// <returns>Resources</returns>
        ICollection<ResourceDTO> GetResourcesByResourceListId(int resourceListId);

        /// <summary>
        /// Returns resources based on Resource Ids
        /// </summary>
        /// <param name="resourceIds">Resource Ids</param>
        /// <returns>Resources</returns>
        ICollection<ResourceDTO> GetResourcesByIds(ICollection<int> resourceIds);        

        /// <summary>
        /// Returns all system level LM Labor resources.
        /// </summary>
        /// <returns>System Level LM Labor Resources.</returns>
        ICollection<ResourceDTO> GetSystemLmLaborResources();

        /// <summary>
        /// Get Performing Orgs based on the list Id
        /// </summary>
        /// <param name="performingOrgListId">List Id</param>
        /// <returns>Performing List Id</returns>
        ICollection<PerformingOrgDTO> GetPerformingOrgsByListId(int performingOrgListId);

        /// <summary>
        /// Gets the global performing organization list.
        /// </summary>
        /// <returns>Global performing organization list name.</returns>
        PerformingOrgListDTO GetPerfOrgList();

        /// <summary>
        /// Get Performing Orgs based on the Ids
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <returns>Performing Orgs/returns>
        ICollection<PerformingOrgDTO> GetPerformingOrgsByIds(ICollection<int> ids);

        /// <summary>
        /// Get Materials for Boe Ids
        /// </summary>
        /// <param name="boeIds">Boe Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Material Dtos</returns>
        ICollection<MaterialDTO> GetMaterialsByBoeIds(Collection<int> boeIds, bool includeRTEFields);

        /// <summary>
        /// Get location data by ids
        /// </summary>
        /// <param name="ids">Ids of locations</param>
        /// <returns>Corresponding locations</returns>
        ICollection<LocationDTO> GetLocationDataByIds(ICollection<int> ids);

        /// <summary>
        /// Used during exports, this methods gets (for every BoeId in the WS) the last user to submit the Boe for approval
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>Mapping of BoeIds and UserIds</returns>
        Dictionary<int, UserDTO> GetBoeIdsAndAuthorsThatLastSubmittedItForApprovalForWs(int wsId);

        /// <summary>
        /// Gets User Data by UserIds
        /// </summary>
        /// <param name="ids">User Ids</param>
        /// <returns>User Data</returns>
        ICollection<UserDTO> GetUsersByIds(ICollection<int> ids);

        #region Boe Stuff

        /// <summary>
        /// Gets the number of materials for the specified Boe
        /// </summary>
        /// <param name="boeId">BOE Id</param>
        /// <returns>Number of materials</returns>
        int GetNumberOfMaterialsForBoeId(int boeId);

        /// <summary>
        /// GetLaborTypesTypesForBoeId
        /// </summary>
        /// <param name="boeId">Boe Id</param>
        /// <returns>Corresponding Labor Types</returns>
        ICollection<ResourceTypeDto> GetLaborTypesTypesForBoeId(int boeId);

        /// <summary>
        /// Check if a BOE exists given a custom field ID
        /// </summary>
        /// <param name="boeId">BOE</param>
        /// <param name="customFieldId">Custom Field</param>
        /// <returns>true if it exists, false if not</returns>
        bool CheckIfBoeExistsGivenCustomFieldID(int boeId, int customFieldId);

        /// <summary>
        /// check if the BOE contains Labor/Cost Elements(Labor, Mission, ODC, etc)
        /// </summary>
        /// <param name="boeId"></param>
        /// <returns></returns>
        bool CheckIfBoeContainsLaborCostElements(int boeId);

        /// <summary>
        /// check if the BOE contains Material Elements
        /// </summary>
        /// <param name="boeId"></param>
        /// <returns></returns>
        bool CheckIfBoeContainsMaterialElements(int boeId);

        /// <summary>
        /// Gets Task Variable Ids based on Boe
        /// </summary>
        /// <param name="boeId">Boe Id</param>
        /// <returns>Task Variable Ids</returns>
        ICollection<int> GetTaskVariableIdsByBoeId(int boeId);

        /// <summary>
        /// Gets the project map data.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <param name="page">The page number.</param>
        /// <returns>A page of data for the front-end.</returns>
        ProjectMapPageModelView GetProjectMapPagedData(int workspaceId, int page);

        /// <summary>
        /// Gets a list of task elements associated with a Boe.
        /// </summary>
        /// <param name="boeId">Boe Id.</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <param name="costPrecision">The Cost precision for the workspace.</param>
        /// <param name="hoursPrecision">The Hours precision for the workspace.</param>
        /// <returns>Task elements associated to a Boe.</returns>
        ICollection<BoeTaskElementDTO> GetBoeTaskElementCollectionByBoeId(int boeId, bool includeRTEFields, int hoursPrecision, int costPrecision);

        /// <summary>
        /// Gets a list of task elements associated with a Workspace.
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <param name="costPrecision">The Cost precision for the workspace.</param>
        /// <param name="hoursPrecision">The Hours precision for the workspace.</param>
        /// <returns>Task elements associated to a Workspace.</returns>
        ICollection<BoeTaskElementDTO> GetBoeTaskElementCollectionByWorkspaceId(int workspaceId, bool includeRTEFields, int hoursPrecision, int costPrecision);

            /// <summary>
        /// Gets a list of approver responses associated with a Boe.
        /// </summary>
        /// <param name="boeId">Boe Id.</param>
        /// <returns>Approver responses associated to a Boe.</returns>
        ICollection<BoeApproverResponseDTO> GetApproverResponseCollectionByBoeId(int boeId);

        /// <summary>
        /// Gets a list of approver responses associated with a Workspace.
        /// </summary>
        /// <param name="workspaceId">The Workspace.</param>
        /// <returns>Approver responses associated to a Workspace.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<int, ICollection<BoeApproverResponseDTO>> GetApproverResponseCollectionByWorkspaceId(int workspaceId);

        /// <summary>
        /// Gets a list of approver user ids associated with Boes inside a workspace.
        /// </summary>
        /// <param name="workspaceId">Workspace to get approval user Ids for.</param>
        /// <returns>List of approver user ids associated with Boes inside a workspace.</returns>
        ICollection<int> GetApprovalUserIdsByWorkspaceId(int workspaceId);

        /// <summary>
        /// Get all custom field values assigned to the given BOE.
        /// </summary>
        /// <returns>Dictionary of custom field values to custom field dto for the BOE</returns>
        IDictionary<CustomFieldValueDTO, CustomFieldDTO> GetAllAssignedBOECustomFieldValuesForBOE(int boeId);

        /// <summary>
        /// Get all custom field values assigned to the given BOE.
        /// </summary>
        /// <returns>Dictionary of custom field values to custom field dto for the BOE</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<int, IDictionary<CustomFieldValueDTO, CustomFieldDTO>> GetAllAssignedBOECustomFieldValuesForBoeIds(ICollection<int> boeIds);

        /// <summary>
        /// Gets a list of other direct Costs associated with Boes.
        /// </summary>
        /// <param name="boeIds">Boe Ids</param>
        /// <param name="loadRteData">Indicate whether RTE data should be loaded automatically</param>
        /// <returns>Other direct Costs assiciated with Boes.</returns>
        ICollection<OtherDirectCostDTO> GetOdcCollectionByBoeIds(ICollection<int> boeIds, bool loadRteData);

        #endregion

        #region Wbs Stuff
        
        /// <summary>
        /// Gets Boes that belong to the Wbs
        /// </summary>
        /// <param name="wbsId">Wbs Id</param>
        /// <returns>Corresponding Boes</returns>
        ICollection<FullBoe> GetBoesByWbs(int wbsId);

        /// <summary>
        /// Gets Boes WITH nesting that belong to the Wbs
        /// </summary>
        /// <param name="wbsId">Wbs Id</param>
        /// <returns>Corresponding Boes</returns>
        ICollection<FullBoe> GetBoesWithNestingByWbs(int wbsId);

        /// <summary>
        /// Gets parent Wbs Elements
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="wbsNumber">Wbs Number</param>
        /// <returns>A collection of Full Wbs Elements</returns>
        ICollection<FullWbs> GetParentWbs(int workspaceId, string wbsNumber);

        /// <summary>
        /// Gets child Wbs Elements
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="wbsNumber">Wbs Number</param>
        /// <returns>A collection of Full Wbs Elements</returns>
        ICollection<FullWbs> GetChildWbs(int workspaceId, string wbsNumber);

        /// <summary>
        /// Gets Task Variables based on Wbs Id
        /// </summary>
        /// <param name="wbsId">Wbs Id</param>
        /// <returns>Task Variables</returns>
        ICollection<int> GetTaskVariablesByWbsId(int wbsId);

        /// <summary>
        /// Gets Workspace Variables based on Wbs Id
        /// </summary>
        /// <param name="wbsId">Wbs Id</param>
        /// <returns>Task Variables</returns>
        ICollection<int> GetWorkspaceVariablesByWbsId(int wbsId);

        #endregion

        #region RTE Fields

        /// <summary>
        /// Populates RTE fields for the supplied objects
        /// </summary>
        /// <param name="itemsToLoad">items whose RTE fields will be loaded</param>
        void PopulateRTEData(ICollection<FullBoe> itemsToLoad);

        /// <summary>
        /// Populates RTE fields for the supplied objects
        /// </summary>
        /// <param name="itemsToLoad">items whose RTE fields will be loaded</param>
        void PopulateRTEData(ICollection<TravelDTO> itemsToLoad);

        /// <summary>
        /// Populates RTE fields for the supplied objects
        /// </summary>
        /// <param name="itemsToLoad">items whose RTE fields will be loaded</param>
        void PopulateRTEData(ICollection<OtherDirectCostDTO> itemsToLoad);

        /// <summary>
        /// Populates RTE fields for the supplied objects
        /// </summary>
        /// <param name="itemsToLoad">items whose RTE fields will be loaded</param>
        void PopulateRTEData(ICollection<MaterialDTO> itemsToLoad);

        /// <summary>
        /// Populates RTE fields for the supplied objects
        /// </summary>
        /// <param name="itemsToLoad">items whose RTE fields will be loaded</param>
        void PopulateRTEData(ICollection<BoeTaskElementDTO> itemsToLoad);

        #endregion

        /// <summary>
        /// Gets the project map data by workspace identifier.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <returns>A collection of project map data.</returns>
        ICollection<ProjectMapModelView> GetProjectMapDataByWorkspaceId(int workspaceId);

        /// <summary>
        /// Gets RTE Template Overrides for the specific workspace
        /// </summary>
        ICollection<RteTemplateSource> GetWsRteOverrides(int workspaceId);
    }
}