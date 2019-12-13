IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[resetWorkspaceDefaultFees]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[resetWorkspaceDefaultFees];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
	
CREATE PROCEDURE [dbo].[resetWorkspaceDefaultFees]
(
@WorkspaceId int
)
AS
/******************************************************************************
**		 
**		Name: [resetWorkspaceDefaultFees]
**		Desc: Resets Workspace Default Fees to be a copy of the System Zone Travel Fees
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

DELETE FROM [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts] WHERE WorkspaceId = @WorkspaceId

INSERT INTO [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts]
			([UpdateDT]
			    ,[WorkspaceID]
				,[ModeID]
				,[TravelAgencyFee]
				,[MiscOther])
SELECT [UpdateDT],
			@WorkspaceID,
			[ModeID],
			[TravelAgencyFee],
			[MiscOther]
FROM [dbo].[MSTTravelNonzoneFeesAndCosts]

GO