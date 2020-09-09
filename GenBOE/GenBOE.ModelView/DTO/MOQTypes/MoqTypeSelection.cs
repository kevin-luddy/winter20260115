// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// MOQ Type selection, with underlying data
    /// </summary>
    public class MoqTypeSelection
    {
        /// <summary>
        /// Task Id that the MOQ Object belongs to
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Boe Id that the task belongs to. The task then owns the MOQ Object. This is needed for the FullWS objects
        /// </summary>
        public int BoeId { get; set; }

        /// <summary>
        /// Selected MOQ Type that corresponds to this 
        /// </summary>
        public MOQType SelectedMOQType { get; set; }

        /// <summary>
        /// Description of the Selected MOQ Type
        /// </summary>
        public string SelectedMOQTypeText
        {
            get
            {
                return this.SelectedMOQType.GetDescription();
            }
        }

        /// <summary>
        /// Rationale
        /// </summary>
        public string Rationale { get; set; }

        /// <summary>
        /// Skill Mix Rationale
        /// </summary>
        public string SkillMixRationale { get; set; }

        /// <summary>
        /// Calculation
        /// </summary>
        public string Calculation { get; set; }

        /// <summary>
        /// SOW Hours / Location
        /// </summary>
        public string SowHoursLocation { get; set; }

        /// <summary>
        /// CER, Parametric model or tool name, or Analogous relationship name
        /// </summary>
        public string CerName { get; set; }

        /// <summary>
        /// CER, Parametric model or tool name, or Analogous relationship location in the proposal
        /// </summary>
        public string CerLocation { get; set; }

        /// <summary>
        /// Description of Hours required
        /// </summary>
        public string DescriptionHoursRequired { get; set; }

        /// <summary>
        /// The SME selected Expert judgement for this basis of estimate for the following reasons
        /// </summary>
        public string SmeReason { get; set; }

        /// <summary>
        /// The logic and assumptions used to estimate hours is
        /// </summary>
        public string SmeHoursLogic { get; set; }

        /// <summary>
        /// The logic and assumptions used to estimate duration is
        /// </summary>
        public string SmeDurationLogic { get; set; }

        /// <summary>
        /// Tasks are estimates
        /// </summary>
        public string SmeTaskEstimates { get; set; }

        /// <summary>
        /// Table Data
        /// </summary>
        public ICollection<MoqTableData> TableData { get; set; } = new List<MoqTableData>();
    }
}
