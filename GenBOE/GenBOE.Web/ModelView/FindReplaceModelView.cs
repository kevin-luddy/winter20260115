using System.ComponentModel.DataAnnotations;

namespace GenBOE.Web.ModelView
{

    public class FindReplaceModelView
    {

        public FindReplaceModelView()
        {
            WorkspaceID = 0;
            WorkspaceName = string.Empty;
        }


        public int WorkspaceID { get; set; }
        public string WorkspaceName { get; set; }

        [Required(ErrorMessage = "Text to find is required.")]
        [StringLength(400, ErrorMessage = "Replace must not exceed 400 chars.")]
        public string FindText { get; set; }

        [Required(ErrorMessage = "Text to Replace is required.")]
        [StringLength(400, ErrorMessage = "Replace must not exceed 400 chars.")]
        public string ReplaceText { get; set; }

    }
}