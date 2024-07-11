EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.10';
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SkillMix]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[SkillMix] (
		[SkillMixID] int IDENTITY(1,1) PRIMARY KEY CLUSTERED
		,[Rationale] varchar(255) NOT NULL
		,[Included] bit DEFAULT 0
		,[ProposedHours] decimal(11, 2) NOT NULL
		,[HistoricalHours] decimal(11, 2) NOT NULL
		,[BOESkillMix] decimal(5, 2) NOT NULL
		,[LaborSkillMix] decimal(5, 2) NOT NULL
		,[ResourceOld] [varchar](20) NOT NULL
		,[ResourceNew] [varchar](20) NOT NULL
		,[BOEID] int NOT NULL
		,[BOETaskElementID] int NOT NULL
		,[MOQTypeSelectionID] int NOT NULL
		,[IsPercentLocked] bit DEFAULT 0
		,CONSTRAINT FK_SkillMix_BOE FOREIGN KEY (BOEID) REFERENCES [dbo].[BOE] ([BOEID])
		,CONSTRAINT FK_SkillMix_BOETaskElement FOREIGN KEY(BOETaskElementID) REFERENCES [dbo].[BOETaskElement] ([BOETaskElementID])
		,CONSTRAINT FK_SkillMix_MOQTypeSelection FOREIGN KEY(MOQTypeSelectionID) REFERENCES [dbo].[MOQTypeSelection] ([MOQTypeSelectionId])
	)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[SkillMix]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[SkillMix]
	(
		[SkillMixID] int NOT NULL
		,[Rationale] varchar(255) NOT NULL
		,[Included] bit DEFAULT 0
		,[ProposedHours] decimal(11, 2) NOT NULL
		,[HistoricalHours] decimal(11, 2) NOT NULL
		,[BOESkillMix] decimal(5, 2) NOT NULL
		,[LaborSkillMix] decimal(5, 2) NOT NULL
		,[ResourceOld] [varchar](20) NOT NULL
		,[ResourceNew] [varchar](20) NOT NULL
		,[BOEID] int NOT NULL
		,[BOETaskElementID] int NOT NULL
		,[MOQTypeSelectionID] int NOT NULL
		,[IsPercentLocked] bit NOT NULL
		,[VersionID] int NOT NULL
	);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MOQTypeSelectionTableDataResourceHours]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[MOQTypeSelectionTableDataResourceHours] (
		[MOQTypeSelectionTableDataResourceHoursId] int IDENTITY(1,1) PRIMARY KEY CLUSTERED,
		[ResourceName] varchar(20) NULL,
		[WbsHours] decimal(11,2) NOT NULL,
		[TotalHours] decimal(11,2) NOT NULL,
		[MOQTypeSelectionTableDataId] int NOT NULL,
		[BOETaskElementID] int NOT NULL,
		[BOEID] int NOT NULL,
		CONSTRAINT FK_MOQTypeSelectionTableDataResourceHours_MOQTypeSelectionTableDataId FOREIGN KEY(MOQTypeSelectionTableDataId) REFERENCES [dbo].[MOQTypeSelectionTableData] ([MOQTypeSelectionTableDataId]),
		CONSTRAINT FK_MOQTypeSelectionTableDataResourceHours_BOETaskElement FOREIGN KEY(BOETaskElementID) REFERENCES [dbo].[BOETaskElement] ([BOETaskElementID]),
		CONSTRAINT FK_MOQTypeSelectionTableDataResourceHours_BOE FOREIGN KEY (BOEID) REFERENCES [dbo].[BOE] (BOEID)
	);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[MOQTypeSelectionTableDataResourceHours]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[MOQTypeSelectionTableDataResourceHours]
	(
		[MOQTypeSelectionTableDataResourceHoursId] int NOT NULL,
		[ResourceName] varchar(20) NULL,
		[WbsHours] decimal(11,2) NOT NULL,
		[TotalHours] decimal(11,2) NOT NULL,
		[MOQTypeSelectionTableDataId] int NOT NULL,
		[BOETaskElementID] int NOT NULL,
		[BOEID] int NOT NULL,
		[VersionID] int NOT NULL,
	);
END
GO

-- 06/17/2024 - twilson3 - PROPH-2065 Missing Index causing Snapshot Transaction to Fail
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BOELaborType]') AND name = N'IX_BOELaborType__BRCResourceID') 
	CREATE NONCLUSTERED INDEX [IX_BOELaborType__BRCResourceID] ON [dbo].[BOELaborType]
	(
		[BRCResourceID] ASC
	)
	INCLUDE([BOETaskElementID]) WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO