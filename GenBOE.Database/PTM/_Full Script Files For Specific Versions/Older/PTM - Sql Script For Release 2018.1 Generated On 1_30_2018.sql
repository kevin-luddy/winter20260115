PRINT '###### SCRIPT IS STARTING ######';
/*
    This file was auto-generated for Release: 2018.1, on 1/30/2018.
    It contains all of the Release specific scripts, modifying data/tables as well as all of the Stored Procedures and User Defined Table Types.
*/

/*
    File: \Release 2018.1\1 - Release 2018.1 Script.sql
*/
PRINT '### Starting file: \Release 2018.1\1 - Release 2018.1 Script.sql';
/*
	## START ##
	
	1/23/2018 Dusan BOEJ-2967 PAR Checklist rows missing in the export
*/
UPDATE PARChecklistContent SET SubmissionItem = 'MATERIALS AND SERVICES'
	WHERE ProposalAdequacyReviewID = 10 AND ChecklistText = '<p><b>Materials and Services</b></p>';
UPDATE PARChecklistContent SET SubmissionItem = 'DIRECT LABOR'
	WHERE ProposalAdequacyReviewID = 10 AND ChecklistText = '<p><b>Direct Labor</b></p>';
UPDATE PARChecklistContent SET SubmissionItem = 'OTHER COSTS'
	WHERE ProposalAdequacyReviewID = 10 AND ChecklistText = '<p><b>Other Costs</b></p>';
UPDATE PARChecklistContent SET SubmissionItem = 'OTHER'
	WHERE ProposalAdequacyReviewID = 10 AND ChecklistText = '<p><b>Other</b></p>';
/*
	1/23/2018 Dusan BOEJ-2967 PAR Checklist rows missing in the export

	## END ##
*/

/*
    File: \1 Views\genTracData.view.sql
*/
PRINT '### Starting file: \1 Views\genTracData.view.sql';
DROP VIEW [genBOE].[genTracData];
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
*******************************************************************************/
SELECT DISTINCT
	P.ProposalID AS [genTracProposalID],
	P.ProposalTrackingID AS [TrackingNumber],
	P.ProposalTitle AS [ProposalTitle],
	
	--Product Line has been renamed Line of Business
	PL.ProductLineLongName AS [LineOfBusinessName],
	
	--Line of Business has been renamed Program Area
	LOB.LineOfBusinessLongName AS [ProgramAreaName],
	
	PT.ProposalType AS [ProposalType],		
	PS.ProposalStatus AS [ProposalStatus],	
	
	P.EstimatedProposalValue AS [EstimatedValue],
	PC.ISGSTotalPrice AS [SubmittedValue],
	

	--per BOEJ-1546, replaced start/end dates with RFPReceivedDate & AnticpatedDeliveryDate

	CAST(P.RFPReceivedDate AS DATE) AS [ProposalStartDate],
	CAST(P.AnticipatedDeliveryDate AS DATE) AS [ProposalEndDate],

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

	INNER JOIN [dbo].[ProductLine] PL ON P.ProductLineID = PL.ProductLineID
	INNER JOIN [dbo].[LineOfBusiness] LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
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


GO

/*
    File: \1 Views\vwLineOfBusiness.view.sql
*/
PRINT '### Starting file: \1 Views\vwLineOfBusiness.view.sql';
DROP VIEW [dbo].[vwLineOfBusiness];
GO

CREATE VIEW [dbo].[vwLineOfBusiness]
AS
SELECT [LineOfBusinessID]
      ,[LineOfBusinessLongName]
  FROM [dbo].[LineOfBusiness]

GO

/*
    File: \1 Views\vwProposalActivityReport.view.sql
*/
PRINT '### Starting file: \1 Views\vwProposalActivityReport.view.sql';
DROP VIEW [dbo].[vwProposalActivityReport];
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
	P.ProposalTitle AS [Proposal Title],
	P.DateCreated AS DateCreated,
	
	LOB.LineOfBusinessID AS LineOfBusinessID,

	PL.ProductLineID AS ProductLineID,
	PL.ProductLineName AS [Product Line],
	/*PL.ProductLineName + ' - ' + */LOB.LineOfBusinessName AS [Product Line / LOB],

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

	PC.ProposalSubmittalDate AS [Submitted Date],	

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
	INNER JOIN dbo.LineOfBusiness LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
	INNER JOIN dbo.ProductLine PL ON P.ProductLineID = PL.ProductLineID
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
DROP VIEW [dbo].[vwProposalLogReport];
GO

