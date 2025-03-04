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
	using GenBOE.DataBridge.Core.DTO.Export.BOE;
	using GenBOE.DataBridge.Core.WorkspaceExportFormat;
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
			//FullWorkspace workspace,
			ICollection<BoeCustomReportComponent> selectedComponents,
			bool isCustomExport,
			WorkspaceExportFormatDTO wsExportFormatDTO,
			BOEExportInputs exportInputs,
			ICollection<BOEExportModelView> boeExportModelViews,
			List<BOESummaryGridModelView> boeSummaryGridModelViews,
			bool segmentedOutput)
		{
			try
			{
				string exportedFileName = this.boeExportService.ExportBoeToWord(selectedComponents, isCustomExport, wsExportFormatDTO,
					exportInputs, boeExportModelViews, boeSummaryGridModelViews, segmentedOutput);

				//string serverFileName = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/Templates/Export/RateCodesImportExample.xlsx");
				//RateGridModelView rates = this.controllerLogic.GetRatesByVersion(id, this.Logic.Revisions);

				// Call the export function and get back the file name of the populated file.
				//string exportedFileName = RateCodeExporter.ExportToExcelFile(serverFileName, rates);

				// Generate a custom ActionResult to cause a file download to the client
				string fileName = Path.GetFileName(exportedFileName);
				// Generate a custom ActionResult to cause a file download to the client
				FileStream fs = new(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

				return this.File(
					fileStream: fs,
					contentType: ExportFileDownloadBase.GetContentType(fileName),
					fileDownloadName: fileName);
			}
			catch (Exception e)
			{
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
