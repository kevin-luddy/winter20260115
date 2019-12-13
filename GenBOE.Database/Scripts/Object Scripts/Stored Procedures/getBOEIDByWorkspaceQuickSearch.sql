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
**		 
**		Name: [getBOEIDByWorkspaceQuickSearch]
**		Desc: Returns BOEID using Quick Search criteria
**			
**		
**
**		Auth: Don Canuso
**		Date: 1/19/2011
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		2/1/11		dcanuso				Based on discussions with the SEs, the way
**										that this stored procedure should work is,
**										all words that are sent in need
**										to be found somewhere
**										So, as long as every word is found
**										in one of the searched fields, then it should
**										be returned.
**										So, need to look through each word.
**		2/15/11		dcanuso				Quick Search User Searches are now on Display Name
**										Initial Parameter changed
**		2/17/11		dcanuso				Added Search Category:
**										1: BOE
**										2: BOE Content Template
**										3: All
**		2/18/11		dcanuso				Rethinking Ordering
**		3/3/11		dcanuso				WBS Number is being padded for ordering
**		3/8/11		dcanuso				WBS_CLIN_BOE_XREF is back so needs to be added to SP
**										Developer will be padding words with
**										"" and separating word phrases with
**										commas (,).
**		3/21/11		dcanuso				Ability to search within your own Workspace
**										So, re-organization of Search Category Table
**										(1, N'BOEs in other Workspaces')
**										(2, N'BOE Content Template')
**										(3, N'BOEs in this Workspaces')
**										(4, N'All')
**										Thus, need to redo SP.
**										Since searching within a Workspace, 
**										Added WorkspaceID
**										For searching, since we are working on
**										incomplete workspaces when searching for
**										BOEs with your Workspace, updates from 
**										INNER to LEFT OUTER
**		5/9/11		dcanuso				Errors on strings - updated SP - changing Word limit
**										Added comments and elimated unneeded code
**		5/9/11		dcanuso				Because, in theory, a user can enter 
**										100 characters and because
**										the SQL code needs to have spaces, 
**										commas, and double quotes, I 
**										discussed with Developer and will
**										be updating parameters to allow 
**										more charcters - easiest to just double
**										Front end code will limit the input that
**										user will be able to enter
**										Good example is, the following 3-10 character
**										strings turns into 41 characters when you 
**										add commas, spaces, and double quotes
**										('"A123456789", "B123456789", "C123456789",')
**										@QuickSearch nvarchar (100) to 200
**		5/15/11		dcanuso				Issues came up with using , as delimiter
**										for Display Name, so updated SP
**										to process DisplayName differently
**										and change the delimiter to ~
**		5/17/11		dcanuso				Making an update to space processing
**		5/25/11		dcanuso				Added case for sending in *
**		6/2/11		dcanuso				Updating WorkspaceID Logic and corrected spelling error
**		8/29/11		dcanuso				WI4788 - DB - update BOE quick and advanced search SPs - These 
**										should never return any BOEs that are of type DTS.
**		10/31/11	dcanuso				WI 5773: BOE DTS check should be 0 not 1 - Updated
**										BOEs in this Workspace – this is a new option available 
**										to all users who has access to the Workspace 
**										(this is the only search option available to foreign users). 
**										If this option is selected, then the current Workspace 
**										in which the Author is working is searched. 
**										In this case, the Workspace does not have to be 
**										marked Searchable, can contain OCI data and can be in any state.
**										BOEs in Unassigned state should not be searched since
**										they will have no data in them.
**										b. BOEs in Draft, Awaiting Approval and 
**										Approved states must be searchable.
**		12/19/11	dcanuso				WI 6504: Only need to ignore DTS BOEs if the 
**										workspace dts autocalculate is set to InSummaryBOEs.
**										Any BOEs from workspaces marked as "In Each BOE" 
**										and marked as DTS will need to be searched
**										Additional Clarification:
**										Interested in all cases where BOE.AutoCalculateDTS is "false" and 
**										all cases where Workspace.DTSAutoCalculateID is "In Each BOE". 
**										The only case you want to skip is the case 
**										where BOE.AutoCalculateDTS is true AND 
**										Workspace.DTSAutoCalculateID is "In Summary BOEs
**										If the DTSAutoCalculateID is "No" 
**										then it would look to the BOE.AutoCalculateDTS setting
**		8/3/12		dcanuso				WI 10412
**										As per SE:
**										I believe ContainsTemplate should not be in the 
**										search criteria when searching for BOEs in Other Workspaces. 
**										Reason: It should return BOEs regardless of whether it contains
**										a BOE Content Templates or not. 
**										The workspace should return BOEs in Other Workspaces 
**										for BOEs ContainsTemplate = 0 && ContainsTemplate = 1
**		11/12/12	dcanuso				Example
**		DECLARE @RC int
**		DECLARE @QuickSearch nvarchar(200) = '"Word~Word~Word~Word"'
**		DECLARE @SearchCategory int = 3
**		DECLARE @WorkspaceID int =78
**		
**		EXECUTE @RC = [GenBOE_main].[dbo].[getBOEIDByWorkspaceQuickSearch] 
**		   @QuickSearch
**		  ,@SearchCategory
**		  ,@WorkspaceID
**
**		11/27/12	dcanuso				WI 13214 Added:
**										@DisplayedCLINNumber varchar(200)
**										@CLINTitle varchar(200)
**										@DisplayedWBSNumber varchar(200)
**										@WBSTitle varchar(200)
**		12/5/12		dcanuso				SP now timing out - Rewrote SP
**		4/5/13		dcanuso				WI 17282 BOE Title added
**		11/11/13	dcanuso				WI 24174 Subcontractor Author
**		1/9/14		dcanuso				Received an email request from developer/SE
**										that Frank wants WS.WorkspaceStateID = 4 
**										removed from the criteria and this should be
**										dine directly on production
**      6/9/2015    dpalider            [BOEJ-124] Fixed how DTS was being filtered out. It was checking 2 values against WS setting instead of
**									    checking 1 against WS and the other one against BOE. I also added () into combo of OR/ADD and 
**										lined things up to make it easier to follow
**      9/16/2016   twilson3			BOEJ-1244 Added filtering of IsUsingEquivalentPerson, all BOE workspaces must match this on the workspace passed in
**		1/19/2017	brunworg			BOEJ-1369 - Filter results by !isSummaryBOE, matching isMaterial, and exclude specified BOEID. 
**										Also limit results returned to TOP @SearchResultsThreshold.
**		3/3/2017	twilson3			BOEJ-1861 Remove DTS
**		4/28/2017	brunworg			BOEJ-2129 - Add IsUsingTM Column to Workspace
**		5/23/2017	ranzalon			BOEJ-2143 - Exclude Project Map from search
**		6/1/2017    ranzalon			BOEJ-2143 - Fix for new Project Map LU values
**		7/19/2017	Dusan				BOEJ-2410 - Allow copying between TM and non-TM workspaces
**		12/7/17		twilson3			BOEJ-1994 - Remove Summary BOE
**		5/25/2018	ranzalon			BOEJ-3503 - Exclude Deleted Workspaces
**		7/12/2018	twilson3			BOEJ-3685 - Fix exclusion of Deleted Workspaces
*******************************************************************************/
SET NOCOUNT ON 

