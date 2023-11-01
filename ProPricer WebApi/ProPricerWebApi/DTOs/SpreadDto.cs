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
	/// Instance of a Spread
	/// </summary>
	public class SpreadDto
	{
		/// <summary>
		/// Gets or Sets the Year
		/// </summary>
		public int Year { get; set; }

		/// <summary>
		/// Gets or Sets the Month
		/// </summary>
		public int Month { get; set; }

		/// <summary>
		/// Gets or Sets the Value
		/// </summary>
		public string Value { get; set; }
	}
}