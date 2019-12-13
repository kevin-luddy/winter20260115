// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common.classes;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class TaskElementValidationTest
    {
        #region Properties & Create System method

        private Mock<IVariableSelectBOEtoSumCalculation> _VariableSelectBOEtoSumCalculation;

        private Mock<BoeTaskElementRecalculation> _BoeTaskElementRecalculation;
        private Mock<IFullObjectFactory> factory;
        private Mock<IRetriever> retriever;

        private Mock<ICommonDataMapper> _CommonDataMapper;
        private Mock<IPermissionsDTODataLoader> _PermissionsDTODataLoader;

        private TaskElementValidation CreateSystem()
        {
            _VariableSelectBOEtoSumCalculation = new Mock<IVariableSelectBOEtoSumCalculation>();
            this.factory = new Mock<IFullObjectFactory>();
            this.retriever = new Mock<IRetriever>();

            _BoeTaskElementRecalculation = new Mock<BoeTaskElementRecalculation>(_VariableSelectBOEtoSumCalculation.Object, factory.Object);
            _CommonDataMapper = new Mock<ICommonDataMapper>();
            _PermissionsDTODataLoader = new Mock<IPermissionsDTODataLoader>();

            GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _CommonDataMapper.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _PermissionsDTODataLoader.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(BoeTaskElementRecalculation), _BoeTaskElementRecalculation.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableSelectBOEtoSumCalculation), _VariableSelectBOEtoSumCalculation.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
            GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);

            return new TaskElementValidation(this._VariableSelectBOEtoSumCalculation.Object);
        }

        #endregion

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void AreSpreadDatesValid_Exception1()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace();
            ICollection<BoeTaskElementDTO> taskElementData = null;

            sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void AreSpreadDatesValid_Exception2()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = null;
            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();

            sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData);
        }

        [TestMethod]
        public void AreSpreadDatesValid_InvalidData_TaskStartDateMissing()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 4, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                EndDate = DateTime.Parse("01/2014"),
                MOQHoursEquation = "0",
                BoeID = boe.Id
            });

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void AreSpreadDatesValid_InvalidData_TaskEndDateMissing()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 2, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                MOQHoursEquation = "0",
                BoeID = boe.Id
            });

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void AreSpreadDatesValid_InvalidData_TaskStartDateFails()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 2, 1), EndDate = new DateTime(2014, 4, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                EndDate = DateTime.Parse("04/2014"),
                MOQHoursEquation = "0",
                BoeID = boe.Id
            });

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void AreSpreadDatesValid_InvalidData_TaskEndDateFails()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 4, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                EndDate = DateTime.Parse("05/2014"),
                MOQHoursEquation = "0",
                BoeID = boe.Id
            });

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void AreSpreadDatesValid_InvalidData_LaborTypeStartDateMissing()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                EndDate = DateTime.Parse("05/2014"),
                MOQHoursEquation = "3",
                taskElementLabors = new Collection<ResourceTypeDto>()
                {
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 20,
                        EndDateValue = DateTime.Parse("04/2014"),
                        ValueSpread = 3,
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 1 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 1 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 1 }
                        }
                    }
                },
                BoeID = boe.Id
            });

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void AreSpreadDatesValid_InvalidData_LaborTypeEndDateMissing()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                EndDate = DateTime.Parse("05/2014"),
                MOQHoursEquation = "3",
                taskElementLabors = new Collection<ResourceTypeDto>()
                {
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 20,
                        StartDateValue = DateTime.Parse("02/2014"),
                        ValueSpread = 3,
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 1 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 1 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 1 }
                        }
                    }
                },
                BoeID = boe.Id
            });

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void AreSpreadDatesValid_InvalidData_SpreadDateFailsStart()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                EndDate = DateTime.Parse("05/2014"),
                MOQHoursEquation = "3",
                taskElementLabors = new Collection<ResourceTypeDto>()
                {
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 20,
                        StartDateValue = DateTime.Parse("03/2014"),
                        EndDateValue = DateTime.Parse("04/2014"),
                        ValueSpread = 3,
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 1 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 1 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 1 }
                        }
                    }
                },
                BoeID = boe.Id
            });

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void AreSpreadDatesValid_InvalidData_SpreadDateFailsEnd()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                EndDate = DateTime.Parse("05/2014"),
                MOQHoursEquation = "3",
                taskElementLabors = new Collection<ResourceTypeDto>()
                {
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 20,
                        StartDateValue = DateTime.Parse("02/2014"),
                        EndDateValue = DateTime.Parse("03/2014"),
                        ValueSpread = 3,
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 1 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 1 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 1 }
                        }
                    }
                },
                BoeID = boe.Id
            });

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void AreSpreadDatesValid_Pass()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                EndDate = DateTime.Parse("05/2014"),
                MOQHoursEquation = "80",
                taskElementLabors = new Collection<ResourceTypeDto>()
                {
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 20,
                        StartDateValue = DateTime.Parse("02/2014"),
                        EndDateValue = DateTime.Parse("04/2014"),
                        ValueSpread = 30,
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                        }
                    },
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 30,
                        StartDateValue = DateTime.Parse("01/2014"),
                        EndDateValue = DateTime.Parse("04/2014"),
                        ValueSpread = 40,
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 1, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                        }
                    },
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 30,
                        StartDateValue = DateTime.Parse("04/2014"),
                        EndDateValue = DateTime.Parse("04/2014"),
                        ValueSpread = 10,
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                        }
                    }
                },
                BoeID = boe.Id
            });
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(taskElementData);
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>());
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>());
            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO>());

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void ValidateTaskElementsWithErrorMessages_InvalidData_HoursTotalTypesVsTotalSpreadsFails()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                EndDate = DateTime.Parse("05/2014"),
                MOQHoursEquation = "40",
                taskElementLabors = new Collection<ResourceTypeDto>()
                {
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 30,
                        StartDateValue = DateTime.Parse("01/2014"),
                        EndDateValue = DateTime.Parse("04/2014"),
                        ValueSpread = 40, // spreads add up to 39 .. FAILURE
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 1, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 9 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                        }
                    }
                },
                BoeID = boe.Id
            });

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ValidateTaskElementsWithErrorMessages_InvalidData_MoqThrows()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                EndDate = DateTime.Parse("05/2014"),
                MOQHoursEquation = "40xxxx +", // parser throws exception
                taskElementLabors = new Collection<ResourceTypeDto>()
                {
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 30,
                        StartDateValue = DateTime.Parse("01/2014"),
                        EndDateValue = DateTime.Parse("04/2014"),
                        ValueSpread = 40, // spreads add up to 39 .. FAILURE
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 1, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 9 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                        }
                    }
                },
                BoeID = boe.Id
            });

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ValidateTaskElementsWithErrorMessages_InvalidData_MoqHoursVsTypesTotalFails()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                EndDate = DateTime.Parse("05/2014"),
                MOQHoursEquation = "40", // spreads & types are 39.. FAILURE
                taskElementLabors = new Collection<ResourceTypeDto>()
                {
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 30,
                        StartDateValue = DateTime.Parse("01/2014"),
                        EndDateValue = DateTime.Parse("04/2014"),
                        ValueSpread = 39,
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 1, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 9 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                        }
                    }
                },
                BoeID = boe.Id
            });

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ValidateTaskElementsWithErrorMessages_InvalidData_CostTotalTypesVsTotalSpreadsFails()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                EndDate = DateTime.Parse("05/2014"),
                MOQHoursEquation = "40",
                taskElementLabors = new Collection<ResourceTypeDto>()
                {
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Cost,
                        Id = 30,
                        StartDateValue = DateTime.Parse("01/2014"),
                        EndDateValue = DateTime.Parse("04/2014"),
                        ValueSpread = 40, // spreads add up to 39 .. FAILURE
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 1, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 9 },
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                        }
                    }
                },
                BoeID = boe.Id
            });

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        /// <summary>
        /// Tests that spreads w/ value of 0, and a bad date do not trip validation, but are ignored.
        /// </summary>
        [TestMethod]
        public void AreSpreadDatesValid_Pass_BOEJ276()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            ICollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>();
            taskElementData.Add(new BoeTaskElementDTO()
            {
                StartDate = DateTime.Parse("01/2014"),
                EndDate = DateTime.Parse("05/2014"),
                MOQHoursEquation = "56",
                taskElementLabors = new Collection<ResourceTypeDto>()
                {
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 20,
                        StartDateValue = DateTime.Parse("02/2014"),
                        EndDateValue = DateTime.Parse("03/2014"),
                        ValueSpread = 20,
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborTypeId = 20, LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborTypeId = 20, LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 10 },
                            new ResourceSpreadDto() { LaborTypeId = 20, LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 0 } // end date is outside of PoP, but 0 value
                        }
                    },
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 30,
                        StartDateValue = DateTime.Parse("02/2014"),
                        EndDateValue = DateTime.Parse("04/2014"),
                        ValueSpread = 33,
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborTypeId = 30, LaborSpreadDate = new DateTime(2014, 1, 1), LaborSpreadValue = 0 }, // start date is outside of PoP, but 0 value
                            new ResourceSpreadDto() { LaborTypeId = 30, LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 11 },
                            new ResourceSpreadDto() { LaborTypeId = 30, LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 11 },
                            new ResourceSpreadDto() { LaborTypeId = 30, LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 11 }
                        }
                    },
                    new ResourceTypeDto()
                    {
                        SpreadType = IES.Common.SpreadType.Hours,
                        Id = 30,
                        StartDateValue = DateTime.Parse("04/2014"),
                        EndDateValue = DateTime.Parse("04/2014"),
                        ValueSpread = 3,
                        LaborSpreads = new Collection<ResourceSpreadDto>()
                        {
                            new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 3 }
                        }
                    }
                },
                BoeID = boe.Id
            });
            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(taskElementData);
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>());
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>());
            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO>());

            bool isValid = !sut.ValidateTaskElementsWithErrorMessages(ws, taskElementData).Any();

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void ValidateTaskElementsWithErrorMessages_NoItems()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };

            ICollection<LaborValidationClass> result = sut.ValidateTaskElementsWithErrorMessages(ws, new List<BoeTaskElementDTO>());

            Assert.IsFalse(result.Any());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void GetInvalidTaskElementIds_Null1()
        {
            TaskElementValidation sut = this.CreateSystem();

            sut.GetInvalidTaskElementIds(null, new List<BoeTaskElementDTO>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void GetInvalidTaskElementIds_Null2()
        {
            TaskElementValidation sut = this.CreateSystem();

            sut.GetInvalidTaskElementIds(new FullWorkspace(), null);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_NoItems()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };

            ICollection<int> result = sut.GetInvalidTaskElementIds(ws, new List<BoeTaskElementDTO>());

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_TaskStartDateMissing()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 4, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>()
            {
                new BoeTaskElementDTO()
                {
                    EndDate = DateTime.Parse("01/2014"),
                    MOQHoursEquation = "0",
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_TaskEndDateMissing()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 2, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>
            {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("01/2014"),
                    MOQHoursEquation = "0",
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_TaskStartDateFails()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 2, 1), EndDate = new DateTime(2014, 4, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>
            {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("01/2014"),
                    EndDate = DateTime.Parse("04/2014"),
                    MOQHoursEquation = "0",
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_TaskEndDateFails()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 4, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>()
            {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("01/2014"),
                    EndDate = DateTime.Parse("05/2014"),
                    MOQHoursEquation = "0",
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_TaskStartAfterTaskEndDateFails()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 12, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>()
            {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("05/2014"),
                    EndDate = DateTime.Parse("04/2014"),
                    MOQHoursEquation = "0",
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_LaborTypeStartDateMissing()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>() {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("01/2014"),
                    EndDate = DateTime.Parse("05/2014"),
                    MOQHoursEquation = "3",
                    taskElementLabors = new Collection<ResourceTypeDto>()
                    {
                        new ResourceTypeDto()
                        {
                            SpreadType = IES.Common.SpreadType.Hours,
                            Id = 20,
                            EndDateValue = DateTime.Parse("04/2014"),
                            ValueSpread = 3,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 1 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 1 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 1 }
                            }
                        }
                    },
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_LaborTypeEndDateMissing()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>()
            {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("01/2014"),
                    EndDate = DateTime.Parse("05/2014"),
                    MOQHoursEquation = "3",
                    taskElementLabors = new Collection<ResourceTypeDto>()
                    {
                        new ResourceTypeDto()
                        {
                            SpreadType = IES.Common.SpreadType.Hours,
                            Id = 20,
                            StartDateValue = DateTime.Parse("02/2014"),
                            ValueSpread = 3,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 1 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 1 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 1 }
                            }
                        }
                    },
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_SpreadDateFailsStart()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>()
            {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("01/2014"),
                    EndDate = DateTime.Parse("05/2014"),
                    MOQHoursEquation = "3",
                    taskElementLabors = new Collection<ResourceTypeDto>()
                    {
                        new ResourceTypeDto()
                        {
                            SpreadType = IES.Common.SpreadType.Hours,
                            Id = 20,
                            StartDateValue = DateTime.Parse("03/2014"),
                            EndDateValue = DateTime.Parse("04/2014"),
                            ValueSpread = 3,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 1 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 1 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 1 }
                            }
                        }
                    },
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_SpreadDateFailsEnd()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>() {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("01/2014"),
                    EndDate = DateTime.Parse("05/2014"),
                    MOQHoursEquation = "3",
                    taskElementLabors = new Collection<ResourceTypeDto>()
                    {
                        new ResourceTypeDto()
                        {
                            SpreadType = IES.Common.SpreadType.Hours,
                            Id = 20,
                            StartDateValue = DateTime.Parse("02/2014"),
                            EndDateValue = DateTime.Parse("03/2014"),
                            ValueSpread = 3,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 1 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 1 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 1 }
                            }
                        }
                    },
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_Pass()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>() {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("01/2014"),
                    EndDate = DateTime.Parse("05/2014"),
                    MOQHoursEquation = "80",
                    taskElementLabors = new Collection<ResourceTypeDto>()
                    {
                        new ResourceTypeDto()
                        {
                            SpreadType = IES.Common.SpreadType.Hours,
                            Id = 20,
                            StartDateValue = DateTime.Parse("02/2014"),
                            EndDateValue = DateTime.Parse("04/2014"),
                            ValueSpread = 30,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                            }
                        },
                        new ResourceTypeDto()
                        {
                            SpreadType = IES.Common.SpreadType.Hours,
                            Id = 30,
                            StartDateValue = DateTime.Parse("01/2014"),
                            EndDateValue = DateTime.Parse("04/2014"),
                            ValueSpread = 40,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 1, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                            }
                        },
                        new ResourceTypeDto()
                        {
                            SpreadType = IES.Common.SpreadType.Hours,
                            Id = 30,
                            StartDateValue = DateTime.Parse("04/2014"),
                            EndDateValue = DateTime.Parse("04/2014"),
                            ValueSpread = 10,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                            }
                        }
                    },
                    BoeID = boe.Id
                }
            };

            this.retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(taskElementData.ToList());
            this.retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>());
            this.retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>());
            this.retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO>());

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_HoursTotalTypesVsTotalSpreadsFails()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>()
            {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("01/2014"),
                    EndDate = DateTime.Parse("05/2014"),
                    MOQHoursEquation = "40",
                    taskElementLabors = new Collection<ResourceTypeDto>()
                    {
                        new ResourceTypeDto()
                        {
                            SpreadType = IES.Common.SpreadType.Hours,
                            Id = 30,
                            StartDateValue = DateTime.Parse("01/2014"),
                            EndDateValue = DateTime.Parse("04/2014"),
                            ValueSpread = 40, // spreads add up to 39 .. FAILURE
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 1, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 9 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                            }
                        }
                    },
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_MoqThrows()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>()
            {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("01/2014"),
                    EndDate = DateTime.Parse("05/2014"),
                    MOQHoursEquation = "40xxxx +", // parser throws exception
                    taskElementLabors = new Collection<ResourceTypeDto>()
                    {
                        new ResourceTypeDto()
                        {
                            SpreadType = IES.Common.SpreadType.Hours,
                            Id = 30,
                            StartDateValue = DateTime.Parse("01/2014"),
                            EndDateValue = DateTime.Parse("04/2014"),
                            ValueSpread = 40, // spreads add up to 39 .. FAILURE
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 1, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 9 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                            }
                        }
                    },
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_MoqHoursVsTypesTotalFails()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>()
            {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("01/2014"),
                    EndDate = DateTime.Parse("05/2014"),
                    MOQHoursEquation = "40", // spreads & types are 39.. FAILURE
                    taskElementLabors = new Collection<ResourceTypeDto>()
                    {
                        new ResourceTypeDto()
                        {
                            SpreadType = IES.Common.SpreadType.Hours,
                            Id = 30,
                            StartDateValue = DateTime.Parse("01/2014"),
                            EndDateValue = DateTime.Parse("04/2014"),
                            ValueSpread = 39,
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 1, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 9 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                            }
                        }
                    },
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetInvalidTaskElementIds_InvalidData_CostTotalTypesVsTotalSpreadsFails()
        {
            TaskElementValidation sut = this.CreateSystem();

            FullWorkspace ws = new FullWorkspace() { Id = 1 };
            FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 5, 1) };
            this.retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id)).Returns(new Collection<FullBoe>() { boe });
            this.retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(ws.Id)).Returns(new List<WorkspaceVariableDTO>());

            IReadOnlyCollection<BoeTaskElementDTO> taskElementData = new List<BoeTaskElementDTO>()
            {
                new BoeTaskElementDTO()
                {
                    StartDate = DateTime.Parse("01/2014"),
                    EndDate = DateTime.Parse("05/2014"),
                    MOQHoursEquation = "40",
                    taskElementLabors = new Collection<ResourceTypeDto>()
                    {
                        new ResourceTypeDto()
                        {
                            SpreadType = IES.Common.SpreadType.Cost,
                            Id = 30,
                            StartDateValue = DateTime.Parse("01/2014"),
                            EndDateValue = DateTime.Parse("04/2014"),
                            ValueSpread = 40, // spreads add up to 39 .. FAILURE
                            LaborSpreads = new Collection<ResourceSpreadDto>()
                            {
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 1, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 2, 1), LaborSpreadValue = 10 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 3, 1), LaborSpreadValue = 9 },
                                new ResourceSpreadDto() { LaborSpreadDate = new DateTime(2014, 4, 1), LaborSpreadValue = 10 }
                            }
                        }
                    },
                    BoeID = boe.Id
                }
            };

            bool isValid = !sut.GetInvalidTaskElementIds(ws, taskElementData).Any();

            Assert.IsFalse(isValid);
        }
    }
}