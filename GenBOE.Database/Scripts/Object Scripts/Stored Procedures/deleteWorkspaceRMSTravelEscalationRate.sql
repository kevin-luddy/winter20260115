IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceRMSTravelEscalationRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceRMSTravelEscalationRate];
GO

CREATE PROCEDURE [dbo].[deleteWorkspaceRMSTravelEscalationRate]
(
@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteWorkspaceRMSTravelEscalationRate]
**		Desc: deletes all RMS Travel Escalation Rate records for specified Workspace
**			
**		
**
**		Auth: Greg Brunworth
**		Date: 01/06/17
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			----------------------------------------
*******************************************************************************/
SET NOCOUNT ON 
				
DELETE FROM [dbo].[WorkspaceRMSTravelEscalationRate]
WHERE [WorkspaceID] = @WorkspaceID

GO