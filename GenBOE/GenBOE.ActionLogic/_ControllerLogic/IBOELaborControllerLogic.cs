// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Web.Mvc;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.ControllerLogic;
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
        /// Populates the passed in <see cref="LaborTaskModelView"/> with metric search dialog parameters.
        /// </summary>
        /// <param name="model">The <see cref="LaborTaskModelView"/> that will be populated.</param>
        void GetMetricSearchDialogParameters(LaborTaskModelView model);
        
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
        /// <param name="workspace">workspace</param>
        /// <param name="inTypeToGet">level of custom field to retrieve</param>
        /// <returns></returns>
        Collection<BOECustomFieldModelView> GetCustomFieldOptionModelViews(FullWorkspace workspace, ControllerCustomFieldType inTypeToGet);

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
        /// <param name="inWorkspaceDTO">workspace</param>
        void ValidateTaskDetails(FullBoe boeDTO, LaborTaskDataModelView laborTaskData, ICollection<ValidationMessage> inValidationErrors, FullWorkspace inWorkspaceDTO);

        /// <summary>
        /// Calculate Labor Spreads
        /// </summary>
        /// <param name="value">Spread Value</param>
        /// <param name="start">Start Date</param>
        /// <param name="end">End Date</param>
        /// <param name="curve">Spread curve ID</param>
        /// <param name="precision">decimal precision</param>
        /// <returns>Recalculated labor spreads</returns>
        ICollection<LaborSpreadDataModelView> CalculateLaborSpreads(decimal value, DateTime startDate, DateTime endDate, SpreadCurves curve, int precision);

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
        /// Gets the valid <see cref="MOQType"/>'s for this company configuration
        /// </summary>
        /// <returns>valid <see cref="MOQType"/>'s for this company configuration</returns>
        ICollection<MOQType> GetMOQTypes();

        /// <summary>
        /// Populates the passed in <see cref="MOQEquationModelView"/> with metrics
        /// </summary>
        /// <param name="ids">The TaskElement id's for which metrics will be retrieved</param>
        /// <param name="model">The <see cref="MOQEquationModelView"/> that will be populated</param>
        void GetMetricByTaskElementIds(Collection<int> ids, MOQEquationModelView model);

        /// <summary>
        /// Gets a <see cref="ViewResultData"/> with historic metrics.
        /// </summary>
        /// <param name="validatedOption">?</param>
        /// <param name="searchTerm">The criteria used to find metrics</param>
        /// <returns>The <see cref="ViewResultData"/> with historic metrics.</returns>
        ViewResultData GetHistoricalMetricsResults(int validatedOption, string searchTerm);

        /// <summary>
        /// Gets a <see cref="ViewResultData"/> with historic metrics from genBOE
        /// </summary>
        /// <param name="metricId">The id of the metric to retrieve</param>
        /// <returns>The <see cref="ViewResultData"/> with historic metrics</returns>
        ViewResultData GetHistoricalMetricsDetails(int metricId);

        /// <summary>
        /// Gets a <see cref="ViewResultData"/> with historic metrics from the source system
        /// </summary>
        /// <param name="metricId">The id of the metric to retrieve</param>
        /// <returns>The <see cref="ViewResultData"/> with historic metrics</returns>
        ViewResultData GetHistoricalMetricsDetailsFromSource(int metricId);

        /// <summary>
        /// Gets an <see cref="MOQEquationModelView"/> populated with historical or internal metrics
        /// </summary>
        /// <param name="taskElement">A <see cref="BoeTaskElementDTO"/> for which metrics will be retrieved</param>
        /// <returns>The <see cref="MOQEquationModelView"/> populated with historical or internal metrics</returns>
        MOQEquationModelView GetMOQModelView(BoeTaskElementDTO taskElement, FullWorkspace workspace);

        /// <summary>
        /// Sets the show metrics link.
        /// </summary>
        /// <returns>The <see cref="MOQEquationModelView"/> populated with the correct company specific value for ShowSearchMetricsLink.</returns>
        void SetShowMetricLink(MOQEquationModelView model);
        
        /// <summary>
        /// Saves metrics to GenBoe
        /// </summary>
        /// <param name="taskElementID">The id of the task element to save the metric to</param>
        /// <param name="metricIDs">the id of the metric to save to the task element</param>
        void SaveHistoricalMetricsToTaskElement(int taskElementID, ICollection<int> metricIDs);

        /// <summary>
        /// Gets historical metric type ahead terms matching the <paramref name="searchTerm"/>
        /// </summary>
        /// <param name="searchTerm">The criteria used to find metrics</param>
        /// <returns>The terms found</returns>
        ICollection<string> GetTypeAheadTerms(string searchTerm);

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
        /// Validate the Labor Task data prior to saving
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="modelView">Labor Task modelview</param>
        /// <returns>Any Validation errors</returns>
        ICollection<ValidationMessage> ValidateLaborTaskData(FullWorkspace ws, LaborTaskDataModelView modelView);

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
        /// <param name="metricIds">Metric IDs</param>
        /// <param name="answers">RTE Template Answers</param>
        /// <param name="moqTypes">MOQ Types for the task</param>
        void SaveLaborTaskData(FullWorkspace ws, BoeTaskElementDTO dtoToSave, ICollection<int> metricIds, ICollection<RTECustomTemplateQuestionAnswerModelView> answers, ICollection<MoqTypeSelection> moqTypes);

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
        /// <returns>Any Validation errors</returns>
        ICollection<ValidationMessage> ValidateTaskElementDto(FullWorkspace ws, BoeTaskElementDTO taskElement);

        /// <summary>
        /// Recalculates the labor types that have percent spread locked.
        /// </summary>
        /// <param name="workspaceData">The workspace data.</param>
        /// <param name="laborTabData">The labor tab data.</param>
        /// <param name="moqTotalHours">The moq total hours.</param>
        void RecalculateLaborSpreads(FullWorkspace workspaceData, RecalcSpreadModelView[] laborTabData, decimal moqTotalHours);

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
        /// <param name="wsUsingTemplateBOEs">Return only Template BOEs MOQ Types</param>
        /// <returns>MOQ Types</returns>
        ICollection<SelectListItem> GetMOQTypeSelectList(bool wsUsingTemplateBOEs);

        /// <summary>
        /// Returns a list of labels to be used in the MOQ Types page.
        /// </summary>
        /// <returns>Labels for MOQ Type Data Table Fields</returns>
        MoqTypeTableDataLabels GetMoqTypeLabels();
    }
}
