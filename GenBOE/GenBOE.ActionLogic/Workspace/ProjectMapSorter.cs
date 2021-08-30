// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenBOE.Dtos;
    using GenBOE.Objects;

    /// <summary>
    /// Sorts a set of Project Map BOEs by assigning a sequence number.
    /// </summary>
    public static class ProjectMapSorter
    {
        /// <summary>
        /// Order BOEs by CLIN, then Resource, then PerfOrg, then Title
        /// </summary>
        /// <param name="boes">BOEs to order</param>
        /// <param name="fullWorkspace">Workspace</param>
        /// <returns>Collection of ordered BOEs</returns>
        public static ICollection<FullBoe> OrderBoes(ICollection<FullBoe> boes, FullWorkspace fullWorkspace)
        {
            if (boes == null)
            {
                throw new ArgumentNullException(nameof(boes));
            }

            if (fullWorkspace == null)
            {
                throw new ArgumentNullException(nameof(fullWorkspace));
            }
            
            List<SortedBoe> sortedList = new List<SortedBoe>();
            foreach (FullBoe boe in boes)
            {
                if (boe.TaskElements.Any())
                {
                    ICollection<ResourceTypeDto> taskElementLabors = boe.TaskElements.SelectMany(t => t.taskElementLabors).ToList();
                    if (taskElementLabors.Any())
                    {
                        sortedList.Add(new SortedBoe
                        {
                            Boe = boe,
                            ClinNumber = fullWorkspace.Clins.FirstOrDefault(c => c.Id == boe.CLINID)?.ClinNumber ?? string.Empty,
                            ResourceName = fullWorkspace.ResourcesForWsResourceListId.First(r => r.Id == taskElementLabors.First().ResourceID).ResourceName,
                            PerfOrgName = fullWorkspace.PerformingOrgsForWsList.First(p => p.Id == taskElementLabors.First().PerformingOrgID).PerformingOrgName
                        });
                    }
                }
            }

            return sortedList.OrderBy(s => s.ClinNumber).ThenBy(s => s.ResourceName).ThenBy(s => s.PerfOrgName).ThenBy(s => s.Boe.Title).Select(o => o.Boe).ToList();
        }
    }

    /// <summary>
    /// A DTO used for sorting BOEs.
    /// </summary>
    internal class SortedBoe
    {
        /// <summary>
        /// Gets or sets the boe.
        /// </summary>
        public FullBoe Boe { get; set; }

        /// <summary>
        /// Gets or sets the clin number.
        /// </summary>
        public string ClinNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the perf org.
        /// </summary>
        public string PerfOrgName { get; set; }
    }
}