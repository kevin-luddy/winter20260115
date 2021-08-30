// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Wordprocessing;
    using GenBOE.Dtos;
    using IES.Common;

    #region Classes

    /// <summary>
    /// Row data for the resource summary
    /// </summary>
    public class ResourceSummaryRowData
    {
        public string ResourceType { get; set; }
        public string ResourceName { get; set; }
        public string ResourceDescription { get; set; }
        public decimal? CostTotal { get; set; }
        public decimal? HoursTotal { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? PeopleTotal { get; set; }
        public decimal? DaysTotal { get; set; }

        public string GroupKey { get { return $"{this.ResourceType}::{this.ResourceName}"; } }
    }

    /// <summary>
    /// Row data for the Project Map Resource Summary
    /// </summary>
    public class ProjectMapResourceSummaryRowData
    {
        public string ResourceCode { get; set; }
        public string CostCenterCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? HoursTotal { get; set; }
        public decimal? CostTotal { get; set; }
        public decimal? TieredPercent { get; set; }
    }

    /// <summary>
    /// Captures monthly values for rollup data
    /// </summary>
    /// <typeparam name="T">The type of data</typeparam>
    public class ValuesByMonth<T> : ICloneable where T : struct
    {
        public T January { get; set; }
        public T February { get; set; }
        public T March { get; set; }
        public T April { get; set; }
        public T May { get; set; }
        public T June { get; set; }
        public T July { get; set; }
        public T August { get; set; }
        public T September { get; set; }
        public T October { get; set; }
        public T November { get; set; }
        public T December { get; set; }

        public object Clone()
        {
            ValuesByMonth<T> clone = new ValuesByMonth<T>
            {
                January = this.January,
                February = this.February,
                March = this.March,
                April = this.April,
                May = this.May,
                June = this.June,
                July = this.July,
                August = this.August,
                September = this.September,
                October = this.October,
                November = this.November,
                December = this.December
            };

            return clone;
        }
    }

    /// <summary>
    /// Captures table row data for rollup summary tables
    /// </summary>
    public class RollupSummaryByYearTableRowData : ICloneable
    {
        public int Year { get; set; }
        public ValuesByMonth<decimal> MonthlyValues { get; set; }
        public decimal YearTotal { get; set; }

        public object Clone()
        {
            RollupSummaryByYearTableRowData clone = new RollupSummaryByYearTableRowData
            {
                Year = this.Year,
                YearTotal = this.YearTotal,
                MonthlyValues = this.MonthlyValues.Clone() as ValuesByMonth<decimal>
            };

            return clone;
        }
    }

    /// <summary>
    /// Captures table group data for Rollup Summary tables by group
    /// </summary>
    public class RollupSummaryByGroupByYearTableGroupData
    {
        public string GroupName { get; set; }
        public ICollection<RollupSummaryByYearTableRowData> YearlyData { get; set; }
        public decimal GroupSubTotal { get; set; }
    }

    /// <summary>
    /// Captures table data for Rollup Summary tables by group
    /// </summary>
    public class RollupSummaryByGroupByYearTableData
    {
        public ICollection<RollupSummaryByGroupByYearTableGroupData> GroupData { get; set; }
        public ICollection<RollupSummaryByYearTableRowData> SummaryData { get; set; }
        public ValuesByMonth<decimal> SummaryTotalsByMonth { get; set; }
        public decimal SummaryTotalComplete { get; set; }
    }

    /// <summary>
    /// Captures table data for Rollup Summary tables
    /// </summary>
    public class RollupSummaryByYearTableData
    {
        public ICollection<RollupSummaryByYearTableRowData> YearlyData { get; set; }
        public decimal SummaryTotalComplete { get; set; }
    }

    /// <summary>
    /// Captures resource table row data
    /// </summary>
    public class ResourceTypesTableRowData
    {
        public string LaborTypeID { get; set; }
        public string PerformingOrgID { get; set; }
        public string PerformingOrgName { get; set; }
        public string PerformingOrgDescription { get; set; }
        public string ResourceID { get; set; }
        public string ResourceName { get; set; }
        public string ResourceDescription { get; set; }
        public string SegmentRegion { get; set; }
        public string ResourceType { get; set; }
        public string Segment { get; set; }
        public long Cost { get; set; }
    }

    /// <summary>
    /// Captures resource table data
    /// </summary>
    public class ResourceTypesTableData
    {
        public ICollection<ResourceTypesTableRowData> ResourcesData { get; set; }
        public long CostTotalComplete { get; set; }
    }

    /// <summary>
    /// Captures resource table row data for Labor resources
    /// </summary>
    public class LaborResourceTypesTableRowData : ResourceTypesTableRowData
    {
        public LaborResourceTypesTableRowData()
        {
            this.Hours = "0";
        }
        public string ElementOfCost { get; set; }
        public ICollection<ExportCustomField> CustomFields { get; set; }
        public string Hours { get; set; }
        public string SpreadCurve { get; set; }
        public string WbsString { get; set; }
        public string ClinString { get; set; }
        public string SummaryReference { get; set; }
    }

    /// <summary>
    /// Captures resource table data for Labor resources
    /// </summary>
    public class LaborResourceTypesTableData
    {
        public ICollection<LaborResourceTypesTableRowData> ResourcesData { get; set; }
        public decimal HoursTotalComplete { get; set; }
        public long CostTotalComplete { get; set; }
    }

    /// <summary>
    /// Captures resource table row data for ODC resources
    /// </summary>
    public class ODCResourceTypesTableRowData : ResourceTypesTableRowData
    {
        public string ElementOfCost { get; set; }
    }

    /// <summary>
    /// Captures resource table data for ODC resources
    /// </summary>
    public class ODCResourceTypesTableData
    {
        public ICollection<ODCResourceTypesTableRowData> ResourcesData { get; set; }
        public long CostTotalComplete { get; set; }
    }

    /// <summary>
    /// Captures resource table row data for Travel resources
    /// </summary>
    public class TravelResourceTypesTableRowData : ResourceTypesTableRowData
    {
        public string ElementOfCost { get; set; }
        public string Mode { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public string Purpose { get; set; }
        public int Trips { get; set; }
        public int People { get; set; }
        public int Days { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Captures resource table data for Travel resources
    /// </summary>
    public class TravelResourceTypesTableData
    {
        public ICollection<TravelResourceTypesTableRowData> ResourcesData { get; set; }
        public long CostTotalComplete { get; set; }
    }

    /// <summary>
    /// Captures resource table row data for RMS Travel resources
    /// </summary>
    public class RMSTravelResourceTypesTableRowData : ResourceTypesTableRowData
    {
        public int GroupID { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public int Zone { get; set; }
        public string Purpose { get; set; }
        public DateTime TripDate { get; set; }
        public decimal People { get; set; }
        public decimal Days { get; set; }
        public decimal Cars { get; set; }
        public bool IsAirfareTrip { get; set; }
        public string SecondaryResourceName { get; set; }
    }

    /// <summary>
    /// Captures resource table data for RMS Travel resources
    /// </summary>
    public class RMSTravelResourceTypesTableData
    {
        public ICollection<RMSTravelResourceTypesTableRowData> ResourcesData { get; set; }
        public long CostTotalComplete { get; set; }
    }

    /// <summary>
    /// Captures resource table row data for Material Resources
    /// </summary>
    public class MaterialResourceTypesTableRowData : ResourceTypesTableRowData
    {
        public DateTime ExpendDate { get; set; }
    }

    /// <summary>
    /// Captures resource table data for Material resources
    /// </summary>
    public class MaterialResourceTypesTableData
    {
        public ICollection<MaterialResourceTypesTableRowData> ResourcesData { get; set; }
        public long CostTotalComplete { get; set; }
    }

    /// <summary>
    /// Captures the task container and task type row for the template
    /// </summary>
    public class BOEExportTaskContainer
    {
        public OpenXmlElement TaskContainer { get; set; }
        public TableRow TaskTypeRow { get; set; }
        public bool Duplicated { get; set; }

        public BOEExportTaskContainer()
        {
            this.TaskContainer = null;
            this.TaskTypeRow = null;
            this.Duplicated = false;
        }

        public BOEExportTaskContainer(OpenXmlElement taskContainer)
            : this()
        {
            if (taskContainer == null)
            {
                throw new ArgumentNullException(nameof(taskContainer));
            }

            this.TaskContainer = taskContainer;
        }

        public BOEExportTaskContainer(OpenXmlElement taskContainer, TableRow taskTypeRow)
            : this(taskContainer)
        {
            if (taskTypeRow == null)
            {
                throw new ArgumentNullException(nameof(taskTypeRow));
            }

            this.TaskTypeRow = taskTypeRow;
        }
    }

    /// <summary>
    /// Captures date rollup data
    /// </summary>
    /// <remarks>
    /// see: RollupSummaryByYearTableRowData
    /// </remarks>
    public class LaborRollupByDateNew : ICloneable
    {
        public LaborRollupByDateNew()
        {
        }

        public LaborRollupByDateNew(int year, ValuesByMonth<decimal> monthlyValues)
        {
            if (monthlyValues == null)
            {
                throw new ArgumentNullException(nameof(monthlyValues));
            }

            this.Year = year;

            this.January = monthlyValues.January;
            this.February = monthlyValues.February;
            this.March = monthlyValues.March;
            this.April = monthlyValues.April;
            this.May = monthlyValues.May;
            this.June = monthlyValues.June;
            this.July = monthlyValues.July;
            this.August = monthlyValues.August;
            this.September = monthlyValues.September;
            this.October = monthlyValues.October;
            this.November = monthlyValues.November;
            this.December = monthlyValues.December;

            this.Resource = string.Empty;
        }

        public int Year { get; set; }
        public decimal January { get; set; }
        public decimal February { get; set; }
        public decimal March { get; set; }
        public decimal April { get; set; }
        public decimal May { get; set; }
        public decimal June { get; set; }
        public decimal July { get; set; }
        public decimal August { get; set; }
        public decimal September { get; set; }
        public decimal October { get; set; }
        public decimal November { get; set; }
        public decimal December { get; set; }
        public String Resource { get; set; }
        public decimal Total
        {
            get
            {
                return this.January + this.February + this.March + this.April + this.May + this.June + this.July + this.August + this.September + this.October + this.November + this.December;
            }
        }

        public object Clone()
        {
            LaborRollupByDateNew clone = new LaborRollupByDateNew
            {
                January = this.January,
                February = this.February,
                March = this.March,
                April = this.April,
                May = this.May,
                June = this.June,
                July = this.July,
                August = this.August,
                September = this.September,
                October = this.October,
                November = this.November,
                December = this.December,
                Resource = this.Resource,
                Year = this.Year
            };

            return clone;
        }
    }

    #endregion

    #region Extensions

    /// <summary>
    /// Extensions for ValuesByMonth
    /// </summary>
    public static class ValuesByMonthExtensions
    {
        /// <summary>
        /// Adds new values to existing ValuesByMonth values
        /// </summary>
        /// <param name="existingValues">Current values of ValuesByMonth</param>
        /// <param name="newValues">New values to be added</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        
        public static void Sum(this ValuesByMonth<decimal> existingValues, ValuesByMonth<decimal> newValues)
        {
            if (existingValues != null && newValues != null)
            {
                existingValues.January += newValues.January;
                existingValues.February += newValues.February;
                existingValues.March += newValues.March;
                existingValues.April += newValues.April;
                existingValues.May += newValues.May;
                existingValues.June += newValues.June;
                existingValues.July += newValues.July;
                existingValues.August += newValues.August;
                existingValues.September += newValues.September;
                existingValues.October += newValues.October;
                existingValues.November += newValues.November;
                existingValues.December += newValues.December;
            }
        }
    }

    /// <summary>
    /// Extensions for LaborRollupByDateNew
    /// </summary>
    public static class LaborRollupByDateNewExtensions
    {
        /// <summary>
        /// Adds new values to existing LaborRollupByDateNew values
        /// </summary>
        /// <param name="existingValues">Current values for LaborRollupByDateNew</param>
        /// <param name="newValues">New values to be added</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        
        private static void Sum(this LaborRollupByDateNew existingValues, LaborRollupByDateNew newValues)
        {
            existingValues.January += newValues.January;
            existingValues.February += newValues.February;
            existingValues.March += newValues.March;
            existingValues.April += newValues.April;
            existingValues.May += newValues.May;
            existingValues.June += newValues.June;
            existingValues.July += newValues.July;
            existingValues.August += newValues.August;
            existingValues.September += newValues.September;
            existingValues.October += newValues.October;
            existingValues.November += newValues.November;
            existingValues.December += newValues.December;
        }

        /// <summary>
        /// Merge two LaborRollupByDateNew ILists
        /// </summary>
        /// <param name="existingValues">Current LaborRollupByDateNew values</param>
        /// <param name="newValues">New values to be merged</param>
        /// <returns>returns new, merged values</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        
        public static List<LaborRollupByDateNew> Merge(this IList<LaborRollupByDateNew> existingValues, IList<LaborRollupByDateNew> newValues)
        {
            ICollection<int> years = existingValues.Select(e => e.Year).Union(newValues.Select(e => e.Year)).OrderBy(y => y).ToList();
            List<LaborRollupByDateNew> result = new List<LaborRollupByDateNew>(years.Count);

            foreach (int year in years)
            {
                LaborRollupByDateNew existingYearRollup = existingValues.FirstOrDefault(e => e.Year == year);
                LaborRollupByDateNew newYearRollup = newValues.FirstOrDefault(e => e.Year == year);

                if (existingYearRollup == null)
                {
                    result.Add(newYearRollup.Clone() as LaborRollupByDateNew);
                }
                else if (newYearRollup == null)
                {
                    result.Add(existingYearRollup.Clone() as LaborRollupByDateNew);
                }
                else
                {
                    // merge
                    LaborRollupByDateNew mergedRollup = existingYearRollup.Clone() as LaborRollupByDateNew;
                    mergedRollup.Sum(newYearRollup);
                    result.Add(mergedRollup);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts LaborRollupByDateNew to LaborRollupByDate
        /// </summary>
        /// <param name="existingValues">LaborRollupByDateNew IList to be converted</param>
        /// <returns>value converted to LaborRollupByDate List</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        
        internal static List<LaborRollupByDate> ConvertToLaborRollupByDate(this IList<LaborRollupByDateNew> existingValues)
        {
            if (existingValues == null)
            {
                throw new ArgumentNullException(nameof(existingValues));
            }

            List<LaborRollupByDate> results = new List<LaborRollupByDate>(existingValues.Count);

            foreach (LaborRollupByDateNew existing in existingValues)
            {
                results.Add(existing.ConvertToLaborRollupByDate());
            }

            return results;
        }

        /// <summary>
        /// Converts LaborRollupByDateNew to LaborRollupByDate
        /// </summary>
        /// <param name="existing">LaborRollupByDateNew to be converted</param>
        /// <returns>value converted to LaborRollupByDate</returns>
        internal static LaborRollupByDate ConvertToLaborRollupByDate(this LaborRollupByDateNew existing)
        {
            return new LaborRollupByDate
            {
                January = existing.January,
                February = existing.February,
                March = existing.March,
                April = existing.April,
                May = existing.May,
                June = existing.June,
                July = existing.July,
                August = existing.August,
                September = existing.September,
                October = existing.October,
                November = existing.November,
                December = existing.December,
                Resource = existing.Resource,
                Year = existing.Year
            };
        }
    }

    /// <summary>
    /// Extensions for Rollups
    /// </summary>
    public static class RollupExtensions
    {
        /// <summary>
        /// Converts LaborRollupByDateNew IList to RollupSummaryByYearTableRowData IList
        /// </summary>
        /// <param name="rollupData">Data to be converted</param>
        /// <returns>Converted data as RollupSummaryByYearTableRowData</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        
        public static IList<RollupSummaryByYearTableRowData> Convert(this IList<LaborRollupByDateNew> rollupData)
        {
            IList<RollupSummaryByYearTableRowData> results;

            if (rollupData == null)
            {
                results = new List<RollupSummaryByYearTableRowData>(0);
            }
            else
            {
                results = new List<RollupSummaryByYearTableRowData>(rollupData.Count);

                foreach (LaborRollupByDateNew rollupEntry in rollupData)
                {
                    ValuesByMonth<decimal> monthlyValues = new ValuesByMonth<decimal>
                    {
                        January = rollupEntry.January,
                        February = rollupEntry.February,
                        March = rollupEntry.March,
                        April = rollupEntry.April,
                        May = rollupEntry.May,
                        June = rollupEntry.June,
                        July = rollupEntry.July,
                        August = rollupEntry.August,
                        September = rollupEntry.September,
                        October = rollupEntry.October,
                        November = rollupEntry.November,
                        December = rollupEntry.December
                    };

                    RollupSummaryByYearTableRowData data = new RollupSummaryByYearTableRowData
                    {
                        Year = rollupEntry.Year,
                        YearTotal = rollupEntry.Total,
                        MonthlyValues = monthlyValues
                    };

                    results.Add(data);
                }
            }

            return results;
        }

        /// <summary>
        /// Converts rollup data dictionary into RollupSummaryByGroupByYearTableData
        /// </summary>
        /// <param name="rollupData">rollup data to be converted</param>
        /// <returns>converted rollup data as RollupSummaryByGroupByYearTableData</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static RollupSummaryByGroupByYearTableData Convert(this Dictionary<int, List<LaborRollupByDateNew>> rollupData)
        {
            ICollection<RollupSummaryByGroupByYearTableGroupData> groupData = new List<RollupSummaryByGroupByYearTableGroupData>();
            ICollection<RollupSummaryByYearTableRowData> summaryData = new List<RollupSummaryByYearTableRowData>();
            ValuesByMonth<decimal> summaryTotalsByMonth = new ValuesByMonth<decimal>();
            decimal summaryTotalComplete = 0m;

            /*
             * RESOURCE YEAR JAN FEB MAR APR MAY JUN JUL AUG SEP OCT NOV DEC TOTAL
             * 
             * ----------------------------- GROUP R1 ----------------------------
             * R1       2012   5   5   5   5   5   5   5   5   5   5   5   5    60
             * R1       2013   4   4   4   4   4   4   4   4   4   4   4   4    48
             *                                                          TOTAL  108  <--- group subtotal
             * ----------------------------- GROUP R2 ----------------------------
             * R2       2012   6   6   6   6   6   6   6   6   6   6   6   6    72
             * R2       2013   8   8   8   8   8   8   8   8   8   8   8   8    92
             *                                                          TOTAL  164
             * --------------------------- SUMMARY DATA --------------------------
             * SUMMARY  2012  11  11  11  11  11  11  11  11  11  11  11  11   132
             *          2013  12  12  12  12  12  12  12  12  12  12  12  12   144
             * --------------------- SUMMARY TOTALS BY MONTH ---------------------
             *         TOTAL  23  23  23  23  23  23  23  23  23  23  23  23   276  <--- summary total complete
             * 
             */

            if (rollupData != null)
            {
                // key: resource ID
                foreach (KeyValuePair<int, List<LaborRollupByDateNew>> rollupDataEntry in rollupData)
                {
                    string groupName = string.Empty;
                    decimal groupSubtotal = 0m;

                    List<RollupSummaryByYearTableRowData> groupYearlyData = new List<RollupSummaryByYearTableRowData>();

                    foreach (LaborRollupByDateNew entry in rollupDataEntry.Value)
                    {
                        groupName = entry.Resource;

                        RollupSummaryByYearTableRowData summaryDataRow = new RollupSummaryByYearTableRowData
                        {
                            Year = entry.Year,
                            YearTotal = entry.Total,
                            MonthlyValues = new ValuesByMonth<decimal>
                            {
                                January = entry.January,
                                February = entry.February,
                                March = entry.March,
                                April = entry.April,
                                May = entry.May,
                                June = entry.June,
                                July = entry.July,
                                August = entry.August,
                                September = entry.September,
                                October = entry.October,
                                November = entry.November,
                                December = entry.December
                            }
                        };

                        groupYearlyData.Add(summaryDataRow);

                        groupSubtotal += entry.Total;

                        #region Accumulate summary data

                        RollupSummaryByYearTableRowData yearlySummaryRow;
                        if ((yearlySummaryRow = summaryData.FirstOrDefault(s => s.Year == summaryDataRow.Year)) == null)
                        {
                            // initialize summary row for the year
                            yearlySummaryRow = new RollupSummaryByYearTableRowData
                            {
                                Year = summaryDataRow.Year,
                                YearTotal = 0m,
                                MonthlyValues = new ValuesByMonth<decimal>()
                            };

                            summaryData.Add(yearlySummaryRow);
                        }

                        // summary data by year
                        yearlySummaryRow.YearTotal += summaryDataRow.YearTotal;
                        yearlySummaryRow.MonthlyValues.January += summaryDataRow.MonthlyValues.January;
                        yearlySummaryRow.MonthlyValues.February += summaryDataRow.MonthlyValues.February;
                        yearlySummaryRow.MonthlyValues.March += summaryDataRow.MonthlyValues.March;
                        yearlySummaryRow.MonthlyValues.April += summaryDataRow.MonthlyValues.April;
                        yearlySummaryRow.MonthlyValues.May += summaryDataRow.MonthlyValues.May;
                        yearlySummaryRow.MonthlyValues.June += summaryDataRow.MonthlyValues.June;
                        yearlySummaryRow.MonthlyValues.July += summaryDataRow.MonthlyValues.July;
                        yearlySummaryRow.MonthlyValues.August += summaryDataRow.MonthlyValues.August;
                        yearlySummaryRow.MonthlyValues.September += summaryDataRow.MonthlyValues.September;
                        yearlySummaryRow.MonthlyValues.October += summaryDataRow.MonthlyValues.October;
                        yearlySummaryRow.MonthlyValues.November += summaryDataRow.MonthlyValues.November;
                        yearlySummaryRow.MonthlyValues.December += summaryDataRow.MonthlyValues.December;

                        // summary data overall monthly totals
                        summaryTotalsByMonth.January += summaryDataRow.MonthlyValues.January;
                        summaryTotalsByMonth.February += summaryDataRow.MonthlyValues.February;
                        summaryTotalsByMonth.March += summaryDataRow.MonthlyValues.March;
                        summaryTotalsByMonth.April += summaryDataRow.MonthlyValues.April;
                        summaryTotalsByMonth.May += summaryDataRow.MonthlyValues.May;
                        summaryTotalsByMonth.June += summaryDataRow.MonthlyValues.June;
                        summaryTotalsByMonth.July += summaryDataRow.MonthlyValues.July;
                        summaryTotalsByMonth.August += summaryDataRow.MonthlyValues.August;
                        summaryTotalsByMonth.September += summaryDataRow.MonthlyValues.September;
                        summaryTotalsByMonth.October += summaryDataRow.MonthlyValues.October;
                        summaryTotalsByMonth.November += summaryDataRow.MonthlyValues.November;
                        summaryTotalsByMonth.December += summaryDataRow.MonthlyValues.December;

                        // summary data overall total (all years)
                        summaryTotalComplete += summaryDataRow.YearTotal;

                        #endregion
                    }

                    RollupSummaryByGroupByYearTableGroupData groupDataRow = new RollupSummaryByGroupByYearTableGroupData
                    {
                        GroupName = groupName,
                        YearlyData = groupYearlyData,
                        GroupSubTotal = groupSubtotal
                    };

                    groupData.Add(groupDataRow);
                }
            }

            RollupSummaryByGroupByYearTableData laborSummaryRollupData = new RollupSummaryByGroupByYearTableData
            {
                GroupData = groupData,
                SummaryData = summaryData,
                SummaryTotalComplete = summaryTotalComplete,
                SummaryTotalsByMonth = summaryTotalsByMonth
            };

            return laborSummaryRollupData;
        }

        /// <summary>
        /// Converts rollup data dictionary to RollupSummaryByYearTableData
        /// </summary>
        /// <param name="rollupData">rollup data to be converted</param>
        /// <returns>converted rollup data as RollupSummaryByYearTableData</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static RollupSummaryByYearTableData ConvertToRollupSummaryByYear(this Dictionary<int, List<LaborRollupByDateNew>> rollupData)
        {
            decimal summaryTotalComplete = 0m;
            List<RollupSummaryByYearTableRowData> allYearlyData = new List<RollupSummaryByYearTableRowData>();

            if (rollupData != null)
            {
                // key: resource ID
                foreach (KeyValuePair<int, List<LaborRollupByDateNew>> rollupDataEntry in rollupData)
                {
                    IList<RollupSummaryByYearTableRowData> resourceYearlyData = rollupDataEntry.Value.Convert();

                    // merge/accumulate year-by-year data
                    foreach (RollupSummaryByYearTableRowData yearlyData in resourceYearlyData)
                    {
                        RollupSummaryByYearTableRowData currentYearlyData;
                        if ((currentYearlyData = allYearlyData.FirstOrDefault(d => d.Year == yearlyData.Year)) == null)
                        {
                            // create a new yearly data row
                            allYearlyData.Add(yearlyData.Clone() as RollupSummaryByYearTableRowData);
                        }
                        else
                        {
                            // merge into the existing yearly data row
                            currentYearlyData.YearTotal += yearlyData.YearTotal;
                            currentYearlyData.MonthlyValues.Sum(yearlyData.MonthlyValues);
                        }
                        summaryTotalComplete += yearlyData.YearTotal;
                    }
                }
            }

            return new RollupSummaryByYearTableData
            {
                SummaryTotalComplete = summaryTotalComplete,
                YearlyData = allYearlyData
            };
        }

        /// <summary>
        /// Converts LaborRollupByDateNew IList into RollupSummaryByYearTableData
        /// </summary>
        /// <param name="rollupData">rollup data to be converted</param>
        /// <returns>converted rollup data as RollupSummaryByYearTableData</returns>
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static RollupSummaryByYearTableData ConvertToRollupSummaryByYear(this IList<LaborRollupByDateNew> rollupData)
        {
            IList<RollupSummaryByYearTableRowData>  yearlyData = rollupData.Convert();

            return new RollupSummaryByYearTableData
            {
                SummaryTotalComplete = yearlyData.Sum(d => d.YearTotal),
                YearlyData = yearlyData
            };
        }

        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static List<LaborRollupByDateNew> Clone(this IList<LaborRollupByDateNew> rollupData)
        {
            List<LaborRollupByDateNew> clone = null;

            if (rollupData != null)
            {
                clone = new List<LaborRollupByDateNew>(rollupData.Count);

                foreach (LaborRollupByDateNew item in rollupData)
                {
                    clone.Add(item.Clone() as LaborRollupByDateNew);
                }
            }

            return clone;
        }

        /// <summary>
        /// Converts BOEExportTaskElementLabor Collection of Resources Data into LaborResourceTypesTableData
        /// </summary>
        /// <param name="resourcesData">Resource data to be converted</param>
        /// <param name="decimalPrecision">The decimal precision.</param>
        /// <returns>Converted resource data as LaborResourceTypesTableData</returns>
        public static LaborResourceTypesTableData Convert(this Collection<BOEExportTaskElementLabor> resourcesData, int decimalPrecision)
        {
            List<LaborResourceTypesTableRowData> laborResourcesData = new List<LaborResourceTypesTableRowData>();

            long costTotal = 0;
            decimal hoursTotal = 0;

            if (resourcesData != null)
            {
                foreach (BOEExportTaskElementLabor resource in resourcesData)
                {
                    long cost = resource.Cost.HasValue ? System.Convert.ToInt64(Math.Round(resource.Cost.Value, 0, MidpointRounding.AwayFromZero)) : 0;
                    decimal hours = resource.Hours ?? 0m;

                    costTotal += cost;
                    hoursTotal += hours;

                    // Resource custom field data
                    ICollection<ExportCustomField> resourceCustomFields = new List<ExportCustomField>();

                    laborResourcesData.Add(new LaborResourceTypesTableRowData
                    {
                        LaborTypeID = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_LaborTypeID),
                        Cost = cost,
                        CustomFields = resourceCustomFields,
                        ElementOfCost = resource.ElementType.GetDescription(),
                        Hours = Utilities.FormatStringWithPrecision(hours, decimalPrecision),
                        PerformingOrgDescription = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_PerformingOrgDescription),
                        PerformingOrgID = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_PerformingOrgID),
                        PerformingOrgName = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_PerformingOrg),
                        ResourceDescription = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_ResourceDescription),
                        ResourceID = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_ResourceID),
                        ResourceName = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_ResourceName),
                        ResourceType = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_LaborTypes),
                        Segment = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_Segment),
                        SegmentRegion = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_SegmentRegion),
                        SpreadCurve = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_SpreadCurve),
                        WbsString = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_WBSString),
                        ClinString = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_CLINString),
                        SummaryReference = GetStringValue(resource.ExportFields, BOEExporterConstants.FieldName_SummaryReference)
                    });
                }
            }

            LaborResourceTypesTableData laborPerfOrgsData = new LaborResourceTypesTableData
            {
                ResourcesData = laborResourcesData,
                HoursTotalComplete = hoursTotal,
                CostTotalComplete = costTotal
            };

            return laborPerfOrgsData;
        }

        /// <summary>
        /// Converts BOEExportTaskElementLabor Collection of Resources Data into TravelResourceTypesTableData
        /// </summary>
        /// <param name="resourcesData">Resources data to be converted</param>
        /// <returns>converted resources data as TravelResourceTypesTableData</returns>
        public static TravelResourceTypesTableData ConvertTravel(this Collection<BOEExportTaskElementLabor> resourcesData)
        {
            List<TravelResourceTypesTableRowData> travelResourcesData = new List<TravelResourceTypesTableRowData>();

            long costTotal = 0;

            if (resourcesData != null)
            {
                foreach (BOEExportTaskElementLabor boeLaborType in resourcesData)
                {
                    long cost = boeLaborType.Cost.HasValue ? System.Convert.ToInt64(Math.Round(boeLaborType.Cost.Value, 0, MidpointRounding.AwayFromZero)) : 0;

                    costTotal += cost;

                    travelResourcesData.Add(new TravelResourceTypesTableRowData
                    {
                        Cost = cost,
                        ElementOfCost = boeLaborType.ElementType.GetDescription(),
                        PerformingOrgID = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_PerformingOrgID),
                        PerformingOrgDescription = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_PerformingOrgDescription),
                        PerformingOrgName = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_PerformingOrg),
                        ResourceDescription = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_ResourceDescription),
                        ResourceID = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_ResourceID),
                        ResourceName = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_ResourceName),
                        ResourceType = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_LaborTypes),
                        Segment = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_Segment),
                        SegmentRegion = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_SegmentRegion),
                        Date = GetDateTimeValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_TaskTypeDate),
                        Days = GetIntValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_TaskTypeDays),
                        Departure = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_TaskTypeDeparture),
                        Destination = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_TaskTypeDestination),
                        Mode = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_Mode),
                        People = GetIntValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_TaskTypePeople),
                        Purpose = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_TaskTypePurpose),
                        Trips = GetIntValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_TaskTypeTrips)
                    });
                }
            }

            TravelResourceTypesTableData travelPerfOrgsData = new TravelResourceTypesTableData
            {
                ResourcesData = travelResourcesData,
                CostTotalComplete = costTotal
            };

            return travelPerfOrgsData;
        }

        /// <summary>
        /// Converts BOEExportTaskElementLabor Collection of Resources Data into TravelResourceTypesTableData
        /// </summary>
        /// <param name="resourcesData">Resources data to be converted</param>
        /// <returns>converted resources data as TravelResourceTypesTableData</returns>
        public static RMSTravelResourceTypesTableData ConvertRMSTravel(this Collection<BOEExportTaskElementLabor> resourcesData)
        {
            List<RMSTravelResourceTypesTableRowData> travelResourcesData = new List<RMSTravelResourceTypesTableRowData>();

            long costTotal = 0;

            if (resourcesData != null)
            {
                foreach (BOEExportTaskElementLabor boeLaborType in resourcesData)
                {
                    long cost = boeLaborType.Cost.HasValue ? System.Convert.ToInt64(Math.Round(boeLaborType.Cost.Value, 0, MidpointRounding.AwayFromZero)) : 0;

                    costTotal += cost;

                    bool isAirfare = boeLaborType.ExportFields[BOEExporterConstants.FieldName_Mode] == MSTTravelMode.ZoneAirfare.ToDescription();

                    travelResourcesData.Add(new RMSTravelResourceTypesTableRowData
                    {
                        Cost = cost,
                        GroupID = GetIntValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_GroupID),
                        ResourceName = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_ResourceName),
                        Departure = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_TaskTypeDeparture),
                        Destination = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_TaskTypeDestination),
                        Zone = GetIntValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_Zone),
                        Purpose = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_TaskTypePurpose),
                        TripDate = GetDateTimeValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_TripDate),
                        Days = GetDecimalValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_Days),
                        People = GetDecimalValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_People),
                        Cars = GetDecimalValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_Cars),
                        IsAirfareTrip = isAirfare,
                        SecondaryResourceName = isAirfare ? GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_SecondaryResourceName) : string.Empty
                    });
                }
            }

            RMSTravelResourceTypesTableData travelPerfOrgsData = new RMSTravelResourceTypesTableData
            {
                ResourcesData = travelResourcesData,
                CostTotalComplete = costTotal
            };

            return travelPerfOrgsData;
        }

        /// <summary>
        /// Converts BOEExportTaskElementLabor Collection of Resources Data into ODCResourceTypesTableData
        /// </summary>
        /// <param name="resourcesData">resources data to be converted</param>
        /// <returns>converted resources data as ODCResourceTypesTableData</returns>
        public static ODCResourceTypesTableData ConvertODC(this Collection<BOEExportTaskElementLabor> resourcesData)
        {
            List<ODCResourceTypesTableRowData> odcResourcesData = new List<ODCResourceTypesTableRowData>();

            long costTotal = 0;

            if (resourcesData != null)
            {
                foreach (BOEExportTaskElementLabor boeLaborType in resourcesData)
                {
                    long cost = boeLaborType.Cost.HasValue ? System.Convert.ToInt64(Math.Round(boeLaborType.Cost.Value, 0, MidpointRounding.AwayFromZero)) : 0;

                    costTotal += cost;

                    odcResourcesData.Add(new ODCResourceTypesTableRowData
                    {
                        Cost = cost,
                        ElementOfCost = boeLaborType.ElementType.GetDescription(),
                        PerformingOrgID = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_PerformingOrgID),
                        PerformingOrgDescription = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_PerformingOrgDescription),
                        PerformingOrgName = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_PerformingOrg),
                        ResourceDescription = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_ResourceDescription),
                        ResourceID = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_ResourceID),
                        ResourceName = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_ResourceName),
                        ResourceType = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_LaborTypes),
                        Segment = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_Segment),
                        SegmentRegion = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_SegmentRegion)
                    });
                }
            }

            ODCResourceTypesTableData odcPerfOrgsData = new ODCResourceTypesTableData
            {
                ResourcesData = odcResourcesData,
                CostTotalComplete = costTotal
            };

            return odcPerfOrgsData;
        }

        /// <summary>
        /// Converts BOEExportTaskElementLabor Collection of Resources Data into MaterialResourceTypesTableData
        /// </summary>
        /// <param name="resourcesData">resources data to be converted</param>
        /// <returns>converted resources data as MaterialResourceTypesTableData</returns>
        public static MaterialResourceTypesTableData ConvertMaterial(this Collection<BOEExportTaskElementLabor> resourcesData)
        {
            List<MaterialResourceTypesTableRowData> materialResourcesData = new List<MaterialResourceTypesTableRowData>();

            long costTotal = 0;

            if (resourcesData != null)
            {
                foreach (BOEExportTaskElementLabor boeLaborType in resourcesData)
                {
                    long cost = boeLaborType.Cost.HasValue ? System.Convert.ToInt64(Math.Round(boeLaborType.Cost.Value, 0, MidpointRounding.AwayFromZero)) : 0;

                    costTotal += cost;

                    materialResourcesData.Add(new MaterialResourceTypesTableRowData
                    {
                        Cost = cost,
                        PerformingOrgID = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_PerformingOrgID),
                        PerformingOrgDescription = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_PerformingOrgDescription),
                        PerformingOrgName = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_PerformingOrg),
                        ResourceDescription = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_ResourceDescription),
                        ResourceID = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_ResourceID),
                        ResourceName = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_ResourceName),
                        ResourceType = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_LaborTypes),
                        Segment = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_Segment),
                        SegmentRegion = GetStringValue(boeLaborType.ExportFields, BOEExporterConstants.FieldName_SegmentRegion)
                    });
                }
            }

            MaterialResourceTypesTableData materialPerfOrgsData = new MaterialResourceTypesTableData
            {
                ResourcesData = materialResourcesData,
                CostTotalComplete = costTotal
            };

            return materialPerfOrgsData;
        }

        /// <summary>
        /// Get datetime value in dictionary based on key
        /// </summary>
        /// <param name="dictionary">Dictionary containing datetime value and key</param>
        /// <param name="key">key to search the dictionary for</param>
        /// <returns>datetime value associated with key, returns DateTime.MinValue if key not found</returns>
        private static DateTime GetDateTimeValue(IDictionary<string, string> dictionary, string key)
        {
            string stringValue = GetStringValue(dictionary, key);

            DateTime result;

            if (!DateTime.TryParse(stringValue, out result))
            {
                result = DateTime.MinValue;
            }

            return result;
        }

        /// <summary>
        /// Get int value in dictionary based on key
        /// </summary>
        /// <param name="dictionary">Dictionary containing int value and key</param>
        /// <param name="key">key to search the dictionary for</param>
        /// <returns>int value associated with key, returns 0 if key not found</returns>
        private static int GetIntValue(IDictionary<string, string> dictionary, string key)
        {
            string stringValue = GetStringValue(dictionary, key);

            int result;

            if (!int.TryParse(stringValue, out result))
            {
                result = 0;
            }

            return result;
        }

        /// <summary>
        /// Get decimal value in dictionary based on key
        /// </summary>
        /// <param name="dictionary">Dictionary containing int value and key</param>
        /// <param name="key">key to search the dictionary for</param>
        /// <returns>int value associated with key, returns 0 if key not found</returns>
        private static decimal GetDecimalValue(IDictionary<string, string> dictionary, string key)
        {
            string stringValue = GetStringValue(dictionary, key);

            decimal result;

            if (!decimal.TryParse(stringValue, out result))
            {
                result = 0m;
            }

            return result;
        }

        /// <summary>
        /// Get string value in dictionary based on key
        /// </summary>
        /// <param name="dictionary">Dictionary containing string value and key</param>
        /// <param name="key">key to search the dictionary for</param>
        /// <returns>string value associated with key, returns string.Empty if key not found</returns>
        private static string GetStringValue(IDictionary<string, string> dictionary, string key)
        {
            string value = string.Empty;

            if (dictionary.ContainsKey(key))
            {
                value = dictionary[key] ?? string.Empty;
            }

            return value;
        }
    }

    #endregion

    #region Enums

    /// <summary>
    /// Enum defining the type of rollup needed
    /// </summary>
    internal enum RollupTypeNew
    {
        None = 0,
        Summary = 1,
        TaskDetailSummary = 2,
        MaterialTaskDetailSummary = 3,
        ODCDetail = 4,
        TravelDetail = 5,
        ODCAndTravel = 6
    }

    #endregion
}
