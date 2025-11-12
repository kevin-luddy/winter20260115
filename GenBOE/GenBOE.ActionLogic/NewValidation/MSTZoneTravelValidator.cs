// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.NewValidation
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using DataBridge.DTO;
	using IES.Common;
	using IES.Common.Exceptions;

	public class MSTZoneTravelValidator : IMSTZoneTravelValidator
	{
		private IFullObjectFactory factory;
		private ITravelTripCustomFieldValueXREFLoader travelTripCustomFieldValueXREFDataLoader;

		/// <summary>
		/// Constructor
		/// </summary>
		public MSTZoneTravelValidator(IFullObjectFactory factory, ITravelTripCustomFieldValueXREFLoader travelTripCustomFieldValueXREFDataLoader)
		{
			this.factory = factory;
			this.travelTripCustomFieldValueXREFDataLoader = travelTripCustomFieldValueXREFDataLoader;
		}

		/// <summary>
		/// Validates the details of the MST Travel Task
		/// </summary>
		/// <param name="travelDTO">DTO with Travel Task Details</param>
		/// <param name="boeID">ID of BOE containing Travel Task</param>
		/// <returns>Collection of any validation messages found. If there are no errors, a blank collection is returned.</returns>
		public Collection<ValidationMessage> ValidateTravelTaskDetails(TravelDTO travelDTO, int boeID)
		{
			if (travelDTO == null)
			{
				throw new ArgumentNullException(nameof(travelDTO));
			}
			Collection<ValidationMessage> toReturn = new Collection<ValidationMessage>();

			// perform date range validation
			List<string> validationerrors = new List<string>();

			// first validate the travelDTOs
			BoeDTO boeDTO = this.factory.CreateFullBoe(boeID);
			StartEndDateTypeValidator validator = new StartEndDateTypeValidator(boeDTO.StartDate, boeDTO.EndDate, "BOE", "Task", true);
			ICollection<IStartEndDates> toValidate = new List<IStartEndDates> { travelDTO };
			validationerrors.AddRange(validator.validation(toValidate, (Collection<Dictionary<string, string>>)null));

			// gather errors up, if any
			foreach (string error in validationerrors)
			{
				toReturn.Add(new ValidationMessage("TaskElements", error));
			}

			Collection<Dictionary<string, string>> travelTaskDict = new Collection<Dictionary<string, string>>();
			travelTaskDict.Add(new Dictionary<string, string>());
			travelTaskDict.First<Dictionary<string, string>>().Add("BOEID", boeID.ToString());
			travelTaskDict.First<Dictionary<string, string>>().Add("TaskID", travelDTO.TaskID);
			travelTaskDict.First<Dictionary<string, string>>().Add("TravelID", travelDTO.Id.ToString());

			Validator uniqueIdValidator = ValidationFactory.Instance.getValidator(ValidationType.BoeTaskIDUnique);
			if (uniqueIdValidator.validation(travelDTO.TaskID, travelTaskDict).Any())
			{
				toReturn.Add(new ValidationMessage("TaskID", "The Task ID must be unique among Labor, Travel and ODC task elements."));
			}

			FullWorkspace ws = this.factory.CreateFullWorkspace(boeDTO.WorkspaceID);
			List<CustomFieldDTO> requiredCustomFields = ws.CustomFields.Where(cf => cf.CustomFieldDisplayID == CustomFieldType.TaskDisplay && cf.CustomFieldRequired).ToList();
			List<int> customFieldIdsPresent = new List<int>();
			ICollection<CustomFieldValueContainer> cfValueContainers = travelDTO.CustomFieldValueContainers
				.Where(x => !(x.IsOpenEnded && string.IsNullOrEmpty(x.OpenEndedValue))).ToCollection();

			foreach (CustomFieldValueContainer cfContainer in cfValueContainers)
			{
				if (cfContainer.CustomFieldID > 0)
				{
					// if we already have custom field id
					customFieldIdsPresent.Add(cfContainer.CustomFieldID);
				}
				else
				{
					// else we need to get it 
					customFieldIdsPresent.Add(this.travelTripCustomFieldValueXREFDataLoader.GetCustomFieldID(cfContainer.CustomFieldValueID));
				}
			}
			foreach (CustomFieldDTO customField in requiredCustomFields) // foreach customfield id required in travel dto
			{
				if (!customFieldIdsPresent.Contains(customField.Id)) // required custom field not found
				{
					toReturn.Add(new ValidationMessage(customField.CustomFieldName, string.Format(Constants.CUSTOM_FIELD_IS_REQUIRED, customField.CustomFieldName)));
				}
			}

			return toReturn;
		}


		/// <summary>
		/// Validates the Trips of a Travel Task
		/// </summary>
		/// <param name="travelTrips">Collection of Trips for the Travel Task</param>
		/// <param name="taskStartDate">Travel Task start date</param>
		/// <param name="taskEndDate">Travel Task end date</param>
		/// <param name="boeID">boe id.</param>
		/// <param name="escalationRateYears">Escalation rate years (used for validation)</param>
		/// <param name="includeTripDetails">Bool to note if trip-specific details are needed for the validation messages</param>
		/// <param name="container">Validation Box container id</param>
		/// <returns>Collection of any validation messages found. If there are no errors, a blank collection is returned.</returns>
		public Collection<ValidationMessage> ValidateTravelTrips(
			ICollection<MSTTravelTripType> travelTrips,
			DateTime? taskStartDate,
			DateTime? taskEndDate,
			int boeID,
			ICollection<int> escalationRateYears,
			bool includeTripDetails,
			string container)
		{
			FullBoe boe = this.factory.CreateFullBoe(boeID);
			if (travelTrips == null)
			{
				throw new ArgumentNullException(nameof(travelTrips));
			}
			if (escalationRateYears == null)
			{
				throw new ArgumentNullException(nameof(escalationRateYears));
			}
			Collection<ValidationMessage> toReturn = new Collection<ValidationMessage>();

			FullWorkspace ws = this.factory.CreateFullWorkspace(boe.WorkspaceID);
			List<CustomFieldDTO> requiredCustomFields = ws.CustomFields.Where(cf => cf.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay && cf.CustomFieldRequired).ToList();

			foreach (MSTTravelTripType trip in travelTrips)
			{
				if (trip.Updateable != UpdateType.Deleted)
				{
					bool zone = (trip.ModeID == MSTTravelMode.ZoneNoAirfare || trip.ModeID == MSTTravelMode.ZoneAirfare);
					bool nonzone = (trip.ModeID == MSTTravelMode.NonZoneDomestic || trip.ModeID == MSTTravelMode.NonZoneInternational);
					List<int> customFieldIdsPresent = new List<int>();
					string tripDetails = string.Empty;
					ICollection<CustomFieldValueContainer> cfValueContainers = trip.CustomFieldValueContainers
						.Where(x => !(x.IsOpenEnded && string.IsNullOrEmpty(x.OpenEndedValue))).ToCollection();

					foreach (CustomFieldValueContainer cfContainer in cfValueContainers)
					{
						if (cfContainer.CustomFieldID > 0)
						{
							// if we already have custom field id
							customFieldIdsPresent.Add(cfContainer.CustomFieldID);
						}
						else
						{
							// else we need to get it
							customFieldIdsPresent.Add(this.travelTripCustomFieldValueXREFDataLoader.GetCustomFieldID(cfContainer.CustomFieldValueID));
						}
					}
					foreach (CustomFieldDTO customField in requiredCustomFields) // foreach customfield id required in travel trip
					{
						if (!customFieldIdsPresent.Contains(customField.Id)) // required custom field not found
						{
							toReturn.Add(new ValidationMessage(customField.CustomFieldName, string.Format("Custom Field {0} is required. " + tripDetails, customField.CustomFieldName), container));
						}
					}

					if (includeTripDetails)
					{
						tripDetails = this.CreateTripDetailsString(trip, zone);
					}

					if (!escalationRateYears.Contains(trip.TripDate.Year))
					{
						toReturn.Add(new ValidationMessage("Trip Date", "The escalation rates are not setup for the year in which this trip is scheduled. Please contact the system admin. " + tripDetails, container));
					}

					if (!escalationRateYears.Contains(trip.EstimateDate.Year))
					{
						toReturn.Add(new ValidationMessage("Estimate Date", "The escalation rates are not setup for the year in which this trip was estimated. Please contact the system admin. " + tripDetails, container));
					}

					//Validate Shared Fields
					if (trip.ModeID == MSTTravelMode.None)
					{
						toReturn.Add(new ValidationMessage("Mode", "Mode is required. " + tripDetails, container));
					}

					if (trip.GroupID.HasValue)
					{
						if (trip.GroupID < 1 || trip.GroupID > 9999)
						{
							toReturn.Add(new ValidationMessage("ID", "ID must be between 1 and 9999. " + tripDetails, container));
						}
					}

					if (trip.PerfOrgID <= 0)
					{
						toReturn.Add(new ValidationMessage("perfOrg", "Performing Org is required. " + tripDetails, container));
					}

					if (string.IsNullOrEmpty(trip.Purpose))
					{
						toReturn.Add(new ValidationMessage("Purpose", "Purpose is required. " + tripDetails, container));
					}

					this.CheckTravelAndTravelTripDates(toReturn, taskStartDate, taskEndDate, trip, boe, includeTripDetails, container);

					if (!trip.NumOfPeople.HasValue || trip.NumOfPeople < -999 || trip.NumOfPeople > 999 || trip.NumOfPeople == 0)
					{
						toReturn.Add(new ValidationMessage("NumOfPeople", "# People must be between -999 and -1 or between 1 and 999. " + tripDetails, container));
					}

					if (!trip.NumOfDays.HasValue || trip.NumOfDays < -999 || trip.NumOfDays > 999 || trip.NumOfDays == 0)
					{
						toReturn.Add(new ValidationMessage("NumOfDays", "# Days must be between -999 and -1 or between 1 and 999. " + tripDetails, container));
					}

					//Validate Zone-only fields
					if (zone)
					{
						if (trip.ZoneOriginID == null || trip.ZoneOriginID < 0)
						{
							toReturn.Add(new ValidationMessage("Origin", "Origin is required. " + tripDetails, container));
						}

						if (string.IsNullOrEmpty(trip.ZoneDestCity))
						{
							toReturn.Add(new ValidationMessage("DestinationCity", "Destination City is required. " + tripDetails, container));
						}

						if (trip.ZoneDestinationID == null || trip.ZoneDestinationID < 0)
						{
							toReturn.Add(new ValidationMessage("DestinationState", "Destination State is required. " + tripDetails, container));
						}
					}

					//Validate Nonzone-only fields
					if (nonzone)
					{
						if (string.IsNullOrEmpty(trip.NonZoneFrom))
						{
							toReturn.Add(new ValidationMessage("From", "From Location is required. " + tripDetails, container));
						}

						if (string.IsNullOrEmpty(trip.NonZoneTo))
						{
							toReturn.Add(new ValidationMessage("To", "To Location is required. " + tripDetails, container));
						}

						if (trip.NonZoneNumCars == null)
						{
							toReturn.Add(new ValidationMessage("NumOfCars", "# Cars is required. " + tripDetails, container));
						}
						else if (trip.NonZoneNumCars < -999 || trip.NonZoneNumCars > 999 || trip.NonZoneNumCars == 0)
						{
							toReturn.Add(new ValidationMessage("NumOfCars", "# Cars must be between -999 and -1 or between 1 and 999. " + tripDetails, container));
						}

						if (trip.NonZoneAirfareEstimate == null)
						{
							toReturn.Add(new ValidationMessage("AirfareEstimate", "Airfare Estimate is required. " + tripDetails, container));
						}
						else if (Math.Round((decimal)trip.NonZoneAirfareEstimate, 2) == 0m)
						{
							toReturn.Add(new ValidationMessage("AirfareEstimate", "Airfare Estimate cannot be 0. " + tripDetails, container));
						}

						if (trip.NonZonePerDiemDaily == null)
						{
							toReturn.Add(new ValidationMessage("PerDiemDaily", "Per Diem Daily is required. " + tripDetails, container));
						}
						else if (Math.Round((decimal)trip.NonZonePerDiemDaily, 2) == 0m)
						{
							toReturn.Add(new ValidationMessage("PerDiemDaily", "Per Diem Daily cannot be 0. " + tripDetails, container));
						}

						if (trip.NonZoneCarRentalTrans == null)
						{
							toReturn.Add(new ValidationMessage("CarRentalTrans", "Car Rental/Trans is required. " + tripDetails, container));
						}
						else if (Math.Round((decimal)trip.NonZoneCarRentalTrans, 2) == 0m)
						{
							toReturn.Add(new ValidationMessage("CarRentalTrans", "Car Rental/Trans cannot be 0. " + tripDetails, container));
						}

						if (trip.NonZoneResourceID == null || trip.NonZoneResourceID <= 0)
						{
							toReturn.Add(new ValidationMessage("NonZoneResourceID", "Resource ID is required. " + tripDetails, container));
						}
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Validate the dates of the additional trips created when multiple occurrences are selected for a trip
		/// </summary>
		/// <param name="inTravelTrips">Trips to validate (additional occurrences, not including initial trip)</param>
		/// <param name="taskStartDate">Task Start Date</param>
		/// <param name="taskEndDate">Task End Date</param>
		/// <param name="container">Validation Box container id</param>
		/// <returns>Collection of validation messages if there are trips outside of task dates, or an empty collection if there are no issues</returns>
		public ICollection<ValidationMessage> ValidateTripDateForMultipleOccurrences(Collection<MSTTravelTripType> inTravelTrips, DateTime taskStartDate, DateTime taskEndDate, string container)
		{
			if (inTravelTrips == null)
			{
				throw new ArgumentNullException(nameof(inTravelTrips));
			}

			ICollection<ValidationMessage> toReturn = new Collection<ValidationMessage>();

			foreach (MSTTravelTripType trip in inTravelTrips)
			{
				if (GenBOEUtilities.AdjustDateTimePrecision(trip.TripDate, DateTimePrecision.Month) < GenBOEUtilities.AdjustDateTimePrecision((taskStartDate), DateTimePrecision.Month) && trip.Updateable != UpdateType.Deleted)
				{
					toReturn.Add(new ValidationMessage("TripDate", "# Occurrences and Interval cause a trip to occur before the task start date on " + trip.TripDate.ToString("MM/yyyy"), container));
				}

				else if (GenBOEUtilities.AdjustDateTimePrecision(trip.TripDate, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision((taskEndDate), DateTimePrecision.Month) && trip.Updateable != UpdateType.Deleted)
				{
					toReturn.Add(new ValidationMessage("TripDate", "# Occurrences and Interval cause a trip to occur after the task end date on " + trip.TripDate.ToString("MM/yyyy"), container));
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Creates the string containing trip details to be appended to validation messages
		/// </summary>
		/// <param name="trip">Trip</param>
		/// <param name="zone">bool noting if trip is Zone(true) or Nonzone (false)</param>
		/// <returns>String with trip details</returns>
		private string CreateTripDetailsString(MSTTravelTripType trip, bool zone)
		{
			string fromLoc = zone ? trip.ZoneOriginName : trip.NonZoneFrom;
			string toLoc = trip.NonZoneTo;
			if (zone)
			{
				if (!string.IsNullOrEmpty(trip.ZoneDestCity))
				{
					toLoc = trip.ZoneDestCity;
					if (!string.IsNullOrEmpty(trip.ZoneDestinationName))
					{
						toLoc += ", " + trip.ZoneDestinationName;
					}
				}
				else
				{
					toLoc = trip.ZoneDestinationName;
				}
			}

			string tripDetails = "For trip";
			if (trip.GroupID.HasValue)
			{
				tripDetails += " " + trip.GroupID.ToString();
			}
			if (!string.IsNullOrEmpty(fromLoc))
			{
				tripDetails += " from " + fromLoc;
			}
			if (!string.IsNullOrEmpty(toLoc))
			{
				tripDetails += " to " + toLoc;
			}
			if (trip.TripDate != DateTime.MinValue)
			{
				tripDetails += " on " + trip.TripDate.ToString("MM/dd/yyyy");
			}

			return tripDetails;
		}

		/// <summary>
		/// This method will check if a travel trip is within the travel task's start/end date and if a nonzone date of estimate is before the trip date
		/// </summary>
		/// <param name="ValidationMessages">validation messages</param>
		/// <param name="taskStartDate">Task start date</param>
		/// <param name="taskEndDate">Task end date</param>
		/// <param name="inTravelTrip">travel trip</param>
		/// <param name="boeDTO">boe containing trip</param>
		/// <param name="includeDetails">Bool to note if trip-specific details are needed for the validation messages</param>
		/// <param name="container">Validation Box container id</param>
		private void CheckTravelAndTravelTripDates(Collection<ValidationMessage> ValidationMessages, DateTime? taskStartDate, DateTime? taskEndDate, MSTTravelTripType inTravelTrip, BoeDTO boeDTO, bool includeDetails, string container)
		{
			string tripDetails = includeDetails ? this.CreateTripDetailsString(inTravelTrip, (inTravelTrip.ModeID == MSTTravelMode.ZoneNoAirfare || inTravelTrip.ModeID == MSTTravelMode.ZoneAirfare)) : string.Empty;

			// the date for the travel trip must be contained within the travel task start/end date
			if (GenBOEUtilities.AdjustDateTimePrecision(inTravelTrip.TripDate, DateTimePrecision.Month) < GenBOEUtilities.AdjustDateTimePrecision((taskStartDate ?? boeDTO.StartDate), DateTimePrecision.Month) && inTravelTrip.Updateable != UpdateType.Deleted)
			{
				ValidationMessages.Add(new ValidationMessage("", "Trip Date must start on or after task start date" + tripDetails, container));
			}

			if (GenBOEUtilities.AdjustDateTimePrecision(inTravelTrip.TripDate, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision((taskEndDate ?? boeDTO.EndDate), DateTimePrecision.Month) && inTravelTrip.Updateable != UpdateType.Deleted)
			{
				ValidationMessages.Add(new ValidationMessage("", "Trip Date must be finished before the task end date" + tripDetails, container));
			}

			// the date of estimate for non-zone trips must be before the trip date and travel task end date
			if (inTravelTrip.ModeID == MSTTravelMode.NonZoneDomestic || inTravelTrip.ModeID == MSTTravelMode.NonZoneInternational)
			{
				if (GenBOEUtilities.AdjustDateTimePrecision(inTravelTrip.EstimateDate, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision((inTravelTrip.TripDate), DateTimePrecision.Month) && inTravelTrip.Updateable != UpdateType.Deleted)
				{
					ValidationMessages.Add(new ValidationMessage("Date of Estimate", "Date of Estimate must be on or before the trip date" + tripDetails, container));
				}

				if (GenBOEUtilities.AdjustDateTimePrecision(inTravelTrip.EstimateDate, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision((taskEndDate ?? boeDTO.EndDate), DateTimePrecision.Month) && inTravelTrip.Updateable != UpdateType.Deleted)
				{
					ValidationMessages.Add(new ValidationMessage("Date of Estimate", "Date of Estimate must be on or before the task end date" + tripDetails, container));
				}
			}
		}
	}
}
