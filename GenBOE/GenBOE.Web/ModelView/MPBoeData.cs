// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace GenBOE.Web.ModelView
{
	/// <summary>
	/// Material PBoe Data
	/// </summary>
	public class MPBoeData
	{
		/// <summary>
		/// PTM Proposal Title
		/// </summary>
		public string PTMProposalTitle { get; set; }

		/// <summary>
		/// RFP Number
		/// </summary>
		public string RFPNumber { get; set; }

		/// <summary>
		/// Collection of CLIN Numbers ('DisplayedCLINNumber')
		/// </summary>
		public ICollection<string> CLINNumbers { get; set; }

		/// <summary>
		/// Collection of WBS Numbers ('DisplayedWBSNumber')
		/// </summary>
		public ICollection<string> WBSNumbers { get; set; }
	}
}