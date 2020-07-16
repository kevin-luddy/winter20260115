// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.ValidationAttributes;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    public class BOETravelElementDetailsModelView : PersistedDataModelView
    {
        public string DisplayEvent { get; set; }

        public BOETravelElementDetailsModelView()
        {
            this.BOEID = -1;
            this.TravelID = -1;
            this.TaskID = string.Empty;
            this.TaskTitle = string.Empty;
            this.TravelTaskDescription = string.Empty;
            this.StartDate = string.Empty;
            this.EndDate = string.Empty;

            this.CustomFieldValues = new Collection<CustomFieldSelectionModelView>();
        }

        public BOETravelElementDetailsModelView(TravelDTO thisTravel)
            : this()
        {
            if (thisTravel == null) { throw new ArgumentNullException(nameof(thisTravel)); }

            this.BOEID = thisTravel.BoeID;
            this.TravelID = thisTravel.Id;
            this.TaskID = thisTravel.TaskID;
            this.TaskTitle = thisTravel.TaskTitle;
            this.TravelTaskDescription = thisTravel.Description;
            this.StartDate = thisTravel.StartDate.HasValue ? thisTravel.StartDate.Value.ToString("MM/yyyy") : string.Empty;
            this.EndDate = thisTravel.EndDate.HasValue ? thisTravel.EndDate.Value.ToString("MM/yyyy") : string.Empty;
            this.UpdateDate = thisTravel.UpdateDate;
                
            foreach (CustomFieldValueContainer container in thisTravel.CustomFieldValueContainers)
            {
                this.CustomFieldValues.Add(new CustomFieldSelectionModelView
                {
                    CustomFieldValueID = container.CustomFieldValueID,
                    SelectionID = container.ContainerID,
                    UpdateDate = container.UpdateDate,
                    CustomFieldID = container.CustomFieldID,
                    IsOpenEnded = container.IsOpenEnded,
                    OpenEndedValue = container.OpenEndedValue
                });
            }
            
        }

        public int? BOEID { get; set; }

        public int TravelID { get; set; }

        //[RegularExpression(@"[a-zA-Z\d]\d{0,2}", ErrorMessage = "Task ID must be of the form 'NNN' or 'ANN' where N is numeric and A is alphabetic.")]
        [StringLength(3, ErrorMessage = "A maximum of 3 characters are allowed.")]
        public string TaskID { get; set; }

        [Required(ErrorMessage = "Task Title is required.")]
        [StringLength(ValidationConstants.MAX_TASK_TITLE_LENGTH, ErrorMessage = "A maximum of 100 characters are allowed for the Title.")]
        public string TaskTitle { get; set; }

        /// <summary>
        /// Travel task description
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_TASK_DESC_LENGTH)]
        [RichText(RichTextDbColumn.TRAVEL_TRIP_TASK_ELEMENT_TRAVEL_TASK_DESCRIPTION, "TravelID")]
        [Display(Name = "Description")]
        public string TravelTaskDescription { get; set; }

        [Required(ErrorMessage = "Task Start Date is required.")]
        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "Start Date must be in MM/YYYY format.")]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public string StartDate { get; set; }

        [Required(ErrorMessage = "Task End Date is required.")]
        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "End Date must be in MM/YYYY format.")]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public string EndDate { get; set; }

        public Collection<CustomFieldSelectionModelView> CustomFieldValues { get; set; }
        /// <summary>
        /// Gets and sets the customfieldoptions for the travel element.
        /// </summary>
        public Collection<BOECustomFieldModelView> CustomFieldOptions { get; set; }
    }
}
