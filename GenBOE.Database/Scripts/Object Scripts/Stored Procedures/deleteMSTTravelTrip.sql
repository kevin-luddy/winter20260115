IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMSTTravelTrip]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMSTTravelTrip];
GO

CREATE PROCEDURE [dbo].[deleteMSTTravelTrip]
(
@MSTTravelTripID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteMSTTravelTrip]
**		Desc: Delete Trip in MST Travel Section
**
**		Auth: Tom Glick
**		Date: 9/9/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/2/18		ranzalon			BOEJ-3268 - Update for Open Ended Custom Fields
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
*******************************************************************************/
SET NOCOUNT ON 
	IF (SELECT UpdateDT FROM [dbo].[MSTTravelTrip] WHERE MSTTravelTripID = @MSTTravelTripID ) = @UpdateDT
		BEGIN
			/*Variables Used for InUse Function*/
			DECLARE	@CurrentPerformingOrganizationID int,
					@CurrentSegmentID int
			SELECT	
				@CurrentPerformingOrganizationID  = PerformingOrganizationID,
				@CurrentSegmentID = SegmentID
			FROM [dbo].[MSTTravelTrip]
			WHERE MSTTravelTripID = @MSTTravelTripID

			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @MSTTripCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @MSTTripCustomFieldXrefs
			SELECT MSTCustomFieldValueID
			FROM dbo.MSTTravelTripCustomFieldValueXREF 
			WHERE MSTTravelTripID = @MSTTravelTripID

			DELETE FROM dbo.MSTTravelTripCustomFieldValueXREF
				FROM dbo.MSTTravelTripCustomFieldValueXREF X
				INNER JOIN dbo.MSTTravelTrip T ON X.MSTTravelTripID = T.MSTTravelTripID
			WHERE
				T.MSTTravelTripID = @MSTTravelTripID
			
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

			/*Used to Handle In Use Processing*/
			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID 
			FROM dbo.BOE B
				INNER JOIN dbo.TravelTripTaskElement TE ON B.BOEID = TE.BOEID
				INNER JOIN dbo.MSTTravelTrip T ON TE.TravelTripTaskElementID = T.TravelTripTaskElementID
			WHERE
				T.MSTTravelTripID = @MSTTravelTripID

			DELETE FROM dbo.MSTTravelTrip
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


GO

