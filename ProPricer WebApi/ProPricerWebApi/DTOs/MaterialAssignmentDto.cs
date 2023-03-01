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
	/// Houses Material Assignment information
	/// </summary>
	public class MaterialAssignmentDto
	{
		/// <summary>
		/// Gets or Sets the Material Name
		/// </summary>
		public string MaterialName { get; set; }

		/// <summary>
		/// Gets or Sets the Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or Sets the Type
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Gets or Sets the Part Name
		/// </summary>
		public string PartName { get; set; }

		/// <summary>
		/// Gets or Sets the Part Description
		/// </summary>
		public string PartDescription { get; set; }

		/// <summary>
		/// Gets or Sets whether we make or buy the material
		/// </summary>
		public string MakeBuy { get; set; }

		/// <summary>
		/// Gets or Sets the Unit Quantity
		/// </summary>
		public string UnitQty { get; set; }

		/// <summary>
		/// Gets or Sets the Ship Quantity
		/// </summary>
		public string ShipQty { get; set; }

		/// <summary>
		/// Gets or Sets the Total Manufacturing Start Quantity
		/// </summary>
		public string TotalMfgStartQty { get; set; }

		/// <summary>
		/// Gets or Sets the Total Cost
		/// </summary>
		public string TotalCost { get; set; }

		/// <summary>
		/// Gets or Sets the Unit Cost
		/// </summary>
		public string UnitCost { get; set; }

		/// <summary>
		/// Gets or Sets the Resource Assignment
		/// </summary>
		public ResourceAssignmentDto ResourceAssignment { get; set; }

		/// <summary>
		/// Gets or Sets the Associated Costs
		/// </summary>
		public IEnumerable<AssociatedCostsDto> AssociatedCosts { get; set; }
	}
}