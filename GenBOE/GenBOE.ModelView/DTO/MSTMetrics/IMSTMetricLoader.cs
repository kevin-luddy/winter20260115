// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;

    /// <summary>
    /// MST Metric Data Loader.
    /// </summary>
    public interface IMSTMetricLoader
    {
        /// <summary>
        /// Gets search criteria for searching MST metrics.
        /// </summary>
        /// <returns>Metric search criteria.</returns>
        MSTMetricSearchCriteriaDTO GetMSTMetricSearchCriteria();

        /// <summary>
        /// Searches MST metrics using the given search criteria.
        /// </summary>
        /// <param name="searchDTO">Search criteria.</param>
        /// <returns>Search result details for the given criteria.</returns>
        ICollection<MSTMetricDetailsDTO> SearchMSTMetrics(MSTMetricSearchDTO searchDTO);

        /// <summary>
        /// Gets a list of metric details from the given ids.
        /// </summary>
        /// <param name="ids">Metric Ids</param>
        /// <returns>Metric details for the given list of Ids.</returns>
        ICollection<MSTMetricDetailsDTO> GetByIds(ICollection<int> ids);

        /// <summary>
        /// Gets a list of metric details saved to the genBOE database.
        /// </summary>
        /// <param name="ids">Metric Ids</param>
        /// <returns>Metric details for the given list of Ids.</returns>
        ICollection<MSTMetricDetailsDTO> GetMetricDetailsByIds(ICollection<int> ids);

        /// <summary>
        /// Gets MST PMM metrics related to task elements.
        /// </summary>
        /// <param name="inTaskElementIds">Task element Ids.</param>
        /// <returns>Metrics related to the given task element Ids.</returns>
        ICollection<MSTMetricDetailsDTO> GetByTaskElementIds(ICollection<int> inTaskElementIds);

        /// <summary>
        /// Gets a mapping of task element id to metric id mappings.
        /// </summary>
        /// <param name="inTaskElementIds">Task element Ids.</param>
        /// <returns>Collection of mappings.</returns>
        ICollection<MetricIdTaskElementIdXrefDTO> GetTaskElementIdMeticIdMappings(ICollection<int> inTaskElementIds);

        /// <summary>
        /// Gets a list of metrics by boe id.
        /// </summary>
        /// <param name="inBoeIds">BOE Ids.</param>
        /// <returns>Metrics by BOE ids.</returns>
        ICollection<MSTMetricDetailsDTO> GetByBoeIds(ICollection<int> inBoeIds);

        /// <summary>
        /// Saves measures to genBOE by reconciling the current measures with the given list.
        /// </summary>
        /// <param name="taskElementId">Task element id.</param>
        /// <param name="pmmMetricIds">PMM metric Ids.</param>
        void Save(int taskElementId, ICollection<int> pmmMetricIds);
    }
}
