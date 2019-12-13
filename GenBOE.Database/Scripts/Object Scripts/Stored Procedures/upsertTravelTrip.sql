IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertTravelTrip]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertTravelTrip];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertTravelTrip]
(
@TravelTripID int,
@GroupID int,
@SegmentID int,
@PerformingOrganizationID int,
@TripID int,
@TripDate date,
@NumTrips int,
@NumPeople int,
@NumDays int,
@Purpose varchar(35),
@TravelTripTaskElementID int,
@UpdateDT datetime2(7)
)
AS
/******************************************************************************
**		 
**		Name: upsertTravelTrip
**		Desc: Insert/Update data into Trip Section of BOE/Travel
**			
**		
**
**		Auth: Don Canuso
**		Date: 7/11/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		7/22/11		dcanuso				Added Purpose
**		7/29/11		dcanuso				Wireframes:
**										Miscellaneous rates that are currently 
**										in use cannot be selected and In use
**										is displayed in place of the checkbox.  
**										A miscellaneous rate is considered in use
**										if any Workspace which is in any state other
**										than Complete or Closed has a trip selected 
**										that uses the miscellaneous rate.
**		8/9/11		dcanuso				Updated Business Rules from SE
**										TravelMiscRate.MiscRateInUse is now
**										handled by upsertTrip
**		11/22/11	dcanuso				InUse Updated
**		12/20/11	dcanuso				WI 6021 Mark resource in use if being 
**										used by a travel trip.
**										If a travel trip is created, 
**										then its matching resource 
**										(given segmentid and costelementid) should be 
**										marked as in use. 
**										Cost element Id will always be 6 (travel) 
**										also ensure by deleting the travel trip or 
**										the task element it belongs to that the 
**										in use flag goes back to false
**		1/10/12		dcanuso				WI 
**										6470: Trip In Use Fixes
**										https://eureka.isgs.lmco.com/#activity/151789
**										6624: After WI 6622, we now need to check that 
**										the originating Labor Resource Rate is in 
**										use as we did for trips.
**										6339: Confirm in-use is working for workspace
**										and labor resource rates
**		2/6/12		dcanuso				More changes to InUse Processing
**		5/22/12		dcanuso				Updated due to Locking Redesign
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		2/11/13		dcanuso				WI14842 Redesign Performing Organization
*******************************************************************************/
SET NOCOUNT ON 

DECLARE	@InsertedTravel AS Table (TravelID int)

IF @TravelTripID < 0  /*Insert Record*/

	BEGIN

	SET @UpdateDT = GetDate()
	
	INSERT INTO [dbo].[TravelTrip]
           ([UpdateDT]
           ,[GroupID]
           ,[SegmentID]
           ,[PerformingOrganizationID]
           ,[TripID]
           ,[TripDate]
           ,[NumTrips]
           ,[NumPeople]
           ,[NumDays]
           ,[Purpose]
           ,[TravelTripTaskElementID]
           /*Since we are using the SP and should
           only be used for "open" Travel not
           locked ones, making sure this is NULL
           */
           ,[TripLockedDT]
/*Column Removed during Locking Redesign            ,[OriginatingTripID]*/
           )
     OUTPUT inserted.TravelTripID INTO @InsertedTravel
     VALUES
           (@UpdateDT
           ,@GroupID
           ,@SegmentID
           ,@PerformingOrganizationID
           ,@TripID
           ,@TripDate
           ,@NumTrips
           ,@NumPeople
           ,@NumDays
           ,@Purpose
           ,@TravelTripTaskElementID
/*Column Removed during Locking Redesign            ,NULL*/
           ,NULL
           )


	SELECT 	@TravelTripID = TravelID FROM @InsertedTravel
	
	/*Update In Use Flag*/
	/*UPDATE [dbo].[PerformingOrganization] SET PerformingOrganizationInUseFlag = 1 WHERE PerformingOrganizationID = @PerformingOrganizationID*/
	
	UPDATE dbo.Trip 
		SET		TripInUse = 1,
				LastUsedDT = @UpdateDT
	WHERE TripID = @TripID
	
	/*
	Pass in a Table - Loop Through and Process All that Are not already  a 1
	*/
	/*
	DECLARE @Resource TABLE
		(
			ResourceID int,
			Processed bit
		)
	INSERT INTO @Resource */
		/* Special Processing for Travel */
		/*SELECT R.ResourceID, 0
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
		R.SegmentID = TT.SegmentID AND
		R.CostElementID = 6 /*Travel*/  AND
		TT.TravelTripID = @TravelTripID AND
		(
			R.ResourceInUseFlag = 0 OR /*If it is 1, there is no reason to set it again*/
			R.ResourceInUseFlag IS NULL
		)
	
	DECLARE @CurrentResourceID int	
	WHILE EXISTS (SELECT 1 FROM @Resource WHERE Processed = 0)
		BEGIN
			SELECT @CurrentResourceID = ResourceID FROM @Resource WHERE Processed = 0
			EXECUTE [dbo].[updateResourceInUseFlagByResourceID] @CurrentResourceID
			UPDATE @Resource SET Processed = 1 WHERE ResourceID = @CurrentResourceID 			 
		END		*/
