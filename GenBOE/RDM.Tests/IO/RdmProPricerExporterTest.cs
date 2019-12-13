// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Tests.IO
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.ActionLogic.IO.Export;
    using IES.Common;
    using IES.DataBridge.ModelViews;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test helper methods of the RDM Pro Pricer Export
    /// </summary>
    [TestClass]
    public class RdmProPricerExporterTest
    {
        /// <summary>
        /// Create Sut
        /// </summary>
        /// <returns>sut</returns>
        private RdmProPricerExporter CreateSut()
        {
            // create SUT
            string zipFilePath = "MyFilePath";
            RevisionModelView revision = new RevisionModelView()
            {
                Id = 8888,
                Revision = "8888",
                History = "Test Revision",
                CreatedBy = "ffelicio",
                StartYear = 2017,
                EndYear = 2040
            };

            ICollection<BurdenElementModelView> burdenElements = new List<BurdenElementModelView>();
            BurdenElementModelView burdenElement1 = new BurdenElementModelView
            {
                Id = 11,
                Description = "OH Development",
                Name = "OH Dev",
                DisplayOrder = 1
            };
            BurdenElementModelView burdenElement2 = new BurdenElementModelView
            {
                Id = 22,
                Description = "OH Production",
                Name = "OH Prod",
                DisplayOrder = 2
            };

            BurdenElementModelView burdenElement3 = new BurdenElementModelView
            {
                Id = 33,
                Description = "Services G&A",
                Name = "G&A T2",
                DisplayOrder = 3
            };

            BurdenElementModelView burdenElement4 = new BurdenElementModelView
            {
                Id = 44,
                Description = "FCCM PH4",
                Name = "FCCM PH4",
                DisplayOrder = 4
            };

            burdenElements.Add(burdenElement1);
            burdenElements.Add(burdenElement2);
            burdenElements.Add(burdenElement3);
            burdenElements.Add(burdenElement4);

            ICollection<BurdenPoolDetailModelView> burdenPools = new Collection<BurdenPoolDetailModelView>();
            BurdenPoolDetailModelView burdenPool1 = new BurdenPoolDetailModelView
            {
                Id = 1111,
                RevisionID = revision.Id,
                BurdenPool = "BurdenPool1",
                Description = "Test1",
                BurdenElementRateCodeArray = new string[] { string.Empty, "538003LB", string.Empty, string.Empty },
                BurdenElementRateCodeMappings = new Collection<BurdenElementIdToRateCodeModelView>()
                {
                    new BurdenElementIdToRateCodeModelView
                    {
                        BurdenElementId = burdenElement2.Id,
                        RateCode = "538003LB"
                    }
                },
                IsGaT2ApplicableForMissionSolutions = false,
                IncludeGaT2InBurdAndCommBurdTables = true,
                IsCommercial = true
            };
            BurdenPoolDetailModelView burdenPool2 = new BurdenPoolDetailModelView
            {
                Id = 2222,
                RevisionID = revision.Id,
                BurdenPool = "BurdenPool2",
                Description = "Test2",
                BurdenElementRateCodeArray = new string[] { "XXDDAA", string.Empty, "NLBESSCH", string.Empty },
                BurdenElementRateCodeMappings = new Collection<BurdenElementIdToRateCodeModelView>()
                {
                    new BurdenElementIdToRateCodeModelView
                    {
                        BurdenElementId = burdenElement1.Id,
                        RateCode = "XXDDAA"
                    },
                    new BurdenElementIdToRateCodeModelView
                    {
                        BurdenElementId = burdenElement3.Id,
                        RateCode = "NLBESSCH"
                    }
                },
                IsGaT2ApplicableForMissionSolutions = false,
                IncludeGaT2InBurdAndCommBurdTables = true,
                IsCommercial = false
            };
            BurdenPoolDetailModelView burdenPool3 = new BurdenPoolDetailModelView
            {
                Id = 3333,
                RevisionID = revision.Id,
                BurdenPool = "BurdenPool3",
                Description = "Test3",
                BurdenElementRateCodeArray = new string[] { "XXDDAA", string.Empty, "XXZPPA", string.Empty },
                BurdenElementRateCodeMappings = new Collection<BurdenElementIdToRateCodeModelView>()
                {
                    new BurdenElementIdToRateCodeModelView
                    {
                        BurdenElementId = burdenElement1.Id,
                        RateCode = "XXDDAA"
                    },
                    new BurdenElementIdToRateCodeModelView
                    {
                        BurdenElementId = burdenElement3.Id,
                        RateCode = "XXZPPA"
                    }
                },
                IsGaT2ApplicableForMissionSolutions = true,
                IncludeGaT2InBurdAndCommBurdTables = false,
                IsCommercial = true
            };

            burdenPools.Add(burdenPool1);
            burdenPools.Add(burdenPool2);
            burdenPools.Add(burdenPool3);

            ICollection<RateDetailModelView> rates = new Collection<RateDetailModelView>();
            decimal rate = 1.11m;
            RateDetailModelView rate1 = new RateDetailModelView()
            {
                RevisionId = revision.Id,
                RateCode = "538003LB",
                RateCategory = RateCategory.DirectLabor,
                RateType = RateType.Cost,
                ResourceType = DirectRateMappingResourceType.Labor,
                Description = "1st rate code",
                RateDescription = "Civil Space Planetary Explor Sys - Lbr",
                ResourceClassId = 1,
                ResourceClass = "Actuals-Core",
                Section = 111,
                ProPricerMappings = new Collection<ProPricerRateCodeXrefModelView>() { new ProPricerRateCodeXrefModelView() { Description = "Civil Space Planetary Explor Sys - Lbr", RateCodeExtensionId = null } },
                CommercialBurdenPool = burdenPool1.BurdenPool,
                CommercialBurdenPoolId = burdenPool1.Id,
                GovernmentBurdenPool = burdenPool2.BurdenPool,
                GovernmentBurdenPoolId = burdenPool2.Id,
                Values = new Collection<RateYearModelView>() { new RateYearModelView() { Year = 2012, Value = rate }, new RateYearModelView() { Year = 2013, Value = rate + 0.01m }, new RateYearModelView() { Year = 2014, Value = rate + 0.02m } },
                Updateable = UpdateType.Upsert
            };

            rate = 2.22m;
            RateDetailModelView rate2 = new RateDetailModelView()
            {
                RevisionId = revision.Id,
                RateCode = "XXDDAA",
                RateCategory = RateCategory.DirectLabor,
                RateType = RateType.Cost,
                ResourceType = DirectRateMappingResourceType.Labor,
                Description = "2nd rate code",
                RateDescription1 = "ATLO Denver Development Hourly & NES Straight Time Rate",
                RateDescription2 = "ENG Denver Development Hourly & NES Straight Time Rate",
                RateDescription7 = "BUS COE Denver Development Hourly & NES Straight Time Rate",
                ResourceClassId2 = 1,
                ResourceClass2 = "Actuals-Core",
                ResourceClassId7 = 1,
                ResourceClass7 = "Actuals-Core",
                Section = 222,
                ProPricerMappings = new Collection<ProPricerRateCodeXrefModelView>() { new ProPricerRateCodeXrefModelView() { Description = "ATLO Denver Development Hourly & NES Straight Time Rate", RateCodeExtensionId = 1 }, new ProPricerRateCodeXrefModelView() { Description = "ENG Denver Development Hourly & NES Straight Time Rate", RateCodeExtensionId = 2 } },
                CommercialBurdenPool = burdenPool3.BurdenPool,
                CommercialBurdenPoolId = burdenPool3.Id,
                GovernmentBurdenPool = burdenPool2.BurdenPool,
                GovernmentBurdenPoolId = burdenPool2.Id,
                Values = new Collection<RateYearModelView>() { new RateYearModelView() { Year = 2012, Value = rate }, new RateYearModelView() { Year = 2013, Value = rate + 0.01m }, new RateYearModelView() { Year = 2014, Value = rate + 0.02m } },
                Updateable = UpdateType.Upsert
            };

            rate = 3.33m;
            RateDetailModelView rate3 = new RateDetailModelView()
            {
                RevisionId = revision.Id,
                RateCode = "NLBESSCH",
                RateCategory = RateCategory.DirectLabor,
                RateType = RateType.Cost,
                ResourceType = DirectRateMappingResourceType.Labor,
                Description = "3rd rate code",
                Section = 333,
                ProPricerMappings = new Collection<ProPricerRateCodeXrefModelView>(),
                CommercialBurdenPool = string.Empty,
                CommercialBurdenPoolId = null,
                GovernmentBurdenPool = string.Empty,
                GovernmentBurdenPoolId = null,
                Values = new Collection<RateYearModelView>() { new RateYearModelView() { Year = 2012, Value = rate }, new RateYearModelView() { Year = 2013, Value = rate + 0.01m }, new RateYearModelView() { Year = 2015, Value = rate + 0.02m } },
                Updateable = UpdateType.Upsert
            };

            rate = 4.44m;
            RateDetailModelView rate4 = new RateDetailModelView()
            {
                RevisionId = revision.Id,
                RateCode = "XXZPPA",
                RateCategory = RateCategory.DirectLabor,
                RateType = RateType.Cost,
                ResourceType = DirectRateMappingResourceType.Labor,
                Description = "4th rate code",
                RateDescription3 = "LABS Support Production HR & SNES OT",
                RateDescription4 = "OTHER Support Production HR & SNES OT",
                Section = 444,
                CommercialBurdenPool = burdenPool1.BurdenPool,
                CommercialBurdenPoolId = burdenPool1.Id,
                GovernmentBurdenPool = burdenPool2.BurdenPool,
                GovernmentBurdenPoolId = burdenPool2.Id,
                ProPricerMappings = new Collection<ProPricerRateCodeXrefModelView>() { new ProPricerRateCodeXrefModelView() { Description = "LABS Support Production HR & SNES OT", RateCodeExtensionId = 3 }, new ProPricerRateCodeXrefModelView() { Description = "OTHER Support Production HR & SNES OT", RateCodeExtensionId = 4 } },
                Values = new Collection<RateYearModelView>() { new RateYearModelView() { Year = 2012, Value = rate }, new RateYearModelView() { Year = 2013, Value = rate + 0.01m }, new RateYearModelView() { Year = 2014, Value = 0.0m } },
                Updateable = UpdateType.Upsert
            };

            rates.Add(rate1);
            rates.Add(rate2);
            rates.Add(rate3);
            rates.Add(rate4);

            RdmProPricerExporter sut = new RdmProPricerExporter(rates, zipFilePath, burdenPools, burdenElements);
            sut.FindMinMaxYears();            
            return sut;
        }

        /// <summary>
        /// Test GetDirectRateRows
        /// </summary>
        [TestMethod]
        public void TestGetDirectRateRows()
        {
            RdmProPricerExporter sut = this.CreateSut();
            ICollection<IProPricerExportModelView> commercialDirectRateRows = sut.GetDirectRateRows(IsGovOrComm.Commerical);
            ICollection<IProPricerExportModelView> governmentDirectRateRows = sut.GetDirectRateRows(IsGovOrComm.Government);

            // Assert results
            Assert.AreEqual(2012, sut.MinYear);
            Assert.AreEqual(2015, sut.MaxYear);
            Assert.AreEqual(19, commercialDirectRateRows.Count);    // 1 header row + 18 data rows
            Assert.AreEqual(19, governmentDirectRateRows.Count);    // 1 header row + 18 data rows

            // validate commercial rows
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Resource Type", "Start Date", "Resource", "Description", "ResourceClass", "Burden Pool", "Rate Type", "Base Rate"), "Missing Direct Rate header row");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2012", "538003LB", "Civil Space Planetary Explor Sys - Lbr", "Actuals-Core", "BurdenPool1", "Cost", "1.11"), "Missing Direct Rate1");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2013", "538003LB", "Civil Space Planetary Explor Sys - Lbr", "Actuals-Core", "BurdenPool1", "Cost", "1.12"), "Missing Direct Rate2");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2014", "538003LB", "Civil Space Planetary Explor Sys - Lbr", "Actuals-Core", "BurdenPool1", "Cost", "1.13"), "Missing Direct Rate3");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2012", "XXDDAA1", "ATLO Denver Development Hourly & NES Straight Time Rate", string.Empty, "BurdenPool3", "Cost", "2.22"), "Missing Direct Rate4");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2013", "XXDDAA1", "ATLO Denver Development Hourly & NES Straight Time Rate", string.Empty, "BurdenPool3", "Cost", "2.23"), "Missing Direct Rate5");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2014", "XXDDAA1", "ATLO Denver Development Hourly & NES Straight Time Rate", string.Empty, "BurdenPool3", "Cost", "2.24"), "Missing Direct Rate6");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2012", "XXDDAA2", "ENG Denver Development Hourly & NES Straight Time Rate", "Actuals-Core", "BurdenPool3", "Cost", "2.22"), "Missing Direct Rate7");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2013", "XXDDAA2", "ENG Denver Development Hourly & NES Straight Time Rate", "Actuals-Core", "BurdenPool3", "Cost", "2.23"), "Missing Direct Rate8");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2014", "XXDDAA2", "ENG Denver Development Hourly & NES Straight Time Rate", "Actuals-Core", "BurdenPool3", "Cost", "2.24"), "Missing Direct Rate9");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2012", "XXZPPA3", "LABS Support Production HR & SNES OT", string.Empty, "BurdenPool1", "Cost", "4.44"), "Missing Direct Rate10");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2013", "XXZPPA3", "LABS Support Production HR & SNES OT", string.Empty, "BurdenPool1", "Cost", "4.45"), "Missing Direct Rate11");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2014", "XXZPPA3", "LABS Support Production HR & SNES OT", string.Empty, "BurdenPool1", "Cost", "0.0"), "Missing Direct Rate12");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2012", "XXZPPA4", "OTHER Support Production HR & SNES OT", string.Empty, "BurdenPool1", "Cost", "4.44"), "Missing Direct Rate13");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2013", "XXZPPA4", "OTHER Support Production HR & SNES OT", string.Empty, "BurdenPool1", "Cost", "4.45"), "Missing Direct Rate14");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2014", "XXZPPA4", "OTHER Support Production HR & SNES OT", string.Empty, "BurdenPool1", "Cost", "0.0"), "Missing Direct Rate15");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2012", "XXDDAA7", "BUS COE Denver Development Hourly & NES Straight Time Rate", "Actuals-Core", "BurdenPool3", "Cost", "2.22"), "Missing Direct Rate16");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2013", "XXDDAA7", "BUS COE Denver Development Hourly & NES Straight Time Rate", "Actuals-Core", "BurdenPool3", "Cost", "2.23"), "Missing Direct Rate17");
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Labor", "\t01/\t2014", "XXDDAA7", "BUS COE Denver Development Hourly & NES Straight Time Rate", "Actuals-Core", "BurdenPool3", "Cost", "2.24"), "Missing Direct Rate18");

            // validate government rows
            Assert.IsTrue(FindMatchingDirectRateRow(commercialDirectRateRows, "Resource Type", "Start Date", "Resource", "Description", "ResourceClass", "Burden Pool", "Rate Type", "Base Rate"), "Missing Government Rate header row");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2012", "538003LB", "Civil Space Planetary Explor Sys - Lbr", "Actuals-Core", "BurdenPool2", "Cost", "1.11"), "Missing Government Rate1");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2013", "538003LB", "Civil Space Planetary Explor Sys - Lbr", "Actuals-Core", "BurdenPool2", "Cost", "1.12"), "Missing Government Rate2");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2014", "538003LB", "Civil Space Planetary Explor Sys - Lbr", "Actuals-Core", "BurdenPool2", "Cost", "1.13"), "Missing Government Rate3");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2012", "XXDDAA1", "ATLO Denver Development Hourly & NES Straight Time Rate", string.Empty, "BurdenPool2", "Cost", "2.22"), "Missing Government Rate4");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2013", "XXDDAA1", "ATLO Denver Development Hourly & NES Straight Time Rate", string.Empty, "BurdenPool2", "Cost", "2.23"), "Missing Government Rate5");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2014", "XXDDAA1", "ATLO Denver Development Hourly & NES Straight Time Rate", string.Empty, "BurdenPool2", "Cost", "2.24"), "Missing Government Rate6");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2012", "XXDDAA2", "ENG Denver Development Hourly & NES Straight Time Rate", "Actuals-Core", "BurdenPool2", "Cost", "2.22"), "Missing Government Rate7");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2013", "XXDDAA2", "ENG Denver Development Hourly & NES Straight Time Rate", "Actuals-Core", "BurdenPool2", "Cost", "2.23"), "Missing Government Rate8");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2014", "XXDDAA2", "ENG Denver Development Hourly & NES Straight Time Rate", "Actuals-Core", "BurdenPool2", "Cost", "2.24"), "Missing Government Rate9");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2012", "XXZPPA3", "LABS Support Production HR & SNES OT", string.Empty, "BurdenPool2", "Cost", "4.44"), "Missing Government Rate10");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2013", "XXZPPA3", "LABS Support Production HR & SNES OT", string.Empty, "BurdenPool2", "Cost", "4.45"), "Missing Government Rate11");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2014", "XXZPPA3", "LABS Support Production HR & SNES OT", string.Empty, "BurdenPool2", "Cost", "0.0"), "Missing Government Rate12");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2012", "XXZPPA4", "OTHER Support Production HR & SNES OT", string.Empty, "BurdenPool2", "Cost", "4.44"), "Missing Government Rate13");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2013", "XXZPPA4", "OTHER Support Production HR & SNES OT", string.Empty, "BurdenPool2", "Cost", "4.45"), "Missing Government Rate14");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2014", "XXZPPA4", "OTHER Support Production HR & SNES OT", string.Empty, "BurdenPool2", "Cost", "0.0"), "Missing Government Rate15");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2012", "XXDDAA7", "BUS COE Denver Development Hourly & NES Straight Time Rate", "Actuals-Core", "BurdenPool2", "Cost", "2.22"), "Missing Government Rate16");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2013", "XXDDAA7", "BUS COE Denver Development Hourly & NES Straight Time Rate", "Actuals-Core", "BurdenPool2", "Cost", "2.23"), "Missing Government Rate17");
            Assert.IsTrue(FindMatchingDirectRateRow(governmentDirectRateRows, "Labor", "\t01/\t2014", "XXDDAA7", "BUS COE Denver Development Hourly & NES Straight Time Rate", "Actuals-Core", "BurdenPool2", "Cost", "2.24"), "Missing Government Rate18");
        }

        /// <summary>
        /// Search the collection of Direct Rate rows and return true if matching row found; false otherwise.
        /// </summary>
        /// <param name="directRateRows">Collection of Direct Rate rows</param>
        /// <param name="resourceType">Resource Type to match</param>
        /// <param name="startDate">Start Date to match</param>
        /// <param name="resource">Resource to match</param>
        /// <param name="description">Description to match</param>
        /// <param name="resourceClass">Resource Class to match</param>
        /// <param name="burdenPool">Burden Pool to match</param>
        /// <param name="rateType">Rate Type to match</param>
        /// 
        /// <param name="baseRate">Base Rate to match</param>
        /// <returns>true if matching row found; false otherwise</returns>
        private bool FindMatchingDirectRateRow(ICollection<IProPricerExportModelView>directRateRows, 
            string resourceType, string startDate, string resource, string description, string resourceClass, string burdenPool, string rateType, string baseRate)
        {
            foreach(IProPricerExportModelView mv in directRateRows)
            {
                ProPricerDirectRateExportRowModelView row = (ProPricerDirectRateExportRowModelView)mv;
                if (row.ResourceType.Equals(resourceType) && row.StartDate.Equals(startDate) && row.Resource.Equals(resource) && row.Description.Equals(description) &&
                     row.ResourceClass.Equals(resourceClass) && row.BurdenPool.Equals(burdenPool) && row.RateType.Equals(rateType) && row.BaseRate.Equals(baseRate))
                {
                    return true;
                }
            }

            return false;   // not found
        }

        /// <summary>
        /// Test GetBurdenRateRows
        /// </summary>
        [TestMethod]
        public void TestGetBurdenRateRows()
        {
            RdmProPricerExporter sut = this.CreateSut();
            ICollection<IProPricerExportModelView> burdenRates = sut.GetBurdenRateRows(false, false);
            ICollection<IProPricerExportModelView> missionSolutionsBurdenRates = sut.GetBurdenRateRows(true, false);

            // Assert results
            Assert.AreEqual(2012, sut.MinYear);
            Assert.AreEqual(2015, sut.MaxYear);
            // What would be row 7 is not added for missionSolutionsBurdenRates because no rates will show for it
            Assert.AreEqual(11, burdenRates.Count);                 // 1 header row + 10 data rows
            Assert.AreEqual(10, missionSolutionsBurdenRates.Count); // 1 header row + 9 data rows

            // validate burden rate rows
            Assert.IsTrue(FindMatchingBurdenRateRow(burdenRates, "Burden Pool", "Description", "Effective Date", "Date", new string[] { "OH Dev", "OH Prod", "G&A T2", "FCCM PH4" }), "Missing burden rate header row");
            Assert.IsTrue(FindMatchingBurdenRateRow(burdenRates, "BurdenPool1", "Test1", string.Empty, "2012", new string[] { string.Empty, "1.11", string.Empty, string.Empty }), "Missing burden rate row1");
            Assert.IsTrue(FindMatchingBurdenRateRow(burdenRates, "BurdenPool1", "Test1", string.Empty, "2013", new string[] { string.Empty, "1.12", string.Empty, string.Empty }), "Missing burden rate row2");
            Assert.IsTrue(FindMatchingBurdenRateRow(burdenRates, "BurdenPool1", "Test1", string.Empty, "2014", new string[] { string.Empty, "1.13", string.Empty, string.Empty }), "Missing burden rate row3");
            Assert.IsTrue(FindMatchingBurdenRateRow(burdenRates, "BurdenPool2", "Test2", string.Empty, "2012", new string[] { "2.22", string.Empty, "3.33", string.Empty }), "Missing burden rate row4");
            Assert.IsTrue(FindMatchingBurdenRateRow(burdenRates, "BurdenPool2", "Test2", string.Empty, "2013", new string[] { "2.23", string.Empty, "3.34", string.Empty }), "Missing burden rate row5");
            Assert.IsTrue(FindMatchingBurdenRateRow(burdenRates, "BurdenPool2", "Test2", string.Empty, "2014", new string[] { "2.24", string.Empty, string.Empty, string.Empty }), "Missing burden rate row6");
            Assert.IsTrue(FindMatchingBurdenRateRow(burdenRates, "BurdenPool2", "Test2", string.Empty, "2015", new string[] { string.Empty, string.Empty, "3.35", string.Empty }), "Missing burden rate row7");
            Assert.IsTrue(FindMatchingBurdenRateRow(burdenRates, "BurdenPool3", "Test3", string.Empty, "2012", new string[] { "2.22", string.Empty, string.Empty, string.Empty }), "Missing burden rate row8");
            Assert.IsTrue(FindMatchingBurdenRateRow(burdenRates, "BurdenPool3", "Test3", string.Empty, "2013", new string[] { "2.23", string.Empty, string.Empty, string.Empty }), "Missing burden rate row9");
            Assert.IsTrue(FindMatchingBurdenRateRow(burdenRates, "BurdenPool3", "Test3", string.Empty, "2014", new string[] { "2.24", string.Empty, string.Empty, string.Empty }), "Missing burden rate row10");

            // validate mission solutions burden rate rows
            Assert.IsTrue(FindMatchingBurdenRateRow(missionSolutionsBurdenRates, "Burden Pool", "Description", "Effective Date", "Date", new string[] { "OH Dev", "OH Prod", "G&A T2", "FCCM PH4" }), "Missing mission solutions burden rate header row");
            Assert.IsTrue(FindMatchingBurdenRateRow(missionSolutionsBurdenRates, "BurdenPool1", "Test1", string.Empty, "2012", new string[] { string.Empty, "1.11", string.Empty, string.Empty }), "Missing mission solutions burden rate row1");
            Assert.IsTrue(FindMatchingBurdenRateRow(missionSolutionsBurdenRates, "BurdenPool1", "Test1", string.Empty, "2013", new string[] { string.Empty, "1.12", string.Empty, string.Empty }), "Missing mission solutions burden rate row2");
            Assert.IsTrue(FindMatchingBurdenRateRow(missionSolutionsBurdenRates, "BurdenPool1", "Test1", string.Empty, "2014", new string[] { string.Empty, "1.13", string.Empty, string.Empty }), "Missing mission solutions burden rate row3");
            Assert.IsTrue(FindMatchingBurdenRateRow(missionSolutionsBurdenRates, "BurdenPool2", "Test2", string.Empty, "2012", new string[] { "2.22", string.Empty, string.Empty, string.Empty }), "Missing mission solutions burden rate row4");
            Assert.IsTrue(FindMatchingBurdenRateRow(missionSolutionsBurdenRates, "BurdenPool2", "Test2", string.Empty, "2013", new string[] { "2.23", string.Empty, string.Empty, string.Empty }), "Missing mission solutions burden rate row5");
            Assert.IsTrue(FindMatchingBurdenRateRow(missionSolutionsBurdenRates, "BurdenPool2", "Test2", string.Empty, "2014", new string[] { "2.24", string.Empty, string.Empty, string.Empty }), "Missing mission solutions burden rate row6");
            Assert.IsTrue(FindMatchingBurdenRateRow(missionSolutionsBurdenRates, "BurdenPool3", "Test3", string.Empty, "2012", new string[] { "2.22", string.Empty, "4.44", string.Empty }), "Missing mission solutions burden rate row8");
            Assert.IsTrue(FindMatchingBurdenRateRow(missionSolutionsBurdenRates, "BurdenPool3", "Test3", string.Empty, "2013", new string[] { "2.23", string.Empty, "4.45", string.Empty }), "Missing mission solutions burden rate row9");
            Assert.IsTrue(FindMatchingBurdenRateRow(missionSolutionsBurdenRates, "BurdenPool3", "Test3", string.Empty, "2014", new string[] { "2.24", string.Empty, "0.0", string.Empty }), "Missing mission solutions burden rate row10");
        }

        /// <summary>
        /// Search the collection of Burden Rate rows and return true if matching row found; false otherwise.
        /// </summary>
        /// <param name="burdenRateRows">Collection of Burden Rate rows</param>
        /// <param name="burdenPool">Burden Pool to match</param>
        /// <param name="description">Description to match</param>
        /// <param name="effectiveDate">Effective Date to match</param>
        /// <param name="date">date to match</param>
        /// <param name="rates">array of rates to match</param>
        /// <returns>true if match found; false otherwise</returns>
        private bool FindMatchingBurdenRateRow(ICollection<IProPricerExportModelView> burdenRateRows,
            string burdenPool, string description, string effectiveDate, string date, string[] rates)
        {
            foreach (IProPricerExportModelView mv in burdenRateRows)
            {
                ProPricerBurdenRateExportRowModelView row = (ProPricerBurdenRateExportRowModelView)mv;
                if (row.BurdenPool.Equals(burdenPool) && row.Description.Equals(description) && 
                    row.EffectiveDate.Equals(effectiveDate) && row.Date.Equals(date) && row.Rates.ToArray().SequenceEqual(rates))
                {
                    return true;
                }
            }

            return false;   // not found
        }
    }
}