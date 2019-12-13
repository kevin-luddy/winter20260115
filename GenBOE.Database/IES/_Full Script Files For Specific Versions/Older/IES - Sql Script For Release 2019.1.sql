EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2019.1';
GO

/*
       ## START ##

       01/15/2019	ranzalon			BOEJ-3965 App Offline via IES Portal
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OfflineApplication]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[OfflineApplication] (
		[ApplicationName] varchar(10) NOT NULL PRIMARY KEY,
		[IsOffline] bit NOT NULL,
		[UpdateDate] datetime2(7) NOT NULL
	);

	INSERT INTO [dbo].[OfflineApplication] VALUES 
		('BOE RMS', 0, GETDATE()),
		('BOE SSC', 0, GETDATE()),
		('PTM', 0, GETDATE()),
		('RDM', 0, GETDATE()),
		('RDSB', 0, GETDATE()),
		('RPM', 0, GETDATE());
END
GO
/*
       01/15/2019	ranzalon			BOEJ-3965 App Offline via IES Portal

       ## END ##
*/