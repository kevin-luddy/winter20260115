EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.30';

/*** Generate Insert for ProPricerFieldLU ***/
GO

INSERT INTO ProPricerFieldLU
	(ProPricerFieldID, ProPricerField, ProPricerTypeID, ProPricerCompanyID)
VALUES
	(76, 'Resource Segment/Region', 1, 0),
	(77, 'Resource Segment/Region', 2, 0)

GO