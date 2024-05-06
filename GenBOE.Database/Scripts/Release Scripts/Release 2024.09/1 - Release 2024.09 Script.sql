EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.09';
GO

/*
	## START ##

	5/1/2024 [ranzalon] - SLMX_POLM_PROPH-1794: Confidence Report
*/
IF NOT EXISTS (SELECT 1 FROM ReportLU WHERE ReportID = 40)
	BEGIN
		INSERT INTO ReportLU (ReportID, ReportName, Description) VALUES(40, 'Confidence Report', 'View the Confidence Report for BOE Calculations and PoP Dates');
	END
GO
/*
	5/1/2024 [ranzalon] - SLMX_POLM_PROPH-1794: Confidence Report

	## END ##
*/