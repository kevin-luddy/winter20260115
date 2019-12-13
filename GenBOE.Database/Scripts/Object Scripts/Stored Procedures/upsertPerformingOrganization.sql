IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertPerformingOrganization]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertPerformingOrganization];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertPerformingOrganization]
(
@PerformingOrganizationID int,
@PerformingOrganizationName varchar(20),
@PerformingOrganizationDescription varchar(50),
@PerformingOrganizationListID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: upsertPerformingOrganization
**		Desc: Insert/Update into PerformingOrganization Table
**			
**		
**
**		Auth: Don Canuso
**		Date: 1/21/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2/4/11		dcanuso				Added Flag Processing:
**										Add: Workspace.PerformingOrganizationChangeFlag
**										IF SA Changes a Global PerformingOrganization – Set All Workspace.PerformingOrganizationChangeFlag = 1
**										IF WS Admin changes a Workspace PerformingOrganization, set Workspace.PerformingOrganizationChangeFlag = 1  
**											for specific workspace
**		3/14/11		dcanuso				PerformingOrganization.Description changed to all 30 characters
**		11/4/11		dcanuso				WI 5854
**										Updates needed to retain DTS Perf Org in Perf Org list id=1
**										Goes along with dev bug 5772
**										upsertPerformingOrganization a row containg 
**										PerfOrgListID=1, 
**										PerfOrgName=DTS, and 
**										PerfOrgDesc="Distributed Time System 
**										should not be updated
**		2/4/13		dcanuso				WI14842 Redesign Performing Organization
**		3/3/2017	twilson3			BOEJ-1861 Remove DTS
**		2/22/18		Dusan				BOEJ-3144 Increase PerformingOrganizationDescription to 50 characters
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @InsertedPerformingOrganization AS Table (PerformingOrganizationID int)
DECLARE @ErrorMessage varchar (500)

IF @PerformingOrganizationID < 0 
	BEGIN

	SET @UpdateDT = GETDATE()

	INSERT INTO [dbo].[PerformingOrganization]
           ([PerformingOrganizationName]
           ,[PerformingOrganizationDescription]
           ,[PerformingOrganizationListID]
           ,[UpdateDT])
     OUTPUT inserted.PerformingOrganizationID INTO @InsertedPerformingOrganization
     VALUES
           (@PerformingOrganizationName
           ,@PerformingOrganizationDescription
           ,@PerformingOrganizationListID
           ,@UpdateDT)

           
      SELECT @PerformingOrganizationID = PerformingOrganizationID FROM @InsertedPerformingOrganization
      
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
/*	
	IF (SELECT PerformingOrganizationInUseFlag FROM [dbo].[PerformingOrganization] WHERE PerformingOrganizationID = @PerformingOrganizationID ) = 1
	BEGIN
		SET @ErrorMessage =   'The Performing Organization with Name ' + @PerformingOrganizationName + ' is in use'
		RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,
					1 -- State,
				   )
		RETURN
	END
*/	
	IF (SELECT UpdateDT FROM dbo.[PerformingOrganization] WHERE PerformingOrganizationID = @PerformingOrganizationID) = @UpdateDT
		BEGIN 
			SET @UpdateDT = GETDATE()
			
			UPDATE [dbo].[PerformingOrganization]
			   SET	[PerformingOrganizationName] = @PerformingOrganizationName
					,[PerformingOrganizationDescription] = @PerformingOrganizationDescription
					,[PerformingOrganizationListID] = @PerformingOrganizationListID
					,[UpdateDT] = @UpdateDT
			WHERE	PerformingOrganizationID = @PerformingOrganizationID 
			
			
		IF @PerformingOrganizationListID = 1 /*IS&GS GLOBAL LIST*/
			BEGIN
			/*A system admin has added a new Performing Organization, 
			thus the flag gets set on all Workspaces that a Performing Organization has changed*/
			UPDATE [dbo].[Workspace]
				SET PerformingOrganizationChangeFlag = 1
			END
		ELSE
			BEGIN
			/* A WS Admin added/changed a PerformingOrganization and
			thus the flag is set for the one Workspace*/
			UPDATE [dbo].[Workspace]
				SET PerformingOrganizationChangeFlag = 1
			WHERE
				PerformingOrganizationListID = @PerformingOrganizationListID
			END			
		
		
		
		

		END
		
	ELSE
			BEGIN

				SET @ErrorMessage =   'The Performing Organization with Name ' + @PerformingOrganizationName + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
           			
END

IF @@ERROR = 0
	SELECT @PerformingOrganizationID as PerformingOrganizationID
GO