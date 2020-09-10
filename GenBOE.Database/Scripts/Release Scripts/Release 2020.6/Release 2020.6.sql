EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2020.6';
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

/*
	## START ##

	9/10/2020 [ranzalon] - BOEJ-4774 New MOQ types
*/

IF NOT EXISTS (SELECT * FROM [dbo].[MOQTypeLU] WHERE [MOQTypeID] >= 5001 AND [MOQTypeID] <= 5009)
BEGIN 

INSERT INTO [dbo].[MOQTypeLU] ([MOQTypeID],[MOQType]) VALUES (5001, 'Actual Program or Task Cost Data (Historical)'),
	(5002, 'Comparative Analysis'), 
	(5003, 'Cost Estimating Relationships (CERs) R2'), 
	(5004, 'Parametric Estimates'),
	(5005, 'Analogous Relationships (ARs)'),
	(5006, 'Statement of Work (SOW)'),
	(5007, 'Level of Effort (LOE)'),
	(5008, 'Subject Matter Expert (SME) Judgement'),
	(5009, 'Non-Labor');

END

/*
	9/10/2020 [ranzalon] - BOEJ-4774 New MOQ types

	## END ##
*/