EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.09';
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SkillMix]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[SkillMix] (
		[SkillMixID] int IDENTITY(1,1) PRIMARY KEY CLUSTERED
		,[Rationale] varchar(255) NULL
		,[Included] bit DEFAULT 0
		,[ProposedHours] decimal(11, 2) NULL
		,[HistoricalHours] decimal(11, 2) NULL
		,[BOESkillMix] decimal(5, 2) NULL
		,[LaborSkillMix] decimal(5, 2) NULL
		,[ResourceOld] [varchar](max) NULL
		,[ResourceNew] [varchar](max) NULL
		,[BOEID] int NULL
		,[BOETaskElementID] int NULL
		,[MOQTypeSelectionID] int NULL
		,CONSTRAINT FK_SkillMix_BOE FOREIGN KEY (BOEID) REFERENCES [dbo].[BOE] ([BOEID])
		,CONSTRAINT FK_SkillMix_BOETaskElement FOREIGN KEY(BOETaskElementID) REFERENCES [dbo].[BOETaskElement] ([BOETaskElementID])
		,CONSTRAINT FK_SkillMix_MOQTypeSelection FOREIGN KEY(MOQTypeSelectionID) REFERENCES [dbo].[MOQTypeSelection] ([MOQTypeSelectionId])
	)
END
GO
