IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteTravelTrip]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteTravelTrip];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteTravelTrip]
(
@TravelTripID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteTravelTrip]
**		Desc: Delete Trip in Travel Section
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
**		7/29/11		dcanuso				Wireframes:
**										Miscellaneous rates that are currently 
**										in use cannot be selected and In use
**										is displayed in place of the checkbox.  
**										A miscellaneous rate is considered in use
**										if any Workspace which is in any state other
**										than Complete or Closed has a trip selected 
**										that uses the miscellaneous rate.
**		8/9/11		dcanuso				Wireframe/Business Rule Update
**										Misc. Rate In Use Flag
**										no longer handles by Travel Trip but now
**										handles in Trip
**		8/18/11		dcanuso				Adding Custom Field Tables
**		11/18/11	dcanuso				WI 6072 Fix InUse of Trip
**		11/25/11	dcanuso				InUse Updated
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
**		7/9/12		dcanuso				WI 9739
**										Description:
**										Able to delete a trip within the two year 
**										of the last time it's been used. 
**										According to the wireframe: Do not allow 
**										a trip to be deleted unless it has been 2 years 
**										after the "Last Used" date. Display "In Use" in 
**										place of the Delete checkbox 
**										for the trips that are in use until 2 years 
**										after the "Last Used" date.
**		8/7/12		dcanuso				WI 10278: Resource Redesign 
**										In Use will no longer be stored in DB
**		2/12/13		dcanuso				WI14842 Redesign Performing Organization
**      6/24/16     twilson3            Fix In-Use Flag for Custom Fields
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[TravelTrip] WHERE TravelTripID = @TravelTripID ) = @UpdateDT
		BEGIN
		

			/*Variables Used for InUse Function*/
			DECLARE	@CurrentPerformingOrganizationID int,
					@CurrentTripID int,
					@CurrentSegmentID int
					
		
			
			SELECT	
				@CurrentPerformingOrganizationID  = PerformingOrganizationID,
				@CurrentTripID = TripID,
				@CurrentSegmentID = SegmentID
			FROM [dbo].[TravelTrip]
			WHERE TravelTripID = @TravelTripID


			DELETE FROM dbo.TravelTripCustomFieldValueXREF
				FROM dbo.TravelTripCustomFieldValueXREF X
				INNER JOIN dbo.TravelTrip T ON X.TravelTripID = T.TravelTripID
			WHERE
				T.TravelTripID = @TravelTripID
					

			/*Used to Handle In Use Processing*/
			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID 
			FROM dbo.BOE B
				INNER JOIN dbo.TravelTripTaskElement TE ON B.BOEID = TE.BOEID
				INNER JOIN dbo.TravelTrip T ON TE.TravelTripTaskElementID = T.TravelTripTaskElementID
			WHERE
				T.TravelTripID = @TravelTripID
			
			

			DELETE FROM dbo.TravelTrip
			WHERE
				TravelTripID = @TravelTripID
	
			/*Update In Use Flag*/
/*			EXECUTE [dbo].[updatePerformingOrganizationInUseFlagByPerformingOrganizationID] @CurrentPerformingOrganizationID*/
					
			/*WI 9739		
			/*WI 6072*/
			/*Only want to Update the date if the row changes*/
			IF @CurrentTripID IS NOT NULL
				BEGIN	
						
					DECLARE @TripInUse bit

					IF EXISTS 
						(
							SELECT TripID FROM dbo.TravelTrip WHERE TripID = @CurrentTripID
							/* Column removed during Locking Redesign
							UNION
							SELECT OriginatingTripID FROM dbo.TravelTrip WHERE OriginatingTripID = @CurrentTripID
							*/
						)	
					/*	IF EXISTS (SELECT T.TripID FROM dbo.Trip T	INNER JOIN dbo.TravelTrip TT ON T.TripID = TT.TripID)*/
						BEGIN						
							SET @TripInUse = 1
						END
					ELSE
						BEGIN						
							SET @TripInUse = 0
						END
					
					
					UPDATE dbo.Trip
						SET TripInUse = @TripInUse,
							UpdateDT =@UpdateDT
					WHERE
						TripInUse <> @TripInUse AND
						TripID = @CurrentTripID

					END
			*/
	
			
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

GO