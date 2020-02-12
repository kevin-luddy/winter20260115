EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2019.8';
GO

/*
		## START ##
		12/2/19		ranzalon			BOEJ-4464 - Added LaborSortId
*/

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE S.name = 'dbo' AND 
	T.name = 'BOELaborType' AND C.name = 'LaborSortId')
BEGIN
       ALTER TABLE [dbo].BOELaborType ADD LaborSortId int;
	   EXEC('Update [dbo].BOELaborType Set LaborSortId = 1');
	   ALTER TABLE [dbo].BOELaborType ALTER COLUMN LaborSortId int NOT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE S.name = 'version' AND 
	T.name = 'BOELaborType' AND C.name = 'LaborSortId')
BEGIN
       ALTER TABLE [version].BOELaborType ADD LaborSortId int NULL;
	   EXEC('Update [version].BOELaborType Set LaborSortId = 1');
	   ALTER TABLE [version].BOELaborType ALTER COLUMN LaborSortId int NOT NULL;
END
GO

/*
       12/2/19		ranzalon			BOEJ-4464 - Added LaborSortId
       ## END ##
*/

/*
		## START ##
		9/26/19		ranzalon			BOEJ-4349	Revised Submittal Date added to WS 
*/

IF NOT EXISTS (
	SELECT * FROM sys.all_columns C
		INNER JOIN sys.tables T on C.object_id = T.object_id
		INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
	WHERE
		T.name = 'Workspace' AND
		C.name = 'RevisedSubmittalDate' AND
		S.name = 'dbo'
)
BEGIN
	ALTER TABLE [dbo].[Workspace] ADD [RevisedSubmittalDate] DateTime2(7) null;
	ALTER TABLE [version].[Workspace] ADD [RevisedSubmittalDate] DateTime2(7) null;
END
GO

/*
       9/26/19		ranzalon			BOEJ-4349	Revised Submittal Date added to WS
       ## END ##
*/

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE S.name = 'dbo' AND 
	T.name = 'ETIuser' AND C.name = 'IsUsPerson')
BEGIN
    ALTER TABLE [dbo].ETIuser ADD IsUsPerson bit; 
	ALTER TABLE [dbo].ETIuser ADD IsSubcontractor bit;
END
GO

/*
		## START ##
		1/22/20		Dusan			Temp fix because there's a path in the code (maybe copy WS) which causes a null failure
*/

ALTER TABLE BOELaborType ADD CONSTRAINT BOELaborType_LaborSortId_Default DEFAULT 2000 FOR LaborSortId;
GO

/*
       1/22/20		Dusan			Temp fix because there's a path in the code (maybe copy WS) which causes a null failure
       ## END ##
*/