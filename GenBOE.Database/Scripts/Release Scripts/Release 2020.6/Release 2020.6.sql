EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2020.5';
GO

/*
	## START ##

	8/27/2020 [ranzalon] - BOEJ-4760 Template BOE 
*/

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE S.name = 'dbo' AND 
	T.name = 'Workspace' AND C.name = 'TemplateBoe')
BEGIN 

ALTER TABLE [dbo].[Workspace]
ADD [TemplateBoe] bit NOT NULL DEFAULT 0;

ALTER TABLE [version].[Workspace]
ADD [TemplateBoe] bit NOT NULL DEFAULT 0;

END

/*
	8/27/2020 [ranzalon] - BOEJ-4760 Template BOE  

	## END ##
*/