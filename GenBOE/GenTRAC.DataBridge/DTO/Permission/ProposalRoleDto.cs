// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO.Permission
{
	using IES.Common;

	/// <summary>
	/// DTO containing the Proposal's tracking number and user's role within the proposal for use within NLF
	/// </summary>
	public class ProposalRoleDto
	{
		/// <summary>
		/// ctor
		/// </summary>
		public ProposalRoleDto()
		{
			Role = PtmRole.NotSet;	
		}

		/// <summary>
		/// Proposal Id
		/// </summary>
		public string TrackingNumber { get; set; }

		/// <summary>
		/// PTM Role
		/// </summary>
		public PtmRole Role { get; set; }
	}
}
