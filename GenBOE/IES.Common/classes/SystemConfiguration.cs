// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


namespace IES.Common.classes
{
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
            this._CompanyMode = string.IsNullOrEmpty(sCompany) ? CompanyConfiguration.ISGS : sCompany.GetEnumeratedValue<CompanyConfiguration>(CompanyConfiguration.ISGS);

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
                return _CompanyMode;
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
}
