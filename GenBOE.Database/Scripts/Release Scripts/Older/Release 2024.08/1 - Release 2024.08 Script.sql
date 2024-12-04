EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.08';

/*** Generate Insert for ProPricerFieldLU ***/
GO

IF NOT EXISTS (SELECT * FROM dbo.ProPricerFieldLU WHERE ProPricerFieldID IN (78, 79))
	INSERT INTO ProPricerFieldLU
		(ProPricerFieldID, ProPricerField, ProPricerTypeID, ProPricerCompanyID)
	VALUES
		(78, 'Resource Segment/Region', 1, 4),
		(79, 'Resource Segment/Region', 2, 4)

GO