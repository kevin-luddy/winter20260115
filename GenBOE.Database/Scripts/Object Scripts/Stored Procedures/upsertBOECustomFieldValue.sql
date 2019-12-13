IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBOECustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBOECustomFieldValue];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertBOECustomFieldValue]
(
@BCFVID [int],
@BOEID [int],
@CustomFieldID [int],
@CustomFieldValueID [int],
@CustomFieldValue varchar(250),
@UpdateDT datetime2,
@IsOpenEnded bit
)
AS
/******************************************************************************
**		 
**		Name: upsertBOECustomFieldValue
**		Desc: Insert/Update the Custom Field Value for the BOE
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/14/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		7/20/11		DCANUSO				WI 4215 - Custom Fields are scope specific
**		3/8/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
**		3/29/18		ranzalon			BOEJ-3268 - Fix copy workspace
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedBOECustomFieldValueXref AS Table (BCFVID int)
DECLARE @InsertedBOECustomFieldValue AS TABLE (CustomFieldValueID int)

IF @BCFVID  < 0  /*Insert Record*/
	BEGIN

	SET @UpdateDT = GETDATE()

	IF @IsOpenEnded = 0 /*Standard CF*/
		BEGIN
			INSERT INTO [dbo].[BOECustomFieldValueXREF]
				   ([BOEID]
				   ,[CustomFieldValueID]
				   ,[UpdateDT])
			 OUTPUT inserted.BCFVID INTO @InsertedBOECustomFieldValueXref
			 VALUES
				   (
				   @BOEID,
				   @CustomFieldValueID,
				   @UpdateDT
				   )
           
			SELECT @BCFVID = BCFVID FROM @InsertedBOECustomFieldValueXref
	
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
					OUTPUT inserted.CustomFieldValueID INTO @InsertedBOECustomFieldValue
					VALUES
						(CONVERT(varchar(10), @CustomFieldID) + '-' + CONVERT(varchar(10), @BOEID) --Name only used when performing copy, so it needs to be unique for mapping
						,@CustomFieldValue
						,@CustomFieldID
						,1
						,@UpdateDT)

					SELECT @CustomFieldValueID = CustomFieldValueID FROM @InsertedBOECustomFieldValue
				END
			ELSE --If the value is already there, make sure the in use flag is set
				BEGIN
					UPDATE [dbo].[CustomFieldValue]
					SET [CustomFieldValueInUseFlag] = 1
					WHERE [CustomFieldValueID] = @CustomFieldValueID
				END
	
			INSERT INTO [dbo].[BOECustomFieldValueXREF]
				   ([BOEID]
				   ,[CustomFieldValueID]
				   ,[UpdateDT])
			 VALUES
				   (@BOEID
				   ,@CustomFieldValueID
				   ,@UpdateDT)
		END
END

ELSE
	BEGIN		
		IF (SELECT UpdateDT FROM [dbo].[BOECustomFieldValueXREF] WHERE BCFVID = @BCFVID) = @UpdateDT
			BEGIN
				SET @UpdateDT = GetDate()

				IF @IsOpenEnded = 0 /*Standard CF*/
					BEGIN
						DECLARE @OriginalCustomFieldValueID int

						SELECT @OriginalCustomFieldValueID = CustomFieldValueID
						FROM dbo.BOECustomFieldValueXREF 
						WHERE BCFVID = @BCFVID
			
			
						UPDATE [dbo].[BOECustomFieldValueXREF]
							SET [CustomFieldValueID] = @CustomFieldValueID,
								[UpdateDT] = @UpdateDT
						WHERE BCFVID = @BCFVID

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
								LEFT OUTER JOIN [dbo].[BOECustomFieldValueXREF] X ON
									CFV.[CustomFieldValueID] = X.[CustomFieldValueID]
							WHERE CFV.CustomFieldValueID = @OriginalCustomFieldValueID


							UPDATE dbo.CustomFieldValue
								SET CustomFieldValueInUseFlag = 1
							WHERE CustomFieldValueID = @CustomFieldValueID

							END		
					END
				ELSE /*Open Ended*/
					BEGIN			
						UPDATE [dbo].[BOECustomFieldValueXREF]
						SET [UpdateDT] = @UpdateDT
						WHERE BCFVID = @BCFVID

						UPDATE [dbo].[CustomFieldValue]
						SET [CustomFieldValueDescription] = @CustomFieldValue
							,[UpdateDT] = @UpdateDT
						WHERE [CustomFieldValueID] = @CustomFieldValueID	
					END
				END
			ELSE
				BEGIN
					DECLARE @ErrorMessage varchar (500)
						SET @ErrorMessage =  'The BOE Custom Field with ID ' + CAST(@BCFVID AS varchar(10)) +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
							11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

IF @@ERROR = 0
	SELECT @BCFVID AS BCFVID
GO