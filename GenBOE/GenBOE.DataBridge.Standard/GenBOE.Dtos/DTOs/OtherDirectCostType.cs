using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using IES.Standard;
using System.Collections.ObjectModel;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class OtherDirectCostType : UpdateableDTO, IBOEMembership, IStartEndDates
    {
        public OtherDirectCostType()
        {
            ODCTypeID = -1;
            ResourceID = 0;
            PerformingOrgID = 0;
            SpreadCurve = SpreadCurves.DiscreteHours;
            StartDate = DateTime.MinValue;
            EndDate = DateTime.MinValue;
            ODCSpreads = new Collection<OtherDirectCostSpread>();
        }

        public OtherDirectCostType(OtherDirectCostType inODCType):this()
        {
            if (inODCType != null)
            {
                ODCTypeID = inODCType.ODCTypeID;
                ResourceID = inODCType.ResourceID;
                PerformingOrgID = inODCType.PerformingOrgID;
                SpreadCurve = inODCType.SpreadCurve;
                StartDate = inODCType.StartDate;
                EndDate = inODCType.EndDate;
                ODCSpreads = inODCType.ODCSpreads;
                Cost = inODCType.Cost;
            }
        }


        public int? ODCTypeID { get; set; }

        // the resource code ID
        public int? ResourceID { get; set; }

        // the performing organization ID
        public int? PerformingOrgID { get; set; }

        public SpreadCurves SpreadCurve { get; set; }

        // the start date
        public DateTime? StartDate { get; set; }

        // the end date
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
        /// </summary>
        internal IEnumerable<OtherDirectCostSpread> ODCSpreadsIEnum { get; set; }

        public Collection<OtherDirectCostSpread> ODCSpreads { get; set; }

        public int BoeID { get; set; }

        public decimal? Cost { get; set; }
    }
}
