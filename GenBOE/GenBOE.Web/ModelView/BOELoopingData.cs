// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	/// <summary>
	/// Model for looping BOE data sent to IESCommsAPI
	/// </summary>
	public class BOELoopingData
	{
		/// <summary>
		/// The BOE ID
		/// </summary>
		public int BOEId { get; set; }

		/// <summary>
		/// The BOE Name
		/// </summary>
		public string BOEName { get; set; }

		/// <summary>
		/// The WBS
		/// </summary>
		public string WBS { get; set; }

		/// <summary>
		/// The CLIN
		/// </summary>
		public string CLIN { get; set; }

		/// <summary>
		/// The TotalCost
		/// </summary>
		public decimal TotalCost { get; set; }
	}
}