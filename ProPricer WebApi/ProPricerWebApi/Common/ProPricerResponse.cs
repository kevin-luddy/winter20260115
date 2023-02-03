/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Common
{
    using System.Collections.Generic;

    /// <summary>
    /// A Pro Pricer Response
    /// </summary>
    public class ProPricerResponse<T>
    {
        /// <summary>
        /// True if the request succeeded, false otherwise.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Error or other informational messages
        /// </summary>
        public ICollection<string> Messages { get; set; } = new List<string>();

        /// <summary>
        /// Data meeting the request criteria
        /// </summary>
        public T Data { get; set; } 
    }
}