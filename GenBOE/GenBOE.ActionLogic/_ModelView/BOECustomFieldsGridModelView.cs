// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System.ComponentModel.DataAnnotations;
    using IES.Common;
    using GenBOE.Dtos;

    /// <summary>
    /// Model View for the BOE Custom Fields Grid
    /// </summary>
    public class BOECustomFieldsGridModelView : PersistedDataModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BOECustomFieldsGridModelView()
        {
            this.CustomFieldID = -1;
            this.FieldName = string.Empty;
            this.isRequired = false;
            this.inUse = false;
            this.isOpenEnded = false;
        }

        /// <summary>
        /// Constructor taking in a Custom Field DTO
        /// </summary>
        /// <param name="customFieldDTO">Custom Field DTO</param>
        public BOECustomFieldsGridModelView(CustomFieldDTO customFieldDTO)
            : this()
        {
            if (customFieldDTO != null)
            {
                this.FieldName = customFieldDTO.CustomFieldName;
                this.CustomFieldID = customFieldDTO.Id;
                this.CustomFieldDisplayID = customFieldDTO.CustomFieldDisplayID;
                this.isRequired = customFieldDTO.CustomFieldRequired;
                this.UpdateDate = customFieldDTO.UpdateDate;
                this.inUse = false;
                this.isOpenEnded = customFieldDTO.IsOpenEnded;
            }
        }

        /// <summary>
        /// Gets/Sets the Custom Field Name
        /// </summary>
        [Required(ErrorMessage = "Field Name is required.")]
        [StringLength(20, ErrorMessage = "A maximum of 20 characters are allowed")]
        public string FieldName { get; set; }

        /// <summary>
        /// Gets/Sets the Custom Field ID
        /// </summary>
        public int CustomFieldID { get; set; }

        /// <summary>
        /// Gets/Sets bool noting if Custom Field is required
        /// </summary>
        public bool isRequired { get; set; }

        /// <summary>
        /// Gets/Sets bool noting if Custom Field is in use
        /// </summary>
        public bool inUse { get; set; }

        /// <summary>
        /// Get/Sets bool noting if Custom Field is open ended
        /// </summary>
        public bool isOpenEnded { get; set; }

        /// <summary>
        /// Gets/Sets Custom Field Display ID (Level)
        /// </summary>
        [Required(ErrorMessage = "Display Field for Level is required.")]
        public CustomFieldType CustomFieldDisplayID { get; set; }
    }
}
