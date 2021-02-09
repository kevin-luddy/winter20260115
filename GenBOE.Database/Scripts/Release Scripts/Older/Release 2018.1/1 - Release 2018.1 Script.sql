EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.1';
GO


/*
	   ## START ##
	   12/4/17 [twilson3] - BOEJ-2685 - Remove Product Line
*/

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'ProductLineID' AND Object_ID = Object_ID(N'[dbo].[LineOfBusiness]'))
BEGIN
	ALTER TABLE [dbo].[LineOfBusiness] DROP CONSTRAINT [FK_LineOfBusiness_ProductLine]
	ALTER TABLE [dbo].[LineOfBusiness] DROP COLUMN ProductLineID
END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'ProductLineID' AND Object_ID = Object_ID(N'[dbo].[Workspace]'))
BEGIN
	ALTER TABLE [dbo].[Workspace] DROP CONSTRAINT [FK_Workspace_ProductLine]
	ALTER TABLE [dbo].[Workspace] DROP COLUMN ProductLineID	
END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'ProductLineID' AND Object_ID = Object_ID(N'[version].[Workspace]'))
ALTER TABLE [version].[Workspace] DROP COLUMN ProductLineID
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'ProductLineID' AND Object_ID = Object_ID(N'[dbo].[WorkspaceCopySource]'))
ALTER TABLE [dbo].[WorkspaceCopySource] DROP COLUMN ProductLineID
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'ProductLineName' AND Object_ID = Object_ID(N'[dbo].[WorkspaceCopySource]'))
ALTER TABLE [dbo].[WorkspaceCopySource] DROP COLUMN ProductLineName
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'ProductLineLongName' AND Object_ID = Object_ID(N'[dbo].[WorkspaceCopySource]'))
ALTER TABLE [dbo].[WorkspaceCopySource] DROP COLUMN ProductLineLongName
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'ProductLineID' AND Object_ID = Object_ID(N'[dbo].[WorkspaceCopyTarget]'))
ALTER TABLE [dbo].WorkspaceCopyTarget DROP COLUMN ProductLineID
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'ProductLineName' AND Object_ID = Object_ID(N'[dbo].[WorkspaceCopyTarget]'))
ALTER TABLE [dbo].WorkspaceCopyTarget DROP COLUMN ProductLineName
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'ProductLineLongName' AND Object_ID = Object_ID(N'[dbo].[WorkspaceCopyTarget]'))
ALTER TABLE [dbo].WorkspaceCopyTarget DROP COLUMN ProductLineLongName
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductLineLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[ProductLineLU];
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductLine]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[ProductLine];
END
GO

/*
	   12/4/17 [twilson3] - BOEJ-2685 - Remove Product Line
	   ## END ##
*/

/*
	   ## START ##
	   12/7/17 [twilson3] - BOEJ-2250 - Remove DTC
*/

DELETE FROM [dbo].[ReportLU] where ReportName = 'Design to Cost'

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'WorkspaceDTCElementID' AND Object_ID = Object_ID(N'[dbo].[WorkBreakdownStructure]'))
BEGIN
	ALTER TABLE [dbo].[WorkBreakdownStructure] DROP CONSTRAINT FK_WorkBreakdownStructure_WorkspaceDTCElement
	DROP INDEX [IX_WorkBreakdownStructure__WorkspaceDTCElementID] ON [dbo].[WorkBreakdownStructure]
	ALTER TABLE [dbo].[WorkBreakdownStructure] DROP COLUMN WorkspaceDTCElementID
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceDTCElement]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[WorkspaceDTCElement];
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[WorkspaceDTCElement]') AND type in (N'U'))
BEGIN
	DROP TABLE [version].[WorkspaceDTCElement];
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DTCElementLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[DTCElementLU];
END
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspaceDTCElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspaceDTCElement];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceDTCElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceDTCElement];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertDefaultWorkspaceDTCElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertDefaultWorkspaceDTCElement];

GO
/*
	   12/7/17 [twilson3] - BOEJ-2250 - Remove DTC
	   ## END ##
*/

/*
	   ## START ##
	   12/7/17 [twilson3] - BOEJ-1994 - Remove Summary BOE
*/

DELETE FROM dbo.ReportLU WHERE ReportName = 'Summary BOE Discrepancy'

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsSummaryTaskElement' AND Object_ID = Object_ID(N'[dbo].[BOETaskElement]'))
BEGIN
	ALTER TABLE [dbo].[BOETaskElement] DROP CONSTRAINT [DF_BOETaskElement_IsSummaryBoeTaskElement]
	ALTER TABLE [dbo].[BOETaskElement] DROP COLUMN [IsSummaryTaskElement]
END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'SummaryBoeSelections' AND Object_ID = Object_ID(N'[dbo].[BOETaskElement]'))
ALTER TABLE [dbo].[BOETaskElement] DROP COLUMN [SummaryBoeSelections]
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsSummaryTaskElement' AND Object_ID = Object_ID(N'[version].[BOETaskElement]'))
BEGIN
	ALTER TABLE [version].[BOETaskElement] DROP CONSTRAINT [DF_BOETaskElement_IsSummaryBoeTaskElement]
	ALTER TABLE [version].[BOETaskElement] DROP COLUMN [IsSummaryTaskElement]
END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'SummaryBoeSelections' AND Object_ID = Object_ID(N'[version].[BOETaskElement]'))
ALTER TABLE [version].[BOETaskElement] DROP COLUMN [SummaryBoeSelections]
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsSummaryBoe' AND Object_ID = Object_ID(N'[dbo].[BOE]'))
BEGIN
	ALTER TABLE [dbo].[BOE] DROP CONSTRAINT [DF_BOE_IsSummaryBoe]
	ALTER TABLE [dbo].[BOE] DROP COLUMN [IsSummaryBoe]
END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsSummaryBoe' AND Object_ID = Object_ID(N'[version].[BOE]'))
BEGIN
	ALTER TABLE [version].[BOE] DROP CONSTRAINT [DF_BOE_IsSummaryBoe]
	ALTER TABLE [version].[BOE] DROP COLUMN [IsSummaryBoe]
END
GO

-- need to drop constraints that are not properly named

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'SummaryBoeSelection' AND Object_ID = Object_ID(N'[dbo].[Workspace]'))
BEGIN
	declare @Command  nvarchar(1000)

	select @Command = 'ALTER TABLE [dbo].[Workspace]' + ' drop constraint ' + d.name
	 from sys.tables t
	  join    sys.default_constraints d
	   on d.parent_object_id = t.object_id
	  join    sys.columns c
	   on c.object_id = t.object_id
		and c.column_id = d.parent_column_id
	 where t.name =  N'Workspace'
	  and t.schema_id = schema_id(N'dbo')
	  and c.name = N'SummaryBoeSelection'
	EXECUTE (@Command)
	ALTER TABLE [dbo].[Workspace] DROP COLUMN [SummaryBoeSelection]
END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'SummaryBoeSelection' AND Object_ID = Object_ID(N'[version].[Workspace]'))
BEGIN
	declare @Command  nvarchar(1000)

	select @Command = 'ALTER TABLE [version].[Workspace]' + ' drop constraint ' + d.name
	 from sys.tables t
	  join    sys.default_constraints d
	   on d.parent_object_id = t.object_id
	  join    sys.columns c
	   on c.object_id = t.object_id
		and c.column_id = d.parent_column_id
	 where t.name =  N'Workspace'
	  and t.schema_id = schema_id(N'version')
	  and c.name = N'SummaryBoeSelection'
	EXECUTE (@Command)
	ALTER TABLE [version].[Workspace] DROP COLUMN [SummaryBoeSelection]
END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'SummaryBoeCustomFieldSelection' AND Object_ID = Object_ID(N'[dbo].[Workspace]'))
BEGIN
	declare @Command  nvarchar(1000)

	select @Command = 'ALTER TABLE [dbo].[Workspace]' + ' drop constraint ' + d.name
	 from sys.tables t
	  join    sys.default_constraints d
	   on d.parent_object_id = t.object_id
	  join    sys.columns c
	   on c.object_id = t.object_id
		and c.column_id = d.parent_column_id
	 where t.name =  N'Workspace'
	  and t.schema_id = schema_id(N'dbo')
	  and c.name = N'SummaryBoeCustomFieldSelection'
	EXECUTE (@Command)
	ALTER TABLE [dbo].[Workspace] DROP COLUMN [SummaryBoeCustomFieldSelection]
END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'SummaryBoeCustomFieldSelection' AND Object_ID = Object_ID(N'[version].[Workspace]'))
BEGIN
	declare @Command  nvarchar(1000)

	select @Command = 'ALTER TABLE [version].[Workspace]' + ' drop constraint ' + d.name
	 from sys.tables t
	  join    sys.default_constraints d
	   on d.parent_object_id = t.object_id
	  join    sys.columns c
	   on c.object_id = t.object_id
		and c.column_id = d.parent_column_id
	 where t.name =  N'Workspace'
	  and t.schema_id = schema_id(N'version')
	  and c.name = N'SummaryBoeCustomFieldSelection'
	EXECUTE (@Command)
	ALTER TABLE [version].[Workspace] DROP COLUMN [SummaryBoeCustomFieldSelection]
