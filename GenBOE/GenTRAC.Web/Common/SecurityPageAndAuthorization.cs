// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Web.Common
{
    using IES.Common;

    /// <summary>
    /// This class will aid in creating a security matrix dictionary, used by the base controller to do security checks.
    /// </summary>
    internal class SecurityPageAndAuthorization
    {
        /// <summary>
        /// Initializes a new instance of the SecurityPageAndAuthorization class
        /// </summary>
        /// <param name="inPage">Security Page</param>
        /// <param name="inAuthorization">Security Authorization</param>
        public SecurityPageAndAuthorization(PtmSecurityPage inPage, SecurityAuthorization inAuthorization)
        {
            this.Page = inPage;
            this.Authorization = inAuthorization;
        }

        /// <summary>
        /// Gets or sets Security Page
        /// </summary>
        public PtmSecurityPage Page { get; set; }

        /// <summary>
        /// Gets or sets Security Authorization
        /// </summary>
        public SecurityAuthorization Authorization { get; set; }
    }
}