// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System.Collections.ObjectModel;

    public class BOECustomFieldModelView : PersistedDataModelView
    {
        public BOECustomFieldModelView() {
            this.CustomFieldOptions = new Collection<BOECustomFieldOptionModelView>();
        }

        public BOECustomFieldsInUseGridModelView CustomFieldMetaData { get; set; }
        public Collection<BOECustomFieldOptionModelView> CustomFieldOptions { get; set; }

        /// <summary>
        /// Whether the user has explicitly confirmed (via popup) that it is OK to proceed with committing changes.
        /// </summary>
        public bool UserHasConfirmed { get; set; }

        /// <summary>
        /// Whether Workspace is using Template BOE
        /// </summary>
        public bool UsingTemplateBoe { get; set; }
    }
}