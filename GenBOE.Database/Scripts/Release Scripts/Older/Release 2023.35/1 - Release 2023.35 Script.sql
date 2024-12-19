EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.35';

GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MessageConfirmation]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[MessageConfirmation] (
		[ID]        INT           IDENTITY (1, 1) NOT NULL,
		[ETIUserId] INT           NOT NULL,
		[MessageId] INT           NOT NULL,
		[UpdateDT]  DATETIME2 (7) NOT NULL,
		CONSTRAINT [PK_MessageConfirmation] PRIMARY KEY CLUSTERED ([ID] ASC),
		CONSTRAINT [FK_MessageConfirmation_UserId] FOREIGN KEY ([ETIUserId]) REFERENCES [dbo].[ETIuser] ([ETIUserID])
	);
END
GO

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