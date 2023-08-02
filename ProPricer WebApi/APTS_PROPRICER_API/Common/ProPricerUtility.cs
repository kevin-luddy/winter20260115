namespace APTSPropricerApi.Common
{
	using EBS.ProPricer.Model;
	using EBS.ProPricer.Reports;
	using EBS.ProPricer.Reports.Export;
	using System.IO;

	/// <summary>
	/// Utility Class for Common Methods used in multiple Controllers
	/// </summary>
	public static class ProPricerUtility
	{

		/// <summary>
		/// Takes BatchReport and Proposal and Generates a file given the Report Type
		/// </summary>
		/// <param name="proposal">Proposal</param>
		/// <param name="batchReport">Batch Report</param>
		/// <param name="exportType">Type of Export</param>
		/// <returns>File for given Report Type</returns>
		public static string GenerateFile(Proposal proposal, BatchReport batchReport, ExportType exportType, string extensionType)
		{
			string tempFile = Path.GetRandomFileName();

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

			tempFile = Path.Combine(ctx.Options.Folder, ctx.Options.FileName + extensionType);

			return tempFile;
		}
	}
}