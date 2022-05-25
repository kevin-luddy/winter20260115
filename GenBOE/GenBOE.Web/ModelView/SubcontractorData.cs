// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
	/// <summary>
	/// Subcontractor Data inside a Workspace
	/// </summary>
	public class SubcontractorData
	{
		/// <summary>
		/// The Subcontractor Name
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// The Total Cost for a Subcontractor
		/// </summary>
		public decimal TotalCost { get; set; }
	}
}