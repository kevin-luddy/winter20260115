EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2021.1.1';
GO

/*
	## START ##

	12/8/2020 [Dusan] - BOEJ-4894: DB work for an extra CF type
*/

IF NOT EXISTS (SELECT 1 FROM CustomFieldDisplayLU WHERE CustomFieldDisplayID = 4)
BEGIN
	INSERT INTO CustomFieldDisplayLU VALUES (4, 'MOQ Type Table Data');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MoqTypeTableCustomFieldValueXREF]') AND type in (N'U'))
BEGIN
	CREATE TABLE dbo.MoqTypeTableCustomFieldValueXREF
	(
		Id					INT				PRIMARY KEY		IDENTITY(1, 1),
		UpdateDT			DATETIME2(7)	NOT NULL,
		MoqTypeTableDataId	INT				NOT NULL		REFERENCES MoqTypeSelectionTableData(MoqTypeSelectionTableDataId),
		CustomFieldValueId	INT				NOT NULL		REFERENCES CustomFieldValue(CustomFieldValueId)
	);
END
GO

/*
	12/8/2020 [Dusan] - BOEJ-4894: DB work for an extra CF type

	## END ##
*/
