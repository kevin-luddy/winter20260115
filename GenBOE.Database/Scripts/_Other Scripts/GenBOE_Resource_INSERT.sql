/*** Generate Insert for ProPricerFieldLU ***/
USE genBOEMST
GO

INSERT INTO ProPricerFieldLU
	(ProPricerFieldID, ProPricerField, ProPricerTypeID, ProPricerCompanyID)
VALUES
	(76, 'Resource Segment/Region', 1, 2),
	(77, 'Resource Segment/Region', 1, 3),
	(78, 'Resource Segment/Region', 2, 2),
	(79, 'Resource Segment/Region', 2, 3)

GO

USE genBOESpace
GO

INSERT INTO ProPricerFieldLU
	(ProPricerFieldID, ProPricerField, ProPricerTypeID, ProPricerCompanyID)
VALUES
	(76, 'Resource Segment/Region', 1, 2),
	(77, 'Resource Segment/Region', 1, 3),
	(78, 'Resource Segment/Region', 2, 2),
	(79, 'Resource Segment/Region', 2, 3)

GO