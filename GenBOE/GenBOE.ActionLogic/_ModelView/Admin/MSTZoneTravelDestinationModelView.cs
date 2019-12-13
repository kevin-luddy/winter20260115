// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Admin
{
    using GenBOE.Dtos;

    public class MSTZoneTravelDestinationModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MSTZoneTravelDestinationModelView()
        {
            this.DestinationID = -1;
            this.Destination = string.Empty;
            this.Abbreviation = string.Empty;
            this.Zone = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="destinationID">ID of the Destination</param>
        /// <param name="destination">Destination name (state)</param>
        /// <param name="abbreviation">Abbreviation of the destination (state) name</param>
        /// <param name="zone">Zone that the Destination is assigned to</param>
        public MSTZoneTravelDestinationModelView(
            int destinationID,
            string destination,
            string abbreviation,
            int zone)
            : this()
        {
            this.DestinationID = destinationID;
            this.Destination = destination;
            this.Abbreviation = abbreviation;
            this.Zone = zone;
        }

        /// <summary>
        /// Gets a DTO for the Destination based on the model view
        /// </summary>
        /// <returns>DTO for the Destination</returns>
        public MSTZoneTravelDestinationDTO GetDestinationDTO()
        {
            MSTZoneTravelDestinationDTO toReturn = new MSTZoneTravelDestinationDTO();

            toReturn.DestinationID = this.DestinationID;
            toReturn.Destination = this.Destination;
            toReturn.Abbreviation = this.Abbreviation;
            toReturn.Zone = this.Zone;

            return toReturn;
        }

        /// <summary>
        /// ID of the Destination
        /// </summary>
        public int DestinationID { get; set; }

        /// <summary>
        /// Destination name (state)
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Abbreviation of the destination (state) name
        /// </summary>
        public string Abbreviation { get; set; }

        /// <summary>
        /// Zone that the Destination is assigned to
        /// </summary>
        public int Zone { get; set; }

    }
}