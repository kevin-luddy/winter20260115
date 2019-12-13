namespace GenBOE.ActionLogic.ModelView.SSRS
{
    /// <summary>
    /// RMS Offload Cost by year report for Project Map workspace types
    /// </summary>
    public class OffloadCostByYearReportRMSModelView
    {
        /// <summary>
        /// WBS
        /// </summary>
        public string Wbs { get; set; }

        /// <summary>
        /// CLIN
        /// </summary>
        public string Clin { get;set; }

        /// <summary>
        /// Activity ID
        /// </summary>
        public string ActivityId { get; set; }

        /// <summary>
        /// Offload Resource
        /// </summary>
        public string OffloadedResource { get; set; }

        /// <summary>
        /// Resource
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Cost Center
        /// </summary>
        public string CostCenter { get; set; }
        
        /// <summary>
        /// Off Load Percent
        /// </summary>
        public decimal OffLoadPercent { get; set; }

        /// <summary>
        /// Sum of Off Load Cost
        /// </summary>
        public decimal SumOfOLCost { get; set; }

        /// <summary>
        /// Total Hours (off load plus in-house hours)
        /// </summary>
        public decimal TotalHours { get; set; }

        /// <summary>
        /// Total Off Load Hours
        /// </summary>
        public decimal TotalOffLoadHours { get; set; }

        /// <summary>
        /// Total in-house hours
        /// </summary>
        public decimal TotalInHouseHours { get; set; }

        /// <summary>
        /// Off Load Year
        /// </summary>
        public string OffloadYear { get; set; }

        /// <summary>
        /// Total hours in a given year
        /// </summary>
        public decimal TotalHoursInYear { get; set; }

        /// <summary>
        /// Hourly Off Load Rate (dollars)
        /// </summary>
        public decimal HourlyOffLoadRate { get; set; }

    }
}