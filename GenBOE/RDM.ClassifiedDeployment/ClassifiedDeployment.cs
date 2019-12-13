// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace ClassifiedDeployment
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Principal;
    using System.Threading;
    using System.Transactions;
    using IES.ActionLogic.Common;
    using IES.ActionLogic.Mediator;
    using IES.Common;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Newtonsoft.Json;

    /// <summary>
    /// This is a simple console application that will run periodically on the classified server to perform the following:
    ///     Pick up JSON file from high-side drop-box
    ///     Search Folder(specified in App.config) and process files with the ".json" extension.
    ///     Support multiple files in case we're doing a "catch-up" operation.
    ///     Path/DB string (specified in App.config).
    ///     Generate and run DB commands to deploy the RDM Revision
    ///     Close connection
    ///     Archive the JSON file to a location(specified in App.config) so it won't run again
    ///     Report back success/failure e-mail messages(E-mailer configuration stored in App.config), when executed, any errors, etc.
    ///     Also write out success/failure via log4net or elmah(specified in App.config).
    /// </summary>
    public static class ClassifiedDeployment
    {
        private static Logger logger = new Logger(typeof(ClassifiedDeployment));
        private static IRevisionMediator revisionMediator = new RevisionMediator(new RevisionLoader(), new CacheDataLoader(new MemoryCache(), -1));
        private static ISectionLoader sectionLoader = new SectionLoader();
        private static IBurdenPoolLoader burdenPoolLoader = new BurdenPoolLoader();
        private static IRateCodeYearLoader rateCodeYearLoader = new RateCodeYearLoader();
        private static IProPricerRateCodeXrefLoader proPricerRateCodeXrefLoader = new ProPricerRateCodeXrefLoader();
        private static IRateDetailLoader rateDetailLoader = new RateDetailLoader(rateCodeYearLoader, proPricerRateCodeXrefLoader);
        private static ICobraDetailLoader cobraDetailLoader = new CobraDetailLoader();

        private static int LIMIT_OF_RETRIES = ConfigurationUtilities.GetAppSetting<int>("LimitOfRetries", 3);
        private static int SLEEP_BETWEEN_RETRIES = (int)Math.Round(1000 * ConfigurationUtilities.GetAppSetting<double>("TimeoutBetweenRetriesSeconds", 5.0), 0);

        /// <summary>
        /// The data fetching scheduler
        /// </summary>
        private static IDataFetchingScheduler dataFetchingScheduler = new DataFetchingScheduler();

        /// <summary>
        /// The emailer
        /// </summary>
        private static IIESEmailer emailer = new IESEmailer(dataFetchingScheduler);

        /// <summary>
        /// Security information
        /// </summary>
        private static SecurityInformation securityInformation = new SecurityInformation(new ActiveDirectoryUtilities(30), new MemoryCache());

        /// <summary>
        /// Entry Point into the application
        /// If called with no command line parameters, 
        ///     deploy any revisions found in the RDM search folder, and
        ///     call RDM to clear the revision cache.
        /// Else if called with any command line parameter,
        ///     call RDM to clear the revision cache.
        /// </summary>
        public static void Main(string[] args)
        {
            Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            UserData currentUser = securityInformation.ActiveUserData;
            string rdmClearRevisionCacheUrl = ConfigurationUtilities.GetAppSetting<string>("RdmClearRevisionCacheUrl", string.Empty);
            Console.WriteLine("RDM Clear Revision Cache URL=" + rdmClearRevisionCacheUrl);

            if (args != null)
            {
                if (args.Length == 0)
                {
                    string rdmSearchFolder = ConfigurationUtilities.GetAppSetting<string>("RdmSearchFolder", string.Empty);
                    string rdmArchiveFolder = ConfigurationUtilities.GetAppSetting<string>("RdmArchiveFolder", string.Empty);
                    bool disableAllEmails = ConfigurationUtilities.GetAppSetting<bool>("DisableAllEmails", false);
                    Console.WriteLine("RDM Search Folder=" + rdmSearchFolder);
                    Console.WriteLine("RDM Archive Folder=" + rdmArchiveFolder);
                    Console.WriteLine("Disable All Emails=" + disableAllEmails);

                    try
                    {
                        string[] files = Directory.GetFiles(rdmSearchFolder, "*.json", SearchOption.TopDirectoryOnly).OrderBy(f => f).ToArray();
                        if (files.Length > 0)
                        {
                            foreach (string jsonPath in files)
                            {
                                ProcessFileWithRetry(currentUser, rdmClearRevisionCacheUrl, rdmArchiveFolder, jsonPath);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No RDM revisions to import at this time.");
                            logger.Error("No RDM revisions to import at this time.");
                        }
                    }
                    catch (DirectoryNotFoundException dnfe)
                    {
                        string failureMessage = $"Directory ({rdmSearchFolder}) not found. Please verify App.config settings: RdmSearchFolder and RdmArchiveFolder are correct.";
                        Console.WriteLine(failureMessage);
                        logger.Error(dnfe, failureMessage);
                        emailer.SendClassifiedDeploymentFailedEmail(failureMessage, currentUser);
                    }
                    catch (SqlException sqlEx)
                    {
                        string failureMessage = $"A SQL exception occurred. {sqlEx.Message}.";
                        Console.WriteLine(failureMessage);
                        Console.WriteLine(sqlEx.StackTrace);
                        logger.Error(sqlEx, failureMessage);
                        emailer.SendClassifiedDeploymentFailedEmail(failureMessage, currentUser);
                    }
                    catch (Exception ex)
                    {
                        string failureMessage = $"An exception occurred. {ex.Message}.";
                        Console.WriteLine(failureMessage);
                        Console.WriteLine(ex.StackTrace);
                        logger.Error(ex, failureMessage);
                        emailer.SendClassifiedDeploymentFailedEmail(failureMessage, currentUser);
                    }
                }
                else
                {
                    // if there are any parameters on the command line, just clear the cache
                    ClearRevisionCache(rdmClearRevisionCacheUrl);
                }
            }
        }

        /// <summary>
        /// Processes a file w/ retry capability, to automate away some of the weirdness
        /// </summary>
        /// <param name="currentUser">Current User</param>
        /// <param name="rdmClearRevisionCacheUrl">URL</param>
        /// <param name="rdmArchiveFolder">Archive folder location</param>
        /// <param name="jsonPath">File path</param>
        /// <param name="retryNumber">Retry #, starting at 0</param>
        private static void ProcessFileWithRetry(UserData currentUser, string rdmClearRevisionCacheUrl, string rdmArchiveFolder, string jsonPath, int retryNumber = 0)
        {
            Console.WriteLine($"File processing attempt #: {retryNumber + 1}");
            try
            {
                RevisionModelView deployedRevision = ImportRevision(jsonPath, rdmArchiveFolder);
                ClearRevisionCache(rdmClearRevisionCacheUrl);
                emailer.SendClassifiedDeploymentSuccessEmail(deployedRevision, currentUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine("## Attempt failed ##");
                Console.WriteLine(ex.Message);
                logger.Error(ex, "File attempt failed");
                if (retryNumber < LIMIT_OF_RETRIES)
                {
                    Thread.Sleep(SLEEP_BETWEEN_RETRIES);
                    ProcessFileWithRetry(currentUser, rdmClearRevisionCacheUrl, rdmArchiveFolder, jsonPath, ++retryNumber);
                }
                else { throw; }
            }
        }

        /// <summary>
        /// Import RDM revision
        /// </summary>
        /// <param name="revisionImportExportModelView">Revision data to import</param>
        /// <param name="rdmArchiveFolder">Archive Folder for storing import file after import</param>
        /// <returns>Imported revision</returns>
        private static RevisionModelView ImportRevision(string jsonPath, string rdmArchiveFolder)
        {
            string json = File.ReadAllText(jsonPath);
            RevisionImportExportModelView revisionImportExportModelView = JsonConvert.DeserializeObject<RevisionImportExportModelView>(json);
            string status = $"Importing File: {jsonPath}.";
            Console.WriteLine(jsonPath);
            logger.Info(status);
            Console.WriteLine("    Generated By: " + revisionImportExportModelView.GeneratedBy);
            Console.WriteLine("    Generated Date: " + revisionImportExportModelView.GeneratedDate);
            Console.WriteLine("    Revision: " + revisionImportExportModelView.Revision.Revision);

            // check if the revision already exists and is published
            ICollection<RevisionModelView> revisions = revisionMediator.GetAll().OrderBy(r => r.Revision).ToList();
            if (revisions.Any(r => r.Revision.Equals(revisionImportExportModelView.Revision.Revision) && !r.IsWipRevision))
            {
                throw new InvalidDataException($"Revision {revisionImportExportModelView.Revision.Revision} already exists - file ignored.");
            }

            // check if a revision was skipped (imported revision should replace current WIP revision)
            int expectedRevisionNumber, importedRevisionNumber;
            RevisionModelView wipRevision = revisions.Single(r => r.IsWipRevision);   
            if (int.TryParse(wipRevision.Revision, out expectedRevisionNumber) && int.TryParse(revisionImportExportModelView.Revision.Revision, out importedRevisionNumber))
            {
                if (expectedRevisionNumber != importedRevisionNumber)
                {
                    throw new InvalidDataException($"Revisions must be imported in order - expected revision {expectedRevisionNumber}, but got revision {importedRevisionNumber} instead.");
                }
            }
            else
            {
                throw new InvalidDataException($"Unable to parse revision numbers");
            }

            // import the revision data
            int? revisionId;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 5, 0) }))
            {
                IDictionary<int, int> sectionIdMap = new Dictionary<int, int>();
                IDictionary<int, int> rateCodeIdMap = new Dictionary<int, int>();
                IDictionary<int, int> burdenPoolIdMap = new Dictionary<int, int>();

                // delete current WIP revision
                revisionMediator.Delete(wipRevision);

                // import the revision and associated data
                // Note: This logic is loosely based on the copyRevision stored procedure.
                revisionImportExportModelView.Revision.Id = -1;
                revisionImportExportModelView.Revision.Updateable = UpdateType.Upsert;
                revisionId = revisionMediator.Upsert(revisionImportExportModelView.Revision);
                if (revisionId.HasValue && revisionId.Value >= 0)
                {
                    revisionImportExportModelView.Revision.Id = revisionId.Value;

                    // import sections'
                    string message = "      [" + DateTime.Now.ToLongTimeString() + "] Sections";
                    Console.WriteLine(message);
                    logger.Info(message);
                    ImportSections(revisionId.Value, revisionImportExportModelView.Sections, sectionIdMap);

                    // import burden pools
                    message = "      [" + DateTime.Now.ToLongTimeString() +"] Burden Pools";
                    Console.WriteLine(message);
                    logger.Info(message);
                    ImportBurdenPools(revisionId.Value, revisionImportExportModelView.BurdenPoolGridModel, burdenPoolIdMap);

                    // import rates and ProPricer rate code extension mappings
                    message = "      [" + DateTime.Now.ToLongTimeString() +"] Rates";
                    Console.WriteLine(message);
                    logger.Info(message);
                    ImportRates(revisionId.Value, revisionImportExportModelView.Rates, sectionIdMap, burdenPoolIdMap, rateCodeIdMap);

                    // import COBRA mappings
                    message = "      [" + DateTime.Now.ToLongTimeString() +"] Cobra";
                    Console.WriteLine(message);
                    logger.Info(message);
                    ImportCobraDetails(revisionId.Value, revisionImportExportModelView.CobraDetails, rateCodeIdMap);
                    
                    // import ProPricer burden pool/burden element mappings
                    message = "      [" + DateTime.Now.ToLongTimeString() +"] PP Mapping";
                    Console.WriteLine(message);
                    logger.Info(message);
                    ImportProPricerMappings(revisionId.Value, revisionImportExportModelView.BurdenPoolGridModel, rateCodeIdMap);

                    // retrieve the imported revision
                    message = "      [" + DateTime.Now.ToLongTimeString() +"] Revisions";
                    Console.WriteLine(message);
                    logger.Info(message);
                    RevisionModelView importedRevision = revisionMediator.GetById(revisionId);

                    // publish the imported revision
                    message = "      [" + DateTime.Now.ToLongTimeString() +"] Publish";
                    Console.WriteLine(message);
                    logger.Info(message);
                    revisionMediator.Publish(importedRevision, revisionImportExportModelView.Revision.PublishedBy, revisionImportExportModelView.Revision.DatePublished);

                    // validate the imported revision
                    message = "      [" + DateTime.Now.ToLongTimeString() +"] Validate";
                    Console.WriteLine(message);
                    logger.Info(message);
                    ValidateImport(revisionImportExportModelView, importedRevision);

                    // Archive the file
                    FileInfo fileInfo = new FileInfo(jsonPath);
                    fileInfo.MoveTo(Path.Combine(rdmArchiveFolder, fileInfo.Name));
                }
                else
                {
                    Console.WriteLine("~~ Unable to import ~~");
                    throw new InvalidDataException($"Unable to import revision {revisionImportExportModelView.Revision}, insert failed.");
                }

                scope.Complete();
            }

            // return the deployed revision
            return revisionMediator.GetById(revisionId);
        }

        /// <summary>
        /// Recursively import the sections and their children.
        /// </summary>
        /// <param name="revisionId">Revision Id</param>
        /// <param name="sections">Set of sections to process</param>
        /// <param name="sectionIdMap">Map between old and new section Id values</param>
        private static void ImportSections(int revisionId, ICollection<SectionModelView> sections, IDictionary<int, int> sectionIdMap)
        {
            foreach (SectionModelView section in sections)
            {
                int oldId = section.Id;
                section.Id = -1;
                section.RevisionId = revisionId;
                section.Updateable = UpdateType.Upsert;
                if (section.ParentId.HasValue && section.ParentId.Value > 0)
                {
                    section.ParentId = sectionIdMap[section.ParentId.Value];    // Update parentId
                }

                int? newId = sectionLoader.Save(section);
                if (newId.HasValue)
                {
                    section.Id = newId.Value;
                    sectionIdMap[oldId] = newId.Value;
                }
                else
                {
                    throw new InvalidDataException("Error saving section data");
                }

                // recursively import child sections
                ImportSections(revisionId, section.ChildNodes, sectionIdMap);
            }
        }

        /// <summary>
        /// Import the burden pools.
        /// </summary>
        /// <param name="revisionId">Revision Id</param>
        /// <param name="burdenPoolGridModelView">Burden pool data to process</param>
        /// <param name="burdenPoolIdMap">Map between old and new burden pool Id values</param>
        private static void ImportBurdenPools(int revisionId, BurdenPoolGridModelView burdenPoolGridModelView, IDictionary<int, int> burdenPoolIdMap)
        {
            int newBurdenPoolId = -1;
            IDictionary<string, int> burdenPoolMap = new Dictionary<string, int>();
            // clone the burden pools (to preserve the imported burden pool data and ProPricer mappings)
            ICollection<BurdenPoolDetailModelView> burdenPools = burdenPoolGridModelView.BurdenPools.DeepClone();
            foreach (BurdenPoolDetailModelView burdenPool in burdenPools)
            {
                burdenPoolMap[burdenPool.BurdenPool] = burdenPool.Id;
                burdenPool.Id = newBurdenPoolId--;
                burdenPool.RevisionID = revisionId;

                // clear out ProPricer mappings (since we haven't imported the rate codes yet).  The ProPricer mappings will be updated later.
                burdenPool.BurdenElementRateCodeArray = new string[] { };
                burdenPool.BurdenElementRateCodeMappings = new Collection<BurdenElementIdToRateCodeModelView>();
                burdenPool.Dirty = true;
            }

            burdenPoolLoader.SaveBurdenPools(burdenPools, revisionId);

            // Populate burdenPoolIdMap with new burden pool Ids. 
            // Also, reset Id and UpdateDate fields in burdenPoolGridModelView.BurdenPools.
            RevisionModelView revision = revisionMediator.GetById(revisionId);
            BurdenPoolGridModelView importedBurdenPoolGridModelView = burdenPoolLoader.GetByRevision(revisionId);
            foreach (BurdenPoolDetailModelView importedBurdenPool in importedBurdenPoolGridModelView.BurdenPools)
            {
                burdenPoolIdMap[burdenPoolMap[importedBurdenPool.BurdenPool]] = importedBurdenPool.Id;
                BurdenPoolDetailModelView burdenPool = burdenPoolGridModelView.BurdenPools.Single(bp => bp.BurdenPool.Equals(importedBurdenPool.BurdenPool));
                burdenPool.Id = importedBurdenPool.Id;
                burdenPool.UpdateDate = importedBurdenPool.UpdateDate;
            }
        }

        /// <summary>
        /// Import the rate codes, associated rate code years/values, and ProPricer extension descriptions.
        /// </summary>
        /// <param name="revisionId">Revision Id</param>
        /// <param name="rates">Set of rates to process</param>
        /// <param name="sectionIdMap">Map between old and new section Id values</param>
        /// <param name="burdenPoolIdMap">Map between old and new burden pool Id values</param>
        /// <param name="rateCodeIdMap">Map between old and new rate code Id values</param>
        private static void ImportRates(int revisionId, ICollection<RateDetailModelView> rates, IDictionary<int, int> sectionIdMap, IDictionary<int, int> burdenPoolIdMap, IDictionary<int, int> rateCodeIdMap)
        {
            int newRateCodeId = -1;
            int newRateCodeYearId = -1;
            IDictionary<string, int> rateCodeMap = new Dictionary<string, int>();
            foreach (RateDetailModelView rateDetailModelView in rates)
            {
                rateCodeMap[rateDetailModelView.RateCode] = rateDetailModelView.Id;
                rateDetailModelView.Id = newRateCodeId--;
                rateDetailModelView.RevisionId = revisionId;
                rateDetailModelView.Dirty = true;

                // Update associated section Id (if any)
                if (rateDetailModelView.Section.HasValue && rateDetailModelView.Section.Value > 0)
                {
                    rateDetailModelView.Section = sectionIdMap[rateDetailModelView.Section.Value];    
                }

                // Update associated burden pool Id (if any)
                if (rateDetailModelView.CommercialBurdenPoolId.HasValue && rateDetailModelView.CommercialBurdenPoolId.Value > 0)
                {
                    rateDetailModelView.CommercialBurdenPoolId = burdenPoolIdMap[rateDetailModelView.CommercialBurdenPoolId.Value];
                }

                // Update associated burden pool Id (if any)
                if (rateDetailModelView.GovernmentBurdenPoolId.HasValue && rateDetailModelView.GovernmentBurdenPoolId.Value > 0)
                {
                    rateDetailModelView.GovernmentBurdenPoolId = burdenPoolIdMap[rateDetailModelView.GovernmentBurdenPoolId.Value];
                }
                
                foreach (RateYearModelView rateYearModelView in rateDetailModelView.Values)
                {
                    if (rateYearModelView.Value.HasValue)
                    {
                        rateYearModelView.Id = newRateCodeYearId--;
                        rateYearModelView.RateCodeId = rateDetailModelView.Id;
                        rateYearModelView.Dirty = true;
                    }
                }
            }

            rateDetailLoader.SaveDetails(rates);

            // Populate rateCodeIdMap with new rate code Ids
            RevisionModelView revision = revisionMediator.GetById(revisionId);
            ICollection<RateDetailModelView> importedRates = rateDetailLoader.GetRatesByRevision(revision);
            foreach (RateDetailModelView rateDetailModelView in importedRates)
            {
                rateCodeIdMap[rateCodeMap[rateDetailModelView.RateCode]] = rateDetailModelView.Id;
            }
        }

        /// <summary>
        /// Import the COBRA details.
        /// </summary>
        /// <param name="revisionId">Revision Id</param>
        /// <param name="cobraDetails">Set of COBRA details to process</param>
        /// <param name="rateCodeIdMap">Map between old and new rate code Id values</param>
        private static void ImportCobraDetails(int revisionId, ICollection<CobraDetailModelView> cobraDetails, IDictionary<int, int> rateCodeIdMap)
        {
            foreach (CobraDetailModelView cobraDetailModelView in cobraDetails)
            {
                cobraDetailModelView.RevisionId = revisionId;
                cobraDetailModelView.Id = rateCodeIdMap[cobraDetailModelView.Id];
                cobraDetailModelView.Dirty = true;
            }

            cobraDetailLoader.SaveDetails(cobraDetails);
        }

        /// <summary>
        /// Import the ProPricer mappings (burden element to rate code).
        /// </summary>
        /// <param name="revisionId">Revision Id</param>
        /// <param name="burdenPoolGridModelView">Burden pool data and ProPricer mappings to process</param>
        /// <param name="rateCodeIdMap">Map between old and new rate code Id values</param>
        private static void ImportProPricerMappings(int revisionId, BurdenPoolGridModelView burdenPoolGridModelView, IDictionary<int, int> rateCodeIdMap)
        {
            int mappingId = -1;
            foreach (BurdenPoolDetailModelView burdenPool in burdenPoolGridModelView.BurdenPools)
            {
                burdenPool.RevisionID = revisionId;
                burdenPool.Dirty = true;                
                foreach (BurdenElementIdToRateCodeModelView mapping in burdenPool.BurdenElementRateCodeMappings)
                {
                    mapping.Id = mappingId--;
                    mapping.RateCodeId = rateCodeIdMap[mapping.RateCodeId];
                    mapping.Updateable = UpdateType.Upsert;
                }               
            }

            burdenPoolLoader.SaveBurdenPools(burdenPoolGridModelView.BurdenPools, revisionId);
        }

        /// <summary>
        /// Validate the actual versus expected import revision data.
        /// </summary>
        /// <param name="revisionImportExportModelView">Revision data to import</param>
        /// <param name="importedRevision">Imported revision data</param>
        private static void ValidateImport(RevisionImportExportModelView revisionImportExportModelView, RevisionModelView importedRevision)
        {          
            ICollection<SectionModelView> importedSections = sectionLoader.RetrieveAllSections(importedRevision);
            ICollection<RateDetailModelView> importedRates = rateDetailLoader.GetRatesByRevision(importedRevision);
            BurdenPoolGridModelView importedBurdenPoolGridModelView = burdenPoolLoader.GetByRevision(importedRevision.Id);

            // verify correct number of sections were imported
            int expectedSections = CountSections(revisionImportExportModelView.Sections);
            int actualSections = CountSections(importedSections);
            AssertItemCounts("Sections", expectedSections, actualSections);

            // verify correct number of rate codes were imported
            int expectedRateCodes = revisionImportExportModelView.Rates?.Count ?? 0;
            int actualRateCodes = importedRates?.Count ?? 0;
            AssertItemCounts("Rate codes", expectedRateCodes, actualRateCodes);

            if (expectedRateCodes > 0)
            {
                // verify correct number of rate values were imported
                int expectedRateValues = revisionImportExportModelView.Rates.Sum(x => x.Values.Count(v => v.Value.HasValue));
                int actualRateValues = importedRates.Sum(x => x.Values.Count(v => v.Value.HasValue));
                AssertItemCounts("Rate values", expectedRateValues, actualRateValues);

                // verify correct number of ProPricer rate code extensions were imported
                int expectedProPricerXrefs = revisionImportExportModelView.Rates.Sum(x => x.ProPricerMappings.Count);
                int actualProPricerXrefs = importedRates.Sum(x => x.ProPricerMappings.Count);
                AssertItemCounts("ProPricer rate code extensions", expectedProPricerXrefs, actualProPricerXrefs);
            }

            // verify correct number of COBRA details were imported
            int expectedCobraDetails = revisionImportExportModelView.CobraDetails.Count(x => !string.IsNullOrWhiteSpace(x.RateSet) || !string.IsNullOrWhiteSpace(x.Code1Description));
            int actualCobraDetails = revisionImportExportModelView.CobraDetails.Count(x => !string.IsNullOrWhiteSpace(x.RateSet) || !string.IsNullOrWhiteSpace(x.Code1Description));
            AssertItemCounts("Cobra Mappings", expectedCobraDetails, actualCobraDetails);

            int expectedBurdenPools = revisionImportExportModelView.BurdenPoolGridModel.BurdenPools?.Count ?? 0;
            int actualBurdenPools = importedBurdenPoolGridModelView.BurdenPools?.Count ?? 0;
            AssertItemCounts("Burden Pools", expectedBurdenPools, actualBurdenPools);

            // verify correct number of burden pool to rate code mappings were imported
            if (expectedBurdenPools > 0)
            {
                int expectedMappings = revisionImportExportModelView.BurdenPoolGridModel.BurdenPools.Sum(x => x.BurdenElementRateCodeMappings.Count);
                int actualMappings = importedBurdenPoolGridModelView.BurdenPools.Sum(x => x.BurdenElementRateCodeMappings.Count);
                AssertItemCounts("ProPricer burden pool/burden element mappings", expectedMappings, actualMappings);
            }

            Console.WriteLine("Import was successful.");
            Console.WriteLine("");
        }

        /// <summary>
        /// Recursively count sections in section hierarchy.
        /// </summary>
        /// <param name="sections">collection of sections to count</param>
        /// <returns>count of sections and all their decendants</returns>
        private static int CountSections(ICollection<SectionModelView> sections)
        {
            int sum = sections.Count;
            if (sections.Any())
            {
                foreach (SectionModelView section in sections)
                {
                    sum += CountSections(section.ChildNodes);
                }
            }

            return sum;
        }

        /// <summary>
        /// Verifies actual versus expected item counts and throws an exception if not equal.
        /// </summary>
        /// <param name="itemName">Name of item being verified</param>
        /// <param name="expectedCount">expected count</param>
        /// <param name="actualCount">actual count</param>
        private static void AssertItemCounts(string itemName, int expectedCount, int actualCount)
        {
            if (expectedCount == actualCount)
            {
                string status = $"{actualCount} {itemName} successfully imported.";
                Console.WriteLine(status);
                logger.Info(status);
            } 
            else
            {
                throw new InvalidDataException($"{itemName} import failed.  Expected {expectedCount} {itemName}, but {actualCount} were actually imported.");
            }
        }

        /// <summary>
        /// Clear the RDM Revision Cache.
        /// </summary>
        /// <param name="rdmClearRevisionCacheUrl">RESTful API endpoint for clearing the RDM revision cache.</param>
        private static void ClearRevisionCache(string rdmClearRevisionCacheUrl)
        {
            try
            {
                HttpClient client = new HttpClient(new HttpClientHandler()
                {
                    UseDefaultCredentials = true
                });

                client.BaseAddress = new Uri(rdmClearRevisionCacheUrl);

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                // List data response.
                string urlParameters = "?t=" + DateTime.Now.Ticks;
                HttpResponseMessage response = client.GetAsync(urlParameters).Result;
                response.EnsureSuccessStatusCode();
                Console.WriteLine("RDM cache cleared");
                Console.WriteLine("Clear Revision Cache Status = " + response.ToString());
                logger.Info("RDM cache cleared");
            }
            catch (HttpRequestException e)
            {
                string exceptionMessage = "Exception detected - Unable to clear RDM cache. " + e.Message;
                Console.WriteLine("\n" + exceptionMessage);
                logger.Error(exceptionMessage);
            }
        }
    }
}
