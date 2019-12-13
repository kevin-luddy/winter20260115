/*
	## START ##

	3/1/17 [twilson3] -- BOEJ-1861 Remove DTS / Distributed Time System
*/

-- set isDeleted flag to 1 for perf org where performingOrgName = 'DTS'
UPDATE [dbo].[PerformingOrganization] SET [DeletedFlag] = 1 WHERE [PerformingOrganizationName] = 'DTS'

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspaceResourceDTS]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspaceResourceDTS];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspaceRegionDTS]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspaceRegionDTS];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceResourceDTS]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceResourceDTS];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceRegionDTS]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceRegionDTS];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteAllWorkspaceResourceDTS]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteAllWorkspaceResourceDTS];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_BOE_AutoCalculateDTS]'))
ALTER TABLE [dbo].[BOE] DROP CONSTRAINT [DF_BOE_AutoCalculateDTS]
GO

DROP INDEX [IX_BOE_WorkspaceID] ON [dbo].[BOE]
GO

/****** Object:  Index [IX_BOE_WorkspaceID]    Script Date: 3/3/2017 1:34:21 PM ******/
CREATE NONCLUSTERED INDEX [IX_BOE_WorkspaceID] ON [dbo].[BOE]
(
	[WorkspaceID] ASC
)
INCLUDE ( 	[BOEID],
	[UpdateDT],
	[BOEStateID],
	[BOEStartDate],
	[BOEEndDate],
	[BOEDescription],
	[DataSource],
	[MetricDisclosureAcknowledge],
	[NumAuthorReassigned],
	[IsMaterial],
	[BOETitle]) WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = OFF, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO


IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'AutoCalculateDTS' AND Object_ID = Object_ID(N'[dbo].[BOE]'))
ALTER TABLE [dbo].[BOE] DROP COLUMN AutoCalculateDTS
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'AutoCalculateDTS' AND Object_ID = Object_ID(N'[version].[BOE]'))
ALTER TABLE [version].[BOE] DROP COLUMN AutoCalculateDTS
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'AutoCalculateDTS' AND Object_ID = Object_ID(N'[dbo].[BOECopySource]'))
ALTER TABLE [dbo].[BOECopySource] DROP COLUMN AutoCalculateDTS
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'AutoCalculateDTS' AND Object_ID = Object_ID(N'[dbo].[BOECopyTarget]'))
ALTER TABLE [dbo].[BOECopyTarget] DROP COLUMN AutoCalculateDTS
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_Workspace_DTSAutoCalculateID]'))
ALTER TABLE [dbo].[Workspace] DROP CONSTRAINT [DF_Workspace_DTSAutoCalculateID]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FK_Workspace_DTSAutoCalculateLU]'))
ALTER TABLE [dbo].[Workspace] DROP CONSTRAINT [FK_Workspace_DTSAutoCalculateLU]
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'DTSAutoCalculateID' AND Object_ID = Object_ID(N'[dbo].[Workspace]'))
ALTER TABLE [dbo].[Workspace] DROP COLUMN DTSAutoCalculateID
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'DTSAutoCalculateID' AND Object_ID = Object_ID(N'[version].[Workspace]'))
ALTER TABLE [version].[Workspace] DROP COLUMN DTSAutoCalculateID
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'DTSAutoCalculateID' AND Object_ID = Object_ID(N'[dbo].[WorkspaceCopySource]'))
ALTER TABLE [dbo].[WorkspaceCopySource] DROP COLUMN DTSAutoCalculateID
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'DTSAutoCalculateID' AND Object_ID = Object_ID(N'[dbo].[WorkspaceCopyTarget]'))
ALTER TABLE [dbo].[WorkspaceCopyTarget] DROP COLUMN DTSAutoCalculateID
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_Workspace_RestrictDTS]'))
ALTER TABLE [dbo].[Workspace] DROP CONSTRAINT [DF_Workspace_RestrictDTS]
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'RestrictDTS' AND Object_ID = Object_ID(N'[dbo].[Workspace]'))
ALTER TABLE [dbo].[Workspace] DROP COLUMN [RestrictDTS]
GO

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'RestrictDTS' AND Object_ID = Object_ID(N'[version].[Workspace]'))
ALTER TABLE [version].[Workspace] DROP COLUMN [RestrictDTS]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceResourceDTS]') AND type in (N'U'))
DROP TABLE [dbo].[WorkspaceResourceDTS]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceRegionDTS]') AND type in (N'U'))
DROP TABLE [dbo].[WorkspaceRegionDTS]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[WorkspaceResourceDTS]') AND type in (N'U'))
DROP TABLE [version].[WorkspaceResourceDTS]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[WorkspaceRegionDTS]') AND type in (N'U'))
DROP TABLE [version].[WorkspaceRegionDTS]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DTSAutoCalculateLU]') AND type in (N'U'))
DROP TABLE [dbo].[DTSAutoCalculateLU]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DTSRegionLU]') AND type in (N'U'))
DROP TABLE [dbo].[DTSRegionLU]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DTSResourceTypeLU]') AND type in (N'U'))
DROP TABLE [dbo].[DTSResourceTypeLU]
GO

