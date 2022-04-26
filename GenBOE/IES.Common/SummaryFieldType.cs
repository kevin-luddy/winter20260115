// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System.ComponentModel;

    /// <summary>
    /// Different summary fields for the trace table
    /// </summary>
    public enum SummaryFieldType
    {
        /// <summary>
        /// CLIN # 
        /// </summary>
        [Description("CLIN #")]
        CLINNum = 1,

        /// <summary>
        /// WBS #
        /// </summary>
        [Description("WBS #")]
        WBSNum = 2,

        /// <summary>
        /// BOE Title
        /// </summary>
        [Description("BOE Title")]
        BOETitle = 3,

        /// <summary>
        /// Task Description
        /// </summary>
        [Description("Task Description")]
        TaskDescription = 4,

        /// <summary>
        /// Performing Org Id
        /// </summary>
        [Description("Performing Org Id")]
        PerformingOrgId = 5,

        /// <summary>
        /// Resource / Activity Id
        /// </summary>
        [Description("Resource / Activity Id")]
        ResourceOrActivityId = 6,

        /// <summary>
        /// Resource Description
        /// </summary>
        [Description("Resource Description")]
        ResourceDescription = 7,

        /// <summary>
        /// Custom Field
        /// </summary>
        [Description("Custom Field")]
        CustomField = 50
    }
}
