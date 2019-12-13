IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspacePerformingOrganizationInUseFlagByPerformingOrganizationID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspacePerformingOrganizationInUseFlagByPerformingOrganizationID];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[getWorkspacePerformingOrganizationInUseFlagByPerformingOrganizationID]
(
@PerformingOrganizationID int,
@PerformingOrganizationListID int
)
AS
/******************************************************************************
**		 
**		Name:	[getWorkspacePerformingOrganizationInUseFlagByPerformingOrganizationID]
**		Desc:	Returns PerformingOrganizationID for those and In Use Flag
**			
**
**		Auth: Don Canuso
**		Date: 2/4/2013
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

IF @PerformingOrganizationID IS NULL
	RETURN


IF EXISTS (
	SELECT T.PerformingOrganizationID FROM  [dbo].[ODCType] T 
		INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID	
		INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
		INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		INNER JOIN dbo.WorkspacePerformingOrganization WR ON W.WorkspaceID = WR.WorkspaceID
	WHERE 
	T.PerformingOrganizationID = @PerformingOrganizationID AND 
	WR.SystemPerformingOrganizationID = @PerformingOrganizationID AND
	W.PerformingOrganizationListID = @PerformingOrganizationListID

	
UNION

	SELECT T.PerformingOrganizationID FROM  [dbo].[BOELaborType] T
		INNER JOIN dbo.BOETaskElement TE ON T.BOETaskElementID = TE.BOETaskElementID
		INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
		INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		INNER JOIN dbo.WorkspacePerformingOrganization WR ON W.WorkspaceID = WR.WorkspaceID
	WHERE 
	T.PerformingOrganizationID = @PerformingOrganizationID AND 
	WR.SystemPerformingOrganizationID = @PerformingOrganizationID AND
	W.PerformingOrganizationListID = @PerformingOrganizationListID



UNION
	SELECT T.PerformingOrganizationID FROM  [dbo].[TravelTrip] T
		INNER JOIN dbo.TravelTripTaskElement TE 
			ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
		INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
		INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
		INNER JOIN dbo.WorkspacePerformingOrganization WR ON W.WorkspaceID = WR.WorkspaceID
	WHERE 
	T.PerformingOrganizationID = @PerformingOrganizationID AND 
	WR.SystemPerformingOrganizationID = @PerformingOrganizationID AND
	W.PerformingOrganizationListID = @PerformingOrganizationListID

)
BEGIN
	SELECT 1 AS InUse
END	
ELSE
	BEGIN
		SELECT 0 AS InUse
	END

GO