-- Remove DTS from Sum Of Variable Lookup and XRef table
DELETE FROM [dbo].[WorkspaceVariableSumVariableResourceTypeXREF] WHERE [SumVariableResourceTypeID] in (SELECT [SumVariableResourceTypeID] FROM [dbo].[SumVariableResourceTypeLU] WHERE [SumVariableResourceType] = 'DTS')
DELETE FROM [version].[WorkspaceVariableSumVariableResourceTypeXREF] WHERE [SumVariableResourceTypeID] in (SELECT [SumVariableResourceTypeID] FROM [dbo].[SumVariableResourceTypeLU] WHERE [SumVariableResourceType] = 'DTS')
DELETE FROM [dbo].[SumVariableResourceTypeLU] WHERE [SumVariableResourceType] = 'DTS'

-- Delete Old Task Element Types
DELETE from [dbo].[TaskElementTypeLU] WHERE [TaskElementType] = 'DTS'
DELETE from [dbo].[TaskElementTypeLU] WHERE [TaskElementType] = 'Mission'

/*
	3/1/17 [twilson3] -- BOEJ-1861 Remove DTS / Distributed Time System

	## END ##
*/

/*
	## START ##
	
	3/7/2016 [Joe] - BOEJ-1818 Increase size of user's Display Name
*/

ALTER TABLE [ETIuser] ALTER COLUMN [DisplayName] varchar(256);

/*
	3/7/2016 [Joe] - BOEJ-1818 Increase size of user's Display Name

	## END ##
*/


EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2.15';
GO

/*
	## START ##
	
	4/25/2016 [RJ] - Added Project Map SSRS Reports
*/

IF(NOT EXISTS(SELECT 1 FROM [dbo].[ReportLU] WHERE ReportID BETWEEN 18 AND 30))
BEGIN
	INSERT INTO [dbo].[ReportLU] (ReportID, ReportName, Description)
	VALUES (18, 'Project Category CLIN Cost Summary', 'Project Category CLIN Cost Summary'),
		(19, 'Project CLIN Category Cost Summary', 'Project CLIN Category Cost Summary'),
		(20, 'Engr Project CLIN Cost Summary', 'Engr Project CLIN Cost Summary'),
		(21, 'Non-Engr Project CLIN Cost Summary', 'Non-Engr Project CLIN Cost Summary'),
		(22, 'Cost Analysis By CLIN, Resource, Activity & CY', 'Cost Analysis By Clin, Resource, Activity & Calendar Year (8 years)'),
		(23, 'Cost Analysis By CLIN, Activity & CY', 'Cost Analysis By Clin, Activity & Calendar Year (17 years)'),
		(24, 'DCMA Pavlo Template', 'DCMA Pavlo Template'),
		(25, 'BOE - Standard Report', 'BOE - Standard Report'),
		(26, 'Cost By Pricing Code (17 years)', 'Cost By Pricing Code (17 years)'),
		(27, 'Cost By Category - Pricing Code (17 years)', 'Cost By Category - Pricing Code (17 years)'),
		(28, 'Offload Cost By Year', 'Offload Cost By Year'),
		(29, 'Offload Cost Summary', 'Offload Cost Summary'),
		(30, 'Staffing Curves', 'Staffing Curves');
