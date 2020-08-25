// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.Calculations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    public class VariableSelectBOEtoSumCalculationSpaceSystems : VariableSelectBOEtoSumCalculation
    {
        public VariableSelectBOEtoSumCalculationSpaceSystems(IPerformingOrgDTODataLoader perfOrgLoader)
            : base(perfOrgLoader)
        {

        }

        /// <summary>
        /// Get the total value of Boes that are associated with a BOE ID
        /// </summary>
        /// <param name="boeId">The boe identifier.</param>
        /// <param name="sumVariableResourceTypes">The in sum variable resource types.</param>
        /// <param name="dataForSumOfBoeCalc">The data for sum of boe calculate.</param>
        /// <returns>
        /// total
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// inSumVariableResourceTypes
        /// or
        /// dataForSumOfBoeCalc
        /// </exception>
        public override decimal GetTotalBasedOnBoeID(int boeId, Collection<int> sumVariableResourceTypes, DataClassForSumOfBOEsCalculation dataForSumOfBoeCalc)
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

            ICollection<ResourceDTO> resources = dataForSumOfBoeCalc.BoeIdToResources.ContainsKey(boeId) ? dataForSumOfBoeCalc.BoeIdToResources[boeId] : new List<ResourceDTO>();

            HashSet<int> sscToSearch = new HashSet<int>(resources.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor && x.Segment.Equals(SegmentType.SSC)).Select(x => x.Id));
            HashSet<int> iwtaToSearch = new HashSet<int>(resources.Where(x => x.ElementOfCost == ElementOfCostType.IWTA).Select(x => x.Id));
            HashSet<int> subToSearch = new HashSet<int>(resources.Where(x => x.ElementOfCost == ElementOfCostType.Sub).Select(x => x.Id));

            foreach (BoeTaskElementDTO boeTask in boeTaskElements)
            {
                foreach (ResourceTypeDto labor in boeTask.taskElementLabors)
                {
                    if (labor.ResourceID.HasValue && labor.LaborSpreads.Any() && labor.SpreadType == SpreadType.Hours)
                    {
                        if (sumVariableResourceTypes.Contains((int)SumVariableResourceType.SSCLMLabor) && sscToSearch.Contains(labor.ResourceID.Value))
                        {
                            toReturn = toReturn + labor.LaborSpreads.Sum(x => x.LaborSpreadValue);
                        }
                        else if (sumVariableResourceTypes.Contains((int)SumVariableResourceType.SSCLOEIWTA) && (boeTask.TaskElementType == TaskElementType.Labor) && iwtaToSearch.Contains(labor.ResourceID.Value))
                        {
                            toReturn = toReturn + labor.LaborSpreads.Sum(x => x.LaborSpreadValue);
                        }
                        else if (sumVariableResourceTypes.Contains((int)SumVariableResourceType.SSCLOESub) && (boeTask.TaskElementType == TaskElementType.Labor) && subToSearch.Contains(labor.ResourceID.Value))
                        {
                            toReturn = toReturn + labor.LaborSpreads.Sum(x => x.LaborSpreadValue);
                        }
                    }
                }
            }

            return toReturn;
        }
    }
}
