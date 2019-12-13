IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSumOfBOEByWorkspaceVariableID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSumOfBOEByWorkspaceVariableID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteSumOfBOEByWorkspaceVariableID]
(
@WorkspaceVariableID int
)
AS
/******************************************************************************
**		 
**		Name: 
**		Desc: Deletes the SumOfBOE_WorkspaceVariableXREF rows for Workspace VariableID
**			
**		
**
**		Auth: Don Canuso
**		Date: 3/17/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/

SET NOCOUNT ON

DELETE FROM [dbo].[SumOfBOE_WorkspaceVariableXREF]
WHERE WorkspaceVariableID = @WorkspaceVariableID

GO