END
GO

/*
	4/25/2016 [RJ] - Added Project Map SSRS Reports

	## END ##
*/

/*
	## START ##
	
	4/28/2016 [RJ] - BOEJ-2119 DB work for Offload Rates
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemOffloadRate]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[SystemOffloadRate](
		OffloadRateID INT IDENTITY(1,1) NOT NULL,
		UpdateDT Datetime2(7) NOT NULL,
		Resource VARCHAR(20) NOT NULL,
		PerfOrg VARCHAR(20) NOT NULL,
		PercentToOffload DECIMAL(4,3) NOT NULL,
		Year INT NOT NULL,
		SubcontractorResource VARCHAR(20) NOT NULL,
		HourlyRate Decimal(7,2) NOT NULL,
		PRIMARY KEY (OffloadRateID),
		CONSTRAINT [Unique_SystemOffloadRate] UNIQUE
		(
			[Resource], [PerfOrg], [Year]
		)
	)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceOffloadRate]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[WorkspaceOffloadRate](
	OffloadRateID INT IDENTITY(1,1) NOT NULL,
	UpdateDT Datetime2(7) NOT NULL,
	WorkspaceID INT NOT NULL,
	Resource VARCHAR(20) NOT NULL,
	PerfOrg VARCHAR(20) NOT NULL,
	PercentToOffload DECIMAL(4,3) NOT NULL,
	Year INT NOT NULL,
	SubcontractorResource VARCHAR(20) NOT NULL,
	HourlyRate Decimal(7,2) NOT NULL,
	PRIMARY KEY (OffloadRateID),
	FOREIGN KEY (WorkspaceID) REFERENCES [dbo].[Workspace](WorkspaceID),
	CONSTRAINT [Unique_WorkspaceOffloadRate] UNIQUE
	(
		[Resource], [PerfOrg], [Year], [WorkspaceID]
	)
	)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[WorkspaceOffloadRate]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[WorkspaceOffloadRate](
	VersionID INT NOT NULL,
	OffloadRateID INT NOT NULL,
	UpdateDT Datetime2(7) NOT NULL,
	WorkspaceID INT NOT NULL,
	Resource VARCHAR(20) NOT NULL,
	PerfOrg VARCHAR(20) NOT NULL,
	PercentToOffload DECIMAL(4,3) NOT NULL,
	Year INT NOT NULL,
	SubcontractorResource VARCHAR(20) NOT NULL,
	HourlyRate Decimal(7,2) NOT NULL
	)
END
GO

/*
	4/28/2016 [RJ] - BOEJ-2119 DB work for Offload Rates

	## END ##
*/

/*
	## START ##

	4/28/2017 BOEJ-2129 [Greg Brunworth] -- Add a "Use T&M" setting
*/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsUsingTM' AND Object_ID = Object_ID('[dbo].[Workspace]'))
BEGIN
	ALTER TABLE [dbo].[Workspace]
		ADD IsUsingTM BIT NOT NULL
		CONSTRAINT DF_Workspace_IsUsingTM DEFAULT 0;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsUsingTM' AND Object_ID = Object_ID('[version].[Workspace]'))
BEGIN
	ALTER TABLE [version].[Workspace]
		ADD IsUsingTM BIT NOT NULL
		CONSTRAINT DF_Workspace_Version_IsUsingTM DEFAULT 0;