IF LEN(@QuickSearch) = 0
	BEGIN
		SELECT NULL AS BOEID
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
SET @IsEP = (SELECT IsUsingEquivalentPerson from dbo.Workspace where WorkspaceID = @WorkspaceID)
DECLARE @IsMaterial bit
SET @IsMaterial = (SELECT IsMaterial from dbo.BOE where BOEID = @BOEID)
DECLARE @BOESearch TABLE
(
	BOEID int PRIMARY KEY,
	Found int DEFAULT 0
)


/*Load Table with available BOEs*/
INSERT INTO @BOESearch(BOEID)
	SELECT DISTINCT (B.BOEID)
		FROM dbo.Workspace WS
			INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
		WHERE WS.IsUsingEquivalentPerson = @IsEP AND
			  (WS.IsDeleted IS NULL OR WS.IsDeleted != 1) AND /* Exclude deleted workspaces */
		      B.IsMaterial = @IsMaterial AND	/* Only search BOE's with same IsMaterial setting as specified BOE */
			  B.BOEID != @BOEID AND				/* Exclude specified BOE */
			  (WS.ProjectMapTypeID = 1 OR WS.ProjectMapTypeID = 2) AND		/* Exclude BOEs in Project Map Workspaces */
		(
			(
				@SearchCategory = 1 /*BOEs in other Workspaces*/
				AND 
				(
					WS.ContainsOCI = 0 /*Does Not Contain OCI*/
					AND WS.AllowSearch = 1 /*BOE is in searchable state*/
					AND WS.WorkspaceID <> @WorkspaceID /*If I am searching Other Workspaces I should not be returning data from my WS*/
				)
			) OR (
				@SearchCategory = 2 /*BOE Content Template*/
				AND 
				(
					WS.ContainsOCI = 0 /*Does Not Contain OCI*/
					AND WS.AllowSearch = 1 /*BOE is in searchable state*/
					AND WS.ContainsTemplate = 1
				)
			) OR (
				@SearchCategory = 3 /*BOEs in this Workspaces*/
				AND
				(
					WS.WorkspaceID = @WorkspaceID 
					AND B.BOEStateID <> 1   /*BOEs in Unassigned state should not be searched since they will have no data in them.*/
				)
			) OR (
				@SearchCategory = 4 	/*All*/
				AND	
				(
					(
						WS.ContainsOCI = 0 /*Does Not Contain OCI*/ 
						AND WS.AllowSearch = 1 ) /*BOE is in searchable state*/ 
					OR
					(	
						@WorkspaceID IS NOT NULL AND 
						WS.WorkspaceID = @WorkspaceID  AND
						B.BOEStateID <> 1  /*BOEs in Unassigned state should not be searched since they will have no data in them.*/ 
					)
				)
			)
		)

/*
	Variable to hold current Word
	Since the limit of the parameter is 100
	It is possible that 1 word is the full 100 
	character limit, so changing from 30 to 100
*/
DECLARE @CurrentWord varchar (200)

DECLARE @Found TABLE (BOEID int, WorkspaceID int)

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