// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.DirectoryServices;
    using System.Linq;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web.Configuration;
    using System.Xml.Linq;
    using IES.Common.Exceptions;

    /// <summary>
    /// Class to manage utility interactions with the AD
    /// </summary>
    public class ActiveDirectoryUtilities : IActiveDirectoryUtilities
    {
        /// <summary>
        /// declare private instance so we can cache returns from AD for a brief period of time
        /// </summary>
        private MemoryCache cache = new MemoryCache();

        /// <summary>
        /// The logger
        /// </summary>
        private Logger log = new Logger(typeof(ActiveDirectoryUtilities));

        /// <summary>
        /// The number of seconds to store in cache
        /// </summary>
        private int secondsToCache = 720;

        /// <summary>
        /// The max number of attempts to make to AD
        /// </summary>
        private const int MAX_AD_TRIES = 10;

        /// <summary>
        /// Active Directory path
        /// </summary>
        private readonly string activeDirectoryPath = ConfigurationUtilities.GetAppSetting("ActiveDirectoryPath");

        /// <summary>
        /// The number of seconds to wait until the DirectorySearcher reaches a timeout.  The default (-1) value will cause the Directory Searcher to use its default value.
        /// </summary>
        private int CLIENT_TIMEOUT_SECONDS = -1;

        /// <summary>
        /// Group account type
        /// </summary>
        private const string ACCOUNT_TYPE_GROUP = "Group";

        /// <summary>
        /// Default constructor
        /// </summary>
        public ActiveDirectoryUtilities()
        {
        }

        /// <summary>
        /// Constructor w/ seconds
        /// </summary>
        /// <param name="inSecondsToCache">The number of seconds to store a value in cache</param>
        public ActiveDirectoryUtilities(int inSecondsToCache)
        {
            this.secondsToCache = inSecondsToCache;
        }

        /// <summary>
        /// Constructor with timeoutSeconds specification.
        /// </summary>
        /// <param name="inSecondsToCache">The number of seconds to store a value in cache</param>
        /// <param name="clientTimeoutSeconds">The number of seconds to wait until the DirectorySearcher reaches a timeout on the client.</param>
        public ActiveDirectoryUtilities(int inSecondsToCache, int clientTimeoutSeconds)
        {
            this.secondsToCache = inSecondsToCache;
            this.CLIENT_TIMEOUT_SECONDS = clientTimeoutSeconds;
        }

        /// <summary>
        /// Get user information from their Ntid
        /// </summary>
        /// <param name="inNtid">user's Ntid</param>
        /// <param name="isGroup">is the thing that we are looking for a group?</param>
        /// <returns>user data regarding account, null if user not found</returns>
        public virtual UserData GetUserByQualifiedAccount(string inNtid, bool isGroup)
        {
            if (inNtid == null)
            {
                throw new ArgumentNullException(nameof(inNtid));
            }

            UserData currentAccount = null;

            string cacheKey = inNtid;

            if (this.cache.Contains(cacheKey))
            {
                object fromCache = this.cache.GetData(cacheKey);
                this.log.Debug("ADUTILS - UserData from temporary cache is " + fromCache.ToString());

                currentAccount = fromCache as UserData;
            }
            else
            {
                // Try to query AD to get the user data.
                int tries = 0;
                bool finished = false;
                while (++tries < MAX_AD_TRIES && !string.IsNullOrEmpty(inNtid))
                {
                    try
                    {
                        using (DirectoryEntry directoryEntry = new DirectoryEntry(this.activeDirectoryPath))
                        {
                            directoryEntry.AuthenticationType = AuthenticationTypes.Secure;

                            string filter = "(&(objectClass=user)(|(cn=" + inNtid + ")(sAMAccountName=" + inNtid + ")))";
                            if (isGroup)
                            {
                                filter = string.Format("(&(objectClass=group)(|(cn=" + inNtid + ")(dn=" + inNtid + ")(samAccountName=" + inNtid + ")))");
                            }

                            using (DirectorySearcher ds = new DirectorySearcher(directoryEntry, filter))
                            {
                                if (isGroup)
                                {
                                    ds.PropertiesToLoad.Add("sAMAccountName");
                                    ds.PropertiesToLoad.Add("name");
                                }
                                else
                                {
                                    ds.PropertiesToLoad.Add("sAMAccountName");
                                    ds.PropertiesToLoad.Add("givenname");
                                    ds.PropertiesToLoad.Add("sn");
                                    ds.PropertiesToLoad.Add("displayname");
                                    ds.PropertiesToLoad.Add("mail");
                                    ds.PropertiesToLoad.Add("telephonenumber");
                                    ds.PropertiesToLoad.Add("lmcUSAPersonIndicator");
                                    ds.PropertiesToLoad.Add("employeeType");
                                }

                                SearchResult searchResult = ds.FindOne();

                                if (searchResult == null)
                                {
                                    return null;
                                }
                                
                                DirectoryEntry user = searchResult.GetDirectoryEntry();

                                if (!user.Properties["sAMAccountName"][0].ToString().ToLower().Equals(inNtid.ToLower().Replace(" ", string.Empty)))
                                {
                                    // This is a weird case that came out of a bug 2935. In this case we were searching for a user with ntid "silva". There was another
                                    // account with ntid "silva$", which is what is returned by FindOne above. So if the NTIDs don't match, we do a longer search
                                    user = null;

                                    SearchResultCollection allMatches = ds.FindAll();

                                    foreach (SearchResult aSearchResult in allMatches)
                                    {
                                        DirectoryEntry aDirEntry = aSearchResult.GetDirectoryEntry();

                                        if (aDirEntry.Properties["sAMAccountName"][0].ToString().ToLower().Equals(inNtid.ToLower()))
                                        {
                                            user = aDirEntry;
                                            break;
                                        }
                                    }

                                    if (user == null)
                                    {
                                        return null;
                                    }
                                }

                                if (!isGroup)
                                {
                                    currentAccount = new UserData
                                    {
                                        FirstName = user.Properties.Contains("givenname") ? user.Properties["givenname"][0].ToString() : string.Empty,
                                        LastName = user.Properties.Contains("sn") ? user.Properties["sn"][0].ToString() : string.Empty,
                                        DisplayName = user.Properties.Contains("displayname") ? user.Properties["displayname"][0].ToString() : string.Empty,
                                        Email = user.Properties.Contains("mail") ? user.Properties["mail"][0].ToString().ToLower() : string.Empty,
                                        Ntid = inNtid,
                                        Phone = user.Properties.Contains("telephonenumber") ? user.Properties["telephonenumber"][0].ToString() : string.Empty,
                                        IsUsPerson = user.Properties.Contains("lmcUSAPersonIndicator") ? (bool?)(user.Properties["lmcUSAPersonIndicator"][0].ToString().ToUpper() == "Y") : null,
                                        IsSubcontractor = user.Properties.Contains("employeeType") ? (bool?)(user.Properties["employeeType"][0].ToString().ToUpper() != "E") : null
                                    };
                                }
                                else
                                {
                                    currentAccount = new UserData
                                    {
                                        DisplayName = user.Properties["name"][0].ToString(),
                                        Ntid = inNtid
                                    };
                                }

                                finished = true;
                            }
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        string message = "Retry " + tries + " of " + MAX_AD_TRIES + " attempts.  Issues communicating with AD.\r\n";
                        this.log.Error(e, message);

                        Thread.Sleep(500);
                    }

                    if (finished)
                    {
                        break; // don't retry .. we're finished
                    }
                }

                if (currentAccount != null)
                {
                    this.log.Debug("GET USER FROM AD : For user Ntid " + cacheKey + " caching their data for " + this.secondsToCache + " seconds [" + currentAccount + "]");
                    this.cache.Add(cacheKey, currentAccount, this.secondsToCache); // cache for this.secondsToCache seconds
                }
            }

            return currentAccount;
        }

        /// <summary>
        /// Returns a collection of groups for a user
        /// </summary>
        /// <param name="inNtid">user's ntid</param>
        /// <returns>collection of groups</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        public ICollection<GroupData> GetGroupsForUser(string inNtid)
        {
            if (string.IsNullOrEmpty(inNtid))
            {
                throw new ArgumentNullException(nameof(inNtid));
            }

            string cacheKey = inNtid + "-UsersGroups";

            object fromCache = this.cache.GetData(cacheKey);
            if (fromCache == null)
            {
                lock(string.Intern(cacheKey))
                {
                    if (!this.cache.Contains(cacheKey))
                    {
                        Dictionary<string, GroupData> distinctGroups = new Dictionary<string, GroupData>();

                        // ad connection
                        using (DirectoryEntry domainConnection = new DirectoryEntry(this.activeDirectoryPath))
                        {
                            domainConnection.AuthenticationType = AuthenticationTypes.Secure;

                            // token Group searcher
                            using (DirectorySearcher ds = new DirectorySearcher(domainConnection, string.Format("(&(objectClass=user)(samAccountName={0}))", inNtid)))
                            {
                                if (CLIENT_TIMEOUT_SECONDS > 0)
                                {
                                    ds.ClientTimeout = TimeSpan.FromSeconds(CLIENT_TIMEOUT_SECONDS);
                                }

                                ds.PropertyNamesOnly = true;
                                
                                SearchResult samResult = ds.FindOne();

                                if (samResult != null)
                                {
                                    DirectoryEntry theUser = samResult.GetDirectoryEntry();
                                    theUser.RefreshCache(new string[] { "tokenGroups" });

                                    StringBuilder filterStringBuilder = new StringBuilder();
                                        
                                    // Just create a single LDAP query for all user SIDs
                                    filterStringBuilder.Append("(&(objectCategory=group)(|");
                                    foreach (byte[] resultBytes in theUser.Properties["tokenGroups"])
                                    {
                                        SecurityIdentifier sid = new SecurityIdentifier(resultBytes, 0);
                                        filterStringBuilder.AppendFormat("({0}={1})", "objectSid", sid.Value);
                                    }

                                    filterStringBuilder.Append("))");

                                    using (DirectorySearcher searcher = new DirectorySearcher(domainConnection, filterStringBuilder.ToString()))
                                    {
                                        if (CLIENT_TIMEOUT_SECONDS > 0)
                                        {
                                            searcher.ClientTimeout = TimeSpan.FromSeconds(CLIENT_TIMEOUT_SECONDS);
                                        }

                                        searcher.PropertiesToLoad.Add("sAMAccountName");
                                        searcher.PropertiesToLoad.Add("distinguishedname");
                                        searcher.PropertiesToLoad.Add("name");

                                        searcher.PageSize = 1000; // Very important to have it here. Otherwise you'll get only 1000 at all. Please refer to DirectorySearcher documentation

                                        // We do not want to go beyond GC
                                        searcher.ReferralChasing = ReferralChasingOption.None;

                                        SearchResultCollection results = searcher.FindAll();

                                        foreach (SearchResult searchResult in results)
                                        {
											GroupData groupData = new GroupData()
                                            {
                                                DisplayName = searchResult.Properties["name"][0].ToString(),
                                                Ntid = searchResult.Properties["sAMAccountName"][0].ToString()
                                            };

                                            distinctGroups[groupData.Ntid] = groupData;
                                        }
                                    }
                                }
                            }
                        }

                        List<GroupData> toCache = distinctGroups.Values.ToList<GroupData>();

                        this.cache.Add(cacheKey, toCache, this.secondsToCache); // cache for this.secondsToCache seconds
                    }
                }
                fromCache = this.cache.GetData(cacheKey);
            }

            return fromCache as List<GroupData>;
        }

        /// <summary>
        /// Returns an XML string containing the user's NTID and all their AD group Ids.
        /// For example: &lt;ROOT&gt;&lt;id&gt;paliderd&lt;/id&gt;&lt;id&gt;ebs.estimationinitiative.devteam&lt;/id&gt;&lt;/ROOT&gt;
        /// </summary>
        /// <param name="ntid">user's ntid</param>
        /// <param name="groups">user's AD groups</param>
        /// <returns>XML string with user and group IDs</returns>
        public string GetUserAndGroupIdsAsXml(string ntid, ICollection<IES.Common.GroupData> groups)
        {
            List<string> groupNames = groups != null ? groups.Select(x => x.Ntid).ToList() : new List<string>();
            groupNames.Add(ntid);
            XElement xml = new XElement("ROOT");
            foreach (string groupName in groupNames)
            {
                xml.Add(new XElement("id", groupName));
            }

            return xml.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Searches AD for the object and returns the associated fully qualified distringuished name
        /// </summary>
        /// <param name="objectClass">Type of the AD object</param>
        /// <param name="returnType">Type of AD identifier to return</param>
        /// <param name="objectName">Name of object to lookup</param>
        /// <returns>Either the GUID or DN of the AD object</returns>
        private string GetObjectDistinguishedName(ObjectClass objectClass,
            ReturnType returnType, string objectName)
        {
            string distinguishedName = string.Empty;
            using (DirectoryEntry entry = new DirectoryEntry(this.activeDirectoryPath))
            {
                using (DirectorySearcher mySearcher = new DirectorySearcher(entry))
                {
                    if (CLIENT_TIMEOUT_SECONDS > 0)
                    {
                        mySearcher.ClientTimeout = TimeSpan.FromSeconds(CLIENT_TIMEOUT_SECONDS);
                    }

                    switch (objectClass)
                    {
                        case ObjectClass.user:
                            mySearcher.Filter = "(&(objectClass=user)(|(cn=" + objectName + ")(sAMAccountName=" + objectName + ")))";
                            // TODO This code is never used, if we do use it in the future then test out the performance fix below
                            // mySearcher.PropertiesToLoad.Add("sAMAccountName");
                            // mySearcher.PropertiesToLoad.Add("distinguishedName");
                            break;
                        case ObjectClass.group:
                            mySearcher.Filter = string.Format("(&(objectClass=group)(|(cn=" + objectName + ")(dn=" + objectName + ")(samAccountName=" + objectName + ")))");
                            mySearcher.PropertyNamesOnly = true;
                            break;
                        case ObjectClass.computer:
                            mySearcher.Filter = "(&(objectClass=computer)(|(cn=" + objectName + ")(dn=" + objectName + ")))";
                            // TODO This code is never used, if we do use it in the future then test out the performance fix below
                            // mySearcher.PropertyNamesOnly = true;
                            break;
                    }

                    SearchResult result = mySearcher.FindOne();

                    if (result == null)
                    {
                        Console.WriteLine("\nWe did not find the group " + objectName + ".");
                        throw new GeneralAppException("We did not find the group " + objectName + ".");
                    }

                    DirectoryEntry directoryObject = result.GetDirectoryEntry();
                    if (returnType.Equals(ReturnType.distinguishedName))
                    {
                        distinguishedName = "LDAP://" + directoryObject.Properties
                            ["distinguishedName"].Value;
                    }

                    if (returnType.Equals(ReturnType.ObjectGUID))
                    {
                        distinguishedName = directoryObject.Guid.ToString();
                    }
                }
            }

            return distinguishedName;
        }

        /// <summary>
        /// Derives account type from objectCategory value of the form "CN=Group,CN=Schema,CN=Configuration,DC=adroot,DC=lmco,DC=com"
        /// </summary>
        /// <param name="objectCategory">The object category value</param>
        /// <returns>Account type</returns>
        public string GetAccountTypeFromObjectCategory(string objectCategory)
        {
            if (objectCategory == null)
            {
                throw new ArgumentNullException(nameof(objectCategory));
            }

            string toReturn = objectCategory;

            if (toReturn.Contains("CN="))
            {
                toReturn = toReturn.Substring(toReturn.IndexOf("CN=") + 3);
                toReturn = toReturn.Split(new[] { ",CN=" }, StringSplitOptions.RemoveEmptyEntries)[0];
            }

            return toReturn;
        }

        /// <summary>
        /// Check to see if the AD group passed in is valid
        /// </summary>
        /// <param name="inGroupName">the group name</param>
        /// <returns>true/false depending on validity</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public bool IsValidADGroup(string inGroupName)
        {
            string cacheKey = inGroupName + "-IsValid";

            bool valid = false;

            if (this.cache.Contains(cacheKey))
            {
                return (bool)this.cache.GetData(cacheKey);
            }
            
            int tries = 0;
            bool finished = false;
            while (++tries < MAX_AD_TRIES)
            {
                try
                {
                    using (DirectoryEntry directoryEntry = new DirectoryEntry(this.activeDirectoryPath))
                    {
                        directoryEntry.AuthenticationType = AuthenticationTypes.Secure;

                        string filter = string.Format("(&(objectClass=group)(|(cn=" + inGroupName + ")(dn=" + inGroupName + ")(samAccountName=" + inGroupName + ")))");

                        using (DirectorySearcher ds = new DirectorySearcher(directoryEntry, filter))
                        {
                            SearchResult searchResult = ds.FindOne();

                            DirectoryEntry group = searchResult.GetDirectoryEntry();
                            valid = !string.IsNullOrEmpty(group.Properties["samAccountName"][0].ToString());
                            finished = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    string message = "Retry " + tries + " of " + MAX_AD_TRIES + " attempts.  Issues communicating with AD.\r\n";
                    this.log.Error(e, message);

                    Thread.Sleep(500);
                }

                if (finished)
                {
                    break; // don't retry
                }
            }

            this.cache.Add(cacheKey, valid, this.secondsToCache); // cache for this.secondsToCache seconds

            return valid;
        }

        /// <summary>
        /// Returns true if the user is a member of the specified AD group.
        /// </summary>
        /// <param name="inUserName">The user name</param>
        /// <param name="inGroupName">The group name</param>
        /// <returns></returns>
        public bool IsMemberOfADGroup(string inUserName, string inGroupName)
        {
            if (string.IsNullOrEmpty(inUserName))
            {
                throw new ArgumentNullException(nameof(inUserName));
            }

            if (string.IsNullOrEmpty(inGroupName))
            {
                throw new ArgumentNullException(nameof(inGroupName));
            }

            ICollection<GroupData> groupData = this.GetGroupsForUser(inUserName);

            if (groupData != null && groupData.Any(g => g.Ntid.ToLower() == inGroupName.Replace(" ", string.Empty).ToLower()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks multiple users against multiple groups, to see if the user is a member of the specified AD group.
        /// </summary>
        /// <param name="usersToCheck">All of the users to check</param>
        /// <param name="groupsToCheckAgainst">All of the groups to check</param>
        /// <returns>A mapping of users and whether they belong to at least 1 group</returns>
        public Dictionary<UserData, bool> CheckUsersBoeAccess(ICollection<UserData> usersToCheck, ICollection<GroupData> groupsToCheckAgainst)
        {
            if (usersToCheck == null) { throw new ArgumentNullException(nameof(usersToCheck)); }
            if (groupsToCheckAgainst == null) { throw new ArgumentNullException(nameof(groupsToCheckAgainst)); }

            // Dictionary<UserData, whetherOrNotUserIsAMemberOfAtLeastOneGroup>.
            Dictionary<UserData, bool> result = usersToCheck.ToList().ToDictionary(x => x, x => false);

            // For each group, check to see if the user is a member of it
            foreach (GroupData individualGroup in groupsToCheckAgainst)
            {
                try { 
                    ICollection<UserData> userNames = this.GetAdGroupUsers(individualGroup.Ntid);
                    usersToCheck.ToList().ForEach(x => { result[x] = result[x] || userNames.Any(z => x.Ntid == z.Ntid); });

                    // if we matched all usernames, we are done
                    if (result.All(x => x.Value)) { break; };
                } catch(GeneralAppException) { } // group does not exist, no harm done, just go to the next one
            }

            return result;
        }

        /// <summary>
        /// Get members of an AD group
        /// </summary>
        /// <param name="inGroupName">The group's name</param>
        /// <returns>Collection of UserData objects</returns>
        /// <exception cref="GeneralAppException">Thrown if group DNE</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public ICollection<UserData> GetAdGroupUsers(string inGroupName)
        {
            ICollection<UserData> userNames = new Collection<UserData>();

            string key = inGroupName + "-Members";
            if (this.cache.Contains(key))
            {
                userNames = this.cache.GetData(key) as ICollection<UserData>;
            }
            else
            {
                int tries = 0;
                bool groupNotFound = true;
                bool allUsersAdded = false;
                while (++tries < MAX_AD_TRIES)
                {
                    try
                    {

						string groupDN = this.GetObjectDistinguishedName(ObjectClass.group, ReturnType.distinguishedName, inGroupName);

                        groupNotFound = false;

                        using (DirectoryEntry root = new DirectoryEntry(this.activeDirectoryPath))
                        {
							string[] attributesToLoad = new[]
                                {
                                "displayname",
                                "distinguishedname",
                                "samaccountname",
                                "givenname",
                                "sn",
                                "mail",
                                "telephonenumber",
                                "objectClass",
                                "st",
                                "company",
                                "c",
                                "title",
                                "employeeID",
                                "lmcEmployeeID",
                                "lmcUSAPersonIndicator",
                                "employeeType"
                            };

							string distinguishedNameWithoutLDAPPrefix = groupDN.Remove(0, 7);
							string searchFilter = "(memberOf=" + distinguishedNameWithoutLDAPPrefix + ")";
                            using (DirectorySearcher searcher = new DirectorySearcher(root, searchFilter, attributesToLoad))
                            {
                                searcher.PageSize = 1000; // Very important to have it here. Otherwise you'll get only 1000 at all. Please refer to DirectorySearcher documentation

                                if (CLIENT_TIMEOUT_SECONDS > 0)
                                {
                                    searcher.ClientTimeout = TimeSpan.FromSeconds(CLIENT_TIMEOUT_SECONDS);
                                }

								SearchResultCollection results = searcher.FindAll();

                                userNames = (from SearchResult user in results
                                              select new UserData()
                                              {
                                                  DisplayName = user.Properties.Contains("displayname") ? user.Properties["displayname"][0].ToString() : string.Empty,
                                                  Ntid = user.Properties.Contains("samaccountname") ? user.Properties["samaccountname"][0].ToString() : string.Empty,
                                                  FirstName = user.Properties.Contains("givenname") ? user.Properties["givenname"][0].ToString() : string.Empty,
                                                  LastName = user.Properties.Contains("sn") ? user.Properties["sn"][0].ToString() : string.Empty,
                                                  Email = user.Properties.Contains("mail") ? user.Properties["mail"][0].ToString().ToLower() : string.Empty,
                                                  Phone = user.Properties.Contains("telephonenumber") ? user.Properties["telephonenumber"][0].ToString() : string.Empty,
                                                  IsGroup = user.Properties.Contains("objectClass") && user.Properties["objectClass"].Contains("group"),
                                                  State = user.Properties.Contains("st") ? user.Properties["st"][0].ToString() : string.Empty,
                                                  Company = user.Properties.Contains("company") ? user.Properties["company"][0].ToString() : string.Empty,
                                                  Country = user.Properties.Contains("c") ? user.Properties["c"][0].ToString() : string.Empty,
                                                  Title = user.Properties.Contains("title") ? user.Properties["title"][0].ToString() : string.Empty,
                                                  EmployeeId = user.Properties.Contains("lmcEmployeeID") ? user.Properties["lmcEmployeeID"][0].ToString() : string.Empty,
                                                  IsUsPerson = user.Properties.Contains("lmcUSAPersonIndicator") ? (bool?)(user.Properties["lmcUSAPersonIndicator"][0].ToString().ToUpper() == "Y") : null,
                                                  IsSubcontractor = user.Properties.Contains("employeeType") ? (bool?)(user.Properties["employeeType"][0].ToString().ToUpper() != "E") : null
                                              }).Where(u => !string.IsNullOrWhiteSpace(u.Ntid)).ToList();
                            }

                            allUsersAdded = true;
                        }
                    }
                    catch (Exception e)
                    {
                        // clear data and try again
                        userNames.Clear();

                        string message = "Retry " + tries + " of " + MAX_AD_TRIES + " attempts.  Issues communicating with AD.\r\n";
                        this.log.Error(e, message);

                        Thread.Sleep(500);
                    }

                    if (groupNotFound)
                    {
                        Console.WriteLine($"\nWe did not find the group {inGroupName}.");
                        throw new GeneralAppException($"We did not find the group {inGroupName}.");
                    }

                    if (allUsersAdded)
                    {
                        break; // don't attempt another retry
                    }
                }

                // add result back to cache
                if (userNames != null)
                {
                    // cache for this.secondsToCache seconds
                    this.cache.Add(key, userNames, this.secondsToCache);
                }
            }

            return userNames;
        }

        /// <summary>
        /// Extract the specified list of property names (for each user in the results collection) into a Dictionary.
        /// </summary>
        /// <param name="results">Collection of search results</param>
        /// <param name="propertyNames">List of property names to extract</param>
        /// <returns>Dictionary (indexed by user account name) of dictionaries (indexed by property name)</returns>
        private Dictionary<string, Dictionary<string, string>> GetActiveDirectoryProperties(SearchResultCollection results, string[] propertyNames)
        {
            Dictionary<string, Dictionary<string, string>> propertiesByUser = new Dictionary<string, Dictionary<string, string>>();

            foreach (SearchResult res in results)
            {
                Dictionary<string, string> properties = this.GetActiveDirectoryProperties(res, propertyNames);
                propertiesByUser[properties["samaccountname"]] = properties;
            }

            return propertiesByUser;
        }

        /// <summary>
        /// Extract the specified list of property names from the search results into a Dictionary.
        /// </summary>
        /// <param name="ad">Search results</param>
        /// <param name="propertyNames">List of property names to extract</param>
        /// <returns>Dictionary (indexed by property name)</returns>
        private Dictionary<string, string> GetActiveDirectoryProperties(SearchResult ad, string[] propertyNames)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            foreach (string propertyName in propertyNames)
            {
                ResultPropertyValueCollection rpv = ad.Properties[propertyName];
                if (rpv.Count > 0)
                {
                    object propertyValue = ad.Properties[propertyName][0];
                    properties[propertyName] = propertyValue.ToString();
                }
            }

            return properties;
        }

        /// <summary>
        /// Search Active Directory by name or account
        /// </summary>
        /// <param name="userSearchString">Search string - either the user's last name or his/her NT account name</param>
        /// <param name="searchBy">Search by last name or account</param>
        /// <param name="matchBy">Starts-with, exact match, or contains</param>
        /// <returns>Active Directory search results</returns>
        private SearchResultCollection FindMatchingUsers(string userSearchString, ActiveDirectorySearchBy searchBy, ActiveDirectoryMatchType matchBy,
            string[] propertiesToLoad)
        {
            if (string.IsNullOrEmpty(userSearchString))
            {
                return null;
            }
            else
            {
                using (DirectoryEntry activeDirectoryRoot = new DirectoryEntry(this.activeDirectoryPath))
                {
                    using (DirectorySearcher search = new DirectorySearcher(activeDirectoryRoot, "(objectCategory=person)", propertiesToLoad))
                    {
                        if (CLIENT_TIMEOUT_SECONDS > 0)
                        {
                            search.ClientTimeout = TimeSpan.FromSeconds(CLIENT_TIMEOUT_SECONDS);
                        }

                        // Modify userSearchString (if necessary) according to the appropriate ActiveDirectoryMatchType.
                        if (matchBy == ActiveDirectoryMatchType.StartsWith)
                        {
                            userSearchString = $"{userSearchString}*";
                        }
                        else if (matchBy == ActiveDirectoryMatchType.Contains)
                        {
                            userSearchString = $"*{userSearchString}*";
                        }

                        // Create the search filter.
                        if (searchBy == ActiveDirectorySearchBy.LastName)
                        {
                            search.Filter = $"(&(objectClass=user)(name={userSearchString}))";
                        }
                        else if (searchBy == ActiveDirectorySearchBy.Account)
                        {
                            search.Filter = $"(&(objectClass=user)(samAccountName={userSearchString}))";
                        }
                        
                        return search.FindAll();
                    }
                }
            }
        }

        /// <summary>
        /// Search Active Directory by group account
        /// </summary>
        /// <param name="groupSearchString">The group NT account name</param>
        /// <param name="matchBy">Starts-with or exact match</param>
        /// <returns>Active Directory search results</returns>
        private SearchResultCollection FindMatchingGroups(string groupSearchString, ActiveDirectoryMatchType matchBy)
        {
            if (string.IsNullOrEmpty(groupSearchString))
            {
                return null;
            }
            else
            {
                using (DirectoryEntry activeDirectoryRoot = new DirectoryEntry(this.activeDirectoryPath))
                {
                    using (DirectorySearcher search = new DirectorySearcher(activeDirectoryRoot, "(objectCategory=group)"))
                    {
                        // TODO This code is only used in PTM, if we do use it in the future in genBOE then test out the performance fix below
                        // search.PropertyNamesOnly = true;
                        if (CLIENT_TIMEOUT_SECONDS > 0)
                        {
                            search.ClientTimeout = TimeSpan.FromSeconds(CLIENT_TIMEOUT_SECONDS);
                        }

                        // Modify groupSearchString (if necessary) according to the appropriate ActiveDirectoryMatchType.
                        if (matchBy == ActiveDirectoryMatchType.StartsWith)
                        {
                            groupSearchString = $"{groupSearchString}*";
                        }
                        else if (matchBy == ActiveDirectoryMatchType.Contains)
                        {
                            groupSearchString = $"*{groupSearchString}*";
                        }

                        search.Filter = $"(&(objectClass=group)(|(cn={groupSearchString})(dn={groupSearchString})(samAccountName={groupSearchString})))";

                        return search.FindAll();
                    }
                }
            }
        }

        /// <summary>
        /// Search Active Directory by user last name and display the results to the Active Directory search results view
        /// </summary>
        /// <param name="userSearchString">Search string - either the user's last name or his/her NT account name</param>
        /// <param name="searchBy">Search by last name or account</param>
        /// <param name="matchBy">Starts-with, exact match, or contains</param>
        /// <returns>Active Directory search results</returns>
        public ICollection<UserData> SearchUsers(string userSearchString, ActiveDirectorySearchBy searchBy, ActiveDirectoryMatchType matchBy)
        {
            // Searching an Account (NTID) with a match type of 'Contains' will usually lead to a timeout and is not recommended.  We want to
            // prevent this issue from occurring in the first place.
            if (searchBy == ActiveDirectorySearchBy.Account && matchBy == ActiveDirectoryMatchType.Contains)
            {
                throw new ArgumentException("A 'Search By' of 'User account' may not be used with a 'Search Type' of 'Contains'.  Please choose a different combination.");
            }

            List<UserData> results = new List<UserData>();

            if (!string.IsNullOrEmpty(userSearchString))
            {
                string sanitizedString = SanitizeInput(userSearchString);

                string[] propertyNames = new string[]
                {
                    "distinguishedname",
                    "samaccountname",
                    "givenName",
                    "sn",
                    "name",
                    "telephonenumber",
                    "company",
                    "title",
                    "mail",
                    "st",
                    "c",
                    "lmcUSAPersonIndicator",
                    "employeeType"
                };

                SearchResultCollection searchResults = this.FindMatchingUsers(sanitizedString, searchBy, matchBy, propertyNames);

                if (searchResults != null && searchResults.Count > 0)
                { 
                    results = CreateUserDtosFromAdData(propertyNames, searchResults, true);
                }

                if (searchBy == ActiveDirectorySearchBy.Account)
                {
                    // only search groups if this is a general account-name search
                    
                    searchResults = this.FindMatchingGroups(sanitizedString, matchBy);

                    if (searchResults != null && searchResults.Count > 0)
                    {                     
                        results.AddRange(CreateUserDtosFromAdData(propertyNames, searchResults, false));
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Creates user data from the AD results
        /// </summary>
        /// <param name="propertyNames">property names</param>
        /// <param name="matchSet">match from AD</param>
        /// <param name="isUserNotGroup">is the AD record an individual user</param>
        /// <returns>Corresponding User Data</returns>
        private List<UserData> CreateUserDtosFromAdData(string[] propertyNames, SearchResultCollection matchSet, bool isUserNotGroup)
        {
            List<UserData> result = new List<UserData>();

            Dictionary<string, Dictionary<string, string>> propertiesByUser = this.GetActiveDirectoryProperties(matchSet, propertyNames);

            foreach (KeyValuePair<string, Dictionary<string, string>> userProperties in propertiesByUser)
            {
                string accountName = userProperties.Key;
                Dictionary<string, string> adproperties = userProperties.Value;

                // For users only, we remove periods from their Display Names
                string displayName = isUserNotGroup ? this.GetPropertyValue("name", adproperties).Replace(".", "") : this.GetPropertyValue("name", adproperties);

                result.Add(new UserData
                {
                    Ntid = accountName,
                    FirstName = this.GetPropertyValue("givenName", adproperties),
                    LastName = this.GetPropertyValue("sn", adproperties),
                    DisplayName = displayName,
                    Phone = this.GetPropertyValue("telephonenumber", adproperties),
                    Company = this.GetPropertyValue("company", adproperties),
                    Email = this.GetPropertyValue("mail", adproperties),
                    State = this.GetPropertyValue("st", adproperties),
                    Country = this.GetPropertyValue("c", adproperties),
                    Title = this.GetPropertyValue("title", adproperties),
                    IsUsPerson = this.GetPropertyValue("lmcUSAPersonIndicator", adproperties) == "Y",
                    IsSubcontractor = this.GetPropertyValue("employeeType", adproperties) != "E"
                });
            }

            return result;
        }

        /// <summary>
        /// Gets the authorization groups from web configuration.
        /// </summary>
        /// <returns>A list of AD groups from web.config that are authorized to use the application.</returns>
        public ICollection<GroupData> GetAuthorizationGroupsFromWebConfig()
        {
            ICollection<GroupData> groups = new List<GroupData>();

            AuthorizationSection section = (AuthorizationSection)WebConfigurationManager.GetSection("system.web/authorization");
            StringCollection approvedADGroups = section.Rules.OfType<AuthorizationRule>().Where(r => r.Action == AuthorizationRuleAction.Allow).Select(r => r.Roles).First();

            foreach (string group in approvedADGroups)
            {
                string[] splitGroup = group.Trim().Split('\\'); //split group into domain [0] and name [1] strings

                groups.Add(new GroupData() { Ntid = splitGroup[1] });
            }

            return groups;
        }

        /// <summary>
        /// Gets accounts that should not be updated during an AD sync
        /// </summary>
        /// <returns>List of NTIDs</returns>
        public ICollection<string> GetNoADSyncAccountsFromWebConfig()
        {
            List<string> result = new List<string>();
            string value = WebConfigurationManager.AppSettings["NoADSyncAccounts"];
            
            if (!string.IsNullOrEmpty(value))
            {
                result.AddRange(value.Split(',').Select(x => x.Trim()));
            }

            return result;
        }

        /// <summary>
        /// Extract the value of the given property
        /// </summary>
        /// <param name="inPropertyName">Property name</param>
        /// <param name="inProperties">List of property names/values</param>
        /// <returns>Value of the given property, or empty string if it does not exist</returns>
        private string GetPropertyValue(string inPropertyName, Dictionary<string, string> inProperties)
        {
            return inProperties.ContainsKey(inPropertyName) ? inProperties[inPropertyName] : string.Empty;
        }

        /// <summary>
        /// determine if this is a group or individual
        /// </summary>
        /// <param name="inNtID">group or user's ntID</param>
        /// <returns>true if group, false if not</returns>
        public bool IsGroup(string inNtID)
        {
            bool isGroup = false;

            string[] propertyNames = new string[]
            {
                "distinguishedname",
                "samaccountname",
                "givenName",
                "sn",
                "name",
                "objectCategory"
            };

            SearchResultCollection match = this.FindMatchingGroups(inNtID, ActiveDirectoryMatchType.Exact);

            Dictionary<string, Dictionary<string, string>> propertiesByUser = this.GetActiveDirectoryProperties(match, propertyNames);

            foreach (KeyValuePair<string, Dictionary<string, string>> userProperties in propertiesByUser)
            {
                Dictionary<string, string> adproperties = userProperties.Value;

                string objectCategory = this.GetPropertyValue("objectCategory", adproperties);
                string accountType = this.GetAccountTypeFromObjectCategory(objectCategory);
                bool isGroupAccount = accountType == ACCOUNT_TYPE_GROUP;

                if (isGroupAccount)
                {
                    isGroup = true;
                }
                else
                {
                    isGroup = false;
                }
            }

            return isGroup;
        }

        /// <summary>
        /// Sanitizes the input to make it safe for AD lookup calls
        /// </summary>
        /// <param name="unsafeInput">Unsafe input to check</param>
        /// <returns>A sanitized, safe output</returns>
        internal string SanitizeInput(string unsafeInput)
        {
            if (string.IsNullOrEmpty(unsafeInput)) { return string.Empty; }

            // besides letters and numbers, we also allow a space, a period, a comma, an appostrophy a dash and a slash (when domains are included)
            List<char> allowedSpecialChars = new List<char>() { ' ', '.', ',', '\'', '-', '\\' };

            string result = new String(unsafeInput.Where(x => Char.IsLetterOrDigit(x) || allowedSpecialChars.Contains(x)).ToArray());

            return result;
        }

        /// <summary>
        /// Used to indicate AD entry type
        /// </summary>
        private enum ObjectClass
        {
            /// <summary>
            /// AD user
            /// </summary>
            user,

            /// <summary>
            /// AD security group
            /// </summary>
            group,

            /// <summary>
            /// AD computer
            /// </summary>
            computer
        }

        /// <summary>
        /// Used to indicate type of 
        /// </summary>
        private enum ReturnType
        {
            /// <summary>
            /// Distinguished name of the AD object
            /// </summary>
            distinguishedName,

            /// <summary>
            /// GUID of the AD object
            /// </summary>
            ObjectGUID
        }
    }
}
