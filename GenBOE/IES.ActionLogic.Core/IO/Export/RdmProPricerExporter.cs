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
	using IES.Common.Core;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Utilities;
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
		private string prefix = DateTime.Today.ToString(CommonConstants.DATE_FORMATTING_YEAR_MONTH_DAY);
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
			Dictionary<string, Stream> zipContents = new();
			// Add streams to dictionary for zipping, generate CSV data for the 5 ProPricer exports.
			using (MemoryStream ms1 = new(ExportProPricer(this.GetDirectRateRows(IsGovOrComm.Commerical))))
			{
				zipContents.Add(string.Format("{0}_Direct_Rate_Commercial.csv", this.prefix), ms1);
				// Note: ms2 (Government Direct Rates (NET)) and ms6 (Government Direct Rates Services (NET)) are identical as requested by the customer.
				using (MemoryStream ms2 = new(ExportProPricer(this.GetDirectRateRows(IsGovOrComm.Government))))
				{
					zipContents.Add(string.Format("{0}_Direct_Rate.csv", this.prefix), ms2);
					// Note: ms3 (Government Burden Rates (NET)) and ms4 (Commercial Burden Rates (GRS)) are identical as requested by the customer.
					using (MemoryStream ms3 = new(ExportProPricer(this.GetBurdenRateRows(false, false))))
					{
						zipContents.Add(string.Format("{0}_Burden_Rate.csv", this.prefix), ms3);
						using (MemoryStream ms4 = new(ExportProPricer(this.GetBurdenRateRows(false, true))))
						{
							zipContents.Add(string.Format("{0}_Burden_Rate_Commercial.csv", this.prefix), ms4);
							using (MemoryStream ms5 = new(ExportProPricer(this.GetBurdenRateRows(true, false))))
							{
								zipContents.Add(string.Format("{0}_Burden_Rate_Services.csv", this.prefix), ms5);
								using (MemoryStream ms6 = new(ExportProPricer(this.GetDirectRateRows(IsGovOrComm.Government))))
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
			StringBuilder sb = new();

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
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
					string.IsNullOrEmpty(rate.RateDescription8) &&
					string.IsNullOrEmpty(rate.RateDescription9) &&
					string.IsNullOrEmpty(rate.RateDescription11) &&
					string.IsNullOrEmpty(rate.RateDescription12) &&
					string.IsNullOrEmpty(rate.RateDescription13) &&
					string.IsNullOrEmpty(rate.RateDescription14) &&
					string.IsNullOrEmpty(rate.RateDescription15) &&
					string.IsNullOrEmpty(rate.RateDescription21) &&
					string.IsNullOrEmpty(rate.RateDescription22) &&
					string.IsNullOrEmpty(rate.RateDescription23) &&
					string.IsNullOrEmpty(rate.RateDescription24) &&
					string.IsNullOrEmpty(rate.RateDescription25) &&
					string.IsNullOrEmpty(rate.RateDescription31) &&
					string.IsNullOrEmpty(rate.RateDescription32) &&
					string.IsNullOrEmpty(rate.RateDescription33) &&
					string.IsNullOrEmpty(rate.RateDescription34) &&
					string.IsNullOrEmpty(rate.RateDescription35) &&
					string.IsNullOrEmpty(rate.RateDescription41) &&
					string.IsNullOrEmpty(rate.RateDescription42) &&
					string.IsNullOrEmpty(rate.RateDescription43) &&
					string.IsNullOrEmpty(rate.RateDescription44) &&
					string.IsNullOrEmpty(rate.RateDescription45) &&
					string.IsNullOrEmpty(rate.RateDescription51) &&
					string.IsNullOrEmpty(rate.RateDescription52) &&
					string.IsNullOrEmpty(rate.RateDescription53) &&
					string.IsNullOrEmpty(rate.RateDescription54) &&
					string.IsNullOrEmpty(rate.RateDescription55) &&
					string.IsNullOrEmpty(rate.RateDescription61) &&
					string.IsNullOrEmpty(rate.RateDescription62) &&
					string.IsNullOrEmpty(rate.RateDescription63) &&
					string.IsNullOrEmpty(rate.RateDescription64) &&
					string.IsNullOrEmpty(rate.RateDescription65) &&
					string.IsNullOrEmpty(rate.RateDescription71) &&
					string.IsNullOrEmpty(rate.RateDescription72) &&
					string.IsNullOrEmpty(rate.RateDescription73) &&
					string.IsNullOrEmpty(rate.RateDescription74) &&
					string.IsNullOrEmpty(rate.RateDescription75) &&
					string.IsNullOrEmpty(rate.RateDescription81) &&
					string.IsNullOrEmpty(rate.RateDescription82) &&
					string.IsNullOrEmpty(rate.RateDescription83) &&
					string.IsNullOrEmpty(rate.RateDescription84) &&
					string.IsNullOrEmpty(rate.RateDescription85) &&
					string.IsNullOrEmpty(rate.RateDescription91) &&
					string.IsNullOrEmpty(rate.RateDescription92) &&
					string.IsNullOrEmpty(rate.RateDescription93) &&
					string.IsNullOrEmpty(rate.RateDescription94) &&
					string.IsNullOrEmpty(rate.RateDescription95))
				{
					continue;   // skip
				}

				// Determine if we have rate descriptions for 1-9, if so we have to create mappings
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

				if (!string.IsNullOrEmpty(rate.RateDescription9))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 9, rate.RateDescription9, rate.ResourceClass9, isGovOrComm));
				}

				#region 1LMX Rates

				// Level 1
				if (!string.IsNullOrEmpty(rate.RateDescription11))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 11, rate.RateDescription11, rate.ResourceClass11, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription12))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 12, rate.RateDescription12, rate.ResourceClass12, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription13))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 13, rate.RateDescription13, rate.ResourceClass13, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription14))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 14, rate.RateDescription14, rate.ResourceClass14, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription15))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 15, rate.RateDescription15, rate.ResourceClass15, isGovOrComm));
				}

				// Level 2
				if (!string.IsNullOrEmpty(rate.RateDescription21))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 21, rate.RateDescription21, rate.ResourceClass21, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription22))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 22, rate.RateDescription22, rate.ResourceClass22, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription23))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 23, rate.RateDescription23, rate.ResourceClass23, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription24))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 24, rate.RateDescription24, rate.ResourceClass24, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription25))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 25, rate.RateDescription25, rate.ResourceClass25, isGovOrComm));
				}

				// Level 3
				if (!string.IsNullOrEmpty(rate.RateDescription31))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 31, rate.RateDescription31, rate.ResourceClass31, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription32))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 32, rate.RateDescription32, rate.ResourceClass32, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription33))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 33, rate.RateDescription33, rate.ResourceClass33, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription34))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 34, rate.RateDescription34, rate.ResourceClass34, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription35))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 35, rate.RateDescription35, rate.ResourceClass35, isGovOrComm));
				}

				// Level 4
				if (!string.IsNullOrEmpty(rate.RateDescription41))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 41, rate.RateDescription41, rate.ResourceClass41, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription42))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 42, rate.RateDescription42, rate.ResourceClass42, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription43))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 43, rate.RateDescription43, rate.ResourceClass43, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription44))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 44, rate.RateDescription44, rate.ResourceClass44, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription45))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 45, rate.RateDescription45, rate.ResourceClass45, isGovOrComm));
				}

				// Level 5
				if (!string.IsNullOrEmpty(rate.RateDescription51))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 51, rate.RateDescription51, rate.ResourceClass51, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription52))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 52, rate.RateDescription52, rate.ResourceClass52, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription53))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 53, rate.RateDescription53, rate.ResourceClass53, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription54))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 54, rate.RateDescription54, rate.ResourceClass54, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription55))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 55, rate.RateDescription55, rate.ResourceClass55, isGovOrComm));
				}

				// Level 6
				if (!string.IsNullOrEmpty(rate.RateDescription61))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 61, rate.RateDescription61, rate.ResourceClass61, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription62))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 62, rate.RateDescription62, rate.ResourceClass62, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription63))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 63, rate.RateDescription63, rate.ResourceClass63, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription64))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 64, rate.RateDescription64, rate.ResourceClass64, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription65))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 65, rate.RateDescription65, rate.ResourceClass65, isGovOrComm));
				}

				// Level 7
				if (!string.IsNullOrEmpty(rate.RateDescription71))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 71, rate.RateDescription71, rate.ResourceClass71, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription72))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 72, rate.RateDescription72, rate.ResourceClass72, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription73))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 73, rate.RateDescription73, rate.ResourceClass73, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription74))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 74, rate.RateDescription74, rate.ResourceClass74, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription75))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 75, rate.RateDescription75, rate.ResourceClass75, isGovOrComm));
				}

				// Level 8
				if (!string.IsNullOrEmpty(rate.RateDescription81))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 81, rate.RateDescription81, rate.ResourceClass81, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription82))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 82, rate.RateDescription82, rate.ResourceClass82, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription83))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 83, rate.RateDescription83, rate.ResourceClass83, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription84))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 84, rate.RateDescription84, rate.ResourceClass84, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription85))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 85, rate.RateDescription85, rate.ResourceClass85, isGovOrComm));
				}

				// Level 9
				if (!string.IsNullOrEmpty(rate.RateDescription91))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 91, rate.RateDescription91, rate.ResourceClass91, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription92))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 92, rate.RateDescription92, rate.ResourceClass92, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription93))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 93, rate.RateDescription93, rate.ResourceClass93, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription94))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 94, rate.RateDescription94, rate.ResourceClass94, isGovOrComm));
				}

				if (!string.IsNullOrEmpty(rate.RateDescription95))
				{
					toReturn.AddRange(this.CreateDirectRateRows(rate, 95, rate.RateDescription95, rate.ResourceClass95, isGovOrComm));
				}

				#endregion
			}

			return toReturn;
		}

		/// <summary>
		/// Adds rows to rate collection for export based on available rates/years for rate code
		/// Creates using appropriate mapping sequence, either 0 (for base rate) or 1-9 for AAAAA1 - AAAAAA9
		/// </summary>
		/// <param name="rate">rate</param>
		/// <param name="mappingSequence">mapping sequence 0 or 1-9</param>
		/// <param name="description">base or description for mapped code 1-9</param>
		/// <param name="resourceClass">resource class for mapped code 0-9</param>
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
		/// Creates using appropriate mapping sequence, either 0 (for base rate) or 1-9 for AAAAA1 - AAAAAA9
		/// </summary>
		/// <param name="rate">rate</param>
		/// <param name="mappingSequence">mapping sequence 0 or 1-9</param>
		/// <param name="description">base or description for mapped code 1-9</param>
		/// <param name="resourceClass">resource class for mapped code 0-9</param>
		/// <param name="isGovOrComm">IsGovOrComm enum</param>
		/// <param name="year">int year</param>
		/// <returns>ProPricerDirectRateExportRowModelView</returns>
		private ProPricerDirectRateExportRowModelView CreateDirectRateRow(RateDetailModelView rate, int mappingSequence, string description, string resourceClass, IsGovOrComm isGovOrComm, int year)
		{
			ProPricerDirectRateExportRowModelView toReturn = new();
			// use base ratecode for resource or create [RATECODE]1,2,3,4,5,6,7,8 or 9
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
			List<int> fccomIndices = new();
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
					ProPricerBurdenRateExportRowModelView row = new()
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

		#endregion
	}
}
