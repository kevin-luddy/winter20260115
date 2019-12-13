IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getProjectMapIDForProjectMapAdvancedSearch]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getProjectMapIDForProjectMapAdvancedSearch];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[getProjectMapIDForProjectMapAdvancedSearch]
(
@WorkspaceName nvarchar(215),
@WorkspaceDescription nvarchar(200), 
@ActivityId varchar(200),
@ActivityName varchar(200),
@CamName varchar(200),
@Category varchar(200),
@SOWTitle varchar(200),
@Task nvarchar(200),
@Rationale varchar(200),
@DisplayedWBSNumber varchar(200),
@WBSTitle varchar(200),
@WorkspaceID int,
@SearchResultsThreshold int
)
AS
/******************************************************************************
**		 
**		Name: [getProjectMapIDForProjectMapAdvancedSearch]
**		Desc: Returns ProjectMap ID using Advanced Search criteria
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 8/15/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**      8/31/17		twilson3			Fix the WBS Searching to use LIKE instead of Contains because of periods
**		9/26/17		twilson3			BOEJ-2520 Update for new ProjectMap tables
**		5/25/2018	ranzalon			BOEJ-3503 - Exclude Deleted Workspaces
**		7/12/2018	twilson3			BOEJ-3685 - Fix exclusion of Deleted Workspaces
*******************************************************************************/
SET NOCOUNT ON 

SELECT
	@WorkspaceName = IsNULL(@WorkspaceName, '""'),
	@WorkspaceDescription = IsNULL(@WorkspaceDescription, '""'), 
	@ActivityId = IsNULL(@ActivityId, '""'),
	@ActivityName = IsNULL(@ActivityName, '""'),
	@CamName = IsNULL(@CamName, '""'),
	@Category = IsNULL(@Category, '""'),
	@SOWTitle = IsNULL(@SOWTitle, '""'),
	@Task = IsNULL(@Task, '""'),
	@Rationale = IsNULL(@Rationale, '""'),
	@DisplayedWBSNumber = IsNULL(@DisplayedWBSNumber, '""'),
	@WBSTitle = IsNULL(@WBSTitle, '""')

DECLARE @ProjectMapTypeID int
SET @ProjectMapTypeID = (SELECT ProjectMapTypeID from dbo.Workspace where WorkspaceID = @WorkspaceID)

DECLARE @Results TABLE
	(
		OrderID int Identity(1,1) NOT NULL,
		ProjectMapId int NOT NULL,
		WorkspaceId int NOT NULL
	)		

INSERT INTO @Results (ProjectMapId, WorkspaceID)	
	SELECT	P.ID, WS.WorkspaceID
		FROM dbo.Workspace WS
			INNER JOIN dbo.ProjectMap P ON WS.WorkspaceID = P.WorkspaceID
		WHERE WS.ProjectMapTypeID = @ProjectMapTypeID 
			AND WS.ContainsOCI = 0 /*Does Not Contain OCI*/
			AND WS.AllowSearch = 1 /*BOE is in searchable state*/
			AND (WS.IsDeleted IS NULL OR WS.IsDeleted != 1) /* Exclude deleted workspaces */
			AND WS.WorkspaceID <> @WorkspaceID /*If I am searching Other Workspaces I should not be returning data from my WS*/
			AND (@WorkspaceName = '""' OR CONTAINS(WS.WorkspaceName, @WorkspaceName)) 
			AND (@WorkspaceDescription = '""' OR CONTAINS(WS.WorkspaceDescription, @WorkspaceDescription))
			AND (@ActivityId = '""' OR CONTAINS(P.ActivityId, @ActivityId))
			AND (@ActivityName = '""' OR CONTAINS(P.ActivityName, @ActivityName))
			AND (@CamName = '""' OR CONTAINS(P.CamName, @CamName))
			AND (@Category = '""' OR CONTAINS(P.Category, @Category))
			AND (@SOWTitle = '""' OR CONTAINS(P.SOWTitle, @SOWTitle))
			AND (@Task = '""' OR CONTAINS(P.Task, @Task))
			AND (@Rationale = '""' OR CONTAINS(P.Rationale, @Rationale))
			AND (@DisplayedWBSNumber = '' OR P.WbsNumber LIKE @DisplayedWBSNumber)
			AND (@WBSTitle = '""' OR CONTAINS(P.WbsElementTitle, @WBSTitle))  
		ORDER BY 
			WS.WorkspaceName, WS.ProposalSubmitDate, P.WbsNumber

SELECT	TOP(@SearchResultsThreshold) R.ProjectMapId AS ProjectMapId, R.WorkspaceId AS WorkspaceId 
	FROM @Results R
		INNER JOIN (	
			SELECT ProjectMapId, MIN (OrderID) AS OrderID
				FROM @Results
				GROUP BY ProjectMapId
		) AS O ON R.OrderID = O.OrderID 
	 ORDER BY O.OrderID;

GO