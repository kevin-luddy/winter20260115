EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.34';
GO

/****** Object:  Table [dbo].[DisclosureTypeLU]    Script Date: 8/29/2023 3:42:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DisclosureTypeLU](
	[DisclosureTypeID] [int] IDENTITY(1, 1) NOT NULL,
	[DisclosureType] [varchar](50) NOT NULL,
 CONSTRAINT [PK_DisclosureTypeLU] PRIMARY KEY CLUSTERED 
(
	[DisclosureTypeID] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

/*** Generate Insert for DisclosureTypeLU ***/
SET IDENTITY_INSERT DisclosureTypeLU ON

INSERT INTO DisclosureTypeLU 
    (DisclosureTypeID, DisclosureType)
VALUES 
	(1, 'Legacy Space'),
	(2, '1LMX')

SET IDENTITY_INSERT DisclosureTypeLU OFF

GO

/*** Alter Table RateCode to include DisclosureType ID ***/
ALTER TABLE [dbo].[RateCode]
ADD [DisclosureTypeId] INT NULL
DEFAULT (1)

GO

/*** ALTER Table RateCode to ADD Foreign Key Constraint on newly added Column ***/
ALTER TABLE [dbo].[RateCode] WITH CHECK 
ADD CONSTRAINT FK_RateCode_DisclosureTypeLU
FOREIGN KEY (DisclosureTypeID) REFERENCES [dbo].[DisclosureTypeLU] ([DisclosureTypeID])

GO

ALTER TABLE [dbo].[RateCode] CHECK CONSTRAINT [FK_RateCode_DisclosureTypeLU]

/*** UPDATE Existing Table Records to Have 'Legacy Space' Value which is 1 ***/
GO

UPDATE RateCode SET DisclosureTypeID = 1

GO


USE [IES]
GO

/****** Object:  StoredProcedure [dbo].[insertRateCodeviaTableParameter]    Script Date: 9/5/2023 4:17:56 PM ******/
DROP PROCEDURE IF EXISTS [dbo].[insertRateCodeviaTableParameter]
GO

GO
/****** Object:  StoredProcedure [dbo].[updateRateCodeviaTableParameter]    Script Date: 9/5/2023 4:06:19 PM ******/
DROP PROCEDURE IF EXISTS [dbo].[updateRateCodeviaTableParameter]
GO

GO
/****** Object:  StoredProcedure [dbo].[deleteRateCodeviaTableParameter]    Script Date: 9/5/2023 4:15:49 PM ******/
DROP PROCEDURE IF EXISTS [dbo].[deleteRateCodeviaTableParameter]
GO

GO
/****** Object:  UserDefinedTableType [dbo].[TT_RateCode]    Script Date: 9/5/2023 4:03:51 PM ******/
DROP TYPE [dbo].[TT_RateCode]
GO

/****** Object:  UserDefinedTableType [dbo].[TT_RateCode]    Script Date: 9/5/2023 4:03:51 PM ******/
CREATE TYPE [dbo].[TT_RateCode] AS TABLE(
	[ID] [int] NOT NULL,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RevisionID] [int] NOT NULL,
	[CategoryID] [int] NOT NULL,
	[Description] [varchar](4000) NOT NULL,
	[SectionID] [int] NULL,
	[RateCode] [varchar](50) NOT NULL,
	[ResourceTypeID] [int] NULL,
	[GovernmentBurdenPoolID] [int] NULL,
	[CommercialBurdenPoolID] [int] NULL,
	[RateTypeID] [int] NULL,
	[DisclosureTypeId] [int] NULL,
	[OrderID] [int] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO

