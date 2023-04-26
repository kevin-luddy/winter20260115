// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace GenBOE.Dtos
{
	using GenBOE.Models;
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// DTO intended to show necessary inner data of Workspace for use with NLF
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class NlfWorkspaceInnerDataDTO
	{
		/// <summary>
		/// Default Contructor
		/// </summary>
		public NlfWorkspaceInnerDataDTO()
		{
			this.WorkspaceId = -1;
			this.WorkspaceUrl = null;
			this.WorkspaceName = null;
			this.LineOfBusiness = null;
			this.PTMTrackingNumber = null;
			this.WorkspaceCreationDate = null;
			this.EstimatingLead = null;
		}

		/// <summary>
		/// Workspace Id
		/// </summary>
		public int WorkspaceId { get; set; }

		/// <summary>
		/// Workspace URL (shortname)
		/// </summary>
		public string WorkspaceUrl { get; set; }

		/// <summary>
		/// Workspace/Proposal (long) Name
		/// </summary>
		public string WorkspaceName { get; set; }

		/// <summary>
		/// Line of Business
		/// </summary>
		public LineOfBusiness LineOfBusiness { get; set; }

		/// <summary>
		/// PTM Tracking Number
		/// </summary>
		public string PTMTrackingNumber { get; set; }

		/// <summary>
		/// Workspace Creation Date
		/// </summary>
		public DateTime? WorkspaceCreationDate { get; set; }

		/// <summary>
		/// Estimating Lead
		/// </summary>
		public string EstimatingLead { get; set; }
	}
}
