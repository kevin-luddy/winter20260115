EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.16';
GO

-- 10/30/2025 ranzalon, PROPH-3422 - Skill Mix Summary Table column name updates
IF COL_LENGTH('dbo.SkillMixSummary', 'ProposedHours') IS NOT NULL
BEGIN

	EXEC sp_rename 'dbo.SkillMixSummary.ProposedHours', 'ProposedLegacyResource', 'COLUMN';
	EXEC sp_rename 'dbo.SkillMixSummary.BusinessResourceHours', 'ProposedBrc', 'COLUMN';
	EXEC sp_rename 'dbo.SkillMixSummary.BOESkillMix', 'ProposedSkillMix', 'COLUMN';
	EXEC sp_rename 'dbo.SkillMixSummary.LaborSkillMix', 'HistoricalSkillMix', 'COLUMN';

END

IF COL_LENGTH('version.SkillMixSummary', 'ProposedHours') IS NOT NULL
BEGIN

	EXEC sp_rename 'version.SkillMixSummary.ProposedHours', 'ProposedLegacyResource', 'COLUMN';
	EXEC sp_rename 'version.SkillMixSummary.BusinessResourceHours', 'ProposedBrc', 'COLUMN';
	EXEC sp_rename 'version.SkillMixSummary.BOESkillMix', 'ProposedSkillMix', 'COLUMN';
	EXEC sp_rename 'version.SkillMixSummary.LaborSkillMix', 'HistoricalSkillMix', 'COLUMN';

END