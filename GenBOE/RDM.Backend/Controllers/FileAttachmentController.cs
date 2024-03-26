// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Web.Controllers
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
	using RDM.Web.Common;

	/// <summary>
	/// Controller for Managing File Attachments
	/// </summary>
	/// <seealso cref="RDMController" />
	[Authorize]
	[Route("api/FileAttachment")]
	public class FileAttachmentController : RDMController
	{
		/// <summary>
		/// The controller logic.
		/// </summary>
		private readonly IFileAttachmentControllerLogic controllerLogic;

		/// <summary>
		/// The section loader.
		/// </summary>
		private readonly ISectionLoader sectionLoader;

		/// <summary>
		/// The file attachment loader
		/// </summary>
		private readonly IFileAttachmentLoader fileAttachmentLoader;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="whosOnlineLoader">Who's Online Loader</param>
		/// <param name="controllerLogic">File Attachment Controller Logic</param>
		/// <param name="sectionLoader">The section loader.</param>
		/// <param name="fileAttachmentLoader">The file attachment loader.</param>
		public FileAttachmentController(IWhosOnlineLoader whosOnlineLoader, IFileAttachmentControllerLogic controllerLogic,
			ISectionLoader sectionLoader, IFileAttachmentLoader fileAttachmentLoader,
			ISecurityInformation securityInformation, IConfiguration configuration, ILogger<FileAttachmentController> logger)
			: base(securityInformation, whosOnlineLoader, controllerLogic, configuration, logger)
		{
			this.controllerLogic = controllerLogic;
			this.sectionLoader = sectionLoader;
			this.fileAttachmentLoader = fileAttachmentLoader;
		}

		/// <summary>
		/// Get the File Attachments information for the specified versionId (if provided).
		/// </summary>
		/// <param name="versionId">Optional revisionId.</param>
		/// <returns>File Attachments Grid data for specified revisionId.  If revisionId is null, return data for WIP version.</returns>
		[HttpGet("[action]")]
		public ActionResult GetFileAttachments(int? versionId)
		{
			IESResponse<FileAttachmentGridModelView> response = new();
			try
			{
				ICollection<RevisionModelView> revisions = this.Logic.Revisions;

				RevisionModelView revision = versionId.HasValue ? revisions.FirstOrDefault(r => r.Id == versionId.Value) : revisions.LastOrDefault();
				if (revision == null)
				{
					throw new GenValidationException("Revision not found.");
				}

				FileAttachmentGridModelView model = new()
				{
					SelectedRevisionId = revision.Id,
					Sections = this.sectionLoader.RetrieveSectionsAsOptions(revision),
					Versions = this.Logic.RevisionMediator.GetRevisionOptions(revisions),
					FileAttachments = this.fileAttachmentLoader.GetByRevision(revision.Id),
					LockInfo = this.controllerLogic.GetCurrentLockInfo(LockArea.FileAttachments)
				};

				response.Data = model;
				response.IsSuccessful = true;
			}
			catch (GenValidationException ex)
			{
				response.Messages = ex.GetValidationMessages(ex.ValidationList);
			}

			return this.Json(response);
		}

		/// <summary>
		/// Saves the specified collection.
		/// POST: File Attachments/Save
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <returns>Json Result of the save.</returns>
		[HttpPost("[action]")]
		public ActionResult Save(FileAttachmentRowModelView[] collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			IESResponse<bool> response = new();
			try
			{

				// Client can add new rows, then delete them all - catch it here.
				Collection<FileAttachmentRowModelView> filteredCollection = new();
				foreach (FileAttachmentRowModelView mv in collection)
				{
					if (mv.Id > 0 || !mv.IsDeleted)
					{
						filteredCollection.Add(mv);
					}
				}

				if (filteredCollection.Any())
				{
					// If we don't have the revisionId in our filteredCollection, get the WIP from DB.
					int revisionId = 0;
					FileAttachmentRowModelView revisionFA = filteredCollection.FirstOrDefault(x => x.RevisionID > 0);

					if (revisionFA != null)
					{
						revisionId = revisionFA.RevisionID;
					}

					// If we have no revisionIds - get the WIP revision.
					if (revisionId == 0)
					{
						revisionId = this.Logic.WipRevision.Id;
					}

					// Confirm that either user owns lock or area is unlocked
					this.Logic.VerifyLockForSaving(LockArea.FileAttachments, revisionId);

					// Get the revision's Sections
					ICollection<OptionModelView> sections = this.sectionLoader.RetrieveSectionsAsOptions(new RevisionModelView { Id = revisionId });
					ICollection<ValidationMessage> validationErrors = this.controllerLogic.ValidateFileAttachments(filteredCollection, sections);
					if (validationErrors.Any())
					{
						throw new GenValidationException(validationErrors);
					}

					using (TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }))
					{
						foreach (FileAttachmentRowModelView filtered in filteredCollection)
						{
							filtered.RevisionID = revisionId;
							this.fileAttachmentLoader.Save(filtered);
						}

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
