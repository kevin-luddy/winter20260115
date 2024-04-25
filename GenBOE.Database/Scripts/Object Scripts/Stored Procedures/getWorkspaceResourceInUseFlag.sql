IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspaceResourceInUseFlagByResourceID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspaceResourceInUseFlag];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[getWorkspaceResourceInUseFlag]
AS
/******************************************************************************
**		 
**		Name:	[getWorkspaceResourceInUseFlag]
**		Desc:	Returns all Workspace ResourceID that are InUse
**			
**
**		Auth: Tommy Lee
**		Date: 04/24/2024
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		04/24/24	e374897				Returns all Workspace Resource ID that are InUse
*******************************************************************************/
SET NOCOUNT ON

SELECT DISTINCT T.ResourceID AS ResourceID
FROM  [dbo].[ODCType] T 
	INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID	
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
	INNER JOIN dbo.WorkspaceResource WR ON W.WorkspaceID = WR.WorkspaceID
WHERE 
	T.ResourceID IS NOT NULL AND
	WR.SystemResourceID IS NOT NULL AND
	WR.ResourceListID IS NOT NULL AND
	WR.ResourceListID = W.ResourceListID
UNION
SELECT DISTINCT T.ResourceID AS ResourceID
FROM  [dbo].[BOELaborType] T
	INNER JOIN dbo.BOETaskElement TE ON T.BOETaskElementID = TE.BOETaskElementID
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
	INNER JOIN dbo.WorkspaceResource WR ON W.WorkspaceID = WR.WorkspaceID
WHERE 
	T.ResourceID IS NOT NULL OR
	T.BRCResourceID IS NOT NULL AND
	WR.SystemResourceID IS NOT NULL AND
	WR.ResourceListID IS NOT NULL AND
	WR.ResourceListID = W.ResourceListID
UNION
SELECT DISTINCT R.ResourceID AS ResourceID
FROM [dbo].[Resource] R
	INNER JOIN dbo.WorkspaceResource WR ON R.ResourceID = WR.SystemResourceID
	INNER JOIN dbo.Workspace W ON WR.WorkspaceID = W.WorkspaceID
	INNER JOIN 
	(
		SELECT BOEID, WorkspaceID
		FROM dbo.BOE
	) B ON W.WorkspaceID = B.WorkspaceID
	INNER JOIN 
		(
			SELECT [TravelTripTaskElementID]
			,[BOEID]
			FROM dbo.TravelTripTaskElement
		) TE ON B.BOEID = TE.BOEID
	INNER JOIN 
		(
			SELECT [TravelTripID]
			,[SegmentID]
			,[TravelTripTaskElementID]
			FROM dbo.TravelTrip
		) TT ON TE.TravelTripTaskElementID = TT.TravelTripTaskElementID 
WHERE 
	WR.SystemResourceID IS NOT NULL AND
	WR.ResourceListID IS NOT NULL AND
	W.ResourceListID IS NOT NULL AND
	R.SegmentID = TT.SegmentID AND
	R.CostElementID = 6 AND
	R.DeletedFlag = 0