// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using IES.Common;
	using IES.Common.Core.Models;

	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class EscalationRatesDTO : UpdateableDTO
	{
		public EscalationRatesDTO()
		{
			EscalationRateID = -1;
			Year = 0;
			DevEscalation = 0;
			LMSIEscalation = 0;
			LockedRate = false;
		}

		public int EscalationRateID { get; set; }

		public int Year { get; set; }
		public decimal DevEscalation { get; set; }
		public decimal LMSIEscalation { get; set; }
		public decimal MiscRate { get; set; }
		public bool LockedRate { get; set; }
	}
}
