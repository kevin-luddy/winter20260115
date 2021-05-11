/*
	### DO NOT EXECUTE AS A PART OF ANY RELEASE ###
	
	This Script will setup an application admin. It is meant for GenTrac database.
	
*/

-- The application will not work if any of these fields do not match AD
DECLARE @ntid VARCHAR(50) = '';
DECLARE @displayName VARCHAR(50) = '';
DECLARE @firstName VARCHAR(50) = '';
DECLARE @lastName VARCHAR(50) = '';
DECLARE @email VARCHAR(50) = '';
-- NOTHING ELSE TO CHANGE BEYOND THIS LINE

-- Insert the user
INSERT INTO genTracUser (UpdateDT, NTID, DisplayName, FirstName, LastName, EmailAddress, IsGroup) VALUES (GETDATE(), @ntid, @displayName, @firstName, @lastName, @email, 0);

-- Grant System Admin role
INSERT INTO SystemUserRole (UpdateDT, UserId, RoleId) VALUES (GETDATE(), (SELECT TOP(1) UserId FROM genTracUser), 10);