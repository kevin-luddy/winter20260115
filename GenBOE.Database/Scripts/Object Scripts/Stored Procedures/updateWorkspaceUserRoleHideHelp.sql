IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateWorkspaceUserRoleHideHelp]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateWorkspaceUserRoleHideHelp];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateWorkspaceUserRoleHideHelp]
(
@WorkspaceUserRoleID int,
@WorkspaceID int,
@HideHelp bit,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name:	[updateWorkspaceUserRoleHideHelp]
**		Desc:	Update Workspace Admin's setting for Hiding Help Section
**			
**		
**
**		Auth: Don Canuso
**		Date: 5/23/2011
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF EXISTS (SELECT 1 FROM dbo.WorkspaceUserRole
				WHERE	WorkspaceUserRoleID = @WorkspaceUserRoleID AND
						RoleID = 4 /*Workspace Administrator*/ AND
						WorkspaceID = @WorkspaceID
			)
	BEGIN
		IF (SELECT UpdateDT FROM dbo.WorkspaceUserRole WHERE	WorkspaceUserRoleID = @WorkspaceUserRoleID) = @UpdateDT 
			BEGIN
		
				SET @UpdateDT = GETDATE()
				
				UPDATE [dbo].[WorkspaceUserRole]
					SET [HideHelp] = @HideHelp,
						[UpdateDT] = @UpdateDT
				WHERE
					[WorkspaceUserRoleID] = @WorkspaceUserRoleID 

			END
		ELSE
				BEGIN

					SET @ErrorMessage =    'The Workspace User Role with ID ' + CAST(@WorkspaceUserRoleID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
							11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN

				END
	END
ELSE
	BEGIN
					SET @ErrorMessage =    'The Workspace User Role with ID ' + CAST(@WorkspaceUserRoleID  AS varchar(10)) + ' is not a Workspace Administrator'
					RAISERROR (
							@ErrorMessage, -- Message text.
							11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
	END
GO