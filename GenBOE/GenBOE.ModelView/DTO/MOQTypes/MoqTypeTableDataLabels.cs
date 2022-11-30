// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using IES.Common;
using IES.Common.classes;

namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// A Class that holds labels for MOQ Type Table Fields
    /// </summary>
    public class MoqTypeTableDataLabels
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public MoqTypeTableDataLabels()
        {
            this.RepositoryName = "Repository Name";
            this.QueryType = "Query Type";
            this.ContractNumber = "Contract Number";
            this.TableName = "Table Name";
            this.DateOfReport = "Date of Report";
            this.HistoricalProgramName = "Historical Program Name";
            this.WbsElement = "WBS/WBS Element";
            this.PoPStart = "Period of Performance (PoP): Start Date";
            this.PoPEnd = "Period of Performance (PoP): End Date";
            this.PoPMonths = "Period of Performance (PoP): Months";
            this.TotalWbsHours = "Total WBS/WBS Element Hours";
            if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
            {
                this.AdditionalQueryFilters = "Employee ID Filters";
                this.TotalRelevantHours = "Total Relevant Hours After Employee ID Filters Applied";
            }
            else
            {
                this.AdditionalQueryFilters = "Additional Query Filters";
                this.TotalRelevantHours = "Total Relevant Hours After Additional Query Filters Applied";
            }
        }

        /// <summary>
        /// Label for Table Name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Label for Repository Name
        /// </summary>
        public string RepositoryName { get; set; }

        /// <summary>
        /// Label for Query Type
        /// </summary>
        public string QueryType { get; set; }

        /// <summary>
        /// Label for Date Of Report
        /// </summary>
        public string DateOfReport { get; set; }

        /// <summary>
        /// Label for Historical Program Name
        /// </summary>
        public string HistoricalProgramName { get; set; }

        /// <summary>
        /// Label for Contract Number
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Label for Wbs Element
        /// </summary>
        public string WbsElement { get; set; }

        /// <summary>
        /// Label for PoP Start
        /// </summary>
        public string PoPStart { get; set; }

        /// <summary>
        /// Label for PoP End
        /// </summary>
        public string PoPEnd { get; set; }

        /// <summary>
        /// Label for PoP Months
        /// </summary>
        public string PoPMonths { get; set; }

        /// <summary>
        /// Label for Total WbsHours
        /// </summary>
        public string TotalWbsHours { get; set; }

        /// <summary>
        /// Label for Additional Query Filters
        /// </summary>
        public string AdditionalQueryFilters { get; set; }

        /// <summary>
        /// Label for Total Relevant hours
        /// </summary>
        public string TotalRelevantHours { get; set; }
    }
}
