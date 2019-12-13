IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[resetWorkspaceOffloadRates]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[resetWorkspaceOffloadRates];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
	
CREATE PROCEDURE [dbo].[resetWorkspaceOffloadRates]
(
@WorkspaceId int
)
AS
/******************************************************************************
**		 
**		Name: [resetWorkspaceOffloadRates]
**		Desc: Resets Workspace Offload Rates to be a copy of the System Offload Rates
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 9/25/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

DELETE FROM [dbo].[WorkspaceOffloadRate] WHERE WorkspaceId = @WorkspaceId

INSERT INTO [dbo].[WorkspaceOffloadRate]
			([WorkspaceID],
			[Resource],
			[PerfOrg],
			[PercentToOffload],
			[Year],
			[SubcontractorResource],
			[HourlyRate],
			[UpdateDT])
SELECT @WorkspaceID,
			[Resource],
			[PerfOrg],
			[PercentToOffload],
			[Year],
			[SubcontractorResource],
			[HourlyRate],
			[UpdateDT] 
FROM [dbo].[SystemOffloadRate]

GO