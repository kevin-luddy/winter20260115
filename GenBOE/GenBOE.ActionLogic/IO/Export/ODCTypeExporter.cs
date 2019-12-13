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
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    [ExcludeFromCodeCoverage]
    public static class ODCTypeExporter
    {
        #region Public Functions

        public static string ExportTempate(string templateFileLocation,
            ResourceDTODataLoader inResourceLoader,
            ICommonDataMapper inCommonMapper,
            FullWorkspace inWorkspace)
        {
            return ExportToExcelFile(templateFileLocation, inResourceLoader, inCommonMapper, inWorkspace, null);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public static string ExportToExcelFile(string templateFileLocation,
            ResourceDTODataLoader inResourceLoader,
            ICommonDataMapper inCommonMapper,
            FullWorkspace inWorkspace, OtherDirectCostDTO inODCElement)
        {
            if (inResourceLoader == null)
            {
                throw new ArgumentNullException(nameof(inResourceLoader));
            }
            if (inCommonMapper == null)
            {
                throw new ArgumentNullException(nameof(inCommonMapper));
            }
            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }

            string toReturn = null;

            var allPerformingOrgs = inWorkspace.PerformingOrgsForWsList;
            var allResourceTypes = inResourceLoader.GetByListIdAndElementOfCost(inWorkspace.ResourceListID, ElementOfCostType.ODC);
            var allCurves = inCommonMapper.getOdcSpreadCurve();

            int maxAutoFillRows = Math.Max(allCurves.Count, Math.Max(allPerformingOrgs.Count, allResourceTypes.Count));

            // Create collections of strings for each row in the export file
            var optionsListWorksheet = new ExcelExportWorksheet("Options Lists");

            for (int i = 0; i < maxAutoFillRows; i++)
            {
                optionsListWorksheet.Add(
                    allResourceTypes.ElementAtOrDefault(i) != null ? allResourceTypes.ElementAt(i).ResourceName + " - " + allResourceTypes.ElementAt(i).ResourceDesc : string.Empty,
                    allPerformingOrgs.ElementAtOrDefault(i) != null ? allPerformingOrgs.ElementAt(i).PerformingOrgName + " - " + allPerformingOrgs.ElementAt(i).PerformingOrgDesc : string.Empty,
                    allCurves.ElementAtOrDefault(i) != null ? allCurves.ElementAt(i).SpreadCurveName : string.Empty
                    );
            }

            var firstWorksheet = new ExcelExportWorksheet();

            if (inODCElement != null)
            {
                Collection<OtherDirectCostType> ODCTypes = inODCElement.ODCTypes;

                foreach (OtherDirectCostType ODCType in ODCTypes)
                {
                    ResourceDTO thisResource = (from resources in allResourceTypes where resources.Id == ODCType.ResourceID select resources).FirstOrDefault();
                    PerformingOrgDTO thisPerfOrg = (from perOrgs in allPerformingOrgs where perOrgs.Id == ODCType.PerformingOrgID select perOrgs).FirstOrDefault();
                    OtherDirectCostSpreadCurveModelView thisSpread = (from curves in allCurves where curves.SpreadCurveID == ODCType.SpreadCurve select curves).FirstOrDefault();

                    int numberOfMonths = 0;
                    if (ODCType.EndDate.HasValue && ODCType.StartDate.HasValue)
                    {
                        // +1 is needed to count the starting month. that's how we do the logic in the page
                        numberOfMonths = ODCType.StartDate.Value.MonthDifference(ODCType.EndDate.Value) + 1;
                    }

                    firstWorksheet.Add(
                        ODCType.ODCTypeID.ToString(),
                        thisResource.ResourceName + " - " + thisResource.ResourceDesc,
                        thisPerfOrg.PerformingOrgName + " - " + thisPerfOrg.PerformingOrgDesc,
                        ODCType.StartDate.Value.ToString("MM/yyyy"),
                        numberOfMonths.ToString(),
                        ODCType.EndDate.Value.ToString("MM/yyyy"),
                        thisSpread.SpreadCurveName,
                        ODCType.Cost.ToString()
                        );
                }
            }

            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, optionsListWorksheet, firstWorksheet);
            
            // Adjust Defined Names
            Dictionary<string, int> lengths = new Dictionary<string, int>()
            {
                { "Resources", allResourceTypes.Count },
                { "PerfOrgs", allPerformingOrgs.Count },
                { "SpreadCurve", allCurves.Count }
            };

            ExcelExporter.AdjustDefinedNames(toReturn, lengths);
            
            return toReturn;
        }


        #endregion Public Functions
    }
}
