PRINT '###### SCRIPT IS STARTING ######';
/*
    This file was auto-generated for Release: 2025.3, on 7/16/2025.
    It contains all of the Release specific scripts, modifying data/tables as well as all of the Stored Procedures and User Defined Table Types.
*/

/*
    File: \Release 2025.3\1 - Release 2025.3 Script.sql
*/
PRINT '### Starting file: \Release 2025.3\1 - Release 2025.3 Script.sql';
EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.3';
GO

-- Author: Oyeyemi Oyetoro
-- JIRA Story: PROPH-2835
-- Added Program Mgr to Role table

IF NOT EXISTS (SELECT * FROM dbo.[RoleLU] WHERE RoleID IN (25))
BEGIN
	SET IDENTITY_INSERT dbo.[RoleLU] ON;

	INSERT INTO [RoleLU]
		(RoleID, [Role])
	VALUES
		(25, 'Program Mgr')
		
	SET IDENTITY_INSERT dbo.[RoleLU] OFF;
END
GO

/*
    File: \1 Views\genTracData.view.sql
*/
PRINT '### Starting file: \1 Views\genTracData.view.sql';
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[genBOE].[genTracData]') AND type in (N'V'))
	DROP VIEW [genBOE].[genTracData]
GO

CREATE VIEW [genBOE].[genTracData] AS 
/******************************************************************************
**		 
**		Name: [genBOE].[genTracData]
**		Desc: 
**			
**		
**
**		Auth: Unknown
**		Date: Unknown
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		01/19/17	n99040				added alias LeadEstimatorName
**      2/15/17		twilson3			Fixed Independent Reviewer filter to use 'Peer Reviewer' as the text
**		2/15/17		Dusan				Removed RoleType
**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
**		11/19/19	twilson3			BOEJ-4312 Do not return Deleted Proposals
**		10/29/21	Dusan				IES-460: Use Revised Anticipated Delivery Date when Available
**		10/12/22	RJ					IES-1933 Cost Volume Tool
*******************************************************************************/
SELECT DISTINCT
	P.ProposalID AS [genTracProposalID],
	P.ProposalTrackingID AS [TrackingNumber],
	P.ProposalTitle AS [ProposalTitle],
	LOB.LineOfBusinessName AS [LineOfBusinessName],
	PA.ProgramAreaName AS [ProgramAreaName],
	PT.ProposalType AS [ProposalType],		
	PS.ProposalStatus AS [ProposalStatus],	
	
	P.EstimatedProposalValue AS [EstimatedValue],
	PC.ISGSTotalPrice AS [SubmittedValue],
	

	--per BOEJ-1546, replaced start/end dates with RFPReceivedDate & AnticpatedDeliveryDate

	CAST(P.RFPReceivedDate AS DATE) AS [ProposalStartDate],
	CASE WHEN p.RevisedSubmittalDate IS NOT NULL THEN CAST(p.RevisedSubmittalDate AS DATE) ELSE CAST(p.AnticipatedDeliveryDate AS DATE) END AS [ProposalEndDate],
	CAST (P.DateCreated AS DATE) AS [CreatedDate],
	CAST(PC.ProposalSubmittalDate AS DATE) AS [SubmittalDate],
	
	P.ProgramName AS [ProgramName],
		
	LEADESTIMATOR.DisplayName AS [LeadEstimator],		
	LEADESTIMATOR.DisplayName AS [LeadEstimatorName], -- report dataset field name consistency
	AER1.AdditionalEstimatingResource1 AS [AdditionalEstimatingResource1],
	AER2.AdditionalEstimatingResource2 AS [AdditionalEstimatingResource2],
	
	P.Customer AS [Customer],
	CT.CustomerType AS [CustomerType],
	
	CL.ContractsLead AS [ContractsLead],
		
	CVL.DisplayName AS [CostVolumeLead],
	
	Mngr.Manager AS Manager,
	
	PrcT.PricingTool AS [PricingTool],
	
	BT.BOETool AS [BOETool],

	CVT.CostVolumeTool AS [CostVolumeTool],
	
	dbo.udfCreateCommaSeparatedList (P.ProposalID, 1) AS [ContractType],
	dbo.udfCreateCommaSeparatedList (P.ProposalID, 2) AS [ElementsOfCost],
	
	P.IsIWTA AS [IWTA],
	
	CASE I.ISGSRole
		WHEN 'IWTA' THEN ''
		ELSE I.ISGSRole
	END AS [PrimeOrSub],
		
	P.RFPNumber AS [RFPNumber],
	PROPOSALMANAGER.DisplayName AS [ProposalManager],
	COVERSHEET.DisplayName AS [CoverSheetApprover],
	PRICINGVERIFIER.DisplayName AS [PricingVerifier],
	INDP_REVIEWER.DisplayName AS [IndependentReviewer],
	MATERIALLEAD.DisplayName AS [MaterialLead],
	SUBCONTRACTLEAD.DisplayName AS [SubcontractLead]
	
FROM [dbo].[Proposal] P
	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.RoleID = 3 /*LEADESTIMATOR*/  
		) LEADESTIMATOR ON P.ProposalID = LEADESTIMATOR.ProposalID

	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.Role = 'Proposal Mgr'
		) PROPOSALMANAGER ON P.ProposalID = PROPOSALMANAGER.ProposalID

	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.Role = 'Material Lead'
		) MATERIALLEAD ON P.ProposalID = MATERIALLEAD.ProposalID

	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.Role = 'Subcontract Lead'
		) SUBCONTRACTLEAD ON P.ProposalID = SUBCONTRACTLEAD.ProposalID

	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.Role = 'Cover Sheet Approver'
		) COVERSHEET ON P.ProposalID = COVERSHEET.ProposalID
		
	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName				
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.Role = 'Pricing Verification'
		) PRICINGVERIFIER ON P.ProposalID = PRICINGVERIFIER.ProposalID
	
	LEFT OUTER  JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.Role = 'Peer Reviewer' --Independent Reviewer name in RoleLU table
		) INDP_REVIEWER ON P.ProposalID = INDP_REVIEWER.ProposalID	

	INNER JOIN [dbo].[ProgramAreaLU] PA ON P.ProgramAreaID = PA.ProgramAreaID
	INNER JOIN [dbo].[LineOfBusinessLU] LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
	INNER JOIN [dbo].[ProposalTypeLU] PT ON P.ProposalTypeID = PT.ProposalTypeID

	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName
			FROM [dbo].[ProposalUserRole] PUR 
				INNER JOIN [dbo].[RoleLU] R ON PUR.RoleID = R.RoleID
				INNER JOIN [dbo].[genTRACUser] U ON PUR.UserID = U.UserID
			WHERE 
				R.RoleID = 2 /*Cost Volume Lead*/
		) CVL ON P.ProposalID = CVL.ProposalID
		
	LEFT OUTER JOIN [dbo].[ProposalChecklist] PC ON P.ProposalID = PC.ProposalID
	INNER JOIN [dbo].[BOEToolLU] BT ON BT.BOEToolID = P.BOEToolID
	INNER JOIN [dbo].[PricingToolLU] PrcT ON PrcT.PricingToolID = P.PricingToolID
	INNER JOIN [dbo].[CostVolumeToolLU] CVT ON CVT.CostVolumeToolID = P.CostVolumeToolID
	INNER JOIN [dbo].[ProposalStatusLU] PS ON PS.ProposalStatusID = P.ProposalStatusID
	INNER JOIN [dbo].[CustomerTypeLU] CT ON CT.CustomerTypeID = P.CustomerTypeID
	INNER JOIN [dbo].[ISGSRoleLU] I ON I.ISGSRoleID = P.ISGSRoleID
	
	LEFT OUTER JOIN
	(
		SELECT 
			ProposalID,
			MAX(SubmitDate) AS MaxSubmitDate
		FROM dbo.ProposalChecklistComplete
		GROUP BY ProposalID
	) PCE ON P.ProposalID = PCE.ProposalID
	
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS ContractsLead
		FROM dbo.ProposalUserRole PUR
			INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 8 /*Contracts Lead*/
	) CL ON P.ProposalID = CL.ProposalID
	
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS AdditionalEstimatingResource1
		FROM dbo.ProposalUserRole PUR
			INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 4	/*Additional Estimating Resource 1*/
	) AER1 ON P.ProposalID = AER1.ProposalID	
	
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS AdditionalEstimatingResource2
		FROM dbo.ProposalUserRole PUR
			INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 5	/*Additional Estimating Resource 2*/
	) AER2 ON P.ProposalID = AER2.ProposalID

	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS Manager			
		FROM dbo.ProposalUserRole PUR
			INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 1 /*Capture Manager*/
	) Mngr ON P.ProposalID = Mngr.ProposalID
WHERE PS.ProposalStatus != 'Deleted'

GO

/*
    File: \1 Views\vwDfarsChecklistResponseReport.sql
*/
PRINT '### Starting file: \1 Views\vwDfarsChecklistResponseReport.sql';
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vwDfarsChecklistResponseReport]') AND type in (N'V'))
	DROP VIEW [dbo].[vwDfarsChecklistResponseReport]
GO

CREATE VIEW [dbo].[vwDfarsChecklistResponseReport] AS
/******************************************************************************
**		 
**		Name: [vwDfarsChecklistResponseReport]
**		Desc: DFARS Checklist Response Report View
**			
**		
**
**		Auth: ranzalon
**		Date: 5/14/19
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/03/19	ranzalon			BOEJ-4217 - Updates based on feedback
*******************************************************************************/
SELECT  
	P.ProposalID AS ProposalID,	
	LOB.LineOfBusinessID as LineOfBusinessID,
	LOB.LineOfBusinessName AS [Line of Business],	
	PA.ProgramAreaID as ProgramAreaID,
	PA.ProgramAreaName as [Program Area Name],
	P.ProposalTrackingID AS [Tracking #],
	P.ProposalTitle AS [Proposal Title],
	CAST(PC.ProposalSubmittalDate AS DATE) AS [Actual Submittal Date],
	PCC.[SubmitDate] AS [Approval Workflow Completed Date],	
	P.[CertificationTimelineCompleted] AS [Certification Completed Date],
	LeadEstimator.UserID AS [LeadEstimatorUserID],
	LeadEstimator.DisplayName AS [LeadEstimatorName],		
	PS.ProposalStatusID AS [ProposalStatusID],
	PS.ProposalStatus AS [Proposal Status],
	ParCC.[QuestionNumber] AS [Question #],
	ParCC.[ChecklistText] AS [Question Text],
	RLU.[ResponseID] AS [ResponseID],
	RLU.[Response] AS [Response],
	PPCX.[CannedResponseId] AS [Selection],
	PPCX.[Comment] As [Comment]

  FROM [dbo].[Proposal] P
	INNER JOIN [dbo].[ProposalChecklistComplete] PCC ON PCC.ProposalID = P.ProposalID
	INNER JOIN [dbo].[LineOfBusinessLU] LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID	
	INNER JOIN [dbo].[ProgramAreaLU] PA ON P.ProgramAreaID = PA.ProgramAreaID
	INNER JOIN dbo.ProposalStatusLU PS ON P.ProposalStatusID = PS.ProposalStatusID
	LEFT OUTER JOIN dbo.ProposalChecklist PC ON P.ProposalID = PC.ProposalID
	LEFT OUTER JOIN
	(
		SELECT
			PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
		FROM ProposalUserRole PUR
			JOIN genTRACUser U ON PUR.UserID = U.UserID
		WHERE
			PUR.RoleID = 3 --Lead Estimator Role
	) LeadEstimator ON P.ProposalID = LeadEstimator.ProposalID
	LEFT OUTER JOIN dbo.ProposalPARChecklistXREF PPCX ON PPCX.ProposalID = P.ProposalID
	LEFT OUTER JOIN dbo.PARChecklistContent ParCC ON PPCX.PARChecklistContentID = ParCC.PARChecklistContentID
	LEFT OUTER JOIN dbo.ResponseLU RLU ON RLU.ResponseID = PPCX.ResponseID
	LEFT OUTER JOIN dbo.CannedResponsesPAR CR ON CR.CannedResponseId = PPCX.CannedResponseId
	LEFT OUTER JOIN dbo.ProposalAdequacyReview PAR ON PAR.ProposalAdequacyReviewID = ParCC.ProposalAdequacyReviewID
	WHERE RLU.[ResponseID] = 2 --'No' Response
		AND PAR.ChecklistVersion >= 12 --Earliest version with Canned Responses
		AND (PS.ProposalStatusID = 2 OR PS.ProposalStatusID = 6 OR PS.ProposalStatusID = 9) --'Completed', 'Pending Certification', 'Pending Contractual Award' Status
		AND PPCX.[CannedResponseId] is NULL --Other is Selected, so response ID is null
		AND PCC.[ChecklistTypeID] = 1 --Default Checklist Type

GO



/*
    File: \1 Views\vwProgramArea.view.sql
*/
PRINT '### Starting file: \1 Views\vwProgramArea.view.sql';
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vwProgramArea]') AND type in (N'V'))
	DROP VIEW [dbo].[vwProgramArea]
GO

CREATE VIEW [dbo].[vwProgramArea]
AS
SELECT [ProgramAreaID]
      ,[ProgramAreaName]
  FROM [dbo].[ProgramAreaLU]

GO

/*
    File: \1 Views\vwProposalActivityReport.view.sql
*/
PRINT '### Starting file: \1 Views\vwProposalActivityReport.view.sql';
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vwProposalActivityReport]') AND type in (N'V'))
	DROP VIEW [dbo].[vwProposalActivityReport]
GO

/****** Object:  View [dbo].[vwProposalActivityReport]    Script Date: 09/25/2013 07:38:34 ******/
CREATE VIEW [dbo].[vwProposalActivityReport] AS
/******************************************************************************
**		 
**		Name: [vwProposalActivityReport]
**		Desc: 
**			
**		
**
**		Auth: Unknown
**		Date: Unknown
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		01/19/17	n99040				added fields TechLead, IndependentReviewer, LeadEstimator, ProposalMgr, CoverSheetApprover, PricingVErification
**		02/02/17	tglick				added new field [Revised Submittal Date]
**		2/15/17		Dusan				Removed RoleType, Added Absolute Value
**		3/22/2017	twilson3			BOEJ-1909 Remove Profit Tracking #
**		3/19/2018	twilson3			BOEJ-3126 Add Forecast Tracking # to SSRS
**		5/30/2018	ranzalon			BOEJ-3405 - Adjusted naming of Actual Submittal Date
**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
*******************************************************************************/
SELECT  
	P.ProposalID AS [ProposalID],

	Pricer.[Pricer Name] AS [Pricer],
	Pricer.PricerNTID AS [PricerNTID],
	Pricer.PricerUserID AS [PricerUserID],

	IndependentReviewer.DisplayName AS [IndependentReviewerName],
	PricingVerification.DisplayName AS [PricingVerificationName],
	CoverSheetApprover.DisplayName AS [CoverSheetApproverName],
	ProposalMgr.DisplayName AS [ProposalMgrName],
	TechLead.DisplayName AS [TechLeadName],
	LeadEstimator.DisplayName AS [LeadEstimatorName],

	P.ProposalTrackingID AS [Tracking #],
	P.ForecastedTrackingId AS [ForecastedTracking#],
	P.ProposalTitle AS [Proposal Title],
	P.DateCreated AS DateCreated,
	
	PA.ProgramAreaID as ProgramAreaID,
	LOB.LineOfBusinessID as LineOfBusinessID,
	LOB.LineOfBusinessName as [Line Of Business],
	PA.ProgramAreaName as [Program Area Name],

	P.Customer AS [Customer],
	P.AnticipatedDeliveryDate AS [Estimated Ship Date],
--	'$' + REPLACE(CONVERT(varchar,CAST(P.EstimatedProposalValue AS MONEY),1), '.00','') AS [Estimated Value],
	P.EstimatedProposalValue AS [Estimated Value],
	P.RevisedSubmittalDate AS [Revised Submittal Date],
	P.DateAssigned AS [Date Assigned],
	CASE 
		WHEN P.ProposalStatusID = 2/*Completed*/
		THEN 
			--ChecklistCompleteDate.MaxUpdateDate_ChecklistComplete
			CONVERT(varchar, CONVERT(datetime2(7), ChecklistCompleteDate.MaxUpdateDate_ChecklistComplete), 100) 
		ELSE NULL--''
	END AS [Checklist Complete Date],

	PC.ProposalSubmittalDate AS [Actual Submittal Date],	

	PC.ISGSTotalPrice	 AS [Total Price],
/*
if IWTA is Yes, then use Submitted value (which is always IS&GS Total Price).  If Iwta is no, then blank 
*/
	CASE P.IsIWTA
		WHEN 1 THEN PC.ISGSTotalPrice
		WHEN 0 THEN NULL--''
		ELSE NULL--''
	END AS [IWTA Submitted Value],
	
	PC.AbsoluteValue,
	PS.ProposalStatusID AS [ProposalStatusID],
	PS.ProposalStatus AS [Proposal Status],
	
	CusType.CustomerTypeID AS CustomerTypeID,
	CusType.CustomerType  AS CustomerType

  FROM [dbo].[Proposal] P
	INNER JOIN [dbo].[ProgramAreaLU] PA ON P.ProgramAreaID = PA.ProgramAreaID
	INNER JOIN [dbo].[LineOfBusinessLU] LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
	INNER JOIN dbo.ProposalStatusLU PS ON P.ProposalStatusID = PS.ProposalStatusID
	INNER JOIN dbo.CustomerTypeLU CusType ON P.CustomerTypeID = CusType.CustomerTypeID
	LEFT OUTER JOIN dbo.ProposalChecklist PC ON P.ProposalID = PC.ProposalID
LEFT OUTER JOIN 
	(
		SELECT /*Should be Submit Date not Update Date*/
			MAX ([SubmitDate]) AS MaxUpdateDate_ChecklistComplete
			,[ProposalID]
		FROM [dbo].[ProposalChecklistComplete]
		GROUP BY ProposalID
	) [ChecklistCompleteDate] ON P.ProposalID = [ChecklistCompleteDate].ProposalID
	
	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName AS [Pricer Name],
				U.UserID AS [PricerUserID],
				U.NTID AS [PricerNTID]
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE RoleID = 3 /*Pricer*/
		) Pricer ON P.ProposalID = Pricer.ProposalID


		-- Independent Reviewer
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 11
		) IndependentReviewer ON P.ProposalID = IndependentReviewer.ProposalID

		--Tech Lead
	 	LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 16
		) TechLead ON P.ProposalID = TechLead.ProposalID

		--Proposal Mgr
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 17
		) ProposalMgr ON P.ProposalID = ProposalMgr.ProposalID

		--Cover Sheet Approver
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 18
		) CoverSheetApprover ON P.ProposalID = CoverSheetApprover.ProposalID

		--Pricing Verification
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 19
		) PricingVerification ON P.ProposalID = PricingVerification.ProposalID

		--Lead Estimator
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 3
		) LeadEstimator ON P.ProposalID = LeadEstimator.ProposalID

GO

/*
    File: \1 Views\vwProposalLogReport.view.sql
*/
PRINT '### Starting file: \1 Views\vwProposalLogReport.view.sql';
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vwProposalLogReport]') AND type in (N'V'))
	DROP VIEW [dbo].[vwProposalLogReport]
GO

