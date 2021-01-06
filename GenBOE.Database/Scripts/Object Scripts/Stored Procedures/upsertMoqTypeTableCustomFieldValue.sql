IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertMoqTypeTableCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertMoqTypeTableCustomFieldValue];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertMoqTypeTableCustomFieldValue]
(
	@Id INT,
	@MoqTypeTableDataId INT,
	@CustomFieldId INT,
	@CustomFieldValueId INT,
	@CustomFieldValue VARCHAR(250),
	@UpdateDT DATETIME2(7),
	@IsOpenEnded BIT
)
AS
/******************************************************************************
**		Name: [upsertMoqTypeTableCustomFieldValue]
**		Desc: Insert/Update the Custom Field Value for the MoqTypeTableCustomFieldValueXREF table 
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2020/12/8	Dusan				Initial Release 
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedCustomFieldValueXref AS Table (Id int)
DECLARE @InsertedCustomFieldValue AS TABLE (CustomFieldValueId int)

IF @Id < 0  /*Insert Record*/
	BEGIN
		SET @UpdateDT = GETDATE()
		IF @IsOpenEnded = 0 /*Standard CF*/
			BEGIN
				INSERT INTO [dbo].[MoqTypeTableCustomFieldValueXREF]
					OUTPUT inserted.Id INTO @InsertedCustomFieldValueXref
					VALUES (@UpdateDT, @MoqTypeTableDataId, @CustomFieldValueId)
           
				SELECT @Id = Id FROM @InsertedCustomFieldValueXref
	
				UPDATE dbo.CustomFieldValue SET CustomFieldValueInUseFlag = 1 WHERE CustomFieldValueId = @CustomFieldValueId
			END
		ELSE /*Open Ended CF*/
			BEGIN
				--Only add custom field value to db if it's not already there
				IF @CustomFieldValueID < 0
					BEGIN
						INSERT INTO [dbo].[CustomFieldValue]
							([CustomFieldValueName]
							,[CustomFieldValueDescription]
							,[CustomFieldId]
							,[CustomFieldValueInUseFlag]
							,[UpdateDT])
						OUTPUT inserted.CustomFieldValueID INTO @InsertedCustomFieldValue
						VALUES
							(CONVERT(VARCHAR(10), @CustomFieldID) + '-' + CONVERT(VARCHAR(10), @MoqTypeTableDataId) --Name only used when performing copy, so it needs to be unique for mapping
							, @CustomFieldValue, @CustomFieldID, 1, @UpdateDT)

						SELECT @Id = CustomFieldValueId FROM @InsertedCustomFieldValue
					END
				ELSE --If the value is already there, make sure the in use flag is set
					BEGIN
						UPDATE [dbo].[CustomFieldValue] SET [CustomFieldValueInUseFlag] = 1 WHERE [CustomFieldValueId] = @CustomFieldValueId
					END

				INSERT INTO [dbo].[MoqTypeTableCustomFieldValueXREF]
					VALUES (@UpdateDT, @MoqTypeTableDataId, @CustomFieldValueId)
			END
	END
ELSE
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[MoqTypeTableCustomFieldValueXREF] WHERE Id = @Id) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			
			IF @IsOpenEnded = 0 /*Standard CF*/
				BEGIN
					DECLARE @OriginalCustomFieldValueId int

					SELECT @OriginalCustomFieldValueId = CustomFieldValueID
						FROM dbo.MoqTypeTableCustomFieldValueXREF 
						WHERE Id = @Id
			
			
					UPDATE [dbo].[MoqTypeTableCustomFieldValueXREF] SET [CustomFieldValueID] = @CustomFieldValueId, [UpdateDT] = @UpdateDT WHERE Id = @Id

					IF @OriginalCustomFieldValueId <> @CustomFieldValueId
						BEGIN
							UPDATE dbo.CustomFieldValue
								SET CustomFieldValueInUseFlag = CASE 
																	WHEN X.[CustomFieldValueID] IS NULL THEN 0
																	WHEN X.[CustomFieldValueID] IS NOT NULL THEN 1
																END,
									UpdateDT = GETDATE()
							FROM dbo.CustomFieldValue CFV 
								LEFT OUTER JOIN [dbo].[MoqTypeTableCustomFieldValueXREF] X ON
									CFV.[CustomFieldValueID] = X.[CustomFieldValueID]
							WHERE CFV.CustomFieldValueID = @OriginalCustomFieldValueId


							UPDATE dbo.CustomFieldValue SET CustomFieldValueInUseFlag = 1 WHERE CustomFieldValueID = @CustomFieldValueID
						END
				END
			ELSE
				BEGIN
					UPDATE [dbo].[MoqTypeTableCustomFieldValueXREF] SET [UpdateDT] = @UpdateDT WHERE Id = @Id
					UPDATE [dbo].[CustomFieldValue] SET [CustomFieldValueDescription] = @CustomFieldValue,[UpdateDT] = @UpdateDT WHERE [CustomFieldValueID] = @CustomFieldValueID
				END
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500) = 'The Moq Type Table Data Custom Field with Id ' + CAST(@Id AS varchar(10)) +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (@ErrorMessage, 11, 1)
			RETURN
		END
END

IF @@ERROR = 0
	SELECT @Id AS Id
GO