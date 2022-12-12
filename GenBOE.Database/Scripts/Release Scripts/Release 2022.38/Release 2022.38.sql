EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2022.38';
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