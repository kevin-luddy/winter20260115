// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO.Request
{
	using System.ComponentModel;

	/// <summary>
	/// Enum defining the request type
	/// </summary>
	public enum RequestType
	{
		/// <summary>
		///Calculate Actuals
		/// </summary>
		[Description("Calculate Actuals")]
		CalculateActuals = 1,

		/// <summary>
		/// Export BOE
		/// </summary>
		[Description("Export BOE")]
		ExportBOE = 2,
	}
}
