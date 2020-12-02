// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WBS.BOE
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using MoreLinq;

    public class ValidateBOE : IValidateBOE
    {
        public const string START_DATE_INVALID = "Start Date must be on or after the {0} Start Date.";
        public const string END_DATE_INVALID = "End Date must be on or before the {0} End Date.";
        public const string DATE_RANGE_INVALID = "End Date must be on or after the Start Date.";
        public const string CLIN_TEXT = "CLIN";
        public const string CONTRACT_TEXT = "Contract";
        private IVariableSelectBOEtoSumCalculation _VariableSelectBOEtoSumCalculation;
        private BOECommentsResponsesValidator _BOECommentsResponsesValidator;
        private ITripDTODataLoader _TripDTODataLoader;
        private IMiscTravelRateDTOLoader miscTravelRateDTOLoader;
        private ILocationDTODataLoader _LocationDTODataLoader;
        private IOffloadRatesDTOLoader offloadRatesDTOLoader;
        private IRteTemplateDataLoader rteTemplateDataLoader;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ValidateBOE(
            IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
            BOECommentsResponsesValidator inBOECommentsResponsesValidator,
            ITripDTODataLoader inTripDTODataLoader,
            IMiscTravelRateDTOLoader inMiscTravelRateDTOLoader,
            ILocationDTODataLoader inLocationDTODataLoader,
            IOffloadRatesDTOLoader offloadRatesDTOLoader,
            IRteTemplateDataLoader rteTemplateDataLoader
            )
        {
            this.miscTravelRateDTOLoader = inMiscTravelRateDTOLoader;
            this._LocationDTODataLoader = inLocationDTODataLoader;
            this._TripDTODataLoader = inTripDTODataLoader;
            this._VariableSelectBOEtoSumCalculation = inVariableSelectBOEtoSumCalculation;
            this._BOECommentsResponsesValidator = inBOECommentsResponsesValidator;

            this.offloadRatesDTOLoader = offloadRatesDTOLoader;
            this.rteTemplateDataLoader = rteTemplateDataLoader;
        }

        /// <summary>
        /// Validate all BOE data (BOE Header, task element details, labor type, and labor spreads)
        /// when the user selects the Validate button
        /// </summary>
        /// <param name="inBOE">the BOE DTO to validate</param>
        /// <param name="ws">Full WS</param>
        /// <returns>all possible validation messages</returns>
        /// Suppressed the following messages because 1) I do use BoeLabor just not in the way the code analysis wants me too and 2) if you can make this less complex, go for it!
        public virtual ValidationBOEModelView ValidateBOE_OnValidateBtnClick(FullBoe inBOE, FullWorkspace ws)
        {
            // See wireframes for what should be checked on "Validate" button click.
            // Basically, we're checking for MIA required fields that are not verified upon a Save
            // and that the total of all labor spreads in a task element equals the moq equation total

            // Currently the Validate function is only checking if required entries are not filled in

            if (inBOE == null)
            {
                throw new ArgumentNullException(nameof(inBOE));
            }

            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            #region Register Variables
            // the validation BOE to populate while traversing through the entire BOE
            ValidationBOEModelView ValidationBOE = new ValidationBOEModelView();

            // the BOE validation to return
            ValidationBOEModelView toReturn = null;

            // the boe task validation class
            ValidationBOETasks boeTasks = new ValidationBOETasks();

            // All the validation messages that occur when checking the task element
            Collection<string> TaskElementMessages = new Collection<string>();

            // the boe labor type validation class
            ValidationBOELaborType boeLabor = new ValidationBOELaborType();

            // All the validation messages that occur when checking the labor types
            Collection<string> LaborTypeMessages = new Collection<string>();

            // the material task validation class
            ValidationBOETasks materialTasks = new ValidationBOETasks();

            // All the validation messages that occur when checking the task element
            Collection<string> materialTaskElementMessages = new Collection<string>();

            // the travel task validation class
            ValidationBOETasks travelTasks = new ValidationBOETasks();

            // All the validation messages that occur when checking the task element
            Collection<string> travelTaskElementMessages = new Collection<string>();

            // All the validation messages that occur when checking the material types
            Collection<string> TravelTypeMessages = new Collection<string>();

            // the boe travel type validation class
            ValidationBOELaborType travelType = new ValidationBOELaborType();

            // the ODC task validation class
            ValidationBOETasks odcTasks = new ValidationBOETasks();

            // All the validation messages that occur when checking the ODC task element
            Collection<string> OdcTaskElementMessages = new Collection<string>();

            // the ODCtype validation class
            ValidationBOELaborType odcType = new ValidationBOELaborType();

            // All the validation messages that occur when checking the  types
            Collection<string> OdcTypeMessages = new Collection<string>();

            // the result of the MOQ Equation
            string MOQEquationCalc = string.Empty;

            // total labor spread value
            decimal TotalLaborSpreadValue = 0;
            #endregion Register Variables

            ws.LoadODCsRTEData();
            ws.LoadMaterialsRTEData();
            ws.LoadTaskElementRTEData();

            // Let's begin the validation....

            // validate BOE Date is either within the CLIN Date 
            if (inBOE.Clin != null)
            {
                ClinDTO clin = inBOE.Clin;

                if (clin.StartDate.HasValue && clin.EndDate.HasValue)
                {
                    if (inBOE.StartDate < clin.StartDate)
                    {
                        ValidationBOE.BOEHeaderMsgs.Add(string.Format(BoeDTO.BOE_START_DATE_INVALID, CLIN_TEXT, clin.StartDate.Value.ToString("MM/yyyy")));
                    }

                    if (inBOE.EndDate > clin.EndDate)
                    {
                        ValidationBOE.BOEHeaderMsgs.Add(string.Format(BoeDTO.BOE_END_DATE_INVALID, CLIN_TEXT, clin.EndDate.Value.ToString("MM/yyyy")));
                    }
                }
            }

            // check the contract date          
            this._CheckIfBOEDateIsValidAgainstContractDate(inBOE, ValidationBOE, ws);
            
            // validate BOE description
            if (!this.IsDescriptionValid(inBOE, ws.Id))
            {
                ValidationBOE.BOEHeaderMsgs.Add(BoeDTO.BOE_DESC_REQUIRED);
            }

            // validate sources of data
            if (!this.IsSourcesOfDataValid(inBOE, ws.Id))
            {
                ValidationBOE.BOEHeaderMsgs.Add(BoeDTO.DATA_SOURCE_REQUIRED);
            }

            ValidateRTEFieldLength(ws, inBOE.Description, "BOE Description", ValidationBOE.BOEHeaderMsgs);
            ValidateRTEFieldLength(ws, inBOE.DataSource, "BOE Data Source", ValidationBOE.BOEHeaderMsgs);

            // validate any BOE Custom Fields
            Collection<string> boeCustomFieldMsgs = this.ValidateBOECustomFields(ws, inBOE);
            foreach (string msg in boeCustomFieldMsgs)
            {
                ValidationBOE.BOECustomFieldValidationMessages.Add(msg);
            }

            // validate if there is at least one task element associated with the BOE. it can be cost or labor, just needs at least one of either
            // need to place so it's a "task" for purposes of validation
            if (!ws.TaskElements.Any(x => (x.BoeID == inBOE.Id)) &&
                !ws.Odcs.Any(x => x.BoeID == inBOE.Id) &&
                !ws.Materials.Any(x => x.BoeID == inBOE.Id) &&
                !ws.Travels.Any(x => x.BoeID == inBOE.Id))
            {
                boeTasks = new ValidationBOETasks();
                TaskElementMessages = new Collection<string>();
                TaskElementMessages.Add(BoeDTO.ONE_TASK_ELEMENT_REQUIRED);
                boeTasks.TaskMessage = "Task: ";
                boeTasks.TaskElementDetails.TaskElementDetailValidationMessages = TaskElementMessages;
                ValidationBOE.Tasks.Add(boeTasks);
            }

            if (ws.Travels.Any(x => x.BoeID == inBOE.Id))
            {
                this._ValidateTravel(inBOE, ws, ValidationBOE, ref travelTasks, ref travelTaskElementMessages, ref TravelTypeMessages, travelType);
            }

            if (ws.Materials.Any(x => x.BoeID == inBOE.Id))
            {
                this._ValidateMaterials(inBOE, ValidationBOE, ref materialTasks, ref materialTaskElementMessages);
            }

            if (ws.TaskElements.Any(x => x.BoeID == inBOE.Id))
            {
                this._ValidateLaborTaskElement(ws, inBOE, ValidationBOE, ref boeTasks, ref TaskElementMessages, boeLabor, ref LaborTypeMessages, ref MOQEquationCalc, ref TotalLaborSpreadValue);
            }

            if (ws.Odcs.Any(x => x.BoeID == inBOE.Id))
            {
                this._ValidateODC(inBOE, ws, ValidationBOE, ref odcTasks, ref OdcTaskElementMessages, odcType, ref OdcTypeMessages, ref TotalLaborSpreadValue);
            }

            // Valid Comments and Approvals
            // validate that all boe comments have a response
            bool boeCommentResponseValidator = this._BOECommentsResponsesValidator.AllBOEAuthorCommentsResponses(inBOE.Id);
            if (boeCommentResponseValidator == false)
            {
                ValidationBOE.BOECommentandApprovals.Add("All BOE Comments do not have a response from the Author.");
            }

            this.ValidateTemplateMoqTypes(ws, inBOE, ValidationBOE);

            // Setting the name for the WBS - incase we have multiple WBS's we would want to list them out.
            ValidationBOE.BOEName = (inBOE.Wbs != null ? inBOE.Wbs.WbsString : CommonConstants.Unassigned_WBS_Display_Text) + " " + (inBOE.Clin != null ? inBOE.Clin.ClinString : CommonConstants.Unassigned_CLIN_Display_Text) + " " + inBOE.Title;
            // sets the boe ID - this is used on the validate all so we can link the boe's back to the users.
            ValidationBOE.BOEID = inBOE.Id;

            toReturn = ValidationBOE;
            return toReturn;
        }

        /// <summary>
        /// Validate all BOE data (BOE Header, task element details, labor type, and labor spreads) in the workspace
        /// </summary>
        /// <param name="ws">Full Ws</param>
        /// <returns>Validation Data</returns>
        public virtual ValidationAllBOEModelView ValidateAllBOEs(FullWorkspace ws)
        {
            // Make sure the workspace is not null
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            // Define a new collection
            ValidationAllBOEModelView CollectionOfErrors = new ValidationAllBOEModelView();

            // preload RTE data.. for validation
            ws.LoadBoesRTEData();

            // For every BOE
            foreach (FullBoe boe in ws.Boes)
            {
                CollectionOfErrors.AllBOEs.Add(this.ValidateBOE_OnValidateBtnClick(boe, ws));
            }

            // return the collection of error messages.
            return CollectionOfErrors;
        }

        /// <summary>
        /// Given a BOE check if it's valid against the workspace contract date
        /// </summary>
        /// <param name="inBOE">BOE</param>
        /// <param name="ValidationBOE">validation model view</param>
        /// <param name="workspace">workspace</param>
        private void _CheckIfBOEDateIsValidAgainstContractDate(FullBoe inBOE, ValidationBOEModelView ValidationBOE, WorkspaceDTO workspace)
        {
            if (GenBOEUtilities.AdjustDateTimePrecision(inBOE.StartDate, DateTimePrecision.Month) < GenBOEUtilities.AdjustDateTimePrecision(workspace.ContractStartDate, DateTimePrecision.Month))
            {
                ValidationBOE.BOEHeaderMsgs.Add(string.Format(BoeDTO.BOE_START_DATE_INVALID, CONTRACT_TEXT, workspace.ContractStartDate.ToString("MM/yyyy")));
            }

            if (GenBOEUtilities.AdjustDateTimePrecision(inBOE.EndDate, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision(workspace.ContractEndDate, DateTimePrecision.Month))
            {
                ValidationBOE.BOEHeaderMsgs.Add(string.Format(BoeDTO.BOE_END_DATE_INVALID, CONTRACT_TEXT, workspace.ContractEndDate.ToString("MM/yyyy")));
            }
        }

        /// <summary>
        /// Validate (non-MST) Travel Task
        /// </summary>
        /// <param name="inBOE">BOE containing Travel</param>
        /// <param name="ws">Workspace containing BOE/Travel</param>
        /// <param name="ValidationBOE">Validation BOE Model View</param>
        /// <param name="travelTasks">Validation BOE Tasks for the Travel Task</param>
        /// <param name="travelTaskElementMessages">Collection of Travel Task Element Messages</param>
        /// <param name="TravelTypeMessages">Collection of Travel Type Messages</param>
        /// <param name="travelType">Validation BOE Labor Type for the Travel Type</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "4#")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "5#")]
        protected virtual void _ValidateTravel(FullBoe inBOE, FullWorkspace ws, ValidationBOEModelView ValidationBOE, ref ValidationBOETasks travelTasks, ref Collection<string> travelTaskElementMessages, ref Collection<string> TravelTypeMessages, ValidationBOELaborType travelType)
        {
            _ = inBOE ?? throw new ArgumentNullException(nameof(inBOE));
            _ = ws ?? throw new ArgumentNullException(nameof(ws));
            _ = ValidationBOE ?? throw new ArgumentNullException(nameof(ValidationBOE));

            // validate the travel Tasks
            var travelTaskElements = ws.Travels.Where(x => x.BoeID == inBOE.Id).ToList();
            HashSet<TripDTO> trips = new HashSet<TripDTO>(this._TripDTODataLoader.GetByIds(travelTaskElements.SelectMany(t => t.TravelTrips).Select(i => i.SystemTripID).ToCollection()));
            HashSet<LocationDTO> departures = new HashSet<LocationDTO>(this._LocationDTODataLoader.GetByIds(trips.Select(i => i.DepartureLocationID).ToCollection()));
            HashSet<LocationDTO> destinations = new HashSet<LocationDTO>(this._LocationDTODataLoader.GetByIds(trips.Select(i => i.DestinationLocationID).ToCollection()));
            HashSet<MiscTravelRateDTO> travelRates = new HashSet<MiscTravelRateDTO>(this.miscTravelRateDTOLoader.GetByIds(trips.Select(i => i.MiscTravelRateID).ToCollection()));

            foreach (TravelDTO travelTask in travelTaskElements)
            {
                travelTasks = new ValidationBOETasks();
                travelTaskElementMessages = new Collection<string>();

                // validate one trip present
                if (!travelTask.TravelTrips.Any()) 
                {
                    travelTaskElementMessages.Add(TravelDTO.ONE_TRIP_REQUIRED);
                }

                // validate travel task is within the BOE date range
                bool isValid = this._ValidateTaskElementStartDateComparedToBOEStartDate(inBOE.StartDate, travelTask.StartDate.Value);
                if (!isValid)
                {
                    travelTaskElementMessages.Add(string.Format(TravelDTO.TRAVEL_TASK_START_DATE_INVALID, inBOE.StartDate.ToString("MM/yyyy")));
                }

                isValid = this._ValidateTaskElementEndDateComparedToBOEStartDate(inBOE.EndDate, travelTask.EndDate.Value);
                if (!isValid)
                {
                    travelTaskElementMessages.Add(string.Format(TravelDTO.TRAVEL_TASK_END_DATE_INVALID, inBOE.EndDate.ToString("MM/yyyy")));
                }

                Collection<string> travelTaskCustomFieldMessages = this._ValidateTravelTaskCustomFields(ws, travelTask);
                if (travelTaskCustomFieldMessages.Any())
                {
                    foreach (string message in travelTaskCustomFieldMessages)
                    {
                        travelTaskElementMessages.Add(message);
                    }
                }

                foreach (TravelTripType travel in travelTask.TravelTrips)
                {
                    TravelTypeMessages = new Collection<string>();
                    bool isTripValid = true;
                    travelType = new ValidationBOELaborType();

                    // validate LT Start/End Date
                    var returnedMessages = this._ValidateTravelTripDate(travelTask, travel, ws, inBOE);
                    foreach (string message in returnedMessages)
                    {
                        TravelTypeMessages.Add(message);
                    }

                    isTripValid = this._ValidateTravelTripSegment(travel, ws);
                    if (!isTripValid)
                    {
                        TravelTypeMessages.Add(string.Format(TravelDTO.TRAVEL_TASK_SEGMENT_INVALID));
                    }

                    isTripValid = this._ValidateTravelTripPerformingOrg(travel);
                    if (!isTripValid)
                    {
                        TravelTypeMessages.Add(string.Format(TravelDTO.TRAVEL_TASK_PERFORG_INVALID));
                    }

                    Collection<string> CustomFieldMessages = this._ValidateTravelTripCustomFields(ws, travel);
                    if (CustomFieldMessages.Any())
                    {
                        foreach (string message in CustomFieldMessages)
                        {
                            TravelTypeMessages.Add(message);
                        }
                    }

                    if (TravelTypeMessages.Any())
                    {
                        TripDTO thisTrip = trips.First(i => i.TripID == travel.SystemTripID);
                        string mode = travelRates.First(i => i.Id == thisTrip.MiscTravelRateID).MiscTravelRateMode;
                        string departureName = departures.First(i => i.Id == thisTrip.DepartureLocationID).LocationName;
                        string destinationName = destinations.First(i => i.Id == thisTrip.DestinationLocationID).LocationName;

                        travelType.LaborTypeHeader = string.Format("Trip: {0} {1} {2} to {3}", thisTrip.TripID, mode, departureName, destinationName);
                        travelType.LaborTypeValidationMsgs = TravelTypeMessages;
                        travelTasks.LaborTypes.Add(travelType);
                    }
                }

                if (travelTaskElementMessages.Any() || travelTasks.LaborTypes.Any())
                {
                    if (travelTaskElementMessages.Any())
                    {
                        travelTasks.TaskMessage = "Task: " + travelTask.TaskID + " " + travelTask.TaskTitle;
                        travelTasks.TaskElementDetails.TaskElementDetailValidationMessages = travelTaskElementMessages;
                        travelTasks.TaskElementDetails.TaskElementDetailsHeader = "Task Element Details";
                    }
                    // sets the id for the task that has errors.
                    travelTasks.TaskId = travelTask.Id;
                    ValidationBOE.Travels.Add(travelTasks);
                }
            }
        }
        
        private void _ValidateMaterials(FullBoe inBOE, ValidationBOEModelView ValidationBOE, ref ValidationBOETasks materialTasks, ref Collection<string> materialTaskElementMessages)
        {
            // validate the Material Tasks
            IReadOnlyCollection<MaterialDTO> materialTaskElements = inBOE.Materials;
            foreach (MaterialDTO materialTask in materialTaskElements)
            {
                materialTasks = new ValidationBOETasks();
                materialTaskElementMessages = new Collection<string>();

                // validate MOQ Type
                if (string.IsNullOrEmpty(materialTask.MoqText))
                {
                    materialTaskElementMessages.Add(this.FormatMOQTextErrorMessage(MaterialDTO.MOQ_TEXT_REQUIRED));
                }

                if (materialTaskElementMessages.Any() || materialTasks.LaborTypes.Any())
                {
                    materialTasks.TaskMessage = "Task: " + materialTask.TaskID + " " + materialTask.TaskTitle;
                    materialTasks.TaskElementDetails.TaskElementDetailValidationMessages = materialTaskElementMessages;
                    materialTasks.TaskElementDetails.TaskElementDetailsHeader = "Task Element Details";
                    // added so the validate all boe can link to the task id
                    materialTasks.TaskId = materialTask.Id;
                    ValidationBOE.Materials.Add(materialTasks);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void _ValidateLaborTaskElement(FullWorkspace workspace, FullBoe inBOE, ValidationBOEModelView ValidationBOE, ref ValidationBOETasks boeTasks, ref Collection<string> TaskElementMessages, ValidationBOELaborType boeLabor, ref Collection<string> LaborTypeMessages, ref string MOQEquationCalc, ref decimal TotalLaborSpreadValue)
        {
            Collection<string> ReturnMsgs;
            ICollection<OffloadRatesDTO> offloadRatesDtos = null;
            if (workspace.ProjectMapType == ProjectMapType.StandardWithOffload)
            {
                // Validate Labor Resources for Offload
                offloadRatesDtos = this.offloadRatesDTOLoader.GetByWorkspaceId(workspace.Id);
            }

            // validate the Task Detail Elements
            var boeTaskElements = workspace.TaskElements.Where(x => x.BoeID == inBOE.Id);
            foreach (BoeTaskElementDTO boeTask in boeTaskElements)
            {
                boeLabor = new ValidationBOELaborType();
                boeTasks = new ValidationBOETasks();
                boeTasks.LaborTypes.Clear();
                MOQEquationCalc = string.Empty;
                TaskElementMessages = new Collection<string>();
                LaborTypeMessages = new Collection<string>();

                // (rule only valid for Labor TE)
                // validate MOQ Type
                if (!workspace.UsingTemplateBOE && boeTask.TaskElementType == TaskElementType.Labor && boeTask.MOQType == MOQType.None)
                {
                    TaskElementMessages.Add(BoeDTO.MOQ_TYPE_REQUIRED);
                }

                // (rule only valid for non summary Labor TE)
                //  validate MOQ Equation
                if (boeTask.TaskElementType == TaskElementType.Labor && string.IsNullOrEmpty(boeTask.MOQHoursEquation))
                {
                    TaskElementMessages.Add(BoeDTO.MOQ_EQ_REQUIRED);
                    TaskElementMessages.Add(BoeDTO.TOTAL_LABOR_SPREAD_INVALID);
                }
                else
                {
                    Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO>();

                    for (int x = 0; x < boeTask.WorkspaceVariableIDs.Count; x++)
                    {
                        // for each workspace variable id in the BOE Task, get it's info from the Workspace variables
                        foreach (WorkspaceVariableDTO wv in inBOE.WorkspaceVariables)
                        {
                            if (wv.Id == boeTask.WorkspaceVariableIDs[x])
                            {
                                WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO();
                                workspaceVar = wv;
                                workspaceVars.Add(workspaceVar);
                                break;
                            }
                        }
                    }

                    // this will get all workspace variables for the given workspace
                    DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                    data.FillData(boeTask.OrdinaryVariables, workspaceVars, workspace);

                    try
                    {
                        MOQEquationCalc = Common.MOQ.Parser.Calculate(boeTask.MOQHoursEquation, boeTask.OrdinaryVariables, workspaceVars, this._VariableSelectBOEtoSumCalculation, data, workspace);
                    }
                    catch // if the equation parsing crashes (maybe a variable is missing)
                    {
                        TaskElementMessages.Add("MOQ Equation is invalid.");
                        MOQEquationCalc = "0";
                    }
                }

                // validate if historic metric disclosure checkbox was checked or not
                // based on if a historic meric is being used
                ICollection<int> boeTaskIds = new Collection<int>();
                boeTaskIds.Add(boeTask.Id);

                if (this.IsHistoricMetricDisclosureRequired(boeTaskIds, inBOE))
                {
                    ValidationBOE.BOEHeaderMsgs.Add(BoeDTO.HISTORIC_METRIC_DISCLOSURE_REQUIRED);
                }

                // Need to determine if there are any required Task custom fields
                switch (boeTask.TaskElementType)
                {
                    case TaskElementType.Labor:
                        ReturnMsgs = this._ValidateTaskCustomFields(workspace, boeTask);
                        foreach (string msg in ReturnMsgs)
                        {
                            TaskElementMessages.Add(msg);
                        }

                        break;
                    case TaskElementType.None:
                    default:
                        break;
                }

                ValidateTaskMoqText(workspace, inBOE, TaskElementMessages, boeTask);
                ValidateTaskDescription(workspace, inBOE, TaskElementMessages, boeTask);

                if (boeTask.TaskElementType == TaskElementType.Labor)
                {
                    // validate TE Start/End Date
                    ReturnMsgs = this._ValidateStartAndEndDates(inBOE.StartDate, inBOE.EndDate, inBOE, boeTask.StartDate, boeTask.EndDate, workspace);
                    foreach (string msg in ReturnMsgs)
                    {
                        TaskElementMessages.Add(msg);
                    }
                }

                // Validate Resource Types
                if (!boeTask.taskElementLabors.Any())
                {
                    if (boeTask.TaskElementType == TaskElementType.Labor)
                    {
                        LaborTypeMessages.Add(BoeDTO.RESOURCE_TYPE_FOR_TASK_ELEMENT_REQUIRED);

                        boeLabor.LaborTypeHeader = "Resource Type:";
                        boeLabor.LaborTypeValidationMsgs = LaborTypeMessages;
                        boeTasks.LaborTypes.Add(boeLabor);
                    }
                }

                // validate RTE field length
                ValidateRTEFieldLength(workspace, boeTask.Description, "Task Description", TaskElementMessages);
                ValidateRTEFieldLength(workspace, boeTask.MOQText, "MOQ Rationale", TaskElementMessages);

                List<ResourceDTO> resourcesFromTask = workspace.ResourcesForWsResourceListId.Where(x => (boeTask.taskElementLabors.Where(y => y.ResourceID.HasValue).Select(z => z.ResourceID.Value)).Contains(x.Id)).ToList();
                TotalLaborSpreadValue = ValidateResourceLabors(workspace, inBOE, boeTasks, LaborTypeMessages, offloadRatesDtos, boeTask, resourcesFromTask);

                // only check moq equation total if this is a labor task element
                if (boeTask.TaskElementType == TaskElementType.Labor)
                {
                    if (!string.IsNullOrEmpty(MOQEquationCalc))
                    {
                        if (TotalLaborSpreadValue != decimal.Parse(MOQEquationCalc))
                        {
                            TaskElementMessages.Add(BoeDTO.TOTAL_LABOR_SPREAD_INVALID);
                        }
                    }
                }

                if (TaskElementMessages.Any() || boeTasks.LaborTypes.Any())
                {
                    boeTasks.TaskMessage = "Task: " + boeTask.BOETaskID + " " + boeTask.TaskTitle;
                    boeTasks.TaskElementDetails.TaskElementDetailValidationMessages = TaskElementMessages;
                    boeTasks.TaskElementDetails.TaskElementDetailsHeader = "Task Element Details";
                    // setting for the validation all boe to link the user to the task.
                    boeTasks.TaskId = boeTask.Id;
                    ValidationBOE.Tasks.Add(boeTasks);
                }
            }
        }

        /// <summary>
        /// Validates Tasks MOQ Text, including RTE Templates in the process
        /// </summary>        
        private void ValidateTaskMoqText(FullWorkspace workspace, FullBoe inBOE, Collection<string> TaskElementMessages, BoeTaskElementDTO boeTask)
        {
            if (!workspace.UsingTemplateBOE)
            {
                // Legacy MOQ Types
                if (string.IsNullOrEmpty(boeTask.MOQText)) // if no data in the field itself
                {
                    // the field is valid if templates are being used, AND all required prompts are answered
                    var taskTemplateWithPrompts = this.rteTemplateDataLoader.GetByBoeIdAndTaskId(workspace.Id, inBOE.Id, boeTask.Id).Where(t => t.SourceId == (int)RteTemplateSource.TaskMOQ);
                    if (!taskTemplateWithPrompts.Any() || taskTemplateWithPrompts.Any(t => t.Required && string.IsNullOrEmpty(t.AnswerText)))
                    {
                        TaskElementMessages.Add(this.FormatMOQTextErrorMessage(BoeDTO.MOQ_TEXT_REQUIRED));
                    }
                }
            }
        }

        /// <summary>
        /// Validate Template MOQ Types
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <param name="boe">BOE to validate</param>
        /// <param name="boeValidation">Boe Validation</param>
        private void ValidateTemplateMoqTypes(FullWorkspace ws, FullBoe boe, ValidationBOEModelView boeValidation)
        {
            if (ws.UsingTemplateBOE)
            {
                ICollection<string> errorMessages;

                foreach (BoeTaskElementDTO task in boe.TaskElements)
                {
                    ICollection<MOQType> moqTypesUsedByTasksResourceTypes = task.taskElementLabors.Where(x => x.MoqTypeSelectionId.HasValue).Select(x => (MOQType)x.MoqTypeSelectionId).ToList();

                    errorMessages = ValidateTemplateMoqForTask(boe.MoqTypeSelections.Where(x => x.TaskId == task.Id).ToList(), moqTypesUsedByTasksResourceTypes);
                    errorMessages.AddRange(ValidateTemplateMoqForLaborType(task, ws.DecimalPrecision));

                    if (errorMessages.Any())
                    {
                        ValidationBOETasks taskValidation = boeValidation.Tasks.FirstOrDefault(x => x.TaskId == task.Id);
                        if (taskValidation == null)
                        {
                            taskValidation = new ValidationBOETasks() { TaskId = task.Id, TaskMessage = $"Task: {task.Id} {task.TaskTitle}", TaskElementDetails = new ValidationBOETaskElementDetails() { TaskElementDetailsHeader = "Task Element Details" } };
                            boeValidation.Tasks.Add(taskValidation);
                        }

                        taskValidation.TaskElementDetails.TaskElementDetailValidationMessages.AddRange(errorMessages);
                    }
                }
            }
        }

        /// <summary>
        /// Validate MOQ Template data on a Task Level. Does NOT validate Labor Type level selection
        /// </summary>
        /// <param name="moqTypesForTask">MOQ Types that belong to the task</param>
        /// <returns>Errors, if any</returns>
        public static Collection<string> ValidateTemplateMoqForTask(ICollection<MoqTypeSelection> moqTypesForTask, ICollection<MOQType> moqTypesSelectedInResourceTypes)
        {
            _ = moqTypesForTask ?? throw new ArgumentNullException(nameof(moqTypesForTask));

            Collection<string> errorMessages = new Collection<string>();

            if (!moqTypesForTask.Any())
            {
                errorMessages.Add(Constants.MOQ_TYPE_REQUIRED_FOR_TASK);
            }
            else
            {
                MoqTypeTableDataLabels labels = new MoqTypeTableDataLabels();

                moqTypesForTask.ForEach(moqType =>
                {
                    switch (moqType.SelectedMOQType)
                    {
                        case (MOQType.AnalogousRelationships):
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.CerName, "Analogous relationship name", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.CerLocation, "Analogous relationship location in the proposal", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.Rationale, "Rationale", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.SkillMixRationale, "Skill Mix Rationale", errorMessages);
                            break;
                        case (MOQType.Comparative):
                        case (MOQType.Historical):
                            if (moqType.TableData.None()) { errorMessages.Add($"{moqType.SelectedMOQType.GetDescription()}: Table data is required."); }
                            moqType.TableData.ForEach(row =>
                            {
                                ValidateRequiredField(moqType.SelectedMOQType, row.TableName, labels.TableName, errorMessages);

                                if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
                                {
                                    ValidateRequiredField(moqType.SelectedMOQType, row.RepositoryName, labels.RepositoryName, errorMessages);
                                    ValidateRequiredField(moqType.SelectedMOQType, row.QueryType, labels.QueryType, errorMessages);
                                }
                                else
                                {
                                    ValidateRequiredField(moqType.SelectedMOQType, row.ContractNumber, labels.ContractNumber, errorMessages);
                                    if (row.TotalWbsHours <= 0) { errorMessages.Add($"{moqType.SelectedMOQType.GetDescription()}: {labels.TotalWbsHours} must be a number greater than 0."); }
                                }

                                if (row.DateOfReport.Year == 1) { errorMessages.Add($"{moqType.SelectedMOQType.GetDescription()}: {labels.DateOfReport} is required."); }
                                if (row.DateOfReport > DateTime.Now.Date) { errorMessages.Add($"{moqType.SelectedMOQType.GetDescription()}: {labels.DateOfReport} must be on or before today's date."); }

                                ValidateRequiredField(moqType.SelectedMOQType, row.HistoricalProgramName, labels.HistoricalProgramName, errorMessages);
                                ValidateRequiredField(moqType.SelectedMOQType, row.WbsElement, labels.WbsElement, errorMessages);

                                if (row.PoPStart.Year == 1) { errorMessages.Add($"{moqType.SelectedMOQType.GetDescription()}: {labels.PoPStart} is required."); }
                                if (row.PoPStart > DateTime.Now.Date) { errorMessages.Add($"{moqType.SelectedMOQType.GetDescription()}: {labels.PoPStart} must be on or before today's date."); }
                                if (row.PoPEnd.Year == 1) { errorMessages.Add($"{moqType.SelectedMOQType.GetDescription()}: {labels.PoPStart} is required."); }
                                if (row.PoPEnd > DateTime.Now.Date) { errorMessages.Add($"{moqType.SelectedMOQType.GetDescription()}: {labels.PoPEnd} must be on or before today's date."); }

                                if (row.PoPEnd < row.PoPStart) { errorMessages.Add($"{moqType.SelectedMOQType.GetDescription()}: {labels.PoPStart} must be on or before {labels.PoPEnd}."); }
                                
                                ValidateRequiredField(moqType.SelectedMOQType, row.AdditionalQueryFilters, labels.AdditionalQueryFilters, errorMessages);

                                if (row.TotalRelevantHours <= 0) { errorMessages.Add($"{moqType.SelectedMOQType.GetDescription()}: {labels.TotalRelevantHours} must be a number greater than 0."); }

                                if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST && row.WbsElement.Length > 12)
                                {
                                    errorMessages.Add($"{moqType.SelectedMOQType.GetDescription()}: {labels.WbsElement} must be 12 characters or less.");
                                }
                            });
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.Rationale, "Rationale", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.SkillMixRationale, "Skill Mix Rationale", errorMessages);
                            break;
                        case (MOQType.CostEstimatingRelationships):
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.CerName, "CER tool name", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.CerLocation, "CER tool location in the proposal", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.Rationale, "Rationale", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.SkillMixRationale, "Skill Mix Rationale", errorMessages);
                            break;
                        case (MOQType.LOE):
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.DescriptionHoursRequired, "Description of Hours required", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.Rationale, "Rationale", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.SkillMixRationale, "Skill Mix Rationale", errorMessages);
                            break;
                        case (MOQType.NonLabor):
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.Rationale, "Rationale", errorMessages);
                            break;
                        case (MOQType.ParametricEstimates):
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.CerName, "Parametric model name", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.CerLocation, "Parametric model location in the proposal", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.Rationale, "Rationale", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.SkillMixRationale, "Skill Mix Rationale", errorMessages);
                            break;
                        case (MOQType.SME):
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.SmeReason, "The SME selected Expert judgement reasons", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.SmeHoursLogic, "The logic and assumptions used to estimate hours", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.SmeDurationLogic, "The logic and assumptions used to estimate duration", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.SmeTaskEstimates, "The SME tasks estimated in this BOE", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.SkillMixRationale, "Skill Mix Rationale", errorMessages);
                            break;
                        case (MOQType.SOW):
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.DescriptionHoursRequired, "Description of Hours required & location in SOW", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.Rationale, "Rationale", errorMessages);
                            ValidateRequiredField(moqType.SelectedMOQType, moqType.SkillMixRationale, "Skill Mix Rationale", errorMessages);
                            break;
                        default:
                            errorMessages.Add("Invalid MOQ Type selected");
                            break;
                    };

                    if (!moqTypesSelectedInResourceTypes.Contains(moqType.SelectedMOQType))
                    {
                        errorMessages.Add($"{moqType.SelectedMOQType.GetDescription()}: All MOQ Types must be selected by at least one Resource Type.");
                    }
                });
            }

            return errorMessages;
        }

        /// <summary>
        /// Validates Moq Selection for a Labor Type
        /// </summary>
        /// <param name="task">Task to validate</param>
        /// <param name="decimalPrecision">WS Decimal Precision</param>
        /// <returns>Errors, if any</returns>
        private static Collection<string> ValidateTemplateMoqForLaborType(BoeTaskElementDTO task, int decimalPrecision)
        {
            Collection<string> errorMessages = new Collection<string>();

            task.taskElementLabors.Where(x => !x.MoqTypeSelectionId.HasValue).ForEach(resourceType =>
            {
                string resourceValue = resourceType.SpreadType == SpreadType.Cost ?
                            "$" + Utilities.AdjustPrecision(resourceType.ValueSpread.Value, 2).ToString()
                                : Utilities.AdjustPrecision(resourceType.ValueSpread.Value, decimalPrecision).ToString();

                errorMessages.Add(string.Format(Constants.MOQ_TYPE_REQUIRED_FOR_RESOURCE_TYPE, resourceType.StartDate, resourceType.EndDate, resourceValue));
            });

            return errorMessages;
        }

        /// <summary>
        /// Validates a required field
        /// </summary>
        /// <param name="moqType">Moq Type</param>
        /// <param name="field">Property to check</param>
        /// <param name="label">Label for the field</param>
        /// <param name="errorMessages">Error Messages</param>
        private static void ValidateRequiredField(MOQType moqType, string field, string label, Collection<string> errorMessages)
        {
            if (string.IsNullOrEmpty(field)) 
            { 
                errorMessages.Add($"{moqType.GetDescription()}: {label} is required."); 
            }
        }

        /// <summary>
        /// Validates Tasks Description, including RTE Templates in the process
        /// </summary>        
        private void ValidateTaskDescription(FullWorkspace workspace, FullBoe inBOE, Collection<string> TaskElementMessages, BoeTaskElementDTO boeTask)
        {
            if (string.IsNullOrEmpty(boeTask.Description)) // if no data in the field itself
            {
                // the field is valid if templates are being used, AND all required prompts are answered
                var taskTemplateWithPrompts = this.rteTemplateDataLoader.GetByBoeIdAndTaskId(workspace.Id, inBOE.Id, boeTask.Id).Where(t => t.SourceId == (int)RteTemplateSource.TaskDescription);
                if (!taskTemplateWithPrompts.Any() || taskTemplateWithPrompts.Any(t => t.Required && string.IsNullOrEmpty(t.AnswerText)))
                {
                    TaskElementMessages.Add(this.FormatMOQTextErrorMessage(BoeDTO.TASK_DESCRIPTION_REQUIRED));
                }
            }
        }

        private decimal ValidateResourceLabors(FullWorkspace workspace, FullBoe inBOE, ValidationBOETasks boeTasks, Collection<string> LaborTypeMessages, ICollection<OffloadRatesDTO> offloadRatesDtos, BoeTaskElementDTO boeTask, ICollection<ResourceDTO> resourcesFromTask)
        {
            decimal TotalLaborSpreadValue = 0;

            foreach (ResourceTypeDto labor in boeTask.taskElementLabors)
            {
                bool invalidCostSpreadPrecision = false;
                bool invalidHoursSpreadPrecision = false;
                ValidationBOELaborType boeLabor = new ValidationBOELaborType();
                LaborTypeMessages = new Collection<string>();

                // need to verify a Performing Org exists
                if (!labor.PerformingOrgID.HasValue)
                {
                    LaborTypeMessages.Add(BoeDTO.PERFORM_ORG_REQUIRED);
                }

                // need to verfy a Resource exists
                if (!labor.ResourceID.HasValue)
                {
                    LaborTypeMessages.Add(BoeDTO.RESOURCE_CODE_REQUIRED);
                }

                // BOEs marked as Multi must have a WBS and or CLIN assigned
                if (inBOE.IsMultiClinWbs && !labor.CLINID.HasValue && !labor.WBSID.HasValue)
                {
                    LaborTypeMessages.Add(BoeDTO.CLIN_WBS_REQUIRED);
                }

                // validate LT Start/End Date
                Collection<string> ReturnMsgs = this._ValidateStartAndEndDates(boeTask.StartDate, boeTask.EndDate, inBOE, labor.StartDate, labor.EndDate, workspace);
                foreach (string msg in ReturnMsgs)
                {
                    LaborTypeMessages.Add(msg);
                }

                // validate LT custom fields
                switch (boeTask.TaskElementType)
                {
                    case TaskElementType.Labor:
                        ReturnMsgs = this._ValidateLaborTypeCustomFields(workspace, labor);
                        foreach (string msg in ReturnMsgs)
                        {
                            LaborTypeMessages.Add(msg);
                        }

                        break;
                    case TaskElementType.None:
                    default:
                        break;
                }

                // Validate that all resource spreads in a task element must equal
                // the MOQ equation total
                if (labor.SpreadType == SpreadType.Hours)
                {
                    foreach (ResourceSpreadDto bls in labor.LaborSpreads)
                    {
                        TotalLaborSpreadValue += bls.LaborSpreadValue;

                        if (!ImportUtils.IsMaxDecimalPlaces(bls.LaborSpreadValue.ToString(), workspace.DecimalPrecision, out _))
                        {
                            invalidHoursSpreadPrecision = true;
                        }
                    }
                }
                else
                {
                    // cost
                    foreach (ResourceSpreadDto bls in labor.LaborSpreads)
                    {
                        if (!ImportUtils.IsMaxDecimalPlaces(bls.LaborSpreadValue.ToString(), workspace.CostDecimalPrecision, out _))
                        {
                            invalidCostSpreadPrecision = true;
                        }
                    }
                }

                if (workspace.ProjectMapType == ProjectMapType.StandardWithOffload)
                {
                    // if a Resource Type is set to try to Offload, Validate it and return findings
                    if (labor.CanOffload)
                    {
                        OffloadLaborRatesValidationResults validationResults = OffloadLaborRates.ValidateLaborResourceCanOffload(labor, offloadRatesDtos, workspace);
                        if (!validationResults.IsValid)
                        {
                            // add at resource level
                            LaborTypeMessages.Add(validationResults.InvalidWarningText);
                        }
                    }
                }

                if (invalidCostSpreadPrecision)
                {
                    LaborTypeMessages.Add("Cost Spreads have invalid precision which may cause rounding issues. If no error in UI, change the Workspace Cost Decimal Precision to another number then back to current number in Workspace Settings.");
                }

                if (invalidHoursSpreadPrecision)
                {
                    LaborTypeMessages.Add("Hour Spreads have invalid precision which may cause rounding issues. If no error in UI, change the Workspace Resource Hours Decimal Precision to another number (default is zero) then back to current number in Workspace Settings.");
                }

                if (LaborTypeMessages.Any())
                {
                    ResourceDTO resource = null;
                    if (labor.ResourceID.HasValue)
                    {
                        resource = resourcesFromTask.FirstOrDefault(x => x.Id == labor.ResourceID.Value);
                    }

                    if (resource == null)
                    {
                        boeLabor.LaborTypeHeader = "Resource Type:";
                    }
                    else
                    {
                        boeLabor.LaborTypeHeader = "Resource Type: " + (resource.ResourceName ?? string.Empty);
                    }

                    boeLabor.LaborTypeValidationMsgs = LaborTypeMessages;
                    boeTasks.LaborTypes.Add(boeLabor);
                }
            }

            return TotalLaborSpreadValue;
        }

        private void _ValidateODC(FullBoe inBOE, FullWorkspace ws, ValidationBOEModelView ValidationBOE, ref ValidationBOETasks odcTasks, ref Collection<string> OdcTaskElementMessages, ValidationBOELaborType odcType, ref Collection<string> OdcTypeMessages, ref decimal TotalLaborSpreadValue)
        {
            // Validate Cost
            var odcs = ws.Odcs.Where(x => x.BoeID == inBOE.Id);
            foreach (OtherDirectCostDTO odc in odcs)
            {
                odcTasks = new ValidationBOETasks();
                OdcTaskElementMessages = new Collection<string>();

                // Validate MOQ Text
                if (string.IsNullOrEmpty(odc.MoqText))
                {
                    OdcTaskElementMessages.Add(this.FormatMOQTextErrorMessage(OtherDirectCostDTO.MOQ_TEXT_REQUIRED));
                }

                // Validate ODC Types
                if (!odc.ODCTypes.Any())
                {
                    OdcTypeMessages.Add(OtherDirectCostDTO.AT_LEAST_ONE_ODC_TYPE_REQUIRED);

                    odcType.LaborTypeHeader = "ODC Type:";
                    odcType.LaborTypeValidationMsgs = OdcTypeMessages;
                    odcTasks.LaborTypes.Add(odcType);
                }
                // validate ODC Start/End Date

                Collection<string> returnMessages = new Collection<string>();
                returnMessages = this._ValidateStartAndEndDates(inBOE.StartDate, inBOE.EndDate, inBOE, odc.StartDate, odc.EndDate, ws);
                foreach (string msg in returnMessages)
                {
                    OdcTaskElementMessages.Add(msg);
                }

                TotalLaborSpreadValue = 0;
                var resourcesFromTask = ws.ResourcesForWsResourceListId.Where(x => (odc.ODCTypes.Where(y => y.ResourceID.HasValue).Select(z => z.ResourceID.Value)).Contains(x.Id));

                foreach (OtherDirectCostType type in odc.ODCTypes)
                {
                    OdcTypeMessages = new Collection<string>();
                    odcType = new ValidationBOELaborType();

                    // need to verify a Performing Org exists
                    if (!type.PerformingOrgID.HasValue)
                    {
                        OdcTypeMessages.Add(OtherDirectCostDTO.PERFORM_ORG_REQUIRED);
                    }
                    // need to verfy a Resource exists
                    if (!type.ResourceID.HasValue)
                    {
                        OdcTypeMessages.Add(OtherDirectCostDTO.RESOURCE_CODE_REQUIRED);
                    }

                    // validate LT Start/End Date
                    Collection<string> ReturnMsgs = this._ValidateStartAndEndDates(odc.StartDate, odc.EndDate, inBOE, type.StartDate, type.EndDate, ws);
                    foreach (string msg in ReturnMsgs)
                    {
                        OdcTypeMessages.Add(msg);
                    }

                    // Validate there is at least one odc spread
                    if (!type.ODCSpreads.Any())
                    {
                        OdcTypeMessages.Add(OtherDirectCostDTO.SPREAD_REQUIRED);
                    }

                    if (OdcTypeMessages.Any())
                    {
                        ResourceDTO resource = null;
                        if (type.ResourceID.HasValue)
                        {
                            resource = resourcesFromTask.FirstOrDefault(x => x.Id == type.ResourceID.Value);
                        }

                        if (resource == null)
                        {
                            odcType.LaborTypeHeader = "ODC Type: ";
                        }
                        else
                        {
                            odcType.LaborTypeHeader = "ODC Type: " + (resource.ResourceDesc ?? string.Empty);
                        }

                        odcType.LaborTypeValidationMsgs = OdcTypeMessages;
                        odcTasks.LaborTypes.Add(odcType);
                    }
                }

                if (OdcTaskElementMessages.Any() || odcTasks.LaborTypes.Any())
                {
                    odcTasks.TaskMessage = "Task: " + odc.TaskTitle;
                    odcTasks.TaskElementDetails.TaskElementDetailValidationMessages = OdcTaskElementMessages;
                    odcTasks.TaskElementDetails.TaskElementDetailsHeader = "Task Element Details";
                    // adds id to odc task with error this is for validateallboes method
                    odcTasks.TaskId = odc.Id;
                    ValidationBOE.Costs.Add(odcTasks);
                }
            }
        }

        /// <summary>
        /// This function will validate the Labor Type start and end date against the Parent start/end date,
        /// the CLIN start/date, and the contract start/end date
        /// </summary>
        /// <param name="boe">BOE</param>
        /// <param name="inStartDate">Start date to check is greater or equal to labor_or_odc/clin/contract start date</param>
        /// <param name="inEndDate">End date to check is less than or equal to labor_or_odc/clin/contract end date</param>
        /// <returns>error messages, if any found - empty collection otherwise</returns>
        private Collection<string> _ValidateStartAndEndDates(DateTime? inParentStartDate, DateTime? inParentEndDate, FullBoe boe, DateTime? inStartDate, DateTime? inEndDate, FullWorkspace ws)
        {
            Collection<string> LTValidationMsgs = new Collection<string>();
            string startDateAsString = string.Empty;
            string endDateAsString = string.Empty;
            bool startDateValid = true;
            bool endDateValid = true;

            if (!inStartDate.HasValue && !inEndDate.HasValue)
            {
                LTValidationMsgs.Add("Start Date for Resource Type required");
                LTValidationMsgs.Add("End Date for Resource Type required");
            }
            else if (!inStartDate.HasValue)
            {
                LTValidationMsgs.Add("Start Date for Resource Type required");
            }
            else if (!inEndDate.HasValue)
            {
                LTValidationMsgs.Add("End Date for Resource Type required");
            }

            if (LTValidationMsgs.Any())
            {
                return LTValidationMsgs;
            }
            // the Labor Type start date must be on or after the Parent Start Date. If the Parent Start Date
            // does not exist, then need to check the CLIN start date. If there is no CLIN start date,
            // need to check the contract start date
            if (!inParentStartDate.Equals(DateTime.MinValue))
            {
                startDateAsString = inParentStartDate.Value.ToString("MM/yyyy");

                if (!(GenBOEUtilities.AdjustDateTimePrecision(inStartDate.Value) >= GenBOEUtilities.AdjustDateTimePrecision(inParentStartDate.Value)))
                {
                    startDateValid = false;
                }
            }

            if (startDateValid)// need to check clin if the start date is still valid
            {
                ClinDTO clin = boe.Clin;

                if (clin != null && 
                         clin.StartDate.HasValue && !clin.StartDate.Equals(DateTime.MinValue))
                {
                    startDateAsString = clin.StartDate.Value.ToString("MM/yyyy");
                    if (!(GenBOEUtilities.AdjustDateTimePrecision(inStartDate.Value) >= GenBOEUtilities.AdjustDateTimePrecision(clin.StartDate.Value)))
                    {
                        startDateValid = false;
                    }
                }

                if (startDateValid) // if startDate is still valid, check contract
                {
                    if (!(GenBOEUtilities.AdjustDateTimePrecision(inStartDate.Value) >= GenBOEUtilities.AdjustDateTimePrecision(ws.ContractStartDate)))
                    {
                        startDateAsString = ws.ContractStartDate.ToString("MM/yyyy");
                        startDateValid = false;
                    }
                }
            }

            // the Labor Type end date must be on or before  the Parent end date. If the Parent end date
            // does not exist, then need to check the CLIN end date. If there is no CLIN end date,
            // need to check the contract end date
            if (!inParentEndDate.Equals(DateTime.MinValue))
            {
                endDateAsString = inParentEndDate.Value.ToString("MM/yyyy");

                if (!(GenBOEUtilities.AdjustDateTimePrecision(inEndDate.Value) <= GenBOEUtilities.AdjustDateTimePrecision(inParentEndDate.Value)))
                {
                    endDateValid = false;
                }
            }

            if (endDateValid) // need to check clin if the end date is still valid
            {
                ClinDTO clin = ws.Boes.First(x => x.Id == boe.Id).Clin;
                if (clin != null && 
                         clin.EndDate.HasValue && !clin.EndDate.Equals(DateTime.MinValue))
                {
                    endDateAsString = clin.EndDate.Value.ToString("MM/yyyy");
                    if (!(GenBOEUtilities.AdjustDateTimePrecision(inEndDate.Value) <= GenBOEUtilities.AdjustDateTimePrecision(clin.EndDate.Value)))
                    {
                        endDateValid = false;
                    }
                }

                if (endDateValid)// if endDate is still valid, check contract
                {
                    endDateAsString = ws.ContractEndDate.ToString("MM/yyyy");

                    if (!(GenBOEUtilities.AdjustDateTimePrecision(inEndDate.Value) <= GenBOEUtilities.AdjustDateTimePrecision(ws.ContractEndDate)))
                    {
                        endDateValid = false;
                    }
                }
            }

            if (!startDateValid)
            {
                LTValidationMsgs.Add(string.Format(START_DATE_INVALID, startDateAsString));
            }

            if (!endDateValid)
            {
                LTValidationMsgs.Add(string.Format(END_DATE_INVALID, endDateAsString));
            }

            if (startDateValid && endDateValid && inStartDate > inEndDate)
            {
                LTValidationMsgs.Add(DATE_RANGE_INVALID);
            }

            return LTValidationMsgs;
        }

        /// <summary>
        /// Compare any element of cost task start date against the boe start date
        /// </summary>
        /// <param name="inBOEStartDate">BOE start date</param>
        /// <param name="inTaskStartDate">element of cost start date</param>
        /// <returns>true if valid range, false if not</returns>
        private bool _ValidateTaskElementStartDateComparedToBOEStartDate(DateTime inBOEStartDate, DateTime inTaskStartDate)
        {
            bool isTaskDateInRange = true;

            if (GenBOEUtilities.AdjustDateTimePrecision(inBOEStartDate, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision(inTaskStartDate, DateTimePrecision.Month))
            {
                isTaskDateInRange = false;
            }

            return isTaskDateInRange;
        }

        /// <summary>
        /// Compare any element of cost task start date against the boe start date
        /// </summary>
        /// <param name="inBOEEndDate">BOE start date</param>
        /// <param name="inTaskEndDate">element of cost start date</param>
        /// <returns>true if valid range, false if not</returns>
        private bool _ValidateTaskElementEndDateComparedToBOEStartDate(DateTime inBOEEndDate, DateTime inTaskEndDate)
        {
            bool isTaskDateInRange = true;

            if (GenBOEUtilities.AdjustDateTimePrecision(inBOEEndDate, DateTimePrecision.Month) < GenBOEUtilities.AdjustDateTimePrecision(inTaskEndDate, DateTimePrecision.Month))
            {
                isTaskDateInRange = false;
            }

            return isTaskDateInRange;
        }

        /// <summary>
        /// Validate the travel trip date compared to the travel task date
        /// </summary>
        /// <param name="inTravelTask">travel task element</param>
        /// <param name="inTravelTrip">travel trip</param>
        /// <returns></returns>
        private Collection<string> _ValidateTravelTripDate(TravelDTO inTravelTask, TravelTripType inTravelTrip, FullWorkspace ws, FullBoe boe)
        {
            Collection<string> toReturn = new Collection<string>();
            // the date for the travel trip must be contained within the task start/end date
            if (!(GenBOEUtilities.AdjustDateTimePrecision(inTravelTrip.TripDate, DateTimePrecision.Month) < GenBOEUtilities.AdjustDateTimePrecision(inTravelTask.StartDate.Value, DateTimePrecision.Month)))
            {
                // the date for the travel trip must be contained within the boe start/end date
                if (!(GenBOEUtilities.AdjustDateTimePrecision(inTravelTrip.TripDate, DateTimePrecision.Month) < GenBOEUtilities.AdjustDateTimePrecision(boe.StartDate, DateTimePrecision.Month)))
                {
                    // the date for the travel trip must be contained within the workspace start/end date
                    if (GenBOEUtilities.AdjustDateTimePrecision(inTravelTrip.TripDate, DateTimePrecision.Month) < GenBOEUtilities.AdjustDateTimePrecision(ws.ContractStartDate, DateTimePrecision.Month))
                    {
                        toReturn.Add(string.Format(TravelDTO.TRIP_START_DATE_INVALID, "Workpace", ws.ContractStartDate.ToString("MM/yyyy")));
                    }
                }
                else
                {
                    toReturn.Add(string.Format(TravelDTO.TRIP_START_DATE_INVALID, "BOE", boe.StartDate.ToString("MM/yyyy")));
                }
            }
            else
            {
                toReturn.Add(string.Format(TravelDTO.TRIP_START_DATE_INVALID, "Task", inTravelTask.StartDate.Value.ToString("MM/yyyy")));

            }

            // adjust the date by adding the trip duration to the end date.
            DateTime endDateWithTripDurationAdded = inTravelTrip.TripDate.AddDays(inTravelTrip.NumOfDays);
            endDateWithTripDurationAdded = new DateTime(endDateWithTripDurationAdded.Year, endDateWithTripDurationAdded.Month, 1);
            if (!(GenBOEUtilities.AdjustDateTimePrecision(endDateWithTripDurationAdded, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision(inTravelTask.EndDate.Value, DateTimePrecision.Month)))
            {
                // check end date on boe.
                if (!(GenBOEUtilities.AdjustDateTimePrecision(endDateWithTripDurationAdded, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision(boe.EndDate, DateTimePrecision.Month)))
                {
                    // check end date on workspace.
                    if (GenBOEUtilities.AdjustDateTimePrecision(endDateWithTripDurationAdded, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision(ws.ContractEndDate, DateTimePrecision.Month))
                    {
                        toReturn.Add(string.Format(TravelDTO.TRIP_END_DATE_INVALID, "Workspace", ws.ContractEndDate.ToString("MM/yyyy")));
                    }
                }
                else
                {
                    toReturn.Add(string.Format(TravelDTO.TRIP_END_DATE_INVALID, "BOE", boe.EndDate.ToString("MM/yyyy")));
                }
            }
            else
            {
                toReturn.Add(string.Format(TravelDTO.TRIP_END_DATE_INVALID, "Task", inTravelTask.EndDate.Value.ToString("MM/yyyy")));
            }

            return toReturn;
        }

        /// <summary>
        /// Validate the travel trip segment compared to the workspace segment
        /// </summary>
        /// <param name="inTravelTrip">travel trip</param>
        /// <returns></returns>
        private bool _ValidateTravelTripSegment(TravelTripType inTravelTrip, FullWorkspace ws)
        {
            bool isTravelTripSegmentValid = true;

            var travelResources = ws.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.Travel);
            Collection<int> wsTravelSegmentIDs = new Collection<int>();

            foreach (ResourceDTO travelResource in travelResources)
            {
                int tempID = (int)travelResource.Segment;
                wsTravelSegmentIDs.Add(tempID);
            }

            if (!wsTravelSegmentIDs.Contains((int)inTravelTrip.Segment))
            {
                isTravelTripSegmentValid = false;
            }

            return isTravelTripSegmentValid;
        }

        /// <summary>
        /// Validate the travel trip performing org
        /// </summary>
        /// <param name="inTravelTrip">travel trip</param>
        /// <returns></returns>
        private bool _ValidateTravelTripPerformingOrg(TravelTripType inTravelTrip)
        {
            bool isTravelTripPerfOrgValid = true;

            if (inTravelTrip.PerfOrgID <= 0)
            {
                isTravelTripPerfOrgValid = false;
            }

            return isTravelTripPerfOrgValid;
        }

        /// <summary>
        /// This function will validate the BOE Task Element Custom Fields
        /// </summary>
        /// <param name="workspace">Workspace</param>
        /// <param name="inBoeTaskElement">the BOE Task Element</param>
        /// <returns>Custom field validation errors.</returns>
        private Collection<string> _ValidateTaskCustomFields(FullWorkspace workspace, BoeTaskElementDTO inBoeTaskElement)
        {
            Collection<string> TaskCustomFieldMsgs = new Collection<string>();

            // need to find out if the boe task element has any custom fields
            // if it does, then need to determine if the custom field is required
            // and if it is required, then need to make sure the custom field has a value
            foreach (CustomFieldDTO customField in workspace.CustomFields)
            {
                // if it's a task custom field and required, check to see if the task element has a value
                if (customField.CustomFieldRequired && customField.CustomFieldDisplayID == CustomFieldType.TaskDisplay)
                {
                    // gets all the custom field options
                    List<int> customFieldOptions = workspace.CustomFieldValues.Where(x => x.CustomFieldID == customField.Id).Select(x => x.CustomFieldValueID).ToList();

                    // gets all the selected options for an element 
                    List<int> selectedCustomFieldOptions =
                    (from mappedValues in workspace.TaskElementsMappingWithCustomFieldsValuesAndContainerIds
                        where mappedValues.Key == inBoeTaskElement.Id
                        select mappedValues.Value).SelectMany(y => y).Select(c => c.Value).ToList();
                    // if the options contains one of the selected values then its assigned else it needs to be.
                    if (!customFieldOptions.Intersect(selectedCustomFieldOptions).Any())
                    {
                        TaskCustomFieldMsgs.Add(string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, customField.CustomFieldName));
                    }
                }
            }

            return TaskCustomFieldMsgs;
        }

        /// <summary>
        /// This function will validate the BOE Labor Type Custom Fields
        /// </summary>
        /// <param name="workspace">Full Workspace.</param>
        /// <param name="inBoeLaborType">the BOE Labor Type</param>
        /// <returns></returns>
        private Collection<string> _ValidateLaborTypeCustomFields(FullWorkspace workspace, ResourceTypeDto inBoeLaborType)
        {
            Collection<string> LTCustomFieldMsgs = new Collection<string>();

            // need to find out if the boe labor type has any custom fields
            // if it does, then need to determine if the custom field is required
            // and if it is required, then need to make sure the custom field has a value
            foreach (CustomFieldDTO customField in workspace.CustomFields)
            {
                // if it's a labor type custom field and required, check to see if the labor type has a value
                if (customField.CustomFieldRequired && customField.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay)
                {
                    // gets all the custom field options
                    List<int> customFieldOptions = workspace.CustomFieldValues.Where(x => x.CustomFieldID == customField.Id).Select(x => x.CustomFieldValueID).ToList();

                    // gets all the selected options for an element 
                    List<int> selectedCustomFieldOptions =
                    (from mappedValues in workspace.LaborTypesMappingWithCustomFieldsValuesAndContainerIds
                        where mappedValues.Key == inBoeLaborType.Id
                        select mappedValues.Value).SelectMany(y => y).Select(c => c.Value).ToList();
                    // if the options contains one of the selected values then its assigned else it needs to be.
                    if (!customFieldOptions.Intersect(selectedCustomFieldOptions).Any())
                    {
                        LTCustomFieldMsgs.Add(string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, customField.CustomFieldName));
                    }
                }
            }

            return LTCustomFieldMsgs;
        }

        /// <summary>
        /// This function will validate the Travel Task Element Custom Fields
        /// </summary>
        /// <param name="workspace">Workspace</param>
        /// <param name="inTravelTaskElement">the Travel Task Element</param>
        /// <returns>Custom field validation errors.</returns>
        private Collection<string> _ValidateTravelTaskCustomFields(FullWorkspace workspace, TravelDTO inTravelTaskElement)
        {
            Collection<string> TravelTaskCustomFieldMsgs = new Collection<string>();
            
            // need to find out if the travel task element has any custom fields
            // if it does, then need to determine if the custom field is required
            // and if it is required, then need to make sure the custom field has a value
            foreach (CustomFieldDTO customField in workspace.CustomFields)
            {
                // if it's a task custom field and required, check to see if the task element has a value
                if (customField.CustomFieldRequired && customField.CustomFieldDisplayID == CustomFieldType.TaskDisplay)
                {
                    // gets all the custom field options
                    List<int> customFieldOptions = workspace.CustomFieldValues.Where(x => x.CustomFieldID == customField.Id).Select(x=>x.CustomFieldValueID).ToList();
                 
                    // gets all the selected options for an element 
                   List<int> selectedCustomFieldOptions = (from mappedValues in workspace.TravelElementsMappingWithCustomFieldsValuesAndContainerIds
                                      where mappedValues.Key == inTravelTaskElement.Id
                                      select mappedValues.Value).SelectMany(y => y).Select(c => c.Value).ToList();
                    // if the options contains one of the selected values then its assigned else it needs to be.
                    if (!customFieldOptions.Intersect(selectedCustomFieldOptions).Any())
                    {
                        TravelTaskCustomFieldMsgs.Add(string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, customField.CustomFieldName));
                    }
                }
            }

            return TravelTaskCustomFieldMsgs;
        }

        /// <summary>
        /// This function will validate the Travel Trip Custom Fields
        /// </summary>
        /// <param name="workspace">Full Workspace.</param>
        /// <param name="inTravelTrip">the Travel Trip</param>
        /// <returns></returns>
        private Collection<string> _ValidateTravelTripCustomFields(FullWorkspace workspace, TravelTripType inTravelTrip)
        {
            Collection<string> TravelTripCustomFieldMsgs = new Collection<string>();
            
            // need to find out if the boe TravelTrip has any custom fields
            // if it does, then need to determine if the custom field is required
            // and if it is required, then need to make sure the custom field has a value
            foreach (CustomFieldDTO customField in workspace.CustomFields)
            {
                // if it's a travel trip custom field and required, check to see if it has a value
                if (customField.CustomFieldRequired && customField.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay)
                {
                    // gets all the custom field options
                    List<int> customFieldOptions = workspace.CustomFieldValues.Where(x => x.CustomFieldID == customField.Id).Select(x => x.CustomFieldValueID).ToList();

                    // gets all the selected options for an element 
                    List<int> selectedCustomFieldOptions = (from mappedValues in workspace.TravelTripsMappingWithCustomFieldsValuesAndContainerIds
                                                            where mappedValues.Key == inTravelTrip.Id
                                                            select mappedValues.Value).SelectMany(y => y).Select(c => c.Value).ToList();
                    // if the options contains one of the selected values then its assigned else it needs to be.
                    if (!customFieldOptions.Intersect(selectedCustomFieldOptions).Any())
                    {
                        TravelTripCustomFieldMsgs.Add(string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, customField.CustomFieldName));
                    }
                }
            }

            return TravelTripCustomFieldMsgs;
        }

        /// <summary>
        /// Determines whether Description is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if description is valid; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsDescriptionValid(BoeDTO boe, int wsId)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            bool valid = !string.IsNullOrEmpty(boe.Description);
            if(!valid) // no data in the field itself
            {
                // the field is valid if templates are being used, AND all required prompts are answered
                var boeTemplateWithPrompts = this.rteTemplateDataLoader.GetByBoeId(wsId, boe.Id).Where(t => t.SourceId == (int)RteTemplateSource.BoeDescription);
                valid = boeTemplateWithPrompts.Any() && !boeTemplateWithPrompts.Any(t => t.Required && string.IsNullOrEmpty(t.AnswerText));
            }

            return valid;
        }

        /// <summary>
        /// Tests if the Source of Data field is valid
        /// </summary>
        /// <returns>True if valid false otherwise</returns>
        protected virtual bool IsSourcesOfDataValid(BoeDTO boe, int wsId)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }
            
            bool valid = !string.IsNullOrEmpty(boe.DataSource);
            if (!valid) // no data in the field itself
            {
                // the field is valid if templates are being used, AND all required prompts are answered
                var boeTemplateWithPrompts = this.rteTemplateDataLoader.GetByBoeId(wsId, boe.Id).Where(t => t.SourceId == (int)RteTemplateSource.BoeSources);
                valid = boeTemplateWithPrompts.Any() && !boeTemplateWithPrompts.Any(t => t.Required && string.IsNullOrEmpty(t.AnswerText));
            }


            return valid;
        }

        /// <summary>
        /// This function will validate the BOE Custom Fields.
        /// </summary>
        /// <param name="workspace">The Workspace used during validation.</param>
        /// <param name="boe">The BOE used to validate against.</param>
        /// <returns>A collection of validation messages.</returns>
        protected virtual Collection<string> ValidateBOECustomFields(FullWorkspace workspace, FullBoe boe)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            Collection<string> BOECustomFieldMsgs = new Collection<string>();
            
            // need to find out if the boe  has any custom fields
            // if it does, then need to determine if the custom field is required
            // and if it is required, then need to make sure the custom field has a value
            foreach (CustomFieldDTO customField in workspace.CustomFields)
            {
                // if it's a labor type custom field and required, check to see if the labor type has a value
                if (customField.CustomFieldRequired && customField.CustomFieldDisplayID == CustomFieldType.BoeDisplay)
                {
                    bool exists = boe.DoesBoeExistGivenCustomFieldId(customField.Id);
                    if (!exists)
                    {
                        BOECustomFieldMsgs.Add(string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, customField.CustomFieldName));
                    }
                }
            }

            return BOECustomFieldMsgs;
        }

        /// <summary>
        /// Adds the company specific MOQ Text label to the Error string
        /// </summary>
        /// <param name="MOQTextErrorMessage">The error text to add the MOQ label to</param>
        /// <returns>The formatted <see cref="string"/></returns>
        protected virtual string FormatMOQTextErrorMessage(string MOQTextErrorMessage)
        {
            return string.Format(MOQTextErrorMessage, CommonConstants.BOE_MOQ_TEXT_LABEL);
        }

        /// <summary>
        /// Return a <see cref="bool"/> indicating if the historic metric disclosure is required
        /// </summary>
        /// <param name="boeTaskIds">The id's of the task elements to retrieve metrics</param>
        /// <param name="boe">the boe containing the flag indicating if the historic metric disclosure is required</param>
        /// <returns>required if true, not required otherwise</returns>
        protected virtual bool IsHistoricMetricDisclosureRequired(ICollection<int> boeTaskIds, FullBoe boe)
        {
           // Metrics deprecated for SSC.
           return false;
        }

        /// <summary>
        /// Validates RTE Field length
        /// </summary>
        /// <param name="workspace">Workspace</param>
        /// <param name="fieldValue">Field value to check</param>
        /// <param name="fieldName">Field name for error message</param>
        /// <param name="errors">Errors to which we'll add errors</param>
        private static void ValidateRTEFieldLength(FullWorkspace workspace, string fieldValue, string fieldName, ICollection<string> errors)
        {
            if (workspace.RteSizeLimit.HasValue && workspace.RteSizeLimit < GenBOEUtilities.ConvertHtmlToText(fieldValue ?? string.Empty).Length)
            {
                errors.Add($"The maximum length of {fieldName} is {workspace.RteSizeLimit.Value} characters.");
            }
        }
    }
}
