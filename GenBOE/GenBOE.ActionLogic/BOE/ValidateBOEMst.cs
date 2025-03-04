// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WBS.BOE
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.NewValidation;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;

    public class ValidateBOEMst : ValidateBOE
    {
        private IMSTZoneTravelValidator mstZoneTravelValidator;
        private RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader;


        /// <summary>
        /// Default constructor
        /// </summary>
        public ValidateBOEMst(
            IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
            BOECommentsResponsesValidator inBOECommentsResponsesValidator,
            ITripDTODataLoader inTripDTODataLoader,
            IMiscTravelRateDTOLoader inMiscTravelRateDTOLoader,
            ILocationDTODataLoader inLocationDTODataLoader,
            IMSTZoneTravelValidator mstZoneTravelValidator,
            RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader,
            IOffloadRatesDTOLoader offloadRatesDTOLoader,
            IRteTemplateDataLoader rteTemplateDataLoader)
            : base(
            inVariableSelectBOEtoSumCalculation,
            inBOECommentsResponsesValidator, 
            inTripDTODataLoader, 
            inMiscTravelRateDTOLoader, 
            inLocationDTODataLoader,
            offloadRatesDTOLoader,
            rteTemplateDataLoader)
        {
            this.mstZoneTravelValidator = mstZoneTravelValidator;
            this.zoneTravelRatesFeesLoader = zoneTravelRatesFeesLoader;
        }

        public override ValidationBOEModelView ValidateBOE_OnValidateBtnClick(FullBoe inBOE, FullWorkspace ws)
        {
            ValidationBOEModelView toReturn = base.ValidateBOE_OnValidateBtnClick(inBOE, ws);

            // validate BOE title
            if (String.IsNullOrEmpty(inBOE.Title))
            {
                toReturn.BOEHeaderMsgs.Add(BoeDTO.BOE_TITLE_REQUIRED);
            }

            return toReturn;
        }

        /// <summary>
        /// Adds the company specific MOQ Text label to the Error string
        /// </summary>
        /// <param name="MOQTextErrorMessage">The error text to add the MOQ label to</param>
        /// <returns>The formatted <see cref="String"/></returns>
        protected override string FormatMOQTextErrorMessage(string MOQTextErrorMessage)
        {
            return String.Format(MOQTextErrorMessage, CommonConstants.BOE_MOQ_TEXT_LABEL_SPACE_SYSTEMS);
        }

        /// <summary>
        /// Tests if the Source of Data field is valid for MST
        /// </summary>
        /// <param name="sourcesOfData">The string to test</param>
        /// <returns>Always returns True</returns>
        protected override bool IsSourcesOfDataValid(BoeDTO boe, int wsId)
        {
            // always return true since the field isn't required
            return true;
        }

        /// <summary>
        /// Validate MST Travel Task
        /// </summary>
        /// <param name="inBOE">BOE containing Travel</param>
        /// <param name="ws">Workspace containing BOE/Travel</param>
        /// <param name="ValidationBOE">Validation BOE Model View</param>
        /// <param name="travelTasks">Validation BOE Tasks for the Travel Task</param>
        /// <param name="travelTaskElementMessages">Collection of Travel Task Element Messages</param>
        /// <param name="TravelTypeMessages">Collection of Travel Type Messages</param>
        /// <param name="travelType">Validation BOE Labor Type for the Travel Type</param>
        /// <param name="escalationRateYears">Escalation Rate Years</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
        protected override void _ValidateTravel(FullBoe inBOE, FullWorkspace ws, ValidationBOEModelView ValidationBOE, ref ValidationBOETasks travelTasks, 
            ref Collection<string> travelTaskElementMessages, ref Collection<string> TravelTypeMessages, ValidationBOELaborType travelType)
        {
            if (ws == null) { throw new ArgumentNullException(nameof(ws)); }
            if (ValidationBOE == null) { throw new ArgumentNullException(nameof(ValidationBOE)); }
            if (travelTasks == null) { throw new ArgumentNullException(nameof(travelTasks)); }
            if (travelTaskElementMessages == null) { throw new ArgumentNullException(nameof(travelTaskElementMessages)); }
            if (TravelTypeMessages == null) { throw new ArgumentNullException(nameof(TravelTypeMessages)); }

			// validate the travel Tasks
			IEnumerable<TravelDTO> travelTaskElements = ws.Travels.Where(x => x.BoeID == inBOE.Id);

            List<int> escalationRateYears = this.zoneTravelRatesFeesLoader.getAllEscalationRatesByWorkspace(ws.Id).Select(x => x.Year).Distinct().ToList();


            foreach (TravelDTO travelTask in travelTaskElements)
            {
                travelTasks = new ValidationBOETasks();
                travelTaskElementMessages = new Collection<string>();

                Collection<ValidationMessage> taskDetailsValidationMessages = this.mstZoneTravelValidator.ValidateTravelTaskDetails(travelTask, inBOE.Id);
                foreach (ValidationMessage message in taskDetailsValidationMessages)
                {
                    travelTaskElementMessages.Add(message.ValidationIssue);
                }

                // TODO - add in custom field validation when MST Travel Custom Field Validator is created
                Collection<string> travelTaskCustomFieldMessages = new Collection<string>();
                if (travelTaskCustomFieldMessages.Any())
                {
                    foreach (string message in travelTaskCustomFieldMessages)
                    {
                        travelTaskElementMessages.Add(message);
                    }
                }

                foreach (MSTTravelTripType msttravel in travelTask.MSTTravelTrips)
                {
                    TravelTypeMessages = new Collection<string>();
                    travelType = new ValidationBOELaborType();

                    Collection<ValidationMessage> tripValidationMessages = this.mstZoneTravelValidator.ValidateTravelTrips(new Collection<MSTTravelTripType>() { msttravel }, travelTask.StartDate, travelTask.EndDate, inBOE.Id, escalationRateYears, false, string.Empty);
                    foreach (ValidationMessage message in tripValidationMessages)
                    {
                        TravelTypeMessages.Add(message.ValidationIssue);
                    }

                    // TODO - add in custom field validation when MST Travel Custom Field Validator is created
                    Collection<string> CustomFieldMessages = new Collection<string>();
                    if (CustomFieldMessages.Any())
                    {
                        foreach (string message in CustomFieldMessages)
                        {
                            TravelTypeMessages.Add(message);
                        }
                    }

                    if (TravelTypeMessages.Any())
                    {
                        bool isZone = (msttravel.ModeID == MSTTravelMode.ZoneNoAirfare || msttravel.ModeID == MSTTravelMode.ZoneAirfare);
                        string departureName = isZone ? msttravel.ZoneOriginName : msttravel.NonZoneFrom;
                        string destinationName = isZone ? msttravel.ZoneDestinationName : msttravel.NonZoneTo;

                        travelType.LaborTypeHeader = string.Format("Trip: {0} {1} {2} to {3}", msttravel.Id, msttravel.ModeID.GetDescription(), departureName, destinationName);
                        travelType.LaborTypeValidationMsgs = TravelTypeMessages;
                        travelTasks.LaborTypes.Add(travelType);
                    }
                }

                if (travelTaskElementMessages.Any() || travelTasks.LaborTypes.Any())
                {
                    travelTasks.TaskMessage = "Task: " + travelTask.TaskID + " " + travelTask.TaskTitle;

                    if (travelTaskElementMessages.Any())
                    {
                        travelTasks.TaskElementDetails.TaskElementDetailValidationMessages = travelTaskElementMessages;
                        travelTasks.TaskElementDetails.TaskElementDetailsHeader = "Task Element Details";
                    }
                    //sets the id for the task that has errors.
                    travelTasks.TaskId = travelTask.Id;
                    ValidationBOE.Travels.Add(travelTasks);
                }
            }
        }
    }
}