CREATE VIEW [dbo].[vwProposalLogReport] AS
/******************************************************************************
**		 
**		Name: [vwProposalLogReport]
**		Desc: View that drives the Proposal Log Report.  This report is also used in Tableau
**			
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		6/30/2020	Dusan				BOEJ-4639 Add Revision Type
**										BOEJ-4590 Add Material POC and Subcontracts POC
**										BOEJ-4631 Add Reason Cert Not Required
**		7/30/2020	Dusan				BOEJ-4639 Add Latest Revision
**      10/13/2021  Koovackal           IES-181 DB Work -Added Profit, Com, ProfitFeeWithCon
**		1/16/2022   Dusan				IES-174 Add Contract Data to the report
**		3/7/2022	Dusan				IES-847 Modify Contracts Data data
**      3/8/2022	Koovackal			IES-849 Changes to "Revise" button. 
**										Removed contract offer code.
**		7/25/2022	Dusan / Thomas		IES-1499, IES-1510, IES-1511: Added new fields into the report: Cage Codes, Type of Contract Action, Cost thru COM
**		2/22/2023	ranzalon			Add CustomerDueDate
**		7/10/24		Dusan				PROPH-1563: Add AdditionalClassification field
**		7/14/24		Dusan				PROPH-1559: Added reason for CCOPD = No
**		7/18/24		Dusan				PROPH-1560: Added Include International Costs
**		8/19/24		Dusan				PROPH-2080: Added IsSupportDefinitizingUCA field
**      11/20/24	twilson3			proph-2357 Add Insurance fields
**		12/3/24		twilson3			proph-2544 Add SetupComments, Backup Estimator Name
**		02/27/25	Carlos				PROPH-2642 Add checklist question 11 to log report
**		05/13/25	ranzalon			PROPH-3038 Added Bid and Mission Segment EPP Dates
*******************************************************************************/
SELECT	
	P.ProposalID AS ProposalID,	
	p.AdditionalClassification,
	cNo.Text AS ReasonCcopdNo,
	P.ReasonCcopdNoOther,
	P.SetupComments,
	CAST (P.DateCreated AS DATE) AS DateCreated,
	YEAR(P.DateCreated) AS [Year],	
	PA.ProgramAreaID as ProgramAreaID,
	LOB.LineOfBusinessID as LineOfBusinessID,
	LOB.LineOfBusinessName AS [Line Of Business],
	PA.ProgramAreaName AS [Program Area Name],	
	P.ProposalTrackingID AS [Tracking #],
	P.ForecastedTrackingId AS [ForecastedTracking#],
	P.ProposalTitle AS [Proposal Title],	
	CAST(P.DateCreated AS DATE) AS [Proposal Start Date],
	CAST (PC.UpdateDate AS Date) AS [Proposal End Date],	
	LeadEstimator.UserID AS [LeadEstimatorUserID],	
	[CostVolumeLead].[Cost Volume Lead] AS [Cost Volume Lead],
	[AdditionalPricingResource1].[Additional Pricing Resource 1] AS [Additional Pricing Resource 1],
	[AdditionalPricingResource2].[Additional Pricing Resource 2] AS [Additional Pricing Resource 2],	
	P.EstimatedProposalValue AS [Estimated Value],
	CAST(P.AnticipatedDeliveryDate AS DATE) AS [Estimated Ship Date],
	CAST(P.RevisedSubmittalDate AS DATE) AS [Revised Submittal Date],
	P.ProgramName AS [Program Name],
	PC.ISGSTotalPrice AS [Submitted Value],
	CONVERT(varchar, CONVERT(datetime2(7), P.LeadEstimatorSignedDT), 100) AS [Lead Estimator Approval Date],
	CONVERT(varchar, CONVERT(datetime2(7), [Workflow Completed Date].MaxSubmitDate), 100) AS [Workflow Completed Date],
	IndependentReviewer.DisplayName AS [IndependentReviewerName],
	PricingVerification.DisplayName AS [PricingVerificationName],
	CoverSheetApprover.DisplayName AS [CoverSheetApproverName],
	LOBMgr.DisplayName AS [LOBMgrName],
	ProposalMgr.DisplayName AS [ProposalMgrName],
	TechLead.DisplayName AS [TechLeadName],
	LeadEstimator.DisplayName AS [LeadEstimatorName],
    PC.[LMLaborHours] AS [LMLaborHours],
    PC.[LMLaborCost] AS [LMLaborCost],
    PC.[SubcontractorCost] AS [SubcontractorCost],
    PC.[MaterialCost] AS [MaterialCost],
    PC.[IWTACost] AS [IWTACost],
    PC.[TravelCost] AS [TravelCost],
    PC.[OtherDirectCost] AS [OtherDirectCost],
	PC.[AbsoluteValue] AS [AbsoluteValue],
	PC.[Profit] AS [Profit/Fee],
	PC.[Com] AS [Com],
	PC.[ProfitFeeWithCom] AS [Profit/Fee + COM],
	PC.[ISGSTotalPrice] AS [Total Price],
	PC.[ROSPercentage] AS [ROS %],
	PC.IncludeInternationalCosts,
	CAST(PC.ProposalSubmittalDate AS DATE) AS [Actual Submittal Date],
	PT.ProposalType AS [Proposal Type],
	dbo.udfCreateCommaSeparatedList (P.ProposalID, 1) AS [Contract Type],
	I.ISGSRole AS [Prime or Sub],
	P.RFPNumber AS [RFP/Contract Modification Number],
	dbo.udfCreateCommaSeparatedList (P.ProposalID, 2) AS [Elements of Cost],
	[IS&GS Contracts POC].[IS&GS Contracts POC] AS [Contracts POC],
	P.Customer AS [Customer],
	CusType.CustomerType AS [Customer Type],
	CreatedBy.NTID AS [Created By],
	CAST (P.DateCreated AS DATE) AS [Created Date],
	PS.ProposalStatusID AS [ProposalStatusID],
	CASE
		WHEN PS.ProposalStatusID = 10 THEN 'Not Awarded'
		ELSE PS.ProposalStatus
	END AS [Proposal Status],
	P.OTISOpportunityID AS [OTIS #],
	CASE
		WHEN P.PricingToolID = 3 AND P.PricingToolName IS NOT NULL THEN P.PricingToolName
		ELSE T.PricingTool
	END AS [Pricing Tool],
	CASE
		WHEN P.BOEToolID = 6 AND P.BOEToolName IS NOT NULL THEN P.BOEToolName
		ELSE B.BOETool
	END AS [BOE Tool],
	CASE
		WHEN P.CostVolumeToolID = 3 AND P.CostVolumeToolName IS NOT NULL THEN P.CostVolumeToolName
		ELSE CV.CostVolumeTool
	END AS [Cost Volume Tool],
	CAST(P.RFPIssuedDate AS DATE) AS [RFP Issued Date],
	CAST(P.RFPReceivedDate AS DATE) AS [RFP Received Date],
	P.[Comments],
	CG.ContractTypeGroupID AS [ContractTypeGroupID],
	CG.ContractTypeGroup AS [ContractTypeGroup],
	CASE P.IsScheduleProposal
		WHEN 1 THEN 'Yes'
		WHEN 0 THEN 'No'
		ELSE 'N/A'
	END AS [Schedule Proposal],
	LeadEstimator.[NTID] AS [PricerNTID],
	[CostVolumeLead].[Cost Volume Lead NTID] AS [Cost Volume Lead NTID],	
	[AdditionalPricingResource1].[Additional Pricing Resource 1 NTID] AS [Additional Pricing Resource 1 NTID],	
	[AdditionalPricingResource2].[Additional Pricing Resource 2 NTID] AS [Additional Pricing Resource 2 NTID],	
	[IS&GS Contracts POC].[IS&GS Contracts POC NTID] AS [Contracts POC NTID],
	CASE
		WHEN P.ProposalLocationID = 9 AND P.ProposalLocationName IS NOT NULL THEN P.ProposalLocationName
		ELSE PLoc.ProposalLocation
	END AS [Proposal Location],
	CASE 
		WHEN P.ProposalTrackingID IS NULL OR P.ProposalTrackingID = '' THEN P.ForecastedTrackingId
		ELSE
			CASE LEN(LEFT (IsNULL(P.ProposalTrackingID, P.ForecastedTrackingId), CHARINDEX ('-',IsNULL(P.ProposalTrackingID, P.ForecastedTrackingId)) -1))
				WHEN 1 THEN P.ForecastedTrackingId
				WHEN 4 THEN LTRIM(RTRIM(LEFT (IsNULL(P.ProposalTrackingID, P.ForecastedTrackingId), 10))) 
				WHEN 2 THEN LTRIM(RTRIM(LEFT (IsNULL(P.ProposalTrackingID, P.ForecastedTrackingId), 8)))
			END
	END	AS MainProposalTrackingID,
	P.CCPDRequired,
	P.CostVolumeClassified,
	ppsLU.ProgramProposalStatus,
	pcLU.ProposalClass AS [Proposal Class],
	CAST(P.[AgreementDate] AS DATE) AS [Agreement Date],
	CAST(P.[CertificationDate] AS DATE) AS [Certification Date],
	CAST(P.[CertificationTimelineCompleted] AS DATE) AS [Certification Timeline Completed Date],
	CAST(P.[CertificationLastEmailed] AS DATE) AS [Certification Last Emailed Date],
	CASE P.[CutOffDateUtilization] 
		WHEN 0 THEN 'Yes'
		WHEN 1 THEN 'No, LM did not request'
		WHEN 2 THEN 'No, LM request denied'
		ELSE ''
	END AS [CutOff Date Utilization],
	CASE 
		WHEN T.[PricingTool] = 'ProPricer' AND B.[BOETool] = 'genBOE' THEN ''
		ELSE P.LOBEstimatingLeadSignComment
	END AS LOBMgrComment,
	CASE p.ReasonCertificationNotRequired
		WHEN 3 THEN p.OtherReasonComment -- Other
		ELSE rCNR.Text
	END AS ReasonCertificationNotRequired,
	CASE 
		WHEN p.RevisionOfId IS NULL THEN 'Original'
		ELSE 'Proposal Revision'
	END AS RevisionType,
	MaterialPOC.DisplayName AS MaterialPOC,
	SubcontractsPOC.DisplayName AS SubcontractsPOC,
	BackupEstimatorPOC.DisplayName AS BackupEstimatorPOC,
	CASE
		WHEN p.ProposalStatusID = 8 THEN 'No'
		ELSE 'Yes'
	END AS IsLatestVersion,
	-- Proposal Contract Data
	previousRomProposal.ProposalTrackingID AS ContractsPreviouslySubmittedRomTrackingNumber,
	previousRomChecklist.ProposalSubmittalDate AS ContractsPreviouslySubmittedRomDate,
	previousRomChecklist.ISGSTotalPrice AS ContractsPreviouslySubmittedRomValue,
	pCD.CustomerSubmittalDate AS ContractsCustomerSubmittalDate,
	pCD.ContractsCorrespondLogNumber AS ContractsCorrespondLogNumber,
	pCD.FinalNegotiatedValue AS ContractsFinalNegotiatedValue,
	pCD.FinalNegotiatedDate AS ContractsFinalNegotiatedDate,
	eppLU.Text AS ContractsEppDelegationAuthority,
	pCD.ProgramEppDate AS ContractsProgramEppDate,
	pCD.LobEppDate AS ContractsLobEppDate,
	pCD.PreSpaceEppDate AS ContractsPreSpaceEppDate,
	pCD.SpaceEppDate AS ContractsSpaceEppDate,
	pCD.PreCorporateEppDate AS ContractsPreCorporateEppDate,
	pCD.CorporateEppDate AS ContractsCorporateEppDate,
	pCD.EppRosDelegationNotes AS ContractsEppRosDelegationNotes,
	pCD.CustomerDueDate AS CustomerDueDate,
	CASE
		WHEN pCD.LmWon = 1 THEN 'Yes'
		WHEN pCD.LmWon = 0 THEN 'No'
		ELSE NULL
	END AS ContractsLmWon,
	pCD.ModCompletedDate AS ContractsModCompletedDate,
	pCD.BidEppDate AS ContractsBidEppDate,
	pCD.MissionSegmentEppDate AS ContractsMissionSegmentEppDate,
	-- end of Proposal Contract Data
	pCD.CageCode,
	CASE
		WHEN p.ContractActionTypeOtherText IS NULL THEN aT.ContractActionType
		ELSE aT.ContractActionType + ': ' + p.ContractActionTypeOtherText
	END AS ContractActionType,
	PC.CostThroughCom,
	ppr.Response AS NlfResponse,
	ppr11.Response AS SupplierMilestoneDatesResponse,
	P.IsSupportDefinitizingUCA,
	CASE
		WHEN pCD.IsInsuranceDirect = 1 THEN 'Yes'
		WHEN pCD.IsInsuranceDirect = 0 THEN 'N/A'
		WHEN pCD.IsInsuranceDirect = 2 THEN 'No'
		ELSE 'N/A'
	END AS IsInsuranceDirect,
	CASE
		WHEN pCD.IsInsuranceDirect = 1 THEN IT.[Text]
		WHEN pCD.IsInsuranceDirect = 0 THEN 'N/A'
		WHEN pCD.IsInsuranceDirect = 2 THEN 'N/A'
		ELSE 'N/A'
	END AS InsuranceType,
	pCD.ProposedInsurance,
	pCD.NegotiatedInsurance
  FROM [dbo].[Proposal] P
    INNER JOIN [dbo].[ProgramAreaLU] PA ON P.ProgramAreaID = PA.ProgramAreaID
	INNER JOIN [dbo].[LineOfBusinessLU] LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
	INNER JOIN dbo.ProposalTypeLU PT ON P.ProposalTypeID = PT.ProposalTypeID
	INNER JOIN dbo.PricingToolLU T ON P.PricingToolID = T.PricingToolID
	INNER JOIN dbo.BOEToolLU B ON P.BOEToolID = B.BOEToolID
	INNER JOIN dbo.CostVolumeToolLU CV on P.CostVolumeToolID = CV.CostVolumeToolID
	INNER JOIN dbo.ProposalLocationLU PLoc ON P.ProposalLocationID = PLoc.ProposalLocationID
	INNER JOIN dbo.ProposalStatusLU PS ON P.ProposalStatusID = PS.ProposalStatusID
	INNER JOIN dbo.CustomerTypeLU CusType ON P.CustomerTypeID = CusType.CustomerTypeID
	INNER JOIN dbo.ISGSRoleLU I ON P.ISGSRoleID = I.ISGSRoleID
	INNER JOIN dbo.ProposalClassLU pcLU ON P.ProposalClassID = pcLU.ProposalClassId
	LEFT OUTER JOIN dbo.ProposalChecklist PC ON P.ProposalID = PC.ProposalID
	LEFT OUTER JOIN dbo.ContractTypeGroupLU CG ON P.ContractTypeGroupID = CG.ContractTypeGroupID
	LEFT OUTER JOIN dbo.genTRACUser CreatedBy ON P.CreatedByUserID = CreatedBy.UserID
	LEFT OUTER JOIN
	(
		SELECT ProposalID, MAX(SubmitDate) AS MaxSubmitDate
			FROM dbo.ProposalChecklistComplete
			GROUP BY ProposalID
	) [Workflow Completed Date] ON P.ProposalID = [Workflow Completed Date].ProposalID
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS [Additional Pricing Resource 2],
			U.UserID AS [Additional Pricing Resource 2 UserID],
			U.NTID AS [Additional Pricing Resource 2 NTID]
		FROM dbo.ProposalUserRole PUR INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 5
		) [AdditionalPricingResource2] ON P.ProposalID = [AdditionalPricingResource2].ProposalID
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS [Additional Pricing Resource 1],
			U.UserID AS [Additional Pricing Resource 1 UserID],
			U.NTID AS [Additional Pricing Resource 1 NTID]
		FROM dbo.ProposalUserRole PUR INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 4	/*Additional Pricing Resource 1*/
	) [AdditionalPricingResource1] ON P.ProposalID = [AdditionalPricingResource1].ProposalID
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS [Cost Volume Lead],
			U.UserID AS [Cost Volume Lead UserID],
			U.NTID AS [Cost Volume Lead NTID]
		FROM dbo.ProposalUserRole PUR INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 2
	) [CostVolumeLead] ON P.ProposalID = [CostVolumeLead].ProposalID
	LEFT OUTER JOIN 
	(
		SELECT 
			PUR.ProposalID,
			U.DisplayName AS [IS&GS Contracts POC],
			U.UserID AS [IS&GS Contracts POC UserID],
			U.NTID AS [IS&GS Contracts POC NTID]
		FROM dbo.ProposalUserRole PUR INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
		WHERE RoleID = 8
	) [IS&GS Contracts POC] ON P.ProposalID = [IS&GS Contracts POC].ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
		FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
		WHERE PUR.RoleID = 11
	) IndependentReviewer ON P.ProposalID = IndependentReviewer.ProposalID
 	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 16
	) TechLead ON P.ProposalID = TechLead.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 17
	) ProposalMgr ON P.ProposalID = ProposalMgr.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 18
	) CoverSheetApprover ON P.ProposalID = CoverSheetApprover.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 19
	) PricingVerification ON P.ProposalID = PricingVerification.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 20
	) LOBMgr ON P.ProposalID = LOBMgr.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 3
	) LeadEstimator ON P.ProposalID = LeadEstimator.ProposalID
	LEFT JOIN dbo.ProgramProposalStatusLU ppsLU ON ppsLU.ProgramProposalStatusID = p.ProgramProposalStatusID
	LEFT JOIN dbo.ReasonCertificationNotRequiredLU rCNR ON rCNR.Id = p.ReasonCertificationNotRequired
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID,	U.DisplayName
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 6
	) MaterialPOC ON P.ProposalID = MaterialPOC.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID, U.DisplayName
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 7 
	) SubcontractsPOC ON P.ProposalID = SubcontractsPOC.ProposalID
	LEFT OUTER JOIN
	(
		SELECT PUR.ProposalID, U.DisplayName
			FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE PUR.RoleID = 12 
	) BackupEstimatorPOC ON P.ProposalID = BackupEstimatorPOC.ProposalID

	-- Proposal Contract Data
	LEFT OUTER JOIN ProposalContractsData pCD ON pCD.ProposalID = p.ProposalID
	LEFT OUTER JOIN dbo.InsuranceTypeLU IT ON pCD.InsuranceType = IT.ID
	LEFT OUTER JOIN Proposal previousRomProposal ON pCD.PreviouslySubmittedROM = previousRomProposal.ProposalID
	LEFT OUTER JOIN ProposalChecklist previousRomChecklist ON pCD.PreviouslySubmittedROM = previousRomChecklist.ProposalID
	LEFT OUTER JOIN EppDelegationAuthorityLU eppLU ON eppLU.Id = pCD.EppDelegationAuthority
	-- end of Proposal Contract Data

	LEFT OUTER JOIN ContractActionTypeLU aT ON p.ContractActionType = aT.ID
	LEFT OUTER JOIN
			(SELECT xref.ProposalID, ppr.SortOrder, ppr.ChecklistText, r.Response
				FROM ProposalPPRChecklistXREF xref 
					INNER JOIN ResponseLU r ON r.ResponseID = xref.ResponseID
					INNER JOIN PPRChecklistContent ppr ON (xref.PPRChecklistContentID = ppr.PPRChecklistContentID AND ppr.ChecklistText LIKE '%NLF Forms%')) AS ppr
			ON ppr.ProposalId = p.ProposalId 
	LEFT OUTER JOIN
			(SELECT xref.ProposalID, ppr.SortOrder, ppr.ChecklistText, r.Response
				FROM ProposalPPRChecklistXREF xref 
					INNER JOIN ResponseLU r ON r.ResponseID = xref.ResponseID
					INNER JOIN PPRChecklistContent ppr ON (xref.PPRChecklistContentID = ppr.PPRChecklistContentID AND ppr.ChecklistText LIKE '%Does the proposal include subcontractors of any dollar value or material supplier > CCoPD threshold with planned dates that go beyond proposal submittal%')) AS ppr11
			ON ppr11.ProposalId = p.ProposalId 
	LEFT OUTER JOIN [CcopdReasonsNo] cNo ON P.ReasonCcopdNo = cNO.Id
GO


/*
    File: \Functions\SplitString.function.sql
*/
PRINT '### Starting file: \Functions\SplitString.function.sql';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SplitString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	DROP FUNCTION [dbo].[SplitString]

GO

CREATE FUNCTION [dbo].[SplitString] (
	@List varchar(8000),
	@Delimiter char(1) = ',',
	@EmptyListItem varchar(80) = null -- default is empty table if no matches
) RETURNS @Results TABLE (
	ItemId int IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	Item varchar(8000) NULL
)
AS
/******************************************************************************
**
**	Name: SplitString
**	Desc: Takes in a delimited string and the delimiter,
**			and returns a table with each value in a separate row.
**
**	Example calls:
**		SplitString ('1, 3, 4, 6', DEFAULT, DEFAULT)
**		SELECT Item FROM SplitString('1, 3, 4, 6', ',', DEFAULT)
**		SELECT Item FROM SplitString('1, 3, 4, 6', ',', 0)
**
**	Example of use with an inner join:
**
**		SELECT
**			p.ProgramID,
**			p.ProgramName,
**			idlist.Item
**		FROM
**			Program p
**			INNER JOIN SplitString(@MyIdList, ',', 0) idlist
**				ON CASE WHEN ISNULL(@MyIdList, '') = '' THEN 0 ELSE p.ProgramID END = CONVERT(INT, idlist.Item)
**
**	Example of use with an outer join:
**
**		SELECT
**			p.ProgramID,
**			p.ProgramName,
**			idlist.Item
**		FROM
**			Program p
**			LEFT JOIN SplitString(@MyIdList, ',', DEFAULT) idlist ON p.ProgramID = CONVERT(INT, idlist.Item)
**		WHERE
**			idlist.Item IS NOT NULL
**
**
**	Called by: Numerous stored procedures
**
*******************************************************************************
**	Change History
*******************************************************************************
**	Date:		Author:		Description:
**	--------	--------	---------------------------------------------------
**	2009-07-06	jdalonzo	Initial creation.
**	2011-03-24	sjrosent	Added @EmptyListItem input parameter which allows
**							caller to choose either an empty table result (by
**							default) OR a table with a single (designated) row
**							item.  So the caller can use an inner OR outer
**							join on the function results, if desired, without
**							the need for a lot of extra code.  (See examples)
**
*******************************************************************************/
BEGIN

DECLARE @item varchar(4000)
DECLARE @iPos int

SET @Delimiter = ISNULL(@Delimiter, ',')
SET @List = RTRIM(LTRIM(@List))

IF @List = ''
	SET @List = null  -- treat empty list as null
ELSE IF RIGHT(@List, 1) <> @Delimiter
	SELECT @List = @List + @Delimiter  -- append trailing delimiter (for algorithm) if none

-- if empty/null list and caller has designated an "empty row" value, then apply it
IF @List IS NULL AND @EmptyListItem IS NOT NULL
	INSERT @Results VALUES (@EmptyListItem)

-- get position of first item
SELECT @iPos = CHARINDEX(@Delimiter, @List, 1)

WHILE @iPos > 0
BEGIN
	-- find next item
	SELECT @item = LTRIM(RTRIM(SUBSTRING(@List, 1, @iPos -1)))
	IF @@ERROR <> 0 BREAK

	-- "remove" it from list
	SELECT @List = SUBSTRING(@List, @iPos + 1, Len(@List) - @iPos + 1)
	IF @@ERROR <> 0 BREAK

	-- "save" it
	INSERT @Results VALUES (@item)
	IF @@ERROR <> 0 BREAK

	-- determine position of next item
	SELECT @iPos = CHARINDEX(@Delimiter, @List, 1)
	IF @@ERROR <> 0 BREAK
END

RETURN

END

GO

/*
    File: \Functions\udfCreateCommaSeparatedList.function.sql
*/
PRINT '### Starting file: \Functions\udfCreateCommaSeparatedList.function.sql';
DROP FUNCTION [dbo].[udfCreateCommaSeparatedList];
GO

CREATE FUNCTION [dbo].[udfCreateCommaSeparatedList]
(
@ProposalID int,
@ListTypeID int
)
RETURNS varchar(1000)
AS
BEGIN

DECLARE @listStr VARCHAR(1000)


IF @ListTypeID = 1 /*Contract Type*/
BEGIN
	SELECT @listStr = COALESCE(@listStr+',' ,'') + C.ContractType
	FROM dbo.Proposal P
		INNER JOIN dbo.ProposalContractTypeXREF X ON P.ProposalID = X.ProposalID
		INNER JOIN dbo.ContractTypeLU C ON X.ContractTypeID = C.ContractTypeID
	WHERE P.ProposalID = @ProposalID		
END



IF @ListTypeID = 2 /*Cost Element*/
BEGIN
	SELECT @listStr = COALESCE(@listStr+',' ,'') + C.CostElement
	FROM dbo.Proposal P
		INNER JOIN dbo.ProposalCostElementXREF X ON P.ProposalID = X.ProposalID
		INNER JOIN dbo.CostElementLU C ON X.CostElementID = C.CostElementID
	WHERE P.ProposalID = @ProposalID		
END


RETURN @listStr

END

GO

/*
    File: \Stored Procedures\archiveProposal.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\archiveProposal.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[archiveProposal]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[archiveProposal];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[archiveProposal]
(	
	@CreateStartDate [date],
	@CreateEndDate [date],
	@LineOfBusinessID [varchar] (100),
	@ProgramAreaID [varchar] (100)
)
AS
/******************************************************************************
**          
**          Name: [archiveProposal]
**          Desc: Archives all Proposals based on parameters
**                
**					Sample SP Call:
**					@CreateStartDate [date] = '4/1/13',
**					@CreateEndDate [date]='5/1/13',
**					@LineOfBusinessID [varchar] (100) = '1',
**					@ProgramAreaID [varchar] (100)='1'
**
**          Auth: Don Canuso
**          Date: 7/17/2013
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                 Description:
**          --------    --------                ------------------------------
**			6/06/18		brunworg				BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

/*
Process Product Lines
*/
DECLARE @LineOfBusinessLU TABLE (LineOfBusinessID INT)

IF @LineOfBusinessID IS NULL OR @LineOfBusinessID = 'ALL'
	BEGIN
		INSERT INTO @LineOfBusinessLU
		SELECT LineOfBusinessID FROM dbo.LineOfBusinessLU
	END
ELSE
	BEGIN
		IF RIGHT(@LineOfBusinessID, 1) <> ','
			SET @LineOfBusinessID = @LineOfBusinessID + ','

	WHILE (SELECT CHARINDEX (',', @LineOfBusinessID) ) > 1
		BEGIN
		      
			  INSERT INTO @LineOfBusinessLU
			  SELECT LEFT (@LineOfBusinessID, CHARINDEX (',', @LineOfBusinessID) -1)
			  SET @LineOfBusinessID = RIGHT (@LineOfBusinessID, LEN (@LineOfBusinessID) - CHARINDEX (',', @LineOfBusinessID) )
		      
		END
	END


/*
Process Line Of Business
*/
DECLARE @ProgramAreaLU TABLE (ProgramAreaID INT)

IF @ProgramAreaID IS NULL OR @ProgramAreaID = 'ALL'
	BEGIN
		INSERT INTO @ProgramAreaLU
		SELECT ProgramAreaID FROM dbo.ProgramAreaLU
	END
ELSE
	BEGIN
		IF RIGHT(@ProgramAreaID, 1) <> ','
			SET @ProgramAreaID = @ProgramAreaID + ','

	WHILE (SELECT CHARINDEX (',', @ProgramAreaID) ) > 1
		BEGIN
		      
			  INSERT INTO @ProgramAreaLU
			  SELECT LEFT (@ProgramAreaID, CHARINDEX (',', @ProgramAreaID) -1)
			  SET @ProgramAreaID = RIGHT (@ProgramAreaID, LEN (@ProgramAreaID) - CHARINDEX (',', @ProgramAreaID) )
		      
		END
	END




DECLARE @Archive int
SELECT @Archive = ProposalStatusID FROM dbo.ProposalStatusLU WHERE ProposalStatus = 'Archived'

DECLARE @ArchiveCount TABLE (ProposalID int)

UPDATE dbo.Proposal
SET	ProposalStatusID = @Archive
OUTPUT inserted.ProposalID INTO @ArchiveCount
FROM dbo.Proposal P
	INNER JOIN @ProgramAreaLU PA ON P.ProgramAreaID = PA.ProgramAreaID
	INNER JOIN @LineOfBusinessLU LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
WHERE
	(
		@CreateStartDate IS NULL OR
		P.DateCreated > = @CreateStartDate
	) AND
	(
		@CreateEndDate IS NULL OR
		P.DateCreated < = @CreateEndDate
	)  AND
	P.ProposalStatusID IN  (
							1, /*In Progress*/
							2  /*Completed*/
						   )
	
	
SELECT COUNT (ProposalID) AS ArchiveCount FROM @ArchiveCount

GO

/*
    File: \Stored Procedures\archiveProposalCount.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\archiveProposalCount.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[archiveProposalCount]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[archiveProposalCount];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[archiveProposalCount]
(	
	@CreateStartDate [date],
	@CreateEndDate [date],
	@LineOfBusinessID [varchar] (100),
	@ProgramAreaID [varchar] (100)
)
AS
/******************************************************************************
**          
**          Name: [archiveProposalCount]
**          Desc: Provides a count for Archives all Proposals based on parameters
**                
**					Sample SP Call:
**					@CreateStartDate [date] = '4/1/13',
**					@CreateEndDate [date]='5/1/13',
**					@LineOfBusinessID [varchar] (100) = '1',
**					@ProgramAreaID [varchar] (100)='1'
**
**          Auth: Don Canuso
**          Date: 7/17/2013
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                 Description:
**          --------    --------                ------------------------------
**			6/06/18		brunworg				BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
**			6/20/18		ranzalon				BOEJ-4114 - Updated for Submitted Status
**			3/3/22		koovackal				IES-849 Changes to "Revise" button
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

/*
Process Product Lines
*/
DECLARE @LineOfBusinessLU TABLE (LineOfBusinessID INT)

IF @LineOfBusinessID IS NULL OR @LineOfBusinessID = 'ALL'
	BEGIN
		INSERT INTO @LineOfBusinessLU
		SELECT LineOfBusinessID FROM dbo.LineOfBusinessLU
	END
ELSE
	BEGIN
		IF RIGHT(@LineOfBusinessID, 1) <> ','
			SET @LineOfBusinessID = @LineOfBusinessID + ','

	WHILE (SELECT CHARINDEX (',', @LineOfBusinessID) ) > 1
		BEGIN
		      
			  INSERT INTO @LineOfBusinessLU
			  SELECT LEFT (@LineOfBusinessID, CHARINDEX (',', @LineOfBusinessID) -1)
			  SET @LineOfBusinessID = RIGHT (@LineOfBusinessID, LEN (@LineOfBusinessID) - CHARINDEX (',', @LineOfBusinessID) )
		      
		END
	END


/*
Process Line Of Business
*/
DECLARE @ProgramAreaLU TABLE (ProgramAreaID INT)

IF @ProgramAreaID IS NULL OR @ProgramAreaID = 'ALL'
	BEGIN
		INSERT INTO @ProgramAreaLU
		SELECT ProgramAreaID FROM dbo.ProgramAreaLU
	END
ELSE
	BEGIN
		IF RIGHT(@ProgramAreaID, 1) <> ','
			SET @ProgramAreaID = @ProgramAreaID + ','

	WHILE (SELECT CHARINDEX (',', @ProgramAreaID) ) > 1
		BEGIN
		      
			  INSERT INTO @ProgramAreaLU
			  SELECT LEFT (@ProgramAreaID, CHARINDEX (',', @ProgramAreaID) -1)
			  SET @ProgramAreaID = RIGHT (@ProgramAreaID, LEN (@ProgramAreaID) - CHARINDEX (',', @ProgramAreaID) )
		      
		END
	END




DECLARE @Archive int
SELECT @Archive = ProposalStatusID FROM dbo.ProposalStatusLU WHERE ProposalStatus = 'Archived'



SELECT COUNT (*) AS ArchiveCount
FROM dbo.Proposal P
	INNER JOIN @ProgramAreaLU PA ON P.ProgramAreaID = PA.ProgramAreaID
	INNER JOIN @LineOfBusinessLU LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
WHERE
	(
		@CreateStartDate IS NULL OR
		P.DateCreated > = @CreateStartDate
	) AND
	(
		@CreateEndDate IS NULL OR
		P.DateCreated < = @CreateEndDate
	)   AND
	P.ProposalStatusID IN  (
							1, /*In Progress*/
							2, /*Completed*/
							6,  /*Pending Certification*/
							9  /*Pending Contractual Award*/
						   )

GO

/*
    File: \Stored Procedures\CopyCannedResponsesPAR.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\CopyCannedResponsesPAR.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CopyCannedResponsesPAR]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CopyCannedResponsesPAR];
GO

CREATE PROCEDURE dbo.CopyCannedResponsesPAR
/******************************************************************************
**		 
**		Name: CopyCannedResponsesPAR
**		Desc: Copies Canned Responses into a new Checklist version. This is executed manually.
**
**		Auth: Dusan
**		Date: 4/13/21
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
(@newChecklistId AS INT) AS
BEGIN
	IF NOT EXISTS (SELECT 1 FROM dbo.CannedResponsesPAR cR INNER JOIN PARChecklistContent pCC ON pCC.PARChecklistContentID = cR.QuestionId WHERE pCC.ProposalAdequacyReviewID = @newChecklistId)
	BEGIN
		DECLARE @oldChecklistId INT = @newChecklistId - 1;

		INSERT INTO dbo.CannedResponsesPAR
		SELECT cR.Text, new.PARChecklistContentID
			FROM dbo.CannedResponsesPAR cR 
				INNER JOIN PARChecklistContent old ON old.PARChecklistContentID = cR.QuestionId AND old.ProposalAdequacyReviewID = @oldChecklistId
				INNER JOIN PARChecklistContent new ON old.QuestionNumber = new.QuestionNumber AND new.ProposalAdequacyReviewID = @newChecklistId;

	END
END
GO

/*
    File: \Stored Procedures\CopyPARChecklist.sql
*/
PRINT '### Starting file: \Stored Procedures\CopyPARChecklist.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CopyPARChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CopyPARChecklist];
GO

CREATE PROCEDURE dbo.CopyPARChecklist
(@newChecklistId AS INT) AS
/******************************************************************************
**		 
**		Name: CopyPARChecklist
**		Desc: Copies PAR Checklist into a new Checklist version. This is executed manually.
**
**		Auth: RJ
**		Date: 4/13/23
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ProposalAdequacyReview WHERE ProposalAdequacyReviewID = @newChecklistId)
	BEGIN
		UPDATE ProposalAdequacyReview SET IsCurrent = 0;

		SET IDENTITY_INSERT ProposalAdequacyReview ON;
		INSERT INTO ProposalAdequacyReview (ProposalAdequacyReviewID, ChecklistVersion, IsCurrent, ProposalChecklistTypeID)
			VALUES (@newChecklistId, @newChecklistId - 1, 1, 1);
		SET IDENTITY_INSERT ProposalAdequacyReview OFF;

		INSERT INTO PARChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalAdequacyReviewID, QuestionNumber, Reference, SubmissionItem, YesOnly)
			SELECT ChecklistText, TextTypeID, SortOrder, ColumnOrder, @newChecklistId, QuestionNumber, Reference, SubmissionItem, YesOnly
				FROM PARChecklistContent
				WHERE ProposalAdequacyReviewId = @newChecklistId - 1;
	END
END
GO

/*
    File: \Stored Procedures\CopyPPRChecklist.sql
*/
PRINT '### Starting file: \Stored Procedures\CopyPPRChecklist.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CopyPPRChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CopyPPRChecklist];
GO