/****** Object:  StoredProcedure [dbo].[updateRateCodeviaTableParameter]    Script Date: 9/5/2023 4:06:19 PM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[updateRateCodeviaTableParameter]
(
@RateCodeParam [dbo].[TT_RateCode] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [updateRateCodeviaTableParameter]
**		Desc: Insert/Update data into RateCode Table
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		2/21/2018	brunworg			Removed CobraRateSet and CobraCode1ID.
**		9/05/2023	hrafiqzadah			Added DisclosureTypeId
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDate datetime2 = GETDATE()

SET @UpdateDate = GetDate()
			
			 
UPDATE [dbo].[RateCode]
	SET 
		[UpdateDate] = TT.[UpdateDate],
		[RevisionID] = TT.[RevisionID],
		[CategoryID] = TT.[CategoryID],
		[Description] = TT.[Description],
		[SectionID] = TT.[SectionID],
		[RateCode] = TT.[RateCode],
		[ResourceTypeID] = TT.[ResourceTypeID],
		[GovernmentBurdenPoolID] = TT.[GovernmentBurdenPoolID],
		[CommercialBurdenPoolID] = TT.[CommercialBurdenPoolID],
		[RateTypeID] = TT.[RateTypeID],
		[DisclosureTypeId] = TT.[DisclosureTypeId]
FROM [dbo].[RateCode] RC
	INNER JOIN @RateCodeParam TT ON 
		RC.ID = TT.ID 
				
IF @@ERROR = 0
	SELECT 
		RC.ID AS ID, 
		RC.UpdateDate AS UpdateDate 
	FROM @RateCodeParam TT
		INNER JOIN dbo.RateCode RC ON TT.ID = RC.ID
	ORDER BY OrderID
GO

/****** Object:  StoredProcedure [dbo].[deleteRateCodeviaTableParameter]    Script Date: 9/5/2023 4:15:49 PM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[deleteRateCodeviaTableParameter]
(
@RateCodeParam [dbo].[TT_RateCode] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteRateCodeviaTableParameter]
**		Desc: Delete data from RateCode Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		11/9/2017	ranzalon			Remove ProPricer Burden Rate mappings
**		9/05/2023	hrafiqzadah			Added DisclosureTypeId
*******************************************************************************/
SET NOCOUNT ON 

			DELETE FROM dbo.RateCodeYear
				FROM dbo.RateCodeYear RCY
				INNER JOIN dbo.RateCode RC ON RCY.RateCodeID = RC.ID
				INNER JOIN @RateCodeParam TT ON 
					RC.ID = TT.ID 

			DELETE FROM dbo.ProPricerRateCodeXref
				FROM dbo.ProPricerRateCodeXref PPX
				INNER JOIN dbo.RateCode RC ON PPX.RateCodeID = RC.ID
				INNER JOIN @RateCodeParam TT ON 
					RC.ID = TT.ID 

			DELETE FROM dbo.ProPricerBurdenRateMap
				FROM dbo.ProPricerBurdenRateMap BRM
				INNER JOIN dbo.RateCode RC ON BRM.RateCodeID = RC.ID
				INNER JOIN @RateCodeParam TT ON
					RC.ID = TT.ID
											
			DELETE FROM [dbo].[RateCode]
			FROM [dbo].[RateCode] RC
				INNER JOIN @RateCodeParam TT ON 
					RC.ID = TT.ID 

IF @@ERROR <> 0
BEGIN
DECLARE @ErrorMessage varchar (500)
SET @ErrorMessage =   'The RateCode Element(s) has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
 
		END
GO


/****** Object:  StoredProcedure [dbo].[insertRateCodeviaTableParameter]    Script Date: 9/5/2023 4:17:56 PM ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertRateCodeviaTableParameter]
(
@RateCodeParam [dbo].[TT_RateCode] READONLY
)
AS
SELECT * FROM @RateCodeParam
/******************************************************************************
**		 
**		Name: [insertRateCodeviaTableParameter]
**		Desc: Insert/Update data into RateCode table.
**			
**		
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**	    9/05/2023	hrafiqzadah			Updated table to include DisclosureType
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDate datetime2 = GETDATE()

/* Declare a @TT_RateCode table to store the incoming table with an additional OrderID for inserting children. */

DECLARE @TT_RateCode TABLE
(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RevisionID] [int] NOT NULL,
	[CategoryID] [int] NOT NULL,
	[Description] [varchar](4000) NOT NULL,
	[SectionID] [int] NULL,
	[RateCode] [varchar](50) NOT NULL,
	[ResourceTypeID] [int] NULL,
	[GovernmentBurdenPoolID] [int] NULL,
	[CommercialBurdenPoolID] [int] NULL,
	[RateTypeID] [int] NULL,
	[DisclosureTypeId] [int] NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
)

INSERT INTO @TT_RateCode
SELECT * FROM @RateCodeParam

DECLARE @ID [int],
		@RevisionID [int],
		@CategoryID [int],
		@Description [varchar](4000),
		@SectionID [int],
		@RateCode [varchar](50),
		@ResourceTypeID [int],
		@GovernmentBurdenPoolID [int],
		@CommercialBurdenPoolID [int],
		@RateTypeID [int],
		@DisclosureTypeId [int],
		@OrderID [int]

