IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[copyWorkspaceVersion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[copyWorkspaceVersion]	
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[copyWorkspaceVersion]
(
@WorkspaceID int,
@WorkspaceName varchar (115),
@WorkspaceShortName varchar(21),
@VersionID int,
@BoeList varchar(max)
)
AS
/******************************************************************************
**		 
**		Name: [copyWorkspaceVersion]
**		Desc: Temporarily Copy All Workspace Data of a previous version
**			  into a new Workspace
**			
**
**		Auth: RJ Anzalone
**		Date: 12/10/19
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		1/22/20		ranzalon			Fixed bug with missing RteTemplateSourceId
**		5/15/20		Dusan				Changed Split_String function call to call our SplitString function
**		6/11/20		Dusan				BOEJ-4655 Exact copy should copy WS email settings
**		7/28/20		RJ					BOEJ-4713 Remove email settings
**		8/27/20		ranzalon			BOEJ-4760 - Template Boe
**		9/15/20		ranzalon			BOEJ-4776/4825 - MOQ Types update
**		10/29/20	Dusan				BOEJ-4925: Fixed MOQTypeSelectionId not copying
**		12/8/2020	ranzalon			BOEJ-4972 - remove CER location and BOELaborType MOQTypeSelectionId fields
*******************************************************************************/
SET NOCOUNT ON 

BEGIN TRANSACTION

BEGIN TRY
	DECLARE @CopyFromWorkspaceID int = @WorkspaceID
	DECLARE @ResourceListID int
	INSERT INTO [dbo].[ResourceList]
			   ([UpdateDT]
			   ,[ResourceListName])
	SELECT RL.[UpdateDT]
		  ,[ResourceListName]
	  FROM [version].[ResourceList] RL
	  INNER JOIN [version].[Workspace] W ON RL.ResourceListID = W.ResourceListID AND RL.VersionID = W.VersionID
	WHERE W.WorkspaceID = @WorkspaceID AND W.VersionID = @VersionID
 
	SELECT  @ResourceListID = SCOPE_IDENTITY() 

	DECLARE @PerformingOrganizationListID int
	INSERT INTO [dbo].[PerformingOrganizationList]
			   ([UpdateDT]
			   ,[PerformingOrganizationListName])
	SELECT PL.[UpdateDT]
		  ,PL.[PerformingOrganizationListName]
	  FROM [version].[PerformingOrganizationList] PL
	  INNER JOIN [version].[Workspace] W ON PL.PerformingOrganizationListID = W.PerformingOrganizationListID AND PL.VersionID = W.VersionID
	WHERE W.WorkspaceID = @WorkspaceID AND W.VersionID = @VersionID
 
	SELECT  @PerformingOrganizationListID = SCOPE_IDENTITY() 

	DECLARE @NewWorkspaceID int
	INSERT INTO [dbo].[Workspace]
			   ([UpdateDT]
			   ,[WorkspaceName]
			   ,[WorkspaceShortName]
			   ,[WorkspaceStateID]
			   ,[ContractStartDate]
			   ,[ContractEndDate]
			   ,[ProposalSubmitDate]
			   ,[WorkspaceDescription]
			   ,[CostVolumeLeadPricerUserID]
			   ,[RFPNumber]
			   ,[TemplateID]
			   ,[ContainsOCI]
			   ,[CreatedByETIUserID]
			   ,[AllowSearch]
			   ,[ResourceListID]
			   ,[PerformingOrganizationListID]
			   ,[PerformingOrganizationChangeFlag]
			   ,[TrackingNumber]
			   ,[ContainsTemplate]
			   ,[NumProPricerExport]
			   ,[ProposalStatusID]
			   ,[StatusComment]
			   ,[BOEExportSortByID]
			   ,[SegmentID]
			   ,[LineOfBusinessID]
			   ,[ProposalClassID]
			   ,[ProposalTitle]
			   ,[IsDeleted]
			   ,[DateDeleted]
			   ,[ResourcePrecision]
			   ,[RecalculationStartedDate]
			   ,[CostPrecision]
			   ,[IsUsingEquivalentPerson]
			   ,[IsUsingTM]
			   ,[ProjectMapTypeID]
			   ,[AllowGridEdit]
			   ,[CustomSorting]
			   ,[ResourceSorting]
			   ,[PerfOrgSorting]
			   ,[LastProPricerInstance]
			   ,[LastProPricerProposal]
			   ,[RteSizeLimit]
			   ,[RevisedSubmittalDate]
			   ,[TemplateBoe]
			   )
		SELECT [UpdateDT]
		  ,@WorkspaceName
		  ,@WorkspaceShortName
		  ,[WorkspaceStateID]
		  ,[ContractStartDate]
		  ,[ContractEndDate]
		  ,[ProposalSubmitDate]
		  ,[WorkspaceDescription]
		  ,[CostVolumeLeadPricerUserID]
		  ,[RFPNumber]
		  ,[TemplateID]
		  ,[ContainsOCI]
		  ,[CreatedByETIUserID]
		  ,[AllowSearch]
		  ,@ResourceListID
		  ,@PerformingOrganizationListID
		  ,[PerformingOrganizationChangeFlag]
		  ,[TrackingNumber]
		  ,[ContainsTemplate]
		  ,[NumProPricerExport]
		  ,[ProposalStatusID]
		  ,[StatusComment]
		  ,[BOEExportSortByID]
		  ,[SegmentID]
		  ,[LineOfBusinessID]
		  ,[ProposalClassID]
		  ,[ProposalTitle]
		  ,1 --[IsDeleted]
		  ,DATEADD(day, -60, GETDATE()) --[DateDeleted] - 60 Days ago so it gets deleted
		  ,[ResourcePrecision]
		  ,[RecalculationStartedDate]
		  ,[CostPrecision]
		  ,[IsUsingEquivalentPerson]
		  ,[IsUsingTM]
		  ,[ProjectMapTypeID]
		  ,[AllowGridEdit]
		  ,[CustomSorting]
		  ,[ResourceSorting]
		  ,[PerfOrgSorting]
		  ,null --LastProPricerInstance
		  ,null --LastProPricerProposal
		  ,[RteSizeLimit]
		  ,[RevisedSubmittalDate]
		  ,[TemplateBoe]
	  FROM [version].[Workspace]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	SELECT @NewWorkspaceID = SCOPE_IDENTITY()

	DECLARE @WorkspaceContractTypeXREF TABLE 
	(
		[WorkspaceContractTypeID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceID] [int] NOT NULL,
		[ContractTypeID] [int] NOT NULL,
		[Processed] bit DEFAULT 0,
		[NewWorkspaceContractTypeID] [int] NULL,
		[NewWorkspaceID] [int] NOT NULL
	)
	INSERT INTO @WorkspaceContractTypeXREF
	SELECT [WorkspaceContractTypeID]
		  ,[UpdateDT]
		  ,[WorkspaceID]
		  ,[ContractTypeID]
		  ,0
		  ,NULL
		  ,@NewWorkspaceID
	FROM [version].[WorkspaceContractTypeXREF] 
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	DECLARE @WorkspaceContractTypeID int
	WHILE EXISTS (SELECT 1 FROM @WorkspaceContractTypeXREF WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @WorkspaceContractTypeID = WorkspaceContractTypeID FROM @WorkspaceContractTypeXREF WHERE Processed = 0
	INSERT INTO [dbo].[WorkspaceContractTypeXREF]
			   (
				[UpdateDT]
			   ,[WorkspaceID]
			   ,[ContractTypeID]
				)
	 SELECT [UpdateDT]
		  ,NewWorkspaceID
		  ,[ContractTypeID]
	  FROM @WorkspaceContractTypeXREF
	WHERE WorkspaceContractTypeID = @WorkspaceContractTypeID

	UPDATE @WorkspaceContractTypeXREF
	SET NewWorkspaceContractTypeID = SCOPE_IDENTITY(),
		Processed = 1
	WHERE WorkspaceContractTypeID = @WorkspaceContractTypeID
	END

	INSERT INTO [dbo].[OutputFormatTemplateWorkspaceXREF]
			   ([WorkspaceID]
			   ,[TemplateID])
	SELECT @NewWorkspaceID
		  ,[TemplateID]
	  FROM [version].[OutputFormatTemplateWorkspaceXREF]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	DECLARE @Resource TABLE
	(
		[ResourceID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[ResourceName] [varchar](20) NOT NULL,
		[ResourceDescription] [varchar](100) NULL,
		[SegmentRegion] [varchar](50) NULL,
		[LaborType] [varchar](50) NULL,
		[SegmentID] [int] NULL,
		[ResourceListID] [int] NOT NULL,
		[CostElementID] [int] NOT NULL,
		[DeletedFlag] [bit] NULL,
		[RateTypeID] [int] NULL,
		Processed bit DEFAULT 0,
		NewResourceListID int,
		NewWorkspaceID int,
		NewResourceID int
	)

	INSERT INTO @Resource
			   ([ResourceID]
			   ,[UpdateDT]
			   ,[ResourceName]
			   ,[ResourceDescription]
			   ,[SegmentRegion]
			   ,[LaborType]
			   ,[SegmentID]
			   ,[ResourceListID]
			   ,[CostElementID]
			   ,[DeletedFlag]
			   ,[RateTypeID]
           		,Processed
				,NewWorkspaceID
				,NewResourceID)
	SELECT R.[ResourceID]
		  ,R.[UpdateDT]
		  ,R.[ResourceName]
		  ,R.[ResourceDescription]
		  ,R.[SegmentRegion]
		  ,R.[LaborType]
		  ,R.[SegmentID]
		  ,@ResourceListID
		  ,R.[CostElementID]
		  ,R.[DeletedFlag]
		  ,R.[RateTypeID]
		  ,0 AS Processed
		  ,@NewWorkspaceID AS NewWorkspaceID
		  ,NULL AS NewResourceID
	  FROM [version].[Resource] R
		INNER JOIN [version].[ResourceList] RL ON R.ResourceListID = RL.ResourceListID AND R.VersionID = RL.VersionID
		INNER JOIN [version].[Workspace] W ON RL.ResourceListID = W.ResourceListID AND W.VersionID = RL.VersionID
	WHERE W.WorkspaceID = @WorkspaceID AND W.VersionID = @VersionID

	DECLARE @ResourceID int
	WHILE EXISTS (SELECT 1 FROM @Resource WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @ResourceID = ResourceID FROM @Resource WHERE Processed = 0

	INSERT INTO [dbo].[Resource]
			   (
				[UpdateDT]
			   ,[ResourceName]
			   ,[ResourceDescription]
			   ,[SegmentRegion]
			   ,[LaborType]
			   ,[SegmentID]
			   ,[ResourceListID]
			   ,[CostElementID]
			   ,[DeletedFlag]
			   ,[RateTypeID]
			   )
	SELECT 
		  R.[UpdateDT]
		  ,R.[ResourceName]
		  ,R.[ResourceDescription]
		  ,R.[SegmentRegion]
		  ,R.[LaborType]
		  ,R.[SegmentID]
		  ,@ResourceListID
		  ,R.[CostElementID]
		  ,R.[DeletedFlag]
		  ,R.[RateTypeID]
	FROM @Resource R
	WHERE
		ResourceID = @ResourceID

	UPDATE @Resource 
	SET	NewResourceID = SCOPE_IDENTITY(),
		Processed = 1
	WHERE 
		ResourceID = @ResourceID


	END

	DECLARE @WorkspaceResource TABLE
	(
		[SystemResourceID] [int],
		[ResourceListID] [int],
		[WorkspaceID] [int],
		NewSystemResourceID [int]
	)
	INSERT INTO @WorkspaceResource
	SELECT 
		   [SystemResourceID]
		  ,@ResourceListID AS [ResourceListID]
		  ,@NewWorkspaceID AS [WorkspaceID]
		  ,NULL
	FROM [version].[WorkspaceResource]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	UPDATE @WorkspaceResource
	SET NewSystemResourceID = R.NewResourceID
	FROM @WorkspaceResource tWR
		INNER JOIN @Resource R ON tWR.SystemResourceID = R.ResourceID

	INSERT INTO [dbo].[WorkspaceResource]
			   ([SystemResourceID]
			   ,[ResourceListID]
			   ,[WorkspaceID])
	/*Workspace Resources*/
	SELECT 
		  [NewSystemResourceID]
		  ,[ResourceListID]
		  ,[WorkspaceID]
	FROM @WorkspaceResource WHERE NewSystemResourceID IS NOT NULL
	UNION
	/*System Resources*/
	SELECT 
		  [SystemResourceID]
		  ,[ResourceListID]
		  ,[WorkspaceID]
	FROM @WorkspaceResource WHERE NewSystemResourceID IS NULL

	DECLARE @PerformingOrganization TABLE
	(
		[PerformingOrganizationID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[PerformingOrganizationName] [varchar](20) NOT NULL,
		[PerformingOrganizationDescription] [varchar](50) NULL,
		[PerformingOrganizationListID] [int] NOT NULL,
		[DeletedFlag] [bit] NULL,
		Processed bit DEFAULT 0,
		NewPerformingOrganizationListID int,
		NewWorkspaceID int,
		NewPerformingOrganizationID int
	)

	INSERT INTO @PerformingOrganization
			   ([PerformingOrganizationID]
			   ,[UpdateDT]
			   ,[PerformingOrganizationName]
			   ,[PerformingOrganizationDescription]
			   ,[PerformingOrganizationListID]
			   ,[DeletedFlag]
           		,Processed
				,NewWorkspaceID
				,NewPerformingOrganizationID)
	SELECT P.[PerformingOrganizationID]
		  ,P.[UpdateDT]
		  ,P.[PerformingOrganizationName]
		  ,P.[PerformingOrganizationDescription]
		  ,@PerformingOrganizationListID
		  ,P.[DeletedFlag]
		  ,0 AS Processed
		  ,@NewWorkspaceID AS NewWorkspaceID
		  ,NULL AS NewPerformingOrganizationID
	  FROM [version].[PerformingOrganization] P
		INNER JOIN [version].[PerformingOrganizationList] PL ON P.PerformingOrganizationListID = PL.PerformingOrganizationListID AND P.VersionID = PL.VersionID
		INNER JOIN [version].[Workspace] W ON PL.PerformingOrganizationListID = W.PerformingOrganizationListID AND W.VersionID = PL.VersionID
	WHERE W.WorkspaceID = @WorkspaceID	and W.VersionID = @VersionID

	DECLARE @PerformingOrganizationID int
	WHILE EXISTS (SELECT 1 FROM @PerformingOrganization WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @PerformingOrganizationID = PerformingOrganizationID FROM @PerformingOrganization WHERE Processed = 0

	INSERT INTO [dbo].[PerformingOrganization]
			   (
				[UpdateDT]
			   ,[PerformingOrganizationName]
			   ,[PerformingOrganizationDescription]
			   ,[PerformingOrganizationListID]
			   ,[DeletedFlag])
	SELECT 
		  R.[UpdateDT]
		  ,R.[PerformingOrganizationName]
		  ,R.[PerformingOrganizationDescription]
		  ,@PerformingOrganizationListID
		  ,R.[DeletedFlag]
	FROM @PerformingOrganization R
	WHERE
		PerformingOrganizationID = @PerformingOrganizationID

	UPDATE @PerformingOrganization 
	SET	NewPerformingOrganizationID = SCOPE_IDENTITY(),
		Processed = 1
	WHERE 
		PerformingOrganizationID = @PerformingOrganizationID

	END

	DECLARE @WorkspacePerformingOrganization TABLE
	(
		[SystemPerformingOrganizationID] [int],
		[PerformingOrganizationListID] [int],
		[WorkspaceID] [int],
		NewSystemPerformingOrganizationID [int]
	)
	INSERT INTO @WorkspacePerformingOrganization
	SELECT 
		   [SystemPerformingOrganizationID]
		  ,@PerformingOrganizationListID AS [PerformingOrganizationListID]
		  ,@NewWorkspaceID AS [WorkspaceID]
		  ,NULL
	FROM [version].[WorkspacePerformingOrganization]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	UPDATE @WorkspacePerformingOrganization
	SET NewSystemPerformingOrganizationID = R.NewPerformingOrganizationID
	FROM @WorkspacePerformingOrganization tWR
		INNER JOIN @PerformingOrganization R ON tWR.SystemPerformingOrganizationID = R.PerformingOrganizationID

	INSERT INTO [dbo].[WorkspacePerformingOrganization]
			   ([SystemPerformingOrganizationID]
			   ,[PerformingOrganizationListID]
			   ,[WorkspaceID])
	SELECT 
		  [NewSystemPerformingOrganizationID]
		  ,[PerformingOrganizationListID]
		  ,[WorkspaceID]
	FROM @WorkspacePerformingOrganization WHERE NewSystemPerformingOrganizationID IS NOT NULL
	UNION
	/*System PerformingOrganizations*/
	SELECT 
		  [SystemPerformingOrganizationID]
		  ,[PerformingOrganizationListID]
		  ,[WorkspaceID]
	FROM @WorkspacePerformingOrganization WHERE NewSystemPerformingOrganizationID IS NULL

	DECLARE @BOE TABLE
	(
		[BOEID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[BOEStateID] [int] NOT NULL,
		[BOEStartDate] [date] NOT NULL,
		[BOEEndDate] [date] NOT NULL,
		[BOEDescription] [varchar](max) NULL,
		[DataSource] [varchar](max) NULL,
		[WorkspaceID] [int] NOT NULL,
		[MetricDisclosureAcknowledge] [bit] NOT NULL,
		[NumAuthorReassigned] [int] NOT NULL,
		[IsMaterial] [bit] NOT NULL,
		Processed bit DEFAULT 0,
		NewBOEID int NULL,
		NewWorkspaceID int NOT NULL,
		[BOETitle] varchar (100) NOT NULL,
		[IsMultiClinWbs] [bit] DEFAULT 0
	)

	IF(@BoeList = '')
	/*All BOEs*/
	BEGIN
		INSERT INTO @BOE	
		SELECT [BOEID]
			  ,[UpdateDT]
			  ,[BOEStateID]
			  ,[BOEStartDate]
			  ,[BOEEndDate]
			  ,[BOEDescription]
			  ,[DataSource]
			  ,[WorkspaceID]
			  ,[MetricDisclosureAcknowledge]
			  ,[NumAuthorReassigned]
			  ,[IsMaterial]
			  ,0
			  ,NULL
			  ,@NewWorkspaceID
			  ,[BOETitle]
			  ,[IsMultiClinWbs]
		  FROM [version].[BOE]
		WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID
	END
	ELSE
	/*Select BOEs*/
	BEGIN
		INSERT INTO @BOE	
		SELECT [BOEID]
			  ,[UpdateDT]
			  ,[BOEStateID]
			  ,[BOEStartDate]
			  ,[BOEEndDate]
			  ,[BOEDescription]
			  ,[DataSource]
			  ,[WorkspaceID]
			  ,[MetricDisclosureAcknowledge]
			  ,[NumAuthorReassigned]
			  ,[IsMaterial]
			  ,0
			  ,NULL
			  ,@NewWorkspaceID
			  ,[BOETitle]
			  ,[IsMultiClinWbs]
		  FROM [version].[BOE]
		WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID AND BOEID IN
		(
			SELECT Item as BOEID FROM dbo.SplitString(@BoeList, ',', DEFAULT)
		)
	END

	DECLARE @BOEID int
	WHILE EXISTS (SELECT 1 FROM @BOE WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @BOEID = BOEID FROM @BOE WHERE Processed = 0

	INSERT INTO [dbo].[BOE]
	([UpdateDT]
	,[BOEStateID]
	,[BOEStartDate]
	,[BOEEndDate]
	,[BOEDescription]
	,[DataSource]
	,[WorkspaceID]
	,[MetricDisclosureAcknowledge]
	,[NumAuthorReassigned]
	,[IsMaterial]
	,[BOETitle]
	,[IsMultiClinWbs]
	)
	SELECT [UpdateDT]
		  ,[BOEStateID]
		  ,[BOEStartDate]
		  ,[BOEEndDate]
		  ,[BOEDescription]
		  ,[DataSource]
		  ,@NewWorkspaceID
		  ,[MetricDisclosureAcknowledge]
		  ,[NumAuthorReassigned]
		  ,[IsMaterial]
		  ,[BOETitle]
		  ,[IsMultiClinWbs]
	  FROM [version].[BOE]
	WHERE BOEID = @BOEID AND VersionID = @VersionID

	UPDATE @BOE 
	SET	NewBOEID = SCOPE_IDENTITY(),
		Processed = 1
	WHERE 
	BOEID = @BOEID	

	END

	-- These changes are to be executed in RMS only. The way we can tell the environments apart is that SSC has LOBs in the range of 1000's. RMS is 2000+ and ISGS is 0-999
	IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessID > 2000)
	BEGIN
		DECLARE @TravelTripTaskElement TABLE
		(
			[TravelTripTaskElementID] [int] NOT NULL,
			[UpdateDT] [datetime2](7) NOT NULL,
			[TravelTaskID] [varchar](3) NULL,
			[TravelTaskTitle] [varchar](100) NOT NULL,
			[TravelTaskDescription] [varchar](max),
			[BOEID] [int] NOT NULL,
			[TaskStartDate] [DATE],
			[TaskEndDate] [DATE],
			Processed bit DEFAULT 0,
			NewTravelTripTaskElementID [int],
			NewBOEID int,
			[SortOrderID] INT
		)
		INSERT INTO @TravelTripTaskElement
		SELECT TE.[TravelTripTaskElementID]
			  ,TE.[UpdateDT]
			  ,TE.[TravelTaskID]      
			  ,TE.[TravelTaskTitle]
			  ,TE.[TravelTaskDescription]
			  ,TE.[BOEID]
			  ,TE.[TaskStartDate]
			  ,TE.[TaskEndDate]
			  ,0
			  ,NULL
			  ,B.NewBOEID
			  ,TE.[SortOrderID]
		FROM [version].[TravelTripTaskElement] TE 
		 INNER JOIN @BOE B ON TE.BOEID = B.BOEID
		WHERE TE.VersionID = @VersionID

		DECLARE @TravelTripTaskElementID [int] 
		WHILE EXISTS (SELECT 1 FROM @TravelTripTaskElement WHERE Processed = 0)
		BEGIN
		SELECT TOP 1 @TravelTripTaskElementID = TravelTripTaskElementID  FROM @TravelTripTaskElement WHERE Processed = 0
		INSERT INTO [dbo].[TravelTripTaskElement]
				   ([UpdateDT]
				   ,[TravelTaskTitle]
				   ,[TravelTaskDescription]
				   ,[BOEID]
				   ,[TravelTaskID]
				   ,[TaskStartDate]
				   ,[TaskEndDate]
				   ,[SortOrderID]
				   )
		SELECT tTE.[UpdateDT]
			  ,tTE.[TravelTaskTitle]
			  ,tTE.[TravelTaskDescription]
			  ,tB.NewBOEID
			  ,tTE.[TravelTaskID]
			  ,tTE.[TaskStartDate]
			  ,tTE.[TaskEndDate]
			  ,tTE.[SortOrderID]
		  FROM @TravelTripTaskElement tTE 
			INNER JOIN @BOE tB ON tTE.BOEID = tB.BOEID
		WHERE TravelTripTaskElementID = @TravelTripTaskElementID

		UPDATE @TravelTripTaskElement
		SET	NewTravelTripTaskElementID = SCOPE_IDENTITY(),
			Processed = 1
		WHERE 
			TravelTripTaskElementID  = @TravelTripTaskElementID	

		END		
	END

	INSERT INTO [dbo].[WorkspaceOffloadRate]
		([UpdateDT]
		,[WorkspaceID]
		,[Resource]
		,[PerfOrg]
		,[PercentToOffload]
		,[Year]
		,[SubcontractorResource]
		,[HourlyRate])
	SELECT [UpdateDT]
		,@NewWorkspaceID
		,[Resource]
		,[PerfOrg]
		,[PercentToOffload]
		,[Year]
		,[SubcontractorResource]
		,[HourlyRate]
	FROM [version].[WorkspaceOffloadRate]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	INSERT INTO [dbo].[ProjectMap]
		([WorkspaceId]
		,[WbsNumber]
		,[WbsElementTitle]
		,[ActivityID]
		,[ActivityName]
		,[Resource]
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
		,[TieredPercentage]
		,[LegacyResourceID])
	SELECT 
		@NewWorkspaceID
		,[WbsNumber]
		,[WbsElementTitle]
		,[ActivityID]
		,[ActivityName]
		,[Resource]
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
		,[TieredPercentage]
		,[LegacyResourceID]
	FROM [version].[ProjectMap]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	INSERT INTO [dbo].[ProjectMapSpread]
			   (
			   [WorkspaceId]
			   ,[ProjectMapId]
			   ,[SpreadDate]
			   ,[SpreadValue]
			   )
	SELECT 	    @NewWorkspaceID,
				NewP.ID,
				S.SpreadDate,
				S.SpreadValue
	FROM  [version].[ProjectMapSpread] S
	INNER JOIN [version].ProjectMap P ON P.ID = S.ProjectMapId AND S.VersionID = P.VersionID
	INNER JOIN ProjectMap NewP ON NewP.WorkspaceId = @NewWorkspaceID AND NewP.OrderID = P.OrderID
	WHERE S.VersionID = @VersionID

	/**** Custom Fields ****/
	INSERT INTO [dbo].[CustomField]
			   ([UpdateDT]
			   ,[CustomFieldName]
			   ,[CustomFieldRequired]
			   ,[CustomFieldDisplayID]
			   ,[WorkspaceID]
			   ,[IsOpenEnded])
	SELECT [UpdateDT]
		  ,[CustomFieldName]
		  ,[CustomFieldRequired]
		  ,[CustomFieldDisplayID]
		  ,@NewWorkspaceID
		  ,[IsOpenEnded]
	  FROM [version].[CustomField]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	DECLARE @CustomFieldMapping TABLE
	(
		OriginalCustomFieldID int,
		NewCustomFieldID int
	)
	INSERT INTO @CustomFieldMapping
	SELECT Original.CustomFieldID, New.CustomFieldID
	FROM
		(
		SELECT [CustomFieldID]
		  ,[UpdateDT]
		  ,[CustomFieldName]
		  ,[CustomFieldRequired]
		  ,[CustomFieldDisplayID]
		  ,[WorkspaceID]
		  ,[IsOpenEnded]
		FROM [version].[CustomField]
		WHERE WorkspaceID = @WorkspaceID and VersionID = @VersionID
		) Original
		INNER JOIN
		(
		SELECT [CustomFieldID]
		  ,[UpdateDT]
		  ,[CustomFieldName]
		  ,[CustomFieldRequired]
		  ,[CustomFieldDisplayID]
		  ,[WorkspaceID]
		  ,[IsOpenEnded]
		FROM [dbo].[CustomField]
		WHERE WorkspaceID = @NewWorkspaceID
		) New ON
		  Original.[UpdateDT] = New.UpdateDT AND
		  Original.[CustomFieldName] = New.CustomFieldName AND
		  Original.[CustomFieldRequired] = New.CustomFieldRequired AND
		  Original.[CustomFieldDisplayID] = New.CustomFieldDisplayID AND
		  Original.[IsOpenEnded] = New.IsOpenEnded

	DECLARE @CustomFieldValue TABLE
	(
		[CustomFieldValueID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[CustomFieldValueName] [varchar](20) NOT NULL,
		[CustomFieldValueDescription] [varchar](250) NULL,
		[CustomFieldID] [int] NOT NULL,
		[CustomFieldValueInUseFlag] [bit] NOT NULL,
		Processed bit,
		NewCustomFieldValueID int,
		NewCustomFieldID int
	)
	INSERT INTO @CustomFieldValue
	SELECT [CustomFieldValueID]
		  ,[UpdateDT]
		  ,[CustomFieldValueName]
		  ,[CustomFieldValueDescription]
		  ,[CustomFieldID]
		  ,[CustomFieldValueInUseFlag]
		  ,0
		  ,NULL
		  ,CFM.NewCustomFieldID
	  FROM [version].[CustomFieldValue] CFV
	INNER JOIN @CustomFieldMapping CFM ON CFV.CustomFieldID = CFM.OriginalCustomFieldID
	WHERE CFV.VersionID = @VersionID

	DECLARE @CustomFieldValueID int
	WHILE EXISTS (SELECT 1 FROM @CustomFieldValue WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @CustomFieldValueID = CustomFieldValueID FROM @CustomFieldValue WHERE Processed = 0

	INSERT INTO [dbo].[CustomFieldValue]
			   ([UpdateDT]
			   ,[CustomFieldValueName]
			   ,[CustomFieldValueDescription]
			   ,[CustomFieldID]
			   ,[CustomFieldValueInUseFlag])
	SELECT CFV.[UpdateDT]
		  ,CFV.[CustomFieldValueName]
		  ,CFV.[CustomFieldValueDescription]
		  ,CFV.NewCustomFieldID
		  ,CFV.[CustomFieldValueInUseFlag]
	  FROM @CustomFieldValue CFV
	WHERE CFV.CustomFieldValueID = @CustomFieldValueID  

	UPDATE @CustomFieldValue
	SET NewCustomFieldValueID = SCOPE_IDENTITY(),
		Processed = 1
	WHERE CustomFieldValueID = @CustomFieldValueID  

	END

	INSERT INTO [dbo].[BOEPotentialRole]
			   ([UpdateDT]
			   ,[ETIUserID]
			   ,[WorkspaceID]
			   ,[RoleID]
			   ,[UserRemoved])
	SELECT 
		  [UpdateDT]
		  ,[ETIUserID]
		  ,@NewWorkspaceID
		  ,[RoleID]
		  ,[UserRemoved]
	  FROM [version].[BOEPotentialRole]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	DECLARE @WorkBreakdownStructure TABLE
	(
		[WBSID] [int]  NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WBSNumber] [varchar](119) NULL,
		[DisplayedWBSNumber] [varchar](50) NOT NULL,
		[WBSTitle] [varchar](255) NULL,
		[WorkspaceID] [int] NULL,
		Processed bit,
		NewWBSID int,
		NewWorkspaceID int
	)
	INSERT INTO @WorkBreakdownStructure
	SELECT WBS.[WBSID]
		  ,WBS.[UpdateDT]
		  ,WBS.[WBSNumber]
		  ,WBS.[DisplayedWBSNumber]
		  ,WBS.[WBSTitle]
		  ,WBS.[WorkspaceID]
		  ,0
		  ,NULL
		  ,@NewWorkspaceID      
	  FROM [version].[WorkBreakdownStructure] WBS
	WHERE WBS.WorkspaceID = @WorkspaceID AND WBS.VersionID = @VersionID

	DECLARE @WBSID int
	WHILE EXISTS (SELECT 1 FROM @WorkBreakdownStructure WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @WBSID = WBSID FROM @WorkBreakdownStructure WHERE Processed = 0

	INSERT INTO [dbo].[WorkBreakdownStructure]
			   ([UpdateDT]
			   ,[WBSNumber]
			   ,[DisplayedWBSNumber]
			   ,[WBSTitle]
			   ,[WorkspaceID])
	SELECT [UpdateDT]
		  ,[WBSNumber]
		  ,[DisplayedWBSNumber]
		  ,[WBSTitle]
		  ,@NewWorkspaceID
	  FROM @WorkBreakdownStructure
	WHERE WBSID = @WBSID

	UPDATE @WorkBreakdownStructure
	SET NewWBSID = SCOPE_IDENTITY(),
		Processed = 1
	WHERE WBSID = @WBSID

	END

	INSERT INTO [dbo].[TMResourceRate]
			   ([UpdateDT]
			   ,[WorkspaceID]
			   ,[TMResourceID]
			   ,[TMResourceRateStartDate]
			   ,[TMResourceRateEndDate]
			   ,[TMResourceRate])
	SELECT TMRR.[UpdateDT]
		  ,@NewWorkspaceID
		  ,CASE	
			WHEN R.NewResourceID IS NOT NULL THEN R.NewResourceID
			ELSE [TMResourceID]
			END AS [TMResourceID]
		  ,[TMResourceRateStartDate]
		  ,[TMResourceRateEndDate]
		  ,[TMResourceRate]
	  FROM [version].[TMResourceRate] TMRR
		LEFT OUTER JOIN @Resource R ON TMRR.TMResourceID = R.ResourceID
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	INSERT INTO [dbo].[WorkspaceStateHistory]
			   ([UpdateDT]
			   ,[WorkspaceID]
			   ,[CurrentWorkspaceStateID]
			   ,[UpdatedWorkspaceStateID]
			   ,[ChangedByETIUserID])
	SELECT [UpdateDT]
		  ,@NewWorkspaceID
		  ,[CurrentWorkspaceStateID]
		  ,[UpdatedWorkspaceStateID]
		  ,[ChangedByETIUserID]
	  FROM [version].[WorkspaceStateHistory]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	DECLARE @WorkspaceVariable TABLE 
	(
		[WorkspaceVariableID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[WorkspaceVariableName] [varchar](20) NULL,
		[WorkspaceVariableValue] [decimal](29, 10) NULL,
		[WorkspaceID] [int] NOT NULL,
		[SortByID] [int] NOT NULL,
		[ValueTypeID] [int] NOT NULL,
		[IsPercentage] [bit] NULL,
		Processed bit,
		NewWorkspaceVariableID int,
		NewWorkspaceID int
	)
	INSERT INTO @WorkspaceVariable
	SELECT [WorkspaceVariableID]
		  ,[UpdateDT]
		  ,[WorkspaceVariableName]
		  ,[WorkspaceVariableValue]
		  ,[WorkspaceID]
		  ,[SortByID]
		  ,[ValueTypeID]
		  ,[IsPercentage]
		  ,0
		  ,NULL
		  ,@NewWorkspaceID
	  FROM [version].[WorkspaceVariable]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	DECLARE @WorkspaceVariableID INT
	WHILE EXISTS (SELECT 1 FROM @WorkspaceVariable WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @WorkspaceVariableID = WorkspaceVariableID FROM  @WorkspaceVariable WHERE Processed = 0
	INSERT INTO [dbo].[WorkspaceVariable]
			   ([UpdateDT]
			   ,[WorkspaceVariableName]
			   ,[WorkspaceVariableValue]
			   ,[WorkspaceID]
			   ,[SortByID]
			   ,[ValueTypeID]
			   ,[IsPercentage])
	SELECT [UpdateDT]
		  ,[WorkspaceVariableName]
		  ,[WorkspaceVariableValue]
		  ,@NewWorkspaceID
		  ,[SortByID]
		  ,[ValueTypeID]
		  ,[IsPercentage]
	  FROM @WorkspaceVariable WV
	WHERE WorkspaceVariableID = @WorkspaceVariableID

	UPDATE @WorkspaceVariable
	SET	NewWorkspaceVariableID = SCOPE_IDENTITY(),
		Processed = 1
	WHERE WorkspaceVariableID = @WorkspaceVariableID
	END

	INSERT INTO [dbo].[WorkspaceUserRole]
			   ([UpdateDT]
			   ,[ETIUserID]
			   ,[RoleID]
			   ,[WorkspaceID]
			   ,[HideHelp])
	SELECT [UpdateDT]
		  ,[ETIUserID]
		  ,[RoleID]
		  ,@NewWorkspaceID
		  ,[HideHelp]
	  FROM [version].[WorkspaceUserRole]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	DECLARE @ProPricerExport TABLE
	(
		[ProPricerExportID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[ProPricerExportName] [varchar](100) NOT NULL,
		[WorkspaceID] [int] NULL,
		Processed bit,
		NewProPricerExportID int,
		NewWorkspaceID int
	)	
	INSERT INTO @ProPricerExport		
	SELECT [ProPricerExportID]
		  ,[UpdateDT]
		  ,[ProPricerExportName]
		  ,[WorkspaceID]
		  ,0
		  ,NULL
		  ,@NewWorkspaceID
	  FROM [version].[ProPricerExport]
	WHERE WorkspaceID = @WorkspaceID and VersionID = @VersionID

	DECLARE @ProPricerExportID int
	WHILE EXISTS (SELECT 1 FROM @ProPricerExport WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @ProPricerExportID = ProPricerExportID FROM @ProPricerExport WHERE Processed = 0

	INSERT INTO [dbo].[ProPricerExport]
			   ([UpdateDT]
			   ,[ProPricerExportName]
			   ,[WorkspaceID])
	SELECT [UpdateDT]
		  ,[ProPricerExportName]
		  ,@NewWorkspaceID
	  FROM @ProPricerExport
	WHERE ProPricerExportID = @ProPricerExportID

	UPDATE @ProPricerExport
	SET NewProPricerExportID = SCOPE_IDENTITY(),
		Processed = 1
	WHERE ProPricerExportID = @ProPricerExportID

	END	  

	INSERT INTO [dbo].[ProPricerFieldXREF]
			   ([ProPricerExportID]
			   ,[ProPricerFieldID]
			   ,[ProPricerTypeID]
			   ,[ListOrder])
	SELECT P.NewProPricerExportID
		  ,X.[ProPricerFieldID]
		  ,X.[ProPricerTypeID]
		  ,X.[ListOrder]
	  FROM [version].[ProPricerFieldXREF] X
		INNER JOIN @ProPricerExport P ON X.ProPricerExportID = P.ProPricerExportID
	WHERE P.WorkspaceID = @WorkspaceID  and X.VersionID = @VersionID

	INSERT INTO [dbo].[ProPricerCustomFieldXREF]
			   ([ProPricerExportID]
			   ,[CustomFieldID]
			   ,[ProPricerTypeID]
			   ,[ProPricerCustomFieldSelectionID]
			   ,[ListOrder])
	SELECT P.NewProPricerExportID
		  ,CM.NewCustomFieldID
		  ,[ProPricerTypeID]
		  ,[ProPricerCustomFieldSelectionID]
		  ,[ListOrder]
	  FROM [version].[ProPricerCustomFieldXREF] CX
		INNER JOIN @ProPricerExport P ON CX.ProPricerExportID = P.ProPricerExportID
		INNER JOIN @CustomFieldMapping CM ON CX.CustomFieldID = CM.OriginalCustomFieldID
	WHERE CX.VersionID = @VersionID

	DECLARE @CLIN TABLE 
	(
		[CLINID] [int]  NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[CLINNumber] [varchar](459) NULL,
		[CLINTitle] [varchar](100) NULL,
		[CLINStartDate] [date] NULL,
		[CLINEndDate] [date] NULL,
		[ContractTypeID] [int] NULL,
		[WorkspaceID] [int] NOT NULL,
		Processed bit,
		NewCLINID int,
		NewWorkspaceID int,
		[DisplayedCLINNumber] [varchar] (50)
	)	
	INSERT INTO @CLIN
	SELECT [CLINID]
		  ,[UpdateDT]
		  ,[CLINNumber]
		  ,[CLINTitle]
		  ,[CLINStartDate]
		  ,[CLINEndDate]
		  ,[ContractTypeID]
		  ,[WorkspaceID]
		  ,0
		  ,NULL
		  ,@NewWorkspaceID
		  ,[DisplayedCLINNumber]
	  FROM [version].[CLIN]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	DECLARE @CLINID INT
	WHILE EXISTS (SELECT 1 FROM @CLIN WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @CLINID = CLINID FROM @CLIN WHERE Processed = 0
	INSERT INTO [dbo].[CLIN]
			   ([UpdateDT]
			   ,[CLINNumber]
			   ,[CLINTitle]
			   ,[CLINStartDate]
			   ,[CLINEndDate]
			   ,[ContractTypeID]
			   ,[WorkspaceID]
			   ,[DisplayedCLINNumber]
			   )
	SELECT [UpdateDT]
		  ,[CLINNumber]
		  ,[CLINTitle]
		  ,[CLINStartDate]
		  ,[CLINEndDate]
		  ,[ContractTypeID]
		  ,@NewWorkspaceID
		  ,[DisplayedCLINNumber]
	  FROM [version].[CLIN]
	WHERE CLINID = @CLINID AND VersionId = @VersionID

	UPDATE @CLIN
	SET	NewCLINID = SCOPE_IDENTITY(),
		Processed = 1
	WHERE CLINID = @CLINID
	END

	-- Start of "RMS Zone Travel" - MstTravelTrip
	-- These changes are to be executed in RMS only. The way we can tell the environments apart is that SSC has LOBs in the range of 1000's. RMS is 2000+ and ISGS is 0-999
	IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessID > 2000)
	BEGIN
		DECLARE @MstTravelTrip TABLE
		(
			[MSTTravelTripID] [int] NOT NULL,
			[ModeID] [int] NOT NULL,
			[TravelTripTaskElementID] [int] NOT NULL,
			[UpdateDT] [datetime2](7) NOT NULL,
			[GroupID] [int] NULL,
			[SegmentID] [int] NOT NULL,
			[Purpose] [varchar](35) NULL,
			[PerformingOrganizationID] [int] NOT NULL,
			[TripDate] [date] NOT NULL,
			[EstimateDate] [date] NULL,
			[NumPeople] DECIMAL(10,6) NOT NULL,
			[NumDays] DECIMAL(10,6) NOT NULL,
			[ZoneOriginID] [int] NULL,
			[ZoneDestCity] [varchar](35) NULL,
			[ZoneDestinationID] [int] NULL,
			[ZoneResourceID] [int] NULL,
			[NonZoneFrom] [varchar](150) NULL,
			[NonZoneTo] [varchar](150) NULL,
			[NonZoneAirFareEstimate] [money] NULL,
			[NonZonePerDiemDaily] [money] NULL,
			[NonZoneCarRentalTrans] [money] NULL,
			[NonZoneNumCars] DECIMAL(10,6) NULL,
			[NonZoneResourceID] int NULL,
			[ClinId] INT NULL,
			[WbsId] INT NULL,

			NewTravelTripID int, -- this will be the new PK value
			Processed bit DEFAULT 0, -- indicating whether the item was processed yet
			NewTravelTripTaskElementID int -- new parent element id
		)
		INSERT INTO @MstTravelTrip
			SELECT 
				 tt.[MSTTravelTripID]
				,tt.[ModeID]
				,tt.[TravelTripTaskElementID]
				,tt.[UpdateDT]
				,tt.[GroupID]
				,tt.[SegmentID]
				,tt.[Purpose]
				,CASE
					WHEN PO.NewPerformingOrganizationID IS NOT NULL THEN PO.NewPerformingOrganizationID
					ELSE tt.[PerformingOrganizationID]
					END AS PerformingOrganizationID
				,tt.[TripDate]
				,tt.[EstimateDate]
				,tt.[NumPeople]
				,tt.[NumDays]
				,tt.[ZoneOriginID]
				,tt.[ZoneDestCity]
				,tt.[ZoneDestinationID]
				,tt.[ZoneResourceID]
				,tt.[NonZoneFrom]
				,tt.[NonZoneTo]
				,tt.[NonZoneAirFareEstimate]
				,tt.[NonZonePerDiemDaily]
				,tt.[NonZoneCarRentalTrans]
				,tt.[NonZoneNumCars]
				,tt.[NonZoneResourceID]
				,tt.[ClinId]
				,tt.[WbsId]
				,NULL -- new PK value
				,0 -- not processed yet
				,TE.NewTravelTripTaskElementID -- the new parent element id
			FROM [version].[MSTTravelTrip] tt -- table containing data to copy
				INNER JOIN @TravelTripTaskElement TE -- joining w/ the already copied parent element (so we can get the correct IDs)
					ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
				LEFT OUTER JOIN @PerformingOrganization PO ON tt.PerformingOrganizationID = PO.PerformingOrganizationID
			WHERE tt.VersionID = @VersionID


		-- Update Wbs Ids based on the new IDs
		UPDATE @MstTravelTrip
			SET WbsId = W.NewWBSID
			FROM  @MstTravelTrip tt INNER JOIN @WorkBreakdownStructure W ON tt.WbsId = W.WBSID

		-- Update Clin Ids based on the new IDs
		UPDATE @MstTravelTrip
			SET ClinId = C.NewCLINID
			FROM  @MstTravelTrip tt INNER JOIN @CLIN C ON tt.ClinId = C.CLINID

		-- Update Resource Ids based on the new IDs
		UPDATE @MstTravelTrip
			SET NonZoneResourceID = R.NewResourceID
			FROM @MstTravelTrip tt INNER JOIN @Resource R ON tt.NonZoneResourceID = R.ResourceID

		DECLARE @MstTravelTripID int
		WHILE EXISTS (SELECT 1 FROM @MstTravelTrip WHERE Processed = 0)
		BEGIN
			SELECT TOP 1 @MstTravelTripID = [MSTTravelTripID] FROM @MstTravelTrip WHERE Processed = 0

			INSERT INTO [dbo].[MSTTravelTrip]
				([ModeID]
				,[TravelTripTaskElementID]
				,[UpdateDT]
				,[GroupID]
				,[SegmentID]
				,[Purpose]
				,[PerformingOrganizationID]
				,[TripDate]
				,[EstimateDate]
				,[NumPeople]
				,[NumDays]
				,[ZoneOriginID]
				,[ZoneDestCity]
				,[ZoneDestinationID]
				,[ZoneResourceID]
				,[NonZoneFrom]
				,[NonZoneTo]
				,[NonZoneAirFareEstimate]
				,[NonZonePerDiemDaily]
				,[NonZoneCarRentalTrans]
				,[NonZoneNumCars]
				,[NonZoneResourceID]
				,[ClinId]
				,[WbsId]
				)
			SELECT
				 tt.[ModeID]
				,tt.NewTravelTripTaskElementID -- the new parent id
				,tt.[UpdateDT]
				,tt.[GroupID]
				,tt.[SegmentID]
				,tt.[Purpose]
				,tt.[PerformingOrganizationID]
				,tt.[TripDate]
				,tt.[EstimateDate]
				,tt.[NumPeople]
				,tt.[NumDays]
				,tt.[ZoneOriginID]
				,tt.[ZoneDestCity]
				,tt.[ZoneDestinationID]
				,tt.[ZoneResourceID]
				,tt.[NonZoneFrom]
				,tt.[NonZoneTo]
				,tt.[NonZoneAirFareEstimate]
				,tt.[NonZonePerDiemDaily]
				,tt.[NonZoneCarRentalTrans]
				,tt.[NonZoneNumCars]
				,tt.[NonZoneResourceID]
				,tt.[ClinId]
				,tt.[WbsId]
				FROM @MstTravelTrip tt
					WHERE [MSTTravelTripID] = @MstTravelTripID  

			UPDATE @MstTravelTrip
				SET Processed = 1, NewTravelTripID = SCOPE_IDENTITY()			
				WHERE [MSTTravelTripID] = @MstTravelTripID 
		END
	END
	-- End of "RMS Zone Travel" - MstTravelTrip

	DECLARE @WBS_CLIN_BOE_XREF TABLE
	(
		[WBSID] [int] NULL,
		[CLINID] [int] NULL,
		[BOEID] [int] NULL,
		NewWBSID int,
		NewClinID int,
		NewBOEID int
	)	
	INSERT INTO @WBS_CLIN_BOE_XREF ([WBSID],[CLINID],[BOEID])
	SELECT  X.[WBSID], X.[CLINID], X.[BOEID]
	FROM [version].[WBS_CLIN_BOE_XREF] X 
	INNER JOIN @WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID
	WHERE X.VersionID = @VersionID
	UNION
	SELECT  X.[WBSID], X.[CLINID], X.[BOEID]
	FROM [version].[WBS_CLIN_BOE_XREF] X 
	INNER JOIN @CLIN C ON X.CLINID = C.CLINID 
	WHERE X.VersionID = @VersionID
	UNION
	SELECT  X.[WBSID], X.[CLINID], X.[BOEID]
	FROM [version].[WBS_CLIN_BOE_XREF] X 
	INNER JOIN @BOE B ON X.BOEID = B.BOEID
	WHERE X.VersionID = @VersionID

	UPDATE @WBS_CLIN_BOE_XREF
	SET NewWBSID = W.NewWBSID
	FROM  @WBS_CLIN_BOE_XREF X
		INNER JOIN @WorkBreakdownStructure W ON X.WBSID = W.WBSID

	UPDATE @WBS_CLIN_BOE_XREF
	SET NewCLINID = C.NewCLINID
	FROM  @WBS_CLIN_BOE_XREF X
		INNER JOIN @CLIN C ON X.CLINID = C.CLINID

	UPDATE @WBS_CLIN_BOE_XREF
	SET NewBOEID = B.NewBOEID
	FROM  @WBS_CLIN_BOE_XREF X
		INNER JOIN @BOE B ON X.BOEID = B.BOEID

	INSERT INTO [dbo].[WBS_CLIN_BOE_XREF]
			   ([WBSID]
			   ,[CLINID]
			   ,[BOEID])
	SELECT DISTINCT NewWBSID, NewCLINID, NewBOEID
	FROM @WBS_CLIN_BOE_XREF

	DECLARE @SumOfBOE_WorkspaceVariableXREF TABLE 
	(
		[WVSumID] [bigint] NOT NULL,
		[WorkspaceVariableID] [int] NOT NULL,
		[CLINID] [int] NULL,
		[WBSID] [int] NULL,
		[BOEID] [int] NULL,
		NewWorkspaceVariableID int,
		NewCLINID int,
		NewWBSID int,
		NewBOEID int
	)
	INSERT INTO @SumOfBOE_WorkspaceVariableXREF
	SELECT X.[WVSumID]
		  ,X.[WorkspaceVariableID]
		  ,X.[CLINID]
		  ,X.[WBSID]
		  ,X.[BOEID]
		  ,WV.NewWorkspaceVariableID
		  ,NULL
		  ,NULL
		  ,NULL
	  FROM [version].[SumOfBOE_WorkspaceVariableXREF] X
	INNER JOIN @WorkspaceVariable WV ON X.WorkspaceVariableID = WV.WorkspaceVariableID
	WHERE X.VersionId = @VersionID

	UPDATE @SumOfBOE_WorkspaceVariableXREF
	SET NewWBSID = W.NewWBSID
	FROM  @SumOfBOE_WorkspaceVariableXREF X
		INNER JOIN @WorkBreakdownStructure W ON X.WBSID = W.WBSID

	UPDATE @SumOfBOE_WorkspaceVariableXREF
	SET NewCLINID = C.NewCLINID
	FROM  @SumOfBOE_WorkspaceVariableXREF X
		INNER JOIN @CLIN C ON X.CLINID = C.CLINID

	UPDATE @SumOfBOE_WorkspaceVariableXREF
	SET NewBOEID = B.NewBOEID
	FROM  @SumOfBOE_WorkspaceVariableXREF X
		INNER JOIN @BOE B ON X.BOEID = B.BOEID

	INSERT INTO [dbo].[SumOfBOE_WorkspaceVariableXREF]
			   ([WorkspaceVariableID]
			   ,[CLINID]
			   ,[WBSID]
			   ,[BOEID])
	SELECT 
		NewWorkspaceVariableID,
		NewCLINID,
		NewWBSID,
		NewBOEID
	FROM @SumOfBOE_WorkspaceVariableXREF

	INSERT INTO [dbo].[WorkspaceVariableSumVariableResourceTypeXREF]
			   ([WorkspaceVariableID]
			   ,[SumVariableResourceTypeID])
	SELECT WV.NewWorkspaceVariableID
		  ,[SumVariableResourceTypeID]
	  FROM [version].[WorkspaceVariableSumVariableResourceTypeXREF] X
	  INNER JOIN @WorkspaceVariable WV ON X.WorkspaceVariableID = WV.WorkspaceVariableID
	WHERE X.VersionId = @VersionID

	INSERT INTO [dbo].[BOEStateHistory]
			   ([UpdateDT]
			   ,[BOEID]
			   ,[FieldID]
			   ,[CurrentBOEStateID]
			   ,[UpdatedBOEStateID]
			   ,[ChangedByETIUserID])
	SELECT BH.[UpdateDT]
		  ,B.NewBOEID
		  ,BH.[FieldID]
		  ,BH.[CurrentBOEStateID]
		  ,BH.[UpdatedBOEStateID]
		  ,BH.[ChangedByETIUserID]
	  FROM [version].[BOEStateHistory] BH
	INNER JOIN @BOE B ON BH.BOEID = B.BOEID
	WHERE BH.VersionID = @VersionID

	INSERT INTO [dbo].[BOEUserRoleHistory]
			   ([UpdateDT]
			   ,[CurrentETIUserID]
			   ,[UpdatedETIUserID]
			   ,[RoleID]
			   ,[BOEID]
			   ,[FieldID]
			   ,[ChangedByETIUserID])
	SELECT H.[UpdateDT]
		  ,H.[CurrentETIUserID]
		  ,H.[UpdatedETIUserID]
		  ,H.[RoleID]
		  ,NewBOEID
		  ,H.[FieldID]
		  ,H.[ChangedByETIUserID]
	  FROM [version].[BOEUserRoleHistory] H
	INNER JOIN @BOE B ON H.BOEID = B.BOEID
	WHERE H.VersionID = @VersionID

	INSERT INTO [dbo].[BOEUserRole]
			   ([UpdateDT]
			   ,[ETIUserID]
			   ,[RoleID]
			   ,[BOEID])
	SELECT R.[UpdateDT]
		  ,R.[ETIUserID]
		  ,R.[RoleID]
		  ,NewBOEID
	  FROM [version].[BOEUserRole] R
	INNER JOIN @BOE B ON R.BOEID = B.BOEID
	WHERE R.VersionID = @VersionID

	INSERT INTO [dbo].[BOEApproval]
			   ([UpdateDT]
			   ,[BOEID]
			   ,[ApprovalETIUserID]
			   ,[ApprovedFlag])
	SELECT B.[UpdateDT]
		  ,tB.NewBOEID
		  ,B.[ApprovalETIUserID]
		  ,B.[ApprovedFlag]
	  FROM [version].[BOEApproval] B
	INNER JOIN @BOE tB ON B.BOEID = tB.BOEID
	WHERE B.VersionID = @VersionID

	DECLARE @BOEComment  TABLE
	(
		[BOECommentID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[FieldID] [int] NOT NULL,
		[BOEComments] [varchar](500) NOT NULL,
		[BOECommentETIUserID] [int] NOT NULL,
		[BOEResponseToCommentID] [int] NULL,
		[BOEID] [int] NOT NULL,
		Processed bit,
		NewBOECommentID [int],
		NewBOEID int
	)
	INSERT INTO @BOEComment
	SELECT B.[BOECommentID]
		  ,B.[UpdateDT]
		  ,B.[FieldID]
		  ,B.[BOEComments]
		  ,B.[BOECommentETIUserID]
		  ,B.[BOEResponseToCommentID]
		  ,B.[BOEID]
		  ,0
		  ,NULL
		  ,tB.NewBOEID
	  FROM [version].[BOEComment] B
	INNER JOIN @BOE tB ON B.BOEID = tB.BOEID
	WHERE B.VersionID = @VersionID

	DECLARE @BOECommentID int
	WHILE EXISTS (SELECT 1 FROM @BOEComment WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @BOECommentID = BOECommentID 
		FROM @BOEComment 
		WHERE Processed = 0
		ORDER BY BOECommentID ASC

	INSERT INTO [dbo].[BOEComment]
			   ([UpdateDT]
			   ,[FieldID]
			   ,[BOEComments]
			   ,[BOECommentETIUserID]
			   ,[BOEResponseToCommentID]
			   ,[BOEID])
	SELECT BC.[UpdateDT]
		  ,BC.[FieldID]
		  ,BC.[BOEComments]
		  ,BC.[BOECommentETIUserID]
		  ,tBC.NewBOECommentID
		  ,BC.NewBOEID
	  FROM @BOEComment BC
		LEFT OUTER JOIN @BOEComment tBC ON BC.BOEResponseToCommentID = tBC.BOECommentID
	WHERE BC.BOECommentID = @BOECommentID  

	UPDATE @BOEComment
	SET NewBOECommentID = SCOPE_IDENTITY(),	
		Processed = 1
	WHERE BOECommentID = @BOECommentID  

	END

	INSERT INTO [dbo].[BOECommentHistory]
			   ([UpdateDT]
			   ,[BOECommentID]
			   ,[BOEID]
			   ,[FieldID]
			   ,[CurrentComment]
			   ,[UpdatedComment]
			   ,[ChangedByETIUserID])
	SELECT BCH.[UpdateDT]
		  ,tBC.NewBOECommentID
		  ,tBC.NewBOEID
		  ,BCH.[FieldID]
		  ,BCH.[CurrentComment]
		  ,BCH.[UpdatedComment]
		  ,BCH.[ChangedByETIUserID]
	  FROM [version].[BOECommentHistory] BCH
		INNER JOIN @BOEComment tBC ON BCH.BOECommentID = tBC.BOECommentID
	  WHERE BCH.VersionID = @VersionID

	  DECLARE @BOETaskElement TABLE 
	(
		[BOETaskElementID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[TaskID] [varchar](3) NULL,
		[TaskTitle] [varchar](100) NOT NULL,
		[TaskDescription] [varchar](max) NULL,
		[TaskStartDate] [date] NULL,
		[TaskEndDate] [date] NULL,
		[MOQHoursEquation] [varchar](500) NULL,
		[MOQCostEquation] [varchar](250) NULL,
		[MOQText] [varchar](max) NULL,
		[MOQTypeID] [int] NULL,
		[BOEID] [int] NULL,
		[LaborTypeWarningFlag] [bit] NOT NULL,
		[IMS_ID] [varchar](20) NULL,
		[TaskElementTypeID] [int] NOT NULL,
		Processed bit,
		NewBOETaskElementID int,
		NewBOEID int,
		[SortOrderID] INT
	)	
	INSERT INTO @BOETaskElement
	SELECT TE.[BOETaskElementID]
		  ,TE.[UpdateDT]
		  ,TE.[TaskID]
		  ,TE.[TaskTitle]
		  ,TE.[TaskDescription]
		  ,TE.[TaskStartDate]
		  ,TE.[TaskEndDate]
		  ,TE.[MOQHoursEquation]
		  ,TE.[MOQCostEquation]
		  ,TE.[MOQText]
		  ,TE.[MOQTypeID]
		  ,B.NewBOEID
		  ,TE.[LaborTypeWarningFlag]
		  ,TE.[IMS_ID]
		  ,TE.[TaskElementTypeID]
		  ,0
		  ,NULL
		  ,B.NewBOEID
		  ,TE.[SortOrderID]
	  FROM [version].[BOETaskElement] TE
		INNER JOIN @BOE B ON TE.BOEID = B.BOEID
	  WHERE TE.VersionID = @VersionID

	DECLARE @WSVar TABLE
	(
	MOQHoursEquation varchar (500),
	Original varchar(100),
	Updated varchar(100),
	OriginalID int,
	UpdatedID int,
	BOETaskElementID int
	)
	INSERT INTO @WSVar (MOQHoursEquation, BOETaskElementID)
	SELECT MOQHoursEquation, BOETaskElementID  
	FROM @BOETaskElement 
	WHERE MOQHoursEquation LIKE '%<WSVAR:%'

	WHILE EXISTS (SELECT 1 FROM @WSVar WHERE MOQHoursEquation LIKE '%<WSVAR:%')
	BEGIN
	UPDATE @WSVar
	SET Original = 
	SUBSTRING 
		(
			MOQHoursEquation,
			CHARINDEX ('<WSVAR:',MOQHoursEquation),
			(CHARINDEX ('>',MOQHoursEquation) - CHARINDEX ('<WSVAR:',MOQHoursEquation) + 1)
		)

	UPDATE @WSVar
	SET OriginalID =
	 REPLACE (RIGHT (Original,
		(LEN (Original) - CHARINDEX (':',Original))),
		'>', '')


	UPDATE @WSVar
	SET UpdatedID = WS.NewWorkspaceVariableID
	FROM @WSVar t
		INNER JOIN @WorkspaceVariable WS ON t.OriginalID = WS.WorkspaceVariableID


	UPDATE @WSVar
	SET Updated = 
	REPLACE (Original, OriginalID, UpdatedID) 


	UPDATE @BOETaskElement
	SET MOQHoursEquation = 
	REPLACE (TE.MOQHoursEquation, V.Original, V.Updated)
	FROM @BOETaskElement TE
		INNER JOIN @WSVar V ON TE.BOETaskElementID = V. BOETaskElementID

	UPDATE @WSVar 
	SET MOQHoursEquation = REPLACE (MOQHoursEquation, Original, '')

	END

	DECLARE @BOETaskElementID int
	WHILE EXISTS (SELECT 1 FROM @BOETaskElement WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @BOETaskElementID = BOETaskElementID FROM @BOETaskElement WHERE Processed = 0
	INSERT INTO [dbo].[BOETaskElement]
			   ([UpdateDT]
			   ,[TaskID]
			   ,[TaskTitle]
			   ,[TaskDescription]
			   ,[TaskStartDate]
			   ,[TaskEndDate]
			   ,[MOQHoursEquation]
			   ,[MOQCostEquation]
			   ,[MOQText]
			   ,[MOQTypeID]
			   ,[BOEID]
			   ,[LaborTypeWarningFlag]
			   ,[IMS_ID]
			   ,[TaskElementTypeID]
			   ,[SortOrderID]
			   )
	 SELECT [UpdateDT]
		  ,[TaskID]
		  ,[TaskTitle]
		  ,[TaskDescription]
		  ,[TaskStartDate]
		  ,[TaskEndDate]
		  ,[MOQHoursEquation]
		  ,[MOQCostEquation]
		  ,[MOQText]
		  ,[MOQTypeID]
		  ,NewBOEID
		  ,[LaborTypeWarningFlag]
		  ,[IMS_ID]
		  ,[TaskElementTypeID]
		  ,[SortOrderID]
	  FROM @BOETaskElement
	WHERE BOETaskElementID = @BOETaskElementID
	
	UPDATE @BOETaskElement
	SET NewBOETaskElementID = SCOPE_IDENTITY(),
		Processed = 1
	WHERE BOETaskElementID = @BOETaskElementID

	END	

	DECLARE @OrdinaryVariable TABLE
	(
		[OrdinaryVariableID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[OrdinaryVariableName] [varchar](50) NOT NULL,
		[OrdinaryVariableValue] [decimal](29, 10) NULL,
		[BOETaskElementID] [int] NOT NULL,
		[SortByID] [int] NOT NULL,
		[ValueTypeID] [int] NOT NULL,
		[IsPercentage] [bit] NULL,
		Processed bit,
		NewOrdinaryVariableID int,
		NewBOETaskElementID int,
		[DefaultSize] varchar(200)
	)
	INSERT INTO @OrdinaryVariable	
	SELECT OV.[OrdinaryVariableID]
		  ,OV.[UpdateDT]
		  ,OV.[OrdinaryVariableName]
		  ,OV.[OrdinaryVariableValue]
		  ,OV.[BOETaskElementID]
		  ,OV.[SortByID]
		  ,OV.[ValueTypeID]
		  ,OV.[IsPercentage]
		  ,0
		  ,NULL
		  ,TE.NewBOETaskElementID
		  ,OV.[DefaultSize]
	  FROM [version].[OrdinaryVariable] OV
	INNER JOIN @BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
	WHERE OV.VersionID = @VersionID

	DECLARE @OrdinaryVariableID int
	WHILE EXISTS (SELECT 1 FROM @OrdinaryVariable WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @OrdinaryVariableID = OrdinaryVariableID FROM @OrdinaryVariable WHERE Processed = 0
	INSERT INTO [dbo].[OrdinaryVariable]
			   ([UpdateDT]
			   ,[OrdinaryVariableName]
			   ,[OrdinaryVariableValue]
			   ,[BOETaskElementID]
			   ,[SortByID]
			   ,[ValueTypeID]
			   ,[IsPercentage]
			   ,[DefaultSize]
			   )
	SELECT [UpdateDT]
		  ,[OrdinaryVariableName]
		  ,[OrdinaryVariableValue]
		  ,NewBOETaskElementID
		  ,[SortByID]
		  ,[ValueTypeID]
		  ,[IsPercentage]
		  ,[DefaultSize]
	  FROM @OrdinaryVariable 
	WHERE OrdinaryVariableID = @OrdinaryVariableID  

	UPDATE @OrdinaryVariable
	SET NewOrdinaryVariableID = SCOPE_IDENTITY(),
		Processed = 1
	WHERE OrdinaryVariableID = @OrdinaryVariableID

	END

	INSERT INTO [dbo].[BOEApprovalHistory]
			   ([UpdateDT]
			   ,[BOEID]
			   ,[Approval]
			   ,[ApprovalETIUserID])
	SELECT H.[UpdateDT]
		  ,B.NewBOEID
		  ,H.[Approval]
		  ,H.[ApprovalETIUserID]
	  FROM [version].[BOEApprovalHistory] H
	INNER JOIN @BOE B ON H.BOEID = B.BOEID
	WHERE H.VersionID = @VersionID

	/** [dbo].[MOQTypeSelection] **/
	DECLARE @MOQTypeSelection TABLE
	(
		[MOQTypeSelectionId] [int] NOT NULL,
		[TaskId] [int] NOT NULL,
		[MOQTypeSelection] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[Order] [int] NOT NULL,
		[CERName] [varchar](255) NULL,
		[HoursDescription] [varchar](max) NULL,
		[SubjectMatterExpert] [varchar](max) NULL,
		[HoursLogicAndAssumptions] [varchar](max) NULL,
		[DurationLogicAndAssumptions] [varchar](max) NULL,
		[EstimateTasks] [varchar](max) NULL,
		[Rationale] [varchar](max) NULL,
		[SkillMix] [varchar](max) NULL,
		Processed bit,
		NewMOQTypeSelectionId int,
		NewTaskId int
	)
	INSERT INTO @MOQTypeSelection
	SELECT
		M.[MOQTypeSelectionId],
		M.[TaskId],
		M.[MOQTypeSelection],
		M.[UpdateDT],
		M.[Order],
		M.[CERName],
		M.[HoursDescription],
		M.[SubjectMatterExpert],
		M.[HoursLogicAndAssumptions],
		M.[DurationLogicAndAssumptions],
		M.[EstimateTasks],
		M.[Rationale],
		M.[SkillMix],
		0,
		NULL,
		T.NewBOETaskElementID
	FROM [version].[MOQTypeSelection] M
	INNER JOIN @BOETaskElement T ON M.TaskId = T.BOETaskElementID
	WHERE M.VersionId = @VersionID

	DECLARE @MOQTypeSelectionId int
	WHILE EXISTS (SELECT 1 FROM @MOQTypeSelection WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @MOQTypeSelectionId = MOQTypeSelectionId FROM @MOQTypeSelection WHERE Processed = 0
	INSERT INTO [dbo].[MOQTypeSelection]
				([TaskId],
				[MOQTypeSelection],
				[UpdateDT],
				[Order],
				[CERName],
				[HoursDescription],
				[SubjectMatterExpert],
				[HoursLogicAndAssumptions],
				[DurationLogicAndAssumptions],
				[EstimateTasks],
				[Rationale],
				[SkillMix]
				)
	SELECT NewTaskId,
		[MOQTypeSelection],
		[UpdateDT],
		[Order],
		[CERName],
		[HoursDescription],
		[SubjectMatterExpert],
		[HoursLogicAndAssumptions],
		[DurationLogicAndAssumptions],
		[EstimateTasks],
		[Rationale],
		[SkillMix]
	FROM @MOQTypeSelection
	WHERE MOQTypeSelectionId = @MOQTypeSelectionId

	UPDATE @MOQTypeSelection
	SET NewMOQTypeSelectionId = SCOPE_IDENTITY(),
		Processed = 1
	WHERE MOQTypeSelectionId = @MOQTypeSelectionId

	END

	/** [dbo].[MOQTypeSelectionTableData] **/
	DECLARE @MOQTypeSelectionTableData TABLE
	(
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
		Processed bit,
		NewMOQTypeSelectionTableDataId int,
		NewMOQTypeSelectionId int
	)
	INSERT INTO @MOQTypeSelectionTableData
	SELECT
		TD.[MOQTypeSelectionTableDataId],
		TD.[MOQTypeSelectionId],
		TD.[UpdateDT],
		TD.[Order],
		TD.[TableName],
		TD.[RepositoryName],
		TD.[QueryType],
		TD.[DateOfReport],
		TD.[HistoricalProgramName],
		TD.[ContractNumber],
		TD.[WbsElement],
		TD.[PeriodOfPerformanceStartDate],
		TD.[PeriodOfPerformanceEndDate],
		TD.[TotalWbsHours],
		TD.[AdditionalQueryFilters],
		TD.[TotalRelevantHoursAfterQueryFilters],
		0,
		NULL,
		S.NewMOQTypeSelectionId
	FROM [version].[MOQTypeSelectionTableData] TD
	INNER JOIN @MOQTypeSelection S ON TD.MOQTypeSelectionId = S.MOQTypeSelectionId
	WHERE TD.VersionId = @VersionID

	DECLARE @MOQTypeSelectionTableDataId int
	WHILE EXISTS (SELECT 1 FROM @MOQTypeSelectionTableData WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @MOQTypeSelectionTableDataId = MOQTypeSelectionTableDataId FROM @MOQTypeSelectionTableData WHERE Processed = 0
	INSERT INTO [dbo].[MOQTypeSelectionTableData]
				([MOQTypeSelectionId],
				[UpdateDT],
				[Order],
				[TableName],
				[RepositoryName],
				[QueryType],
				[DateOfReport],
				[HistoricalProgramName],
				[ContractNumber],
				[WbsElement],
				[PeriodOfPerformanceStartDate],
				[PeriodOfPerformanceEndDate],
				[TotalWbsHours],
				[AdditionalQueryFilters],
				[TotalRelevantHoursAfterQueryFilters]
				)
	SELECT NewMOQTypeSelectionId,
		[UpdateDT],
		[Order],
		[TableName],
		[RepositoryName],
		[QueryType],
		[DateOfReport],
		[HistoricalProgramName],
		[ContractNumber],
		[WbsElement],
		[PeriodOfPerformanceStartDate],
		[PeriodOfPerformanceEndDate],
		[TotalWbsHours],
		[AdditionalQueryFilters],
		[TotalRelevantHoursAfterQueryFilters]
	FROM @MOQTypeSelectionTableData
	WHERE MOQTypeSelectionTableDataId = @MOQTypeSelectionTableDataId

	UPDATE @MOQTypeSelectionTableData
	SET NewMOQTypeSelectionTableDataId = SCOPE_IDENTITY(),
		Processed = 1
	WHERE MOQTypeSelectionTableDataId = @MOQTypeSelectionTableDataId

	END


	DECLARE @BOELaborType TABLE 
	(
		[BOELaborTypeID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[ResourceID] [int] NULL,
		[PerformingOrganizationID] [int] NULL,
		[BOELaborTypeStartDate] [date] NOT NULL,
		[BOELaborTypeEndDate] [date] NOT NULL,
		[SpreadCurveID] [int] NOT NULL,
		[PercentSpread] [decimal](38, 6) NULL,
		[ValueSpread] [DECIMAL] (18, 6) NULL,
		[BOETaskElementID] [int] NULL,
		[SpreadTypeID] [int] NULL,
		[PercentSpreadLocked] [bit] NOT NULL,
		[HourSpreadLocked] [bit] NOT NULL,
		[WBSID] [int] NULL,
		[CLINID] [int] NULL,
		[CanOffload] bit default 0,
		[LaborSortId] [int] NOT NULL,
		Processed bit,
		[NewBOELaborTypeID] [int],
		[NewResourceID] [int],
		[NewPerformingOrganizationID] [int],
		[NewBOETaskElementID] [int],
		[NewWBSID] [int] NULL,
		[NewCLINID] [int] NULL
	)	
	INSERT INTO @BOELaborType
	SELECT LT.[BOELaborTypeID]
		  ,LT.[UpdateDT]
		  ,LT.[ResourceID]
		  ,LT.[PerformingOrganizationID]
		  ,LT.[BOELaborTypeStartDate]
		  ,LT.[BOELaborTypeEndDate]
		  ,LT.[SpreadCurveID]
		  ,LT.[PercentSpread]
		  ,LT.[ValueSpread]
		  ,LT.[BOETaskElementID]
		  ,LT.[SpreadTypeID]
		  ,LT.[PercentSpreadLocked]
		  ,LT.[HourSpreadLocked]
		  ,LT.[WBSID]
		  ,LT.[CLINID]
		  ,LT.[CanOffload]
		  ,LT.[LaborSortId]
		  ,0/*PROCESSED*/
		  ,NULL
		  ,CASE
			WHEN R.NewResourceID IS NOT NULL THEN R.NewResourceID
			ELSE LT.[ResourceID]
			END AS ResourceID
		  ,CASE
			WHEN PO.NewPerformingOrganizationID IS NOT NULL THEN PO.NewPerformingOrganizationID
			ELSE LT.[PerformingOrganizationID]
			END AS PerformingOrganizationID
		  ,TE.NewBOETaskElementID
		  ,CASE
			WHEN W.NewWBSID IS NOT NULL THEN W.NewWBSID
			ELSE LT.[WBSID]
			END AS WBSID
		  ,CASE
			WHEN C.NewCLINID IS NOT NULL THEN C.NewCLINID
			ELSE LT.[CLINID]
			END AS CLINID
	  FROM [version].[BOELaborType] LT
	INNER JOIN @BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
	LEFT OUTER JOIN @Resource R ON LT.ResourceID = R.ResourceID
	LEFT OUTER JOIN @PerformingOrganization PO ON LT.PerformingOrganizationID = PO.PerformingOrganizationID
	LEFT OUTER JOIN @WorkBreakdownStructure W on LT.WBSID = W.WBSID
	LEFT OUTER JOIN @CLIN C on LT.CLINID = C.CLINID
	WHERE LT.VersionID = @VersionID
	
	DECLARE @BOELaborTypeID int
	WHILE EXISTS (SELECT 1 FROM @BOELaborType WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @BOELaborTypeID = BOELaborTypeID FROM @BOELaborType WHERE Processed = 0

	INSERT INTO [dbo].[BOELaborType]
			   ([UpdateDT]
			   ,[ResourceID]
			   ,[PerformingOrganizationID]
			   ,[BOELaborTypeStartDate]
			   ,[BOELaborTypeEndDate]
			   ,[SpreadCurveID]
			   ,[PercentSpread]
			   ,[ValueSpread]
			   ,[BOETaskElementID]
			   ,[SpreadTypeID]
			   ,[PercentSpreadLocked]
			   ,[HourSpreadLocked]
			   ,[WBSID]
			   ,[CLINID]
			   ,[CanOffload]
			   ,[LaborSortId])
	SELECT [UpdateDT]
		  ,CASE 
		  WHEN NewResourceID IS NOT NULL THEN NewResourceID
		  ELSE ResourceID
		  END AS [ResourceID]      
		  ,CASE
			WHEN NewPerformingOrganizationID IS NOT NULL THEN NewPerformingOrganizationID
			ELSE [PerformingOrganizationID]
			END AS PerformingOrganizationID
		  ,[BOELaborTypeStartDate]
		  ,[BOELaborTypeEndDate]
		  ,[SpreadCurveID]
		  ,[PercentSpread]
		  ,[ValueSpread]
		  ,NewBOETaskElementID
		  ,[SpreadTypeID]
		  ,[PercentSpreadLocked]
		  ,[HourSpreadLocked]
		  ,CASE
			WHEN NewWBSID IS NOT NULL THEN NewWBSID
			ELSE WBSID
			END AS WBSID
		  ,CASE
			WHEN NewCLINID IS NOT NULL THEN NewCLINID
			ELSE CLINID
			END AS CLINID
			,[CanOffload]
			,[LaborSortId]
	  FROM @BOELaborType
	WHERE  [BOELaborTypeID] = @BOELaborTypeID
      
	UPDATE @BOELaborType
	SET [NewBOELaborTypeID] = SCOPE_IDENTITY(),
		Processed = 1
	WHERE  [BOELaborTypeID] = @BOELaborTypeID

	END	

	INSERT INTO [dbo].[BOETaskElementWorkspaceVariableXREF]
			   ([BOETaskElementID]
			   ,[WorkspaceVariableID])
	SELECT TE.NewBOETaskElementID
		  ,WV.NewWorkspaceVariableID
	  FROM [version].[BOETaskElementWorkspaceVariableXREF] X
		INNER JOIN @BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
		INNER JOIN @WorkspaceVariable WV ON X.WorkspaceVariableID = WV.WorkspaceVariableID
	  WHERE X.VersionId = @VersionID
	INSERT INTO [dbo].[BOETaskElementMetricDetailXREF]
	(
	 [BOETaskElementID]
	,[MetricDetailID]
	,[UpdateDT]
	)
	SELECT 
	 TE.NewBOETaskElementID
	,X.[MetricDetailID]
	,X.[UpdateDT]
	FROM [version].[BOETaskElementMetricDetailXREF] X
		INNER JOIN @BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
	WHERE X.VersionId = @VersionID
	INSERT INTO [dbo].[BOETaskElementCustomFieldValueXREF]
			   ([UpdateDT]
			   ,[BOETaskElementID]
			   ,[CustomFieldValueID])
	SELECT X.[UpdateDT]
		  ,TE.NewBOETaskElementID
		  ,CFV.NewCustomFieldValueID
	  FROM [version].[BOETaskElementCustomFieldValueXREF] X
		INNER JOIN @BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
		INNER JOIN @CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID
	  WHERE X.VersionId = @VersionID
	INSERT INTO [dbo].[BOELaborTypeCustomFieldValueXREF]
			   ([UpdateDT]
			   ,[BOELaborTypeID]
			   ,[CustomFieldValueID])
	SELECT X.[UpdateDT]
		  ,TE.NewBOELaborTypeID
		  ,CFV.NewCustomFieldValueID
	  FROM [version].[BOELaborTypeCustomFieldValueXREF] X
		INNER JOIN @BOELaborType TE ON X.BOELaborTypeID = TE.BOELaborTypeID
		INNER JOIN @CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID
	  WHERE X.VersionId = @VersionID
	INSERT INTO [dbo].[BOECustomFieldValueXREF]
			   ([UpdateDT]
			   ,[BOEID]
			   ,[CustomFieldValueID])
	SELECT X.[UpdateDT]
		  ,B.NewBOEID
		  ,CFV.NewCustomFieldValueID
	  FROM [version].[BOECustomFieldValueXREF] X
		INNER JOIN @BOE B ON X.BOEID = B.BOEID
		INNER JOIN @CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID
	  WHERE X.VersionId = @VersionID

	-- These changes are to be executed in RMS only. The way we can tell the environments apart is that SSC has LOBs in the range of 1000's. RMS is 2000+ and ISGS is 0-999
	IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessID > 2000)
	BEGIN
		INSERT INTO [dbo].[MSTTravelTripCustomFieldValueXREF]
				   ([MSTTravelTripID]
				   ,[MSTCustomFieldValueID]
				   ,[UpdateDT])
		SELECT TE.NewTravelTripID
			  ,CFV.NewCustomFieldValueID
			  ,X.[UpdateDT]
		  FROM [version].[MSTTravelTripCustomFieldValueXREF] X
			INNER JOIN @MstTravelTrip TE ON X.MSTTravelTripID = TE.MSTTravelTripID
			INNER JOIN @CustomFieldValue CFV ON X.MSTCustomFieldValueID = CFV.CustomFieldValueID
		WHERE X.VersionId = @VersionID

		INSERT INTO [dbo].[TravelTripTaskElementCustomFieldValueXREF]
			   ([TravelTripTaskElementID]
			   ,[CustomFieldValueID]
			   ,[UpdateDT])
	SELECT TE.NewTravelTripTaskElementID
		  ,CFV.NewCustomFieldValueID
		  ,X.[UpdateDT]
	  FROM [version].[TravelTripTaskElementCustomFieldValueXREF] X
		INNER JOIN @TravelTripTaskElement TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
		INNER JOIN @CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID
	  WHERE X.VersionId = @VersionID
	END

	/**** RTE Templates ****/

	INSERT INTO [dbo].[RteTemplate]
				([UpdateDT],
				[WorkspaceID],
				[Description],
				[AuthorID],
				[CreatedOn])
				SELECT [UpdateDT]
		  ,@NewWorkspaceID
		  ,[Description]
		  ,[AuthorID]
		  ,[CreatedOn]
	  FROM [version].[RteTemplate]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	DECLARE @RTETemplateMapping TABLE
	(
		OriginalTemplateID int,
		NewTemplateID int
	)

	INSERT INTO @RTETemplateMapping
	SELECT Original.TemplateID, New.TemplateID
	FROM
		(
		SELECT [TemplateID]
		  ,[UpdateDT]
		  ,[Description]
		  ,[AuthorID]
		  ,[CreatedOn]
		FROM [version].[RteTemplate]
		WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID
		) Original
		INNER JOIN
		(
		SELECT [TemplateID]
		  ,[UpdateDT]
		  ,[Description]
		  ,[AuthorID]
		  ,[CreatedOn]
		FROM [dbo].[RteTemplate]
		WHERE WorkspaceID = @NewWorkspaceID
		) New ON
		  Original.[Description] = New.[Description] AND
		  Original.[AuthorID] = New.[AuthorID] AND
		  Original.[CreatedOn] = New.[CreatedOn] 

	INSERT INTO [dbo].[RteTemplateAssigned]
	SELECT RTM.NewTemplateID, Original.[RteTemplateSourceId]
	FROM 
		(SELECT RA.[TemplateID],
			RA.[RteTemplateSourceId]
		FROM [version].[RteTemplateAssigned] RA
		INNER JOIN [version].[RteTemplate] R ON R.[TemplateID] = RA.[TemplateID] AND R.[VersionID] = RA.[VersionID]
		WHERE R.WorkspaceID = @WorkspaceID and RA.[VersionID] = @VersionID
		) Original
		INNER JOIN @RTETemplateMapping RTM ON RTM.OriginalTemplateID = Original.TemplateID

	DECLARE @RTETemplateQuestion TABLE
	(
		[QuestionID] [int] NOT NULL,
		[UpdateDT] [datetime2](7) NOT NULL,
		[TemplateID] [int] NOT NULL,
		[Text] [varchar](500) NOT NULL,
		[SortOrder] [int] NOT NULL,
		[Required] [bit] NOT NULL,
		Processed bit,
		NewQuestionID int,
		NewTemplateID int
	)
	INSERT INTO @RTETemplateQuestion
	SELECT [QuestionID]
		  ,[UpdateDT]
		  ,[TemplateID]
		  ,[Text]
		  ,[SortOrder]
		  ,[Required]
		  ,0
		  ,NULL
		  ,RTM.NewTemplateID
	  FROM [version].[RteTemplateQuestion] RTQ
	INNER JOIN @RTETemplateMapping RTM ON RTM.OriginalTemplateID = RTQ.TemplateID
	WHERE RTQ.VersionID = @VersionID

	DECLARE @QuestionID int
	WHILE EXISTS (SELECT 1 FROM @RTETemplateQuestion WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @QuestionID = [QuestionID] FROM @RTETemplateQuestion WHERE Processed = 0

	INSERT INTO [dbo].[RteTemplateQuestion]
			   ([UpdateDT],
				[TemplateID],
				[Text],
				[SortOrder],
				[Required])
	SELECT RTQ.[UpdateDT]
		  ,RTQ.NewTemplateID
		  ,RTQ.[Text]
		  ,RTQ.[SortOrder]
		  ,RTQ.[Required]
	  FROM @RTETemplateQuestion RTQ
	WHERE RTQ.[QuestionID] = @QuestionID  

	UPDATE @RTETemplateQuestion
	SET NewQuestionID = SCOPE_IDENTITY(),
		Processed = 1
	WHERE [QuestionID] = @QuestionID  

	END

	INSERT INTO [dbo].RTETemplateAnswer
				([UpdateDT],
				[QuestionID],
				[BOEID],
				[TaskID],
				[Text],
				[RteTemplateSourceId])
	SELECT RTA.[UpdateDT],
			RTQ.NewQuestionID,
			B.NewBOEID,
			T.NewBOETaskElementID,
			RTA.[Text],
			RTA.[RteTemplateSourceId]
		FROM [version].RTETemplateAnswer RTA
		INNER JOIN @RTETemplateQuestion RTQ ON RTA.QuestionID = RTQ.QuestionID
		INNER JOIN @BOE B ON RTA.BOEID = B.BOEID
		LEFT JOIN @BOETaskElement T on T.[BOETaskElementID] = RTA.[TaskID]
		WHERE RTA.VersionID = @VersionID
	
	DECLARE @SumOfBOE_OrdinaryVariableXREF TABLE 
	(
		[OVSumID] [bigint] NOT NULL,
		[OrdinaryVariableID] [int] NOT NULL,
		[CLINID] [int] NULL,
		[WBSID] [int] NULL,
		[BOEID] [int] NULL,
		NewOrdinaryVariableID int,
		NewCLINID int,
		NewWBSID int,
		NewBOEID int
	)
	INSERT INTO @SumOfBOE_OrdinaryVariableXREF
	SELECT X.[OVSumID]
		  ,X.[OrdinaryVariableID]
		  ,X.[CLINID]
		  ,X.[WBSID]
		  ,X.[BOEID]
		  ,V.NewOrdinaryVariableID
		  ,NULL
		  ,NULL
		  ,NULL
	  FROM [version].[SumOfBOE_OrdinaryVariableXREF] X
	INNER JOIN @OrdinaryVariable V ON X.OrdinaryVariableID = V.OrdinaryVariableID
	  WHERE X.VersionId = @VersionID

	UPDATE @SumOfBOE_OrdinaryVariableXREF
	SET NewWBSID = W.NewWBSID
	FROM  @SumOfBOE_OrdinaryVariableXREF X
		INNER JOIN @WorkBreakdownStructure W ON X.WBSID = W.WBSID
	UPDATE @SumOfBOE_OrdinaryVariableXREF
	SET NewCLINID = C.NewCLINID
	FROM  @SumOfBOE_OrdinaryVariableXREF X
		INNER JOIN @CLIN C ON X.CLINID = C.CLINID
	UPDATE @SumOfBOE_OrdinaryVariableXREF
	SET NewBOEID = B.NewBOEID
	FROM  @SumOfBOE_OrdinaryVariableXREF X
		INNER JOIN @BOE B ON X.BOEID = B.BOEID

	INSERT INTO [dbo].[SumOfBOE_OrdinaryVariableXREF]
			   ([OrdinaryVariableID]
			   ,[CLINID]
			   ,[WBSID]
			   ,[BOEID])
	SELECT            
		NewOrdinaryVariableID,
		NewCLINID,
		NewWBSID,
		NewBOEID
	FROM  @SumOfBOE_OrdinaryVariableXREF 
	INSERT INTO [dbo].[OrdinaryVariableSumVariableResourceTypeXREF]
			   ([OrdinaryVariableID]
			   ,[SumVariableResourceTypeID])
	SELECT O.NewOrdinaryVariableID
		  ,[SumVariableResourceTypeID]
	  FROM [version].[OrdinaryVariableSumVariableResourceTypeXREF] X
		INNER JOIN @OrdinaryVariable O ON X.OrdinaryVariableID = O.OrdinaryVariableID
	  WHERE X.VersionId = @VersionID

	INSERT INTO [dbo].[BOELaborSpread]
			   ([BOELaborTypeID]
			   ,[LaborSpreadDate]
			   ,[LaborSpreadValue])
	SELECT LT.NewBOELaborTypeID
		  ,[LaborSpreadDate]
		  ,[LaborSpreadValue]
	  FROM [version].[BOELaborSpread] LS
		INNER JOIN @BOELaborType LT ON LS.BOELaborTypeID = LT.BOELaborTypeID
	  WHERE LS.VersionId = @VersionID

	/*Locked Tables*/

	INSERT INTO [dbo].[WorkspaceLockedPerDiem]
			   ([PerDiemID]
			   ,[UpdateDT]
			   ,[PerDiemDestination]
			   ,[Qualification]
			   ,[HotelRate]
			   ,[MIERate]
			   ,[PerDiemNotes]
			   ,[PerDiemLastUpdateETIUserID]
			   ,[PerDiemLastUpdateDT]
			   ,[WorkspaceID])
	SELECT [PerDiemID]
		  ,[UpdateDT]
		  ,[PerDiemDestination]
		  ,[Qualification]
		  ,[HotelRate]
		  ,[MIERate]
		  ,[PerDiemNotes]
		  ,[PerDiemLastUpdateETIUserID]
		  ,[PerDiemLastUpdateDT]
		  ,@NewWorkspaceID
	  FROM [version].[WorkspaceLockedPerDiem]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	INSERT INTO [dbo].[WorkspaceLockedTravelEscalationRate]
			   ([TravelEscalationRateID]
			   ,[UpdateDT]
			   ,[Year]
			   ,[DevEscalation]
			   ,[LMSIEscalation]
			   ,[MiscRate]
			   ,[WorkspaceID])
	SELECT [TravelEscalationRateID]
		  ,[UpdateDT]
		  ,[Year]
		  ,[DevEscalation]
		  ,[LMSIEscalation]
		  ,[MiscRate]
		  ,@NewWorkspaceID
	  FROM [version].[WorkspaceLockedTravelEscalationRate]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	INSERT INTO [dbo].[WorkspaceLockedTravelMiscRate]
			   ([TravelMiscRateID]
			   ,[UpdateDT]
			   ,[TransportationMode]
			   ,[MiscellaneousRate]
			   ,[SortCode]
			   ,[MiscRateInUse]
			   ,[WorkspaceID])
	SELECT [TravelMiscRateID]
		  ,[UpdateDT]
		  ,[TransportationMode]
		  ,[MiscellaneousRate]
		  ,[SortCode]
		  ,[MiscRateInUse]
		  ,@NewWorkspaceID
	  FROM [version].[WorkspaceLockedTravelMiscRate]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	INSERT INTO [dbo].[WorkspaceLockedTrip]
			   ([TripID]
			   ,[UpdateDT]
			   ,[TravelMiscRateID]
			   ,[DepartureLocationID]
			   ,[DestinationLocationID]
			   ,[PerDiemID]
			   ,[TransportationFare]
			   ,[RoundTripMiles]
			   ,[FareLastUpdateETIUserID]
			   ,[FareLastUpdateDT]
			   ,[TripInUse]
			   ,[LastUsedDT]
			   ,[RentalCarRate]
			   ,[DepartureLocationCode]
			   ,[DestinationLocationCode]
			   ,[WorkspaceID])
	SELECT [TripID]
		  ,[UpdateDT]
		  ,[TravelMiscRateID]
		  ,[DepartureLocationID]
		  ,[DestinationLocationID]
		  ,[PerDiemID]
		  ,[TransportationFare]
		  ,[RoundTripMiles]
		  ,[FareLastUpdateETIUserID]
		  ,[FareLastUpdateDT]
		  ,[TripInUse]
		  ,[LastUsedDT]
		  ,[RentalCarRate]
		  ,[DepartureLocationCode]
		  ,[DestinationLocationCode]
		  ,@NewWorkspaceID
	  FROM [version].[WorkspaceLockedTrip]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	/* Copy new BOE/INL Forms */

	DECLARE @IBOE TABLE
	(
		[IBOEFormID] [int] NOT NULL,
		Processed bit DEFAULT 0,
		[NEW_IBOEFormID] [int] NULL,
		NewWorkspaceID int NOT NULL
	)

	INSERT INTO @IBOE 
		SELECT 
			IBOEFormID, 0, NULL, @NewWorkspaceID
		FROM [version].[BOEFormIBOE]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	DECLARE @IBOEID int
	WHILE EXISTS (SELECT 1 FROM @IBOE WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @IBOEID = [IBOEFormID] FROM @IBOE WHERE Processed = 0

	INSERT INTO [dbo].[BOEFormIBOE]
	(UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, BusinessArea, Revision,FormVersion)
	SELECT UpdateDT, @NewWorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, BusinessArea, Revision,FormVersion
	  FROM [version].[BOEFormIBOE]
	WHERE [IBOEFormID] = @IBOEID AND VersionID = @VersionID

	UPDATE @IBOE 
	SET	[NEW_IBOEFormID] = SCOPE_IDENTITY(),
		Processed = 1
	WHERE 
	[IBOEFormID] = @IBOEID	

	END

	--Insert the INL IBOE Resources
	INSERT INTO [dbo].[BOEFormIBOEResourcesXREF]
	SELECT B.[NEW_IBOEFormID]
		  ,IsNull(R.NewSystemResourceID, x.ResourceID)
	FROM [version].[BOEFormIBOEResourcesXREF] x
		INNER JOIN @IBOE B ON B.[IBOEFormID] = x.[IBOEFormID]
		LEFT OUTER JOIN @WorkspaceResource R ON x.ResourceID = R.[SystemResourceID]
	WHERE x.[IBOEFormID] IN (SELECT [IBOEFormID] FROM @IBOE) AND x.VersionID = @VersionID

	DECLARE @PBOE TABLE
	(
		[PBOEFormID] [int] NOT NULL,
		Processed bit DEFAULT 0,
		[NEW_PBOEFormID] [int] NULL,
		NewWorkspaceID int NOT NULL
	)

	INSERT INTO @PBOE 
		SELECT 
			PBOEFormID, 0, NULL, @NewWorkspaceID
		FROM [version].[BOEFormPBOE]
	WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

	DECLARE @PBOEID int
	WHILE EXISTS (SELECT 1 FROM @PBOE WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @PBOEID = [PBOEFormID] FROM @PBOE WHERE Processed = 0

	INSERT INTO [dbo].[BOEFormPBOE]
		(UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion,  
		[DegreeOfCompetition],
		[CCoPD],
		[CCoPDOtherText],
		[RFP],
		[ProposalNumber],
		[SupplierName],
		[ValidityDate],
		[SupplierProposalSupportingDataIncluded],
		[PriceAnalysisIncluded],
		[CommercialItemDocIncluded],
		[CostAnalysisIncluded],
		[ShouldCostEstimate],
		[ShouldCostEstimateDate],
		[SowWritten],
		[SowWrittenDate],
		[RFPRelease],
		[RFPReleaseDate],
		[FirmSupplierReceipt],
		[FirmSupplierReceiptDate],
		[SourceSelection],
		[SourceSelectionDate],
		[CID],
		[CIDDate],
		[GovtReview],
		[GovtReviewDate],
		[PriceAnalysis],
		[PriceAnalysisDate],
		[TechnicalEvaluation],
		[TechnicalEvaluationDate],
		[FactFinding],
		[FactFindingDate],
		[CostAnalysis],
		[CostAnalysisDate],
		[GovtPricing],
		[GovtPricingDate],
		[SupplierNegotiations],
		[SupplierNegotiationsDate],
		[MOU],
		[MOUDate],
		[Procurement],
		[ProcurementDate],
		[PlannedDate_WrittenApproval],
		[PlannedDate_ApprovedSubmission],
		[CIDText],
		[GovtReviewText],
		[PriceAnalysisText],
		[TechnicalEvaluationText],
		[FactFindingText],
		[CostAnalysisText],
		[GovtPricingText],
		[SupplierNegotiationsText],
		[MOUText],
		[ProcurementText],
		ShouldCostEstimateText,
		SowWrittenText,
		RFPReleaseText,
		FirmSupplierReceiptText,
		SourceSelectionText)
	SELECT UpdateDT, @NewWorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion, 
		[DegreeOfCompetition],
		[CCoPD],
		[CCoPDOtherText],
		[RFP],
		[ProposalNumber],
		[SupplierName],
		[ValidityDate],
		[SupplierProposalSupportingDataIncluded],
		[PriceAnalysisIncluded],
		[CommercialItemDocIncluded],
		[CostAnalysisIncluded],
		[ShouldCostEstimate],
		[ShouldCostEstimateDate],
		[SowWritten],
		[SowWrittenDate],
		[RFPRelease],
		[RFPReleaseDate],
		[FirmSupplierReceipt],
		[FirmSupplierReceiptDate],
		[SourceSelection],
		[SourceSelectionDate],
		[CID],
		[CIDDate],
		[GovtReview],
		[GovtReviewDate],
		[PriceAnalysis],
		[PriceAnalysisDate],
		[TechnicalEvaluation],
		[TechnicalEvaluationDate],
		[FactFinding],
		[FactFindingDate],
		[CostAnalysis],
		[CostAnalysisDate],
		[GovtPricing],
		[GovtPricingDate],
		[SupplierNegotiations],
		[SupplierNegotiationsDate],
		[MOU],
		[MOUDate],
		[Procurement],
		[ProcurementDate],
		[PlannedDate_WrittenApproval],
		[PlannedDate_ApprovedSubmission],
		[CIDText],
		[GovtReviewText],
		[PriceAnalysisText],
		[TechnicalEvaluationText],
		[FactFindingText],
		[CostAnalysisText],
		[GovtPricingText],
		[SupplierNegotiationsText],
		[MOUText],
		[ProcurementText],
		ShouldCostEstimateText,
		SowWrittenText,
		RFPReleaseText,
		FirmSupplierReceiptText,
		SourceSelectionText
	  FROM [version].[BOEFormPBOE]
	WHERE [PBOEFormID] = @PBOEID AND VersionID = @VersionID

	UPDATE @PBOE 
	SET	[NEW_PBOEFormID] = SCOPE_IDENTITY(),
		Processed = 1
	WHERE 
	[PBOEFormID] = @PBOEID	

	END

	--Insert the INL PBOE Resources
	INSERT INTO [dbo].[BOEFormPBOEResourcesXREF]
	SELECT B.[NEW_PBOEFormID]
		  ,IsNull(R.NewSystemResourceID, x.ResourceID)
	FROM [version].[BOEFormPBOEResourcesXREF] x
		INNER JOIN @PBOE B ON B.[PBOEFormID] = x.[PBOEFormID]
		LEFT OUTER JOIN @WorkspaceResource R ON x.ResourceID = R.[SystemResourceID]
	WHERE x.[PBOEFormID] IN (SELECT [PBOEFormID] FROM @PBOE) AND x.VersionID = @VersionID

	-- Insert the INL IBOE CLIN selections
	INSERT INTO [dbo].[BOEFormIBOECLINsXREF]
	SELECT B.[NEW_IBOEFormID],
		c.[NewCLINID],
		x.[ContractType]
	FROM [version].[BOEFormIBOECLINsXREF] x
		INNER JOIN @IBOE B ON B.[IBOEFormID] = x.[IBOEFormID]
		INNER JOIN @CLIN C on C.[CLINID] = x.[CLINID]
	WHERE x.VersionID = @VersionID

	-- Insert the INL PBOE CLIN selections
	INSERT INTO [dbo].[BOEFormPBOECLINsXREF]
	SELECT B.[NEW_PBOEFormID],
		c.[NewCLINID],
		x.[ContractType]
	FROM [version].[BOEFormPBOECLINsXREF] x
		INNER JOIN @PBOE B ON B.[PBOEFormID] = x.[PBOEFormID]
		INNER JOIN @CLIN C on C.[CLINID] = x.[CLINID]
	WHERE x.VersionID = @VersionID

	INSERT INTO [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts] ([UpdateDT], [WorkspaceID], ModeID, TravelAgencyFee, MiscOther)
		SELECT [UpdateDT], @NewWorkspaceID, ModeID, TravelAgencyFee, MiscOther
		FROM [version].[WorkspaceRMSTravelNonzoneFeesAndCosts] WHERE [WorkspaceID] = @CopyFromWorkspaceID AND VersionID = @VersionID

	INSERT INTO [dbo].[WorkspaceRMSTravelEscalationRate] ([UpdateDT], [WorkspaceID], [Year], [Escalation], [MiscRate], [PerDiemRate])
		SELECT [UpdateDT], @NewWorkspaceID, [Year], [Escalation], [MiscRate], [PerDiemRate]
		FROM [version].[WorkspaceRMSTravelEscalationRate] WHERE [WorkspaceID] = @CopyFromWorkspaceID AND VersionID = @VersionID

	IF @@ERROR = 0
		BEGIN
			COMMIT TRANSACTION

			/*Workspace Copy Metric*/
			IF @CopyFromWorkspaceID IS NOT NULL 
			BEGIN

			DECLARE @CreateDate datetime2(7) = GetDate()

			INSERT INTO [dbo].[WorkspaceCopySource]
			   ([WorkspaceID]
			   ,[UpdateDT]
			   ,[WorkspaceName]
			   ,[WorkspaceShortName]
			   ,[WorkspaceStateID]
			   ,[ContractStartDate]
			   ,[ContractEndDate]
			   ,[ProposalSubmitDate]
			   ,[WorkspaceDescription]
			   ,[CostVolumeLeadPricerUserID]
			   ,[RFPNumber]
			   ,[TemplateID]
			   ,[ContainsOCI]
			   ,[CreatedByETIUserID]
			   ,[AllowSearch]
			   ,[ResourceListID]
			   ,[PerformingOrganizationListID]
			   ,[PerformingOrganizationChangeFlag]
			   ,[TrackingNumber]
			   ,[ContainsTemplate]
			   ,[NumProPricerExport]
			   ,[ProposalStatusID]
			   ,[StatusComment]
			   ,[BOEExportSortByID]
			   ,[SegmentID]
			   ,[LineOfBusinessID]
			   ,[ProposalClassID]
			   ,[ProposalTitle]
			   ,[IsDeleted]
			   ,[DateDeleted]
			   ,[LineOfBusinessName]
			   ,[NumOfBOEs]
			   ,[CreateDate]
			   )
		SELECT W.[WorkspaceID]
			  ,W.[UpdateDT]
			  ,W.[WorkspaceName]
			  ,W.[WorkspaceShortName]
			  ,W.[WorkspaceStateID]
			  ,W.[ContractStartDate]
			  ,W.[ContractEndDate]
			  ,W.[ProposalSubmitDate]
			  ,W.[WorkspaceDescription]
			  ,W.[CostVolumeLeadPricerUserID]
			  ,W.[RFPNumber]
			  ,W.[TemplateID]
			  ,W.[ContainsOCI]
			  ,W.[CreatedByETIUserID]
			  ,W.[AllowSearch]
			  ,W.[ResourceListID]
			  ,W.[PerformingOrganizationListID]
			  ,W.[PerformingOrganizationChangeFlag]
			  ,W.[TrackingNumber]
			  ,W.[ContainsTemplate]
			  ,W.[NumProPricerExport]
			  ,W.[ProposalStatusID]
			  ,W.[StatusComment]
			  ,W.[BOEExportSortByID]
			  ,W.[SegmentID]
			  ,W.[LineOfBusinessID]
			  ,W.[ProposalClassID]
			  ,W.[ProposalTitle]
			  ,W.[IsDeleted]
			  ,W.[DateDeleted]
			  ,LOB.[LineOfBusinessName]	      
			  ,(SELECT COUNT (*) FROM [version].[BOE] WHERE WorkspaceID = @CopyFromWorkspaceID) AS [NumOfBOEs]
			  ,@CreateDate
		  FROM [version].[Workspace] W
			LEFT OUTER JOIN [dbo].[LineOfBusiness] LOB ON W.LineOfBusinessID = LOB.LineOfBusinessID
		  WHERE WorkspaceID = @CopyFromWorkspaceID AND W.VersionID = @VersionID
	  	   
		INSERT INTO [dbo].[WorkspaceCopyTarget]
			([WorkspaceID]
			,[UpdateDT]
			,[WorkspaceName]
			,[WorkspaceShortName]
			,[WorkspaceStateID]
			,[ContractStartDate]
			,[ContractEndDate]
			,[ProposalSubmitDate]
			,[WorkspaceDescription]
			,[CostVolumeLeadPricerUserID]
			,[RFPNumber]
			,[TemplateID]
			,[ContainsOCI]
			,[CreatedByETIUserID]
			,[AllowSearch]
			,[ResourceListID]
			,[PerformingOrganizationListID]
			,[PerformingOrganizationChangeFlag]
			,[TrackingNumber]
			,[ContainsTemplate]
			,[NumProPricerExport]
			,[ProposalStatusID]
			,[StatusComment]
			,[BOEExportSortByID]
			,[SegmentID]
			,[LineOfBusinessID]
			,[ProposalClassID]
			,[ProposalTitle]
			,[IsDeleted]
			,[DateDeleted]
			,[LineOfBusinessName]
			,[NumOfBOEs]
			,[CreateDate]
			)
		SELECT W.[WorkspaceID]
			  ,W.[UpdateDT]
			  ,W.[WorkspaceName]
			  ,W.[WorkspaceShortName]
			  ,W.[WorkspaceStateID]
			  ,W.[ContractStartDate]
			  ,W.[ContractEndDate]
			  ,W.[ProposalSubmitDate]
			  ,W.[WorkspaceDescription]
			  ,W.[CostVolumeLeadPricerUserID]
			  ,W.[RFPNumber]
			  ,W.[TemplateID]
			  ,W.[ContainsOCI]
			  ,W.[CreatedByETIUserID]
			  ,W.[AllowSearch]
			  ,W.[ResourceListID]
			  ,W.[PerformingOrganizationListID]
			  ,W.[PerformingOrganizationChangeFlag]
			  ,W.[TrackingNumber]
			  ,W.[ContainsTemplate]
			  ,W.[NumProPricerExport]
			  ,W.[ProposalStatusID]
			  ,W.[StatusComment]
			  ,W.[BOEExportSortByID]
			  ,W.[SegmentID]
			  ,W.[LineOfBusinessID]
			  ,W.[ProposalClassID]
			  ,W.[ProposalTitle]
			  ,W.[IsDeleted]
			  ,W.[DateDeleted]
			  ,LOB.[LineOfBusinessName]	    
			  ,(SELECT COUNT (*) FROM [dbo].[BOE] WHERE WorkspaceID = @NewWorkspaceID) AS [NumOfBOEs]
			  ,@CreateDate
		  FROM [dbo].[Workspace] W
			LEFT OUTER JOIN [dbo].[LineOfBusiness] LOB ON W.LineOfBusinessID = LOB.LineOfBusinessID
	 WHERE WorkspaceID = @NewWorkspaceID

		INSERT INTO [dbo].[WorkspaceCopyMetric]
			   ([SourceWorkspaceID]
			   ,[TargetWorkspaceID]
			   ,[CreateDate])
		 VALUES
			   (@CopyFromWorkspaceID
			   ,@NewWorkspaceID
			   ,@CreateDate)
			END

			SELECT @NewWorkspaceID AS WorkspaceID
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
GO