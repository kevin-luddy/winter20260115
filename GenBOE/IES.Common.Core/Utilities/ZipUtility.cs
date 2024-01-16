// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Packaging;
	using System.Net.Mime;

	/// <summary>
	/// Generates Zip archives.
	/// </summary>
	public static class ZipUtility
	{
		/// <summary>
		/// Zips a collection of streams into a physical zip file.
		/// </summary>
		/// <param name="contents">The streams and associated file names to zip up</param>
		/// <param name="folderPath">The folder where the zip file will be created</param>
		/// <returns>Path of the zipped file</returns>
		public static string ZipFiles(Dictionary<string, Stream> contents, string folderPath)
		{
			string toReturn = string.Empty;

			if (contents != null)
			{
				// Create a new unique file name for saving the zipped data
				toReturn = Path.GetDirectoryName(folderPath) + "\\" + Path.GetRandomFileName() + ".zip";

				// Open a new ZipPackage using the unique file name
				using (Package zip = Package.Open(toReturn, FileMode.Create, FileAccess.ReadWrite))
				{
					// Add each file from the given dictionary to the ZipPackage
					foreach (KeyValuePair<string, Stream> file in contents)
					{
						AddToArchive(zip, file.Key, file.Value);
					}
				}
			}

			// Return the file name of the ZipPackage
			return toReturn;
		}

		/// <summary>
		/// Adds a stream of data into a zip file using the specified file name
		/// </summary>
		/// <param name="zip">The ZipPackage to add the stream of data to</param>
		/// <param name="fileName">The file name that the stream of data will use in the package</param>
		/// <param name="fileData">The stream of data to add</param>
		private static void AddToArchive(Package zip, string fileName, Stream fileData)
		{
			// Add a trailing forward slash to the URI for the file
			string zipUri = string.Concat("/", Path.GetFileName(fileName));
			Uri partUri = new Uri(zipUri, UriKind.Relative);

			// Set the content type to zipped data
			string contentType = MediaTypeNames.Application.Zip;

			// Create a new package part to represent the stream of data  
			PackagePart pkgPart = zip.CreatePart(partUri, contentType, CompressionOption.Normal);

			// Read all of the bytes from the stream to add to the zip file 
			byte[] bites = new byte[fileData.Length];
			fileData.Read(bites, 0, (int)fileData.Length);

			// Compress and write the bytes to the package part in the zip file 
			pkgPart.GetStream().Write(bites, 0, bites.Length);
		}
	}
}