CREATE VIEW [dbo].[vwProposalLogReport] AS
/******************************************************************************
**		 
**		Name: [vwProposalLogReport]
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
**		10/21/14	Unknown				Added MainProposalTrackingID to assist in performance tuning Groove
**		11/03/15	dturk				Task 37343:Add new CCPDRequired field to both Groove views
**		02/10/16	dturk				Task 37631:Add new ProgramProposalStatus field to both Groove views
**		01/04/17	Dusan				Removing Revisions & LMIS
**		01/05/17	pattoncr			Removing Segment
**      01/05/17	twilson3			BOEJ-1706 Remove ICE fields
**		01/19/17	n99040				added fields TechLead, IndependentReviewer, LeadEstimator, ProposalMgr, CoverSheetApprover, PricingVErification
**		02/02/17	tglick				added new field [Revised Submittal Date]
**      02/17/17	twilson3			BOEJ-1903 Add LOB Estimating Manager
**										BOEJ-1905 Remove IS&GS from Total Price column
**		3/22/2017	twilson3			BOEJ-1909 Remove Profit Tracking #
**		10/18/2017	Dusan				BOEJ-2652 Add Proposal Class field to the report
*******************************************************************************/
SELECT  
	
	P.ProposalID AS ProposalID,
	
	CAST (P.DateCreated AS DATE) AS DateCreated,
	YEAR(P.DateCreated) AS [Year],
	
	LOB.LineOfBusinessID AS LineOfBusinessID,
	PL.ProductLineID AS ProductLineID,
	PL.ProductLineName AS [Product Line],
	LOB.LineOfBusinessName AS [Product Line / LOB],
	
	P.ProposalTrackingID AS [Tracking #],
	P.ProposalTitle AS [Proposal Title],
	CASE P.IsIWTA
		WHEN 1 THEN 'Yes'
		WHEN 0 THEN 'No'
		ELSE 'N/A'
	END AS [IWTA],
/*
if IWTA is Yes, then use Submitted value (which is always IS&GS Total Price).  If Iwta is no, then blank 
*/	
	CASE P.IsIWTA
		WHEN 1 THEN PC.ISGSTotalPrice
		WHEN 0 THEN NULL--''
		ELSE NULL--''
	END AS [IWTA Submitted Value],
	
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
	--[PCE Proposal Review Complete Date].MaxSubmitDate 
	CONVERT(varchar, CONVERT(datetime2(7), [PCE Proposal Review Complete Date].MaxSubmitDate), 100) AS [PCE Proposal Review Complete Date],
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
	PC.[ProfitFee] AS [Profit/Fee + COM],	
	PC.[ISGSTotalPrice] AS [Total Price],

	PC.[ROSPercentage] AS [ROS %],
	

	CAST(PC.ProposalSubmittalDate AS DATE) AS [Submitted Date],
	PT.ProposalType AS [Proposal Type],
	
	dbo.udfCreateCommaSeparatedList (P.ProposalID, 1) AS [Contract Type],
	
	CASE I.ISGSRole
		WHEN 'IWTA' THEN ''
		ELSE I.ISGSRole
	END AS [Prime or Sub],
	
	P.RFPNumber AS [RFP/Contract Modification Number],

	dbo.udfCreateCommaSeparatedList (P.ProposalID, 2) AS [Elements of Cost],
		
	[IS&GS Contracts POC].[IS&GS Contracts POC] AS [Contracts POC],
	P.Customer AS [Customer],
	CusType.CustomerType AS [Customer Type],
	CreatedBy.NTID AS [Created By],
	CAST (P.DateCreated AS DATE) AS [Created Date],
	
	PS.ProposalStatusID AS [ProposalStatusID],
	PS.ProposalStatus AS [Proposal Status],
	
	P.OTISOpportunityID AS [OTIS #],
	
	CASE
		WHEN P.PricingToolID = 3 AND P.PricingToolName IS NOT NULL THEN P.PricingToolName
		ELSE T.PricingTool
	END AS [Pricing Tool],


	CASE
		WHEN P.BOEToolID = 6 AND P.BOEToolName IS NOT NULL THEN P.BOEToolName
		ELSE B.BOETool
	END AS [BOE Tool]


	       ,CAST(P.NegotiationStartDate AS DATE) AS [Negotiation Start Date]
           ,CAST(P.NegotiationEndDate AS DATE) AS [Negotiation End Date]
		   ,CAST(P.AwardDate AS DATE) AS [Award Date]
		   ,CAST(P.RFPIssuedDate AS DATE) AS [RFP Issued Date]
		   ,CAST(P.RFPReceivedDate AS DATE) AS [RFP Received Date]
		   ,CASE P.[UndefinitizedContractActions]
					WHEN 1 THEN 'Yes'
					WHEN 0 THEN 'No'
					ELSE ''
			END AS [UCA]		   
		   ,CAST(P.AuditEntranceConferenceDate AS DATE) AS [Audit Entrance Conference Date]  
		   ,CAST(P.AuditReportDate AS DATE) AS [Audit Report Date]
		   ,CASE P.[ProposalDeemedInadequate] 
					WHEN 1 THEN 'Yes'
					WHEN 0 THEN 'No'
					ELSE ''
			END AS [Proposal Inadequate]		   
		   ,P.[Comments]
		   
		   ,CG.ContractTypeGroupID AS [ContractTypeGroupID] 
		   ,CG.ContractTypeGroup AS [ContractTypeGroup]
		   
		    
		   ,	CASE P.IsScheduleProposal
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
	
	CASE LEN(LEFT (ProposalTrackingID, CHARINDEX ('-',ProposalTrackingID) -1))
		WHEN 4 THEN LTRIM(RTRIM(LEFT (P.ProposalTrackingID, 10))) 
		WHEN 2 THEN LTRIM(RTRIM(LEFT (P.ProposalTrackingID, 8)))
	END	AS MainProposalTrackingID
	,CCPDRequired
  ,ppsLU.ProgramProposalStatus 		
  ,pcLU.ProposalClass AS [Proposal Class]
  FROM [dbo].[Proposal] P
	INNER JOIN dbo.LineOfBusiness LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
	INNER JOIN dbo.ProductLine PL ON P.ProductLineID = PL.ProductLineID
	INNER JOIN dbo.ProposalTypeLU PT ON P.ProposalTypeID = PT.ProposalTypeID
	
	INNER JOIN dbo.PricingToolLU T ON P.PricingToolID = T.PricingToolID
	INNER JOIN dbo.BOEToolLU B ON P.BOEToolID = B.BOEToolID
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
			SELECT 
				ProposalID,
				MAX(SubmitDate) AS MaxSubmitDate
			FROM dbo.ProposalChecklistComplete
			GROUP BY ProposalID
		) [PCE Proposal Review Complete Date] ON P.ProposalID = [PCE Proposal Review Complete Date].ProposalID



	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName AS [Additional Pricing Resource 2],
				U.UserID AS [Additional Pricing Resource 2 UserID],
				U.NTID AS [Additional Pricing Resource 2 NTID]
				
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE RoleID = 5	/*Additional Pricing Resource 2*/
		) [AdditionalPricingResource2] ON P.ProposalID = [AdditionalPricingResource2].ProposalID
	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName AS [Additional Pricing Resource 1],
				U.UserID AS [Additional Pricing Resource 1 UserID],
				U.NTID AS [Additional Pricing Resource 1 NTID]
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE RoleID = 4	/*Additional Pricing Resource 1*/
		) [AdditionalPricingResource1] ON P.ProposalID = [AdditionalPricingResource1].ProposalID

	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName AS [Cost Volume Lead],
				U.UserID AS [Cost Volume Lead UserID],
				U.NTID AS [Cost Volume Lead NTID]
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE RoleID = 2 /*Cost Volume Lead*/
		) [CostVolumeLead] ON P.ProposalID = [CostVolumeLead].ProposalID



	LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName AS [IS&GS Contracts POC],
				U.UserID AS [IS&GS Contracts POC UserID],
				U.NTID AS [IS&GS Contracts POC NTID]
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE RoleID = 8 /*IS&GS Contracts POC*/
		) [IS&GS Contracts POC] ON P.ProposalID = [IS&GS Contracts POC].ProposalID


		-- Independent Reviewer
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 11  /*11  Peer Reviewer*/
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

		--LOB Estimating Manager
		LEFT OUTER JOIN
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 20
		) LOBMgr ON P.ProposalID = LOBMgr.ProposalID

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

	LEFT JOIN dbo.ProgramProposalStatusLU ppsLU
	ON ppsLU.ProgramProposalStatusID = p.ProgramProposalStatusID

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
    File: \Functions\_DeleteErrorLogs.sql
*/
PRINT '### Starting file: \Functions\_DeleteErrorLogs.sql';
-- Clean up Error Logs older than 30 days
DELETE FROM [ELMAH_Error] WHERE TimeUtc < DATEADD(d, -30, getdate());
GO

/*
    File: \Stored Procedures\archiveProposal.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\archiveProposal.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[archiveProposal]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[archiveProposal];
GO

CREATE PROCEDURE [dbo].[archiveProposal]
(	
	@CreateStartDate [date],
	@CreateEndDate [date],
	@ProductLineID [varchar] (100),
	@LineOfBusinessID [varchar] (100)
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
**					@ProductLineID [varchar] (100) = '1',
**					@LineOfBusinessID [varchar] (100)='1'
**
**          Auth: Don Canuso
**          Date: 7/17/2013
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ------------------------------
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

/*
Process Product Lines
*/
DECLARE @ProductLine TABLE (ProductLineID INT)

IF @ProductLineID IS NULL OR @ProductLineID = 'ALL'
	BEGIN
		INSERT INTO @ProductLine
		SELECT ProductLineID FROM dbo.ProductLine
	END
ELSE
	BEGIN
		IF RIGHT(@ProductLineID, 1) <> ','
			SET @ProductLineID = @ProductLineID + ','

	WHILE (SELECT CHARINDEX (',', @ProductLineID) ) > 1
		BEGIN
		      
			  INSERT INTO @ProductLine
			  SELECT LEFT (@ProductLineID, CHARINDEX (',', @ProductLineID) -1)
			  SET @ProductLineID = RIGHT (@ProductLineID, LEN (@ProductLineID) - CHARINDEX (',', @ProductLineID) )
		      
		END
	END


/*
Process Line Of Business
*/
DECLARE @LineOfBusiness TABLE (LineOfBusinessID INT)

IF @LineOfBusinessID IS NULL OR @LineOfBusinessID = 'ALL'
	BEGIN
		INSERT INTO @LineOfBusiness
		SELECT LineOfBusinessID FROM dbo.LineOfBusiness
	END
