// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Reporting
{
    using System.Collections.Generic;
    using GenBOE.Objects;

    /// <summary>
    /// MV for the Boe Discrepancy Report
    /// </summary>
    public class BoeDiscrepancyReportModelView
    {
        /// <summary>
        /// Boe Id
        /// </summary>
        public int BoeId { get; set; }

        /// <summary>
        /// Full Boe that's the issue
        /// </summary>
        public string BoeTitle { get; set; }

        /// <summary>
        /// Boe Authors
        /// </summary>
        public string BoeAuthors { get; set; }

        /// <summary>
        /// Clin for the BOE (if any)
        /// </summary>
        public string Clin { get; set; }

        /// <summary>
        /// Wbs for the BOE (if any)
        /// </summary>
        public string Wbs { get; set; }

        /// <summary>
        /// Task Elements with issues
        /// </summary>
        public ICollection<BoeTaskDetailsMV> ElementsWithIssues { get; set; }
    }

    /// <summary>
    /// Task Info for the report
    /// </summary>
    public class BoeTaskDetailsMV
    {
        /// <summary>
        /// Actual Task Id (from the DB)
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Displayed (user entered) Task Id
        /// </summary>
        public string DisplayedTaskId { get; set; }

        /// <summary>
        /// Task Title
        /// </summary>
        public string TaskTitle { get; set; }

        /// <summary>
        /// Either cost or labor
        /// </summary>
        public BoeInconsistencyEnum DiscrepancyEnum { get; set; }

        public bool IsUsingEquivalentPerson { get; set; }

        /// <summary>
        /// Returns text based on the Discrepancy Type
        /// </summary>
        public string InconsistencyText
        {
            get
            {
                string result = string.Empty;
                string hoursLabel = FullObjectHelper.ShowEquivalentPersonsOption && this.IsUsingEquivalentPerson ? "EP" : "Hours";

                if (this.DiscrepancyEnum == BoeInconsistencyEnum.Cost)
                {
                    result = "Cost";
                }
                else if (this.DiscrepancyEnum == BoeInconsistencyEnum.Hours)
                {
                    result = hoursLabel;
                }
                else if (this.DiscrepancyEnum == BoeInconsistencyEnum.HoursAndCost)
                {
                    result = "Cost and " + hoursLabel;
                }
                else if (this.DiscrepancyEnum == BoeInconsistencyEnum.ODC)
                {
                    result = "ODC";
                }
				else if (this.DiscrepancyEnum == BoeInconsistencyEnum.Resource)
				{
					result = "One or more Labor Type missing Resource and/or Business Resource Code";
				}

                return result;
            }
        }
    }

    /// <summary>
    /// Enum for the BOE Incosistency Report
    /// </summary>
    public enum BoeInconsistencyEnum
    {
        Hours,

        Cost,

        HoursAndCost,

        ODC,

		Resource
    }
}
