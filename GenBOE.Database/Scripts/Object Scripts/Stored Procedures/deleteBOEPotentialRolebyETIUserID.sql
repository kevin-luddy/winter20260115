IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBOEPotentialRolebyETIUserID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBOEPotentialRolebyETIUserID];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteBOEPotentialRolebyETIUserID]
(
@ETIUserID int,
@WorkspaceID int,
@RoleID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteWorkspacePermissionbyETIUserID] RENAMED to deleteBOEPotentialRolebyETIUserID
**		Desc: Deletes a Workspace Permission from dbo.BOEPotentialRole (Workspace Permission table renamed) and linkages
**			
**		   
**		
**
**		Auth: Don Canuso
**		Date: August 2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		11/3/10		dcanuso				Developer noticed WorkspaceID not being 
										in DELETE statement.  Corrected.
										PER SE  via Developer:
										If a user has permissions with and without 
										an associated group, when you delete the 
										permissions based on an ETI User ID, 
										the permissions associated with a group 
										are not removed.
**		11/9/10		dcanuso				Removal of [WBS_CLIN_BOE_XREF] from database
**										Columns Added to BOE table
**		11/30/10	dcanuso				Developer Request: ADD @RoleID
**		12/14/10	dcanuso				New/Updated Rules:
										If no other BOE permissions are in place, 
											delete the workspace user role
										Can not delete a boe permission if the same
											permission is being used
										For Workspace Reviewer, Check if in use
											prior to deletion 
										Stored procedure should only be used to
											delete potential roles (Approver,Author)
**		1/17/11		dcanuso				Workspace Permission table renamed to BOE Potential Role
**		2/14/11		dcanuso				WI2036 - Removing ETIGroupID from BOEUserRole
**		2/14/11		dcanuso				Renaming SP from Workspace Permissions to BOEPotentialRole
**		2/21/11		dcanuso				WI Bug 2126:  Cannot remove assigned user's permissions 
**										When user is assigned as author/approver in a BOE as group and 
**										individual roles, cannot remove their individual permissions 
**										even though they are also present in a group. 
**										
**										SP does a check if the BOE role is being used elsewhere which is fine
**										if the user has more than one role in the BOE potential role
**										 (so both a group and invididual role) allow the individual role 
**										delete to take place still. 
**										A delete would fail only if this was the last BOE potential role 
**										and BOE permission was in place. 
**										
**										So, in conclusion: if there is more than one
**										record in the BOE Potential Role table,
**										allow the delete to occur
**		5/11/11		dcanuso				Correcting logic
**		11/20/13	dcanuso				Permission Story - Remove ETI Group ID
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDT FROM dbo.BOEPotentialRole /*Workspace Permission table renamed*/ WHERE 
			ETIUserID = @ETIUserID AND 
			/*ETIGroupID IS NULL AND */
			WorkspaceID = @WorkspaceID AND 
			RoleID = @RoleID) = @UpdateDT
	BEGIN
	
		IF (SELECT count(*) FROM BOEPotentialRole WHERE 
			ETIUserID = @ETIUserID AND
			WorkspaceID = @WorkspaceID AND 
			RoleID = @RoleID) = 1
				BEGIN
				

					/*Check to see if the BOE role is being used elsewhere*/
					/*Another BOE permission is in place within the Workspace*/
					IF EXISTS (SELECT 1 FROM dbo.BOEUserRole BUR 
									INNER JOIN dbo.BOE B ON BUR.BOEID = B.BOEID
								WHERE 
									BUR.ETIUserID = @ETIUserID AND
									B.WorkspaceID = @WorkspaceID AND 
									BUR.RoleID = @RoleID)				
						BEGIN
								SET @ErrorMessage =   'BOE Permissions are in place'
								RAISERROR (
								@ErrorMessage, -- Message text.
								11, -- Severity,/*Severity Changed to 11*/
								1 -- State,
								)
								RETURN
						END

						
						

					/* At this point, the role is not being used for a BOE so OK to delete from Permission Table*/	
					DELETE FROM [dbo].[BOEPotentialRole] /*Workspace Permission table renamed*/
					WHERE 
						[ETIUserID] = @ETIUserID AND
						/*[ETIGroupID] IS NULL AND*/
						[RoleID] = @RoleID AND
						[WorkspaceID] = @WorkspaceID 
						
						
						
					/*
					Now check to see if the User has any remaining permission
					If yes, do nothing
					If No, delete the Workspace User  Role
					*/
					IF NOT EXISTS 
						(
							SELECT 1 
								FROM dbo.BOEUserRole BUR 
									INNER JOIN dbo.BOE B ON BUR.BOEID = B.BOEID
								WHERE 
									BUR.[ETIUserID] = @ETIUserID AND 
									B.[WorkspaceID] = @WorkspaceID
							UNION
							SELECT 1 
								FROM dbo.WorkspaceUserRole 
								WHERE 
									[ETIUserID] = @ETIUserID AND 
									/*[ETIGroupID] IS NULL AND */
									[WorkspaceID] = @WorkspaceID
							UNION
							SELECT 1 
								FROM dbo.BOEPotentialRole /*Workspace Permission table renamed*/
								WHERE 
									[ETIUserID] = @ETIUserID AND 
								/*	[ETIGroupID] IS NULL AND */
									[WorkspaceID] = @WorkspaceID
							)						
						BEGIN
							DELETE FROM dbo.WorkspaceUserRole
							WHERE 
								[ETIUserID] = @ETIUserID AND
								/*[ETIGroupID] IS NULL AND*/
								[WorkspaceID] = @WorkspaceID AND 
								[RoleID] = 7 /*Workspace User Role*/
						END
				END						
				 
	ELSE
		/*More than one record exists so move forward and delete*/
		BEGIN
			DELETE FROM [dbo].[BOEPotentialRole] /*Workspace Permission table renamed*/
			WHERE 
				[ETIUserID] = @ETIUserID AND
				/*[ETIGroupID] IS NULL AND*/
				[RoleID] = @RoleID AND
				[WorkspaceID] = @WorkspaceID 
				
		END
			
END
					
ELSE
	BEGIN
				/*Workspace Permission table renamed*/
				SET @ErrorMessage =   'The BOE Potential Role has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
	END

GO