ELSE
	BEGIN
		IF RIGHT(@LineOfBusinessID, 1) <> ','
			SET @LineOfBusinessID = @LineOfBusinessID + ','

	WHILE (SELECT CHARINDEX (',', @LineOfBusinessID) ) > 1
		BEGIN
		      
			  INSERT INTO @LineOfBusiness
			  SELECT LEFT (@LineOfBusinessID, CHARINDEX (',', @LineOfBusinessID) -1)
			  SET @LineOfBusinessID = RIGHT (@LineOfBusinessID, LEN (@LineOfBusinessID) - CHARINDEX (',', @LineOfBusinessID) )
		      
		END
	END




DECLARE @Archive int
SELECT @Archive = ProposalStatusID FROM dbo.ProposalStatusLU WHERE ProposalStatus = 'Archived'

DECLARE @ArchiveCount TABLE (ProposalID int)

UPDATE dbo.Proposal
SET	ProposalStatusID = @Archive
OUTPUT inserted.ProposalID INTO @ArchiveCount
FROM dbo.Proposal P
	INNER JOIN @LineOfBusiness LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
	INNER JOIN @ProductLine PL ON P.ProductLineID = PL.ProductLineID
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

CREATE PROCEDURE [dbo].[archiveProposalCount]
(	
	@CreateStartDate [date],
	@CreateEndDate [date],
	@ProductLineID [varchar] (100),
	@LineOfBusinessID [varchar] (100)
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
**					@ProductLineID [varchar] (100) = '1',
**					@LineOfBusinessID [varchar] (100)='1'
**
**          Auth: Don Canuso
**          Date: 7/17/2013
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ------------------------------
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

/*
Process Product Lines
*/
DECLARE @ProductLine TABLE (ProductLineID INT)

IF @ProductLineID IS NULL OR @ProductLineID = 'ALL'
	BEGIN
		INSERT INTO @ProductLine
		SELECT ProductLineID FROM dbo.ProductLine
	END
ELSE
	BEGIN
		IF RIGHT(@ProductLineID, 1) <> ','
			SET @ProductLineID = @ProductLineID + ','

	WHILE (SELECT CHARINDEX (',', @ProductLineID) ) > 1
		BEGIN
		      
			  INSERT INTO @ProductLine
			  SELECT LEFT (@ProductLineID, CHARINDEX (',', @ProductLineID) -1)
			  SET @ProductLineID = RIGHT (@ProductLineID, LEN (@ProductLineID) - CHARINDEX (',', @ProductLineID) )
		      
		END
	END


/*
Process Line Of Business
*/
DECLARE @LineOfBusiness TABLE (LineOfBusinessID INT)

IF @LineOfBusinessID IS NULL OR @LineOfBusinessID = 'ALL'
	BEGIN
		INSERT INTO @LineOfBusiness
		SELECT LineOfBusinessID FROM dbo.LineOfBusiness
	END
ELSE
	BEGIN
		IF RIGHT(@LineOfBusinessID, 1) <> ','
			SET @LineOfBusinessID = @LineOfBusinessID + ','

	WHILE (SELECT CHARINDEX (',', @LineOfBusinessID) ) > 1
		BEGIN
		      
			  INSERT INTO @LineOfBusiness
			  SELECT LEFT (@LineOfBusinessID, CHARINDEX (',', @LineOfBusinessID) -1)
			  SET @LineOfBusinessID = RIGHT (@LineOfBusinessID, LEN (@LineOfBusinessID) - CHARINDEX (',', @LineOfBusinessID) )
		      
		END
	END




DECLARE @Archive int
SELECT @Archive = ProposalStatusID FROM dbo.ProposalStatusLU WHERE ProposalStatus = 'Archived'



SELECT COUNT (*) AS ArchiveCount
FROM dbo.Proposal P
	INNER JOIN @LineOfBusiness LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID
	INNER JOIN @ProductLine PL ON P.ProductLineID = PL.ProductLineID
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
							2  /*Completed*/
						   )

GO

/*
    File: \Stored Procedures\CreatePARChecklistReport.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\CreatePARChecklistReport.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreatePARChecklistReport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CreatePARChecklistReport];

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
*******************************************************************************/
SET NOCOUNT ON

SELECT DISTINCT 
	C.QuestionNumber,
	C.Reference,
	C.SubmissionItem,
	X.PageNumber,
	X.Comment,
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

