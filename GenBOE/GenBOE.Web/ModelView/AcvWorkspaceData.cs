// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	/// <summary>
	/// ACV Workspace Data Class (data returned from BOE)
	/// 
	/// THIS MUST MATCH THE BOE SERVICE, DO NOT CHANGE
	/// </summary>
	public class AcvWorkspaceData
	{
		/// <summary>
		/// WS Id
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// WS Short Name
		/// </summary>
		public string ShortName { get; set; }

		/// <summary>
		/// WS Long Name
		/// </summary>
		public string LongName { get; set; }

		/// <summary>
		/// PTM Tracking Number
		/// </summary>
		public string PtmTrackingNumber { get; set; }

		/// <summary>
		/// Is the Workspace Current for the PTM Tracking Number
		/// </summary>
		public bool IsCurrentWorkspace { get; set; }
	}
}