IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertSystemPerformingOrganization]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertSystemPerformingOrganization];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertSystemPerformingOrganization]
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
**		Name: upsertSystemPerformingOrganization
**		Desc: Insert/Update into PerformingOrganization Table
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/7/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		3/3/2017	twilson3			BOEJ-1861 Remove DTS
*******************************************************************************/
SET NOCOUNT ON 
IF @PerformingOrganizationListID > 1
	RETURN

DECLARE @InsertedPerformingOrganization AS Table (PerformingOrganizationID int)
DECLARE @ErrorMessage varchar (500)

IF @PerformingOrganizationID < 0 
	BEGIN

	SET @UpdateDT = GETDATE()


	INSERT INTO [dbo].[PerformingOrganization]
           ([PerformingOrganizationName]
           ,[PerformingOrganizationDescription]
           ,[PerformingOrganizationListID]
           ,[UpdateDT]
           ,[DeletedFlag]
           )
     OUTPUT inserted.PerformingOrganizationID INTO @InsertedPerformingOrganization
     VALUES
            (@PerformingOrganizationName,
			@PerformingOrganizationDescription,
			@PerformingOrganizationListID,
			@UpdateDT,
			0/*New entry should be False for Deleted Flag*/
			)
			

           
      SELECT @PerformingOrganizationID = PerformingOrganizationID FROM @InsertedPerformingOrganization
		/*
		A system admin has added a new PerformingOrganization, 
		thus the flag gets set on all Workspaces that 
		a PerformingOrganization has changed
		*/
		UPDATE [dbo].[Workspace]
			SET PerformingOrganizationChangeFlag = 1


     END
ELSE
	IF (SELECT UpdateDT FROM dbo.[PerformingOrganization] WHERE PerformingOrganizationID = @PerformingOrganizationID) = @UpdateDT
		BEGIN 
			SET @UpdateDT = GETDATE()
/*Applies to Resource			

			/*
			WI 14477
			1. PerformingOrganization gets marked as Deleted, 
				create new PerformingOrganization row. 
			*/

			
			/*
				Update to System Level PerformingOrganizations are truly Deletes and Adds
				The Delete is setting the flag as we never deleted
				System Level PerformingOrganizations because Workspaces can be using them
				So, we set the delete flag and add a new row so new Workspaces
				get the new edit
			*/

			DECLARE @InUseFlag int
			DECLARE @temp TABLE (ReturnValue int NOT NULL)
			INSERT @temp EXECUTE [dbo].[getPerformingOrganizationInUseFlagBySystemPerformingOrganizationID] @PerformingOrganizationID
			SELECT @InUseFlag = ReturnValue FROM @temp
			
			IF @InUseFlag = 1
			BEGIN				
		
			UPDATE [dbo].[PerformingOrganization]
			SET DeletedFlag = 1
			WHERE PerformingOrganizationID = @PerformingOrganizationID
							

			/*
			When the PerformingOrganization is in use, we only edit a select number of columns
			UPDATE [dbo].[PerformingOrganization]
			   SET	 /*WI7145 In Use Can Not Edit PerformingOrganization Name[PerformingOrganizationName] = @PerformingOrganizationName
					,*/[PerformingOrganizationDescription] = @PerformingOrganizationDescription
					,[SegmentRegion] = @SegmentRegion
					,[LaborType] = @LaborType				  
					/*WI7145 In Use Can Not Edit SegmentID,[SegmentID] = @SegmentID*/
					,[PerformingOrganizationListID] = @PerformingOrganizationListID
					/*WI7145 In Use Can Not Edit Cost Element,[CostElementID] = @CostElementID*/
					,[UpdateDT] = @UpdateDT
			WHERE	PerformingOrganizationID = @PerformingOrganizationID/* AND
					[PerformingOrganizationInUseFlag] = 1*/
			*/
			
			INSERT INTO [dbo].[PerformingOrganization]
			   ([PerformingOrganizationName]
			   ,[PerformingOrganizationDescription]
			   ,[PerformingOrganizationListID]
			   ,[UpdateDT]
			   ,[DeletedFlag]
			   )
		     OUTPUT inserted.PerformingOrganizationID INTO @InsertedPerformingOrganization
			SELECT 
				PerformingOrganizationName,
				@PerformingOrganizationDescription,
				PerformingOrganizationListID,
				@UpdateDT,
				0/*New entry should be False for Deleted Flag*/
			FROM  [dbo].[PerformingOrganization]
			WHERE	PerformingOrganizationID = @PerformingOrganizationID
			
           
	      SELECT @PerformingOrganizationID = PerformingOrganizationID FROM @InsertedPerformingOrganization

								
			END


			
			IF @InUseFlag = 0
			BEGIN		
*/
			UPDATE [dbo].[PerformingOrganization]
			SET DeletedFlag = 1
			WHERE PerformingOrganizationID = @PerformingOrganizationID


			/*
			When the PerformingOrganization is NOT in use, we can edit all of the columns
			
					
			UPDATE [dbo].[PerformingOrganization]
			   SET	 [PerformingOrganizationName] = @PerformingOrganizationName
					,[PerformingOrganizationDescription] = @PerformingOrganizationDescription
					,[SegmentRegion] = @SegmentRegion
					,[LaborType] = @LaborType				  
					,[SegmentID] = @SegmentID
					,[PerformingOrganizationListID] = @PerformingOrganizationListID
					,[CostElementID] = @CostElementID
					,[UpdateDT] = @UpdateDT
			WHERE	PerformingOrganizationID = @PerformingOrganizationID /*AND
					[PerformingOrganizationInUseFlag] = 0*/
					
					
					
			*/
			
			
			INSERT INTO [dbo].[PerformingOrganization]
           ([PerformingOrganizationName]
           ,[PerformingOrganizationDescription]
           ,[PerformingOrganizationListID]
           ,[UpdateDT]
           ,[DeletedFlag]
           )
		 OUTPUT inserted.PerformingOrganizationID INTO @InsertedPerformingOrganization
		 VALUES
            (@PerformingOrganizationName,
			@PerformingOrganizationDescription,
			@PerformingOrganizationListID,
			@UpdateDT,
			0/*New entry should be False for Deleted Flag*/
			)
			
      SELECT @PerformingOrganizationID = PerformingOrganizationID FROM @InsertedPerformingOrganization

		/*A system admin has added a new PerformingOrganization, 
		thus the flag gets set on all Workspaces that a PerformingOrganization has changed*/
		UPDATE [dbo].[Workspace]
			SET PerformingOrganizationChangeFlag = 1

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
           			
IF @@ERROR = 0
	SELECT @PerformingOrganizationID as PerformingOrganizationID
GO