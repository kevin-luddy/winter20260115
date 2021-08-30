// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
    using IES.ActionLogic.Validation;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AdminControllerLogicTest
    {
        /// <summary>
        /// Mock Revision Loader
        /// </summary>
        private Mock<IRevisionMediator> revisionMediator = new Mock<IRevisionMediator>();

        /// <summary>
        /// The area locking loader
        /// </summary>
        private Mock<IAreaLockingLoader> areaLockingLoader = new Mock<IAreaLockingLoader>();

        /// <summary>
        /// AD Utils
        /// </summary>
        private Mock<ActiveDirectoryUtilities> adUtils = new Mock<ActiveDirectoryUtilities>();

        /// <summary>
        /// Security Info
        /// </summary>
        private Mock<SecurityInformation> securityInfo;

        /// <summary>
        /// The rate detail loader
        /// </summary>
        private Mock<IRateDetailLoader> rateDetailLoader = new Mock<IRateDetailLoader>();

        /// <summary>
        /// The rate code replication loader
        /// </summary>
        private Mock<IRateCodeReplicationLoader> rateCodeReplicationLoader = new Mock<IRateCodeReplicationLoader>();

        /// <summary>
        /// Creates the sut.
        /// </summary>
        /// <returns>The System under test.</returns>
        public AdminControllerLogic CreateSut()
        {
            this.securityInfo = new Mock<SecurityInformation>(this.adUtils.Object, null);
            this.revisionMediator.Setup(x => x.GetWipRevision()).Returns(new RevisionModelView());
            return new AdminControllerLogic(this.areaLockingLoader.Object, this.revisionMediator.Object, this.adUtils.Object, this.securityInfo.Object, this.rateDetailLoader.Object, this.rateCodeReplicationLoader.Object);
        }

        /// <summary>
        /// Validates the rate code replication with null 
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateRateCodeReplication_EX1()
        {
            var sut = this.CreateSut();
            sut.ValidateRateCodeReplication(null);
        }

        /// <summary>
        /// Validates the rate code replication - TO Column must be unique.
        /// </summary>
        [TestMethod]
        public void ValidateRateCodeReplication_TO_Not_Unique()
        {
            var sut = this.CreateSut();
            this.rateDetailLoader.Setup(x => x.GetRateCodesForRevision(It.IsAny<int>())).Returns(new RateDto[] { new RateDto { Rate = "TIM" }, new RateDto { Rate = "BOB" }, new RateDto { Rate = "JEFF" } });

            this.rateCodeReplicationLoader.Setup(x => x.GetAll()).Returns(new List<RateCodeModelView> { new RateCodeModelView { Id = 5, From = "TIM", To = "BOB" } });

            List<RateCodeModelView> rateCodes = new List<RateCodeModelView>
            {
                new RateCodeModelView {
                    Id = 5,
                    IsDeleted = true,
                    From = "TIM",
                    To = "BOB"
                },
                new RateCodeModelView {
                    Id = -1,
                    IsDeleted = false,
                    To = "TIM",
                    From = "BOB"
                },
                new RateCodeModelView {
                    Id = -2,
                    IsDeleted = false,
                    To = "TIM",
                    From = "JEFF"
                }
            };

            ICollection<ValidationMessage> errors = sut.ValidateRateCodeReplication(rateCodes);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(errors.First().ValidationIssue, string.Format(AdminValidationConstants.RATE_CODE_TO_UNIQUE, "TIM"));
        }

        /// <summary>
        /// Validates the rate code replication - From and To must not match.
        /// </summary>
        [TestMethod]
        public void ValidateRateCodeReplication_FROM_TO_Match()
        {
            var sut = this.CreateSut();
            this.rateDetailLoader.Setup(x => x.GetRateCodesForRevision(It.IsAny<int>())).Returns(new RateDto[] { new RateDto { Rate = "TIM" }, new RateDto { Rate = "BOB" }, new RateDto { Rate = "JEFF" } });

            this.rateCodeReplicationLoader.Setup(x => x.GetAll()).Returns(new List<RateCodeModelView> { new RateCodeModelView { Id = 5, From = "TIM", To = "BOB" } });

            List<RateCodeModelView> rateCodes = new List<RateCodeModelView>
            {
                new RateCodeModelView {
                    Id = 5,
                    IsDeleted = true,
                    From = "TIM",
                    To = "BOB"
                },
                new RateCodeModelView {
                    Id = -1,
                    IsDeleted = false,
                    To = "TIM",
                    From = "TIM"
                }
            };

            ICollection<ValidationMessage> errors = sut.ValidateRateCodeReplication(rateCodes);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(errors.First().ValidationIssue, string.Format(AdminValidationConstants.RATE_CODE_FROM_TO_MATCH, "TIM"));
        }

        /// <summary>
        /// Validates the rate code replication - From and To must not match.
        /// </summary>
        [TestMethod]
        public void ValidateRateCodeReplication_FROM_TO_Match_Different_Row()
        {
            var sut = this.CreateSut();
            this.rateDetailLoader.Setup(x => x.GetRateCodesForRevision(It.IsAny<int>())).Returns(new RateDto[] { new RateDto { Rate = "TIM" }, new RateDto { Rate = "BOB" }, new RateDto { Rate = "JEFF" } });

            this.rateCodeReplicationLoader.Setup(x => x.GetAll()).Returns(new List<RateCodeModelView> { new RateCodeModelView { Id = 5, From = "TIM", To = "BOB" } });

            List<RateCodeModelView> rateCodes = new List<RateCodeModelView>
            {
                new RateCodeModelView {
                    Id = 5,
                    IsDeleted = true,
                    From = "TIM",
                    To = "BOB"
                },
                new RateCodeModelView {
                    Id = -1,
                    IsDeleted = false,
                    To = "TIM",
                    From = "BOB"
                },
                new RateCodeModelView {
                    Id = -2,
                    IsDeleted = false,
                    To = "JEFF",
                    From = "TIM"
                }
            };

            ICollection<ValidationMessage> errors = sut.ValidateRateCodeReplication(rateCodes);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(errors.First().ValidationIssue, string.Format(AdminValidationConstants.RATE_CODE_FROM_TO_MATCH, "TIM"));
        }

        /// <summary>
        /// Validates the rate code replication - From missing.
        /// </summary>
        [TestMethod]
        public void ValidateRateCodeReplication_FROM_Missing()
        {
            var sut = this.CreateSut();
            this.rateDetailLoader.Setup(x => x.GetRateCodesForRevision(It.IsAny<int>())).Returns(new RateDto[] { new RateDto { Rate = "TIM" }, new RateDto { Rate = "BOB" }, new RateDto { Rate = "JEFF" } });

            this.rateCodeReplicationLoader.Setup(x => x.GetAll()).Returns(new List<RateCodeModelView> { new RateCodeModelView { Id = 5, From = "TIM", To = "BOB" } });

            List<RateCodeModelView> rateCodes = new List<RateCodeModelView>
            {
                new RateCodeModelView {
                    Id = 5,
                    IsDeleted = true,
                    From = "TIM",
                    To = "BOB"
                },
                new RateCodeModelView {
                    Id = -1,
                    IsDeleted = false,
                    To = "TIM"
                }
            };

            ICollection<ValidationMessage> errors = sut.ValidateRateCodeReplication(rateCodes);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(errors.First().ValidationIssue, string.Format(AdminValidationConstants.RATE_CODE_REQUIRED, AdminValidationConstants.RATE_CODE_FROM_COLUMN));
        }

        /// <summary>
        /// Validates the rate code replication - To missing.
        /// </summary>
        [TestMethod]
        public void ValidateRateCodeReplication_TO_Missing()
        {
            var sut = this.CreateSut();
            this.rateDetailLoader.Setup(x => x.GetRateCodesForRevision(It.IsAny<int>())).Returns(new RateDto[] { new RateDto { Rate = "TIM" }, new RateDto { Rate = "BOB" }, new RateDto { Rate = "JEFF" } });

            this.rateCodeReplicationLoader.Setup(x => x.GetAll()).Returns(new List<RateCodeModelView> { new RateCodeModelView { Id = 5, From = "TIM", To = "BOB" } });

            List<RateCodeModelView> rateCodes = new List<RateCodeModelView>
            {
                new RateCodeModelView {
                    Id = 5,
                    IsDeleted = true
                },
                new RateCodeModelView {
                    Id = -1,
                    IsDeleted = false,
                    From = "BOB"
                }
            };

            ICollection<ValidationMessage> errors = sut.ValidateRateCodeReplication(rateCodes);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(errors.First().ValidationIssue, string.Format(AdminValidationConstants.RATE_CODE_REQUIRED, AdminValidationConstants.RATE_CODE_TO_COLUMN));
        }

        /// <summary>
        /// Validates the rate code replication - From invalid
        /// </summary>
        [TestMethod]
        public void ValidateRateCodeReplication_FROM_Invalid()
        {
            var sut = this.CreateSut();
            this.rateDetailLoader.Setup(x => x.GetRateCodesForRevision(It.IsAny<int>())).Returns(new RateDto[] { new RateDto { Rate = "TIM" }, new RateDto { Rate = "BOB" }, new RateDto { Rate = "JEFF" } });
            this.rateCodeReplicationLoader.Setup(x => x.GetAll()).Returns(new List<RateCodeModelView> { new RateCodeModelView { Id = 5, From = "TIM", To = "BOB" } });
            
            List<RateCodeModelView> rateCodes = new List<RateCodeModelView>
            {
                new RateCodeModelView {
                    Id = 5,
                    IsDeleted = true,
                    From = "TIM",
                    To = "BOB"
                },
                new RateCodeModelView {
                    Id = -1,
                    IsDeleted = false,
                    To = "TIM",
                    From = "BOBBY"
                }
            };

            ICollection<ValidationMessage> errors = sut.ValidateRateCodeReplication(rateCodes);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(errors.First().ValidationIssue, string.Format(AdminValidationConstants.RATE_CODE_INVALID, "BOBBY", AdminValidationConstants.RATE_CODE_FROM_COLUMN));
        }

        /// <summary>
        /// Validates the rate code replication - TO invalid
        /// </summary>
        [TestMethod]
        public void ValidateRateCodeReplication_TO_Invalid()
        {
            var sut = this.CreateSut();
            this.rateDetailLoader.Setup(x => x.GetRateCodesForRevision(It.IsAny<int>())).Returns(new RateDto[] { new RateDto { Rate = "TIM" }, new RateDto { Rate = "BOB" }, new RateDto { Rate = "JEFF" } });

            this.rateCodeReplicationLoader.Setup(x => x.GetAll()).Returns(new List<RateCodeModelView> { new RateCodeModelView { Id = 5, From = "TIM", To = "BOB" } });

            List<RateCodeModelView> rateCodes = new List<RateCodeModelView>
            {
                new RateCodeModelView {
                    Id = 5,
                    IsDeleted = true,
                    From = "TIM",
                    To = "BOB"
                },
                new RateCodeModelView {
                    Id = -1,
                    IsDeleted = false,
                    To = "TIM2",
                    From = "BOB"
                }
            };

            ICollection<ValidationMessage> errors = sut.ValidateRateCodeReplication(rateCodes);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(errors.First().ValidationIssue, string.Format(AdminValidationConstants.RATE_CODE_INVALID, "TIM2", AdminValidationConstants.RATE_CODE_TO_COLUMN));
        }

        /// <summary>
        /// Validates the rate code replication - Valid
        /// </summary>
        [TestMethod]
        public void ValidateRateCodeReplication_Valid()
        {
            var sut = this.CreateSut();
            this.rateDetailLoader.Setup(x => x.GetRateCodesForRevision(It.IsAny<int>())).Returns(new RateDto[] { new RateDto { Rate = "TIM" }, new RateDto { Rate = "BOB" }, new RateDto { Rate = "JEFF" } });

            this.rateCodeReplicationLoader.Setup(x => x.GetAll()).Returns(new List<RateCodeModelView> { new RateCodeModelView { Id = 5, From = "TIM", To = "BOB" } });

            List<RateCodeModelView> rateCodes = new List<RateCodeModelView>
            {
                new RateCodeModelView {
                    Id = 5,
                    IsDeleted = true,
                    From = "TIM",
                    To = "BOB"
                },
                new RateCodeModelView {
                    Id = -1,
                    IsDeleted = false,
                    To = "TIM",
                    From = "BOB"
                }
            };

            ICollection<ValidationMessage> errors = sut.ValidateRateCodeReplication(rateCodes);

            Assert.AreEqual(0, errors.Count);
        }

        /// <summary>
        /// Validate Cobra Detail Model Views with null collection.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateCobraDetailModelViews_EX1()
        {
            AdminControllerLogic sut = this.CreateSut();
            sut.ValidateCobraDetailModelViews(null);
        }

        /// <summary>
        /// Validate Cobra Detail Model Views - Happy path.
        /// </summary>
        [TestMethod]
        public void ValidateCobraDetailModelViews_NoErrors()
        {
            AdminControllerLogic sut = this.CreateSut();
            Collection<CobraDetailModelView> cobraDetails = new Collection<CobraDetailModelView>();
            cobraDetails.Add(new CobraDetailModelView
            {
                RateCategoryDescription = "Direct Labor",
                RateCode = "RateCode1",
                Description = "Test1",
                Code1 = Code1.INDIRECT,
                Code1Description = Code1.INDIRECT.GetDescription(),
                RateSet = "RateSet1"
            });
            cobraDetails.Add(new CobraDetailModelView
            {
                RateCategoryDescription = "Direct Labor",
                RateCode = "RateCode2",
                Description = "Test2",
                Code1 = Code1.SVCCTR,
                Code1Description = Code1.SVCCTR.GetDescription(),
                RateSet = "RateSet2"
            });
            cobraDetails.Add(new CobraDetailModelView
            {
                RateCategoryDescription = "Direct Labor",
                RateCode = "RateCode3",
                Description = "Test3"
            });
            ICollection<ValidationMessage> messages = sut.ValidateCobraDetailModelViews(cobraDetails);

            Assert.AreEqual(0, messages.Count);
        }

        /// <summary>
        /// Validate Cobra Detail Model Views - with invalid data
        /// </summary>
        [TestMethod]
        public void ValidateCobraDetailModelViews_Invalid()
        {
            AdminControllerLogic sut = this.CreateSut();
            Collection<CobraDetailModelView> cobraDetails = new Collection<CobraDetailModelView>();
            cobraDetails.Add(new CobraDetailModelView
            {
                RateCategoryDescription = "Direct Labor",
                RateCode = "RateCode1",
                Description = "Test1",
                Code1 = Code1.INDIRECT,
                Code1Description = Code1.INDIRECT.GetDescription(),
                RateSet = string.Empty
            });
            cobraDetails.Add(new CobraDetailModelView
            {
                RateCategoryDescription = "Direct Labor",
                RateCode = "RateCode2",
                Description = "Test2",
                Code1 = Code1.NA,
                Code1Description = Code1.NA.GetDescription(),
                RateSet = "RateSet2"
            });
            cobraDetails.Add(new CobraDetailModelView
            {
                RateCategoryDescription = "Direct Labor",
                RateCode = "RateCode3",
                Description = "Test3",
                Code1 = null,
                RateSet = "RateSet3"
            });
            ICollection<ValidationMessage> messages = sut.ValidateCobraDetailModelViews(cobraDetails);

            Assert.AreEqual(2, messages.Count);
            string expectedMessage1 = string.Format(AdminValidationConstants.COBRA_MAPPING_MISSING_RATE_SET, "RateCode1");
            string expectedMessage2 = string.Format(AdminValidationConstants.COBRA_MAPPING_MISSING_CODE_1, "RateCode2, RateCode3");
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage1)));
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage2)));
        }

        /// <summary>
        /// Validate Cobra Years - with null or empty collection.
        /// </summary>
        [TestMethod]
        public void ValidateCobraYearConfigurations_NullOrEmptyCollection()
        {
            AdminControllerLogic sut = this.CreateSut();
            ICollection<ValidationMessage> messages = sut.ValidateCobraYearConfigurations(null);
            Assert.AreEqual(1, messages.Count);
            string expectedMessage1 = string.Format(AdminValidationConstants.COBRA_YEAR_CONFIGURATION_REQUIRED);
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage1)));

            messages = sut.ValidateCobraYearConfigurations(new Collection<CobraYearGridModelView> ());
            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage1)));
        }

        /// <summary>
        /// Validate Cobra Years - Happy path
        /// </summary>
        [TestMethod]
        public void ValidateCobraYearConfigurations_NoErrors()
        {
            AdminControllerLogic sut = this.CreateSut();
            Collection<CobraYearGridModelView> cobraYears = new Collection<CobraYearGridModelView>();
            cobraYears.Add(new CobraYearGridModelView
            {
                CobraDate = DateTime.Now,
                Year = DateTime.Now.Year
            });

            ICollection<ValidationMessage> messages = sut.ValidateCobraYearConfigurations(cobraYears);

            Assert.AreEqual(0, messages.Count);
        }

        /// <summary>
        /// Validate Cobra Years - with invalid data
        /// </summary>
        [TestMethod]
        public void ValidateCobraYearConfigurations_Invalid()
        {
            AdminControllerLogic sut = this.CreateSut();
            Collection<CobraYearGridModelView> cobraYears = new Collection<CobraYearGridModelView>();
            int year1 = 2001;
            cobraYears.Add(new CobraYearGridModelView
            {
                CobraDate = DateTime.MinValue,
                Year = year1
            });

            DateTime date2 = new DateTime(2002, 2, 2);
            int year0 = 0;
            cobraYears.Add(new CobraYearGridModelView
            {
                CobraDate = date2,
                Year = year0
            });

            DateTime date3a = new DateTime(2003, 12, 31);
            DateTime date3b = new DateTime(2003, 1, 1);
            int dupYear3 = 2003;
            cobraYears.Add(new CobraYearGridModelView
            {
                CobraDate = date3a,
                Year = dupYear3
            });
            cobraYears.Add(new CobraYearGridModelView
            {
                CobraDate = date3b,
                Year = dupYear3
            });

            DateTime dupDate4 = new DateTime(2003, 12, 31);
            int year3 = 2003;
            int year4 = 2004;
            cobraYears.Add(new CobraYearGridModelView
            {
                CobraDate = dupDate4,
                Year = year3
            });
            cobraYears.Add(new CobraYearGridModelView
            {
                CobraDate = dupDate4,
                Year = year4
            });


            DateTime date5a = new DateTime(2003, 12, 31);
            DateTime date5b = new DateTime(2006, 1, 1);
            int year5 = 2005;
            cobraYears.Add(new CobraYearGridModelView
            {
                CobraDate = date5a,
                Year = year5
            });
            cobraYears.Add(new CobraYearGridModelView
            {
                CobraDate = date5b,
                Year = year5
            });

            ICollection<ValidationMessage> messages = sut.ValidateCobraYearConfigurations(cobraYears);

            Assert.AreEqual(5, messages.Count);
            string expectedMessage1 = string.Format(AdminValidationConstants.COBRA_YEAR_CONFIGURATION_MISSING_OR_INVALID_COBRA_DATE, year1);
            string expectedMessage2 = string.Format(AdminValidationConstants.COBRA_YEAR_CONFIGURATION_MISSING_YEAR, date2.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR));
            string expectedMessage3 = string.Format(AdminValidationConstants.COBRA_YEAR_CONFIGURATION_DUPLICATE_YEARS, string.Format("{0}, {1}", year3, year5));
            string expectedMessage4 = string.Format(AdminValidationConstants.COBRA_YEAR_CONFIGURATION_DUPLICATE_COBRA_DATES, dupDate4.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR));
            string expectedMessage5 = string.Format(AdminValidationConstants.COBRA_YEAR_CONFIGURATION_INVALID_DATE_COMBINATION, string.Format("{0}, {1}, {2}", year0, year5, year5));           
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage1)));
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage2)));
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage3)));
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage4)));
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage5)));
        }

        /// <summary>
        /// Validate Rate Year Configuration - Happy path
        /// </summary>
        [TestMethod]
        public void ValidateRateYearConfiguration_NoErrors()
        {
            AdminControllerLogic sut = this.CreateSut();
            ICollection<ValidationMessage> messages = sut.ValidateRateYearConfiguration(AdminValidationConstants.RATE_YEAR_CONFIGURATION_MIN_YEAR.ToString(), AdminValidationConstants.RATE_YEAR_CONFIGURATION_MAX_YEAR.ToString());
            Assert.AreEqual(0, messages.Count);
        }

        /// <summary>
        /// Validate Rate Year Configuration - Invalid combinations
        /// </summary>
        [TestMethod]
        public void ValidateRateYearConfiguration_Invalid()
        {
            AdminControllerLogic sut = this.CreateSut();
            int configMinYear = AdminValidationConstants.RATE_YEAR_CONFIGURATION_MIN_YEAR;
            int configMaxYear = AdminValidationConstants.RATE_YEAR_CONFIGURATION_MAX_YEAR;

            ICollection<ValidationMessage> messages = sut.ValidateRateYearConfiguration((configMinYear-1).ToString(), configMaxYear.ToString());
            Assert.AreEqual(1, messages.Count);
            string expectedMessage = string.Format(AdminValidationConstants.RATE_YEAR_CONFIGURATION_YEAR_OUT_OF_RANGE, "Start", AdminValidationConstants.RATE_YEAR_CONFIGURATION_MIN_YEAR, AdminValidationConstants.RATE_YEAR_CONFIGURATION_MAX_YEAR);
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage)));

            messages = sut.ValidateRateYearConfiguration(configMinYear.ToString(), (configMaxYear + 1).ToString());
            Assert.AreEqual(1, messages.Count);
            expectedMessage = string.Format(AdminValidationConstants.RATE_YEAR_CONFIGURATION_YEAR_OUT_OF_RANGE, "End", AdminValidationConstants.RATE_YEAR_CONFIGURATION_MIN_YEAR, AdminValidationConstants.RATE_YEAR_CONFIGURATION_MAX_YEAR);
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage)));

            messages = sut.ValidateRateYearConfiguration(configMaxYear.ToString(), configMinYear.ToString());
            Assert.AreEqual(1, messages.Count);
            expectedMessage = AdminValidationConstants.RATE_YEAR_CONFIGURATION_YEARS_OUT_OF_ORDER;
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage)));

            messages = sut.ValidateRateYearConfiguration("2002a", configMaxYear.ToString());
            Assert.AreEqual(1, messages.Count);
            expectedMessage = AdminValidationConstants.RATE_YEAR_CONFIGURATION_YEAR_NOT_NUMERIC;
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage)));

            messages = sut.ValidateRateYearConfiguration(configMaxYear.ToString(), "");
            Assert.AreEqual(1, messages.Count);
            expectedMessage = AdminValidationConstants.RATE_YEAR_CONFIGURATION_YEAR_NOT_NUMERIC;
            Assert.IsTrue(messages.Any(x => x.ValidationIssue.Equals(expectedMessage)));
        }

        /// <summary>
        /// Test User Authorization for RDM Viewers, Admins and COBRA Admins against a representative set of controller actions.
        /// </summary>
        [TestMethod]
        public void TestUserAuthorization()
        {
            AdminControllerLogic sut = this.CreateSut();
            string userntid = "moquser";
            this.SetupRdmUserPermissions(userntid, false, false, false);  // Not an RDM user
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, "index"));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, "index"));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, IESWebConstants.ACTION_VIEW_RATES));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_BURDEN_POOL, IESWebConstants.ACTION_GET_BURDEN_POOLS));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_COBRA_DATA));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_PRO_PRICER_DATA));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_REVISION_AS_JSON));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, IESWebConstants.ACTION_LOCK));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN, IESWebConstants.ACTION_GET_MENU_OPTIONS));

            this.SetupRdmUserPermissions(userntid, true, false, false);  // RDM Admin
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, IESWebConstants.ACTION_VIEW_RATES));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_BURDEN_POOL, IESWebConstants.ACTION_GET_BURDEN_POOLS));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_COBRA_DATA), "RDM Admin is not allowed access to COBRA Admin actions");
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_PRO_PRICER_DATA));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_REVISION_AS_JSON));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, IESWebConstants.ACTION_LOCK));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN, IESWebConstants.ACTION_GET_MENU_OPTIONS));

            this.SetupRdmUserPermissions(userntid, false, true, false);  // RDM COBRA Admin
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, IESWebConstants.ACTION_VIEW_RATES));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_BURDEN_POOL, IESWebConstants.ACTION_GET_BURDEN_POOLS));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_COBRA_DATA));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_PRO_PRICER_DATA), "RDM COBRA Admin is not allowed access to regular Admin actions");
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_REVISION_AS_JSON), "RDM COBRA Admin is not allowed access to regular Admin actions");
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, IESWebConstants.ACTION_LOCK));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN, IESWebConstants.ACTION_GET_MENU_OPTIONS));

            this.SetupRdmUserPermissions(userntid, false, false, true);  // RDM Viewer
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, IESWebConstants.ACTION_VIEW_RATES));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_BURDEN_POOL, IESWebConstants.ACTION_GET_BURDEN_POOLS), "RDM Viewer is not allowed access to Admin actions");
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_COBRA_DATA), "RDM Viewer is not allowed access to Admin actions");
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_PRO_PRICER_DATA), "RDM Viewer is not allowed access to Admin actions");
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_REVISION_AS_JSON), "RDM Viewer is not allowed access to Admin actions");
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, IESWebConstants.ACTION_LOCK), "RDM Viewer is not allowed access to Admin actions");
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN, IESWebConstants.ACTION_GET_MENU_OPTIONS));

            this.SetupRdmUserPermissions(userntid, true, false, true);  // RDM Admin & Viewer
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, IESWebConstants.ACTION_VIEW_RATES));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_BURDEN_POOL, IESWebConstants.ACTION_GET_BURDEN_POOLS));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_COBRA_DATA), "RDM Admin is not allowed access to COBRA Admin actions");
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_PRO_PRICER_DATA));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_REVISION_AS_JSON));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, IESWebConstants.ACTION_LOCK));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN, IESWebConstants.ACTION_GET_MENU_OPTIONS));

            this.SetupRdmUserPermissions(userntid, false, true, true);  // RDM COBRA Admin & Viewer
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, IESWebConstants.ACTION_VIEW_RATES));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_BURDEN_POOL, IESWebConstants.ACTION_GET_BURDEN_POOLS), "RDM COBRA Admin is not allowed access to regular Admin actions");
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_COBRA_DATA));
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_PRO_PRICER_DATA), "RDM COBRA Admin is not allowed access to regular Admin actions");
            Assert.IsFalse(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_REVISION_AS_JSON), "RDM COBRA Admin is not allowed access to regular Admin actions");
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, IESWebConstants.ACTION_LOCK));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN, IESWebConstants.ACTION_GET_MENU_OPTIONS));

            this.SetupRdmUserPermissions(userntid, true, true, false);  // RDM Admin and COBRA Admin
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, IESWebConstants.ACTION_VIEW_RATES));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_BURDEN_POOL, IESWebConstants.ACTION_GET_BURDEN_POOLS));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_COBRA_DATA));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_PRO_PRICER_DATA));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_REVISION_AS_JSON));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, IESWebConstants.ACTION_LOCK));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN, IESWebConstants.ACTION_GET_MENU_OPTIONS));

            this.SetupRdmUserPermissions(userntid, true, true, true);  // RDM Admin, COBRA Admin, and Viewer
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, "index"));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_RATE, IESWebConstants.ACTION_VIEW_RATES));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_BURDEN_POOL, IESWebConstants.ACTION_GET_BURDEN_POOLS));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_COBRA_DATA));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_PRO_PRICER_DATA));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_REPORTS, IESWebConstants.ACTION_EXPORT_REVISION_AS_JSON));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_HOME, IESWebConstants.ACTION_LOCK));
            Assert.IsTrue(sut.IsUserAuthorized(IESWebConstants.CONTROLLER_ADMIN, IESWebConstants.ACTION_GET_MENU_OPTIONS));
        }

        /// <summary>
        /// Set up Security Information to Mock RDM user for desired permission configurations
        /// </summary>
        /// <param name="userntid">User NT Id to mock</param>
        /// <param name="isRdmAdminUser">Value to mock for IsRdmAdminUser.</param>
        /// <param name="isRdmCobraAdminUser">Value to mock for IsRdmCobraAdminUser.</param>
        /// <param name="isRdmViewerUser">Value to mock for IsRdmViewerUser.</param>
        private void SetupRdmUserPermissions(string userntid, bool isRdmAdminUser, bool isRdmCobraAdminUser, bool isRdmViewerUser)
        {
            this.securityInfo.Setup(x => x.ActiveUserNTID).Returns(userntid);
            this.securityInfo.Setup(x => x.IsRdmAdminUser(userntid)).Returns(isRdmAdminUser);
            this.securityInfo.Setup(x => x.IsRdmCobraAdminUser(userntid)).Returns(isRdmCobraAdminUser);
            this.securityInfo.Setup(x => x.IsRdmViewerUser(userntid)).Returns(isRdmViewerUser);
        }
    }
}
