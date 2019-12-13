IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspaceVariableByTaskElementID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspaceVariableByTaskElementID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getWorkspaceVariableByTaskElementID]
(
@BOETaskElementID int
)
AS
/******************************************************************************
**		 
**		Name: [getWorkspaceVariableByTaskElementID]
**		Desc: Returns the Workspace Variables Used in a BOE Task Element
**			
**		
**
**		Auth: Don Canuso
**		Date: 10/4/10
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
**		11/17/10	dcanuso				Removing UpdateDT due to developer request
**		3/17/11		dcanuso				New columns added to Workspace Variable Table
**		8/5/11		dcanuso				Added IsPercentage column
*******************************************************************************/
SET NOCOUNT ON 

SELECT	WX.[BOETaskWSVarID] AS [BOETaskWSVarID],
		WX.[BOETaskElementID] AS [BOETaskElementID],
		WV.[WorkspaceVariableID] AS [WorkspaceVariableID],
		WV.[WorkspaceVariableName] AS [WorkspaceVariableName],
		WV.[WorkspaceVariableValue] AS [WorkspaceVariableValue],
		WV.[SortByID] AS [SortByID],
        WV.[ValueTypeID] AS [ValueTypeID],
        WV.[IsPercentage] AS [IsPercentage]
FROM dbo.BOETaskElementWorkspaceVariableXREF WX 
	INNER JOIN dbo.WorkspaceVariable WV ON WX.WorkspaceVariableID = WV.WorkspaceVariableID
WHERE 
	WX.[BOETaskElementID] = @BOETaskElementID

GO