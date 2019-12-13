IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[restoreWorkspaceRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[restoreWorkspaceRate];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[restoreWorkspaceRate]
(
@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name: [restoreWorkspaceRate]
**		Desc: 
**				When a Workspace gets restored,
**				the Travel rates need to be restored to the 
**				current rates that the System Admins. are actively
**				using for all programs.
**				We copied that Trip and PerDiem when we executed the
**				lockWorkspace Rates stored procedure
**				So, we will restore the rates that were used
**				prior to the lock
**				
**				
**				
**			
**		
**
**		Auth: Don Canuso
**		Date: 7/13/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		9/2/11		dcanuso				WI 4881: [Travel Rates] Misc Rates 
**										updates change Trips while in Locked
**		1/4/12		dcanuso				Schema Changes to Labor and Workspace
**										Resource Rates
**		1/18/12		dcanuso				Changes to LRR/WRR/Locking
**		2/6/12		dcanuso				More changes to InUse Processing
**		4/27/12		DCANUSO				WI 8256 REDESIGN LOCKING
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDT datetime2 = GetDate()

/*
First Update the Travel Trip to the Originating Rates
*/
UPDATE dbo.TravelTrip
	SET TripLockedDT = NULL,
		UpdateDT = @UpdateDT
FROM dbo.Workspace WS
	INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
	INNER JOIN dbo.TravelTripTaskElement TE ON B.BOEID = TE.BOEID
	INNER JOIN dbo.TravelTrip TT ON TE.TravelTripTaskElementID = TT.TravelTripTaskElementID
	INNER JOIN dbo.Trip T ON TT.TripID = T.TripID
	INNER JOIN dbo.PerDiem PD ON T.PerDiemID = PD.PerDiemID
	INNER JOIN dbo.TravelMiscRate MR ON T.TravelMiscRateID = MR.TravelMiscRateID
WHERE WS.WorkspaceID = @WorkspaceID/* Removed AND 		TT.TripLockedDT IS NULL */
	

/*WI 8256 LOCKING TABLES*/			
DELETE FROM dbo.[WorkspaceLockedPerDiem] 
	FROM dbo.[WorkspaceLockedPerDiem] L
		INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID

DELETE FROM dbo.[WorkspaceLockedTravelEscalationRate]
	FROM dbo.[WorkspaceLockedTravelEscalationRate] L 			
		INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
	
DELETE FROM dbo.[WorkspaceLockedTravelMiscRate] 
	FROM dbo.[WorkspaceLockedTravelMiscRate] L 			
		INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
	
DELETE FROM dbo.[WorkspaceLockedTrip] 
	FROM dbo.[WorkspaceLockedTrip] L
		INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
	WHERE WS.WorkspaceID = @WorkspaceID
GO