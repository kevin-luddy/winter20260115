// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using GenBOE.ActionLogic.IO.Import;
    using IES.Common;

    public class ImportMoqTableResultsModelView : MoqTableData
    {
        public ImportMoqTableResultsModelView()
            :base ()
        {
        }

        public ImportMoqTableResultsModelView(ImportedMoqTable table, MoqTableImportType importType)
            :base ()
        {
            _ = table ?? throw new ArgumentNullException(nameof(table));

            Id = table.Id;
            TableName = table.TableName;
            RepositoryName = table.RepositoryName;
            QueryType = table.QueryType;
            DateOfReport = table.DateOfReport;
            HistoricalProgramName = table.HistoricalProgramName;
            ContractNumber = table.ContractNumber;
            WbsElement = table.WbsElement;
            PoPStart = table.PoPStart;
            PoPEnd = table.PoPEnd;
            PoPStartWeek = table.PoPStartWeek;
            PoPEndWeek = table.PoPEndWeek;
            PoPStartYear = table.PoPStartYear;
            PoPEndYear = table.PoPEndYear;
            TotalWbsHours = table.TotalWbsHours;
            AdditionalQueryFilters = table.AdditionalQueryFilters;
            TotalRelevantHours = table.TotalRelevantHours;
            Order = table.Order;
            MOQTypeSelectionId = table.MOQTypeSelectionId;
            CustomFieldValueContainers = table.CustomFieldValueContainers;
            ImportType = (int)importType;
        }

        /// <summary>
        /// Import type - either create or error type
        /// </summary>
        public int ImportType { get; set; }
    }
}
