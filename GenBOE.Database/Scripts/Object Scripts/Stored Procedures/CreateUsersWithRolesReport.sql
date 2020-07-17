IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateUsersWithRolesReport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[CreateUsersWithRolesReport];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CreateUsersWithRolesReport]
(
	@cutoffCreationDate AS DATE = NULL,
	@cutoffModifiedDate AS DATE = NULL
)
AS
/******************************************************************************
**		 
**		Name: [CreateUsersWithRolesReport]
**		Desc: SSRS: Create Users With Roles Report
**			
**		Auth: Dusan
**		Date: 5/28/2020
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		5/28/2020	Dusan				BOEJ-4588 - Report identifying users who are assigned a Role within a Workspace
**		7/ 7/2020	Dusan				BOEJ-4688 - Add indicator if the WS is deleted
*******************************************************************************/

	SET NOCOUNT ON
	SELECT x.DisplayName, x.NTID, x.RoleName, x.WorkspaceName, 
			x.WorkspaceState, x.WorkspaceLastModifiedDate, x.WorkspaceCreationDate, CASE WHEN x.IsDeleted = 1 THEN 'Deleted' ELSE NULL END AS IsWsDeleted
		FROM UsersWithRolesReport x
		WHERE 
			(@cutoffCreationDate IS NULL OR x.WorkspaceCreationDate >= @cutoffCreationDate)
			AND (@cutoffModifiedDate IS NULL OR x.WorkspaceLastModifiedDate >= @cutoffModifiedDate)
		ORDER BY DisplayName, RoleName, WorkspaceName
GO

GRANT EXECUTE ON OBJECT::[dbo].[CreateUsersWithRolesReport] TO generationReporter;
GO
