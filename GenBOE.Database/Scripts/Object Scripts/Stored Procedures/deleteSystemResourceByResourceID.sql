IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSystemResourceByResourceID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSystemResourceByResourceID];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteSystemResourceByResourceID]
(
@ResourceID int,
@UpdateDT datetime2
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
**		8/3/12		dcanuso				Discussion with Developer: 
**										Removing ResourceListID as it will always 
**										be greater than 1 and it is difficult
**										to get from code
**		3/25/13		dcanuso				Bug 16927 
**										Update the SPs for the "in use" rules for System Resource Rates 
**										(LRR) and System Admin Resources.
**										
**										"In Use" applys when:
**										It has a Labor Resource Rate associated with it 
**										AND the Resource is being used in a BOE
**										
**										It has a Labor Resource Rate associated with it 
**										AND a Workspace Resource Rate Mapped to it
**
**										Per SE: In Use rules are determined based off WS State not closed and complete
**										
**										Addional Notes:
**										If a System Resource + Rate is no longer being used in a BOE, 
**										the "in use" is removed from both
**										If a user attempts to delete the System Resource that has a Rate, 
**										a popup will be displayed asking the user to confirm the deletion of both.
**										See wireframe https://isgs-gen.external.lmco.com/sites/Estimating_Init/doclib14/Wireframes/Resource%20Rates.mht
**										
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
/*
In the redesign all that is done is a Delete Flag gets set

IF (SELECT IsNull(ResourceInUseFlag, -99) FROM [dbo].[Resource] WHERE ResourceID = @ResourceID ) = 1
	BEGIN
		SET @ErrorMessage =   'The Resource with ID ' + CAST(@ResourceID  AS varchar(10)) + ' is in use'
		RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,
					1 -- State,
				   )
		RETURN
	END
*/
IF (SELECT UpdateDT FROM [dbo].[Resource] WHERE ResourceID = @ResourceID) = @UpdateDT
		BEGIN
		

				UPDATE  [dbo].[Resource] 
				SET DeletedFlag = 1
				WHERE 
					ResourceID = @ResourceID AND
					ResourceListID = 1 


END

	ELSE
		BEGIN
			SET @ErrorMessage =   'The Resource with ID ' + CAST(@ResourceID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO