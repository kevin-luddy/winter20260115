IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertSystemResource]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertSystemResource];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertSystemResource]
(
@ResourceID int,
@ResourceName varchar(20),
@ResourceDescription varchar(100),
@SegmentRegion varchar(50),
@LaborType varchar(50),
@SegmentID int,
@ResourceListID int,
@CostElementID int,
@UpdateDT datetime2,
@RateTypeID int
)
AS
/******************************************************************************
**		 
**		Name: upsertResource
**		Desc: Insert/Update into Resource Table
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
**										Add: Workspace.ResourceChangeFlag
**										IF SA Changes a Global Resource – Set All Workspace.ResourceChangeFlag = 1
**										IF WS Admin changes a Workspace Resource, set Workspace.ResourceChangeFlag = 1  
**										for specific workspace
**										Added Resource In Use Flag		
**		3/23/11		dcanuso				Added Error when editing In Use row	
**		5/23/11		dcanuso				Added 2 new variables
**		6/2/11		dcanuso				Burden Pool removed
**		6/13/11		dcanuso				New Resource Table columns added
**		8/2/11		dcanuso				WI 4436: remove the column Company 
**										from the Resource table and any SPs.
**		8/2/11		dcanuso				WI TBD: remove the column Labor Category 
**										from the Resource table and any SPs.
**		9/15/11		dcanuso				Resource.ResourceName and Segment Region
**										updated to varchar (30)
**		9/27/11		dcanuso				Resource.ResourceDescription, Segment Region,
**										Labor Type updated to varchar (50)
**		2/7/2012	dcanuso				WI 7145 - Resource Update allowed
**										When In Use, ResourceName and CostElementID 
**										can not be updated
**										When NOT in use, all can be updated
**										InUse Updates
**		7/24/12		dcanuso				WI 8960 Redesign Resources
**										SP only used for system resources
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		1/17/13		dcanuso				WI 14477 Resource gets marked as Deleted,
**										create new resource row. 
**										And if labor resource rate already existed, 
**										create new copy and point it to the new resource
**		1/28/13		dcanuso				WI14779 Remove Workspace.ResourceChangeFlag
**		7/25/13		dcanuso				WI 1939 Updates to Labor Tab
**		1/26/15		kotwickm			Updated resource desc to 100 characters
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
*******************************************************************************/
SET NOCOUNT ON 
IF @ResourceListID > 1
	RETURN

DECLARE @InsertedResource AS Table (ResourceID int)
DECLARE @ErrorMessage varchar (500)

IF @ResourceID < 0 
	BEGIN

	SET @UpdateDT = GETDATE()


	INSERT INTO [dbo].[Resource]
           ([ResourceName]
           ,[ResourceDescription]
           ,[SegmentRegion]
           ,[LaborType]
           ,[SegmentID]
           ,[ResourceListID]
           ,[CostElementID]
           ,[UpdateDT]
           ,[DeletedFlag]
           ,[RateTypeID]
           )
     OUTPUT inserted.ResourceID INTO @InsertedResource
     VALUES
            (@ResourceName,
			@ResourceDescription,
			@SegmentRegion,
			@LaborType,
			@SegmentID,
			@ResourceListID,
			@CostElementID,
			@UpdateDT,
			0/*New entry should be False for Deleted Flag*/,
			@RateTypeID
			)
			

           
      SELECT @ResourceID = ResourceID FROM @InsertedResource
/*      
	IF @ResourceListID = 1 /*IS&GS GLOBAL LIST*/
		BEGIN
		/*
		A system admin has added a new Resource, 
		thus the flag gets set on all Workspaces that 
		a Resource has changed
		*/
		UPDATE [dbo].[Workspace]
			SET ResourceChangeFlag = 1
		END
*/
     END
