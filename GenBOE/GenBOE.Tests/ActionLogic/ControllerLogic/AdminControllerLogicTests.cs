// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.ControllerLogic
{
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.ModelView.Admin;
    using GenBOE.ActionLogic.Synchronization;
    using IES.Common;
    using IES.Common.Exceptions;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    [TestClass]
    public class AdminControllerLogicTests
    {
        Mock<ICommonDataMapper> commonDataMapper;
        Mock<IResourceListDTODataLoader> resourceListDTODataLoader;
        Mock<IMSTZoneTravelOriginDTODataLoader> mstZoneTravelOriginDTODataLoader;
        Mock<IMSTZoneTravelDestinationDTODataLoader> mstZoneTravelDestinationDTODataLoader;
        Mock<IMSTTravelNonzoneFeesAndCostsDTODataLoader> mstTravelNonzoneFeesAndCostsDTODataLoader;
        
        public AdminControllerLogic CreateSut()
        {
            this.commonDataMapper = new Mock<ICommonDataMapper>();
            this.resourceListDTODataLoader = new Mock<IResourceListDTODataLoader>();
            
            return new AdminControllerLogic(this.commonDataMapper.Object, this.resourceListDTODataLoader.Object, null);
        }

        public AdminControllerLogic CreateSSCSut()
        {
            this.commonDataMapper = new Mock<ICommonDataMapper>();
            this.resourceListDTODataLoader = new Mock<IResourceListDTODataLoader>();
            this.mstZoneTravelOriginDTODataLoader = new Mock<IMSTZoneTravelOriginDTODataLoader>();
            this.mstZoneTravelDestinationDTODataLoader = new Mock<IMSTZoneTravelDestinationDTODataLoader>();
            this.mstTravelNonzoneFeesAndCostsDTODataLoader = new Mock<IMSTTravelNonzoneFeesAndCostsDTODataLoader>();
            
            return new AdminControllerLogicSpaceSystems(this.commonDataMapper.Object, this.resourceListDTODataLoader.Object, null);
        }

        public AdminControllerLogic CreateMSTSut()
        {
            this.commonDataMapper = new Mock<ICommonDataMapper>();
            this.resourceListDTODataLoader = new Mock<IResourceListDTODataLoader>();
            this.mstZoneTravelOriginDTODataLoader = new Mock<IMSTZoneTravelOriginDTODataLoader>();
            this.mstZoneTravelDestinationDTODataLoader = new Mock<IMSTZoneTravelDestinationDTODataLoader>();
            this.mstTravelNonzoneFeesAndCostsDTODataLoader = new Mock<IMSTTravelNonzoneFeesAndCostsDTODataLoader>();

            return new AdminControllerLogicMST(this.commonDataMapper.Object, this.resourceListDTODataLoader.Object, 
                this.mstZoneTravelOriginDTODataLoader.Object, this.mstZoneTravelDestinationDTODataLoader.Object, this.mstTravelNonzoneFeesAndCostsDTODataLoader.Object, null);
        }

        [TestMethod]
        public void TestGetZoneTravelOriginGridData()
        {
            AdminControllerLogic sut = CreateMSTSut();

            ICollection<MSTZoneTravelOriginDTO> expectedOriginCollection = new Collection<MSTZoneTravelOriginDTO>();
            MSTZoneTravelOriginDTO expectedOrigin = new MSTZoneTravelOriginDTO()
            {
                OriginID = 1,
                Origin = "Test Origin",
                Site = "T"
            };
            expectedOriginCollection.Add(expectedOrigin);

            Collection<MSTZoneTravelResourceDTO> expectedResources = new Collection<MSTZoneTravelResourceDTO>();
            expectedResources.Add(new MSTZoneTravelResourceDTO(1, "res1"));
            expectedResources.Add(new MSTZoneTravelResourceDTO(2, "res2"));
            expectedResources.Add(new MSTZoneTravelResourceDTO(3, "res3"));
            expectedResources.Add(new MSTZoneTravelResourceDTO(4, "res4"));
            expectedResources.Add(new MSTZoneTravelResourceDTO(5, "res5"));
            expectedResources.Add(new MSTZoneTravelResourceDTO(6, "res6"));
            expectedResources.Add(new MSTZoneTravelResourceDTO(7, "res7"));
            expectedResources.Add(new MSTZoneTravelResourceDTO(8, "res8"));
            expectedResources.Add(new MSTZoneTravelResourceDTO(9, "res9"));
            expectedResources.Add(new MSTZoneTravelResourceDTO(10, "res10"));
            expectedResources.Add(new MSTZoneTravelResourceDTO(11, "res11"));
            expectedResources.Add(new MSTZoneTravelResourceDTO(12, "res12"));

            this.mstZoneTravelOriginDTODataLoader.Setup(x => x.GetAllOrigins()).Returns(expectedOriginCollection);
            this.mstZoneTravelOriginDTODataLoader.Setup(x => x.GetOriginResourcesByOriginID(1)).Returns(expectedResources);

            ManageMSTZoneTravelOriginsGridModelView actual = sut.GetZoneTravelOriginGridData();
            Assert.IsNotNull(actual);
            MSTZoneTravelOriginModelView actualMV = actual.MSTZoneTravelOriginsCollection.FirstOrDefault();
            Assert.IsNotNull(actualMV);

            MSTZoneTravelOriginDTO actualOrigin = new MSTZoneTravelOriginDTO()
            {
                OriginID = actualMV.OriginID,
                Origin = actualMV.Origin,
                Site = actualMV.Site
            };

            Assert.AreEqual(expectedOrigin.OriginID, actualOrigin.OriginID);
            Assert.AreEqual(expectedOrigin.Origin, actualOrigin.Origin);
            Assert.AreEqual(expectedOrigin.Site, actualOrigin.Site);

            Collection<MSTZoneTravelResourceDTO> actualResources = new Collection<MSTZoneTravelResourceDTO>();
            actualResources.Add(new MSTZoneTravelResourceDTO(actualMV.ResourceIDPRZ1, actualMV.ResourcePRZ1));
            actualResources.Add(new MSTZoneTravelResourceDTO(actualMV.ResourceIDPRZ2, actualMV.ResourcePRZ2));
            actualResources.Add(new MSTZoneTravelResourceDTO(actualMV.ResourceIDPRZ3, actualMV.ResourcePRZ3));
            actualResources.Add(new MSTZoneTravelResourceDTO(actualMV.ResourceIDPRZ4, actualMV.ResourcePRZ4));
            actualResources.Add(new MSTZoneTravelResourceDTO(actualMV.ResourceIDPRZ5, actualMV.ResourcePRZ5));
            actualResources.Add(new MSTZoneTravelResourceDTO(actualMV.ResourceIDPRZ6, actualMV.ResourcePRZ6));
            actualResources.Add(new MSTZoneTravelResourceDTO(actualMV.ResourceIDTRZ1, actualMV.ResourceTRZ1));
            actualResources.Add(new MSTZoneTravelResourceDTO(actualMV.ResourceIDTRZ2, actualMV.ResourceTRZ2));
            actualResources.Add(new MSTZoneTravelResourceDTO(actualMV.ResourceIDTRZ3, actualMV.ResourceTRZ3));
            actualResources.Add(new MSTZoneTravelResourceDTO(actualMV.ResourceIDTRZ4, actualMV.ResourceTRZ4));
            actualResources.Add(new MSTZoneTravelResourceDTO(actualMV.ResourceIDTRZ5, actualMV.ResourceTRZ5));
            actualResources.Add(new MSTZoneTravelResourceDTO(actualMV.ResourceIDTRZ6, actualMV.ResourceTRZ6));

            Assert.AreEqual(expectedResources.Count(), actualResources.Count());
            for(int i = 0; i < actualResources.Count(); i++)
            {
                Assert.AreEqual(expectedResources[i].ResourceID, actualResources[i].ResourceID);
                Assert.AreEqual(expectedResources[i].Resource, actualResources[i].Resource);
            }
        }

        [TestMethod]
        public void TestGetZoneTravelDestinationsGridData()
        {
            AdminControllerLogic sut = CreateMSTSut();

            ICollection<MSTZoneTravelDestinationDTO> expectedDestinationCollection = new Collection<MSTZoneTravelDestinationDTO>();
            MSTZoneTravelDestinationDTO expectedDestination = new MSTZoneTravelDestinationDTO()
            {
                DestinationID = 1,
                Destination = "Test Dest",
                Abbreviation = "TD",
                Zone = 1
            };
            expectedDestinationCollection.Add(expectedDestination);

            this.mstZoneTravelDestinationDTODataLoader.Setup(x => x.GetAllDestinations()).Returns(expectedDestinationCollection);

            ManageMSTZoneTravelDestinationsGridModelView actual = sut.GetZoneTravelDestinationsGridData();
            Assert.IsNotNull(actual);
            MSTZoneTravelDestinationModelView actualMV = actual.MSTZoneTravelDestinationsCollection.FirstOrDefault();
            Assert.IsNotNull(actualMV);

            MSTZoneTravelDestinationDTO actualDestination = new MSTZoneTravelDestinationDTO()
            {
                DestinationID = actualMV.DestinationID,
                Destination = actualMV.Destination,
                Abbreviation = actualMV.Abbreviation,
                Zone = actualMV.Zone
            };

            Assert.AreEqual(expectedDestination.DestinationID, actualDestination.DestinationID);
            Assert.AreEqual(expectedDestination.Destination, actualDestination.Destination);
            Assert.AreEqual(expectedDestination.Abbreviation, actualDestination.Abbreviation);
            Assert.AreEqual(expectedDestination.Zone, actualDestination.Zone);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSaveOriginData_EX()
        {
            AdminControllerLogic sut = CreateMSTSut();
            sut.SaveOriginData(null);
        }

        [TestMethod]
        public void TestSaveOriginData_Upsert()
        {
            AdminControllerLogic sut = CreateMSTSut();

            MSTZoneTravelOriginModelView input = new MSTZoneTravelOriginModelView()
            {
                OriginID = 1,
                Origin = "Test Origin",
                Site = "T",
                ResourceIDPRZ1 = 1,
                ResourcePRZ1 = "Res1",
                ResourceIDPRZ2 = 2,
                ResourcePRZ2 = null,
                ResourceIDPRZ3 = 3,
                ResourcePRZ3 = "Res3",
                ResourceIDPRZ4 = 4,
                ResourcePRZ4 = "Res4",
                ResourceIDPRZ5 = 5,
                ResourcePRZ5 = "Res5",
                ResourceIDPRZ6 = 6,
                ResourcePRZ6 = "Res6",
                ResourceIDTRZ1 = 7,
                ResourceTRZ1 = "Res7",
                ResourceIDTRZ2 = 8,
                ResourceTRZ2 = "Res8",
                ResourceIDTRZ3 = 9,
                ResourceTRZ3 = "Res9",
                ResourceIDTRZ4 = 10,
                ResourceTRZ4 = "Res10",
                ResourceIDTRZ5 = 11,
                ResourceTRZ5 = "Res11",
                ResourceIDTRZ6 = 12,
                ResourceTRZ6 = "Res12",
                Deleted = false
            };

            this.mstZoneTravelOriginDTODataLoader.Setup(x => x.SaveOrigin(It.IsAny<MSTZoneTravelOriginDTO>(), It.IsAny<ICollection<MSTZoneTravelResourceDTO>>())).Verifiable();

            sut.SaveOriginData(new Collection<MSTZoneTravelOriginModelView> { input });

            this.mstZoneTravelOriginDTODataLoader.Verify(x => x.SaveOrigin(It.IsAny<MSTZoneTravelOriginDTO>(), It.IsAny<ICollection<MSTZoneTravelResourceDTO>>()), Times.Once());
        }

        [TestMethod]
        public void TestSaveOriginData_Delete()
        {
            AdminControllerLogic sut = CreateMSTSut();

            MSTZoneTravelOriginModelView input = new MSTZoneTravelOriginModelView()
            {
                OriginID = 1,
                Origin = "Test Origin",
                Site = "T",
                Deleted = true
            };

            MSTZoneTravelOriginDTO dto = new MSTZoneTravelOriginDTO()
            {
                OriginID = input.OriginID,
                Origin = input.Origin,
                Site = input.Site
            };

            this.mstZoneTravelOriginDTODataLoader.Setup(x => x.SaveOrigin(It.IsAny<MSTZoneTravelOriginDTO>(), It.IsAny<ICollection<MSTZoneTravelResourceDTO>>())).Verifiable();
            this.mstZoneTravelOriginDTODataLoader.Setup(x => x.GetOriginByOriginID(1)).Returns(dto);

            sut.SaveOriginData(new Collection<MSTZoneTravelOriginModelView> { input });

            this.mstZoneTravelOriginDTODataLoader.Verify(x => x.SaveOrigin(It.IsAny<MSTZoneTravelOriginDTO>(), It.IsAny<ICollection<MSTZoneTravelResourceDTO>>()), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSaveDestinationData_EX()
        {
            AdminControllerLogic sut = CreateMSTSut();

            sut.SaveDestinationData(null);
        }

        [TestMethod]
        public void TestSaveDestinationData()
        {
            AdminControllerLogic sut = CreateMSTSut();

            MSTZoneTravelDestinationModelView input = new MSTZoneTravelDestinationModelView()
            {
                DestinationID = 1,
                Destination = "Test Dest",
                Abbreviation = "TD",
                Zone = 1
            };

            this.mstZoneTravelDestinationDTODataLoader.Setup(x => x.SaveDestination(It.IsAny<MSTZoneTravelDestinationDTO>())).Verifiable();

            sut.SaveDestinationData(input);

            this.mstZoneTravelDestinationDTODataLoader.Verify(x => x.SaveDestination(It.IsAny<MSTZoneTravelDestinationDTO>()), Times.Once());
        }

        [TestMethod]
        public void TestGetNonzoneFeesAndCostsGridData()
        {
            AdminControllerLogic sut = CreateMSTSut();

            ICollection<MSTTravelNonzoneFeesAndCostsDTO> expectedFeesAndCostsCollection = new Collection<MSTTravelNonzoneFeesAndCostsDTO>();
            MSTTravelNonzoneFeesAndCostsDTO expectedFeesAndCosts = new MSTTravelNonzoneFeesAndCostsDTO()
            {
                ModeID = 3,
                TravelAgencyFee = 50,
                MiscOther = 25
            };
            expectedFeesAndCostsCollection.Add(expectedFeesAndCosts);

            this.mstTravelNonzoneFeesAndCostsDTODataLoader.Setup(x => x.getAllFeesAndCosts()).Returns(expectedFeesAndCostsCollection);

            NonzoneFeesAndCostsGridModelView actual = sut.GetNonzoneFeesAndCostsGridData();
            Assert.IsNotNull(actual);
            NonzoneFeesAndCostsModelView actualMV = actual.NonzoneFeesAndCostsCollection.FirstOrDefault();
            Assert.IsNotNull(actualMV);

            MSTTravelNonzoneFeesAndCostsDTO actualDTO = new MSTTravelNonzoneFeesAndCostsDTO()
            {
                ModeID = actualMV.ModeID,
                TravelAgencyFee = actualMV.TravelAgencyFee,
                MiscOther = actualMV.MiscOther
            };

            Assert.AreEqual(expectedFeesAndCosts.ModeID, actualDTO.ModeID);
            Assert.AreEqual(expectedFeesAndCosts.TravelAgencyFee, actualDTO.TravelAgencyFee);
            Assert.AreEqual(expectedFeesAndCosts.MiscOther, actualDTO.MiscOther);
        }

        [TestMethod]
        public void TestSaveFeesAndCostsData()
        {
            AdminControllerLogic sut = CreateMSTSut();

            NonzoneFeesAndCostsModelView input = new NonzoneFeesAndCostsModelView()
            {
                Mode = MSTTravelMode.NonZoneDomestic,
                TravelAgencyFee = 50,
                MiscOther = 25
            };

            this.mstTravelNonzoneFeesAndCostsDTODataLoader.Setup(x => x.saveFeesAndCosts(It.IsAny<MSTTravelNonzoneFeesAndCostsDTO>())).Verifiable();

            sut.SaveFeesAndCostsData(input);

            this.mstTravelNonzoneFeesAndCostsDTODataLoader.Verify(x => x.saveFeesAndCosts(It.IsAny<MSTTravelNonzoneFeesAndCostsDTO>()), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSaveFeesAndCostsData_EX()
        {
            AdminControllerLogic sut = CreateMSTSut();

            sut.SaveFeesAndCostsData(null);
        }

        [TestMethod]
        public void ValidateEscalationRate_BadYear()
        {
            AdminControllerLogic sut = CreateSut();
            EscalationRateModelView mv = new EscalationRateModelView
            {
                Year = 100
            };

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateEscalationRate(mv, new List<EscalationRatesDTO>(), messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("ValidYear", messages.First().FieldName);
        }

        [TestMethod]
        public void ValidateEscalationRate_DuplicateYear()
        {
            AdminControllerLogic sut = CreateSut();
            EscalationRateModelView mv = new EscalationRateModelView
            {
                Year = 2016
            };

            List<EscalationRatesDTO> rates = new List<EscalationRatesDTO>();
            rates.Add(new EscalationRatesDTO { Year = 2016 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateEscalationRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Escalation Rate Year must be unique.", messages.First().ValidationIssue);
        }

        [TestMethod]
        public void ValidateEscalationRate_OK()
        {
            AdminControllerLogic sut = CreateSut();
            EscalationRateModelView mv = new EscalationRateModelView
            {
                Year = 2016,
                EscalationRateID = 5
            };

            List<EscalationRatesDTO> rates = new List<EscalationRatesDTO>();
            
            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateEscalationRate(mv, rates, messages);

            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void ValidateEscalationRate_ExistingYear()
        {
            AdminControllerLogic sut = CreateSut();
            EscalationRateModelView mv = new EscalationRateModelView
            {
                Year = 2016,
                EscalationRateID = 5
            };

            List<EscalationRatesDTO> rates = new List<EscalationRatesDTO>();
            rates.Add(new EscalationRatesDTO { Year = 2016, Id = 5, EscalationRateID = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateEscalationRate(mv, rates, messages);

            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateEscalationRate_EX1()
        {
            AdminControllerLogic sut = CreateSut();

            sut.ValidateEscalationRate(null, new List<EscalationRatesDTO>(), new Collection<ValidationMessage>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateEscalationRate_EX2()
        {
            AdminControllerLogic sut = CreateSut();

            sut.ValidateEscalationRate(new EscalationRateModelView(), null, new Collection<ValidationMessage>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateEscalationRate_EX3()
        {
            AdminControllerLogic sut = CreateSut();

            sut.ValidateEscalationRate(new EscalationRateModelView(), new List<EscalationRatesDTO>(), null);
        }

        [TestMethod]
        public void ValidateEscalationRate_BadEscalation1_RMS()
        {
            AdminControllerLogic sut = CreateMSTSut();

            EscalationRateModelView mv = new EscalationRateModelView
            {
                Year = 2016,
                EscalationRateID = 5,
                LMSIEscalation = 1,
                MiscRate = 2
            };

            List<EscalationRatesDTO> rates = new List<EscalationRatesDTO>();
            rates.Add(new EscalationRatesDTO { Year = 2016, Id = 5, EscalationRateID = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateEscalationRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Airfare Escalation is required.", messages.First().ValidationIssue);
        }

        [TestMethod]
        public void ValidateEscalationRate_BadEscalation2_RMS()
        {
            AdminControllerLogic sut = CreateMSTSut();

            EscalationRateModelView mv = new EscalationRateModelView
            {
                Year = 2016,
                EscalationRateID = 5,
                DevEscalation = 12345.66666m,
                LMSIEscalation = 1,
                MiscRate = 2
            };

            List<EscalationRatesDTO> rates = new List<EscalationRatesDTO>();
            rates.Add(new EscalationRatesDTO { Year = 2016, Id = 5, EscalationRateID = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateEscalationRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Airfare Escalation must be in the form of nnnn.nnn where n is a numeric number.", messages.First().ValidationIssue);
        }

        [TestMethod]
        public void ValidateEscalationRate_BadEscalation1_SSC()
        {
            AdminControllerLogic sut = CreateSSCSut();

            EscalationRateModelView mv = new EscalationRateModelView
            {
                Year = 2016,
                EscalationRateID = 5,
                LMSIEscalation = 4m
            };

            List<EscalationRatesDTO> rates = new List<EscalationRatesDTO>();
            rates.Add(new EscalationRatesDTO { Year = 2016, Id = 5, EscalationRateID = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateEscalationRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Dev Escalation is required.", messages.First().ValidationIssue);
        }

        [TestMethod]
        public void ValidateEscalationRate_BadEscalation2_SSC()
        {
            AdminControllerLogic sut = CreateSSCSut();

            EscalationRateModelView mv = new EscalationRateModelView
            {
                Year = 2016,
                EscalationRateID = 5,
                DevEscalation = 12345.66666m,
                LMSIEscalation = 4m
            };

            List<EscalationRatesDTO> rates = new List<EscalationRatesDTO>();
            rates.Add(new EscalationRatesDTO { Year = 2016, Id = 5, EscalationRateID = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateEscalationRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Dev Escalation must be in the form of nnnn.nnn where n is a numeric number.", messages.First().ValidationIssue);
        }

        [TestMethod]
        public void ValidateEscalationRate_BadLMSIEscalation1_SSC()
        {
            AdminControllerLogic sut = CreateSSCSut();

            EscalationRateModelView mv = new EscalationRateModelView
            {
                Year = 2016,
                EscalationRateID = 5,
                DevEscalation = 4m
            };

            List<EscalationRatesDTO> rates = new List<EscalationRatesDTO>();
            rates.Add(new EscalationRatesDTO { Year = 2016, Id = 5, EscalationRateID = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateEscalationRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("LMSI Escalation is required.", messages.First().ValidationIssue);
        }

        [TestMethod]
        public void ValidateEscalationRate_BadLMSIEscalation2_SSC()
        {
            AdminControllerLogic sut = CreateSSCSut();

            EscalationRateModelView mv = new EscalationRateModelView
            {
                Year = 2016,
                EscalationRateID = 5,
                DevEscalation = 6m,
                LMSIEscalation = 55443355.44424232m
            };

            List<EscalationRatesDTO> rates = new List<EscalationRatesDTO>();
            rates.Add(new EscalationRatesDTO { Year = 2016, Id = 5, EscalationRateID = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateEscalationRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("LMSI Escalation must be in the form of nnnn.nnn where n is a numeric number.", messages.First().ValidationIssue);
        }

        #region ValidateOffloadRate

        [TestMethod]
        public void ValidateOffloadRate_OK()
        {
            AdminControllerLogic sut = CreateSut();
            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                Resource = "r",
                PerformingOrg = "p",
                SubcontractorResource = "s",
                HourlyRate = 55m,
                PercentToOffload = 0.05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
                rates.Add(new OffloadRatesDTO { Year = 2015, Id = 4, Resource = "r", PerformingOrg = "p", SubResource = "s", Percent = 0.05m });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        public void ValidateOffloadRate_ExistingId()
        {
            AdminControllerLogic sut = CreateSut();
            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                Resource = "r",
                PerformingOrg = "p",
                SubcontractorResource = "s",
                HourlyRate = 55m,
                PercentToOffload = 0.05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2016, Id = 5});

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(0, messages.Count);
        }

        /// <summary>
        /// Test editing a Sub Resource for the only combination of a resource and performing org
        /// </summary>
        [TestMethod]
        public void ValidateOffloadRate_EditExistingSubResource()
        {
            AdminControllerLogic sut = CreateSut();
            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                Resource = "r",
                PerformingOrg = "p",
                SubcontractorResource = "s",
                HourlyRate = 55m,
                PercentToOffload = 0.05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = mv.Year, Id = mv.OffloadRateID, Resource = mv.Resource,
                PerformingOrg = mv.PerformingOrg, SubResource = "s0", HourlyRate = mv.HourlyRate, Percent = mv.PercentToOffload });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(0, messages.Count);
        }

        /// <summary>
        /// Test editing a Percent to Offload for the only combination of a resource and performing org
        /// </summary>
        [TestMethod]
        public void ValidateOffloadRate_EditExistingPercentToOffload()
        {
            AdminControllerLogic sut = CreateSut();
            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                Resource = "r",
                PerformingOrg = "p",
                SubcontractorResource = "s",
                HourlyRate = 55m,
                PercentToOffload = 0.05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = mv.Year, Id = mv.OffloadRateID, Resource = mv.Resource,
                PerformingOrg = mv.PerformingOrg, SubResource = mv.SubcontractorResource, HourlyRate = mv.HourlyRate, Percent = 0.06m });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(0, messages.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateOffloadRate_EX1()
        {
            AdminControllerLogic sut = CreateSut();

            sut.ValidateOffloadRate(null, new List<OffloadRatesDTO>(), new Collection<ValidationMessage>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateOffloadRate_EX2()
        {
            AdminControllerLogic sut = CreateSut();

            sut.ValidateOffloadRate(new OffloadRateModelView(), null, new Collection<ValidationMessage>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateOffloadRate_EX3()
        {
            AdminControllerLogic sut = CreateSut();

            sut.ValidateOffloadRate(new OffloadRateModelView(), new List<OffloadRatesDTO>(), null);
        }

        [TestMethod]
        public void ValidateOffloadRate_BadOffload1()
        {
            AdminControllerLogic sut = CreateMSTSut();

            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                PerformingOrg = "p",
                SubcontractorResource = "s",
                HourlyRate = 55m,
                PercentToOffload = 0.05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2016, Id = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("Resource", messages.First().FieldName);
        }

        [TestMethod]
        public void ValidateOffloadRate_BadOffload2()
        {
            AdminControllerLogic sut = CreateMSTSut();

            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                PerformingOrg = "p",
                Resource = "r",
                HourlyRate = 55m,
                PercentToOffload = 0.05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2016, Id = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("SubcontractorResource", messages.First().FieldName);
        }

        [TestMethod]
        public void ValidateOffloadRate_BadOffload3()
        {
            AdminControllerLogic sut = CreateMSTSut();

            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                PerformingOrg = "p",
                Resource = "r",
                SubcontractorResource = "s",
                PercentToOffload = 0.05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2016, Id = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("HourlyRate", messages.First().FieldName);
        }

        [TestMethod]
        public void ValidateOffloadRate_BadOffload4()
        {
            AdminControllerLogic sut = CreateMSTSut();

            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                PerformingOrg = "p",
                Resource = "r",
                SubcontractorResource = "s",
                HourlyRate = 55m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2016, Id = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("PercentToOffload", messages.First().FieldName);
        }

        [TestMethod]
        public void ValidateOffloadRate_BadOffload5()
        {
            AdminControllerLogic sut = CreateMSTSut();

            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                Resource = "r",
                PerformingOrg = "p",
                SubcontractorResource = "s",
                HourlyRate = 55m,
                PercentToOffload = 1.05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2016, Id = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("PercentToOffload", messages.First().FieldName);
        }


        [TestMethod]
        public void ValidateOffloadRate_BadOffload6()
        {
            AdminControllerLogic sut = CreateMSTSut();

            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                Resource = "r",
                SubcontractorResource = "s",
                HourlyRate = 55m,
                PercentToOffload = .05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2016, Id = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("PerformingOrg", messages.First().FieldName);
        }

        [TestMethod]
        public void ValidateOffloadRate_BadOffload7()
        {
            AdminControllerLogic sut = CreateMSTSut();

            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                PerformingOrg = "p",
                Resource = "r",
                SubcontractorResource = "s",
                PercentToOffload = 0.05m,
                HourlyRate = 1000m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2016, Id = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("HourlyRate", messages.First().FieldName);
        }

        [TestMethod]
        public void ValidateOffloadRate_BadOffload8()
        {
            AdminControllerLogic sut = CreateMSTSut();

            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                PerformingOrg = "p",
                Resource = "r",
                SubcontractorResource = "s",
                PercentToOffload = 0.05m,
                HourlyRate = 100.888m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2016, Id = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("HourlyRate", messages.First().FieldName);
        }

        [TestMethod]
        public void ValidateOffloadRate_BadOffload9()
        {
            AdminControllerLogic sut = CreateMSTSut();

            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                PerformingOrg = "p",
                Resource = "r",
                SubcontractorResource = "s",
                HourlyRate = 55m,
                PercentToOffload = 0.88665m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2016, Id = 5 });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("PercentToOffload", messages.First().FieldName);
        }

        [TestMethod]
        public void ValidateOffloadRate_BadOffloadCombo()
        {
            AdminControllerLogic sut = CreateMSTSut();

            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                Resource = "r",
                PerformingOrg = "p",
                SubcontractorResource = "s",
                HourlyRate = 55m,
                PercentToOffload = 0.05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2016, Id = 4, Resource = "r", PerformingOrg = "p", SubResource = "s", Percent = 0.05m });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages.First().ValidationIssue.Contains("unique"));
        }

        /// <summary>
        /// Fails due to subcontractor resource not matching
        /// </summary>
        [TestMethod]
        public void ValidateOffloadRate_BaddOffload7()
        {
            AdminControllerLogic sut = CreateSut();
            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                Resource = "r",
                PerformingOrg = "p",
                SubcontractorResource = "s",
                HourlyRate = 55m,
                PercentToOffload = 0.05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
                rates.Add(new OffloadRatesDTO { Year = 2015, Id = 4, Resource = "r", PerformingOrg = "p", SubResource = "BAD", Percent = 0.05m });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages.First().ValidationIssue.Contains("same Subcontractor Resource"));
        }

        /// <summary>
        /// Fails due to percentage not matching
        /// </summary>
        [TestMethod]
        public void ValidateOffloadRate_BaddOffload8()
        {
            AdminControllerLogic sut = CreateSut();
            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                Resource = "r",
                PerformingOrg = "p",
                SubcontractorResource = "s",
                HourlyRate = 55m,
                PercentToOffload = 0.05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2015, Id = 4, Resource = "r", PerformingOrg = "p", SubResource = "s", Percent = 0.15m });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.IsTrue(messages.First().ValidationIssue.Contains("same Percent To Offload"));
        }

        /// <summary>
        /// Fails due to percentage as well as subcontractor resource not matching
        /// </summary>
        [TestMethod]
        public void ValidateOffloadRate_BaddOffload9()
        {
            AdminControllerLogic sut = CreateSut();
            OffloadRateModelView mv = new OffloadRateModelView
            {
                Year = 2016,
                OffloadRateID = 5,
                Resource = "r",
                PerformingOrg = "p",
                SubcontractorResource = "s",
                HourlyRate = 55m,
                PercentToOffload = 0.05m
            };

            List<OffloadRatesDTO> rates = new List<OffloadRatesDTO>();
            rates.Add(new OffloadRatesDTO { Year = 2015, Id = 4, Resource = "r", PerformingOrg = "p", SubResource = "BAD", Percent = 0.15m });

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateOffloadRate(mv, rates, messages);

            Assert.AreEqual(2, messages.Count);
            Assert.IsTrue(messages.ElementAt(0).ValidationIssue.Contains("same Subcontractor Resource"));
            Assert.IsTrue(messages.ElementAt(1).ValidationIssue.Contains("same Percent To Offload"));
        }

        #endregion

        #region ValidateSystemSettings

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateSystemSettings_EX1()
        {
            AdminControllerLogic sut = CreateSut();
            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateSystemSettings(null, messages);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateSystemSettings_EX2()
        {
            AdminControllerLogic sut = CreateSut();
            ICollection<SystemSettingDTO> ss = new Collection<SystemSettingDTO> {
                new SystemSettingDTO { Key = "a", Value = "aaa" }
            };

            sut.ValidateSystemSettings(ss, null);
        }

        [TestMethod]
        public void ValidateSystemSettings_NullKey()
        {
            AdminControllerLogic sut = CreateSut();
            ICollection<SystemSettingDTO> ss = new Collection<SystemSettingDTO> {
                new SystemSettingDTO { Key = string.Empty, Value = "aaa" }
            };

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateSystemSettings(ss, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("The Key field is required.", messages.First().ValidationIssue);
        }

        [TestMethod]
        public void ValidateSystemSettings_KeyTooLong()
        {
            AdminControllerLogic sut = CreateSut();
            ICollection<SystemSettingDTO> ss = new Collection<SystemSettingDTO> {
                new SystemSettingDTO { Key = new string('a', 256), Value = new string('a', 4000) }
            };

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateSystemSettings(ss, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("The field Key must be a string with a maximum length of 255.", messages.First().ValidationIssue);
        }

        [TestMethod]
        public void ValidateSystemSettings_ValueTooLong()
        {
            AdminControllerLogic sut = CreateSut();
            ICollection<SystemSettingDTO> ss = new Collection<SystemSettingDTO> {
                new SystemSettingDTO { Key = new string('a', 255), Value = new string('a', 4001) }
            };

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateSystemSettings(ss, messages);

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("The field Value must be a string with a maximum length of 4000.", messages.First().ValidationIssue);
        }

        [TestMethod]
        public void ValidateSystemSettings_OK()
        {
            AdminControllerLogic sut = CreateSut();
            ICollection<SystemSettingDTO> ss = new Collection<SystemSettingDTO> {
                new SystemSettingDTO { Key = new string('a', 255), Value = new string('a', 4000) }
            };

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            sut.ValidateSystemSettings(ss, messages);

            Assert.AreEqual(0, messages.Count);
        }

        #endregion

        #region Validate Template

        /// <summary>
        /// Test ValidateTemplateOnSaveOrEdit() with everything valid
        /// </summary>
        [TestMethod]
        public void TestValidateTemplateOnSaveOrEdit()
        {
            AdminControllerLogic sut = this.CreateSut();

            Collection<ValidationMessage> result = sut.ValidateTemplateOnSaveOrEdit("name", "desc");

            Assert.IsFalse(result.Any());
        }

        /// <summary>
        /// Test ValidateTemplateOnSaveOrEdit() with missing name
        /// </summary>
        [TestMethod]
        public void TestValidateTemplateOnSaveOrEdit_NoName()
        {
            AdminControllerLogic sut = this.CreateSut();

            Collection<ValidationMessage> result = sut.ValidateTemplateOnSaveOrEdit(null, "desc");

            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result.First().ValidationIssue, "Template Name is required.");
        }

        /// <summary>
        /// Test ValidateTemplateOnSaveOrEdit() with missing description
        /// </summary>
        [TestMethod]
        public void TestValidateTemplateOnSaveOrEdit_NoDesc()
        {
            AdminControllerLogic sut = this.CreateSut();

            Collection<ValidationMessage> result = sut.ValidateTemplateOnSaveOrEdit("name", null);

            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result.First().ValidationIssue, "Template Description is required.");
        }

        /// <summary>
        /// Test ValidateTemplateOnSaveOrEdit() with invalid characters in the name
        /// </summary>
        [TestMethod]
        public void TestValidateTemplateOnSaveOrEdit_InvalidCharacters()
        {
            AdminControllerLogic sut = this.CreateSut();

            // * is an invalid character
            Collection<ValidationMessage> result = sut.ValidateTemplateOnSaveOrEdit("name*", "desc");

            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result.First().ValidationIssue, "Template Name cannot contain any of the following reserved characters: < > : \" ' \\ / | ? *");
        }

        #endregion
    }
}