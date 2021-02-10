EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2.18';
GO

DECLARE @value INT;
SELECT @value = COLUMNPROPERTY(OBJECT_ID('dbo.BOE'), 'CamName', 'IsFulltextIndexed')

IF (@value = 1)
BEGIN
ALTER FULLTEXT INDEX ON [dbo].[BOE] DROP ([CamName])
ALTER FULLTEXT INDEX ON [dbo].[BOE] DROP ([Category])
ALTER FULLTEXT INDEX ON [dbo].[BOE] DROP ([Rationale])
ALTER FULLTEXT INDEX ON [dbo].[BOE] DROP ([SOWTitle])
PRINT 'Dropped FULL Text Indices'
END

/*
	## START ##
	9/11/17 [twilson3] - Project Map DB
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMap]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[ProjectMap](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[WorkspaceId] int NOT NULL,
		[WbsNumber] varchar(120) NOT NULL,
		[WbsElementTitle] varchar(255) NOT NULL,
		[ActivityID] varchar(100) NOT NULL,
		[ActivityName] varchar(max) NOT NULL,
		[Resource] varchar(20) NOT NULL,
		[LegacyResource] varchar(20) NULL,
		[CostCenter] varchar(20) NOT NULL,
		[StartDate] date NOT NULL,
		[EndDate] date NOT NULL,
		[CLIN] varchar(120) NOT NULL,
		[Task] varchar(max) NULL,
		[SOW] varchar(255) NULL,
		[SOWTitle] varchar(max) NULL,
		[Rationale] varchar(max) NULL,
		[CamName] varchar(255) NULL,
		[Category] varchar(255) NULL,
		[Hours] decimal(18,6) NULL,
		[Dollars] decimal(18,6) NULL,
		[CanOffload] bit NOT NULL,
		[AddOrDelete] varchar(1) NULL,
		[ClassOfCost] varchar(3) NULL,
		[OrderID] INT NOT NULL
	 CONSTRAINT [PK_ProjectMap] PRIMARY KEY NONCLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	CREATE CLUSTERED INDEX [IX_ProjectMap_WorkspaceId] ON [dbo].[ProjectMap] 
	(
		[WorkspaceId]   ASC
	) 
	ON [PRIMARY]

	CREATE NONCLUSTERED INDEX [IX_ProjectMap_OrderId] ON [dbo].[ProjectMap] 
	(
		[OrderID]   ASC
	) 
	ON [PRIMARY]

	CREATE FULLTEXT INDEX ON [dbo].[ProjectMap]([CamName], [Category], [Rationale], [SOWTitle], [Task], [ActivityID], [ActivityName], [WbsNumber], [WbsElementTitle])   
		KEY INDEX PK_ProjectMap
		ON ftcSearch

END
GO

IF COL_LENGTH('[dbo].[ProjectMap]', 'OrderID') IS NULL
BEGIN
    ALTER TABLE [dbo].[ProjectMap]ADD [OrderID] int NULL

	CREATE NONCLUSTERED INDEX [IX_ProjectMap_OrderId] ON [dbo].[ProjectMap] 
	(
		[OrderID]   ASC
	) 
	ON [PRIMARY]
END


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProjectMapSpread]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[ProjectMapSpread](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[WorkspaceId] int NOT NULL,
		[ProjectMapId] int NOT NULL,
		[SpreadDate] date NOT NULL,
		[SpreadValue] decimal(18,6) NOT NULL,
	FOREIGN KEY ([ProjectMapId]) REFERENCES [dbo].[ProjectMap](Id),
	CONSTRAINT [PK_ProjectMapSpread] PRIMARY KEY NONCLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	CREATE CLUSTERED INDEX [IX_ProjectMapSpread_WorkspaceId] ON [dbo].[ProjectMapSpread] 
	(
		[WorkspaceId]   ASC
	) 
	ON [PRIMARY]

	CREATE NONCLUSTERED INDEX [IX_ProjectMapSpread_ProjectMapId] ON [dbo].[ProjectMapSpread] 
	(
		[ProjectMapId]   ASC
	) 
	ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[ProjectMap]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[ProjectMap](
		[VersionID] [int] NOT NULL,
		[ID] [int] NOT NULL,
		[WorkspaceId] int NOT NULL,
		[WbsNumber] varchar(120) NOT NULL,
		[WbsElementTitle] varchar(255) NOT NULL,
		[ActivityID] varchar(100) NOT NULL,
		[ActivityName] varchar(max) NOT NULL,
		[Resource] varchar(20) NOT NULL,
		[LegacyResource] varchar(20) NULL,
		[CostCenter] varchar(20) NOT NULL,
		[StartDate] date NOT NULL,
		[EndDate] date NOT NULL,
		[CLIN] varchar(120) NOT NULL,
		[Task] varchar(max) NULL,
		[SOW] varchar(255) NULL,
		[SOWTitle] varchar(max) NULL,
		[Rationale] varchar(max) NULL,
		[CamName] varchar(255) NULL,
		[Category] varchar(255) NULL,
		[Hours] decimal(18,6) NULL,
		[Dollars] decimal(18,6) NULL,
		[CanOffload] bit NOT NULL,
		[AddOrDelete] varchar(1) NULL,
		[ClassOfCost] varchar(3) NULL,
		[OrderID] INT NOT NULL
	) ON [PRIMARY]

END
GO

IF COL_LENGTH('[version].[ProjectMap]', 'OrderID') IS NULL
BEGIN
    ALTER TABLE [version].[ProjectMap]ADD [OrderID] int NULL
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[version].[ProjectMapSpread]') AND type in (N'U'))
BEGIN
	CREATE TABLE [version].[ProjectMapSpread](
		[VersionID] [int] NOT NULL,
		[ID] [int] NOT NULL,
		[WorkspaceId] int NOT NULL,
		[ProjectMapId] int NOT NULL,
		[SpreadDate] date NOT NULL,
		[SpreadValue] decimal(18,6) NOT NULL) 
	ON [PRIMARY]

END
GO
/*
	9/11/17 [twilson3] - Project Map DB
	## END ##

	## START ##
	9/25/17 [twilson3] - BOEJ-2570 Remove old stored procs
*/

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceOffloadRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceOffloadRate];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertWorkspaceOffloadRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertWorkspaceOffloadRate];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getBOEIDForProjectMapAdvancedSearch]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getBOEIDForProjectMapAdvancedSearch];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getBOEIDForProjectMapQuickSearch]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getBOEIDForProjectMapQuickSearch];

