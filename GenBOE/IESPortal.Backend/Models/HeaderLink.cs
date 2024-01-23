// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IESPortal.Backend.Models
{
	/// <summary>
	/// ViewModel for Header Links
	/// </summary>
	public class HeaderLink
	{
		/// <summary>
		/// Name of the Application
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Url for the Application (null/empty if not yet ready)
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// True if the url should open in a new window/tab
		/// </summary>
		public bool IsNewWindow { get; set; }
	}
}
