namespace APTSPropricerApi.Common
{
	using EBS.ProPricer.Reports.Export;

	/// <summary>
	/// Utility Class for Common Methods used in multiple Controllers
	/// </summary>
	public static class ProPricerUtility
	{
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