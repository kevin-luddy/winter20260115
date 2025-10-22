// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.Controllers.Backend
{
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Objects;
	using GenBOE.Web.Common;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.OfficeUtilities;
	using Microsoft.Ajax.Utilities;
	using System;
	using System.Collections.Generic;
	using System.Data.Entity.Core;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Http;
	using System.Web.UI.WebControls;

	public class ImportController : BoeDataBaseAPIController
	{
		#region Properties & Ctor

		/// <summary>
		/// Logger
		/// </summary>
		private readonly Logger logger = new Logger("ImportController");

		/// <summary>
		/// Service for PermissionsController
		/// </summary>
		private PermissionControllerLogic PermissionControllerLogic { get; set; }

		/// <summary>
		/// Constructor for the Import Controller
		/// </summary>
		/// <param name="securityAccess">Security Access</param>
		/// <param name="factory">Full object factory</param>
		/// <param name="userLoader">User loader</param>
		/// <param name="permissionsLoader">Permission loader</param>
		/// <param name="PermissionControllerLogic">Permissions Controller Logic</param>
		public ImportController(ISecurityAccess securityAccess, IFullObjectFactory factory, IUserDTODataLoader userLoader, IPermissionsDTODataLoader permissionsLoader,
			PermissionControllerLogic PermissionControllerLogic
			) : base(securityAccess, factory, userLoader, permissionsLoader)
		{
			this.PermissionControllerLogic = PermissionControllerLogic;
		}

		#endregion Properties & Ctor

		/// <summary>
		/// Import File
		/// </summary>
		/// <returns>True/False to indicate if operation is successful</returns>
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public async Task<IESSingleResponse<bool>> ImportPermissionsAsync()
		{
			// Check if the request contains multipart/form-data.
			if (!Request.Content.IsMimeMultipartContent())
			{
				throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
			}

			// Variables for Import Process and Return
			IESSingleResponse<bool> result = new IESSingleResponse<bool>();
			ICollection<SavePermissionModelView> permissions = new List<SavePermissionModelView>();

			// Variables for Processing Request Data
			ImportViewModel importViewModel = new ImportViewModel();
			Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
			ICollection<string> filePaths = new List<string>();

			// Perform Action
			string errorMessage = string.Empty;
			try
			{
				string root = HttpContext.Current.Server.MapPath("~/App_Data");
				MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(root);
				await Request.Content.ReadAsMultipartAsync(provider);

				// Provider has the content with the files being part of FileData
				// We parse out FileData and Content individually to reduce complexity
				// Parse out the HTTP Content that is the object: ImportViewModel
				Utilities.GetModelValuesFromContent(provider.Contents.Where(x => x.Headers.ContentDisposition.Name != "\"files\"").ToList(), keyValuePairs);

				// Now that the dictionary has been populated, lets populate our model
				Utilities.PopulateModel<ImportViewModel>(importViewModel, keyValuePairs);

				// Parse out the File data
				foreach (MultipartFileData file in provider.FileData)
				{
					// Set the filePath here because we will dispose of it in the Finally block
					filePaths.Add(file.LocalFileName);

					Stream stream = new FileStream(file.LocalFileName, FileMode.Open);
					permissions.AddRange(PermissionsImporter.ImportFromExcelFile(stream));
					stream.Dispose();
				}

				if (permissions.Count > 0)
				{
					this.PermissionControllerLogic.SaveNewPermissions(importViewModel.Workspace, permissions);
					errorMessage = string.Empty;
				}
				else
				{
					// Return a success message
					errorMessage = "No Permissions Were Imported.";
				}
			}
			// Catch custom exceptions from ExcelImporter and ResourcesImporter and generate friendly
			// exception messages to display for the user
			catch (NotExcelFileException)
			{
				errorMessage = "File is an invalid format. File must be in a MS Excel format (.xlsx or .xls).";
			}
			catch (ColumnMissingException ex2)
			{
				errorMessage = string.Format("File does not contain all of the required columns. File must contain 'NtId', 'Role' columns. The following columns are missing: {0}.", ex2.Message);
			}
			catch (CellValueMissingException ex3)
			{
				errorMessage = string.Format("A row in the content does not contain a value for NtId and Role. Every filled row must have a value for each. Check the following column: {0}.", ex3.Message);
			}
			catch (DuplicateValuesException ex4)
			{
				errorMessage = string.Format("Values must be unique. The following are not unique: {0}", ex4.Message);
			}
			catch (EntityCommandExecutionException)
			{
				errorMessage = "The Permissions were recently updated by another user. Please refresh the page to review these latest changes. Once the page is refreshed, you can try your import operation again.";
			}
			catch (GenValidationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				result.Messages.Add($"Unknown error occurred importing Permissions: {ex.Message}");
			}
			finally
			{
				if (filePaths.Any())
				{
					filePaths.ForEach(path =>
					{
						if (File.Exists(path))
						{
							File.Delete(path);
						}
					});
				}
			}

			if (errorMessage.Length > 0)
			{
				result.Messages.Add(errorMessage);
			}

			return result;
		}
	}
}