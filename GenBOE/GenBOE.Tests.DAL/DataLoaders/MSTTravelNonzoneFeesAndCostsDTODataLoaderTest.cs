// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;

    [TestClass]
    public class MSTTravelNonzoneFeesAndCostsDTODataLoaderTest : MOQLoaderObject
    {
        MSTTravelNonzoneFeesAndCostsDTODataLoader sut = null;

        [TestInitialize]
        public void init()
        {
            sut = new MSTTravelNonzoneFeesAndCostsDTODataLoader();
        }

        [TestMethod]
        public void TestMSTTravelNonzoneFeesAndCostsDTODataLoader()
        {
            //Test getAllFeesAndCosts()
            ICollection<MSTTravelNonzoneFeesAndCostsDTO> allFeesAndCosts = sut.getAllFeesAndCosts();

            Assert.IsTrue(allFeesAndCosts.Any());

            // Get single DTO for testing purposes
            // Above assert confirms the collection will have at least one entry, so no need for FirstOrDefault
            MSTTravelNonzoneFeesAndCostsDTO testDTO = allFeesAndCosts.First();

            //Test getFeesAndCostsByModeID()
            MSTTravelNonzoneFeesAndCostsDTO actualDTO = sut.getFeesAndCostsByModeID(testDTO.ModeID);

            Assert.AreEqual(testDTO.ModeID, actualDTO.ModeID);
            Assert.AreEqual(testDTO.TravelAgencyFee, actualDTO.TravelAgencyFee);
            Assert.AreEqual(testDTO.MiscOther, actualDTO.MiscOther);
            Assert.AreEqual(testDTO.UpdateDate, actualDTO.UpdateDate);

            //Test saveFeesAndCosts()
            decimal oldTravelAgencyFee = testDTO.TravelAgencyFee;
            decimal oldMiscOther = testDTO.MiscOther;

            testDTO.TravelAgencyFee = 99;
            testDTO.MiscOther = 75;

            sut.saveFeesAndCosts(testDTO);
            actualDTO = sut.getFeesAndCostsByModeID(testDTO.ModeID);

            Assert.AreEqual(testDTO.TravelAgencyFee, actualDTO.TravelAgencyFee);
            Assert.AreEqual(testDTO.MiscOther, actualDTO.MiscOther);

            //Set back to original values
            testDTO.TravelAgencyFee = oldTravelAgencyFee;
            testDTO.MiscOther = oldMiscOther;
            sut.saveFeesAndCosts(testDTO);
            actualDTO = sut.getFeesAndCostsByModeID(testDTO.ModeID);
            Assert.AreEqual(testDTO.TravelAgencyFee, oldTravelAgencyFee);
            Assert.AreEqual(testDTO.MiscOther, oldMiscOther);
        }
    }
}
