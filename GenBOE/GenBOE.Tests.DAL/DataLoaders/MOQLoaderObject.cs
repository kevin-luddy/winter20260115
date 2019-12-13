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
    using System.Diagnostics;
    using System.Linq;
    using System.Transactions;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// To be used by tests that require data in the database.  Has initialize and teardown
    /// methods in order to delete any data created by the test.
    /// </summary>
    [TestClass]
    abstract public class MOQLoaderObject : MOQObject
    {
        private static Collection<string> _FirstNames { get; set; }
        private static Collection<string> _LastNames { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IClinDTODataLoader _clinDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IWbsDTODataLoader _wbsDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IBoeDTODataLoader _boeDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IUserDTODataLoader _userDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IResourceListDTODataLoader _resourceListDL= null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDegclareVisibleInstanceFields")]
        protected IPerformingOrgListDTODataLoader _perfOrgListDL= null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IPerformingOrgDTODataLoader _perfOrgDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IResourceDTODataLoader _resourceOrgDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IWorkspaceDTODataLoader _workspaceDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IPermissionsDTODataLoader _permissionDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IBoeTaskElementDTODataLoader _taskElementDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IWorkspaceVariableDTODataLoader _workspaceVariableDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IBoeApproverResponseDTODataLoader _approverResponseDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IMiscTravelRateDTOLoader _miscTravelRateDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected IPerDiemDTODataLoader _perDiemDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected ILocationDTODataLoader _locationDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected ITripDTODataLoader _tripDL = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected ITravelDTODataLoader _travelDL = null;
        

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected MOQLoaderObject() : base(false)
        {
            IResourceTypeLoader resourceTypeLoader = new ResourceTypeLoader();
            IResourceSpreadLoader resourceSpreadLoader = new ResourceSpreadLoader();
            IOrdinaryVariableLoader ordinaryVariableLoader = new OrdinaryVariableLoader();
            IBoeTaskElementCustomFieldValueXREFLoader taskElementCustomFieldLoader = new BoeTaskElementCustomFieldValueXREFLoader();
            ILaborTypeCustomFieldValueXREFLoader laborTypeCustomFieldLoader = new LaborTypeCustomFieldValueXREFLoader();
            var _travelTripTaskElementCustomFieldValue = new Mock<ITravelTripTaskElementCustomFieldValueXREFLoader>();
            var _travelTripCustomFieldValue = new Mock<ITravelTripCustomFieldValueXREFLoader>();
            var adUtils = new Mock<IActiveDirectoryUtilities>();
            var memCache = new Mock<MemoryCache>();
            SecurityInformation securityInformation = new SecurityInformation(adUtils.Object, memCache.Object);

            _taskElementDL = new BoeTaskElementDTODataLoader(resourceTypeLoader, resourceSpreadLoader, ordinaryVariableLoader, taskElementCustomFieldLoader, laborTypeCustomFieldLoader);

            // initialize data loaders
            _clinDL = new ClinDTODataLoader();
            _wbsDL = new WbsDTODataLoader();
            _boeDL = new BoeDTODataLoader();
            _userDL = new UserDTODataLoader(securityInformation, adUtils.Object);
            _resourceListDL = new ResourceListDTODataLoader();
            _resourceOrgDL = new ResourceDTODataLoader();
            _perfOrgDL = new PerformingOrgDTODataLoader();
            _perfOrgListDL = new PerformingOrgListDTODataLoader();
            _workspaceDL = new WorkspaceDTODataLoader();
            _permissionDL = new PermissionsDTODataLoader(adUtils.Object);
            _workspaceVariableDL = new WorkspaceVariableDTODataLoader();
            _approverResponseDL = new BoeApproverResponseDTODataLoader();
            _miscTravelRateDL = new MiscTravelRateDTOLoader();
            _perDiemDL = new PerDiemDTODataLoader();
            _locationDL = new LocationDTODataLoader();
            _tripDL = new TripDTODataLoader();
            
             _travelDL = new TravelDTODataLoader(_travelTripTaskElementCustomFieldValue.Object, _travelTripCustomFieldValue.Object);

            // clean up left over test data .. once
            ClassCleanup();

            // create a branch new set of objects for test use
            this._Initialize(new ObjectGraph());
        }

        static bool INITIAL_CLEAN_FINISHED = false;
        private void ClassCleanup()
        {
            if (!INITIAL_CLEAN_FINISHED)
            {
                // clean out any remnants of test left over
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.Database.CommandTimeout = 1500; // 25 minutes
                    gbe.deleteMockTestData();
                }

                INITIAL_CLEAN_FINISHED = true;

                // reset the global test cases so we don't have any issues with deleted data still being called
                GlobalTestCaseSetup.ResetGlobalWorkspaceID();
                GlobalTestCaseSetup.ResetGlobalBOEID();
                GlobalTestCaseSetup.ResetGlobalWBSID();
                GlobalTestCaseSetup.ResetGlobalTaskElementID();
                GlobalTestCaseSetup.ResetGlobalODCElementID();
                GlobalTestCaseSetup.ResetGlobalBOELaborTypeID();
                GlobalTestCaseSetup.ResetGlobalClinID();
                GlobalTestCaseSetup.ResetGlobalPerfOrgListID();
                GlobalTestCaseSetup.ResetGlobalPerfOrgID();
                GlobalTestCaseSetup.ResetGlobalResourceListID();
                GlobalTestCaseSetup.ResetGlobalResourceID();
                GlobalTestCaseSetup.ResetGlobalCustomFieldID();
                GlobalTestCaseSetup.ResetGlobalCustomFieldValueID();
                GlobalTestCaseSetup.ResetGlobalBOECustomFieldValueXrefID();
                GlobalTestCaseSetup.ResetGlobalBOELaborTypeCustomFieldValueXrefID();
                GlobalTestCaseSetup.ResetGlobalBOETaskElementCustomFieldValueXrefID();

            }
        }

        public void DeleteBOEs(Collection<BoeDTO> boesToDelete)
        {
            if (boesToDelete == null)
            {
                throw new ArgumentNullException(nameof(boesToDelete));
            }

            foreach (BoeDTO boeToDelete in boesToDelete)
            {
                // first delete task elements for each BOE
                ICollection<BoeTaskElementDTO> taskElementsForBOE = _taskElementDL.GetByBoeId(boeToDelete.Id, 0, 2);
                foreach (BoeTaskElementDTO taskElementForBOE in taskElementsForBOE)
                {
                    // delete any vars, then delete task element                    
                    foreach (OrdinaryVariableDto var in taskElementForBOE.OrdinaryVariables)
                    {
                        var.Updateable = UpdateType.Deleted;
                        _taskElementDL.SaveOrdinaryVariables(new Collection<OrdinaryVariableDto> { var });

                    }

                    // confirm
                    BoeTaskElementDTO deletedTE = _taskElementDL.GetById(taskElementForBOE.Id, Workspace.DecimalPrecision, Workspace.CostDecimalPrecision);
                    Assert.AreEqual(0, deletedTE.OrdinaryVariables.Count);

                    // delete task element now
                    taskElementForBOE.Updateable = UpdateType.Deleted;
                    _taskElementDL.SaveBoeTaskElements(new Collection<BoeTaskElementDTO> { taskElementForBOE });

                    // confirm delete worked
                    BoeTaskElementDTO deletedTaskElement = _taskElementDL.GetById(taskElementForBOE.Id, Workspace.DecimalPrecision, Workspace.CostDecimalPrecision);
                    Assert.IsNull(deletedTaskElement);
                }

                // now delete BOE itself
                boeToDelete.Updateable = UpdateType.Deleted;
                using (TransactionScope scope = new TransactionScope())
                {
                    _boeDL.Save(boeToDelete);
                    scope.Complete();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "metricAdmin"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "wsadmin"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "wbs"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "clin"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "boes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "departureloc"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "perdiem"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "perforg"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "perforglist"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "resourcelist"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "sysadmin"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "systemtrip"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "travelrate"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "wadmin"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), TestInitialize()]
        override public void Setup()
        {
            if (!_INITIALIZED)
            {
                try
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
                    {
                        Stopwatch swBig = new Stopwatch();
                        swBig.Start();
                        Stopwatch sw = new Stopwatch();
                        // if the user didn't pass in values for our objects, create them all now
                        sw.Start();
                        if (CostVolumeLead == null)
                        {
                            CostVolumeLead = _CreateNewUserNotSaved();
                        }

                        CostVolumeLead = _userDL.SaveUser(CostVolumeLead);
                        Assert.IsNotNull(CostVolumeLead);
                        Console.WriteLine("user  took " + sw.ElapsedMilliseconds);

                   

                        sw.Restart();
                        if (WorkspaceAdmin == null)
                        {
                            WorkspaceAdmin = _CreateNewUserNotSaved();
                        }
                        WorkspaceAdmin = _userDL.SaveUser(WorkspaceAdmin);
                        Assert.IsNotNull(WorkspaceAdmin);
                        Console.WriteLine("wadmin  took " + sw.ElapsedMilliseconds);

                        sw.Restart();
                        if (SysAdmin == null)
                        {
                            SysAdmin = _CreateNewUserNotSaved();
                        }
                        SysAdmin = _userDL.SaveUser(SysAdmin);
                        Assert.IsNotNull(SysAdmin);
                        Console.WriteLine("sysadmin  took " + sw.ElapsedMilliseconds);

                        sw.Restart();
                        if (MetricAdmin == null)
                        {
                            MetricAdmin = _CreateNewUserNotSaved();
                        }
                        MetricAdmin = _userDL.SaveUser(MetricAdmin);
                        Assert.IsNotNull(MetricAdmin);
                        Console.WriteLine("metricAdmin  took " + sw.ElapsedMilliseconds);

                        sw.Restart();
                        if (CreateWorkspacePermissions == null)
                        {
                            CreateWorkspacePermissions = _CreateNewUserNotSaved();
                        }
                        CreateWorkspacePermissions = _userDL.SaveUser(CreateWorkspacePermissions);
                        Assert.IsNotNull(CreateWorkspacePermissions);

                        sw.Restart();

                        if (Approver1 == null)
                        {
                            Approver1 = _CreateNewUserNotSaved();
                        }

                        Approver1 = _userDL.SaveUser(Approver1);
                        Assert.IsNotNull(Approver1);
                        Console.WriteLine("approver  took " + sw.ElapsedMilliseconds);

                        sw.Restart();

                        if (Approver2 == null)
                        {
                            Approver2 = _CreateNewUserNotSaved();
                        }

                        Approver2 = _userDL.SaveUser(Approver2);
                        Assert.IsNotNull(Approver2);
                        Console.WriteLine("approver2  took " + sw.ElapsedMilliseconds);

                        sw.Restart();
                        if (Author == null)
                        {
                            Author = _CreateNewUserNotSaved();
                        }

                        Author = _userDL.SaveUser(Author);
                        Assert.IsNotNull(Author);
                        Console.WriteLine("author  took " + sw.ElapsedMilliseconds);

                        sw.Restart();
                        // misc travel rate
                        if (MiscTravelRate == null)
                        {
                            MiscTravelRate = _CreateMiscTraveRate(-1);
                        }
                        Dictionary<int, int> travelRateIds = _miscTravelRateDL.SaveMiscTravelRates(new Collection<MiscTravelRateDTO> { MiscTravelRate });
                        MiscTravelRate = _miscTravelRateDL.GetById(travelRateIds[-1]);
                        Assert.IsNotNull(MiscTravelRate);
                        Console.WriteLine("travelrate  took " + sw.ElapsedMilliseconds);

                        sw.Restart();
                        // departure location
                        if (DepartureLocation == null)
                        {
                            string dName = Guid.NewGuid().ToString().Substring(0, 10);
                            DepartureLocation = new LocationDTO { Id = -1, LastUpdatedBy = Author.UserID, LocationName = "MOCK" + dName, Updateable = UpdateType.Upsert };
                        }
                        int? depLocId = _locationDL.Save(DepartureLocation);
                        DepartureLocation = _locationDL.GetById(depLocId.Value);
                        Assert.IsNotNull(DepartureLocation);
                        Console.WriteLine("departureloc  took " + sw.ElapsedMilliseconds);

                        // destination location
                        sw.Restart();
                        if (DestinationLocation == null)
                        {
                            string deName = Guid.NewGuid().ToString().Substring(0, 10);
                            DestinationLocation = new LocationDTO { Id = -1, LastUpdatedBy = Author.UserID, LocationName = "MOCK" + deName, Updateable = UpdateType.Upsert };
                        }
                        int? destLocId = _locationDL.Save(DestinationLocation);
                        DestinationLocation = _locationDL.GetById(destLocId.Value);
                        Assert.IsNotNull(DestinationLocation);
                        Console.WriteLine("destination  took " + sw.ElapsedMilliseconds);

                        // per diem
                        sw.Restart();
                        if (PerDiem == null)
                        {
                            PerDiem = new PerDiemDTO { Id = -1, HotelRate = 10m, MIERate = 10m, PerDiemDestination = "MockGlobalLoaderPerDiemDest " + randomNumberGenerator.Next(), Qualification = string.Empty, PerDiemNotes = "blah blah global loader", LastUpdatedBy = Author.UserID, Updateable = UpdateType.Upsert };
                        }
                        int perDiemId = _perDiemDL.SavePerDiem(PerDiem);
                        PerDiem = _perDiemDL.GetByIds(new Collection<int> { perDiemId }).FirstOrDefault();
                        Assert.IsNotNull(PerDiem);
                        Console.WriteLine("perdiem  took " + sw.ElapsedMilliseconds);

                        // system trip
                         sw.Restart();
                        if (SystemTrip == null)
                        {
                            SystemTrip = _CreateTrip(-1);
                        }
                        Dictionary<int, int> systemTripIds = _tripDL.SaveTrips(new Collection<TripDTO> { SystemTrip });
                        SystemTrip = _tripDL.GetTripByTripID(systemTripIds[-1]);
                        Assert.IsNotNull(SystemTrip);
                        Console.WriteLine("systemtrip  took " + sw.ElapsedMilliseconds);

                        sw.Restart();
                        if (Workspace == null)
                        {
                            string name = Guid.NewGuid().ToString().Substring(0, 10);
                            string longname = Guid.NewGuid().ToString().Substring(0, 18);
                            CommonDataLoader dataLoader = new CommonDataLoader();
                            
                            Workspace = new WorkspaceDTO
                            {
                                Id = -1,
                                WorkspaceName = "Mock_" + longname,
                                Shortname = "sn_" + name,
                                Description = "Desc_Workspace MOQ Description",
                                WorkspaceState = IES.Common.WorkspaceState.Initialization,
                                CreatedByUserID = Author.UserID,
                                CostVolumeLeadPricerUserID = CostVolumeLead.UserID,
                                PerfOrgListID = -1,
                                ResourceListID = -1,
                                AllowSearch = true,
                                ProposalSubmittalDate = Convert.ToDateTime("02/01/2011"),
                                NumberOfTimesExportedToProPricer = 0,
                                ProposalStatus = ProposalStatusType.None,
                                StatusComment = "MOQLoaderObject test case setup",
                                BOEExportSortByID = (int)ExportSortBOEBy.WBS,
                                Segment = SegmentType.SSC,
                                TemplateID = (int)ExcelReportTemplateType.MASTER
                            };
                        }

                        int wsid = _workspaceDL.SaveWorkspaceSettings(CostVolumeLead.UserID, Workspace);
                        Workspace = _workspaceDL.GetById(wsid);
                        Assert.IsNotNull(Workspace);
                        Console.WriteLine("workspace  took " + sw.ElapsedMilliseconds);

                        sw.Restart();
                        // inserting WS above gives us our resourcelist/resource and performingorglist/perf org...
                        // now update them with the data passed in
                        if (ResourceList == null)
                        {
                            ResourceListDTO privateResourceList = _resourceListDL.GetResourceList(Workspace.ResourceListID);
                            ResourceList = new ResourceListDTO { ResourceListID = Workspace.ResourceListID, ResourceListName = Guid.NewGuid().ToString().Substring(0, 10), Updateable = UpdateType.Upsert, UpdateDate = privateResourceList.UpdateDate };
                        }

                        int resourceListId = _resourceListDL.SaveResourceList(ResourceList);
                        ResourceList = _resourceListDL.GetResourceList(resourceListId);
                        Assert.IsNotNull(ResourceList);
                        Assert.AreEqual(resourceListId, ResourceList.ResourceListID);
                        Console.WriteLine("resource list  took " + sw.ElapsedMilliseconds);

                        sw.Restart();
                        if (PerfOrgList == null)
                        {
                            PerformingOrgListDTO privatePerfOrgList = _perfOrgListDL.GetPerfOrgList(Workspace.PerfOrgListID);
                            PerfOrgList = new PerformingOrgListDTO { PerformingOrgListID = Workspace.PerfOrgListID, PerformingOrgListName = Guid.NewGuid().ToString().Substring(0, 10), Updateable = UpdateType.Upsert, UpdateDate = privatePerfOrgList.UpdateDate };
                        }

                        int perfOrgListId = _perfOrgListDL.SavePerformingOrgList(PerfOrgList);
                        PerfOrgList = _perfOrgListDL.GetPerfOrgList(perfOrgListId);
                        Assert.IsNotNull(PerfOrgList);
                        Assert.AreEqual(perfOrgListId, PerfOrgList.PerformingOrgListID);
                        Console.WriteLine("perforglist  took " + sw.ElapsedMilliseconds);

                        sw.Restart();
                        if (Resource == null)
                        {

                            string name = Guid.NewGuid().ToString().Substring(0, 10);
                            Resource = new ResourceDTO { Id = -1, ResourceName = name, RateType = RateType.Hours, ResourceDesc = name, SegRegion = "UnitTest", LaborType = "UnitTest", ElementOfCost = ElementOfCostType.LMLabor, Updateable = UpdateType.Upsert };
                        }

                        WorkspaceDTO tempWorkspace = new WorkspaceDTO();
                        tempWorkspace.ResourceListID = this.ResourceList.ResourceListID;

                        _resourceOrgDL.SaveWorkspaceResources(tempWorkspace, new Collection<ResourceDTO> { Resource });
                        ////Resource = _resourceOrgDL.GetAllResourcesByResourceListId(ResourceList.ResourceListID).Where(x => x.ResourceName.Equals(Resource.ResourceName)).First();
                        Assert.IsNotNull(Resource);
                        Console.WriteLine("resource  took " + sw.ElapsedMilliseconds);

                        sw.Restart();
                        if (SystemResource == null)
                        {

                            string name = Guid.NewGuid().ToString().Substring(0, 10);
                            SystemResource = new ResourceDTO { Id = -1, ResourceName = name, RateType = RateType.Hours, ResourceDesc = name, SegRegion = "UnitTest", LaborType = "UnitTest", ElementOfCost = ElementOfCostType.LMLabor, Updateable = UpdateType.Upsert };
                        }

                        _resourceOrgDL.SaveSystemResources(new Collection<ResourceDTO> { SystemResource });
                        ////SystemResource = _resourceOrgDL.GetAllResourcesByResourceListId(1).Where(x => x.ResourceName.Equals(SystemResource.ResourceName)).First();
                        Assert.IsNotNull(SystemResource);
                        Console.WriteLine("system resource  took " + sw.ElapsedMilliseconds);

                        sw.Restart();
                        if (Perforg == null)
                        {
                            string name = Guid.NewGuid().ToString().Substring(0, 10);
                            Perforg = new PerformingOrgDTO { Id = -1, PerformingOrgName = name, PerformingOrgDesc = name, Updateable = UpdateType.Upsert };
                        }

                        Dictionary<int, int> perfOrgIds = _perfOrgDL.SaveWorkspacePerformingOrgs(new Collection<PerformingOrgDTO> { Perforg }, PerfOrgList.PerformingOrgListID);
                        Perforg = _perfOrgDL.GetById(perfOrgIds[-1]);
                        Assert.IsNotNull(Perforg);
                        Console.WriteLine("perforg  took " + sw.ElapsedMilliseconds);


                        #region Create Clin1, Clin2, Clin3
                        if (Clin1 == null)
                        {
                            Clin1 = _CreateClin(-1, Convert.ToDateTime("08/01/2010"), Convert.ToDateTime("04/01/2011"));
                        }
                        Console.WriteLine("clin1 took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        if (Clin2 == null)
                        {
                            Clin2 = _CreateClin(-2, Convert.ToDateTime("08/01/2010"), Convert.ToDateTime("04/01/2011"));
                        }
                        Console.WriteLine("clin2 took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        if (Clin3 == null)
                        {
                            Clin3 = _CreateClin(-3, Convert.ToDateTime("08/01/2010"), Convert.ToDateTime("04/01/2011"));
                        }
                        Dictionary<int, int> clinids = _clinDL.Save(new Collection<ClinDTO> { Clin1, Clin2, Clin3 });
                        Assert.AreEqual(3, clinids.Keys.Count);

                        Console.WriteLine("clin3 took " + sw.ElapsedMilliseconds);
                        sw.Restart();
                        #endregion  Create Clin1, Clin2, Clin3

                        if (Wbs == null)
                        {
                            Wbs = _CreateWBS(-1, new Collection<int> { Clin1.Id, Clin2.Id });
                        }
                        Console.WriteLine("wbs took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        Dictionary<int, int> wbsids = _wbsDL.Save(new Collection<WbsDTO> { Wbs });
                        int key = wbsids.First().Key;
                        Wbs = _wbsDL.GetById(wbsids[key]);
                        Assert.IsNotNull(Wbs);

                        #region Create Boe1, Boe2, Boe3, and MaterialBoe
                        if (Boe1 == null)
                        {
                            Boe1 = _CreateBOE(-1, Clin1.Id, Wbs.Id);
                            Boe1.UpdatedByUserId = Author.UserID;
                        }
                        Console.WriteLine("boe1 took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        int? boe1id = _boeDL.Save(Boe1);
                        Boe1 = _boeDL.GetById(boe1id.Value);

                        if (Boe2 == null)
                        {
                            Boe2 = _CreateBOE(-1, Clin1.Id, Wbs.Id);
                            Boe2.UpdatedByUserId = Author.UserID;
                        }
                        int? boe2id = _boeDL.Save(Boe2);
                        Boe2 = _boeDL.GetById(boe2id.Value);
                        Console.WriteLine("boe2 took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        if (Boe3 == null)
                        {
                            Boe3 = _CreateBOE(-1, Clin1.Id, Wbs.Id);
                            Boe3.UpdatedByUserId = Author.UserID;
                        }
                        int? boe3id = _boeDL.Save(Boe3);
                        Boe3 = _boeDL.GetById(boe3id.Value);
                        Console.WriteLine("boe3 took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        if (BOEMaterial == null)
                        {
                            BOEMaterial = _CreateBOE(-1, Clin1.Id, Wbs.Id, true);
                            BOEMaterial.UpdatedByUserId = Author.UserID;
                        }
                        int? boematerialid = _boeDL.Save(BOEMaterial);
                        BOEMaterial = _boeDL.GetByIds(new Collection<int> { boematerialid.Value }).First();
                        Console.WriteLine("BOE material took " + sw.ElapsedMilliseconds);
                        sw.Restart();
                        #endregion Create Boe1, Boe2, Boe3, and MaterialBoe

                        // assign approvers to boe1
                        _approverResponseDL.Save(new Collection<BoeApproverResponseDTO>{ 
                new BoeApproverResponseDTO{ Updateable= UpdateType.Upsert, BoeID = Boe1.Id, CurrentUserETIUserID = this.WorkspaceAdmin.UserID, ETIUserID = this.Approver1.UserID, Id = -1 },
                new BoeApproverResponseDTO{ Updateable= UpdateType.Upsert, BoeID = Boe1.Id, CurrentUserETIUserID = this.WorkspaceAdmin.UserID, ETIUserID = this.Approver2.UserID, Id = -2 }});
                        Assert.AreEqual(2, _permissionDL.GetBOEPermissions(new List<int>(){Boe1.Id}).Where(x => x.Role == Role.Approver).Count());
                        Console.WriteLine("BOE1 approver took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // assign approvers to boe2
                        _approverResponseDL.Save(new Collection<BoeApproverResponseDTO>{ 
                new BoeApproverResponseDTO{ Updateable= UpdateType.Upsert, BoeID = Boe2.Id, CurrentUserETIUserID = this.WorkspaceAdmin.UserID, ETIUserID = this.Approver1.UserID, Id = -1 },
                new BoeApproverResponseDTO{ Updateable= UpdateType.Upsert, BoeID = Boe2.Id, CurrentUserETIUserID = this.WorkspaceAdmin.UserID, ETIUserID = this.Approver2.UserID, Id = -2 }});
                        Assert.AreEqual(2, _permissionDL.GetBOEPermissions(new List<int>(){Boe2.Id}).Where(x => x.Role == Role.Approver).Count());
                        Console.WriteLine("BOE2 approver took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // set sysadmin
                        _permissionDL.SavePermission(new PermissionsDTO { ETIUserId = SysAdmin.UserID, Role = Role.SystemAdmin, Updateable = UpdateType.Upsert });
                        IEnumerable<PermissionsDTO> perms = _permissionDL.GetAdminPermissions().Where(x => x.ETIUserId == SysAdmin.UserID && x.Role == Role.SystemAdmin);
                        Assert.AreEqual(1, perms.Count());
                        Console.WriteLine("sysadmin permissions took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // set metricadmin
                        _permissionDL.SavePermission(new PermissionsDTO { ETIUserId = MetricAdmin.UserID, Role = Role.MetricsAdmin, Updateable = UpdateType.Upsert });
                        perms = _permissionDL.GetAdminPermissions().Where(x => x.ETIUserId == MetricAdmin.UserID && x.Role == Role.MetricsAdmin);
                        Assert.AreEqual(1, perms.Count());
                        Console.WriteLine("metricAdmin permissions took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // set createworkspacepermissions
                        _permissionDL.SavePermission(new PermissionsDTO { ETIUserId = CreateWorkspacePermissions.UserID, Role = Role.CreateWorkspacePermissions, Updateable = UpdateType.Upsert });
                        perms = _permissionDL.GetAdminPermissions().Where(x => x.ETIUserId == CreateWorkspacePermissions.UserID && x.Role == Role.CreateWorkspacePermissions);
                        Assert.AreEqual(1, perms.Count());
                        sw.Restart();

                        // set workspace admin
                        _permissionDL.SavePermission(new PermissionsDTO { ETIUserId = WorkspaceAdmin.UserID, Role = Role.WorkspaceAdmin, Updateable = UpdateType.Upsert, WorkspaceId = Workspace.Id });
                        perms = _permissionDL.GetWorkspacePermissions(Workspace.Id).Where(x => x.ETIUserId == this.WorkspaceAdmin.UserID && x.Role == Role.WorkspaceAdmin);
                        Assert.AreEqual(1, perms.Count());
                        Console.WriteLine("wsadmin permissions took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // set potential author for the workspace
                        _permissionDL.SavePermission(new PermissionsDTO { ETIUserId = Author.UserID, Role = Role.Author, Updateable = UpdateType.Upsert, WorkspaceId = Workspace.Id });


                        perms = _permissionDL.GetBOEPotentialPermissionsForWorkspace(Workspace.Id).Where(x => x.Role == Role.Author);
                        Assert.AreEqual(1, perms.Count());
                        Console.WriteLine("author permissions took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // set potential approver on boe1
                        _permissionDL.SavePermission(new PermissionsDTO { ETIUserId = Approver1.UserID, Role = Role.Approver, Updateable = UpdateType.Upsert, WorkspaceId = Workspace.Id });

                        Console.WriteLine("approver boe1 permissions took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // set potential approver on boe2
                        _permissionDL.SavePermission(new PermissionsDTO { ETIUserId = Approver2.UserID, Role = Role.Approver, Updateable = UpdateType.Upsert, WorkspaceId = Workspace.Id });


                        perms = _permissionDL.GetBOEPotentialPermissionsForWorkspace(Workspace.Id).Where(x => x.Role == Role.Approver);
                        Assert.AreEqual(2, perms.Count());
                        Console.WriteLine("approver boe2 permissions took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // confirm 3 WSUser (1 unique author, 2 unique approvers)
                        perms = _permissionDL.GetWorkspacePermissions(Workspace.Id).Where(x => x.Role == Role.WorkspaceUser);
                        Assert.AreEqual(3, perms.Count());

                        // add task elements to BOE
                        if (TaskElement == null)
                        {
                            TaskElement = _CreateTaskElement(
                                                -1,
                                                Convert.ToDateTime("09/01/2010"),
                                                Convert.ToDateTime("07/01/2015"),
                                                "14000 Hours * 85% ComplexityFactor",
                                                "hours and factor",
                                                MOQType.SSCAnalogySimilarTo);
                        }
                        Dictionary<int, int> taskElementIds = _taskElementDL.SaveBoeTaskElements(new Collection<BoeTaskElementDTO> { TaskElement });
                        TaskElement = _taskElementDL.GetById(taskElementIds[-1], Workspace.DecimalPrecision, Workspace.CostDecimalPrecision);
                        Assert.IsNotNull(TaskElement);
                        Console.WriteLine("task element took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // create 3 new sum of Boe ordinary/task variables
                        // 1 will reference a summary wbs, another a summary clin, and then the another a boe id

                        // sum of boes - boeid
                        if (SummaryBOEOrdinaryVariable == null)
                        {
                            SummaryBOEOrdinaryVariable = _CreateOrdinaryTaskVariable(-1, "Employee", 245m, VarSortBOEBy.WBS, VarValueType.SumOfBOEs,
                                                        new Collection<SelectBOEsToSum> { new SelectBOEsToSum { WBSID = null, BoeID = Boe1.Id, CLINID = null } }, TaskElement.Id);
                        }
                        Dictionary<int, int> varIds = _taskElementDL.SaveOrdinaryVariables(new Collection<OrdinaryVariableDto> { SummaryBOEOrdinaryVariable });
                        SummaryBOEOrdinaryVariable = _taskElementDL.GetTaskVariableByTaskVariableID(varIds[-1]);
                        Assert.IsNotNull(SummaryBOEOrdinaryVariable);
                        Console.WriteLine("ordinary variable took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // sum of boes - wbs
                        if (SummaryWBSOrdinaryVariable == null)
                        {
                            SummaryWBSOrdinaryVariable = _CreateOrdinaryTaskVariable(-2, "SourceCode", 10m, VarSortBOEBy.WBS, VarValueType.SumOfBOEs,
                                                        new Collection<SelectBOEsToSum> { new SelectBOEsToSum { WBSID = Wbs.Id, BoeID = null, CLINID = null } }, TaskElement.Id);
                        }
                        varIds = _taskElementDL.SaveOrdinaryVariables(new Collection<OrdinaryVariableDto> { SummaryWBSOrdinaryVariable });
                        SummaryWBSOrdinaryVariable = _taskElementDL.GetTaskVariableByTaskVariableID(varIds[-2]);
                        Assert.IsNotNull(SummaryWBSOrdinaryVariable);
                        Console.WriteLine("sum of boes (wbs) permissions took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // sum of boes - clin
                        if (SummaryCLINOrdinaryVariable == null)
                        {
                            SummaryCLINOrdinaryVariable = _CreateOrdinaryTaskVariable(-3, "Vacation", 40m, VarSortBOEBy.CLIN, VarValueType.SumOfBOEs,
                                                        new Collection<SelectBOEsToSum> { new SelectBOEsToSum { WBSID = Wbs.Id, BoeID = null, CLINID = Clin1.Id } }, TaskElement.Id);
                        }
                        varIds = _taskElementDL.SaveOrdinaryVariables(new Collection<OrdinaryVariableDto> { SummaryCLINOrdinaryVariable });
                        SummaryCLINOrdinaryVariable = _taskElementDL.GetTaskVariableByTaskVariableID(varIds[-3]);
                        Assert.IsNotNull(SummaryCLINOrdinaryVariable);
                        Console.WriteLine("sum of boes (clin) permissions took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // workspace variable - clin
                        if (SummaryCLINWorkspaceVariable == null)
                        {
                            SummaryCLINWorkspaceVariable = _CreateWorkspaceVariable(-1, "Pi", 3.14m, VarSortBOEBy.WBS, VarValueType.Discrete,
                                                        new Collection<SelectBOEsToSum> { new SelectBOEsToSum { WBSID = null, BoeID = null, CLINID = Clin1.Id } });
                        }
                        varIds = _workspaceVariableDL.SaveWorkspaceVariables(new Collection<WorkspaceVariableDTO> { SummaryCLINWorkspaceVariable });
                        SummaryCLINWorkspaceVariable = _workspaceVariableDL.GetById(varIds[-1]);
                        Assert.IsNotNull(SummaryCLINWorkspaceVariable);
                        Console.WriteLine("workspace var (clin) took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        // workspace variable - wbs
                        if (SummaryWbsWorkspaceVariable == null)
                        {
                            SummaryWbsWorkspaceVariable = _CreateWorkspaceVariable(-1, "PiWbs", 7.28m, VarSortBOEBy.WBS, VarValueType.Discrete,
                                                        new Collection<SelectBOEsToSum> { new SelectBOEsToSum { WBSID = Wbs.Id, BoeID = null, CLINID = null } });
                        }
                        varIds = _workspaceVariableDL.SaveWorkspaceVariables(new Collection<WorkspaceVariableDTO> { SummaryWbsWorkspaceVariable });
                        SummaryWbsWorkspaceVariable = _workspaceVariableDL.GetById(varIds[-1]);
                        Assert.IsNotNull(SummaryWbsWorkspaceVariable);
                        Console.WriteLine("workspace var (wbs) took " + sw.ElapsedMilliseconds);
                        sw.Restart();

                        scope.Complete();

                        _INITIALIZED = true;
                        Console.WriteLine("entire setup took " + swBig.ElapsedMilliseconds);
                    }
                }
                catch (Exception)
                {
                    _INITIALIZED = false;
                    throw;
                }
            }
        }

        protected OrdinaryVariableDto _CreateOrdinaryTaskVariable(int inId, string inName, decimal inValue, VarSortBOEBy inSortBy, VarValueType inValueType, Collection<SelectBOEsToSum> inToSum, int inTaskElementId)
        {
            OrdinaryVariableDto toReturn = new OrdinaryVariableDto()
            {
                Updateable = UpdateType.Upsert,
                Id = inId,
                OrdinaryVariableName = inName,
                OrdinaryVariableValue = inValue,
                SortBOEBy = inSortBy,
                ValueType = inValueType,
                SelectedBOEsToSum = inToSum,
                TaskElementId = inTaskElementId
            };

            return toReturn;
        }

        protected BoeTaskElementDTO _CreateTaskElement(int inId, DateTime inStartDate, DateTime inEndDate, string inMOQEquation, string inMOQText, MOQType inMOQType)
        {
            BoeTaskElementDTO newElement = new BoeTaskElementDTO()
            {
                Id = inId,
                BOETaskID = "B4" + Math.Abs(inId),
                TaskTitle = Guid.NewGuid().ToString(),
                Description = "a great new moq task " + Math.Abs(inId),
                StartDate = inStartDate,
                EndDate = inEndDate,
                MOQHoursEquation = inMOQEquation,
                MOQText = inMOQText,
                MOQType = inMOQType,
                BoeID = Boe1.Id,
                Updateable = UpdateType.Upsert
            };

            return newElement;
        }

        /// <summary>
        /// Create new BOE, must pass in either ClinID or WbsID (or both)
        /// NOT SAVED!!!
        /// </summary>
        /// <param name="inId">id to use for creation, ex. -1, -2, etc</param>
        /// <param name="inClinId">clin id to use, must have either clin or wbs valued</param>
        /// <param name="inWbsId">wbs id to use, must have either clin or wbs valued</param>
        /// <param name="inCreateMaterial">Should the BOE be a material boe?</param>
        /// <returns>Newly created BoeDTO, NOT SAVED</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        protected BoeDTO _CreateBOE(int inId, int? inClinId, int? inWbsId, bool inCreateMaterial = false)
        {
            if (!inClinId.HasValue && !inWbsId.HasValue)
            {
                throw new ArgumentOutOfRangeException("either inClinId or inWbsId must be valued");
            }

            BoeDTO toReturn = new BoeDTO 
            {
                Id = inId,
                AuthorIDs = new Collection<int>{Author.UserID},
                Description = "MOQ BOE " + Math.Abs(inId) + " Description", 
                WBSID = inWbsId,
                CLINID = inClinId,
                WorkspaceID = Workspace.Id, 
                Updateable = UpdateType.Upsert,
                isMaterial = inCreateMaterial,
                DataSource = "MOQ Data Source"
            };

            return toReturn;
        }

        /// <summary>
        /// Create new WBS
        /// NOT SAVED!!!
        /// </summary>
        /// <param name="inId">id to use for creation, ex. -1, -2, etc</param>
        /// <param name="inClinIds">the clin ids to associate to the wbs</param>
        /// <returns>newly created WbsDTO, NOT SAVED</returns>
        protected WbsDTO _CreateWBS(int inId, Collection<int> inClinIds)
        {
            WbsDTO toReturn = new WbsDTO
            {
                Id = inId,
                ClinIDs = inClinIds,
                WbsNumber = MOQObject.randomNumberGenerator.Next().ToString(),
                WbsTitle = MOQObject.randomNumberGenerator.Next().ToString(),
                WorkspaceID = Workspace.Id,
                Updateable = UpdateType.Upsert
            };

            return toReturn;
        }

        /// <summary>
        /// Create new WBS
        /// NOT SAVED!!!
        /// </summary>
        /// <param name="inId">id to use for creation, ex. -1, -2, etc</param>
        /// <param name="inClinIds">the clin ids to associate to the wbs</param>
        /// <returns>newly created WbsDTO, NOT SAVED</returns>
        protected WbsDTO _CreateWBS(int inId, Collection<int> inClinIds, string wbsNumber)
        {
            WbsDTO toReturn = new WbsDTO
            {
                Id = inId,
                ClinIDs = inClinIds,
                WbsNumber = wbsNumber,
                WbsTitle = MOQObject.randomNumberGenerator.Next().ToString(),
                WorkspaceID = Workspace.Id,
                Updateable = UpdateType.Upsert
            };

            return toReturn;
        }

        /// <summary>
        /// Create new Clin
        /// NOT SAVED!!!
        /// </summary>
        /// <param name="inId">id to use for creation, ex. -1, -2, etc</param>
        /// <param name="inStartDate">start date to use</param>
        /// <param name="inEndDate">end date to use</param>
        /// <returns>Newly created ClineDTO, NOT SAVED</returns>
        protected ClinDTO _CreateClin(int inId, DateTime inStartDate, DateTime inEndDate)
        {
            ClinDTO toReturn = new ClinDTO
            {
                Id = inId,
                ClinNumber = "Moq" + MOQObject.randomNumberGenerator.Next().ToString(),
                ClinTitle = "Clin"+Math.Abs(inId)+" MOQ Title",
                WorkspaceID = Workspace.Id,
                StartDate = inStartDate,
                EndDate = inEndDate,
                Updateable = UpdateType.Upsert
            };

            return toReturn;
        }

        protected WorkspaceVariableDTO _CreateWorkspaceVariable(int inId, string inName, decimal inValue, VarSortBOEBy inSortBy, VarValueType inValueType, Collection<SelectBOEsToSum> inToSum)
        {
            WorkspaceVariableDTO newVar = new WorkspaceVariableDTO()
            {
                Id = inId,
                WorkspaceVariableName = inName,
                WorkspaceVariableValue = inValue,
                WorkspaceID = Workspace.Id,
                Updateable = UpdateType.Upsert,
                SortBOEBy = inSortBy,
                ValueType = inValueType,
                SelectedBOEsToSum = inToSum
            };

            return newVar;
        }

        protected MiscTravelRateDTO _CreateMiscTraveRate(int inId)
        {
            // find a sort code that isn't used
            Collection<MiscTravelRateDTO> travelrates = _miscTravelRateDL.GetAll();
            IEnumerable<int> travelRateSortCodes = travelrates.Select(x => x.SortCode).ToArray();
            bool unique = false;
            int sortCode = 0;
            while (!unique)
            {
                if (!travelRateSortCodes.Contains(sortCode))
                {
                    unique = true;
                }
                else if (++sortCode == 255)
                {
                    Assert.Fail("Ran out of sort codes to try");
                }
            }

            MiscTravelRateDTO travelRate = new MiscTravelRateDTO();
            travelRate.SortCode = sortCode;
            travelRate.Id = inId;
            travelRate.MiscTravelRate = randomNumberGenerator.Next(400);
            travelRate.MiscTravelRateMode = "MockNewLdrGlblMode" + sortCode;
            travelRate.Updateable = UpdateType.Upsert;

            return travelRate;
        }

        protected TripDTO _CreateTrip(int inId)
        {
            // create a trip 
            TripDTO trip = new TripDTO();
            trip.TripID = inId;
            trip.Updateable = UpdateType.Upsert;
            trip.MiscTravelRateID = MiscTravelRate.Id;
            trip.DepartureLocationID = DepartureLocation.Id;
            trip.DestinationLocationID = DestinationLocation.Id;
            trip.PerDiemID = PerDiem.Id;
            trip.Fare = 50;
            trip.RTMiles = 50;
            trip.FareUpdatedByUserID = Author.UserID;

            return trip;
        }

        protected UserDTO _CreateNewUserNotSaved()
        {
            // Create a new user
            UserDTO newUser = new UserDTO { UserID = -1 };

            string unique = Guid.NewGuid().ToString().Substring(0, 4);

            // try to adjust theNTID since we can generate duplicates...
            newUser.FirstName = _FirstNames[MOQObject.randomNumberGenerator.Next(_FirstNames.Count() - 1)];
            newUser.LastName = _LastNames[MOQObject.randomNumberGenerator.Next(_LastNames.Count() - 1)];
            newUser.DisplayName = newUser.FirstName + " " + newUser.LastName;

            // create the ntID from the users randomly generated name, we have to stay within the max # of 
            // characters in the name though.  place some random GUID chars after the name to help with uniqueness
            newUser.NTID = newUser.FirstName.Substring(0, newUser.FirstName.Length >= 1 ? 1 : newUser.FirstName.Length) +
                            newUser.LastName.Substring(0, newUser.LastName.Length >= 5 ? 5 : newUser.LastName.Length) + unique;

            newUser.PhoneNumber = "555/555-5555";

            newUser.IsUsPerson = true;
            newUser.IsSubcontractor = false;

            return newUser;
        }

        protected UserDTO _CreateNewUserSaved()
        {
            UserDTO newUser = this._CreateNewUserNotSaved();

            newUser = _userDL.SaveUser(newUser);

            Assert.IsNotNull(newUser);

            return newUser;
        }

        /// <summary>
        /// override cleanup method and do not allow anyone to adjust it from here down
        /// </summary>
        sealed override public void Cleanup()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();


            sw.Stop();
            Console.WriteLine("entire delete mock took " + sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// If the last test didn't pass, make sure we set our state to not initialized
        /// so the next test gets a fair shot.
        /// </summary>
        [TestCleanup()]
        override public void TestCleanup()
        {
            if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
            {
                _INITIALIZED = false;
            }
        }

        #region init names
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static MOQLoaderObject()
        {
            // NOTE: jim - I tried and tried to get these values in a file to be read in ... it worked great locally (After hours of trying to get
            // the files copied to the output file where the test executed.  I could not get it to run on TFS, the files would not copy to the folder
            // where the build lives.  So I just bit the bullet and put the names directly in her.  I am so sorry.  :(

            // load the user names into our array from file
            string[] fnames = {"AARON",
                            "ABDUL",
                            "ABE",
                            "ABEL",
                            "ABRAHAM",
                            "ABRAM",
                            "ADALBERTO",
                            "ADAM",
                            "ADAN",
                            "ADOLFO",
                            "ADOLPH",
                            "ADRIAN",
                            "AGUSTIN",
                            "AHMAD",
                            "AHMED",
                            "AL",
                            "ALAN",
                            "ALBERT",
                            "ALBERTO",
                            "ALDEN",
                            "ALDO",
                            "ALEC",
                            "ALEJANDRO",
                            "ALEX",
                            "ALEXANDER",
                            "ALEXIS",
                            "ALFONSO",
                            "ALFONZO",
                            "ALFRED",
                            "ALFREDO",
                            "ALI",
                            "ALLAN",
                            "ALLEN",
                            "ALONSO",
                            "ALONZO",
                            "ALPHONSE",
                            "ALPHONSO",
                            "ALTON",
                            "ALVA",
                            "ALVARO",
                            "ALVIN",
                            "AMADO",
                            "AMBROSE",
                            "AMOS",
                            "ANDERSON",
                            "ANDRE",
                            "ANDREA",
                            "ANDREAS",
                            "ANDRES",
                            "ANDREW",
                            "ANDY",
                            "ANGEL",
                            "ANGELO",
                            "ANIBAL",
                            "ANTHONY",
                            "ANTIONE",
                            "ANTOINE",
                            "ANTON",
                            "ANTONE",
                            "ANTONIA",
                            "ANTONIO",
                            "ANTONY",
                            "ANTWAN",
                            "ARCHIE",
                            "ARDEN",
                            "ARIEL",
                            "ARLEN",
                            "ARLIE",
                            "ARMAND",
                            "ARMANDO",
                            "ARNOLD",
                            "ARNOLDO",
                            "ARNULFO",
                            "ARON",
                            "ARRON",
                            "ART",
                            "ARTHUR",
                            "ARTURO",
                            "ASA",
                            "ASHLEY",
                            "AUBREY",
                            "AUGUST",
                            "AUGUSTINE",
                            "AUGUSTUS",
                            "AURELIO",
                            "AUSTIN",
                            "AVERY",
                            "BARNEY",
                            "BARRETT",
                            "BARRY",
                            "BART",
                            "BARTON",
                            "BASIL",
                            "BEAU",
                            "BEN",
                            "BENEDICT",
                            "BENITO",
                            "BENJAMIN",
                            "BENNETT",
                            "BENNIE",
                            "BENNY",
                            "BENTON",
                            "BERNARD",
                            "BERNARDO",
                            "BERNIE",
                            "BERRY",
                            "BERT",
                            "BERTRAM",
                            "BILL",
                            "BILLIE",
                            "BILLY",
                            "BLAINE",
                            "BLAIR",
                            "BLAKE",
                            "BO",
                            "BOB",
                            "BOBBIE",
                            "BOBBY",
                            "BOOKER",
                            "BORIS",
                            "BOYCE",
                            "BOYD",
                            "BRAD",
                            "BRADFORD",
                            "BRADLEY",
                            "BRADLY",
                            "BRADY",
                            "BRAIN",
                            "BRANDEN",
                            "BRANDON",
                            "BRANT",
                            "BRENDAN",
                            "BRENDON",
                            "BRENT",
                            "BRENTON",
                            "BRET",
                            "BRETT",
                            "BRIAN",
                            "BRICE",
                            "BRITT",
                            "BROCK",
                            "BRODERICK",
                            "BROOKS",
                            "BRUCE",
                            "BRUNO",
                            "BRYAN",
                            "BRYANT",
                            "BRYCE",
                            "BRYON",
                            "BUCK",
                            "BUD",
                            "BUDDY",
                            "BUFORD",
                            "BURL",
                            "BURT",
                            "BURTON",
                            "BUSTER",
                            "BYRON",
                            "CALEB",
                            "CALVIN",
                            "CAMERON",
                            "CAREY",
                            "CARL",
                            "CARLO",
                            "CARLOS",
                            "CARLTON",
                            "CARMELO",
                            "CARMEN",
                            "CARMINE",
                            "CAROL",
                            "CARROL",
                            "CARROLL",
                            "CARSON",
                            "CARTER",
                            "CARY",
                            "CASEY",
                            "CECIL",
                            "CEDRIC",
                            "CEDRICK",
                            "CESAR",
                            "CHAD",
                            "CHADWICK",
                            "CHANCE",
                            "CHANG",
                            "CHARLES",
                            "CHARLEY",
                            "CHARLIE",
                            "CHAS",
                            "CHASE",
                            "CHAUNCEY",
                            "CHESTER",
                            "CHET",
                            "CHI",
                            "CHONG",
                            "CHRIS",
                            "CHRISTIAN",
                            "CHRISTOPER",
                            "CHRISTOPHER",
                            "CHUCK",
                            "CHUNG",
                            "CLAIR",
                            "CLARENCE",
                            "CLARK",
                            "CLAUD",
                            "CLAUDE",
                            "CLAUDIO",
                            "CLAY",
                            "CLAYTON",
                            "CLEMENT",
                            "CLEMENTE",
                            "CLEO",
                            "CLETUS",
                            "CLEVELAND",
                            "CLIFF",
                            "CLIFFORD",
                            "CLIFTON",
                            "CLINT",
                            "CLINTON",
                            "CLYDE",
                            "CODY",
                            "COLBY",
                            "COLE",
                            "COLEMAN",
                            "COLIN",
                            "COLLIN",
                            "COLTON",
                            "COLUMBUS",
                            "CONNIE",
                            "CONRAD",
                            "CORDELL",
                            "COREY",
                            "CORNELIUS",
                            "CORNELL",
                            "CORTEZ",
                            "CORY",
                            "COURTNEY",
                            "COY",
                            "CRAIG",
                            "CRISTOBAL",
                            "CRISTOPHER",
                            "CRUZ",
                            "CURT",
                            "CURTIS",
                            "CYRIL",
                            "CYRUS",
                            "DALE",
                            "DALLAS",
                            "DALTON",
                            "DAMIAN",
                            "DAMIEN",
                            "DAMION",
                            "DAMON",
                            "DAN",
                            "DANA",
                            "DANE",
                            "DANIAL",
                            "DANIEL",
                            "DANILO",
                            "DANNIE",
                            "DANNY",
                            "DANTE",
                            "DARELL",
                            "DAREN",
                            "DARIN",
                            "DARIO",
                            "DARIUS",
                            "DARNELL",
                            "DARON",
                            "DARREL",
                            "DARRELL",
                            "DARREN",
                            "DARRICK",
                            "DARRIN",
                            "DARRON",
                            "DARRYL",
                            "DARWIN",
                            "DARYL",
                            "DAVE",
                            "DAVID",
                            "DAVIS",
                            "DEAN",
                            "DEANDRE",
                            "DEANGELO",
                            "DEE",
                            "DEL",
                            "DELBERT",
                            "DELMAR",
                            "DELMER",
                            "DEMARCUS",
                            "DEMETRIUS",
                            "DENIS",
                            "DENNIS",
                            "DENNY",
                            "DENVER",
                            "DEON",
                            "DEREK",
                            "DERICK",
                            "DERRICK",
                            "DESHAWN",
                            "DESMOND",
                            "DEVIN",
                            "DEVON",
                            "DEWAYNE",
                            "DEWEY",
                            "DEWITT",
                            "DEXTER",
                            "DICK",
                            "DIEGO",
                            "DILLON",
                            "DINO",
                            "DION",
                            "DIRK",
                            "DOMENIC",
                            "DOMINGO",
                            "DOMINIC",
                            "DOMINICK",
                            "DOMINIQUE",
                            "DON",
                            "DONALD",
                            "DONG",
                            "DONN",
                            "DONNELL",
                            "DONNIE",
                            "DONNY",
                            "DONOVAN",
                            "DONTE",
                            "DORIAN",
                            "DORSEY",
                            "DOUG",
                            "DOUGLAS",
                            "DOUGLASS",
                            "DOYLE",
                            "DREW",
                            "DUANE",
                            "DUDLEY",
                            "DUNCAN",
                            "DUSTIN",
                            "DUSTY",
                            "DWAIN",
                            "DWAYNE",
                            "DWIGHT",
                            "DYLAN",
                            "EARL",
                            "EARLE",
                            "EARNEST",
                            "ED",
                            "EDDIE",
                            "EDDY",
                            "EDGAR",
                            "EDGARDO",
                            "EDISON",
                            "EDMOND",
                            "EDMUND",
                            "EDMUNDO",
                            "EDUARDO",
                            "EDWARD",
                            "EDWARDO",
                            "EDWIN",
                            "EFRAIN",
                            "EFREN",
                            "ELBERT",
                            "ELDEN",
                            "ELDON",
                            "ELDRIDGE",
                            "ELI",
                            "ELIAS",
                            "ELIJAH",
                            "ELISEO",
                            "ELISHA",
                            "ELLIOT",
                            "ELLIOTT",
                            "ELLIS",
                            "ELLSWORTH",
                            "ELMER",
                            "ELMO",
                            "ELOY",
                            "ELROY",
                            "ELTON",
                            "ELVIN",
                            "ELVIS",
                            "ELWOOD",
                            "EMANUEL",
                            "EMERSON",
                            "EMERY",
                            "EMIL",
                            "EMILE",
                            "EMILIO",
                            "EMMANUEL",
                            "EMMETT",
                            "EMMITT",
                            "EMORY",
                            "ENOCH",
                            "ENRIQUE",
                            "ERASMO",
                            "ERIC",
                            "ERICH",
                            "ERICK",
                            "ERIK",
                            "ERIN",
                            "ERNEST",
                            "ERNESTO",
                            "ERNIE",
                            "ERROL",
                            "ERVIN",
                            "ERWIN",
                            "ESTEBAN",
                            "ETHAN",
                            "EUGENE",
                            "EUGENIO",
                            "EUSEBIO",
                            "EVAN",
                            "EVERETT",
                            "EVERETTE",
                            "EZEKIEL",
                            "EZEQUIEL",
                            "EZRA",
                            "FABIAN",
                            "FAUSTINO",
                            "FAUSTO",
                            "FEDERICO",
                            "FELIPE",
                            "FELIX",
                            "FELTON",
                            "FERDINAND",
                            "FERMIN",
                            "FERNANDO",
                            "FIDEL",
                            "FILIBERTO",
                            "FLETCHER",
                            "FLORENCIO",
                            "FLORENTINO",
                            "FLOYD",
                            "FOREST",
                            "FORREST",
                            "FOSTER",
                            "FRANCES",
                            "FRANCESCO",
                            "FRANCIS",
                            "FRANCISCO",
                            "FRANK",
                            "FRANKIE",
                            "FRANKLIN",
                            "FRANKLYN",
                            "FRED",
                            "FREDDIE",
                            "FREDDY",
                            "FREDERIC",
                            "FREDERICK",
                            "FREDRIC",
                            "FREDRICK",
                            "FREEMAN",
                            "FRITZ",
                            "GABRIEL",
                            "GAIL",
                            "GALE",
                            "GALEN",
                            "GARFIELD",
                            "GARLAND",
                            "GARRET",
                            "GARRETT",
                            "GARRY",
                            "GARTH",
                            "GARY",
                            "GASTON",
                            "GAVIN",
                            "GAYLE",
                            "GAYLORD",
                            "GENARO",
                            "GENE",
                            "GEOFFREY",
                            "GEORGE",
                            "GERALD",
                            "GERALDO",
                            "GERARD",
                            "GERARDO",
                            "GERMAN",
                            "GERRY",
                            "GIL",
                            "GILBERT",
                            "GILBERTO",
                            "GINO",
                            "GIOVANNI",
                            "GIUSEPPE",
                            "GLEN",
                            "GLENN",
                            "GONZALO",
                            "GORDON",
                            "GRADY",
                            "GRAHAM",
                            "GRAIG",
                            "GRANT",
                            "GRANVILLE",
                            "GREG",
                            "GREGG",
                            "GREGORIO",
                            "GREGORY",
                            "GROVER",
                            "GUADALUPE",
                            "GUILLERMO",
                            "GUS",
                            "GUSTAVO",
                            "GUY",
                            "HAI",
                            "HAL",
                            "HANK",
                            "HANS",
                            "HARLAN",
                            "HARLAND",
                            "HARLEY",
                            "HAROLD",
                            "HARRIS",
                            "HARRISON",
                            "HARRY",
                            "HARVEY",
                            "HASSAN",
                            "HAYDEN",
                            "HAYWOOD",
                            "HEATH",
                            "HECTOR",
                            "HENRY",
                            "HERB",
                            "HERBERT",
                            "HERIBERTO",
                            "HERMAN",
                            "HERSCHEL",
                            "HERSHEL",
                            "HILARIO",
                            "HILTON",
                            "HIPOLITO",
                            "HIRAM",
                            "HOBERT",
                            "HOLLIS",
                            "HOMER",
                            "HONG",
                            "HORACE",
                            "HORACIO",
                            "HOSEA",
                            "HOUSTON",
                            "HOWARD",
                            "HOYT",
                            "HUBERT",
                            "HUEY",
                            "HUGH",
                            "HUGO",
                            "HUMBERTO",
                            "HUNG",
                            "HUNTER",
                            "HYMAN",
                            "IAN",
                            "IGNACIO",
                            "IKE",
                            "IRA",
                            "IRVIN",
                            "IRVING",
                            "IRWIN",
                            "ISAAC",
                            "ISAIAH",
                            "ISAIAS",
                            "ISIAH",
                            "ISIDRO",
                            "ISMAEL",
                            "ISRAEL",
                            "ISREAL",
                            "ISSAC",
                            "IVAN",
                            "IVORY",
                            "JACINTO",
                            "JACK",
                            "JACKIE",
                            "JACKSON",
                            "JACOB",
                            "JACQUES",
                            "JAE",
                            "JAIME",
                            "JAKE",
                            "JAMAAL",
                            "JAMAL",
                            "JAMAR",
                            "JAME",
                            "JAMEL",
                            "JAMES",
                            "JAMEY",
                            "JAMIE",
                            "JAMISON",
                            "JAN",
                            "JARED",
                            "JAROD",
                            "JARRED",
                            "JARRETT",
                            "JARROD",
                            "JARVIS",
                            "JASON",
                            "JASPER",
                            "JAVIER",
                            "JAY",
                            "JAYSON",
                            "JC",
                            "JEAN",
                            "JED",
                            "JEFF",
                            "JEFFEREY",
                            "JEFFERSON",
                            "JEFFERY",
                            "JEFFREY",
                            "JEFFRY",
                            "JERALD",
                            "JERAMY",
                            "JERE",
                            "JEREMIAH",
                            "JEREMY",
                            "JERMAINE",
                            "JEROLD",
                            "JEROME",
                            "JEROMY",
                            "JERRELL",
                            "JERROD",
                            "JERROLD",
                            "JERRY",
                            "JESS",
                            "JESSE",
                            "JESSIE",
                            "JESUS",
                            "JEWEL",
                            "JEWELL",
                            "JIM",
                            "JIMMIE",
                            "JIMMY",
                            "JOAN",
                            "JOAQUIN",
                            "JODY",
                            "JOE",
                            "JOEL",
                            "JOESPH",
                            "JOEY",
                            "JOHN",
                            "JOHNATHAN",
                            "JOHNATHON",
                            "JOHNIE",
                            "JOHNNIE",
                            "JOHNNY",
                            "JOHNSON",
                            "JON",
                            "JONAH",
                            "JONAS",
                            "JONATHAN",
                            "JONATHON",
                            "JORDAN",
                            "JORDON",
                            "JORGE",
                            "JOSE",
                            "JOSEF",
                            "JOSEPH",
                            "JOSH",
                            "JOSHUA",
                            "JOSIAH",
                            "JOSPEH",
                            "JOSUE",
                            "JUAN",
                            "JUDE",
                            "JUDSON",
                            "JULES",
                            "JULIAN",
                            "JULIO",
                            "JULIUS",
                            "JUNIOR",
                            "JUSTIN",
                            "KAREEM",
                            "KARL",
                            "KASEY",
                            "KEENAN",
                            "KEITH",
                            "KELLEY",
                            "KELLY",
                            "KELVIN",
                            "KEN",
                            "KENDALL",
                            "KENDRICK",
                            "KENETH",
                            "KENNETH",
                            "KENNITH",
                            "KENNY",
                            "KENT",
                            "KENTON",
                            "KERMIT",
                            "KERRY",
                            "KEVEN",
                            "KEVIN",
                            "KIETH",
                            "KIM",
                            "KING",
                            "KIP",
                            "KIRBY",
                            "KIRK",
                            "KOREY",
                            "KORY",
                            "KRAIG",
                            "KRIS",
                            "KRISTOFER",
                            "KRISTOPHER",
                            "KURT",
                            "KURTIS",
                            "KYLE",
                            "LACY",
                            "LAMAR",
                            "LAMONT",
                            "LANCE",
                            "LANDON",
                            "LANE",
                            "LANNY",
                            "LARRY",
                            "LAUREN",
                            "LAURENCE",
                            "LAVERN",
                            "LAVERNE",
                            "LAWERENCE",
                            "LAWRENCE",
                            "LAZARO",
                            "LEANDRO",
                            "LEE",
                            "LEIF",
                            "LEIGH",
                            "LELAND",
                            "LEMUEL",
                            "LEN",
                            "LENARD",
                            "LENNY",
                            "LEO",
                            "LEON",
                            "LEONARD",
                            "LEONARDO",
                            "LEONEL",
                            "LEOPOLDO",
                            "LEROY",
                            "LES",
                            "LESLEY",
                            "LESLIE",
                            "LESTER",
                            "LEVI",
                            "LEWIS",
                            "LINCOLN",
                            "LINDSAY",
                            "LINDSEY",
                            "LINO",
                            "LINWOOD",
                            "LIONEL",
                            "LLOYD",
                            "LOGAN",
                            "LON",
                            "LONG",
                            "LONNIE",
                            "LONNY",
                            "LOREN",
                            "LORENZO",
                            "LOU",
                            "LOUIE",
                            "LOUIS",
                            "LOWELL",
                            "LOYD",
                            "LUCAS",
                            "LUCIANO",
                            "LUCIEN",
                            "LUCIO",
                            "LUCIUS",
                            "LUIGI",
                            "LUIS",
                            "LUKE",
                            "LUPE",
                            "LUTHER",
                            "LYLE",
                            "LYMAN",
                            "LYNDON",
                            "LYNN",
                            "LYNWOOD",
                            "MAC",
                            "MACK",
                            "MAJOR",
                            "MALCOLM",
                            "MALCOM",
                            "MALIK",
                            "MAN",
                            "MANUAL",
                            "MANUEL",
                            "MARC",
                            "MARCEL",
                            "MARCELINO",
                            "MARCELLUS",
                            "MARCELO",
                            "MARCO",
                            "MARCOS",
                            "MARCUS",
                            "MARGARITO",
                            "MARIA",
                            "MARIANO",
                            "MARIO",
                            "MARION",
                            "MARK",
                            "MARKUS",
                            "MARLIN",
                            "MARLON",
                            "MARQUIS",
                            "MARSHALL",
                            "MARTIN",
                            "MARTY",
                            "MARVIN",
                            "MARY",
                            "MASON",
                            "MATHEW",
                            "MATT",
                            "MATTHEW",
                            "MAURICE",
                            "MAURICIO",
                            "MAURO",
                            "MAX",
                            "MAXIMO",
                            "MAXWELL",
                            "MAYNARD",
                            "MCKINLEY",
                            "MEL",
                            "MELVIN",
                            "MERLE",
                            "MERLIN",
                            "MERRILL",
                            "MERVIN",
                            "MICAH",
                            "MICHAEL",
                            "MICHAL",
                            "MICHALE",
                            "MICHEAL",
                            "MICHEL",
                            "MICKEY",
                            "MIGUEL",
                            "MIKE",
                            "MIKEL",
                            "MILAN",
                            "MILES",
                            "MILFORD",
                            "MILLARD",
                            "MILO",
                            "MILTON",
                            "MINH",
                            "MIQUEL",
                            "MITCH",
                            "MITCHEL",
                            "MITCHELL",
                            "MODESTO",
                            "MOHAMED",
                            "MOHAMMAD",
                            "MOHAMMED",
                            "MOISES",
                            "MONROE",
                            "MONTE",
                            "MONTY",
                            "MORGAN",
                            "MORRIS",
                            "MORTON",
                            "MOSE",
                            "MOSES",
                            "MOSHE",
                            "MURRAY",
                            "MYLES",
                            "MYRON",
                            "NAPOLEON",
                            "NATHAN",
                            "NATHANAEL",
                            "NATHANIAL",
                            "NATHANIEL",
                            "NEAL",
                            "NED",
                            "NEIL",
                            "NELSON",
                            "NESTOR",
                            "NEVILLE",
                            "NEWTON",
                            "NICHOLAS",
                            "NICK",
                            "NICKOLAS",
                            "NICKY",
                            "NICOLAS",
                            "NIGEL",
                            "NOAH",
                            "NOBLE",
                            "NOE",
                            "NOEL",
                            "NOLAN",
                            "NORBERT",
                            "NORBERTO",
                            "NORMAN",
                            "NORMAND",
                            "NORRIS",
                            "NUMBERS",
                            "OCTAVIO",
                            "ODELL",
                            "ODIS",
                            "OLEN",
                            "OLIN",
                            "OLIVER",
                            "OLLIE",
                            "OMAR",
                            "OMER",
                            "OREN",
                            "ORLANDO",
                            "ORVAL",
                            "ORVILLE",
                            "OSCAR",
                            "OSVALDO",
                            "OSWALDO",
                            "OTHA",
                            "OTIS",
                            "OTTO",
                            "OWEN",
                            "PABLO",
                            "PALMER",
                            "PARIS",
                            "PARKER",
                            "PASQUALE",
                            "PAT",
                            "PATRICIA",
                            "PATRICK",
                            "PAUL",
                            "PEDRO",
                            "PERCY",
                            "PERRY",
                            "PETE",
                            "PETER",
                            "PHIL",
                            "PHILIP",
                            "PHILLIP",
                            "PIERRE",
                            "PORFIRIO",
                            "PORTER",
                            "PRESTON",
                            "PRINCE",
                            "QUENTIN",
                            "QUINCY",
                            "QUINN",
                            "QUINTIN",
                            "QUINTON",
                            "RAFAEL",
                            "RALEIGH",
                            "RALPH",
                            "RAMIRO",
                            "RAMON",
                            "RANDAL",
                            "RANDALL",
                            "RANDELL",
                            "RANDOLPH",
                            "RANDY",
                            "RAPHAEL",
                            "RASHAD",
                            "RAUL",
                            "RAY",
                            "RAYFORD",
                            "RAYMON",
                            "RAYMOND",
                            "RAYMUNDO",
                            "REED",
                            "REFUGIO",
                            "REGGIE",
                            "REGINALD",
                            "REID",
                            "REINALDO",
                            "RENALDO",
                            "RENATO",
                            "RENE",
                            "REUBEN",
                            "REX",
                            "REY",
                            "REYES",
                            "REYNALDO",
                            "RHETT",
                            "RICARDO",
                            "RICH",
                            "RICHARD",
                            "RICHIE",
                            "RICK",
                            "RICKEY",
                            "RICKIE",
                            "RICKY",
                            "RICO",
                            "RIGOBERTO",
                            "RILEY",
                            "ROB",
                            "ROBBIE",
                            "ROBBY",
                            "ROBERT",
                            "ROBERTO",
                            "ROBIN",
                            "ROBT",
                            "ROCCO",
                            "ROCKY",
                            "ROD",
                            "RODERICK",
                            "RODGER",
                            "RODNEY",
                            "RODOLFO",
                            "RODRICK",
                            "RODRIGO",
                            "ROGELIO",
                            "ROGER",
                            "ROLAND",
                            "ROLANDO",
                            "ROLF",
                            "ROLLAND",
                            "ROMAN",
                            "ROMEO",
                            "RON",
                            "RONALD",
                            "RONNIE",
                            "RONNY",
                            "ROOSEVELT",
                            "RORY",
                            "ROSARIO",
                            "ROSCOE",
                            "ROSENDO",
                            "ROSS",
                            "ROY",
                            "ROYAL",
                            "ROYCE",
                            "RUBEN",
                            "RUBIN",
                            "RUDOLF",
                            "RUDOLPH",
                            "RUDY",
                            "RUEBEN",
                            "RUFUS",
                            "RUPERT",
                            "RUSS",
                            "RUSSEL",
                            "RUSSELL",
                            "RUSTY",
                            "RYAN",
                            "SAL",
                            "SALVADOR",
                            "SALVATORE",
                            "SAM",
                            "SAMMIE",
                            "SAMMY",
                            "SAMUAL",
                            "SAMUEL",
                            "SANDY",
                            "SANFORD",
                            "SANG",
                            "SANTIAGO",
                            "SANTO",
                            "SANTOS",
                            "SAUL",
                            "SCOT",
                            "SCOTT",
                            "SCOTTIE",
                            "SCOTTY",
                            "SEAN",
                            "SEBASTIAN",
                            "SERGIO",
                            "SETH",
                            "SEYMOUR",
                            "SHAD",
                            "SHANE",
                            "SHANNON",
                            "SHAUN",
                            "SHAWN",
                            "SHAYNE",
                            "SHELBY",
                            "SHELDON",
                            "SHELTON",
                            "SHERMAN",
                            "SHERWOOD",
                            "SHIRLEY",
                            "SHON",
                            "SID",
                            "SIDNEY",
                            "SILAS",
                            "SIMON",
                            "SOL",
                            "SOLOMON",
                            "SON",
                            "SONNY",
                            "SPENCER",
                            "STACEY",
                            "STACY",
                            "STAN",
                            "STANFORD",
                            "STANLEY",
                            "STANTON",
                            "STEFAN",
                            "STEPHAN",
                            "STEPHEN",
                            "STERLING",
                            "STEVE",
                            "STEVEN",
                            "STEVIE",
                            "STEWART",
                            "STUART",
                            "SUNG",
                            "SYDNEY",
                            "SYLVESTER",
                            "TAD",
                            "TANNER",
                            "TAYLOR",
                            "TED",
                            "TEDDY",
                            "TEODORO",
                            "TERENCE",
                            "TERRANCE",
                            "TERRELL",
                            "TERRENCE",
                            "TERRY",
                            "THAD",
                            "THADDEUS",
                            "THANH",
                            "THEO",
                            "THEODORE",
                            "THERON",
                            "THOMAS",
                            "THURMAN",
                            "TIM",
                            "TIMMY",
                            "TIMOTHY",
                            "TITUS",
                            "TOBIAS",
                            "TOBY",
                            "TOD",
                            "TODD",
                            "TOM",
                            "TOMAS",
                            "TOMMIE",
                            "TOMMY",
                            "TONEY",
                            "TONY",
                            "TORY",
                            "TRACEY",
                            "TRACY",
                            "TRAVIS",
                            "TRENT",
                            "TRENTON",
                            "TREVOR",
                            "TREY",
                            "TRINIDAD",
                            "TRISTAN",
                            "TROY",
                            "TRUMAN",
                            "TUAN",
                            "TY",
                            "TYLER",
                            "TYREE",
                            "TYRELL",
                            "TYRON",
                            "TYRONE",
                            "TYSON",
                            "ULYSSES",
                            "VAL",
                            "VALENTIN",
                            "VALENTINE",
                            "VAN",
                            "VANCE",
                            "VAUGHN",
                            "VERN",
                            "VERNON",
                            "VICENTE",
                            "VICTOR",
                            "VINCE",
                            "VINCENT",
                            "VINCENZO",
                            "VIRGIL",
                            "VIRGILIO",
                            "VITO",
                            "VON",
                            "WADE",
                            "WALDO",
                            "WALKER",
                            "WALLACE",
                            "WALLY",
                            "WALTER",
                            "WALTON",
                            "WARD",
                            "WARNER",
                            "WARREN",
                            "WAYLON",
                            "WAYNE",
                            "WELDON",
                            "WENDELL",
                            "WERNER",
                            "WES",
                            "WESLEY",
                            "WESTON",
                            "WHITNEY",
                            "WILBER",
                            "WILBERT",
                            "WILBUR",
                            "WILBURN",
                            "WILEY",
                            "WILFORD",
                            "WILFRED",
                            "WILFREDO",
                            "WILL",
                            "WILLARD",
                            "WILLIAM",
                            "WILLIAMS",
                            "WILLIAN",
                            "WILLIE",
                            "WILLIS",
                            "WILLY",
                            "WILMER",
                            "WILSON",
                            "WILTON",
                            "WINFORD",
                            "WINFRED",
                            "WINSTON",
                            "WM",
                            "WOODROW",
                            "WYATT",
                            "XAVIER",
                            "YONG",
                            "YOUNG",
                            "ZACHARIAH",
                            "ZACHARY",
                            "ZACHERY",
                            "ZACK",
                            "ZACKARY",
                            "ZANE",
                            "MARY",
                            "PATRICIA",
                            "LINDA",
                            "BARBARA",
                            "ELIZABETH",
                            "JENNIFER",
                            "MARIA",
                            "SUSAN",
                            "MARGARET",
                            "DOROTHY",
                            "LISA",
                            "NANCY",
                            "KAREN",
                            "BETTY",
                            "HELEN",
                            "SANDRA",
                            "DONNA",
                            "CAROL",
                            "RUTH",
                            "SHARON",
                            "MICHELLE",
                            "LAURA",
                            "SARAH",
                            "KIMBERLY",
                            "DEBORAH",
                            "JESSICA",
                            "SHIRLEY",
                            "CYNTHIA",
                            "ANGELA",
                            "MELISSA",
                            "BRENDA",
                            "AMY",
                            "ANNA",
                            "REBECCA",
                            "VIRGINIA",
                            "KATHLEEN",
                            "PAMELA",
                            "MARTHA",
                            "DEBRA",
                            "AMANDA",
                            "STEPHANIE",
                            "CAROLYN",
                            "CHRISTINE",
                            "MARIE",
                            "JANET",
                            "CATHERINE",
                            "FRANCES",
                            "ANN",
                            "JOYCE",
                            "DIANE",
                            "ALICE",
                            "JULIE",
                            "HEATHER",
                            "TERESA",
                            "DORIS",
                            "GLORIA",
                            "EVELYN",
                            "JEAN",
                            "CHERYL",
                            "MILDRED",
                            "KATHERINE",
                            "JOAN",
                            "ASHLEY",
                            "JUDITH",
                            "ROSE",
                            "JANICE",
                            "KELLY",
                            "NICOLE",
                            "JUDY",
                            "CHRISTINA",
                            "KATHY",
                            "THERESA",
                            "BEVERLY",
                            "DENISE",
                            "TAMMY",
                            "IRENE",
                            "JANE",
                            "LORI",
                            "RACHEL",
                            "MARILYN",
                            "ANDREA",
                            "KATHRYN",
                            "LOUISE",
                            "SARA",
                            "ANNE",
                            "JACQUELINE",
                            "WANDA",
                            "BONNIE",
                            "JULIA",
                            "RUBY",
                            "LOIS",
                            "TINA",
                            "PHYLLIS",
                            "NORMA",
                            "PAULA",
                            "DIANA",
                            "ANNIE",
                            "LILLIAN",
                            "EMILY",
                            "ROBIN",
                            "PEGGY",
                            "CRYSTAL",
                            "GLADYS",
                            "RITA",
                            "DAWN",
                            "CONNIE",
                            "FLORENCE",
                            "TRACY",
                            "EDNA",
                            "TIFFANY",
                            "CARMEN",
                            "ROSA",
                            "CINDY",
                            "GRACE",
                            "WENDY",
                            "VICTORIA",
                            "EDITH",
                            "KIM",
                            "SHERRY",
                            "SYLVIA",
                            "JOSEPHINE",
                            "THELMA",
                            "SHANNON",
                            "SHEILA",
                            "ETHEL",
                            "ELLEN",
                            "ELAINE",
                            "MARJORIE",
                            "CARRIE",
                            "CHARLOTTE",
                            "MONICA",
                            "ESTHER",
                            "PAULINE",
                            "EMMA",
                            "JUANITA",
                            "ANITA",
                            "RHONDA",
                            "HAZEL",
                            "AMBER",
                            "EVA",
                            "DEBBIE",
                            "APRIL",
                            "LESLIE",
                            "CLARA",
                            "LUCILLE",
                            "JAMIE",
                            "JOANNE",
                            "ELEANOR",
                            "VALERIE",
                            "DANIELLE",
                            "MEGAN",
                            "ALICIA",
                            "SUZANNE",
                            "MICHELE",
                            "GAIL",
                            "BERTHA",
                            "DARLENE",
                            "VERONICA",
                            "JILL",
                            "ERIN",
                            "GERALDINE",
                            "LAUREN",
                            "CATHY",
                            "JOANN",
                            "LORRAINE",
                            "LYNN",
                            "SALLY",
                            "REGINA",
                            "ERICA",
                            "BEATRICE",
                            "DOLORES",
                            "BERNICE",
                            "AUDREY",
                            "YVONNE",
                            "ANNETTE",
                            "JUNE",
                            "SAMANTHA",
                            "MARION",
                            "DANA",
                            "STACY",
                            "ANA",
                            "RENEE",
                            "IDA",
                            "VIVIAN",
                            "ROBERTA",
                            "HOLLY",
                            "BRITTANY",
                            "MELANIE",
                            "LORETTA",
                            "YOLANDA",
                            "JEANETTE",
                            "LAURIE",
                            "KATIE",
                            "KRISTEN",
                            "VANESSA",
                            "ALMA",
                            "SUE",
                            "ELSIE",
                            "BETH",
                            "JEANNE",
                            "VICKI",
                            "CARLA",
                            "TARA",
                            "ROSEMARY",
                            "EILEEN",
                            "TERRI",
                            "GERTRUDE",
                            "LUCY",
                            "TONYA",
                            "ELLA",
                            "STACEY",
                            "WILMA",
                            "GINA",
                            "KRISTIN",
                            "JESSIE",
                            "NATALIE",
                            "AGNES",
                            "VERA",
                            "WILLIE",
                            "CHARLENE",
                            "BESSIE",
                            "DELORES",
                            "MELINDA",
                            "PEARL",
                            "ARLENE",
                            "MAUREEN",
                            "COLLEEN",
                            "ALLISON",
                            "TAMARA",
                            "JOY",
                            "GEORGIA",
                            "CONSTANCE",
                            "LILLIE",
                            "CLAUDIA",
                            "JACKIE",
                            "MARCIA",
                            "TANYA",
                            "NELLIE",
                            "MINNIE",
                            "MARLENE",
                            "HEIDI",
                            "GLENDA",
                            "LYDIA",
                            "VIOLA",
                            "COURTNEY",
                            "MARIAN",
                            "STELLA",
                            "CAROLINE",
                            "DORA",
                            "JO",
                            "VICKIE",
                            "MATTIE",
                            "TERRY",
                            "MAXINE",
                            "IRMA",
                            "MABEL",
                            "MARSHA",
                            "MYRTLE",
                            "LENA",
                            "CHRISTY",
                            "DEANNA",
                            "PATSY",
                            "HILDA",
                            "GWENDOLYN",
                            "JENNIE",
                            "NORA",
                            "MARGIE",
                            "NINA",
                            "CASSANDRA",
                            "LEAH",
                            "PENNY",
                            "KAY",
                            "PRISCILLA",
                            "NAOMI",
                            "CAROLE",
                            "BRANDY",
                            "OLGA",
                            "BILLIE",
                            "DIANNE",
                            "TRACEY",
                            "LEONA",
                            "JENNY",
                            "FELICIA",
                            "SONIA",
                            "MIRIAM",
                            "VELMA",
                            "BECKY",
                            "BOBBIE",
                            "VIOLET",
                            "KRISTINA",
                            "TONI",
                            "MISTY",
                            "MAE",
                            "SHELLY",
                            "DAISY",
                            "RAMONA",
                            "SHERRI",
                            "ERIKA",
                            "KATRINA",
                            "CLAIRE",
                            "LINDSEY",
                            "LINDSAY",
                            "GENEVA",
                            "GUADALUPE",
                            "BELINDA",
                            "MARGARITA",
                            "SHERYL",
                            "CORA",
                            "FAYE",
                            "ADA",
                            "NATASHA",
                            "SABRINA",
                            "ISABEL",
                            "MARGUERITE",
                            "HATTIE",
                            "HARRIET",
                            "MOLLY",
                            "CECILIA",
                            "KRISTI",
                            "BRANDI",
                            "BLANCHE",
                            "SANDY",
                            "ROSIE",
                            "JOANNA",
                            "IRIS",
                            "EUNICE",
                            "ANGIE",
                            "INEZ",
                            "LYNDA",
                            "MADELINE",
                            "AMELIA",
                            "ALBERTA",
                            "GENEVIEVE",
                            "MONIQUE",
                            "JODI",
                            "JANIE",
                            "MAGGIE",
                            "KAYLA",
                            "SONYA",
                            "JAN",
                            "LEE",
                            "KRISTINE",
                            "CANDACE",
                            "FANNIE",
                            "MARYANN",
                            "OPAL",
                            "ALISON",
                            "YVETTE",
                            "MELODY",
                            "LUZ",
                            "SUSIE",
                            "OLIVIA",
                            "FLORA",
                            "SHELLEY",
                            "KRISTY",
                            "MAMIE",
                            "LULA",
                            "LOLA",
                            "VERNA",
                            "BEULAH",
                            "ANTOINETTE",
                            "CANDICE",
                            "JUANA",
                            "JEANNETTE",
                            "PAM",
                            "KELLI",
                            "HANNAH",
                            "WHITNEY",
                            "BRIDGET",
                            "KARLA",
                            "CELIA",
                            "LATOYA",
                            "PATTY",
                            "SHELIA",
                            "GAYLE",
                            "DELLA",
                            "VICKY",
                            "LYNNE",
                            "SHERI",
                            "MARIANNE",
                            "KARA",
                            "JACQUELYN",
                            "ERMA",
                            "BLANCA",
                            "MYRA",
                            "LETICIA",
                            "PAT",
                            "KRISTA",
                            "ROXANNE",
                            "ANGELICA",
                            "JOHNNIE",
                            "ROBYN",
                            "FRANCIS",
                            "ADRIENNE",
                            "ROSALIE",
                            "ALEXANDRA",
                            "BROOKE",
                            "BETHANY",
                            "SADIE",
                            "BERNADETTE",
                            "TRACI",
                            "JODY",
                            "KENDRA",
                            "JASMINE",
                            "NICHOLE",
                            "RACHAEL",
                            "CHELSEA",
                            "MABLE",
                            "ERNESTINE",
                            "MURIEL",
                            "MARCELLA",
                            "ELENA",
                            "KRYSTAL",
                            "ANGELINA",
                            "NADINE",
                            "KARI",
                            "ESTELLE",
                            "DIANNA",
                            "PAULETTE",
                            "LORA",
                            "MONA",
                            "DOREEN",
                            "ROSEMARIE",
                            "ANGEL",
                            "DESIREE",
                            "ANTONIA",
                            "HOPE",
                            "GINGER",
                            "JANIS",
                            "BETSY",
                            "CHRISTIE",
                            "FREDA",
                            "MERCEDES",
                            "MEREDITH",
                            "LYNETTE",
                            "TERI",
                            "CRISTINA",
                            "EULA",
                            "LEIGH",
                            "MEGHAN",
                            "SOPHIA",
                            "ELOISE",
                            "ROCHELLE",
                            "GRETCHEN",
                            "CECELIA",
                            "RAQUEL",
                            "HENRIETTA",
                            "ALYSSA",
                            "JANA",
                            "KELLEY",
                            "GWEN",
                            "KERRY",
                            "JENNA",
                            "TRICIA",
                            "LAVERNE",
                            "OLIVE",
                            "ALEXIS",
                            "TASHA",
                            "SILVIA",
                            "ELVIRA",
                            "CASEY",
                            "DELIA",
                            "SOPHIE",
                            "KATE",
                            "PATTI",
                            "LORENA",
                            "KELLIE",
                            "SONJA",
                            "LILA",
                            "LANA",
                            "DARLA",
                            "MAY",
                            "MINDY",
                            "ESSIE",
                            "MANDY",
                            "LORENE",
                            "ELSA",
                            "JOSEFINA",
                            "JEANNIE",
                            "MIRANDA",
                            "DIXIE",
                            "LUCIA",
                            "MARTA",
                            "FAITH",
                            "LELA",
                            "JOHANNA",
                            "SHARI",
                            "CAMILLE",
                            "TAMI",
                            "SHAWNA",
                            "ELISA",
                            "EBONY",
                            "MELBA",
                            "ORA",
                            "NETTIE",
                            "TABITHA",
                            "OLLIE",
                            "JAIME",
                            "WINIFRED",
                            "KRISTIE",
                            "MARINA",
                            "ALISHA",
                            "AIMEE",
                            "RENA",
                            "MYRNA",
                            "MARLA",
                            "TAMMIE",
                            "LATASHA",
                            "BONITA",
                            "PATRICE",
                            "RONDA",
                            "SHERRIE",
                            "ADDIE",
                            "FRANCINE",
                            "DELORIS",
                            "STACIE",
                            "ADRIANA",
                            "CHERI",
                            "SHELBY",
                            "ABIGAIL",
                            "CELESTE",
                            "JEWEL",
                            "CARA",
                            "ADELE",
                            "REBEKAH",
                            "LUCINDA",
                            "DORTHY",
                            "CHRIS",
                            "EFFIE",
                            "TRINA",
                            "REBA",
                            "SHAWN",
                            "SALLIE",
                            "AURORA",
                            "LENORA",
                            "ETTA",
                            "LOTTIE",
                            "KERRI",
                            "TRISHA",
                            "NIKKI",
                            "ESTELLA",
                            "FRANCISCA",
                            "JOSIE",
                            "TRACIE",
                            "MARISSA",
                            "KARIN",
                            "BRITTNEY",
                            "JANELLE",
                            "LOURDES",
                            "LAUREL",
                            "HELENE",
                            "FERN",
                            "ELVA",
                            "CORINNE",
                            "KELSEY",
                            "INA",
                            "BETTIE",
                            "ELISABETH",
                            "AIDA",
                            "CAITLIN",
                            "INGRID",
                            "IVA",
                            "EUGENIA",
                            "CHRISTA",
                            "GOLDIE",
                            "CASSIE",
                            "MAUDE",
                            "JENIFER",
                            "THERESE",
                            "FRANKIE",
                            "DENA",
                            "LORNA",
                            "JANETTE",
                            "LATONYA",
                            "CANDY",
                            "MORGAN",
                            "CONSUELO",
                            "TAMIKA",
                            "ROSETTA",
                            "DEBORA",
                            "CHERIE",
                            "POLLY",
                            "DINA",
                            "JEWELL",
                            "FAY",
                            "JILLIAN",
                            "DOROTHEA",
                            "NELL",
                            "TRUDY",
                            "ESPERANZA",
                            "PATRICA",
                            "KIMBERLEY",
                            "SHANNA",
                            "HELENA",
                            "CAROLINA",
                            "CLEO",
                            "STEFANIE",
                            "ROSARIO",
                            "OLA",
                            "JANINE",
                            "MOLLIE",
                            "LUPE",
                            "ALISA",
                            "LOU",
                            "MARIBEL",
                            "SUSANNE",
                            "BETTE",
                            "SUSANA",
                            "ELISE",
                            "CECILE",
                            "ISABELLE",
                            "LESLEY",
                            "JOCELYN",
                            "PAIGE",
                            "JONI",
                            "RACHELLE",
                            "LEOLA",
                            "DAPHNE",
                            "ALTA",
                            "ESTER",
                            "PETRA",
                            "GRACIELA",
                            "IMOGENE",
                            "JOLENE",
                            "KEISHA",
                            "LACEY",
                            "GLENNA",
                            "GABRIELA",
                            "KERI",
                            "URSULA",
                            "LIZZIE",
                            "KIRSTEN",
                            "SHANA",
                            "ADELINE",
                            "MAYRA",
                            "JAYNE",
                            "JACLYN",
                            "GRACIE",
                            "SONDRA",
                            "CARMELA",
                            "MARISA",
                            "ROSALIND",
                            "CHARITY",
                            "TONIA",
                            "BEATRIZ",
                            "MARISOL",
                            "CLARICE",
                            "JEANINE",
                            "SHEENA",
                            "ANGELINE",
                            "FRIEDA",
                            "LILY",
                            "ROBBIE",
                            "SHAUNA",
                            "MILLIE",
                            "CLAUDETTE",
                            "CATHLEEN",
                            "ANGELIA",
                            "GABRIELLE",
                            "AUTUMN",
                            "KATHARINE",
                            "SUMMER",
                            "JODIE",
                            "STACI",
                            "LEA",
                            "CHRISTI",
                            "JIMMIE",
                            "JUSTINE",
                            "ELMA",
                            "LUELLA",
                            "MARGRET",
                            "DOMINIQUE",
                            "SOCORRO",
                            "RENE",
                            "MARTINA",
                            "MARGO",
                            "MAVIS",
                            "CALLIE",
                            "BOBBI",
                            "MARITZA",
                            "LUCILE",
                            "LEANNE",
                            "JEANNINE",
                            "DEANA",
                            "AILEEN",
                            "LORIE",
                            "LADONNA",
                            "WILLA",
                            "MANUELA",
                            "GALE",
                            "SELMA",
                            "DOLLY",
                            "SYBIL",
                            "ABBY",
                            "LARA",
                            "DALE",
                            "IVY",
                            "DEE",
                            "WINNIE",
                            "MARCY",
                            "LUISA",
                            "JERI",
                            "MAGDALENA",
                            "OFELIA",
                            "MEAGAN",
                            "AUDRA",
                            "MATILDA",
                            "LEILA",
                            "CORNELIA",
                            "BIANCA",
                            "SIMONE",
                            "BETTYE",
                            "RANDI",
                            "VIRGIE",
                            "LATISHA",
                            "BARBRA",
                            "GEORGINA",
                            "ELIZA",
                            "LEANN",
                            "BRIDGETTE",
                            "RHODA",
                            "HALEY",
                            "ADELA",
                            "NOLA",
                            "BERNADINE",
                            "FLOSSIE",
                            "ILA",
                            "GRETA",
                            "RUTHIE",
                            "NELDA",
                            "MINERVA",
                            "LILLY",
                            "TERRIE",
                            "LETHA",
                            "HILARY",
                            "ESTELA",
                            "VALARIE",
                            "BRIANNA",
                            "ROSALYN",
                            "EARLINE",
                            "CATALINA",
                            "AVA",
                            "MIA",
                            "CLARISSA",
                            "LIDIA",
                            "CORRINE",
                            "ALEXANDRIA",
                            "CONCEPCION",
                            "TIA",
                            "SHARRON",
                            "RAE",
                            "DONA",
                            "ERICKA",
                            "JAMI",
                            "ELNORA",
                            "CHANDRA",
                            "LENORE",
                            "NEVA",
                            "MARYLOU",
                            "MELISA",
                            "TABATHA",
                            "SERENA",
                            "AVIS",
                            "ALLIE",
                            "SOFIA",
                            "JEANIE",
                            "ODESSA",
                            "NANNIE",
                            "HARRIETT",
                            "LORAINE",
                            "PENELOPE",
                            "MILAGROS",
                            "EMILIA",
                            "BENITA",
                            "ALLYSON",
                            "ASHLEE",
                            "TANIA",
                            "TOMMIE",
                            "ESMERALDA",
                            "KARINA",
                            "EVE",
                            "PEARLIE",
                            "ZELMA",
                            "MALINDA",
                            "NOREEN",
                            "TAMEKA",
                            "SAUNDRA",
                            "HILLARY",
                            "AMIE",
                            "ALTHEA",
                            "ROSALINDA",
                            "JORDAN",
                            "LILIA",
                            "ALANA",
                            "GAY",
                            "CLARE",
                            "ALEJANDRA",
                            "ELINOR",
                            "MICHAEL",
                            "LORRIE",
                            "JERRI",
                            "DARCY",
                            "EARNESTINE",
                            "CARMELLA",
                            "TAYLOR",
                            "NOEMI",
                            "MARCIE",
                            "LIZA",
                            "ANNABELLE",
                            "LOUISA",
                            "EARLENE",
                            "MALLORY",
                            "CARLENE",
                            "NITA",
                            "SELENA",
                            "TANISHA",
                            "KATY",
                            "JULIANNE",
                            "JOHN",
                            "LAKISHA",
                            "EDWINA",
                            "MARICELA",
                            "MARGERY",
                            "KENYA",
                            "DOLLIE",
                            "ROXIE",
                            "ROSLYN",
                            "KATHRINE",
                            "NANETTE",
                            "CHARMAINE",
                            "LAVONNE",
                            "ILENE",
                            "KRIS",
                            "TAMMI",
                            "SUZETTE",
                            "CORINE",
                            "KAYE",
                            "JERRY",
                            "MERLE",
                            "CHRYSTAL",
                            "LINA",
                            "DEANNE",
                            "LILIAN",
                            "JULIANA",
                            "ALINE",
                            "LUANN",
                            "KASEY",
                            "MARYANNE",
                            "EVANGELINE",
                            "COLETTE",
                            "MELVA",
                            "LAWANDA",
                            "YESENIA",
                            "NADIA",
                            "MADGE",
                            "KATHIE",
                            "EDDIE",
                            "OPHELIA",
                            "VALERIA",
                            "NONA",
                            "MITZI",
                            "MARI",
                            "GEORGETTE",
                            "CLAUDINE",
                            "FRAN",
                            "ALISSA",
                            "ROSEANN",
                            "LAKEISHA",
                            "SUSANNA",
                            "REVA",
                            "DEIDRE",
                            "CHASITY",
                            "SHEREE",
                            "CARLY",
                            "JAMES",
                            "ELVIA",
                            "ALYCE",
                            "DEIRDRE",
                            "GENA",
                            "BRIANA",
                            "ARACELI",
                            "KATELYN",
                            "ROSANNE",
                            "WENDI",
                            "TESSA",
                            "BERTA",
                            "MARVA",
                            "IMELDA",
                            "MARIETTA",
                            "MARCI",
                            "LEONOR",
                            "ARLINE",
                            "SASHA",
                            "MADELYN",
                            "JANNA",
                            "JULIETTE",
                            "DEENA",
                            "AURELIA",
                            "JOSEFA",
                            "AUGUSTA",
                            "LILIANA",
                            "YOUNG",
                            "CHRISTIAN",
                            "LESSIE",
                            "AMALIA",
                            "SAVANNAH",
                            "ANASTASIA",
                            "VILMA",
                            "NATALIA",
                            "ROSELLA",
                            "LYNNETTE",
                            "CORINA",
                            "ALFREDA",
                            "LEANNA",
                            "CAREY",
                            "AMPARO",
                            "COLEEN",
                            "TAMRA",
                            "AISHA",
                            "WILDA",
                            "KARYN",
                            "CHERRY",
                            "QUEEN",
                            "MAURA",
                            "MAI",
                            "EVANGELINA",
                            "ROSANNA",
                            "HALLIE",
                            "ERNA",
                            "ENID",
                            "MARIANA",
                            "LACY",
                            "JULIET",
                            "JACKLYN",
                            "FREIDA",
                            "MADELEINE",
                            "MARA",
                            "HESTER",
                            "CATHRYN",
                            "LELIA",
                            "CASANDRA",
                            "BRIDGETT",
                            "ANGELITA",
                            "JANNIE",
                            "DIONNE",
                            "ANNMARIE",
                            "KATINA",
                            "BERYL",
                            "PHOEBE",
                            "MILLICENT",
                            "KATHERYN",
                            "DIANN",
                            "CARISSA",
                            "MARYELLEN",
                            "LIZ",
                            "LAURI",
                            "HELGA",
                            "GILDA",
                            "ADRIAN",
                            "RHEA",
                            "MARQUITA",
                            "HOLLIE",
                            "TISHA",
                            "TAMERA",
                            "ANGELIQUE",
                            "FRANCESCA",
                            "BRITNEY",
                            "KAITLIN",
                            "LOLITA",
                            "FLORINE",
                            "ROWENA",
                            "REYNA",
                            "TWILA",
                            "FANNY",
                            "JANELL",
                            "INES",
                            "CONCETTA",
                            "BERTIE",
                            "ALBA",
                            "BRIGITTE",
                            "ALYSON",
                            "VONDA",
                            "PANSY",
                            "ELBA",
                            "NOELLE",
                            "LETITIA",
                            "KITTY",
                            "DEANN",
                            "BRANDIE",
                            "LOUELLA",
                            "LETA",
                            "FELECIA",
                            "SHARLENE",
                            "LESA",
                            "BEVERLEY",
                            "ROBERT",
                            "ISABELLA",
                            "HERMINIA",
                            "TERRA",
                            "CELINA"};

            _FirstNames = new Collection<string>(fnames);

            string[] lnames = {"SMITH",
                                "JOHNSON",
                                "WILLIAMS",
                                "JONES",
                                "BROWN",
                                "DAVIS",
                                "MILLER",
                                "WILSON",
                                "MOORE",
                                "TAYLOR",
                                "ANDERSON",
                                "THOMAS",
                                "JACKSON",
                                "WHITE",
                                "HARRIS",
                                "MARTIN",
                                "THOMPSON",
                                "GARCIA",
                                "MARTINEZ",
                                "ROBINSON",
                                "CLARK",
                                "RODRIGUEZ",
                                "LEWIS",
                                "LEE",
                                "WALKER",
                                "HALL",
                                "ALLEN",
                                "YOUNG",
                                "HERNANDEZ",
                                "KING",
                                "WRIGHT",
                                "LOPEZ",
                                "HILL",
                                "SCOTT",
                                "GREEN",
                                "ADAMS",
                                "BAKER",
                                "GONZALEZ",
                                "NELSON",
                                "CARTER",
                                "MITCHELL",
                                "PEREZ",
                                "ROBERTS",
                                "TURNER",
                                "PHILLIPS",
                                "CAMPBELL",
                                "PARKER",
                                "EVANS",
                                "EDWARDS",
                                "COLLINS",
                                "STEWART",
                                "SANCHEZ",
                                "MORRIS",
                                "ROGERS",
                                "REED",
                                "COOK",
                                "MORGAN",
                                "BELL",
                                "MURPHY",
                                "BAILEY",
                                "RIVERA",
                                "COOPER",
                                "RICHARDSON",
                                "COX",
                                "HOWARD",
                                "WARD",
                                "TORRES",
                                "PETERSON",
                                "GRAY",
                                "RAMIREZ",
                                "JAMES",
                                "WATSON",
                                "BROOKS",
                                "KELLY",
                                "SANDERS",
                                "PRICE",
                                "BENNETT",
                                "WOOD",
                                "BARNES",
                                "ROSS",
                                "HENDERSON",
                                "COLEMAN",
                                "JENKINS",
                                "PERRY",
                                "POWELL",
                                "LONG",
                                "PATTERSON",
                                "HUGHES",
                                "FLORES",
                                "WASHINGTON",
                                "BUTLER",
                                "SIMMONS",
                                "FOSTER",
                                "GONZALES",
                                "BRYANT",
                                "ALEXANDER",
                                "RUSSELL",
                                "GRIFFIN",
                                "DIAZ",
                                "HAYES",
                                "MYERS",
                                "FORD",
                                "HAMILTON",
                                "GRAHAM",
                                "SULLIVAN",
                                "WALLACE",
                                "WOODS",
                                "COLE",
                                "WEST",
                                "JORDAN",
                                "OWENS",
                                "REYNOLDS",
                                "FISHER",
                                "ELLIS",
                                "HARRISON",
                                "GIBSON",
                                "MCDONALD",
                                "CRUZ",
                                "MARSHALL",
                                "ORTIZ",
                                "GOMEZ",
                                "MURRAY",
                                "FREEMAN",
                                "WELLS",
                                "WEBB",
                                "SIMPSON",
                                "STEVENS",
                                "TUCKER",
                                "PORTER",
                                "HUNTER",
                                "HICKS",
                                "CRAWFORD",
                                "HENRY",
                                "BOYD",
                                "MASON",
                                "MORALES",
                                "KENNEDY",
                                "WARREN",
                                "DIXON",
                                "RAMOS",
                                "REYES",
                                "BURNS",
                                "GORDON",
                                "SHAW",
                                "HOLMES",
                                "RICE",
                                "ROBERTSON",
                                "HUNT",
                                "BLACK",
                                "DANIELS",
                                "PALMER",
                                "MILLS",
                                "NICHOLS",
                                "GRANT",
                                "KNIGHT",
                                "FERGUSON",
                                "ROSE",
                                "STONE",
                                "HAWKINS",
                                "DUNN",
                                "PERKINS",
                                "HUDSON",
                                "SPENCER",
                                "GARDNER",
                                "STEPHENS",
                                "PAYNE",
                                "PIERCE",
                                "BERRY",
                                "MATTHEWS",
                                "ARNOLD",
                                "WAGNER",
                                "WILLIS",
                                "RAY",
                                "WATKINS",
                                "OLSON",
                                "CARROLL",
                                "DUNCAN",
                                "SNYDER",
                                "HART",
                                "CUNNINGHAM",
                                "BRADLEY",
                                "LANE",
                                "ANDREWS",
                                "RUIZ",
                                "HARPER",
                                "FOX",
                                "RILEY",
                                "ARMSTRONG",
                                "CARPENTER",
                                "WEAVER",
                                "GREENE",
                                "LAWRENCE",
                                "ELLIOTT",
                                "CHAVEZ",
                                "SIMS",
                                "AUSTIN",
                                "PETERS",
                                "KELLEY",
                                "FRANKLIN",
                                "LAWSON",
                                "FIELDS",
                                "GUTIERREZ",
                                "RYAN",
                                "SCHMIDT",
                                "CARR",
                                "VASQUEZ",
                                "CASTILLO",
                                "WHEELER",
                                "CHAPMAN",
                                "OLIVER",
                                "MONTGOMERY",
                                "RICHARDS",
                                "WILLIAMSON",
                                "JOHNSTON",
                                "BANKS",
                                "MEYER",
                                "BISHOP",
                                "MCCOY",
                                "HOWELL",
                                "ALVAREZ",
                                "MORRISON",
                                "HANSEN",
                                "FERNANDEZ",
                                "GARZA",
                                "HARVEY",
                                "LITTLE",
                                "BURTON",
                                "STANLEY",
                                "NGUYEN",
                                "GEORGE",
                                "JACOBS",
                                "REID",
                                "KIM",
                                "FULLER",
                                "LYNCH",
                                "DEAN",
                                "GILBERT",
                                "GARRETT",
                                "ROMERO",
                                "WELCH",
                                "LARSON",
                                "FRAZIER",
                                "BURKE",
                                "HANSON",
                                "DAY",
                                "MENDOZA",
                                "MORENO",
                                "BOWMAN",
                                "MEDINA",
                                "FOWLER",
                                "BREWER",
                                "HOFFMAN",
                                "CARLSON",
                                "SILVA",
                                "PEARSON",
                                "HOLLAND",
                                "DOUGLAS",
                                "FLEMING",
                                "JENSEN",
                                "VARGAS",
                                "BYRD",
                                "DAVIDSON",
                                "HOPKINS",
                                "MAY",
                                "TERRY",
                                "HERRERA",
                                "WADE",
                                "SOTO",
                                "WALTERS",
                                "CURTIS",
                                "NEAL",
                                "CALDWELL",
                                "LOWE",
                                "JENNINGS",
                                "BARNETT",
                                "GRAVES",
                                "JIMENEZ",
                                "HORTON",
                                "SHELTON",
                                "BARRETT",
                                "OBRIEN",
                                "CASTRO",
                                "SUTTON",
                                "GREGORY",
                                "MCKINNEY",
                                "LUCAS",
                                "MILES",
                                "CRAIG",
                                "RODRIQUEZ",
                                "CHAMBERS",
                                "HOLT",
                                "LAMBERT",
                                "FLETCHER",
                                "WATTS",
                                "BATES",
                                "HALE",
                                "RHODES",
                                "PENA",
                                "BECK",
                                "NEWMAN",
                                "HAYNES",
                                "MCDANIEL",
                                "MENDEZ",
                                "BUSH",
                                "VAUGHN",
                                "PARKS",
                                "DAWSON",
                                "SANTIAGO",
                                "NORRIS",
                                "HARDY",
                                "LOVE",
                                "STEELE",
                                "CURRY",
                                "POWERS",
                                "SCHULTZ",
                                "BARKER",
                                "GUZMAN",
                                "PAGE",
                                "MUNOZ",
                                "BALL",
                                "KELLER",
                                "CHANDLER",
                                "WEBER",
                                "LEONARD",
                                "WALSH",
                                "LYONS",
                                "RAMSEY",
                                "WOLFE",
                                "SCHNEIDER",
                                "MULLINS",
                                "BENSON",
                                "SHARP",
                                "BOWEN",
                                "DANIEL",
                                "BARBER",
                                "CUMMINGS",
                                "HINES",
                                "BALDWIN",
                                "GRIFFITH",
                                "VALDEZ",
                                "HUBBARD",
                                "SALAZAR",
                                "REEVES",
                                "WARNER",
                                "STEVENSON",
                                "BURGESS",
                                "SANTOS",
                                "TATE",
                                "CROSS",
                                "GARNER",
                                "MANN",
                                "MACK",
                                "MOSS",
                                "THORNTON",
                                "DENNIS",
                                "MCGEE",
                                "FARMER",
                                "DELGADO",
                                "AGUILAR",
                                "VEGA",
                                "GLOVER",
                                "MANNING",
                                "COHEN",
                                "HARMON",
                                "RODGERS",
                                "ROBBINS",
                                "NEWTON",
                                "TODD",
                                "BLAIR",
                                "HIGGINS",
                                "INGRAM",
                                "REESE",
                                "CANNON",
                                "STRICKLAND",
                                "TOWNSEND",
                                "POTTER",
                                "GOODWIN",
                                "WALTON",
                                "ROWE",
                                "HAMPTON",
                                "ORTEGA",
                                "PATTON",
                                "SWANSON",
                                "JOSEPH",
                                "FRANCIS",
                                "GOODMAN",
                                "MALDONADO",
                                "YATES",
                                "BECKER",
                                "ERICKSON",
                                "HODGES",
                                "RIOS",
                                "CONNER",
                                "ADKINS",
                                "WEBSTER",
                                "NORMAN",
                                "MALONE",
                                "HAMMOND",
                                "FLOWERS",
                                "COBB",
                                "MOODY",
                                "QUINN",
                                "BLAKE",
                                "MAXWELL",
                                "POPE",
                                "FLOYD",
                                "OSBORNE",
                                "PAUL",
                                "MCCARTHY",
                                "GUERRERO",
                                "LINDSEY",
                                "ESTRADA",
                                "SANDOVAL",
                                "GIBBS",
                                "TYLER",
                                "GROSS",
                                "FITZGERALD",
                                "STOKES",
                                "DOYLE",
                                "SHERMAN",
                                "SAUNDERS",
                                "WISE",
                                "COLON",
                                "GILL",
                                "ALVARADO",
                                "GREER",
                                "PADILLA",
                                "SIMON",
                                "WATERS",
                                "NUNEZ",
                                "BALLARD",
                                "SCHWARTZ",
                                "MCBRIDE",
                                "HOUSTON",
                                "CHRISTENSEN",
                                "KLEIN",
                                "PRATT",
                                "BRIGGS",
                                "PARSONS",
                                "MCLAUGHLIN",
                                "ZIMMERMAN",
                                "FRENCH",
                                "BUCHANAN",
                                "MORAN",
                                "COPELAND",
                                "ROY",
                                "PITTMAN",
                                "BRADY",
                                "MCCORMICK",
                                "HOLLOWAY",
                                "BROCK",
                                "POOLE",
                                "FRANK",
                                "LOGAN",
                                "OWEN",
                                "BASS",
                                "MARSH",
                                "DRAKE",
                                "WONG",
                                "JEFFERSON",
                                "PARK",
                                "MORTON",
                                "ABBOTT",
                                "SPARKS",
                                "PATRICK",
                                "NORTON",
                                "HUFF",
                                "CLAYTON",
                                "MASSEY",
                                "LLOYD",
                                "FIGUEROA",
                                "CARSON",
                                "BOWERS",
                                "ROBERSON",
                                "BARTON",
                                "TRAN",
                                "LAMB",
                                "HARRINGTON",
                                "CASEY",
                                "BOONE",
                                "CORTEZ",
                                "CLARKE",
                                "MATHIS",
                                "SINGLETON",
                                "WILKINS",
                                "CAIN",
                                "BRYAN",
                                "UNDERWOOD",
                                "HOGAN",
                                "MCKENZIE",
                                "COLLIER",
                                "LUNA",
                                "PHELPS",
                                "MCGUIRE",
                                "ALLISON",
                                "BRIDGES",
                                "WILKERSON",
                                "NASH",
                                "SUMMERS",
                                "ATKINS",
                                "WILCOX",
                                "PITTS",
                                "CONLEY",
                                "MARQUEZ",
                                "BURNETT",
                                "RICHARD",
                                "COCHRAN",
                                "CHASE",
                                "DAVENPORT",
                                "HOOD",
                                "GATES",
                                "CLAY",
                                "AYALA",
                                "SAWYER",
                                "ROMAN",
                                "VAZQUEZ",
                                "DICKERSON",
                                "HODGE",
                                "ACOSTA",
                                "FLYNN",
                                "ESPINOZA",
                                "NICHOLSON",
                                "MONROE",
                                "WOLF",
                                "MORROW",
                                "KIRK",
                                "RANDALL",
                                "ANTHONY",
                                "WHITAKER",
                                "OCONNOR",
                                "SKINNER",
                                "WARE",
                                "MOLINA",
                                "KIRBY",
                                "HUFFMAN",
                                "BRADFORD",
                                "CHARLES",
                                "GILMORE",
                                "DOMINGUEZ",
                                "ONEAL",
                                "BRUCE",
                                "LANG",
                                "COMBS",
                                "KRAMER",
                                "HEATH",
                                "HANCOCK",
                                "GALLAGHER",
                                "GAINES",
                                "SHAFFER",
                                "SHORT",
                                "WIGGINS",
                                "MATHEWS",
                                "MCCLAIN",
                                "FISCHER",
                                "WALL",
                                "SMALL",
                                "MELTON",
                                "HENSLEY",
                                "BOND",
                                "DYER",
                                "CAMERON",
                                "GRIMES",
                                "CONTRERAS",
                                "CHRISTIAN",
                                "WYATT",
                                "BAXTER",
                                "SNOW",
                                "MOSLEY",
                                "SHEPHERD",
                                "LARSEN",
                                "HOOVER",
                                "BEASLEY",
                                "GLENN",
                                "PETERSEN",
                                "WHITEHEAD",
                                "MEYERS",
                                "KEITH",
                                "GARRISON",
                                "VINCENT",
                                "SHIELDS",
                                "HORN",
                                "SAVAGE",
                                "OLSEN",
                                "SCHROEDER",
                                "HARTMAN",
                                "WOODARD",
                                "MUELLER",
                                "KEMP",
                                "DELEON",
                                "BOOTH",
                                "PATEL",
                                "CALHOUN",
                                "WILEY",
                                "EATON",
                                "CLINE",
                                "NAVARRO",
                                "HARRELL",
                                "LESTER",
                                "HUMPHREY",
                                "PARRISH",
                                "DURAN",
                                "HUTCHINSON",
                                "HESS",
                                "DORSEY",
                                "BULLOCK",
                                "ROBLES",
                                "BEARD",
                                "DALTON",
                                "AVILA",
                                "VANCE",
                                "RICH",
                                "BLACKWELL",
                                "YORK",
                                "JOHNS",
                                "BLANKENSHIP",
                                "TREVINO",
                                "SALINAS",
                                "CAMPOS",
                                "PRUITT",
                                "MOSES",
                                "CALLAHAN",
                                "GOLDEN",
                                "MONTOYA",
                                "HARDIN",
                                "GUERRA",
                                "MCDOWELL",
                                "CAREY",
                                "STAFFORD",
                                "GALLEGOS",
                                "HENSON",
                                "WILKINSON",
                                "BOOKER",
                                "MERRITT",
                                "MIRANDA",
                                "ATKINSON",
                                "ORR",
                                "DECKER",
                                "HOBBS",
                                "PRESTON",
                                "TANNER",
                                "KNOX",
                                "PACHECO",
                                "STEPHENSON",
                                "GLASS",
                                "ROJAS",
                                "SERRANO",
                                "MARKS",
                                "HICKMAN",
                                "ENGLISH",
                                "SWEENEY",
                                "STRONG",
                                "PRINCE",
                                "MCCLURE",
                                "CONWAY",
                                "WALTER",
                                "ROTH",
                                "MAYNARD",
                                "FARRELL",
                                "LOWERY",
                                "HURST",
                                "NIXON",
                                "WEISS",
                                "TRUJILLO",
                                "ELLISON",
                                "SLOAN",
                                "JUAREZ",
                                "WINTERS",
                                "MCLEAN",
                                "RANDOLPH",
                                "LEON",
                                "BOYER",
                                "VILLARREAL",
                                "MCCALL",
                                "GENTRY",
                                "CARRILLO",
                                "KENT",
                                "AYERS",
                                "LARA",
                                "SHANNON",
                                "SEXTON",
                                "PACE",
                                "HULL",
                                "LEBLANC",
                                "BROWNING",
                                "VELASQUEZ",
                                "LEACH",
                                "CHANG",
                                "HOUSE",
                                "SELLERS",
                                "HERRING",
                                "NOBLE",
                                "FOLEY",
                                "BARTLETT",
                                "MERCADO",
                                "LANDRY",
                                "DURHAM",
                                "WALLS",
                                "BARR",
                                "MCKEE",
                                "BAUER",
                                "RIVERS",
                                "EVERETT",
                                "BRADSHAW",
                                "PUGH",
                                "VELEZ",
                                "RUSH",
                                "ESTES",
                                "DODSON",
                                "MORSE",
                                "SHEPPARD",
                                "WEEKS",
                                "CAMACHO",
                                "BEAN",
                                "BARRON",
                                "LIVINGSTON",
                                "MIDDLETON",
                                "SPEARS",
                                "BRANCH",
                                "BLEVINS",
                                "CHEN",
                                "KERR",
                                "MCCONNELL",
                                "HATFIELD",
                                "HARDING",
                                "ASHLEY",
                                "SOLIS",
                                "HERMAN",
                                "FROST",
                                "GILES",
                                "BLACKBURN",
                                "WILLIAM",
                                "PENNINGTON",
                                "WOODWARD",
                                "FINLEY",
                                "MCINTOSH",
                                "KOCH",
                                "BEST",
                                "SOLOMON",
                                "MCCULLOUGH",
                                "DUDLEY",
                                "NOLAN",
                                "BLANCHARD",
                                "RIVAS",
                                "BRENNAN",
                                "MEJIA",
                                "KANE",
                                "BENTON",
                                "JOYCE",
                                "BUCKLEY",
                                "HALEY",
                                "VALENTINE",
                                "MADDOX",
                                "RUSSO",
                                "MCKNIGHT",
                                "BUCK",
                                "MOON",
                                "MCMILLAN",
                                "CROSBY",
                                "BERG",
                                "DOTSON",
                                "MAYS",
                                "ROACH",
                                "CHURCH",
                                "CHAN",
                                "RICHMOND",
                                "MEADOWS",
                                "FAULKNER",
                                "ONEILL",
                                "KNAPP",
                                "KLINE",
                                "BARRY",
                                "OCHOA",
                                "JACOBSON",
                                "GAY",
                                "AVERY",
                                "HENDRICKS",
                                "HORNE",
                                "SHEPARD",
                                "HEBERT",
                                "CHERRY",
                                "CARDENAS",
                                "MCINTYRE",
                                "WHITNEY",
                                "WALLER",
                                "HOLMAN",
                                "DONALDSON",
                                "CANTU",
                                "TERRELL",
                                "MORIN",
                                "GILLESPIE",
                                "FUENTES",
                                "TILLMAN",
                                "SANFORD",
                                "BENTLEY",
                                "PECK",
                                "KEY",
                                "SALAS",
                                "ROLLINS",
                                "GAMBLE",
                                "DICKSON",
                                "BATTLE",
                                "SANTANA",
                                "CABRERA",
                                "CERVANTES",
                                "HOWE",
                                "HINTON",
                                "HURLEY",
                                "SPENCE",
                                "ZAMORA",
                                "YANG",
                                "MCNEIL",
                                "SUAREZ",
                                "CASE",
                                "PETTY",
                                "GOULD",
                                "MCFARLAND",
                                "SAMPSON",
                                "CARVER",
                                "BRAY",
                                "ROSARIO",
                                "MACDONALD",
                                "STOUT",
                                "HESTER",
                                "MELENDEZ",
                                "DILLON",
                                "FARLEY",
                                "HOPPER",
                                "GALLOWAY",
                                "POTTS",
                                "BERNARD",
                                "JOYNER",
                                "STEIN",
                                "AGUIRRE",
                                "OSBORN",
                                "MERCER",
                                "BENDER",
                                "FRANCO",
                                "ROWLAND",
                                "SYKES",
                                "BENJAMIN",
                                "TRAVIS",
                                "PICKETT",
                                "CRANE",
                                "SEARS",
                                "MAYO",
                                "DUNLAP",
                                "HAYDEN",
                                "WILDER",
                                "MCKAY",
                                "COFFEY",
                                "MCCARTY",
                                "EWING",
                                "COOLEY",
                                "VAUGHAN",
                                "BONNER",
                                "COTTON",
                                "HOLDER",
                                "STARK",
                                "FERRELL",
                                "CANTRELL",
                                "FULTON",
                                "LYNN",
                                "LOTT",
                                "CALDERON",
                                "ROSA",
                                "POLLARD",
                                "HOOPER",
                                "BURCH",
                                "MULLEN",
                                "FRY",
                                "RIDDLE",
                                "LEVY",
                                "DAVID",
                                "DUKE",
                                "ODONNELL",
                                "GUY",
                                "MICHAEL",
                                "BRITT",
                                "FREDERICK",
                                "DAUGHERTY",
                                "BERGER",
                                "DILLARD",
                                "ALSTON",
                                "JARVIS",
                                "FRYE",
                                "RIGGS",
                                "CHANEY",
                                "ODOM",
                                "DUFFY",
                                "FITZPATRICK",
                                "VALENZUELA",
                                "MERRILL",
                                "MAYER",
                                "ALFORD",
                                "MCPHERSON",
                                "ACEVEDO",
                                "DONOVAN",
                                "BARRERA",
                                "ALBERT",
                                "COTE",
                                "REILLY",
                                "COMPTON",
                                "RAYMOND",
                                "MOONEY",
                                "MCGOWAN",
                                "CRAFT",
                                "CLEVELAND",
                                "CLEMONS",
                                "WYNN",
                                "NIELSEN",
                                "BAIRD",
                                "STANTON",
                                "SNIDER",
                                "ROSALES",
                                "BRIGHT",
                                "WITT",
                                "STUART",
                                "HAYS",
                                "HOLDEN",
                                "RUTLEDGE",
                                "KINNEY",
                                "CLEMENTS",
                                "CASTANEDA",
                                "SLATER",
                                "HAHN",
                                "EMERSON",
                                "CONRAD",
                                "BURKS",
                                "DELANEY",
                                "PATE",
                                "LANCASTER",
                                "SWEET",
                                "JUSTICE",
                                "TYSON",
                                "SHARPE",
                                "WHITFIELD",
                                "TALLEY",
                                "MACIAS",
                                "IRWIN",
                                "BURRIS",
                                "RATLIFF",
                                "MCCRAY",
                                "MADDEN",
                                "KAUFMAN",
                                "BEACH",
                                "GOFF",
                                "CASH",
                                "BOLTON",
                                "MCFADDEN",
                                "LEVINE",
                                "GOOD",
                                "BYERS",
                                "KIRKLAND",
                                "KIDD",
                                "WORKMAN",
                                "CARNEY",
                                "DALE",
                                "MCLEOD",
                                "HOLCOMB",
                                "ENGLAND",
                                "FINCH",
                                "HEAD",
                                "BURT",
                                "HENDRIX",
                                "SOSA",
                                "HANEY",
                                "FRANKS",
                                "SARGENT",
                                "NIEVES",
                                "DOWNS",
                                "RASMUSSEN",
                                "BIRD",
                                "HEWITT",
                                "LINDSAY",
                                "LE",
                                "FOREMAN",
                                "VALENCIA",
                                "ONEIL",
                                "DELACRUZ",
                                "VINSON",
                                "DEJESUS",
                                "HYDE",
                                "FORBES",
                                "GILLIAM",
                                "GUTHRIE",
                                "WOOTEN",
                                "HUBER",
                                "BARLOW",
                                "BOYLE",
                                "MCMAHON",
                                "BUCKNER",
                                "ROCHA",
                                "PUCKETT",
                                "LANGLEY",
                                "KNOWLES",
                                "COOKE",
                                "VELAZQUEZ",
                                "WHITLEY",
                                "NOEL",
                                "VANG"};

            _LastNames = new Collection<string>(lnames);
        }
        #endregion init names
    }
}
