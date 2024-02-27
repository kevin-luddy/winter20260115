// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Common
{
	using System;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Text;
	using System.Web;
	using System.Web.Mvc;
	using GenBOE.ActionLogic.Common;
	using IES.Common;
	using GenBOE.Objects;
	using IES.Common.classes;
	using System.Collections.Generic;
	using GenBOE.Dtos;
	using System.Linq;

	[ExcludeFromCodeCoverage]
	sealed public class SiteMasterUtilities
	{
		private const string REQUEST_CACHE_KEY_WORKSPACE = "Workspace";
		private const string REQUEST_CACHE_KEY_WORKSPACE_ID = "WorkspaceID";

		private IFullObjectFactory Factory;

		private static Logger _log = new Logger(typeof(SiteMasterUtilities));

		/// <summary>
		/// Default constructor
		/// </summary>
		private SiteMasterUtilities()
		{
		}

		public SiteMasterUtilities(IFullObjectFactory factory)
		{
			this.Factory = factory;
		}

		public static Boolean CurrentActionMatches(ControllerContext context, String controllerNameToMatch, String actionNameToMatch)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}
			if (controllerNameToMatch == null)
			{
				throw new ArgumentNullException(nameof(controllerNameToMatch));
			}
			if (actionNameToMatch == null)
			{
				throw new ArgumentNullException(nameof(actionNameToMatch));
			}

			return (context.RouteData.Values["action"].ToString().IsEquivalentTo(actionNameToMatch)
				&& context.RouteData.Values["controller"].ToString().IsEquivalentTo(controllerNameToMatch));
		}

		/// <summary>
		/// This method will set the workspace in use for this request in this requests HttpContext.Current.Items cache for use by Views
		/// </summary>
		/// <param name="workspace">shortname of the workspace</param>
		public void SetWorkspaceInCurrentRequestCache(string workspace)
		{
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}
			if (HttpContext.Current.Items[REQUEST_CACHE_KEY_WORKSPACE] == null ||
				HttpContext.Current.Items[REQUEST_CACHE_KEY_WORKSPACE_ID] == null ||
				!workspace.Equals(HttpContext.Current.Items[REQUEST_CACHE_KEY_WORKSPACE]))
			{
				HttpContext.Current.Items[REQUEST_CACHE_KEY_WORKSPACE] = workspace;

				// Get the Workspace ID from the Mapper
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspace);

				HttpContext.Current.Items[REQUEST_CACHE_KEY_WORKSPACE_ID] = ws.Id;
			}

		}

		/// <summary>
		/// This method will set the browser support reminder in a cookie
		/// so..., users are alerted only one time
		/// </summary>
		/// <return>reminder is set to true or false</return>
		private static void CreateBrowserReminderCookie()
		{
			HttpCookie cookie = HttpContext.Current.Request.Cookies[WebConstants.BROWSER_REMINDER] ?? new HttpCookie(WebConstants.BROWSER_REMINDER);
			cookie.Values[WebConstants.BROWSER_REMINDER] = DateTime.Now.ToShortDateString();
			cookie.Expires = DateTime.Now.AddHours(2);
			HttpContext.Current.Response.Cookies.Add(cookie);
		}

		/// <summary>
		/// Sets the Cookie session on whether this ECI Banner should show up every browser session
		/// </summary>
		private static HttpCookie CreateEciForbiddenCookie()
		{
			HttpCookie cookie = HttpContext.Current.Request.Cookies[WebConstants.ECI_FORBIDDEN_BANNER] ?? new HttpCookie(WebConstants.ECI_FORBIDDEN_BANNER);
			cookie.Values[WebConstants.ECI_FORBIDDEN_BANNER] = DateTime.Now.ToShortDateString();
			HttpContext.Current.Response.Cookies.Add(cookie);

			return cookie;
		}

		/// <summary>
		/// Whether to show the ECI Forbidden Message
		/// </summary>
		/// <returns>True to show, otherwise false.</returns>
		public static bool ShowEciForbiddenMessage()
		{
			bool displayBanner = false;

			if (IESBannerApp == Constants.BOE_SPACE_INTERNATIONAL_APP_NAME)
			{
				HttpCookie cookie = HttpContext.Current.Request.Cookies[WebConstants.ECI_FORBIDDEN_BANNER];

				if (cookie == null)
				{
					displayBanner = true;

					cookie = CreateEciForbiddenCookie();
				}

				cookie.Expires = DateTime.Now.AddMinutes(30.0);
			}

			return displayBanner;
		}

		/// <summary>
		/// This method returns the browser reminder from the cookie.  If the cookie is present
		/// the user should not see the prompt again since we've prompted them already
		/// </summary>
		/// <returns>true means display the reminder, false means don't display</returns>
		public static bool GetBrowserReminder()
		{
			bool displayReminder = false;
			HttpCookie cookie = HttpContext.Current.Request.Cookies[WebConstants.BROWSER_REMINDER];

			// no cookie, create one and display reminder to the user
			if (cookie == null)
			{
				displayReminder = true;

				// create browser reminder cookie
				CreateBrowserReminderCookie();
			}

			return displayReminder;
		}

		/// <summary>
		/// This method returns the current workspace from the session.  This should never be called 
		/// from an action, but is useful in Views for creating action links and the render partial method.
		/// </summary>
		/// <returns>The current workspace from the session.</returns>
		public static string GetCurrentWorkspace()
		{
			return (string)HttpContext.Current.Items[REQUEST_CACHE_KEY_WORKSPACE];
		}

		/// <summary>
		/// This method returns the current workspace ID from the session.  This should never be called 
		/// from an action, but is useful in Views for creating action links and the render partial method.
		/// </summary>
		/// <returns></returns>
		public static int GetCurrentWorkspaceID()
		{
			return (int)HttpContext.Current.Items[REQUEST_CACHE_KEY_WORKSPACE_ID];
		}

		/// <summary>
		/// Creates a string containing all of the errors in the Model State
		/// </summary>
		/// <param name="modelStateDictionary">The ModelState being checked</param>
		/// <returns>A string of errors</returns>
		public static string CreateValidationErrorResponse(ModelStateDictionary modelStateDictionary)
		{
			if (modelStateDictionary == null)
			{
				throw new ArgumentNullException(nameof(modelStateDictionary));
			}

			StringBuilder errors = new StringBuilder();
			foreach (ModelState state in modelStateDictionary.Values)
			{
				foreach (ModelError error in state.Errors)
				{
					if (!string.IsNullOrEmpty(error.ErrorMessage))
					{
						errors.AppendLine(error.ErrorMessage);
					}
					else
					{
						errors.AppendLine("An exception occurred during model creation.  Please check your data, and try again");
						_log.Error(error.Exception, "An exception  was found in the model state.");
					}
				}
			}
			return errors.ToString();
		}

		/// <summary>
		/// This function will take a collection of user Ids and converts them
		/// to a list the view can read
		/// </summary>
		/// <param name="inApprovers"></param>
		/// <returns></returns>
		public static string ConvertCollectionToJavascriptString(Collection<int> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			if (collection.Count > 0)
			{
				StringBuilder input = new StringBuilder();
				foreach (int x in collection)
				{
					input.Append("'");
					input.Append(x.ToString().Trim());
					input.Append("'");
					input.Append(",");
				}

				// Trim the trailing comma
				input = input.Remove(input.Length - 1, 1);

				return input.ToString();
			}

			else
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets Archive URL for the site; used to decide if the app is running in archive
		/// </summary>
		/// <returns>Archive URL</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string ArchiveUrl()
		{
			return ConfigurationUtilities.GetAppSetting("ArchiveUrlBoe");
		}

		/// <summary>
		/// Gets the Server URL
		/// </summary>
		/// <returns>Server URL for this website</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string ServerUrl()
		{
			return ConfigurationUtilities.GetAppSetting("ServerURL");
		}

		/// <summary>
		/// Returns true if RMS Archive
		/// </summary>
		/// <returns>True if RMS Archive</returns>
		public static bool IsRMSArchive()
		{
			return SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST &&
				ArchiveUrl() == ServerUrl();
		}

		/// <summary>
		/// Returns true if this is RMS Production
		/// </summary>
		/// <returns>True if RMS Producvtion, false otherwise</returns>
		public static bool IsRMSProduction()
		{
			return SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST &&
				ProductionUrl() == ServerUrl();
		}

		/// <summary>
		/// Returns true if this is Read Only
		/// </summary>
		/// <returns></returns>
		public static bool IsReadOnly()
		{
			return !string.IsNullOrEmpty(ConfigurationUtilities.GetAppSetting("IsReadOnly")) && ConfigurationUtilities.GetAppSetting("IsReadOnly").ToLower().Equals("true");
		}

		/// <summary>
		/// Returns Resources / Business Resource Codes based on Company mode and 1LMX or Legacy distinction
		/// </summary>
		/// <param name="resourceData">Original Resources list</param>
		/// <param name="isBrc">Bool to signify if Resources are of type Business Resource Codes</param>
		/// <returns>Filtered list of Resources</returns>
		public static IReadOnlyCollection<ResourceDTO> GetResourcesBasedOnCompanyMode(IReadOnlyCollection<ResourceDTO> resourceData, bool isBrc)
		{
			if (Utilities.IsBRCEnabledForSystem)
			{
				if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
				{
					if (!isBrc)
					{
						resourceData = resourceData.Where(x => x.ElementOfCost != ElementOfCostType.LMLabor || (x.SegRegion != WebConstants.SPACE_1LMX_CORE && x.SegRegion != WebConstants.SPACE_1LMX_SERVICES)).ToList();
					}
					else
					{
						resourceData = resourceData.Where(x => x.ElementOfCost != ElementOfCostType.LMLabor || x.SegRegion == WebConstants.SPACE_1LMX_CORE || x.SegRegion == WebConstants.SPACE_1LMX_SERVICES).ToList();
					}
				}

				if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
				{
					if (!isBrc)
					{
						resourceData = resourceData.Where(x => x.ElementOfCost != ElementOfCostType.LMLabor || (x.SegRegion != WebConstants.RMS_1LMX_CORE && x.SegRegion != WebConstants.RMS_1LMX_SERVICES)).ToList();

					}
					else
					{
						resourceData = resourceData.Where(x => x.ElementOfCost != ElementOfCostType.LMLabor || x.SegRegion == WebConstants.RMS_1LMX_CORE || x.SegRegion == WebConstants.RMS_1LMX_SERVICES).ToList();
					}
				}
			}

			return resourceData;
		}

		#region A number of settings that were moved into web.config to support classified installations. These methods expose the settings.

		/// <summary>
		/// Gets production URL for the site; used to decide if the app is running in production
		/// </summary>
		/// <returns>Production URL</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string ProductionUrl()
		{
			return ConfigurationUtilities.GetAppSetting("ProdUrlBoe");
		}

		#endregion

		/// <summary>
		/// Indicates whether the BOEForm area of the site is visible or not.
		/// </summary>
		/// <returns>true if BOEForm area of the site is visible; false otherwise.</returns>
		public static bool IsBOEFormVisible
		{
			get
			{
				bool value;
				bool.TryParse(ConfigurationUtilities.GetAppSetting("IsBOEFormVisible"), out value);
				return value;
			}
		}

		/// <summary>
		/// Indicates whether the PBOE/IBOE forms should add Portion Markings.
		/// 
		/// This should be only turned on in a classified SSC instance
		/// </summary>
		/// <returns>true if Portion Markings are turned on; false otherwise.</returns>
		public static bool IsPortionMarkingEnabled
		{
			get
			{
				bool value = false;
				bool.TryParse(ConfigurationUtilities.GetAppSetting("IsPortionMarkingEnabled"), out value);
				return value;
			}
		}

		/// <summary>
		/// Indicates whether the Project Map Workspaces are enabled.
		/// 
		/// This should be only turned on in an RMS instance
		/// </summary>
		/// <returns>true if Project Map Workspaces are enabled; false otherwise.</returns>
		public static bool IsProjectMapEnabled
		{
			get
			{
				bool value = false;
				bool.TryParse(ConfigurationUtilities.GetAppSetting("IsProjectMapEnabled"), out value);
				return value;
			}
		}

		/// <summary>
		/// Indicates whether to show the IES Header iframe
		/// </summary>
		public static bool ShowIesHeader
		{
			get
			{
				bool value = false;
				bool.TryParse(ConfigurationUtilities.GetAppSetting("ShowIesHeader"), out value);
				return value;
			}
		}

		/// <summary>
		/// Gets the text for the Unclassified Banner
		/// </summary>
		public static string UnclassifiedBannerText
		{
			get
			{
				return ConfigurationUtilities.GetAppSetting("UnclassifiedBannerText");
			}
		}

		/// <summary>
		/// Gets the IES Banner App name
		/// </summary>
		public static string IESBannerApp
		{
			get
			{
				return ConfigurationUtilities.GetAppSetting("IESBannerApp");
			}
		}
	}
}