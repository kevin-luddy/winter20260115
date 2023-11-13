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
**		Desc: Inserts a record into the Message Confirmation Table
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
*******************************************************************************/
SET NOCOUNT ON 

DECLARE	@ErrorMessage varchar (500)

IF EXISTS	(SELECT 1 FROM [dbo].[MessageConfirmation] WHERE	
					 ETIUserId = @ETIUserID AND
				 	 MessageId = @MessageID
			)
			BEGIN
				SET @ErrorMessage =   'There is already a record in the database.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
ELSE
	BEGIN		
		
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