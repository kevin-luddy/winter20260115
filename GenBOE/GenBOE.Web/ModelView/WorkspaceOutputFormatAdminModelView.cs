using System;
using System.ComponentModel.DataAnnotations;
using GenBOE.ActionLogic.ModelView;
using GenBOE.Dtos;

namespace GenBOE.Web.ModelView
{
    public class WorkspaceOutputFormatAdminModelView : PersistedDataModelView
    {
        public WorkspaceOutputFormatAdminModelView()
        {
        }

        public WorkspaceOutputFormatAdminModelView(WorkspaceDTO inWorkspaceDTO)
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

        [Required(ErrorMessage = "Template Name is required.")]
        [StringLength(100, ErrorMessage = "A maximum of 100 characters are allowed")]
        public string TemplateName { get; set; }

        [Required(ErrorMessage = "Template Description is required.")]
        [StringLength(500, ErrorMessage = "A maximum of 500 characters are allowed")]
        public string TemplateDescription { get; set; }

        [Required(ErrorMessage = "Template file is required.")]
        [RegularExpression(@".*docx$", ErrorMessage = "Template file must be in <b>.docx</b> format")]
        public string TemplateFileLocation { get; set; }
    }
}