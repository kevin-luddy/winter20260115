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
        [Description("Task - Custom Field(s)")]
        TaskCustomFields = 11,
        [Description("Task - Description")]
        TaskDescription = 12,
        [Description("Task - Method of Quoting Type")]
        TaskMOQType = 13,
        [Description("Task - Method of Quoting Equation")]
        TaskMOQEquation = 14,
        [Description("Task - Method of Quoting Historical Prompts")]
        TaskMOQHistoricalPrompts = 15,
        [Description("Task - Method of Quoting Comparative Analysis Prompts")]
        TaskMOQComparativeAnalysisPrompts = 16,
        [Description("Task - Method of Quoting Additional Query Filters")]
        TaskMOQAdditionalQueryFilters = 17,
        [Description("Task - Method of Quoting CERs Prompts")]
        TaskMOQCERsPrompts = 18,
        [Description("Task - Method of Quoting Parametric Estimates Prompts")]
        TaskMOQParametricEstimatesPrompts = 19,
        [Description("Task - Method of Quoting ARs Prompts")]
        TaskMOQARsPrompts = 20,
        [Description("Task - Method of Quoting SOW Prompts")]
        TaskMOQSOWPrompts = 21,
        [Description("Task - Method of Quoting LOE Prompts")]
        TaskMOQLOEPrompts = 22,
        [Description("Task - Method of Quoting SME Judgement Prompts")]
        TaskMOQSMEJudgementPrompts = 23,
        [Description("Task - Method of Quoting Rationale")]
        TaskMOQRationale = 24,
        [Description("Task - Method of Quoting Skill Mix Rationale")]
        TaskMOQSkillMixRationale = 25,
        [Description("Task - Resource Types Summary Table")]
        TaskResourceTypesSummaryTable = 26,
        [Description("Task - Cost/Hours Spread Tables")]
        TaskSpreadTables = 27,
        [Description("Resource - Information and Spread Tables")]
        ResourceInfoAndSpreadTables = 28
    }
}
