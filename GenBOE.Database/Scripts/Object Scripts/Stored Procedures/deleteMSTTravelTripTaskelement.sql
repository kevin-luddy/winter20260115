 IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMSTTravelTripTaskelement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMSTTravelTripTaskelement];
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteMSTTravelTripTaskelement]
(
@TravelTripTaskelementID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteMSTTravelTripTaskelement]
**		Desc: Delete Travel Trip Task Element in Travel Section of BOE and all sub-elements
**			  for MST travel BASED ON deleteTravelTripTaskelement
**		
**
**		Auth: Tom Glick 
**		Date: 10/5/16 
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/2/18		ranzalon			BOEJ-3268 - Update for Open Ended Custom Fields
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[TravelTripTaskelement] WHERE TravelTripTaskelementID = @TravelTripTaskelementID) = @UpdateDT
		BEGIN
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @MSTTripCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @MSTTripCustomFieldXrefs
			SELECT MSTCustomFieldValueID
			FROM dbo.MSTTravelTripCustomFieldValueXREF X
				INNER JOIN dbo.MSTTravelTrip T ON X.MSTTravelTripID = T.MSTTravelTripID
				INNER JOIN dbo.TravelTripTaskelement TE ON T.TravelTripTaskelementID = TE.TravelTripTaskelementID
			WHERE
				TE.TravelTripTaskelementID = @TravelTripTaskelementID

			DELETE FROM dbo.MSTTravelTripCustomFieldValueXREF
				FROM dbo.MSTTravelTripCustomFieldValueXREF X
				INNER JOIN dbo.MSTTravelTrip T ON X.MSTTravelTripID = T.MSTTravelTripID
				INNER JOIN dbo.TravelTripTaskelement TE ON T.TravelTripTaskelementID = TE.TravelTripTaskelementID
			WHERE
				TE.TravelTripTaskelementID = @TravelTripTaskelementID

			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @MSTTripCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)

			/*WI 6072*/
			DECLARE @TripAffected TABLE (TripID int)					
			INSERT INTO @TripAffected 
			SELECT T.MSTTravelTripID
			FROM dbo.MSTTravelTrip T
				INNER JOIN dbo.TravelTripTaskelement TE ON T.TravelTripTaskelementID = TE.TravelTripTaskelementID
			WHERE
				TE.TravelTripTaskelementID = @TravelTripTaskelementID
			
			/*Need Segment ID from Travel Trip to process Resource Flag*/
			DECLARE @Segment TABLE
				(
					SegmentID int
				)
			INSERT INTO @Segment 
			SELECT SegmentID
				FROM dbo.MSTTravelTrip T
				INNER JOIN dbo.TravelTripTaskelement TE ON T.TravelTripTaskelementID = TE.TravelTripTaskelementID
			WHERE
				TE.TravelTripTaskelementID = @TravelTripTaskelementID
	

			DELETE FROM dbo.MSTTravelTrip
				FROM dbo.MSTTravelTrip T
				INNER JOIN dbo.TravelTripTaskelement TE ON T.TravelTripTaskelementID = TE.TravelTripTaskelementID
			WHERE
				TE.TravelTripTaskelementID = @TravelTripTaskelementID

			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @TravelTaskCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @TravelTaskCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.TravelTripTaskElementCustomFieldValueXREF 
			WHERE TravelTripTaskelementID = @TravelTripTaskelementID

			DELETE FROM dbo.TravelTripTaskelementCustomFieldValueXREF				
				FROM dbo.TravelTripTaskelementCustomFieldValueXREF X
				INNER JOIN dbo.TravelTripTaskelement TE ON X.TravelTripTaskelementID = TE.TravelTripTaskelementID
			WHERE
				TE.TravelTripTaskelementID = @TravelTripTaskelementID

			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @TravelTaskCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)

			/*Used to Handle In Use Processing*/
			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID 
			FROM dbo.BOE B
				INNER JOIN dbo.TravelTripTaskelement TE ON B.BOEID = TE.BOEID
			WHERE TE.TravelTripTaskelementID = @TravelTripTaskelementID

			
			DELETE FROM dbo.TravelTripTaskelement
			WHERE
				TravelTripTaskelementID = @TravelTripTaskelementID	
			
		END
		
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Travel Trip Task Element with ID ' + CAST(@TravelTripTaskelementID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END


GO

