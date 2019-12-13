IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateWorkspaceStatus]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateWorkspaceStatus];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateWorkspaceStatus]
(
@WorkspaceID int,
@WorkspaceStateID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name:	[updateWorkspaceStatus]
**		Desc:	Update Workspace and sets the WorkspaceStateID (status)
**			
**
**		Auth: Don Canuso
**		Date: 8/25/2011
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDT FROM dbo.Workspace WHERE WorkspaceID = @WorkspaceID) = @UpdateDT
	BEGIN			
		UPDATE dbo.Workspace
			SET WorkspaceStateID = @WorkspaceStateID
		WHERE
			WorkspaceID = @WorkspaceID
	END
ELSE
	BEGIN
		SET @ErrorMessage =   'The Workspace with ID ' + CAST (@WorkspaceID AS VARCHAR (10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
	END
GO