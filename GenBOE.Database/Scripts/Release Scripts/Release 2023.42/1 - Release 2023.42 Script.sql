EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.42';

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertMessageConfirmation]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertMessageConfirmation];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertMessageConfirmation]
(
	@ETIUserID int,
	@MessageID int
)
AS
/******************************************************************************
**		 
**		Name: insertMessageConfirmation
**		Desc: Upserts a record into the Message Confirmation Table
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 11/09/23
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**      12/13/2023  e374897             PROPH-1377 Upsert Message Confirmation
*******************************************************************************/
SET NOCOUNT ON 

DECLARE	@ErrorMessage varchar (500)

IF EXISTS	(SELECT 1 FROM [dbo].[MessageConfirmation] WHERE	
					 ETIUserId = @ETIUserID AND
				 	 MessageId = @MessageID
			)
			-- Update
			BEGIN
				UPDATE [dbo].[MessageConfirmation]
					SET
						[UpdateDT] = @UpdateDT
					WHERE
						ETIUserId = @ETIUserID AND
				 		MessageId = @MessageID
			END
ELSE
	BEGIN		
		-- Insert
		DECLARE @UpdateDT datetime2 = GETDATE()
			
		INSERT INTO [dbo].[MessageConfirmation]
				   ([ETIUserId]
				   ,[MessageId]
				   ,[UpdateDT])
			 VALUES
				   (
					@ETIUserID, 
					@MessageID, 
					@UpdateDT
					)
	END
GO
