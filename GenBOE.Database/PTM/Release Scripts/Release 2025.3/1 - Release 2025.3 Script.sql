EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.3';
GO

-- Author: Oyeyemi Oyetoro (e378233)
-- JIRA Story: PROPH-2835
-- Added Program Mgr and MSAC POC to Role table

IF NOT EXISTS (SELECT * FROM dbo.[RoleLU] WHERE RoleID IN (25))
BEGIN
	SET IDENTITY_INSERT dbo.[RoleLU] ON;

	INSERT INTO [RoleLU]
		(RoleID, [Role])
	VALUES
		(25, 'Program Mgr')
		
	SET IDENTITY_INSERT dbo.[RoleLU] OFF;
END
GO

IF NOT EXISTS (SELECT * FROM dbo.[RoleLU] WHERE RoleID IN (26))
BEGIN
	SET IDENTITY_INSERT dbo.[RoleLU] ON;

	INSERT INTO [RoleLU]
		(RoleID, [Role])
	VALUES
		(26, 'MSAC POC (Business Development)')
		
	SET IDENTITY_INSERT dbo.[RoleLU] OFF;
END
GO