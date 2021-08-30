// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GenBOE.ActionLogic.ModelView;
using IES.Common;
using GenBOE.Dtos;

namespace GenBOE.Web.ModelView
{
    /// <summary>
    /// Model view class to support paged results of MST metrics from PMM.
    /// </summary>
    public class HistoricalMetricsFromMSTModelView : PagedResultsModelView<int>
    {

        /// <summary>
        /// Default constructor.
        /// </summary>
        public HistoricalMetricsFromMSTModelView()
        {
            this.MetricsSearchResults = new Collection<MSTMetricDetailsDTO>();
            this.CurrentPage = 1;
            this.PagedIndexes = new Collection<int>();
            this.ResultsPerPage = 10;
            this.MSTSearchResultsHelpLink = ConfigurationUtilities.GetAppSetting("MSTSearchResultsHelpLink", string.Empty);
        }

        /// <summary>
        /// Alternate Constructor.
        /// </summary>
        /// <param name="inMSTSearchResults">Search results from user search criteria.</param>
        public HistoricalMetricsFromMSTModelView(ICollection<MSTMetricDetailsDTO> inMSTSearchResults) : this()
        {

            if (inMSTSearchResults == null)
            {
                throw new ArgumentNullException(nameof(inMSTSearchResults));
            }

            this.CurrentPage = 1;
            this.PagedIndexes = new Collection<int>();
            this.ResultsPerPage = 10;

            this.MetricsSearchResults = new Collection<MSTMetricDetailsDTO>();

            foreach (MSTMetricDetailsDTO metricDTO in inMSTSearchResults)
            {
                if (this.MetricsSearchResults.Count < this.ResultsPerPage)
                {
                    this.MetricsSearchResults.Add(metricDTO);
                }
                this.PagedIndexes.Add(metricDTO.Id);
            }
        }

        /// <summary>
        /// List of search results for user search criteria.
        /// </summary>
        public ICollection<MSTMetricDetailsDTO> MetricsSearchResults { get; set; }

        /// <summary>
        /// A link to the MST search results help document.
        /// </summary>
        public string MSTSearchResultsHelpLink { get; set; }
    }   
}
