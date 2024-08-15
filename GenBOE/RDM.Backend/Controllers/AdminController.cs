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
	using System.Globalization;
	using System.Linq;
	using System.Transactions;
	using IES.ActionLogic.Core.ControllerLogic;
	using IES.Common.Core;
	using IES.Common.Core.Configuration;
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
	/// Controller for Administration.
	/// </summary>
	[Authorize]
	[Route("api/Admin")]
	public class AdminController : RDMController
	{
		/// <summary>
		/// The Admin Controller Logic.
		/// </summary>
		private readonly IAdminControllerLogic controllerLogic;

		/// <summary>
		/// The Home Controller Logic.
		/// </summary>
		private readonly IHomeControllerLogic homeControllerLogic;

		/// <summary>
		/// Cobra Mapping Detail Loader
		/// </summary>
		private readonly ICobraDetailLoader cobraDetailLoader;

		/// <summary>
		/// Cobra Years Loader
		/// </summary>
		private readonly ICobraYearsLoader cobraYearsLoader;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="controllerLogic">Admin Controller Logic.</param>
		/// <param name="homeControllerLogic">Home Controller Logic.</param>
		/// <param name="cobraDetailLoader">Cobra Mapping Detail Loader</param>
		/// <param name="cobraYearsLoader">Cobra Years Loader</param>
		/// <param name="whosOnlineLoader">Who's Online Loader</param>
		public AdminController(ISecurityInformation securityInformation, IAdminControllerLogic controllerLogic,
			IHomeControllerLogic homeControllerLogic, ICobraDetailLoader cobraDetailLoader,
			ICobraYearsLoader cobraYearsLoader, IWhosOnlineLoader whosOnlineLoader, IConfiguration configuration,
			ILogger<AdminController> logger)
			: base(securityInformation, whosOnlineLoader, controllerLogic, configuration, logger)
		{
			this.controllerLogic = controllerLogic;
			this.homeControllerLogic = homeControllerLogic;
			this.cobraDetailLoader = cobraDetailLoader;
			this.cobraYearsLoader = cobraYearsLoader;
		}

		/// <summary>
		/// Gets the COBRA mapping details for specified revision.
		/// </summary>
		/// <param name="id">The revision Id.</param>
		/// <returns>COBRA mapping details for the version requested</returns>
		[HttpGet("[action]")]
		public ActionResult GetCobraDetailsByVersion(int? id)
		{
			IESResponse<CobraGridModelView> response = new();

			try
			{
				ICollection<RevisionModelView> revisions = this.Logic.Revisions;

				RevisionModelView revision = id.HasValue ? revisions.FirstOrDefault(r => r.Id == id.Value) : revisions.LastOrDefault();
				if (revision == null)
				{
					throw new GenValidationException("Revision not found.");
				}

				CobraGridModelView model = new()
				{
					SelectedRevisionId = revision.Id,
					Versions = this.Logic.RevisionMediator.GetRevisionOptions(revisions),
					CobraDetails = this.cobraDetailLoader.GetCobraDetailsByRevision(revision),
					CobraCodes = ExtensionMethods.GetOptions<Code1>().OrderBy(x => x.Label).ToList(),
					LockInfo = this.homeControllerLogic.GetCurrentLockInfo(LockArea.CobraData)
				};

				response.Data = model;
				response.IsSuccessful = true;
			}
			catch (Exception ex)
			{
				this.log.LogError(ex, "Unknown Exception");
				response.Messages.Add(ex.Message);
			}

			return this.Json(response);
		}

		/// <summary>
		/// Saves the specified collection.
		/// POST: SaveCobraDetails
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <returns>Json Result of the saved COBRA mapping details.</returns>
		[HttpPost("[action]")]
		public ActionResult SaveCobraDetails(CobraDetailModelView[] collection)
		{
			IESResponse<ICollection<CobraDetailModelView>> response = new();
			try
			{

				if (collection == null || collection.None())
				{
					throw new GenValidationException("There are no COBRA mapping details being saved.");
				}

				// Confirm that either user owns lock or area is unlocked
				this.Logic.VerifyLockForSaving(LockArea.CobraData, collection[0].RevisionId);

				RevisionModelView revision = this.Logic.RevisionMediator.GetById(collection[0].RevisionId);
				ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateCobraDetailModelViews(collection);
				if (validationErrors.Any())
				{
					throw new GenValidationException(validationErrors);
				}

				using (TransactionScope scope = new(TransactionScopeOption.Required,
					new TransactionOptions
					{
						IsolationLevel = IsolationLevel.Snapshot,
						Timeout = new TimeSpan(0, 0, 2 * ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", CommonConstants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT))
					}))
				{
					this.cobraDetailLoader.SaveDetails(collection);
					scope.Complete();
				}

				response.Data = this.cobraDetailLoader.GetCobraDetailsByRevision(revision);
				response.IsSuccessful = true;
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			return this.Json(response);
		}

		/// <summary>
		/// Gets the Rate Code Replication.
		/// </summary>
		/// <returns>The Rate Code Replication</returns>
		[HttpGet("[action]")]
		public ActionResult GetRateCodeReplication()
		{
			RateCodeReplicationModelView model = this.controllerLogic.GetRateCodeReplication();
			return this.Json(model);
		}

		/// <summary>
		/// Saves the Rate Code Replication.
		/// </summary>
		/// <param name="rateCodes">The Rate Codes to save.</param>
		/// <returns>The JSON result of the save.</returns>
		[HttpPost("[action]")]
		public ActionResult SaveRateCodeReplication(RateCodeModelView[] rateCodes)
		{
			if (rateCodes == null)
			{
				throw new ArgumentNullException(nameof(rateCodes));
			}
			
			IESResponse<bool> response = new();
			try
			{

				ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateRateCodeReplication(rateCodes);

				if (validationErrors.Any())
				{
					throw new GenValidationException(validationErrors);
				}

				foreach (RateCodeModelView rate in rateCodes)
				{
					if (rate.IsDeleted)
					{
						rate.Updateable = UpdateType.Deleted;
					}
					else
					{
						rate.Updateable = UpdateType.Upsert;
					}
				}

				using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
				{
					// save them
					this.controllerLogic.SaveRateCodeReplication(rateCodes);
					scope.Complete();
				}

				response.IsSuccessful = true;
				response.Data = true; 
				// Old return { Status = true, Message = "Rate Code Replication has been saved." }
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			return this.Json(response);
		}

		/// <summary>
		/// Gets the WIP Revision Configuration.
		/// </summary>
		/// <returns>The WIP Revision information</returns>
		[HttpGet("[action]")]
		public ActionResult GetRevisionConfiguration()
		{
			return this.Json(this.Logic.WipRevision);
		}

		/// <summary>
		/// Gets the menu options.
		/// </summary>
		/// <param name="menuOptionControllerName">The name of the controller for the selected menu option.</param>
		/// <returns>The options.</returns>
		[HttpGet("[action]")]
		public ActionResult GetMenuOptions(string menuOptionControllerName)
		{
			// Set to lower case to be used for comparison to determine the selected controller.
			menuOptionControllerName = menuOptionControllerName != null
				? menuOptionControllerName.ToLower(CultureInfo.CurrentCulture)
				: string.Empty;

			// Create the menu.
			Collection<MenuOptionModelView> menu = new()
			{
				this.GetHomeMenuOption(menuOptionControllerName),
				this.GetPprdMenuOption(menuOptionControllerName),
				this.GetRatesMenuOption(menuOptionControllerName),
				this.GetReportsMenuOption(menuOptionControllerName),
				this.GetManageFileAttachmentsMenuOption(menuOptionControllerName, false),
				this.GetAdminMenuOption(menuOptionControllerName)
			};

			return this.Json(new { menu });
		}

		/// <summary>
		/// Gets the Cobra Year Configuration mappings.
		/// </summary>
		/// <returns>The Year to CobraDate mappings.</returns>
		[HttpGet("[action]")]
		public ActionResult GetCobraYearConfiguration()
		{
			ICollection<CobraYearGridModelView> cobraYearMappings = this.cobraYearsLoader.GetAll();

			return this.Json(new { cobraYearMappings });
		}

		/// <summary>
		/// Saves the Cobra Year Configuration mappings.
		/// </summary>
		/// <param name="dataToSave">The collection of configurations to save. We get a list of items that we want to save, 
		/// needing to remove anything that is no longer in the collection</param>
		/// <returns>The JSON result of the save.</returns>
		[HttpPost("[action]")]
		public ActionResult SaveCobraYearConfiguration(CobraYearGridModelView[] dataToSave)
		{
			IESResponse<bool> response = new();
			try
			{
				ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateCobraYearConfigurations(dataToSave);

				if (validationErrors.Any())
				{
					throw new GenValidationException(validationErrors);
				}

				int newId = -1;

				// retrieve all of the current mappings.. mark them all as deleted
				List<CobraYearGridModelView> currentMappings = this.cobraYearsLoader.GetAll().ToList();
				currentMappings.ForEach(x => x.Updateable = UpdateType.Deleted);

				// mark all of the data as upsert, this is what we are keeping, appropriately setting IDs for the new items
				dataToSave.ToList().ForEach(x => { x.Updateable = UpdateType.Upsert; x.Id = x.Id > 0 ? x.Id : newId--; });

				// remove from the current mappings any that we are keeping
				dataToSave.Where(itemToSave => itemToSave.Id > 0).ToList().ForEach(itemToSave => currentMappings.Remove(currentMappings.First(current => current.Id == itemToSave.Id)));

				// merge them together
				dataToSave.AddRange(currentMappings);

				using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
				{
					// save them
					this.cobraYearsLoader.Save(dataToSave);
					scope.Complete();
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

		/// <summary>
		/// Saves the Revision Configuration start and end years.
		/// </summary>
		/// <param name="revisionConfiguration">Revision configuration to save.</param>        
		/// <returns>The JSON result of the save.</returns>
		[HttpPost("[action]")]
		public ActionResult SaveRevisionConfiguration(RevisionModelView revisionConfiguration)
		{
			IESResponse<bool> response = new();
			try
			{
				ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateRateYearConfiguration(revisionConfiguration.StartYear.ToString(), revisionConfiguration.EndYear.ToString());

				if (validationErrors.Any())
				{
					throw new GenValidationException(validationErrors);
				}

				// retrieve the current WIP revision and set the start/end year
				RevisionModelView revision = this.Logic.WipRevision;

				if (revision == null)
				{
					throw new GenValidationException("Revision Configuration could not be saved.  Unable to get current WIP revision.");
				}
				else
				{
					using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
					{
						revision.StartYear = revisionConfiguration.StartYear;
						revision.EndYear = revisionConfiguration.EndYear;
						revision.History = revisionConfiguration.History;
						revision.ReleaseNotes = revisionConfiguration.ReleaseNotes;
						revision.Updateable = UpdateType.Upsert;
						this.Logic.RevisionMediator.Upsert(revision);
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

			// For reference: { Status = true, Message = "Revision Configuration has been saved." }
			return this.Json(response);
		}

		/// <summary>
		/// RESTful endpoint to clear the Revision cache.
		/// </summary>
		/// <returns>JSON object with status of request.</returns>
		[HttpGet("[action]")]
		public ActionResult ClearRevisionCache()
		{
			this.controllerLogic.ClearRevisionCache();
			return this.Json(new { Status = true });    // success - revision cache cleared
		}
	}
}