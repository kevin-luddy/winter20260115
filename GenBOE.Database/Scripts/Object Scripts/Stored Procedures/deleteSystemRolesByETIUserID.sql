IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSystemRolesByETIUserID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSystemRolesByETIUserID];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteSystemRolesByETIUserID]
(
@ETIUserID int,
@RoleID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteSystemRolesByETIUserID]
**		Desc: Deletes a System User Role from System User Role
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
**		11/20/13	dcanuso				Permission Story - Remove ETI Group ID
******************************************************************************/
SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDT FROM dbo.SystemUserRole WHERE 
			ETIUserID = @ETIUserID AND 
			/*ETIGroupID IS NULL AND */
			RoleID = @RoleID) = @UpdateDT
	BEGIN

	
		DELETE FROM dbo.SystemUserRole
		WHERE 
			[ETIUserID] = @ETIUserID AND
			/*[ETIGroupID] IS NULL AND*/
			[RoleID] = @RoleID
		
				
	END
ELSE
	BEGIN
	
				SET @ErrorMessage =   'The System User Role has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
	END

GO