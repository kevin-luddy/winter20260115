IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspaceResource]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspaceResource];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertWorkspaceResource]
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
**		Name: upsertWorkspaceResource
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
**		7/24/12		dcanuso				WI8960: SP will be used for non-System Resources
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		11/1/12		dcanuso				Meetings with SE caused some Business 
**										Rule changes - Rqmt doc will be created
**		1/28/13		dcanuso				WI 14779 Remove Workspace.ResourceChangeFlag
**		7/25/13		dcanuso				WI 19369 Changes for Labor Tab
**      1/25/16     kotwickm			Updating desc to 100 for SSC
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
*******************************************************************************/
SET NOCOUNT ON 

/*
If ResourceID < 0
Add Resource as a Workspace Resource to Resource Table 
Add Reference to Newly created Resource in Workspace Resource Table
*/


DECLARE @InsertedResource AS Table (ResourceID int)
DECLARE @ErrorMessage varchar (500)

DECLARE @WorkspaceID int
SELECT @WorkspaceID = WorkspaceID FROM dbo.Workspace
WHERE ResourceListID = @ResourceListID

DECLARE @CurrentResourceID int = @ResourceID

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
			0/*Flag set to false for new*/,
			@RateTypeID
			)
			

           
      SELECT @ResourceID = ResourceID FROM @InsertedResource
      
      
      INSERT INTO [dbo].[WorkspaceResource]
           ([SystemResourceID]
           ,[ResourceListID]
           ,[WorkspaceID])
	  VALUES
           (@ResourceID
           ,@ResourceListID
           ,@WorkspaceID)


		/*
		WI 14779 Remove Workspace.ResourceChangeFlag
		/* A WS Admin added/changed a Resource and
		thus the flag is set for the one Workspace*/
		UPDATE [dbo].[Workspace]
			SET ResourceChangeFlag = 1
		WHERE
			ResourceListID = @ResourceListID
		*/
		
	END			
