// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using IES.Common.classes;

    /// <summary>
    /// Utilities for accessing configuration settings
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ConfigurationUtilities
    {
        #region AppSettings

        /// <summary>
        /// Retrieve a typed value from the application settings.  Check the company-specific configuration first.  If no override value is found, then check the application-level settings.
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="key">Settings key</param>
        /// <returns>The application setting, as a typed value</returns>
        public static T GetAppSetting<T>(string key) where T : struct, IConvertible
        {
            return GetAppSetting(key, default(T));
        }

        /// <summary>
        /// Retrieve a string value from the application settings.  Check the company-specific configuration first.  If no override value is found, then check the application-level settings.
        /// </summary>
        /// <param name="key">Settings key</param>
        /// <returns>The application setting</returns>
        public static string GetAppSetting(string key)
        {
            string value = SystemConfiguration.Instance().CompanyConfigurationSettings.AppSettings[key];

            if (string.IsNullOrEmpty(value))
            {
                value = System.Web.Configuration.WebConfigurationManager.AppSettings[key];
            }

            return value;
        }

        /// <summary>
        /// Retrieve a typed value from the application settings.  Check the company-specific configuration first.  If no override value is found, then check the application-level settings.
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="key">Settings key</param>
        /// <param name="defaultValue">Value to use if the setting does not exist</param>
        /// <returns>The application setting, as a typed value</returns>
        public static T GetAppSetting<T>(string key, T defaultValue) where T : IConvertible
        {
            T value = defaultValue;

            string stringValue = GetAppSetting(key);

            if (!string.IsNullOrEmpty(stringValue))
            {
                Type typeT = typeof(T);

                try
                {
                    if (typeT.IsEnum)
                    {
                        value = (T)Enum.Parse(typeT, stringValue);
                    }
                    else
                    {
                        value = (T)Convert.ChangeType(stringValue, typeT);
                    }
                }
                catch (InvalidCastException)
                {
                    value = defaultValue;
                }
                catch (FormatException)
                {
                    value = defaultValue;
                }
                catch (OverflowException)
                {
                    value = defaultValue;
                }
            }

            return value;
        }

        #endregion

        #region ConnectionStrings

        /// <summary>
        /// Retrieve connection string settings from the web configuration.  Check the company-specific configuration first.  If no override value is found, then check the application-level settings.
        /// </summary>
        /// <param name="key">Connection string key</param>
        /// <returns>The connection string settings</returns>
        public static ConnectionStringSettings GetConnectionString(string key)
        {
            ConnectionStringSettings value = SystemConfiguration.Instance().CompanyConfigurationSettings.ConnectionStrings[key];

            if (value == null)
            {
                value = System.Web.Configuration.WebConfigurationManager.ConnectionStrings[key];
            }

            return value;
        }

        #endregion

        #region Global properties

        /// <summary>
        /// Absolute URI prefix of the site
        /// </summary>
        private static string _siteUrlPrefix = null;

        /// <summary>
        /// Absolute URI prefix of the site
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public static string SiteUrlPrefix
        {
            get
            {
                return _siteUrlPrefix;
            }
        }

        /// <summary>
        /// Set the <seealso cref="SiteUrlPrefix"/> property.
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void SetSiteUrlPrefix(HttpContextBase httpContext, Logger log = null)
        {
            if (_siteUrlPrefix == null && httpContext != null)
            {
                string prefix;

                try
                {
                    prefix = httpContext.Request.Url.AbsoluteUri.Substring(0, httpContext.Request.Url.AbsoluteUri.Length - httpContext.Request.Url.AbsolutePath.Length);
                }
                catch (Exception ex)
                {
                    if (log != null)
                    {
                        log.Error(ex);
                        log.Warn("Using ServerURL application setting for SiteUrlPrefix.");
                    }

                    prefix = ConfigurationUtilities.GetAppSetting("ServerURL");

                    if (!prefix.StartsWith("http"))
                    {
                        prefix = string.Format("http://{0}", prefix);
                    }
                }

                if (log != null)
                {
                    log.Info(string.Format("SiteUrlPrefix = {0}", prefix));
                }

                _siteUrlPrefix = prefix;
            }
        }

        /// <summary>
        /// Ies Portal URL
        /// </summary>
        /// <param name="active">Active application name, i.e. PTM, RDM, etc.</param>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public static string IESHeaderUrl(string active)
        {
            if (active == null)
            {
                throw new ArgumentNullException(nameof(active));
            }

            string iesUrl = ConfigurationUtilities.GetAppSetting("IESHomeUrl");
            return string.IsNullOrEmpty(iesUrl) ? string.Empty : iesUrl + "Header?active=" + active;
        }
        
        /// <summary>
        /// IES Portal Banner Url
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public static string IESBannerUrl(string active)
        {
            if (active == null)
            {
                throw new ArgumentNullException(nameof(active));
            }

            string iesUrl = ConfigurationUtilities.GetAppSetting("IESHomeUrl");

            return string.IsNullOrEmpty(iesUrl) ? string.Empty : iesUrl + "Banner?active=" + active;
        }

        /// <summary>
        /// IES App Offline Url
        /// </summary>
        /// <param name="app">application name</param>
        /// <returns>url</returns>
        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public static string AppOfflineUrl(string app)
        {
            if (string.IsNullOrEmpty(app))
            {
                throw new ArgumentNullException(nameof(app));
            }

            string iesUrl = ConfigurationUtilities.GetAppSetting("IESHomeUrl");

            return string.IsNullOrEmpty(iesUrl) ? string.Empty : iesUrl + "AppOffline?app=" + app;
        }

        #endregion
    }
}
