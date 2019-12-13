using System.Collections.ObjectModel;

namespace GenBOE.Web.ModelView
{
    public class ImportTripsResultsModelView
    {
        public Collection<ImportedTripModelView> ImportedTrips { get; set; }
        public Collection<ImportedPerDiemModelView> ImportedPerDiems { get; set; }
        public Collection<ImportedLocationModelView> ImportedLocations { get; set; }

        public ImportTripsResultsModelView() 
        {
            ImportedTrips = new Collection<ImportedTripModelView>();
            ImportedPerDiems = new Collection<ImportedPerDiemModelView>();
            ImportedLocations = new Collection<ImportedLocationModelView>();
        }
    }
    
    public class ImportedTripModelView
    {
        public int ImportType { get; set; }

        public int TripID { get; set; }
        public string Mode { get; set; }
        public int MiscTravelRateID { get; set; }
        public int DepartureID { get; set; }
        public string DepartureCode { get; set; }
        public int DestinationID { get; set; }
        public string DestinationCode { get; set; }
        public string DepartureLocation { get; set; }        
        public int PerDiemID { get; set; }
        public string PerDiemDestination { get; set; }
        public decimal Fare { get; set; }
        public int RTMiles { get; set; }
        public string Qualification { get; set; }
        public Collection<string> MissingFields { get; set; }
        public string InvalidField { get; set; }
        public string InvalidValue { get; set; }
        public decimal RentalCarRate { get; set; }

    }

    public class ImportedPerDiemModelView
    {
        public int PerDiemID { get; set; }
        public string PerDiemDestination { get; set; }
        public string Qualification { get; set; }
        public decimal HotelRate { get; set; }
        public decimal MIERate { get; set; }
        public string PerDiemNotes { get; set; }
    }

    public class ImportedLocationModelView
    {
        public int LocationID { get; set; }
        public string LocationName { get; set; }
        public string LocationCode { get; set; }
    }
}