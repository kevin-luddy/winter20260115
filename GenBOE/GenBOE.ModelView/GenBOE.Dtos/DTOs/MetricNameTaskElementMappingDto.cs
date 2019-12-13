// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Maps task element ids to historic metric titles.
    /// </summary>
    public class MetricNameTaskElementMappingDTO
    {
        private ICollection<MetricIdTaskElementIdXrefDTO> idXrefs;
        private Dictionary<int, string> metricNames;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MetricNameTaskElementMappingDTO()
        {
            idXrefs = new Collection<MetricIdTaskElementIdXrefDTO>();
            metricNames = new Dictionary<int, string>();
        }

        /// <summary>
        /// Alternate constructor.
        /// </summary>
        /// <param name="idXrefs">Xref of task element and metric ids.</param>
        /// <param name="metricNames">Metric ids and their title.</param>
        public MetricNameTaskElementMappingDTO(ICollection<MetricIdTaskElementIdXrefDTO> idXrefs, Dictionary<int, string> metricNames)
        {
            this.idXrefs = idXrefs;
            this.metricNames = metricNames;
        }

        /// <summary>
        /// Gets a formatted historical metric string for export to excel.
        /// </summary>
        /// <param name="id">Task element id.</param>
        /// <returns>Export formated historical metircs.</returns>
        public string GetMetricNamesByTaskElementId(int id)
        {
            string historicalMetricString = string.Empty;
            var historicalMetrics = (from m in metricNames
                                     join x in idXrefs on m.Key equals x.MetricId 
                                     where x.TaskElementId == id
                                     select new 
                                     {
                                         metricId = m.Key,
                                         metricTitle = m.Value
                                     });
            
            if (historicalMetrics.Any())
            {
                historicalMetricString = String.Join(", ", historicalMetrics.Select(m => "ID " + m.metricId + ": " + m.metricTitle));
            }

            return historicalMetricString;
        }
    }

    /// <summary>
    /// Contains a mapping of task element Id to historical metric Id.
    /// </summary>
    public class MetricIdTaskElementIdXrefDTO
    {
        public int TaskElementId { get; set; }
        public int MetricId { get; set; }
    }
}
