IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertMSTTravelTrip]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertMSTTravelTrip];
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[upsertMSTTravelTrip]
(
	@MSTTravelTripID int,
	@ModeID int,
	@TravelTripTaskElementID int, 
	@UpdateDT datetime2(7), 
	@GroupID int, 
	@SegmentID int, 
	@Purpose varchar(35), 
	@PerformingOrganizationID int, 
	@TripDate date, 
	@EstimateDate date, 
	@NumPeople decimal(10,6), 
	@NumDays decimal(10,6), 
	@ZoneOriginID int, 
	@ZoneDestCity varchar(35), 
	@ZoneDestinationID int, 
	@ZoneResourceID int, 
	@NonZoneFrom varchar(150), 
	@NonZoneTo varchar(150), 
	@NonZoneAirFareEstimate money, 
	@NonZonePerDiemDaily money, 
	@NonZoneCarRentalTrans money, 
	@NonZoneNumCars decimal(10,6) ,
	@NonZoneResourceID int,
	@ClinId int,
	@WbsId int
)
AS
/****************************************************************************
**		Name: upsertMSTTravelTrip
**		Desc: Insert/Update data into Trip Section of BOE/Travel for MST 
**
**		Auth: Tom Glick
**		Date: 9/9/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		10/5/2016	tglick				modified for schema changes to msttraveltrip
**		11/1/2016	Dusan				Modified Num of People, Days and Cars to Decimal datatype
**		11/2/2016   tglick				Added non-zone resource id
**      11/7/16		Dusan				BOEJ-1518 Multi Clin/Wbs for RMS Zone Travel
*******************************************************************************/
SET NOCOUNT ON 

DECLARE	@InsertedMSTTravel AS Table (MSTTravelID int)

IF @MSTTravelTripID < 0  /*Insert Record*/
	BEGIN
	SET @UpdateDT = GetDate()
	INSERT INTO [dbo].[MSTTravelTrip]	
	( 
		[ModeID]
		,[TravelTripTaskElementID] 
		,[UpdateDT] 
		,[GroupID] 
		,[SegmentID] 
		,[Purpose] 
		,[PerformingOrganizationID] 
		,[TripDate] 
		,[EstimateDate] 
		,[NumPeople] 
		,[NumDays] 
		,[ZoneOriginID] 
		,[ZoneDestCity] 
		,[ZoneDestinationID] 
		,[ZoneResourceID] 
		,[NonZoneFrom] 
		,[NonZoneTo] 
		,[NonZoneAirFareEstimate] 
		,[NonZonePerDiemDaily] 
		,[NonZoneCarRentalTrans] 
		,[NonZoneNumCars] 
		,[NonZoneResourceID]
		,[ClinId]
		,[WbsId]
	) 
	OUTPUT inserted.MSTTravelTripID INTO @InsertedMSTTravel
	VALUES
	(
		 @ModeID
		,@TravelTripTaskElementID  
		,@UpdateDT  
		,@GroupID   
		,@SegmentID 
		,@Purpose  
		,@PerformingOrganizationID 
		,@TripDate 
		,@EstimateDate  
		,@NumPeople  
		,@NumDays 
		,@ZoneOriginID  
		,@ZoneDestCity  
		,@ZoneDestinationID
		,@ZoneResourceID
		,@NonZoneFrom
		,@NonZoneTo
		,@NonZoneAirFareEstimate
		,@NonZonePerDiemDaily
		,@NonZoneCarRentalTrans
		,@NonZoneNumCars
		,@NonZoneResourceID
		,@ClinId
		,@WbsId
	)
	SELECT 	@MSTTravelTripID = MSTTravelID FROM @InsertedMSTTravel
	END
	ELSE
		BEGIN
			IF (SELECT UpdateDT FROM [dbo].[MSTTravelTrip] WHERE MSTTravelTripID = @MSTTravelTripID) = @UpdateDT
			BEGIN	
			
			SET @UpdateDT = GetDate()
		
			UPDATE [dbo].[MSTTravelTrip]
			   SET 
				 [ModeID] = @ModeID
				,[TravelTripTaskElementID] = @TravelTripTaskElementID
				,[UpdateDT] = @UpdateDT
				,[GroupID]  = @GroupID
				,[SegmentID] = @SegmentID
				,[Purpose] = @Purpose
				,[PerformingOrganizationID]  = @PerformingOrganizationID
				,[TripDate] = @TripDate
				,[EstimateDate] = @EstimateDate
				,[NumPeople] = @NumPeople
				,[NumDays]  = @NumDays
				,[ZoneOriginID] = @ZoneOriginID
				,[ZoneDestCity] = @ZoneDestCity
				,[ZoneDestinationID] = @ZoneDestinationID
				,[ZoneResourceID] = @ZoneResourceID
				,[NonZoneFrom] = @NonZoneFrom
				,[NonZoneTo] = @NonZoneTo
				,[NonZoneAirFareEstimate] = @NonZoneAirFareEstimate
				,[NonZonePerDiemDaily] = @NonZonePerDiemDaily
				,[NonZoneCarRentalTrans] = @NonZoneCarRentalTrans
				,[NonZoneNumCars] = @NonZoneNumCars
				,[NonZoneResourceID] = @NonZoneResourceID
				,[ClinId] = @ClinId
				,[WbsId] = @WbsId
			WHERE 
				MSTTravelTripID = @MSTTravelTripID
		END
		ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =   'The Travel Trip with ID ' + CAST(@MSTTravelTripID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
			RETURN
		END
	END
IF @@ERROR = 0
	SELECT @MSTTravelTripID AS MSTTravelTripID


GO