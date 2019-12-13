// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.Calculations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    public class VariableSelectBOEtoSumCalculation : IVariableSelectBOEtoSumCalculation
    {
        #region Private Fields and Constructor

        protected IPerformingOrgDTODataLoader PerfOrgLoader { get; }

        public VariableSelectBOEtoSumCalculation(IPerformingOrgDTODataLoader perfOrgLoader)
        {
            this.PerfOrgLoader = perfOrgLoader;
        }

        #endregion

        /// <summary>
        /// This method checks to see if the Workspace var has been cached, if not, it calculates it
        /// </summary>
        /// <param name="workspaceVar">Workspace variable</param>
        /// <param name="dataForSumOfBoeCalc">The data for sum of boe calculate.</param>
        /// <returns>
        /// Its value
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// inWorkspaceVar
        /// or
        /// dataForSumOfBoeCalc
        /// </exception>
        public virtual decimal GetWorkspaceVarLabelTotal(WorkspaceVariableDTO workspaceVar, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc)
        {
            if (workspaceVar == null)
            {
                throw new ArgumentNullException(nameof(workspaceVar));
            }
            if (dataForSumOfBoeCalc == null)
            {
                throw new ArgumentNullException(nameof(dataForSumOfBoeCalc));
            }

            decimal toReturn = 0;

            // extra check that this is indeed a SumOfBOEs workspace variable
            if (workspaceVar.ValueType.Equals(VarValueType.SumOfBOEs))
            {
                // for everything that is selected, get the total hours
                foreach (SelectBOEsToSum selectBoe in workspaceVar.SelectedBOEsToSum)
                {
                    // get selected BOE total hours
                    if (selectBoe.BoeID.HasValue)
                    {
                        toReturn = toReturn + Convert.ToDecimal(this.GetTotalBasedOnBoeID(selectBoe.BoeID.Value, workspaceVar.SumVariableResourceTypeIDs, dataForSumOfBoeCalc));
                    }

                    // get selected CLINs
                    else if (selectBoe.CLINID.HasValue)
                    {
                        toReturn = toReturn + Convert.ToDecimal(this.GetTotalBasedOnCLINID(selectBoe.CLINID.Value, workspaceVar.SumVariableResourceTypeIDs, dataForSumOfBoeCalc));
                    }

                    // get selected WBSs
                    else if (selectBoe.WBSID.HasValue)
                    {
                        toReturn = toReturn + Convert.ToDecimal(this.GetTotalBasedOnWBSID(selectBoe.WBSID.Value, workspaceVar.SumVariableResourceTypeIDs, dataForSumOfBoeCalc));
                    }
                }
            }
            else
            {
                toReturn = workspaceVar.WorkspaceVariableValue;
            }

            return toReturn;  // decimal variable values should NOT be adjusted for decimal precision
        }

        /// <summary>
        /// Get the total value of Boes that are associated with a WBS ID
        /// </summary>
        /// <param name="wbsId">wbs id</param>
        /// <param name="sumVariableResourceTypes">The sum variable resource types.</param>
        /// <param name="dataForSumOfBoeCalc">The data for sum of boe calculate.</param>
        /// <returns>total</returns>
        /// <exception cref="System.ArgumentNullException">inSumVariableResourceTypes or dataForSumOfBoeCalc</exception>
        public virtual decimal GetTotalBasedOnWBSID(int wbsId, Collection<int> sumVariableResourceTypes, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc)
        {
            if (sumVariableResourceTypes == null)
            {
                throw new ArgumentNullException(nameof(sumVariableResourceTypes));
            }

            if (dataForSumOfBoeCalc == null)
            {
                throw new ArgumentNullException(nameof(dataForSumOfBoeCalc));
            }

            decimal toReturn = 0;

            List<int> nestedBOEs = dataForSumOfBoeCalc.WbsIdToBoeIds[wbsId].ToList();

            object LOCK = new object();
            Collection<decimal> tempResults = new Collection<decimal>();

            if (nestedBOEs.Any())
            {
                Parallel.ForEach(nestedBOEs, new ParallelOptions { MaxDegreeOfParallelism = 5 }, id =>
                {
                    decimal number = this.GetTotalBasedOnBoeID(id, sumVariableResourceTypes, dataForSumOfBoeCalc);

                    lock (LOCK)
                    {
                        tempResults.Add(number);
                    }
                });

                toReturn = tempResults.Sum();
            }

            return toReturn;  // decimal variable values should NOT be adjusted for decimal precision
        }

        /// <summary>
        /// Get the total value of Boes that are associated with a CLIN ID
        /// </summary>
        /// <param name="clinId">clin id</param>
        /// <param name="sumVariableResourceTypes">The sum variable resource types.</param>
        /// <param name="dataForSumOfBoeCalc">The data for sum of boe calculate.</param>
        /// <returns>total</returns>
        /// <exception cref="System.ArgumentNullException">sumVariableResourceTypes or dataForSumOfBoeCalc</exception>
        public virtual decimal GetTotalBasedOnCLINID(int clinId, Collection<int> sumVariableResourceTypes, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc)
        {
            if (sumVariableResourceTypes == null)
            {
                throw new ArgumentNullException(nameof(sumVariableResourceTypes));
            }

            if (dataForSumOfBoeCalc == null)
            {
                throw new ArgumentNullException(nameof(dataForSumOfBoeCalc));
            }

            decimal toReturn = 0;

            // for the given CLIN, find all BOEs that use that CLIN and return the total hours
            List<int> boeIds = dataForSumOfBoeCalc.ClinIdToBoeIds.ContainsKey(clinId) ? dataForSumOfBoeCalc.ClinIdToBoeIds[clinId] : new List<int>(0);

            object LOCK = new object();
            Collection<decimal> tempResults = new Collection<decimal>();

            if (boeIds.Any())
            {
                Parallel.ForEach(boeIds, new ParallelOptions { MaxDegreeOfParallelism = 5 }, id =>
                {
                    decimal number = this.GetTotalBasedOnBoeID(id, sumVariableResourceTypes, dataForSumOfBoeCalc);

                    lock (LOCK)
                    {
                        tempResults.Add(number);
                    }
                });

                toReturn = tempResults.Sum();
            }

            return toReturn;  // decimal variable values should NOT be adjusted for decimal precision
        }

        /// <summary>
        /// Get the total value of Boes that are associated with a BOE ID
        /// </summary>
        /// <param name="boeId">The boe identifier.</param>
        /// <param name="sumVariableResourceTypes">The sum variable resource types.</param>
        /// <param name="dataForSumOfBoeCalc">The data for sum of boe calculate.</param>
        /// <returns>total</returns>
        /// <exception cref="System.ArgumentNullException">inSumVariableResourceTypes or dataForSumOfBoeCalc</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public virtual decimal GetTotalBasedOnBoeID(int boeId, Collection<int> sumVariableResourceTypes, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc)
        {
            if (sumVariableResourceTypes == null)
            {
                throw new ArgumentNullException(nameof(sumVariableResourceTypes));
            }

            if (dataForSumOfBoeCalc == null)
            {
                throw new ArgumentNullException(nameof(dataForSumOfBoeCalc));
            }

            decimal toReturn = 0;

            List<BoeTaskElementDTO> boeTaskElements = dataForSumOfBoeCalc.BoeIdToTaskElements.ContainsKey(boeId) ? dataForSumOfBoeCalc.BoeIdToTaskElements[boeId] : new List<BoeTaskElementDTO>();
            HashSet<ResourceDTO> resources = dataForSumOfBoeCalc.BoeIdToResources.ContainsKey(boeId) ? new HashSet<ResourceDTO>(dataForSumOfBoeCalc.BoeIdToResources[boeId]) : new HashSet<ResourceDTO>();

            HashSet<int> dsToSearch = new HashSet<int>(resources.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor && x.Segment.Equals(SegmentType.DS)).Select(x => x.Id));
            HashSet<int> esToSearch = new HashSet<int>(resources.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor && x.Segment.Equals(SegmentType.ES)).Select(x => x.Id));
            HashSet<int> tsToSearch = new HashSet<int>(resources.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor && x.Segment.Equals(SegmentType.TS)).Select(x => x.Id));
            HashSet<int> lsToSearch = new HashSet<int>(resources.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor && x.Segment.Equals(SegmentType.LS)).Select(x => x.Id));
            HashSet<int> iwtaToSearch = new HashSet<int>(resources.Where(x => x.ElementOfCost == ElementOfCostType.IWTA).Select(x => x.Id));
            HashSet<int> subToSearch = new HashSet<int>(resources.Where(x => x.ElementOfCost == ElementOfCostType.Sub).Select(x => x.Id));

            HashSet<int> variableResourceTypes = new HashSet<int>(sumVariableResourceTypes);

            foreach (BoeTaskElementDTO boeTask in boeTaskElements)
            {
                foreach (ResourceTypeDto labor in boeTask.taskElementLabors)
                {
                    if (labor.ResourceID.HasValue && labor.LaborSpreads.Any() && labor.SpreadType == SpreadType.Hours)
                    {
                        if (variableResourceTypes.Contains((int)SumVariableResourceType.DSLabor) && dsToSearch.Contains(labor.ResourceID.Value))
                        {
                            toReturn = toReturn + labor.LaborSpreads.Sum(x => x.LaborSpreadValue);
                        }
                        else if (variableResourceTypes.Contains((int)SumVariableResourceType.ESLabor) && esToSearch.Contains(labor.ResourceID.Value))
                        {
                            toReturn = toReturn + labor.LaborSpreads.Sum(x => x.LaborSpreadValue);
                        }
                        else if (variableResourceTypes.Contains((int)SumVariableResourceType.TSLabor) && tsToSearch.Contains(labor.ResourceID.Value))
                        {
                            toReturn = toReturn + labor.LaborSpreads.Sum(x => x.LaborSpreadValue);
                        }
                        else if (variableResourceTypes.Contains((int)SumVariableResourceType.LSLabor) && lsToSearch.Contains(labor.ResourceID.Value))
                        {
                            toReturn = toReturn + labor.LaborSpreads.Sum(x => x.LaborSpreadValue);
                        }
                        else if (variableResourceTypes.Contains((int)SumVariableResourceType.LOEIWTA) && (boeTask.TaskElementType == TaskElementType.Labor) && iwtaToSearch.Contains(labor.ResourceID.Value))
                        {
                            toReturn = toReturn + labor.LaborSpreads.Sum(x => x.LaborSpreadValue);
                        }
                        else if (variableResourceTypes.Contains((int)SumVariableResourceType.LOESub) && (boeTask.TaskElementType == TaskElementType.Labor) && subToSearch.Contains(labor.ResourceID.Value))
                        {
                            toReturn = toReturn + labor.LaborSpreads.Sum(x => x.LaborSpreadValue);
                        }
                    }
                }
            }

            return toReturn;  // decimal variable values should NOT be adjusted for decimal precision
        }

        /// <summary>
        /// Get task variable total based on what's currently saved in the database
        /// </summary>
        /// <param name="taskVar">task variable</param>
        /// <param name="dataForSumOfBoeCalc">The data for sum of boe calculation.</param>
        /// <returns>total</returns>
        public virtual decimal GetTaskVarLabelTotal(OrdinaryVariableDto taskVar, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc)
        {
            if (taskVar == null)
            {
                throw new ArgumentNullException(nameof(taskVar));
            }

            if (dataForSumOfBoeCalc == null)
            {
                throw new ArgumentNullException(nameof(dataForSumOfBoeCalc));
            }

            decimal toReturn = 0;

            // extra check that this is indeed a SumOfBOEs task variable
            if (taskVar.ValueType.Equals(VarValueType.SumOfBOEs))
            {
                // for everything selected, get the hours
                foreach (SelectBOEsToSum selectBoe in taskVar.SelectedBOEsToSum)
                {
                    // if a BOE ID was selected, get the total hours from the boe task
                    if (selectBoe.BoeID.HasValue)
                    {
                        toReturn += Convert.ToDecimal(this.GetTotalBasedOnBoeID(selectBoe.BoeID.Value, taskVar.SumVariableResourceTypeIDs.ToCollection(), dataForSumOfBoeCalc));
                    }

                    // if a WBS ID was selected, get all BOEs within the WBS
                    else if (selectBoe.WBSID.HasValue)
                    {
                        toReturn += Convert.ToDecimal(this.GetTotalBasedOnWBSID(selectBoe.WBSID.Value, taskVar.SumVariableResourceTypeIDs.ToCollection(), dataForSumOfBoeCalc));
                    }

                    // if a CLIN ID was selected, get all BOEs within the CLIN
                    else if (selectBoe.CLINID.HasValue)
                    {
                        toReturn += Convert.ToDecimal(this.GetTotalBasedOnCLINID(selectBoe.CLINID.Value, taskVar.SumVariableResourceTypeIDs.ToCollection(), dataForSumOfBoeCalc));
                    }
                }
            }
            else
            {
                toReturn = taskVar.OrdinaryVariableValue ?? 0;
            }

            return toReturn;  // decimal variable values should NOT be adjusted for decimal precision
        }
    }
}