GO

/*
	9/25/17 [twilson3] - BOEJ-2570 Remove old stored procs
	## END ##

	## START ##
	9/28/17 [twilson3] - BOEJ-2534 Clean up after the project map table change
*/

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'IsAddOrDelete' AND Object_ID = Object_ID(N'[dbo].[BOELaborType]'))
BEGIN

	BEGIN TRANSACTION

	BEGIN TRY

	-- Copy data from the BOE tables into the new projectmap tables
	INSERT INTO [dbo].[ProjectMap]
			   (
				[WorkspaceId]
				,[WbsNumber]
				,[WbsElementTitle]
				,[ActivityID]
				,[ActivityName]
				,[Resource]
				,[LegacyResource]
				,[CostCenter] 
				,[StartDate] 
				,[EndDate] 
				,[CLIN] 
				,[Task] 
				,[SOW] 
				,[SOWTitle] 
				,[Rationale]
				,[CamName] 
				,[Category] 
				,[Hours]
				,[Dollars] 
				,[CanOffload]
				,[AddOrDelete] 
				,[ClassOfCost] 
				,[OrderID]
			   )
	SELECT 	    B.WorkspaceId
				,WBS.[DisplayedWBSNumber]
				,WBS.[WBSTitle]
				,B.[BOETitle]
				,B.[BOEDescription]
				,R.[ResourceName]
				,LT.[OldResource]
				,PO.[PerformingOrganizationName]
				,LT.[BOELaborTypeStartDate] 
				,LT.[BOELaborTypeEndDate] 
				,C.[DisplayedCLINNumber] 
				,TE.[TaskDescription] 
				,B.[SOW] 
				,B.[SOWTitle] 
				,B.[Rationale]
				,B.[CamName] 
				,B.[Category] 
				,CASE
				WHEN LT.[SpreadTypeID] = 1 THEN LT.[ValueSpread]
				ELSE 0
				END
				,CASE
				WHEN LT.[SpreadTypeID] = 2 THEN LT.[ValueSpread]
				ELSE 0
				END
				,LT.[CanOffload]
				,CASE
					WHEN LT.[IsAddOrDelete] = 1 THEN 'A'
					ELSE 'D'
				END
				,CASE
					WHEN B.[ClassOfCost] = 1 THEN 'REC'
					WHEN B.[ClassOfCost] = 2 THEN 'NRE'
					ELSE 'DNR'
				END
				,LT.BOELaborTypeID  -- using BOELaborTypeID here to find the real ids when inserting spreads later
	FROM  [dbo].[Workspace] W
	INNER JOIN [dbo].[BOE] B ON B.WorkspaceID = W.WorkspaceID
	INNER JOIN [dbo].[BOETaskElement] TE ON B.BOEID = TE.BOEID
	INNER JOIN [dbo].[BOELaborType] LT ON LT.BOETaskElementID = TE.BOETaskElementID
	INNER JOIN [dbo].[WBS_CLIN_BOE_XREF] xref ON xref.BOEID = B.BOEID
	INNER JOIN [dbo].[WorkBreakdownStructure] WBS ON xref.WBSID = WBS.WBSID
	INNER JOIN [dbo].[CLIN] C ON xref.CLINID = C.CLINID
	INNER JOIN [dbo].[Resource] R ON LT.ResourceID = R.ResourceID
	INNER JOIN [dbo].[PerformingOrganization] PO ON LT.PerformingOrganizationID = PO.PerformingOrganizationID
	WHERE W.[ProjectMapTypeID] IN (3, 4)

	INSERT INTO [dbo].[ProjectMapSpread]
			   (
			   [WorkspaceId]
			   ,[ProjectMapId]
			   ,[SpreadDate]
			   ,[SpreadValue]
			   )
	SELECT 	    P.[WorkspaceId],
				P.[ID],
				LS.[LaborSpreadDate],
				LS.[LaborSpreadValue]
	FROM  [dbo].[BOELaborSpread] LS
	   INNER JOIN [dbo].[ProjectMap] P ON P.OrderID = LS.BOELaborTypeID

	DECLARE @BOE TABLE
	(
		[BOEID] [int] NULL
	)	

	INSERT INTO @BOE (BOEID)
	SELECT B.BOEID
		FROM  [dbo].[Workspace] W
		INNER JOIN [dbo].[BOE] B ON B.WorkspaceID = W.WorkspaceID
		WHERE W.[ProjectMapTypeID] IN (3, 4)

	DELETE FROM [BOELaborSpread]
	WHERE BOELaborTypeID IN
	(
		SELECT LT.BOELaborTypeID
		FROM @BOE B 
		INNER JOIN [dbo].[BOETaskElement] TE ON B.BOEID = TE.BOEID
		INNER JOIN [dbo].[BOELaborType] LT ON LT.BOETaskElementID = TE.BOETaskElementID
	)

	DELETE FROM [dbo].[BOELaborType]
	WHERE BOETaskElementID IN
	(
		SELECT TE.BOETaskElementID
		FROM @BOE B 
		INNER JOIN [dbo].[BOETaskElement] TE ON B.BOEID = TE.BOEID
	)

	DELETE FROM [dbo].[BOEUserRole]
	WHERE BOEID IN
	(
		SELECT B.BOEID
		FROM @BOE B 
	)

	DELETE FROM [dbo].[BOEUserRoleHistory]
	WHERE BOEID IN
	(
		SELECT B.BOEID
		FROM @BOE B 
	)

	DELETE FROM [dbo].[BOEApproval]
	WHERE BOEID IN
	(
		SELECT B.BOEID
		FROM @BOE B 
	)

	DELETE FROM [dbo].[TravelTripTaskElement]
	WHERE BOEID IN
	(
		SELECT B.BOEID
		FROM @BOE B 
	)

	DELETE FROM [dbo].[BOETaskElement]
	WHERE BOEID IN
	(
		SELECT B.BOEID
		FROM @BOE B 
	)

	DECLARE @WBS_CLIN_BOE_XREF TABLE
	(
		[WBSID] [int] NULL,
		[CLINID] [int] NULL,
		[BOEID] [int] NULL
	)	

	INSERT INTO @WBS_CLIN_BOE_XREF ([WBSID],[CLINID],[BOEID])
	SELECT  X.[WBSID], X.[CLINID], X.[BOEID]
	FROM [dbo].[WBS_CLIN_BOE_XREF] X 
	INNER JOIN @BOE B ON X.BOEID = B.BOEID

	DELETE FROM [dbo].[WBS_CLIN_BOE_XREF]
	WHERE BOEID IN
	(
		SELECT B.BOEID
		FROM @BOE B 
	)

	DELETE FROM [dbo].[CLIN]
	WHERE CLINID IN
	(
		SELECT distinct xref.CLINID
		FROM @WBS_CLIN_BOE_XREF xref 
	)

	DELETE FROM [dbo].[WorkBreakdownStructure]
	WHERE WBSID IN
	(
		SELECT distinct xref.WBSID
		FROM @WBS_CLIN_BOE_XREF xref 
	)

	DELETE FROM [dbo].[BOEStateHistory]
	WHERE BOEID IN
	(
		SELECT B.BOEID
		FROM @BOE B 
	)

	DELETE FROM [dbo].[BOE]
	WHERE BOEID IN
	(
		SELECT B.BOEID
		FROM @BOE B 
	)

	ALTER TABLE [dbo].[BOELaborType] DROP COLUMN [IsAddOrDelete]
	ALTER TABLE [version].[BOELaborType] DROP COLUMN [IsAddOrDelete]
	ALTER TABLE [dbo].[BOE] DROP COLUMN [SOW] 
	ALTER TABLE [dbo].[BOE] DROP COLUMN [SOWTitle]
	ALTER TABLE [dbo].[BOE] DROP COLUMN [CamName]
	ALTER TABLE [dbo].[BOE] DROP COLUMN [Category]
	ALTER TABLE [version].[BOE] DROP COLUMN [SOW]
	ALTER TABLE [version].[BOE] DROP COLUMN [SOWTitle]
	ALTER TABLE [version].[BOE] DROP COLUMN [CamName]
	ALTER TABLE [version].[BOE] DROP COLUMN [Category]
	ALTER TABLE [dbo].[BOE] DROP COLUMN [Rationale]
	ALTER TABLE [version].[BOE] DROP COLUMN [Rationale]
	ALTER TABLE [dbo].[BOE] DROP COLUMN ClassOfCost
	ALTER TABLE [version].[BOE] DROP COLUMN ClassOfCost
	ALTER TABLE [dbo].[BOELaborType] DROP COLUMN [OldResource]
	ALTER TABLE [version].[BOELaborType] DROP COLUMN [OldResource]

	IF @@ERROR = 0
		BEGIN
			COMMIT TRANSACTION
		END
	END TRY


	BEGIN CATCH
		ROLLBACK TRANSACTION
	

		DECLARE @ErrorMessage varchar (500)
		SELECT @ErrorMessage = ERROR_MESSAGE()
		RAISERROR (
				@ErrorMessage, -- Message text.
				11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)

		RETURN
	
	END CATCH
END
GO
/*
	9/28/17 [twilson3] - BOEJ-2534 Clean up after the project map table change
	## END ##
*/