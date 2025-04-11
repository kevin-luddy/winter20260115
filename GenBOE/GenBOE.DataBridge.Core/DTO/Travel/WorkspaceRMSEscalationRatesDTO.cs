// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO.Travel
{
	using IES.Common;
	using IES.Common.Core.Models;

	/// <summary>
	/// Escalation Rates DTO at the Workspace Level
	/// </summary>
	public class WorkspaceRMSEscalationRatesDTO : UpdateableDTO
	{
		/// <summary>
		/// Public constructor.
		/// </summary>
		public WorkspaceRMSEscalationRatesDTO()
		{
			this.Id = -1;
			WorkspaceId = -1;
			AirfareRate = 0;
			PerDiemRate = 0;
			MiscRate = 0;
		}

		/// <summary>
		/// The workspace Id.
		/// </summary>
		public int WorkspaceId { get; set; }


		public int Year { get; set; }

		public decimal AirfareRate { get; set; }

		public decimal PerDiemRate { get; set; }

		public decimal MiscRate { get; set; }
	}
}
