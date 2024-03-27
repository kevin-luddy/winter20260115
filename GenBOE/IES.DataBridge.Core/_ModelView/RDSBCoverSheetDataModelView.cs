// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Backend.Models
{
	/// <summary>
	/// RDSB Cover Sheet Data.
	/// </summary>
	public class RDSBCoverSheetDataModelView
	{
		/// <summary>
		/// Casb section.
		/// </summary>
		public string CasbSection { get; set; }

		/// <summary>
		/// Non compliance section.
		/// </summary>
		public string NonComplianceSection { get; set; }

		/// <summary>
		/// Adequate disclosure section.
		/// </summary>
		public bool? AdequateDisclosure { get; set; }

		/// <summary>
		/// Non compliance notification.
		/// </summary>
		public bool? NoncomplianceNotification {  get; set; }
	}
}