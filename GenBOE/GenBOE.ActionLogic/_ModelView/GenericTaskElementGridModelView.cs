// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using GenBOE.DataBridge.DTO;
    using IES.Common;

    public class GenericTaskElementGridModelView
    {
        public string DisplayEvent { get; set; }
        public string DeleteAction { get; set; }

        public Collection<GenericTaskElementGridRow> TaskElements { get; set; }
    }

    public class GenericTaskElementGridRow : PersistedDataModelView
    {
        public GenericTaskElementGridRow()
        {
            this.TaskElementDetailID = -1;
            this.TaskID = string.Empty;
            this.Title = string.Empty;
            this.StartDate = DateTime.MinValue;
            this.EndDate = DateTime.MinValue;
            this.TotalHours = 0;
            this._totalCost = 0;
            this.Deleted = false;
            this.TaskType = TaskElementType.None;
            this.ResourceDecimalPrecision = 0;
            this.FailedValidation = false;
        }

        public GenericTaskElementGridRow(BoeTaskElementDTO inBoeTaskElement)
            : this()
        {
            if (inBoeTaskElement != null)
            {
                this.TaskElementDetailID = inBoeTaskElement.Id;
                this.TaskID = inBoeTaskElement.BOETaskID;
                this.Title = inBoeTaskElement.TaskTitle;
                this.StartDate = inBoeTaskElement.StartDate;
                this.EndDate = inBoeTaskElement.EndDate;
                this.TotalHours = inBoeTaskElement.TotalHours;
                this.Deleted = false;
                this.TaskType = inBoeTaskElement.TaskElementType;
                this._totalCost = inBoeTaskElement.TotalCost;
                this.BOETaskElementOrder = inBoeTaskElement.BOETaskElementOrder;
                this.FailedValidation = false;
            }
        }

        [Required]
        public int? TaskElementDetailID { get; set; }

        /// <summary>
        /// The value of the order in which the task will appear in the boe listing
        /// </summary>
        public int BOETaskElementOrder { get; set; }

        public string Title { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime? StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime? EndDate { get; set; }

        public decimal? TotalHours { get; set; }

        private decimal? _totalCost;

        [DisplayFormat(DataFormatString = "{0:N}")]
        public decimal? TotalCost
        {
            get
            {
                return this._totalCost ?? 0;
            }
            set
            {
                this._totalCost = value;
            }
        }

        public string TaskID { get; set; }
        public bool Deleted { get; set; }

        public TaskElementType TaskType { get; set; }

        /// <summary>
        /// Number of decimal places for resources.
        /// </summary>
        public int ResourceDecimalPrecision { get; set; }

        /// <summary>
        /// Gets Total Hours in the correct format as defined for the workspace.
        /// </summary>
        public string TotalHoursFormatted
        {
            get
            {
                return Utilities.FormatStringWithPrecision(this.TotalHours.HasValue ? this.TotalHours.Value : 0, this.ResourceDecimalPrecision);
            }
        }

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

        public override string ToString()
        {
            return String.Format("TaskElementGridModelView ID {0} TaskElementDetailID {1} Title {2} StartDate {3} EndDate {4} TotalHours {5} TotalCost {6} Deleted {7}", this.TaskID, this.TaskElementDetailID, this.Title, this.StartDate, this.EndDate, this.TotalHours, this.TotalCost, this.Deleted);
        }

        public bool FailedValidation { get; set; }
    }
}
