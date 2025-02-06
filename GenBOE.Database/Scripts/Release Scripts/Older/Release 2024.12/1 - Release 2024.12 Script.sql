EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.12';

/*** Generate Insert for ProPricerFieldLU ***/
GO

IF NOT EXISTS (SELECT * FROM dbo.ProPricerFieldLU WHERE ProPricerFieldID IN (80, 83))
	INSERT INTO ProPricerFieldLU
		(ProPricerFieldID, ProPricerField, ProPricerTypeID, ProPricerCompanyID)
	VALUES
		(80, 'Task URL', 1, 3),
		(81, 'Task URL', 2, 3),
		(82, 'Workspace URL', 1, 3),
		(83, 'Workspace URL', 2, 3)
GO