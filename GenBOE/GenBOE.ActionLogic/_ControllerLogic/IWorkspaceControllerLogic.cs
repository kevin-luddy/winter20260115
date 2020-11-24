// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.ModelView.Workspace;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;

    public interface IWorkspaceControllerLogic
    {
        /// <summary>
        /// Finds the Adjacent BOEs based on the sort column/order
        /// </summary>
        /// <param name="modelView">The workspace homepage modelview</param>
        /// <param name="boeId">The boe to find adjacent boes for.</param>
        /// <param name="sortColumn">The current sort column.</param>
        /// <param name="sortOrder">The current sort order.</param>
        /// <returns></returns>
        AdjacentItems FindAdjacentBoes(HomeWorkspaceGridModelView modelView, int boeId, string sortColumn, SortOrder sortOrder);

        /// <summary>
        /// Calculate any linked task elements that need to be updated based on the workspace variable
        /// </summary>
        /// <param name="inTaskElementsToSave">task elements to save</param>
        /// <param name="inWorkspace">The <see cref="FullWorkspace"/> object</param>
        void CalculateLinkedTaskElements(Collection<BoeTaskElementDTO> inTaskElementsToSave, FullWorkspace inWorkspace);

        IWorkspaceIdentificationModelView GetWorkspaceIdentificationModelView(FullWorkspace workspace);

        ICreateWorkspaceModelView GetCreateWorkspaceModelView();

        WorkspaceResourceRateGridTMModelView GetWorkspaceResourceRateGridTMModelView(FullWorkspace workspace, WorkspaceResourceRateGridTMModelView modelView);

        WorkspaceResourceRateTMModelView GetWorkspaceResourceRateTMModelView(FullWorkspace workspace, int workspaceResourceRateID);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<string, Tuple<bool, int>> GetWorkspaceResourcesForWorkspaceResourceRateTM(FullWorkspace workspace);

        bool IsWorkspaceResourceDateWithinContract(FullWorkspace workspace, string dateString);

        bool IsWorkspaceResourceRateDateValidWithoutOverlapTM(FullWorkspace workspace, int currentResourceRateID, int resourceID, string startDateString, string endDateString);
        
        /// <summary>
        /// SaveWorkspaceResourceRatesTM
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="inWorkspaceResourceRateTMModelViews"></param>
        /// <returns>bool</returns>
        bool SaveWorkspaceResourceRatesTM(FullWorkspace workspace, Collection<WorkspaceResourceRateTMModelView> inWorkspaceResourceRateTMModelViews);

        /// <summary>
        /// Gets a filtered list of Resources
        /// </summary>
        /// <param name="inWorkspaceResources">The list of resources to filter</param>
        /// <returns></returns>
        ICollection<ResourceDTO> GetFilteredWorkspaceResources(IReadOnlyCollection<ResourceDTO> inWorkspaceResources);

        /// <summary>
        /// Gets a filtered list of T&amp;M Resources
        /// </summary>
        /// <param name="workspaceResources">The list of resources to filter</param>
        /// <returns></returns>
        ICollection<ResourceDTO> GetFilteredWorkspaceResourcesTM(IReadOnlyCollection<ResourceDTO> workspaceResources);

        /// <summary>
        /// Gets a filtered list of Resources
        /// </summary>
        /// <param name="inWorkspaceResources">The list of resources to filter</param>
        /// <returns></returns>
        ICollection<ResourceDTO> GetFilteredOtherWorkspaceResources(IReadOnlyCollection<ResourceDTO> inWorkspaceResources);

        /// <summary>
        /// Get a collection of ModelView objects for BOEs that are potential candidates for Workoffline export.
        /// </summary>
        /// <param name="workspace">The <see cref="FullWorkspace"/> object</param>
        /// <returns></returns>
        ICollection<ExportBOEModelView> GetExportBOEModelData(FullWorkspace workspace);

        /// <summary>
        /// Return true, if user can export boe.
        /// </summary>
        /// <param name="isAuthor">A boolean idicating if this is an author</param>
        /// <param name="isWorkspaceAdmin">This parameter is ignored in IS&amp;GS mode.</param>
        /// <returns></returns>
        bool CanExportBoeForWorkoffline(bool isAuthor, bool isWorkspaceAdmin);
        WorkspaceResourceRateTMModelView CreateWorkspaceResourceRateTMModelView();
        WorkspaceResourceRateTMModelView CreateWorkspaceResourceRateTMModelView(TMResourceRateDTO resourceRateDTO, ResourceDTO workspaceResourceDTO, bool inUse);

        /// <summary>
        /// Get the Model Views for the Custom Fields grid
        /// </summary>
        /// <param name="customFields">Custom Fields</param>
        /// <param name="workspaceId">The workspace Id.</param>
        /// <returns>Model Views for the Custom Fields grid</returns>
        ICollection<BOECustomFieldsGridModelView> GetCustomFieldsGridModelViews(ICollection<CustomFieldDTO> customFields, int workspaceId);

        /// <summary>
        /// Takes a ModelView object and persists the data contained
        /// </summary>
        /// <param name="importedData">ModelView containing the import data</param>
        /// <param name="workspace">Workspace to import data into</param>
        /// <returns></returns>
        bool CompleteImportWorkspaceFromExcel(ImportWorkofflineResultsModelView importedData, FullWorkspace workspace);

        /// <summary>
        /// Gets the default ExcelReportTemplateType for a new workspace
        /// </summary>
        /// <returns>Default ExcelReportTemplateType</returns>
        ExcelReportTemplateType GetDefaultReportTemplateType();

        /// <summary>
        /// Gets the default picklist values for template types
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <returns>Collection of template types</returns>
        ICollection<ExcelReportTemplateType> GetPicklistReportTemplateTypes(FullWorkspace ws);

        /// <summary>
        /// Create a copy of a previous verison of the workspace
        /// </summary>
        /// <param name="ws">The Workspace</param>
        /// <param name="versionId">ID of the version</param>
        /// <param name="exportAllBoes">bool noting if all boes should be copied</param>
        /// <param name="boesToExport">list of BOE IDs if copying select BOEs</param>
        /// <returns>ID of the new temporary Workspace</returns>
        int CopyWorkspaceVersion(FullWorkspace ws, int versionId, bool exportAllBoes, ICollection<int> boesToExport);

        /// <summary>
        /// Create the Workspace Data Report for a previous version of a Workspace
        /// </summary>
        /// <param name="ws">The temporary copy of the previous workspace version</param>
        /// <param name="templateFileLocation">template file location</param>
        /// <param name="originalWorkspaceName">Original Workspace name</param>
        /// <param name="versionId">Version ID</param>
        /// <returns>File location of Workspace Data Report for the previous version</returns>
        string CreateWorkspaceDataReportForVersion(FullWorkspace ws, string templateFileLocation, MetricNameTaskElementMappingDTO metricTaskElementMappings, string originalWorkspaceName, int versionId);

        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">The <see cref="ICreateWorkspaceModelView"/> to populate</param>
        /// <param name="workspace">The <see cref="WorkspaceDTO"/> containing data used to populate the model view</param>
        void PopulateCompanySpecificWorkspaceProperties(ICreateWorkspaceModelView theModel, WorkspaceDTO workspace);

        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="workspaceSearchResults">The <see cref="WorkspaceSearchResultModelView"/> to populate</param>
        void PopulateCompanySpecificProperties(WorkspaceSearchResultModelView workspaceSearchResults);

        /// <summary>
        /// Gets the Workspace Identificatioin view name for the IS&amp;GS mode
        /// </summary>
        String WorkspaceIdentificationViewName { get; }

        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">The <see cref="IWorkspaceIdentificationModelView"/> to populate</param>
        /// <param name="workspace">The <see cref="WorkspaceDTO"/> containing data used to populate the model view</param>
        void PopulateCompanySpecificWorkspaceProperties(IWorkspaceIdentificationModelView theModel, WorkspaceDTO workspace);

        /// <summary>
        /// Identify all task and workspace variables that reference the designated BOE.  Recalculate the values of each such variable as well as the
        /// hourly totals (and spread distributions) for any task elements that USE them.  Continue (recursively) resolving references to any
        /// NESTED variables until the process is complete.
        /// </summary>
        /// <param name="boeID">BOE PK identifier</param>
        /// <param name="ws">Workspace</param>
        void ProcessAllVariableDependencies(int boeID, FullWorkspace ws);

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        void WsRecalculationStep1(FullWorkspace ws, ref HashSet<BoeTaskElementDTO> tasksToSave, ref HashSet<WorkspaceVariableDTO> workspaceVariablesToSave, ref HashSet<FullBoe> boesToTransition, HashSet<BoeDTO> originalWsBoes, bool decimalPrecisionChanged, bool costDecimalPrecisionChanged, int costPrecision);

        /// <summary>
        /// !!! This method does not do any other transitions based on the new state, it is purely for recalculation state changes !!!
        /// Changes the workspace state -> this is used because during recalculation we want to lock the workspace; 
        /// Once the recalculation is done, we want to return it back to the original state;
        /// </summary>
        /// <param name="ws">Workspace to change; The LastUpdateDate gets updated after the save so that way it can be modified and saved again, if needed</param>
        /// <param name="currentUserID">Current User Id</param>
        /// <param name="newState">State to which the WS will be changed to</param>
        /// <param name="recalculationDate">Date/Time when the recalculation started; Null if it's finished</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        void ChangeTheWorkspaceStateDuringRecalculation(FullWorkspace ws, int currentUserID, WorkspaceState newState, DateTime? recalculationDate);

        /// <summary>
        /// Updates the Zone Travel Rates to be the System default rates.
        /// </summary>
        /// <param name="workspaceId">The id for workspace to update.</param>
        void CopySystemZoneTravelRates(int workspaceId);

        /// <summary>
        /// Gets the last updated time the Zone Travel was updated; Null if it is latest.
        /// </summary>
        /// <param name="workspaceId">The id for workspace to update.</param>
        DateTime? GetLastupdatedTimeZoneTravel(int workspaceId);

        /// <summary>
        /// Gets the last updated time the Offload was updated; Null if it is latest.
        /// </summary>
        /// <param name="workspaceId">The id for workspace to update.</param>
        DateTime? GetLastupdatedTimeOffload(int workspaceId);

        /// <summary>
        /// Performs Validation for SaveWorkspaceIdentification
        /// </summary>
        /// <param name="ws">FullWorkspace</param>
        /// <param name="workspaceDetails">IWorkspaceIdentificationModelView</param>
        /// <param name="isAdmin">Is System Admin?</param>
        /// <param name="ptmTrackingNumberNotRequired">Is PTM Tracking Number Not Required</param>
        /// <returns>ICollection of ValidationMessage</returns>
        ICollection<ValidationMessage> SaveWorkspaceIdentificationValidation(FullWorkspace ws, IWorkspaceIdentificationModelView workspaceDetails, bool isAdmin, bool ptmTrackingNumberNotRequired);

        /// <summary>
        /// CostVolumeLeadPricer Validation
        /// </summary>
        /// <param name="inCostVolumeLeadPricerNTID">Cost Volume Lead Pricer NTID</param>
        /// <returns>ICollection of ValidationMessage</returns>
        ICollection<ValidationMessage> CostVolumeLeadPricerValidation(string inCostVolumeLeadPricerNTID);

        /// <summary>
        /// Deletes the given workspace variables
        /// </summary>
        /// <param name="variables">Variables to delete</param>
        void DeleteWorkspaceVariables(Collection<WorkspaceVariableDTO> variables);

        /// <summary>
        /// Saves the project map data.
        /// </summary>
        /// <param name="projectMapData">The project map data.</param>
        /// <param name="workspace">The full workspace.</param>
        void SaveProjectMapData(ICollection<ProjectMapModelView> projectMapData, FullWorkspace workspace);

        /// <summary>
        /// Adjust precision for Project Map.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <param name="decimalPrecisionChanged">if set to <c>true</c> [decimal precision changed].</param>
        /// <param name="costDecimalPrecisionChanged">if set to <c>true</c> [cost decimal precision changed].</param>
        void ProjectMapAdjustPrecision(FullWorkspace ws, bool decimalPrecisionChanged, bool costDecimalPrecisionChanged);

        /// <summary>
        /// Gets the workspace emails.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        /// <returns>
        /// A collection of Workspace Emails.
        /// </returns>
        ICollection<WorkspaceEmailOverrideModelView> GetWorkspaceEmails(int workspaceId);

        /// <summary>
        /// Saves the workspace emails.
        /// </summary>
        /// <param name="emails">The emails.</param>
        /// <param name="workspaceId">The workspace identifier.</param>
        void SaveWorkspaceEmails(ICollection<WorkspaceEmailOverrideModelView> emails, int workspaceId);

        /// <summary>
        /// Converts the PTM line of business.
        /// </summary>
        /// <param name="lineOfBusinessID">The line of business identifier.</param>
        /// <returns>The BOE id for a LOB.</returns>
        int ConvertPTMLineOfBusiness(int lineOfBusinessID);

        /// <summary>
        /// Converts the PTM contract type id to BOE id.
        /// </summary>
        /// <param name="contractTypeId">The contract type identifier.</param>
        /// <returns>BOE id or -1</returns>
        int ConvertPTMContractTypeId(int contractTypeId);

        /// <summary>
        /// Converts the PTM proposal class id to BOE id.
        /// </summary>
        /// <param name="proposalClassId">The proposal class identifier.</param>
        /// <returns>BOE id or -1</returns>
        int ConvertPTMProposalClassId(int proposalClassId);

        /// <summary>
        /// Creates the default Sikorsky Custom Fields
        /// </summary>
        /// <param name="wsId">The workspace Id.</param>
        void CreateSikorskyCustomFields(int wsId);

        /// <summary>
        /// Gets MOQ Type data when WS is changing from not using BOE Templates to using BOE Templates. This data still needs to be saved later
        /// </summary>
        /// <param name="ws">Workspace which is being saved</param>
        /// <returns>MOQ Type Data to save</returns>
        ICollection<MoqTypeSelection> GetMoqTypesDataForBoeTemplateSettingChange(FullWorkspace ws);

        /// <summary>
        /// Save MOQ Types
        /// </summary>
        /// <param name="moqTypesToSave">Moq Types To Save</param>
        void SaveMoqTypes(ICollection<MoqTypeSelection> moqTypesToSave);
    }
}
