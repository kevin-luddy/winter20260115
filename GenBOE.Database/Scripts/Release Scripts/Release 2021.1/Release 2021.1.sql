EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2021.1';
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

/*
	## START ##

	9/10/2020 [ranzalon] - BOEJ-4773 New Tables for MOQ Types
*/

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MOQTypeSelection]') AND type in (N'U'))
BEGIN

CREATE TABLE [dbo].[MOQTypeSelection] (
	[MOQTypeSelectionId] [int] NOT NULL PRIMARY KEY IDENTITY(1,1),
	[TaskId] [int] NOT NULL FOREIGN KEY REFERENCES [dbo].[BOETaskElement](BOETaskElementID),
	[MOQTypeSelection] [int] NOT NULL FOREIGN KEY REFERENCES [dbo].[MOQTypeLU](MOQTypeID),
	[UpdateDT] [datetime2](7) NOT NULL,
	[Order] [int] NOT NULL DEFAULT 2000,
	[CERLocation] [varchar](255) NULL,
	[HoursDescription] [varchar](max) NULL,
	[SubjectMatterExpert] [varchar](max) NULL,
	[HoursLogicAndAssumptions] [varchar](max) NULL,
	[DurationLogicAndAssumptions] [varchar](max) NULL,
	[EstimateTasks] [varchar](max) NULL,
	[Rationale] [varchar](max) NULL,
	[SkillMix] [varchar](max) NULL
)

CREATE TABLE [version].[MOQTypeSelection] (
	[MOQTypeSelectionId] [int] NOT NULL,
	[TaskId] [int] NOT NULL,
	[MOQTypeSelection] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[Order] [int] NOT NULL,
	[CERLocation] [varchar](255) NULL,
	[HoursDescription] [varchar](max) NULL,
	[SubjectMatterExpert] [varchar](max) NULL,
	[HoursLogicAndAssumptions] [varchar](max) NULL,
	[DurationLogicAndAssumptions] [varchar](max) NULL,
	[EstimateTasks] [varchar](max) NULL,
	[Rationale] [varchar](max) NULL,
	[SkillMix] [varchar](max) NULL,
	[VersionId] [int] NOT NULL
)

END

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MOQTypeSelectionTableData]') AND type in (N'U'))
BEGIN

CREATE TABLE [dbo].[MOQTypeSelectionTableData](
	[MOQTypeSelectionTableDataId] [int] NOT NULL PRIMARY KEY IDENTITY(1,1),
	[MOQTypeSelectionId] [int] NOT NULL FOREIGN KEY REFERENCES [dbo].[MOQTypeSelection](MOQTypeSelectionId),
	[UpdateDT] [datetime2](7) NOT NULL,
	[Order] [int] NOT NULL DEFAULT 2000,
	[TableName] [varchar](255) NOT NULL,
	[RepositoryName] [varchar](50) NULL,
	[QueryType] [varchar](40) NULL,
	[DateOfReport] [datetime2](7) NOT NULL,
	[HistoricalProgramName] [varchar](125) NOT NULL,
	[ContractNumber] [varchar](255) NULL,
	[WbsElement] [varchar](2500) NOT NULL,
	[PeriodOfPerformanceStartDate] [datetime2](7) NOT NULL,
	[PeriodOfPerformanceEndDate] [datetime2](7) NOT NULL,
	[TotalWbsHours] [decimal](10,2) NOT NULL,
	[AdditionalQueryFilters] [varchar](2500) NOT NULL,
	[TotalRelevantHoursAfterQueryFilters] [decimal](10,2) NOT NULL
)

CREATE TABLE [version].[MOQTypeSelectionTableData](
	[MOQTypeSelectionTableDataId] [int] NOT NULL,
	[MOQTypeSelectionId] [int] NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[Order] [int] NOT NULL,
	[TableName] [varchar](255) NOT NULL,
	[RepositoryName] [varchar](50) NULL,
	[QueryType] [varchar](40) NULL,
	[DateOfReport] [datetime2](7) NOT NULL,
	[HistoricalProgramName] [varchar](125) NOT NULL,
	[ContractNumber] [varchar](255) NULL,
	[WbsElement] [varchar](2500) NOT NULL,
	[PeriodOfPerformanceStartDate] [datetime2](7) NOT NULL,
	[PeriodOfPerformanceEndDate] [datetime2](7) NOT NULL,
	[TotalWbsHours] [decimal](10,2) NOT NULL,
	[AdditionalQueryFilters] [varchar](2500) NOT NULL,
	[TotalRelevantHoursAfterQueryFilters] [decimal](10,2) NOT NULL,
	[VersionId] [int] NOT NULL
)

END
/*
	9/10/2020 [ranzalon] - BOEJ-4773 New Tables for MOQ Types

	## END ##
*/