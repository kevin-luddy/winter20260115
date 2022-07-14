EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2022.x';
GO

/*
		## START ##
		7/7/22		Dusan		IES-1328: Add 2 new fields to the Sections
*/

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'SectionContainsCasbDisclosure' AND Object_ID = Object_ID(N'[dbo].[Section]'))
BEGIN
	ALTER TABLE [dbo].[Section]
	ADD [SectionContainsCasbDisclosure] BIT NOT NULL DEFAULT 0
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'SectionContainsNonCompliance' AND Object_ID = Object_ID(N'[dbo].[Section]'))
BEGIN
	ALTER TABLE [dbo].[Section]
	ADD [SectionContainsNonCompliance] BIT NOT NULL DEFAULT 0
END
GO

/*
		7/7/22		Dusan		IES-1328: Add 2 new fields to the Sections
       ## END ##
*/