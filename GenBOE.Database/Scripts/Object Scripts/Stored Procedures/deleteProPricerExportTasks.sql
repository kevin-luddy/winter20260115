IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProPricerExportTasks]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProPricerExportTasks];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteProPricerExportTasks]
(
@ProPricerExportID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteProPricerExportTasks]
**		Desc: Delete all of the selected Fields and Custom Fields that 
*				were added to the ProPricer Export - Tasks.
**
**		Auth: Don Canuso
**		Date: 4/27/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[ProPricerExport] WHERE ProPricerExportID = @ProPricerExportID) = @UpdateDT
		BEGIN
			
			DELETE FROM dbo.ProPricerFieldXREF
			WHERE 
				ProPricerExportID = @ProPricerExportID AND
				ProPricerTypeID = 1 /*Tasks*/
				
			DELETE FROM dbo.ProPricerCustomFieldXREF
			WHERE 
				ProPricerExportID = @ProPricerExportID AND
				ProPricerTypeID = 1 /*Tasks*/
				
			UPDATE dbo.ProPricerExport
			SET UpdateDT = GETDATE()
			WHERE ProPricerExportID = @ProPricerExportID
			
			

		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Pro Pricer Export with ID ' + CAST(@ProPricerExportID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO