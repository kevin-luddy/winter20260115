IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceResourceByResourceID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceResourceByResourceID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteWorkspaceResourceByResourceID]
(
@ResourceID int,
@ResourceListID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteResourceByResourceID]
**		Desc: Delete Resource based on its ResourceID
**			
**
**		Auth: Don Canuso
**		Date: 1/24/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2/3/11		dcanuso				Adding addition WHERE to only allow
**										deletions of non-InUse Resource
**		3/23/11		dcanuso				Added Flag Processing:
**										Add: Workspace.ResourceChangeFlag
**										IF SA Changes a Global Resource – Set All Workspace.ResourceChangeFlag = 1
**										IF WS Admin changes a Workspace Resource, set Workspace.ResourceChangeFlag = 1  
**										for specific workspace
**										Added Resource In Use Flag			
**										Added Error if editing In Use row
**		11/30/11	dcanuso				See Note Below
**		1/18/12		dcanuso				Updates to WRR/LRR/Locking
**		2/7/12		dcanuso				Additional InUse Changes
**		7/24/12		dcanuso				WI8960
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		2/5/13		dcanuso				WI 14477
**		5/4/2017	brunworg			BOEJ-2125 Add T&M Resource Rates
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
******************************************************************************/


/*
11/30/11 Note from Wireframe:
a.       In the System Administrator Default Resources table, 
the Resource that has a rate in the Manage Labor Resource Rates table 
that is not “In use”, will have a checkbox where it is able to be deleted.
1.       Wireframe 28 – When a Resource is selected in the 
System Administrator Default Resources table, 
the Delete button is clicked, and that Resource has a rate in the 
Manage Labor Resource Rates table, the following dialog will appear:  “The selected resource has a rate assigned in the Manage Labor Resource Rates table.  Are you sure you want to delete this resource and its corresponding resource rate?
1.       If the user clicks “Yes” the Resource is deleted from the System Administrator Default Resources table and the corresponding rate within the Manage Labor Resource Rates table will be deleted as well.
b.      In the System Administrator Default Resources table, the Resource that currently has an “In use” rate in the Manage Labor Resource Rates table, will become “In use” and cannot be deleted until the resource rate is deleted from the Manage Labor Resource Rates grid.
1.       While “In use” the Resource cannot be deleted through the UI or through the Import function.


Workspace Administrator Resources page - When a resource rate is added in the Manage Subcontractor, IWTA, and LMSI Rates grid, the resource that is now associated with a rate becomes linked to the Workspace Administrator Resources table.

a.       In the the Workspace Administrator Resources table, the Resource that has a rate in the Manage Subcontractor, IWTA, and LMSI Rates table that is not “In use”, will have a checkbox where it is able to be deleted.

                                                         i.            Wireframe 29 – When a Resource is selected in the Workspace Administrator Resources table, the Delete button is clicked, and that Resource has a rate in the Manage Subcontractor, IWTA, and LMSI Rates table, the following dialog will appear:  “The selected resource has a rate assigned in the Manage Labor Resource Rates table.  Are you sure you want to delete this resource and its corresponding resource rate?

1.       If the user clicks “Yes” the Resource is deleted from the Workspace Administrator Resources table and the corresponding rate within the Manage Subcontractor, IWTA, and LMSI Rates table will be deleted as well.

b.      In the Workspace Administrator Resources table, the Resource that currently has an “In use” rate in the Manage Subcontractor, IWTA, and LMSI Rates table, will become “In use” and cannot be deleted until the resource rate is deleted from the Manage Subcontractor, IWTA, and LMSI Rates grid.

                                                         i.            While “In use” the Resource cannot be deleted through the UI or through the Import function.


*/


SET NOCOUNT ON 

DECLARE @ErrorMessage varchar (500)

DECLARE @InUseFlag int
DECLARE @temp TABLE (ReturnValue int NOT NULL)
INSERT @temp EXECUTE [dbo].[getWorkspaceResourceInUseFlagByResourceID] @ResourceID, @ResourceListID
SELECT @InUseFlag = ReturnValue FROM @temp


IF @InUseFlag = 1
	BEGIN
		SET @ErrorMessage =   'The Resource with ID ' + CAST(@ResourceID  AS varchar(10)) + ' is in use'
		RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,
					1 -- State,
				   )
		RETURN
	END


/*
Determin if Workspace or System Resource 
*/
DECLARE @ResourceType int
/*0/Null = System/Otherwise Workspace*/
SELECT @ResourceType = 	IsNull(ResourceID, 0) 
FROM [dbo].[Resource] 
WHERE ResourceID = @ResourceID AND 
ResourceListID = @ResourceListID


DECLARE @WorkspaceID int
SELECT @WorkspaceID = WorkspaceID FROM dbo.Workspace
WHERE ResourceListID = @ResourceListID


/* A WS Admin added/changed a Resource and
thus the flag is set for the one Workspace*/
/*
UPDATE [dbo].[Workspace]
	SET ResourceChangeFlag = 1
WHERE
	ResourceListID = @ResourceListID AND
	WorkspaceID = @WorkspaceID
*/
			
DELETE FROM dbo.TMResourceRate
WHERE
TMResourceID = @ResourceID AND
WorkspaceID = @WorkspaceID

DELETE FROM dbo.WorkspaceResource 
WHERE SystemResourceID = @ResourceID AND
WorkspaceID = @WorkspaceID


/*EXECUTE dbo.updateResourceInUseFlagByResourceID @ResourceID*/


IF @ResourceType > 1
BEGIN
	DELETE FROM [dbo].[Resource] 
	WHERE 
	ResourceID = @ResourceID AND
	ResourceListID = @ResourceListID AND
	ResourceListID <> 1 /*AND
						(
							ResourceInUseFlag = 0 OR
							ResourceInUseFlag IS NULL
						)*/
					
END

GO