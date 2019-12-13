IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertProPricerExportField]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertProPricerExportField];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertProPricerExportField]
(
@ProPricerExportID int,
@ProPricerFieldID int,
@ProPricerTypeID int,
@ListOrder int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [insertProPricerExportField]
**		Desc: Insert selected Tasks and Resources added to
**				the ProPricer Export.
**			
**		
**
**		Auth: Don Canuso
**		Date: 4/27/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
*******************************************************************************/
SET NOCOUNT ON 


IF (SELECT UpdateDT FROM dbo.ProPricerExport WHERE ProPricerExportID = @ProPricerExportID) = @UpdateDT
BEGIN
	IF NOT EXISTS	(
					SELECT 1 FROM dbo.ProPricerFieldXREF
					WHERE
					ProPricerExportID = @ProPricerExportID AND
						(
							ProPricerFieldID = @ProPricerFieldID AND 
							@ProPricerFieldID <> 10 /*BLANK*/ AND
							@ProPricerFieldID <> 21 /*BLANK*/  
						) AND
					ProPricerTypeID = @ProPricerTypeID 
					)
		BEGIN
			INSERT INTO [dbo].[ProPricerFieldXREF]
				   ([ProPricerExportID]
				   ,[ProPricerFieldID]
				   ,[ProPricerTypeID]
				   ,[ListOrder])
			 VALUES
				   (@ProPricerExportID
				   ,@ProPricerFieldID
				   ,@ProPricerTypeID
				   ,@ListOrder)
		END
END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =    'The Pro Pricer Export with ID ' + CAST(@ProPricerExportID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END
GO