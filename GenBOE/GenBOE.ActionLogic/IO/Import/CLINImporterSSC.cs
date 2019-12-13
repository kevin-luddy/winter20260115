// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.ActionLogic.Common;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.PickList;

    /// <summary>
    /// Used for importing new genBOE CLIN elements from an Excel file
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CLINImporterSSC : CLINImporter
    {
        #region Public Functions

        /// <summary>
        /// Initializes a new instance of the <see cref="CLINImporterSSC"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public CLINImporterSSC(IFullObjectFactory factory) : base(factory)   
        {
        }

        #endregion Public Functions

        #region Protected Functions

        /// <summary>
        /// Gets the required columns.
        /// </summary>
        protected override string[] GetRequiredColumns()
        {
            return new string[] { clinIDColumn, ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER, ImportExportConstants.CLIN_TITLE_COLUMN_HEADER, ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER, ImportExportConstants.CLIN_CONTRACT_TYPE_HEADER };
        }

        /// <summary>
        /// Creates the new imported clin.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="clinID">The clin identifier.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns>A new Imported CLIN.</returns>
        protected override ImportedClin CreateNewImportedClin(Dictionary<string, string> row, int clinID, WorkspaceDTO workspace, ICollection<PickListDto> allContractTypes)
        {
            if (ReferenceEquals(row, null))
            {
                throw new ArgumentNullException(nameof(row));
            }
            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            bool validContractType = allContractTypes.Any(c => c.Text == row[ImportExportConstants.CLIN_CONTRACT_TYPE_HEADER]);

            PickListDto contractType = row.ContainsKey(ImportExportConstants.CLIN_CONTRACT_TYPE_HEADER) && validContractType
                ? allContractTypes.First(c => c.Text == row[ImportExportConstants.CLIN_CONTRACT_TYPE_HEADER])
                : allContractTypes.First(c => c.Id == Constants.CONTRACT_TYPE_NOT_SET);

            ImportedClin toReturn = new ImportedClin
            {
                Id = clinID,
                ClinNumber = row[ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER],
                ClinTitle = row[ImportExportConstants.CLIN_TITLE_COLUMN_HEADER],
                WorkspaceID = workspace.Id,
                Updateable = UpdateType.Upsert,
                ContractType = contractType.Id
            };

            this.ParseStartAndEndDates(row, workspace, toReturn);

            if ((contractType.Id != Constants.CONTRACT_TYPE_NOT_SET && !workspace.SelectedContractTypes.Contains(contractType.Id)) || !validContractType)
            {
                toReturn.ImportTypes.Add(ClinImportResult.MissingInvalidContractType);
            }
            else
            {
                toReturn.ImportTypes.Add(ClinImportResult.CreateClin);
            }

            return toReturn;
        }

        /// <summary>
        /// Creates the update clin.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="row">The row.</param>
        /// <param name="clinID">The clin identifier.</param>
        /// <param name="oldClin">The old clin.</param>
        /// <returns>An imported clin that is to be updated.</returns>
        protected override ImportedClin CreateUpdateClin(FullWorkspace workspace, Dictionary<string, string> row, int clinID, FullClin oldClin, ICollection<PickListDto> allContractTypes)
        {
            if (allContractTypes == null)
            {
                throw new ArgumentNullException(nameof(allContractTypes));
            }

            if (ReferenceEquals(oldClin, null))
            {
                throw new ArgumentNullException(nameof(oldClin));
            }
            ImportedClin importedClin = base.CreateUpdateClin(workspace, row, clinID, oldClin, allContractTypes);

            if (importedClin != null)
            {
                PickListDto contractType = row.ContainsKey(ImportExportConstants.CLIN_CONTRACT_TYPE_HEADER) ? allContractTypes.First(c => c.Text == row[ImportExportConstants.CLIN_CONTRACT_TYPE_HEADER]) : allContractTypes.First(c => c.Id == Constants.CONTRACT_TYPE_NOT_SET);
                importedClin.ContractType = contractType.Id;

                // If the Clin was changed and there were no other parsing issues, then we'll set this
                // Clin as an Update
                if (contractType.Id != Constants.CONTRACT_TYPE_NOT_SET && !workspace.SelectedContractTypes.Contains(contractType.Id))
                {
                    importedClin.ImportTypes.Add(ClinImportResult.MissingInvalidContractType);
                }
                else if(!importedClin.ImportTypes.Contains(ClinImportResult.UpdateClin) && contractType.Id != oldClin.ContractType)
                {
                    importedClin.ImportTypes.Add(ClinImportResult.UpdateClin);
                }
            }

            return importedClin;
        }

        /// <summary>
        /// Checks the headers.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="toReturn">To return.</param>
        /// <param name="currentClinID">The current clin identifier.</param>
        /// <param name="row">The row.</param>
        /// <param name="uniqueClinNumberLinqResultsFromSpreadsheet">The unique clin number linq results from spreadsheet.</param>
        /// <param name="uniqueClinNumberLinqResultsFromDatabase">The unique clin number linq results from database.</param>
        /// <param name="uniqueClinIDLinqResults">The unique clin identifier linq results.</param>
        /// <param name="allContractTypes">All of the contract types.</param>
        /// <returns>True/false if the headers are valid.</returns>
        protected override bool CheckHeaders(FullWorkspace workspace, Collection<ImportedClin> toReturn, ref int currentClinID, Dictionary<string, string> row, ICollection<Dictionary<string, string>> uniqueClinNumberLinqResultsFromSpreadsheet, ICollection<FullClin> uniqueClinNumberLinqResultsFromDatabase, ICollection<Dictionary<string, string>> uniqueClinIDLinqResults, ICollection<PickListDto> allContractTypes)
        {
            bool validHeaders = base.CheckHeaders(workspace, toReturn, ref currentClinID, row, uniqueClinNumberLinqResultsFromSpreadsheet, uniqueClinNumberLinqResultsFromDatabase, uniqueClinIDLinqResults, allContractTypes);

            if (validHeaders)
            {
                if (!row.ContainsKey(ImportExportConstants.CLIN_CONTRACT_TYPE_HEADER))
                {
                    toReturn.Add(new ImportedClin(row[ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER], row[ImportExportConstants.CLIN_TITLE_COLUMN_HEADER], null, null, ClinImportResult.MissingInvalidContractType));
                    validHeaders = false;
                }
            }

            return validHeaders;
        }

        #endregion Protected Functions
    }

}