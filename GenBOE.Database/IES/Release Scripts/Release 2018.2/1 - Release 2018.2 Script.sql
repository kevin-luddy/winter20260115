/*
	## START ##
	1/25/18 ranzalon		BOEJ-2715 RDSB Document Information
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RDSBDocumentInformation]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[RDSBDocumentInformation] (
		ID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		PTMProposalID int  NOT NULL UNIQUE,
		CreatedBy varchar(1000)  NOT NULL,
		CreatedDate DATETIME2(7)  NOT NULL,
		LastUpdateDT DATETIME2(7)  NOT NULL,
		RDMRevisionID int NOT NULL FOREIGN KEY REFERENCES [dbo].[Revision](ID)
	)
	ON [PRIMARY];
END
GO

/*
	1/25/18 ranzalon		BOEJ-2715 RDSB Document Information
	## END ##
*/

/*
	## START ##

	2/1/2018 [Dusan] - Adding a table that will track our DB updates/versioning (mostly to be used by UAT and such)
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BoeDatabaseVersion]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[BoeDatabaseVersion] (
		Id				INT					IDENTITY(1,1)		PRIMARY KEY,
		DBVersion		VARCHAR(100)		NOT NULL,
		AppVersion		VARCHAR(100)		NOT NULL,
		UpdateDate		DATETIME			NOT NULL
	);

END

GO

-- Due to the order in which the files get combined, we need to put this SP in here.. It does have a separate file, 
-- including all of the comments in there, this is a quick and dirty version of it

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateDbVersion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[UpdateDbVersion];

GO

CREATE PROCEDURE [dbo].[UpdateDbVersion](@DbVersion VARCHAR(100), @AppVersion VARCHAR(100)) 
AS
	IF(NOT EXISTS(SELECT 1 FROM [dbo].[BoeDatabaseVersion] WHERE DBVersion = @DbVersion AND AppVersion = @AppVersion))
	BEGIN
		INSERT INTO [dbo].[BoeDatabaseVersion] (DBVersion, AppVersion, UpdateDate)
		VALUES (@DbVersion, @AppVersion, GETDATE())
	END

GO

EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.2';
GO

/*
	2/1/2018 [Dusan] - Adding a table that will track our DB updates/versioning (mostly to be used by UAT and such)

	## END ##
*/
/*
	## START ##
	2/1/18	brunworg		BOEJ-2820 Back End - Document Settings - DB
*/

IF NOT EXISTS (
	SELECT * FROM sys.all_columns C
		INNER JOIN sys.tables T on C.object_id = T.object_id
		INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
	WHERE
		T.name = 'RDSBDocumentInformation' AND
		C.name = 'StartYear' AND
		S.name = 'dbo'
)
BEGIN
	ALTER TABLE [dbo].[RDSBDocumentInformation] ADD [StartYear] [int] NULL;
END
GO

IF NOT EXISTS (
	SELECT * FROM sys.all_columns C
		INNER JOIN sys.tables T on C.object_id = T.object_id
		INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
	WHERE
		T.name = 'RDSBDocumentInformation' AND
		C.name = 'EndYear' AND
		S.name = 'dbo'
)
BEGIN
	ALTER TABLE [dbo].[RDSBDocumentInformation] ADD [EndYear] [int] NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RDSBRateCodeXref]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[RDSBRateCodeXref] (
		ID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		UpdateDate [datetime2](7) NOT NULL,
		RDSBDocumentInformationID int NOT NULL,
		RateCodeID int NOT NULL
	)
	ON [PRIMARY]

	ALTER TABLE [dbo].[RDSBRateCodeXref]  WITH CHECK ADD  CONSTRAINT [FK_RDSBRateCodeXref_RateCode] FOREIGN KEY([RateCodeID])
	REFERENCES [dbo].[RateCode] ([ID])

	ALTER TABLE [dbo].[RDSBRateCodeXref]  WITH CHECK ADD  CONSTRAINT [FK_RDSBRateCodeXref_RDSBDocumentInformation] FOREIGN KEY([RDSBDocumentInformationID])
	REFERENCES [dbo].[RDSBDocumentInformation] ([ID])
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RDSBSectionXref]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[RDSBSectionXref] (
		ID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		UpdateDate [datetime2](7) NOT NULL,
		RDSBDocumentInformationID int NOT NULL,
		SectionID int NOT NULL
	)
	ON [PRIMARY]

	ALTER TABLE [dbo].[RDSBSectionXref]  WITH CHECK ADD  CONSTRAINT [FK_RDSBSectionXref_RateCode] FOREIGN KEY([SectionID])
	REFERENCES [dbo].[Section] ([ID])

	ALTER TABLE [dbo].[RDSBSectionXref]  WITH CHECK ADD  CONSTRAINT [FK_RDSBSectionXref_RDSBDocumentInformation] FOREIGN KEY([RDSBDocumentInformationID])
	REFERENCES [dbo].[RDSBDocumentInformation] ([ID])
END
GO

/*
	2/1/18	brunworg		BOEJ-2820 Back End - Document Settings - DB
	## END ##
*/