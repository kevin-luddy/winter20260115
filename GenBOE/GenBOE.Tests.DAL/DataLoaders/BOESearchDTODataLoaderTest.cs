// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;
    using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests Search Loader
    /// </summary>
    [TestClass]
    public class BOESearchDTODataLoaderTest
    {
        /// <summary>
        /// Cutoff date from the app.config
        /// </summary>
        static readonly DateTime cutOffDate = DateTime.Parse(ConfigurationUtilities.GetAppSetting("MoqTemplateStartDate"));

        /// <summary>
        /// Runs a number of tests for quick / basic search for new moq type options
        /// </summary>
        [TestMethod]
        public void TestSearchWithNewMoqTypes()
        {
            RunBasicSearchTest(false, true, SearchCategory.All);
            RunBasicSearchTest(false, true, SearchCategory.BOEsInThisWorkspace);
            RunBasicSearchTest(false, true, SearchCategory.BOEsInOtherWorkspaces);

            RunBasicSearchTest(true,  false, SearchCategory.All);
            // RunBasicSearchTest(true,  false, SearchCategory.BOEsInThisWorkspace); // no currently matching data exists, don't feel it matters enough to set it up.
            RunBasicSearchTest(true,  false, SearchCategory.BOEsInOtherWorkspaces);

            RunAdvancedSearchTest(false, true, SearchCategory.All);
            // RunAdvancedSearchTest(false, true, SearchCategory.BOEsInThisWorkspace); // no currently matching data exists, don't feel it matters enough to set it up.
            RunAdvancedSearchTest(false, true, SearchCategory.BOEsInOtherWorkspaces);

            RunAdvancedSearchTest(true, false, SearchCategory.All);
            // RunAdvancedSearchTest(true,  false, SearchCategory.BOEsInThisWorkspace); // no currently matching data exists, don't feel it matters enough to set it up.
            RunAdvancedSearchTest(true, false, SearchCategory.BOEsInOtherWorkspaces);

        }

        /// <summary>
        /// Executes a test & verifies it
        /// </summary>
        private void RunBasicSearchTest(bool usingBoeTemplates, bool testingOldWs, SearchCategory searchType)
        {
            BOESearchDTODataLoader sut = new BOESearchDTODataLoader();

            int wsId, boeId = -1;
            string searchString = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                Workspace ws = gbe.Workspaces.First(x => !string.IsNullOrEmpty(x.WorkspaceDescription) && x.TemplateBoe == usingBoeTemplates
                                                        && ((testingOldWs && x.WorkspaceCreationDate < cutOffDate) || (!testingOldWs && x.WorkspaceCreationDate >= cutOffDate))
                                                        && !x.ContainsOCI && x.AllowSearch && x.IsDeleted != true 
                                                        && x.BOEs.Any(z => !string.IsNullOrEmpty(z.BOEDescription)));

                wsId = ws.WorkspaceID;
                foreach (var (boe, x) in ws.BOEs.Where(z => !string.IsNullOrEmpty(z.BOEDescription)).SelectMany(boe => boe.BOEDescription.Split(' ').Select(x => (boe, x))))
                {                    
                    searchString = Regex.Replace(x, @"</?\w+>", string.Empty).Trim();
                    boeId = boe.BOEID;
                    break;
                };
            }

            int foundWsId = sut.GetQuickSearchResults(new BOESearchDTO { WorkspaceID = wsId, SelectedCategory = searchType, QuickSearchText = searchString, BOEID = boeId, SearchResultsThreshold = 10 }).First().WorkspaceID;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                Workspace ws = gbe.Workspaces.First(x => x.WorkspaceID == foundWsId);
                Assert.AreEqual(testingOldWs, (ws.WorkspaceCreationDate ?? DateTime.MinValue) < cutOffDate);
                if (!usingBoeTemplates)
                {
                    // Searching from a WS not using template boe can only search other WSs not using template boe
                    Assert.IsFalse(ws.TemplateBoe);
                }
            }
        }

        /// <summary>
        /// Executes a test & verifies it
        /// </summary>
        private void RunAdvancedSearchTest(bool usingBoeTemplates, bool testingOldWs, SearchCategory searchType)
        {
            BOESearchDTODataLoader sut = new BOESearchDTODataLoader();

            int wsId, boeId = -1;
            string searchString = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                Workspace ws = gbe.Workspaces.First(x => !string.IsNullOrEmpty(x.WorkspaceDescription) && x.TemplateBoe == usingBoeTemplates
                                                        && ((testingOldWs && (x.WorkspaceCreationDate < cutOffDate || !x.WorkspaceCreationDate.HasValue)) || (!testingOldWs && x.WorkspaceCreationDate >= cutOffDate))
                                                        && !x.ContainsOCI && x.AllowSearch && x.IsDeleted != true
                                                        && x.BOEs.Any(z => !string.IsNullOrEmpty(z.BOEDescription)));

                wsId = ws.WorkspaceID;
                foreach (var (boe, x) in ws.BOEs.Where(z => !string.IsNullOrEmpty(z.BOEDescription)).SelectMany(boe => boe.BOEDescription.Split(' ').Select(x => (boe, x))))
                {
                    searchString = Regex.Replace(x, @"</?\w+>", string.Empty).Trim();
                    boeId = boe.BOEID;
                    break;
                };
            }

            int foundWsId = sut.GetAdvancedSearchResults(new BOESearchDTO { WorkspaceID = wsId, SelectedCategory = searchType, BoeDescription = searchString, BOEID = boeId, SearchResultsThreshold = 10 }).First().WorkspaceID;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                Workspace ws = gbe.Workspaces.First(x => x.WorkspaceID == foundWsId);                
                Assert.AreEqual(testingOldWs, (ws.WorkspaceCreationDate ?? DateTime.MinValue) < cutOffDate);
                if (!usingBoeTemplates)
                {
                    // Searching from a WS not using template boe can only search other WSs not using template boe
                    Assert.IsFalse(ws.TemplateBoe);
                }
            }
        }
    }
}
