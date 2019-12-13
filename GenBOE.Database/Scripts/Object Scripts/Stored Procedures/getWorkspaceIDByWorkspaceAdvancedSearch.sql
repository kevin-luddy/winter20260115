IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspaceIDByWorkspaceAdvancedSearch]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspaceIDByWorkspaceAdvancedSearch];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getWorkspaceIDByWorkspaceAdvancedSearch]
(
@WorkspaceName nvarchar(200),
@WorkspaceDescription nvarchar(200), 
@ProposalSubmitStartDate DATETIME2,
@ProposalSubmitEndDate DATETIME2,
@RFPNumber nvarchar(200),
@CostVolume nvarchar(200),
@ProjectMapType int = NULL
)
AS
/******************************************************************************
**		 
**		Name: [getWorkspaceIDByWorkspaceAdvancedSearch]
**		Desc: Returns WorkspaceID using Advanced Search criteria
**				All words that are sent in need
**				to be found in the same field with an AND
**				So, AND between words that are sent in
**				Application will add " and  AND between
**				the word, so if @Paramter = 'Yellow Blue',
**				the application will pass in
**				'"Yellow" AND "Blue"'
**				
**		Auth: Don Canuso
**		Date: 2/17/2011
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		6/22/11		dcanuso				Added All as an option
**										Bug: 3916:
**										Update to search all workspace proposal 
**										types when user selects "All" from proposal 
**										type drop-down.
**		3/12/12		dcanuso				WI 7912 Workspace should be searchable in any state
**		1/7/13		dcanuso				Task #14148: Rel 1.5.4 getWorkspaceIDByWorkspaceAdvancedSearch
**										SP throws errors when searching on a name 
**										with 2 or more dashes
**										No errors in logic, but SP updated the following columns
**										@WorkspaceName nvarchar(200),
**										@WorkspaceDescription nvarchar(200), 
**										@RFPNumber nvarchar(200),
**										@CostVolume nvarchar(200)
**		5/9/2017	twilson3			BOEJ-2146 Added @ProjectMapType
**		5/10/2017	ranzalon			BOEJ-2053 - Remove Proposal Type
**		5/25/2018	ranzalon			BOEJ-3503 - Exclude Deleted Workspaces
**		7/12/2018	twilson3			BOEJ-3685 - Fix exclusion of Deleted Workspaces
*******************************************************************************/
SET NOCOUNT ON 

SELECT
	@WorkspaceName = IsNULL(@WorkspaceName, '""'),
	@WorkspaceDescription = IsNULL(@WorkspaceDescription, '""'), 
	@RFPNumber = IsNULL(@RFPNumber, '""'),
	@CostVolume = IsNULL(@CostVolume, '""')

SELECT WS.WorkspaceID AS WorkspaceID
FROM dbo.Workspace WS
	INNER JOIN dbo.ETIuser CostVolume ON WS.CostVolumeLeadPricerUserID = CostVolume.ETIUserID
WHERE
(WS.IsDeleted IS NULL OR WS.IsDeleted != 1) AND /* Is not deleted*/
WS.ContainsOCI = 0 /*Does Not Contain OCI*/ AND
WS.AllowSearch = 1 /*BOE is in searchable state*/ AND
	(@WorkspaceName = '""' OR CONTAINS(WS.WorkspaceName, @WorkspaceName)) AND
	(@WorkspaceDescription = '""' OR CONTAINS(WS.WorkspaceDescription, @WorkspaceDescription)) AND
	(@ProposalSubmitStartDate IS NULL OR (WS.ProposalSubmitDate >= @ProposalSubmitStartDate)) AND
	(@ProposalSubmitEndDate IS NULL OR (WS.ProposalSubmitDate <= @ProposalSubmitEndDate)) AND
	(@RFPNumber = '""' OR CONTAINS(WS.RFPNumber, @RFPNumber)) AND
	(
		@CostVolume = '""' OR 
		(CONTAINS(CostVolume.DisplayName, @CostVolume)) OR
		(CONTAINS(CostVolume.LastName, @CostVolume)) 
	) AND
	(
		@ProjectMapType IS NULL OR
		WS.[ProjectMapTypeID] = @ProjectMapType
	)
GROUP BY WS.WorkspaceID, WS.ProposalSubmitDate, WS.WorkspaceName
ORDER BY WS.ProposalSubmitDate, WS.WorkspaceName

GO