EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.2';
GO

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