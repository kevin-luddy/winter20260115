// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// The ModelView used for Menu Options.
    /// </summary>
    public class MenuOptionModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MenuOptionModelView()
        {
            this.Label = string.Empty;
            this.Link = string.Empty;
            this.Selected = false;
            this.SubMenuOptions = null;
            this.UserCanAccess = false;
            this.IsAdminMenuOption = false;
        }

        /// <summary>
        /// The Label for the menu option.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The Link for the menu option.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Whether or not the menu option is selected.
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// The list of sub menu options for the associated menu option.
        /// </summary>
        public Collection<MenuOptionModelView> SubMenuOptions { get; set; }

        /// <summary>
        /// Whether or not the user has access to the option.
        /// </summary>
        public bool UserCanAccess { get; set; }

        /// <summary>
        /// Whether or not the option is the Admin menu option
        /// </summary>
        public bool IsAdminMenuOption { get; set; }
    }
}
