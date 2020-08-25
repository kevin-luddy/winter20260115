// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Tests.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.ActionLogic.ControllerLogic;
    using IES.ActionLogic.Mediator;
    using IES.Common;
    using IES.Common.classes;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class VersionControllerLogicTest
    {
        /// <summary>
        /// Test User Data
        /// </summary>
        private UserData userData = new UserData() { DisplayName = "Test User", Ntid = "moqdomain" };

        /// <summary>
        /// Mock Revision Mediator
        /// </summary>
        private Mock<IRevisionMediator> revisionMediator = new Mock<IRevisionMediator>();

        /// <summary>
        /// The area locking loader
        /// </summary>
        private Mock<IAreaLockingLoader> areaLockingLoader = new Mock<IAreaLockingLoader>();

        /// <summary>
        /// Mock Rate Detail Loader.
        /// </summary>
        private Mock<IRateDetailLoader> rateDetailLoader = new Mock<IRateDetailLoader>();
        
        /// <summary>
        /// Mock Security Information
        /// </summary>
        private Mock<ISecurityInformation> securityInformation = new Mock<ISecurityInformation>();

        /// <summary>
        /// AD Utils
        /// </summary>
        private Mock<ActiveDirectoryUtilities> adUtils = new Mock<ActiveDirectoryUtilities>();

        /// <summary>
        /// Revision data for Rev 121.
        /// </summary>
        private RevisionModelView rev121 = new RevisionModelView()
        {
            Id = 1,
            Revision = "121",
            History = "Test Rev 121",
            CreatedBy = "brunworg",
            DateCreated = DateTime.Now,
            StartYear = 2002,
            EndYear = 2040,
            DatePublished = DateTime.Now,
            PublishedBy = "brunworg",
            UpdateDate = DateTime.Now,
            ReleaseNotes = "Test Notes 121"
        };

        /// <summary>
        /// Revision data for Rev 122.
        /// </summary>
        private RevisionModelView rev122 = new RevisionModelView()
        {
            Id = 2,
            Revision = "122",
            History = "Test Rev 122",
            CreatedBy = "brunworg",
            DateCreated = DateTime.Now,
            StartYear = 2002,
            EndYear = 2040,
            DatePublished = DateTime.Now,
            PublishedBy = "brunworg",
            UpdateDate = DateTime.Now,
            ReleaseNotes = "Test Notes 122"
        };

        /// <summary>
        /// Revision data for Rev 123.
        /// </summary>
        private RevisionModelView rev123 = new RevisionModelView()
        {
            Id = 3,
            Revision = "123",
            History = "Test Rev 123",
            CreatedBy = "brunworg",
            DateCreated = DateTime.Now,
            StartYear = 2002,
            EndYear = 2040,
            UpdateDate = DateTime.Now,
            ReleaseNotes = "Test Notes 123"
        };   // WIP Revision

        /// <summary>
        /// Revision Option for Rev 121.
        /// </summary>
        private RevisionOptionModelView rev121Option = new RevisionOptionModelView()
            { Id = 1, Label = "121", Revision = "121", StartYear = 2002, EndYear = 2040 };

        /// <summary>
        /// Revision Option for Rev 122.
        /// </summary>
        private RevisionOptionModelView rev122Option = new RevisionOptionModelView()
            { Id = 2, Label = "122", Revision = "122", StartYear = 2002, EndYear = 2040 };

        /// <summary>
        /// Revision Option for Rev 123.
        /// </summary>
        private RevisionOptionModelView rev123Option = new RevisionOptionModelView()
            { Id = 3, Label = CommonConstants.WorkInProgress, Revision = "123", StartYear = 2002, EndYear = 2040 };

        /// <summary>
        /// Create System Under Test
        /// </summary>
        /// <returns></returns>
        public VersionControllerLogic CreateSut()
        {
            this.revisionMediator.Setup(x => x.GetById(It.IsAny<int>())).Returns(new RevisionModelView());
            this.revisionMediator.Setup(x => x.GetAll()).Returns(new List<RevisionModelView> { new RevisionModelView() });
            this.revisionMediator.Setup(x => x.GetWipRevision()).Returns(rev123);

            return new VersionControllerLogic(this.rateDetailLoader.Object, this.areaLockingLoader.Object, this.revisionMediator.Object, this.adUtils.Object, this.securityInformation.Object, null);
        }

        /// <summary>
        /// Test GetVersionDifferences exception (for null revisions list)
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetVersionDifferences_NullRevisions()
        {
            var sut = this.CreateSut();
            this.SetupIsRdmAdminUser(true);
            this.SetupNullRevisions();
            sut.GetVersionDifferences(9999, false, userData);
        }

        /// <summary>
        /// Test GetVersionDifferences exception (for empty revisions list)
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetVersionDifferences_EmptyRevisions()
        {
            var sut = this.CreateSut();
            this.SetupIsRdmAdminUser(true);
            this.SetupEmptyRevisions();
            VersionComparisonModelView mv = sut.GetVersionDifferences(9999, false, userData);
        }

        /// <summary>
        /// Test GetVersionDifferences Invalid id
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestGetVersionDifferences_InvalidId()
        {
            var sut = this.CreateSut();
            bool isRdmAdmin = true;
            this.SetupIsRdmAdminUser(isRdmAdmin);
            this.Setup3Revisions();
            sut.GetVersionDifferences(9999, isRdmAdmin, userData);
        }

        /// <summary>
        /// Test GetVersionDifferences with 1 revision (test admin access)
        /// 
        /// Purpose of this test is to verify that an exception is NOT thrown
        /// </summary>
        [TestMethod]
        public void TestGetVersionDifferences_Admin1Revision_NoEX()
        {
            var sut = this.CreateSut();
            bool isRdmAdmin = true;
            this.SetupIsRdmAdminUser(isRdmAdmin);
            this.Setup1Revision();

            Assert.IsNotNull(sut.GetVersionDifferences(3, isRdmAdmin, userData));
        }

        /// <summary>
        /// Test GetVersionDifferences with 1 revision (test non-admin access)
        /// 
        /// Purpose of this test is to verify that an exception is NOT thrown
        /// </summary>
        [TestMethod]
        public void TestGetVersionDifferences_NonAdmin1Revision_NoEX()
        {
            var sut = this.CreateSut();
            bool isRdmAdmin = false;
            this.SetupIsRdmAdminUser(isRdmAdmin);
            this.Setup1Revision();

            Assert.IsNotNull(sut.GetVersionDifferences(3, isRdmAdmin, userData));
        }

        /// <summary>
        /// Test GetVersionDifferences with 2 revisions (test admin access)
        /// </summary>
        [TestMethod]
        public void TestGetVersionDifferences_Admin2Revisions()
        {
            var sut = this.CreateSut();
            bool isRdmAdmin = true;
            this.SetupIsRdmAdminUser(isRdmAdmin);
            this.Setup2Revisions();
            VersionComparisonModelView mv = sut.GetVersionDifferences(3, isRdmAdmin, userData);
            Assert.AreEqual(isRdmAdmin, mv.AdminUser);
            Assert.AreEqual(false, mv.IsEarliestVersion);
            Assert.AreEqual(2, mv.AvailableVersions.Count);
            Assert.IsNull(mv.PPRDDifferences);
            Assert.AreSame(this.rev123Option, mv.SelectedRevision);
            Assert.AreEqual(123, mv.SelectedVersionNumber);
            Assert.AreEqual(122, mv.PreviousVersionNumber);
            Assert.AreEqual(CommonConstants.WorkInProgress, mv.SelectedVersionNumberDisplay);
            Assert.AreEqual($"Version 122", mv.PreviousVersionNumberDisplay);

            // Test id = -1 (RDM admin - should return WIP revision)
            mv = sut.GetVersionDifferences(-1, isRdmAdmin, userData);
            Assert.AreEqual(isRdmAdmin, mv.AdminUser);
            Assert.AreEqual(false, mv.IsEarliestVersion);
            Assert.AreEqual(2, mv.AvailableVersions.Count);
            Assert.IsNull(mv.PPRDDifferences);
            Assert.AreSame(this.rev123Option, mv.SelectedRevision);
            Assert.AreEqual(123, mv.SelectedVersionNumber);
            Assert.AreEqual(122, mv.PreviousVersionNumber);
            Assert.AreEqual(CommonConstants.WorkInProgress, mv.SelectedVersionNumberDisplay);
            Assert.AreEqual($"Version 122", mv.PreviousVersionNumberDisplay);

            // Test id = 2 (RDM admin - should return earliest revision)
            mv = sut.GetVersionDifferences(2, isRdmAdmin, userData);
            Assert.AreEqual(isRdmAdmin, mv.AdminUser);
            Assert.AreEqual(true, mv.IsEarliestVersion);
        }

        /// <summary>
        /// Test GetVersionDifferences with 2 revision (test non-admin access)
        /// 
        /// Purpose of this test is to verify that an exception is NOT thrown
        /// </summary>
        [TestMethod]
        public void TestGetVersionDifferences_NonAdmin2Revision_NoEX()
        {
            var sut = this.CreateSut();
            bool isRdmAdmin = false;
            this.SetupIsRdmAdminUser(isRdmAdmin);
            this.Setup2Revisions();

            Assert.IsNotNull(sut.GetVersionDifferences(3, isRdmAdmin, userData));
        }

        /// <summary>
        /// Test GetVersionDifferences with 3 revisions (test admin and non-admin access)
        /// </summary>
        [TestMethod]
        public void TestGetVersionDifferences_3Revisions()
        {
            var sut = this.CreateSut();
            bool isRdmAdmin = true;
            this.SetupIsRdmAdminUser(isRdmAdmin);
            this.Setup3Revisions();
            VersionComparisonModelView mv = sut.GetVersionDifferences(3, isRdmAdmin, userData);
            Assert.AreEqual(this.rev123.History, mv.WorkInProgressHistory);
            Assert.AreEqual(isRdmAdmin, mv.AdminUser);
            Assert.AreEqual(false, mv.IsEarliestVersion);
            Assert.AreEqual(3, mv.AvailableVersions.Count);
            Assert.IsNull(mv.PPRDDifferences);
            Assert.AreSame(this.rev123Option, mv.SelectedRevision);
            Assert.AreEqual(123, mv.SelectedVersionNumber);
            Assert.AreEqual(122, mv.PreviousVersionNumber);
            Assert.AreEqual(CommonConstants.WorkInProgress, mv.SelectedVersionNumberDisplay);
            Assert.AreEqual($"Version 122", mv.PreviousVersionNumberDisplay);

            isRdmAdmin = false;
            this.SetupIsRdmAdminUser(isRdmAdmin);
            this.Setup3Revisions();
            mv = sut.GetVersionDifferences(2, isRdmAdmin, userData);
            Assert.AreEqual(this.rev123.History, mv.WorkInProgressHistory);
            Assert.AreEqual(isRdmAdmin, mv.AdminUser);
            Assert.AreEqual(false, mv.IsEarliestVersion);
            Assert.AreEqual(2, mv.AvailableVersions.Count);
            Assert.IsNull(mv.PPRDDifferences);
            Assert.AreSame(this.rev122Option, mv.SelectedRevision);
            Assert.AreEqual(122, mv.SelectedVersionNumber);
            Assert.AreEqual(121, mv.PreviousVersionNumber);
            Assert.AreEqual($"Version 122", mv.SelectedVersionNumberDisplay);
            Assert.AreEqual($"Version 121", mv.PreviousVersionNumberDisplay);

            // Test id = -1 (RDM admin - should return WIP revision)
            isRdmAdmin = true;
            this.SetupIsRdmAdminUser(isRdmAdmin);
            this.Setup3Revisions();
            mv = sut.GetVersionDifferences(-1, isRdmAdmin, userData);
            Assert.AreEqual(this.rev123.History, mv.WorkInProgressHistory);
            Assert.AreEqual(isRdmAdmin, mv.AdminUser);
            Assert.AreEqual(false, mv.IsEarliestVersion);
            Assert.AreEqual(3, mv.AvailableVersions.Count);
            Assert.IsNull(mv.PPRDDifferences);
            Assert.AreSame(this.rev123Option, mv.SelectedRevision);
            Assert.AreEqual(123, mv.SelectedVersionNumber);
            Assert.AreEqual(122, mv.PreviousVersionNumber);
            Assert.AreEqual(CommonConstants.WorkInProgress, mv.SelectedVersionNumberDisplay);
            Assert.AreEqual($"Version 122", mv.PreviousVersionNumberDisplay);

            // Test id = -1 (non-admin - should return first (non WIP) revision)
            isRdmAdmin = false;
            this.SetupIsRdmAdminUser(isRdmAdmin);
            mv = sut.GetVersionDifferences(-1, isRdmAdmin, userData);
            Assert.AreEqual(this.rev123.History, mv.WorkInProgressHistory);
            Assert.AreEqual(isRdmAdmin, mv.AdminUser);
            Assert.AreEqual(false, mv.IsEarliestVersion);
            Assert.AreEqual(2, mv.AvailableVersions.Count);
            Assert.IsNull(mv.PPRDDifferences);
            Assert.AreSame(this.rev122Option, mv.SelectedRevision);
            Assert.AreEqual(122, mv.SelectedVersionNumber);
            Assert.AreEqual(121, mv.PreviousVersionNumber);
            Assert.AreEqual($"Version 122", mv.SelectedVersionNumberDisplay);
            Assert.AreEqual($"Version 121", mv.PreviousVersionNumberDisplay);

            // Test id = 1 (RDM admin - should return earliest revision)
            isRdmAdmin = true;
            mv = sut.GetVersionDifferences(1, isRdmAdmin, userData);
            Assert.AreEqual(this.rev123.History, mv.WorkInProgressHistory);
            Assert.AreEqual(isRdmAdmin, mv.AdminUser);
            Assert.AreEqual(true, mv.IsEarliestVersion);

            // Test id = 1 (non-admin - should return earliest revision)
            isRdmAdmin = false;
            mv = sut.GetVersionDifferences(1, isRdmAdmin, userData);
            Assert.AreEqual(isRdmAdmin, mv.AdminUser);
            Assert.AreEqual(true, mv.IsEarliestVersion);
        }

        /// <summary>
        /// Test that RatesAreValid succeeds with valid rates.
        /// </summary>
        [TestMethod]
        public void TestRatesAreValid()
        {
            var sut = this.CreateSut();

            this.rateDetailLoader.Setup(x => x.GetRatesByRevision(It.IsAny<RevisionModelView>()))
                .Returns(this.CreateValidRates());

            Assert.IsFalse(sut.GetInvalidRates(1).Any());
        }

        /// <summary>
        /// Test that RatesAreValid fails with a backward-looking rate that is beyond the lower bound.
        /// </summary>
        [TestMethod]
        public void TestRatesAreInvalidBackwardLookingLowerBound()
        {
            var sut = this.CreateSut();

            this.rateDetailLoader.Setup(x => x.GetRatesByRevision(It.IsAny<RevisionModelView>()))
                .Returns(this.CreateInvalidRatesBackwardLookingLowerBound());

            ICollection<string> result = sut.GetInvalidRates(1);
            Assert.IsTrue(result.Any());
            Assert.AreEqual("backwardLookingLowerBound", result.FirstOrDefault());
        }

        /// <summary>
        /// Test that RatesAreValid fails with a backward-looking rate that is beyond the upper bound.
        /// </summary>
        [TestMethod]
        public void TestRatesAreInvalidBackwardLookingUpperBound()
        {
            var sut = this.CreateSut();

            this.rateDetailLoader.Setup(x => x.GetRatesByRevision(It.IsAny<RevisionModelView>()))
                .Returns(this.CreateInvalidRatesBackwardLookingUpperBound());

            ICollection<string> result = sut.GetInvalidRates(1);
            Assert.IsTrue(result.Any());
            Assert.AreEqual("backwardLookingUpperBound", result.FirstOrDefault());
        }

        /// <summary>
        /// Test that RatesAreValid fails with a forward-looking rate that is beyond the lower bound.
        /// </summary>
        [TestMethod]
        public void TestRatesAreInvalidForwardLookingLowerBound()
        {
            var sut = this.CreateSut();

            this.rateDetailLoader.Setup(x => x.GetRatesByRevision(It.IsAny<RevisionModelView>()))
                .Returns(this.CreateInvalidRatesForwardLookingLowerBound());

            ICollection<string> result = sut.GetInvalidRates(1);
            Assert.IsTrue(result.Any());
            Assert.AreEqual("forwardLookingLowerBound", result.FirstOrDefault());
        }

        /// <summary>
        /// Test that RatesAreValid fails with a forward-looking rate that is beyond the upper bound.
        /// </summary>
        [TestMethod]
        public void TestRatesAreInvalidForwardLookingUpperBound()
        {
            var sut = this.CreateSut();

            this.rateDetailLoader.Setup(x => x.GetRatesByRevision(It.IsAny<RevisionModelView>()))
                .Returns(this.CreateInvalidRatesForwardLookingUpperBound());

            ICollection<string> result = sut.GetInvalidRates(1);
            Assert.IsTrue(result.Any());
            Assert.AreEqual("forwardLookingUpperBound", result.FirstOrDefault());
        }

        /// <summary>
        /// Create a valid set of rates using rates that test the edge cases.
        /// </summary>
        /// <returns>Valid rates.</returns>
        private Collection<RateDetailModelView> CreateValidRates()
        {
            int publishYear = DateTime.Now.Year;

            Collection<RateDetailModelView> rates = new Collection<RateDetailModelView>();

            RateDetailModelView backwardLookingLowerBound = new RateDetailModelView()
            {
                RateCode = "backwardLookingLowerBound",
                RateCategory = RateCategory.Fccom,
                Values = new Collection<RateYearModelView>()
                {
                    new RateYearModelView() { Year = publishYear - CommonConstants.RATE_TABLE_YEARS_TO_DISPLAY_BACKWARD_LOOKING_ADJUSTMENT, Value = 1 }
                }
            };

            RateDetailModelView backwardLookingUpperBound = new RateDetailModelView()
            {
                RateCode = "backwardLookingUpperBound",
                RateCategory = RateCategory.Fccom,
                Values = new Collection<RateYearModelView>()
                {
                    new RateYearModelView() { Year = publishYear + CommonConstants.RATE_TABLE_YEARS_TO_DISPLAY, Value = (decimal?) 0.1 }
                }
            };

            RateDetailModelView forwardLookingLowerBound = new RateDetailModelView()
            {
                RateCode = "forwardLookingLowerBound",
                RateCategory = RateCategory.DirectLabor,
                Values = new Collection<RateYearModelView>()
                {
                    new RateYearModelView() { Year = publishYear, Value = 1 }
                }
            };

            RateDetailModelView forwardLookingUpperBound = new RateDetailModelView()
            {
                RateCode = "forwardLookingUpperBound",
                RateCategory = RateCategory.DirectLabor,
                Values = new Collection<RateYearModelView>()
                {
                    new RateYearModelView() { Year = publishYear + CommonConstants.RATE_TABLE_YEARS_TO_DISPLAY, Value = (decimal?) 0.1 }
                }
            };


            // Order matters for other CreateInvalidRates methods.
            rates.Add(backwardLookingLowerBound);
            rates.Add(backwardLookingUpperBound);
            rates.Add(forwardLookingLowerBound);
            rates.Add(forwardLookingUpperBound);

            return rates;
        }

        /// <summary>
        /// Creates an invalid backward-looking rate by moving the value outside the lower bound.
        /// </summary>
        /// <returns>An invalid rate collection.</returns>
        private Collection<RateDetailModelView> CreateInvalidRatesBackwardLookingLowerBound()
        {
            Collection<RateDetailModelView> rates = this.CreateValidRates();
            rates[0].Values.First().Year--; 
            return rates;
        }

        /// <summary>
        /// Creates an invalid backward-looking rate by moving the value outside the upper bound.
        /// </summary>
        /// <returns>An invalid rate collection.</returns>
        private Collection<RateDetailModelView> CreateInvalidRatesBackwardLookingUpperBound()
        {
            Collection<RateDetailModelView> rates = this.CreateValidRates();
            rates[1].Values.First().Year++;
            return rates;
        }

        /// <summary>
        /// Creates an invalid forward-looking rate by moving the value outside the lower bound.
        /// </summary>
        /// <returns>An invalid rate collection.</returns>
        private Collection<RateDetailModelView> CreateInvalidRatesForwardLookingLowerBound()
        {
            Collection<RateDetailModelView> rates = this.CreateValidRates();
            rates[2].Values.First().Year--;
            return rates;
        }

        /// <summary>
        /// Creates an invalid forward-looking rate by moving the value outside the upper bound.
        /// </summary>
        /// <returns>An invalid rate collection.</returns>
        private Collection<RateDetailModelView> CreateInvalidRatesForwardLookingUpperBound()
        {
            Collection<RateDetailModelView> rates = this.CreateValidRates();
            rates[3].Values.First().Year++;
            return rates;
        }

        /// <summary>
        /// Set up Security Information to Mock desired IsRdmAdminUser value.
        /// </summary>
        /// <param name="isRdmAdminUser">Value to mock for IsRemAdminUser.</param>
        private void SetupIsRdmAdminUser(bool isRdmAdminUser)
        {
            string userntid = "moquser";
            this.securityInformation.Setup(x => x.ActiveUserNTID).Returns(userntid);
            this.securityInformation.Setup(x => x.IsRdmAdminUser(userntid)).Returns(isRdmAdminUser);
        }

        /// <summary>
        /// Set up null revisions list.
        /// </summary>
        private void SetupNullRevisions()
        {
            this.revisionMediator.Setup(x => x.GetAll()).Returns((IList<RevisionModelView>) null);
            this.revisionMediator.Setup(x => x.GetRevisionOptions(null)).Returns((IList<RevisionOptionModelView>) null);
        }

        /// <summary>
        /// Set up empty revisions list.
        /// </summary>
        private void SetupEmptyRevisions()
        {
            IList<RevisionModelView> revisions = new List<RevisionModelView>();
            IList<RevisionOptionModelView> revisionOptions = new List<RevisionOptionModelView>();
            this.revisionMediator.Setup(x => x.GetAll()).Returns(revisions);
            this.revisionMediator.Setup(x => x.GetRevisionOptions(revisions)).Returns(revisionOptions);
        }

        /// <summary>
        /// Set up revisions list with only one revision.
        /// </summary>
        private void Setup1Revision()
        {
            IList<RevisionModelView> revisions = new List<RevisionModelView> { this.rev122 };
            IList<RevisionModelView> revisionsNonAdmin = new List<RevisionModelView>();
            IList<RevisionOptionModelView> revisionOptions = new List<RevisionOptionModelView> { this.rev122Option };
            IList<RevisionOptionModelView> revisionOptionsNonAdmin = new List<RevisionOptionModelView>();
            this.revisionMediator.Setup(x => x.GetAll()).Returns(revisions);
            this.revisionMediator.Setup(x => x.GetRevisionOptions(revisions)).Returns(revisionOptions);
            this.revisionMediator.Setup(x => x.GetRevisionOptions(revisionsNonAdmin)).Returns(revisionOptionsNonAdmin);
            this.revisionMediator.Setup(x => x.GetPriorRevision(revisions, It.IsAny<int>())).Returns((RevisionModelView)null);
        }

        /// <summary>
        /// Set up revisions list with two revisions.
        /// </summary>
        private void Setup2Revisions()
        {
            IList<RevisionModelView> revisions = new List<RevisionModelView> { this.rev122, this.rev123 };
            IList<RevisionModelView> revisionsNonAdmin = new List<RevisionModelView> { this.rev122 };
            IList<RevisionOptionModelView> revisionOptions = new List<RevisionOptionModelView> { this.rev123Option, this.rev122Option };
            IList<RevisionOptionModelView> revisionOptionsNonAdmin = new List<RevisionOptionModelView> { this.rev122Option };
            this.revisionMediator.Setup(x => x.GetAll()).Returns(revisions);
            this.revisionMediator.Setup(x => x.GetRevisionOptions(revisions)).Returns(revisionOptions);
            this.revisionMediator.Setup(x => x.GetRevisionOptions(revisionsNonAdmin)).Returns(revisionOptionsNonAdmin);
            this.revisionMediator.Setup(x => x.GetPriorRevision(revisions, 3)).Returns(this.rev122);
            this.revisionMediator.Setup(x => x.GetPriorRevision(revisions, 2)).Returns((RevisionModelView)null);
        }

        /// <summary>
        /// Set up revisions list with three revisions.
        /// </summary>
        private void Setup3Revisions()
        {
            IList<RevisionModelView> revisions = new List<RevisionModelView> { this.rev121, this.rev122, this.rev123 };
            IList<RevisionModelView> revisionsNonAdmin = new List<RevisionModelView> { this.rev121, this.rev122 };
            IList<RevisionOptionModelView> revisionOptions = new List<RevisionOptionModelView> { this.rev123Option, this.rev122Option, this.rev121Option };
            IList<RevisionOptionModelView> revisionOptionsNonAdmin = new List<RevisionOptionModelView> { this.rev122Option, this.rev121Option };
            this.revisionMediator.Setup(x => x.GetAll()).Returns(revisions);
            this.revisionMediator.Setup(x => x.GetRevisionOptions(revisions)).Returns(revisionOptions);
            this.revisionMediator.Setup(x => x.GetRevisionOptions(revisionsNonAdmin)).Returns(revisionOptionsNonAdmin);
            this.revisionMediator.Setup(x => x.GetPriorRevision(revisions, 3)).Returns(this.rev122);
            this.revisionMediator.Setup(x => x.GetPriorRevision(revisions, 2)).Returns(this.rev121);
            this.revisionMediator.Setup(x => x.GetPriorRevision(revisions, 1)).Returns((RevisionModelView)null);
        }
    }
}