END
GO
/*
	4/28/2017 BOEJ-2129 [Greg Brunworth] -- Add a "Use T&M" setting

	## END ##
*/

/*
	## START ##
	
	4/25/2017 [twilson3] - BOEJ-2121 Project Map updates
*/

ALTER TABLE [dbo].[WorkBreakdownStructure] ALTER COLUMN [DisplayedWBSNumber] varchar(50);
ALTER TABLE [dbo].[WorkBreakdownStructure] ALTER COLUMN [WBSTitle] varchar(255);
ALTER TABLE [version].[WorkBreakdownStructure] ALTER COLUMN [DisplayedWBSNumber] varchar(50);
ALTER TABLE [version].[WorkBreakdownStructure] ALTER COLUMN [WBSTitle] varchar(255);
ALTER TABLE [dbo].[CLIN] ALTER COLUMN [DisplayedCLINNumber] varchar(50);
ALTER TABLE [version].[CLIN] ALTER COLUMN [DisplayedCLINNumber] varchar(50);

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'CanOffload' AND Object_ID = Object_ID(N'[dbo].[BOELaborType]'))
ALTER TABLE [dbo].[BOELaborType] ADD [CanOffload] bit default 0;
GO

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'CanOffload' AND Object_ID = Object_ID(N'[version].[BOELaborType]'))
ALTER TABLE [version].[BOELaborType] ADD [CanOffload] bit default 0;
GO

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'SOW' AND Object_ID = Object_ID(N'[dbo].[BOE]'))
BEGIN
	ALTER TABLE [dbo].[BOE] ADD [SOW] varchar(255) NULL;
	ALTER TABLE [dbo].[BOE] ADD [SOWTitle] varchar(255) NULL;
	ALTER TABLE [dbo].[BOE] ADD [Rationale] varchar(255) NULL;
	ALTER TABLE [dbo].[BOE] ADD [CamName] varchar(255) NULL;
	ALTER TABLE [dbo].[BOE] ADD [Category] varchar(255) NULL;
	ALTER TABLE [version].[BOE] ADD [SOW] varchar(255) NULL;
	ALTER TABLE [version].[BOE] ADD [SOWTitle] varchar(255) NULL;
	ALTER TABLE [version].[BOE] ADD [Rationale] varchar(255) NULL;
	ALTER TABLE [version].[BOE] ADD [CamName] varchar(255) NULL;
	ALTER TABLE [version].[BOE] ADD [Category] varchar(255) NULL;
END
GO
/*
	4/25/2017 [twilson3] - BOEJ-2121 Project Map updates

	## END ##
*/
/*
	## START ##
	
	5/03/17		ranzalon				BOEJ-2154 - Add ProjectMapTypeID Column to Workspace
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMapTypeLU]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[ProjectMapTypeLU](
		ProjectMapTypeID int not null,
		ProjectMapType varchar(30) not null,
		PRIMARY KEY (ProjectMapTypeID)
	);

	INSERT INTO [dbo].[ProjectMapTypeLU]
	VALUES
	(1, 'None'),
	(2, 'Time-Phased Project Map'),
	(3, 'Non-Time-Phased Project Map');
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'ProjectMapTypeID' AND OBJECT_ID = OBJECT_ID(N'[dbo].[Workspace]'))
	ALTER TABLE [dbo].[Workspace] ADD [ProjectMapTypeID] INT NOT NULL DEFAULT 1, FOREIGN KEY ([ProjectMapTypeID]) REFERENCES [ProjectMapTypeLU](ProjectMapTypeID);
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'ProjectMapTypeID' AND OBJECT_ID = OBJECT_ID(N'[version].[Workspace]'))
	ALTER TABLE [version].[Workspace] ADD [ProjectMapTypeID] INT NOT NULL DEFAULT 1;
GO

/*
	5/03/17		ranzalon				BOEJ-2154 - Add ProjectMapTypeID Column to Workspace

	## END ##
*/

