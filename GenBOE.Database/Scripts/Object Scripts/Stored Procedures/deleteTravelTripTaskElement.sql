IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteTravelTripTaskElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteTravelTripTaskElement];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteTravelTripTaskElement]
(
@TravelTripTaskElementID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteTravelTripTaskElement]
**		Desc: Delete Travel Trip Task Element in Travel Section of BOE and all sub-elements
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
**		8/18/11		dcanuso				Adding Custom Field Tables
**		11/18/11	dcanuso				WI 6072:The DeleteTravelTripTaskElement SP
**										should check if the Trip's TripInUse flag can be reset. 
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
**		6/26/12		dcanuso				WI Escalation and Locking updated
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
**		8/27/14		dcanuso				Image Story
**		10/3/14		dcanuso				Image Story Removal
**      6/24/16     twilson3            Fix In-Use Flag for Custom Fields
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[TravelTripTaskElement] WHERE TravelTripTaskElementID = @TravelTripTaskElementID) = @UpdateDT
		BEGIN

			DELETE FROM dbo.TravelTripCustomFieldValueXREF
				FROM dbo.TravelTripCustomFieldValueXREF X
				INNER JOIN dbo.TravelTrip T ON X.TravelTripID = T.TravelTripID
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE
				TE.TravelTripTaskElementID = @TravelTripTaskElementID

			/*WI 6072*/
			DECLARE @TripAffected TABLE (TripID int)					
			INSERT INTO @TripAffected 
			SELECT TripID
			FROM dbo.TravelTrip T
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE
				TE.TravelTripTaskElementID = @TravelTripTaskElementID
			
			/*Need Segment ID from Travel Trip to process Resource Flag*/
			DECLARE @Segment TABLE
				(
					SegmentID int
				)
			INSERT INTO @Segment 
			SELECT SegmentID
				FROM dbo.TravelTrip T
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE
				TE.TravelTripTaskElementID = @TravelTripTaskElementID
/*			
			/*ReProcess In Use Flag for deleted Travel Trips*/ 
			DECLARE @PerformingOrganization TABLE
			(
				PerformingOrganizationID int,
				Processed bit
			)
			INSERT INTO @PerformingOrganization
			SELECT DISTINCT PerformingOrganizationID, 0
			FROM dbo.TravelTrip T
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE
				TE.TravelTripTaskElementID = @TravelTripTaskElementID
*/
			
			DELETE FROM dbo.TravelTrip
				FROM dbo.TravelTrip T
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE
				TE.TravelTripTaskElementID = @TravelTripTaskElementID



			DELETE FROM dbo.TravelTripTaskElementCustomFieldValueXREF				
				FROM dbo.TravelTripTaskElementCustomFieldValueXREF X
				INNER JOIN dbo.TravelTripTaskElement TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE
				TE.TravelTripTaskElementID = @TravelTripTaskElementID

			/*Used to Handle In Use Processing*/
			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID 
			FROM dbo.BOE B
				INNER JOIN dbo.TravelTripTaskElement TE ON B.BOEID = TE.BOEID
			WHERE TE.TravelTripTaskElementID = @TravelTripTaskElementID

			
			DELETE FROM dbo.TravelTripTaskElement
			WHERE
				TravelTripTaskElementID = @TravelTripTaskElementID	
			
			
		END
		
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Travel Trip Task Element with ID ' + CAST(@TravelTripTaskElementID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
GO