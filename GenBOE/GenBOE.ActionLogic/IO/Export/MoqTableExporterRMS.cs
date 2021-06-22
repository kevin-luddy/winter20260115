// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    public class MoqTableExporterRMS : MoqTableExporter
    {
        /// <summary>
        /// MOQ Table export excel file path
        /// </summary>
        public override string MOQ_TABLE_EXCEL_MAP_PATH
        {
            get { return "~/Templates/Export/MOQTableRMS.xlsx"; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MoqTableExporterRMS()
            :base()
        {

        }

        /// <summary>
        /// Get the MOQ Table Data row data
        /// </summary>
        /// <param name="table">MOQ Table</param>
        /// <param name="ws">Workspace</param>
        /// <returns>MOQ Table Data row for the given data</returns>
        protected override IList<string> GetMOQTableDataRow(MoqTableData table, FullWorkspace ws)
        {
            _ = table ?? throw new ArgumentNullException(nameof(table));
            _ = ws ?? throw new ArgumentNullException(nameof(ws));

            IList<string> toReturn = new List<string>()
            {
                table.TableName,
                table.DateOfReport.ToShortDateString(),
                table.HistoricalProgramName,
                table.ContractNumber,
                table.WbsElement,
                table.PoPStartString,
                table.PoPEndString,
                table.TotalWbsHours.ToString(),
                table.AdditionalQueryFilters,
                table.TotalRelevantHours.ToString()
            };

            foreach (CustomFieldDTO customField in ws.CustomFields.Where(x => x.CustomFieldDisplayID == CustomFieldType.MoqTypeTableDataDisplay))
            {
                CustomFieldValueContainer customFieldValue = table.CustomFieldValueContainers.FirstOrDefault(x => x.CustomFieldID == customField.Id);
                toReturn.Add(customFieldValue != null ? customFieldValue.OpenEndedValue : string.Empty);
            }

            return toReturn;
        }
    }
}