/*
	## START ##
	
	5/04/2016 [brunworg] - Add T&M Resource Rates table
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TMResourceRate]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[TMResourceRate](
		[TMResourceRateID] [int] IDENTITY(1,1) NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] [int] NOT NULL,
		[TMResourceID] [int] NOT NULL,
		[TMResourceRateStartDate] [date] NULL,
		[TMResourceRateEndDate] [date] NULL,
		[TMResourceRate] [decimal](7, 2) NULL,
		PRIMARY KEY (TMResourceRateID),
		FOREIGN KEY (WorkspaceID) REFERENCES [dbo].[Workspace](WorkspaceID),
		FOREIGN KEY (TMResourceID) REFERENCES [dbo].[Resource](ResourceID)
	)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[TMResourceRate]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[TMResourceRate](
		[VersionID] [int] NOT NULL,
		[TMResourceRateID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] [int] NOT NULL,
		[TMResourceID] [int] NOT NULL,
		[TMResourceRateStartDate] [date] NULL,
		[TMResourceRateEndDate] [date] NULL,
		[TMResourceRate] [decimal](7, 2) NULL
	)
END
GO
/*
	5/04/2016 [brunworg] - Add T&M Resource Rates table

	## END ##
*/

/*
	## START ##
	
	5/10/2017	ranzalon			BOEJ-2053 - Remove Proposal Type
*/
IF OBJECT_ID('FinancialLeadershipAcademy_getLaborBOECount', 'P') IS NOT NULL
DROP PROCEDURE [dbo].[FinancialLeadershipAcademy_getLaborBOECount]
GO

IF OBJECT_ID('FinancialLeadershipAcademy_getLaborBOEHours', 'P') IS NOT NULL
DROP PROCEDURE [dbo].[FinancialLeadershipAcademy_getLaborBOEHours]
GO

IF OBJECT_ID('FinancialLeadershipAcademy_getProposalType', 'P') IS NOT NULL
DROP PROCEDURE [dbo].[FinancialLeadershipAcademy_getProposalType]
GO

IF OBJECT_ID('FinancialLeadershipAcademy_getQuotingMethodCount', 'P') IS NOT NULL
DROP PROCEDURE [dbo].[FinancialLeadershipAcademy_getQuotingMethodCount]
GO

IF OBJECT_ID('RealignWorkspace', 'P') IS NOT NULL
DROP PROCEDURE [dbo].[RealignWorkspace]
GO 

IF OBJECT_ID('RealignWorkspacesJob', 'P') IS NOT NULL
DROP PROCEDURE [dbo].[RealignWorkspacesJob]
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Workspace_ProposalTypeLU]')
				AND parent_object_id = OBJECT_ID(N'[dbo].[Workspace]'))
	ALTER TABLE [dbo].[Workspace]
	DROP CONSTRAINT [FK_Workspace_ProposalTypeLU];
	
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Workspace]') AND name = N'IX_Workspace_C1')
DROP INDEX [IX_Workspace_C1] ON [dbo].[Workspace] WITH ( ONLINE = OFF )
GO

IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'ProposalTypeID' AND OBJECT_ID = OBJECT_ID(N'[dbo].[Workspace]'))
	ALTER TABLE [dbo].[Workspace]
	DROP COLUMN [ProposalTypeID];

IF OBJECT_ID('[dbo].[ProposalTypeLU]', 'U') IS NOT NULL
	DROP TABLE [dbo].[ProposalTypeLU];
	
IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'ProposalTypeID' AND OBJECT_ID = OBJECT_ID(N'[dbo].[WorkspaceCopySource]'))
	ALTER TABLE [dbo].[WorkspaceCopySource]
	DROP COLUMN [ProposalTypeID];

IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'ProposalType' AND OBJECT_ID = OBJECT_ID(N'[dbo].[WorkspaceCopySource]'))
	ALTER TABLE [dbo].[WorkspaceCopySource]
	DROP COLUMN [ProposalType];
		
IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'ProposalTypeID' AND OBJECT_ID = OBJECT_ID(N'[dbo].[WorkspaceCopyTarget]'))
	ALTER TABLE [dbo].[WorkspaceCopyTarget]
	DROP COLUMN [ProposalTypeID];

IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'ProposalType' AND OBJECT_ID = OBJECT_ID(N'[dbo].[WorkspaceCopyTarget]'))
	ALTER TABLE [dbo].[WorkspaceCopyTarget]
	DROP COLUMN [ProposalType];
	
IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'ProposalTypeID' AND OBJECT_ID = OBJECT_ID(N'[version].[Workspace]'))
	ALTER TABLE [version].[Workspace]
	DROP COLUMN [ProposalTypeID];
	
CREATE NONCLUSTERED INDEX [IX_Workspace_C1] ON [dbo].[Workspace] 
(
	[WorkspaceStateID] ASC,
	[ContainsOCI] ASC,
	[AllowSearch] ASC
)
INCLUDE ( [WorkspaceID],
[WorkspaceName],
[ProposalSubmitDate],
[CostVolumeLeadPricerUserID]) WITH (PAD_INDEX  = ON, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = OFF, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 80) ON [PRIMARY]
GO

/*
	5/10/2017	ranzalon			BOEJ-2053 - Remove Proposal Type

	## END ##
*/

/*
	## START ##
	
	5/11/17		pattoncr - BOEJ-2151 - Additional SSRS Work for the Proof of Concept
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Configuration]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Configuration](
		ConfigurationID INT IDENTITY(1,1) NOT NULL,
		ConfigurationKey VARCHAR(256) NOT NULL,
		ConfigurationValue VARCHAR(256) NOT NULL,
		PRIMARY KEY (ConfigurationID),
		CONSTRAINT [Unique_Configuration] UNIQUE
		(
			[ConfigurationKey]
		)
	)
	
	-- Create the BaseSiteURL value needed for SSRS.
	insert into [dbo].[Configuration] values ('BaseSiteURL', 'http://dev-genboerms.us.lmco.com/');
END
GO

/*
	5/11/17		pattoncr - BOEJ-2151 - Additional SSRS Work for the Proof of Concept
	
	## END ##
*/

/*
	## START ##
	
	5/17/17		Mike - BOEJ-2174 - Add Sikorsky LOB into RMS (and RMS only)
*/

IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessId > 2001 AND LineOfBusinessId < 2999)
BEGIN
       IF NOT EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessName = 'Sikorsky')
       BEGIN
              INSERT INTO [dbo].[LineOfBusiness] (LineOfBusinessName, LineOfBusinessLongName, LineOfBusinessURL, ProductLineID, ForesightLineOfBusinessID, IsActive)
                     VALUES ('Sikorsky', 'Sikorsky', 'Sikorsky', 1, -1, 1)
       END
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'LineOfBusinessLU')
	DROP TABLE [dbo].[LineOfBusinessLU];
GO
/*
	5/17/17		Mike - BOEJ-2174 - Add Sikorsky LOB into RMS (and RMS only)
	
	## END ##
*/

/*
	## START ##
	
	5/22/17		Dusan - BOEJ-2181 Add "IsAddOrDelete" column
*/

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsAddOrDelete' AND Object_ID = Object_ID(N'[dbo].[BOELaborType]'))
	ALTER TABLE [dbo].[BOELaborType] ADD [IsAddOrDelete] bit;
GO

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsAddOrDelete' AND Object_ID = Object_ID(N'[version].[BOELaborType]'))
	ALTER TABLE [version].[BOELaborType] ADD [IsAddOrDelete] bit;
GO

/*
	5/22/17		Dusan - BOEJ-2181 Add "IsAddOrDelete" column
	
	## END ##
*/

