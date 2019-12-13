EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.5';
GO

/*
		## START ##
		5/1/18 ranzalon - BOEJ-3416 - Template available to all Workspaces
*/

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsAvailableToAllWorkspaces' AND Object_ID = Object_ID(N'[dbo].[OutputFormatTemplate]'))
BEGIN
	ALTER TABLE [dbo].[OutputFormatTemplate]
	ADD [IsAvailableToAllWorkspaces] BIT NOT NULL DEFAULT 0 
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsAvailableToAllWorkspaces' AND Object_ID = Object_ID(N'[version].[OutputFormatTemplate]'))
BEGIN
	ALTER TABLE [version].[OutputFormatTemplate]
	ADD [IsAvailableToAllWorkspaces] BIT NOT NULL DEFAULT 0
END

/*
		5/1/18 ranzalon - BOEJ-3416 - Template available to all Workspaces
		## END ##
*/

/*
		## START ##
		5/9/18 ranzalon - BOEJ-3412 - Archive Templates
*/

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteOutputFormatTemplate]') AND type in (N'P', N'PC'))
BEGIN
	DROP PROCEDURE [dbo].[deleteOutputFormatTemplate]; /* Will get re-added as archiveOutputFormatTemplate */
END

/*
		5/9/18 ranzalon - BOEJ-3412 - Archive Templates
		## END ##
*/
/*
       ## START ##
       
       5/14/2018		twilson3		BOEJ-3362 BOE Updates for PickList
*/
IF EXISTS (
                           SELECT * FROM sys.all_columns C
                                  INNER JOIN sys.tables T on C.object_id = T.object_id
                                  INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
                           WHERE
                                  T.name = 'LineOfBusiness' AND
                                  C.name = 'LineOfBusinessURL' AND
                                  S.name = 'dbo'
                           )
BEGIN
	ALTER TABLE LineOfBusiness DROP COLUMN LineOfBusinessURL

	ALTER TABLE LineOfBusiness DROP COLUMN ForesightLineOfBusinessID

	ALTER TABLE LineOfBusiness DROP COLUMN [LineOfBusinessLongName]

	UPDATE LineOfBusiness SET [LineOfBusinessName] = 'Civil Space' WHERE [LineOfBusinessName] = 'Civil Space (CS)'

	-- These changes are to be executed in SSC only. The way we can tell the environments apart is that SSC has LOBs in the range of 1000's. RMS is 2000+ and ISGS is 0-999
	IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessID > 1000 AND LineOfBusinessID < 1999)
	BEGIN
		INSERT INTO LineOfBusiness ([LineOfBusinessName],[IsActive])
		VALUES ('AWE', 0), ('Commercial Launch', 0), ('Comm Space', 0);
	END

	ALTER TABLE [dbo].[ProposalClassLU] ADD IsActive bit not NULL default 1

	ALTER TABLE [dbo].[ContractTypeLU] ADD IsActive bit not NULL default 1

END

GO

UPDATE [ContractTypeLU] SET ContractType = 'IWTA-FWP' WHERE ContractType = 'IWTAFWP'
UPDATE [ContractTypeLU] SET ContractType = 'IWTA-FCC' WHERE ContractType = 'IWTAFCC'
UPDATE [ContractTypeLU] SET IsActive = 0 WHERE ContractType in ('Coop.Agreement/Grant', 'Other', 'Commercial')
UPDATE [ProposalClassLU] SET [ProposalClass] = 'ROM' WHERE [ProposalClass] = 'ROM/Budgetary'
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[ContractTypeLU] WHERE ContractType = 'Time and Material Level of Effort')
BEGIN
	INSERT INTO [ContractTypeLU] ([ContractTypeId], [ContractType], [IsActive]) VALUES (1026, 'Time and Material Level of Effort', 0);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[ProposalClassLU] WHERE [ProposalClass] = 'Not Set')
BEGIN
	INSERT INTO [ProposalClassLU] ([ProposalClassID], [ProposalClass], [IsActive]) VALUES (1004, 'Not Set', 0);
END
GO
/*
		5/14/2018		twilson3		BOEJ-3362 BOE Updates for PickList
		## END ##
*/

/*
		## START ##
		6/6/18 ranzalon - BOEJ-3502 - Unique workspace shortnames
*/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'UC_WorkspaceShortName')
BEGIN
	ALTER TABLE [dbo].Workspace ADD  CONSTRAINT [UC_WorkspaceShortName] UNIQUE (WorkspaceShortName)
END
GO

/*
		6/6/18 ranzalon - BOEJ-3502 - Unique workspace shortnames
		## END ##
*/

/*
		## START ##
		6/6/18 ranzalon - BOEJ-3524 - Update BOE Status Report description
*/

IF EXISTS (SELECT 1 FROM [dbo].[ReportLU] WHERE [ReportID] = 2 AND [Description] = 'View the WBS, CLIN, Author, Approver, Status, Start Date, End Date and Total Hours for each BOE.')
BEGIN
	UPDATE [dbo].[ReportLU] SET [Description] = 'View the WBS, CLIN, Author, Approver, Status, Start Date, End Date, Total Hours, and Total Cost for each BOE.' WHERE [ReportID] = 2
END

/*
		6/6/18 ranzalon - BOEJ-3524 - Update BOE Status Report description
		## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '2', @AppVersion = '2018.5';
GO

/*
       ## START ##
       
       6/8/2018		twilson3		BOEJ-3572 Update Sikorsky report names
*/
UPDATE [dbo].[ReportLU] SET ReportName = 'Project Category CLIN Cost Summary' WHERE ReportName = 'Category/CLIN Summary';
UPDATE [dbo].[ReportLU] SET ReportName = 'Project CLIN Category Cost Summary' WHERE ReportName = 'CLIN/Category Summary';
UPDATE [dbo].[ReportLU] SET ReportName = 'Flattened Cost by CLIN, Res, Activity 8 yr. (DCMA)' WHERE ReportName = 'Flattened Costs by CLIN, Res, Act & Yr (yrs)';
UPDATE [dbo].[ReportLU] SET ReportName = 'Flattened Cost by CLIN, Res, Activity 8 yr. (DCMA)' WHERE ReportName = 'Cost Analysis by CLIN, Activity and CY - 8 Yrs';
UPDATE [dbo].[ReportLU] SET ReportName = 'Cost Analysis by CLIN, CC, Activity and CY - 8 Yrs' WHERE ReportName = 'Cost by CLIN, Res, Act & Yr (8yrs)';
UPDATE [dbo].[ReportLU] SET ReportName = 'Cost Analysis by CLIN, Activity and CY - 17 Yrs' WHERE ReportName = 'Cost by CLIN, Act & Yr (17yrs)';
UPDATE [dbo].[ReportLU] SET ReportName = 'BOE - Standard Report' WHERE ReportName = 'BOE Summary Report';
UPDATE [dbo].[ReportLU] SET ReportName = 'Cost by CLIN - Pricing Code - 17 Yr.' WHERE ReportName = 'By Pricing Code (17 yrs)';
UPDATE [dbo].[ReportLU] SET ReportName = 'Cost by Category - Pricing Code - 17 Yr.' WHERE ReportName = 'By Cat/Pricing Code (17 yrs)';

/*
		6/8/2018		twilson3		BOEJ-3572 Update Sikorsky report names
		## END ##
*/