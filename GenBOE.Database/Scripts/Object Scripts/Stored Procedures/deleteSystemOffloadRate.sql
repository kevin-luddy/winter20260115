IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSystemOffloadRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSystemOffloadRate];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteSystemOffloadRate]
(
@OffloadRateID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteSystemOffloadRate]
**		Desc: Delete System Offload Rate 
**			
**		
**
**		Auth: RJ Anzalone
**		Date: 4/26/17
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
*******************************************************************************/
SET NOCOUNT ON 

IF (SELECT UpdateDT FROM [dbo].[SystemOffloadRate] WHERE OffloadRateID = @OffloadRateID ) = @UpdateDT
	BEGIN
		DELETE FROM dbo.SystemOffloadRate WHERE OffloadRateID = @OffloadRateID
	END
ELSE
	BEGIN
		DECLARE @ErrorMessage varchar (500)
		SET @ErrorMessage =   'The System Offload Rate with ID ' + CAST(@OffloadRateID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
				@ErrorMessage, -- Message text.
		        11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)
		RETURN

	END

GO