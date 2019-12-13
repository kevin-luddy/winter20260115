IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspacePerformingOrganizationByPerformingOrganizationID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspacePerformingOrganizationByPerformingOrganizationID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteWorkspacePerformingOrganizationByPerformingOrganizationID]
(
@PerformingOrganizationID int,
@PerformingOrganizationListID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteWorkspacePerformingOrganizationByPerformingOrganizationID]
**		Desc: Delete PerformingOrganization based on its PerformingOrganizationID
**			
**
**		Auth: Don Canuso
**		Date: 2/8/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		3/3/2017	twilson3			BOEJ-1861 Remove DTS
******************************************************************************/

SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

/*DECLARE @InUseFlag int
DECLARE @temp TABLE (ReturnValue int NOT NULL)
INSERT @temp EXECUTE [dbo].[getWorkspacePerformingOrganizationInUseFlagByPerformingOrganizationID] @PerformingOrganizationID, @PerformingOrganizationListID
SELECT @InUseFlag = ReturnValue FROM @temp


IF @InUseFlag = 1
	BEGIN
		SET @ErrorMessage =   'The Performing Organization with ID ' + CAST(@PerformingOrganizationID  AS varchar(10)) + ' is in use'
		RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,
					1 -- State,
				   )
		RETURN
	END
*/
/*
/*
Determin if Workspace or System PerformingOrganization 
*/
DECLARE @PerformingOrganizationType int
/*0/Null = System/Otherwise Workspace*/
SELECT @PerformingOrganizationType = 	IsNull(PerformingOrganizationID, 0) 
FROM [dbo].[PerformingOrganization] 
WHERE PerformingOrganizationID = @PerformingOrganizationID AND 
PerformingOrganizationListID = @PerformingOrganizationListID
*/

DECLARE @WorkspaceID int
SELECT @WorkspaceID = WorkspaceID FROM dbo.Workspace
WHERE PerformingOrganizationListID = @PerformingOrganizationListID


/* A WS Admin added/changed a PerformingOrganization and
thus the flag is set for the one Workspace*/

UPDATE [dbo].[Workspace]
	SET PerformingOrganizationChangeFlag = 1
WHERE
	PerformingOrganizationListID = @PerformingOrganizationListID AND
	WorkspaceID = @WorkspaceID

			
DELETE FROM dbo.WorkspacePerformingOrganization 
WHERE SystemPerformingOrganizationID = @PerformingOrganizationID AND
WorkspaceID = @WorkspaceID


--IF @PerformingOrganizationType > 1
--BEGIN
	DELETE FROM [dbo].[PerformingOrganization] 
	WHERE 
	PerformingOrganizationID = @PerformingOrganizationID AND
	PerformingOrganizationListID = @PerformingOrganizationListID AND
	PerformingOrganizationListID <> 1 /*AND
						(
							PerformingOrganizationInUseFlag = 0 OR
							PerformingOrganizationInUseFlag IS NULL
						)*/
--END

GO