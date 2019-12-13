IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceVariableResourceType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceVariableResourceType];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteWorkspaceVariableResourceType]
(
@WorkspaceVariableID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteWorkspaceVariableResourceType]
**		Desc: deletes Workspace Variable and Resource Type
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
				
DELETE FROM [dbo].[WorkspaceVariableSumVariableResourceTypeXREF]
WHERE [WorkspaceVariableID] = @WorkspaceVariableID


GO