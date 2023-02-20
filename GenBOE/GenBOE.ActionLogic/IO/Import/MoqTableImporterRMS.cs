// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// MOQ Table Importer for RMS overrides
    /// </summary>
    public class MoqTableImporterRMS : MoqTableImporter
    {
        protected override List<string> REQUIRED_COLUMNS => new List<string> { TABLE_NAME, DATE_OF_REPORT, HISTORICAL_PROGRAM_NAME, CONTRACT_NUMBER, WBS_ELEMENT, START_DATE, END_DATE };

		protected override List<string> ALL_COLUMNS => new List<string> { TABLE_NAME, DATE_OF_REPORT, HISTORICAL_PROGRAM_NAME, CONTRACT_NUMBER, WBS_ELEMENT, START_DATE, END_DATE, TOTAL_WBS_HOURS, ADDITIONAL_QUERY_FILTERS, TOTAL_RELEVANT_HOURS_RMS };


		/// <summary>
		/// Constructor
		/// </summary>
		public MoqTableImporterRMS()
            : base()
        {

        }

        /// <summary>
        /// Populate the company specific fields for the MOQ Table
        /// </summary>
        /// <param name="moqTable">The imported MOQ Table</param>
        /// <param name="row">row from the import file</param>
        protected override void ImportCompanySpecificMoqTableData(ImportedMoqTable moqTable, Dictionary<string, string> row, bool sapConnectionEnabled)
        {
            _ = moqTable ?? throw new ArgumentNullException(nameof(moqTable));
            _ = row ?? throw new ArgumentNullException(nameof(row));

            // Contract Number
            if (row.ContainsKey(CONTRACT_NUMBER) && !string.IsNullOrEmpty(row[CONTRACT_NUMBER]))
            {
                moqTable.ContractNumber = row[CONTRACT_NUMBER];
                if (moqTable.ContractNumber.Length > Constants.MOQ_TYPE_TEXT_FIELD_LENGTH)
                {
                    moqTable.ImportTypes.Add(MoqTableImportType.LargeContractNumber);
                }
            }
            else
            {
                moqTable.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
            }

            // WBS/WBS Element
            if (row.ContainsKey(WBS_ELEMENT) && !string.IsNullOrEmpty(row[WBS_ELEMENT]))
            {
                moqTable.WbsElement = row[WBS_ELEMENT];
                
                if ((!sapConnectionEnabled && moqTable.WbsElement.Length > Constants.MOQ_WBS_ELEMENT_RMS_SAP_DISABLED_FIELD_LENGTH) 
                    || moqTable.WbsElement.Length > Constants.MOQ_WBS_ELEMENT_FIELD_LENGTH)
                {
                    moqTable.ImportTypes.Add(MoqTableImportType.LargeWBSElement);
                }
            }
            else if (!moqTable.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
            {
                moqTable.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
            }

			// Additional Query Filters
            if (row.ContainsKey(ADDITIONAL_QUERY_FILTERS) && !string.IsNullOrEmpty(row[ADDITIONAL_QUERY_FILTERS]))
            {
                moqTable.AdditionalQueryFilters = row[ADDITIONAL_QUERY_FILTERS];
            }
			else if ((!Utilities.IsSAPEnabled || !sapConnectionEnabled) && !moqTable.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
			{
				// Additional Query Filters is required if SAP is disabled 
				moqTable.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
			}

			if (!Utilities.IsSAPEnabled || !sapConnectionEnabled)
			{
			    // Total WBS/WBS Element Hours
				if ((!row.ContainsKey(TOTAL_WBS_HOURS) || string.IsNullOrEmpty(row[TOTAL_WBS_HOURS])) && !moqTable.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
				{
					moqTable.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
				}
				else
				{
					if (decimal.TryParse(row[TOTAL_WBS_HOURS], out decimal totalWbsHours))
					{
						moqTable.TotalWbsHours = totalWbsHours;
					}
					else
					{
						moqTable.ImportTypes.Add(MoqTableImportType.InvalidTotalWbsHours);
					}
				}

				// Total Relevant Hours
				if (row.ContainsKey(TOTAL_RELEVANT_HOURS_RMS) && !string.IsNullOrEmpty(row[TOTAL_RELEVANT_HOURS_RMS]))
				{
					if (decimal.TryParse(row[TOTAL_RELEVANT_HOURS_RMS], out decimal totalRelevantHours))
					{
						moqTable.TotalRelevantHours = totalRelevantHours;
					}
					else
					{
						moqTable.ImportTypes.Add(MoqTableImportType.InvalidTotalRelevantHours);
					}
				}
				else if (!moqTable.ImportTypes.Contains(MoqTableImportType.MissingRequiredField))
				{
					moqTable.ImportTypes.Add(MoqTableImportType.MissingRequiredField);
				}
			}
        }
    }
}