ELSE	
	/*
		Resource ID > 0
		In this situation, Resource List being passed in may be incorrect
		System Lists may come in but we are only interested in Workspace Lists
		ResourceListID will need to be determined from Resource Table
		
		If Resource belongs to Resource List is 1, 
		Delete WSR, 
		Add Resource, 
		Add WSR
		
		If Resource List > 1 Then Straight Edit
	*/		


	BEGIN
		DECLARE @IsSystem int
		SELECT @IsSystem = ResourceListID 
			FROM [dbo].[Resource]
			WHERE ResourceID = @ResourceID

	
		
		IF @IsSystem = 1
		BEGIN
				
			SET @UpdateDT = GETDATE()
			
			DELETE FROM dbo.WorkspaceResource
			WHERE	SystemResourceID = @ResourceID AND
					ResourceListID = @ResourceListID AND
					WorkspaceID = @WorkspaceID
			
			INSERT INTO [dbo].[Resource]
				   ([ResourceName]
				   ,[ResourceDescription]
				   ,[SegmentRegion]
				   ,[LaborType]
				   ,[SegmentID]
				   ,[ResourceListID]
				   ,[CostElementID]
				   ,[UpdateDT]
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
					@RateTypeID
					)
			

           
      SELECT @ResourceID = ResourceID FROM @InsertedResource
      
      


      INSERT INTO [dbo].[WorkspaceResource]
           ([SystemResourceID]
           ,[ResourceListID]
           ,[WorkspaceID])
	  VALUES
           (@ResourceID
           ,@ResourceListID
           ,@WorkspaceID)


		/*
		WI 14779 Remove Workspace.ResourceChangeFlag
		/* A WS Admin added/changed a Resource and
		thus the flag is set for the one Workspace*/
		UPDATE [dbo].[Workspace]
			SET ResourceChangeFlag = 1
		WHERE
			ResourceListID = @ResourceListID
		*/			
			
		/*EXECUTE dbo.updateResourceInUseFlagByWorkspaceID @ResourceID*/
		
		
		
		/*Developer change - Now relink all items to new Resource*/
		UPDATE dbo.BOELaborType 
		 SET ResourceID = @ResourceID 
		 FROM dbo.BOELaborType LT
			INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
			--INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		 WHERE 
			LT.ResourceID = @CurrentResourceID AND
			B.WorkspaceID = @WorkspaceID 
			
		 UPDATE dbo.ODCType
		 SET ResourceID = @ResourceID 
		 FROM dbo.ODCType LT
			INNER JOIN dbo.ODCTaskElement TE ON LT.ODCTaskElementID = TE.ODCTaskElementID
			INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
			--INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		 WHERE 
			LT.ResourceID = @CurrentResourceID AND
			B.WorkspaceID = @WorkspaceID 	

     END
     
	ELSE /*IsSystem > 1 Thus Straight Edit*/
		BEGIN
			/*WI 7145  Removing	
			IF (SELECT ResourceInUseFlag FROM [dbo].[Resource] WHERE ResourceID = @ResourceID) = 1
				BEGIN

					SET @ErrorMessage =   'The Resource with Name ' + @ResourceName + ' is in use'
					RAISERROR (
						@ErrorMessage, -- Message text.
						11, -- Severity,
						1 -- State,
						)
					RETURN
				END
	           			
			*/           			
		IF (SELECT UpdateDT FROM dbo.[Resource] WHERE ResourceID = @ResourceID) = @UpdateDT
			BEGIN 
				SET @UpdateDT = GETDATE()



				
				DECLARE @InUseFlag int
				DECLARE @temp TABLE (ReturnValue int NOT NULL)
				INSERT @temp EXECUTE [dbo].[getWorkspaceResourceInUseFlagByResourceID] @ResourceID, @ResourceListID
				SELECT @InUseFlag = ReturnValue FROM @temp

				
				IF @InUseFlag = 1
				BEGIN				
				UPDATE [dbo].[Resource]
				   SET	 /*WI7145 In Use Can Not Edit Resource Name[ResourceName] = @ResourceName
						,*/[ResourceDescription] = @ResourceDescription
						,[SegmentRegion] = @SegmentRegion
						,[LaborType] = @LaborType				  
						/*WI7145 In Use Can Not Edit SegmentID,[SegmentID] = @SegmentID*/
						,[ResourceListID] = @ResourceListID
						/*WI7145 In Use Can Not Edit Cost Element,[CostElementID] = @CostElementID*/
						,[UpdateDT] = @UpdateDT
						,[RateTypeID] = @RateTypeID
				WHERE	ResourceID = @ResourceID
				END
				ELSE
				BEGIN
				UPDATE [dbo].[Resource]
				   SET	 [ResourceName] = @ResourceName
						,[ResourceDescription] = @ResourceDescription
						,[SegmentRegion] = @SegmentRegion
						,[LaborType] = @LaborType				  
						,[SegmentID] = @SegmentID
						,[ResourceListID] = @ResourceListID
						,[CostElementID] = @CostElementID
						,[UpdateDT] = @UpdateDT
						,[RateTypeID] = @RateTypeID
					WHERE	ResourceID = @ResourceID 
				END
		/*
		WI 14779 Remove Workspace.ResourceChangeFlag
			UPDATE [dbo].[Workspace]
				SET ResourceChangeFlag = 1
			WHERE
				ResourceListID = @ResourceListID
*/
	END			

	ELSE /*Update Dates Don't Match*/
			BEGIN

					SET @ErrorMessage =   'The Resource with Name ' + @ResourceName + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
						@ErrorMessage, -- Message text.
						11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
					RETURN
				END
	           			
	END/*End System Is a Straight Edit*/

END /*End Resource ID > 0*/
IF @@ERROR = 0
	SELECT @ResourceID as ResourceID
GO