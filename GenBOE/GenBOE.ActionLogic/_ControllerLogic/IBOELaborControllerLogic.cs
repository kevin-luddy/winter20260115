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
    using System.Web.Mvc;
	using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.IESSAPClient;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;

    public interface IBOELaborControllerLogic
    {
        /// <summary>
        /// Calculate any linked task elements that need to be updated based on the currently being saved task element
        /// The most common example of this type of update is a task element that references the "being saved task element" in a task or workspace variable
        /// </summary>
        /// <param name="inValidationErrors">validation errors</param>
        /// <param name="inTaskElementsToSave">task elements to save</param>
        /// <param name="inOtherBoeTaskElementToSave">more task elements to save</param>
        /// <returns>Table mapping each recalculated task element ID to its hourly total</returns>
        IDictionary<int, decimal> CalculateLinkedTaskElements(Collection<ValidationMessage> inValidationErrors, ICollection<BoeTaskElementDTO> inTaskElementsToSave, Collection<BoeTaskElementDTO> inOtherBoeTaskElementToSave, FullWorkspace fullWorkspace);
        
        /// <summary>
        /// check if labor recalculation is needed for a boe to sum workspace/task variable based off of the boe
        /// </summary>
        /// <param name="inValidationErrors">validation errors</param>
        /// <param name="inOtherBoeTaskElementToSave">other task elements to save</param>
        /// <param name="inBoe">boe</param>
        /// <param name="inNextSetToCheck">next task elements to check</param>
        void CheckIfLaborRecalculationIsNeededBasedOnSumToBOEVariable(FullWorkspace fullWorkspace, Collection<ValidationMessage> inValidationErrors, Collection<BoeTaskElementDTO> inOtherBoeTaskElementToSave, FullBoe inBoe, Collection<BoeTaskElementDTO> inNextSetToCheck, Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, Collection<WorkspaceVariableDTO> incomingWorkspaceVariables = null, Dictionary<int, decimal> valueMapping = null);
		        
        /// <summary>
        /// Convert Collection of BoeTaskOrdinaryVariable to Collection of BoeTaskOrdinaryVariable
        /// This is used in the saving of a task element 
        /// </summary>
        /// <param name="inTaskVariableModelViews">task variable model views</param>
        /// <param name="inTaskElementID"> task element id</param>
        /// <param name="inBoeID">boe id </param>
        /// <param name="workspace">The Workspace.</param>
        /// <returns>task variables</returns>
        Collection<OrdinaryVariableDto> ConvertTaskVariableModelViewCollectionToTaskOrdinaryVariableCollection(Collection<BoeTaskOrdinaryVariableModelView> inTaskVariableModelViews, int inTaskElementID, int inBoeID, WorkspaceDTO workspace);

        /// <summary>
        /// Get custom field option model views
        /// </summary>
        /// <param name="ws">workspace</param>
        /// <param name="inTypeToGet">level of custom field to retrieve</param>
        /// <returns></returns>
        Collection<BOECustomFieldModelView> GetCustomFieldOptionModelViews(FullWorkspace ws, ControllerCustomFieldType inTypeToGet);

        ICollection<SpreadCurves> GetSpreadCurves(RateType rateType);
        
        /// <summary>
        /// Validate MOQ Equation to determine if a save task element can continue, then make sure it can be calculated correctly
        /// </summary>
        /// <param name="inBoeID">boe</param>
        /// <param name="laborTaskData">task element</param>
        /// <param name="inValidationErrors">validation errors</param>
        /// <param name="inWorkspaceDTO">workspace</param>
        /// <returns>The MOQ equation result, if valid</returns>
        string ValidateMOQEquation(int inBoeID, LaborTaskDataModelView laborTaskData, ICollection<ValidationMessage> inValidationErrors, FullWorkspace inWorkspaceDTO);

		/// <summary>
		/// This function will validate task start/end date, labor type level custom fields, task element level custom fields, task ID being unique with the BOE,
		/// and task variable unique name
		/// </summary>
		/// <param name="boeDTO">boe</param>
		/// <param name="laborTaskData">task modelview includes task details, labors, and spreads</param>
		/// <param name="inValidationErrors">validation errors</param>
		/// <param name="ws">workspace</param>		
		/// <param name="moqEquationTotal"> Moq equation total</param>
		void ValidateTaskDetails(FullBoe boeDTO, LaborTaskDataModelView laborTaskData, ICollection<ValidationMessage> inValidationErrors, FullWorkspace ws, decimal? moqEquationTotal = null);

		/// <summary>
		/// Calculate Labor Spreads
		/// </summary>
		/// <param name="value">Spread Value</param>
		/// <param name="startDate">Start Date</param>
		/// <param name="endDate">End Date</param>
		/// <param name="curve">Spread curve ID</param>
		/// <param name="precision">decimal precision</param>
		/// <param name="ucotSpreads">The Ucot Spreads</param>
		/// <param name="ucotFactor">UCOT factor</param>
		/// <param name="calculateUCOT">Whether to calculate UCOT</param>
		/// <returns>Recalculated labor spreads</returns>
		ICollection<LaborSpreadDataModelView> CalculateLaborSpreads(decimal value, DateTime startDate, DateTime endDate, SpreadCurves curve, int precision, bool calculateUCOT, decimal ucotFactor, out ICollection<LaborSpreadDataModelView> ucotSpreads);

		/// <summary>
		/// Returns the correct MOQ help text per company configuration
		/// </summary>
		/// <returns>per company configuration MOQ help text</returns>
		string GetMOQTypesHelpText();

        /// <summary>
        /// Returns the correct MOQ Equation label text per company configuration
        /// </summary>
        /// <returns>per company configuration MOQ Equation label text</returns>
        string GetMOQEquationLabel();

        /// <summary>
        /// Returns the correct MOQ text label per company configuration
        /// </summary>
        /// <returns>per company configuration MOQ text label text</returns>
        string GetMOQTextLabel();

        /// <summary>
        /// Gets an <see cref="MOQEquationModelView"/> 
        /// </summary>
        /// <param name="taskElement">A <see cref="BoeTaskElementDTO"/></param>
        /// <returns>The <see cref="MOQEquationModelView"/></returns>
        MOQEquationModelView GetMOQModelView(BoeTaskElementDTO taskElement, FullWorkspace workspace);

        /// <summary>
        /// Returns a <see cref="bool"/> indicating if the read only flag should be overridden
        /// </summary>
        /// <param name="ws">the <see cref="FullWorkspace"/> being viewed</param>
        /// <param name="boe">the <see cref="FullWorkspace"/> being viewed</param>
        /// <returns>true if read only should be overridden, false otherwise</returns>
        bool OverrideReadOnly(FullWorkspace ws, FullBoe boe);

        /// <summary>
        /// Determine the full dependency-chain of task elements and task variables which are based on a BOE
        /// </summary>
        /// <param name="boeID">BOE PK identifier</param>
        /// <param name="workspace">Workspace</param>
        /// <returns>Bottom-up (breadth-first) dependency-chains</returns>
        VariableDependencyResolutionData ResolveAllVariableDependencies(int boeID, FullWorkspace workspace);

        /// <summary>
        /// Identify all task and workspace variables that reference the designated BOE.  Recalculate the values of each such variable as well as the
        /// hourly totals (and spread distributions) for any task elements that USE them.  Continue (recursively) resolving references to any
        /// NESTED variables until the process is complete.
        /// </summary>
        /// <param name="boeID">BOE PK identifier</param>
        /// <param name="ws">Workspace</param>
        void ProcessAllVariableDependencies(int boeID, FullWorkspace ws);

        /// <summary>
        /// Recursive worker method.
        /// </summary>
        /// <param name="boeID">BOE PK identifier</param>
        /// <param name="ws">Workspace</param>
        /// <param name="dependencies">Task variable dependency data</param>
        void ProcessAllVariableDependencies(int boeID, FullWorkspace ws, VariableDependencyResolutionData dependencies);

        /// <summary>
        /// Gets the perf orgs for a workspace and returns the model to the front end.
        /// </summary>
        /// <param name="ws">full ws.</param>
        /// <returns>Collection of perf orgs.</returns>
        Collection<PerformingOrgModelView> GetPerformingOrgs(FullWorkspace ws);

        /// <summary>
        /// Get the Labor Task Data 
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="boe">BOE</param>
        /// <param name="taskElementId">Task Element ID</param>
        /// <returns>Labor Task Data</returns>
        LaborTaskDataModelView GetLaborTaskData(FullWorkspace ws, FullBoe boe, int taskElementId);

		/// <summary>
		/// Converts Boe Task Element Dto to Labor Task Data Model View
		/// </summary>
		/// <param name="ws">The Workspace.</param>
		/// <param name="boe">The Boe</param>
		/// <param name="dto">Task Element DTO</param>
		/// <returns>Converted MV</returns>
		LaborTaskDataModelView ConvertDtoToModelView(FullWorkspace ws, FullBoe boe, BoeTaskElementDTO dto);

		/// <summary>
		/// Validate the Labor Task data prior to saving
		/// </summary>
		/// <param name="ws">Workspace</param>
		/// <param name="modelView">Labor Task modelview</param>
		/// <returns>Any Validation errors</returns>
		ICollection<ValidationMessage> ValidateLaborTaskDataWithDataModification(FullWorkspace ws, LaborTaskDataModelView modelView);

        /// <summary>
        /// Validate a labor task for saving in a locked Workspace
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="modelView">Labor Task ModelView</param>
        /// <returns>Any validation errors</returns>
        ICollection<ValidationMessage> ValidateLockedLaborTaskData(FullWorkspace ws, LaborTaskDataModelView modelView);

        /// <summary>
        /// Save the Labor Task data
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="dtoToSave">Task DTO</param>
        /// <param name="answers">RTE Template Answers</param>
        /// <param name="moqTypes">MOQ Types for the task</param>
        void SaveLaborTaskData(FullWorkspace ws, BoeTaskElementDTO dtoToSave, ICollection<RTECustomTemplateQuestionAnswerModelView> answers, ICollection<MoqTypeSelection> moqTypes);

        /// <summary>
        /// Converts Labor Task Data Model View to BOE Task Element DTO 
        /// </summary>
        /// <param name="modelview">Labor Task Data Model View  to convert</param>
        /// <param name="ws">The Workspace.</param>
        /// <returns>Converted BOE Task Element DTO</returns>
        BoeTaskElementDTO ConvertModelViewToDto(LaborTaskDataModelView modelview, FullWorkspace ws);

		/// <summary>
		/// Validate Task Element DTO before saving
		/// </summary>
		/// <param name="ws">workspace</param>
		/// <param name="taskElement">task element</param>
		/// <param name="modelView">labor task</param>
		/// <returns>Any Validation errors</returns>
		ICollection<ValidationMessage> ValidateTaskElementDto(FullWorkspace ws, BoeTaskElementDTO taskElement, LaborTaskDataModelView modelView);

		/// <summary>
		/// Recalculates the labor types that have percent spread locked.
		/// </summary>
		/// <param name="workspaceData">The workspace data.</param>
		/// <param name="laborTabData">The labor tab data.</param>
		/// <param name="moqTotalHours">The moq total hours.</param>
		/// <param name="calculateUCOT">Whether to calculate UCOT</param>
		void RecalculateLaborSpreads(FullWorkspace workspaceData, RecalcSpreadModelView[] laborTabData, decimal moqTotalHours, bool calculateUCOT);

		/// <summary>
		/// Ralculates the discrete UCOT spread.
		/// </summary>
		/// <param name="workspaceData">The workspace data.</param>
		/// <param name="ucotSpreadItem">The labor tab data.</param>
		/// <returns></returns>
		RecalcSpreadModelView RecalculateDiscreteUCOTSpreads(FullWorkspace workspaceData, RecalcSpreadModelView ucotSpreadItem);

		/// <summary>
		/// Updates the OrderList for the Labor Types in a Task Element
		/// </summary>
		/// <param name="ws">The Workspace</param>
		/// <param name="taskElement">Task Element containing the Labor Types</param>
		/// <param name="modelView">Collection of the Labor Type Order</param>
		void ReOrderLaborTypeOrder(FullWorkspace ws, BoeTaskElementDTO taskElement, LaborTypeOrderCollection modelView);

        /// <summary>
        /// Get a list of MOQ Types for dropdown
        /// </summary>
        /// <param name="useNewMoqTypes">Are we be using new MOQ Types (after 10/2020)</param>
        /// <param name="moqType">Selected MOQ Type</param>
        /// <returns>MOQ Types</returns>
        ICollection<SelectListItem> GetMOQTypeSelectList(bool useNewMoqTypes, MOQType? moqType);

        /// <summary>
        /// Returns a list of labels to be used in the MOQ Types page.
        /// </summary>
        /// <returns>Labels for MOQ Type Data Table Fields</returns>
        MoqTypeTableDataLabels GetMoqTypeLabels();

        /// <summary>
        /// Returns help URLs for MOQ Type fields
        /// </summary>
        /// <returns>help URLs for MOQ Type fields</returns>
        MoqTypeHelpUrls GetMoqTypeHelpUrls();

        /// <summary>
        /// Export MOQ Tables
        /// </summary>
        /// <param name="moqTypeId">MOQ Type ID</param>
        /// <param name="ws">Workspace</param>
        /// <param name="templateFileLocation">Template file location</param>
        /// <returns>file name for the export</returns>
        string ExportMoqTables(int moqTypeId, FullWorkspace ws, string templateFileLocation);

		/// <summary>
		/// Import MOQ Tables
		/// </summary>
		/// <param name="ws">Workspace</param>
		/// <param name="request">http request containing import file</param>
		/// <returns>Imported MOQ Table modelviews</returns>
		Task<ImportMoqTableResultDataModelView> ImportMoqTables(FullWorkspace ws, HttpRequestBase request);

        /// <summary>
        /// Complete the MOQ Table import
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="importResults">MOQ Table import results</param>
        /// <param name="moqTypeId">MOQ Type Id</param>
        void CompleteImportMoqTables(ICollection<ImportMoqTableResultsModelView> importResults, int moqTypeId);

        /// <summary>
        /// Converts a List of SAP Filters into text
        /// </summary>
        /// <param name="filters">The list of Filters</param>
        /// <returns>Textual representation of the filters</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<IESResponse<string>> ConvertSapFilter(ICollection<QueryViewModel> filters);

        /// <summary>
        /// Parses text for a list of SAP Filters
        /// </summary>
        /// <param name="text">The text to parse</param>
        /// <returns>List of SAP Filters</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<IESResponse<QueryViewModel>> ParseSapFilter(string text);

        /// <summary>
        /// Export Actuals data for SAP
        /// </summary>
        /// <param name="tableData">The MOQ Table Data</param>
        /// <returns>Validation Response with file as byte array</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<IESResponse<byte>> ExportActualsSap(MoqTableDataModelView tableData);        

        /// <summary>
        /// Validates Actuals data for SAP
        /// </summary>
        /// <param name="tableData">The MOQ Table Data</param>
        /// <returns>Validation Response</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<ICollection<IESResponse<CalculateActualsViewModel>>> CalculateAllActualsSap(ICollection<MoqTableDataModelView> tableData);

		/// <summary>
        /// Validates Actuals data for SAP
        /// </summary>
        /// <param name="tableData">The MOQ Table Data</param>
        /// <returns>Validation Response</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<ICollection<IESResponse<CalculateActualsWithSkillMixViewModel>>> CalculateAllActualsSapWithSkillMix(ICollection<MoqTableDataModelView> tableData);

        /// <summary>
		/// Get Resources converted to Business Resource Code List
		/// </summary>
		/// <returns>List of Resources and their respective Business Resource Codes</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<IESResponse<SkillMixConvertedResourceViewModel>> GetSkillMixConvertedResources();

		/// <summary>
		/// Checks the usage of active T&M rates in the task.
		/// </summary>
		/// <param name="ws">Workspace.</param>
		/// <param name="laborTask">Labor Task.</param>
		/// <returns></returns>
		bool CheckTMRates(FullWorkspace ws, ICollection<LaborTypeDataModelView> laborTypes);
	}
}
