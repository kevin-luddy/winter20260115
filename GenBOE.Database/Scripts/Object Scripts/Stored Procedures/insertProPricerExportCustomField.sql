IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertProPricerExportCustomField]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertProPricerExportCustomField];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertProPricerExportCustomField]
(
@ProPricerExportID int,
@CustomFieldID int,
@ProPricerTypeID int,
@ProPricerCustomFieldSelectionID int,
@ListOrder int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [insertProPricerExportCustomField]
**		Desc: Insert selected Tasks and Resources added to
**				the ProPricer Export - Custom Fields.
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
					SELECT 1 FROM dbo.ProPricerCustomFieldXREF
					WHERE
					ProPricerExportID = @ProPricerExportID AND
					CustomFieldID = @CustomFieldID AND
					ProPricerTypeID = @ProPricerTypeID AND
					ProPricerCustomFieldSelectionID = @ProPricerCustomFieldSelectionID
					)
		BEGIN
			INSERT INTO [dbo].[ProPricerCustomFieldXREF]
				   ([ProPricerExportID]
				   ,[CustomFieldID]
				   ,[ProPricerTypeID]
				   ,[ProPricerCustomFieldSelectionID]
				   ,[ListOrder])
			 VALUES
				   (@ProPricerExportID
				   ,@CustomFieldID
				   ,@ProPricerTypeID
				   ,@ProPricerCustomFieldSelectionID
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