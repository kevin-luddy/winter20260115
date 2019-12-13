// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace CopyWorkspace
{

    using IES.Common;

    /// <summary>
    /// Security Information override for the Active User NT ID
    /// </summary>
    /// <seealso cref="IES.Common.SecurityInformation" />
    public class SecurityInformationOverride : SecurityInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityInformationOverride"/> class.
        /// </summary>
        /// <param name="utils">The utils.</param>
        /// <param name="memCache">The memory cache.</param>
        public SecurityInformationOverride(IActiveDirectoryUtilities utils, ICache memCache) : base(utils, memCache)
        {
        }

        /// <summary>
        /// Returns the name of the currently logged on user (ex. acct04\jsmith)
        /// </summary>
        protected override string ActiveUserNTIDWithDomain
        {
            get
            {
                return ConfigurationUtilities.GetAppSetting("ActiveUserNTIDWithDomain");
            }
        }
    }
}
