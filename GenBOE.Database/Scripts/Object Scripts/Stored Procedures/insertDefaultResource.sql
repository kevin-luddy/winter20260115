IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertDefaultResource]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].insertDefaultResource;

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertDefaultResource]
(
@WorkspaceID int,
@ResourceListID int,
@SegmentID int
)
AS
/******************************************************************************
**		 
**		Name: [insertDefaultResource]
**		Desc: Inserts the default Resource List and Resources for a Workspace
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/4/2011
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		5/23/11		dcanuso				Added new columns to SP code
**		6/2/11		dcanuso				Burden Pool removed
**		6/13/11		dcanuso				Added new Resource columns
**		7/20/11		dcanuso				Added @ResourceListID
**		8/2/11		dcanuso				WI 4436: remove the column Company 
**										from the Resource table and any SPs.
**		8/2/11		dcanuso				WI TBD: remove the column Labor Category 
**										from the Resource table and any SPs.
**		2/9/12		dcanuso				Setting InUse to 0
**		12/4/12		dcanuso				WI 13509: Workspace and Segment (Resource)
**		12/20/12	dcanuso				Per SE: Deleted Resources are now copied
**		1/2/13		dcanuso				WI 13829 (Changes from 12/20) need to be
**										added for 1.5.4 but WI 13509 needs
**										to be removed
**		1/10/13		dcanuso				https://eureka.isgs.lmco.com/#activity/271515
**										Undo WI 13829
**		10/15/13	dcanuso				Resource.RateTypeID added
*******************************************************************************/
SET NOCOUNT ON 

IF EXISTS (SELECT 1 FROM dbo.Workspace WHERE WorkspaceID = @WorkspaceID AND ResourceListID IS NULL)
	BEGIN

		/*Create Resource List*/	
		DECLARE @UpdateDT datetime2  = GetDate()
		DECLARE @InsertedResourceList AS Table (ResourceListID int)

		DECLARE @ResourceListName varchar(50)
		IF @ResourceListID = 1
			BEGIN
				SET @ResourceListName = 'Resource'
			END
		ELSE
			BEGIN
				SELECT @ResourceListName = ResourceListName 
				FROM dbo.ResourceList
				WHERE 	ResourceListID = @ResourceListID
			END	
				
		INSERT INTO [dbo].[ResourceList]
           ([ResourceListName]
           ,[UpdateDT])
		OUTPUT inserted.ResourceListID INTO @InsertedResourceList
		VALUES
           (
           /*By default, the initial resource list is named Resource*/
           @ResourceListName,/*'Resource'*/
           @UpdateDT
           )
	        
			/*
			@ResourceListID is the Resource List ID that we are copying from
			Now, we need a new variable to use in our INSERT
			*/
			DECLARE @NewResourceListID int
	 		SELECT @NewResourceListID = ResourceListID FROM @InsertedResourceList
	 
/*System Resources used by 
Workspaces are no longer added to Resource table
They are added to 
Workspace Resource

		/*Create the Reources under the List*/
		INSERT INTO [dbo].[Resource]
           ([ResourceName]
           ,[ResourceDescription]
           ,[SegmentRegion]
           ,[LaborType]
           ,[SegmentID]
           ,[ResourceListID]
           ,[ResourceInUseFlag]
           ,[CostElementID]
           ,[UpdateDT])
		SELECT 
			   [ResourceName]
			  ,[ResourceDescription]
			  ,[SegmentRegion]
			  ,[LaborType]
			  ,[SegmentID]
			  ,@NewResourceListID
			  /*
			  Using The values from other resource lists
			  The values in the global list should be 0
			  */
			  ,0/*Now that System Resources can be InUse - need to set this as 0 rather than rely on System Resource In Use Flag [ResourceInUseFlag] - Initial [ResourceInUseFlag] would be 0 - Not in Use*/
			  ,[CostElementID]
			  ,@UpdateDT
		  FROM [dbo].[Resource]
		  /*We are using the Resource List ID - the one we are copying FROM*/
		 WHERE ResourceListID = @ResourceListID
		 /* 1 = IS&GS Default/Global Resource List*/
*/


IF @ResourceListID = 1
BEGIN
	/*Create the Reources under the List*/
	INSERT INTO [dbo].[WorkspaceResource]
           ([SystemResourceID]
           ,[ResourceListID]
           ,[WorkspaceID])
		SELECT 
			 [ResourceID]
			,@NewResourceListID
			,@WorkspaceID
		  FROM [dbo].[Resource] R 
		  /*We are using the Resource List ID - the one we are copying FROM*/
		 WHERE 
			R.ResourceListID = @ResourceListID
		   AND	(
					R.DeletedFlag = 0 OR
					R.DeletedFlag IS NULL
				)
		/* 1 = IS&GS Default/Global Resource List*/
		AND 
		(
			(
			@SegmentID IS NOT NULL AND
			ISNULL(R.SegmentID, -999) = ISNULL(@SegmentID, -999)
			) OR
			@SegmentID IS NULL
		)


						
END
ELSE
BEGIN
		/*Create the Reources under the List*/
		
		INSERT INTO [dbo].[Resource]
           ([UpdateDT]
           ,[ResourceName]
           ,[ResourceDescription]
           ,[SegmentRegion]
           ,[LaborType]
           ,[SegmentID]
           ,[ResourceListID]
           ,[CostElementID]
           ,[DeletedFlag]
           ,[RateTypeID]
           )
        SELECT @UpdateDT
			  ,[ResourceName]
			  ,[ResourceDescription]
			  ,[SegmentRegion]
			  ,[LaborType]
			  ,[SegmentID]
			  ,@NewResourceListID  --[ResourceListID]
			  ,[CostElementID]
			  ,0 --[DeletedFlag]
			  ,[RateTypeID]
		FROM [dbo].[Resource]
		WHERE ResourceListID = @ResourceListID


		INSERT INTO [dbo].[WorkspaceResource]
           ([SystemResourceID]
           ,[ResourceListID]
           ,[WorkspaceID])
		SELECT 
			 R.[ResourceID]
			,@NewResourceListID
			,@WorkspaceID
		  FROM [dbo].[WorkspaceResource] WR
			INNER JOIN [dbo].[Resource] R ON WR.SystemResourceID = R.ResourceID
		  /*We are using the Resource List ID - the one we are copying FROM*/		  
		 WHERE 
			/*Using the System Resources that the current list/workspace is using*/
			WR.ResourceListID = @ResourceListID AND
			R.ResourceListID = 1
			/*
				Removed for WI 13829
					 AND	(
					R.DeletedFlag = 0 OR
					R.DeletedFlag IS NULL
				)
			*/
	UNION
		SELECT 
			 R2.[ResourceID]
			,@NewResourceListID
			,@WorkspaceID
		  FROM [dbo].[WorkspaceResource] WR
			INNER JOIN [dbo].[Resource] R ON WR.SystemResourceID = R.ResourceID
			INNER JOIN [dbo].[Resource] R2 ON R.ResourceName = R2.ResourceName
		  /*We are using the Resource List ID - the one we are copying FROM*/		  
		 WHERE 
			/*Using the System Resources that the current list/workspace is using*/
			WR.ResourceListID = @ResourceListID AND
			R.ResourceListID = @ResourceListID AND
			R2.ResourceListID = @NewResourceListID
END

	UPDATE dbo.Workspace
	SET ResourceListID = @NewResourceListID
	WHERE WorkspaceID = @WorkspaceID

	END
GO