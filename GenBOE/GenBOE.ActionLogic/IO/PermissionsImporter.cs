// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Import
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using DocumentFormat.OpenXml.Packaging;
	using GenBOE.ActionLogic.ModelView;
	using IES.Common;
	using IES.Common.OfficeUtilities;

	/// <summary>
	/// Used for importing new workspace permissions from an Excel file
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static class PermissionsImporter
	{
		// Individual column names
		private const string ntIdColumn = "NtId";
		private const string roleColumn = "Role";

		// Array of the columns that must be contained in the imported file
		private static readonly string[] requiredColumns = new string[] { ntIdColumn, roleColumn };

		// Array of the columns in the imported file that must contain values
		private static readonly string[] requiredValueColumns = new string[] { ntIdColumn, roleColumn };

		/// <summary>
		/// Returns a collection of Resource DTO objects that can be used to submit newly imported
		/// Resource elements into the genBOE DB.
		/// </summary>
		/// <param name="excelFileStream">A file stream holding the uploaded data from the user</param>
		/// <param name="listID">The ID of the list to associate these performing orgs with</param>
		/// <returns>Collection of new Resource DTO objects holding all of the new WBS elements.</returns>
		public static ICollection<SavePermissionModelView> ImportFromExcelFile(Stream excelFileStream)
		{
			try
			{
				// Open the document as read-only.
				using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
				{
					// Get a collection of all rows in the file, filtering out rows that only have data in
					// non import-related columns. Each row is represented as a Key/Value pair Dictionary object
					// in an enumerable collection
					ICollection<Dictionary<string, string>> allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, string.Empty, requiredColumns, requiredColumns, requiredValueColumns, null);

					// Turn each row into a DTO object and return the collection
					return CreateDTOsToReturn(allRows);
				}
			}
			catch (FileFormatException)
			{
				throw new NotExcelFileException();
			}
		}

		private static ICollection<SavePermissionModelView> CreateDTOsToReturn(ICollection<Dictionary<string, string>> allRows)
		{
			Dictionary<string, SavePermissionModelView> permissions = new Dictionary<string, SavePermissionModelView>();

			// For each Dictionary object (representing imported row data)
			foreach (Dictionary<string, string> row in allRows)
			{
				string ntId = row[ntIdColumn];
				string role = row[roleColumn];

				Role roleEnum = (Role)Enum.Parse(typeof(Role), role);

				SavePermissionModelView savePermissionModelView;
				if (!permissions.TryGetValue(ntId, out savePermissionModelView))
				{
					savePermissionModelView = new SavePermissionModelView();
					permissions.Add(ntId, savePermissionModelView);
					savePermissionModelView.EntityIds.Add(ntId);
				}

				savePermissionModelView.Roles.Add(roleEnum);
			}

			return permissions.Values;
		}
	}
}
