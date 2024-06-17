-- Note: common disclosure has FK to skill mix table and each resource (from skill mix table) can have multiple business resources

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CommonDisclosureSkillMix]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[CommonDisclosureSkillMix] (
		[CommonDisclosureSkillMixID] int IDENTITY(1,1) PRIMARY KEY CLUSTERED
		,[Rationale] varchar(255) NULL
		,[Included] bit DEFAULT 0
		,[ProposedHours] decimal(11, 2) NULL
		,[HistoricalHours] decimal(11, 2) NULL
		,[BOESkillMix] decimal(5, 2) NULL
		,[LaborSkillMix] decimal(5, 2) NULL
		,[ResourceID] [varchar](20) NULL
		,[BusinessResourceID] [varchar](20) NULL
		,[SkillMixID] int NULL
		,CONSTRAINT FK_CommonDisclosureSkillMix_SkillMixID FOREIGN KEY (SkillMixID) REFERENCES [dbo].[SkillMix] ([SkillMixID])
	)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[CommonDisclosureSkillMix]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[CommonDisclosureSkillMix]
	(
		[CommonDisclosureSkillMixID] int NULL
		,[Rationale] varchar(255) NULL
		,[Included] bit DEFAULT 0
		,[ProposedHours] decimal(11, 2) NULL
		,[HistoricalHours] decimal(11, 2) NULL
		,[BOESkillMix] decimal(5, 2) NULL
		,[LaborSkillMix] decimal(5, 2) NULL
		,[ResourceID] [varchar](20) NULL
		,[BusinessResourceID] [varchar](20) NULL
		,[SkillMixID] int NULL
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