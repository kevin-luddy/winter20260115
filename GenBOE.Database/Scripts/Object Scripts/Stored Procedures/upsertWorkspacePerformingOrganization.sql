IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspacePerformingOrganization]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspacePerformingOrganization];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertWorkspacePerformingOrganization]
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
**		Name: upsertWorkspacePerformingOrganization
**		Desc: Insert/Update into Workspace Performing Organization Table
**			
**		
**
**		Auth: Don Canuso
**		Date: 2/6/13
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		3/3/2017	twilson3			BOEJ-1861 Remove DTS
*******************************************************************************/
SET NOCOUNT ON 

/*
If PerformingOrganizationID < 0
Add PerformingOrganization as a Workspace PerformingOrganization to PerformingOrganization Table 
Add Reference to Newly created PerformingOrganization in Workspace PerformingOrganization Table
*/


DECLARE @InsertedPerformingOrganization AS Table (PerformingOrganizationID int)
DECLARE @ErrorMessage varchar (500)

DECLARE @WorkspaceID int
SELECT @WorkspaceID = WorkspaceID FROM dbo.Workspace
WHERE PerformingOrganizationListID = @PerformingOrganizationListID

DECLARE @CurrentPerformingOrganizationID int = @PerformingOrganizationID

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
			0/*Flag set to false for new*/
			)
			

           
      SELECT @PerformingOrganizationID = PerformingOrganizationID FROM @InsertedPerformingOrganization
      
      
      INSERT INTO [dbo].[WorkspacePerformingOrganization]
           ([SystemPerformingOrganizationID]
           ,[PerformingOrganizationListID]
           ,[WorkspaceID])
	  VALUES
           (@PerformingOrganizationID
           ,@PerformingOrganizationListID
           ,@WorkspaceID)



		UPDATE [dbo].[Workspace]
			SET PerformingOrganizationChangeFlag = 1
		WHERE
			PerformingOrganizationListID = @PerformingOrganizationListID

		
	END			
