// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Offload Rates DataLoader Tests.
    /// </summary>
    /// <seealso cref="GenBOE.Tests.DAL.DataLoaders.MOQLoaderObject" />
    [TestClass]
    public class OffloadRatesDTODataLoaderTest : MOQLoaderObject
    {
        /// <summary>
        /// Tests the Get All System Offload DTOs method.
        /// </summary>
        [TestMethod]
        public void L_GetAllSystemOffloadDTOs()
        {
            var sut = new OffloadRatesDTOLoader();

            ICollection<OffloadRatesDTO> systemRates = sut.GetAllSystemRates();

            Assert.IsNotNull(systemRates);
            // Commenting out this assert since DAL tests run against Space and not RMS
            // Assert.IsTrue(systemRates.Count() > 0);
        }

        /// <summary>
        /// Tests modifying the System DTOs.
        /// </summary>
        [TestMethod]
        public void L_ModifySystemDTOs()
        {
            var sut = new OffloadRatesDTOLoader();

            OffloadRatesDTO dto = CreateDto();

            int newId;
            using (TransactionScope scope = new TransactionScope())
            {
                newId = sut.Save(dto).Value;
                scope.Complete();
            }

            ICollection<OffloadRatesDTO> systemRates = sut.GetAllSystemRates();

            OffloadRatesDTO savedDto = systemRates.FirstOrDefault(s => s.Id == newId);
            Assert.IsNotNull(savedDto, "System Rate did not save.");

            AssertEquality(dto, savedDto);

            savedDto.Updateable = IES.Common.UpdateType.Deleted;
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(savedDto);
                scope.Complete();
            }

            systemRates = sut.GetAllSystemRates();

            savedDto = systemRates.FirstOrDefault(s => s.Id == newId);
            Assert.IsNull(savedDto, "System Rate did not delete.");
        }

        /// <summary>
        /// Tests the Get system offload rates by id(s) methods.
        /// </summary>
        [TestMethod]
        public void L_GetSystemOffloadRatesByIds()
        {
            var sut = new OffloadRatesDTOLoader();

            OffloadRatesDTO dto = CreateDto();

            int newId;
            using (TransactionScope scope = new TransactionScope())
            {
                newId = sut.Save(dto).Value;
                scope.Complete();
            }

            ICollection<OffloadRatesDTO> systemRates = sut.GetAllSystemRates();

            ICollection<OffloadRatesDTO> systemRatesByIds = sut.GetByIds(systemRates.Select(s => s.Id).ToList());

            Assert.AreEqual(systemRates.Count, systemRatesByIds.Count);
            AssertEquality(dto, systemRatesByIds.First(s => s.Id == newId));

            OffloadRatesDTO savedDto = sut.GetById(newId);
            AssertEquality(dto, savedDto);

            savedDto.Updateable = IES.Common.UpdateType.Deleted;
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(savedDto);
                scope.Complete();
            }
        }

        /// <summary>
        /// Test that CopySystemDefaultOffloadRates correctly copies the system rates
        /// </summary>
        [TestMethod]
        public void L_CopySystemDefaultOffloadRates()
        {
            OffloadRatesDTOLoader sut = new OffloadRatesDTOLoader();

            int workspaceId = GlobalTestCaseSetup.GlobalWorkspaceID;

            OffloadRatesDTO systemDto = this.CreateDto();

            int newId;
            using (TransactionScope scope = new TransactionScope())
            {
                newId = sut.Save(systemDto).Value;
                scope.Complete();
            }
            OffloadRatesDTO savedSystemDto = sut.GetById(newId);
            ICollection<OffloadRatesDTO> allSystemRates = sut.GetAllSystemRates();

            sut.CopySystemDefaultOffloadRates(workspaceId);
            ICollection<OffloadRatesDTO> allWorkspaceRates = sut.GetByWorkspaceId(workspaceId);

            Assert.AreEqual(allSystemRates.Count, allWorkspaceRates.Count);
            for (int i = 0; i < allSystemRates.Count; i++)
            {
                Assert.AreEqual(allSystemRates.ElementAt(i).Year, allWorkspaceRates.ElementAt(i).Year);
                Assert.AreEqual(allSystemRates.ElementAt(i).HourlyRate, allWorkspaceRates.ElementAt(i).HourlyRate);
                Assert.AreEqual(allSystemRates.ElementAt(i).Percent, allWorkspaceRates.ElementAt(i).Percent);
                Assert.AreEqual(allSystemRates.ElementAt(i).PerformingOrg, allWorkspaceRates.ElementAt(i).PerformingOrg);
                Assert.AreEqual(allSystemRates.ElementAt(i).Resource, allWorkspaceRates.ElementAt(i).Resource);
                Assert.AreEqual(allSystemRates.ElementAt(i).SubResource, allWorkspaceRates.ElementAt(i).SubResource);
            }
        }

        /// <summary>
        /// Test that CopySystemDefaultOffloadRates throws appropriate exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void L_CopySystemDefaultOffloadRates_ArgumentException()
        {
            OffloadRatesDTOLoader sut = new OffloadRatesDTOLoader();

            // workspaceId < 1 will throw an Argument Exception
            sut.CopySystemDefaultOffloadRates(0);
        }

        /// <summary>
        /// Tests AreCurrentOffloadRatesOutOfDate when system and workspace rate counts are different
        /// </summary>
        [TestMethod]
        public void L_AreCurrentOffloadRatesOutOfDate_DifferentRateCounts()
        {
            OffloadRatesDTOLoader sut = new OffloadRatesDTOLoader();

            int workspaceId = GlobalTestCaseSetup.GlobalWorkspaceID;

            OffloadRatesDTO systemDto = this.CreateDto();

            int newId;
            using (TransactionScope scope = new TransactionScope())
            {
                newId = sut.Save(systemDto).Value;
                scope.Complete();
            }

            bool result = sut.AreCurrentOffloadRatesOutOfDate(workspaceId);

            Assert.IsTrue(result);

            OffloadRatesDTO savedSystemDto = sut.GetById(newId);
            savedSystemDto.Updateable = UpdateType.Deleted;
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(savedSystemDto);
                scope.Complete();
            }
        }

        /// <summary>
        /// Tests AreCurrentOffloadRatesOutOfDate when there is a difference between a system and workspace rate
        /// </summary>
        [TestMethod]
        public void L_AreCurrentOffloadRatesOutOfDate_RateDifferences()
        {
            OffloadRatesDTOLoader sut = new OffloadRatesDTOLoader();

            int workspaceId = GlobalTestCaseSetup.GlobalWorkspaceID;

            OffloadRatesDTO systemDto = this.CreateDto();
            int newId;
            using (TransactionScope scope = new TransactionScope())
            {
                newId = sut.Save(systemDto).Value;
                scope.Complete();
            }

            OffloadRatesDTO savedSystemDto = sut.GetById(newId);

            // Copy the system rates to the workspace
            using (TransactionScope scope = new TransactionScope())
            {
                sut.CopySystemDefaultOffloadRates(workspaceId);
                scope.Complete();
            }

            ICollection<OffloadRatesDTO> savedWorkspaceDtos = sut.GetByWorkspaceId(workspaceId);

            // Adjust the system dto to make the workspace rates out of date
            savedSystemDto.Updateable = UpdateType.Upsert;
            savedSystemDto.HourlyRate += 1;
            using (TransactionScope scope = new TransactionScope())
            {
                sut.Save(savedSystemDto);
                scope.Complete();
            }

            bool result = sut.AreCurrentOffloadRatesOutOfDate(workspaceId);

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests AreCurrentOffloadRatesOutOfDate when there are no differences between system and workspace rates
        /// </summary>
        [TestMethod]
        public void L_AreCurrentOffloadRatesOutOfDate_RatesUpToDate()
        {
            OffloadRatesDTOLoader sut = new OffloadRatesDTOLoader();

            int workspaceId = GlobalTestCaseSetup.GlobalWorkspaceID;

            OffloadRatesDTO systemDto = this.CreateDto();

            int newId;
            using (TransactionScope scope = new TransactionScope())
            {
                newId = sut.Save(systemDto).Value;
                scope.Complete();
            }

            OffloadRatesDTO savedSystemDto = sut.GetById(newId);

            sut.CopySystemDefaultOffloadRates(workspaceId);
            ICollection<OffloadRatesDTO> savedWorkspaceDtos = sut.GetByWorkspaceId(workspaceId);

            bool result = sut.AreCurrentOffloadRatesOutOfDate(workspaceId);

            Assert.IsFalse(result);
        }

        /// <summary>
        /// Asserts the equality between two offload rates.
        /// </summary>
        /// <param name="expectedRate">The dto.</param>
        /// <param name="actualRate">The saved dto.</param>
        private void AssertEquality(OffloadRatesDTO expectedRate, OffloadRatesDTO actualRate)
        {
            Assert.AreEqual(expectedRate.HourlyRate, actualRate.HourlyRate);
            Assert.AreEqual(expectedRate.Percent, actualRate.Percent);
            Assert.AreEqual(expectedRate.PerformingOrg, actualRate.PerformingOrg);
            Assert.AreEqual(expectedRate.Resource, actualRate.Resource);
            Assert.AreEqual(expectedRate.SubResource, actualRate.SubResource);
            Assert.AreEqual(expectedRate.Year, actualRate.Year);
        }

        /// <summary>
        /// Creates the dto.
        /// </summary>
        /// <returns>New DTO created, but not yet saved.</returns>
        private OffloadRatesDTO CreateDto()
        {
            return new OffloadRatesDTO
            {
                HourlyRate = 55.44m,
                Id = -1,
                Percent = 0.05m,
                PerformingOrg = "SA15",
                Resource = Guid.NewGuid().ToString().Remove(0, 16),
                SubResource = Guid.NewGuid().ToString().Remove(0, 16),
                Year = 2017,
                Updateable = IES.Common.UpdateType.Upsert
            };
        }
    }
}
