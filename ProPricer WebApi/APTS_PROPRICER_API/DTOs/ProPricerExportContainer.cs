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
    /// Container class for a ProPricer Direct Import
    /// </summary>
    public class ProPricerImportContainer
    {
        /// <summary>
        /// Gets or sets the task data.
        /// </summary>
        public string taskData { get; set; }

        /// <summary>
        /// Gets or sets the task export option.
        /// </summary>
        public ProPricerExportOption taskExportOption { get; set; }

        /// <summary>
        /// Gets or sets the resource data.
        /// </summary>
        public string resourceData { get; set; }

        /// <summary>
        /// Gets or sets the resource export option.
        /// </summary>
        public ProPricerExportOption resourceExportOption { get; set; }
    }
}