CREATE PROCEDURE dbo.CopyPPRChecklist
(@newChecklistId AS INT) AS
/******************************************************************************
**		 
**		Name: CopyPPRChecklist
**		Desc: Copies PPR Checklist into a new Checklist version. This is executed manually.
**
**		Auth: RJ
**		Date: 4/13/23
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ProposalPricingReview WHERE ProposalPricingReviewID = @newChecklistId)
	BEGIN
		UPDATE ProposalPricingReview SET IsCurrent = 0;

		SET IDENTITY_INSERT ProposalPricingReview ON;
		INSERT INTO ProposalPricingReview (ProposalPricingReviewID, ChecklistVersion, IsCurrent, ProposalChecklistTypeID)
			VALUES (@newChecklistId, @newChecklistId - 1, 1, 1);
		SET IDENTITY_INSERT ProposalPricingReview OFF;

		INSERT INTO PPRChecklistContent (ChecklistText, TextTypeID, SortOrder, ColumnOrder, ProposalPricingReviewID)
			SELECT ChecklistText, TextTypeID, SortOrder, ColumnOrder, @newChecklistId
				FROM PPRChecklistContent
				WHERE ProposalPricingReviewID = @newChecklistId - 1;
	END
END
GO

/*
    File: \Stored Procedures\CreateDfarsChecklistResponseReport.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\CreateDfarsChecklistResponseReport.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateDfarsChecklistResponseReport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CreateDfarsChecklistResponseReport];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateDfarsChecklistResponseReport]
(
	@LOB varchar(8000) = NULL,
	@PA varchar(8000) = NULL,
	@StartDate [date] = NULL,
	@EndDate [date] = NULL,
	@ExecutionUserID varchar(8000) = NULL
)
AS
/******************************************************************************
**		 
**		Name: [CreateDfarsChecklistResponseReport]
**		Desc: SSRS: DFARS Checklist Response Report
**			
**
**
**		Auth: ranzalon
**		Date: 5/14/2019
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/03/19	ranzalon			BOEJ-4217 - Updates based on feedback
*******************************************************************************/

SET NOCOUNT ON

/*Line of Business*/
DECLARE @tblLineOfBusiness TABLE (LineOfBusinessID int)

IF @LOB IS NULL OR @LOB = 'All'
	BEGIN
		INSERT INTO @tblLineOfBusiness
		SELECT [LineOfBusinessID] FROM dbo.LineOfBusinessLU
	END
ELSE	
	BEGIN
		IF RIGHT(@LOB, 1) <> ','
	      SET @LOB = @LOB + ','
	
		WHILE (SELECT CHARINDEX (',', @LOB) ) > 1
			BEGIN
			      
				  INSERT INTO @tblLineOfBusiness
				  SELECT LEFT(@LOB, CHARINDEX (',', @LOB) -1)
				  SET @LOB = RIGHT(@LOB, LEN (@LOB) - CHARINDEX (',', @LOB) )
			      
			END
	END

/*Program Area*/
DECLARE @tblProgramArea TABLE (ProgramAreaID int)

IF @PA IS NULL OR @PA = 'All'
	BEGIN
		INSERT INTO @tblProgramArea
		SELECT ProgramAreaID FROM dbo.ProgramAreaLU
	END
ELSE	
	BEGIN
		IF RIGHT(@PA, 1) <> ','
	      SET @PA = @PA + ','
	
		WHILE (SELECT CHARINDEX (',', @PA) ) > 1
			BEGIN
			      
				  INSERT INTO @tblProgramArea
				  SELECT LEFT (@PA, CHARINDEX (',', @PA) -1)
				  SET @PA = RIGHT (@PA, LEN (@PA) - CHARINDEX (',', @PA) )
			      
			END
	END

/*Individual Running Report and the groups they belong to as @ExecutionUserID*/
DECLARE @tblExecutionUser TABLE (ExecutionUserID int)
IF @ExecutionUserID IS NULL
	RETURN 
ELSE
BEGIN
	IF (RIGHT(@ExecutionUserID, 1) <> ',')
	BEGIN
		  SET @ExecutionUserID = @ExecutionUserID + ','
	END

	WHILE (SELECT CHARINDEX(',', @ExecutionUserID)) > 1
	BEGIN
		  INSERT INTO @tblExecutionUser
		  SELECT LEFT(@ExecutionUserID, CHARINDEX (',', @ExecutionUserID) -1)
		  SET @ExecutionUserID = RIGHT(@ExecutionUserID, LEN(@ExecutionUserID) - CHARINDEX(',', @ExecutionUserID))	      
	END
END

