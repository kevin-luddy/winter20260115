// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// DTO intended to show Material PBoe Data
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable]
	public class MPBoeDataDTO
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public MPBoeDataDTO()
		{
			this.PTMProposalTitle = null;
			this.RFPNumber = null;
			this.CLINNumbers = null;
			this.WBSNumbers = null;
			this.TrackingNumber = null;
		}

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
		/// Workspace Name
		/// </summary>
		public string WorkspaceName { get; set; }

		/// <summary>
		/// Workspace ShortName
		/// </summary>
		public string ShortName { get; set; }

		/// <summary>
		/// Collection of WBS Numbers ('DisplayedWBSNumber')
		/// </summary>
		public ICollection<string> WBSNumbers { get; set; }

		/// <summary>
		/// Tracking Number
		/// </summary>
		public string TrackingNumber { get; set; }

	}
}