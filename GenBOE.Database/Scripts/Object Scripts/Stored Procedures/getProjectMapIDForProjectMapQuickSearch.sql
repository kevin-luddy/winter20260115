IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getProjectMapIDForProjectMapQuickSearch]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getProjectMapIDForProjectMapQuickSearch];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[getProjectMapIDForProjectMapQuickSearch]
(
	@QuickSearch nvarchar (200),
	@WorkspaceID int,
	@SearchResultsThreshold int
)
AS
/******************************************************************************
**		 
**		Name: [getProjectMapIDForProjectMapQuickSearch]
**		Desc: BOEJ-2445 Returns ProjectMap ID using Quick Search criteria for Project Map
**			
**		
**
**		Auth: Tim Wilson
**		Date: 8/15/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		5/25/2018	ranzalon			BOEJ-3503 - Exclude Deleted Workspaces
**		7/12/2018	twilson3			BOEJ-3685 - Fix exclusion of Deleted Workspaces
*******************************************************************************/
SET NOCOUNT ON 

IF LEN(@QuickSearch) = 0
	BEGIN
		SELECT NULL AS ProjectMapId, NULL AS WorkspaceId 
		RETURN
	END

IF RIGHT(@QuickSearch,1) <> '~' SET @QuickSearch = @QuickSearch + '~'

/*Create table to hold parsed words*/
DECLARE @WordSearch TABLE
(
	Word varchar(200),
	Processed bit DEFAULT 0
)

/*Split words*/
WHILE (SELECT CHARINDEX('~', @QuickSearch)) > 0
	BEGIN
		INSERT INTO @WordSearch (Word) 
			SELECT  '"' + REPLACE(RTRIM(LEFT (@QuickSearch, CHARINDEX('~', @QuickSearch)-1)), '"', '') + '"'

		SET @QuickSearch = RIGHT(@QuickSearch, LEN(@QuickSearch)- CHARINDEX('~', @QuickSearch))
	END
	
/*Variable to hold how many words are passed in*/
DECLARE @WordSearchCount int
SELECT @WordSearchCount = COUNT (Word) FROM @WordSearch
DECLARE @IsEP bit
DECLARE @ProjectMapTypeID int
SET @ProjectMapTypeID = (SELECT ProjectMapTypeID from dbo.Workspace where WorkspaceID = @WorkspaceID)

DECLARE @BOESearch TABLE
(
	ProjectMapId int PRIMARY KEY,
	Found int DEFAULT 0
)


/*Load Table with available BOEs*/
INSERT INTO @BOESearch(ProjectMapId)
	SELECT DISTINCT (P.ID)
		FROM dbo.Workspace WS
			INNER JOIN dbo.ProjectMap P ON WS.WorkspaceID = P.WorkspaceID
		WHERE WS.ProjectMapTypeID = @ProjectMapTypeID AND	
		(
			WS.ContainsOCI = 0 /*Does Not Contain OCI*/
			AND WS.AllowSearch = 1 /*BOE is in searchable state*/
			AND (WS.IsDeleted IS NULL OR WS.IsDeleted != 1) /* Exclude deleted workspaces */
			AND WS.WorkspaceID <> @WorkspaceID /*If I am searching Other Workspaces I should not be returning data from my WS*/
		)

/*
	Variable to hold current Word
	Since the limit of the parameter is 100
	It is possible that 1 word is the full 100 
	character limit, so changing from 30 to 100
*/
DECLARE @CurrentWord varchar (200)

DECLARE @Found TABLE (ProjectMapId int, WorkspaceID int)

WHILE EXISTS (SELECT 1 FROM @WordSearch WHERE Processed = 0)
	BEGIN
		/*Obtain First Word to Search*/
		SELECT TOP 1 @CurrentWord = Word FROM @WordSearch WHERE Processed = 0
		DELETE FROM @Found
		
		INSERT INTO @Found
			SELECT DISTINCT P.ID, WS.WorkspaceID
				FROM dbo.Workspace  WS
					INNER JOIN dbo.ProjectMap P ON WS.WorkspaceID = P.WorkspaceID
				WHERE CONTAINS(P.ActivityId, @CurrentWord) OR CONTAINS(P.Rationale, @CurrentWord) OR CONTAINS(P.ActivityName, @CurrentWord) OR
					  CONTAINS(P.SOWTitle, @CurrentWord) OR CONTAINS(P.CamName, @CurrentWord) OR CONTAINS(P.Category, @CurrentWord) OR
					  CONTAINS(WS.WorkspaceName, @CurrentWord) OR CONTAINS(WS.WorkspaceDescription, @CurrentWord) OR
					  CONTAINS(P.Task, @CurrentWord) OR CONTAINS(P.WbsNumber, @CurrentWord) OR CONTAINS(P.WbsElementTitle, @CurrentWord)

		UPDATE @BOESearch
			SET Found = Found + 1
			FROM @BOESearch BS
				INNER JOIN (SELECT DISTINCT ProjectMapId FROM @Found WHERE ProjectMapId IS NOT NULL) F ON BS.ProjectMapId = F.ProjectMapId

		/*Update Words with Processed*/
		UPDATE @WordSearch 
			SET	Processed = 1
			WHERE
				Word = @CurrentWord /*Since Word has double quotes already, do not need to add REPLACE(@CurrentWord, '"', '')*/
	END						

SELECT TOP(@SearchResultsThreshold)	P.ID AS ProjectMapId, WS.WorkspaceId AS WorkspaceId
	FROM dbo.Workspace WS
		INNER JOIN dbo.ProjectMap P ON WS.WorkspaceID = P.WorkspaceID
		INNER JOIN @BOESearch BS ON P.ID = BS.ProjectMapId
	WHERE @WordSearchCount = BS.Found   
	ORDER BY
		WS.WorkspaceName,
		WS.ProposalSubmitDate,
		P.WbsNumber;

GO