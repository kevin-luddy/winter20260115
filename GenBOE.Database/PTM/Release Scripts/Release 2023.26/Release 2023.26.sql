EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.26';
GO

/*
                ## START ##

                7/19/23 twilson3 - PROPH-947 - New Cage Code
*/

IF NOT EXISTS(SELECT 1 FROM CageCodes where Address1 = 'LMGI')
BEGIN
                INSERT INTO CageCodes (CageCode, Address1, Address2, City, "State", Zip)
                VALUES ('2Y210', 'LMGI', '6801 Rockledge Dr', 'Bethesda' ,'MD' ,'20817-1803');
END
GO
/*
                ## END ##

               7/19/23 twilson3 - PROPH-947 - New Cage Code
*/
