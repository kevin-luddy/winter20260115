namespace APTSPropricerApi.Common
{
	using EBS.ProPricer.Reports.Export;
	using EBS.ProPricer.Model;
	using EBS.ProPricer.Reports;
	using System.IO;
	using DevExpress.XtraPrinting;
	using EBS.ProPricer.Reports.Engine.Runtime;
	using EBS.ProPricer.Reports.Engine.Processing;

	/// <summary>
	/// Utility Class for Common Methods used in multiple Controllers
	/// </summary>
	public static class ProPricerUtility
	{
		/// <summary>
		/// Generate TempFile for Export to utilize
		/// </summary>
		/// <param name="batchReport">Batch Report</param>
		/// <param name="proposal">Proposal</param>
		/// <param name="exportType">Export Type</param>
		/// <returns>Temp File</returns>
		public static string GenerateTempFile(BatchReport batchReport, Proposal proposal, ExportType exportType)
		{
			batchReport.Open();

			string tempFile = Path.GetRandomFileName();

			if (exportType == ExportType.Pdf)
			{
				//ReportInfo reportInfo = new ReportInfo((RegularReport)batchReport, proposal);
				//ReportRuntimeContext ctx = new ReportRuntimeContext(reportInfo, proposal, ReportExtender.Instance);

				//ctx.OutputOptions.Destination = ReportDestination.File;
				//ctx.OutputOptions.ExportDestinationType = exportType;
				//ctx.OutputOptions.ExportPath = Directory.GetParent(tempFile).FullName;
				//ctx.OutputOptions.ExportFileName = Path.GetFileName(tempFile);
				//ctx.OutputOptions.OpenExportedDocument = false;

				//ReportGenerator generator = new ReportGenerator(ctx);
				//generator.ProcessReport();

				BatchReportContextManager mgr = new BatchReportContextManager(proposal);
				BatchReportRuntimeContext ctx = new BatchReportRuntimeContext(batchReport, mgr);

				ctx.Options.ExportType = exportType;
				ctx.Options.Folder = Constants.TEMP_DIRECTORY;
				ctx.Options.FileName = Path.GetFileNameWithoutExtension(tempFile);
				ctx.Options.Destination = ReportDestination.File;
				ctx.Options.OutputMode = OutputMode.Combined;
				ctx.ProcessAll = true;

				BatchReportGenerator generator = new BatchReportGenerator(ctx);
				ctx.Generator = generator;
				generator.Process();

				ctx.RuntimeReports.ForEach(x =>
				{
					x.OutputOptions.Destination = ReportDestination.File;
					x.OutputOptions.ExportDestinationType = exportType;
					x.OutputOptions.ExportPath = Directory.GetParent(tempFile).FullName;
					x.OutputOptions.ExportFileName = Path.GetFileName(tempFile);
					x.OutputOptions.OpenExportedDocument = false;

					ReportGenerator generator2 = new ReportGenerator(x);
					generator2.ProcessReport();
				});

				tempFile = Path.Combine(ctx.Options.Folder, ctx.Options.FileName + GetExportExtension(ctx.Options.ExportType));
			}
			else
			{
				BatchReportContextManager mgr = new BatchReportContextManager(proposal);
				BatchReportRuntimeContext ctx = new BatchReportRuntimeContext(batchReport, mgr);

				ctx.Options.ExportType = exportType;
				ctx.Options.Folder = Constants.TEMP_DIRECTORY;
				ctx.Options.FileName = Path.GetFileNameWithoutExtension(tempFile);
				ctx.Options.Destination = ReportDestination.File;
				ctx.Options.OutputMode = OutputMode.Combined;
				ctx.ProcessAll = true;

				BatchReportGenerator generator = new BatchReportGenerator(ctx);
				ctx.Generator = generator;
				generator.Process();

				tempFile = Path.Combine(ctx.Options.Folder, ctx.Options.FileName + GetExportExtension(ctx.Options.ExportType));
			}

			

			return tempFile;
		}

		/// <summary>
		/// Get File extension for Report (Default is Excel => .xlsx)
		/// </summary>
		/// <param name="exportType">Enum of File type</param>
		/// <returns>Complete extension of File (i.e .xlsx)</returns>
		public static string GetExportExtension(ExportType exportType)
		{
			// Defaulting to Excel
			string extension = ".xlsx";

			if (exportType == ExportType.Pdf)
			{
				extension = ".pdf";
			}

			if (exportType == ExportType.Word)
			{
				extension = ".docx";
			}

			return extension;
		}
	}
}