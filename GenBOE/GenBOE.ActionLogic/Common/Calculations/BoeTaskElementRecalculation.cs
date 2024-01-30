// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.Calculations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Common.MOQ;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    /// <summary>
    /// For any workspace or task variable changes made to a BOE, the BOE labor spread will need to be
    /// recalculated.
    /// </summary>
    public class BoeTaskElementRecalculation : IBoeTaskElementRecalculation
    {
        #region Private Fields and Constructor

        private IVariableSelectBOEtoSumCalculation _VarSelectBOEtoSumCalc;
        private IFullObjectFactory _Factory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inVarSelectBOEtoSumCalc"></param>
        /// <param name="factory">Full object factory.</param>
        public BoeTaskElementRecalculation(
            IVariableSelectBOEtoSumCalculation inVarSelectBOEtoSumCalc,
            IFullObjectFactory factory)
        {
            this._VarSelectBOEtoSumCalc = inVarSelectBOEtoSumCalc;
            this._Factory = factory;
        }

        #endregion

        /// <summary>
        /// Calculates MOQ Hours
        /// </summary>
        /// <param name="boeTaskElement">BOE Task element</param>
        /// <param name="fullWorkspace">workspace that contains both the BOE and the task element</param>
        /// <returns>string representing the calculated MOQ hours</returns>
        public String CalculateMOQHoursTotal(BoeTaskElementDTO boeTaskElement, FullWorkspace fullWorkspace)
        {
            if (boeTaskElement == null)
            {
                throw new ArgumentNullException(nameof(boeTaskElement));
            }

            if (fullWorkspace == null)
            {
                throw new ArgumentNullException(nameof(fullWorkspace));
            }

            // Recalculate MOQ Equation
            ICollection<WorkspaceVariableDTO> workspaceVars = (from v in fullWorkspace.WorkspaceVariables
                                                              where boeTaskElement.WorkspaceVariableIDs.Contains(v.Id)
                                                              select v).ToCollection();
            
            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(boeTaskElement.OrdinaryVariables, workspaceVars, fullWorkspace);

            string calculatedResult = string.Empty;
            //HACK: bube, this prevents an invalid equation that made its way into the database to not throw an exception, and returns 0 instead.
            //Updates to the system (primarily import BOE process) should be made to properly validate and prevent these invalid MOQEquations from 
            //  making it into the database in the first place
            try
            {
                calculatedResult = Parser.Calculate(boeTaskElement.MOQHoursEquation, boeTaskElement.OrdinaryVariables, workspaceVars, this._VarSelectBOEtoSumCalc, data, fullWorkspace);
            }
            catch (FormatException)
            {
                calculatedResult = "0";
            }
            catch (GeneralMOQParsingException)
            {
                calculatedResult = "0";
            }

            return calculatedResult;
        }

        /// <summary>
        /// This function recalculates the labor spreads
        /// </summary>
        /// <param name="laborTypes">The labor types.</param>
        /// <param name="ws">The workspace.</param>
        public void AddRecalulatedLaborSpreadsOntoLaborTypes(ICollection<ResourceTypeDto> laborTypes, WorkspaceDTO ws)
        {
            if (laborTypes == null)
            {
                throw new ArgumentNullException(nameof(laborTypes));
            }

            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            LaborSpreadRequest laborSpreadRequest = new LaborSpreadRequest();

            foreach (ResourceTypeDto labortype in laborTypes)
            {
                if (labortype.SpreadCurveID != SpreadCurves.DiscreteHours && labortype.SpreadCurveID != SpreadCurves.DiscreteCost)
                {
                    // get labor spread request
                    laborSpreadRequest.CurveID = labortype.SpreadCurveID;
                    laborSpreadRequest.StartDate = (DateTime)labortype.StartDate;
                    laborSpreadRequest.EndDate = (DateTime)labortype.EndDate;
                    laborSpreadRequest.HourSpread = labortype.ValueSpread.HasValue ? labortype.ValueSpread.Value : 0;
                    
                    // Recalculate Labor Spread
                    // add new labor spread to labor type.
                    int decimalPrecision = labortype.SpreadType == SpreadType.Cost ? ws.CostDecimalPrecision : ws.DecimalPrecision;
                    labortype.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(laborSpreadRequest, decimalPrecision);

                    //Update UpdateType property
                    foreach (ResourceSpreadDto laborSpread in labortype.LaborSpreads)
                    {
                        laborSpread.Updateable = UpdateType.Upsert;
                    }
                }
            }
        }

        /// <summary>
        /// This function recalculates the labor spreads
        /// </summary>
        /// <param name="odcTypes">The odc types.</param>
        /// <param name="ws">The workspace.</param>
        public void AddRecalulatedODCSpreadsOntoODCTypes(ICollection<OtherDirectCostType> odcTypes, WorkspaceDTO ws)
        {
            if (odcTypes == null)
            {
                throw new ArgumentNullException(nameof(odcTypes));
            }

            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            LaborSpreadRequest laborSpreadRequest = new LaborSpreadRequest();

            foreach (OtherDirectCostType odctype in odcTypes)
            {
                if (odctype.SpreadCurve == SpreadCurves.Level)
                {
                    // get labor spread request
                    laborSpreadRequest.CurveID = SpreadCurves.SpreadCurve3;
                    laborSpreadRequest.StartDate = (DateTime)odctype.StartDate;
                    laborSpreadRequest.EndDate = (DateTime)odctype.EndDate;
                    laborSpreadRequest.HourSpread = odctype.Cost.HasValue ? Convert.ToInt64(odctype.Cost.Value * 100) : 0;

                    Collection<OtherDirectCostSpread> odcSpreadsToAdd = new Collection<OtherDirectCostSpread>();
                    // Recalculate Labor Spread
                    // add new labor spread to labor type.
                    Collection<ResourceSpreadDto> spreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(laborSpreadRequest, ws.DecimalPrecision);
                    foreach (ResourceSpreadDto ls in spreads)
                    {
                        odcSpreadsToAdd.Add(new OtherDirectCostSpread()
                        {
                            Updateable = UpdateType.Upsert,
                            CostSpreadValue = Convert.ToInt64(ls.LaborSpreadValue),
                            ODCSpreadDate = ls.LaborSpreadDate
                        });
                    }
                    odctype.ODCSpreads = odcSpreadsToAdd;
                }
                else if (odctype.SpreadCurve == SpreadCurves.Load)
                {
                    DateTime cursorDate = (DateTime)odctype.StartDate;
                    Collection<OtherDirectCostSpread> odcSpreadsToAdd = new Collection<OtherDirectCostSpread>();
                    for (long x = 0; x <= numberOfMonthsBetweenTwoMMYYYYDates((DateTime)odctype.StartDate, (DateTime)odctype.EndDate); x++)
                    {
                        odcSpreadsToAdd.Add(new OtherDirectCostSpread()
                        {
                            Updateable = UpdateType.Upsert,
                            CostSpreadValue = Convert.ToInt64(odctype.Cost * 100),
                            ODCSpreadDate = cursorDate
                        });
                        cursorDate = cursorDate.AddMonths(1);
                    }
                    odctype.ODCSpreads = odcSpreadsToAdd;
                }
                else
                {
                    odctype.ODCSpreads = new Collection<OtherDirectCostSpread>();
                }
            }
        }

        /// <summary>
        /// Recalculate BOE Labor given a CLIN ID
        /// </summary>
        /// <param name="clin">The clin.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <param name="fullWorkspace">The full workspace.</param>
        /// <param name="incomingBoeTaskElements">The incoming boe task elements.</param>
        /// <param name="incomingWorkspaceVariables">The incoming workspace variables.</param>
        /// <returns>
        /// list of BOEs that have been updated
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// inClin
        /// or
        /// fullWorkspace
        /// </exception>
        
        public List<BoeTaskElementDTO> RecalculateLaborWithClin(FullClin clin, VariableType variableType, FullWorkspace fullWorkspace,
            Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, Collection<WorkspaceVariableDTO> incomingWorkspaceVariables = null)
        {
            if (incomingWorkspaceVariables == null)
            {
                incomingWorkspaceVariables = new Collection<WorkspaceVariableDTO>();
            }
            if (incomingBoeTaskElements == null)
            {
                incomingBoeTaskElements = new Collection<BoeTaskElementDTO>();
            }
            if (clin == null)
            {
                throw new ArgumentNullException(nameof(clin));
            }
            if (fullWorkspace == null)
            {
                throw new ArgumentNullException(nameof(fullWorkspace));
            }

            List<BoeTaskElementDTO> TaskElements = new List<BoeTaskElementDTO>();
            List<int> VarIDs = new List<int>();

            // if the CLIN ID exists, get all ordinary variables that have this CLIN in a summed task variable
            // then for all returned ordinary variable IDs, get the boe task element/boe to update
            if (variableType == VariableType.Task)
            {
                VarIDs.AddRange(clin.BoeTaskVariableIds);
            }
            else
            {
                VarIDs.AddRange(clin.WorkspaceVariableIds);
            }

            foreach (int x in VarIDs)
            {
				Collection<BoeTaskElementDTO> tasks = this.RecalculateLaborWithVariable(x, variableType, fullWorkspace, incomingBoeTaskElements, incomingWorkspaceVariables);

                // add the task elements only if they do not exist in the current list
                TaskElements.AddRange(from t in tasks
                                      where !(from p in TaskElements
                                              select p.Id
                                             ).Contains(t.Id)
                                      select t);

            }

            return TaskElements;
        }

        /// <summary>
        /// Recalculate the BOE Labor given a task/workspace variable ID
        /// </summary>
        /// <param name="variableID">task/workspace variable ID</param>
        /// <param name="variableType">Variable type</param>
        /// <param name="fullWorkspace">workspace</param>
        /// <param name="incomingBoeTaskElements">Task elements associated with the </param>
        /// <returns>BOE Task Elements that need to be updated</returns>
        public Collection<BoeTaskElementDTO> RecalculateLaborWithVariable(int variableID, VariableType variableType, FullWorkspace fullWorkspace,
            Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, Collection<WorkspaceVariableDTO> incomingWorkspaceVariables = null)
        {
            if (incomingWorkspaceVariables == null)
            {
                incomingWorkspaceVariables = new Collection<WorkspaceVariableDTO>();
            }

            if (incomingBoeTaskElements == null)
            {
                incomingBoeTaskElements = new Collection<BoeTaskElementDTO>();
            }

            if (fullWorkspace == null)
            {
                throw new ArgumentNullException(nameof(fullWorkspace));
            }

            Collection<BoeTaskElementDTO> toReturn = new Collection<BoeTaskElementDTO>();
            HashSet<BoeTaskElementDTO> workspaceTaskElements = new HashSet<BoeTaskElementDTO>(fullWorkspace.TaskElements);
            incomingBoeTaskElements = incomingBoeTaskElements.ToCollection();

            List<int> BoeTaskElementIDs = new List<int>();

            // get all task element IDs that are match the inputted task/workspace variable ID
            if (variableType == VariableType.Task)
            {
                // find the task element associated with the task element variable
                int idOfTaskElement = (from te in workspaceTaskElements
                                       from v in te.OrdinaryVariables
                                       where v.Id == variableID
                                       select te.Id).FirstOrDefault();

                if (idOfTaskElement > 0)
                {
                    BoeTaskElementIDs.Add(idOfTaskElement);
                }
            }
            else
            {
                // find all the task elements associated with the workspace variable
                ICollection<int> idsOfTaskElementsAssociatedWithVariable = (from te in workspaceTaskElements
                                                                            where te.WorkspaceVariableIDs.Contains(variableID)
                                                                            select te.Id).Distinct().ToCollection();
                BoeTaskElementIDs.AddRange(idsOfTaskElementsAssociatedWithVariable);
            }

            foreach (int id in BoeTaskElementIDs)
            {
                // first, try to pull the task element from the incoming task elements 
                bool pulledFromIncomingTaskElements = true;
                BoeTaskElementDTO dto = incomingBoeTaskElements.FirstOrDefault(te => te.Id == id);
                if (dto == null)
                {
                    // grab it from the workspace instead
                    dto = workspaceTaskElements.FirstOrDefault(x => x.Id == id);
                    pulledFromIncomingTaskElements = false;
                } 

                // it turns out that the dto is being returned as the 'updatedBoeTask' so it's not really a new DTO
                BoeTaskElementDTO updatedBoeTask = this.RecalculateLaborWithTaskElement(dto, variableType, fullWorkspace, incomingBoeTaskElements);

                if (pulledFromIncomingTaskElements)
                {
                    // since it has been processed, remove it from the incoming so it's not processed again.
                    incomingBoeTaskElements.Remove(incomingBoeTaskElements.First(x => x.Id == dto.Id));
                }

                incomingBoeTaskElements.Add(updatedBoeTask);
                toReturn.Add(updatedBoeTask);
            }

            return toReturn;
        }

        /// <summary>
        /// Recalculate BOE Labor given a wbs ID
        /// </summary>
        /// <param name="wbsToCheck">The WBS to check.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <param name="fullWorkspace">The full workspace.</param>
        /// <param name="incomingBoeTaskElements">The incoming boe task elements.</param>
        /// <param name="incomingWorkspaceVariables">The incoming workspace variables.</param>
        /// <returns>
        /// list of BOEs to update
        /// </returns>
        
        public List<BoeTaskElementDTO> RecalculateLaborWithWBS(FullWbs wbsToCheck, VariableType variableType, FullWorkspace fullWorkspace,
            Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, Collection<WorkspaceVariableDTO> incomingWorkspaceVariables = null)
        {
            if (wbsToCheck == null)
            {
                throw new ArgumentNullException(nameof(wbsToCheck));
            }

            if (incomingWorkspaceVariables == null)
            {
                incomingWorkspaceVariables = new Collection<WorkspaceVariableDTO>();
            }

            if (incomingBoeTaskElements == null)
            {
                incomingBoeTaskElements = new Collection<BoeTaskElementDTO>();
            }

            if (fullWorkspace == null)
            {
                throw new ArgumentNullException(nameof(fullWorkspace));
            }

            List<BoeTaskElementDTO> TaskElements = new List<BoeTaskElementDTO>();
            List<int> TaskVarIDs = new List<int>();

            if (variableType == VariableType.Task)
            {
                TaskVarIDs.AddRange(wbsToCheck.TaskVariableIds);
            }
            else
            {
                TaskVarIDs.AddRange(wbsToCheck.WorkspaceVariableIds);
            }

            foreach (int x in TaskVarIDs)
            {
				Collection<BoeTaskElementDTO> tasks = this.RecalculateLaborWithVariable(x, variableType, fullWorkspace, incomingBoeTaskElements, incomingWorkspaceVariables);
                
                // add the task elements only if they do not exist in the current list
                TaskElements.AddRange(from t in tasks
                                      where !(from p in TaskElements
                                              select p.Id
                                             ).Contains(t.Id)
                                      select t);
            }

            // Handle parent WBS calculations
            FullWbs parentWBS = this.GetParentWBS(wbsToCheck, fullWorkspace);
            if (parentWBS != null)
            {
				List<BoeTaskElementDTO> returnWBS = this.RecalculateLaborWithWBS(parentWBS, variableType, fullWorkspace, incomingBoeTaskElements, incomingWorkspaceVariables);
                // add the task elements only if they do not exist in the current list
                TaskElements.AddRange(from t in returnWBS
                                      where !(from p in TaskElements
                                              select p.Id
                                             ).Contains(t.Id)
                                      select t);
            }

            return TaskElements;
        }

        /// <summary>
        /// Recalculate the BOE Labor given a task element.
        /// </summary>
        /// <param name="boeTaskElement">Task element to be recalculated.</param>
        /// <param name="variableType">Recalc using workspace or task variable.</param>
        /// <param name="fullWorkspace">Workspace object.</param>
        /// <param name="incomingBoeTaskElements">Task elements.</param>
        /// <param name="costPrecisionChanged">True if the cost precision has changed. This will only occur during a task element copy from a different workspace with a different cost precision.</param>
        /// <returns>Recalculated task element.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public BoeTaskElementDTO RecalculateLaborWithTaskElement(BoeTaskElementDTO boeTaskElement, VariableType variableType, FullWorkspace fullWorkspace,
            Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, bool costPrecisionChanged = false, bool hoursPrecisionChanged = false)
        {
            if (incomingBoeTaskElements == null)
            {
                incomingBoeTaskElements = new Collection<BoeTaskElementDTO>();
            }

            if (boeTaskElement == null)
            {
                throw new ArgumentNullException(nameof(boeTaskElement));
            }

            if (fullWorkspace == null)
            {
                throw new ArgumentNullException(nameof(fullWorkspace));
            }

            // find the boe associated with the task element
            FullBoe boeForTaskElement = fullWorkspace.Boes.FirstOrDefault(x => x.Id == boeTaskElement.BoeID);
            if (boeForTaskElement == null)
            {
                throw new ArgumentException("BOE with id of '" + boeTaskElement.BoeID + "' cannot be found in the workspace with id of '" + fullWorkspace.Id + "'.", nameof(fullWorkspace));
            }

            LaborSpreadRequest laborSpreadRequest = new LaborSpreadRequest();
            decimal totalHourSpread = 0;
            decimal totalPercentSpread = 0;

            // Recalculate MOQ Equation
            Collection<WorkspaceVariableDTO> workspaceVarsForTaskElement = fullWorkspace.WorkspaceVariables.Where(i => boeTaskElement.WorkspaceVariableIDs.Contains(i.Id)).ToCollection<WorkspaceVariableDTO>();

            // Data that will be passed into the parser
            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
            data.FillData(boeTaskElement.OrdinaryVariables, workspaceVarsForTaskElement, fullWorkspace);

            // update task variable values first so moq total will be accurate
            foreach (OrdinaryVariableDto taskVariable in boeTaskElement.OrdinaryVariables)
            {
                taskVariable.OrdinaryVariableValue = this._VarSelectBOEtoSumCalc.GetTaskVarLabelTotal(taskVariable, data);
                taskVariable.Updateable = UpdateType.Upsert;
            }

            string moqTotal = Parser.Calculate(boeTaskElement.MOQHoursEquation, boeTaskElement.OrdinaryVariables, workspaceVarsForTaskElement, this._VarSelectBOEtoSumCalc, data, fullWorkspace);
            decimal moqTotalDecimal = Convert.ToDecimal(moqTotal);

            HashSet<ResourceTypeDto> taskElementLabors = new HashSet<ResourceTypeDto>(boeTaskElement.taskElementLabors);

            foreach (ResourceTypeDto labortype in taskElementLabors)
            {
                if (labortype.SpreadType == SpreadType.Cost)
                {
                    if (labortype.SpreadCurveID != SpreadCurves.DiscreteCost)
                    {
                        // Costs are spread over a curve.
                        laborSpreadRequest.CurveID = labortype.SpreadCurveID;
                        laborSpreadRequest.StartDate = (DateTime)labortype.StartDate;
                        laborSpreadRequest.EndDate = (DateTime)labortype.EndDate;
                        laborSpreadRequest.HourSpread = labortype.ValueSpread.HasValue ? labortype.ValueSpread.Value : 0;

                        // Recalculate Labor Type Cost Spread. Adjust precision if necessary.
                        labortype.ValueSpread = labortype.ValueSpread.HasValue ? Utilities.AdjustPrecision(labortype.ValueSpread.Value, fullWorkspace.CostDecimalPrecision) : 0;
                        labortype.Updateable = UpdateType.Upsert;

                        // Recalculate Labor Spread & set the new spreads -> only when you aren't dealing with cost..
                        labortype.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(laborSpreadRequest, fullWorkspace.CostDecimalPrecision);

                        // Update UpdateType property
                        foreach (ResourceSpreadDto laborSpread in labortype.LaborSpreads)
                        {
                            laborSpread.Updateable = UpdateType.Upsert;
                        }
                    }
                    // If the old precision is 2 and was changed to 0 for a Discrete cost curve.
                    else if (costPrecisionChanged && fullWorkspace.CostDecimalPrecision == 0)
                    {
                        // Only update discrete spreads if the precision is changing from 2 to 0. 0 to 2 does not require spreads to be updated.
                        foreach (ResourceSpreadDto spread in labortype.LaborSpreads)
                        {
                            // Round each discrete spread to the new precision. Note this will likely change the eventual total cost.
                            decimal newSpreadValue = Utilities.AdjustPrecision(spread.LaborSpreadValue, fullWorkspace.CostDecimalPrecision);

                            spread.LaborSpreadValue = newSpreadValue;
                            spread.Updateable = UpdateType.Upsert;
                        }
                        // Sum up the newly adjusted spreads and update the total cost for the resource.
                        labortype.ValueSpread = labortype.LaborSpreads.Sum(s => s.LaborSpreadValue);
                        labortype.Updateable = UpdateType.Upsert;
                    }
                }
                else if (labortype.SpreadCurveID != SpreadCurves.DiscreteHours)
                {
					decimal PercentSpread = 0m;
                    decimal ValueSpread = 0;

                    if (boeTaskElement.TaskElementType.Equals(TaskElementType.Labor))
                    {
                        // if this is a Labor task element, check if Percent Spread is locked or Hour Spread is locked
                        //      if Percent Spread is locked, keep percent spread value as is and readjust the value spread
                        //      if Hour Spread is locked, calculate new percent spread and keep value spread
                        // else keep percent spread as is, recalculate value spread
                        if (labortype.PercentSpreadLocked)
                        {
                            PercentSpread = labortype.PercentSpread.HasValue ? labortype.PercentSpread.Value : 0;
                            ValueSpread = Utilities.AdjustPrecision(moqTotalDecimal * PercentSpread / 100, fullWorkspace.DecimalPrecision);
                        }
                        else if (labortype.HourSpreadLocked)
                        {
                            if (moqTotalDecimal != 0)
                            {
                                PercentSpread = Convert.ToDecimal(Math.Round((labortype.ValueSpread.Value / moqTotalDecimal * 100), MidpointRounding.AwayFromZero));
                            }
                            else
                            {
                                PercentSpread = 0;
                            }
                            ValueSpread = labortype.ValueSpread.Value;
                        }
                        else
                        {
                            PercentSpread = labortype.PercentSpread.HasValue ? labortype.PercentSpread.Value : 0;
                            ValueSpread = Utilities.AdjustPrecision(moqTotalDecimal * PercentSpread / 100, fullWorkspace.DecimalPrecision);
                        }
                    }
                    else
                    {
                        PercentSpread = labortype.PercentSpread.HasValue ? labortype.PercentSpread.Value : 0;
                        ValueSpread = Utilities.AdjustPrecision(moqTotalDecimal * PercentSpread / 100, fullWorkspace.DecimalPrecision);
                    }

                    laborSpreadRequest.CurveID = labortype.SpreadCurveID;
                    laborSpreadRequest.StartDate = (DateTime)labortype.StartDate;
                    laborSpreadRequest.EndDate = (DateTime)labortype.EndDate;

                    totalPercentSpread += PercentSpread;

                    // Recalculate Labor Type Hours Spread
                    labortype.ValueSpread = ValueSpread;
                    labortype.Updateable = UpdateType.Upsert; // set upsert property

                    totalHourSpread += labortype.ValueSpread.HasValue ? labortype.ValueSpread.Value : 0;

                    laborSpreadRequest.HourSpread = labortype.ValueSpread.HasValue ? labortype.ValueSpread.Value : 0;

                    // Recalculate Labor Spread & set the new spreads -> only when you aren't dealing with cost..
                    labortype.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(laborSpreadRequest, fullWorkspace.DecimalPrecision);

                    //Update UpdateType property
                    foreach (ResourceSpreadDto laborSpread in labortype.LaborSpreads)
                    {
                        laborSpread.Updateable = UpdateType.Upsert;
                    }

                    labortype.PercentSpread = PercentSpread;
                }
                else // only update the labor type % spread if its discrete hours
                {
                    if (hoursPrecisionChanged)
                    {
                        // Updating all the spreads 
                        foreach (ResourceSpreadDto spread in labortype.LaborSpreads)
                        {
                            // Round each discrete spread to the new precision. Note this will likely change the eventual total cost.
                            decimal newSpreadValue = Utilities.AdjustPrecision(spread.LaborSpreadValue, fullWorkspace.ResourceDecimalPrecision);

                            spread.LaborSpreadValue = newSpreadValue;
                            spread.Updateable = UpdateType.Upsert;
                        }
                        // Sum up the newly adjusted spreads and update the total cost for the resource.
                        labortype.ValueSpread = labortype.LaborSpreads.Sum(s => s.LaborSpreadValue);
                    }

                    if (moqTotalDecimal != 0)
                    {
                        labortype.PercentSpread = Math.Abs((Convert.ToDecimal((labortype.ValueSpread / moqTotalDecimal) * 100)));
                    }
                    else // to avoid divide by zero errors, set the % spread to 0 if this is about to occur
                    {
                        labortype.PercentSpread = 0;
                    }

                    totalHourSpread += labortype.ValueSpread.HasValue ? labortype.ValueSpread.Value : 0;
                    labortype.Updateable = UpdateType.Upsert; // set upsert property
                }
            }

            // Copied from BoeLaborControllerLogic.AdjustDeltaHours
            // if the total Percent Spread for this element is 100, but there is still a delta, need to change the hours spread for the last non-discrete labor type
            // so we can achieve 100% Percent Spread with no delta
            decimal deltaHours = Utilities.AdjustPrecision(totalHourSpread, fullWorkspace.DecimalPrecision) - moqTotalDecimal; 

            if (deltaHours != 0)
            {
                // only adjust labor types that aren't discrete and percent spread locked is locked
                ICollection<ResourceTypeDto> eligibleResourceEntries = taskElementLabors.Where(r => r.SpreadCurveID != SpreadCurves.DiscreteHours && r.SpreadType != SpreadType.Cost && r.PercentSpreadLocked).ToList();

                decimal totalSpreadPercentage = eligibleResourceEntries.Sum(r => r.PercentSpread.HasValue ? r.PercentSpread.Value : 0);
                if (totalSpreadPercentage == 100)
                {
                    decimal adjustment = (deltaHours < 0) ? Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(fullWorkspace.DecimalPrecision) : -1 * Utilities.MultiplicationFactorForCalculationsDueToPrecisionAdjustment(fullWorkspace.DecimalPrecision);  // adjust either up or down

                    while (deltaHours != 0)
                    {
                        foreach (ResourceTypeDto resourceEntry in eligibleResourceEntries)
                        {
                            if (deltaHours == 0)
                            {
                                break;
                            }

                            // apply adjustment to the current entry
                            decimal resourceHours = resourceEntry.ValueSpread.HasValue ? Utilities.AdjustPrecision(resourceEntry.ValueSpread.Value, fullWorkspace.DecimalPrecision) : 0;
                            resourceEntry.ValueSpread = resourceHours + adjustment;
                            resourceEntry.Updateable = UpdateType.Upsert;

                            // get labor spread request
                            laborSpreadRequest.CurveID = resourceEntry.SpreadCurveID;
                            laborSpreadRequest.StartDate = (DateTime)resourceEntry.StartDate;
                            laborSpreadRequest.EndDate = (DateTime)resourceEntry.EndDate;
                            laborSpreadRequest.HourSpread = resourceEntry.ValueSpread.HasValue ? resourceEntry.ValueSpread.Value : 0;

                            // Recalculate Labor Spread
                            // add new labor spread to labor type.
                            resourceEntry.LaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(laborSpreadRequest, fullWorkspace.DecimalPrecision);

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
            }

            boeTaskElement.LaborTypeWarningFlag = true;
            boeTaskElement.Updateable = UpdateType.Upsert;

            this.RecalculateLaborWithBoe(boeForTaskElement, variableType, fullWorkspace, incomingBoeTaskElements);

            return boeTaskElement;
        }

        /// <summary>
        /// Recalculate labor for a BOE
        /// </summary>
        /// <param name="boe"></param>
        
        public virtual Collection<BoeTaskElementDTO> RecalculateLaborWithBoe(FullBoe boe, VariableType variableType, FullWorkspace fullWorkspace,
            Collection<BoeTaskElementDTO> incomingBoeTaskElements = null, Collection<WorkspaceVariableDTO> incomingWorkspaceVariables = null)
        {
            if (incomingWorkspaceVariables == null)
            {
                incomingWorkspaceVariables = new Collection<WorkspaceVariableDTO>();
            }
            if (incomingBoeTaskElements == null)
            {
                incomingBoeTaskElements = new Collection<BoeTaskElementDTO>();
            }
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }
            if (fullWorkspace == null)
            {
                throw new ArgumentNullException(nameof(fullWorkspace));
            }

            List<BoeTaskElementDTO> TaskElements = new List<BoeTaskElementDTO>();

            List<int> VarIDs = new List<int>();

            // get the list of task variable IDs that contain a link to the inputted BOE ID
            if (variableType == VariableType.Task)
            {
                VarIDs.AddRange(boe.TaskVariableIds);
            }
            else
            {
                VarIDs.AddRange(boe.WorkspaceVariablesIdsForBoe);
            }


            // recalculate the labor types that contain the task variable ID
            foreach (int x in VarIDs)
            {
				Collection<BoeTaskElementDTO> returnTaskVar = this.RecalculateLaborWithVariable(x, variableType, fullWorkspace, incomingBoeTaskElements, incomingWorkspaceVariables);

                // add the task elements only if they do not exist in the current list
                TaskElements.AddRange(from t in returnTaskVar
                                        where !(from p in TaskElements
                                                select p.Id
                                                ).Contains(t.Id)
                                        select t);
            }

            // recalculate the CLIN that contains the CLIN ID
            if (boe.CLINID.HasValue)
            {
                ClinDTO clinAssociatedWithBoe = (from c in fullWorkspace.Clins
                                                    where c.Id == boe.CLINID
                                                    select c).First();

				List<BoeTaskElementDTO> returnClin = this.RecalculateLaborWithClin(this._Factory.CreateFullClin(clinAssociatedWithBoe), variableType, fullWorkspace, incomingBoeTaskElements, incomingWorkspaceVariables);

                // add the task elements only if they do not exist in the current list
                TaskElements.AddRange(from t in returnClin
                                        where !(from p in TaskElements
                                                select p.Id
                                                ).Contains(t.Id)
                                        select t);
            }

            //recalculate the labor type that contains the WBS ID
            if (boe.WBSID.HasValue)
            {
                FullWbs wbsAssociatedWithBoe = fullWorkspace.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);

				List<BoeTaskElementDTO> returnWBS = this.RecalculateLaborWithWBS(wbsAssociatedWithBoe, variableType, fullWorkspace, incomingBoeTaskElements, incomingWorkspaceVariables);

                // add the task elements only if they do not exist in the current list
                TaskElements.AddRange(from t in returnWBS
                                        where !(from p in TaskElements
                                                select p.Id
                                                ).Contains(t.Id)
                                        select t);

            }

            return TaskElements.ToCollection();
        }

        /// <summary>
        /// Get the parent WBS for a given WBS
        /// </summary>
        /// <param name="inWBS">WBS we want to find the parent for</param>
        /// <param name="fullWorkspace">workspace</param>
        /// <returns>parent WBS</returns>
        private FullWbs GetParentWBS(WbsDTO inWBS, FullWorkspace fullWorkspace)
        {
            if (inWBS == null)
            {
                throw new ArgumentNullException(nameof(inWBS));
            }

            // get all WBSs
            IReadOnlyCollection<FullWbs> AllWbs = fullWorkspace.WbsElements;

            // need to get all parent WBSs so we can then find the "right" one
            Collection<FullWbs> AllParentWbs = new Collection<FullWbs>();

            foreach (FullWbs wbs in AllWbs)
            {
                // do not include the WBS we are searching for
                if (inWBS.WbsNumber.StartsWith(wbs.WbsNumber) && inWBS.WbsNumber != wbs.WbsNumber)
                {
                    AllParentWbs.Add(wbs);
                }
            }

            FullWbs parentWbs = null;
            foreach (FullWbs wbs in AllParentWbs)
            {
                // the parent WBS will be the WBS with the longest WBS number
                if (parentWbs == null || wbs.WbsNumber.Length > parentWbs.WbsNumber.Length)
                {
                    parentWbs = wbs;
                }
            }
            return parentWbs;
        }

        /// <summary>
        /// Adjusts the labor totals.
        /// </summary>
        /// <param name="boeTaskElement">The boe task element.</param>
        /// <param name="MOQTotal">The moq total.</param>
        /// <exception cref="System.ArgumentNullException">inBoeTaskElement</exception>
        public void AdjustLaborTotals(BoeTaskElementDTO boeTaskElement, decimal MOQTotal)
        {
            if (boeTaskElement == null)
            {
                throw new ArgumentNullException(nameof(boeTaskElement));
            }

            decimal totalPercent = 0;
            decimal totalHours = 0;

            Collection<ResourceTypeDto> nonDiscreteHoursResources = boeTaskElement.taskElementLabors.Where(r => r.SpreadCurveID.HasValue && r.SpreadCurveID.Value != SpreadCurves.DiscreteHours && r.SpreadType != SpreadType.Cost).ToCollection();

            foreach (ResourceTypeDto laborType in nonDiscreteHoursResources)
            {
                totalHours += laborType.ValueSpread.HasValue ? laborType.ValueSpread.Value : 0;
                totalPercent += laborType.PercentSpread.HasValue ? laborType.PercentSpread.Value : 0;
            }

            decimal delta = MOQTotal - totalHours;

            if (totalPercent == 100 && delta != 0)
            {
                int count = nonDiscreteHoursResources.Count;

                // always perform calculations from the last non-discrete LT to the first non-discrete LT
                for (int x = count; x > 0; x--)
                {
                    if (delta == 0)
                    {
                        break;
                    }
                    //always grab the LT from the bottom of the list
                    ResourceTypeDto toAdjust = nonDiscreteHoursResources[x - 1];

                    // need a way to only take as much away from valuespread that would equal 0
                    decimal amountToAdjust = 0;

                    if (toAdjust.ValueSpread.Value * (toAdjust.ValueSpread.Value + delta) < 1)
                    {
                        amountToAdjust = -toAdjust.ValueSpread.Value;
                    }
                    else
                    {
                        amountToAdjust = delta;
                    }

                    delta = delta - amountToAdjust;
                    toAdjust.ValueSpread = toAdjust.ValueSpread.Value + amountToAdjust;
                }
            }
        }

        #region Helper Methods

        public static long numberOfMonthsBetweenTwoMMYYYYDates(DateTime start, DateTime end)
        {
            long toReturn = 0;

            toReturn = ((end.Year - start.Year) * 12) + end.Month - start.Month;

            return toReturn;
        }

        #endregion
    }
}