DECLARE @InsertedRateCode AS Table (ID int)

WHILE EXISTS (SELECT * FROM @TT_RateCode WHERE ID < 0)
BEGIN
	SELECT TOP 1 
     	@ID = ID,
		@RevisionID = RevisionID,
		@CategoryID = CategoryID,
		@Description = Description,
		@SectionID = SectionID,
		@RateCode = RateCode,
		@ResourceTypeID = ResourceTypeID,
		@GovernmentBurdenPoolID = GovernmentBurdenPoolID,
		@CommercialBurdenPoolID = CommercialBurdenPoolID,
		@RateTypeID = RateTypeID,
		@DisclosureTypeID = DisclosureTypeId,
		@OrderID =  OrderID
	FROM @TT_RateCode
	WHERE ID < 0

	INSERT INTO [dbo].[RateCode]
           ([UpdateDate]
		   ,[RevisionID]
           ,[CategoryID]
           ,[Description]
           ,[SectionID]
           ,[RateCode]
           ,[ResourceTypeID]
           ,[GovernmentBurdenPoolID]
           ,[CommercialBurdenPoolID]
           ,[RateTypeID]
		   ,[DisclosureTypeId]
		   )
     OUTPUT inserted.ID INTO @InsertedRateCode
     VALUES
           (@UpdateDate
		   ,@RevisionID
           ,@CategoryID
           ,@Description
           ,@SectionID
           ,@RateCode
           ,@ResourceTypeID
           ,@GovernmentBurdenPoolID
           ,@CommercialBurdenPoolID
           ,@RateTypeID
		   ,@DisclosureTypeId
            ) 
            
	SELECT @ID = ID FROM @InsertedRateCode
	
	UPDATE @TT_RateCode
		SET ID = @ID
	WHERE 
		@OrderID = OrderID AND
		ID < 0
END

IF @@ERROR = 0
	SELECT 
		TT.ID AS ID, 
		@UpdateDate AS UpdateDate
	FROM @TT_RateCode TT
		ORDER BY OrderID
GO