END
GO

/*
	   12/7/17 [twilson3] - BOEJ-1994 - Remove Summary BOE
	   ## END ##
*/

/*
	   ## START ##
	   12/15/17 [twilson3] - BOEJ-2248 Remove Labor Rates
*/

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceResourceRate]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[WorkspaceResourceRate];
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[WorkspaceResourceRate]') AND type in (N'U'))
BEGIN
	DROP TABLE [version].[WorkspaceResourceRate];
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LaborResourceRate]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[LaborResourceRate];
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[LaborResourceRate]') AND type in (N'U'))
BEGIN
	DROP TABLE [version].[LaborResourceRate];
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceLockedLaborResourceRate]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[WorkspaceLockedLaborResourceRate];
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[WorkspaceLockedLaborResourceRate]') AND type in (N'U'))
BEGIN
	DROP TABLE [version].[WorkspaceLockedLaborResourceRate];
END
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateWorkspaceCalculateLaborCostFlag]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateWorkspaceCalculateLaborCostFlag];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspaceResourceRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspaceResourceRate];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceResourceRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceResourceRate];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspaceRateUpdateFlag]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspaceRateUpdateFlag];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertLaborResourceRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertLaborResourceRate];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateLockedWorkspaceResourceRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateLockedWorkspaceResourceRate];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteLaborResourceRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteLaborResourceRate];
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'CalculateLaborCostFlag' AND Object_ID = Object_ID(N'[dbo].[Workspace]'))
BEGIN
	ALTER TABLE [dbo].[Workspace] DROP CONSTRAINT [DF_Workspace_CalculateLaborCostFlag]
	ALTER TABLE [dbo].[Workspace] DROP COLUMN [CalculateLaborCostFlag]
END
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'CalculateLaborCostFlag' AND Object_ID = Object_ID(N'[version].[Workspace]'))
ALTER TABLE [version].[Workspace] DROP COLUMN [CalculateLaborCostFlag]
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'CalculateLaborCostFlag' AND Object_ID = Object_ID(N'[dbo].[WorkspaceCopySource]'))
ALTER TABLE [dbo].[WorkspaceCopySource] DROP COLUMN [CalculateLaborCostFlag]
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'CalculateLaborCostFlag' AND Object_ID = Object_ID(N'[dbo].[WorkspaceCopyTarget]'))
ALTER TABLE [dbo].[WorkspaceCopyTarget] DROP COLUMN [CalculateLaborCostFlag]
GO

EXEC [dbo].[UpdateDbVersion] @DbVersion = '2', @AppVersion = '2018.1';
/*
	   12/15/17 [twilson3] - BOEJ-2248 Remove Labor Rates
	   ## END ##
*/

/*
	   ## START ##
	   1/8/18 Dusan - BOEJ-2935 Remove bad NTIDs from the DB; these users are no longer active, their names were blanked in the database;
*/
DELETE FROM EtiUser WHERE NTID IN ('shepherd', 'roy', 'gray');
GO
/*
	   1/8/18 Dusan - BOEJ-2935 Remove bad NTIDs from the DB; these users are no longer active, their names were blanked in the database;
	   ## END ##
*/
/*
	   ## START ##
	   1/1/18 [twilson3] - BOEJ-2904 Removed LaborSpread Index
*/

IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BOELaborSpread]') AND name = N'IX_BOELaborSpread_BOELaborTypeID') 
	DROP INDEX [IX_BOELaborSpread_BOELaborTypeID] ON [dbo].[BOELaborSpread]
