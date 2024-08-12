// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BOE
{
	using IES.Common;
	using System.Collections.Generic;

	/// <summary>
	/// Post Model that holds all necessary properties for Posting to GenBOE</param>
	/// </summary>
	public class BOETotalsPostModel
	{
		/// <summary>
		/// PTM Tracking Number
		/// </summary>
		public string TrackingNumber { get; set; }

		/// <summary>
		/// BOE Form Type
		/// </summary>
		public BOEFormType BOEFormType { get; set; }

		/// <summary>
		/// Dictionary of BOE Id and Respective Resources 
		/// </summary>
		public ICollection<BOEResourcesPair> NlfResources { get; set; }
	}
}
