IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertTrip]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertTrip];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertTrip]
(
@TripID int,
@TravelMiscRateID int,
@DepartureLocationID int,
@DestinationLocationID int,
@PerDiemID int,
@TransportationFare decimal(8, 2),
@RoundTripMiles int,
@UpdatedByETIUserID int,
@UpdateDT datetime2(7),
@RentalCarRate decimal (6,2),
@DestinationLocationCode varchar(10),
@DepartureLocationCode varchar(10)
)
AS
/******************************************************************************
**		 
**		Name: upsertTrip
**		Desc: Insert/Update data into Manage Travel
**			
**				NOTES: The inserts into Location may need to get changed
**				to their own upsert SP that get called from this SP.
**				
**				For Per Diem, question is what happens when you change a rate
**				I assume there is an update to the current rates.
**		
**
**		Auth: Don Canuso
**		Date: 6/24/2011
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		7/18/11		dcanuso				Trip.Purpose added 
**		7/19/11		dcanuso				WI 4203:Update the upsertTrip SP to only
**										mark Trip updated when Trip data changes
**		7/20/11		DCANUSO				PULLED OUT LOCATION AND PER DIEM
**		7/22/11		dcanuso				Purpose removed from Trip
**		8/2/11		dcanuso				WI 4354: DB - System Admin - Add Trip - 
**										Remove "Code" fields
**		8/9/11		dcanuso				Business Rule Changes:
**										https://eureka.isgs.lmco.com/#groups/etiboe?activityId=101439
**										When is a Misc Rate in use?
**										1. When a trip is In Use (current way we are doing it).
**										2. When a Sys Admin creates a trip that uses the rate.
**										Answer:
**										When a Sys Admin creates a trip that uses the rate.
**		10/21/11	dcanuso				Trip.FareLastUpdateDT changed from datetime2 to date
**		11/28/11	dcanuso				Adding RentalCarRate decimal (6,2) to Trip
**		12/13/11	dcanuso				WI 6166 
**		12/19/11	dcanuso				WI 6261:
**										ADD:
**										[version].[Trip].[LocationCode]
**										[dbo].[Trip].[LocationCode]
**										(DestinationLocationCode and DepartureLocationCode)
**										DROP:
**										[version].[Location].[LocationCode]
**										[dbo].[Location].[LocationCode]
*******************************************************************************/
SET NOCOUNT ON 

DECLARE	@Inserted AS Table (ID int)
/*
Stored procedure uses GetDate and update date serveral times
and will overwrite the @UpdateDT so 
using a new one
*/
DECLARE @UpdateDate datetime2 = GetDate() 

IF @TripID < 0  /*Insert Record*/
	BEGIN

		INSERT INTO [dbo].[Trip]
           ([UpdateDT]
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
           ,[DestinationLocationCode]
           ,[DepartureLocationCode]
			)
		OUTPUT inserted.TripID INTO @Inserted
			VALUES
				   (@UpdateDate--@UpdateDT
				   ,@TravelMiscRateID
				   ,@DepartureLocationID
				   ,@DestinationLocationID
				   ,@PerDiemID
				   ,@TransportationFare
				   ,@RoundTripMiles
				   ,@UpdatedByETIUserID
				   ,CAST(@UpdateDate AS DATE) /*[FareLastUpdateDT]: On an insert the Last Update Date is the current date*/
				   ,0 /*When inserting a new record TripInUse is false*/
				   ,NULL /*When inserting a new record LastUsedDT is NULL, that is not used yet*/
				   ,@RentalCarRate				   
				   ,@DestinationLocationCode
				   ,@DepartureLocationCode
				   )

			SELECT 	@TripID = [ID] FROM @Inserted
			
			/*When a system trip is created, set the In Use flag to 1 for the Travel Rate that the System Trip is using*/
			UPDATE dbo.TravelMiscRate
			SET MiscRateInUse = 1,
				UpdateDT = @UpdateDate
			WHERE
				TravelMiscRateID = @TravelMiscRateID
	END

