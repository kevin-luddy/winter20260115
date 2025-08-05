EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.10';
GO

-- 8/5/2025	Katie Pham (e309214),	PROPH-3202 - Skill Mix Disabling via UI

INSERT INTO [dbo].[SystemSetting] ([Key], [Value]) VALUES ('SkillMixWhitelist', '');
INSERT INTO [dbo].[SystemSetting] ([Key], [Value]) VALUES ('EnableSkillMixWhitelist', 'false');

GO