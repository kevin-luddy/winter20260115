IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[UsersWithRolesReport]') AND type in (N'V'))
	DROP VIEW dbo.UsersWithRolesReport;
GO

CREATE VIEW dbo.UsersWithRolesReport AS
	SELECT * FROM (
		-- BOE level Assigned Roles
		SELECT
			DISTINCT 
				u.DisplayName, u.NTID, r.RoleName, w.WorkspaceName, wS.WorkspaceState,
				w.UpdateDT AS WorkspaceLastModifiedDate, w.WorkspaceCreationDate, w.IsDeleted
				FROM BOEPotentialRole br
						INNER JOIN RoleLU r ON bR.RoleId = r.RoleID
						INNER JOIN ETIuser u ON u.ETIUserID = bR.ETIUserID
						INNER JOIN Workspace w ON w.WorkspaceId = br.WorkspaceId
						INNER JOIN WorkspaceStateLU wS ON w.WorkspaceStateID = wS.WorkspaceStateID
		UNION
		-- WS level roles, for Reviewer only
		SELECT
			DISTINCT 
				u.DisplayName, u.NTID, r.RoleName, w.WorkspaceName, wS.WorkspaceState,
				w.UpdateDT AS WorkspaceLastModifiedDate, w.WorkspaceCreationDate, w.IsDeleted
				FROM WorkspaceUserRole bR 
						INNER JOIN RoleLU r ON bR.RoleId = r.RoleID
						INNER JOIN ETIuser u ON u.ETIUserID = bR.ETIUserID
						INNER JOIN Workspace w ON w.WorkspaceId = bR.WorkspaceId
						INNER JOIN WorkspaceStateLU wS ON w.WorkspaceStateID = wS.WorkspaceStateID
				WHERE r.RoleName != 'Workspace User'
	) data;
GO
