// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using IES.Common;

    /// <summary>
    /// MOQ Table Data class
    /// </summary>
    public class MoqTableData : UpdateableDTO
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MoqTableData()
        {
            Id = -1;
            Order = 2000;
        }

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
        public DateTime DateOfReport { get; set; }

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

        /// <summary>
        /// Number representing the order the MOQ Type table is displayed in when there are multiple MOQ Type tables
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// ID of the MOQ Type Selection this table data belongs to
        /// </summary>
        public int MOQTypeSelectionId { get; set; }
    }
}
