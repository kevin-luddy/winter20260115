IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertCustomField]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertCustomField];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertCustomField]
(
@CustomFieldID [int],
@CustomFieldName [varchar](20),
@CustomFieldRequired [bit],
@IsOpenEnded [bit],
@CustomFieldDisplayID int,
@WorkspaceID [int],
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [upsertCustomField]
**		Desc: Insert/Update data into Custom Field table 
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/3/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		5/12/11		dcanuso				Removed:
**										@BOEDisplay [bit]
**										@TaskDisplay [bit]
**										@LaborTypeDisplay [bit]
**										
**										Replaced with:
**										@CustomFieldDisplayID
**		3/6/18		ranzalon			BOEJ-3079 - Open Ended Custom Fields
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedCustomField AS Table (CustomFieldID int)


IF @CustomFieldID  < 0  /*Insert Record*/
	BEGIN
	SET @UpdateDT = GETDATE()
	INSERT INTO [dbo].[CustomField]
           ([CustomFieldName]
           ,[CustomFieldRequired]
		   ,[IsOpenEnded]
           ,[CustomFieldDisplayID]
           ,[WorkspaceID]
           ,[UpdateDT])
     OUTPUT inserted.CustomFieldID INTO @InsertedCustomField
     VALUES
           (
			@CustomFieldName,
            IsNull(@CustomFieldRequired, 0), 
			IsNull(@IsOpenEnded, 0),
            @CustomFieldDisplayID,
            @WorkspaceID,
            @UpdateDT
            ) 
            
	SELECT @CustomFieldID = CustomFieldID FROM @InsertedCustomField
END

ELSE
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[CustomField] WHERE CustomFieldID = @CustomFieldID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			
			UPDATE [dbo].[CustomField]
			   SET [CustomFieldName] = @CustomFieldName
				  ,[CustomFieldRequired] = IsNull(@CustomFieldRequired, 0)
				  ,[IsOpenEnded] = IsNull(@IsOpenEnded, 0)
				  ,[CustomFieldDisplayID] = @CustomFieldDisplayID
				  ,[UpdateDT] = @UpdateDT
			 WHERE CustomFieldID = @CustomFieldID

		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =  'The Custom Field with Name ' + @CustomFieldName +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
END

IF @@ERROR = 0
	SELECT @CustomFieldID AS CustomFieldID
GO