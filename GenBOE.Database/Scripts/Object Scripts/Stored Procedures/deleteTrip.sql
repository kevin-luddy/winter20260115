IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteTrip]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteTrip];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteTrip]
(
@TripID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteTrip]
**		Desc: Delete a Trip when not in use.
**
**		Auth: Don Canuso
**		Date: 6/24/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		7/6/11		dcanuso				https://eureka.isgs.lmco.com/#groups/etiboe?activityId=88593
**										Trips - If you delete a trip, should per diem data be deleted too
**										or will that always remain in our system?
**										The Per Diem data should be deleted if it is no longer referenced
**										by any trip.
**		8/3/11		dcanuso				Remove InUSe Flag check
**		8/5/11		dcanuso				Added back InUSe Flag check-pushed to future iteration
**		8/9/11		dcanuso				Travel Misc Rates In Use being handled here
******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[Trip] WHERE TripID = @TripID) = @UpdateDT
		BEGIN
			DECLARE @PerDiemID int
			SELECT @PerDiemID = PerDiemID FROM dbo.Trip WHERE TripID = @TripID
			
			DECLARE @TravelMiscRateID int
			SELECT @TravelMiscRateID = TravelMiscRateID FROM dbo.Trip WHERE TripID = @TripID
			
			
			DELETE FROM dbo.Trip WHERE TripID = @TripID AND TripInUse = 0
			
			DELETE FROM dbo.PerDiem 
				FROM dbo.PerDiem PD
					LEFT OUTER JOIN dbo.Trip T ON PD.PerDiemID = T.PerDiemID
			 WHERE 
				T.PerDiemID IS NULL AND PD.PerDiemID = @PerDiemID 
				
				
				/*
				Check if the travel rate is being used
				
				If I am deleting a trip, I should only have to update the Flag to 0 when it is the last one
				*/
				
				
			IF NOT EXISTS (SELECT TravelMiscRateID FROM dbo.Trip WHERE TravelMiscRateID = @TravelMiscRateID)
				BEGIN
				
				UPDATE dbo.TravelMiscRate
					SET MiscRateInUse = 0,
						UpdateDT = GETDATE()
				FROM dbo.TravelMiscRate TR
				WHERE TR.TravelMiscRateID = @TravelMiscRateID 
				
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

GO