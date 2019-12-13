using IES.Common;

namespace GenBOE.ActionLogic.ModelView
{
    public class ResourceModelView
    {
        public ResourceModelView()
        {
            this.ResourceID = -1;
            this.ResourceName = string.Empty;
            this.ResourceDesc = string.Empty;
            this.LaborType = string.Empty;
            this.SegRegion = string.Empty;
            this.ResourceListID = -1;
            this.ResourceInUse = false;
            this.BurdenPool = string.Empty;
            this.ElementOfCost = ElementOfCostType.LMLabor;
            this.Segment = SegmentType.None;
        }
        public int ResourceID { get; set; }
        public string ResourceName { get; set; }
        public string ResourceDesc { get; set; }
        public string LaborType { get; set; }
        public string SegRegion { get; set; }
        public int ResourceListID { get; set; }
        public bool ResourceInUse { get; set; }
        public string BurdenPool { get; set; }
        public ElementOfCostType ElementOfCost { get; set; }
        public SegmentType Segment { get; set; }

        //HACK
        // there is an issue with a not null enums and this gets around it
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public int? SegmentTypeInt { set { this.Segment = value.HasValue ? (SegmentType)value.Value : SegmentType.None; } }

        public int getPrimaryKeyID()
        {
            return this.ResourceID;
        }

    }
}
