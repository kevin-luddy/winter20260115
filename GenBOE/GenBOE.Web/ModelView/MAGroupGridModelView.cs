using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using GenBOE.ActionLogic.ModelView;

namespace GenBOE.Web.ModelView
{
    [ExcludeFromCodeCoverage]
    public class MAGroupGridModelView : PersistedDataModelView
    {
        public MAGroupGridModelView()
        {
            GroupName = String.Empty; ;
            GroupID = -1;
            GroupDescription = String.Empty;
            UsersInGroup = new Collection<PermissionsGridModelView>();
            InUse = false;
        }

        /// <summary>
        /// Group name of this element
        /// </summary>
        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed")]
        [Required(ErrorMessage = "Group Name is required.")]
        public string GroupName { get; set; }

        [StringLength(50, ErrorMessage = "A maximum of 50 characters are allowed")]
        public string GroupDescription { get; set; }
        
        public Collection<PermissionsGridModelView> UsersInGroup{ get; set; }
        /// <summary>
        /// Group ID for the group name for the element
        /// </summary>
        public int GroupID { get; set; }

        public bool InUse { get; set; }

    }// end PermissionsGridModelView

}
