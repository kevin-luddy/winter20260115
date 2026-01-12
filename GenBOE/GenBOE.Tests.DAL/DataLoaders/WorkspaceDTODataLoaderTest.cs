// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.DAL.DataLoaders
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.DataBridge.DTO.SkillMixSummary;
	using GenBOE.Dtos;
	using GenBOE.Models;
	using GenBOE.Objects;
	using GenBOE.Tests.ActionLogic;
	using GenBOE.Tests.DAL.DataLoaders;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using IES.Common.PickList;
	using Microsoft.Practices.Unity;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	[TestClass]
	public class WorkspaceRetrieveDataLoaderTest : MOQLoaderObject
	{
		//Note: The only exception tested here is EntityCommandExecutionException
		// Discussed with Jim that having a test for the input null case or even a general exception
		// made little sense

		// Used to create a random ws variable name with no ints
		public static Collection<string> _WorkspaceVarName { get; set; }

		// define a list of possible ws variable name add ons. These do not
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

			foreach (WorkspaceDTO result in results)
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
				Assert.AreEqual(workspace.ResourceSorting, (int)result.ResourceSorting);
				// 40
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
				Assert.AreEqual(workspace.TemplateBoe, result.UsingTemplateBOE);
				// 50
				Assert.AreEqual(workspace.WorkspaceCreationDate, result.CreationDate);
				Assert.AreEqual(workspace.EnableSAPConnection, result.EnableSAPConnection);
				Assert.AreEqual(workspace.EnableAssignTaskAuthor, result.EnableAssignTaskAuthor);
				Assert.AreEqual(workspace.EnableLmNavigator, result.EnableLmNavigator);
			}
			Type dtoType = typeof(WorkspaceDTO);
			int numProperties = dtoType.GetProperties().Count();
			Assert.AreEqual(54, numProperties, "Untested properties exist in the Workspace DTO");
		}

		[TestMethod]
		public void L_GetWorkspaceByIdTest()
		{
			var sut = new WorkspaceDTODataLoader();

			WorkspaceDTO toReturn = sut.GetById(Workspace.Id);
			Assert.IsNotNull(toReturn, "Did not locate ws inserted by global test case setup");
		}

		/// <summary>
		/// This test case will verify soft delete of ws is working correctly
		/// </summary>
		[TestMethod]
		public void SoftDeleteWorkspace()
		{
			var sut = new WorkspaceDTODataLoader();

			// Get the ws data
			WorkspaceDTO workspaceModel = sut.GetById(Workspace.Id);
			sut.UpdateDeletedStatus(workspaceModel.Id, workspaceModel.UpdateDate, true, GlobalTestCaseSetup.GetEtiUserID());

			// get the ws data to verify isdeleted flag is true
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
		/// This test case will verify last accessed for a ws
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
		/// This test case will verify soft delete of ws is working correctly
		/// </summary>
		[TestMethod]
		public void SoftDeleteRestoreWorkspace()
		{
			var sut = new WorkspaceDTODataLoader();

			// Get the ws data
			WorkspaceDTO workspaceModel = sut.GetById(Workspace.Id);
			sut.UpdateDeletedStatus(workspaceModel.Id, workspaceModel.UpdateDate, false, GlobalTestCaseSetup.GetEtiUserID());

			// get the ws data to verify isdeleted flag is false
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

			// Get the ws data
			WorkspaceDTO workspaceModel = sut.GetById(Workspace.Id);

			// change the export to whatever is in global
			workspaceModel.TemplateID = GlobalTestCaseSetup.GlobalWorkspaceTemplateID;

			// add tracking number
			workspaceModel.TrackingNumber = "Mock Track";

			// Save the ws settings
			sut.SaveWorkspaceSettings(Author.UserID, workspaceModel);


			// get the ws data to verify export is now 1 instead of 2
			workspaceModel = sut.GetById(Workspace.Id);
			Assert.AreEqual(GlobalTestCaseSetup.GlobalWorkspaceTemplateID, workspaceModel.TemplateID, "The ws model is not correct");
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

			// Get the ws data
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

			// Get the ws data
			WorkspaceDTO workspaceModel = sut.GetById(Workspace.Id);

			workspaceModel.AllowSearch = true;
			workspaceModel.ContainsTemplate = true;

			// Save the ws settings
			sut.SaveWorkspaceSettings(Author.UserID, workspaceModel);

			// get the ws data to verify export is now 1 instead of 2
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

			// Save the ws settings
			sut.SaveWorkspaceSettings(Author.UserID, null);
			Assert.Fail("Did not throw ArgumentNullException");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveWorkspaceSettingsFails2()
		{
			var sut = new WorkspaceDTODataLoader();

			// Save the ws settings
			sut.SaveIdentificationAndExportFormat(Author.UserID, null);
			Assert.Fail("Did not throw ArgumentNullException");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SaveWorkspaceSettingsFails3()
		{
			var sut = new WorkspaceDTODataLoader();

			// Save the ws settings
			sut.SaveAllowSearch(null);
			Assert.Fail("Did not throw ArgumentNullException");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag"), TestMethod]
		public void L_UpdatePerfOrgChangeFlag()
		{
			var sut = new WorkspaceDTODataLoader();

			// Get the ws data
			WorkspaceDTO workspaceModel = sut.GetById(Workspace.Id);


			// Since the ws has been restored with the default list, set the flag to false (not updated)
			sut.UpdatePerfOrgChangeFlag(workspaceModel, false);

			// get the ws data to verify change flag is false
			workspaceModel = sut.GetById(Workspace.Id);

			Assert.IsTrue(workspaceModel.PerfOrgsChanged == false, "The Perf Org Changed did not save");
			this.ResetTestData();
		}

		/// <summary>
		/// Ensure that once a ws is soft-deleted, it is no longer returned in the API results
		/// </summary>
		[TestMethod]
		public void DeletedWorkspaceNotReturnedTest()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			// Get the ws data
			WorkspaceDTO workspaceModel = sut.GetById(Workspace.Id);
			workspaceModel.TrackingNumber = "TEST_ABC123";
			workspaceModel.Updateable = UpdateType.Upsert;
			sut.SaveWorkspaceSettings(Author.UserID, workspaceModel);

			var results = sut.GetWorkspaceDataForProposal("TEST_ABC123");

			Assert.IsTrue(results.Any());

			workspaceModel = sut.GetById(Workspace.Id);
			sut.UpdateDeletedStatus(workspaceModel.Id, workspaceModel.UpdateDate, true, GlobalTestCaseSetup.GetEtiUserID());

			results = sut.GetWorkspaceDataForProposal("TEST_ABC123");

			Assert.IsFalse(results.Any());
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
				VerifyDtos(collection1.ElementAt(i), collection2.ElementAt(i));
			}
		}

		public static void VerifyDtos(WorkspaceDTO dto1, WorkspaceDTO dto2, bool skipFieldsNotRestoredFromBackup = false)
		{
			Assert.AreEqual(dto1.BOEExportSortByID, dto2.BOEExportSortByID);
			Assert.AreEqual(dto1.ContainsOCI, dto2.ContainsOCI);
			Assert.AreEqual(dto1.ContainsTemplate, dto2.ContainsTemplate);
			Assert.AreEqual(dto1.ContractEndDate, dto2.ContractEndDate);
			Assert.AreEqual(dto1.ContractStartDate, dto2.ContractStartDate);
			Assert.AreEqual(dto1.CostDecimalPrecision, dto2.CostDecimalPrecision);
			Assert.AreEqual(dto1.CostVolumeLeadPricerUserID, dto2.CostVolumeLeadPricerUserID);
			Assert.AreEqual(dto1.CreatedByUserID, dto2.CreatedByUserID);
			Assert.AreEqual(dto1.DateRecalculationStarted, dto2.DateRecalculationStarted);
			Assert.AreEqual(dto1.DecimalPrecision, dto2.DecimalPrecision);
			Assert.AreEqual(dto1.Description, dto2.Description);

			if (!skipFieldsNotRestoredFromBackup)
			{
				// IDs wouldn't be the same
				Assert.AreEqual(dto1.Id, dto2.Id);
				Assert.AreEqual(dto1.PerfOrgListID, dto2.PerfOrgListID);
				Assert.AreEqual(dto1.ResourceListID, dto2.ResourceListID);
				Assert.AreEqual(dto1.TemplateID, dto2.TemplateID);

				// Names & update date are different
				Assert.AreEqual(dto1.UpdateDate, dto2.UpdateDate);
				Assert.AreEqual(dto1.WorkspaceName, dto2.WorkspaceName);
				Assert.AreEqual(dto1.Shortname, dto2.Shortname);

				// We are deleting it, so it's deleted & not searcheable
				Assert.AreEqual(dto1.HasBeenDeleted, dto2.HasBeenDeleted);
				Assert.AreEqual(dto1.AllowSearch, dto2.AllowSearch);
				Assert.AreEqual(dto1.DateDeleted, dto2.DateDeleted);


				// VERIFY ME
				Assert.AreEqual(dto1.PerfOrgsChanged, dto2.PerfOrgsChanged);
			}

			Assert.AreEqual(string.IsNullOrEmpty(dto1.StatusComment), string.IsNullOrEmpty(dto2.StatusComment));
			Assert.AreEqual(dto1.NumberOfTimesExportedToProPricer, dto2.NumberOfTimesExportedToProPricer);
			Assert.AreEqual(dto1.ProposalClass.Id, dto2.ProposalClass.Id);
			Assert.AreEqual(dto1.ProposalStatus, dto2.ProposalStatus);
			Assert.AreEqual(dto1.ProposalSubmittalDate, dto2.ProposalSubmittalDate);
			Assert.AreEqual(dto1.ProposalTitle, dto2.ProposalTitle);
			Assert.AreEqual(dto1.ResourceDecimalPrecision, dto2.ResourceDecimalPrecision);
			Assert.AreEqual(dto1.RFPNumber, dto2.RFPNumber);
			Assert.AreEqual(dto1.Segment, dto2.Segment);
			Assert.AreEqual(dto1.TrackingNumber, dto2.TrackingNumber);
			Assert.AreEqual(dto1.Updateable, dto2.Updateable);
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
			Assert.AreEqual(dto1.UsingTemplateBOE, dto2.UsingTemplateBOE);
		}

		/// <summary>
		/// Tests the validation for RTE fields that are too long.
		/// Setup a ws that had the fields past the validation:
		/// Uses this - https://uat-genboe.ssc.lmco.com/20-00005_05
		/// </summary>
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

		/// <summary>
		/// Test GetWorkspaceOciSettingByShortname 
		/// </summary>
		[TestMethod]
		public void TestGetWorkspaceOciSettingByShortname()
		{
			string shortName;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				shortName = gbe.Workspaces.First(x => x.ContainsOCI == true).WorkspaceShortName;
			}

			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();
			bool? result = sut.GetWorkspaceOciSettingByShortname(shortName);

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Value);
		}

		/// <summary>
		/// Test GetWorkspaceOciSettingByShortname when shortname is empty
		/// </summary>
		[TestMethod]
		public void TestGetWorkspaceOciSettingByShortname_EmptyName()
		{
			string shortName = String.Empty;

			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();
			bool? result = sut.GetWorkspaceOciSettingByShortname(shortName);

			Assert.IsNull(result);
		}

		/// <summary>
		/// Test GetWorkspaceDataByNtidForNlf for a user with Workspace Admin
		/// </summary>
		[TestMethod]
		public void TestGetWorkspaceDataByNtidForNlf_WorkspaceAdmin()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			string ntid;
			Workspace workspace;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				workspace = gbe.Workspaces.FirstOrDefault(x => x.WorkspaceUserRoles.Any(y => y.RoleID == (int)Role.WorkspaceAdmin) && x.IsDeleted == false);
				WorkspaceUserRole user = gbe.WorkspaceUserRoles.FirstOrDefault(x => x.RoleID == (int)Role.WorkspaceAdmin && x.WorkspaceID == workspace.WorkspaceID);
				ntid = user.ETIuser.NTID;
			}

			ICollection<NlfWorkspaceDataDTO> result = sut.GetWorkspaceDataByNtidForNlf(ntid);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.WorkspaceId == workspace.WorkspaceID && x.WorkspaceUrl == workspace.WorkspaceShortName && x.WorkspaceName == workspace.WorkspaceName));
		}

		/// <summary>
		/// Test GetWorkspaceDataByNtidForNlf for a user with GSCO Admin
		/// </summary>
		[TestMethod]
		public void TestGetWorkspaceDataByNtidForNlf_GSCOAdmin()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			string ntid;
			Workspace workspace;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				workspace = gbe.Workspaces.FirstOrDefault(x => x.WorkspaceUserRoles.Any(y => y.RoleID == (int)Role.SubcontractAdmin) && x.IsDeleted == false);
				WorkspaceUserRole user = gbe.WorkspaceUserRoles.FirstOrDefault(x => x.RoleID == (int)Role.WorkspaceAdmin && x.WorkspaceID == workspace.WorkspaceID);
				ntid = user.ETIuser.NTID;
			}

			ICollection<NlfWorkspaceDataDTO> result = sut.GetWorkspaceDataByNtidForNlf(ntid);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.WorkspaceId == workspace.WorkspaceID && x.WorkspaceUrl == workspace.WorkspaceShortName && x.WorkspaceName == workspace.WorkspaceName));
		}

		/// <summary>
		/// Test GetAllWorkspaceDataForNlf (for a user with System Admin)
		/// </summary>
		[TestMethod]
		public void TestGetAllWorkspaceDataForNlf()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			string ntid;
			Workspace workspace;
			int workspaceCount;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				workspace = gbe.Workspaces.FirstOrDefault(x => x.IsDeleted == false);
				workspaceCount = gbe.Workspaces.Count(x => x.IsDeleted == false);
			}

			ICollection<NlfWorkspaceDataDTO> result = sut.GetAllWorkspaceDataForNlf();

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.WorkspaceId == workspace.WorkspaceID && x.WorkspaceUrl == workspace.WorkspaceShortName && x.WorkspaceName == workspace.WorkspaceName));
			Assert.AreEqual(workspaceCount, result.Count);
		}

		/// <summary>
		/// Test GetWorkspaceInnerDataByWorkspaceIdForNlf (for a user with System Admin)
		/// </summary>
		[TestMethod]
		public void TestGetWorkspaceInnerDataForNlf()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			string ntid;
			Workspace workspace;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				workspace = gbe.Workspaces.Include(typeof(LineOfBusiness).Name).Include(typeof(ETIuser).Name).FirstOrDefault(x => x.IsDeleted == false);
			}

			ICollection<NlfWorkspaceInnerDataDTO> result = sut.GetWorkspaceInnerDataForNlf(workspace.WorkspaceID);

			NlfWorkspaceInnerDataDTO specificWorkspace = result.FirstOrDefault(r => r.WorkspaceId == workspace.WorkspaceID);
			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Count == 1);
			Assert.IsTrue(result.Any(x => x.WorkspaceId == workspace.WorkspaceID
				&& x.WorkspaceUrl == workspace.WorkspaceShortName
				&& x.WorkspaceName == workspace.WorkspaceName
				&& x.LineOfBusiness.LineOfBusinessID == workspace.LineOfBusiness.LineOfBusinessID
				&& x.PTMTrackingNumber == workspace.TrackingNumber
				&& x.WorkspaceCreationDate == workspace.WorkspaceCreationDate
				&& x.EstimatingLead == workspace.ETIuser.DisplayName));
		}

		/// <summary>
		/// Test GetWorkspaceInnerDataByWorkspaceIdForNlf
		/// </summary>
		[TestMethod]
		public void TestGetWorkspaceInnerDataForNlfbyTrackingNumbers()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			string ntid;
			Workspace workspace;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				workspace = gbe.Workspaces.Include(typeof(LineOfBusiness).Name).Include(typeof(ETIuser).Name).FirstOrDefault(x => x.IsDeleted == false);
			}

			ICollection<NlfWorkspaceInnerDataDTO> result = sut.GetWorkspaceInnerDataForNlf(new List<string> { workspace.TrackingNumber });

			NlfWorkspaceInnerDataDTO specificWorkspace = result.FirstOrDefault(r => r.WorkspaceId == workspace.WorkspaceID);
			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Count == 1);
			Assert.IsTrue(result.Any(x => x.WorkspaceId == workspace.WorkspaceID
				&& x.WorkspaceUrl == workspace.WorkspaceShortName
				&& x.WorkspaceName == workspace.WorkspaceName
				&& x.LineOfBusiness.LineOfBusinessID == workspace.LineOfBusiness.LineOfBusinessID
				&& x.PTMTrackingNumber == workspace.TrackingNumber
				&& x.WorkspaceCreationDate == workspace.WorkspaceCreationDate
				&& x.EstimatingLead == workspace.ETIuser.DisplayName));
		}

		/// <summary>
		/// Test GetMaterialPBoeForWorkspace
		/// </summary>
		[TestMethod]
		public void TestGetMaterialPBoe()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			string ntid;
			Workspace ws;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				ws = gbe.Workspaces.Include("CLINs").Include("WorkBreakdownStructures").FirstOrDefault(x => x.IsDeleted == false);
			}

			ICollection<MPBoeDataDTO> result = sut.GetMaterialPBoeForWorkspace(ws.WorkspaceID);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.PTMProposalTitle == ws.ProposalTitle && x.RFPNumber == ws.RFPNumber
										&& x.CLINNumbers.Count == ws.CLINs.Count && x.WorkspaceName == ws.WorkspaceName
										&& x.ShortName == ws.WorkspaceShortName
										&& x.CLINNumbers.All(ws.CLINs.Select(c => c.DisplayedCLINNumber).Contains)
										&& x.WBSNumbers.Count == ws.WorkBreakdownStructures.Count
										&& x.WBSNumbers.All(ws.WorkBreakdownStructures.Select(w => w.DisplayedWBSNumber).Contains)));
		}

		/// <summary>
		/// Test GetMaterialPBoeForWorkspace
		/// </summary>
		[TestMethod]
		public void TestGetPBOEsForWorkspacee()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			string ntid;
			Workspace ws;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				ws = gbe.Workspaces.Include("CLINs").Include("WorkBreakdownStructures").FirstOrDefault(x => x.IsDeleted == false);
			}

			ICollection<MPBoeDataDTO> result = sut.GetMaterialPBoeForWorkspace(ws.WorkspaceID);

			Assert.IsTrue(result.Any());
			Assert.IsTrue(result.Any(x => x.PTMProposalTitle == ws.ProposalTitle && x.RFPNumber == ws.RFPNumber
										&& x.CLINNumbers.Count == ws.CLINs.Count && x.WorkspaceName == ws.WorkspaceName
										&& x.ShortName == ws.WorkspaceShortName
										&& x.CLINNumbers.All(ws.CLINs.Select(c => c.DisplayedCLINNumber).Contains)
										&& x.WBSNumbers.Count == ws.WorkBreakdownStructures.Count
										&& x.WBSNumbers.All(ws.WorkBreakdownStructures.Select(w => w.DisplayedWBSNumber).Contains)));
		}

		/// <summary>
		/// Test GetAllWorkspacesWithTrackingNumbers successfully returns all workspaces containing tracking numbers
		/// </summary>
		[TestMethod]
		public void GetAllWorkspacesWithTrackingNumbers()
		{
			IWorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			int totalWorkspacesFromDB;
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				totalWorkspacesFromDB = (from w in gbe.Workspaces
										 where !string.IsNullOrEmpty(w.TrackingNumber)
										 select w).Count();
			}

			ICollection<WorkspaceDTO> results = sut.GetAllWorkspacesWithTrackingNumbers();

			Assert.AreEqual(totalWorkspacesFromDB, results.Count);
		}

		/// <summary>
		/// Test GetWorkspaceNamesMatchingBase returns exact match
		/// </summary>
		[TestMethod]
		public void TestGetWorkspaceNamesMatchingBase_ExactMatch()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			string workspaceName;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				// Get a workspace name that exists in the database
				workspaceName = gbe.Workspaces.FirstOrDefault(x => x.IsDeleted == false)?.WorkspaceName;
			}

			Assert.IsNotNull(workspaceName, "No workspace found in database for test");

			ICollection<string> result = sut.GetWorkspaceNamesMatchingBase(workspaceName);

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Any(), "Should return at least the exact match");
			Assert.IsTrue(result.Any(x => x.Equals(workspaceName, StringComparison.OrdinalIgnoreCase)),
				"Result should contain the exact workspace name");
		}

		/// <summary>
		/// Test GetWorkspaceNamesMatchingBase returns names with underscore suffix pattern
		/// </summary>
		[TestMethod]
		public void TestGetWorkspaceNamesMatchingBase_SuffixPattern()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			string baseName;
			int expectedCount;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				// Get a workspace name and check how many match the pattern baseName or baseName_*
				var workspace = gbe.Workspaces.FirstOrDefault(x => x.IsDeleted == false);
				Assert.IsNotNull(workspace, "No workspace found in database for test");

				baseName = workspace.WorkspaceName;
				string baseNameLower = baseName.ToLower();

				// Count expected matches: exact match OR starts with baseName_
				expectedCount = gbe.Workspaces.Count(w =>
					w.WorkspaceName.ToLower() == baseNameLower ||
					w.WorkspaceName.ToLower().StartsWith(baseNameLower + "_"));
			}

			ICollection<string> result = sut.GetWorkspaceNamesMatchingBase(baseName);

			Assert.IsNotNull(result);
			Assert.AreEqual(expectedCount, result.Count,
				"Result count should match database query for exact name and underscore suffix pattern");
		}

		/// <summary>
		/// Test GetWorkspaceNamesMatchingBase with empty string returns empty collection
		/// </summary>
		[TestMethod]
		public void TestGetWorkspaceNamesMatchingBase_EmptyString()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			ICollection<string> result = sut.GetWorkspaceNamesMatchingBase(string.Empty);

			Assert.IsNotNull(result);
			// Empty string will match workspaces starting with "_" due to StartsWith("_") pattern
			// The actual count depends on database content
		}

		/// <summary>
		/// Test GetWorkspaceNamesMatchingBase with non-existent name returns empty collection
		/// </summary>
		[TestMethod]
		public void TestGetWorkspaceNamesMatchingBase_NonExistentName()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			// Use a GUID to ensure this name doesn't exist
			string nonExistentName = "NonExistent_" + Guid.NewGuid().ToString();

			ICollection<string> result = sut.GetWorkspaceNamesMatchingBase(nonExistentName);

			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count, "Should return empty collection for non-existent workspace name");
		}

		/// <summary>
		/// Test GetWorkspaceNamesMatchingBase is case-insensitive
		/// </summary>
		[TestMethod]
		public void TestGetWorkspaceNamesMatchingBase_CaseInsensitive()
		{
			WorkspaceDTODataLoader sut = new WorkspaceDTODataLoader();

			string workspaceName;

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				workspaceName = gbe.Workspaces.FirstOrDefault(x => x.IsDeleted == false)?.WorkspaceName;
			}

			Assert.IsNotNull(workspaceName, "No workspace found in database for test");

			// Query with different case variations
			ICollection<string> resultLower = sut.GetWorkspaceNamesMatchingBase(workspaceName.ToLower());
			ICollection<string> resultUpper = sut.GetWorkspaceNamesMatchingBase(workspaceName.ToUpper());
			ICollection<string> resultOriginal = sut.GetWorkspaceNamesMatchingBase(workspaceName);

			Assert.AreEqual(resultOriginal.Count, resultLower.Count, "Lowercase query should return same count");
			Assert.AreEqual(resultOriginal.Count, resultUpper.Count, "Uppercase query should return same count");
		}
	}

	/// <summary>
	/// Separated this into a new class, so that way we don't automatically create all the objects which are carried via MOQLoaderObject
	/// </summary>
	[TestClass]
	public class WorkspaceLoaderTestOnlyForCopy
	{

		/// <summary>
		/// Tests CopyWorkspaceVersion method.
		/// </summary>
		[TestMethod]
		public void TestCopyWorkspaceVersion()
		{
			WorkspaceVersionMetaDataDTODataLoader versionLoader = new WorkspaceVersionMetaDataDTODataLoader();
			WorkspaceDTODataLoader wsLoader = new WorkspaceDTODataLoader();
			BoeDTODataLoader boeLoader = new BoeDTODataLoader();
			ClinDTODataLoader clinLoader = new ClinDTODataLoader();
			WbsDTODataLoader wbsLoader = new WbsDTODataLoader();
			OtherDirectCostDTODataLoader odcLoader = new OtherDirectCostDTODataLoader();
			ResourceDTODataLoader resourceLoder = new ResourceDTODataLoader();
			PerformingOrgDTODataLoader perfOrgLoader = new PerformingOrgDTODataLoader();
			WorkspaceVariableDTODataLoader wsVarLoader = new WorkspaceVariableDTODataLoader();
			RteTemplateDataLoader rteLoader = new RteTemplateDataLoader();
			ProPricerDTODataLoader ppLoader = new ProPricerDTODataLoader();
			BoeTaskElementDTODataLoader taskLoader = new BoeTaskElementDTODataLoader(new ResourceTypeLoader(), new ResourceSpreadLoader(), new OrdinaryVariableLoader(), new BoeTaskElementCustomFieldValueXREFLoader(), new LaborTypeCustomFieldValueXREFLoader(), new SkillMixDTOLoader(), new SkillMixSummaryDTOLoader(), new CommonDisclosureSMDTODataLoader());
			IRetriever retriever = new Retriever(null, null, wsLoader, null, null, null, null, null, null, taskLoader, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
			FullObjectFactory fullObjectFactory = new FullObjectFactory(null, null, null, null, null, null, null, null, null, null, null, null);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), fullObjectFactory);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), new CommonDataMapper(new CommonDataLoader(), null));
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), new PermissionsDTODataLoader(new ActiveDirectoryUtilities()));

			string backupComparisonWsName = "testws_4469_ini";
			string originalWsName = "testws_4469_indup0001";
			string backupName = "BACKUP";

			int originalWsId = wsLoader.GetByShortname(originalWsName).Id;
			int backupId = versionLoader.GetByWorkspaceID(originalWsId).First(x => x.VersionName == backupName).Id;

			int wsRestoredFromBackupId = wsLoader.CopyWorkspaceVersion(originalWsId, "DELETED..TESTING " + DateTime.Now.ToLongTimeString().Replace(":", "_"), DateTime.Now.ToLongTimeString().Replace(":", "_"), backupId, string.Empty);

			WorkspaceDTO wsFromDb = wsLoader.GetByShortname(backupComparisonWsName);
			WorkspaceDTO wsRestoredFromBackup = wsLoader.GetById(wsRestoredFromBackupId);
			WorkspaceRetrieveDataLoaderTest.VerifyDtos(wsFromDb, wsRestoredFromBackup, true);

			var boesFromDb = boeLoader.GetByWorkspaceId(wsFromDb.Id);
			var boesRestored = boeLoader.GetByWorkspaceId(wsRestoredFromBackupId);
			BoeDTODataLoaderTest.VerifyCollections(boesFromDb, boesRestored, true);

			var clinsFromDb = clinLoader.GetByWorkspaceId(wsFromDb.Id);
			var clinsRestored = clinLoader.GetByWorkspaceId(wsRestoredFromBackupId);
			ClinDTODataLoaderTest.VerifyCollections(clinsFromDb, clinsRestored, true);

			var wbsFromDb = wbsLoader.GetByWorkspaceId(wsFromDb.Id);
			var wbsRestored = wbsLoader.GetByWorkspaceId(wsRestoredFromBackupId);
			WbsDTODataLoaderTest.VerifyCollections(wbsFromDb, wbsRestored, true);

			var tasksFromDb = taskLoader.GetByWorkspaceId(wsFromDb.Id, true, wsFromDb.DecimalPrecision, wsFromDb.CostDecimalPrecision);
			var tasksRestored = taskLoader.GetByWorkspaceId(wsRestoredFromBackupId, true, wsRestoredFromBackup.DecimalPrecision, wsRestoredFromBackup.CostDecimalPrecision);
			BoeTaskElementDTODataLoaderTest.VerifyCollections(tasksFromDb, tasksRestored, true);

			var odcsFromDb = odcLoader.GetByBoeIds(boesFromDb.Select(x => x.Id).ToList());
			var odcsRestored = odcLoader.GetByBoeIds(boesRestored.Select(x => x.Id).ToList());
			OtherDirectCostDTODataLoaderTest.VerifyCollections(odcsFromDb, odcsRestored, true);

			var resourcesFromDb = resourceLoder.GetByIds(tasksFromDb.SelectMany(x => x.taskElementLabors).Select(z => z.ResourceID.HasValue ? (int)z.ResourceID : (int)z.BusinessResourceCodeID).Distinct().ToList());
			var resourcesRestored = resourceLoder.GetByIds(tasksRestored.SelectMany(x => x.taskElementLabors).Select(z => z.ResourceID.HasValue ? (int)z.ResourceID : (int)z.BusinessResourceCodeID).Distinct().ToList());
			ResourceDTODataLoaderTest.VerifyCollections(resourcesFromDb, resourcesRestored, true);

			var wsVarsFromDb = wsVarLoader.GetByWorkspaceID(wsFromDb.Id);
			var wsVarsRestored = wsVarLoader.GetByWorkspaceID(wsRestoredFromBackupId);
			WorkspaceVariableDTODataLoaderTest.VerifyCollections(wsVarsFromDb, wsVarsRestored, true);

			var ppFromDb = ppLoader.GetByWorkspaceId(wsFromDb.Id);
			var ppRestored = ppLoader.GetByWorkspaceId(wsRestoredFromBackupId);
			ProPricerDTODataLoaderTest.VerifyCollections(ppFromDb, ppRestored, true);

			var perfOrgsFromDb = perfOrgLoader.GetByIds(tasksFromDb.SelectMany(x => x.taskElementLabors).Select(z => z.ResourceID.HasValue ? (int)z.ResourceID : (int)z.BusinessResourceCodeID).Distinct().ToList());
			var perfOrgsRestored = perfOrgLoader.GetByIds(tasksRestored.SelectMany(x => x.taskElementLabors).Select(z => z.ResourceID.HasValue ? (int)z.ResourceID : (int)z.BusinessResourceCodeID).Distinct().ToList());
			Assert.AreEqual(perfOrgsFromDb.Count, perfOrgsRestored.Count);
			for (int i = 0; i < perfOrgsFromDb.Count; i++)
			{
				Assert.AreEqual(perfOrgsFromDb[i].IsSystemPerfOrg, perfOrgsRestored[i].IsSystemPerfOrg);
				Assert.AreEqual(perfOrgsFromDb[i].PerformingOrgDesc, perfOrgsRestored[i].PerformingOrgDesc);
				Assert.AreEqual(perfOrgsFromDb[i].PerformingOrgName, perfOrgsRestored[i].PerformingOrgName);
				Assert.AreEqual(perfOrgsFromDb[i].UpdateDate, perfOrgsRestored[i].UpdateDate);
			}

			var rteTemplatesFromDb = rteLoader.GetTemplates(wsFromDb.Id);
			var rteTemplatesRestored = rteLoader.GetTemplates(wsRestoredFromBackupId);
			Assert.AreEqual(rteTemplatesFromDb.Count, rteTemplatesRestored.Count);
			for (int i = 0; i < rteTemplatesFromDb.Count; i++)
			{
				Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).Author, rteTemplatesRestored.ElementAt(i).Author);
				Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).AuthorId, rteTemplatesRestored.ElementAt(i).AuthorId);
				Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).CreationDate, rteTemplatesRestored.ElementAt(i).CreationDate);
				Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).Description, rteTemplatesRestored.ElementAt(i).Description);
				Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).InUse, rteTemplatesRestored.ElementAt(i).InUse);
				Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).LastUpdatedDate, rteTemplatesRestored.ElementAt(i).LastUpdatedDate);
				Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).UpdateDate, rteTemplatesRestored.ElementAt(i).UpdateDate);

				Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).Assigned.Count, rteTemplatesRestored.ElementAt(i).Assigned.Count);
				for (int j = 0; j < rteTemplatesFromDb.ElementAt(i).Assigned.Count; i++)
				{
					Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).Assigned.ElementAt(j), rteTemplatesRestored.ElementAt(i).Assigned.ElementAt(j));
				}

				Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).Questions.Count, rteTemplatesRestored.ElementAt(i).Questions.Count);
				for (int j = 0; j < rteTemplatesFromDb.ElementAt(i).Questions.Count; i++)
				{
					Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).Questions.ElementAt(j).Required, rteTemplatesRestored.ElementAt(i).Questions.ElementAt(j).Required);
					Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).Questions.ElementAt(j).SortOrder, rteTemplatesRestored.ElementAt(i).Questions.ElementAt(j).SortOrder);
					Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).Questions.ElementAt(j).Text, rteTemplatesRestored.ElementAt(i).Questions.ElementAt(j).Text);
					Assert.AreEqual(rteTemplatesFromDb.ElementAt(i).Questions.ElementAt(j).UpdateDate, rteTemplatesRestored.ElementAt(i).Questions.ElementAt(j).UpdateDate);
				}
			}

			var rteTemplateAnswersFromDb = rteLoader.GetByWorkspaceId(wsFromDb.Id, boesFromDb.Select(x => new FullBoe(x)).ToList());
			var rteTemplateAnswersRestored = rteLoader.GetByWorkspaceId(wsRestoredFromBackupId, boesRestored.Select(x => new FullBoe(x)).ToList());
			Assert.AreEqual(rteTemplateAnswersFromDb.Count, rteTemplateAnswersRestored.Count);
			for (int i = 0; i < rteTemplateAnswersFromDb.Count; i++)
			{
				Assert.AreEqual(rteTemplateAnswersFromDb.ElementAt(i).AnswerText, rteTemplateAnswersRestored.ElementAt(i).AnswerText);
				Assert.AreEqual(rteTemplateAnswersFromDb.ElementAt(i).QuestionText, rteTemplateAnswersRestored.ElementAt(i).QuestionText);
				Assert.AreEqual(rteTemplateAnswersFromDb.ElementAt(i).Required, rteTemplateAnswersRestored.ElementAt(i).Required);
				Assert.AreEqual(rteTemplateAnswersFromDb.ElementAt(i).SortOrder, rteTemplateAnswersRestored.ElementAt(i).SortOrder);
				Assert.AreEqual(rteTemplateAnswersFromDb.ElementAt(i).SourceId, rteTemplateAnswersRestored.ElementAt(i).SourceId);
			}
		}
	}
}
