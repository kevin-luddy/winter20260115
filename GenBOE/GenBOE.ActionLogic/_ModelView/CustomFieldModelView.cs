using IES.Common;

namespace GenBOE.ActionLogic.ModelView
{
    public class CustomFieldModelView
    {
        public CustomFieldModelView()
        {
            this.CustomFieldName = string.Empty;
            this.CustomFieldRequired = false;
            this.WorkspaceID = -1;
        }

        public int CustomFieldID { get; set; }

        public string CustomFieldName { get; set; }

        public CustomFieldType CustomFieldDisplayID { get; set; }        

        public bool CustomFieldRequired { get; set; }

        public int WorkspaceID { get; set; }
    }    
}