ELSE	
	/*
		PerformingOrganization ID > 0
		In this situation, PerformingOrganization List being passed in may be incorrect
		System Lists may come in but we are only interested in Workspace Lists
		PerformingOrganizationListID will need to be determined from PerformingOrganization Table
		
		If PerformingOrganization belongs to PerformingOrganization List is 1, 
		Delete WSR, 
		Add PerformingOrganization, 
		Add WSR
		
		If PerformingOrganization List > 1 Then Straight Edit
	*/		


	BEGIN
		DECLARE @IsSystem int
		SELECT @IsSystem = PerformingOrganizationListID 
			FROM [dbo].[PerformingOrganization]
			WHERE PerformingOrganizationID = @PerformingOrganizationID

	
		
		IF @IsSystem = 1
		BEGIN
				
			SET @UpdateDT = GETDATE()
			
			DELETE FROM dbo.WorkspacePerformingOrganization
			WHERE	SystemPerformingOrganizationID = @PerformingOrganizationID AND
					PerformingOrganizationListID = @PerformingOrganizationListID AND
					WorkspaceID = @WorkspaceID
			
			INSERT INTO [dbo].[PerformingOrganization]
				   ([PerformingOrganizationName]
				   ,[PerformingOrganizationDescription]
				   ,[PerformingOrganizationListID]
				   ,[UpdateDT])
			 OUTPUT inserted.PerformingOrganizationID INTO @InsertedPerformingOrganization
			 VALUES
					(@PerformingOrganizationName,
					@PerformingOrganizationDescription,
					@PerformingOrganizationListID,
					@UpdateDT
					)
			

           
      SELECT @PerformingOrganizationID = PerformingOrganizationID FROM @InsertedPerformingOrganization
      
      


      INSERT INTO [dbo].[WorkspacePerformingOrganization]
           ([SystemPerformingOrganizationID]
           ,[PerformingOrganizationListID]
           ,[WorkspaceID])
	  VALUES
           (@PerformingOrganizationID
           ,@PerformingOrganizationListID
           ,@WorkspaceID)


		/* 
		A WS Admin added/changed a PerformingOrganization and
		thus the flag is set for the one Workspace
		*/

		UPDATE [dbo].[Workspace]
			SET PerformingOrganizationChangeFlag = 1
		WHERE
			PerformingOrganizationListID = @PerformingOrganizationListID
			
		/*EXECUTE dbo.updatePerformingOrganizationInUseFlagByWorkspaceID @PerformingOrganizationID*/
		
		
		
		/*Developer change - Now relink all items to new PerformingOrganization*/
		UPDATE dbo.BOELaborType 
		 SET PerformingOrganizationID = @PerformingOrganizationID 
		 FROM dbo.BOELaborType LT
			INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
			--INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		 WHERE 
			LT.PerformingOrganizationID = @CurrentPerformingOrganizationID AND
			B.WorkspaceID = @WorkspaceID 
			
      
      
		 UPDATE dbo.ODCType
		 SET PerformingOrganizationID = @PerformingOrganizationID 
		 FROM dbo.ODCType LT
			INNER JOIN dbo.ODCTaskElement TE ON LT.ODCTaskElementID = TE.ODCTaskElementID
			INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
			--INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		 WHERE 
			LT.PerformingOrganizationID = @CurrentPerformingOrganizationID AND
			B.WorkspaceID = @WorkspaceID 
				
		


		 UPDATE dbo.TravelTrip
		 SET PerformingOrganizationID = @PerformingOrganizationID 
		 FROM dbo.TravelTrip LT
			INNER JOIN dbo.TravelTripTaskElement TE ON LT.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
			--INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		 WHERE 
			LT.PerformingOrganizationID = @CurrentPerformingOrganizationID AND
			B.WorkspaceID = @WorkspaceID 

		

     END
     
	ELSE /*IsSystem > 1 Thus Straight Edit*/
		BEGIN
			/*WI 7145  Removing	
			IF (SELECT PerformingOrganizationInUseFlag FROM [dbo].[PerformingOrganization] WHERE PerformingOrganizationID = @PerformingOrganizationID) = 1
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



				/*This is from Resource and does not seem to apply
				DECLARE @InUseFlag int
				DECLARE @temp TABLE (ReturnValue int NOT NULL)
				INSERT @temp EXECUTE [dbo].[getWorkspacePerformingOrganizationInUseFlagByPerformingOrganizationID] @PerformingOrganizationID, @PerformingOrganizationListID
				SELECT @InUseFlag = ReturnValue FROM @temp

				
				IF @InUseFlag = 1
				BEGIN				
				UPDATE [dbo].[PerformingOrganization]
				   SET	 /*WI7145 In Use Can Not Edit PerformingOrganization Name[PerformingOrganizationName] = @PerformingOrganizationName
						,*/[PerformingOrganizationDescription] = @PerformingOrganizationDescription
						/*WI7145 In Use Can Not Edit SegmentID,[SegmentID] = @SegmentID*/
						,[PerformingOrganizationListID] = @PerformingOrganizationListID
						/*WI7145 In Use Can Not Edit Cost Element,[CostElementID] = @CostElementID*/
						,[UpdateDT] = @UpdateDT
				WHERE	PerformingOrganizationID = @PerformingOrganizationID
				END
				ELSE
				BEGIN
				UPDATE [dbo].[PerformingOrganization]
				   SET	 [PerformingOrganizationName] = @PerformingOrganizationName
						,[PerformingOrganizationDescription] = @PerformingOrganizationDescription
						,[PerformingOrganizationListID] = @PerformingOrganizationListID
						,[UpdateDT] = @UpdateDT
					WHERE	PerformingOrganizationID = @PerformingOrganizationID 
				END

			UPDATE [dbo].[Workspace]
				SET PerformingOrganizationChangeFlag = 1
			WHERE
				PerformingOrganizationListID = @PerformingOrganizationListID
			*/


			
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

	ELSE /*Update Dates Don't Match*/
			BEGIN

					SET @ErrorMessage =   'The Performing Organization with Name ' + @PerformingOrganizationName + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
						@ErrorMessage, -- Message text.
						11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
					RETURN
				END
	           			
	END/*End System Is a Straight Edit*/

END /*End PerformingOrganization ID > 0*/
IF @@ERROR = 0
	SELECT @PerformingOrganizationID as PerformingOrganizationID
GO