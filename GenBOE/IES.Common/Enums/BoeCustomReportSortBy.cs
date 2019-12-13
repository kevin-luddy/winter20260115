namespace IES.Common
{
    using System.ComponentModel;

    /// <summary>
    /// Used to specify sort order for the BOE Custom Report selector dialog
    /// </summary>
    public enum BoeCustomReportSortBy
    {
        [Description("--- Select Sort by Criteria ---")]
        SelectSortByCriteria,
        
        [Description("WBS")]
        WBS,

        [Description("CLIN")]
        CLIN,

        [Description("Author")]
        Author,

        [Description("BOE Title")]
        BOETitle,

        [Description("BOE Custom Field")]
        BOECustomField
    }
}
