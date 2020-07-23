/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi
{
    using System.Collections.Specialized;
    using System.Configuration;

    /// <summary>
    /// Individual company configuration and override settings
    /// </summary>
    /// <seealso cref="SystemConfigurationSection"/>
    public class CompanyConfigurationSection
    {
        private CompanyConfiguration _Company;
        private NameValueCollection _AppSettings;
        private ConnectionStringSettingsCollection _ConnectionStrings;

        /// <summary>
        /// Constructor
        /// </summary>
        public CompanyConfigurationSection()
        {
            this._Company = CompanyConfiguration.None;
            this._AppSettings = new NameValueCollection();
            this._ConnectionStrings = new ConnectionStringSettingsCollection();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="company">Company</param>
        public CompanyConfigurationSection(CompanyConfiguration company) : this()
        {
            this._Company = company;
        }

        /// <summary>
        /// Company identifier
        /// </summary>
        public CompanyConfiguration Company { get { return this._Company; } }

        /// <summary>
        /// Override values for application settings
        /// </summary>
        public NameValueCollection AppSettings { get { return this._AppSettings; } }

        /// <summary>
        /// Override values for connection strings
        /// </summary>
        public ConnectionStringSettingsCollection ConnectionStrings { get { return this._ConnectionStrings; } }
    }
}
