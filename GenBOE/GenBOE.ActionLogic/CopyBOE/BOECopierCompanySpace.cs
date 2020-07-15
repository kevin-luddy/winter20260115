// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.CopyBOE
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Supports specific methods for BOE copy specific to Space.
    /// </summary>
    public class BOECopierCompanySpace : IBOECopierCompany
    {
        /// <summary>
        /// Returns a collection of Metric IDs that are used by the Task Element
        /// </summary>
        /// <param name="taskElementId">Task Element ID</param>
        /// <returns>Collection of Metric IDs</returns>
        public ICollection<int> GetMetricsUsedByTaskElement(int taskElementId)
        {
            // Metrics were deprecated for SSC
            return new Collection<int>();
        }

        /// <summary>
        /// Saves and associated a task element Id with a list of metric Ids.
        /// </summary>
        /// <param name="taskElementId">Task element Id.</param>
        /// <param name="metricIds">Associated metric Ids.</param>
        public void SaveMetricsToTaskElement(int taskElementId, ICollection<int> metricIds)
        {
            // Metrics deprecated for SSC
            return;
        }
    }
}
