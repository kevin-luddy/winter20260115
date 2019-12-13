IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertSystemProPricerExportField]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertSystemProPricerExportField];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertSystemProPricerExportField]
(
@SystemProPricerExportID int,
@ProPricerFieldID int,
@ProPricerTypeID int,
@ListOrder int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [insertSystemProPricerExportField]
**		Desc: Insert selected Tasks and Resources added to
**				the System ProPricer Export.
**			
**		
**
**		Auth: Timothy I. Wilson
**		Date: 7/15/2019
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			--------------------------------------
**		7/12/19		twilson3			BOEJ-4037	System Pro Pricer Export 
*******************************************************************************/
SET NOCOUNT ON 


IF (SELECT UpdateDT FROM dbo.SystemProPricerExport WHERE SystemProPricerExportID = @SystemProPricerExportID) = @UpdateDT
BEGIN
	IF NOT EXISTS	(
					SELECT 1 FROM dbo.SystemProPricerFieldXREF
					WHERE
					SystemProPricerExportID = @SystemProPricerExportID AND
						(
							ProPricerFieldID = @ProPricerFieldID AND 
							@ProPricerFieldID <> 10 /*BLANK*/ AND
							@ProPricerFieldID <> 21 /*BLANK*/  
						) AND
					ProPricerTypeID = @ProPricerTypeID 
					)
		BEGIN
			INSERT INTO [dbo].[SystemProPricerFieldXREF]
				   ([SystemProPricerExportID]
				   ,[ProPricerFieldID]
				   ,[ProPricerTypeID]
				   ,[ListOrder])
			 VALUES
				   (@SystemProPricerExportID
				   ,@ProPricerFieldID
				   ,@ProPricerTypeID
				   ,@ListOrder)
		END
END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =    'The System Pro Pricer Export with ID ' + CAST(@SystemProPricerExportID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
GO