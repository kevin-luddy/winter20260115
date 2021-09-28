// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using IES.Common;

    /// <summary>
    /// DTO for Resources of Origins for MST Zone Travel
    /// </summary>
    public class MSTZoneTravelResourceDTO : UpdateableDTO
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MSTZoneTravelResourceDTO()
            :this(string.Empty)
        {
        }

        /// <summary>
        /// Constructor taking in values
        /// </summary>
        /// <param name="resourceID">ID for individual Resource</param>
        /// <param name="resource">Name of the Resource</param>
        /// <param name="zone">Zone that the Resource is assigned to</param>
        /// <param name="isAirfare">Bool noting if resource is Airfare (true) or Per Diem/Misc (false)</param>
        /// <param name="lookupValue">Lookup value for the Resource</param>
        /// <param name="description">Description of the Resource</param>
        public MSTZoneTravelResourceDTO(int resourceID, string resource, int zone, bool isAirfare, string lookupValue, string description, int originID)
        {
            ResourceID = resourceID;
            Resource = resource;
            Zone = zone;
            IsAirfare = isAirfare;
            LookupValue = lookupValue;
            Description = description;
            OriginID = originID;
        }

        /// <summary>
        /// Constructor taking in resourceID and resource - used when updating Origins
        /// </summary>
        /// <param name="resourceID">ID for individual Resource</param>
        /// <param name="resource">Name of the Resource</param>
        public MSTZoneTravelResourceDTO(int resourceID, string resource)
            :this(resourceID, resource, 0, false, string.Empty, string.Empty, -1)
        {
        }

        /// <summary>
        /// Constructor taking in resource - used when adding Origins
        /// </summary>
        /// <param name="resource">Name of the Resource</param>
        public MSTZoneTravelResourceDTO(string resource)
            :this(-1, resource)
        {
        }

        /// <summary>
        /// ID for individual Resource
        /// </summary>
        public int ResourceID { get; set; }

        /// <summary>
        /// Name of the Resource
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Zone that the Resource is assigned to
        /// </summary>
        public int Zone { get; set; }

        /// <summary>
        /// Bool noting if resource is Airfare (true) or Per Diem/Misc (false)
        /// </summary>
        public bool IsAirfare { get; set; }

        /// <summary>
        /// Lookup value for the Resource
        /// </summary>
        public string LookupValue { get; set; }

        /// <summary>
        /// Description of the Resource
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ID of Origin Resource belongs to
        /// </summary>
        public int OriginID { get; set; }
    }
}
