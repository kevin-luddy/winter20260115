/*
	## START OF SCRIPTS FROM SCOTT ##

	02/08/2016 [Dusan]: Changes from Scott - Updates for BEOJ-838 to add a new Line of Business for Space Systems

	These changes are to be executed in Space only. The way we can tell the environments apart is that SSC has LOBs in the range of 1000's. MST is 2000+ and ISGS is 0-999
*/

IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusinessLU] WHERE LineOfBusinessId > 1000 AND LineOfBusinessId < 1999)
BEGIN
	IF NOT EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessName = 'Mission Solutions (MsnSln)')
	BEGIN
		INSERT INTO [dbo].[LineOfBusiness] (LineOfBusinessName, LineOfBusinessLongName, LineOfBusinessURL, ProductLineID, ForesightLineOfBusinessID, IsActive)
			VALUES ('Mission Solutions (MsnSln)', 'Mission Solutions (MsnSln)', 'Mission S', 1, -1, 1)
	END

	IF NOT EXISTS (SELECT 1 FROM [dbo].[LineOfBusinessLU] WHERE LineOfBusinessID = 1008)
	BEGIN
		INSERT INTO [dbo].[LineOfBusinessLU] (LineOfBusinessID, LineOfBusiness)
			VALUES (1008, 'Mission Solutions (MsnSln)')
	END
END
GO

/*
	02/08/2016 [Dusan]: Changes from Scott - Updates for BEOJ-838 to add a new Line of Business for Space Systems

	## END OF SCRIPTS FROM SCOTT ##
*/



/*
	## START OF SCRIPTS FOR BOEJ-854 ##

	02/18/2016 [Mike B]: Delete duplicate rows from WorkspaceUserRole and BOEPotentialRole
*/

BEGIN
delete a
from WorkspaceUserRole a, WorkspaceUserRole b
where a.ETIUserID = b.ETIUserID
and a.RoleID = b.RoleID
and a.WorkspaceID = b.WorkspaceID
and a.WorkspaceUserRoleID > b.WorkspaceUserRoleID

delete a
from BOEPotentialRole  a, BOEPotentialRole  b
where a.ETIUserID = b.ETIUserID
and a.RoleID = b.RoleID
and a.WorkspaceID = b.WorkspaceID
and a.BOEPotentialRoleID > b.BOEPotentialRoleID
END
/*
	## END OF SCRIPTS FOR BOEJ-854 ##

	02/18/2016 [Mike B]: Delete duplicate rows from WorkspaceUserRole and BOEPotentialRole
*/