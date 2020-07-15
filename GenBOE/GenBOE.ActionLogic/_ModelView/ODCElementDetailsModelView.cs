// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.ActionLogic.ModelView
{
    using System.ComponentModel.DataAnnotations;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.ValidationAttributes;
    using GenBOE.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Model view for ODC element details
    /// </summary>
    [StartEndDateValidation(StartDate = "StartDate", EndDate = "EndDate", CanBeEqual = true, ErrorMessage = "Start Date must be before the End Date")]
    public class ODCElementDetailsModelView : PersistedDataModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ODCElementDetailsModelView()
        {
            this.ODCID = -1;
            this.TaskID = string.Empty;
            this.TaskTitle = string.Empty;
            this.ODCTaskDescription = string.Empty;
            this.ODCMOQText = string.Empty;
            this.BOEID = -1;
            this.StartDate = string.Empty;
            this.EndDate = string.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inODCDTO">The <see cref="OtherDirectCostDTO"/> used to populate the model view</param>
        public ODCElementDetailsModelView(OtherDirectCostDTO inODCDTO)
            : this()
        {
            if (inODCDTO != null)
            {
                this.ODCID = inODCDTO.Id;
                this.TaskID = inODCDTO.TaskID;
                this.TaskTitle = inODCDTO.TaskTitle;
                this.ODCTaskDescription = inODCDTO.TaskDescription;
                this.ODCMOQText = inODCDTO.MoqText;
                this.BOEID = inODCDTO.BoeID;
                this.StartDate = inODCDTO.StartDate.HasValue ? inODCDTO.StartDate.Value.ToString("MM/yyyy") : string.Empty;
                this.EndDate = inODCDTO.EndDate.HasValue ? inODCDTO.EndDate.Value.ToString("MM/yyyy") : string.Empty;
            }
        }

        /// <summary>
        /// Gets/Sets ODCID
        /// </summary>
        public int ODCID { get; set; }

        /// <summary>
        /// Gets/Sets TaskID
        /// </summary>
        [RegularExpression(@"[a-zA-Z\d]\d{0,2}", ErrorMessage = "Task ID must be of the form 'NNN' or 'ANN' where N is numeric and A is alphabetic.")]
        public string TaskID { get; set; }

        /// <summary>
        /// Gets/Sets TaskTitle
        /// </summary>
        [Required(ErrorMessage = "Task Title is required.")]
        [StringLength(ValidationConstants.MAX_TASK_TITLE_LENGTH, ErrorMessage = "A maximum of 100 characters are allowed for the Title.")]
        public string TaskTitle { get; set; }

        /// <summary>
        /// Gets/Sets TaskDescription
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_TASK_DESC_LENGTH)]
        [RichText(RichTextDbColumn.ODC_TASK_ELEMENT_ODC_TASK_DESCRIPTION, "ODCID", Required = true)]
        [Display(Name = "Task Description")]
        public string ODCTaskDescription { get; set; }

        /// <summary>
        /// Gets/Sets ODCMOQText
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_MOQ_TEXT_LENGTH)]
        [RichText(RichTextDbColumn.ODC_TASK_ELEMENT_ODC_MOQ_TEXT, "ODCID")]
        [Display(Name = "MOQ Text")]
        public string ODCMOQText { get; set; }

        /// <summary>
        /// Gets/Sets BOEID
        /// </summary>
        public int BOEID { get; set; }

        /// <summary>
        /// Gets/Sets StartDate
        /// </summary>
        [Required(ErrorMessage = "Task Start Date is required.")]
        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "Start Date must be in MM/YYYY format.")]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public string StartDate { get; set; }

        /// <summary>
        /// Gets/Sets EndDate
        /// </summary>
        [Required(ErrorMessage = "Task End Date is required.")]
        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "End Date must be in MM/YYYY format.")]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public string EndDate { get; set; }

        /// <summary>
        /// Gets/Sets MOQTextLabel
        /// Contains the text displayed on the IS&amp;GS view for the MOQ Text field
        /// </summary>
        public string MOQTextLabel { get; set; }
    }
}
