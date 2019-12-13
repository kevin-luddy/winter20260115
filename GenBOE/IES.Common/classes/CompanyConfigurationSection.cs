// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Specialized;
using System.Configuration;

namespace IES.Common
{
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
