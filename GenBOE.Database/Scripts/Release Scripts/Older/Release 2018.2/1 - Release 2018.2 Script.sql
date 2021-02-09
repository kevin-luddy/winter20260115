EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.2';
GO

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
	   1/16/18 [twilson3] - BOEJ-2887 Remove Historical Metrics
*/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MetricAdminGroup]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[BOETaskElementMetricXREF];
	DROP TABLE [dbo].[MetricAdminUserRole];
	DROP TABLE [dbo].[Metric];
	DROP TABLE [dbo].[MetricAdminGroup];
	DROP TABLE [dbo].[MetricStatusLU];
	DROP TABLE [dbo].[UsageLU];
	DROP TABLE [dbo].[LaborSourceLU];


	DROP TABLE [version].[BOETaskElementMetricXREF];
	DROP TABLE [version].[MetricAdminUserRole];
	DROP TABLE [version].[MetricAdminGroup];
	DROP TABLE [version].[Metric];
	
END
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getMetricBySearch]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getMetricBySearch];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOETaskElementMetric]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOETaskElementMetric];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMetricAdminGroup]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMetricAdminGroup];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMetricAdminUserRoleByETIUserAndGroupID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMetricAdminUserRoleByETIUserAndGroupID];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteHistoricalMetric]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteHistoricalMetric];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getHistoricalMetricSearchBox]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getHistoricalMetricSearchBox];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOETaskElementMetric]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOETaskElementMetric];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertMetricAdminUserRole]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertMetricAdminUserRole];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertHistoricalMetric]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertHistoricalMetric];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertMetricAdminGroup]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertMetricAdminGroup];
GO

/*
	   1/16/18 [twilson3] - BOEJ-2887 Remove Historical Metrics
	   ## END ##
*/
/*
	   ## START ##
	   1/25/18 [twilson3] - BOEJ-2972 Merge Backup Sprocs
*/
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[createSystemWorkspaceVersion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[createSystemWorkspaceVersion];
GO

/*
	   1/25/18 [twilson3] - BOEJ-2972 Merge Backup Sprocs
	   ## END ##
*/

/*
	   ## START ##
	   1/31/18 [pattoncr] - BOEJ-2979 Zone Travel Admin Issues
*/
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'LookupValue' AND Object_ID = Object_ID('[dbo].[MSTZoneTravelResource]'))
BEGIN
	ALTER TABLE [dbo].[MSTZoneTravelResource] ALTER COLUMN [LookupValue] VARCHAR(120);
END
GO

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'Description' AND Object_ID = Object_ID('[dbo].[MSTZoneTravelResource]'))
BEGIN
	ALTER TABLE [dbo].[MSTZoneTravelResource] ALTER COLUMN [Description] VARCHAR(120);
END
GO
/*
	   1/31/18 [pattoncr] - BOEJ-2979 Zone Travel Admin Issues
	   ## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '2', @AppVersion = '2018.2';
GO

/*
	   ## START ##
	   2/8/18 [Dusan] - BOEJ-3074 Remove RMS LOB
*/

UPDATE LineOfBusiness
	SET IsActive = 0
	WHERE LineOfBusinessName = 'Cyber, Ships and Advanced Technologies';

/*
	   2/8/18 [Dusan] - BOEJ-3074 Remove RMS LOB
	   ## END ##
*/