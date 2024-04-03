// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.OfficeUtilities
{
	using System;
	using System.IO;
	using System.Linq;
	using DocumentFormat.OpenXml.Packaging;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Models;

	public class PackageUtilities
	{
		/// <summary>
		/// Determines the parent template id that was 
		/// placed into the version package property
		/// </summary>
		/// <returns>parent template ID</returns>
		public int? DetermineParentTemplateId(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			int? parentTemplateId = null;
			string versionString;
			lock (CacheConstants.OPEN_XML_LOCK)
			{
				using (WordprocessingDocument document =
					WordprocessingDocument.Open(stream, false))
				{
					versionString = document.PackageProperties.Version;
				}
			}
			if (!string.IsNullOrEmpty(versionString))
			{
				bool parsed = Int32.TryParse(versionString, out int tempId);
				if (parsed)
				{
					parentTemplateId = tempId;
				}
			}

			return parentTemplateId;
		}

		/// <summary>
		/// Updates the package's version to be the parentTemplateID
		/// of the word template.
		/// </summary>
		/// <param name="fileData"></param>
		/// <param name="physicalFilePathCache"></param>
		/// <param name="exportFormat"></param>
		/// <returns>updated stream</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public MemoryStream UpdateDocumentVersion(byte[] fileData, string physicalFilePathCache, ExcelReportTemplate exportFormat)
		{
			MemoryStream mem = new();
			lock (CacheConstants.OPEN_XML_LOCK)
			{
				byte[] docBytes;

				// if the file is serialized to the DB (.FileData property), then use THAT; otherwise use the physical file
				if (fileData != null && fileData.Any())
				{
					docBytes = fileData;
				}
				else if (!string.IsNullOrEmpty(physicalFilePathCache))
				{
					docBytes = System.IO.File.ReadAllBytes(physicalFilePathCache);
				}
				else
				{
					docBytes = Array.Empty<byte>();
				}

				mem.Write(docBytes, 0, docBytes.Length);

				if (exportFormat != null)
				{
					int parentTemplateId = exportFormat.ParentTemplateId ?? exportFormat.TemplateId;

					using (WordprocessingDocument document =
						WordprocessingDocument.Open(mem, true))
					{
						document.PackageProperties.Version = parentTemplateId.ToString();
					}
				}
			}
			return mem;
		}
	}
}
