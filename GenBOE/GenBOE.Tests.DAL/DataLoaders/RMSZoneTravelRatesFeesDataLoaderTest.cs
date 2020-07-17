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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    [TestClass]
    public class RMSZoneTravelRatesFeesDataLoaderTest : MOQLoaderObject
    {
        RMSZoneTravelRatesFeesDataLoader sut = null;

        [TestInitialize]
        public void init()
        {
            sut = new RMSZoneTravelRatesFeesDataLoader(new EscalationRatesDTOLoader(), new MSTTravelNonzoneFeesAndCostsDTODataLoader());
        }

        [TestMethod]
        public void TestFeesAndCosts()
        {
            //Test getAllFeesAndCosts()
            ICollection<MSTTravelNonzoneFeesAndCostsDTO> systemFees = new MSTTravelNonzoneFeesAndCostsDTODataLoader().getAllFeesAndCosts();
            ICollection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO> allFeesAndCosts = sut.getAllFeesAndCostsByWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);

            // Assert that the workspace starts with no fees and Costs
            Assert.IsFalse(allFeesAndCosts.Any());

            Assert.IsTrue(sut.AreCurrentFeesOutOfDate(GlobalTestCaseSetup.GlobalWorkspaceID));

            sut.CopySystemDefaultFees(GlobalTestCaseSetup.GlobalWorkspaceID);

            allFeesAndCosts = sut.getAllFeesAndCostsByWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);

            Assert.IsTrue(allFeesAndCosts.Any());

            Assert.IsFalse(sut.AreCurrentFeesOutOfDate(GlobalTestCaseSetup.GlobalWorkspaceID));

            // BOEJ-1708 - Verify the CopySystemDefaultFees() method performs a kill and fill as follows:  
            //      Insert an extra "fees and costs" record (that is not part of the system fees) into the workspace.
            //      Call CopySystemDefaultFees() and verify the inserted record has been removed.
            WorkspaceRMSTravelNonzoneFeesAndCostsDTO extraWorkspaceFee = new WorkspaceRMSTravelNonzoneFeesAndCostsDTO { Id = -1, MiscOther = 111.11m, ModeID = 1, TravelAgencyFee = 222.22m, WorkspaceId = GlobalTestCaseSetup.GlobalWorkspaceID, UpdateDate = DateTime.Now, Updateable = UpdateType.Upsert };
            sut.saveWorkspaceFeesAndCosts(extraWorkspaceFee);
            allFeesAndCosts = sut.getAllFeesAndCostsByWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);
            Assert.AreEqual(systemFees.Count + 1, allFeesAndCosts.Count); // verify the record was inserted
            sut.CopySystemDefaultFees(GlobalTestCaseSetup.GlobalWorkspaceID);
            allFeesAndCosts = sut.getAllFeesAndCostsByWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);
            Assert.IsTrue(allFeesAndCosts.Any());
            Assert.IsFalse(sut.AreCurrentFeesOutOfDate(GlobalTestCaseSetup.GlobalWorkspaceID));

            // Test all of the properties
            foreach (MSTTravelNonzoneFeesAndCostsDTO systemFee in systemFees)
            {
                WorkspaceRMSTravelNonzoneFeesAndCostsDTO workspaceFee = allFeesAndCosts.FirstOrDefault(f => f.ModeID == systemFee.ModeID);
                Assert.IsNotNull(workspaceFee);
                Assert.AreEqual(systemFee.MiscOther, workspaceFee.MiscOther);
                Assert.AreEqual(systemFee.ModeID, workspaceFee.ModeID);
                Assert.AreEqual(systemFee.TravelAgencyFee, workspaceFee.TravelAgencyFee);
                Assert.AreEqual(systemFee.UpdateDate, workspaceFee.UpdateDate);
            }
        }

        [TestMethod]
        public void TestEscalationRates()
        {
            //Test getAllFeesAndCosts()
            ICollection<EscalationRatesDTO> systemRates = new EscalationRatesDTOLoader().GetAll();
            ICollection<WorkspaceRMSEscalationRatesDTO> allWorkspaceRates = sut.getAllEscalationRatesByWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);

            // Assert that the workspace starts with no fees and Costs
            Assert.IsFalse(allWorkspaceRates.Any());

            Assert.IsTrue(sut.AreCurrentEscalationRatesOutOfDate(GlobalTestCaseSetup.GlobalWorkspaceID));

            sut.CopySystemDefaultRates(GlobalTestCaseSetup.GlobalWorkspaceID);

            allWorkspaceRates = sut.getAllEscalationRatesByWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);

            Assert.IsTrue(allWorkspaceRates.Any());

            Assert.IsFalse(sut.AreCurrentEscalationRatesOutOfDate(GlobalTestCaseSetup.GlobalWorkspaceID));

            // BOEJ-1708 - Verify the CopySystemDefaultRates() method performs a kill and fill as follows:  
            //      Insert an extra "rates" record (that is not part of the system rates) into the workspace.
            //      Call CopySystemDefaultRates() and verify the inserted record has been removed.
            WorkspaceRMSEscalationRatesDTO extraWorkspaceRates = new WorkspaceRMSEscalationRatesDTO {  Id = -1, AirfareRate = 0.11111m, MiscRate = 0.11112m, PerDiemRate = 0.11113m, Year=2111, WorkspaceId = GlobalTestCaseSetup.GlobalWorkspaceID, UpdateDate = DateTime.Now, Updateable = UpdateType.Upsert };
            sut.saveWorkspaceEscalationRate(extraWorkspaceRates);
            allWorkspaceRates = sut.getAllEscalationRatesByWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);
            Assert.AreEqual(systemRates.Count + 1, allWorkspaceRates.Count); // verify the record was inserted
            sut.CopySystemDefaultRates(GlobalTestCaseSetup.GlobalWorkspaceID);
            allWorkspaceRates = sut.getAllEscalationRatesByWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);
            Assert.IsTrue(allWorkspaceRates.Any());
            Assert.IsFalse(sut.AreCurrentEscalationRatesOutOfDate(GlobalTestCaseSetup.GlobalWorkspaceID));

            // Test all of the properties
            foreach (EscalationRatesDTO systemRate in systemRates)
            {
                WorkspaceRMSEscalationRatesDTO workspaceFee = allWorkspaceRates.FirstOrDefault(f => f.Year == systemRate.Year);
                Assert.IsNotNull(workspaceFee);
                Assert.AreEqual(systemRate.DevEscalation, workspaceFee.AirfareRate);
                Assert.AreEqual(systemRate.DevEscalation, workspaceFee.PerDiemRate);
                Assert.AreEqual(systemRate.MiscRate, workspaceFee.MiscRate);
                Assert.AreEqual(systemRate.Year, workspaceFee.Year);
                Assert.AreEqual(systemRate.UpdateDate, workspaceFee.UpdateDate);
            }

            sut.CopySystemDefaultRates(GlobalTestCaseSetup.GlobalWorkspaceID);
            allWorkspaceRates = sut.getAllEscalationRatesByWorkspace(GlobalTestCaseSetup.GlobalWorkspaceID);
            Assert.AreEqual(systemRates.Count, allWorkspaceRates.Count);
        }
    }
}
