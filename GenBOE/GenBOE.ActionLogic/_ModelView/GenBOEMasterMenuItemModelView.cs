// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Common;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Model View for the sites menus
    /// </summary>
    public class GenBOEMasterMenuItemModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public GenBOEMasterMenuItemModelView()
        {
            this.linkText = string.Empty;
            this.actionName = string.Empty;
            this.controllerName = string.Empty;
            this.routeValues = null;
            this.htmlAttributes = null;
            this.securityPage = SecurityPage.None;
            this.subMenuItems = new Collection<GenBOEMasterMenuItemModelView>();
            this.menuLocation = MenuLocation.Left;
            this.routeName = string.Empty;
        }

        /// <summary>
        /// Gets/Sets linkText
        /// </summary>
        public string linkText { get; set; }

        /// <summary>
        /// Gets/Sets actionName
        /// </summary>
        public string actionName { get; set; }

        /// <summary>
        /// Gets/Sets controllerName
        /// </summary>
        public string controllerName { get; set; }

        /// <summary>
        /// Gets/Sets routeValues
        /// </summary>
        public object routeValues { get; set; }

        /// <summary>
        /// Gets/Sets htmlAttributes
        /// </summary>
        public object htmlAttributes { get; set; }

        /// <summary>
        /// Gets/Sets securityPage
        /// </summary>
        public SecurityPage securityPage { get; set; }

        /// <summary>
        /// Gets/Sets linkUrl
        /// </summary>
        public Uri linkUrl { get; set; }

        /// <summary>
        /// Gets/Sets subMenuItems
        /// </summary>
        public Collection<GenBOEMasterMenuItemModelView> subMenuItems { get; set; }

        /// <summary>
        /// Gets/Sets menuLocation
        /// </summary>
        public MenuLocation menuLocation { get; set; }

        /// <summary>
        /// Gets/Sets routeName
        /// </summary>
        public string routeName { get; set; }

        /// <summary>
        /// Builds the Collection of <see cref="GenBOEMasterMenuItemModelView"/> objects for the site menu
        /// </summary>
        /// <param name="ws">The current workspace</param>
        /// <returns>the Collection of <see cref="GenBOEMasterMenuItemModelView"/> objects for the site menu</returns>
        public static ICollection<GenBOEMasterMenuItemModelView> BuildSiteMasterMenuItems(WorkspaceDTO ws)
        {
            if (ws == null) { throw new ArgumentNullException(nameof(ws)); }

            ISecurityInformation securityInformation = GenBOEUnityContainer.Container.Resolve(typeof(ISecurityInformation)) as ISecurityInformation;

            ICollection<GenBOEMasterMenuItemModelView> menuItems = new Collection<GenBOEMasterMenuItemModelView> {
                new GenBOEMasterMenuItemModelView {
                    linkText = "Home",
                    actionName = WebConstants.ACTION_INDEX,
                    controllerName = WebConstants.CONTROLLER_WORKSPACE,
                    routeValues = new { workspace = ws.Shortname },
                    htmlAttributes = null,
                    securityPage = SecurityPage.Home,
                    routeName = WebConstants.ROUTE_WORKSPACE
                },
                new GenBOEMasterMenuItemModelView {
                    linkText="Workspace Administration",
                    routeName = WebConstants.ROUTE_WORKSPACE,
                    subMenuItems = new Collection<GenBOEMasterMenuItemModelView> {
                        new GenBOEMasterMenuItemModelView {
                            linkText = "Workspace Settings",
                            actionName = WebConstants.ACTION_WORKSPACE_SETTINGS,
                            controllerName = WebConstants.CONTROLLER_WORKSPACE,
                            routeValues = new { workspace = ws.Shortname },
                            htmlAttributes = null,
                            securityPage = SecurityPage.WorkspaceSettings,
                            routeName = WebConstants.ROUTE_WORKSPACE
                        },
                        new GenBOEMasterMenuItemModelView {
                            linkText = "Permissions",
                            actionName = WebConstants.ACTION_MANAGE_PERMISSIONS,
                            controllerName = WebConstants.CONTROLLER_PERMISSIONS,
                            routeValues = new { workspace = ws.Shortname },
                            htmlAttributes = null,
                            securityPage = SecurityPage.WorkspaceAdminPermissions,
                            routeName = WebConstants.ROUTE_WORKSPACE
                        },
                        // Hide Import IMS for now via display-none class until we figure out what we are doing with it
                        new GenBOEMasterMenuItemModelView {
                            linkText = "Import IMS",
                            actionName = WebConstants.ACTION_DISPLAY_IMS_IMPORT_PAGE,
                            controllerName = WebConstants.CONTROLLER_WORKSPACE,
                            routeValues = new { workspace = ws.Shortname },
                            htmlAttributes = new { name = "ImportIMSMenuLink", @class="display-none" },
                            securityPage = SecurityPage.WorkspaceSettings,
                            routeName = WebConstants.ROUTE_WORKSPACE
                        },
                        new GenBOEMasterMenuItemModelView {
                            linkText = "Manage CLINs",
                            actionName = WebConstants.ACTION_INDEX,
                            controllerName = WebConstants.CONTROLLER_CLIN,
                            routeValues = new { workspace = ws.Shortname },
                            htmlAttributes = new { name = "ManageCLINsMenuLink" },
                            securityPage = SecurityPage.ManageCLINs,
                            routeName = WebConstants.ROUTE_WORKSPACE
                        },
                        new GenBOEMasterMenuItemModelView {
                            linkText = "Manage WBS",
                            actionName = WebConstants.ACTION_INDEX,
                            controllerName = WebConstants.CONTROLLER_WBS,
                            routeValues = new { workspace = ws.Shortname },
                            htmlAttributes = new { name = "ManageWBSMenuLink" },
                            securityPage = SecurityPage.ManageWBS,
                            routeName = WebConstants.ROUTE_WORKSPACE
                        },

                        new GenBOEMasterMenuItemModelView {
                            linkText = "Manage BOEs",
                            actionName = WebConstants.ACTION_INDEX,
                            controllerName = WebConstants.CONTROLLER_BOE,
                            routeValues = new { workspace = ws.Shortname },
                            htmlAttributes = new { name = "ManageBOEsMenuLink" },
                            securityPage = SecurityPage.ManageBOEs,
                            routeName = WebConstants.ROUTE_WORKSPACE
                        },
                         new GenBOEMasterMenuItemModelView {
                            linkText = "Manage INL Forms",
                            actionName = WebConstants.ACTION_INDEX,
                            controllerName = WebConstants.CONTROLLER_BOE_FORMS,
                            routeValues = new { workspace = ws.Shortname },
                            htmlAttributes = new { name = "ManageINLFormsMenu" },
                            securityPage = SecurityPage.ManageBOEForms,
                            routeName = WebConstants.ROUTE_WORKSPACE
                        },
                        new GenBOEMasterMenuItemModelView {
                            linkText = "Find/Replace BOE Text",
                            actionName = WebConstants.ACTION_INDEX,
                            controllerName = WebConstants.CONTROLLER_FIND_REPLACE,
                            routeValues = new { workspace = ws.Shortname },
                            htmlAttributes = new { name = "FindReplaceBOEMenuLink" },
                            securityPage = SecurityPage.FindReplace,
                            routeName = WebConstants.ROUTE_WORKSPACE
                        },
                        new GenBOEMasterMenuItemModelView {
                            linkText = "Update Zone Travel Rates",
                            actionName = "placeholderAction",
                            controllerName = "placeholderController",
                            routeValues = null,
                            htmlAttributes = new { name = "UpdateZoneTravelRatesLink" },
                            securityPage = SecurityPage.UpdateZoneTravelRatesMenuOption,
                            routeName = WebConstants.ROUTE_WORKSPACE
                        },
                        new GenBOEMasterMenuItemModelView {
                            linkText = "Update Offload Rates",
                            actionName = "placeholderAction",
                            controllerName = "placeholderController",
                            routeValues = null,
                            htmlAttributes = new { name = "UpdateOffloadRatesLink" },
                            securityPage = SecurityPage.UpdateOffloadRatesMenuOption,
                            routeName = WebConstants.ROUTE_WORKSPACE
                        }
                    }
                },
                new GenBOEMasterMenuItemModelView {
                    linkText="Workspace",
                    routeName = WebConstants.ROUTE_WORKSPACE,
                    htmlAttributes = new { name = "WorkspaceMenuLink" },
                    securityPage = SecurityPage.BulkSubmit,
                    subMenuItems = new Collection<GenBOEMasterMenuItemModelView> {
                        new GenBOEMasterMenuItemModelView {
                            linkText = "BOE Bulk Submit",
                            actionName = WebConstants.ACTION_INDEX,
                            controllerName = WebConstants.CONTROLLER_BOE_BULK_SUBMIT,
                            routeValues = new { workspace = ws.Shortname },
                            htmlAttributes = new { name = "BOEBulkSubmitMenuLink" },
                            securityPage = SecurityPage.BulkSubmit,
                            routeName = WebConstants.ROUTE_WORKSPACE
                        }
                    }
                },
                new GenBOEMasterMenuItemModelView {
                    linkText = "Reports",
                    actionName = WebConstants.ACTION_INDEX,
                    controllerName = WebConstants.CONTROLLER_REPORTS,
                    routeValues = new { workspace = ws.Shortname },
                    htmlAttributes = null,
                    securityPage = SecurityPage.Reports,
                    routeName = WebConstants.ROUTE_REPORT
                }
            };

            AppendHelpMenuItems(menuItems);

            // RTE Templates are only going to be used in standard workspace types
            if (!ws.IsProjectMapWorkspace)
            {
                menuItems.First(x => x.linkText == "Workspace Administration").subMenuItems.Insert(6, new GenBOEMasterMenuItemModelView
                {
                    linkText = "Manage Custom RTE Templates",
                    actionName = WebConstants.ACTION_INDEX,
                    controllerName = WebConstants.CONTROLLER_RTE_TEMPLATES,
                    routeValues = new { workspace = ws.Shortname },
                    htmlAttributes = new { name = "ManageRteTemplatesMenuLink" },
                    securityPage = SecurityPage.RTETemplates,
                    routeName = WebConstants.ROUTE_WORKSPACE
                });
            }
            
            // Per Les, Project Map admins will never be pricers, and pricers will never be admins.. So we show this to all project maps
            if (securityInformation.IsAllowedProPricerAccess(securityInformation.ActiveUserNTID) || ws.IsProjectMapWorkspace)
            {
                // If the user has access to ProPricer Export, add the menu item in the 9th spot in the Workspace Admin Menu
                menuItems.First(x => x.linkText == "Workspace Administration").subMenuItems.Insert(8, new GenBOEMasterMenuItemModelView
                {
                    linkText = "Export to ProPricer",
                    actionName = WebConstants.ACTION_DISPLAY_EXPORT_TO_PROPRICER_INDEX,
                    controllerName = WebConstants.CONTROLLER_REPORTS,
                    routeValues = new { workspace = ws.Shortname },
                    htmlAttributes = new { name = "ExportToProPricer" },
                    securityPage = SecurityPage.ExportToProPricer,
                    routeName = WebConstants.ROUTE_WORKSPACE
                });
            }

            // Only show this menu item when in Initialization and Working states, and we are a standard workspace, not a project map
            if(!ws.IsProjectMapWorkspace && (ws.WorkspaceState == WorkspaceState.Initialization || ws.WorkspaceState == WorkspaceState.Working))
            {
                menuItems.First(x => x.linkText == "Workspace Administration").subMenuItems.Add(new GenBOEMasterMenuItemModelView
                {
                    linkText = "Show Getting Started",
                    actionName = WebConstants.ACTION_DISPLAY_WORKSPACE_SHOW_GETTING_STARTED_HELP,
                    controllerName = WebConstants.CONTROLLER_WORKSPACE,
                    routeValues = new { workspace = ws.Shortname },
                    securityPage = SecurityPage.GettingStartedMenuOption,
                    routeName = WebConstants.ROUTE_WORKSPACE
                });
            }
             
            return menuItems;
        }

        /// <summary>
        /// Builds the Collection of <see cref="GenBOEMasterMenuItemModelView"/> objects for the home menu
        /// </summary>
        /// <returns>the Collection of <see cref="GenBOEMasterMenuItemModelView"/> objects for the home menu</returns>
        public static Collection<GenBOEMasterMenuItemModelView> BuildHomeMasterMenuItems()
        {
             Collection<GenBOEMasterMenuItemModelView> menuItems = new Collection<GenBOEMasterMenuItemModelView> {
                new GenBOEMasterMenuItemModelView {
                    linkText = "Home",
                    actionName = WebConstants.ACTION_INDEX,
                    controllerName = WebConstants.CONTROLLER_HOME,
                    routeValues = null,
                    htmlAttributes = null,
                    securityPage = SecurityPage.Home
                },
                new GenBOEMasterMenuItemModelView {
                    linkText="Admin",
                    subMenuItems = new Collection<GenBOEMasterMenuItemModelView> {
                        new GenBOEMasterMenuItemModelView {
                            linkText = "System Administration",
                            actionName = WebConstants.ACTION_SYSTEM_ADMIN,
                            controllerName = WebConstants.CONTROLLER_ADMIN,
                            routeValues = null,
                            htmlAttributes = new { @class = "SystemAdminSubMenu"},
                            securityPage = SecurityPage.SystemAdmin
                        },
                        new GenBOEMasterMenuItemModelView {
                            linkText = "Metrics Administration",
                            controllerName = WebConstants.CONTROLLER_ADMIN,
                            routeValues = null,
                            htmlAttributes = null
                        },
                        new GenBOEMasterMenuItemModelView {
                            linkText = "Who's Online",
                            actionName = WebConstants.ACTION_HOME_DISPLAY_WHOSONLINE,
                            controllerName = WebConstants.CONTROLLER_HOME,
                            routeValues = null,
                            securityPage = SecurityPage.SystemAdmin
                        },
                        new GenBOEMasterMenuItemModelView {
                            linkText = "GenBOE Metrics",
                            actionName = WebConstants.ACTION_HOME_DISPLAY_METRICS_DETAILS,
                            controllerName = WebConstants.CONTROLLER_HOME,
                            routeValues = null,
                            securityPage = SecurityPage.SystemAdmin
                        }
                    }
                }
            };

            AppendHelpMenuItems(menuItems);

            return menuItems;
        }

        /// <summary>
        /// SSC Help URL
        /// </summary>
        private const string SSC_HELP_LINK = "https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Tools/default.aspx?RootFolder=%2Fsites%2Ffbo%2FCCDME%2FEstimating%2FTools%2FProPricer%20User%20Forum%2FgenBOE%2FgenBOE%20Training%20Documents&FolderCTID=0x0120007A9FC2393F740A4CAE02B8F033B82084&";

        /// <summary>
        /// RMS Help URL - Author Training
        /// </summary>
        private const string RMS_HELP_AUTHOR_LINK = "file://us.lmco.com\\mst\\depot\\depot_public\\pricing-controller\\genBOE%20Author%20Training.pptx";

        /// <summary>
        /// RMS Help URL - Admin Training
        /// </summary>
        private const string RMS_HELP_WS_ADMIN_LINK = "file://us.lmco.com\\mst\\depot\\depot_public\\pricing-controller\\genBOE-Workspace%20Administrator%20Training-RMS.pptx";

        /// <summary>
        /// Appends the help menu items to the menu
        /// </summary>
        /// <param name="menuItems"></param>
        private static void AppendHelpMenuItems(ICollection<GenBOEMasterMenuItemModelView> menuItems)
        {
            if (ConfigurationUtilities.GetAppSetting("ShutOffExternalLinksForClassifiedInstall").ToLower().Equals("false"))
            {
                if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
                {
                    menuItems.Add(
                        new GenBOEMasterMenuItemModelView
                        {
                            linkText = "Help",
                            menuLocation = MenuLocation.Right,
                            securityPage = SecurityPage.Home,
                            linkUrl = new Uri(SSC_HELP_LINK)
                        });
                }
                else if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
                {
                    menuItems.Add(
                        new GenBOEMasterMenuItemModelView
                        {
                            linkText = "Help",
                            menuLocation = MenuLocation.Right,
                            subMenuItems = new Collection<GenBOEMasterMenuItemModelView> {
                            new GenBOEMasterMenuItemModelView {
                                securityPage = SecurityPage.Home,
                                linkText = "Author Training",
                                linkUrl = new Uri(RMS_HELP_AUTHOR_LINK)
                            },
                            new GenBOEMasterMenuItemModelView {
                                securityPage = SecurityPage.Home,
                                linkText = "Workspace Administrator Training",
                                linkUrl = new Uri(RMS_HELP_WS_ADMIN_LINK)
                            }
                        }
                    });
                }
            }
        }
    }
}