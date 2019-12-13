IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateMSTZoneTravelDestination]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[UpdateMSTZoneTravelDestination];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[UpdateMSTZoneTravelDestination]
(
@DestinationID int,
@Zone int
)
AS
/******************************************************************************
**		 
**		Name: [deleteMSTZoneTravelDestination]
**		Desc: Update the zone of an MST Zone Travel Destination.
**
**		Auth: RJ Anzalone
**		Date: 8/16/16
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/16/16		ranzalon			SP created. Updates the zone for a 
**										given destination.
******************************************************************************/
SET NOCOUNT ON

UPDATE [dbo].[MSTZoneTravelDestination]
	SET [Zone] = @Zone
	WHERE [DestinationID] = @DestinationID;

GO