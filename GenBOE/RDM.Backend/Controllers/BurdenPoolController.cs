// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Backend.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Transactions;
	using IES.ActionLogic.Core.ControllerLogic;
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
	/// The controller for Burden Pools.
	/// </summary>
	[Authorize]
	[Route("api/BurdenPool")]
	public class BurdenPoolController : RDMController
	{
		/// <summary>
		/// The Burden Pool Controller Logic.
		/// </summary>
		private readonly IBurdenPoolControllerLogic controllerLogic;

		/// <summary>
		/// BurdenPoolLoader
		/// </summary>
		private readonly IBurdenPoolLoader burdenPoolLoader;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="controllerLogic">Controller Logic</param>
		/// <param name="burdenPoolLoader">Burden Pool Loader</param>
		/// <param name="whosOnlineLoader">Who's Online Loader</param>
		public BurdenPoolController(IBurdenPoolControllerLogic controllerLogic, IBurdenPoolLoader burdenPoolLoader, IWhosOnlineLoader whosOnlineLoader,
			ISecurityInformation securityInformation, IConfiguration configuration, ILogger<BurdenPoolController> logger)
			: base(securityInformation, whosOnlineLoader, controllerLogic, configuration, logger)
		{
			this.controllerLogic = controllerLogic;
			this.burdenPoolLoader = burdenPoolLoader;
		}

		/// <summary>
		/// Get the Burden Pool Grid information for the specified versionId (if provided).
		/// </summary>
		/// <param name="versionId">Optional versionId.</param>
		/// <returns>Burden Pool Grid data for specified versionId.  If versionId is null, return data for WIP version.</returns>
		[HttpGet("[action]")]
		public ActionResult GetBurdenPools(int? versionId)
		{
			if (!versionId.HasValue)
			{
				versionId = this.Logic.WipRevision.Id;
			}

			BurdenPoolGridModelView model = this.burdenPoolLoader.GetByRevision(versionId.Value);
			model.Versions = this.Logic.RevisionMediator.GetRevisionOptions();
			model.Revision = new RevisionModelView(model.Versions.FirstOrDefault(r => r.Id == versionId.Value));
			model.LockInfo = this.controllerLogic.GetCurrentLockInfo(LockArea.RDMBurdenPools);

			return this.Json(model);
		}

		/// <summary>
		/// Saves the specified collection.
		/// POST: Burden Pool/Save
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <returns>Json Result of the save.</returns>
		[HttpPost("[action]")]
		public ActionResult Save(BurdenPoolDetailModelView[] collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			IESResponse<bool> response = new();
			try
			{
				// Client can add new rows, then delete them all - catch it here.
				Collection<BurdenPoolDetailModelView> filteredCollection = new();
				foreach (BurdenPoolDetailModelView bpdmv in collection)
				{
					if (bpdmv.Id > 0 || !bpdmv.IsDeleted)
					{
						filteredCollection.Add(bpdmv);
					}
				}

				if (filteredCollection.Any())
				{
					// If we don't have the revisionId in our filteredCollection, get the WIP from DB.
					int revisionId = 0;
					BurdenPoolDetailModelView revisionBp = filteredCollection.FirstOrDefault(x => x.RevisionID > 0);
					if (revisionBp != null)
					{
						revisionId = revisionBp.RevisionID;
					}

					// If we have no revisionIds - get the WIP revision.
					if (revisionId == 0)
					{
						revisionId = this.Logic.WipRevision.Id;
					}

					// Confirm that either user owns lock or area is unlocked
					this.Logic.VerifyLockForSaving(LockArea.RDMBurdenPools, revisionId);

					ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateBurdenPools(filteredCollection);
					if (validationErrors.Any())
					{
						throw new GenValidationException(validationErrors);
					}

					using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
					{
						this.burdenPoolLoader.SaveBurdenPools(filteredCollection, revisionId);
						scope.Complete();
					}
				}
				response.Data = true;
				response.IsSuccessful = true;
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			return this.Json(response);
		}
	}
}