EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2025.14';
GO

-- 10/01/2025 Yemi Oyetoro (e378233), PROPH-3302 - (Database) SKILL MIX - Summary Table

IF OBJECT_ID('dbo.SkillMixSummary', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SkillMixSummary](
	[SkillMixSummaryID] [int] IDENTITY(1,1) NOT NULL,
	[Rationale] [varchar](255) NOT NULL,
	[Included] [bit] NOT NULL,
	[ProposedHours] [decimal](11, 2) NOT NULL,
	[HistoricalHours] [decimal](11, 2) NOT NULL,
	[ResourceHours] [decimal](11, 2) NOT NULL,
	[BusinessResourceHours] [decimal](11, 2) NOT NULL,
	[BOESkillMix] [decimal](5, 2) NOT NULL,
	[LaborSkillMix] [decimal](5, 2) NOT NULL,
	[ResourceID] [varchar](20) NOT NULL,
	[BusinessResourceID] [varchar](20) NOT NULL,
	[BOEID] [int] NOT NULL,
	[BOETaskElementID] [int] NOT NULL,
	[IsUserInput] [bit] NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[SkillMixSummaryID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[SkillMixSummary] ADD  DEFAULT ((0)) FOR [Included]

	ALTER TABLE [dbo].[SkillMixSummary] ADD  DEFAULT ((0)) FOR [IsUserInput]

	ALTER TABLE [dbo].[SkillMixSummary]  WITH CHECK ADD  CONSTRAINT [FK_SkillMixSummary_BOE] FOREIGN KEY([BOEID])
	REFERENCES [dbo].[BOE] ([BOEID])

	ALTER TABLE [dbo].[SkillMixSummary] CHECK CONSTRAINT [FK_SkillMixSummary_BOE]

	ALTER TABLE [dbo].[SkillMixSummary]  WITH CHECK ADD  CONSTRAINT [FK_SkillMixSummary_BOETaskElement] FOREIGN KEY([BOETaskElementID])
	REFERENCES [dbo].[BOETaskElement] ([BOETaskElementID])

	ALTER TABLE [dbo].[SkillMixSummary] CHECK CONSTRAINT [FK_SkillMixSummary_BOETaskElement]
END


IF OBJECT_ID('version.SkillMixSummary', 'U') IS NULL
BEGIN
	CREATE TABLE [version].[SkillMixSummary](
		[SkillMixSummaryID] [int] NOT NULL,
		[Rationale] [varchar](255) NOT NULL,
		[Included] [bit] NOT NULL,
		[ProposedHours] [decimal](11, 2) NOT NULL,
		[HistoricalHours] [decimal](11, 2) NOT NULL,
		[ResourceHours] [decimal](11, 2) NOT NULL,
		[BusinessResourceHours] [decimal](11, 2) NOT NULL,
		[BOESkillMix] [decimal](5, 2) NOT NULL,
		[LaborSkillMix] [decimal](5, 2) NOT NULL,
		[ResourceID] [varchar](20) NOT NULL,
		[BusinessResourceID] [varchar](20) NOT NULL,
		[BOEID] [int] NOT NULL,
		[BOETaskElementID] [int] NOT NULL,
		[IsUserInput] [bit] NOT NULL,
		[VersionID] [int] NOT NULL
	) ON [PRIMARY]

	ALTER TABLE [version].[SkillMixSummary] ADD  DEFAULT ((0)) FOR [Included]
END
GO

-- CREATE or Alter deleteSkillMixSummary
CREATE OR ALTER PROCEDURE [dbo].[deleteSkillMixSummary]
(
	@BOETaskElementID int
)
AS
	/******************************************************************************
	**		 
	**		Name: [deleteSkillMixSummary]
	**		Desc: Delete all parts of SkillMix summary Table
	**			
	**		
	**
	**		Auth: Oyetoro Oyeyemi
	**		Date: 9/2025
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			-------------------------------------------
	**      9/24/25		e378233 			PROPH-3302 Skill Mix Summary DB Table
	*****************************************************************************/

	-- deleting a skill mix could delete 1-many rows in summary table

	BEGIN
		DELETE FROM dbo.[SkillMixSummary]
		WHERE [BOETaskElementID] = @BOETaskElementID

		SELECT @@ROWCOUNT AS RowsAffected;
	END
GO


-- Drop/Create insertSkillMixSummaryviaTableParameter
IF OBJECT_ID('[dbo].[insertSkillMixSummaryviaTableParameter]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[insertSkillMixSummaryviaTableParameter];
GO

-- Drop/Create TT_SkillMixSummary
IF EXISTS (
    SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = 'TT_SkillMixSummary' AND schema_id = SCHEMA_ID('dbo')
)
    DROP TYPE [dbo].[TT_SkillMixSummary];
GO

/****** Object:  UserDefinedTableType [dbo].[TT_SkillMixSummary]    Script Date: 9/30/2025 10:28:15 PM ******/
CREATE TYPE [dbo].[TT_SkillMixSummary] AS TABLE(
	[Rationale] [varchar](255) NOT NULL,
	[Included] [bit] NOT NULL DEFAULT ((0)),
	[ProposedHours] [decimal](11, 2) NOT NULL,
	[HistoricalHours] [decimal](11, 2) NOT NULL,
	[ResourceHours] [decimal](11, 2) NOT NULL,
	[BusinessResourceHours] [decimal](11, 2) NOT NULL,
	[BOESkillMix] [decimal](5, 2) NOT NULL,
	[LaborSkillMix] [decimal](5, 2) NOT NULL,
	[ResourceID] [varchar](20) NOT NULL,
	[BusinessResourceID] [varchar](20) NOT NULL,
	[BOEID] [int] NOT NULL,
	[BOETaskElementID] [int] NOT NULL,
	[IsUserInput] [bit] NOT NULL,
	[OrderID] [int] NOT NULL
)
GO


-- CREATE/ALTER insertSkillMixSummaryviaTableParameter
CREATE OR ALTER PROCEDURE [dbo].[insertSkillMixSummaryviaTableParameter]
(
	@skillMixSummaryTableParameter [dbo].[TT_SkillMixSummary] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertSkillMixSummaryviaTableParameter]
**		Desc: Insert/Update data into SkillMix Summary Table
**			
**		
**
**		Auth: Oyetoro Oyeyemi
**		Date: 9/2025
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**      9/24/25		e378233 			PROPH-3302 Skill Mix Summary DB Table
*****************************************************************************/
BEGIN
	DECLARE @DistinctBOETaskElementID int
	SELECT @DistinctBOETaskElementID = BOETaskElementID
	FROM (
		SELECT DISTINCT BOETaskElementID
		FROM @SkillMixSummaryTableParameter
	) AS temp_SkillMixSummary

	DELETE FROM [dbo].[SkillMixSummary]
	WHERE [BOETaskElementID] = @DistinctBOETaskElementID

	INSERT INTO [dbo].[SkillMixSummary]
		([Rationale]
		 ,[Included]
		 ,[ProposedHours]
		 ,[HistoricalHours]
		 ,[ResourceHours]
		 ,[BusinessResourceHours]
		 ,[BOESkillMix]
		 ,[LaborSkillMix]
		 ,[ResourceID]
		 ,[BusinessResourceID]
		 ,[BOEID]
		 ,[BOETaskElementID]
		 ,[IsUserInput]
		)
	SELECT T.[Rationale]
		 ,T.[Included]
		 ,T.[ProposedHours]
		 ,T.[HistoricalHours]
		 ,T.[ResourceHours]
		 ,T.[BusinessResourceHours]
		 ,T.[BOESkillMix]
		 ,T.[LaborSkillMix]
		 ,T.[ResourceID]
		 ,T.[BusinessResourceID]
		 ,T.[BOEID]
		 ,T.[BOETaskElementID]
		 ,T.[IsUserInput]
	FROM @SkillMixSummaryTableParameter T
END

IF @@ERROR = 0
	SELECT COUNT(*) FROM @SkillMixSummaryTableParameter
GO


-- CREATE/ALTER deleteBOETaskElementviaTableParameter
CREATE OR ALTER PROCEDURE [dbo].[deleteBOETaskElementviaTableParameter]
(
@BOETaskElement [dbo].[TT_BOETaskElement] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOETaskElementviaTableParameter]
**		Desc: Delete Flag set in LM Task Element Section of BOE and all sub-elements (Labor Types and Labor Spread)
**			
**		
**
**		Auth: Don Canuso
**		Date: 8/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**      6/24/16     twilson3            Fix In-Use Flag for Custom Fields
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		4/2/18		ranzalon			BOEJ-3268 - Update for Open Ended Custom Fields
**		6/5/20		ranzalon			BOEJ-4658 - Update for RTE Template Answers
**		3/6/25		e405721				PROPH-2895 - Update Delete for Skill Mix and Common Disclosure
**		9/30/25		e378233				PROPH-3302 - Update Delete for Skill Mix Summary
*******************************************************************************/
SET NOCOUNT ON 

			-- Delete RTE Template Answers
			DELETE FROM dbo.[RteTemplateAnswer]
			WHERE TaskID IN
				(
					SELECT BOETaskElementID
					FROM @BOETaskElement
				)

			DELETE FROM dbo.BOELaborSpread
				FROM dbo.BOELaborSpread LS
				INNER JOIN dbo.BOELaborType LT ON LS.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT
			
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @LaborTypeCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @LaborTypeCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOELaborTypeCustomFieldValueXREF
			WHERE BOELaborTypeID IN
				(
					SELECT BOELaborTypeID
					FROM dbo.BOELaborType LT
					INNER JOIN @BOETaskElement TT ON 
					LT.BOETaskElementID = TT.BOETaskElementID
				)	
			
			DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT
				
			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @LaborTypeCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)
				
			DELETE FROM dbo.BOETaskElementWorkspaceVariableXREF
			FROM dbo.BOETaskElementWorkspaceVariableXREF X  
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

				
			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF
			FROM  dbo.SumOfBOE_OrdinaryVariableXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN  dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

			

			DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF
			FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN  dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
							INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT


				

			DELETE FROM dbo.OrdinaryVariable
			FROM dbo.OrdinaryVariable OV
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @TaskCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @TaskCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOETaskElementCustomFieldValueXREF
			WHERE BOETaskElementID IN
			(
				SELECT BOETaskElementID
				FROM @BOETaskElement
			)
				
			DELETE FROM dbo.BOETaskElementCustomFieldValueXREF
			FROM dbo.BOETaskElementCustomFieldValueXREF X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @TaskCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)
			
			DELETE FROM [dbo].[BOETaskElementMetricDetailXREF]
				FROM [dbo].[BOETaskElementMetricDetailXREF] X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT


			
 
			DELETE FROM dbo.BOELaborType
				FROM dbo.BOELaborType LT
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT

				
				
			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID 
			FROM dbo.BOE B
				INNER JOIN dbo.BOETaskElement TE ON B.BOEID = TE.BOEID
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT


			DELETE FROM dbo.CommonDisclosureSkillMix
				FROM dbo.CommonDisclosureSkillMix CS
					INNER JOIN dbo.BOETaskElement TE ON CS.BOETaskElementID = TE.BOETaskElementID
					INNER JOIN @BOETaskElement TT ON 
						TE.BOETaskElementID = TT.BOETaskElementID AND
						TE.UpdateDT = TT.UpdateDT


			DELETE FROM dbo.SkillMixSummary
				FROM dbo.SkillMixSummary SMS
					INNER JOIN dbo.BOETaskElement TE ON SMS.BOETaskElementID = TE.BOETaskElementID
					INNER JOIN @BOETaskElement TT ON 
						TE.BOETaskElementID = TT.BOETaskElementID AND
						TE.UpdateDT = TT.UpdateDT

			DELETE FROM dbo.SkillMix
				FROM dbo.SkillMix S
					INNER JOIN dbo.BOETaskElement TE ON S.BOETaskElementID = TE.BOETaskElementID
					INNER JOIN @BOETaskElement TT ON 
						TE.BOETaskElementID = TT.BOETaskElementID AND
						TE.UpdateDT = TT.UpdateDT
			
			
			DELETE FROM [dbo].[BOETaskElement]
			FROM [dbo].[BOETaskElement] TE
				INNER JOIN @BOETaskElement TT ON 
					TE.BOETaskElementID = TT.BOETaskElementID AND
					TE.UpdateDT = TT.UpdateDT



IF @@ERROR <> 0
BEGIN
DECLARE @ErrorMessage varchar (500)
SET @ErrorMessage =   'The BOE Task Element(s) has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
 
		END
GO


-- CREATE/ALTER copyWorkspace
CREATE OR ALTER  PROCEDURE [dbo].[copyWorkspace]
(
@WorkspaceID int ,
@WorkspaceName varchar (115),
@WorkspaceShortName varchar(21),
@CostVolumeLeadPricerUserID int,
@SAPSpaceCutoff datetime2(7) NULL
)
AS
/******************************************************************************
**		 
**		Name: [copyWorkspace]
**		Desc:	Copy All Workspace Data into a new Workspace
**
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		1/22/20		ranzalon			Fixed bug with missing RteTemplateSourceId
**		6/11/20		Dusan				BOEJ-4655 Exact copy should copy WS email settings
**		8/19/20		Dusan				BOEJ-4728 Add Lead Pricer / Estimator change to the copy
**		8/27/20		ranzalon			BOEJ-4760 - Template Boe
**		9/15/20		ranzalon			BOEJ-4776/4825 - MOQ Types update
**		12/8/2020	ranzalon			BOEJ-4972 - remove CER location and BOELaborType MOQTypeSelectionId fields
**		1/4/2021	Dusan				BOEJ-4894: Added support for MoqTypeTableCustomFieldValueXREF
**		11/30/2021	Dusan				IES-461: Remove RMS Zone Travel copying
**		4/13/2022	jquijano			IES-1014: Remove deprecated PBOE fields
**		4/22/2022	jquijano			IES-1019: Add new fields to copy workspace
**		5/2/2022	jquijano			IES-1126: Add VendorId, SupplierProposedValue
**		1/31/23		e405721				ACV-221 - Enable SAP Connection
**		3/1/23		twilson3			ACV-343 Update MOQ Column sizes
**		3/20/23		Dusan				ACV-498: Updated MOQ Column size (Wbs Element due to prod issue)
**		3/23/23		twilson3			ACV-274 Handle SAP Fiscal Week Cutoff
**      8/10/23     twilson             PROPH-1029 Investigate Project Spreads
**		1/18/24		ranzalon			PROPH-1070 Update for HistoricalReferenceExplanation
**		1/28/24		e302876  			PROPH-1492 ADD BRC to Copy BOEs, Copy WS, Archive/Restore
**		3/18/24		twilson3			PROPH-1760 Fix BRC IDs for new WS
**		7/10/24		e405721				PROPH-2019 Skill Mix, Common Disclosure, MOQ Type Resource Hours Updates
**		07/11/24	twilson3			proph-2166 Missing Columns
**		07/12/24	twilson3			proph-2019 Fix @Temp Table definitions
**		07/30/24	e405721				PROPH-2218 Add BRC Name into MOQ Type Selection Table Data Resource Hours Table
**		10/15/24	e405721				PROPH-2392: Update for Skill Mix V2
**		1/14/25		twilson3			PROPH-2596 - Add UCOT Factor
**		1/15/25		e309214				PROPH-1854 Database Changes for Assign Author
**		9/30/25		e378233				PROPH-3302 Updated Copy Workspace for Skill Mix Summary
*******************************************************************************/
SET NOCOUNT ON 

BEGIN TRANSACTION

BEGIN TRY

DECLARE @CopyFromWorkspaceID int = @WorkspaceID
DECLARE @CopyWorkspaceCreated datetime2(7)
SELECT @CopyWorkspaceCreated = [WorkspaceCreationDate] FROM [dbo].[Workspace] WHERE WorkspaceID = @CopyFromWorkspaceID

DECLARE @ResourceListID int
INSERT INTO [dbo].[ResourceList]
           ([UpdateDT]
           ,[ResourceListName])
SELECT RL.[UpdateDT]
      ,[ResourceListName]
  FROM [dbo].[ResourceList] RL
  INNER JOIN [dbo].[Workspace] W ON RL.ResourceListID = W.ResourceListID
WHERE W.WorkspaceID = @WorkspaceID
 
SELECT  @ResourceListID = SCOPE_IDENTITY() 

DECLARE @PerformingOrganizationListID int
INSERT INTO [dbo].[PerformingOrganizationList]
           ([UpdateDT]
           ,[PerformingOrganizationListName])
SELECT PL.[UpdateDT]
      ,PL.[PerformingOrganizationListName]
  FROM [dbo].[PerformingOrganizationList] PL
  INNER JOIN [dbo].[Workspace] W ON PL.PerformingOrganizationListID = W.PerformingOrganizationListID
WHERE W.WorkspaceID = @WorkspaceID
 
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
		   ,[EnableSAPConnection]
		   ,[UCOTFactor]
		   ,[EnableAssignTaskAuthor]
           )
SELECT [UpdateDT]
      ,@WorkspaceName
      ,@WorkspaceShortName
      ,[WorkspaceStateID]
      ,[ContractStartDate]
      ,[ContractEndDate]
      ,[ProposalSubmitDate]
      ,[WorkspaceDescription]
      ,@CostVolumeLeadPricerUserID
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
	  ,null --LastProPricerInstance
	  ,null --LastProPricerProposal
	  ,[RteSizeLimit]
	  ,[RevisedSubmittalDate]
	  ,[TemplateBoe]
	  ,[EnableSAPConnection]
	  ,[UCOTFactor]
	  ,[EnableAssignTaskAuthor]
  FROM [dbo].[Workspace]
WHERE WorkspaceID = @WorkspaceID

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
FROM [dbo].[WorkspaceContractTypeXREF] 
WHERE WorkspaceID = @WorkspaceID

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
      ,NewWorkspaceID--[WorkspaceID]
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
SELECT @NewWorkspaceID--[WorkspaceID]
      ,[TemplateID]
  FROM [dbo].[OutputFormatTemplateWorkspaceXREF]
WHERE WorkspaceID = @WorkspaceID

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
/*	[ResourceInUseFlag] [bit] NOT NULL,*/
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
  FROM [dbo].[Resource] R
	INNER JOIN [dbo].[ResourceList] RL ON R.ResourceListID = RL.ResourceListID
	INNER JOIN [dbo].[Workspace] W ON RL.ResourceListID = W.ResourceListID
WHERE W.WorkspaceID = @WorkspaceID	

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
FROM [dbo].[WorkspaceResource]
WHERE WorkspaceID = @WorkspaceID

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

INSERT INTO WorkspaceEmailXREF
	SELECT EmailId, @NewWorkspaceID, TurnOn, UpdateDT
			FROM WorkspaceEmailXREF
			WHERE WorkspaceId = @WorkspaceID

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
SELECT R.[PerformingOrganizationID]
	  ,R.[UpdateDT]
      ,R.[PerformingOrganizationName]
      ,R.[PerformingOrganizationDescription]
      ,@PerformingOrganizationListID
      ,R.[DeletedFlag]
      ,0 AS Processed
      ,@NewWorkspaceID AS NewWorkspaceID
      ,NULL AS NewPerformingOrganizationID
  FROM [dbo].[PerformingOrganization] R
	INNER JOIN [dbo].[PerformingOrganizationList] RL ON R.PerformingOrganizationListID = RL.PerformingOrganizationListID
	INNER JOIN [dbo].[Workspace] W ON RL.PerformingOrganizationListID = W.PerformingOrganizationListID
WHERE W.WorkspaceID = @WorkspaceID	

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
FROM [dbo].[WorkspacePerformingOrganization]
WHERE WorkspaceID = @WorkspaceID

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
  FROM [dbo].[BOE]
WHERE WorkspaceID = @WorkspaceID

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
      ,@NewWorkspaceID--[WorkspaceID]
      ,[MetricDisclosureAcknowledge]
      ,[NumAuthorReassigned]
      ,[IsMaterial]
	  ,[BOETitle]
	  ,[IsMultiClinWbs]
  FROM [dbo].[BOE]
WHERE BOEID = @BOEID 

UPDATE @BOE 
SET	NewBOEID = SCOPE_IDENTITY(),
	Processed = 1
WHERE 
BOEID = @BOEID	

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
FROM [dbo].[WorkspaceOffloadRate]
WHERE WorkspaceID = @WorkspaceID

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
FROM [dbo].[ProjectMap]
WHERE WorkspaceID = @WorkspaceID

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
FROM  ProjectMapSpread S
INNER JOIN ProjectMap P ON P.ID = S.ProjectMapId
INNER JOIN ProjectMap NewP ON NewP.WorkspaceId = @NewWorkspaceID AND NewP.OrderID = P.OrderID
WHERE S.WorkspaceID = @WorkspaceID

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
      ,@NewWorkspaceID--[WorkspaceID]
	  ,[IsOpenEnded]
  FROM [dbo].[CustomField]
WHERE WorkspaceID = @WorkspaceID

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
	FROM [dbo].[CustomField]
	WHERE WorkspaceID = @WorkspaceID
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
  FROM [dbo].[CustomFieldValue] CFV
INNER JOIN @CustomFieldMapping CFM ON CFV.CustomFieldID = CFM.OriginalCustomFieldID

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
      ,CFV.NewCustomFieldID--[CustomFieldID]
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
  FROM [dbo].[BOEPotentialRole]
WHERE WorkspaceID = @WorkspaceID
          
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
  FROM [dbo].[WorkBreakdownStructure] WBS
WHERE WBS.WorkspaceID = @WorkspaceID

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
  FROM [dbo].[TMResourceRate] TMRR
	LEFT OUTER JOIN @Resource R ON TMRR.TMResourceID = R.ResourceID
WHERE WorkspaceID = @WorkspaceID

INSERT INTO [dbo].[WorkspaceStateHistory]
           ([UpdateDT]
           ,[WorkspaceID]
           ,[CurrentWorkspaceStateID]
           ,[UpdatedWorkspaceStateID]
           ,[ChangedByETIUserID])
SELECT [UpdateDT]
      ,@NewWorkspaceID--[WorkspaceID]
      ,[CurrentWorkspaceStateID]
      ,[UpdatedWorkspaceStateID]
      ,[ChangedByETIUserID]
  FROM [dbo].[WorkspaceStateHistory]
WHERE WorkspaceID = @WorkspaceID

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
  FROM [dbo].[WorkspaceVariable]
WHERE WorkspaceID = @WorkspaceID

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
      ,@NewWorkspaceID--[WorkspaceID]
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
  FROM [dbo].[WorkspaceUserRole]
WHERE WorkspaceID = @WorkspaceID

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
  FROM [dbo].[ProPricerExport]
WHERE WorkspaceID = @WorkspaceID

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
  FROM [dbo].[ProPricerFieldXREF] X
	INNER JOIN @ProPricerExport P ON X.ProPricerExportID = P.ProPricerExportID
WHERE P.WorkspaceID = @WorkspaceID

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
  FROM [dbo].[ProPricerCustomFieldXREF] CX
	INNER JOIN @ProPricerExport P ON CX.ProPricerExportID = P.ProPricerExportID
	INNER JOIN @CustomFieldMapping CM ON CX.CustomFieldID = CM.OriginalCustomFieldID

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
  FROM [dbo].[CLIN]
WHERE WorkspaceID = @WorkspaceID

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
      ,@NewWorkspaceID--[WorkspaceID]
      ,[DisplayedCLINNumber]
  FROM [dbo].[CLIN]
WHERE CLINID = @CLINID

UPDATE @CLIN
SET	NewCLINID = SCOPE_IDENTITY(),
	Processed = 1
WHERE CLINID = @CLINID
END

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
FROM [dbo].[WBS_CLIN_BOE_XREF] X 
INNER JOIN @WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID
UNION
SELECT  X.[WBSID], X.[CLINID], X.[BOEID]
FROM [dbo].[WBS_CLIN_BOE_XREF] X 
INNER JOIN @CLIN C ON X.CLINID = C.CLINID 
UNION
SELECT  X.[WBSID], X.[CLINID], X.[BOEID]
FROM [dbo].[WBS_CLIN_BOE_XREF] X 
INNER JOIN @BOE B ON X.BOEID = B.BOEID

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
  FROM [dbo].[SumOfBOE_WorkspaceVariableXREF] X
INNER JOIN @WorkspaceVariable WV ON X.WorkspaceVariableID = WV.WorkspaceVariableID

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
  FROM [dbo].[WorkspaceVariableSumVariableResourceTypeXREF] X
  INNER JOIN @WorkspaceVariable WV ON X.WorkspaceVariableID = WV.WorkspaceVariableID

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
  FROM [dbo].[BOEStateHistory] BH
INNER JOIN @BOE B ON BH.BOEID = B.BOEID

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
  FROM [dbo].[BOEUserRoleHistory] H
INNER JOIN @BOE B ON H.BOEID = B.BOEID

INSERT INTO [dbo].[BOEUserRole]
           ([UpdateDT]
           ,[ETIUserID]
           ,[RoleID]
           ,[BOEID])
SELECT R.[UpdateDT]
      ,R.[ETIUserID]
      ,R.[RoleID]
      ,NewBOEID
  FROM [dbo].[BOEUserRole] R
INNER JOIN @BOE B ON R.BOEID = B.BOEID

INSERT INTO [dbo].[BOEApproval]
           ([UpdateDT]
           ,[BOEID]
           ,[ApprovalETIUserID]
           ,[ApprovedFlag])
SELECT B.[UpdateDT]
      ,tB.NewBOEID--[BOEID]
      ,B.[ApprovalETIUserID]
      ,B.[ApprovedFlag]
  FROM [dbo].[BOEApproval] B
INNER JOIN @BOE tB ON B.BOEID = tB.BOEID

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
  FROM [dbo].[BOEComment] B
INNER JOIN @BOE tB ON B.BOEID = tB.BOEID

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
  FROM [dbo].[BOECommentHistory] BCH
	INNER JOIN   @BOEComment tBC ON BCH.BOECommentID = tBC.BOECommentID

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
	[SortOrderID] INT,
	[AuthorUserId] int NULL
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
      ,dbo.MapToNewMoqType(TE.[MOQTypeID], GETDATE())
      ,B.NewBOEID--[BOEID]
      ,TE.[LaborTypeWarningFlag]
      ,TE.[IMS_ID]
      ,TE.[TaskElementTypeID]
      ,0
      ,NULL
      ,B.NewBOEID
	  ,TE.[SortOrderID]
	  ,TE.[AuthorUserId]
  FROM [dbo].[BOETaskElement] TE
	INNER JOIN @BOE B ON TE.BOEID = B.BOEID

/*
Fix Workspace Variables in MOQ Equations
*/
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
		   ,[AuthorUserId]
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
      ,NewBOEID--[BOEID]
      ,[LaborTypeWarningFlag]
      ,[IMS_ID]
      ,[TaskElementTypeID]
	  ,[SortOrderID]
	  ,[AuthorUserId]
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
  FROM [dbo].[OrdinaryVariable] OV
INNER JOIN @BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID

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
      ,NewBOETaskElementID--[BOETaskElementID]
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
      ,B.NewBOEID--[BOEID]
      ,H.[Approval]
      ,H.[ApprovalETIUserID]
  FROM [dbo].[BOEApprovalHistory] H
INNER JOIN @BOE B ON H.BOEID = B.BOEID

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
	[HistoricalReferenceExplanation] [varchar](max) NULL,
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
	M.[HistoricalReferenceExplanation],
	M.[SkillMix],
	0,
	NULL,
	T.NewBOETaskElementID
FROM [dbo].[MOQTypeSelection] M
INNER JOIN @BOETaskElement T ON M.TaskId = T.BOETaskElementID

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
			[HistoricalReferenceExplanation],
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
	[HistoricalReferenceExplanation],
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
	[WbsElement] [varchar](8000) NOT NULL,
	[PeriodOfPerformanceStartDate] [datetime2](7) NOT NULL,
	[PeriodOfPerformanceEndDate] [datetime2](7) NOT NULL,
	[TotalWbsHours] [decimal](11,2) NOT NULL,
	[AdditionalQueryFilters] [varchar](2500) NULL,
	[TotalRelevantHoursAfterQueryFilters] [decimal](11,2) NOT NULL,
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
FROM [dbo].[MOQTypeSelectionTableData] TD
INNER JOIN @MOQTypeSelection S ON TD.MOQTypeSelectionId = S.MOQTypeSelectionId

/* If before SAP Cutoff, reset the POP Start/End dates */
IF @SAPSpaceCutoff IS NOT NULL AND @SAPSpaceCutoff >= @CopyWorkspaceCreated
   UPDATE @MOQTypeSelectionTableData SET [PeriodOfPerformanceStartDate] = '0001-01-01', [PeriodOfPerformanceEndDate] = '0001-01-01', [QueryType] = 'Weekly ' WHERE [QueryType] = 'Weekly'

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

/** [dbo].[SkillMix] **/
DECLARE @SkillMix TABLE
(
	[SkillMixID] [int] NOT NULL,
	[Rationale] varchar(255) NOT NULL,
	[Included] [bit] NULL,
	[ProposedHours] decimal(11,2) NOT NULL,
	[HistoricalHours] decimal(11,2) NOT NULL,
	[BOESkillMix] decimal(5,2) NOT NULL,
	[LaborSkillMix] decimal(5,2) NOT NULL,
    [ResourceOld] varchar(20) NOT NULL,
    [ResourceNew] varchar(20) NOT NULL,
    [BOETaskElementID] [int] NOT NULL,
    [BOEID] [int] NOT NULL,
	[IsUserInput] bit NOT NULL,
	Processed bit,
    NewBOETaskElementID int,
    NewBOEID int
)
INSERT INTO @SkillMix
SELECT
	SM.[SkillMixID],
	SM.[Rationale],
	SM.[Included],
	SM.[ProposedHours],
	SM.[HistoricalHours],
	SM.[BOESkillMix],
	SM.[LaborSkillMix],
    SM.[ResourceOld],
    SM.[ResourceNew],
    SM.[BOETaskElementID],
    SM.[BOEID],
	SM.[IsUserInput],
	0,
    T.[NewBOETaskElementID],
    B.[NewBOEID]
FROM [dbo].[SkillMix] SM
INNER JOIN @BOE B ON SM.BOEID = B.BOEID
INNER JOIN @BOETaskElement T on T.[BOETaskElementID] = SM.[BOETaskElementID]

DECLARE @SkillMixID int
WHILE EXISTS (SELECT 1 FROM @SkillMix WHERE Processed = 0)
BEGIN
SELECT TOP 1 @SkillMixID = SkillMixID FROM @SkillMix WHERE Processed = 0
INSERT INTO [dbo].[SkillMix]
			([Rationale],
			[Included],
			[ProposedHours],
			[HistoricalHours],
			[BOESkillMix],
			[LaborSkillMix],
            [ResourceOld],
            [ResourceNew],
            [BOETaskElementID],
            [BOEID],
			[IsUserInput]
			)
SELECT
	[Rationale],
	[Included],
	[ProposedHours],
	[HistoricalHours],
	[BOESkillMix],
	[LaborSkillMix],
    [ResourceOld],
    [ResourceNew],
    [NewBOETaskElementID],
    [NewBOEID],
	[IsUserInput]
FROM @SkillMix
WHERE SkillMixID = @SkillMixID

UPDATE @SkillMix
SET Processed = 1
WHERE SkillMixID = @SkillMixID

END

/** [dbo].[CommonDisclosureSkillMix] **/
DECLARE @CommonDisclosureSkillMix TABLE
(
	[CommonDisclosureSkillMixID] [int] NOT NULL,
	[Rationale] varchar(255) NOT NULL,
	[Included] [bit] NOT NULL,
	[ProposedHours] decimal(11,2) NOT NULL,
	[HistoricalHours] decimal(11,2) NOT NULL,
	[BOESkillMix] decimal(5,2) NOT NULL,
	[LaborSkillMix] decimal(5,2) NOT NULL,
    [ResourceID] varchar(20) NOT NULL,
    [BusinessResourceID] varchar(20) NOT NULL,
    [BOEID] [int] NOT NULL,
    [BOETaskElementID] [int] NOT NULL,
	[IsUserInput] bit NOT NULL,
	Processed bit,
    NewBOEID int,
    NewBOETaskElementID int
)
INSERT INTO @CommonDisclosureSkillMix
SELECT
	CD.[CommonDisclosureSkillMixID],
	CD.[Rationale],
	CD.[Included],
	CD.[ProposedHours],
	CD.[HistoricalHours],
	CD.[BOESkillMix],
	CD.[LaborSkillMix],
    CD.[ResourceID],
    CD.[BusinessResourceID],
    CD.[BOEID],
    CD.[BOETaskElementID],
	CD.[IsUserInput],
	0,
    B.[NewBOEID],
    T.[NewBOETaskElementID]
FROM [dbo].[CommonDisclosureSkillMix] CD
INNER JOIN @BOE B ON CD.BOEID = B.BOEID
INNER JOIN @BOETaskElement T on T.[BOETaskElementID] = CD.[BOETaskElementID]

DECLARE @CommonDisclosureSkillMixID int
WHILE EXISTS (SELECT 1 FROM @CommonDisclosureSkillMix WHERE Processed = 0)
BEGIN
SELECT TOP 1 @CommonDisclosureSkillMixID = CommonDisclosureSkillMixID FROM @CommonDisclosureSkillMix WHERE Processed = 0
INSERT INTO [dbo].[CommonDisclosureSkillMix]
			([Rationale],
			[Included],
			[ProposedHours],
			[HistoricalHours],
			[BOESkillMix],
			[LaborSkillMix],
            [ResourceID],
            [BusinessResourceID],
            [BOEID],
            [BOETaskElementID],
			[IsUserInput]
			)
SELECT
	[Rationale],
	[Included],
	[ProposedHours],
	[HistoricalHours],
	[BOESkillMix],
	[LaborSkillMix],
    [ResourceID],
    [BusinessResourceID],
    [NewBOEID],
    [NewBOETaskElementID],
	[IsUserInput]
FROM @CommonDisclosureSkillMix
WHERE CommonDisclosureSkillMixID = @CommonDisclosureSkillMixID

UPDATE @CommonDisclosureSkillMix
SET Processed = 1
WHERE CommonDisclosureSkillMixID = @CommonDisclosureSkillMixID

END

/** [dbo].[SkillMixSummary] **/
DECLARE @SkillMixSummary TABLE
(
	[SkillMixSummaryID] [int] NOT NULL,
	[Rationale] varchar(255) NOT NULL,
	[Included] [bit] NOT NULL,
	[ProposedHours] decimal(11,2) NOT NULL,
	[HistoricalHours] decimal(11,2) NOT NULL,
	[ResourceHours] decimal(11,2) NOT NULL,
	[BusinessResourceHours] decimal(11,2) NOT NULL,
	[BOESkillMix] decimal(5,2) NOT NULL,
	[LaborSkillMix] decimal(5,2) NOT NULL,
    [ResourceID] varchar(20) NOT NULL,
    [BusinessResourceID] varchar(20) NOT NULL,
    [BOEID] [int] NOT NULL,
    [BOETaskElementID] [int] NOT NULL,
	[IsUserInput] bit NOT NULL,
	Processed bit,
    NewBOEID int,
    NewBOETaskElementID int
)
INSERT INTO @SkillMixSummary
SELECT
	SMS.[SkillMixSummaryID],
	SMS.[Rationale],
	SMS.[Included],
	SMS.[ProposedHours],
	SMS.[HistoricalHours],
	SMS.[ResourceHours],
	SMS.[BusinessResourceHours],
	SMS.[BOESkillMix],
	SMS.[LaborSkillMix],
    SMS.[ResourceID],
    SMS.[BusinessResourceID],
    SMS.[BOEID],
    SMS.[BOETaskElementID],
	SMS.[IsUserInput],
	0,
    B.[NewBOEID],
    T.[NewBOETaskElementID]
FROM [dbo].[SkillMixSummary] SMS
INNER JOIN @BOE B ON SMS.BOEID = B.BOEID
INNER JOIN @BOETaskElement T on T.[BOETaskElementID] = SMS.[BOETaskElementID]

DECLARE @SkillMixSummaryID int
WHILE EXISTS (SELECT 1 FROM @SkillMixSummary WHERE Processed = 0)
BEGIN
SELECT TOP 1 @SkillMixSummaryID = SkillMixSummaryID FROM @SkillMixSummary WHERE Processed = 0
INSERT INTO [dbo].[SkillMixSummary]
			([Rationale],
			[Included],
			[ProposedHours],
			[HistoricalHours],
			[ResourceHours],
			[BusinessResourceHours],
			[BOESkillMix],
			[LaborSkillMix],
            [ResourceID],
            [BusinessResourceID],
            [BOEID],
            [BOETaskElementID],
			[IsUserInput]
			)
SELECT
	[Rationale],
	[Included],
	[ProposedHours],
	[HistoricalHours],
	[ResourceHours],
	[BusinessResourceHours],
	[BOESkillMix],
	[LaborSkillMix],
    [ResourceID],
    [BusinessResourceID],
    [NewBOEID],
    [NewBOETaskElementID],
	[IsUserInput]
FROM @SkillMixSummary
WHERE SkillMixSummaryID = @SkillMixSummaryID

UPDATE @SkillMixSummary
SET Processed = 1
WHERE SkillMixSummaryID = @SkillMixSummaryID

END

/** [dbo].[MOQTypeSelectionTableDataResourceHours] **/
DECLARE @MOQTypeSelectionTableDataResourceHours TABLE
(
	[MOQTypeSelectionTableDataResourceHoursId] [int] NOT NULL,
	[ResourceName] varchar(20) NULL,
	[BRCName] varchar(20) NULL,
	[WbsHours] decimal(11,2) NOT NULL,
	[TotalHours] decimal(11,2) NOT NULL,
	[MOQTypeSelectionTableDataId] [int] NOT NULL,
	[BOETaskElementID] [int] NOT NULL,
	[BOEID] [int] NOT NULL,
	Processed bit,
	NewMOQTypeSelectionTableDataId int,
    NewBOETaskElementID int,
    NewBOEID int
)
INSERT INTO @MOQTypeSelectionTableDataResourceHours
SELECT
	M.[MOQTypeSelectionTableDataResourceHoursId],
	M.[ResourceName],
	M.[BRCName],
	M.[WbsHours],
	M.[TotalHours],
	M.[MOQTypeSelectionTableDataId],
	M.[BOETaskElementID],
	M.[BOEID],
	0,
	MOQ.[NewMOQTypeSelectionTableDataId],
    T.[NewBOETaskElementID],
    B.[NewBOEID]
FROM [dbo].[MOQTypeSelectionTableDataResourceHours] M
INNER JOIN @MOQTypeSelectionTableData MOQ ON M.MOQTypeSelectionTableDataId = MOQ.MOQTypeSelectionTableDataId
INNER JOIN @BOE B ON M.BOEID = B.BOEID
INNER JOIN @BOETaskElement T on T.[BOETaskElementID] = M.[BOETaskElementID]

DECLARE @MOQTypeSelectionTableDataResourceHoursId int
WHILE EXISTS (SELECT 1 FROM @MOQTypeSelectionTableDataResourceHours WHERE Processed = 0)
BEGIN
SELECT TOP 1 @MOQTypeSelectionTableDataResourceHoursId = MOQTypeSelectionTableDataResourceHoursId FROM @MOQTypeSelectionTableDataResourceHours WHERE Processed = 0
INSERT INTO [dbo].[MOQTypeSelectionTableDataResourceHours]
			([ResourceName],
			[BRCName],
			[WbsHours],
			[TotalHours],
			[MOQTypeSelectionTableDataId],
			[BOETaskElementID],
			[BOEID]
			)
