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
    using System.Text;
    using System.Text.RegularExpressions;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    [ExcludeFromCodeCoverage]
    public class ArtemisImporter
    {
        // Create static Regex object for WbsNumber.
        private static Regex regexWbsNumber = new Regex(ValidationConstants.WBS_NUMBER, RegexOptions.None, Constants.REGEX_TIMEOUT);

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "br")]
        public ArtemisImport ImportFromCSVFiles(Stream TaskFileStream, Stream WBSFileStream, WorkspaceDTO workspaceDTO)
        {
            if (workspaceDTO == null)
            {
                throw new ArgumentNullException(nameof(workspaceDTO));
            }

            ArtemisImport toReturn = new ArtemisImport();
            Collection<ImportErrorMessage> ErrorMessages = new Collection<ImportErrorMessage>();

            int WBSIDCounter = -1;

            Collection<TaskDataFromFile> TaskElementsFromFile = new Collection<TaskDataFromFile>();
            Collection<WbsDTO> WBSElementsFromFile = new Collection<WbsDTO>();

            using (StreamReader sr = new StreamReader(TaskFileStream)) 
             {
                 String line;

                 while ((line = sr.ReadLine()) != null)
                 {
                     string[] columns = this.fixColumns(line.Split(',')).ToArray();

                     TaskDataFromFile thisRow = this.TaskElementCreation(columns, ErrorMessages);
                     if (thisRow != null)
                     {
                         TaskElementsFromFile.Add(thisRow);
                     }                     
                 }
             }

            ICollection<String> WBS_CODES = (from ids in TaskElementsFromFile select ids.WBS_CODE).ToList();

             using (StreamReader sr = new StreamReader(WBSFileStream))
             {
                 String line;
                 while ((line = sr.ReadLine()) != null)
                 {
                     string[] columns = this.fixColumns(line.Split(',')).ToArray();
                     WbsDTO thisRow = this.WBSCreation(columns, workspaceDTO.Id, WBSIDCounter, ErrorMessages);
                     WBSIDCounter--;
                     if (thisRow != null && WBS_CODES.Contains(thisRow.WbsNumber))
                     {
                         WBSElementsFromFile.Add(thisRow);
                     }

                 }
             }

            toReturn.WBSs = WBSElementsFromFile;

            Collection<BoeDTO> BoesToSave = new Collection<BoeDTO>();
            Collection<BoeTaskElementDTO> BoeTaskElementsToSave = new Collection<BoeTaskElementDTO>();
            int newBOEID = 0;
            int newTaskID = 0;

            foreach(WbsDTO wbs in WBSElementsFromFile)
            {
                BoeDTO newBOE = new BoeDTO();
                newBOE.Id = --newBOEID; // need to decrement the ID 
                newBOE.WBSID = wbs.Id;
                newBOE.Description = wbs.WbsTitle;
                newBOE.State = BOEState.Unassigned;

                DateTime StartDate = DateTime.MaxValue;
                DateTime EndDate = DateTime.MinValue;

                foreach(TaskDataFromFile task in (from task in TaskElementsFromFile where task.WBS_CODE == wbs.WbsNumber select task))
                {
                    StartDate = task.Start_Date < StartDate ? task.Start_Date : StartDate;
                    EndDate = task.End_Date > EndDate ? task.End_Date : EndDate;
                    BoeTaskElementsToSave.Add(
                        new BoeTaskElementDTO()
                        {
                            Id = --newTaskID,
                            IMS_ID = task.IMS_CODE,
                            TaskTitle = task.TSK_DESC,
                            Updateable = UpdateType.Upsert,
                            BoeID = newBOE.Id
                        });
                }

                if (StartDate < workspaceDTO.ContractStartDate || EndDate > workspaceDTO.ContractEndDate)
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                        Title = "WBS Tasks start and end dates do not fall within the contract start and end date for the workspace.",
                        Message = "Workspace POP: " + workspaceDTO.ContractStartDate.ToString("MM/yyyy") + "-" + workspaceDTO.ContractEndDate.ToString("MM/yyyy") +
                        "<br/> Generated BOE POP: (" + wbs.WbsTitle + ") " + StartDate.ToString("MM/yyyy") + "-" + EndDate.ToString("MM/yyyy")
                    });
                }

                newBOE.StartDate = StartDate;
                newBOE.EndDate = EndDate;
                newBOE.WorkspaceID = workspaceDTO.Id;
                newBOE.Updateable = UpdateType.Upsert;
  
                BoesToSave.Add(newBOE);
            }

            toReturn.Messages = ErrorMessages;
            toReturn.BOEs = BoesToSave;
            toReturn.BoeTaskElements = BoeTaskElementsToSave;
            return toReturn;
        }

        private TaskDataFromFile TaskElementCreation(string[] columns, Collection<ImportErrorMessage> ErrorMessages)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            TaskDataFromFile toReturn = null;

            if (columns[0].Equals("ACTIVITY"))
            {

                Boolean hasStartDate = false;
                Boolean hasEndDate = false;

                if (columns.Length < 113)
                {
                    throw new IncorrectColumnCountException();
                }

                toReturn = new TaskDataFromFile();
                toReturn.IMS_CODE = this.PrepData(columns[3]);
                toReturn.TSK_DESC = this.PrepData(columns[56]);
                toReturn.WBS_CODE = this.PrepData(columns[79]);

                try
                {
                    toReturn.Start_Date = DateTime.ParseExact(this.PrepDate(columns[111]), "MMyyyy", provider);
                    hasStartDate = true;
                }
                catch (FormatException)
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                            Title="Invalid data type in field Task Start Date. data type should be \"ddMMyyyyhhss\"",
                            Message = "WBS: " + toReturn.WBS_CODE + " Data found:" + columns[111]
                    });
                }

                try
                {
                    toReturn.End_Date = DateTime.ParseExact(this.PrepDate(columns[112]), "MMyyyy", provider);
                    hasEndDate = true;
                }
                catch (FormatException)
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                        Title = "Invalid data type in field Task End Date. data type should be \"ddMMyyyyhhss\"",
                        Message = "WBS: " + toReturn.WBS_CODE + " Data found:" + columns[112]
                    });
                }

                if (hasEndDate && hasStartDate)
                {
                    if (toReturn.End_Date < toReturn.Start_Date)
                    {
                        ErrorMessages.Add(new ImportErrorMessage()
                        {
                            Title = "Start Date occurs after the End Date.",
                            Message = "WBS: " + toReturn.WBS_CODE + " Data found:" + toReturn.Start_Date + "-" + toReturn.End_Date
                        });
                    }

                }

                if (toReturn.IMS_CODE.Length > 20)
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                        Title = "IMS Code exceeds maximum character limit",
                        Message = "WBS: " + toReturn.WBS_CODE + " Data found: '" + toReturn.IMS_CODE+"'"
                    });
                }

                if (toReturn.WBS_CODE.Length > 30)
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                        Title = "A WBS Number must not exceed the maximum character limit of 30 characters.",
                        Message = "Task File, WBS: \"" + toReturn.WBS_CODE + "\""
                    });
                }else if(toReturn.WBS_CODE.Length < 1)
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                        Title = "A WBS Number is missing",
                        Message = "Task File, WBS: \"" + toReturn.WBS_CODE + "\""
                    });
                }else if (!regexWbsNumber.IsMatch(toReturn.WBS_CODE))
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                        Title = "WBS Number format is invalid.  WBS Number should contain 0-5 characters per level with a maximum of 10 levels seperated by periods.",
                        Message = "Task File, WBS: " + toReturn.WBS_CODE
                    });
                }

                //toReturn.TSK_DESC is both the Title and the Description.  Description has no limit but title does.
                if (toReturn.TSK_DESC.Length > 100)
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                        Title = "Task Title exceeds maximum character limit",
                        Message = "WBS: " + toReturn.WBS_CODE + " Data found:" + toReturn.TSK_DESC
                    });
                }

            }

            return toReturn;   
        }

        private WbsDTO WBSCreation(string[] columns, int workspaceID, int WBSIDCounter, Collection<ImportErrorMessage> ErrorMessages)
        {

            if (columns.Length < 4)
            {
                throw new IncorrectColumnCountException();
            }


            WbsDTO toReturn = null;
            if (columns[0].Equals("STRUCTURE"))
            {
                toReturn = new WbsDTO();
                toReturn.Id = WBSIDCounter;
                toReturn.WbsNumber = this.PrepData(columns[2]);
                toReturn.WbsTitle = this.PrepData(columns[5]);
                toReturn.WorkspaceID = workspaceID;
                toReturn.Updateable = UpdateType.Upsert;

                if (toReturn.WbsNumber.Length > 100)
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                        Title = "The WBS Number is not a valid format.",
                        Message = "WBS: " + toReturn.WbsNumber
                    });
                }

                if (toReturn.WbsTitle.Length > 100)
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                        Title = "The WBS Title exceeds maximum character limit",
                        Message = "WBS: " + toReturn.WbsNumber + " Data found:" + toReturn.WbsTitle
                    });
                }

                if (toReturn.WbsNumber.Length > 30)
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                        Title = "WBS Number exceeds maximum character limit of 30 characters.",
                        Message = "WBS: " + toReturn.WbsNumber
                    });
                }

                if (toReturn.WbsNumber.Length > 30 || toReturn.WbsNumber.Length < 1)
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                        Title = "A WBS Number must exist and not exceed the maximum character limit of 30 characters.",
                        Message = "WBS File, WBS Number: \"" + toReturn.WbsNumber + "\""
                    });
                }else if (!regexWbsNumber.IsMatch(toReturn.WbsNumber) || toReturn.WbsNumber.Length > 30)
                {
                    ErrorMessages.Add(new ImportErrorMessage()
                    {
                        Title = "WBS Number format is invalid.  WBS Number should contain 0-5 characters per level with a maximum of 10 levels seperated by periods and should be no more than 30 characters in total.",
                        Message = "WBS File, WBS Number: \"" + toReturn.WbsNumber +"\""
                    });
                }
            }

            
            return toReturn;
        }

        private List<String> fixColumns(String[] columns)
        {
            List<String> fixedArray = new List<String>();

            for (int x = 0; x < columns.Length; x++ )
            {
                if (columns[x].StartsWith("'") && !(columns[x].EndsWith("'")))
                {
                    StringBuilder fixIT = new  StringBuilder(columns[x]);
                    fixIT.Append(",");
                    string columnX = columns[++x];
                    fixIT.Append(columnX);

                    while (!columnX.EndsWith("'"))
                    {
                        columnX = columns[++x];
                        fixIT.Append(",");
                        fixIT.Append(columnX);
                    }

                    fixedArray.Add(fixIT.ToString());
                }
                else
                {
                    fixedArray.Add(columns[x]);
                }
            }

            return fixedArray;

         }
        
        private String PrepDate(String data)
        {
            if (data.StartsWith("'") && data.EndsWith("'"))
            {
                data = data.Substring(3, 6);
            }
            else
            {
                data = data.Substring(2, 6);
            }

            data = data.Trim();

            return data;
        }

        private String PrepData(String data)
        {
            if (data.StartsWith("'") && data.EndsWith("'"))
            {
                data = data.Substring(1, data.Length - 2);
            }

            data = data.Trim();

            return data;
        }
    }

    class TaskDataFromFile
    {
        public TaskDataFromFile()
        {
            this.IMS_CODE = "";
            this.TSK_DESC = "";
            this.WBS_CODE = "";
            this.Start_Date=DateTime.MinValue;
            this.End_Date = DateTime.MinValue;
        }

        public String IMS_CODE{get; set;}
        public String TSK_DESC { get; set; }
        public String WBS_CODE { get; set; }
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
    }
}

