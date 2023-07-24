// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests Active Directory Utilities.
    /// </summary>
    [TestClass]
    public class ActiveDirectoryUtilitiesTest
    {
        /// <summary>
        /// The account to use for an Exact lookup.
        /// </summary>
        private const string EXACT_ACCOUNT = "ectl0401";

        /// <summary>
        /// The name to use for an Exact lookup.
        /// </summary>
        private const string EXACT_LAST_NAME = "Patton, Christopher R";

        /// <summary>
        /// The account to use for a StartsWith lookup.
        /// </summary>
        private const string STARTS_WITH_ACCOUNT = "pattonc";

        /// <summary>
        /// The name to use for a StartsWith lookup.
        /// </summary>
        private const string STARTS_WITH_LAST_NAME = "Patton, Chris";

        /// <summary>
        /// The account to use for a Contains lookup.
        /// </summary>
        private const string CONTAINS_ACCOUNT = "attoncr";

        /// <summary>
        /// The name to use for a Contains lookup.
        /// </summary>
        private const string CONTAINS_LAST_NAME = "atton, Christopher R";

        /// <summary>
        /// test searching for an ntid that exists in the AD
        /// </summary>
        [TestMethod]
        public void TestSearchFound()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);
            UserData userdata = sut.GetUserByQualifiedAccount("iestibco", false);
            Assert.IsNotNull(userdata);
            Assert.AreEqual(string.Empty, userdata.Email);
        }

        /// <summary>
        /// test searching for an ntid that does not exist in the AD
        /// </summary>
        [TestMethod]
        public void TestSearchNotFound()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);
            UserData userdata = sut.GetUserByQualifiedAccount("doesnotexist", false);
            Assert.IsNull(userdata);
        }

        /// <summary>
        /// test searching for an ntid with no domain
        /// </summary>
        [TestMethod]
        public void TestSearchFoundNoDomain()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);
            UserData userdata = sut.GetUserByQualifiedAccount("iestibco", false);
            Assert.IsNotNull(userdata);
            Assert.AreEqual(string.Empty, userdata.Email);
        }

        /// <summary>
        /// test searching for an ntid with no domain and watch the exception occur
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSearchFoundNoNTID()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);
            sut.GetUserByQualifiedAccount(null, false);
        }

        /// <summary>
        /// IsValidADGroupTest
        /// </summary>
        [TestMethod]
        public void IsValidADGroupTest()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

            Assert.IsTrue(sut.IsValidADGroup("EBS.EstimationInitiative.DevTeam"));
            Assert.IsFalse(sut.IsValidADGroup("doesnotexist"));
            Assert.IsFalse(sut.IsValidADGroup("does.not.exist"));
        }

        /// <summary>
        /// IsMemberOfADGroupTest
        /// </summary>
        [TestMethod]
        public void IsMemberOfADGroupTest()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

			Assert.IsTrue(sut.IsMemberOfADGroup("paliderd", "EBS.EstimationInitiative.DevTeam"));
            Assert.IsFalse(sut.IsMemberOfADGroup("a-wilsot", "all.lmco.us.nonemp"));
        }

        /// <summary>
        /// CheckUsersBoeAccess_Exception1
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CheckUsersBoeAccess_Exception1()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

            sut.CheckUsersBoeAccess(null, new List<GroupData>());
        }

        /// <summary>
        /// CheckUsersBoeAccess_Exception2
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CheckUsersBoeAccess_Exception2()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

            sut.CheckUsersBoeAccess(new List<UserData>(), null);
        }

        /// <summary>
        /// CheckUsersBoeAccess_TestWithData
        /// </summary>
        [TestMethod]
        public void CheckUsersBoeAccess_TestWithData()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

            List<UserData> users = new List<UserData>()
            {
                new UserData() { Ntid = "paliderd" },
                new UserData() { Ntid = "rokey" }
            };

            List<GroupData> groups = new List<GroupData>()
            {
                new GroupData() { Ntid = "EBS.EstimationInitiative.DevTeam" }
            };

            Dictionary<UserData, bool> result = sut.CheckUsersBoeAccess(users, groups);

            Assert.IsTrue(result[users[0]]);
            Assert.IsFalse(result[users[1]]);
        }

        /// <summary>
        /// Tests w/ a bad group, to make sure an exception is not thrown
        /// </summary>
        [TestMethod]
        public void CheckUsersBoeAccess_BadGroupData()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

            List<UserData> users = new List<UserData>()
            {
                new UserData() { Ntid = "paliderd" }
            };

            List<GroupData> groups = new List<GroupData>()
            {
                new GroupData() { Ntid = "EBS.EstimationInitiative.DevTeam.THIS_GROUP_SHOULD_NOT_EXIST" }
            };

            Dictionary<UserData, bool> result = sut.CheckUsersBoeAccess(users, groups);

            Assert.IsFalse(result[users[0]]);
        }

        /// <summary>
        /// GetADGroupUsersTest
        /// </summary>
        [TestMethod]
        public void GetADGroupUsersTest()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

            var result = sut.GetAdGroupUsers("EBS.EstimationInitiative.DevTeam");

            Assert.IsTrue(result.Count > 0);
            Assert.IsNotNull(result.Select(x => x.Ntid == "a-wilsot"));
        }

		/// <summary>
        /// Ensures that users can be found by an Exact search and that using the partial search terms (missing beginning or end) with the Exact search does not work.
        /// </summary>
        [TestMethod]
        public void TestSearchUsersExact()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

            // Find Exact matches.
            ICollection<UserData> userdata = sut.SearchUsers(EXACT_ACCOUNT, ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Exact);
            Assert.IsNotNull(userdata, "Missing exact match on Account.");
            foreach (UserData user in userdata)
            {
                Assert.AreEqual(string.Empty, user.Email);
            }

            userdata = sut.SearchUsers(EXACT_LAST_NAME, ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.Exact);
            Assert.AreNotEqual(0, userdata.Count, "Missing exact match on LastName.");

            // Ensure partial matches fail.
            userdata = sut.SearchUsers(STARTS_WITH_LAST_NAME, ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.Exact);
            Assert.AreEqual(0, userdata.Count, "Found partial match on LastName (should only work for StartsWith or Contains).");

            userdata = sut.SearchUsers(STARTS_WITH_ACCOUNT, ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Exact);
            Assert.AreEqual(0, userdata.Count, "Found partial match on Account (should only work for StartsWith or Contains).");

            userdata = sut.SearchUsers(CONTAINS_LAST_NAME, ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.Exact);
            Assert.AreEqual(0, userdata.Count, "Found partial match on LastName (should only work for Contains).");

            userdata = sut.SearchUsers(CONTAINS_ACCOUNT, ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Exact);
            Assert.AreEqual(0, userdata.Count, "Found partial match on Account (should only work for Contains).");
        }

        /// <summary>
        /// Ensures that users can be found by a StartsWith search and that using the partial search term (missing beginning) with the StartsWith search does not work.
        /// </summary>
        [TestMethod]
        public void TestSearchUsersStartsWith()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

            // Find StartsWith matches.
            ICollection<UserData> userdata = sut.SearchUsers(STARTS_WITH_LAST_NAME, ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.StartsWith);
            Assert.AreNotEqual(0, userdata.Count, "Missing StartsWith match on LastName.");

            userdata = sut.SearchUsers(STARTS_WITH_ACCOUNT, ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.StartsWith);
            Assert.AreNotEqual(0, userdata.Count, "Missing StartsWith match on Account.");

            // Ensure partial matches fail.
            userdata = sut.SearchUsers(CONTAINS_LAST_NAME, ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.StartsWith);
            Assert.AreEqual(0, userdata.Count, "Found partial match on LastName (should only work for Contains).");

            userdata = sut.SearchUsers(CONTAINS_ACCOUNT, ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.StartsWith);
            Assert.AreEqual(0, userdata.Count, "Found partial match on Account (should only work for Contains).");
        }

        /// <summary>
        /// Ensures that users can be found by a Contains search.
        /// </summary>
        [TestMethod]
        public void TestSearchUsersContains()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30, 300);

            // Find Contains matches.
            ICollection<UserData> userdata = sut.SearchUsers(CONTAINS_LAST_NAME, ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.Contains);
            Assert.AreNotEqual(0, userdata.Count, "Missing Contains match on LastName.");

            // An exception should be thrown as 'Account' and 'Contains' should not be a valid parameter combination due to timeouts.
            try
            {
                userdata = sut.SearchUsers(CONTAINS_ACCOUNT, ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Contains);
                Assert.Fail("An exception should have been thrown.  'Account' and 'Contains' are not a valid combination of parameters.");
            }
            catch (ArgumentException)
            {
                // Do nothing, the proper exception was thrown.
            }
        }

        /// <summary>
        /// TestMany
        /// </summary>
        [TestMethod]
        public void TestMany()
        {
            ActiveDirectoryUtilities activeDirectory = new ActiveDirectoryUtilities(0);

            UserData user;
            ICollection<GroupData> groups;
            bool isMember;
            bool isValid;

            user = activeDirectory.GetUserByQualifiedAccount("paliderd", false);
            Assert.AreEqual("paliderd", user.Ntid);

            user = activeDirectory.GetUserByQualifiedAccount("EBS.EstimationInitiative.DevTeam", true);
            Assert.AreEqual("EBS.EstimationInitiative.DevTeam", user.Ntid, true);


            isMember = activeDirectory.IsMemberOfADGroup("paliderd", "EBS.EstimationInitiative.DevTeam");
            Assert.IsTrue(isMember);


            groups = activeDirectory.GetGroupsForUser("paliderd");
            Assert.IsTrue(groups.Any());

            groups = activeDirectory.GetGroupsForUser("paliderdfake");
            Assert.IsTrue(!groups.Any());

            isValid = activeDirectory.IsValidADGroup("EBS.EstimationInitiative.DevTeam");
            Assert.IsTrue(isValid);

            isValid = activeDirectory.IsValidADGroup("EO.Test Space X Group");
            Assert.IsFalse(isValid);
        }

        /// <summary>
        /// TestSearchUsers_InjectionCheck
        /// </summary>
        [TestMethod]
        public void TestSearchUsers_InjectionCheck()
        {
            // test searching for an ntid that exists in the AD
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(0, 300);
            ICollection<UserData> userdata = null;

            // Tests the lookup with just a *
            userdata = sut.SearchUsers("*", ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Exact);
            Assert.IsFalse(userdata.Any()); // PROBLEM

            userdata = sut.SearchUsers("*", ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.Exact);
            Assert.IsFalse(userdata.Any()); // PROBLEM

            // Tests the lookup by groups (by account)
            userdata = sut.SearchUsers("xxxx)(sAMAccountName=*", ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Exact);
            Assert.IsFalse(userdata.Any()); // PROBLEM

            userdata = sut.SearchUsers("xxxx)(sAMAccountName=", ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.StartsWith);
            Assert.IsFalse(userdata.Any()); // PROBLEM

            // An exception should be thrown as 'Account' and 'Contains' should not be a valid parameter combination due to timeouts.
            try
            {
                userdata = sut.SearchUsers("xxxx)(sAMAccountName=", ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Contains);
                Assert.Fail("An exception should have been thrown.  'Account' and 'Contains' are not a valid combination of parameters.");
            }
            catch (ArgumentException)
            {
                // Do nothing, the proper exception was thrown.
            }

            // Tests the lookup by account
            userdata = sut.SearchUsers(")(sAMAccountName=*", ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Exact);
            Assert.IsFalse(userdata.Any()); // PROBLEM

            userdata = sut.SearchUsers(")(sAMAccountName=", ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.StartsWith);
            Assert.IsFalse(userdata.Any()); // PROBLEM

            // An exception should be thrown as 'Account' and 'Contains' should not be a valid parameter combination due to timeouts.
            try
            {
                userdata = sut.SearchUsers(")(sAMAccountName=", ActiveDirectorySearchBy.Account, ActiveDirectoryMatchType.Contains);
                Assert.Fail("An exception should have been thrown.  'Account' and 'Contains' are not a valid combination of parameters.");
            }
            catch (ArgumentException)
            {
                // Do nothing, the proper exception was thrown.
            }

            // Tests the lookup by last name
            userdata = sut.SearchUsers("*)(name=*", ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.Exact);
            Assert.IsFalse(userdata.Any()); // PROBLEM

            userdata = sut.SearchUsers("*)(name=", ActiveDirectorySearchBy.LastName, ActiveDirectoryMatchType.StartsWith);
            // Filter out the "NAME-CHANGED" result that's in the AD - sanitized text will be "name", which returns this result
            Assert.IsFalse(userdata.Any(x => x.DisplayName != "NAME-CHANGED")); // PROBLEM

            // Cannot run an ActiveDirectoryMatchType.Contains test for this scenario because the sanitized text will be 'name', which is contained
            // in the name field for many actual users (e.g. McNamee, Swinamer, VanName, etc.).
        }

        /// <summary>
        /// TestSanitizeInput
        /// </summary>
        [TestMethod]
        public void TestSanitizeInput()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(0);

            // empty/null input
            Assert.AreEqual(string.Empty, sut.SanitizeInput(null));
            Assert.AreEqual(string.Empty, sut.SanitizeInput(string.Empty));
            Assert.AreEqual(string.Empty, sut.SanitizeInput("*"));

            // basic tests to verify things come back in the right order
            Assert.AreEqual("ab", sut.SanitizeInput("ab*"));
            Assert.AreEqual("ab", sut.SanitizeInput("*ab"));
            Assert.AreEqual("ab", sut.SanitizeInput("a*b"));
            Assert.AreEqual("ab", sut.SanitizeInput("**a****b****"));

            // preventing an attack
            Assert.AreEqual("xxxxsAMAccountName", sut.SanitizeInput("xxxx)(sAMAccountName=*"));

            // all allowed chars
            Assert.AreEqual("1234567890qwertyuioplkjhgfdsazxcvbnmQWERTYUIOPLKJHGFDSAZXCVBNM .,'-\\", sut.SanitizeInput("1234567890qwertyuioplkjhgfdsazxcvbnmQWERTYUIOPLKJHGFDSAZXCVBNM .,'-\\"));

            // a bunch of bad chars
            Assert.AreEqual(string.Empty, sut.SanitizeInput("\"{}:;[]+=_)(*&^%$#@!~`<>?/"));

            // more real world..
            Assert.AreEqual("Palider, Dusan", sut.SanitizeInput("Palider, Dusan"));
            Assert.AreEqual("Dusan O'Palider, Jr.", sut.SanitizeInput("Dusan O'Palider, Jr."));
            Assert.AreEqual("Dusan O'Palider, 3rd.", sut.SanitizeInput("Dusan O'Palider, 3rd."));

            Assert.AreEqual("US\\lmco.all.emp", sut.SanitizeInput("US\\lmco.all.emp"));
            Assert.AreEqual("ipe.genBOE.approvedsubcontractors", sut.SanitizeInput("ipe.genBOE.approvedsubcontractors"));
        }

        /// <summary>
        /// Tests group lookup
        /// </summary>
        [TestMethod]
        public void TestSearchGroupValid()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

            UserData groupInfo = sut.GetUserByQualifiedAccount("EBS.EstimationInitiative.DevTeam", true);

            Assert.IsNotNull(groupInfo);
            Assert.IsNotNull(groupInfo.DisplayName);
            Assert.IsNotNull(groupInfo.Ntid);
            Assert.AreEqual(groupInfo.DisplayName.ToLower(), groupInfo.Ntid.ToLower());
        }

        /// <summary>
        /// Tests group lookup, with a wrong group name
        /// </summary>
        [TestMethod]
        public void TestSearchGroupInvalid()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

            UserData groupInfo = sut.GetUserByQualifiedAccount("blahblah", true);

            Assert.IsNull(groupInfo);
        }

        /// <summary>
        /// Test if passed in nt id is related to a group or a person
        /// </summary>
        [TestMethod]
        public void TestIsGroup()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);
            string groupName = "EBS.EstimationInitiative.DevTeam";
            bool isGroup = sut.IsGroup(groupName);
            Assert.IsTrue(isGroup, "This is not a group");

            groupName = "goodwik1";
            isGroup = sut.IsGroup(groupName);
            Assert.IsFalse(isGroup, "This is not a user");
        }

        /// <summary>
        /// Test GetUserAndGroupIdsAsXml method.
        /// </summary>
        [TestMethod]
        public void TestGetUserAndGroupIdsAsXml()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

            string ntid = "ejfudd";
            ICollection<IES.Common.GroupData> groups = null;
            Assert.AreEqual("<ROOT><id>ejfudd</id></ROOT>", sut.GetUserAndGroupIdsAsXml(ntid, groups));

            groups = new Collection<IES.Common.GroupData> { };
            Assert.AreEqual("<ROOT><id>ejfudd</id></ROOT>", sut.GetUserAndGroupIdsAsXml(ntid, groups));

            groups = new Collection<IES.Common.GroupData> {
                new GroupData { Ntid = "warner.bros", DisplayName = "Warner Brothers" }
            };
            Assert.AreEqual("<ROOT><id>warner.bros</id><id>ejfudd</id></ROOT>", sut.GetUserAndGroupIdsAsXml(ntid, groups));

            groups = new Collection<IES.Common.GroupData> {
                new GroupData { Ntid = "warner.bros", DisplayName = "Warner Brothers" },
                new GroupData { Ntid = "looney.toons", DisplayName = "Looney Toons" }
            };
            Assert.AreEqual("<ROOT><id>warner.bros</id><id>looney.toons</id><id>ejfudd</id></ROOT>", sut.GetUserAndGroupIdsAsXml(ntid, groups));
        }

        #region Exception Tests

        /// <summary>
        /// IsMemberOfADGroup_EmptyUserName
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsMemberOfADGroup_EmptyUserName()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);
            sut.IsMemberOfADGroup(string.Empty, "EBS.EstimationInitiative.DevTeam"); // empty user name
        }

        /// <summary>
        /// IsMemberOfADGroup_EmptyGroupName
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsMemberOfADGroup_EmptyGroupName()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);
            sut.IsMemberOfADGroup("a-wilsot", string.Empty); // empty group name
        }

        /// <summary>
        /// GetAccountTypeFromObjectCategory with exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetAccountTypeFromObjectCategory_ExceptionTest()
        {
            ActiveDirectoryUtilities sut = new ActiveDirectoryUtilities(30);

            sut.GetAccountTypeFromObjectCategory(null);
        }

        #endregion Exception Tests
    }
}