CREATE  PROCEDURE [dbo].[CreateProposalActivityReport]
(
	@ProposalStatus varchar (8000),
	@AllProposals bit=NULL,
	@SpecificProposals bit=NULL,
	@CustomerType varchar (8000)=NULL,
	@LOB varchar (8000)=NULL,
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
**		8/20/13		dcanuso				Task #21003: Reports SP should ensure no 
**										deleted proposals are displayed to non Admins
**		8/20/13		dcanuso				Proposal.ProfitTrackingNumber added
**		8/20/13		dcanuso				Task #21241: For Proposal Activity Report, 
**										handle pricer as NTID rather than User ID.
**		4/25/14		dcanuso				Dates need to be set at 0:00:00 so casting as Date
**		11/20/14	dcanuso				New Proposal Set Up role - Renamed to 
**										reuse Product Line Viewer to Role XREF
**		01/19/17	n99040				added fields TechLead, IndependentReviewer, LeadEstimator, ProposalMgr, CoverSheetApprover, PricingVErification
**		02/02/17	tglick				added new field [Revised Submittal Date]
**		2/15/17		Dusan				added Absolute Value
**		3/22/2017	twilson3			BOEJ-1909 Remove Profit Tracking #
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

	SET @LOB = '1,2,3'  /*Evaluates as Civil - Energy & Environmental Services*/

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






/*Line of Business*/
DECLARE @tblLineOfBusiness TABLE (LineOfBusinessID int)

IF @LOB IS NULL OR @LOB = 'All'
	BEGIN
		INSERT INTO @tblLineOfBusiness
		SELECT LineOfBusinessID FROM dbo.LineOfBusiness
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
      ,[Proposal Title]
      ,[LineOfBusinessID]
      ,[ProductLineID]
      ,[Product Line]
      ,[Product Line / LOB]
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
      ,[Submitted Date]
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
			[LineOfBusinessID] IN (SELECT LineOfBusinessID FROM @tblLineOfBusiness) AND
			[CustomerTypeID] IN (SELECT CustomerTypeID FROM @tblCustomerType) 
		)
	) AND
	
	(@CreateStartDate IS NULL OR CAST(DateCreated AS DATE) > = @CreateStartDate) AND
	(@CreateEndDate IS NULL OR CAST(DateCreated AS DATE) < = @CreateEndDate) AND
	(@TrackingNumber IS NULL OR [Tracking #] LIKE @TrackingNumber + '%') AND
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
					ProductLineID IN
						(
							SELECT ProductLineID
							FROM dbo.ProductLineRoleXREF X
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
**		To avoid confusion.. here's some info about how names changed recently (2016ish)..
**
**			OLD           -> NEW
**			ProductLine   -> LOB
**			LOB           -> Program Area
**
**		Auth: Don Canuso
**		Date: 7/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/20/13		dcanuso				Task #21003: Reports SP should ensure no 
**										deleted proposals are displayed to non Admins
**		8/20/13		dcanuso				Proposal.ProfitTrackingNumber added
**		8/28/13		dcanuso				Fields added
**		11/20/13	dcanuso				Removed: LMISTotalPrice
**		1/9/14		dcanuso				WI 24977 Update Proposal Log Criteria 
**										to recognize PROFIT number or genTRAC number
**		2/28/14		DCANUSO				Adding in Reorg Table to show 2014 equivalent
**										to 2013 records
**		3/31/14		dcanuso				Enhancement 26789 - Update Proposal Log Report
**										 for LOB/PA Mapping
**										Solution:  Provide LOB and Program Area mappings 
**										in the Proposal Metrics report where:
**										1) The tracking ID Lisa identified for
**										 mapping = the first 10 characters of the 
**										tracking ID in the report, and
**										2) The tracking ID in the report 
**										has a Submitted Date in 2013.
**		4/25/14		dcanuso				Dates need to be set at 0:00:00 so casting as Date
**		6/19/14		dcanuso				Task 29789 Proposal.ProposalTrackingID
**										going from beginning with 
**										2014 to beginning with 14
**		9/19/14		dcanuso				Adding new columns:
**										ProposalLocationID
**										BOEToolName
**										PricingToolName
**										ProposalLocationName
**										Columns are entered when user
**										selects other from LU table
**		11/20/14	dcanuso				New Proposal Set Up role - Renamed to 
**										reuse Product Line Viewer to Role XREF
**		11/03/15	dturk				Added CCPDRequired field to result set per WI 37342 and 37331
**		11/05/15	dturk				Modified CCPDRequired field to display yes/no/Unavailable for Record per WI 37342 and 37331
**		02/10/16	dturk				Added ProgramProposalStatus field to result set per WI 37631
**		1/5/17		Dusan				Removing Revisions and LMIS
**		1/5/17		pattoncr			Removing Segment
**      1/5/2017	twilson3			BOEJ-1706 Remove ICE fields
**		01/19/17	n99040				added fields TechLead, IndependentReviewer, LeadEstimator, ProposalMgr, CoverSheetApprover, PricingVErification
										removed fields [2014 Line of Business], and [2014 Program Area]
**		01/19/24	n99040				removed input parameter @AddCurrentOrgMapping, and associated code(but there was no associated code!)
**		02/02/17	tglick				added new fields [Revised Submittal Date], [AbsoluteValue]
**      02/17/17	twilson3			BOEJ-1903 Add LOB Estimating Manager
**										BOEJ-1905 Remove IS&GS from Total Price column
**		3/22/2017	twilson3			BOEJ-1909 Remove Profit Tracking #
**		4/12/2017	ranzalon			BOEJ-2096 Update Line Of Business to use correct table (Product Line)
**		10/18/2017	Dusan				BOEJ-2652 Add Proposal Class field to the report
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
		SELECT ProductLineID FROM dbo.ProductLine
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
SELECT LEFT(ProposalTrackingID, 10)
FROM dbo.Proposal
WHERE LEN(LEFT (ProposalTrackingID, CHARINDEX ('-',ProposalTrackingID) -1)) = 4/*To Support 4 Digit Dates*/
GROUP BY LEFT(ProposalTrackingID, 10)
UNION
SELECT LEFT(ProposalTrackingID, 8)
FROM dbo.Proposal
WHERE LEN(LEFT (ProposalTrackingID, CHARINDEX ('-',ProposalTrackingID) -1)) = 2/*To Support 2 Digit Dates*/
GROUP BY LEFT(ProposalTrackingID, 8)

/*
FOR TESTING:
SELECT * FROM @tblProposalStatus
SELECT * FROM @tblYear
SELECT * FROM @tblLineOfBusiness
SELECT * FROM @tblLeadEstimator
*/
SELECT V.[ProposalID]
      ,V.[DateCreated]
      ,V.[Year]
      ,V.[LineOfBusinessID]
      ,V.[ProductLineID]
      ,V.[Product Line]
      ,V.[Product Line / LOB]
      ,V.[Tracking #]
      ,V.[Proposal Title]
      ,V.[IWTA]
      ,V.[IWTA Submitted Value]
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
				WHEN LEN (DATEPART(MM, [PCE Proposal Review Complete Date])) = 1 
					THEN '0' + CAST (DATEPART(MM, [PCE Proposal Review Complete Date]) AS CHAR(1)) 
				ELSE 
						CAST (DATEPART(MM, [PCE Proposal Review Complete Date]) AS CHAR(2))
			END  + '/' + 
			CASE 
				WHEN LEN (DATEPART(DD, [PCE Proposal Review Complete Date])) = 1 
					THEN '0' + CAST (DATEPART(DD, [PCE Proposal Review Complete Date]) AS CHAR(1)) 
				ELSE 
						CAST (DATEPART(DD, [PCE Proposal Review Complete Date]) AS CHAR(2))
			END  + '/' + 
			CAST (DATEPART(YYYY, [PCE Proposal Review Complete Date]) AS CHAR(4))
      			AS varchar (10))	
    		 + ' ' +
			RIGHT ([PCE Proposal Review Complete Date], 7) 
		AS  [PCE Proposal Review Complete Date]       
      
      
      ,V.[LMLaborHours]
      ,V.[LMLaborCost]
      ,V.[SubcontractorCost]
      ,V.[MaterialCost]
      ,V.[IWTACost]
      ,V.[TravelCost]
      ,V.[OtherDirectCost]
      ,V.[Profit/Fee + COM]
      ,V.[Total Price]
      ,V.[ROS %]
      ,V.[Submitted Date]
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
      ,V.[Negotiation Start Date]
      ,V.[Negotiation End Date]
      ,V.[Award Date]
      ,V.[RFP Issued Date]
      ,V.[RFP Received Date]
      ,V.[UCA]
      ,V.[Audit Entrance Conference Date]
      ,V.[Audit Report Date]
      ,V.[Proposal Inadequate]
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
	 ,V.ProgramProposalStatus
	 ,V.[AbsoluteValue]
	 ,V.[Revised Submittal Date]
	 ,V.[Proposal Class]
FROM [dbo].[vwProposalLogReport] V
	LEFT OUTER JOIN @MaxRev M ON 
		(
			LEFT(V.[Tracking #],10) = M.MainProposalTrackingID AND
			LEN (M.MainProposalTrackingID) = 10
		) OR
		(
			LEFT(V.[Tracking #],8) = M.MainProposalTrackingID AND
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
					[ProductLineID] IN (SELECT LineOfBusinessID FROM @tblLineOfBusiness) 
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
			CAST([Submitted Date] AS DATE) > = @SubmitStartDate AND
			CAST([Submitted Date] AS DATE) < = @SubmitEndDate			
			
			
			
		)  OR
		
		(
			(
				@TrackingNumber IS NOT NULL AND
				[Tracking #] LIKE @TrackingNumber + '%'
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
					ProductLineID IN
						(
							SELECT ProductLineID
							FROM dbo.ProductLineRoleXREF X
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

CREATE PROCEDURE [dbo].[deleteAttachment]
(
@AttachmentID int,
@UpdateDate datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteAttachment]
**		Desc: Delete Attachment
**			
**		
**
**		Auth: twilson3
**		Date: 9/7/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[Attachment] WHERE ID = @AttachmentID ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.[Attachment] WHERE ID = @AttachmentID
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Attachment with ID ' + CAST(@AttachmentID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO

/*
    File: \Stored Procedures\deletegenTRACUser.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\deletegenTRACUser.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deletegenTRACUser]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deletegenTRACUser];

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
    File: \Stored Procedures\deleteProposal.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteProposal.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposal]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposal];

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
**
**		Auth: Don Canuso
**		Date: 04/3/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**      12/21/2016	twilson3			BOEJ-1143 Approval Emailer -- remove old Email tables/stored procs
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not display technical details to the user
**		9/7/2017	twilson3			BOEJ-2459 Add Attachment table
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.Attachment WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalUserRole WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalContractTypeXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalCostElementXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalPARChecklistXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalPPRChecklistXREF WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalChecklist WHERE ProposalID = @ProposalID
			DELETE FROM dbo.ProposalChecklistComplete WHERE ProposalID = @ProposalID
			DELETE FROM dbo.Proposal WHERE ProposalID = @ProposalID
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
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
    File: \Stored Procedures\deleteProposalClass.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteProposalClass.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProposalClass]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProposalClass];
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
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not display technical details to the user
******************************************************************************/
SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDT FROM dbo.SystemUserRole WHERE SystemUserRoleID = @SystemUserRoleID) = @UpdateDT
	BEGIN
			
		DELETE FROM dbo.ProductLineRoleXREF			
			FROM dbo.ProductLineRoleXREF X
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
    File: \Stored Procedures\getMyFunctionalTree.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\getMyFunctionalTree.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getMyFunctionalTree]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getMyFunctionalTree];
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

CREATE PROCEDURE [dbo].[getMyProposals]
(
@ProposalStatusID int = NULL,
@AssignedStart datetime2(7) = NULL,
@AssignedEnd datetime2(7) = NULL,
@Search varchar (250) = NULL,
@NTID varchar (1000) = NULL,
@genTracUserID INT = NULL
)
AS
/******************************************************************************
**          
**          Name: [getMyProposals]
**          Desc: Mimics My Proposal Page
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
**          1/5/2017	twilson3				BOEJ-1693 Show proposals for LOB Viewer permissions for Users (does not include AD Groups)
**			01/19/2017	n99040					added fields TechLead, IndependentReviewer, LeadEstimator, ProposalMgr, CoverSheetApprover, PricingVErification
**			01/25/2017	tglick					BOEJ-1766 Lead Estimator is not populating correctly
**			02/02/2017	tglick					added new fields [Revised Submittal Date], [AbsoluteValue]
**			3/7/2017	twilson3				BOEJ-1942, BOEJ-1943 Fix permissions for who can see proposals on homepage
**			11/29/17	pattoncr				Remove domain.
******************************************************************************/
SET NOCOUNT ON 

/*
TESTING
DECLARE @ProposalStatus int /*NULL MEANS ALL*/--=1
DECLARE @AssignedStart datetime2(7)--='01-01-2014'
DECLARE @AssignedEnd datetime2(7)--='02-28-2014'
DECLARE @Search varchar (MAX) = 'Nia'
DECLARE @ManagerNTID varchar (100) = 'sipiak'
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



IF @Search IS NOT NULL
	SET @Search = '%' + @Search + '%'



/*We want this employee and everyone under this employee*/
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

--3105
SELECT DISTINCT
P.ProposalID AS ProposalID,
P.ProposalTrackingID AS [Tracking Number],
P.ProposalTitle AS [Proposal Title],
LOB.LineOfBusinessName AS [Program Area],
P.Customer AS [Customer],
P.EstimatedProposalValue AS [Estimated Value],
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
P.AnticipatedDeliveryDate AS [Estimated Ship Date (Due Date)],
CAST(PC.ProposalSubmittalDate AS DATE) AS [Proposal Submit Date],
S.ProposalStatus AS [Proposal Status],
P.RevisedSubmittalDate AS [Revised Submittal Date]

FROM dbo.Proposal P 
	INNER JOIN dbo.LineOfBusiness LOB ON P.LineOfBusinessID = LOB.LineOfBusinessID

	INNER JOIN dbo.ProposalStatusLU S ON P.ProposalStatusID = S.ProposalStatusID

	INNER JOIN dbo.ProposalUserRole PUR ON P.ProposalID = PUR.ProposalID
	
	LEFT OUTER JOIN dbo.ProposalChecklist PC ON P.ProposalID = PC.ProposalID

		LEFT OUTER JOIN
		(
			SELECT PUR.ProposalID,U.DisplayName
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.RoleLU R ON PUR.RoleID = R.RoleID
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				/*11  Peer Reviewer*/
			PUR.RoleID = 11
		) PeerReviewer ON P.ProposalID = PeerReviewer.ProposalID

		LEFT OUTER JOIN
		(
			SELECT PUR.ProposalID,U.DisplayName
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.RoleLU R ON PUR.RoleID = R.RoleID
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				/*1	Capture Manager*/
			PUR.RoleID = 1
		) CaptureManager ON P.ProposalID = CaptureManager.ProposalID


		LEFT OUTER JOIN 
		(
			SELECT 
				PUR.ProposalID,
				U.DisplayName AS [Cost Volume Lead],
				U.UserID AS [Cost Volume Lead UserID],
				U.NTID AS [Cost Volume Lead NTID]
			FROM dbo.ProposalUserRole PUR
				INNER JOIN dbo.genTRACUser U ON PUR.UserID = U.UserID
			WHERE RoleID = 2 /*Cost Volume Lead*/
		) [CostVolumeLead] ON P.ProposalID = [CostVolumeLead].ProposalID


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
		LEFT JOIN 
		(
			SELECT
				PUR.ProposalID,	U.DisplayName, U.UserID, U.NTID
			FROM ProposalUserRole PUR
				JOIN genTRACUser U ON PUR.UserID = U.UserID
			WHERE
				PUR.RoleID = 3
		) LeadEstimator ON P.ProposalID = LeadEstimator.ProposalID

	LEFT OUTER JOIN
		(
			SELECT 
				ProposalID,
				MAX(SubmitDate) AS MaxSubmitDate
			FROM dbo.ProposalChecklistComplete
			GROUP BY ProposalID
		) [ProposalReviewCompleteDate] ON P.ProposalID = [ProposalReviewCompleteDate].ProposalID


WHERE
(
	(
		(@ProposalStatusID IS NULL AND P.ProposalStatusID IN (1/*In Progress*/,2/*Completed*/)) 
				OR
		(@ProposalStatusID IS NOT NULL AND P.ProposalStatusID = @ProposalStatusID)
	) AND
		(
			(
				@ProposalStatusID = 1/*In Progress*/ AND
				(@AssignedStart IS NULL OR CAST (P.DateAssigned AS Date) > = @AssignedStart) AND
				(@AssignedEnd IS NULL OR CAST (P.DateAssigned AS Date) < = @AssignedEnd) 
			) OR
			(
				 P.ProposalStatusID = 2/*Completed*/ AND
				(@AssignedStart IS NULL OR CAST ([ProposalReviewCompleteDate].MaxSubmitDate AS Date) > = @AssignedStart) AND
				(@AssignedEnd IS NULL OR CAST ([ProposalReviewCompleteDate].MaxSubmitDate AS Date) < = @AssignedEnd) 
			) OR				 
			(
				@ProposalStatusID IS NULL AND
					(
						(@AssignedStart IS NULL OR CAST (P.DateAssigned AS Date) > = @AssignedStart) AND
						(@AssignedEnd IS NULL OR CAST (P.DateAssigned AS Date) < = @AssignedEnd) 
					) OR
					(
						(@AssignedStart IS NULL OR CAST ([ProposalReviewCompleteDate].MaxSubmitDate AS Date) > = @AssignedStart) AND
						(@AssignedEnd IS NULL OR CAST ([ProposalReviewCompleteDate].MaxSubmitDate AS Date) < = @AssignedEnd) 
					) 
			)
				 
		)
) AND
/*Permissions*/
	(
		(PUR.UserID = @genTracUserID ) 
		OR
			(
				PUR.RoleID IN (3 /*Pricer*/, 2 /*Cost Volume Lead*/, 18 /*Cover Sheet Approver*/,19 /*Pricing Verification*/,11 /*Independent Reviewer*/,20 /*LOB Estimating Lead/Mgr*/,4 /*Additional Pricing Resource 1*/,5 /*Additional Pricing Resource 2*/,12 /*Backup Lead Estimator*/) AND
				PUR.UserID IN
					(
						SELECT UserID FROM dbo.genTRACUser U
							INNER JOIN MyCTE C ON 
								U.NTID = C.NTID
					)
			)
	) AND
/*Search Capability*/
	(
		@Search IS NULL
		OR
			(
				@Search IS NOT NULL 
				AND 
				(
					P.ProposalTrackingID LIKE @Search OR
					P.ProposalTitle  LIKE @Search OR
					LOB.LineOfBusinessName  LIKE @Search OR
					P.Customer  LIKE @Search OR
					CAST (P.EstimatedProposalValue AS varchar (MAX))  LIKE @Search OR
					CaptureManager.DisplayName  LIKE @Search OR
					LeadEstimator.DisplayName  LIKE @Search OR
					CostVolumeLead.[Cost Volume Lead]  LIKE @Search OR
					CAST (P.DateAssigned AS varchar (100))  LIKE @Search OR
					CAST (P.AnticipatedDeliveryDate AS varchar (100)) LIKE @Search OR
					CAST (PC.ProposalSubmittalDate AS varchar (100))  LIKE @Search OR
					S.ProposalStatus  LIKE @Search 
				)
		)
	)

GO

GRANT EXECUTE ON OBJECT::dbo.getMyProposals TO generationReporter;
GO

/*
    File: \Stored Procedures\insertProposalUserRole.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\insertProposalUserRole.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertProposalUserRole]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertProposalUserRole];
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

CREATE PROCEDURE [dbo].[insertSystemUserRole]
(
	@UserID int,
	@RoleID int,
	@ProductLineID varchar(100)
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
**		1/12/2017	gbrunwo				BOEJ-1688 Update PTM SPs to not display technical details to the user
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
		
		
		
		IF @ProductLineID IS NOT NULL AND 
			@RoleID IN 
				(
					SELECT RoleID FROM dbo.RoleLU 
					WHERE Role IN ('Viewer', 'Proposal Setup Administrator')
				)
				
			BEGIN
			/*
			Process ProductLine
			*/
			IF RIGHT(@ProductLineID, 1) <> ','
				SET @ProductLineID = @ProductLineID + ','

			DECLARE @ProductLine TABLE (ProductLineID INT)


			WHILE (SELECT CHARINDEX (',', @ProductLineID) ) > 1
			BEGIN
	
			INSERT INTO @ProductLine
			SELECT LEFT (@ProductLineID, CHARINDEX (',', @ProductLineID) -1)
			SET @ProductLineID = RIGHT (@ProductLineID, LEN (@ProductLineID) - CHARINDEX (',', @ProductLineID) )
			;


			WITH [Target] AS 
				(
					SELECT 
						SystemUserRoleID AS SystemUserRoleID,
						ProductLineID AS ProductLineID
					FROM [dbo].[ProductLineRoleXREF]
					WHERE SystemUserRoleID = @SystemUserRoleID
				)
			MERGE INTO [Target]
			USING	(
					SELECT DISTINCT 
						@SystemUserRoleID AS SystemUserRoleID, 
						ProductLineID AS ProductLineID
					FROM @ProductLine
					)  AS [Source] ON
					[Target].[SystemUserRoleID] = [Source].[SystemUserRoleID] AND
					[Target].[ProductLineID] = [Source].[ProductLineID]
			WHEN NOT MATCHED BY SOURCE 
			THEN 
			DELETE	

			WHEN NOT MATCHED BY TARGET THEN
			INSERT (SystemUserRoleID, ProductLineID)
			VALUES ([Source].[SystemUserRoleID], [Source].[ProductLineID])
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
**		To avoid confusion.. here's some info about how names changed recently (2016ish)..
**
**			OLD           -> NEW
**			ProductLine   -> LOB
**			LOB           -> Program Area
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
			PL.ProductLineName
FROM @tblLineOfBusiness tLOB
	INNER JOIN dbo.ProductLine PL ON PL.ProductLineID = tLOB.LineOfBusinessID
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
    File: \Stored Procedures\rsProposalStatus.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\rsProposalStatus.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsProposalStatus]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsProposalStatus];
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
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

/*
WI 29822
Status to Complete: 
dbo.ProposalChecklist.TempProposalSubmittalDate copied to 
dbo.ProposalChecklist.ProposalSubmittalDate

Complete to InProgress
dbo.ProposalChecklist.ProposalSubmittalDate is set to NULL
*/
DECLARE @CurrentProposalStatusID int
DECLARE @NewProposalStatusID int

SELECT @CurrentProposalStatusID = ProposalStatusID
FROM dbo.Proposal 
WHERE ProposalID = @ProposalID


/*
If the proposal is not Active (i.e., Cancelled, Archived, Revision, Deleted), 
then no one should be able to unlock the checklist.
*/
IF EXISTS (
				SELECT ProposalStatusID 
				FROM dbo.Proposal 
				WHERE ProposalID = @ProposalID AND
				ProposalStatusID IN (3,4,5,6)
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
Completed States would change to InProgess
*/
UPDATE [dbo].[Proposal] 
    SET  [ProposalStatusID] = 1 /*In Progress*/
     WHERE 
          ProposalID = @ProposalID AND
          ProposalStatusID = 2 /*Completed*/


/*Update Date for the Proposal gets updated*/
UPDATE [dbo].[Proposal] 
    SET  [UpdateDate] = @UpdateDate
     WHERE 
          ProposalID = @ProposalID




		/*WI 29822
		ProposalStatusID	ProposalStatus
		1					In Progress
		2					Completed
		
		Status to Complete: 
		dbo.ProposalChecklist.TempProposalSubmittalDate copied to 
		dbo.ProposalChecklist.ProposalSubmittalDate

		Complete to InProgress
		dbo.ProposalChecklist.ProposalSubmittalDate is set to NULL
		*/
		SELECT @NewProposalStatusID = ProposalStatusID
		FROM dbo.Proposal 
		WHERE ProposalID = @ProposalID
		
		IF (@CurrentProposalStatusID <> 2 OR @CurrentProposalStatusID IS NULL) AND @NewProposalStatusID = 2
		BEGIN
			UPDATE dbo.ProposalChecklist
			SET ProposalSubmittalDate = TempProposalSubmittalDate
			WHERE
			ProposalID = @ProposalID
		END

	IF (@CurrentProposalStatusID = 2) AND @NewProposalStatusID = 1
		BEGIN
			UPDATE dbo.ProposalChecklist
			SET ProposalSubmittalDate = NULL
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
    File: \Stored Procedures\updateProposalInformation.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\updateProposalInformation.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProposalInformation]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProposalInformation];
GO

CREATE PROCEDURE [dbo].[updateProposalInformation]
(
      @ProposalID [int],
      @UpdateDate [datetime2](7),
      @ProposalStatusID [int],
      @ProposalSubmittalDate [date],
      @ISGSTotalPrice [bigint],
      @PricerChecklistSubmittalDate [date],
      @PeerChecklistSubmittalDate [date]
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
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

/*
WI 29822
Status to Complete: 
dbo.ProposalChecklist.TempProposalSubmittalDate copied to 
dbo.ProposalChecklist.ProposalSubmittalDate

Complete to InProgress
dbo.ProposalChecklist.ProposalSubmittalDate is set to NULL
*/

DECLARE @CurrentProposalStatusID int
DECLARE @NewProposalStatusID int

SELECT @CurrentProposalStatusID = ProposalStatusID
FROM dbo.Proposal 
WHERE ProposalID = @ProposalID



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
							,[TempProposalSubmittalDate] =	ISNULL(@ProposalSubmittalDate, [TempProposalSubmittalDate])
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

		/*WI 29822
		ProposalStatusID	ProposalStatus
		1					In Progress
		2					Completed
		
		Status to Complete: 
		dbo.ProposalChecklist.TempProposalSubmittalDate copied to 
		dbo.ProposalChecklist.ProposalSubmittalDate

		Complete to InProgress
		dbo.ProposalChecklist.ProposalSubmittalDate is set to NULL
		*/
		SELECT @NewProposalStatusID = ProposalStatusID
		FROM dbo.Proposal 
		WHERE ProposalID = @ProposalID
		
		IF (@CurrentProposalStatusID <> 2 OR @CurrentProposalStatusID IS NULL) AND @NewProposalStatusID = 2
		BEGIN
			UPDATE dbo.ProposalChecklist
			SET ProposalSubmittalDate = TempProposalSubmittalDate
			WHERE
			ProposalID = @ProposalID
		END

	IF (@CurrentProposalStatusID = 2) AND @NewProposalStatusID = 1
		BEGIN
			UPDATE dbo.ProposalChecklist
			SET ProposalSubmittalDate = NULL
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
	PageNumber varchar (50) DEFAULT NULL 
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

UPDATE @Checklist
	SET StringToProcess = StringToProcess + '~'
WHERE RIGHT (StringToProcess, 1) <> '~'

UPDATE @Checklist
	SET ChecklistID = 
		LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1)
	
UPDATE @Checklist
	SET StringToProcess = 
		RIGHT (StringToProcess, LEN (StringToProcess) - CHARINDEX ('~', StringToProcess))

UPDATE @Checklist
	SET ResponseID = 
		LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1)

UPDATE @Checklist
	SET StringToProcess = 
		RIGHT (StringToProcess, LEN (StringToProcess) - CHARINDEX ('~', StringToProcess))

UPDATE @Checklist
	SET Comment = 
		IsNull(LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1), NULL)

UPDATE @Checklist
	SET StringToProcess = 
		RIGHT (StringToProcess, LEN (StringToProcess) - CHARINDEX ('~', StringToProcess))

UPDATE @Checklist
	SET PageNumber = 
		IsNull(LEFT (StringToProcess, CHARINDEX ('~', StringToProcess) -1), NULL)

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
	PageNumber = C.PageNumber
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
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

/*
WI 29822
Status to Complete: 
dbo.ProposalChecklist.TempProposalSubmittalDate copied to 
dbo.ProposalChecklist.ProposalSubmittalDate

Complete to InProgress
dbo.ProposalChecklist.ProposalSubmittalDate is set to NULL
*/

DECLARE @CurrentProposalStatusID int
DECLARE @NewProposalStatusID int

SELECT @CurrentProposalStatusID = ProposalStatusID
FROM dbo.Proposal 
WHERE ProposalID = @ProposalID



            IF (SELECT UpdateDate FROM [dbo].[Proposal] WHERE ProposalID = @ProposalID) = @UpdateDate
                  BEGIN
                  
                  SET @UpdateDate = GETDATE()
                              
                  UPDATE [dbo].[Proposal]
                        SET  [UpdateDate] = @UpdateDate
                          ,[ProposalStatusID] = @ProposalStatusID
                         WHERE 
                              ProposalID = @ProposalID



				/*WI 29822
				ProposalStatusID	ProposalStatus
				1					In Progress
				2					Completed
				
				Status to Complete: 
				dbo.ProposalChecklist.TempProposalSubmittalDate copied to 
				dbo.ProposalChecklist.ProposalSubmittalDate

				Complete to InProgress
				dbo.ProposalChecklist.ProposalSubmittalDate is set to NULL
				*/
				SELECT @NewProposalStatusID = ProposalStatusID
				FROM dbo.Proposal 
				WHERE ProposalID = @ProposalID
				
				IF (@CurrentProposalStatusID <> 2 OR @CurrentProposalStatusID IS NULL) AND @NewProposalStatusID = 2
				BEGIN
					UPDATE dbo.ProposalChecklist
					SET ProposalSubmittalDate = TempProposalSubmittalDate
					WHERE
					ProposalID = @ProposalID
				END

			IF (@CurrentProposalStatusID = 2) AND @NewProposalStatusID = 1
				BEGIN
					UPDATE dbo.ProposalChecklist
					SET ProposalSubmittalDate = NULL
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
	  @ProposalID [int]
)
AS
/******************************************************************************
**          
**          Name: [upsertAttachment]
**          Desc: Insert/Update Attachment
**                
**          
**
**          Auth: twilson3
**          Date: 9/7/2017
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                       Description:
**          --------    --------                ---------------------------------------
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF @AttachmentID  < 0  /*Insert Record*/
	BEGIN
		DECLARE @Inserted AS Table (ID int)
		SET @UpdateDate = GETDATE()

	INSERT INTO [dbo].[Attachment]
		([UpdateDate]
		,[Name]
		,[Contents]
		,[UploadedBy]
		,[AttachmentType]
		,[ProposalID]
		)
	OUTPUT inserted.ID INTO @Inserted
	VALUES
		(@UpdateDate
		,@Name
		,@Contents
		,@UploadedBy
		,@AttachmentType
		,@ProposalID
		)

		SELECT @AttachmentID = ID FROM @Inserted
	END
