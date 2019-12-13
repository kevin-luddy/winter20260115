// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// T&M Resource Rate DataLoader Tests.
    /// </summary>
    /// <seealso cref="GenBOE.Tests.DAL.DataLoaders.MOQLoaderObject" />
    [TestClass]
    public class TMResourceRateDTODataLoaderTest : MOQLoaderObject
    {

        private static ITMResourceRateDTODataLoader sut = new TMResourceRateDTODataLoader();

        /// <summary>
        /// Test the Save and Get methods
        /// </summary>
        [TestMethod]
        public void L_SaveAndGetResourceRateDTOByID()
        {
            // create TMResourceRate
            TMResourceRateDTO resourceRate = new TMResourceRateDTO()
            {
                ResourceRateID = -1,
                WorkspaceID = this.Workspace.Id,
                ResourceID = this.Resource.Id,
                StartDate = GenBOEUtilities.AdjustDateTimePrecision(new DateTime(2011, 1, 15), DateTimePrecision.Month),
                EndDate = GenBOEUtilities.AdjustDateTimePrecision(new DateTime(2012, 1, 15), DateTimePrecision.Month),
                ResourceRate = 0.56m,
                Updateable = UpdateType.Upsert
            };

            // save it
            Dictionary<int, int> ids = sut.SaveTMResourceRates(new List<TMResourceRateDTO>() { resourceRate });

            // test
            TMResourceRateDTO result = sut.GetByIds(new Collection<int> { ids[-1] }).FirstOrDefault();

            Assert.IsTrue(resourceRate.ResourceRateID > 0, "T&M resource rate id valid");
            // verify it's the same.

            Type dtoType = typeof(TMResourceRateDTO);

            int numProperties = dtoType.GetProperties().Count();

            // 5 properties we can't test (Id, ResourceRateId, update date, updateDateLong, updatable), plus the 5 below 
            Assert.AreEqual(5 + 6, numProperties, "Untested properties exist in the DTO.");

            // These next 6 statements are the reason why the above number is 6.
            Assert.AreEqual(resourceRate.ResourceRate, result.ResourceRate);
            Assert.AreEqual(resourceRate.WorkspaceID, result.WorkspaceID);
            Assert.AreEqual(resourceRate.ResourceID, result.ResourceID);
            Assert.AreEqual(resourceRate.StartDate, result.StartDate);
            Assert.AreEqual(resourceRate.EndDate, result.EndDate);
            // 5
            Assert.AreEqual(resourceRate.LockedRate, result.LockedRate);

            // Update Test

            result.Updateable = UpdateType.Upsert;
            result.StartDate = GenBOEUtilities.AdjustDateTimePrecision(new DateTime(2016, 1, 15), DateTimePrecision.Month);
            result.EndDate = GenBOEUtilities.AdjustDateTimePrecision(new DateTime(2017, 1, 15), DateTimePrecision.Month);
            result.ResourceRate = .56m;

            int currentResourceRateID = result.ResourceRateID;

            // save it
            ids = sut.SaveTMResourceRates(new List<TMResourceRateDTO>() { result });

            // test
            result = sut.GetByIds(new Collection<int> { currentResourceRateID }).FirstOrDefault();

            Assert.AreEqual(.56m, result.ResourceRate);
            Assert.AreEqual(GenBOEUtilities.AdjustDateTimePrecision(new DateTime(2016, 1, 15), DateTimePrecision.Month), result.StartDate);
            Assert.AreEqual(GenBOEUtilities.AdjustDateTimePrecision(new DateTime(2017, 1, 15), DateTimePrecision.Month), result.EndDate);

            // Delete Test
            int total = sut.GetByWorkspaceId(this.Workspace.Id).Count();
            result.Updateable = UpdateType.Deleted;
            ids = sut.SaveTMResourceRates(new List<TMResourceRateDTO>() { result });

            Assert.AreEqual(total - 1, sut.GetByWorkspaceId(this.Workspace.Id).Count());

            this.ResetTestData();
        }

        /// <summary>
        /// Test Get by WorkspaceID
        /// </summary>
        [TestMethod]
        public void L_GetResourceRateIDsByWorkspaceID()
        {
            // create TMResourceRate
            TMResourceRateDTO resourceRate1 = new TMResourceRateDTO()
            {
                ResourceRateID = -1,
                WorkspaceID = this.Workspace.Id,
                ResourceID = this.Resource.Id,
                StartDate = GenBOEUtilities.AdjustDateTimePrecision(new DateTime(2015, 1, 15), DateTimePrecision.Month),
                EndDate = GenBOEUtilities.AdjustDateTimePrecision(new DateTime(2016, 1, 15), DateTimePrecision.Month),
                ResourceRate = 1.23m,
                Updateable = UpdateType.Upsert
            };
            TMResourceRateDTO resourceRate2 = new TMResourceRateDTO()
            {
                ResourceRateID = -2,
                WorkspaceID = this.Workspace.Id,
                ResourceID = this.Resource.Id,
                StartDate = GenBOEUtilities.AdjustDateTimePrecision(new DateTime(2015, 1, 15), DateTimePrecision.Month),
                EndDate = GenBOEUtilities.AdjustDateTimePrecision(new DateTime(2016, 1, 15), DateTimePrecision.Month),
                ResourceRate = 1.23m,
                Updateable = UpdateType.Upsert
            };
            TMResourceRateDTO resourceRate3 = new TMResourceRateDTO()
            {
                ResourceRateID = -3,
                WorkspaceID = this.Workspace.Id,
                ResourceID = this.Resource.Id,
                StartDate = GenBOEUtilities.AdjustDateTimePrecision(new DateTime(2015, 1, 15), DateTimePrecision.Month),
                EndDate = GenBOEUtilities.AdjustDateTimePrecision(new DateTime(2016, 1, 15), DateTimePrecision.Month),
                ResourceRate = 1.23m,
                Updateable = UpdateType.Upsert
            };

            // save it
            Dictionary<int, int> ids = sut.SaveTMResourceRates(new List<TMResourceRateDTO>() { resourceRate1, resourceRate2, resourceRate3 });
            Assert.IsTrue(resourceRate1.ResourceRateID > 0, "T&M resource rate id valid");
            Assert.IsTrue(resourceRate2.ResourceRateID > 0, "T&M resource rate id valid");
            Assert.IsTrue(resourceRate3.ResourceRateID > 0, "T&M resource rate id valid");

            // test
            ICollection<TMResourceRateDTO> result = sut.GetByWorkspaceId(this.Workspace.Id);

            Assert.IsTrue(result.FirstOrDefault(i => i.ResourceRateID == ids[-1]) != null &&
                          result.FirstOrDefault(i => i.ResourceRateID == ids[-2]) != null &&
                          result.FirstOrDefault(i => i.ResourceRateID == ids[-3]) != null);
        }

        /// <summary>
        /// Test save with missing argument.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void L_SaveTMResourceRatesWithError()
        {
            sut.SaveTMResourceRates(null);
        }

        /// <summary>
        /// Test save without Updateable.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void L_SaveTMResourceRatesWithoutUpdateable()
        {
            Collection<TMResourceRateDTO> toSave = new Collection<TMResourceRateDTO>() {
                new TMResourceRateDTO() {
                    ResourceRateID = 1
                }
            };

            sut.SaveTMResourceRates(toSave);
        }
    }
}
