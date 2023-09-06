// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	/// <summary>
	/// BOE Form Data inside a Workspace
	/// </summary>
	public class BOEFormData
	{
		/// <summary>
		/// Vendor ID
		/// </summary>
		public string VendorID { get; set; }

		/// <summary>
		/// PBOE Database Id
		/// </summary>
		public int PBOEId { get; set; }

		/// <summary>
		/// The BOE Form's Name
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// The Total Cost for a BOE Form instance
		/// </summary>
		public decimal TotalCost { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is incomplete.
		/// </summary>
		public bool IsIncomplete { get; set; }
	}
}