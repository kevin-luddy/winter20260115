IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceRMSTravelNonzoneFeesAndCosts]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceRMSTravelNonzoneFeesAndCosts];
GO

CREATE PROCEDURE [dbo].[deleteWorkspaceRMSTravelNonzoneFeesAndCosts]
(
@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteWorkspaceRMSTravelNonzoneFeesAndCosts]
**		Desc: deletes all RMS Travel Non-zone Fees and Costs records for specified Workspace
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
				
DELETE FROM [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts]
WHERE [WorkspaceID] = @WorkspaceID

GO