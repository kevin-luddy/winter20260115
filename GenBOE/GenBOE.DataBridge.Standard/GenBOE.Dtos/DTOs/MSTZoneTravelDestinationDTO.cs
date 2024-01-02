// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using IES.Standard;

    /// <summary>
    /// DTO for Destinations for MST Zone Travel
    /// </summary>
    public class MSTZoneTravelDestinationDTO : UpdateableDTO
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MSTZoneTravelDestinationDTO()
        {
            DestinationID = -1;
            Destination = string.Empty;
            Abbreviation = string.Empty;
            Zone = 0;
        }

        /// <summary>
        /// Individual ID for the Destination
        /// </summary>
        public int DestinationID { get; set; }

        /// <summary>
        /// Destination name (state)
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Destination (state) abbreviation
        /// </summary>
        public string Abbreviation { get; set; }

        /// <summary>
        /// Zone destination is assigned to
        /// </summary>
        public int Zone { get; set; }
    }
}
