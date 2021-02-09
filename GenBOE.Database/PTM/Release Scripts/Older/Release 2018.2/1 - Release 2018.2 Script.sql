/*
	## START ##
	
	1/24/2018 twilson3 BOEJ-2808 Add RDSB link into PTM
*/
	IF NOT EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'Proposal' AND
					C.name = 'DocumentId' AND
					S.name = 'dbo'
				)
BEGIN
	ALTER TABLE [dbo].[Proposal] ADD [DocumentId] int NULL
END
GO
/*
	1/24/2018 twilson3 BOEJ-2808 Add RDSB link into PTM

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

	2/14/2018 [pattoncr] - BOEJ-3017 - Allow more files uploaded
*/

IF NOT EXISTS(SELECT * FROM AttachmentTypeLU WHERE ID = 4)
BEGIN
	INSERT INTO AttachmentTypeLU (ID, Name) values (4, 'Other');
END

/*
	2/14/2018 [pattoncr] - BOEJ-3017 - Allow more files uploaded

	## END ##
*/