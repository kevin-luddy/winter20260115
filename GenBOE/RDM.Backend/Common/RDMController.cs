// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace RDM.Backend.Common
{
	using System;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using IES.ActionLogic.Core.ControllerLogic;
	using IES.Common.Core;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.Controllers;
	using Microsoft.AspNetCore.Mvc.Filters;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using Newtonsoft.Json;

	/// <summary>
	/// RDM base controller
	/// </summary>
	[Authorize]
	public abstract class RDMController : IES.Common.Core.IESController, IActionFilter
	{
		#region Loaders and Such

		/// <summary>
		/// Who's Online Loader
		/// </summary>
		protected IWhosOnlineLoader WhosOnlineLoader { get; set; }

		/// <summary>
		/// Controller Logic
		/// </summary>
		protected IRdmControllerLogic Logic { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="whosOnlineLoader">Who's Online Loader</param>
		/// <param name="logic">RDM Controller Logic</param>
		/// <param name="logger">The logger</param>
		protected RDMController(ISecurityInformation securityInformation, IWhosOnlineLoader whosOnlineLoader,
			IRdmControllerLogic logic, IConfiguration configuration, ILogger logger) : base(logger, securityInformation, configuration)
		{
			this.WhosOnlineLoader = whosOnlineLoader;
			this.Logic = logic;
		}

		#endregion

		#region Events

		/// <summary>
		/// Initalizes an action with any permissions
		/// </summary>
		/// <param name="logger">The logger</param>
		/// <param name="controllerName">The controller to initialize</param>
		/// <param name="functionName">The function to initialize</param>
		[NonAction]
		protected void InitializeAction(ILogger logger, string controllerName, string functionName)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			if (functionName == null)
			{
				throw new ArgumentNullException(nameof(functionName));
			}

			// Add current user details
			try
			{
				this.WhosOnlineLoader.UpdateLastAccessTime(this.Logic.ActiveUser, ApplicationName.RDM.GetDescription());
			}
			catch
			{
				// Prevent displaying error in case user is in the middle of a transaction with save
			}

			if (!this.Logic.IsUserAuthorized(controllerName, functionName))
			{
				throw new AuthorizationException(functionName + " was not authorized");
			}
		}

		/// <summary>
		/// Override of the default OnActionExecuting, to allow us to do security verification
		/// </summary>
		/// <param name="context">Context</param>
		[NonAction]
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			IdentitySwap.IdentitySwappingForTesting.SwapIdentity("IsIdentitySwappingAllowed", "ActiveDirectoryPath",
				"ADGroupsAllowedToSwapIdentity", context.HttpContext);

			if (context != null)
			{
				ControllerActionDescriptor descriptor = context.ActionDescriptor as ControllerActionDescriptor;
				// Check to see if we have a dictionary for the controller. If we do, get it. If not, throw an exception
				string controllerName = descriptor?.ControllerName?.ToLower(CultureInfo.CurrentCulture);
				string functionName = descriptor?.ActionName?.ToLower(CultureInfo.CurrentCulture);
				this.InitializeAction(this.log, controllerName, functionName);
			}

			base.OnActionExecuting(context);
		}

		#endregion

		/// <summary>
		/// Gets data for Who's Online
		/// </summary>
		/// <returns>Who's Online data</returns>
		[HttpPost("[action]")]
		public ActionResult GetWhosOnline()
		{
			return this.Json(this.WhosOnlineLoader.GetWhosOnlineData(ApplicationName.RDM.GetDescription()));
		}

		#region Menu

		/// <summary>
		/// Gets the Revision menu option
		/// </summary>
		/// <param name="menuOptionControllerName">Current controller name</param>
		/// <returns>Menu option information</returns>
		protected MenuOptionModelView GetHomeMenuOption(string menuOptionControllerName)
		{
			return new MenuOptionModelView()
			{
				Label = "Home",
				Link = "/" + IESWebConstants.CONTROLLER_HOME,
				Selected = menuOptionControllerName == IESWebConstants.CONTROLLER_HOME.ToLower(CultureInfo.CurrentCulture),
				UserCanAccess = true
			};
		}

		/// <summary>
		/// Gets the PPR&amp;D menu option
		/// </summary>
		/// <param name="menuOptionControllerName">Current controller name</param>
		/// <returns>Menu option information</returns>
		protected MenuOptionModelView GetPprdMenuOption(string menuOptionControllerName)
		{
			return new MenuOptionModelView()
			{
				Label = "PPR&D",
				Link = "/" + IESWebConstants.CONTROLLER_PPRD,
				Selected = menuOptionControllerName == IESWebConstants.CONTROLLER_PPRD.ToLower(CultureInfo.CurrentCulture),
				UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_PPRD, IESWebConstants.VIEW_PPRD)
			};
		}

		/// <summary>
		/// Gets the Rates menu option
		/// </summary>
		/// <param name="menuOptionControllerName">Current controller name</param>
		/// <returns>Menu option information</returns>
		protected MenuOptionModelView GetRatesMenuOption(string menuOptionControllerName)
		{
			return new MenuOptionModelView()
			{
				Label = "Rates",
				Link = "#",
				Selected =
						menuOptionControllerName == IESWebConstants.CONTROLLER_RATE.ToLower(CultureInfo.CurrentCulture) ||
						menuOptionControllerName == IESWebConstants.CONTROLLER_BURDEN_POOL.ToLower(CultureInfo.CurrentCulture),
				SubMenuOptions = new Collection<MenuOptionModelView>()
				{
					this.GetManageRatesMenuOption(),
					this.GetManageBurdenPoolsMenuOption(),
					this.GetComparePublishRollbackMenuOption()
				}
			};
		}

		/// <summary>
		/// Gets the Manage Rates menu option
		/// </summary>
		/// <returns>Menu option information</returns>
		protected MenuOptionModelView GetManageRatesMenuOption()
		{
			return new MenuOptionModelView()
			{
				Label = "Manage Rates",
				Link = "/" + IESWebConstants.CONTROLLER_RATE,
				UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE,
								IESWebConstants.ACTION_VIEW_RATES)
			};
		}

		/// <summary>
		/// Gets the Manage Burden Pools menu option
		/// </summary>
		/// <returns>Menu option information</returns>
		protected MenuOptionModelView GetManageBurdenPoolsMenuOption()
		{
			return new MenuOptionModelView()
			{
				Label = "Manage Burden Pools",
				Link = "/" + IESWebConstants.CONTROLLER_BURDEN_POOL,
				UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_BURDEN_POOL,
								IESWebConstants.ACTION_VIEW_BURDEN_POOLS)
			};
		}

		/// <summary>
		/// Gets the Compare / Publish / Rollback menu option
		/// </summary>
		/// <returns>Menu option information</returns>
		private MenuOptionModelView GetComparePublishRollbackMenuOption()
		{
			return new MenuOptionModelView()
			{
				// if user is not admin, adjust menu item label to "Compare" 
				Label = this.Logic.IsRDMAdminUser ? "Compare / Publish / Rollback" : "Compare",
				Link = "/" + IESWebConstants.VIEW_VERSION,
				UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_VERSION,
								IESWebConstants.VIEW_VERSION_DIFF)
			};
		}

		/// <summary>
		/// Gets the Reports menu option
		/// </summary>
		/// <param name="menuOptionControllerName">Current controller name</param>
		/// <returns>Menu option information</returns>
		protected MenuOptionModelView GetReportsMenuOption(string menuOptionControllerName)
		{
			return new MenuOptionModelView()
			{
				Label = "Reports / Exports",
				Link = "/" + IESWebConstants.CONTROLLER_REPORTS,
				Selected = menuOptionControllerName == IESWebConstants.CONTROLLER_REPORTS.ToLower(CultureInfo.CurrentCulture),
				UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.VIEW_REPORTS)
			};
		}

		/// <summary>
		/// Gets the Admin menu option
		/// </summary>
		/// <param name="menuOptionControllerName">Current controller name</param>
		/// <returns>Menu option information</returns>
		protected MenuOptionModelView GetAdminMenuOption(string menuOptionControllerName)
		{
			return new MenuOptionModelView()
			{
				// The UserCanAccess property is ignored on parent menu options.  It is automatically determined
				// in the Angular controller based on whether or not the user has access to any child menu options.
				Label = "Admin",
				Link = "#",
				Selected = menuOptionControllerName == IESWebConstants.CONTROLLER_ADMIN.ToLower(CultureInfo.CurrentCulture) || (this.Logic.IsRDMAdminUser && menuOptionControllerName == IESWebConstants.CONTROLLER_FILE_ATTACHMENTS.ToLower(CultureInfo.CurrentCulture)),
				IsAdminMenuOption = true,
				SubMenuOptions = new Collection<MenuOptionModelView>()
				{
					this.GetManageFileAttachmentsMenuOption(menuOptionControllerName, true),
					this.GetRevisionConfigurationMenuOption(),
					this.GetCobraYearConfigurationMenuOption(),
					this.GetManageCobraMappingsMenuOption(),
					this.GetRateCodeReplicationMenuOption()
				}
			};
		}

		/// <summary>
		/// Gets the Manage File Attachments menu option for the Admin list.
		/// </summary>
		/// <param name="menuOptionControllerName">Current controller name</param>
		/// <param name="isAdmin">True if this is for the admin menu, false if this is for the non-admin menu.</param>
		/// <returns>Menu option for Managing File Attachments</returns>
		protected MenuOptionModelView GetManageFileAttachmentsMenuOption(string menuOptionControllerName, bool isAdmin)
		{
			return new MenuOptionModelView()
			{
				Selected = !isAdmin && menuOptionControllerName == IESWebConstants.CONTROLLER_FILE_ATTACHMENTS.ToLower(CultureInfo.CurrentCulture),
				Label = isAdmin ? "Manage File Attachments" : "File Attachments",
				Link = "/" + IESWebConstants.CONTROLLER_FILE_ATTACHMENTS + "/" +
								   IESWebConstants.VIEW_FILE_ATTACHMENTS,
				UserCanAccess = this.Logic.IsRDMAdminUser == isAdmin
			};
		}

		/// <summary>
		/// Gets the Revision Configuration menu option
		/// </summary>
		/// <returns>Menu option information</returns>
		private MenuOptionModelView GetRevisionConfigurationMenuOption()
		{
			return new MenuOptionModelView()
			{
				Label = "Revision Configuraton",
				Link = "/" + IESWebConstants.CONTROLLER_ADMIN + "/" +
								   IESWebConstants.VIEW_REVISION_CONFIGURATION,
				UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN,
								IESWebConstants.VIEW_REVISION_CONFIGURATION)
			};
		}

		/// <summary>
		/// Gets the rate code replication menu option.
		/// </summary>
		/// <returns>Menu option information</returns>
		private MenuOptionModelView GetRateCodeReplicationMenuOption()
		{
			return new MenuOptionModelView()
			{
				Label = "Rate Code Replication",
				Link = "/" + IESWebConstants.CONTROLLER_ADMIN + "/" +
								   IESWebConstants.VIEW_RATE_CODE_REPLICATION,
				UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN,
								IESWebConstants.VIEW_RATE_CODE_REPLICATION)
			};
		}

		/// <summary>
		/// Gets the Cobra Year Configuration menu option
		/// </summary>
		/// <returns>Menu option information</returns>
		private MenuOptionModelView GetCobraYearConfigurationMenuOption()
		{
			return new MenuOptionModelView()
			{
				Label = "Cobra Year Configuration",
				Link = "/" + IESWebConstants.CONTROLLER_ADMIN + "/" +
								   IESWebConstants.VIEW_COBRA_YEAR_CONFIGURATION,
				UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN,
								IESWebConstants.VIEW_COBRA_YEAR_CONFIGURATION)
			};
		}

		/// <summary>
		/// Gets the Manage Cobra Mappings menu option
		/// </summary>
		/// <returns>Menu option information</returns>
		protected MenuOptionModelView GetManageCobraMappingsMenuOption()
		{
			return new MenuOptionModelView()
			{
				Label = "Manage Cobra Mappings",
				Link = "/" + IESWebConstants.CONTROLLER_ADMIN + "/" +
								   IESWebConstants.VIEW_COBRA_DETAILS,
				UserCanAccess = this.Logic.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN,
								IESWebConstants.VIEW_COBRA_DETAILS)
			};
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Jsons the specified data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <returns>Returns JSON for the specified data.</returns>
		protected ContentResult Json(object data)
		{
			JsonSerializerSettings microsoftDateFormatSettings = new()
			{
				DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
				DateParseHandling = DateParseHandling.DateTime,
				DateTimeZoneHandling = DateTimeZoneHandling.Local
			};
			return this.Content(JsonConvert.SerializeObject(data, microsoftDateFormatSettings), "application/json");
		}

		#endregion
	}
}