GO
/*
	   1/1/18 [twilson3] - BOEJ-2904 Removed LaborSpread Index
	   ## END ##
*/
/*
		## START ##
		1/15/18 [ranzalon] - BOEJ-2889 - Template Backup Data
*/

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[OutputFormatTemplateVersionXREF]') AND type in (N'U'))
BEGIN
	--Create new table
	CREATE TABLE [version].[OutputFormatTemplateVersionXREF] (
		VersionID int not null,
		BackupTemplateID int not null
	);
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[version].[OutputFormatTemplate]') AND name = N'version_OutputFormatTemplate_VersionID')
BEGIN
	--Add temporary id column for groups of UpdateDT and TemplateID to existing version table
	--Add temporary identity column - used for deleting the duplicate rows
	ALTER TABLE [version].[OutputFormatTemplate]
	ADD [GroupTemplateID] int, [TempID] int;
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[version].[OutputFormatTemplate]') AND name = N'version_OutputFormatTemplate_VersionID')
BEGIN
	--Populate tempid (not using identity to avoid issues with identity added later)
	declare @temp int = 0
	UPDATE [version].[OutputFormatTemplate]
	SET [TempID] = @temp, @temp = @temp + 1;
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[version].[OutputFormatTemplate]') AND name = N'version_OutputFormatTemplate_VersionID')
BEGIN
	--Populate GroupTemplateID, setting a unique ID for each pairing of TemplateID and UpdateDT
	--Use DENSE_RANK() to get an id value for each UpdateDT-TemplateID pair
	UPDATE [version].[OutputFormatTemplate]
	SET [GroupTemplateID] = oft2.TemplateRank
	FROM [version].[OutputFormatTemplate] oft1
	LEFT OUTER JOIN
	(
		SELECT TemplateID, UpdateDT, DENSE_RANK() OVER (ORDER BY UpdateDT, TemplateID) AS TemplateRank
		FROM [version].[OutputFormatTemplate]
	) AS oft2
	ON oft1.TemplateID = oft2.TemplateID
	AND oft1.UpdateDT = oft2.UpdateDT;
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[version].[OutputFormatTemplate]') AND name = N'version_OutputFormatTemplate_VersionID')
BEGIN
	--Populate OutputFormatTemplateVersionXREF
	INSERT INTO [version].[OutputFormatTemplateVersionXREF] (VersionID, BackupTemplateID)
	SELECT oft.VersionID, oft.GroupTemplateID 
	FROM [version].[OutputFormatTemplate] oft
	GROUP BY oft.VersionID, oft.GroupTemplateID;
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[version].[OutputFormatTemplate]') AND name = N'version_OutputFormatTemplate_VersionID')
BEGIN
	--delete now duplicate rows in version.OutputFormatTemplate
	DELETE FROM [version].[OutputFormatTemplate]
	WHERE TempID in 
	  (
		SELECT b.TempID
		FROM [version].[OutputFormatTemplate] a, [version].[OutputFormatTemplate] b
		WHERE a.GroupTemplateID = b.GroupTemplateID and b.tempid > a.tempid
	  );
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[version].[OutputFormatTemplate]') AND name = N'version_OutputFormatTemplate_VersionID')
BEGIN
	-- Remove index so VersionID can be dropped
	DROP INDEX [version_OutputFormatTemplate_VersionID] ON [version].[OutputFormatTemplate];
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'VersionID' AND Object_ID = Object_ID(N'[version].[OutputFormatTemplate]'))
BEGIN
	--Remove VersionID and TempID from [version].[OutputFormatTemplate]
	ALTER TABLE [version].[OutputFormatTemplate]
	DROP COLUMN VersionID, TempID;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'BackupTemplateID' AND Object_ID = Object_ID(N'[version].[OutputFormatTemplate]'))
BEGIN
	--Add BackupTemplateID identity column
	ALTER TABLE [version].[OutputFormatTemplate]
	ADD [BackupTemplateID] int IDENTITY(1,1);
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'GroupTemplateID' AND Object_ID = Object_ID(N'[version].[OutputFormatTemplate]'))
BEGIN
	--update xref table with new identity column values
	UPDATE [version].[OutputFormatTemplateVersionXREF]
	SET BackupTemplateID = OFT.BackupTemplateID
	FROM [version].[OutputFormatTemplate] OFT
	WHERE OFT.GroupTemplateID = version.OutputFormatTemplateVersionXREF.BackupTemplateID;
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'GroupTemplateID' AND Object_ID = Object_ID(N'[version].[OutputFormatTemplate]'))
BEGIN
	--remove GroupTemplateID
	ALTER TABLE [version].[OutputFormatTemplate]
	DROP COLUMN GroupTemplateID;
END
GO

/*
		1/15/18 [ranzalon] - BOEJ-2889 - Template Backup Data
		## END ##
*/

/*
		## START ##
		1/30/18 Dusan - BOEJ-3052 Update LOB name for RMS
*/
  UPDATE LineOfBusiness
	SET LineOfBusinessName = 'C6ISR',
		LineOfBusinessLongName = 'C6ISR',
		LineOfBusinessURL = 'C6ISR'
	WHERE LineOfBusinessName = 'C4ISR & Undersea Systems';

/*
		1/30/18 Dusan - BOEJ-3052 Update LOB name for RMS
		## END ##
*/