END

ELSE
	BEGIN
			IF (SELECT UpdateDT FROM [dbo].[TravelTrip] WHERE TravelTripID = @TravelTripID) = @UpdateDT
			BEGIN	
			 
			/*Variables Used for InUse Function*/
			DECLARE	@CurrentPerformingOrganizationID int,
					@CurrentTripID int,
					@CurrentSegmentID int,
					@WorkspaceID int
					
		
			SELECT	
				@CurrentPerformingOrganizationID  = TT.PerformingOrganizationID,
				@CurrentTripID = TT.TripID,
				@CurrentSegmentID = TT.SegmentID,
				@WorkspaceID = B.WorkspaceID
			FROM [dbo].[TravelTrip] TT
				INNER JOIN [dbo].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
			WHERE TravelTripID = @TravelTripID
			
			SET @UpdateDT = GetDate()
			
			UPDATE [dbo].[TravelTrip]
			   SET [UpdateDT] = @UpdateDT
				  ,[GroupID] = @GroupID
				  ,[SegmentID] = @SegmentID
				  ,[PerformingOrganizationID] = @PerformingOrganizationID
				  ,[TripID] = @TripID
				  ,[TripDate] = @TripDate
				  ,[NumTrips] = @NumTrips
				  ,[NumPeople] = @NumPeople
				  ,[NumDays] = @NumDays
				  ,[Purpose] = @Purpose
				  ,[TravelTripTaskElementID] = @TravelTripTaskElementID
				  ,[TripLockedDT] = NULL
/*Column Removed during Locking Redesign 				  ,[OriginatingTripID] = NULL*/
			WHERE 
				TravelTripID = @TravelTripID
				
				/*Update In Use Flag*/
/*				UPDATE [dbo].[PerformingOrganization] SET PerformingOrganizationInUseFlag = 1 WHERE PerformingOrganizationID = @PerformingOrganizationID*/
				
				UPDATE dbo.Trip 
					SET		TripInUse = 1,
							LastUsedDT = @UpdateDT
				WHERE TripID = @TripID


				/*EXECUTE [dbo].[updateResourceInUseFlagByWorkspaceID] @WorkspaceID							*/

/*				EXECUTE [dbo].[updatePerformingOrganizationInUseFlagByPerformingOrganizationID] @CurrentPerformingOrganizationID*/
								
								
					
					
				IF IsNull(@CurrentTripID, -99)  <> @TripID
					BEGIN
						/*Check if TripID is used elsewhere*/
						UPDATE dbo.Trip 
						SET TripInUse  =
								CASE	
									WHEN TT.TripID IS NULL THEN 0
									/*
									The Trip was In Use
									I only want to change it to not in 
									use if this was the last Travel Trip
									Using the Trip
									If not, I want to leave it alone
									
									WHEN TT.TripID IS NOT NULL THEN 1
									*/
								END,
							UpdateDT = @UpdateDT
						FROM  dbo.Trip T							 
							LEFT OUTER JOIN 
								(
									SELECT TripID FROM [dbo].[TravelTrip] WHERE TripID = @CurrentTripID
									/*Column Removed during Locking Redesign 
									UNION
									SELECT IsNull(OriginatingTripID, -99) FROM [dbo].[TravelTrip] WHERE OriginatingTripID = @CurrentTripID
									*/
								)  TT ON T.TripID = TT.TripID
						WHERE	T.TripID = @CurrentTripID AND
								/*
									The Trip was In Use
									I only want to change it to not in 
									use if this was the last Travel Trip
									Using the Trip
									If not, I want to leave it alone
								*/
								T.TripInUse = 1 AND
								TT.TripID IS NULL
						
					END	
					

					
		END
			ELSE
			BEGIN
				DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Travel Trip with ID ' + CAST(@TravelTripID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
				RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
				RETURN
			END
	END

IF @@ERROR = 0
	SELECT @TravelTripID AS TravelTripID
GO