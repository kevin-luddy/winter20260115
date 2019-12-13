IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertTravelTripCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertTravelTripCustomFieldValue];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertTravelTripCustomFieldValue]
(
@TCFVID [int],
@TravelTripID [int],
@CustomFieldValueID [int],
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: upsertTravelTripCustomFieldValue
**		Desc: Insert/Update the Custom Field Value for the TravelTrip
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

DECLARE @InsertedTravelTripCustomFieldValue AS Table (TCFVID int)



IF @TCFVID  < 0  /*Insert Record*/
	BEGIN

	SET @UpdateDT = GETDATE()
	
	INSERT INTO [dbo].[TravelTripCustomFieldValueXREF]
           ([TravelTripID]
           ,[CustomFieldValueID]
           ,[UpdateDT])
     OUTPUT inserted.TCFVID INTO @InsertedTravelTripCustomFieldValue
     VALUES
           (
           @TravelTripID,
           @CustomFieldValueID,
           @UpdateDT
           )
           
	SELECT @TCFVID = TCFVID FROM @InsertedTravelTripCustomFieldValue
	
	UPDATE dbo.CustomFieldValue
	SET CustomFieldValueInUseFlag = 1
	WHERE CustomFieldValueID = @CustomFieldValueID
END

ELSE
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[TravelTripCustomFieldValueXREF] WHERE TCFVID = @TCFVID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			
			DECLARE @OriginalCustomFieldValueID int

			SELECT @OriginalCustomFieldValueID = CustomFieldValueID
			FROM dbo.TravelTripCustomFieldValueXREF 
			WHERE TCFVID = @TCFVID
			
			
			UPDATE [dbo].[TravelTripCustomFieldValueXREF]
				SET [CustomFieldValueID] = @CustomFieldValueID,
					[UpdateDT] = @UpdateDT
			WHERE TCFVID = @TCFVID

			IF @OriginalCustomFieldValueID <> @CustomFieldValueID
				BEGIN
				/*Only should be looking within the scope
				of the Custom Field, so if the scope is TravelTrip
				then only look in TravelTripCustomFieldValueXREF
				
				Only need to update if it is the last one - and then set to 0
				*/
				
				
				IF NOT EXISTS (SELECT [CustomFieldValueID] FROM [dbo].[TravelTripCustomFieldValueXREF] 
								WHERE CustomFieldValueID = @OriginalCustomFieldValueID)
				BEGIN
												
					UPDATE dbo.CustomFieldValue
						SET CustomFieldValueInUseFlag = 0,
						UpdateDT = GETDATE()
					FROM dbo.CustomFieldValue CFV 
					WHERE CFV.CustomFieldValueID = @OriginalCustomFieldValueID
				END

				UPDATE dbo.CustomFieldValue
					SET CustomFieldValueInUseFlag = 1
				WHERE CustomFieldValueID = @CustomFieldValueID

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
END

IF @@ERROR = 0
	SELECT @TCFVID AS TCFVID
GO