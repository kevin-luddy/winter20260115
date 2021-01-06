IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getBOEIDByWorkspaceQuickSearch]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getBOEIDByWorkspaceQuickSearch];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[getBOEIDByWorkspaceQuickSearch]
(
	@QuickSearch nvarchar (200),
	@SearchCategory int,
	@WorkspaceID int,
	@BOEID int,
	@SearchResultsThreshold int
)
AS
/******************************************************************************
**		Name: [getBOEIDByWorkspaceQuickSearch]
**		Desc: Returns BOEID using Quick Search criteria
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		5/25/2018	ranzalon			BOEJ-3503 - Exclude Deleted Workspaces
**		7/12/2018	twilson3			BOEJ-3685 - Fix exclusion of Deleted Workspaces
**		9/9/2020	ranzalon			BOEJ-4770 - Only search WSs with same Template BOE value
**		11/4/2021	Dusan				BOEJ-4889 - Removing restriction to only search by the same Template BOE value
**		11/24/2020	Dusan				BOEJ-4949 - Add restrictions based on Template BOE value; did some code cleanup as well
**		12/15/2020	Dusan				BOEJ-4953 - Fixing logic w/ MOQ Type restrictions
*******************************************************************************/
SET NOCOUNT ON 


IF LEN(@QuickSearch) = 0
	BEGIN
		SELECT NULL AS BOEID
		RETURN
	END

DECLARE @WordSearch TABLE ( Word varchar(200), Processed bit DEFAULT 0 )
DECLARE @BOESearch TABLE( BOEID int PRIMARY KEY, Found int DEFAULT 0 )
DECLARE @Found TABLE (BOEID int, WorkspaceID int)
DECLARE @CurrentWord varchar (200)

IF RIGHT(@QuickSearch,1) <> '~' SET @QuickSearch = @QuickSearch + '~'

WHILE (SELECT CHARINDEX('~', @QuickSearch)) > 0
	BEGIN
		INSERT INTO @WordSearch (Word) 
			SELECT  '"' + REPLACE(RTRIM(LEFT (@QuickSearch, CHARINDEX('~', @QuickSearch)-1)), '"', '') + '"'

		SET @QuickSearch = RIGHT(@QuickSearch, LEN(@QuickSearch)- CHARINDEX('~', @QuickSearch))
	END

DECLARE @WordSearchCount int = (SELECT COUNT (1) FROM @WordSearch)
DECLARE @IsEP bit = (SELECT IsUsingEquivalentPerson from dbo.Workspace where WorkspaceID = @WorkspaceID)
DECLARE @IsMaterial bit = (SELECT IsMaterial from dbo.BOE where BOEID = @BOEID)
/* if you are starting from an Old workspace (PreNewMoq) ==> you can only copy BOEs from another Old workspace */
DECLARE @restrictToPreNewMoq BIT = CASE WHEN dbo.WasWorkspaceCreatedAfterNewMoqTypes((SELECT WorkspaceCreationDate FROM Workspace WHERE WorkspaceId = @WorkspaceID)) = 0 THEN 1 ELSE 0 END 
/* if you are starting from a * / No (Not using Boe Template) ==> you can only copy from * / No */
DECLARE @restrictToNotUsingBoeTemplate BIT = CASE WHEN (SELECT TemplateBoe FROM Workspace WHERE WorkspaceId = @WorkspaceID) = 0 THEN 1 ELSE 0 END 

