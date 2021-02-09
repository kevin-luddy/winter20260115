EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.4';
GO

/*
       ## START ##

       4/25/18       twilson3	BOEJ-3384 Parent Section for RDSB
*/
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ParentSection' AND Object_ID = Object_ID(N'[dbo].[RDSBDocumentInformation]'))
BEGIN
	ALTER TABLE [dbo].[RDSBDocumentInformation]
	ADD [ParentSection] VARCHAR(10) NULL;
END

GO
/*
       4/25/18       twilson3	BOEJ-3384 Parent Section for RDSB

       ## END ##
*/