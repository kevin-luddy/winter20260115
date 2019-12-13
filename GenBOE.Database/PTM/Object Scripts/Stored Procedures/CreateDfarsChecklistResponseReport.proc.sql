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