IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deletePerformingOrganizationByPerformingOrganizationListID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deletePerformingOrganizationByPerformingOrganizationListID];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deletePerformingOrganizationByPerformingOrganizationListID]
(
@PerformingOrganizationListID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deletePerformingOrganizationByPerformingOrganizationListID]
**		Desc: Delete PerformingOrganizations assigned to a PerformingOrganizationListID
**				Update Date is from Performing Organization			
**
**		Auth: Don Canuso
**		Date: 1/24/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2/3/11		dcanuso				Adding addition WHERE to only allow
**										deletions of non-InUse PO
**		2/11/11		dcanuso				Added Flag Processing:
**										Add: Workspace.PerformingOrganizationChangeFlag
**										IF SA Changes a Global PerformingOrganization – Set All Workspace.PerformingOrganizationChangeFlag = 1
**										IF WS Admin changes a Workspace PerformingOrganization, set Workspace.PerformingOrganizationChangeFlag = 1  
**											for specific workspace
**		11/4/11		dcanuso				WI 5854
**										Updates needed to retain DTS Perf Org in Perf Org list id=1
**										Goes along with dev bug 5772
**										deletePerformingOrganizationByPerformingOrganizationListID 
**										when ListID = 1, 
**										never delete the PerfOrgName=DTs and 
**										PerfOrgDesc="Distributed Time System" row
**		2/11/13		dcanuso				WI14842 Redesign Performing Organization
**		3/3/2017	twilson3			BOEJ-1861 Remove DTS
******************************************************************************/
SET NOCOUNT ON 
/*Check for WI 5854*/

	IF (SELECT UpdateDT FROM dbo.PerformingOrganizationList WHERE PerformingOrganizationListID = @PerformingOrganizationListID) = @UpdateDT
		BEGIN

			/*Setting delete flag rather than deleting PO*/
			UPDATE [dbo].[PerformingOrganization] 
			SET DeletedFlag = 1
			WHERE 
				PerformingOrganizationListID = @PerformingOrganizationListID
				/*AND PerformingOrganizationInUseFlag = 0 */
				
				
			IF @PerformingOrganizationListID = 1 /*IS&GS GLOBAL LIST*/
				BEGIN
				/*A system admin has added a new Performing Organization, 
				thus the flag gets set on all Workspaces that a Performing Organization has changed*/
				UPDATE [dbo].[Workspace]
					SET PerformingOrganizationChangeFlag = 1
				END
			ELSE
				BEGIN
				/* A WS Admin added/changed a Performing Organization and
				thus the flag is set for the one Workspace*/
				UPDATE [dbo].[Workspace]
					SET PerformingOrganizationChangeFlag = 1
				WHERE
					PerformingOrganizationListID = @PerformingOrganizationListID
				END		
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =   'The Performing Organization List with ID ' + CAST(@PerformingOrganizationListID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO