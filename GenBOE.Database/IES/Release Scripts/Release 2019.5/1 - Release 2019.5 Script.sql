EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2019.5';
GO

/*
		## START ##
		8/13/19		ranzalon			BOEJ-4314	New Burden Pools column 
*/

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IncludeGaT2InBurdAndCommBurdTables' AND Object_ID = Object_ID('[dbo].[BurdenPoolLU]'))
BEGIN
	ALTER TABLE [dbo].[BurdenPoolLU]
	ADD [IncludeGaT2InBurdAndCommBurdTables] bit not null DEFAULT(0)
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[BurdenPoolLU] WHERE [IncludeGaT2InBurdAndCommBurdTables] = 1)
BEGIN
	UPDATE [dbo].[BurdenPoolLU]
	SET [IncludeGaT2InBurdAndCommBurdTables] = 1
	WHERE [BurdenPool] like 'OHSERV%' AND [RevisionID] in (SELECT ID FROM [dbo].[Revision] WHERE [DatePublished] IS NULL)
END
GO

/*
       8/13/19		ranzalon			BOEJ-4314	New Burden Pools column
       ## END ##
*/