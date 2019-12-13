using System.Linq;
using System.Diagnostics.CodeAnalysis;
using IES.Common;
using System.Collections.ObjectModel;

namespace GenBOE.Web.ModelView
{
    [ExcludeFromCodeCoverage]
    public class PermissionsGridModelView
    {
        private Collection<PermissionRoleModelView> _Roles = new Collection<PermissionRoleModelView>();

        public PermissionsGridModelView()
        {
            Users = new Collection<PermissionUserModelView>();
            _Roles = new Collection<PermissionRoleModelView>();
            isGroup = false;
            groupMembers = new Collection<string>();
            genBOEAccess = "";
        }

        /// <summary>
        /// For this element, the users belonging to the group
        /// </summary>
        public Collection<PermissionUserModelView> Users { get; set; }

        /// <summary>
        /// For this element, the roles belonging to the users contained in the element
        /// </summary>
        public Collection<PermissionRoleModelView> Roles
        {
            get
            {
                if (_Roles != null)
                {
                    // we don't ever want to show the user the 'workspace user' role .. 
                    // it's too confusing.  the database will manage this role automatically
                    // as other roles around it are inserted and deleted.
                    return new Collection<PermissionRoleModelView>(_Roles.Where(x => x.RoleID != (int)Role.WorkspaceUser).Select(y => y).ToArray());
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _Roles = value;
            }
        }

        /// <summary>
        /// If this element is a group or not
        /// </summary>
        public bool isGroup { get; set; }

        /// <summary>
        /// If it's a group, this will get the users in the group
        /// </summary>
        public Collection<string> groupMembers { get; set; }

        /// <summary>
        /// User/Group's access to GenBOE - "Yes" or "No" for users, "View Users" for groups
        /// </summary>
        public string genBOEAccess { get; set; }

    }// end PermissionsGridModelView

}
