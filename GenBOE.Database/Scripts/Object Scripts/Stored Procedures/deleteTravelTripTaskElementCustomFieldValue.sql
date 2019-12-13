IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteTravelTripTaskElementCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteTravelTripTaskElementCustomFieldValue];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteTravelTripTaskElementCustomFieldValue]
(
@TTECFVID [int],
@TravelTripTaskElementID [int],
@CustomFieldValueID [int],
@UpdateDT datetime2,
@IsOpenEnded bit
)
AS
/******************************************************************************
**		 
**		Name: [deleteTravelTripTaskElementCustomFieldValue]
**		Desc: Deletes data from the dbo.TravelTripTaskElementCustomFieldValueXREF table which
**				holds the Custom Field Value for the TravelTrip Task Element
**			
**		
**
**		Auth: Don Canuso
**		Date: 8/17/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		12/13/11	dcanuso				WI6166
**		3/12/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
*******************************************************************************/
SET NOCOUNT ON 

IF (SELECT UpdateDT FROM [dbo].[TravelTripTaskElementCustomFieldValueXREF] WHERE TTECFVID = @TTECFVID) = @UpdateDT
	BEGIN		
		/*Variable For In Use Check*/

		DELETE FROM [dbo].[TravelTripTaskElementCustomFieldValueXREF] WHERE TTECFVID = @TTECFVID

		IF @IsOpenEnded = 0 /*Additional Standard CF work*/
			BEGIN
				/*Only should be looking within the scope
				of the Custom Field, so if the scope is TravelTrip
				then only look in TravelTripCustomFieldValueXREF
				*/
		
				IF NOT EXISTS (SELECT [CustomFieldValueID] FROM [dbo].[TravelTripTaskElementCustomFieldValueXREF]
								WHERE CustomFieldValueID = @CustomFieldValueID)
					BEGIN
				
						UPDATE dbo.CustomFieldValue
							SET CustomFieldValueInUseFlag = 0,
								UpdateDT = GETDATE()
						FROM dbo.CustomFieldValue CFV 
						WHERE CFV.CustomFieldValueID = @CustomFieldValueID
		
					END
			END
		ELSE /*Additional Open Ended work*/
			BEGIN
				DELETE FROM [dbo].[CustomFieldValue] WHERE CustomFieldValueID = @CustomFieldValueID
			END
	END
ELSE
	BEGIN
		DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =  'The Travel Trip Task Element Custom Field with ID ' + CAST(@TTECFVID AS varchar(10)) +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
		RETURN
	END
GO