SELECT
	[ResourceName],
	[BRCName],
	[WbsHours],
	[TotalHours],
	[NewMOQTypeSelectionTableDataId],
	[NewBOETaskElementID],
	[NewBOEID]
FROM @MOQTypeSelectionTableDataResourceHours
WHERE MOQTypeSelectionTableDataResourceHoursId = @MOQTypeSelectionTableDataResourceHoursId

UPDATE @MOQTypeSelectionTableDataResourceHours
SET Processed = 1
WHERE MOQTypeSelectionTableDataResourceHoursId = @MOQTypeSelectionTableDataResourceHoursId

END

/****** Object:  Table [dbo].[BOELaborType]    Script Date: 05/17/2012 10:50:02 ******/
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
	[BRCResourceID] [int] NULL,
	Processed bit,
	[NewBOELaborTypeID] [int],
	[NewResourceID] [int] NULL,
	[NewBRCResourceID] [int] NULL,
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
	  ,LT.[BRCResourceID]
	  ,0/*PROCESSED*/
      ,NULL
      ,CASE
		WHEN R.NewResourceID IS NOT NULL THEN R.NewResourceID
		ELSE LT.[ResourceID]
		END AS ResourceID
      ,CASE
		WHEN BR.NewResourceID IS NOT NULL THEN BR.NewResourceID
		ELSE LT.[BRCResourceID]
		END AS BRCResourceID
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
  FROM [dbo].[BOELaborType] LT
INNER JOIN @BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
LEFT OUTER JOIN @Resource R ON LT.ResourceID = R.ResourceID
LEFT OUTER JOIN @Resource BR ON LT.BRCResourceID = BR.ResourceID
LEFT OUTER JOIN @PerformingOrganization PO ON LT.PerformingOrganizationID = PO.PerformingOrganizationID
LEFT OUTER JOIN @WorkBreakdownStructure W on LT.WBSID = W.WBSID
LEFT OUTER JOIN @CLIN C on LT.CLINID = C.CLINID
	
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
		   ,[LaborSortId]
		   ,[BRCResourceID])
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
	   ,CASE 
		WHEN NewBRCResourceID IS NOT NULL THEN NewBRCResourceID
        ELSE BRCResourceID
        END AS [BRCResourceID]
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
  FROM [dbo].[BOETaskElementWorkspaceVariableXREF] X
	INNER JOIN @BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
	INNER JOIN @WorkspaceVariable WV ON X.WorkspaceVariableID = WV.WorkspaceVariableID
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
FROM [dbo].[BOETaskElementMetricDetailXREF] X
	INNER JOIN @BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
INSERT INTO [dbo].[BOETaskElementCustomFieldValueXREF]
           ([UpdateDT]
           ,[BOETaskElementID]
           ,[CustomFieldValueID])
SELECT X.[UpdateDT]
      ,TE.NewBOETaskElementID
      ,CFV.NewCustomFieldValueID
  FROM [dbo].[BOETaskElementCustomFieldValueXREF] X
	INNER JOIN @BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
	INNER JOIN @CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID
INSERT INTO [dbo].[BOELaborTypeCustomFieldValueXREF]
           ([UpdateDT]
           ,[BOELaborTypeID]
           ,[CustomFieldValueID])
SELECT X.[UpdateDT]
      ,TE.NewBOELaborTypeID
      ,CFV.NewCustomFieldValueID
  FROM [dbo].[BOELaborTypeCustomFieldValueXREF] X
	INNER JOIN @BOELaborType TE ON X.BOELaborTypeID = TE.BOELaborTypeID
	INNER JOIN @CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID
INSERT INTO [dbo].[BOECustomFieldValueXREF]
           ([UpdateDT]
           ,[BOEID]
           ,[CustomFieldValueID])
SELECT X.[UpdateDT]
      ,B.NewBOEID
      ,CFV.NewCustomFieldValueID
  FROM [dbo].[BOECustomFieldValueXREF] X
	INNER JOIN @BOE B ON X.BOEID = B.BOEID
	INNER JOIN @CustomFieldValue CFV ON X.CustomFieldValueID = CFV.CustomFieldValueID

INSERT INTO [dbo].[MoqTypeTableCustomFieldValueXREF] ([UpdateDT], [MoqTypeTableDataId], [CustomFieldValueId])
	SELECT X.[UpdateDT], t.NewMOQTypeSelectionTableDataId, CFV.NewCustomFieldValueID
		FROM [dbo].[MoqTypeTableCustomFieldValueXREF] X, @MOQTypeSelectionTableData t, @CustomFieldValue CFV
		WHERE t.MoqTypeSelectionTableDataId = X.MoqTypeTableDataId AND X.CustomFieldValueID = CFV.CustomFieldValueID

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
  FROM [dbo].[RteTemplate]
WHERE WorkspaceID = @WorkspaceID

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
	FROM [dbo].[RteTemplate]
	WHERE WorkspaceID = @WorkspaceID
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
	FROM [dbo].[RteTemplateAssigned] RA
	INNER JOIN [dbo].[RteTemplate] R ON R.[TemplateID] = RA.[TemplateID]
	WHERE R.WorkspaceID = @WorkspaceID
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
  FROM [dbo].[RteTemplateQuestion] RTQ
INNER JOIN @RTETemplateMapping RTM ON RTM.OriginalTemplateID = RTQ.TemplateID

DECLARE @QuestionID int
WHILE EXISTS (SELECT 1 FROM @RTETemplateQuestion WHERE Processed = 0)
BEGIN
	SELECT TOP 1 @QuestionID = [QuestionID] FROM @RTETemplateQuestion WHERE Processed = 0

	INSERT INTO [dbo].[RteTemplateQuestion] ([UpdateDT], [TemplateID], [Text], [SortOrder], [Required])
	SELECT RTQ.[UpdateDT], RTQ.NewTemplateID, RTQ.[Text], RTQ.[SortOrder], RTQ.[Required]
		FROM @RTETemplateQuestion RTQ
		WHERE RTQ.[QuestionID] = @QuestionID  

	UPDATE @RTETemplateQuestion
		SET NewQuestionID = SCOPE_IDENTITY(), Processed = 1
		WHERE [QuestionID] = @QuestionID  
END

INSERT INTO RTETemplateAnswer
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
	FROM RTETemplateAnswer RTA
		INNER JOIN @RTETemplateQuestion RTQ ON RTA.QuestionID = RTQ.QuestionID
		INNER JOIN @BOE B ON RTA.BOEID = B.BOEID
		LEFT JOIN @BOETaskElement T on T.[BOETaskElementID] = RTA.[TaskID]

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
  FROM [dbo].[SumOfBOE_OrdinaryVariableXREF] X
	INNER JOIN @OrdinaryVariable V ON X.OrdinaryVariableID = V.OrdinaryVariableID

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

INSERT INTO [dbo].[SumOfBOE_OrdinaryVariableXREF] ([OrdinaryVariableID], [CLINID], [WBSID], [BOEID])
	SELECT NewOrdinaryVariableID, NewCLINID, NewWBSID, NewBOEID
	FROM  @SumOfBOE_OrdinaryVariableXREF 

INSERT INTO [dbo].[OrdinaryVariableSumVariableResourceTypeXREF] ([OrdinaryVariableID], [SumVariableResourceTypeID])
	SELECT O.NewOrdinaryVariableID, [SumVariableResourceTypeID]
	FROM [dbo].[OrdinaryVariableSumVariableResourceTypeXREF] X
		INNER JOIN @OrdinaryVariable O ON X.OrdinaryVariableID = O.OrdinaryVariableID

INSERT INTO [dbo].[BOELaborSpread] ([BOELaborTypeID], [LaborSpreadDate], [LaborSpreadValue])
	SELECT LT.NewBOELaborTypeID, [LaborSpreadDate], [LaborSpreadValue]
	FROM [dbo].[BOELaborSpread] LS
		INNER JOIN @BOELaborType LT ON LS.BOELaborTypeID = LT.BOELaborTypeID

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
      ,@NewWorkspaceID--[WorkspaceID]
  FROM [dbo].[WorkspaceLockedPerDiem]
WHERE WorkspaceID = @WorkspaceID

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
      ,@NewWorkspaceID--[WorkspaceID]
  FROM [dbo].[WorkspaceLockedTravelEscalationRate]
WHERE WorkspaceID = @WorkspaceID

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
      ,@NewWorkspaceID--[WorkspaceID]
  FROM [dbo].[WorkspaceLockedTravelMiscRate]
WHERE WorkspaceID = @WorkspaceID

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
      ,@NewWorkspaceID--[WorkspaceID]
  FROM [dbo].[WorkspaceLockedTrip]
WHERE WorkspaceID = @WorkspaceID

/* Copy new BOE/INL Forms */
DECLARE @IBOE TABLE
(
	[IBOEFormID] [int] NOT NULL,
	Processed bit DEFAULT 0,
	[NEW_IBOEFormID] [int] NULL,
	NewWorkspaceID int NOT NULL
)

INSERT INTO @IBOE 
	SELECT IBOEFormID, 0, NULL, @NewWorkspaceID
	FROM [dbo].[BOEFormIBOE]
	WHERE WorkspaceID = @WorkspaceID

DECLARE @IBOEID int
WHILE EXISTS (SELECT 1 FROM @IBOE WHERE Processed = 0)
BEGIN
	SELECT TOP 1 @IBOEID = [IBOEFormID] FROM @IBOE WHERE Processed = 0
	INSERT INTO [dbo].[BOEFormIBOE]
		(UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, BusinessArea, Revision,FormVersion)
		SELECT UpdateDT, @NewWorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, BusinessArea, Revision,FormVersion
		FROM [dbo].[BOEFormIBOE]
		WHERE [IBOEFormID] = @IBOEID 

	UPDATE @IBOE 
		SET [NEW_IBOEFormID] = SCOPE_IDENTITY(), Processed = 1
		WHERE [IBOEFormID] = @IBOEID
END

--Insert the INL IBOE Resources
INSERT INTO [dbo].[BOEFormIBOEResourcesXREF]
	SELECT B.[NEW_IBOEFormID], IsNull(R.NewSystemResourceID, x.ResourceID)
	FROM [dbo].[BOEFormIBOEResourcesXREF] x
		INNER JOIN @IBOE B ON B.[IBOEFormID] = x.[IBOEFormID]
		LEFT OUTER JOIN @WorkspaceResource R ON x.ResourceID = R.[SystemResourceID]
	WHERE x.[IBOEFormID] IN (SELECT [IBOEFormID] FROM @IBOE)

DECLARE @PBOE TABLE
(
	[PBOEFormID] [int] NOT NULL,
	Processed bit DEFAULT 0,
	[NEW_PBOEFormID] [int] NULL,
	NewWorkspaceID int NOT NULL
)

INSERT INTO @PBOE 
	SELECT PBOEFormID, 0, NULL, @NewWorkspaceID
	FROM [dbo].[BOEFormPBOE]
	WHERE WorkspaceID = @WorkspaceID

DECLARE @PBOEID int
WHILE EXISTS (SELECT 1 FROM @PBOE WHERE Processed = 0)
BEGIN
	SELECT TOP 1 @PBOEID = [PBOEFormID] FROM @PBOE WHERE Processed = 0
	INSERT INTO [dbo].[BOEFormPBOE]
		(UpdateDT, WorkspaceID, FormName, Description, [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion,  
		[CCoPD],
		[CCoPDOtherText],
		[RFP],
		[ProposalNumber],
		[SupplierName],
		[ValidityDate],
		[ShouldCostEstimate],
		[ShouldCostEstimateDate],
		[RFPRelease],
		[RFPReleaseDate],
		[FirmSupplierReceipt],
		[FirmSupplierReceiptDate],
		[SourceSelection],
		[SourceSelectionDate],
		[CID],
		[CIDDate],
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
		[PlannedDate_WrittenApproval],
		[PlannedDate_ApprovedSubmission],
		[CIDText],
		[PriceAnalysisText],
		[TechnicalEvaluationText],
		[FactFindingText],
		[CostAnalysisText],
		[GovtPricingText],
		[SupplierNegotiationsText],
		[MOUText],
		ShouldCostEstimateText,
		RFPReleaseText,
		FirmSupplierReceiptText,
		SourceSelectionText
		,[SupplierCCoPD]
		,[SourceSelectionDescription]
		,[CommercialityDescription]
		,[TechnicalEvaluationDescription]
		,[PriceAnalysisDescription]
		,[CostAnalysisDescription]
		,[RationaleValueSummary]
		,[GovtPricingReceived]
		,[GovtPricingReceivedDate]
		,[GovtPricingReceivedText]
		,[CostAnalysisUnqual]
		,[CostAnalysisUnqualDate]
		,[CostAnalysisUnqualText]
		,[VendorId]
		,[SupplierProposedValue])
	SELECT UpdateDT, @NewWorkspaceID, FormName, Description, [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion, 
		[CCoPD],
		[CCoPDOtherText],
		[RFP],
		[ProposalNumber],
		[SupplierName],
		[ValidityDate],
		[ShouldCostEstimate],
		[ShouldCostEstimateDate],
		[RFPRelease],
		[RFPReleaseDate],
		[FirmSupplierReceipt],
		[FirmSupplierReceiptDate],
		[SourceSelection],
		[SourceSelectionDate],
		[CID],
		[CIDDate],
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
		[PlannedDate_WrittenApproval],
		[PlannedDate_ApprovedSubmission],
		[CIDText],
		[PriceAnalysisText],
		[TechnicalEvaluationText],
		[FactFindingText],
		[CostAnalysisText],
		[GovtPricingText],
		[SupplierNegotiationsText],
		[MOUText],
		ShouldCostEstimateText,
		RFPReleaseText,
		FirmSupplierReceiptText,
		SourceSelectionText
		,[SupplierCCoPD]
		,[SourceSelectionDescription]
		,[CommercialityDescription]
		,[TechnicalEvaluationDescription]
		,[PriceAnalysisDescription]
		,[CostAnalysisDescription]
		,[RationaleValueSummary]
		,[GovtPricingReceived]
		,[GovtPricingReceivedDate]
		,[GovtPricingReceivedText]
		,[CostAnalysisUnqual]
		,[CostAnalysisUnqualDate]
		,[CostAnalysisUnqualText]
		,[VendorId]
		,[SupplierProposedValue]
	  FROM [dbo].[BOEFormPBOE]
	WHERE [PBOEFormID] = @PBOEID 
	UPDATE @PBOE 
		SET	[NEW_PBOEFormID] = SCOPE_IDENTITY(), Processed = 1
		WHERE [PBOEFormID] = @PBOEID
END

--Insert the INL PBOE Resources
INSERT INTO [dbo].[BOEFormPBOEResourcesXREF]
	SELECT B.[NEW_PBOEFormID], IsNull(R.NewSystemResourceID, x.ResourceID)
	FROM [dbo].[BOEFormPBOEResourcesXREF] x
		INNER JOIN @PBOE B ON B.[PBOEFormID] = x.[PBOEFormID]
		LEFT OUTER JOIN @WorkspaceResource R ON x.ResourceID = R.[SystemResourceID]
	WHERE x.[PBOEFormID] IN (SELECT [PBOEFormID] FROM @PBOE)

-- Insert the INL IBOE CLIN selections
INSERT INTO [dbo].[BOEFormIBOECLINsXREF]
	SELECT B.[NEW_IBOEFormID], c.[NewCLINID], x.[ContractType]
	FROM [dbo].[BOEFormIBOECLINsXREF] x
		INNER JOIN @IBOE B ON B.[IBOEFormID] = x.[IBOEFormID]
		INNER JOIN @CLIN C on C.[CLINID] = x.[CLINID]

-- Insert the INL PBOE CLIN selections
INSERT INTO [dbo].[BOEFormPBOECLINsXREF]
	SELECT B.[NEW_PBOEFormID], c.[NewCLINID], x.[ContractType]
	FROM [dbo].[BOEFormPBOECLINsXREF] x
		INNER JOIN @PBOE B ON B.[PBOEFormID] = x.[PBOEFormID]
		INNER JOIN @CLIN C on C.[CLINID] = x.[CLINID]

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
			  ,(SELECT COUNT (*) FROM [dbo].[BOE] WHERE WorkspaceID = @CopyFromWorkspaceID) AS [NumOfBOEs]
			  ,@CreateDate
		  FROM [dbo].[Workspace] W
			LEFT OUTER JOIN [dbo].[LineOfBusiness] LOB ON W.LineOfBusinessID = LOB.LineOfBusinessID
		  WHERE WorkspaceID = @CopyFromWorkspaceID
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
			INSERT INTO [dbo].[WorkspaceCopyMetric] ([SourceWorkspaceID], [TargetWorkspaceID], [CreateDate])
				VALUES (@CopyFromWorkspaceID, @NewWorkspaceID, @CreateDate)
		END

		SELECT @NewWorkspaceID AS WorkspaceID
	END

END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION
	-- Useful for Debugging, uncomment and run in SSMS, then execute Sproc in SSMS to see the line number of the error
	--SELECT  
 --       ERROR_NUMBER() AS ErrorNumber  
 --       ,ERROR_SEVERITY() AS ErrorSeverity  
 --       ,ERROR_STATE() AS ErrorState  
 --       ,ERROR_PROCEDURE() AS ErrorProcedure  
 --       ,ERROR_LINE() AS ErrorLine  
 --       ,ERROR_MESSAGE() AS ErrorMessage; 
 
	DECLARE @ErrorMessage varchar (500)
	SELECT @ErrorMessage = ERROR_MESSAGE()
	RAISERROR (@ErrorMessage, 11, 1)
	RETURN
END CATCH
GO


-- CREATE/ALTER copyWorkspaceVersion
CREATE OR ALTER PROCEDURE [dbo].[copyWorkspaceVersion]
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
**		1/4/2021	Dusan				BOEJ-4894: Added support for MoqTypeTableCustomFieldValueXREF
**		4/13/2022	jquijano			IES-1014: Remove deprecated PBOE fields
**		4/22/2022	jquijano			IES-1019: Add new fields to copy workspace
**		5/2/2022	jquijano			IES-1126: Add VendorId, SupplierProposedValue
**		1/31/23		e405721				ACV-221 - Enable SAP Connection
**		3/1/23		twilson3			ACV-343 Update MOQ Column sizes
**		3/20/23		Dusan				ACV-498: Updated MOQ Column size (Wbs Element due to prod issue)
**      8/10/23     twilson             PROPH-1029 Investigate Project Spreads
**		1/18/24		ranzalon			PROPH-1070 Update for HistoricalReferenceExplanation
**		1/28/24		e302876  			PROPH-1492 ADD BRC to Copy BOEs, Copy WS, Archive/Restore
**		7/11/24		e405721				PROPH-2156: Update Copy Workspace Version for Skill Mix, Common Disclosure, and MOQ Type Resource Hours Table Data
**		07/11/24	twilson3			proph-2166 Missing Columns
**		07/12/24	twilson3			proph-2019 Fix @Temp Table definitions
**		07/30/24	e405721				PROPH-2218 Add BRC Name into MOQ Type Selection Table Data Resource Hours Table
**		10/15/24	e405721				PROPH-2392: Update for Skill Mix V2
**		1/14/25		twilson3			PROPH-2596 - Add UCOT Factor
**		1/15/25		e309214				PROPH-1854 Database Changes for Assign Author
**		2/4/2025	twilson3			PROPH-2786 Link BRCs to new workspace, not old workspace
**		9/30/25		e378233				PROPH-3302 Updated Copy Workspace Version for Skill Mix Summary
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
			   ,[EnableSAPConnection]
			   ,[UCOTFactor]
			   ,[EnableAssignTaskAuthor]
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
		  ,[EnableSAPConnection]
		  ,[UCOTFactor]
		  ,[EnableAssignTaskAuthor]
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
	WHERE S.WorkspaceID = @WorkspaceID AND S.VersionID = @VersionID

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
		[SortOrderID] INT,
		[AuthorUserId] int NULL
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
		  ,TE.[AuthorUserId]
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
			   ,[AuthorUserId]
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
		  ,[AuthorUserId]
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
		[HistoricalReferenceExplanation] [varchar](max) NULL,
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
		M.[HistoricalReferenceExplanation],
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
				[HistoricalReferenceExplanation],
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
		[HistoricalReferenceExplanation],
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
		[WbsElement] [varchar](8000) NOT NULL,
		[PeriodOfPerformanceStartDate] [datetime2](7) NOT NULL,
		[PeriodOfPerformanceEndDate] [datetime2](7) NOT NULL,
		[TotalWbsHours] [decimal](11,2) NOT NULL,
		[AdditionalQueryFilters] [varchar](2500) NULL,
		[TotalRelevantHoursAfterQueryFilters] [decimal](11,2) NOT NULL,
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

	
	/** [dbo].[SkillMix] **/
	DECLARE @SkillMix TABLE
	(
		[SkillMixID] [int] NOT NULL,
		[Rationale] varchar(255) NOT NULL,
		[Included] [bit] NULL,
		[ProposedHours] decimal(11,2) NOT NULL,
		[HistoricalHours] decimal(11,2) NOT NULL,
		[BOESkillMix] decimal(5,2) NOT NULL,
		[LaborSkillMix] decimal(5,2) NOT NULL,
		[ResourceOld] varchar(20) NOT NULL,
		[ResourceNew] varchar(20) NOT NULL,
		[BOETaskElementID] [int] NOT NULL,
		[BOEID] [int] NOT NULL,
		[IsUserInput] bit NOT NULL,
		Processed bit,
		NewBOETaskElementID int,
		NewBOEID int
	)
	INSERT INTO @SkillMix
	SELECT
		SM.[SkillMixID],
		SM.[Rationale],
		SM.[Included],
		SM.[ProposedHours],
		SM.[HistoricalHours],
		SM.[BOESkillMix],
		SM.[LaborSkillMix],
		SM.[ResourceOld],
		SM.[ResourceNew],
		SM.[BOETaskElementID],
		SM.[BOEID],
		SM.[IsUserInput],
		0,
		T.[NewBOETaskElementID],
		B.[NewBOEID]
	FROM [version].[SkillMix] SM
	INNER JOIN @BOE B ON SM.BOEID = B.BOEID
	INNER JOIN @BOETaskElement T on T.[BOETaskElementID] = SM.[BOETaskElementID]
	WHERE SM.VersionId = @VersionID

	DECLARE @SkillMixID int
	WHILE EXISTS (SELECT 1 FROM @SkillMix WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @SkillMixID = SkillMixID FROM @SkillMix WHERE Processed = 0
	INSERT INTO [dbo].[SkillMix]
			([Rationale],
			[Included],
			[ProposedHours],
			[HistoricalHours],
			[BOESkillMix],
			[LaborSkillMix],
            [ResourceOld],
            [ResourceNew],
            [BOETaskElementID],
            [BOEID],
			[IsUserInput]
			)
	SELECT
		[Rationale],
		[Included],
		[ProposedHours],
		[HistoricalHours],
		[BOESkillMix],
		[LaborSkillMix],
		[ResourceOld],
		[ResourceNew],
		[NewBOETaskElementID],
		[NewBOEID],
		[IsUserInput]
	FROM @SkillMix
	WHERE SkillMixID = @SkillMixID

	UPDATE @SkillMix
	SET Processed = 1
	WHERE SkillMixID = @SkillMixID

	END

	/** [dbo].[CommonDisclosureSkillMix] **/
	DECLARE @CommonDisclosureSkillMix TABLE
	(
		[CommonDisclosureSkillMixID] [int] NOT NULL,
		[Rationale] varchar(255) NOT NULL,
		[Included] [bit] NOT NULL,
		[ProposedHours] decimal(11,2) NOT NULL,
		[HistoricalHours] decimal(11,2) NOT NULL,
		[BOESkillMix] decimal(5,2) NOT NULL,
		[LaborSkillMix] decimal(5,2) NOT NULL,
		[ResourceID] varchar(20) NOT NULL,
		[BusinessResourceID] varchar(20) NOT NULL,
	    [BOEID] [int] NOT NULL,
	    [BOETaskElementID] [int] NOT NULL,
		[IsUserInput] bit NOT NULL,
		Processed bit,
	    NewBOEID int,
	    NewBOETaskElementID int
	)
	INSERT INTO @CommonDisclosureSkillMix
	SELECT
		CD.[CommonDisclosureSkillMixID],
		CD.[Rationale],
		CD.[Included],
		CD.[ProposedHours],
		CD.[HistoricalHours],
		CD.[BOESkillMix],
		CD.[LaborSkillMix],
	    CD.[ResourceID],
	    CD.[BusinessResourceID],
	    CD.[BOEID],
	    CD.[BOETaskElementID],
		CD.[IsUserInput],
		0,
	    B.[NewBOEID],
	    T.[NewBOETaskElementID]
	FROM [version].[CommonDisclosureSkillMix] CD
	INNER JOIN @BOE B ON CD.BOEID = B.BOEID
	INNER JOIN @BOETaskElement T on T.[BOETaskElementID] = CD.[BOETaskElementID]
	WHERE CD.VersionId = @VersionID
	
	DECLARE @CommonDisclosureSkillMixID int
	WHILE EXISTS (SELECT 1 FROM @CommonDisclosureSkillMix WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @CommonDisclosureSkillMixID = CommonDisclosureSkillMixID FROM @CommonDisclosureSkillMix WHERE Processed = 0
	INSERT INTO [dbo].[CommonDisclosureSkillMix]
				([Rationale],
				[Included],
				[ProposedHours],
				[HistoricalHours],
				[BOESkillMix],
				[LaborSkillMix],
	            [ResourceID],
	            [BusinessResourceID],
	            [BOEID],
	            [BOETaskElementID],
				[IsUserInput]
				)
	SELECT
		[Rationale],
		[Included],
		[ProposedHours],
		[HistoricalHours],
		[BOESkillMix],
		[LaborSkillMix],
	    [ResourceID],
	    [BusinessResourceID],
	    [NewBOEID],
	    [NewBOETaskElementID],
		[IsUserInput]
	FROM @CommonDisclosureSkillMix
	WHERE CommonDisclosureSkillMixID = @CommonDisclosureSkillMixID
	
	UPDATE @CommonDisclosureSkillMix
	SET Processed = 1
	WHERE CommonDisclosureSkillMixID = @CommonDisclosureSkillMixID
	
	END
	

	/** [dbo].[SkillMixSummary] **/
	DECLARE @SkillMixSummary TABLE
	(
		[SkillMixSUmmaryID] [int] NOT NULL,
		[Rationale] varchar(255) NOT NULL,
		[Included] [bit] NOT NULL,
		[ProposedHours] decimal(11,2) NOT NULL,
		[HistoricalHours] decimal(11,2) NOT NULL,
		[ResourceHours] decimal(11,2) NOT NULL,
		[BusinessResourceHours] decimal(11,2) NOT NULL,
		[BOESkillMix] decimal(5,2) NOT NULL,
		[LaborSkillMix] decimal(5,2) NOT NULL,
		[ResourceID] varchar(20) NOT NULL,
		[BusinessResourceID] varchar(20) NOT NULL,
	    [BOEID] [int] NOT NULL,
	    [BOETaskElementID] [int] NOT NULL,
		[IsUserInput] bit NOT NULL,
		Processed bit,
	    NewBOEID int,
	    NewBOETaskElementID int
	)
	INSERT INTO @SkillMixSummary
	SELECT
		SMS.[SkillMixSummaryID],
		SMS.[Rationale],
		SMS.[Included],
		SMS.[ProposedHours],
		SMS.[HistoricalHours],
		SMS.[ResourceHours],
		SMS.[BusinessResourceHours],
		SMS.[BOESkillMix],
		SMS.[LaborSkillMix],
	    SMS.[ResourceID],
	    SMS.[BusinessResourceID],
	    SMS.[BOEID],
	    SMS.[BOETaskElementID],
		SMS.[IsUserInput],
		0,
	    B.[NewBOEID],
	    T.[NewBOETaskElementID]
	FROM [version].[SkillMixSummary] SMS
	INNER JOIN @BOE B ON SMS.BOEID = B.BOEID
	INNER JOIN @BOETaskElement T on T.[BOETaskElementID] = SMS.[BOETaskElementID]
	WHERE SMS.VersionId = @VersionID
	
	DECLARE @SkillMixSummaryID int
	WHILE EXISTS (SELECT 1 FROM @SkillMixSummary WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @SkillMixSummaryID = SkillMixSummaryID FROM @SkillMixSummary WHERE Processed = 0
	INSERT INTO [dbo].[SkillMixSummary]
				([Rationale],
				[Included],
				[ProposedHours],
				[HistoricalHours],
				[ResourceHours],
				[BusinessResourceHours],
				[BOESkillMix],
				[LaborSkillMix],
	            [ResourceID],
	            [BusinessResourceID],
	            [BOEID],
	            [BOETaskElementID],
				[IsUserInput]
				)
	SELECT
		[Rationale],
		[Included],
		[ProposedHours],
		[HistoricalHours],
		[ResourceHours],
		[BusinessResourceHours],
		[BOESkillMix],
		[LaborSkillMix],
	    [ResourceID],
	    [BusinessResourceID],
	    [NewBOEID],
	    [NewBOETaskElementID],
		[IsUserInput]
	FROM @SkillMixSummary
	WHERE SkillMixSummaryID = @SkillMixSummaryID
	
	UPDATE @SkillMixSummary
	SET Processed = 1
	WHERE SkillMixSummaryID = @SkillMixSummaryID
	
	END
	
	/** [dbo].[MOQTypeSelectionTableDataResourceHours] **/
	DECLARE @MOQTypeSelectionTableDataResourceHours TABLE
	(
		[MOQTypeSelectionTableDataResourceHoursId] [int] NOT NULL,
		[ResourceName] varchar(20) NULL,
		[BRCName] varchar(20) NULL,
		[WbsHours] decimal(11,2) NOT NULL,
		[TotalHours] decimal(11,2) NOT NULL,
		[MOQTypeSelectionTableDataId] [int] NOT NULL,
		[BOETaskElementID] [int] NOT NULL,
		[BOEID] [int] NOT NULL,
		Processed bit,
		NewMOQTypeSelectionTableDataId int,
	    NewBOETaskElementID int,
	    NewBOEID int
	)
	INSERT INTO @MOQTypeSelectionTableDataResourceHours
	SELECT
		M.[MOQTypeSelectionTableDataResourceHoursId],
		M.[ResourceName],
		M.[BRCName],
		M.[WbsHours],
		M.[TotalHours],
		M.[MOQTypeSelectionTableDataId],
		M.[BOETaskElementID],
		M.[BOEID],
		0,
		MOQ.[NewMOQTypeSelectionTableDataId],
	    T.[NewBOETaskElementID],
	    B.[NewBOEID]
	FROM [version].[MOQTypeSelectionTableDataResourceHours] M
	INNER JOIN @MOQTypeSelectionTableData MOQ ON M.MOQTypeSelectionTableDataId = MOQ.MOQTypeSelectionTableDataId
	INNER JOIN @BOE B ON M.BOEID = B.BOEID
	INNER JOIN @BOETaskElement T on T.[BOETaskElementID] = M.[BOETaskElementID]
	WHERE M.VersionId = @VersionID
	
	DECLARE @MOQTypeSelectionTableDataResourceHoursId int
	WHILE EXISTS (SELECT 1 FROM @MOQTypeSelectionTableDataResourceHours WHERE Processed = 0)
	BEGIN
	SELECT TOP 1 @MOQTypeSelectionTableDataResourceHoursId = MOQTypeSelectionTableDataResourceHoursId FROM @MOQTypeSelectionTableDataResourceHours WHERE Processed = 0
	INSERT INTO [dbo].[MOQTypeSelectionTableDataResourceHours]
				([ResourceName],
				[BRCName],
				[WbsHours],
				[TotalHours],
				[MOQTypeSelectionTableDataId],
				[BOETaskElementID],
				[BOEID]
				)
	SELECT
		[ResourceName],
		[BRCName],
		[WbsHours],
		[TotalHours],
		[NewMOQTypeSelectionTableDataId],
		[NewBOETaskElementID],
		[NewBOEID]
	FROM @MOQTypeSelectionTableDataResourceHours
	WHERE MOQTypeSelectionTableDataResourceHoursId = @MOQTypeSelectionTableDataResourceHoursId
	
	UPDATE @MOQTypeSelectionTableDataResourceHours
	SET Processed = 1
	WHERE MOQTypeSelectionTableDataResourceHoursId = @MOQTypeSelectionTableDataResourceHoursId
	
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
		[BRCResourceID] [int] NULL,
		Processed bit,
		[NewBOELaborTypeID] [int],
		[NewResourceID] [int],
		[NewBRCResourceID] [int],
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
		  ,LT.[BRCResourceID]
		  ,0/*PROCESSED*/
		  ,NULL
		  ,CASE
			WHEN R.NewResourceID IS NOT NULL THEN R.NewResourceID
			ELSE LT.[ResourceID]
			END AS ResourceID
		  ,CASE
			WHEN BRC.NewResourceID IS NOT NULL THEN BRC.NewResourceID
			ELSE LT.[BRCResourceID]
			END AS BRCResourceID
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
	LEFT OUTER JOIN @Resource BRC on LT.BRCResourceID = R.ResourceID
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
			   ,[LaborSortId]
			   ,[BRCResourceID])
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
			,CASE 
			  WHEN [NewBRCResourceID] IS NOT NULL THEN [NewBRCResourceID]
			  ELSE [BRCResourceID]
			  END AS [BRCResourceID]     
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
	INSERT INTO [dbo].[MoqTypeTableCustomFieldValueXREF] ([UpdateDT], [MoqTypeTableDataId], [CustomFieldValueId])
		SELECT X.[UpdateDT], t.NewMOQTypeSelectionTableDataId, CFV.NewCustomFieldValueID
			FROM [version].[MoqTypeTableCustomFieldValueXREF] X, @MOQTypeSelectionTableData t, @CustomFieldValue CFV
			WHERE t.MoqTypeSelectionTableDataId = X.MoqTypeTableDataId AND X.CustomFieldValueID = CFV.CustomFieldValueID AND X.VersionId = @VersionID

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
		(UpdateDT, WorkspaceID, FormName, Description, [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion,  
		[CCoPD],
		[CCoPDOtherText],
		[RFP],
		[ProposalNumber],
		[SupplierName],
		[ValidityDate],
		[ShouldCostEstimate],
		[ShouldCostEstimateDate],
		[RFPRelease],
		[RFPReleaseDate],
		[FirmSupplierReceipt],
		[FirmSupplierReceiptDate],
		[SourceSelection],
		[SourceSelectionDate],
		[CID],
		[CIDDate],
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
		[PlannedDate_WrittenApproval],
		[PlannedDate_ApprovedSubmission],
		[CIDText],
		[PriceAnalysisText],
		[TechnicalEvaluationText],
		[FactFindingText],
		[CostAnalysisText],
		[GovtPricingText],
		[SupplierNegotiationsText],
		[MOUText],
		ShouldCostEstimateText,
		RFPReleaseText,
		FirmSupplierReceiptText,
		SourceSelectionText
		,[SupplierCCoPD]
		,[SourceSelectionDescription]
		,[CommercialityDescription]
		,[TechnicalEvaluationDescription]
		,[PriceAnalysisDescription]
		,[CostAnalysisDescription]
		,[RationaleValueSummary]
		,[GovtPricingReceived]
		,[GovtPricingReceivedDate]
		,[GovtPricingReceivedText]
		,[CostAnalysisUnqual]
		,[CostAnalysisUnqualDate]
		,[CostAnalysisUnqualText]
		,[VendorId]
		,[SupplierProposedValue])
	SELECT UpdateDT, @NewWorkspaceID, FormName, Description, [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion, 
		[CCoPD],
		[CCoPDOtherText],
		[RFP],
		[ProposalNumber],
		[SupplierName],
		[ValidityDate],
		[ShouldCostEstimate],
		[ShouldCostEstimateDate],
		[RFPRelease],
		[RFPReleaseDate],
		[FirmSupplierReceipt],
		[FirmSupplierReceiptDate],
		[SourceSelection],
		[SourceSelectionDate],
		[CID],
		[CIDDate],
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
		[PlannedDate_WrittenApproval],
		[PlannedDate_ApprovedSubmission],
		[CIDText],
		[PriceAnalysisText],
		[TechnicalEvaluationText],
		[FactFindingText],
		[CostAnalysisText],
		[GovtPricingText],
		[SupplierNegotiationsText],
		[MOUText],
		ShouldCostEstimateText,
		RFPReleaseText,
		FirmSupplierReceiptText,
		SourceSelectionText
		,[SupplierCCoPD]
		,[SourceSelectionDescription]
		,[CommercialityDescription]
		,[TechnicalEvaluationDescription]
		,[PriceAnalysisDescription]
		,[CostAnalysisDescription]
		,[RationaleValueSummary]
		,[GovtPricingReceived]
		,[GovtPricingReceivedDate]
		,[GovtPricingReceivedText]
		,[CostAnalysisUnqual]
		,[CostAnalysisUnqualDate]
		,[CostAnalysisUnqualText]
		,[VendorId]
		,[SupplierProposedValue]
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


-- CREATE/ALTER deleteBOE
CREATE OR ALTER PROCEDURE [dbo].[deleteBOE]
(
@BOEID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOE]
**		Desc: Delete all parts of BOE
**			
**		
**
**		Auth: Don Canuso
**		Date: 8/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**      1/17/17		twilson3			BOEJ-1604 Cleanup DB Project
**		7/21/17		twilson3			BOEJ-2408 Remove WBS/CLIN/BOE Ref only when this is last WBS->CLIN ref
**		8/30/2017	Dusan				Fixing up performance
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		4/2/18		ranzalon			BOEJ-3268 - Update for Open Ended Custom Fields
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**		12/13/19	twilson3			BOEJ-4434 - RTE Template Answers
**		10/29/20	Dusan				BOEJ-4924 - MOQ Type Selection data
**		1/4/2021	Dusan				BOEJ-4894: Added support for MoqTypeTableCustomFieldValueXREF
**		7/17/2024	e405721				PROPH-2163: Update for Skill Mix, Common Disclosure Skill Mix, MOQ Type Selection Table Data Resource Hours
**		9/30/25		e378233				PROPH-3302 Updated for Skill Mix Summary
*****************************************************************************/
SET NOCOUNT ON 

	IF (SELECT UpdateDT FROM [dbo].[BOE] WHERE BOEID = @BOEID) = @UpdateDT
		BEGIN
			SET @UpdateDT = GETDATE()
			
			DELETE FROM dbo.BOELaborSpread
			FROM dbo.BOELaborSpread LS
				INNER JOIN dbo.BOELaborType LT ON LS.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID

			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @LaborTypeCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @LaborTypeCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID
			
			DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID
			
			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @LaborTypeCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)
				
			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF
			FROM dbo.SumOfBOE_OrdinaryVariableXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID
								
			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF WHERE BOEID = @BOEID
				
			DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF
			FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID
				
			DELETE FROM dbo.OrdinaryVariable
			FROM dbo.OrdinaryVariable OV
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID

			-- Delete RTE Template Answers
			DELETE FROM dbo.[RteTemplateAnswer]
			WHERE BOEID = @BOEID
				
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @BoeCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @BoeCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOECustomFieldValueXREF
			WHERE BOEID = @BOEID

			DELETE FROM dbo.BOECustomFieldValueXREF WHERE BOEID = @BOEID
			
			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @BoeCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)

			DELETE FROM dbo.BOELaborType
			FROM dbo.BOELaborType LT 
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID
				
			DELETE FROM dbo.BOEApprovalHistory WHERE BOEID = @BOEID
			DELETE FROM dbo.BOECommentHistory WHERE BOEID = @BOEID
								
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @TaskCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @TaskCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOETaskElementCustomFieldValueXREF X
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
			WHERE TE.BOEID = @BOEID

			DELETE FROM dbo.BOETaskElementCustomFieldValueXREF
			FROM dbo.BOETaskElementCustomFieldValueXREF X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
			WHERE TE.BOEID = @BOEID
		
			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @TaskCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)

			DELETE FROM [dbo].[BOETaskElementMetricDetailXREF]
				FROM [dbo].[BOETaskElementMetricDetailXREF] X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
			WHERE TE.BOEID = @BOEID
				
			DELETE FROM dbo.BOETaskElementWorkspaceVariableXREF
			FROM  dbo.BOETaskElementWorkspaceVariableXREF X
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID
			
			-- MOQ Type Selection data
			DELETE FROM dbo.[MoqTypeTableCustomFieldValueXREF]
					FROM dbo.[MoqTypeTableCustomFieldValueXREF] x
						INNER JOIN MoqTypeSelectionTableData t ON t.MoqTypeSelectionTableDataId = x.MoqTypeTableDataId 
						INNER JOIN MoqTypeSelection mS ON mS.MoqTypeSelectionId = t.MoqTypeSelectionId 
						INNER JOIN BoeTaskElement tE ON tE.BoeTaskElementId = mS.TaskId 
					WHERE tE.BOEID = @BOEID
			
			DELETE m
					FROM dbo.[MOQTypeSelectionTableDataResourceHours] m
			WHERE m.BOEID = @BOEID
			
			DELETE FROM dbo.MOQTypeSelectionTableData
				FROM dbo.MOQTypeSelectionTableData t
				INNER JOIN dbo.MOQTypeSelection s ON s.MOQTypeSelectionId = t.MOQTypeSelectionId
				INNER JOIN dbo.BOETaskElement TE ON s.TaskId = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID

			DELETE cd
					FROM dbo.CommonDisclosureSkillMix cd
			WHERE cd.BOEID = @BOEID

			DELETE sm
					FROM dbo.[SkillMix] sm
			WHERE sm.BOEID = @BOEID

			DELETE sms
					FROM dbo.SkillMixSummary sms
			WHERE sms.BOEID = @BOEID

			DELETE FROM dbo.MOQTypeSelection
				FROM dbo.MOQTypeSelection s
				INNER JOIN dbo.BOETaskElement TE ON s.TaskId = TE.BOETaskElementID
			WHERE TE.BOEID = @BOEID

			DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF WHERE BOEID = @BOEID
			DELETE FROM dbo.BOEUserRoleHistory WHERE BOEID = @BOEID
			DELETE FROM dbo.BOEUserRole WHERE BOEID = @BOEID
			DELETE FROM dbo.BOETaskElement WHERE BOEID = @BOEID
			DELETE FROM dbo.BOEComment WHERE BOEID = @BOEID					
			DELETE FROM dbo.BOEApproval WHERE BOEID = @BOEID	
			DELETE FROM dbo.BOEStateHistory WHERE BOEID = @BOEID
			DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF WHERE BOEID = @BOEID
			DELETE FROM dbo.WBS_CLIN_BOE_XREF WHERE BOEID = @BOEID AND (CLINID IS NULL OR WBSID IS NULL)

			-- Delete the XREF if there are other XREFs for this WBSID
			-- Otherwise, only remove the BOEID from the XREF

			DECLARE @NumberXREFs int
			DECLARE @WBSID int
			SELECT @WBSID = WBSID FROM dbo.WBS_CLIN_BOE_XREF WHERE BOEID = @BOEID

			SELECT @NumberXREFs = COUNT(*) FROM dbo.WBS_CLIN_BOE_XREF WHERE WBSID = @WBSID

			IF (@NumberXREFs > 1)
				BEGIN
					DELETE FROM dbo.WBS_CLIN_BOE_XREF WHERE BOEID = @BOEID
				END
			ELSE
				BEGIN
					UPDATE dbo.WBS_CLIN_BOE_XREF SET BOEID = NULL WHERE BOEID = @BOEID
				END

			DELETE FROM dbo.MaterialTaskElement WHERE BOEID = @BOEID
			
			DELETE FROM dbo.ODCSpread                 
			FROM dbo.ODCSpread S
				INNER JOIN dbo.ODCType T ON S.ODCTypeID = T.ODCTypeID
				INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID
			WHERE TE.BOEID = @BOEID
			
			DELETE FROM dbo.ODCType                 
			FROM dbo.ODCType T 
				INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID
			WHERE TE.BOEID = @BOEID

			DELETE FROM dbo.ODCTaskElement WHERE BOEID = @BOEID

			DELETE FROM dbo.TravelTripCustomFieldValueXREF
				FROM dbo.TravelTripCustomFieldValueXREF X
				INNER JOIN dbo.TravelTrip T ON X.TravelTripID = T.TravelTripID
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID

			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @MSTTripCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @MSTTripCustomFieldXrefs
			SELECT MSTCustomFieldValueID
			FROM dbo.MSTTravelTripCustomFieldValueXREF X
				INNER JOIN dbo.MSTTravelTrip T ON X.mstTravelTripID = T.mstTravelTripID
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID

			DELETE FROM dbo.MSTTravelTripCustomFieldValueXREF
				FROM dbo.MSTTravelTripCustomFieldValueXREF X
				INNER JOIN dbo.MSTTravelTrip T ON X.mstTravelTripID = T.mstTravelTripID
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID
			
			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @MSTTripCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)

			DECLARE @TripAffected TABLE (TripID int)					
			INSERT INTO @TripAffected 
			SELECT TripID
			FROM dbo.TravelTrip T
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID
			
			DELETE FROM dbo.TravelTrip                 
			FROM dbo.TravelTrip T 
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID

			DELETE FROM dbo.mstTravelTrip                 
			FROM dbo.mstTravelTrip T 
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID

			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @TravelTaskCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @TravelTaskCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.TravelTripTaskElementCustomFieldValueXREF X
				INNER JOIN dbo.TravelTripTaskElement TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID

			DELETE FROM dbo.TravelTripTaskElementCustomFieldValueXREF				
				FROM dbo.TravelTripTaskElementCustomFieldValueXREF X
				INNER JOIN dbo.TravelTripTaskElement TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
			WHERE TE.BOEID = @BOEID

			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @TravelTaskCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)

			DELETE FROM dbo.TravelTripTaskElement WHERE BOEID = @BOEID

			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID FROM dbo.BOE WHERE BOEID = @BOEID

			DELETE FROM dbo.BOE	WHERE BOEID = @BOEID
				
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =    'The BOE with ID ' + CAST(@BOEID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO


-- CREATE/ALTER deleteBOETaskElement
CREATE OR ALTER PROCEDURE [dbo].[deleteBOETaskElement]
(
@BOETaskElementID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteBOETaskElement]
**		Desc: Delete Flag set in LM Task Element Section of BOE and all sub-elements (Labor Types and Labor Spread)
**			
**		
**
**		Auth: Don Canuso
**		Date: 8/2010
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**      1/17/17		twilson3			BOEJ-1604 Cleanup DB Project
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		4/2/18		ranzalon			BOEJ-3268 - Update for Open Ended Custom Fields
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**		12/13/19	twilson3			BOEJ-4434 - RTE Template Answers
**		10/29/20	Dusan				BOEJ-4924 - MOQ Type Selection data
**		1/4/2021	Dusan				BOEJ-4894: Added support for MoqTypeTableCustomFieldValueXREF
**		7/17/2024	e405721				PROPH-2164: Update for Skill Mix, Common Disclosure Skill Mix, MOQ Type Selection Table Data Resource Hours
**		9/30/25		e378233				PROPH-3302 Updated for Skill Mix Summary
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[BOETaskElement] WHERE BOETaskElementID = @BOETaskElementID) = @UpdateDT
		BEGIN

			-- Delete RTE Template Answers
			DELETE FROM dbo.[RteTemplateAnswer]
			WHERE TaskID = @BOETaskElementID

			DELETE FROM dbo.BOELaborSpread
				FROM dbo.BOELaborSpread LS
				INNER JOIN dbo.BOELaborType LT ON LS.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE
				TE.BOETaskElementID = @BOETaskElementID
				
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @LaborTypeCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @LaborTypeCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE
				TE.BOETaskElementID = @BOETaskElementID

			DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE
				TE.BOETaskElementID = @BOETaskElementID
				
			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @LaborTypeCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)
				
			DELETE FROM dbo.BOETaskElementWorkspaceVariableXREF
			WHERE
				BOETaskElementID = @BOETaskElementID
				
			DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF
			FROM  dbo.SumOfBOE_OrdinaryVariableXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN  dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOETaskElementID = @BOETaskElementID
			

			DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF
			FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN  dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOETaskElementID = @BOETaskElementID

				

			DELETE FROM dbo.OrdinaryVariable
			FROM dbo.OrdinaryVariable OV
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
			WHERE TE.BOETaskElementID = @BOETaskElementID
				
			--Get Custom Field Value IDs before the xrefs are deleted
			DECLARE @TaskCustomFieldXrefs TABLE (CustomFieldValueID int)

			INSERT INTO @TaskCustomFieldXrefs
			SELECT CustomFieldValueID
			FROM dbo.BOETaskElementCustomFieldValueXREF
			WHERE BOETaskElementID = @BOETaskElementID

			DELETE FROM dbo.BOETaskElementCustomFieldValueXREF
			FROM dbo.BOETaskElementCustomFieldValueXREF X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
			WHERE TE.BOETaskElementID = @BOETaskElementID

			--Delete Custom Field Values for deleted Open Ended Custom Fields
			DELETE FROM dbo.CustomFieldValue
			WHERE CustomFieldValueID in
			(
				SELECT x.CustomFieldValueID
				FROM @TaskCustomFieldXrefs x
				JOIN dbo.CustomFieldValue v on x.CustomFieldValueID = v.CustomFieldValueID
				JOIN dbo.CustomField c on v.CustomFieldId = c.CustomFieldID
				WHERE c.IsOpenEnded = 1
			)
			
			DELETE FROM [dbo].[BOETaskElementMetricDetailXREF]
				FROM [dbo].[BOETaskElementMetricDetailXREF] X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
			WHERE TE.BOETaskElementID = @BOETaskElementID	

			
			DELETE FROM dbo.BOELaborType
				FROM dbo.BOELaborType LT
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
			WHERE
				TE.BOETaskElementID = @BOETaskElementID
				
				
			DECLARE @WorkspaceID int
			SELECT @WorkspaceID = WorkspaceID 
			FROM dbo.BOE B
				INNER JOIN dbo.BOETaskElement TE ON B.BOEID = TE.BOEID
			WHERE TE.BOETaskElementID = @BOETaskElementID

			-- MOQ Type Selection data
			DELETE FROM dbo.[MoqTypeTableCustomFieldValueXREF]
					FROM dbo.[MoqTypeTableCustomFieldValueXREF] x
						INNER JOIN MoqTypeSelectionTableData t ON t.MoqTypeSelectionTableDataId = x.MoqTypeTableDataId 
						INNER JOIN MoqTypeSelection mS ON mS.MoqTypeSelectionId = t.MoqTypeSelectionId 
					WHERE mS.TaskId = @BOETaskElementID

			DELETE m
					FROM dbo.[MOQTypeSelectionTableDataResourceHours] m
			WHERE m.BOETaskElementID = @BOETaskElementID

			DELETE FROM dbo.MOQTypeSelectionTableData
				FROM dbo.MOQTypeSelectionTableData t
				INNER JOIN dbo.MOQTypeSelection s ON s.MOQTypeSelectionId = t.MOQTypeSelectionId
				INNER JOIN dbo.BOETaskElement TE ON s.TaskId = TE.BOETaskElementID
			WHERE
				TE.BOETaskElementID = @BOETaskElementID

			DELETE cd
				FROM dbo.CommonDisclosureSkillMix cd
			WHERE cd.BOETaskElementID = @BOETaskElementID

			DELETE sm
				FROM dbo.[SkillMix] sm
			WHERE sm.BOETaskElementID = @BOETaskElementID

			DELETE sms
				FROM dbo.SkillMixSummary sms
			WHERE sms.BOETaskElementID = @BOETaskElementID

			DELETE FROM dbo.MOQTypeSelection
				FROM dbo.MOQTypeSelection s
				INNER JOIN dbo.BOETaskElement TE ON s.TaskId = TE.BOETaskElementID
			WHERE
				TE.BOETaskElementID = @BOETaskElementID

			-- Task Element
			DELETE FROM dbo.BOETaskElement
			WHERE
				BOETaskElementID = @BOETaskElementID

		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Task Element with ID ' + CAST(@BOETaskElementID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
 
		END

