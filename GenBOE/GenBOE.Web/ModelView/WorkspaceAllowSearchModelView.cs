using System;
using GenBOE.ActionLogic.ModelView;
using GenBOE.Dtos;

namespace GenBOE.Web.ModelView
{
    public class WorkspaceAllowSearchModelView : PersistedDataModelView
    {
        public WorkspaceAllowSearchModelView() : base()
        {
        }

        public WorkspaceAllowSearchModelView(WorkspaceDTO inWorkspaceDTO) : base ()
        {
            if (inWorkspaceDTO == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceDTO));
            }

            AllowSearch = inWorkspaceDTO.AllowSearch;
            ContainsTemplate = inWorkspaceDTO.ContainsTemplate;
            UpdateDate = inWorkspaceDTO.UpdateDate;
        }

        public bool AllowSearch { get; set; }
        public bool ContainsTemplate { get; set; }

    }
}
