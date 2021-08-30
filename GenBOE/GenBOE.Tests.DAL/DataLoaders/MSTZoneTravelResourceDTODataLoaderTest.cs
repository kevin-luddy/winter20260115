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
    using GenBOE.Models;

    [TestClass]
    public class MSTZoneTravelResourceDTODataLoaderTest : MOQLoaderObject
    {
        MSTZoneTravelResourceDTODataLoader sut = null;

        [TestInitialize]
        public void Init()
        {
            sut = new MSTZoneTravelResourceDTODataLoader();
        }

        /// <summary>
        /// Test the GetResourceByOriginZoneAndMode method
        /// </summary>
        [TestMethod]
        public void TestGetResourceByOriginZoneAndMode()
        {
            MSTZoneTravelResourceDTO expectedResource = GetTestResource();
            Assert.IsNotNull(expectedResource);

            MSTZoneTravelResourceDTO actualResource = sut.GetResourceByOriginZoneAndMode(expectedResource.OriginID, expectedResource.Zone, expectedResource.IsAirfare);

            Assert.AreEqual(expectedResource.ResourceID, actualResource.ResourceID);
            Assert.AreEqual(expectedResource.Resource, actualResource.Resource);
            Assert.AreEqual(expectedResource.OriginID, actualResource.OriginID);
            Assert.AreEqual(expectedResource.Zone, actualResource.Zone);
            Assert.AreEqual(expectedResource.IsAirfare, actualResource.IsAirfare);
            Assert.AreEqual(expectedResource.LookupValue, actualResource.LookupValue);
            Assert.AreEqual(expectedResource.Description, actualResource.Description);
        }

        /// <summary>
        /// Test that the GetAllResources method returns a result
        /// </summary>
        [TestMethod]
        public void TestGetAllResources()
        {
            ICollection<MSTZoneTravelResourceDTO> result = sut.GetAllResources();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// Gets a Resource from the db to aid in testing
        /// </summary>
        /// <returns>Resource DTO to aid in testing</returns>
        private MSTZoneTravelResourceDTO GetTestResource()
        {
            MSTZoneTravelResourceDTO toReturn;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from r in gbe.MSTZoneTravelResources
                            select new MSTZoneTravelResourceDTO
                            {
                                ResourceID = r.ResourceID,
                                Resource = r.Resource,
                                OriginID = r.OriginID,
                                Zone = r.Zone,
                                IsAirfare = r.isAirfare,
                                LookupValue = r.LookupValue,
                                Description = r.Description
                            }).FirstOrDefault();
            }

            return toReturn;
        }
    }
}