GO

-- CREATE/ALTER createWorkspaceVersion
CREATE OR ALTER PROCEDURE [dbo].[createWorkspaceVersion]
(
@VersionName varchar(50),
@CreatedByETIUserID int,
@WorkspaceStateID int,
@WorkspaceID int
)
AS
/******************************************************************************
**		 
**		Name: [createWorkspaceVersion]
**		Desc:	Inserts a record in the Workspace Version Table
**				Then, copies every table from dbo to version schema
**				copying all of the Workspace data
**			
**		
**
**		Auth: Don Canuso
**		Date: 5/17/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		4/04/2018	brunworg			BOEJ-3177 Added TieredPercentage column.
**		4/13/18		pattoncr			BOEJ-3185 - DB Work (Sikorsky Legacy Resources)
**		5/2/18		ranzalon			BOEJ-3416 - Templates available to all Workspaces
**		6/18/18		ranzalon			BOEJ-3448 - ProPricer API updates
**		10/2/18		ranzalon			BOEJ-3699 - RTE Size Limit
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**		7/12/19		twilson3			BOEJ-4037	System Pro Pricer Export 
**		9/26/19		ranzalon			BOEJ-4349 - Revised Submittal Date
**		12/2/19		ranzalon			BOEJ-4464 - Added LaborSortId
**		12/5/19		twilson3			BOEJ-4429 - RTE Templates
**		12/13/19	twilson3			BOEJ-4434 - RTE Template Answers
**		12/17/19	twilson3			BOEJ-4434 Fix Assigned
**		8/27/20		ranzalon			BOEJ-4760 - Template Boe
**		9/15/20		ranzalon			BOEJ-4776/4825 - MOQ Types update
**		12/8/2020	ranzalon			BOEJ-4972 - remove CER location and BOELaborType MOQTypeSelectionId fields
**		1/4/2021	Dusan				BOEJ-4894: Added support for MoqTypeTableCustomFieldValueXREF
**		1/31/23		e405721				ACV-221 - Enable SAP Connection
**		1/18/24		ranzalon			PROPH-1070 Update for HistoricalReferenceExplanation
**		1/28/24		e302876  			PROPH-1492 ADD BRC to Copy BOEs, Copy WS, Archive/Restore
**		7/16/24		e405721				PROPH-2160: Update Create Workspace Version for Skill Mix, Common Disclosure Skill Mix and MOQ Type Selection Table Data Resource Hours
**		7/30/24		e405721				PROPH-2218 Add BRC Name into MOQ Type Selection Table Data Resource Hours Table
**		10/15/24	e405721				PROPH-2392: Update for Skill Mix V2
**		1/14/25		twilson3			PROPH-2596 - Add UCOT Factor
**		1/15/25		e309214				PROPH-1854 Database Changes for Assign Author
**		9/30/25		e378233				PROPH-3302 Updated for Skill Mix Summary
*******************************************************************************/
SET NOCOUNT ON 
--BEGIN TRANSACTION 

DECLARE @Inserted AS Table (ID int)
DECLARE @VersionID int,
@CreateDate datetime2 = GetDate()

INSERT INTO [dbo].[WorkspaceVersion]
([VersionName]
,[VersionCreated]
,[CreatedByETIUserID]
,[WorkspaceStateID]
,[WorkspaceID])
OUTPUT inserted.VersionID INTO @Inserted           
VALUES
(@VersionName
,@CreateDate
,@CreatedByETIUserID
,@WorkspaceStateID
,@WorkspaceID)

SELECT @VersionID = ID FROM @Inserted

INSERT INTO [version].[PerformingOrganizationList]
([PerformingOrganizationListID]
,[PerformingOrganizationListName]
,[UpdateDT]
,[VersionID])
SELECT PO.[PerformingOrganizationListID]
,PO.[PerformingOrganizationListName]
,PO.[UpdateDT]
,@VersionID
FROM [dbo].[PerformingOrganizationList] PO
INNER JOIN dbo.Workspace WS ON WS.PerformingOrganizationListID = PO.PerformingOrganizationListID
WHERE
WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[ResourceList]
([ResourceListID]
,[ResourceListName]
,[UpdateDT]
,[VersionID])
SELECT R.[ResourceListID]
,R.[ResourceListName]
,R.[UpdateDT]
,@VersionID
FROM [dbo].[ResourceList] R
INNER JOIN dbo.Workspace WS ON WS.ResourceListID = R.ResourceListID
WHERE
WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[Workspace]
([WorkspaceID]
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
--,[ResourceChangeFlag]
,[PerformingOrganizationChangeFlag]
,[TrackingNumber]
,[ContainsTemplate]
,[NumProPricerExport]
,[ProposalStatusID]
,[StatusComment]
,[UpdateDT]
,[VersionID]
/*Remove Code for 7681 ,[LaborPrecisionID]*/
/*WI9756*/
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
,[EnableSAPConnection]
,[UCOTFactor]
,[EnableAssignTaskAuthor])
SELECT [WorkspaceID]
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
,[UpdateDT]
,@VersionID
/*Remove Code for 7681 ,[LaborPrecisionID]*/
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
,[EnableSAPConnection]
,[UCOTFactor]
,[EnableAssignTaskAuthor]
FROM [dbo].[Workspace]
WHERE WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceContractTypeXREF]
	     ([WorkspaceContractTypeID]
	     ,[UpdateDT]
	     ,[WorkspaceID]
	     ,[ContractTypeID]
	     ,[VersionID])
SELECT WR.[WorkspaceContractTypeID]
	,WR.[UpdateDT]
	,WR.[WorkspaceID]
	,WR.[ContractTypeID]
	,@VersionID
FROM [dbo].[WorkspaceContractTypeXREF] WR
	INNER JOIN dbo.Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
WHERE 
	WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[OutputFormatTemplate]
([TemplateID]
,[Template]
,[TemplateDescription]
,[TemplateFile]
,[UpdateDT]
,[IsActive]
,[ParentTemplateID]
,[IsAvailableToAllWorkspaces]
)
SELECT OFT.[TemplateID]
,OFT.[Template]
,OFT.[TemplateDescription]
,OFT.[TemplateFile]
,OFT.[UpdateDT]
,OFT.[IsActive]
,OFT.[ParentTemplateID]
,OFT.[IsAvailableToAllWorkspaces]
FROM [dbo].[OutputFormatTemplate] OFT
INNER JOIN dbo.OutputFormatTemplateWorkspaceXREF OX ON OFT.TemplateID = OX.TemplateID
INNER JOIN dbo.Workspace WS ON OX.WorkspaceID = WS.WorkspaceID
WHERE 
WS.WorkspaceID = @WorkspaceID
AND NOT EXISTS
(
	SELECT 1
	FROM [version].[OutputFormatTemplate] VOFT
	WHERE VOFT.TemplateID = OFT.TemplateID AND VOFT.UpdateDT = OFT.UpdateDT
)

INSERT INTO [version].[OutputFormatTemplateVersionXREF]
([VersionID]
,[BackupTemplateID]
)
SELECT
@VersionID
,OVX.[BackupTemplateID]
FROM [dbo].[OutputFormatTemplate] OFT
INNER JOIN dbo.OutputFormatTemplateWorkspaceXREF OX ON OFT.TemplateID = OX.TemplateID
INNER JOIN dbo.Workspace WS ON OX.WorkspaceID = WS.WorkspaceID
INNER JOIN [version].[OutputFormatTemplate] OVX ON OFT.TemplateID = OVX.TemplateID AND OFT.UpdateDT = OVX.UpdateDT	
WHERE 
WS.WorkspaceID = @WorkspaceID 

INSERT INTO [version].[BOE]
([BOEID]
,[BOEStateID]
,[BOEStartDate]
,[BOEEndDate]
,[BOEDescription]
,[DataSource]
,[WorkspaceID]
,[MetricDisclosureAcknowledge]
,[NumAuthorReassigned]
,[IsMaterial]
,[UpdateDT]
,[VersionID]
,[BOETitle]
,[IsMultiClinWbs]
)
SELECT B.[BOEID]
,B.[BOEStateID]
,B.[BOEStartDate]
,B.[BOEEndDate]
,B.[BOEDescription]
,B.[DataSource]
,B.[WorkspaceID]
,B.[MetricDisclosureAcknowledge]
,B.[NumAuthorReassigned]
,B.[IsMaterial]
,B.[UpdateDT]
,@VersionID
,B.BOETitle
,B.IsMultiClinWbs
FROM [dbo].[BOE] B
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[WorkspaceOffloadRate]
	([OffloadRateID]
	,[VersionID]
	,[UpdateDT]
	,[WorkspaceID]
	,[Resource]
	,[PerfOrg]
	,[PercentToOffload]
	,[Year]
	,[SubcontractorResource]
	,[HourlyRate])
SELECT [OffloadRateID]
	,@VersionID
	,[UpdateDT]
	,[WorkspaceID]
	,[Resource]
	,[PerfOrg]
	,[PercentToOffload]
	,[Year]
	,[SubcontractorResource]
	,[HourlyRate]
FROM [dbo].[WorkspaceOffloadRate]
WHERE WorkspaceID = @WorkspaceID

INSERT INTO [version].[ProjectMap]
	([VersionID]
	,[ID]
	,[WorkspaceId]
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
	@VersionID
	,[ID]
	,[WorkspaceId]
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
FROM [dbo].[ProjectMap]
WHERE WorkspaceID = @WorkspaceID

INSERT INTO [version].[ProjectMapSpread]
	     ([VersionID]
	     ,[ID]
		   ,[WorkspaceId]
		   ,[ProjectMapId]
	     ,[SpreadDate]
	     ,[SpreadValue]
	     )
SELECT 	    @VersionID,
			[ID],
			[WorkspaceId],
			[ProjectMapId],
			[SpreadDate],
			[SpreadValue]
FROM  [dbo].ProjectMapSpread S
WHERE [WorkspaceId] = @WorkspaceID

/** RTE Templates **/

INSERT INTO [version].[RteTemplate]
			([TemplateID],
			[UpdateDT],
			[WorkspaceID],
			[Description],
			[AuthorID],
			[CreatedOn],
			[VersionID])
			SELECT [TemplateID],
		[UpdateDT]
	,[WorkspaceID]
	,[Description]
	,[AuthorID]
	,[CreatedOn]
	  ,@VersionID
  FROM [dbo].[RteTemplate]
WHERE WorkspaceID = @WorkspaceID

INSERT INTO [version].[RteTemplateAssigned]
	([TemplateID],
	[RteTemplateSourceId],
	VersionID)
	SELECT RA.[TemplateID],
		RA.[RteTemplateSourceId],
		@VersionID
	FROM [dbo].[RteTemplateAssigned] RA
	INNER JOIN [dbo].[RteTemplate] R on R.[TemplateID] = RA.[TemplateID]
WHERE R.WorkspaceID = @WorkspaceID

INSERT INTO [version].[RteTemplateQuestion]
	     ([QuestionID],
		    [UpdateDT],
			[TemplateID],
			[Text],
			[SortOrder],
			[Required],
			[VersionID])
SELECT RTQ.[QuestionID]
	  ,RTQ.[UpdateDT]
	,RTQ.[TemplateID]
	,RTQ.[Text]
	,RTQ.[SortOrder]
	,RTQ.[Required]
	  ,@VersionID
  FROM [dbo].[RteTemplateQuestion] RTQ
  INNER JOIN dbo.[RteTemplate] RT ON RT.TemplateID = RTQ.TemplateID
  WHERE RT.WorkspaceID = @WorkspaceID

INSERT INTO [version].[RteTemplateAnswer]
	([AnswerID],
	[UpdateDT],
	[QuestionID],
	[BOEID],
	[TaskID],
	[Text],
	[RteTemplateSourceId],
	[VersionID])
SELECT RTA.[AnswerID],
		RTA.[UpdateDT],
		RTA.[QuestionID],
		RTA.[BOEID],
		RTA.[TaskID],
		RTA.[Text],
		RTA.[RteTemplateSourceId],
		@VersionID
	FROM [dbo].[RteTemplateAnswer] RTA
	INNER JOIN [dbo].[RteTemplateQuestion] RTQ ON RTA.[QuestionID] = RTQ.[QuestionID]
	INNER JOIN dbo.[RteTemplate] RT ON RT.TemplateID = RTQ.TemplateID
	WHERE RT.WorkspaceID = @WorkspaceID

/** Custom Fields **/

INSERT INTO [version].[CustomField]
([CustomFieldID]
,[CustomFieldName]
,[CustomFieldRequired]
,[CustomFieldDisplayID]
,[WorkspaceID]
,[UpdateDT]
,[IsOpenEnded]
,[VersionID])
SELECT CF.[CustomFieldID]
,CF.[CustomFieldName]
,CF.[CustomFieldRequired]
,CF.[CustomFieldDisplayID]
,CF.[WorkspaceID]
,CF.[UpdateDT]
,CF.[IsOpenEnded]
,@VersionID
FROM [dbo].[CustomField] CF
INNER JOIN dbo.Workspace WS ON CF.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[CLIN]
(
 [CLINID]
,[CLINNumber]
,[CLINTitle]
,[CLINStartDate]
,[CLINEndDate]
,[ContractTypeID]
,[WorkspaceID]
,[UpdateDT]
,[VersionID]
,[DisplayedCLINNumber]
)
SELECT 
 C.[CLINID]
,C.[CLINNumber]
,C.[CLINTitle]
,C.[CLINStartDate]
,C.[CLINEndDate]
,C.[ContractTypeID]
,C.[WorkspaceID]
,C.[UpdateDT]
,@VersionID
,C.[DisplayedCLINNumber]
FROM [dbo].[CLIN] C
INNER JOIN dbo.Workspace WS ON C.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[BOEPotentialRole]
([BOEPotentialRoleID]
,[ETIUserID]
/*,[ETIGroupID]*/
,[WorkspaceID]
,[RoleID]
,[UserRemoved]
,[UpdateDT]
,[VersionID])
SELECT BR.[BOEPotentialRoleID]
,BR.[ETIUserID]
/*,BR.[ETIGroupID]*/
,BR.[WorkspaceID]
,BR.[RoleID]
,BR.[UserRemoved]
,BR.[UpdateDT]
,@VersionID
FROM [dbo].[BOEPotentialRole] BR
INNER JOIN dbo.Workspace WS ON BR.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[WorkBreakdownStructure]
	     ([WBSID]
	     ,[UpdateDT]
	     ,[WBSNumber]
	     ,[DisplayedWBSNumber]
	     ,[WBSTitle]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT 
 WBS.[WBSID]
,WBS.[UpdateDT] 
,WBS.[WBSNumber]
,WBS.[DisplayedWBSNumber]
,WBS.[WBSTitle]
,WBS.[WorkspaceID]
,@VersionID
FROM [dbo].[WorkBreakdownStructure] WBS
	INNER JOIN dbo.Workspace WS ON WBS.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[TMResourceRate]
	     ([TMResourceRateID]
	     ,[UpdateDT]
	     ,[WorkspaceID]
	     ,[TMResourceID]
	     ,[TMResourceRateStartDate]
	     ,[TMResourceRateEndDate]
	     ,[TMResourceRate]
	     ,[VersionID])
SELECT TMRR.[TMResourceRateID]
	,TMRR.[UpdateDT]
	,TMRR.[WorkspaceID]
	,TMRR.[TMResourceID]
	,TMRR.[TMResourceRateStartDate]
	,TMRR.[TMResourceRateEndDate]
	,TMRR.[TMResourceRate]
	,@VersionID
FROM [dbo].[TMResourceRate] TMRR
	INNER JOIN dbo.Workspace WS ON TMRR.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID	

INSERT INTO [version].[WorkspaceStateHistory]
([WorkspaceStateHistoryID]
,[WorkspaceID]
,[CurrentWorkspaceStateID]
,[UpdatedWorkspaceStateID]
,[ChangedByETIUserID]
,[UpdateDT]
,[VersionID])
SELECT WSH.[WorkspaceStateHistoryID]
,WSH.[WorkspaceID]
,WSH.[CurrentWorkspaceStateID]
,WSH.[UpdatedWorkspaceStateID]
,WSH.[ChangedByETIUserID]
,WSH.[UpdateDT]
,@VersionID
FROM [dbo].[WorkspaceStateHistory] WSH
INNER JOIN dbo.Workspace WS ON WSH.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceVariable]
([WorkspaceVariableID]
,[WorkspaceVariableName]
,[WorkspaceVariableValue]
,[WorkspaceID]
,[SortByID]
,[ValueTypeID]
,[IsPercentage]
,[UpdateDT]
,[VersionID])
SELECT WSV.[WorkspaceVariableID]
,WSV.[WorkspaceVariableName]
,WSV.[WorkspaceVariableValue]
,WSV.[WorkspaceID]
,WSV.[SortByID]
,WSV.[ValueTypeID]
,WSV.[IsPercentage]
,WSV.[UpdateDT]
,@VersionID
FROM [dbo].[WorkspaceVariable] WSV
INNER JOIN dbo.Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceVariableSumVariableResourceTypeXREF]
([WVSVRTID]
,[WorkspaceVariableID]
,[SumVariableResourceTypeID]
,[VersionID])
SELECT X.[WVSVRTID]
,X.[WorkspaceVariableID]
,X.[SumVariableResourceTypeID]
,@VersionID
FROM [dbo].[WorkspaceVariableSumVariableResourceTypeXREF] X
INNER JOIN [dbo].[WorkspaceVariable] WSV ON X.WorkspaceVariableID = WSV.WorkspaceVariableID
INNER JOIN dbo.Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceUserRole]
([WorkspaceUserRoleID]
,[ETIUserID]
/*,[ETIGroupID]*/
,[RoleID]
,[WorkspaceID]
,[HideHelp]
,[UpdateDT]
,[VersionID])
SELECT WUR.[WorkspaceUserRoleID]
,WUR.[ETIUserID]
/*,WUR.[ETIGroupID]*/
,WUR.[RoleID]
,WUR.[WorkspaceID]
,WUR.[HideHelp]
,WUR.[UpdateDT]
,@VersionID
FROM [dbo].[WorkspaceUserRole] WUR
INNER JOIN dbo.Workspace WS ON WUR.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[ProPricerExport]
([ProPricerExportID]
,[ProPricerExportName]
,[WorkspaceID]
,[UpdateDT]
,[VersionID])
SELECT PPE.[ProPricerExportID]
,PPE.[ProPricerExportName]
,PPE.[WorkspaceID]
,PPE.[UpdateDT]
,@VersionID
FROM [dbo].[ProPricerExport] PPE
INNER JOIN dbo.Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[Resource]
	     ([ResourceID]
	     ,[ResourceName]
	     ,[ResourceDescription]
	     ,[SegmentRegion]
	     ,[LaborType]
	     ,[SegmentID]
	     ,[ResourceListID]
	     /*,[ResourceInUseFlag]*/
	     ,[CostElementID]
	     ,[UpdateDT]
		   ,[VersionID]
		   ,[DeletedFlag]
		   ,[RateTypeID]
		   )
SELECT R.[ResourceID]
	,R.[ResourceName]
	,R.[ResourceDescription]
	,R.[SegmentRegion]
	,R.[LaborType]
	,R.[SegmentID]
	,R.[ResourceListID]
	/*,R.[ResourceInUseFlag]*/
	,R.[CostElementID]
	,R.[UpdateDT]
	,@VersionID
	,R.[DeletedFlag]
	,R.RateTypeID
FROM [dbo].[Resource] R 
--INNER JOIN [dbo].[BOELaborType] BLT ON R.ResourceID = BLT.ResourceID
--INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
--INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON R.ResourceListID = WS.ResourceListID
--B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceResource]
	     ([WorkspaceResourceID]
	     ,[SystemResourceID]
	     ,[ResourceListID]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT WR.[WorkspaceResourceID]
	,WR.[SystemResourceID]
	,WR.[ResourceListID]
	,WR.[WorkspaceID]
	,@VersionID
FROM [dbo].[WorkspaceResource] WR
INNER JOIN dbo.Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

/*
INSERT INTO [version].[PerformingOrganization]
([PerformingOrganizationID]
,[PerformingOrganizationName]
,[PerformingOrganizationDescription]
,[PerformingOrganizationListID]
,[PerformingOrganizationInUseFlag]
,[UpdateDT]
,[VersionID])
SELECT PO.[PerformingOrganizationID]
	,PO.[PerformingOrganizationName]
	,PO.[PerformingOrganizationDescription]
	,PO.[PerformingOrganizationListID]
	,PO.[PerformingOrganizationInUseFlag]
	,PO.[UpdateDT]
	,@VersionID
FROM [dbo].[PerformingOrganization] PO
INNER JOIN [dbo].[BOELaborType] BLT ON PO.PerformingOrganizationID = BLT.PerformingOrganizationID
INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID
*/

INSERT INTO [version].[PerformingOrganization]
	     ([PerformingOrganizationID]
	     ,[PerformingOrganizationName]
	     ,[PerformingOrganizationDescription]
	     ,[PerformingOrganizationListID]
	     /*,[PerformingOrganizationInUseFlag]*/
	     ,[UpdateDT]
		   ,[VersionID]
		   ,[DeletedFlag])
SELECT R.[PerformingOrganizationID]
	,R.[PerformingOrganizationName]
	,R.[PerformingOrganizationDescription]
	,R.[PerformingOrganizationListID]
	/*,R.[PerformingOrganizationInUseFlag]*/
	,R.[UpdateDT]
	,@VersionID
	,R.[DeletedFlag]
FROM [dbo].[PerformingOrganization] R 
--INNER JOIN [dbo].[BOELaborType] BLT ON R.PerformingOrganizationID = BLT.PerformingOrganizationID
--INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
--INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON R.PerformingOrganizationListID = WS.PerformingOrganizationListID
--B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspacePerformingOrganization]
	     ([WorkspacePerformingOrganizationID]
	     ,[SystemPerformingOrganizationID]
	     ,[PerformingOrganizationListID]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT WR.[WorkspacePerformingOrganizationID]
	,WR.[SystemPerformingOrganizationID]
	,WR.[PerformingOrganizationListID]
	,WR.[WorkspaceID]
	,@VersionID
FROM [dbo].[WorkspacePerformingOrganization] WR
INNER JOIN dbo.Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[OutputFormatTemplateWorkspaceXREF]
([OutputFormatID]
,[WorkspaceID]
,[TemplateID]
,[VersionID])
SELECT OX.[OutputFormatID]
,OX.[WorkspaceID]
,OX.[TemplateID]
,@VersionID
FROM [dbo].[OutputFormatTemplateWorkspaceXREF] OX
INNER JOIN dbo.Workspace WS ON OX.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[ProPricerFieldXREF]
([PFID]
,[ProPricerExportID]
,[ProPricerFieldID]
,[ProPricerTypeID]
,[ListOrder]
,[VersionID])
SELECT PX.[PFID]
,PX.[ProPricerExportID]
,PX.[ProPricerFieldID]
,PX.[ProPricerTypeID]
,PX.[ListOrder]
,@VersionID
FROM [dbo].[ProPricerFieldXREF] PX
INNER JOIN [dbo].[ProPricerExport] PPE ON PX.ProPricerExportID = PPE.ProPricerExportID
INNER JOIN dbo.Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[ProPricerCustomFieldXREF]
([PCID]
,[ProPricerExportID]
,[CustomFieldID]
,[ProPricerTypeID]
,[ProPricerCustomFieldSelectionID]
,[ListOrder]
,[VersionID])
SELECT PX.[PCID]
,PX.[ProPricerExportID]
,PX.[CustomFieldID]
,PX.[ProPricerTypeID]
,PX.[ProPricerCustomFieldSelectionID]
,PX.[ListOrder]
,@VersionID
FROM [dbo].[ProPricerCustomFieldXREF] PX
INNER JOIN [dbo].[ProPricerExport] PPE ON PX.ProPricerExportID = PPE.ProPricerExportID
INNER JOIN dbo.Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WBS_CLIN_BOE_XREF]
([WCBID]
,[WBSID]
,[CLINID]
,[BOEID]
,[VersionID])
SELECT X.[WCBID], X.[WBSID], X.[CLINID], X.[BOEID], @VersionID
FROM [dbo].[WBS_CLIN_BOE_XREF] X INNER JOIN dbo.WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID AND WBS.WorkspaceID = @WorkspaceID
UNION
SELECT X.[WCBID], X.[WBSID], X.[CLINID], X.[BOEID], @VersionID
FROM [dbo].[WBS_CLIN_BOE_XREF] X INNER JOIN dbo.CLIN C ON X.CLINID = C.CLINID AND C.WorkspaceID = @WorkspaceID
UNION
SELECT X.[WCBID], X.[WBSID], X.[CLINID], X.[BOEID], @VersionID
FROM [dbo].[WBS_CLIN_BOE_XREF] X INNER JOIN dbo.BOE B ON X.BOEID = B.BOEID AND B.WorkspaceID = @WorkspaceID

INSERT INTO [version].[SumOfBOE_WorkspaceVariableXREF]
([WVSumID]
,[WorkspaceVariableID]
,[CLINID]
,[WBSID]
,[BOEID]
,[VersionID])
SELECT X.[WVSumID]
,X.[WorkspaceVariableID]
,X.[CLINID]
,X.[WBSID]
,X.[BOEID]
,@VersionID
FROM [dbo].[SumOfBOE_WorkspaceVariableXREF] X
INNER JOIN [dbo].[WorkspaceVariable] WSV ON X.WorkspaceVariableID = WSV.WorkspaceVariableID
INNER JOIN dbo.Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEStateHistory]
	     ([BOEStateHistoryID]
	     ,[BOEID]
	     ,[FieldID]
	     ,[CurrentBOEStateID]
	     ,[UpdatedBOEStateID]
	     ,[ChangedByETIUserID]
	     ,[UpdateDT]
	     ,[VersionID])
SELECT BH.[BOEStateHistoryID]
	,BH.[BOEID]
	,BH.[FieldID]
	,BH.[CurrentBOEStateID]
	,BH.[UpdatedBOEStateID]
	,BH.[ChangedByETIUserID]
	,BH.[UpdateDT]
	,@VersionID
  FROM [dbo].[BOEStateHistory] BH
INNER JOIN dbo.BOE B ON BH.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOECommentHistory]
([BOECommentHistoryID]
,[BOECommentID]
,[BOEID]
,[FieldID]
,[CurrentComment]
,[UpdatedComment]
,[ChangedByETIUserID]
,[UpdateDT]
,[VersionID])
SELECT BH.[BOECommentHistoryID]
,BH.[BOECommentID]
,BH.[BOEID]
,BH.[FieldID]
,BH.[CurrentComment]
,BH.[UpdatedComment]
,BH.[ChangedByETIUserID]
,BH.[UpdateDT]
,@VersionID
FROM [dbo].[BOECommentHistory] BH
INNER JOIN [dbo].[BOEComment] BC ON BH.BOECommentID = BC.BOECommentID
INNER JOIN dbo.BOE B ON BH.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEUserRole]
([BOEUserRoleID]
,[ETIUserID]
,[RoleID]
,[BOEID]
,[UpdateDT]
,[VersionID])
SELECT BUR.[BOEUserRoleID]
,BUR.[ETIUserID]
,BUR.[RoleID]
,BUR.[BOEID]
,BUR.[UpdateDT]
,@VersionID
FROM [dbo].[BOEUserRole] BUR
INNER JOIN dbo.BOE B ON BUR.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

