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
	/// DTO for associated Costs
	/// </summary>
	public class AssociatedCostsDto
	{
		/// <summary>
		/// Gets or sets the 
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Gets or sets the Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Resource
		/// </summary>
		public ResourcesDto Resource { get; set; }

		/// <summary>
		/// Gets or sets the Resource Fields
		/// </summary>
		public IEnumerable<ResourceFieldsDto> ResourceFields { get; set; }

		/// <summary>
		/// Gets or sets the Amount
		/// </summary>
		public double Amount { get; set; }

		/// <summary>
		/// Gets or sets the Total Amount
		/// </summary>
		public double? TotalAmount { get; set; }

		/// <summary>
		/// Gets or sets the Link Qty
		/// </summary>
		public bool LinkQty { get; set; }

		/// <summary>
		/// Gets or sets the Link Spread
		/// </summary>
		public bool LinkSpread { get; set; }

		/// <summary>
		/// Gets or sets the Start Date
		/// </summary>
		public string StartDate { get; set; }

		/// <summary>
		/// Gets or sets the End Date
		/// </summary>
		public string EndDate { get; set; }

		/// <summary>
		/// Gets or sets the Spread Curve
		/// </summary>
		public string SpreadCurve { get; set; }

		/// <summary>
		/// Gets or sets the list of Spreads
		/// </summary>
		public IEnumerable<SpreadDto> Spread { get; set; }
	}
}