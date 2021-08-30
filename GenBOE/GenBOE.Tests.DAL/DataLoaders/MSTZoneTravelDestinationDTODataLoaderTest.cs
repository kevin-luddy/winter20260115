// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;

    [TestClass]
    public class MSTZoneTravelDestinationDTODataLoaderTest : MOQLoaderObject
    {
        MSTZoneTravelDestinationDTODataLoader sut = null;

        [TestInitialize]
        public void Init()
        {
            sut = new MSTZoneTravelDestinationDTODataLoader();
        }

        /// <summary>
        /// Tests all methods of the MSTZoneTravelDestinationDTODataLoader
        /// Modifies the zone of an existing destination but sets it back to its original value
        /// </summary>
        [TestMethod]
        public void TestDestinationDataLoader()
        {
            // Test GetAllDestinations
            ICollection<MSTZoneTravelDestinationDTO> AllDestinations = sut.GetAllDestinations();

            // Assert
            Assert.IsTrue(AllDestinations.Any(), "No Destinations exist.");

            // Get single DTO for testing purposes
            // Above assert confirms the collection will have at least one entry, so no need for FirstOrDefault
            MSTZoneTravelDestinationDTO testDTO = AllDestinations.First();

            // Test GetDestinationByDestinationID
            MSTZoneTravelDestinationDTO getByIDDTO = sut.GetDestinationByDestinationID(testDTO.DestinationID);

            // Asserts
            Assert.IsTrue(testDTO.DestinationID == getByIDDTO.DestinationID, "DestinationIDs do not match.");
            Assert.IsTrue(testDTO.Destination == getByIDDTO.Destination, "Destinations do not match.");
            Assert.IsTrue(testDTO.Abbreviation == getByIDDTO.Abbreviation, "Abbreviations do not match");
            Assert.IsTrue(testDTO.Zone == getByIDDTO.Zone, "Zones do not match.");

            // Test GetByIDs
            ICollection<MSTZoneTravelDestinationDTO> getByIDsDTOs = sut.GetByIDs(new Collection<int>() { testDTO.DestinationID });

            // Asserts
            Assert.IsTrue(getByIDsDTOs.Any(), "GetByIDs returned no results.");
            Assert.IsTrue(testDTO.DestinationID == getByIDsDTOs.First().DestinationID, "DestinationIDs do not match.");
            Assert.IsTrue(testDTO.Destination == getByIDsDTOs.First().Destination, "Destinations do not match.");
            Assert.IsTrue(testDTO.Abbreviation == getByIDsDTOs.First().Abbreviation, "Abbreviations do not match");
            Assert.IsTrue(testDTO.Zone == getByIDsDTOs.First().Zone, "Zones do not match.");

            // Test SaveDestination
            int oldZone = testDTO.Zone;
            int newZone = oldZone == 6 ? 1 : oldZone + 1;
            testDTO.Zone = newZone;
            sut.SaveDestination(testDTO);
            MSTZoneTravelDestinationDTO updatedTestDTO = sut.GetDestinationByDestinationID(testDTO.DestinationID);

            // Assert
            Assert.IsTrue(updatedTestDTO.Zone == newZone, "Zone was not updated properly.");

            // Set Zone back to original value
            testDTO.Zone = oldZone;
            sut.SaveDestination(testDTO);
            updatedTestDTO = sut.GetDestinationByDestinationID(testDTO.DestinationID);

            // Assert
            Assert.IsTrue(updatedTestDTO.Zone == oldZone, "Zone was not returned to original value.");
        }
    }
}
