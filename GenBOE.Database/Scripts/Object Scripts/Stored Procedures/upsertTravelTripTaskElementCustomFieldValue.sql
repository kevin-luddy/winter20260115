IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertTravelTripTaskElementCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertTravelTripTaskElementCustomFieldValue];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertTravelTripTaskElementCustomFieldValue]
(
@TTECFVID [int],
@TravelTripTaskElementID [int],
@CustomFieldID [int],
@CustomFieldValueID [int],
@CustomFieldValue varchar(250),
@UpdateDT datetime2,
@IsOpenEnded bit
)
AS
/******************************************************************************
**		 
**		Name: upsertTravelTripTaskElementCustomFieldValue
**		Desc: Insert/Update Custom Field Value for the TravelTrip Task Element
**			
**		
**
**		Auth: Don Canuso
**		Date: 8/17/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		12/13/11	dcanuso				WI 6166
**		3/12/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
**		3/29/18		ranzalon			BOEJ-3268 - Fix copy workspace
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedTravelTripTaskElementCustomFieldValueXref AS Table (TTECFVID int)
DECLARE @InsertedTravelTripTaskElementCustomFieldValue AS Table (CustomFieldValueID int)

IF @TTECFVID  < 0  /*Insert Record*/
	BEGIN

	SET @UpdateDT = GETDATE()
	
	IF @IsOpenEnded = 0 /*Standard CF*/
		BEGIN
			INSERT INTO [dbo].[TravelTripTaskElementCustomFieldValueXREF]
				   ([TravelTripTaskElementID]
				   ,[CustomFieldValueID]
				   ,[UpdateDT])
			 OUTPUT inserted.TTECFVID INTO @InsertedTravelTripTaskElementCustomFieldValueXref
			 VALUES
				   (
				   @TravelTripTaskElementID,
				   @CustomFieldValueID,
				   @UpdateDT
				   )
           
			SELECT @TTECFVID = TTECFVID FROM @InsertedTravelTripTaskElementCustomFieldValueXref
	
			UPDATE dbo.CustomFieldValue
			SET CustomFieldValueInUseFlag = 1
			WHERE CustomFieldValueID = @CustomFieldValueID
		END
	ELSE /*Open Ended CF*/
		BEGIN
			--Only add custom field value to db if it's not already there
			IF @CustomFieldValueID < 0
				BEGIN
					INSERT INTO [dbo].[CustomFieldValue]
						([CustomFieldValueName]
						,[CustomFieldValueDescription]
						,[CustomFieldID]
						,[CustomFieldValueInUseFlag]
						,[UpdateDT])
					OUTPUT inserted.CustomFieldValueID INTO @InsertedTravelTripTaskElementCustomFieldValue
					VALUES
						(CONVERT(varchar(10), @CustomFieldID) + '-' + CONVERT(varchar(10), @TravelTripTaskElementID) --Name only used when performing copy, so it needs to be unique for mapping
						,@CustomFieldValue
						,@CustomFieldID
						,1
						,@UpdateDT)

					SELECT @CustomFieldValueID = CustomFieldValueID FROM @InsertedTravelTripTaskElementCustomFieldValue
				END
			ELSE --If the value is already there, make sure the in use flag is set
				BEGIN
					UPDATE [dbo].[CustomFieldValue]
					SET [CustomFieldValueInUseFlag] = 1
					WHERE [CustomFieldValueID] = @CustomFieldValueID
				END

			INSERT INTO [dbo].[TravelTripTaskElementCustomFieldValueXREF]
				   ([TravelTripTaskElementID]
				   ,[CustomFieldValueID]
				   ,[UpdateDT])
			 VALUES
				   (@TravelTripTaskElementID
				   ,@CustomFieldValueID
				   ,@UpdateDT)
		END
END

ELSE /*Update Record*/
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[TravelTripTaskElementCustomFieldValueXREF] WHERE TTECFVID = @TTECFVID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			
			IF @IsOpenEnded = 0 /*Standard CF*/
				BEGIN
					DECLARE @OriginalCustomFieldValueID int

					SELECT @OriginalCustomFieldValueID = CustomFieldValueID
					FROM dbo.TravelTripTaskElementCustomFieldValueXREF 
					WHERE TTECFVID = @TTECFVID
			
					UPDATE [dbo].[TravelTripTaskElementCustomFieldValueXREF]
						SET [CustomFieldValueID] = @CustomFieldValueID,
							[UpdateDT] = @UpdateDT
					WHERE TTECFVID = @TTECFVID


					IF @OriginalCustomFieldValueID <> @CustomFieldValueID
						BEGIN
						/*Only should be looking within the scope
						of the Custom Field, so if the scope is TravelTrip
						then only look in TravelTripCustomFieldValueXREF
				
						Should only need to set  the value to false when last one in use
						*/
				
						IF NOT EXISTS (SELECT [CustomFieldValueID] FROM [dbo].[TravelTripTaskElementCustomFieldValueXREF]
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
			ELSE /*Open Ended*/
				BEGIN
					UPDATE [dbo].[TravelTripTaskElementCustomFieldValueXREF]
					SET [UpdateDT] = @UpdateDT
					WHERE TTECFVID = @TTECFVID

					UPDATE [dbo].[CustomFieldValue]
					SET [CustomFieldValueDescription] = @CustomFieldValue
						,[UpdateDT] = @UpdateDT
					WHERE [CustomFieldValueID] = @CustomFieldValueID	
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
END

IF @@ERROR = 0
	SELECT @TTECFVID AS TTECFVID
GO