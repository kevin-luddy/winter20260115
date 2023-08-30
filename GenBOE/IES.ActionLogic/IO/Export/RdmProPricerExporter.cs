// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace IES.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using IES.Common;
    using IES.Common.Compression;
    using IES.Common.Exceptions;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// The RDM ProPricer Exporter.
    ///
    /// Naming convention for Direct Rate export files:  (Note: Direct Rates are always paired with Burden Pools)
    ///   - Commercial Rates (GRS) - Includes unallowable expenses, filename=YYYY-MM-DD_Direct_Rate_Commercial.xlsx (Burden Pools with -G suffix)
    ///   - Government Rates (NET) - Net of unallowable expenses, filename= YYYY-MM-DD_Direct_Rate.xlsx  (Burden Pools without -G suffix)
    ///   - Government Rates Services (NET) - Copy of Government rates for Services (Non-Commercial), filename= YYY-MM-DD_Direct_Rate_Services.xlsx (Burden Pools without -G suffix)
    ///
    /// Naming convention for Burden Rate export files:
    ///   - Government Rates (NET) - Net of unallowable expenses, filename= YYYY-MM-DD_Burden_Rate.xlsx(Burden Pools without -G suffix)
    ///   - Commercial Rates (GRS) - Includes unallowable expenses, filename=YYYY-MM-DD_Burden_Rate_Commercial.xlsx(Burden Pools with -G suffix) 
    ///   - Mission Solutions (Government Rates) (NET) - Net of unallowable expenses, filename= YYYY-MM-DD_Burden_Rate_Services.xlsx (Used by Mission Solutions when Core Costs are proposed. It adds Services G&amp;A )
    /// 
    /// Notes for Burden Rate export files:
    ///       All files get all of the burden pools.
    ///       The Government and Commercial Rates files are identical.
    ///       The only difference is that the Mission Solutions export gets the "MS Only" Rates included; whereas, the Govt &amp; Comm blank out the fields that are set for "MS Only".
    /// </summary>
    public class RdmProPricerExporter
    {
        #region Properties
        /// <summary>
        /// ZipPathFile
        /// </summary>
        private string ZipFilePath { get; set; }

        /// <summary>
        /// Rates
        /// </summary>
        private ICollection<RateDetailModelView> Rates { get; set; }

        /// <summary>
        /// BurdenPools
        /// </summary>
        private ICollection<BurdenPoolDetailModelView> BurdenPools { get; set; }

        /// <summary>
        /// BurdenElements
        /// </summary>
        private ICollection<BurdenElementModelView> BurdenElements { get; set; }

        /// <summary>
        /// MinYear
        /// </summary>
        internal int MinYear { get; private set; }

        /// <summary>
        /// MaxYear
        /// </summary>
        internal int MaxYear { get; private set; }

        /// <summary>
        /// prefix
        /// </summary>
        private string prefix = DateTime.Today.ToString(Constants.DATE_FORMATTING_YEAR_MONTH_DAY);
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rates">rates</param>
        /// <param name="zipFilePath">zipFilePath</param>
        /// <param name="burdenPools">burdenPools</param>
        /// <param name="burdenElements">burdenelements</param>
        public RdmProPricerExporter(ICollection<RateDetailModelView> rates, string zipFilePath,
            ICollection<BurdenPoolDetailModelView> burdenPools,
            ICollection<BurdenElementModelView> burdenElements)
        {
            this.Rates = rates;
            this.ZipFilePath = zipFilePath;
            this.MinYear = 0;
            this.MaxYear = 0;
            this.BurdenElements = burdenElements;
            this.BurdenPools = burdenPools;
        }
        #region Public Methods

        /// <summary>
        /// This function handles exporting to ProPricer
        /// </summary>
        /// <returns>Location of the zipped files.</returns>
        public string ExportReport()
        {
            this.FindMinMaxYears(); // set min/max year
            string toReturn;
            // Initialize collection of streams to zip
            Dictionary<string, Stream> zipContents = new Dictionary<string, Stream>();
            // Add streams to dictionary for zipping, generate CSV data for the 5 ProPricer exports.
            using (MemoryStream ms1 = new MemoryStream(ExportProPricer(this.GetDirectRateRows(IsGovOrComm.Commerical))))
            {
                zipContents.Add(string.Format("{0}_Direct_Rate_Commercial.csv", this.prefix), ms1);
                // Note: ms2 (Government Direct Rates (NET)) and ms6 (Government Direct Rates Services (NET)) are identical as requested by the customer.
                using (MemoryStream ms2 = new MemoryStream(ExportProPricer(this.GetDirectRateRows(IsGovOrComm.Government))))
                {
                    zipContents.Add(string.Format("{0}_Direct_Rate.csv", this.prefix), ms2);
                    // Note: ms3 (Government Burden Rates (NET)) and ms4 (Commercial Burden Rates (GRS)) are identical as requested by the customer.
                    using (MemoryStream ms3 = new MemoryStream(ExportProPricer(this.GetBurdenRateRows(false, false))))
                    {
                        zipContents.Add(string.Format("{0}_Burden_Rate.csv", this.prefix), ms3);
                        using (MemoryStream ms4 = new MemoryStream(ExportProPricer(this.GetBurdenRateRows(false, true))))
                        {
                            zipContents.Add(string.Format("{0}_Burden_Rate_Commercial.csv", this.prefix), ms4);
                            using (MemoryStream ms5 = new MemoryStream(ExportProPricer(this.GetBurdenRateRows(true, false))))
                            {
                                zipContents.Add(string.Format("{0}_Burden_Rate_Services.csv", this.prefix), ms5);
                                using (MemoryStream ms6 = new MemoryStream(ExportProPricer(this.GetDirectRateRows(IsGovOrComm.Government))))
                                {
                                    zipContents.Add(string.Format("{0}_Direct_Rate_Services.csv", this.prefix), ms6);
                                    // Zip files and return zip file path & name to caller
                                    toReturn = Zip.ZipFiles(zipContents, this.ZipFilePath);
                                }
                           }
                        }
                    }
                }
            }

            return toReturn;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Converts ProPricer Direct Rate or Burden Rate data to a byte array containing rows of comma separated strings.
        /// </summary>
        /// <param name="proPricerExportRows">A collection of ProPricerExportRowModelViews to be exported.</param>
        /// <returns>Byte Array containing comma separated row data.</returns>
        private static byte[] ExportProPricer(ICollection<IProPricerExportModelView> proPricerExportRows)
        {
            StringBuilder sb = new StringBuilder();

            // combine the rows into a single string
            foreach (IProPricerExportModelView row in proPricerExportRows)
            {
                row.ExportString(sb);
            }

            // return byte array representation of the data
            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        /// <summary>
        /// Get Direct Rate Commercial export row model views.
        /// </summary>
        /// <param name="isGovOrComm">IsGovOrComm enum</param>
        /// <returns>Collection of ProPricer export model views.</returns>
        internal ICollection<IProPricerExportModelView> GetDirectRateRows(IsGovOrComm isGovOrComm)
        {
            ICollection<IProPricerExportModelView> toReturn = new Collection<IProPricerExportModelView>();
            toReturn.Add(ProPricerDirectRateExportRowModelView.GetDirectRateHeaderModelView()); // get col header names
            foreach (RateDetailModelView rate in this.Rates)
            {
                // Only export rates if at least one description is specified.
                if (string.IsNullOrEmpty(rate.RateDescription) &&
                    string.IsNullOrEmpty(rate.RateDescription1) &&
                    string.IsNullOrEmpty(rate.RateDescription2) &&
                    string.IsNullOrEmpty(rate.RateDescription3) &&
                    string.IsNullOrEmpty(rate.RateDescription4) &&
                    string.IsNullOrEmpty(rate.RateDescription5) &&
                    string.IsNullOrEmpty(rate.RateDescription6) &&
                    string.IsNullOrEmpty(rate.RateDescription7) &&
                    string.IsNullOrEmpty(rate.RateDescription8))
                {
                    continue;   // skip
                }

                // Determine if we have rate descriptions for 1-8, if so we have to create mappings
                if (!string.IsNullOrEmpty(rate.RateDescription))
                {
                    toReturn.AddRange(this.CreateDirectRateRows(rate, 0, rate.RateDescription, rate.ResourceClass, isGovOrComm));
                }

                if (!string.IsNullOrEmpty(rate.RateDescription1))
                {
                    toReturn.AddRange(this.CreateDirectRateRows(rate, 1, rate.RateDescription1, rate.ResourceClass1, isGovOrComm));
                }

                if (!string.IsNullOrEmpty(rate.RateDescription2))
                {
                    toReturn.AddRange(this.CreateDirectRateRows(rate, 2, rate.RateDescription2, rate.ResourceClass2, isGovOrComm));
                }

                if (!string.IsNullOrEmpty(rate.RateDescription3))
                {
                    toReturn.AddRange(this.CreateDirectRateRows(rate, 3, rate.RateDescription3, rate.ResourceClass3, isGovOrComm));
                }

                if (!string.IsNullOrEmpty(rate.RateDescription4))
                {
                    toReturn.AddRange(this.CreateDirectRateRows(rate, 4, rate.RateDescription4, rate.ResourceClass4, isGovOrComm));
                }

                if (!string.IsNullOrEmpty(rate.RateDescription5))
                {
                    toReturn.AddRange(this.CreateDirectRateRows(rate, 5, rate.RateDescription5, rate.ResourceClass5, isGovOrComm));
                }

                if (!string.IsNullOrEmpty(rate.RateDescription6))
                {
                    toReturn.AddRange(this.CreateDirectRateRows(rate, 6, rate.RateDescription6, rate.ResourceClass6, isGovOrComm));
                }

                if (!string.IsNullOrEmpty(rate.RateDescription7))
                {
                    toReturn.AddRange(this.CreateDirectRateRows(rate, 7, rate.RateDescription7, rate.ResourceClass7, isGovOrComm));
                }
                
                if (!string.IsNullOrEmpty(rate.RateDescription8))
                {
                    toReturn.AddRange(this.CreateDirectRateRows(rate, 8, rate.RateDescription8, rate.ResourceClass8, isGovOrComm));
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Adds rows to rate collection for export based on available rates/years for rate code
        /// Creates using appropriate mapping sequence, either 0 (for base rate) or 1-8 for AAAAA1 - AAAAAA8
        /// </summary>
        /// <param name="rate">rate</param>
        /// <param name="mappingSequence">mapping sequence 0 or 1-8</param>
        /// <param name="description">base or description for mapped code 1-8</param>
        /// <param name="resourceClass">resource class for mapped code 0-8</param>
        /// <param name="isGovOrComm">IsGovOrComm enum</param>
        /// <returns>ProPricerDirectRateExportRowModelViews</returns>
        private ICollection<ProPricerDirectRateExportRowModelView> CreateDirectRateRows(
            RateDetailModelView rate, int mappingSequence, string description, string resourceClass, IsGovOrComm isGovOrComm)
        {
            ICollection<ProPricerDirectRateExportRowModelView> toReturn = new Collection<ProPricerDirectRateExportRowModelView>();
            for (int year = this.MinYear; year <= this.MaxYear; year++)
            {
                if (rate.Values.Any(r => r.Year == year && r.Value.HasValue))
                {
                    toReturn.Add(this.CreateDirectRateRow(rate, mappingSequence, description, resourceClass, isGovOrComm, year));
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Add single row to direct rate collection for export
        /// Creates using appropriate mapping sequence, either 0 (for base rate) or 1-8 for AAAAA1 - AAAAAA8
        /// </summary>
        /// <param name="rate">rate</param>
        /// <param name="mappingSequence">mapping sequence 0 or 1-8</param>
        /// <param name="description">base or description for mapped code 1-8</param>
        /// <param name="resourceClass">resource class for mapped code 0-8</param>
        /// <param name="isGovOrComm">IsGovOrComm enum</param>
        /// <param name="year">int year</param>
        /// <returns>ProPricerDirectRateExportRowModelView</returns>
        private ProPricerDirectRateExportRowModelView CreateDirectRateRow(RateDetailModelView rate, int mappingSequence, string description, string resourceClass, IsGovOrComm isGovOrComm, int year)
        {
            ProPricerDirectRateExportRowModelView toReturn = new ProPricerDirectRateExportRowModelView();
            // use base ratecode for resource or create [RATECODE]1,2,3,4,5,6,7, or 8
            toReturn.ResourceType = rate.ResourceType.GetName();
            toReturn.Resource = mappingSequence == 0 ? rate.RateCode : string.Format(rate.RateCode + "{0}", mappingSequence.ToString());
            toReturn.Description = description;
            toReturn.ResourceClass = resourceClass;
            toReturn.BurdenPool = isGovOrComm == IsGovOrComm.Government ? rate.GovernmentBurdenPool : rate.CommercialBurdenPool;
            toReturn.StartDate = string.Format("\t01/\t{0}", year); // hack stops excel from converting to date
            toReturn.EndDate = string.Format("\t12/\t{0}", year);   // hack stops excel from converting to date
            toReturn.RateType = rate.RateType.GetName();
            toReturn.BaseRate = rate.Values
                .Where(r => r.Year == year)
                .Select(r => r.Value)
                .FirstOrDefault().ToString();
            return toReturn;
        }

        /// <summary>
        /// Generate burden rate export rows.
        /// </summary>
        /// <param name="isMissionSolutionsExport">true if Mission Solutions Only Rates should be included; false otherwise.</param>
        /// <param name="isCommercialExport">true if this is the Commercial Export; false otherwise.</param>
        /// <returns>Collection of ProPricer export model views.</returns>
        internal ICollection<IProPricerExportModelView> GetBurdenRateRows(bool isMissionSolutionsExport, bool isCommercialExport)
        {
            ICollection<IProPricerExportModelView> toReturn = new Collection<IProPricerExportModelView>();
            // get the index of the "G&A T2" (Services G&A) column, as it requires special handling below
            BurdenElementModelView burdenElement = this.BurdenElements.Single(x => x.Name == "G&A T2");
            List<BurdenElementModelView> orderedList = this.BurdenElements.OrderBy(x => x.DisplayOrder).ToList();
            int gat2BurdenElementIndex = orderedList.IndexOf(burdenElement);
            List<int> fccomIndices = new List<int>();
            for (int i = 0; i < orderedList.Count; i++)
            {
                if (orderedList[i].Name.StartsWith("FCCOM") || orderedList[i].Name.StartsWith("FCCM") || orderedList[i].Name.StartsWith("FCM"))
                {
                    fccomIndices.Add(i);
                }
            }

            // Create a model view for the header row
            toReturn.Add(new ProPricerBurdenRateExportRowModelView()
            {
                BurdenPool = "Burden Pool",
                Description = "Description",
                EffectiveDate = "Effective Date",
                Date = "Date",
                Rates = this.BurdenElements.OrderBy(x => x.DisplayOrder).Select(bp => bp.Name).ToList()
            });
            
            foreach (BurdenPoolDetailModelView burdenPool in this.BurdenPools)
            {
                for (int year = this.MinYear; year <= this.MaxYear; year++)
                {
                    ProPricerBurdenRateExportRowModelView row = new ProPricerBurdenRateExportRowModelView
                    {
                        BurdenPool = burdenPool.BurdenPool,
                        Description = burdenPool.Description,
                        EffectiveDate = string.Empty, // placeholder column - always blank
                        Date = year.ToString(), // 4-digit year
                        Rates = new List<string>()
                    };

                    int burdenElementIndex = -1;
                    foreach (string rateCodeMapping in burdenPool.BurdenElementRateCodeArray)
                    {
                        burdenElementIndex++;

                        if (string.IsNullOrEmpty(rateCodeMapping))
                        {
                            row.Rates.Add(string.Empty);
                            continue;   // no mapping for this burden element
                        }

                        // certain rate codes in the "G&A T2" (Services G&A) column are excluded from the Mission Solutions export 
                        // skip the "G&A T2" (Services G&A) column rate if this is the Mission Solutions export and the burden pool is not marked/checked for "G&A T2" inclusion. 
                        if (burdenElementIndex == gat2BurdenElementIndex && isMissionSolutionsExport && !burdenPool.IsGaT2ApplicableForMissionSolutions)
                        {
                            row.Rates.Add(string.Empty);
                            continue;   // skip this mapping
                        }

                        // certain rate codes in the "G&A T2" (Services G&A) column are excluded from the Burden Rate and Commercial Burden Rate exports
                        // skip the "G&A T2" (Services G&A) column rate if this is not the Mission Solutions export and the burden pool is not marked/checked for "G&A T2" inclusion. 
                        if (burdenElementIndex == gat2BurdenElementIndex && !isMissionSolutionsExport && !burdenPool.IncludeGaT2InBurdAndCommBurdTables)
                        {
                            row.Rates.Add(string.Empty);
                            continue; // skip this mapping
                        }

                        // certain rate codes in the FCCOM columns are excluded in the Commercial export
                        // Skip the FCCOM column rates if this is the a Commercial export and the burden pool is marked to exclude FCCOM
                        if (isCommercialExport && burdenPool.ExcludeFCCOMFromCommercial && fccomIndices.Contains(burdenElementIndex))
                        {
                            row.Rates.Add(string.Empty);
                            continue;   // skip this mapping
                        }
                                                
                        // find mapped rate for this burden element
                        string rateValue = string.Empty;
                        RateDetailModelView rate = this.Rates.FirstOrDefault(x => x.RateCode == rateCodeMapping);
                        if (rate != null && rate.Values != null)
                        {
                            // find Rate for current year
                            RateYearModelView rateYearModelView = rate.Values.FirstOrDefault(v => v.Year == year);
                            if (rateYearModelView != null && rateYearModelView.Value.HasValue)
                            {
                                rateValue = rateYearModelView.Value.ToString();
                            }
                        }

                        row.Rates.Add(rateValue);
                    }

                    // only add the row if one or more Rates are populated
                    if (row.Rates.Any(x => !string.IsNullOrEmpty(x)))
                    {
                        toReturn.Add(row);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Iterate through the Rates and finds the min and max Year values.
        /// </summary>
        internal void FindMinMaxYears()
        {
            this.MinYear = 9999;
            this.MaxYear = 0;

            foreach (RateDetailModelView rate in this.Rates)
            {
                int rateMinYear = rate.Values.Min(x => x.Year);
                int rateMaxYear = rate.Values.Max(x => x.Year);
                this.MinYear = Math.Min(this.MinYear, rateMinYear);
                this.MaxYear = Math.Max(this.MaxYear, rateMaxYear);
            }

            if (this.MinYear == 9999 || this.MaxYear == 0)
            {
                throw new GeneralAppException("Unable to get min/max rate years.");
            }
        }
    }
}
#endregion