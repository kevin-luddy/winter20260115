EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.8';
GO

/*
                ## START ##

                10/30/2025 twilson3 - PROPH-3424 - New Cage Codes
*/

IF NOT EXISTS(SELECT 1 FROM CageCodes where CageCode = '149N6')
BEGIN
                INSERT INTO CageCodes (CageCode, Address1, Address2, City, [State], Zip)
                VALUES ('149N6', '7765 Old Telegraph Road', NULL, 'Severn' ,'MD' ,'21144-1148'), 
                        ('151N4', '14530 McCormick Dr', NULL, 'Tampa' ,'FL' ,'33626-3022');
               
END
GO
/*
                ## END ##

               10/30/2025 twilson3 - PROPH-3424 - New Cage Codes
*/

/******************************************************************************
** This only serves the purpose of allowing the full release script to be generated
** with required changes for Stored Procedures and Views
** 10/30/25		e403038				PROPH-3406 Label Modifications => Current Planned renamed to ScheduledActual and Current Scheduled renamed to Planned

*/
