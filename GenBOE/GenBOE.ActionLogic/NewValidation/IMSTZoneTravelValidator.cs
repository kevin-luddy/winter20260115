// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.NewValidation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common.Exceptions;
    public interface IMSTZoneTravelValidator
    {
        /// <summary>
        /// Validates the details of the MST Travel Task
        /// </summary>
        /// <param name="travelDTO">DTO with Travel Task Details</param>
        /// <param name="boeID">ID of BOE containing Travel Task</param>
        /// <returns>Collection of any validation messages found. If there are no errors, a blank collection is returned.</returns>
        Collection<ValidationMessage> ValidateTravelTaskDetails(TravelDTO travelDTO, int boeID);

        /// <summary>
        /// Validates the Trips of a Travel Task
        /// </summary>
        /// <param name="travelTrips">Collection of Trips for the Travel Task</param>
        /// <param name="taskStartDate">Travel Task start date</param>
        /// <param name="taskEndDate">Travel Task end date</param>
        /// <param name="boeID">ID of BOE containing the Travel Task/Trips</param>
        /// <param name="escalationRateYears">Escalation rate years (used for validation)</param>
        /// <param name="includeTripDetails">Bool to note if trip-specific details are needed for the validation messages</param>
        /// <param name="container">Validation Box container id</param>
        /// <returns>Collection of any validation messages found. If there are no errors, a blank collection is returned.</returns>
        Collection<ValidationMessage> ValidateTravelTrips(ICollection<MSTTravelTripType> travelTrips, DateTime? taskStartDate, DateTime? taskEndDate, int boeID, ICollection<int> escalationRateYears, bool includeTripDetails, string container );

        /// <summary>
        /// Validate the dates of the additional trips created when multiple occurrences are selected for a trip
        /// </summary>
        /// <param name="inTravelTrips">Trips to validate (additional occurrences, not including initial trip)</param>
        /// <param name="taskStartDate">Task Start Date</param>
        /// <param name="taskEndDate">Task End Date</param>
        /// <param name="container">Validation Box container</param>
        /// <returns>Validation message if there is one ore more trips outside of task dates, or null if there are no issues</returns>
        ICollection<ValidationMessage> ValidateTripDateForMultipleOccurrences(Collection<MSTTravelTripType> inTravelTrips, DateTime taskStartDate, DateTime taskEndDate, string container);
    }
}
