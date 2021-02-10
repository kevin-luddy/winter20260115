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