SELECT	 V.[Tracking #]
		,V.[Proposal Title]
		,V.[Proposal Status]
		,V.[Line of Business]
		,V.[Program Area Name]
		,V.[LeadEstimatorName]
		,V.[Actual Submittal Date]
		,V.[Approval Workflow Completed Date]
		,V.[Certification Completed Date]
		,V.[Question #]
		,V.[Question Text]
		,V.[Comment]
FROM	dbo.[vwDfarsChecklistResponseReport] V
WHERE	[LineOfBusinessID] IN (SELECT LineOfBusinessID FROM @tblLineOfBusiness) 
		AND
		[ProgramAreaID] IN (SELECT ProgramAreaID FROM @tblProgramArea)
		AND
		(
			(
				@StartDate IS NULL AND
				@EndDate IS NULL 
			)
			OR
			(
				@StartDate IS NOT NULL AND
				@EndDate IS NOT NULL AND
				CAST([Actual Submittal Date] AS DATE) >= @StartDate AND
				CAST([Actual Submittal Date] AS DATE) <= @EndDate	
			)
		)
		AND
		/*Now Define Role Access*/
		(
			/*If you are a System Admin, then you see everything*/
			EXISTS (
						SELECT S.SystemUserRoleID
						FROM dbo.SystemUserRole S
							INNER JOIN dbo.RoleLU R ON S.RoleID = R.RoleID
							INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
						WHERE 
							R.[Role] = 'Administrator'
					) OR
			/*System Pricer gets to see anything where they are defined as a Proposal User*/		
					(
						ProposalStatusID <> 4/*Deleted*/ AND
						EXISTS
							(
								SELECT S.SystemUserRoleID
								FROM dbo.SystemUserRole S
									INNER JOIN dbo.RoleLU R ON S.RoleID = R.RoleID
									INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
								WHERE 
									R.[Role] = 'System Pricer'
							) AND
						V.ProposalID IN
							(
								SELECT ProposalID
								FROM dbo.ProposalUserRole PUR
									INNER JOIN @tblExecutionUser U ON PUR.UserID = U.ExecutionUserID
							)								
							
				/*Viewers get to see where they are defined as Product Line Viewers*/							
					) OR

					(
						ProposalStatusID <> 4/*Deleted*/ AND

						EXISTS
							(
								SELECT S.SystemUserRoleID
								FROM dbo.SystemUserRole S
									INNER JOIN dbo.RoleLU R ON S.RoleID = R.RoleID
									INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
								WHERE 
									R.[Role] = 'Viewer'
							) AND
						LineOfBusinessID IN
							(
								SELECT LineOfBusinessID
								FROM dbo.LineOfBusinessRoleXREF X
									INNER JOIN dbo.SystemUserRole S ON X.SystemUserRoleID = S.SystemUserRoleID
									INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
							)		
					)
		)

GO

GRANT EXECUTE ON OBJECT::dbo.CreateDfarsChecklistResponseReport TO generationReporter;
GO

/*
    File: \Stored Procedures\CreatePARChecklistReport.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\CreatePARChecklistReport.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreatePARChecklistReport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CreatePARChecklistReport];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreatePARChecklistReport]
(
	@ProposalID INT,
	@ProposalAdequacyReviewID INT
)	
AS
/******************************************************************************
**		 
**		Name: [CreatePARChecklistReport]
**		Desc: SSRS: Create PAR Checklist Report
**			
**		
**
**		Auth: Don Canuso
**		Date: 12/3/2014
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/22/19		ranzalon			BOEJ-4157 - Update for Canned Responses
*******************************************************************************/
SET NOCOUNT ON

SELECT DISTINCT 
	C.QuestionNumber,
	C.Reference,
	C.SubmissionItem,
	X.PageNumber,
	COALESCE(X.Comment, R.Text) as Comment,
	C.SortOrder,
	C.TextTypeID
FROM 
(
	SELECT 
		PARChecklistContentID,
		ProposalAdequacyReviewID,
		QuestionNumber,
		Reference,
		SubmissionItem,
		SortOrder,
		TextTypeID
		
	FROM [dbo].[PARChecklistContent]
	WHERE 
		ProposalAdequacyReviewID = @ProposalAdequacyReviewID AND
		ColumnOrder = 1 AND
		(
			QuestionNumber IS NOT NULL OR
			Reference IS NOT NULL OR
			SubmissionItem IS NOT NULL
		)
) C
	LEFT OUTER JOIN [dbo].[ProposalPARChecklistXREF] X ON 
			C.PARChecklistContentID = X.PARChecklistContentID
			AND X.ProposalID = @ProposalID
			AND X.ResponseTypeID = 1
	LEFT OUTER JOIN [dbo].[CannedResponsesPAR] R ON
			R.CannedResponseId = X.CannedResponseId
ORDER BY C.SortOrder

GO

GRANT EXECUTE ON OBJECT::dbo.CreatePARChecklistReport TO generationReporter;
GO

/*
    File: \Stored Procedures\CreateProposalActivityReport.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\CreateProposalActivityReport.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateProposalActivityReport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CreateProposalActivityReport];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[CreateProposalActivityReport]
(
	@ProposalStatus varchar (8000),
	@AllProposals bit=NULL,
	@SpecificProposals bit=NULL,
	@CustomerType varchar (8000)=NULL,
	@PA varchar (8000)=NULL,
	@CreateStartDate [date]=NULL,
	@CreateEndDate [date]=NULL,
	@PricerNTID varchar (8000)=NULL,
	@TrackingNumber varchar(10)=NULL,
	@ExecutionUserID varchar (8000)=NULL
)	
AS
/******************************************************************************
**		 
**		Name: [CreateProposalActivityReport]
**		Desc: SSRS: Proposal Activity Report
**			
**		
**
**		Auth: Don Canuso
**		Date: 7/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		01/19/17	n99040				added fields TechLead, IndependentReviewer, LeadEstimator, ProposalMgr, CoverSheetApprover, PricingVErification
**		02/02/17	tglick				added new field [Revised Submittal Date]
**		2/15/17		Dusan				added Absolute Value
**		3/22/2017	twilson3			BOEJ-1909 Remove Profit Tracking #
**		3/19/2018	twilson3			BOEJ-3126 Add Forecast Tracking # to SSRS
**		5/30/2018	ranzalon			BOEJ-3405 - Adjusted naming of Actual Submittal Date
**		6/06/2018	brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
*******************************************************************************/


SET NOCOUNT ON

/*
	EXAMPLE OF DATA SET
	NULL Will be sent for specific proposals when Specific Proposals is False
	ALL will be sent when All is selected
	A string of IDs will be sent for every multi-select box
	
	SET	@ProposalStatus = '1,2,3'--OR ALL

	SET @AllProposals = 0

	SET	@SpecificProposals = 0

	SET @PA = '1,2,3'  /*Evaluates as Civil - Energy & Environmental Services*/

	SET @TrackingNumber  = '2013-00015'

*/


	
/*
Process Proposal Status
*/
DECLARE @tblProposalStatus TABLE (ProposalStatusID int)
IF @ProposalStatus IS NULL OR @ProposalStatus = 'All'
	BEGIN
		INSERT INTO @tblProposalStatus
		SELECT ProposalStatusID FROM dbo.ProposalStatusLU
	END
ELSE	
	BEGIN
		IF RIGHT(@ProposalStatus, 1) <> ','
	      SET @ProposalStatus = @ProposalStatus + ','
	
		WHILE (SELECT CHARINDEX (',', @ProposalStatus) ) > 1
			BEGIN
			      
				  INSERT INTO @tblProposalStatus
				  SELECT LEFT (@ProposalStatus, CHARINDEX (',', @ProposalStatus) -1)
				  SET @ProposalStatus = RIGHT (@ProposalStatus, LEN (@ProposalStatus) - CHARINDEX (',', @ProposalStatus) )
			      
			END
	END


IF  @SpecificProposals = 1
BEGIN

/*Customer Type*/
DECLARE @tblCustomerType TABLE (CustomerTypeID int)

IF @CustomerType IS NULL OR @CustomerType = 'All'
	BEGIN
		INSERT INTO @tblCustomerType
		SELECT CustomerTypeID FROM dbo.CustomerTypeLU
	END
ELSE	
	BEGIN
		IF RIGHT(@CustomerType, 1) <> ','
	      SET @CustomerType = @CustomerType + ','
	
		WHILE (SELECT CHARINDEX (',', @CustomerType) ) > 1
			BEGIN
			      
				  INSERT INTO @tblCustomerType
				  SELECT LEFT (@CustomerType, CHARINDEX (',', @CustomerType) -1)
				  SET @CustomerType = RIGHT (@CustomerType, LEN (@CustomerType) - CHARINDEX (',', @CustomerType) )
			      
			END
	END






/*Program Area*/
DECLARE @tblProgramArea TABLE (ProgramAreaID int)

IF @PA IS NULL OR @PA = 'All'
	BEGIN
		INSERT INTO @tblProgramArea
		SELECT ProgramAreaID FROM dbo.ProgramAreaLU
	END
ELSE	
	BEGIN
		IF RIGHT(@PA, 1) <> ','
	      SET @PA = @PA + ','
	
		WHILE (SELECT CHARINDEX (',', @PA) ) > 1
			BEGIN
			      
				  INSERT INTO @tblProgramArea
				  SELECT LEFT (@PA, CHARINDEX (',', @PA) -1)
				  SET @PA = RIGHT (@PA, LEN (@PA) - CHARINDEX (',', @PA) )
			      
			END
	END

END


/*Individual Running Report and the groups they belong to as @ExecutionUserID*/
DECLARE @tblExecutionUser TABLE (ExecutionUserID int)

IF @ExecutionUserID IS NULL
	RETURN 
ELSE
BEGIN
	IF RIGHT(@ExecutionUserID, 1) <> ','
		  SET @ExecutionUserID = @ExecutionUserID + ','

	WHILE (SELECT CHARINDEX (',', @ExecutionUserID) ) > 1
	BEGIN
	      
		  INSERT INTO @tblExecutionUser
		  SELECT LEFT (@ExecutionUserID, CHARINDEX (',', @ExecutionUserID) -1)
		  SET @ExecutionUserID = RIGHT (@ExecutionUserID, LEN (@ExecutionUserID) - CHARINDEX (',', @ExecutionUserID) )
	      
	END
END


SELECT 
	   [Pricer]
	  ,[IndependentReviewerName]
	  ,[PricingVerificationName]
	  ,[CoverSheetApproverName]
	  ,[ProposalMgrName]
	  ,[TechLeadName]
	  ,[LeadEstimatorName]
      ,[Tracking #]
	  ,[ForecastedTracking#]
      ,[Proposal Title]
      ,[ProgramAreaID]
      ,[LineOfBusinessID]
      ,[Line Of Business]
      ,[Program Area Name]
      ,[Customer]
      ,[Estimated Ship Date]
      ,[Estimated Value]
      ,[Date Assigned]
      , CAST (
			CASE 
				WHEN LEN (DATEPART(MM, [Checklist Complete Date])) = 1 
					THEN '0' + CAST (DATEPART(MM, [Checklist Complete Date]) AS CHAR(1)) 
				ELSE 
						CAST (DATEPART(MM, [Checklist Complete Date]) AS CHAR(2))
			END  + '/' + 
			CASE 
				WHEN LEN (DATEPART(DD, [Checklist Complete Date])) = 1 
					THEN '0' + CAST (DATEPART(DD, [Checklist Complete Date]) AS CHAR(1)) 
				ELSE 
						CAST (DATEPART(DD, [Checklist Complete Date]) AS CHAR(2))
			END  + '/' + 
			CAST (DATEPART(YYYY, [Checklist Complete Date]) AS CHAR(4))
      			AS varchar (10))	
    		 + ' ' +
			RIGHT ([Checklist Complete Date], 7) 
		AS  [Checklist Complete Date] 
      ,[Actual Submittal Date]
      ,[Total Price]
      ,[IWTA Submitted Value]
      ,[ProposalStatusID]
      ,[Proposal Status]
      ,[ProposalID]
      ,[PricerUserID]
      ,[PricerNTID]
      ,[DateCreated]
	  ,[Revised Submittal Date]
	  ,AbsoluteValue
FROM [dbo].[vwProposalActivityReport]
WHERE
	/*Proposal Status is defined as mandatory*/
	ProposalStatusID IN (SELECT ProposalStatusID FROM @tblProposalStatus) AND 
	
	(
		(
			@AllProposals = 1
		)  OR
		(
			@SpecificProposals = 1  AND
			[ProgramAreaID] IN (SELECT ProgramAreaID FROM @tblProgramArea) AND
			[CustomerTypeID] IN (SELECT CustomerTypeID FROM @tblCustomerType) 
		)
	) AND
	
	(@CreateStartDate IS NULL OR CAST(DateCreated AS DATE) > = @CreateStartDate) AND
	(@CreateEndDate IS NULL OR CAST(DateCreated AS DATE) < = @CreateEndDate) AND
	(@TrackingNumber IS NULL OR ([Tracking #] LIKE @TrackingNumber + '%' OR [ForecastedTracking#] LIKE @TrackingNumber + '%')) AND
	(@PricerNTID IS NULL OR [PricerNTID] = @PricerNTID)	
AND
	/*Now Define Role Access*/
	(
		/*If you are a System Admin, then you see everything*/
		EXISTS (
					SELECT S.SystemUserRoleID
					FROM dbo.SystemUserRole S
						INNER JOIN dbo.RoleLU R ON S.RoleID = R.RoleID
						INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
					WHERE 
						R.[Role] = 'Administrator'
				) OR
		/*System Pricer gets to see anything where they are defined as a Proposal User*/		
				(					ProposalStatusID <> 4/*Deleted*/ AND

					EXISTS
						(
							SELECT S.SystemUserRoleID
							FROM dbo.SystemUserRole S
								INNER JOIN dbo.RoleLU R ON S.RoleID = R.RoleID
								INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
							WHERE 
								R.[Role] = 'System Pricer'
						) AND
					ProposalID IN
						(
							SELECT ProposalID
							FROM dbo.ProposalUserRole PUR
								INNER JOIN @tblExecutionUser U ON PUR.UserID = U.ExecutionUserID
						)								
							
			/*Viewers get to see where they are defined as Product Line Viewers*/							
				) OR

				(					ProposalStatusID <> 4/*Deleted*/ AND

					EXISTS
						(
							SELECT S.SystemUserRoleID
							FROM dbo.SystemUserRole S
								INNER JOIN dbo.RoleLU R ON S.RoleID = R.RoleID
								INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
							WHERE 
								R.[Role] = 'Viewer'
						) AND
					LineOfBusinessID IN
						(
							SELECT LineOfBusinessID
							FROM dbo.LineOfBusinessRoleXREF X
								INNER JOIN dbo.SystemUserRole S ON X.SystemUserRoleID = S.SystemUserRoleID
								INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
						)								
							
							
				)
				
		)
GO

GRANT EXECUTE ON OBJECT::dbo.CreateProposalActivityReport TO generationReporter;
GO

/*
    File: \Stored Procedures\CreateProposalLogReport.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\CreateProposalLogReport.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateProposalLogReport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CreateProposalLogReport];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateProposalLogReport]
(
	@ProposalStatus varchar (8000),
	@AllProposals bit=NULL,
	@SpecificProposals bit=NULL,
	@Year varchar (8000)=NULL,
	@LOB varchar (8000)=NULL,
	@LeadEstimator varchar(8000)=NULL,
	@SubmitStartDate [date]=NULL,
	@SubmitEndDate [date]=NULL,
	@TrackingNumber varchar(10)=NULL,
	@ExecutionUserID varchar (8000)=NULL
)	
AS
/******************************************************************************
**		 
**		Name: [CreateProposalLogReport]
**		Desc: SSRS: Proposal Log Report
**
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/22/2020	ranzalon			BOEJ-4535 Add Lead Estimator Approval Date
**		6/30/2020	Dusan				BOEJ-4639 Add Revision Type
**										BOEJ-4590 Add Material POC and Subcontracts POC
**										BOEJ-4631 Add Reason Cert Not Required
**		7/30/2020	Dusan				BOEJ-4639 Add Latest Revision
**		1/16/2022   Dusan				IES-174 Add Contract Data to the report
**		3/7/2022	Dusan				IES-847 Modify Contracts Data data
**		7/25/2022	Dusan / Thomas		IES-1499, IES-1510, IES-1511: Added new fields into the report: Cage Codes, Type of Contract Action, Cost thru COM
**		7/28/2022	Dusan				IES-1578 Add [Profit/Fee] and [COM] individual fields to the report
**		10/13/2022	RJ					IES-1933 Cost Volume Tool
**		02/22/23	ranzalon			Add CustomerDueDate
**		7/10/23		Dusan				PROPH-1563: Add AdditionalClassification field
**		7/14/24		Dusan				PROPH-1559: Added reason for CCOPD = No
**		7/18/24		Dusan				PROPH-1560: Added Include International Costs
**		8/19/24		Dusan				PROPH-2080: Added IsSupportDefinitizingUCA field
**		11/21/24	twilson3			proph-2357 Add Insurance fields
**		02/27/25	Carlos				PROPH-2642 Added SupplierMilestoneDatesResponse field
**		05/13/25	ranzalon			PROPH-3038 Added Bid and Mission Segment EPP Dates
*******************************************************************************/

SET NOCOUNT ON

DECLARE @tblProposalStatus TABLE (ProposalStatusID int)
IF @ProposalStatus IS NULL OR @ProposalStatus = 'All'
	BEGIN
		INSERT INTO @tblProposalStatus
		SELECT ProposalStatusID FROM dbo.ProposalStatusLU
	END
ELSE	
	BEGIN
		IF RIGHT(@ProposalStatus, 1) <> ','
	      SET @ProposalStatus = @ProposalStatus + ','
	
		WHILE (SELECT CHARINDEX (',', @ProposalStatus) ) > 1
			BEGIN
			      
				  INSERT INTO @tblProposalStatus
				  SELECT LEFT (@ProposalStatus, CHARINDEX (',', @ProposalStatus) -1)
				  SET @ProposalStatus = RIGHT (@ProposalStatus, LEN (@ProposalStatus) - CHARINDEX (',', @ProposalStatus) )
			      
			END
	END

IF  @SpecificProposals = 1
BEGIN

/*Process Year*/
DECLARE @tblYear TABLE ([Year] varchar(100))
IF @Year IS NULL OR @Year = 'All'
	BEGIN
		INSERT INTO @tblYear
		SELECT DISTINCT YEAR(DateCreated) FROM dbo.Proposal
	END
ELSE	
	BEGIN
		IF RIGHT(@Year, 1) <> ','
	      SET @Year = @Year + ','
	
		WHILE (SELECT CHARINDEX (',', @Year) ) > 1
			BEGIN
			      
				  INSERT INTO @tblYear
				  SELECT LEFT (@Year, CHARINDEX (',', @Year) -1)
				  SET @Year = RIGHT (@Year, LEN (@Year) - CHARINDEX (',', @Year) )
			      
			END
	END




/*Line of Business*/
DECLARE @tblLineOfBusiness TABLE (LineOfBusinessID int)

IF @LOB IS NULL OR @LOB = 'All'
	BEGIN
		INSERT INTO @tblLineOfBusiness
		SELECT LineOfBusinessID FROM dbo.LineOfBusinessLU
	END
ELSE	
	BEGIN
		IF RIGHT(@LOB, 1) <> ','
	      SET @LOB = @LOB + ','
	
		WHILE (SELECT CHARINDEX (',', @LOB) ) > 1
			BEGIN
			      
				  INSERT INTO @tblLineOfBusiness
				  SELECT LEFT (@LOB, CHARINDEX (',', @LOB) -1)
				  SET @LOB = RIGHT (@LOB, LEN (@LOB) - CHARINDEX (',', @LOB) )
			      
			END
	END

/*Lead Estimator*/
DECLARE @tblLeadEstimator TABLE (LeadEstimatorID int)

IF @LeadEstimator IS NULL OR @LeadEstimator = 'All'
	BEGIN
		INSERT INTO @tblLeadEstimator
		SELECT  DISTINCT Pricer.UserID AS [UserID]
		FROM [dbo].[Proposal] P
			INNER JOIN 
				(
					SELECT 
						PUR.ProposalID,
						U.DisplayName AS [Pricer Name],
						U.UserID AS [UserID]
						
					FROM dbo.ProposalUserRole PUR
						INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
					WHERE	RoleID = 3 /*Lead Estimator*/
				) Pricer ON P.ProposalID = Pricer.ProposalID
	END
ELSE
BEGIN
	IF RIGHT(@LeadEstimator, 1) <> ','
		  SET @LeadEstimator = @LeadEstimator + ','

	WHILE (SELECT CHARINDEX (',', @LeadEstimator) ) > 1
	BEGIN
	      
		  INSERT INTO @tblLeadEstimator
		  SELECT LEFT (@LeadEstimator, CHARINDEX (',', @LeadEstimator) -1)
		  SET @LeadEstimator = RIGHT (@LeadEstimator, LEN (@LeadEstimator) - CHARINDEX (',', @LeadEstimator) )
	      
	END
END

END

/*Individual Running Report and the groups they belong to as @ExecutionUserID*/
DECLARE @tblExecutionUser TABLE (ExecutionUserID int)
IF @ExecutionUserID IS NULL
	RETURN 
ELSE
BEGIN
	IF RIGHT(@ExecutionUserID, 1) <> ','
		  SET @ExecutionUserID = @ExecutionUserID + ','

	WHILE (SELECT CHARINDEX (',', @ExecutionUserID) ) > 1
	BEGIN
	      
		  INSERT INTO @tblExecutionUser
		  SELECT LEFT (@ExecutionUserID, CHARINDEX (',', @ExecutionUserID) -1)
		  SET @ExecutionUserID = RIGHT (@ExecutionUserID, LEN (@ExecutionUserID) - CHARINDEX (',', @ExecutionUserID) )
	      
	END
END

DECLARE @MaxRev TABLE(MainProposalTrackingID char (10))
INSERT INTO @MaxRev
SELECT LEFT(IsNULL(NULLIF(ProposalTrackingID,''), '00-00000'), 10)
FROM dbo.Proposal
WHERE LEN(LEFT (IsNULL(NULLIF(ProposalTrackingID,''), '00-00000'), CHARINDEX ('-',IsNULL(NULLIF(ProposalTrackingID,''), '00-00000')) -1)) = 4/*To Support 4 Digit Dates*/
GROUP BY LEFT(IsNULL(NULLIF(ProposalTrackingID,''), '00-00000'), 10)
UNION
SELECT LEFT(IsNULL(NULLIF(ProposalTrackingID,''), '00-00000'), 8)
FROM dbo.Proposal
WHERE LEN(LEFT (IsNULL(NULLIF(ProposalTrackingID,''), '00-00000'), CHARINDEX ('-',IsNULL(NULLIF(ProposalTrackingID,''), '00-00000')) -1)) = 2/*To Support 2 Digit Dates*/
GROUP BY LEFT(IsNULL(NULLIF(ProposalTrackingID,''), '00-00000'), 8)

SELECT V.[ProposalID]
      ,V.[DateCreated]
      ,V.[Year]
      ,V.[ProgramAreaID]
      ,V.[LineOfBusinessID]
      ,V.[Line Of Business]
      ,V.[Program Area Name]
      ,V.[Tracking #]
      ,V.[ForecastedTracking#]
      ,V.[Proposal Title]
      ,V.[Proposal Start Date]
      ,V.[Proposal End Date]
      ,V.[LeadEstimatorUserID]
      ,V.IndependentReviewerName
	  ,V.PricingVerificationName
	  ,V.CoverSheetApproverName
	  ,V.LOBMgrName
	  ,V.ProposalMgrName
	  ,V.TechLeadName
	  ,V.LeadEstimatorName
      ,V.[Cost Volume Lead]
      ,V.[Additional Pricing Resource 1]
      ,V.[Additional Pricing Resource 2]
      ,V.[Estimated Value]
      ,V.[Estimated Ship Date]
      ,V.[Program Name]
      ,V.[Submitted Value]  
	  , CAST (
			CASE 
				WHEN LEN (DATEPART(MM, V.[Lead Estimator Approval Date])) = 1 
					THEN '0' + CAST (DATEPART(MM, V.[Lead Estimator Approval Date]) AS CHAR(1)) 
				ELSE 
						CAST (DATEPART(MM, V.[Lead Estimator Approval Date]) AS CHAR(2))
			END  + '/' + 
			CASE 
				WHEN LEN (DATEPART(DD, V.[Lead Estimator Approval Date])) = 1 
					THEN '0' + CAST (DATEPART(DD, V.[Lead Estimator Approval Date]) AS CHAR(1)) 
				ELSE 
						CAST (DATEPART(DD, V.[Lead Estimator Approval Date]) AS CHAR(2))
			END  + '/' + 
			CAST (DATEPART(YYYY, V.[Lead Estimator Approval Date]) AS CHAR(4))
      			AS varchar (10))	
    		 + ' ' +
			RIGHT (V.[Lead Estimator Approval Date], 7) 
		AS  [Lead Estimator Approval Date] 
      , CAST (
			CASE 
				WHEN LEN (DATEPART(MM, V.[Workflow Completed Date])) = 1 
					THEN '0' + CAST (DATEPART(MM, V.[Workflow Completed Date]) AS CHAR(1)) 
				ELSE 
						CAST (DATEPART(MM, V.[Workflow Completed Date]) AS CHAR(2))
			END  + '/' + 
			CASE 
				WHEN LEN (DATEPART(DD, V.[Workflow Completed Date])) = 1 
					THEN '0' + CAST (DATEPART(DD, V.[Workflow Completed Date]) AS CHAR(1)) 
				ELSE 
						CAST (DATEPART(DD, V.[Workflow Completed Date]) AS CHAR(2))
			END  + '/' + 
			CAST (DATEPART(YYYY, V.[Workflow Completed Date]) AS CHAR(4))
      			AS varchar (10))	
    		 + ' ' +
			RIGHT (V.[Workflow Completed Date], 7) 
		AS  [Workflow Completed Date]     
      ,V.[LMLaborHours]
      ,V.[LMLaborCost]
      ,V.[SubcontractorCost]
      ,V.[MaterialCost]
      ,V.[IWTACost]
      ,V.[TravelCost]
      ,V.[OtherDirectCost]
      ,V.[Profit/Fee + COM]
	  ,V.[Profit/Fee]
	  ,V.[COM]
      ,V.[Total Price]
      ,V.[ROS %]
      ,V.[Actual Submittal Date]
      ,V.[Proposal Type]
      ,V.[Contract Type]
      ,V.[Prime or Sub]
      ,V.[RFP/Contract Modification Number]
      ,V.[Elements of Cost]
      ,V.[Contracts POC]
      ,V.[Customer]
      ,V.[Customer Type]
      ,V.[Created By]
      ,V.[Created Date]
      ,V.[ProposalStatusID]
      ,V.[Proposal Status]
      ,V.[OTIS #]
      ,V.[Pricing Tool]
      ,V.[BOE Tool]
	  ,V.[Cost Volume Tool]
      ,V.[RFP Issued Date]
      ,V.[RFP Received Date]
      ,V.[Comments]
      ,V.[ContractTypeGroupID]
      ,V.[ContractTypeGroup]
      ,V.[Schedule Proposal]
      ,V.[Proposal Location]
      ,[CCPDRequired] = 
		CASE V.[CCPDRequired] 
			WHEN 1 THEN 'Yes'
			WHEN 0 THEN 'No'
			ELSE 'Unavailable for Record'
			END
	 ,[CostVolumeClassified] =
		CASE V.[CostVolumeClassified]
			WHEN 1 THEN 'Yes'
			WHEN 0 THEN 'No'
			ELSE 'Unavailable for Record'
			END
	 ,V.ProgramProposalStatus
	 ,V.[AbsoluteValue]
	 ,V.[Revised Submittal Date]
	 ,V.[Proposal Class]
	 ,V.[Agreement Date]
	 ,V.[Certification Date]
	 ,V.[CutOff Date Utilization]
	 ,V.[LOBMgrComment]
	 ,V.ReasonCertificationNotRequired
	 ,V.RevisionType
	 ,V.MaterialPOC
	 ,V.SubcontractsPOC
	 ,V.IsLatestVersion
	 -- Proposal Contract Data
	 ,V.ContractsPreviouslySubmittedRomTrackingNumber
	 ,V.ContractsPreviouslySubmittedRomDate
	 ,V.ContractsPreviouslySubmittedRomValue
	 ,V.ContractsCustomerSubmittalDate
	 ,V.ContractsCorrespondLogNumber
	 ,V.ContractsFinalNegotiatedValue
	 ,V.ContractsFinalNegotiatedDate
	 ,V.ContractsEppDelegationAuthority
 	 ,V.ContractsProgramEppDate
	 ,V.ContractsLobEppDate
	 ,V.ContractsPreSpaceEppDate
	 ,V.ContractsSpaceEppDate
	 ,V.ContractsPreCorporateEppDate
	 ,V.ContractsCorporateEppDate
	 ,V.ContractsEppRosDelegationNotes
	 ,V.ContractsLmWon
	 ,V.ContractsModCompletedDate
	 ,V.ContractsBidEppDate
	 ,V.ContractsMissionSegmentEppDate
	 -- end of Proposal Contract Data
	 ,V.CageCode
	 ,V.ContractActionType
	 ,V.CostThroughCom
	 ,V.CustomerDueDate
	 ,V.NlfResponse
	 ,V.SupplierMilestoneDatesResponse
	 ,AdditionalClassification =
		CASE V.[AdditionalClassification]
			WHEN 1 THEN 'Yes'
			WHEN 0 THEN 'No'
			ELSE NULL
			END
	,V.ReasonCcopdNo
	,V.ReasonCcopdNoOther
	,IncludeInternationalCosts =
		CASE V.IncludeInternationalCosts
			WHEN 1 THEN 'Yes'
			WHEN 0 THEN 'No'
			ELSE NULL
			END
	,IsSupportDefinitizingUCA =
		CASE V.IsSupportDefinitizingUCA
			WHEN 1 THEN 'Yes'
			WHEN 0 THEN 'No'
			ELSE NULL
			END
	,V.IsInsuranceDirect
	,V.InsuranceType
	,V.ProposedInsurance
	,V.NegotiatedInsurance
FROM [dbo].[vwProposalLogReport] V
	LEFT OUTER JOIN @MaxRev M ON 
		(
			LEFT(IsNULL(V.[Tracking #], '00-00000'),10) = M.MainProposalTrackingID AND
			LEN (M.MainProposalTrackingID) = 10
		) OR
		(
			LEFT(IsNULL(V.[Tracking #], '00-00000'),8) = M.MainProposalTrackingID AND
			LEN (M.MainProposalTrackingID) = 8
		)
WHERE
	/*Proposal Status is defined as mandatory*/
	(
		ProposalStatusID IN  
			(SELECT ProposalStatusID FROM @tblProposalStatus)
			
	) AND 
	
	(
		(
			@AllProposals = 1
		) OR	
		(
			@SpecificProposals = 1 AND
				(
					@Year = 'All' OR @Year IS NULL OR	[Year] IN (SELECT [Year] FROM @tblYear)
				) AND
				(
					@LOB = 'All' OR
					@LOB IS NULL OR 
					[LineOfBusinessID] IN (SELECT LineOfBusinessID FROM @tblLineOfBusiness) 
				) AND
				(
					@LeadEstimator = 'All' OR
					@LeadEstimator IS NULL OR
						(
							[LeadEstimatorUserID] IN (SELECT [LeadEstimatorID] FROM @tblLeadEstimator) 
						)
							
				)
		) OR		
		(
			@SubmitStartDate IS NOT NULL AND
			@SubmitEndDate IS NOT NULL AND
			CAST([Actual Submittal Date] AS DATE) > = @SubmitStartDate AND
			CAST([Actual Submittal Date] AS DATE) < = @SubmitEndDate
		)  OR		
		(
			(
				@TrackingNumber IS NOT NULL AND
				([Tracking #] LIKE @TrackingNumber + '%' OR [ForecastedTracking#] LIKE @TrackingNumber + '%')
			)			
		)	
	)

AND
	/*Now Define Role Access*/
	(
		/*If you are a System Admin, then you see everything*/
		EXISTS (
					SELECT S.SystemUserRoleID
					FROM dbo.SystemUserRole S
						INNER JOIN dbo.RoleLU R ON S.RoleID = R.RoleID
						INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
					WHERE 
						R.[Role] = 'Administrator'
				) OR
		/*System Pricer gets to see anything where they are defined as a Proposal User*/		
				(
					ProposalStatusID <> 4/*Deleted*/ AND
					EXISTS
						(
							SELECT S.SystemUserRoleID
							FROM dbo.SystemUserRole S
								INNER JOIN dbo.RoleLU R ON S.RoleID = R.RoleID
								INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
							WHERE 
								R.[Role] = 'System Pricer'
						) AND
					V.ProposalID IN
						(
							SELECT ProposalID
							FROM dbo.ProposalUserRole PUR
								INNER JOIN @tblExecutionUser U ON PUR.UserID = U.ExecutionUserID
						)								
							
			/*Viewers get to see where they are defined as Product Line Viewers*/							
				) OR

				(
					ProposalStatusID <> 4/*Deleted*/ AND

					EXISTS
						(
							SELECT S.SystemUserRoleID
							FROM dbo.SystemUserRole S
								INNER JOIN dbo.RoleLU R ON S.RoleID = R.RoleID
								INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
							WHERE 
								R.[Role] = 'Viewer'
						) AND
					LineOfBusinessID IN
						(
							SELECT LineOfBusinessID
							FROM dbo.LineOfBusinessRoleXREF X
								INNER JOIN dbo.SystemUserRole S ON X.SystemUserRoleID = S.SystemUserRoleID
								INNER JOIN @tblExecutionUser U ON S.UserID = U.ExecutionUserID
						)								
							
							
				)
		
	)
GO

GRANT EXECUTE ON OBJECT::dbo.CreateProposalLogReport TO generationReporter;
GO

/*
    File: \Stored Procedures\deleteAttachment.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteAttachment.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteAttachment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteAttachment];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteAttachment]
(
@AttachmentID int,
@UpdateDate datetime2,
@ProposalId int,
@DeleteAttachmentRecord bit
)
AS
/******************************************************************************
**		 
**		Name: [deleteAttachment]
**		Desc: Delete Attachment
**			
**
**		Auth: twilson3
**		Date: 9/7/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**      --------    --------            ---------------------------------------
**      7/1/2020	Dusan				BOEJ-4638 Post submittal attachments working with Revisioned Proposal
*******************************************************************************/
	SET NOCOUNT ON 
	IF (SELECT UpdateDate FROM [dbo].[Attachment] WHERE ID = @AttachmentID) = @UpdateDate
		BEGIN
			DELETE FROM dbo.[ProposalsAttachments] WHERE AttachmentId = @AttachmentID AND ProposalId = @ProposalId
			IF @DeleteAttachmentRecord = 1 
				DELETE FROM dbo.[Attachment] WHERE ID = @AttachmentID
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage = 'The Attachment with ID ' + CAST(@AttachmentID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (@ErrorMessage, 11, 1)
			RETURN
		END

GO

/*
    File: \Stored Procedures\deleteChecklist.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteChecklist.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteChecklist];

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteChecklist]
(
@ProposalID int,
@UpdateDate datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteChecklist]
**		Desc: Deletes the Checklist for a Proposal.  Needed when switching the ProposalClass to Forecasted.
**			
**		
**
**		Auth: Chris Patton
**		Date: 04/10/18
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**      04/10/2018	pattoncr			Initial creation.  BOEJ-3301 - Checklist and Post Submittal Attachments not cleared when changing a Proposal to Forecasted
*******************************************************************************/
SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[ProposalChecklist] WHERE ProposalID = @ProposalID ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.ProposalPARChecklistXREF WHERE ProposalID = @ProposalID;
			DELETE FROM dbo.ProposalPPRChecklistXREF WHERE ProposalID = @ProposalID;
			DELETE FROM dbo.ProposalChecklist WHERE ProposalID = @ProposalID;
			DELETE FROM dbo.ProposalChecklistComplete WHERE ProposalID = @ProposalID;
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Proposal Checklist for the Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO

/*
    File: \Stored Procedures\deleteContractType.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteContractType.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteContractType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteContractType];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteContractType]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteContractType]
	**		Desc:	Delete Contract Type LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/7/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.ContractTypeGroupXREF
		WHERE ContractTypeId = @Id

	DELETE FROM dbo.[ContractTypeLU]
		WHERE ContractTypeId = @Id

GO

/*
    File: \Stored Procedures\deleteContractTypeGroup.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteContractTypeGroup.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteContractTypeGroup]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteContractTypeGroup];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteContractTypeGroup]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteContractTypeGroup]
	**		Desc:	Delete Contract Type Group LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/7/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.ContractTypeGroupXREF
		WHERE ContractTypeGroupId = @Id

	DELETE FROM dbo.[ContractTypeGroupLU]
		WHERE ContractTypeGroupId = @Id

GO

/*
    File: \Stored Procedures\deletegenTRACUser.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\deletegenTRACUser.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deletegenTRACUser]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deletegenTRACUser];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deletegenTRACUser]
(
@UserID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deletegenTRACUser]
**		Desc: Delete genTRACUser 
**			
**		
**
**		Auth: Don Canuso
**		Date: 4/5/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not display technical details to the user
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[genTRACUser] WHERE UserID = @UserID ) = @UpdateDT
		BEGIN
		

			DELETE FROM dbo.genTRACUser
			WHERE UserID = @UserID
	
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The User with ID ' + CAST(@UserID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO

/*
    File: \Stored Procedures\deleteLOB.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteLOB.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteLOB]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteLOB];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteLOB]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteLOB]
	**		Desc:	Delete LOB LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/7/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			----------------------------------------
	**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.[LineOfBusinessLU]
		WHERE [LineOfBusinessID] = @Id

GO

/*
    File: \Stored Procedures\deleteProgramArea.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteProgramArea.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProgramArea]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProgramArea];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProgramArea]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteProgramArea]
	**		Desc:	Delete Program Area LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/7/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			----------------------------------------
	**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.[ProgramAreaLU]
		WHERE [ProgramAreaID] = @Id

GO

/*
    File: \Stored Procedures\deleteProposal.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteProposal.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposal]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposal];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProposal]
(
@ProposalID int,
@UpdateDate datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteProposal]
**		Desc: Delete Proposal
**			
**
**		Auth: Don Canuso
**		Date: 04/3/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		9/7/2017	twilson3			BOEJ-2459 Add Attachment table
**		7/6/2020	Dusan				Attachment Proposals table
**		11/2/2021	Koovackal			Contracts Data and Offer tables
**      2/15/2022   Koovackal           Remove Contracts Offer table deletion
*******************************************************************************/
SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID ) = @UpdateDate
		BEGIN
			DECLARE @attachmentIds AS Table (Id int)
			INSERT INTO @attachmentIds SELECT AttachmentId FROM dbo.ProposalsAttachments WHERE ProposalId = @ProposalId AND IsRevisionReference = 0
			DELETE FROM dbo.ProposalsAttachments WHERE ProposalID = @ProposalID
			DELETE FROM dbo.Attachment WHERE Id IN (SELECT * FROM @attachmentIds)
			DELETE FROM dbo.ProposalUserRole WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalContractTypeXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalCostElementXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalPARChecklistXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalPPRChecklistXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalChecklist WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalChecklistComplete WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalContractsData WHERE ProposalID = @ProposalID
			DELETE FROM dbo.Proposal WHERE ProposalID = @ProposalID
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =   'The Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (@ErrorMessage, 11, 1)
			RETURN
		END
GO

/*
    File: \Stored Procedures\deleteProposalClass.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteProposalClass.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposalClass]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposalClass];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProposalClass]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteProposalClass]
	**		Desc:	Delete Proposal Class LU values 
	**			
	**		
	**
	**		Auth: Dusan Palider
	**		Date: 7/17/2016
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.ProposalClassLU
		WHERE ProposalClassId = @Id

GO


/*
    File: \Stored Procedures\deleteProposalRolesByProposalID.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteProposalRolesByProposalID.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposalRolesByProposalID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposalRolesByProposalID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProposalRolesByProposalID]
(
@ProposalID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteProposalRolesByProposalID]
**		Desc: Deletes all Proposal User Roles from Proposal User Role
**				fpr a Proposal
**			
**		   
**		
**
**		Auth: Don Canuso
**		Date: 4/17/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
******************************************************************************/
SET NOCOUNT ON 

DELETE FROM dbo.ProposalUserRole
WHERE [ProposalID] = @ProposalID

GO

/*
    File: \Stored Procedures\deleteProposalRolesByUserID.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteProposalRolesByUserID.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposalRolesByUserID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposalRolesByUserID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProposalRolesByUserID]
(
@UserID int,
@UpdateDT datetime2,
@ProposalID int,
@RoleID int,
@RoleTypeID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteProposalRolesByUserID]
**		Desc: Deletes a Proposal User Role from Proposal User Role
**			
**		   
**		
**
**		Auth: Don Canuso
**		Date: 4/3/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not display technical details to the user
******************************************************************************/
SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDate FROM dbo.ProposalUserRole WHERE 
			UserID = @UserID AND 
			ProposalID = @ProposalID AND 
			RoleID = @RoleID AND
			IsNull(RoleTypeID,-99) = ISNULL(@RoleTypeID,-99)
			) = @UpdateDT
	BEGIN
		DELETE FROM dbo.ProposalUserRole
		WHERE 
			[UserID] = @UserID AND
			[ProposalID] = @ProposalID AND 
			[RoleID] = @RoleID AND
			IsNull(RoleTypeID,-99) = ISNULL(@RoleTypeID,-99)
	END
ELSE
	BEGIN
		SET @ErrorMessage =   'The Proposal User Role has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
			@ErrorMessage, -- Message text.
	        11, -- Severity,/*Severity Changed to 11*/
			1 -- State,
			)
		RETURN
	END

GO

/*
    File: \Stored Procedures\deleteProposalType.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteProposalType.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposalType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposalType];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProposalType]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteProposalType]
	**		Desc:	Delete Proposal Type LU values 
	**			
	**		
	**
	**		Auth: Dusan Palider
	**		Date: 7/17/2016
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.[ProposalTypeLU]
		WHERE ProposalTypeId = @Id

GO

/*
    File: \Stored Procedures\deleteSystemRoleByID.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteSystemRoleByID.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSystemRoleByID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSystemRoleByID];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteSystemRoleByID]
(
@UpdateDT datetime2,
@SystemUserRoleID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteSystemRoleByID]
**		Desc: Deletes a System User Role from System User Role
**			
**		   
**		
**
**		Auth: Don Canuso
**		Date: 4/23/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		11/20/14	dcanuso				New Proposal Set Up role - Renamed to 
**										reuse Product Line Viewer to Role XREF
**		1/12/2017	brunworg			BOEJ-1688 Update PTM SPs to not display technical details to the user
**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
******************************************************************************/
SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDT FROM dbo.SystemUserRole WHERE SystemUserRoleID = @SystemUserRoleID) = @UpdateDT
	BEGIN
			
		DELETE FROM dbo.LineOfBusinessRoleXREF			
			FROM dbo.LineOfBusinessRoleXREF X
				INNER JOIN dbo.SystemUserRole S ON X.SystemUserRoleID = S.SystemUserRoleID			
		WHERE S.SystemUserRoleID = @SystemUserRoleID
		
	
		DELETE FROM dbo.SystemUserRole
		WHERE SystemUserRoleID = @SystemUserRoleID
	END
ELSE
	BEGIN
		SET @ErrorMessage =   'The System User Role has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
			@ErrorMessage, -- Message text.
	        11, -- Severity,/*Severity Changed to 11*/
			1 -- State,
			)
		RETURN
	END
GO

/*
    File: \Stored Procedures\deleteTypeOfRequest.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteTypeOfRequest.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteTypeOfRequest]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteTypeOfRequest];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteTypeOfRequest]
(
	@Id			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteTypeOfRequest]
	**		Desc:	Delete Type Of Request LU Values
	**			
	**		
	**
	**		Auth: Dusan Palider
	**		Date: 7/17/2016
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	DELETE FROM dbo.RequestTypeLU
		WHERE RequestTypeID = @Id

GO

/*
    File: \Stored Procedures\GetAcvUsageData.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\GetAcvUsageData.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAcvUsageData]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].GetAcvUsageData;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetAcvUsageData]  AS
	/******************************************************************************
	**		 
	**		Name: [GetAcvUsageData]
	**		Desc: SSRS: Create Acv Usage Data Report
	**			
	**		
	**
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		3/20/24		Dusan				Initial Creation
	*******************************************************************************/

	SET NOCOUNT ON
		SELECT YEAR(c.CreatedDt) AS Year, lob.LineOfBusinessName AS Lob, COUNT(1) AS Count
				FROM ACV.dbo.CostVolume c, genTrac.dbo.Proposal p, genTrac.dbo.LineOfBusinessLU lob		
				WHERE		
					c.PtmTrackingNumber = p.ProposalTrackingID	
					AND p.LineOfBusinessID = lob.LineOfBusinessID	
				GROUP BY lob.LineOfBusinessName, YEAR(c.CreatedDT);


GRANT EXECUTE ON OBJECT::dbo.GetAcvUsageData TO generationReporter;
GO


/*
    File: \Stored Procedures\GetAcvUsageDataComplete.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\GetAcvUsageDataComplete.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAcvUsageDataComplete]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].GetAcvUsageDataComplete;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetAcvUsageDataComplete]  AS
	/******************************************************************************
	**		 
	**		Name: [GetAcvUsageDataComplete]
	**		Desc: SSRS: Create Acv Usage Data Report - Complete Records Only
	**			
	**		
	**
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		3/20/24		Dusan				Initial Creation
	*******************************************************************************/

	SET NOCOUNT ON
		SELECT YEAR(pc.ProposalSubmittalDate) AS Year, lob.LineOfBusinessName AS Lob, COUNT(1) AS Count
				FROM ACV.dbo.CostVolume c, genTrac.dbo.Proposal p, genTrac.dbo.LineOfBusinessLU lob, genTrac.dbo.ProposalChecklist pc
				WHERE		
					c.PtmTrackingNumber = p.ProposalTrackingID	
					AND p.LineOfBusinessID = lob.LineOfBusinessID
					AND p.ProposalID = pc.ProposalID
					AND pc.ProposalSubmittalDate IS NOT NULL
				GROUP BY lob.LineOfBusinessName, YEAR(pc.ProposalSubmittalDate);

GRANT EXECUTE ON OBJECT::dbo.GetAcvUsageDataComplete TO generationReporter;
GO


/*
    File: \Stored Procedures\GetAcvUsageDataDetails.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\GetAcvUsageDataDetails.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAcvUsageDataDetails]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].GetAcvUsageDataDetails;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetAcvUsageDataDetails]  AS
	/******************************************************************************
	**		 
	**		Name: [GetAcvUsageDataDetails]
	**		Desc: SSRS: Create Acv Usage Data Report - Details
	**			
	**		
	**
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		3/20/24		Dusan				Initial Creation
	*******************************************************************************/

	SET NOCOUNT ON
		SELECT c.PtmTrackingNumber, c.PtmTitle, c.WorkspaceId, c.WorkspaceShortName, c.WorkspaceName,
				cU.NTID AS CvAuthorNtid, cU.DisplayName AS CvAuthorName, -- THIS WILL NEED TO CHANGE ONCE THE MULTIPLE AUTHORS ARE REDONE
				c.CreatedDT AS CostVolumeCreatedDate, c.UpdateDT AS CostVolumeUpdateDate,
				p.ProposalTitle, p.UpdateDate AS PtmUpdateDate, pS.ProposalStatus, p.DateCreated AS PtmCreatedDate, p.DateAssigned AS AssignedDate,
				pT.ProposalType, p.IsIWTA, p.ProgramName, p.Customer, pCT.CustomerType, pIs.ISGSRole, pR.RequestType, p.RFPNumber, lob.LineOfBusinessName,
				pA.ProgramAreaName, pp.PricingTool, p.PricingToolName, pb.BOETool, p.BOEToolName, pC.CostVolumeTool, p.CostVolumeToolName, p.AnticipatedDeliveryDate,
				p.EstimatedProposalValue, p.IsActive, pAuthor.DisplayName AS PtmAuthor, RFPIssuedDate, RFPReceivedDate, pCG.ContractTypeGroup, p.WorkflowStatus
				FROM ACV.dbo.CostVolume c, genTrac.dbo.Proposal p, genTrac.dbo.LineOfBusinessLU lob, ACV.dbo.[User] as cU, genTrac.dbo.ProposalStatusLU pS,
					genTrac.dbo.ProposalTypeLU pT, genTrac.dbo.CustomerTypeLU pCT, genTrac.dbo.ISGSRoleLU pIs, genTrac.dbo.RequestTypeLU pR,
					genTrac.dbo.ProgramAreaLU pA, genTrac.dbo.PricingToolLU pP, genTrac.dbo.BOEToolLU pB, genTrac.dbo.CostVolumeToolLU pC, 
					genTrac.dbo.genTRACUser pAuthor, genTrac.dbo.ContractTypeGroupLU pCG
				WHERE		
					c.PtmTrackingNumber = p.ProposalTrackingID	
					AND p.LineOfBusinessID = lob.LineOfBusinessID
					AND c.AuthorId = cU.Id
					AND p.ProposalStatusID = pS.ProposalStatusID
					AND p.ProposalTypeID = pT.ProposalTypeID
					AND p.CustomerTypeID = pCT.CustomerTypeID
					AND p.ISGSRoleID = pIs.ISGSRoleID
					AND p.RequestTypeID = pR.RequestTypeID
					AND p.ProgramAreaID = pA.ProgramAreaID
					AND p.PricingToolID = pP.PricingToolID
					AND p.BOEToolID = pB.BOEToolID
					AND p.CostVolumeToolID = pC.CostVolumeToolID
					AND p.CreatedByUserID = pAuthor.UserID
					AND p.ContractTypeGroupID = pCG.ContractTypeGroupID

GRANT EXECUTE ON OBJECT::dbo.GetAcvUsageDataDetails TO generationReporter;
GO


/*
    File: \Stored Procedures\getMyFunctionalTree.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\getMyFunctionalTree.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getMyFunctionalTree]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getMyFunctionalTree];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getMyFunctionalTree]
(
@NTID varchar (1000),
@genTracUserID INT
)
AS
/******************************************************************************
**          
**          Name: [getMyFunctionalTree]
**          Desc: Mimics My Functional Tree (Including Me)
**                
**          
**
**          Auth: Don Canuso
**          Date: 4/11/14
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                -------------------------------
**			4/11/14		dcanuso					Initial creation.
**			11/29/17	pattoncr				Remove domain.
******************************************************************************/
SET NOCOUNT ON 

/*
TESTING

DECLARE @NTID varchar (100) = 'sipiak'
DECLARE @genTracUserID INT
*/


IF @genTracUserID IS NULL AND @NTID IS NOT NULL
BEGIN
	SELECT @genTracUserID = UserID 
	FROM dbo.genTRACUser 
	WHERE 
		NTID = @NTID
END



IF @genTracUserID IS NOT NULL AND @NTID IS NULL
BEGIN
	SELECT 
		@NTID = NTID
	FROM dbo.genTRACUser 
	WHERE 
		UserID = @genTracUserID
END





/*We want this employee and everyone under this employee - Pricer and Cost Volume Lead columns only*/
DECLARE @EmployeeLMPeopleID varchar (6)

SELECT @EmployeeLMPeopleID = empl_ser_no
FROM DataMart.DataMartEmployee
WHERE 
	nt_account_nm = @NTID




;
WITH MyCTE
AS ( 
SELECT empl_ser_no AS LMPeopleID, empl_first_nm as  FirstName, empl_last_nm as  LastName,
rpt_to_ser_no as  FunctionalManagerLMPeopleID,
nt_account_nm as  NTID
FROM DataMart.DataMartEmployee
WHERE 
empl_ser_no = @EmployeeLMPeopleID

UNION ALL

SELECT empl_ser_no AS LMPeopleID, empl_first_nm as  FirstName, empl_last_nm as  LastName,
rpt_to_ser_no as  FunctionalManagerLMPeopleID,
nt_account_nm as  NTID
FROM DataMart.DataMartEmployee E
	INNER JOIN MyCTE  ON E.rpt_to_ser_no = MyCTE.LMPeopleID
WHERE E.rpt_to_ser_no IS NOT NULL 
)


SELECT 
U.UserID AS genTracUserID
FROM MyCTE C
	INNER JOIN dbo.genTRACUser U ON
		C.NTID = U.NTID

GO

/*
    File: \Stored Procedures\getMyProposals.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\getMyProposals.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getMyProposals]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getMyProposals];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getMyProposals]
(
	@ProposalStatusID int = NULL,
	@AssignedStart datetime2(7) = NULL,
	@AssignedEnd datetime2(7) = NULL,
	@Search varchar (250) = NULL,
	@NTID varchar (1000) = NULL,
	@ShowProposalsForMyOrganization bit = NULL,
	@UserAndGroupIDs XML = NULL, -- XML list of User and Group IDs (Only used when ShowProposalsForMyOrganization = 1)
	@ProposalClassFilterID int = NULL
)
AS
/******************************************************************************
**          
**          Name: [getMyProposals]
**          Desc: Mimics My Proposal Page
**                
**          Auth: Don Canuso
**          Date: 4/11/14
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                -------------------------------
**			3/10/2020	ranzalon				BOEJ-4490 - No Bid
**			3/31/2020	ranzalon				BOEJ-4531 No Bid Date
**			6/5/2020	Dusan					BOEJ-4657: Adding Forecasted proposals into field for search; cleaned up some formatting on text
**			6/18/2020	ranzalon				BOEJ-4636 - Revised Proposals in All
**			7/15/2020	Dusan					BOEJ-4700: Pull Has / Is Revision Data
**			8/14/2020	ranzalon				BOEJ-4676 - Use revised submittal date when available
**			2/2/2021	ranzalon				BOEJ-4861 - Add submitted value
**			2/28/2022	koovackal				IES-846 Create 2 new statuses
**			11/14/2022	twilson3				IES-1976 - Fix missing parens
******************************************************************************/
	SET NOCOUNT ON 

	IF @Search IS NOT NULL SET @Search = '%' + @Search + '%'
	DECLARE @genTracUserID INT = NULL;
	IF @genTracUserID IS NULL AND @NTID IS NOT NULL
	BEGIN
		SELECT @genTracUserID = UserID 
			FROM dbo.genTRACUser 
			WHERE NTID = @NTID
	END
	/*We want this employee and everyone under this employee*/
	DECLARE @EmployeeLMPeopleID varchar (6)

	SELECT @EmployeeLMPeopleID = empl_ser_no FROM DataMart.DataMartEmployee WHERE nt_account_nm = @NTID;
	WITH MyCTE
		AS ( SELECT empl_ser_no AS LMPeopleID, empl_first_nm as  FirstName, empl_last_nm as  LastName, rpt_to_ser_no as  FunctionalManagerLMPeopleID, nt_account_nm as  NTID
				FROM DataMart.DataMartEmployee
				WHERE empl_ser_no = @EmployeeLMPeopleID
			UNION ALL
			SELECT empl_ser_no AS LMPeopleID, empl_first_nm as  FirstName, empl_last_nm as  LastName, rpt_to_ser_no as  FunctionalManagerLMPeopleID, nt_account_nm as  NTID
				FROM DataMart.DataMartEmployee E
					INNER JOIN MyCTE  ON E.rpt_to_ser_no = MyCTE.LMPeopleID
				WHERE E.rpt_to_ser_no IS NOT NULL 
		  )

	SELECT DISTINCT
		P.ProposalID AS ProposalID,
		P.ProposalTrackingID AS [Tracking Number],
		P.ProposalTitle AS [Proposal Title],
		PA.ProgramAreaName AS [Program Area],
		P.Customer AS [Customer],
		P.CustomerTypeId As [CustomerTypeId],
		P.EstimatedProposalValue AS [Estimated Value],
		PC.ISGSTotalPrice as [Submitted Value],
		CaptureManager.DisplayName AS [Capture Manager],
		LeadEstimator.DisplayName AS [Pricer Name],
		PeerReviewer.DisplayName AS [Peer Reviewer],
		CostVolumeLead.[Cost Volume Lead] AS [Cost Volume Lead],
		IndependentReviewer.UserID AS IndependentReviewerUserId,
		IndependentReviewer.NTID AS IndependentReviewerNtId,
		IndependentReviewer.DisplayName AS IndependentReviewerName,
		PricingVerification.UserID AS PricingVerificationUserId,
		PricingVerification.NTID AS PricingVerificationNtId,
		PricingVerification.DisplayName AS PricingVerificationName,
		CoverSheetApprover.UserID AS CoverSheetApproverUserId,
		CoverSheetApprover.NTID AS CoverSheetApproverNtId,
		CoverSheetApprover.DisplayName AS CoverSheetApproverName,
		ProposalMgr.UserID AS ProposalMgrUserId,
		ProposalMgr.NTID AS ProposalMgrNtId,
		ProposalMgr.DisplayName AS ProposalMgrName,
		TechLead.UserID AS TechLeadUserId,
		TechLead.NTID  AS TechLeadNtId,
		TechLead.DisplayName AS TechLeadName,
		LeadEstimator.UserID AS LeadEstimatorUserId,
		LeadEstimator.NTID  AS LeadEstimatorNtId,
		LeadEstimator.DisplayName AS LeadEstimatorName,
		P.DateAssigned AS [Date Assigned],
		CASE WHEN P.RevisedSubmittalDate IS NOT NULL THEN P.RevisedSubmittalDate ELSE P.AnticipatedDeliveryDate END AS [Estimated Ship Date (Due Date)],
		CAST(CC.ChecklistCompleteDate AS DATE) AS [ChecklistCompleteDate],
		CAST(PC.ProposalSubmittalDate AS DATE) AS [Proposal Submit Date],
		S.ProposalStatus AS [Proposal Status],
		P.RevisedSubmittalDate AS [Revised Submittal Date],
		P.DocumentId AS DocumentId,
		P.ForecastedTrackingID AS [Forecasted Tracking Number],
		PCLU.ProposalClass AS ProposalClass,
		CASE 
			WHEN EXISTS (SELECT 1 FROM Proposal WHERE RevisionOfId = p.ProposalID) OR p.RevisionOfId IS NOT NULL THEN 1
			ELSE 0
		END AS HasOrIsRevision
	FROM dbo.Proposal P 
		INNER JOIN dbo.ProgramAreaLU PA ON P.ProgramAreaID = PA.ProgramAreaID
		INNER JOIN dbo.ProposalStatusLU S ON P.ProposalStatusID = S.ProposalStatusID
		INNER JOIN dbo.ProposalClassLU PCLU ON P.ProposalClassID = PCLU.ProposalClassID
		INNER JOIN dbo.ProposalUserRole PUR ON P.ProposalID = PUR.ProposalID
		LEFT OUTER JOIN (SELECT MAX(submitDate) AS ChecklistCompleteDate, ProposalID FROM ProposalChecklistComplete GROUP BY ProposalID) CC ON P.ProposalID = CC.ProposalID
		LEFT OUTER JOIN dbo.ProposalChecklist PC ON P.ProposalID = PC.ProposalID
		LEFT OUTER JOIN (SELECT PUR.ProposalID,U.DisplayName
							FROM dbo.ProposalUserRole PUR INNER JOIN dbo.RoleLU R ON PUR.RoleID = R.RoleID INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
							WHERE PUR.RoleID = 11 /*11  Peer Reviewer*/
						) PeerReviewer ON P.ProposalID = PeerReviewer.ProposalID
		LEFT OUTER JOIN (SELECT PUR.ProposalID,U.DisplayName
							FROM dbo.ProposalUserRole PUR INNER JOIN dbo.RoleLU R ON PUR.RoleID = R.RoleID INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
							WHERE PUR.RoleID = 1 /*1	Capture Manager*/
						) CaptureManager ON P.ProposalID = CaptureManager.ProposalID
		LEFT OUTER JOIN (SELECT PUR.ProposalID, U.DisplayName AS [Cost Volume Lead], U.UserID AS [Cost Volume Lead UserID], U.NTID AS [Cost Volume Lead NTID]
							FROM dbo.ProposalUserRole PUR INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
							WHERE RoleID = 2 /*Cost Volume Lead*/
						) [CostVolumeLead] ON P.ProposalID = [CostVolumeLead].ProposalID
		LEFT OUTER JOIN (SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
							FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
							WHERE PUR.RoleID = 11
						) IndependentReviewer ON P.ProposalID = IndependentReviewer.ProposalID
 		LEFT OUTER JOIN (SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
							FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
							WHERE PUR.RoleID = 16
						) TechLead ON P.ProposalID = TechLead.ProposalID
		LEFT OUTER JOIN (SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
							FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
							WHERE PUR.RoleID = 17
						) ProposalMgr ON P.ProposalID = ProposalMgr.ProposalID
		LEFT OUTER JOIN (SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
							FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
							WHERE PUR.RoleID = 18
						) CoverSheetApprover ON P.ProposalID = CoverSheetApprover.ProposalID
		LEFT OUTER JOIN (SELECT PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
							FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
							WHERE PUR.RoleID = 19
						) PricingVerification ON P.ProposalID = PricingVerification.ProposalID		
		LEFT JOIN (SELECT PUR.ProposalID, U.DisplayName, U.UserID, U.NTID
						FROM ProposalUserRole PUR JOIN genTRACUser U ON PUR.UserID = U.UserID
						WHERE PUR.RoleID = 3
				  ) LeadEstimator ON P.ProposalID = LeadEstimator.ProposalID
		LEFT OUTER JOIN (SELECT ProposalID, MAX(SubmitDate) AS MaxSubmitDate
							FROM dbo.ProposalChecklistComplete
							GROUP BY ProposalID
						) [ProposalReviewCompleteDate] ON P.ProposalID = [ProposalReviewCompleteDate].ProposalID
	WHERE
	(
		(
			(@ProposalStatusID IS NULL AND P.ProposalStatusID IN (1/*In Progress*/,2/*Completed*/,6/*Pending Certification*/,7/*No Bid*/,8/*Revised*/,
																  9/*Pending Contractual Award*/,10/*Lost*/))
			OR (@ProposalStatusID IS NOT NULL AND P.ProposalStatusID = @ProposalStatusID)
		) AND (
			(
				(@ProposalStatusID = 1/*In Progress*/ OR @ProposalStatusID = 6/*Pending Certification*/ OR @ProposalStatusID = 8/*Revised*/ OR @ProposalStatusID = 9/*Pending Contractual Award*/) 
				AND (@AssignedStart IS NULL OR CAST (P.DateAssigned AS Date) > = @AssignedStart)
				AND (@AssignedEnd IS NULL OR CAST (P.DateAssigned AS Date) < = @AssignedEnd)
			) OR (
				P.ProposalStatusID = 2/*Completed*/ OR P.ProposalStatusID = 10/*Lost*/
				AND (@AssignedStart IS NULL OR CAST ([ProposalReviewCompleteDate].MaxSubmitDate AS Date) > = @AssignedStart) 
				AND (@AssignedEnd IS NULL OR CAST ([ProposalReviewCompleteDate].MaxSubmitDate AS Date) < = @AssignedEnd) 
			) OR (
				P.ProposalStatusID = 7/*No Bid*/ 
				AND (@AssignedStart IS NULL OR CAST (P.NoBidDate AS Date) > = @AssignedStart) 
				AND (@AssignedEnd IS NULL OR CAST (P.NoBidDate AS Date) < = @AssignedEnd) 
			) OR (
				@ProposalStatusID IS NULL 
				AND (
					  (
						(@AssignedStart IS NULL OR CAST (P.DateAssigned AS Date) > = @AssignedStart) 
						AND (@AssignedEnd IS NULL OR CAST (P.DateAssigned AS Date) < = @AssignedEnd) 
					  ) OR (
						(@AssignedStart IS NULL OR CAST ([ProposalReviewCompleteDate].MaxSubmitDate AS Date) > = @AssignedStart) 
						AND (@AssignedEnd IS NULL OR CAST ([ProposalReviewCompleteDate].MaxSubmitDate AS Date) < = @AssignedEnd) 
					  )
					) 
			)
		) AND (
			(ISNULL(@ProposalClassFilterID,0) < 1) /* All */
			OR (ISNULL(@ProposalClassFilterID,0) = 1 AND PCLU.ProposalClass = 'Forecasted') /* Forecasted */ 
			OR (ISNULL(@ProposalClassFilterID,0) = 2 AND PCLU.ProposalClass != 'Forecasted') /* Non-Forecasted */ 
		)
	) AND (/*Permissions*/
		(PUR.UserID = @genTracUserID)
		OR (
			PUR.RoleID IN (3 /*Pricer*/, 2 /*Cost Volume Lead*/, 18 /*Cover Sheet Approver*/,19 /*Pricing Verification*/,11 /*Independent Reviewer*/,20 /*LOB Estimating Lead/Mgr*/,4 /*Additional Pricing Resource 1*/,5 /*Additional Pricing Resource 2*/,12 /*Backup Lead Estimator*/) AND
			PUR.UserID IN (SELECT UserID FROM dbo.genTRACUser U INNER JOIN MyCTE C ON U.NTID = C.NTID)
		) OR (	
			@ShowProposalsForMyOrganization = 1 AND
			P.LineOfBusinessID in (SELECT XREF.LineOfBusinessID
										FROM dbo.LineOfBusinessRoleXREF XREF
											JOIN dbo.SystemUserRole SUR on SUR.SystemUserRoleID = XREF.SystemUserRoleID
											JOIN dbo.genTRACUser u on SUR.UserID = u.UserID
											JOIN (SELECT Node.Id.value('text()[1]', 'varchar(1000)') AS NTID FROM @UserAndGroupIDs.nodes('ROOT/id') AS Node(Id)) as UserAndGroupIDs on UserAndGroupIDs.NTID = u.NTID
										WHERE SUR.RoleID = 13 /*Viewer*/)
		)
	) AND (/*Search Capability*/
		@Search IS NULL
		OR (
			P.ProposalTrackingID LIKE @Search OR
			P.ForecastedTrackingId LIKE @Search OR
			P.ProposalTitle  LIKE @Search OR
			PA.ProgramAreaName  LIKE @Search OR
			P.Customer  LIKE @Search OR
			CAST (P.EstimatedProposalValue AS varchar (MAX))  LIKE @Search OR
			CaptureManager.DisplayName  LIKE @Search OR
			LeadEstimator.DisplayName  LIKE @Search OR
			CostVolumeLead.[Cost Volume Lead]  LIKE @Search OR
			CAST (P.DateAssigned AS varchar (100))  LIKE @Search OR
			CAST (P.AnticipatedDeliveryDate AS varchar (100)) LIKE @Search OR
			CAST (PC.ProposalSubmittalDate AS varchar (100))  LIKE @Search OR
			S.ProposalStatus LIKE @Search 
		)
	)
GO

GRANT EXECUTE ON OBJECT::dbo.getMyProposals TO generationReporter;
GO

/*
    File: \Stored Procedures\GetProposalRevisionHistory.sql
*/
PRINT '### Starting file: \Stored Procedures\GetProposalRevisionHistory.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetProposalRevisionHistory]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[GetProposalRevisionHistory];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetProposalRevisionHistory] (@ProposalId INT)
AS
/******************************************************************************
**		 
**		Name: [GetProposalRevisionHistory]
**		Desc: Gets a revision history for a proposal. Any proposal id in the 
**				history tree will result in the entire history to be returned 
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		7/13/2020	Dusan				BOEJ-4694: Initial SP creation
*******************************************************************************/
	SET NOCOUNT ON;

	WITH ProposalRevisions AS
		(
		SELECT ProposalId, RevisionOfId, ProposalTrackingId, ProposalId AS ParentId
			FROM Proposal 
			WHERE RevisionOfId IS NULL AND ForecastedTrackingId IS NULL
		UNION ALL
		SELECT P.ProposalId, P.RevisionOfId, P.ProposalTrackingId, r.ParentId AS ParentId 
			FROM Proposal AS P
				INNER JOIN ProposalRevisions r ON p.RevisionOfId = r.ProposalID
			WHERE P.RevisionOfId IS NOT NULL AND ForecastedTrackingId IS NULL
		)
	SELECT ProposalId, RevisionOfId, ProposalTrackingId
		FROM ProposalRevisions
		WHERE ParentId = (SELECT ParentId FROM ProposalRevisions WHERE ProposalId = @proposalId);

GO


/*
    File: \Stored Procedures\insertProposalUserRole.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\insertProposalUserRole.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertProposalUserRole]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertProposalUserRole];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertProposalUserRole]
(
	@ProposalID int,
	@RoleID int,
	@RoleTypeID int,
	@UserID int
	)
AS
/******************************************************************************
**		 
**		Name: insertProposalUserRole
**		Desc: Inserts a record into the Proposal User Role table for DTO
**			
**		
**
**		Auth: Don Canuso
**		Date: 4/3/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		5/13/13		dcanuso				WI 18300
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not display technical details to the user
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE @ErrorMessage varchar (500)

IF EXISTS	(SELECT 1 FROM [dbo].[ProposalUserRole] WHERE	
					 UserID = @UserID AND
					 ProposalID = @ProposalID AND
				 	 RoleID = @RoleID AND
				 	 ISNULL(RoleTypeID, -99) = ISNULL(@RoleTypeID, -99)
			)
			BEGIN
				SET @ErrorMessage =   'This Proposal User Role already exists.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
ELSE
	BEGIN		
		
		DECLARE @UpdateDate datetime2 = GETDATE(),
				@ProposalUserRoleID int
			
		INSERT INTO [dbo].[ProposalUserRole]
				   ([UserID]
				   ,[RoleID]
				   ,[ProposalID]
				   ,[UpdateDate]
				   ,[RoleTypeID])		 
		OUTPUT inserted.ProposalUserRoleID INTO @Inserted
			 VALUES
				   (
					@UserID, 
					@RoleID, 
					@ProposalID, 
					@UpdateDate,
					@RoleTypeID					
					)		 
		SELECT @ProposalUserRoleID = ID FROM @Inserted
		
/*
Code is now going to do this		
		IF @RoleID = 3	/*Pricer*/
			/*
			When a New Pricer is assigned
			Proposal.DateAssigned gets updated
			*/			
			BEGIN
				UPDATE dbo.Proposal
				SET DateAssigned = @UpdateDate
				WHERE ProposalID = @ProposalID
			END
*/					
			

		
	END
	

IF @@ERROR = 0
	SELECT @ProposalUserRoleID as ProposalUserRoleID

GO

/*
    File: \Stored Procedures\insertSystemUserRole.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\insertSystemUserRole.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertSystemUserRole]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertSystemUserRole];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertSystemUserRole]
(
	@UserID int,
	@RoleID int,
	@LineOfBusinessID varchar(100)
)
AS
/******************************************************************************
**		 
**		Name: insertSystemUserRole
**		Desc: Inserts a record into the System User Role table
**			
**		
**
**		Auth: Don Canuso
**		Date: 4/23/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		11/20/14	dcanuso				New Proposal Set Up role - Renamed to 
**										reuse Product Line Viewer to Role XREF
**		1/12/2017	brunworg			BOEJ-1688 Update PTM SPs to not display technical details to the user
**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE @ErrorMessage varchar (500)

IF EXISTS	(SELECT 1 FROM [dbo].[SystemUserRole] WHERE	
					 UserID = @UserID AND
				 	 RoleID = @RoleID
			)
			BEGIN
				SET @ErrorMessage =   'This System User Role already exists.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
ELSE
	BEGIN		
		
		DECLARE @UpdateDT datetime2 = GETDATE(),
				@SystemUserRoleID int
			
		INSERT INTO [dbo].[SystemUserRole]
				   ([UserID]
				   ,[RoleID]
				   ,[UpdateDT])		 
		OUTPUT inserted.SystemUserRoleID INTO @Inserted
			 VALUES
				   (
					@UserID, 
					@RoleID, 
					@UpdateDT
					)		 
		SELECT @SystemUserRoleID = ID FROM @Inserted
		
		
		
		IF @LineOfBusinessID IS NOT NULL AND 
			@RoleID IN 
				(
					SELECT RoleID FROM dbo.RoleLU 
					WHERE Role IN ('Viewer', 'Proposal Setup Administrator')
				)
				
			BEGIN
			/*
			Process LineOfBusiness
			*/
			IF RIGHT(@LineOfBusinessID, 1) <> ','
				SET @LineOfBusinessID = @LineOfBusinessID + ','

			DECLARE @LineOfBusiness TABLE (LineOfBusinessID INT)


			WHILE (SELECT CHARINDEX (',', @LineOfBusinessID) ) > 1
			BEGIN
	
			INSERT INTO @LineOfBusiness
			SELECT LEFT (@LineOfBusinessID, CHARINDEX (',', @LineOfBusinessID) -1)
			SET @LineOfBusinessID = RIGHT (@LineOfBusinessID, LEN (@LineOfBusinessID) - CHARINDEX (',', @LineOfBusinessID) )
			;


			WITH [Target] AS 
				(
					SELECT 
						SystemUserRoleID AS SystemUserRoleID,
						LineOfBusinessID AS LineOfBusinessID
					FROM [dbo].[LineOfBusinessRoleXREF]
					WHERE SystemUserRoleID = @SystemUserRoleID
				)
			MERGE INTO [Target]
			USING	(
					SELECT DISTINCT 
						@SystemUserRoleID AS SystemUserRoleID, 
						LineOfBusinessID AS LineOfBusinessID
					FROM @LineOfBusiness
					)  AS [Source] ON
					[Target].[SystemUserRoleID] = [Source].[SystemUserRoleID] AND
					[Target].[LineOfBusinessID] = [Source].[LineOfBusinessID]
			WHEN NOT MATCHED BY SOURCE 
			THEN 
			DELETE	

			WHEN NOT MATCHED BY TARGET THEN
			INSERT (SystemUserRoleID, LineOfBusinessID)
			VALUES ([Source].[SystemUserRoleID], [Source].[LineOfBusinessID])
			;	

	
END
			
			END
	END

IF @@ERROR = 0
	SELECT @SystemUserRoleID as SystemUserRoleID

GO

/*
    File: \Stored Procedures\rsCentralEstimator.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\rsCentralEstimator.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsCentralEstimator]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsCentralEstimator];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsCentralEstimator]
(
	@CentralEstimator varchar(8000)
)	
AS
SET NOCOUNT ON
/*
SP Used for SSRS Report Header
*/

/*
This is being used for all Lead Estimators (removed central/field roletype)
*/
DECLARE @tblCentralEstimator TABLE (CentralEstimatorID int)

IF @CentralEstimator IS NULL OR @CentralEstimator = 'All'
	BEGIN
		INSERT INTO @tblCentralEstimator
		SELECT  -1
	END
ELSE	
	BEGIN
	IF RIGHT(@CentralEstimator, 1) <> ','
		  SET @CentralEstimator = @CentralEstimator + ','

	WHILE (SELECT CHARINDEX (',', @CentralEstimator) ) > 1
	BEGIN
	      
		  INSERT INTO @tblCentralEstimator
		  SELECT LEFT (@CentralEstimator, CHARINDEX (',', @CentralEstimator) -1)
		  SET @CentralEstimator = RIGHT (@CentralEstimator, LEN (@CentralEstimator) - CHARINDEX (',', @CentralEstimator) )
	      
	END

END

DECLARE @listStr VARCHAR(1000)

DECLARE @Results TABLE (DisplayName varchar(100)) /*USED TO TAKE CARE OF DUPLICATES*/
INSERT INTO @Results
SELECT DISTINCT U.DisplayName
FROM @tblCentralEstimator tFE
	INNER JOIN dbo.ProposalUserRole PUR ON tFE.CentralEstimatorID = PUR.UserID
						INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
					WHERE 
						PUR.RoleID = 3 /*Pricer*/ 


IF EXISTS (SELECT 1 FROM @tblCentralEstimator WHERE CentralEstimatorID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN
	SELECT @listStr = COALESCE(@listStr+',' ,'') + DisplayName
	FROM @Results
END



SELECT
	CASE 
		WHEN @CentralEstimator IS NULL THEN NULL 
		ELSE @listStr
	END

GO

GRANT EXECUTE ON OBJECT::dbo.rsCentralEstimator TO generationReporter;
GO

/*
    File: \Stored Procedures\rsContractType.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\rsContractType.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsContractType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsContractType];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsContractType]
(
	@ContractType varchar (8000)
)	
AS
SET NOCOUNT ON
/*
SP Used for SSRS Report Header
*/


/*
Process ContractType
*/
DECLARE @tblContractType TABLE (ContractTypeID int)
IF @ContractType IS NULL OR @ContractType = 'All'
	BEGIN
		INSERT INTO @tblContractType
		SELECT -1 
	END
ELSE	
	BEGIN
		IF RIGHT(@ContractType, 1) <> ','
	      SET @ContractType = @ContractType + ','
	
		WHILE (SELECT CHARINDEX (',', @ContractType) ) > 1
			BEGIN
			      
				  INSERT INTO @tblContractType
				  SELECT LEFT (@ContractType, CHARINDEX (',', @ContractType) -1)
				  SET @ContractType = RIGHT (@ContractType, LEN (@ContractType) - CHARINDEX (',', @ContractType) )
			      
			END
	END



DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblContractType WHERE ContractTypeID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN
SELECT @listStr = COALESCE(@listStr+', ' ,'') + C.ContractType
FROM @tblContractType tC
	INNER JOIN dbo.ContractTypeLU C ON tC.ContractTypeID = C.ContractTypeID
END



SELECT @listStr

GO

/*
    File: \Stored Procedures\rsCustomerType.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\rsCustomerType.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsCustomerType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsCustomerType];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsCustomerType]
(
	@CustomerType varchar (8000)
)	
AS
SET NOCOUNT ON
/*
SP Used for SSRS Report Header
*/


/*
Process CustomerType
*/
DECLARE @tblCustomerType TABLE (CustomerTypeID int)
IF @CustomerType IS NULL OR @CustomerType = 'All'
	BEGIN
		INSERT INTO @tblCustomerType
		SELECT -1 
	END
ELSE	
	BEGIN
		IF RIGHT(@CustomerType, 1) <> ','
	      SET @CustomerType = @CustomerType + ','
	
		WHILE (SELECT CHARINDEX (',', @CustomerType) ) > 1
			BEGIN
			      
				  INSERT INTO @tblCustomerType
				  SELECT LEFT (@CustomerType, CHARINDEX (',', @CustomerType) -1)
				  SET @CustomerType = RIGHT (@CustomerType, LEN (@CustomerType) - CHARINDEX (',', @CustomerType) )
			      
			END
	END



DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblCustomerType WHERE CustomerTypeID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN
SELECT @listStr = COALESCE(@listStr+', ' ,'') + C.CustomerType
FROM @tblCustomerType tC
	INNER JOIN dbo.CustomerTypeLU C ON tC.CustomerTypeID = C.CustomerTypeID
END



SELECT @listStr

GO

GRANT EXECUTE ON OBJECT::dbo.rsCustomerType TO generationReporter;
GO

/*
    File: \Stored Procedures\rsLineOfBusiness.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\rsLineOfBusiness.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsLineOfBusiness]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsLineOfBusiness];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsLineOfBusiness]
(
	@LOB varchar (8000)
)	
AS
/******************************************************************************
**		 
**		Name: [rsLineOfBusiness]
**		Desc: SP Used for SSRS Report Header
**			
**
**		Auth: Unknown
**		Date: Unknown
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		unknown		unknown				unknown
**
**		4/13/2017	ranzalon			BOEJ-2096 Update Line Of Business to use correct table (Product Line)
**		6/06/2018	brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
*******************************************************************************/
SET NOCOUNT ON
/*Line of Business*/
DECLARE @tblLineOfBusiness TABLE (LineOfBusinessID int)

IF @LOB IS NULL OR @LOB = 'All'
	BEGIN
		INSERT INTO @tblLineOfBusiness
		SELECT -1 
	END
ELSE	
	BEGIN
		IF RIGHT(@LOB, 1) <> ','
	      SET @LOB = @LOB + ','
	
		WHILE (SELECT CHARINDEX (',', @LOB) ) > 1
			BEGIN
			      
				  INSERT INTO @tblLineOfBusiness
				  SELECT LEFT (@LOB, CHARINDEX (',', @LOB) -1)
				  SET @LOB = RIGHT (@LOB, LEN (@LOB) - CHARINDEX (',', @LOB) )
			      
			END
	END


DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblLineOfBusiness WHERE LineOfBusinessID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN

SELECT @listStr = 
			COALESCE(@listStr+', ' ,'') + 
			LOB.LineOfBusinessName
FROM @tblLineOfBusiness tLOB
	INNER JOIN dbo.LineOfBusinessLU LOB ON LOB.LineOfBusinessID = tLOB.LineOfBusinessID
END


SELECT
	CASE 
		WHEN @LOB IS NULL THEN NULL 
		ELSE @listStr
	END

GO

GRANT EXECUTE ON OBJECT::dbo.rsLineOfBusiness TO generationReporter;
GO

/*
    File: \Stored Procedures\rsProgramArea.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\rsProgramArea.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsProgramArea]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsProgramArea];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsProgramArea]
(
	@PA varchar (8000)
)	
AS
/******************************************************************************
**		 
**		Name: [rsProgramArea]
**		Desc: SP Used for SSRS Report Header
**			
**
**		Auth: brunworg
**		Date: 6/08/2018 (copied from rsLineOfBusiness)
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON
/*Program Area*/
DECLARE @tblProgramArea TABLE (ProgramAreaID int)

IF @PA IS NULL OR @PA = 'All'
	BEGIN
		INSERT INTO @tblProgramArea
		SELECT -1 
	END
ELSE	
	BEGIN
		IF RIGHT(@PA, 1) <> ','
	      SET @PA = @PA + ','
	
		WHILE (SELECT CHARINDEX (',', @PA) ) > 1
			BEGIN
			      
				  INSERT INTO @tblProgramArea
				  SELECT LEFT (@PA, CHARINDEX (',', @PA) -1)
				  SET @PA = RIGHT (@PA, LEN (@PA) - CHARINDEX (',', @PA) )
			      
			END
	END


DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblProgramArea WHERE ProgramAreaID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN

SELECT @listStr = 
			COALESCE(@listStr+', ' ,'') + 
			PA.ProgramAreaName
FROM @tblProgramArea tPA
	INNER JOIN dbo.ProgramAreaLU PA ON PA.ProgramAreaID = tPA.ProgramAreaID
END


SELECT
	CASE 
		WHEN @PA IS NULL THEN NULL 
		ELSE @listStr
	END

GO

GRANT EXECUTE ON OBJECT::dbo.rsProgramArea TO generationReporter;
GO

/*
    File: \Stored Procedures\rsProposalStatus.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\rsProposalStatus.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsProposalStatus]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsProposalStatus];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsProposalStatus]
(
	@ProposalStatus varchar (8000)
)	
AS
SET NOCOUNT ON
/*
SP Used for SSRS Report Header
*/


/*
Process Proposal Status
*/
DECLARE @tblProposalStatus TABLE (ProposalStatusID int)
IF @ProposalStatus IS NULL OR @ProposalStatus = 'All'
	BEGIN
		INSERT INTO @tblProposalStatus
		SELECT -1 
	END
ELSE	
	BEGIN
		IF RIGHT(@ProposalStatus, 1) <> ','
	      SET @ProposalStatus = @ProposalStatus + ','
	
		WHILE (SELECT CHARINDEX (',', @ProposalStatus) ) > 1
			BEGIN
			      
				  INSERT INTO @tblProposalStatus
				  SELECT LEFT (@ProposalStatus, CHARINDEX (',', @ProposalStatus) -1)
				  SET @ProposalStatus = RIGHT (@ProposalStatus, LEN (@ProposalStatus) - CHARINDEX (',', @ProposalStatus) )
			      
			END
	END



DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblProposalStatus WHERE ProposalStatusID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN
SELECT @listStr = COALESCE(@listStr+',' ,'') + PS.ProposalStatus
FROM @tblProposalStatus tPS
	INNER JOIN dbo.ProposalStatusLU PS ON tPS.ProposalStatusID = PS.ProposalStatusID
END



SELECT @listStr

GO

GRANT EXECUTE ON OBJECT::dbo.rsProposalStatus TO generationReporter;
GO

/*
    File: \Stored Procedures\rsSegment.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\rsSegment.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsSegment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsSegment];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsSegment]
(
	@Segment varchar(8000)
)	
AS
/*
Stored Procedure used for the header in SSRS
Specifically Proposal Log Report


Change Log:
8/20/13	dcanuso	Task #21084: Proposal Log: Report Header Segment needs to be short name

*/

SET NOCOUNT ON
/*
SP Used for SSRS Report Header
*/
/*@Segment*/
DECLARE @tblSegment TABLE (SegmentID int)

IF @Segment IS NULL OR @Segment = 'All'
	BEGIN
		INSERT INTO @tblSegment
		SELECT -1 
	END
ELSE	
	BEGIN
		IF RIGHT(@Segment, 1) <> ','
	      SET @Segment = @Segment + ','
	
		WHILE (SELECT CHARINDEX (',', @Segment) ) > 1
			BEGIN
			      
				  INSERT INTO @tblSegment
				  SELECT LEFT (@Segment, CHARINDEX (',', @Segment) -1)
				  SET @Segment = RIGHT (@Segment, LEN (@Segment) - CHARINDEX (',', @Segment) )
			      
			END
	END
	



DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblSegment WHERE SegmentID = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN
SELECT @listStr = COALESCE(@listStr+',' ,'') + S.SegmentShortName
FROM @tblSegment tS
	INNER JOIN dbo.SegmentLU S ON tS.SegmentID = S.SegmentID
END



SELECT
	CASE 
		WHEN @Segment IS NULL THEN NULL 
		ELSE @listStr
	END

GO

/*
    File: \Stored Procedures\rsYear.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\rsYear.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsYear]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsYear];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[rsYear]
(
	@Year varchar (8000)
)	
AS
SET NOCOUNT ON
/*
SP Used for SSRS Report Header
*/

/*Process Year*/
DECLARE @tblYear TABLE ([Year] CHAR(4))
IF @Year IS NULL OR @Year = 'All'
	BEGIN
		INSERT INTO @tblYear
		SELECT -1
	END
ELSE	
	BEGIN
		IF RIGHT(@Year, 1) <> ','
	      SET @Year = @Year + ','
	
		WHILE (SELECT CHARINDEX (',', @Year) ) > 1
			BEGIN
			      
				  INSERT INTO @tblYear
				  SELECT LEFT (@Year, CHARINDEX (',', @Year) -1)
				  SET @Year = RIGHT (@Year, LEN (@Year) - CHARINDEX (',', @Year) )
			      
			END
	END




DECLARE @listStr VARCHAR(1000)


IF EXISTS (SELECT 1 FROM @tblYear WHERE Year = -1)
BEGIN
	SET @listStr = 'All'
END
ELSE
BEGIN
SELECT @listStr = COALESCE(@listStr+',' ,'') + [Year]
FROM @tblYear
END



SELECT @listStr

GO

GRANT EXECUTE ON OBJECT::dbo.rsYear TO generationReporter;
GO

/*
    File: \Stored Procedures\unlockProposal.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\unlockProposal.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[unlockProposal]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[unlockProposal];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[unlockProposal]
(
      @ProposalID [int],
      @UpdateDate [datetime2](7),
      @UnlockOptionID [int]
)
AS
/******************************************************************************
**          
**          Name: [unlockProposal]
**          Desc: unlock Proposal using Proposal Update Date
**                
**
**          Auth: Don Canuso
**          Date: 7/2/13
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ------------------------------
**			6/11/14		dcanuso					Task 29822:Update Submittal Date 
**												based on Proposal Status Change
**			1/12/2017	gbrunwo					BOEJ-1688 Update PTM SPs to not 
**												display technical details to the user
**			5/31/2018	ranzalon				BOJE-3405 - remove TempProposalSubmittalDate
**			9/5/2018	twilson3				BOEJ-3761 Remove id for new Submitted status
**			9/11/2018	twilson3				BOEJ-3818 Change status to In progress if submitted
**          3/3/2022    koovackal               IES-849 Changes to "Revise" button
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

/*
If the proposal is not Active (i.e., Archived, Revision, Deleted), 
then no one should be able to unlock the checklist.
*/
IF EXISTS (
				SELECT ProposalStatusID 
				FROM dbo.Proposal 
				WHERE ProposalID = @ProposalID AND
				ProposalStatusID IN (3,4,5)
				) 
                  BEGIN
                        SET @ErrorMessage =   'The Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' is in a state that can not be unlocked'
                        RAISERROR (
                              @ErrorMessage, -- Message text.
                          11, -- Severity,/*Severity Changed to 11*/
                              1 -- State,
                              )
                        RETURN
                  END

			
IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID) = @UpdateDate
      BEGIN                  
      SET @UpdateDate = GETDATE()

	  /*
		UnlockOptionID
			1.Unlock Pricer
			2. Unlock Peer
			3. Unlock both Pricer and Peer
	  */      
      UPDATE dbo.ProposalChecklistComplete
      SET 
		UpdateDate = @UpdateDate,
		SubmitDate = NULL 
      WHERE 
      ProposalID = @ProposalID AND 
      (
		  (
				ResponseTypeID = 1 /*Pricer*/ AND
				@UnlockOptionID = 1
		  ) OR
			(
			ResponseTypeID = 2 /*Peer*/ AND
			@UnlockOptionID = 2
		  ) OR
		( 
			@UnlockOptionID = 3 AND
			(
				ResponseTypeID = 1 /*Pricer*/ OR 
				ResponseTypeID = 2 /*Peer*/
			) 
		) 
	)



/*
Completed, Pending Certification, Pending Contractual Award states would change to InProgess
*/
UPDATE [dbo].[Proposal] 
    SET  [ProposalStatusID] = 1 /*In Progress*/
     WHERE
          ProposalID = @ProposalID AND
          (ProposalStatusID = 2 /*Completed*/ OR
		   ProposalStatusID = 6 /*Pending Certification*/ OR
		   ProposalStatusID = 9) /*Pending Contractual Award*/


/*Update Date for the Proposal gets updated*/
UPDATE [dbo].[Proposal] 
    SET  [UpdateDate] = @UpdateDate
     WHERE 
          ProposalID = @ProposalID

END
                  
            ELSE
                  BEGIN
                        SET @ErrorMessage =   'The Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
                        RAISERROR (
                              @ErrorMessage, -- Message text.
                          11, -- Severity,/*Severity Changed to 11*/
                              1 -- State,
                              )
                        RETURN
                  END
   
   

   
                        
IF @@ERROR = 0
      SELECT @ProposalID as ProposalID

GO

/*
    File: \Stored Procedures\UpdateDbVersion.sql
*/
PRINT '### Starting file: \Stored Procedures\UpdateDbVersion.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateDbVersion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[UpdateDbVersion];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[UpdateDbVersion](@DbVersion VARCHAR(100), @AppVersion VARCHAR(100)) 
AS
/******************************************************************************
**		 
**		Name: [UpdateDbVersion]
**		Desc: Updates the DB version, if necessary
**
**		Auth: Dusan
**		Date: 11/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**
******************************************************************************/
	IF(NOT EXISTS(SELECT 1 FROM [dbo].[BoeDatabaseVersion] WHERE DBVersion = @DbVersion AND AppVersion = @AppVersion))
	BEGIN
		INSERT INTO [dbo].[BoeDatabaseVersion] (DBVersion, AppVersion, UpdateDate)
		VALUES (@DbVersion, @AppVersion, GETDATE())
	END

GO

/*
    File: \Stored Procedures\updateProposalForecastEmailSent.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\updateProposalForecastEmailSent.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProposalForecastEmailSent]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProposalForecastEmailSent];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateProposalForecastEmailSent]
(
      @ProposalID [int],
      @UpdateDate [datetime2](7)
)
AS
/******************************************************************************
**          
**          Name: [updateProposalForecastEmailSent]
**          Desc: Update Proposal Forecast Email Sent
**                
**          
**
**          Auth: twilson3
**          Date: 3/14/18
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ------------------------------
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID) = @UpdateDate
    BEGIN
                  
        SET @UpdateDate = GETDATE()
                              
        UPDATE [dbo].[Proposal]
            SET  [UpdateDate] = @UpdateDate
                ,[ForecastEmailSent] = 1
                WHERE 
                    ProposalID = @ProposalID
	END              
ELSE
    BEGIN
        SET @ErrorMessage =   'The Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
        RAISERROR (
                @ErrorMessage, -- Message text.
            11, -- Severity,/*Severity Changed to 11*/
                1 -- State,
                )
        RETURN
    END

GO

/*
    File: \Stored Procedures\updateProposalInformation.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\updateProposalInformation.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProposalInformation]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProposalInformation];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateProposalInformation]
(
      @ProposalID [int],
      @UpdateDate [datetime2](7),
      @ProposalStatusID [int],
      @ProposalSubmittalDate [date],
      @ISGSTotalPrice [bigint],
      @PricerChecklistSubmittalDate [date],
      @PeerChecklistSubmittalDate [date],
	  @InformationComments VARCHAR(MAX) = NULL
)
AS
/******************************************************************************
**          
**          Name: [updateProposalInformation]
**          Desc: Manage Proposal Information - Admin Function
**                
**          
**
**          Auth: Don Canuso
**          Date: 7/3/13
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                 Description:
**          --------    --------                -------------------------------
**			8/7/13		dcanuso					Total price now isgs total price
**			6/11/14		dcanuso					Task 29822:Update Submittal Date 
**												based on Proposal Status Change
**			1/12/2017	gbrunwo					BOEJ-1688 Update PTM SPs to not 
**												display technical details to the user
**			5/31/2018	ranzalon				BOEJ-3405 - remove TempProposalSubmittalDate
**			8/10/2020	ranzalon				BOEJ-4649 Manage Proposal Info Comments
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID) = @UpdateDate
      BEGIN
      
      SET @UpdateDate = GETDATE()

	  IF @ProposalStatusID IS NOT NULL
		  BEGIN                  
			  UPDATE [dbo].[Proposal]
					SET  [UpdateDate] = @UpdateDate
						,[ProposalStatusID] = @ProposalStatusID
					 WHERE 
						  ProposalID = @ProposalID
		  END


		IF @ISGSTotalPrice IS NOT NULL OR @ProposalSubmittalDate IS NOT NULL
			BEGIN		
				  UPDATE [dbo].[ProposalChecklist]
						SET  [UpdateDate] = @UpdateDate
							,[ISGSTotalPrice] = IsNull(@ISGSTotalPrice, ISGSTotalPrice)
							/*WI 29822 */ 
							,[ProposalSubmittalDate] =	ISNULL(@ProposalSubmittalDate, [ProposalSubmittalDate])							
						 WHERE 
							  ProposalID = @ProposalID
			END
			
			
		IF @PricerChecklistSubmittalDate IS NOT NULL
			BEGIN
				UPDATE dbo.ProposalChecklistComplete	
					SET	 [UpdateDate] = @UpdateDate
						,[SubmitDate]	= @PricerChecklistSubmittalDate
				WHERE
					ProposalID = @ProposalID AND
					ResponseTypeID = 1 /*Pricer*/
			END


		IF @PeerChecklistSubmittalDate IS NOT NULL
			BEGIN
				UPDATE dbo.ProposalChecklistComplete	
					SET	 [UpdateDate] = @UpdateDate
						,[SubmitDate]	= @PeerChecklistSubmittalDate
				WHERE
					ProposalID = @ProposalID AND
					ResponseTypeID = 2 /*Peer*/
			END

		IF @InformationComments IS NOT NULL
		  BEGIN                  
			  UPDATE [dbo].[Proposal]
					SET  [UpdateDate] = @UpdateDate
						,[InformationComments] = @InformationComments
					 WHERE 
						  ProposalID = @ProposalID
		  END
    
      END
      
ELSE
      BEGIN
            SET @ErrorMessage =   'The Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
            RAISERROR (
                  @ErrorMessage, -- Message text.
              11, -- Severity,/*Severity Changed to 11*/
                  1 -- State,
                  )
            RETURN
      END




            
IF @@ERROR = 0
SELECT @ProposalID as ProposalID

GO

/*
    File: \Stored Procedures\updateProposalPARChecklist.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\updateProposalPARChecklist.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProposalPARChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProposalPARChecklist];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateProposalPARChecklist]
(
	@ProposalID [int],
	@PARChecklist_Response [varchar](MAX),
	@ResponseTypeID [int],
	@Comment [varchar] (max),
	@CompletedByUserID int,
	@IsSubmittal bit
)
AS
/******************************************************************************
**		 
**		Name:	[updateProposalPARChecklist]
**		Desc:	Update Proposal PAR Checklist Responses
**			
**		
**
**		Auth: Don Canuso
**		Date: 5/7/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		11/10/14	dcanuso				WI 30055 Comment column added so added
**										characters to column
**`		03/05/19	Dusan				Added canned responses
*******************************************************************************/
SET NOCOUNT ON 
DECLARE 
	@ErrorMessage varchar (500),
	@SubmitDate datetime2(7),
	@UpdateDate datetime2(7) = GETDATE()
	
IF @IsSubmittal = 1 SET @SubmitDate = @UpdateDate

SET @PARChecklist_Response = REPLACE (@PARChecklist_Response, '~~', '~NULL~')
SET @PARChecklist_Response = REPLACE (@PARChecklist_Response, '~^', '~NULL^')
/*
Process @PARChecklist_Response
*/
IF RIGHT(@PARChecklist_Response, 1) <> '^'
	SET @PARChecklist_Response = @PARChecklist_Response + '^'

DECLARE @Checklist TABLE 
(
	StringToProcess varchar (MAX),
	ChecklistID INT,
	ResponseID int,
	Comment varchar (500) DEFAULT NULL,
	PageNumber varchar (50) DEFAULT NULL,
	CannedResponseId INT NULL
)

DECLARE @NextString varchar(1000), @NextPlaceHolderID int
WHILE (SELECT CHARINDEX ('^', @PARChecklist_Response) ) > 1
BEGIN
	SELECT @NextString = LEFT (@PARChecklist_Response, CHARINDEX ('^', @PARChecklist_Response) -1)

	INSERT INTO @Checklist (StringToProcess) VALUES (@NextString)

	SET @PARChecklist_Response = RIGHT (@PARChecklist_Response, LEN (@PARChecklist_Response) - LEN (@NextString) -1)
	SET @NextString = NULL

END


/*
	There are three columns that need to updated so handling as three sets of statement
	If performance is an issue, dynamic processing could be added.
*/	

UPDATE @Checklist SET StringToProcess = StringToProcess + '~' WHERE RIGHT (StringToProcess, 1) <> '~'
UPDATE @Checklist 
	SET ChecklistID = LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1)

UPDATE @Checklist SET StringToProcess = RIGHT (StringToProcess, LEN (StringToProcess) - CHARINDEX ('~', StringToProcess))
UPDATE @Checklist
	SET ResponseID = LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1)

UPDATE @Checklist SET StringToProcess = RIGHT (StringToProcess, LEN (StringToProcess) - CHARINDEX ('~', StringToProcess))
UPDATE @Checklist
	SET Comment = IsNull(LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1), NULL)

UPDATE @Checklist SET StringToProcess = RIGHT (StringToProcess, LEN (StringToProcess) - CHARINDEX ('~', StringToProcess))
UPDATE @Checklist
	SET PageNumber = IsNull(LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1), NULL)

UPDATE @Checklist SET StringToProcess = RIGHT (StringToProcess, LEN (StringToProcess) - CHARINDEX ('~', StringToProcess))
UPDATE @Checklist
	SET CannedResponseId = TRY_CONVERT(int, IsNull(LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1), NULL))

UPDATE @Checklist
	SET Comment = CASE 
						WHEN Comment = '' THEN NULL 
						WHEN Comment = 'NULL' THEN NULL
						ELSE Comment
					END,
		PageNumber 	 = CASE 
						WHEN PageNumber = '' THEN NULL 
						WHEN PageNumber = 'NULL' THEN NULL
						ELSE PageNumber
					END

UPDATE dbo.ProposalPARChecklistXREF
SET ResponseID = C.ResponseID,
	Comment = C.Comment,
	PageNumber = C.PageNumber,
	CannedResponseId = C.CannedResponseId
FROM  dbo.ProposalPARChecklistXREF X
	INNER JOIN @Checklist C ON X.PARChecklistContentID = C.ChecklistID
WHERE
	X.ProposalID = @ProposalID AND
	X.ResponseTypeID = @ResponseTypeID 

/*Update completed table*/
IF NOT EXISTS (SELECT 1 
				FROM dbo.ProposalChecklistComplete 
				WHERE
					ProposalID = @ProposalID AND
					ResponseTypeID = @ResponseTypeID AND
					ChecklistTypeID = 2	/*Proposal Adequacy Review (Included in this is Proposal Adequacy Artifact)*/
			 )
BEGIN
INSERT INTO [dbo].[ProposalChecklistComplete]
           ([UpdateDate]
           ,[ProposalID]
           ,[CompletedByUserID]
           ,[ResponseTypeID]
           ,[ChecklistTypeID]
           ,[Comment]
           ,[SubmitDate]
           ,[LastSaveDate]
           )
     VALUES
           (GETDATE()
           ,@ProposalID
           ,@CompletedByUserID
           ,@ResponseTypeID
           ,2	/*Proposal Adequacy Review (Included in this is Proposal Adequacy Artifact)*/
           ,@Comment
           ,@SubmitDate
           ,@UpdateDate
           )
END
ELSE
BEGIN
UPDATE [dbo].[ProposalChecklistComplete]
   SET [UpdateDate] = GETDATE()
      ,[CompletedByUserID] = @CompletedByUserID
      ,[Comment] = @Comment
      ,[SubmitDate] = @SubmitDate
      ,[LastSaveDate] = @UpdateDate
   WHERE
		ProposalID = @ProposalID AND
		ResponseTypeID = @ResponseTypeID AND
		ChecklistTypeID = 2	/*Proposal Adequacy Review (Included in this is Proposal Adequacy Artifact)*/
END

GO

/*
    File: \Stored Procedures\updateProposalPPRChecklist.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\updateProposalPPRChecklist.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProposalPPRChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProposalPPRChecklist];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateProposalPPRChecklist]
(
	@ProposalID [int],
	@PPRChecklist_Response [varchar](1000),
	@ResponseTypeID [int],
	@Comment [varchar] (max),
	@CompletedByUserID int,
	@IsSubmittal bit
)
AS
/******************************************************************************
**		 
**		Name:	[updateProposalPPRChecklist]
**		Desc:	Update Proposal PPR Checklist Responses
**			
**		
**
**		Auth: Don Canuso
**		Date: 5/7/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 
DECLARE 
	@ErrorMessage varchar (500),
	@SubmitDate datetime2(7),
	@UpdateDate datetime2(7)
	
SET @UpdateDate = GETDATE()

IF @IsSubmittal = 1 SET @SubmitDate = @UpdateDate

	


/*
Process @PPRChecklist_Response
*/
IF RIGHT(@PPRChecklist_Response, 1) <> ';'
	SET @PPRChecklist_Response = @PPRChecklist_Response + ';'

DECLARE @Checklist TABLE 
(
	ChecklistID INT,
	ResponseID int
)

DECLARE @NextString varchar(1000)
WHILE (SELECT CHARINDEX (';', @PPRChecklist_Response) ) > 1
BEGIN
	
	
	SELECT @NextString = LEFT (@PPRChecklist_Response, CHARINDEX (';', @PPRChecklist_Response) -1)
	
	INSERT INTO @Checklist
	SELECT 
		LEFT (@NextString, CHARINDEX (',', @NextString) -1),
		RIGHT (@NextString, LEN (@NextString) - CHARINDEX (',', @NextString))
	
	SET @PPRChecklist_Response = RIGHT (@PPRChecklist_Response, LEN (@PPRChecklist_Response) - LEN (@NextString) -1)
	SET @NextString = NULL
	/*
	for testing
	SELECT * FROM @Checklist
	SELECT @PPRChecklist_Response
	*/
END



UPDATE dbo.ProposalPPRChecklistXREF
SET ResponseID = C.ResponseID/*,
	Comment = CASE C.ResponseID
				WHEN 4 THEN @Comment
				ELSE NULL
			  END*/
FROM  dbo.ProposalPPRChecklistXREF X
	INNER JOIN @Checklist C ON X.PPRChecklistContentID = C.ChecklistID
WHERE
X.ProposalID = @ProposalID AND
X.ResponseTypeID = @ResponseTypeID AND
C.ResponseID <> 4 /*DO NOT UPDATE COMMENTS*/


/*
UPDATE dbo.ProposalPPRChecklistXREF
SET Comment = @Comment
FROM  dbo.ProposalPPRChecklistXREF X
	INNER JOIN @Checklist C ON X.PPRChecklistContentID = C.ChecklistID
WHERE
X.ProposalID = @ProposalID AND
X.ResponseTypeID = @ResponseTypeID AND
C.ResponseID = 4 /*UPDATE COMMENTS*/
*/



/*Update completed table*/
IF NOT EXISTS (SELECT 1 
				FROM dbo.ProposalChecklistComplete 
				WHERE
					ProposalID = @ProposalID AND
					ResponseTypeID = @ResponseTypeID AND
					ChecklistTypeID = 1 /*Proposal Pricing Review*/
			 )
BEGIN
INSERT INTO [dbo].[ProposalChecklistComplete]
           ([UpdateDate]
           ,[ProposalID]
           ,[CompletedByUserID]
           ,[ResponseTypeID]
           ,[ChecklistTypeID]
           ,[Comment]
           ,[SubmitDate]
           ,[LastSaveDate]
           )
     VALUES
           (
			@UpdateDate
           ,@ProposalID
           ,@CompletedByUserID
           ,@ResponseTypeID
           ,1 /*Proposal Pricing Review*/
           ,@Comment
           ,@SubmitDate
           ,@UpdateDate
           )
END
ELSE
BEGIN
UPDATE [dbo].[ProposalChecklistComplete]
   SET [UpdateDate] = @UpdateDate
      ,[CompletedByUserID] = @CompletedByUserID
      ,[Comment] = @Comment
      ,[SubmitDate] = @SubmitDate
      ,[LastSaveDate] = @UpdateDate
   WHERE
		ProposalID = @ProposalID AND
		ResponseTypeID = @ResponseTypeID AND
		ChecklistTypeID = 1 /*Proposal Pricing Review*/
		
IF @IsSubmittal = 1
	BEGIN			
		UPDATE [dbo].[Proposal]
			SET [UpdateDate] = @UpdateDate
		WHERE
			ProposalID = @ProposalID 
	END
	
END

GO

/*
    File: \Stored Procedures\updateProposalStatus.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\updateProposalStatus.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProposalStatus]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProposalStatus];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateProposalStatus]
(
      @ProposalID [int],
      @UpdateDate [datetime2](7),
      @ProposalStatusID [int]
)
AS
/******************************************************************************
**          
**          Name: [updateProposalStatus]
**          Desc: Update Proposal Status
**                
**          
**
**          Auth: Don Canuso
**          Date: 6/14/13
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ------------------------------
**			6/11/14		dcanuso					Task 29822:Update Submittal Date 
**												based on Proposal Status Change
**			1/12/2017	gbrunwo					BOEJ-1688 Update PTM SPs to not 
**												display technical details to the user
**			5/31/2018	ranzalon				BOEJ-3405 - remove TempProposalSubmittalDate
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

            IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID) = @UpdateDate
                  BEGIN
                  
                  SET @UpdateDate = GETDATE()
                              
                  UPDATE [dbo].[Proposal]
                        SET  [UpdateDate] = @UpdateDate
                          ,[ProposalStatusID] = @ProposalStatusID
                         WHERE 
                              ProposalID = @ProposalID

                  END
                  
            ELSE
                  BEGIN
                        SET @ErrorMessage =   'The Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
                        RAISERROR (
                              @ErrorMessage, -- Message text.
                          11, -- Severity,/*Severity Changed to 11*/
                              1 -- State,
                              )
                        RETURN
                  END
   
   

   
                        
IF @@ERROR = 0
      SELECT @ProposalID as ProposalID

GO

/*
    File: \Stored Procedures\upsertAttachment.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertAttachment.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertAttachment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertAttachment];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertAttachment]
(
	  @AttachmentID [int],
	  @UpdateDate [datetime2](7),
	  @Name [varchar](100),
	  @Contents [varbinary](max),
	  @UploadedBy  [varchar] (1000),
	  @AttachmentType [int],
	  @ProposalID [int],
	  @IsRevisionReference [bit]
)
AS
/******************************************************************************
**          
**          Name: [upsertAttachment]
**          Desc: Insert/Update Attachment
**          
**
**          Auth: twilson3
**          Date: 9/7/2017
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:         Description:
**			--------    --------        ---------------------------------------
**          7/1/2020	Dusan			BOEJ-4638 Post submittal attachments working with Revisioned Proposal
******************************************************************************/
	SET NOCOUNT ON 

	IF @AttachmentID < 0 -- Inserting a new attachment
		BEGIN
			DECLARE @Inserted AS Table (ID int)
			INSERT INTO [dbo].[Attachment] (UpdateDate, Name, Contents, UploadedBy)
				OUTPUT inserted.ID INTO @Inserted
				VALUES (GETDATE(), @Name, @Contents, @UploadedBy)
			SELECT @AttachmentID = ID FROM @Inserted
		END
	ELSE IF @IsRevisionReference = 0 -- updating an existing attachment
		BEGIN
			IF (SELECT UpdateDate FROM Attachment WHERE ID = @AttachmentID) = @UpdateDate
				UPDATE Attachment
					SET UpdateDate = GETDATE(), Name = @Name, Contents = @Contents, UploadedBy = @UploadedBy
					WHERE ID = @AttachmentID
			ELSE
				BEGIN
					DECLARE @ErrorMessage varchar (500)
					SET @ErrorMessage = 'The Attachment with ID ' + CAST(@AttachmentID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (@ErrorMessage, 11, 1)
					RETURN
				END
		END

	DELETE FROM ProposalsAttachments WHERE ProposalId = @ProposalID AND AttachmentId = @AttachmentID
	INSERT INTO ProposalsAttachments (ProposalId, AttachmentId, IsRevisionReference, AttachmentType) 
		VALUES (@ProposalID, @AttachmentID, @IsRevisionReference, @AttachmentType)

	IF @@ERROR = 0
		SELECT @AttachmentID as AttachmentID

GO

/*
    File: \Stored Procedures\upsertContractType.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertContractType.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertContractType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertContractType];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertContractType]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT,
	/*Comma separated list of Contract Type Group IDs with no comma at the end*/
	@GroupIds   VARCHAR(1000)
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertContractType]
	**		Desc:	Insert/Update Contract Type LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/7/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)

			INSERT INTO [dbo].[ContractTypeLU]
						(
							ContractType,
							IsActive
						)
				OUTPUT inserted.ContractTypeId INTO @Inserted
				VALUES
						(
							@Text,
							@IsActive
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			UPDATE [dbo].[ContractTypeLU]
			   SET 
					ContractType = @Text,
					IsActive = @IsActive
				WHERE 
					ContractTypeId = @Id
		END

	-- Update the XREFs to Groups
	DELETE FROM dbo.ContractTypeGroupXREF
		WHERE ContractTypeId = @Id

	INSERT INTO dbo.ContractTypeGroupXREF ([ContractTypeGroupID], [ContractTypeID]) 
			SELECT Item, @Id FROM [SplitString] (@GroupIds, ',', DEFAULT)

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertContractTypeGroup.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertContractTypeGroup.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertContractTypeGroup]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertContractTypeGroup];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertContractTypeGroup]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertContractTypeGroup]
	**		Desc:	Insert/Update Contract Type Group LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/7/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)

			INSERT INTO [dbo].[ContractTypeGroupLU]
						(
							ContractTypeGroup,
							IsActive
						)
				OUTPUT inserted.ContractTypeGroupId INTO @Inserted
				VALUES
						(
							@Text,
							@IsActive
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			UPDATE [dbo].[ContractTypeGroupLU]
			   SET 
					ContractTypeGroup = @Text,
					IsActive = @IsActive
				WHERE 
					ContractTypeGroupId = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertgenTRACUser.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertgenTRACUser.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertgenTRACUser]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertgenTRACUser];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertgenTRACUser]
