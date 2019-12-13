IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspaceEmail]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspaceEmail];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertWorkspaceEmail]
(
@WorkspaceEmailID int,
@EmailID int,
@WorkspaceID int,
@TurnOn bit,
@UpdateDT datetime2(7)
)
AS
/******************************************************************************
**		 
**		Name: [upsertWorkspaceEmail]
**		Desc: Insert/Update data for Workspace Emails
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 3/29/2018
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

IF @WorkspaceEmailID < 0  /*Insert Record*/
	BEGIN

		DECLARE	@InsertedWorkspaceEmail AS Table (WorkspaceEmailID int)

		SET @UpdateDT = GetDate()

		INSERT INTO [dbo].[WorkspaceEmailXREF]
				   ([EmailID]
				   ,[WorkspaceID]
				   ,[TurnOn]
				   ,[UpdateDT])          
			 OUTPUT inserted.WorkspaceEmailID INTO @InsertedWorkspaceEmail
			 VALUES
				   (@EmailID
				   ,@WorkspaceID
				   ,@TurnOn
				   ,@UpdateDT)


			SELECT @WorkspaceEmailID = WorkspaceEmailID FROM @InsertedWorkspaceEmail
	
	END
ELSE
	BEGIN
			IF (SELECT UpdateDT FROM [dbo].[WorkspaceEmailXREF] WHERE WorkspaceEmailID = @WorkspaceEmailID) = @UpdateDT
			BEGIN	
			
			SET @UpdateDT = GetDate()
			
			UPDATE [dbo].[WorkspaceEmailXREF]
			   SET [TurnOn] = @TurnOn
				  ,[UpdateDT] = @UpdateDT
			WHERE 
				WorkspaceEmailID = @WorkspaceEmailID
				
		END
			ELSE
			BEGIN
				DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Workspace Email with ID ' + @WorkspaceEmailID + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
			
	END

IF @@ERROR = 0
	SELECT @WorkspaceEmailID AS WorkspaceEmailID
GO