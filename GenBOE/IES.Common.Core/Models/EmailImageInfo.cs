// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Models
{
	/// <summary>
	/// Image properties used to incorporate image content into an email message
	/// </summary>
	public class EmailImageInfo
	{
		/// <summary>
		/// Unique content id (cid) for the image in the email message
		/// </summary>
		public string EmailContentId { get; set; }

		/// <summary>
		/// Base-64 representation of the image
		/// </summary>
		public string Base64String { get; set; }

		/// <summary>
		/// MIME type of the image (e.g. "image/jpeg")
		/// </summary>
		public string MimeType { get; set; }

		/// <summary>
		/// Height of the image (optional)
		/// </summary>
		public string Height { get; set; }

		/// <summary>
		/// Width of the image (optional)
		/// </summary>
		public string Width { get; set; }
	}
}
