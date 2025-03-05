// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO.Travel
{
	using IES.Common;
	using IES.Common.Core.Models;

	public class MSTTravelNonzoneFeesAndCostsDTO : UpdateableDTO
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public MSTTravelNonzoneFeesAndCostsDTO()
		{
			Id = -1;
			ModeID = 0;
			TravelAgencyFee = 0m;
			MiscOther = 0m;
		}

		/// <summary>
		/// ID of the Travel Mode
		/// </summary>
		public int ModeID { get; set; }

		/// <summary>
		/// Travel Agency Fee for the Travel Mode
		/// </summary>
		public decimal TravelAgencyFee { get; set; }

		/// <summary>
		/// Miscellaneous/Other Cost for the Travel Mode
		/// </summary>
		public decimal MiscOther { get; set; }
	}
}
