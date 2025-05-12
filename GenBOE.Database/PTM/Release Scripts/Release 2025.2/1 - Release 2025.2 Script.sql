EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.2';
GO

-- Author: twilson3
-- JIRA Story: PROPH-2429
-- Added MOQ Type to Resource output for ProPricer Export

IF NOT EXISTS (SELECT * FROM dbo.ProPricerFieldLU WHERE ProPricerFieldID IN (84))
	INSERT INTO ProPricerFieldLU
		(ProPricerFieldID, ProPricerField, ProPricerTypeID, ProPricerCompanyID)
	VALUES
		(84, 'MOQ Type', 2, 2)
GO