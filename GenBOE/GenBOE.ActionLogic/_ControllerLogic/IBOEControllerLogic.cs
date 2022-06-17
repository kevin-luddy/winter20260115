// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Web;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.ActionLogic.IESSAPClient;
    using GenBOE.DataBridge.DTO;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common.Exceptions;	

	public interface IBOEControllerLogic
    {
        /// <summary>
        /// Get BOE Header Model View
        /// </summary>
        /// <param name="boe">The <see cref="BoeDTO"/> used to populate the <see cref="BOEHeaderISGSModelView"/></param>
        /// <returns>the populated <see cref="BOEHeaderISGSModelView"/></returns>
        IBOEHeaderModelView GetCreateBOEHeaderMV(BoeDTO boe, ICollection<RTECustomTemplateQuestionAnswerModelView> answers);

        /// <summary>
        /// Returns a bool indicating if historic metrics should be shown
        /// </summary>
        /// <param name="ids">The id's of the boe's to check</param>
        /// <returns>true if the metrics should be shown, false otherwise</returns>
        bool ShowHistoricMetricCheck(ICollection<int> ids);

        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">the <see cref="BOEAdvancedSearchModelView"/> to populate</param>
        void PopulateCompanySpecificProperties(BOEAdvancedSearchModelView theModel);

        /// <summary>
        /// Gets the model view for the BOE task element grid
        /// </summary>
        /// <param name="boe">BOE containing the task element grid</param>
        /// <param name="ws">Workspace the BOE exists in</param>
        /// <returns>model view for the BOE task element grid</returns>
        GenericTaskElementGridModelView GetTaskGridModelView(FullBoe boe, FullWorkspace ws);

        /// <summary>
        /// Gets the model view for the Duplicate Task Elements Dialog
        /// </summary>
        /// <param name="boe">BOE containing the tasks to be duplicated</param>
        /// <param name="taskType">Type of taks to be duplicated</param>
        /// <returns>Model view for the duplicate tasks dialog</returns>
        TaskElementDuplicateFormCollection GetDuplicateTaskModelView(FullBoe boe, TaskType taskType);

        /// <summary>
        /// Creates the model view for the BOE Header
        /// </summary>
        /// <param name="boe">BOE containing the header to create</param>
        /// <param name="ws">Workspace containing the BOE</param>
        /// <returns>model view for the BOE Header</returns>
        IBOEHeaderModelView CreateBOEHeaderMV(FullBoe boe, WorkspaceDTO ws);

        /// <summary>
        /// Gets  model views for custom fields in the BOE Header
        /// </summary>
        /// <param name="ws">workspace containing the BOE/Custom fields</param>
        /// <returns>model views for custom fields in the BOE Header</returns>
        Collection<BOECustomFieldModelView> GetCustomFieldModelViews(FullWorkspace ws);

        /// <summary>
        /// Gets the model view for the BOE Summary Grid
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boe">BOE containing BOE Summary</param>
        /// <param name="isSubcontractorUser">Bool designating if user is a subcontractor</param>
        /// <returns>model view for the BOE Summary Grid</returns>
        ICollection<BOESummaryGridModelView> GetBOESummaryGridModelViews(FullWorkspace ws, FullBoe boe, bool isSubcontractorUser);

        /// <summary>
        /// Gets the model view for the Manage BOE Grid
        /// </summary>
        /// <param name="ws">Workspace containing the BOEs</param>
        /// <param name="boes">BOEs to be managed</param>
        /// <returns>model view for the Manage BOE Grid</returns>
        ICollection<ManageBOEModelView> GetManageBOEGridData(FullWorkspace ws, IReadOnlyCollection<FullBoe> boes);


        /// <summary>
        /// Updates the BOE Start/End dates with the default dates. Either sets them to Workspace
        /// PoP dates or the CLIN dates when applicable.
        /// </summary>
        /// <param name="ws">Full Workspace</param>
        /// <param name="boe">BOE to update</param>
        /// <param name="clinId">New clin of BOE</param>
        void setDefaultBoeDates(FullWorkspace ws, FullBoe boe, int? clinId);

        /// <summary>
        /// Exports a BOE to a pre-formatted MS Word template and sends the file as a download
        /// to the user.
        /// </summary>
        /// <param name="ws">Workspace containing the boe</param>
        /// <param name="boeID">ID of BOE to preview</param>
        /// <param name="Response">current HTTP response</param>
        void ExportBOESearchPreview(FullWorkspace ws, int boeID, HttpResponseBase Response);

        /// <summary>
        /// Exports a ProjectMap to a pre-formatted MS Word template and sends the file as a download
        /// to the user.
        /// </summary>
        /// <param name="ws">Workspace containing the boe</param>
        /// <param name="projectMapId">ID of projectMapId to preview</param>
        /// <param name="Response">current HTTP response</param>
        void ExportProjectMapSearchPreview(FullWorkspace ws, int projectMapId, HttpResponseBase Response);

        /// <summary>
        /// Saves a BOE Header edit.
        /// </summary>
        /// <param name="ws">Workspace containing the BOE</param>
        /// <param name="boe">BOE containing the header</param>
        /// <param name="inBOEHeader">Modelview for the BOE Header</param>
        /// <param name="inBOEHeaderDescription">Modelview for the BOE description</param>
        /// <param name="descriptionOnly">Bool denoting if only descrpition was changed</param>
        void SaveEditBoeHeader(FullWorkspace ws, FullBoe boe, IBOEHeaderModelView inBOEHeader, BOEHeaderDescriptionModelView inBOEHeaderDescription, bool descriptionOnly);

        /// <summary>
        /// Perform the action for submitting a BOE for review
        /// </summary>
        /// <param name="ws">Workspace containing the BOE</param>
        /// <param name="boeID">ID of BOE to be submitted</param>
        void SubmitForReview(FullWorkspace ws, int boeID);

        /// <summary>
        /// Performs action to delete all task elements for a BOE
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boe">BOE Containing task elements</param>
        void DeleteAllBOETaskElements(FullWorkspace ws, FullBoe boe);

        /// <summary>
        /// Performs the action to delete the given task element
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boe">Boe containing task to be deleted</param>
        /// <param name="deletedTask">Task to be deleted</param>
        void DeleteTaskElement(FullWorkspace ws, FullBoe boe, GenericTaskElementGridRow deletedTask);

        /// <summary>
        /// For each task element that was recalculated, if their parent boe was in approved or awaiting approval state, move it back to draft
        /// </summary>
        /// <param name="workspace">Workspace containing task elements</param>
        /// <param name="boeTaskElementsToRecalculate">Elements to recalculate</param>
        void AdjustStateOfTaskElements(FullWorkspace workspace, Collection<BoeTaskElementDTO> boeTaskElementsToRecalculate);

        /// <summary>
        /// Perform the action for submitting a BOE for approval
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boe">BOE to be submitted</param>
        /// <returns>Modelview for BOE Validation</returns>
        ValidationBOEModelView SubmitForApproval(FullWorkspace ws, FullBoe boe);

        /// <summary>
        /// Performs a search for BOEs based on the params passed in
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boeID">The boe identifier.</param>
        /// <param name="advSearchParams">Parameters to search for</param>
        /// <returns>Modelview for search results</returns>
        SearchResultsModelView AdvancedSearchForBOEs(FullWorkspace ws, int boeID, BOEAdvancedSearchModelView advSearchParams);

        /// <summary>
        /// Performs a search for BOEs based on the params passed in
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="advSearchParams">Parameters to search for</param>
        /// <returns>Modelview for search results</returns>
        SearchResultsModelView AdvancedSearchForBOEs(FullWorkspace ws, BOEProjectMapAdvancedSearchModelView advSearchParams);

        /// <summary>
        /// Validate that atleast one field is filled in.
        /// </summary>
        /// <param name="advSearchParams">Parameters for advanced search</param>
        /// <returns>if search is valid or not</returns>
        bool AdvSearchRequiredFieldValidation(BOEAdvancedSearchModelView advSearchParams);

        /// <summary>
        /// Validate that atleast one field is filled in.
        /// </summary>
        /// <param name="advSearchParams">Parameters for advanced search</param>
        /// <returns>if search is valid or not</returns>
        bool AdvSearchRequiredFieldValidation(BOEProjectMapAdvancedSearchModelView advSearchParams); 

        /// <summary>
        /// Performs a quicksearch for BOEs based on the params passed in
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boeID">The boe identifier.</param>
        /// <param name="quickSearchParams">Parameters to search for</param>
        /// <returns>Modelview of search results</returns>
        SearchResultsModelView QuickSearchForBOEs(FullWorkspace ws, int? boeID, BOEQuickSearchModelView quickSearchParams);

        /// <summary>
        /// Gets data for search results
        /// </summary>
        /// <param name="ws">Workspace search is performed in</param>
        /// <param name="searchResults">Results of search</param>
        void PageSearchResults(FullWorkspace ws, SearchResultsModelView searchResults);

        /// <summary>
        /// Performs action to export BOEs from the Manage BOEs page
        /// </summary>
        /// <param name="ws">Workspace containing BOEs</param>
        /// <param name="templateFileName">file name of the template used for export</param>
        /// <param name="blankTemplate">Bool to determine if template should be blank or contain all BOEs</param>
        /// <returns>exprted file name and formatted filename in an array</returns>
        string[] ExportManageBOE(FullWorkspace ws, string templateFileName, bool blankTemplate);

        /// <summary>
        /// Performs actions to start import of BOEs on the Manage BOEs page
        /// </summary>
        /// <param name="ws">Workspace containing BOEs</param>
        /// <param name="Request">current HTTP request</param>
        /// <param name="dataToSave">(output) Data to be saved by import</param>
        /// <param name="errorsOccurred">(output) bool noting if any errors occured</param>
        /// <param name="exception">(output) Exception if any occured</param>
        /// <returns>Modelview of the import results</returns>
        Collection<ImportBoeResultsModelView> ImportManageBOE(FullWorkspace ws, HttpRequestBase Request, out ICollection<ImportBoeResultsModelView> dataToSave, out bool errorsOccurred, out Exception exception);

        //TODO: CompleteImportManageBOE once logic is moved to controllerlogic
        //WI 32107

        /// <summary>
        /// Determines if there are any conflicts when copying a BOE
        /// </summary>
        /// <param name="ws">Workspace containing BOE</param>
        /// <param name="boeID">ID of BOE being copied to</param>
        /// <param name="copyBOEID">ID of BOE being copied</param>
        /// <param name="taskElementsToCopy">Task elements being copied</param>
        /// <param name="travelElementsToCopy">Travel elements being copied</param>
        /// <returns>Modelview of copy BOE conflicts</returns>
        BOECopyConflictsModelView DisplayCopyBOEConflicts(FullWorkspace ws, int boeID, int copyBOEID, ICollection<int> taskElementsToCopy, ICollection<int> travelElementsToCopy);

        /// <summary>
        /// Calculates the defaults for the Manage BOE page
        /// </summary>
        /// <param name="theModelView">modelview of the Manage BOE Grid</param>
        /// <param name="workspace">workspace containing Mamage BOE page</param>
        void CalculateManageBOEDefaults(ManageBOEGridWidgetModelView theModelView, FullWorkspace workspace);

        /// <summary>
        /// Gets company specific header information for the manage Boe page.
        /// </summary>
        /// <returns></returns>
        string GetCompanySpecificManageBoeHeaderInfo { get; }

        /// <summary>
        /// Updates the OrderList for Taskelements in a BOE
        /// </summary>
        /// <param name="ws">Full workspace</param>
        /// <param name="boeObject">Full BOE</param>
        /// <param name="theModelView">Collection of TaskElementReOrderingCollection </param>
        void ReOrderTaskElementOrder(FullWorkspace ws, FullBoe boeObject, TaskElementOrderCollection theModelView);

        /// <summary>
        /// Validates the BOE from SaveManageBOE
        /// </summary>
        /// <param name="boes">Collection of BOES that are being saved</param>
        /// <param name="ws">full workspace for the boes</param>
        /// <param name="boePermissions">All the Permisssions</param>
        /// <param name="BoeStateDictionary">BOE States</param>
        /// <param name="ValidationErrors">Collection Of Error Messages to add too</param>
        /// <param name="workspaceBoeDictionary"></param>
        void ValidateSaveManageBOE(Collection<ManageBOEModelView> boes, FullWorkspace ws, ICollection<PermissionsDTO> boePermissions, Dictionary<int, BOEState> BoeStateDictionary, Collection<ValidationMessage> ValidationErrors, IDictionary<int, FullBoe> workspaceBoeDictionary);

        /// <summary>
        /// Validates the BOE Level Custom Fields being saved for a BOE.
        /// </summary>
        /// <param name="ws">Workspace containing the BOE</param>
        /// <param name="boe">BOE containing the header</param>
        /// <param name="inBOEHeader">Modelview for the BOE Header</param>
        void ValidateBOEHeaderCustomFields(FullWorkspace ws, FullBoe boe, IBOEHeaderModelView inBOEHeader);

        /// <summary>
        /// Updates the workspace variables by removing multiboe references 
        /// </summary>
        /// <param name="ws">full workspace</param>
        /// <param name="workspaceVariablesAffectedByMulti">workspace vars that use boe</param>
        /// <param name="MultiBOEIDs">MultiBOE ID's</param>
        void RemoveMultiBOEReferenceWorkspaceVar(FullWorkspace ws, IList<WorkspaceVariableDTO> workspaceVariablesAffectedByMulti, Collection<int> MultiBOEIDs);

        /// <summary>
        /// Removes boes from a task variable that are now multiboe
        /// </summary>
        /// <param name="ws">full boe</param>
        /// <param name="multiBOEIDs">multi boe ids</param>
        /// <returns>returns a collection of modified taskelements with removed sumofboes that are multiboe</returns>
        Collection<BoeTaskElementDTO> RemoveMultiBOEReferenceTaskVar(FullWorkspace ws, ICollection<int> multiBOEIDs);

        /// <summary>
        /// Gets the Boe offload data.
        /// </summary>
        /// <param name="ws">The full workspace.</param>
        /// <param name="boeId">The boe identifier.</param>
        /// <returns>A Model View housing data for a Boe Offload</returns>
        BoeOffloadModelView RetrieveBoeOffloadData(FullWorkspace ws, int boeId);
        
        /// <summary>
        /// Copies the project map.
        /// </summary>
        /// <param name="ws">Workspace to be copied to.</param>
        /// <param name="copyProjectMapId">Id of Project Map to be copied.</param>
        void CopyProjectMap(FullProjectMapWorkspace ws, int copyProjectMapId);

        /// <summary>
        /// Saves the BOE states.
        /// </summary>
        /// <param name="boeStates">The BOE states.</param>
        /// <param name="ws">The ws.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>The updated date as a long</returns>
        long? SaveBOEStates(IDictionary<int, BOEState> boeStates, FullWorkspace ws, IList<string> errorMessages);

        /// <summary>
        /// Delete Moq Types For Boes
        /// </summary>
        /// <param name="ws">workspace</param>
        /// <param name="boeIds">BoeIds</param>
        void DeleteMoqTypesForBoe(FullWorkspace ws, ICollection<int> boeIds);

        /// <summary>
        /// Save Bulk Boe Roles
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="boeRolesToSave">Boe Roles to Save</param>
        /// <returns>Error messages, if any</returns>
        IList<string> SaveBoeBulkRoles(FullWorkspace ws, ICollection<ManageBOEModelView> boeRolesToSave);

        /// <summary>
        /// Gets all of the possible query operators.
        /// </summary>
        /// <returns>Collection of view model operators</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<ICollection<QueryOperatorViewModel>> GetAllOperators();

		/// <summary>
		/// Gets all of the possible query fields.
		/// </summary>
		/// <returns>Collection of view model fields</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		Task<ICollection<QueryFieldViewModel>> GetAllFields();
    }
}
