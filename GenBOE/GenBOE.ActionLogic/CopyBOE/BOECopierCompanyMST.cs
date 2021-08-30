// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.CopyBOE
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;

    /// <summary>
    /// Supports specific methods for BOE copy specific to MST.
    /// </summary>
    public class BOECopierCompanyMST : IBOECopierCompany
    {
        /// <summary>
        /// MST Metric Loader.
        /// </summary>
        private IMSTMetricLoader _metricLoader;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="metricLoader">MST Metric Loader.</param>
        public BOECopierCompanyMST(IMSTMetricLoader metricLoader)
        {
            this._metricLoader = metricLoader;
        }

        /// <summary>
        /// Returns a collection of Metric IDs that are used by the Task Element
        /// </summary>
        /// <param name="taskElementId">Task Element ID</param>
        /// <returns>Collection of Metric IDs</returns>
        public ICollection<int> GetMetricsUsedByTaskElement(int taskElementId)
        {
            ICollection<int> metricIds = new Collection<int>();

            ICollection<MSTMetricDetailsDTO> inUseMetrics = new Collection<MSTMetricDetailsDTO>();
            inUseMetrics = this._metricLoader.GetByTaskElementIds(new Collection<int> { taskElementId });

            foreach (MSTMetricDetailsDTO metricDTO in inUseMetrics)
            {
                metricIds.Add(metricDTO.PMMMeasureId);
            }

            return metricIds;
        }

        /// <summary>
        /// Saves and associated a task element Id with a list of metric Ids.
        /// </summary>
        /// <param name="taskElementId">Task element Id.</param>
        /// <param name="metricIds">Associated metric Ids.</param>
        public void SaveMetricsToTaskElement(int taskElementId, ICollection<int> metricIds)
        {
            this._metricLoader.Save(taskElementId, metricIds);
        }
    }
}
