EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.15';
GO

-- 10/13/2025 RJ Anzalone (ranzalon), PROPH-3286 - Enable LM Navigator Link
IF COL_LENGTH('[dbo].[Workspace]', 'EnableLmNavigator') IS NULL
BEGIN

ALTER TABLE [dbo].[Workspace]
ADD EnableLmNavigator bit not null
DEFAULT 0

END
GO

IF COL_LENGTH('version.Workspace', 'EnableLmNavigator') IS NULL
BEGIN

ALTER TABLE [version].[Workspace]
ADD EnableLmNavigator bit not null
DEFAULT 0

END
GO