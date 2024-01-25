// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2013 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IdentitySwap
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
	using System.Security.Principal;
    using System.Threading;
    using System.Web;
	using Microsoft.AspNetCore.Http;

	/// <summary>
	/// Used to impersonate a user, for testing purposes. It is driven by cookies.
	/// </summary>
	public class IdentitySwappingForTesting
    {
        #region Constants

        /// <summary>
        /// Cookie name, used for Impersonation User
        /// </summary>
        private static string COOKIE_FOR_IMPERSONATION_USER = "IdentitySwappingCookieUser";

        /// <summary>
        /// Cookie name, used for Impersonation Roles
        /// </summary>
        private static string COOKIE_FOR_IMPERSONATION_ROLES = "IdentitySwappingCookieRoles";

        /// <summary>
        /// Value
        /// </summary>
        private static string VALUE = "value";

        #endregion

        #region Settings

        /// <summary>
        /// Cookie Expiration Period, by default 30 minutes
        /// </summary>
        private static int cookieExpiration = 30;

        /// <summary>
        /// Cookie Expiration Period in minutes (by default it's set to 30 minutes)
        /// </summary>
        public static int CookieExpirationTimeInMinutes
        {
            get
            {
                return cookieExpiration;
            }

            set
            {
                cookieExpiration = value;
            }
        }

        #endregion

        #region !! Static !! properties

        /// <summary>
        /// Dictionary used to keep track of users we evaluated already, to reduce the number of AD calls needed;
        /// If the groups change in web.config it gets automatically blown out, so it doesn't store outdated information;
        /// </summary>
        private static Dictionary<string, bool> usersWithGroupsAllowedToSwapIdentity;

        /// <summary>
        /// Property to access the dictionary, used so that way if it's null it gets setup properly..
        /// </summary>
        private static Dictionary<string, bool> UsersWithGroupsAllowedToSwapIdentity
        {
            get
            {
                if (usersWithGroupsAllowedToSwapIdentity == null)
                {
                    usersWithGroupsAllowedToSwapIdentity = new Dictionary<string, bool>();
                }

                return usersWithGroupsAllowedToSwapIdentity;
            }

            set
            {
                usersWithGroupsAllowedToSwapIdentity = value;
            }
        }

        #endregion

        /// <summary>
        /// Causes impersonation to happen, based on the values stored in the cookies. This needs to happen every time a userId is being requested, because it doesn't get persisted.
        /// This method does NOT throw an exception.
        /// </summary>
        /// <param name="keyForADPath">Key for Active Directory Path</param>
        /// <returns>Boolean indicating whether impersonation was successful</returns>
        private static bool SwapIdentity(string keyForADPath, HttpContext httpContext)
        {
            bool result = false;

            try
            {
                bool impersonationAllowed = true;

                if (impersonationAllowed)
                {
                    // Get the cookie that contains the information. If the cookie doesn't exist, we do nothing
                    string cookieUser = httpContext.Request.Cookies[COOKIE_FOR_IMPERSONATION_USER];
					string cookieRoles = httpContext.Request.Cookies[COOKIE_FOR_IMPERSONATION_ROLES];
                    
                    // Setup Data
                    string userWithDomain = string.Empty;
                    string roles = string.Empty;

                    // Get data from Cookies
                    if (cookieUser != null)
                    {
                        string userWithOrWithoutDomain = cookieUser;
                        userWithDomain = AttemptToResolveDomainForUsersNtid(keyForADPath, userWithOrWithoutDomain);

                        if (userWithOrWithoutDomain != userWithDomain)
                        {
                            cookieUser = userWithDomain;
                        }

						// Update the actual cookie
						httpContext.Response.Cookies.Delete(COOKIE_FOR_IMPERSONATION_USER);
						httpContext.Response.Cookies.Append(COOKIE_FOR_IMPERSONATION_USER, cookieUser, 
                            new CookieOptions()
                            {
                                Expires = DateTime.Now.AddMinutes(CookieExpirationTimeInMinutes)
					        }
                        );
                    }

                    if (cookieRoles != null)
                    {
                        roles = cookieRoles;

						// Update the actual cookie
						httpContext.Response.Cookies.Delete(COOKIE_FOR_IMPERSONATION_ROLES);
						httpContext.Response.Cookies.Append(COOKIE_FOR_IMPERSONATION_ROLES, cookieRoles , 
                            new CookieOptions()
                            {
                                Expires = DateTime.Now.AddMinutes(CookieExpirationTimeInMinutes)
							}
                        );
                    }

                    // Process Data
                    if (!string.IsNullOrEmpty(userWithDomain))
                    {
                        // Process Roles, if there were any
                        string[] rolesForImpersonatedUser = null;
                        try
                        {
                            if (!string.IsNullOrEmpty(roles))
                            {
                                rolesForImpersonatedUser = roles.Split(',');
                            }
                        }
                        catch
                        {
                            rolesForImpersonatedUser = null;
                        }

                        // Setup Principal that we will be impersonating            
                        GenericPrincipal impersonatedUser = new GenericPrincipal(new GenericIdentity(userWithDomain, string.Empty), rolesForImpersonatedUser);

                        // Impersonate the user; Since we don't know if you are using Thread or Context to get Current User, we modify both.
                        Thread.CurrentPrincipal = impersonatedUser;
                        httpContext.User = impersonatedUser;

                        result = true;
                    }
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        #region Public Methods

        #region (OPTIONAL) Cookie Handling

        // this is normally done via JS, but if you want to hook it up manually, you can use these methods

        /// <summary>
        /// Creates a cookie that will be used to store the values for impersonation
        /// </summary>
        /// <param name="userWithDomain">domain and NTID of the user you want to impersonate</param>
        /// <param name="roles">(optional) A comma separated list of roles that you want the user to have</param>
        public static void CreateCookieForSwappingIdentities(HttpContext httpContext, string userWithDomain, string roles = null)
        {
            httpContext.Response.Cookies.Delete(COOKIE_FOR_IMPERSONATION_USER);
			httpContext.Response.Cookies.Append(COOKIE_FOR_IMPERSONATION_USER, userWithDomain, 
                new CookieOptions()
                {
                    Expires = DateTime.Now.AddMinutes(CookieExpirationTimeInMinutes)
		        }
            );

            httpContext.Response.Cookies.Delete(COOKIE_FOR_IMPERSONATION_ROLES);
            httpContext.Response.Cookies.Append(COOKIE_FOR_IMPERSONATION_ROLES, roles,
				new CookieOptions()
				{
					Expires = DateTime.Now.AddMinutes(CookieExpirationTimeInMinutes)
				}
			);
		}

        /// <summary>
        /// Clears out the impersonation cookie, basically ending the Impersonation
        /// </summary>
        public static void ClearIdentitySwappingCookie(HttpContext httpContext)
        {
            httpContext.Response.Cookies.Delete(COOKIE_FOR_IMPERSONATION_USER);
			httpContext.Response.Cookies.Delete(COOKIE_FOR_IMPERSONATION_ROLES);
        }

        #endregion

        /// <summary>
        /// If web.config setting is set to true, it causes impersonation to happen. This is the prefered way of calling this method.
        /// Causes impersonation to happen, based on the values stored in the cookies. This needs to happen every time a userId is being requested, because it doesn't get persisted.
        /// This method does NOT throw an exception.
        /// </summary>
        /// <param name="keyForWhetherWeShouldProceed">A key in web.config appSetting's section that indicates whether impersonation should happen</param>
        /// <param name="keyForADPath">A key in web.config appSetting's section that gives us the Active Directory path</param>
        /// <param name="keyForAllowedGroups">A key in web.config appSetting's section that gives us a comma separated list of Active Directory groups that allow users to Swap Identity</param>
        /// <returns>Boolean indicating whether impersonation was successful</returns>
        public static bool SwapIdentity(string keyForWhetherWeShouldProceed, string keyForADPath, string keyForAllowedGroups, HttpContext httpContext)
        {
            bool result = false;
            try
            {
                if (IsIdentitySwappingAllowed(keyForWhetherWeShouldProceed, keyForADPath, keyForAllowedGroups))
                {
                    result = SwapIdentity(keyForADPath, httpContext);
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Returns true if impersonation should happen, based on the web.config setting.
        /// </summary>
        /// <param name="keyForWhetherWeShouldProceed">A key in web.config appSetting's section that indicates whether impersonation should happen</param>
        /// <returns>True or false, indicating if impersonation should happen</returns>
        public static bool IsIdentitySwappingAllowed(string keyForWhetherWeShouldProceed, string keyForADPath, string keyForAllowedGroups)
        {
            bool result = false;

            try
            {
                string shouldWeProceed = ConfigurationManager.AppSettings[keyForWhetherWeShouldProceed];

                if(shouldWeProceed != null && shouldWeProceed.ToLower() == bool.TrueString.ToLower())
                {
                    string currentNtidWithDomain = Thread.CurrentPrincipal.Identity.Name;

                    // We want to check the dictionary first; if it contains the record already, then we do not need to try AD again, but return whatever we found last time
                    if (UsersWithGroupsAllowedToSwapIdentity.ContainsKey(currentNtidWithDomain))
                    {
                        result = UsersWithGroupsAllowedToSwapIdentity[currentNtidWithDomain];
                    }
                    else
                    {
                        string adPath = ConfigurationManager.AppSettings[keyForADPath];
                        string allowedGroupsCommaSeparatedList = ConfigurationManager.AppSettings[keyForAllowedGroups];

                        List<string> allowedGroupsList = new System.Collections.Generic.List<string>();
                        if (allowedGroupsCommaSeparatedList != null)
                        {
                            string[] allowedGroups = allowedGroupsCommaSeparatedList.Split(',');

                            if (allowedGroups != null && allowedGroups.Any())
                            {
                                allowedGroupsList = allowedGroups.Select(x => x.Trim()).Where(x => x != ",").ToList();
                            }

                            ADClass adClass = new ADClass(adPath);

                            result = adClass.DoesUserBelongToGroup(currentNtidWithDomain, allowedGroupsList);
                            UsersWithGroupsAllowedToSwapIdentity.Add(currentNtidWithDomain, result);
                        }
                    }
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Attempts to use the AD to resolve the domain for a user's NTID, if the domain was not provided
        /// </summary>
        /// <param name="keyForADPath">Key for the AD Path (in web.config)</param>
        /// <param name="originalNtid">Originally supplied NTID</param>
        /// <returns>NTID with domain prepended to it</returns>
        private static string AttemptToResolveDomainForUsersNtid(string keyForADPath, string originalNtid)
        {
            string result = originalNtid;

            try
            {
                // If the NTID contains \\, we assume that the domain was supplied by the user, and so we do nothing
                // Otherwise, we try to get the domain from the AD
                if (!originalNtid.Contains("\\"))
                {
                    string adPath = ConfigurationManager.AppSettings[keyForADPath];
                    ADClass adClass = new ADClass(adPath);

                    string domain = adClass.GetDomainName(originalNtid);

                    if (!string.IsNullOrEmpty(domain))
                    {
                        result = domain + "\\" + originalNtid;
                    }
                }
            }
            catch
            {
                result = originalNtid;
            }

            return result;
        }

        #endregion
    }
}