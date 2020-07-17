// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using IES.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;

    [TestClass]
    public class MSTZoneTravelOriginDTODataLoaderTest : MOQLoaderObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        MSTZoneTravelOriginDTODataLoader sut = null;

        [TestInitialize]
        public void Init()
        {
            sut = new MSTZoneTravelOriginDTODataLoader();
        }

        /// <summary>
        /// Tests all methods of the MSTZoneTravelOriginDTODataLoader, except GetAllOrigins()
        /// Creates, updates, and deletes an Origin and its Resources
        /// </summary>
        [TestMethod]
        public void TestOriginDataLoader()
        {
            string originName = "Test Location";
            string newOriginName = "New Test Location";
            // first try to retrieve and delete the "Test Location", "New Test Location" origins in case other test runs left it there
            ICollection<MSTZoneTravelOriginDTO> allorigins = sut.GetAllOrigins();
            if (allorigins.Any(o => o.Origin == originName))
            {
                MSTZoneTravelOriginDTO toDelete = allorigins.First(o => o.Origin == originName);
                toDelete.Updateable = UpdateType.Deleted;
                sut.SaveOrigin(toDelete, null);
            }

            if (allorigins.Any(o => o.Origin == newOriginName))
            {
                MSTZoneTravelOriginDTO toDelete = allorigins.First(o => o.Origin == newOriginName);
                toDelete.Updateable = UpdateType.Deleted;
                sut.SaveOrigin(toDelete, null);
            }

            // Test saving new Origin
            // Set up Origin DTO
            MSTZoneTravelOriginDTO testOriginDTO = new MSTZoneTravelOriginDTO();
            testOriginDTO.Origin = originName;
            testOriginDTO.Site = "T";
            testOriginDTO.Updateable = UpdateType.Upsert;

            // Set up Resource DTOs
            Collection<MSTZoneTravelResourceDTO> testResDTOs = new Collection<MSTZoneTravelResourceDTO>();
            testResDTOs.Add(new MSTZoneTravelResourceDTO(null)); //will be set to NO-RATE
            testResDTOs.Add(new MSTZoneTravelResourceDTO("TPRZ2"));
            testResDTOs.Add(new MSTZoneTravelResourceDTO("TPRZ3"));
            testResDTOs.Add(new MSTZoneTravelResourceDTO("TPRZ4"));
            testResDTOs.Add(new MSTZoneTravelResourceDTO("TPRZ5"));
            testResDTOs.Add(new MSTZoneTravelResourceDTO("TPRZ6"));
            testResDTOs.Add(new MSTZoneTravelResourceDTO("TTRZ1"));
            testResDTOs.Add(new MSTZoneTravelResourceDTO("TTRZ2"));
            testResDTOs.Add(new MSTZoneTravelResourceDTO("TTRZ3"));
            testResDTOs.Add(new MSTZoneTravelResourceDTO("TTRZ4"));
            testResDTOs.Add(new MSTZoneTravelResourceDTO("TTRZ5"));
            testResDTOs.Add(new MSTZoneTravelResourceDTO("TTRZ6"));

            Dictionary<int, int> idDictionary = sut.SaveOrigin(testOriginDTO, testResDTOs);

            // Assert
            Assert.IsTrue(idDictionary.ContainsKey(testOriginDTO.OriginID), "Origin was not added.");
            Assert.IsTrue(idDictionary[-1] != testOriginDTO.OriginID, "Origin ID was not updated.");
            testOriginDTO.OriginID = idDictionary[-1];

            // Test GetOriginResourcesByOriginID
            Collection<MSTZoneTravelResourceDTO> savedResDTOs = sut.GetOriginResourcesByOriginID(testOriginDTO.OriginID).ToCollection();

            // Assert
            Assert.IsTrue(savedResDTOs[0].Resource == "NO-RATE", "PR Zone 1 Resource does not match.");
            Assert.IsTrue(savedResDTOs[1].Resource == testResDTOs[1].Resource, "PR Zone 2 Resource does not match.");
            Assert.IsTrue(savedResDTOs[2].Resource == testResDTOs[2].Resource, "PR Zone 3 Resource does not match.");
            Assert.IsTrue(savedResDTOs[3].Resource == testResDTOs[3].Resource, "PR Zone 4 Resource does not match.");
            Assert.IsTrue(savedResDTOs[4].Resource == testResDTOs[4].Resource, "PR Zone 5 Resource does not match.");
            Assert.IsTrue(savedResDTOs[5].Resource == testResDTOs[5].Resource, "PR Zone 6 Resource does not match.");
            Assert.IsTrue(savedResDTOs[6].Resource == testResDTOs[6].Resource, "TR Zone 1 Resource does not match.");
            Assert.IsTrue(savedResDTOs[7].Resource == testResDTOs[7].Resource, "TR Zone 2 Resource does not match.");
            Assert.IsTrue(savedResDTOs[8].Resource == testResDTOs[8].Resource, "TR Zone 3 Resource does not match.");
            Assert.IsTrue(savedResDTOs[9].Resource == testResDTOs[9].Resource, "TR Zone 4 Resource does not match.");
            Assert.IsTrue(savedResDTOs[10].Resource == testResDTOs[10].Resource, "TR Zone 5 Resource does not match.");
            Assert.IsTrue(savedResDTOs[11].Resource == testResDTOs[11].Resource, "TR Zone 6 Resource does not match.");
            // Since all are expected to be false, using OR logic one value being true will cause the assert to fail
            Assert.IsFalse(savedResDTOs[0].IsAirfare || savedResDTOs[1].IsAirfare || savedResDTOs[2].IsAirfare || savedResDTOs[3].IsAirfare || savedResDTOs[4].IsAirfare || savedResDTOs[5].IsAirfare, "One of the per diem resources is set to airfare.");
            // Since all are expected to be true, using AND logic one value being false will cause the assert to fail
            Assert.IsTrue(savedResDTOs[6].IsAirfare && savedResDTOs[7].IsAirfare && savedResDTOs[8].IsAirfare && savedResDTOs[9].IsAirfare && savedResDTOs[10].IsAirfare && savedResDTOs[11].IsAirfare, "One of the airfare resources is set to per diem.");
            Assert.IsTrue(savedResDTOs[0].Zone == 1, "PR Zone 1 Resource is not set to Zone 1.");
            Assert.IsTrue(savedResDTOs[1].Zone == 2, "PR Zone 2 Resource is not set to Zone 2.");
            Assert.IsTrue(savedResDTOs[2].Zone == 3, "PR Zone 3 Resource is not set to Zone 3.");
            Assert.IsTrue(savedResDTOs[3].Zone == 4, "PR Zone 4 Resource is not set to Zone 4.");
            Assert.IsTrue(savedResDTOs[4].Zone == 5, "PR Zone 5 Resource is not set to Zone 5.");
            Assert.IsTrue(savedResDTOs[5].Zone == 6, "PR Zone 6 Resource is not set to Zone 6.");
            Assert.IsTrue(savedResDTOs[6].Zone == 1, "TR Zone 1 Resource is not set to Zone 1.");
            Assert.IsTrue(savedResDTOs[7].Zone == 2, "TR Zone 2 Resource is not set to Zone 2.");
            Assert.IsTrue(savedResDTOs[8].Zone == 3, "TR Zone 3 Resource is not set to Zone 3.");
            Assert.IsTrue(savedResDTOs[9].Zone == 4, "TR Zone 4 Resource is not set to Zone 4.");
            Assert.IsTrue(savedResDTOs[10].Zone == 5, "TR Zone 5 Resource is not set to Zone 5.");
            Assert.IsTrue(savedResDTOs[11].Zone == 6, "TR Zone 6 Resource is not set to Zone 6.");
            foreach(MSTZoneTravelResourceDTO dto in savedResDTOs)
            {
                Assert.AreEqual(testOriginDTO.OriginID, dto.OriginID);
            }


            // Update Resources with new IDs and other fields
            testResDTOs = savedResDTOs;

            // Test GetOriginByOriginID
            MSTZoneTravelOriginDTO getByIDDTO = sut.GetOriginByOriginID(testOriginDTO.OriginID);

            // Assert
            Assert.IsTrue(getByIDDTO.OriginID == testOriginDTO.OriginID, "Origin ID does not match.");
            Assert.IsTrue(getByIDDTO.Origin == testOriginDTO.Origin, "Origin does not match.");
            Assert.IsTrue(getByIDDTO.Site == testOriginDTO.Site, "Site does not match.");

            // Test GetByIDs
            ICollection<MSTZoneTravelOriginDTO> getByIDsDTOs = sut.GetByIDs(new Collection<int>() { testOriginDTO.OriginID });

            // Assert
            Assert.IsTrue(getByIDsDTOs.Any(), "GetByIDs returned no results.");
            Assert.IsTrue(getByIDsDTOs.First().OriginID == testOriginDTO.OriginID, "Origin ID does not match.");
            Assert.IsTrue(getByIDsDTOs.First().Origin == testOriginDTO.Origin, "Origin does not match.");
            Assert.IsTrue(getByIDsDTOs.First().Site == testOriginDTO.Site, "Site does not match.");

            // Test OriginExists
            // Should be false because it will only match itself
            Assert.IsFalse(sut.originExists(testOriginDTO));
            MSTZoneTravelOriginDTO duplicateOrigin = new MSTZoneTravelOriginDTO();
            duplicateOrigin.Origin = originName;
            duplicateOrigin.Site = "T2";
            // Should be true because Origin name matches the test origin
            Assert.IsTrue(sut.originExists(duplicateOrigin));

            // Test Update Origin
            testOriginDTO.Origin = newOriginName;
            testOriginDTO.Site = "X";
            testOriginDTO.Updateable = UpdateType.Upsert;
            testResDTOs[0].Resource = "XPRZ1";
            testResDTOs[1].Resource = "XPRZ2";
            testResDTOs[2].Resource = null;
            idDictionary = sut.SaveOrigin(testOriginDTO, testResDTOs);

            // Assert
            Assert.IsTrue(idDictionary.ContainsKey(testOriginDTO.OriginID), "Origin was not updated.");
            Assert.IsTrue(idDictionary[testOriginDTO.OriginID] == testOriginDTO.OriginID, "Origin ID was changed.");
            getByIDDTO = sut.GetOriginByOriginID(testOriginDTO.OriginID);
            Assert.IsTrue(getByIDDTO.OriginID == testOriginDTO.OriginID, "Origin ID does not match.");
            Assert.IsTrue(getByIDDTO.Origin == testOriginDTO.Origin, "Origin does not match.");
            Assert.IsTrue(getByIDDTO.Site == testOriginDTO.Site, "Site does not match.");
            testOriginDTO = getByIDDTO;

            savedResDTOs = sut.GetOriginResourcesByOriginID(testOriginDTO.OriginID).ToCollection();
            Assert.IsTrue(savedResDTOs[0].Resource == testResDTOs[0].Resource, "PR Zone 1 Resource does not match.");
            Assert.IsTrue(savedResDTOs[1].Resource == testResDTOs[1].Resource, "PR Zone 2 Resource does not match.");
            Assert.IsTrue(savedResDTOs[2].Resource == "NO-RATE", "PR Zone 3 Resource does not match.");
            Assert.IsTrue(savedResDTOs[3].Resource == testResDTOs[3].Resource, "PR Zone 4 Resource does not match.");
            Assert.IsTrue(savedResDTOs[4].Resource == testResDTOs[4].Resource, "PR Zone 5 Resource does not match.");
            Assert.IsTrue(savedResDTOs[5].Resource == testResDTOs[5].Resource, "PR Zone 6 Resource does not match.");
            Assert.IsTrue(savedResDTOs[6].Resource == testResDTOs[6].Resource, "TR Zone 1 Resource does not match.");
            Assert.IsTrue(savedResDTOs[7].Resource == testResDTOs[7].Resource, "TR Zone 2 Resource does not match.");
            Assert.IsTrue(savedResDTOs[8].Resource == testResDTOs[8].Resource, "TR Zone 3 Resource does not match.");
            Assert.IsTrue(savedResDTOs[9].Resource == testResDTOs[9].Resource, "TR Zone 4 Resource does not match.");
            Assert.IsTrue(savedResDTOs[10].Resource == testResDTOs[10].Resource, "TR Zone 5 Resource does not match.");
            Assert.IsTrue(savedResDTOs[11].Resource == testResDTOs[11].Resource, "TR Zone 6 Resource does not match.");

            // Test Delete Origin
            testOriginDTO.Updateable = UpdateType.Deleted;
            sut.SaveOrigin(testOriginDTO, null);

            // Assert
            Assert.IsNull(sut.GetOriginByOriginID(testOriginDTO.OriginID), "Origin was not deleted.");
            Assert.IsTrue(!sut.GetOriginResourcesByOriginID(testOriginDTO.OriginID).Any(), "Origin's resources were not deleted.");
        }

        /// <summary>
        /// Tests GetAllOrigins method
        /// </summary>
        [TestMethod]
        public void L_GetAllOrigins()
        {
            ICollection<MSTZoneTravelOriginDTO> AllOrigins = sut.GetAllOrigins();
            Assert.IsTrue(AllOrigins.Any(), "GetAllOrigins() returned no results.");
        }
    }
}
