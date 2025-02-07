// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Backend.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Transactions;
	using IES.ActionLogic.Core.ControllerLogic;
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
	/// Controller for the Versions.
	/// </summary>
	[Authorize]
	[Route("api/Version")]
	public class VersionController : RDMController
	{
		/// <summary>
		/// Version Controller Logic
		/// </summary>
		private readonly IVersionControllerLogic controllerLogic;

		/// <summary>
		/// Home Controller Logic
		/// </summary>
		private readonly IHomeControllerLogic homeControllerLogic;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="controllerLogic">Version Controller Logic</param>
		/// <param name="homeControllerLogic">Home Controller Logic</param>
		/// <param name="whosOnlineLoader">Who's Online Loader</param>
		public VersionController(IVersionControllerLogic controllerLogic, IHomeControllerLogic homeControllerLogic,
			IWhosOnlineLoader whosOnlineLoader, ISecurityInformation securityInformation, IConfiguration configuration,
			ILogger<VersionController> logger)
			: base(securityInformation, whosOnlineLoader, controllerLogic, configuration, logger)
		{
			this.controllerLogic = controllerLogic;
			this.homeControllerLogic = homeControllerLogic;
		}

		/// <summary>
		/// Get the differences for the Version Comparison grid when a new Version is selected
		/// </summary>
		/// <param name="id">Version Selected</param>
		/// <param name="secondId">Second selected revision ID, or -1 to return previous revision</param>
		/// <returns>Updated differences based on the selected version</returns>
		[HttpGet("[action]")]
		public ActionResult GetVersionDifferences(int id, int secondId)
		{
			IESResponse<VersionComparisonModelView> response = new();
			try
			{
				response.Data = this.controllerLogic.GetVersionDifferences(id, secondId, this.Logic.IsRDMAdminUser, this.Logic.ActiveUser);
				response.IsSuccessful = true;
			}
			catch (ArgumentException ae)
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
		/// Validates the rates for the selected revision to see if any rates would have all 0 values for the displayed years prior to publishing.
		/// </summary>
		/// <param name="id">The revision to validate.</param>
		/// <returns>JsonResult containing whether the rates were valid.</returns>
		[HttpGet("[action]")]
		public ActionResult ValidateRates(int? id)
		{
			ICollection<string> invalidRates = this.controllerLogic.GetInvalidRates(id);

			return this.Json(new { Valid = !invalidRates.Any(), InvalidRates = invalidRates });
		}

		/// <summary>
		/// Publish the current WIP revision.
		/// </summary>
		/// <param name="revision">The revision of the WIP to publish.</param>
		/// <param name="history">The updated history for publishing.</param>
		/// <param name="releaseNotes">The release notes for publishing.</param>
		/// <returns>JsonResult containing the Id of the new WIP revision.</returns>
		[HttpPost("[action]")]
		public ActionResult Publish(string revision, string history, string releaseNotes)
		{
			int? newId = null;
			IESResponse<(int?, ICollection<AreaLockData>)> response = new();

			string message = null;
			try
			{
				// lock the revision (all areas)
				this.homeControllerLogic.LockAllAreas(out bool status, out message);
			
				if (status)
				{
					this.log.LogDebug($"Revision locked successfully by {this.Logic.ActiveUser.DisplayName}.");
					RevisionModelView wipRevision = this.Logic.WipRevision;

					if (wipRevision.Revision != revision)
					{
						message = $"Publish failed - Revision {revision} is no longer the WIP.  Please refresh the page.";
					}
					else
					{
						wipRevision.History = history;
						wipRevision.ReleaseNotes = releaseNotes;

						using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
						{
							newId = this.Logic.RevisionMediator.Publish(wipRevision, this.Logic.ActiveUser.DisplayName);
							scope.Complete();
						}

						RevisionModelView publishedRevision = this.Logic.RevisionMediator.GetById(wipRevision.Id);
						this.controllerLogic.SendPublishEmail(publishedRevision, this.Logic.ActiveUser);

						message = "Publish operation was successful.";
						response.IsSuccessful = true;
					}
				}
				else
				{
					message = $"Publish failed - {message}";
					this.log.LogDebug(message);
				}
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}
			catch (GeneralAppException ex)
			{
				message = "Publish failed due to a system exception.";
				this.log.LogError(ex, message);
			}
			finally
			{
				ICollection<AreaLockData> locks = this.homeControllerLogic.UnlockAllAreas();
				response.Data = new(newId, locks);

				if (!string.IsNullOrWhiteSpace(message))
				{
					response.Messages.Add(message);
				}
			}

			return this.Json(response);

			// Historical reference { Status = status, Id = newId, ActiveLocks = activeLocks, Message = message }
		}

		/// <summary>
		/// Rollback the current WIP revision.
		/// </summary>
		/// <param name="revision">The revision of the WIP to publish.</param>
		/// <returns>JsonResult containing the Id of the new WIP revision.</returns>
		[HttpPost("[action]")]
		public ActionResult Rollback(string revision)
		{
			int? id = null;
			IESResponse<(int?, ICollection<AreaLockData>)> response = new();
			string message = null;

			try
			{
				// lock the revision (all areas)
				this.homeControllerLogic.LockAllAreas(out bool status, out message);
			
				if (status)
				{
					this.log.LogDebug($"Revision locked successfully by {this.Logic.ActiveUser.DisplayName}.");

					RevisionModelView wipRevision = this.Logic.WipRevision;

					if (wipRevision.Revision != revision)
					{
						message = $"Rollback failed - Revision {revision} is no longer the WIP.  Please refresh the page.";
					}
					else
					{
						RevisionModelView lastPublishedRevision = this.Logic.LastPublishedRevision;
						using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
						{
							id = this.Logic.RevisionMediator.Rollback(lastPublishedRevision, wipRevision);
							scope.Complete();
						}

						message = "Rollback operation was successful.";
						response.IsSuccessful = true;
					}
				}
				else
				{
					message = $"Rollback failed - {message}";
					this.log.LogDebug(message);
				}
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}
			catch (GeneralAppException ex)
			{
				message = "Rollback failed due to a system exception.";
				this.log.LogError(ex, message);
			}
			finally
			{
				ICollection<AreaLockData> locks = this.homeControllerLogic.UnlockAllAreas();
				response.Data = new(id, locks);

				if (!string.IsNullOrWhiteSpace(message))
				{
					response.Messages.Add(message);
				}
			}

			return this.Json(response);

			// Historical reference { Status = status, Id = id, ActiveLocks = activeLocks, Message = message }
		}
	}
}