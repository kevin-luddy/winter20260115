IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deletePerformingOrganizationByPerformingOrganizationID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deletePerformingOrganizationByPerformingOrganizationID];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deletePerformingOrganizationByPerformingOrganizationID]
(
@PerformingOrganizationID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deletePerformingOrganizationByPerformingOrganizationID]
**		Desc: Delete PerformingOrganization based on its PerformingOrganizationID
**			
**
**		Auth: Don Canuso
**		Date: 1/24/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2/11/11		dcanuso				Added Flag Processing:
**										Add: Workspace.PerformingOrganizationChangeFlag
**										IF SA Changes a Global PerformingOrganization – Set All Workspace.PerformingOrganizationChangeFlag = 1
**										IF WS Admin changes a Workspace PerformingOrganization, set Workspace.PerformingOrganizationChangeFlag = 1  
**											for specific workspace
**		3/23/11		dcanuso				Add Error if trying to update an In Use record
**		
**		11/4/11		dcanuso				WI 5854
**										Updates needed to retain DTS Perf Org in Perf Org list id=1
**										Goes along with dev bug 5772
**										deletePerformingOrganizationByPerformingOrganizationID
**										a perfOrgID that also contains PerfOrgListID=1, 
**										PerfOrgName=DTS, and PerfOrgDesc="Distributed Time System 
**										should not be deleted
**		3/3/2017	twilson3			BOEJ-1861 Remove DTS
******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)


/*NO LONGER USING IN USE FLAG
IF (SELECT PerformingOrganizationInUseFlag FROM [dbo].[PerformingOrganization] WHERE PerformingOrganizationID = @PerformingOrganizationID ) = 1
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




	IF (SELECT UpdateDT FROM [dbo].[PerformingOrganization] WHERE PerformingOrganizationID = @PerformingOrganizationID) = @UpdateDT
		BEGIN
		
			DECLARE @PerformingOrganizationListID int
			SELECT @PerformingOrganizationListID = PerformingOrganizationListID 
			FROM dbo.PerformingOrganization
			WHERE PerformingOrganizationID = @PerformingOrganizationID

			/*Setting delete flags rather than deleting PO*/
			UPDATE [dbo].[PerformingOrganization] 
			SET DeletedFlag = 1
			WHERE	PerformingOrganizationID = @PerformingOrganizationID 

			
/*			DELETE FROM [dbo].[PerformingOrganization] 
			WHERE	PerformingOrganizationID = @PerformingOrganizationID AND
					PerformingOrganizationInUseFlag = 0
*/			
		
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

			SET @ErrorMessage =   'The Performing Organization with ID ' + CAST(@PerformingOrganizationID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO