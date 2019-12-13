IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBOETaskElementCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBOETaskElementCustomFieldValue];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertBOETaskElementCustomFieldValue]
(
@BTECFVID [int],
@BOETaskElementID [int],
@CustomFieldID [int],
@CustomFieldValueID [int],
@CustomFieldValue varchar(250),
@UpdateDT datetime2,
@IsOpenEnded bit
)
AS
/******************************************************************************
**		 
**		Name: upsertBOETaskElementCustomFieldValue
**		Desc: Insert/Update Custom Field Value for the BOE Task Element
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/14/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		7/20/11		DCANUSO				WI 4215 - Custom Fields are scope specific
**		3/8/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
**		3/29/18		ranzalon			BOEJ-3268 - Fix copy workspace
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedBOETaskElementCustomFieldValueXref AS Table (BTECFVID int)
DECLARE @InsertedBOETaskElementCustomFieldValue AS TABLE (CustomFieldValueID int)

IF @BTECFVID  < 0  /*Insert Record*/
	BEGIN

	SET @UpdateDT = GETDATE()
	IF @IsOpenEnded = 0 /*Standard CF*/
		BEGIN
			INSERT INTO [dbo].[BOETaskElementCustomFieldValueXREF]
				   ([BOETaskElementID]
				   ,[CustomFieldValueID]
				   ,[UpdateDT])
			 OUTPUT inserted.BTECFVID INTO @InsertedBOETaskElementCustomFieldValueXref
			 VALUES
				   (
				   @BOETaskElementID,
				   @CustomFieldValueID,
				   @UpdateDT
				   )
           
			SELECT @BTECFVID = BTECFVID FROM @InsertedBOETaskElementCustomFieldValueXref
	
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
					OUTPUT inserted.CustomFieldValueID INTO @InsertedBOETaskElementCustomFieldValue
					VALUES
						(CONVERT(varchar(10), @CustomFieldID) + '-' + CONVERT(varchar(10), @BOETaskElementID) --Name only used when performing copy, so it needs to be unique for mapping
						,@CustomFieldValue
						,@CustomFieldID
						,1
						,@UpdateDT)

					SELECT @CustomFieldValueID = CustomFieldValueID FROM @InsertedBOETaskElementCustomFieldValue
				END
			ELSE --If the value is already there, make sure the in use flag is set
				BEGIN
					UPDATE [dbo].[CustomFieldValue]
					SET [CustomFieldValueInUseFlag] = 1
					WHERE [CustomFieldValueID] = @CustomFieldValueID
				END

			INSERT INTO [dbo].[BOETaskElementCustomFieldValueXREF]
				   ([BOETaskElementID]
				   ,[CustomFieldValueID]
				   ,[UpdateDT])
			 VALUES
				   (@BOETaskElementID
				   ,@CustomFieldValueID
				   ,@UpdateDT)
		END
END

ELSE /*Update Record*/
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[BOETaskElementCustomFieldValueXREF] WHERE BTECFVID = @BTECFVID) = @UpdateDT
			BEGIN
				SET @UpdateDT = GetDate()
			
				IF @IsOpenEnded = 0 /*Standard CF*/
					BEGIN
						DECLARE @OriginalCustomFieldValueID int

						SELECT @OriginalCustomFieldValueID = CustomFieldValueID
						FROM dbo.BOETaskElementCustomFieldValueXREF 
						WHERE BTECFVID = @BTECFVID
			
						UPDATE [dbo].[BOETaskElementCustomFieldValueXREF]
							SET [CustomFieldValueID] = @CustomFieldValueID,
								[UpdateDT] = @UpdateDT
						WHERE BTECFVID = @BTECFVID


						IF @OriginalCustomFieldValueID <> @CustomFieldValueID
							BEGIN
								/*Only should be looking within the scope
								of the Custom Field, so if the scope is BOE
								then only look in BOECustomFieldValueXREF
								*/
				
								UPDATE dbo.CustomFieldValue
									SET CustomFieldValueInUseFlag = CASE 
																		WHEN X.[CustomFieldValueID] IS NULL THEN 0
																		WHEN X.[CustomFieldValueID] IS NOT NULL THEN 1
																	END,
										UpdateDT = GETDATE()
								FROM dbo.CustomFieldValue CFV 
									LEFT OUTER JOIN [dbo].[BOETaskElementCustomFieldValueXREF] X ON
										CFV.[CustomFieldValueID] = X.[CustomFieldValueID]
								WHERE CFV.CustomFieldValueID = @OriginalCustomFieldValueID


								UPDATE dbo.CustomFieldValue
									SET CustomFieldValueInUseFlag = 1
								WHERE CustomFieldValueID = @CustomFieldValueID
							END
						END
					ELSE /*Open Ended*/
						BEGIN
							UPDATE [dbo].[BOETaskElementCustomFieldValueXREF]
							SET [UpdateDT] = @UpdateDT
							WHERE BTECFVID = @BTECFVID

							UPDATE [dbo].[CustomFieldValue]
							SET [CustomFieldValueDescription] = @CustomFieldValue
								,[UpdateDT] = @UpdateDT
							WHERE [CustomFieldValueID] = @CustomFieldValueID	
						END		
			END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =  'The Task Element Custom Field with ID ' + CAST(@BTECFVID AS varchar(10)) +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
END

IF @@ERROR = 0
	SELECT @BTECFVID AS BTECFVID
GO