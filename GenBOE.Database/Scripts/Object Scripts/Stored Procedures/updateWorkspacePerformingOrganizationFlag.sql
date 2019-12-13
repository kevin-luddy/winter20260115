IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateWorkspacePerformingOrganizationFlag]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateWorkspacePerformingOrganizationFlag];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateWorkspacePerformingOrganizationFlag]
(
@WorkspaceID int,
@PerformingOrganizationFlag bit,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: updateWorkspacePerformingOrganizationFlag
**		Desc: Sets the PerformingOrganizationChangeFlag  in the Workspace Table
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/14/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

IF (SELECT UpdateDT FROM [dbo].[Workspace] WHERE WorkspaceID = @WorkspaceID) = @UpdateDT
	BEGIN 
		SET @UpdateDT = GETDATE()
			
		UPDATE [dbo].[Workspace]
			SET [PerformingOrganizationChangeFlag] = @PerformingOrganizationFlag,
				[UpdateDT] = @UpdateDT
		WHERE WorkspaceID = @WorkspaceID
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

IF @@ERROR = 0
	SELECT @WorkspaceID as WorkspaceID
GO