/*WI 5160*/
INSERT INTO [version].[BOEUserRoleHistory]
	     (
		[BOEUserRoleHistoryID]
	     ,[UpdateDT]
	     ,[CurrentETIUserID]
	     ,[UpdatedETIUserID]
	     ,[RoleID]
	     ,[BOEID]
	     ,[FieldID]
	     ,[ChangedByETIUserID]
	     ,[VersionID]
	     )
SELECT BUR.[BOEUserRoleHistoryID]
	,BUR.[UpdateDT]
	,BUR.[CurrentETIUserID]
	,BUR.[UpdatedETIUserID]
	,BUR.[RoleID]
	,BUR.[BOEID]
	,BUR.[FieldID]
	,BUR.[ChangedByETIUserID]
	,@VersionID
FROM [dbo].[BOEUserRoleHistory] BUR
INNER JOIN dbo.BOE B ON BUR.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEApproval]
([BOEApprovalID]
,[BOEID]
,[ApprovalETIUserID]
,[ApprovedFlag]
,[UpdateDT]
,[VersionID])
SELECT BA.[BOEApprovalID]
,BA.[BOEID]
,BA.[ApprovalETIUserID]
,BA.[ApprovedFlag]
,BA.[UpdateDT]
,@VersionID
FROM [dbo].[BOEApproval] BA
INNER JOIN dbo.BOE B ON BA.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEComment]
([BOECommentID]
,[FieldID]
,[BOEComments]
,[BOECommentETIUserID]
,[BOEResponseToCommentID]
,[BOEID]
,[UpdateDT]
,[VersionID])
SELECT BC.[BOECommentID]
,BC.[FieldID]
,BC.[BOEComments]
,BC.[BOECommentETIUserID]
,BC.[BOEResponseToCommentID]
,BC.[BOEID]
,BC.[UpdateDT]
,@VersionID
FROM [dbo].[BOEComment] BC
INNER JOIN dbo.BOE B ON BC.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[CustomFieldValue]
([CustomFieldValueID]
,[CustomFieldValueName]
,[CustomFieldValueDescription]
,[CustomFieldID]
,[CustomFieldValueInUseFlag]
,[UpdateDT]
,[VersionID])
SELECT CFV.[CustomFieldValueID]
,CFV.[CustomFieldValueName]
,CFV.[CustomFieldValueDescription]
,CFV.[CustomFieldID]
,CFV.[CustomFieldValueInUseFlag]
,CFV.[UpdateDT]
,@VersionID
FROM [dbo].[CustomFieldValue] CFV
INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
INNER JOIN dbo.Workspace WS ON CF.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOETaskElement]
([BOETaskElementID]
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
,[UpdateDT]
,[VersionID]
,[SortOrderID]
,[AuthorUserId]
)
SELECT BTE.[BOETaskElementID]
,BTE.[TaskID]
,BTE.[TaskTitle]
,BTE.[TaskDescription]
,BTE.[TaskStartDate]
,BTE.[TaskEndDate]
,BTE.[MOQHoursEquation]
,BTE.[MOQCostEquation]
,BTE.[MOQText]
,BTE.[MOQTypeID]
,BTE.[BOEID]
,BTE.[LaborTypeWarningFlag]
,BTE.[IMS_ID]
,BTE.[TaskElementTypeID]
,BTE.[UpdateDT]
,@VersionID
,BTE.[SortOrderID]
,BTE.[AuthorUserId]
FROM [dbo].[BOETaskElement] BTE
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[OrdinaryVariable]
([OrdinaryVariableID]
,[OrdinaryVariableName]
,[OrdinaryVariableValue]
,[BOETaskElementID]
,[SortByID]
,[ValueTypeID]
,[IsPercentage]
,[UpdateDT]
,[VersionID]
,[DefaultSize]
)
SELECT OV.[OrdinaryVariableID]
,OV.[OrdinaryVariableName]
,OV.[OrdinaryVariableValue]
,OV.[BOETaskElementID]
,OV.[SortByID]
,OV.[ValueTypeID]
,OV.[IsPercentage]
,OV.[UpdateDT]
,@VersionID
,OV.[DefaultSize]
FROM [dbo].[OrdinaryVariable] OV
INNER JOIN [dbo].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[OrdinaryVariableSumVariableResourceTypeXREF]
([OVSVRTID]
,[OrdinaryVariableID]
,[SumVariableResourceTypeID]
,[VersionID])
SELECT X.[OVSVRTID]
,X.[OrdinaryVariableID]
,X.[SumVariableResourceTypeID]
,@VersionID
FROM [dbo].[OrdinaryVariableSumVariableResourceTypeXREF] X
INNER JOIN [dbo].[OrdinaryVariable] OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
INNER JOIN [dbo].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEApprovalHistory]
([BOEApprovalHistoryID]
,[BOEID]
,[Approval]
,[ApprovalETIUserID]
,[UpdateDT]
,[VersionID])
SELECT BH.[BOEApprovalHistoryID]
,BH.[BOEID]
,BH.[Approval]
,BH.[ApprovalETIUserID]
,BH.[UpdateDT]
,@VersionID
FROM [dbo].[BOEApprovalHistory] BH
INNER JOIN dbo.BOE B ON BH.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

/** [dbo].[MOQTypeSelection] **/
INSERT INTO [version].[MOQTypeSelection]
([MOQTypeSelectionId],
[TaskId],
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
[SkillMix],
[HistoricalReferenceExplanation],
[VersionId]
)
SELECT M.[MOQTypeSelectionId],
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
M.[HistoricalReferenceExplanation],
@VersionID
FROM [dbo].[MOQTypeSelection] M
INNER JOIN [dbo].[BOETaskElement] T ON M.TaskId = T.BOETaskElementID
INNER JOIN dbo.BOE B ON T.BOEID  = B.BOEID
INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
WHERE W.WorkspaceID = @WorkspaceID

/** [dbo].[MOQTypeSelectionTableData] **/
INSERT INTO [version].[MOQTypeSelectionTableData]
([MOQTypeSelectionTableDataId],
[MOQTypeSelectionId],
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
[TotalRelevantHoursAfterQueryFilters],
[VersionId]
)
SELECT TD.[MOQTypeSelectionTableDataId],
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
@VersionID
FROM [dbo].[MOQTypeSelectionTableData] TD
INNER JOIN [dbo].[MOQTypeSelection] M ON TD.MOQTypeSelectionId = M.MOQTypeSelectionId
INNER JOIN [dbo].[BOETaskElement] T ON M.TaskId = T.BOETaskElementID
INNER JOIN dbo.BOE B ON T.BOEID  = B.BOEID
INNER JOIN dbo.Workspace W ON B.WorkspaceID = W.WorkspaceID
WHERE W.WorkspaceID = @WorkspaceID

/** [dbo].[SkillMix] **/
INSERT INTO [version].[SkillMix]
([SkillMixID],
[Rationale],
[Included],
[ProposedHours],
[HistoricalHours],
[BOESkillMix],
[LaborSkillMix],
[ResourceOld],
[ResourceNew],
[BOEID],
[BOETaskElementID],
[IsUserInput],
[VersionId]
)
SELECT SM.[SkillMixID],
SM.[Rationale],
SM.[Included],
SM.[ProposedHours],
SM.[HistoricalHours],
SM.[BOESkillMix],
SM.[LaborSkillMix],
SM.[ResourceOld],
SM.[ResourceNew],
SM.[BOEID],
SM.[BOETaskElementID],
SM.[IsUserInput],
@VersionID
FROM [dbo].[SkillMix] SM
INNER JOIN dbo.BOE B ON SM.BOEID = B.BOEID
INNER JOIN dbo.BOETaskElement T on SM.[BOETaskElementID] = T.[BOETaskElementID]
WHERE B.WorkspaceID = @WorkspaceID

/** [dbo].[CommonDisclosureSkillMix] **/
INSERT INTO [version].[CommonDisclosureSkillMix]
([CommonDisclosureSkillMixID],
[Rationale],
[Included],
[ProposedHours],
[HistoricalHours],
[BOESkillMix],
[LaborSkillMix],
[ResourceID],
[BusinessResourceID],
[BOETaskElementID],
[IsUserInput],
[BOEID],
[VersionId]
)
SELECT CD.[CommonDisclosureSkillMixID],
CD.[Rationale],
CD.[Included],
CD.[ProposedHours],
CD.[HistoricalHours],
CD.[BOESkillMix],
CD.[LaborSkillMix],
CD.[ResourceID],
CD.[BusinessResourceID],
CD.[BOETaskElementID],
CD.[IsUserInput],
CD.[BOEID],
@VersionID
FROM [dbo].[CommonDisclosureSkillMix] CD
INNER JOIN dbo.BOE B ON CD.BOEID = B.BOEID
INNER JOIN dbo.BOETaskElement T on CD.[BOETaskElementID] = T.[BOETaskElementID]
WHERE B.WorkspaceID = @WorkspaceID

/** [dbo].[SkillMixSummary] **/
INSERT INTO [version].[SkillMixSummary]
([SkillMixSummaryID],
[Rationale],
[Included],
[ProposedHours],
[HistoricalHours],
[ResourceHours],
[BusinessResourceHours],
[BOESkillMix],
[LaborSkillMix],
[ResourceID],
[BusinessResourceID],
[BOETaskElementID],
[IsUserInput],
[BOEID],
[VersionId]
)
SELECT SMS.[SkillMixSummaryID],
SMS.[Rationale],
SMS.[Included],
SMS.[ProposedHours],
SMS.[HistoricalHours],
SMS.[ResourceHours],
SMS.[BusinessResourceHours],
SMS.[BOESkillMix],
SMS.[LaborSkillMix],
SMS.[ResourceID],
SMS.[BusinessResourceID],
SMS.[BOETaskElementID],
SMS.[IsUserInput],
SMS.[BOEID],
@VersionID
FROM [dbo].[SkillMixSummary] SMS
INNER JOIN dbo.BOE B ON SMS.BOEID = B.BOEID
INNER JOIN dbo.BOETaskElement T on SMS.[BOETaskElementID] = T.[BOETaskElementID]
WHERE B.WorkspaceID = @WorkspaceID

/** [dbo].[MOQTypeSelectionTableDataResourceHours] **/
INSERT INTO [version].[MOQTypeSelectionTableDataResourceHours]
([MOQTypeSelectionTableDataResourceHoursId],
[ResourceName],
[BRCName],
[WbsHours],
[TotalHours],
[MOQTypeSelectionTableDataId],
[BOETaskElementID],
[BOEID],
[VersionId]
)
SELECT M.[MOQTypeSelectionTableDataResourceHoursId],
M.[ResourceName],
M.[BRCName],
M.[WbsHours],
M.[TotalHours],
M.[MOQTypeSelectionTableDataId],
M.[BOETaskElementID],
M.[BOEID],
@VersionID
FROM [dbo].[MOQTypeSelectionTableDataResourceHours] M
INNER JOIN dbo.BOE B ON M.BOEID = B.BOEID
INNER JOIN dbo.[MOQTypeSelectionTableData] MOQ ON M.MOQTypeSelectionTableDataId = MOQ.MOQTypeSelectionTableDataId
WHERE B.WorkspaceID = @WorkspaceID

/*Updated for WI 8398*/
INSERT INTO [version].[BOELaborType]
([BOELaborTypeID]
,[ResourceID]
,[PerformingOrganizationID]
,[BOELaborTypeStartDate]
,[BOELaborTypeEndDate]
,[SpreadCurveID]
,[PercentSpread]
,[ValueSpread]
,[BOETaskElementID]
,[SpreadTypeID]
,[UpdateDT]
,[VersionID]
,[PercentSpreadLocked]
,[HourSpreadLocked]
,[WBSID]
,[CLINID]
,[CanOffload]
,[LaborSortId]
,[BRCResourceID]
)
SELECT BLT.[BOELaborTypeID]
,BLT.[ResourceID]
,BLT.[PerformingOrganizationID]
,BLT.[BOELaborTypeStartDate]
,BLT.[BOELaborTypeEndDate]
,BLT.[SpreadCurveID]
,BLT.[PercentSpread]
,BLT.[ValueSpread]
,BLT.[BOETaskElementID]
,BLT.[SpreadTypeID]
,BLT.[UpdateDT]
,@VersionID
,BLT.PercentSpreadLocked
,BLT.HourSpreadLocked
,BLT.WBSID
,BLT.CLINID
,BLT.[CanOffload]
,BLT.[LaborSortId]
,BLT.[BRCResourceID]
FROM [dbo].[BOELaborType] BLT
INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOETaskElementWorkspaceVariableXREF]
([BOETaskWSVarID]
,[BOETaskElementID]
,[WorkspaceVariableID]
,[VersionID])
SELECT X.[BOETaskWSVarID]
,X.[BOETaskElementID]
,X.[WorkspaceVariableID]
,@VersionID
FROM [dbo].[BOETaskElementWorkspaceVariableXREF] X
INNER JOIN [dbo].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOETaskElementMetricDetailXREF]
([BTEMDID]
,[BOETaskElementID]
,[MetricDetailID]
,[UpdateDT]
,[VersionID])
SELECT X.[BTEMDID]
,X.[BOETaskElementID]
,X.[MetricDetailID]
,X.[UpdateDT]
,@VersionID
FROM [dbo].[BOETaskElementMetricDetailXREF] X
INNER JOIN [dbo].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOETaskElementCustomFieldValueXREF]
([BTECFVID]
,[BOETaskElementID]
,[CustomFieldValueID]
,[UpdateDT]
,[VersionID])
SELECT X.[BTECFVID]
,X.[BOETaskElementID]
,X.[CustomFieldValueID]
,X.[UpdateDT]
,@VersionID
FROM [dbo].[BOETaskElementCustomFieldValueXREF] X
INNER JOIN [dbo].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOECustomFieldValueXREF]
([BCFVID]
,[BOEID]
,[CustomFieldValueID]
,[UpdateDT]
,[VersionID])
SELECT X.[BCFVID]
,X.[BOEID]
,X.[CustomFieldValueID]
,X.[UpdateDT]
,@VersionID
FROM [dbo].[BOECustomFieldValueXREF] X
INNER JOIN dbo.BOE B ON X.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[MoqTypeTableCustomFieldValueXREF] ([Id], [UpdateDT], [MoqTypeTableDataId], [CustomFieldValueId], [VersionID])
    SELECT x.[Id], x.[UpdateDT], x.[MoqTypeTableDataId], x.[CustomFieldValueId], @VersionID
	  FROM [dbo].[MoqTypeTableCustomFieldValueXREF] x, MoqTypeSelectionTableData t, MoqTypeSelection mS, BoeTaskElement tE, dbo.BOE B
	  WHERE
		t.MoqTypeSelectionTableDataId = x.MoqTypeTableDataId AND mS.MoqTypeSelectionId = t.MoqTypeSelectionId
		AND tE.BoeTaskElementId = mS.TaskId AND tE.BOEID = B.BOEID AND B.WorkspaceID = @WorkspaceID

INSERT INTO [version].[SumOfBOE_OrdinaryVariableXREF]
([OVSumID]
,[OrdinaryVariableID]
,[CLINID]
,[WBSID]
,[BOEID]
,[VersionID])
SELECT X.[OVSumID]
,X.[OrdinaryVariableID]
,X.[CLINID]
,X.[WBSID]
,X.[BOEID]
,@VersionID
FROM [dbo].[SumOfBOE_OrdinaryVariableXREF] X
INNER JOIN [dbo].[OrdinaryVariable] OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
INNER JOIN [dbo].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOELaborTypeCustomFieldValueXREF]
([BLTCFVID]
,[BOELaborTypeID]
,[CustomFieldValueID]
,[UpdateDT]
,[VersionID])
SELECT X.[BLTCFVID]
,X.[BOELaborTypeID]
,X.[CustomFieldValueID]
,X.[UpdateDT]
,@VersionID
FROM [dbo].[BOELaborTypeCustomFieldValueXREF] X 
INNER JOIN [dbo].[BOELaborType] BLT ON X.BOELaborTypeID = BLT.BOELaborTypeID
INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOELaborSpread]
([BOELaborSpreadID]
,[BOELaborTypeID]
,[LaborSpreadDate]
,[LaborSpreadValue]
,[VersionID])
SELECT LS.[BOELaborSpreadID]
,LS.[BOELaborTypeID]
,LS.[LaborSpreadDate]
,LS.[LaborSpreadValue]
,@VersionID
FROM [dbo].[BOELaborSpread] LS
INNER JOIN [dbo].[BOELaborType] BLT ON LS.BOELaborTypeID = BLT.BOELaborTypeID
INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[ODCTaskElement]
	     ([ODCTaskElementID]
	     ,[ODCTaskTitle]
	     ,[ODCTaskDescription]
	     ,[ODCMOQText]
	     ,[BOEID]
	     ,[ODCTaskID]
	     ,[UpdateDT]
	     ,[VersionID]
	     ,[TaskStartDate]
	     ,[TaskEndDate]
		   ,[SortOrderID]
	     )
SELECT OTE.[ODCTaskElementID]
	,OTE.[ODCTaskTitle]
	,OTE.[ODCTaskDescription]
	,OTE.[ODCMOQText]
	,OTE.[BOEID]
	,OTE.[ODCTaskID]
	,OTE.[UpdateDT]
	,@VersionID
	,OTE.[TaskStartDate]
	,OTE.[TaskEndDate]
	  ,OTE.[SortOrderID]
  FROM [dbo].[ODCTaskElement] OTE
	INNER JOIN dbo.BOE B ON OTE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[ODCType]
	     ([ODCTypeID]
		   ,[ResourceID]
	     ,[PerformingOrganizationID]
	     ,[ODCTypeStartDate]
	     ,[ODCTypeEndDate]
	     ,[SpreadCurveID]
	     ,[ODCTypeCost]
	     ,[ODCTaskElementID]
	     ,[UpdateDT]
	     ,[VersionID])
SELECT OT.[ODCTypeID]
	,OT.[ResourceID]
	,OT.[PerformingOrganizationID]
	,OT.[ODCTypeStartDate]
	,OT.[ODCTypeEndDate]
	,OT.[SpreadCurveID]
	,OT.[ODCTypeCost]
	,OT.[ODCTaskElementID]
	,OT.[UpdateDT]
	,@VersionID
 FROM [dbo].[ODCType] OT 
	INNER JOIN dbo.ODCTaskElement OTE ON OT.ODCTaskElementID = OTE.ODCTaskElementID
	INNER JOIN dbo.BOE B ON OTE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[ODCSpread]
	     ([ODCSpreadID]
		   ,[ODCSpreadDate]
	     ,[ODCSpreadValue]
	     ,[ODCTypeID]
	     ,[VersionID])
SELECT OS.[ODCSpreadID]
	,OS.[ODCSpreadDate]
	,OS.[ODCSpreadValue]
	,OS.[ODCTypeID]
	,@VersionID
FROM [dbo].[ODCSpread] OS
	INNER JOIN dbo.ODCType OT ON OS.ODCTypeID = OT.ODCTypeID
	INNER JOIN dbo.ODCTaskElement OTE ON OT.ODCTaskElementID = OTE.ODCTaskElementID
	INNER JOIN dbo.BOE B ON OTE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[MaterialTaskElement]
	     ([MaterialTaskElementID]
	     ,[MaterialTaskID]
	     ,[MaterialTaskTitle]
	     ,[MaterialTaskDescription]
	     ,[MaterialMOQText]
	     ,[BOEID]
	     ,[UpdateDT]
	     ,[VersionID])
SELECT TE.[MaterialTaskElementID]
	,TE.[MaterialTaskID]
	,TE.[MaterialTaskTitle]
	,TE.[MaterialTaskDescription]
	,TE.[MaterialMOQText]
	,TE.[BOEID]
	,TE.[UpdateDT]
	,@VersionID
  FROM [dbo].[MaterialTaskElement] TE
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID
	
INSERT INTO [version].[PerDiem]
	     ([PerDiemID]
		   ,[UpdateDT]
	     ,[PerDiemDestination]
	     ,[Qualification]
	     ,[HotelRate]
	     ,[MIERate]
	     ,[PerDiemNotes]
	     /*,[RentalCarRate]*/
	     ,[PerDiemLastUpdateETIUserID]
	     ,[PerDiemLastUpdateDT]
	     /*WI8256,[LockedRate]*/
	     ,[VersionID])
SELECT PD.[PerDiemID]
	,PD.[UpdateDT]
	,PD.[PerDiemDestination]
	,PD.[Qualification]
	,PD.[HotelRate]
	,PD.[MIERate]
	,PD.[PerDiemNotes]
	/*,PD.[RentalCarRate]*/
	,PD.[PerDiemLastUpdateETIUserID]
	,PD.[PerDiemLastUpdateDT]
	/*WI8256,PD.[LockedRate]*/
	,@VersionID
  FROM [dbo].[PerDiem] PD
	INNER JOIN [dbo].[Trip] T ON PD.PerDiemID = T.PerDiemID
	INNER JOIN [dbo].[TravelTrip] TT ON T.TripID = TT.TripID
	INNER JOIN [dbo].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

--WI 4881
INSERT INTO [version].[TravelMiscRate]
	     ([TravelMiscRateID]
	     ,[UpdateDT]
	     ,[TransportationMode]
	     ,[MiscellaneousRate]
	     ,[SortCode]
	     ,[MiscRateInUse]
/*WI8256           ,[LockedRate]*/
	     ,[VersionID])
SELECT MR.[TravelMiscRateID]
	,MR.[UpdateDT]
	,MR.[TransportationMode]
	,MR.[MiscellaneousRate]
	,MR.[SortCode]
	,MR.[MiscRateInUse]
/*WI8256      ,MR.[LockedRate]*/
	,@VersionID
  FROM [dbo].[TravelMiscRate] MR
	INNER JOIN [dbo].[Trip] T ON MR.TravelMiscRateID = T.TravelMiscRateID
	INNER JOIN [dbo].[TravelTrip] TT ON T.TripID = TT.TripID
	INNER JOIN [dbo].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

/*WI6261*/
INSERT INTO [version].[Trip]
	     ([TripID]
	     ,[UpdateDT]
	     ,[TravelMiscRateID]
	     ,[DepartureLocationID]
	     ,[DestinationLocationID]
	     ,[PerDiemID]
	     ,[TransportationFare]
	     ,[RoundTripMiles]
	     ,[FareLastUpdateDT]
	     ,[FareLastUpdateETIUserID]
	     ,[TripInUse]
	     ,[LastUsedDT]
/*WI8256           ,[LockedRate]*/
	     ,[VersionID]
/*WI8256           ,[Year]
	     ,[DevEscalation]
	     ,[LMSIEscalation]*/
	     ,[RentalCarRate]
	     ,[DestinationLocationCode]
	     ,[DepartureLocationCode]
	     )
SELECT T.[TripID]
	,T.[UpdateDT]
	,T.[TravelMiscRateID]
	,T.[DepartureLocationID]
	,T.[DestinationLocationID]
	,T.[PerDiemID]
	,T.[TransportationFare]
	,T.[RoundTripMiles]
	,T.[FareLastUpdateDT]
	,T.[FareLastUpdateETIUserID]
	,T.[TripInUse]
	,T.[LastUsedDT]
/*WI8256      ,T.[LockedRate]*/
	,@VersionID
/*WI8256      ,T.[Year]
	,T.[DevEscalation]
	,T.[LMSIEscalation]*/
	,T.[RentalCarRate]
	,T.[DestinationLocationCode]
	,T.[DepartureLocationCode]
FROM [dbo].[Trip] T 
	INNER JOIN [dbo].[TravelTrip] TT ON T.TripID = TT.TripID
	INNER JOIN [dbo].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[TravelTrip]
	     ([TravelTripID]
	     ,[UpdateDT]
	     ,[GroupID]
	     ,[SegmentID]
	     ,[PerformingOrganizationID]
	     ,[TripID]
	     ,[TripDate]
	     ,[NumTrips]
	     ,[NumPeople]
	     ,[NumDays]
	     ,[Purpose]           
	     ,[TravelTripTaskElementID]
	     ,[TripLockedDT]
 /*WI8256          ,[OriginatingTripID]*/
	     ,[VersionID])
SELECT TT.[TravelTripID]
	,TT.[UpdateDT]
	,TT.[GroupID]
	,TT.[SegmentID]
	,TT.[PerformingOrganizationID]
	,TT.[TripID]
	,TT.[TripDate]
	,TT.[NumTrips]
	,TT.[NumPeople]
	,TT.[NumDays]
	,TT.[Purpose]      
	,TT.[TravelTripTaskElementID]
	,TT.[TripLockedDT]
 /*WI8256     ,TT.[OriginatingTripID]*/
	,@VersionID
  FROM [dbo].[TravelTrip] TT
	INNER JOIN [dbo].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[TravelTripTaskElement]
	     ([TravelTripTaskElementID]
	     ,[UpdateDT]
	     ,[TravelTaskID]
	     ,[TravelTaskTitle]
	     ,[TravelTaskDescription]
	     ,[BOEID]
	     ,[VersionID]
	     ,[TaskStartDate]
	     ,[TaskEndDate]
		   ,[SortOrderID]
	     )
SELECT TE.[TravelTripTaskElementID]
	,TE.[UpdateDT]
	,TE.[TravelTaskID]
	,TE.[TravelTaskTitle]
	,TE.[TravelTaskDescription]
	,TE.[BOEID]
	,@VersionID
	,TE.[TaskStartDate]
	,TE.[TaskEndDate]
	  ,TE.[SortOrderID]
  FROM [dbo].[TravelTripTaskElement] TE 
	INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
	INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[TravelTripTaskElementCustomFieldValueXREF]
	     ([TTECFVID]
	     ,[TravelTripTaskElementID]
	     ,[CustomFieldValueID]
	     ,[UpdateDT]
	     ,[VersionID])
SELECT X.[TTECFVID]
	,X.[TravelTripTaskElementID]
	,X.[CustomFieldValueID]
	,X.[UpdateDT]
	,@VersionID
FROM [dbo].[TravelTripTaskElementCustomFieldValueXREF] X
	INNER JOIN dbo.TravelTripTaskElement TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[TravelTripCustomFieldValueXREF]
	     ([TCFVID]
	     ,[TravelTripID]
	     ,[CustomFieldValueID]
	     ,[UpdateDT]
	     ,[VersionID])
SELECT X.[TCFVID]
	,X.[TravelTripID]
	,X.[CustomFieldValueID]
	,X.[UpdateDT]
	,@VersionID
  FROM [dbo].[TravelTripCustomFieldValueXREF] X
	INNER JOIN dbo.TravelTrip M ON X.TravelTripID = M.TravelTripID
	INNER JOIN dbo.TravelTripTaskElement TE ON M.TravelTripTaskElementID = TE.TravelTripTaskElementID
	INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
	INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

-- Start of "RMS Zone Travel"
INSERT INTO [version].[MSTTravelTrip]
		([MSTTravelTripID]
		,[ModeID]
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
	  ,[VersionID]
		)
	SELECT 
		tt.[MSTTravelTripID]
		,tt.[ModeID]
		,tt.[TravelTripTaskElementID]
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
		,@VersionID
		FROM [dbo].[MSTTravelTrip] tt 
			INNER JOIN [dbo].TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN [dbo].BOE B ON TE.BOEID = B.BOEID
			INNER JOIN [dbo].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE
			WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[MSTTravelTripCustomFieldValueXREF]
			([MSTTCFVID]
			,[MSTTravelTripID]
			,[MSTCustomFieldValueID]
			,[UpdateDT]
			,[VersionID])
		SELECT 
			cI.[MSTTCFVID]
			,cI.[MSTTravelTripID]
			,cI.[MSTCustomFieldValueID]
			,cI.[UpdateDT]
			,@VersionID
		FROM [dbo].[MSTTravelTripCustomFieldValueXREF] cI 
			INNER JOIN [dbo].MSTTravelTrip tt ON cI.MSTTravelTripID = tt.MSTTravelTripID
			INNER JOIN [dbo].TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN [dbo].BOE B ON TE.BOEID = B.BOEID
			INNER JOIN [dbo].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE
			WS.WorkspaceID = @WorkspaceID
-- End of "RMS Zone Travel"

/*LOCKING TABLES*/
INSERT INTO [version].[WorkspaceLockedPerDiem]
	     ([WorkspaceLockedPerDiemID]
	     ,[PerDiemID]
	     ,[UpdateDT]
	     ,[PerDiemDestination]
	     ,[Qualification]
	     ,[HotelRate]
	     ,[MIERate]
	     ,[PerDiemNotes]
	     ,[PerDiemLastUpdateETIUserID]
	     ,[PerDiemLastUpdateDT]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT L.[WorkspaceLockedPerDiemID]
	,L.[PerDiemID]
	,L.[UpdateDT]
	,L.[PerDiemDestination]
	,L.[Qualification]
	,L.[HotelRate]
	,L.[MIERate]
	,L.[PerDiemNotes]
	,L.[PerDiemLastUpdateETIUserID]
	,L.[PerDiemLastUpdateDT]
	,L.[WorkspaceID]
	,@VersionID
  FROM [dbo].[WorkspaceLockedPerDiem] L
	INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceLockedTravelEscalationRate]
	     ([WorkspaceLockedTravelEscalationRateID]
	     ,[TravelEscalationRateID]
	     ,[UpdateDT]
	     ,[Year]
	     ,[DevEscalation]
	     ,[LMSIEscalation]
		   ,[MiscRate]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT L.[WorkspaceLockedTravelEscalationRateID]
	,L.[TravelEscalationRateID]
	,L.[UpdateDT]
	,L.[Year]
	,L.[DevEscalation]
	,L.[LMSIEscalation]
	  ,L.[MiscRate]
	,L.[WorkspaceID]
	,@VersionID
  FROM [dbo].[WorkspaceLockedTravelEscalationRate] L
	INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceLockedTravelMiscRate]
	     ([WorkspaceLockedTravelMiscRateID]
	     ,[TravelMiscRateID]
	     ,[UpdateDT]
	     ,[TransportationMode]
	     ,[MiscellaneousRate]
	     ,[SortCode]
	     ,[MiscRateInUse]
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT L.[WorkspaceLockedTravelMiscRateID]
	,L.[TravelMiscRateID]
	,L.[UpdateDT]
	,L.[TransportationMode]
	,L.[MiscellaneousRate]
	,L.[SortCode]
	,L.[MiscRateInUse]
	,L.[WorkspaceID]
	,@VersionID      
  FROM [dbo].[WorkspaceLockedTravelMiscRate] L
	INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceLockedTrip]
	     ([WorkspaceLockedTripID]
	     ,[TripID]
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
	     ,[WorkspaceID]
	     ,[VersionID])
SELECT L.[WorkspaceLockedTripID]
	,L.[TripID]
	,L.[UpdateDT]
	,L.[TravelMiscRateID]
	,L.[DepartureLocationID]
	,L.[DestinationLocationID]
	,L.[PerDiemID]
	,L.[TransportationFare]
	,L.[RoundTripMiles]
	,L.[FareLastUpdateETIUserID]
	,L.[FareLastUpdateDT]
	,L.[TripInUse]
	,L.[LastUsedDT]
	,L.[RentalCarRate]
	,L.[DepartureLocationCode]
	,L.[DestinationLocationCode]
	,L.[WorkspaceID]
	,@VersionID
  FROM  [dbo].[WorkspaceLockedTrip] L
	INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
WHERE WS.WorkspaceID = @WorkspaceID

