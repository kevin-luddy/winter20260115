EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.14';
GO

-- 10/01/2025 Yemi Oyetoro (e378233), PROPH-3302 - (Database) SKILL MIX - Summary Table

IF OBJECT_ID('dbo.SkillMixSummary', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SkillMixSummary](
	[SkillMixSummaryID] [int] IDENTITY(1,1) NOT NULL,
	[Rationale] [varchar](255) NOT NULL,
	[Included] [bit] NOT NULL,
	[ProposedHours] [decimal](11, 2) NOT NULL,
	[HistoricalHours] [decimal](11, 2) NOT NULL,
	[ResourceHours] [decimal](11, 2) NOT NULL,
	[BusinessResourceHours] [decimal](11, 2) NOT NULL,
	[BOESkillMix] [decimal](5, 2) NOT NULL,
	[LaborSkillMix] [decimal](5, 2) NOT NULL,
	[ResourceID] [varchar](20) NOT NULL,
	[BusinessResourceID] [varchar](20) NOT NULL,
	[BOEID] [int] NOT NULL,
	[BOETaskElementID] [int] NOT NULL,
	[IsUserInput] [bit] NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[SkillMixSummaryID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[SkillMixSummary] ADD  DEFAULT ((0)) FOR [Included]

	ALTER TABLE [dbo].[SkillMixSummary] ADD  DEFAULT ((0)) FOR [IsUserInput]

	ALTER TABLE [dbo].[SkillMixSummary]  WITH CHECK ADD  CONSTRAINT [FK_SkillMixSummary_BOE] FOREIGN KEY([BOEID])
	REFERENCES [dbo].[BOE] ([BOEID])

	ALTER TABLE [dbo].[SkillMixSummary] CHECK CONSTRAINT [FK_SkillMixSummary_BOE]

	ALTER TABLE [dbo].[SkillMixSummary]  WITH CHECK ADD  CONSTRAINT [FK_SkillMixSummary_BOETaskElement] FOREIGN KEY([BOETaskElementID])
	REFERENCES [dbo].[BOETaskElement] ([BOETaskElementID])

	ALTER TABLE [dbo].[SkillMixSummary] CHECK CONSTRAINT [FK_SkillMixSummary_BOETaskElement]
END


IF OBJECT_ID('version.SkillMixSummary', 'U') IS NULL
BEGIN
	CREATE TABLE [version].[SkillMixSummary](
		[SkillMixSummaryID] [int] NOT NULL,
		[Rationale] [varchar](255) NOT NULL,
		[Included] [bit] NOT NULL,
		[ProposedHours] [decimal](11, 2) NOT NULL,
		[HistoricalHours] [decimal](11, 2) NOT NULL,
		[ResourceHours] [decimal](11, 2) NOT NULL,
		[BusinessResourceHours] [decimal](11, 2) NOT NULL,
		[BOESkillMix] [decimal](5, 2) NOT NULL,
		[LaborSkillMix] [decimal](5, 2) NOT NULL,
		[ResourceID] [varchar](20) NOT NULL,
		[BusinessResourceID] [varchar](20) NOT NULL,
		[BOEID] [int] NOT NULL,
		[BOETaskElementID] [int] NOT NULL,
		[IsUserInput] [bit] NOT NULL,
		[VersionID] [int] NOT NULL
	) ON [PRIMARY]

	ALTER TABLE [version].[SkillMixSummary] ADD  DEFAULT ((0)) FOR [Included]
END
GO

-- Drop/Create TT_SkillMixSummary
IF EXISTS (
    SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = 'TT_SkillMixSummary' AND schema_id = SCHEMA_ID('dbo')
)
    DROP TYPE [dbo].[TT_SkillMixSummary];
GO

/****** Object:  UserDefinedTableType [dbo].[TT_SkillMixSummary]    Script Date: 9/30/2025 10:28:15 PM ******/
CREATE TYPE [dbo].[TT_SkillMixSummary] AS TABLE(
	[Rationale] [varchar](255) NOT NULL,
	[Included] [bit] NOT NULL DEFAULT ((0)),
	[ProposedHours] [decimal](11, 2) NOT NULL,
	[HistoricalHours] [decimal](11, 2) NOT NULL,
	[ResourceHours] [decimal](11, 2) NOT NULL,
	[BusinessResourceHours] [decimal](11, 2) NOT NULL,
	[BOESkillMix] [decimal](5, 2) NOT NULL,
	[LaborSkillMix] [decimal](5, 2) NOT NULL,
	[ResourceID] [varchar](20) NOT NULL,
	[BusinessResourceID] [varchar](20) NOT NULL,
	[BOEID] [int] NOT NULL,
	[BOETaskElementID] [int] NOT NULL,
	[IsUserInput] [bit] NOT NULL,
	[OrderID] [int] NOT NULL
)
GO