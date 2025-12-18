EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.19';
GO

-- 12/18/2025 RJ (ranzalon), Haz (e403038) PROPH-3500 - UCOT Blacklist
IF NOT EXISTS (SELECT 1 FROM [dbo].[SystemSetting] WHERE [Key] = 'UCOTBlacklist')
BEGIN
	INSERT INTO [dbo].[SystemSetting] ([Key], [Value]) VALUES ('UCOTBlacklist', '');
END