INSERT INTO @BOESearch(BOEID)
	SELECT DISTINCT (B.BOEID)
		FROM dbo.Workspace WS
			INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
		WHERE WS.IsUsingEquivalentPerson = @IsEP 
			AND (WS.IsDeleted IS NULL OR WS.IsDeleted != 1) /* Exclude deleted workspaces */
		    AND B.IsMaterial = @IsMaterial /* Only search BOE's with same IsMaterial setting as specified BOE */
			AND B.BOEID != @BOEID /* Exclude specified BOE */
			AND (WS.ProjectMapTypeID = 1 OR WS.ProjectMapTypeID = 2) /* Exclude BOEs in Project Map Workspaces */
			AND	(
			   (@SearchCategory = 1 /* BOEs in other Workspaces */ AND ( WS.WorkspaceID <> @WorkspaceID AND WS.ContainsOCI = 0 AND WS.AllowSearch = 1)) /* Does Not Contain OCI, BOE is in searchable state */
				OR (@SearchCategory = 2 /*BOE Content Template*/ AND ( WS.ContainsTemplate = 1 AND WS.ContainsOCI = 0 AND WS.AllowSearch = 1)) /* Does Not Contain OCI, BOE is in searchable state */
				OR (@SearchCategory = 3 /*BOEs in this Workspaces*/ AND ( WS.WorkspaceID = @WorkspaceID AND B.BOEStateID <> 1))   /*BOEs in Unassigned state should not be searched since they will have no data in them.*/
				OR (@SearchCategory = 4 /*All*/
						AND	((WS.ContainsOCI = 0 AND WS.AllowSearch = 1 /* Does Not Contain OCI, BOE is in searchable state */ )
							OR (@WorkspaceID IS NOT NULL AND  WS.WorkspaceID = @WorkspaceID AND B.BOEStateID <> 1)  /*BOEs in Unassigned state should not be searched since they will have no data in them.*/ 
						)
					)
				)
			/* START new MOQ Types filtering */
			AND /* not restricted or (restricted and not new) => !a || (a && !b) = !a || !b */
				(@restrictToPreNewMoq = 0 OR dbo.WasWorkspaceCreatedAfterNewMoqTypes(WS.WorkspaceCreationDate) = 0) 
			AND /* not restricted or (restricted and not using template) => !a || (a && !b) = !a || !b */
				(@restrictToNotUsingBoeTemplate = 0 OR WS.TemplateBoe = 0)
			/* END new MOQ Types filtering */

