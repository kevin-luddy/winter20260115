// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests.ActionLogic.ControllerLogic
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Net.Http;
	using System.Threading.Tasks;
	using System.Web.Configuration;
	using System.Web.Helpers;
	using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.Common.Email;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.CopyBOE;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.ActionLogic.WBS;
	using GenBOE.ActionLogic.WBS.BOE;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.Common.Interfaces;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;
	using IES.Common.Exceptions;
	using Microsoft.Practices.Unity;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	[TestClass]
	public class BOEControllerLogicTest : MOQObject
	{
		#region Private members
		private readonly Mock<IRetriever> _retriever = new Mock<IRetriever>();

		private readonly Mock<ICustomFieldValueDTODataLoader> _customFieldValueDTODataLoader = new Mock<ICustomFieldValueDTODataLoader>();
		private readonly Mock<IBOESummary> _boeSummary = new Mock<IBOESummary>();
		private readonly Mock<IUserDTODataLoader> _userLoader = new Mock<IUserDTODataLoader>();
		private readonly Mock<IActiveDirectoryUtilities> _ADUtils = new Mock<IActiveDirectoryUtilities>();
		private readonly Mock<IPermissionsDTODataLoader> _permissionsLoader = new Mock<IPermissionsDTODataLoader>();
		private readonly Mock<ICommonDataMapper> _commonDataMapper = new Mock<ICommonDataMapper>();
		private readonly Mock<IFullObjectFactory> Factory = new Mock<IFullObjectFactory>();
		private readonly Mock<IBOEExporter> _boeExporter = new Mock<IBOEExporter>();
		private readonly Mock<IBOECustomExporter> _boeCustomExporter = new Mock<IBOECustomExporter>();
		private readonly Mock<IGenBOEControllerLogic> _genBOEControllerLogic = new Mock<IGenBOEControllerLogic>();
		private readonly Mock<IBoeMediator> _boeMediator = new Mock<IBoeMediator>();
		private readonly Mock<IValidationHelper> _validationHelper = new Mock<IValidationHelper>();
		private readonly Mock<IBOECommentDTODataLoader> _boeCommentDTODataLoader = new Mock<IBOECommentDTODataLoader>();
		private readonly Mock<IBoeEmailer> _emailer = new Mock<IBoeEmailer>();
		private readonly Mock<IBoeTaskElementMediator> _boeTaskElementMediator = new Mock<IBoeTaskElementMediator>();
		private readonly Mock<IWorkspaceVariableDTODataLoader> _workspaceVariableDTODataLoader = new Mock<IWorkspaceVariableDTODataLoader>();
		private readonly Mock<IBOEStateMachine> _boeStateMachine = new Mock<IBOEStateMachine>();
		private readonly Mock<IVariableSelectBOEtoSumCalculation> _variableSelectBOEtoSumCalculation = new Mock<IVariableSelectBOEtoSumCalculation>();
		private readonly Mock<IBOELaborControllerLogic> _boeLaborControllerLogic = new Mock<IBOELaborControllerLogic>();
		private readonly Mock<IValidateBOE> _validateBOE = new Mock<IValidateBOE>();
		private readonly Mock<ISecurityInformation> _securityInformation = new Mock<ISecurityInformation>();
		private readonly Mock<IBOESearchDTODataLoader> _boeSearchLoader = new Mock<IBOESearchDTODataLoader>();
		private readonly Mock<ISecurityAccess> _securityAccess = new Mock<ISecurityAccess>();
		private readonly Mock<IBoeTaskElementRecalculation> _boeTaskElementRecalculation = new Mock<IBoeTaskElementRecalculation>();
		private readonly Mock<IBOEImporter> _boeImporter = new Mock<IBOEImporter>();
		private readonly Mock<IVariableCircularReferenceChecker> _variableCircularReferenceChecker = new Mock<IVariableCircularReferenceChecker>();
		private readonly Mock<IConflictBOE> _conflictBOE = new Mock<IConflictBOE>();
		private readonly Mock<INestedWBSUtilities> _nestedWBSUtilities = new Mock<INestedWBSUtilities>();
		private readonly Mock<IBoeTaskElementMediator> _taskElementMediator = new Mock<GenBOE.ActionLogic.BLL.IBoeTaskElementMediator>();
		private readonly Mock<Validator> _validator = new Mock<Validator>();
		private readonly Mock<RMSZoneTravelRatesFeesDataLoader> _zoneTravelRatesFeesDataLoader = new Mock<RMSZoneTravelRatesFeesDataLoader>();
		private readonly Mock<IMoqTypeDataLoader> moqTypeLoader = new Mock<IMoqTypeDataLoader>();
		private readonly Mock<IBoeApproverResponseDTODataLoader> boeApproverResponseLoader = new Mock<IBoeApproverResponseDTODataLoader>();
		private readonly Mock<GenBOE.ActionLogic.IESSAPClient.IESSAPClient> iesSapClient = new Mock<GenBOE.ActionLogic.IESSAPClient.IESSAPClient>(null, null);
		private readonly ITokenService tokenService = new TokenService(new MemoryCache());

		/// <summary>
		/// Test Initialize
		/// </summary>
		[TestInitialize]
		public void InitializeSystem()
		{
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IActiveDirectoryUtilities), _ADUtils.Object);
		}

		private BOEControllerLogic CreateSystem()
		{
			this.moqTypeLoader.Setup(x => x.GetByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>());

			return new BOEControllerLogic(_boeSummary.Object,
				_userLoader.Object, _ADUtils.Object, _permissionsLoader.Object, Factory.Object, _boeExporter.Object,
				_boeCustomExporter.Object, _genBOEControllerLogic.Object, _boeMediator.Object, _validationHelper.Object, _boeCommentDTODataLoader.Object, _emailer.Object,
				_boeTaskElementMediator.Object, _workspaceVariableDTODataLoader.Object, _boeStateMachine.Object, _variableSelectBOEtoSumCalculation.Object, _boeLaborControllerLogic.Object,
				_validateBOE.Object, _securityInformation.Object, _boeSearchLoader.Object, _securityAccess.Object, _boeTaskElementRecalculation.Object,
				_boeImporter.Object, _variableCircularReferenceChecker.Object, _conflictBOE.Object, _nestedWBSUtilities.Object, null, this._zoneTravelRatesFeesDataLoader.Object, null, moqTypeLoader.Object, boeApproverResponseLoader.Object, iesSapClient.Object, tokenService);
		}

		private BOEControllerLogic CreateSystemSpaceSystems()
		{
			this.moqTypeLoader.Setup(x => x.GetByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>());

			return new BOEControllerLogicSpaceSystems(_boeSummary.Object, _userLoader.Object, _ADUtils.Object, _permissionsLoader.Object,
				Factory.Object, _boeExporter.Object, _boeCustomExporter.Object, _genBOEControllerLogic.Object, _boeMediator.Object,
				_validationHelper.Object, _boeCommentDTODataLoader.Object, _emailer.Object, _boeTaskElementMediator.Object, _workspaceVariableDTODataLoader.Object,
				_boeStateMachine.Object, _variableSelectBOEtoSumCalculation.Object, _boeLaborControllerLogic.Object, _validateBOE.Object, _securityInformation.Object,
				_boeSearchLoader.Object, _securityAccess.Object, _boeTaskElementRecalculation.Object, _boeImporter.Object,
				_variableCircularReferenceChecker.Object, _conflictBOE.Object, _nestedWBSUtilities.Object, null, this._zoneTravelRatesFeesDataLoader.Object, null, moqTypeLoader.Object, boeApproverResponseLoader.Object, iesSapClient.Object, tokenService);
		}

		private BOEControllerLogic CreateSystemMST()
		{
			this.moqTypeLoader.Setup(x => x.GetByBoeId(It.IsAny<int>())).Returns(new List<MoqTypeSelection>());

			return new BOEControllerLogicMST(_boeSummary.Object, _userLoader.Object, _ADUtils.Object, _permissionsLoader.Object,
				Factory.Object, _boeExporter.Object, _boeCustomExporter.Object, _genBOEControllerLogic.Object, _boeMediator.Object,
				_validationHelper.Object, _boeCommentDTODataLoader.Object, _emailer.Object, _boeTaskElementMediator.Object, _workspaceVariableDTODataLoader.Object,
				_boeStateMachine.Object, _variableSelectBOEtoSumCalculation.Object, _boeLaborControllerLogic.Object, _validateBOE.Object, _securityInformation.Object,
				_boeSearchLoader.Object, _securityAccess.Object, _boeTaskElementRecalculation.Object, _boeImporter.Object,
				_variableCircularReferenceChecker.Object, _conflictBOE.Object, _nestedWBSUtilities.Object, new Mock<OffloadRatesDTOLoader>().Object, null, this._zoneTravelRatesFeesDataLoader.Object, null, moqTypeLoader.Object, boeApproverResponseLoader.Object, iesSapClient.Object, tokenService);
		}

		private void DoGetCreateBOEHeaderMVTest(BOEControllerLogic sut, CompanyConfiguration config)
		{
			BoeDTO boe = new BoeDTO { Id = 1, WorkspaceID = this.Workspace.Id, Title = "Test BOE", DataSource = "MySource", HistoricMetricDisclosureChecked = true, UpdateDate = new System.DateTime(1992, 2, 12) };

			//Act
			IBOEHeaderModelView theModelView = sut.GetCreateBOEHeaderMV(boe, null);

			//Assert: make sure data matches up
			Assert.AreEqual(boe.Title, theModelView.Title);
			Assert.AreEqual(boe.Id, theModelView.BOEID);
			Assert.AreEqual(boe.DataSource, theModelView.DataSource);
			Assert.IsTrue(theModelView.HistoricMetricDisclosureChecked);
			Assert.AreEqual(boe.UpdateDate, theModelView.UpdateDate);

			theModelView.Title = "Test BOE modified";
			Assert.AreNotEqual(boe.Title, theModelView.Title);

			switch (config)
			{
				case CompanyConfiguration.ISGS:
					BOEHeaderModelView theModelViewISGS = theModelView as BOEHeaderModelView;
					Assert.IsNotNull(theModelViewISGS, "The IBOEHeaderModelView is not the required type.");
					break;

				case CompanyConfiguration.SpaceSystems:
					BOEHeaderSpaceModelView theModelViewSSC = theModelView as BOEHeaderSpaceModelView;
					Assert.IsNotNull(theModelViewSSC, "The IBOEHeaderModelView is not the required type.");
					break;

				case CompanyConfiguration.MST:
					// We are re-using the SSC model view for MST
					BOEHeaderSpaceModelView theModelViewMST = theModelView as BOEHeaderSpaceModelView;
					Assert.IsNotNull(theModelViewMST, "The IBOEHeaderModelView is not the required type.");
					break;
			}
		}

		#endregion

		#region GetCreateBOEHeaderMV Tests


		[TestMethod]
		public void GetCreateBOEHeaderMVTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			DoGetCreateBOEHeaderMVTest(sut, CompanyConfiguration.ISGS);
		}

		[TestMethod]
		public void GetCreateBOEHeaderMVTestSpaceSystems()
		{
			BOEControllerLogic sut = this.CreateSystemSpaceSystems();
			DoGetCreateBOEHeaderMVTest(sut, CompanyConfiguration.SpaceSystems);
		}

		[TestMethod]
		public void GetCreateBOEHeaderMVTestMST()
		{
			BOEControllerLogic sut = this.CreateSystemMST();
			DoGetCreateBOEHeaderMVTest(sut, CompanyConfiguration.MST);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetCreateBOEHeaderMVExceptionTest1()
		{
			BOEControllerLogic sut = this.CreateSystem();
			sut.GetCreateBOEHeaderMV(null, null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetCreateBOEHeaderMVExceptionTest1SpaceSystems()
		{
			BOEControllerLogic sut = this.CreateSystemSpaceSystems();
			sut.GetCreateBOEHeaderMV(null, null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetCreateBOEHeaderMVExceptionTest1MST()
		{
			BOEControllerLogic sut = this.CreateSystemMST();
			sut.GetCreateBOEHeaderMV(null, null);
		}

		#endregion

		#region BOESearchResults Tests

		[TestMethod]
		public void BOEAdvancedSearchResultsTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);

			FullWorkspace ws1 = new FullWorkspace() { Id = 1 };
			FullWorkspace ws2 = new FullWorkspace() { Id = 2 };
			Factory.Setup(x => x.CreateFullWorkspace(ws1.Id)).Returns(ws1);
			Factory.Setup(x => x.CreateFullWorkspace(ws2.Id)).Returns(ws2);
			FullBoe boe1 = new FullBoe() { Id = 1, WorkspaceID = ws1.Id, Workspace = ws1, Description = "BOE1" };
			FullBoe boe2 = new FullBoe() { Id = 2, WorkspaceID = ws2.Id, Workspace = ws2, Description = "BOE2" };
			FullBoe boe3 = new FullBoe() { Id = 3, WorkspaceID = ws2.Id, Workspace = ws2, Description = "BOE3" };
			BoeTaskElementDTO taskElement = new BoeTaskElementDTO { BoeID = boe1.Id, Id = 1 };
			UserDTO user = new UserDTO { DisplayName = "I. M. Auser", NTID = "imauser", UserID = 1 };
			UserDTO[] authorsArray = new UserDTO[] { user };
			Collection<PermissionsDTO> perm1 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = authorsArray[0].UserID } };

			BOESearchResultDTO res1 = new BOESearchResultDTO() { BOEID = boe1.Id, WorkspaceID = boe1.WorkspaceID };
			BOESearchResultDTO res2 = new BOESearchResultDTO() { BOEID = boe2.Id, WorkspaceID = boe2.WorkspaceID };
			BOESearchResultDTO res3 = new BOESearchResultDTO() { BOEID = boe3.Id, WorkspaceID = boe3.WorkspaceID };
			ICollection<BOESearchResultDTO> searchResults = new Collection<BOESearchResultDTO>();
			searchResults.Add(res1);
			searchResults.Add(res2);
			searchResults.Add(res3);

			BOESearchResult boeRes1 = new BOESearchResult() { BOEID = boe1.Id, WorkspaceId = boe1.WorkspaceID };
			BOESearchResult boeRes2 = new BOESearchResult() { BOEID = boe2.Id, WorkspaceId = boe2.WorkspaceID };
			BOESearchResult boeRes3 = new BOESearchResult() { BOEID = boe3.Id, WorkspaceId = boe3.WorkspaceID };
			ICollection<BOESearchResult> boeSearchResults1 = new Collection<BOESearchResult>
			{
				boeRes1,
				boeRes2,
				boeRes3
			};
			ICollection<BOESearchResult> boeSearchResults2 = new Collection<BOESearchResult>
			{
				boeRes2,
				boeRes3
			};

			// Setup mock calls.
			_retriever.Setup(i => i.GetWorkspaceVariableDTOsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<WorkspaceVariableDTO>());
			_retriever.Setup(i => i.GetBoeTaskElementCollectionByBoeId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO> { taskElement });
			_retriever.Setup(i => i.GetBoeTaskElementCollectionByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO> { taskElement });
			_retriever.Setup(i => i.GetCurrentActiveUser()).Returns(user);
			_retriever.Setup(i => i.GetTravelCollectionByBoeID(It.IsAny<int>(), false)).Returns(new List<TravelDTO>());

			Factory.Setup(x => x.CreateFullBoe(boe1.Id)).Returns(boe1);
			Factory.Setup(x => x.CreateFullBoe(boe2.Id)).Returns(boe2);
			Factory.Setup(x => x.CreateFullBoe(boe3.Id)).Returns(boe3);
			_permissionsLoader.Setup(x => x.GetBOEPermissions(It.IsAny<ICollection<int>>())).Returns(perm1);

			BOEAdvancedSearchModelView advSearchParams = new BOEAdvancedSearchModelView();
			_boeSearchLoader.Setup(x => x.GetAdvancedSearchResults(It.IsAny<BOESearchDTO>())).Returns(searchResults);

			_boeSearchLoader.Setup(x => x.GetSearchResults(new List<int> { 1, 2, 3 })).Returns(boeSearchResults1);
			_boeSearchLoader.Setup(x => x.GetSearchResults(new List<int> { 2, 3 })).Returns(boeSearchResults2);
			// Test Domestic User
			_securityInformation.Setup(x => x.IsDomesticUser(System.Threading.Thread.CurrentPrincipal)).Returns(true);
			SearchResultsModelView results = sut.AdvancedSearchForBOEs(ws1, boe1.Id, advSearchParams);

			// verify results
			Assert.AreEqual(results.SearchResultsMessage, string.Empty);
			Assert.AreEqual(3, results.BOEResults.Count);
			for (int x = 0; x < searchResults.Count; x++)
			{
				Assert.IsTrue(searchResults.ElementAt(x).BOEID == results.BOEResults.ElementAt(x).BOEID);
			}

			// Test Foreign User
			_securityInformation.Setup(x => x.IsDomesticUser(System.Threading.Thread.CurrentPrincipal)).Returns(false);
			_securityAccess.Setup(x => x.IsAuthorized(It.Is<SecurityPermissionsRequested>(s => s.WorkspaceId == 1), It.IsAny<WorkspaceDTO>(), It.IsAny<Collection<SecurityPermissionsResponse>>())).Returns(SecurityAuthorization.None);
			_securityAccess.Setup(x => x.IsAuthorized(It.Is<SecurityPermissionsRequested>(s => s.WorkspaceId == 2), It.IsAny<WorkspaceDTO>(), It.IsAny<Collection<SecurityPermissionsResponse>>())).Returns(SecurityAuthorization.Read);
			results = sut.AdvancedSearchForBOEs(ws1, boe1.Id, advSearchParams);

			// verify results
			Assert.AreEqual(results.SearchResultsMessage, string.Empty);
			Assert.AreEqual(2, results.BOEResults.Count);
			Assert.IsTrue(searchResults.ElementAt(1).BOEID == results.BOEResults.ElementAt(0).BOEID);
			Assert.IsTrue(searchResults.ElementAt(2).BOEID == results.BOEResults.ElementAt(1).BOEID);
		}

		[TestMethod]
		public void BOEQuickSearchResultsTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);

			FullWorkspace ws1 = new FullWorkspace() { Id = 1 };
			FullWorkspace ws2 = new FullWorkspace() { Id = 2 };
			Factory.Setup(x => x.CreateFullWorkspace(ws1.Id)).Returns(ws1);
			Factory.Setup(x => x.CreateFullWorkspace(ws2.Id)).Returns(ws2);
			FullBoe boe1 = new FullBoe() { Id = 1, WorkspaceID = ws1.Id, Workspace = ws1, Description = "BOE1" };
			FullBoe boe2 = new FullBoe() { Id = 2, WorkspaceID = ws2.Id, Workspace = ws2, Description = "BOE2" };
			FullBoe boe3 = new FullBoe() { Id = 3, WorkspaceID = ws2.Id, Workspace = ws2, Description = "BOE3" };
			BoeTaskElementDTO taskElement = new BoeTaskElementDTO { BoeID = boe1.Id, Id = 1 };
			UserDTO user = new UserDTO { DisplayName = "I. M. Auser", NTID = "imauser", UserID = 1 };
			UserDTO[] authorsArray = new UserDTO[] { user };
			Collection<PermissionsDTO> perm1 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = authorsArray[0].UserID } };

			BOESearchResultDTO res1 = new BOESearchResultDTO() { BOEID = boe1.Id, WorkspaceID = boe1.WorkspaceID };
			BOESearchResultDTO res2 = new BOESearchResultDTO() { BOEID = boe2.Id, WorkspaceID = boe2.WorkspaceID };
			BOESearchResultDTO res3 = new BOESearchResultDTO() { BOEID = boe3.Id, WorkspaceID = boe3.WorkspaceID };
			ICollection<BOESearchResultDTO> searchResults = new Collection<BOESearchResultDTO>();
			searchResults.Add(res1);
			searchResults.Add(res2);
			searchResults.Add(res3);

			BOESearchResult boeRes1 = new BOESearchResult() { BOEID = boe1.Id, WorkspaceId = boe1.WorkspaceID };
			BOESearchResult boeRes2 = new BOESearchResult() { BOEID = boe2.Id, WorkspaceId = boe2.WorkspaceID };
			BOESearchResult boeRes3 = new BOESearchResult() { BOEID = boe3.Id, WorkspaceId = boe3.WorkspaceID };
			ICollection<BOESearchResult> boeSearchResults1 = new Collection<BOESearchResult>
			{
				boeRes1,
				boeRes2,
				boeRes3
			};
			ICollection<BOESearchResult> boeSearchResults2 = new Collection<BOESearchResult>
			{
				boeRes2,
				boeRes3
			};

			TravelDTO travelDto = new TravelDTO
			{
				BoeID = boe1.Id,
				Description = "desc",
				Id = 500,
				MSTTravelTrips = new List<MSTTravelTripType>
				{
					new MSTTravelTripType {
						BoeID = boe1.Id,
						Id = 501,
						Purpose = "purpose",
						ModeID = MSTTravelMode.ZoneNoAirfare,
						PerfOrgID = 4
					}
				},
				TaskTitle = "Travel Task Title"

			};

			// Setup mock calls.
			_retriever.Setup(i => i.GetWorkspaceVariableDTOsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<WorkspaceVariableDTO>());
			_retriever.Setup(i => i.GetBoeTaskElementCollectionByBoeId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO> { taskElement });
			_retriever.Setup(i => i.GetBoeTaskElementCollectionByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO> { taskElement });
			_retriever.Setup(i => i.GetCurrentActiveUser()).Returns(user);
			_retriever.Setup(i => i.GetTravelCollectionByBoeID(It.IsAny<int>(), false)).Returns(new List<TravelDTO> { travelDto });

			Factory.Setup(x => x.CreateFullBoe(boe1.Id)).Returns(boe1);
			Factory.Setup(x => x.CreateFullBoe(boe2.Id)).Returns(boe2);
			Factory.Setup(x => x.CreateFullBoe(boe3.Id)).Returns(boe3);
			_permissionsLoader.Setup(x => x.GetBOEPermissions(It.IsAny<ICollection<int>>())).Returns(perm1);

			// --------------------------------------- Quick Search -------------------------------------------------
			BOEQuickSearchModelView quickSearchParams = new BOEQuickSearchModelView();
			_boeSearchLoader.Setup(x => x.GetQuickSearchResults(It.IsAny<BOESearchDTO>())).Returns(searchResults);
			_boeSearchLoader.Setup(x => x.GetSearchResults(new List<int> { 1, 2, 3 })).Returns(boeSearchResults1);
			_boeSearchLoader.Setup(x => x.GetSearchResults(new List<int> { 2, 3 })).Returns(boeSearchResults2);

			// Test Domestic User
			_securityInformation.Setup(x => x.IsDomesticUser(System.Threading.Thread.CurrentPrincipal)).Returns(true);
			SearchResultsModelView results = sut.QuickSearchForBOEs(ws1, boe1.Id, quickSearchParams);

			// verify results
			Assert.AreEqual(results.SearchResultsMessage, string.Empty);
			Assert.AreEqual(3, results.BOEResults.Count);
			for (int x = 0; x < searchResults.Count; x++)
			{
				Assert.IsTrue(searchResults.ElementAt(x).BOEID == results.BOEResults.ElementAt(x).BOEID);
			}

			// Test Foreign User
			_securityInformation.Setup(x => x.IsDomesticUser(System.Threading.Thread.CurrentPrincipal)).Returns(false);
			_securityAccess.Setup(x => x.IsAuthorized(It.Is<SecurityPermissionsRequested>(s => s.WorkspaceId == 1), It.IsAny<WorkspaceDTO>(), It.IsAny<Collection<SecurityPermissionsResponse>>())).Returns(SecurityAuthorization.None);
			_securityAccess.Setup(x => x.IsAuthorized(It.Is<SecurityPermissionsRequested>(s => s.WorkspaceId == 2), It.IsAny<WorkspaceDTO>(), It.IsAny<Collection<SecurityPermissionsResponse>>())).Returns(SecurityAuthorization.Read);
			results = sut.QuickSearchForBOEs(ws1, boe1.Id, quickSearchParams);

			// verify results
			Assert.AreEqual(results.SearchResultsMessage, string.Empty);
			Assert.AreEqual(2, results.BOEResults.Count);
			Assert.IsTrue(searchResults.ElementAt(1).BOEID == results.BOEResults.ElementAt(0).BOEID);
			Assert.IsTrue(searchResults.ElementAt(2).BOEID == results.BOEResults.ElementAt(1).BOEID);
		}

		[TestMethod]
		public void BOEProjectMapAdvancedSearchResultsTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);

			FullWorkspace ws1 = new FullWorkspace() { Id = 1, ProjectMapType = ProjectMapType.NonTimePhasedProjectMap };
			FullWorkspace ws2 = new FullWorkspace() { Id = 2, ProjectMapType = ProjectMapType.NonTimePhasedProjectMap };
			Factory.Setup(x => x.CreateFullWorkspace(ws1.Id)).Returns(ws1);
			Factory.Setup(x => x.CreateFullWorkspace(ws2.Id)).Returns(ws2);
			FullBoe boe1 = new FullBoe() { Id = 1, WorkspaceID = ws1.Id, Workspace = ws1, Description = "BOE1", CamName = "BOE1" };
			FullBoe boe2 = new FullBoe() { Id = 2, WorkspaceID = ws2.Id, Workspace = ws2, Description = "BOE2" };
			FullBoe boe3 = new FullBoe() { Id = 3, WorkspaceID = ws2.Id, Workspace = ws2, Description = "BOE3", SOWTitle = "BOE3" };
			BoeTaskElementDTO taskElement = new BoeTaskElementDTO { BoeID = boe1.Id, Id = 1, Description = "BOE1" };
			UserDTO user = new UserDTO { DisplayName = "I. M. Auser", NTID = "imauser", UserID = 1 };
			UserDTO[] authorsArray = new UserDTO[] { user };
			Collection<PermissionsDTO> perm1 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = authorsArray[0].UserID } };

			BOESearchResultDTO res1 = new BOESearchResultDTO() { ProjectMapId = boe1.Id, WorkspaceID = boe1.WorkspaceID };
			BOESearchResultDTO res2 = new BOESearchResultDTO() { ProjectMapId = boe2.Id, WorkspaceID = boe2.WorkspaceID };
			BOESearchResultDTO res3 = new BOESearchResultDTO() { ProjectMapId = boe3.Id, WorkspaceID = boe3.WorkspaceID };
			ICollection<BOESearchResultDTO> searchResults = new Collection<BOESearchResultDTO>();
			searchResults.Add(res1);
			searchResults.Add(res2);
			searchResults.Add(res3);

			BOESearchResult boeRes1 = new BOESearchResult() { ProjectMapId = boe1.Id, WorkspaceId = boe1.WorkspaceID };
			BOESearchResult boeRes2 = new BOESearchResult() { ProjectMapId = boe2.Id, WorkspaceId = boe2.WorkspaceID };
			BOESearchResult boeRes3 = new BOESearchResult() { ProjectMapId = boe3.Id, WorkspaceId = boe3.WorkspaceID };
			ICollection<BOESearchResult> boeSearchResults1 = new Collection<BOESearchResult>
			{
				boeRes1,
				boeRes2,
				boeRes3
			};
			ICollection<BOESearchResult> boeSearchResults2 = new Collection<BOESearchResult>
			{
				boeRes2,
				boeRes3
			};

			// Setup mock calls.
			_retriever.Setup(i => i.GetWorkspaceVariableDTOsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<WorkspaceVariableDTO>());
			_retriever.Setup(i => i.GetBoeTaskElementCollectionByBoeId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO> { taskElement });
			_retriever.Setup(i => i.GetBoeTaskElementCollectionByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO> { taskElement });
			_retriever.Setup(i => i.GetCurrentActiveUser()).Returns(user);

			Factory.Setup(x => x.CreateFullBoe(boe1.Id)).Returns(boe1);
			Factory.Setup(x => x.CreateFullBoe(boe2.Id)).Returns(boe2);
			Factory.Setup(x => x.CreateFullBoe(boe3.Id)).Returns(boe3);
			_permissionsLoader.Setup(x => x.GetBOEPermissions(It.IsAny<ICollection<int>>())).Returns(perm1);

			BOEProjectMapAdvancedSearchModelView advSearchParams = new BOEProjectMapAdvancedSearchModelView();
			_boeSearchLoader.Setup(x => x.GetAdvancedSearchResults(It.IsAny<BOEProjectMapSearchDTO>())).Returns(searchResults);

			_boeSearchLoader.Setup(x => x.GetSearchResults(new List<int> { 1, 2, 3 })).Returns(boeSearchResults1);
			_boeSearchLoader.Setup(x => x.GetSearchResults(new List<int> { 2, 3 })).Returns(boeSearchResults2);
			// Test Domestic User
			_securityInformation.Setup(x => x.IsDomesticUser(System.Threading.Thread.CurrentPrincipal)).Returns(true);
			SearchResultsModelView results = sut.AdvancedSearchForBOEs(ws1, advSearchParams);

			// verify results
			Assert.AreEqual(results.SearchResultsMessage, string.Empty);
			Assert.AreEqual(3, results.BOEResults.Count);
			for (int x = 0; x < searchResults.Count; x++)
			{
				Assert.IsTrue(searchResults.ElementAt(x).BOEID == results.BOEResults.ElementAt(x).BOEID);
			}

			// Test Foreign User
			_securityInformation.Setup(x => x.IsDomesticUser(System.Threading.Thread.CurrentPrincipal)).Returns(false);
			_securityAccess.Setup(x => x.IsAuthorized(It.Is<SecurityPermissionsRequested>(s => s.WorkspaceId == 1), It.IsAny<WorkspaceDTO>(), It.IsAny<Collection<SecurityPermissionsResponse>>())).Returns(SecurityAuthorization.None);
			_securityAccess.Setup(x => x.IsAuthorized(It.Is<SecurityPermissionsRequested>(s => s.WorkspaceId == 2), It.IsAny<WorkspaceDTO>(), It.IsAny<Collection<SecurityPermissionsResponse>>())).Returns(SecurityAuthorization.Read);
			results = sut.AdvancedSearchForBOEs(ws1, advSearchParams);

			// verify results
			Assert.AreEqual(results.SearchResultsMessage, string.Empty);
			Assert.AreEqual(2, results.BOEResults.Count);
			Assert.IsTrue(searchResults.ElementAt(1).BOEID == results.BOEResults.ElementAt(0).BOEID);
			Assert.IsTrue(searchResults.ElementAt(2).BOEID == results.BOEResults.ElementAt(1).BOEID);
		}

		[TestMethod]
		public void BOEProjectMapQuickSearchResultsTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);

			FullWorkspace ws1 = new FullWorkspace() { Id = 1, ProjectMapType = ProjectMapType.NonTimePhasedProjectMap };
			FullWorkspace ws2 = new FullWorkspace() { Id = 2, ProjectMapType = ProjectMapType.NonTimePhasedProjectMap };
			Factory.Setup(x => x.CreateFullWorkspace(ws1.Id)).Returns(ws1);
			Factory.Setup(x => x.CreateFullWorkspace(ws2.Id)).Returns(ws2);
			FullBoe boe1 = new FullBoe() { Id = 1, WorkspaceID = ws1.Id, Workspace = ws1, Description = "BOE1" };
			FullBoe boe2 = new FullBoe() { Id = 2, WorkspaceID = ws2.Id, Workspace = ws2, Description = "BOE2" };
			FullBoe boe3 = new FullBoe() { Id = 3, WorkspaceID = ws2.Id, Workspace = ws2, Description = "BOE3" };
			BoeTaskElementDTO taskElement = new BoeTaskElementDTO { BoeID = boe1.Id, Id = 1 };
			UserDTO user = new UserDTO { DisplayName = "I. M. Auser", NTID = "imauser", UserID = 1 };
			UserDTO[] authorsArray = new UserDTO[] { user };
			Collection<PermissionsDTO> perm1 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = authorsArray[0].UserID } };

			BOESearchResultDTO res1 = new BOESearchResultDTO() { ProjectMapId = boe1.Id, WorkspaceID = boe1.WorkspaceID };
			BOESearchResultDTO res2 = new BOESearchResultDTO() { ProjectMapId = boe2.Id, WorkspaceID = boe2.WorkspaceID };
			BOESearchResultDTO res3 = new BOESearchResultDTO() { ProjectMapId = boe3.Id, WorkspaceID = boe3.WorkspaceID };
			ICollection<BOESearchResultDTO> searchResults = new Collection<BOESearchResultDTO>();
			searchResults.Add(res1);
			searchResults.Add(res2);
			searchResults.Add(res3);

			BOESearchResult boeRes1 = new BOESearchResult() { ProjectMapId = boe1.Id, WorkspaceId = boe1.WorkspaceID };
			BOESearchResult boeRes2 = new BOESearchResult() { ProjectMapId = boe2.Id, WorkspaceId = boe2.WorkspaceID };
			BOESearchResult boeRes3 = new BOESearchResult() { ProjectMapId = boe3.Id, WorkspaceId = boe3.WorkspaceID };
			ICollection<BOESearchResult> boeSearchResults1 = new Collection<BOESearchResult>
			{
				boeRes1,
				boeRes2,
				boeRes3
			};
			ICollection<BOESearchResult> boeSearchResults2 = new Collection<BOESearchResult>
			{
				boeRes2,
				boeRes3
			};

			// Setup mock calls.
			_retriever.Setup(i => i.GetWorkspaceVariableDTOsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<WorkspaceVariableDTO>());
			_retriever.Setup(i => i.GetBoeTaskElementCollectionByBoeId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO> { taskElement });
			_retriever.Setup(i => i.GetBoeTaskElementCollectionByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO> { taskElement });
			_retriever.Setup(i => i.GetCurrentActiveUser()).Returns(user);

			Factory.Setup(x => x.CreateFullBoe(boe1.Id)).Returns(boe1);
			Factory.Setup(x => x.CreateFullBoe(boe2.Id)).Returns(boe2);
			Factory.Setup(x => x.CreateFullBoe(boe3.Id)).Returns(boe3);
			_permissionsLoader.Setup(x => x.GetBOEPermissions(It.IsAny<ICollection<int>>())).Returns(perm1);

			// --------------------------------------- Quick Search -------------------------------------------------
			BOEQuickSearchModelView quickSearchParams = new BOEQuickSearchModelView();
			_boeSearchLoader.Setup(x => x.GetQuickSearchResults(It.IsAny<BOEProjectMapSearchDTO>())).Returns(searchResults);
			_boeSearchLoader.Setup(x => x.GetSearchResults(new List<int> { 1, 2, 3 })).Returns(boeSearchResults1);
			_boeSearchLoader.Setup(x => x.GetSearchResults(new List<int> { 2, 3 })).Returns(boeSearchResults2);

			// Test Domestic User
			_securityInformation.Setup(x => x.IsDomesticUser(System.Threading.Thread.CurrentPrincipal)).Returns(true);
			SearchResultsModelView results = sut.QuickSearchForBOEs(ws1, boe1.Id, quickSearchParams);

			// verify results
			Assert.AreEqual(results.SearchResultsMessage, string.Empty);
			Assert.AreEqual(3, results.BOEResults.Count);
			for (int x = 0; x < searchResults.Count; x++)
			{
				Assert.IsTrue(searchResults.ElementAt(x).BOEID == results.BOEResults.ElementAt(x).BOEID);
			}

			// Test Foreign User
			_securityInformation.Setup(x => x.IsDomesticUser(System.Threading.Thread.CurrentPrincipal)).Returns(false);
			_securityAccess.Setup(x => x.IsAuthorized(It.Is<SecurityPermissionsRequested>(s => s.WorkspaceId == 1), It.IsAny<WorkspaceDTO>(), It.IsAny<Collection<SecurityPermissionsResponse>>())).Returns(SecurityAuthorization.None);
			_securityAccess.Setup(x => x.IsAuthorized(It.Is<SecurityPermissionsRequested>(s => s.WorkspaceId == 2), It.IsAny<WorkspaceDTO>(), It.IsAny<Collection<SecurityPermissionsResponse>>())).Returns(SecurityAuthorization.Read);
			results = sut.QuickSearchForBOEs(ws1, boe1.Id, quickSearchParams);

			// verify results
			Assert.AreEqual(results.SearchResultsMessage, string.Empty);
			Assert.AreEqual(2, results.BOEResults.Count);
			Assert.IsTrue(searchResults.ElementAt(1).BOEID == results.BOEResults.ElementAt(0).BOEID);
			Assert.IsTrue(searchResults.ElementAt(2).BOEID == results.BOEResults.ElementAt(1).BOEID);
		}

		[TestMethod]
		public void BOESearchResultsThresholdTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);

			FullWorkspace ws1 = new FullWorkspace() { Id = 1 };
			FullWorkspace ws2 = new FullWorkspace() { Id = 2 };
			Factory.Setup(x => x.CreateFullWorkspace(ws1.Id)).Returns(ws1);
			Factory.Setup(x => x.CreateFullWorkspace(ws2.Id)).Returns(ws2);
			FullBoe boe1 = new FullBoe() { Id = 1, WorkspaceID = ws1.Id, Workspace = ws1, Description = "BOE1" };
			FullBoe boe2 = new FullBoe() { Id = 2, WorkspaceID = ws2.Id, Workspace = ws2, Description = "BOE2" };
			FullBoe boe3 = new FullBoe() { Id = 3, WorkspaceID = ws2.Id, Workspace = ws2, Description = "BOE3" };
			BoeTaskElementDTO taskElement = new BoeTaskElementDTO { BoeID = boe1.Id, Id = 1 };
			UserDTO user = new UserDTO { DisplayName = "I. M. Auser", NTID = "imauser", UserID = 1 };
			UserDTO[] authorsArray = new UserDTO[] { user };
			Collection<PermissionsDTO> perm1 = new Collection<PermissionsDTO> { new PermissionsDTO { ETIUserId = authorsArray[0].UserID } };
			BOESearchResult boeRes1 = new BOESearchResult() { BOEID = boe1.Id, WorkspaceId = boe1.WorkspaceID };
			BOESearchResult boeRes2 = new BOESearchResult() { BOEID = boe2.Id, WorkspaceId = boe2.WorkspaceID };
			BOESearchResult boeRes3 = new BOESearchResult() { BOEID = boe3.Id, WorkspaceId = boe3.WorkspaceID };
			ICollection<BOESearchResult> boeSearchResults = new Collection<BOESearchResult>
			{
				boeRes2,
				boeRes3
			};
			BOESearchResultDTO res1 = new BOESearchResultDTO() { BOEID = boe1.Id, WorkspaceID = boe1.WorkspaceID };
			BOESearchResultDTO res2 = new BOESearchResultDTO() { BOEID = boe2.Id, WorkspaceID = boe2.WorkspaceID };
			BOESearchResultDTO res3 = new BOESearchResultDTO() { BOEID = boe3.Id, WorkspaceID = boe3.WorkspaceID };
			ICollection<BOESearchResultDTO> searchResults = new Collection<BOESearchResultDTO>();
			searchResults.Add(res1);
			searchResults.Add(res2);
			searchResults.Add(res3);
			// add more searchResults in order to exceed the threshold
			int searchResultsThreshold = ConfigurationUtilities.GetAppSetting<int>("SearchResultsThreshold", Constants.SEARCH_RESULTS_THRESHOLD_DEFAULT);
			for (int x = 0; x < searchResultsThreshold; x++)
			{
				searchResults.Add(res1);
			}

			// Setup mock calls.
			_retriever.Setup(i => i.GetWorkspaceVariableDTOsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<WorkspaceVariableDTO>());
			_retriever.Setup(i => i.GetBoeTaskElementCollectionByBoeId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO> { taskElement });
			_retriever.Setup(i => i.GetBoeTaskElementCollectionByWorkspaceId(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new List<BoeTaskElementDTO> { taskElement });
			_retriever.Setup(i => i.GetCurrentActiveUser()).Returns(user);
			_retriever.Setup(i => i.GetTravelCollectionByBoeID(It.IsAny<int>(), false)).Returns(new List<TravelDTO>());

			Factory.Setup(x => x.CreateFullBoe(boe1.Id)).Returns(boe1);
			Factory.Setup(x => x.CreateFullBoe(boe2.Id)).Returns(boe2);
			Factory.Setup(x => x.CreateFullBoe(boe3.Id)).Returns(boe3);
			_permissionsLoader.Setup(x => x.GetBOEPermissions(It.IsAny<ICollection<int>>())).Returns(perm1);
			_securityInformation.Setup(x => x.IsDomesticUser(System.Threading.Thread.CurrentPrincipal)).Returns(false);
			_securityAccess.Setup(x => x.IsAuthorized(It.Is<SecurityPermissionsRequested>(s => s.WorkspaceId == 1), It.IsAny<WorkspaceDTO>(), It.IsAny<Collection<SecurityPermissionsResponse>>())).Returns(SecurityAuthorization.None);
			_securityAccess.Setup(x => x.IsAuthorized(It.Is<SecurityPermissionsRequested>(s => s.WorkspaceId == 2), It.IsAny<WorkspaceDTO>(), It.IsAny<Collection<SecurityPermissionsResponse>>())).Returns(SecurityAuthorization.Read);
			_boeSearchLoader.Setup(x => x.GetQuickSearchResults(It.IsAny<BOESearchDTO>())).Returns(searchResults);
			_boeSearchLoader.Setup(x => x.GetSearchResults(It.IsAny<ICollection<int>>())).Returns(boeSearchResults);

			BOEQuickSearchModelView quickSearchParams = new BOEQuickSearchModelView();
			SearchResultsModelView results = sut.QuickSearchForBOEs(ws1, boe1.Id, quickSearchParams);
			// Threshold should be reached, even though only 2 results are returned (foreign user does not have access to res1 - WorkspaceID=1).
			Assert.AreNotEqual(results.SearchResultsMessage, string.Empty);
			Assert.AreEqual(2, results.BOEResults.Count);
		}

		#endregion


		#region BOEAdvancedSearchModelViewPopulateCompanySpecificProperties Tests

		[TestMethod]
		public void BOEAdvancedSearchModelViewPopulateCompanySpecificPropertiesISGS()
		{
			BOEControllerLogic sut = this.CreateSystem();
			BOEAdvancedSearchModelView theModel = new BOEAdvancedSearchModelView();
			BOEAdvancedSearchModelView theModelNull = null;

			sut.PopulateCompanySpecificProperties(theModel);
			sut.PopulateCompanySpecificProperties(theModelNull);

			Assert.AreEqual(CommonConstants.LABEL_TEXT_LEAD_PRICER_SSC, theModel.LabelLeadPricer, "The LabelLeadPricer property was not set.");
			Assert.IsTrue(theModel.ShowRFP, "The ShowRFP property was not set.");
			Assert.IsNull(theModelNull);
		}

		[TestMethod]
		public void BOEAdvancedSearchModelViewPopulateCompanySpecificPropertiesSSC()
		{
			BOEControllerLogic sut = this.CreateSystemSpaceSystems();
			BOEAdvancedSearchModelView theModel = new BOEAdvancedSearchModelView();
			BOEAdvancedSearchModelView theModelNull = null;

			sut.PopulateCompanySpecificProperties(theModel);
			sut.PopulateCompanySpecificProperties(theModelNull);

			Assert.AreEqual(CommonConstants.LABEL_TEXT_LEAD_PRICER_SSC, theModel.LabelLeadPricer, "The LabelLeadPricer property was not set.");
			Assert.IsFalse(theModel.ShowRFP, "The ShowRFP property was not set.");
			Assert.IsNull(theModelNull);
		}

		[TestMethod]
		public void BOEAdvancedSearchModelViewPopulateCompanySpecificPropertiesMST()
		{
			BOEControllerLogic sut = this.CreateSystemMST();
			BOEAdvancedSearchModelView theModel = new BOEAdvancedSearchModelView();
			BOEAdvancedSearchModelView theModelNull = null;

			sut.PopulateCompanySpecificProperties(theModel);
			sut.PopulateCompanySpecificProperties(theModelNull);

			Assert.AreEqual(CommonConstants.LABEL_TEXT_LEAD_PRICER_SSC, theModel.LabelLeadPricer, "The LabelLeadPricer property was not set.");
			Assert.IsTrue(theModel.ShowRFP, "The ShowRFP property was not set.");
			Assert.IsNull(theModelNull);
		}

		#endregion BOEAdvancedSearchModelViewPopulateCompanySpecificProperties Tests

		[TestMethod]
		public void ReOrderTaskElementTest()
		{
			Mock<IRetriever> retriever = new Mock<IRetriever>();
			Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
			Mock<IPermissionsDTODataLoader> permissionsLoader = new Mock<IPermissionsDTODataLoader>();
			Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

			FullWorkspace ws = new FullWorkspace()
			{
				Id = 1
			};

			retriever.Setup(x => x.GetFullWorkspaceById(It.IsAny<int>())).Returns(ws);

			#region set up boetask elements
			BoeTaskElementDTO task1 = new BoeTaskElementDTO()
			{
				TaskTitle = "title1",
				BOETaskElementOrder = 0,
				Id = 1
			};
			BoeTaskElementDTO task2 = new BoeTaskElementDTO()
			{
				TaskTitle = "title2",
				BOETaskElementOrder = 0,
				Id = 2
			};
			BoeTaskElementDTO task3 = new BoeTaskElementDTO()
			{
				TaskTitle = "title3",
				BOETaskElementOrder = 0,
				Id = 3
			};
			BoeTaskElementDTO task4 = new BoeTaskElementDTO()
			{
				TaskTitle = "title4",
				BOETaskElementOrder = 0,
				Id = 4
			};
			BoeTaskElementDTO task5 = new BoeTaskElementDTO()
			{
				TaskTitle = "title5",
				BOETaskElementOrder = 0,
				Id = 5
			};

			//set up boe collection that exist in our DB

			ICollection<BoeTaskElementDTO> BoeTaskElementCollection = new Collection<BoeTaskElementDTO>();
			BoeTaskElementCollection.Add(task1);
			BoeTaskElementCollection.Add(task2);
			BoeTaskElementCollection.Add(task3);
			BoeTaskElementCollection.Add(task4);
			BoeTaskElementCollection.Add(task5);
			#endregion
			#region setup updated task
			BoeTaskElementDTO utask1 = new BoeTaskElementDTO()
			{
				TaskTitle = "title1",
				BOETaskElementOrder = 4,
				Updateable = UpdateType.Upsert,
				Id = 1
			};
			BoeTaskElementDTO utask2 = new BoeTaskElementDTO()
			{
				TaskTitle = "title2",
				BOETaskElementOrder = 3,
				Updateable = UpdateType.Upsert,
				Id = 2
			};
			BoeTaskElementDTO utask3 = new BoeTaskElementDTO()
			{
				TaskTitle = "title3",
				BOETaskElementOrder = 2,
				Updateable = UpdateType.Upsert,
				Id = 3
			};
			BoeTaskElementDTO utask4 = new BoeTaskElementDTO()
			{
				TaskTitle = "title4",
				BOETaskElementOrder = 1,
				Updateable = UpdateType.Upsert,
				Id = 4
			};
			BoeTaskElementDTO utask5 = new BoeTaskElementDTO()
			{
				TaskTitle = "title5",
				BOETaskElementOrder = 0,
				Id = 5,
				Updateable = UpdateType.Upsert
			};

			ICollection<BoeTaskElementDTO> UpdatedTaskElementCollection = new Collection<BoeTaskElementDTO>();

			UpdatedTaskElementCollection.Add(utask1);
			UpdatedTaskElementCollection.Add(utask2);
			UpdatedTaskElementCollection.Add(utask3);
			UpdatedTaskElementCollection.Add(utask4);
			UpdatedTaskElementCollection.Add(utask5);
			#endregion
			#region order from user
			TaskElementOrder order1 = new TaskElementOrder()
			{
				TaskID = task1.Id,
				ListOrder = 4
			};
			TaskElementOrder order2 = new TaskElementOrder()
			{
				TaskID = task2.Id,
				ListOrder = 3
			};
			TaskElementOrder order3 = new TaskElementOrder()
			{
				TaskID = task3.Id,
				ListOrder = 2
			};
			TaskElementOrder order4 = new TaskElementOrder()
			{
				TaskID = task4.Id,
				ListOrder = 1
			};
			TaskElementOrder order5 = new TaskElementOrder()
			{
				TaskID = task5.Id,
				ListOrder = 0
			};

			//set up the user input.
			TaskElementOrderCollection UserTaskElementCollection = new TaskElementOrderCollection();
			UserTaskElementCollection.BOETaskElements = new Collection<TaskElementOrder>();
			UserTaskElementCollection.BOETaskElements.Add(order1);
			UserTaskElementCollection.BOETaskElements.Add(order2);
			UserTaskElementCollection.BOETaskElements.Add(order3);
			UserTaskElementCollection.BOETaskElements.Add(order4);
			UserTaskElementCollection.BOETaskElements.Add(order5);
			#endregion

			FullBoe boeObject = new FullBoe()
			{
				Id = 1
			};

			_taskElementMediator.Setup(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), ws));
			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boeObject.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(BoeTaskElementCollection);

			BOEControllerLogic sut = new BOEControllerLogic(_boeSummary.Object,
				_userLoader.Object, _ADUtils.Object, _permissionsLoader.Object, Factory.Object, _boeExporter.Object,
				_boeCustomExporter.Object, _genBOEControllerLogic.Object, _boeMediator.Object, _validationHelper.Object, _boeCommentDTODataLoader.Object, _emailer.Object,
				_taskElementMediator.Object, _workspaceVariableDTODataLoader.Object, _boeStateMachine.Object, _variableSelectBOEtoSumCalculation.Object, _boeLaborControllerLogic.Object,
				_validateBOE.Object, _securityInformation.Object, _boeSearchLoader.Object, _securityAccess.Object, _boeTaskElementRecalculation.Object,
				_boeImporter.Object, _variableCircularReferenceChecker.Object, _conflictBOE.Object, _nestedWBSUtilities.Object, null, this._zoneTravelRatesFeesDataLoader.Object, null, this.moqTypeLoader.Object, boeApproverResponseLoader.Object, iesSapClient.Object, tokenService);

			sut.ReOrderTaskElementOrder(ws, boeObject, UserTaskElementCollection);

			_taskElementMediator.Verify(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), ws), Times.Once());

			//make sure the boe taskelements are updated correctly. 
			for (int x = 0; x < UpdatedTaskElementCollection.Count; x++)
			{
				Assert.IsTrue(UpdatedTaskElementCollection.ElementAt(x).BOETaskElementOrder == BoeTaskElementCollection.ElementAt(x).BOETaskElementOrder);
				Assert.IsTrue(UpdatedTaskElementCollection.ElementAt(x).Updateable == BoeTaskElementCollection.ElementAt(x).Updateable);
			}

		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ReOrderTaskElement_NullTest()
		{
			Mock<IRetriever> retriever = new Mock<IRetriever>();
			Mock<ICommonDataMapper> commonDataMapper = new Mock<ICommonDataMapper>();
			Mock<IPermissionsDTODataLoader> permissionsLoader = new Mock<IPermissionsDTODataLoader>();
			Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

			FullWorkspace ws = new FullWorkspace()
			{
				Id = 1
			};

			#region set up boetask elements
			BoeTaskElementDTO task1 = new BoeTaskElementDTO()
			{
				TaskTitle = "title1",
				BOETaskElementOrder = 0,
				Id = 1
			};
			BoeTaskElementDTO task2 = new BoeTaskElementDTO()
			{
				TaskTitle = "title2",
				BOETaskElementOrder = 0,
				Id = 2
			};
			BoeTaskElementDTO task3 = new BoeTaskElementDTO()
			{
				TaskTitle = "title3",
				BOETaskElementOrder = 0,
				Id = 3
			};
			BoeTaskElementDTO task4 = new BoeTaskElementDTO()
			{
				TaskTitle = "title4",
				BOETaskElementOrder = 0,
				Id = 4
			};
			BoeTaskElementDTO task5 = new BoeTaskElementDTO()
			{
				TaskTitle = "title5",
				BOETaskElementOrder = 0,
				Id = 5
			};

			//set up boe collection that exist in our DB

			ICollection<BoeTaskElementDTO> BoeTaskElementCollection = new Collection<BoeTaskElementDTO>();
			BoeTaskElementCollection.Add(task1);
			BoeTaskElementCollection.Add(task2);
			BoeTaskElementCollection.Add(task3);
			BoeTaskElementCollection.Add(task4);
			BoeTaskElementCollection.Add(task5);
			#endregion
			#region order from user
			TaskElementOrder order1 = new TaskElementOrder()
			{
				TaskID = task1.Id,
				ListOrder = 4
			};
			TaskElementOrder order2 = new TaskElementOrder()
			{
				TaskID = task2.Id,
				ListOrder = 3
			};
			TaskElementOrder order3 = new TaskElementOrder()
			{
				TaskID = task3.Id,
				ListOrder = 2
			};
			TaskElementOrder order4 = new TaskElementOrder()
			{
				TaskID = task4.Id,
				ListOrder = 1
			};
			TaskElementOrder order5 = new TaskElementOrder()
			{
				TaskID = task5.Id,
				ListOrder = 0
			};

			//set up the user input.
			TaskElementOrderCollection UserTaskElementCollection = new TaskElementOrderCollection();
			UserTaskElementCollection.BOETaskElements = new Collection<TaskElementOrder>();
			UserTaskElementCollection.BOETaskElements.Add(order1);
			UserTaskElementCollection.BOETaskElements.Add(order2);
			UserTaskElementCollection.BOETaskElements.Add(order3);
			UserTaskElementCollection.BOETaskElements.Add(order4);
			UserTaskElementCollection.BOETaskElements.Add(order5);
			#endregion

			FullBoe boeObject = null;

			_taskElementMediator.Setup(x => x.MediatedBulkSaveTaskElements(It.IsAny<ICollection<BoeTaskElementDTO>>(), ws));

			BOEControllerLogic sut = new BOEControllerLogic(_boeSummary.Object,
				_userLoader.Object, _ADUtils.Object, _permissionsLoader.Object, Factory.Object, _boeExporter.Object,
				_boeCustomExporter.Object, _genBOEControllerLogic.Object, _boeMediator.Object, _validationHelper.Object, _boeCommentDTODataLoader.Object, _emailer.Object,
				_taskElementMediator.Object, _workspaceVariableDTODataLoader.Object, _boeStateMachine.Object, _variableSelectBOEtoSumCalculation.Object, _boeLaborControllerLogic.Object,
				_validateBOE.Object, _securityInformation.Object, _boeSearchLoader.Object, _securityAccess.Object, _boeTaskElementRecalculation.Object,
				_boeImporter.Object, _variableCircularReferenceChecker.Object, _conflictBOE.Object, _nestedWBSUtilities.Object, null, this._zoneTravelRatesFeesDataLoader.Object, null, this.moqTypeLoader.Object, boeApproverResponseLoader.Object, iesSapClient.Object, tokenService);

			sut.ReOrderTaskElementOrder(ws, boeObject, UserTaskElementCollection);


		}

		/// <summary>
		/// Verifies GetDuplicateTaskModelView will return correct data for Labor tasks
		/// </summary>
		[TestMethod]
		public void GetDuplicateTaskModelViewWithTasksLaborTest()
		{
			BOEControllerLogic sut = this.CreateSystem();

			Mock<IRetriever> retriever = new Mock<IRetriever>();
			Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), new Mock<ICommonDataMapper>().Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), new Mock<IPermissionsDTODataLoader>().Object);
			retriever.Setup(x => x.GetFullWorkspaceById(It.IsAny<int>())).Returns(new FullWorkspace());

			FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 4, 1) };

			BoeTaskElementDTO task1 = new BoeTaskElementDTO()
			{
				TaskTitle = "title1",
				Id = 1,
				BOETaskID = "123",
				TaskElementType = TaskElementType.Labor
			};
			BoeTaskElementDTO task2 = new BoeTaskElementDTO()
			{
				TaskTitle = "title2",
				Id = 2,
				BOETaskID = "456",
				TaskElementType = TaskElementType.Labor
			};

			TaskElementDuplicateFormModelView dupForm1 = new TaskElementDuplicateFormModelView
			{
				BOETaskID = "123",
				DuplicateCount = 0,
				TaskID = 1,
				TaskTitle = "title1"
			};
			TaskElementDuplicateFormModelView dupForm2 = new TaskElementDuplicateFormModelView
			{
				BOETaskID = "456",
				DuplicateCount = 0,
				TaskID = 2,
				TaskTitle = "title2"
			};

			ICollection<BoeTaskElementDTO> taskCollection = new List<BoeTaskElementDTO>();
			taskCollection.Add(task1);
			taskCollection.Add(task2);

			Collection<TaskElementDuplicateFormModelView> dupFormCollection = new Collection<TaskElementDuplicateFormModelView>();
			dupFormCollection.Add(dupForm1);
			dupFormCollection.Add(dupForm2);

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(taskCollection);

			//Act
			TaskElementDuplicateFormCollection theModelView = sut.GetDuplicateTaskModelView(boe, TaskType.Labor);

			//Assert: make sure data matches up
			Assert.AreEqual(boe, theModelView.Boe);
			Assert.AreEqual(TaskType.Labor, theModelView.TaskType);
			Assert.AreEqual(2, theModelView.DuplicateTaskRequests.Count);

			//make sure the collection of  TaskElementDuplicateFormModelView are correct
			for (int i = 0; i < dupFormCollection.Count; i++)
			{
				Assert.IsTrue(dupFormCollection.ElementAt(i).BOETaskID == theModelView.DuplicateTaskRequests.ElementAt(i).BOETaskID);
				Assert.IsTrue(dupFormCollection.ElementAt(i).DuplicateCount == theModelView.DuplicateTaskRequests.ElementAt(i).DuplicateCount);
				Assert.IsTrue(dupFormCollection.ElementAt(i).TaskID == theModelView.DuplicateTaskRequests.ElementAt(i).TaskID);
				Assert.IsTrue(dupFormCollection.ElementAt(i).TaskTitle == theModelView.DuplicateTaskRequests.ElementAt(i).TaskTitle);
			}
		}

		/// <summary>
		/// Verifies GetDuplicateTaskModelView will return correct data for Travel tasks
		/// </summary>
		[TestMethod]
		public void GetDuplicateTaskModelViewWithTasksTravelTest()
		{
			BOEControllerLogic sut = this.CreateSystem();

			Mock<IRetriever> retriever = new Mock<IRetriever>();
			Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

			FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 4, 1) };

			TravelDTO task1 = new TravelDTO()
			{
				TaskTitle = "title1",
				Id = 1,
				TaskID = "123"
			};
			TravelDTO task2 = new TravelDTO()
			{
				TaskTitle = "title2",
				Id = 2,
				TaskID = "456"
			};

			ICollection<TravelDTO> taskCollection = new List<TravelDTO>();

			taskCollection.Add(task1);
			taskCollection.Add(task2);

			TaskElementDuplicateFormModelView dupForm1 = new TaskElementDuplicateFormModelView
			{
				BOETaskID = "123",
				DuplicateCount = 0,
				TaskID = 1,
				TaskTitle = "title1"
			};
			TaskElementDuplicateFormModelView dupForm2 = new TaskElementDuplicateFormModelView
			{
				BOETaskID = "456",
				DuplicateCount = 0,
				TaskID = 2,
				TaskTitle = "title2"
			};

			Collection<TaskElementDuplicateFormModelView> dupFormCollection = new Collection<TaskElementDuplicateFormModelView>();
			dupFormCollection.Add(dupForm1);
			dupFormCollection.Add(dupForm2);

			retriever.Setup(x => x.GetTravelCollectionByBoeID(boe.Id, false)).Returns(taskCollection);

			//Act
			TaskElementDuplicateFormCollection theModelView = sut.GetDuplicateTaskModelView(boe, TaskType.Travel);

			//Assert: make sure data matches up
			Assert.AreEqual(boe, theModelView.Boe);
			Assert.AreEqual(TaskType.Travel, theModelView.TaskType);
			Assert.AreEqual(2, theModelView.DuplicateTaskRequests.Count);

			//make sure the collection of  TaskElementDuplicateFormModelView are correct
			for (int i = 0; i < dupFormCollection.Count; i++)
			{
				Assert.IsTrue(dupFormCollection.ElementAt(i).BOETaskID == theModelView.DuplicateTaskRequests.ElementAt(i).BOETaskID);
				Assert.IsTrue(dupFormCollection.ElementAt(i).DuplicateCount == theModelView.DuplicateTaskRequests.ElementAt(i).DuplicateCount);
				Assert.IsTrue(dupFormCollection.ElementAt(i).TaskID == theModelView.DuplicateTaskRequests.ElementAt(i).TaskID);
				Assert.IsTrue(dupFormCollection.ElementAt(i).TaskTitle == theModelView.DuplicateTaskRequests.ElementAt(i).TaskTitle);
			}
		}

		/// <summary>
		/// Verifies GetDuplicateTaskModelView will return correct data for ODC tasks
		/// </summary>
		[TestMethod]
		public void GetDuplicateTaskModelViewWithTasksOdcTest()
		{
			BOEControllerLogic sut = this.CreateSystem();

			Mock<IRetriever> retriever = new Mock<IRetriever>();
			Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);

			FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 4, 1) };

			OtherDirectCostDTO task1 = new OtherDirectCostDTO()
			{
				TaskTitle = "title1",
				Id = 1,
				TaskID = "123"
			};
			OtherDirectCostDTO task2 = new OtherDirectCostDTO()
			{
				TaskTitle = "title2",
				Id = 2,
				TaskID = "456"
			};

			ICollection<OtherDirectCostDTO> taskCollection = new List<OtherDirectCostDTO>();
			taskCollection.Add(task1);
			taskCollection.Add(task2);

			TaskElementDuplicateFormModelView dupForm1 = new TaskElementDuplicateFormModelView
			{
				BOETaskID = "123",
				DuplicateCount = 0,
				TaskID = 1,
				TaskTitle = "title1"
			};
			TaskElementDuplicateFormModelView dupForm2 = new TaskElementDuplicateFormModelView
			{
				BOETaskID = "456",
				DuplicateCount = 0,
				TaskID = 2,
				TaskTitle = "title2"
			};

			Collection<TaskElementDuplicateFormModelView> dupFormCollection = new Collection<TaskElementDuplicateFormModelView>();
			dupFormCollection.Add(dupForm1);
			dupFormCollection.Add(dupForm2);

			retriever.Setup(x => x.GetOdcCollectionByBoeIds(new Collection<int>() { boe.Id }, false)).Returns(taskCollection);

			//Act
			TaskElementDuplicateFormCollection theModelView = sut.GetDuplicateTaskModelView(boe, TaskType.ODC);

			//Assert
			Assert.AreEqual(boe, theModelView.Boe);
			Assert.AreEqual(TaskType.ODC, theModelView.TaskType);
			Assert.AreEqual(2, theModelView.DuplicateTaskRequests.Count);

			//make sure the collection of  TaskElementDuplicateFormModelView are correct
			for (int i = 0; i < dupFormCollection.Count; i++)
			{
				Assert.AreEqual(dupFormCollection.ElementAt(i).BOETaskID, theModelView.DuplicateTaskRequests.ElementAt(i).BOETaskID);
				Assert.IsTrue(dupFormCollection.ElementAt(i).DuplicateCount == theModelView.DuplicateTaskRequests.ElementAt(i).DuplicateCount);
				Assert.IsTrue(dupFormCollection.ElementAt(i).TaskID == theModelView.DuplicateTaskRequests.ElementAt(i).TaskID);
				Assert.IsTrue(dupFormCollection.ElementAt(i).TaskTitle == theModelView.DuplicateTaskRequests.ElementAt(i).TaskTitle);
			}
		}

		/// <summary>
		/// Verifies GetDuplicateTaskModelView will return correct data when there are no tasks to return
		/// </summary>
		[TestMethod]
		public void GetDuplicateTaskModelViewWithNoTasksLaborTest()
		{
			BOEControllerLogic sut = this.CreateSystem();

			Mock<IRetriever> retriever = new Mock<IRetriever>();
			Mock<IFullObjectFactory> factory = new Mock<IFullObjectFactory>();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), new Mock<ICommonDataMapper>().Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), new Mock<IPermissionsDTODataLoader>().Object);
			retriever.Setup(x => x.GetFullWorkspaceById(It.IsAny<int>())).Returns(new FullWorkspace());

			FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 4, 1) };

			ICollection<BoeTaskElementDTO> taskCollection = new List<BoeTaskElementDTO>();

			retriever.Setup(x => x.GetBoeTaskElementCollectionByBoeId(boe.Id, false, It.IsAny<int>(), It.IsAny<int>())).Returns(taskCollection);

			//Act
			TaskElementDuplicateFormCollection theModelView = sut.GetDuplicateTaskModelView(boe, TaskType.Labor);

			//Assert
			Assert.AreEqual(boe, theModelView.Boe);
			Assert.AreEqual(TaskType.Labor, theModelView.TaskType);
			Assert.AreEqual(0, theModelView.DuplicateTaskRequests.Count);
		}

		/// <summary>
		/// Verifies GetDuplicateTaskModelVew will throw null exception when null boe is passed
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetDuplicateTaskModelView_NullTest()
		{
			BOEControllerLogic sut = this.CreateSystem();

			FullBoe boe = null;

			//Act
			sut.GetDuplicateTaskModelView(boe, TaskType.Labor);

			//Assert: (Test expects null exception)
		}

		/// <summary>
		/// Creates a mock factory mock object for validation
		/// </summary>
		/// <returns></returns>
		public Mock<ValidationFactory> CreateValidationFactoryMock()
		{
			Mock<Validator> workspaceUniqueNameValidator = new Mock<Validator>();
			Mock<Validator> workspaceUniqueShortnameValidator = new Mock<Validator>();
			Mock<Validator> workspaceCostVolumeLeadNotGroupValidator = new Mock<Validator>();
			Mock<Validator> boeTaskIdUniqueValidator = new Mock<Validator>();
			Mock<Validator> boeDateRangeValidator = new Mock<Validator>();
			Mock<Validator> wbsUniqueNumberValidator = new Mock<Validator>();
			Mock<Validator> resourceUniqueIDValidator = new Mock<Validator>();
			Mock<Validator> performingOrgUniqueIDValidator = new Mock<Validator>();
			Mock<Validator> wbsRenumberValidator = new Mock<Validator>();
			Mock<Validator> boeWBSMoveValidator = new Mock<Validator>();
			Mock<Validator> boeCLINMoveValidator = new Mock<Validator>();
			Mock<Validator> boeMaterialExistsforWbsValidator = new Mock<Validator>();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();
			Mock<Validator> resourceUniqueDescValidator = new Mock<Validator>();
			Mock<Validator> workspaceCostVolumeLeadNotSubcontractorValidator = new Mock<Validator>();



			Mock<ValidationFactory> validationFactoyMock = new Mock<ValidationFactory>(workspaceUniqueNameValidator.Object,
				 workspaceUniqueShortnameValidator.Object,
				 workspaceCostVolumeLeadNotGroupValidator.Object,
				 boeTaskIdUniqueValidator.Object,
				 boeDateRangeValidator.Object,
				 wbsUniqueNumberValidator.Object,
				 resourceUniqueIDValidator.Object,
				 performingOrgUniqueIDValidator.Object,
				 wbsRenumberValidator.Object,
				 boeWBSMoveValidator.Object,
				 boeCLINMoveValidator.Object,
				 boeMaterialExistsforWbsValidator.Object,
				 boeMaterialElementExistsValidator.Object,
				 resourceUniqueDescValidator.Object,
				 workspaceCostVolumeLeadNotSubcontractorValidator.Object);
			return validationFactoyMock;
		}

		[TestMethod]
		/// <summary>
		/// Test WBS CircularReference
		/// </summary>
		public void ValidateSaveManageBOEWBSCircularReference_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOEWBSMoveValidator> validatorWBSMove = new Mock<BOEWBSMoveValidator>(circularReference.Object, Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEWBSMoveValidator), validatorWBSMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { WbsID = 1, BoeID = 1, State = BOEState.Draft, isMaterial = false, IsMultiClinWbs = false };
			boeMV.Authors.Add(1);
			boeMV.Approvers.Add(2);
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };
			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user2, user3, user4 };
			UserDTO approver = new UserDTO() { NTID = user2.NTID, UserID = user2.Id };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, WBSID = 2, Id = 1, State = BOEState.Draft, AuthorIDs = new Collection<int>() { user1.Id } };
			FullWbs wbs1 = new FullWbs() { Id = 2 };
			FullWbs multi = new FullWbs() { Id = 2, WbsTitle = "MULTI", WbsNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Draft}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};

			string errorMessage = string.Empty;
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.Draft, BOEState.Draft, out errorMessage)).Returns(true);
			Factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(multi);

			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>());
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, multi });
			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			validatorWBSMove.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, object>>>())).Returns(new Collection<string>() { "The selected WBS # would create a circular reference." });
			_userLoader.Setup(x => x.GetUserByID(user2.Id)).Returns(approver);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BOEWBSMove)).Returns(validatorWBSMove.Object);
			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);



			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();
			ValidationMessage message = new ValidationMessage("WBS", "The selected WBS # would create a circular reference.");
			ExpectedErrors.Add(message);

			Assert.IsTrue(ValidationErrors.Count == 1, "Only expecting one error message");
			Assert.IsTrue(ExpectedErrors.FirstOrDefault().ValidationIssue == ValidationErrors.FirstOrDefault().ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors.FirstOrDefault().FieldName == ValidationErrors.FirstOrDefault().FieldName, "Different Field threw a message");


		}

		[TestMethod]
		/// <summary>
		/// Test Clin CircularReference
		/// </summary>
		public void ValidateSaveManageBOEClinCircularReference_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOECLINMoveValidator> validatorClinMove = new Mock<BOECLINMoveValidator>(circularReference.Object, Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOECLINMoveValidator), validatorClinMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { ClinID = 1, BoeID = 1, State = BOEState.Draft, isMaterial = false, IsMultiClinWbs = false };
			boeMV.Authors.Add(1);
			boeMV.Approvers.Add(2);
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };
			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user2, user3, user4 };
			UserDTO approver = new UserDTO() { NTID = user2.NTID, UserID = user2.Id };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, CLINID = 2, Id = 1, State = BOEState.Draft, AuthorIDs = new Collection<int>() { user1.Id } };
			FullClin clin1 = new FullClin() { Id = 2 };
			FullClin multi = new FullClin() { Id = 2, ClinTitle = "MULTI", ClinNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Draft}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};
			string errorMessage = string.Empty;
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.Draft, BOEState.Draft, out errorMessage)).Returns(true);
			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, multi });
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { });
			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			validatorClinMove.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, object>>>())).Returns(new Collection<string>() { "The selected Clin # would create a circular reference." });
			_userLoader.Setup(x => x.GetUserByID(user2.Id)).Returns(approver);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);

			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BOECLINMove)).Returns(validatorClinMove.Object);
			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);



			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();
			ValidationMessage message = new ValidationMessage("CLIN", "The selected Clin # would create a circular reference.");
			ExpectedErrors.Add(message);

			Assert.IsTrue(ValidationErrors.Count == 1, "Only expecting one error message");
			Assert.IsTrue(ExpectedErrors.FirstOrDefault().ValidationIssue == ValidationErrors.FirstOrDefault().ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors.FirstOrDefault().FieldName == ValidationErrors.FirstOrDefault().FieldName, "Different Field threw a message");


		}

		[TestMethod]
		/// <summary>
		/// Test Material validation
		/// </summary>
		public void ValidateSaveManageBOEMaterial_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BoeLaborCostElementExistsValidator> validatorCost = new Mock<BoeLaborCostElementExistsValidator>(Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BoeLaborCostElementExistsValidator), validatorCost.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { ClinID = 1, WbsID = 1, BoeID = 1, State = BOEState.Draft, isMaterial = true, IsMultiClinWbs = false };
			boeMV.Authors.Add(1);
			boeMV.Approvers.Add(2);
			boeMV.SubcontractorAuthors.Add(3);
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };
			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			PermissionsDTO user5 = new PermissionsDTO() { BOEId = 1, Role = Role.SubcontractorAuthor, NTID = "user5", Id = 5, ETIUserId = 5 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user2, user3, user4, user5 };
			UserDTO approver = new UserDTO() { NTID = user2.NTID, UserID = user2.Id };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, CLINID = 1, WBSID = 1, Id = 1, State = BOEState.Draft, AuthorIDs = new Collection<int>() { user1.Id } };
			FullClin clin1 = new FullClin() { Id = 1 };
			FullWbs wbs1 = new FullWbs() { Id = 1 };
			FullWbs multiwbs = new FullWbs() { Id = 2, WbsNumber = "MULTI", WbsTitle = "MULTI" };

			FullClin multiclin = new FullClin() { Id = 2, ClinTitle = "MULTI", ClinNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Draft}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};
			string errorMessage = string.Empty;
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.Draft, BOEState.Draft, out errorMessage)).Returns(true);
			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, multiclin });
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, multiwbs });

			Factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(multiwbs);

			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			validatorCost.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { "The BOE is not a validate Material" });
			_userLoader.Setup(x => x.GetUserByID(user2.Id)).Returns(approver);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);

			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeLaborCostElementExists)).Returns(validatorCost.Object);
			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);

			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();

			ValidationMessage message = new ValidationMessage(null, "Subcontractor users are restricted from being assigned to Material BOEs.");
			ExpectedErrors.Add(message);
			ValidationMessage message1 = new ValidationMessage("Material", "The BOE is not a validate Material");
			ExpectedErrors.Add(message1);


			Assert.AreEqual(2, ValidationErrors.Count);
			Assert.IsTrue(ExpectedErrors[0].ValidationIssue == ValidationErrors[0].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[0].FieldName == ValidationErrors[0].FieldName, "Different Field threw a message");
			Assert.IsTrue(ExpectedErrors[1].ValidationIssue == ValidationErrors[1].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[1].FieldName == ValidationErrors[1].FieldName, "Different Field threw a message");

		}

		[TestMethod]
		/// <summary>
		/// Test Material -> non material with task elements validation
		/// </summary>
		public void ValidateSaveManageBOEMaterialExist_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BoeLaborCostElementExistsValidator> validatorCost = new Mock<BoeLaborCostElementExistsValidator>(Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BoeLaborCostElementExistsValidator), validatorCost.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { ClinID = 1, WbsID = 1, BoeID = 1, State = BOEState.Draft, isMaterial = false, IsMultiClinWbs = false };
			boeMV.Authors.Add(1);
			boeMV.Approvers.Add(2);
			boeMV.SubcontractorAuthors.Add(3);
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };
			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user2, user3, user4 };
			UserDTO approver = new UserDTO() { NTID = user2.NTID, UserID = user2.Id };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, CLINID = 1, WBSID = 1, Id = 1, State = BOEState.Draft, AuthorIDs = new Collection<int>() { user1.Id } };
			FullClin clin1 = new FullClin() { Id = 1 };
			FullWbs wbs1 = new FullWbs() { Id = 1 };
			FullWbs multiwbs = new FullWbs() { Id = 2, WbsNumber = "MULTI", WbsTitle = "MULTI" };

			FullClin multiclin = new FullClin() { Id = 2, ClinTitle = "MULTI", ClinNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Draft}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};
			string errorMessage = string.Empty;
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.Draft, BOEState.Draft, out errorMessage)).Returns(true);
			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, multiclin });
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, multiwbs });
			Factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(multiwbs);


			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			_userLoader.Setup(x => x.GetUserByID(user2.Id)).Returns(approver);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { "BOE contaitns Material Task Elements" });
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);

			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);

			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();


			ValidationMessage message = new ValidationMessage("Material", "BOE contaitns Material Task Elements");
			ExpectedErrors.Add(message);


			Assert.IsTrue(ValidationErrors.Count == 1, "Only expecting two error message");
			Assert.IsTrue(ExpectedErrors[0].ValidationIssue == ValidationErrors[0].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[0].FieldName == ValidationErrors[0].FieldName, "Different Field threw a message");


		}

		[TestMethod]
		/// <summary>
		/// Test Multi Wbs/Clin
		/// </summary>
		public void ValidateSaveManageBOEMultiBOE_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BoeLaborCostElementExistsValidator> validatorCost = new Mock<BoeLaborCostElementExistsValidator>(Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BoeLaborCostElementExistsValidator), validatorCost.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { ClinID = 1, WbsID = 1, BoeID = 1, State = BOEState.Draft, isMaterial = true, IsMultiClinWbs = true };
			boeMV.Authors.Add(1);
			boeMV.Approvers.Add(2);
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };
			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user2, user3, user4 };
			UserDTO approver = new UserDTO() { NTID = user2.NTID, UserID = user2.Id };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, CLINID = 1, WBSID = 1, Id = 1, State = BOEState.Draft, AuthorIDs = new Collection<int>() { user1.Id } };
			FullClin clin1 = new FullClin() { Id = 1 };
			FullWbs wbs1 = new FullWbs() { Id = 1 };
			FullWbs multiwbs = new FullWbs() { Id = 2, WbsNumber = "MULTI", WbsTitle = "MULTI" };

			FullClin multiclin = new FullClin() { Id = 2, ClinTitle = "MULTI", ClinNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Draft}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};
			string errorMessage = string.Empty;
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.Draft, BOEState.Draft, out errorMessage)).Returns(true);
			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, multiclin });
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, multiwbs });

			validatorCost.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			Factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(multiwbs);

			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			_userLoader.Setup(x => x.GetUserByID(user2.Id)).Returns(approver);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeLaborCostElementExists)).Returns(validatorCost.Object);

			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);

			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();


			ValidationMessage message1 = new ValidationMessage("MultiBOE", ValidationConstants.MULTI_BOE_MATERIAL_BOE);
			ValidationMessage message2 = new ValidationMessage("MultiBOE", ValidationConstants.MULTI_BOE_NEEDS_CLIN);
			ValidationMessage message3 = new ValidationMessage("MultiBOE", ValidationConstants.MULTI_BOE_NEEDS_WBS);
			ExpectedErrors.Add(message1);
			ExpectedErrors.Add(message2);
			ExpectedErrors.Add(message3);


			Assert.AreEqual(3, ValidationErrors.Count);
			Assert.IsTrue(ExpectedErrors[0].ValidationIssue == ValidationErrors[0].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[0].FieldName == ValidationErrors[0].FieldName, "Different Field threw a message");
			Assert.IsTrue(ExpectedErrors[1].ValidationIssue == ValidationErrors[1].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[1].FieldName == ValidationErrors[1].FieldName, "Different Field threw a message");
			Assert.IsTrue(ExpectedErrors[2].ValidationIssue == ValidationErrors[2].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[2].FieldName == ValidationErrors[2].FieldName, "Different Field threw a message");
		}

		[TestMethod]
		/// <summary>
		/// Test Multi Wbs/Clin
		/// </summary>
		public void ValidateSaveManageBOEMultiBOEWithWBSClin_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BoeLaborCostElementExistsValidator> validatorCost = new Mock<BoeLaborCostElementExistsValidator>(Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BoeLaborCostElementExistsValidator), validatorCost.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { ClinID = 2, WbsID = 2, BoeID = 1, State = BOEState.Draft, isMaterial = true, IsMultiClinWbs = true };
			boeMV.Authors.Add(1);
			boeMV.Approvers.Add(2);
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };
			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user2, user3, user4 };
			UserDTO approver = new UserDTO() { NTID = user2.NTID, UserID = user2.Id };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, CLINID = 2, WBSID = 2, Id = 1, State = BOEState.Draft, AuthorIDs = new Collection<int>() { user1.Id } };
			FullClin clin1 = new FullClin() { Id = 1 };
			FullWbs wbs1 = new FullWbs() { Id = 1 };
			FullWbs multiwbs = new FullWbs() { Id = 2, WbsNumber = "MULTI", WbsTitle = "MULTI" };

			FullClin multiclin = new FullClin() { Id = 2, ClinTitle = "MULTI", ClinNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Draft}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};
			string errorMessage = string.Empty;
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.Draft, BOEState.Draft, out errorMessage)).Returns(true);
			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, multiclin });
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, multiwbs });
			Factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(multiwbs);

			validatorCost.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });

			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			_userLoader.Setup(x => x.GetUserByID(user2.Id)).Returns(approver);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeLaborCostElementExists)).Returns(validatorCost.Object);

			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);

			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();


			ValidationMessage message = new ValidationMessage("MultiBOE", ValidationConstants.MULTI_BOE_MATERIAL_BOE);
			ExpectedErrors.Add(message);


			Assert.IsTrue(ValidationErrors.Count == 1, "Only expecting one error message");
			Assert.IsTrue(ExpectedErrors[0].ValidationIssue == ValidationErrors[0].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[0].FieldName == ValidationErrors[0].FieldName, "Different Field threw a message");
		}

		[TestMethod]
		/// <summary>
		/// Test Multi Wbs/Clin
		/// </summary>
		public void ValidateSaveManageNoMultiBOEWbsClin_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BoeLaborCostElementExistsValidator> validatorCost = new Mock<BoeLaborCostElementExistsValidator>(Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BoeLaborCostElementExistsValidator), validatorCost.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { ClinID = 2, WbsID = 2, BoeID = 1, State = BOEState.Draft, isMaterial = true, IsMultiClinWbs = false };
			boeMV.Authors.Add(1);
			boeMV.Approvers.Add(2);
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };
			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user2, user3, user4 };
			UserDTO approver = new UserDTO() { NTID = user2.NTID, UserID = user2.Id };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, CLINID = 2, WBSID = 2, Id = 1, State = BOEState.Draft, AuthorIDs = new Collection<int>() { user1.Id } };
			FullClin clin1 = new FullClin() { Id = 1 };
			FullWbs wbs1 = new FullWbs() { Id = 1 };
			FullWbs multiwbs = new FullWbs() { Id = 2, WbsNumber = "MULTI", WbsTitle = "MULTI" };

			FullClin multiclin = new FullClin() { Id = 2, ClinTitle = "MULTI", ClinNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Draft}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};
			string errorMessage = string.Empty;
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.Draft, BOEState.Draft, out errorMessage)).Returns(true);
			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, multiclin });
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, multiwbs });

			validatorCost.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			Factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(multiwbs);

			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			_userLoader.Setup(x => x.GetUserByID(user2.Id)).Returns(approver);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeLaborCostElementExists)).Returns(validatorCost.Object);

			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);

			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();


			ValidationMessage message = new ValidationMessage("MultiBOE", ValidationConstants.MULTI_BOE_CLIN_ASSIGNED);
			ValidationMessage message1 = new ValidationMessage("MultiBOE", ValidationConstants.MULTI_BOE_WBS_ASSIGNED);
			ExpectedErrors.Add(message);
			ExpectedErrors.Add(message1);


			Assert.IsTrue(ValidationErrors.Count == 2, "Only expecting two error message");
			Assert.IsTrue(ExpectedErrors[0].ValidationIssue == ValidationErrors[0].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[0].FieldName == ValidationErrors[0].FieldName, "Different Field threw a message");
			Assert.IsTrue(ExpectedErrors[1].ValidationIssue == ValidationErrors[1].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[1].FieldName == ValidationErrors[1].FieldName, "Different Field threw a message");
		}

		[TestMethod]
		/// <summary>
		/// Test ValidateAuthorApproverAssigned
		/// </summary>
		public void ValidateSaveManageAuthorApproverAssigned_Removed_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BoeLaborCostElementExistsValidator> validatorCost = new Mock<BoeLaborCostElementExistsValidator>(Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BoeLaborCostElementExistsValidator), validatorCost.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { ClinID = 1, WbsID = 1, BoeID = 1, State = BOEState.Draft, isMaterial = true, IsMultiClinWbs = false };
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };
			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user2, user3, user4 };
			UserDTO approver = new UserDTO() { NTID = user2.NTID, UserID = user2.Id };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, CLINID = 1, WBSID = 1, Id = 1, State = BOEState.Draft, AuthorIDs = new Collection<int>() { user1.Id } };
			FullClin clin1 = new FullClin() { Id = 1 };
			FullWbs wbs1 = new FullWbs() { Id = 1 };
			FullWbs multiwbs = new FullWbs() { Id = 2, WbsNumber = "MULTI", WbsTitle = "MULTI" };

			FullClin multiclin = new FullClin() { Id = 2, ClinTitle = "MULTI", ClinNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Draft}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};
			string errorMessage = string.Empty;
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.Draft, BOEState.Draft, out errorMessage)).Returns(true);
			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, multiclin });
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, multiwbs });

			validatorCost.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });

			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			_userLoader.Setup(x => x.GetUserByID(user2.Id)).Returns(approver);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeLaborCostElementExists)).Returns(validatorCost.Object);
			Factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(multiwbs);

			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);

			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();


			ValidationMessage message = new ValidationMessage("Author", "Author and Approver cannot be removed once they are assigned to a BOE. Please select an Author and Approver(s).");
			ExpectedErrors.Add(message);


			Assert.IsTrue(ValidationErrors.Count == 1, "Only expecting two error message");
			Assert.IsTrue(ExpectedErrors[0].ValidationIssue == ValidationErrors[0].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[0].FieldName == ValidationErrors[0].FieldName, "Different Field threw a message");

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestMethod]
		/// <summary>
		/// Test ValidateAuthorApproverAssigned
		/// </summary>
		public void ValidateSaveManageAuthorApproverReAssigned_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BoeLaborCostElementExistsValidator> validatorCost = new Mock<BoeLaborCostElementExistsValidator>(Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BoeLaborCostElementExistsValidator), validatorCost.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { ClinID = 1, WbsID = 1, BoeID = 1, State = BOEState.AwaitingApproval, isMaterial = false, IsMultiClinWbs = false };
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
			boeMV.Approvers.Add(100);
			boeMV.Authors.Add(101);
			boeMV.SubcontractorAuthors.Add(102);

			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };

			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user2, user3, user4 };
			UserDTO approver = new UserDTO() { NTID = "user15", UserID = 15 };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, CLINID = 1, WBSID = 1, Id = 1, State = BOEState.AwaitingApproval, AuthorIDs = new Collection<int>() { user1.Id } };
			FullClin clin1 = new FullClin() { Id = 1 };
			FullWbs wbs1 = new FullWbs() { Id = 1 };
			FullWbs multiwbs = new FullWbs() { Id = 2, WbsNumber = "MULTI", WbsTitle = "MULTI" };

			FullClin multiclin = new FullClin() { Id = 2, ClinTitle = "MULTI", ClinNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Approved}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};
			string errorMessage = string.Empty;
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.AwaitingApproval, BOEState.AwaitingApproval, out errorMessage)).Returns(true);
			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, multiclin });
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, multiwbs });

			validatorCost.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });

			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			_userLoader.Setup(x => x.GetUserByID(100)).Returns(approver);
			_userLoader.Setup(x => x.GetUserByID(2)).Returns(approver);
			_userLoader.Setup(x => x.GetByIds(new List<int>() { 2 })).Returns(new Collection<UserDTO>() { approver });
			Factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(multiwbs);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeLaborCostElementExists)).Returns(validatorCost.Object);
			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);

			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();


			ValidationMessage message = new ValidationMessage("Author", "Unable to reassign author since BOE is not in DRAFT or UNASSIGNED state.");
			ValidationMessage message3 = new ValidationMessage("Subcontractor Author", "Unable to reassign subcontractor author since BOE is not in DRAFT or UNASSIGNED state.");
			ValidationMessage message5 = new ValidationMessage("Approver", "Approver must have the Approver role");
			ExpectedErrors.Add(message5);
			ExpectedErrors.Add(message);
			ExpectedErrors.Add(message3);

			Assert.IsTrue(ValidationErrors.Count == 3, "Only expecting three error messages");
			Assert.IsTrue(ExpectedErrors[0].ValidationIssue == ValidationErrors[0].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[0].FieldName == ValidationErrors[0].FieldName, "Different Field threw a message");
			Assert.IsTrue(ExpectedErrors[1].ValidationIssue == ValidationErrors[1].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[1].FieldName == ValidationErrors[1].FieldName, "Different Field threw a message");
			Assert.IsTrue(ExpectedErrors[2].ValidationIssue == ValidationErrors[2].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[2].FieldName == ValidationErrors[2].FieldName, "Different Field threw a message");
		}
		[TestMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		/// <summary>
		/// Test ValidateAuthorApproverAssigned
		/// </summary>
		public void ValidateSaveManageAuthorApproverStateTransitionFail_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BoeLaborCostElementExistsValidator> validatorCost = new Mock<BoeLaborCostElementExistsValidator>(Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BoeLaborCostElementExistsValidator), validatorCost.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { ClinID = 1, WbsID = 1, BoeID = 1, State = BOEState.Draft, isMaterial = false, IsMultiClinWbs = false };
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
			boeMV.Approvers.Add(100);
			boeMV.Authors.Add(101);
			boeMV.SubcontractorAuthors.Add(102);

			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };

			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user2, user3, user4 };
			UserDTO approver = new UserDTO() { NTID = "user15", UserID = 15 };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, CLINID = 1, WBSID = 1, Id = 1, State = BOEState.Draft, AuthorIDs = new Collection<int>() { user1.Id } };
			FullClin clin1 = new FullClin() { Id = 1 };
			FullWbs wbs1 = new FullWbs() { Id = 1 };
			FullWbs multiwbs = new FullWbs() { Id = 2, WbsNumber = "MULTI", WbsTitle = "MULTI" };
			UserDTO group = new UserDTO() { NTID = "test.user15", UserID = 15, DisplayName = "test.test" };

			FullClin multiclin = new FullClin() { Id = 2, ClinTitle = "MULTI", ClinNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Approved}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};
			ICollection<UserData> GetAdGroupUsers = new Collection<UserData>{
				 new UserData(){
					 Ntid = "test"
				 }
			 };
			int userId = 1000;
			string errorMessage = "Fail State Transition";
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.Draft, BOEState.Draft, out errorMessage)).Returns(false);
			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, multiclin });
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, multiwbs });
			Factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(multiwbs);

			validatorCost.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });

			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			_userLoader.Setup(x => x.GetUserByID(100)).Returns(approver);
			_userLoader.Setup(x => x.GetUserByID(2)).Returns(approver);
			_userLoader.Setup(x => x.GetByIds(new List<int>() { 2 })).Returns(new Collection<UserDTO>() { group });
			_userLoader.Setup(x => x.GetByIds(new List<int>() { 1000 })).Returns(new Collection<UserDTO>() { group });
			_userLoader.Setup(x => x.UserExists("test", out userId)).Returns(true);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeLaborCostElementExists)).Returns(validatorCost.Object);
			_ADUtils.Setup(x => x.GetAdGroupUsers(It.IsAny<string>())).Returns(GetAdGroupUsers);
			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();
			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);


			ValidationMessage message = new ValidationMessage("State", errorMessage);
			ExpectedErrors.Add(message);


			Assert.IsTrue(ValidationErrors.Count == 1, "Only expecting two error message");
			Assert.IsTrue(ExpectedErrors[0].FieldName == ValidationErrors[0].FieldName, "Different Field threw a message");
		}

		[TestMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		/// <summary>
		/// Test ValidateAuthorApproverAssigned no approver
		/// </summary>
		public void ValidateSaveManageAuthorApproverAssigned_NoApproverTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BoeLaborCostElementExistsValidator> validatorCost = new Mock<BoeLaborCostElementExistsValidator>(Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BoeLaborCostElementExistsValidator), validatorCost.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { ClinID = 1, WbsID = 1, BoeID = 1, State = BOEState.Approved, isMaterial = false, IsMultiClinWbs = false };
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
			boeMV.Approvers.Add(2);
			boeMV.Authors.Add(1);

			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };

			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user3, user4 };
			UserDTO approver = new UserDTO() { NTID = "user15", UserID = 2 };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, CLINID = 1, WBSID = 1, Id = 1, State = BOEState.Approved, AuthorIDs = new Collection<int>() { user1.Id } };
			FullClin clin1 = new FullClin() { Id = 1 };
			FullWbs wbs1 = new FullWbs() { Id = 1 };
			FullWbs multiwbs = new FullWbs() { Id = 2, WbsNumber = "MULTI", WbsTitle = "MULTI" };

			FullClin multiclin = new FullClin() { Id = 2, ClinTitle = "MULTI", ClinNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Approved}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};
			ICollection<UserData> GetAdGroupUsers = new Collection<UserData>{
				 new UserData(){
					 Ntid = "test"
				 }
			 };
			int userId = 1000;
			string errorMessage = "Fail State Transition";
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.Approved, BOEState.Approved, out errorMessage)).Returns(true);
			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, multiclin });
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, multiwbs });
			Factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(multiwbs);

			validatorCost.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });

			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			_userLoader.Setup(x => x.GetUserByID(2)).Returns(approver);
			_userLoader.Setup(x => x.GetByIds(new List<int>() { 2 })).Returns(new Collection<UserDTO>() { });
			_userLoader.Setup(x => x.UserExists("test", out userId)).Returns(true);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeLaborCostElementExists)).Returns(validatorCost.Object);
			_ADUtils.Setup(x => x.GetAdGroupUsers(It.IsAny<string>())).Returns(GetAdGroupUsers);
			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();
			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);


			ValidationMessage message = new ValidationMessage("Approver", "Author assigned, however no approver was assigned.");
			ExpectedErrors.Add(message);


			Assert.IsTrue(ValidationErrors.Count == 1, "Only expecting two error message");
			Assert.IsTrue(ExpectedErrors[0].ValidationIssue == ValidationErrors[0].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[0].FieldName == ValidationErrors[0].FieldName, "Different Field threw a message");
		}

		[TestMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		/// <summary>
		/// Test ValidateAuthorApproverAssigned no author
		/// </summary>
		public void ValidateSaveManageAuthorApproverAssigned_NoAuthorTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BoeLaborCostElementExistsValidator> validatorCost = new Mock<BoeLaborCostElementExistsValidator>(Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BoeLaborCostElementExistsValidator), validatorCost.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { ClinID = 1, WbsID = 1, BoeID = -1, State = BOEState.Approved, isMaterial = false, IsMultiClinWbs = false };
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
			boeMV.Approvers.Add(2);

			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };

			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user2, user3, user4 };
			UserDTO approver = new UserDTO() { NTID = "user15", UserID = 2 };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, CLINID = 1, WBSID = 1, Id = 1, State = BOEState.Approved, AuthorIDs = new Collection<int>() { user1.Id } };
			FullClin clin1 = new FullClin() { Id = 1 };
			FullWbs wbs1 = new FullWbs() { Id = 1 };
			FullWbs multiwbs = new FullWbs() { Id = 2, WbsNumber = "MULTI", WbsTitle = "MULTI" };

			FullClin multiclin = new FullClin() { Id = 2, ClinTitle = "MULTI", ClinNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Approved}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};
			ICollection<UserData> GetAdGroupUsers = new Collection<UserData>{
				 new UserData(){
					 Ntid = "test"
				 }
			 };
			int userId = 1000;
			string errorMessage = "Fail State Transition";
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.Approved, BOEState.Approved, out errorMessage)).Returns(true);
			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, multiclin });
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, multiwbs });

			validatorCost.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			Factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(multiwbs);

			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			_userLoader.Setup(x => x.GetUserByID(2)).Returns(approver);
			_userLoader.Setup(x => x.GetByIds(new List<int>() { 2 })).Returns(new Collection<UserDTO>() { });
			_userLoader.Setup(x => x.UserExists("test", out userId)).Returns(true);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeLaborCostElementExists)).Returns(validatorCost.Object);
			_ADUtils.Setup(x => x.GetAdGroupUsers(It.IsAny<string>())).Returns(GetAdGroupUsers);
			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();
			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);


			ValidationMessage message = new ValidationMessage("Author", "An Author or Subcontractor Author is required.");
			ExpectedErrors.Add(message);


			Assert.IsTrue(ValidationErrors.Count == 1, "Only expecting two error message");
			Assert.IsTrue(ExpectedErrors[0].ValidationIssue == ValidationErrors[0].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[0].FieldName == ValidationErrors[0].FieldName, "Different Field threw a message");
		}

		[TestMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		/// <summary>
		/// Test ValidateAuthorApproverAssigned author and approver can not be the same
		/// </summary>
		public void ValidateSaveManageAuthorApproverAssigned_AuthorApproverFailTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BoeLaborCostElementExistsValidator> validatorCost = new Mock<BoeLaborCostElementExistsValidator>(Factory.Object);
			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BoeLaborCostElementExistsValidator), validatorCost.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			ManageBOEModelView boeMV = new ManageBOEModelView() { ClinID = 1, WbsID = 1, BoeID = -1, State = BOEState.Approved, isMaterial = false, IsMultiClinWbs = false };
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
			boeMV.Approvers.Add(2);
			boeMV.Authors.Add(2);
			FullWorkspace ws = new FullWorkspace() { WorkspaceState = WorkspaceState.Working, Id = 1 };

			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>() { boeMV };
			PermissionsDTO user1 = new PermissionsDTO() { BOEId = 1, Role = Role.Author, NTID = "user1", Id = 1, ETIUserId = 1 };
			PermissionsDTO user2 = new PermissionsDTO() { BOEId = 1, Role = Role.Approver, NTID = "user2", Id = 2, ETIUserId = 2 };

			PermissionsDTO user3 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceReviewer, NTID = "user3", Id = 3, ETIUserId = 3 };
			PermissionsDTO user4 = new PermissionsDTO() { BOEId = 1, Role = Role.WorkspaceAdmin, NTID = "user4", Id = 4, ETIUserId = 4 };
			Collection<PermissionsDTO> permissions = new Collection<PermissionsDTO>() { user1, user2, user3, user4 };
			UserDTO author = new UserDTO() { NTID = user1.NTID, UserID = 1 };
			FullBoe boe = new FullBoe() { WorkspaceID = 1, CLINID = 1, WBSID = 1, Id = 1, State = BOEState.Approved, AuthorIDs = new Collection<int>() { user1.Id } };
			FullClin clin1 = new FullClin() { Id = 1 };
			FullWbs wbs1 = new FullWbs() { Id = 1 };
			FullWbs multiwbs = new FullWbs() { Id = 2, WbsNumber = "MULTI", WbsTitle = "MULTI" };

			FullClin multiclin = new FullClin() { Id = 2, ClinTitle = "MULTI", ClinNumber = "MULTI" };
			Dictionary<int, BOEState> boeDictionary = new Dictionary<int, BOEState>()
			{
				{1, BOEState.Approved}
			};

			Dictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>()
			{
				{1, boe}
			};
			ICollection<UserData> GetAdGroupUsers = new Collection<UserData>{
				 new UserData(){
					 Ntid = "test"
				 }
			 };
			int userId = 1000;
			string errorMessage = "Fail State Transition";
			_boeStateMachine.Setup(x => x.PerformStateTransitionValidation(boe, ws, BOEState.Approved, BOEState.Approved, out errorMessage)).Returns(true);
			_retriever.Setup(x => x.GetApproverResponseCollectionByBoeId(ws.Id)).Returns(new Collection<BoeApproverResponseDTO>() { new BoeApproverResponseDTO() { BoeID = boe.Id, ETIUserID = user2.Id } });
			_retriever.Setup(x => x.GetClinsByWorkspaceId(ws.Id)).Returns(new Collection<FullClin>() { clin1, multiclin });
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(ws.Id)).Returns(new Collection<FullWbs>() { wbs1, multiwbs });
			Factory.Setup(x => x.CreateFullWbs(It.IsAny<WbsDTO>())).Returns(multiwbs);

			validatorCost.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });

			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(ws.Id, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { boe });
			_userLoader.Setup(x => x.GetUserByID(2)).Returns(author);
			_userLoader.Setup(x => x.GetByIds(new List<int>() { 2 })).Returns(new Collection<UserDTO>() { });
			_userLoader.Setup(x => x.UserExists("test", out userId)).Returns(true);
			validatorMaterials.Setup(x => x.validation(It.IsAny<string>(), It.IsAny<Collection<Dictionary<string, string>>>())).Returns(new Collection<string>() { });
			_permissionsLoader.Setup(x => x.GetBOEPotentialPermissionsForWorkspace(1)).Returns(permissions);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeMaterialElementExists)).Returns(validatorMaterials.Object);
			validationFactoyMock.Setup(x => x.getValidator(ValidationType.BoeLaborCostElementExists)).Returns(validatorCost.Object);
			_ADUtils.Setup(x => x.GetAdGroupUsers(It.IsAny<string>())).Returns(GetAdGroupUsers);
			Collection<ValidationMessage> ExpectedErrors = new Collection<ValidationMessage>();
			sut.ValidateSaveManageBOE(boeMVs, ws, permissions, boeDictionary, ValidationErrors, workspaceBoeDictionary);



			ValidationMessage message = new ValidationMessage("Author", "Approver cannot be the same person as author");
			ValidationMessage message1 = new ValidationMessage("Approver", "Approver must have the Approver role");
			ExpectedErrors.Add(message);
			ExpectedErrors.Add(message1);


			Assert.IsTrue(ValidationErrors.Count == 2, "Only expecting two error message");
			Assert.IsTrue(ExpectedErrors[0].ValidationIssue == ValidationErrors[0].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[0].FieldName == ValidationErrors[0].FieldName, "Different Field threw a message");

			Assert.IsTrue(ExpectedErrors[1].ValidationIssue == ValidationErrors[1].ValidationIssue, "Not the message we are expecting");
			Assert.IsTrue(ExpectedErrors[1].FieldName == ValidationErrors[1].FieldName, "Different Field threw a message");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ValidateSaveManageBOE_NullWSTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOEWBSMoveValidator> validatorWBSMove = new Mock<BOEWBSMoveValidator>(circularReference.Object, Factory.Object);

			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);

			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEWBSMoveValidator), validatorWBSMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			FullWorkspace ws = null;
			ICollection<PermissionsDTO> boePermissions = new Collection<PermissionsDTO>();
			Dictionary<int, BOEState> BoeStateDictionary = new Dictionary<int, BOEState>();
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
			IDictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>();
			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>();
			sut.ValidateSaveManageBOE(boeMVs, ws, boePermissions, BoeStateDictionary, ValidationErrors, workspaceBoeDictionary);

		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ValidateSaveManageBOE_NullBoeTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOEWBSMoveValidator> validatorWBSMove = new Mock<BOEWBSMoveValidator>(circularReference.Object, Factory.Object);

			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);

			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEWBSMoveValidator), validatorWBSMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			FullWorkspace ws = new FullWorkspace();
			ICollection<PermissionsDTO> boePermissions = new Collection<PermissionsDTO>();
			Dictionary<int, BOEState> BoeStateDictionary = new Dictionary<int, BOEState>();
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
			IDictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>();
			Collection<ManageBOEModelView> boeMVs = null;
			sut.ValidateSaveManageBOE(boeMVs, ws, boePermissions, BoeStateDictionary, ValidationErrors, workspaceBoeDictionary);

		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ValidateSaveManageBOE_NullPermissionsTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOEWBSMoveValidator> validatorWBSMove = new Mock<BOEWBSMoveValidator>(circularReference.Object, Factory.Object);

			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);

			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEWBSMoveValidator), validatorWBSMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			FullWorkspace ws = new FullWorkspace();
			ICollection<PermissionsDTO> boePermissions = null;
			Dictionary<int, BOEState> BoeStateDictionary = new Dictionary<int, BOEState>();
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
			IDictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>();
			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>();
			sut.ValidateSaveManageBOE(boeMVs, ws, boePermissions, BoeStateDictionary, ValidationErrors, workspaceBoeDictionary);

		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ValidateSaveManageBOE_NullBOEStateTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOEWBSMoveValidator> validatorWBSMove = new Mock<BOEWBSMoveValidator>(circularReference.Object, Factory.Object);

			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);

			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEWBSMoveValidator), validatorWBSMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			FullWorkspace ws = new FullWorkspace();
			ICollection<PermissionsDTO> boePermissions = new Collection<PermissionsDTO>();
			Dictionary<int, BOEState> BoeStateDictionary = null;
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
			IDictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>();
			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>();
			sut.ValidateSaveManageBOE(boeMVs, ws, boePermissions, BoeStateDictionary, ValidationErrors, workspaceBoeDictionary);

		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ValidateSaveManageBOE_NullValidationTest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOEWBSMoveValidator> validatorWBSMove = new Mock<BOEWBSMoveValidator>(circularReference.Object, Factory.Object);

			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);

			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEWBSMoveValidator), validatorWBSMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			FullWorkspace ws = new FullWorkspace();
			ICollection<PermissionsDTO> boePermissions = new Collection<PermissionsDTO>();
			Dictionary<int, BOEState> BoeStateDictionary = new Dictionary<int, BOEState>();
			Collection<ValidationMessage> ValidationErrors = null;
			IDictionary<int, FullBoe> workspaceBoeDictionary = new Dictionary<int, FullBoe>();
			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>();
			sut.ValidateSaveManageBOE(boeMVs, ws, boePermissions, BoeStateDictionary, ValidationErrors, workspaceBoeDictionary);

		}
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ValidateSaveManageBOE_NullWSBOETest()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOEWBSMoveValidator> validatorWBSMove = new Mock<BOEWBSMoveValidator>(circularReference.Object, Factory.Object);

			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);

			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEWBSMoveValidator), validatorWBSMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion

			FullWorkspace ws = new FullWorkspace();
			ICollection<PermissionsDTO> boePermissions = new Collection<PermissionsDTO>();
			Dictionary<int, BOEState> BoeStateDictionary = new Dictionary<int, BOEState>();
			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();
			IDictionary<int, FullBoe> workspaceBoeDictionary = null;
			Collection<ManageBOEModelView> boeMVs = new Collection<ManageBOEModelView>();
			sut.ValidateSaveManageBOE(boeMVs, ws, boePermissions, BoeStateDictionary, ValidationErrors, workspaceBoeDictionary);

		}
		/// <summary>
		/// Test a good removal of multiboes
		/// </summary>
		[TestMethod]
		public void RemoveMultiBOEReferenceWorkspaceVar_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOEWBSMoveValidator> validatorWBSMove = new Mock<BOEWBSMoveValidator>(circularReference.Object, Factory.Object);

			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);

			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();
			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEWBSMoveValidator), validatorWBSMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO>();
			Mock<VariableSelectBOEtoSumCalculation> varSelectBOEToSum = new Mock<VariableSelectBOEtoSumCalculation>();
			WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO()
			{
				Id = 1,
				SelectedBOEsToSum = new Collection<SelectBOEsToSum>(){
						new SelectBOEsToSum() {
							BoeID = 1
						}
					}
			};
			workspaceVars.Add(workspaceVar);

			List<WorkspaceVariableDTO> userWorkspaceVar = new List<WorkspaceVariableDTO>();

			_retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(1)).Returns(workspaceVars);
			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(1, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>());
			_retriever.Setup(x => x.GetResourcesByResourceListId(1)).Returns(new Collection<ResourceDTO>());
			_retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());
			_retriever.Setup(x => x.GetResourcesByResourceListId(It.IsAny<int>())).Returns(new Collection<ResourceDTO>());
			_retriever.Setup(x => x.GetClinsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullClin>());
			_retriever.Setup(x => x.GetFullWbsElementsByWorkspaceId(It.IsAny<int>())).Returns(new Collection<FullWbs>());

			varSelectBOEToSum.Setup(x => x.GetWorkspaceVarLabelTotal(It.IsAny<WorkspaceVariableDTO>(), It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(0);
			_workspaceVariableDTODataLoader.Setup(x => x.SaveWorkspaceVariables(It.IsAny<Collection<WorkspaceVariableDTO>>()));
			FullWorkspace ws = new FullWorkspace();
			ws.Id = 1;
			Assert.AreEqual(1, workspaceVar.SelectedBOEsToSum.Count());
			Assert.AreEqual(0, userWorkspaceVar.Count());
			sut.RemoveMultiBOEReferenceWorkspaceVar(ws, userWorkspaceVar, new Collection<int>() { 1 });

			Assert.AreEqual(0, workspaceVar.SelectedBOEsToSum.Count());
			Assert.AreEqual(1, userWorkspaceVar.Count());


		}

		/// <summary>
		/// Test a ws null removal of multiboes
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RemoveMultiBOEReferenceWorkspaceVar_TestNull_WS()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOEWBSMoveValidator> validatorWBSMove = new Mock<BOEWBSMoveValidator>(circularReference.Object, Factory.Object);

			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);

			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();
			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEWBSMoveValidator), validatorWBSMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO>();
			Mock<VariableSelectBOEtoSumCalculation> varSelectBOEToSum = new Mock<VariableSelectBOEtoSumCalculation>();
			WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO()
			{
				Id = 1,
				SelectedBOEsToSum = new Collection<SelectBOEsToSum>(){
						new SelectBOEsToSum() {
							BoeID = 1
						}
					}
			};
			workspaceVars.Add(workspaceVar);

			List<WorkspaceVariableDTO> userWorkspaceVar = new List<WorkspaceVariableDTO>();

			_retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(1)).Returns(workspaceVars);
			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(1, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>());
			_retriever.Setup(x => x.GetResourcesByResourceListId(1)).Returns(new Collection<ResourceDTO>());
			_retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());


			varSelectBOEToSum.Setup(x => x.GetWorkspaceVarLabelTotal(It.IsAny<WorkspaceVariableDTO>(), It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(0);
			_workspaceVariableDTODataLoader.Setup(x => x.SaveWorkspaceVariables(It.IsAny<Collection<WorkspaceVariableDTO>>()));
			FullWorkspace ws = null;

			Assert.AreEqual(1, workspaceVar.SelectedBOEsToSum.Count());
			Assert.AreEqual(0, userWorkspaceVar.Count());
			sut.RemoveMultiBOEReferenceWorkspaceVar(ws, userWorkspaceVar, new Collection<int>() { 1 });
		}

		/// <summary>
		/// Test a good removal of multiboes
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RemoveMultiBOEReferenceWorkspaceVar_TestNull_workspaceVariablesAffectedByMulti()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOEWBSMoveValidator> validatorWBSMove = new Mock<BOEWBSMoveValidator>(circularReference.Object, Factory.Object);

			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);

			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();
			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEWBSMoveValidator), validatorWBSMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion
			Collection<WorkspaceVariableDTO> workspaceVars = new Collection<WorkspaceVariableDTO>();
			Mock<VariableSelectBOEtoSumCalculation> varSelectBOEToSum = new Mock<VariableSelectBOEtoSumCalculation>();
			WorkspaceVariableDTO workspaceVar = new WorkspaceVariableDTO()
			{
				Id = 1,
				SelectedBOEsToSum = new Collection<SelectBOEsToSum>(){
						new SelectBOEsToSum() {
							BoeID = 1
						}
					}
			};
			workspaceVars.Add(workspaceVar);

			List<WorkspaceVariableDTO> userWorkspaceVar = null;

			_retriever.Setup(x => x.GetWorkspaceVariableDTOsByWorkspaceId(1)).Returns(workspaceVars);
			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(1, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>());
			_retriever.Setup(x => x.GetResourcesByResourceListId(1)).Returns(new Collection<ResourceDTO>());
			_retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>());


			varSelectBOEToSum.Setup(x => x.GetWorkspaceVarLabelTotal(It.IsAny<WorkspaceVariableDTO>(), It.IsAny<DataClassForSumOfBOEsCalculation>())).Returns(0);
			_workspaceVariableDTODataLoader.Setup(x => x.SaveWorkspaceVariables(It.IsAny<Collection<WorkspaceVariableDTO>>()));
			FullWorkspace ws = new FullWorkspace();

			Assert.AreEqual(1, workspaceVar.SelectedBOEsToSum.Count());

			sut.RemoveMultiBOEReferenceWorkspaceVar(ws, userWorkspaceVar, new Collection<int>() { 1 });

		}

		[TestMethod]
		public void RemoveMultiBOEReferenceTaskVar_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOEWBSMoveValidator> validatorWBSMove = new Mock<BOEWBSMoveValidator>(circularReference.Object, Factory.Object);

			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();
			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEWBSMoveValidator), validatorWBSMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion
			Collection<OrdinaryVariableDto> TaskVars = new Collection<OrdinaryVariableDto>();
			OrdinaryVariableDto taskVar = new OrdinaryVariableDto()
			{
				Id = 1,
				SelectedBOEsToSum = new Collection<SelectBOEsToSum>(){
						new SelectBOEsToSum() {
							BoeID = 1,
							CLINID = 1
						}
					},
				TaskElementId = 1
			};

			TaskVars.Add(taskVar);


			BoeTaskElementDTO task1 = new BoeTaskElementDTO();
			task1.OrdinaryVariables.Add(taskVar);
			task1.Id = 1;
			task1.BoeID = 1;


			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(1, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { new FullBoe() { CLINID = 1 } });
			_retriever.Setup(x => x.GetResourcesByResourceListId(1)).Returns(new Collection<ResourceDTO>());
			_retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { task1 });


			FullWorkspace ws = new FullWorkspace();
			ws.Id = 1;
			Assert.AreEqual(1, task1.OrdinaryVariables.Count());

			Assert.AreEqual(1, task1.OrdinaryVariables[0].SelectedBOEsToSum.Count());
			sut.RemoveMultiBOEReferenceTaskVar(ws, new Collection<int>() { 1 });
			Assert.AreEqual(1, task1.OrdinaryVariables.Count());

			Assert.AreEqual(0, task1.OrdinaryVariables[0].SelectedBOEsToSum.Count());

		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RemoveMultiBOEReferenceTaskVar_TestNull()
		{
			BOEControllerLogic sut = this.CreateSystem();
			Mock<IWorkspaceVariableDTODataLoader> workspacevardto = new Mock<IWorkspaceVariableDTODataLoader>();
			Mock<VariableCircularReferenceChecker> circularReference = new Mock<VariableCircularReferenceChecker>(workspacevardto.Object);
			Mock<BOEWBSMoveValidator> validatorWBSMove = new Mock<BOEWBSMoveValidator>(circularReference.Object, Factory.Object);

			Mock<BOEMaterialElementExistsValidator> validatorMaterials = new Mock<BOEMaterialElementExistsValidator>(Factory.Object);
			Mock<ValidationFactory> validationFactoyMock = CreateValidationFactoryMock();
			Mock<Validator> boeMaterialElementExistsValidator = new Mock<Validator>();
			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), _variableCircularReferenceChecker.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), boeMaterialElementExistsValidator.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ValidationFactory), validationFactoyMock.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IVariableCircularReferenceChecker), circularReference.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEWBSMoveValidator), validatorWBSMove.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IUserDTODataLoader), _userLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(BOEMaterialElementExistsValidator), validatorMaterials.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(Validator), _validator.Object);
			#endregion
			Collection<OrdinaryVariableDto> TaskVars = new Collection<OrdinaryVariableDto>();
			OrdinaryVariableDto taskVar = new OrdinaryVariableDto()
			{
				Id = 1,
				SelectedBOEsToSum = new Collection<SelectBOEsToSum>(){
						new SelectBOEsToSum() {
							BoeID = 1,
							CLINID = 1
						}
					}
			};

			TaskVars.Add(taskVar);


			BoeTaskElementDTO task1 = new BoeTaskElementDTO();
			task1.OrdinaryVariables.Add(taskVar);
			task1.Id = 1;
			task1.BoeID = 1;


			_retriever.Setup(x => x.GetFullBoesByWorkspaceId(1, It.IsAny<bool>(), It.IsAny<IEnumerable<BoeTaskElementDTO>>())).Returns(new Collection<FullBoe>() { new FullBoe() { CLINID = 1 } });
			_retriever.Setup(x => x.GetResourcesByResourceListId(1)).Returns(new Collection<ResourceDTO>());
			_retriever.Setup(x => x.GetBoeTaskElementCollectionByWorkspaceId(1, false, It.IsAny<int>(), It.IsAny<int>())).Returns(new Collection<BoeTaskElementDTO>() { task1 });


			FullWorkspace ws = null;
			sut.RemoveMultiBOEReferenceTaskVar(ws, new Collection<int>() { 1 });
		}

		[TestMethod]
		public void SetDefaultBoeDatesTest()
		{
			BOEControllerLogic sut = this.CreateSystem();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			#endregion

			FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 4, 1) };

			ClinDTO clin = new ClinDTO()
			{
				Id = 1,
				StartDate = new DateTime(2012, 1, 1),
				EndDate = new DateTime(2013, 1, 1)
			};

			ClinDTO clin_noDates = new ClinDTO()
			{
				Id = 2,
				StartDate = null,
				EndDate = null
			};

			Collection<FullClin> wsClins = new Collection<FullClin>() { new FullClin(clin), new FullClin(clin_noDates) };

			FullWorkspace ws = new FullWorkspace()
			{
				Id = 100,
				ContractStartDate = new DateTime(2010, 1, 1),
				ContractEndDate = new DateTime(2015, 12, 1)
			};

			_retriever.Setup(x => x.GetClinsByWorkspaceId(100)).Returns(wsClins);

			//Regular BOE with CLIN Dates
			//Act
			sut.setDefaultBoeDates(ws, boe, 1);

			//Assert
			Assert.AreEqual(boe.StartDate, clin.StartDate);
			Assert.AreEqual(boe.EndDate, clin.EndDate);

			//Regular BOE with CLIN no Dates
			//Act
			sut.setDefaultBoeDates(ws, boe, 2);

			//Assert
			Assert.AreEqual(boe.StartDate, ws.ContractStartDate);
			Assert.AreEqual(boe.EndDate, ws.ContractEndDate);

			//Regular BOE with no CLIN
			//Act
			sut.setDefaultBoeDates(ws, boe, null);

			//Assert
			Assert.AreEqual(boe.StartDate, ws.ContractStartDate);
			Assert.AreEqual(boe.EndDate, ws.ContractEndDate);


		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SetDefaultBoeDates_NullWs()
		{
			BOEControllerLogic sut = this.CreateSystem();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			#endregion

			FullBoe boe = new FullBoe() { Id = 2, StartDate = new DateTime(2014, 1, 1), EndDate = new DateTime(2014, 4, 1) };

			//Act
			sut.setDefaultBoeDates(null, boe, 1);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SetDefaultBoeDates_NullBoe()
		{
			BOEControllerLogic sut = this.CreateSystem();

			#region InitializeUnity
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			#endregion

			FullWorkspace ws = new FullWorkspace()
			{
				Id = 100,
				ContractStartDate = new DateTime(2010, 1, 1),
				ContractEndDate = new DateTime(2015, 12, 1)
			};

			//Act
			sut.setDefaultBoeDates(ws, null, 1);
		}

		[TestMethod]
		/// <summary>
		/// Test ValidateBOEHeaderCustomFields - User saves header removing one selection and adding another
		/// </summary>
		public void ValidateBOEHeaderCustomFields_ValidNonSummary_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);

			FullWorkspace ws = new FullWorkspace() { Id = 1 };
			FullBoe boe = new FullBoe() { Id = 1 };
			BOEHeaderModelView boeHeader = new BOEHeaderModelView() { BOEID = boe.Id };

			CustomFieldDTO cf1 = new CustomFieldDTO { Id = 1, CustomFieldName = "Color", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = false };
			CustomFieldDTO cf2 = new CustomFieldDTO { Id = 2, CustomFieldName = "Name", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = false };
			Collection<CustomFieldDTO> wsCustomFields = new Collection<CustomFieldDTO>() { cf1, cf2 };
			_retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(ws.Id)).Returns(wsCustomFields);

			//Custom Field 1
			CustomFieldValueDTO boeCustomFieldValue_Red = new CustomFieldValueDTO { CustomFieldID = cf1.Id, CustomFieldValueID = 2, CustomFieldValueName = "Red", CustomFieldValueDescription = "the color red", CustomFieldValueInUseFlag = false };
			CustomFieldValueDTO boeCustomFieldValue_Blue = new CustomFieldValueDTO { CustomFieldID = cf1.Id, CustomFieldValueID = 3, CustomFieldValueName = "Blue", CustomFieldValueDescription = "the color blue", CustomFieldValueInUseFlag = false };
			ICollection<CustomFieldValueDTO> colorOptions = new Collection<CustomFieldValueDTO> { boeCustomFieldValue_Red, boeCustomFieldValue_Blue };
			_customFieldValueDTODataLoader.Setup(x => x.GetCustomFieldValueDTOsByCustomFieldIds(new List<int> { cf1.Id })).Returns(colorOptions);

			//Custom Field 2
			CustomFieldValueDTO boeCustomFieldValue_Mike = new CustomFieldValueDTO { CustomFieldID = cf2.Id, CustomFieldValueID = 4, CustomFieldValueName = "Mike", CustomFieldValueDescription = "Mike Basquill", CustomFieldValueInUseFlag = true };
			CustomFieldValueDTO boeCustomFieldValue_Matt = new CustomFieldValueDTO { CustomFieldID = cf2.Id, CustomFieldValueID = 5, CustomFieldValueName = "Matt", CustomFieldValueDescription = "Matt Kotwicki", CustomFieldValueInUseFlag = false };
			ICollection<CustomFieldValueDTO> nameOptions = new Collection<CustomFieldValueDTO> { boeCustomFieldValue_Mike, boeCustomFieldValue_Matt };
			_customFieldValueDTODataLoader.Setup(x => x.GetCustomFieldValueDTOsByCustomFieldIds(new List<int> { cf2.Id })).Returns(nameOptions);

			//Setup Existing custom field selection (Nothing in Color; Mike for Name)
			Collection<CustomFieldValueContainer> existingSelections = new Collection<CustomFieldValueContainer>() {
				new CustomFieldValueContainer()
				{
					ContainerID = 25,
					CustomFieldValueID = boeCustomFieldValue_Mike.Id,
					UpdateDate = new DateTime(2010,1,1)
				}
			};
			boe.CustomFieldValueContainers = existingSelections;

			// Selections in Header Save
			//Select Red for Color field
			CustomFieldSelectionModelView selection1 = new CustomFieldSelectionModelView() { CustomFieldValueID = 2, SelectionID = -1 };
			//Delete selection for Name
			CustomFieldSelectionModelView selection2 = new CustomFieldSelectionModelView() { CustomFieldValueID = -1, SelectionID = 25 };
			Collection<CustomFieldSelectionModelView> selections = new Collection<CustomFieldSelectionModelView>() { selection1, selection2 };
			boeHeader.CustomFieldValues = selections;


			Collection<ValidationMessage> errors = null;

			_validationHelper.Setup(x => x.BOEHeaderCustomFieldValidation(It.IsAny<Collection<CustomFieldValueContainer>>(), It.IsAny<ICollection<CustomFieldDTO>>())).Returns(errors);

			//ACT
			sut.ValidateBOEHeaderCustomFields(ws, boe, boeHeader);

			//ASSERT
			_validationHelper.Verify(x => x.BOEHeaderCustomFieldValidation(It.IsAny<Collection<CustomFieldValueContainer>>(), It.IsAny<ICollection<CustomFieldDTO>>()), Times.Once());
		}

		[TestMethod]
		[ExpectedException(typeof(GenValidationException))]
		/// <summary>
		/// Test ValidateBOEHeaderCustomFields - User saves header removing one selection and adding another. Error is thrown.
		/// </summary>
		public void ValidateBOEHeaderCustomFields_InvalidNonSummary_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);

			FullWorkspace ws = new FullWorkspace() { Id = 1 };
			FullBoe boe = new FullBoe() { Id = 1 };
			BOEHeaderModelView boeHeader = new BOEHeaderModelView() { BOEID = boe.Id };

			CustomFieldDTO cf1 = new CustomFieldDTO { Id = 1, CustomFieldName = "Color", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = false };
			CustomFieldDTO cf2 = new CustomFieldDTO { Id = 2, CustomFieldName = "Name", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = true };
			Collection<CustomFieldDTO> wsCustomFields = new Collection<CustomFieldDTO>() { cf1, cf2 };
			_retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(ws.Id)).Returns(wsCustomFields);

			//Custom Field 1
			CustomFieldValueDTO boeCustomFieldValue_Red = new CustomFieldValueDTO { CustomFieldID = cf1.Id, CustomFieldValueID = 2, CustomFieldValueName = "Red", CustomFieldValueDescription = "the color red", CustomFieldValueInUseFlag = false };
			CustomFieldValueDTO boeCustomFieldValue_Blue = new CustomFieldValueDTO { CustomFieldID = cf1.Id, CustomFieldValueID = 3, CustomFieldValueName = "Blue", CustomFieldValueDescription = "the color blue", CustomFieldValueInUseFlag = false };
			ICollection<CustomFieldValueDTO> colorOptions = new Collection<CustomFieldValueDTO> { boeCustomFieldValue_Red, boeCustomFieldValue_Blue };
			_customFieldValueDTODataLoader.Setup(x => x.GetCustomFieldValueDTOsByCustomFieldIds(new List<int> { cf1.Id })).Returns(colorOptions);

			//Custom Field 2
			CustomFieldValueDTO boeCustomFieldValue_Mike = new CustomFieldValueDTO { CustomFieldID = cf2.Id, CustomFieldValueID = 4, CustomFieldValueName = "Mike", CustomFieldValueDescription = "Mike Basquill", CustomFieldValueInUseFlag = true };
			CustomFieldValueDTO boeCustomFieldValue_Matt = new CustomFieldValueDTO { CustomFieldID = cf2.Id, CustomFieldValueID = 5, CustomFieldValueName = "Matt", CustomFieldValueDescription = "Matt Kotwicki", CustomFieldValueInUseFlag = false };
			ICollection<CustomFieldValueDTO> nameOptions = new Collection<CustomFieldValueDTO> { boeCustomFieldValue_Mike, boeCustomFieldValue_Matt };
			_customFieldValueDTODataLoader.Setup(x => x.GetCustomFieldValueDTOsByCustomFieldIds(new List<int> { cf2.Id })).Returns(nameOptions);

			//Setup Existing custom field selection (Nothing in Color; Mike for Name)
			Collection<CustomFieldValueContainer> existingSelections = new Collection<CustomFieldValueContainer>() {
				new CustomFieldValueContainer()
				{
					ContainerID = 25,
					CustomFieldValueID = boeCustomFieldValue_Mike.Id,
					UpdateDate = new DateTime(2010,1,1)
				}
			};
			boe.CustomFieldValueContainers = existingSelections;

			// Selections in Header Save
			//Select Red for Color field
			CustomFieldSelectionModelView selection1 = new CustomFieldSelectionModelView() { CustomFieldValueID = 2, SelectionID = -1 };
			//Delete selection for Name
			CustomFieldSelectionModelView selection2 = new CustomFieldSelectionModelView() { CustomFieldValueID = -1, SelectionID = 25 };
			Collection<CustomFieldSelectionModelView> selections = new Collection<CustomFieldSelectionModelView>() { selection1, selection2 };
			boeHeader.CustomFieldValues = selections;

			Collection<ValidationMessage> errors = new Collection<ValidationMessage>() { new ValidationMessage("Custom Field " + cf2.CustomFieldName + " is required.") };

			_validationHelper.Setup(x => x.BOEHeaderCustomFieldValidation(It.IsAny<Collection<CustomFieldValueContainer>>(), It.IsAny<ICollection<CustomFieldDTO>>())).Returns(errors);

			//ACT
			sut.ValidateBOEHeaderCustomFields(ws, boe, boeHeader);

			//ASSERT
			_validationHelper.Verify(x => x.BOEHeaderCustomFieldValidation(It.IsAny<Collection<CustomFieldValueContainer>>(), It.IsAny<ICollection<CustomFieldDTO>>()), Times.Once());
		}

		[TestMethod]
		[ExpectedException(typeof(GenValidationException))]
		/// <summary>
		/// Test ValidateBOEHeaderCustomFields for open ended custom field. Error is thrown.
		/// </summary>
		public void ValidateBOEHeaderOpenEndedCustomFields_InvalidNonSummary_Test()
		{
			BOEControllerLogic sut = this.CreateSystem();

			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(ICommonDataMapper), _commonDataMapper.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IPermissionsDTODataLoader), _permissionsLoader.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);

			FullWorkspace ws = new FullWorkspace() { Id = 1 };
			FullBoe boe = new FullBoe() { Id = 1 };
			BOEHeaderModelView boeHeader = new BOEHeaderModelView() { BOEID = boe.Id };

			CustomFieldDTO cf1 = new CustomFieldDTO { Id = 1, CustomFieldName = "Color", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = false, IsOpenEnded = true };
			CustomFieldDTO cf2 = new CustomFieldDTO { Id = 2, CustomFieldName = "Name", WorkspaceID = ws.Id, CustomFieldDisplayID = CustomFieldType.BoeDisplay, CustomFieldRequired = true, IsOpenEnded = true };
			Collection<CustomFieldDTO> wsCustomFields = new Collection<CustomFieldDTO>() { cf1, cf2 };
			_retriever.Setup(x => x.GetCustomFieldsByWorkspaceId(ws.Id)).Returns(wsCustomFields);

			_customFieldValueDTODataLoader.Setup(x => x.GetCustomFieldValueDTOsByCustomFieldIds(It.IsAny<ICollection<int>>())).Returns(new Collection<CustomFieldValueDTO>());

			Collection<CustomFieldValueContainer> existingValues = new Collection<CustomFieldValueContainer>() {
				new CustomFieldValueContainer()
				{
					ContainerID = 25,
					CustomFieldValueID = 1,
					UpdateDate = new DateTime(2010,1,1),
					IsOpenEnded = true,
					OpenEndedValue = string.Empty
				}
			};
			boe.CustomFieldValueContainers = existingValues;

			// Selections in Header Save
			CustomFieldSelectionModelView selection1 = new CustomFieldSelectionModelView() { CustomFieldValueID = 2, SelectionID = -1, IsOpenEnded = true, OpenEndedValue = string.Empty };
			CustomFieldSelectionModelView selection2 = new CustomFieldSelectionModelView() { CustomFieldValueID = -1, SelectionID = -2, IsOpenEnded = true, OpenEndedValue = string.Empty };
			Collection<CustomFieldSelectionModelView> selections = new Collection<CustomFieldSelectionModelView>() { selection1, selection2 };
			boeHeader.CustomFieldValues = selections;

			Collection<ValidationMessage> errors = new Collection<ValidationMessage>() { new ValidationMessage("Custom Field " + cf2.CustomFieldName + " is required.") };

			_validationHelper.Setup(x => x.BOEHeaderCustomFieldValidation(It.IsAny<Collection<CustomFieldValueContainer>>(), It.IsAny<ICollection<CustomFieldDTO>>())).Returns(errors);

			//ACT
			sut.ValidateBOEHeaderCustomFields(ws, boe, boeHeader);
		}

		/// <summary>
		/// Test that Total Cost of Travel is calculated appropriately for both domestic and international.
		/// </summary>
		[TestMethod]
		public void CalculateTotalCostTravel_Test()
		{
			BOEControllerLogic sut = this.CreateSystemMST();
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IRetriever), _retriever.Object);
			GenBOEUnityContainer.Container.RegisterInstance(typeof(IFullObjectFactory), Factory.Object);

			// Create escalation rates.
			List<WorkspaceRMSEscalationRatesDTO> escalationRates = new List<WorkspaceRMSEscalationRatesDTO>
			{
				new WorkspaceRMSEscalationRatesDTO { Year = DateTime.Today.Year - 1, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1 },
				new WorkspaceRMSEscalationRatesDTO { Year = DateTime.Today.Year, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1 },
				new WorkspaceRMSEscalationRatesDTO { Year = DateTime.Today.Year + 1, AirfareRate = 1, MiscRate = 1, PerDiemRate = 1 }
			};

			Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = new Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO>();

			// Create fees.
			WorkspaceRMSTravelNonzoneFeesAndCostsDTO nonZoneDomestic = new WorkspaceRMSTravelNonzoneFeesAndCostsDTO();
			nonZoneDomestic.Id = 1;
			nonZoneDomestic.ModeID = (int)MSTTravelMode.NonZoneDomestic;
			nonZoneDomestic.TravelAgencyFee = 0.5M;
			nonZoneDomestic.MiscOther = 0.25M;

			WorkspaceRMSTravelNonzoneFeesAndCostsDTO nonZoneInternational = new WorkspaceRMSTravelNonzoneFeesAndCostsDTO();
			nonZoneInternational.Id = 2;
			nonZoneInternational.ModeID = (int)MSTTravelMode.NonZoneInternational;
			nonZoneInternational.TravelAgencyFee = 0.2M;
			nonZoneInternational.MiscOther = 0.3M;

			fees.Add((int)MSTTravelMode.NonZoneDomestic, nonZoneDomestic);
			fees.Add((int)MSTTravelMode.NonZoneInternational, nonZoneInternational);

			// Create NonZoneDomestic trip.
			FullBoe boe = new FullBoe();
			boe.Id = 1;
			Factory.Setup(x => x.CreateFullBoe(boe.Id)).Returns(boe);

			Collection<TravelDTO> travels = new Collection<TravelDTO>();
			TravelDTO travelDTO = new TravelDTO() { BoeID = boe.Id };
			Collection<MSTTravelTripType> mstTravelTrips = new Collection<MSTTravelTripType>();

			_retriever.Setup(x => x.GetTravelByWorkspaceId(It.IsAny<int>(), false)).Returns(travels);

			MSTTravelTripType trip = new MSTTravelTripType();
			trip.NumOfDays = 1;
			trip.NumOfPeople = 2;
			trip.NonZoneNumCars = 3;
			trip.NonZonePerDiemDaily = 4;
			trip.NonZoneCarRentalTrans = 5;
			trip.NonZoneAirfareEstimate = 6;
			trip.EstimateDate = DateTime.Today;
			trip.TripDate = DateTime.Today;
			trip.ModeID = MSTTravelMode.NonZoneDomestic;

			mstTravelTrips.Add(trip);
			travelDTO.MSTTravelTrips = mstTravelTrips;
			travels.Add(travelDTO);

			decimal resultDomestic = sut.CalculateTotalCostTravel(travels, 0, escalationRates, fees);
			Assert.AreEqual(28.5M, resultDomestic, "Domestic total cost of travel calculated incorrectly.");

			// Modify trip to be for NonZoneInternational, then retest.
			trip.ModeID = MSTTravelMode.NonZoneInternational;

			mstTravelTrips.Clear();
			travels.Clear();

			mstTravelTrips.Add(trip);
			travelDTO.MSTTravelTrips = mstTravelTrips;
			travels.Add(travelDTO);

			decimal resultInternational = sut.CalculateTotalCostTravel(travels, 0, escalationRates, fees);
			Assert.AreEqual(27.4M, resultInternational, "International total cost of travel calculated incorrectly.");
		}

		/// <summary>
		/// Tests getting all the fields.
		/// </summary>
		[TestMethod]
		[Ignore]
		public async Task GetAllFields_Test()
		{
			ICollection<GenBOE.ActionLogic.IESSAPClient.QueryFieldViewModel> fields = new List<GenBOE.ActionLogic.IESSAPClient.QueryFieldViewModel>();

			using(HttpClient httpClient = new HttpClient())
			{
                GenBOE.ActionLogic.IESSAPClient.IESSAPClient client = new GenBOE.ActionLogic.IESSAPClient.IESSAPClient(WebConfigurationManager.AppSettings["IESSAPUrl"], httpClient);

				Utilities.AddAuthorizationHeader(client.HttpClient, (await tokenService.GetToken()).AccessToken);

				fields = await client.ApiQueryFilterGetAllFieldsForCompanyCodeAsync(GenBOE.ActionLogic.IESSAPClient.CompanyConfiguration.SpaceSystems);
			}

			Assert.IsTrue(fields.Any());
		}

		/// <summary>
		/// Tests getting all the operators.
		/// </summary>
		[TestMethod]
		[Ignore]
		public async Task GetAllOperators_Test()
		{
			ICollection<GenBOE.ActionLogic.IESSAPClient.QueryOperatorViewModel> operators = new List<GenBOE.ActionLogic.IESSAPClient.QueryOperatorViewModel>();

			using (HttpClient httpClient = new HttpClient())
			{
                GenBOE.ActionLogic.IESSAPClient.IESSAPClient client = new GenBOE.ActionLogic.IESSAPClient.IESSAPClient(WebConfigurationManager.AppSettings["IESSAPUrl"], httpClient);

				Utilities.AddAuthorizationHeader(client.HttpClient, (await tokenService.GetToken()).AccessToken);

				operators = await client.ApiQueryFilterGetAllOperatorsAsync();
			}

			Assert.IsTrue(operators.Any());
		}
	}
}