ELSE
	BEGIN
	
	IF (SELECT UpdateDT FROM dbo.[Resource] WHERE ResourceID = @ResourceID) = @UpdateDT
		BEGIN 
			SET @UpdateDT = GETDATE()
			

			/*
				Update to System Level Resources are truly Deletes and Adds
				The Delete is setting the flag as we never deleted
				System Level Resources because Workspaces can be using them
				So, we set the delete flag and add a new row so new Workspaces
				get the new edit
			*/
			
			DECLARE @InUseFlag int
			DECLARE @temp TABLE (ReturnValue int NOT NULL)
			INSERT @temp EXECUTE [dbo].[getResourceInUseFlagBySystemResourceID] @ResourceID
			SELECT @InUseFlag = ReturnValue FROM @temp
			
			IF @InUseFlag = 1
			BEGIN				
			
			UPDATE [dbo].[Resource]
			SET DeletedFlag = 1
			WHERE ResourceID = @ResourceID
							

			/*
			When the Resource is in use, we only edit a select number of columns
			UPDATE [dbo].[Resource]
			   SET	 /*WI7145 In Use Can Not Edit Resource Name[ResourceName] = @ResourceName
					,*/[ResourceDescription] = @ResourceDescription
					,[SegmentRegion] = @SegmentRegion
					,[LaborType] = @LaborType				  
					/*WI7145 In Use Can Not Edit SegmentID,[SegmentID] = @SegmentID*/
					,[ResourceListID] = @ResourceListID
					/*WI7145 In Use Can Not Edit Cost Element,[CostElementID] = @CostElementID*/
					,[UpdateDT] = @UpdateDT
			WHERE	ResourceID = @ResourceID/* AND
					[ResourceInUseFlag] = 1*/
			*/
			
			INSERT INTO [dbo].[Resource]
			   ([ResourceName]
			   ,[ResourceDescription]
			   ,[SegmentRegion]
			   ,[LaborType]
			   ,[SegmentID]
			   ,[ResourceListID]
			   ,[CostElementID]
			   ,[UpdateDT]
			   ,[DeletedFlag]
			   ,[RateTypeID]
			   )
		     OUTPUT inserted.ResourceID INTO @InsertedResource
			SELECT 
				ResourceName,
				@ResourceDescription,
				@SegmentRegion,
				@LaborType,
				SegmentID,
				ResourceListID,
				CostElementID,
				@UpdateDT,
				0/*New entry should be False for Deleted Flag*/,
				@RateTypeID
			FROM  [dbo].[Resource]
			WHERE	ResourceID = @ResourceID
			
           
	      SELECT @ResourceID = ResourceID FROM @InsertedResource

								
			END


			
			IF @InUseFlag = 0
			BEGIN		

			UPDATE [dbo].[Resource]
			SET DeletedFlag = 1
			WHERE ResourceID = @ResourceID

			/*
			When the Resource is NOT in use, we can edit all of the columns
			
					
			UPDATE [dbo].[Resource]
			   SET	 [ResourceName] = @ResourceName
					,[ResourceDescription] = @ResourceDescription
					,[SegmentRegion] = @SegmentRegion
					,[LaborType] = @LaborType				  
					,[SegmentID] = @SegmentID
					,[ResourceListID] = @ResourceListID
					,[CostElementID] = @CostElementID
					,[UpdateDT] = @UpdateDT
			WHERE	ResourceID = @ResourceID /*AND
					[ResourceInUseFlag] = 0*/
					
					
					
			*/
			
			
			INSERT INTO [dbo].[Resource]
           ([ResourceName]
           ,[ResourceDescription]
           ,[SegmentRegion]
           ,[LaborType]
           ,[SegmentID]
           ,[ResourceListID]
           ,[CostElementID]
           ,[UpdateDT]
           ,[DeletedFlag]
           ,[RateTypeID]
           )
		 OUTPUT inserted.ResourceID INTO @InsertedResource
		 VALUES
            (@ResourceName,
			@ResourceDescription,
			@SegmentRegion,
			@LaborType,
			@SegmentID,
			@ResourceListID,
			@CostElementID,
			@UpdateDT,
			0/*New entry should be False for Deleted Flag*/,
			@RateTypeID
			)
			

           
      SELECT @ResourceID = ResourceID FROM @InsertedResource

			
			END


/*			
	IF @ResourceListID = 1 /*IS&GS GLOBAL LIST*/
		BEGIN
		/*A system admin has added a new Resource, 
		thus the flag gets set on all Workspaces that a Resource has changed*/
		UPDATE [dbo].[Workspace]
			SET ResourceChangeFlag = 1
		END
*/



		END
		
	ELSE
			BEGIN

				SET @ErrorMessage =   'The Resource with Name ' + @ResourceName + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
           			
END

IF @@ERROR = 0
	SELECT @ResourceID as ResourceID
GO