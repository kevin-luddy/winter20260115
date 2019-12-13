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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using DocumentFormat.OpenXml.Packaging;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Used for importing new genBOE BOE elements from an Excel file
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProjectImporter
    {
        // Create static Regex object for WbsNumber.
        private static Regex regexWbsNumber = new Regex(ValidationConstants.WBS_NUMBER, RegexOptions.None, Constants.REGEX_TIMEOUT);

        #region Constants

        private const string TAB_TASK = "Task_Table";
        private const string ASSIGNMENT_TASK = "Assignment_Table";
        
        // Individual column names
        private const string CLIN_COLUMN = "CLIN";
        private const string CLIN_DECRIPTION_COLUMN = "CLIN Description";

        private const string ID_COLUMN = "ID";
        private const string TASK_ID_COLUMN = "Task_ID";
        private const string WBS_COLUMN = "WBS";
        private const string WBS_DESCRIPTION_COLUMN = "WBS Description";
        private const string TASK_COLUMN = "Task";
        private const string START_DATE_COLUMN = "Start_Date";
        private const string END_DATE_COLUMN = "Finish_Date";
        private const string SCHEDULED_WORK_COLUMN = "Scheduled_Work";
        private const string DEPARTMENT_COLUMN = "Department";
        
        // Array of the columns that must be contained in the imported file
        private readonly string[] requiredTaskColumns = new string[] {ID_COLUMN, WBS_COLUMN, WBS_DESCRIPTION_COLUMN, TASK_COLUMN};

        private readonly string[] possibleTaskColumns = new string[] { ID_COLUMN, WBS_COLUMN, WBS_DESCRIPTION_COLUMN, CLIN_COLUMN, CLIN_DECRIPTION_COLUMN, TASK_COLUMN };
                
        // Array of the columns that must be contained in the imported file
        private readonly string[] requiredAssignmentColumns = new string[] { TASK_ID_COLUMN, DEPARTMENT_COLUMN, START_DATE_COLUMN, END_DATE_COLUMN, SCHEDULED_WORK_COLUMN };

        #endregion Constants

        #region Public Functions

        public ProjectImport ImportFromExcelFiles(Stream excelFileStream, FullWorkspace workspace)
        {

            if (excelFileStream == null)
            {
                throw new ArgumentNullException(nameof(excelFileStream));
            }

            try
            {
                ProjectImport importResults = new ProjectImport();

                // Open the document as read-only.
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                {
                    ICollection<Dictionary<string, string>> AssignmentRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, ASSIGNMENT_TASK, this.requiredAssignmentColumns, this.requiredAssignmentColumns, this.requiredAssignmentColumns);
                    ICollection<Dictionary<string, string>> taskRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, TAB_TASK, this.requiredTaskColumns, this.possibleTaskColumns, this.requiredTaskColumns);
                    importResults = this.ImportAll(taskRows, AssignmentRows, workspace);
                }

                return importResults;
            }
            catch (FileFormatException)
            {
                throw new NotExcelFileException("Imported file was an incorrect format.");
            }
        }

        #endregion Public Functions

        private ProjectImport ImportAll(ICollection<Dictionary<string, string>> TaskRows, ICollection<Dictionary<string, string>> AssignmentRows, FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            // Create the collection to return
            CultureInfo provider = CultureInfo.InvariantCulture;
            ProjectImport toReturn = new ProjectImport();

            // Set the ID counter. New WBS objects must have IDs < 0 and mutiple WBS elements submitted to
            // the loader must have different IDs. So, we'll go -1, -2, -3, etc.
            int newClinID = -1;
            int newWBSID = -1;
            int newBOEID = -1;
            int newTaskID = -1;

            DateTime End_Date = DateTime.MaxValue;
            DateTime Start_Date =DateTime.MinValue;

            WorkspaceDTO thisWorkspace = workspace;

            DateTime POPEnd_Date = thisWorkspace.ContractEndDate;
            DateTime POPStart_Date = thisWorkspace.ContractStartDate;

            foreach (Dictionary<string, string> taskRow in TaskRows)
            {
                int? currentClinID = null;
                int currentWBSID = newWBSID;
                
                if (taskRow.ContainsKey(CLIN_COLUMN) && !String.IsNullOrEmpty(taskRow[CLIN_COLUMN]) && taskRow[CLIN_COLUMN] != "NA")
                {
                    ICollection<ClinDTO> workspaceClins = workspace.Clins.Where(x => x.ClinNumber == taskRow[CLIN_COLUMN]).ToList<ClinDTO>();

                    //to CLINS
                    if(workspaceClins.Any()){
                        currentClinID = workspaceClins.FirstOrDefault().Id;
                    }
                    else if((from clinsToReturn in toReturn.CLINs where clinsToReturn.ClinNumber == taskRow[CLIN_COLUMN] select clinsToReturn).Any())
                    {
                        currentClinID = (from clinsToReturn in toReturn.CLINs where clinsToReturn.ClinNumber == taskRow[CLIN_COLUMN] select clinsToReturn).FirstOrDefault().Id;
                    }
                    else
                    {
                        currentClinID = newClinID;

                        if (taskRow[CLIN_COLUMN].Length > 20)
                        {
                            toReturn.Messages.Add(new ImportErrorMessage()
                            {
                                Title = "CLIN number must be less than 20 characters",
                                Message = "CLIN: " + taskRow[CLIN_COLUMN]
                            });
                        }

                        if (taskRow[CLIN_DECRIPTION_COLUMN].Length > 100)
                        {
                            toReturn.Messages.Add(new ImportErrorMessage()
                            {
                                Title = "CLIN Description must be less than 100 characters",
                                Message = "CLIN: " + taskRow[CLIN_COLUMN]
                            });
                        }

                        toReturn.CLINs.Add(new ClinDTO()
                        {
                            Id = newClinID,
                            ClinNumber = taskRow[CLIN_COLUMN],
                            ClinTitle = taskRow[CLIN_DECRIPTION_COLUMN],
                            WorkspaceID = workspace.Id,
                            StartDate=null,
                            EndDate=null,
                            Updateable = UpdateType.Upsert
                        });

                        newClinID--; 
                    }
                }

                //TO WBS
                ICollection<WbsDTO> currentlyEvaluatedWBSDTOs = (from WbsToReturn in toReturn.WBSs where WbsToReturn.WbsNumber == taskRow[WBS_COLUMN] select WbsToReturn).ToList();
                if (currentlyEvaluatedWBSDTOs.Any())
                {
                    currentWBSID = currentlyEvaluatedWBSDTOs.FirstOrDefault().Id;
                    if (currentClinID.HasValue && !currentlyEvaluatedWBSDTOs.FirstOrDefault().ClinIDs.Contains(currentClinID.Value))
                    {
                        currentlyEvaluatedWBSDTOs.FirstOrDefault().ClinIDs.Add(currentClinID.Value);
                    }
                }
                else
                {
                    Collection<int> Clins = new Collection<int>();
                    if (currentClinID.HasValue)
                    {
                        Clins.Add(currentClinID.Value);
                    }

                    if (!regexWbsNumber.IsMatch(taskRow[WBS_COLUMN]) || taskRow[WBS_COLUMN].Length > 30 || taskRow[WBS_COLUMN].Length < 1)
                    {
                        toReturn.Messages.Add(new ImportErrorMessage()
                        {
                            Title = "WBS Number format is invalid.  WBS Number should contain 0-5 characters per level with a maximum of 10 levels seperated by periods and should be no more than 30 characters in total.",
                            Message = "WBS: " + taskRow[WBS_COLUMN]
                        });
                    }

                    if (taskRow[WBS_DESCRIPTION_COLUMN].Length > 100)
                    {
                        toReturn.Messages.Add(new ImportErrorMessage()
                        {
                            Title = "WBS Description must be less than 100 characters",
                            Message = "WBS: " + taskRow[WBS_COLUMN] + " Description: " + taskRow[WBS_DESCRIPTION_COLUMN]
                        });
                    }

                    toReturn.WBSs.Add(new WbsDTO(){
                        Id = newWBSID,
                        WbsNumber=taskRow[WBS_COLUMN],
                        ClinIDs=Clins,
                        WbsTitle = taskRow[WBS_DESCRIPTION_COLUMN],
                        WorkspaceID = workspace.Id,
                        Updateable = UpdateType.Upsert
                    });
                    newWBSID--;
                }

                // filter down assignments to only ones for this task
                foreach (Dictionary<string, string> assignmentRow in (from taskAssignments in AssignmentRows where taskAssignments[TASK_ID_COLUMN] == taskRow[ID_COLUMN] select taskAssignments))
                {
                    int currentBOEID = newBOEID;
                    //TO BOE
                    ICollection<BOEImport> currentlyEvaluatedBOEDTO = (from BoesToReturn in toReturn.BOEs where BoesToReturn.thisBOE.CLINID == currentClinID && BoesToReturn.thisBOE.WBSID == currentWBSID && BoesToReturn.Department == assignmentRow[DEPARTMENT_COLUMN] select BoesToReturn).ToList();

                    try
                    {
                        End_Date = DateTime.Parse(assignmentRow[END_DATE_COLUMN], provider);
                    }
                    catch (FormatException)
                    {
                        toReturn.Messages.Add(new ImportErrorMessage()
                        {
                            Title = "Invalid End Date, data type should be \"MM/yyyy\"",
                            Message = "IMS_ID: " + assignmentRow[TASK_ID_COLUMN] + " Data found:" + assignmentRow[END_DATE_COLUMN]
                        });
                    }

                    try
                    {
                        Start_Date = DateTime.Parse(assignmentRow[START_DATE_COLUMN], provider);
                    }
                    catch (FormatException)
                    {
                        toReturn.Messages.Add(new ImportErrorMessage()
                        {
                            Title = "Invalid data type in field Task End Date. data type should be \"MM/yyyy\"",
                            Message = "IMS_ID: " + assignmentRow[TASK_ID_COLUMN] + " Data found:" + assignmentRow[START_DATE_COLUMN]
                        });
                    }

                    if (End_Date > POPEnd_Date || End_Date < POPStart_Date || Start_Date < POPStart_Date || Start_Date > POPEnd_Date)
                    {
                        toReturn.Messages.Add(new ImportErrorMessage()
                        {
                            Title = "Assignment date ranges are not within the workspace POP",
                            Message = "IMS_ID: " + assignmentRow[TASK_ID_COLUMN] + " Range found:" + assignmentRow[START_DATE_COLUMN] + " - " + assignmentRow[END_DATE_COLUMN] + " POP Range:" + POPStart_Date + " - " + POPEnd_Date
                        });
                    }

                    if (End_Date < Start_Date)
                    {
                        toReturn.Messages.Add(new ImportErrorMessage()
                        {
                            Title = "Assignment date ranges are not valid",
                            Message = "IMS_ID: " + assignmentRow[TASK_ID_COLUMN] + " Range found:" + assignmentRow[START_DATE_COLUMN] + " - " + assignmentRow[END_DATE_COLUMN]
                        });
                    }

                    if (currentlyEvaluatedBOEDTO.Count == 1)
                    {
                        currentBOEID = currentlyEvaluatedBOEDTO.FirstOrDefault().thisBOE.Id;
                        if (currentlyEvaluatedBOEDTO.FirstOrDefault().thisBOE.EndDate < End_Date)
                        {
                            currentlyEvaluatedBOEDTO.FirstOrDefault().thisBOE.EndDate = End_Date;
                        }

                        if (currentlyEvaluatedBOEDTO.FirstOrDefault().thisBOE.StartDate > Start_Date)
                        {
                            currentlyEvaluatedBOEDTO.FirstOrDefault().thisBOE.StartDate = Start_Date;
                        }
                    }
                    else if (!currentlyEvaluatedBOEDTO.Any())
                    {
                        toReturn.BOEs.Add(new BOEImport()
                        {
                            thisBOE = new BoeDTO()
                            {
                                WorkspaceID = workspace.Id,
                                State = BOEState.Unassigned,
                                CLINID = currentClinID.HasValue ? currentClinID.Value : (int?)null,
                                Description = taskRow[WBS_DESCRIPTION_COLUMN],
                                StartDate = Start_Date,
                                EndDate = End_Date,
                                Id = currentBOEID,
                                WBSID = currentWBSID,
                                Updateable = UpdateType.Upsert
                            },
                            Department=assignmentRow[DEPARTMENT_COLUMN]
                        });
                        newBOEID--;
                    }
                    else
                    {
                        throw new GeneralAppException("ERROR in IMS Project Parser");
                    }


                    ICollection<BoeTaskElementDTO> currentlyEvaluatedTaskDTO = (from TaskToReturn in toReturn.BoeTaskElements where TaskToReturn.IMS_ID == assignmentRow[TASK_ID_COLUMN] && TaskToReturn.BoeID == currentBOEID select TaskToReturn).ToList();

                    if (currentlyEvaluatedTaskDTO.Count == 1)
                    {
                        currentlyEvaluatedTaskDTO.FirstOrDefault().MOQHoursEquation += " + " + assignmentRow[SCHEDULED_WORK_COLUMN];
                    }
                    else if (!currentlyEvaluatedTaskDTO.Any())
                    {
                        toReturn.BoeTaskElements.Add(new BoeTaskElementDTO()
                        {
                            BoeID = currentBOEID,
                            Id = newTaskID,
                            IMS_ID = taskRow[ID_COLUMN],
                            TaskTitle = taskRow[TASK_COLUMN],
                            MOQHoursEquation = assignmentRow[SCHEDULED_WORK_COLUMN],
                            Updateable = UpdateType.Upsert
                        });

                        newTaskID--;
                    }
                    else
                    {
                        throw new GeneralAppException("ERROR in IMS Project Parser");
                    }
                }
            }

            return toReturn;
        }
    }
}

