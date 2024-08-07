// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BOE
{
	using System;

	/// <summary>
	/// Row Data for PBOE used for PBOE Table on NLF
	/// </summary>
	public class PBOERow
	{
		/// <summary>
		/// PBOE Id on NLF
		/// </summary>
		public int PBOEId { get; set; }

		/// <summary>
		/// PBOE Name
		/// </summary>
		public string FormName { get; set; }

		/// <summary>
		/// Cost - Total Cost in calculations
		/// </summary>
		public decimal Cost { get; set; }

		/// <summary>
		/// T&M Cost
		/// </summary>
		public decimal TMCost { get; set; }

		/// <summary>
		/// Total Cost => Cost + T&M Cost
		/// </summary>
		public decimal TotalCost { get; set; }

		/// <summary>
		/// Update Date of the BOE
		/// </summary>
		public DateTime UpdateDT { get; set; }

		/// <summary>
		/// BOE Validation Status
		/// </summary>
		public string ValidationStatus { get; set; }
	}
}