WHILE EXISTS (SELECT 1 FROM @WordSearch WHERE Processed = 0)
	BEGIN
		/*Obtain First Word to Search*/
		SELECT TOP 1 @CurrentWord = Word FROM @WordSearch WHERE Processed = 0
		DELETE FROM @Found
		
		INSERT INTO @Found
			SELECT DISTINCT B.BOEID, WS.WorkspaceID
				FROM dbo.Workspace  WS
					INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
				WHERE CONTAINS(B.BOEDescription, @CurrentWord) OR CONTAINS(B.DataSource, @CurrentWord) OR CONTAINS(B.BOETitle, @CurrentWord) OR
					  CONTAINS(WS.WorkspaceName, @CurrentWord) OR CONTAINS(WS.WorkspaceDescription, @CurrentWord)

		INSERT INTO @Found
			SELECT DISTINCT B.BOEID, WS.WorkspaceID
				FROM dbo.Workspace  WS
					INNER JOIN dbo.ETIuser CostVolume ON WS.CostVolumeLeadPricerUserID = CostVolume.ETIUserID
					INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
				WHERE CONTAINS(CostVolume.DisplayName, @CurrentWord) 

		INSERT INTO @Found
			SELECT DISTINCT B.BOEID, WS.WorkspaceID
				FROM dbo.Workspace  WS
					INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
					INNER JOIN dbo.BOETaskElement TE ON B.BOEID = TE.BOEID
				WHERE CONTAINS(TE.TaskTitle, @CurrentWord) OR CONTAINS(TE.TaskDescription, @CurrentWord)

		INSERT INTO @Found
			SELECT DISTINCT B.BOEID, WS.WorkspaceID
				FROM dbo.Workspace  WS
					INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
					INNER JOIN dbo.BOETaskElement TE ON B.BOEID = TE.BOEID
					INNER JOIN dbo.BOELaborType LT ON TE.BOETaskElementID = LT.BOETaskElementID
					INNER JOIN dbo.PerformingOrganization PO ON LT.PerformingOrganizationID = PO.PerformingOrganizationID
				WHERE CONTAINS(PO.PerformingOrganizationName, @CurrentWord) 

		INSERT INTO @Found
			SELECT DISTINCT B.BOEID, WS.WorkspaceID
				FROM dbo.Workspace  WS
					INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
					INNER JOIN dbo.BOEUserRole BUR_Author ON B.BOEID = BUR_Author.BOEID AND BUR_Author.RoleID = 1 /*Author*/
					INNER JOIN dbo.ETIuser Author ON BUR_Author.ETIUserID = Author.ETIUserID
				WHERE CONTAINS(Author.DisplayName, @CurrentWord) 

		INSERT INTO @Found
			SELECT DISTINCT B.BOEID, WS.WorkspaceID
				FROM dbo.Workspace  WS
					INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
					INNER JOIN dbo.BOEUserRole BUR_Approver ON B.BOEID = BUR_Approver.BOEID AND BUR_Approver.RoleID = 3 /*Approver*/  
					INNER JOIN dbo.ETIuser Approver ON BUR_Approver.ETIUserID = Approver.ETIUserID
				WHERE CONTAINS(Approver.DisplayName, @CurrentWord) 

		/*WI 24174*/
		INSERT INTO @Found
			SELECT DISTINCT B.BOEID, WS.WorkspaceID
				FROM dbo.Workspace  WS
					INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
					INNER JOIN dbo.BOEUserRole BUR_Approver ON B.BOEID = BUR_Approver.BOEID AND BUR_Approver.RoleID = 9 /*subcontractor author*/  
					INNER JOIN dbo.ETIuser Approver ON BUR_Approver.ETIUserID = Approver.ETIUserID
				WHERE CONTAINS(Approver.DisplayName, @CurrentWord) 

		INSERT INTO @Found
			SELECT DISTINCT B.BOEID, WS.WorkspaceID
				FROM dbo.Workspace  WS
					INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
					INNER JOIN dbo.WBS_CLIN_BOE_XREF X ON B.BOEID = X.BOEID
					INNER JOIN dbo.WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID
				WHERE CONTAINS(WBS.DisplayedWBSNumber, @CurrentWord) OR CONTAINS(WBS.WBSTitle, @CurrentWord)

		INSERT INTO @Found
			SELECT DISTINCT B.BOEID, WS.WorkspaceID
				FROM dbo.Workspace  WS
					INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
					INNER JOIN dbo.WBS_CLIN_BOE_XREF X ON B.BOEID = X.BOEID
					INNER JOIN dbo.CLIN C ON X.CLINID = C.CLINID						
				WHERE CONTAINS(C.DisplayedCLINNumber, @CurrentWord) OR CONTAINS(C.CLINTitle, @CurrentWord)

		UPDATE @BOESearch
			SET Found = Found + 1
			FROM @BOESearch BS
				INNER JOIN (SELECT DISTINCT BOEID FROM @Found WHERE BOEID IS NOT NULL) F ON BS.BOEID = F.BOEID

		/*Update Words with Processed*/
		UPDATE @WordSearch 
			SET	Processed = 1
			WHERE
				Word = @CurrentWord /*Since Word has double quotes already, do not need to add REPLACE(@CurrentWord, '"', '')*/
	END						
SELECT TOP(@SearchResultsThreshold)	B.BOEID AS BOEID, WS.WorkspaceId AS WorkspaceId
	FROM dbo.Workspace WS
		INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
		INNER JOIN @BOESearch BS ON B.BOEID = BS.BOEID
		LEFT OUTER JOIN dbo.WBS_CLIN_BOE_XREF X ON B.BOEID = X.BOEID
		LEFT OUTER JOIN dbo.WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID
	WHERE @WordSearchCount = BS.Found   
	ORDER BY
		WS.WorkspaceName,
		WS.ProposalSubmitDate,
		WBS.WBSNumber;

GO