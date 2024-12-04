// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    [Serializable()]
    [ExcludeFromCodeCoverage]
    public abstract class ResourceRateDTO : UpdateableDTO
    {
        protected ResourceRateDTO() : base()
        {
            ResourceRateID = -1;
            ResourceID = -1;
            ResourceRate = 0.0M;
            LockedRate = false;
        }
        public int ResourceRateID { get; set; }
        public decimal? ResourceRate { get; set; }
		public string ResourceName { get; set; }
        public int ResourceID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool LockedRate { get; set; }
    }
}
