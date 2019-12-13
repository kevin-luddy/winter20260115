/*
	8/16/17 [Dusan] -- BOEJ-2474: Rename Initial Resource to Resource

	## START ##
*/
UPDATE [dbo].[ProPricerFieldLU] SET [ProPricerField] = 'Resource' WHERE [ProPricerFieldID] = 69;
GO
/*
	8/16/17 [Dusan] -- BOEJ-2474: Rename Initial Resource to Resource

	## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2.17';
GO
/*
	## START ##

	8/17/17 [Dusan] -- BOEJ-2469: Add Old Resource
*/
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'OldResource' AND Object_ID = Object_ID(N'[dbo].[BOELaborType]'))
	BEGIN
		ALTER TABLE [dbo].[BOELaborType] ADD [OldResource] VARCHAR(50);
	END
GO
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'OldResource' AND Object_ID = Object_ID(N'[version].[BOELaborType]'))
	BEGIN
		ALTER TABLE [version].[BOELaborType] ADD [OldResource] VARCHAR(50);
	END
GO
/*
	8/17/17 [Dusan] -- BOEJ-2469: Add Old Resource

	## END ##
*/

/*
	## START ##
	
	8/22/17		Dusan - BOEJ-2481 - Add LM Project Map Word Template
*/

SET IDENTITY_INSERT [dbo].[OutputFormatTemplate] ON
IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessId > 2001 AND LineOfBusinessId < 2999) -- RMS only
	AND NOT EXISTS(SELECT 1 FROM [dbo].[OutputFormatTemplate] WHERE TemplateID = 2009) -- does not exist yet
BEGIN
	INSERT INTO [dbo].[OutputFormatTemplate] (TemplateID, UpdateDT, Template, TemplateDescription, IsActive, ParentTemplateID) 
		VALUES (2009, GETDATE(), 'RMS - RMS Project Map', 'Standard Project Map template for RMS', 1, 2009)
END
SET IDENTITY_INSERT [dbo].[OutputFormatTemplate] OFF

GO
/*
	8/22/17		Dusan - BOEJ-2481 - Add LM Project Map Word Template
	
	## END ##
*/

/*
	## START ##
	8/24/17 [twilson3] - BOEJ-2475 Add Old Resource Code to Pro Pricer Task and Resource Fields for ProjectMap (4) only
*/

IF NOT EXISTS ( SELECT 1 FROM [dbo].[ProPricerFieldLU] WHERE [ProPricerFieldID]=74)
BEGIN
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(74,'Old Resource',1,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(75,'Old Resource',2,4);
END
GO

/*
	8/24/17 [twilson3] - BOEJ-2475 Add Old Resource Code to Pro Pricer Task and Resource Fields for ProjectMap (4) only
	## END ##
*/

/*
	## START ##
	9/5/17 [Joe] - Adding allow grid edit
*/

IF COL_LENGTH('[dbo].[Workspace]', 'AllowGridEdit') IS NULL
BEGIN
    ALTER TABLE [dbo].[Workspace] 
	ADD [AllowGridEdit] bit NOT NULL
	DEFAULT(0)
END

IF COL_LENGTH('[version].[Workspace]', 'AllowGridEdit') IS NULL
BEGIN
    ALTER TABLE [version].[Workspace] 
                ADD [AllowGridEdit] bit NOT NULL
                DEFAULT(0)
END


/*
	9/5/17 [Joe] - Adding allow grid edit
	## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '2', @AppVersion = '2.17';

/*
	## START ##
	9/6/17 [twilson3] - BOEJ-2505 Label Changes
*/

UPDATE [dbo].[ReportLU] SET [Description] = 'Summary estimate at cost sorted by category, CLIN, activity, activity type code, start and end date' WHERE [ReportName] = 'Category/CLIN Summary'
UPDATE [dbo].[ReportLU] SET [Description] = 'Summary estimate at cost sorted by CLIN, category, activity, activity type code, start and end date' WHERE [ReportName] = 'CLIN/Category Summary'
UPDATE [dbo].[ReportLU] SET [Description] = 'Project CLIN cost summary estimate sorted by CLIN, activity, activity type code, start and end date' WHERE [ReportName] = 'Project CLIN Cost Summary'
UPDATE [dbo].[ReportLU] SET [Description] = 'Summary estimate at cost sorted by CLIN, activity, activity type code, pricing code and year for an 8 year period with an individual sequence # referencing an individual basis of estimate' WHERE [ReportName] = 'Cost by CLIN, Res, Act & Yr (8yrs)'
UPDATE [dbo].[ReportLU] SET [Description] = 'Summary estimate at cost sorted by CLIN, activity, activity type code, pricing code and year for a 17 year period with an individual sequence # referencing an individual basis of estimate' WHERE [ReportName] = 'Cost by CLIN, Act & Yr (17yrs)'
UPDATE [dbo].[ReportLU] SET [Description] = 'Combined basis of estimate word document created for each activity in the project map with data for activity, CLIN, activity type code, WBS, POP dates, task, rationale, labor hours, cost dollars' WHERE [ReportName] = 'BOE Summary Report'
UPDATE [dbo].[ReportLU] SET [Description] = 'Summary estimate at cost sorted by pricing code, activity type code and year for a 17 year period' WHERE [ReportName] = 'By Pricing Code (17 yrs)'
UPDATE [dbo].[ReportLU] SET [Description] = 'Summary estimate at cost sorted by category, pricing code, activity type code and year for a 17 year period' WHERE [ReportName] = 'By Cat/Pricing Code (17 yrs)'
UPDATE [dbo].[ReportLU] SET [Description] = 'Requirements Planning System - Summary of total hours and dollars totaled by activity type code and spread by month' WHERE [ReportName] = 'RPS'
UPDATE [dbo].[ReportLU] SET [Description] = 'Responsibility Assignment Matrix - Matrix showing activity type codes estimated across the top, WBS and activity ID down the side and their corresponding estimates' WHERE [ReportName] = 'RAM'
UPDATE [dbo].[ProPricerFieldLU] SET [ProPricerField] = 'Legacy Resource' WHERE [ProPricerField] = 'Old Resource'
UPDATE [dbo].[ProPricerFieldLU] SET [ProPricerField] = 'Activity Type Code' WHERE [ProPricerField] = 'Resource' AND [ProPricerCompanyID] = 4

/*
	9/6/17 [twilson3] - BOEJ-2505 Label Changes
	## END ##
*/


EXEC [dbo].[UpdateDbVersion] @DbVersion = '2', @AppVersion = '2.17';


/*
	## START ##
	9/11/17 [Dusan] - Adding full text search on the columns below, to allow the search to work against project maps
*/

ALTER FULLTEXT INDEX ON [dbo].[BOE] ADD ([CamName])
ALTER FULLTEXT INDEX ON [dbo].[BOE] ADD ([Category])
ALTER FULLTEXT INDEX ON [dbo].[BOE] ADD ([Rationale])
ALTER FULLTEXT INDEX ON [dbo].[BOE] ADD ([SOWTitle])
GO

/*
	9/11/17 [Dusan] - Adding full text search on the columns below, to allow the search to work against project maps
	## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '3', @AppVersion = '2.17';
