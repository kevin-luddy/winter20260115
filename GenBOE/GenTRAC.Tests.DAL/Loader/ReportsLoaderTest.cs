// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenTRAC.Tests.DAL.Loader
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.DataBridge.DTO.Reports;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test class for the ReportsLoaderTest
    /// </summary>
    [TestClass]
    public class ReportsLoaderTest
    {
        /// <summary>
        /// Test data
        /// </summary>
        private TestData testData = TestData.GetInstance();

        /// <summary>
        /// Reports loader
        /// </summary>
        /// <returns>loader</returns>
        private ReportsLoader CreateSystem()
        {
            return new ReportsLoader();
        }

        /// <summary>
        /// Get proposal years
        /// </summary>
        [TestMethod]
        public void L_GetProposalYears()
        {
            var sut = this.CreateSystem();

            // crete a proposal so we know there is at least one year in the system
            this.testData.GetProposal(inCreateNew: true);

            ICollection<int> afterYears = sut.GetProposalYears();

            Assert.IsTrue(afterYears.Contains(DateTime.Today.Year));
            Assert.IsTrue(afterYears.Any());
        }
    }
}