(
@UserID int,
@UpdateDT datetime2,
@NTID varchar(1000),
@DisplayName varchar(256),
@EmailAddress varchar(50),
@PhoneNumber varchar(20),
@FirstName varchar(25),
@LastName varchar(25),
@IsGroup bit,
@IsUsPerson bit,
@IsSubcontractor bit
)
AS
/*****************************************************************************
**		 
**		Name: upsertgenTRACUser
**		Desc: Insert/Update genTRAC User Information
**			
**		
**
**		Auth: Don Canuso
**		Date: 4/3/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not 
**										display technical details to the user
**		3/7/2017	Joe					BOEJ-1818 Increase size of the user's display name field
**		11/29/17	pattoncr			Remove domain.
**		6/6/19		Dusan				Added IsUsPerson and IsSubcontractor
**		11/20/19	ranzalon			BOEJ-4400 - fix update for sub/us
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Inserted AS Table (ID int)
DECLARE @ErrorMessage varchar (500)

IF @UserID < 0 
	IF EXISTS (SELECT 1 FROM dbo.genTRACUser WHERE NTID = @NTID)
				BEGIN

				SET @ErrorMessage =   'A user with this NTID already exists'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
				END
	ELSE
		BEGIN

			SET @UpdateDT = GETDATE()

		INSERT INTO [dbo].[genTRACUser]
           ([UpdateDT]
           ,[NTID]
           ,[DisplayName]
           ,[EmailAddress]
           ,[PhoneNumber]
           ,[FirstName]
           ,[LastName]
           ,[IsGroup]
		   ,[IsUsPerson]
		   ,[IsSubcontractor]
           )
		 OUTPUT inserted.UserID INTO @Inserted
         VALUES
           (@UpdateDT
           ,@NTID
           ,@DisplayName
           ,@EmailAddress
           ,@PhoneNumber
           ,@FirstName
           ,@LastName
           ,ISNULL(@IsGroup, 0)
		   ,@IsUsPerson
		   ,@IsSubcontractor
           )
           
		 SELECT @UserID = ID FROM @Inserted
									
		 END
ELSE
	BEGIN
	IF (SELECT UpdateDT FROM dbo.genTRACUser WHERE UserID = @UserID) = @UpdateDT
		BEGIN 
			IF EXISTS	(SELECT 1 
							FROM dbo.genTRACUser 
							WHERE	NTID = @NTID AND 
								UserID <> @UserID
						)
				BEGIN
					SET @ErrorMessage =   'A user with this NTID already exists'
					RAISERROR (
						@ErrorMessage, -- Message text.
						11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
					RETURN
				END
			ELSE
				BEGIN
				
					SET @UpdateDT = GETDATE()
					
					UPDATE [dbo].[genTRACUser]
					   SET [UpdateDT] = @UpdateDT
						  ,[NTID] = @NTID
						  ,[DisplayName] = @DisplayName
						  ,[EmailAddress] = @EmailAddress
						  ,[PhoneNumber] = @PhoneNumber
						  ,[FirstName] = @FirstName
						  ,[LastName] = @LastName
						  ,[IsGroup] = ISNULL(@IsGroup, 0)
						  ,[IsUsPerson] = @IsUsPerson
						  ,[IsSubcontractor] = @IsSubcontractor
					WHERE 
						[UserID] = @UserID
				END
		END		
	ELSE
			BEGIN
				SET @ErrorMessage =   'The User with ID ' + CAST(@UserID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
           			
	END


IF @@ERROR = 0
	SELECT @UserID as UserID

GO

/*
    File: \Stored Procedures\upsertLOB.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertLOB.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertLOB]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertLOB];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertLOB]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertLOB]
	**		Desc:	Insert/Update LOB LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/7/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			----------------------------------------
	**		5/21/2018	twilson3			BOEJ-3363 Remove Long Text
	**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)

			INSERT INTO [dbo].[LineOfBusinessLU]
						(
							LineOfBusinessName,
							IsActive
						)
				OUTPUT inserted.LineOfBusinessID INTO @Inserted
				VALUES
						(
							@Text,
							@IsActive
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			UPDATE [dbo].[LineOfBusinessLU]
			   SET 
					LineOfBusinessName = @Text,
					IsActive = @IsActive
				WHERE 
					LineOfBusinessID = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertProgramArea.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertProgramArea.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProgramArea]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProgramArea];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProgramArea]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT,
	@ParentId	INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertProgramArea]
	**		Desc:	Insert/Update Program Area LU values 
	**			
	**		
	**
	**		Auth: Timothy I. Wilson
	**		Date: 5/7/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			----------------------------------------
	**		5/21/2018	twilson3			BOEJ-3363 Remove Long Text
	**		6/06/18		brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)

			INSERT INTO [dbo].[ProgramAreaLU]
						(
							[ProgramAreaName],
							IsActive,
							[LineOfBusinessID]
						)
				OUTPUT inserted.[ProgramAreaID] INTO @Inserted
				VALUES
						(
							@Text,
							@IsActive,
							@ParentId
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			UPDATE [dbo].[ProgramAreaLU]
			   SET 
					[ProgramAreaName] = @Text,
					IsActive = @IsActive,
					[LineOfBusinessID] = @ParentId
				WHERE 
					[ProgramAreaID] = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertProposal.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertProposal.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposal]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposal];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[upsertProposal]
(
	  @ProposalID [int],
	  @UpdateDate [datetime2](7),
	  @ProposalTitle [varchar](100),
	  @OTISOpportunityID [varchar](25),
	  @ProposalStatusID [int],
	  @ProposalTypeID [int],
	  @IsIWTA [bit],
	  @ProgramName [varchar](50),
	  @Customer [varchar](50),
	  @CustomerTypeID [int],
	  @ISGSRoleID [int],
	  @RequestTypeID [int],
	  @RFPNumber [varchar](40) = NULL,
	  @LineOfBusinessID [int],
	  @ProgramAreaID [int],
	  @PricingToolID [int],
	  @BOEToolID [int],
	  @AnticipatedDeliveryDate [date],
	  @ProposalCostElementXREF [varchar] (50),
	  @EstimatedProposalValue [bigint],
	  @ProposalContractTypeXREF [varchar] (50),
	  @DateAssigned  [bit],
	  @CreatedByUserID [int],
	  @RFPIssuedDate [date],
	  @RFPReceivedDate [date],	  
	  @Comments  [varchar] (2500),
	  @ContractTypeGroupID [int],
	  @IsScheduleProposal [bit],
	  @ProposalLocationID [int],
	  @ProposalLocationName [varchar] (50),
	  @BOEToolName [varchar] (50),
	  @PricingToolName [varchar] (50),
	  @ProposalChecklistTypeID [int],
	  @ChangeChecklistFlag [bit],
	  @ProgramProposalStatusID [int],
	  @ProposalClassID int,
	  @WorkflowStatus int,
	  @WorkflowStatusLastUpdated datetime2 = NULL,
	  @LeadEstimatorSignedDT datetime2 = NULL,
	  @LeadEstimatorSignComment varchar (1000) = NULL,
	  @CoverSheetApproverSignedDT datetime2 = NULL,
	  @CoverSheetApproverSignComment varchar (1000) = NULL,
	  @PricingVerifierSignedDT datetime2 = NULL,
	  @PricingVerifierSignComment varchar (1000) = NULL,
	  @IndependentReviewerSignedDT datetime2 = NULL,
	  @IndependentReviewerSignComment varchar (1000) = NULL,
	  @LOBEstimatingLeadSignedDT datetime2 = NULL,
	  @LOBEstimatingLeadSignComment varchar (1000) = NULL,
	  @ApprovalEmailText varchar (1000) = NULL,
	  @RevisedSubmittalDate datetime2 = NULL,
	  @CCPDRequired [bit] = NULL,
	  @CostVolumeClassified [bit] = NULL,
	  @DocumentId int = NULL,
	  @ForecastedTrackingID varchar (13) = NULL,
	  @TrackingID varchar (13) = NULL,
	  @IsForecast [bit],
	  @AgreementDate datetime2 = NULL,
	  @CertificationDate datetime2 = NULL,
	  @CutOffDateUtilization int = NULL,
	  @CertificationTimelineCompleted datetime2 = NULL,
      @CertificationLastEmailed datetime2 = NULL,
	  @NoBidDate datetime2 = NULL,
	  @IsRevision bit,
	  @RevisionOfId int,
	  @ReasonCertificationNotRequired INT = 1,
	  @OtherReasonComment VARCHAR(1000) = NULL,
	  @SetupComments VARCHAR(MAX) = NULL,
	  @ModExecutedLastEmailed datetime2 = NULL,
	  @ProposalCompletedDate datetime2 = NULL,
	  @ContractActionType int = NULL,
	  @ContractActionTypeOtherText VARCHAR(100) = NULL,
	  @CostVolumeToolID int,
	  @CostVolumeToolName VARCHAR(50),
	  @AdditionalClassification BIT,
	  @ReasonCcopdNo INT,
	  @ReasonCcopdNoOther VARCHAR(100),
	  @IsSupportDefinitizingUCA BIT
)
AS
/******************************************************************************
**          
**          Name: [upsertProposal]
**          Desc: Insert/Update Proposal - Basic/General Information
**                
**          
**
**          Auth: Don Canuso
**          Date: 4/3/2013
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ---------------------------------------
**			1/ 7/17		Dusan					Removing Revisions & LMIS
**			1/12/2017	gbrunwo					BOEJ-1688 Update PTM SPs to not 
**												display technical details to the user
**			02/02/2017	tglick					added new field [Revised Submittal Date]
**			3/20/2017	twilson3				BOEJ-1957 Move CCPD from Checklist to Proposal
**			1/24/2017	twilson3				BOEJ-2808 Add RDSB link into PTM
**			3/06/2018	brunworg				BOEJ-3124 Add Forecasted Tracking Number into PTM
**			3/9/2018	twilson3				BOEJ-3128 Integration of Forecasted Tracking ID
**			3/14/2018	twilson3				BOEJ-3118 Email notification for Forecasted Proposal
**			6/06/2018	brunworg				BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
**			8/29/2018	twilson3				BOEJ-3756 Post Proposal redo
**			9/27/2018	ranzalon				BOEJ-3739 Classified Cost Volume
**			3/31/2020	ranzalon				BOEJ-4531 No Bid Date
**			6/17/2020	ranzalon				BOEJ-4535 IsRevision
**			6/23/2020	Dusan					BOEJ-4626 Add Certification Not Required
**			6/23/2020	ranzalon				BOEJ-4669 No new tracking number when IsRevision 
**			7/2/2020	ranzalon				BOEJ-4687 Link Revisions to Revised Proposal
**			7/31/2020	ranzalon				BOEJ-4648 Proposal Setup Comments
**			3/10/2022   jquijano                IES-854 Add new email (Mod)
**			4/28/2022	jquijano				IES-1067 Show Certification TimelineCompleted Date
**			7/19/2022	ranzalon				IES-1504 - Contract Action Type
**			10/12/2022	ranzalon				IES-1933 - Cost Volume Tool
**			7/9/23		Dusan					PROPH-1563 - Added an Additional Classification Column
**			7/14/24		Dusan					PROPH-1559: Added reason for CCOPD = No
**			8/19/24		Dusan					PROPH-2080: Added IsSupportDefinitizingUCA field
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)
/*
Process Contract Types
*/
IF RIGHT(@ProposalContractTypeXREF, 1) <> ','
	  SET @ProposalContractTypeXREF = @ProposalContractTypeXREF + ','

