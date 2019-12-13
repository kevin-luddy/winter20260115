IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMSTTravelTripCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMSTTravelTripCustomFieldValue];
GO

CREATE PROCEDURE [dbo].[deleteMSTTravelTripCustomFieldValue]
(
@MSTTCFVID [int],
@MSTTravelTripID [int],
@MSTCustomFieldValueID [int],
@UpdateDT datetime2,
@IsOpenEnded bit
)
AS
/******************************************************************************
**		 
**		Name: [deleteMSTTravelTripCustomFieldValue]
**		Desc: Deletes data from the dbo.MSTTravelTripCustomFieldValueXREF table which
**				holds the Custom Field Value for the MSTTravelTrip
**			
**		
**
**		Auth: Tom Glick (dupe of deleteTravelTripCustomFieldValue
**		Date: 9/14/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		12/13/11	dcanuso				WI 6166
**		3/8/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
*******************************************************************************/
SET NOCOUNT ON 

IF (SELECT UpdateDT FROM [dbo].[MSTTravelTripCustomFieldValueXREF] WHERE MSTTCFVID = @MSTTCFVID) = @UpdateDT
	BEGIN
		

		DELETE FROM [dbo].[MSTTravelTripCustomFieldValueXREF] WHERE  MSTTCFVID = @MSTTCFVID
		
		IF @IsOpenEnded = 0 /*Additional Standard CF work*/
			BEGIN
				/*
				Only should be looking within the scope
				of the Custom Field, so if the scope is MSTTravelTrip
				then only look in MSTTravelTripCustomFieldValueXREF
				*/

				IF NOT EXISTS (SELECT  MSTCustomFieldValueID FROM [dbo].[MSTTravelTripCustomFieldValueXREF] 
								WHERE  MSTCustomFieldValueID = @MSTCustomFieldValueID)
					BEGIN
						UPDATE dbo.CustomFieldValue
						SET CustomFieldValueInUseFlag = 0,
							UpdateDT = GETDATE()
						FROM dbo.CustomFieldValue CFV 
						WHERE CFV.CustomFieldValueID = @MSTCustomFieldValueID
					END
			END
		ELSE /*Additional Open Ended work*/
			BEGIN
				DELETE FROM [dbo].[CustomFieldValue] WHERE CustomFieldValueID = @MSTCustomFieldValueID
			END
	END
ELSE
	BEGIN
		DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =  'The Travel Trip Custom Field with ID ' + CAST(@MSTTCFVID AS varchar(10)) +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
		RETURN
	END

GO

