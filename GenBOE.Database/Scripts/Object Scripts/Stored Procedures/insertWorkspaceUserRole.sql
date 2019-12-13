IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertWorkspaceUserRole]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertWorkspaceUserRole];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertWorkspaceUserRole]
(
	@ETIUserID int,
	@RoleID int,
	@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name: insertWorkspaceUserRole
**		Desc: Inserts a record into the Workspace User Role table for DTO
**			
**		
**
**		Auth: Don Canuso
**		Date: 11/16/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index	
**		6/1/11		dcanuso				Updating SP to use SQL Server Date
**		6/15/11		dcanuso				Request to remove Update Date from SP	
**		11/20/13	dcanuso				Permission Story - Remove ETI Group ID		
*******************************************************************************/
SET NOCOUNT ON 

DECLARE	@ErrorMessage varchar (500)

IF EXISTS	(SELECT 1 FROM [dbo].[WorkspaceUserRole] WHERE	
					 ETIUserID = @ETIUserID AND
					 /*ETIGroupID = @ETIGroupID AND*/
					 WorkspaceID = @WorkspaceID AND
				 	 RoleID = @RoleID
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
			
		INSERT INTO [dbo].[WorkspaceUserRole]
				   ([ETIUserID]
				/*   ,[ETIGroupID]*/
				   ,[RoleID]
				   ,[WorkspaceID]
				   ,[UpdateDT])
			 VALUES
				   (
					@ETIUserID, 
					/*@ETIGroupID, */
					@RoleID, 
					@WorkspaceID, 
					@UpdateDT
					)
	END
GO