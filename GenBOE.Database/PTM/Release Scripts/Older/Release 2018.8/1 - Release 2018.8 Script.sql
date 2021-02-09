EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.8';
GO

/*
		## START ##
		9/25/18 RJ - BOEJ-3739: Classified Cost Volume
*/

IF NOT EXISTS (
	SELECT * FROM sys.all_columns C
		INNER JOIN sys.tables T on C.object_id = T.object_id
		INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
	WHERE
		T.name = 'Proposal' AND
		C.name = 'CostVolumeClassified' AND
		S.name = 'dbo'
)
BEGIN

ALTER TABLE [dbo].[Proposal]
ADD [CostVolumeClassified] bit null

END
GO

/*
		9/25/18 RJ - BOEJ-3739: Classified Cost Volume
		## END ##
*/