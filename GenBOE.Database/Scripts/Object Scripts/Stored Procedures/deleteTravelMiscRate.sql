IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteTravelMiscRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteTravelMiscRate];

GO

SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteTravelMiscRate]
(
@TravelMiscRateID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteTravelMiscRate]
**		Desc: Delete Travel Misc Rate 
**			
**		
**
**		Auth: Don Canuso
**		Date: 6/21/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		7/29/11		dcanuso				Only allow deletions if In Use Flag is False
*******************************************************************************/
SET NOCOUNT ON 


IF (SELECT UpdateDT FROM [dbo].[TravelMiscRate] WHERE TravelMiscRateID = @TravelMiscRateID ) = @UpdateDT
	BEGIN
		DELETE FROM dbo.TravelMiscRate WHERE TravelMiscRateID = @TravelMiscRateID AND MiscRateInUse = 0
	END
ELSE
	BEGIN
		DECLARE @ErrorMessage varchar (500)
		SET @ErrorMessage =   'The Travel Misc Rate with ID ' + CAST(@TravelMiscRateID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
		RETURN

	END

GO