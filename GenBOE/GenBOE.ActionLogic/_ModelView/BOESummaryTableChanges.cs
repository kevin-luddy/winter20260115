namespace GenBOE.ActionLogic.ModelView
{
    using IES.Common;

    /// <summary>
    /// Changes that need to be applied to the BOE Summary table on the UI.
    /// </summary>
    public class BOESummaryTableChanges
    {
        public bool IsNew { get; set; }
        public string LaborType { get; set; }
        public ElementOfCostType? Category { get; set; }
        public decimal? TotalHours { get; set; }
        public decimal? TotalCost { get; set; }

        /// <summary>
        /// The total number of resource entries that are "rolled-up" into this row
        /// </summary>
        public int? RollupCount { get; set; }

        // Actions
        public bool Delete { get; set; }

        /// <summary>
        /// Whether to hide or show the row on the UI
        /// </summary>
        public bool Hide { get { return (this.Category.HasValue && this.Category.Value != ElementOfCostType.NotSet) && (!this.TotalHours.HasValue || this.TotalHours.Value == 0L) && (!this.TotalCost.HasValue || this.TotalCost.Value == 0m); } }
    }
}
