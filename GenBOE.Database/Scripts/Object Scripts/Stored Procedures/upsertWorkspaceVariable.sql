IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspaceVariable]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspaceVariable];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertWorkspaceVariable]
(
@WorkspaceVariableID int,
@WorkspaceVariableName varchar(20),
@WorkspaceVariableValue decimal (29,10),
@WorkspaceID int,
@SortByID int,
@ValueTypeID int,
@IsPercentage bit,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name:	[upsertWorkspaceVariable]
**		Desc:	Insert/Update WorkspaceVariable
**			
**		
**
**		Auth: Don Canuso
**		Date: 10/2/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		1/13/2011	dcanuso				Workspace Variable name changed to 20 characters
**		3/17/11		dcanuso				New columns added to Workspace Variable Table
**		3/24/11		dcanuso				variable changed to decimal (29,10)
**		4/28/11		dcanuso				Adding Duplicate check with SP rather than 
**										Unique Index
**		8/3/11		dcanuso				Developer request to add IsPercentage
**		8/31/11		dcanuso				User is now able to select the Resource
**										Types that will be used for Summing BOEs
**		9/13/11		dcanuso				Moving 8/31 code to new insert/delete SP
*******************************************************************************/
SET NOCOUNT ON 
DECLARE	@ErrorMessage varchar (500)

DECLARE @InsertedWorkspaceVariable AS Table (WorkspaceVariableID int)


IF @WorkspaceVariableID < 0
	BEGIN
	
		IF EXISTS	(SELECT 1 FROM dbo.WorkspaceVariable 
						WHERE	WorkspaceVariableName = @WorkspaceVariableName AND
								WorkspaceID = @WorkspaceID
					)
			BEGIN
				SET @ErrorMessage =   'There is already a Workspace Variable with Name ' + @WorkspaceVariableName 
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
		ELSE
			BEGIN	
		
				SELECT @UpdateDT = GETDATE()
				
				INSERT INTO [dbo].[WorkspaceVariable]
				   ([WorkspaceVariableName]
				   ,[WorkspaceVariableValue]
				   ,[WorkspaceID]
				   ,[SortByID]
				   ,[ValueTypeID]
				   ,[IsPercentage]
				   ,[UpdateDT])
				OUTPUT inserted.WorkspaceVariableID INTO @InsertedWorkspaceVariable
				VALUES
				   (@WorkspaceVariableName
				   ,@WorkspaceVariableValue
				   ,@WorkspaceID
				   ,@SortByID
				   ,@ValueTypeID
				   ,@IsPercentage
				   ,@UpdateDT
				   )
		           
				SELECT @WorkspaceVariableID = WorkspaceVariableID FROM @InsertedWorkspaceVariable
				
				
			END
			
			
		







	END			
ELSE
	BEGIN
	    IF (SELECT UpdateDT FROM dbo.WorkspaceVariable WHERE WorkspaceVariableID = @WorkspaceVariableID) = @UpdateDT
			BEGIN
				SELECT @UpdateDT = GETDATE()
				
				UPDATE [dbo].[WorkspaceVariable]
					SET [WorkspaceVariableName] = @WorkspaceVariableName,
						[WorkspaceVariableValue] = @WorkspaceVariableValue,
						[SortByID] = @SortByID,
						[ValueTypeID] = @ValueTypeID,
						[IsPercentage] = @IsPercentage,
						[UpdateDT] = @UpdateDT
					WHERE
						WorkspaceVariableID = @WorkspaceVariableID
						
			END
		ELSE
			BEGIN
				SET @ErrorMessage =   'The Workspace Variable with Name ' + @WorkspaceVariableName + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
END

IF @@ERROR = 0
	SELECT @WorkspaceVariableID AS WorkspaceVariableID
GO