ELSE
	/*Update*/
	BEGIN
		IF (SELECT UpdateDate FROM [dbo].[Attachment] WHERE ID = @AttachmentID AND ProposalID = @ProposalID AND [AttachmentType] = @AttachmentType) = @UpdateDate
			BEGIN
				SET @UpdateDate = GETDATE()
							  
				UPDATE [dbo].[Attachment]
					SET  [UpdateDate] = @UpdateDate
						,[Name] = @Name
						,[Contents] = @Contents
						,[UploadedBy] = @UploadedBy
						WHERE ID = @AttachmentID;
			END
		ELSE
			BEGIN
				SET @ErrorMessage =   'The Attachment with ID ' + CAST(@AttachmentID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
						@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
				RETURN
			END
	END

IF @@ERROR = 0
	SELECT @AttachmentID as AttachmentID

GO

/*
    File: \Stored Procedures\upsertgenTRACUser.proc.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertgenTRACUser.proc.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertgenTRACUser]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertgenTRACUser];
GO

/****** Object:  StoredProcedure [dbo].[upsertgenProfitUser]    Script Date: 04/03/2013 18:49:35 ******/

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
@IsGroup bit
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
	  @RFPNumber [varchar](40),
	  @ProductLineID [int],
	  @LineOfBusinessID [int],
	  @PricingToolID [int],
	  @BOEToolID [int],
	  @AnticipatedDeliveryDate [date],
	  @ProposalCostElementXREF [varchar] (50),
	  @EstimatedProposalValue [bigint],
	  @ProposalContractTypeXREF [varchar] (50),
	  @DateAssigned  [bit],
	  @CreatedByUserID [int],
	  @NegotiationStartDate [date],
	  @NegotiationEndDate [date],
	  @AwardDate [date],
	  @RFPIssuedDate [date],
	  @RFPReceivedDate [date],	  
	  @UndefinitizedContractActions [bit],
	  @AuditEntranceConferenceDate [date],
	  @AuditReportDate [date],	  
	  @ProposalDeemedInadequate [bit],
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
	  @CCPDRequired [bit] = NULL
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
**          5/7/13      dcanuso                 WI 18113
**          5/13/13     dcanuso                 WI 18300
**          5/22/13     dcanuso                 Developer request: Only insert Question
**                                              and comment in XREF table
**			6/7/13		dcanuso					Date Assigned will be a bit and updated when true
**			6/7/13		dcanuso					Revision Story Added
**			9/6/13		dcanuso					WI 21791 Award Date Added
**			5/28/14		dcanuso					Proposal.ProposalTrackingID
**												going from beginning with 
**												2014 to beginning with 14
**			7/2/14		dcanuso					ProposalEMail Table Added
**			9/19/14		dcanuso					Adding new columns:
**												ProposalLocationID
**												BOEToolName
**												PricingToolName
**												ProposalLocationName
**												Columns are entered when user
**												selects other from LU table
**			7/27/15		dcanuso					International Story
**			9/10/15		dcanuso					When the checklist changes, if
**												the Proposal is completed, etc.
**												it must be unlocked and the 
**												Status goes to In Progress
**			2/10/16		dturk					WI_37360 - Added new column ProgramProposalStatus
**
**			8/17/16		pattoncr				BOEJ-1126 - Added new column ProposalClassID
**			12/13/16	twilson3				BOEJ-1634 Approval Workflow Database
**			12/21/2016	twilson3				BOEJ-1143 Approval Emailer -- remove old Email tables/stored procs
**			1/ 7/17		Dusan					Removing Revisions & LMIS
**			1/12/2017	gbrunwo					BOEJ-1688 Update PTM SPs to not 
**												display technical details to the user
**			02/02/2017	tglick					added new field [Revised Submittal Date]
**			3/20/2017	twilson3				BOEJ-1957 Move CCPD from Checklist to Proposal
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
/*
<ProposalID> +� � � + <Proposal Title>

Proposal ID is (<YYYY>-<5-digit sequence starting at 00001><Rev #) 
2013-10001Rnn
*/
IF @ProposalID  < 0  /*Insert Record*/
	BEGIN
		DECLARE @Inserted AS Table (ID int)
		DECLARE @ProposalTrackingID varchar (13), @ProposalTrackingGeneratorID int
		SELECT TOP 1 
				@ProposalTrackingID = CAST(ProposalYear AS varchar(4)) + '-' + CASE LEN(ProposalNumber) 
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
		SET @ProposalTrackingID =	RIGHT(@ProposalTrackingID, (LEN(@ProposalTrackingID)-2))
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
		,[ProductLineID]
		,[LineOfBusinessID]
		,[PricingToolID]
		,[BOEToolID]
		,[AnticipatedDeliveryDate]
		,[EstimatedProposalValue]  
		,[IsActive]
		,[CreatedByUserID]
		,[NegotiationStartDate]
		,[NegotiationEndDate]
		,[AwardDate]
		,[RFPIssuedDate]		   
		,[RFPReceivedDate]		   
		,[UndefinitizedContractActions]		   
		,[AuditEntranceConferenceDate]		   
		,[AuditReportDate]		   
		,[ProposalDeemedInadequate]		   
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
		)
	OUTPUT inserted.ProposalID INTO @Inserted
	VALUES
		(@UpdateDate
		,@ProposalTrackingID
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
		,@ProductLineID
		,@LineOfBusinessID
		,@PricingToolID
		,@BOEToolID
		,@AnticipatedDeliveryDate
		,@EstimatedProposalValue
		,1 /*Active On Insert*/
		,@CreatedByUserID
		,@NegotiationStartDate
		,@NegotiationEndDate
		,@AwardDate
		,@RFPIssuedDate
		,@RFPReceivedDate
		,@UndefinitizedContractActions
		,@AuditEntranceConferenceDate		   
		,@AuditReportDate
		,@ProposalDeemedInadequate
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
		)

		SELECT @ProposalID = ID FROM @Inserted
	  
		UPDATE dbo.ProposalTrackingGenerator
		SET UsedByProposalID = @ProposalID  
		WHERE ProposalTrackingGeneratorID = @ProposalTrackingGeneratorID 
  
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
						,[ProductLineID] = @ProductLineID
						,[LineOfBusinessID] = @LineOfBusinessID
						,[PricingToolID] = @PricingToolID
						,[BOEToolID] = @BOEToolID
						,[AnticipatedDeliveryDate] = @AnticipatedDeliveryDate
						,[EstimatedProposalValue] = @EstimatedProposalValue
						,[DateAssigned] = 
										CASE WHEN @DateAssigned = 1 THEN @UpdateDate
										ELSE [DateAssigned]
										END
						--,[CreatedByUserID] = @CreatedByUserID
						,[NegotiationStartDate] = @NegotiationStartDate
						,[NegotiationEndDate] = @NegotiationEndDate
						,[AwardDate] = @AwardDate
						,[RFPIssuedDate] = @RFPIssuedDate
						,[RFPReceivedDate] = @RFPReceivedDate		   
						,[UndefinitizedContractActions]	= @UndefinitizedContractActions	   
						,[AuditEntranceConferenceDate] = @AuditEntranceConferenceDate		   
						,[AuditReportDate] = @AuditReportDate
						,[ProposalDeemedInadequate] = @ProposalDeemedInadequate		   
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

