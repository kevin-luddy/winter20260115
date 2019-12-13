IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteTMResourceRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteTMResourceRate];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteTMResourceRate]
(
@TMResourceRateID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteTMResourceRate]
**		Desc: Delete T&M Resource Rate 
**		
**
**		Auth: Greg Brunworth
**		Date: 5/03/17
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

IF (SELECT UpdateDT FROM [dbo].[TMResourceRate] WHERE TMResourceRateID = @TMResourceRateID ) = @UpdateDT
	BEGIN
		DELETE FROM dbo.TMResourceRate WHERE TMResourceRateID = @TMResourceRateID
	END
ELSE
	BEGIN
		DECLARE @ErrorMessage varchar (500)
		SET @ErrorMessage =   'The T&M Resource Rate with ID ' + CAST(@TMResourceRateID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
		RETURN

	END

GO