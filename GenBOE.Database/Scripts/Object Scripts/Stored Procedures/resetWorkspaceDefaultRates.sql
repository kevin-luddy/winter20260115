IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[resetWorkspaceDefaultRates]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[resetWorkspaceDefaultRates];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
	
CREATE PROCEDURE [dbo].[resetWorkspaceDefaultRates]
(
@WorkspaceId int
)
AS
/******************************************************************************
**		 
**		Name: [resetWorkspaceDefaultRates]
**		Desc: Resets Workspace Zone Travel Rates to be a copy of the System Zone Travel Rates
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 11/20/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

DELETE FROM [dbo].[WorkspaceRMSTravelEscalationRate] WHERE WorkspaceId = @WorkspaceId

INSERT INTO [dbo].[WorkspaceRMSTravelEscalationRate]
			([UpdateDT]
			    ,[WorkspaceID]
				,[Year]
				,[Escalation]
				,[PerDiemRate]
				,[MiscRate])
SELECT [UpdateDT],
			@WorkspaceID,
			[Year],
			[DevEscalation],
			[LMSIEscalation],
			[MiscRate] 
FROM [dbo].[TravelEscalationRate]

GO