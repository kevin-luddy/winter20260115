IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteAllTravelTripTaskElements]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteAllTravelTripTaskElements];
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[deleteAllTravelTripTaskElements]
(
@BoeId int
)
AS
/******************************************************************************
**		 
**		Name: [deleteAllTravelTripTaskElements]
**		Desc: Deletes All Travel Tasks and Trips for a BOE
**			
**		
**
**		Auth: brunworg
**		Date: 1/2018
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*****************************************************************************/
DECLARE @TravelTripCursor CURSOR, @TravelTripTaskElementID int, @UpdateDT datetime2(7);
SET NOCOUNT ON 
	BEGIN
		SET @TravelTripCursor = CURSOR FOR
			SELECT [TravelTripTaskElementID],[UpdateDT]
			FROM [dbo].[TravelTripTaskElement]
			WHERE BOEID = @BoeId;

		OPEN @TravelTripCursor
		FETCH NEXT FROM @TravelTripCursor
		INTO @TravelTripTaskElementID, @UpdateDT;

		WHILE @@FETCH_STATUS = 0
		BEGIN

			EXEC [dbo].[deleteTravelTripTaskElement] @TravelTripTaskElementID, @UpdateDT

			FETCH NEXT FROM @TravelTripCursor
			INTO @TravelTripTaskElementID, @UpdateDT;

		END
	END
GO