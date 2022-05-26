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
		/// The BOE Form's Name
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// The Total Cost for a BOE Form instance
		/// </summary>
		public decimal TotalCost { get; set; }
	}
}