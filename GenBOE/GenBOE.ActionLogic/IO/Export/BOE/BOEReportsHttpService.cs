// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.ActionLogic.IO.Export.BOE
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
	using DocumentFormat.OpenXml.EMMA;
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.Exceptions;

	public class BOEReportsHttpService : BaseHttpService
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public BOEReportsHttpService() : base("ReportsBackendServiceUrl", "Reports", new Logger(typeof(BOEReportsHttpService)))
		{
		}

		/// <summary>
		/// Export BOEs to Word
		/// </summary>
		/// <param name="selectedComponents">optional selected components</param>
		/// <param name="httpResponse">HttpResponse to add response to</param>
		/// <param name="isCustomExport">Whether this is a custom export or not</param>
		/// <param name="wsExportFormatDTO">The workspace export format</param>
		/// <param name="exportInputs">The export input params</param>
		/// <param name="boeExportModelViews">The Export ModelViews for BOE</param>
		/// <param name="boeSummaryGridModelViews">The summary grid modelviews for BOE</param>
		/// <param name="segmentedOutput">Whether this is a segmented output (different files zipped) or not</param>
		/// <returns>Async Task used for await</returns>
		/// <exception cref="GenValidationException"></exception>
		public async Task ExportBOEsToWord(ICollection<BoeCustomReportComponent> selectedComponents, HttpResponseBase httpResponse, bool isCustomExport,
			WorkspaceExportFormatDTO wsExportFormatDTO, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, 
			List<BOESummaryGridModelView> boeSummaryGridModelViews, bool segmentedOutput)
		{
			ExportBoeWordRequestViewModel model = new ExportBoeWordRequestViewModel(exportInputs)
			{
				SelectedComponents = selectedComponents?.ToList(),
				IsCustomExport = isCustomExport,
				ExportFormatDTO = wsExportFormatDTO,
				BoeExportModelViews = boeExportModelViews?.ToList(),
				BoeSummaryGridModelViews = boeSummaryGridModelViews,
				SegmentedOutput = segmentedOutput
			};

			string returnFilename = string.Format("genBOEExport-{0}.zip", exportInputs.Workspace.WorkspaceName).Replace(",", string.Empty);

			if (!segmentedOutput)
			{
				if (isCustomExport)
				{
					returnFilename = string.Format("genBOECustomExport-{0}.docx", exportInputs.Workspace.WorkspaceName);
				}
				else
				{
					returnFilename = string.Format("genBOEExport-{0}.docx", exportInputs.Workspace.WorkspaceName).Replace(",", string.Empty);
				}
			}

			IESSingleResponse<byte[]> returnStream = await this.Post<byte[], ExportBoeWordRequestViewModel>("ExportBoeToWord", model);

			if (returnStream.IsSuccessful)
			{
				returnFilename = Utilities.StripIllegalFileNameCharacters(returnFilename);
				httpResponse.ContentType = segmentedOutput ? BOEExporter.CONTENT_TYPE_ZIP : BOEExporterConstants.ContentType_DOCX;
				httpResponse.Clear();
				httpResponse.BufferOutput = true;
				httpResponse.AppendHeader(BOEExporterConstants.CONTENT_HEADER_NAME, string.Format(BOEExporterConstants.CONTENT_HEADER_FORMAT_STRING, returnFilename));

				await httpResponse.OutputStream.WriteAsync(returnStream.Data, 0, returnStream.Data.Length);
			}
			else
			{
				string supportLink = Utilities.ServiceCentralLink();
				string message = string.Format("An error has occurred.  This might be the result of invalid data.  Try running the 'Validate All BOEs' report, and correct any errors it may find.  If the data is valid, and the error persists, please create a ticket with Helpdesk at {0}.", supportLink);

				throw new GenValidationException(message, string.Join(Environment.NewLine, returnStream.Messages));
			}
		}

		/// <summary>
		/// Export BOEs to Word
		/// </summary>
		/// <param name="selectedComponents">optional selected components</param>
		/// <param name="stream">The stream to export into</param>
		/// <param name="isCustomExport">Whether this is a custom export or not</param>
		/// <param name="wsExportFormatDTO">The workspace export format</param>
		/// <param name="exportInputs">The export input params</param>
		/// <param name="boeExportModelViews">The Export ModelViews for BOE</param>
		/// <param name="boeSummaryGridModelViews">The summary grid modelviews for BOE</param>
		/// <param name="segmentedOutput">Whether this is a segmented output (different files zipped) or not</param>
		/// <returns>Async Task used for await</returns>
		/// <exception cref="GenValidationException"></exception>
		public async Task ExportBOEsToWordStream(ICollection<BoeCustomReportComponent> selectedComponents, Stream stream, bool isCustomExport,
			WorkspaceExportFormatDTO wsExportFormatDTO, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews,
			List<BOESummaryGridModelView> boeSummaryGridModelViews, bool segmentedOutput)
		{
			ExportBoeWordRequestViewModel model = new ExportBoeWordRequestViewModel(exportInputs)
			{
				SelectedComponents = selectedComponents?.ToList(),
				IsCustomExport = isCustomExport,
				ExportFormatDTO = wsExportFormatDTO,
				BoeExportModelViews = boeExportModelViews?.ToList(),
				BoeSummaryGridModelViews = boeSummaryGridModelViews,
				SegmentedOutput = segmentedOutput
			};

			IESSingleResponse<byte[]> returnStream = await this.Post<byte[], ExportBoeWordRequestViewModel>("ExportBoeToWord", model);

			if (returnStream.IsSuccessful)
			{
				await stream.WriteAsync(returnStream.Data, 0, returnStream.Data.Length);
			}
			else
			{
				string supportLink = Utilities.ServiceCentralLink();
				string message = string.Format("An error has occurred.  This might be the result of invalid data.  Try running the 'Validate All BOEs' report, and correct any errors it may find.  If the data is valid, and the error persists, please create a ticket with Helpdesk at {0}.", supportLink);

				throw new GenValidationException(message, string.Join(Environment.NewLine, returnStream.Messages));
			}
		}

		/// <summary>
		/// Exports the Manage BOEs information to Excel.
		/// </summary>
		/// <param name="exportInputs">BOE exportInputs.</param>
		/// <returns>Export File location</returns>
		public async Task<string> ExportManageBoesToExcel(BOEExcelExportInputs exportInputs)
		{
			IESSingleResponse<byte[]> returnStream = await this.Post<byte[], BOEExcelExportInputs>("ExportManageBoesToExcel", exportInputs);

			if (returnStream.IsSuccessful)
			{
				// Save the return byte[] to a file
				string tempFilename = Path.GetTempFileName();
				File.WriteAllBytes(tempFilename, returnStream.Data);
				return tempFilename;
			}
			else
			{
				throw new GenValidationException("Error calling Reports Service", string.Join(Environment.NewLine, returnStream.Messages));
			}
		}
	}
}
