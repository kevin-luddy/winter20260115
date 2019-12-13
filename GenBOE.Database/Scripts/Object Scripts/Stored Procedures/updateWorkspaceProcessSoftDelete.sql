IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateWorkspaceProcessSoftDelete]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateWorkspaceProcessSoftDelete];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateWorkspaceProcessSoftDelete]
(
@WorkspaceID int,
@UpdateDT datetime2,
@Delete bit
)
AS
/******************************************************************************
**		 
**		Name: [updateWorkspaceProcessSoftDelete]
**		Desc: Handles Soft Deletes
**			
**		
**
**		Auth: Don Canuso
**		Date: 10/10/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

/*
@Delete 
If True - Delete the Workspace (Soft Delete)
If False - Remove Soft Delete
*/


	IF (SELECT UpdateDT FROM [dbo].[Workspace] WHERE WorkspaceID = @WorkspaceID ) = @UpdateDT
		BEGIN
		
			UPDATE dbo.Workspace
				SET IsDeleted = @Delete,
					DateDeleted = 
									CASE	
										WHEN @Delete = 1 THEN GetDate()
										WHEN @Delete = 0 THEN NULL
									END
			WHERE
				WorkspaceID = @WorkspaceID

			SELECT @WorkspaceID AS WorkspaceID
	
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Workspace with ID ' + CAST(@WorkspaceID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO