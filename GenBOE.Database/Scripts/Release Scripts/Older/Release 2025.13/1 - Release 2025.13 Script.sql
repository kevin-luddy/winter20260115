EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.13';
GO

-- 8/27/2025 RJ Anzalone (ranzalon), PROPH-3065 - new Workspace Auditor Role
IF NOT EXISTS (SELECT 1 FROM [dbo].[RoleLU] WHERE RoleName = 'Workspace Auditor')
BEGIN
	INSERT INTO [dbo].[RoleLU] (RoleID, RoleName)
	VALUES (11, 'Workspace Auditor')
END
GO

-- The following is commented out but to be included so it can be run manually for RMS ONLY
--IF NOT EXISTS (SELECT 1 FROM [dbo].[ETIuser] WHERE NTID = 'rms.estcompliance')
--BEGIN
--	INSERT INTO [dbo].[ETIuser] (NTID, DisplayName, UpdateDT)
--	VALUES ('rms.estcompliance', 'RMS.EstCompliance', GETDATE())
--END
--GO

--DECLARE @RmsEstComplianceId int;
--SELECT @RmsEstComplianceId = ETIUserID FROM [dbo].[ETIuser] WHERE NTID = 'rms.estcompliance'

--IF NOT EXISTS (SELECT 1 FROM [dbo].[SystemUserRole] WHERE ETIUserID = @RmsEstComplianceId AND RoleID = 11)
--BEGIN
--	INSERT INTO [dbo].[SystemUserRole] (ETIUserID, RoleID, UpdateDT)
--	VALUES (@RmsEstComplianceId, 11, GETDATE())
--END
--GO