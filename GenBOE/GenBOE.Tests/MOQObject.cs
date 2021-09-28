// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Tests
{
    using System;
    using System.Collections.ObjectModel;
    using GenBOE.DataBridge.DTO;
    using IES.Common;
    using GenBOE.Dtos;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Transactions;

    [TestClass]
    abstract public class MOQObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        protected static bool _INITIALIZED = false;

        public static Random randomNumberGenerator
        {
            get
            {
                return _randomNumberGenerator;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        protected static ObjectGraph _GENOBJECTS = new ObjectGraph();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected MOQObject() : this(true)
        {
            this._Initialize(new ObjectGraph());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected MOQObject(bool inRunInit)
        {
            if (inRunInit)
            {
                this._Initialize(new ObjectGraph());
            }
        }

        virtual protected void _Initialize(ObjectGraph inObjectGraph)
        {
            if (!_INITIALIZED)
            {
                _GENOBJECTS = inObjectGraph;
                Setup();
            }
        }

        public BoeDTO Boe1
        {
            get { return _GENOBJECTS.Boe1; }
            set { _GENOBJECTS.Boe1 = value; }
        }
        public BoeDTO Boe2
        {
            get { return _GENOBJECTS.Boe2; }
            set
            {
                _GENOBJECTS.Boe2 = value;
            }
        }
        public BoeDTO Boe3
        {
            get { return _GENOBJECTS.Boe3; }
            set
            {
                _GENOBJECTS.Boe3 = value;
            }
        }

        public BoeDTO BOEMaterial
        {
            get { return _GENOBJECTS.BOEMaterial; }
            set { _GENOBJECTS.BOEMaterial = value; }
        }
        public WorkspaceDTO Workspace
        {
            get { return _GENOBJECTS.Workspace; }
            set { _GENOBJECTS.Workspace = value; }
        }
        public PerformingOrgDTO Perforg
        {
            get { return _GENOBJECTS.Perforg; }
            set { _GENOBJECTS.Perforg = value; }
        }
        public PerformingOrgListDTO PerfOrgList
        {
            get { return _GENOBJECTS.PerfOrgList; }
            set { _GENOBJECTS.PerfOrgList = value; }
        }
        public ResourceDTO Resource
        {
            get { return _GENOBJECTS.Resource; }
            set { _GENOBJECTS.Resource = value; }
        }
        public ResourceDTO SystemResource
        {
            get { return _GENOBJECTS.SystemResource; }
            set { _GENOBJECTS.SystemResource = value; }
        }
        public SystemSettingDTO SystemSetting
        {
            get { return _GENOBJECTS.SystemSetting; }
            set { _GENOBJECTS.SystemSetting = value; }
        }
        public ResourceListDTO ResourceList
        {
            get { return _GENOBJECTS.ResourceList; }
            set { _GENOBJECTS.ResourceList = value; }
        }
        public WbsDTO Wbs
        {
            get { return _GENOBJECTS.Wbs; }
            set { _GENOBJECTS.Wbs = value; }
        }
        public ClinDTO Clin1
        {
            get { return _GENOBJECTS.Clin1; }
            set { _GENOBJECTS.Clin1 = value; }
        }
        public ClinDTO Clin2
        {
            get { return _GENOBJECTS.Clin2; }
            set { _GENOBJECTS.Clin2 = value; }
        }
        public ClinDTO Clin3
        {
            get { return _GENOBJECTS.Clin3; }
            set { _GENOBJECTS.Clin3 = value; }
        }
        public UserDTO CostVolumeLead
        {
            get { return _GENOBJECTS.CostVolumeLead; }
            set { _GENOBJECTS.CostVolumeLead = value; }
        }
        public UserDTO Author
        {
            get { return _GENOBJECTS.Author; }
            set { _GENOBJECTS.Author = value; }
        }
        public UserDTO SubcontractorAuthor
        {
            get { return _GENOBJECTS.SubcontractorAuthor; }
            set { _GENOBJECTS.SubcontractorAuthor = value; }
        }
        public UserDTO Approver1
        {
            get { return _GENOBJECTS.Approver1; }
            set { _GENOBJECTS.Approver1 = value; }
        }
        public UserDTO Approver2
        {
            get { return _GENOBJECTS.Approver2; }
            set { _GENOBJECTS.Approver2 = value; }
        }
        public UserDTO SysAdmin
        {
            get { return _GENOBJECTS.SysAdmin; }
            set { _GENOBJECTS.SysAdmin = value; }
        }

        public UserDTO MetricAdmin
        {
            get { return _GENOBJECTS.MetricAdmin; }
            set { _GENOBJECTS.MetricAdmin = value; }
        }
        public UserDTO CreateWorkspacePermissions
        {
            get { return _GENOBJECTS.CreateWorkspacePermissions; }
            set { _GENOBJECTS.CreateWorkspacePermissions = value; }
        }
        public UserDTO WorkspaceAdmin
        {
            get { return _GENOBJECTS.WorkspaceAdmin; }
            set { _GENOBJECTS.WorkspaceAdmin = value; }
        }
        public BoeTaskElementDTO TaskElement
        {
            get { return _GENOBJECTS.TaskElement; }
            set { _GENOBJECTS.TaskElement = value; }
        }
        public OrdinaryVariableDto SummaryWBSOrdinaryVariable
        {
            get { return _GENOBJECTS.SummaryWBSOrdinaryVariable; }
            set { _GENOBJECTS.SummaryWBSOrdinaryVariable = value; }
        }
        public OrdinaryVariableDto SummaryCLINOrdinaryVariable
        {
            get { return _GENOBJECTS.SummaryCLINOrdinaryVariable; }
            set { _GENOBJECTS.SummaryCLINOrdinaryVariable = value; }
        }
        public OrdinaryVariableDto SummaryBOEOrdinaryVariable
        {
            get { return _GENOBJECTS.SummaryBOEOrdinaryVariable; }
            set { _GENOBJECTS.SummaryBOEOrdinaryVariable = value; }
        }
        public WorkspaceVariableDTO SummaryCLINWorkspaceVariable
        {
            get { return _GENOBJECTS.SummaryCLINWorkspaceVariable; }
            set { _GENOBJECTS.SummaryCLINWorkspaceVariable = value; }
        }
        public WorkspaceVariableDTO SummaryWbsWorkspaceVariable
        {
            get { return _GENOBJECTS.SummaryWbsWorkspaceVariable; }
            set { _GENOBJECTS.SummaryWbsWorkspaceVariable = value; }
        }
        public MiscTravelRateDTO MiscTravelRate
        {
            get { return _GENOBJECTS.MiscTravelRate; }
            set { _GENOBJECTS.MiscTravelRate = value; }
        }
        public TripDTO SystemTrip
        {
            get { return _GENOBJECTS.SystemTrip; }
            set { _GENOBJECTS.SystemTrip = value; }
        }
        public LocationDTO DepartureLocation
        {
            get { return _GENOBJECTS.DepartureLocation; }
            set { _GENOBJECTS.DepartureLocation = value; }
        }
        public LocationDTO DestinationLocation
        {
            get { return _GENOBJECTS.DestinationLocation; }
            set { _GENOBJECTS.DestinationLocation = value; }
        }
        public PerDiemDTO PerDiem
        {
            get { return _GENOBJECTS.PerDiem; }
            set { _GENOBJECTS.PerDiem = value; }
        }

        /// <summary>
        /// Reset test data with empty values
        /// </summary>
        virtual public void ResetTestData()
        {
            this.ResetTestData(new ObjectGraph());
        }

        /// <summary>
        /// Reset test data, specifying seed data.  Any values
        /// set to NULL will be initialized with defaults.
        /// </summary>
        /// <param name="inObjectGraph">Class containing seed data for tests</param>
        virtual public void ResetTestData(ObjectGraph inObjectGraph)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                this.Cleanup();

                _GENOBJECTS = inObjectGraph;

                _INITIALIZED = false;
                scope.Complete();
            }
        }

        /// <summary>
        /// Set the test up with seed data
        /// (will only reseed if _INITIALIZED is false)
        /// </summary>
        [TestInitialize()]
        virtual public void Setup()
        {
            if (!_INITIALIZED)
            {
                int id = 1;

                PerfOrgList = new PerformingOrgListDTO { PerformingOrgListID = id++, PerformingOrgListName = "MOQ Perf List" };
                ResourceList = new ResourceListDTO { ResourceListID = id++, ResourceListName = "MOQ Resource List" };

                Author = new UserDTO { UserID = id++ };
                SubcontractorAuthor = new UserDTO { UserID = id++ };
                SysAdmin = new UserDTO { UserID = id++ };
                MetricAdmin = new UserDTO { UserID = id++ };
                WorkspaceAdmin = new UserDTO { UserID = id++ };
                Approver1 = new UserDTO { UserID = id++ };
                Approver2 = new UserDTO { UserID = id++ };
                CostVolumeLead = new UserDTO { UserID = id++ };
                CreateWorkspacePermissions = new UserDTO { UserID = id++ };

                Workspace = new WorkspaceDTO { Id = id++, ContractStartDate = Convert.ToDateTime("01/01/2000"), ContractEndDate = Convert.ToDateTime("12/31/2010"), PerfOrgListID=PerfOrgList.PerformingOrgListID, Segment = SegmentType.SSC};
                Perforg = new PerformingOrgDTO { Id = id++, PerformingOrgName = "KristinePO", PerformingOrgDesc = "Kristines Test"};
                Resource = new ResourceDTO { Id = id++, ResourceName = "BBBBBBB", ResourceDesc = "Lots of Bs", SegRegion = "BB", LaborType = "BBBB", ElementOfCost = ElementOfCostType.LMLabor, RateType = RateType.Hours };
                SystemResource = new ResourceDTO { Id = id++, ResourceName = "RRRRRRR", ResourceDesc = "Lots of Rs", SegRegion = "RR", LaborType = "RRRR", ElementOfCost = ElementOfCostType.LMLabor, RateType = RateType.Hours };
                Wbs = new WbsDTO { Id = id++, WbsNumber = "2.2", WbsTitle = "mock wbs title", WorkspaceID = Workspace.Id };

                Clin1 = new ClinDTO { Id = id++, ClinNumber = "1.1", ClinTitle = "mock clin1 title", ClinPaddedNumber = "000000000000000000001??.000000000000000000001??", StartDate = Convert.ToDateTime("02/01/2000"), EndDate = Convert.ToDateTime("10/31/2010"), WorkspaceID = Workspace.Id };
                Clin2 = new ClinDTO { Id = id++, ClinNumber = "1.2", ClinTitle = "mock clin2 title", ClinPaddedNumber = "000000000000000000001??.000000000000000000002??", StartDate = Convert.ToDateTime("02/01/2000"), EndDate = Convert.ToDateTime("10/31/2010"), WorkspaceID = Workspace.Id };
                Clin3 = new ClinDTO { Id = id++, ClinNumber = "1.3", ClinTitle = "mock clin3 title", ClinPaddedNumber = "000000000000000000001??.000000000000000000003??", StartDate = Convert.ToDateTime("02/01/2000"), EndDate = Convert.ToDateTime("10/31/2010"), WorkspaceID = Workspace.Id };

                Boe1 = new BoeDTO
                {
                    Id = id++,
                    State = BOEState.Draft,
                    StartDate = Convert.ToDateTime("11/01/2005"),
                    EndDate = Convert.ToDateTime("01/01/2006"),
                    Description = "BOE MOCK TEST",
                    DataSource = "Data Source MOCK TEST",
                    WBSID = Wbs.Id,
                    CLINID = Clin1.Id,
                    AuthorIDs = new Collection<int>{Author.UserID},
                    WorkspaceID = Workspace.Id,
                    CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { }
                };
                Boe2 = new BoeDTO
                {
                    Id = id++,
                    State = BOEState.Draft,
                    StartDate = Convert.ToDateTime("11/01/2006"),
                    EndDate = Convert.ToDateTime("01/01/2007"),
                    Description = "BOE MOCK TEST2",
                    DataSource = "Data Source MOCK TEST",
                    WBSID = Wbs.Id,
                    CLINID = Clin2.Id,
                    AuthorIDs = new Collection<int> { Author.UserID},
                    WorkspaceID = Workspace.Id,
                    CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { }
                };
                Boe3 = new BoeDTO
                {
                    Id = id++,
                    State = BOEState.Draft,
                    StartDate = Convert.ToDateTime("11/01/2006"),
                    EndDate = Convert.ToDateTime("01/01/2007"),
                    Description = "BOE MOCK TEST3 SubcontractorAuthor",
                    DataSource = "Data Source MOCK TEST",
                    WBSID = Wbs.Id,
                    CLINID = Clin2.Id,
                    SubcontractorAuthorIDs = new Collection<int> { SubcontractorAuthor.UserID },
                    WorkspaceID = Workspace.Id,
                    CustomFieldValueContainers = new Collection<CustomFieldValueContainer> { }
                };
                

                TaskElement = new BoeTaskElementDTO
                {
                    Id = id++,
                    BOETaskID = "B4",
                    TaskTitle = Guid.NewGuid().ToString(),
                    Description = "a great new moq task",
                    StartDate = Convert.ToDateTime("09/01/2010"),
                    EndDate = Convert.ToDateTime("07/01/2015"),
                    MOQHoursEquation = "14000 Hours * 85% ComplexityFactor",
                    MOQText = "hours and factor",
                    MOQType = MOQType.Standard,
                    BoeID = Boe1.Id,
                    UpdateDate = DateTime.Now
                };

                SummaryBOEOrdinaryVariable = new OrdinaryVariableDto
                {
                    BoeID = Boe1.Id,
                    Id = id++,
                    OrdinaryVariableName = Guid.NewGuid().ToString(),
                    OrdinaryVariableValue = MOQObject.randomNumberGenerator.Next(),
                    SortBOEBy = VarSortBOEBy.CLIN,
                    ValueType = VarValueType.SumOfBOEs,
                    SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { BoeID = Boe1.Id } }
                };

                SummaryWBSOrdinaryVariable = new OrdinaryVariableDto
                {
                    BoeID = Boe1.Id,
                    Id = id++,
                    OrdinaryVariableName = Guid.NewGuid().ToString(),
                    OrdinaryVariableValue = MOQObject.randomNumberGenerator.Next(),
                    SortBOEBy = VarSortBOEBy.CLIN,
                    ValueType = VarValueType.SumOfBOEs,
                    SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { WBSID = Wbs.Id } }
                };

                SummaryCLINOrdinaryVariable = new OrdinaryVariableDto
                {
                    BoeID = Boe1.Id,
                    Id = id++,
                    OrdinaryVariableName = Guid.NewGuid().ToString(),
                    OrdinaryVariableValue = MOQObject.randomNumberGenerator.Next(),
                    SortBOEBy = VarSortBOEBy.CLIN,
                    ValueType = VarValueType.SumOfBOEs,
                    SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { CLINID = Clin1.Id } }
                };

                SummaryCLINWorkspaceVariable = new WorkspaceVariableDTO
                {
                    Id = id++,
                    WorkspaceVariableName = Guid.NewGuid().ToString(),
                    WorkspaceVariableValue = MOQObject.randomNumberGenerator.Next(),
                    WorkspaceID = Workspace.Id,
                    SortBOEBy = VarSortBOEBy.WBS,
                    ValueType = VarValueType.SumOfBOEs,
                    SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { CLINID = Clin1.Id } }
                };

                SummaryWbsWorkspaceVariable = new WorkspaceVariableDTO
                {
                    Id = id++,
                    WorkspaceVariableName = Guid.NewGuid().ToString(),
                    WorkspaceVariableValue = MOQObject.randomNumberGenerator.Next(),
                    WorkspaceID = Workspace.Id,
                    SortBOEBy = VarSortBOEBy.WBS,
                    ValueType = VarValueType.SumOfBOEs,
                    SelectedBOEsToSum = new Collection<SelectBOEsToSum> { new SelectBOEsToSum { WBSID = Wbs.Id } }
                };

                MiscTravelRate = new MiscTravelRateDTO
                {
                    Id = id++,
                    MiscTravelRate = 55,
                    SortCode = 77,
                    MiscTravelRateMode = "MockNewGlobalMode"
                };

                string depName = Guid.NewGuid().ToString().Substring(0, 10);

                DepartureLocation = new LocationDTO
                {
                    Id = id++,
                    LocationName = "MOCK" + depName,
                    LastUpdatedBy = Author.UserID
                };

                string destName = Guid.NewGuid().ToString().Substring(0, 10);
                DestinationLocation = new LocationDTO
                {
                    Id = id++,
                    LocationName = "MOCK" + destName,
                    LastUpdatedBy = Author.UserID
                };

                PerDiem = new PerDiemDTO
                {
                    Id = id++,
                    HotelRate = 10m,
                    MIERate = 10m,
                    PerDiemDestination = "MockGlobalPerDiemDest",
                    Qualification = "MockGlobal" + Guid.NewGuid().ToString().Substring(0, 20),
                    PerDiemNotes = "blah blah global",
                    LastUpdatedBy = Author.UserID
                };

                SystemTrip = new TripDTO
                {
                    TripID = id++,
                    MiscTravelRateID = MiscTravelRate.Id,
                    DepartureLocationID = DepartureLocation.Id,
                    DestinationLocationID = DestinationLocation.Id,
                    Fare = 50,
                    RTMiles = 50,
                    FareUpdatedByUserID = Author.UserID,
                    RentalCarRate = 10m
                };

                _INITIALIZED = true;
            }
        }

        public string CreateRandomWord(int size)
        {
            return CreateRandomWord(size, false, string.Empty);
        }

        public string CreateRandomWord(int size, bool allowNumbers)
        {
            return CreateRandomWord(size, allowNumbers, string.Empty);
        }

        public string CreateRandomWord(int size, bool allowNumbers, string extraAllowedChars)
        {
            string viableChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (allowNumbers)
            {
                viableChars = string.Concat(viableChars, "0123456789");
            }
            viableChars = string.Concat(viableChars, extraAllowedChars);

            string word = string.Empty;

            for (int i = 0; i < size; i++)
            {
                word = string.Concat(word, viableChars[MOQObject.randomNumberGenerator.Next(viableChars.Length)]);
            }

            return word;
        }

        virtual public void Cleanup()
        {
            this._Initialize(new ObjectGraph());
        }

        /// <summary>
        /// Always reset MOQObjects after a test (barely any time since not connected to db)
        /// </summary>
        [TestCleanup()]
        virtual public void TestCleanup()
        {
            _INITIALIZED = false;
        }

        private TestContext testContextInstance;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private static Random _randomNumberGenerator = new Random((int)DateTime.Now.Ticks);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public class ObjectGraph
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public BoeDTO Boe1;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public BoeDTO Boe2;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public BoeDTO Boe3;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public BoeDTO BOEMaterial;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public WorkspaceDTO Workspace;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public PerformingOrgDTO Perforg;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public PerformingOrgListDTO PerfOrgList;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public ResourceDTO Resource;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public ResourceListDTO ResourceList;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public WbsDTO Wbs;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public ClinDTO Clin1;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public ClinDTO Clin2;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public ClinDTO Clin3;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public UserDTO CostVolumeLead;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public UserDTO Author;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public UserDTO SubcontractorAuthor;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public UserDTO Approver1;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public UserDTO Approver2;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public UserDTO SysAdmin;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public UserDTO CreateWorkspacePermissions;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public UserDTO WorkspaceAdmin;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public BoeTaskElementDTO TaskElement;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public OrdinaryVariableDto SummaryWBSOrdinaryVariable;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public OrdinaryVariableDto SummaryCLINOrdinaryVariable;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public OrdinaryVariableDto SummaryBOEOrdinaryVariable;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public WorkspaceVariableDTO SummaryCLINWorkspaceVariable;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public WorkspaceVariableDTO SummaryWbsWorkspaceVariable;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public MiscTravelRateDTO MiscTravelRate;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public TripDTO SystemTrip;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public LocationDTO DepartureLocation;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public LocationDTO DestinationLocation;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public PerDiemDTO PerDiem;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public UserDTO MetricAdmin;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public ResourceDTO SystemResource;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public SystemSettingDTO SystemSetting;
    }

}
