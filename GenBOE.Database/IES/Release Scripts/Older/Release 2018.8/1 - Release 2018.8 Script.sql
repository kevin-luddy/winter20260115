EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.8';
GO

/*
       ## START ##

       09/18/2018	twilson3			BOEJ-3805 Add ExcludeFCCOM
*/
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ExcludeFCCOM' AND Object_ID = Object_ID(N'[dbo].[BurdenPoolLU]'))
BEGIN
	ALTER TABLE [dbo].[BurdenPoolLU]
	ADD [ExcludeFCCOM] BIT NOT NULL DEFAULT 0
END
GO

UPDATE [dbo].[BurdenPoolLU] SET [ExcludeFCCOM] = 1 WHERE [BurdenPool] LIKE '%-G'
GO
/*
       09/18/2018	twilson3			BOEJ-3805 Add ExcludeFCCOM

       ## END ##
*/