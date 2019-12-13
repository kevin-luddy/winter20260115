IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteCustomField]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteCustomField];

GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteCustomField]
(
@CustomFieldID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteCustomField]
**		Desc: Delete Custom Field and all subcomponents
**
**		Auth: Don Canuso
**		Date: 8/9/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/18/11		dcanuso				Adding Custom Field Tables for Cost Elements
**		4/4/18		ranzalon			BOEJ-3299 - Update for zone travel trip CFs
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
******************************************************************************/
SET NOCOUNT ON 


IF (SELECT UpdateDT FROM [dbo].[CustomField] WHERE CustomFieldID = @CustomFieldID) = @UpdateDT
BEGIN

DECLARE @Reorder TABLE
(
ProPricerExportID int,
ProPricerTypeID int,
ListOrder tinyint,
Processed bit DEFAULT 0
)
INSERT INTO @Reorder (ProPricerExportID, ProPricerTypeID, ListOrder)
SELECT ProPricerExportID, ProPricerTypeID, ListOrder 
FROM dbo.ProPricerCustomFieldXREF 
WHERE CustomFieldID = @CustomFieldID
	
			/*DELETE XREF REFENCES*/
			
			DELETE FROM dbo.BOECustomFieldValueXREF 
			FROM dbo.BOECustomFieldValueXREF X
				INNER JOIN dbo.CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID
				INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			WHERE
				CF.CustomFieldID = @CustomFieldID
				
				
			DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF 
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID
				INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			WHERE
				CF.CustomFieldID = @CustomFieldID
				
			DELETE FROM dbo.BOETaskElementCustomFieldValueXREF 
			FROM dbo.BOETaskElementCustomFieldValueXREF X
				INNER JOIN dbo.CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID
				INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			WHERE
				CF.CustomFieldID = @CustomFieldID
				
				
			DELETE FROM dbo.TravelTripCustomFieldValueXREF
				FROM dbo.TravelTripCustomFieldValueXREF X
				INNER JOIN dbo.CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID
				INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			WHERE
				CF.CustomFieldID = @CustomFieldID			
			

			DELETE FROM dbo.TravelTripTaskElementCustomFieldValueXREF				
				FROM dbo.TravelTripTaskElementCustomFieldValueXREF X
				INNER JOIN dbo.CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID
				INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			WHERE
				CF.CustomFieldID = @CustomFieldID

			DELETE FROM dbo.MSTTravelTripCustomFieldValueXREF
				FROM dbo.MSTTravelTripCustomFieldValueXREF X
				INNER JOIN dbo.CustomFieldValue CFV ON X.MSTCustomFieldValueID = CFV.CustomFieldValueID
				INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID 
			WHERE
				CF.CustomFieldID = @CustomFieldID

			/*DELETE FROM ProPricerCustomFieldXREF TABLE AS WELL*/
			DELETE FROM dbo.ProPricerCustomFieldXREF
			FROM dbo.ProPricerCustomFieldXREF  X
				INNER JOIN dbo.CustomField CF ON X.CustomFieldID = CF.CustomFieldID
			WHERE
				CF.CustomFieldID = @CustomFieldID

			/*DELETE ALL CUSTOM FIELD VALUES UNDER THE CUSTOM FIELD*/
			DELETE FROM dbo.CustomFieldValue 
			FROM dbo.CustomFieldValue CFV
				INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			WHERE
				CF.CustomFieldID = @CustomFieldID				
				
			/*DELETE THE CUSTOM FIELD*/
			DELETE FROM [dbo].[CustomField] WHERE CustomFieldID = @CustomFieldID


DECLARE @ListOrder int
WHILE EXISTS (SELECT 1 FROM @Reorder WHERE Processed = 0)
BEGIN
	SELECT	@ListOrder = MAX(ListOrder) FROM @Reorder WHERE Processed = 0

	UPDATE dbo.ProPricerCustomFieldXREF
	SET ListOrder = x.ListOrder - 1
	FROM dbo.ProPricerCustomFieldXREF X
		INNER JOIN @Reorder R ON
			X.ProPricerExportID = R.ProPricerExportID AND
			X.ProPricerTypeID = R.ProPricerTypeID
	WHERE
		X.ListOrder > @ListOrder
		

	UPDATE dbo.ProPricerFieldXREF
	SET ListOrder = x.ListOrder - 1
	FROM dbo.ProPricerFieldXREF X
		INNER JOIN @Reorder R ON
			X.ProPricerExportID = R.ProPricerExportID AND
			X.ProPricerTypeID = R.ProPricerTypeID
	WHERE
		X.ListOrder > @ListOrder


	UPDATE @Reorder SET Processed = 1 WHERE ListOrder = @ListOrder
	
END

		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =   'The CustomField with ID ' + CAST(@CustomFieldID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO