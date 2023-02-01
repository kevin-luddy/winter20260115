EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.1';
GO

/*
    ## START ##

    12/12/2022 [twilson3] - IES-2010 Set AdditionalQueryFilters to be nullable
*/

ALTER TABLE dbo.MOQTypeSelectionTableData ALTER COLUMN AdditionalQueryFilters [varchar](2500) NULL
GO

/*
    12/12/2022 [twilson3] - IES-2010 Set AdditionalQueryFilters to be nullable

    ## END ##
*/

/*
    ## START ##

    1/12/2023 [e405721] - ACV-168 Set AdditionalQueryFilters to be nullable in version
*/

ALTER TABLE [version].MOQTypeSelectionTableData ALTER COLUMN AdditionalQueryFilters [varchar](2500) NULL
GO

/*
    1/12/2023 [e405721] - ACV-168 Set AdditionalQueryFilters to be nullable in version

    ## END ##
*/

/*
    ## START ##

    1/31/2023 [e405721] - ACV-221 "SAP Connection Enabled"
*/

IF COL_LENGTH('dbo.Workspace', 'EnableSAPConnection') IS NULL
BEGIN
	ALTER TABLE dbo.Workspace
		ADD EnableSAPConnection BIT DEFAULT 0 NOT NULL
END
GO

IF COL_LENGTH('version.Workspace', 'EnableSAPConnection') IS NULL
BEGIN
	ALTER TABLE [version].Workspace
		ADD EnableSAPConnection BIT DEFAULT 0 NOT NULL
END
GO

/*
    1/31/2023 [e405721] - ACV-221 "SAP Connection Enabled"

    ## END ##
*/
*/

/*
    ## START ##

    1/31/2023 [ranzalon] - ACV-209 RMS MOQ Table Source Field
*/

IF DB_NAME() like '%MST%' AND NOT EXISTS (SELECT 1 FROM [dbo].[MOQTypeSelectionTableData] WHERE RepositoryName is not null)
BEGIN
    UPDATE [dbo].[MOQTypeSelectionTableData]
    SET RepositoryName = 'User'
END
GO

/*
    1/31/2023 [ranzalon] - ACV-209 RMS MOQ Table Source Field

    ## END ##
*/