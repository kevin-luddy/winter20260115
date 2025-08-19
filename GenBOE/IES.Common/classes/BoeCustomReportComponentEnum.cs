namespace IES.Common
{
    using System.ComponentModel;

    /// <summary>
    /// Used to specify which report components to include on the BOE Custom Report
    /// </summary>
    /// <remarks>
    /// =CONCATENATE("[Description(""", A1,""")]")
    /// =SUBSTITUTE(SUBSTITUTE(A1," ", ""), "-", "")
    /// </remarks>
    public enum BoeCustomReportComponent
    {
        [Description("BOE - Proposal/Program Name")]
        BOEProgramName = 2,
        [Description("BOE - WBS #/Title")]
        BOEWBS = 3,
        [Description("BOE - CLIN #/Title")]
        BOECLIN = 4,
        [Description("BOE - Description")]
        BOEDescription = 5,
        [Description("BOE - Custom Field(s)")]
        BOECustomFields = 6,
        [Description("BOE - Sources of Data")]
        BOESourcesofData = 7,
        [Description("BOE - Resource Summary Table")]
        BOEResourceSummaryTable = 8,
        [Description("BOE - Hours/Cost Spread Summary Tables")]
        BOESpreadSummaryTables = 9,
        [Description("BOE - Prepared by/Signature Section")]
        BOESignatures = 10,
		[Description("Task - Author")]
		TaskAuthor = 11,
		[Description("Task - Custom Field(s)")]
        TaskCustomFields = 12,
        [Description("Task - Description")]
        TaskDescription = 13,
        [Description("Task - Method of Quoting Type")]
        TaskMOQType = 14,
        [Description("Task - Method of Quoting Equation")]
        TaskMOQEquation = 15,
        [Description("Task - Method of Quoting Additional Query Filters")]
        TaskMOQAdditionalQueryFilters = 16,
        [Description("Task - Method of Quoting Employee ID Filters")]
        TaskMOQEmployeeIDFilters = 17,
        [Description("Task - Method of Quoting Rationale")]
        TaskMOQRationale = 18,
		[Description("Task - Skill Mix Tables")]
		SkillMixTables = 19,
		[Description("Task - Resource Types Summary Table")]
        TaskResourceTypesSummaryTable = 20,
        [Description("Task - Cost/Hours Spread Tables")]
        TaskSpreadTables = 21,
		[Description("Resource - Information and Spread Tables")]
        ResourceInfoAndSpreadTables = 22,
	}
}