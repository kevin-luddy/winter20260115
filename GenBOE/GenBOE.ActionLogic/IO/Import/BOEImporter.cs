// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.ActionLogic.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Used for importing new genBOE BOE elements from an Excel file
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), ExcludeFromCodeCoverage]
    public class BOEImporter : IBOEImporter
    {
        IActiveDirectoryUtilities adUtils;

        #region Constants

        IPermissionsDTODataLoader _IPermissionsDTOLoader;
        IUserDTODataLoader _IUserDTODataLoader;
        IFullObjectFactory factory;

        private Logger _log = new Logger(typeof(BOEImporter));

        // Individual column names
        private const string boeIDColumn = "genBOE BOE ID";
        private const string wbsStringColumn = "WBS";
        private const string clinStringColumn = "CLIN";
        private const string subcontractorAuthorColumn = "Subcontractor Authors";
        private const string deleteStatus = "Delete";
        private const string multiClinWbsColumn = "Multi BOE";
        private const string yesValue = "yes";
        private const string noValue = "no";

        // Array of the columns that must be contained in the imported file
        private readonly string[] requiredColumns = new string[] { boeIDColumn, wbsStringColumn, clinStringColumn, ImportExportConstants.MATERIAL_COLUMN_HEADER, multiClinWbsColumn, ImportExportConstants.AUTHORS_COLUMN_HEADER, subcontractorAuthorColumn, ImportExportConstants.APPROVERS_COLUMN_HEADER, ImportExportConstants.STATUS_COLUMN_HEADER };

        // Array of the columns in the imported file that must contain values
        private readonly string[] requiredValueColumns = new string[] { };

        #endregion Constants

        #region Public Functions

        public BOEImporter(
            IPermissionsDTODataLoader inIPermissionsDTOLoader,
            IUserDTODataLoader inIUserDTODataLoader,
            IFullObjectFactory factory,
            IActiveDirectoryUtilities adUtils)
        {
            this._IPermissionsDTOLoader = inIPermissionsDTOLoader;
            this._IUserDTODataLoader = inIUserDTODataLoader;
            this.factory = factory;
            this.adUtils = adUtils;
        }

        public Collection<ImportedBoe> ImportBoeFromExcelFile(Stream excelFileStream, FullWorkspace workspace)
        {
            if (excelFileStream == null)
            {
                throw new ArgumentNullException(nameof(excelFileStream));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                try
                {
                    Collection<ImportedBoe> importResults = new Collection<ImportedBoe>();

                    // Open the document as read-only.
                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                    {
                        // Get a collection of all rows in the file, filtering out rows that only have data in
                        // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                        // in an enumerable collection

                        List<string> allColumns = new List<string>(this.requiredColumns);

                        ICollection<Dictionary<string, string>> allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, this.requiredColumns, allColumns.ToArray(), this.requiredValueColumns);

                        // Turn each row into a DTO object and return the collection
                        importResults = this.CreateImportedBoes(allRows, workspace);
                    }

                    return importResults;
                }
                catch (FileFormatException)
                {
                    throw new NotExcelFileException("Imported file was an incorrect format.");
                }
            }
        }

        #endregion Public Functions

        #region Private Functions

        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private Collection<ImportedBoe> CreateImportedBoes(ICollection<Dictionary<string, string>> allRows, FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            // Create the collection to return
            Collection<ImportedBoe> toReturn = new Collection<ImportedBoe>();

            // Set the ID counter. New WBS objects must have IDs < 0 and multiple WBS elements submitted to
            // the loader must have different IDs. So, we'll go -1, -2, -3, etc.
            int currentBoeID = -1;

            IReadOnlyCollection<FullBoe> allBoes = workspace.Boes;
            IReadOnlyCollection<WbsDTO> allWbs = workspace.WbsElements.ToList<WbsDTO>();
            IReadOnlyCollection<FullClin> allClins = workspace.Clins;
            WbsDTO MultiWBS = workspace.MultiBOEWbs;
            ClinDTO MultiClin = workspace.MultiBOEClin;

            Collection<PermissionsDTO> potentialPermissions = this._IPermissionsDTOLoader.GetBOEPotentialPermissionsForWorkspace(workspace.Id);

            Collection<UserDTO> allAuthors = new Collection<UserDTO>((from x in potentialPermissions
                                                                      where x.Role == Role.Author || x.Role == Role.SubcontractorAuthor
                                                                      select this._IUserDTODataLoader.GetUserByID(x.ETIUserId)).ToList());

            Collection<UserDTO> allSubcontractorAuthors = new Collection<UserDTO>((from x in potentialPermissions
                                                                      where x.Role == Role.SubcontractorAuthor
                                                                      select this._IUserDTODataLoader.GetUserByID(x.ETIUserId)).ToList());

            Collection<UserDTO> allApprovers = new Collection<UserDTO>((from x in potentialPermissions
                                                                        where x.Role == Role.Approver
                                                                        select this._IUserDTODataLoader.GetUserByID(x.ETIUserId)).ToList());

            #region Breakdown AD groups

            List<UserDTO> tempUsers = this.BreakDownADGroupsIntoIndividualUsers(allAuthors);
            tempUsers.AddRange(allAuthors.ToList());
            allAuthors = new Collection<UserDTO>(tempUsers);

            tempUsers = this.BreakDownADGroupsIntoIndividualUsers(allSubcontractorAuthors);
            tempUsers.AddRange(allSubcontractorAuthors.ToList());
            allSubcontractorAuthors = new Collection<UserDTO>(tempUsers);

            tempUsers = this.BreakDownADGroupsIntoIndividualUsers(allApprovers);
            tempUsers.AddRange(allApprovers.ToList());
            allApprovers = new Collection<UserDTO>(tempUsers);

            #endregion

            List<int> boeIds = new List<int>();
            foreach(Dictionary<string, string> row in allRows)
            {
                if (row.Keys.Contains(boeIDColumn))
                {
                    boeIds.Add(Convert.ToInt32(row[boeIDColumn]));
                }
            }
            ICollection<FullBoe> fullBoes = this.factory.CreateFullBoes(boeIds);

            foreach (Dictionary<string, string> row in allRows)
            {
				IEnumerable<Dictionary<string, string>> uniqueBoeIDLinqResults = from r in allRows
                                             where row.ContainsKey(boeIDColumn) && r.ContainsKey(boeIDColumn) && r[boeIDColumn] == row[boeIDColumn]
                                             select r;

                int returnCount = toReturn.Count;
                // check if either Author column is populated AND Approver column is NOT populated
                if ((row.ContainsKey(ImportExportConstants.AUTHORS_COLUMN_HEADER) || row.ContainsKey(subcontractorAuthorColumn)) && (!row.ContainsKey(ImportExportConstants.APPROVERS_COLUMN_HEADER)))
                {
                    toReturn.Add(new ImportedBoe(null, null, null, BoeImportResult.MissingWbsClinAuthorOrApproverOrMaterial));
                }
                // check if BOTH Author columns ARE empty AND Approver column is NOT empty
                else if ((!row.ContainsKey(ImportExportConstants.AUTHORS_COLUMN_HEADER) && (!row.ContainsKey(subcontractorAuthorColumn)) && (row.ContainsKey(ImportExportConstants.APPROVERS_COLUMN_HEADER))))
                {
                    toReturn.Add(new ImportedBoe(null, null, null, BoeImportResult.MissingWbsClinAuthorOrApproverOrMaterial));
                }
                else if ((!row.ContainsKey(clinStringColumn) || string.IsNullOrEmpty(row[clinStringColumn])) && (!row.ContainsKey(wbsStringColumn) || string.IsNullOrEmpty(row[wbsStringColumn])) && row[multiClinWbsColumn].IsEquivalentTo(noValue))
                {
                    toReturn.Add(new ImportedBoe(null, null, null, BoeImportResult.MissingWbsClinAuthorOrApproverOrMaterial));
                }
                else if (!row.ContainsKey(ImportExportConstants.MATERIAL_COLUMN_HEADER) || string.IsNullOrEmpty(row[ImportExportConstants.MATERIAL_COLUMN_HEADER]))
                {
                    toReturn.Add(new ImportedBoe(null, null, null, BoeImportResult.MissingWbsClinAuthorOrApproverOrMaterial));
                }
                else if (row.ContainsKey(ImportExportConstants.MATERIAL_COLUMN_HEADER)  && row[ImportExportConstants.MATERIAL_COLUMN_HEADER].IsEquivalentTo(yesValue) && (!row.ContainsKey(wbsStringColumn) || string.IsNullOrEmpty(row[wbsStringColumn])))
                {
                    toReturn.Add(new ImportedBoe(null, row.ContainsKey(wbsStringColumn) ? row[wbsStringColumn] : null, row.ContainsKey(clinStringColumn) ? row[clinStringColumn] : null, BoeImportResult.WBSDoesNotExistForMaterial));
                }
                else if (uniqueBoeIDLinqResults.Count() > 1)
                {
                    toReturn.Add(new ImportedBoe(row[boeIDColumn], row[wbsStringColumn], row[clinStringColumn], BoeImportResult.NonUniqueBoeID));
                }
                else
                {
                    // if at least one author and approver are assigned
                    if ((row.ContainsKey(ImportExportConstants.AUTHORS_COLUMN_HEADER) && !string.IsNullOrEmpty(row[ImportExportConstants.AUTHORS_COLUMN_HEADER])) &&
                        (row.ContainsKey(ImportExportConstants.APPROVERS_COLUMN_HEADER) && !string.IsNullOrEmpty(row[ImportExportConstants.APPROVERS_COLUMN_HEADER])))
                    {
                        // make sure each author is NOT an approver
                        string[] authorsList = row[ImportExportConstants.AUTHORS_COLUMN_HEADER].ToLower().Split('\n');
                        string[] approversList = row[ImportExportConstants.APPROVERS_COLUMN_HEADER].ToLower().Split('\n');

                        ICollection<string> bothAuthorAndApprover = authorsList.Intersect(approversList).ToList();

                        if (bothAuthorAndApprover.Any())
                        {
                            toReturn.Add(new ImportedBoe(null, row.ContainsKey(wbsStringColumn) ? row[wbsStringColumn] : null, row.ContainsKey(clinStringColumn) ? row[clinStringColumn] : null, BoeImportResult.AuthorAndApproverAreTheSame));
                        }
                    }
                }

                // Have we processed this BOE yet?
                if (returnCount == toReturn.Count)
                {
                    // Test for errors that won't end error testing.
                    ImportedBoe boeResult = new ImportedBoe();

                    bool errorsFound = false;

                    WbsDTO wbsDTO = null;
                    int? wbsID = null;
                    if (row.ContainsKey(wbsStringColumn))
                    {

                        wbsDTO = (from x in allWbs
                                  where x.WbsString.IsEquivalentTo(row[wbsStringColumn])
                                  select x).FirstOrDefault();

                        if (wbsDTO == null)
                        {
                            // undefined WBS
                            boeResult.ImportTypes.Add(BoeImportResult.WbsDoesNotExist);
                            errorsFound = true;
                        }
                        else
                        {
                            wbsID = wbsDTO.Id;
                        }
                    }

                    ClinDTO clinDTO = null;
                    int? clinID = null;
                    if (row.ContainsKey(clinStringColumn))
                    {
                        clinDTO = (from x in allClins
                                   where x.ClinString.IsEquivalentTo(row[clinStringColumn])
                                   select x).FirstOrDefault();

                        if (clinDTO == null)
                        {
                            // undefined CLIN
                            boeResult.ImportTypes.Add(BoeImportResult.ClinDoesNotExist);
                            errorsFound = true;
                        }
                        else
                        {
                            clinID = clinDTO.Id;
                        }
                    }

                    bool isMaterial = false;
                    if (row.ContainsKey(ImportExportConstants.MATERIAL_COLUMN_HEADER))
                    {
                        string isMaterialString = row[ImportExportConstants.MATERIAL_COLUMN_HEADER];

                        // if material isn't yes, no, or entered, throw error message
                        if (isMaterialString != "Yes" && isMaterialString != "No") // || string.IsNullOrEmpty(isMaterialString))
                        {
                            boeResult.ImportTypes.Add(BoeImportResult.InvalidMaterial);
                            errorsFound = true;
                        }
                        else
                        {
                            if (isMaterialString == "Yes")
                            {
                                isMaterial = true;
                            }
                            else if (isMaterialString == "No")
                            {
                                isMaterial = false;
                            }

                        }
                    }

                    bool isMultiClinWbs = false;
                    //users don't need to set yes or no
                        string isMultiClinWbsString = row.ContainsKey(multiClinWbsColumn)? row[multiClinWbsColumn]: string.Empty;

                        // if MultiBOE isn't yes, no, or entered, throw error message
                        if (isMultiClinWbsString != "Yes" && isMultiClinWbsString != "No" && !string.IsNullOrEmpty(isMultiClinWbsString))
                        {
                            boeResult.ImportTypes.Add(BoeImportResult.InvalidMultiBOEClinWbs);
                            errorsFound = true;
                        }
                        else
                        {
                            //if yes then its multi
                            if (isMultiClinWbsString == "Yes")
                            {
                                //we'll need to deal with the clin and wbs later.
                                isMultiClinWbs = true;
                            }
                            else if (isMultiClinWbsString == "No" || string.IsNullOrEmpty(isMultiClinWbsString))
                            {
                                //if the user didnt select or did select no check the clins
                                if (wbsDTO == MultiWBS && clinDTO == MultiClin)
                                {
                                    //if the user made the changes to both clin/wbs to be the multi clin and wbs then 
                                    isMultiClinWbs = true;
                                }
                                else
                                {
                                    //not multi
                                    isMultiClinWbs = false;
                                    if(wbsDTO == MultiWBS)
                                    {wbsDTO = null;}
                                    if(clinDTO == MultiClin)
                                    {clinDTO = null;}
                                }
                            }
                        }
                  
                    // need to check if this BOE already exists and if Material is able to be changed
                    if (row.Keys.Contains(boeIDColumn))
                    {
                        FullBoe boeObject = fullBoes.FirstOrDefault(x => x.Id == Convert.ToInt32(row[boeIDColumn]));

                        if (boeObject == null)
                        {
                            // The BOE being imported was not found
                            boeResult.ImportTypes.Add(BoeImportResult.BoeDoesNotExist);
                        }
                        else
                        {

                            ImportedBoe boeToCheck = new ImportedBoe(boeObject);

                            // if the BOE's material was changed, we need to make sure that it is a valid change
                            if (boeToCheck.isMaterial != isMaterial)
                            {
                                // if material changed from false to true, need to check if labor/cost task elements exist for it or change cannot occur
                                if (isMaterial)
                                {
                                    boeResult.ImportTypes.Add(BoeImportResult.MaterialBoeCannotBeChanged);
                                    errorsFound = true;
                                }
                                else  // if material changed from true to false, then check if material elements already exist for it or change cannot occur
                                {
                                    if (boeObject.ContainsMaterialElements)
                                    {
                                        boeResult.ImportTypes.Add(BoeImportResult.MaterialBoeCannotBeChanged);
                                        errorsFound = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (isMaterial)
                    {
                        // Cannot create new material BOEs
                        boeResult.ImportTypes.Add(BoeImportResult.BoeCannotBeChangedToMaterial);
                        errorsFound = true;
                    }

                    // verify that Authors in the spreadsheet exist in the database
                    Collection<int> authorIDs = new Collection<int>();
                    bool missingAuthor = false;
                    if (row.ContainsKey(ImportExportConstants.AUTHORS_COLUMN_HEADER))
                    {
                        foreach (string authorDisplayName in row[ImportExportConstants.AUTHORS_COLUMN_HEADER].Split('\n'))
                        {
                            UserDTO author = (from x in allAuthors
                                              where x.DisplayName.IsEquivalentTo(authorDisplayName)
                                              select x).FirstOrDefault();
                            if (author == null)
                            {
                                if (missingAuthor == false)
                                {
                                    boeResult.ImportTypes.Add(BoeImportResult.AuthorDoesNotExist);
                                    errorsFound = true;
                                }
                                missingAuthor = true;
                            }
                            else
                            {
                                authorIDs.Add(author.UserID);
                            }
                        }
                    }

                    // verify that Subcontractor Authors in the spreadsheet exist in the database
                    Collection<int> subcontractorAuthorIDs = new Collection<int>();
                    bool missingSubcontractorAuthor = false;
                    bool subAuthorErrors = false;
                    if (row.ContainsKey(subcontractorAuthorColumn))
                    {
                        foreach (string subcontractorAuthorDisplayName in row[subcontractorAuthorColumn].Split('\n'))
                        {
                            UserDTO subcontractorAuthor = (from x in allSubcontractorAuthors
                                                           where x.DisplayName.IsEquivalentTo(subcontractorAuthorDisplayName)
                                                           select x).FirstOrDefault();
                            if (subcontractorAuthor == null)
                            {
                                if (missingSubcontractorAuthor == false)
                                {
                                    boeResult.ImportTypes.Add(BoeImportResult.SubcontractorAuthorDoesNotExist);
                                    errorsFound = true;
                                }
                                missingSubcontractorAuthor = true;
                            }

                            //Subcontractor authors are not allowed to be assigned to the Author role on Material BOE's
                            if (isMaterial)
                            {
                                boeResult.ImportTypes.Add(BoeImportResult.CannotAssignSubAuthorToMaterialBoe);
                                errorsFound = true;
                                subAuthorErrors = true;
                            }
                                                       
                            if (!subAuthorErrors)  //Subcontractor Author passed all the checks and is good to be assigned to this BOE
                            {
                                subcontractorAuthorIDs.Add(subcontractorAuthor.UserID);
                            }
                        }
                    }


                    Collection<int> approverIDs = new Collection<int>();
                    bool missingApprover = false;
                    if (row.ContainsKey(ImportExportConstants.APPROVERS_COLUMN_HEADER))
                    {
                        foreach (string approverDisplayName in row[ImportExportConstants.APPROVERS_COLUMN_HEADER].Split('\n'))
                        {
                            UserDTO approver = (from x in allApprovers
                                                where x.DisplayName.IsEquivalentTo(approverDisplayName)
                                                select x).FirstOrDefault();
                            if (approver == null)
                            {
                                if (missingApprover == false)
                                {
                                    boeResult.ImportTypes.Add(BoeImportResult.ApproverDoesNotExist);
                                    errorsFound = true;
                                }
                                missingApprover = true;
                            }
                            else
                            {
                                approverIDs.Add(approver.UserID);
                            }
                        }
                    }

                    if (!errorsFound)
                    {
                        //cant have a multi and material
                        if (isMultiClinWbs && isMaterial)
                        {
                            boeResult.ImportTypes.Add(BoeImportResult.InvalidMultiBOEMaterialCombo);
                            errorsFound = true;
                        }
                    }

                    //if this is a multiboe make sure the wbs and clin are multi.
                    if(isMultiClinWbs)
                    {
                        wbsDTO = MultiWBS;
                        clinDTO = MultiClin;
                        boeResult.WBSID = MultiWBS.Id;
                        boeResult.WbsString = MultiWBS.WbsString;
                        boeResult.ClinString = MultiClin.ClinString;
                        boeResult.CLINID = MultiClin.Id;
                    }
                    else if (clinDTO == null && wbsDTO == null)
                    {
                        boeResult.ImportTypes.Add(BoeImportResult.MissingWbsClinAuthorOrApproverOrMaterial);

                        errorsFound = true;
                    }

                    if (errorsFound)
                    {
                        boeResult.WbsString =  row.ContainsKey(wbsStringColumn) ? row[wbsStringColumn] : string.Empty;
                        boeResult.ClinString = row.ContainsKey(clinStringColumn) ? row[clinStringColumn] : string.Empty;
                        boeResult.AuthorIDs = authorIDs;
                        boeResult.SubcontractorAuthorIDs = subcontractorAuthorIDs;
                        boeResult.ApproverIDs = approverIDs;
                        toReturn.Add(boeResult);
                    }
                    else
                    {
                        if (!row.Keys.Contains(boeIDColumn) || string.IsNullOrEmpty(row[boeIDColumn].Trim()))
                        {
                            // Add the new Model View to the collection to be returned
                            this.AddNewImportedBOE(row, currentBoeID--, workspace, wbsDTO, clinDTO, authorIDs, subcontractorAuthorIDs, approverIDs, toReturn, isMaterial, isMultiClinWbs);
                        }
                        else
                        {
                            try
                            {
                                FullBoe boeObject = fullBoes.FirstOrDefault(x => x.Id == Convert.ToInt32(row[boeIDColumn])) ?? throw new GenValidationException("Invalid BOE Id found");

                                boeResult = new ImportedBoe(boeObject);
                                boeResult.WbsString = row.ContainsKey(wbsStringColumn) ? row[wbsStringColumn] : string.Empty;
                                boeResult.ClinString = row.ContainsKey(clinStringColumn) ? row[clinStringColumn] : string.Empty;
                                boeResult.ApproverIDs = approverIDs;
                                boeResult.AuthorIDs = authorIDs;
                                boeResult.SubcontractorAuthorIDs = subcontractorAuthorIDs;
                                if(isMultiClinWbs)
                                {
                                     boeResult.WbsString = MultiWBS.WbsString;
                                     boeResult.ClinString = MultiClin.ClinString;
                                }
                                try
                                {
                                    DataRelationshipVerifier.VerifyDataRelation(boeResult, workspace.Id);

                                    bool boeChanged = false;
                                    if (row.ContainsKey(ImportExportConstants.STATUS_COLUMN_HEADER) && row[ImportExportConstants.STATUS_COLUMN_HEADER] == deleteStatus)
                                    {
                                        boeResult.Updateable = UpdateType.Deleted;
                                        boeResult.ImportTypes.Add(BoeImportResult.DeleteBoe);
                                        boeChanged = true;
                                    }
                                    else
                                    {
                                        boeResult.Updateable = UpdateType.Upsert;
                                        boeResult.ImportTypes.Add(BoeImportResult.UpdateBoe);

                                       
                                        // check for changes
                                        if (wbsID != boeResult.WBSID || clinID != boeResult.CLINID ||
                                            boeResult.isMaterial != isMaterial ||
                                            boeResult.IsMultiClinWbs != isMultiClinWbs)
                                        {
                                            boeChanged = true;
                                        }
                                        else
                                        {
											// since this can't set the changed flag to false, 
											// we really only need to evaluate it if the changed flag is still false

											// check for changed approvers
											IEnumerable<int> existingApproversIDs = from x in this._IPermissionsDTOLoader.GetBOEPermissions(new List<int>(){ boeResult.Id }).Where(x => x.Role == Role.Approver).Select(x => x).ToArray()
                                                                       select x.ETIUserId;
											Collection<int> newApproversIDs = approverIDs;

											IEnumerable<int> addedApprovers = from x in approverIDs
                                                                 where !existingApproversIDs.Contains(x)
                                                                 select x;
											IEnumerable<int> deletedApprovers = from x in existingApproversIDs
                                                                   where !newApproversIDs.Contains(x)
                                                                   select x;

                                            if (addedApprovers.Count() + deletedApprovers.Count() > 0)
                                            {
                                                boeChanged = true;
                                            }

											// check for changed Authors
											IEnumerable<int> existingAuthorsIDs = from x in this._IPermissionsDTOLoader.GetBOEPermissions(new List<int> () { boeResult.Id }).Where(x => x.Role == Role.Author).Select(x => x).ToArray()
                                                                      select x.ETIUserId;
											Collection<int> newAuthorsIDs = authorIDs;

											IEnumerable<int> addedAuthors = from x in authorIDs
                                                               where !existingAuthorsIDs.Contains(x)
                                                               select x;
											IEnumerable<int> deletedAuthors = from x in existingAuthorsIDs
                                                                 where !newAuthorsIDs.Contains(x)
                                                                 select x;
                                            if (addedAuthors.Count() + deletedAuthors.Count() > 0)
                                            {
                                                boeChanged = true;
                                            }

											// check for changed Subcontractor Authors
											IEnumerable<int> existingSubcontractorAuthorsIDs = from x in this._IPermissionsDTOLoader.GetBOEPermissions(new List<int>() { boeResult.Id } ).Where(x => x.Role == Role.SubcontractorAuthor).Select(x => x).ToArray()
                                                                                  select x.ETIUserId;
											Collection<int> newSubcontractorAuthorsIDs = subcontractorAuthorIDs;

											IEnumerable<int> addedSubcontractorAuthors = from x in subcontractorAuthorIDs
                                                                            where !existingSubcontractorAuthorsIDs.Contains(x)
                                                                            select x;
											IEnumerable<int> deletedSubcontractorAuthors = from x in existingSubcontractorAuthorsIDs
                                                                              where !newSubcontractorAuthorsIDs.Contains(x)
                                                                              select x;
                                            if (addedSubcontractorAuthors.Count() + deletedSubcontractorAuthors.Count() > 0)
                                            {
                                                boeChanged = true;
                                            }

                                            // check for allowed changed status
                                            BOEState boeState = (BOEState)Enum.Parse(typeof(BOEState), row[ImportExportConstants.STATUS_COLUMN_HEADER].Replace(" ", ""));
                                            if (row.ContainsKey(ImportExportConstants.STATUS_COLUMN_HEADER) && (boeResult.State == BOEState.Approved || boeResult.State == BOEState.AwaitingApproval) &&
                                                (boeState == BOEState.Draft || boeState == BOEState.DraftLocked))
                                            {
                                                boeChanged = true;
                                                boeResult.State = boeState;
                                            }
                                        }

                                        boeResult.WBSID = wbsID;
                                        boeResult.CLINID = clinID;
                                        boeResult.AuthorIDs = authorIDs;
                                        boeResult.SubcontractorAuthorIDs = subcontractorAuthorIDs;
                                        boeResult.ApproverIDs = approverIDs;
                                        boeResult.isMaterial = isMaterial;
                                        boeResult.IsMultiClinWbs = isMultiClinWbs;
                                    }

                                    if (boeChanged)
                                    {
                                        toReturn.Add(boeResult);
                                    }
                                }
                                // Catch exceptions where the genBOE WBS ID supplied does not exist in the current workspace
                                catch (InvalidDataRelationException)
                                {
                                    this.AddNewImportedBOE(row, currentBoeID--, workspace, wbsDTO, clinDTO, authorIDs, subcontractorAuthorIDs, approverIDs, toReturn, isMaterial, isMultiClinWbs);
                                    // create new row
                                }
                                catch (ArgumentNullException)
                                {
                                    this.AddNewImportedBOE(row, currentBoeID--, workspace, wbsDTO, clinDTO, authorIDs, subcontractorAuthorIDs, approverIDs, toReturn, isMaterial, isMultiClinWbs);
                                    // create new row
                                }
                            }
                            // Catch exceptions where the genBOE WBS ID column is not an integer.
                            catch (FormatException)
                            {
                                this.AddNewImportedBOE(row, currentBoeID--, workspace, wbsDTO, clinDTO, authorIDs, subcontractorAuthorIDs, approverIDs, toReturn, isMaterial, isMultiClinWbs);
                            }
                            catch (OverflowException)
                            {
                                this.AddNewImportedBOE(row, currentBoeID--, workspace, wbsDTO, clinDTO, authorIDs, subcontractorAuthorIDs, approverIDs, toReturn, isMaterial, isMultiClinWbs);
                            }
                        }
                    }
                }
            }

            Collection<ImportedBoe> validBoes =
                new Collection<ImportedBoe>((from x in toReturn
                                             where x.ImportTypes.Contains(BoeImportResult.CreateBoe) || x.ImportTypes.Contains(BoeImportResult.UpdateBoe)
                                             select x).ToArray());

            this.CheckForInvalidNesting(validBoes, allBoes.ToCollection<BoeDTO>());

            return toReturn;
        }

        /// <summary>
        /// Breaks down AD groups into individual users
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        private List<UserDTO> BreakDownADGroupsIntoIndividualUsers(Collection<UserDTO> users)
        {
            List<UserDTO> tempUsers = new List<UserDTO>();
            foreach (UserDTO user in users)
            {
                if (user.NTID.Contains('.')) // AD group name
                {
					ICollection<UserData> members = this.adUtils.GetAdGroupUsers(user.DisplayName);

                    int userId = 0;
                    List<int> userIds = new List<int>();

                    foreach (UserData member in members.OrderBy(m => m.DisplayName))
                    {
                        bool userExists = this._IUserDTODataLoader.UserExists(member.Ntid, out userId);

                        if (userExists)
                        {
                            if (users.All(x => x.UserID != userId))
                            {
                                userIds.Add(userId);
                            }
                        }
                    }

                    tempUsers.AddRange(this._IUserDTODataLoader.GetByIds(userIds));
                }
            }
            return tempUsers;
        }

        private void CheckForInvalidNesting(Collection<ImportedBoe> importedBoeCollection, Collection<BoeDTO> existingBoes)
        {
            // First check against existing BOEs
            ICollection<FullWbs> wbses = this.factory.CreateFullWbses(existingBoes.Where(x => x.WBSID.HasValue).Select(x => x.WBSID.Value).Distinct().ToCollection());
            ICollection<FullWbs> wbsLinkedToImportedBoes = this.factory.CreateFullWbses(importedBoeCollection.Where(x => x.WBSID.HasValue).Select(x => x.WBSID.Value).ToList());

            foreach (BoeDTO boe in existingBoes)
            {
                if (boe.WBSID.HasValue)
                {
                    string wbsNumber = wbses.First(x => x.Id == boe.WBSID.Value).WbsNumber;                    

                    foreach (ImportedBoe importedBoe in importedBoeCollection)
                    {
                        if (importedBoe.WBSID.HasValue)
                        {
                            string importedWbsNumber = wbsLinkedToImportedBoes.First(x => x.Id == importedBoe.WBSID.Value).WbsNumber;

                            // Only check new BOEs, and updated boes with changes to the WBS
                            if (!importedBoe.ImportTypes.Contains(BoeImportResult.UpdateBoe) ||
                                importedBoe.ImportTypes.Contains(BoeImportResult.UpdateBoe) &&
                                (importedBoe.WBSID != boe.WBSID || importedBoe.Id != boe.Id))
                            {
                                if (wbsNumber.StartsWith(importedWbsNumber + ".", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    // we're creating a BOE on a parent wbs, this is invalid.
                                    importedBoe.ImportTypes = new Collection<BoeImportResult>();
                                    importedBoe.ImportTypes.Add(BoeImportResult.BoeAlreadyExists);
                                }

                                if (importedWbsNumber.StartsWith(wbsNumber + ".", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    // we're creating a BOE on a child wbs, this is also invalid.
                                    importedBoe.ImportTypes = new Collection<BoeImportResult>();
                                    importedBoe.ImportTypes.Add(BoeImportResult.BoeAlreadyExists);
                                }
                            }
                        }
                    }
                }
            }

            Collection<ImportedBoe> newImportedBoeCollection = new Collection<ImportedBoe>(
                (from x in importedBoeCollection
                 where x.ImportTypes.Contains(BoeImportResult.CreateBoe)
                 select x).ToArray()
                 );

            ICollection<FullWbs> wbsLinkedToImportedBoesForNew = this.factory.CreateFullWbses(newImportedBoeCollection.Where(x => x.WBSID.HasValue).Select(x => x.WBSID.Value).ToList());

            // then check new BOEs making both new BOEs invalid.
            foreach (ImportedBoe boe in newImportedBoeCollection)
            {
                if (boe.WBSID.HasValue)
                {
                    string wbsNumber = wbsLinkedToImportedBoesForNew.First(x => x.Id == boe.WBSID.Value).WbsNumber;

                    foreach (ImportedBoe importedBoe in newImportedBoeCollection)
                    {
                        if (importedBoe.WBSID.HasValue)
                        {
                            string importedWbsNumber = wbsLinkedToImportedBoesForNew.First(x => x.Id == importedBoe.WBSID.Value).WbsNumber;

                            // don't check self
                            if (importedBoe.Id != boe.Id)
                            {
                                if (wbsNumber.StartsWith(importedWbsNumber + ".", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    // we're creating a BOE on a parent wbs, this is invalid.
                                    importedBoe.ImportTypes = new Collection<BoeImportResult>();
                                    importedBoe.ImportTypes.Add(BoeImportResult.BoeAlreadyExists);

                                    boe.ImportTypes = new Collection<BoeImportResult>();
                                    boe.ImportTypes.Add(BoeImportResult.BoeAlreadyExists);
                                }

                                if (importedWbsNumber.StartsWith(wbsNumber + ".", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    // we're creating a BOE on a child wbs, this is also invalid.
                                    importedBoe.ImportTypes = new Collection<BoeImportResult>();
                                    importedBoe.ImportTypes.Add(BoeImportResult.BoeAlreadyExists);

                                    boe.ImportTypes = new Collection<BoeImportResult>();
                                    boe.ImportTypes.Add(BoeImportResult.BoeAlreadyExists);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void AddNewImportedBOE(
            Dictionary<string, string> row,
            int currentBOEID,
            FullWorkspace workspace,
            WbsDTO wbs,
            ClinDTO clin,
            Collection<int> authorIDs,
            Collection<int> subcontractorAuthorIDs,
            Collection<int> approverIDs,
            Collection<ImportedBoe> importedBoeCollection,
            bool isMaterial,
            bool isMultiClinWbs)
        {
            if (!row.ContainsKey(ImportExportConstants.STATUS_COLUMN_HEADER) || row[ImportExportConstants.STATUS_COLUMN_HEADER] != deleteStatus)
            {
                ImportedBoe boe = new ImportedBoe();
                boe.Id = currentBOEID;
                boe.WorkspaceID = workspace.Id;
                boe.WBSID = wbs == null ? (int?)null : wbs.Id;
                boe.WbsString = wbs == null ? null : wbs.WbsString;
                boe.CLINID = clin == null ? (int?)null : clin.Id;
                boe.ClinString = clin == null ? null : clin.ClinString;
                boe.AuthorIDs = authorIDs;
                boe.SubcontractorAuthorIDs = subcontractorAuthorIDs;
                boe.ApproverIDs = approverIDs;

                if (!approverIDs.Any() || (!authorIDs.Any() && !subcontractorAuthorIDs.Any()))
                {
                    boe.State = BOEState.Unassigned;
                }
                else
                {
                    boe.State = BOEState.Draft;
                }
                
                //boe.State = BOEState.Draft;
                boe.Updateable = UpdateType.Upsert;
                boe.isMaterial = isMaterial;
                boe.IsMultiClinWbs = isMultiClinWbs;
                boe.ImportTypes.Add(BoeImportResult.CreateBoe);

                importedBoeCollection.Add(boe);
            }
        }

        #endregion Private Functions
    }

    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags", Justification = "Allows non-consecutive numbering of enum values")]
    public enum BoeImportResult
    {
        None = 0,
        CreateBoe = 1,
        UpdateBoe = 2,
        DeleteBoe = 3,
        MissingWbsClinAuthorOrApproverOrMaterial = 4,
        WbsDoesNotExist = 5,
        ClinDoesNotExist = 6,
        AuthorDoesNotExist = 7,
        SubcontractorAuthorDoesNotExist = 8,
        ApproverDoesNotExist = 9,
        NonUniqueBoeID = 10,
        InvalidStartDate = 11,
        InvalidEndDate = 12,
        BoeAlreadyExists = 13,
        CircularReferences = 14,
        InvalidMaterial = 15,
        MaterialBoeCannotBeChanged = 16,
        BoeCannotBeChangedToMaterial = 17,
        AuthorAndApproverAreTheSame = 18,
        InvalidStartDateFormat = 19,
        InvalidEndDateFormat = 20,
        WBSDoesNotExistForMaterial = 24,
        CannotAssignSubAuthorToExistingBoe = 25,
        CannotAssignSubAuthorToMaterialBoe = 26,
        BoeNotInDraftState = 28,
        BoeIDMissingInvalid = 29,
        ImportingUserNotAuthor = 30,
        BoeNotInWorkspace = 31,
        BoeTitle100CharLimit = 32,
        BoeSourceOfData2000CharLimit = 33,
        BoeDoesNotExist = 34,
        BoeTitleRequired = 35,
        DuplicateBoeID = 36,
        WorkspaceNotInWorkingState = 37,
        InvalidMultiBOEClinWbs = 38,
        InvalidMultiBOEMaterialCombo = 40,
        MissingRequiredBOECustomField = 43
    }

    [ExcludeFromCodeCoverage]
    public class ImportedBoe : BoeDTO
    {
        public ImportedBoe()
        {
            this.ImportTypes = new Collection<BoeImportResult>();
            this.ApproverIDs = new Collection<int>();
        }

        public ImportedBoe(string boeID, string inWbsString, string inClinString, BoeImportResult inImportResult)
            : this()
        {
            this.Id = Convert.ToInt32(boeID);
            this.WbsString = inWbsString;
            this.ClinString = inClinString;
            this.ImportTypes.Add(inImportResult);
        }

        public ImportedBoe(BoeDTO boe)
            : this()
        {
            if (boe != null)
            {
                this.CLINID = boe.CLINID;
                this.CustomFieldValueContainers = boe.CustomFieldValueContainers;
                this.DataSource = boe.DataSource;
                this.Description = boe.Description;
                this.HistoricMetricDisclosureChecked = boe.HistoricMetricDisclosureChecked;
                this.Id = boe.Id;
                this.State = boe.State;
                this.SubmitForApprovalDate = boe.SubmitForApprovalDate;
                this.UpdateDate = boe.UpdateDate;
                this.WBSID = boe.WBSID;
                this.WCBID = boe.WCBID;
                this.WorkspaceID = boe.WorkspaceID;
                this.isMaterial = boe.isMaterial;
                this.IsMultiClinWbs = boe.IsMultiClinWbs;
            }
        }

        public string WbsString { get; set; }
        public string ClinString { get; set; }
		// This needs to stay an "int" and not a "ImportResult" in order to translate correctly during model binding.
		public int ImportType { get; set; }
		public Collection<int> ApproverIDs { get; set; }
        public Collection<BoeImportResult> ImportTypes { get; set; }
    }
}
