namespace GenBOE.Dtos
{
    public class TripData
    {
        public TripData()
        {
        }

        public int? ModeID { get; set; }
        public int? DepartureID { get; set; }
        public string DepartureName { get; set; }
        public int? DestinationID { get; set; }
        public string DestinationName { get; set; }
        public string Qualifciation { get; set; }
        public int? TripID { get; set; }
    }
}
