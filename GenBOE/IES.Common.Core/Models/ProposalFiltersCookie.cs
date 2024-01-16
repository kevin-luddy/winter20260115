// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using IES.Common.Core.Enums;

namespace IES.Common.Core.Models
{
	/// <summary>
	/// Cookie for storing proposal filter options
	/// </summary>
	public class ProposalFiltersCookie
	{
		/// <summary>
		/// Gets or sets selected filter option (In Progress, Completed, All)
		/// </summary>
		public ProposalFilterOption FilterOption { get; set; }

		/// <summary>
		/// Gets or sets selected viewer filter option (Show only my proposals, Show my organization's proposals)
		/// </summary>
		public ViewerProposalFilterOption ViewerFilterOption { get; set; }

		/// <summary>
		/// Gets or sets filter start date
		/// </summary>
		public string FilterStartDate { get; set; }

		/// <summary>
		/// Gets or sets filter end date
		/// </summary>
		public string FilterEndDate { get; set; }

		/// <summary>
		/// Gets or sets selected Proposal Class filter option (All, Forecasted, Non-Forecasted).
		/// </summary>
		public ProposalClassFilterOption ProposalClassFilterOption { get; set; }
	}
}