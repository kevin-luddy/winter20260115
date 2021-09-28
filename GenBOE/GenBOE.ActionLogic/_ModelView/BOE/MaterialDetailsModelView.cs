// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.BOE
{

    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.ValidationAttributes;
    using GenBOE.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Material Details Model View
    /// </summary>
    public class MaterialDetailsModelView : PersistedDataModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MaterialDetailsModelView()
        {
            this.MaterialID = -1;
            this.TaskID = string.Empty;
            this.TaskTitle = string.Empty;
            this.TaskDescription = string.Empty;
            this.MoqText = string.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inMaterial">A <see cref="MaterialDTO"/> object</param>
        public MaterialDetailsModelView(MaterialDTO inMaterial) : this()
        {
            if (inMaterial != null)
            {
                this.MaterialID = inMaterial.Id;
                this.TaskID = inMaterial.TaskID;
                this.TaskTitle = inMaterial.TaskTitle;
                this.TaskDescription = inMaterial.TaskTitle;
                this.MoqText = inMaterial.MoqText;
                this.BOEID = inMaterial.BoeID;
                this.UpdateDate = inMaterial.UpdateDate;
            }
        }

        /// <summary>
        /// Gets/Sets MaterialID
        /// </summary>
        public int? MaterialID { get; set; }

        /// <summary>
        /// Gets/Sets TaskID
        /// </summary>
        [StringLength(3, ErrorMessage = "A maximum of 3 characters are allowed for the Task ID.")]
        public string TaskID { get; set; }

        /// <summary>
        /// Gets/Sets TaskTitle
        /// </summary>
        [Required(ErrorMessage = "Task Title is required.")]
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed for the Task Title.")]
        public string TaskTitle { get; set; }

        /// <summary>
        /// Gets/Sets TaskDescription
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_TASK_DESC_LENGTH)]
        [RichText(RichTextDbColumn.MATERIAL_TASK_ELEMENT_TASK_DESCRIPTION, "MaterialID")]
        [Display(Name = "Task Description")]
        public string TaskDescription { get; set; }
        
        /// <summary>
        /// Gets/Sets MoqText
        /// </summary>
        [HtmlTextLength(ValidationConstants.MAX_MOQ_TEXT_LENGTH)]
        [RichText(RichTextDbColumn.MATERIAL_TASK_ELEMENT_MATERIAL_MOQ_TEXT, "MaterialID")]
        [Display(Name = "MOQ Text")]
        public string MoqText { get; set; }

        /// <summary>
        /// Gets/Sets BOEID
        /// </summary>
        public int BOEID { get; set; }

        /// <summary>
        /// Gets/Sets StartDate
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets/Sets EndDate
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets/Sets MOQTextLabel
        /// </summary>
        public string MOQTextLabel { get; set; }
    }
}
