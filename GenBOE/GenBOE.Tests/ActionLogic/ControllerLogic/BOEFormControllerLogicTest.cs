// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.Exceptions;
    using IES.Common.PickList;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BOEFormControllerLogicTest : MOQObject
    {

        private class Stubs
        {
            public FullWorkspace Workspace1 = new FullWorkspace() { Id = 1, ContractStartDate = new DateTime(2012, 6, 15), ContractEndDate = new DateTime(2015, 5, 15), IsUsingTM = true };

            public ResourceDTO Res1 = new ResourceDTO() { Id = 1, ResourceName = "Res1", ElementOfCost = ElementOfCostType.Sub, RateType = RateType.Hours };
            public ResourceDTO Res2 = new ResourceDTO() { Id = 2, ResourceName = "Res2", ElementOfCost = ElementOfCostType.IWTA, RateType = RateType.Hours };
            public ResourceDTO Res3 = new ResourceDTO() { Id = 3, ResourceName = "Res3", ElementOfCost = ElementOfCostType.Sub, RateType = RateType.Hours };
            public ResourceDTO Res4 = new ResourceDTO() { Id = 4, ResourceName = "Res4", ElementOfCost = ElementOfCostType.ODC, RateType = RateType.Hours };   // the T&M rates for this resource should not be validated
            public ResourceDTO Res5 = new ResourceDTO() { Id = 5, ResourceName = "Res5", ElementOfCost = ElementOfCostType.IWTA, RateType = RateType.Cost };   // the T&M rates for this resource should not be validated
            public ResourceDTO Res6 = new ResourceDTO() { Id = 6, ResourceName = "Res6", ElementOfCost = ElementOfCostType.Sub, RateType = RateType.Cost };    // the T&M rates for this resource should not be validated

            public TMResourceRateDTO Rr11;
            public TMResourceRateDTO Rr12;
            public TMResourceRateDTO Rr13;
            public TMResourceRateDTO Rr21;
            public TMResourceRateDTO Rr22;
            public TMResourceRateDTO Rr23;
            public TMResourceRateDTO Rr31;

            public Collection<TMResourceRateDTO> TmResourceRateCollection;
            public BoeTaskElementDTO TaskElement1 = new BoeTaskElementDTO();

            public BoeTaskElementDTO TaskElement2 = new BoeTaskElementDTO();

            public Collection<BoeTaskElementDTO> TaskElementCollection;
            public BOEFormIBOEDTO Iboe;
            public BOEFormPBOEDTO Pboe;
            public Collection<BOEFormIBOEDTO> IboeCollection;
            public Collection<BOEFormPBOEDTO> PboeCollection;

            public Collection<BOEFormIBOEDTO> BOEFormIboeDTOCollection;

            public Collection<BOEFormPBOEDTO> BOEFormPboeDTOCollection;

            public Stubs()
            {
                Workspace1 = new FullWorkspace()
                {
                    Id = 1,
                    ContractStartDate = new DateTime(2012, 6, 15),
                    ContractEndDate = new DateTime(2015, 5, 15),
                    IsUsingTM = true
                };

                Rr11 = new TMResourceRateDTO()
                {
                    Id = 1,
                    ResourceID = Res1.Id,
                    WorkspaceID = Workspace1.Id,
                    ResourceRate = 0.11m,
                    StartDate = new DateTime(2012, 6, 15),
                    EndDate = new DateTime(2013, 5, 15)
                };
                Rr12 = new TMResourceRateDTO()
                {
                    Id = 2,
                    ResourceID = Res1.Id,
                    WorkspaceID = Workspace1.Id,
                    ResourceRate = 0.22m,
                    StartDate = new DateTime(2013, 6, 15),
                    EndDate = new DateTime(2014, 5, 15)
                };
                Rr13 = new TMResourceRateDTO()
                {
                    Id = 3,
                    ResourceID = Res1.Id,
                    WorkspaceID = Workspace1.Id,
                    ResourceRate = 0.33m,
                    StartDate = new DateTime(2014, 6, 15),
                    EndDate = new DateTime(2015, 5, 15)
                };
                Rr21 = new TMResourceRateDTO()
                {
                    Id = 4,
                    ResourceID = Res2.Id,
                    WorkspaceID = Workspace1.Id,
                    ResourceRate = 2.11m,
                    StartDate = new DateTime(2012, 6, 15),
                    EndDate = new DateTime(2013, 5, 15)
                };
                Rr22 = new TMResourceRateDTO()
                {
                    Id = 5,
                    ResourceID = Res2.Id,
                    WorkspaceID = Workspace1.Id,
                    ResourceRate = 2.22m,
                    StartDate = new DateTime(2013, 6, 15),
                    EndDate = new DateTime(2014, 5, 15)
                };
                Rr23 = new TMResourceRateDTO()
                {
                    Id = 6,
                    ResourceID = Res2.Id,
                    WorkspaceID = Workspace1.Id,
                    ResourceRate = 2.33m,
                    StartDate = new DateTime(2014, 6, 15),
                    EndDate = new DateTime(2015, 5, 15)
                };
                Rr31 = new TMResourceRateDTO()
                {
                    Id = 7,
                    ResourceID = Res3.Id,
                    WorkspaceID = Workspace1.Id,
                    ResourceRate = 3.11m,
                    StartDate = new DateTime(2012, 6, 15),
                    EndDate = new DateTime(2015, 5, 15)
                };

                TmResourceRateCollection =
                    new Collection<TMResourceRateDTO>() {Rr11, Rr12, Rr13, Rr21, Rr22, Rr23, Rr31};

                TaskElement1 = new BoeTaskElementDTO()
                {
                    Id = 1,
                    StartDate = new DateTime(2012, 7, 15),
                    EndDate = new DateTime(2015, 5, 15),
                    TotalCost = 11111.0m,
                    TotalHours = 1000.0m,
                    taskElementLabors = new Collection<ResourceTypeDto>()
                    {
                        new ResourceTypeDto()
                        {
                            ResourceID = Res1.Id,
                            StartDateValue = new DateTime(2012, 7, 15),
                            EndDateValue = new DateTime(2014, 5, 15),
                            ValueSpread = 700.0m,
                            SpreadType = SpreadType.Hours,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 7, 15),
                                    LaborSpreadValue = 100
                                },
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 8, 15),
                                    LaborSpreadValue = 200
                                },
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2013, 9, 15),
                                    LaborSpreadValue = 100
                                },
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2014, 7, 15),
                                    LaborSpreadValue = 300
                                }
                            }
                        },
                        new ResourceTypeDto()
                        {
                            ResourceID = Res2.Id,
                            StartDateValue = new DateTime(2013, 6, 15),
                            EndDateValue = new DateTime(2015, 5, 15),
                            ValueSpread = 300.0m,
                            SpreadType = SpreadType.Hours,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2014, 1, 15),
                                    LaborSpreadValue = 50
                                },
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2014, 2, 15),
                                    LaborSpreadValue = 100
                                },
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2015, 5, 15),
                                    LaborSpreadValue = 150
                                }
                            }
                        },
                        new ResourceTypeDto()
                        {
                            ResourceID = Res6.Id,
                            StartDateValue = new DateTime(2012, 7, 15),
                            EndDateValue = new DateTime(2013, 5, 15),
                            ValueSpread = 11111.0m,
                            SpreadType = SpreadType.Cost,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 7, 15),
                                    LaborSpreadValue = 11111.0m
                                }
                            }
                        }
                    }
                };

                TaskElement2 = new BoeTaskElementDTO()
                {
                    Id = 2,
                    StartDate = new DateTime(2012, 9, 15),
                    EndDate = new DateTime(2012, 11, 15),
                    TotalCost = 222.0m,
                    TotalHours = 123.0m,
                    taskElementLabors = new Collection<ResourceTypeDto>()
                    {
                        new ResourceTypeDto()
                        {
                            ResourceID = Res1.Id,
                            StartDateValue = new DateTime(2012, 9, 15),
                            EndDateValue = new DateTime(2012, 11, 15),
                            ValueSpread = 100.0m,
                            SpreadType = SpreadType.Hours,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 9, 15),
                                    LaborSpreadValue = 50
                                },
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 10, 15),
                                    LaborSpreadValue = 23
                                },
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 11, 15),
                                    LaborSpreadValue = 27
                                }
                            }
                        },
                        new ResourceTypeDto()
                        {
                            ResourceID = Res2.Id,
                            StartDateValue = new DateTime(2012, 9, 15),
                            EndDateValue = new DateTime(2012, 11, 15),
                            ValueSpread = 20.0m,
                            SpreadType = SpreadType.Hours,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 9, 15),
                                    LaborSpreadValue = 5
                                },
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 10, 15),
                                    LaborSpreadValue = 6
                                },
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 11, 15),
                                    LaborSpreadValue = 9
                                }
                            }
                        },
                        new ResourceTypeDto()
                        {
                            ResourceID = Res3.Id,
                            StartDateValue = new DateTime(2012, 9, 15),
                            EndDateValue = new DateTime(2012, 11, 15),
                            ValueSpread = 3.0m,
                            SpreadType = SpreadType.Hours,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 9, 15),
                                    LaborSpreadValue = 1
                                },
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 10, 15),
                                    LaborSpreadValue = 1
                                },
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 11, 15),
                                    LaborSpreadValue = 1
                                }
                            }
                        },
                        new ResourceTypeDto()
                        {
                            ResourceID = Res5.Id,
                            StartDateValue = new DateTime(2012, 7, 15),
                            EndDateValue = new DateTime(2013, 5, 15),
                            ValueSpread = 222.0m,
                            SpreadType = SpreadType.Cost,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto()
                                {
                                    LaborSpreadDate = new DateTime(2012, 7, 15),
                                    LaborSpreadValue = 222.0m
                                }
                            }
                        }
                    }
                };

                TaskElementCollection = new Collection<BoeTaskElementDTO>() {TaskElement1, TaskElement2};

                Iboe = new BOEFormIBOEDTO()
                {
                    Id = 111,
                    FormName = "IBOE1",
                    ResourceIds = new Collection<int>() {Res2.Id, Res5.Id}
                };
                Pboe = new BOEFormPBOEDTO()
                {
                    Id = 222,
                    FormName = "PBOE2",
                    ResourceIds = new Collection<int>() {Res1.Id, Res3.Id, Res4.Id, Res6.Id}
                };
                IboeCollection = new Collection<BOEFormIBOEDTO>() {Iboe};
                PboeCollection = new Collection<BOEFormPBOEDTO>() {Pboe};

                BOEFormIboeDTOCollection = new Collection<BOEFormIBOEDTO>()
                {
                    new BOEFormIBOEDTO {Id = 1, FormName = "a", Revision = 1},
                    new BOEFormIBOEDTO {Id = 2, FormName = "B", Revision = 3},
                    new BOEFormIBOEDTO {Id = 3, FormName = "c", Revision = 5},
                    new BOEFormIBOEDTO {Id = 4, FormName = "c", Revision = 7}
                };

                BOEFormPboeDTOCollection = new Collection<BOEFormPBOEDTO>()
                {
                    new BOEFormPBOEDTO {Id = 1, FormName = "a", Revision = 2},
                    new BOEFormPBOEDTO {Id = 2, FormName = "a b", Revision = 4},
                    new BOEFormPBOEDTO {Id = 23, FormName = "af af", Revision = 6},
                    new BOEFormPBOEDTO {Id = 999, FormName = "axe", Revision = 8}
                };
            }



        }
        private Stubs StubbedData; 

        private Mock<IRetriever> retriever = new Mock<IRetriever>();
        private Mock<IUserDTODataLoader> userLoader = new Mock<IUserDTODataLoader>();
        private Mock<IPermissionsDTODataLoader> permissionsLoader = new Mock<IPermissionsDTODataLoader>();
        private Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
        private Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();
        private Mock<IBOEFormIBOEDTODataLoader> iboeFormDataLoader = new Mock<IBOEFormIBOEDTODataLoader>(MockBehavior.Loose);
        private Mock<IBOEFormPBOEDTODataLoader> pboeFormDataLoader = new Mock<IBOEFormPBOEDTODataLoader>(MockBehavior.Loose);
        private Mock<IResourceDTODataLoader> resourceDTODataLoader = new Mock<IResourceDTODataLoader>();
        private Mock<ITMResourceRateDTODataLoader> tmResourceRateDTODataLoader = new Mock<ITMResourceRateDTODataLoader>();
        Mock<TMCalculator> _TMCalculator;
        Mock<IBOEFormExporter> iboeExporter;
        Mock<PBOEFormExporter> pboeExporter;
        BOEFormControllerLogic sut;

        [TestInitialize]
        public void CreateSystem()
        {
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            _TMCalculator = new Mock<TMCalculator>();
            iboeExporter = new Mock<IBOEFormExporter>(userLoader.Object, resourceDTODataLoader.Object, _TMCalculator.Object);
            pboeExporter = new Mock<PBOEFormExporter>(userLoader.Object, resourceDTODataLoader.Object, _TMCalculator.Object);

            StubbedData = new Stubs();

            sut = new BOEFormControllerLogic(iboeFormDataLoader.Object, pboeFormDataLoader.Object, resourceDTODataLoader.Object, tmResourceRateDTODataLoader.Object, iboeExporter.Object, pboeExporter.Object, _TMCalculator.Object);
        }

        #region ExportBOEFormReport Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExportBOEFormReport_TestArgumentException_workspace()
        {
            sut.ExportBOEFormReport(null, 0, BOEFormType.IBOE, false, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExportBOEFormReport_TestArgumentException_boeFormId()
        {
            FullWorkspace workspace = new FullWorkspace();
            sut.ExportBOEFormReport(workspace, 0, BOEFormType.IBOE, false, null);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        public void ExportBOEFormReport_TestArgumentException_boeFormType()
        {
            FullWorkspace workspace = new FullWorkspace();
            sut.ExportBOEFormReport(workspace, 1, BOEFormType.NotSet, false, null);
        }

        [TestMethod ]
        public void ExportBOEFormReport_Test()
        {

            iboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns( new BOEFormIBOEDTO() { Id = 1, FormName = "1" });
            iboeExporter.Setup(x => x.ExportToWordFile(It.IsAny<FullWorkspace>(), It.IsAny<string>(), It.IsAny<BOEFormIBOEDTO>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<ICollection<PickListDto>>())).Verifiable("failed");

            pboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(new BOEFormPBOEDTO() { Id = 1, FormName = "1" });
            pboeExporter.Setup(x => x.ExportToWordFile(It.IsAny<FullWorkspace>(), It.IsAny<string>(), It.IsAny<BOEFormPBOEDTO>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<ICollection<PickListDto>>())).Verifiable("failed");

            FullWorkspace workspace = new FullWorkspace();
            sut.ExportBOEFormReport(workspace, 1, BOEFormType.IBOE, It.IsAny<bool>(), null);
            sut.ExportBOEFormReport(workspace, 1, BOEFormType.PBOE, It.IsAny<bool>(), null);

        }

        #endregion

        #region SaveBOEFormIBOE Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveBOEFormIBOE_Test_ArgumentNullException()
        {
            sut.SaveBOEFormIBOE(null, It.IsAny<int>());
        }

        [TestMethod]
        public void SaveBOEFormIBOE_Test()
        {
            iboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(new BOEFormIBOEDTO() { Id = 1, FormName = "1" }).Verifiable();
            iboeFormDataLoader.Setup(x => x.Save(It.IsAny<BOEFormIBOEDTO>())).Verifiable();

			BOEFormIBOEModelView mv = new BOEFormIBOEModelView(new BOEFormIBOEDTO()) { BOEFormId = 1 };
            sut.SaveBOEFormIBOE(mv, It.IsInRange<int>(1, int.MaxValue, Range.Inclusive));

            iboeFormDataLoader.Verify(x => x.GetById( It.IsAny<int>()), Times.Once());
            iboeFormDataLoader.Verify(x => x.Save(It.IsAny<BOEFormIBOEDTO>()), Times.Once());

        }

        #endregion

        #region SaveBOEFormPBOE Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveBOEFormPBOE_Test_ArgumentNullException()
        {
            sut.SaveBOEFormPBOE(null, It.IsAny<int>());
        }

        [TestMethod]
        public void SaveBOEFormPBOE_Test()
        {
            pboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(new BOEFormPBOEDTO() { Id = 1, FormName = "1" });
            pboeFormDataLoader.Setup(x => x.Save(It.IsAny<BOEFormPBOEDTO>())).Verifiable();

			BOEFormPBOEModelView mv = new BOEFormPBOEModelView(new BOEFormPBOEDTO()) { BOEFormId = 1 };
            sut.SaveBOEFormPBOE(mv, It.IsAny<int>());

            pboeFormDataLoader.Verify(x => x.GetById(It.IsAny<int>()), Times.Once());
            pboeFormDataLoader.Verify(x => x.Save(It.IsAny<BOEFormPBOEDTO>()), Times.Once());
        }

        #endregion

        #region DeleteBOEFormIBOE Tests

        [TestMethod]
        public void DeleteBOEFormIBOE_Test()
        {
            iboeFormDataLoader.Setup(x => x.Save(It.IsAny<BOEFormIBOEDTO>())).Verifiable();

			BOEFormIBOEModelView mv = new BOEFormIBOEModelView (    new BOEFormIBOEDTO()   );
            sut.SaveBOEFormIBOE(mv, It.IsAny<int>());

            iboeFormDataLoader.Verify(x => x.Save(It.IsAny<BOEFormIBOEDTO>()), Times.Once());
        }

        #endregion

        #region DeleteBOEFormPBOE Tests

        [TestMethod]
        public void DeleteBOEFormPBOE_Test()
        {
            pboeFormDataLoader.Setup(x => x.Save(It.IsAny<BOEFormPBOEDTO>())).Verifiable();

			BOEFormPBOEModelView mv = new BOEFormPBOEModelView(new BOEFormPBOEDTO());
            sut.SaveBOEFormPBOE(mv, It.IsAny<int>());

            pboeFormDataLoader.Verify(x => x.Save(It.IsAny<BOEFormPBOEDTO>()), Times.Once());
        }

        #endregion

        #region ValidateBOEFormsTMResources Tests

        /// <summary>
        /// Test T&amp;M Resource rate validation for IBOE and PBOE forms with null validationError argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateBOEFormsTMResources_Test_Argument1NullException()
        {
            sut.ValidateBOEFormsTMResources(null, It.IsAny<Collection<int>>(), It.IsAny<FullWorkspace>(), It.IsAny<Collection<int>>(), It.IsAny<Collection<int>>());
        }

        /// <summary>
        /// Test T&amp;M Resource rate validation for IBOE and PBOE forms with null resourceIdsWithValidTMRates argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateBOEFormsTMResources_Test_Argument2NullException()
        {
            sut.ValidateBOEFormsTMResources(It.IsAny<Collection<ValidationMessage>>(), It.IsAny<Collection<int>>(), null, It.IsAny<Collection<int>>(), It.IsAny<Collection<int>>());
        }

        /// <summary>
        /// Test T&amp;M Resource rate validation for IBOE and PBOE forms with null workspace argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateBOEFormsTMResources_Test_Argument3NullException()
        {
            sut.ValidateBOEFormsTMResources(It.IsAny<Collection<ValidationMessage>>(), It.IsAny<Collection<int>>(), null, It.IsAny<Collection<int>>(), It.IsAny<Collection<int>>());
        }

        /// <summary>
        /// Test T&amp;M Resource rate validation for IBOE and PBOE forms with and without errors
        /// </summary>
        [TestMethod]
        public void ValidateBOEFormsTMResources_Test_IBOEAndPBOE()
        {
            this.resourceDTODataLoader.Setup(x => x.GetById(1)).Returns(StubbedData.Res1);
            this.resourceDTODataLoader.Setup(x => x.GetById(2)).Returns(StubbedData.Res2);
            this.resourceDTODataLoader.Setup(x => x.GetById(3)).Returns(StubbedData.Res3);
            this.resourceDTODataLoader.Setup(x => x.GetById(4)).Returns(StubbedData.Res4);
            this.resourceDTODataLoader.Setup(x => x.GetById(5)).Returns(StubbedData.Res5);
            this.resourceDTODataLoader.Setup(x => x.GetById(6)).Returns(StubbedData.Res6);
            this.tmResourceRateDTODataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(StubbedData.TmResourceRateCollection);
            this.iboeFormDataLoader.Setup(x => x.GetById(StubbedData.Iboe.Id)).Returns(StubbedData.Iboe);
            this.pboeFormDataLoader.Setup(x => x.GetById(StubbedData.Pboe.Id)).Returns(StubbedData.Pboe);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { new FullBoe() { Id = 1000 } });
            this.retriever.Setup(x => x.GetLaborTypesTypesForBoeId(1000)).Returns(new List<ResourceTypeDto>()
            {
                new ResourceTypeDto() { ResourceID = StubbedData.Res1.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2012, 9, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res2.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2012, 9, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res3.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2012, 9, 15) }
            });

            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();
            Collection<int> resourceIdsWithValidTMRates = new Collection<int>();
            FullWorkspace workspace1 = new FullWorkspace() { Id = 1, ContractStartDate = new DateTime(2012, 7, 15), ContractEndDate = new DateTime(2015, 4, 15), IsUsingTM = true};
            sut.ValidateBOEFormsTMResources(validationErrors, resourceIdsWithValidTMRates, workspace1, new Collection<int>() { StubbedData.Iboe.Id }, new Collection<int>() { StubbedData.Pboe.Id });

            Assert.IsFalse(validationErrors.Any(), "Expecting no errors");
            Assert.AreEqual(resourceIdsWithValidTMRates.Count, 6);
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res1.Id));
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res2.Id));
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res3.Id));
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res4.Id));
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res5.Id));
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res6.Id));

            // Verify T&M Rate Calculations
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(StubbedData.TaskElementCollection);
            retriever.Setup(x => x.GetTMResourceRates(It.IsAny<int>())).Returns(StubbedData.TmResourceRateCollection);
            iboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(StubbedData.Iboe);
            pboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(StubbedData.Pboe);
            pboeFormDataLoader.Setup(x => x.Save(It.IsAny<BOEFormPBOEDTO>())).Verifiable();

            iboeFormDataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(StubbedData.IboeCollection).Verifiable();
            pboeFormDataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(StubbedData.PboeCollection).Verifiable();
            Collection<BOEFormModelView> BOEFormModelViewCollection = sut.GetSummaryForms(workspace1) as Collection<BOEFormModelView>;
            Assert.AreEqual(BOEFormModelViewCollection.Count, StubbedData.IboeCollection.Count + StubbedData.PboeCollection.Count);
            Assert.IsTrue(BOEFormModelViewCollection.All(x => x.HasValidTMRates == true));

            BOEFormModelView resultIboeForm = BOEFormModelViewCollection.Where(x => x.BOEFormId == StubbedData.Iboe.Id).Single();
            BOEFormModelView resultPboeForm = BOEFormModelViewCollection.Where(x => x.BOEFormId == StubbedData.Pboe.Id).Single();
            Assert.AreEqual(resultIboeForm.TotalCost, 222.0m);
            Assert.AreEqual(resultIboeForm.TMCost, 724.70m);
            Assert.AreEqual(resultPboeForm.TotalCost, 11111.0m);
            Assert.AreEqual(resultPboeForm.TMCost, 174.33m);

            validationErrors.Clear();
            resourceIdsWithValidTMRates.Clear();

            FullWorkspace workspace2 = new FullWorkspace() { Id = 2, ContractStartDate = new DateTime(2012, 7, 15), ContractEndDate = new DateTime(2015, 4, 15), IsUsingTM = true };

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { new FullBoe() { Id = 2000 } });
            this.retriever.Setup(x => x.GetLaborTypesTypesForBoeId(2000)).Returns(new List<ResourceTypeDto>()
            {
                new ResourceTypeDto() { ResourceID = StubbedData.Res1.Id, StartDateValue = new DateTime(2012, 4, 15), EndDateValue = new DateTime(2012, 9, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res2.Id, StartDateValue = new DateTime(2012, 3, 15), EndDateValue = new DateTime(2020, 9, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res3.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2020, 9, 15) }
            });

            sut.ValidateBOEFormsTMResources(validationErrors, resourceIdsWithValidTMRates, workspace2, new Collection<int>() { StubbedData.Iboe.Id }, new Collection<int>() { StubbedData.Pboe.Id });

            Assert.AreEqual(validationErrors.Count, 3, "Only expecting three error messages");
            string expectedMsg1 = string.Format("The T&M rates for resource {0} do not cover the entire period of performance {1:MM/yyyy} - {2:MM/yyyy}.", StubbedData.Res1.ResourceName, new DateTime(2012, 4, 15), new DateTime(2012, 9, 15));
            string expectedMsg2 = string.Format("The T&M rates for resource {0} do not cover the entire period of performance {1:MM/yyyy} - {2:MM/yyyy}.", StubbedData.Res2.ResourceName, new DateTime(2012, 3, 15), new DateTime(2020, 9, 15));
            string expectedMsg3 = string.Format("The T&M rates for resource {0} do not cover the entire period of performance {1:MM/yyyy} - {2:MM/yyyy}.", StubbedData.Res3.ResourceName, new DateTime(2012, 9, 15), new DateTime(2020, 9, 15));
            Collection<string> actualMsgStrings = validationErrors.Select(x => x.ValidationIssue).ToCollection();
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg1));
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg2));
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg3));

            Assert.AreEqual(resourceIdsWithValidTMRates.Count, 3);
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res4.Id));
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res5.Id));
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res6.Id));
        }
        #endregion ValidateBOEFormsTMResources Tests

        #region ValidateBOEFormTMResources Tests

        /// <summary>
        /// Test T&amp;M Resource rate validation when ValidationErrors argument is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateBOEFormTMResources_Test_Argument1NullException()
        {
            sut.ValidateBOEFormTMResources(null, It.IsAny<Collection<int>>(), It.IsAny<FullWorkspace>(), It.IsAny<Collection<int>>());
        }

        /// <summary>
        /// Test T&amp;M Resource rate validation when workspace argument is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateBOEFormTMResources_Test_Argument2NullException()
        {
            sut.ValidateBOEFormTMResources(It.IsAny<Collection<ValidationMessage>>(), It.IsAny<Collection<int>>(), null, It.IsAny<Collection<int>>());
        }

        /// <summary>
        /// Test T&amp;M Resource rate validation when resourceIds argument is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateBOEFormTMResources_Test_Argument3NullException()
        {
            sut.ValidateBOEFormTMResources(It.IsAny<Collection<ValidationMessage>>(), It.IsAny<Collection<int>>(), It.IsAny<FullWorkspace>(), null);
        }

        /// <summary>
        /// Test T&amp;M Resource rate validation when the rate values are null
        /// </summary>
        [TestMethod]
        public void ValidateBOEFormTMResources_TestNullRates()
        {
            ICollection<TMResourceRateDTO> tmResourceRatesFromDB = StubbedData.TmResourceRateCollection.DeepClone();
            TMResourceRateDTO tmResourceRateToUpdate = tmResourceRatesFromDB.Single(x => x.Id == 2);
            tmResourceRateToUpdate.ResourceRate = null;

            this.resourceDTODataLoader.Setup(x => x.GetById(1)).Returns(StubbedData.Res1);
            this.resourceDTODataLoader.Setup(x => x.GetById(2)).Returns(StubbedData.Res2);
            this.resourceDTODataLoader.Setup(x => x.GetById(3)).Returns(StubbedData.Res3);
            this.resourceDTODataLoader.Setup(x => x.GetById(4)).Returns(StubbedData.Res4);
            this.resourceDTODataLoader.Setup(x => x.GetById(5)).Returns(StubbedData.Res5);
            this.resourceDTODataLoader.Setup(x => x.GetById(6)).Returns(StubbedData.Res6);
            this.tmResourceRateDTODataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(tmResourceRatesFromDB);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { new FullBoe() { Id = 17 } });
            this.retriever.Setup(x => x.GetLaborTypesTypesForBoeId(17)).Returns(new List<ResourceTypeDto>()
            {
                new ResourceTypeDto() { ResourceID = StubbedData.Res1.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2012, 9, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res2.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2012, 9, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res3.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2012, 9, 15) }
            });

            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();
            Collection<int> resourceIdsWithValidTMRates = new Collection<int>();
            sut.ValidateBOEFormTMResources(validationErrors, resourceIdsWithValidTMRates, StubbedData.Workspace1, new Collection<int> { StubbedData.Res1.Id, StubbedData.Res2.Id });

            Assert.AreEqual(validationErrors.Count, 1, "Only expecting one error message");
            Assert.AreEqual(string.Format("One or more of the T&M rates for resource {0} are not populated.", StubbedData.Res1.ResourceName), validationErrors.First().ValidationIssue);
            Assert.IsTrue(resourceIdsWithValidTMRates.Count == 1);
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res2.Id));

            // Verify T&M Rate Calculations when some T&M rates are not populated
            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(StubbedData.TaskElementCollection);
            retriever.Setup(x => x.GetTMResourceRates(It.IsAny<int>())).Returns(tmResourceRatesFromDB);
            iboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(StubbedData.Iboe);
            pboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(StubbedData.Pboe);
            pboeFormDataLoader.Setup(x => x.Save(It.IsAny<BOEFormPBOEDTO>())).Verifiable();

            iboeFormDataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(StubbedData.IboeCollection).Verifiable();
            pboeFormDataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(StubbedData.PboeCollection).Verifiable();
            Collection<BOEFormModelView> BOEFormModelViewCollection = sut.GetSummaryForms(StubbedData.Workspace1) as Collection<BOEFormModelView>;
            Assert.AreEqual(BOEFormModelViewCollection.Count, StubbedData.IboeCollection.Count + StubbedData.PboeCollection.Count);

            BOEFormModelView resultIboeForm = BOEFormModelViewCollection.Where(x => x.BOEFormId == StubbedData.Iboe.Id).Single();
            BOEFormModelView resultPboeForm = BOEFormModelViewCollection.Where(x => x.BOEFormId == StubbedData.Pboe.Id).Single();
            Assert.IsTrue(resultIboeForm.HasValidTMRates);
            Assert.IsFalse(resultPboeForm.HasValidTMRates);
            Assert.AreEqual(resultIboeForm.TotalCost, 222.0m);
            Assert.AreEqual(resultIboeForm.TMCost, 724.70m);
            Assert.AreEqual(resultPboeForm.TotalCost, 11111.0m);
            Assert.AreEqual(resultPboeForm.TMCost, 0.00m);
        }

        /// <summary>
        /// Test T&amp;M Resource rate validation dates when the start or end dates are null
        /// </summary>
        [TestMethod]
        public void ValidateBOEFormTMResources_TestNullStartEndDates()
        {
            ICollection<TMResourceRateDTO> tmResourceRatesFromDB = StubbedData.TmResourceRateCollection.DeepClone();
            TMResourceRateDTO tmResourceRateToUpdate = tmResourceRatesFromDB.Single(x => x.Id == 3);
            tmResourceRateToUpdate.EndDate = null;
            tmResourceRateToUpdate = tmResourceRatesFromDB.Single(x => x.Id == 5);
            tmResourceRateToUpdate.StartDate = null;

            this.resourceDTODataLoader.Setup(x => x.GetById(1)).Returns(StubbedData.Res1);
            this.resourceDTODataLoader.Setup(x => x.GetById(2)).Returns(StubbedData.Res2);
            this.tmResourceRateDTODataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(tmResourceRatesFromDB);

            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();
            Collection<int> resourceIdsWithValidTMRates = new Collection<int>();
            sut.ValidateBOEFormTMResources(validationErrors, resourceIdsWithValidTMRates, StubbedData.Workspace1, new Collection<int> { StubbedData.Res1.Id, StubbedData.Res2.Id });

            Assert.AreEqual(validationErrors.Count, 2, "Only expecting two error messages");
            string expectedMsg1 = string.Format("One or more of the T&M rates for resource {0} are missing a start or end date.", StubbedData.Res1.ResourceName);
            string expectedMsg2 = string.Format("One or more of the T&M rates for resource {0} are missing a start or end date.", StubbedData.Res2.ResourceName);
            Collection<string> actualMsgStrings = validationErrors.Select(x => x.ValidationIssue).ToCollection();
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg1));
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg2));
            Assert.IsTrue(resourceIdsWithValidTMRates.Count == 0);
        }

        /// <summary>
        /// Test T&amp;M Resource rate validation when there are no rates
        /// </summary>
        [TestMethod]
        public void ValidateBOEFormTMResources_TestNoRates()
        {
            this.resourceDTODataLoader.Setup(x => x.GetById(1)).Returns(StubbedData.Res1);
            this.resourceDTODataLoader.Setup(x => x.GetById(2)).Returns(StubbedData.Res2);
            this.tmResourceRateDTODataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(new Collection<TMResourceRateDTO>() { });

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { new FullBoe() { Id = 88 } });
            this.retriever.Setup(x => x.GetLaborTypesTypesForBoeId(88)).Returns(new List<ResourceTypeDto>()
            {
                new ResourceTypeDto() { ResourceID = StubbedData.Res1.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2012, 9, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res2.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2012, 9, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res3.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2012, 9, 15) }
            });

            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();
            Collection<int> resourceIdsWithValidTMRates = new Collection<int>();
            sut.ValidateBOEFormTMResources(validationErrors, resourceIdsWithValidTMRates, StubbedData.Workspace1, new Collection<int> { StubbedData.Res1.Id, StubbedData.Res2.Id });

            Assert.AreEqual(validationErrors.Count, 2, "Only expecting two error messages");
            string expectedMsg1 = string.Format("There are no T&M rates for resource {0}", StubbedData.Res1.ResourceName);
            string expectedMsg2 = string.Format("There are no T&M rates for resource {0}", StubbedData.Res2.ResourceName);
            Collection<string> actualMsgStrings = validationErrors.Select(x => x.ValidationIssue).ToCollection();
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg1));
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg2));
            Assert.IsTrue(resourceIdsWithValidTMRates.Count == 0);
        }

        /// <summary>
        /// Test T&amp;M Resource rate validation when there are gaps or overlaps in the rate date ranges
        /// </summary>
        [TestMethod]
        public void ValidateBOEFormTMResources_TestDateRangeGapsOrOverlaps()
        {
            ICollection<TMResourceRateDTO> tmResourceRatesFromDB = StubbedData.TmResourceRateCollection.DeepClone();
            TMResourceRateDTO tmResourceRateToUpdate = tmResourceRatesFromDB.Single(x => x.Id == 2);
                tmResourceRateToUpdate.EndDate = new DateTime(2014, 6, 15);
                tmResourceRateToUpdate = tmResourceRatesFromDB.Single(x => x.Id == 5);
                tmResourceRateToUpdate.StartDate = new DateTime(2013, 5, 15);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { new FullBoe() { Id = 66 } });
            this.retriever.Setup(x => x.GetLaborTypesTypesForBoeId(66)).Returns(new List<ResourceTypeDto>()
            {
                new ResourceTypeDto() { ResourceID = StubbedData.Res1.Id, StartDateValue = new DateTime(2013, 5, 15), EndDateValue = new DateTime(2013, 5, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res2.Id, StartDateValue = new DateTime(2013, 5, 15), EndDateValue = new DateTime(2013, 5, 15) }
            });

            this.resourceDTODataLoader.Setup(x => x.GetById(1)).Returns(StubbedData.Res1);
            this.resourceDTODataLoader.Setup(x => x.GetById(2)).Returns(StubbedData.Res2);
            this.tmResourceRateDTODataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(tmResourceRatesFromDB);

            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();
            Collection<int> resourceIdsWithValidTMRates = new Collection<int>();
            sut.ValidateBOEFormTMResources(validationErrors, resourceIdsWithValidTMRates, StubbedData.Workspace1, new Collection<int> { StubbedData.Res1.Id, StubbedData.Res2.Id });

            Assert.AreEqual(validationErrors.Count, 2, "Only expecting two error messages");
            string expectedMsg1 = string.Format("The T&M rates for resource {0} are not sequential, i.e. there are gaps or overlaps in the date ranges.", StubbedData.Res1.ResourceName);
            string expectedMsg2 = string.Format("The T&M rates for resource {0} are not sequential, i.e. there are gaps or overlaps in the date ranges.", StubbedData.Res2.ResourceName);
            Collection<string> actualMsgStrings = validationErrors.Select(x => x.ValidationIssue).ToCollection();
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg1), "expecting gap or overlap in Res1");
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg2), "expecting gap or overlap in Res2");
            Assert.IsTrue(resourceIdsWithValidTMRates.Count == 0);
        }

        /// <summary>
        /// Test T&amp;M Resource rate validation cases:
        ///   workspace1 - valid rates beyond the contract period of performance start and end dates
        ///   workspace2 - valid rates exactly matching the contract period of performance start and end dates
        ///   workspace3 - invalid rates not covering the contract period of performance start date
        ///   workspace4 - invalid rates not covering the contract period of performance end date
        /// </summary>
        [TestMethod]
        public void ValidateBOEFormTMResources_TestDateRanges()
        {
            this.resourceDTODataLoader.Setup(x => x.GetById(1)).Returns(StubbedData.Res1);
            this.resourceDTODataLoader.Setup(x => x.GetById(2)).Returns(StubbedData.Res2);
            this.resourceDTODataLoader.Setup(x => x.GetById(3)).Returns(StubbedData.Res3);
            this.tmResourceRateDTODataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(StubbedData.TmResourceRateCollection);

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { new FullBoe() { Id = 28 } });
            this.retriever.Setup(x => x.GetLaborTypesTypesForBoeId(28)).Returns(new List<ResourceTypeDto>()
            {
                new ResourceTypeDto() { ResourceID = StubbedData.Res1.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2012, 9, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res2.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2012, 9, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res3.Id, StartDateValue = new DateTime(2012, 9, 15), EndDateValue = new DateTime(2012, 9, 15) }
            });

            FullWorkspace workspace1 = new FullWorkspace() { Id = 1, ContractStartDate = new DateTime(2012, 7, 15), ContractEndDate = new DateTime(2015, 4, 15), IsUsingTM = true };
            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();
            Collection<int> resourceIdsWithValidTMRates = new Collection<int>();
            sut.ValidateBOEFormTMResources(validationErrors, resourceIdsWithValidTMRates, workspace1, new Collection<int> { StubbedData.Res1.Id, StubbedData.Res2.Id, StubbedData.Res3.Id });
            Assert.IsFalse(validationErrors.Count > 0, "Expecting no errors");
            Assert.AreEqual(resourceIdsWithValidTMRates.Count, 3);
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res1.Id));
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res2.Id));
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res3.Id));

            validationErrors.Clear();
            resourceIdsWithValidTMRates.Clear();
            FullWorkspace workspace2 = new FullWorkspace() { Id = 1, ContractStartDate = new DateTime(2012, 6, 15), ContractEndDate = new DateTime(2015, 5, 15), IsUsingTM = true };
            sut.ValidateBOEFormTMResources(validationErrors, resourceIdsWithValidTMRates, workspace2, new Collection<int> { StubbedData.Res1.Id, StubbedData.Res2.Id, StubbedData.Res3.Id });
            Assert.AreEqual(validationErrors.Count, 0, "Expecting no errors");
            Assert.AreEqual(resourceIdsWithValidTMRates.Count, 3);
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res1.Id));
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res2.Id));
            Assert.IsTrue(resourceIdsWithValidTMRates.Contains(StubbedData.Res3.Id));

            validationErrors.Clear();
            resourceIdsWithValidTMRates.Clear();
            FullWorkspace workspace3 = new FullWorkspace() { Id = 1, ContractStartDate = new DateTime(2012, 4, 15), ContractEndDate = new DateTime(2015, 5, 15), IsUsingTM = true };

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { new FullBoe() { Id = 99 } });
            this.retriever.Setup(x => x.GetLaborTypesTypesForBoeId(99)).Returns(new List<ResourceTypeDto>()
            {
                new ResourceTypeDto() { ResourceID = StubbedData.Res1.Id, StartDateValue = new DateTime(2012, 4, 15), EndDateValue = new DateTime(2015, 5, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res2.Id, StartDateValue = new DateTime(2012, 4, 15), EndDateValue = new DateTime(2015, 5, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res3.Id, StartDateValue = new DateTime(2012, 4, 15), EndDateValue = new DateTime(2015, 5, 15) }
            });

            sut.ValidateBOEFormTMResources(validationErrors, resourceIdsWithValidTMRates, workspace3, new Collection<int> { StubbedData.Res1.Id, StubbedData.Res2.Id, StubbedData.Res3.Id });
            Assert.AreEqual(validationErrors.Count, 3, "Only expecting three error messages");
            string expectedMsg1 = string.Format("The T&M rates for resource {0} do not cover the entire period of performance {1:MM/yyyy} - {2:MM/yyyy}.", StubbedData.Res1.ResourceName, workspace3.ContractStartDate, workspace3.ContractEndDate);
            string expectedMsg2 = string.Format("The T&M rates for resource {0} do not cover the entire period of performance {1:MM/yyyy} - {2:MM/yyyy}.", StubbedData.Res2.ResourceName, workspace3.ContractStartDate, workspace3.ContractEndDate);
            string expectedMsg3 = string.Format("The T&M rates for resource {0} do not cover the entire period of performance {1:MM/yyyy} - {2:MM/yyyy}.", StubbedData.Res3.ResourceName, workspace3.ContractStartDate, workspace3.ContractEndDate);
            Collection<string> actualMsgStrings = validationErrors.Select(x => x.ValidationIssue).ToCollection();
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg1));
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg2));
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg3));
            Assert.AreEqual(resourceIdsWithValidTMRates.Count, 0);

            validationErrors.Clear();
            resourceIdsWithValidTMRates.Clear();
            FullWorkspace workspace4 = new FullWorkspace() { Id = 1, ContractStartDate = new DateTime(2012, 6, 15), ContractEndDate = new DateTime(2015, 7, 15), IsUsingTM = true };

            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new List<FullBoe>() { new FullBoe() { Id = 789 } });
            this.retriever.Setup(x => x.GetLaborTypesTypesForBoeId(789)).Returns(new List<ResourceTypeDto>()
            {
                new ResourceTypeDto() { ResourceID = StubbedData.Res1.Id, StartDateValue = new DateTime(2012, 6, 15), EndDateValue = new DateTime(2015, 7, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res2.Id, StartDateValue = new DateTime(2012, 6, 15), EndDateValue = new DateTime(2015, 7, 15) },
                new ResourceTypeDto() { ResourceID = StubbedData.Res3.Id, StartDateValue = new DateTime(2012, 6, 15), EndDateValue = new DateTime(2015, 7, 15) }
            });

            sut.ValidateBOEFormTMResources(validationErrors, resourceIdsWithValidTMRates, workspace4, new Collection<int> { StubbedData.Res1.Id, StubbedData.Res2.Id, StubbedData.Res3.Id });
            Assert.AreEqual(validationErrors.Count, 3, "Only expecting three error messages");
            expectedMsg1 = string.Format("The T&M rates for resource {0} do not cover the entire period of performance {1:MM/yyyy} - {2:MM/yyyy}.", StubbedData.Res1.ResourceName, workspace4.ContractStartDate, workspace4.ContractEndDate);
            expectedMsg2 = string.Format("The T&M rates for resource {0} do not cover the entire period of performance {1:MM/yyyy} - {2:MM/yyyy}.", StubbedData.Res2.ResourceName, workspace4.ContractStartDate, workspace4.ContractEndDate);
            expectedMsg3 = string.Format("The T&M rates for resource {0} do not cover the entire period of performance {1:MM/yyyy} - {2:MM/yyyy}.", StubbedData.Res3.ResourceName, workspace4.ContractStartDate, workspace4.ContractEndDate);
            actualMsgStrings = validationErrors.Select(x => x.ValidationIssue).ToCollection();
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg1));
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg2));
            Assert.IsTrue(actualMsgStrings.Contains(expectedMsg3));
            Assert.AreEqual(resourceIdsWithValidTMRates.Count, 0);
        }
        #endregion ValidateBOEFormTMResources Tests

        #region ValidateBoeForm Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidatePBOEForm_Test_Argument1NullException()
        {
            sut.ValidatePBOE(null, It.IsAny<Collection<ValidationMessage>>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidatePBOEForm_Test_Argument2NullException()
        {
            sut.ValidatePBOE(new BOEFormPBOEDTO() , null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateIBOEForm_Test_Argument1NullException()
        {
            sut.ValidateIBOE(null, It.IsAny<Collection<ValidationMessage>>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateIBOEForm_Test_Argument2NullException()
        {
            sut.ValidateIBOE(new BOEFormIBOEDTO(), null);
        }

        [TestMethod]
        public void ValidatePBOEForm_Test_Missing()
        {
            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            BOEFormPBOEDTO pboe = new BOEFormPBOEDTO
            {
                Approver = "approver",
                ApproverPhone = "phonea",
                Description = "desc",
                FormName = "test1",
                Poc = "poc",
                PocPhone = "phoneb",
                ProposalDate = "date",
                ProposalTitle = "title",
                Revision = 0,
                Version = 1,
                WorkspaceId = 5,
                CCoPDApplies = true,
                ProposalNumber = "pn",
                RFP = "rfp",
                SupplierName = "sn",
                ValidityDate = "date2",
				SupplierProposedValue = 30m,
				VendorId = "vendorId"
            };

            sut.ValidatePBOE(pboe, messages);

            Assert.AreEqual(0, messages.Count);

            messages = new Collection<ValidationMessage>();

            pboe.CCoPDApplies = false;
            sut.ValidatePBOE(pboe, messages);
            Assert.AreEqual(1, messages.Count);
            pboe.CCoPDApplies = true;
            messages = new Collection<ValidationMessage>();

            pboe.RFP = null;
            sut.ValidatePBOE(pboe, messages);
            Assert.AreEqual(1, messages.Count);
            pboe.RFP = "test";
            messages = new Collection<ValidationMessage>();

            pboe.ProposalNumber = null;
            sut.ValidatePBOE(pboe, messages);
            Assert.AreEqual(1, messages.Count);
            pboe.ProposalNumber = "test";
            messages = new Collection<ValidationMessage>();

            pboe.SupplierName = null;
            sut.ValidatePBOE(pboe, messages);
            Assert.AreEqual(1, messages.Count);
            pboe.SupplierName = "test";
            messages = new Collection<ValidationMessage>();

            pboe.ValidityDate = null;
            sut.ValidatePBOE(pboe, messages);
            Assert.AreEqual(1, messages.Count);
            pboe.ValidityDate = "test";
            messages = new Collection<ValidationMessage>();

            pboe.ProposalTitle = null;
            sut.ValidatePBOE(pboe, messages);
            Assert.AreEqual(1, messages.Count);
            pboe.ProposalTitle = "test";
            messages = new Collection<ValidationMessage>();

            pboe.ProposalDate = null;
            sut.ValidatePBOE(pboe, messages);
            Assert.AreEqual(1, messages.Count);
            pboe.ProposalDate = "test";
            messages = new Collection<ValidationMessage>();

            pboe.Description = null;
            sut.ValidatePBOE(pboe, messages);
            Assert.AreEqual(1, messages.Count);
            pboe.Description = "test";
            messages = new Collection<ValidationMessage>();

            pboe.Poc = null;
            sut.ValidatePBOE(pboe, messages);
            Assert.AreEqual(1, messages.Count);
            pboe.Poc = "test";
            messages = new Collection<ValidationMessage>();

            pboe.Approver = null;
            sut.ValidatePBOE(pboe, messages);
            Assert.AreEqual(1, messages.Count);
            pboe.Approver = "test";
            messages = new Collection<ValidationMessage>();
        }

        [TestMethod]
        public void ValidateIBOEForm_Test_Missing()
        {
            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            BOEFormIBOEDTO iboe = new BOEFormIBOEDTO
            {
                Approver = "approver",
                ApproverPhone = "phonea",
                BasisAndRationale = "bandR",
                BusinessArea = "isgs",
                Description = "desc",
                FormName = "test1",
                Poc = "poc",
                PocPhone = "phoneb",
                ProposalDate = "date",
                ProposalTitle = "title",
                Revision = 0,
                Version = 1,
                WorkspaceId = 5
            };

            sut.ValidateIBOE(iboe, messages);

            Assert.AreEqual(0, messages.Count);

            iboe.BusinessArea = null;
            sut.ValidateIBOE(iboe, messages);
            Assert.AreEqual(1, messages.Count);
            iboe.BusinessArea = "test";
            messages = new Collection<ValidationMessage>();

            iboe.ProposalTitle = null;
            sut.ValidateIBOE(iboe, messages);
            Assert.AreEqual(1, messages.Count);
            iboe.ProposalTitle = "test";
            messages = new Collection<ValidationMessage>();

            iboe.ProposalDate = null;
            sut.ValidateIBOE(iboe, messages);
            Assert.AreEqual(1, messages.Count);
            iboe.ProposalDate = "test";
            messages = new Collection<ValidationMessage>();

            iboe.Description = null;
            sut.ValidateIBOE(iboe, messages);
            Assert.AreEqual(1, messages.Count);
            iboe.Description = "test";
            messages = new Collection<ValidationMessage>();

            iboe.Poc = null;
            sut.ValidateIBOE(iboe, messages);
            Assert.AreEqual(1, messages.Count);
            iboe.Poc = "test";
            messages = new Collection<ValidationMessage>();

            iboe.Approver = null;
            sut.ValidateIBOE(iboe, messages);
            Assert.AreEqual(1, messages.Count);
            iboe.Approver = "test";
            messages = new Collection<ValidationMessage>();
        }

        #endregion ValidateBoeForm Tests

        #region GetForm Tests

        [TestMethod]
        public void GetForm_Test_IBOE()
        {
            int boeFormId = It.IsAny<int>();
            BOEFormPBOEDTO pdto = new BOEFormPBOEDTO() { Id = boeFormId, FormName = "TestName a" };
            pboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(pdto).Verifiable();

            BOEFormIBOEDTO idto = new BOEFormIBOEDTO() { Id = boeFormId, FormName = "TestName b" };
            iboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(idto).Verifiable();

            //sut = new BOEFormControllerLogic(boeStateMachine.Object, factory.Object, boeDtoDataLoader.Object, userLoader.Object, permissionsLoader.Object, performingOrgDTODataLoader.Object, iboeFormDataLoader.Object, pboeFormDataLoader.Object, iboeExporter.Object, pboeExporter.Object);

            BOEFormModelView mv = sut.GetIBOEForm(It.IsAny<int>(), boeFormId, It.IsAny<string>());
            Assert.AreEqual(mv.BOEFormId, boeFormId);
            Assert.AreEqual(mv.BOEFormName, idto.FormName);
            Assert.AreEqual(mv.BOEFormType, idto.BOEFormType);
            //Assert.AreEqual(mv.TotalCost, idto.TotalCost); TODO

            pboeFormDataLoader.Verify(x => x.GetById(It.IsAny<int>()), Times.Never());
            iboeFormDataLoader.Verify(x => x.GetById(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void GetForm_Test_PBOE()
        {
            int boeFormId = It.IsAny<int>();
            BOEFormIBOEDTO idto = new BOEFormIBOEDTO() { Id = boeFormId, FormName = "TestName" };
            iboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(idto).Verifiable();

            BOEFormPBOEDTO pdto = new BOEFormPBOEDTO() { Id = boeFormId, FormName = "TestName" };
            pboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(pdto).Verifiable();

            //sut = new BOEFormControllerLogic(boeStateMachine.Object, factory.Object, boeDtoDataLoader.Object, userLoader.Object, permissionsLoader.Object, performingOrgDTODataLoader.Object, iboeFormDataLoader.Object, pboeFormDataLoader.Object, iboeExporter.Object, pboeExporter.Object);

            BOEFormModelView mv = sut.GetPBOEForm(It.IsAny<int>(), boeFormId, It.IsAny<string>());
            Assert.AreEqual(mv.BOEFormId, boeFormId);
            Assert.AreEqual(mv.BOEFormName, pdto.FormName);
            Assert.AreEqual(mv.BOEFormType, pdto.BOEFormType);
            //Assert.AreEqual(mv.TotalCost, pdto.TotalCost); TODO

            iboeFormDataLoader.Verify(x => x.GetById(It.IsAny<int>()), Times.Never());
            pboeFormDataLoader.Verify(x => x.GetById(It.IsAny<int>()), Times.Once());
        }

        #endregion

        #region GetSummaryForms Tests

        [TestMethod]
        public void GetSummaryForms_Test()
        {
            iboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(new BOEFormIBOEDTO() { Id = 1, FormName = "1" });
            pboeFormDataLoader.Setup(x => x.GetById(It.IsAny<int>())).Returns(new BOEFormPBOEDTO() { Id = 1, FormName = "1" });
            pboeFormDataLoader.Setup(x => x.Save(It.IsAny<BOEFormPBOEDTO>())).Verifiable();

            Collection<BOEFormIBOEDTO> BOEFormIboeDTOCollection = StubbedData.BOEFormIboeDTOCollection;
            Collection<BOEFormPBOEDTO> BOEFormPboeDTOCollection = StubbedData.BOEFormPboeDTOCollection;

            iboeFormDataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(BOEFormIboeDTOCollection).Verifiable();
            pboeFormDataLoader.Setup(x => x.GetByWorkspaceId(It.IsAny<int>())).Returns(BOEFormPboeDTOCollection).Verifiable();
            FullWorkspace workspace = new FullWorkspace();

            retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(StubbedData.TaskElementCollection);


            Collection<BOEFormModelView> BOEFormModelViewCollection = sut.GetSummaryForms(workspace) as Collection<BOEFormModelView>;
            Assert.AreEqual( BOEFormModelViewCollection.Count , BOEFormIboeDTOCollection.Count + BOEFormPboeDTOCollection.Count );

            BOEFormModelView vm;
            foreach (BOEFormDTO boe in BOEFormPboeDTOCollection)
            {
                vm = BOEFormModelViewCollection.Where(x => x.BOEFormType == boe.BOEFormType && x.BOEFormId == boe.Id).Single();
                // these are actually the only fields that get converted due to 
                // the call to ConvertSummaryDtoToModelView inside of GetSummaryForms
                Assert.IsTrue(vm.BOEFormId == boe.Id, "BOEFormId");
                Assert.IsTrue(vm.BOEFormName == boe.FormName, "BOEFormName");
                Assert.IsTrue(vm.BOEFormType == boe.BOEFormType, "BOEFormType");
            }

            foreach (BOEFormDTO boe in BOEFormIboeDTOCollection)
            {
                vm = BOEFormModelViewCollection.Where(x => x.BOEFormType == boe.BOEFormType && x.BOEFormId == boe.Id).Single();
                // these are actually the only fields that get converted due to 
                // the call to ConvertSummaryDtoToModelView inside of GetSummaryForms
                Assert.IsTrue(vm.BOEFormId == boe.Id, "BOEFormId");
                Assert.IsTrue(vm.BOEFormName == boe.FormName, "BOEFormName");
                Assert.IsTrue(vm.BOEFormType == boe.BOEFormType, "BOEFormType");
            }
        }

        #endregion

        #region GetCurrentFormVersion Tests

        [TestMethod]
        public void GetCurrentFormVersion_Test_OrderNumberReturned_IBOE()
        {
            iboeFormDataLoader.Setup(x => x.GetCurrentFormVersion());
            pboeFormDataLoader.Setup(x => x.GetCurrentFormVersion());
            sut.GetCurrentFormVersion(BOEFormType.IBOE);
            iboeFormDataLoader.Verify(x => x.GetCurrentFormVersion(), Times.Once());
            pboeFormDataLoader.Verify(x => x.GetCurrentFormVersion(), Times.Never());
        }

        [TestMethod]
        public void GetCurrentFormVersion_Test_OrderNumberReturned_PBOE()
        {
            iboeFormDataLoader.Setup(x => x.GetCurrentFormVersion());
            pboeFormDataLoader.Setup(x => x.GetCurrentFormVersion());
            sut.GetCurrentFormVersion(BOEFormType.PBOE);
            iboeFormDataLoader.Verify(x => x.GetCurrentFormVersion(), Times.Never());
            pboeFormDataLoader.Verify(x => x.GetCurrentFormVersion(), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetCurrentFormVersion_Test_ArgumentException()
        { 
            sut.GetCurrentFormVersion(BOEFormType.NotSet);
        }

        #endregion
         
        #region Misc Tests
        [TestMethod]
        public void GetProposalTitleAndRfpNumber_Test_()
        {
            // Proposal Title and RFP Number exist
            FullWorkspace workspace1 = new FullWorkspace()
            {
                Id = 1,
                ContractStartDate = new DateTime(2012, 6, 15),
                ContractEndDate = new DateTime(2015, 5, 15),
                IsUsingTM = true,
                ProposalTitle = "My Proposal Title",
                RFPNumber = "1234"
            };

            string result = sut.GetProposalTitleAndRfpNumber(workspace1);
            Assert.AreEqual("My Proposal Title / 1234", result);

            // Proposal Title exists but RFP number is null or empty
            workspace1 = new FullWorkspace()
            {
                Id = 1,
                ContractStartDate = new DateTime(2012, 6, 15),
                ContractEndDate = new DateTime(2015, 5, 15),
                IsUsingTM = true,
                ProposalTitle = "My Proposal Title"
            };

            result = sut.GetProposalTitleAndRfpNumber(workspace1);
            Assert.AreEqual("My Proposal Title", result);

            // Proposal Title and RFP Number are both null or empty
            workspace1 = new FullWorkspace()
            {
                Id = 1,
                ContractStartDate = new DateTime(2012, 6, 15),
                ContractEndDate = new DateTime(2015, 5, 15),
                IsUsingTM = true
            };

            result = sut.GetProposalTitleAndRfpNumber(workspace1);
            Assert.AreEqual(string.Empty, result);
        }

        #endregion
    }
}
