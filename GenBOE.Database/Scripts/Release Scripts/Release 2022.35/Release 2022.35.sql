EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2022.35';
GO

/*
    ## START ##

    10/27/2022 [ranzalon] - IES-1951 Increase WBS/WBS Element Character Count
*/

IF((SELECT COL_LENGTH('dbo.MOQTypeSelectionTableData', 'WbsElement') AS 'varchar') = 5000)
   BEGIN
     ALTER TABLE dbo.MOQTypeSelectionTableData
         ALTER COLUMN WbsElement VARCHAR(8000)
   END

/*
    10/27/2022 [ranzalon] - IES-1951 Increase WBS/WBS Element Character Count

    ## END ##
*/