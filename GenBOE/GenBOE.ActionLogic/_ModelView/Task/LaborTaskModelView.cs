// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Common;

    public class LaborTaskModelView
    {
        public LaborTaskModelView()
        {
            this.MetricsPagingActionName = string.Empty;
            this.DescriptionTemplateAnswers = new List<RTECustomTemplateQuestionAnswerModelView>();
        }

        #region Dropdown values 

        /// <summary>
        /// Gets or sets the moq types.
        /// </summary>
        public string MOQTypes { get; set; }

        #endregion Dropdown values 

        /// <summary>
        /// Gets or sets the task element identifier.
        /// </summary>
        public int? TaskElementId { get; set; }

        /// <summary>
        /// Gets or sets the BOE identifier.
        /// </summary>
        public int BoeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether BOE is multiclinwbs.
        /// </summary>
        public bool BOEIsMulti { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is offload workspace.
        /// </summary>
        public bool IsOffloadWorkspace { get; set; }

        /// <summary>
        /// Gets or sets the moq equation label.
        /// </summary>
        public string MOQEquationLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user has permission to date shift tasks.
        /// </summary>
        public bool AllowDateShift { get; set; }

        /// <summary>
        /// Gets or sets the state of the BOE.
        /// </summary>
        public int BOEState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is OCI.
        /// </summary>
        public bool ContainsOci { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this task element contains discrete spreads.
        /// </summary>
        public bool ContainsDiscrete { get; set; }

        /// <summary>
        /// Number of decimal places for resource costs.
        /// </summary>
        public int CostDecimalPrecision { get; set; }

        /// <summary>
        /// String format used by the UI for cost decimal precision.
        /// </summary>
        public string CostPrecisionStringFormat
        {
            get
            {
                return Utilities.CostPrecisionFormattingString(this.CostDecimalPrecision);
            }
        }

        /// <summary>
        /// Number of decimal places for resources.
        /// </summary>
        public int? ResourceDecimalPrecision { get; set; }

        /// <summary>
        /// String format used by the UI for decimal precision.
        /// </summary>
        public string DecimalPrecisionStringFormat
        {
            get
            {
                return Utilities.PrecisionFormattingString(this.ResourceDecimalPrecision);
            }
        }

        /// <summary>
        /// Gets or sets the hours label.
        /// </summary>
        public string HoursLabel { get; set; }

        /// <summary>
        /// Returns a company specific search parameters for the metrics search dialog. 
        /// </summary>
        /// <returns>Search dialog parameters.</returns>
        public MetricsSearchDialogParametersModelView MetricsSearchDialogParameters { get; set; }

        /// <summary>
        /// Company specific paging action for paging thru metrics search results.
        /// </summary>
        public string MetricsPagingActionName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [labor type warning].
        /// </summary>
        public bool LaborTypeWarning { get; set; }

        /// <summary>
        /// Gets or sets the RTE Custom Template Answers at Task level.
        /// </summary>
        public ICollection<RTECustomTemplateQuestionAnswerModelView> DescriptionTemplateAnswers { get; set; }

        /// <summary>
        /// Gets or sets the Task Description.
        /// </summary>
        public string TaskDescription { get; set; }
    }
}
