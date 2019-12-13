using GenBOE.ActionLogic.ModelView;

namespace GenBOE.Web.ModelView
{
    public class PermissionUserModelView : PersistedDataModelView
    {
        public PermissionUserModelView()
        {
            DisplayName = string.Empty;
            UserID = int.MinValue;
        }

        /// <summary>
        /// Users display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Users id
        /// </summary>
        public int UserID { get; set; }
    }
}