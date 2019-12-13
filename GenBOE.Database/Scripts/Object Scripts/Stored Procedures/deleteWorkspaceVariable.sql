IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceVariable]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceVariable];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteWorkspaceVariable]
(
@WorkspaceVariableID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: deleteWorkspaceVariable
**		Desc: Deletes a Workspace Variable 
**			
**		   
**		
**
**		Auth: Don Canuso
**		Date: 10/2/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/31/11		DCANUSO				REFERNECE TABLES ADDED
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDT FROM dbo.WorkspaceVariable WHERE WorkspaceVariableID = @WorkspaceVariableID) = @UpdateDT
	BEGIN
			
		IF EXISTS (SELECT 1 FROM dbo.BOETaskElementWorkspaceVariableXREF WHERE WorkspaceVariableID = @WorkspaceVariableID)
			BEGIN
				/*
					WorkspaceVariableName is in use
				*/
					SET @ErrorMessage =   'The Workspace Variable with ID ' + CAST (@WorkspaceVariableID AS varchar(10)) + ' is in use within a BOE.'
					RAISERROR (
						@ErrorMessage, -- Message text.
						11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
					RETURN
			END
		ELSE
			BEGIN
				DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF WHERE WorkspaceVariableID = @WorkspaceVariableID 
				DELETE FROM dbo.WorkspaceVariableSumVariableResourceTypeXREF  WHERE WorkspaceVariableID = @WorkspaceVariableID
				DELETE FROM dbo.WorkspaceVariable WHERE WorkspaceVariableID = @WorkspaceVariableID 
			
			END
		
	END
ELSE
	BEGIN
		SET @ErrorMessage =   'The Workspace Variable with ID ' + CAST(@WorkspaceVariableID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		
		RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
	END

GO