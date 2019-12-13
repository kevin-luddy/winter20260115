IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceRolesByETIUserID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceRolesByETIUserID];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteWorkspaceRolesByETIUserID]
(
@ETIUserID int,
@WorkspaceID int,
@RoleID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteWorkspaceRolesByETIUserID]
**		Desc: Deletes a Workspace User Role from Workspace User Role
**			
**		   
**		
**
**		Auth: Don Canuso
**		Date: 12/14/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2/14/11		dcanuso				WI2036 - Removing ETIGroupID from BOEUserRole
**		5/12/11		DCANUSO				correcting logic for deleting a Workspace User Role
**		11/20/13	dcanuso				Permission Story - Remove ETI Group ID
******************************************************************************/
SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDT FROM dbo.WorkspaceUserRole WHERE 
			ETIUserID = @ETIUserID AND 
			/*ETIGroupID IS NULL AND */
			WorkspaceID = @WorkspaceID AND 
			RoleID = @RoleID) = @UpdateDT
	BEGIN
		/*
		Another BOE permission is in place within the Workspace
		If You are deleting a Workspace User with
		BOE Permissions
		You will no longer have access to BOE
		So prevent that from happening
		*/
		IF @RoleID = 7 /*Workspace User*/
		BEGIN
			/*First Check if there is a record in BUR*/
			IF EXISTS (SELECT 1 FROM dbo.BOEUserRole BUR 
							INNER JOIN dbo.BOE B ON BUR.BOEID = B.BOEID
						WHERE BUR.ETIUserID = @ETIUserID AND B.WorkspaceID = @WorkspaceID)				
				BEGIN
					/*MORE THAN ONE RECORD, THEN I CAN DELETE*/
					IF (SELECT COUNT(WorkspaceUserRoleID) 
						FROM dbo.WorkspaceUserRole WHERE ETIUserID = @ETIUserID AND WorkspaceID = @WorkspaceID) > 1
							BEGIN
								DELETE FROM dbo.WorkspaceUserRole
								WHERE 
									[ETIUserID] = @ETIUserID AND
									/*[ETIGroupID] IS NULL AND*/
									[WorkspaceID] = @WorkspaceID AND 
									[RoleID] = @RoleID
							END
					/*IF ONLY ONE - CAN NOT BE DELETED*/
					ELSE
							BEGIN
								SET @ErrorMessage =   'Last Workspace User Role Permission can not be deleted when BOE Permissions are in place'
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
						DELETE FROM dbo.WorkspaceUserRole
						WHERE 
							[ETIUserID] = @ETIUserID AND
							/*[ETIGroupID] IS NULL AND*/
							[WorkspaceID] = @WorkspaceID AND 
							[RoleID] = @RoleID
					END

			END
	ELSE /*Not Role 7 - So Delete the Role*/
			BEGIN
				DELETE FROM dbo.WorkspaceUserRole
				WHERE 
					[ETIUserID] = @ETIUserID AND
					/*[ETIGroupID] IS NULL AND*/
					[WorkspaceID] = @WorkspaceID AND 
					[RoleID] = @RoleID
			END
	END
ELSE
	BEGIN
	
				SET @ErrorMessage =   'The Workspace User Role has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
	END

GO