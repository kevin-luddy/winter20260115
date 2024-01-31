EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.3';
GO

/*
                ## START ##

                1/30/24 [ranzalon] - PROPH-1444 - new assignees
*/

IF NOT EXISTS (SELECT 1 FROM [dbo].[RoleLU] WHERE RoleID = 23 OR RoleID = 24)
BEGIN
	SET IDENTITY_INSERT [dbo].[RoleLU] ON

	INSERT INTO [dbo].[RoleLU] ([RoleID] ,[Role])
	VALUES (23, 'Backup Subcontracts Lead'), (24, 'Backup Material Lead')
		
	SET IDENTITY_INSERT [dbo].[RoleLU] OFF
END

/*
                ## END ##

                1/30/24 [ranzalon] - PROPH-1444 - new assignees
*/