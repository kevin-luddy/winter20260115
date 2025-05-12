EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.08';
GO

-- 04/24/2025	e405721, updateDateShiftviaTableParameter SP	PROPH-2826: Rearchitect Dateshift

-- Author: twilson3
-- JIRA Story: PROPH-2429
-- Added MOQ Type to Resource output for ProPricer Export

IF NOT EXISTS (SELECT * FROM dbo.ProPricerFieldLU WHERE ProPricerFieldID IN (84))
	INSERT INTO ProPricerFieldLU
		(ProPricerFieldID, ProPricerField, ProPricerTypeID, ProPricerCompanyID)
	VALUES
		(84, 'MOQ Type', 2, 2)
GO