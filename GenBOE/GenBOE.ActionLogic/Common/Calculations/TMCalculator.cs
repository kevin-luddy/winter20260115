// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.Calculations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common.Exceptions;

    public class TMCalculator
    {
        /// <summary>
        /// Calculates total cost for a labor task using the T&amp;M rates for the associated resource.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="laborTask">The labor task containing the labor spread values</param>
        /// <returns>The total cost after multiplying each labor spread value against the corresponding T&amp;M rate.</returns>
        public decimal TotalCostForTaskSpread(FullWorkspace workspace, ResourceTypeDto laborTask)
        {
            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }
            if (ReferenceEquals(laborTask, null))
            {
                throw new ArgumentNullException(nameof(laborTask));
            }

            decimal totalCost = 0;
            try
            { 
                // Get T&M rates from workspace for this resourceId.
                IReadOnlyCollection<TMResourceRateDTO> wsRates = workspace.TMResourceRatesForWorkspace;

                //Left Join (DefaultIfEmpty()) spreads to T&M spreadRates by resourceId & workspaceId, where laborTask date in T&M resource date range.
                // Missing T&M Rates will throw NullReferenceException.
                totalCost = (from laborSpread in laborTask.LaborSpreads
                    join tmResoureRate in wsRates
                    on new { res = laborTask.ResourceID.Value, ws = workspace.Id } equals
                    new { res = tmResoureRate.ResourceID, ws = tmResoureRate.WorkspaceID } into hrs
                    from hr in hrs.Where(tmResoureRate => tmResoureRate.StartDate.Value <= laborSpread.LaborSpreadDate)
                        .Where(tmResoureRate => tmResoureRate.EndDate.Value >= laborSpread.LaborSpreadDate).DefaultIfEmpty()
                    select (laborSpread.LaborSpreadValue * hr.ResourceRate.Value)).Sum();
            }
            catch (NullReferenceException)
            {
                throw new GenValidationException($"T&M Rates are missing for Resource: {laborTask.ResourceID.ToString()}.");
            }
            catch (Exception ex)
            {
                throw new GenValidationException($"Application encountered an error calculating T&M rates for {laborTask.ToString()}:" + ex.Message);
            }

            return totalCost;
         }
    }
}
