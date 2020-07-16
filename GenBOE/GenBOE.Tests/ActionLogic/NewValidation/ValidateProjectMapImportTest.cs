// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.NewValidation
{

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.NewValidation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;


    [TestClass]
    public class ValidateProjectMapImportTest
    {
        private Mock<ICommonDataMapper> commonDataMapper;
        private IFullObjectFactory fullObjectFactory;
        private Mock<IPermissionsDTODataLoader> permissionsLoader;
        private Mock<IOffloadRatesDTOLoader> offloadRatesLoader;
        private Mock<IRetriever> retriever;
        private Mock<IUserDTODataLoader> userLoader;

        /// <summary>
        ///     Initializes data before each test run for this class.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            this.fullObjectFactory = new FullObjectFactory(null, null, null, null, null, null, null, null, null,
                null, null, null);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), this.fullObjectFactory);

            this.retriever = new Mock<IRetriever>();
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), this.retriever.Object);
            this.userLoader = new Mock<IUserDTODataLoader>();

            this.permissionsLoader = new Mock<IPermissionsDTODataLoader>();

            PerformingOrgDTO perfOrg = new PerformingOrgDTO
            {
                IsSystemPerfOrg = false,
                PerformingOrgDesc = "P",
                PerformingOrgName = "P",
                Id = 15
            };
            this.retriever.Setup(x => x.GetPerformingOrgsByListId(It.IsAny<int>()))
                .Returns(new Collection<PerformingOrgDTO> { perfOrg });

            ResourceDTO resource1 = new ResourceDTO
            {
                ResourceDesc = "R",
                ResourceName = "R",
                Id = 20,
                ElementOfCost = ElementOfCostType.LMLabor,
                RateType = RateType.Hours
            };
            ResourceDTO resource2 = new ResourceDTO
            {
                ResourceDesc = "S",
                ResourceName = "S",
                Id = 30,
                ElementOfCost = ElementOfCostType.Sub,
                RateType = RateType.Cost
            };
            ResourceDTO resource3 = new ResourceDTO
            {
                ResourceDesc = "S2",
                ResourceName = "S2",
                Id = 31,
                ElementOfCost = ElementOfCostType.Sub,
                RateType = RateType.Cost
            };
            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>()))
                .Returns(new Collection<ResourceDTO> { resource1, resource2, resource3 });

            this.commonDataMapper = new Mock<ICommonDataMapper>();
            this.offloadRatesLoader = new Mock<IOffloadRatesDTOLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(IOffloadRatesDTOLoader), this.offloadRatesLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), this.commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader),
                this.permissionsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), this.userLoader.Object);
        }

        [TestMethod]
        public void PMapImport_ActIdResourceWbs_NotUnique()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "1-2",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "1-3",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-2",
                    WbsNumber = "4.01.06",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-2",
                    WbsNumber = "4.01.06",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    WbsElementTitle = "1234",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-2",
                    WbsNumber = "4.01.06",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    WbsElementTitle = "1234",
                    ClassOfCost = ClassOfCost.DevNonRecurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-2",
                    WbsNumber = "4.01.06",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    WbsElementTitle = "1234",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdResourceWbsIsUnique();
            Assert.AreEqual(8, sut.ValidationMessages.Count,
                "Validator should generate 8 error messages, one for each row where the composite key Activity ID, Resource, WBS Element Title, and WBS Number is not unique");
        }

        [TestMethod]
        public void PMapImport_ActIdResourceWbs_IsUnique()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    WbsElementTitle = "123",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01",
                    WbsElementTitle = "123",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01",
                    WbsElementTitle = "1234",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06",
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    WbsElementTitle = "123",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                }

            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdResourceWbsIsUnique();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since the composite key Activity ID, Resource, WBS Element Title, and WBS Number is unique");
        }

        [TestMethod]
        public void PMapImport_ActIdResourceWbs_AdtlTest()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    CostCenter = "aaa",
                    WbsNumber = "wbs",
                    WbsElementTitle = "wbstitle",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    CostCenter = "aaa1",
                    WbsNumber = "wbs",
                    WbsElementTitle = "wbstitle",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdResourceWbsIsUnique();

            Assert.AreEqual(0, sut.ValidationMessages.Count);

            projectMapRows = new ProjectMapModelView[]
            {
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    CostCenter = "aaa",
                    WbsNumber = "wbs",
                    WbsElementTitle = "wbstitle",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    InitialResource = "4043WWC",
                    CostCenter = "aaa",
                    WbsNumber = "wbs",
                    WbsElementTitle = "wbstitle",
                    ClassOfCost = ClassOfCost.Recurring.ToDescription()
                }
            };

            sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdResourceWbsIsUnique();

            Assert.AreEqual(2, sut.ValidationMessages.Count);
        }

        [TestMethod]
        public void PMapImport_ActIdStartEndDate_NotUnique()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    ActivityID = "BSW1023",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                },
                new ProjectMapModelView
                {
                    CamName = "1-2",
                    ActivityID = "BSW1023",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                },
                new ProjectMapModelView
                {
                    CamName = "1-3",
                    ActivityID = "BSW1023",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    ActivityID = "BSW1022",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    ActivityID = "BSW1048",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                },
                new ProjectMapModelView
                {
                    CamName = "3-2",
                    ActivityID = "BSW1048",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdDatesIsUnique();
            Assert.AreEqual(5, sut.ValidationMessages.Count,
                "Validator should generate 5 error messages, one for each row that is not unique");
        }

        [TestMethod]
        public void PMapImport_ActIdStartEndDate_IsUnique()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    ActivityID = "BSW1023",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    ActivityID = "BSW1022",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    ActivityID = "BSW1048",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdDatesIsUnique();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since the composite key Activity ID, Start Date, and End Date is unique");
        }

        [TestMethod]
        public void PMapImport_ActIdStartEndDate_AdtlTest()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019"),
                    InitialResource = "aaa",
                    CostCenter = "bbb"
                },
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019"),
                    InitialResource = "aaa1",
                    CostCenter = "bbb"
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdDatesIsUnique();
            Assert.AreEqual(0, sut.ValidationMessages.Count);

            projectMapRows = new ProjectMapModelView[]
            {
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019"),
                    InitialResource = "aaa",
                    CostCenter = "bbb"
                },
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019"),
                    InitialResource = "aaa",
                    CostCenter = "bbb"
                }
            };

            sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdDatesIsUnique();
            Assert.AreEqual(2, sut.ValidationMessages.Count);
        }

        [TestMethod]
        public void PMapImport_ActIdActNameWbs_NotUnique()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    ActivityName = "A/C #1 - Conduct MDSC Review & Resolve Issues - Pre Instrumentation"
                },
                new ProjectMapModelView
                {
                    CamName = "1-2",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    ActivityName = "A/C #1 - Conduct MDSC Review & Resolve Issues - Pre Instrumentation"
                },
                new ProjectMapModelView
                {
                    CamName = "1-3",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    ActivityName = "A/C #1 - Conduct MDSC Review & Resolve Issues - Pre Instrumentation"
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    ActivityName = "A/C #1 - Prepare for MDSC Review - Pre Instrumentation"
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    ActivityName = "COFT Support"
                },
                new ProjectMapModelView
                {
                    CamName = "3-2",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    ActivityName = "COFT Support"
                },
                new ProjectMapModelView
                {
                    CamName = "3-2",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    ActivityName = "COFT Support",
                    WbsElementTitle = "123"
                },
                new ProjectMapModelView
                {
                    CamName = "3-2",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    ActivityName = "COFT Support",
                    WbsElementTitle = "123"
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdActNameWbsIsUnique();
            Assert.AreEqual(7, sut.ValidationMessages.Count,
                "Validator should generate 7 error messages, one for each row where the composite key Activity ID, Activity Name, Wbs Element Title, and WBS Number is not unique");
        }

        [TestMethod]
        public void PMapImport_ActIdActNameWbs_IsUnique()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    ActivityName = "A/C #1 - Conduct MDSC Review & Resolve Issues - Pre Instrumentation"
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    ActivityName = "A/C #1 - Prepare for MDSC Review - Pre Instrumentation"
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    ActivityName = "COFT Support"
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    ActivityName = "COFT Support",
                    WbsElementTitle = "123"
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdActNameWbsIsUnique();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since the composite key Activity ID, Activity Name, WBS Element Title, and WBS Number is unique");
        }

        [TestMethod]
        public void PMapImport_ActIdActNameWbs_AdtlTest()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    ActivityName = "A/C #1 - Conduct MDSC Review & Resolve Issues - Pre Instrumentation",
                    WbsNumber = "4.01.06.01",
                    WbsElementTitle = "aaa",
                    InitialResource = "bbb",
                    CostCenter = "ccc"
                },
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    ActivityName = "A/C #1 - Conduct MDSC Review & Resolve Issues - Pre Instrumentation",
                    WbsNumber = "4.01.06.01",
                    WbsElementTitle = "aaa",
                    InitialResource = "bbb",
                    CostCenter = "ccc1"
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdActNameWbsIsUnique();
            Assert.AreEqual(0, sut.ValidationMessages.Count);

            projectMapRows = new ProjectMapModelView[]
            {
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    ActivityName = "A/C #1 - Conduct MDSC Review & Resolve Issues - Pre Instrumentation",
                    WbsNumber = "4.01.06.01",
                    WbsElementTitle = "aaa",
                    InitialResource = "bbb",
                    CostCenter = "ccc"
                },
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    ActivityName = "A/C #1 - Conduct MDSC Review & Resolve Issues - Pre Instrumentation",
                    WbsNumber = "4.01.06.01",
                    WbsElementTitle = "aaa",
                    InitialResource = "bbb",
                    CostCenter = "ccc"
                }
            };

            sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdActNameWbsIsUnique();
            Assert.AreEqual(2, sut.ValidationMessages.Count);
        }

        [TestMethod]
        public void PMapImport_ActNameResource_NotUnique()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView {CamName = "1-1", ActivityName = "COFT Support", InitialResource = "4043WWC"},
                new ProjectMapModelView {CamName = "1-2", ActivityName = "COFT Support", InitialResource = "4043WWC"},
                new ProjectMapModelView {CamName = "1-3", ActivityName = "COFT Support", InitialResource = "4043WWC"},
                new ProjectMapModelView {CamName = "2-1", ActivityName = "COFT Support", InitialResource = "4043WWB"},
                new ProjectMapModelView {CamName = "3-1", ActivityName = "COFT Support", InitialResource = "4043PP3"},
                new ProjectMapModelView {CamName = "3-2", ActivityName = "COFT Support", InitialResource = "4043PP3"}
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActNameInitialResourceIsUnique();
            Assert.AreEqual(5, sut.ValidationMessages.Count,
                "Validator should generate 5 error messages, one for each row where the composite key Activity Name and Resource is not unique");
        }

        [TestMethod]
        public void PMapImport_ActIdActNameResource_IsUnique()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView {CamName = "1-1", ActivityName = "COFT Support", InitialResource = "4043WWC"},
                new ProjectMapModelView {CamName = "2-1", ActivityName = "COFT Support", InitialResource = "4043WWB"},
                new ProjectMapModelView {CamName = "3-1", ActivityName = "COFT Support", InitialResource = "4043PP3"}
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActNameInitialResourceIsUnique();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since the composite key Activity Name and Resource is unique");
        }

        [TestMethod]
        public void PMapImport_ActNameResource_AdtlTest()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView { ActivityName = "COFT Support", InitialResource = "4043WWC", CostCenter = "aaa" },
                new ProjectMapModelView { ActivityName = "COFT Support", InitialResource = "4043WWC", CostCenter = "aaa1" }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActNameInitialResourceIsUnique();
            Assert.AreEqual(0, sut.ValidationMessages.Count);

            projectMapRows = new ProjectMapModelView[]
            {
                new ProjectMapModelView { ActivityName = "COFT Support", InitialResource = "4043WWC", CostCenter = "aaa" },
                new ProjectMapModelView { ActivityName = "COFT Support", InitialResource = "4043WWC", CostCenter = "aaa" }
            };

            sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActNameInitialResourceIsUnique();
            Assert.AreEqual(2, sut.ValidationMessages.Count);
        }

        [TestMethod]
        public void PMapImport_ActIdWbsCategory_NotUnique()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "ENG"
                },
                new ProjectMapModelView
                {
                    CamName = "1-2",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "ENG"
                },
                new ProjectMapModelView
                {
                    CamName = "1-3",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "ENG"
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "Pilots"
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "PP&C"
                },
                new ProjectMapModelView
                {
                    CamName = "3-2",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "PP&C"
                },
                new ProjectMapModelView
                {
                    CamName = "3-2",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "PP&C",
                    WbsElementTitle = "1234"
                },
                new ProjectMapModelView
                {
                    CamName = "3-2",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "PP&C",
                    WbsElementTitle = "1234"
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdWbsCategoryIsUnique();
            Assert.AreEqual(7, sut.ValidationMessages.Count,
                "Validator should generate 7 error messages, one for each row where the composite key Activity ID, WBS Element Title, WBS Number, and Category is not unique");
        }

        [TestMethod]
        public void PMapImport_ActIdWbsCategory_IsUnique()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "ENG"
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "Pilots"
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "PP&C"
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "PP&C",
                    WbsElementTitle = "123"
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdWbsCategoryIsUnique();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since the composite key Activity ID, WBS Element Title, WBS Number, and Category is unique");
        }

        [TestMethod]
        public void PMapImport_ClassOfCost_NotConsistent()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "ENG",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023b",
                    Category = "Pilots",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Category = "PP&C",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Category = "PP&C",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            Dictionary<string, ICollection<ProjectMapModelView>> tieredData = ProjectMapConverter.GroupByTiers(projectMapRows);
            sut.ValidateClassOfCostIsConsistent(tieredData);
            Assert.AreEqual(2, sut.ValidationMessages.Count,
                "Validator should generate 2 errors since the Class of Cost inside a tiered group is not consistent");
        }

        [TestMethod]
        public void PMapImport_ClassOfCost_IsConsistent()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "ENG",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023b",
                    Category = "Pilots",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Category = "PP&C",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Category = "PP&C",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            Dictionary<string, ICollection<ProjectMapModelView>> tieredData = ProjectMapConverter.GroupByTiers(projectMapRows);
            sut.ValidateClassOfCostIsConsistent(tieredData);
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since the Class of Cost inside a tiered group is consistent");
        }

        [TestMethod]
        public void PMapImport_Rationale_NotConsistent()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Rationale = "ENG",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023b",
                    Rationale = "Pilots",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Rationale = "PP&C",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Rationale = "PP&Casdf",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            Dictionary<string, ICollection<ProjectMapModelView>> tieredData = ProjectMapConverter.GroupByTiers(projectMapRows);
            sut.ValidateRationaleIsConsistent(tieredData);
            Assert.AreEqual(2, sut.ValidationMessages.Count,
                "Validator should generate 2 errors since the Rationale inside a tiered group is not consistent");
        }

        [TestMethod]
        public void PMapImport_Rationale_IsConsistent()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Rationale = "ENG",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023b",
                    Rationale = "Pilots",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Rationale = "PP&C",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Rationale = "PP&C",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            Dictionary<string, ICollection<ProjectMapModelView>> tieredData = ProjectMapConverter.GroupByTiers(projectMapRows);
            sut.ValidateRationaleIsConsistent(tieredData);
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since the Rationale inside a tiered group is consistent");
        }

        [TestMethod]
        public void PMapImport_Category_NotConsistent()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "ENG",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023b",
                    Category = "Pilots",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Category = "PP&C",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Category = "PP&Cf",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            Dictionary<string, ICollection<ProjectMapModelView>> tieredData = ProjectMapConverter.GroupByTiers(projectMapRows);
            sut.ValidateCategoryIsConsistent(tieredData);
            Assert.AreEqual(2, sut.ValidationMessages.Count,
                "Validator should generate 2 errors since the Category inside a tiered group is not consistent");
        }

        [TestMethod]
        public void PMapImport_Category_IsConsistent()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "ENG",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023b",
                    Category = "Pilots",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Category = "PP&C",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Category = "PP&C",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            Dictionary<string, ICollection<ProjectMapModelView>> tieredData = ProjectMapConverter.GroupByTiers(projectMapRows);
            sut.ValidateCategoryIsConsistent(tieredData);
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since the Category inside a tiered group is consistent");
        }

        [TestMethod]
        public void PMapImport_CamName_NotConsistent()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "ENG"
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023b",
                    Category = "Pilots"
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Category = "PP&C"
                },
                new ProjectMapModelView
                {
                    CamName = "3-2",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Category = "PP&C"
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            Dictionary<string, ICollection<ProjectMapModelView>> tieredData = ProjectMapConverter.GroupByTiers(projectMapRows);
            sut.ValidateCamNameIsConsistent(tieredData);
            Assert.AreEqual(2, sut.ValidationMessages.Count,
                "Validator should generate 2 errors since the Cam Name inside a tiered group is not consistent");
        }

        [TestMethod]
        public void PMapImport_CamName_IsConsistent()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023",
                    Category = "ENG",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023b",
                    Category = "Pilots",
                    ClassOfCost =  ClassOfCost.Recurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Category = "PP&C",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    WbsNumber = "4.01.06.01",
                    ActivityID = "BSW1023c",
                    Category = "PP&C",
                    ClassOfCost =  ClassOfCost.NonRecurring.GetDescription()
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            Dictionary<string, ICollection<ProjectMapModelView>> tieredData = ProjectMapConverter.GroupByTiers(projectMapRows);
            sut.ValidateCamNameIsConsistent(tieredData);
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since the Cam Name inside a tiered group is consistent");
        }

        [TestMethod]
        public void PMapImport_ActIdWbsCategory_AdtlTest()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    WbsNumber = "4.01.06.01",
                    WbsElementTitle = "aaa",
                    Category = "ENG",
                    InitialResource = "bbb",
                    CostCenter = "ccc"
                },
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    WbsNumber = "4.01.06.01",
                    WbsElementTitle = "aaa",
                    Category = "ENG",
                    InitialResource = "bbb",
                    CostCenter = "ccc1"
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdWbsCategoryIsUnique();
            Assert.AreEqual(0, sut.ValidationMessages.Count);

            projectMapRows = new ProjectMapModelView[]
            {
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    WbsNumber = "4.01.06.01",
                    WbsElementTitle = "aaa",
                    Category = "ENG",
                    InitialResource = "bbb",
                    CostCenter = "ccc"
                },
                new ProjectMapModelView
                {
                    ActivityID = "BSW1023",
                    WbsNumber = "4.01.06.01",
                    WbsElementTitle = "aaa",
                    Category = "ENG",
                    InitialResource = "bbb",
                    CostCenter = "ccc"
                }
            };

            sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateActIdWbsCategoryIsUnique();
            Assert.AreEqual(2, sut.ValidationMessages.Count);
        }

        [TestMethod]
        public void PMapImport_ProjectLength_LongerThan17Years()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    ActivityID = "BSW1023",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    ActivityID = "BSW1022",
                    StartDate = DateTime.Parse("1/16/2019"),
                    EndDate = DateTime.Parse("1/15/2037")
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    ActivityID = "BSW1048",
                    StartDate = DateTime.Parse("1/15/2020"),
                    EndDate = DateTime.Parse("2/1/2025")
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace { ProjectMapType = ProjectMapType.TimePhasedProjectMap });
            sut.ValidateProjectLengthLessThanMaxYears();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate one error since the earliest start date and the latest end date span more than 17 years");
        }

        [TestMethod]
        public void PMapImport_ProjectLength_17YearsOrLess()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    ActivityID = "BSW1023",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    ActivityID = "BSW1022",
                    StartDate = DateTime.Parse("1/16/2019"),
                    EndDate = DateTime.Parse("1/15/2035")
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    ActivityID = "BSW1048",
                    StartDate = DateTime.Parse("1/15/2020"),
                    EndDate = DateTime.Parse("2/1/2025")
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace { ProjectMapType = ProjectMapType.TimePhasedProjectMap });
            sut.ValidateProjectLengthLessThanMaxYears();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since the earliest start date and the latest end date span 17 years or less");
        }

        [TestMethod]
        public void PMapImport_ProjectLength_LongerThan45Years()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    ActivityID = "BSW1023",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    ActivityID = "BSW1022",
                    StartDate = DateTime.Parse("1/16/2019"),
                    EndDate = DateTime.Parse("1/15/2077")
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    ActivityID = "BSW1048",
                    StartDate = DateTime.Parse("1/15/2020"),
                    EndDate = DateTime.Parse("2/1/2025")
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace { ProjectMapType = ProjectMapType.NonTimePhasedProjectMap });
            sut.ValidateProjectLengthLessThanMaxYears();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate one error since the earliest start date and the latest end date span more than 45 years");
        }

        [TestMethod]
        public void PMapImport_ProjectLength_45YearsOrLess()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    ActivityID = "BSW1023",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019")
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    ActivityID = "BSW1022",
                    StartDate = DateTime.Parse("1/16/2019"),
                    EndDate = DateTime.Parse("1/15/2063")
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    ActivityID = "BSW1048",
                    StartDate = DateTime.Parse("1/15/2020"),
                    EndDate = DateTime.Parse("2/1/2025")
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace { ProjectMapType = ProjectMapType.NonTimePhasedProjectMap });
            sut.ValidateProjectLengthLessThanMaxYears();
            Assert.AreEqual(1, sut.ValidationMessages.Count);
            Assert.AreEqual("You loaded more than 17 years of data. Reports which list year by year data will not work correctly, as those support maximum of 17 years.",
                sut.ValidationMessages[0].ValidationIssue);
        }

        [TestMethod]
        public void PMapImport_EndDate_Before_StartDate()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    StartDate = DateTime.Parse("2/1/2019"),
                    EndDate = DateTime.Parse("1/15/2019"),
                    WbsNumber = "123",
                    ActivityID = "123",
                    ActivityName = "123",
                    WbsElementTitle = "123",
                    InitialResource = "123",
                    Hours = 1,
                    CostCenter = "123",
                    Clin = "123",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription(),
                    TieredPercentage = 7.8m
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    StartDate = DateTime.Parse("1/16/2019"),
                    EndDate = DateTime.Parse("1/15/2036"),
                    WbsNumber = "123",
                    ActivityID = "123",
                    ActivityName = "123",
                    WbsElementTitle = "123",
                    InitialResource = "123",
                    Hours = 1,
                    CostCenter = "123",
                    Clin = "123",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    StartDate = DateTime.Parse("1/15/2025"),
                    EndDate = DateTime.Parse("2/1/2020"),
                    WbsNumber = "123",
                    ActivityID = "123",
                    ActivityName = "123",
                    WbsElementTitle = "123",
                    InitialResource = "123",
                    Hours = 1,
                    CostCenter = "123",
                    Clin = "123",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(2, sut.ValidationMessages.Count,
                "Validator should generate two errors since two rows have End Dates before the associated Start Date");
        }

        [TestMethod]
        public void PMapImport_EndDate_After_StartDate()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019"),
                    WbsNumber = "123",
                    ActivityID = "123",
                    ActivityName = "123",
                    WbsElementTitle = "123",
                    InitialResource = "123",
                    Hours = 1,
                    CostCenter = "123",
                    Clin = "123",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
                }, //valid
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    StartDate = DateTime.Parse("1/16/2019"),
                    EndDate = DateTime.Parse("1/15/2036"),
                    WbsNumber = "123",
                    ActivityID = "123",
                    ActivityName = "123",
                    WbsElementTitle = "123",
                    InitialResource = "123",
                    Hours = 1,
                    CostCenter = "123",
                    Clin = "123",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
                }, //valid
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    StartDate = DateTime.Parse("1/15/2020"),
                    EndDate = DateTime.Parse("2/1/2025"),
                    WbsNumber = "123",
                    ActivityID = "123",
                    ActivityName = "123",
                    WbsElementTitle = "123",
                    InitialResource = "123",
                    Hours = 1,
                    CostCenter = "123",
                    Clin = "123",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
                } //valid
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since all Start Dates are before the associated End Date");
        }

        [TestMethod]
        public void PMapImport_StartDate_EqualTo_EndDate()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    StartDate = DateTime.Parse("2/1/2019"),
                    EndDate = DateTime.Parse("2/1/2019"),
                    WbsNumber = "123",
                    ActivityID = "123",
                    ActivityName = "123",
                    WbsElementTitle = "123",
                    InitialResource = "123",
                    Hours = 1,
                    CostCenter = "123",
                    Clin = "123",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
                }, //invalid
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    StartDate = DateTime.Parse("1/16/2019"),
                    EndDate = DateTime.Parse("1/15/2036"),
                    WbsNumber = "123",
                    ActivityID = "123",
                    ActivityName = "123",
                    WbsElementTitle = "123",
                    InitialResource = "123",
                    Hours = 1,
                    CostCenter = "123",
                    Clin = "123",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
                }, //valid
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    StartDate = DateTime.Parse("2/1/2020"),
                    EndDate = DateTime.Parse("2/1/2020"),
                    WbsNumber = "123",
                    ActivityID = "123",
                    ActivityName = "123",
                    WbsElementTitle = "123",
                    InitialResource = "123",
                    Hours = 1,
                    CostCenter = "123",
                    Clin = "123",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
                } //invalid
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            // TODO DUSAN Look into showing warnings for start/end date on the same Month
            // Assert.AreEqual(2, sut.ValidationMessages.Count, "Validator should generate two errors since two rows have Start and End dates on the same day");
        }

        [TestMethod]
        public void PMapImport_StartDate_Before_EndDate()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    StartDate = DateTime.Parse("1/15/2019"),
                    EndDate = DateTime.Parse("2/1/2019"),
                    WbsNumber = "123",
                    ActivityID = "123",
                    ActivityName = "123",
                    WbsElementTitle = "123",
                    InitialResource = "123",
                    Hours = 1,
                    CostCenter = "123",
                    Clin = "123",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    StartDate = DateTime.Parse("1/16/2019"),
                    EndDate = DateTime.Parse("1/15/2036"),
                    WbsNumber = "123",
                    ActivityID = "123",
                    ActivityName = "123",
                    WbsElementTitle = "123",
                    InitialResource = "123",
                    Hours = 1,
                    CostCenter = "123",
                    Clin = "123",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
                },
                new ProjectMapModelView
                {
                    CamName = "3-1",
                    StartDate = DateTime.Parse("1/15/2020"),
                    EndDate = DateTime.Parse("2/1/2025"),
                    WbsNumber = "123",
                    ActivityID = "123",
                    ActivityName = "123",
                    WbsElementTitle = "123",
                    InitialResource = "123",
                    Hours = 1,
                    CostCenter = "123",
                    Clin = "123",
                    ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
                }
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since all Start Dates and End Dates are on different days");
        }

        [TestMethod]
        public void PMapImport_HoursAndDollars_NullOrZero()
        {
            ProjectMapModelView[] projectMapRows =
            {
                PopulateRequiredPropsForHoursAndDollarsTest(null, null), //invalid
                PopulateRequiredPropsForHoursAndDollarsTest(0, null), //invalid
                PopulateRequiredPropsForHoursAndDollarsTest(null, 0), //invalid
                PopulateRequiredPropsForHoursAndDollarsTest(0, 0), //invalid
                PopulateRequiredPropsForHoursAndDollarsTest(1, 0), //valid
                PopulateRequiredPropsForHoursAndDollarsTest(0, 1), //valid
                PopulateRequiredPropsForHoursAndDollarsTest(null, 1), //valid
                PopulateRequiredPropsForHoursAndDollarsTest(1, null), //valid
                PopulateRequiredPropsForHoursAndDollarsTest(1, 1) //invalid
            };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(5, sut.ValidationMessages.Count,
                "Validator should generate 5 errors since 5 out of 9 records are either both 0/null or both not 0/null");
        }

        private static ProjectMapModelView PopulateRequiredPropsForHoursAndDollarsTest(decimal? hours, decimal? dollars)
        {
            return new ProjectMapModelView
            {
                CamName = "1-1",
                Hours = hours,
                Dollars = dollars,
                StartDate = DateTime.Parse("1/15/2019"),
                EndDate = DateTime.Parse("2/1/2019"),
                WbsNumber = "123",
                ActivityID = "123",
                ActivityName = "123",
                WbsElementTitle = "123",
                InitialResource = "123",
                CostCenter = "123",
                Clin = "123",
                ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
            };
        }

        [TestMethod]
        public void PMapImport_WbsNumber_Missing()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.WbsNumber = null;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error since required attribute WbsNumber is null");
        }

        [TestMethod]
        public void PMapImport_ActivityID_Missing()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.ActivityID = null;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                $"Validator should generate 1 error since required attribute {nameof(ProjectMapModelView.ActivityID)} is null");
        }

        [TestMethod]
        public void PMapImport_ActivityName_Missing()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.ActivityName = null;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                $"Validator should generate 1 error since required attribute {nameof(ProjectMapModelView.ActivityName)} is null");
        }

        [TestMethod]
        public void PMapImport_WbsElementTitle_Missing()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.WbsElementTitle = null;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                $"Validator should generate 1 error since required attribute {nameof(ProjectMapModelView.WbsElementTitle)} is null");
        }

        [TestMethod]
        public void PMapImport_InitialResource_Missing()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.InitialResource = null;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                $"Validator should generate 1 error since required attribute {nameof(ProjectMapModelView.InitialResource)} is null");
        }

        [TestMethod]
        public void PMapImport_StartDate_Missing()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.StartDate = null;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                $"Validator should generate 1 error since required attribute {nameof(ProjectMapModelView.StartDate)} is null");
            Assert.AreEqual("Start Date cannot be blank and must be valid", sut.ValidationMessages.FirstOrDefault().ValidationIssue,
                "Error message is different than expected.");
        }

        [TestMethod]
        public void PMapImport_CostCenter_Missing()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.CostCenter = null;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                $"Validator should generate 1 error since required attribute {nameof(ProjectMapModelView.CostCenter)} is null");
            Assert.AreEqual("Cost Center is required.", sut.ValidationMessages.FirstOrDefault().ValidationIssue,
                "Error message is different than expected.");
        }

        [TestMethod]
        public void PMapImport_Clin_Missing()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.Clin = null;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                $"Validator should generate 1 error since required attribute {nameof(ProjectMapModelView.Clin)} is null");
            Assert.AreEqual("CLIN is required.", sut.ValidationMessages.FirstOrDefault().ValidationIssue,
                "Error message is different than expected.");
        }

        [TestMethod]
        public void PMapImport_EndDate_Missing()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.EndDate = null;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.RunModelValidation();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                $"Validator should generate 1 error since required attribute {nameof(ProjectMapModelView.EndDate)} is null");
            Assert.AreEqual("End Date cannot be blank and must be valid", sut.ValidationMessages.FirstOrDefault().ValidationIssue,
                "Error message is different than expected.");
        }

        private ProjectMapModelView GetValidProjectMapModelView()
        {
            return new ProjectMapModelView
            {
                EndDate = DateTime.Parse("2/2/2019"),
                Hours = 1,
                StartDate = DateTime.Parse("2/1/2019"),
                WbsNumber = "123",
                ActivityID = "123",
                ActivityName = "123",
                WbsElementTitle = "123",
                InitialResource = "123",
                CostCenter = "123",
                Clin = "123",
                ClassOfCost = ClassOfCost.NonRecurring.ToDescription()
            };
        }

        [TestMethod]
        public void PMapImport_Invalid_Resource()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.InitialResource = "456";
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateInitialResourceExistsInResourceTable();

            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error since resource does not exist in resource table");
        }

        [TestMethod]
        public void PMapImport_Invalid_Resource_Nonlabor()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.InitialResource = "S2";
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateInitialResourceExistsInResourceTable();

            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error since resource exists in resource table but is not the correct Element Cost type (it is non-Labor)");
        }

        [TestMethod]
        public void PMapImport_Valid_Resource()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.InitialResource = "R";
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateInitialResourceExistsInResourceTable();

            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since resource exists in resource table");
        }

        /// <summary>
        /// Tests for an invalid hours precision.
        /// </summary>
        [TestMethod]
        public void PMapImport_Invalid_Hours_Precision()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.Hours = 3.2478645m;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidatePrecision();

            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error for invalid hours precision");
        }

        /// <summary>
        /// Tests for an invalid dollars precision.
        /// </summary>
        [TestMethod]
        public void PMapImport_Invalid_Dollars_Precision()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.Hours = null;
            projectMapModelViewToTest.InitialResource = "S";
            projectMapModelViewToTest.Dollars = 3.2478645m;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidatePrecision();

            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error for invalid dollars precision");
        }

        /// <summary>
        /// Tests for an invalid discrete month spread decimal precision.
        /// </summary>
         [TestMethod]
        public void PMapImport_Invalid_Hours_Spread_Precision()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.Hours = 10m;
            projectMapModelViewToTest.DiscreteMonths = new decimal?[204];
            projectMapModelViewToTest.DiscreteMonths[0] = 9.99999m;
            projectMapModelViewToTest.DiscreteMonths[0] = 0.00001m;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace { ProjectMapType = ProjectMapType.TimePhasedProjectMap });
            sut.ValidatePrecision();

            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 2 errors for invalid spreads precision");
        }

        /// <summary>
        /// Tests a valid tiered percentage value.
        /// </summary>
        [TestMethod]
        public void PMapImport_Valid_TieredPercentage_Value()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.TieredPercentage = 3.2m;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidatePrecision();

            Assert.AreEqual(0, sut.ValidationMessages.Count);
        }

        /// <summary>
        /// Tests for an invalid tiered percentage value out-of-range.
        /// </summary>
        [TestMethod]
        public void PMapImport_Invalid_TieredPercentage_Value()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.TieredPercentage = 1000.0m;
            ProjectMapModelView[] projectMapRowsTooLarge = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRowsTooLarge, new FullWorkspace());
            sut.ValidatePrecision();

            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error for invalid tiered percentage value too large");

            projectMapModelViewToTest.TieredPercentage = -1.2m;
            ProjectMapModelView[] projectMapRowsNegative = { projectMapModelViewToTest };

            sut = new ProjectMapValidator(projectMapRowsNegative, new FullWorkspace());
            sut.ValidatePrecision();

            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error for invalid negative tiered percentage value");
        }

        /// <summary>
        /// Tests for an invalid tiered percentage precision.
        /// </summary>
        [TestMethod]
        public void PMapImport_Invalid_TieredPercentage_Precision()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.TieredPercentage = 3.2478645m;
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidatePrecision();

            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error for invalid tiered percentage precision");
        }

        [TestMethod]
        public void PMapImport_Invalid_CostCenter()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.CostCenter = "456";
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateCostCenterExistsInPerformingOrgTable();

            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error since resource does not exist in resource table");
        }

        [TestMethod]
        public void PMapImport_Valid_CostCenter()
        {
            ProjectMapModelView projectMapModelViewToTest = this.GetValidProjectMapModelView();
            projectMapModelViewToTest.CostCenter = "P";
            ProjectMapModelView[] projectMapRows = { projectMapModelViewToTest };

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, new FullWorkspace());
            sut.ValidateCostCenterExistsInPerformingOrgTable();

            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since resource exists in resource table");
        }

        [TestMethod]
        public void PMapImport_Valid_Hours_Costs_InitialResource()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    WbsElementTitle = "123",
                    ActivityID = "BSW1023",
                    Hours = 1,
                    InitialResource = "R"
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01",
                    WbsElementTitle = "123",
                    ActivityID = "BSW1023",
                    Dollars = 1,
                    InitialResource = "S"
                }
            };

            FullWorkspace ws = new FullWorkspace();
            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateHoursCostsAreValidResourceType();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate no errors since the hours and costs are associated with the correct resource rate types");
        }

        [TestMethod]
        public void PMapImport_InValid_Hours_Costs_InitialResource()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    WbsElementTitle = "123",
                    ActivityID = "BSW1023",
                    Hours = 1,
                    InitialResource = "S"
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01",
                    WbsElementTitle = "123",
                    ActivityID = "BSW1023",
                    Dollars = 1,
                    InitialResource = "R"
                },
                new ProjectMapModelView
                {
                    CamName = "1-1",
                    WbsNumber = "4.01.06.01",
                    WbsElementTitle = "123",
                    ActivityID = "BSW1023",
                    Dollars = 0,
                    InitialResource = "S"
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01",
                    WbsElementTitle = "123",
                    ActivityID = "BSW1023",
                    Hours = 0,
                    InitialResource = "R"
                },
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01",
                    WbsElementTitle = "123",
                    ActivityID = "BSW1023",
                    Hours = 0,
                    InitialResource = "X"
                }
            };

            FullWorkspace ws = new FullWorkspace();
            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateHoursCostsAreValidResourceType();
            Assert.AreEqual(2, sut.ValidationMessages.Count,
                "Validator should generate 2 errors since the combinations of hours, costs, and resources are incorrect");
        }

        [TestMethod]
        public void PMapImport_Valid_Project_Map_Type_Has_Valid_Bucketized_Data()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01",
                    WbsElementTitle = "123",
                    ActivityID = "BSW1023",
                    Dollars = 1,
                    InitialResource = "S",
                    DiscreteMonths = new decimal?[204]
                }
            };

            FullWorkspace ws = new FullWorkspace();
            ws.ProjectMapType = ProjectMapType.TimePhasedProjectMap;

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateProjectMapTypeHasValidBucketizedData();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate 0 errors since the workspace has time phased project map with bucketized data.");

            projectMapRows[0].DiscreteMonths = null;
            ws.ProjectMapType = ProjectMapType.NonTimePhasedProjectMap;

            sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateProjectMapTypeHasValidBucketizedData();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate 0 errors since the workspace is not time phased project map and has no bucketized data.");

            ws.ProjectMapType = ProjectMapType.StandardWithOffload;

            sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateProjectMapTypeHasValidBucketizedData();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate 0 errors since the workspace is not time phased project map and has no bucketized data.");

            ws.ProjectMapType = ProjectMapType.StandardWithoutOffload;

            sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateProjectMapTypeHasValidBucketizedData();
            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate 0 errors since the workspace is not time phased project map and has no bucketized data.");

        }

        [TestMethod]
        public void PMapImport_InValid_Project_Map_Type_Has_Valid_Bucketized_Data()
        {
            ProjectMapModelView[] projectMapRows =
            {
                new ProjectMapModelView
                {
                    CamName = "2-1",
                    WbsNumber = "4.01",
                    WbsElementTitle = "123",
                    ActivityID = "BSW1023",
                    Dollars = 1,
                    InitialResource = "S",
                    DiscreteMonths = null
                }
            };

            FullWorkspace ws = new FullWorkspace();
            ws.ProjectMapType = ProjectMapType.TimePhasedProjectMap;

            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateProjectMapTypeHasValidBucketizedData();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error since the workspace has time phased project map and has no bucketized data.");

            projectMapRows[0].DiscreteMonths = new decimal?[204];
            ws.ProjectMapType = ProjectMapType.NonTimePhasedProjectMap;

            sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateProjectMapTypeHasValidBucketizedData();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error since the workspace is not time phased project map and has bucketized data.");

            ws.ProjectMapType = ProjectMapType.StandardWithOffload;

            sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateProjectMapTypeHasValidBucketizedData();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error since the workspace is not time phased project map and has bucketized data.");

            ws.ProjectMapType = ProjectMapType.StandardWithoutOffload;

            sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateProjectMapTypeHasValidBucketizedData();
            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 error since the workspace is not time phased project map and has bucketized data.");

        }

        [TestMethod]
        public void PMapImport_Valid_Bucketized_Spreads_Start_End_Dates()
        {
            var projectMapRows = new[]
                {this.GetValidProjectMapModelView(),this.GetValidProjectMapModelView()};

            projectMapRows[0].StartDate = DateTime.Parse("11/02/2018");
            projectMapRows[0].EndDate = DateTime.Parse("12/24/2019");
            projectMapRows[0].DiscreteMonths = new decimal?[204];
 
            for (int i = 10; i < 24; i++) // test the minimum and maximum bounds
            {
                projectMapRows[0].DiscreteMonths[i] = i;
            }

            projectMapRows[1].StartDate = DateTime.Parse("09/02/2018");
            projectMapRows[1].EndDate = DateTime.Parse("11/24/2028");
            projectMapRows[1].DiscreteMonths = new decimal?[204];

            for (int i = 9; i < 15; i++)
            {
                projectMapRows[1].DiscreteMonths[i] = i;
            }
 
            FullWorkspace ws = new FullWorkspace();
            ws.ProjectMapType = ProjectMapType.TimePhasedProjectMap;
            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateBucketizedSpreadsStartEndDates();

            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate 0 errors since the workspace is time phased project map with valid spreads.");


            projectMapRows[0].StartDate = DateTime.Parse("01/02/2018");
            projectMapRows[0].EndDate = DateTime.Parse("12/24/2034");
            projectMapRows[0].InitialResource = "S";
            projectMapRows[0].Dollars = null;
            projectMapRows[0].DiscreteMonths = new decimal?[204];

            for (int i = 0; i < 204; i++)
            {
                projectMapRows[0].DiscreteMonths[i] = i;
            }

            ws = new FullWorkspace();
            ws.ProjectMapType = ProjectMapType.TimePhasedProjectMap;
            sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateBucketizedSpreadsStartEndDates();

            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate 0 errors since the workspace is time phased project map with valid min/max spreads.");

        }

        [TestMethod]
        public void PMapImport_InValid_Bucketized_Spreads_Start_End_Dates()
        {
            var projectMapRows = new[]
                {this.GetValidProjectMapModelView(),
                this.GetValidProjectMapModelView(),
                this.GetValidProjectMapModelView()};

            projectMapRows[0].StartDate = DateTime.Parse("03/02/2018");
            projectMapRows[0].EndDate = DateTime.Parse("04/24/2018");
            projectMapRows[0].DiscreteMonths = new decimal?[204];

            projectMapRows[0].DiscreteMonths[0] = 1;
            projectMapRows[0].DiscreteMonths[1] = 2;
            projectMapRows[0].DiscreteMonths[2] = 3;

            projectMapRows[1].StartDate = DateTime.Parse("02/02/2018");
            projectMapRows[1].EndDate = DateTime.Parse("02/24/2020");
            projectMapRows[1].DiscreteMonths = new decimal?[204];
            projectMapRows[1].DiscreteMonths[0] = 1;
            projectMapRows[1].DiscreteMonths[25] = 26;
            projectMapRows[1].DiscreteMonths[26] = 27;
            projectMapRows[1].DiscreteMonths[203] = 204;

            FullWorkspace ws = new FullWorkspace();
            ws.ProjectMapType = ProjectMapType.TimePhasedProjectMap;
            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateBucketizedSpreadsStartEndDates();

            Assert.AreEqual(5, sut.ValidationMessages.Count,
                "Validator should generate 5 errors since the workspace is time phased project map with invalid spreads.");

            projectMapRows = new[]
                {this.GetValidProjectMapModelView(),this.GetValidProjectMapModelView()};

            projectMapRows[0].StartDate = DateTime.Parse("11/02/2018");
            projectMapRows[0].EndDate = DateTime.Parse("12/24/2019");
            projectMapRows[0].DiscreteMonths = new decimal?[204];

            for (int i = 9; i < 25; i++) // test the minimum and maximum bounds
            {
                projectMapRows[0].DiscreteMonths[i] = i;
            }

            projectMapRows[1].StartDate = DateTime.Parse("09/02/2018");
            projectMapRows[1].EndDate = DateTime.Parse("11/24/2028");
            projectMapRows[1].DiscreteMonths = new decimal?[204];

            sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateBucketizedSpreadsStartEndDates();

            Assert.AreEqual(2, sut.ValidationMessages.Count,
                "Validator should generate 2 errors since the workspace is time phased project map with invalid spreads.");

            projectMapRows = new[]
            {this.GetValidProjectMapModelView()};

            projectMapRows[0].StartDate = DateTime.Parse("01/02/2018");
            projectMapRows[0].EndDate = DateTime.Parse("02/24/2035");
            projectMapRows[0].InitialResource = "S";
            projectMapRows[0].Dollars = null;
            projectMapRows[0].DiscreteMonths = new decimal?[204];

            sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateBucketizedSpreadsStartEndDates();

            Assert.AreEqual(1, sut.ValidationMessages.Count,
                "Validator should generate 1 errors since the start and end dates are more than 17 years apart.");

        }

        [TestMethod]
        public void PMapImport_Valid_Bucketized_Spreads_Hours_Costs()
        {
            ProjectMapModelView[] projectMapRows =
            {
                this.GetValidProjectMapModelView(),
                this.GetValidProjectMapModelView(),
                this.GetValidProjectMapModelView()
            };

            projectMapRows[0].StartDate = DateTime.Parse("01/02/2018");
            projectMapRows[0].EndDate = DateTime.Parse("02/24/2018");
            projectMapRows[0].InitialResource = "R";
            projectMapRows[0].Hours = 78;
            projectMapRows[0].DiscreteMonths = new decimal?[204];

            for (int i = 0; i < 12; i++)
            {
                projectMapRows[0].DiscreteMonths[i] = i + 1;
            }

            projectMapRows[1].StartDate = DateTime.Parse("01/02/2018");
            projectMapRows[1].EndDate = DateTime.Parse("02/24/2028");
            projectMapRows[1].InitialResource = "S";
            projectMapRows[1].Dollars = 21;
            projectMapRows[1].DiscreteMonths = new decimal?[204];

            for (int i = 0; i < 6; i++)
            {
                projectMapRows[1].DiscreteMonths[i] = i + 1;
            }

            projectMapRows[2].StartDate = DateTime.Parse("01/02/2018");
            projectMapRows[2].EndDate = DateTime.Parse("02/24/2028");
            projectMapRows[2].InitialResource = "R";
            projectMapRows[2].Hours = 6;
            projectMapRows[2].DiscreteMonths = new decimal?[204];

            projectMapRows[2].DiscreteMonths[22] = 1;
            projectMapRows[2].DiscreteMonths[34] = 1;
            projectMapRows[2].DiscreteMonths[46] = 1;
            projectMapRows[2].DiscreteMonths[58] = 1;
            projectMapRows[2].DiscreteMonths[70] = 1;
            projectMapRows[2].DiscreteMonths[82] = 1;

            FullWorkspace ws = new FullWorkspace();
            ws.ProjectMapType = ProjectMapType.TimePhasedProjectMap;
            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateBucketizedSpreadsHoursCosts();

            Assert.AreEqual(0, sut.ValidationMessages.Count,
                "Validator should generate 0 errors since the hours/costs are equal to sum of the spread.");
        }

        [TestMethod]
        public void PMapImport_InValid_Bucketized_Spreads_Hours_Costs()
        {
            ProjectMapModelView[] projectMapRows =
            {
                this.GetValidProjectMapModelView(),
                this.GetValidProjectMapModelView(),
                this.GetValidProjectMapModelView(),
                this.GetValidProjectMapModelView()
            };

            projectMapRows[0].StartDate = DateTime.Parse("01/02/2018");
            projectMapRows[0].EndDate = DateTime.Parse("02/24/2018");
            projectMapRows[0].InitialResource = "R";
            projectMapRows[0].Hours = 79;
            projectMapRows[0].DiscreteMonths = new decimal?[204];

            for (int i = 0; i < 12; i++)
            {
                projectMapRows[0].DiscreteMonths[i] = i + 1;
            }

            projectMapRows[1].StartDate = DateTime.Parse("01/02/2018");
            projectMapRows[1].EndDate = DateTime.Parse("02/24/2028");
            projectMapRows[1].InitialResource = "S";
            projectMapRows[1].Dollars = 20;
            projectMapRows[1].DiscreteMonths = new decimal?[204];

            for (int i = 0; i < 6; i++)
            {
                projectMapRows[1].DiscreteMonths[i] = i + 1;
            }

            projectMapRows[2].StartDate = DateTime.Parse("01/02/2018");
            projectMapRows[2].EndDate = DateTime.Parse("02/24/2028");
            projectMapRows[2].InitialResource = "S";
            projectMapRows[2].Dollars = 1;
            projectMapRows[2].DiscreteMonths = null;

            projectMapRows[3].StartDate = DateTime.Parse("01/02/2018");
            projectMapRows[3].EndDate = DateTime.Parse("02/24/2028");
            projectMapRows[3].InitialResource = "S";
            projectMapRows[3].Dollars = null;
            projectMapRows[3].DiscreteMonths = new decimal?[204];

            FullWorkspace ws = new FullWorkspace();
            ws.ProjectMapType = ProjectMapType.TimePhasedProjectMap;
            ProjectMapValidator sut = new ProjectMapValidator(projectMapRows, ws);
            sut.ValidateBucketizedSpreadsHoursCosts();

            Assert.AreEqual(2, sut.ValidationMessages.Count,
                "Validator should generate 2 errors since hours/costs are not equal to sum of the spread.");
        }
    }
}