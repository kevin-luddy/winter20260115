namespace GenBOE.ActionLogic.IO.Export.BOE
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web;
	using GenBOE.Dtos;
	using IES.Common;
	using IES.Common.Exceptions;

	public class BOEHttpService : BaseHttpService
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public BOEHttpService() : base("ReportsBackendServiceUrl", "Reports", new Logger(typeof(BOEHttpService)))
		{
		}

		public async Task ExportBOEsToWord(ICollection<BoeCustomReportComponent> selectedComponents, HttpResponseBase httpResponse, bool isCustomExport,
			WorkspaceExportFormatDTO wsExportFormatDTO, BOEExportInputs exportInputs, ICollection<BOEExportModelView> boeExportModelViews, 
			List<BOESummaryGridModelView> boeSummaryGridModelViews, bool segmentedOutput)
		{
			ExportBoeWordRequestViewModel model = new ExportBoeWordRequestViewModel(exportInputs)
			{
				SelectedComponents = selectedComponents.ToList(),
				IsCustomExport = isCustomExport,
				ExportFormatDTO = wsExportFormatDTO,
				BoeExportModelViews = boeExportModelViews.ToList(),
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

			IESSingleResponse<Stream> returnStream = await this.PostReturnStream<ExportBoeWordRequestViewModel>("ExportBoeToWord", model);

			if (returnStream.IsSuccessful)
			{

				httpResponse.ContentType = segmentedOutput ? BOEExporter.CONTENT_TYPE_ZIP : BOEExporter.CONTENT_TYPE_DOCX;
				httpResponse.Clear();
				httpResponse.BufferOutput = true;
				httpResponse.AppendHeader("Content-Disposition", $"attachment;filename={returnFilename}");

				await returnStream.Data.CopyToAsync(httpResponse.OutputStream);
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
