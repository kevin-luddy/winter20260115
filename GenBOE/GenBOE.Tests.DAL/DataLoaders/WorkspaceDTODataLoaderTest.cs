// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class WorkspaceRetrieveDataLoaderTest : MOQLoaderObject
    {
        //Note: The only exception tested here is EntityCommandExecutionException
        // Discussed with Jim that having a test for the input null case or even a general exception
        // made little sense

        // Used to create a random workspace variable name with no ints
        public static Collection<string> _WorkspaceVarName { get; set; }

        // define a list of possible workspace variable name add ons. These do not
        // really  need to make logical sense. They just have to be strings with no spaces/no symbols
        static WorkspaceRetrieveDataLoaderTest()
        {
            #region Workspace Variables Array

            string[] WorkspaceVariables = { "ABC",
                                         "DEF",
                                          "GHI",
                                          "JKL",
                                          "MNO",
                                          "PQR",
                                          "STU",
                                          "VWX",
                                          "YZ",
                                          "RollingStones",
                                          "Weezer",
                                           "Gaga",
                                           "Apple",
                                           "Orange",
                                           "Chocolate",
                                           "Sugar",
                                           "Flour",
                                           "Eggs",
                                           "HotDog",
                                           "Blue",
                                           "Red",
                                           "Hot",
                                           "Cold",
                                           "TBD",
                                           "ETA",
                                           "ERA",
                                           "ELDP",
                                           "Tiger",
                                           "EPA",
                                           "Geico",
                                           "ByHour",
                                           "Percenty",
                                           "Approvals",
                                           "LinesOfCode",
                                           "Permission",
                                           "Sine",
                                           "Cosine",
                                           "Tan",
                                           "Value",
                                           "Speed",
                                           "Velocity",
                                           "Gravity",
                                           "Shocking",
                                           "Mean",
                                           "Variance",
                                           "Average",
                                           "Avg",
                                           "Regression",
                                           "Performance",
                                           "SharedBy",
                                           "Discrete",
                                           "Opionated",
                                           "LinesTested",
                                           "PercentIncrease",
                                           "ABACDEFGJK",
                                           "Balloon",
                                           "MarketBalance",
                                           "FavoriteWorkspaceVar",
                                           "LeastFavVar",
                                           "Estimation",
                                           "GuessFactor",
                                           "BlackBoxTesting",
                                           "Developers",
                                           "Testers",
                                           "Abstract",
                                           "Reviewed",
                                           "LM",
                                           "CIO",
                                           "ISGS",
                                           "BusinessPractice",
                                           "Normalization",
                                           "Location",
                                           "GameTheory",
                                           "BlackSwan",
                                           "Chance",
                                           "NumberofRolls",
                                           "Head",
                                           "Tail",
                                           "StdDev",
                                           "Adams",
                                           "Allegheny",
                                           "Armstrong",
                                           "Berks",
                                           "Blair",
                                           "Butler",
                                           "Montgomery",
                                           "Sullivan",
                                           "Union",
                                           "Warren",
                                           "York",
                                           "Juniata",
                                           "Greene",
                                           "Monroe",
                                           "Dauphin",
                                           "Delaware",
                                           "Somerset",
                                           "Pike",
                                           "Potter",
                                           "Ruiz",
                                           "Werth",
                                           "NumberOfPlayers",
                                           "Halladay",
                                           "Ibanez",
                                           "Rauuul",
                                           "RHoward",
                                           "McNabb",
                                           "Westbrook",
                                           "Carbon",
                                           "Radium",
                                           "Nitrogen",
                                           "Xenon",
                                           "Magnesium",
                                           "AtomicNumbers",
                                           "GroupNumbers",
                                           "Molecules",
                                           "Volume",
                                           "Moles",
                                           "KineticEnergy",
                                           "PPM",
                                           "Atmosphere",
                                           "blackboxtesting",
                                           "whiteboxtesting",
                                           "goonies",
                                           "goblins",
                                           "ghosts",
                                           "POC",
                                           "STaRS",
                                           "SEs",
                                           "Arches",
                                           "Yellow",
                                           "Phillies",
                                           "giants",
                                           "RedSox",
                                           "Flyers",
                                           "Orioles",
                                           "CitizenBank",
                                           "KOP",
                                           "Razzamatazz",
                                           "Teal",
                                           "NapTime",
                                           "thundercats",
                                           "rainbowbrite",
                                           "gijoe",
                                           "transformers",
                                           "gummibears",
                                           "spongebob",
                                           "YogiBear",
                                           "Jetsons",
                                           "FraggleRock",
                                           "MLK",
                                           "Presidents",
                                           "Joy",
                                           "TurkeyDay",
                                           "CBS",
                                           "ABC",
                                           "NBC",
                                           "FX"
                                          };

            #endregion Workspace Variables Array

            _WorkspaceVarName = new Collection<string>(WorkspaceVariables);
        }

        [TestMethod]
        public void L_LockAndRestoreTravelForWorkspace()
        {
            var _travelTripTaskElementCustomFieldValue = new Mock<ITravelTripTaskElementCustomFieldValueXREFLoader>();
            var _travelTripCustomFieldValue = new Mock<ITravelTripCustomFieldValueXREFLoader>();
            var travelDL = new TravelDTODataLoader(_travelTripTaskElementCustomFieldValue.Object, _travelTripCustomFieldValue.Object);
            int boeID = this.Boe1.Id;

            // save one travel so there is always at least one in the db
            TravelTripType travelTrip = new TravelTripType();
            travelTrip.TravelTripID = -1;
            travelTrip.SystemTripID = this.SystemTrip.TripID;
            travelTrip.BoeID = boeID;
            travelTrip.GroupID = 2;
            travelTrip.NumOfDays = 2;
            travelTrip.NumOfIntervals = 0;
            travelTrip.NumOfOccurences = 0;
            travelTrip.NumOfPeople = 3;
            travelTrip.NumOfTrips = 2;
            travelTrip.PerfOrgID = this.Perforg.Id;
            travelTrip.Segment = IES.Common.SegmentType.SSC;
            travelTrip.TripDate = DateTime.Now;
            travelTrip.Updateable = UpdateType.Upsert;
            travelTrip.UpdateDate = DateTime.Now;


            string guid = Guid.NewGuid().ToString();

            TravelDTO travel = new TravelDTO();
            travel.Id = -1;
            travel.Updateable = UpdateType.Upsert;
            travel.UpdateDate = DateTime.Now;
            travel.TaskID = "ETA";
            travel.TaskTitle = guid;
            travel.BoeID = boeID;
            travel.Description = "mock ws locking travel trip";
            travel.TravelTrips = new Collection<TravelTripType> { travelTrip };

            travelDL.SaveTravels(new Collection<TravelDTO> { travel });

            // should only have 1 trip based on the above
            travel = travelDL.GetByBoeIds(new Collection<int>() { boeID }).First();
            var tvTrips = travel.TravelTrips.ToList();
            Assert.AreEqual(1, tvTrips.Count());

            // we can now lock the trip
            var sut = new WorkspaceDTODataLoader();

            sut.LockTravelAndResourceRatesForWorkspace(Workspace.Id);

            // confirm the trip is locked, to do this we just make sure the WS locked trip is added
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                // get the locked trip .. which has a pointer back to the trip we cloned from
                var assertOnTravelTrips = from tvTrip in gbe.WorkspaceLockedTrips
                                          where tvTrip.TripID == SystemTrip.TripID && tvTrip.WorkspaceID == Workspace.Id
                                          select tvTrip;
                Assert.AreEqual(1, assertOnTravelTrips.Count(), "One travel trip not found");
                WorkspaceLockedTrip assertOnTravelTrip = assertOnTravelTrips.First();
                Assert.IsNotNull(assertOnTravelTrip.UpdateDT);
                Assert.AreEqual(DateTime.Now.DayOfYear, assertOnTravelTrip.UpdateDT.DayOfYear);
            }

            // restore the trip rates
            sut.RestoreTravelForWorkspace(Workspace.Id);

            // confirm the trip is restored, to do this we just make sure the WS locked trip is deleted
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                var assertOnTravelTrips = from tvTrip in gbe.WorkspaceLockedTrips
                                          where tvTrip.TripID == SystemTrip.TripID && tvTrip.WorkspaceID == Workspace.Id
                                          select tvTrip;

                Assert.AreEqual(0, assertOnTravelTrips.Count(), "Locked trip found, should have been deleted with restore.");

            }

            // now cleanup traveltrip and travel
            travel = travelDL.GetById(travel.Id);
            travel.Updateable = UpdateType.Deleted;
            travelDL.SaveTravels(new Collection<TravelDTO> { travel });
            travel = travelDL.GetById(travel.Id);
            Assert.IsNull(travel);

            this.ResetTestData();
        }

        [TestMethod]
        public void L_GetAllWorkspaces()
        {
            var sut = new WorkspaceDTODataLoader();

            ICollection<GenBOEHomepageWorkspaceRowModelView> results = sut.GetAllWsForHomepageGrid(this._userDL.GetOrCreateUserByNtid("paliderd").UserID);
            int totalWorkspacesFromDB;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                totalWorkspacesFromDB = (from w in gbe.Workspaces
                                         select w).Count();
            }

            Assert.AreEqual(totalWorkspacesFromDB, results.Count, "The number of workspaces returned did not match the number in the database.");
        }

        [TestMethod]
        public void L_Workspace_GetByIdsTest()
        {
            WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

            List<Workspace> workspaces;
            List<WorkspaceContractTypeXREF> contractTypes;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                // take 10 random workspaces
                workspaces = gbe.Workspaces.OrderBy(x => Guid.NewGuid()).Take(10).ToList();
                contractTypes = gbe.WorkspaceContractTypeXREFs.ToList();
            }

            ICollection<WorkspaceDTO> results = sut.GetByIds(workspaces.Select(x => x.WorkspaceID).ToCollection());

            Assert.AreEqual(workspaces.Count, results.Count);

            foreach (var result in results)
            {
                Workspace workspace = workspaces.First(x => x.WorkspaceID == result.Id);
                // 0
                // Updateable is not checked.. for obvious reasons..
                Assert.AreEqual(workspace.WorkspaceID, result.Id);
                Assert.AreEqual(workspace.WorkspaceName, result.WorkspaceName);
                Assert.AreEqual(workspace.WorkspaceShortName, result.Shortname);
                Assert.AreEqual(workspace.WorkspaceDescription, result.Description);
                // 5
                Assert.AreEqual(workspace.PerfOrgSorting, (int)result.PerfOrgSorting);
                Assert.AreEqual(workspace.LineOfBusinessID ?? -1, result.LineOfBusiness.Id);
                Assert.AreEqual(workspace.CostVolumeLeadPricerUserID, result.CostVolumeLeadPricerUserID);
                Assert.AreEqual(GenBOEUtilities.AdjustDateTimePrecision(workspace.ContractStartDate, DateTimePrecision.Month), result.ContractStartDate);
                Assert.AreEqual(GenBOEUtilities.AdjustDateTimePrecision(workspace.ContractEndDate, DateTimePrecision.Month), result.ContractEndDate);
                // 10
                Assert.AreEqual(workspace.ProposalSubmitDate, result.ProposalSubmittalDate);
                Assert.AreEqual(workspace.RFPNumber, result.RFPNumber);
                Assert.AreEqual(workspace.WorkspaceStateID, (int)result.WorkspaceState);
                Assert.AreEqual(workspace.ContainsOCI, result.ContainsOCI);
                Assert.AreEqual(workspace.TemplateID, result.TemplateID);
                // 15
                Assert.AreEqual(workspace.CreatedByETIUserID, result.CreatedByUserID);
                Assert.AreEqual(workspace.ResourceListID, result.ResourceListID);
                Assert.AreEqual(workspace.PerformingOrganizationListID, result.PerfOrgListID);
                Assert.AreEqual(workspace.PerformingOrganizationChangeFlag, result.PerfOrgsChanged);
                Assert.AreEqual(workspace.ContainsTemplate, result.ContainsTemplate);
                // 20
                Assert.AreEqual(workspace.TrackingNumber, result.TrackingNumber);
                Assert.AreEqual(workspace.NumProPricerExport, result.NumberOfTimesExportedToProPricer);
                Assert.AreEqual(workspace.UpdateDT, result.UpdateDate);
                Assert.AreEqual(workspace.ProposalStatusID, (int)result.ProposalStatus);
                Assert.AreEqual(workspace.StatusComment, result.StatusComment);
                // 25
                Assert.AreEqual(workspace.AllowGridEdit, result.AllowGridEdit);
                Assert.AreEqual(workspace.BOEExportSortByID, result.BOEExportSortByID);
                Assert.AreEqual(workspace.SegmentID ?? (int)SegmentType.None, (int)result.Segment);
                Assert.AreEqual(workspace.ProposalClassID ?? Constants.PROPOSAL_CLASS_TYPE_NOT_SET, (int)result.ProposalClass.Id);
                Assert.AreEqual(contractTypes.Where(x => x.WorkspaceID == result.Id).Count(), result.SelectedContractTypes.Count);
                // 30
                Assert.AreEqual(workspace.AllowSearch, result.AllowSearch);
                Assert.AreEqual(workspace.ProposalTitle, result.ProposalTitle);
                Assert.AreEqual(workspace.IsDeleted != null ? workspace.IsDeleted : false, result.HasBeenDeleted);
                Assert.AreEqual(workspace.DateDeleted, result.DateDeleted);
                Assert.AreEqual(workspace.ResourcePrecision, result.ResourceDecimalPrecision);
                // 35
                Assert.AreEqual(workspace.ResourcePrecision ?? 0, result.DecimalPrecision);
                Assert.AreEqual(workspace.RecalculationStartedDate ?? DateTime.Today, result.DateRecalculationStarted ?? DateTime.Today);
                Assert.AreEqual(workspace.CostPrecision, result.CostDecimalPrecision);
                Assert.AreEqual(workspace.CustomSorting, (int)result.CustomFieldSorting);
                Assert.AreEqual(workspace.ResourceSorting, (int)result.ResourceSorting);// 40
                // UpdateDateLong is not checked
                // Updateable is not checked
                Assert.AreEqual(workspace.IsUsingEquivalentPerson, result.IsUsingEquivalentPerson);
                Assert.AreEqual(workspace.IsUsingTM, result.IsUsingTM);
                Assert.AreEqual(workspace.ProjectMapTypeID, (int)result.ProjectMapType);
                // 45
                Assert.AreEqual(workspace.LastProPricerInstance, result.LastProPricerInstance);
                Assert.AreEqual(workspace.LastProPricerProposal, result.LastProPricerProposal);
                Assert.AreEqual(workspace.RteSizeLimit, result.RteSizeLimit);
                Assert.AreEqual(workspace.RevisedSubmittalDate, result.RevisedSubmittalDate);
                // 49
            }
            Type dtoType = typeof(WorkspaceDTO);
            int numProperties = dtoType.GetProperties().Count();
            Assert.AreEqual(49, numProperties, "Untested properties exist in the Workspace DTO");
        }

        [TestMethod]
        public void L_GetWorkspaceByIdTest()
        {
            var sut = new WorkspaceDTODataLoader();

            WorkspaceDTO toReturn = sut.GetById(Workspace.Id);
            Assert.IsNotNull(toReturn, "Did not locate workspace inserted by global test case setup");
        }

        /// <summary>
        /// This test case will verify soft delete of workspace is working correctly
        /// </summary>
        [TestMethod]
        public void SoftDeleteWorkspace()
        {
            var sut = new WorkspaceDTODataLoader();

            // Get the workspace data
            WorkspaceDTO workspaceModel = sut.GetById(Workspace.Id);
            sut.UpdateDeletedStatus(workspaceModel.Id, workspaceModel.UpdateDate, true, GlobalTestCaseSetup.GetEtiUserID());

            // get the workspace data to verify isdeleted flag is true
            workspaceModel = sut.GetById(Workspace.Id);

            Assert.IsTrue(workspaceModel.HasBeenDeleted);
            Assert.IsNotNull(workspaceModel.DateDeleted);

            this.ResetTestData();
        }

        /// <summary>
        /// This test case will verify update of favorite status
        /// </summary>
        [TestMethod]
        public void FavoriteWorkspace()
        {
            WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

            int userId = GlobalTestCaseSetup.GetEtiUserID();
            sut.UpdateFavorite(Workspace.Id, userId, true);

            bool? favorite;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                favorite = gbe.WorkspaceUserXREFs.FirstOrDefault(w => w.WorkspaceID == Workspace.Id)?.IsFavorite;
            }

            Assert.IsTrue(favorite ?? false);

            sut.UpdateFavorite(Workspace.Id, userId, false);

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                favorite = gbe.WorkspaceUserXREFs.FirstOrDefault(w => w.WorkspaceID == Workspace.Id)?.IsFavorite;
            }

            Assert.IsFalse(favorite ?? false);
        }

        /// <summary>
        /// This test case will verify last accessed for a workspace
        /// </summary>
        [TestMethod]
        public void LastAccessedWorkspace()
        {
            WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

            int userId = GlobalTestCaseSetup.GetEtiUserID();
            DateTime? lastAccessed;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                lastAccessed = gbe.WorkspaceUserXREFs.FirstOrDefault(w => w.WorkspaceID == Workspace.Id)?.LastAccessed;
            }

            sut.UpdateLastAccessed(Workspace.Id, userId);

            DateTime? lastAccessed2;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                lastAccessed2 = gbe.WorkspaceUserXREFs.FirstOrDefault(w => w.WorkspaceID == Workspace.Id)?.LastAccessed;
            }

            Assert.IsNotNull(lastAccessed2);

            if (lastAccessed.HasValue && lastAccessed.Value != lastAccessed2.Value)
            {
                Assert.IsTrue(lastAccessed2.Value > lastAccessed.Value);
            }

            sut.UpdateLastAccessed(Workspace.Id, userId);

            DateTime? lastAccessed3;
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                lastAccessed3 = gbe.WorkspaceUserXREFs.FirstOrDefault(w => w.WorkspaceID == Workspace.Id)?.LastAccessed;
            }

            Assert.IsNotNull(lastAccessed3);
            Assert.AreEqual(lastAccessed2, lastAccessed3);
        }

        /// <summary>
        /// This test case will verify soft delete of workspace is working correctly
        /// </summary>
        [TestMethod]
        public void SoftDeleteRestoreWorkspace()
        {
            var sut = new WorkspaceDTODataLoader();

            // Get the workspace data
            WorkspaceDTO workspaceModel = sut.GetById(Workspace.Id);
            sut.UpdateDeletedStatus(workspaceModel.Id, workspaceModel.UpdateDate, false, GlobalTestCaseSetup.GetEtiUserID());

            // get the workspace data to verify isdeleted flag is false
            workspaceModel = sut.GetById(Workspace.Id);

            Assert.IsFalse(workspaceModel.HasBeenDeleted);
            Assert.IsNull(workspaceModel.DateDeleted);

            this.ResetTestData();
        }

        /// <summary>
        /// This test case will verify SaveWorkspaceSettings is working correctly
        /// </summary>
        [TestMethod]
        public void L_SaveWorkspaceSettings()
        {
            var sut = new WorkspaceDTODataLoader();

            // Get the workspace data
            WorkspaceDTO workspaceModel = sut.GetById(Workspace.Id);

            // change the export to whatever is in global
            workspaceModel.TemplateID = GlobalTestCaseSetup.GlobalWorkspaceTemplateID;
            
            // add tracking number
            workspaceModel.TrackingNumber = "Mock Track";

            // Save the workspace settings
            sut.SaveWorkspaceSettings(Author.UserID, workspaceModel);


            // get the workspace data to verify export is now 1 instead of 2
            workspaceModel = sut.GetById(Workspace.Id);
            Assert.AreEqual(GlobalTestCaseSetup.GlobalWorkspaceTemplateID, workspaceModel.TemplateID, "The workspace model is not correct");
            Assert.AreEqual("Mock Track", workspaceModel.TrackingNumber, "The tracking number didn't match Mock Track");

            this.ResetTestData();
        }

        /// <summary>
        /// This test case will verify SaveWorkspaceSettings is working correctly
        /// </summary>
        [TestMethod]
        public void L_InsertDefaultMultiValues()
        {
            var sut = new WorkspaceDTODataLoader();

            // Get the workspace data
            WorkspaceDTO workspaceModel = sut.GetById(Workspace.Id);

            // change the export to whatever is in global
            workspaceModel.TemplateID = GlobalTestCaseSetup.GlobalWorkspaceTemplateID;
            List<CLIN> preWSClins = new List<CLIN>();
            List<CLIN> postWSClins = new List<CLIN>();
            List<WorkBreakdownStructure> preWSWBSs = new List<WorkBreakdownStructure>();
            List<WorkBreakdownStructure> postWSWBSs = new List<WorkBreakdownStructure>();
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                preWSClins.AddRange(gbe.CLINs.Where(x => x.WorkspaceID == workspaceModel.Id).ToList());
                preWSWBSs.AddRange(gbe.WorkBreakdownStructures.Where(x => x.WorkspaceID == workspaceModel.Id).ToList());

                gbe.insertDefaultMultiClinWBS(workspaceModel.Id);
                postWSClins.AddRange(gbe.CLINs.Where(x => x.WorkspaceID == workspaceModel.Id).ToList());
                postWSWBSs.AddRange(gbe.WorkBreakdownStructures.Where(x => x.WorkspaceID == workspaceModel.Id).ToList());

            }
            Assert.IsTrue(preWSClins.Count < postWSClins.Count);
            Assert.IsTrue(preWSWBSs.Count < postWSWBSs.Count);

            Assert.IsNull(preWSClins.Where(x => x.CLINNumber == Constants.UNIQUE_MULTI_NUMBER).FirstOrDefault());
            Assert.IsNull(preWSWBSs.Where(x => x.WBSNumber == Constants.UNIQUE_MULTI_NUMBER).FirstOrDefault());

            Assert.IsNotNull(postWSClins.Where(x => x.CLINNumber == Constants.UNIQUE_MULTI_NUMBER).FirstOrDefault());
            Assert.IsNotNull(postWSWBSs.Where(x => x.WBSNumber == Constants.UNIQUE_MULTI_NUMBER).FirstOrDefault());

            this.ResetTestData();
        }

        [TestMethod]
        public void SaveAllowSearch()
        {
            var sut = new WorkspaceDTODataLoader();

            // Get the workspace data
            WorkspaceDTO workspaceModel = sut.GetById(Workspace.Id);

            workspaceModel.AllowSearch = true;
            workspaceModel.ContainsTemplate = true;

            // Save the workspace settings
            sut.SaveWorkspaceSettings(Author.UserID, workspaceModel);

            // get the workspace data to verify export is now 1 instead of 2
            workspaceModel = sut.GetById(Workspace.Id);

            Assert.IsTrue(workspaceModel.AllowSearch == true, "The Allow Search value is not set to true");
            Assert.IsFalse(workspaceModel.AllowSearch == false, "The Allow Search value is set to null");
            Assert.IsTrue(workspaceModel.ContainsTemplate == true, "Contains template is not set to true");

            this.ResetTestData();
        }

        [TestMethod]
        public void SaveEmailOverride()
        {
            var sut = new WorkspaceDTODataLoader();

            ICollection<WorkspaceEmailOverrideDTO> overrides = sut.GetWorkspaceEmailOverrides(this.Workspace.Id);

            Assert.AreEqual(0, overrides.Count);

            overrides.Add(new WorkspaceEmailOverrideDTO
            {
                TurnOn = true,
                EmailType = EmailTypes.ApproverEmailBOEAwaitingApproval,
                Updateable = UpdateType.Upsert
            });


            sut.SaveWorkspaceEmailOverrides(overrides, this.Workspace.Id);
            ICollection<WorkspaceEmailOverrideDTO> overrides2 = sut.GetWorkspaceEmailOverrides(this.Workspace.Id);
            Assert.AreEqual(1, overrides2.Count);
            Assert.IsTrue(overrides2.First().TurnOn);
            Assert.AreEqual(EmailTypes.ApproverEmailBOEAwaitingApproval, overrides2.First().EmailType);

            overrides2.First().Updateable = UpdateType.Deleted;
            sut.SaveWorkspaceEmailOverrides(overrides2, this.Workspace.Id);

            overrides = sut.GetWorkspaceEmailOverrides(this.Workspace.Id);

            Assert.AreEqual(0, overrides.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveWorkspaceSettingsFails()
        {
            var sut = new WorkspaceDTODataLoader();

            // Save the workspace settings
            sut.SaveWorkspaceSettings(Author.UserID, null);
            Assert.Fail("Did not throw ArgumentNullException");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveWorkspaceSettingsFails2()
        {
            var sut = new WorkspaceDTODataLoader();

            // Save the workspace settings
            sut.SaveIdentificationAndExportFormat(Author.UserID, null);
            Assert.Fail("Did not throw ArgumentNullException");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveWorkspaceSettingsFails3()
        {
            var sut = new WorkspaceDTODataLoader();

            // Save the workspace settings
            sut.SaveAllowSearch(null);
            Assert.Fail("Did not throw ArgumentNullException");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag"), TestMethod]
        public void L_UpdatePerfOrgChangeFlag()
        {
            var sut = new WorkspaceDTODataLoader();

            // Get the workspace data
            WorkspaceDTO workspaceModel = sut.GetById(Workspace.Id);


            // Since the workspace has been restored with the default list, set the flag to false (not updated)
            sut.UpdatePerfOrgChangeFlag(workspaceModel, false);

            // get the workspace data to verify change flag is false
            workspaceModel = sut.GetById(Workspace.Id);

            Assert.IsTrue(workspaceModel.PerfOrgsChanged == false, "The Perf Org Changed did not save");
            this.ResetTestData();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "newAvg"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "originalAvg")]
        //[TestMethod]
        public void TestOldVsNew()
        {
            //WorkspaceDTODataLoader loader = new WorkspaceDTODataLoader();

            //Stopwatch sw = new Stopwatch();
            //List<WorkspaceDTO> originalData = new List<WorkspaceDTO>();
            //List<WorkspaceDTO> newData = new List<WorkspaceDTO>();
            //List<long> originalTimes = new List<long>();
            //List<long> newTimes = new List<long>();

            //List<int> ids = new List<int>();

            //{
            //    int maxItems = 30;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        ids = gbe.Workspaces.OrderBy(x => new Guid()).Select(x => x.WorkspaceID).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < 5; i++) { sw.Restart(); originalData.AddRange(loader.GetByIds_OLD(ids)); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < 5; i++) { sw.Restart(); newData.AddRange(loader.GetByIds(ids)); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}

            //{
            //    originalData = new List<WorkspaceDTO>(); newData = new List<WorkspaceDTO>(); originalTimes = new List<long>(); newTimes = new List<long>();
            //    List<string> shortNames;

            //    int maxItems = 300;
            //    using (GenBoeEntities gbe = new GenBoeEntities())
            //    {
            //        shortNames = gbe.Workspaces.OrderBy(x => new Guid()).Select(x => x.WorkspaceShortName).Take(maxItems).ToList();
            //    }

            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); originalData.Add(loader.GetByShortname_OLD(shortNames.ElementAt(i))); sw.Stop(); originalTimes.Add(sw.ElapsedMilliseconds); }
            //    for (int i = 0; i < maxItems; i++) { sw.Restart(); newData.Add(loader.GetByShortname(shortNames.ElementAt(i))); sw.Stop(); newTimes.Add(sw.ElapsedMilliseconds); }

            //    double originalAvg = originalTimes.Average();
            //    double newAvg = newTimes.Average();

            //    this.VerifyCollections(originalData.OrderBy(x => x.Id).ToList(), newData.OrderBy(x => x.Id).ToList());
            //}
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Leaving this here for future test support")]
        private void VerifyCollections(ICollection<WorkspaceDTO> collection1, ICollection<WorkspaceDTO> collection2)
        {
            Assert.AreEqual(collection1.Count, collection2.Count);
            for (int i = 0; i < collection1.Count; i++)
            {
                this.VerifyDtos(collection1.ElementAt(i), collection2.ElementAt(i));
            }
        }

        private void VerifyDtos(WorkspaceDTO dto1, WorkspaceDTO dto2)
        {
            Assert.AreEqual(dto1.AllowSearch, dto2.AllowSearch);
            Assert.AreEqual(dto1.BOEExportSortByID, dto2.BOEExportSortByID);
            Assert.AreEqual(dto1.ContainsOCI, dto2.ContainsOCI);
            Assert.AreEqual(dto1.ContainsTemplate, dto2.ContainsTemplate);
            Assert.AreEqual(dto1.ContractEndDate, dto2.ContractEndDate);
            Assert.AreEqual(dto1.ContractStartDate, dto2.ContractStartDate);
            Assert.AreEqual(dto1.CostDecimalPrecision, dto2.CostDecimalPrecision);
            Assert.AreEqual(dto1.CostVolumeLeadPricerUserID, dto2.CostVolumeLeadPricerUserID);
            Assert.AreEqual(dto1.CreatedByUserID, dto2.CreatedByUserID);
            Assert.AreEqual(dto1.DateDeleted, dto2.DateDeleted);
            Assert.AreEqual(dto1.DateRecalculationStarted, dto2.DateRecalculationStarted);
            Assert.AreEqual(dto1.DecimalPrecision, dto2.DecimalPrecision);
            Assert.AreEqual(dto1.Description, dto2.Description);
            Assert.AreEqual(dto1.HasBeenDeleted, dto2.HasBeenDeleted);
            Assert.AreEqual(dto1.Id, dto2.Id);
            Assert.AreEqual(dto1.NumberOfTimesExportedToProPricer, dto2.NumberOfTimesExportedToProPricer);
            Assert.AreEqual(dto1.PerfOrgListID, dto2.PerfOrgListID);
            Assert.AreEqual(dto1.PerfOrgsChanged, dto2.PerfOrgsChanged);
            Assert.AreEqual(dto1.ProposalClass.Id, dto2.ProposalClass.Id);
            Assert.AreEqual(dto1.ProposalStatus, dto2.ProposalStatus);
            Assert.AreEqual(dto1.ProposalSubmittalDate, dto2.ProposalSubmittalDate);
            Assert.AreEqual(dto1.ProposalTitle, dto2.ProposalTitle);
            Assert.AreEqual(dto1.ResourceDecimalPrecision, dto2.ResourceDecimalPrecision);
            Assert.AreEqual(dto1.ResourceListID, dto2.ResourceListID);
            Assert.AreEqual(dto1.RFPNumber, dto2.RFPNumber);
            Assert.AreEqual(dto1.Segment, dto2.Segment);
            Assert.AreEqual(dto1.Shortname, dto2.Shortname);
            Assert.AreEqual(dto1.StatusComment, dto2.StatusComment);
            Assert.AreEqual(dto1.TemplateID, dto2.TemplateID);
            Assert.AreEqual(dto1.TrackingNumber, dto2.TrackingNumber);
            Assert.AreEqual(dto1.Updateable, dto2.Updateable);
            Assert.AreEqual(dto1.UpdateDate, dto2.UpdateDate);
            Assert.AreEqual(dto1.WorkspaceName, dto2.WorkspaceName);
            Assert.AreEqual(dto1.WorkspaceState, dto2.WorkspaceState);

            Assert.AreEqual(dto1.SelectedContractTypes.Count, dto2.SelectedContractTypes.Count);
            for (int i = 0; i < dto1.SelectedContractTypes.Count; i++)
            {
                Assert.AreEqual(dto1.SelectedContractTypes.ElementAt(i), dto2.SelectedContractTypes.ElementAt(i));
            }

            if (dto1.LineOfBusiness == null) { Assert.IsNull(dto2.LineOfBusiness); }
            else
            {
                Assert.AreEqual(dto1.LineOfBusiness.Id, dto2.LineOfBusiness.Id);
                Assert.AreEqual(dto1.LineOfBusiness.Text, dto2.LineOfBusiness.Text);
            }
            Assert.AreEqual(dto1.IsUsingTM, dto2.IsUsingTM);
            Assert.AreEqual(dto1.ProjectMapType, dto2.ProjectMapType);
        }

        [TestMethod]
        public void TestGetRteFieldsExceedingLimit()
        {
            int wsId = 1;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                // Dusan set this one up. Hopefully nobody messed with it..
                wsId = gbe.Workspaces.First(x => x.RteSizeLimit == 111).WorkspaceID;
            }

            var sut = new WorkspaceDTODataLoader();

            var result = sut.GetRteFieldsExceedingLimit(wsId);

            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(1, result.Count(x => x.Field.Contains("Task Description")));
            Assert.AreEqual(1, result.Count(x => x.Field.Contains("Task MOQ Rationale")));
            Assert.AreEqual(1, result.Count(x => x.Field.Contains("BOE Sources Of Data")));
            Assert.AreEqual(1, result.Count(x => x.Field.Contains("BOE Description")));
        }
    }
}
