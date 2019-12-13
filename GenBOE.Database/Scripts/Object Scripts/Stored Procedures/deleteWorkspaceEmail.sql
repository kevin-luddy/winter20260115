IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceEmail]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceEmail];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteWorkspaceEmail]
(
@WorkspaceEmailID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteWorkspaceEmail]
**		Desc: Delete Workspace Email (use System defaults)
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 3/29/2018
*******************************************************************************
**		Change History
*******************************************************************************
*******************************************************************************/
SET NOCOUNT ON 


IF (SELECT UpdateDT FROM [dbo].[WorkspaceEmailXREF] WHERE WorkspaceEmailID = @WorkspaceEmailID ) = @UpdateDT
	BEGIN
		
		DELETE FROM [dbo].[WorkspaceEmailXREF]
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

GO