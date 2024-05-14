// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
	using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using IES.Common.Core.Utilities;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using RDM.Backend.Common;

	/// <summary>
	/// Controller for the PPR&amp;D Document.
	/// </summary>
	[Authorize]
	[Route("api/PPRD")]
	public class PPRDController : RDMController
	{
		/// <summary>
		/// Home Controller Logic
		/// </summary>
		private readonly IHomeControllerLogic homeControllerLogic;

		/// <summary>
		/// The Burden Pool Controller Logic.
		/// </summary>
		private readonly IPPRDControllerLogic controllerLogic;

		/// <summary>
		/// Section Loader
		/// </summary>
		private readonly ISectionLoader sectionLoader;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="controllerLogic">Controller Logic</param>
		/// <param name="sectionLoader">Section Loader</param>
		/// <param name="homeControllerLogic">Home Controller Logic</param>
		/// <param name="whosOnlineLoader">Who's Online Loader</param>
		public PPRDController(IPPRDControllerLogic controllerLogic,
			ISectionLoader sectionLoader, IHomeControllerLogic homeControllerLogic,
			IWhosOnlineLoader whosOnlineLoader, ISecurityInformation securityInformation,
			IConfiguration configuration, ILogger<PPRDController> logger)
		: base(securityInformation, whosOnlineLoader, controllerLogic, configuration, logger)
		{
			this.controllerLogic = controllerLogic;
			this.sectionLoader = sectionLoader;
			this.homeControllerLogic = homeControllerLogic;
		}

		/// <summary>
		/// Gets the PPR&amp;D data
		/// </summary>
		/// <returns>PPR&amp;D data</returns>
		[HttpGet("[action]")]
		public ActionResult GetPPRD()
		{
			RevisionModelView revision = this.Logic.WipRevision;
			PPRDModelView pprd = new()
			{
				Revision = revision,
				SectionContentTypeOptions = ExtensionMethods.GetOptions<SectionContentType>().OrderBy(x => x.Label).ToList(),
				ChildNodes = this.sectionLoader.RetrieveAllSections(revision),
				LockInfo = this.homeControllerLogic.GetCurrentLockInfo(LockArea.RDMSections)
			};

			return this.Json(pprd);
		}

		/// <summary>
		/// Saves the specified PPR&amp;D document.
		/// POST: Section/Save
		/// </summary>
		/// <param name="pprd">The PPR&amp;D document to be saved.</param>
		/// <returns>Json Result of the save.</returns>
		[HttpPost("[action]")]
		public ActionResult Save(PPRDModelView pprd)
		{
			if (pprd == null)
			{
				throw new ArgumentNullException(nameof(pprd));
			}

			IESResponse<bool> response = new();
			try
			{
				#region Validate

				Collection<ValidationMessage> errors = new();
				if (!this.ModelState.IsValid)
				{
					errors = CommonUtilities.CreateModelStateValidationErrorList(this.ModelState);
				}

				this.controllerLogic.ValidateSections(pprd.ChildNodes, errors);
				if (errors.Any())
				{
					throw new GenValidationException(errors);
				}

				#endregion

				// Confirm that either user owns lock or area is unlocked
				this.Logic.VerifyLockForSaving(LockArea.RDMSections, pprd.Revision.Id);

				#region Save

				using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
				{
					this.sectionLoader.UpdateSectionsAndContent(pprd.Revision, pprd.ChildNodes);
					scope.Complete();
				}

				#endregion

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
		/// Validates the delete section.
		/// </summary>
		/// <param name="sectionIds">The section ids.</param>
		/// <returns>Json Result of the validation.</returns>
		[HttpPost("[action]")]
		public ActionResult ValidateDeleteSection(int[] sectionIds)
		{
			IESResponse<bool> response = new();
			try
			{
				Collection<ValidationMessage> errors = new();
				RevisionModelView revision = this.Logic.WipRevision;
				ICollection<OptionModelView> sections = this.sectionLoader.RetrieveSectionsAsOptions(revision);
				this.controllerLogic.ValidateDeletionSections(sectionIds, errors, sections, revision);
				if (errors.Any())
				{
					throw new GenValidationException(errors);
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