DECLARE @ContractType TABLE (ContractTypeID INT)

WHILE (SELECT CHARINDEX (',', @ProposalContractTypeXREF) ) > 1
BEGIN
	  
	INSERT INTO @ContractType
	SELECT LEFT (@ProposalContractTypeXREF, CHARINDEX (',', @ProposalContractTypeXREF) -1)
	SET @ProposalContractTypeXREF = RIGHT (@ProposalContractTypeXREF, LEN (@ProposalContractTypeXREF) - CHARINDEX (',', @ProposalContractTypeXREF) )
	  
END
/*
Process Cost Element
*/
IF RIGHT(@ProposalCostElementXREF, 1) <> ','
	  SET @ProposalCostElementXREF = @ProposalCostElementXREF + ','

DECLARE @CostElement TABLE (CostElementID INT)

WHILE (SELECT CHARINDEX (',', @ProposalCostElementXREF) ) > 1
BEGIN
	  
	INSERT INTO @CostElement
	SELECT LEFT (@ProposalCostElementXREF, CHARINDEX (',', @ProposalCostElementXREF) -1)
	SET @ProposalCostElementXREF = RIGHT (@ProposalCostElementXREF, LEN (@ProposalCostElementXREF) - CHARINDEX (',', @ProposalCostElementXREF) )
	  
