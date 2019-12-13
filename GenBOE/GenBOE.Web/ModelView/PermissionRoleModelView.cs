using IES.Common;

namespace GenBOE.Web.ModelView
{
    public class PermissionRoleModelView
    {
        public PermissionRoleModelView()
        {
            RoleID = (int)Role.WorkspaceUser;
            RoleName = string.Empty;
        }

        /// <summary>
        /// Role name
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// ID of the role
        /// </summary>
        public int RoleID { get; set; }
    }
}
