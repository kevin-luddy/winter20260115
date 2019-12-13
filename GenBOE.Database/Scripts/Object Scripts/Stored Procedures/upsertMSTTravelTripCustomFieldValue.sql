IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertMSTTravelTripCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertMSTTravelTripCustomFieldValue];
GO

CREATE PROCEDURE [dbo].[upsertMSTTravelTripCustomFieldValue]
(
@MSTTCFVID [int],
@MSTTravelTripID [int],
@CustomFieldID [int],
@MSTCustomFieldValueID [int],
@CustomFieldValue varchar(250),
@UpdateDT datetime2,
@IsOpenEnded bit
)
AS
/******************************************************************************
**		 
**		Name: upsertMSTTravelTripCustomFieldValue
**		Desc: Insert/Update the Custom Field Value for the MSTTravelTrip
**			
**		
**
**		Auth: Tom Glick duped from upsertTravelTripCustomFieldValue
**		Date: 9/14/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		3/12/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
**		3/29/18		ranzalon			BOEJ-3268 - Fix copy workspace
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @InsertedMSTTravelTripCustomFieldValueXref AS Table (MSTTCFVID int)
DECLARE @InsertedMSTTravelTripCustomFieldValue AS Table (CustomFieldValueID int)

IF @MSTTCFVID  < 0  /*Insert Record*/
	BEGIN

	SET @UpdateDT = GETDATE()
	
	IF @IsOpenEnded = 0 /*Standard CF*/
		BEGIN
			INSERT INTO [dbo].[MSTTravelTripCustomFieldValueXREF]
				   ([MSTTravelTripID]
				   ,[MSTCustomFieldValueID]
				   ,[UpdateDT])
			 OUTPUT inserted.MSTTCFVID INTO @InsertedMSTTravelTripCustomFieldValueXref
			 VALUES
				   (
				   @MSTTravelTripID,
				   @MSTCustomFieldValueID,
				   @UpdateDT
				   )
           
			SELECT @MSTTCFVID = MSTTCFVID FROM @InsertedMSTTravelTripCustomFieldValueXref
	
			UPDATE dbo.CustomFieldValue
			SET CustomFieldValueInUseFlag = 1
			WHERE CustomFieldValueID = @MSTCustomFieldValueID
		END
	ELSE /*Open Ended CF*/
		BEGIN
			--Only add custom field value to db if it's not already there
			IF @MSTCustomFieldValueID < 0
				BEGIN
					INSERT INTO [dbo].[CustomFieldValue]
						([CustomFieldValueName]
						,[CustomFieldValueDescription]
						,[CustomFieldID]
						,[CustomFieldValueInUseFlag]
						,[UpdateDT])
					OUTPUT inserted.CustomFieldValueID INTO @InsertedMSTTravelTripCustomFieldValue
					VALUES
						(CONVERT(varchar(10), @CustomFieldID) + '-' + CONVERT(varchar(10), @MSTTravelTripID) --Name only used when performing copy, so it needs to be unique for mapping
						,@CustomFieldValue
						,@CustomFieldID
						,1
						,@UpdateDT)

					SELECT @MSTCustomFieldValueID = CustomFieldValueID FROM @InsertedMSTTravelTripCustomFieldValue
				END
			ELSE --If the value is already there, make sure the in use flag is set
				BEGIN
					UPDATE [dbo].[CustomFieldValue]
					SET [CustomFieldValueInUseFlag] = 1
					WHERE [CustomFieldValueID] = @MSTCustomFieldValueID
				END

			INSERT INTO [dbo].[MSTTravelTripCustomFieldValueXREF]
				   ([MSTTravelTripID]
				   ,[MSTCustomFieldValueID]
				   ,[UpdateDT])
			 VALUES
				   (@MSTTravelTripID
				   ,@MSTCustomFieldValueID
				   ,@UpdateDT)
		END
END

ELSE
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[MSTTravelTripCustomFieldValueXREF] WHERE MSTTCFVID = @MSTTCFVID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			
			IF @IsOpenEnded = 0 /*Standard CF*/
				BEGIN
					DECLARE @MSTOriginalCustomFieldValueID int

					SELECT @MSTOriginalCustomFieldValueID = MSTCustomFieldValueID
					FROM dbo.MSTTravelTripCustomFieldValueXREF 
					WHERE MSTTCFVID = @MSTTCFVID			
			
					UPDATE [dbo].[MSTTravelTripCustomFieldValueXREF]
						SET [MSTCustomFieldValueID] = @MSTCustomFieldValueID,
							[UpdateDT] = @UpdateDT
					WHERE MSTTCFVID = @MSTTCFVID

					IF @MSTOriginalCustomFieldValueID <> @MSTCustomFieldValueID
						BEGIN
						/*Only should be looking within the scope
						of the Custom Field, so if the scope is MSTTravelTrip
						then only look in MSTTravelTripCustomFieldValueXREF
				
						Only need to update if it is the last one - and then set to 0
						*/
							IF NOT EXISTS (SELECT [MSTCustomFieldValueID] FROM [dbo].[MSTTravelTripCustomFieldValueXREF] 
											WHERE MSTCustomFieldValueID = @MSTOriginalCustomFieldValueID)
								BEGIN
												
									UPDATE dbo.CustomFieldValue
										SET CustomFieldValueInUseFlag = 0,
										UpdateDT = GETDATE()
									FROM dbo.CustomFieldValue CFV 
									WHERE CFV.CustomFieldValueID = @MSTOriginalCustomFieldValueID
								END

							UPDATE dbo.CustomFieldValue
								SET CustomFieldValueInUseFlag = 1
							WHERE CustomFieldValueID = @MSTCustomFieldValueID

						END
				END
			ELSE /*Open Ended*/
				BEGIN
					UPDATE [dbo].[MSTTravelTripCustomFieldValueXREF]
					SET [UpdateDT] = @UpdateDT
					WHERE MSTTCFVID = @MSTTCFVID

					UPDATE [dbo].[CustomFieldValue]
					SET [CustomFieldValueDescription] = @CustomFieldValue
						,[UpdateDT] = @UpdateDT
					WHERE [CustomFieldValueID] = @MSTCustomFieldValueID	
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
END

IF @@ERROR = 0
	SELECT @MSTTCFVID AS MSTTCFVID


GO