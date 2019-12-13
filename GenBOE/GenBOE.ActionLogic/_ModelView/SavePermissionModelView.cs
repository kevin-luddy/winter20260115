using IES.Common;
using System;
using System.Collections.ObjectModel;

namespace GenBOE.ActionLogic.ModelView
{
    public class SavePermissionModelView
    {
        public SavePermissionModelView()
        {
            this.EntityIds = new Collection<String>();
            this.Roles = new Collection<Role>();
            this.WorkspaceId = -1;
        }
       
        public int WorkspaceId { get; set; }
        public Collection<String> EntityIds { get; set; }
        public Collection<Role> Roles { get; set; }

        public void setEntityIdString(String inEntityIds)
        {
            this.EntityIds.Add(inEntityIds);
        }
    }
}
