EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2019.1';
GO

/*
		## START ##
		1/29/2019 Dusan - Adding a "system" user, which will be used for automatic WS backups
*/
SET IDENTITY_INSERT EtiUser ON;
IF (SELECT COUNT(1) FROM EtiUser WHERE EtiUserId = 0) < 1
	BEGIN
		INSERT INTO EtiUser (EtiUserId, UpdateDT, NTID, DisplayName, EmailAddress, PhoneNumber, FirstName, LastName)
			VALUES (0, GETDATE(), 'genapps', 'genBOE System', NULL, NULL, 'genBOE', 'System');
	END
SET IDENTITY_INSERT EtiUser OFF;
GO

/*
		1/29/2019 Dusan - Adding a "system" user, which will be used for automatic WS backups
		## END ##
*/
