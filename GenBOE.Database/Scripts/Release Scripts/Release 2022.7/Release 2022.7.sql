EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2022.7';
GO

/*
	## START ##

	2/7/2022 [Jesse] - IES-707: Update genBOE to Export separate MS Word Files
*/
IF NOT EXISTS (SELECT 1 FROM ReportLU WHERE ReportID = 39)
	BEGIN
		INSERT INTO ReportLU (ReportID, ReportName, Description) VALUES(39, 'All BOEs (Multiple Files)', 'Export all BOEs to the {0} Output Format in zip format.');
	END
GO
/*
	2/7/2022 [Jesse] - IES-707: Update genBOE to Export separate MS Word Files

	## END ##
*/