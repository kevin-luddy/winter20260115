/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.DTOs
{
	/// <summary>
	/// Data Transfer Object for Traceabilty Report
	/// </summary>
	public class TraceReportDto
	{
		/// <summary>
		/// Gets or sets Res Name
		/// </summary>
		public string ResName { get; set; }

		/// <summary>
		/// Gets or sets Res Description
		/// </summary>
		public string ResDescription { get; set; }

		/// <summary>
		/// Gets or sets Res Class
		/// </summary>
		public string ResClass { get; set; }

		/// <summary>
		/// Gets or sets Pp Col
		/// </summary>
		public string PpCol { get; set; }

		/// <summary>
		/// Gets or sets Discrete Amt
		/// </summary>
		public double? DiscreteAmt { get; set; }

		/// <summary>
		/// Gets or sets Factored Amt
		/// </summary>
		public double? FactoredAmt { get; set; }
	}
}