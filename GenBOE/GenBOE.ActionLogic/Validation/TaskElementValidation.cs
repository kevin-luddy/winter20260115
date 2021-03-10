// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Handles Task Element validation
    /// </summary>
    public class TaskElementValidation
    {
        #region Error Message Constants

        private static string GENERIC_FAILURE = "Validation Failed";

        private static string TASK_START_DATE_MISSING = "Task Start Date is required.";
        private static string TASK_END_DATE_MISSING = "Task End Date is required.";

        private static string TASK_START_DATE_FAILED = "Task Start Date is currently set before the BOE Start Date or after the BOE End Date.";
        private static string TASK_END_DATE_FAILED = "Task End Date is currently set before the BOE Start Date or after the BOE End Date.";
        private static string TASK_END_BEFORE_START_DATE = "Task Start Date is currently set after the Task End Date";

        private static string RESOURCE_START_DATE_FAILED = "Resource Start Date is currently set before the Task Start Date (Start Date: {0}, End Date: {1}, Value Spread ({2}): {3}).";

        private static string RESOURCE_END_DATE_FAILED = "Resource End Date is currently set after the Task End Date. (Start Date: {0}, End Date: {1}, Value Spread ({2}): {3}).";

        private static string SPREAD_DATES_FAILED = "Resource Spread date is outside the Resource period of performance. (Spread Date: {0}, Spread Value: {1}).";

        private static string MOQ_EQUATION_FAILED = "An invalid MOQ equation was entered.";
        private static string INVALID_TASK_VARIABLE = "Task variable {0} calculated value does not match the saved value.";

        private static string DELTA_FAILED = "Total Resource Spread must equal the MOQ Equation total.  Verify the MOQ Equation Total is fully spread and no delta remains.";

        private static string SPREAD_RESOURCE_TYPE_HOUR_TOTALS_FAILED = "Resource Types {0} totals do not equal the total Resource Spreads total.";
        private static string SPREAD_RESOURCE_TYPE_COST_TOTALS_FAILED = "Resource Types cost totals do not equal the total Resource Spreads total.";

        private static string RESOURCE_SPREAD_VALUE_FAILED = "Sum of Spread Values for the Resource Type (Start Date: {0}, End Date: {1}, Value Spread ({2}): {3}) does not match the Resource Type Value.";

        private static string DATES_HEADER = "<b><i>The following errors are due to dates which are outside of the period of performance.  Please adjust the dates to fall within the " 
            + "period of performance.  Using the \"Recalculate Task Element\" button will automatically adjust the resource dates.  *Note:  Any discrete spreads which fall outside of " 
            + "the adjusted period of performance will be deleted and a delta value may remain.</i></b><br />";

        private static string VALUES_HEADER = "<br /><b><i>The following errors are due to value issues.  Resources using a spread curve may be automatically corrected "
            + "by clicking the \"Recalculate Task Element\" button.  Resources using a discrete spread (cost or {0}) will require manual adjustment.  "
            + "*Note:  Any discrete spreads that fall outside of the adjusted period of performance will be deleted and a delta value may remain.</i></b><br />";

        #endregion

        #region Properties & ctor

        /// <summary>
        /// Variable Select Boe To Sum Calculation class
        /// </summary>
        private IVariableSelectBOEtoSumCalculation VariableSelectBOEtoSumCalculation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskElementValidation"/> class for unit tests.
        /// </summary>
        internal TaskElementValidation()
        { }

        /// <summary>
        /// Default Ctor
        /// </summary>
        /// <param name="variableSelectBOEtoSumCalculation">Variable Select Boe To Sum Calculation</param>
        public TaskElementValidation(IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation)
        {
            this.VariableSelectBOEtoSumCalculation = variableSelectBOEtoSumCalculation;
        }

        #endregion

        /// <summary>
        /// This method returns invalid Task Element Ids, without any messages or reasons. It is more efficient because it only checks valid/invalid, without worrying about all the reasons.
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="taskElementsToValidate">Labor Task Elements to check</param>
        /// <returns>A Collection of invalid Task Element Ids</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "If a task element fails validation, we can mark it as invalid and move on. We do not want to let this bubble up.")]
        public virtual ICollection<int> GetInvalidTaskElementIds(FullWorkspace ws, IReadOnlyCollection<BoeTaskElementDTO> taskElementsToValidate)
        {
            if (ws == null) { throw new ArgumentNullException(nameof(ws)); }
            if (taskElementsToValidate == null) { throw new ArgumentNullException(nameof(taskElementsToValidate)); }
            if (!taskElementsToValidate.Any()) { return new List<int>(); }

            ConcurrentBag<int> result = new ConcurrentBag<int>();
            ConcurrentBag<LaborValidationClass> dateErrors = new ConcurrentBag<LaborValidationClass>();
            ConcurrentBag<LaborValidationClass> valueErrors = new ConcurrentBag<LaborValidationClass>();

            foreach (int boeId in taskElementsToValidate.Select(x => x.BoeID).Distinct())
            {
                FullBoe boe = ws.Boes.Single(x => x.Id == boeId);

                DateTime boeStartDate = boe.StartDate.Normalize();
                DateTime boeEndDate = boe.EndDate.Normalize();

                object LOCK = new object();

                Parallel.ForEach(taskElementsToValidate.Where(x => x.BoeID == boeId), taskElement =>
                {
                    try
                    {
                        if (!ValidateTaskDates(boeStartDate, boeEndDate, taskElement, ref dateErrors, true)
                                || !ValidateResourceTypesForIndividualTask(ws, taskElement, ref dateErrors, ref valueErrors, true)
                                || !this.ValidateOverallSumOfHoursAndCosts(ws, taskElement, ref valueErrors, LOCK, true))
                        {
                            result.Add(taskElement.Id);
                        }
                    }
                    catch (Exception)
                    {
                        result.Add(taskElement.Id);
                    }
                });
            }

            return result.Distinct().ToList(); // distinct should not be needed, but just in case..
        }

        /// <summary>
        /// Validates Task Elements
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="taskElementsToValidate">Labor Task Elements to check</param>
        /// <returns>A list of errors (or an empty ICollection if none)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "If anything fails that we didn't catch inside of the methods, we need to handle it and make the task element as failing validation..")]
        public virtual ICollection<LaborValidationClass> ValidateTaskElementsWithErrorMessages(FullWorkspace ws, ICollection<BoeTaskElementDTO> taskElementsToValidate)
        {
            if (ws == null) { throw new ArgumentNullException(nameof(ws)); }
            if (taskElementsToValidate == null) { throw new ArgumentNullException(nameof(taskElementsToValidate)); }
            if (!taskElementsToValidate.Any()) { return new List<LaborValidationClass>(); }

            List<LaborValidationClass> errors = new List<LaborValidationClass>();

            object LOCK = new object();

            object otherLOCK = new object();

            foreach (int boeId in taskElementsToValidate.Select(x => x.BoeID).Distinct())
            {
                FullBoe boe = ws.Boes.Single(x => x.Id == boeId);

                DateTime boeStartDate = boe.StartDate.Normalize();
                DateTime boeEndDate = boe.EndDate.Normalize();

                Parallel.ForEach(taskElementsToValidate.Where(x => x.BoeID == boeId), taskElement =>
                {
                    try
                    {
                        ConcurrentBag<LaborValidationClass> dateErrors = new ConcurrentBag<LaborValidationClass>();
                        ConcurrentBag<LaborValidationClass> valueErrors = new ConcurrentBag<LaborValidationClass>();
                        ConcurrentBag<LaborValidationClass> otherErrors = new ConcurrentBag<LaborValidationClass>();

                        Parallel.Invoke(
                            // Doing this because the MOQ calculation usually takes the longest.. So we are going to run the other checks in parallel to the check that deals w/ the MOQ equation.
                            () => this.ValidateOverallSumOfHoursAndCosts(ws, taskElement, ref valueErrors, LOCK),
                            () =>
                            {
                                ValidateTaskDates(boeStartDate, boeEndDate, taskElement, ref dateErrors);
                                ValidateResourceTypesForIndividualTask(ws, taskElement, ref dateErrors, ref valueErrors);
                            }
                        );

                        #region Put together the error list

                        List<LaborValidationClass> temp = new List<LaborValidationClass>();
                        if (dateErrors.Any())
                        {
                            temp.Add(new LaborValidationClass() { ErrorMessage = new ValidationMessage(DATES_HEADER), ErrorType = LaborValidationErrorTypeEnum.Other, BoeId = taskElement.BoeID, TaskElementId = taskElement.Id });
                            temp.AddRange(dateErrors);
                        }

                        if (valueErrors.Any())
                        {
                            string hoursLabel = FullObjectHelper.HoursLabel(ws);
                            string valuesHeader = string.Format(VALUES_HEADER, hoursLabel);
                            temp.Add(new LaborValidationClass() { ErrorMessage = new ValidationMessage(valuesHeader), ErrorType = LaborValidationErrorTypeEnum.Other, BoeId = taskElement.BoeID, TaskElementId = taskElement.Id });
                            temp.AddRange(valueErrors);
                        }

                        if (otherErrors.Any())
                        {
                            temp.Add(new LaborValidationClass() { ErrorMessage = new ValidationMessage(GENERIC_FAILURE), ErrorType = LaborValidationErrorTypeEnum.Other, BoeId = taskElement.BoeID, TaskElementId = taskElement.Id });
                            temp.AddRange(otherErrors);
                        }

                        if (temp.Any())
                        {
                            lock (otherLOCK)
                            {
                                errors.AddRange(temp);
                            }
                        }

                        #endregion
                    }
                    catch
                    {
                        errors.Add(new LaborValidationClass() { ErrorMessage = new ValidationMessage(GENERIC_FAILURE), ErrorType = LaborValidationErrorTypeEnum.Other, BoeId = taskElement.BoeID, TaskElementId = taskElement.Id });
                    }
                });
            }

            return errors;
        }

        #region Validators

        /// <summary>
        /// Validates Task Dates vs Boe Dates
        /// </summary>
        /// <param name="boeStartDate">Boe Start Date</param>
        /// <param name="boeEndDate">Boe End Date</param>
        /// <param name="taskElement">Task Element to Validate</param>
        /// <param name="dateErrors">Date Errors which will be adjusted if any errors are found.</param>
        /// <param name="returnOnFirstInvalid">If this is set to true, we are only looking to see if the task elementis invalid. If it is, we abort processing and report the finding</param>
        /// <returns>False if invalid</returns>
        private static bool ValidateTaskDates(DateTime boeStartDate, DateTime boeEndDate, BoeTaskElementDTO taskElement, ref ConcurrentBag<LaborValidationClass> dateErrors, bool returnOnFirstInvalid = false)
        {
            if (!taskElement.StartDate.HasValue || taskElement.StartDate == DateTime.MinValue) { if (returnOnFirstInvalid) { return false; } dateErrors.Add(GenerateTaskElementError(taskElement, TASK_START_DATE_MISSING)); }
            if (!taskElement.EndDate.HasValue || taskElement.EndDate == DateTime.MinValue) { if (returnOnFirstInvalid) { return false; } dateErrors.Add(GenerateTaskElementError(taskElement, TASK_END_DATE_MISSING)); }

            if (taskElement.StartDate.HasValue && taskElement.EndDate.HasValue)
            {
                DateTime taskStartDate = taskElement.StartDate.Value.Normalize();
                if (taskStartDate < boeStartDate || taskStartDate > boeEndDate) 
                { 
                    if (returnOnFirstInvalid) { return false; } 
                    dateErrors.Add(GenerateTaskElementError(taskElement, TASK_START_DATE_FAILED)); 
                }

                DateTime taskEndDate = taskElement.EndDate.Value.Normalize();
                if (taskEndDate > boeEndDate || taskEndDate < boeStartDate) 
                { 
                    if (returnOnFirstInvalid) { return false; } 
                    dateErrors.Add(GenerateTaskElementError(taskElement, TASK_END_DATE_FAILED)); 
                }

                if (taskStartDate > taskEndDate) 
                { 
                    if (returnOnFirstInvalid) { return false; } 
                    dateErrors.Add(GenerateTaskElementError(taskElement, TASK_END_BEFORE_START_DATE)); 
                }
            }

            return true;
        }

        /// <summary>
        /// Validates overall Sum of (hours/costs) for the task (including MOQ and Delta)
        /// </summary>
        /// <param name="ws">Ws to which the Boe belongs</param>
        /// <param name="taskElement">Task which we are validating</param>
        /// <param name="valueErrors">Value Errors which will be adjusted if any errors are found.</param>
        /// <param name="LOCK">Lock used for parallel processing, to make sure we aren't stepping on other threads</param>
        /// <param name="returnOnFirstInvalid">If this is set to true, we are only looking to see if the task elementis invalid. If it is, we abort processing and report the finding</param>
        /// <returns>False if invalid</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "If anything fails w/ the MOQ, we want to catch it and mark it as invalid.")]
        private bool ValidateOverallSumOfHoursAndCosts(FullWorkspace ws, BoeTaskElementDTO taskElement, ref ConcurrentBag<LaborValidationClass> valueErrors, object LOCK, bool returnOnFirstInvalid = false)
        {
            #region Hours Check -> (MOQ Equation Value == Sum of Types Hours) and (Sum of Types Hours == Sum of Spread Hours); also checks Task level Sum Of Boe variables;

            decimal moqResult = 0;

            decimal? sumOfResourceTypeHours = taskElement.taskElementLabors.Where(x => x.SpreadType == SpreadType.Hours).Sum(x => x.ValueSpread);
            decimal? sumOfResourceSpreadHours = taskElement.taskElementLabors.Where(s => s.SpreadType == SpreadType.Hours).Sum(a => a.LaborSpreads.Sum(b => b.LaborSpreadValue));
            if (sumOfResourceTypeHours != sumOfResourceSpreadHours)
            {
                if (returnOnFirstInvalid) { return false; }
                string hoursLabel = FullObjectHelper.HoursLabel(ws);
                string failedMessage = string.Format(SPREAD_RESOURCE_TYPE_HOUR_TOTALS_FAILED, hoursLabel);

                valueErrors.Add(new LaborValidationClass() { ErrorMessage = new ValidationMessage(failedMessage), ErrorType = LaborValidationErrorTypeEnum.ResourceTypeSpreadSum, TaskElementId = taskElement.Id, BoeId = taskElement.BoeID });
            }

            try
            {
                DataClassForSumOfBOEsCalculation dataForCalculation = new DataClassForSumOfBOEsCalculation();

                lock (LOCK) // DB calls cannot happen in parallel
                {
                    Collection<WorkspaceVariableDTO> workspaceVars = ws.WorkspaceVariables.Where(x => taskElement.WorkspaceVariableIDs.Contains(x.Id)).ToCollection();
                    dataForCalculation.FillData(taskElement.OrdinaryVariables, workspaceVars, ws);

                    bool variablesValid = this.ValidateTaskSumOfBoesVariables(ws.DecimalPrecision, taskElement, dataForCalculation, ref valueErrors, returnOnFirstInvalid);
                    if (!variablesValid && returnOnFirstInvalid) { return false; }

                    moqResult = this.CalculateValueOfMoqEquation(ws, taskElement, dataForCalculation, workspaceVars);
                }
            }
            catch
            {
                if (returnOnFirstInvalid) { return false; }
                valueErrors.Add(new LaborValidationClass() { ErrorMessage = new ValidationMessage(MOQ_EQUATION_FAILED), ErrorType = LaborValidationErrorTypeEnum.Other, TaskElementId = taskElement.Id, BoeId = taskElement.BoeID });
            }

            if (moqResult != sumOfResourceTypeHours)
            {
                if (returnOnFirstInvalid) { return false; }
                valueErrors.Add(new LaborValidationClass() 
                    { ErrorMessage = new ValidationMessage(DELTA_FAILED), ErrorType = LaborValidationErrorTypeEnum.Delta, TaskElementId = taskElement.Id, BoeId = taskElement.BoeID });
            }

            #endregion

            #region Cost Check -> Sum of Types Costs == Sum of Spread Costs

            decimal? sumOfResourceTypeCosts = taskElement.taskElementLabors.Where(x => x.SpreadType == SpreadType.Cost).Sum(x => x.ValueSpread);
            decimal? sumOfResourceSpreadCosts = taskElement.taskElementLabors.Where(s => s.SpreadType == SpreadType.Cost).Sum(a => a.LaborSpreads.Sum(b => b.LaborSpreadValue));

            if (sumOfResourceTypeCosts != sumOfResourceSpreadCosts)
            {
                if (returnOnFirstInvalid) { return false; }
                valueErrors.Add(new LaborValidationClass() 
                    { ErrorMessage = new ValidationMessage(SPREAD_RESOURCE_TYPE_COST_TOTALS_FAILED), ErrorType = LaborValidationErrorTypeEnum.ResourceTypeSpreadCost, TaskElementId = taskElement.Id, BoeId = taskElement.BoeID });
            }

            #endregion

            return true;
        }

        /// <summary>
        /// Validates Resource Types for an individual Task
        /// </summary>
        /// <param name="ws">WS to which the Boe/Task belong</param>
        /// <param name="taskElement">Task being validated</param>
        /// <param name="dateErrors">A list of errors that will be adjusted if any date errors are found.</param>
        /// <param name="valueErrors">A list of errors that will be adjusted if any value errors are found.</param>
        /// <param name="returnOnFirstInvalid">If this is set to true, we are only looking to see if the task elementis invalid. If it is, we abort processing and report the finding</param>
        /// <returns>False if invalid</returns>
        private static bool ValidateResourceTypesForIndividualTask(FullWorkspace ws, BoeTaskElementDTO taskElement, ref ConcurrentBag<LaborValidationClass> dateErrors,
            ref ConcurrentBag<LaborValidationClass> valueErrors, bool returnOnFirstInvalid = false)
        {
            foreach (ResourceTypeDto resourceType in taskElement.taskElementLabors)
            {
                string resourceTypeString = resourceType.SpreadType == SpreadType.Cost ? "cost" : FullObjectHelper.HoursLabel(ws);
                string resourceTypeValueString = resourceType.SpreadType == SpreadType.Cost ? "$" + Utilities.AdjustPrecision(resourceType.ValueSpread.Value, 2).ToString()
                                    : Utilities.AdjustPrecision(resourceType.ValueSpread.Value, ws.DecimalPrecision).ToString();

                string resourceStartDateString = resourceType.StartDateValue.ToShortDateString();
                string resourceEndDateString = resourceType.EndDateValue.ToShortDateString();

                if (resourceType.StartDate == DateTime.MaxValue || resourceType.StartDate < taskElement.StartDate)
                {
                    if (returnOnFirstInvalid) { return false; }
                    dateErrors.Add(GenerateResourceTypeError(resourceType, string.Format(RESOURCE_START_DATE_FAILED, 
                        resourceStartDateString, resourceEndDateString, resourceTypeString, resourceTypeValueString)));
                }

                if (resourceType.EndDate == DateTime.MinValue || resourceType.EndDate > taskElement.EndDate)
                {
                    if (returnOnFirstInvalid) { return false; }
                    dateErrors.Add(GenerateResourceTypeError(resourceType, string.Format(RESOURCE_END_DATE_FAILED,
                        resourceStartDateString, resourceEndDateString, resourceTypeString, resourceTypeValueString)));
                }

                if (resourceType.ValueSpread != resourceType.LaborSpreads.Sum(x => x.LaborSpreadValue))
                {
                    if (returnOnFirstInvalid) { return false; }
                    valueErrors.Add(GenerateResourceTypeError(resourceType, string.Format(RESOURCE_SPREAD_VALUE_FAILED,
                        resourceStartDateString, resourceEndDateString, resourceTypeString, resourceTypeValueString)));
                }

                foreach (ResourceSpreadDto badSpread in resourceType.LaborSpreads.Where(spread => (spread.LaborSpreadDate < resourceType.StartDate || spread.LaborSpreadDate > resourceType.EndDate)
                    && spread.LaborSpreadValue != 0)) // BOE-J 276 - Ignore bad dates for spreads that have values of 0
                {
                    if (returnOnFirstInvalid) { return false; }
                    dateErrors.Add(GenerateResourceSpreadError(badSpread, taskElement.Id, string.Format(SPREAD_DATES_FAILED,
                        badSpread.LaborSpreadDate.ToShortDateString(), badSpread.LaborSpreadValue)));
                }
            }

            return true;
        }

        /// <summary>
        /// Validates that the value of the task variable (sum of boes one) is correct
        /// </summary>
        /// <param name="wsDecimalPrecision">WS Decimal precision</param>
        /// <param name="taskElement">Task which we are validating</param>
        /// <param name="data">Data used for calculations, must come already filled</param>
        /// <param name="valueErrors">Value Errors which will be adjusted if any errors are found.</param>
        /// <param name="returnOnFirstInvalid">If this is set to true, we are only looking to see if the task elementis invalid. If it is, we abort processing and report the finding</param>
        /// <returns>False if invalid</returns>
        private bool ValidateTaskSumOfBoesVariables(int wsDecimalPrecision, BoeTaskElementDTO taskElement, DataClassForSumOfBOEsCalculation data, ref ConcurrentBag<LaborValidationClass> valueErrors, bool returnOnFirstInvalid)
        {
            bool result = true;

            foreach (OrdinaryVariableDto variable in taskElement.OrdinaryVariables.Where(x => x.ValueType == VarValueType.SumOfBOEs).ToList())
            { 
                decimal calculatedValue = this.VariableSelectBOEtoSumCalculation.GetTaskVarLabelTotal(variable, data);
                if(Utilities.AdjustPrecision(variable.OrdinaryVariableValue ?? 0, wsDecimalPrecision) != Utilities.AdjustPrecision(calculatedValue, wsDecimalPrecision))
                {
                    if (returnOnFirstInvalid) { return false; }
                    result = false;

                    valueErrors.Add(new LaborValidationClass()
                    { ErrorMessage = new ValidationMessage(string.Format(INVALID_TASK_VARIABLE, variable.OrdinaryVariableName)), ErrorType = LaborValidationErrorTypeEnum.TaskElement, TaskElementId = taskElement.Id, BoeId = taskElement.BoeID });
                }
            }

            return result;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Calculate the MOQ Equation value
        /// </summary>
        /// <param name="ws">Ws to which the Boe belongs</param>
        /// <param name="taskElement">Task to which the MOQ Equation belongs</param>
        /// <param name="dataForCalculation">Data used for calculations, must come already filled</param>
        /// <param name="workspaceVars">Workspace Variables</param>
        /// <returns>Decimal value of the equation; 0 if the equation was empty; Throws an exception if there was an error</returns>
        private decimal CalculateValueOfMoqEquation(FullWorkspace ws, BoeTaskElementDTO taskElement, DataClassForSumOfBOEsCalculation dataForCalculation, Collection<WorkspaceVariableDTO> workspaceVars)
        {
            decimal moqResult = 0;

            if (!string.IsNullOrEmpty(taskElement.MOQHoursEquation))
            {
                string moqResultString = Common.MOQ.Parser.Calculate(taskElement.MOQHoursEquation, taskElement.OrdinaryVariables, workspaceVars, this.VariableSelectBOEtoSumCalculation, dataForCalculation, ws);

                if (decimal.TryParse(moqResultString, out moqResult))
                {
                    moqResult = Utilities.AdjustPrecision(moqResult, ws.DecimalPrecision);
                }
            }

            return moqResult;
        }

        private static LaborValidationClass GenerateTaskElementError(BoeTaskElementDTO taskElement, string errorMessage)
        {
            return new LaborValidationClass()
            {
                ErrorMessage = new ValidationMessage(errorMessage),
                ErrorType = LaborValidationErrorTypeEnum.TaskElement,
                BoeId = taskElement.BoeID,
                TaskElementId = taskElement.Id
            };
        }

        private static LaborValidationClass GenerateResourceTypeError(ResourceTypeDto resourceType, string errorMessage)
        {
            return new LaborValidationClass()
            {
                ErrorMessage = new ValidationMessage(errorMessage),
                ErrorType = LaborValidationErrorTypeEnum.ResourceType,
                BoeId = resourceType.BoeID,
                TaskElementId = resourceType.TaskElementId,
                TypeId = resourceType.Id
            };
        }

        private static LaborValidationClass GenerateResourceSpreadError(ResourceSpreadDto resourceSpread, int taskElementId, string errorMessage)
        {
            return new LaborValidationClass()
            {
                ErrorMessage = new ValidationMessage(errorMessage),
                ErrorType = LaborValidationErrorTypeEnum.ResourceSpread,
                BoeId = resourceSpread.BoeID,
                TaskElementId = taskElementId,
                TypeId = resourceSpread.LaborTypeId,
                SpreadId = resourceSpread.Id
            };
        }

        #endregion
    }
}