IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateTravelTripInUse]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateTravelTripInUse];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[updateTravelTripInUse]
AS
/******************************************************************************
**		 
**		Name: [updateTravelTripInUse]
**		Desc: Update Travel Trip In Use flag according to Requirements
**			
**	
**
**		Auth: Don Canuso
**		Date: 7/9/12
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
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
**		8/9/12		dcanuso				Rules better defined based on conversation with SE:
**										If No One is currently using a Trip, 
**										set Last Used Date to NULL
**										If In Use but no one has used it in the last 2 years, 
**										mark In Use as 0 (Not in Use)
**										If No One is using Trip, In Use is 0
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDT datetime2(7) = GetDate()

/*
If No One is currently using a Trip, 
set Last Used Date to NULL
AND
If No One is using Trip, In Use is 0
*/

UPDATE dbo.Trip
	SET LastUsedDT = NULL,

		TripInUse =
					CASE 
						WHEN TT.TripID IS NULL THEN 0
					END,
					
		UpdateDT = @UpdateDT
FROM dbo.Trip T  
	LEFT OUTER JOIN dbo.TravelTrip TT ON T.TripID = TT.TripID
WHERE 
	TT.TripID IS NULL



/*
If In Use but no one has used it in the last 2 years, 
mark In Use as 0 (Not in Use)
*/
UPDATE dbo.Trip
	SET TripInUse =
					CASE 
						/*Not being used and > 2 years last used*/					
						/*
						0 = Can Be Deleted - it is not being used currently
						and the last time it was being used, was 2 years ago 
						*/
						WHEN	T.LastUsedDT IS NOT NULL AND
								T.LastUsedDT <= DateAdd(YYYY, -2, @UpdateDT)
						THEN 0 
						ELSE T.TripInUse /*No Matches - Leave Alone - Failsafe */						
					END,
		UpdateDT = @UpdateDT
FROM dbo.Trip T  
	INNER JOIN dbo.TravelTrip TT ON T.TripID = TT.TripID
GO