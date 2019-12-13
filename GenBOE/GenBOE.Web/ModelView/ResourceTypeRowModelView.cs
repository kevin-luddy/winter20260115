namespace GenBOE.Web.ModelView
{
    using System;
    using IES.Common;

    public class ResourceTypeRowModelView
    {
        public ResourceTypeRowModelView()
        {
        }

        public int BOELaborTypeID { get; set; }
        public ElementOfCostType ElementOfCost { get; set; }
        public SegmentType Segment { get; set; }
        public string ResourceCode { get; set; }
        public int? ResourceID { get; set; }
        public string PerformingOrg { get; set; }
        public int? PerformingOrgID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public SpreadCurves? SpreadCurveID { get; set; }
        public decimal PercentSpread { get; set; }
        public bool PercentSpreadLocked { get; set; }
        public int HourSpread { get; set; }
        public string UpdateDateLong { get; set; }
        public int BOETaskElementID { get; set; }
        public RateType RateType { get; set; }
        public bool HourSpreadLocked { get; set; }
        public decimal CostSpread { get; set; }
        public bool Deleted { get; set; }
    }
}