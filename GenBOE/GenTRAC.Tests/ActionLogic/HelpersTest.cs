// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests.ActionLogic
{
    using System;
    using GenTRAC.ActionLogic.GeneralHelper;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Helpers Test class
    /// </summary>
    [TestClass]
    public class HelpersTest
    {
        /// <summary>
        /// Helpers Test 1
        /// </summary>
        [TestMethod]
        public void IsProposalCertificationLate1()
        {
            bool expected = false;
            DateTime? submittalDate = null;
            ProposalStatus status = ProposalStatus.Archived;

            bool actual = Helpers.IsProposalCertificationLate(status, submittalDate);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Helpers Test 2
        /// </summary>
        [TestMethod]
        public void IsProposalCertificationLate2()
        {
            bool expected = false;
            DateTime? submittalDate = null;
            ProposalStatus status = ProposalStatus.Completed;

            bool actual = Helpers.IsProposalCertificationLate(status, submittalDate);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Helpers Test 3
        /// </summary>
        [TestMethod]
        public void IsProposalCertificationLate3()
        {
            bool expected = true;
            DateTime? submittalDate = null;
            ProposalStatus status = ProposalStatus.Submitted;

            bool actual = Helpers.IsProposalCertificationLate(status, submittalDate);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Helpers Test 4
        /// </summary>
        [TestMethod]
        public void IsProposalCertificationLate4()
        {
            bool expected = true;
            DateTime? submittalDate = DateTime.Now.AddDays(-61);
            ProposalStatus status = ProposalStatus.Submitted;

            bool actual = Helpers.IsProposalCertificationLate(status, submittalDate);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Helpers Test 5
        /// </summary>
        [TestMethod]
        public void IsProposalCertificationLate5()
        {
            bool expected = false;
            DateTime? submittalDate = DateTime.Now.AddDays(-60);
            ProposalStatus status = ProposalStatus.Submitted;

            bool actual = Helpers.IsProposalCertificationLate(status, submittalDate);

            Assert.AreEqual(expected, actual);
        }
    }
}