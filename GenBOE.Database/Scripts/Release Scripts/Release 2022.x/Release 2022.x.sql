EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2022.x.x';
GO

/*
    ## START ##

    04/14/2022 [Quijano] - IES-1014 PBOE DB Changes - Add new columns
*/

IF COL_LENGTH ('dbo.BOEFormPBOE', 'SupplierCCoPD') IS NULL
BEGIN
    ALTER TABLE dbo.BOEFormPBOE
        ADD SupplierCCoPD INT NULL
END
GO

IF COL_LENGTH('dbo.BOEFormPBOE', 'DegreeOfCompetition') IS NOT NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ALTER COLUMN DegreeOfCompetition INT NULL
END
GO

IF COL_LENGTH('dbo.BOEFormPBOE', 'SupplierProposalSupportingDataIncluded') IS NOT NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ALTER COLUMN SupplierProposalSupportingDataIncluded INT NULL
END
GO

IF COL_LENGTH('dbo.BOEFormPBOE', 'PriceAnalysisIncluded') IS NOT NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ALTER COLUMN PriceAnalysisIncluded INT NULL
END
GO

IF COL_LENGTH('dbo.BOEFormPBOE', 'CommercialItemDocIncluded') IS NOT NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ALTER COLUMN CommercialItemDocIncluded INT NULL
END
GO

IF COL_LENGTH('dbo.BOEFormPBOE', 'CostAnalysisIncluded') IS NOT NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ALTER COLUMN CostAnalysisIncluded INT NULL
END
GO

IF COL_LENGTH('dbo.BOEFormPBOE', 'SowWritten') IS NOT NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ALTER COLUMN SowWritten INT NULL
END
GO

IF COL_LENGTH('dbo.BOEFormPBOE', 'GovtReview') IS NOT NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ALTER COLUMN GovtReview INT NULL
END
GO

IF COL_LENGTH('dbo.BOEFormPBOE', 'Procurement') IS NOT NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ALTER COLUMN Procurement INT NULL
END
GO

/*
    04/14/2022 [Quijano] - IES-1014 PBOE DB Changes - Add new columns

    ## END ##
*/