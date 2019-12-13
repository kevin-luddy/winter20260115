IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspaceHistoryLog]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspaceHistoryLog];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getWorkspaceHistoryLog]
(
@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name:	getWorkspaceHistoryLog
**		Desc:	Returns Workspace Status Change History
**			
**		
**
**		Auth: Adam Dille
**		Date: 1/12/2011
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON
 
SELECT
NS.WorkspaceState AS NewValue,
OS.WorkspaceState AS OldValue,
U.DisplayName AS DisplayName,
H.UpdateDT AS UpdateDate
FROM [dbo].[ETIuser] AS U
	INNER JOIN dbo.WorkspaceStateHistory AS H ON U.ETIUserID = H.ChangedByETIUserID
	INNER JOIN dbo.WorkspaceStateLU AS OS ON H.CurrentWorkspaceStateID = OS.WorkspaceStateID
	INNER JOIN dbo.WorkspaceStateLU AS NS ON H.UpdatedWorkspaceStateID = NS.WorkspaceStateID
WHERE H.WorkspaceID = @WorkspaceID
ORDER BY H.UpdateDT DESC

GO