IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspacePermissions]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspacePermissions];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getWorkspacePermissions]
(
@NTID varchar(10)
)
AS
/******************************************************************************
**		 
**		Name: getWorkspacePermissions
**		Desc: Returns Workspace Permissions
**			
**		
**
**		Auth: Don Canuso
**		Date: 12/1/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		5/9/11		dcanuso				NTID and Domain needed to be unique	
**		11/28/17	pattoncr			Remove domain.
*******************************************************************************/
SET NOCOUNT ON 

	SELECT 
		WS.WorkspaceID AS WorkspaceID,
		WSR.RoleID AS WorkspaceRoleID,
		WS.WorkspaceStateID AS WorkspaceStateID
	FROM dbo.Workspace WS
		INNER JOIN dbo.WorkspaceUserRole WSR ON WS.WorkspaceID = WSR.WorkspaceID
		INNER JOIN dbo.ETIuser E ON E.ETIUserID = WSR.ETIUserID
	WHERE 
		E.NTID = @NTID

GO