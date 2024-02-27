// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Backend.Common
{
	using System.Collections.Generic;
	using IES.Common.Core.Security;

	/// <summary>
	/// This class generates a dictionary that will be used by the controllers to check access authorization
	/// for each controller's action
	/// </summary>
	internal static class AuthorizationDictionarySetup
    {
        #region Inner Workings

        /// <summary>
        /// Generates the dictionary for Controller security checks.
        /// Each controller should have a region that maps ALL of its actions to the Security Page and Security Authorization.
        /// </summary>
        /// <returns>Dictionary of all controllers, actions and page/security authorizations</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        internal static Dictionary<string, Dictionary<string, SecurityAuthorization>> GenerateDictionary()
        {
            Dictionary<string, Dictionary<string, SecurityAuthorization>> result = new();

            AddAdminControllerActions(result);
            AddDocumentControllerActions(result);

            return result;
        }

        /// <summary>
        /// Adds each action into the controller dictionary
        /// </summary>
        /// <param name="action">Controller Action</param>
        /// <param name="authorization">Security Authorization</param>
        /// <param name="controller">Controller Dictionary</param>
        private static void AddActionToController(string action, SecurityAuthorization authorization, Dictionary<string, SecurityAuthorization> controller)
        {
            controller.Add(action.ToLower(), authorization);
        }

        #endregion

        #region Controller Setup Methods

        /// <summary>
        /// Generates items for the Admin Controller
        /// </summary>
        /// <param name="result">Dictionary into which the data will be added</param>
        private static void AddAdminControllerActions(Dictionary<string, Dictionary<string, SecurityAuthorization>> result)
        {
            Dictionary<string, SecurityAuthorization> adminControllerActions = new();

            // TODO - Add Admin Actions as needed
            // AddActionToController(IESWebConstants.Action.ADMIN_SOME_OPERATION, SecurityAuthorization.Read, adminControllerActions);

            result.Add(IES.Common.Core.Constants.IESWebConstants.CONTROLLER_ADMIN.ToLower(), adminControllerActions);
        }

        /// <summary>
        /// Generates items for the Document Controller
        /// </summary>
        /// <param name="result">Dictionary into which the data will be added</param>
        private static void AddDocumentControllerActions(Dictionary<string, Dictionary<string, SecurityAuthorization>> result)
        {
            Dictionary<string, SecurityAuthorization> documentControllerActions = new();

            AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_INDEX_DOCUMENT, SecurityAuthorization.None, documentControllerActions);
            AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_GET_DOCUMENT, SecurityAuthorization.CreateReadUpdateDelete, documentControllerActions);
            AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_SAVE_DOCUMENT, SecurityAuthorization.CreateReadUpdateDelete, documentControllerActions);
            AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_DELETE_DOCUMENT, SecurityAuthorization.CreateReadUpdateDelete, documentControllerActions);
            AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_PUBLISH_DOCUMENT, SecurityAuthorization.Read, documentControllerActions);
            AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_SHOW_WHOS_ONLINE, SecurityAuthorization.None, documentControllerActions);
            AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_GET_WHOS_ONLINE, SecurityAuthorization.None, documentControllerActions);
            AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_GET_PROPOSALS_FOR_NEW_DOCUMENT, SecurityAuthorization.None, documentControllerActions);
            AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_SAVE_NEW_DOCUMENT, SecurityAuthorization.CreateReadUpdateDelete, documentControllerActions);
            AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_GET_RATE_CODES_FOR_REVISION, SecurityAuthorization.None, documentControllerActions);
            AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_GET_SECTIONS_FOR_REVISION, SecurityAuthorization.None, documentControllerActions);
			AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_GET_INFO, SecurityAuthorization.None, documentControllerActions);
			AddActionToController(IES.Common.Core.Constants.IESWebConstants.ACTION_GET_APP_SETTINGS_FEATURE, SecurityAuthorization.None, documentControllerActions);
			result.Add(IES.Common.Core.Constants.IESWebConstants.CONTROLLER_DOCUMENT.ToLower(), documentControllerActions);
        }

        #endregion
    }
}