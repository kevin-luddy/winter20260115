IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspaceResourceInUseFlagByResourceID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspaceResourceInUseFlagByResourceID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[getWorkspaceResourceInUseFlagByResourceID]
(
@ResourceID int,
@ResourceListID int
)
AS
/******************************************************************************
**		 
**		Name:	[getWorkspaceResourceInUseFlagByResourceID]
**		Desc:	Returns ResourceID for those and In Use Flag
**			
**
**		Auth: Don Canuso
**		Date: 7/24/2012
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		11/28/12	dcanuso				Make sure Resource is not Deleted (Delete Flag)
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**      4/3/24      e302876             PROPH-1617 - update stored procs for BRC
*******************************************************************************/
SET NOCOUNT ON 

IF @ResourceID IS NULL
	RETURN


IF EXISTS (
	SELECT T.ResourceID FROM  [dbo].[ODCType] T 
		INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID	
		INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
		INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		INNER JOIN dbo.WorkspaceResource WR ON W.WorkspaceID = WR.WorkspaceID
	WHERE 
	T.ResourceID = @ResourceID AND 
	WR.SystemResourceID = @ResourceID AND
	W.ResourceListID = @ResourceListID
	
UNION
	SELECT T.ResourceID FROM  [dbo].[BOELaborType] T
		INNER JOIN dbo.BOETaskElement TE ON T.BOETaskElementID = TE.BOETaskElementID
		INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
		INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		INNER JOIN dbo.WorkspaceResource WR ON W.WorkspaceID = WR.WorkspaceID
	WHERE 
	T.ResourceID IS NOT NULL AND
	T.ResourceID = @ResourceID AND 
	WR.SystemResourceID = @ResourceID AND
	W.ResourceListID = @ResourceListID
UNION
	SELECT T.BRCResourceID FROM  [dbo].[BOELaborType] T
		INNER JOIN dbo.BOETaskElement TE ON T.BOETaskElementID = TE.BOETaskElementID
		INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
		INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		INNER JOIN dbo.WorkspaceResource WR ON W.WorkspaceID = WR.WorkspaceID
	WHERE 
	T.BRCResourceID IS NOT NULL AND
	T.BRCResourceID = @BRCResourceID AND 
	WR.SystemResourceID = @BRCResourceID AND
	W.ResourceListID = @ResourceListID
UNION
/* Special Processing for Travel */
		/* Special Processing for Travel */
		SELECT R.ResourceID
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
		WR.SystemResourceID = @ResourceID AND 
		WR.ResourceListID = @ResourceListID AND
		W.ResourceListID = @ResourceListID AND
		R.ResourceID = @ResourceID AND
		R.SegmentID = TT.SegmentID AND
		R.CostElementID = 6 /*Travel*/ AND
		R.DeletedFlag = 0
)
BEGIN
	SELECT 1 AS InUse
END	
ELSE
	BEGIN
		SELECT 0 AS InUse
	END
GO