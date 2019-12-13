IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteTravelTripCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteTravelTripCustomFieldValue];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteTravelTripCustomFieldValue]
(
@TCFVID [int],
@TravelTripID [int],
@CustomFieldValueID [int],
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteTravelTripCustomFieldValue]
**		Desc: Deletes data from the dbo.TravelTripCustomFieldValueXREF table which
**				holds the Custom Field Value for the TravelTrip
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
**		12/13/11	dcanuso				WI 6166
*******************************************************************************/
SET NOCOUNT ON 

IF (SELECT UpdateDT FROM [dbo].[TravelTripCustomFieldValueXREF] WHERE TCFVID = @TCFVID) = @UpdateDT
	BEGIN
		

		DELETE FROM [dbo].[TravelTripCustomFieldValueXREF] WHERE TCFVID = @TCFVID
		

		/*
		Only should be looking within the scope
		of the Custom Field, so if the scope is TravelTrip
		then only look in TravelTripCustomFieldValueXREF
		*/

		IF NOT EXISTS (SELECT CustomFieldValueID FROM [dbo].[TravelTripCustomFieldValueXREF] 
						WHERE CustomFieldValueID = @CustomFieldValueID)
			BEGIN
				UPDATE dbo.CustomFieldValue
				SET CustomFieldValueInUseFlag = 0,
					UpdateDT = GETDATE()
				FROM dbo.CustomFieldValue CFV 
				WHERE CFV.CustomFieldValueID = @CustomFieldValueID
			END


	END
ELSE
	BEGIN
		DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =  'The Travel Trip Custom Field with ID ' + CAST(@TCFVID AS varchar(10)) +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
		RETURN
	END

GO