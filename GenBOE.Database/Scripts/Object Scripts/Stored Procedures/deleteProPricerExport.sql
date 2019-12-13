IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProPricerExport]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProPricerExport];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteProPricerExport]
(
@ProPricerExportID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteProPricerExport]
**		Desc: Delete ProPricer Export and all of its references
**
**		Auth: Don Canuso
**		Date: 5/3/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			--------------------------------------
**		8/15/11		dcanuso				WI 4635 - System ProPricer Exports can
**										be deleted
**										ProPricer Exports can be deleted by
**										System Admins.
**										Removing WHERE in DELETE FROM 
**										dbo.ProPricerExport WHERE WorkspaceID
**										IS NOT NULL because system ones are NULL
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


			DELETE FROM dbo.ProPricerFieldXREF
			WHERE 
				ProPricerExportID = @ProPricerExportID AND
				ProPricerTypeID = 2 /*Resources*/
				
			DELETE FROM dbo.ProPricerCustomFieldXREF
			WHERE 
				ProPricerExportID = @ProPricerExportID AND
				ProPricerTypeID = 2 /*Resources*/


			DELETE FROM dbo.ProPricerExport
			WHERE 
				ProPricerExportID = @ProPricerExportID 
			
			

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