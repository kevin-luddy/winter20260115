/*
	## START ##

	1/3/17 [Chris] -- BOEJ-1651 Pro Pricer Export Template Linkage
*/

-- Move existing and backup system templates to align with the default WorkspaceID.
UPDATE dbo.ProPricerExport SET WorkspaceID = 1 where ProPricerScopeID = 2 and WorkspaceID != 1;
UPDATE version.ProPricerExport SET WorkspaceID = 1 where ProPricerScopeID = 2 and WorkspaceID != 1;
GO

/*
	1/3/17 [Chris] -- BOEJ-1651 Pro Pricer Export Template Linkage

	## END ##
*/



EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2.14';
GO

/*
	## START ##

	1/16/2017 [twilson3] - BOEJ-1604 - Cleanup DB Project
*/

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[generationMaintenance]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[generationMaintenance];
GO

IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[IndexFragmentation]') AND name = N'IX_IndexFragmentation__TableName_IndexName_FillFactor') 
	DROP INDEX [IX_IndexFragmentation__TableName_IndexName_FillFactor] ON [dbo].[IndexFragmentation] WITH ( ONLINE = OFF )

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'IndexFragmentation')
	DROP TABLE dbo.IndexFragmentation;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[vwHistoricalMetrics]') AND type in (N'V'))
	DROP VIEW [dbo].[vwHistoricalMetrics]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[vwRefx_Workspace]') AND type in (N'V'))
	DROP VIEW [dbo].[vwRefx_Workspace]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOEPotentialRolebyUserAndGroupID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOEPotentialRolebyUserAndGroupID];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOETaskElementgenDataMetric]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOETaskElementgenDataMetric];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOETaskElementgenDataMetric]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOETaskElementgenDataMetric];
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'BOETaskElementgenDataMetricXREF')
	ALTER TABLE [dbo].[BOETaskElementgenDataMetricXREF] DROP CONSTRAINT [PK_BOETaskElementgenDataMetricXREF]
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'BOETaskElementgenDataMetricXREF')
	DROP TABLE [dbo].[BOETaskElementgenDataMetricXREF]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteCustomFieldValueByCustomFieldID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteCustomFieldValueByCustomFieldID];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteCustomForm]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteCustomForm];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteCustomFormInputs]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[DeleteCustomFormInputs];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertCustomForm]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertCustomForm];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpsertCustomFormInputs]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[UpsertCustomFormInputs];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomFormSelectionsXREF]') AND type in (N'U'))
	DROP TABLE [dbo].[CustomFormSelectionsXREF]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[CustomFormSelectionsXREF]') AND type in (N'U'))
	DROP TABLE [version].[CustomFormSelectionsXREF]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[CustomFormInputs]') AND type in (N'U'))
	DROP TABLE [version].[CustomFormInputs]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomFormInputs]') AND type in (N'U'))
	DROP TABLE [dbo].[CustomFormInputs]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomForm]') AND type in (N'U'))
	DROP TABLE [dbo].[CustomForm]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteETIUser]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteETIUser];
GO

-- Noone calls this sproc, they just delete from the table directly
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOECommentHistory]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOECommentHistory];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deletePerformingOrganizationList]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deletePerformingOrganizationList];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSystemRolesByETIUserAndGroupID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSystemRolesByETIUserAndGroupID];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceRolesByETIUserAndGroupID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceRolesByETIUserAndGroupID];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getBOEByWorkspaceID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getBOEByWorkspaceID];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getCLINByWorkspaceID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getCLINByWorkspaceID];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspace]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspace];
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'foresightProgram')
ALTER TABLE [dbo].[foresightProgram] DROP CONSTRAINT [PK_ForesightProgram]
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'foresightProgram')
	DROP TABLE [dbo].[foresightProgram]
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'IsgsPerfOrgXREF')
	ALTER TABLE [dbo].[IsgsPerfOrgXREF] DROP CONSTRAINT [PK_IsgsPerfOrgXREF]
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'IsgsPerfOrgXREF')
	DROP TABLE [dbo].[IsgsPerfOrgXREF]
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'IsgsResourceXREF')
	ALTER TABLE [dbo].[IsgsResourceXREF] DROP CONSTRAINT [PK_IsgsResourceXREF]
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'IsgsResourceXREF')
	DROP TABLE [dbo].[IsgsResourceXREF]
GO


/*
	1/16/2017 [twilson3] - BOEJ-1604 - Cleanup DB Project

	## END ##
*/ 

/*
	## START ##

	1/26/2017 [pattoncr] - BOEJ-1746 genBOE DLA Template - error message
*/

ALTER TABLE [dbo].[BOETaskElement] ALTER COLUMN [MOQHoursEquation] VARCHAR(500) NULL;
ALTER TABLE [version].[BOETaskElement] ALTER COLUMN [MOQHoursEquation] VARCHAR(500) NULL;
GO

/*
	1/26/2017 [pattoncr] - BOEJ-1746 genBOE DLA Template - error message
	
	## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '2', @AppVersion = '2.14';
GO