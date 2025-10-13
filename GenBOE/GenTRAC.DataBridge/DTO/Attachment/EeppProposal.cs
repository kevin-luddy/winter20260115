// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
	using IES.Common;

	/// <summary>
	/// The eEPP Proposal model, returned to PTM. This object must match the same class in eEPP
	/// </summary>
	public class EeppProposal
	{
		/// <summary>
		/// The eEPP Proposal ID
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The EPP Level
		/// </summary>
		public EPPLevel EppLevel { get; set; }

		/// <summary>
		/// The eEPP Proposal Status
		/// </summary>
		public string Status { get; set; }
	}
}