ELSE
	BEGIN
			IF (SELECT UpdateDT FROM [dbo].[Trip] WHERE TripID = @TripID) = @UpdateDT
				BEGIN	
				
				
				/*
				Setting Per Diem Last Update By and Dates
	
				Fare Last Updated By
				This field is output only and shows who made the last change, including when it was created, to any of the following data:
				Code – departure Loc 
				Destination
				Code – Destination
				Fare
				R/T Miles
				
				Fare Last Updated Date
				This field is output only and shows the date of the last change, including when it was created, made to any of the following data:
				Code – departure Loc 
				Destination
				Code – Destination
				Fare
				R/T Miles
 		
 				Issue here is, you can not change just the Departure Code Location Code.(Edit)
 				In the Add A trip, you would change IDs, so that is what I am using to determine a change
 				
 				
 				NOTE: 8/3/11: Location Code is removed - WI 4354
		        */
 
				
				/*Variable Used for InUse Function*/
				DECLARE	@CurrentTravelMiscRateID int
				SELECT 	@CurrentTravelMiscRateID =  T.TravelMiscRateID
				FROM dbo.Trip T WHERE TripID = @TripID


				UPDATE [dbo].[Trip]
				   SET [UpdateDT] = @UpdateDate--@UpdateDT
					  ,[TravelMiscRateID] = @TravelMiscRateID
					  ,[DepartureLocationID] = @DepartureLocationID
					  ,[DestinationLocationID] = @DestinationLocationID
					  ,[PerDiemID] = @PerDiemID
					  ,[TransportationFare] = @TransportationFare
					  ,[RoundTripMiles] = @RoundTripMiles
					  ,[RentalCarRate] = @RentalCarRate	
					  ,[DestinationLocationCode] = @DestinationLocationCode
					  ,[DepartureLocationCode] = @DepartureLocationCode
					  ,[FareLastUpdateETIUserID] = 
							CASE WHEN	(
								/*Changed in Trip Redesign:
								Any of the following data:
								    Fare-Fare for the transportation (air, train, bus, ship, …) in US Dollars
								    Rental Car
								    R/T Miles
								[DepartureLocationID] <> @DepartureLocationID OR
								[DestinationLocationID] <> @DestinationLocationID OR 
								*/
								[TransportationFare] <> @TransportationFare OR
								[RoundTripMiles] <> @RoundTripMiles
										) THEN @UpdatedByETIUserID
							ELSE
								[FareLastUpdateETIUserID]
							END
					  ,[FareLastUpdateDT] = 					
							CASE WHEN	(
								/*Changed in Trip Redesign:
								Any of the following data:
								    Fare-Fare for the transportation (air, train, bus, ship, …) in US Dollars
								    Rental Car
								    R/T Miles
								[DepartureLocationID] <> @DepartureLocationID OR
								[DestinationLocationID] <> @DestinationLocationID OR 
								*/
								[TransportationFare] <> @TransportationFare OR
								[RoundTripMiles] <> @RoundTripMiles
										) THEN CAST (@UpdateDate AS DATE)
							ELSE
								[FareLastUpdateDT]
							END
				WHERE 
					TripID = @TripID  
					
					
					
				IF @CurrentTravelMiscRateID <> @TravelMiscRateID
				BEGIN
									
					/*
						For the Travel Mis Rate, we only want to 
						to Update the date and the flag
						if the row changes, that is, if the Trip 
						changes the Travel Rate they were using
						
						So, check the travel rates, if they are different, 
						check the new travel rate, if it is not in use
						make it in use
					*/

					UPDATE dbo.TravelMiscRate
						SET MiscRateInUse = 1,
							UpdateDT = @UpdateDT
					FROM dbo.TravelMiscRate
					WHERE
						TravelMiscRateID = @TravelMiscRateID AND
						MiscRateInUse = 0 /*Only want to update In Use and the date if there is an update*/
						
						
					/*
						Now, check the original travel rate that was being used
						if it is not used anywhere else, set the flag to false
					*/
					IF NOT EXISTS (SELECT TravelMiscRateID FROM dbo.Trip WHERE TravelMiscRateID = @CurrentTravelMiscRateID)
						BEGIN 
							UPDATE dbo.TravelMiscRate 
							SET MiscRateInUse = 0,
								UpdateDT = @UpdateDate
							FROM dbo.TravelMiscRate TR
							WHERE
							TR.TravelMiscRateID = @CurrentTravelMiscRateID AND
							MiscRateInUse <> 0
						END
					
				END
		END

	
			ELSE
				BEGIN
					DECLARE @ErrorMessage varchar (500)
					SET @ErrorMessage =   'The Trip with ID ' + CAST(@TripID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
						@ErrorMessage, -- Message text.
						11, -- Severity,/*Severity Changed to 11*/
						1 -- State,
						)
					RETURN
				END


END

IF @@ERROR = 0
	SELECT @TripID AS TripID
GO