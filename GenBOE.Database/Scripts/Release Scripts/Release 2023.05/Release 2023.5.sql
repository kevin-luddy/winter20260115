EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.5';
GO

/*
    ## START ##

   3/1/23		twilson3			ACV-343 Update MOQ Column sizes
*/

ALTER TABLE dbo.MOQTypeSelectionTableData ALTER COLUMN TotalRelevantHoursAfterQueryFilters [decimal](11,2) NOT NULL
GO

ALTER TABLE dbo.MOQTypeSelectionTableData ALTER COLUMN TotalWbsHours [decimal](11,2) NOT NULL
GO

ALTER TABLE [version].MOQTypeSelectionTableData ALTER COLUMN TotalRelevantHoursAfterQueryFilters [decimal](11,2) NOT NULL
GO

ALTER TABLE [version].MOQTypeSelectionTableData ALTER COLUMN TotalWbsHours [decimal](11,2) NOT NULL
GO


/*
    3/1/23		twilson3			ACV-343 Update MOQ Column sizes

    ## END ##
*/
