// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.ValidationAttributes;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    public class BOEHeaderDescriptionModelView : PersistedDataModelView
    {
        public BOEHeaderDescriptionModelView()
        {
            this.Description = string.Empty;
            this.BOEID = 0;
            this.RteTemplateAnswers = new List<RTECustomTemplateQuestionAnswerModelView>();
        }

        public BOEHeaderDescriptionModelView(BoeDTO inBoe, ICollection<RTECustomTemplateQuestionAnswerModelView> rteTemplateAnswers)
            : this()
        {
            if (inBoe != null)
            {
                this.Description = inBoe.Description;
                this.StartDate = inBoe.StartDate.ToString("MM/yyyy");
                this.EndDate = inBoe.EndDate.ToString("MM/yyyy");
                this.BOEID = inBoe.Id;
                this.RteTemplateAnswers = rteTemplateAnswers;
            }
        }

        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "Start Date must be in MM/YYYY format.")]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public String StartDate { get; set; }

        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "End Date must be in MM/YYYY format.")]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public String EndDate { get; set; }

        /// <summary>
        /// BOE description
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_BOE_DESC_LENGTH)]
        [RichText(RichTextDbColumn.BOE_BOE_DESCRIPTION, "BOEID")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// BOE ID
        /// </summary>
        /// <remarks>Setter must be public to support model binding (notably for image processing)</remarks>
        public int BOEID { get; set; }

        /// <summary>
        /// Gets or sets the RTE Custom Template Answers at BOE level.
        /// </summary>
        public ICollection<RTECustomTemplateQuestionAnswerModelView> RteTemplateAnswers { get; set; }
    }
}
