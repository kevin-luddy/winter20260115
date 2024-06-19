-- Note: common disclosure has FK to skill mix table and each resource (from skill mix table) can have multiple business resources

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CommonDisclosureSkillMix]') AND type in (N'U'))
DROP TABLE [dbo].[CommonDisclosureSkillMix];
GO
BEGIN
	CREATE TABLE [dbo].[CommonDisclosureSkillMix] (
		[CommonDisclosureSkillMixID] int IDENTITY(1,1) PRIMARY KEY CLUSTERED
		,[Rationale] varchar(255) NOT NULL
		,[Included] bit DEFAULT 0 NOT NULL
		,[ProposedHours] decimal(11, 2) NOT NULL
		,[HistoricalHours] decimal(11, 2) NOT NULL
		,[BOESkillMix] decimal(5, 2) NOT NULL
		,[LaborSkillMix] decimal(5, 2) NOT NULL
		,[ResourceID] [varchar](20) NOT NULL
		,[BusinessResourceID] [varchar](20) NOT NULL
		,[SkillMixID] int NOT NULL
		,[BOEID] int NOT NULL
		,[BOETaskElementID] int NOT NULL
		,[MOQTypeSelectionID] int NOT NULL
		,CONSTRAINT FK_CommonDisclosureSkillMix_SkillMixID FOREIGN KEY (SkillMixID) REFERENCES [dbo].[SkillMix] ([SkillMixID])
		,CONSTRAINT FK_CommonDisclosureSkillMix_BOE FOREIGN KEY (BOEID) REFERENCES [dbo].[BOE] ([BOEID])
		,CONSTRAINT FK_CommonDisclosureSkillMix_BOETaskElement FOREIGN KEY(BOETaskElementID) REFERENCES [dbo].[BOETaskElement] ([BOETaskElementID])
		,CONSTRAINT FK_CommonDisclosureSkillMix_MOQTypeSelection FOREIGN KEY(MOQTypeSelectionID) REFERENCES [dbo].[MOQTypeSelection] ([MOQTypeSelectionId])
	)
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[CommonDisclosureSkillMix]') AND type in (N'U'))
DROP TABLE [version].[CommonDisclosureSkillMix];
GO
BEGIN
	CREATE TABLE [version].[CommonDisclosureSkillMix]
	(
		[CommonDisclosureSkillMixID] int NOT NULL
		,[Rationale] varchar(255) NOT NULL
		,[Included] bit DEFAULT 0 NOT NULL
		,[ProposedHours] decimal(11, 2) NOT NULL
		,[HistoricalHours] decimal(11, 2) NOT NULL
		,[BOESkillMix] decimal(5, 2) NOT NULL
		,[LaborSkillMix] decimal(5, 2) NOT NULL
		,[ResourceID] [varchar](20) NOT NULL
		,[BusinessResourceID] [varchar](20) NOT NULL
		,[SkillMixID] int NOT NULL
		,[BOEID] int NOT NULL
		,[BOETaskElementID] int NOT NULL
		,[MOQTypeSelectionID] int NOT NULL
		,[VersionID] int NOT NULL
	);
END
GO

/*
	## START ##
	06/16/2024 [e302876] - PROPH-2018 Common Disclosure Skill Mix DB Table
*/

-- 06/16/2024 [e302876] - PROPH-2018 Common Disclosure Skill Mix DB Table
-- Stored procs created:
	-- CommonDisclosureSkillMixViaTable.sql
	-- deleteCommonDisclosureSkillMix.sql
/*
   06/16/2024 [e302876] - PROPH-2018 Common Disclosure Skill Mix DB Table
   ## END ##
*/