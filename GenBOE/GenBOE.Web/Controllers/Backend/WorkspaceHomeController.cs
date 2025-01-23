// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers
{
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.Metrics;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using IES.Common;
	using IES.Common.Exceptions;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Web.Http.Cors;
	using System.Web.Mvc;

	/// <summary>
	/// Workspace Home Controller for getting workspace home data.
	/// </summary>
	[EnableCors("*", "*", "*", SupportsCredentials = true)]
	public class WorkspaceHomeController : BoeDataBaseAPIController
	{
		#region Properties & Ctor
		/// <summary>
		/// Common data mapper.
		/// </summary>
		private ICommonDataMapper commonDataMapper { get; set; }

		/// <summary>
		/// Site master utils.
		/// </summary>
		private SiteMasterUtilities siteMasterUtilities { get; set; }

		/// <summary>
		/// System metrics.
		/// </summary>
		private SystemMetrics systemMetrics { get; set; }

		/// <summary>
		/// Service for GenBOE Controller.
		/// </summary>
		private IGenBOEControllerLogic genBOEControllerLogic { get; set; }

		/// <summary>
		/// Logger
		/// </summary>
		private Logger logger = new Logger("WorkspaceHomeController");

		/// <summary>
		/// ctor
		/// </summary>
		public WorkspaceHomeController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader, ICommonDataMapper commonDataMapper, SiteMasterUtilities siteMasterUtilities, SystemMetrics systemMetrics, IGenBOEControllerLogic genBOEControllerLogic)
			: base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.commonDataMapper = commonDataMapper;
			this.siteMasterUtilities = siteMasterUtilities;
			this.systemMetrics = systemMetrics;
			this.genBOEControllerLogic = genBOEControllerLogic;
		}
		#endregion

		/// <summary>
		/// Gets the site menu items based on the workspace short name.
		/// </summary>
		/// <param name="workspaceShortname">Shortspace Name</param>
		/// <returns>Workspace menu items.</returns>
		[HttpGet]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public IESResponse<GenBOEMasterMenuItemModelView> GetWorkspaceMenuItems(string workspaceShortname)
		{
			IESResponse<GenBOEMasterMenuItemModelView> result = new IESResponse<GenBOEMasterMenuItemModelView>();

			try
			{
				FullWorkspace ws = this.Factory.CreateFullWorkspace(workspaceShortname);
				SecurityAuthorization permission = this.CheckPermission(SecurityPage.WorkspaceHome, ws);

				// Only return data if the permissions is at a read level or above.
				if (permission >= SecurityAuthorization.Read)
				{
					// Uses the existing GenBOE Controller because there is post processing logic happening after the site menu is constructed. This eliminates the need to replicate the logic here and confines it to one spot.
					using (GenBOEController controller = new GenBOEController(SecurityAccess, commonDataMapper, siteMasterUtilities, systemMetrics, Factory, UserLoader, PermissionsLoader, genBOEControllerLogic))
					{
						ICollection<GenBOEMasterMenuItemModelView> menuItems = new Collection<GenBOEMasterMenuItemModelView>();

						ViewResult viewResult = controller.DisplaySiteMasterMenu(workspaceShortname);

						if (viewResult != null)
						{
							menuItems = (ICollection<GenBOEMasterMenuItemModelView>)viewResult.Model;
						}
						else
						{
							throw new GenValidationException("Workspace menu could not be constructed.");
						}

						result.Data = menuItems;
						result.IsSuccessful = true;
					}
				}
				else
				{
					result.Messages.Add($"Insufficient permissions for returning the Workspace menu data.");
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred returning Workspace menu data: {ex.Message}");
			}

			return result;
		}
	}
}