USE [IES]
GO
/****** Object:  StoredProcedure [dbo].[copyRevision]    Script Date: 10/25/2023 10:37:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[copyRevision]
(
	 @Id int
    ,@NewRevision varchar(1000)
	,@NewHistory nvarchar(max)
	,@NewCreatedBy varchar(1000)
	,@NewReleaseNotes nvarchar(max)
)
AS
	/******************************************************************************
	**		 
	**		Name:	[copyRevision]
	**		Desc:	Copy a PPR&D Revision.  This includes copying all of the 
	**				associated rates, COBRA/ProPricer Mappings, and PPR&D content.
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 7/10/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		08/01/2017	brunworg			Added StartYear and EndYear columns,
	**										removed IsDeleted from RateCode table.
	**		08/10/2017	brunworg			Remove transaction handling.
	**		08/10/2017	brunworg			BOEJ-2453 - Fix bug so ProPricerBurdenRateMap 
	**										gets RateCodeID values for new revision.
	**		08/15/2017  tglick				modified to pull new burdenpoolIds
	**										for comm/govt burden pools on new ratecodes
	**		08/21/2017	brunworg			Remove AlternateDescription, DataTypeID,
	**										and DataFormat from RateCode table.
	**		08/21/2017	brunworg			Remove PPRD, Document, DocumentTypeLU,
	**										CostVolume, and CostVolumeRateCode tables.
	**		08/21/2017	brunworg			Add "IsInternal" field to Section table.
	**		08/22/2017	brunworg			Remove "IsDeleted" field from all tables.
	**		08/22/2017	brunworg			Redesign Section and related tables.
	**		09/19/2017	brunworg			Fix bug that caused duplicate sections.
	**		10/05/2017	ranzalon			Update for History and Release Notes
	**		10/10/2017	Dusan				Added RevisionUniqueSectionId
	**		01/02/2018	twilson3			BOEJ-2704 File Attachments
	**		01/04/2018	ranzalon			BOEJ-2698 Burden Pool Categorization - 
	**										Updated for IsCommercial bit
	**		01/18/2018	Dusan				Added ResourceClassID into copying
	**		05/15/2018	ranzalon			Added IsRdsbRequired for sections
	**		08/21/2019	ranzalon			Added IncludeGaT2InBurdAndCommBurdTables
	**										for burden pools
	**		07/07/2022	Dusan				Added SectionContainsCasbDisclosure and SectionContainsNonCompliance
	**		10/26/2023	May				    Add ability address table fields
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;
	DECLARE @StartYear int, @EndYear int;
	
	SELECT @StartYear = StartYear, @EndYear = EndYear FROM [dbo].Revision WHERE ID = @Id;
	IF @@ROWCOUNT = 0	
		BEGIN
			SET @ErrorMessage = 'Copy failed - Revision could not be found.'
			RAISERROR (
					@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN		
		END
	ELSE
		BEGIN
			DECLARE @RevisionID INT;
			DECLARE @RevisionIdResultSet table (ID INT);
			DECLARE @SectionMap AS TABLE (OldId Int, [NewId] Int);
			DECLARE @RateCodeMap AS TABLE (OldId Int, [NewId] Int);
			DECLARE @BurdenPoolMap as TABLE (oldId Int, [NewId] Int); -- burden pools (comm/govt) for new ratecodes 

			BEGIN TRY
				-- Upsert New Revision
				INSERT INTO @RevisionIdResultSet (ID) EXECUTE [dbo].upsertRevision -1, null, @NewRevision, @NewHistory, @NewCreatedBy, @StartYear, @EndYear, @NewReleaseNotes;
				SELECT TOP 1 @RevisionID = ID FROM @RevisionIdResultSet;

				-- Copy associated PPR&D document (all sections and associated content)
				INSERT INTO [dbo].Section
				(UpdateDate, RevisionID, ParentID, DisplayOrder, Title, TextContent, SectionContentTypeID, [Office], [Agency], [LMBA],[Name],[Street],[CityST] ,[Phone] ,[Email],[Other],[IncludeInCoversheet], IsInternalSection, DisplayRateCode, RevisionUniqueSectionId, IsRdsbRequired, SectionContainsCasbDisclosure, SectionContainsNonCompliance)
				OUTPUT Inserted.ParentID, Inserted.Id INTO @SectionMap
				SELECT GETDATE() AS UpdateDate,
								@RevisionID as RevisionID,
								Id AS ParentID, -- Note that we save the old Id in the new ParentID.
								DisplayOrder, Title, TextContent, SectionContentTypeID, [Office], [Agency], [LMBA],[Name],[Street],[CityST] ,[Phone] ,[Email],[Other],[IncludeInCoversheet], IsInternalSection, DisplayRateCode, RevisionUniqueSectionId, IsRdsbRequired, SectionContainsCasbDisclosure, SectionContainsNonCompliance
				FROM dbo.Section 
				WHERE RevisionID = @ID;

				-- Apply the section Map to update ParentID values for section hierarchy
				UPDATE SectionNew
				SET ParentID = MapNew.[NewId]
				FROM @SectionMap AS M
				INNER JOIN [dbo].Section AS SectionNew ON SectionNew.Id = M.[NewId] -- ROWS we need to fix
				INNER JOIN @SectionMap AS MapOld ON MapOld.OldId = SectionNew.ParentID
				INNER JOIN [dbo].Section AS SectionOld ON SectionOld.Id = M.[OldId] 
				LEFT OUTER JOIN @SectionMap AS MapNew ON MapNew.OldId = SectionOld.ParentID;

				-- Copy File Attachment entries for the revision

				INSERT INTO [dbo].FileAttachment
				(UpdateDate
				,[Name]
				,[Link]
				,[SectionId]
				,[RevisionId])
				SELECT GETDATE() AS UpdateDate,
				faOld.Name,
				faOld.Link,
				m.NewID as SectionId,
				@RevisionID as RevisionID
				FROM [dbo].FileAttachment AS faOld
				LEFT OUTER JOIN @SectionMap m on faOld.SectionId = m.OldId
				WHERE faOld.RevisionId = @Id

				-- Copy BurdenPoolLU entries for the revision
				MERGE
					[dbo].[BurdenPoolLU] as bp
				USING(
					SELECT bpOld.* 
					FROM [dbo].[BurdenPoolLU] as bpOld
					WHERE bpOld.RevisionID = @Id ) as x
				ON (1=0)
				WHEN NOT MATCHED
					THEN INSERT (UpdateDate, BurdenPool, [Description],  IsGaT2ApplicableForMissionSolutions, IncludeGaT2InBurdAndCommBurdTables, RevisionID, IsCommercial, [ExcludeFCCOM])
					VALUES(GETDATE(), BurdenPool, [Description], IsGaT2ApplicableForMissionSolutions, IncludeGaT2InBurdAndCommBurdTables, @RevisionID, IsCommercial, [ExcludeFCCOM])
				OUTPUT x.[ID], Inserted.Id INTO @BurdenPoolMap;

				-- Copy rates for the revision
				MERGE
					[dbo].[RateCode] AS rc
				USING(
					SELECT rcOld.*, m.NewID as newSectionID
					FROM [dbo].[RateCode] AS rcOld
					LEFT OUTER JOIN @SectionMap m on rcOld.SectionID = m.OldId
					WHERE rcOld.RevisionID = @Id) as x
				ON (1=0)  
				WHEN NOT MATCHED   
					THEN INSERT (UpdateDate, RevisionID, CategoryID, Description,
						   SectionID, RateCode, ResourceTypeID, 
						   GovernmentBurdenPoolID, CommercialBurdenPoolID, 
						   RateTypeID, CobraRateSet, CobraCode1ID)
					VALUES (GETDATE(), @RevisionID, CategoryID, Description,
						   newSectionID, RateCode, ResourceTypeID,
						   -- add new ids for GovernmentBurdenPoolId and CommercialBurdenPoolId
						   ( SELECT [NewId] FROM @BurdenPoolMap WHERE OldId = GovernmentBurdenPoolId),
						   ( SELECT [NewId] FROM @BurdenPoolMap WHERE OldId = CommercialBurdenPoolId),
						   RateTypeID, CobraRateSet, CobraCode1ID)
				OUTPUT x.[ID], Inserted.Id INTO @RateCodeMap;

				-- Copy Rate code years for the revision
				INSERT INTO [dbo].[RateCodeYear] ([UpdateDate],[RateCodeID],[Year],[Rate])
				SELECT GETDATE(), rcMap.NewID, Year, Rate
				FROM [dbo].[RateCodeYear] rcyOld
				LEFT OUTER JOIN @RateCodeMap rcMap on rcyOld.RateCodeID = rcMap.OldId
				JOIN [dbo].RateCode rc on rc.ID = rcyOld.RateCodeID
				WHERE rc.RevisionId = @Id;

				-- Copy ProPricer mapping and cross reference information for the revision
				INSERT INTO [dbo].[ProPricerBurdenRateMap]
						   ([UpdateDate],[BurdenPoolID],[BurdenElementID],[RateCodeID])
				SELECT GETDATE(), bpNew.Id as BurdenPoolID, ppOld.BurdenElementID, rcMap.NewID 
				FROM [dbo].[ProPricerBurdenRateMap] ppOld
				join  [dbo].[BurdenPoolLU] bpOld on bpOld.ID = ppOld.BurdenPoolID and bpOld.RevisionID = @Id
				join  [dbo].[BurdenPoolLU] bpNew on bpOld.BurdenPool = bpNew.BurdenPool and bpNew.RevisionID = @RevisionID
				LEFT OUTER JOIN @RateCodeMap rcMap on ppOld.RateCodeID = rcMap.OldId;

				INSERT INTO [dbo].[ProPricerRateCodeXref] ([UpdateDate],[RateCodeID],[Description],[RateCodeExtensionID], [ResourceClassID])
				SELECT GETDATE(), rcMap.NewID, ppOld.Description, RateCodeExtensionID, ResourceClassID
				FROM [dbo].[ProPricerRateCodeXref] ppOld
				LEFT OUTER JOIN @RateCodeMap rcMap on ppOld.RateCodeID = rcMap.OldId
				JOIN [dbo].RateCode rc on rc.ID = ppOld.RateCodeID
				WHERE rc.RevisionId = @Id;
				
			END TRY
			BEGIN CATCH
				SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(), @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE();

				IF @ErrorMessage IS NULL
					BEGIN
						SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' could not be copied.'
					END
								
				RAISERROR (
						@ErrorMessage, -- Message text.
						@ErrorSeverity, -- Severity,
						@ErrorState -- State,
						)
				RETURN
			END CATCH;
		END

	IF @@ERROR = 0
		SELECT @RevisionId as Id, @NewRevision as Revision;

GO