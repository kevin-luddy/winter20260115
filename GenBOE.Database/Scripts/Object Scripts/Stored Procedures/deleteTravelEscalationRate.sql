IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteTravelEscalationRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteTravelEscalationRate];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteTravelEscalationRate]
(
@TravelEscalationRateID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteTravelEscalationRate]
**		Desc: Delete Travel Escalation Rate 
**			
**		
**
**		Auth: Don Canuso
**		Date: 6/22/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 


IF (SELECT UpdateDT FROM [dbo].[TravelEscalationRate] WHERE TravelEscalationRateID = @TravelEscalationRateID ) = @UpdateDT
	BEGIN
		DELETE FROM dbo.TravelEscalationRate WHERE TravelEscalationRateID = @TravelEscalationRateID
	END
ELSE
	BEGIN
		DECLARE @ErrorMessage varchar (500)
		SET @ErrorMessage =   'The Travel Escalation Rate with ID ' + CAST(@TravelEscalationRateID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
		RETURN

	END

GO