IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertDefaultPerformingOrganization]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertDefaultPerformingOrganization];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertDefaultPerformingOrganization]
(
@WorkspaceID int,
@PerformingOrganizationListID int
)
AS
/******************************************************************************
**		 
**		Name: [insertDefaultPerformingOrganization]
**		Desc: Inserts the default PerformingOrganization List and PerformingOrganizations for a Workspace
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/4/2013 (Complete Rewrite)
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

IF EXISTS (SELECT 1 FROM dbo.Workspace WHERE WorkspaceID = @WorkspaceID AND PerformingOrganizationListID IS NULL)
	BEGIN

		/*Create PerformingOrganization List*/	
		DECLARE @UpdateDT datetime2  = GetDate()
		DECLARE @InsertedPerformingOrganizationList AS Table (PerformingOrganizationListID int)

		DECLARE @PerformingOrganizationListName varchar(50)
		IF @PerformingOrganizationListID = 1
			BEGIN
				SET @PerformingOrganizationListName = 'Performing Organization'
			END
		ELSE
			BEGIN
				SELECT @PerformingOrganizationListName = PerformingOrganizationListName 
				FROM dbo.PerformingOrganizationList
				WHERE 	PerformingOrganizationListID = @PerformingOrganizationListID
			END	
				
		INSERT INTO [dbo].[PerformingOrganizationList]
           ([PerformingOrganizationListName]
           ,[UpdateDT])
		OUTPUT inserted.PerformingOrganizationListID INTO @InsertedPerformingOrganizationList
		VALUES
           (
           /*By default, the initial PerformingOrganization list is named PerformingOrganization*/
           @PerformingOrganizationListName,/*'PerformingOrganization'*/
           @UpdateDT
           )
	        
			/*
			@PerformingOrganizationListID is the PerformingOrganization List ID that we are copying from
			Now, we need a new variable to use in our INSERT
			*/
			DECLARE @NewPerformingOrganizationListID int
	 		SELECT @NewPerformingOrganizationListID = PerformingOrganizationListID FROM @InsertedPerformingOrganizationList
	 


IF @PerformingOrganizationListID = 1
BEGIN
	/*Create the Reources under the List*/
	INSERT INTO [dbo].[WorkspacePerformingOrganization]
           ([SystemPerformingOrganizationID]
           ,[PerformingOrganizationListID]
           ,[WorkspaceID])
		SELECT 
			 [PerformingOrganizationID]
			,@NewPerformingOrganizationListID
			,@WorkspaceID
		  FROM [dbo].[PerformingOrganization] R 
		  /*We are using the PerformingOrganization List ID - the one we are copying FROM*/
		 WHERE 
			R.PerformingOrganizationListID = @PerformingOrganizationListID
		   AND	(
					R.DeletedFlag = 0 OR
					R.DeletedFlag IS NULL
				)
		/* 1 = IS&GS Default/Global PerformingOrganization List*/


						
END
ELSE
BEGIN
		/*Create the PO under the List*/
		
		INSERT INTO [dbo].[PerformingOrganization]
           ([UpdateDT]
           ,[PerformingOrganizationName]
           ,[PerformingOrganizationDescription]
           ,[PerformingOrganizationListID]
           ,[DeletedFlag])
        SELECT @UpdateDT
			  ,[PerformingOrganizationName]
			  ,[PerformingOrganizationDescription]
			  ,@NewPerformingOrganizationListID  --[PerformingOrganizationListID]
			  ,0 --[DeletedFlag]
		FROM [dbo].[PerformingOrganization]
		WHERE PerformingOrganizationListID = @PerformingOrganizationListID


		INSERT INTO [dbo].[WorkspacePerformingOrganization]
           ([SystemPerformingOrganizationID]
           ,[PerformingOrganizationListID]
           ,[WorkspaceID])
		SELECT 
			 R.[PerformingOrganizationID]
			,@NewPerformingOrganizationListID
			,@WorkspaceID
		  FROM [dbo].[WorkspacePerformingOrganization] WR
			INNER JOIN [dbo].[PerformingOrganization] R ON WR.SystemPerformingOrganizationID = R.PerformingOrganizationID
		  /*We are using the PerformingOrganization List ID - the one we are copying FROM*/		  
		 WHERE 
			/*Using the System PerformingOrganizations that the current list/workspace is using*/
			WR.PerformingOrganizationListID = @PerformingOrganizationListID AND
			R.PerformingOrganizationListID = 1
			/*
				Removed for WI 13829
					 AND	(
					R.DeletedFlag = 0 OR
					R.DeletedFlag IS NULL
				)
			*/
	UNION
		SELECT 
			 R2.[PerformingOrganizationID]
			,@NewPerformingOrganizationListID
			,@WorkspaceID
		  FROM [dbo].[WorkspacePerformingOrganization] WR
			INNER JOIN [dbo].[PerformingOrganization] R ON WR.SystemPerformingOrganizationID = R.PerformingOrganizationID
			INNER JOIN [dbo].[PerformingOrganization] R2 ON R.PerformingOrganizationName = R2.PerformingOrganizationName
		  /*We are using the PerformingOrganization List ID - the one we are copying FROM*/		  
		 WHERE 
			/*Using the System PerformingOrganizations that the current list/workspace is using*/
			WR.PerformingOrganizationListID = @PerformingOrganizationListID AND
			R.PerformingOrganizationListID = @PerformingOrganizationListID AND
			R2.PerformingOrganizationListID = @NewPerformingOrganizationListID


END

	UPDATE dbo.Workspace
	SET PerformingOrganizationListID = @NewPerformingOrganizationListID
	WHERE WorkspaceID = @WorkspaceID

	END
GO