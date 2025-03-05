// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Backend.Controllers
{
	using System;
	using System.Collections.Generic;
	using IES.ActionLogic.Core.ControllerLogic;
	using IES.Common.Core;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using RDM.Backend.Common;

	/// <summary>
	/// The Home Controller.
	/// </summary>
	[Authorize]
	[Route("api/Home")]
	public class HomeController : RDMController
	{
		/// <summary>
		/// Home Controller Logic
		/// </summary>
		private readonly IHomeControllerLogic controllerLogic;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="controllerLogic">Home Controller Logic</param>
		/// <param name="whosOnlineLoader">Who's Online Loader</param>
		public HomeController(IHomeControllerLogic controllerLogic, IWhosOnlineLoader whosOnlineLoader,
			ISecurityInformation securityInformation, IConfiguration configuration, ILogger<HomeController> logger)
		: base(securityInformation, whosOnlineLoader, controllerLogic, configuration, logger)
		{
			this.controllerLogic = controllerLogic;
		}

		/// <summary>
		/// Gets the PPR&amp;D revisions
		/// </summary>
		/// <returns>PPR&amp;D revisions</returns>
		[HttpGet("[action]")]
		public ActionResult GetRevisions()
		{
			RevisionGridModelView model = new()
			{
				Revisions = this.Logic.Revisions,
				IsRdmAdminUser = this.Logic.IsRDMAdminUser,
				IsRdmCobraAdminUser = this.Logic.IsRDMCobraAdminUser
			};

			return this.Json(model);
		}

		/// <summary>
		/// Attempts to lock the PPR&amp;D revision for edit
		/// </summary>
		/// <param name="id">Area to lock</param>
		/// <returns>user's display name if revision locked; otherwise throws exceptions.</returns>
		[HttpGet("[action]")]
		public ActionResult Lock(LockArea id)
		{
			IESResponse<LockModelView> response = new();
			try
			{
				ValidateArea(id);

				LockModelView lockInfo = this.controllerLogic.LockArea(id, false, out bool status, out string message);

				this.log.LogDebug(
					status
						? $"Lock successfully created in {id.ToDescription()} by {this.Logic.ActiveUser.DisplayName}."
						: $"Failed to create lock in {id.ToDescription()} for {this.Logic.ActiveUser.DisplayName}. {message}");

				response.Data = lockInfo;
				response.IsSuccessful = status;
				if (!status)
				{
					response.Messages.Add(message);
				}
			}
			catch (AuthorizationException ae)
			{
				response.Messages.Add(ae.Message);
			}
			catch (NotImplementedException ne)
			{
				string message = $"Error Locking LockArea: {id}";
				this.log.LogError(ne, message);
				response.Messages.Add(message);
				response.Messages.Add(ne.Message);
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			return this.Json(response);
		}

		/// <summary>
		/// Get the current lock information
		/// </summary>
		/// <param name="lockArea">Lock Area</param>
		/// <returns>Lock Information</returns>
		[HttpGet("[action]")]
		public LockModelView GetCurrentLockInfo(LockArea lockArea)
		{
			return this.controllerLogic.GetCurrentLockInfo(lockArea);
		}

		/// <summary>
		/// Attempts to unlock the PPR&amp;D revision.
		/// </summary>
		/// <param name="id">Area to lock</param>
		/// <returns>success if revision unlocked; otherwise throws exceptions.</returns>
		[HttpGet("[action]")]
		public ActionResult Unlock(LockArea id)
		{
			IESResponse<LockModelView> response = new();
			try
			{
				ValidateArea(id);

				LockModelView lockInfo = this.controllerLogic.UnlockArea(id, false);

				this.log.LogDebug(lockInfo.InUse == null
					? $"Lock successfully removed in {id.ToDescription()}."
					: $"Failed to remove lock in {id.ToDescription()} because it was locked by {lockInfo.Editing}.");

				response.Data = lockInfo;
				response.IsSuccessful = true;
			}
			catch (AuthorizationException ae)
			{
				response.Messages.Add(ae.Message);
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			return this.Json(response);
		}

		/// <summary>
		/// Attempts to refresh the lock for the PPR&amp;D revision.
		/// Called whenever the editing user performs client-side CRUD actions or requests a lock refresh.
		/// </summary>
		/// <param name="id">Area to lock</param>
		/// <returns>success if revision still locked; otherwise throws exceptions.</returns>
		[HttpGet("[action]")]
		public ActionResult RefreshLock(LockArea id)
		{
			IESResponse<LockModelView> response = new();
			try
			{
				ValidateArea(id);

			LockModelView lockInfo = this.controllerLogic.RefreshLockOnArea(id, out bool status, out string message);

			this.log.LogDebug(status
				? $"Lock successfully refreshed in {id.ToDescription()} by {this.Logic.ActiveUser.DisplayName}."
				: $"Failed to refresh lock in {id.ToDescription()} for {this.Logic.ActiveUser.DisplayName}. {message}");

				response.Data = lockInfo;
				response.IsSuccessful = status;
				if (!status)
				{
					response.Messages.Add(message);
				}
			}
			catch (AuthorizationException ae)
			{
				response.Messages.Add(ae.Message);
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			return this.Json(response);
		}

		/// <summary>
		/// Validates that id for area is within the LockArea enum range
		/// </summary>
		/// <param name="areaId">area id</param>
		[NonAction]
		private static void ValidateArea(LockArea areaId)
		{
			if (!Enum.IsDefined(typeof(LockArea), areaId))
			{
				throw new GenValidationException("Not a valid area to lock.");
			}
		}

		/// <summary>
		/// Get User Roles
		/// </summary>
		/// <returns>User Roles</returns>
		[HttpGet("[action]")]
		public ICollection<string> GetUserRoles()
		{
			List<string> roles = new();

			string ntId = this.securityInformation.ActiveUserNTID;
			if (this.securityInformation.IsRdmAdminUser(ntId))
			{
				roles.Add("SystemAdmin");
			}

			if (this.securityInformation.IsRdmCobraAdminUser(ntId))
			{
				roles.Add("CobraAdmin");
			}

			if (this.securityInformation.IsRdmViewerUser(ntId))
			{
				roles.Add("Viewer");
			}

			return roles;
		}
	}
}