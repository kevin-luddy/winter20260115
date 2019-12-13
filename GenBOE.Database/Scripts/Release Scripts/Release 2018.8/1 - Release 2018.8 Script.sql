EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.8';
GO

/*
		## START ##
		10/2/18 RJ - BOEJ-3699: RTE Size Limit
*/

IF NOT EXISTS (
	SELECT * FROM sys.all_columns C
		INNER JOIN sys.tables T on C.object_id = T.object_id
		INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
	WHERE
		T.name = 'Workspace' AND
		C.name = 'RteSizeLimit' AND
		S.name = 'dbo'
)
BEGIN

ALTER TABLE [dbo].[Workspace]
ADD [RteSizeLimit] int null;

ALTER TABLE [version].[Workspace]
ADD [RteSizeLimit] int null;

END
GO

/*
		10/2/18 RJ - BOEJ-3699: RTE Size Limit
		## END ##
*/