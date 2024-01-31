// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
	using System;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using GenBOE.Dtos;
	using IES.Common.OfficeUtilities;

	// <summary>
	/// Responsible for Permissions-specific Excel export.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static class PermissionsExporter
	{
		/// <summary>
		/// Exports default Permissions to an Excel file.
		/// </summary>
		/// <param name="templateFileLocation">The location of the Excel file template</param>
		/// <param name="performingOrgs">The collection of Permissions to export</param>
		/// <returns>Path to the exported Performing Orgs file</returns>
		public static string ExportToExcelFile(string templateFileLocation, Collection<PermissionsDTO> permissions)
		{
			// Check inputs
			if (templateFileLocation == null)
			{
				throw new ArgumentNullException(nameof(templateFileLocation));
			}
			if (permissions == null)
			{
				throw new ArgumentNullException(nameof(permissions));
			}

			string toReturn = string.Empty;

			// Create collections of strings for each row in the export file
			ExcelExportWorksheet worksheet = new ExcelExportWorksheet();

			worksheet.AddRange(from permission in permissions
							   select new Collection<string>
							   {
								   permission.NTID,
								   permission.Role.ToString()
							   });

			// Pass the rows to the generic Excel exporter
			toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);

			// Return the file path
			return toReturn;
		}
	}
}
