namespace IdentitySwap
{
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.DirectoryServices.ActiveDirectory;
    using System.Linq;
    using System.Security.Principal;
	using System.Text;

	/// <summary>
	/// Class to help with Active Directory handling, for Identity Swapping
	/// </summary>
    public class ADClass
    {
        /// <summary>
        /// Path to the AD Server
        /// </summary>
        private string activeDirectoryPath { get; set; }

        /// <summary>
        /// Default Ctor
        /// </summary>
        /// <param name="path">Path to the directory</param>
        public ADClass(string path)
        {
            this.activeDirectoryPath = path;
        }

        /// <summary>
        /// Checks whether the user belongs to a group supplied
        /// </summary>
        /// <param name="userId">User Id with or without Domain</param>
        /// <param name="groupsToCheck">Groups to check the Ntid Against</param>
        /// <returns>True if the user belongs, false if s/he does not</returns>
        public bool DoesUserBelongToGroup(string userId, List<string> groupsToCheck)
        {
            bool result = false;

            try
            {
                userId = this.GetNtidFromDomainAndNtid(userId);

                List<GroupData> groupsFromAd = GetGroupsFromAd(userId);

                List<string> groupsForUser =
                    groupsFromAd.Select(x => x.DisplayName.ToLower()).Union(
                        groupsFromAd.Select(x => x.NtDomain.ToLower() + "\\" + x.DisplayName.ToLower())).Union(
                        groupsFromAd.Select(x => x.Ntid.ToLower())).Union(
                        groupsFromAd.Select(x => x.NtDomain.ToLower() + "\\" + x.Ntid.ToLower())).ToList();

                result = groupsToCheck.Any(group => groupsForUser.Contains(group.ToLower()));
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Gets domain of a user
        /// </summary>
        /// <param name="ntid">Group Name or Ntid</param>
        /// <returns>The NT Domain</returns>
        public string GetDomainName(string ntid)
        {
            string toReturn = string.Empty;

            string filterFormat = "(|(&(objectcategory={0})({1}={2})))";

            string objectCategory = "user";
            string propertyName = "samAccountName";

            using (DirectoryEntry domainConnection = new DirectoryEntry(this.activeDirectoryPath))
            {
                using (DirectorySearcher ds = new DirectorySearcher(domainConnection, string.Format(filterFormat, objectCategory, propertyName, ntid)))
                {
                    ds.PropertyNamesOnly = true;
                    SearchResult result = ds.FindOne();

                    if (result != null)
                    {
                        using (DirectoryEntry entry = result.GetDirectoryEntry())
                        {
                            if (entry.Properties["distinguishedName"] != null)
                            {
                                string domainName = entry.Properties["distinguishedName"].Value.ToString();
                                toReturn = this.GetDomainFromDistinguishedName(domainName);
                            }
                        }
                    }
                }
            }

            return toReturn;
        }

        #region Private Methods

        /// <summary>
        /// Gets NTID only, from a string containing (optionally) domain followed by \ followed by ntid
        /// </summary>
        /// <param name="ntidWithDomain">domain \ ntid</param>
        /// <returns>NTID without domain</returns>
        private string GetNtidFromDomainAndNtid(string ntidWithDomain)
        {
            string result = string.Empty;

            if (ntidWithDomain != null)
            {
                if (!ntidWithDomain.Contains("\\"))
                {
                    result = ntidWithDomain;
                }
                else
                {
                    string[] parts = ntidWithDomain.Split('\\');
                    if (parts != null && parts.Any())
                    {
                        result = parts.Last();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Connects to AD and gets the actual groups
        /// </summary>
        /// <param name="userId">User id without domain</param>
        /// <returns>Groups</returns>
        private List<GroupData> GetGroupsFromAd(string userId)
        {
            List<GroupData> groupsFromAd = new List<GroupData>();

            using (DirectoryEntry domainConnection = new DirectoryEntry(this.activeDirectoryPath))
            {
                domainConnection.AuthenticationType = AuthenticationTypes.Secure;

                // token Group searcher
                using (DirectorySearcher ds = new DirectorySearcher(domainConnection, string.Format("(&(objectClass=user)(sAMAccountName={0}))", userId)))
                {
					ds.PropertyNamesOnly = true;
					SearchResult samResult = ds.FindOne();

                    if (samResult != null)
                    {
                        DirectoryEntry theUser = samResult.GetDirectoryEntry();
                        theUser.RefreshCache(new string[] { "tokenGroups" });

						StringBuilder filterStringBuilder = new();

						// Just create a single LDAP query for all user SIDs
						filterStringBuilder.Append("(&(objectCategory=group)(|");
						foreach (byte[] resultBytes in theUser.Properties["tokenGroups"])
						{
							SecurityIdentifier sid = new(resultBytes, 0);
							filterStringBuilder.AppendFormat("({0}={1})", "objectSid", sid.Value);
						}

						filterStringBuilder.Append("))");

						using (DirectorySearcher sidSearcher = new(domainConnection, filterStringBuilder.ToString()))
						{
							sidSearcher.PropertiesToLoad.Add("sAMAccountName");
							sidSearcher.PropertiesToLoad.Add("distinguishedname");
							sidSearcher.PropertiesToLoad.Add("name");

							sidSearcher.PageSize = 1000; // Very important to have it here. Otherwise you'll get only 1000 at all. Please refer to DirectorySearcher documentation

							// We do not want to go beyond GC
							sidSearcher.ReferralChasing = ReferralChasingOption.None;

							SearchResultCollection results = sidSearcher.FindAll();

							foreach (SearchResult sidResult in results)
							{
								GroupData groupData = new()
								{
									DisplayName = (string)sidResult.Properties["name"][0],
									NtDomain = this.GetDomainFromDistinguishedName((string)sidResult.Properties["distinguishedname"][0]),
									Ntid = (string)sidResult.Properties["sAMAccountName"][0]
								};

								groupsFromAd.Add(groupData);
							}
						}
                    }
                }

                return groupsFromAd;
            }
        }

        /// <summary>
        /// Converts a distinguishedName in the form DC=acct02,DC=us,DC=lmco,DC=com or
        /// acct.us.lmco.com to acct02
        /// </summary>
        /// <param name="distinguishedName">The distinguished name</param>
        /// <returns>The fully qualilfied domain name</returns>
        private string GetDomainFromDistinguishedName(string distinguishedName)
        {
            if (distinguishedName == null)
            {
                throw new ArgumentNullException("distinguishedName");
            }

            string toReturn = distinguishedName;

            if (toReturn.Contains("DC="))
            {
                // Handle DC=acct02,DC=us,DC=lmco,DC=com format
                toReturn = toReturn.Substring(toReturn.IndexOf("DC=") + 3);
                toReturn = toReturn.Split(new[] { ",DC=" }, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            else if (toReturn.Contains("."))
            {
                // Handle acct0X.us.lmco.com format
                toReturn = toReturn.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
            }

            return toReturn;
        }

        #endregion
    }

    class GroupData
    {
        /// <summary>
        /// Gets or sets the ntid
        /// </summary>
        public string Ntid { get; set; }

        /// <summary>
        /// Gets or sets the NT Domain
        /// </summary>
        public string NtDomain { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public string DisplayName { get; set; }
    }
}