END

/* Process @IsForecast */
DECLARE @ProposalTrackingGeneratorID int
DECLARE @GeneratedTrackingID bit
SELECT @GeneratedTrackingID = 0
IF @IsForecast = 1
	BEGIN
		SET @TrackingID = NULL
	END
ELSE
	BEGIN
	/* Generate new Tracking ID if this is a new Non-Forecast proposal or editing a Non-Forecast proposal that used to be Forecast */
		IF (@ProposalID < 0 AND @IsRevision = 0) OR @TrackingID = '' OR @TrackingID is NULL
			BEGIN
				SELECT TOP 1 
						@TrackingID = CAST(ProposalYear AS varchar(4)) + '-' + CASE LEN(ProposalNumber) 
																								WHEN 1 THEN '0000' + CAST(ProposalNumber AS varchar(5))
																								WHEN 2 THEN '000' + CAST(ProposalNumber AS varchar(5))
																								WHEN 3 THEN '00' + CAST(ProposalNumber AS varchar(5))
																								WHEN 4 THEN '0' + CAST(ProposalNumber AS varchar(5))
																								WHEN 5 THEN CAST(ProposalNumber AS varchar(5))
																						END ,
						@ProposalTrackingGeneratorID = ProposalTrackingGeneratorID
				FROM dbo.ProposalTrackingGenerator
				WHERE ProposalYear = YEAR (GetDate()) AND UsedByProposalID IS NULL
			
			
				/*Removing first 2 characters in new proposals*/
				SET @TrackingID =	RIGHT(@TrackingID, (LEN(@TrackingID)-2))
				SET @GeneratedTrackingID = 1
			END
	END

/*
<ProposalID> +� � � + <Proposal Title>

Proposal ID is (<YYYY>-<5-digit sequence starting at 00001><Rev #) 
2013-10001Rnn
*/
IF @ProposalID  < 0  /*Insert Record*/
	BEGIN
		DECLARE @Inserted AS Table (ID int)
		SET @UpdateDate = GETDATE()

	INSERT INTO [dbo].[Proposal]
		([UpdateDate]
		,[ProposalTrackingID]
		,[ProposalTitle]
		,[OTISOpportunityID]
		,[ProposalStatusID]
		,[DateCreated]/*Current Date when created*/
		,[DateAssigned]/*Current Date when created*/
		,[ProposalTypeID]
		,[IsIWTA]
		,[ProgramName]
		,[Customer]
		,[CustomerTypeID]
		,[ISGSRoleID]
		,[RequestTypeID]
		,[RFPNumber]
		,[LineOfBusinessID]
		,[ProgramAreaID]
		,[PricingToolID]
		,[BOEToolID]
		,[AnticipatedDeliveryDate]
		,[EstimatedProposalValue]  
		,[IsActive]
		,[CreatedByUserID]
		,[RFPIssuedDate]		   
		,[RFPReceivedDate]		   
		,[Comments]
		,[ContractTypeGroupID]
		,[IsScheduleProposal]
		,[ProposalLocationID]
		,[ProposalLocationName]
		,[BOEToolName]
		,[PricingToolName]
		,[ProgramProposalStatusID]
		,[ProposalClassID]
		,[WorkflowStatus]
		,[WorkflowStatusLastUpdated]
		,[LeadEstimatorSignedDT]
		,[LeadEstimatorSignComment]
		,[CoverSheetApproverSignedDT]
		,[CoverSheetApproverSignComment]
		,[PricingVerifierSignedDT]
		,[PricingVerifierSignComment]
		,[IndependentReviewerSignedDT]
		,[IndependentReviewerSignComment]
		,[LOBEstimatingLeadSignedDT]
		,[LOBEstimatingLeadSignComment]
		,[ApprovalEmailText]
		,[RevisedSubmittalDate]
		,[CCPDRequired]
		,[CostVolumeClassified]
		,[DocumentId]
		,[ForecastedTrackingID]
		,[AgreementDate]
		,[CertificationDate]
		,[CutOffDateUtilization]
		,[CertificationTimelineCompleted]
		,[CertificationLastEmailed]
		,[NoBidDate]
		,[RevisionOfId]
		,[ReasonCertificationNotRequired]
		,[OtherReasonComment]
		,[SetupComments]
		,[ModExecutedLastEmailed]
		,[ProposalCompletedDate]
		,[ContractActionType]
		,[ContractActionTypeOtherText]
		,[CostVolumeToolID]
		,[CostVolumeToolName]
		,[AdditionalClassification]
		,ReasonCcopdNo
		,ReasonCcopdNoOther
		,IsSupportDefinitizingUCA
		)
	OUTPUT inserted.ProposalID INTO @Inserted
	VALUES
		(@UpdateDate
		,@TrackingID
		,@ProposalTitle
		,@OTISOpportunityID
		,1 /*InProgress On Creation*/
		,@UpdateDate/*Current Date when created*/
		,@UpdateDate/*Current Date when created*/
		,@ProposalTypeID
		,@IsIWTA
		,@ProgramName
		,@Customer
		,@CustomerTypeID
		,@ISGSRoleID
		,@RequestTypeID
		,@RFPNumber
		,@LineOfBusinessID
		,@ProgramAreaID
		,@PricingToolID
		,@BOEToolID
		,@AnticipatedDeliveryDate
		,@EstimatedProposalValue
		,1 /*Active On Insert*/
		,@CreatedByUserID
		,@RFPIssuedDate
		,@RFPReceivedDate
		,@Comments
		,@ContractTypeGroupID
		,@IsScheduleProposal
		,@ProposalLocationID
		,@ProposalLocationName
		,@BOEToolName
		,@PricingToolName
		,@ProgramProposalStatusID
		,@ProposalClassID
		,@WorkflowStatus
		,@WorkflowStatusLastUpdated
		,@LeadEstimatorSignedDT
		,@LeadEstimatorSignComment
		,@CoverSheetApproverSignedDT
		,@CoverSheetApproverSignComment
		,@PricingVerifierSignedDT
		,@PricingVerifierSignComment
		,@IndependentReviewerSignedDT
		,@IndependentReviewerSignComment
		,@LOBEstimatingLeadSignedDT
		,@LOBEstimatingLeadSignComment
		,@ApprovalEmailText
		,@RevisedSubmittalDate
		,@CCPDRequired
		,@CostVolumeClassified
		,@DocumentId
		,@ForecastedTrackingID
		,@AgreementDate
		,@CertificationDate
		,@CutOffDateUtilization
		,@CertificationTimelineCompleted
		,@CertificationLastEmailed
		,@NoBidDate
		,@RevisionOfId
		,@ReasonCertificationNotRequired
		,@OtherReasonComment
		,@SetupComments
		,@ModExecutedLastEmailed
		,@ProposalCompletedDate
		,@ContractActionType
		,@ContractActionTypeOtherText
		,@CostVolumeToolID
		,@CostVolumeToolName
		,@AdditionalClassification
		,@ReasonCcopdNo
		,@ReasonCcopdNoOther
		,@IsSupportDefinitizingUCA
		)

		SELECT @ProposalID = ID FROM @Inserted
	  
		IF @GeneratedTrackingID = 1
		BEGIN
			UPDATE dbo.ProposalTrackingGenerator
			SET UsedByProposalID = @ProposalID  
			WHERE ProposalTrackingGeneratorID = @ProposalTrackingGeneratorID 
		END
  
		INSERT INTO [dbo].[ProposalContractTypeXREF]
			([ProposalID]
			,[ContractTypeID])
		SELECT DISTINCT @ProposalID, ContractTypeID           
		FROM @ContractType


		INSERT INTO [dbo].[ProposalCostElementXREF]
			([ProposalID]
			,[CostElementID])
		SELECT DISTINCT @ProposalID, CostElementID           
		FROM @CostElement

		/*CALL STORED PROCEDURE THAT MANAGES CHECKLIST*/
		EXECUTE [dbo].[upsertProposalChecklistTemplate] @ProposalID, @ProposalChecklistTypeID, @ChangeChecklistFlag
	END
