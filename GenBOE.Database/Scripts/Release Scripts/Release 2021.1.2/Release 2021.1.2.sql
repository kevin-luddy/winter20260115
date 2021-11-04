EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2021.1.2';
GO

/*
    ## START ##

    11/3/2021 [Asuncion] - IES-444 MOQ Type Table - WBS/WBS Element Character Limit Increase
*/

IF COL_LENGTH ('dbo.MOQTypeSelectionTableData', 'WbsElement') IS NOT NULL
BEGIN
    ALTER TABLE dbo.MOQTypeSelectionTableData
    ALTER COLUMN WbsElement varchar(5000) NOT NULL
END
GO

/*
    11/3/2021 [Asuncion] - IES-444 MOQ Type Table - WBS/WBS Element Character Limit Increase

    ## END ##
*/