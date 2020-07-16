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

    /// <summary>
    /// Boes to sum calculation
    /// </summary>
    public class VariableSelectBOEtoSumCalculationMST : VariableSelectBOEtoSumCalculation
    {
        public VariableSelectBOEtoSumCalculationMST(IPerformingOrgDTODataLoader perfOrgLoader)
            : base(perfOrgLoader)
        {
            // Nothing to do here
        }

        /// <summary>
        /// Get the total value of Boes that are associated with a BOE ID
        /// </summary>
        /// <param name="boeId">The BOE ID</param>
        /// <param name="sumVariableResourceTypes">The resource types</param>
        /// <param name="dataForSumOfBoeCalc">The data used for the calculation</param>
        /// <returns>The total</returns>
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

            ICollection<ResourceDTO> Resources = dataForSumOfBoeCalc.BoeIdToResources.ContainsKey(boeId) ? dataForSumOfBoeCalc.BoeIdToResources[boeId] : new List<ResourceDTO>();

            HashSet<int> MSTToSearch = new HashSet<int>(Resources.Where(x => x.ElementOfCost == ElementOfCostType.LMLabor && x.Segment.Equals(SegmentType.RMS)).Select(x => x.Id));
            HashSet<int> IWTAToSearch = new HashSet<int>(Resources.Where(x => x.ElementOfCost == ElementOfCostType.IWTA).Select(x => x.Id));
            HashSet<int> SubToSearch = new HashSet<int>(Resources.Where(x => x.ElementOfCost == ElementOfCostType.Sub).Select(x => x.Id));

            foreach (BoeTaskElementDTO boeTask in boeTaskElements)
            {
                foreach (ResourceTypeDto labor in boeTask.taskElementLabors)
                {
                    if (labor.ResourceID.HasValue && labor.LaborSpreads.Any() && labor.SpreadType == SpreadType.Hours)
                    {
                        if (sumVariableResourceTypes.Contains((int)SumVariableResourceType.MSTLMLabor) && MSTToSearch.Contains(labor.ResourceID.Value))
                        {
                            toReturn = toReturn + labor.LaborSpreads.Sum(x => x.LaborSpreadValue);
                        }
                        else if (sumVariableResourceTypes.Contains((int)SumVariableResourceType.MSTLOEIWTA)
                            && (boeTask.TaskElementType == TaskElementType.Labor) && IWTAToSearch.Contains(labor.ResourceID.Value))
                        {
                            toReturn = toReturn + labor.LaborSpreads.Sum(x => x.LaborSpreadValue);
                        }
                        else if (sumVariableResourceTypes.Contains((int)SumVariableResourceType.MSTLOESub)
                            && (boeTask.TaskElementType == TaskElementType.Labor) && SubToSearch.Contains(labor.ResourceID.Value))
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
