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
    public class OffloadRatesDTO : UpdateableDTO
    {
        public OffloadRatesDTO()
        {
            this.Id = -1;
            this.Year = 0;
        }

        public int Year { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal Percent { get; set; }
        public string Resource { get; set; }
        public string SubResource { get; set; }
        public string PerformingOrg { get; set; }
    }
}
