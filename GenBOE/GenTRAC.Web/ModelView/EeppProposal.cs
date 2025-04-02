// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.ModelView
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	/// <summary>
	/// The eEPP Proposal model, returned to PTM. This object must match the same class in eEPP
	public class EeppProposal
	{
		/// <summary>
		/// The eEPP Proposal ID
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The eEPP Proposal Status
		/// </summary>
		public string Status { get; set; }
	}
}