ELSE
	/*Update*/
	BEGIN
		IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID) = @UpdateDate
			BEGIN
				SET @UpdateDate = GETDATE()
							  
				UPDATE [dbo].[Proposal]
					SET  [UpdateDate] = @UpdateDate
							,[ProposalTitle] = @ProposalTitle
							,[OTISOpportunityID] = @OTISOpportunityID
						,[ProposalStatusID] = @ProposalStatusID
						,[ProposalTypeID] = @ProposalTypeID
						,[IsIWTA] = @IsIWTA
						,[ProgramName] = @ProgramName
						,[Customer] = @Customer
						,[CustomerTypeID] = @CustomerTypeID
						,[ISGSRoleID] = @ISGSRoleID
						,[RequestTypeID] = @RequestTypeID
						,[RFPNumber] = @RFPNumber
						,[LineOfBusinessID] = @LineOfBusinessID
						,[ProgramAreaID] = @ProgramAreaID
						,[PricingToolID] = @PricingToolID
						,[BOEToolID] = @BOEToolID
						,[AnticipatedDeliveryDate] = @AnticipatedDeliveryDate
						,[EstimatedProposalValue] = @EstimatedProposalValue
						,[DateAssigned] = 
										CASE WHEN @DateAssigned = 1 THEN @UpdateDate
										ELSE [DateAssigned]
										END
						--,[CreatedByUserID] = @CreatedByUserID
						,[RFPIssuedDate] = @RFPIssuedDate
						,[RFPReceivedDate] = @RFPReceivedDate		   
						,[Comments] = @Comments
						,[ContractTypeGroupID] = @ContractTypeGroupID
						,[IsScheduleProposal] = @IsScheduleProposal
						,[ProposalLocationID] = @ProposalLocationID
						,[ProposalLocationName] = @ProposalLocationName
						,[BOEToolName] = @BOEToolName
						,[PricingToolName] = @PricingToolName
						,[ProgramProposalStatusID] = @ProgramProposalStatusID
						,[ProposalClassID] = @ProposalClassID
						,[WorkflowStatus] = @WorkflowStatus
						,[WorkflowStatusLastUpdated] = @WorkflowStatusLastUpdated
						,[LeadEstimatorSignedDT] = @LeadEstimatorSignedDT
						,[LeadEstimatorSignComment] = @LeadEstimatorSignComment
						,[CoverSheetApproverSignedDT] = @CoverSheetApproverSignedDT
						,[CoverSheetApproverSignComment] = @CoverSheetApproverSignComment
						,[PricingVerifierSignedDT] = @PricingVerifierSignedDT
						,[PricingVerifierSignComment] = @PricingVerifierSignComment
						,[IndependentReviewerSignedDT] = @IndependentReviewerSignedDT
						,[IndependentReviewerSignComment] = @IndependentReviewerSignComment
						,[LOBEstimatingLeadSignedDT] = @LOBEstimatingLeadSignedDT
						,[LOBEstimatingLeadSignComment] = @LOBEstimatingLeadSignComment
						,[ApprovalEmailText] = @ApprovalEmailText
						,[RevisedSubmittalDate] = @RevisedSubmittalDate
						,[CCPDRequired] = @CCPDRequired
						,[CostVolumeClassified] = @CostVolumeClassified
						,[DocumentId] = @DocumentId
						,[ForecastedTrackingId] = @ForecastedTrackingId
						,[ProposalTrackingID] = @TrackingID
						,[ForecastEmailSent] = 0
						,[AgreementDate] = @AgreementDate
						,[CertificationDate] = @CertificationDate
						,[CutOffDateUtilization] = @CutOffDateUtilization
						,[CertificationTimelineCompleted] = @CertificationTimelineCompleted
						,[CertificationLastEmailed] = @CertificationLastEmailed
						,[NoBidDate] = @NoBidDate
						,[RevisionOfId] = @RevisionOfId
						,[ReasonCertificationNotRequired] = @ReasonCertificationNotRequired
						,[OtherReasonComment] = @OtherReasonComment
						,[SetupComments] = @SetupComments
						,[ModExecutedLastEmailed] = @ModExecutedLastEmailed
						,[ProposalCompletedDate] = @ProposalCompletedDate
						,[ContractActionType] = @ContractActionType
						,[ContractActionTypeOtherText] = @ContractActionTypeOtherText
						,[CostVolumeToolID] = @CostVolumeToolID
						,[CostVolumeToolName] = @CostVolumeToolName
						,[AdditionalClassification] = @AdditionalClassification
						,ReasonCcopdNo = @ReasonCcopdNo
						,ReasonCcopdNoOther = @ReasonCcopdNoOther
						,IsSupportDefinitizingUCA = @IsSupportDefinitizingUCA
						WHERE 
							ProposalID = @ProposalID;

				WITH [Target] AS 
					(
							SELECT 
								ProposalID,
								ContractTypeID
							FROM [dbo].[ProposalContractTypeXREF]
							WHERE
								ProposalID = @ProposalID
					)
				MERGE INTO [Target]
				USING (
							SELECT DISTINCT 
								@ProposalID AS ProposalID, 
								ContractTypeID           
							FROM @ContractType
							)  AS [Source] ON
							[Target].[ProposalID] = [Source].[ProposalID] AND
							[Target].[ContractTypeID] = [Source].[ContractTypeID]
				WHEN NOT MATCHED BY SOURCE 
				THEN 
				DELETE      

				WHEN NOT MATCHED BY TARGET THEN
				INSERT (ProposalID, ContractTypeID)
				VALUES ([Source].[ProposalID], [Source].[ContractTypeID]); 
				WITH [Target] AS 
					(
							SELECT 
								ProposalID,
								CostElementID
							FROM [dbo].[ProposalCostElementXREF]
							WHERE
								ProposalID = @ProposalID
					)
				MERGE INTO [Target]
				USING (
							SELECT DISTINCT 
								@ProposalID AS ProposalID, 
								CostElementID           
							FROM @CostElement
							)  AS [Source] ON
							[Target].[ProposalID] = [Source].[ProposalID] AND
							[Target].[CostElementID] = [Source].[CostElementID]
				WHEN NOT MATCHED BY SOURCE 
				THEN 
				DELETE      

				WHEN NOT MATCHED BY TARGET THEN
				INSERT (ProposalID, CostElementID)
				VALUES ([Source].[ProposalID], [Source].[CostElementID])
				;     

				IF @ChangeChecklistFlag = 1
				BEGIN
					/*
						Before updating the checklist, the Proposal needs to go back to 
						In Progress with all the rules of unlocking applied
					*/

					DECLARE @CurrentProposalDateTime [datetime2](7)
					SELECT @CurrentProposalDateTime = [UpdateDate] 
							FROM [dbo].[Proposal] 
							WHERE ProposalID = @ProposalID

					EXECUTE [dbo].[unlockProposal] @ProposalID, @UpdateDate, 3 /*@UnlockOptionID*/

					/*CALL STORED PROCEDURE THAT MANAGES CHECKLIST*/
					EXECUTE [dbo].[upsertProposalChecklistTemplate] @ProposalID, @ProposalChecklistTypeID, @ChangeChecklistFlag
				END

				IF @GeneratedTrackingID = 1
					BEGIN
						UPDATE dbo.ProposalTrackingGenerator
						SET UsedByProposalID = @ProposalID  
						WHERE ProposalTrackingGeneratorID = @ProposalTrackingGeneratorID 
					END
			END
		ELSE
			BEGIN
				SET @ErrorMessage =   'The Proposal with ID ' + CAST(@ProposalID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
						@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
				RETURN
			END
	END

IF @@ERROR = 0
	  SELECT @ProposalID as ProposalID

GO

/*
    File: \Stored Procedures\upsertProposalChecklist.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertProposalChecklist.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalChecklist];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProposalChecklist]
(
	@ProposalChecklistID [int],
	@UpdateDate [datetime2](7),
	@ProposalID [int],
	@ProposalSubmittalDate [date],
	@ISGSTotalPrice [bigint],
	@Profit [bigint],
	@Com [bigint],
	@ProfitFeeWithCom [bigint],
	@ROSPercentage [decimal](4, 2),
	@LMLaborHours [decimal](11, 2),
	@LMLaborCost [bigint],
	@SubcontractorCost [bigint],
	@MaterialCost [bigint],
	@IWTACost [bigint],
	@TravelCost [bigint],
	@OtherDirectCost [bigint],
	@DeliverChecklistDFARS [bit],
	@AbsoluteValue [bigint] = NULL,
	@CostThroughCom [bigint] = NULL,
	@IncludeInternationalCosts [BIT]
)
AS
/******************************************************************************
**		 
**		Name:	[upsertProposalChecklist]
**		Desc:	Insert/Update Proposal Checlist 
**			
**		
**
**		Auth: Don Canuso
**		Date: 5/16/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/27/13		dcanuso				additional columns added
**		11/20/13	dcanuso				Removed LMIS Total Price
**		1/29/14		dcanuso				On the UI, the entire checklist page 
**										is one transaction.
**										The Optimistic locking is affecting users 
**										from saving when 2 users are saving different
**										 sections of the page.  So, a decision was made
**										 to ignore the optimistic locking for this page
**										 and specifically for this SP because only one 
**										user type (pricer) is allowed to edit this 
**										information and it will be controlled 
**										by the application.
**		5/27/14		dcanuso				New Column Added: TempProposalSubmittalDate
**		11/03/15	dturk				New Column Added: TempProposalSubmittalDate
**		1/5/2017	pattoncr			Removing Segment
**      1/5/2017	twilson3			BOEJ-1706 Remove ICE fields
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not 
**										display technical details to the user
**		02/02/17	tglick				added new field [AbsoluteValue]
**		3/20/2017	twilson3			BOEJ-1957 Move CCPD from Checklist to Proposal
**		5/31/2018	ranzalon			BOEJ-3405 - remove TempProposalSubmittalDate
**      10/13/2021  Koovackal           IES-181 DB Work - Added Profit, Com,
**                                      ProfitFeeWithCom
**		07/19/2022	ranzalon			IES-1505 - added CostThroughCom
**		7/18/24		Dusan				PROPH-1560: Added Include International Costs
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF @ProposalChecklistID  < 0  /*Insert Record*/
	BEGIN
		DECLARE @Inserted AS Table (ID int)
		SET @UpdateDate = GETDATE()

INSERT INTO [dbo].[ProposalChecklist]
           ([UpdateDate]
           ,[ProposalID]
           ,[ProposalSubmittalDate]
           ,[ISGSTotalPrice]
           ,[Profit]
           ,[Com]
           ,[ProfitFeeWithCom]
           ,[ROSPercentage]
           ,[LMLaborHours]
           ,[LMLaborCost]
           ,[SubcontractorCost]
           ,[MaterialCost]
           ,[IWTACost]
           ,[TravelCost]
           ,[OtherDirectCost]
		   ,[DeliverChecklistDFARS]
		   ,[AbsoluteValue]
		   ,[CostThroughCom]
		   ,[IncludeInternationalCosts]
           )
     OUTPUT inserted.ProposalChecklistID INTO @Inserted
     VALUES
           (
             @UpdateDate
            ,@ProposalID
            ,@ProposalSubmittalDate
            ,@ISGSTotalPrice
			,@Profit
			,@Com
			,@ProfitFeeWithCom
			,@ROSPercentage
			,@LMLaborHours
			,@LMLaborCost
			,@SubcontractorCost
			,@MaterialCost
			,@IWTACost
			,@TravelCost
			,@OtherDirectCost
			,@DeliverChecklistDFARS
			,@AbsoluteValue
			,@CostThroughCom
			,@IncludeInternationalCosts
           )

	SELECT @ProposalChecklistID = ID FROM @Inserted
	

	END
ELSE
	/*Update*/
	BEGIN
		/*We are ignoring Optimistic Locking on this page
		IF (SELECT ProposalChecklistID FROM [dbo].[ProposalChecklist] WHERE ProposalChecklistID = @ProposalChecklistID) = @ProposalChecklistID
			BEGIN*/
			
	SET @UpdateDate = GETDATE()


		UPDATE [dbo].[ProposalChecklist]
		   SET [UpdateDate] = @UpdateDate
			  ,[ProposalID] = @ProposalID
			  ,[ProposalSubmittalDate] = @ProposalSubmittalDate
			  ,[ISGSTotalPrice] = @ISGSTotalPrice
			  ,[Profit] = @Profit
			  ,[Com] = @Com
			  ,[ProfitFeeWithCom] = @ProfitFeeWithCom
			  ,[ROSPercentage] = @ROSPercentage
			  ,[LMLaborHours] = @LMLaborHours
			  ,[LMLaborCost] = @LMLaborCost
			  ,[SubcontractorCost] = @SubcontractorCost
			  ,[MaterialCost] = @MaterialCost
			  ,[IWTACost] = @IWTACost
			  ,[TravelCost] = @TravelCost
			  ,[OtherDirectCost] = @OtherDirectCost
			  ,[DeliverChecklistDFARS] = @DeliverChecklistDFARS
			  ,[AbsoluteValue] = @AbsoluteValue
			  ,[CostThroughCom] = @CostThroughCom
			  ,[IncludeInternationalCosts] = @IncludeInternationalCosts
				 WHERE 
					ProposalChecklistID = @ProposalChecklistID

	END
/*
	Since Optimistic Locking is not allowed, this error will no longer exist			
		ELSE
			BEGIN
				SET @ErrorMessage =   'The Proposal Checklist entry with ID ' + CAST(@ProposalChecklistID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
   
           			
END
*/   

IF @@ERROR = 0
	SELECT @ProposalChecklistID AS ProposalChecklistID

GO

/*
    File: \Stored Procedures\upsertProposalChecklistTemplate.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertProposalChecklistTemplate.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalChecklistTemplate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalChecklistTemplate];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProposalChecklistTemplate]
(
      @ProposalID [int],
	  @ProposalChecklistTypeID [int],
	  @ChangeChecklistFlag [bit]
)
AS
/******************************************************************************
**          
**          Name: [upsertProposalChecklistTemplate]
**          Desc: Insert/Update/Delete Proposal Checklist Skeleton - Unanswered Questions
**                
**          
**
**          Auth: Don Canuso
**          Date: 7/27/2015
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ---------------------------------------
******************************************************************************/
SET NOCOUNT ON 


IF @ChangeChecklistFlag = 1
BEGIN
	/*THERE IS A CURRENT CHECKLIST THAT NEEDS TO BE REMOVED*/


		/*Determine the current checklist being used for deletion*/
		DECLARE @CurrentProposalAdequacyReviewID INT
		DECLARE @CurrentProposalPricingReviewID INT
		
		SELECT DISTINCT @CurrentProposalAdequacyReviewID = ProposalAdequacyReviewID
		FROM dbo.PARChecklistContent Q
			INNER JOIN dbo.ProposalPARChecklistXREF A ON Q.PARChecklistContentID = A.PARChecklistContentID
		WHERE ProposalID = @ProposalID

		SELECT DISTINCT @CurrentProposalPricingReviewID = ProposalPricingReviewID
		FROM dbo.PPRChecklistContent Q
			INNER JOIN dbo.ProposalPPRChecklistXREF A ON Q.PPRChecklistContentID = A.PPRChecklistContentID
		WHERE ProposalID = @ProposalID


        /*Delete the old/current checklist values*/
        DELETE FROM dbo.ProposalPARChecklistXREF
        FROM dbo.ProposalPARChecklistXREF A
			INNER JOIN 
				(
					SELECT 
						PARChecklistContentID, 
						ProposalAdequacyReviewID
					FROM dbo.PARChecklistContent
					WHERE ProposalAdequacyReviewID = @CurrentProposalAdequacyReviewID
				) Q ON A.PARChecklistContentID = Q.PARChecklistContentID
        WHERE
			A.ProposalID = @ProposalID 
        

        DELETE FROM dbo.ProposalPPRChecklistXREF
        FROM dbo.ProposalPPRChecklistXREF A
			INNER JOIN 
				(
					SELECT 
						PPRChecklistContentID, 
						ProposalPricingReviewID
					FROM dbo.PPRChecklistContent
					WHERE ProposalPricingReviewID = @CurrentProposalPricingReviewID
				) Q ON A.PPRChecklistContentID = Q.PPRChecklistContentID
        WHERE
			A.ProposalID = @ProposalID 



		/*Also need to delete any commnets that were added*/
       DELETE FROM dbo.ProposalChecklistComplete
       WHERE ProposalID = @ProposalID 

END


/*Add New Checklist*/
      
            DECLARE @ProposalAdequacyReviewID INT
            SELECT TOP 1 @ProposalAdequacyReviewID = ProposalAdequacyReviewID
            FROM dbo.ProposalAdequacyReview
            WHERE IsCurrent = 1 AND [ProposalChecklistTypeID] = @ProposalChecklistTypeID
            
            DECLARE @ProposalPricingReviewID INT
            SELECT TOP 1 @ProposalPricingReviewID = ProposalPricingReviewID
            FROM dbo.ProposalPricingReview
            WHERE IsCurrent = 1 AND [ProposalChecklistTypeID] = @ProposalChecklistTypeID           
            
            DECLARE @QuestionID int
            SELECT @QuestionID = TextTypeID FROM dbo.TextTypeLU WHERE TextType = 'Question'

/*
Comments are stored in dbo.ProposalChecklistComplete
Rows for comment answers are not needed for XREF Table
            
            DECLARE @CommentID int
            SELECT @CommentID = TextTypeID FROM dbo.TextTypeLU WHERE TextType = 'Comment'
            
			DECLARE @PeerCommentID int
			SELECT  @PeerCommentID = TextTypeID FROM dbo.TextTypeLU WHERE TextType = 'Peer Comment'

			DECLARE @PricerCommentID int
			SELECT @PricerCommentID = TextTypeID FROM dbo.TextTypeLU WHERE TextType = 'Pricer Comment'
            
*/
            INSERT INTO [dbo].[ProposalPARChecklistXREF]
           ([ProposalID]
           ,[PARChecklistContentID]
           ,[ResponseID]
           ,[ResponseTypeID]
                  )

            SELECT 
                  @ProposalID,
                  [PARChecklistContentID]
                  ,5 /* [ResponseID] = 5 - Not Set*/
                  ,1
                  
        FROM [dbo].[PARChecklistContent]
        WHERE ProposalAdequacyReviewID = @ProposalAdequacyReviewID
        AND TextTypeID IN (@QuestionID)/*Removed based on comments being stored elsewhere:, @CommentID, @PricerCommentID, @PeerCommentID)*/
        
            UNION
            SELECT 
                  @ProposalID,
                  [PARChecklistContentID]
                  ,5 /* [ResponseID] = 5 - Not Set*/
                  ,2
                  
        FROM [dbo].[PARChecklistContent]
        WHERE ProposalAdequacyReviewID = @ProposalAdequacyReviewID
        AND TextTypeID IN (@QuestionID)/*Removed based on comments being stored elsewhere:, @CommentID, @PricerCommentID, @PeerCommentID)*/


            INSERT INTO [dbo].[ProposalPPRChecklistXREF]
           ([ProposalID]
           ,[PPRChecklistContentID]
           ,[ResponseID]
           ,[ResponseTypeID]
           )

            SELECT 
                  @ProposalID,
                  [PPRChecklistContentID]
                  ,5 /* [ResponseID] = 5 - Not Set*/
                  ,1
                  
        FROM [dbo].[PPRChecklistContent]
        WHERE ProposalPricingReviewID = @ProposalPricingReviewID
        AND TextTypeID IN (@QuestionID)/*Removed based on comments being stored elsewhere:, @CommentID, @PricerCommentID, @PeerCommentID)*/
        
        
        
        /*
		First Question in PPR Checklist should be set to NO rather than N/A
		*/
		UPDATE [dbo].[ProposalPPRChecklistXREF]
		SET [ResponseID] = 2 /*NO*/
		WHERE 
				ProposalID = @ProposalID AND
				[ResponseID] = 5 /*NOT SET FROM ABOVE*/ AND
				[ResponseTypeID] = 1 AND
				[PPRChecklistContentID] = 
					(
						SELECT TOP 1 [PPRChecklistContentID]
						FROM [dbo].[PPRChecklistContent]
						WHERE 
						TextTypeID = 4 /*QUESTION*/ AND
						ProposalPricingReviewID = @ProposalPricingReviewID
						ORDER BY SortOrder ASC
					)
GO

/*
    File: \Stored Procedures\upsertProposalClass.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertProposalClass.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalClass]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalClass];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProposalClass]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertProposalClass]
	**		Desc:	Insert/Update Proposal Class LU values 
	**			
	**		
	**
	**		Auth: Dusan Palider
	**		Date: 7/17/2016
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)

			INSERT INTO [dbo].ProposalClassLU
						(
							ProposalClass,
							IsActive
						)
				OUTPUT inserted.ProposalClassId INTO @Inserted
				VALUES
						(
							@Text,
							@IsActive
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			UPDATE [dbo].ProposalClassLU
			   SET 
					ProposalClass = @Text,
					IsActive = @IsActive
				WHERE 
					ProposalClassID = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertProposalContractsData.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertProposalContractsData.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalContractsData]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalContractsData];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProposalContractsData]
(
	@ProposalContractsDataId [int],
	@UpdateDT [datetime2](7),
	@ProposalID [int],
	@PreviouslySubmittedROM [int],
	@CustomerSubmittalDate [date],
	@ContractsCorrespondLogNumber [varchar](20),
	@FinalNegotiatedValue [bigint],
	@FinalNegotiatedDate [date],
	@EppDelegationAuthority [int],
	@BidEppDate [date],
	@ProgramEppDate [date],
	@MissionSegmentEppDate [date],
	@LobEppDate [date],
	@PreSpaceEppDate [date],
	@SpaceEppDate [date],
	@PreCorporateEppDate [date],
	@CorporateEppDate [date],
	@EppRosDelegationNotes [varchar](1000),
	@LmWon [bit],
	@ModCompletedDate [date],
	@CageCode [varchar](10),
	@CustomerDueDate [date],
	@IsInsuranceDirect [int] = NULL,
	@InsuranceType [int] = NULL,
	@ProposedInsurance [bigint] = NULL,
	@NegotiatedInsurance [bigint] = NULL
)
AS
/******************************************************************************
**
**		Name: [upsertProposalContractsData]
**		Desc: Upsert Proposal Contracts Data
**
**
**		Auth: Ajay Koovackal
**		Date: 10/29/21
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		10/29/21    Koovackal			Creation
**      02/16/22    Koovackal           Add upsert for EPP fields
**		07/18/22	ranzalon			Add CageCode
**		02/22/23	ranzalon			Add CustomerDueDate
**		11/12/24	twilson3			Add Insurance fields
**		05/12/25	ranzalon			Add Bid and Mission Segment EPP Dates
*******************************************************************************/
SET NOCOUNT ON

	IF @ProposalContractsDataId < 0 -- Insert new
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			INSERT INTO [dbo].[ProposalContractsData] (UpdateDT, ProposalID, PreviouslySubmittedROM, CustomerSubmittalDate,
														ContractsCorrespondLogNumber, FinalNegotiatedValue, FinalNegotiatedDate,
														EppDelegationAuthority, ProgramEppDate, LobEppDate, PreSpaceEppDate, 
														SpaceEppDate, PreCorporateEppDate, CorporateEppDate, EppRosDelegationNotes, 
														LmWon, ModCompletedDate, CageCode, CustomerDueDate, IsInsuranceDirect, 
														InsuranceType, ProposedInsurance, NegotiatedInsurance, BidEppDate, MissionSegmentEppDate)
				OUTPUT inserted.ProposalContractsDataId INTO @Inserted
				VALUES (GETDATE(), @ProposalID, @PreviouslySubmittedROM, @CustomerSubmittalDate, @ContractsCorrespondLogNumber,
						@FinalNegotiatedValue, @FinalNegotiatedDate, @EppDelegationAuthority, @ProgramEppDate, @LobEppDate, 
						@PreSpaceEppDate, @SpaceEppDate, @PreCorporateEppDate, @CorporateEppDate, @EppRosDelegationNotes, @LmWon, 
						@ModCompletedDate, @CageCode, @CustomerDueDate, @IsInsuranceDirect, @InsuranceType, @ProposedInsurance, 
						@NegotiatedInsurance, @BidEppDate, @MissionSegmentEppDate)
			SELECT @ProposalContractsDataId = Id FROM @Inserted
		END
	ELSE -- updating existing
		BEGIN
			IF (SELECT UpdateDT FROM ProposalContractsData WHERE ProposalContractsDataId = @ProposalContractsDataId) = @UpdateDT
				UPDATE ProposalContractsData
					SET UpdateDT = GETDATE(),
						ProposalID = @ProposalID,
						PreviouslySubmittedROM = @PreviouslySubmittedROM,
						CustomerSubmittalDate = @CustomerSubmittalDate,
						ContractsCorrespondLogNumber = @ContractsCorrespondLogNumber,
						FinalNegotiatedValue = @FinalNegotiatedValue, 
						FinalNegotiatedDate = @FinalNegotiatedDate,
						EppDelegationAuthority = @EppDelegationAuthority,
						BidEppDate = @BidEppDate,
						ProgramEppDate = @ProgramEppDate,
						MissionSegmentEppDate = @MissionSegmentEppDate,
						LobEppDate = @LobEppDate,
						PreSpaceEppDate = @PreSpaceEppDate,
						SpaceEppDate = @SpaceEppDate,
						PreCorporateEppDate = @PreCorporateEppDate,
						CorporateEppDate = @CorporateEppDate,
						EppRosDelegationNotes = @EppRosDelegationNotes,
						LmWon = @LmWon,
						ModCompletedDate = @ModCompletedDate,
						CageCode = @CageCode,
						CustomerDueDate = @CustomerDueDate,
						IsInsuranceDirect = @IsInsuranceDirect, 
						InsuranceType = @InsuranceType, 
						ProposedInsurance = @ProposedInsurance, 
						NegotiatedInsurance = @NegotiatedInsurance
					WHERE ProposalContractsDataId = @ProposalContractsDataId
			ELSE
				BEGIN
					DECLARE @ErrorMessage varchar (500)
					SET @ErrorMessage = 'The ProposalContractsData with Id ' + CAST(@ProposalContractsDataId  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser. Please refresh your data.'
					RAISERROR (@ErrorMessage, 11, 1)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @ProposalContractsDataId as ProposalContractsDataId

GO

/*
    File: \Stored Procedures\upsertProposalType.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertProposalType.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalType];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProposalType]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertProposalType]
	**		Desc:	Insert/Update Proposal Type LU values 
	**			
	**		
	**
	**		Auth: Dusan Palider
	**		Date: 7/17/2016
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)

			INSERT INTO [dbo].[ProposalTypeLU]
						(
							ProposalType,
							IsActive
						)
				OUTPUT inserted.ProposalTypeId INTO @Inserted
				VALUES
						(
							@Text,
							@IsActive
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			UPDATE [dbo].[ProposalTypeLU]
			   SET 
					ProposalType = @Text,
					IsActive = @IsActive
				WHERE 
					ProposalTypeId = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertRequestTypes.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertRequestTypes.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRequestTypes]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertRequestTypes];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertRequestTypes]
(
	@Id			INT,
	@Text		VARCHAR(50),
	@IsActive	BIT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertRequestTypes]
	**		Desc:	Insert/Update Request Types LU values 
	**			
	**		
	**
	**		Auth: Dusan Palider
	**		Date: 7/17/2016
	*******************************************************************************
	**		Change History
	*******************************************************************************
	*******************************************************************************/
	SET NOCOUNT ON 

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)

			INSERT INTO [dbo].RequestTypeLU
						(
							RequestType,
							IsActive
						)
				OUTPUT inserted.RequestTypeID INTO @Inserted
				VALUES
						(
							@Text,
							@IsActive
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			UPDATE [dbo].RequestTypeLU
			   SET 
					RequestType = @Text,
					IsActive = @IsActive
				WHERE 
					RequestTypeID = @Id
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO




/*
    File: \Table Based Processing\DataMart.sql
*/
PRINT '### Starting file: \Table Based Processing\DataMart.sql';
-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertDataMartEmployees]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertDataMartEmployees];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_DatamartEmployee' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_DatamartEmployee];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_DatamartEmployee] AS TABLE(
	[empl_ser_no] varchar(50) NOT NULL,
	[empl_first_nm] varchar(30) NOT NULL,
	[empl_last_nm] varchar(30) NOT NULL,
	[nt_domain_nm] varchar(15) NOT NULL,
	[nt_account_nm] varchar(20) NOT NULL,
	[rpt_to_ser_no] varchar(11) NOT NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertDataMartEmployees]
(
@Employees [dbo].[TT_DatamartEmployee] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertDataMartEmployees]
**		Desc: Insert/Update data into DataMart table
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 10/2019
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

DELETE FROM [DataMart].[DataMartEmployee]

INSERT INTO [DataMart].[DataMartEmployee]
           (
			[empl_ser_no],
			[empl_first_nm],
			[empl_last_nm],
			[nt_domain_nm],
			[nt_account_nm],
			[rpt_to_ser_no]
           )
SELECT 	    E.[empl_ser_no],
			E.[empl_first_nm],
			E.[empl_last_nm],
			E.[nt_domain_nm],
			E.[nt_account_nm],
			E.[rpt_to_ser_no]
FROM  @Employees E



IF @@ERROR = 0
SELECT E.OrderID
FROM  @Employees E
INNER JOIN [DataMart].[DataMartEmployee] X ON
	E.[empl_ser_no] = X.[empl_ser_no]
ORDER BY OrderID
GO

/*
    File: \Table Based Processing\tt_ProposalContractTypeXREF.udtt.sql
*/
PRINT '### Starting file: \Table Based Processing\tt_ProposalContractTypeXREF.udtt.sql';
-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'tt_ProposalContractTypeXREF' AND ss.name = N'dbo')
	DROP TYPE [dbo].[tt_ProposalContractTypeXREF];
GO

CREATE TYPE [dbo].[tt_ProposalContractTypeXREF] AS  TABLE (
    [ProposalID]     INT NOT NULL,
    [ContractTypeID] INT NOT NULL);

GO

/*
    File: \X_Data Cleanup\DeleteErrorLogs.sql
*/
PRINT '### Starting file: \X_Data Cleanup\DeleteErrorLogs.sql';
-- Clean up Error Logs older than 30 days
DELETE FROM [ELMAH_Error] WHERE TimeUtc < DATEADD(d, -30, getdate());
GO

/*
    File: \X_Data Cleanup\DeleteOrphanedAttachments.sql
*/
PRINT '### Starting file: \X_Data Cleanup\DeleteOrphanedAttachments.sql';
-- This should never happen, but just in case, we'll delete orphaned attachments
DELETE FROM Attachment WHERE Id NOT IN (SELECT DISTINCT AttachmentId FROM ProposalsAttachments);
GO

PRINT '###### SCRIPT FINISHED ######';