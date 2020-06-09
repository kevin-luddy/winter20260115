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
**
**
**		Auth: Don Canuso
**		Date: 7/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
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
**		3/19/2018	twilson3			BOEJ-3126 Add Forecast Tracking # to SSRS
**		4/17/2018   Dusan				BOEJ-3388 Remove 2 IWTA columns and tweak how another one works
**		5/10/2018	Dusan				BOEJ-3402 Fixed labels for Workflow Submitted Date
**		5/30/2018	ranzalon			BOEJ-3405 - Adjusted naming of Actual Submittal Date
**		6/06/2018	brunworg			BOEJ-3480 Renamed ProductLine and LineOfBusiness tables.
**		8/30/2018	Dusan				BOEJ-3757 SSRS Updates w/ Post Proposal Changes
**		9/27/2018	ranzalon			BOEJ-3741 - Classified Cost Volume
**		8/5/2019	twilson3			BOEJ-4274 Add LOB Manager Comments
**		4/22/2020	ranzalon			BOEJ-4535 Add Lead Estimator Approval Date
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