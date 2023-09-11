// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using GenBOE.Models;
using System;

namespace GenBOE.Web.ModelView
{
	/// <summary>
	/// NLF Workspace inner Data Class (data returned from BOE)
	/// 
	/// THIS MUST MATCH THE BOE SERVICE IN NLF, DO NOT CHANGE
	/// </summary>
	public class NlfWorkspaceInnerData
	{
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
		/// Line of Business Id
		/// </summary>
		public int? LineOfBusinessId { get; set; }

		/// <summary>
		/// Line Of Business Name
		/// </summary>
		public string LineOfBusinessName { get; set; }

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