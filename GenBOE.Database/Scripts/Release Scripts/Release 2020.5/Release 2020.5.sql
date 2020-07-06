EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2020.5';
GO

/*
	## START ##

	4/28/2020 [ranzalon] - BOEJ-4412 - Workspace Author role
*/

IF NOT EXISTS (SELECT * FROM [dbo].[RoleLU] WHERE [RoleName] = 'Workspace Author')
BEGIN
	INSERT INTO [dbo].[RoleLU] ([RoleID],[RoleName]) VALUES (11, 'Workspace Author')
END
GO

/*
	4/28/2020 [ranzalon] - BOEJ-4412 - Workspace Author role

	## END ##
*/
