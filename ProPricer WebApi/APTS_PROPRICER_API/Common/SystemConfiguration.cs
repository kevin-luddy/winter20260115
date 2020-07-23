/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi
{
    using System.ComponentModel;

    /// <summary>
    /// Singleton that stores system configurations from web.config
    /// </summary>
    public sealed  class SystemConfiguration
    {
        #region Private Members
        
        // static instance of this object
        static volatile private SystemConfiguration _UniqueInstance = null;

        // static lock object used to make methods thread safe
        static private object mLock = new object();

        private CompanyConfiguration _CompanyMode;
        private CompanyConfigurationSection _CompanyConfigurationSection;

        #endregion Private Members

        #region private methods

        /// <summary>
        /// Default private constructor due to singleton class instance
        /// </summary>
        private SystemConfiguration()
        {
            //get the company configuration string if it doesn't contain a valid value set to IS&GS
            // Note: This MUST NOT use the ConfigurationUtilities methods or an infinite loop condition will occur
            string sCompany = System.Web.Configuration.WebConfigurationManager.AppSettings["CompanyConfiguration"];
            this._CompanyMode = string.IsNullOrEmpty(sCompany) ? CompanyConfiguration.SpaceSystems : sCompany.GetEnumeratedValue<CompanyConfiguration>(CompanyConfiguration.SpaceSystems);

            this._CompanyConfigurationSection = SystemConfigurationSection.Section[this.CompanyMode];
        }

        #endregion private methods

        /// <summary>
        /// gets Company Configuration
        /// </summary>
        public CompanyConfiguration CompanyMode
        {
            get
            {
                return this._CompanyMode;
            }
        }

        /// <summary>
        /// Company-specific configuration override settings
        /// </summary>
        public CompanyConfigurationSection CompanyConfigurationSettings
        {
            get
            {
                return this._CompanyConfigurationSection;
            }
        }

        #region public methods

        /// <summary>
        /// The instance of the singleton
        /// </summary>
        /// <returns>the instance of the singleton</returns>
        public static SystemConfiguration Instance()
        {
            // double lock in case many threads hit first if check and then blocked on lock ... they will be filtered out on second if check
            if (_UniqueInstance == null)
            {
                lock (mLock)
                {
                    if (_UniqueInstance == null)
                    {
                        // get singleton reference
                        _UniqueInstance = new SystemConfiguration();

                    }
                }
            }

            return SystemConfiguration._UniqueInstance;
        }


        #endregion public methods

    }

    /// <summary>
    /// Defines the supported company configurations
    /// </summary>
    public enum CompanyConfiguration
    {
        /// <summary>
        /// None Selected
        /// </summary>
        None = 0,

        /// <summary>
        /// Space Systems
        /// </summary>
        [Description("Space")]
        SpaceSystems = 2,

        /// <summary>
        /// RMS
        /// </summary>
        [Description("RMS")]
        RMS = 3
    }
}
