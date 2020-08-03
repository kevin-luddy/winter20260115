EXEC dbo.[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2020.5';
GO

/*
	## START ##

	7/31/2020 [ranzalon] - BOEJ-4648 - Comments for Proposal Setup
*/

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE S.name = 'dbo' AND 
	T.name = 'Proposal' AND C.name = 'SetupComments')
BEGIN 

ALTER TABLE dbo.[Proposal]
ADD [SetupComments] VARCHAR(max) NULL;

END

/*
	7/31/2020 [ranzalon] - BOEJ-4648 - Comments for Proposal Setup

	## END ##
*/