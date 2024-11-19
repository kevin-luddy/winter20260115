EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.10';
GO

-- Dusan - PROPH-1563
IF NOT EXISTS (
	SELECT * FROM sys.all_columns C
		INNER JOIN sys.tables T on C.object_id = T.object_id
		INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
	WHERE
		T.name = 'Proposal' AND
		C.name = 'AdditionalClassification' AND
		S.name = 'dbo'
)
BEGIN
	ALTER TABLE Proposal ADD AdditionalClassification BIT;
END
GO