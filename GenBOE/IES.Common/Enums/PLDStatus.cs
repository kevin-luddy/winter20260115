// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Enums
{
	using System.ComponentModel;

	/// <summary>
	/// PLD PA Statuses
	/// </summary>
	public enum PLDStatus
	{
		[Description("Open")]
		Open = 1,
		[Description("Submitted")]
		Submitted = 2,
		[Description("Awarded/Finalized")]
		AwardedFinalized = 3,
		[Description("Canceled")]
		Canceled = 4,
		[Description("Inactive")]
		Inactive = 5,
		[Description("Expired")]
		Expired = 6,
		[Description("Negotiated")]
		Negotiated = 7,
		[Description("In Negotiation")]
		InNegotiation = 8,
		[Description("Loss")]
		Loss = 9
	}
}
