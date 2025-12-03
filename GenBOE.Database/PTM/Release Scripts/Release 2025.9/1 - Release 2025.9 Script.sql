EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.9';
GO

-- Author: RJ Anzalone (ranzalon)
-- PROPH-3420 - 11/4/2025
-- Alternative Pricing Methodology fields
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AlternativePricingMethodology]') AND type in (N'U'))
BEGIN
	CREATE TABLE dbo.AlternativePricingMethodology (
		Id INT PRIMARY KEY,
		Text VARCHAR(100) NOT NULL
	);

	INSERT INTO AlternativePricingMethodology VALUES
		(1, 'FAR 52-215-20 Alternate 1'),
		(2, 'NDAA Section 890/TINA Lite'),
		(3, 'Price Based Negotiations'),
		(4, 'Other Exception');
END

IF COL_LENGTH('dbo.Proposal', 'SubjectToAlternativePricingMethodology') IS NULL
BEGIN
	ALTER TABLE dbo.Proposal
	ADD SubjectToAlternativePricingMethodology bit NULL,
		AlternativePricingMethodology int NULL REFERENCES dbo.AlternativePricingMethodology (Id),
		AlternativePricingMethodologyOtherText varchar(50) NULL;
END