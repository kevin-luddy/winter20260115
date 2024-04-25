// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Backend.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using IES.ActionLogic.Core.ControllerLogic;
	using IES.ActionLogic.Core.IO.Export;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.OfficeUtilities;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using RDM.Backend.Common;

	/// <summary>
	/// Controller for Reports.
	/// </summary>
	[Authorize]
	[Route("api/Reports")]
	public class ReportsController : RDMController
	{
		/// <summary>
		/// Rate Detail Loader
		/// </summary>
		private readonly IRateDetailLoader rateDetailLoader;

		/// <summary>
		/// Cobra Loader
		/// </summary>
		private readonly ICobraYearsLoader cobraLoader;

		/// <summary>
		/// burden pool loader
		/// </summary>
		private readonly IBurdenPoolLoader burdenPoolLoader;

		/// <summary>
		/// Reports controller logic
		/// </summary>
		private readonly IReportsControllerLogic reportsControllerLogic;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="reportsControllerLogic">Reports controller logic</param>
		/// <param name="rateDetailLoader">Rate Detail loader</param>
		/// <param name="cobraLoader">Cobra Loader</param>
		/// <param name="burdenPoolLoader">Burden Pool loader</param>
		/// <param name="whosOnlineLoader">Who's Online Loader</param>
		public ReportsController(IReportsControllerLogic reportsControllerLogic,
			IRateDetailLoader rateDetailLoader, ICobraYearsLoader cobraLoader, IBurdenPoolLoader burdenPoolLoader, IWhosOnlineLoader whosOnlineLoader,
			ISecurityInformation securityInformation, IConfiguration configuration, ILogger<ReportsController> logger)
			: base(securityInformation, whosOnlineLoader, reportsControllerLogic, configuration, logger)
		{
			this.reportsControllerLogic = reportsControllerLogic;
			this.rateDetailLoader = rateDetailLoader;
			this.cobraLoader = cobraLoader;
			this.burdenPoolLoader = burdenPoolLoader;
		}

		/// <summary>
		/// Index for Reports.
		/// </summary>
		/// <param name="id">A possible id to a revision, if passed in.</param>
		/// <returns>Index page for Reports.</returns>
		[HttpGet("[action]")]
		public ActionResult GetReportsModelView(int? id)
		{
			ReportsModelView modelView = new()
			{
				AvailableVersions = this.Logic.RevisionMediator.GetRevisionOptions(),
				AdminUser = this.Logic.IsRDMAdminUser,
				CobraAdminUser = this.Logic.IsRDMCobraAdminUser
			};

			if (!this.Logic.IsRDMAdminUser && !this.Logic.IsRDMCobraAdminUser)
			{
				modelView.AvailableVersions.Remove(modelView.AvailableVersions.First(z => z.Label == CommonConstants.WorkInProgress));
			}

			if (modelView.AvailableVersions.Any())
			{
				// try to use the revision Id passed in
				RevisionOptionModelView revisionOption = modelView.AvailableVersions.FirstOrDefault(v => v.Id == id);

				revisionOption ??= modelView.AvailableVersions.FirstOrDefault();

				if (revisionOption != null)
				{
					modelView.SelectedVersion = revisionOption.Id;
				}
			}

			return this.Json(modelView);
		}

		/// <summary>
		/// Exports the Cobra Data.
		/// </summary>
		/// <param name="id">Revision Id</param>
		/// <returns>An ActionResult.</returns>
		[HttpGet("[action]")]
		public IActionResult ExportCobraData(int id)
		{
			try
			{
				string templateFileLocation = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/Templates/Export/CobraExport.xlsx");

				RevisionModelView revision = this.Logic.RevisionMediator.GetById(id);
				if (revision == null)
				{
					throw new ArgumentException("Unable to export Cobra data - could not find selected revision.");
				}

				int startYear = 2011; // Start year will always be 2011 as per the customer (BOEJ-3286)
				ICollection<CobraExportRowModelView> cobraExportRows =
					this.cobraLoader.GetCobraExportRows(id, startYear);
				string exportedFileName = CobraExporter.ExportToExcelFile(templateFileLocation, cobraExportRows);

				// The filename is hardcoded to make it more obvious what is being replace by string.format().
				string fileDownloadName =
					$"Cobra_RDM-Rev{revision.Revision}_{DateTime.Today.ToString(CommonConstants.DATE_FORMATTING_YEAR_MONTH_DAY)}.xlsx";

				// Generate a custom ActionResult to cause a file download to the client

				FileStream fs = new(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

				return File(
					fileStream: fs,
					contentType: ExportFileDownloadBase.GetContentType(fileDownloadName),
					fileDownloadName: fileDownloadName);
			}
			catch (GeneralAppException ex)
			{
				this.log.LogError(ex, "Unknown Exception");
				return this.CreateTextFileWithErrorMessage(ex);
			}
		}

		/// <summary>
		/// Exports the ProPricer Data.
		/// </summary>
		/// <param name="id">Revision Id</param>
		/// <returns>An ActionResult.</returns>
		[HttpGet("[action]")]
		public IActionResult ExportProPricerData(int id)
		{
			try
			{
				string zipPathFile = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/Templates/Export/ProPricerExport.zip");
				BurdenPoolGridModelView burdenPoolGridModel = this.burdenPoolLoader.GetByRevision(id);
				RevisionModelView selectedRevision = this.Logic.RevisionMediator.GetById(id);
				ICollection<RateDetailModelView> rates = this.rateDetailLoader.GetRatesByRevision(selectedRevision);

				// Generate the zip file containing the ProPricer direct and burden rate exports.
				string exportedFileName = this.reportsControllerLogic.ExportProPricerData(zipPathFile, rates, burdenPoolGridModel.BurdenPools, burdenPoolGridModel.BurdenElements);

				// The filename is hardcoded to make it more obvious what is being replaced by string.format().
				string fileName = $"ProPricer_RDM_Rev{selectedRevision.Revision}_{DateTime.Today.ToString(CommonConstants.DATE_FORMATTING_YEAR_MONTH_DAY)}.zip";


				// Generate a custom ActionResult to cause a file download to the client
				FileStream fs = new(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

				return this.File(
					fileStream: fs,
					contentType: ExportFileDownloadBase.GetContentType(fileName),
					fileDownloadName: fileName);

			}
			catch (GeneralAppException ex)
			{
				this.log.LogError(ex, "Unknown Exception");
				return this.CreateTextFileWithErrorMessage(ex);
			}
		}

		/// <summary>
		/// Generates the Full PPRD and returns a Word document.
		/// </summary>
		/// <param name="id">Revision Id</param>
		/// <param name="portionMarkingRequired">Is Portion Marking Required</param>
		/// <returns>The Word file representing full PPRD.</returns>
		[HttpGet("[action]")]
		public async Task<IActionResult> GenerateFullPPRD(string id, bool? portionMarkingRequired)
		{
			IActionResult result;
			try
			{
				string projectDirectory = Directory.GetCurrentDirectory();
				string serverFileName = Path.Combine(projectDirectory, "Templates", "Export", "PPRDTemplate.docx");
				result = await this.reportsControllerLogic.GenerateFullPPRD(id, serverFileName, portionMarkingRequired);
			}
			catch (GeneralAppException e)
			{
				this.log.LogError(e, "Error generating full PPR&D doc");
				result = this.CreateTextFileWithErrorMessage(e.Message);
			}

			return result;
		}

		/// <summary>
		/// Exports a zip file containing the Revision data as XML.
		/// </summary>
		/// <param name="id">Revision Id</param>
		/// <returns>An ActionResult.</returns>
		[HttpGet("[action]")]
		public IActionResult ExportRevisionAsJson(string id)
		{
			try
			{
				// Create a new random file name in the specified directory
				string exportDirectory = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/Templates/Export/");
				string serverFileName = Path.GetDirectoryName(exportDirectory) + "\\" + Path.GetRandomFileName() + ".json";
				return this.reportsControllerLogic.ExportRevisionAsJson(id, serverFileName);
			}
			catch (GeneralAppException ex)
			{
				this.log.LogError(ex, "Unknown Exception");
				return this.CreateTextFileWithErrorMessage(ex);
			}
		}
	}
}