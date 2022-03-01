// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System.ComponentModel;

    /// <summary>
    /// The pages security is configured to secure
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum PtmSecurityPage
    {
        /// <summary>
        /// Uninitialized/default value
        /// </summary>
        None = 0,

        /// <summary>
        /// Home
        /// </summary>
        Home = 1,

        /// <summary>
        /// administration
        /// </summary>
        Admin = 2,

        /// <summary>
        /// Reports
        /// </summary>
        Reports = 3,

        /// <summary>
        /// Proposal
        /// </summary>
        Proposal = 4,

        /// <summary>
        /// Checklist
        /// </summary>
        Checklist = 5,

        /// <summary>
        /// Checklist Report
        /// </summary>
        ChecklistReport = 6,

        /// <summary>
        /// Approvals
        /// </summary>
        Approvals = 9,

        /// <summary>
        /// Post Submittal Attachments
        /// </summary>
        PostSubmittalAttachments = 10,

        /// <summary>
        /// Proposal Certification Timeline
        /// </summary>
        CertificationTimeline = 11,

        /// <summary>
        /// Revision History Tab
        /// </summary>
        RevisionHistory = 12,

        /// <summary>
        /// Contracts Tab
        /// </summary>
        Contracts = 13
    }

    /// <summary>
    /// Cutoff Date Utilization.
    /// </summary>
    public enum CutOffDateUtilization
    {
        /// <summary>
        /// Yes, cutoff date is utilized
        /// </summary>
        [Description("Yes")]
        Yes = 0,
        [Description("No, LM did not request")]
        NoRequest = 1,
        [Description("No, LM request denied")]
        NoRequestDenied = 2

    }

    /// <summary>
    /// The Security Roles in the system
    /// </summary>
    public enum PtmRole
    {
        /// <summary>
        /// Uninitialized/default value
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Capture Manager
        /// </summary>
        [Description("Capture Manager")]
        CaptureManager = 1,

        /// <summary>
        /// Cost Volume Lead
        /// </summary>
        [Description("Cost Volume Lead")]
        CostVolumeLead = 2,

        /// <summary>
        /// Lead Estimator
        /// </summary>
        [Description("Lead Estimator")]
        Pricer = 3,

        /// <summary>
        /// Additional Estimating Resource 1
        /// </summary>
        [Description("Additional Estimating Resource 1")]
        AdditionalPricingResource1 = 4,

        /// <summary>
        /// Additional Estimating Resource 2
        /// </summary>
        [Description("Additional Estimating Resource 2")]
        AdditionalPricingResource2 = 5,

        /// <summary>
        /// Supply Chain POC (Matl)
        /// </summary>
        [Description("Material Lead")]
        SupplyChainPOCMatl = 6,

        /// <summary>
        /// Supply Chain POC (Subs)
        /// </summary>
        [Description("Subcontracts Lead")]
        SupplyChainPOCSubs = 7,

        /// <summary>
        /// Contracts POC
        /// </summary>
        [Description("Contracts Lead")]
        ContractsPOC = 8,

        /// <summary>
        /// Additional User
        /// </summary>
        [Description("Additional User")]
        AdditionalUser = 9,

        /// <summary>
        /// Admin
        /// </summary>
        [Description("Admin")]
        Admin = 10,

        /// <summary>
        /// Peer Reviewer
        /// </summary>
        [Description("Independent Reviewer")]
        PeerReviewer = 11,

        /// <summary>
        /// Backup Pricer
        /// </summary>
        [Description("Backup Estimator")]
        BackupPricer = 12,

        /// <summary>
        /// Viewer
        /// </summary>
        [Description("Viewer")]
        Viewer = 13,

        /// <summary>
        /// System Pricer
        /// </summary>
        [Description("Estimator")]
        // TODO Suspect this is not being used (no occurences in ProposalUserRole table)
        SystemPricer = 14,

        /// <summary>
        /// Proposal Setup Admin
        /// </summary>
        [Description("Proposal Setup Admin")]
        ProposalSetupAdmin = 15,

        /// <summary>
        /// Tech Lead
        /// </summary>
        [Description("Tech Lead")]
        TechLead = 16,

        /// <summary>
        /// Propsal Mgr
        /// </summary>
        [Description("Proposal Manager")]
        ProposalMgr = 17,

        /// <summary>
        /// Cover Sheet Approver
        /// </summary>
        [Description("Cover Sheet Approver")]
        CoverSheetApprover = 18,

        /// <summary>
        /// Pricing Verification
        /// </summary>
        [Description("Pricing Verification")]
        PricingVerification = 19,

        /// <summary>
        /// Lob Estimating Lead
        /// </summary>
        [Description("LOB Estimating Manager / Delegate")]
        LOBEstLead = 20,

        /// <summary>
        /// GenBOE Workspace Creator
        /// </summary>
        [Description("GenBOE Workspace Creator")]
        GenBoeWorkspaceCreator = 21,

        /// <summary>
        /// Backup Contracts POC
        /// </summary>
        [Description("Backup Contracts Lead")]
        BackupContractsPOC = 22,

        /// <summary>
        /// CAREFUL
        /// 
        /// Do not use this as a ROLE. This is only used for some decisions in which dropdown should be 
        /// used in the dropdown population
        ///
        /// Lob Estimating Manager
        /// </summary>
        [Description("LOB Estimating Manager")]
        LOBEstMgr = 99
    }

    /// <summary>
    /// Enum for the location of the menu item
    /// </summary>
    public enum MenuLocation
    {
        /// <summary>
        /// Left side menu
        /// </summary>
        Left = 0,

        /// <summary>
        /// Right side menu
        /// </summary>
        Right = 1
    }

    /// <summary>
    /// The type of Workflow approver
    /// </summary>
    public enum WorkflowApprover
    {
        /// <summary>
        /// The lead estimator
        /// </summary>
        LeadEstimator,

        /// <summary>
        /// The cover sheet
        /// </summary>
        CoverSheet,

        /// <summary>
        /// The pricing verifier
        /// </summary>
        PricingVerifier,

        /// <summary>
        /// The independent reviewer
        /// </summary>
        IndependentReviewer,

        /// <summary>
        /// The lob estimating lead
        /// </summary>
        LOBEstimatingLead
    }

    /// <summary>
    /// The current status inside the workflow for a Proposal.
    /// </summary>
    public enum WorkflowStatus
    {
        /// <summary>
        /// Workflow not started.
        /// </summary>
        [Description("Not Started")]
        NotStarted = 0,

        /// <summary>
        /// Workflow started.
        /// </summary>
        [Description("Started")]
        Started = 10,

        /// <summary>
        /// After the initial approver email has been sent.
        /// </summary>
        [Description("Initial Approver Email sent")]
        InitialApproverEmail = 20,

        /// <summary>
        /// After the second approver email has been sent.
        /// </summary>
        [Description("Second Approver Email sent")]
        SecondApproverEmail = 30,

        /// <summary>
        /// After the third approver email has been sent.
        /// </summary>
        [Description("Third Approver Email sent")]
        ThirdApproverEmail = 40,

        /// <summary>
        /// After Lead Estimator alerted that approver(s) have not approved after three emails.
        /// </summary>
        [Description("Lead Estimator Alert sent about Approver(s)")]
        LeadEstimatorAlert = 50,

        /// <summary>
        /// All approved.
        /// </summary>
        [Description("All Approvers approved")]
        AllApproved = 60,

        /// <summary>
        /// After the initial lob lead email has been sent.
        /// </summary>
        [Description("Initial LOB Lead Email sent")]
        InitialLOBLeadEmail = 70,

        /// <summary>
        /// After the Second lob lead email has been sent.
        /// </summary>
        [Description("Second LOB Lead Email sent")]
        SecondLOBLeadEmail = 80,

        /// <summary>
        /// After the third lob lead email has been sent.
        /// </summary>
        [Description("Third LOB Lead Email sent")]
        ThirdLOBLeadEmail = 90,

        /// <summary>
        /// After Lead Estimator alerted that LOB Lead have not approved after three emails.
        /// </summary>
        [Description("Lead Estimator Alert sent about LOB Lead")]
        LeadEstimatorAlertLOB = 100,

        /// <summary>
        /// After LOB Lead has approved, the proposal is now locked.
        /// </summary>
        [Description("Proposal Locked")]
        ProposalLocked = 130
    }

    /// <summary>
    /// Defines proposal status
    /// </summary>
    public enum ProposalStatus
    {
        /// <summary>
        /// In Progress
        /// </summary>
        [Description("In Progress")]
        InProgress = 1,

        /// <summary>
        /// Completed
        /// </summary>
        [Description("Completed")]
        Completed = 2,

        /// <summary>
        /// Archived
        /// </summary>
        [Description("Archived")]
        Archived = 3,

        /// <summary>
        /// Deleted
        /// </summary>
        [Description("Deleted")]
        Deleted = 4,

        /// <summary>
        /// Pending Certification
        /// </summary>
        [Description("Pending Certification")]
        PendingCertification = 6,

        /// <summary>
        /// No Bid
        /// </summary>
        [Description("No Bid")]
        NoBid = 7,

        /// <summary>
        /// Revised
        /// </summary>
        [Description("Revised")]
        Revised = 8,

        /// <summary>
        /// Pending Contractual Award
        /// </summary>
        [Description("Pending Contractual Award")]
        PendingAward = 9,

        /// <summary>
        /// Lost
        /// </summary>
        [Description("Lost")]
        Lost = 10
    }

    /// <summary>
    /// Defines Proposal Checklist Types
    /// </summary>
    public enum ProposalChecklistType
    {
        /// <summary>
        /// Uninitialized/default value
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// The default checklist type
        /// </summary>
        Default = 1,

        /// <summary>
        /// The international/Commercial checklist.
        /// </summary>
        [IES.Common.IsActive(false)]
        InternationalCommercial = 2
    }

    /// <summary>
    /// Defines customer type
    /// </summary>
    public enum CustomerType
    {
        /// <summary>
        /// Uninitialized/default value
        /// </summary>
        [Description("Select End Customer Type")]
        NotSet = 0,

        /// <summary>
        /// Commercial
        /// </summary>
        [Description("Commercial")]
        Commercial = 1,

        /// <summary>
        /// Federal Government
        /// </summary>
        [Description("Federal Government")]
        FederalGovernment = 2,

        /// <summary>
        /// International – Commercial
        /// </summary>
        [Description("International – Commercial")]
        InternationalCommercial = 3,

        /// <summary>
        /// International-Foreign Military Sale(FMS)-US Gov’t
        /// </summary>
        [Description("International-Foreign Military Sale(FMS)-US Gov’t")]
        InternationalForeignMilitarySaleUSGovt = 4,

        /// <summary>
        /// State and Local
        /// </summary>
        [Description("State and Local")]
        StateAndLocal = 5,

        /// <summary>
        /// N/A
        /// </summary>
        [Description("N/A")]
        [IES.Common.IsActive(false)]
        NA = 6
    }

    /// <summary>
    /// Defines ISGS Role
    /// </summary>
    public enum ISGSRole
    {
        /// <summary>
        /// Uninitialized/default value
        /// </summary>
        [Description("Select Role")]
        NotSet = 0,

        /// <summary>
        /// Prime
        /// </summary>
        [Description("Prime")]
        Prime = 1,

        /// <summary>
        /// Sub
        /// </summary>
        [Description("Sub")]
        Sub = 2,

        /// <summary>
        /// IWTA
        /// </summary>
        [Description("IWTA")]
        IWTA = 3,

        /// <summary>
        /// N/A
        /// </summary>
        [Description("N/A")]
        [IES.Common.IsActive(false)]
        NA = 4
    }

    /// <summary>
    /// Proposal location where the proposal was created
    /// Virtual implies multiple locations
    /// "Other" allows free form location data to be entered
    /// </summary>
    public enum ProposalLocation
    {
        /// <summary>
        /// Select Proposal Location
        /// </summary>
        [Description("Select Proposal Location")]
        NotSet = 0,

        /// <summary>
        /// Chantilly, VA
        /// </summary>
        [Description("Chantilly, VA"), IES.Common.IsActive(false)]
        ChantillyVA = 1,

        /// <summary>
        /// Colorado Springs, CO
        /// </summary>
        [Description("Colorado Springs, CO"), IES.Common.IsActive(false)]
        ColoradoSpringsCO = 2,

        /// <summary>
        /// Hanover, MD
        /// </summary>
        [Description("Hanover, MD"), IES.Common.IsActive(false)]
        HanoverMD = 3,

        /// <summary>
        /// Herndon, VA
        /// </summary>
        [Description("Herndon, VA"), IES.Common.IsActive(false)]
        HerndonVA = 4,

        /// <summary>
        /// Littleton, CO
        /// </summary>
        [Description("Littleton, CO"), IES.Common.IsActive(false)]
        LittletonCO = 5,

        /// <summary>
        /// Rockville, MD
        /// </summary>
        [Description("Rockville, MD"), IES.Common.IsActive(false)]
        RockvilleMD = 6,

        /// <summary>
        /// Valley Forge, PA
        /// </summary>
        [Description("Valley Forge, PA")]
        ValleyForgePA = 7,

        /// <summary>
        /// Virtual
        /// </summary>
        [Description("Virtual"), IES.Common.IsActive(false)]
        Virtual = 8,

        /// <summary>
        /// Other
        /// </summary>
        [Description("Other")]
        Other = 9,

        /// <summary>
        /// Cape Canaveral, FL
        /// </summary>
        [Description("Cape Canaveral, FL")]
        CapeCanaveralFL = 10,

        /// <summary>
        /// Denver, CO
        /// </summary>
        [Description("Denver, CO")]
        DenverCO = 11,

        /// <summary>
        /// Huntsville, AL
        /// </summary>
        [Description("Huntsville, AL")]
        HuntsvilleAL = 12,

        /// <summary>
        /// Michoud, LA
        /// </summary>
        [Description("Michoud, LA")]
        MichoudLA = 13,

        /// <summary>
        /// Sunnyvale, CA
        /// </summary>
        [Description("Sunnyvale, CA")]
        SunnyvaleCA = 14,

        /// <summary>
        /// Titusville, FL
        /// </summary>
        [Description("Titusville, FL")]
        TitusvilleFL = 15
    }

    /// <summary>
    /// Defines pricing tool
    /// "Other" allows free form Pricing Tool data to be entered
    /// </summary>
    public enum PricingTool
    {
        /// <summary>
        /// Uninitialized/default value
        /// </summary>
        [Description("Select Pricing Tool")]
        NotSet = 0,

        /// <summary>
        /// ProPricer
        /// </summary>
        [Description("ProPricer")]
        ProPricer = 1,

        /// <summary>
        /// Excel
        /// </summary>
        [Description("Excel")]
        Excel = 2,

        /// <summary>
        /// Other
        /// </summary>
        [Description("Other")]
        Other = 3
    }

    /// <summary>
    /// Defines BOE tool
    /// "Other" allows free form BOE Tool data to be entered
    /// </summary>
    public enum BOETool
    {
        /// <summary>
        /// Uninitialized/default value
        /// </summary>
        [Description("Select BOE Tool")]
        NotSet = 0,

        /// <summary>
        /// genBOE
        /// </summary>
        [Description("genBOE")]
        genBOE = 1,

        /// <summary>
        /// wBOE
        /// </summary>
        [Description("wBOE")]
        [IES.Common.IsActive(false)]
        wBOE = 2,

        /// <summary>
        /// ABE
        /// </summary>
        [Description("ABE")]
        [IES.Common.IsActive(false)]
        ABE = 3,

        /// <summary>
        /// Excel
        /// </summary>
        [Description("Excel")]
        Excel = 4,

        /// <summary>
        /// Word
        /// </summary>
        [Description("Word")]
        Word = 5,

        /// <summary>
        /// Other
        /// </summary>
        [Description("Other")]
        Other = 6,

        /// <summary>
        /// N/A
        /// </summary>
        [Description("N/A")]
        [IES.Common.IsActive(false)]
        NA = 7
    }

    /// <summary>
    /// Defines resource type
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// Uninitialized/default value
        /// </summary>
        [Description("Select Resource Type")]
        NotSet = 0,

        /// <summary>
        /// Pricer
        /// </summary>
        [Description("Estimator")]
        Pricer = 3,

        /// <summary>
        /// Strategist
        /// </summary>
        [Description("Strategist")]
        Strategist = 4,

        /// <summary>
        /// Parametric Estimator
        /// </summary>
        [Description("Parametric Estimator")]
        ParametricEstimator = 5,

        /// <summary>
        /// Other
        /// </summary>
        [Description("Other")]
        Other = 6
    }

    /// <summary>
    /// Defines cost element type
    /// </summary>
    public enum CostElementType
    {
        /// <summary>
        /// IWTA
        /// </summary>
        [Description("IWTA")]
        IWTA = 1,

        /// <summary>
        /// Labor
        /// </summary>
        [Description("Labor")]
        Labor = 2,

        /// <summary>
        /// Materials
        /// </summary>
        [Description("Materials")]
        Materials = 3,

        /// <summary>
        /// Other Direct Charges (ODC)
        /// </summary>
        [Description("Other Direct Charges (ODC)")]
        ODC = 4,

        /// <summary>
        /// Subcontractors
        /// </summary>
        [Description("Subcontractors")]
        Subs = 5,

        /// <summary>
        /// Travel
        /// </summary>
        [Description("Travel")]
        Travel = 6,

        /// <summary>
        /// TDY
        /// </summary>
        [Description("TDY/Relocation")]
        TDY = 7
    }

    /// <summary>
    /// Defines proposal filter options
    /// </summary>
    public enum ProposalFilterOption
    {
        /// <summary>
        /// In Progress
        /// </summary>
        [Description("In Progress")]
        InProgress = 0,

        /// <summary>
        /// Completed
        /// </summary>
        [Description("Completed")]
        Completed = 1,

        /// <summary>
        /// No Bid
        /// </summary>
        [Description("No Bid")]
        NoBid = 2,

        /// <summary>
        /// All
        /// </summary>
        [Description("All")]
        All = 3
    }

    /// <summary>
    /// Defines Proposal Class filter options.
    /// </summary>
    public enum ProposalClassFilterOption
    {
        /// <summary>
        /// All
        /// </summary>
        [Description("All")]
        All = 0,

        /// <summary>
        /// Forecasted
        /// </summary>
        [Description("Forecasted")]
        Forecasted = 1,

        /// <summary>
        /// Non-Forecasted
        /// </summary>
        [Description("Non-Forecasted")]
        NonForecasted = 2
    }

    /// <summary>
    /// Defines viewer filter options for proposals.  
    /// These options are only available when the user has the Viewer role or is a memeber of a group with Viewer role.
    /// </summary>
    public enum ViewerProposalFilterOption
    {
        /// <summary>
        /// Show only the user's proposals
        /// </summary>
        [Description("Show only my proposals")]
        ShowOnlyMyProposals = 0,

        /// <summary>
        /// Show proposals for product lines (LOBs) where the user has Viewer privileges.
        /// </summary>
        [Description("Show proposals for my organization")]
        ShowProposalsForMyOrganization = 1
    }

    /// <summary>
    /// Proposal Revision response type: Pricer = 1, Peer = 2
    /// </summary>
    public enum RevisionResponseType
    {
        /// <summary>
        /// Pricer
        /// </summary>
        Pricer = 1,

        /// <summary>
        /// Peer
        /// </summary>
        Peer = 2
    }

    /// <summary>
    /// Checklist type: ProposalPricingReview  = 1, ProposalAdequacyReview = 2
    /// </summary>
    public enum ChecklistType
    {
        /// <summary>
        /// Proposal Pricing Review
        /// </summary>
        ProposalPricingReview = 1,

        /// <summary>
        /// Proposal Adequacy Review
        /// </summary>
        ProposalAdequacyReview = 2
    }

    /// <summary>
    /// Checklist text type: Introduction = 1, Header = 2, Text = 3, Question = 4, PricerComment = 5, PeerComment = 6
    /// </summary>
    public enum ChecklistTextType
    {
        /// <summary>
        /// Introduction
        /// </summary>
        Introduction = 1,

        /// <summary>
        /// Header
        /// </summary>
        Header = 2,

        /// <summary>
        /// Text
        /// </summary>
        Text = 3,

        /// <summary>
        /// Question
        /// </summary>
        Question = 4,

        /// <summary>
        /// Pricer Comment
        /// </summary>
        PricerComment = 5,

        /// <summary>
        /// Peer Comment
        /// </summary>
        PeerComment = 6
    }

    /// <summary>
    /// Checklist response option: Yes = 1, No = 2, NA = 3, NotSet = 5
    /// </summary>
    public enum ChecklistResponseOption
    {
        /// <summary>
        /// Yes
        /// </summary>
        Yes = 1,

        /// <summary>
        /// No
        /// </summary>
        No = 2,

        /// <summary>
        /// N/A
        /// </summary>
        NA = 3,

        /// <summary>
        /// Not Set
        /// </summary>
        NotSet = 5
    }

    /// <summary>
    /// Checklist response type: Pricer = 1 or Peer = 2
    /// </summary>
    public enum ChecklistResponseType
    {
        /// <summary>
        /// Pricer
        /// </summary>
        Pricer = 1,

        /// <summary>
        /// Peer
        /// </summary>
        Peer = 2
    }

    /// <summary>
    /// Checklist response type: Pricer, Peer, ShowBoth
    /// </summary>
    public enum ShowChecklistResponse
    {
        /// <summary>
        /// Pricer
        /// </summary>
        Pricer = 1,

        /// <summary>
        /// Peer
        /// </summary>
        Peer = 2,

        /// <summary>
        /// Show both pricer and peer reviewer
        /// </summary>
        ShowBoth = 3
    }

    /// <summary>
    /// Checklist display types to determine read only
    /// </summary>
    public enum ChecklistDisplayTypesForReadOnly
    {
        /// <summary>
        /// display checklist index
        /// </summary>
        DisplayChecklistIndex = 1,

        /// <summary>
        /// display general information
        /// </summary>
        DisplayChecklistGeneralInformation = 2,

        /// <summary>
        /// dispaly checklist proposal pricing data
        /// </summary>
        DisplayChecklistProposalPricingData = 3,

        /// <summary>
        /// display checklist PPR document data
        /// </summary>
        DisplayChecklistPPRDocument = 4,

        /// <summary>
        /// display checklist PAR document data
        /// </summary>
        DisplayChecklistPARDocument = 5
    }

    /// <summary>
    /// Unlock Checklist Options
    /// </summary>
    public enum UnlockChecklistOption
    {
        /// <summary>
        /// Uninitialized/default value
        /// </summary>
        [Description("Select Role(s) to Unlock")]
        NotSet = 0,

        /// <summary>
        /// Unlock Pricer
        /// </summary>
        [Description("Unlock Lead Estimator")]
        UnlockPricer = 1,

        /// <summary>
        /// Unlock Peer
        /// </summary>
        [Description("Unlock Peer")]
        UnlockPeer = 2,

        /// <summary>
        /// Unlock Both
        /// </summary>
        [Description("Unlock both Lead Estimator and Independent Reviewer")]
        UnlockBoth = 3
    }

    /// <summary>
    /// Defines proposal status for the reports (proposal log and proposal activity)
    /// </summary>
    public enum ProposalReportStatus
    {
        /// <summary>
        /// Active (In Progress and Completed)
        /// </summary>
        [Description("Active")]
        Active = 1,

        /// <summary>
        /// Archived
        /// </summary>
        [Description("Archived")]
        Archived = 3,

        /// <summary>
        /// Deleted
        /// </summary>
        [Description("Deleted")]
        Deleted = 4,

        /// <summary>
        /// No Bid
        /// </summary>
        [Description("No Bid")]
        NoBid = 7,

        /// <summary>
        /// Revised
        /// </summary>
        [Description("Revised")]
        Revised = 8,

        /// <summary>
        /// All
        /// </summary>
        [Description("All")]
        All = 100
    }

    /// <summary>
    /// Defines filter option for the proposal log report
    /// </summary>
    public enum ProposalLogFilterOption
    {
        /// <summary>
        /// All proposals
        /// </summary>
        AllProposalsFilter = 1,

        /// <summary>
        /// specific proposals
        /// </summary>
        SpecificProposalRadioFilter = 2,

        /// <summary>
        /// submit date range
        /// </summary>
        SubmitDateRangeFilter = 3,

        /// <summary>
        /// tracking number
        /// </summary>
        TrackingNumberFilter = 4
    }

    /// <summary>
    /// Proposal Activity Report's Filter Option
    /// </summary>
    public enum ProposalActivityFilterOption
    {
        /// <summary>
        /// All customer types
        /// </summary>
        AllCustomerTypesFilter = 1,

        /// <summary>
        /// Specific customer types and or/Program Areas
        /// </summary>
        SpecificCustomerTypesFilter = 2
    }

    /// <summary>
    /// The email types used for sending a past due or due soon email for a Proposal.
    /// </summary>
    public enum EmailType
    {
        /// <summary>
        /// The initial approval email for approvers
        /// </summary>
        InitialApprovalEmail,

        /// <summary>
        /// The second approval email for approvers
        /// </summary>
        SecondApprovalEmail,

        /// <summary>
        /// The third approval email for approvers
        /// </summary>
        FinalApprovalEmail,

        /// <summary>
        /// The lead alert for when approvers have not approved after 3 emails
        /// </summary>
        LeadAlertForApprovers,

        /// <summary>
        /// The initial lob approval email
        /// </summary>
        InitialLOBApprovalEmail,

        /// <summary>
        /// The second lob approval email
        /// </summary>
        SecondLOBApprovalEmail,

        /// <summary>
        /// The final lob approval email
        /// </summary>
        FinalLOBApprovalEmail,

        /// <summary>
        /// The lead alert for when LOB Approver has not approved after 3 emails
        /// </summary>
        LeadAlertForLOBApprover,

        /// <summary>
        /// The lob approved email
        /// </summary>
        LOBApprovedEmail,

        /// <summary>
        /// The forecast alert email
        /// </summary>
        ForecastAlertEmail,

        /// <summary>
        /// The certification timeline email
        /// </summary>
        CertificationTimelineEmail,

        /// <summary>
        /// The reminder email for when the optional document is missing
        /// </summary>
        OptionalDocumentReminderEmail,

        /// <summary>
        /// The proposal status has been set to "Lost"
        /// </summary>
        LostStatusSet
    }

    /// <summary>
    /// Defines program/proposal status
    /// </summary>
    public enum ProgramProposalStatus
    {
        /// <summary>
        /// Uninitialized/default value
        /// </summary>
        [Description("Select Program/Proposal Status")]
        NotSet = 0,

        /// <summary>
        /// Under Strategic Review – ISGS
        /// </summary>
        [Description("Under Strategic Review – IS&GS")]
        UnderStrategicReviewISGS = 1,

        /// <summary>
        /// Under Strategic Review – Tech Services
        /// </summary>
        [Description("Under Strategic Review – Tech Services")]
        UnderStrategicReviewTechServices = 2,

        /// <summary>
        /// LM Retained – SSC
        /// </summary>
        [Description("LM Retained – SSC")]
        LMRetainedSSC = 3,

        /// <summary>
        /// LM Retained – MST
        /// </summary>
        [Description("LM Retained – RMS")]
        LMRetainedMST = 4
    }

    /// <summary>
    /// The type of Post Submittal Attachment on a proposal.
    /// </summary>
    public enum AttachmentType
    {
        /// <summary>
        /// A Cost Kickoff attachment.
        /// </summary>
        [Description("Cost Kick-Off Package")]
        CostKickOffPackage = 1,

        /// <summary>
        /// A Responsibility Assignments Matrix attachment.
        /// </summary>
        [Description("Responsibility Assignments Matrix (RAM)")]
        ResponsibilityAssignmentsMatrix = 2,

        /// <summary>
        /// A Delegation of Authority attachment.
        /// </summary>
        [Description("Documented Approval to Submit (and DOA if applicable) (<a href=\"https://space.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Documented%20Approval%20to%20Submit%20(and%20DOA%20if%20applicable).docx?Web=1\" target=\"_blank\">Additional information</a>)")]
        DelegationOfAuthority = 3,

        /// <summary>
        /// Arbitrary file uploaded by the user.
        /// </summary>
        [Description("Other")]
        Other = 4
    }

    public enum CannedResponse
    {
        [Description("Canned Response 1")]
        Response1,

        [Description("Canned Response 2")]
        Response2,

        [Description("Canned Response 3")]
        Response3,

        [Description("Other")]
        Other
    }

    /// <summary>
    /// Enumeration for Reason why Certification is Not Required
    /// </summary>
    public enum ReasonCertificationNotRequired
    {
        [Description("Lost / Not Awarded")]
        LostNotAwarded = 1,

        [Description("Awarded Under Threshold")]
        AwardedUnderThreshold = 2,

        [Description("Other")]
        Other = 3
    }
}
