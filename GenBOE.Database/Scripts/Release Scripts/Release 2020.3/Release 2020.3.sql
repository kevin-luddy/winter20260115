EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2020.3';
GO

/*
	## START ##

	6/3/2020 [ranzalon] - BOEJ-4607 - BOE/WBS Report
*/

IF NOT EXISTS (SELECT * FROM [dbo].[ReportLU] WHERE [ReportID] = 38)
BEGIN
	INSERT INTO [dbo].[ReportLU] ([ReportID],[ReportName],[Description]) VALUES (38, 'WBS Summary of Hours, ODC Costs', 'Export the WBS, Total Hours, and Total Cost for each BOE including WBS Title')
END
GO

/*
	6/3/2020 [ranzalon] - BOEJ-4607 - BOE/WBS Report

	## END ##
*/