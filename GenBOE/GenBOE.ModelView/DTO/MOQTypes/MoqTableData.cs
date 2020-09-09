// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;

    /// <summary>
    /// MOQ Table Data class
    /// </summary>
    public class MoqTableData
    {
        /// <summary>
        /// Table Name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Repository Name
        /// </summary>
        public string RepositoryName { get; set; }

        /// <summary>
        /// Query Type
        /// </summary>
        public string QueryType { get; set; }

        /// <summary>
        /// Date Of Report
        /// </summary>
        public string DateOfReport { get; set; }

        /// <summary>
        /// Historical Program Name
        /// </summary>
        public string HistoricalProgramName { get; set; }

        /// <summary>
        /// Contract Number
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Wbs Element
        /// </summary>
        public string WbsElement { get; set; }

        /// <summary>
        /// PoP Start
        /// </summary>
        public DateTime PoPStart { get; set; }

        /// <summary>
        /// PoP End
        /// </summary>
        public DateTime PoPEnd { get; set; }

        /// <summary>
        /// Total WbsHours
        /// </summary>
        public decimal TotalWbsHours { get; set; }

        /// <summary>
        /// Additional Query Filters
        /// </summary>
        public string AdditionalQueryFilters { get; set; }

        /// <summary>
        /// Total Relevant hours
        /// </summary>
        public decimal TotalRelevantHours { get; set; }
    }
}
