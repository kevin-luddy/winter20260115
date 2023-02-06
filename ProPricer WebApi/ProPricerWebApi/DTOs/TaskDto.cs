/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.DTOs
{
	using System.Collections.Generic;

	/// <summary>
	/// Incomplete Data Transfer Object for Tasks
	/// </summary>
	public class TaskDto
	{
		/// <summary>
		/// Gets or Sets the id
		/// </summary>
		public String Id { get; set; }

		/// <summary>
		/// Gets or Sets the name
		/// </summary>
		public String Name { get; set; }

		/// <summary>
		/// Gets or Sets the Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or Sets the Start Date
		/// </summary>
		public string StartDate { get; set; }

		/// <summary>
		/// Gets or Sets the End Date
		/// </summary>
		public string EndDate { get; set; }

		/// <summary>
		/// Gets or Sets the Actual Fee
		/// </summary>
		public string ActualFee { get; set; }

		/// <summary>
		/// Gets or Sets the Quantity
		/// </summary>
		public int Quantity { get; set; }

		/// <summary>
		/// Gets or Sets the Resource Assignments
		/// </summary>
		public IEnumerable<ResourceAssignmentDto> ResourceAssignments { get; set; }

		/// <summary>
		/// Gets or Sets the Material Assignments
		/// </summary>
		public IEnumerable<MaterialAssignmentDto> MaterialAssignments { get; set; }

		/// <summary>
		/// Gets or Sets the Summary Fields
		/// </summary>
		public IEnumerable<SummaryFieldsDto> SummaryFields { get; set; }

		/// <summary>
		/// Gets or Sets the Travel information
		/// </summary>
		public IEnumerable<TravelsDto> Travels { get; set; }
	}
}