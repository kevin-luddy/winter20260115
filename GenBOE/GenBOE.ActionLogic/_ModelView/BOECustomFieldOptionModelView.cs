// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using GenBOE.Dtos;

    public class BOECustomFieldOptionModelView : PersistedDataModelView
    {
        public BOECustomFieldOptionModelView()
        {
            this.CustomFieldOptionID = -1;
            this.ID = string.Empty;
            this.Description = string.Empty;
            this.InUse = false;
            this.Deleted = false;
            this.UpdateDate = DateTime.Now;
        }

        public BOECustomFieldOptionModelView(PerformingOrgDTO performingOrg) : this()
        {
            if (performingOrg != null)
            {
                this.CustomFieldOptionID = performingOrg.Id;
                this.ID = performingOrg.PerformingOrgName;
                this.Description = performingOrg.PerformingOrgDesc;
                this.Deleted = false;
                this.UpdateDate = performingOrg.UpdateDate;
            }
        }

        public BOECustomFieldOptionModelView(CustomFieldValueDTO customFieldValueDTO): this()
        {
            if (customFieldValueDTO != null)
            {
                this.CustomFieldOptionID = customFieldValueDTO.CustomFieldValueID;
                this.ID = customFieldValueDTO.CustomFieldValueName;
                this.Description = customFieldValueDTO.CustomFieldValueDescription;
                this.InUse = customFieldValueDTO.CustomFieldValueInUseFlag;
                this.UpdateDate = customFieldValueDTO.UpdateDate;
            }
        }

        public int CustomFieldOptionID { get; set; }

        [Required(ErrorMessage="ID is required.")]
        [StringLength(20, ErrorMessage = "A maximum of 20 characters is allowed")]
        public string ID { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(100, ErrorMessage = "A maximum of 100 characters is allowed")]
        public string Description { get; set; }

        public bool InUse { get; set; }
        public bool Deleted { get; set; }
    }
}
