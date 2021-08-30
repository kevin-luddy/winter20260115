// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.ActionLogic
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using GenTRAC.ActionLogic.GeneralHelper;
    using GenTRAC.DataBridge.DTO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the Sort Helper
    /// </summary>
    [TestClass]
    public class SortHelperTest
    {
        /// <summary>
        /// Tests the Sorter - null sort-by field
        /// </summary>
        [TestMethod]
        public void TestSorter_NoSort()
        {
            List<ProposalDto> rawData = CreateDummyData();

            SortHelper<ProposalDto> sorter = new SortHelper<ProposalDto>();

            List<ProposalDto> actual = sorter.Sort(rawData, string.Empty, SortOrder.Ascending).ToList();

            Assert.AreSame(rawData.ElementAt(0), actual.ElementAt(0));
            Assert.AreSame(rawData.ElementAt(1), actual.ElementAt(1));
            Assert.AreSame(rawData.ElementAt(2), actual.ElementAt(2));
            Assert.AreSame(rawData.ElementAt(3), actual.ElementAt(3));
            Assert.AreSame(rawData.ElementAt(4), actual.ElementAt(4));
        }

        /// <summary>
        /// Tests the Sorter
        /// </summary>
        [TestMethod]
        public void TestSorter_FullSort()
        {
            List<ProposalDto> rawData = CreateDummyData();

            SortHelper<ProposalDto> sorter = new SortHelper<ProposalDto>();

            List<ProposalDto> sortedByCaptureName = sorter.Sort(rawData, "ProposalTitle", SortOrder.Ascending).ToList();

            Assert.AreSame(rawData.ElementAt(0), sortedByCaptureName.ElementAt(0));
            Assert.AreSame(rawData.ElementAt(3), sortedByCaptureName.ElementAt(1));
            Assert.AreSame(rawData.ElementAt(4), sortedByCaptureName.ElementAt(2));
            Assert.AreSame(rawData.ElementAt(2), sortedByCaptureName.ElementAt(3));
            Assert.AreSame(rawData.ElementAt(1), sortedByCaptureName.ElementAt(4));

            List<ProposalDto> sortedByCaptureNameDesc = sorter.Sort(rawData, "ProposalTitle", SortOrder.Descending).ToList();

            Assert.AreSame(rawData.ElementAt(1), sortedByCaptureNameDesc.ElementAt(0));
            Assert.AreSame(rawData.ElementAt(2), sortedByCaptureNameDesc.ElementAt(1));
            Assert.AreSame(rawData.ElementAt(3), sortedByCaptureNameDesc.ElementAt(3));
            Assert.AreSame(rawData.ElementAt(4), sortedByCaptureNameDesc.ElementAt(2));
            Assert.AreSame(rawData.ElementAt(0), sortedByCaptureNameDesc.ElementAt(4));
        }

        /// <summary>
        /// Creates Dummy Data
        /// </summary>
        /// <returns>Test Data</returns>
        private static List<ProposalDto> CreateDummyData()
        {
            List<ProposalDto> rawData = new List<ProposalDto>();

            rawData.Add(new ProposalDto()
            {
                ProposalTitle = "Hello"
            });

            rawData.Add(new ProposalDto()
            {
                ProposalTitle = "World!"
            });

            rawData.Add(new ProposalDto()
            {
                ProposalTitle = "This"
            });

            rawData.Add(new ProposalDto()
            {
                ProposalTitle = "is a boring"
            });

            rawData.Add(new ProposalDto()
            {
                ProposalTitle = "test."
            });
            return rawData;
        }
    }
}
