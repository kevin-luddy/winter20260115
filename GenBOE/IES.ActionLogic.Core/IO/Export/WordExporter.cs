// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Core.IO.Export
{
	using System;
	using System.IO;
	using System.Linq;
	using Aspose.Words;
	using Aspose.Words.Markup;
	using Aspose.Words.Saving;
	using Aspose.Words.Tables;
	using GenBOE.DataBridge.Core.IO.Export;
	using IES.ActionLogic.Core.Common;
	using IES.Common.Core.Configuration;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.OfficeUtilities;
	using IES.Common.Core.Services;
	using IES.Common.Core.Utilities;
	using Microsoft.Extensions.Logging;
	using Newtonsoft.Json;

	/// <summary>
	/// Word Exporter class containing export methods and other utilities
	/// </summary>
	public class WordExporter
	{
		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="WordExporter" /> class.
		/// </summary>
		/// <param name="logger">The logger</param>
		public WordExporter(ILogger<WordExporter> logger)
		{
			this.logger = logger;
		}

		/// <summary>
		/// Run the export: Read the Word template file into an in-memory OpenXml Wordprocessing document, call the designated data
		/// population method, then return the results as a byte stream.
		/// </summary>
		/// <param name="templateFilePathFull">Full path to the Word template file</param>
		/// <param name="populateData">Method to populate data</param>
		/// <param name="stream">Stream into which to write the exported Word document.</param>
		/// <param name="portionMarkingRequired">Is Portion Marking Required</param>
		/// <param name="tokenService">The token service</param>
		protected async Task Export(string templateFilePathFull, Action<Document> populateData, Stream stream, bool? portionMarkingRequired, ITokenService tokenService)
		{
			// open a copy of the Excel template file into memory
			byte[] byteArray = File.ReadAllBytes(templateFilePathFull);

			await Export(byteArray, populateData, stream, portionMarkingRequired, tokenService);
		}

		/// <summary>
		/// Run the export: Read the Word template file into an in-memory OpenXml Wordprocessing document, call the designated data
		/// population method, then return the results as a byte stream.
		/// </summary>
		/// <param name="byteArray">Contents of the Word template file</param>
		/// <param name="populateData">Method to populate data</param>
		/// <param name="stream">Stream into which to write the exported Word document.</param>
		/// <param name="portionMarkingRequired">Is Portion Marking Required</param>
		/// <param name="tokenService">The token service</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
		protected async Task Export(byte[] byteArray, Action<Document> populateData, Stream stream, bool? portionMarkingRequired, ITokenService tokenService)
		{
			if (byteArray == null)
			{
				throw new ArgumentNullException(nameof(byteArray));
			}

			if (populateData == null)
			{
				throw new ArgumentNullException(nameof(populateData));
			}

			bool doPortionMarking = portionMarkingRequired.HasValue && portionMarkingRequired.Value;
			string tempFilename = Path.GetTempFileName();
			new FileInfo(tempFilename).Attributes |= FileAttributes.Temporary;
			using (Stream documentStream = new FileStream(tempFilename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose))
			{
				documentStream.Write(byteArray, 0, byteArray.Length);

				// Create the document object in memory
				Document document = new Document(documentStream);
					
				// Call the worker method to load-in the data
				populateData(document);

				if (!doPortionMarking)
				{
					// write the document from the file into the caller's stream
					document.Save(stream, SaveFormat.Docx);
				}

				if (doPortionMarking)
				{
					ByteArrayContent content;
					byte[] tempBytes;
					using (MemoryStream memoryStream = new MemoryStream())
					{
						document.Save(memoryStream, SaveFormat.Docx);
						tempBytes = memoryStream.ToArray();
						content = new ByteArrayContent(tempBytes);
					}

					content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ExportFileDownloadBase.ContentType_DOCX);
					HttpClient httpClient = new HttpClient();
					CommonUtilities.AddAuthorizationHeader(httpClient, (await tokenService.GetToken()).AccessToken);
					string portionMarkingAPI = ConfigurationUtilities.GetAppSetting("PortionMarkingAPI");
					Task<HttpResponseMessage> syncAPICall = Task.Run(() => httpClient.PostAsync(portionMarkingAPI + "/api/PortionMarking/PortionMarkDocument", content));
					try
					{
						syncAPICall.Wait();
						HttpResponseMessage response = syncAPICall.Result;

						if (!response.IsSuccessStatusCode)
						{
							throw new Exception("Error calling Portion Marking with status Code: " + response.StatusCode.ToString());
						}

						// Deserialize the entire response because we're getting more than just the byte[] back
						string allBytes = await response.Content.ReadAsStringAsync();
						Result<byte[]> deserializedResult = JsonConvert.DeserializeObject<Result<byte[]>>(allBytes);

						if (deserializedResult.Messages.Any())
						{
							HelperCreateErrorDocument(deserializedResult, stream);
						}
						else
						{
							MemoryStream memStream = new MemoryStream(deserializedResult.Data);
							memStream.Seek(0, SeekOrigin.Begin);
							memStream.CopyTo(stream);
						}
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "POST to PortionMarkingAPI failed.");
						Result<byte[]> tempResult = new Result<byte[]>();
						tempResult.Messages.Add("The requested action could not be completed. If the problem persists, please contact your application administrator.");
						HelperCreateErrorDocument(tempResult, stream);
					}
				}
			}
		}

		/// <summary>
        /// Helper for creating a document to return an error message to the user. Returns the result as a byte stream.
        /// </summary>
        /// <param name="deserializedResult">Failed http response</param>
        /// <param name="stream">Stream into which to write the exported error Word document.</param>
		private void HelperCreateErrorDocument(Result<byte[]> deserializedResult, Stream stream)
		{
			string tempErrorFilename = Path.GetTempFileName();
			Document errorDocument = new Document();
				
			Body body = errorDocument.AppendChild(new Body(errorDocument));
			foreach (string message in deserializedResult.Messages)
			{
				Paragraph para = body.AppendChild(new Paragraph(errorDocument));
				Run run = para.AppendChild(new Run(errorDocument));
				run.Text = message;
			}

			errorDocument.Save(stream, SaveFormat.Docx);
		}

		/// <summary>
		/// Removes element
		/// </summary>
		/// <param name="element">Element to remove</param>
		public void RemoveIt(Node element)
		{
			if (element != null && element.ParentNode != null)
			{
				element.Remove();
			}
		}

		/// <summary>
		/// Do not allow the contents of the row to be split between pages.
		/// </summary>
		/// <param name="row">The row</param>
		protected void SetCannotSplit(Row row)
		{
			if (row == null)
			{
				throw new ArgumentNullException(nameof(row));
			}

			row.RowFormat.AllowBreakAcrossPages = false;
		}

		/// <summary>
		/// Clone a table row
		/// </summary>
		/// <param name="markedRow">Table row to clone</param>
		/// <returns>Cloned copy of the table row</returns>
		protected Row CloneMarkedTemplateRow(Row markedRow)
		{
			if (markedRow == null)
			{
				throw new ArgumentNullException(nameof(markedRow));
			}

			Row clonedRow = markedRow.Clone(true) as Row;

			#region Delete IDs to avoid conflict with existing elements

			foreach (StructuredDocumentTag sdt in clonedRow.GetChildNodes(NodeType.StructuredDocumentTag, true))
			{
				sdt.Placeholder?.RemoveIt();
			}

			#endregion

			return clonedRow;
		}

		/// <summary>
		/// Remove an element from the document (DOM)
		/// </summary>
		/// <param name="element">Element to remove</param>
		protected void RemoveElement(Node element)
		{
			if (element == null)
			{
				return;
			}

			Node parentElement = element.ParentNode;

			if (parentElement != null)
			{
				element.Remove();

				// enforce well-formedness of table cell XML (i.e. must contain a paragraph)
				if (parentElement is Cell cell && !cell.GetChildNodes(NodeType.Paragraph, true).Any())
				{
					if (parentElement.ParentNode is Row row && row.Cells.Count == 1)
					{
						row.Remove();  // if this is the only cell, then (because it is empty) just remove the entire row
					}
					else
					{
						cell.AppendChild(new Paragraph(parentElement.Document));
					}
				}
			}
		}
	}
}