/*
	## START ##
	
	5/23/17		Mike - BOEJ-2172 - WS Project Map enum change
*/

IF EXISTS (SELECT 1 FROM [dbo].[ProjectMapTypeLU] WHERE ProjectMapTypeID = 1 and ProjectMapType = 'None')
BEGIN
	UPDATE [dbo].[ProjectMapTypeLU] set ProjectMapType = 'Standard without Offload'
	where ProjectMapTypeID = 1
END

IF EXISTS (SELECT 1 FROM [dbo].[ProjectMapTypeLU] WHERE ProjectMapTypeID = 2 and ProjectMapType = 'Time-Phased Project Map')
BEGIN
	UPDATE [dbo].[ProjectMapTypeLU] set ProjectMapType = 'Standard with Offload'
	where ProjectMapTypeID = 2
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[ProjectMapTypeLU] WHERE ProjectMapTypeID = 4)
BEGIN
	insert into [dbo].[ProjectMapTypeLU] (ProjectMapTypeID, ProjectMapType) 
	values (4, 'Time-Phased Project Map')
END
	   
/*
	## END ##
	
	5/23/17		Mike - BOEJ-2172 - WS Project Map enum change
*/
EXEC [dbo].[UpdateDbVersion] @DbVersion = '2', @AppVersion = '2.15';
GO

/*
	## START ##
	
	5/23/17		RJ - BOEJ-2198 - Output Format Template
*/

SET IDENTITY_INSERT [dbo].[OutputFormatTemplate] ON
IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessId > 2001 AND LineOfBusinessId < 2999) -- RMS only
	AND NOT EXISTS(SELECT 1 FROM [dbo].[OutputFormatTemplate] WHERE TemplateID = 2008) -- does not exist yet
BEGIN
	INSERT INTO [dbo].[OutputFormatTemplate] (TemplateID, UpdateDT, Template, TemplateDescription, IsActive, ParentTemplateID) 
		VALUES (2008, GETDATE(), 'RMS - Sikorsky Project Map', 'Standard Project Map template for Sikorsky', 1, 2008)
END
SET IDENTITY_INSERT [dbo].[OutputFormatTemplate] OFF

GO
/*
	5/23/17		RJ - BOEJ-2198 - Output Format Template
	
	## END ##
*/

/*
	## START ##
	
	5/23/17		RJ - BOEJ-2198 - Output Format Template - Add project map to all existing workspaces
*/
IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessId > 2001 AND LineOfBusinessId < 2999) -- RMS only
BEGIN
	INSERT INTO [dbo].[OutputFormatTemplateWorkspaceXREF] (WorkspaceID, TemplateID)
		SELECT WorkspaceID, 2008 AS TemplateID 
			FROM [dbo].[Workspace] 
			WHERE WorkspaceID NOT IN (SELECT WorkspaceID FROM [dbo].[OutputFormatTemplateWorkspaceXREF] WHERE TemplateID = 2008);
END
GO
/*
	5/23/17		RJ - BOEJ-2198 - Output Format Template
	
	## END ##
*/

/*
	## START ##

	6/8/2017	RJ - BOEJ-2158 - Rename Reports
*/
UPDATE [dbo].[ReportLU]
	SET [ReportName] = 'Category/CLIN Summary',
		[Description] = 'Summary estimate at cost sorted by category, CLIN, activity, resource, start and end date'
	WHERE ReportID = 18;

UPDATE [dbo].[ReportLU]
	SET [ReportName] = 'CLIN/Category Summary',
		[Description] = 'Summary estimate at cost sorted by CLIN, category, activity, resource, start and end date'
	WHERE ReportID = 19;

UPDATE [dbo].[ReportLU]
	SET [ReportName] = 'Engr CLIN Summary',
		[Description] = 'Engineering summary estimate at cost sorted by CLIN, activity, resource, start and end date'
	WHERE ReportID = 20;

