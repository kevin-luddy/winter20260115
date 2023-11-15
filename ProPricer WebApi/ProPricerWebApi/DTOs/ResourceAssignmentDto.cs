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
	/// DTO for Resource Assignment
	/// </summary>
	public class ResourceAssignmentDto
	{
		/// <summary>
		/// Gets or Sets the Id
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Gets or Sets the Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or Sets the Info Description
		/// </summary>
		public string InfoDescription { get; set; }

		/// <summary>
		/// Gets or Sets the Resource Fields
		/// </summary>
		public IEnumerable<ResourceFieldsDto> ResourceFields { get; set; }

		/// <summary>
		/// Gets or Sets the Source Type
		/// </summary>
		public string SourceType { get; set; }

		/// <summary>
		/// Gets or Sets the Amount
		/// </summary>
		public string Amount { get; set; }

		/// <summary>
		/// Gets or Sets the Start Date
		/// </summary>
		public string StartDate { get; set; }

		/// <summary>
		/// Gets or Sets the End Date
		/// </summary>
		public string EndDate { get; set; }

		/// <summary>
		/// Gets or Sets the Spread Curve
		/// </summary>
		public string SpreadCurve { get; set; }

		/// <summary>
		/// Gets or Sets the Spread
		/// </summary>
		public IEnumerable<SpreadDto> Spread { get; set; }

		/// <summary>
		/// Gets or Sets the Direct Cost
		/// </summary>
		public string DirectCost { get; set; }

		/// <summary>
		/// Gets or Sets the Price
		/// </summary>
		public string Price { get; set; }

		/// <summary>
		/// Gets or Sets the Burden Cost
		/// </summary>
		public IEnumerable<BurdenCostDto> BurdenCost { get; set; }
	}
}