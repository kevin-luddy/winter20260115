// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.DAL.Common
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Test Security User Authorizations Data Loader
    /// </summary>
    [TestClass]
    public class SecurityUserAuthorizationsDataLoaderTest
    {
        /// <summary>
        /// Handle to test data.
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// AD utilities
        /// </summary>
        private Mock<IActiveDirectoryUtilities> adUtilities = null;

        /// <summary>
        /// Tests getPermissionsForUser method
        /// </summary>
        [TestMethod]
        public void GetPermissionsForUserTest()
        {
            var sut = this.CreateSystem();

            var proposal = this.testData.GetProposal();
            var user = this.testData.GetUser(true, new UserDTO()
            {
                Id = -1,
                Ntid = "securityusertest" + TestData.CreateRandomWord(3),
                EmailAddress = "test@ptm.ssc.lmco.com",
                Updateable = UpdateType.Upsert
            });

            var expectedResult = this.testData.GetProposalPermission(true, new ProposalPermissionDto()
            {
                ProposalID = proposal.Id,
                Role = PtmRole.Pricer,
                Id = -1,
                Updateable = UpdateType.Upsert,
                UserId = user.Id
            });

            var result = sut.GetPermissionsForUser(user);

            Assert.AreEqual(2, result.Count); // the one we set up, plus none.
            Assert.AreEqual(1, result.Count(x => x.ProposalID == expectedResult.ProposalID && x.AuthorizedRole == expectedResult.Role));
        }

        /// <summary>
        /// Tests getPermissionsForUser method with system permissions
        /// </summary>
        [TestMethod]
        public void GetSystemPermissionsForUserTest()
        {
            // delete all test users in DB, only run this locally every once in a while to clear out Database
            // this.testData.DeleteAllTestUsers();

            var sut = this.CreateSystem();

            var proposal = this.testData.GetProposal();
            var user = this.testData.GetUser(true, new UserDTO()
            {
                Id = -1,
                Ntid = "securityusertest" + TestData.CreateRandomWord(3),
                Updateable = UpdateType.Upsert
            });

            var expectedResult = this.testData.GetPermission(true, new SystemPermissionDto()
            {
                Id = -1,
                Role = PtmRole.Viewer,
                Updateable = UpdateType.Upsert,
                UserId = user.Id,
                LineOfBusinessIDs = new Collection<int>() { 10 }
            });

            var result = sut.GetPermissionsForUser(user);
            
            // won't know how many results, so just make sure specific proposal added during this test is included
            SecurityPermissionsResponse permission = result.FirstOrDefault(x => x.AuthorizedRole == expectedResult.Role && x.ProposalID == proposal.Id);
            Assert.IsNotNull(permission);
        }

        /// <summary>
        /// Creates system
        /// </summary>
        /// <returns>system under test</returns>
        private SecurityUserAuthorizationsDataLoader CreateSystem()
        {
            this.adUtilities = new Mock<IActiveDirectoryUtilities>();

            return new SecurityUserAuthorizationsDataLoader(this.adUtilities.Object);
        }
    }
}
