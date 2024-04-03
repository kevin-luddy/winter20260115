EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.3';
GO

IF COL_LENGTH ('dbo.MOQTypeSelection', 'HistoricalReferenceExplanation') IS NULL
BEGIN
	ALTER TABLE [dbo].[MOQTypeSelection]
	ADD [HistoricalReferenceExplanation] VARCHAR(MAX) NULL;
	
	ALTER TABLE [version].[MOQTypeSelection]
	ADD [HistoricalReferenceExplanation] VARCHAR(MAX) NULL;
END
GO