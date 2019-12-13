using System;
using System.Diagnostics.CodeAnalysis;
using IES.Common;
using System.Collections.ObjectModel;

namespace GenBOE.Web.ModelView
{
    [ExcludeFromCodeCoverage]
    public class SavePermissionsModelView
    {
        public SavePermissionsModelView()
        {
            EntityIds = new Collection<String>();
            Roles = new Collection<Role>();
            WorkspaceId = -1;
        }

        public int WorkspaceId { get; set; }
        public Collection<String> EntityIds { get; set; }
        public Collection<Role> Roles { get; set; }

        public void setEntityIdString(String inEntityIds)
        {
            EntityIds.Add(inEntityIds);
        }
    }

}
