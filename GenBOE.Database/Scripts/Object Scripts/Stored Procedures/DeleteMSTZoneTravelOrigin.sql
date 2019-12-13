IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMSTZoneTravelOrigin]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMSTZoneTravelOrigin];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteMSTZoneTravelOrigin]
(
	@OriginID int
)
AS
/******************************************************************************
**		 
**		Name: [deleteMSTZoneTravelOrigin]
**		Desc: Delete an MST Zone Travel Origin and its Resources when not 
**		no longer needed.
**
**		Auth: RJ Anzalone
**		Date: 8/16/16
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		2/1/18		pattoncr			Do not delete the Origin if it is in use.
**		8/16/16		ranzalon			SP created. If an origin is deleted, 
**										it is removed, along with all 
**										associated resources
******************************************************************************/
SET NOCOUNT ON

	-- Only process the deletion if the Origin is not currently in use.  If it is in use, warn the user.
	IF (NOT EXISTS(SELECT 1 FROM MSTTravelTrip WHERE ZoneResourceID in (SELECT ResourceID FROM [dbo].[MSTZoneTravelResource] WHERE OriginID = @OriginID )))
	BEGIN
		DELETE FROM [dbo].[MSTZoneTravelResource]
		WHERE [OriginID] = @OriginID;

		DELETE FROM [dbo].[MSTZoneTravelOrigin]
		WHERE [OriginID] = @OriginID;
	END
	ELSE
	BEGIN
		DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =   'The Origin with ID ' + CAST(@OriginID  AS varchar(10)) + ' is currently in use and cannot be deleted.  To delete, all existing usages must first be removed.'
		RAISERROR (
				@ErrorMessage, -- Message text.
				11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
		RETURN
	END

GO