// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Reports.Backend.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using GenBOE.DataBridge.Core;
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using GenBOE.DataBridge.Core.DTO.FullObjects;
	using GenBOE.Reports.Backend.Services;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.OfficeUtilities;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;

	[Route("Reports")]
	public class ReportsController : IES.Common.Core.IESController
	{
		private IBoeExportService boeExportService;

		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="logger">logger</param>
		/// <param name="boeExportService">word export service</param>
		/// <param name="securityInformation">security information</param>
		/// <param name="configuration">configuration</param>
		public ReportsController(ILogger<ReportsController> logger, IBoeExportService boeExportService,
			ISecurityInformation securityInformation, IConfiguration configuration) : base(logger, securityInformation, configuration)
		{
			this.boeExportService = boeExportService;
		}

		/// <summary>
		/// Exports BOE(s) to Word document, may be zipped
		/// </summary>
		/// <param name="workspace">The current workspace</param>
		/// <param name="selectedComponents">List of BOEs to be included in the report; if null, then include ALL</param>
		/// <param name="isCustomExport">Flag indicating wheter the export is a custom export</param>
		/// <param name="wsExportFormatDTO">the Workspace Format DTO</param>
		/// <param name="exportInputs">the export inputs</param>
		/// <param name="boeExportModelViews">the boe export model views</param>
		/// <param name="boeSummaryGridModelViews">the boe summary grid model veiws</param>
		/// <param name="segmentedOutput">Should the output be broken into segments and zipped</param>
		[HttpPost("[action]")]
		public async Task<IActionResult> ExportBoeToWord(
			FullWorkspace workspace,
			ICollection<BoeCustomReportComponent> selectedComponents,
			bool isCustomExport,
			WorkspaceExportFormatDTO wsExportFormatDTO,
			BOEExportInputs exportInputs,
			ICollection<BOEExportModelView> boeExportModelViews,
			List<BOESummaryGridModelView> boeSummaryGridModelViews,
			bool segmentedOutput)
		{
			string tempFileLocation = string.Empty;
			string exportedFileName = string.Empty;
			try
			{
				FileStream fs;
				if (segmentedOutput)
				{
					tempFileLocation = this.boeExportService.ExportBoeToZip(workspace, selectedComponents, isCustomExport, wsExportFormatDTO,
						exportInputs, boeExportModelViews, boeSummaryGridModelViews);

					fs = new(tempFileLocation, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
					exportedFileName = string.Format("genBOEExport-{0}.zip", workspace.WorkspaceName).Replace(",", string.Empty);
				}
				else
				{
					tempFileLocation = Path.GetTempFileName();
					fs = new(tempFileLocation, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
					
					exportedFileName = this.boeExportService.ExportBoeToWord(fs, workspace, selectedComponents, isCustomExport, wsExportFormatDTO,
						exportInputs, boeExportModelViews, boeSummaryGridModelViews);
				}

				// Generate a custom ActionResult to cause a file download to the client
				
				fs.Seek(0, SeekOrigin.Begin);

				return this.File(
					fileStream: fs,
					contentType: segmentedOutput ?  ExportFileDownloadBase.ContentType_ZIP : ExportFileDownloadBase.ContentType_DOCX,
					fileDownloadName: exportedFileName);
			}
			catch (Exception e)
			{
				if (!string.IsNullOrWhiteSpace(exportedFileName) && System.IO.File.Exists(exportedFileName))
				{
					try
					{
						System.IO.File.Delete(exportedFileName);
					}
					catch
					{
						// if it errored out, that is ok
					}
				}

				this.log.LogError(e, "Unknown Exception.");
				return await this.CreateTextFileWithErrorMessage(e.Message);
			}
		}


		[HttpGet(Name = "GetWeatherForecast")]
		public IEnumerable<WeatherForecast> Get()
		{
			this.log.LogDebug("Company Mode is " + SystemConfiguration.Instance().CompanyMode.ToString());
			this.log.LogDebug("Inside weather forecast get");
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}
	}
}
