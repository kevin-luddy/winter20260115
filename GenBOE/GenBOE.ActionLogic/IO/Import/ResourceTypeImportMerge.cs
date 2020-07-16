// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.Common.MOQ;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    /// <summary>
    /// Merges import ResourceTypeDto and BoeLaborSpread object with currently stored data
    /// </summary>
    public class ResourceTypeImportMerge
    {
        private IVariableSelectBOEtoSumCalculation _variableSelectBoeToSumCalculator;
        private ICustomFieldValueDTODataLoader _customFieldValueLoader;

        public ResourceTypeImportMerge(IVariableSelectBOEtoSumCalculation variableSelectCalculator, ICustomFieldValueDTODataLoader customFieldValueLoader)
        {
            this._variableSelectBoeToSumCalculator = variableSelectCalculator;
            this._customFieldValueLoader = customFieldValueLoader;
        }

        public Collection<ResourceTypeDto> ExtractLaborTypesFromOfflineImport(WorkofflineImportedTaskElement importedTaskElement)
        {
            Collection<ResourceTypeDto> extractedData = new Collection<ResourceTypeDto>();
            if (importedTaskElement == null)
            {
                return extractedData;
            }

            
            foreach (WorkofflineImportedResourceType resource in importedTaskElement.ImportedResourceTypes)
            {

                if (resource.ImportTypes.Contains(LaborTypeImportResult.UpdateLaborType) || resource.ImportTypes.Contains(LaborTypeImportResult.AddLaborType))
                {
                    resource.LaborSpreads = new Collection<ResourceSpreadDto>();
                    resource.CloneImportedCustomFieldsToContainer();
                    foreach (ResourceSpreadDto spread in resource.ImportedResourceSpreads)
                    {
                        resource.LaborSpreads.Add(spread);
                    }
                    extractedData.Add(resource);
                }
            }

            return extractedData;
        }

        public ResourceTypeDto MergeLaborTypeWithData(
            ResourceTypeDto importedLaborType,
            BoeTaskElementDTO taskElement,
            bool updateAllLaborTypeData,
            decimal moqValue,
            WorkspaceDTO workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            //If the BoeTaskElementDTO is null, throw exception.
            //  Otherwise there is no object to associate the ResourceTypeDto to.
            if (taskElement == null)
            {
                throw new ArgumentNullException(nameof(taskElement), "taskElement cannot be null");
            }
            //ensure the task element has a valid BOEID, subsequent DB inserts will eventually 
            //  fail otherwise
            if (taskElement.BoeID <= 0)
            {
                throw new ArgumentException("BOEID must be valid for taskElement");
            }

            ResourceTypeDto originalLaborType = null;

            //get a previously saved Labor type if it exists and the BoeTaskElement is valid
            originalLaborType = taskElement.taskElementLabors.FirstOrDefault(l => l.Id == importedLaborType.Id);

            //create a new ResourceTypeDto if an existing one could not be discovered.
            if (originalLaborType == null)
            {
                originalLaborType = new ResourceTypeDto();
                originalLaborType.Id = importedLaborType.Id;
                originalLaborType.BoeID = taskElement.BoeID;
            }
            originalLaborType.Updateable = UpdateType.Upsert;

            if (updateAllLaborTypeData)
            {
                originalLaborType.StartDateValue = importedLaborType.StartDateValue.Normalize();
                originalLaborType.EndDateValue = importedLaborType.EndDateValue.Normalize();
                originalLaborType.SpreadCurveID = importedLaborType.SpreadCurveID;

                if (importedLaborType.ResourceID.HasValue)
                {
                    originalLaborType.ResourceID = importedLaborType.ResourceID;
                }
                if (importedLaborType.PerformingOrgID.HasValue)
                {
                    originalLaborType.PerformingOrgID = importedLaborType.PerformingOrgID;
                }

                originalLaborType.WBSID = importedLaborType.WBSID;
                originalLaborType.CLINID = importedLaborType.CLINID;
                originalLaborType.HourSpreadLocked = importedLaborType.HourSpreadLocked;
                originalLaborType.PercentSpreadLocked = importedLaborType.PercentSpreadLocked;
                originalLaborType.SpreadType = importedLaborType.SpreadType;

                #region Percent spread and hours spread

                SpreadCurves importedCurveType = (SpreadCurves)importedLaborType.SpreadCurveID;

                if (importedLaborType.SpreadType == SpreadType.Hours)
                {
                    //If a Percent Spread was provided, this value is used first to calculate Value
                    if (importedLaborType.PercentSpread.HasValue && importedLaborType.PercentSpread.Value != 0m)
                    {
                        originalLaborType.PercentSpread = importedLaborType.PercentSpread;
                        originalLaborType.ValueSpread = Utilities.AdjustPrecision(((moqValue * originalLaborType.PercentSpread.Value) / 100m), workspace.DecimalPrecision);
                    }
                    else if (importedLaborType.ValueSpread.HasValue && importedLaborType.ValueSpread.Value != 0L)
                    {
                        originalLaborType.PercentSpread = (moqValue == 0m) ? 0m : decimal.Round((100m * importedLaborType.ValueSpread.Value) / moqValue, 3, MidpointRounding.AwayFromZero);
                        originalLaborType.ValueSpread = importedLaborType.ValueSpread;
                    }
                    else
                    {
                        originalLaborType.PercentSpread = 0m;
                        originalLaborType.ValueSpread = 0L;
                    }
                }
                else if (importedLaborType.SpreadType == SpreadType.Cost && importedCurveType != SpreadCurves.DiscreteCost)
                {
                    // Cost is spread over a curve.
                    originalLaborType.PercentSpread = 0m;
                    originalLaborType.ValueSpread = importedLaborType.ValueSpread;
                }

                #endregion

                #region Labor spread distribution

                int decimalPrecision = importedLaborType.SpreadType == SpreadType.Hours ? workspace.DecimalPrecision : workspace.CostDecimalPrecision;

                // If the imported spread curve type is discrete, pull the curves in explicitly.
                if (importedCurveType == SpreadCurves.DiscreteCost || importedCurveType == SpreadCurves.DiscreteHours)
                {
                    originalLaborType.LaborSpreads = this.MergeLaborSpreads(importedLaborType.LaborSpreads, taskElement.BoeID);

                    decimal spreadValueTotal = originalLaborType.LaborSpreads.Sum(s => s.LaborSpreadValue);
                    if (importedCurveType == SpreadCurves.DiscreteCost)
                    {
                        originalLaborType.ValueSpread = Utilities.AdjustPrecision(spreadValueTotal, decimalPrecision);
                        originalLaborType.PercentSpread = 0m;
                    }
                    else // DiscreteHours
                    {
                        originalLaborType.ValueSpread = spreadValueTotal;
                        originalLaborType.PercentSpread = (moqValue == 0m) ? 0m : decimal.Round((100m * spreadValueTotal) / moqValue, 3, MidpointRounding.AwayFromZero);
                    }
                }
                //otherwise recalculate the individual spread values based on the curvespecified
                else
                {
                    LaborSpreadRequest lrs = new LaborSpreadRequest();
                    lrs.CurveID = importedCurveType;
                    lrs.EndDate = importedLaborType.EndDateValue;
                    lrs.StartDate = importedLaborType.StartDateValue;
                    lrs.HourSpread = originalLaborType.ValueSpread.Value;

                    Collection<ResourceSpreadDto> calculatedLaborSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(lrs, decimalPrecision);

                    // Still need to "merge" new calculated values into the resource.
                    originalLaborType.LaborSpreads = this.MergeLaborSpreads(calculatedLaborSpreads, taskElement.BoeID);
                }

                #endregion

                IList<CustomFieldContainerFieldValueMapping> resourceCustomFields = this._customFieldValueLoader.GetResourceCustomFieldContainerFieldValueMappings(originalLaborType.Id);
                originalLaborType.CustomFieldValueContainers.Merge(importedLaborType.CustomFieldValueContainers, resourceCustomFields);
            }

            return originalLaborType;
        }

        /// <summary>
        /// Creates new spreads from imported or computed spreads.
        /// </summary>
        /// <param name="importedSpreads">Imported spreads for a resource.</param>
        /// <param name="boeIdForSpreads">BOE Id for the spread.</param>
        /// <returns>Newly created spread from import.</returns>
        private Collection<ResourceSpreadDto> MergeLaborSpreads(Collection<ResourceSpreadDto> importedSpreads, int boeIdForSpreads)
        {
            Collection<ResourceSpreadDto> modifiedSpreads = new Collection<ResourceSpreadDto>();
            int newSpreadIdValueCounter = -1;

            foreach (ResourceSpreadDto importedRecord in importedSpreads)
            {
                //The Expectation is that resource spreads will be cleared and recreated every LaborType is updated.
                //  Instead of merging data, create all new Resource Spreads from the imported data.
                ResourceSpreadDto ls = new ResourceSpreadDto();
                ls.Id = newSpreadIdValueCounter;
                ls.BoeID = boeIdForSpreads;
                ls.UpdateDate = DateTime.Now;
                newSpreadIdValueCounter--;
                ls.Updateable = UpdateType.Upsert;
                ls.LaborSpreadDate = importedRecord.LaborSpreadDate;
                ls.LaborSpreadValue = importedRecord.LaborSpreadValue;

                modifiedSpreads.Add(ls);
            }

            return modifiedSpreads;
        }

        /// <summary>
        /// Calculate the total hours for the MOQ equation
        /// </summary>
        /// <param name="taskElement">Task element</param>
        /// <param name="workspace">Workspace</param>
        /// <returns>MOQ total hours</returns>
        public decimal GetMOQTotalHours(BoeTaskElementDTO taskElement, FullWorkspace workspace)
        {
            if (taskElement == null)
            {
                throw new ArgumentNullException(nameof(taskElement));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            //Need to get the MOQ Equation value to correctly calculate spread values
            string moqStringValue = string.Empty;

            try
            {
                Collection<WorkspaceVariableDTO> workspaceVars = workspace.WorkspaceVariables.Where(x => taskElement.WorkspaceVariableIDs.Contains(x.Id)).ToCollection();
                Collection<OrdinaryVariableDto> ordinaryVars = taskElement.OrdinaryVariables.ToCollection();
                
                DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                data.FillData(ordinaryVars, workspaceVars, workspace);
                
                moqStringValue = Parser.Calculate(taskElement.MOQHoursEquation,
                    taskElement.OrdinaryVariables,
                    workspace.WorkspaceVariables.ToList(), this._variableSelectBoeToSumCalculator,
                    data, 
                    workspace);
            }
            catch (FormatException) //equation contains an undefined value
            {
                moqStringValue = string.Empty;
            }

            decimal moqValue = default(decimal);
            if (!decimal.TryParse(moqStringValue, out moqValue))
            {
                moqValue = default(decimal);
            }

            return moqValue;
        }

        /// <summary>
        /// In cases where the total hours DOES NOT divide evenly among the number of percent-locked auto-calculated (non-discrete)
        /// resource entries, we need to adjust (+/-1) the hour spread values for a subset of those entries so that the overall
        /// delta is corrected to zero.
        /// </summary>
        /// <param name="resources">Resource labor types</param>
        /// <param name="moqTotalHours">The total hours needing to be allocated</param>
        /// <returns>The resource entries whose hours were adjusted</returns>
        /// <seealso cref="GenBOE.ActionLogic.ControllerLogic.BOELaborControllerLogic"/>
        public ICollection<ResourceTypeDto> AdjustDeltaHours(ICollection<ResourceTypeDto> resources, decimal moqTotalHours, WorkspaceDTO ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            IList<ResourceTypeDto> adjustedResourceEntries = new List<ResourceTypeDto>();

            ICollection<ResourceTypeDto> hourValuedImportedResources = resources.Where(r => r.SpreadCurveID.HasValue && r.SpreadType != SpreadType.Cost).ToList();

            decimal actualTotalHours = hourValuedImportedResources.Where(r => r.ValueSpread.HasValue).Sum(r => r.ValueSpread.Value);
            decimal deltaHours = actualTotalHours - moqTotalHours;

            if (deltaHours != 0)
            {
                ICollection<ResourceTypeDto> percentAdjustableImportedResources = hourValuedImportedResources.Where(r => r.SpreadCurveID.HasValue && r.SpreadCurveID.Value != SpreadCurves.DiscreteHours && r.SpreadCurveID.Value != SpreadCurves.DiscreteCost && r.PercentSpread.HasValue && r.PercentSpread.Value > 0m).ToList();

                decimal totalSpreadPercentage = percentAdjustableImportedResources.Sum(r => r.PercentSpread.HasValue ? r.PercentSpread.Value : 0m);

                if (totalSpreadPercentage == 100m)  // 100%-allocation: adjustment needed
                {
                    decimal adjustment = deltaHours*(-1);  // adjust either up or down

                    while (deltaHours != 0)
                    {
                        foreach (ResourceTypeDto resourceEntry in percentAdjustableImportedResources)
                        {
                            if (deltaHours == 0)
                            {
                                break;
                            }

                            // apply adjustment to the current entry
                            decimal resourceHours = resourceEntry.ValueSpread.HasValue ? resourceEntry.ValueSpread.Value : 0;
                            resourceEntry.ValueSpread = resourceHours + adjustment;
                            adjustedResourceEntries.Add(resourceEntry);

                            // update the delta
                            deltaHours += adjustment;
                        }
                    }
                }
            }

            // if resource hours were changed, then recalculate the spread distribution
            // Note: The resources here will ALWAYS be hour-based/curve-derived.
            if (adjustedResourceEntries.Any())
            {
                foreach (ResourceTypeDto adjustedResource in adjustedResourceEntries)
                {
                    Collection<ResourceSpreadDto> recalculatedSpreads = SpreadCurve.CalculateLaborSpreadsBasedOnCurve(new LaborSpreadRequest
                    {
                        CurveID = adjustedResource.SpreadCurveID,
                        EndDate = adjustedResource.EndDateValue.Normalize(),
                        StartDate = adjustedResource.StartDateValue.Normalize(),
                        HourSpread = adjustedResource.ValueSpread.Value
                    }, ws.DecimalPrecision);
                    
                    adjustedResource.LaborSpreads = this.MergeLaborSpreads(recalculatedSpreads, adjustedResource.BoeID);
                }
            }

            return adjustedResourceEntries;
        }
    }
}