CREATE PROCEDURE [dbo].[upsertProposalChecklist]
(
	@ProposalChecklistID [int],
	@UpdateDate [datetime2](7),
	@ProposalID [int],
	@ProposalSubmittalDate [date],
	@ISGSTotalPrice [bigint],
	@ProfitFee [bigint],
	@ROSPercentage [decimal](4, 2),
	@LMLaborHours [decimal](11, 2),
	@LMLaborCost [bigint],
	@SubcontractorCost [bigint],
	@MaterialCost [bigint],
	@IWTACost [bigint],
	@TravelCost [bigint],
	@OtherDirectCost [bigint],
	@TempProposalSubmittalDate [date],
	@DeliverChecklistDFARS [bit],
	@AbsoluteValue [bigint] = NULL
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
           ,[ProfitFee]
           ,[ROSPercentage]
           ,[LMLaborHours]
           ,[LMLaborCost]
           ,[SubcontractorCost]
           ,[MaterialCost]
           ,[IWTACost]
           ,[TravelCost]
           ,[OtherDirectCost]
           ,[TempProposalSubmittalDate]
		   ,[DeliverChecklistDFARS]
		   ,[AbsoluteValue]
           )
     OUTPUT inserted.ProposalChecklistID INTO @Inserted
     VALUES
           (
             @UpdateDate
            ,@ProposalID
            ,@ProposalSubmittalDate
            ,@ISGSTotalPrice
			,@ProfitFee
			,@ROSPercentage
			,@LMLaborHours
			,@LMLaborCost
			,@SubcontractorCost
			,@MaterialCost
			,@IWTACost
			,@TravelCost
			,@OtherDirectCost
			,@TempProposalSubmittalDate
			,@DeliverChecklistDFARS
			,@AbsoluteValue
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
			  ,[ProfitFee] = @ProfitFee
			  ,[ROSPercentage] = @ROSPercentage
			  ,[LMLaborHours] = @LMLaborHours
			  ,[LMLaborCost] = @LMLaborCost
			  ,[SubcontractorCost] = @SubcontractorCost
			  ,[MaterialCost] = @MaterialCost
			  ,[IWTACost] = @IWTACost
			  ,[TravelCost] = @TravelCost
			  ,[OtherDirectCost] = @OtherDirectCost
			  ,[TempProposalSubmittalDate] = @TempProposalSubmittalDate
			  ,[DeliverChecklistDFARS] = @DeliverChecklistDFARS
			  ,[AbsoluteValue] = @AbsoluteValue
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
    File: \Stored Procedures\upsertProposalType.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertProposalType.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProposalType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProposalType];
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

PRINT '###### SCRIPT FINISHED ######';