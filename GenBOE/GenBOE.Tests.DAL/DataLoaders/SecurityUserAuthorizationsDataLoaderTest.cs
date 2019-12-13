using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IES.Common;
using Moq;
using GenBOE.DataBridge.Common;
using System.Collections.ObjectModel;

namespace GenBOE.Tests.DAL.DataLoaders
{
    [TestClass]
    public class SecurityUserAuthorizationsDataLoaderTest
    {
        /// <summary>
        /// AD utilities
        /// </summary>
        private Mock<IActiveDirectoryUtilities> adUtilities = null;

        static private Collection<UserCreated> _Users = null;

        [ClassInitialize]
        /// Since this function is defined as ClassInitialize, it will be the first function
        /// called in this entire class.
        public static void GetCurrentWorkspace(TestContext testContext)
        {
            // create users, which will also create them in their 'potential' security roles
            GlobalTestCaseSetup.ResetGlobalWorkspaceID();
            GlobalTestCaseSetup.CreateGlobalBOE();
            _Users = GlobalTestCaseSetup.GetAllUsers();

            var authors = from u in _Users
                          where ((Role)(u.Role)).Equals(Role.Author)
                         select u;
            var approvers = from u in _Users
                            where ((Role)(u.Role)).Equals(Role.Approver)
                          select u;
            var reviewers = from u in _Users
                            where ((Role)(u.Role)).Equals(Role.WorkspaceReviewer)
                          select u;
            var sysadmin = from u in _Users
                           where ((Role)(u.Role)).Equals(Role.SystemAdmin)
                          select u;
            var workspaceadmin = from u in _Users
                                 where ((Role)(u.Role)).Equals(Role.WorkspaceAdmin)
                          select u;
            Assert.IsTrue(authors.Count() == 1, "Found " + authors.Count() + " instead of expected 1");
            Assert.IsTrue(approvers.Count() == 3, "Found " + approvers.Count() + " instead of expected 3");
            Assert.IsTrue(reviewers.Count() == 0, "Found " + reviewers.Count() + " instead of expected 0");

            Assert.IsTrue(sysadmin.Count() == 1, "Found " + sysadmin.Count() + " instead of expected 1");
            Assert.IsTrue(workspaceadmin.Count() == 1, "Found " + workspaceadmin.Count() + " instead of expected 1");
        }

        [TestMethod]
        public void GetPermissionsForCurrentlyLoggedInUserTest()
        {
            this.adUtilities = new Mock<IActiveDirectoryUtilities>();
            SecurityUserAuthorizationsDataLoader sut = new SecurityUserAuthorizationsDataLoader(this.adUtilities.Object);


            bool atLeastOneUserHasWorkspaceAdmin = false;
            bool atLeastOneUserHasSystemAdmin = false;
            int numOfAuthors = 0;
            int numOfApprovers = 0;
            int numOfReviewers = 0;

            foreach (UserCreated user in _Users)
            {
                var permissions = sut.GetPermissionsForUser(user.User.NTID);
                Assert.IsTrue(permissions.Count > 0);

                foreach (SecurityPermissionsResponse permission in permissions)
                {
                    atLeastOneUserHasSystemAdmin |= permission.AuthorizedRole.Equals(Role.SystemAdmin);
                    atLeastOneUserHasWorkspaceAdmin |= permission.AuthorizedRole.Equals(Role.WorkspaceAdmin);

                    if (permission.AuthorizedRole.Equals(Role.Author))
                    {
                        numOfAuthors++;
                    }
                    if (permission.AuthorizedRole.Equals(Role.Approver))
                    {
                        numOfApprovers++;
                    }
                    if (permission.AuthorizedRole.Equals(Role.WorkspaceReviewer))
                    {
                        numOfReviewers++;
                    }
                }
            }


            // reviewers are chopped off when a BOE is created ... and only 1 author can be present
            Assert.IsTrue(atLeastOneUserHasWorkspaceAdmin, "No user has WorkspaceAdmin permissions.");
            Assert.IsTrue(atLeastOneUserHasSystemAdmin, "No user has SystemAdmin permissions.");
            Assert.IsTrue(numOfAuthors == 1, "Not 1 users present in this role");
            Assert.IsTrue(numOfApprovers == 3, "Not 3 users present in this role");
            Assert.IsTrue(numOfReviewers == 0, "Not 0 users present in this role");
        }

        [TestMethod]
        public void GetPermissionsForUserThatDoesNotExistTest()
        {
            this.adUtilities = new Mock<IActiveDirectoryUtilities>();
            SecurityUserAuthorizationsDataLoader sut = new SecurityUserAuthorizationsDataLoader(this.adUtilities.Object);

            var permissions = sut.GetPermissionsForUser("DNE");
            Assert.IsTrue(permissions.Count == 1);
            Assert.IsTrue(permissions.ElementAt(0).AuthorizedRole == Role.None);
        }
    }
}
