// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class PerDiemDTO : UpdateableDTO
    {
        public PerDiemDTO()
        {
            LastUpdatedBy = -1;
            LockedRate = false;
        }

        public string PerDiemDestination { get; set; }
        public string Qualification { get; set; }
        public decimal HotelRate { get; set; }
        public decimal MIERate { get; set; }
        public string PerDiemNotes { get; set; }
        public int? LastUpdatedBy { get; set; }
        public DateTime PerDiemLastUpdatedDate { get; set; }
        /// <summary>
        /// Is this rate locked
        /// </summary>
        public bool LockedRate { get; set; }
    }
}