/* Copy New INL Forms */
INSERT INTO [version].[BOEFormIBOE]
([IBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, BusinessArea, Revision,FormVersion, VersionId)
SELECT [IBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, BusinessArea, Revision,FormVersion, @VersionID
  FROM [dbo].[BOEFormIBOE]
WHERE WorkspaceID = @WorkspaceID 

INSERT INTO [version].[BOEFormIBOEResourcesXREF]
SELECT x.[IBOEFormID]
	  ,[ResourceID]
	  , @VersionID
FROM [dbo].[BOEFormIBOEResourcesXREF] x
	INNER JOIN [dbo].[BOEFormIBOE] B ON B.[IBOEFormID] = x.[IBOEFormID]
WHERE B.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEFormIBOECLINsXREF]
SELECT x.[IBOEFormID]
	  ,[ClinID]
	  ,[ContractType]
	  ,@VersionID
FROM [dbo].[BOEFormIBOECLINsXREF] x
	INNER JOIN [dbo].[BOEFormIBOE] B ON B.[IBOEFormID] = x.[IBOEFormID]
WHERE B.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEFormPBOE]
([PBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion,  
	[DegreeOfCompetition],	[CCoPD],	[CCoPDOtherText],	[RFP],	[ProposalNumber],	[SupplierName],	[ValidityDate],		[SupplierProposalSupportingDataIncluded],	[PriceAnalysisIncluded],	[CommercialItemDocIncluded],
	[CostAnalysisIncluded],	[ShouldCostEstimate],	[ShouldCostEstimateDate],	[SowWritten],	[SowWrittenDate],	[RFPRelease],	[RFPReleaseDate],	[FirmSupplierReceipt],	[FirmSupplierReceiptDate],	[SourceSelection],
	[SourceSelectionDate],	[CID],	[CIDDate],	[GovtReview],	[GovtReviewDate],	[PriceAnalysis],	[PriceAnalysisDate],	[TechnicalEvaluation],	[TechnicalEvaluationDate],	[FactFinding],	[FactFindingDate],
	[CostAnalysis],	[CostAnalysisDate],	[GovtPricing],	[GovtPricingDate],	[SupplierNegotiations],	[SupplierNegotiationsDate],	[MOU],	[MOUDate],	[Procurement],	[ProcurementDate],
	[PlannedDate_WrittenApproval],	[PlannedDate_ApprovedSubmission], VersionId, [CIDText],
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
	,[SupplierCCoPD]
	,[SourceSelectionDescription]
	,[CommercialityDescription]
	,[TechnicalEvaluationDescription]
	,[PriceAnalysisDescription]
	,[CostAnalysisDescription]
	,[RationaleValueSummary]
	,[GovtPricingReceived]
	,[GovtPricingReceivedDate]
	,[GovtPricingReceivedText]
	,[CostAnalysisUnqual]
	,[CostAnalysisUnqualDate]
	,[CostAnalysisUnqualText]
	,[VendorId]
	,[SupplierProposedValue])
SELECT [PBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion, [DegreeOfCompetition],
	[CCoPD],	[CCoPDOtherText],	[RFP],	[ProposalNumber],	[SupplierName],	[ValidityDate],	[SupplierProposalSupportingDataIncluded],	[PriceAnalysisIncluded],	[CommercialItemDocIncluded],
	[CostAnalysisIncluded],	[ShouldCostEstimate],	[ShouldCostEstimateDate],	[SowWritten],	[SowWrittenDate],	[RFPRelease],	[RFPReleaseDate],	[FirmSupplierReceipt],	[FirmSupplierReceiptDate],
	[SourceSelection],	[SourceSelectionDate],	[CID],	[CIDDate],	[GovtReview],	[GovtReviewDate],	[PriceAnalysis],	[PriceAnalysisDate],	[TechnicalEvaluation],	[TechnicalEvaluationDate],
	[FactFinding],	[FactFindingDate],	[CostAnalysis],	[CostAnalysisDate],	[GovtPricing],	[GovtPricingDate],	[SupplierNegotiations],	[SupplierNegotiationsDate],	[MOU],
	[MOUDate],	[Procurement],	[ProcurementDate],	[PlannedDate_WrittenApproval],	[PlannedDate_ApprovedSubmission], @VersionID, [CIDText],
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
	,[SupplierCCoPD]
	,[SourceSelectionDescription]
	,[CommercialityDescription]
	,[TechnicalEvaluationDescription]
	,[PriceAnalysisDescription]
	,[CostAnalysisDescription]
	,[RationaleValueSummary]
	,[GovtPricingReceived]
	,[GovtPricingReceivedDate]
	,[GovtPricingReceivedText]
	,[CostAnalysisUnqual]
	,[CostAnalysisUnqualDate]
	,[CostAnalysisUnqualText]
	,[VendorId]
	,[SupplierProposedValue]
  FROM [dbo].[BOEFormPBOE]
WHERE WorkspaceID = @WorkspaceID 

INSERT INTO [version].[BOEFormPBOEResourcesXREF]
SELECT x.[PBOEFormID]
	  ,[ResourceID]
	  ,@VersionID
FROM [dbo].[BOEFormPBOEResourcesXREF] x
	INNER JOIN [dbo].[BOEFormPBOE] B ON B.[PBOEFormID] = x.[PBOEFormID]
WHERE B.WorkspaceID = @WorkspaceID

INSERT INTO [version].[BOEFormPBOECLINsXREF]
SELECT x.[PBOEFormID]
	  ,[ClinID]
	  ,[ContractType]
	  ,@VersionID
FROM [dbo].[BOEFormPBOECLINsXREF] x
	INNER JOIN [dbo].[BOEFormPBOE] B ON B.[PBOEFormID] = x.[PBOEFormID]
WHERE B.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceRMSTravelEscalationRate] ([TravelEscalationRateID], UpdateDt, WorkspaceId, Year, Escalation, VersionId, MiscRate, PerDiemRate)
SELECT [TravelEscalationRateID]
		,[UpdateDT]
		,[WorkspaceID]
		,[Year]
		,[Escalation]
		,@VersionID
		,[MiscRate]
		,[PerDiemRate]
FROM [dbo].[WorkspaceRMSTravelEscalationRate] w
WHERE w.WorkspaceID = @WorkspaceID

INSERT INTO [version].[WorkspaceRMSTravelNonzoneFeesAndCosts]
SELECT FeesAndCostsID
		,[UpdateDT]
		,[WorkspaceID]
		,ModeID
		,TravelAgencyFee
		,MiscOther
		,@VersionID
FROM [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts] w
WHERE w.WorkspaceID = @WorkspaceID
/*
IF @@ERROR = 0
	BEGIN
		COMMIT TRANSACTION
	END
ELSE
	BEGIN
		ROLLBACK TRANSACTION
	END
*/

IF LEFT (@VersionName,  8) <> '[SYSTEM:' AND LEFT(@VersionName, 8) <> '[SYS:RPC'
	SELECT ID AS VersionID FROM @Inserted

GO


-- CREATE/ALTER deleteFullWorkspace
CREATE OR ALTER PROCEDURE [dbo].[deleteFullWorkspace]
(
@WorkspaceID int 
)
AS
/******************************************************************************
**		 
**		Name: [deleteFullWorkspace]
**		Desc: Delete the entire Workspace
**			
**		
**
**		Auth: Don Canuso
**		Date: 01/23/12
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**      1/17/17		twilson3			BOEJ-1604 Cleanup DB Project
**		3/3/2017	twilson3			BOEJ-1861 Remove DTS
**		5/4/2017	brunworg			BOEJ-2125 Add T&M Resource Rates
**		12/7/2017	twilson3			BOEJ-2250 Remove DTC
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		6/29/2018	twilson3			BOEJ-3551 New Homepage Table
**		10/25/2018	twilson3			BOEJ-3878 Email Table and WorkspaceOffloadRate Table.  Moved deletes around to mimic deleteMockTestData.sql for easier Compare in future
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**		5/22/20		Dusan				BOEJ-4616 Cleanning up the method, as it was neglegted
**		9/15/20		ranzalon			BOEJ-4776/4825 - MOQ Types update
**		1/4/2021	Dusan				BOEJ-4894: Added support for MoqTypeTableCustomFieldValueXREF
**		11/18/2021  e405721				IES-528: Process Soft Delete is Failing
**		12/6/2021	Dusan				IES-666: Issue w/ order of deletions, failing due to FK constraints w/ MoqTypeTableCustomFieldValueXREF
**		10/04/2023	hrafiqzadah			PROPH-1031: Update to delete from ProjectMapSpread
**		7/18/2024	e405721				PROPH-2165: Update Delete Full Workspace for Skill Mix, Common Disclosure, and MOQ Type Resource Hours Table Data
**		9/30/25		e378233				PROPH-3302 Updated for Skill Mix Summary
*******************************************************************************/
SET NOCOUNT ON 
	IF @WorkspaceID IS NULL
	RETURN
	BEGIN TRANSACTION
		DECLARE @MockWorkspace TABLE
(
	WorkspaceID int PRIMARY KEY
)	
		INSERT INTO @MockWorkspace VALUES (@WorkspaceID)
		DECLARE @VersionID int
		DECLARE @Metric TABLE (MetricID int)
		DECLARE @TripAffected TABLE (TripID int)	
		DECLARE @WorkspaceVersion TABLE
			(
				WorkspaceID int,
				VersionID int,
				Processed bit
			)
		INSERT INTO @WorkspaceVersion
		SELECT WV.WorkspaceID, WV.VersionID, 0 
		FROM dbo.WorkspaceVersion WV
			INNER JOIN @MockWorkspace W ON WV.WorkspaceID = W.WorkspaceID
		WHILE EXISTS (SELECT 1 FROM @WorkspaceVersion WHERE Processed = 0)
		BEGIN
			SELECT TOP 1 
						@WorkspaceID = WorkspaceID, 
						@VersionID = VersionID 
			FROM @WorkspaceVersion WHERE Processed = 0

			EXECUTE dbo.deleteWorkspaceVersion @WorkspaceID, @VersionID

			UPDATE @WorkspaceVersion 
			SET Processed = 1
			WHERE	WorkspaceID = @WorkspaceID AND
					VersionID = @VersionID
		END
		DELETE FROM dbo.RteTemplateAssigned WHERE TemplateID IN (SELECT TemplateId FROM dbo.RteTemplate WHERE WorkspaceId = @WorkspaceID);
		DELETE FROM dbo.RteTemplateAnswer WHERE QuestionId IN (SELECT QuestionId FROM dbo.RteTemplateQuestion WHERE TemplateID IN (SELECT TemplateId FROM dbo.RteTemplate WHERE WorkspaceId = @WorkspaceID));
		DELETE FROM dbo.RteTemplateQuestion WHERE TemplateID IN (SELECT TemplateId FROM dbo.RteTemplate WHERE WorkspaceId = @WorkspaceID);
		DELETE FROM dbo.RteTemplate WHERE WorkspaceId = @WorkspaceID;
		DELETE FROM dbo.ProPricerCustomFieldXREF
			FROM dbo.ProPricerCustomFieldXREF X
			INNER JOIN dbo.CustomField CF ON X.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOELaborSpread
			FROM dbo.BOELaborSpread LS
				INNER JOIN dbo.BOELaborType LT ON LS.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
				INNER JOIN dbo.BOELaborType LT ON X.BOELaborTypeID = LT.BOELaborTypeID
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF
			FROM dbo.BOELaborTypeCustomFieldValueXREF X
			INNER JOIN dbo.CustomFieldValue CFV ON CFV.CustomFieldValueID = X.CustomFieldValueID
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF
			FROM dbo.SumOfBOE_OrdinaryVariableXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF 
				FROM dbo.SumOfBOE_OrdinaryVariableXREF X
				INNER JOIN dbo.BOE B ON X.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF
			FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF X
				INNER JOIN dbo.OrdinaryVariable OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.OrdinaryVariable
			FROM dbo.OrdinaryVariable OV
				INNER JOIN dbo.BOETaskElement TE ON OV.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM [dbo].[BOECustomFieldValueXREF]
		FROM [dbo].[BOECustomFieldValueXREF] X
			INNER JOIN dbo.BOE B ON X.BOEID = B.BOEID
			INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM [dbo].[BOECustomFieldValueXREF]
		FROM [dbo].[BOECustomFieldValueXREF] X
			INNER JOIN dbo.CustomFieldValue CFV ON CFV.CustomFieldValueID = X.CustomFieldValueID
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOELaborType
			FROM dbo.BOELaborType LT 
				INNER JOIN dbo.BOETaskElement TE ON LT.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
				
		DELETE FROM dbo.[MoqTypeTableCustomFieldValueXREF]
			FROM dbo.[MoqTypeTableCustomFieldValueXREF] x
				INNER JOIN MoqTypeSelectionTableData t ON t.MoqTypeSelectionTableDataId = x.MoqTypeTableDataId 
				INNER JOIN MoqTypeSelection mS ON mS.MoqTypeSelectionId = t.MoqTypeSelectionId 
				INNER JOIN BoeTaskElement tE ON tE.BoeTaskElementId = mS.TaskId 
				INNER JOIN dbo.BOE B ON tE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.[MOQTypeSelectionTableDataResourceHours]
			FROM dbo.[MOQTypeSelectionTableDataResourceHours] M
			INNER JOIN dbo.BOE B ON M.BOEID = B.BOEID
			INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.MOQTypeSelectionTableData
			FROM dbo.MOQTypeSelectionTableData TD
				INNER JOIN dbo.MOQTypeSelection M on TD.MOQTypeSelectionId = M.MOQTypeSelectionId
				INNER JOIN dbo.BOETaskElement T ON M.TaskId = T.BOETaskElementID
				INNER JOIN dbo.BOE B ON T.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOEApprovalHistory 
				FROM dbo.BOEApprovalHistory BAH 
				INNER JOIN dbo.BOE B ON BAH.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOECommentHistory
				FROM dbo.BOECommentHistory BCH 
				INNER JOIN dbo.BOE B ON BCH.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOETaskElementCustomFieldValueXREF
			FROM dbo.BOETaskElementCustomFieldValueXREF X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOETaskElementCustomFieldValueXREF
			FROM dbo.BOETaskElementCustomFieldValueXREF X 
			INNER JOIN dbo.CustomFieldValue CFV ON CFV.CustomFieldValueID = X.CustomFieldValueID
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID
		DELETE FROM [dbo].[BOETaskElementMetricDetailXREF]
				FROM [dbo].[BOETaskElementMetricDetailXREF]  X 
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID 
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOETaskElementWorkspaceVariableXREF
			FROM  dbo.BOETaskElementWorkspaceVariableXREF X
				INNER JOIN dbo.BOETaskElement TE ON X.BOETaskElementID = TE.BOETaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID

		DELETE FROM dbo.[MOQTypeSelectionTableData]
			FROM dbo.[MOQTypeSelectionTableData] Mtd
				INNER JOIN dbo.MOQTypeSelection M ON M.MOQTypeSelectionId = Mtd.MOQTypeSelectionId
				INNER JOIN dbo.BOETaskElement T ON M.TaskId = T.BOETaskElementID
				INNER JOIN dbo.BOE B ON T.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.[CommonDisclosureSkillMix]
			FROM dbo.[CommonDisclosureSkillMix] CD
			INNER JOIN dbo.BOE B ON CD.BOEID = B.BOEID
			INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.[SkillMix]
			FROM dbo.[SkillMix] SM
			INNER JOIN dbo.BOE B ON SM.BOEID = B.BOEID
			INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.[SkillMixSummary]
			FROM dbo.[SkillMixSummary] SMS
			INNER JOIN dbo.BOE B ON SMS.BOEID = B.BOEID
			INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.MOQTypeSelection
			FROM dbo.MOQTypeSelection M
				INNER JOIN dbo.BOETaskElement T ON M.TaskId = T.BOETaskElementID
				INNER JOIN dbo.BOE B ON T.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID

		DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF WHERE WorkspaceVariableId IN (SELECT WorkspaceVariableId FROM WorkspaceVariable WHERE WorkspaceId = @workspaceId);
		DELETE FROM dbo.BOEUserRoleHistory
				FROM dbo.BOEUserRoleHistory TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOEUserRole
				FROM dbo.BOEUserRole TE 
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOETaskElement
			FROM dbo.BOETaskElement TE 
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOEComment
				FROM dbo.BOEComment TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOEApproval
				FROM dbo.BOEApproval TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.BOEStateHistory
				FROM dbo.BOEStateHistory TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.WBS_CLIN_BOE_XREF
				FROM dbo.WBS_CLIN_BOE_XREF TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.MaterialTaskElement
			FROM dbo.MaterialTaskElement TE
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.ODCSpread                 
			FROM dbo.ODCSpread S
				INNER JOIN dbo.ODCType T ON S.ODCTypeID = T.ODCTypeID
				INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.ODCType                 
			FROM dbo.ODCType T 
				INNER JOIN dbo.ODCTaskElement TE ON T.ODCTaskElementID = TE.ODCTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.ODCTaskElement                 
			FROM dbo.ODCTaskElement TE 
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.TravelTripCustomFieldValueXREF
				FROM dbo.TravelTripCustomFieldValueXREF X
				INNER JOIN dbo.TravelTrip T ON X.TravelTripID = T.TravelTripID
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.TravelTripCustomFieldValueXREF
				FROM dbo.TravelTripCustomFieldValueXREF X	
			INNER JOIN dbo.CustomFieldValue CFV ON CFV.CustomFieldValueID = X.CustomFieldValueID
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.MSTTravelTripCustomFieldValueXREF
				FROM dbo.MSTTravelTripCustomFieldValueXREF cI
				INNER JOIN dbo.MSTTravelTrip tt ON cI.MSTTravelTripID = tt.MSTTravelTripID
				INNER JOIN dbo.TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.MSTTravelTripCustomFieldValueXREF
				FROM dbo.MSTTravelTripCustomFieldValueXREF X	
			INNER JOIN dbo.CustomFieldValue CFV ON CFV.CustomFieldValueID = X.MSTCustomFieldValueID
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.MSTTravelTrip 
			  FROM dbo.MSTTravelTrip tt
				INNER JOIN dbo.TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		INSERT INTO @TripAffected 
			SELECT TripID
			FROM dbo.TravelTrip T
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.TravelTrip                 
			FROM dbo.TravelTrip T 
				INNER JOIN dbo.TravelTripTaskElement TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.TravelTripTaskElementCustomFieldValueXREF				
				FROM dbo.TravelTripTaskElementCustomFieldValueXREF X
				INNER JOIN dbo.TravelTripTaskElement TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.TravelTripTaskElementCustomFieldValueXREF				
			FROM dbo.TravelTripTaskElementCustomFieldValueXREF X
			INNER JOIN dbo.CustomFieldValue CFV ON CFV.CustomFieldValueID = X.CustomFieldValueID
			INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
			INNER JOIN @MockWorkspace WS ON CF.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.TravelTripTaskElement                 
			FROM dbo.TravelTripTaskElement TE 
				INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.CustomFieldValue 
			FROM dbo.CustomFieldValue CFV
				INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
				INNER JOIN @MockWorkspace W ON CF.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.CustomField 
			FROM dbo.CustomField CF 
				INNER JOIN @MockWorkspace W ON CF.WorkspaceID = W.WorkspaceID
		UPDATE dbo.Trip
				SET TripInUse = CASE 
					WHEN TT.TripID IS NULL THEN 0
					WHEN TT.TripID IS NOT NULL THEN 1
					END,
					UpdateDT = GETDATE()
			FROM dbo.Trip T  
				INNER JOIN @TripAffected TA ON T.TripID = TA.TripID
				LEFT OUTER JOIN 
					(
						SELECT TripID AS TripID FROM dbo.TravelTrip
						
					) TT ON T.TripID = TT.TripID
		DELETE FROM dbo.WBS_CLIN_BOE_XREF
			FROM dbo.WBS_CLIN_BOE_XREF X 
				INNER JOIN dbo.WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID
				INNER JOIN @MockWorkspace W ON WBS.WorkspaceID = W.WorkspaceID
		DELETE FROM [dbo].[WorkspaceEmailXREF]
			FROM [dbo].[WorkspaceEmailXREF] xref
				INNER JOIN @MockWorkspace W ON xref.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.WorkBreakdownStructure
			FROM dbo.WorkBreakdownStructure WBS 
				INNER JOIN @MockWorkspace W ON WBS.WorkspaceID = W.WorkspaceID
		DELETE FROM [dbo].[BOEFormIBOEResourcesXREF]
				FROM [dbo].[BOEFormIBOEResourcesXREF] x
					INNER JOIN [dbo].[BOEFormIBOE] BF ON x.[IBOEFormID] = BF.[IBOEFormID]
					INNER JOIN @MockWorkspace W ON BF.WorkspaceID = W.WorkspaceID
		DELETE FROM [dbo].[BOEFormIBOECLINsXREF]
				FROM [dbo].[BOEFormIBOECLINsXREF] x
					INNER JOIN [dbo].[BOEFormIBOE] BF ON x.[IBOEFormID] = BF.[IBOEFormID]
					INNER JOIN @MockWorkspace W ON BF.WorkspaceID = W.WorkspaceID
		DELETE FROM [dbo].[BOEFormIBOE]
				FROM [dbo].[BOEFormIBOE] BF
					INNER JOIN @MockWorkspace W ON BF.WorkspaceID = W.WorkspaceID
		DELETE FROM [dbo].[BOEFormPBOEResourcesXREF]
				FROM [dbo].[BOEFormPBOEResourcesXREF] x
					INNER JOIN [dbo].[BOEFormPBOE] BF ON x.[PBOEFormID] = BF.[PBOEFormID]
					INNER JOIN @MockWorkspace W ON BF.WorkspaceID = W.WorkspaceID
		DELETE FROM [dbo].[BOEFormPBOECLINsXREF]
				FROM [dbo].[BOEFormPBOECLINsXREF] x
					INNER JOIN [dbo].[BOEFormPBOE] BF ON x.[PBOEFormID] = BF.[PBOEFormID]
					INNER JOIN @MockWorkspace W ON BF.WorkspaceID = W.WorkspaceID
		DELETE FROM [dbo].[BOEFormPBOE]
				FROM [dbo].[BOEFormPBOE] BF
					INNER JOIN @MockWorkspace W ON BF.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.BOE	
			FROM dbo.BOE B 
				INNER JOIN @MockWorkspace WS ON B.WorkspaceID = WS.WorkspaceID;
		DELETE FROM [dbo].[WorkspaceVariableSumVariableResourceTypeXREF]
				FROM [dbo].[WorkspaceVariableSumVariableResourceTypeXREF] X 
					INNER JOIN dbo.WorkspaceVariable WV ON X.WorkspaceVariableID = WV.WorkspaceVariableID
					INNER JOIN dbo.Workspace W ON WV.WorkspaceID = W.WorkspaceID
					INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID					
		DELETE FROM [dbo].[WorkspaceStateHistory]
			FROM [dbo].[WorkspaceStateHistory] WH 
					INNER JOIN  dbo.Workspace W ON WH.WorkspaceID = W.WorkspaceID
					INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID
		DELETE FROM [dbo].[WorkspaceUserRole]
			FROM [dbo].[WorkspaceUserRole] WUR
				INNER JOIN  @MockWorkspace W ON WUR.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.WorkspaceVariable 
				FROM dbo.WorkspaceVariable  WV
					INNER JOIN @MockWorkspace W ON WV.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.WBS_CLIN_BOE_XREF 
		FROM dbo.WBS_CLIN_BOE_XREF X
			INNER JOIN dbo.CLIN C ON X.CLINID = C.CLINID
			INNER JOIN @MockWorkspace WS ON C.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF
		FROM dbo.SumOfBOE_OrdinaryVariableXREF X
			INNER JOIN dbo.CLIN C ON X.CLINID = C.CLINID
			INNER JOIN @MockWorkspace WS ON C.WorkspaceID = WS.WorkspaceID
		DELETE FROM dbo.CLIN
			FROM dbo.CLIN C 
				INNER JOIN @MockWorkspace WS ON C.WorkspaceID = WS.WorkspaceID
		DELETE FROM [dbo].[BOEPotentialRole]
			FROM dbo.BOEPotentialRole PR
				INNER JOIN @MockWorkspace W ON PR.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.[WorkspaceUserXREF]
			FROM dbo.[WorkspaceUserXREF] xref
			INNER JOIN  @MockWorkspace W ON xref.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.WorkspaceUserRole 
			FROM dbo.WorkspaceUserRole WUR
			INNER JOIN @MockWorkspace W ON WUR.WorkspaceID = W.WorkspaceID							 
		DELETE FROM dbo.TMResourceRate
     		FROM dbo.TMResourceRate TMRR
				INNER JOIN @MockWorkspace W ON TMRR.WorkspaceID = W.WorkspaceID								 
		DELETE FROM dbo.WorkspaceOffloadRate
			FROM dbo.WorkspaceOffloadRate WOR
				INNER JOIN @MockWorkspace W ON WOR.WorkspaceID = W.WorkspaceID	
		DELETE FROM [dbo].[WorkspaceResource]
			FROM [dbo].[WorkspaceResource] WR
				INNER JOIN @MockWorkspace MW ON WR.WorkspaceID = MW.WorkspaceID
		DELETE FROM [dbo].[WorkspacePerformingOrganization]
			FROM [dbo].[WorkspacePerformingOrganization] WR
				INNER JOIN @MockWorkspace MW ON WR.WorkspaceID = MW.WorkspaceID
		DELETE FROM [dbo].[PerformingOrganization] 
			FROM [dbo].[PerformingOrganization] PO
				INNER JOIN dbo.PerformingOrganizationList PL ON PO.PerformingOrganizationListID = PL.PerformingOrganizationListID
				INNER JOIN dbo.Workspace W ON PL.PerformingOrganizationListID = W.PerformingOrganizationListID
				INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID				
			WHERE PO.PerformingOrganizationListID <> 1
		DELETE FROM [dbo].[Resource] 
			FROM [dbo].[Resource] R
				INNER JOIN dbo.ResourceList RL ON R.ResourceListID = RL.ResourceListID
				INNER JOIN dbo.Workspace W ON RL.ResourceListID = W.ResourceListID
				INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID
			WHERE R.ResourceListID <> 1				 
		DELETE FROM dbo.WorkspaceRestoreLog
			FROM dbo.WorkspaceRestoreLog WRL 
			INNER JOIN @MockWorkspace W ON WRL.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.OutputFormatTemplateWorkspaceXREF
			FROM dbo.OutputFormatTemplateWorkspaceXREF X
			INNER JOIN @MockWorkspace W ON X.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.CustomFieldValue
			FROM dbo.CustomFieldValue CFV
				INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
				INNER JOIN @MockWorkspace W ON CF.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.CustomField
			FROM dbo.CustomField CF
			INNER JOIN @MockWorkspace W ON CF.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.WorkspaceLockedTrip
  FROM dbo.WorkspaceLockedTrip L
	INNER JOIN dbo.Workspace W ON L.WorkspaceID = W.WorkspaceID
	INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID				
		DELETE FROM dbo.WorkspaceLockedTravelMiscRate
  FROM dbo.WorkspaceLockedTravelMiscRate L
	INNER JOIN dbo.Workspace W ON L.WorkspaceID = W.WorkspaceID
	INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID				
		DELETE FROM dbo.WorkspaceLockedPerDiem
  FROM dbo.WorkspaceLockedPerDiem L
	INNER JOIN dbo.Workspace W ON L.WorkspaceID = W.WorkspaceID
	INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID				
		DELETE FROM dbo.WorkspaceLockedTravelEscalationRate
  FROM dbo.WorkspaceLockedTravelEscalationRate L
	INNER JOIN dbo.Workspace W ON L.WorkspaceID = W.WorkspaceID
	INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID				
		DELETE FROM dbo.WorkspaceContractTypeXREF
FROM dbo.WorkspaceContractTypeXREF X
INNER JOIN @MockWorkspace W ON X.WorkspaceID = W.WorkspaceID
		DELETE FROM [dbo].[ProPricerCustomFieldXREF]
FROM [dbo].[ProPricerCustomFieldXREF] X
INNER JOIN  dbo.ProPricerExport PE ON X.[ProPricerExportID] = PE.[ProPricerExportID]
INNER JOIN  @MockWorkspace W ON PE.WorkspaceID = W.WorkspaceID
		DELETE FROM [dbo].[ProPricerFieldXREF]
FROM [dbo].[ProPricerFieldXREF] X
INNER JOIN  dbo.ProPricerExport PE ON X.[ProPricerExportID] = PE.[ProPricerExportID]
INNER JOIN  @MockWorkspace W ON PE.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.ProPricerExport
FROM dbo.ProPricerExport PE
INNER JOIN  @MockWorkspace W ON PE.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.[WorkspaceRMSTravelEscalationRate]
FROM dbo.[WorkspaceRMSTravelEscalationRate] R
INNER JOIN  @MockWorkspace W ON R.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.[WorkspaceRMSTravelNonzoneFeesAndCosts]
FROM dbo.[WorkspaceRMSTravelNonzoneFeesAndCosts] F
INNER JOIN  @MockWorkspace W ON F.WorkspaceID = W.WorkspaceID
		DELETE FROM dbo.Workspace
			FROM dbo.Workspace W
			INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID
		DELETE FROM dbo.PerformingOrganizationList
			FROM dbo.PerformingOrganizationList PL
				INNER JOIN dbo.Workspace W ON PL.PerformingOrganizationListID = W.PerformingOrganizationListID
				INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID				
		DELETE FROM dbo.ResourceList
			FROM dbo.ResourceList RL
				INNER JOIN dbo.Workspace W ON RL.ResourceListID = W.ResourceListID
				INNER JOIN @MockWorkspace MW ON W.WorkspaceID = MW.WorkspaceID

/** PROPH-1031 - Delete from ProjectMapSpread **/
DELETE FROM ProjectMapSpread WHERE WorkspaceId = @WorkspaceID;
DELETE FROM ProjectMap WHERE WorkspaceId = @WorkspaceID;

IF @@ERROR = 0
	COMMIT TRANSACTION
ELSE
	ROLLBACK TRANSACTION

GO


-- CREATE/ALTER deleteWorkspace
CREATE OR ALTER PROCEDURE [dbo].[deleteWorkspace]
(
@WorkspaceID int,
@UpdateDT datetime2
)
AS
/******************************************************************************
**		 
**		Name: [deleteWorkspace]
**		Desc: Delete Workspace 
**			
**		
**
**		Auth: Don Canuso
**		Date: 01/10/12
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		8/21/13		dcanuso				workspace can have multiple 
**										contract types
**		8/27/14		dcanuso				Image Story
**		10/3/14		dcanuso				Image Story Removal
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
**		6/29/2018	twilson3			BOEJ-3551 New Homepage Table
*******************************************************************************/
SET NOCOUNT ON 


	IF (SELECT UpdateDT FROM [dbo].[Workspace] WHERE WorkspaceID = @WorkspaceID ) = @UpdateDT
		BEGIN
		
			DELETE FROM [dbo].[WorkspaceContractTypeXREF] 
			WHERE
				WorkspaceID = @WorkspaceID

			DELETE FROM [dbo].[WorkspaceUserXREF]
			WHERE
				WorkspaceID = @WorkspaceID
		
			DELETE FROM dbo.Workspace
			WHERE
				WorkspaceID = @WorkspaceID
	
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Workspace with ID ' + CAST(@WorkspaceID  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO


-- CREATE/ALTER deleteWorkspaceVersion
CREATE OR ALTER  PROCEDURE [dbo].[deleteWorkspaceVersion]
(
@WorkspaceID int,
@VersionID int
)
/******************************************************************************
**		 
**		Name:	[deleteWorkspaceVersion]
**		Desc:	Deletes all of the data inserted from createWorkspaceVersion
**				including the deletion of the record in the Workspace Version Table
**			
**		
**
**		Auth: Don Canuso
**		Date: 5/17/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**      1/17/17		twilson3			BOEJ-1604 Cleanup DB Project
**		3/3/2017	twilson3			BOEJ-1861 Remove DTS
**		5/4/2017	brunworg			BOEJ-2125 Add T&M Resource Rates
**		9/25/2017	twilson3			BOEJ-2535 Add ProjectMap Tables
**		12/7/2017	twilson3			BOEJ-2250 Remove DTC
**		12/15/17	twilson3			BOEJ-2248 Remove Labor Rates
**		1/11/18		ranzalon			BOEJ-2889 Updated template backup
**		1/16/18		twilson3			BOEJ-2887 Remove Historical Metrics
**		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, MaterialXref
**		12/13/19	twilson3			BOEJ-4434 - RTE Template Answers
**		12/17/19	twilson3			BOEJ-4434 Fix Assigned
**		9/15/20		ranzalon			BOEJ-4776/4825 - MOQ Types update
**		1/4/2021	Dusan				BOEJ-4894: Added support for MoqTypeTableCustomFieldValueXREF; additional cleanup
**		7/11/2024	e405721				PROPH-2136: Update for Skill Mix, Common Disclosure, and MOQ Type Selection Table Data Resource Hours
**		9/30/25		e378233				PROPH-3302 Updated for Skill Mix Summary
*******************************************************************************/
AS
SET NOCOUNT ON

IF EXISTS (SELECT 1 FROM dbo.WorkspaceVersion WHERE VersionID = @VersionID AND WorkspaceID = @WorkspaceID)
BEGIN
	BEGIN TRANSACTION
	/*DELETE THE DATA FROM TABLES*/
		
	DELETE FROM [version].[BOE] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEApproval] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEApprovalHistory] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEComment] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOECommentHistory] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOECustomFieldValueXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEFormIBOE] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEFormIBOEResourcesXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEFormIBOECLINsXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEFormPBOE] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEFormPBOEResourcesXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEFormPBOECLINsXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOELaborSpread] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOELaborType] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOELaborTypeCustomFieldValueXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEPotentialRole] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEStateHistory] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOETaskElement] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOETaskElementCustomFieldValueXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOETaskElementWorkspaceVariableXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEUserRole] WHERE VersionID = @VersionID
	DELETE FROM [version].[BOEUserRoleHistory] WHERE VersionID = @VersionID
	DELETE FROM [version].[CLIN] WHERE VersionID = @VersionID
	DELETE FROM [version].[ProjectMap] WHERE VersionID = @VersionID
	DELETE FROM [version].[ProjectMapSpread] WHERE VersionID = @VersionID
	DELETE FROM [version].[CustomField] WHERE VersionID = @VersionID
	DELETE FROM [version].[CustomFieldValue] WHERE VersionID = @VersionID
	DELETE FROM [version].[MaterialTaskElement] WHERE VersionID = @VersionID
	DELETE FROM [version].[MoqTypeTableCustomFieldValueXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[MOQTypeSelectionTableDataResourceHours] WHERE VersionID = @VersionID
	DELETE FROM [version].[MOQTypeSelectionTableData] WHERE VersionID = @VersionID
	DELETE FROM [version].[CommonDisclosureSkillMix] WHERE VersionID = @VersionID
	DELETE FROM [version].[SkillMix] WHERE VersionID = @VersionID
	DELETE FROM [version].[SkillMixSummary] WHERE VersionID = @VersionID
	DELETE FROM [version].[MOQTypeSelection] WHERE VersionID = @VersionID
	DELETE FROM [version].[ODCSpread] WHERE VersionID = @VersionID
	DELETE FROM [version].[ODCTaskElement] WHERE VersionID = @VersionID
	DELETE FROM [version].[ODCType] WHERE VersionID = @VersionID
	DELETE FROM [version].[OrdinaryVariable] WHERE VersionID = @VersionID
	DELETE FROM [version].[OrdinaryVariableSumVariableResourceTypeXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[OutputFormatTemplateVersionXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[OutputFormatTemplateWorkspaceXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[PerDiem] WHERE VersionID = @VersionID
	DELETE FROM [version].[PerformingOrganization] WHERE VersionID = @VersionID
	DELETE FROM [version].[PerformingOrganizationList] WHERE VersionID = @VersionID
	DELETE FROM [version].[ProPricerCustomFieldXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[ProPricerExport] WHERE VersionID = @VersionID
	DELETE FROM [version].[ProPricerFieldXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[Resource] WHERE VersionID = @VersionID
	DELETE FROM [version].[ResourceList] WHERE VersionID = @VersionID
	DELETE FROM [version].[RteTemplateAnswer] WHERE VersionID = @VersionID
	DELETE FROM [version].[RteTemplateAssigned] WHERE VersionID = @VersionID
	DELETE FROM [version].[RteTemplate] WHERE VersionID = @VersionID
	DELETE FROM [version].[RteTemplateQuestion] WHERE VersionID = @VersionID
	DELETE FROM [version].[SumOfBOE_OrdinaryVariableXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[SumOfBOE_WorkspaceVariableXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[TravelMiscRate] WHERE VersionID = @VersionID
	DELETE FROM [version].[TravelTrip] WHERE VersionID = @VersionID
	DELETE FROM [version].[TravelTripCustomFieldValueXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[TravelTripTaskElement] WHERE VersionID = @VersionID
	DELETE FROM [version].[TravelTripTaskElementCustomFieldValueXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[Trip] WHERE VersionID = @VersionID
	DELETE FROM [version].[WBS_CLIN_BOE_XREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkBreakdownStructure] WHERE VersionID = @VersionID
	DELETE FROM [version].[Workspace] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspaceContractTypeXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspaceLockedPerDiem] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspaceLockedTravelEscalationRate] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspaceLockedTravelMiscRate] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspaceLockedTrip] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspacePerformingOrganization] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspaceResource] WHERE VersionID = @VersionID
	DELETE FROM [version].[TMResourceRate] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspaceStateHistory] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspaceUserRole] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspaceVariable] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspaceVariableSumVariableResourceTypeXREF] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspaceRMSTravelEscalationRate] WHERE VersionID = @VersionID
	DELETE FROM [version].[WorkspaceRMSTravelNonzoneFeesAndCosts] WHERE VersionID = @VersionID

-- Start of "RMS Zone Travel"
	DELETE FROM [version].[MSTTravelTrip] WHERE VersionID = @VersionID
	DELETE FROM [version].[MSTTravelTripCustomFieldValueXREF] WHERE VersionID = @VersionID
-- End of "RMS Zone Travel"

	DELETE FROM [dbo].[WorkspaceVersion] WHERE VersionID = @VersionID

END
ELSE
	BEGIN
			DECLARE @ErrorMessage varchar (500)
			SET @ErrorMessage =   'The Workspace Version with ID ' + CAST(@WorkspaceID  AS varchar(10)) +  ' and Version ID ' + CAST(@VersionID  AS varchar(10)) + ' does not exist'
			RAISERROR (@ErrorMessage, 11, 1)
			RETURN
	END


IF @@ERROR = 0
	BEGIN
		COMMIT TRANSACTION
	END
ELSE
	BEGIN
		ROLLBACK TRANSACTION
	END

GO


-- CREATE/ALTER restoreWorkspaceVersion
CREATE OR ALTER  PROCEDURE [dbo].[restoreWorkspaceVersion] (@VersionID int, @ETIUserID int, @WorkspaceID int)
AS
/******************************************************************************
**		 
**		Name: [restoreWorkspaceVersion]
**		Desc:	Creates a System Back Up
**				Deletes the data in the active ([dbo]) Workspace
**				Restores the data in the Back Up ([version]) Workspace
**				Update States and create logs
**
**				Metrics:
**				If a Metric was deleted, it will not be restored.
**				The row in the BOE Task Element  Metric  XREF table will be deleted.
**				
**				Resources and Performing Organizations:
**				If a Resource or a P.O. is deleted, the row in the BOE Labor Type table will now contain NULL 
**				values for P.O. and Resource rather than the ID.
**				A user will need to fix this before submitting for approval.
**				
**				Custom Fields/Output Format Template/ Workspace Variables:
**				Restored exactly how they were backed up.
**				
**				When the restore completes, a string will be sent to the Back End describing what did not work, 
**				for example, Resources XYZ was unable to be restored.
**				
**				Also, when a user starts a restore, the stored procedure will kick off a Back Up with SYSTEM: at the 
**				start of the version
**				
**		Auth: Don Canuso
**		Date: 6/7/11
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		02/13/20	ranzalon			BOEJ-4506 - Fix RTE Assigned, RTE template deletion order
**		8/27/20		ranzalon			BOEJ-4760 - Template Boe
**		9/15/20		ranzalon			BOEJ-4776/4825 - MOQ Types update
**		12/8/2020	ranzalon			BOEJ-4972 - remove CER location and BOELaborType MOQTypeSelectionId fields
**		1/4/2021	Dusan				BOEJ-4894: Added support for MoqTypeTableCustomFieldValueXREF; additional cleanup
**		1/31/23		e405721				ACV-221 - Enable SAP Connection
**		1/18/24		ranzalon			PROPH-1070 Update for HistoricalReferenceExplanation
**		1/28/24		e302876  			PROPH-1492 ADD BRC to Copy BOEs, Copy WS, Archive/Restore
**		7/16/24		e405721				PROPH-2161: Update Restore Workspace Version for Skill Mix, Common Disclosure, and MOQ Type Resource Hours Table Data
**		7/30/24		e405721				PROPH-2218 Add BRC Name into MOQ Type Selection Table Data Resource Hours Table
**		10/15/24	e405721				PROPH-2392: Update for Skill Mix V2
**		1/14/25		twilson3			PROPH-2596 - Add UCOT Factor
**		1/15/25		e309214				PROPH-1854 Database Changes for Assign Author
**		9/30/25		e378233				PROPH-3302 Updated for Skill Mix Summary
*******************************************************************************/
SET NOCOUNT ON 