UPDATE [dbo].[ReportLU]
	SET [ReportName] = 'Non-Engr CLIN Summary',
		[Description] = 'Non-Engineering summary estimate at cost sorted by CLIN, activity, resource, start and end date'
	WHERE ReportID = 21;

UPDATE [dbo].[ReportLU]
	SET [ReportName] = 'Cost by CLIN, Res, Act & Yr (8yrs)',
		[Description] = 'Summary estimate at cost sorted by CLIN, activity, resource, pricing code and year for an 8 year period with an individual sequence # referencing an individual basis of estimate'
	WHERE ReportID = 22;

UPDATE [dbo].[ReportLU]
	SET [ReportName] = 'Cost by CLIN, Act & Yr (17yrs)',
		[Description] = 'Summary estimate at cost sorted by CLIN, activity, resource, pricing code and year for a 17 year period with an individual sequence # referencing an individual basis of estimate'
	WHERE ReportID = 23;

UPDATE [dbo].[ReportLU]
	SET [ReportName] = 'BOE Summary Report',
		[Description] = 'Combined basis of estimate word document created for each activity in the project map with data for activity, CLIN, resource, WBS, POP dates, task, rationale, labor hours, cost dollars'
	WHERE ReportID = 24;
	
UPDATE [dbo].[ReportLU]
	SET [ReportName] = 'Standard Reports', 
		[Description] = 'Runs all standard reports: ''Cost by CLIN, Res, Act & Yr (8yrs)'', ''Cost by CLIN, Act & Yr (17yrs)'', ''BOE Summary Report'', and ''All BOEs'''
	WHERE [ReportID] = 25
	
UPDATE [dbo].[ReportLU]
	SET [ReportName] = 'By Pricing Code (17 yrs)',
		[Description] = 'Summary estimate at cost sorted by pricing code, resource and year for a 17 year period'
	WHERE ReportID = 26;
	
UPDATE [dbo].[ReportLU]
	SET [ReportName] = 'By Cat/Pricing Code (17 yrs)',
		[Description] = 'Summary estimate at cost sorted by category, pricing code, resource and year for a 17 year period'
	WHERE ReportID = 27;
	
UPDATE [dbo].[ReportLU]
	SET [Description] = 'Detailed summary by year of the offload calculation for each new offload activity generated in BOEDB and added to the post-offload project map'
	WHERE ReportID = 28;
	
UPDATE [dbo].[ReportLU]
	SET [Description] = 'Detailed summary of the offload calculation for each new offload activity generated in BOEDB and added to the post-offload project map'
	WHERE ReportID = 29;
	
UPDATE [dbo].[ReportLU]
	SET [Description] = 'Staffing graph'
	WHERE ReportID = 30;
/*
	6/8/2017	RJ - BOEJ-2158 - Rename Reports

	## END ##
*/

/*
	## START ##

	6/9/2017	RJ - BOEJ-2158 - Add new reports
*/
IF NOT EXISTS (SELECT 1 FROM [dbo].[ReportLU] WHERE [ReportID] >= 31 AND [ReportID] <=34)
BEGIN
INSERT INTO [dbo].[ReportLU] ([ReportID], [ReportName], [Description])
VALUES (31, 'RPS', 'Requirements Planning System - Summary of total hours and dollars totaled by resource and spread by month'),
	(32, 'PRP', 'Program Requirements Plan - Summary showing hours and dollars for each activity in the project map spread by month'),
	(33, 'RAM', 'Responsibility Assignment Matrix - Matrix showing resources estimated across the top, WBS and activity ID down the side and their corresponding estimates'),
	(34, 'Pre vs Post Offload Totals', 'Compares Pre-offload project map totals to post-offload project map totals with a total value for offload dollars and decremented labor hours');
END


/*
	6/9/2017	RJ - BOEJ-2158 - Add new reports

	## END ##
*/