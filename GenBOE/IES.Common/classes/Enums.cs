// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
	using System;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;

	/// <summary>
	/// The States a BOE can be in
	/// </summary>
	public enum BOEState
	{
		None = 0,
		[Description("Unassigned")]
		Unassigned = 1,
		[Description("Draft")]
		Draft = 2,
		[Description("Awaiting Approval")]
		AwaitingApproval = 3,
		[Description("Approved")]
		Approved = 4,
		DateShiftDraft = 5,
		[Description("Locked Draft")]
		DraftLocked = 6
	}

	/// <summary>
	/// Different Lines of Business
	/// </summary>
	public enum LineOfBusinessType
	{
		[Description("Not Set")]
		NotSet = 0,

		//
		// MST
		[Description("Integrated Warfare Systems and Sensors")]
		MSTIntegratedWarfareSystemsAndSensors = 2001,
		[Description("New Ventures")]
		MSTNewVentures = 2002,
		[Description("Training and Logistics Solutions")]
		MSTTrainingAndLogistics = 2003,
		[Description("C4ISR & Undersea Systems")]
		MSTUnderseaSystems = 2004,
		[Description("Cyber, Ships and Advanced Technologies")]
		MSTShipAndAviationSystems = 2005,
		[Description("Sikorsky")]
		MSTSikorsky = 2006
	}

	/// <summary>
	/// Constants associated with the LineOfBusinessType.
	/// </summary>
	public static class LineOfBusinessTypeConstants
	{
		/// <summary>
		/// Identifies the minimum enum value that can be validly selected by a user for the Line of Business
		/// Used to validate the users input.
		/// </summary>
		public const int MSTMinimumSelectableValue = (int)LineOfBusinessType.MSTIntegratedWarfareSystemsAndSensors;

		/// <summary>
		/// Identifies the maximum enum value that can be validly selected by a user for the Line of Business
		/// Used to validate the users input.
		/// </summary>
		public const int MSTMaximumSelectableValue = (int)LineOfBusinessType.MSTSikorsky;
	}

	/// <summary>
	/// Different types of elements of cost
	/// NOTE: This determines the SORT order on the BOE SUMMARY.  Make sure
	/// the order remains correct when adding new values.
	/// </summary>
	public enum ElementOfCostType
	{
		NotSet = 0,
		[Description("LM Labor")]
		LMLabor = 1,
		[Description("IWTA")]
		IWTA = 2,
		[Description("Sub")]
		Sub = 3,
		[Description("Materials")]
		Materials = 4,
		[Description("ODC")]
		ODC = 5,
		[Description("Travel")]
		Travel = 6
	}

	/// <summary>
	/// Specifies constants tightly coupled with the ElementOfCostType
	/// </summary>
	public static class ElementOfCostTypeConstants
	{
		/// <summary>
		/// Identifies the minimum enum value that can be validly selected by a user for the Element of Cost
		/// Used to validate the users input.
		/// </summary>
		public const int MinimumSelectableValue = (int)ElementOfCostType.LMLabor;

		/// <summary>
		/// Identifies the maximum enum value that can be validly selected by a user for the Element of Cost
		/// Used to validate the users input.
		/// </summary>
		public const int MaximumSelectableValue = (int)ElementOfCostType.Travel;
	}

	/// <summary>
	/// Different types of proposals
	/// </summary>
	public enum ProposalStatusType
	{
		None = 0,
		Lost = 1,
		Won = 2
	}

	/// <summary>
	/// The types of entities in the system (mainly used with managing permissions)
	/// </summary>
	public enum EntityType
	{
		User = 0,
		Group = 1
	}

	/// <summary>
	/// States for a user-removed workspace
	/// </summary>
	public enum WorkspaceUserRemoved
	{
		Default = 0,
		Deleted = 1,
		EmailSent = 2
	}

	/// <summary>
	/// The IDs of the emails stored in the database.  The emails in the 
	/// database will contain the subject and body of the email message
	/// associated with the email.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
	public enum EmailTypes
	{
		RequestWorkspacePermission = 20,
		WorkspaceRestored = 24,
		BOESubmittedForReview = 28,
		BOEAuthorRespondedToComment = 29,
		BOEApproversAuthorApproverApproved = 30,
		BOEApproversAuthorApproverRejected = 31,
		ApproverEmailBOEAwaitingApproval = 32, //- (Draft -> AA, to approvers)
		WorkspaceAdminEmailAllBOEsApproved = 33, // (admins only)
		BOEAuthorReviewerCommented = 34,
		BOEOpenedForEdit = 35,
		BOEUpdatedToAuthorsAndApprovers = 36,
		CLINUpdatedToAuthorsAndApprovers = 37,
		BOEDeleted = 38,
		BOEAuthorsApproversInUseResourceUpdated = 39,
		BOEAuthorsChanged = 40,
		BOEApproversChanged = 41,
		BOEAuthorsApproversDatesUpdated = 42,
		BOECLINWBSChanged = 43,
		WorkspaceWorkingToLocked = 44,
		WorkspaceLockedToWorking = 45,
		WorkspaceWorkingToInitialization = 46,
		WorkspaceInitializationToWorkingSubsequent = 47,
		BOEAuthorsApproversDatesUpdatedError = 48,
		TemplateUnassigned = 49,
		TemplateAssigned = 50,
		TemplatePromptDeleted = 51
	}

	/// <summary>
	/// The types of reports that can be generated by the system. Non-DB-backed.
	/// </summary>
	public enum ReportType
	{
		Export = 1,
		View = 2,
		Custom = 3,
		SSRS = 4
	}

	/// <summary>
	/// The reports that the system can generate. DB-backed LU.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
	public enum Reports
	{
		WorkspaceActivity = 1,
		BOEStatus = 2,
		AllBOEs = 3,
		BOEActivity = 4,
		WorkspaceData = 5,
		BOEStatusByBOE = 6,
		BOEStatusByWBS = 7,
		BOEStatusByCLIN = 8,
		TravelUnitCost = 9,
		TravelExtendedCost = 10,
		BoeDiscrepancy = 12,
		ValidateAllBOE = 13,
		INLFormsExport = 17,
		[Description("Category/CLIN Summary")]
		CategoryClinSummary = 18,
		[Description("CLIN/Category Summary")]
		ClinCategorySummary = 19,
		[Description("Project CLIN Cost Summary")]
		ProjectClinCostSummary = 20,
		[Description("Cost by CLIN, Res, Act & Yr (8yrs)")]
		CostByClinResActYr = 22,
		[Description("Cost by CLIN, Act & Yr (17yrs)")]
		CostByClinActYr = 23,
		[Description("BOE Summary Report")]
		BOESummaryReport = 24,
		[Description("Standard Reports")]
		StandardReports = 25,
		[Description("By Pricing Code (17 yrs)")]
		ByPricingCode = 26,
		[Description("By Cat/Pricing Code (17 yrs)")]
		ByCatPricingCode = 27,
		[Description("Offload Cost By Year")]
		OffloadCostByYear = 28,
		[Description("Offload Cost Summary")]
		OffloadCostSummary = 29,
		[Description("Staffing Curves")]
		StaffingCurves = 30,
		[Description("RPS")]
		Rps = 31,
		[Description("PRP")]
		Prp = 32,
		[Description("RAM")]
		Ram = 33,
		[Description("Pre vs Post Offload Totals")]
		PreVsPostOffloadTotals = 34,
		[Description("Offload Dollar Workbench Export")]
		WorkbenchOffload = 35,
		[Description("Flattened Cost by CLIN, Res, Act & Yr (8yrs)")]
		CostByClinResActYrFlat = 36,
		[Description("Offload Detailed Report")]
		OffloadDetailedReport = 37,
		[Description("WBS Summary of Hours, ODC Costs")]
		WbsBoeReport = 38,
		[Description("All BOEs broken down in segments")]
		AllBOEsSegmented = 39

	}

	/// <summary>
	/// Excel Report Template Types
	/// </summary>
	public enum ExcelReportTemplateType
	{
		[Description("")]
		NotSet = -1,
		[Description("DS & ES - Standard - Portrait - With Cost")]
		DS_ES_STANDARD_PORTRAIT_WITH_COST = 1,
		[Description("DS & ES - Standard - Landscape - With Cost")]
		DS_ES_STANDARD_LANDSCAPE_WITH_COST = 2,
		[Description("DS & ES - Standard - Portrait - Without Cost")]
		DS_ES_STANDARD_PORTRAIT_WITHOUT_COST = 3,
		[Description("DS & ES - Standard - Landscape - Without Cost")]
		DS_ES_STANDARD_LANDSCAPE_WITHOUT_COST = 4,
		[Description("DS & ES - EDS - Landscape - Without Labor Cost")]
		DS_ES_EDS_LANDSCAPE_WITHOUT_LABOR_COST = 5,
		[Description("LMSI - GSPS - Landscape - Without Labor Cost")]
		LMSI_GSPS_LANDSCAPE_WITHOUT_LABOR_COST = 6,
		// [Description("TSS - NGI - Portrait - Without Labor Cost")]
		// TSS_NGI_PORTRAIT_WITHOUT_LABOR_COST = 7, // Removed as OBE
		[Description("DS & ES - IECS - Landscape - With Cost")]
		DS_ES_IECS_LANDSCAPE_WITH_COST = 8,
		[Description("DS - Standard - Portrait - Without Cost #2")]
		DS_STANDARD_PORTRAIT_WITHOUT_COST_2 = 9,
		[Description("LMSI - AEHF - Landscape - With Non-Labor Cost")]
		LMSI_AEHF_LANDSCAPE_WITH_NON_LABOR_COST = 10,
		[Description("DS - Standard - Landscape - With Cost - Includes Variable Details")]
		DS_STANDARD_LANDSCAPE_WITH_COST_INCL_VAR_DETAILS = 11,
		[Description("LMSI GSM-O Landscape with Time Phased Summaries")]
		LMSI_GSM_O_LANDSACPE_WITH_TIME_PHASED_SUMMARIES = 12,
		[Description("LMSI - NISSC - Landscape - With Non-Labor Cost")]
		LMSI_NISSC_LANDSCAPE_WITH_NON_LABOR_COST = 13,
		[Description("LMSI - Standard - Landscape")]
		LMSI_STANDARD_LANDSCAPE = 14,
		[Description("RMS - Space Fence - Portrait")]
		MST_SPACE_FENCE_PORTRAIT = 2001,
		[Description("RMS - Standard - Portrait")]
		MST_STANDARD_PORTRAIT = 2002,
		[Description("RMS - Portrait - 12 point 1 inch margins")]
		MST_PORTRAIT_12_POINT_1_INCH_MARGINS = 2003,
		[Description("RMS - Deepwater - Portrait")]
		MST_DEEPWATER_PORTRAIT = 2004,
		[Description("RMS - Portrait - 12 point 1 inch margins with genBOE Task IDs")]
		MST_PORTRAIT_12_POINT_1_INCH_MARGINS_GENBOE_TASK_IDS = 2005,
		[Description("RMS - Portrait - 12 point 1 inch margins with genBOE BOE, Task, and Resource IDs")]
		MST_PORTRAIT_12_POINT_1_INCH_MARGINS_GENBOE_BOE_TASK_RESOURCE_IDS = 2006,
		[Description("RMS - Portrait - 12 point 1 inch margins - Custom Fields")]
		MST_PORTRAIT_12_POINT_1_INCH_MARGINS_CUSTOM_FIELDS = 2007,
		[Description("RMS - Sikorsky Project Map")]
		RMS_SIKORSKY_PROJECT_MAP = 2008,
		[Description("RMS - RMS Project Map")]
		RMS_RMS_PROJECT_MAP = 2009,
		[Description("RMS - Portrait - CLIN-WBS with SOW ID")]
		RMS_PORTRAIT_WBS_CLIN_SOW = 2010,
		[Description("Master")]
		MASTER = 9001
	}

	/// <summary>
	/// The Security Roles in the system
	/// </summary>
	public enum Role
	{
		None = 0,
		Author = 1,
		WorkspaceReviewer = 2,
		Approver = 3,
		WorkspaceAdmin = 4,
		MetricsAdmin = 5,
		SystemAdmin = 6,
		WorkspaceUser = 7,
		CreateWorkspacePermissions = 8,
		SubcontractorAuthor = 9,
		SubcontractAdmin = 10
	};

	/// <summary>
	/// Spread Curves
	/// </summary>
	public enum SpreadCurves
	{
		None = -1,
		[Description("Discrete Cost")]
		DiscreteCost = 0,
		[Description("Discrete Hours")]
		DiscreteHours = 1,
		[Description("Curve 1")]
		SpreadCurve1 = 2,
		[Description("Curve 2")]
		SpreadCurve2 = 3,
		[Description("Curve 3")]
		SpreadCurve3 = 4,
		[Description("Curve 4")]
		SpreadCurve4 = 5,
		[Description("Curve 5")]
		SpreadCurve5 = 6,
		[Description("Curve 6")]
		SpreadCurve6 = 7,
		[Description("Curve 7")]
		SpreadCurve7 = 8,
		[Description("Curve 8")]
		SpreadCurve8 = 9,
		[Description("Curve 9")]
		SpreadCurve9 = 10,
		[Description("Curve 10")]
		SpreadCurve10 = 11,
		[Description("Curve 11")]
		SpreadCurve11 = 12,
		[Description("Curve 12")]
		SpreadCurve12 = 13,
		[Description("Curve 13")]
		SpreadCurve13 = 14,
		[Description("Curve 14")]
		SpreadCurve14 = 15,
		[Description("Curve 15")]
		SpreadCurve15 = 16,
		[Description("Curve 16")]
		SpreadCurve16 = 17,
		[Description("Curve 17")]
		SpreadCurve17 = 18,
		[Description("Curve 18")]
		SpreadCurve18 = 19,
		[Description("Curve 19")]
		SpreadCurve19 = 20,
		[Description("Curve 20")]
		SpreadCurve20 = 21,
		[Description("Curve 21")]
		SpreadCurve21 = 22,
		[Description("Curve 22")]
		SpreadCurve22 = 23,
		[Description("Curve 23")]
		SpreadCurve23 = 24,
		[Description("Curve 24")]
		SpreadCurve24 = 25,
		[Description("Curve 25")]
		SpreadCurve25 = 26,
		[Description("Curve 26")]
		SpreadCurve26 = 27,
		[Description("Curve 27")]
		SpreadCurve27 = 28,
		[Description("Curve 28")]
		SpreadCurve28 = 29,
		[Description("Curve 29")]
		SpreadCurve29 = 30,
		[Description("Curve 30")]
		SpreadCurve30 = 31,
		[Description("Curve 31")]
		SpreadCurve31 = 32,
		[Description("Curve 32")]
		SpreadCurve32 = 33,
		[Description("Curve 33")]
		SpreadCurve33 = 34,
		[Description("Curve 34")]
		SpreadCurve34 = 35,
		[Description("Curve 35")]
		SpreadCurve35 = 36,
		[Description("Curve 36")]
		SpreadCurve36 = 37,
		[Description("Curve 37")]
		SpreadCurve37 = 38,
		[Description("Curve 38")]
		SpreadCurve38 = 39,
		[Description("Curve 39")]
		SpreadCurve39 = 40,
		[Description("Curve 40")]
		SpreadCurve40 = 41,
		[Description("Curve 41")]
		SpreadCurve41 = 42,
		[Description("Curve 42")]
		SpreadCurve42 = 43,
		[Description("Curve 43")]
		SpreadCurve43 = 44,
		[Description("Curve 44")]
		SpreadCurve44 = 45,
		[Description("Curve 45")]
		SpreadCurve45 = 46,
		[Description("Curve 46")]
		SpreadCurve46 = 47,
		[Description("Curve 47")]
		SpreadCurve47 = 48,
		[Description("Curve 48")]
		SpreadCurve48 = 49,
		[Description("Curve 49")]
		SpreadCurve49 = 50,
		[Description("Curve 50")]
		SpreadCurve50 = 51,
		[Description("Level")]
		Level = 52,
		[Description("Load")]
		Load = 53,
		[Description("Curve 51")]
		SpreadCurve51 = 54,
		[Description("Curve 52")]
		SpreadCurve52 = 55,
		[Description("Curve 53")]
		SpreadCurve53 = 56

	}

	/// <summary>
	/// The States a Workspace can be in
	/// </summary>
	public enum WorkspaceState
	{
		[Description("None")]
		None = 0,
		[Description("Initialization")]
		Initialization = 1,
		[Description("Working")]
		Working = 2,
		[Description("Locked")]
		Locked = 3,
		[Description("Complete")]
		Complete = 4,
		[Description("Closed")]
		Closed = 5
	}

	/// <summary>
	/// The possible field types used by BOE History
	/// </summary>
	public enum FieldType
	{
		None = 0,
		BOECreated = 1,
		BOEStatus = 2,
		Comment = 3,
		AuthorResponse = 4,
		ApproverResponse = 5,
		SubmittedForReview = 6,
		Author = 7,
		Approver = 8,
		SubcontractorAuthor = 9
	}

	// The ApproverResponseType will keep track if an approver has approved or rejected
	// a BOE. This information is kept as a bit in the database, but we need a way to keep
	// track of this information if an approver hasn't responded yet which is why the None
	// is necessary
	public enum ApproverReponseType
	{
		Rejected = 0,
		Approved = 1,
		None = 3
	}

	/// <summary>
	/// This is for the possible search categories when doing a BOE Search
	/// </summary>
	public enum SearchCategory
	{
		None = 0,
		BOEsInOtherWorkspaces = 1,
		BOEContentTemplates = 2,
		BOEsInThisWorkspace = 3,
		All = 4
	}

	/// <summary>
	/// Variable Value Type
	/// </summary>
	public enum VarValueType
	{
		None = 0,
		Discrete = 1,
		SumOfBOEs = 2
	}

	/// <summary>
	/// variable Sort BOE By..if VarValueType is SumOfBOEs
	/// </summary>
	public enum VarSortBOEBy
	{
		WBS = 1,
		CLIN = 2
	}

	/// <summary>
	/// Export Sort BOE By
	/// </summary>
	public enum ExportSortBOEBy
	{
		WBS = 1,
		CLIN = 2
	}

	/// <summary>
	/// Status for Historical Metric
	/// </summary>
	public enum HistoricalMetricsStatus
	{
		[Description("None")]
		None = 0,
		[Description("Not Validated")]
		Not_Validated = 1,
		[Description("Validated")]
		Validated = 2
	}

	/// <summary>
	/// Usage of Historical Metric
	/// </summary>
	public enum HistoricalMetricsUsage
	{
		None = 0,
		Expansion = 1,
		LaborRatio = 2,
		ProductivityRate = 3,
		Sizing = 4,
		Hours = 5
	}

	/// <summary>
	/// Sources for Historical Metric
	/// </summary>
	public enum HistoricalMetricsSource
	{
		None = 0,
		LMOnly = 1,
		SubOnly = 2,
		LMAndSub = 3,
		IndustryData = 4
	}

	/// <summary>
	///  ProPricer Scope
	/// </summary>
	public enum ProPricerScope
	{
		Workspace = 1,
		System = 2
	}

	/// <summary>
	/// ProPricer Field Task
	/// </summary>
	public enum ProPricerField_Task
	{
		BOEStartDate = 1,
		BOEEndDate = 2,
		CLINNumber = 3,
		CLINTitle = 4,
		ProPricerTaskID = 5,
		TaskTitle = 6,
		TOTAL = 7,
		WBSNumber = 8,
		WBSTitle = 9,
		BLANK = 10,
		ResourceID = 22,
		PerformingOrg = 23,
		MOQType = 24,
		GenBOEBOEID = 25,
		GenBOETaskID = 26,
		GenBOEResourceID = 27,
		BOETitle = 31,
		TaskID = 33,
		Trip_NumPeople = 37,
		Trip_NumDays = 38,
		Trip_Origin = 39,
		Trip_Destination = 40,
		Trip_AirEst = 41,
		Trip_PerDiem = 42,
		Trip_CarRental = 43,
		Trip_NumCars = 44,
		Trip_ID = 51,
		Trip_GroupID = 52,
		Trip_Purpose = 53,
		Trip_TravelMode = 56,
		ProjMapTaskId = 57,
		ProjMapActivityName = 58,
		ProjMapStartDate = 59,
		ProjMapEndDate = 60,
		ProjMapQuantity = 61,
		ProjMapCamName = 62,
		ProjMapWbsNumber = 63,
		ProjMapCostCenter = 64,
		ProjMapClin = 65,
		ProjMapSowNumber = 66,
		ProjMapAddDelete = 67,
		ProjMapCategory = 72,
		ProjMapClassOfCost = 73,
		ProjMapOldResource = 74,
		ResourceSegmentRegion = 76,
	}

	/// <summary>
	/// ProPricer Resources
	/// </summary>
	public enum ProPricerField_Resources
	{
		CLINNumber = 11,
		CLINTitle = 12,
		DISCRETE = 13,
		IMSCode = 14,
		PerformingOrg = 15,
		ProPricerTaskID = 16,
		ResourceID = 17,
		StartDate = 18,
		WBSNumber = 19,
		WBSTitle = 20,
		BLANK = 21,
		GenBOEBOEID = 28,
		GenBOETaskID = 29,
		GenBOEResourceID = 30,
		BOETitle = 32,
		TaskTitle = 34,
		TaskID = 35,
		EP = 36,
		Trip_GroupID = 45,
		Trip_TripDate = 46,
		Trip_Origin = 47,
		Trip_Destination = 48,
		Trip_NumCars = 49,
		ProjMapTaskId = 68,
		ProjMapInitialResoure = 69,
		ProjMapSpreadCode = 70,
		ProjMapStartDate = 71,
		ProjMapOldResource = 75,
		ResourceSegmentRegion = 77
	}

	/// <summary>
	/// EPP Levels
	/// </summary>
	public enum EppDelegationAuthority
	{
		[Description("Program")]
		Program = 1,
		[Description("Line of Business")]
		LoB = 2,
		[Description("Space")]
		Space = 3,
		[Description("Corporate")]
		Corporate = 4
	}

	/// <summary>
	/// ProPricer Types
	/// </summary>
	public enum ProPricerType
	{
		None = 0,
		Tasks = 1,
		Resources = 2
	}

	/// <summary>
	/// ProPricer Custom Field Selection
	/// </summary>
	public enum ProPricerCustomFieldSelection
	{
		None = 0,
		CustomFieldID = 1,
		CustomFieldDescription = 2
	}

	/// <summary>
	/// This enum will keep track if a workspace or task variable is being used in BOE Task Element recalculations
	/// </summary>
	public enum VariableType
	{
		Workspace = 1,
		Task = 2
	}

	/// <summary>
	/// Custom Field Types
	/// </summary>
	public enum CustomFieldType
	{
		[Description("BOE")]
		BoeDisplay = 1,
		[Description("Task")]
		TaskDisplay = 2,
		[Description("Resource Types")]
		LaborTypeDisplay = 3,
		[Description("MOQ Type Table Data")]
		MoqTypeTableDataDisplay = 4
	}

	/// <summary>
	/// Rate Formats by Target
	/// </summary>
	public enum RateFormatTarget
	{
		[Description("PPR&D")]
		PPRD = 1,
		[Description("Grid")]
		Grid = 2,
		[Description("Export")]
		Export = 3,
		[Description("ProPricer Export")]
		ProPricer = 4,
		[Description("Cobra Export")]
		Cobra = 5
	}

	/// <summary>
	/// Rate Types
	/// Note: RateType values are shared between BOE and RDM.
	/// </summary>
	public enum RateType
	{
		[Description("")]
		NotSet = 0,
		[Description("Hours")]
		Hours = 1,
		[Description("Cost")]
		Cost = 2
	}

	/// <summary>
	/// Specifies set of constants associated with the Rate
	/// </summary>
	public static class RateTypeConstants
	{
		/// <summary>
		/// Minimum value selectable by a user for the RateType
		/// </summary>
		public const int MinimumSelectableValue = (int)RateType.Hours;

		/// <summary>
		/// Maximum value selectable by a user for the RateType
		/// </summary>
		public const int MaximumSelectableValue = (int)RateType.Cost;
	}

	/// <summary>
	/// Rate Compare State
	/// Note: State of rates on rate compare page.
	/// </summary>
	public enum RateCompareState
	{
		[Description("Added")]
		Added = 0,
		[Description("Deleted")]
		Deleted = 1,
		[Description("Edit")]
		Edit = 2
	}

	/// <summary>
	/// Spread Types
	/// </summary>
	public enum SpreadType
	{
		[Description("")]
		NotSet = 0,
		[Description("Hours")]
		Hours = 1,
		[Description("Cost")]
		Cost = 2
	}

	/// <summary>
	/// Segment Types
	/// </summary>
	public enum SegmentType
	{
		[Description("None")]
		None = 0,
		[Description("Development Segment (DS)")]
		DS = 1,
		[Description("ES")]
		ES = 2,
		[Description("LM Services Segment (LS)")]
		LS = 3,
		[Description("TS")]
		TS = 4,
		[Description("SSC")]
		SSC = 1001,
		[Description("RMS")]
		RMS = 2001
	}

	/// <summary>
	/// Task Element Types
	/// </summary>
	public enum TaskElementType
	{
		None = 0,
		Labor = 1
	}


	/// <summary>
	/// Sum Variable Resource Types
	/// </summary>
	public enum SumVariableResourceType
	{
		DSLabor = 1,
		ESLabor = 2,
		TSLabor = 3,
		LSLabor = 4,
		LOEIWTA = 5,
		LOESub = 6,
		/* DTS = 9, // No longer supported */
		[Description("LM Labor")]
		SSCLMLabor = 1001,
		[Description("IWTA")]
		SSCLOEIWTA = 1002,
		[Description("Sub")]
		SSCLOESub = 1003,
		/*[Description("DTS")]
        SSCDTS = 1006, // No longer supported */
		[Description("LM Labor")]
		MSTLMLabor = 2001,
		[Description("IWTA")]
		MSTLOEIWTA = 2002,
		[Description("Sub")]
		MSTLOESub = 2003
	}

	/// <summary>
	/// Date Time Precision
	/// </summary>
	public enum DateTimePrecision
	{
		Month = 1,
		Day = 2
	}

	/// <summary>
	/// Find/Replace Element Types
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
	public enum FindReplaceElementType
	{
		BOE = 1, // BOE as in BOETaskElement
				 // No longer allowed Material = 2,
				 // No longer allowed  ODC = 3,
		Travel = 4
	}

	/// <summary>
	/// Sort Order
	/// </summary>
	public enum SortOrder
	{
		Descending,
		Ascending
	}

	#region Active Directory lookup

	/// <summary>
	/// The types of entities in the system (mainly used with managing permissions)
	/// </summary>
	public enum UserType
	{
		NotSet = 0,
		User = 1,
		Group = 2
	}

	/// <summary>
	/// User information to search for
	/// </summary>
	public enum ActiveDirectorySearchBy
	{
		/// <summary>
		/// User's last name
		/// </summary>
		LastName,
		/// <summary>
		/// User's account name
		/// </summary>
		Account
	}

	/// <summary>
	/// Match precision
	/// </summary>
	public enum ActiveDirectoryMatchType
	{
		/// <summary>
		/// Match entries that start with the designated string
		/// </summary>
		StartsWith,
		/// <summary>
		/// Match entries that exactly match the designated string
		/// </summary>
		Exact,
		/// <summary>
		/// Match entries that contain the designated string.
		/// </summary>
		Contains
	}

	#endregion

	/// <summary>
	/// In Use Data Type represents all data types/DTOs that need to know if its in use or not
	/// </summary>
	public enum InUseDataType
	{
		WorkspaceResources,
		SystemResources,
		SystemResourceRate,
		WorkspacePerformingOrgs
	}

	/// <summary>
	/// Levels
	/// </summary>
	public enum Level
	{
		NotSet = 0,
		Workspace = 1,
		CLINCollection = 2,
		CLIN = 3,
		BOE = 4,
		Task = 5,
		Travel = 6,
		Labor = 7
	}

	/// <summary>
	/// Date Adjust Flowdown Types
	/// </summary>
	public enum DateAdjustFlowdownType
	{
		NotSet = 0,
		Automatic = 1,
		Manual = 2,
		NoChange = 3
	}

	/// <summary>
	/// Handle the discrete selection in workspace change dates
	/// </summary>
	public enum DateAdjustDiscreteType
	{
		NotSet = 0,
		Truncate = 1,
		First = 2,
		Last = 3,
		Curve = 4,
		NoChange = 5
	}

	/// <summary>
	/// Date Adjustment Types
	/// </summary>
	public enum DateAdjustmentType
	{
		None = 0,
		Shift = 1,
		Expand = 2,
		Compress = 3,
		ShiftAndCompress = 4,
		ShiftAndExpand = 5
	}


	/// <summary>
	/// Months
	/// </summary>
	public enum Month
	{
		None = 0,
		January = 1,
		February = 2,
		March = 3,
		April = 4,
		May = 5,
		June = 6,
		July = 7,
		August = 8,
		September = 9,
		October = 10,
		November = 11,
		December = 12
	}

	/// <summary>
	/// Resource Type Field
	/// </summary>
	public enum ResourceTypeField
	{
		NotSet = 0,
		Delete,
		ElementOfCost,
		Resource,
		PerformingOrganization,
		StartDate,
		EndDate,
		SpreadCurve,
		PercentSpread,
		HoursSpread,
		CostSpread,
		ResourceWbs,
		ResourceClin
	}

	/// <summary>
	/// Property reflection options.
	/// </summary>
	public enum PropertyReflectionOptions : int
	{
		/// <summary>         
		/// Take all.         
		/// </summary>         
		All = 0,

		/// <summary>         
		/// Ignores indexer properties.         
		/// </summary>         
		IgnoreIndexer = 1,

		/// <summary>         
		/// Ignores all other IEnumerable properties         
		/// except strings.         
		/// </summary>         
		IgnoreEnumerable = 2
	}

	/// <summary>
	/// Defines the supported company configurations
	/// </summary>
	public enum CompanyConfiguration
	{
		None = 0,
		[Description("IS&GS")]
		ISGS = 1,
		[Description("Space")]
		SpaceSystems = 2,
		[Description("RMS")]
		MST = 3
	}

	/// <summary>
	/// Task Types
	/// </summary>
	public enum TaskType
	{
		Labor = 0,
		Travel = 1,
		ODC = 2
	}

	/// <summary>
	/// Integrated Non-Labor Form Type (BOE Form Type)
	/// </summary>
	public enum BOEFormType
	{
		[Display(Name = " ")]
		[Description("")]
		NotSet = 0,
		[Display(Name = "PBOE")]
		[Description("PBOE")]
		PBOE = 10,
		[Display(Name = "IBOE")]
		[Description("IBOE")]
		IBOE = 20
	}

	/// <summary>
	/// MST Travel Modes
	/// </summary>
	public enum MSTTravelMode
	{
		[Description("")]
		None = 0,
		[Description("Domestic – Zone - No Airfare")]
		ZoneNoAirfare = 1,
		[Description("Domestic – Zone - Round Trip Airfare")]
		ZoneAirfare = 2,
		[Description("Domestic – Non Zone")]
		NonZoneDomestic = 3,
		[Description("International")]
		NonZoneInternational = 4
	}

	/// <summary>
	/// Certified Cost or Pricing Data Applicability for PBOE
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
	[Flags]
	public enum Ccopd : int
	{
		[Description("")]
		None = 0,
		[Description("CCoPD applies")]
		Applies = 1,
		[Description("Commercial Item Exception applies")]
		Commercial = 2,
		[Description("Competition Exception applies")]
		Competition = 4,
		[Description("Other Exception applies")]
		Other = 8,
		[Description("< CCoPD Threshold Exception applies")]
		Threshold = 16
	}

	/// <summary>
	/// An enumeration that allows for a triple boolean state.
	/// </summary>
	public enum TripleBooleanState
	{
		[Description("N/A")]
		NA = 0,
		[Description("Yes")]
		Yes = 1,
		[Description("No")]
		No = 2
	}

	/// <summary>
	/// For PBOE, references what type of DateTime is being used for a scheduled event.
	/// </summary>
	public enum ScheduleEvent
	{
		[Description("N/A")]
		NA = 0,
		[Description("Planned")]
		Planned = 1,
		[Description("Actual")]
		Actual = 2
	}

	/// <summary>
	/// Target areas used in RateFormatter definitions.
	/// </summary>
	public enum RateTarget
	{
		[Description("PPRD")]
		PPRD,
		[Description("Rate")]
		Rate
	}

	/// <summary>
	/// RateCategory definitions.
	/// </summary>
	public enum RateCategory
	{
		[Description("Direct Labor")]
		DirectLabor = 1,
		[Description("FCCOM")]
		Fccom = 2,
		[Description("Fringe")]
		Fringe = 3,
		[Description("G&A")]
		GA = 4,
		[Description("Labor Escalation Factor")]
		LaborEscalationFactor = 5,
		[Description("Labor Escalation Percentage")]
		LaborEscalationPercentage = 6,
		[Description("Non-Labor Escalation Factor")]
		NonLaborEscalationFactor = 7,
		[Description("Non-Labor Escalation Percentage")]
		NonLaborEscalationPercentage = 8,
		[Description("Overhead")]
		Overhead = 9,
		[Description("Staff Month Conversion")]
		SMConv = 10,
		[Description("Service Center")]
		ServiceCenter = 11,
		[Description("Travel Fee")]
		TravelFee = 12,
		[Description("Travel Mlge")]
		TravelMlge = 13,
		[Description("Travel OTC")]
		TravelOtc = 14,
		[Description("Travel RC")]
		TravelRc = 15
	}

	/// <summary>
	/// Cobra Code1 enumeration.
	/// </summary>
	public enum Code1
	{
		[Description("")]
		NA = 0,
		[Description("INDIRECT")]
		INDIRECT = 1,
		[Description("SVCCTR")]
		SVCCTR = 2
	}

	/// <summary>
	/// RMS Project Map Type enumeration
	/// </summary>
	public enum ProjectMapType
	{
		[Description("Standard without Offload")]
		StandardWithoutOffload = 1,
		[Description("Standard with Offload")]
		StandardWithOffload = 2,
		[Description("Non-Time-Phased Project Map")]
		NonTimePhasedProjectMap = 3,
		[Description("Time-Phased Project Map")]
		TimePhasedProjectMap = 4
	}

	/// <summary>
	/// Enumeration for SSRS Report Types.
	/// </summary>
	public enum SSRSReportType
	{
		[Description("Project Category CLIN Cost Summary")]
		ProjectCategoryCLINCostSummary = 1,
		[Description("Project CLIN Category Cost Summary")]
		ProjectCLINCategoryCostSummary = 2,
		[Description("Cost Analysis by CLIN, CC, Activity and CY - 8 Yrs")]
		CostAnalysis8Years = 3,
		[Description("Cost Analysis by CLIN, Activity and CY - 17 Yrs")]
		CostAnalysis17Years = 4,
		[Description("Cost Analysis by CLIN, Activity and CY - 8 Yrs")]
		CostAnalysis8YearsFlat = 5
	}

	/// <summary>
	/// Enumeration for Disclosure Types
	/// </summary>
	public enum DisclosureType
	{
		[Description("")]
		None = 0,
		[Description("Legacy Space")]
		LegacySpace = 1,
		[Description("1LMX")]
		OneLMX = 2
	}

	/// <summary>
	/// DirectRateMappingResourceType enumeration.
	/// </summary>
	public enum DirectRateMappingResourceType
	{
		[Description("")]
		None = 0,
		[Description("Labor")]
		Labor = 1,
		[Description("Other")]
		Other = 2,
		[Description("Subcontractor")]
		Subcontractor = 3,
		[Description("Travel")]
		Travel = 4,
		[Description("Material")]
		Material = 5
	}

	/// <summary>
	/// DirectRate use Government or Commercial Burden Pool?
	/// </summary>
	public enum IsGovOrComm
	{
		[Description("")]
		None = 0,
		[Description("Government")]
		Government = 1,
		[Description("Commercial")]
		Commerical = 2
	}

	/// <summary>
	/// The Class of Cost for a Project Map BOE.
	/// </summary>
	public enum ClassOfCost
	{
		[Description("")]
		None = 0,
		[Description("REC")]
		Recurring = 1,
		[Description("NRE")]
		NonRecurring = 2,
		[Description("DNR")]
		DevNonRecurring = 3
	}

	/// <summary>
	/// Area being locked for edits
	/// Currently not backed by any lookup table in IES DB
	/// </summary>
	public enum LockArea
	{
		None = 0,
		[Description("RDM Sections")]
		RDMSections = 1,
		[Description("RDM Rates")]
		RDMRates = 2,
		[Description("RDM Burden Pools")]
		RDMBurdenPools = 3,
		[Description("File Attachments")]
		FileAttachments = 4,
		[Description("COBRA Data")]
		CobraData = 5,
		[Description("Rate Code Replication")]
		RateCodeReplication = 6
	}

	/// <summary>
	/// Section content types
	/// DB-backed in SectionContentTypeLU table of IES DB
	/// </summary>
	public enum SectionContentType
	{
		None = 0,
		[Description("Section")]
		Section = 1,
		[Description("Text Content")]
		Text = 2,
		[Description("Rate Table by Year")]
		RateTable = 3
	}

	/// <summary>
	/// Project Map Validation Offsets
	/// </summary>
	public enum ProjectMapValidationOffset
	{
		GridSave = 1,
		ExcelImport = 2
	}

	/// <summary>
	/// Choices for how to sort Custom Fields, Resources, and Performing Orgs.
	/// </summary>
	public enum CustomFieldSorting
	{
		/// <summary>
		/// Sort by description.
		/// </summary>
		Description = 0,

		/// <summary>
		/// Sort by Id.
		/// </summary>
		ID = 1
	}

	/// <summary>
	/// RTE Template Source
	/// </summary>
	public enum RteTemplateSource
	{
		NA = 0,
		[Description("Boe Description")]
		BoeDescription = 1,
		[Description("Boe Sources")]
		BoeSources = 2,
		[Description("Task Description")]
		TaskDescription = 3,
		[Description("Task MOQ Rationale")]
		TaskMOQ = 4
	}

	/// <summary>
	/// MOQ Table import types
	/// </summary>
	public enum MoqTableImportType
	{
		None = 0,
		CreateMoqTable = 1,
		MissingTableName = 2,
		MissingRequiredField = 3,
		LargeTableName = 4,
		LargeRepositoryName = 5,
		InvalidQueryType = 6,
		LargeContractNumber = 7,
		InvalidDateOfReport = 9,
		LargeHistoricalProgramName = 10,
		LargeWBSElement = 11,
		InvalidPopStart = 12,
		InvalidPopEnd = 13,
		InvalidPopRange = 14,
		InvalidTotalRelevantHours = 15,
		InvalidPopStartFW = 16,
		InvalidPopEndFW = 17,
		InvalidSapCalculation = 18,
		InvalidTotalWbsHours = 19,
		InvalidPopStartSunday = 20,
		InvalidPopEndSunday = 21
	}

	/// <summary>
	/// Selectable options for Repository Name
	/// </summary>
	public enum RepositoryName
	{
		None = 0,
		[Description("SAP / WEBI")]
		SapWebi = 1,
		Other = 2,
		// Below values are for when SAP Connection is NOT enabled or for RMS and should not be displayed in the repository name drop-down
		User = 3,
		SAP = 4,
		[Description("SAP - WEBI")]
		ConnectionDisabledSapWebi = 5
	}
}