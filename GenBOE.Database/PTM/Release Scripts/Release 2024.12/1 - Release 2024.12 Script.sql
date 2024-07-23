EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.12';
GO

-- Dusan - PROPH-1560
IF NOT EXISTS (
	SELECT * FROM sys.all_columns C
		INNER JOIN sys.tables T on C.object_id = T.object_id
		INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
	WHERE
		T.name = 'ProposalChecklist' AND
		C.name = 'IncludeInternationalCosts' AND
		S.name = 'dbo'
)
BEGIN
	ALTER TABLE ProposalChecklist ADD IncludeInternationalCosts BIT;
END
GO