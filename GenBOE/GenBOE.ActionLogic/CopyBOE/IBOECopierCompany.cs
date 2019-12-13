// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.CopyBOE
{
    using System.Collections.Generic;

    /// <summary>
    /// Supports company specific methods for BOE copy.
    /// </summary>
    public interface IBOECopierCompany
    {
        /// <summary>
        /// Returns a collection of Metric IDs that are used by the Task Element
        /// </summary>
        /// <param name="taskElementId">Task Element ID</param>
        /// <returns>Collection of Metric IDs</returns>
        ICollection<int> GetMetricsUsedByTaskElement(int taskElementId);

        /// <summary>
        /// Saves and associated a task element Id with a list of metric Ids.
        /// </summary>
        /// <param name="taskElementId">Task element Id.</param>
        /// <param name="metricIds">Associated metric Ids.</param>
        void SaveMetricsToTaskElement(int taskElementId, ICollection<int> metricIds);
    }
}
