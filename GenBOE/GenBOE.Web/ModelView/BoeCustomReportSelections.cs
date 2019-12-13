namespace GenBOE.Web.ModelView
{
    using System;
    using System.Collections.Generic;
    using IES.Common;

    [Serializable]
    public class BoeCustomReportSelections
    {
        public BoeCustomReportSelections()
        {
            this.BoeSortBy = null;
            this.BoeSortByValue = null;
            this.BoeSecondarySortBy = null;
            this.BoeSecondarySortByValue = null;
            this.BoesSelected = new Dictionary<int, string>();
            this.ComponentsSelected = new List<BoeCustomReportComponent>();
        }

        public string BoeSortBy { get; set; }
        public string BoeSortByValue { get; set; }
        public string BoeSecondarySortBy { get; set; }
        public string BoeSecondarySortByValue { get; set; }
        public IDictionary<int, string> BoesSelected { get; set; }
        public ICollection<BoeCustomReportComponent> ComponentsSelected { get; set; }
    }
}
