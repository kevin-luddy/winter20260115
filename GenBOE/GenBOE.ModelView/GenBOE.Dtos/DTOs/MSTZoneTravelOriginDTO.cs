// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using IES.Common;

    /// <summary>
    /// DTO for Origins for MST Zone Travel
    /// </summary>
    public class MSTZoneTravelOriginDTO : UpdateableDTO
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MSTZoneTravelOriginDTO()
        {
            OriginID = -1;
            Origin = string.Empty;
            Site = string.Empty;
        }

        /// <summary>
        /// Individual ID for the Origin
        /// </summary>
        public int OriginID { get; set; }
        
        /// <summary>
        /// Origin location name
        /// </summary>
        public string Origin { get; set; }
        
        /// <summary>
        /// Site of the Origin
        /// </summary>
        public string Site { get; set; }        
    }
}
