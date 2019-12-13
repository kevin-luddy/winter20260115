// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    [ExcludeFromCodeCoverage]
    public static class ODCSpreadExporter
    {
        [SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        public static string ExportToExcelFile(
            string templateFileLocation,
            ICollection<OtherDirectCostType> odcTypes,
            ResourceDTODataLoader inResourceDTODataLoader,
            IPerformingOrgDTODataLoader perfOrgLoader)
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }

            if (odcTypes == null)
            {
                throw new ArgumentNullException(nameof(odcTypes));
            }

            if (inResourceDTODataLoader == null)
            {
                throw new ArgumentNullException(nameof(inResourceDTODataLoader));
            }

            if (perfOrgLoader == null)
            {
                throw new ArgumentNullException(nameof(perfOrgLoader));
            }

            var toReturn = string.Empty;

            // Create a new random file name in the specified directory
            toReturn = ExcelUtilities.CopyExcelTemplateFile(templateFileLocation);

            lock (CacheConstants.OPEN_XML_LOCK)
            {
                using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(toReturn, true))
                {
                    ExcelUtilities.DuplicateColumn(spreadsheet, null, "Labor Type", odcTypes.Select(t => t.ODCTypeID.ToString()).ToArray());
                }
            }

            // Create collections of strings for each row in the export file
            var worksheet = new ExcelExportWorksheet();

            // Create the first row in the file
            var row = new Collection<string>();
            row.Add(string.Empty);

            ICollection<ResourceDTO> resources = inResourceDTODataLoader.GetByIds(odcTypes.Where(x => x.ResourceID.HasValue).Select(x => x.ResourceID.Value).Distinct().ToList());

            foreach (var odcType in odcTypes)
            {
                if (odcType.ResourceID.HasValue)
                {
                    var resource = resources.First(x => x.Id == odcType.ResourceID.Value);
                    row.Add(resource.ResourceName);
                }
                else
                {
                    row.Add(string.Empty);
                }
            }
            row.Add(string.Empty); //row.Add("Total by Months");
            worksheet.Add(row);

            // Create the second row in the file
            row = new Collection<string>();
            row.Add(string.Empty);

            HashSet<PerformingOrgDTO> perfOrgsFromDb = new HashSet<PerformingOrgDTO>(perfOrgLoader.GetByIds(odcTypes.Where(x => x.PerformingOrgID.HasValue).Select(x => x.PerformingOrgID.Value).Distinct().ToList()));

            foreach (var odcType in odcTypes)
            {
                if (odcType.PerformingOrgID.HasValue)
                {
                    var performingOrg = perfOrgsFromDb.First(x => x.Id == odcType.PerformingOrgID.Value);
                    row.Add(performingOrg.PerformingOrgName);
                }
                else
                {
                    row.Add(string.Empty);
                }
            }
            row.Add(string.Empty);
            worksheet.Add(row);

            // Create the third row in the file
            row = new Collection<string>();
            row.Add(string.Empty);
            foreach (var odcType in odcTypes)
            {
               row.Add(((decimal)odcType.ODCSpreads.Sum(st => st.CostSpreadValue)/100).ToString());
            }
            row.Add(((decimal)odcTypes.Sum(l => l.ODCSpreads.Sum(st=>st.CostSpreadValue))/100).ToString());
            worksheet.Add(row);

            if (odcTypes.Count > 0)
            {
                var spreadStart = odcTypes.Min(l => l.StartDate).Value;
                var spreadEnd = odcTypes.Max(l => l.EndDate).Value;
                var spreadMonths = 12 * (spreadEnd.Year - spreadStart.Year) - spreadStart.Month + spreadEnd.Month + 1;

                var date = spreadStart;
                for (var ndx = 0; ndx < spreadMonths; ndx++)
                {
                    row = new Collection<string>();
                    row.Add(date.ToString("MM/yyyy"));
                    foreach (var odcType in odcTypes)
                    {
                        var spread = (from s in odcType.ODCSpreads
                                      where s.ODCSpreadDate.Value.Month == date.Month &&
                                      s.ODCSpreadDate.Value.Year == date.Year
                                      select s).FirstOrDefault();

                        if (spread != null)
                        {
                            row.Add(((decimal)spread.CostSpreadValue.Value/100).ToString());
                        }
                        else
                        {
                            row.Add(string.Empty);
                        }
                    }
                    var totalHoursForMonth = (from t in odcTypes
                                              from s in t.ODCSpreads
                                              where s.ODCSpreadDate.Value.Month == date.Month &&
                                              s.ODCSpreadDate.Value.Year == date.Year
                                              select s).Sum(s => s.CostSpreadValue);
                    row.Add(((decimal)totalHoursForMonth/100).ToString());
                    worksheet.Add(row);

                    date = date.AddMonths(1);
                }
            }

            // Export without duplicating the template file first
            ExcelExporter.ExportToExcelFile(toReturn, false, worksheet);

            return toReturn;
        }

    }
}
