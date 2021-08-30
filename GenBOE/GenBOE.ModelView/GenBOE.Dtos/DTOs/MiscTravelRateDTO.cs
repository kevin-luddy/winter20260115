// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.ComponentModel.DataAnnotations;
    using IES.Common;

    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class MiscTravelRateDTO : UpdateableDTO
    {
        public MiscTravelRateDTO()
        {
            MiscTravelRateMode = string.Empty;
            MiscTravelRate = 0;
            SortCode = 0;
            inUse = false;
        }

        [StringLength(25, ErrorMessage = "A maximum of 25 characters are allowed")]
        public string MiscTravelRateMode { get; set; }
        public decimal MiscTravelRate { get; set; }
        public int SortCode { get; set; }
        public bool inUse { get; set; }
        public bool lockedRate { get; set; }

    }
}
