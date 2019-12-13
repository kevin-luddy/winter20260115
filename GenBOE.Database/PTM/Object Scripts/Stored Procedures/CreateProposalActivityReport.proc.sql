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