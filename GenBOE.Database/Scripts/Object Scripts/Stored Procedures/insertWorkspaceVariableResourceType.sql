IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertWorkspaceVariableResourceType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertWorkspaceVariableResourceType];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[insertWorkspaceVariableResourceType]
(
@WorkspaceVariableID int,
@SumVariableResourceTypeID int
)
AS
/******************************************************************************
**		 
**		Name: [insertWorkspaceVariableResourceType]
**		Desc: Insert Workspace Variable and Resource Type
**				No updates
**				Only inserts/deletes
**			
**		
**
**		Auth: Don Canuso
**		Date: 09/13/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
*******************************************************************************/
SET NOCOUNT ON 
				
IF NOT EXISTS 
	(SELECT 1 FROM dbo.WorkspaceVariableSumVariableResourceTypeXREF 
		WHERE	WorkspaceVariableID = @WorkspaceVariableID AND 
				SumVariableResourceTypeID = @SumVariableResourceTypeID
	)
	INSERT INTO [dbo].[WorkspaceVariableSumVariableResourceTypeXREF]
				   ([WorkspaceVariableID]
				   ,[SumVariableResourceTypeID])
	SELECT @WorkspaceVariableID, @SumVariableResourceTypeID
GO