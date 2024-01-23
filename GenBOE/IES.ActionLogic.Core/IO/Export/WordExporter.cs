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
	using DocumentFormat.OpenXml;
	using DocumentFormat.OpenXml.Packaging;
	using DocumentFormat.OpenXml.Wordprocessing;
	using IES.Common.Core.Constants;

	/// <summary>
	/// Word Exporter class containing export methods and other utilities
	/// </summary>
	public class WordExporter
	{
		/// <summary>
		/// Run the export: Read the Word template file into an in-memory OpenXml Wordprocessing document, call the designated data
		/// population method, then return the results as a byte stream.
		/// </summary>
		/// <param name="templateFilePathFull">Full path to the Word template file</param>
		/// <param name="populateData">Method to populate data</param>
		/// <param name="stream">Stream into which to write the exported Word document.</param>
		protected void Export(string templateFilePathFull, Action<WordprocessingDocument> populateData, Stream stream)
		{
			// open a copy of the Excel template file into memory
			byte[] byteArray = File.ReadAllBytes(templateFilePathFull);

			Export(byteArray, populateData, stream);
		}

		/// <summary>
		/// Run the export: Read the Word template file into an in-memory OpenXml Wordprocessing document, call the designated data
		/// population method, then return the results as a byte stream.
		/// </summary>
		/// <param name="byteArray">Contents of the Word template file</param>
		/// <param name="populateData">Method to populate data</param>
		/// <param name="stream">Stream into which to write the exported Word document.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This is not an issue with MemoryStream, it allows multiple disposals")]
		protected void Export(byte[] byteArray, Action<WordprocessingDocument> populateData, Stream stream)
		{
			if (byteArray == null)
			{
				throw new ArgumentNullException(nameof(byteArray));
			}

			if (populateData == null)
			{
				throw new ArgumentNullException(nameof(populateData));
			}

			string tempFilename = Path.GetTempFileName();
			new FileInfo(tempFilename).Attributes |= FileAttributes.Temporary;
			using (Stream documentStream = new FileStream(tempFilename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose))
			{
				documentStream.Write(byteArray, 0, byteArray.Length);

				// synchronize write-access to avoid deadlocks in the IsolatedStorageFile class
				lock (CacheConstants.OPEN_XML_LOCK)
				{
					// Create the document object in memory
					using (WordprocessingDocument document = WordprocessingDocument.Open(documentStream, true))
					{
						// Call the worker method to load-in the data
						populateData(document);

						// Save all the changes
						SaveDocument(document);
					}

					// write the document from the file into the caller's stream
					documentStream.Seek(0, SeekOrigin.Begin);
					documentStream.CopyTo(stream);
				}
			}
		}

		/// <summary>
		/// Save the document.
		/// </summary>
		/// <param name="document">The OpenXml Word document object</param>
		protected void SaveDocument(WordprocessingDocument document)
		{
			if (document == null)
			{
				throw new ArgumentNullException(nameof(document));
			}

			document.MainDocumentPart.Document.Save();
		}

		/// <summary>
		/// Removes element
		/// </summary>
		/// <param name="element">Element to remove</param>
		public void RemoveIt(OpenXmlElement element)
		{
			if (element != null && element.Parent != null)
			{
				element.Remove();
			}
		}

		/// <summary>
		/// Do not allow the contents of the row to be split between pages.
		/// </summary>
		/// <param name="row">The row</param>
		protected void SetCannotSplit(TableRow row)
		{
			if (row == null)
			{
				throw new ArgumentNullException(nameof(row));
			}

			TableRowProperties tableRowProperties;
			if ((tableRowProperties = row.TableRowProperties) == null)
			{
				tableRowProperties = new TableRowProperties();
				row.Append(tableRowProperties);
			}

			if (!tableRowProperties.Descendants<CantSplit>().Any())
			{
				CantSplit cantSplit = new();
				tableRowProperties.Append(cantSplit);
			}
		}

		/// <summary>
		/// Clone a table row
		/// </summary>
		/// <param name="markedRow">Table row to clone</param>
		/// <returns>Cloned copy of the table row</returns>
		protected TableRow CloneMarkedTemplateRow(TableRow markedRow)
		{
			if (markedRow == null)
			{
				throw new ArgumentNullException(nameof(markedRow));
			}

			TableRow clonedRow = markedRow.CloneNode(true) as TableRow;

			#region Delete IDs to avoid conflict with existing elements

			foreach (SdtId id in clonedRow.Descendants<SdtId>())
			{
				id.Remove();
			}

			foreach (SdtPlaceholder placeholder in clonedRow.Descendants<SdtPlaceholder>())
			{
				placeholder.Remove();
			}

			#endregion

			return clonedRow;
		}

		/// <summary>
		/// Remove an element from the document (DOM)
		/// </summary>
		/// <param name="element">Element to remove</param>
		protected void RemoveElement(OpenXmlElement element)
		{
			if (element == null)
			{
				return;
			}

			OpenXmlElement parentElement = element.Parent;

			if (parentElement != null)
			{
				element.Remove();

				// enforce well-formedness of table cell XML (i.e. must contain a paragraph)
				if (parentElement is TableCell && !parentElement.Descendants<Paragraph>().Any())
				{
					TableRow row = parentElement.Parent as TableRow;
					if (row != null && row.Descendants<TableCell>().Count() == 1)
					{
						row.Remove();  // if this is the only cell, then (because it is empty) just remove the entire row
					}
					else
					{
						parentElement.AppendChild(new Paragraph());
					}
				}
			}
		}
	}
}
