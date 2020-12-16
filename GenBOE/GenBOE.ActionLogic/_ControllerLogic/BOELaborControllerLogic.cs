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
    using System.Linq;
    using System.Transactions;
    using System.Web.Configuration;
    using System.Web.Mvc;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.MOQ;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using Microsoft.Practices.ObjectBuilder2;

    /// <summary>
    /// Action Logic for the BOE Labor Controller
    /// </summary>
    public class BOELaborControllerLogic : IBOELaborControllerLogic
    {
        // This code should be refactored after the ViewResult as a return to some methods are removed when the metric model views for IS&GS and Space Systems are refactored to use an interface
        // WI28181
        private readonly Common.Calculations.BoeTaskElementRecalculation _BoeTaskElementRecalculation;
        private readonly IBOEStateMachine _boeStateMachine;
        private readonly IBoeMediator _BoeMediator;
        private readonly IBoeTaskElementMediator _BoeTaskElementMediator;
        private readonly IResourceDTODataLoader _ResourceLoader;
        private readonly IFullObjectFactory factory;
        private readonly IWorkspaceVariableDTODataLoader _WorkspaceVariableDTODataLoader;
        private readonly IBoeDTODataLoader _BoeLoader;
        private readonly IPerformingOrgDTODataLoader PerfOrgLoader;
        private readonly IOrdinaryVariableLoader _taskVariableLoader;
        private readonly IRteTemplateDataLoader rteTemplateDataLoader;
        private readonly IMoqTypeDataLoader moqTypeDataLoader;

        /// <summary>
        /// Task Element Validation Class
        /// </summary>
        private readonly TaskElementValidation taskElementValidation;
        private readonly IVariableCircularReferenceChecker circularReferenceChecker;
        private readonly Logger logger = new Logger(typeof(BOELaborControllerLogic));
        protected ICommonDataMapper CommonDataMapper { get; }

        public BOELaborControllerLogic(
            Common.Calculations.BoeTaskElementRecalculation inBoeTaskElementRecalc,
            IBOEStateMachine inBoeStateMachine,
            IBoeMediator inBoeMediator,
            IBoeTaskElementMediator inBoeTaskElementMediator,
            Common.Calculations.IVariableSelectBOEtoSumCalculation inVarSelectBoeToSumCalc,
            IResourceDTODataLoader inResourceLoader,
            IFullObjectFactory factory,
            IWorkspaceVariableDTODataLoader inWorkspaceVariableDTODataLoader,
            IBoeDTODataLoader inBoeLoader,
            IBoeTaskElementDTODataLoader inTaskElementDataLoader,
            IUserDTODataLoader inuserLoader,
            IPermissionsDTODataLoader inPermissionsLoader,
            IPerformingOrgDTODataLoader PerfOrgLoader,
            IOrdinaryVariableLoader inTaskVariableLoader,
            TaskElementValidation taskElementValidation,
            IVariableCircularReferenceChecker circularReferenceChecker,
            ICommonDataMapper commonDataMapper,
            IRteTemplateDataLoader rteTemplateDataLoader,
            IMoqTypeDataLoader moqTypeDataLoader)
        {
            this._BoeTaskElementRecalculation = inBoeTaskElementRecalc;
            this._boeStateMachine = inBoeStateMachine;
            this._BoeMediator = inBoeMediator;
            this._BoeTaskElementMediator = inBoeTaskElementMediator;
            this.VariableSelectBOEtoSumCalculation = inVarSelectBoeToSumCalc;
            this._ResourceLoader = inResourceLoader;
            this.factory = factory;
            this._WorkspaceVariableDTODataLoader = inWorkspaceVariableDTODataLoader;
            this._BoeLoader = inBoeLoader;
            this.BoeTaskElementDTODataLoader = inTaskElementDataLoader;
            this.UserLoader = inuserLoader;
            this.PermissionsLoader = inPermissionsLoader;
            this.PerfOrgLoader = PerfOrgLoader;
            this._taskVariableLoader = inTaskVariableLoader;
            this.taskElementValidation = taskElementValidation;
            this.circularReferenceChecker = circularReferenceChecker;
            this.CommonDataMapper = commonDataMapper;
            this.rteTemplateDataLoader = rteTemplateDataLoader;
            this.moqTypeDataLoader = moqTypeDataLoader;
        }

        #region Public Members

        /// <summary>
        /// Finds the adjacent tasks for a task.
        /// </summary>
        /// <param name="boe">The boe to search.</param>
        /// <param name="taskId">The current task Id</param>
        /// <returns></returns>
        internal AdjacentItems FindAdjacentTasks(FullBoe boe, int taskId)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            int[] orderedTaskIds = boe.TaskElements.OrderBy(t => t.BOETaskElementOrder).Select(t => t.Id).ToArray();
            AdjacentItems adjacentItems = new AdjacentItems();
            int? previousId = null;

            for(int i = 0; i < orderedTaskIds.Length; i++)
            {
                int currentId = orderedTaskIds[i];

                if (currentId == taskId)
                {
                    adjacentItems.PreviousId = previousId;

                    if (orderedTaskIds.Length > i + 1)
                    {
                        adjacentItems.NextId = orderedTaskIds[i + 1];
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
        /// Recalculates the labor types that have percent spread locked.
        /// </summary>
        /// <param name="workspaceData">The workspace data.</param>
        /// <param name="laborTabData">The labor tab data.</param>
        /// <param name="moqTotalHours">The moq total hours.</param>
        public void RecalculateLaborSpreads(FullWorkspace workspaceData, RecalcSpreadModelView[] laborTabData, decimal moqTotalHours)
        {
            if (workspaceData == null)
            {
                throw new ArgumentNullException(nameof(workspaceData));
            }

            if (laborTabData == null)
            {
                throw new ArgumentNullException(nameof(laborTabData));
            }

            #region First pass: Initial allocation of hours to each resource entry

            decimal overallHoursSpread = 0m;

            ICollection<RecalcSpreadModelView> validResourceEntries = laborTabData.Where(l => l.IsValid()).ToList();
            foreach (RecalcSpreadModelView resourceTypeData in validResourceEntries)
            {
                SpreadCurves? spreadCurve = resourceTypeData.curve;

                decimal hourSpread = resourceTypeData.value ?? 0;

                if (resourceTypeData.rateType == RateType.Cost)
                {
                    // percent, hours and cost spread values stay the same for cost spreads
                }
                else if (spreadCurve.HasValue && spreadCurve.Value == SpreadCurves.DiscreteHours)
                {
                    // hours and cost spread values stay the same for discrete hours spreads, but percent changes
                    resourceTypeData.percentSpread = (moqTotalHours == 0) ? 0m : decimal.Round(Convert.ToDecimal((100m * hourSpread) / moqTotalHours), 3, MidpointRounding.AwayFromZero);
                }
                else if (resourceTypeData.percentLocked.Value)
                {
                    decimal percentSpread = resourceTypeData.percentSpread.Value;

                    // assign an integer value for both decimal variables
                    decimal hoursSpreadValue = (percentSpread == 0) ? 0 : Utilities.AdjustPrecision((moqTotalHours * percentSpread / 100), workspaceData.DecimalPrecision);
                    resourceTypeData.value = hoursSpreadValue;
                    hourSpread = hoursSpreadValue;
                }
                else
                {
                    resourceTypeData.percentSpread = (moqTotalHours == 0) ? 0m : decimal.Round(Convert.ToDecimal((100m * hourSpread) / moqTotalHours), 3, MidpointRounding.AwayFromZero);
                }

                overallHoursSpread += hourSpread;
            }

            #endregion

            #region Second pass: Apply any delta adjustments

            // apply adjustments BEFORE the spread values are recalculated

            decimal deltaHours = Utilities.AdjustPrecision(overallHoursSpread, workspaceData.DecimalPrecision) - moqTotalHours;

            ICollection<RecalcSpreadModelView> adjustments = this.AdjustDeltaHours(validResourceEntries, deltaHours, workspaceData);
            if (adjustments.Any())
            {
                deltaHours = 0;
            }

            #endregion

            #region Third pass: Recalculate hour spread values

            foreach (RecalcSpreadModelView resourceTypeData in laborTabData)
            {
                if (resourceTypeData.IsValid())
                {
                    int precision = (resourceTypeData.rateType == RateType.Cost) ? workspaceData.CostDecimalPrecision : workspaceData.DecimalPrecision;

                    decimal hourSpreadValue = resourceTypeData.value.HasValue ? Utilities.AdjustPrecision(resourceTypeData.value.Value, precision) : 0;
                    resourceTypeData.spreads = this.CalculateLaborSpreads(hourSpreadValue, resourceTypeData.start.Value, resourceTypeData.end.Value, resourceTypeData.curve.Value, precision);
                }
                else
                {
                    resourceTypeData.spreads = new List<LaborSpreadDataModelView>();
                }
            }

            #endregion
        }

        /// <summary>
        /// In cases where the total hours DOES NOT divide evenly among the number of percent-locked auto-calculated (non-discrete)
        /// resource entries, we need to adjust (+/-1) the hour spread values for a subset of those entries so that the overall
        /// delta is corrected to zero.
        /// </summary>
        /// <param name="laborTypesData">Resource types data</param>
        /// <param name="deltaHours">Current delta</param>
        /// <returns>The resource entries whose hours were adjusted</returns>
        private ICollection<RecalcSpreadModelView> AdjustDeltaHours(ICollection<RecalcSpreadModelView> laborTypesData, decimal deltaHours, WorkspaceDTO ws)
        {
            IList<RecalcSpreadModelView> adjustedResourceEntries = new List<RecalcSpreadModelView>();

            if (deltaHours != 0)
            {
                // only adjust labor types that aren't discrete and percent spread locked is locked
                ICollection<RecalcSpreadModelView> eligibleResourceEntries = laborTypesData.Where(r => r.IsValid() && r.curve.Value != SpreadCurves.DiscreteHours && r.rateType != RateType.Cost && r.percentLocked.Value).ToList();

                decimal totalSpreadPercentage = eligibleResourceEntries.Sum(r => r.percentSpread.HasValue ? r.percentSpread.Value : 0);
                if (totalSpreadPercentage == 100)
                {
                    decimal adjustment = (deltaHours < 0) ? Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(ws.DecimalPrecision) : -1 * Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(ws.DecimalPrecision);  // adjust either up or down

                    while (deltaHours != 0)
                    {
                        foreach (RecalcSpreadModelView resourceEntry in eligibleResourceEntries)
                        {
                            if (deltaHours == 0)
                            {
                                break;
                            }

                            // apply adjustment to the current entry
                            decimal resourceHours = resourceEntry.value.HasValue ? Utilities.AdjustPrecision(resourceEntry.value.Value, ws.DecimalPrecision) : 0;
                            resourceEntry.value = resourceHours + adjustment;
                            adjustedResourceEntries.Add(resourceEntry);

                            // update the delta
                            deltaHours += adjustment;
                        }
                    }
                }
            }

            return adjustedResourceEntries;
        }

        /// <summary>
        /// Get the Labor Task Data 
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="boe">BOE</param>
        /// <param name="taskElementId">Task Element ID</param>
        /// <returns>Labor Task Data</returns>
        public LaborTaskDataModelView GetLaborTaskData(FullWorkspace ws, FullBoe boe, int taskElementId)
        {
            if (ReferenceEquals(boe, null))
            {
                throw new ArgumentNullException(nameof(boe));
            }

            if (ReferenceEquals(ws, null))
            {
                throw new ArgumentNullException(nameof(ws));
            }

            // Get DTO from the db
            BoeTaskElementDTO taskElementDto = this.factory.CreateTaskElement(taskElementId, ws.DecimalPrecision, ws.CostDecimalPrecision);

            if (taskElementDto == null && taskElementId <= 0)
            {
                taskElementDto = new BoeTaskElementDTO
                {
                    BoeID = boe.Id,
                    StartDate = boe.StartDate,
                    EndDate = boe.EndDate
                };
            }

            // Convert to ModelView
            LaborTaskDataModelView toReturn = this.ConvertDtoToModelView(ws, boe, taskElementDto);
            toReturn.AdjacentItems = this.FindAdjacentTasks(boe, taskElementId);
            toReturn.ValidationErrors = this.taskElementValidation.ValidateTaskElementsWithErrorMessages(ws, new List<BoeTaskElementDTO>() { taskElementDto }).Select(e => e.ErrorMessage).ToList();
            
            return toReturn;
        }

        /// <summary>
        /// Gets the perf orgs for a workspace and returns the model to the front end.
        /// </summary>
        /// <param name="ws">full ws.</param>
        /// <returns>Collection of perf orgs.</returns>
        public Collection<PerformingOrgModelView> GetPerformingOrgs(FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            return ws.PerformingOrgsForWsList.Select(org => new PerformingOrgModelView(org)).ToCollection();
        }

        /// <summary>
        /// Validate the Labor Task data prior to saving
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="modelView">Labor Task modelview</param>
        /// <returns>Any Validation errors</returns>
        public ICollection<ValidationMessage> ValidateLaborTaskData(FullWorkspace ws, LaborTaskDataModelView modelView)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            
            ICollection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();

            if (modelView != null)
            {
                FullBoe boe = factory.CreateFullBoe((int)modelView.TaskElementData.BOEID);

                // Validation - Phase I - Ignore deleted rows
                validationErrors = ValidateLaborTaskDataLabor(ws, boe, modelView);

                #region Remove rows that were added then deleted

                ICollection<LaborTypeDataModelView> addedThenDeletedResourceTypes = modelView.LaborTypesData.Where(r => r.Deleted && r.BOELaborTypeID.HasValue && r.BOELaborTypeID.Value < 0).ToCollection();
                if (addedThenDeletedResourceTypes.Any())
                {
                    foreach (LaborTypeDataModelView resourceType in addedThenDeletedResourceTypes)
                    {
                        modelView.LaborTypesData.Remove(resourceType);
                    }
                }

                #endregion

                #region Validation - Phase II

                // validate task details composite
                this.ValidateTaskDetails(boe, modelView, validationErrors, ws);

                bool addNullValidationError = true;

                if (modelView.TaskElementData.TaskOrdinaryVariables.Any())
                {
                    foreach (BoeTaskOrdinaryVariableModelView variable in modelView.TaskElementData.TaskOrdinaryVariables)
                    {
                        if (addNullValidationError && variable.OrdinaryVariableValue == null)
                        {
                            validationErrors.Add(new ValidationMessage("TaskVariable", "All MOQ equation variable fields must contain values. Please fill in all of the variable fields below."));
                            addNullValidationError = false;
                        }
                    }
                }

                // validate moq equation
                string result = this.ValidateMOQEquation(boe.Id, modelView, validationErrors, ws);

                #region Validate that MOQ Equation Value = Labor Hour Spreads = Resource Hours Spreads

                    decimal? moqResult = 0;

                    decimal tempMoqResult;

                    if (!string.IsNullOrEmpty(result))
                    {
                        if (decimal.TryParse(result, out tempMoqResult))
                        {
                            moqResult = Utilities.AdjustPrecision(tempMoqResult, ws.DecimalPrecision);
                        }
                    }

                    // Need to find any missing labors stored in DB that are not handled by UI
                    if (modelView.TaskElementData.TaskElementDetailID > 0)
                    {
                        BoeTaskElementDTO element = ws.TaskElements.FirstOrDefault(a => a.Id == modelView.TaskElementData.TaskElementDetailID);
                        if (element != null)
                        {
                            List<int> idsOfLaborsBeingEdited = modelView.LaborTypesData.Where(a => a.BOELaborTypeID.HasValue).Select(a => a.BOELaborTypeID.Value).ToList();

                            // Find missing labors stored in the DB that are not being saved by the UI
                            ICollection<ResourceTypeDto> missingLabors = element.taskElementLabors.Where(a => !idsOfLaborsBeingEdited.Contains(a.Id)).ToList();

                            // these missing labors need to be removed
                            foreach (ResourceTypeDto missing in missingLabors)
                            {
                                this.logger.Error("During Save of Task Element, there was a missing task element labor found in the DB that will be deleted with id " + missing.Id);
                                LaborTypeDataModelView toDelete = new LaborTypeDataModelView(missing, new ResourceDTO(), new PerformingOrgDTO());
                                toDelete.Deleted = true;
                                modelView.LaborTypesData.Add(toDelete);
                            }
                        }
                    }

                    if (modelView.LaborTypesData.Any())
                    {
                        Collection<LaborTypeDataModelView> laborTypes = (from lt in modelView.LaborTypesData
                                                                         where lt.Deleted == false && lt.RateType == RateType.Hours
                                                                         select lt).ToCollection();

                        // should only validate if there are any labor types
                        if (laborTypes.Any())
                        {
                            decimal? laborTypesHours = laborTypes.Sum(a => a.HourSpread ?? 0);

                            // only consider spreads for which spread type = Hours
                            decimal? spreadsHours = laborTypes.Sum(a => a.Spreads.Sum(b => b.LaborSpreadValue));

                            if (moqResult != laborTypesHours || laborTypesHours != spreadsHours)
                            {
                                string hoursLabel = FullObjectHelper.HoursLabel(ws);
                                string errorMsg = "The total Resource Type <b>" + hoursLabel + " Spread</b> must equal the Total Resource Spread which must also equal the total " + hoursLabel + " computed by the <b>" + this.GetMOQEquationLabel() + "</b>."
                                                + "<br />    · If the <b>Total</b> Resource Spread is incorrect for a resource: change the <b>Spread Curve</b> so it recalculates. Then change it back to the original <b>Spread Curve</b>. "
                                            + "<br />    · Verify the " + this.GetMOQEquationLabel() + " Total = Total Resource " + hoursLabel + " = Total Resource Spread and the Delta = 0.";

                                validationErrors.Add(new ValidationMessage("MOQ-Labor", errorMsg));
                            }
                        }
                    }

                    #endregion

                #endregion

            }
                
            #region Validate Precision

            if (modelView.LaborTypesData.Any())
            {
                foreach(LaborTypeDataModelView labor in modelView.LaborTypesData)
                {
                    if (labor.RateType == RateType.Cost)
                    {
                        CheckPrecision(labor.CostSpread, ws.CostDecimalPrecision, "Resource Types Row - Cost");
                        // only checking discrete since non-discrete is not saved in Database
                        if (labor.SpreadCurveID.HasValue && labor.SpreadCurveID == SpreadCurves.DiscreteCost)
                        {
                            if (labor.Spreads.Any())
                            {
                                foreach(LaborSpreadDataModelView spread in labor.Spreads)
                                {
                                    CheckPrecision(spread.LaborSpreadValue, ws.CostDecimalPrecision, "Resource Spreads");
                                }
                            }
                        }
                    } 
                    else
                    {
                        CheckPrecision(labor.HourSpread, ws.DecimalPrecision, "Resource Types Row - Hours Spread");
                        // only checking discrete since non-discrete is not saved in Database
                        if (labor.SpreadCurveID.HasValue && labor.SpreadCurveID == SpreadCurves.DiscreteHours)
                        {
                            if (labor.Spreads.Any())
                            {
                                foreach (LaborSpreadDataModelView spread in labor.Spreads)
                                {
                                    CheckPrecision(spread.LaborSpreadValue, ws.DecimalPrecision, "Resource Spreads");
                                }
                            }
                        }
                    }
                }
            }

            #endregion Validate Precision

            return validationErrors;
        }

        /// <summary>
        /// Validate a labor task for saving in a locked Workspace
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="modelView">Labor Task ModelView</param>
        /// <returns>Any validation errors</returns>
        public ICollection<ValidationMessage> ValidateLockedLaborTaskData(FullWorkspace ws, LaborTaskDataModelView modelView)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            ICollection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();

            BoeTaskElementDTO originalTask = ws.Boes.FirstOrDefault(x => x.Id == modelView.TaskElementData.BOEID).TaskElements.FirstOrDefault(x => x.Id == modelView.TaskElementData.TaskElementDetailID);

            IReadOnlyCollection<WorkspaceVariableDTO> allWorkspaceVariables = ws.Boes.First(x => x.Id == originalTask.BoeID).WorkspaceVariables;
            
            // Validate no change to Task ID
            if((string.IsNullOrEmpty(modelView.TaskElementData.TaskID) && !string.IsNullOrEmpty(originalTask.BOETaskID))
                || (!string.IsNullOrEmpty(modelView.TaskElementData.TaskID) && modelView.TaskElementData.TaskID != originalTask.BOETaskID))
            {
                validationErrors.Add(new ValidationMessage(string.Format(Constants.CANNOT_CHANGE_IN_LOCKED_TASK, "Task ID")));
            }

            // Validate no change to Task Title
            if (modelView.TaskElementData.Title != originalTask.TaskTitle)
            {
                validationErrors.Add(new ValidationMessage(string.Format(Constants.CANNOT_CHANGE_IN_LOCKED_TASK, "Task Title")));
            }

            // Validate no change to Dates
            if (modelView.TaskElementData.StartDate != originalTask.StartDate.Value.ToMonthString() 
                || modelView.TaskElementData.EndDate != originalTask.EndDate.Value.ToMonthString())
            {
                validationErrors.Add(new ValidationMessage(string.Format(Constants.CANNOT_CHANGE_IN_LOCKED_TASK, "Task Start and End Dates")));
            }

            // Validate no change to MOQ Equation
            // Untag variables before comparing
            List<WorkspaceVariableDTO> inUseWorkspaceVariables = (from wID in originalTask.WorkspaceVariableIDs
                                                                  from workspaceVariable in allWorkspaceVariables
                                                                  where workspaceVariable.Id == wID
                                                                  select workspaceVariable).ToList();

            originalTask.MOQHoursEquation = Common.MOQ.Parser.UntagVariables(originalTask.MOQHoursEquation, inUseWorkspaceVariables);

            if ((modelView.TaskElementData.MOQHoursEquation == "0" && originalTask.MOQHoursEquation != "0" && !string.IsNullOrEmpty(originalTask.MOQHoursEquation)) 
                || (!string.IsNullOrEmpty(originalTask.MOQHoursEquation) && modelView.TaskElementData.MOQHoursEquation != originalTask.MOQHoursEquation))
            {
                validationErrors.Add(new ValidationMessage(string.Format(Constants.CANNOT_CHANGE_IN_LOCKED_TASK, "MOQ Equation")));
            }
            
            // Validate no change to Labors
            bool errorFound = false;
            foreach (ResourceTypeDto originalLabor in originalTask.taskElementLabors)
            {
                LaborTypeDataModelView labor = modelView.LaborTypesData.FirstOrDefault(x => x.BOELaborTypeID == originalLabor.Id);

                if (labor == null)
                {
                    validationErrors.Add(new ValidationMessage("Resource Types" + Constants.CANNOT_CHANGE_IN_LOCKED_TASK));
                    break;
                }

                if (labor.ResourceID != originalLabor.ResourceID
                    || labor.PerformingOrgID != originalLabor.PerformingOrgID
                    || labor.StartDate != originalLabor.StartDate.Value.ToMonthString()
                    || labor.EndDate != originalLabor.EndDate.Value.ToMonthString()
                    || labor.SpreadCurveID != originalLabor.SpreadCurveID
                    || labor.PercentSpread != originalLabor.PercentSpread
                    || (originalLabor.SpreadType == SpreadType.Hours && labor.HourSpread != originalLabor.ValueSpread)
                    || (originalLabor.SpreadType == SpreadType.Cost && labor.CostSpread != originalLabor.ValueSpread))
                {
                    validationErrors.Add(new ValidationMessage(string.Format(Constants.CANNOT_CHANGE_IN_LOCKED_TASK, "Resource Types")));
                    errorFound = true;
                    break;
                }

                // Validate no change to Spreads
                for (int i = 0; i < originalLabor.LaborSpreads.Count(); i++)
                {
                    if(labor.Spreads.ElementAt(i).LaborSpreadValue != originalLabor.LaborSpreads.ElementAt(i).LaborSpreadValue
                        || labor.Spreads.ElementAt(i).LaborSpreadDate != originalLabor.LaborSpreads.ElementAt(i).LaborSpreadDate.ToMonthString())
                    {
                        validationErrors.Add(new ValidationMessage(string.Format(Constants.CANNOT_CHANGE_IN_LOCKED_TASK, "Resource Spreads")));
                        errorFound = true;
                        break;
                    }
                }

                if (errorFound)
                {
                    break;
                }
            }

            this.ValidateTaskElementRteSizeLimit(modelView, ws, validationErrors);

            return validationErrors;
        }

        /// <summary>
        /// Validate the Labor of the Labor Task Data
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="modelView">Modelview</param>
        /// <returns>Any validation errors found</returns>
        private ICollection<ValidationMessage> ValidateLaborTaskDataLabor(FullWorkspace ws, FullBoe boe, LaborTaskDataModelView modelView)
        {
            ICollection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();

            int laborTypeIndex = -1;
            bool isUsingEP = FullObjectHelper.ShowEquivalentPersonsOption && ws.IsUsingEquivalentPerson;

            foreach (LaborTypeDataModelView labor in modelView.LaborTypesData)
            {
                string fieldPrefix = string.Format("{0}{1}{2}", "LaborTypes[", ++laborTypeIndex, "].");

                if (!labor.Deleted)
                {
                    if (!labor.ResourceID.HasValue || labor.ResourceID < 1)
                    {
                        validationErrors.Add(new ValidationMessage(fieldPrefix + "ResourceID", "Resource is required or is not valid."));
                    }
                    if (!labor.PerformingOrgID.HasValue || labor.PerformingOrgID < 1)
                    {
                        validationErrors.Add(new ValidationMessage(fieldPrefix + "PerformingOrgID", "Performing organization is required or is not valid."));
                    }
                    if (!labor.SpreadCurveID.HasValue || labor.SpreadCurveID == SpreadCurves.None)
                    {
                        validationErrors.Add(new ValidationMessage(fieldPrefix + "SpreadCurveID", "Spread curve is required."));
                    }
                    if (!labor.PercentSpread.HasValue && labor.RateType == RateType.Hours)
                    {
                        validationErrors.Add(new ValidationMessage(fieldPrefix + "PercentSpread", "Percent spread is required."));
                    }

                    if (!labor.HourSpread.HasValue && labor.RateType == RateType.Hours)
                    {
                        if (isUsingEP)
                        {
                            validationErrors.Add(new ValidationMessage(fieldPrefix + "HourSpread", "EP spread is required."));
                        }
                        else
                        {
                            validationErrors.Add(new ValidationMessage(fieldPrefix + "HourSpread", "Hours spread is required."));
                        }
                    }

                    if (labor.RateType == RateType.Cost && !labor.CostSpread.HasValue)
                    {
                        validationErrors.Add(new ValidationMessage(fieldPrefix + "CostSpread", "Cost is required."));
                    }

                    if (labor.RateType == RateType.Cost && labor.HourSpread.HasValue && labor.HourSpread.Value != 0)
                    {
                        validationErrors.Add(new ValidationMessage(fieldPrefix + "HourSpread", "Hours spread is non-zero for a Cost Resource Type."));
                    }

                    if (string.IsNullOrEmpty(labor.StartDate))
                    {
                        validationErrors.Add(new ValidationMessage(fieldPrefix + "StartDate", "Start date is required."));
                    }
                    if (string.IsNullOrEmpty(labor.EndDate))
                    {
                        validationErrors.Add(new ValidationMessage(fieldPrefix + "EndDate", "End date is required."));
                    }
                    if (boe.IsMultiClinWbs && !(labor.CLINID > 0) && !(labor.WBSID > 0))
                    {
                        validationErrors.Add(new ValidationMessage(fieldPrefix, "WBS or CLIN must be selected."));
                    }
                }
            }

            return validationErrors;
        }

        /// <summary>
        /// Validate Task Element DTO before saving
        /// </summary>
        /// <param name="ws">workspace</param>
        /// <param name="taskElement">task element</param>
        public ICollection<ValidationMessage> ValidateTaskElementDto(FullWorkspace ws, BoeTaskElementDTO taskElement)
        {
            _ = ws ?? throw new ArgumentNullException(nameof(ws));
            _ = taskElement ?? throw new ArgumentNullException(nameof(taskElement));
            
            ICollection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();
            FullBoe boe = factory.CreateFullBoe(taskElement.BoeID);

            // Validate Task Variables
            VariableCircularReferenceCheckerCache circularReferenceCache = new VariableCircularReferenceCheckerCache();
            List<WorkspaceVariableDTO> inUseWorkspaceVariables = ws.WorkspaceVariables.Where(i => taskElement.WorkspaceVariableIDs.Contains(i.Id)).ToList();
            ICollection<OrdinaryVariableDto> invalidOrdinaryVariables = circularReferenceChecker.OrdinaryVariablesCreateCircularReference(circularReferenceCache, taskElement.BoeID, taskElement.OrdinaryVariables, ws);
            ICollection<WorkspaceVariableDTO> invalidWorkspaceVariables = circularReferenceChecker.WorkspaceVariablesCreateCircularReference(circularReferenceCache, taskElement.BoeID, inUseWorkspaceVariables, ws);
            ICollection<string> allInvalidVariableNames = invalidWorkspaceVariables.Select(w => w.WorkspaceVariableName).Union(invalidOrdinaryVariables.Select(o => o.OrdinaryVariableName)).ToCollection();

            if (allInvalidVariableNames.Any())
            {
                validationErrors.Add(new ValidationMessage("TaskVariable", "Some variables included in the MOQ Equation will cause circular references to occur. The following variables must be removed before saving: " + String.Join(", ", allInvalidVariableNames)));
            }

            // Validate Spread Dates
            bool startDateError = false;
            bool endDateError = false;
            foreach(ResourceTypeDto labor in taskElement.taskElementLabors)
            {
                foreach (ResourceSpreadDto laborSpread in labor.LaborSpreads)
                {
                    if (laborSpread.LaborSpreadDate < labor.StartDate && !startDateError)
                    {
                        startDateError = true;
                        validationErrors.Add(new ValidationMessage("StartDate", "Resource Spread Start Date cannot be before the Resource Type Start Date.", "LaborSpreadForm"));
                    }

                    if (laborSpread.LaborSpreadDate > labor.EndDate && !endDateError)
                    {
                        endDateError = true;
                        validationErrors.Add(new ValidationMessage("EndDate", "Resource Spread End Date cannot be after the Resource Type End Date.", "LaborSpreadForm"));
                    }
                }
            }           

            // perform BOE Labor Type validation
            ICollection<string> errors = new List<string>();
            List<IStartEndDates> tasksToValidate = new List<IStartEndDates>();
            StartEndDateTypeValidator BOEvalidator = new StartEndDateTypeValidator(boe.StartDate, boe.EndDate, "BOE", "Task");
            
            if (taskElement.Updateable != UpdateType.Deleted)
            {
                tasksToValidate.Add(taskElement);
                errors = BOEvalidator.validation(tasksToValidate, (Collection<Dictionary<string, string>>)null);

                // gather errors up, if any
                foreach (string error in errors)
                {
                    validationErrors.Add(new ValidationMessage("TaskElements", error));
                }
            }

            #region Identify sum-of-BOE task variable mappings for deletion

            BoeTaskElementDTO originalTaskElement = factory.CreateTaskElement(taskElement.Id, ws.DecimalPrecision, ws.CostDecimalPrecision);

            if (originalTaskElement != null)
            {
                ICollection<int> ordinaryVariableIds =
                    (from U in taskElement.OrdinaryVariables
                     join O in originalTaskElement.OrdinaryVariables on U.Id equals O.Id
                     select O.Id).ToList();

                foreach (int ordinaryVariableId in ordinaryVariableIds)
                {
                    OrdinaryVariableDto updatedTaskVariable;
                    OrdinaryVariableDto originalTaskVariable;

                    if ((originalTaskVariable = originalTaskElement.OrdinaryVariables.FirstOrDefault(v => v.Id == ordinaryVariableId)) != null &&
                        (updatedTaskVariable = taskElement.OrdinaryVariables.FirstOrDefault(v => v.Id == ordinaryVariableId)) != null)
                    {
                        foreach (SelectBOEsToSum selection in originalTaskVariable.SelectedBOEsToSum)
                        {
                            bool stillSelected = true;

                            if (selection.WBSID.HasValue)
                            {
                                stillSelected = updatedTaskVariable.SelectedBOEsToSum.Any(s => s.WBSID == selection.WBSID.Value);
                            }
                            else if (selection.CLINID.HasValue)
                            {
                                stillSelected = updatedTaskVariable.SelectedBOEsToSum.Any(s => s.CLINID == selection.CLINID.Value);
                            }
                            else if (selection.BoeID.HasValue)
                            {
                                stillSelected = updatedTaskVariable.SelectedBOEsToSum.Any(s => s.BoeID == selection.BoeID.Value);
                            }

                            if (!stillSelected)
                            {
                                // add the original selection as a "to-be-deleted" item on the updated selections
                                selection.Updateable = UpdateType.Deleted;
                                updatedTaskVariable.SelectedBOEsToSum.Add(selection);
                            }
                        }
                    }
                }
            }
            #endregion

            return validationErrors;
        }

        /// <summary>
        /// Save the Labor Task data
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="dtoToSave">Task DTO</param>
        /// <param name="metricIds">Metric IDs</param>
        /// <param name="answers">RTE Template Answers</param>
        /// <param name="moqTypes">MOQ Types for the task</param>
        public void SaveLaborTaskData(FullWorkspace ws, BoeTaskElementDTO dtoToSave, ICollection<int> metricIds, ICollection<RTECustomTemplateQuestionAnswerModelView> answers, ICollection<MoqTypeSelection> moqTypes)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (moqTypes == null)
            {
                throw new ArgumentNullException(nameof(moqTypes));
            }

            #region Identify workspace variables for update and if Other BOE Recalcuations needed

            bool OtherBOERecalculationsNeeded = false;

            // Dictionary to keep track of workspace variable IDs that need to be updated and their old workspace total
            Dictionary<int, decimal> WorkspaceVarOldValueD = new Dictionary<int, decimal>();
            ICollection<WorkspaceVariableDTO> workspaceVariablesForBoe = FullWorkspaceHelper.GetWorkspaceVariablesAssociatedWithBoe(dtoToSave.BoeID, ws);

            // before the save occurs, get all task variables that will be effected by the save so we can get the old total value
            // to be put into an email
            if (workspaceVariablesForBoe.Any())
            {
                if (dtoToSave.taskElementLabors.Any())
                {
                    foreach (WorkspaceVariableDTO workspacevar in workspaceVariablesForBoe)
                    {
                        DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                        data.FillData(null, new List<WorkspaceVariableDTO>() { workspacevar }, ws);

                        decimal oldTotalValue = VariableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspacevar, data);

                        //update the data variable with the current data then calculate the Sum to determine if there was a difference.
                        data.UpdateBOETasks(dtoToSave.BoeID, new Collection<BoeTaskElementDTO>() { dtoToSave }, ws);
                        decimal newTotalValue = VariableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspacevar, data);

                        // if there's a difference, or a new item was inserted, we'll recalculate the workspace var
                        if (oldTotalValue != newTotalValue || dtoToSave.Id < 0)
                        {
                            WorkspaceVarOldValueD.Add(workspacevar.Id, oldTotalValue);
                            OtherBOERecalculationsNeeded = true;
                        }
                    }
                }
            }

            // Other recalculations also needed if new task or updated moq
            if(dtoToSave.Id < 0)
            {
                OtherBOERecalculationsNeeded = true;
            }
            else
            {
                BoeTaskElementDTO originalTask = ws.TaskElements.First(x => x.Id == dtoToSave.Id);
                if(originalTask.TotalHours != dtoToSave.TotalHours)
                {
                    OtherBOERecalculationsNeeded = true;
                }
            }

            IDictionary<int, decimal> taskElementTotalHours = null;

            #endregion

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                int newTaskId = this._BoeTaskElementMediator.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO>() { dtoToSave }, ws).First().Value;

                if (answers != null && answers.Any())
                {
                    answers.ForEach(x => { x.TaskId = newTaskId; });
                    this.rteTemplateDataLoader.SaveAnswers(answers);
                }

                if (ws.UsingTemplateBOE)
                {
                    // Update MOQ Types via kill and fill
                    // Get existing MOQ Types and table data, set them all to deleted, and save
                    ICollection<MoqTypeSelection> existingMoqTypes = this.moqTypeDataLoader.GetByBoeId(dtoToSave.BoeID).Where(x => x.TaskId == dtoToSave.Id).ToCollection();
                    foreach(var moqType in existingMoqTypes)
                    {
                        moqType.Updateable = UpdateType.Deleted;
                    }

                    this.moqTypeDataLoader.Save(existingMoqTypes);

                    // Set all incoming MOQ Types as Upsert and set ids to -1 for insert
                    int i = -1;
                    foreach (MoqTypeSelection moqType in moqTypes)
                    {
                        moqType.Id = i--;
                        moqType.Updateable = UpdateType.Upsert;

                        moqType.TaskId = newTaskId;

                        foreach(MoqTableData table in moqType.TableData)
                        {
                            table.Id = i--;
                            table.Updateable = UpdateType.Upsert;

                            table.DateOfReport = table.DateOfReport.Normalize(DateTimePrecision.Day);
                            table.PoPStart = table.PoPStart.Normalize(DateTimePrecision.Day);
                            table.PoPEnd = table.PoPEnd.Normalize(DateTimePrecision.Day);
                        }
                    }

                    this.moqTypeDataLoader.Save(moqTypes);
                }

                ws.RefreshBoes();

                // Save historical metrics for BOE Task Element
                // Previously we had to convert the id from output of MediatedBulkSaveTaskElements for new Task Elements, but now dtoToSave.Id is set correctly
                this.SaveHistoricalMetricsToTaskElement(dtoToSave.Id, metricIds);
                
                if (OtherBOERecalculationsNeeded)
                {
                    // first perform workspace variable recalculations
                    Collection<WorkspaceVariableDTO> colWorkspaceVariables = new Collection<WorkspaceVariableDTO>();
                    // save the workspace variables
                    foreach (int x in WorkspaceVarOldValueD.Keys)
                    {
                        WorkspaceVariableDTO workspaceVar = ws.WorkspaceVariables.First(i => i.Id == x);

                        DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                        data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

                        workspaceVar.WorkspaceVariableValue = VariableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
                        workspaceVar.Updateable = UpdateType.Upsert;
                        colWorkspaceVariables.Add(workspaceVar);
                    }

                    this._WorkspaceVariableDTODataLoader.SaveWorkspaceVariables(colWorkspaceVariables);
                    ws.RefreshWorkspaceVariables();

                    // find all linked task elements thru a task or workspace variable and then save any updates
                    Collection<BoeTaskElementDTO> otherBoeTaskElementToSave = new Collection<BoeTaskElementDTO>();
                    taskElementTotalHours = this.CalculateLinkedTaskElements(new Collection<ValidationMessage>(), new Collection<BoeTaskElementDTO>() { dtoToSave }, otherBoeTaskElementToSave, ws);
                }

                scope.Complete();
            }

            if (taskElementTotalHours != null)
            {
                foreach (KeyValuePair<int, decimal> entry in taskElementTotalHours)
                {
                    BoeTaskElementDTO taskElement;
                    if ((taskElement = ws.TaskElements.FirstOrDefault(t => t.Id == entry.Key)) != null)
                    {
                        if (entry.Value != taskElement.TotalHours)  // If the task's total hours are being changed ...
                        {
                            // ... then also mark the parent BOE for update.

                            BoeDTO taskElementBOE;
                            if ((taskElementBOE = ws.Boes.FirstOrDefault(b => b.Id == taskElement.BoeID)) != null)
                            {
                                taskElementBOE.Updateable = UpdateType.Upsert;  // for state transition
                            }
                        }
                    }
                }
            }

            // Process task variable dependencies
            if (OtherBOERecalculationsNeeded)
            {
                this.ProcessAllVariableDependencies(dtoToSave.BoeID, ws);
            }
        }

        /// <summary>
        /// Calculate any linked task elements that need to be updated based on the currently being saved task element
        /// The most common example of this type of update is a task element that references the "being saved task element" in a task or workspace variable
        /// </summary>
        /// <param name="inValidationErrors">validation errors</param>
        /// <param name="inTaskElementsToSave">task elements to save</param>
        /// <param name="inOtherBoeTaskElementToSave">more task elements to save</param>
        /// <param name="fullWorkspace">Full workspace object.</param>
        /// <returns>Table that maps task-element-ID to recalculated labor hours; only contains entries whose values have changed</returns>
        public IDictionary<int, decimal> CalculateLinkedTaskElements(Collection<ValidationMessage> inValidationErrors, ICollection<BoeTaskElementDTO> inTaskElementsToSave,
            Collection<BoeTaskElementDTO> inOtherBoeTaskElementToSave, FullWorkspace fullWorkspace)
        {
            if (inTaskElementsToSave == null)
            {
                throw new ArgumentNullException(nameof(inTaskElementsToSave));
            }
            if (fullWorkspace == null)
            {
                throw new ArgumentNullException(nameof(fullWorkspace));
            }

            Dictionary<int, decimal> valueMapping = new Dictionary<int, decimal>();

            foreach (BoeTaskElementDTO taskElement in inTaskElementsToSave)
            {
                FullBoe boe = fullWorkspace.Boes.First(b => b.Id == taskElement.BoeID);

                Collection<BoeTaskElementDTO> boeTaskElements = new Collection<BoeTaskElementDTO>();
                Collection<WorkspaceVariableDTO> workspaceVariables = new Collection<WorkspaceVariableDTO>();

                // check if labor recalculation is needed given a task variable. if it is, it will be saved
                this.CheckIfLaborRecalculationIsNeededBasedOnSumToBOEVariable(fullWorkspace, inValidationErrors, inOtherBoeTaskElementToSave, boe, this._BoeTaskElementRecalculation.RecalculateLaborWithBoe(boe, VariableType.Task, fullWorkspace, boeTaskElements, workspaceVariables), boeTaskElements, workspaceVariables, valueMapping);

                // check if labor recalculation is needed given a workspace variable. if it is, it will be saved
                this.CheckIfLaborRecalculationIsNeededBasedOnSumToBOEVariable(fullWorkspace, inValidationErrors, inOtherBoeTaskElementToSave, boe, this._BoeTaskElementRecalculation.RecalculateLaborWithBoe(boe, VariableType.Workspace, fullWorkspace, boeTaskElements, workspaceVariables), boeTaskElements, workspaceVariables, valueMapping);
            }

            return valueMapping;
        }

        /// <summary>
        /// check if labor recalculation is needed for a boe to sum workspace/task variable based off of the boe
        /// </summary>
        /// <param name="fullWorkspace">The full workspace</param>
        /// <param name="inValidationErrors">validation errors</param>
        /// <param name="inOtherBoeTaskElementToSave">other task elements to save</param>
        /// <param name="inBoe">boe</param>
        /// <param name="inNextSetToCheck">next task elements to check</param>
        /// <param name="incomingBoeTaskElements"></param>
        /// <param name="incomingWorkspaceVariables"></param>
        /// <param name="valueMapping"></param>
        public void CheckIfLaborRecalculationIsNeededBasedOnSumToBOEVariable(FullWorkspace fullWorkspace, Collection<ValidationMessage> inValidationErrors, Collection<BoeTaskElementDTO> inOtherBoeTaskElementToSave, FullBoe inBoe, Collection<BoeTaskElementDTO> inNextSetToCheck, Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, Collection<WorkspaceVariableDTO> incomingWorkspaceVariables = null, Dictionary<int, decimal> valueMapping = null)
        {
            if (fullWorkspace == null)
            {
                throw new ArgumentNullException(nameof(fullWorkspace));
            }

            if (incomingBoeTaskElements == null)
            {
                incomingBoeTaskElements = new Collection<BoeTaskElementDTO>();
            }

            if (incomingWorkspaceVariables == null)
            {
                incomingWorkspaceVariables = new Collection<WorkspaceVariableDTO>();
            }

            if (inBoe == null)
            {
                throw new ArgumentNullException(nameof(inBoe));
            }

            if (inNextSetToCheck == null)
            {
                throw new ArgumentNullException(nameof(inNextSetToCheck));
            }

            if (inOtherBoeTaskElementToSave == null)
            {
                throw new ArgumentNullException(nameof(inOtherBoeTaskElementToSave));
            }

            if (inValidationErrors == null)
            {
                throw new ArgumentNullException(nameof(inValidationErrors));
            }

                if (valueMapping == null)
                {
                    valueMapping = new Dictionary<int, decimal>();
                }

                foreach (BoeTaskElementDTO task in inNextSetToCheck)
                {
                    bool containsTaskAlready = inOtherBoeTaskElementToSave.Any(x => x.Id == task.Id);
                    bool valueHasChanged = false;
                    bool updateBOEFlag = false;

                    if (valueMapping.ContainsKey(task.Id))
                    {
                        decimal? newValue = task.taskElementLabors.Where(a => a.ValueSpread.HasValue).Sum(a => a.ValueSpread);

                        if (newValue != task.TotalHours)
                        {
                            updateBOEFlag = true;
                        }

                        if (valueMapping[task.Id] != newValue.Value)
                        {
                            valueHasChanged = true;
                            valueMapping[task.Id] = newValue.Value;
                        }
                    }
                    else
                    {
                        decimal? value = task.taskElementLabors.Where(a => a.ValueSpread.HasValue).Sum(a => a.ValueSpread);

                        if (value != task.TotalHours)
                        {
                            updateBOEFlag = true;
                        }

                        valueMapping.Add(task.Id, value.Value);
                    }

                    FullBoe boeToSave = fullWorkspace.Boes.First(x => x.Id == task.BoeID);

                if ((!containsTaskAlready || valueHasChanged) && updateBOEFlag)
                {
                    // this only needs to happen if it's the first time it changed;
                    if (!containsTaskAlready)
                    {
                        inOtherBoeTaskElementToSave.Add(task);

                        // get boe to determine if state needs to change. if a variable causes the task element to be recalculated and the boe is in awaiting approval/approved, it needs
                        // to go back to draft
                        BOEState oldBOEState = boeToSave.State;
                        if (boeToSave.State == BOEState.AwaitingApproval || boeToSave.State == BOEState.Approved || boeToSave.State == BOEState.DraftLocked)
                        {
                            BOEState newBOEState = BOEState.Draft;

                            // Validate the Awaiting Approval or Approved to Draft state transition
                            string validationMessage;
                            if (!this._boeStateMachine.PerformStateTransitionValidation(boeToSave, fullWorkspace, oldBOEState, newBOEState, out validationMessage))
                            {
                                // not valid ... communicate to user
                                inValidationErrors.Add(new ValidationMessage("TaskVariable", validationMessage));
                            }

                            // If the transition is valid, set the BOE to Draft and save it
                            boeToSave.Updateable = UpdateType.Upsert;
                            boeToSave.State = newBOEState;

                            this._BoeMediator.MediatedSave(fullWorkspace, boeToSave);

                            // Perform common state transition actions
                            this._boeStateMachine.PerformStateTransitionAction(boeToSave, fullWorkspace, oldBOEState, boeToSave.State);
                        }
                    }

                    // every time the value changes, we need to do a save of the new data, and recalculate the WS variables affected
                    if (valueHasChanged)
                    {
                        // Need to update the last-update-date, in order to be able to save it..
                        BoeTaskElementDTO oldElement = this.factory.CreateTaskElement(task.Id, fullWorkspace.DecimalPrecision, fullWorkspace.CostDecimalPrecision);

                        task.UpdateDate = oldElement.UpdateDate;
                        task.Updateable = UpdateType.Upsert;

                        foreach (OrdinaryVariableDto var in task.OrdinaryVariables)
                        {
                            var.UpdateDate = oldElement.OrdinaryVariables.Where(a => a.Id == var.Id).Select(a => a.UpdateDate).FirstOrDefault();
                            var.Updateable = UpdateType.Upsert;
                        }

                        foreach (ResourceTypeDto type in task.taskElementLabors)
                        {
                            ResourceTypeDto oldLaborType = oldElement.taskElementLabors.FirstOrDefault(a => a.Id == type.Id);

                            type.UpdateDate = oldLaborType.UpdateDate;
                            type.Updateable = UpdateType.Upsert;

                            foreach (ResourceSpreadDto spread in type.LaborSpreads)
                            {
                                spread.UpdateDate = oldLaborType.LaborSpreads.Where(a => a.Id == spread.Id).Select(a => a.UpdateDate).FirstOrDefault();
                                spread.Updateable = UpdateType.Upsert;
                            }
                        }
                    }

                    // Save the updated BOE Task Element
                    this._BoeTaskElementMediator.MediatedBulkSaveTaskElements(new Collection<BoeTaskElementDTO> { task }, fullWorkspace);

                    // Need to makes sure any workspace variables that are based on this BOE are updated as well
                    ICollection<WorkspaceVariableDTO> workspaceVariables = FullWorkspaceHelper.GetWorkspaceVariablesAssociatedWithBoe(boeToSave.Id, fullWorkspace);
                    if (workspaceVariables.Any())
                    {
                        foreach (WorkspaceVariableDTO workspaceVariable in workspaceVariables)
                        {
                            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                            data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVariable }, fullWorkspace);

                            workspaceVariable.WorkspaceVariableValue = this.VariableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVariable, data);
                            workspaceVariable.Updateable = UpdateType.Upsert;
                            this._WorkspaceVariableDTODataLoader.SaveWorkspaceVariables(new Collection<WorkspaceVariableDTO> { workspaceVariable });
                        }
                        fullWorkspace.RefreshWorkspaceVariables();
                    }

                    // Only need to do the recursive call if it's the first time through
                    if (!containsTaskAlready)
                    {
                        // recursive call to make sure the last updated task element doesn't effect another task element
                        this.CheckIfLaborRecalculationIsNeededBasedOnSumToBOEVariable(fullWorkspace, inValidationErrors, inOtherBoeTaskElementToSave, boeToSave, this._BoeTaskElementRecalculation.RecalculateLaborWithBoe(boeToSave, VariableType.Task, fullWorkspace, incomingBoeTaskElements, incomingWorkspaceVariables),
                            incomingBoeTaskElements, incomingWorkspaceVariables, valueMapping);

                        this.CheckIfLaborRecalculationIsNeededBasedOnSumToBOEVariable(fullWorkspace, inValidationErrors, inOtherBoeTaskElementToSave, boeToSave, this._BoeTaskElementRecalculation.RecalculateLaborWithBoe(boeToSave, VariableType.Workspace, fullWorkspace, incomingBoeTaskElements, incomingWorkspaceVariables),
                            incomingBoeTaskElements, incomingWorkspaceVariables, valueMapping);
                    }
                }
            }
        }

        /// <summary>
        /// Validate MOQ Equation to determine if a save task element can continue, then make sure it can be calculated correctly
        /// </summary>
        /// <param name="inBoeID">boe</param>
        /// <param name="laborTaskData">task element</param>
        /// <param name="inValidationErrors">validation errors</param>
        /// <param name="inWorkspaceDTO">workspace</param>
        /// <returns>The MOQ equation result, if valid</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public string ValidateMOQEquation(int inBoeID, LaborTaskDataModelView laborTaskData, ICollection<ValidationMessage> inValidationErrors, FullWorkspace inWorkspaceDTO)
        {
            string result = string.Empty;
            _ = laborTaskData ?? throw new ArgumentNullException(nameof(laborTaskData));
            _ = inValidationErrors ?? throw new ArgumentNullException(nameof(inValidationErrors));
            _ = inWorkspaceDTO ?? throw new ArgumentNullException(nameof(inWorkspaceDTO));

            //  Ensure the MOQ Equation is valid. if not, display a validation error to the user, don't allow the save
            try
            {
                // a moq equation is allowed to be empty so don't validate on an empty one
                // First, need to validate the calculation, this will catch things like a missing (
                // Second, need to calculate the equation, this will catch things like a task variable not filled out completely
                if (!string.IsNullOrEmpty(laborTaskData.TaskElementData.MOQHoursEquation))
                {
                    Parser.Validate(laborTaskData.TaskElementData.MOQHoursEquation);

                    // for each workspace variable id in the BOE Task, get it's info from the Workspace DTO Mapper
                    Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO>();

                    // get all the workspace variables for the workspace
                    IReadOnlyCollection<WorkspaceVariableDTO> workspaceDTOVariables = inWorkspaceDTO.WorkspaceVariables;
                    foreach (int workspaceVariableId in laborTaskData.TaskElementData.WorkspaceVariableIDs)
                    {
                        foreach (WorkspaceVariableDTO wv in workspaceDTOVariables)
                        {
                            if (wv.Id == workspaceVariableId)
                            {
                                workspaceVars.Add(wv);
                                break;
                            }
                        }
                    }

                    Collection<OrdinaryVariableDto> boeTaskVars2 = this.ConvertTaskVariableModelViewCollectionToTaskOrdinaryVariableCollection(laborTaskData.TaskElementData.TaskOrdinaryVariables, laborTaskData.TaskElementData.TaskElementDetailID.Value, inBoeID, inWorkspaceDTO);

                    DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                    data.FillData(boeTaskVars2, workspaceVars, inWorkspaceDTO);

                    result = Parser.Calculate(laborTaskData.TaskElementData.MOQHoursEquation, boeTaskVars2, workspaceVars, this.VariableSelectBOEtoSumCalculation, data, inWorkspaceDTO);
                }
            }
            catch (GeneralMOQParsingException)
            {
                inValidationErrors.Add(new ValidationMessage("Invalid MOQ Equation ", "An invalid MOQ equation was entered."));
            }
            catch (GeneralMOQCalculationException)
            {
                inValidationErrors.Add(new ValidationMessage("Invalid MOQ Equation ", "An invalid MOQ equation was entered."));
            }
            catch (Exception)
            {
                inValidationErrors.Add(new ValidationMessage("Invalid MOQ Equation ", "An invalid MOQ equation was entered."));
            }

            return result;
        }

        /// <summary>
        /// Convert Collection of BoeTaskOrdinaryVariable to Collection of BoeTaskOrdinaryVariable
        /// This is used in the saving of a task element 
        /// </summary>
        /// <param name="inTaskVariableModelViews">task variable model views</param>
        /// <param name="inTaskElementID"> task element id</param>
        /// <param name="inBoeID">boe id </param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>task variables</returns>
        public Collection<OrdinaryVariableDto> ConvertTaskVariableModelViewCollectionToTaskOrdinaryVariableCollection(Collection<BoeTaskOrdinaryVariableModelView> inTaskVariableModelViews, int inTaskElementID, int inBoeID, WorkspaceDTO workspace)
        {
            if (inTaskVariableModelViews == null)
            {
                throw new ArgumentNullException(nameof(inTaskVariableModelViews));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            
            Collection<OrdinaryVariableDto> toReturn = new Collection<OrdinaryVariableDto>();
            // Convert each ordinary variable model view into a DTO for saving
            foreach (BoeTaskOrdinaryVariableModelView boeTaskOrdinaryVariableModelView in inTaskVariableModelViews)
            {
                // new variable
                if (boeTaskOrdinaryVariableModelView.OrdinaryVariableID < 0)
                {
                    toReturn.Add(boeTaskOrdinaryVariableModelView.GetAssociatedDTO());
                }
                else // existing variable
                {
                    BoeTaskElementDTO taskElement = this.factory.CreateTaskElement(inTaskElementID, workspace.DecimalPrecision, workspace.CostDecimalPrecision);
                    OrdinaryVariableDto boeTaskOrdinaryVariableDTO = (from v in taskElement.OrdinaryVariables
                                                                      where v.Id == boeTaskOrdinaryVariableModelView.OrdinaryVariableID
                                                                      select v).FirstOrDefault();

                    DataRelationshipVerifier.VerifyDataRelation(boeTaskOrdinaryVariableDTO, inBoeID);

                    OrdinaryVariableDto updatedVar = boeTaskOrdinaryVariableModelView.GetAssociatedDTO(boeTaskOrdinaryVariableDTO);


                    toReturn.Add(updatedVar);
                }
            }
            return toReturn;
        }

        /// <summary>
        /// This function will validate task start/end date, labor type level custom fields, task element level custom fields, task ID being unique with the BOE,
        /// and task variable unique name
        /// </summary>
        /// <param name="boeDTO">boe</param>
        /// <param name="laborTaskData">task modelview includes task details, labors, and spreads</param>
        /// <param name="inValidationErrors">validation errors</param>
        /// <param name="ws">workspace</param>
        public void ValidateTaskDetails(FullBoe boeDTO, LaborTaskDataModelView laborTaskData, ICollection<ValidationMessage> inValidationErrors, FullWorkspace ws)
        {
            _ = boeDTO ?? throw new ArgumentNullException(nameof(boeDTO));
            _ = inValidationErrors ?? throw new ArgumentNullException(nameof(inValidationErrors));
            _ = ws ?? throw new ArgumentNullException(nameof(ws));
            _ = laborTaskData ?? throw new ArgumentNullException(nameof(laborTaskData));
            _ = laborTaskData.TaskElementData ?? throw new ArgumentNullException(nameof(laborTaskData), "TaskElementData cannot be null");

            // validate RTE field length
            if(ws.RteSizeLimit.HasValue)
            {
                this.ValidateTaskElementRteSizeLimit(laborTaskData, ws, inValidationErrors);
            }

            this.ValidateTaskElementDates(laborTaskData, ws, boeDTO, inValidationErrors, out BoeTaskElementDTO taskElement);
            this.ValidateLaborTypeDates(laborTaskData, inValidationErrors);
            this.ValidateLaborTypeCustomFields(laborTaskData, ws, taskElement, inValidationErrors);
            this.ValidateTaskCustomFields(laborTaskData, ws, inValidationErrors);
            this.ValidateMoqTypes(ws, laborTaskData, inValidationErrors);
        }

        /// <summary>
        /// Validate the RTE Size Limit in the Task Element
        /// </summary>
        /// <param name="laborTaskData">Task Composite MV</param>
        /// <param name="ws">Workspace</param>
        /// <param name="inValidationErrors">Validation Errors collection</param>
        private void ValidateTaskElementRteSizeLimit(LaborTaskDataModelView laborTaskData, WorkspaceDTO ws, ICollection<ValidationMessage> inValidationErrors)
        {
            if (!string.IsNullOrEmpty(laborTaskData.TaskElementData.TaskDescription) && ws.RteSizeLimit < GenBOEUtilities.ConvertHtmlToText(laborTaskData.TaskElementData.TaskDescription).Length)
            {
                inValidationErrors.Add(new ValidationMessage("TaskDescription",
                    string.Format("The maximum length of Task Description is {0} characters.", ws.RteSizeLimit.Value)));
            }

            if (!string.IsNullOrEmpty(laborTaskData.TaskElementData.MOQText) && ws.RteSizeLimit < GenBOEUtilities.ConvertHtmlToText(laborTaskData.TaskElementData.MOQText).Length)
            {
                inValidationErrors.Add(new ValidationMessage("MOQText",
                    string.Format("The maximum length of MOQ Rationale is {0} characters.", ws.RteSizeLimit.Value)));
            }
        }

        /// <summary>
        /// Validate Task Element dates
        /// </summary>
        /// <param name="laborTaskData">Task Composite MV</param>
        /// <param name="ws">Workspace</param>
        /// <param name="boeDTO">BOE</param>
        /// <param name="inValidationErrors">Validation Errors collection</param>
        /// <param name="taskElement">Task Element</param>
        private void ValidateTaskElementDates(LaborTaskDataModelView laborTaskData, WorkspaceDTO ws, FullBoe boeDTO, ICollection<ValidationMessage> inValidationErrors, out BoeTaskElementDTO taskElement)
        {
            if (!laborTaskData.TaskElementData.TaskElementDetailID.HasValue || laborTaskData.TaskElementData.TaskElementDetailID.Value < 0)
            {
                taskElement = null;

                // Verify Task Element Dates are with BOE Range.
                if (GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(laborTaskData.TaskElementData.StartDate), DateTimePrecision.Month) <
                    GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(boeDTO.StartDate), DateTimePrecision.Month))
                {
                    inValidationErrors.Add(new ValidationMessage("Task Element", "Task Element Start Date cannot be before BOE Start Date."));
                }

                if (GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(laborTaskData.TaskElementData.EndDate), DateTimePrecision.Month) >
                    GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(boeDTO.EndDate), DateTimePrecision.Month))
                {
                    inValidationErrors.Add(new ValidationMessage("Task Element", "Task Element End Date cannot be after the BOE End Date."));
                }

                if (GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(laborTaskData.TaskElementData.StartDate), DateTimePrecision.Month) >
                    GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(laborTaskData.TaskElementData.EndDate), DateTimePrecision.Month))
                {
                    inValidationErrors.Add(new ValidationMessage("StartDate", "Task start date must happen before the end date."));
                }
            }
            else
            {
                taskElement = this.factory.CreateTaskElement(laborTaskData.TaskElementData.TaskElementDetailID.Value, ws.DecimalPrecision, ws.CostDecimalPrecision);
                laborTaskData.TaskElementData.StartDate = taskElement.StartDate.Value.ToMonthString();
                laborTaskData.TaskElementData.EndDate = taskElement.EndDate.Value.ToMonthString();
            }
        }

        /// <summary>
        /// Verify Labor Types are within Task Element date range
        /// </summary>
        /// <param name="laborTaskData">Task Composite MV</param>
        /// <param name="inValidationErrors">Validation Errors collection</param>
        private void ValidateLaborTypeDates(LaborTaskDataModelView laborTaskData, ICollection<ValidationMessage> inValidationErrors)
        {
            if (laborTaskData.LaborTypesData.Count > 0)
            {
                List<LaborTypeDataModelView> ltList = (from a in laborTaskData.LaborTypesData
                              where !a.Deleted
                              select a).ToList();

                // Only do if all Labor Types have not been deleted.
                if (ltList.Any())
                {
                    try
                    {
                        if (GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(ltList.Min(l => l.StartDate)), DateTimePrecision.Month) <
                            GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(laborTaskData.TaskElementData.StartDate), DateTimePrecision.Month))
                        {
                            inValidationErrors.Add(new ValidationMessage("Resource Types", "Resource Types Start Date cannot be before Task Element Start Date.", "LaborTypesForm"));
                        }

                        if (GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(ltList.Max(l => l.EndDate)), DateTimePrecision.Month) >
                            GenBOEUtilities.AdjustDateTimePrecision(Convert.ToDateTime(laborTaskData.TaskElementData.EndDate), DateTimePrecision.Month))
                        {
                            inValidationErrors.Add(new ValidationMessage("Resource Types", "Resource Types End Date cannot be after the Task Element End Date.", "LaborTypesForm"));
                        }
                    }
                    catch(FormatException)
                    {
                        // do nothing, this validation message is caught and added elsewhere (MVC)
                    }
                }
            }
        }

        /// <summary>
        /// Validate the Labor Type Custom Fields
        /// </summary>
        /// <param name="laborTaskData">Task Composite MV</param>
        /// <param name="ws">Workspace</param>
        /// <param name="taskElement">Task Element</param>
        /// <param name="inValidationErrors">Validation Errors collection</param>
        private void ValidateLaborTypeCustomFields(LaborTaskDataModelView laborTaskData, FullWorkspace ws, BoeTaskElementDTO taskElement, ICollection<ValidationMessage> inValidationErrors)
        {
            //list of all customfields so we can check requiredness for LT. 
            Collection<BOECustomFieldModelView> LTCustomFields = this.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.LaborTypes);

            foreach (BOECustomFieldModelView LTCF in LTCustomFields)
            {
                if (LTCF.CustomFieldMetaData.isOpenEnded && LTCF.CustomFieldMetaData.isRequired)
                {
                    foreach (LaborTypeDataModelView laborType in laborTaskData.LaborTypesData)
                    {
                        // if the labor type is being deleted, do not worry about custom fields
                        if (!laborType.Deleted)
                        {
                            foreach (CustomFieldSelectionModelView customField in laborType.CustomFieldValues)
                            {
                                if (customField.CustomFieldID == LTCF.CustomFieldMetaData.CustomFieldID &&
                                    string.IsNullOrEmpty(customField.OpenEndedValue))
                                {
                                    ValidationMessage validationMessage = new ValidationMessage("CustomField",
                                        string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, LTCF.CustomFieldMetaData.FieldName),
                                        "LaborTypesForm");
                                    if (!inValidationErrors.Any(v => v.ValidationIssue == validationMessage.ValidationIssue))
                                    {
                                        inValidationErrors.Add(validationMessage);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Collection<BOECustomFieldOptionModelView> RFOs =
                        LTCF.CustomFieldMetaData.isRequired ? LTCF.CustomFieldOptions : null;

                    if (RFOs != null && RFOs.Any())
                    {
                        //for each LT being saved confirm it has an option for this field.
                        int laborTypeIndex = -1;
                        foreach (LaborTypeDataModelView laborType in laborTaskData.LaborTypesData)
                        {
                            // if the labor type is being deleted, do not worry about custom fields
                            if (!laborType.Deleted)
                            {
                                bool found = false;

                                ICollection<int> RequiredOptionIDs =
                                    (from AO in RFOs select AO.CustomFieldOptionID).ToList();
                                ICollection<int> SelectedValues =
                                    (from SV in laborType.CustomFieldValues select SV.CustomFieldValueID).ToList();

                                foreach (int sv in SelectedValues)
                                {
                                    if (RequiredOptionIDs.Contains(sv))
                                    {
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    string laborTypePrefix =
                                        string.Format("{0}{1}{2}", "LaborTypes[", ++laborTypeIndex, "].");
                                    ValidationMessage validationMessage = new ValidationMessage(
                                            laborTypePrefix + "LaborType-CFID" + LTCF.CustomFieldMetaData.CustomFieldID,
                                            string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, LTCF.CustomFieldMetaData.FieldName),
                                            "LaborTypesForm");
                                    if (!inValidationErrors.Any(v => v.ValidationIssue == validationMessage.ValidationIssue))
                                    {
                                        inValidationErrors.Add(validationMessage);
                                    }
                                }
                            }
                        }

                        // technically there were no labor types edit, but if there are labor types that exist with this task element, we still need to verify if a required custom
                        // field should be selected (since it could have been created after this labor type was created)
                        if (!laborTaskData.LaborTypesData.Any())
                        {
                            if (laborTaskData.TaskElementData.TaskElementDetailID.HasValue &&
                                laborTaskData.TaskElementData.TaskElementDetailID > 0)
                            {
                                ICollection<ResourceTypeDto> labors = taskElement.taskElementLabors;

                                //for each LT being saved confirm it has an option for this field.
                                foreach (ResourceTypeDto labor in labors)
                                {
                                    bool found = false;

                                    ICollection<int> RequiredOptionIDs =
                                        (from AO in RFOs select AO.CustomFieldOptionID).ToList();
                                    ICollection<int> SelectedValues =
                                        (from SV in labor.CustomFieldValueContainers select SV.CustomFieldValueID)
                                        .ToList();

                                    foreach (int sv in SelectedValues)
                                    {
                                        if (RequiredOptionIDs.Contains(sv))
                                        {
                                            found = true;
                                            break;
                                        }
                                    }

                                    if (!found)
                                    {
                                        inValidationErrors.Add(new ValidationMessage("CustomField",
                                            string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, LTCF.CustomFieldMetaData.FieldName),
                                            "LaborTypesForm"));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validate the Task Custom Fields
        /// </summary>
        /// <param name="laborTaskData">Task Composite MV</param>
        /// <param name="ws">Workspace</param>
        /// <param name="inValidationErrors">Validation Errors collection</param>
        private void ValidateTaskCustomFields(LaborTaskDataModelView laborTaskData, FullWorkspace ws, ICollection<ValidationMessage> inValidationErrors)
        {
            //list of all customfields so we can check requiredness for task details. 
            Collection<BOECustomFieldModelView> TaskCustomFields = this.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.Task);

            ICollection<BOECustomFieldModelView> StandardTaskCustomFields = TaskCustomFields.Where(x => !x.CustomFieldMetaData.isOpenEnded && x.CustomFieldMetaData.isRequired).ToCollection();

            foreach (BOECustomFieldModelView customField in StandardTaskCustomFields)
            {
                bool found = false;

                ICollection<int> RequiredOptionIDs = (from AO in customField.CustomFieldOptions select AO.CustomFieldOptionID).ToList();
                ICollection<int> SelectedValues = (from SV in laborTaskData.TaskElementData.CustomFieldValues select SV.CustomFieldValueID).ToList();

                foreach (int sv in SelectedValues)
                {
                    if (RequiredOptionIDs.Contains(sv))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    inValidationErrors.Add(new ValidationMessage("CustomField", string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, customField.CustomFieldMetaData.FieldName)));
                }
            }

            ICollection<CustomFieldSelectionModelView> openEndedTaskCustomFieldSelections =
                laborTaskData.TaskElementData.CustomFieldValues.Where(x => x.IsOpenEnded).ToCollection();
            ICollection<BOECustomFieldModelView> requiredOpenEndedTaskCustomFields =
                TaskCustomFields.Where(x => x.CustomFieldMetaData.isOpenEnded && x.CustomFieldMetaData.isRequired)
                    .ToCollection();
            
            foreach (CustomFieldSelectionModelView selection in openEndedTaskCustomFieldSelections)
            {
                BOECustomFieldModelView customField = requiredOpenEndedTaskCustomFields.FirstOrDefault(c => c.CustomFieldMetaData.CustomFieldID == selection.CustomFieldID);
                if (customField != null &&
                    string.IsNullOrEmpty(selection.OpenEndedValue))
                {
                    inValidationErrors.Add(new ValidationMessage("CustomField", string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, customField.CustomFieldMetaData.FieldName)));
                }
            }
        }

        /// <summary>
        /// Validates MOQ Types for UI, only fully required fields
        /// </summary>
        /// <param name="ws">Full WS</param>
        /// <param name="taskData">Task Data</param>
        /// <param name="errors">Validation Errors</param>
        private void ValidateMoqTypes(WorkspaceDTO ws, LaborTaskDataModelView taskData, ICollection<ValidationMessage> errors)
        {
            if (ws.UsingTemplateBOE)
            {
                ICollection<string> taskErrors = ValidateBOE.ValidateTemplateMoqForTask(taskData.MOQTypes, ws.RteSizeLimit);
                errors.AddRange(taskErrors.Select(error => new ValidationMessage(error)));
            }
        }

        /// <summary>
        /// Get custom field option model views
        /// </summary>
        /// <param name="ws">workspace</param>
        /// <param name="inTypeToGet">level of custom field to retrieve</param>
        /// <returns></returns>
        public Collection<BOECustomFieldModelView> GetCustomFieldOptionModelViews(FullWorkspace ws, ControllerCustomFieldType inTypeToGet)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            IReadOnlyCollection<CustomFieldDTO> customFields = ws.CustomFields;

            Collection<BOECustomFieldModelView> customFieldModelViews = new Collection<BOECustomFieldModelView>();

            if (customFields != null)
            {
                IReadOnlyCollection<CustomFieldValueDTO> allCustomFieldValues = ws.CustomFieldValues;
                foreach (CustomFieldDTO customField in customFields)
                {
                    BOECustomFieldsGridModelView metadata = new BOECustomFieldsGridModelView(customField);

                    ICollection<CustomFieldValueDTO> options = allCustomFieldValues.Where(i => i.CustomFieldID == customField.Id).ToCollection<CustomFieldValueDTO>();

                    metadata.inUse = options.Any(x => x.CustomFieldValueInUseFlag);

                    if ((inTypeToGet == ControllerCustomFieldType.Task && metadata.CustomFieldDisplayID == CustomFieldType.TaskDisplay) ||
                        (inTypeToGet == ControllerCustomFieldType.LaborTypes && metadata.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay))
                    {
                        Collection<BOECustomFieldOptionModelView> optionstoAdd = new Collection<BOECustomFieldOptionModelView>();

                        foreach (CustomFieldValueDTO option in options)
                        {
                            optionstoAdd.Add(new BOECustomFieldOptionModelView(option));
                        }

                        customFieldModelViews.Add(new BOECustomFieldModelView()
                        {
                            CustomFieldMetaData = metadata,
                            CustomFieldOptions = optionstoAdd
                        });
                    }
                }
            }
            return customFieldModelViews;
        }

        /// <summary>
        /// Constructs a list of spread curve choices corresponding to the designated rate type.
        /// </summary>
        /// <param name="rateType">Rate type</param>
        /// <returns>List of spread curve choices</returns>
        public ICollection<SpreadCurves> GetSpreadCurves(RateType rateType)
        {
            List<SpreadCurves> results = Enum.GetValues(typeof(SpreadCurves)).Cast<SpreadCurves>().ToList<SpreadCurves>();

            if (rateType == RateType.Cost)
            {
                results.Remove(SpreadCurves.DiscreteHours);
            }
            else
            {
                results.Remove(SpreadCurves.DiscreteCost);
            }
            results.Remove(SpreadCurves.Level);
            results.Remove(SpreadCurves.Load);
            results.Remove(SpreadCurves.None);

            return results;
        }

        /// <summary>
        /// Root-level method for recalculation of the spreads.
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="laborTabData">Contents of the labor tab page that are needed for the recalculation</param>
        /// <returns>Set of changes that need to be applied to the UI as a result of the recalculation</returns>
        public ICollection<SpreadValueTableChanges> RecalculateSpreads(FullWorkspace ws, LaborTabDataModelView laborTabData)
        {
            if (laborTabData == null)
            {
                throw new ArgumentNullException(nameof(laborTabData));
            }

            ICollection<SpreadValueTableChanges> changes = new List<SpreadValueTableChanges>();

            this.RecalculateAfterMOQEquationChange(ws, laborTabData, changes);            

            return changes;
        }

        /// <summary>
        /// Calculate Labor Spreads
        /// </summary>
        /// <param name="value">Spread Value</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="curve">Spread curve ID</param>
        /// <param name="precision">decimal precision</param>
        /// <returns>Recalculated labor spreads</returns>
        public ICollection<LaborSpreadDataModelView> CalculateLaborSpreads(decimal value, DateTime startDate, DateTime endDate, SpreadCurves curve, int precision)
        {
            ICollection<LaborSpreadDataModelView> toReturn = new Collection<LaborSpreadDataModelView>();

            LaborSpreadRequest request = new LaborSpreadRequest()
            {
                CurveID = curve,
                StartDate = startDate,
                EndDate = endDate,
                HourSpread = value
            };

            ICollection<ResourceSpreadDto> spreadDtos = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(request, precision);

            // Convert dto to mv
            foreach(ResourceSpreadDto dto in spreadDtos)
            {
                toReturn.Add(new LaborSpreadDataModelView()
                {
                    LaborSpreadDate = dto.LaborSpreadDate.ToMonthString(),
                    LaborSpreadValue = dto.LaborSpreadValue
                });
            }

            return toReturn;
        }

        /// <summary>
        /// THESE ARE LEGACY MOQ TYPES AS OF 10/2020
        /// 
        /// Gets the valid <see cref="MOQType"/>'s for this company configuration
        /// </summary>
        /// <returns>valid <see cref="MOQType"/>'s for this company configuration</returns>
        internal virtual ICollection<MOQType> GetMOQTypes()
        {
            return new MOQType[]
            {
                MOQType.Standard,
                MOQType.EstimatingRelationships,
                MOQType.Probability,
                MOQType.Factor,
                MOQType.Unit,
                MOQType.Comparison,
                MOQType.Judgment,
                MOQType.LevelOfEffort,
                MOQType.VendorQuote
            };
        }

        /// <summary>
        /// Returns the correct MOQ help text for the IS&amp;GS configuration
        /// </summary>
        /// <returns>IS&amp;GS configuration MOQ help text</returns>
        public virtual string GetMOQTypesHelpText()
        {
            return CommonConstants.BOE_MOQ_TYPES_HELP_TEXT_ISGS;
        }

        /// <summary>
        /// Returns the correct MOQ Equation label text for the IS&amp;GS configuration
        /// </summary>
        /// <returns>IS&amp;GS configuration MOQ Equation label text</returns>
        public virtual string GetMOQEquationLabel()
        {
            return CommonConstants.BOE_MOQ_EQUATION_LABEL;
        }

        /// <summary>
        /// Returns the correct MOQ text label for the IS&amp;GS configuration
        /// </summary>
        /// <returns>IS&amp;GS configuration MOQ text label text</returns>
        public virtual string GetMOQTextLabel()
        {
            return CommonConstants.BOE_MOQ_TEXT_LABEL;
        }

        /// <summary>
        /// Populates the passed in <see cref="LaborTaskModelView"/> with metric search dialog parameters.
        /// </summary>
        /// <param name="model">The <see cref="LaborTaskModelView"/> that will be populated.</param>
        public virtual void GetMetricSearchDialogParameters(LaborTaskModelView model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            model.MetricsSearchDialogParameters = new MetricsSearchDialogParametersModelView();
        }
        
        /// <summary>
        /// Populates the passed in <see cref="MOQEquationModelView"/> with metric parameters.
        /// </summary>
        /// <param name="ids">The TaskElement id's for which metrics will be retrieved</param>
        /// <param name="model">The <see cref="MOQEquationModelView"/> that will be populated</param>
        public virtual void GetMetricByTaskElementIds(Collection<int> ids, MOQEquationModelView model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            // Nothing to do here except disable search link. Metrics no longer supported for SSC.
            this.SetShowMetricLink(model);
        }

        /// <summary>
        /// Gets a <see cref="System.Web.Mvc.ViewResult"/> with historic metrics.
        /// </summary>
        /// <param name="validatedOption">?</param>
        /// <param name="searchTerm">The criteria used to find metrics</param>
        /// <returns>The <see cref="System.Web.Mvc.ViewResult"/> with historic metrics</returns>
        public virtual ViewResultData GetHistoricalMetricsResults(int validatedOption, string searchTerm)
        {
            // This code should be refactored  to remove the ViewResult as a return after the metric model views for IS&GS and Space Systems are refactored to use an interface
            // 28181
            return new ViewResultData();
        }

        /// <summary>
        /// Gets a <see cref="System.Web.Mvc.ViewResult"/> with historic metrics
        /// </summary>
        /// <param name="metricId">The id of the metric to retrieve</param>
        /// <returns>The <see cref="System.Web.Mvc.ViewResult"/> with historic metrics</returns>
        public virtual ViewResultData GetHistoricalMetricsDetails(int metricId)
        {
            throw new NotImplementedException("Historical metrics are not supported for SSC.");
        }

        /// <summary>
        /// Gets a <see cref="System.Web.Mvc.ViewResult"/> with historic metrics from the source system.
        /// </summary>
        /// <param name="metricId">The id of the metric to retrieve</param>
        /// <returns>The <see cref="System.Web.Mvc.ViewResult"/> with historic metrics</returns>
        public virtual ViewResultData GetHistoricalMetricsDetailsFromSource(int metricId)
        {
            throw new NotImplementedException("Historical metrics are not supported for SSC.");
        }

        /// <summary>
        /// Gets an <see cref="MOQEquationModelView"/> for IS&amp;GS
        /// </summary>
        /// <param name="taskElement">A <see cref="BoeTaskElementDTO"/>Task element used in the model view.</param>
        /// <param name="workspace">Full workspace object.</param>
        /// <returns>The <see cref="MOQEquationModelView"/> for the given task element.</returns>
        public virtual MOQEquationModelView GetMOQModelView(BoeTaskElementDTO taskElement, FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (taskElement == null)
            {
                throw new ArgumentNullException(nameof(taskElement));
            }

            MOQEquationModelView toReturn = new MOQEquationModelView(taskElement, this.VariableSelectBOEtoSumCalculation, workspace);
            toReturn.MoqTemplateAnswers = this.rteTemplateDataLoader.GetByBoeIdAndTaskId(workspace.Id, taskElement.BoeID, taskElement.Id).Where(t => t.SourceId == (int)RteTemplateSource.TaskMOQ).ToList();
            this.SetShowMetricLink(toReturn);
            return toReturn;
        }

        /// <summary>
        /// Sets the show metrics link.
        /// </summary>
        /// <returns>The <see cref="MOQEquationModelView"/> populated with the correct company specific value for ShowSearchMetricsLink.</returns>
        public virtual void SetShowMetricLink(MOQEquationModelView model)
        {
            if (model == null) { throw new ArgumentNullException(nameof(model)); }
            model.ShowSearchMetricsLink = false;
        }

        /// <summary>
        /// Saves metrics
        /// </summary>
        /// <param name="taskElementID">The id of the task element to save the metric to</param>
        /// <param name="metricIDs">the id of the metric to save to the task element</param>
        public virtual void SaveHistoricalMetricsToTaskElement(int taskElementID, ICollection<int> metricIDs)
        {
            // SSC historical metrics have been deprecated.
        }

        /// <summary>
        /// Gets historical metric type ahead terms matching the <paramref name="searchTerm"/>
        /// </summary>
        /// <param name="searchTerm">The criteria used to find metrics</param>
        /// <returns>The terms found</returns>
        public virtual ICollection<string> GetTypeAheadTerms(string searchTerm)
        {
            // SSC historical metrics have been deprecated.
            return new Collection<string>();
        }

        /// <summary>
        /// Returns a <see cref="bool"/> indicating if the read only flag should be overridden
        /// </summary>
        /// <param name="ws">the <see cref="FullWorkspace"/> being viewed</param>
        /// <param name="boe">the <see cref="FullWorkspace"/> being viewed</param>
        /// <returns>true if read only should be overridden, false otherwise</returns>
        public virtual bool OverrideReadOnly(FullWorkspace ws, FullBoe boe)
        {
            throw new NotImplementedException("This should be overridden by RMS/SSC");
        }

        /// <summary>
        /// Updates the OrderList for the Labor Types in a Task Element
        /// </summary>
        /// <param name="ws">The Workspace</param>
        /// <param name="taskElement">Task Element containing the Labor Types</param>
        /// <param name="modelView">Collection of the Labor Type Order</param>
        public virtual void ReOrderLaborTypeOrder(FullWorkspace ws, BoeTaskElementDTO taskElement, LaborTypeOrderCollection modelView)
        {
            if(taskElement == null)
            {
                throw new ArgumentNullException(nameof(taskElement));
            }

            // Update the order of the labor types
            taskElement.taskElementLabors.Select(x => { x.LaborTypeOrder = (modelView.LaborTypes.First(y => y.LaborTypeID == x.Id)).ListOrder; return x; }).ToCollection();

            // Update the update type for all the labor types
            taskElement.taskElementLabors.Select(x => { x.Updateable = UpdateType.Upsert; return x; }).ToCollection();
            taskElement.Updateable = UpdateType.Upsert;

            // Save
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this._BoeTaskElementMediator.MediatedBulkSaveTaskElements(new List<BoeTaskElementDTO> { taskElement }, ws);
                scope.Complete();
            }
        }

        #endregion Public members

        #region Private members

        /// <summary>
        /// Converts Boe Task Element Dto to Labor Task Data Model View
        /// </summary>
        /// <param name="ws">The Workspace.</param>
        /// <param name="boe">The Boe</param>
        /// <param name="dto">Task Element DTO</param>
        /// <returns>Converted MV</returns>
        private LaborTaskDataModelView ConvertDtoToModelView(FullWorkspace ws, FullBoe boe, BoeTaskElementDTO dto)
        {
            LaborTaskDataModelView toReturn = new LaborTaskDataModelView()
            {
                TaskElementData = new TaskElementDetailModelView(dto),
                TaskCustomFields = this.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.Task),
                LaborCustomFields = this.GetCustomFieldOptionModelViews(ws, ControllerCustomFieldType.LaborTypes),
                MOQTypes = boe.MoqTypeSelections.Where(x => x.TaskId == dto.Id).ToList()
            };

            if (dto.CustomFieldValueContainers != null)
            {
                foreach (CustomFieldValueContainer value in dto.CustomFieldValueContainers)
                {
                    toReturn.TaskElementData.CustomFieldValues.Add(new CustomFieldSelectionModelView() {
                        CustomFieldValueID = value.CustomFieldValueID,
                        SelectionID = value.ContainerID,
                        UpdateDate = value.UpdateDate,
                        CustomFieldID = value.CustomFieldID,
                        IsOpenEnded = value.IsOpenEnded,
                        OpenEndedValue = value.OpenEndedValue
                    });
                }
            }

            HashSet<ResourceDTO> resourcesFromDb = new HashSet<ResourceDTO>(this._ResourceLoader.GetByIds(dto.taskElementLabors.Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value).Distinct().ToList()));
            HashSet<PerformingOrgDTO> performingOrgsFromDb = new HashSet<PerformingOrgDTO>(this.PerfOrgLoader.GetByIds(dto.taskElementLabors.Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value).Distinct().ToList()));

            foreach (ResourceTypeDto labor in dto.taskElementLabors)
            {
                ResourceDTO resource = new ResourceDTO();
                if(labor.ResourceID != null)
                {
                    resource = resourcesFromDb.First(x => x.Id == labor.ResourceID.Value);
                }

                PerformingOrgDTO perfOrg = new PerformingOrgDTO();
                if (labor.PerformingOrgID != null)
                {
                    perfOrg = performingOrgsFromDb.First(x => x.Id == labor.PerformingOrgID.Value);
                }

                LaborTypeDataModelView laborToAdd = new LaborTypeDataModelView(labor, resource, perfOrg);

                ICollection<CustomFieldSelectionModelView> laborCustomFieldSelection = new Collection<CustomFieldSelectionModelView>();
                ICollection<CustomFieldValueContainer> laborCustomFieldValues = labor.CustomFieldValueContainers;

                if(laborCustomFieldValues != null)
                {
                    foreach(CustomFieldValueContainer value in laborCustomFieldValues)
                    {
                        laborCustomFieldSelection.Add(new CustomFieldSelectionModelView()
                        {
                            CustomFieldValueID = value.CustomFieldValueID,
                            SelectionID = value.ContainerID,
                            UpdateDate = value.UpdateDate,
                            CustomFieldID = value.CustomFieldID,
                            IsOpenEnded = value.IsOpenEnded,
                            OpenEndedValue = value.OpenEndedValue
                        });
                    }
                    laborToAdd.CustomFieldValues = laborCustomFieldSelection;
                }

                toReturn.LaborTypesData.Add(laborToAdd);
            }
            
            toReturn.ContainsDiscrete = toReturn.LaborTypesData.Select(x => x.SpreadCurveID).Any(x => x.Value == SpreadCurves.DiscreteCost || x.Value == SpreadCurves.DiscreteHours);

            return toReturn;
        }

        /// <summary>
        /// Converts Labor Task Data Model View to BOE Task Element DTO 
        /// </summary>
        /// <param name="modelview">Labor Task Data Model View  to convert</param>
        /// <param name="ws">The Workspace.</param>
        /// <returns>Converted BOE Task Element DTO</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public BoeTaskElementDTO ConvertModelViewToDto(LaborTaskDataModelView modelview, FullWorkspace ws)
        {
            if (modelview == null)
            {
                throw new ArgumentNullException(nameof(modelview));
            }

            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            IDictionary<int, MOQTypeModelView> moqTypes = this.CommonDataMapper.getMOQTypeDictionary();

            BoeTaskElementDTO toReturn = new BoeTaskElementDTO
            {
                Updateable = UpdateType.Upsert
            };

            
            // Convert Task Element Data
            toReturn.BoeID = modelview.TaskElementData.BOEID;
            toReturn.Id = modelview.TaskElementData.TaskElementDetailID ?? -1;
            toReturn.BOETaskID = modelview.TaskElementData.TaskID;
            toReturn.TaskTitle = modelview.TaskElementData.Title;
            toReturn.Description = modelview.TaskElementData.TaskDescription;
            toReturn.MOQType = modelview.TaskElementData.MOQType;
            toReturn.MOQTypeName = modelview.TaskElementData.MOQType == MOQType.None ? null : moqTypes[(int)modelview.TaskElementData.MOQType].MOQTypeName;
            toReturn.MOQText = modelview.TaskElementData.MOQText;
            toReturn.UpdateDate = modelview.TaskElementData.UpdateDate;
            toReturn.LaborTypeWarningFlag = modelview.TaskElementData.LaborTypeWarning;            
            toReturn.WorkspaceVariableIDs = modelview.TaskElementData.WorkspaceVariableIDs;
            toReturn.TaskElementType = TaskElementType.Labor;
            toReturn.BOETaskElementOrder = modelview.TaskElementData.BOETaskElementOrder;
            //if the taskelement is new we will save the order id with 2000. This is so the taskelement always goes to the bottom of the page.
            if (toReturn.Id < 0)
            {
                toReturn.BOETaskElementOrder = 2000;
            }
            // Get Workspace variables being used by this task element
            List<WorkspaceVariableDTO> inUseWorkspaceVariables = ws.WorkspaceVariables.Where(i => toReturn.WorkspaceVariableIDs.Contains(i.Id)).ToList();

            toReturn.MOQHoursEquation = ActionLogic.Common.MOQ.Parser.TagVariables(
                            modelview.TaskElementData.MOQHoursEquation,
                            inUseWorkspaceVariables);

            // Convert the Ordinary Variables
            foreach (BoeTaskOrdinaryVariableModelView variable in modelview.TaskElementData.TaskOrdinaryVariables)
            {
                // new variable
                if (variable.OrdinaryVariableID < 0)
                {
                    toReturn.OrdinaryVariables.Add(variable.GetAssociatedDTO());
                }
                else // existing variable
                {
                    BoeTaskElementDTO taskElement = this.factory.CreateTaskElement(toReturn.Id, ws.DecimalPrecision, ws.CostDecimalPrecision);
                    OrdinaryVariableDto boeTaskOrdinaryVariableDTO = (from v in taskElement.OrdinaryVariables
                                                                      where v.Id == variable.OrdinaryVariableID
                                                                      select v).FirstOrDefault();

                    DataRelationshipVerifier.VerifyDataRelation(boeTaskOrdinaryVariableDTO, toReturn.BoeID);

                    OrdinaryVariableDto updatedVar = variable.GetAssociatedDTO(boeTaskOrdinaryVariableDTO);


                    toReturn.OrdinaryVariables.Add(updatedVar);
                }
            }

            // Convert start/end dates of Task Element Data
            toReturn.StartDate = modelview.TaskElementData.StartDate.ToDateTimeMidMonth();
            toReturn.EndDate = modelview.TaskElementData.EndDate.ToDateTimeMidMonth();

            // Ids for new custom fields on task and labors.
            int newTaskCustomFieldId = 0;
            int newLaborCustomFieldId = 0;

            // Convert Custom Fields
            foreach (CustomFieldSelectionModelView cf in modelview.TaskElementData.CustomFieldValues)
            {
                UpdateType typeOfUpdate;
                if (cf.CustomFieldValueID != -1 || (cf.IsOpenEnded && !string.IsNullOrEmpty(cf.OpenEndedValue)))
                {
                    typeOfUpdate = UpdateType.Upsert;
                }
                else
                {
                    typeOfUpdate = UpdateType.Deleted;
                }

                toReturn.CustomFieldValueContainers.Add(new CustomFieldValueContainer()
                {
                    Id = cf.SelectionID < 0 ? --newTaskCustomFieldId : cf.SelectionID,
                    CustomFieldValueID = cf.CustomFieldValueID,
                    ContainerID = cf.SelectionID < 0 ? --newTaskCustomFieldId : cf.SelectionID,
                    CustomFieldID = cf.CustomFieldID,
                    IsOpenEnded = cf.IsOpenEnded,
                    OpenEndedValue = cf.OpenEndedValue,
                    UpdateDate = cf.UpdateDate,
                    Updateable = typeOfUpdate
                });
            }

            // Convert Labor Types
            foreach (LaborTypeDataModelView labor in modelview.LaborTypesData.Where(x => !x.Deleted || (x.Deleted && x.BOELaborTypeID.HasValue && x.BOELaborTypeID.Value > 0)))
            {
                ResourceTypeDto resourceToAdd = new ResourceTypeDto
                {
                    Id = labor.BOELaborTypeID ?? -1
                };

                if (labor.Deleted)
                {
                    resourceToAdd.UpdateDate = labor.UpdateDate;
                    resourceToAdd.Updateable = UpdateType.Deleted;
                }
                else
                {
                    resourceToAdd.ResourceID = labor.ResourceID;
                    resourceToAdd.PerformingOrgID = labor.PerformingOrgID;
                    resourceToAdd.SpreadCurveID = labor.SpreadCurveID;
                    resourceToAdd.PercentSpread = labor.PercentSpread;
                    resourceToAdd.CanOffload = labor.CanOffload;
                    resourceToAdd.TieredPercentage = labor.TieredPercentage;
                    resourceToAdd.SpreadType = labor.RateType == RateType.Hours ? SpreadType.Hours : SpreadType.Cost;
                    resourceToAdd.BoeID = modelview.TaskElementData.BOEID;
                    resourceToAdd.ValueSpread = labor.RateType == RateType.Hours ? labor.HourSpread : labor.CostSpread;
                    resourceToAdd.UpdateDate = labor.UpdateDate;
                    if (labor.SpreadCurveID == SpreadCurves.DiscreteHours)
                    {
                        resourceToAdd.HourSpreadLocked = false;
                        resourceToAdd.PercentSpreadLocked = false;
                    }
                    else
                    {
                        resourceToAdd.HourSpreadLocked = labor.HourSpreadLocked;
                        resourceToAdd.PercentSpreadLocked = labor.PercentSpreadLocked;
                    }
                    resourceToAdd.WBSID = labor.WBSID > (int?)0 ? labor.WBSID : null;
                    resourceToAdd.CLINID = labor.CLINID > (int?)0 ? labor.CLINID : null;
                    resourceToAdd.LaborTypeOrder = labor.LaborTypeOrder;
                    resourceToAdd.Updateable = UpdateType.Upsert;

                    // Convert start/end date
                    if (!string.IsNullOrWhiteSpace(labor.StartDate))
                    {
                        try
                        {
                            resourceToAdd.StartDateValue = labor.StartDate.ToDateTimeMidMonth();
                        }
                        catch (FormatException)
                        {
                            // do nothing, this validation message is caught and added elsewhere (MVC)
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(labor.EndDate))
                    {
                        try
                        {
                            resourceToAdd.EndDateValue = labor.EndDate.ToDateTimeMidMonth();
                        }
                        catch (FormatException)
                        {
                            // do nothing, this validation message is caught and added elsewhere (MVC)
                        }
                    }

                    // Convert Custom Fields
                    foreach (CustomFieldSelectionModelView cf in labor.CustomFieldValues)
                    {
                        UpdateType typeOfUpdate;
                        if (cf.CustomFieldValueID > 0 || (cf.IsOpenEnded && !string.IsNullOrEmpty(cf.OpenEndedValue)))
                        {
                            typeOfUpdate = UpdateType.Upsert;
                        }
                        else
                        {
                            typeOfUpdate = UpdateType.Deleted;
                        }

                        resourceToAdd.CustomFieldValueContainers.Add(new CustomFieldValueContainer()
                        {
                            Id = cf.SelectionID < 0 ? --newLaborCustomFieldId : cf.SelectionID,
                            CustomFieldValueID = cf.CustomFieldValueID,
                            ContainerID = cf.SelectionID < 0 ? --newLaborCustomFieldId : cf.SelectionID,
                            CustomFieldID = cf.CustomFieldID,
                            IsOpenEnded = cf.IsOpenEnded,
                            OpenEndedValue = cf.OpenEndedValue,
                            UpdateDate = cf.UpdateDate,
                            Updateable = typeOfUpdate
                        });
                    }

                    // Convert Spreads
                    int newLaborSpreadId = 0;
                    if (labor.Spreads != null && labor.Spreads.Any())
                    {
                        resourceToAdd.LaborSpreads = labor.Spreads.Select(x => new ResourceSpreadDto()
                        {
                            Id = --newLaborSpreadId,
                            LaborSpreadDate = x.LaborSpreadDate.ToDateTimeMidMonth(),
                            LaborSpreadValue = x.LaborSpreadValue ?? 0,
                            BoeID = modelview.TaskElementData.BOEID,
                            UpdateDateLong = x.UpdateDateLong,
                            Updateable = UpdateType.Upsert
                        }).ToCollection();
                    }
                }

                toReturn.taskElementLabors.Add(resourceToAdd);
            }

            // this is needed to determine whether we need to run sum of boes calculations against entire workspace
            toReturn.TotalCost = toReturn.taskElementLabors.Where(tl => tl.SpreadType == SpreadType.Cost).Sum(l => l.ValueSpread);
            toReturn.TotalHours = toReturn.taskElementLabors.Where(tl => tl.SpreadType == SpreadType.Hours).Sum(l => l.ValueSpread);

            return toReturn;
        }

        /// <s/// <summary>
        /// Execute a recalculation of the labor tab data following a change to the MOQ equation on the UI.
        /// </summary>
        /// <param name="workspaceData">Workspace</param>
        /// <param name="laborTabData">Contents of the labor tab page that are needed for the recalculation</param>
        /// <param name="results">Set of changes that need to be applied to the UI as a result of the recalculation</param>
        private void RecalculateAfterMOQEquationChange(FullWorkspace workspaceData, LaborTabDataModelView laborTabData, ICollection<SpreadValueTableChanges> changes)
        {
            decimal moqTotalHours = laborTabData.MOQEquationTotal ?? 0;

            #region First pass: Initial allocation of hours to each resource entry

            decimal overallHoursSpread = 0m;

            ICollection<LaborTypeDataModelView> activeResourceEntries = laborTabData.LaborTypesData.GetNonDeletedResourceEntries();
            foreach (LaborTypeDataModelView resourceTypeData in activeResourceEntries)
            {
                SpreadCurves? spreadCurve = resourceTypeData.SpreadCurveID;

                decimal hourSpread = resourceTypeData.HourSpread ?? 0;

                if (resourceTypeData.RateType == RateType.Cost)
                {
                    // percent, hours and cost spread values stay the same for cost spreads
                }
                else if (spreadCurve.HasValue && spreadCurve.Value == SpreadCurves.DiscreteHours)
                {
                    // hours and cost spread values stay the same for discrete hours spreads, but percent changes
                    resourceTypeData.PercentSpread = (moqTotalHours == 0) ? 0m : decimal.Round(Convert.ToDecimal((100m * hourSpread) / moqTotalHours), 3, MidpointRounding.AwayFromZero);
                }
                else if (resourceTypeData.PercentSpreadLocked)
                {
                    decimal percentSpread = resourceTypeData.PercentSpread ?? 0;

                    // assign an integer value for both decimal variables
                    decimal hoursSpreadValue = (percentSpread == 0) ? 0 : Utilities.AdjustPrecision((moqTotalHours * percentSpread / 100), workspaceData.DecimalPrecision);
                    resourceTypeData.HourSpread = hoursSpreadValue;
                    hourSpread = hoursSpreadValue;
                }
                else
                {
                    resourceTypeData.PercentSpread = (moqTotalHours == 0) ? 0m : decimal.Round(Convert.ToDecimal((100m * hourSpread) / moqTotalHours), 3, MidpointRounding.AwayFromZero);
                }

                overallHoursSpread += hourSpread;
            }

            #endregion

            #region Second pass: Recalculate cost spread values

            foreach (LaborTypeDataModelView resourceTypeData in activeResourceEntries)
            {
                SpreadCurves? spreadCurve = resourceTypeData.SpreadCurveID;

                if (resourceTypeData.RateType == RateType.Cost || (spreadCurve.HasValue && spreadCurve.Value == SpreadCurves.DiscreteHours))
                {
                    // cost spread values stay the same cost type resources and discrete hours spreads
                }
                else
                {
                    int laborTypeId = resourceTypeData.BOELaborTypeID ?? -1;

                    if (resourceTypeData.PercentSpreadLocked)
                    {
                        // hours changed - need to recalculate spread distribution
                        decimal hourSpreadValue = resourceTypeData.HourSpread.HasValue ? Utilities.AdjustPrecision(resourceTypeData.HourSpread.Value, workspaceData.DecimalPrecision) : 0;
                        this.ApplySpreadValueUpdates(hourSpreadValue, laborTabData, resourceTypeData, laborTypeId, changes, workspaceData.DecimalPrecision);
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Update the spreads (Cost or Hours) for the designated Resource Types table row.
        /// </summary>
        /// <param name="spreadValue">Value to be spread.</param>
        /// <param name="laborTabData">Contents of the labor tab page that are needed for the recalculation</param>
        /// <param name="resourceTypeRowData">The resource-type entry related to the spreads.</param>
        /// <param name="laborTypeId">ID for the resource-type entry.</param>
        /// <param name="results">Set of changes that need to be applied to the UI as a result of the recalculation</param>
        /// <param name="decimalPrecision">The number of decimal places to use for spread values.</param>
        private void ApplySpreadValueUpdates(decimal spreadValue, LaborTabDataModelView laborTabData, LaborTypeDataModelView resourceTypeRowData, int laborTypeId, ICollection<SpreadValueTableChanges> changes, int decimalPrecision)
        {
            if (resourceTypeRowData.SpreadCurveID.HasValue)
            {
                DateTime dtStartDate;
                DateTime dtEndDate;
                if (!DateTime.TryParse(resourceTypeRowData.StartDate, out dtStartDate))
                {
                    dtStartDate = laborTabData.TaskStartDate.Value;
                }
                if (!DateTime.TryParse(resourceTypeRowData.EndDate, out dtEndDate))
                {
                    dtEndDate = laborTabData.TaskEndDate.Value;
                }

                // recalculate the hourly or cost spread values against the designated curve
                Collection<ResourceSpreadDto> newLaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(new LaborSpreadRequest()
                {
                    CurveID = resourceTypeRowData.SpreadCurveID.Value,
                    HourSpread = spreadValue,
                    StartDate = dtStartDate.Normalize(DateTimePrecision.Month),
                    EndDate = dtEndDate.Normalize(DateTimePrecision.Month)
                }, decimalPrecision);

                this.ApplyRecalculatedSpreadValues(newLaborSpreads, laborTabData, resourceTypeRowData, laborTypeId, changes);
            }
        }

        /// <summary>
        /// Recalculate (but do not save) the hourly values for a task element, its resources and their spreads after a change to the total MOQ hours.
        /// </summary>
        /// <param name="taskElement">Task element</param>
        /// <param name="boeFull">BOE containing the task element</param>
        /// <param name="workspace">Workspace containing the task element</param>
        /// <returns>The recalculated task element</returns>
        private BoeTaskElementDTO RecalculateTaskElementHourValues(BoeTaskElementDTO taskElement, FullBoe boeFull, FullWorkspace workspace)
        {
            if (taskElement == null)
            {
                throw new ArgumentNullException(nameof(taskElement));
            }

            if (boeFull == null)
            {
                throw new ArgumentNullException(nameof(boeFull));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            bool taskWasChanged = false;

            decimal totalHoursBefore = taskElement.TotalHours ?? 0m;

            // recalculate the MOQ equation
            string recalculatedMoqHours = this._BoeTaskElementRecalculation.CalculateMOQHoursTotal(taskElement, workspace);
            decimal moqHoursTotal;
            decimal moqEquationTotal;
            if (decimal.TryParse(recalculatedMoqHours, out moqHoursTotal))
            {
                taskElement.TotalHours = moqEquationTotal = Utilities.AdjustPrecision(moqHoursTotal, workspace.DecimalPrecision);
            }
            else
            {
                moqEquationTotal = taskElement.TotalHours ?? 0;
            }

            // labor types
            LaborTaskDataModelView laborTypesViewModelData = this.GetLaborTaskData(workspace, boeFull, taskElement.Id);
            ICollection<LaborTypeDataModelView> laborTypesData = laborTypesViewModelData.LaborTypesData;

            // labor spreads
            // table mapping each resource entry (LaborTypeId) to its ORIGINAL spread data (month/value)
            IDictionary<int, IDictionary<string, decimal?>> laborSpreadDataOriginal = new Dictionary<int, IDictionary<string, decimal?>>();
            foreach (LaborTypeDataModelView resourceEntry in laborTypesViewModelData.LaborTypesData)
            {
                IDictionary<string, decimal?> spreadsOriginal = new Dictionary<string, decimal?>();

                // adjust values to zero (to avoid delta analysis and force results to include ALL spread recalculations)
                foreach (LaborSpreadDataModelView spread in resourceEntry.Spreads)
                {
                    spreadsOriginal[spread.LaborSpreadDate] = spread.LaborSpreadValue;  // save original value before zero-ing out
                    
                    spread.LaborSpreadValue = 0m;
                }

                laborSpreadDataOriginal[resourceEntry.BOELaborTypeID.Value] = spreadsOriginal;
            }

            LaborTabDataModelView laborTabData = new LaborTabDataModelView
            {
                MOQEquation = taskElement.MOQHoursEquation,
                MOQEquationTotal = moqEquationTotal,
                TaskStartDate = taskElement.StartDate,
                TaskEndDate = taskElement.EndDate,
                LaborTypesData = laborTypesData
            };

            // call the recalculation logic
            ICollection<SpreadValueTableChanges> recalcResults = this.RecalculateSpreads(workspace, laborTabData);

            if (recalcResults != null && recalcResults.Any())
            {
                taskWasChanged = true;
            }

            // apply resource type total hours adjustments
            foreach (ResourceTypeDto resourceEntry in taskElement.taskElementLabors)
            {
                if (!resourceEntry.SpreadCurveID.HasValue || resourceEntry.SpreadCurveID.Value == SpreadCurves.DiscreteCost || resourceEntry.SpreadCurveID.Value == SpreadCurves.DiscreteHours)
                {
                    resourceEntry.Updateable = UpdateType.None;
                }
                else // curve-based spread
                {
                    // find the set of spreads for this resource
                    LaborTypeDataModelView mvResourceSpreadData = laborTypesData.FirstOrDefault(s => s.BOELaborTypeID == resourceEntry.Id);

                    // convert all spread view models to DTOs
                    Collection<ResourceSpreadDto> resourceSpreads = new Collection<ResourceSpreadDto>();
                    foreach (LaborSpreadDataModelView mvSpread in mvResourceSpreadData.Spreads)
                    {
                        ResourceSpreadDto spreadDto = mvSpread.ToBoeLaborSpread(resourceEntry.Id);
                        spreadDto.Updateable = UpdateType.Upsert;
                        resourceSpreads.Add(spreadDto);
                    }

                    // attach the spread DTOs to the resource entry DTO
                    resourceEntry.LaborSpreads = resourceSpreads;

                    // calculate/apply the new hour total for the resource entry
                    decimal totalHourSpread = resourceSpreads.Sum(s => s.LaborSpreadValue);
                    decimal afterValue = Utilities.AdjustPrecision(totalHourSpread, workspace.DecimalPrecision);

                    decimal beforeValue = resourceEntry.ValueSpread.GetValueOrDefault(0m);

                    if (resourceEntry.HourSpreadLocked)
                    {
                        // If the hours were locked, they should not be changed..
                        resourceEntry.ValueSpread = beforeValue;
                    }
                    else
                    {
                        resourceEntry.ValueSpread = afterValue;
                    }

                    resourceEntry.Updateable = UpdateType.Upsert;

                    if (beforeValue != afterValue)
                    {
                        taskWasChanged = true;
                    }
                }
            }

            decimal totalHoursAfter = taskElement.TotalHours ?? 0L;
            if (taskWasChanged || (totalHoursBefore != totalHoursAfter))
            {
                // If the recalculation caused the task's labor hour allocations to change, then mark the task element and the corresponding BOE for update
                taskElement.Updateable = UpdateType.Upsert;
                boeFull.Updateable = UpdateType.Upsert;
            }

            // now check to make sure total hours after equals MOQ
            // Copied from BoeLaborControllerLogic.AdjustDeltaHours
            // if the total Percent Spread for this element is 100, but there is still a delta, need to change the hours spread for the last non-discrete labor type
            // so we can achieve 100% Percent Spread with no delta
            
            // only adjust labor types that aren't discrete and percent spread locked is locked
            ICollection<ResourceTypeDto> eligibleResourceEntries = taskElement.taskElementLabors.Where(r => r.SpreadCurveID != SpreadCurves.DiscreteHours && r.SpreadType != SpreadType.Cost && r.PercentSpreadLocked).ToList();

            decimal totalSpreadPercentage = eligibleResourceEntries.Sum(r => r.PercentSpread.HasValue ? r.PercentSpread.Value : 0);
            decimal totalResourceHours = eligibleResourceEntries.Sum(r => r.ValueSpread.HasValue ? r.ValueSpread.Value : 0);
            decimal deltaHours = Utilities.AdjustPrecision(totalResourceHours, workspace.DecimalPrecision) - moqEquationTotal;
            if (totalSpreadPercentage == 100 && deltaHours != 0)
            {
                taskElement.Updateable = UpdateType.Upsert;
                boeFull.Updateable = UpdateType.Upsert;

                decimal adjustment = (deltaHours < 0) ? Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(workspace.DecimalPrecision) : -1 * Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(workspace.DecimalPrecision);  // adjust either up or down

                while (deltaHours != 0)
                {
                    foreach (ResourceTypeDto resourceEntry in eligibleResourceEntries)
                    {
                        if (deltaHours == 0)
                        {
                            break;
                        }

                        // apply adjustment to the current entry
                        decimal resourceHours = resourceEntry.ValueSpread.HasValue ? Utilities.AdjustPrecision(resourceEntry.ValueSpread.Value, workspace.DecimalPrecision) : 0;
                        resourceEntry.ValueSpread = resourceHours + adjustment;
                        resourceEntry.Updateable = UpdateType.Upsert;

                        // get labor spread request
                        LaborSpreadRequest laborSpreadRequest = new LaborSpreadRequest();
                        laborSpreadRequest.CurveID = resourceEntry.SpreadCurveID;
                        laborSpreadRequest.StartDate = (DateTime)resourceEntry.StartDate;
                        laborSpreadRequest.EndDate = (DateTime)resourceEntry.EndDate;
                        laborSpreadRequest.HourSpread = resourceEntry.ValueSpread.HasValue ? resourceEntry.ValueSpread.Value : 0;

                        // Recalculate Labor Spread
                        // add new labor spread to labor type.
                        resourceEntry.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(laborSpreadRequest, workspace.DecimalPrecision);

                        //Update UpdateType property
                        foreach (ResourceSpreadDto laborSpread in resourceEntry.LaborSpreads)
                        {
                            laborSpread.Updateable = UpdateType.Upsert;
                        }

                        // update the delta
                        deltaHours += adjustment;
                    }
                }
            }

            return taskElement;
        }

        /// <summary>
        /// Apply the new spread distribution to the resource being edited.
        /// </summary>
        /// <param name="newLaborSpreads">The new spread distribution</param>
        /// <param name="workspaceData">Workspace</param>
        /// <param name="boeId">BOE</param>
        /// <param name="laborTabData">Contents of the labor tab page that are needed for the recalculation</param>
        /// <param name="resourceTypeRowData">The resource-type entry being deleted</param>
        /// <param name="laborTypeId">ID for the resource-type entry being deleted</param>
        /// <param name="results">Set of changes that need to be applied to the UI as a result of the recalculation</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void ApplyRecalculatedSpreadValues(Collection<ResourceSpreadDto> newLaborSpreads, LaborTabDataModelView laborTabData, LaborTypeDataModelView resourceTypeRowData, int laborTypeId, ICollection<SpreadValueTableChanges> changes)
        {
            if (newLaborSpreads != null)
            {
                LaborTypeDataModelView spreadData = laborTabData.LaborTypesData.FirstOrDefault(s => s.BOELaborTypeID == laborTypeId);
                ICollection<LaborSpreadDataModelView> existingLaborSpreads = (spreadData == null) ? new List<LaborSpreadDataModelView>() : spreadData.Spreads;

                /*
                 * This is analogous to doing an "outer join" of sorts:
                 * 
                 * We want to make sure we account for months that may have been added (before and/or after the current endpoints).
                 * We also want to make sure we account for deletions (i.e. if the resource type's spread date range was shortened).
                 * 
                 */

                // need to loop against the FULL date spread (i.e. of ALL resource entries)
                ICollection<DateTime> spreadDatesFull = existingLaborSpreads.Any(s => s.LaborSpreadDate != null) ? existingLaborSpreads.GetSpreadDatesFull() : new Collection<DateTime>();

                #region Adjust start/end dates to include all (new AND existing) entries

                DateTime spreadStartDate = newLaborSpreads.Select(s => s.LaborSpreadDate).Min();
                DateTime spreadEndDate = newLaborSpreads.Select(s => s.LaborSpreadDate).Max();

                if (spreadDatesFull.Any() && spreadStartDate != null && spreadDatesFull.Min() < spreadStartDate)
                {
                    spreadStartDate = spreadDatesFull.Min();
                }

                if (spreadDatesFull.Any() && spreadEndDate != null && spreadDatesFull.Max() > spreadEndDate)
                {
                    spreadEndDate = spreadDatesFull.Max();
                }

                ICollection<DateTime> spreadDates = new List<DateTime>();

                // display the header (all dates across the spread)
                for (DateTime dt = spreadStartDate.Date; dt.Date <= spreadEndDate; dt = dt.AddMonths(1))
                {
                    spreadDates.Add(dt.Normalize());
                }

                spreadDatesFull = spreadDates;

                decimal newHoursTotal = 0m;
                decimal previousHoursTotal = 0m;
                #endregion

                foreach (DateTime spreadMonth in spreadDatesFull)
                {
                    LaborSpreadDataModelView existingSpreadEntry = existingLaborSpreads.FirstOrDefault(s => s.LaborSpreadDate == spreadMonth.ToMonthString());
                    ResourceSpreadDto newSpreadEntry = newLaborSpreads.FirstOrDefault(s => s.LaborSpreadDate == spreadMonth);

                    if (existingSpreadEntry == null && newSpreadEntry == null)  // neither spread distribution contains an entry for this month 
                    {
                        // skip - no changes
                    }
                    else if (existingSpreadEntry == null)  // month is being added to this spread
                    {
                        SpreadValueTableChanges svtChanges = changes.GetOrCreateSpreadValueTableChanges(laborTypeId, spreadMonth);

                        svtChanges.Copy(resourceTypeRowData, newSpreadEntry);

                        newHoursTotal += newSpreadEntry.LaborSpreadValue;
                    }
                    else if (newSpreadEntry == null)  // month is being deleted from this spread
                    {
                        previousHoursTotal += existingSpreadEntry.LaborSpreadValue.GetValueOrDefault(0m);
                        
                        SpreadValueTableChanges svtChanges = changes.GetOrCreateSpreadValueTableChanges(laborTypeId, spreadMonth);
                        svtChanges.Delete = true;
                        svtChanges.SpreadValue = 0m;

                        existingSpreadEntry.LaborSpreadValue = 0m;
                    }
                    else  // compare both
                    {
                        decimal previousSpreadValue = existingSpreadEntry.LaborSpreadValue.GetValueOrDefault(0m);
                        decimal newSpreadValue = newSpreadEntry.LaborSpreadValue;

                        newHoursTotal += newSpreadValue;
                        previousHoursTotal += previousSpreadValue;

                        // determine before/after delta of the spread value for this month
                        decimal delta = newSpreadValue - previousSpreadValue;

                        if (delta != 0m)  // if the spread value changed
                        {
                            // report changes to the UI - update spread value
                            SpreadValueTableChanges svtChanges = changes.GetOrCreateSpreadValueTableChanges(laborTypeId, spreadMonth);
                            svtChanges.SpreadValue = newSpreadValue;

                            // apply the recalculated hours back into the current list of spread values
                            existingSpreadEntry.LaborSpreadValue = newSpreadValue;
                        }
                    }
                }
            }
        }
        #region Task variable recalculation

        /// <summary>
        /// Build a list of dependencies (in order of how they should be evaluated/saved)
        /// </summary>
        /// <param name="triggeringBoeID">BOE</param>
        /// <param name="dependencies">Dependencies</param>
        /// <returns>Order list of dependencies (in save order)</returns>
        private ICollection<VariableDependencyNode> ResolveVariableUpdateOrder(int triggeringBoeID, VariableDependencyResolutionData dependencies)
        {
            DirectedGraph<VariableDependencyNode> graph = new DirectedGraph<VariableDependencyNode>();

            IList<VariableDependencyNode> vertices = new List<VariableDependencyNode>();

            // create a vertex for the current BOE
            VariableDependencyNode triggeringBoeVertex = this.GetOrCreateTaskVariableDependencyNode(VertexType.BOE, triggeringBoeID, vertices);

            this.DoResolveTaskVariableUpdateOrder(triggeringBoeVertex, dependencies, graph, vertices);

            return graph.TopologicalSort();
        }

        /// <summary>
        /// Get or create a dependency node for the graph
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="ID">Entity ID</param>
        /// <param name="vertices">Collection of vertices (to prevent duplicates)</param>
        /// <returns>Dependency node</returns>
        private VariableDependencyNode GetOrCreateTaskVariableDependencyNode(VertexType vertexType, int ID, IList<VariableDependencyNode> vertices)
        {
            VariableDependencyNode vertex;

            if ((vertex = vertices.FirstOrDefault(v => v.Vertex == vertexType && v.ID == ID)) == null)
            {
                vertex = new VariableDependencyNode
                {
                    Vertex = vertexType,
                    ID = ID
                };

                vertices.Add(vertex);
            }

            return vertex;
        }

        /// <summary>
        /// Recursive worker method
        /// </summary>
        /// <param name="vertex">Vertex</param>
        /// <param name="dependencies">Dependencies</param>
        /// <param name="graph">Directed graph</param>
        /// <param name="vertices">Collection of vertices (to prevent duplicates)</param>
        private void DoResolveTaskVariableUpdateOrder(VariableDependencyNode vertex, VariableDependencyResolutionData dependencies, DirectedGraph<VariableDependencyNode> graph, IList<VariableDependencyNode> vertices)
        {
            if (vertex.Vertex == VertexType.BOE)
            {
                int boeID = vertex.ID;

                // find all sum-of-BOE task variables that depend on this BOE
                ICollection<VariableDependencyData> taskVariables = this.GetSumOfBoeTaskVariables(boeID, dependencies);

                // add a transition (edge) for each
                foreach (VariableDependencyData taskVariableData in taskVariables)
                {
                    //TaskVariableDependencyNode taskVariableVertex = this.GetOrCreateTaskVariableDependencyNode(TaskVariableDependencyNode.VertexType.TaskVariable, taskVariableData.TaskVariable.OrdinaryVariableName, vertices);
                    VariableDependencyNode taskVariableVertex = this.GetOrCreateTaskVariableDependencyNode(VertexType.Variable, taskVariableData.Variable.Id, vertices);
                    graph.AddEdge(vertex, taskVariableVertex);
                    this.DoResolveTaskVariableUpdateOrder(taskVariableVertex, dependencies, graph, vertices);
                }
            }
            else if (vertex.Vertex == VertexType.TaskElement)
            {
                TaskElementDependencyData dependencyData = dependencies.TaskElements.First(v => v.TaskElementID == vertex.ID);

                // find the BOE that owns this task element
                VariableDependencyNode boeVertex = this.GetOrCreateTaskVariableDependencyNode(VertexType.BOE, dependencyData.BoeID, vertices);
                graph.AddEdge(vertex, boeVertex);
                this.DoResolveTaskVariableUpdateOrder(boeVertex, dependencies, graph, vertices);
            }
            else if (vertex.Vertex == VertexType.Variable)
            {
                VariableDependencyData dependencyData = dependencies.Variables.First(v => v.Variable.Id == vertex.ID);

                foreach (int taskElementId in dependencyData.Variable.TaskElementIds)
                {
                    // find the task element that owns/uses this task variable in its MOQ equation
                    VariableDependencyNode taskElementVertex = this.GetOrCreateTaskVariableDependencyNode(VertexType.TaskElement, taskElementId, vertices);
                    graph.AddEdge(vertex, taskElementVertex);
                    this.DoResolveTaskVariableUpdateOrder(taskElementVertex, dependencies, graph, vertices);
                }
            }
        }

        /// <summary>
        /// Find all sum-of-BOE task variables that depend on a BOE
        /// </summary>
        /// <param name="boeID">BOE</param>
        /// <param name="dependencies">Dependencies</param>
        /// <returns>Task variables</returns>
        private ICollection<VariableDependencyData> GetSumOfBoeTaskVariables(int boeID, VariableDependencyResolutionData dependencies)
        {
            ICollection<VariableDependencyData> directlyLinkedVariables = dependencies.Variables.Where(v => v.Variable.SelectedBOEsToSum.Any(b => b.BoeID == boeID)).ToList();
            ICollection<VariableDependencyData> indirectlyLinkedVariables = dependencies.Variables.Where(v => v.Variable.SelectedBOEsToSum.Any(b => b.ChildBoeIDs.Contains(boeID))).ToList();

            IList<VariableDependencyData> resultingVariables = new List<VariableDependencyData>(directlyLinkedVariables);

            // merge the two lists ...
            foreach (VariableDependencyData indirectVar in indirectlyLinkedVariables)
            {
                if (!directlyLinkedVariables.Any(v => v.Variable.Id == indirectVar.Variable.Id))  // ... but do not add duplicates
                {
                    resultingVariables.Add(indirectVar);
                }
            }

            return resultingVariables;
        }

        /// <summary>
        /// Determine the full dependency-chain of task elements, task variables and workspace variables that are based on a BOE
        /// </summary>
        /// <param name="boeID">BOE PK identifier</param>
        /// <param name="workspace">Workspace</param>
        /// <returns>Bottom-up (breadth-first) dependency-chains</returns>
        public VariableDependencyResolutionData ResolveAllVariableDependencies(int boeID, FullWorkspace workspace)
        {
            return this.ResolveVariableDependencies(boeID, workspace, (b, w) =>
            {
                ICollection<OrdinaryVariableDto> referencedTaskVariables = FullWorkspaceHelper.GetTaskVariablesAssociatedWithBoe(b, w);
                ICollection<WorkspaceVariableDTO> referencedWorkspaceVariables = FullWorkspaceHelper.GetWorkspaceVariablesAssociatedWithBoe(b, w);

                List<IVariableDTO> referencedVariablesList = new List<IVariableDTO>();
                referencedVariablesList.AddRange(referencedTaskVariables);
                referencedVariablesList.AddRange(referencedWorkspaceVariables);
                ICollection<IVariableDTO> referencedVariables = referencedVariablesList.ToCollection();

                return referencedVariables;
            });
        }

        /// <summary>
        /// Determine the full dependency-chain of task elements and task variables which are based on a BOE
        /// </summary>
        /// <param name="boeID">BOE PK identifier</param>
        /// <param name="workspace">Workspace</param>
        /// <param name="getReferencedVariables">Function pointer to retrieve variables</param>
        /// <returns>Bottom-up (breadth-first) dependency-chains</returns>
        private VariableDependencyResolutionData ResolveVariableDependencies(int boeID, FullWorkspace workspace, Func<int, FullWorkspace, ICollection<IVariableDTO>> getReferencedVariables)
        {
            // to store results of recursion
            IList<int> dependentTaskElementIds = new List<int>();  // captures breadth-first ordering
            IDictionary<int, TaskElementDependencyData> dependencyData = new Dictionary<int, TaskElementDependencyData>();  // maps each task element ID to its dependencies
            IList<VariableDependencyData> dependentTaskVariables = new List<VariableDependencyData>();  // captures breadth-first ordering

            // resolve the full dependency chain using breadth-first recursion
            this.DoResolveVariableDependencies(boeID, workspace, getReferencedVariables, dependentTaskElementIds, dependencyData, dependentTaskVariables);

            /*
             * 
             * Assemble the final results, in breadth-first order, with duplicates removed
             * 
             */

            // remove duplicates - must be done explicitly (i.e. NOT using LINQ Distinct) in order to preserve original ordering
            IList<int> uniqueDependentTaskElementIds = new List<int>();
            foreach (int id in dependentTaskElementIds)
            {
                if (!uniqueDependentTaskElementIds.Contains(id))
                {
                    uniqueDependentTaskElementIds.Add(id);
                }
            }

            // use ordered ID list to assemble the (ordered) list of dependencies
            List<TaskElementDependencyData> orderedDependencyData = new List<TaskElementDependencyData>();
            foreach (int id in uniqueDependentTaskElementIds)
            {
                orderedDependencyData.Add(dependencyData[id]);
            }

            // consolidate duplicate references (so there is only a single object instance of each task/workspace variable in the graph)
            foreach (TaskElementDependencyData taskElementDependency in orderedDependencyData)
            {
                #region Process task variable duplicates

                IDictionary<OrdinaryVariableDto, IVariableDTO> duplicateTaskVariableInstances = new Dictionary<OrdinaryVariableDto, IVariableDTO>();

                foreach (OrdinaryVariableDto taskVariable in taskElementDependency.TaskElement.OrdinaryVariables)
                {
                    VariableDependencyData taskVariableDependencyData;
                    if ((taskVariableDependencyData = dependentTaskVariables.FirstOrDefault(tv => tv.Variable.Id == taskVariable.Id && tv.Variable.VariableType == VariableType.Task)) != null)
                    {
                        duplicateTaskVariableInstances[taskVariable] = taskVariableDependencyData.Variable;
                    }
                }

                foreach (KeyValuePair<OrdinaryVariableDto, IVariableDTO> duplicateEntry in duplicateTaskVariableInstances)
                {
                    // remove the duplicate instance and replace with a reference to the instance from the Variables collection
                    taskElementDependency.TaskElement.OrdinaryVariables.Remove(duplicateEntry.Key);
                    taskElementDependency.TaskElement.OrdinaryVariables.Add(duplicateEntry.Value as OrdinaryVariableDto);
                }

                #endregion

                #region Process workspace variable duplicates (BOE-owned)

                IDictionary<WorkspaceVariableDTO, IVariableDTO> boeDuplicateWorkspaceVariableInstances = new Dictionary<WorkspaceVariableDTO, IVariableDTO>();

                foreach (WorkspaceVariableDTO workspaceVariable in taskElementDependency.BoeFull.WorkspaceVariables)
                {
                    VariableDependencyData workspaceVariableDependencyData;
                    if ((workspaceVariableDependencyData = dependentTaskVariables.FirstOrDefault(tv => tv.Variable.Id == workspaceVariable.Id && tv.Variable.VariableType == VariableType.Workspace)) != null)
                    {
                        boeDuplicateWorkspaceVariableInstances[workspaceVariable] = workspaceVariableDependencyData.Variable;
                    }
                }

                foreach (KeyValuePair<WorkspaceVariableDTO, IVariableDTO> duplicateEntry in boeDuplicateWorkspaceVariableInstances)
                {
                    // In this case, we actually do want to update the Full BOE's ReadOnly Collection to ensure that every Variable instance is the same for each id (same hashcode).
                    // Doing this allows us to later update all of the variables correctly instead of having a variable be updated in the FullWorkspace but not in the FullBoe.

                    // remove the duplicate instance and replace with a reference to the instance from the Variables collection
                    List<WorkspaceVariableDTO> variables = taskElementDependency.BoeFull.WorkspaceVariables.ToList();
                    variables.Remove(duplicateEntry.Key);
                    variables.Add(duplicateEntry.Value as WorkspaceVariableDTO);
                    taskElementDependency.BoeFull.UpdateWorkspaceVariables(variables);
                }

                #endregion

                #region Process workspace variable duplicates (workspace-owned)

                IDictionary<WorkspaceVariableDTO, IVariableDTO> wsDuplicateWorkspaceVariableInstances = new Dictionary<WorkspaceVariableDTO, IVariableDTO>();

                foreach (WorkspaceVariableDTO workspaceVariable in taskElementDependency.WorkspaceFull.WorkspaceVariables)
                {
                    VariableDependencyData workspaceVariableDependencyData;
                    if ((workspaceVariableDependencyData = dependentTaskVariables.FirstOrDefault(tv => tv.Variable.Id == workspaceVariable.Id && tv.Variable.VariableType == VariableType.Workspace)) != null)
                    {
                        wsDuplicateWorkspaceVariableInstances[workspaceVariable] = workspaceVariableDependencyData.Variable;
                    }
                }

                foreach (KeyValuePair<WorkspaceVariableDTO, IVariableDTO> duplicateEntry in wsDuplicateWorkspaceVariableInstances)
                {
                    // In this one case, we actually do want to update the Full Workspace's ReadOnly Collection to ensure that every Variable instance is the same for each id (same hashcode).
                    // Doing this allows us to later update all of the variables correctly instead of having a variable be updated in the FullWorkspace but not in the FullBoe.

                    // remove the duplicate instance and replace with a reference to the instance from the Variables collection
                    List<WorkspaceVariableDTO> variables = taskElementDependency.WorkspaceFull.WorkspaceVariables.ToList();
                    variables.Remove(duplicateEntry.Key);
                    variables.Add(duplicateEntry.Value as WorkspaceVariableDTO);
                    taskElementDependency.WorkspaceFull.UpdateWorkspaceVariables(variables);
                }

                #endregion
            }

            return new VariableDependencyResolutionData
            {
                TaskElements = orderedDependencyData,
                Variables = dependentTaskVariables
            };
        }

        /// <summary>
        /// Recursive worker method to determine the full dependency-chain of task variables which are based on a BOE
        /// </summary>
        /// <param name="boeID">BOE PK identifier</param>
        /// <param name="workspace">Workspace</param>
        /// <param name="getReferencedVariables">Variables</param>
        /// <param name="dependentTaskElementIds">Accumulated results of recursion - task element IDs</param>
        /// <param name="dependencyData">Accumulated results of recursion - task element entity relationships (needed for later processing)</param>
        /// <param name="dependentTaskVariables">Accumulated results of recursion - task variables</param>
        /// <returns>Bottom-up (breadth-first) dependency-chain of task element IDs</returns>
        private void DoResolveVariableDependencies(int boeID, FullWorkspace workspace, Func<int, FullWorkspace, ICollection<IVariableDTO>> getReferencedVariables, IList<int> dependentTaskElementIds, IDictionary<int, TaskElementDependencyData> dependencyData, IList<VariableDependencyData> dependentTaskVariables)
        {
            // get the list of variables used by this BOE
            ICollection<IVariableDTO> referencedVariables = getReferencedVariables(boeID, workspace);

            if (referencedVariables.Any())
            {
                foreach (IVariableDTO currentVariable in referencedVariables)
                {
                    // store variable (check for duplicates first)
                    if (!dependentTaskVariables.Any(v => v.Variable.Id == currentVariable.Id))
                    {
                        dependentTaskVariables.Add(new VariableDependencyData
                        {
                            Variable = currentVariable,
                            WorkspaceFull = workspace  // i.e. the workspace associated with the VARIABLE
                        });
                    }

                    // process each task element that uses the variable
                    foreach (int currentTaskElementId in currentVariable.TaskElementIds)
                    {
                        #region Collect results (breadth-first)

                        // determine entity relationships for the current task variable
                        BoeTaskElementDTO currentTaskElement = this.BoeTaskElementDTODataLoader.GetById(currentTaskElementId, workspace.DecimalPrecision, workspace.CostDecimalPrecision);
                        int currentTaskBoeId = currentTaskElement.BoeID;

                        FullBoe currentTaskBoeFull = workspace.Boes.FirstOrDefault(b => b.Id == currentTaskBoeId);

                        // store task element ID (leaf-nodes first)
                        dependentTaskElementIds.Add(currentTaskElementId);

                        // store dependency data
                        dependencyData[currentTaskElementId] = new TaskElementDependencyData
                        {
                            TaskElementID = currentTaskElementId,
                            TaskElement = currentTaskElement,
                            BoeID = currentTaskBoeId,
                            BoeFull = currentTaskBoeFull,
                            WorkspaceFull = workspace
                        };

                        #endregion

                        // process child nodes (breadth-first recursion)
                        this.DoResolveVariableDependencies(currentTaskBoeId, workspace, getReferencedVariables, dependentTaskElementIds, dependencyData, dependentTaskVariables);
                    }
                }
            }
        }

        /// <summary>
        /// Recalculate (and save) the value of a task or workspace variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <param name="workspace">Workspace</param>
        /// <returns>Recalculated value</returns>
        private decimal RecalculateVariableValue(IVariableDTO variable, FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            decimal beforeValue;
            decimal recalculatedValue;

            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable));
            }
            else if (variable.VariableType == VariableType.Workspace)
            {
                WorkspaceVariableDTO workspaceVariable = variable as WorkspaceVariableDTO;

                DataClassForSumOfBOEsCalculation sumOfBoesCalculator = new DataClassForSumOfBOEsCalculation();
                sumOfBoesCalculator.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVariable }, workspace);

                // recalculate the workspace variable's value ...
                beforeValue = workspaceVariable.WorkspaceVariableValue;
                workspaceVariable.WorkspaceVariableValue = recalculatedValue = this.VariableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVariable, sumOfBoesCalculator);

                if (recalculatedValue != beforeValue)
                {
                    // ... then save it ...
                    workspaceVariable.Updateable = UpdateType.Upsert;
                    this._WorkspaceVariableDTODataLoader.SaveWorkspaceVariables(new Collection<WorkspaceVariableDTO> { workspaceVariable });

                    WorkspaceVariableDTO reloadedWorkspaceVariable = this._WorkspaceVariableDTODataLoader.GetById(workspaceVariable.Id);
                    workspaceVariable.UpdateDate = reloadedWorkspaceVariable.UpdateDate;
                }
            }
            else if (variable.VariableType == VariableType.Task)
            {
                OrdinaryVariableDto taskVariable = variable as OrdinaryVariableDto;

                DataClassForSumOfBOEsCalculation sumOfBoesCalculator = new DataClassForSumOfBOEsCalculation();
                sumOfBoesCalculator.FillData(new List<OrdinaryVariableDto>() { taskVariable }, null, workspace);

                // recalculate the task variable's value ...
                beforeValue = taskVariable.OrdinaryVariableValue ?? 0m;
                taskVariable.OrdinaryVariableValue = recalculatedValue = this.VariableSelectBOEtoSumCalculation.GetTaskVarLabelTotal(taskVariable, sumOfBoesCalculator);

                if (recalculatedValue != beforeValue)
                {
                    // ... then save it
                    taskVariable.Updateable = UpdateType.Upsert;
                    this._taskVariableLoader.Save(taskVariable);

                    OrdinaryVariableDto reloadedTaskVariable = this._taskVariableLoader.GetById(taskVariable.Id);
                    taskVariable.UpdateDate = reloadedTaskVariable.UpdateDate;
                }
            }
            else
            {
                throw new InvalidCastException(string.Format("Argument 'variable' must be of type '{0}'.", typeof(OrdinaryVariableDto).GetType().FullName));
            }

            return recalculatedValue;
        }

        #endregion
                  
        #endregion Private Members

        #region Protected Members

        /// <summary>
        /// Determines if string is an integer
        /// </summary>
        /// <param name="sValue">search Text</param>
        /// <returns>bool</returns>
        protected bool IsValidNumber(string sValue)
        {
            IsNumericValidation isNV = new IsNumericValidation();
            Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

            bool bResult;

            if (!isNV.isNumeric(sValue))
            {
                return true;
            }
            else
            {
                bResult = isNV.isInteger(sValue);

                if (!bResult)
                {
                    string ErrMsg = "Number must be between 0 and 2,147,483,647";
                    ValidationErrors.Add(new ValidationMessage("Invalid Integer", ErrMsg));

                    throw new GenValidationException(ValidationErrors);
                }
            }
            return bResult;
        }

        /// <summary>
        /// Exposes the <see cref="VariableSelectBOEtoSumCalculation"/>
        /// </summary>
        protected Common.Calculations.IVariableSelectBOEtoSumCalculation VariableSelectBOEtoSumCalculation { get; }

        /// <summary>
        /// Exposes the <see cref="BoeTaskElementDTODataLoader"/>
        /// </summary>
        protected IBoeTaskElementDTODataLoader BoeTaskElementDTODataLoader { get; }

        /// <summary>
        /// Exposes the <see cref="UserDTODataLoader"/>
        /// </summary>
        protected IUserDTODataLoader UserLoader { get; }

        /// <summary>
        /// Exposes the <see cref="PermissionsDTODataLoader"/>
        /// </summary>
        protected IPermissionsDTODataLoader PermissionsLoader { get; }

        #endregion

        #region All variable dependencies

        /// <summary>
        /// Identify all task and workspace variables that reference the designated BOE.  Recalculate the values of each such variable as well as the
        /// hourly totals (and spread distributions) for any task elements that USE them.  Continue (recursively) resolving references to any
        /// NESTED variables until the process is complete.  Note: This method runs within its OWN DB transaction scope.
        /// </summary>
        /// <param name="boeID">BOE PK identifier</param>
        /// <param name="ws">Workspace</param>
        public void ProcessAllVariableDependencies(int boeID, FullWorkspace ws)
        {
            // resolve the complete task-variable dependency chain (in breadth-first order)
            VariableDependencyResolutionData dependencies = this.ResolveAllVariableDependencies(boeID, ws);

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                this.ProcessAllVariableDependencies(boeID, ws, dependencies);
                scope.Complete();
            }
        }

        /// <summary>
        /// Recursive worker method.
        /// </summary>
        /// <param name="boeID">BOE PK identifier</param>
        /// <param name="ws">Workspace</param>
        /// <param name="dependencies">Task variable dependency data</param>
        public void ProcessAllVariableDependencies(int boeID, FullWorkspace ws, VariableDependencyResolutionData dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }
            else if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            ICollection<Tuple<FullBoe, FullWorkspace>> boesForPossibleStateTransition = new List<Tuple<FullBoe, FullWorkspace>>();

            ICollection<VariableDependencyNode> orderedDependencies = this.ResolveVariableUpdateOrder(boeID, dependencies);

            // now just enumerate through the ordered list of updates and apply each in turn
            foreach (VariableDependencyNode dependency in orderedDependencies)
            {
                if (dependency.Vertex == VertexType.BOE)
                {
                    #region Determine if BOE state needs to be changed (back) to Draft (actual update occurs after the loop completes)

                    // If a BOE is either Approved or Awaiting-Approval, then it should move back to Draft (if its MOQ total changes)
                    ICollection<int> affectedBoeIds = dependencies.TaskElements.Select(t => t.BoeID).Distinct().ToList();

                    foreach (int affectedBoeID in affectedBoeIds)
                    {
                        TaskElementDependencyData taskElementDependency = dependencies.TaskElements.First(t => t.BoeID == affectedBoeID);

                        FullBoe moqAffectedBoe = taskElementDependency.BoeFull;
                        BOEState currentBoeState = moqAffectedBoe.State;

                        if (currentBoeState == BOEState.AwaitingApproval || currentBoeState == BOEState.Approved || currentBoeState == BOEState.DraftLocked)
                        {
                            if (!boesForPossibleStateTransition.Any(t => t.Item1.Id == moqAffectedBoe.Id))
                            {
                                boesForPossibleStateTransition.Add(new Tuple<FullBoe, FullWorkspace>(moqAffectedBoe, taskElementDependency.WorkspaceFull));
                            }
                        }
                    }

                    #endregion
                }
                else if (dependency.Vertex == VertexType.TaskElement)
                {
                    #region Recalculate hours, costs and spread distributions for the task element that used the variable

                    TaskElementDependencyData taskElementData = dependencies.TaskElements.First(t => t.TaskElementID == dependency.ID);

                    BoeTaskElementDTO recalculatedTaskElement = this.RecalculateTaskElementHourValues(taskElementData.TaskElement, taskElementData.BoeFull, taskElementData.WorkspaceFull);

                    // If the recalculation caused the task's total hours to change, then ...
                    if (recalculatedTaskElement.Updateable == UpdateType.Upsert)
                    {
                        // Resource Types w/ locked hours have 0ed out the spreads, we need to recalculate them correctly
                        foreach (ResourceTypeDto resourceType in recalculatedTaskElement.taskElementLabors)
                        {
                            if (resourceType.ValueSpread.HasValue && resourceType.HourSpreadLocked)
                            {
                                resourceType.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(new LaborSpreadRequest()
                                {
                                    CurveID = resourceType.SpreadCurveID,
                                    EndDate = resourceType.EndDateValue,
                                    StartDate = resourceType.StartDateValue,
                                    HourSpread = resourceType.ValueSpread.Value
                                }, ws.DecimalPrecision);
                            }
                        }

                        // ... save it
                        this._BoeTaskElementMediator.MediatedSaveTaskElements(new Collection<BoeTaskElementDTO> { recalculatedTaskElement }, taskElementData.WorkspaceFull);
                    }

                    #endregion
                }
                else if (dependency.Vertex == VertexType.Variable)
                {
                    #region Recalculate and save the variable

                    VariableDependencyData variableData = dependencies.Variables.First(v => v.Variable.Id == dependency.ID);

                    // Adjust/update values for all sum-of-BOE variables (including nested ones).
                    // Note: This call includes a DB save.
                    this.RecalculateVariableValue(variableData.Variable, variableData.WorkspaceFull);

                    #endregion
                }
            }

            foreach (Tuple<FullBoe, FullWorkspace> entry in boesForPossibleStateTransition)
            {
                FullBoe moqAffectedBoe = entry.Item1;

                // only move BOE to Draft if it has  been updated
                if (moqAffectedBoe.Updateable == UpdateType.Upsert)
                {
                    FullWorkspace workspaceFull = entry.Item2;

                    // save the adjustments
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
                    {
                        // Validate the transition to Draft state
                        string validationMessage;
                        BOEState currentBoeState = moqAffectedBoe.State;
                        BOEState newBOEState = BOEState.Draft;  // always editable following a variable-triggered recalculation

                        if (this._boeStateMachine.PerformStateTransitionValidation(moqAffectedBoe, workspaceFull, currentBoeState, newBOEState, out validationMessage))
                        {
                            // If OK to transition ...

                            // ... Save the BOE
                            moqAffectedBoe.Updateable = UpdateType.Upsert;
                            moqAffectedBoe.State = newBOEState;
                            moqAffectedBoe.UpdatedByUserId = ws.CurrentActiveUser.UserID;
                            this._BoeLoader.Save(moqAffectedBoe);

                            // ... Perform the state transition
                            this._boeStateMachine.PerformStateTransitionAction(moqAffectedBoe, workspaceFull, currentBoeState, newBOEState);
                        }

                        scope.Complete();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Checks decimal Precision, throws GenValidationException if invalid precision.
        /// </summary>
        /// <param name="value">The value to check, null is allowed.</param>
        /// <param name="precision">The precision to check against.</param>
        /// <param name="fieldName">The field name to use in case of exception.</param>
        private void CheckPrecision(decimal? value, int precision, string fieldName)
        {
            if (value.HasValue)
            {
                if (!ImportUtils.IsMaxDecimalPlaces(value.ToString(), precision, out decimal val))
                {
                    throw new GenValidationException("Too much decimal precision in " + fieldName);
                }
            }
        }

        /// <summary>
        /// Get a list of MOQ Types for dropdown
        /// </summary>
        /// <param name="useNewMoqTypes">Are we be using new MOQ Types (after 10/2020)</param>
        /// <param name="moqType">Selected MOQ Type</param>
        /// <returns>MOQ Types</returns>
        public ICollection<SelectListItem> GetMOQTypeSelectList(bool useNewMoqTypes, MOQType? moqType)
        {
            ICollection<MOQType> templateBoeMoqTypes = useNewMoqTypes 
                ? new List<MOQType>() { MOQType.Historical, MOQType.Comparative, MOQType.CostEstimatingRelationships, MOQType.ParametricEstimates, MOQType.AnalogousRelationships, MOQType.SOW, MOQType.LOE, MOQType.SME, MOQType.NonLabor } 
                : this.GetMOQTypes();

            List<SelectListItem> results = templateBoeMoqTypes.Select(t => new SelectListItem
            {
                Text = t.GetDescription(),
                Value = ((int)t).ToString(),
                Selected = t == moqType
            }).ToList();

            return results;
        }

        /// <summary>
        /// Returns a list of labels to be used in the MOQ Types page.
        /// </summary>
        /// <returns>Labels for MOQ Type Data Table Fields</returns>
        public virtual MoqTypeTableDataLabels GetMoqTypeLabels()
        {
            return new MoqTypeTableDataLabels();
        }

        /// <summary>
        /// Returns help URLs for MOQ Type fields
        /// </summary>
        /// <returns>help URLs for MOQ Type fields</returns>
        public virtual MoqTypeHelpUrls GetMoqTypeHelpUrls()
        {
            // TODO - populate with SSC URLs when we have them
            MoqTypeHelpUrls toReturn = new MoqTypeHelpUrls();

            toReturn.TableNameSuffix = string.Empty;
            toReturn.RepositoryNameSuffix = string.Empty;
            toReturn.QueryTypeSuffix = string.Empty;
            toReturn.DateOfReportSuffix = string.Empty;
            toReturn.HistoricalProgramNameSuffix = string.Empty;
            toReturn.ContractNumberSuffix = string.Empty;
            toReturn.WBSElementSuffix = string.Empty;
            toReturn.PoPStartSuffix = string.Empty;
            toReturn.PoPEndSuffix = string.Empty;
            toReturn.TotalWBSHoursSuffix = string.Empty;
            toReturn.AdditionalQueryFiltersSuffix = string.Empty;
            toReturn.TotalRelevantHoursSuffix = string.Empty;

            toReturn.CERNameSuffix = string.Empty;
            toReturn.PENameSuffix = string.Empty;
            toReturn.ARNameSuffix = string.Empty;

            toReturn.SOWDescriptionSuffix = string.Empty;
            toReturn.LOEDescriptionSuffix = string.Empty;

            toReturn.SMEReasonsSuffix = string.Empty;
            toReturn.SMEHoursLogicSuffix = string.Empty;
            toReturn.SMEDurationLogicSuffix = string.Empty;
            toReturn.SMETasksSuffix = string.Empty;

            toReturn.HistoricalRationaleSuffix = string.Empty;
            toReturn.ComparativeRationaleSuffix = string.Empty;
            toReturn.CerPeArRationaleSuffix = string.Empty;
            toReturn.SowLoeRationaleSuffix = string.Empty;
            toReturn.NonLaborRationaleSuffix = string.Empty;
            toReturn.SkillMixSuffix = string.Empty;

            return toReturn;
        }
    }

    public enum ControllerCustomFieldType
    {
        Task = 0,
        LaborTypes = 1
    }
}