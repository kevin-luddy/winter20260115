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
	/// DTO for Travel Trip information
	/// </summary>
	public class TravelsDto
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
		/// Gets or Sets the Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or Sets the Destination
		/// </summary>
		public string Destination { get; set; }

		/// <summary>
		/// Gets or Sets the Destination Description
		/// </summary>
		public string DestinationDescription { get; set; }

		/// <summary>
		/// Gets or Sets the Comments
		/// </summary>
		public string Comments { get; set; }

		/// <summary>
		/// Gets or Sets the number of People
		/// </summary>
		public int People { get; set; }

		/// <summary>
		/// Gets or Sets the Days
		/// </summary>
		public string Days { get; set; }

		/// <summary>
		/// Gets or Sets the number of Trips
		/// </summary>
		public int Trips { get; set; }

		/// <summary>
		/// Gets or Sets the Trip Cost
		/// </summary>
		public string TripCost { get; set; }

		/// <summary>
		/// Gets or Sets the Total Cost
		/// </summary>
		public string TotalCost { get; set; }

		/// <summary>
		/// Gets or Sets the ResourceAssignment
		/// </summary>
		public ResourceAssignmentDto ResourceAssignment { get; set; }

		/// <summary>
		/// Gets or Sets the Expenses
		/// </summary>
		public IEnumerable<TravelExpenseDto> Expenses { get; set; }
	}
}