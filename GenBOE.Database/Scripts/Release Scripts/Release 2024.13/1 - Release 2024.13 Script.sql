EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.13'; 
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SkillMix]') AND type in (N'U')) 
BEGIN 
	ALTER TABLE [dbo].[SkillMix] 
		DROP CONSTRAINT [FK_SkillMix_MOQTypeSelection];
	ALTER TABLE [dbo].[SkillMix] 
		DROP COLUMN MOQTypeSelectionID; 
	ALTER TABLE [dbo].[SkillMix] 
		DROP COLUMN IsPercentLocked; 
	ALTER TABLE [dbo].[SkillMix] 
		ADD IsUserInput bit NOT NULL DEFAULT 0; 
	ALTER TABLE [dbo].[SkillMix] 
	ALTER COLUMN [ResourceNew] varchar(50) NULL; 
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[SkillMix]') AND type in (N'U')) 
BEGIN 
	ALTER TABLE [version].[SkillMix] 
		DROP CONSTRAINT [FK_SkillMix_MOQTypeSelection];
	ALTER TABLE [version].[SkillMix] 
		DROP COLUMN MOQTypeSelectionID; 
	ALTER TABLE [version].[SkillMix] 
		DROP COLUMN IsPercentLocked; 
	ALTER TABLE [version].[SkillMix] 
		ADD IsUserInput bit NOT NULL DEFAULT 0; 
	ALTER TABLE [version].[SkillMix] 
	ALTER COLUMN [ResourceNew] varchar(50) NULL; 
END 
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CommonDisclosure]') AND type in (N'U')) 
BEGIN 
	ALTER TABLE [dbo].[CommonDisclosure] 
		DROP COLUMN MOQTypeSelectionID; 
	ALTER TABLE [dbo].[CommonDisclosure] 
		DROP COLUMN IsPercentLocked; 
END 
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[CommonDisclosure]') AND type in (N'U')) 
BEGIN 
	ALTER TABLE [version].[CommonDisclosure] 
		DROP COLUMN MOQTypeSelectionID; 
	ALTER TABLE [version].[CommonDisclosure] 
		DROP COLUMN IsPercentLocked; 
END 
GO
