// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    /// <summary>
    /// DTO used to search MST metrics by selected criterea.
    /// </summary>
    public class MSTMetricSearchDTO
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MSTMetricSearchDTO()
        {
            this.SearchFor = string.Empty;
            this.MeasureFunctionId = string.Empty;
        }

        /// <summary>
        /// Search string that could be contained in a Program Description, Measure Description or Comment data.
        /// </summary>
        public string SearchFor { get; set; }

        /// <summary>
        /// When not null or zero, the search is narrowed to the program id and anded with other search criteria.
        /// </summary>
        public int? ProgramId { get; set; }

        /// <summary>
        /// When not null or zero, the search is narrowed to the measure name id and anded with other search criteria.
        /// </summary>
        public int? MeasureId { get; set; }

        /// <summary>
        /// When not null, the search is narrowed to the measure function id and anded with other search criteria.
        /// </summary>
        public string MeasureFunctionId { get; set; }

        /// <summary>
        /// When not null, the search is narrowed to the measure qualifier id and anded with other search criteria.
        /// </summary>
        public string MeasureQualifierId { get; set; }

        /// <summary>
        /// When not null or zero, the search is narrowed to the data source id and anded with other search criteria.
        /// </summary>
        public int? DataSourceId { get; set; } 
    }
}
