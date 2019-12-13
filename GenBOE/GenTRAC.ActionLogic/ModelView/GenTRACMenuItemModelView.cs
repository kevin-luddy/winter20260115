// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView
{
    using System;
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// Model View for menu items.
    /// </summary>
    public class GenTRACMenuItemModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public GenTRACMenuItemModelView()
        {
            this.LinkText = string.Empty;
            this.ActionName = string.Empty;
            this.ControllerName = string.Empty;
            this.RouteValues = null;
            this.HtmlAttributes = null;
            this.SecurityPage = PtmSecurityPage.None;
            this.SubMenuItems = new List<GenTRACMenuItemModelView>();
            this.MenuLocation = MenuLocation.Left;
            this.RouteName = string.Empty;
        }

        /// <summary>
        /// Text to display
        /// </summary>
        public string LinkText { get; set; }

        /// <summary>
        /// Action to call
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// Controller for that action
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        /// Any extra route values
        /// </summary>
        public object RouteValues { get; set; }

        /// <summary>
        /// Any HTML Attributes on the link
        /// </summary>
        public object HtmlAttributes { get; set; }

        /// <summary>
        /// The security page
        /// </summary>
        public PtmSecurityPage SecurityPage { get; set; }

        /// <summary>
        /// The link URL
        /// </summary>
        public Uri LinkUrl { get; set; }

        /// <summary>
        /// Any submenu items
        /// </summary>
        public ICollection<GenTRACMenuItemModelView> SubMenuItems { get; set; }

        /// <summary>
        /// The menu location
        /// </summary>
        public MenuLocation MenuLocation { get; set; }

        /// <summary>
        /// The route name
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// Whether or not to select the item.
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// The action when the link is clicked.
        /// </summary>
        public string OnClickAction { get; set; }
    }
}
