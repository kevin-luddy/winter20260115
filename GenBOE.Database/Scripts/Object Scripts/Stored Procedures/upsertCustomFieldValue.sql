IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertCustomFieldValue];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertCustomFieldValue]
(
@CustomFieldValueID [int],
@CustomFieldValueName [varchar](20),
@CustomFieldValueDescription [varchar](250),
@CustomFieldID [int],
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [upsertCustomFieldValue]
**		Desc: Insert/Update data into Custom Field Value table 
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/3/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		9/28/11		dcanuso				CustomFieldValueDescription changed to 50
**		1/26/17		ranzalon			Allow Description update for in-use CF
**		2/2/17		Dusan				Moved the update date per RJs comments
**		7/8/18		Dusan				BOEJ-3670: Increased field size of CF Description to 250. This was missed before.
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedCustomFieldValue AS Table (CustomFieldValueID int)


IF @CustomFieldValueID  < 0  /*Insert Record*/
	BEGIN
	SET @UpdateDT = GETDATE()
	
	INSERT INTO [dbo].[CustomFieldValue]
           ([CustomFieldValueName]
           ,[CustomFieldValueDescription]
           ,[CustomFieldID]
           ,[CustomFieldValueInUseFlag]
           ,[UpdateDT])
     OUTPUT inserted.CustomFieldValueID INTO @InsertedCustomFieldValue
     VALUES
           (@CustomFieldValueName
           ,@CustomFieldValueDescription
           ,@CustomFieldID
           ,0 /* If you are adding a new value, it can't be in use yet - CustomFieldValueInUseFlag*/
           ,@UpdateDT) 
            
	SELECT @CustomFieldValueID = CustomFieldValueID FROM @InsertedCustomFieldValue
END

ELSE
	BEGIN
		IF (SELECT UpdateDT FROM [dbo].[CustomFieldValue] WHERE CustomFieldValueID = @CustomFieldValueID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GetDate()
			
			-- Update Name and UpdateDT only if not in use
			UPDATE [dbo].[CustomFieldValue]
			   SET [CustomFieldValueName] = @CustomFieldValueName
			 WHERE 
				CustomFieldValueID = @CustomFieldValueID AND 
				CustomFieldValueInUseFlag = 0
				
			-- Update Description regardless of being in use
			UPDATE [dbo].[CustomFieldValue]
			   SET [CustomFieldValueDescription] = @CustomFieldValueDescription
					,[UpdateDT] = @UpdateDT
			 WHERE CustomFieldValueID = @CustomFieldValueID		

		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =  'The Custom Field with Name ' + @CustomFieldValueName +  ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
END

IF @@ERROR = 0
	SELECT @CustomFieldValueID AS CustomFieldValueID

GO