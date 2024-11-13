EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.11';
GO

-- Dusan - PROPH-1559
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CcopdReasonsNo]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[CcopdReasonsNo] (
		Id INT PRIMARY KEY,
		Text VARCHAR(100) NOT NULL
	);

	INSERT INTO CcopdReasonsNo VALUES
		(1, 'Commercial Item Exception applies'),
		(2, 'Adequate Price Competition Exception applies'),
		(3, '< CCoPD Threshold Exception applies'),
		(4, 'Other Transaction Authority (OTA) (non-FAR based)'),
		(5, 'Other Exception applies (explain)');
END
GO

IF NOT EXISTS (
	SELECT * FROM sys.all_columns C
		INNER JOIN sys.tables T on C.object_id = T.object_id
		INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
	WHERE
		T.name = 'Proposal' AND
		C.name = 'ReasonCcopdNo' AND
		S.name = 'dbo'
)
BEGIN
	ALTER TABLE Proposal ADD ReasonCcopdNo INT REFERENCES dbo.CcopdReasonsNo (Id);
	ALTER TABLE Proposal ADD ReasonCcopdNoOther VARCHAR(100);
END
GO