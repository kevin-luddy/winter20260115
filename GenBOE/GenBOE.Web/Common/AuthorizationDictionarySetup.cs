// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Common
{
    using System.Collections.Generic;
    using GenBOE.ActionLogic.Common;
    using IES.Common;

    /// <summary>
    /// This class generates a dictionary that will be used by the controllers to check access authorization
    /// for each controller's action
    /// </summary>
    public static class AuthorizationDictionarySetup
    {
        /// <summary>
        /// Generates the dictionary for Controller security checks.
        /// Each controller should have a region that maps ALL of its actions to the Security Page and Security Authorization.
        /// </summary>
        /// <returns>Dictionary of all controllers, actions and page/security authorizations</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> GenerateDictionary()
        {
            Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result = new Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>>();

            AddHomeControllerActions(result);
            AddBoeControllerActions(result);
            
            return result;
        }

        /// <summary>
        /// Generates items for the BOE Controller.
        /// </summary>
        /// <param name="result">Dictionary into which the data will be added</param>
        private static void AddBoeControllerActions(Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result)
        {
            Dictionary<string, SecurityPageAndAuthorization> BoeControllerActions = new Dictionary<string, SecurityPageAndAuthorization>();

            AddActionToController("SaveSummaryTaskElements", SecurityPage.BOELaborGrid, SecurityAuthorization.CreateReadUpdateDelete, BoeControllerActions);

            result.Add("BOE", BoeControllerActions);
        }

        #region Controller Setup Methods

        /// <summary>
        /// Generates items for the Home Controller
        /// </summary>
        /// <param name="result">Dictionary into which the data will be added</param>
        private static void AddHomeControllerActions(Dictionary<string, Dictionary<string, SecurityPageAndAuthorization>> result)
        {
            Dictionary<string, SecurityPageAndAuthorization> HomeControllerActions = new Dictionary<string, SecurityPageAndAuthorization>();

            AddActionToController("Index", SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);
            AddActionToController(WebConstants.ACTION_HOME_DISPLAY_MASTER_MENU, SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);
            AddActionToController("Validate", SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);
            AddActionToController("WhosOnline", SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);
            AddActionToController("Search", SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);
            AddActionToController("SearchUserName", SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);
            AddActionToController(WebConstants.ACTION_HOME_DISPLAY_WHOSONLINE, SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);
            AddActionToController(WebConstants.ACTION_HOME_PAGE_WHOSONLINE, SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);            
            AddActionToController("GetWorkspaceAndBoeData", SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);
            AddActionToController("GetBoeData", SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);
            AddActionToController(WebConstants.ACTION_HOME_DISPLAY_METRICS_DETAILS, SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);
            AddActionToController(WebConstants.ACTION_HOME_GET_USER_METRICS, SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);
            AddActionToController(WebConstants.ACTION_HOME_RESTORE_WORKSPACE, SecurityPage.Home, SecurityAuthorization.CreateReadUpdateDelete, HomeControllerActions);
            AddActionToController(WebConstants.ACTION_HOME_RESTORE_PTM_WORKSPACE, SecurityPage.Home, SecurityAuthorization.CreateReadUpdateDelete, HomeControllerActions);
            AddActionToController(WebConstants.ACTION_HOME_DELETE_WORKSPACES, SecurityPage.Home, SecurityAuthorization.CreateReadUpdateDelete, HomeControllerActions);
            AddActionToController(WebConstants.ACTION_HOME_GET_TRACKING_NUMBERS, SecurityPage.Home, SecurityAuthorization.CreateReadUpdateDelete, HomeControllerActions);
            AddActionToController(WebConstants.ACTION_HOME_CHANGE_FAVORITE, SecurityPage.Home, SecurityAuthorization.Read, HomeControllerActions);

            result.Add("Home", HomeControllerActions);
        }


        #endregion
   
        /// <summary>
        /// Adds each action into the controller dictionary
        /// </summary>
        /// <param name="action">Controller Action</param>
        /// <param name="page">Security Page</param>
        /// <param name="authorization">Security Authorization</param>
        /// <param name="controller">Controller Dictionary</param>
        private static void AddActionToController(string action, SecurityPage page, SecurityAuthorization authorization, Dictionary<string, SecurityPageAndAuthorization> controller)
        {
            controller.Add(action.ToLower(), new SecurityPageAndAuthorization(page, authorization));
        }
    }

    /// <summary>
    /// This class will aid in creating a security matrix dictionary, used by the base controller to do security checks.
    /// </summary>
    public class SecurityPageAndAuthorization
    {
        /// <summary>
        /// Security Page
        /// </summary>
        public SecurityPage Page { get; set; }

        /// <summary>
        /// Security Authorization
        /// </summary>
        public SecurityAuthorization Authorization { get; set; }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="Page">Security Page</param>
        /// <param name="Authorization">Security Authorization</param>
        public SecurityPageAndAuthorization(SecurityPage Page, SecurityAuthorization Authorization)
        {
            this.Page = Page;
            this.Authorization = Authorization;
        }
    }
}