IF EXISTS (SELECT 1 FROM dbo.WorkspaceVersion WHERE VersionID = @VersionID AND WorkspaceID = @WorkspaceID)
BEGIN
	BEGIN TRY
		/*Create a Back Up of the current Workspace*/
		DECLARE @VersionName varchar(50),
				@WorkspaceStateID int		
		SELECT	@VersionName = '[SYSTEM: VERSION RESTORE] ' + CONVERT(varchar, GETDATE(), 22),
				@WorkspaceStateID = WorkspaceStateID 
		FROM dbo.Workspace 
		WHERE WorkspaceID = @WorkspaceID
		EXECUTE  [dbo].[createWorkspaceVersion] @VersionName,0,@WorkspaceStateID,@WorkspaceID

		DELETE FROM [dbo].[BOETaskElementMetricDetailXREF]
			FROM [dbo].[BOETaskElementMetricDetailXREF] X
				INNER JOIN [dbo].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
				INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.WBS_CLIN_BOE_XREF 
			FROM [dbo].[WBS_CLIN_BOE_XREF] X
				INNER JOIN dbo.WorkBreakdownStructure WBS 
					ON X.WBSID = WBS.WBSID AND WBS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.WBS_CLIN_BOE_XREF 
			FROM [dbo].[WBS_CLIN_BOE_XREF] X
				INNER JOIN dbo.CLIN C 
					ON X.CLINID = C.CLINID AND C.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.WBS_CLIN_BOE_XREF 
			FROM [dbo].[WBS_CLIN_BOE_XREF] X
				INNER JOIN  dbo.BOE B 
					ON X.BOEID = B.BOEID AND B.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.BOEStateHistory  
			FROM [dbo].[BOEStateHistory] BH
				INNER JOIN dbo.BOE B ON BH.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.SumOfBOE_WorkspaceVariableXREF 
			FROM [dbo].[SumOfBOE_WorkspaceVariableXREF] X
				INNER JOIN [dbo].[WorkspaceVariable] WSV ON X.WorkspaceVariableID = WSV.WorkspaceVariableID
				INNER JOIN dbo.Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.BOETaskElementWorkspaceVariableXREF 
			FROM [dbo].[BOETaskElementWorkspaceVariableXREF] X
				INNER JOIN [dbo].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
				INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.SumOfBOE_OrdinaryVariableXREF 
			FROM [dbo].[SumOfBOE_OrdinaryVariableXREF] X
				INNER JOIN [dbo].[OrdinaryVariable] OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN [dbo].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
				INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.WorkspaceVariableSumVariableResourceTypeXREF
			FROM dbo.WorkspaceVariableSumVariableResourceTypeXREF X
				INNER JOIN [dbo].[WorkspaceVariable] WSV ON X.WorkspaceVariableID = WSV.WorkspaceVariableID
				INNER JOIN dbo.Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID	
		DELETE FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF
			FROM dbo.OrdinaryVariableSumVariableResourceTypeXREF X 
				INNER JOIN [dbo].[OrdinaryVariable] OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
				INNER JOIN [dbo].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
				INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.BOEUserRole 
			FROM [dbo].[BOEUserRole] BUR
				INNER JOIN dbo.BOE B ON BUR.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.BOEUserRoleHistory
			FROM [dbo].[BOEUserRoleHistory] BUR
				INNER JOIN dbo.BOE B ON BUR.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.BOEApprovalHistory 
			FROM [dbo].[BOEApprovalHistory] BH
				INNER JOIN dbo.BOE B ON BH.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.BOEApproval 
			FROM [dbo].[BOEApproval] BA
				INNER JOIN dbo.BOE B ON BA.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.BOECommentHistory 
			FROM [dbo].[BOECommentHistory] BH
				INNER JOIN dbo.BOE B ON BH.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.BOEComment 
			FROM [dbo].[BOEComment] BC
				INNER JOIN dbo.BOE B ON BC.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[BOELaborSpread] 
			FROM [dbo].[BOELaborSpread] LS
				INNER JOIN [dbo].[BOELaborType] BLT ON LS.BOELaborTypeID = BLT.BOELaborTypeID
				INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
				INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.BOELaborTypeCustomFieldValueXREF 
			FROM [dbo].[BOELaborTypeCustomFieldValueXREF] X 
				INNER JOIN [dbo].[BOELaborType] BLT ON X.BOELaborTypeID = BLT.BOELaborTypeID
				INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
				INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.BOELaborType 
			FROM [dbo].[BOELaborType] BLT
				INNER JOIN [dbo].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
				INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[OrdinaryVariable] 
			FROM [dbo].[OrdinaryVariable] OV
				INNER JOIN [dbo].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
				INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[BOEPotentialRole]
			FROM [dbo].[BOEPotentialRole] BR
				INNER JOIN dbo.Workspace WS ON BR.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[WorkspaceUserRole]
			FROM [dbo].[WorkspaceUserRole] WUR
				INNER JOIN dbo.Workspace WS ON WUR.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[WorkspaceVariable]
			FROM [dbo].[WorkspaceVariable] WSV
				INNER JOIN dbo.Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[BOETaskElementCustomFieldValueXREF] 
			FROM [dbo].[BOETaskElementCustomFieldValueXREF] X
				INNER JOIN [dbo].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
				INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[RteTemplateAnswer]
			FROM [dbo].[RteTemplateAnswer] RTA 
				INNER JOIN [RteTemplateQuestion] RTQ ON RTQ.[QuestionID] = RTA.[QuestionID]
				INNER JOIN dbo.[RteTemplate] RT ON RTQ.TemplateID = RT.TemplateID
			WHERE RT.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[RteTemplateQuestion]
			FROM [dbo].[RteTemplateQuestion] RTQ
				INNER JOIN dbo.[RteTemplate] RT ON RTQ.TemplateID = RT.TemplateID
			WHERE RT.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[RteTemplateAssigned]
			FROM [dbo].[RteTemplateAssigned] RTA
				INNER JOIN dbo.[RteTemplate] RT ON RTA.TemplateID = RT.TemplateID
			WHERE RT.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[RteTemplate]
			WHERE WorkspaceID = @WorkspaceID
		DELETE FROM dbo.[MoqTypeTableCustomFieldValueXREF]
			FROM dbo.[MoqTypeTableCustomFieldValueXREF] x
				INNER JOIN MoqTypeSelectionTableData t ON t.MoqTypeSelectionTableDataId = x.MoqTypeTableDataId 
				INNER JOIN MoqTypeSelection mS ON mS.MoqTypeSelectionId = t.MoqTypeSelectionId 
				INNER JOIN BoeTaskElement tE ON tE.BoeTaskElementId = mS.TaskId 
				INNER JOIN dbo.BOE B ON tE.BOEID = B.BOEID
			WHERE B.WorkspaceId = @WorkspaceId
		DELETE FROM [dbo].[MOQTypeSelectionTableDataResourceHours]
			FROM [dbo].[MOQTypeSelectionTableDataResourceHours] M
			INNER JOIN dbo.BOE B ON M.BOEID  = B.BOEID
			WHERE B.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[MOQTypeSelectionTableData]
			FROM [dbo].[MOQTypeSelectionTableData] TD
			INNER JOIN [dbo].[MOQTypeSelection] M ON TD.MOQTypeSelectionId = M.MOQTypeSelectionId
			INNER JOIN [dbo].[BOETaskElement] T on M.TaskId = T.BOETaskElementID
			INNER JOIN dbo.BOE B ON T.BOEID  = B.BOEID
			WHERE B.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[CommonDisclosureSkillMix]
			FROM [dbo].[CommonDisclosureSkillMix] CD
			INNER JOIN dbo.BOE B ON CD.BOEID  = B.BOEID
			WHERE B.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[SkillMix]
			FROM [dbo].[SkillMix] SM
			INNER JOIN dbo.BOE B ON SM.BOEID  = B.BOEID
			WHERE B.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[SkillMixSummary]
			FROM [dbo].[SkillMixSummary] SMS
			INNER JOIN dbo.BOE B ON SMS.BOEID  = B.BOEID
			WHERE B.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[MOQTypeSelection]
			FROM [dbo].[MOQTypeSelection] M
			INNER JOIN [dbo].[BOETaskElement] T on M.TaskId = T.BOETaskElementID
			INNER JOIN dbo.BOE B ON T.BOEID  = B.BOEID
			WHERE B.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[BOETaskElement]
			FROM [dbo].[BOETaskElement] BTE
				INNER JOIN dbo.BOE B ON BTE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].WorkspaceStateHistory	
			FROM [dbo].[WorkspaceStateHistory] WSH
				INNER JOIN dbo.Workspace WS ON WSH.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].BOECommentHistory 
			FROM [dbo].[BOECommentHistory] BCH
				INNER JOIN dbo.BOE B ON BCH.BOEID  = B.BOEID	
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[BOECustomFieldValueXREF] 
			FROM [dbo].[BOECustomFieldValueXREF] X
				INNER JOIN dbo.BOE B ON X.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[TravelTripTaskElementCustomFieldValueXREF] 	
		FROM [dbo].[TravelTripTaskElementCustomFieldValueXREF] X
			INNER JOIN dbo.TravelTripTaskElement TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
			INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[TravelTripCustomFieldValueXREF] 
		  FROM [dbo].[TravelTripCustomFieldValueXREF] X
			INNER JOIN dbo.TravelTrip M ON X.TravelTripID = M.TravelTripID
			INNER JOIN dbo.TravelTripTaskElement TE ON M.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
			INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.ProPricerCustomFieldXREF 
			FROM [dbo].[ProPricerCustomFieldXREF] PX
				INNER JOIN [dbo].[ProPricerExport] PPE ON PX.ProPricerExportID = PPE.ProPricerExportID
				INNER JOIN dbo.Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.ProPricerFieldXREF 
			FROM [dbo].[ProPricerFieldXREF] PX
				INNER JOIN [dbo].[ProPricerExport] PPE ON PX.ProPricerExportID = PPE.ProPricerExportID
				INNER JOIN dbo.Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.ProPricerExport 
			FROM [dbo].[ProPricerExport] PPE
				INNER JOIN dbo.Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.MSTTravelTripCustomFieldValueXREF
			FROM dbo.MSTTravelTripCustomFieldValueXREF cI
			INNER JOIN dbo.MSTTravelTrip tt ON cI.MSTTravelTripID = tt.MSTTravelTripID
			INNER JOIN dbo.TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
			INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.MSTTravelTrip 
		  FROM dbo.MSTTravelTrip tt
			INNER JOIN dbo.TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
			INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[CustomFieldValue] 
			FROM [dbo].[CustomFieldValue] CFV
				INNER JOIN dbo.CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
				INNER JOIN dbo.Workspace WS ON CF.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[CustomField] 
			FROM [dbo].[CustomField] CF
				INNER JOIN dbo.Workspace WS ON CF.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID	
		DELETE FROM [dbo].[WorkspaceOffloadRate]
			WHERE WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[ProjectMapSpread]	
			WHERE WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[ProjectMap]	
			WHERE WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[WorkBreakdownStructure]
			FROM [dbo].[WorkBreakdownStructure] WBS
				INNER JOIN dbo.Workspace WS ON WBS.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.TMResourceRate
			FROM dbo.TMResourceRate TMRR
				INNER JOIN dbo.Workspace WS ON TMRR.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID	
		DELETE FROM [dbo].[CLIN]
			FROM [dbo].[CLIN] C
				INNER JOIN dbo.Workspace WS ON C.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID	
		DELETE FROM [dbo].[ODCSpread] 
			FROM [dbo].[ODCSpread] S
				INNER JOIN [dbo].[ODCType] T ON S.ODCTypeID = T.ODCTypeID
				INNER JOIN [dbo].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.ODCType 
			FROM [dbo].[ODCType] T
				INNER JOIN [dbo].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
				INNER JOIN dbo.BOE B ON TE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[ODCTaskElement] 
			FROM [dbo].[ODCTaskElement] TE
				INNER JOIN dbo.BOE B ON TE.BOEID  = B.BOEID
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[MaterialTaskElement] 
		 FROM [dbo].[MaterialTaskElement] TE
			INNER JOIN dbo.BOE B ON TE.BOEID = B.BOEID
			INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[TravelTrip]
		FROM [dbo].[TravelTrip] TT 
			INNER JOIN [dbo].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
			INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[TravelTripTaskElement]
		FROM [dbo].[TravelTripTaskElement] TE 
			INNER JOIN [dbo].[BOE] B ON TE.BOEID = B.BOEID
			INNER JOIN [dbo].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[BOEFormIBOEResourcesXREF]
			FROM [dbo].[BOEFormIBOEResourcesXREF] x
				INNER JOIN [dbo].[BOEFormIBOE] BF ON x.[IBOEFormID] = BF.[IBOEFormID]
			WHERE 
				BF.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[BOEFormIBOECLINsXREF]
			FROM [dbo].[BOEFormIBOECLINsXREF] x
				INNER JOIN [dbo].[BOEFormIBOE] BF ON x.[IBOEFormID] = BF.[IBOEFormID]
			WHERE 
				BF.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[BOEFormIBOE] WHERE WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[BOEFormPBOEResourcesXREF]
			FROM [dbo].[BOEFormPBOEResourcesXREF] x
				INNER JOIN [dbo].[BOEFormPBOE] BF ON x.[PBOEFormID] = BF.[PBOEFormID]
			WHERE 
				BF.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[BOEFormPBOECLINsXREF]
			FROM [dbo].[BOEFormPBOECLINsXREF] x
				INNER JOIN [dbo].[BOEFormPBOE] BF ON x.[PBOEFormID] = BF.[PBOEFormID]
			WHERE 
				BF.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[BOEFormPBOE] WHERE WorkspaceID = @WorkspaceID
		DELETE FROM dbo.BOE	
			FROM [dbo].[BOE] B
				INNER JOIN dbo.Workspace WS ON B.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID	
		DELETE FROM dbo.OutputFormatTemplateWorkspaceXREF 
			FROM [dbo].[OutputFormatTemplateWorkspaceXREF] OX
				INNER JOIN dbo.Workspace WS ON OX.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.[WorkspaceLockedPerDiem] 
			FROM dbo.[WorkspaceLockedPerDiem] L
				INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.[WorkspaceLockedTravelEscalationRate]
			FROM dbo.[WorkspaceLockedTravelEscalationRate] L 			
				INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.[WorkspaceLockedTravelMiscRate] 
			FROM dbo.[WorkspaceLockedTravelMiscRate] L 			
				INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.[WorkspaceLockedTrip] 
			FROM dbo.[WorkspaceLockedTrip] L
				INNER JOIN dbo.Workspace WS ON L.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM dbo.WorkspaceResource
		 FROM dbo.WorkspaceResource WR
				INNER JOIN dbo.Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[Resource]
			FROM [dbo].[Resource] R
				INNER JOIN [dbo].[ResourceList] RL ON R.ResourceListID = RL.ResourceListID
				INNER JOIN dbo.Workspace WS ON RL.ResourceListID = WS.ResourceListID
		WHERE 
			R.ResourceListID IS NOT NULL AND
			R.ResourceListID <> 1 AND	
			WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[WorkspaceRMSTravelEscalationRate] WHERE WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts] WHERE WorkspaceID = @WorkspaceID

		/*Restore the Workspace*/

		DECLARE @ErrorMessage varchar(8000) 
		IF EXISTS (SELECT vRL.ResourceListID 
							FROM [version].ResourceList vRL 
								INNER JOIN [version].[Workspace] W ON W.ResourceListID = vRL.ResourceListID
								LEFT OUTER JOIN dbo.ResourceList RL ON vRL.ResourceListID = RL.ResourceListID
							WHERE 
								vRL.VersionID = @VersionID AND
								W.VersionID = @VersionID AND
								W.WorkspaceID = @WorkspaceID AND
								RL.ResourceListID IS NULL
						) --IS NOT NULL
				BEGIN				
					SELECT DISTINCT	@ErrorMessage = 
						IsNull(@ErrorMessage,'WARNING: ') + 
						'Resource List ID: ' + 
						CAST (vRL.ResourceListID AS varchar(10)) + 
						' Resource List Name ' + 
						vRL.ResourceListName + 
						' no longer exists.' 
						+ CHAR(13)
					FROM [version].ResourceList vRL 
								INNER JOIN [version].[Workspace] W ON W.ResourceListID = vRL.ResourceListID
								LEFT OUTER JOIN dbo.ResourceList RL ON vRL.ResourceListID = RL.ResourceListID
							WHERE 
								vRL.VersionID = @VersionID AND
								W.VersionID = @VersionID AND
								W.WorkspaceID = @WorkspaceID AND
								RL.ResourceListID IS NULL		
				END

		DELETE FROM dbo.WorkspacePerformingOrganization
		 FROM dbo.WorkspacePerformingOrganization WR
				INNER JOIN dbo.Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
		DELETE FROM [dbo].[PerformingOrganization]
			FROM [dbo].[PerformingOrganization] R
				INNER JOIN [dbo].[PerformingOrganizationList] RL ON R.PerformingOrganizationListID = RL.PerformingOrganizationListID
				INNER JOIN dbo.Workspace WS ON RL.PerformingOrganizationListID = WS.PerformingOrganizationListID
		WHERE 
			R.PerformingOrganizationListID IS NOT NULL AND
			R.PerformingOrganizationListID <> 1 AND	
			WS.WorkspaceID = @WorkspaceID

		IF EXISTS (SELECT vPL.PerformingOrganizationListID 
							FROM [version].PerformingOrganizationList vPL 
								INNER JOIN [version].[Workspace] W ON W.PerformingOrganizationListID = vPL.PerformingOrganizationListID
								LEFT OUTER JOIN dbo.PerformingOrganizationList PL ON vPL.PerformingOrganizationListID = PL.PerformingOrganizationListID
							WHERE 
								vPL.VersionID = @VersionID AND
								W.VersionID = @VersionID AND
								W.WorkspaceID = @WorkspaceID AND
								PL.PerformingOrganizationListID IS NULL
						) --IS NOT NULL
				BEGIN
					
					SELECT	DISTINCT	@ErrorMessage = 
						IsNull(@ErrorMessage,'WARNING: ') + 
						'Performing Organization List ID: ' + 
						CAST (vPL.PerformingOrganizationListID AS varchar(10)) + 
						' Performing Organization List Name: ' + 
						vPL.PerformingOrganizationListName + 
						' no longer exists.' + 
						CHAR(13)
							FROM [version].PerformingOrganizationList vPL 
								INNER JOIN [version].[Workspace] W ON W.PerformingOrganizationListID = vPL.PerformingOrganizationListID
								LEFT OUTER JOIN dbo.PerformingOrganizationList PL ON vPL.PerformingOrganizationListID = PL.PerformingOrganizationListID
							WHERE 
								vPL.VersionID = @VersionID AND
								W.VersionID = @VersionID AND
								W.WorkspaceID = @WorkspaceID AND
								PL.PerformingOrganizationListID IS NULL
				END

		DELETE FROM dbo.[WorkspaceContractTypeXREF] 
			FROM dbo.[WorkspaceContractTypeXREF] X
				INNER JOIN dbo.Workspace WS ON X.WorkspaceID = WS.WorkspaceID
			WHERE WS.WorkspaceID = @WorkspaceID
			
		UPDATE [dbo].[Workspace] 
			SET
			[WorkspaceName] = vW.[WorkspaceName]
			,[WorkspaceShortName] = vW.[WorkspaceShortName]
			,[WorkspaceStateID] =	/*Wireframes:
									If version state is 1 (Initialization) keep at 1
									all others set to 2 (Working)*/  
									CASE vW.[WorkspaceStateID]
										WHEN 1 THEN vW.[WorkspaceStateID]
										ELSE 2
									END
			,[ContractStartDate] = vW.[ContractStartDate]
			,[ContractEndDate] = vW.[ContractEndDate]
			,[ProposalSubmitDate] = vW.[ProposalSubmitDate]
			,[WorkspaceDescription] = vW.[WorkspaceDescription]
			,[CostVolumeLeadPricerUserID] = vW.[CostVolumeLeadPricerUserID]
			,[RFPNumber] = vW.[RFPNumber]
			,[TemplateID] = vW.[TemplateID]
			,[ContainsOCI] = vW.[ContainsOCI]
			,[CreatedByETIUserID] = vW.[CreatedByETIUserID]
			,[AllowSearch] = vW.[AllowSearch]
			,[ResourceListID]=RL.[ResourceListID]
			,[PerformingOrganizationListID]=PL.[PerformingOrganizationListID]
			,[PerformingOrganizationChangeFlag] = vW.[PerformingOrganizationChangeFlag]
			,[TrackingNumber] = vW.[TrackingNumber]
			,[ContainsTemplate] = vW.[ContainsTemplate]
			,[NumProPricerExport] = vW.[NumProPricerExport]
			,[ProposalStatusID] = vW.[ProposalStatusID]
			,[StatusComment] = vW.[StatusComment]
			,[UpdateDT] = vW.[UpdateDT]
			,[IsUsingEquivalentPerson] = vW.[IsUsingEquivalentPerson]
			,[IsUsingTM] = vW.[IsUsingTM]
			,[ProjectMapTypeID] = vW.[ProjectMapTypeID]
			,[AllowGridEdit] = vW.[AllowGridEdit]
			,[CustomSorting] = vW.[CustomSorting]
			,[ResourceSorting] = vw.[ResourceSorting]
			,[PerfOrgSorting] = vw.[PerfOrgSorting]

		/*Remove Code for 7681 	,[LaborPrecisionID] = vW.LaborPrecisionID*/
			/*WI8535*/
			,[BOEExportSortByID] = vW.BOEExportSortByID		
			,[SegmentID] = vW.SegmentID
			,[LineOfBusinessID] = vW.LineOfBusinessID
			,[ProposalClassID] = vW.ProposalClassID
			,[ProposalTitle] = vw.[ProposalTitle]
			,[IsDeleted] = vw.[IsDeleted]
			,[DateDeleted] = vw.DateDeleted
			,[ResourcePrecision] = vw.[ResourcePrecision]
			,[RecalculationStartedDate] = vW.[RecalculationStartedDate]
			,[CostPrecision] = vW.[CostPrecision]
			,[LastProPricerInstance] = vW.[LastProPricerInstance]
			,[LastProPricerProposal] = vW.[LastProPricerProposal]
			,[RteSizeLimit] = vW.[RteSizeLimit]
			,[RevisedSubmittalDate] = vW.[RevisedSubmittalDate]
			,[TemplateBoe] = vW.[TemplateBoe]
			,[EnableSAPConnection] = vW.[EnableSAPConnection]
			,[UCOTFactor] = vW.[UCOTFactor]
			,[EnableAssignTaskAuthor] = vW.[EnableAssignTaskAuthor]
		FROM [dbo].[Workspace] W
			INNER JOIN [version].[Workspace] vW ON W.WorkspaceID = vW.WorkspaceID
			LEFT OUTER JOIN [dbo].[ResourceList] RL ON vW.ResourceListID = RL.ResourceListID
			LEFT OUTER JOIN [dbo].[PerformingOrganizationList] PL ON vW.PerformingOrganizationListID = PL.PerformingOrganizationListID
		WHERE 
			vW.WorkspaceID = @WorkspaceID AND 
			vW.VersionID = @VersionID

		DECLARE @MissingResource TABLE
			(
				ResourceID int,
				ResourceListID int,
				Processed bit
			)
		IF  EXISTS  (SELECT vR.ResourceID 
					FROM [version].[Resource] vR
								INNER JOIN [version].[WorkspaceResource] WR ON vR.ResourceID = WR.SystemResourceID
								INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN dbo.[Resource] R ON vR.ResourceID = R.ResourceID
							WHERE 
								vR.VersionID = @VersionID AND
								WR.VersionID = @VersionID AND
								WS.VersionID = @VersionID AND 
								WS.WorkspaceID = @WorkspaceID AND
								R.ResourceID IS NULL
						) --IS NOT NULL
				BEGIN
					/*Need to Restore the deleted Workspace Resource*/
					INSERT INTO @MissingResource
					SELECT	DISTINCT vR.ResourceID, vR.ResourceListID, 0
					FROM [version].[Resource] vR
							INNER JOIN [version].[WorkspaceResource] WR ON vR.ResourceID = WR.SystemResourceID
							INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
							LEFT OUTER JOIN dbo.[Resource] R ON vR.ResourceID = R.ResourceID
						WHERE 
							vR.VersionID = @VersionID AND
							WR.VersionID = @VersionID AND
							WS.VersionID = @VersionID AND 
							WS.WorkspaceID = @WorkspaceID AND
							R.ResourceID IS NULL			
						
					SET IDENTITY_INSERT [dbo].[Resource] ON
					INSERT INTO [dbo].[Resource]
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
				   )
				SELECT DISTINCT 
					   vR.[ResourceID]
					  ,vR.[UpdateDT]
					  ,vR.[ResourceName]
					  ,vR.[ResourceDescription]
					  ,vR.[SegmentRegion]
					  ,vR.[LaborType]
					  ,vR.[SegmentID]
					  ,vR.[ResourceListID]
					  ,vR.[CostElementID]
					  ,vR.[DeletedFlag]
					  ,vR.[RateTypeID]
					FROM [version].[Resource] vR
							INNER JOIN @MissingResource MR ON vR.ResourceID = MR.ResourceID
							INNER JOIN [version].[WorkspaceResource] WR ON vR.ResourceID = WR.SystemResourceID
							INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
							LEFT OUTER JOIN dbo.[Resource] R ON vR.ResourceID = R.ResourceID
						WHERE 
							vR.VersionID = @VersionID AND
							WR.VersionID = @VersionID AND
							WS.VersionID = @VersionID AND 
							WS.WorkspaceID = @WorkspaceID AND
							R.ResourceID IS NULL AND
							MR.ResourceListID <> 1
					SET IDENTITY_INSERT [dbo].[Resource] OFF					
				END
		IF EXISTS (SELECT 1 FROM [version].[WorkspaceResource] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[WorkspaceResource] ON
		INSERT INTO [dbo].[WorkspaceResource]
				   ([WorkspaceResourceID]
				   ,[SystemResourceID]
				   ,[ResourceListID]
				   ,[WorkspaceID])
		SELECT WR.[WorkspaceResourceID]
			  ,WR.[SystemResourceID]
			  ,WR.[ResourceListID]
			  ,WR.[WorkspaceID]
		  FROM [version].[WorkspaceResource] WR
		INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
		LEFT OUTER JOIN [dbo].[Resource] R ON WR.SystemResourceID = R.ResourceID
		WHERE 
		WR.VersionID = @VersionID AND
		WS.VersionID = @VersionID AND
		WS.WorkspaceID = @WorkspaceID AND
		WR.SystemResourceID NOT IN (SELECT ResourceID FROM @MissingResource WHERE ResourceListID = 1)

		SET IDENTITY_INSERT [dbo].[WorkspaceResource] OFF
		END
		IF EXISTS (SELECT 1 FROM [version].[WorkspaceContractTypeXREF] WHERE VersionID = @VersionID)
		BEGIN

		/*ContractType*/
		SET IDENTITY_INSERT [dbo].[WorkspaceContractTypeXREF] ON
		INSERT INTO [dbo].[WorkspaceContractTypeXREF]
				   ([WorkspaceContractTypeID]
				   ,[UpdateDT]
				   ,[WorkspaceID]
				   ,[ContractTypeID]
				   )
		SELECT WC.[WorkspaceContractTypeID]
			  ,WC.[UpdateDT]
			  ,WC.[WorkspaceID]
			  ,WC.[ContractTypeID]
		FROM [version].[WorkspaceContractTypeXREF] WC
			INNER JOIN [version].Workspace WS ON WC.WorkspaceID = WS.WorkspaceID
		WHERE 
		WC.VersionID = @VersionID AND
		WS.WorkspaceID = @WorkspaceID AND 
		WS.VersionID = @VersionID 

		SET IDENTITY_INSERT [dbo].[WorkspaceContractTypeXREF] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[OutputFormatTemplateVersionXREF] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[OutputFormatTemplate] ON
		INSERT INTO [dbo].[OutputFormatTemplate]
		([TemplateID]
		,[Template]
		,[TemplateDescription]
		,[TemplateFile]
		,[UpdateDT]
		,[IsActive]
		,[ParentTemplateID]
		,[IsAvailableToAllWorkspaces]
		)
		SELECT OFT.[TemplateID]
		,OFT.[Template]
		,OFT.[TemplateDescription]
		,OFT.[TemplateFile]
		,OFT.[UpdateDT]
		,OFT.[IsActive]
		,OFT.[ParentTemplateID]
		,OFT.[IsAvailableToAllWorkspaces]
		FROM [version].[OutputFormatTemplate] OFT
		INNER JOIN [version].OutputFormatTemplateWorkspaceXREF OX ON OFT.TemplateID = OX.TemplateID
		INNER JOIN [version].Workspace WS ON OX.WorkspaceID = WS.WorkspaceID
		INNER JOIN [version].[OutputFormatTemplateVersionXREF] OVX ON OFT.BackupTemplateID = OVX.BackupTemplateID
		/*
		Check the "real" Template table to see if the Template is there and
		only restore if it is not there*/
		LEFT OUTER JOIN [dbo].[OutputFormatTemplate] dbo_OFT ON OFT.TemplateID = dbo_OFT.TemplateID
		WHERE 
		OVX.VersionID = @VersionID AND
		OX.VersionID = @VersionID AND
		WS.WorkspaceID = @WorkspaceID AND 
		WS.VersionID = @VersionID AND
		dbo_OFT.TemplateID IS NULL

		SET IDENTITY_INSERT [dbo].[OutputFormatTemplate] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[OutputFormatTemplateWorkspaceXREF] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[OutputFormatTemplateWorkspaceXREF] ON
		INSERT INTO [dbo].[OutputFormatTemplateWorkspaceXREF]
		([OutputFormatID]
		,[WorkspaceID]
		,[TemplateID]
		)
		SELECT OX.[OutputFormatID]
		,OX.[WorkspaceID]
		,OX.[TemplateID]
		FROM [version].[OutputFormatTemplateWorkspaceXREF] OX
		INNER JOIN [version].Workspace WS ON OX.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		OX.VersionID = @VersionID

		SET IDENTITY_INSERT [dbo].[OutputFormatTemplateWorkspaceXREF] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[BOE] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT  [dbo].[BOE] ON
		INSERT INTO [dbo].[BOE]
		([BOEID]
		,[BOEStateID]
		,[BOEStartDate]
		,[BOEEndDate]
		,[BOEDescription]
		,[DataSource]
		,[WorkspaceID]
		,[MetricDisclosureAcknowledge]
		,[NumAuthorReassigned]
		,[IsMaterial]
		,[UpdateDT]
		,[BOETitle]
		,[IsMultiClinWbs]
		)
		SELECT B.[BOEID]
		/*Wireframes - All BOEs status are changed to 2 (DRAFT) B.[BOEStateID]*/
		/*WI 5989 Change BOE state back to Draft if its current state is Draft,Awaiting Approval or Approved
		Unassigned BOEs should be left as is*/
		,CASE B.[BOEStateID]
			WHEN 0 THEN 0
			WHEN 1 THEN 1
			WHEN 2 THEN 2
			WHEN 3 THEN 2
			WHEN 4 THEN 2
			WHEN 6 THEN 2	--BOEJ-471
		END AS BOEStateID
		,B.[BOEStartDate]
		,B.[BOEEndDate]
		,B.[BOEDescription]
		,B.[DataSource]
		,B.[WorkspaceID]
		,B.[MetricDisclosureAcknowledge]
		,B.[NumAuthorReassigned]
		,B.[IsMaterial]
		,B.[UpdateDT]
		,B.[BOETitle]
		,B.[IsMultiClinWbs]
		FROM [version].[BOE] B
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE 
		B.VersionID = @VersionID AND
		WS.VersionID = @VersionID AND
		WS.WorkspaceID = @WorkspaceID 
		SET IDENTITY_INSERT  [dbo].[BOE] OFF
		END
		IF EXISTS (SELECT 1 FROM [version].[ODCTaskElement] WHERE VersionID = @VersionID)
		BEGIN


		SET IDENTITY_INSERT  [dbo].[ODCTaskElement] ON
		INSERT INTO [dbo].[ODCTaskElement]
		([ODCTaskElementID]
		,[ODCTaskTitle]
		,[ODCTaskDescription]
		,[ODCMOQText]
		,[BOEID]
		,[ODCTaskID]
		,[UpdateDT]
		,[TaskStartDate]
		,[TaskEndDate]
		,[SortOrderID]
		)

		SELECT TE.[ODCTaskElementID]
		,TE.[ODCTaskTitle]
		,TE.[ODCTaskDescription]
		,TE.[ODCMOQText]
		,TE.[BOEID]
		,TE.[ODCTaskID]
		,TE.[UpdateDT]
		,TE.[TaskStartDate]
		,TE.[TaskEndDate]
		,TE.[SortOrderID]
		FROM [version].[ODCTaskElement] TE
		INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		TE.VersionID = @VersionID AND 
		B.VersionID = @VersionID

		SET IDENTITY_INSERT  [dbo].[ODCTaskElement] OFF
		END
		IF EXISTS (SELECT vR.ResourceID 
							FROM [version].[Resource] vR
								INNER JOIN [version].[ODCType] T ON vR.ResourceID = T.ResourceID
								INNER JOIN [version].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
								INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
								INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN dbo.[Resource] R ON vR.ResourceID = R.ResourceID
							WHERE 
								vR.VersionID = @VersionID AND
								T.VersionID = @VersionID AND
								TE.VersionID = @VersionID AND
								B.VersionID = @VersionID AND 
								WS.VersionID = @VersionID AND 
								WS.WorkspaceID = @WorkspaceID AND
								R.ResourceID IS NULL
						) --IS NOT NULL
				BEGIN
					
					SELECT	DISTINCT	@ErrorMessage = 
						IsNull(@ErrorMessage,'WARNING: ') + 
						'ResourceID: ' + 
						CAST (vR.ResourceID AS varchar(10)) + 
						' Resource Name: ' + vR.ResourceName + 
						' no longer exists.' + 
						CHAR(13)
					FROM [version].[Resource] vR
								INNER JOIN [version].[ODCType] T ON vR.ResourceID = T.ResourceID
								INNER JOIN [version].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
								INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
								INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN dbo.[Resource] R ON vR.ResourceID = R.ResourceID
							WHERE 
								vR.VersionID = @VersionID AND
								T.VersionID = @VersionID AND
								TE.VersionID = @VersionID AND
								B.VersionID = @VersionID AND 
								WS.VersionID = @VersionID AND 
								WS.WorkspaceID = @WorkspaceID AND
								R.ResourceID IS NULL			
				END

		DECLARE @MissingPerformingOrganization TABLE
			(
				PerformingOrganizationID int,
				PerformingOrganizationListID int,
				Processed bit
			)
		IF EXISTS (SELECT vR.PerformingOrganizationID 
					FROM [version].[PerformingOrganization] vR
								INNER JOIN [version].[WorkspacePerformingOrganization] WR ON vR.PerformingOrganizationID = WR.SystemPerformingOrganizationID
								INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN dbo.[PerformingOrganization] R ON vR.PerformingOrganizationID = R.PerformingOrganizationID
							WHERE 
								vR.VersionID = @VersionID AND
								WR.VersionID = @VersionID AND
								WS.VersionID = @VersionID AND 
								WS.WorkspaceID = @WorkspaceID AND
								R.PerformingOrganizationID IS NULL
						) --IS NOT NULL
				BEGIN
					/*Need to Restore the deleted Workspace PerformingOrganization*/
					INSERT INTO @MissingPerformingOrganization
					SELECT	DISTINCT vR.PerformingOrganizationID, vR.PerformingOrganizationListID, 0
					FROM [version].[PerformingOrganization] vR
							INNER JOIN [version].[WorkspacePerformingOrganization] WR ON vR.PerformingOrganizationID = WR.SystemPerformingOrganizationID
							INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
							LEFT OUTER JOIN dbo.[PerformingOrganization] R ON vR.PerformingOrganizationID = R.PerformingOrganizationID
						WHERE 
							vR.VersionID = @VersionID AND
							WR.VersionID = @VersionID AND
							WS.VersionID = @VersionID AND 
							WS.WorkspaceID = @WorkspaceID AND
							R.PerformingOrganizationID IS NULL			
						
					SET IDENTITY_INSERT [dbo].[PerformingOrganization] ON
					INSERT INTO [dbo].[PerformingOrganization]
				   ([PerformingOrganizationID]
				   ,[UpdateDT]
				   ,[PerformingOrganizationName]
				   ,[PerformingOrganizationDescription]
				   ,[PerformingOrganizationListID]
				   ,[DeletedFlag])
				SELECT DISTINCT 
					   vR.[PerformingOrganizationID]
					  ,vR.[UpdateDT]
					  ,vR.[PerformingOrganizationName]
					  ,vR.[PerformingOrganizationDescription]
					  ,vR.[PerformingOrganizationListID]
					  ,vR.[DeletedFlag]
					FROM [version].[PerformingOrganization] vR
							INNER JOIN @MissingPerformingOrganization MR ON vR.PerformingOrganizationID = MR.PerformingOrganizationID
							INNER JOIN [version].[WorkspacePerformingOrganization] WR ON vR.PerformingOrganizationID = WR.SystemPerformingOrganizationID
							INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
							LEFT OUTER JOIN dbo.[PerformingOrganization] R ON vR.PerformingOrganizationID = R.PerformingOrganizationID
						WHERE 
							vR.VersionID = @VersionID AND
							WR.VersionID = @VersionID AND
							WS.VersionID = @VersionID AND 
							WS.WorkspaceID = @WorkspaceID AND
							R.PerformingOrganizationID IS NULL AND
							MR.PerformingOrganizationListID <> 1
					SET IDENTITY_INSERT [dbo].[PerformingOrganization] OFF					
				END
		IF EXISTS (SELECT 1 FROM [version].[WorkspacePerformingOrganization] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[WorkspacePerformingOrganization] ON
		INSERT INTO [dbo].[WorkspacePerformingOrganization]
				   ([WorkspacePerformingOrganizationID]
				   ,[SystemPerformingOrganizationID]
				   ,[PerformingOrganizationListID]
				   ,[WorkspaceID])
		SELECT WR.[WorkspacePerformingOrganizationID]
			  ,WR.[SystemPerformingOrganizationID]
			  ,WR.[PerformingOrganizationListID]
			  ,WR.[WorkspaceID]
		  FROM [version].[WorkspacePerformingOrganization] WR
		INNER JOIN [version].Workspace WS ON WR.WorkspaceID = WS.WorkspaceID
		LEFT OUTER JOIN [dbo].[PerformingOrganization] R ON WR.SystemPerformingOrganizationID = R.PerformingOrganizationID
		WHERE 
		WR.VersionID = @VersionID AND
		WS.VersionID = @VersionID AND
		WS.WorkspaceID = @WorkspaceID AND
		WR.SystemPerformingOrganizationID NOT IN (SELECT PerformingOrganizationID FROM @MissingPerformingOrganization WHERE PerformingOrganizationListID = 1)


		SET IDENTITY_INSERT [dbo].[WorkspacePerformingOrganization] OFF

		END
		IF EXISTS (SELECT vPO.PerformingOrganizationID 
							FROM [version].[PerformingOrganization] vPO
								INNER JOIN [version].[ODCType] T ON vPO.PerformingOrganizationID = T.PerformingOrganizationID
								INNER JOIN [version].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
								INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
								INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN dbo.[PerformingOrganization] PO ON vPO.PerformingOrganizationID = PO.PerformingOrganizationID
							WHERE 
								vPO.VersionID = @VersionID AND
								T.VersionID = @VersionID AND
								TE.VersionID = @VersionID AND
								B.VersionID = @VersionID AND 
								WS.VersionID = @VersionID AND 
								WS.WorkspaceID = @WorkspaceID AND
								PO.PerformingOrganizationID IS NULL
						) --IS NOT NULL
				BEGIN
				
					SELECT	DISTINCT	@ErrorMessage = 
						IsNull(@ErrorMessage,'WARNING: ') + 
						'PerformingOrganizationID: ' + 
						CAST (vPO.PerformingOrganizationID AS varchar(10)) + 
						' Performing Organization Name: ' + 
						vPO.PerformingOrganizationName + 
						' no longer exists.' + 
						CHAR(13)
					FROM [version].[PerformingOrganization] vPO
								INNER JOIN [version].[ODCType] T ON vPO.PerformingOrganizationID = T.PerformingOrganizationID
								INNER JOIN [version].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
								INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
								INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN  dbo.[PerformingOrganization] PO ON vPO.PerformingOrganizationID = PO.PerformingOrganizationID
							WHERE 
								vPO.VersionID = @VersionID AND
								T.VersionID = @VersionID AND
								TE.VersionID = @VersionID AND
								B.VersionID = @VersionID AND 
								WS.VersionID = @VersionID AND 
								WS.WorkspaceID = @WorkspaceID AND
								PO.PerformingOrganizationID IS NULL

				END
		IF EXISTS (SELECT 1 FROM [version].[ODCType] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[ODCType] ON
		INSERT INTO [dbo].[ODCType]
		([ODCTypeID]
		,[ResourceID]
		,[PerformingOrganizationID]
		,[ODCTypeStartDate]
		,[ODCTypeEndDate]
		,[SpreadCurveID]
		,[ODCTypeCost]
		,[ODCTaskElementID]
		,[UpdateDT]
		)
		SELECT T.[ODCTypeID]
		,R.[ResourceID]
		,PO.[PerformingOrganizationID]
		,T.[ODCTypeStartDate]
		,T.[ODCTypeEndDate]
		,T.[SpreadCurveID]
		,T.[ODCTypeCost]
		,T.[ODCTaskElementID]
		,T.[UpdateDT]
		FROM [version].[ODCType] T
		INNER JOIN [version].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
		INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		LEFT OUTER JOIN [dbo].[Resource] R ON T.ResourceID = R.ResourceID
		LEFT OUTER JOIN [dbo].[PerformingOrganization] PO ON T.PerformingOrganizationID = PO.PerformingOrganizationID
		WHERE 
		T.VersionID = @VersionID AND
		TE.VersionID = @VersionID AND
		B.VersionID = @VersionID AND 
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID


		SET IDENTITY_INSERT [dbo].[ODCType] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[ODCSpread] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[ODCSpread] ON
		INSERT INTO [dbo].[ODCSpread]
		([ODCSpreadID]
		,[ODCTypeID]
		,[ODCSpreadDate]
		,[ODCSpreadValue]
		)
		SELECT S.[ODCSpreadID]
		,S.[ODCTypeID]
		,S.[ODCSpreadDate]
		,S.[ODCSpreadValue]
		FROM [version].[ODCSpread] S
		INNER JOIN [version].[ODCType] T ON S.ODCTypeID = T.ODCTypeID
		INNER JOIN [version].[ODCTaskElement] TE ON T.ODCTaskElementID = TE.ODCTaskElementID
		INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID AND S.VersionID = @VersionID AND
		T.VersionID = @VersionID AND
		TE.VersionID = @VersionID AND
		B.VersionID = @VersionID

		SET IDENTITY_INSERT [dbo].[ODCSpread] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[MaterialTaskElement] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT  [dbo].[MaterialTaskElement] ON
		INSERT INTO [dbo].[MaterialTaskElement]
				   ([MaterialTaskElementID]
				   ,[MaterialTaskID]
				   ,[MaterialTaskTitle]
				   ,[MaterialTaskDescription]
				   ,[MaterialMOQText]
				   ,[BOEID]
				   ,[UpdateDT])
		SELECT TE.[MaterialTaskElementID]
			  ,TE.[MaterialTaskID]
			  ,TE.[MaterialTaskTitle]
			  ,TE.[MaterialTaskDescription]
			  ,TE.[MaterialMOQText]
			  ,TE.[BOEID]
			  ,TE.[UpdateDT]
		FROM [version].[MaterialTaskElement] TE
		INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		TE.VersionID = @VersionID AND 
		B.VersionID = @VersionID

		SET IDENTITY_INSERT  [dbo].[MaterialTaskElement] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[TravelTripTaskElement] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT dbo.TravelTripTaskElement ON

		INSERT INTO [dbo].[TravelTripTaskElement]
				   ([TravelTripTaskElementID]
				   ,[UpdateDT]
				   ,[TravelTaskID]
				   ,[TravelTaskTitle]
				   ,[TravelTaskDescription]
				   ,[BOEID]
				   ,[TaskStartDate]
				   ,[TaskEndDate]
				   ,[SortOrderID]
				   )
		SELECT TE.[TravelTripTaskElementID]
			  ,TE.[UpdateDT]
			  ,TE.[TravelTaskID]
			  ,TE.[TravelTaskTitle]
			  ,TE.[TravelTaskDescription]
			  ,TE.[BOEID]
			  ,TE.[TaskStartDate]
			  ,TE.[TaskEndDate]
			  ,TE.[SortOrderID]
		FROM [version].[TravelTripTaskElement] TE 
			INNER JOIN [version].[BOE] B ON TE.BOEID = B.BOEID
			INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE
			TE.VersionID = @VersionID AND
			B.VersionID = @VersionID AND
			WS.VersionID = @VersionID AND
			WS.WorkspaceID = @WorkspaceID

		SET IDENTITY_INSERT dbo.TravelTripTaskElement OFF

		END
		IF EXISTS (SELECT vT.TripID
							FROM [version].[Trip] vT 
								INNER JOIN [version].[TravelTrip] TT ON vT.TripID = TT.TripID
								INNER JOIN [version].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
								INNER JOIN [version].[BOE] B ON TE.BOEID = B.BOEID
								INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN [dbo].[Trip] T ON vT.TripID = T.TripID 
			/*WI8256					LEFT OUTER JOIN [dbo].[Trip] OrigTrip ON TT.OriginatingTripID = OrigTrip.TripID*/
							WHERE 
								vT.VersionID = @VersionID AND
								TT.VersionID = @VersionID AND
								TE.VersionID = @VersionID AND
								B.VersionID = @VersionID AND
								WS.VersionID = @VersionID AND
								WS.WorkspaceID = @WorkspaceID AND
								(
									T.TripID IS NULL/*WI8256 AND
									OrigTrip.TripID IS NULL*/
								)
					
						) --IS NOT NULL
				BEGIN

					SELECT	DISTINCT	@ErrorMessage = 
						IsNull(@ErrorMessage,'WARNING: ') + 
						'TripID: ' + 
						CAST (vT.TripID AS varchar(10)) + 
 						CHAR(13)  + 
 				
						' Departure: ' + 
						CAST (Departure.LocationName AS varchar(10)) + 
 						CHAR(13)  + 
 				
						' Destination: ' + 
						CAST (Destination.LocationName AS varchar(10)) + 
 						CHAR(13) +
 													
						' no longer exists.' + 
						CHAR(13)
					FROM [version].[Trip] vT 
								INNER JOIN [version].[TravelTrip] TT ON vT.TripID = TT.TripID
								INNER JOIN [version].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
								INNER JOIN [version].[BOE] B ON TE.BOEID = B.BOEID
								INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
								INNER JOIN [dbo].[Location] Departure ON vt.DepartureLocationID = Departure.LocationID
								INNER JOIN [dbo].[Location] Destination ON vt.DestinationLocationID = Destination.LocationID
								LEFT OUTER JOIN [dbo].[Trip] T ON T.TripID = vT.TripID 						
							WHERE 
								vT.VersionID = @VersionID AND
								TT.VersionID = @VersionID AND
								TE.VersionID = @VersionID AND
								B.VersionID = @VersionID AND
								WS.VersionID = @VersionID AND
								WS.WorkspaceID = @WorkspaceID AND 
								T.TripID IS NULL


				END
		IF EXISTS (SELECT vPO.PerformingOrganizationID 
					FROM [version].[PerformingOrganization] vPO 
								INNER JOIN [version].[TravelTrip] TT ON vPO.PerformingOrganizationID = TT.PerformingOrganizationID
								INNER JOIN [version].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
								INNER JOIN [version].[BOE] B ON TE.BOEID = B.BOEID
								INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN  dbo.[PerformingOrganization] PO ON vPO.PerformingOrganizationID = PO.PerformingOrganizationID
							WHERE 
								vPO.VersionID = @VersionID AND
								TT.VersionID = @VersionID AND
								TE.VersionID = @VersionID AND
								B.VersionID = @VersionID AND
								WS.VersionID = @VersionID AND
								WS.WorkspaceID = @WorkspaceID AND
								PO.PerformingOrganizationID IS NULL
						) --IS NOT NULL
				BEGIN
				
					SELECT	DISTINCT	@ErrorMessage = 
						IsNull(@ErrorMessage,'WARNING: ') + 
						'PerformingOrganizationID: ' + 
						CAST (vPO.PerformingOrganizationID AS varchar(10)) + 
						' Performing Organization Name: ' + 
						vPO.PerformingOrganizationName + 
						' no longer exists.' + 
						CHAR(13)
					FROM [version].[PerformingOrganization] vPO 
								INNER JOIN [version].[TravelTrip] TT ON vPO.PerformingOrganizationID = TT.PerformingOrganizationID
								INNER JOIN [version].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
								INNER JOIN [version].[BOE] B ON TE.BOEID = B.BOEID
								INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN  dbo.[PerformingOrganization] PO ON vPO.PerformingOrganizationID = PO.PerformingOrganizationID
							WHERE 
								vPO.VersionID = @VersionID AND
								TT.VersionID = @VersionID AND
								TE.VersionID = @VersionID AND
								B.VersionID = @VersionID AND
								WS.VersionID = @VersionID AND
								WS.WorkspaceID = @WorkspaceID AND
								PO.PerformingOrganizationID IS NULL

				END
		IF EXISTS (SELECT 1 FROM [version].[TravelTrip] WHERE VersionID = @VersionID)
		BEGIN
		/*Bug 8146*/
		SET IDENTITY_INSERT dbo.TravelTrip ON

		INSERT INTO [dbo].[TravelTrip]
				   ([TravelTripID]
				   ,[UpdateDT]
				   ,[GroupID]
				   ,[SegmentID]
				   ,[PerformingOrganizationID]
				   ,[TripID]
				   ,[TripDate]
				   ,[NumTrips]
				   ,[NumPeople]
				   ,[NumDays]
				   ,[Purpose]
				   ,[TravelTripTaskElementID]
				   ,[TripLockedDT]
		/*WI8256     ,[OriginatingTripID]*/
					)
		SELECT TT.[TravelTripID]
			  ,TT.[UpdateDT]
			  ,TT.[GroupID]
			  ,TT.[SegmentID]
			  ,PO.[PerformingOrganizationID]
		/*WI8256      ,
			  /*BUG 8146*/
				CASE	
					WHEN T.[TripID] IS NULL THEN OT.TripID	
					ELSE*/
				, T.[TripID]
		/*WI8256		END AS TripID			*/
			
			  ,TT.[TripDate]
			  ,TT.[NumTrips]
			  ,TT.[NumPeople]
			  ,TT.[NumDays]
			  ,TT.[Purpose]
			  ,TT.[TravelTripTaskElementID]
			  ,TT.[TripLockedDT]
			  /*BUG 8146*/
		/*WI8256		, CASE	
					WHEN T.[TripID] IS NULL THEN NULL
					ELSE TT.[OriginatingTripID]
				  END AS [OriginatingTripID]*/
		FROM [version].[TravelTrip] TT 
			INNER JOIN [version].[TravelTripTaskElement] TE ON TT.TravelTripTaskElementID = TE.TravelTripTaskElementID
			INNER JOIN [version].[BOE] B ON TE.BOEID = B.BOEID
			INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
			/*INNER JOIN TO CONFIRM ALL DATA IS AVAILABLE*/
			LEFT OUTER JOIN [dbo].[PerformingOrganization] PO ON TT.PerformingOrganizationID = PO.PerformingOrganizationID

			LEFT OUTER JOIN [dbo].[Trip] T ON TT.TripID = T.TripID
		/*WI8256	LEFT OUTER JOIN [dbo].[Trip] OT ON TT.OriginatingTripID = OT.TripID	*/
	
	
		WHERE
			TT.VersionID = @VersionID AND
			TE.VersionID = @VersionID AND
			B.VersionID = @VersionID AND
			WS.VersionID = @VersionID AND
			WS.WorkspaceID = @WorkspaceID
	
	
		SET IDENTITY_INSERT dbo.TravelTrip OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[WorkspaceOffloadRate] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[WorkspaceOffloadRate] ON
		INSERT INTO [dbo].[WorkspaceOffloadRate]
		([OffloadRateID]
			,[UpdateDT]
			,[WorkspaceID]
			,[Resource]
			,[PerfOrg]
			,[PercentToOffload]
			,[Year]
			,[SubcontractorResource]
			,[HourlyRate]
		)
		SELECT WOR.[OffloadRateID]
			,WOR.[UpdateDT]
			,WOR.[WorkspaceID]
			,WOR.[Resource]
			,WOR.[PerfOrg]
			,WOR.[PercentToOffload]
			,WOR.[Year]
			,WOR.[SubcontractorResource]
			,WOR.[HourlyRate]
		FROM [version].[WorkspaceOffloadRate] WOR
		WHERE 
		WOR.VersionID = @VersionID AND
		WOR.WorkspaceID = @WorkspaceID
		SET IDENTITY_INSERT [dbo].[WorkspaceOffloadRate] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[ProjectMap] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[ProjectMap] ON
		INSERT INTO [dbo].[ProjectMap]
		([ID]
			,[WorkspaceId]
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
		)
		SELECT PM.[ID]
			,PM.[WorkspaceId]
			,PM.[WbsNumber]
			,PM.[WbsElementTitle]
			,PM.[ActivityID]
			,PM.[ActivityName]
			,PM.[Resource]
			,PM.[CostCenter] 
			,PM.[StartDate] 
			,PM.[EndDate] 
			,PM.[CLIN] 
			,PM.[Task] 
			,PM.[SOW] 
			,PM.[SOWTitle] 
			,PM.[Rationale]
			,PM.[CamName] 
			,PM.[Category] 
			,PM.[Hours]
			,PM.[Dollars] 
			,PM.[CanOffload]
			,PM.[AddOrDelete] 
			,PM.[ClassOfCost] 
			,PM.[OrderID]
			,PM.[TieredPercentage]
			,PM.[LegacyResourceID]
		FROM [version].[ProjectMap] PM
		WHERE 
		PM.VersionID = @VersionID AND
		PM.WorkspaceID = @WorkspaceID
		SET IDENTITY_INSERT [dbo].[ProjectMap] OFF

		INSERT INTO [dbo].[ProjectMapSpread]
				   ([WorkspaceId]
				   ,[ProjectMapId]
				   ,[SpreadDate]
				   ,[SpreadValue]
				   )
		SELECT 	    S.[WorkspaceId],
					S.[ProjectMapId],
					S.[SpreadDate],
					S.[SpreadValue]
		FROM  [version].ProjectMapSpread S
		WHERE 
		S.VersionID = @VersionID AND 
		S.[WorkspaceId] = @WorkspaceID

		END
		IF EXISTS (SELECT 1 FROM [version].[CustomField] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[CustomField] ON
		INSERT INTO [dbo].[CustomField]
		([CustomFieldID]
		,[CustomFieldName]
		,[CustomFieldRequired]
		,[CustomFieldDisplayID]
		,[WorkspaceID]
		,[UpdateDT]
		,[IsOpenEnded]
		)
		SELECT CF.[CustomFieldID]
		,CF.[CustomFieldName]
		,CF.[CustomFieldRequired]
		,CF.[CustomFieldDisplayID]
		,CF.[WorkspaceID]
		,CF.[UpdateDT]
		,CF.[IsOpenEnded]
		FROM [version].[CustomField] CF
		INNER JOIN [version].Workspace WS ON CF.WorkspaceID = WS.WorkspaceID
		WHERE 
		WS.VersionID = @VersionID AND
		WS.WorkspaceID = @WorkspaceID AND 
		CF.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[CustomField] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[CLIN] WHERE VersionID = @VersionID)
		BEGIN


		SET IDENTITY_INSERT [dbo].[CLIN] ON
		INSERT INTO [dbo].[CLIN]
		([CLINID]
		,[CLINNumber]
		,[CLINTitle]
		,[CLINStartDate]
		,[CLINEndDate]
		,[ContractTypeID]
		,[WorkspaceID]
		,[UpdateDT]
		,[DisplayedCLINNumber]
		)
		SELECT C.[CLINID]
		,C.[CLINNumber]
		,C.[CLINTitle]
		,C.[CLINStartDate]
		,C.[CLINEndDate]
		,C.[ContractTypeID]
		,C.[WorkspaceID]
		,C.[UpdateDT]
		,C.[DisplayedCLINNumber]
		FROM [version].[CLIN] C
		INNER JOIN [version].Workspace WS ON C.WorkspaceID = WS.WorkspaceID
		WHERE WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID	 AND C.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[CLIN] OFF


		END
		IF EXISTS (SELECT 1 FROM [version].[BOEPotentialRole] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[BOEPotentialRole] ON
		INSERT INTO [dbo].[BOEPotentialRole]
		([BOEPotentialRoleID]
		,[ETIUserID]
		/*,[ETIGroupID]*/
		,[WorkspaceID]
		,[RoleID]
		,[UserRemoved]
		,[UpdateDT]
		)
		SELECT BR.[BOEPotentialRoleID]
		,BR.[ETIUserID]
		/*,BR.[ETIGroupID]*/
		,BR.[WorkspaceID]
		,BR.[RoleID]
		,BR.[UserRemoved]
		,BR.[UpdateDT]
		FROM [version].[BOEPotentialRole] BR
		INNER JOIN [version].Workspace WS ON BR.WorkspaceID = WS.WorkspaceID
		WHERE WS.VersionID = @VersionID AND  WS.WorkspaceID = @WorkspaceID	 AND BR.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[BOEPotentialRole] OFF
		END
		IF EXISTS (SELECT 1 FROM [version].[WorkBreakdownStructure] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[WorkBreakdownStructure] ON
		INSERT INTO [dbo].[WorkBreakdownStructure]
		(			[WBSID]
				   ,[UpdateDT]
				   ,[WBSNumber]
				   ,[DisplayedWBSNumber]
				   ,[WBSTitle]
				   ,[WorkspaceID])
		SELECT 
		WBS.[WBSID]
		,WBS.[UpdateDT]
		,WBS.[WBSNumber]
		,WBS.[DisplayedWBSNumber]
		,WBS.[WBSTitle]
		,WBS.[WorkspaceID]
		FROM [version].[WorkBreakdownStructure] WBS
		INNER JOIN [version].Workspace WS ON WBS.WorkspaceID = WS.WorkspaceID
		WHERE  WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID	 AND WBS.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[WorkBreakdownStructure] OFF
		END
		IF EXISTS (SELECT vTMRR.[TMResourceID] 
				FROM [version].[TMResourceRate] vTMRR
					INNER JOIN [version].[Workspace] WS ON vTMRR.WorkspaceID = WS.WorkspaceID			
					LEFT OUTER JOIN [dbo].[Resource] R ON vTMRR.[TMResourceID] = R.ResourceID
				WHERE 
					vTMRR.VersionID = @VersionID AND
					WS.VersionID = @VersionID AND 
					WS.WorkspaceID = @WorkspaceID AND
					R.ResourceID IS NULL
				)
				BEGIN
		
				SELECT 	DISTINCT	@ErrorMessage = 
						IsNull(@ErrorMessage,'WARNING: ') + 
						ErrorMessage.ErrorMessage
				FROM 
					(	SELECT 
						'ResourceID: ' + 
						CAST (vR.ResourceID AS varchar(10)) + 
						' Resource Name: ' + 
						vR.ResourceName + 
						' no longer exists.' + 
						CHAR(13) AS ErrorMessage
				FROM [version].[TMResourceRate] vTMRR
					INNER JOIN [version].[Resource] vR ON vTMRR.[TMResourceID] = vR.ResourceID
					INNER JOIN [version].[Workspace] WS ON vTMRR.WorkspaceID = WS.WorkspaceID			
					LEFT OUTER JOIN [dbo].[Resource] R ON vTMRR.[TMResourceID] =  R.ResourceID
				WHERE 
					vTMRR.VersionID = @VersionID AND
					WS.VersionID = @VersionID AND 
					WS.WorkspaceID = @WorkspaceID AND
					R.ResourceID IS NULL AND
					vR.VersionID = @VersionID 
				) ErrorMessage				
					
				END
		IF EXISTS (SELECT 1 FROM [version].[TMResourceRate] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[TMResourceRate] ON

		INSERT INTO [dbo].[TMResourceRate]
				   ([TMResourceRateID]
				   ,[UpdateDT]
				   ,[WorkspaceID]
				   ,[TMResourceID]
				   ,[TMResourceRateStartDate]
				   ,[TMResourceRateEndDate]
				   ,[TMResourceRate])
		SELECT TMRR.[TMResourceRateID]
			  ,TMRR.[UpdateDT]
			  ,TMRR.[WorkspaceID]
			  ,TMRR.[TMResourceID]
			  ,TMRR.[TMResourceRateStartDate]
			  ,TMRR.[TMResourceRateEndDate]
			  ,TMRR.[TMResourceRate]
		FROM [version].[TMResourceRate] TMRR
			INNER JOIN [version].Workspace WS ON TMRR.WorkspaceID = WS.WorkspaceID
			LEFT OUTER JOIN [dbo].[Resource] R ON TMRR.TMResourceID = R.ResourceID
		WHERE 
		TMRR.VersionID = @VersionID AND
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID

		SET IDENTITY_INSERT [dbo].[TMResourceRate] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[WorkspaceStateHistory] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[WorkspaceStateHistory] ON
		INSERT INTO [dbo].[WorkspaceStateHistory]
		([WorkspaceStateHistoryID]
		,[WorkspaceID]
		,[CurrentWorkspaceStateID]
		,[UpdatedWorkspaceStateID]
		,[ChangedByETIUserID]
		,[UpdateDT]
		)
		SELECT WSH.[WorkspaceStateHistoryID]
		,WSH.[WorkspaceID]
		,WSH.[CurrentWorkspaceStateID]
		,WSH.[UpdatedWorkspaceStateID]
		,WSH.[ChangedByETIUserID]
		,WSH.[UpdateDT]
		FROM [version].[WorkspaceStateHistory] WSH
		INNER JOIN [version].Workspace WS ON WSH.WorkspaceID = WS.WorkspaceID
		WHERE 
		WS.VersionID = @VersionID AND  
		WS.WorkspaceID = @WorkspaceID AND 
		WSH.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[WorkspaceStateHistory] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[WorkspaceVariable] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[WorkspaceVariable] ON
		INSERT INTO [dbo].[WorkspaceVariable]
		([WorkspaceVariableID]
		,[WorkspaceVariableName]
		,[WorkspaceVariableValue]
		,[WorkspaceID]
		,[SortByID]
		,[ValueTypeID]
		,[IsPercentage]
		,[UpdateDT]
		)
		SELECT WSV.[WorkspaceVariableID]
		,WSV.[WorkspaceVariableName]
		,WSV.[WorkspaceVariableValue]
		,WSV.[WorkspaceID]
		,WSV.[SortByID]
		,WSV.[ValueTypeID]
		,WSV.[IsPercentage]
		,WSV.[UpdateDT]
		FROM [version].[WorkspaceVariable] WSV
		INNER JOIN [version].Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		WSV.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[WorkspaceVariable] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[WorkspaceUserRole] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[WorkspaceUserRole] ON
		INSERT INTO [dbo].[WorkspaceUserRole]
		([WorkspaceUserRoleID]
		,[ETIUserID]
		/*,[ETIGroupID]*/
		,[RoleID]
		,[WorkspaceID]
		,[HideHelp]
		,[UpdateDT]
		)
		SELECT WUR.[WorkspaceUserRoleID]
		,WUR.[ETIUserID]
		/*,WUR.[ETIGroupID]*/
		,WUR.[RoleID]
		,WUR.[WorkspaceID]
		,WUR.[HideHelp]
		,WUR.[UpdateDT]
		FROM [version].[WorkspaceUserRole] WUR
		INNER JOIN [version].Workspace WS ON WUR.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		WUR.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[WorkspaceUserRole] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[ProPricerExport] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT  [dbo].[ProPricerExport] ON
		INSERT INTO [dbo].[ProPricerExport]
		([ProPricerExportID]
		,[ProPricerExportName]
		,[WorkspaceID]
		,[UpdateDT]
		)
		SELECT PPE.[ProPricerExportID]
		,PPE.[ProPricerExportName]
		,PPE.[WorkspaceID]
		,PPE.[UpdateDT]
		FROM [version].[ProPricerExport] PPE
		INNER JOIN [version].Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		PPE.VersionID = @VersionID
		SET IDENTITY_INSERT  [dbo].[ProPricerExport] OFF


		END
		IF EXISTS (SELECT 1 FROM [version].[ProPricerFieldXREF] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT  [dbo].[ProPricerFieldXREF] ON
		INSERT INTO [dbo].[ProPricerFieldXREF]
		([PFID]
		,[ProPricerExportID]
		,[ProPricerFieldID]
		,[ProPricerTypeID]
		,[ListOrder]
		)
		SELECT PX.[PFID]
		,PX.[ProPricerExportID]
		,PX.[ProPricerFieldID]
		,PX.[ProPricerTypeID]
		,PX.[ListOrder]
		FROM [version].[ProPricerFieldXREF] PX
		INNER JOIN [version].[ProPricerExport] PPE ON PX.ProPricerExportID = PPE.ProPricerExportID
		INNER JOIN [version].Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		PX.VersionID = @VersionID AND
		PPE.VersionID = @VersionID
		SET IDENTITY_INSERT  [dbo].[ProPricerFieldXREF] OFF
		END
		IF EXISTS (SELECT 1 FROM [version].[ProPricerCustomFieldXREF] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[ProPricerCustomFieldXREF] ON
		INSERT INTO [dbo].[ProPricerCustomFieldXREF]
		([PCID]
		,[ProPricerExportID]
		,[CustomFieldID]
		,[ProPricerTypeID]
		,[ProPricerCustomFieldSelectionID]
		,[ListOrder]
		)
		SELECT PX.[PCID]
		,PX.[ProPricerExportID]
		,PX.[CustomFieldID]
		,PX.[ProPricerTypeID]
		,PX.[ProPricerCustomFieldSelectionID]
		,PX.[ListOrder]
		FROM [version].[ProPricerCustomFieldXREF] PX
		INNER JOIN [version].[ProPricerExport] PPE ON PX.ProPricerExportID = PPE.ProPricerExportID
		INNER JOIN [version].Workspace WS ON PPE.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		PX.VersionID = @VersionID AND
		PPE.VersionID = @VersionID
		SET IDENTITY_INSERT  [dbo].[ProPricerCustomFieldXREF] OFF
		END
		IF EXISTS (SELECT 1 FROM [version].[WBS_CLIN_BOE_XREF] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[WBS_CLIN_BOE_XREF] ON
		INSERT INTO [dbo].[WBS_CLIN_BOE_XREF]
		([WCBID]
		,[WBSID]
		,[CLINID]
		,[BOEID]
		)
		SELECT X.[WCBID], X.[WBSID], X.[CLINID], X.[BOEID]
		FROM [version].[WBS_CLIN_BOE_XREF] X 
		INNER JOIN [version].WorkBreakdownStructure WBS ON X.WBSID = WBS.WBSID
		INNER JOIN [version].Workspace WS ON WBS.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND
		WBS.VersionID = @VersionID AND
		X.VersionID = @VersionID
		UNION
		SELECT X.[WCBID], X.[WBSID], X.[CLINID], X.[BOEID]
		FROM [version].[WBS_CLIN_BOE_XREF] X 
		INNER JOIN [version].CLIN C ON X.CLINID = C.CLINID 
		INNER JOIN [version].Workspace WS ON C.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND
		C.VersionID = @VersionID AND
		X.VersionID = @VersionID
		UNION
		SELECT X.[WCBID], X.[WBSID], X.[CLINID], X.[BOEID]
		FROM [version].[WBS_CLIN_BOE_XREF] X 
		INNER JOIN [version].BOE B ON X.BOEID = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND
		B.VersionID = @VersionID AND
		X.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[WBS_CLIN_BOE_XREF] OFF
		END
		IF EXISTS (SELECT 1 FROM [version].[SumOfBOE_WorkspaceVariableXREF] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[SumOfBOE_WorkspaceVariableXREF] ON
		INSERT INTO [dbo].[SumOfBOE_WorkspaceVariableXREF]
		([WVSumID]
		,[WorkspaceVariableID]
		,[CLINID]
		,[WBSID]
		,[BOEID]
		)
		SELECT X.[WVSumID]
		,X.[WorkspaceVariableID]
		,X.[CLINID]
		,X.[WBSID]
		,X.[BOEID]
		FROM [version].[SumOfBOE_WorkspaceVariableXREF] X
		INNER JOIN [version].[WorkspaceVariable] WSV ON X.WorkspaceVariableID = WSV.WorkspaceVariableID
		INNER JOIN [version].Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		X.VersionID = @VersionID AND
		WSV.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[SumOfBOE_WorkspaceVariableXREF] OFF
		END
		IF EXISTS (SELECT 1 FROM [version].[WorkspaceVariableSumVariableResourceTypeXREF] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[WorkspaceVariableSumVariableResourceTypeXREF] ON
		INSERT INTO [dbo].[WorkspaceVariableSumVariableResourceTypeXREF]
		([WVSVRTID]
		,[WorkspaceVariableID]
		,[SumVariableResourceTypeID])
		SELECT X.[WVSVRTID]
		,X.[WorkspaceVariableID]
		,X.[SumVariableResourceTypeID]
		FROM [version].[WorkspaceVariableSumVariableResourceTypeXREF] X 
		INNER JOIN [version].[WorkspaceVariable] WSV ON X.WorkspaceVariableID = WSV.WorkspaceVariableID
		INNER JOIN [version].Workspace WS ON WSV.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		X.VersionID = @VersionID AND
		WSV.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[WorkspaceVariableSumVariableResourceTypeXREF] OFF
		END
		IF EXISTS (SELECT 1 FROM [version].[BOEStateHistory] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[BOEStateHistory] ON
		INSERT INTO [dbo].[BOEStateHistory]
				   ([BOEStateHistoryID]
				   ,[BOEID]
				   ,[FieldID]
				   ,[CurrentBOEStateID]
				   ,[UpdatedBOEStateID]
				   ,[ChangedByETIUserID]
				   ,[UpdateDT]
				   )
		SELECT BH.[BOEStateHistoryID]
			  ,BH.[BOEID]
			  ,BH.[FieldID]
			  ,BH.[CurrentBOEStateID]
			  ,BH.[UpdatedBOEStateID]
			  ,BH.[ChangedByETIUserID]
			  ,BH.[UpdateDT]
		  FROM [version].[BOEStateHistory] BH
		INNER JOIN [version].BOE B ON BH.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		BH.VersionID = @VersionID AND 
		B.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[BOEStateHistory] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[BOEUserRoleHistory] WHERE VersionID = @VersionID)
		BEGIN

		/*WI 5160*/
		SET IDENTITY_INSERT  [dbo].[BOEUserRoleHistory] ON
		INSERT INTO [dbo].[BOEUserRoleHistory]
				   ([BOEUserRoleHistoryID]
				   ,[UpdateDT]
				   ,[CurrentETIUserID]
				   ,[UpdatedETIUserID]
				   ,[RoleID]
				   ,[BOEID]
				   ,[FieldID]
				   ,[ChangedByETIUserID])
		SELECT BUR.[BOEUserRoleHistoryID]
			  ,BUR.[UpdateDT]
			  ,BUR.[CurrentETIUserID]
			  ,BUR.[UpdatedETIUserID]
			  ,BUR.[RoleID]
			  ,BUR.[BOEID]
			  ,BUR.[FieldID]
			  ,BUR.[ChangedByETIUserID]
		FROM [version].[BOEUserRoleHistory] BUR
		INNER JOIN [version].BOE B ON BUR.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND  
		WS.WorkspaceID = @WorkspaceID AND 
		BUR.VersionID = @VersionID AND 
		B.VersionID = @VersionID
		SET IDENTITY_INSERT  [dbo].[BOEUserRoleHistory] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[BOEUserRole] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT  [dbo].[BOEUserRole] ON
		INSERT INTO [dbo].[BOEUserRole]
		([BOEUserRoleID]
		,[ETIUserID]
		,[RoleID]
		,[BOEID]
		,[UpdateDT]
		)
		SELECT BUR.[BOEUserRoleID]
		,BUR.[ETIUserID]
		,BUR.[RoleID]
		,BUR.[BOEID]
		,BUR.[UpdateDT]
		FROM [version].[BOEUserRole] BUR
		INNER JOIN [version].BOE B ON BUR.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND  
		WS.WorkspaceID = @WorkspaceID AND 
		BUR.VersionID = @VersionID AND 
		B.VersionID = @VersionID
		SET IDENTITY_INSERT  [dbo].[BOEUserRole] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[BOEApproval] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT  [dbo].[BOEApproval] ON
		INSERT INTO [dbo].[BOEApproval]
		([BOEApprovalID]
		,[BOEID]
		,[ApprovalETIUserID]
		,[ApprovedFlag]
		,[UpdateDT]
		)
		SELECT BA.[BOEApprovalID]
		,BA.[BOEID]
		,BA.[ApprovalETIUserID]
		,NULL /*WI 9601 Approval Flag Set To NULL In Restore BA.[ApprovedFlag]*/
		,BA.[UpdateDT]
		FROM [version].[BOEApproval] BA
		INNER JOIN [version].BOE B ON BA.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND  
		WS.WorkspaceID = @WorkspaceID AND 
		BA.VersionID = @VersionID AND 
		B.VersionID = @VersionID
		SET IDENTITY_INSERT  [dbo].[BOEApproval] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[BOEComment] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[BOEComment] ON
		INSERT INTO [dbo].[BOEComment]
		([BOECommentID]
		,[FieldID]
		,[BOEComments]
		,[BOECommentETIUserID]
		,[BOEResponseToCommentID]
		,[BOEID]
		,[UpdateDT]
		)
		SELECT BC.[BOECommentID]
		,BC.[FieldID]
		,BC.[BOEComments]
		,BC.[BOECommentETIUserID]
		,BC.[BOEResponseToCommentID]
		,BC.[BOEID]
		,BC.[UpdateDT]
		FROM [version].[BOEComment] BC
		INNER JOIN [version].BOE B ON BC.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND  
		WS.WorkspaceID = @WorkspaceID AND 
		BC.VersionID = @VersionID AND 
		B.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[BOEComment] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[BOECommentHistory] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[BOECommentHistory] ON
		INSERT INTO [dbo].[BOECommentHistory]
		([BOECommentHistoryID]
		,[BOECommentID]
		,[BOEID]
		,[FieldID]
		,[CurrentComment]
		,[UpdatedComment]
		,[ChangedByETIUserID]
		,[UpdateDT]
		)
		SELECT BCH.[BOECommentHistoryID]
		,BCH.[BOECommentID]
		,BCH.[BOEID]
		,BCH.[FieldID]
		,BCH.[CurrentComment]
		,BCH.[UpdatedComment]
		,BCH.[ChangedByETIUserID]
		,BCH.[UpdateDT]
		FROM [version].[BOECommentHistory] BCH
		INNER JOIN [version].[BOEComment] BC ON BCH.BOECommentID = BC.BOECommentID
		INNER JOIN [version].BOE B ON BCH.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID AND 
		BCH.VersionID = @VersionID AND 
		BC.VersionID = @VersionID AND
		B.VersionID = @VersionID
		SET IDENTITY_INSERT  [dbo].[BOECommentHistory] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[CustomFieldValue] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[CustomFieldValue] ON
		INSERT INTO [dbo].[CustomFieldValue]
		([CustomFieldValueID]
		,[CustomFieldValueName]
		,[CustomFieldValueDescription]
		,[CustomFieldID]
		,[CustomFieldValueInUseFlag]
		,[UpdateDT]
		)
		SELECT CFV.[CustomFieldValueID]
		,CFV.[CustomFieldValueName]
		,CFV.[CustomFieldValueDescription]
		,CFV.[CustomFieldID]
		,CFV.[CustomFieldValueInUseFlag]
		,CFV.[UpdateDT]
		FROM [version].[CustomFieldValue] CFV
		INNER JOIN [version].CustomField CF ON CFV.CustomFieldID = CF.CustomFieldID
		INNER JOIN [version].Workspace WS ON CF.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID AND 
		CFV.VersionID = @VersionID AND
		CF.VersionID = @VersionID
		SET IDENTITY_INSERT  [dbo].[CustomFieldValue] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[MSTTravelTrip] WHERE VersionID = @VersionID)
		BEGIN
			SET IDENTITY_INSERT [dbo].[MSTTravelTrip] ON

			INSERT INTO [dbo].[MSTTravelTrip]
				([MSTTravelTripID]
				,[ModeID]
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
				tt.[MSTTravelTripID]
				,tt.[ModeID]
				,tt.[TravelTripTaskElementID]
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
				FROM [version].[MSTTravelTrip] tt 
					INNER JOIN [version].TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
					INNER JOIN [version].BOE B ON TE.BOEID = B.BOEID
					INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
				WHERE
					tt.VersionID = @VersionID AND
					TE.VersionID = @VersionID AND
					B.VersionID = @VersionID AND
					WS.VersionID = @VersionID AND
					WS.WorkspaceID = @WorkspaceID

			SET IDENTITY_INSERT dbo.[MSTTravelTrip] OFF
		END
		IF EXISTS (SELECT 1 FROM [version].[MSTTravelTripCustomFieldValueXREF] WHERE VersionID = @VersionID)
		BEGIN
			SET IDENTITY_INSERT [dbo].[MSTTravelTripCustomFieldValueXREF] ON

				INSERT INTO [dbo].[MSTTravelTripCustomFieldValueXREF]
					([MSTTCFVID]
					,[MSTTravelTripID]
					,[MSTCustomFieldValueID]
					,[UpdateDT]
					)
				SELECT 
					cI.[MSTTCFVID]
					,cI.[MSTTravelTripID]
					,cI.[MSTCustomFieldValueID]
					,cI.[UpdateDT]
				FROM [version].[MSTTravelTripCustomFieldValueXREF] cI 
					INNER JOIN [version].MSTTravelTrip tt ON cI.MSTTravelTripID = tt.MSTTravelTripID
					INNER JOIN [version].TravelTripTaskElement TE ON tt.TravelTripTaskElementID = TE.TravelTripTaskElementID
					INNER JOIN [version].BOE B ON TE.BOEID = B.BOEID
					INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
				WHERE
					cI.VersionID = @VersionID AND
					tt.VersionID = @VersionID AND
					TE.VersionID = @VersionID AND
					B.VersionID = @VersionID AND
					WS.VersionID = @VersionID AND
					WS.WorkspaceID = @WorkspaceID

			SET IDENTITY_INSERT dbo.[MSTTravelTripCustomFieldValueXREF] OFF
		END
		IF EXISTS (SELECT 1 FROM [version].[BOETaskElement] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT  [dbo].[BOETaskElement] ON
		INSERT INTO [dbo].[BOETaskElement]
		([BOETaskElementID]
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
		,[UpdateDT]
		,[SortOrderID]
		,[AuthorUserId]
		)
		SELECT BTE.[BOETaskElementID]
		,BTE.[TaskID]
		,BTE.[TaskTitle]
		,BTE.[TaskDescription]
		,BTE.[TaskStartDate]
		,BTE.[TaskEndDate]
		,BTE.[MOQHoursEquation]
		,BTE.[MOQCostEquation]
		,BTE.[MOQText]
		,BTE.[MOQTypeID]
		,BTE.[BOEID]
		,BTE.[LaborTypeWarningFlag]
		,BTE.[IMS_ID]
		,BTE.[TaskElementTypeID]
		,BTE.[UpdateDT]
		,BTE.[SortOrderID]
		,BTE.[AuthorUserId]
		FROM [version].[BOETaskElement] BTE
		INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		BTE.VersionID = @VersionID AND 
		B.VersionID = @VersionID

		SET IDENTITY_INSERT  [dbo].[BOETaskElement] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[OrdinaryVariable] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[OrdinaryVariable] ON
		INSERT INTO [dbo].[OrdinaryVariable]
		([OrdinaryVariableID]
		,[OrdinaryVariableName]
		,[OrdinaryVariableValue]
		,[BOETaskElementID]
		,[SortByID]
		,[ValueTypeID]
		,[IsPercentage]
		,[UpdateDT]
		,[DefaultSize]
		)
		SELECT OV.[OrdinaryVariableID]
		,OV.[OrdinaryVariableName]
		,OV.[OrdinaryVariableValue]
		,OV.[BOETaskElementID]
		,OV.[SortByID]
		,OV.[ValueTypeID]
		,OV.[IsPercentage]
		,OV.[UpdateDT]
		,OV.[DefaultSize]
		FROM [version].[OrdinaryVariable] OV
		INNER JOIN [version].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		OV.VersionID = @VersionID AND 
		B.VersionID = @VersionID AND
		BTE.VersionID = @VersionID
		SET IDENTITY_INSERT  [dbo].[OrdinaryVariable] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[BOEApprovalHistory] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[BOEApprovalHistory] ON
		INSERT INTO [dbo].[BOEApprovalHistory]
		([BOEApprovalHistoryID]
		,[BOEID]
		,[Approval]
		,[ApprovalETIUserID]
		,[UpdateDT]
		)
		SELECT BH.[BOEApprovalHistoryID]
		,BH.[BOEID]
		,BH.[Approval]
		,BH.[ApprovalETIUserID]
		,BH.[UpdateDT]
		FROM [version].[BOEApprovalHistory] BH
		INNER JOIN [version].BOE B ON BH.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		BH.VersionID = @VersionID AND 
				B.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[BOEApprovalHistory] OFF

		END
		IF EXISTS (SELECT vR.ResourceID 
							FROM [version].[Resource] vR
								INNER JOIN [version].[BOELaborType] BLT ON vR.ResourceID = BLT.ResourceID
								INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
								INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
								INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN dbo.[Resource] R  ON vR.ResourceID = R.ResourceID
							WHERE 
								vR.VersionID = @VersionID AND
								BLT.VersionID = @VersionID AND
								BTE.VersionID = @VersionID AND
								B.VersionID = @VersionID AND 
								WS.VersionID = @VersionID AND 
								WS.WorkspaceID = @WorkspaceID AND
								R.ResourceID IS NULL
						) --IS NOT NULL
				BEGIN

					SELECT DISTINCT	@ErrorMessage = 
						IsNull(@ErrorMessage,'WARNING: ') + 
						'ResourceID: ' + 
						CAST (vR.ResourceID AS varchar(10)) + 
						' Resource Name: ' + 
						vR.ResourceName + 
						' no longer exists.' + 
						CHAR(13) 
					FROM [version].[Resource] vR
								INNER JOIN [version].[BOELaborType] BLT ON vR.ResourceID = BLT.ResourceID
								INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
								INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
								INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN dbo.[Resource] R  ON vR.ResourceID = R.ResourceID
							WHERE 
								vR.VersionID = @VersionID AND
								BLT.VersionID = @VersionID AND
								BTE.VersionID = @VersionID AND
								B.VersionID = @VersionID AND 
								WS.VersionID = @VersionID AND 
								WS.WorkspaceID = @WorkspaceID AND
								R.ResourceID IS NULL

				END
		IF EXISTS (SELECT vPO.PerformingOrganizationID 
							FROM [version].[PerformingOrganization] vPO
								INNER JOIN [version].[BOELaborType] BLT ON vPO.PerformingOrganizationID = BLT.PerformingOrganizationID
								INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
								INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
								INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN dbo.[PerformingOrganization] PO  ON PO.PerformingOrganizationID = vPO.PerformingOrganizationID
							WHERE 
								vPO.VersionID = @VersionID AND
								BLT.VersionID = @VersionID AND
								BTE.VersionID = @VersionID AND
								B.VersionID = @VersionID AND 
								WS.VersionID = @VersionID AND 
								WS.WorkspaceID = @WorkspaceID AND
								PO.PerformingOrganizationID IS NULL
						)-- IS NOT NULL
				BEGIN

					SELECT	DISTINCT	@ErrorMessage = 
						IsNull(@ErrorMessage,'WARNING: ') + 
						'PerformingOrganizationID: ' + 
						CAST (vPO.PerformingOrganizationID AS varchar(10)) + 
						' Performing Organization Name: ' + 
						vPO.PerformingOrganizationName + 
						' no longer exists.' + 
						CHAR(13)
							FROM [version].[PerformingOrganization] vPO
								INNER JOIN [version].[BOELaborType] BLT ON vPO.PerformingOrganizationID = BLT.PerformingOrganizationID
								INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
								INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
								INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
								LEFT OUTER JOIN dbo.[PerformingOrganization] PO  ON PO.PerformingOrganizationID = vPO.PerformingOrganizationID
							WHERE 
								vPO.VersionID = @VersionID AND
								BLT.VersionID = @VersionID AND
								BTE.VersionID = @VersionID AND
								B.VersionID = @VersionID AND 
								WS.VersionID = @VersionID AND 
								WS.WorkspaceID = @WorkspaceID AND
								PO.PerformingOrganizationID IS NULL
				END
		IF EXISTS (SELECT 1 FROM [version].[RteTemplate] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[RteTemplate] ON
		INSERT INTO [dbo].[RteTemplate]
		([TemplateID],
		[UpdateDT],
		[WorkspaceID],
		[Description],
		[AuthorID],
		[CreatedOn]
		)
		SELECT RT.[TemplateID]
		,RT.[UpdateDT]
		,RT.[WorkspaceID]
		,RT.[Description]
		,RT.[AuthorID]
		,RT.[CreatedOn]
		FROM [version].[RteTemplate] RT
		WHERE 
		RT.VersionID = @VersionID AND
		RT.WorkspaceID = @WorkspaceID 
		SET IDENTITY_INSERT [dbo].[RteTemplate] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[RteTemplateAssigned] WHERE VersionID = @VersionID)
		BEGIN

		INSERT INTO [dbo].[RteTemplateAssigned]
		([TemplateID],
		[RteTemplateSourceId]
		)
		SELECT RTA.[TemplateID]
		,RTA.[RteTemplateSourceId]
		FROM [version].[RteTemplateAssigned] RTA
		INNER JOIN [version].[RteTemplate] RT ON RT.[TemplateID] = RTA.[TemplateID]
		WHERE   
		RT.VersionID = @VersionID AND RT.WorkspaceID = @WorkspaceID AND 
		RTA.VersionID = @VersionID

		END
		IF EXISTS (SELECT 1 FROM [version].[RteTemplateQuestion] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[RteTemplateQuestion] ON
		INSERT INTO [dbo].[RteTemplateQuestion]
		([QuestionID],
		[UpdateDT],
		[TemplateID],
		[Text],
		[SortOrder],
		[Required]
		)
		SELECT RTQ.[QuestionID]
		,RTQ.[UpdateDT]
		,RTQ.[TemplateID]
		,RTQ.[Text]
		,RTQ.[SortOrder]
		,RTQ.[Required]
		FROM [version].[RteTemplateQuestion] RTQ
		INNER JOIN [version].[RteTemplate] RT ON RT.[TemplateID] = RTQ.[TemplateID]
		WHERE   
		RT.VersionID = @VersionID AND RT.WorkspaceID = @WorkspaceID AND 
		RTQ.VersionID = @VersionID
		SET IDENTITY_INSERT  [dbo].[RteTemplateQuestion] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[RteTemplateAnswer] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[RteTemplateAnswer] ON
		INSERT INTO [dbo].[RteTemplateAnswer]
		([AnswerID],
		[UpdateDT],
		[QuestionID],
		[BOEID],
		[TaskID],
		[Text],
		[RteTemplateSourceId]
		)
		SELECT RTA.[AnswerID]
		,RTA.[UpdateDT]
		,RTA.[QuestionID]
		,RTA.[BOEID]
		,RTA.[TaskID]
		,RTA.[Text]
		,RTA.[RteTemplateSourceId]
		FROM [version].[RteTemplateAnswer] RTA
		INNER JOIN [version].[RteTemplateQuestion] RTQ ON RTA.[QuestionID] = RTQ.[QuestionID]
		INNER JOIN [version].[RteTemplate] RT ON RT.[TemplateID] = RTQ.[TemplateID]
		WHERE   
		RT.VersionID = @VersionID AND RT.WorkspaceID = @WorkspaceID AND 
		RTQ.VersionID = @VersionID AND RTA.VersionID = @VersionID
		SET IDENTITY_INSERT  [dbo].[RteTemplateAnswer] OFF

		END
		/** [dbo].[MOQTypeSelection] **/
		IF EXISTS (SELECT 1 FROM [version].[MOQTypeSelection] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[MOQTypeSelection] ON
		INSERT INTO [dbo].[MOQTypeSelection]
		([MOQTypeSelectionId],
		[TaskId],
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
		[HistoricalReferenceExplanation],
		[SkillMix]
		)
		SELECT M.[MOQTypeSelectionId],
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
			M.[HistoricalReferenceExplanation],
			M.[SkillMix]
		FROM [version].[MOQTypeSelection] M
		INNER JOIN [version].[BOETaskElement] T on M.TaskId = T.BOETaskElementID
		INNER JOIN [version].[BOE] B ON T.BOEID  = B.BOEID
		INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE 
		M.VersionId = @VersionID AND
		T.VersionID = @VersionID AND
		B.VersionID = @VersionID AND
		WS.VersionID = @VersionID AND
		WS.WorkspaceID = @WorkspaceID

		SET IDENTITY_INSERT [dbo].[MOQTypeSelection] OFF

		END
		/** [dbo].[MOQTypeSelectionTableData] **/
		IF EXISTS (SELECT 1 FROM [version].[MOQTypeSelectionTableData] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[MOQTypeSelectionTableData] ON
		INSERT INTO [dbo].[MOQTypeSelectionTableData]
		([MOQTypeSelectionTableDataId],
		[MOQTypeSelectionId],
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
		SELECT TD.[MOQTypeSelectionTableDataId],
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
		TD.[TotalRelevantHoursAfterQueryFilters]
		FROM [version].[MOQTypeSelectionTableData] TD
		INNER JOIN [version].[MOQTypeSelection] M ON TD.[MOQTypeSelectionId] = M.[MOQTypeSelectionId]
		INNER JOIN [version].[BOETaskElement] T on M.TaskId = T.BOETaskElementID
		INNER JOIN [version].[BOE] B ON T.BOEID  = B.BOEID
		INNER JOIN [version].[Workspace] WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE 
		TD.VersionId = @VersionID AND
		M.VersionId = @VersionID AND
		T.VersionID = @VersionID AND
		B.VersionID = @VersionID AND
		WS.VersionID = @VersionID AND
		WS.WorkspaceID = @WorkspaceID

		SET IDENTITY_INSERT [dbo].[MOQTypeSelectionTableData] OFF

		END


		IF EXISTS (SELECT 1 FROM [version].[MoqTypeTableCustomFieldValueXREF] WHERE VersionID = @VersionID)
		BEGIN
			SET IDENTITY_INSERT [dbo].[MoqTypeTableCustomFieldValueXREF] ON

			INSERT INTO [dbo].[MoqTypeTableCustomFieldValueXREF] ([Id], [UpdateDT], [MoqTypeTableDataId], [CustomFieldValueId])
				SELECT x.[Id], x.[UpdateDT], x.[MoqTypeTableDataId], x.[CustomFieldValueId]
					FROM [version].[MoqTypeTableCustomFieldValueXREF] x
					INNER JOIN [version].MoqTypeSelectionTableData t ON t.MoqTypeSelectionTableDataId = x.MoqTypeTableDataId AND t.VersionId = @VersionID
					INNER JOIN [version].MoqTypeSelection mS ON mS.MoqTypeSelectionId = t.MoqTypeSelectionId AND mS.VersionId = @VersionID
					INNER JOIN [version].BoeTaskElement tE ON tE.BoeTaskElementId = mS.TaskId AND tE.VersionId = @VersionID
					INNER JOIN [version].BOE b ON tE.BOEID = b.BOEID AND b.VersionId = @VersionID
					INNER JOIN [version].Workspace ws ON b.WorkspaceID = ws.WorkspaceId AND ws.VersionId = @VersionID
					WHERE ws.WorkspaceId = @WorkspaceID

			SET IDENTITY_INSERT [dbo].[MoqTypeTableCustomFieldValueXREF] OFF

		END

		/** [dbo].[SkillMix] **/
		IF EXISTS (SELECT 1 FROM [version].[SkillMix] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[SkillMix] ON
		INSERT INTO [dbo].[SkillMix]
		([SkillMixID],
		[Rationale],
		[Included],
		[ProposedHours],
		[HistoricalHours],
		[BOESkillMix],
		[LaborSkillMix],
		[ResourceOld],
		[ResourceNew],
		[BOETaskElementID],
		[BOEID],
		[IsUserInput]
		)
		SELECT SM.[SkillMixID],
			SM.[Rationale],
			SM.[Included],
			SM.[ProposedHours],
			SM.[HistoricalHours],
			SM.[BOESkillMix],
			SM.[LaborSkillMix],
			SM.[ResourceOld],
			SM.[ResourceNew],
			SM.[BOETaskElementID],
			SM.[BOEID],
			SM.[IsUserInput]
		FROM [version].[SkillMix] SM
		WHERE 
		SM.VersionId = @VersionID

		SET IDENTITY_INSERT [dbo].[SkillMix] OFF

		END

		/** [dbo].[CommonDisclosureSkillMix] **/
		IF EXISTS (SELECT 1 FROM [version].[CommonDisclosureSkillMix] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[CommonDisclosureSkillMix] ON
		INSERT INTO [dbo].[CommonDisclosureSkillMix]
		([CommonDisclosureSkillMixID],
		[Rationale],
		[Included],
		[ProposedHours],
		[HistoricalHours],
		[BOESkillMix],
		[LaborSkillMix],
		[ResourceID],
		[BusinessResourceID],
		[BOEID],
		[BOETaskElementID]
		)
		SELECT CD.[CommonDisclosureSkillMixID],
			CD.[Rationale],
			CD.[Included],
			CD.[ProposedHours],
			CD.[HistoricalHours],
			CD.[BOESkillMix],
			CD.[LaborSkillMix],
			CD.[ResourceID],
			CD.[BusinessResourceID],
			CD.[BOEID],
			CD.[BOETaskElementID]
		FROM [version].[CommonDisclosureSkillMix] CD
		WHERE 
		CD.VersionId = @VersionID

		SET IDENTITY_INSERT [dbo].[CommonDisclosureSkillMix] OFF

		END

		/** [dbo].[SkillMixSummary] **/
		IF EXISTS (SELECT 1 FROM [version].[SkillMixSummary] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[SkillMixSummary] ON
		INSERT INTO [dbo].[SkillMixSummary]
		([SkillMixSummaryID],
		[Rationale],
		[Included],
		[ProposedHours],
		[HistoricalHours],
		[ResourceHours],
		[BusinessResourceHours],
		[BOESkillMix],
		[LaborSkillMix],
		[ResourceID],
		[BusinessResourceID],
		[BOEID],
		[BOETaskElementID]
		)
		SELECT SMS.[SkillMixSummaryID],
			SMS.[Rationale],
			SMS.[Included],
			SMS.[ProposedHours],
			SMS.[HistoricalHours],
			SMS.[ResourceHours],
			SMS.[BusinessResourceHours],
			SMS.[BOESkillMix],
			SMS.[LaborSkillMix],
			SMS.[ResourceID],
			SMS.[BusinessResourceID],
			SMS.[BOEID],
			SMS.[BOETaskElementID]
		FROM [version].[SkillMixSummary] SMS
		WHERE 
		SMS.VersionId = @VersionID

		SET IDENTITY_INSERT [dbo].[SkillMixSummary] OFF

		END

		/** [dbo].[MOQTypeSelectionTableDataResourceHours] **/
		IF EXISTS (SELECT 1 FROM [version].[MOQTypeSelectionTableDataResourceHours] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[MOQTypeSelectionTableDataResourceHours] ON
		INSERT INTO [dbo].[MOQTypeSelectionTableDataResourceHours]
		([MOQTypeSelectionTableDataResourceHoursId],
		[ResourceName],
		[BRCName],
		[WbsHours],
		[TotalHours],
		[MOQTypeSelectionTableDataId],
		[BOETaskElementID],
		[BOEID]
		)
		SELECT M.[MOQTypeSelectionTableDataResourceHoursId],
			M.[ResourceName],
			M.[BRCName],
			M.[WbsHours],
			M.[TotalHours],
			M.[MOQTypeSelectionTableDataId],
			M.[BOETaskElementID],
			M.[BOEID]
		FROM [version].[MOQTypeSelectionTableDataResourceHours] M
		WHERE 
		M.VersionId = @VersionID

		SET IDENTITY_INSERT [dbo].[MOQTypeSelectionTableDataResourceHours] OFF

		END

		IF EXISTS (SELECT 1 FROM [version].[BOELaborType] WHERE VersionID = @VersionID)
		BEGIN
		/*Updated for WI 8398*/
		SET IDENTITY_INSERT [dbo].[BOELaborType] ON
		INSERT INTO [dbo].[BOELaborType]
		([BOELaborTypeID]
		,[ResourceID]
		,[PerformingOrganizationID]
		,[BOELaborTypeStartDate]
		,[BOELaborTypeEndDate]
		,[SpreadCurveID]
		,[PercentSpread]
		,[ValueSpread]
		,[BOETaskElementID]
		,[SpreadTypeID]
		,[UpdateDT]
		,[PercentSpreadLocked]
		,[HourSpreadLocked]
		,[WBSID]
		,[CLINID]
		,[CanOffload]
		,[LaborSortID]
		,[BRCResourceID]
		)
		SELECT BLT.[BOELaborTypeID]
		,R.[ResourceID]
		,PO.[PerformingOrganizationID]
		,BLT.[BOELaborTypeStartDate]
		,BLT.[BOELaborTypeEndDate]
		,BLT.[SpreadCurveID]
		,BLT.[PercentSpread]
		,BLT.[ValueSpread]
		,BLT.[BOETaskElementID]
		,BLT.[SpreadTypeID]
		,BLT.[UpdateDT]
		,BLT.[PercentSpreadLocked]
		,BLT.[HourSpreadLocked]
		,BLT.[WBSID]
		,BLT.[CLINID]
		,BLT.[CanOffload]
		,BLT.[LaborSortID]
		,BLT.[BRCResourceID]
		FROM [version].[BOELaborType] BLT
		INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		LEFT OUTER JOIN [dbo].[Resource] R ON BLT.ResourceID = R.ResourceID
		LEFT OUTER JOIN [dbo].[PerformingOrganization] PO ON BLT.PerformingOrganizationID = PO.PerformingOrganizationID
		WHERE 
		BLT.VersionID = @VersionID AND
		BTE.VersionID = @VersionID AND
		B.VersionID = @VersionID AND 
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID


		SET IDENTITY_INSERT [dbo].[BOELaborType] OFF


		END
		IF EXISTS (SELECT 1 FROM [version].[BOETaskElementWorkspaceVariableXREF] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[BOETaskElementWorkspaceVariableXREF] ON
		INSERT INTO [dbo].[BOETaskElementWorkspaceVariableXREF]
		([BOETaskWSVarID]
		,[BOETaskElementID]
		,[WorkspaceVariableID]
		)
		SELECT X.[BOETaskWSVarID]
		,X.[BOETaskElementID]
		,X.[WorkspaceVariableID]
		FROM [version].[BOETaskElementWorkspaceVariableXREF] X
		INNER JOIN [version].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		X.VersionID = @VersionID AND 
				B.VersionID = @VersionID AND 
				BTE.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[BOETaskElementWorkspaceVariableXREF] OFF
		END
		IF EXISTS (SELECT 1 FROM [version].[BOETaskElementMetricDetailXREF] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[BOETaskElementMetricDetailXREF] ON
		INSERT INTO [dbo].[BOETaskElementMetricDetailXREF]
		([BTEMDID]
		,[BOETaskElementID]
		,[MetricDetailID]
		,[UpdateDT]
		)
		SELECT X.[BTEMDID]
		,X.[BOETaskElementID]
		,X.[MetricDetailID]
		,X.[UpdateDT]
		FROM [version].[BOETaskElementMetricDetailXREF] X
		INNER JOIN [version].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID AND 
				X.VersionID = @VersionID AND
				BTE.VersionID = @VersionID AND
				B.VersionID = @VersionID 

		SET IDENTITY_INSERT [dbo].[BOETaskElementMetricDetailXREF] OFF


		END
		IF EXISTS (SELECT 1 FROM [version].[BOETaskElementCustomFieldValueXREF] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[BOETaskElementCustomFieldValueXREF] ON
		INSERT INTO [dbo].[BOETaskElementCustomFieldValueXREF]
		([BTECFVID]
		,[BOETaskElementID]
		,[CustomFieldValueID]
		,[UpdateDT]
		)
		SELECT X.[BTECFVID]
		,X.[BOETaskElementID]
		,X.[CustomFieldValueID]
		,X.[UpdateDT]
		FROM [version].[BOETaskElementCustomFieldValueXREF] X
		INNER JOIN [version].[BOETaskElement] BTE ON X.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND  WS.WorkspaceID = @WorkspaceID AND 
		X.VersionID = @VersionID AND
		BTE.VersionID = @VersionID AND
		B.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[BOETaskElementCustomFieldValueXREF] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[BOECustomFieldValueXREF] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT [dbo].[BOECustomFieldValueXREF] ON
		INSERT INTO [dbo].[BOECustomFieldValueXREF]
		([BCFVID]
		,[BOEID]
		,[CustomFieldValueID]
		,[UpdateDT]
		)
		SELECT X.[BCFVID]
		,X.[BOEID]
		,X.[CustomFieldValueID]
		,X.[UpdateDT]
		FROM [version].[BOECustomFieldValueXREF] X
		INNER JOIN [version].BOE B ON X.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND WS.WorkspaceID = @WorkspaceID AND 
		X.VersionID = @VersionID AND 
		B.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[BOECustomFieldValueXREF] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[TravelTripTaskElementCustomFieldValueXREF] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[TravelTripTaskElementCustomFieldValueXREF] ON
		INSERT INTO [dbo].[TravelTripTaskElementCustomFieldValueXREF]
		([TTECFVID]
		,[TravelTripTaskElementID]
		,[CustomFieldValueID]
		,[UpdateDT]
		)
		SELECT X.[TTECFVID]
		,X.[TravelTripTaskElementID]
		,X.[CustomFieldValueID]
		,X.[UpdateDT]
		FROM [version].[TravelTripTaskElementCustomFieldValueXREF] X
		INNER JOIN [version].[TravelTripTaskElement] TE ON X.TravelTripTaskElementID = TE.TravelTripTaskElementID
		INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND  WS.WorkspaceID = @WorkspaceID AND 
		X.VersionID = @VersionID AND
		TE.VersionID = @VersionID AND
		B.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[TravelTripTaskElementCustomFieldValueXREF] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[TravelTripCustomFieldValueXREF] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[TravelTripCustomFieldValueXREF] ON
		INSERT INTO [dbo].[TravelTripCustomFieldValueXREF]
		([TCFVID]
		,[TravelTripID]
		,[CustomFieldValueID]
		,[UpdateDT]
		)
		SELECT X.[TCFVID]
		,X.[TravelTripID]
		,X.[CustomFieldValueID]
		,X.[UpdateDT]
		FROM [version].[TravelTripCustomFieldValueXREF] X
		INNER JOIN [version].TravelTrip T ON X.TravelTripID  = T.TravelTripID
		INNER JOIN [version].[TravelTripTaskElement] TE ON T.TravelTripTaskElementID = TE.TravelTripTaskElementID
		INNER JOIN [version].BOE B ON TE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND  WS.WorkspaceID = @WorkspaceID AND 
		X.VersionID = @VersionID AND
		T.VersionID = @VersionID AND
		TE.VersionID = @VersionID AND
		B.VersionID = @VersionID

		SET IDENTITY_INSERT [dbo].[TravelTripCustomFieldValueXREF] OFF


		END
		IF EXISTS (SELECT 1 FROM [version].[SumOfBOE_OrdinaryVariableXREF] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT  [dbo].[SumOfBOE_OrdinaryVariableXREF] ON
		INSERT INTO [dbo].[SumOfBOE_OrdinaryVariableXREF]
		([OVSumID]
		,[OrdinaryVariableID]
		,[CLINID]
		,[WBSID]
		,[BOEID]
		)
		SELECT X.[OVSumID]
		,X.[OrdinaryVariableID]
		,X.[CLINID]
		,X.[WBSID]
		,X.[BOEID]
		FROM [version].[SumOfBOE_OrdinaryVariableXREF] X
		INNER JOIN [version].[OrdinaryVariable] OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
		INNER JOIN [version].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		X.VersionID = @VersionID AND
		OV.VersionID = @VersionID AND
		BTE.VersionID = @VersionID AND
		B.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[SumOfBOE_OrdinaryVariableXREF] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[OrdinaryVariableSumVariableResourceTypeXREF] WHERE VersionID = @VersionID)
		BEGIN
		SET IDENTITY_INSERT  [dbo].[OrdinaryVariableSumVariableResourceTypeXREF] ON
		INSERT INTO [dbo].[OrdinaryVariableSumVariableResourceTypeXREF]
		([OVSVRTID]
		,[OrdinaryVariableID]
		,[SumVariableResourceTypeID])
		SELECT X.[OVSVRTID]
		,X.[OrdinaryVariableID]
		,X.[SumVariableResourceTypeID]
		FROM [version].[OrdinaryVariableSumVariableResourceTypeXREF] X
		INNER JOIN [version].[OrdinaryVariable] OV ON X.OrdinaryVariableID = OV.OrdinaryVariableID
		INNER JOIN [version].[BOETaskElement] BTE ON OV.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		X.VersionID = @VersionID AND
		OV.VersionID = @VersionID AND
		BTE.VersionID = @VersionID AND
		B.VersionID = @VersionID
		SET IDENTITY_INSERT [dbo].[OrdinaryVariableSumVariableResourceTypeXREF] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[BOELaborTypeCustomFieldValueXREF] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT  [dbo].[BOELaborTypeCustomFieldValueXREF] ON
		INSERT INTO [dbo].[BOELaborTypeCustomFieldValueXREF]
		([BLTCFVID]
		,[BOELaborTypeID]
		,[CustomFieldValueID]
		,[UpdateDT]
		)
		SELECT X.[BLTCFVID]
		,X.[BOELaborTypeID]
		,X.[CustomFieldValueID]
		,X.[UpdateDT]
		FROM [version].[BOELaborTypeCustomFieldValueXREF] X 
		INNER JOIN [version].[BOELaborType] BLT ON X.BOELaborTypeID = BLT.BOELaborTypeID
		INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE  
		WS.VersionID = @VersionID AND  
		WS.WorkspaceID = @WorkspaceID AND 
		X.VersionID = @VersionID AND
		BLT.VersionID = @VersionID AND
		BTE.VersionID = @VersionID AND
		B.VersionID = @VersionID
		SET IDENTITY_INSERT  [dbo].[BOELaborTypeCustomFieldValueXREF] OFF

		END
		IF EXISTS (SELECT 1 FROM [version].[BOELaborSpread] WHERE VersionID = @VersionID)
		BEGIN

		SET IDENTITY_INSERT [dbo].[BOELaborSpread] ON
		INSERT INTO [dbo].[BOELaborSpread]
		([BOELaborSpreadID]
		,[BOELaborTypeID]
		,[LaborSpreadDate]
		,[LaborSpreadValue]
		)
		SELECT LS.[BOELaborSpreadID]
		,LS.[BOELaborTypeID]
		,LS.[LaborSpreadDate]
		,LS.[LaborSpreadValue]
		FROM [version].[BOELaborSpread] LS
		INNER JOIN [version].[BOELaborType] BLT ON LS.BOELaborTypeID = BLT.BOELaborTypeID
		INNER JOIN [version].[BOETaskElement] BTE ON BLT.BOETaskElementID = BTE.BOETaskElementID
		INNER JOIN [version].BOE B ON BTE.BOEID  = B.BOEID
		INNER JOIN [version].Workspace WS ON B.WorkspaceID = WS.WorkspaceID
		WHERE   
		WS.VersionID = @VersionID AND 
		WS.WorkspaceID = @WorkspaceID AND 
		LS.VersionID = @VersionID AND
		BLT.VersionID = @VersionID AND
		BTE.VersionID = @VersionID AND
		B.VersionID = @VersionID

		SET IDENTITY_INSERT [dbo].[BOELaborSpread] OFF


		END
		IF EXISTS (SELECT 1 FROM [version].[BOEFormIBOE] WHERE VersionID = @VersionID)
		BEGIN
	
			SET IDENTITY_INSERT [dbo].[BOEFormIBOE] ON
			INSERT INTO [dbo].[BOEFormIBOE]
			([IBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, BusinessArea, Revision,FormVersion)
			SELECT [IBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, BusinessArea, Revision,FormVersion
			FROM [version].[BOEFormIBOE]
			WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

			SET IDENTITY_INSERT [dbo].[BOEFormIBOE] OFF

			INSERT INTO [dbo].[BOEFormIBOEResourcesXREF]
			SELECT x.[IBOEFormID], R.ResourceID
			FROM [version].[BOEFormIBOEResourcesXREF] x
				INNER JOIN [dbo].[BOEFormIBOE] BF ON BF.[IBOEFormID] = x.[IBOEFormID]
				LEFT OUTER JOIN [dbo].[Resource] R ON x.ResourceID = R.ResourceID
			WHERE 
				x.VersionID = @VersionID AND
				BF.WorkspaceID = @WorkspaceID

			INSERT INTO [dbo].[BOEFormIBOECLINsXREF]
			SELECT x.[IBOEFormID], x.[ClinID], x.[ContractType]
			FROM [version].[BOEFormIBOECLINsXREF] x
				INNER JOIN [dbo].[BOEFormIBOE] BF ON BF.[IBOEFormID] = x.[IBOEFormID]
			WHERE 
				x.VersionID = @VersionID AND
				BF.WorkspaceID = @WorkspaceID

		END
		IF EXISTS (SELECT 1 FROM [version].[BOEFormPBOE] WHERE VersionID = @VersionID)
		BEGIN
	
			SET IDENTITY_INSERT [dbo].[BOEFormPBOE] ON
			INSERT INTO [dbo].[BOEFormPBOE]
			([PBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion,  
			[DegreeOfCompetition],[CCoPD],[CCoPDOtherText],[RFP],[ProposalNumber],[SupplierName],[ValidityDate],[SupplierProposalSupportingDataIncluded],[PriceAnalysisIncluded],[CommercialItemDocIncluded],
			[CostAnalysisIncluded],[ShouldCostEstimate],[ShouldCostEstimateDate],[SowWritten],[SowWrittenDate],[RFPRelease],[RFPReleaseDate],[FirmSupplierReceipt],[FirmSupplierReceiptDate],
			[SourceSelection],[SourceSelectionDate],[CID],[CIDDate],[GovtReview],[GovtReviewDate],[PriceAnalysis],[PriceAnalysisDate],[TechnicalEvaluation],[TechnicalEvaluationDate],[FactFinding],
			[FactFindingDate],[CostAnalysis],[CostAnalysisDate],[GovtPricing],[GovtPricingDate],[SupplierNegotiations],[SupplierNegotiationsDate],[MOU],[MOUDate],[Procurement],[ProcurementDate],
			[PlannedDate_WrittenApproval],[PlannedDate_ApprovedSubmission]
			,[SupplierCCoPD]
			,[SourceSelectionDescription]
			,[CommercialityDescription]
			,[TechnicalEvaluationDescription]
			,[PriceAnalysisDescription]
			,[CostAnalysisDescription]
			,[RationaleValueSummary]
			,[GovtPricingReceived]
			,[GovtPricingReceivedDate]
			,[GovtPricingReceivedText]
			,[CostAnalysisUnqual]
			,[CostAnalysisUnqualDate]
			,[CostAnalysisUnqualText]
			,[VendorId]
			,[SupplierProposedValue])
			SELECT [PBOEFormID], UpdateDT, WorkspaceID, FormName, Description, [BasisAndRationale], [ProposalTitle],[ProposalDate],Poc, PocPhone, Approver, ApproverPhone, Revision,FormVersion, 
				[DegreeOfCompetition],[CCoPD],[CCoPDOtherText],[RFP],[ProposalNumber],[SupplierName],[ValidityDate],[SupplierProposalSupportingDataIncluded],[PriceAnalysisIncluded],
				[CommercialItemDocIncluded],[CostAnalysisIncluded],[ShouldCostEstimate],[ShouldCostEstimateDate],[SowWritten],[SowWrittenDate],[RFPRelease],[RFPReleaseDate],[FirmSupplierReceipt],
				[FirmSupplierReceiptDate],[SourceSelection],[SourceSelectionDate],[CID],[CIDDate],[GovtReview],[GovtReviewDate],[PriceAnalysis],[PriceAnalysisDate],[TechnicalEvaluation],
				[TechnicalEvaluationDate],[FactFinding],[FactFindingDate],[CostAnalysis],[CostAnalysisDate],[GovtPricing],[GovtPricingDate],[SupplierNegotiations],[SupplierNegotiationsDate],
				[MOU],[MOUDate],[Procurement],[ProcurementDate],[PlannedDate_WrittenApproval],[PlannedDate_ApprovedSubmission]
				,[SupplierCCoPD]
				,[SourceSelectionDescription]
				,[CommercialityDescription]
				,[TechnicalEvaluationDescription]
				,[PriceAnalysisDescription]
				,[CostAnalysisDescription]
				,[RationaleValueSummary]
				,[GovtPricingReceived]
				,[GovtPricingReceivedDate]
				,[GovtPricingReceivedText]
				,[CostAnalysisUnqual]
				,[CostAnalysisUnqualDate]
				,[CostAnalysisUnqualText]
				,[VendorId]
				,[SupplierProposedValue]
			  FROM [version].[BOEFormPBOE]
			WHERE WorkspaceID = @WorkspaceID AND VersionID = @VersionID

			SET IDENTITY_INSERT [dbo].[BOEFormPBOE] OFF

			INSERT INTO [dbo].[BOEFormPBOEResourcesXREF]
			SELECT x.[PBOEFormID], R.ResourceID
			FROM [version].[BOEFormPBOEResourcesXREF] x
				INNER JOIN [dbo].[BOEFormPBOE] BF ON BF.[PBOEFormID] = x.[PBOEFormID]
				LEFT OUTER JOIN [dbo].[Resource] R ON x.ResourceID = R.ResourceID
			WHERE 
				x.VersionID = @VersionID AND
				BF.WorkspaceID = @WorkspaceID

			INSERT INTO [dbo].[BOEFormPBOECLINsXREF]
			SELECT x.[PBOEFormID], x.[ClinID], x.[ContractType]
			FROM [version].[BOEFormPBOECLINsXREF] x
				INNER JOIN [dbo].[BOEFormPBOE] BF ON BF.[PBOEFormID] = x.[PBOEFormID]
			WHERE 
				x.VersionID = @VersionID AND
				BF.WorkspaceID = @WorkspaceID

		END

		INSERT INTO [dbo].[WorkspaceRMSTravelNonzoneFeesAndCosts] ([UpdateDT], [WorkspaceID], ModeID, TravelAgencyFee, MiscOther)
			SELECT [UpdateDT], [WorkspaceID], ModeID, TravelAgencyFee, MiscOther
			FROM [version].[WorkspaceRMSTravelNonzoneFeesAndCosts] WHERE VersionID = @VersionID
		INSERT INTO [dbo].[WorkspaceRMSTravelEscalationRate] ([UpdateDT], [WorkspaceID], [Year], [Escalation], [PerDiemRate], [MiscRate])
			SELECT [UpdateDT], [WorkspaceID], [Year], [Escalation], [PerDiemRate], [MiscRate]
			FROM [version].[WorkspaceRMSTravelEscalationRate] WHERE VersionID = @VersionID
		INSERT INTO [dbo].[WorkspaceRestoreLog]
				   ([ActionPerformed]
				   ,[WorkspaceID]
				   ,[WorkspaceName]
				   ,[ETIUserID]
				   ,[RestoreDT]
				   ,[VersionCreated])
		SELECT
				   'Workspace Restore' /*Action Performed*/
				   ,@WorkspaceID 
				   ,WorkspaceName
				   ,@ETIUserID /*User performing the Restore*/
				   ,GETDATE() /*Date the restore was performed*/
				   ,WV.VersionCreated /*Date the version was created*/
		FROM dbo.Workspace W 
			INNER JOIN dbo.WorkspaceVersion WV ON W.WorkspaceID = WV.WorkspaceID
		WHERE 
			W.WorkspaceID = @WorkspaceID AND
			WV.WorkspaceID = @WorkspaceID AND
			WV.VersionID = @VersionID

		EXECUTE [dbo].[updateTravelTripInUse]
		SELECT @ErrorMessage AS ErrorMessage 
	END TRY

	BEGIN CATCH	
		SET IDENTITY_INSERT [dbo].[BOE] OFF
		SET IDENTITY_INSERT [dbo].[BOEApproval] OFF
		SET IDENTITY_INSERT [dbo].[BOEApprovalHistory] OFF
		SET IDENTITY_INSERT [dbo].[BOEComment] OFF
		SET IDENTITY_INSERT [dbo].[BOECommentHistory] OFF
		SET IDENTITY_INSERT [dbo].[BOECustomFieldValueXREF] OFF
		SET IDENTITY_INSERT [dbo].[BOEFormIBOE] OFF
		SET IDENTITY_INSERT [dbo].[BOEFormPBOE] OFF
		SET IDENTITY_INSERT [dbo].[BOELaborSpread] OFF
		SET IDENTITY_INSERT [dbo].[BOELaborType] OFF
		SET IDENTITY_INSERT [dbo].[BOELaborTypeCustomFieldValueXREF] OFF
		SET IDENTITY_INSERT [dbo].[BOEPotentialRole] OFF
		SET IDENTITY_INSERT [dbo].[BOEStateHistory] OFF
		SET IDENTITY_INSERT [dbo].[BOETaskElement] OFF
		SET IDENTITY_INSERT [dbo].[BOETaskElementCustomFieldValueXREF] OFF
		SET IDENTITY_INSERT [dbo].[BOETaskElementWorkspaceVariableXREF] OFF
		SET IDENTITY_INSERT [dbo].[BOEUserRole] OFF
		SET IDENTITY_INSERT [dbo].[BOEUserRoleHistory] OFF
		SET IDENTITY_INSERT [dbo].[BusinessArea] OFF
		SET IDENTITY_INSERT [dbo].[CLIN] OFF
		SET IDENTITY_INSERT [dbo].[CustomField] OFF
		SET IDENTITY_INSERT [dbo].[CustomFieldValue] OFF
		SET IDENTITY_INSERT [dbo].[ETIGroup] OFF
		SET IDENTITY_INSERT [dbo].[ETIuser] OFF
		SET IDENTITY_INSERT [dbo].[LineOfBusiness] OFF
		SET IDENTITY_INSERT [dbo].[Location] OFF
		SET IDENTITY_INSERT [dbo].[MaterialTaskElement] OFF
		SET IDENTITY_INSERT [dbo].[MileageReimbursementRate] OFF
		SET IDENTITY_INSERT [dbo].[MOQTypeSelection] OFF
		SET IDENTITY_INSERT [dbo].[MOQTypeSelectionTableData] OFF
		SET IDENTITY_INSERT [dbo].[SkillMix] OFF
		SET IDENTITY_INSERT [dbo].[CommonDisclosureSkillMix] OFF
		SET IDENTITY_INSERT [dbo].[SkillMixSummary] OFF
		SET IDENTITY_INSERT [dbo].[MOQTypeSelectionTableDataResourceHours] OFF
		SET IDENTITY_INSERT [dbo].[MoqTypeTableCustomFieldValueXREF] OFF
		SET IDENTITY_INSERT [dbo].[ODCSpread] OFF
		SET IDENTITY_INSERT [dbo].[ODCTaskElement] OFF
		SET IDENTITY_INSERT [dbo].[ODCType] OFF
		SET IDENTITY_INSERT [dbo].[OrdinaryVariable] OFF
		SET IDENTITY_INSERT [dbo].[OrdinaryVariableSumVariableResourceTypeXREF] OFF
		SET IDENTITY_INSERT [dbo].[OutputFormatTemplate] OFF
		SET IDENTITY_INSERT [dbo].[OutputFormatTemplateWorkspaceXREF] OFF
		SET IDENTITY_INSERT [dbo].[PerformingOrganization] OFF
		SET IDENTITY_INSERT [dbo].[PerformingOrganizationList] OFF
		SET IDENTITY_INSERT [dbo].[ProjectMap] OFF
		SET IDENTITY_INSERT [dbo].[ProPricerCustomFieldXREF] OFF
		SET IDENTITY_INSERT [dbo].[ProPricerExport] OFF
		SET IDENTITY_INSERT [dbo].[ProPricerFieldXREF] OFF
		SET IDENTITY_INSERT [dbo].[Resource] OFF
		SET IDENTITY_INSERT [dbo].[ResourceList] OFF
		SET IDENTITY_INSERT [dbo].[RteTemplate] OFF
		SET IDENTITY_INSERT [dbo].[RteTemplateQuestion] OFF
		SET IDENTITY_INSERT [dbo].[RteTemplateAnswer] OFF
		SET IDENTITY_INSERT [dbo].[SumOfBOE_OrdinaryVariableXREF] OFF
		SET IDENTITY_INSERT [dbo].[SumOfBOE_WorkspaceVariableXREF] OFF
		SET IDENTITY_INSERT [dbo].[SystemUserRole] OFF
		SET IDENTITY_INSERT [dbo].[TMResourceRate] OFF
		SET IDENTITY_INSERT [dbo].[TravelEscalationRate] OFF
		SET IDENTITY_INSERT [dbo].[TravelMiscRate] OFF
		SET IDENTITY_INSERT [dbo].[TravelTrip] OFF
		SET IDENTITY_INSERT [dbo].[TravelTripCustomFieldValueXREF] OFF
		SET IDENTITY_INSERT [dbo].[TravelTripTaskElement] OFF
		SET IDENTITY_INSERT [dbo].[TravelTripTaskElementCustomFieldValueXREF] OFF
		SET IDENTITY_INSERT [dbo].[Trip] OFF
		SET IDENTITY_INSERT [dbo].[WBS_CLIN_BOE_XREF] OFF
		SET IDENTITY_INSERT [dbo].[WorkBreakdownStructure] OFF
		SET IDENTITY_INSERT [dbo].[Workspace] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceContractTypeXREF] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceLockedPerDiem] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceLockedTravelEscalationRate] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceLockedTravelMiscRate] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceLockedTrip] OFF
		SET IDENTITY_INSERT [dbo].[WorkspacePerformingOrganization] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceResource] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceRestoreLog] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceStateHistory] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceUserRole] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceVariable] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceVariableSumVariableResourceTypeXREF] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceVersion] OFF
		SET IDENTITY_INSERT [dbo].[MSTTravelTripCustomFieldValueXREF] OFF
		SET IDENTITY_INSERT [dbo].[MSTTravelTrip] OFF
		SET IDENTITY_INSERT [dbo].[WorkspaceOffloadRate] OFF

		SELECT ERROR_MESSAGE()  AS ErrorMessage 		
		RETURN
	
	END CATCH
END
GO