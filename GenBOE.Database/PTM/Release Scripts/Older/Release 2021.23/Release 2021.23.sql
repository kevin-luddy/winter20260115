EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2021.7';
GO

/*
	## START ##

	10/04/2021 [Koovackal] - IES-409 Permissions - Add a 2nd Role - Backup Contracts Lead Part 1
*/
IF NOT EXISTS (SELECT 1 FROM RoleLU WHERE RoleId = 22)
BEGIN
	SET IDENTITY_INSERT RoleLU ON;
	INSERT INTO RoleLU (RoleId, Role)
		VALUES	(22, 'Backup Contracts Lead')
	SET IDENTITY_INSERT RoleLU OFF;
END
GO
/*
	10/04/2021 [Koovackal] - IES-409 Permissions - Add a 2nd Role - Backup Contracts Lead Part 1

	## END ##
*/
