using System;
using System.ComponentModel.DataAnnotations;
using GenBOE.ActionLogic.ModelView;
using GenBOE.Dtos;

namespace GenBOE.Web.ModelView
{
    public class WorkspaceOutputFormatModelView : PersistedDataModelView
    {
        public WorkspaceOutputFormatModelView()
        {
        }

        public WorkspaceOutputFormatModelView(WorkspaceDTO inWorkspaceDTO)
            : base()
        {
            if (inWorkspaceDTO == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceDTO));
            }

            TemplateID = inWorkspaceDTO.TemplateID;
            UpdateDate = inWorkspaceDTO.UpdateDate;
        }

        public int TemplateID { get; set; }

        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string TemplateName { get; set; }

        [StringLength(500, ErrorMessage = "A maximum of 500 characters are allowed")]
        public string TemplateDescription { get; set; }
        public string TemplateFileLocation { get; set; }

        public string ActiveUserDisplayName { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        public string ActiveUserPhoneNumber { get; set; }

        [Required(ErrorMessage = "Need-by Date is required.")]
        [RegularExpression(@"([0]?[1-9]|1[0-2])/(0?[1-9]|[12][0-9]|3[01])/([1-2]\d{3})", ErrorMessage = "Need-by Date must be in mm/dd/yyyy format.")]
        public string RequestNeedByDate { get; set; }

        [Required(ErrorMessage = "Template Name is required.")]
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string RequestTemplateName { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "A maximum of 500 characters are allowed")]
        public string RequestTemplateDescription { get; set; }

        public string RequestComments { get; set; }
    }
}