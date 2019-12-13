namespace IES.Web.Common
{
    using GenBOE.Common;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    [ExcludeFromCodeCoverage]
    public static class SiteMasterUtilities
    {
        
        /// <summary>
        /// Returns true/false indicating whether the piwik should be disabled. This is used for classified installations.
        /// </summary>
        /// <returns>Bool whether the links should be shut off or not</returns>
        public static bool DisablePiwik()
        {
            bool result = false;

            if (!string.IsNullOrEmpty(ConfigurationUtilities.GetAppSetting("DisablePiwik"))
                && ConfigurationUtilities.GetAppSetting("DisablePiwik").ToLower().Equals("true"))
            {
                result = true;
            }

            return result;
        }
        
        /// <summary>
        /// Gets the Piwik URL from web.config
        /// </summary>
        /// <returns>Piwik Url</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public static string PiwikUrl()
        {
            return ConfigurationUtilities.GetAppSetting("PiwikURL");
        }

        /// <summary>
        /// Returns PiwikId from the web.config; if it doesn't exist -> returns 0
        /// </summary>
        /// <returns>Piwik Id for the site</returns>
        public static int PiwikId()
        {
            int result;

            if (!int.TryParse(ConfigurationUtilities.GetAppSetting("PiwikId"), out result))
            {
                result = 0;
            }

            return result;
        }

        /// <summary>
        /// Returns the Current user.
        /// </summary>
        /// <returns>The current user.</returns>
        public static string CurrentUser()
        {
            string[] splitDomainAndNtid = Thread.CurrentPrincipal.Identity.Name.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", splitDomainAndNtid);
        }
    }
}