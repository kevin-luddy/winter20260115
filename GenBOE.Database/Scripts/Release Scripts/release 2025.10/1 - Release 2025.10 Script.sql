EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.10';
GO

-- Author: ranzalon
-- JIRA Story: PROPH-3038
-- Contracts Tab Delegation of Authority Addition & Additional EPP Date Fields

IF NOT EXISTS (SELECT * FROM dbo.ProPricerFieldLU WHERE ProPricerFieldID IN (85, 86))
	INSERT INTO ProPricerFieldLU
		(ProPricerFieldID, ProPricerField, ProPricerTypeID, ProPricerCompanyID)
	VALUES
		(85, 'Task Author', 1, 0),
		(86, 'Task Author', 2, 0)
GO