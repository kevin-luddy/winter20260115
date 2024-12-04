// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using IES.Common;

	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class BOEFormIBOEDTO : BOEFormDTO
	{
		/// <summary>
		/// ctor
		/// </summary>
		public BOEFormIBOEDTO() :
			base()
		{
		}

		/// <summary>
		/// The business area.
		/// </summary>
		public string BusinessArea { get; set; }

		/// <summary>
		/// The BOE Form type.
		/// </summary>
		public override BOEFormType BOEFormType
		{
			get
			{
				return BOEFormType.IBOE;
			}
		}

		#region NLF-only fields

		/// <summary>
		/// Workspace Proposal Title - used when there is no IBOE Proposal Title
		/// </summary>
		public string WorkspaceProposalTitle { get; set; }

		/// <summary>
		/// Workspace RFP Number - used when there is no IBOE Proposal Title
		/// </summary>
		public string WorkspaceRfpNumber { get; set; }

		/// <summary>
		/// The selected IWTA Resource Names
		/// </summary>
		public ICollection<string> Resources { get; set; }

		/// <summary>
		/// Collection of clin-contract xrefs
		/// </summary>
		public ICollection<ClinContractDto> ClinContractXrefs { get; set; }

		/// <summary>
		/// PTM Tracking Number
		/// </summary>
		public string PTMTrackingNumber { get; set; }

		#endregion NLF-only fields
	}
}
