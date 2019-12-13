IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[lockWorkspaceRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[lockWorkspaceRate];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[lockWorkspaceRate]
(
@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name: [lockWorkspaceRate]
**		Desc: 
**				When a Workspace goes into a locked, complete, or closed state,
**				the Travel rates that we used need to be locked
**				So, we will create a snapshot (copy) of the current rate
**				And link all of the BOEs to that new rate
**				We also need to capture the originating Trip ID (and Per Diem ID)
**				(the one that is currently being used) so that 
**				the System Admin. can continue to edit that Trip
**				without affecting the rates for the locked Workspace 
**				
**				Should only be executed the first time it goes into one of the three states
**				Should not be executed when moving from one of the three states to
**				another of the same three states
**				
**				Additionl Information:
**				Does the SA Trip get locked too?
**				No, System Administrators must be able to continue to modify the trips without
**				affecting the locked workspaces. System Admin trips are never locked.
**				
**				
**			
**		
**
**		Auth: Don Canuso
**		Date: 7/12/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		7/23/11		dcanuso				Purpose moved
**		8/26/11		dcanuso				WI: 4846:
**										Disable updates to per diem rates and 
**										trips that have their ratelocked bit 
**										set to true.	
**		8/30/11		dcanuso				WI 4846 OBE - In code - Removed in SP
**		9/2/11		dcanuso				WI 4881: [Travel Rates] Misc Rates 
**										updates change Trips while in Locked
**		9/9/11		dcanuso				Trip.Year, DevEscalation, and LMSI Escalation
**										Added
**		9/16/11		dcanuso				WI 5163 Change Escalation Rate to 5 decimal
**		10/24/11	dcanuso				TravelTrip.TripLockedDT changed
**										from datetime2 to date - CAST Implemented
**		11/28/11	dcanuso				Rental Car Rate moving from PerDiem to Trip
**		1/4/12		dcanuso				Handle Workspace Resource Rates
**		1/18/12		dcanuso				Changes to LRR/WRR/Locking
**		1/23/2012	dcanuso				Update "LockWorkspaceRate" to lock/copy all 
**										of the system (labor) resource rates instead
**										of just those referenced by workspace resource 
**										rates when a workspace is locked.
**		2/6/12		dcanuso				More changes to InUse Processing
**		2/10/12		dcanuso				WI7259 Remove code just added
**		4/27/12		DCANUSO				WI 8256 REDESIGN LOCKING
**		11/12/12	dcanuso				SW SE - Can lock the whole LRR when locking
**										WS - Make a copy of the table
**		12/10/2016	Dusan				Adding Misc Rate into the SP
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDT datetime2 = GetDate()

/*
Get the current Trip and Per Diem
Will need to be updated with the 
copied (locked) Trip and PerDiem
And now, with new WI, Travel Misc Rate
*/


/* Copy of Years go into dbo.WorkspaceLockedTravelEscalationRate*/
INSERT INTO [dbo].[WorkspaceLockedTravelEscalationRate]
           ([TravelEscalationRateID]
           ,[UpdateDT]
           ,[Year]
           ,[DevEscalation]
           ,[LMSIEscalation]
		   ,[MiscRate]
           ,[WorkspaceID])
SELECT [TravelEscalationRateID]
      ,[UpdateDT]
      ,[Year]
      ,[DevEscalation]
      ,[LMSIEscalation]
	  ,[MiscRate]
      ,@WorkspaceID
  FROM [dbo].[TravelEscalationRate]



DECLARE @LockBOE TABLE
(
	TravelTripID int PRIMARY KEY,
	TripID int,
	PerDiemID int,
	TravelMiscRateID int,	
	Processed bit DEFAULT 0
)	
INSERT INTO @LockBOE 
	(TravelTripID, TripID, PerDiemID, TravelMiscRateID)
SELECT
	TT.TravelTripID,
	T.TripID,
	PD.PerDiemID,
	MR.TravelMiscRateID
FROM dbo.Workspace WS
	INNER JOIN dbo.BOE B ON WS.WorkspaceID = B.WorkspaceID
	INNER JOIN dbo.TravelTripTaskElement TE ON B.BOEID = TE.BOEID
	INNER JOIN dbo.TravelTrip TT ON TE.TravelTripTaskElementID = TT.TravelTripTaskElementID
	INNER JOIN dbo.Trip T ON TT.TripID = T.TripID
	INNER JOIN dbo.PerDiem PD ON T.PerDiemID = PD.PerDiemID
	INNER JOIN dbo.TravelMiscRate MR ON T.TravelMiscRateID = MR.TravelMiscRateID
WHERE WS.WorkspaceID = @WorkspaceID/* Removed AND 		TT.TripLockedDT IS NULL */


INSERT INTO [dbo].[WorkspaceLockedTrip]
           ([TripID]
           ,[UpdateDT]
           ,[TravelMiscRateID]
           ,[DepartureLocationID]
           ,[DestinationLocationID]
           ,[PerDiemID]
           ,[TransportationFare]
           ,[RoundTripMiles]
           ,[FareLastUpdateETIUserID]
           ,[FareLastUpdateDT]
           ,[TripInUse]
           ,[LastUsedDT]
           ,[RentalCarRate]
           ,[DepartureLocationCode]
           ,[DestinationLocationCode]
           ,[WorkspaceID])
SELECT DISTINCT 
	   T.[TripID]
      ,T.[UpdateDT]
      ,T.[TravelMiscRateID]
      ,T.[DepartureLocationID]
      ,T.[DestinationLocationID]
      ,T.[PerDiemID]
      ,T.[TransportationFare]
      ,T.[RoundTripMiles]
      ,T.[FareLastUpdateETIUserID]
      ,T.[FareLastUpdateDT]
      ,T.[TripInUse]
      ,T.[LastUsedDT]
      ,T.[RentalCarRate]
      ,T.[DepartureLocationCode]
      ,T.[DestinationLocationCode]
      ,@WorkspaceID
  FROM [dbo].[Trip] T
	INNER JOIN @LockBOE B ON T.TripID = B.TripID




INSERT INTO [dbo].[WorkspaceLockedPerDiem]
           ([PerDiemID]
           ,[UpdateDT]
           ,[PerDiemDestination]
           ,[Qualification]
           ,[HotelRate]
           ,[MIERate]
           ,[PerDiemNotes]
           ,[PerDiemLastUpdateETIUserID]
           ,[PerDiemLastUpdateDT]
           ,[WorkspaceID])
SELECT DISTINCT  P.[PerDiemID]
      ,P.[UpdateDT]
      ,P.[PerDiemDestination]
      ,P.[Qualification]
      ,P.[HotelRate]
      ,P.[MIERate]
      ,P.[PerDiemNotes]
      ,P.[PerDiemLastUpdateETIUserID]
      ,P.[PerDiemLastUpdateDT]
      ,@WorkspaceID
  FROM [dbo].[PerDiem] P
	INNER JOIN @LockBOE B ON P.PerDiemID = B.PerDiemID




INSERT INTO [dbo].[WorkspaceLockedTravelMiscRate]
           ([TravelMiscRateID]
           ,[UpdateDT]
           ,[TransportationMode]
           ,[MiscellaneousRate]
           ,[SortCode]
           ,[MiscRateInUse]
           ,[WorkspaceID])
SELECT DISTINCT T.[TravelMiscRateID]
      ,T.[UpdateDT]
      ,T.[TransportationMode]
      ,T.[MiscellaneousRate]
      ,T.[SortCode]
      ,T.[MiscRateInUse]
      ,@WorkspaceID
  FROM [dbo].[TravelMiscRate] T
	INNER JOIN @LockBOE B ON T.TravelMiscRateID = B.TravelMiscRateID

/*
Now, I have my copies
I need to Lock the Rates
*/
UPDATE dbo.TravelTrip
	SET TripLockedDT = CAST(@UpdateDT AS date)
FROM dbo.TravelTrip TT
	INNER JOIN @LockBOE L ON TT.TravelTripID = L.TravelTripID

GO