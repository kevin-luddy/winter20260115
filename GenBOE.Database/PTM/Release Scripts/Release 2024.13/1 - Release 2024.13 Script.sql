EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.13';
GO

-- Dusan - PROPH-2080
IF NOT EXISTS (
	SELECT * FROM sys.all_columns C
		INNER JOIN sys.tables T on C.object_id = T.object_id
		INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
	WHERE
		T.name = 'Proposal' AND
		C.name = 'IsSupportDefinitizingUCA' AND
		S.name = 'dbo'
)
BEGIN
	ALTER TABLE Proposal ADD IsSupportDefinitizingUCA BIT;
END
GO