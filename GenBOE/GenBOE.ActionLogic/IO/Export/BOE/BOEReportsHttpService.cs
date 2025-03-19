namespace GenBOE.ActionLogic.IO.Export.BOE
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
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

				httpResponse.ContentType = segmentedOutput ? BOEExporter.CONTENT_TYPE_ZIP : BOEExporter.CONTENT_TYPE_DOCX;
				httpResponse.Clear();
				httpResponse.BufferOutput = true;
				httpResponse.AppendHeader("Content-Disposition", $"attachment;filename={returnFilename}");

				await httpResponse.OutputStream.WriteAsync(returnStream.Data, 0, returnStream.Data.Length);
			}
			else
			{
				throw new GenValidationException("Error calling Reports Service", string.Join(Environment.NewLine, returnStream.Messages));
			}
		}

		public async Task ExportBOEsToExcel()
		{ }
	}
}
