// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.OfficeUtilities;

    [ExcludeFromCodeCoverage]
    public class TripsImporter
    {
        private Logger _log = new Logger(typeof(TripsImporter));

        private ITripDTODataLoader _tripLoader;
        private IMiscTravelRateDTOLoader miscTravelRateLoader;

        private const string tripIDColumn = "Trip ID";
        private const string modeColumn = "Mode";
        private const string departureLocColumn = "Departure Loc";
        private const string departureLocCodeColumn = "Code - Departure Loc";
        private const string perDiemDestColumn = "Per Diem Dest";
        private const string destinationColumn = "Destination";
        private const string destinationCodeColumn = "Code - Destination";
        private const string qualificationColumn = "Qualification";
        private const string fareColumn = "Fare";
        private const string hotelColumn = "Hotel";
        private const string mieRateColumn = "MIE Rate";
        private const string perDiemNotesColumn = "Per Diem Notes";
        private const string rentalCarColumn = "Rental Car";
        private const string rtMilesColumn = "R/T Miles";
        
        private const int RTMILES_MIN = 0;
        private const int RTMILES_MAX = 99999;
        private const decimal ALL_RATE_MIN = 0;
        private const decimal RENTAL_CAR_MAX = 9999.99m;
        private const decimal MIERATE_MAX = 9999.99m;
        private const decimal HOTEL_MAX = 99999.99m;
        private const decimal FARE_MAX = 999999.99m;
        private const int LENGTH_LOCATION = 40;
        private const int LENGTH_CODE = 10;
        private const int LENGTH_PER_DIEM_DESTINATION = 40;
        private const int LENGTH_QUALIFICATION = 40;
        private const int LENGTH_PER_DIEM_NOTES = 100;

        // Array of the columns that must be contained in the imported file
        // These are columns that must be present in the imported file, not columns that must be populated with data.
        private readonly string[] requiredColumns = new string[]
        {
            tripIDColumn,
            modeColumn,
            departureLocColumn,
            departureLocCodeColumn,
            perDiemDestColumn,
            destinationColumn,
            destinationCodeColumn,
            qualificationColumn,
            fareColumn,
            hotelColumn,
            mieRateColumn,
            perDiemNotesColumn,
            rentalCarColumn,
            rtMilesColumn
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inTripLoader"></param>
        /// <param name="inMiscTravelRateLoader"></param>
        public TripsImporter(ITripDTODataLoader inTripLoader,
            IMiscTravelRateDTOLoader inMiscTravelRateLoader)
        {
            this._tripLoader = inTripLoader;
            this.miscTravelRateLoader = inMiscTravelRateLoader;
        }

        /// <summary>
        /// Import all the trips in the excel file.
        /// </summary>
        /// <param name="excelFileStream"></param>
        /// <returns></returns>
        public ImportedTripResults ImportTripsFromExcelFile(Stream excelFileStream, ICollection<PerDiemDTO> allPerDiems, ICollection<LocationDTO> allLocations)
        {
            if (excelFileStream == null)
            {
                throw new ArgumentNullException(nameof(excelFileStream));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                try
                {
                    ImportedTripResults importResults = null;

                    // Open the document as read-only.
                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                    {
                        // Get a collection of all rows in the file, filtering out rows that only have data in
                        // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                        // in an enumerable collection
                        ICollection<Dictionary<string, string>> allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, this.requiredColumns, this.requiredColumns);

                        // Turn each row into a DTO object and return the collection
                        importResults = this.CreateImportedTrips(allRows, allPerDiems, allLocations);
                    }

                    return importResults;
                }
                catch (FileFormatException)
                {
                    throw new NotExcelFileException("Imported file was an incorrect format.");
                }
            }
        }

        /// <summary>
        /// Creates the collection of trips to be imported, or with their errors.
        /// </summary>
        /// <param name="allRows"></param>
        /// <returns></returns>
        private ImportedTripResults CreateImportedTrips(ICollection<Dictionary<string, string>> allRows, ICollection<PerDiemDTO> allPerDiems, ICollection<LocationDTO> allLocations)
        {
            Collection<ImportedTrip> importResults = new Collection<ImportedTrip>();

            ICollection<TripDTO> allTrips = this._tripLoader.GetAllTrips();
            ICollection<MiscTravelRateDTO> allModes = this.miscTravelRateLoader.GetAll();
            ICollection<PerDiemDTO> modifiedPerDiems = new Collection<PerDiemDTO>();
            ICollection<LocationDTO> newLocations = new Collection<LocationDTO>();
            ICollection<int> invalidPerDiems = new Collection<int>();

            int newObjectPrimaryKey = -1;

            foreach (Dictionary<string, string> row in allRows)
            {
                ImportedTrip result = this.CreateResult(row, allTrips, allModes, allPerDiems, allLocations, newObjectPrimaryKey, modifiedPerDiems, newLocations, invalidPerDiems);
                if (result != null)
                {
                    importResults.Add(result);
                    newObjectPrimaryKey-= 2; // this is 2 because each result can have 2 new locations, and the value is not passed by reference.
                }
            }

            // update results with invalid per diems
            var validImports = importResults.Where(x => x.ImportTypes.Contains(TripImportResult.AddNewTrip) || x.ImportTypes.Contains(TripImportResult.UpdateExistingTrip)).ToArray();

            foreach (ImportedTrip validImport in validImports)
            {
                if (invalidPerDiems.Contains(validImport.PerDiemID))
                {
                    validImport.ImportTypes.Clear();
                    validImport.ImportTypes.Add(TripImportResult.MismatchedPerDiemData);
                }

                // search for another matching valid trip
                var matchingTrips = (from x in validImports
                                     where x.TripID != validImport.TripID &&
                                        x.MiscTravelRateID == validImport.MiscTravelRateID &&
                                        x.DepartureLocationID == validImport.DepartureLocationID &&
                                        x.DestinationLocationID == validImport.DestinationLocationID &&
                                        validImport.Qualification.IsEquivalentTo(x.Qualification)
                                     select x).ToArray();

                if (matchingTrips.Length > 0)
                {
                    validImport.ImportTypes.Clear();
                    validImport.ImportTypes.Add(TripImportResult.MatchesNewTrip);
                }
            }

            return new ImportedTripResults() { ImportedTrips = importResults, ModifiedPerDiems = modifiedPerDiems, NewLocations = newLocations };
        }

        /// <summary>
        /// Creates a single trip with its result.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="allTrips"></param>
        /// <param name="allModes"></param>
        /// <param name="newObjectPrimaryKey"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public ImportedTrip CreateResult(Dictionary<string, string> row, ICollection<TripDTO> allTrips, ICollection<MiscTravelRateDTO> allModes, ICollection<PerDiemDTO> allPerDiems, ICollection<LocationDTO> allLocations, int newObjectPrimaryKey, ICollection<PerDiemDTO> modifiedPerDiems, ICollection<LocationDTO> newLocations, ICollection<int> invalidPerDiems)
        {
            if (modifiedPerDiems == null)
            {
                throw new ArgumentNullException(nameof(modifiedPerDiems));
            }
            if (newLocations == null)
            {
                throw new ArgumentNullException(nameof(newLocations));
            }
            if (invalidPerDiems == null)
            {
                throw new ArgumentNullException(nameof(invalidPerDiems));
            }
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            #region Trip Object Import

            ImportedTrip currentResult = new ImportedTrip();
            
            bool missingFields = false;

            // Check for required data.
            #region Required Fields

            if (!row.ContainsKey(modeColumn))
            {
                currentResult.Mode = string.Empty;
                currentResult.MissingFields.Add(modeColumn);
                missingFields = true;
            }
            else
            {
                currentResult.Mode = row[modeColumn];
            }

            string departureLocationName;
            if (!row.ContainsKey(departureLocColumn))
            {
                departureLocationName = string.Empty;
                currentResult.MissingFields.Add(departureLocColumn);
                missingFields = true;
            }
            else
            {
                departureLocationName = this.TruncateString(row[departureLocColumn], LENGTH_LOCATION);
                currentResult.DepartureLocationName = departureLocationName;
            }

            string destinationLocationName;
            if (!row.ContainsKey(destinationColumn))
            {
                destinationLocationName = string.Empty;
                currentResult.MissingFields.Add(destinationColumn);
                missingFields = true;
            }
            else
            {
                destinationLocationName = this.TruncateString(row[destinationColumn], LENGTH_LOCATION);
            }

            string perDiemDestination;
            if (!row.ContainsKey(perDiemDestColumn))
            {
                perDiemDestination = string.Empty;
                currentResult.MissingFields.Add(perDiemDestColumn);
                missingFields = true;
            }
            else
            {
                perDiemDestination = row[perDiemDestColumn];
                currentResult.PerDiemDestination = this.TruncateString(perDiemDestination, LENGTH_PER_DIEM_DESTINATION);
            }

            if (!row.ContainsKey(fareColumn))
            {
                currentResult.MissingFields.Add(fareColumn);
                missingFields = true;
            }

            if (!row.ContainsKey(hotelColumn))
            {
                currentResult.MissingFields.Add(hotelColumn);
                missingFields = true;
            }

            if (!row.ContainsKey(mieRateColumn))
            {
                currentResult.MissingFields.Add(mieRateColumn);
                missingFields = true;
            }

            if (!row.ContainsKey(rentalCarColumn))
            {
                currentResult.MissingFields.Add(rentalCarColumn);
                missingFields = true;
            }
            if (!row.ContainsKey(rtMilesColumn))
            {
                currentResult.MissingFields.Add(rtMilesColumn);
                missingFields = true;
            }

            if (missingFields)
            {
                currentResult.ImportTypes.Add(TripImportResult.MissingRequiredField);
                return currentResult;
            }

            #endregion Required Fields

            // Try to get other fields
            #region NonRequired Fields

            string departureLocCode = string.Empty;
            if (row.ContainsKey(departureLocCodeColumn))
            {
                departureLocCode = this.TruncateString(row[departureLocCodeColumn], LENGTH_CODE);
            }

            string destinationLocCode = string.Empty;
            if (row.ContainsKey(destinationCodeColumn))
            {
                destinationLocCode = this.TruncateString(row[destinationCodeColumn], LENGTH_CODE);
            }

            string qualification = string.Empty;
            if (row.ContainsKey(qualificationColumn))
            {
                qualification = this.TruncateString(row[qualificationColumn], LENGTH_QUALIFICATION);
                currentResult.Qualification = qualification;
            }
            
            string perDiemNotes = string.Empty;
            if (row.ContainsKey(perDiemNotesColumn))
            {
                perDiemNotes = this.TruncateString(row[perDiemNotesColumn], LENGTH_PER_DIEM_NOTES);
            }

            #endregion NonRequired Fields

            #region Individaul Field Validation

            MiscTravelRateDTO currentMiscTravelRate = allModes.FirstOrDefault(x => x.MiscTravelRateMode.IsEquivalentTo(currentResult.Mode));
            if (currentMiscTravelRate == null)
            {
                currentResult.ImportTypes.Add(TripImportResult.InvalidMode);
                return currentResult;
            }

            decimal fare;
            if (!decimal.TryParse(row[fareColumn], out fare) || fare < ALL_RATE_MIN || fare > FARE_MAX)
            {
                currentResult.ImportTypes.Add(TripImportResult.FieldContainsInvalidValue);
                currentResult.InvalidField = "Fare";
                currentResult.InvalidValue = row[fareColumn];
                return currentResult;
            }

            decimal rtMiles;
            if (!decimal.TryParse(row[rtMilesColumn], out rtMiles) || rtMiles < RTMILES_MIN || rtMiles > RTMILES_MAX)
            {
                currentResult.ImportTypes.Add(TripImportResult.FieldContainsInvalidValue);
                currentResult.InvalidField = "R/T Miles";
                currentResult.InvalidValue = row[rtMilesColumn];
                return currentResult;
            }

            decimal hotel;
            if (!decimal.TryParse(row[hotelColumn], out hotel) || hotel < ALL_RATE_MIN || hotel > HOTEL_MAX)
            {
                currentResult.ImportTypes.Add(TripImportResult.FieldContainsInvalidValue);
                currentResult.InvalidField = "Hotel";
                currentResult.InvalidValue = row[hotelColumn];
                return currentResult;
            }

            decimal mieRate;
            if (!decimal.TryParse(row[mieRateColumn], out mieRate) || mieRate < ALL_RATE_MIN || mieRate > MIERATE_MAX)
            {
                currentResult.ImportTypes.Add(TripImportResult.FieldContainsInvalidValue);
                currentResult.InvalidField = "MIE Rate";
                currentResult.InvalidValue = row[mieRateColumn];
                return currentResult;
            }
            
            decimal rentalCarRate;
            if (!decimal.TryParse(row[rentalCarColumn], out rentalCarRate) || rentalCarRate < ALL_RATE_MIN || rentalCarRate > RENTAL_CAR_MAX)
            {
                currentResult.ImportTypes.Add(TripImportResult.FieldContainsInvalidValue);
                currentResult.InvalidField = "Rental Car";
                currentResult.InvalidValue = row[rentalCarColumn];
                return currentResult;
            }

            #endregion Individaul Field Validation

            // at this point, all required fields exist and all rates are valid #s, and the mode was valid.

            // If the row has a trip ID, then its an edit.
            if (row.ContainsKey(tripIDColumn))
            {
                int tripID;

                // try to get trip ID
                if (!int.TryParse(row[tripIDColumn], out tripID))
                {
                    currentResult.ImportTypes.Add(TripImportResult.FieldContainsInvalidValue);
                    currentResult.InvalidField = "Trip ID";
                    currentResult.InvalidValue = row[tripIDColumn];
                    return currentResult;
                }

                TripDTO currentTrip = allTrips.FirstOrDefault(x => x.TripID == tripID);
                currentResult.TripID = tripID;

                if (currentTrip == null)
                {
                    currentResult.ImportTypes.Add(TripImportResult.TripIDDoesNotExist);
                    return currentResult;
                }

                PerDiemDTO currentPerDiem = allPerDiems.FirstOrDefault(x => x.Id == currentTrip.PerDiemID);
                LocationDTO currentDeparture = allLocations.FirstOrDefault(x => x.Id == currentTrip.DepartureLocationID);
                LocationDTO currentDestination = allLocations.FirstOrDefault(x => x.Id == currentTrip.DestinationLocationID);

                // if all the fields are equal, don't update
                if (currentMiscTravelRate.Id == currentTrip.MiscTravelRateID &&
                    departureLocationName.ToLower() == currentDeparture.LocationName.Trim().ToLower() &&
                    departureLocCode.IsEquivalentTo(currentTrip.DepartureLocationCode) &&
                    destinationLocationName.IsEquivalentTo(currentDestination.LocationName) &&
                    destinationLocCode.IsEquivalentTo(currentTrip.DestinationLocationCode) && 
                    perDiemDestination.IsEquivalentTo(currentPerDiem.PerDiemDestination) &&
                    qualification.IsEquivalentTo(currentPerDiem.Qualification) && 
                    hotel == currentPerDiem.HotelRate && mieRate == currentPerDiem.MIERate &&
                    perDiemNotes.IsEquivalentTo(currentPerDiem.PerDiemNotes) && 
                    rentalCarRate == currentTrip.RentalCarRate &&
                    fare == currentTrip.Fare &&
                    rtMiles == currentTrip.RTMiles)
                {
                    // no updates occured.
                    return null;
                }
                
                // Verify none of the fields that cannot change have not changed
                if (currentTrip.MiscTravelRateID != currentMiscTravelRate.Id ||
                    departureLocationName.IsNotEquivalentTo(currentDeparture.LocationName) ||
                    departureLocCode.IsNotEquivalentTo(currentTrip.DepartureLocationCode) ||
                    destinationLocationName.IsNotEquivalentTo(currentDestination.LocationName) ||
                    destinationLocCode.IsNotEquivalentTo(currentTrip.DestinationLocationCode) ||
                    perDiemDestination.IsNotEquivalentTo(currentPerDiem.PerDiemDestination) ||
                    qualification.IsNotEquivalentTo(currentPerDiem.Qualification))
                {
                    currentResult.ImportTypes.Add(TripImportResult.FieldsCannotChangeForExistingTrip);
                    return currentResult;
                }

                // update Trip
                currentResult = new ImportedTrip(currentTrip);                
                currentResult.Fare = fare;
                currentResult.RTMiles = (int)Math.Round(rtMiles, 0);
                currentResult.RentalCarRate = rentalCarRate;

                // These are display columns, sent with text for the FE.
                currentResult.Mode = row[modeColumn];
                currentResult.DepartureLocationName = departureLocationName;
                currentResult.PerDiemDestination = perDiemDestination;
                currentResult.Qualification = qualification;

                currentResult.ImportTypes.Add(TripImportResult.UpdateExistingTrip);
            }
            else
            {
                // Get all trips with this mode
                HashSet<TripDTO> filteredTrips = new HashSet<TripDTO>((from x in allTrips
                                                     where x.MiscTravelRateID == currentMiscTravelRate.Id
                                                     select x));

                // get matching per diems
                HashSet<int> filteredPerDiems = new HashSet<int>(from y in allPerDiems
                                                                 where y.Qualification.IsEquivalentTo(qualification)
                                                                 select y.Id);

                // get matching departure locations
                HashSet<int> filteredDepartures = new HashSet<int>(from z in allLocations
                                                                   where z.LocationName.IsEquivalentTo(departureLocationName)
                                                                   select z.Id);

                // get matching destinations
                HashSet<int> filteredDestinations = new HashSet<int>(from w in allLocations
                                                                     where w.LocationName.IsEquivalentTo(destinationLocationName)
                                                                     select w.Id);

                // Search for an existing trip
                // An "existing trip" has the same Mode, Departure Location, Destination, and Qualification as this trip.
                TripDTO existingTrip = (from x in filteredTrips
                                        where filteredPerDiems.Contains(x.PerDiemID) &&
                                            filteredDepartures.Contains(x.DepartureLocationID) &&
                                            filteredDestinations.Contains(x.DestinationLocationID)
                                        select x).FirstOrDefault();

                if (existingTrip != null)
                {
                    currentResult.ImportTypes.Add(TripImportResult.MatchesExistingTrip);
                    return currentResult;
                }

                // No match was found, Add Trip
                // departure loc, per diem, and qualification are already set
                currentResult.TripID = newObjectPrimaryKey;
                currentResult.MiscTravelRateID = currentMiscTravelRate.Id;
                currentResult.Fare = fare;
                currentResult.RTMiles = (int)Math.Round(rtMiles, 0);
                currentResult.RentalCarRate = rentalCarRate;
                currentResult.DepartureLocationCode = departureLocCode;
                currentResult.DestinationLocationCode = destinationLocCode;

                currentResult.ImportTypes.Add(TripImportResult.AddNewTrip);
            }

            #endregion Trip Object Import

            #region Per Diem Object Import

            // Determine if this Per Diem has already been modified during this import operation
            PerDiemDTO existingPerDiem = modifiedPerDiems.FirstOrDefault(
                x => x.PerDiemDestination.IsEquivalentTo(perDiemDestination) && 
                    x.Qualification.IsEquivalentTo(qualification));

            // this per diem has been modified... 
            if (existingPerDiem != null)
            {
                // Verify the modifications match either the original or the current modification.
                PerDiemDTO originalPerDiem = allPerDiems.FirstOrDefault(x => x.Id == existingPerDiem.Id);

                // if the original is null, the "existing" in the modified list is a new per diem with a negative ID.
                if (originalPerDiem == null && existingPerDiem.Id < 0) 
                {
                    // The current per diem needs to match the "existing" new per diem.
                    if (existingPerDiem.HotelRate != hotel ||
                        existingPerDiem.MIERate != mieRate ||
                        existingPerDiem.PerDiemNotes != perDiemNotes)
                    {
                        // keep track of the invalid per diems, previously correct imports will need to be updated to invalid.
                        invalidPerDiems.Add(existingPerDiem.Id);

                        currentResult.ImportTypes.Clear();
                        currentResult.ImportTypes.Add(TripImportResult.MismatchedPerDiemData);
                    }                    
                    currentResult.PerDiemID = existingPerDiem.Id;

                }
                else if (originalPerDiem == null && existingPerDiem.Id > 0 || existingPerDiem.Id == 0)
                {
                    throw new GeneralAppException("The Per Diem ID referenced in the import does not exist");
                }
                else
                {
                    // The current per diem needs to match either the modified per diem or the original per diem.
                    if ((existingPerDiem.HotelRate != hotel ||
                        existingPerDiem.MIERate != mieRate ||
                        existingPerDiem.PerDiemNotes != perDiemNotes) && 
                        (originalPerDiem.HotelRate != hotel ||
                        originalPerDiem.MIERate != mieRate ||
                        originalPerDiem.PerDiemNotes != perDiemNotes))
                    {
                        // keep track of the invalid per diems, previously correct imports will need to be updated to invalid.
                        invalidPerDiems.Add(existingPerDiem.Id);

                        currentResult.ImportTypes.Clear();
                        currentResult.ImportTypes.Add(TripImportResult.MismatchedPerDiemData);
                    } 
                    currentResult.PerDiemID = existingPerDiem.Id;
                }
            }
            else
            {
                // check for existing within all per diems
                existingPerDiem = allPerDiems.FirstOrDefault(
                    x => x.PerDiemDestination.IsEquivalentTo(perDiemDestination) && 
                        x.Qualification.IsEquivalentTo(qualification));  

                if (existingPerDiem != null)
                {
                    // determine if an edit has occured
                    if (existingPerDiem.HotelRate != hotel || existingPerDiem.MIERate != mieRate || existingPerDiem.PerDiemNotes != perDiemNotes)
                    {
                        existingPerDiem.HotelRate = hotel;
                        existingPerDiem.MIERate = mieRate;
                        existingPerDiem.PerDiemNotes = perDiemNotes;

                        modifiedPerDiems.Add(existingPerDiem);
                    }
                    currentResult.PerDiemID = existingPerDiem.Id;
                }
                else
                {
                    // new per diem
                    PerDiemDTO newPerDiem = new PerDiemDTO();
                    newPerDiem.Id = newObjectPrimaryKey;
                    newPerDiem.PerDiemDestination = perDiemDestination;
                    newPerDiem.Qualification = qualification;
                    newPerDiem.HotelRate = hotel;
                    newPerDiem.MIERate = mieRate;
                    newPerDiem.PerDiemNotes = perDiemNotes;

                    currentResult.PerDiemID = newPerDiem.Id;
                    modifiedPerDiems.Add(newPerDiem);
                }
            }

            #endregion Per Diem Object Import

            #region Location Object Import

            // Determine if we need to add new locations
            // check Departure
            LocationDTO existingLocation = allLocations.FirstOrDefault(x => x.LocationName == departureLocationName);
            if (existingLocation == null)
            {
                LocationDTO newLocation = new LocationDTO();
                newLocation.Id = newObjectPrimaryKey;
                newLocation.LocationName = departureLocationName;

                currentResult.DepartureLocationID = newLocation.Id;
                newLocations.Add(newLocation);

                if (newObjectPrimaryKey == int.MinValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(newObjectPrimaryKey), "the primary key must be greater than int.MinValue");
                }
                checked
                {
                    newObjectPrimaryKey--;
                }
            }
            else
            {
                currentResult.DepartureLocationID = existingLocation.Id;
            }
            // check Destination
            existingLocation = allLocations.FirstOrDefault(x => x.LocationName == destinationLocationName);
            if (existingLocation == null)
            {
                LocationDTO newLocation = new LocationDTO();
                newLocation.Id = newObjectPrimaryKey;
                newLocation.LocationName = destinationLocationName;

                currentResult.DestinationLocationID = newLocation.Id;
                newLocations.Add(newLocation);
            }
            else
            {
                currentResult.DestinationLocationID = existingLocation.Id;
            }

            #endregion Location Object Import

            return currentResult;
        }

        private string TruncateString(string value, int maxCharacters)
        {
            value = value.Trim();

            if (value.Length > maxCharacters)
            {
                return value.Substring(0, maxCharacters);
            }
            return value;
        }
    }

    public enum TripImportResult
    {
        AddNewTrip = 1,
        UpdateExistingTrip = 2,
        FieldsCannotChangeForExistingTrip = 3,
        MatchesExistingTrip = 4,
        MissingRequiredField = 5,
        FieldContainsInvalidValue = 6,
        TripIDDoesNotExist = 7,
        InvalidMode = 8,
        MismatchedPerDiemData = 9,
        MatchesNewTrip = 10
    }

    [ExcludeFromCodeCoverage]
    public class ImportedTripResults
    {
        public ICollection<ImportedTrip> ImportedTrips { get; set; }
        public ICollection<PerDiemDTO> ModifiedPerDiems { get; set; }
        public ICollection<LocationDTO> NewLocations { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ImportedTrip : TripDTO
    {
        public Collection<TripImportResult> ImportTypes { get; set; }

        // For Display
        public string Mode { get; set; }
        public string DepartureLocationName { get; set; }
        public string PerDiemDestination { get; set; }
        public string Qualification { get; set; }

        // For Invalid Rows in the file
        public Collection<string> MissingFields { get; set; }
        public string InvalidField { get; set; }
        public string InvalidValue { get; set; }

        public ImportedTrip()
        {
            this.ImportTypes = new Collection<TripImportResult>();
            this.MissingFields = new Collection<string>();
        }

        public ImportedTrip(TripDTO tripDTO)
            : this()
        {
            if (tripDTO != null)
            {
                this.TripID = tripDTO.TripID;
                this.LockedRate = tripDTO.LockedRate;
                this.MiscTravelRateID = tripDTO.MiscTravelRateID;
                this.DepartureLocationID = tripDTO.DepartureLocationID;
                this.DestinationLocationID = tripDTO.DestinationLocationID;
                this.PerDiemID = tripDTO.PerDiemID;
                this.Fare = tripDTO.Fare;
                this.RTMiles = tripDTO.RTMiles;
                this.FareUpdatedByUserID = tripDTO.FareUpdatedByUserID;
                this.FareLastUpdatedDate = tripDTO.FareLastUpdatedDate;
                this.LastUsedDate = tripDTO.LastUsedDate;
                this.TripCount = tripDTO.TripCount;
                this.InUse = tripDTO.InUse;
                this.DepartureLocationCode = tripDTO.DepartureLocationCode;
                this.DestinationLocationCode = tripDTO.DestinationLocationCode;
            }
        }
    }
}
