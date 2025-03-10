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
	using GenBOE.DataBridge.Core.ModelView;
	using GenBOE.Reports.Backend.Services;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using IES.Common.Core.OfficeUtilities;
	using IES.Common.Core.Utilities;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;

	[Route("Reports")]
	public class ReportsController : IES.Common.Core.IESController
	{
		private readonly IBoeExportService boeExportService;

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
		public async Task<IESResponse<byte[]>> ExportBoeToWord(ExportBoeWordRequestViewModel requestModel)
		{
			IESResponse<byte[]> response = new();
			string tempFileLocation = string.Empty;
			try
			{
				BOEExportInputs exportInputs = new(requestModel);
				FileStream fs;
				if (requestModel.SegmentedOutput)
				{
					tempFileLocation = this.boeExportService.ExportBoeToZip(requestModel.SelectedComponents, requestModel.IsCustomExport, requestModel.ExportFormatDTO,
						exportInputs, requestModel.BoeExportModelViews, requestModel.BoeSummaryGridModelViews);

					response.Data = System.IO.File.ReadAllBytes(tempFileLocation);
				}
				else
				{
					// initialize memory stream to 1 MB to start
					using (MemoryStream ms = new(1000000))
					{
						tempFileLocation = Path.GetTempFileName();
						fs = new(tempFileLocation, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

						this.boeExportService.ExportBoeToWord(ms, requestModel.SelectedComponents, requestModel.IsCustomExport, requestModel.ExportFormatDTO,
							exportInputs, requestModel.BoeExportModelViews, requestModel.BoeSummaryGridModelViews);

						ms.Seek(0, SeekOrigin.Begin);
						response.Data = ms.ToArray();
					}
				}

				response.IsSuccessful = true;
			}
			catch (Exception e)
			{
				this.log.LogError(e, "Unknown Exception.");

				string supportLink = CommonUtilities.ServiceCentralLink();

				response.Messages.Add($"An error has occurred.  This might be the result of invalid data.  If the data is valid, and the error persists, please create a ticket with IES Helpdesk at {supportLink}.");
			}
			finally
			{
				if (!string.IsNullOrWhiteSpace(tempFileLocation) && System.IO.File.Exists(tempFileLocation))
				{
					try
					{
						System.IO.File.Delete(tempFileLocation);
					}
					catch
					{
						// if it errored out, that is ok
					}
				}
			}

			return response;
		}

		/// <summary>
		/// Exports BOE to Excel
		/// </summary>
		/// <param name="requestModel"></param>
		/// <returns></returns>
		[HttpPost("[action]")]
		public async Task<IActionResult> ExportBoeToExcel(BOEExcelExportInputs requestModel)
		{
			string tempFileLocation = string.Empty;
			string exportedFileName = string.Empty;
			try
			{
				FileStream fs;
				tempFileLocation = this.boeExportService.ExportBoeToExcel(requestModel);

				fs = new(tempFileLocation, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
				exportedFileName = string.Format("genBOEExcelExport-{0}.zip", requestModel.WorkspaceName).Replace(",", string.Empty);
				

				// Generate a custom ActionResult to cause a file download to the client
				fs.Seek(0, SeekOrigin.Begin);

				return this.File(
					fileStream: fs,
					contentType: ExportFileDownloadBase.ContentType_XLSX,
					fileDownloadName: exportedFileName);
			}
			catch (Exception e)
			{
				this.log.LogError(e, "Unknown Exception.");
				return await this.CreateTextFileWithErrorMessage(e.Message);
			}
		}

		[HttpGet("[action]")]
		public IEnumerable<WeatherForecast> GetWeatherForecast()
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
