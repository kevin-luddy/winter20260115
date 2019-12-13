EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.5.1';
GO

/*
		## START ##
		7/2/18 Dusan - BOEJ-3650: Rename reports
*/
UPDATE ReportLU SET ReportName = 'BOE Summary Report' WHERE ReportName = 'BOE - Standard Report';
UPDATE ReportLU SET Description = 'Runs all standard reports' WHERE ReportName = 'Standard Reports';

/*
		7/2/18 Dusan - BOEJ-3650: Rename reports
		## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.6';
GO

/*
		## START ##
		6/14/18 ranzalon - BOEJ-3448 - ProPricer Updates
*/

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LastProPricerInstance' AND Object_ID = Object_ID(N'[dbo].[Workspace]'))
BEGIN
	ALTER TABLE [dbo].[Workspace]
	ADD [LastProPricerInstance] int NULL,
		[LastProPricerProposal] varchar(50) NULL;

	ALTER TABLE [version].[Workspace]
	ADD [LastProPricerInstance] int NULL,
		[LastProPricerProposal] varchar(50) NULL;
END
GO

/*
		6/14/18 ranzalon - BOEJ-3448 - ProPricer Updates
		## END ##
*/

/*
		## START ##
		6/28/18 twilson3 - BOEJ-3551	HomePage Updates
*/
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceUserXREF]') AND type in (N'U'))
BEGIN
	--Create new table
	CREATE TABLE [dbo].[WorkspaceUserXREF] (
		WorkspaceUserID int IDENTITY(1,1) not null,
		[ETIUserId] int not null,
		[WorkspaceID] int not null,
		[IsFavorite] bit null,
		[LastAccessed] datetime2 null,
		UpdateDT datetime2 not null
	CONSTRAINT [PK_WorkspaceUserXREF] PRIMARY KEY CLUSTERED 
	(
		[ETIUserId] ASC,
		[WorkspaceID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[WorkspaceUserXREF] WITH CHECK ADD CONSTRAINT [FK_WorkspaceUserXREF_UserId] FOREIGN KEY ([ETIUserId]) REFERENCES [dbo].[ETIuser]([ETIUserId]);
	ALTER TABLE [dbo].[WorkspaceUserXREF] WITH CHECK ADD CONSTRAINT [FK_WorkspaceUserXREF_Workspace] FOREIGN KEY ([WorkspaceID]) REFERENCES [dbo].[Workspace]([WorkspaceID]);

END
GO
/*
		6/28/18 twilson3 - BOEJ-3551	HomePage Updates
		## END ##
*/
/*
		## START ##
		6/28/18 brunworg - BOEJ-3608	Offload Text - DB work
*/
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemSetting]') AND type in (N'U'))
BEGIN
	--Create new table
	CREATE TABLE [dbo].[SystemSetting] (
		[Key] varchar(255) NOT NULL,
		[Value] varchar(4000) NULL
	CONSTRAINT [PK_SystemSetting] PRIMARY KEY NONCLUSTERED
	(
		[Key] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	INSERT INTO [dbo].[SystemSetting] ([Key], [Value]) VALUES ('ProjectMapOffloadText', 'located in CD folder: VIII Estimating Backup.');
	INSERT INTO [dbo].[SystemSetting] ([Key], [Value]) VALUES ('JustifyingPublication', 'AEM-18-0013');
END
GO
/*
		6/28/18 brunworg - BOEJ-3608	Offload Text - DB work
		## END ##
*/