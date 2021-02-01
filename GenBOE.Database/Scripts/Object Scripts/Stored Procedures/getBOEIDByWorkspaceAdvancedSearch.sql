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
**		Name: [getBOEIDByWorkspaceAdvancedSearch]
**		Desc: Returns BOEID using Advanced Search criteria
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		5/25/2018	ranzalon			BOEJ-3503 - Exclude Deleted Workspaces
**		7/12/2018	twilson3			BOEJ-3685 - Fix exclusion of Deleted Workspaces
**		9/9/2020	ranzalon			BOEJ-4770 - Only search WSs with same Template BOE value
**		11/4/2021	Dusan				BOEJ-4889 - Removing restriction to only search by the same Template BOE value
**		11/24/2020	Dusan				BOEJ-4949 - Add restrictions based on Template BOE value; did some code cleanup as well
**		12/15/2020	Dusan				BOEJ-4953 - Fixing logic w/ MOQ Type restrictions
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @Results TABLE ( OrderID int Identity(1,1) NOT NULL, BOEID int NOT NULL, WorkspaceId int NOT NULL )		

DECLARE @IsEP bit = (SELECT IsUsingEquivalentPerson from dbo.Workspace where WorkspaceID = @WorkspaceID)
DECLARE @IsMaterial bit = (SELECT IsMaterial from dbo.BOE where BOEID = @BOEID)
/* if you are starting from an Old workspace (PreNewMoq) ==> you can only copy BOEs from another Old workspace */
DECLARE @restrictToPreNewMoq BIT = CASE WHEN dbo.WasWorkspaceCreatedAfterNewMoqTypes((SELECT WorkspaceCreationDate FROM Workspace WHERE WorkspaceId = @WorkspaceID)) = 0 THEN 1 ELSE 0 END 
/* if you are starting from a * / No (Not using Boe Template) ==> you can only copy from * / No */
DECLARE @restrictToNotUsingBoeTemplate BIT = CASE WHEN (SELECT TemplateBoe FROM Workspace WHERE WorkspaceId = @WorkspaceID) = 0 THEN 1 ELSE 0 END 


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
			/* START new MOQ Types filtering */
			AND /* not restricted or (restricted and not new) => !a || (a && !b) = !a || !b */
				(@restrictToPreNewMoq = 0 OR dbo.WasWorkspaceCreatedAfterNewMoqTypes(WS.WorkspaceCreationDate) = 0) 
			AND /* not restricted or (restricted and not using template) => !a || (a && !b) = !a || !b */
				(@restrictToNotUsingBoeTemplate = 0 OR WS.TemplateBoe = 0)
			/* END new MOQ Types filtering */
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