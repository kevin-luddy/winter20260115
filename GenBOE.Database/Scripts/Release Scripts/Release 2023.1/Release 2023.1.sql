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