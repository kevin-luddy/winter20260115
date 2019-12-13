IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getBOEIDByWorkspaceAdvancedSearch]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getBOEIDByWorkspaceAdvancedSearch];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[getBOEIDByWorkspaceAdvancedSearch]
(
@WorkspaceName nvarchar(215),
@WorkspaceDescription nvarchar(200), 
@ProposalSubmitStartDate DATETIME2,
@ProposalSubmitEndDate DATETIME2,
@RFPNumber nvarchar(200),
@BOEDescription nvarchar(200),
@TaskTitle nvarchar(200),
@TaskDescription nvarchar(200),
@DataSource nvarchar(200),
@PerformingOrganization nvarchar(200),
@CostVolume varchar(200),
@Author varchar(200),
@Approver varchar(200),
@DisplayedCLINNumber varchar(200),
@CLINTitle varchar(200),
@DisplayedWBSNumber varchar(200),
@WBSTitle varchar(200),
@BOETitle varchar(200),
@SearchCategory int,
@WorkspaceID int,
@BOEID int,
@SearchResultsThreshold int
)
AS
/******************************************************************************
**		 
**		Name: [getBOEIDByWorkspaceAdvancedSearch]
**		Desc: Returns BOEID using Advanced Search criteria
**			
**		
**
**		Auth: Don Canuso
**		Date: 1/19/2011
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		2/1/11		dcanuso				Based on discussions with the SEs, the way
**										that this stored procedure should work is,
**										all words that are sent in need
**										to be found in the same field with an AND
**										So, AND between words that are sent in
**										Application will add " and  AND between
**										the word, so if @Paramter = 'Yellow Blue',
**										the application will pass in
**										'"Yellow" AND "Blue"'  (Terms in ' " " ')
**										Wildcards (' " Term * " ')				
**		2/15/11		dcanuso				Searching for people changed from ID to DisplayName
**		2/17/11		dcanuso				Added Search Category:
**										1: BOE
**										2: BOE Content Template
**										3: All
**		2/23/11		dcanuso				Developer advised that ID 
**										can not be NULL - So, they will be ''
**										coming from the code
**		3/3/11		dcanuso				WBS Number is being padded for ordering
**		3/8/11		dcanuso				WBS_CLIN_BOE_XREF is back so needs to be added to SP
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
**										@WorkspaceName nvarchar(100) to 200
**										@WorkspaceDescription nvarchar(100) to 200
**										@RFPNumber nvarchar(100) to 200
**										@BOEDescription nvarchar(100) to 200
**										@TaskTitle nvarchar(100) to 200
**										@TaskDescription nvarchar(100) to 200
**										@DataSource nvarchar(100) to 200
**										@PerformingOrganization nvarchar(100) to 200
**										@CostVolume varchar(40) to 80
**										@Author varchar(40) to 80
**										@Approver varchar(40) to 80
**		5/15/11		dcanuso				Developer requested user parameters changed to 200
**		5/15/11		dcanuso				WI 3459 Advanced search is returning duplicate BOEs.
**										It appears to be returning as many dups as there are task for that BOE
**										Added Temp table to process results differently
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
**		3/9/12		dcanuso				WI 7912 Workspace should be searchable in any state
**		3/12/12		dcanuso				Backing out WI 7912 - Wrong SP Change
**		8/9/12		dcanuso				Bug #10631: [Manage BOE > Import] Importing updates to existing
**										BOE from Non-Material to Material should have a Material task 
**										automatically created.
**		11/12/12	dcanuso				Example:
**										DECLARE @WorkspaceName nvarchar(200) ='"Word~Word~Word~Word"'
**										Empty parameters should be passed in as NULL
**		11/27/12	dcanuso				WI 13214 Added:
**										@DisplayedCLINNumber varchar(200)
**										@CLINTitle varchar(200)
**										@DisplayedWBSNumber varchar(200)
**										@WBSTitle varchar(200)
**		4/5/13		dcanuso				WI 17282 BOE Title Added by Space
**		9/27/13		dcanuso				Task 22684:Increase Workspace Name to 115
**										So increasing this SP by 15 as well
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

SELECT
	@WorkspaceName = IsNULL(@WorkspaceName, '""'),
	@WorkspaceDescription = IsNULL(@WorkspaceDescription, '""'), 
	@RFPNumber = IsNULL(@RFPNumber, '""'),
	@BOEDescription = IsNULL(@BOEDescription, '""'),
	@TaskTitle = IsNULL(@TaskTitle, '""'),
	@TaskDescription = IsNULL(@TaskDescription, '""'),
	@DataSource = IsNULL(@DataSource, '""'),
	@PerformingOrganization = IsNULL(@PerformingOrganization, '""'),
	@CostVolume = IsNULL(@CostVolume, '""'),
	@Author = IsNULL(@Author, '""'),
	@Approver = IsNULL(@Approver, '""'),
	@DisplayedCLINNumber = IsNULL(@DisplayedCLINNumber, '""'),
	@CLINTitle = IsNULL(@CLINTitle, '""'),
	@DisplayedWBSNumber = IsNULL(@DisplayedWBSNumber, '""'),
	@WBSTitle = IsNULL(@WBSTitle, '""'),
	@BOETitle = IsNULL (@BOETitle, '""')

DECLARE @IsEP bit
SET @IsEP = (SELECT IsUsingEquivalentPerson from dbo.Workspace where WorkspaceID = @WorkspaceID)
DECLARE @IsMaterial bit
SET @IsMaterial = (SELECT IsMaterial from dbo.BOE where BOEID = @BOEID)

DECLARE @Results TABLE
	(
		OrderID int Identity(1,1) NOT NULL,
		BOEID int NOT NULL,
		WorkspaceId int NOT NULL
	)		

INSERT INTO @Results (BOEID, WorkspaceID)	
	SELECT	B.BOEID, WS.WorkspaceID
		FROM dbo.Workspace WS
			INNER JOIN dbo.ETIuser CostVolume ON WS.CostVolumeLeadPricerUserID = CostVolume.ETIUserID
			INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
			LEFT OUTER JOIN dbo.BOETaskElement TE ON B.BOEID = TE.BOEID
			LEFT OUTER JOIN dbo.BOELaborType LT ON TE.BOETaskElementID = LT.BOETaskElementID
			LEFT OUTER JOIN dbo.PerformingOrganization PO ON LT.PerformingOrganizationID = PO.PerformingOrganizationID
			LEFT OUTER JOIN dbo.BOEUserRole BUR_Author ON B.BOEID = BUR_Author.BOEID AND BUR_Author.RoleID IN (1, /*Author*/9	/*Subcontractor Author*/)
			LEFT OUTER JOIN dbo.ETIuser Author ON BUR_Author.ETIUserID = Author.ETIUserID
			LEFT OUTER JOIN dbo.BOEUserRole BUR_Approver ON B.BOEID = BUR_Approver.BOEID AND BUR_Approver.RoleID = 3 /*Approver*/
			LEFT OUTER JOIN dbo.ETIuser Approver ON BUR_Approver.ETIUserID = Approver.ETIUserID
			LEFT OUTER JOIN dbo.WBS_CLIN_BOE_XREF X ON B.BOEID = X.BOEID
			LEFT OUTER JOIN dbo.WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID
			LEFT OUTER JOIN dbo.CLIN C ON X.CLINID = C.CLINID
		WHERE WS.IsUsingEquivalentPerson = @IsEP AND
			  (WS.IsDeleted IS NULL OR WS.IsDeleted != 1) AND /* Exclude deleted workspaces */
		      B.IsMaterial = @IsMaterial AND	/* Only search BOE's with same IsMaterial setting as specified BOE */
			  B.BOEID != @BOEID AND				/* Exclude specified BOE */
			  (WS.ProjectMapTypeID = 1 OR WS.ProjectMapTypeID = 2) AND		/* Exclude BOEs in Project Map Workspaces */
			(
				(
					@SearchCategory = 1  /*1 = BOEs in other Workspaces*/
					AND
					(
						WS.ContainsOCI = 0 /*Does Not Contain OCI*/ 
						AND WS.AllowSearch = 1 /*BOE is in searchable state*/  
					)
				) OR (
					@SearchCategory = 2  /*2= BOE Content Template*/
					AND (
						WS.ContainsOCI = 0 /*Does Not Contain OCI*/ 
						AND WS.AllowSearch = 1 /*BOE is in searchable state*/
						AND WS.ContainsTemplate = 1
					)
				) OR (
					@SearchCategory = 3  /*3= BOEs in this Workspaces*/
					AND
					(
						WS.WorkspaceID = @WorkspaceID
						AND B.BOEStateID <> 1   /*BOEs in Unassigned state should not be searched since they will have no data in them.*/
					) 
				) OR (
					@SearchCategory = 4  /*4 = All*/
					AND
					(
						(
							WS.ContainsOCI = 0 /*Does Not Contain OCI*/
							AND WS.AllowSearch = 1 /*BOE is in searchable state*/  
						) OR (
							WS.WorkspaceID = @WorkspaceID 
							AND B.BOEStateID <> 1 /*BOEs in Unassigned state should not be searched since they will have no data in them.*/
						)	 
					)
				)
			) 
			AND (@WorkspaceName = '""' OR CONTAINS(WS.WorkspaceName, @WorkspaceName)) 
			AND (@WorkspaceDescription = '""' OR CONTAINS(WS.WorkspaceDescription, @WorkspaceDescription))
			AND (@ProposalSubmitStartDate IS NULL OR (WS.ProposalSubmitDate >= @ProposalSubmitStartDate))
			AND (@ProposalSubmitEndDate IS NULL OR (WS.ProposalSubmitDate <= @ProposalSubmitEndDate))
			AND (@RFPNumber = '""' OR CONTAINS(WS.RFPNumber, @RFPNumber))
			AND (@BOEDescription = '""' OR CONTAINS(B.BOEDescription, @BOEDescription))
			AND (@BOETitle = '""' OR CONTAINS(B.BOETitle, @BOETitle))
			AND (@TaskTitle = '""' OR CONTAINS(TE.TaskTitle, @TaskTitle))
			AND (@TaskDescription = '""' OR CONTAINS(TE.TaskDescription, @TaskDescription))
			AND (@DataSource = '""' OR CONTAINS(B.DataSource, @DataSource))
			AND (@PerformingOrganization = '""' OR CONTAINS(PO.PerformingOrganizationName, @PerformingOrganization))
			AND (@CostVolume = '""' OR (CONTAINS(CostVolume.DisplayName, @CostVolume)) OR (CONTAINS(CostVolume.LastName, @CostVolume))) 
			AND (@Author = '""' OR (CONTAINS(Author.DisplayName, @Author)) OR (CONTAINS(Author.LastName, @Author)))
			AND (@Approver  = '""' OR (CONTAINS(Approver.DisplayName, @Approver )) OR (CONTAINS(Approver.LastName, @Approver ))) 
			AND (@DisplayedCLINNumber = '""' OR CONTAINS(C.DisplayedCLINNumber, @DisplayedCLINNumber))
			AND (@CLINTitle = '""' OR CONTAINS(C.CLINTitle, @CLINTitle))
			AND (@DisplayedWBSNumber = '""' OR CONTAINS(WBS.DisplayedWBSNumber, @DisplayedWBSNumber))
			AND (@WBSTitle = '""' OR CONTAINS(WBS.WBSTitle, @WBSTitle))  
		ORDER BY 
			WS.WorkspaceName, WS.ProposalSubmitDate, WBS.WBSNumber

SELECT	TOP(@SearchResultsThreshold) R.BOEID AS BOEID, R.WorkspaceId AS WorkspaceId 
	FROM @Results R
		INNER JOIN (	
			SELECT BOEID, MIN (OrderID) AS OrderID
				FROM @Results
				GROUP BY BOEID
		) AS O ON R.OrderID = O.OrderID 
	 ORDER BY O.OrderID;

GO