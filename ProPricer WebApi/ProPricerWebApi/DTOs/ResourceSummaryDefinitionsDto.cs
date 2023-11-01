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
	/// Resource Summary Definitions 
	/// </summary>
	public class ResourceSummaryDefinitionsDto
	{
		/// <summary>
		/// Gets or Sets the id
		/// </summary>
		public String id { get; set; }

		/// <summary>
		/// Gets or Sets the name
		/// </summary>
		public String name { get; set; }

		/// <summary>
		/// Gets or Sets the dataType
		/// </summary>
		public String dataType { get; set; }

		/// <summary>
		/// Gets or Sets the max Length
		/// </summary>
		public Byte maxLength { get; set; }

		/// <summary>
		/// Gets or Sets the decimals
		/// </summary>
		public String decimals { get; set; }

		/// <summary>
		/// Gets or Sets the default Value
		/// </summary>
		public String defaultValue { get; set; }

		/// <summary>
		/// Gets or Sets the validate
		/// </summary>
		public Boolean validate { get; set; }

		/// <summary>
		/// Gets or Sets the required
		/// </summary>
		public Boolean required { get; set; }
	}
}