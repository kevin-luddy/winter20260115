/*
	Copyright 2016-2018 Lockheed Martin Corporation.

	This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
	commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
	by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
	and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi
{
	using System;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;

	/// <summary>
	/// Sort Order
	/// </summary>
	public enum SortOrder
	{
        /// <summary>
        /// The descending order
        /// </summary>
        Descending,

        /// <summary>
        /// The ascending order
        /// </summary>
        Ascending
    }
}