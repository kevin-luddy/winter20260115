EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2022.x.x';
GO

/*
    ## START ##

    04/14/2022 [Quijano] - IES-1014 PBOE DB Changes - Add new columns, modify existing to allow nulls
*/

IF COL_LENGTH ('dbo.BOEFormPBOE', 'SupplierCCoPD') IS NULL
BEGIN
    ALTER TABLE dbo.BOEFormPBOE
        ADD SupplierCCoPD INT NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'SourceSelectionDescription') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD SourceSelectionDescription VARCHAR(MAX) NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'CommercialityDescription') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD CommercialityDescription VARCHAR(MAX) NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'TechnicalEvaluationDescription') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD TechnicalEvaluationDescription VARCHAR(MAX) NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'PriceAnalysisDescription') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD PriceAnalysisDescription VARCHAR(MAX) NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'CostAnalysisDescription') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD CostAnalysisDescription VARCHAR(MAX) NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'RationaleValueSummary') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD RationaleValueSummary VARCHAR(MAX) NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'GovtPricingReceived') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD GovtPricingReceived INT NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'GovtPricingReceivedDate') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD GovtPricingReceivedDate DATE NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'GovtPricingReceivedText') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD GovtPricingReceivedText VARCHAR(30) NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'CostAnalysisUnqual') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD CostAnalysisUnqual INT NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'CostAnalysisUnqualDate') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD CostAnalysisUnqualDate DATE NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'CostAnalysisUnqualText') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD CostAnalysisUnqualText VARCHAR(30) NULL
END
GO

/* make removed fields nullable */
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
    04/22/2022 [Quijano] - IES-1014 PBOE DB Changes - Add new columns

    ## END ##
*/

/*
    ## START ##

    04/22/2022 [Quijano] - IES-1019 PBOE DB Changes - Add new columns to backup schema
*/

IF COL_LENGTH ('version.BOEFormPBOE', 'SupplierCCoPD') IS NULL
BEGIN
    ALTER TABLE version.BOEFormPBOE
        ADD SupplierCCoPD INT NULL
END
GO

IF COL_LENGTH ('version.BOEFormPBOE', 'SourceSelectionDescription') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD SourceSelectionDescription VARCHAR(MAX) NULL
END
GO

IF COL_LENGTH ('version.BOEFormPBOE', 'CommercialityDescription') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD CommercialityDescription VARCHAR(MAX) NULL
END
GO

IF COL_LENGTH ('version.BOEFormPBOE', 'TechnicalEvaluationDescription') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD TechnicalEvaluationDescription VARCHAR(MAX) NULL
END
GO

IF COL_LENGTH ('version.BOEFormPBOE', 'PriceAnalysisDescription') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD PriceAnalysisDescription VARCHAR(MAX) NULL
END
GO

IF COL_LENGTH ('version.BOEFormPBOE', 'CostAnalysisDescription') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD CostAnalysisDescription VARCHAR(MAX) NULL
END
GO

IF COL_LENGTH ('version.BOEFormPBOE', 'RationaleValueSummary') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD RationaleValueSummary VARCHAR(MAX) NULL
END
GO

IF COL_LENGTH ('version.BOEFormPBOE', 'GovtPricingReceived') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD GovtPricingReceived INT NULL
END
GO

IF COL_LENGTH ('version.BOEFormPBOE', 'GovtPricingReceivedDate') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD GovtPricingReceivedDate DATE NULL
END
GO

IF COL_LENGTH ('version.BOEFormPBOE', 'GovtPricingReceivedText') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD GovtPricingReceivedText VARCHAR(30) NULL
END
GO

IF COL_LENGTH ('version.BOEFormPBOE', 'CostAnalysisUnqual') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD CostAnalysisUnqual INT NULL
END
GO

IF COL_LENGTH ('version.BOEFormPBOE', 'CostAnalysisUnqualDate') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD CostAnalysisUnqualDate DATE NULL
END
GO

IF COL_LENGTH ('version.BOEFormPBOE', 'CostAnalysisUnqualText') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD CostAnalysisUnqualText VARCHAR(30) NULL
END
GO

/* make removed fields nullable */
IF COL_LENGTH('version.BOEFormPBOE', 'DegreeOfCompetition') IS NOT NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ALTER COLUMN DegreeOfCompetition INT NULL
END
GO

IF COL_LENGTH('version.BOEFormPBOE', 'SupplierProposalSupportingDataIncluded') IS NOT NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ALTER COLUMN SupplierProposalSupportingDataIncluded INT NULL
END
GO

IF COL_LENGTH('version.BOEFormPBOE', 'PriceAnalysisIncluded') IS NOT NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ALTER COLUMN PriceAnalysisIncluded INT NULL
END
GO

IF COL_LENGTH('version.BOEFormPBOE', 'CommercialItemDocIncluded') IS NOT NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ALTER COLUMN CommercialItemDocIncluded INT NULL
END
GO

IF COL_LENGTH('version.BOEFormPBOE', 'CostAnalysisIncluded') IS NOT NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ALTER COLUMN CostAnalysisIncluded INT NULL
END
GO

IF COL_LENGTH('version.BOEFormPBOE', 'SowWritten') IS NOT NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ALTER COLUMN SowWritten INT NULL
END
GO

IF COL_LENGTH('version.BOEFormPBOE', 'GovtReview') IS NOT NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ALTER COLUMN GovtReview INT NULL
END
GO

IF COL_LENGTH('version.BOEFormPBOE', 'Procurement') IS NOT NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ALTER COLUMN Procurement INT NULL
END
GO

/*
    04/22/2022 [Quijano] - IES-1019 PBOE DB Changes - Add new columns to backup schema

    ## END ##
*/

/*
    ## START ##

    05/02/2022 [Quijano] - IES-1126 Add VendorId and SupplierProposedValue for PBOE
*/
IF COL_LENGTH('version.BOEFormPBOE', 'VendorId') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD VendorId VARCHAR(20) NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'VendorId') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD VendorId VARCHAR(20) NULL
END
GO

IF COL_LENGTH('version.BOEFormPBOE', 'SupplierProposedValue') IS NULL
BEGIN
	ALTER TABLE version.BOEFormPBOE
		ADD SupplierProposedValue DECIMAL(13,2) NULL
END
GO

IF COL_LENGTH ('dbo.BOEFormPBOE', 'SupplierProposedValue') IS NULL
BEGIN
	ALTER TABLE dbo.BOEFormPBOE
		ADD SupplierProposedValue DECIMAL(13,2) NULL
END
GO

/*
    ## END ##

    05/02/2022 [Quijano] - IES-1126 Add VendorId and SupplierProposedValue for PBOE
*/