// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.ValidationAttributes;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;

    public class TaskElementDetailModelView : PersistedDataModelView
    {
        public TaskElementDetailModelView()
        {
            this.BOETaskElementOrder = 2000;
            this.TaskElementDetailID = -1;
            this.TaskID = string.Empty;
            this.Title = string.Empty;
            this.TaskDescription = string.Empty;
            this.StartDate = "01/1970";
            this.EndDate = "01/1970";
            this.MOQHoursEquation = string.Empty;
            this.MOQType = MOQType.None;
            this.MOQText = string.Empty;
            this.UpdatedByUserID = 0;
            this.BOEID = -1;
            this.TaskOrdinaryVariables = new Collection<BoeTaskOrdinaryVariableModelView>();
            this.WorkspaceVariableIDs = new Collection<int>();
            this.Deleted = false;
            this.CustomFieldValues = new Collection<CustomFieldSelectionModelView>();
            this.LaborTypeWarning = false;
            this.RteTemplateAnswers = new List<RTECustomTemplateQuestionAnswerModelView>();
        }

        public TaskElementDetailModelView(BoeTaskElementDTO inBoeTaskElement)
            : this()
        {
            if (inBoeTaskElement != null)
            {
                this.BOETaskElementOrder = inBoeTaskElement.BOETaskElementOrder;
                this.BOEID = inBoeTaskElement.BoeID;
                this.TaskElementDetailID = inBoeTaskElement.Id;
                this.TaskID = inBoeTaskElement.BOETaskID;
                this.Title = inBoeTaskElement.TaskTitle;
                this.TaskDescription = inBoeTaskElement.Description;
                this.StartDate = inBoeTaskElement.StartDate.HasValue ? inBoeTaskElement.StartDate.Value.ToString("MM/yyyy") : string.Empty;
                this.EndDate = inBoeTaskElement.EndDate.HasValue ? inBoeTaskElement.EndDate.Value.ToString("MM/yyyy") : string.Empty;
                this.MOQType = inBoeTaskElement.MOQType;
                this.MOQText = inBoeTaskElement.MOQText;
                this.MOQHoursEquation = inBoeTaskElement.MOQHoursEquation;
                this.UpdateDate = inBoeTaskElement.UpdateDate;
                this.LaborTypeWarning = inBoeTaskElement.LaborTypeWarningFlag.HasValue ? inBoeTaskElement.LaborTypeWarningFlag.Value : false;
                this.CustomFieldValues = new Collection<CustomFieldSelectionModelView>();
				this.AuthorUserId = inBoeTaskElement.AuthorUserId;
            }
        }

        /// <summary>
        /// Gets or sets the BOE task element id.
        /// </summary>
        public int? TaskElementDetailID { get; set; }
        
        [Required(ErrorMessage= "Task Title is required.")]
        [StringLength(ValidationConstants.MAX_TASK_TITLE_LENGTH, ErrorMessage = "A maximum of 100 characters are allowed for the Title.")]
        public string Title { get; set; }

        /// <summary>
        /// Task Description
        /// </summary>
        [Required(ErrorMessage = "Task Description is required.")]
        [HtmlTextLength(ValidationConstants.MAX_TASK_DESC_LENGTH)]
        [RichText(RichTextDbColumn.BOE_TASK_ELEMENT_TASK_DESCRIPTION, "TaskElementDetailID")]
        [Display(Name = "Task Description")]
        public string TaskDescription { get; set; }

        [Required(ErrorMessage= "Task Start Date is required.")]
        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "Start Date must be in MM/YYYY format.")]        
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public string StartDate { get; set; }

        [Required(ErrorMessage = "Task End Date is required.")]
        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "End Date must be in MM/YYYY format.")]        
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public string EndDate { get; set; }

        public int UpdatedByUserID { get; set; }

        public int BOEID { get; set; }

        [RegularExpression(@"[a-zA-Z\d]\d{0,2}", ErrorMessage = "Task ID must be of the form 'NNN' or 'ANN' where N is numeric and A is alphabetic.")]   
        public string TaskID { get; set; }

        // This regular expression represents characters NOT allowed in MOQ Equation
        // \x22 represents "
        [RegularExpression(@"^[^!#&:;<=>?@\'{|}~[\\\]\x22]{1,250}$", ErrorMessage = "An invalid character was entered.")]
        [StringLength(250, ErrorMessage = "A maximum of 250 characters are allowed for the MOQ Equation.")]
        public string MOQHoursEquation { get; set; }

        public MOQType MOQType { get; set; }

        [HtmlTextLength(ValidationConstants.MAX_MOQ_TEXT_LENGTH)]
        [RichText(RichTextDbColumn.BOE_TASK_ELEMENT_MOQ_TEXT, "TaskElementDetailID")]
        [Display(Name = "MOQ Text")]
        public string MOQText { get; set; }

        public bool Deleted { get; set; }

        public Collection<BoeTaskOrdinaryVariableModelView> TaskOrdinaryVariables { get; set; }

        public Collection<int> WorkspaceVariableIDs { get; set; }

        public Collection<CustomFieldSelectionModelView> CustomFieldValues { get; set; }

        public bool LaborTypeWarning { get; set; }

        /// <summary>
        /// The value of the order in which the task will appear in the boe listing
        /// </summary>
        public int BOETaskElementOrder { get; set; }

        /// <summary>
        /// Gets or sets the RTE Custom Template Answers at Task level. Used during the Save from the UI.
        /// </summary>
        public ICollection<RTECustomTemplateQuestionAnswerModelView> RteTemplateAnswers { get; set; }

		/// <summary>
		/// Author User Id
		/// </summary>
		public int? AuthorUserId { get; set; }

		/// <summary>
		/// Get or set RTE field size character limit
		/// </summary>
		public int RteSizeLimit { get; set; }

		#region ISGS Versus SSC terminology

		public string MOQEquationLabel
        {
            get
            {
                return CommonConstants.BOE_MOQ_EQUATION_LABEL;
            }
        }
        #endregion
    }
}
