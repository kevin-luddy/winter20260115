// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// DTO intended to show Workspace Data for use with NLF
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class NlfWorkspaceDataDTO
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public NlfWorkspaceDataDTO()
		{
			this.WorkspaceId = -1;
			this.WorkspaceUrl = null;
			this.WorkspaceName = null;
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
	}
}
