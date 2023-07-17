PRINT '###### SCRIPT IS STARTING ######';
/*
    This file was auto-generated for Release: 2023.x, on 7/12/2023.
    It contains all of the Release specific scripts, modifying data/tables as well as all of the Stored Procedures and User Defined Table Types.
*/

/*
    File: \Stored Procedures\copyRevision.sql
*/
PRINT '### Starting file: \Stored Procedures\copyRevision.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[copyRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[copyRevision];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[copyRevision]
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
				(UpdateDate, RevisionID, ParentID, DisplayOrder, Title, TextContent, SectionContentTypeID, IsInternalSection, DisplayRateCode, RevisionUniqueSectionId, IsRdsbRequired, SectionContainsCasbDisclosure, SectionContainsNonCompliance)
				OUTPUT Inserted.ParentID, Inserted.Id INTO @SectionMap
				SELECT GETDATE() AS UpdateDate,
								@RevisionID as RevisionID,
								Id AS ParentID, -- Note that we save the old Id in the new ParentID.
								DisplayOrder, Title, TextContent, SectionContentTypeID, IsInternalSection, DisplayRateCode, RevisionUniqueSectionId, IsRdsbRequired, SectionContainsCasbDisclosure, SectionContainsNonCompliance
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

/*
    File: \Stored Procedures\deleteBanner.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteBanner.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBanner]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBanner];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteBanner]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteBanner]
	**		Desc:	Delete a Banner
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 3/14/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	
	IF (SELECT UpdateDate FROM [dbo].[Banner] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.[Banner] WHERE ID = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Banner with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\deleteBurdenPoolLU.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteBurdenPoolLU.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteBurdenPoolLU]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteBurdenPoolLU];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteBurdenPoolLU]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteBurdenPoolLU]
	**		Desc:	Delete a Burden Pool 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		08/03/2017	brunworg			Change stored procedure name.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[BurdenPoolLU] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			UPDATE dbo.RateCode SET GovernmentBurdenPoolId = null WHERE GovernmentBurdenPoolId = @Id
			UPDATE dbo.RateCode SET CommercialBurdenPoolId = null WHERE CommercialBurdenPoolId = @Id
			DELETE FROM dbo.ProPricerBurdenRateMap WHERE BurdenPoolID = @Id
			DELETE FROM dbo.BurdenPoolLU WHERE ID = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Burden Pool with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\deleteCobraFiscalYear.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteCobraFiscalYear.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteCobraFiscalYear]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteCobraFiscalYear];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteCobraFiscalYear]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteCobraFiscalYear]
	**		Desc:	Delete a COBRA Fiscal Year
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before delete.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[CobraFiscalYearLU] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.CobraFiscalYearLU WHERE Id = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The COBRA Fiscal Year with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END

GO


/*
    File: \Stored Procedures\deleteFileAttachment.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteFileAttachment.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteFileAttachment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteFileAttachment];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteFileAttachment]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteFileAttachment]
	**		Desc:	Delete a File Attachment
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 1/2/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	
	IF (SELECT UpdateDate FROM [dbo].[FileAttachment] WHERE FileAttachmentId = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.[FileAttachment] WHERE FileAttachmentId = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The File Attachment with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\deleteProPricerBurdenRateMap.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteProPricerBurdenRateMap.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProPricerBurdenRateMap]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProPricerBurdenRateMap];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProPricerBurdenRateMap]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteProPricerBurdenRateMap]
	**		Desc:	Delete a ProPricer Burden Rate Mapping
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before delete.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[ProPricerBurdenRateMap] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.ProPricerBurdenRateMap WHERE Id = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The ProPricer Burden Rate Mapping with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\deleteProPricerRateCodeXref.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteProPricerRateCodeXref.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProPricerRateCodeXref]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProPricerRateCodeXref];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteProPricerRateCodeXref]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteProPricerRateCodeXref]
	**		Desc:	Delete a ProPricer Rate Code cross reference
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before delete.
	*******************************************************************************/
	SET NOCOUNT ON 

	IF (SELECT UpdateDate FROM [dbo].[ProPricerRateCodeXref] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.ProPricerRateCodeXref WHERE Id = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The ProPricer Rate Code Cross Reference with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\deleteRDSBDocumentInformation.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteRDSBDocumentInformation.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRDSBDocumentInformation]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRDSBDocumentInformation];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteRDSBDocumentInformation]
(
	@Id	INT,
	@LastUpdateDT datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteRDSBDocumentInformation]
	**		Desc:	Delete RDSB Document Information
	**			
	**		
	**
	**		Auth: ranzalon
	**		Date: 1/25/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		02/19/2018	ranzalon			Updated to delete Xrefs
	*******************************************************************************/
	SET NOCOUNT ON 
	
	IF (SELECT LastUpdateDT FROM [dbo].[RDSBDocumentInformation] WHERE ID = @Id ) = @LastUpdateDT
		BEGIN
			DELETE FROM dbo.RDSBRateCodeXref WHERE RDSBDocumentInformationID = @Id;
			DELETE FROM dbo.RDSBSectionXref WHERE RDSBDocumentInformationID = @Id;
			DELETE FROM dbo.RDSBDocumentInformation WHERE ID = @Id;
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500);
			SET @ErrorMessage = 'The RDSB Document Information with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.';
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					);
			RETURN
		END
GO

/*
    File: \Stored Procedures\deleteReplication.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteReplication.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteReplication]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteReplication];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteReplication]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteReplication]
	**		Desc:	Delete a Rate Code Replication
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 3/14/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	
	IF (SELECT UpdateDate FROM [dbo].[RateCodeReplication] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			DELETE FROM dbo.[RateCodeReplication] WHERE ID = @Id
		END
	ELSE
		BEGIN
			DECLARE @ErrorMessage varchar (500)
				SET @ErrorMessage =   'The Rate Code Replication with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\deleteRevision.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteRevision.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRevision];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteRevision]
(
	@Id			int,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteRevision]
	**		Desc:	Delete a PPR&D Revision
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Corrected order of delete statements.
	**										Added code to delete associated Document
	**										and Sections.
	**		7/10/2017	brunworg			Added code to raise an exception if
	**										deleting Revision with associated 
	**										CostVolume.
	**		7/10/2017	brunworg			Remove references to ActivityTypeMap and
	**										ProPricerActivityTypeXref tables.
	**		8/04/2017	brunworg			Updated to reflect new data model with
	**										revisionId in BurdenPoolLU table.
	**		8/10/2017	brunworg			Remove transaction handling.
	**		8/21/2017	brunworg			Remove PPRD, Document, DocumentTypeLU,
	**										CostVolume, and CostVolumeRateCode tables.
	**		8/22/2017	brunworg			Redesign Section and related tables.
	**		8/24/2017	brunworg			Updated to delete ProPricerBurdenPoolMap  
	**										and ProPricerRateCodeXref entries before 
	**										RateCodes.
	**		9/8/2017	brunworg			Modified ProPricerBurdenRateMap delete statement.
	**		1/2/2018	twilson3			BOEJ-2704 File Attachments
	**		1/25/2018	ranzalon			BOEJ-2715 - RDSB Document Information
	**      7/12/2023	twilson3			PROPH-917 - Delete XRefs for RateCode and Section
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;

	IF (SELECT UpdateDate FROM [dbo].[Revision] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			BEGIN TRY
				DELETE FROM [dbo].[FileAttachment] WHERE [RevisionId] = @Id
				DELETE FROM [dbo].[ProPricerBurdenRateMap] 
				WHERE EXISTS
					(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
				DELETE FROM [dbo].[ProPricerRateCodeXref] 
				WHERE EXISTS 
					(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
				DELETE FROM [dbo].[RateCodeYear] 
				WHERE EXISTS 
					(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
				DELETE FROM [dbo].[RDSBRateCodeXref] 
				WHERE EXISTS
					(SELECT * FROM [dbo].RateCode rc where  RateCodeID = rc.ID AND rc.RevisionID = @Id)
				DELETE FROM [dbo].[RateCode] WHERE RevisionID = @Id
				DELETE FROM [dbo].[BurdenPoolLU] where RevisionID = @Id	
				DELETE FROM [dbo].[RDSBSectionXref]
				WHERE EXISTS
					(SELECT * FROM [dbo].[Section] sec where  SectionID = sec.ID AND sec.RevisionID = @Id)
				DELETE FROM [dbo].[Section] WHERE RevisionID = @Id
				DELETE FROM [dbo].[RDSBDocumentInformation] WHERE RDMRevisionID = @Id
				DELETE FROM [dbo].[Revision] WHERE ID = @Id
			END TRY
			BEGIN CATCH
				SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(), @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE();

				SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' could not be deleted.'
				RAISERROR (
						@ErrorMessage, -- Message text.
						@ErrorSeverity, -- Severity,
						@ErrorState -- State,
						)
				RETURN
			END CATCH;
		END
	ELSE
		BEGIN
			SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO

/*
    File: \Stored Procedures\deleteSection.sql
*/
PRINT '### Starting file: \Stored Procedures\deleteSection.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSection]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSection];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[deleteSection]
(
	@Id			INT,
	@UpdateDate datetime2
)
AS
	/******************************************************************************
	**		 
	**		Name:	[deleteSection]
	**		Desc:	Delete a document section
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 7/6/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		08/10/2017	brunworg			Remove transaction handling.
	**		08/22/2017	brunworg			Redesign Section and related tables.
	**		09/07/2017	brunworg			Modified to delete child sections.
	**		01/30/2018	brunworg			Remove associated RateCodes before deleting sections.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;

	IF (SELECT UpdateDate FROM [dbo].[Section] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			-- Retrieve section and all child sections.  
			-- Use a Common Table Expression (CTE) to gather all of the hierarchical section IDs.
			DECLARE @TempSectionIDs TABLE (ID INT, UpdateDate datetime2(7), level INT);

			WITH CteSectionIDs (ParentID, ID, UpdateDate, level)
			AS
			(
				-- start with specified section ID
				SELECT ParentID, ID, UpdateDate, 0 as level FROM dbo.[Section] WHERE ID = @ID
				UNION ALL
				-- recursive child sections
				SELECT child.ParentID, child.Id, child.UpdateDate, level + 1 FROM dbo.[Section] as child INNER JOIN CteSectionIDs as parent ON child.ParentID = parent.ID
			)
			-- Statement that executes the CTE
			INSERT INTO @TempSectionIDs (ID, UpdateDate, level)
			SELECT ID, UpdateDate, level from CteSectionIDs;

			-- Remove any rate code associations with the sections being deleted.
			UPDATE [dbo].[RateCode]
			SET SectionID = null
			WHERE SectionID in (SELECT ID from @TempSectionIDs);

			-- Delete the sections found by the CTE
			BEGIN TRY
				DECLARE @SectionId int, @SectionUpdateDate datetime2(7);
				DECLARE cur CURSOR LOCAL FOR
					SELECT s.ID, s.Updatedate 
					FROM [dbo].Section s
					JOIN @TempSectionIDs tmp on s.ID = tmp.ID
					ORDER BY tmp.level desc;

				OPEN cur
				FETCH NEXT FROM cur INTO @SectionId, @SectionUpdateDate

				WHILE @@FETCH_STATUS = 0 BEGIN
					DELETE FROM [dbo].[Section] WHERE Id = @SectionId AND UpdateDate = @SectionUpdateDate
					FETCH NEXT FROM cur INTO @SectionId, @SectionUpdateDate
				END
				CLOSE cur;
				DEALLOCATE cur;
			END TRY
			BEGIN CATCH
				SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(), @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE();

				SET @ErrorMessage =   'The Section with ID ' + CAST(@Id  AS varchar(10)) + ' (and child sections) could not be deleted.'
				RAISERROR (
						@ErrorMessage, -- Message text.
						@ErrorSeverity, -- Severity,
						@ErrorState -- State,
						)
				RETURN
			END CATCH;
		END
	ELSE
		BEGIN
			SET @ErrorMessage =   'The Section with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\ELMAH_GetErrorsXml.sql
*/
PRINT '### Starting file: \Stored Procedures\ELMAH_GetErrorsXml.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_GetErrorsXml]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ELMAH_GetErrorsXml];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ELMAH_GetErrorsXml]
(
    @Application NVARCHAR(60),
    @PageIndex INT = 0,
    @PageSize INT = 15,
    @TotalCount INT OUTPUT
)
AS 
/******************************************************************************
**		 
**		Name: [ELMAH_GetErrorsXml]
**		Desc:	Gets a group of elmah errors in xml format
**			
**		
**
**		Auth: Tim Wilson
**		Date: 11/22/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**
*******************************************************************************/
    SET NOCOUNT ON

    DECLARE @FirstTimeUTC DATETIME
    DECLARE @FirstSequence INT
    DECLARE @StartRow INT
    DECLARE @StartRowIndex INT

    SELECT 
        @TotalCount = COUNT(1) 
    FROM 
        [ELMAH_Error]
    WHERE 
        [Application] = @Application

    -- Get the ID of the first error for the requested page

    SET @StartRowIndex = @PageIndex * @PageSize + 1

    IF @StartRowIndex <= @TotalCount
    BEGIN

        SET ROWCOUNT @StartRowIndex

        SELECT  
            @FirstTimeUTC = [TimeUtc],
            @FirstSequence = [Sequence]
        FROM 
            [ELMAH_Error]
        WHERE   
            [Application] = @Application
        ORDER BY 
            [TimeUtc] DESC, 
            [Sequence] DESC

    END
    ELSE
    BEGIN

        SET @PageSize = 0

    END

    -- Now set the row count to the requested page size and get
    -- all records below it for the pertaining application.

    SET ROWCOUNT @PageSize

    SELECT 
        errorId     = [ErrorId], 
        application = [Application],
        host        = [Host], 
        type        = [Type],
        source      = [Source],
        message     = [Message],
        [user]      = [User],
        statusCode  = [StatusCode], 
        time        = CONVERT(VARCHAR(50), [TimeUtc], 126) + 'Z'
    FROM 
        [ELMAH_Error] error
    WHERE
        [Application] = @Application
    AND
        [TimeUtc] <= @FirstTimeUTC
    AND 
        [Sequence] <= @FirstSequence
    ORDER BY
        [TimeUtc] DESC, 
        [Sequence] DESC
    FOR
        XML AUTO

GO

/*
    File: \Stored Procedures\ELMAH_GetErrorXml.sql
*/
PRINT '### Starting file: \Stored Procedures\ELMAH_GetErrorXml.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_GetErrorXml]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ELMAH_GetErrorXml];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ELMAH_GetErrorXml]
(
    @Application NVARCHAR(60),
    @ErrorId UNIQUEIDENTIFIER
)
AS
/******************************************************************************
**		 
**		Name: [ELMAH_GetErrorXml]
**		Desc:	Gets a specific elmah error in xml format
**			
**		
**
**		Auth: Tim Wilson
**		Date: 11/22/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**
*******************************************************************************/
    SET NOCOUNT ON

    SELECT 
        [AllXml]
    FROM 
        [ELMAH_Error]
    WHERE
        [ErrorId] = @ErrorId
    AND
        [Application] = @Application

GO


/*
    File: \Stored Procedures\ELMAH_LogError.sql
*/
PRINT '### Starting file: \Stored Procedures\ELMAH_LogError.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_LogError]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ELMAH_LogError];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ELMAH_LogError]
(
    @ErrorId UNIQUEIDENTIFIER,
    @Application NVARCHAR(60),
    @Host NVARCHAR(30),
    @Type NVARCHAR(100),
    @Source NVARCHAR(60),
    @Message NVARCHAR(500),
    @User NVARCHAR(50),
    @AllXml NTEXT,
    @StatusCode INT,
    @TimeUtc DATETIME
)
AS
/******************************************************************************
**		 
**		Name:   [ELMAH_LogError]
**		Desc:	Creates an Error Log entry in ELMAH_Error table
**			
**		
**
**		Auth: Tim Wilson
**		Date: 11/22/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**
*******************************************************************************/
    SET NOCOUNT ON

	INSERT
    INTO
        [ELMAH_Error]
        (
            [ErrorId],
            [Application],
            [Host],
            [Type],
            [Source],
            [Message],
            [User],
            [AllXml],
            [StatusCode],
            [TimeUtc]
        )
    VALUES
        (
            @ErrorId,
            @Application,
            @Host,
            @Type,
            @Source,
            @Message,
            @User,
            @AllXml,
            @StatusCode,
            @TimeUtc
        )

GO


/*
    File: \Stored Procedures\ExportCobraRates.sql
*/
PRINT '### Starting file: \Stored Procedures\ExportCobraRates.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportCobraRates]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ExportCobraRates];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ExportCobraRates]
(
	@RevisionID INT
)
AS
/******************************************************************************
**		 
**		Name: [genBOE].[ExportCobraRates]
**		Desc: Generate COBRA Rate Export file
**			
**      TODO - Modify to only export changed rates. 
**             Note: This will probably end up being generated via C# code
**                   instead of a stored procedure. But for now, this will  
**                   serve as an example of how to export COBRA rates.
**		
**
**		Auth: brunworg
**		Date: 3/20/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		08/03/2017	brunworg			Updated to reflect new data model with
**										cobraCode1ID in RateCode table.
**		08/22/2017	brunworg			Redesign Section and related tables.
*******************************************************************************/
SET NOCOUNT ON 

SELECT rc.CobraRateSet, cclu.Description, rc.Description, cfylu.FiscalYearStartDate as Date, rcy.Rate as Value
FROM [dbo].[Revision] rev
JOIN [dbo].[RateCode] rc on rc.[RevisionID] = rev.[ID]
JOIN [dbo].[CobraCode1LU] cclu on rc.CobraCode1ID = cclu.ID
JOIN [dbo].[RateCodeYear] rcy on rc.[ID] = rcy.[RateCodeID]
JOIN [dbo].[CobraFiscalYearLU] cfylu on rcy.[Year] = cfylu.Year
WHERE rev.[ID] = @RevisionID and rc.CobraRateSet is not null
ORDER BY rc.RateCode;

GO

/*
    File: \Stored Procedures\ExportProPricerBurdenRates.sql
*/
PRINT '### Starting file: \Stored Procedures\ExportProPricerBurdenRates.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportProPricerBurdenRates]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ExportProPricerBurdenRates];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ExportProPricerBurdenRates]
(
	@RevisionID INT
)
AS	
/******************************************************************************
**		 
**		Name: [genBOE].[ExportProPricerBurdenRates]
**		Desc: Generate ProPricer Burden Rate Export file
**			
**      TODO - Determine why certain years are exported, but others are not? 
**		
**
**		Auth: brunworg
**		Date: 3/20/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/21/2017	brunworg			Updated to reflect new data model.
**                                      Changed ESCAL column from being a 
**                                      hard-coded placeholder to one of the 
**                                      burden element columns.
**		08/03/2017	brunworg			Updated to reflect new data model with
**										revisionId in BurdenPoolLU table.
*******************************************************************************/
SET NOCOUNT ON 

SELECT * FROM (
  SELECT bplu.[BurdenPool], bplu.[Description]
	,'' as EffectiveDate
	,rcy.[Year] as Date
	,belu.[BurdenElement] as BurdenElement, rcy.[Rate]
  FROM [dbo].[Revision] rev
  JOIN [dbo].[BurdenPoolLU] bplu on bplu.RevisionId = rev.[ID]
  JOIN [dbo].[ProPricerBurdenRateMap] map on map.[BurdenPoolID] = bplu.[ID]
  JOIN [dbo].[RateCode] rc on map.[RateCodeId] = rc.[ID]
  JOIN [dbo].[RateCodeYear] rcy on rc.[ID] = rcy.[RateCodeID]
  JOIN [dbo].[BurdenElementLU] belu on map.[BurdenElementID] = belu.[ID]
  WHERE rev.[ID] = @RevisionID) as src
  PIVOT (max(src.[Rate]) for src.[BurdenElement] in ([ESCAL],[OH Dev],[OH Prod],[OH FBM],[OH LVS],
			[OH Hunts],[OH Offsite],[OH Michoud],[OH Mich MH],[OH Prg Uni],[OH Pro DIR],
			[OH Pro MSC],[OH Pro Serv],[OH On Serv],[OH Off Serv],[OH PH4],[OH PH5],
			[Fringe],[Fringe T2],[FRG Serv Corp],[FRG Serv LMOS],[G&A],[G&A T2],
			[FCCOM Dev],[FCCOM Prod],[FCCOM FBM],[FCCOM LVS],[FCCOM Hunt],[FCCOM Ofst],
			[FCCOM Mich],[FCM Prg Un],[FCCOM Proc],[FCCOM ProM],[FCCOM G&A],[FCM T2 G&A],
			[FCCM IWTA],[FCCM PH2],[FCCM PH3],[FCCM PH4],[Fee/Prft])) as piv;

GO



/*
    File: \Stored Procedures\ExportProPricerDirectRates.sql
*/
PRINT '### Starting file: \Stored Procedures\ExportProPricerDirectRates.sql';
 IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportProPricerDirectRates]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[ExportProPricerDirectRates];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ExportProPricerDirectRates]
(
	@RevisionID INT
)
AS	
/******************************************************************************
**		 
**		Name: [genBOE].[ExportProPricerDirectRates]
**		Desc: Generate ProPricer Direct Rate Export data for both Government
**            and Commercial rates.
**            The results will be a UNION of the following:
**            1) Rate Codes that do not need Rate Code Extensions.
**            2) Rate Codes that are mapped by activity type and
**               need to be expanded using the Rate Code Extensions.
**            3) Pro Pricer Equivalent Rate Codes.
**               The rates for these activity types will be
**               exported a second time substituting the first 4 characters
**               of the rate code as follows.
**	    	      XXDD => XADD
**	    	      XXLM => XMLM
**	    	      XXZD => XCZD
**	    	      XXZP => XCZP
**		
**
**		Auth: brunworg
**		Date: 3/20/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/21/2017	brunworg			Updated to reflect new data model.
**		7/12/2017	brunworg			Changed RateCodeYear - Year field
**                                      from varchar(50) to int.
**		08/22/2017	brunworg			Redesign Section and related tables.
*******************************************************************************/
SET NOCOUNT ON 

SELECT reslu.[Description], '01/' + CAST(rcy.[Year] as varchar(4)) as StartDate, '12/' + CAST(rcy.[Year] as varchar(4)) as EndDate
	  ,rc.[RateCode] + ISNULL(rcelu.[RateCodeExtension],'') as Resource  
      ,xref.[Description]
      ,'' as ResourceClass
	  ,ISNULL(bplu1.BurdenPool, '') as GovernmentBurdenPool
	  ,ISNULL(bplu2.BurdenPool, '') as CommercialBurdenPool
	  ,rtlu.[Description]
	  ,'' as EffectiveDate
	  ,rcy.[Rate] as BaseRate
	  ,0 as Step
	  ,0 as Factor
  FROM [dbo].[Revision] rev
  JOIN [dbo].[RateCode] rc on rc.[RevisionID] = rev.[ID]
  JOIN [dbo].[ProPricerRateCodeXref] xref on rc.ID = xref.RateCodeID
  LEFT OUTER JOIN [dbo].[RateCodeExtensionLU] rcelu on xref.RateCodeExtensionID = rcelu.ID
  JOIN [dbo].[ResourceTypeLU] reslu on rc.[ResourceTypeID] = reslu.[ID]
  JOIN [dbo].[RateTypeLU] rtlu on rc.[RateTypeID] = rtlu.[ID]
  JOIN [dbo].[RateCodeYear] rcy on rc.[ID] = rcy.[RateCodeID]
  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu1 on rc.[GovernmentBurdenPoolID] = bplu1.[ID]
  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu2 on rc.[CommercialBurdenPoolID] = bplu2.[ID]
  WHERE rev.[ID] = @RevisionID
UNION
SELECT reslu.[Description], '01/' + CAST(rcy.[Year] as varchar(4)) as StartDate, '12/' + CAST(rcy.[Year] as varchar(4)) as EndDate
	  ,REPLACE(REPLACE(REPLACE(REPLACE(rc.[RateCode],'XXDD','XXAD'),'XXLM','XMLM'),'XXZD','XCZD'),'XXZP','XCZP') + ISNULL(rcelu.[RateCodeExtension],'') as Resource 
      ,xref.[Description]
      ,'' as ResourceClass
	  ,ISNULL(bplu1.BurdenPool, '') as GovernmentBurdenPool
	  ,ISNULL(bplu2.BurdenPool, '') as CommercialBurdenPool
	  ,rtlu.[Description]
	  ,'' as EffectiveDate
	  ,rcy.[Rate] as BaseRate
	  ,0 as Step
	  ,0 as Factor
  FROM [dbo].[Revision] rev
  JOIN [dbo].[RateCode] rc on rc.[RevisionID] = rev.[ID]
  JOIN [dbo].[ProPricerRateCodeXref] xref on rc.ID = xref.RateCodeID
  LEFT OUTER JOIN [dbo].[RateCodeExtensionLU] rcelu on xref.RateCodeExtensionID = rcelu.ID
  JOIN [dbo].[ResourceTypeLU] reslu on rc.[ResourceTypeID] = reslu.[ID]
  JOIN [dbo].[RateTypeLU] rtlu on rc.[RateTypeID] = rtlu.[ID]
  JOIN [dbo].[RateCodeYear] rcy on rc.[ID] = rcy.[RateCodeID]
  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu1 on rc.[GovernmentBurdenPoolID] = bplu1.[ID]
  LEFT OUTER JOIN [dbo].[BurdenPoolLU] bplu2 on rc.[CommercialBurdenPoolID] = bplu2.[ID]
  WHERE rev.[ID] = @RevisionID AND LEFT(rc.RateCode,4) in ('XXDD', 'XXLM', 'XXZD', 'XXZP')
ORDER BY Resource, EndDate;

GO

/*
    File: \Stored Procedures\GetProPricerBurdenRateCodes.sql
*/
PRINT '### Starting file: \Stored Procedures\GetProPricerBurdenRateCodes.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetProPricerBurdenRateCodes]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[GetProPricerBurdenRateCodes];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetProPricerBurdenRateCodes]
(
	@RevisionID INT
)
AS	
/******************************************************************************
**		 
**		Name: [genBOE].[GetProPricerBurdenRateCodes]
**		Desc: Get ProPricer Burden Rate Codes
**		
**
**		Auth: brunworg
**		Date: 3/26/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/21/2017	brunworg			Added ESCAL burden element column.
**		08/03/2017	brunworg			Updated to reflect new data model with
**										revisionId in BurdenPoolLU table.
*******************************************************************************/
SET NOCOUNT ON 

SELECT * FROM (
  SELECT bplu.[BurdenPool], bplu.[Description]
	,belu.[BurdenElement] as BurdenElement, rc.[RateCode]
  FROM [dbo].[Revision] rev
  JOIN [dbo].[BurdenPoolLU] bplu on bplu.RevisionId = rev.[ID]
  JOIN [dbo].[ProPricerBurdenRateMap] map on map.[BurdenPoolID] = bplu.[ID]
  JOIN [dbo].[RateCode] rc on map.[RateCodeId] = rc.[ID]
  JOIN [dbo].[BurdenElementLU] belu on map.[BurdenElementID] = belu.[ID]
  WHERE rev.[ID] = @RevisionID) as src
  PIVOT (max(src.[RateCode]) for src.[BurdenElement] in ([ESCAL],[OH Dev],[OH Prod],[OH FBM],[OH LVS],
			[OH Hunts],[OH Offsite],[OH Michoud],[OH Mich MH],[OH Prg Uni],[OH Pro DIR],
			[OH Pro MSC],[OH Pro Serv],[OH On Serv],[OH Off Serv],[OH PH4],[OH PH5],
			[Fringe],[Fringe T2],[FRG Serv Corp],[FRG Serv LMOS],[G&A],[G&A T2],
			[FCCOM Dev],[FCCOM Prod],[FCCOM FBM],[FCCOM LVS],[FCCOM Hunt],[FCCOM Ofst],
			[FCCOM Mich],[FCM Prg Un],[FCCOM Proc],[FCCOM ProM],[FCCOM G&A],[FCM T2 G&A],
			[FCCM IWTA],[FCCM PH2],[FCCM PH3],[FCCM PH4],[Fee/Prft])) as piv
ORDER BY BurdenPool;

GO


/*
    File: \Stored Procedures\GetRateCodeList.sql
*/
PRINT '### Starting file: \Stored Procedures\GetRateCodeList.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetRateCodeList]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[GetRateCodeList];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetRateCodeList]
(
	@RevisionID INT,
	@StartYear  varchar(50) = NULL,
	@EndYear    varchar(50) = NULL
)
AS	
/******************************************************************************
**		 
**		Name: [genBOE].[GetRateCodeList]
**		Desc: Get rate codes, rate code details, and rate values by year.
**            If @StartYear and @EndYear parameters are provided, return rates
**            between those years (inclusive).
**            Otherwise, return rates for all years.
**			 
**		
**
**		Auth: brunworg
**		Date: 3/26/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			---------------------------------------
**		06/21/2017	brunworg			Added Government and Commercial 
**                                      Burden Pool columns.
**                                      Added pivot columns thru 2099.
**		07/03/2017	brunworg			Added StartYear and EndYear parameters.
**                                      Return each year/rate as separate rows.
**      07/03/2017	brunworg			Modified where clause to remove duplicate code.
**		08/21/2017	brunworg			Remove AlternateDescription, DataTypeID,
**										and DataFormat from RateCode table.
**		08/22/2017	brunworg			Redesign Section and related tables.
*******************************************************************************/
SET NOCOUNT ON 

SELECT clu.Description, rc.Description, rc.RateCode, ISNULL(s.Title,'') as Section,
	    ISNULL(rc.CobraRateSet,'') as CobraRateSet, rc.CobraCode1ID as CobraCode1, 
	    ISNULL(reslu.Description,'') as ResourceType, 
		ISNULL(bplu1.BurdenPool,'') as GovernmentBurdenPool, ISNULL(bplu2.BurdenPool,'') as CommercialBurdenPool, 
		ISNULL(rtlu.Description,'') as RateType, rcy.year, rcy.rate
FROM dbo.Revision rev
JOIN dbo.RateCode rc on rev.ID = rc.RevisionID
JOIN dbo.CategoryLU clu on rc.CategoryID = clu.ID
LEFT OUTER JOIN dbo.Section s on rc.SectionID = s.ID
LEFT OUTER JOIN dbo.ResourceTypeLU reslu on rc.ResourceTypeID = reslu.ID
LEFT OUTER JOIN dbo.BurdenPoolLU bplu1 on rc.GovernmentBurdenPoolID = bplu1.ID
LEFT OUTER JOIN dbo.BurdenPoolLU bplu2 on rc.CommercialBurdenPoolID = bplu2.ID
LEFT OUTER JOIN dbo.RateTypeLU rtlu on rc.RateTypeID = rtlu.ID
JOIN dbo.RateCodeYear rcy on rc.ID= rcy.RateCodeID
WHERE rev.ID = @RevisionID AND ((@StartYear IS NULL AND @EndYear IS NULL) OR (rcy.Year >= @StartYear AND rcy.Year <= @EndYear))
ORDER BY clu.Description, rc.RateCode, rcy.Year;

GO

/*
    File: \Stored Procedures\publishRevision.sql
*/
PRINT '### Starting file: \Stored Procedures\publishRevision.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[publishRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[publishRevision];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[publishRevision]
(
	 @Id int
	,@UpdateDate datetime2(7)
    ,@PublishedBy varchar(1000)
	,@History nvarchar(max)
	,@ReleaseNotes nvarchar(max)
	,@DatePublished datetime2(7) = NULL
)
AS
	/******************************************************************************
	**		 
	**		Name:	[publishRevision]
	**		Desc:	Publish a PPR&D Revision.  This includes creating the new Work
	**				In Progress (WIP) revision by making a copy of the published 
	**				revision and incrementing the revision number by 1.
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 7/7/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**      07/13/2017	brunworg			Add code to make a copy of the 
	**										previous revision.
	**		08/10/2017	brunworg			Remove transaction handling.
	**		08/24/2017	brunworg			Change return signature to contain
	**										new WIP Revision ID and Revision name.
	**		10/05/2017	ranzalon			Update for History and Release Notes
	**		10/11/2017	ranzalon			Update to take in History and Release
	**										Note inputs, don't copy release notes
	**		05/08/2018	brunworg			Added optional DatePublished parameter.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;
	DECLARE @Revision varchar(50), @NewRevision int;

	SELECT @Revision = Revision FROM [dbo].Revision WHERE ID = @Id;
	IF @@ROWCOUNT = 0	
		BEGIN
			SET @ErrorMessage = 'Publish failed - Revision could not be found.'
			RAISERROR (
					@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN		
		END

	IF (SELECT UpdateDate FROM [dbo].[Revision] WHERE ID = @Id) = @UpdateDate
		BEGIN
			BEGIN TRY
				-- Mark the revision as published
				SET @UpdateDate = GETDATE()
				UPDATE [dbo].Revision
					SET UpdateDate = @UpdateDate
						,DatePublished = ISNULL(@DatePublished, @UpdateDate)
						,PublishedBy = @PublishedBy
						,History = @History
						,ReleaseNotes = @ReleaseNotes
					WHERE 
						ID = @Id;

				-- Create a new Work-In-Progress revision by making a copy of the published revision 
				SET @NewRevision = CONVERT(int, @Revision) + 1;	-- increment the revision number
				EXECUTE dbo.copyRevision @Id=@Id, @NewRevision=@NewRevision, @NewHistory=@History, @NewCreatedBy=@PublishedBy, @NewReleaseNotes=null;
			END TRY
			BEGIN CATCH
				SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(), @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE();

				IF @ErrorMessage IS NULL
					BEGIN
						SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' could not be published.'
					END
								
				RAISERROR (
						@ErrorMessage, -- Message text.
						@ErrorSeverity, -- Severity,
						@ErrorState -- State,
						)
				RETURN
			END CATCH;
		END
	ELSE
		BEGIN
			SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
					11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN
		END

	IF @@ERROR = 0
		SELECT @Id AS Id, @Revision as Revision;
GO

/*
    File: \Stored Procedures\remapSectionReferences.sql
*/
PRINT '### Starting file: \Stored Procedures\remapSectionReferences.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[remapSectionReferences]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[remapSectionReferences];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[remapSectionReferences]
(
	@Id			INT,
	@UpdateDate datetime2,
	@NewId		INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[remapSectionReferences] (originally remapRateCodesForSection)
	**		Desc:	Update all references to @Id (old section Id)
	**				to point to @NewId (new section Id)
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 9/24/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		01/11/2017	brunworg			Renamed and modified to update file 
	**                                      attachment references.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500), @ErrorSeverity INT, @ErrorState INT, @ErrorProcedure VARCHAR(1000), @ErrorLine INT;

	IF (SELECT UpdateDate FROM [dbo].[Section] WHERE ID = @Id ) = @UpdateDate
		BEGIN
			UPDATE [dbo].[RateCode] 
			SET SectionID = @NewId
			WHERE SectionID = @Id;

			UPDATE [dbo].[FileAttachment] 
			SET SectionID = @NewId
			WHERE SectionID = @Id;
		END
	ELSE
		BEGIN
			SET @ErrorMessage =   'The Section with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
			RAISERROR (
					@ErrorMessage, -- Message text.
			        11, -- Severity,/*Severity Changed to 11*/
					1 -- State,
					)
			RETURN

		END
GO


/*
    File: \Stored Procedures\UpdateDbVersion.sql
*/
PRINT '### Starting file: \Stored Procedures\UpdateDbVersion.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateDbVersion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[UpdateDbVersion];

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[UpdateDbVersion](@DbVersion VARCHAR(100), @AppVersion VARCHAR(100)) 
AS
/******************************************************************************
**		 
**		Name: [UpdateDbVersion]
**		Desc: Updates the DB version, if necessary
**
**		Auth: Dusan
**		Date: 11/2016
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**
******************************************************************************/
	IF(NOT EXISTS(SELECT 1 FROM [dbo].[BoeDatabaseVersion] WHERE DBVersion = @DbVersion AND AppVersion = @AppVersion))
	BEGIN
		INSERT INTO [dbo].[BoeDatabaseVersion] (DBVersion, AppVersion, UpdateDate)
		VALUES (@DbVersion, @AppVersion, GETDATE())
	END

GO

/*
    File: \Stored Procedures\updateOfflineApplication.sql
*/
PRINT '### Starting file: \Stored Procedures\updateOfflineApplication.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateOfflineApplication]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateOfflineApplication];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[updateOfflineApplication]
(
	@ApplicationName varchar(10),
	@IsOffline bit,
	@UpdateDate datetime2(7)
)
AS
/******************************************************************************
**          
**          Name: [updateOfflineApplication]
**          Desc: Update Offline Application
**
**          Auth: RJ Anzalone
**          Date: 1/16/2019
*******************************************************************************
**          Change History
*******************************************************************************
**          Date:       Author:                 Description:
**          --------    --------                ---------------------------------------
**
*******************************************************************************/

SET NOCOUNT ON 
DECLARE @ErrorMessage varchar (500)

IF (SELECT UpdateDate FROM [dbo].[OfflineApplication] WHERE [ApplicationName] = @ApplicationName) = @UpdateDate
	BEGIN
		SET @UpdateDate = GETDATE()
		UPDATE [dbo].[OfflineApplication]
			SET UpdateDate = @UpdateDate
				,IsOffline = @IsOffline
			WHERE
				ApplicationName = @ApplicationName
	END
ELSE
	BEGIN
		SET @ErrorMessage = 'The offline status of ' + @ApplicationName + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
		RAISERROR (
				@ErrorMessage, -- Message text.
				11, -- Severity,
				1 -- State,
				)
		RETURN
	END

GO

/*
    File: \Stored Procedures\upsertBanner.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertBanner.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBanner]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBanner];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertBanner]
(
	@Id						INT,
	@UpdateDate				datetime2(7),
	@SelectedApps			varchar(255),
	@StartDate				datetime2(7),
	@HoursToShow			int,
	@BannerText				varchar(500),
	@TurnOffTicker			bit
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertBanner]
	**		Desc:	Insert/Update a Banner
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 3/14/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[Banner]
						([UpdateDate]
						,[SelectedApps]
						,[StartDate]
						,[HoursToShow]
						,[BannerText]
						,[TurnOffTicker]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
					    ,@SelectedApps
						,@StartDate
						,@HoursToShow
						,@BannerText
						,@TurnOffTicker
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[Banner] WHERE [ID] = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].[Banner]
					   SET  UpdateDate = @UpdateDate
							,[SelectedApps] = @SelectedApps
							,[StartDate] = @StartDate
							,[HoursToShow] = @HoursToShow
							,[BannerText] = @BannerText
							,[TurnOffTicker] = @TurnOffTicker
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Banner with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertBurdenPoolLU.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertBurdenPoolLU.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertBurdenPoolLU]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertBurdenPoolLU];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertBurdenPoolLU]
(
	 @Id			int
	,@UpdateDate	datetime2(7)
	,@RevisionID	int
	,@BurdenPool	varchar(50)
    ,@Description	varchar(4000)
    ,@IsGaT2ApplicableForMissionSolutions bit
	,@IncludeGaT2InBurdAndCommBurdTables bit
	,@IsCommercial bit
	,@ExcludeFCCOM bit)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertBurdenPoolLU]
	**		Desc:	Insert/Update Burden Pool values 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		08/03/2017	brunworg			Change stored procedure name and add
	**										RevisionID parameter.
	**		01/04/2018	ranzalon			BOEJ-2698 Burden Pool Categorization - 
	**										Add IsCommercial bit
	**		09/18/2018	twilson3			BOEJ-3805 Add ExcludeFCCOM
	**		08/13/2019	ranzalon			BOEJ-4314 IncludeGaT2InBurdAndCommBurdTables column
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[BurdenPoolLU]
					   ([UpdateDate]
					   ,[RevisionID]
					   ,[BurdenPool]
					   ,[Description]
					   ,[IsGaT2ApplicableForMissionSolutions]
					   ,[IncludeGaT2InBurdAndCommBurdTables]
					   ,[IsCommercial]
					   ,[ExcludeFCCOM])
				 OUTPUT inserted.ID INTO @Inserted
				 VALUES
					   (@UpdateDate
					   ,@RevisionID
					   ,@BurdenPool
					   ,@Description
					   ,@IsGaT2ApplicableForMissionSolutions
					   ,@IncludeGaT2InBurdAndCommBurdTables
					   ,@IsCommercial
					   ,@ExcludeFCCOM)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[BurdenPoolLU] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].BurdenPoolLU
					   SET UpdateDate = @UpdateDate
						  ,RevisionID = @RevisionID
						  ,BurdenPool = @BurdenPool
						  ,Description = @Description
						  ,IsGaT2ApplicableForMissionSolutions = @IsGaT2ApplicableForMissionSolutions
						  ,IncludeGaT2InBurdAndCommBurdTables = @IncludeGaT2InBurdAndCommBurdTables
						  ,IsCommercial = @IsCommercial
						  ,ExcludeFCCOM = @ExcludeFCCOM
						WHERE 
							ID = @Id;
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Burden Pool with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertCobraFiscalYear.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertCobraFiscalYear.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertCobraFiscalYear]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertCobraFiscalYear];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertCobraFiscalYear]
(
	@Id						INT,
	@UpdateDate				datetime2(7),
	@Year					int,
	@FiscalYearStartDate	date
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertCobraFiscalYear]
	**		Desc:	Insert/Update a COBRA Fiscal Year 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before upsert.
	**		7/11/2017	brunworg			Changed @Year from varchar(50) to int.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].CobraFiscalYearLU
						([UpdateDate]
						,[Year]
						,[FiscalYearStartDate]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
					    ,@Year
						,@FiscalYearStartDate
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[CobraFiscalYearLU] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].CobraFiscalYearLU
					   SET  UpdateDate = @UpdateDate
							,Year = @Year
							,FiscalYearStartDate = @FiscalYearStartDate
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The COBRA Fiscal Year with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertFileAttachment.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertFileAttachment.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertFileAttachment]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertFileAttachment];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertFileAttachment]
(
	@Id						INT,
	@UpdateDate				datetime2(7),
	@Name					varchar(100),
	@Link					varchar(255),
	@SectionId				int = NULL,
	@RevisionId				int
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertFileAttachment]
	**		Desc:	Insert/Update a File Attachment
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 1/2/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[FileAttachment]
						([UpdateDate]
						,[Name]
						,[Link]
						,[SectionId]
						,[RevisionId]
						)
				OUTPUT inserted.FileAttachmentId INTO @Inserted
				VALUES
						(@UpdateDate
					    ,@Name
						,@Link
						,@SectionId
						,@RevisionId
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[FileAttachment] WHERE [FileAttachmentId] = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].[FileAttachment]
					   SET  UpdateDate = @UpdateDate
							,[Name] = @Name
							,[Link] = @Link
							,[SectionId] = @SectionId
						WHERE 
							FileAttachmentId = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The File Attachment with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertProPricerBurdenRateMap.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertProPricerBurdenRateMap.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProPricerBurdenRateMap]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProPricerBurdenRateMap];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProPricerBurdenRateMap]
(
	@Id					INT,
	@UpdateDate			datetime2(7),
	@BurdenPoolID		INT,
	@BurdenElementID	INT,
	@RateCodeID			INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertProPricerBurdenRateMap]
	**		Desc:	Insert/Update a ProPricer Burden Rate Mapping 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before upsert.
	**		7/18/2017	tglick				Removed RevisionID, now part of BurdenPoolLU
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].ProPricerBurdenRateMap
						([UpdateDate]
						,[BurdenPoolID]
						,[BurdenElementID]
						,[RateCodeID]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
						,@BurdenPoolID
						,@BurdenElementID
						,@RateCodeID
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[ProPricerBurdenRateMap] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].ProPricerBurdenRateMap
					   SET  UpdateDate = @UpdateDate
						   ,BurdenPoolID = @BurdenPoolID
						   ,BurdenElementID = @BurdenElementID
						   ,RateCodeID = @RateCodeID
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The ProPricer Burden Rate Mapping with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertProPricerRateCodeXref.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertProPricerRateCodeXref.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProPricerRateCodeXref]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProPricerRateCodeXref];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertProPricerRateCodeXref]
(
	@Id					INT,
	@UpdateDate			datetime2(7),
	@RateCodeID			INT,
	@Description		varchar(255),
	@RateCodeExtensionID	INT,
	@ResourceClassID	INT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertProPricerRateCodeXref]
	**		Desc:	Insert/Update a ProPricer Rate Code cross reference
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		7/3/2017	brunworg			Check UpdateDate before upsert.
	**		8/22/2017	brunworg			Redesign Section and related tables.
	**		12/22/2017	brunworg			Add ResourceClassID field.
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].ProPricerRateCodeXref
						([UpdateDate]
						,[RateCodeID]
						,[Description]
						,[RateCodeExtensionID]
						,[ResourceClassID]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
						,@RateCodeID
						,@Description
						,@RateCodeExtensionID
						,@ResourceClassID
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[ProPricerRateCodeXref] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].ProPricerRateCodeXref
					   SET  UpdateDate = @UpdateDate
						   ,RateCodeID = @RateCodeID
						   ,Description = @Description
						   ,RateCodeExtensionID = @RateCodeExtensionID
						   ,ResourceClassID = @ResourceClassID
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The ProPricer Rate Code Cross Reference with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertRDSBDocumentInformation.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertRDSBDocumentInformation.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRDSBDocumentInformation]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertRDSBDocumentInformation];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertRDSBDocumentInformation]
(
	 @Id INT
	,@LastUpdateDT DATETIME2(7)
	,@PTMProposalID INT
	,@CreatedBy VARCHAR(1000)
	,@RDMRevisionID INT
	,@StartYear INT
	,@EndYear INT
	,@ParentSection VARCHAR(10) = NULL
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertRDSBDocumentInformation]
	**		Desc:	Insert/Update RDSBDocumentInformation
	**			
	**		
	**
	**		Auth: ranzalon
	**		Date: 1/25/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		02/01/2018	brunworg			Added StartYear and EndYear fields.
	**		4/27/18		twilson3			BOEJ-3384 Added Parent Section
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF EXISTS (SELECT 1 FROM [dbo].[Revision] WHERE ID = @RDMRevisionID)
		BEGIN
			IF @Id  < 0 
				/* Insert */
				BEGIN
					DECLARE @Inserted AS Table (Id int)
					SET @LastUpdateDT = GETDATE()

					INSERT INTO [dbo].[RDSBDocumentInformation]
							   (PTMProposalID
							   ,CreatedBy
							   ,CreatedDate
							   ,LastUpdateDT
							   ,RDMRevisionID
							   ,StartYear
							   ,EndYear
							   ,ParentSection)
						 OUTPUT inserted.ID INTO @Inserted
						 VALUES
							   (@PTMProposalID
							   ,@CreatedBy
							   ,@LastUpdateDT
							   ,@LastUpdateDT
							   ,@RDMRevisionID
							   ,@StartYear
							   ,@EndYear
							   ,@ParentSection)

					SELECT @Id = Id FROM @Inserted
				END
			ELSE
				/* Update */
				BEGIN
					IF (SELECT LastUpdateDT FROM [dbo].[RDSBDocumentInformation] WHERE ID = @Id) = @LastUpdateDT
						BEGIN
							SET @LastUpdateDT = GETDATE()
							UPDATE [dbo].[RDSBDocumentInformation]
							   SET LastUpdateDT = @LastUpdateDT
								  ,RDMRevisionID = @RDMRevisionID
								  ,StartYear = @StartYear
								  ,EndYear = @EndYear
								  ,ParentSection = @ParentSection
								WHERE 
									ID = @Id
						END
					ELSE
						BEGIN
							SET @ErrorMessage =   'The RDSB Document Information with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
							RAISERROR (
									@ErrorMessage, -- Message text.
									11, -- Severity,/*Severity Changed to 11*/
									1 -- State,
									)
							RETURN
						END
				END
			END
		ELSE
			BEGIN
				SET @ErrorMessage =   'A Revision with ID ' + CAST(@RDMRevisionID  AS varchar(10)) + ' does not exist. Please select a valid Revision.'
							RAISERROR (
									@ErrorMessage, -- Message text.
									11, -- Severity,/*Severity Changed to 11*/
									1 -- State,
									)
							RETURN
			END

	IF @@ERROR = 0
		SELECT @Id AS NewId
GO

/*
    File: \Stored Procedures\upsertReplication.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertReplication.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertReplication]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertReplication];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertReplication]
(
	@Id						INT,
	@UpdateDate				datetime2(7),
	@From			varchar(50),
	@To				varchar(50)
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertReplication]
	**		Desc:	Insert/Update a Rate Code Replication
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 5/29/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[RateCodeReplication]
						([UpdateDate]
						,[From]
						,[To]
						)
				OUTPUT inserted.ID INTO @Inserted
				VALUES
						(@UpdateDate
					    ,@From
						,@To
						)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[RateCodeReplication] WHERE [ID] = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].[RateCodeReplication]
					   SET  UpdateDate = @UpdateDate
							,[From] = @From
							,[To] = @To
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Rate Code Replication with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId

GO

/*
    File: \Stored Procedures\upsertRevision.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertRevision.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertRevision];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertRevision]
(
	 @Id int
	,@UpdateDate datetime2(7)
	,@Revision varchar(50)
    ,@History nvarchar(max)
    ,@CreatedBy varchar(1000) = NULL		-- only used on insert
	,@StartYear int
	,@EndYear int
	,@ReleaseNotes nvarchar(max)
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertRevision]
	**		Desc:	Insert/Update a PPR&D Revision 
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 6/23/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**      07/07/2017	brunworg			Adjusted parameters as follows:
	**										Removed DateCreated parameter (only set
	**										CreatedBy on insert).
	**										Removed DatePublished and PublishedBy
	**										parameters (created separate 
	**										publishRevision procedure).
	**										Removed InUse and Editing parameters
	**										(created separate lock/unlockRevision
	**										procedures).
	**		08/01/2017	brunworg			Added StartYear and EndYear columns.
	**		10/04/2017	ranzalon			Updating for History and Release Notes
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[Revision]
					   ([UpdateDate]
					   ,[Revision]
					   ,[History]
					   ,[DateCreated]
					   ,[CreatedBy]
					   ,[StartYear]
					   ,[EndYear]
					   ,[ReleaseNotes])
				 OUTPUT inserted.ID INTO @Inserted
				 VALUES
					   (@UpdateDate
					   ,@Revision
					   ,@History
					   ,GETDATE()		-- set DateCreated to current date
					   ,@CreatedBy
					   ,@StartYear
					   ,@EndYear
					   ,@ReleaseNotes)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[Revision] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].Revision
					   SET UpdateDate = @UpdateDate
						  ,Revision = @Revision
						  ,History = @History
						  ,StartYear = @StartYear
						  ,EndYear = @EndYear
						  ,ReleaseNotes = @ReleaseNotes
						WHERE 
							ID = @Id;
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Revision with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId
GO

/*
    File: \Stored Procedures\upsertSection.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertSection.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertSection]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertSection];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertSection]
(
	@Id						INT,
	@UpdateDate				datetime2(7),
	@RevisionID				INT,
	@ParentID				INT,
	@DisplayOrder			INT,
	@Title					varchar(4000),
	@TextContent			nvarchar(max),
	@SectionContentTypeID	INT,
	@IsInternalSection		BIT = 0,
	@DisplayRateCode		BIT = 0,
	@RevisionUniqueSectionId INT,
	@IsRdsbRequired			BIT = 0,
	@SectionContainsCasbDisclosure BIT,
	@SectionContainsNonCompliance BIT
)
AS
	/******************************************************************************
	**		 
	**		Name:	[upsertSection]
	**		Desc:	Insert/Update a Section
	**			
	**		
	**
	**		Auth: brunworg
	**		Date: 7/6/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	**		08/21/2017	brunworg			Remove PPRD, Document, DocumentTypeLU,
	**										CostVolume, and CostVolumeRateCode tables.
	**		08/21/2017	brunworg			Add "IsInternalSection" field to Section table.
	**		08/22/2017	brunworg			Remove "IsDeleted" field from all tables.
	**		08/22/2017	brunworg			Redesign Section and related tables.
	**		10/10/2017	Dusan				Added RevisionUniqueSectionId
	**		05/15/2018	ranzalon			Added IsRdsbRequired
	**		07/07/2022	Dusan				Added SectionContainsCasbDisclosure and SectionContainsNonCompliance
	*******************************************************************************/
	SET NOCOUNT ON 
	DECLARE @ErrorMessage varchar (500)

	-- creating a new section, need to generate new RevisionUniqueSectionId
	IF @RevisionUniqueSectionId < 0
	BEGIN
		SELECT @RevisionUniqueSectionId = MAX(RevisionUniqueSectionId) FROM [dbo].[Section]
		SET @RevisionUniqueSectionId = @RevisionUniqueSectionId + 1
	END

	IF @Id  < 0 
		/* Insert */
		BEGIN
			DECLARE @Inserted AS Table (Id int)
			SET @UpdateDate = GETDATE()

			INSERT INTO [dbo].[Section]
					   ([UpdateDate]
					   ,[RevisionID]
					   ,[ParentID]
					   ,[DisplayOrder]
					   ,[Title]
					   ,[TextContent]
					   ,[SectionContentTypeID]
					   ,[IsInternalSection]
					   ,[DisplayRateCode]
					   ,[RevisionUniqueSectionId]
					   ,[IsRdsbRequired]
					   ,[SectionContainsCasbDisclosure]
					   ,[SectionContainsNonCompliance])
				 OUTPUT inserted.ID INTO @Inserted
				 VALUES
					   (@UpdateDate
					   ,@RevisionID
					   ,@ParentID
					   ,@DisplayOrder
					   ,@Title
					   ,@TextContent
					   ,@SectionContentTypeID
					   ,@IsInternalSection
					   ,@DisplayRateCode
					   ,@RevisionUniqueSectionId
					   ,@IsRdsbRequired
					   ,@SectionContainsCasbDisclosure
					   ,@SectionContainsNonCompliance)

			SELECT @Id = Id FROM @Inserted
		END
	ELSE
		/* Update */
		BEGIN
			IF (SELECT UpdateDate FROM [dbo].[Section] WHERE ID = @Id) = @UpdateDate
				BEGIN
					SET @UpdateDate = GETDATE()
					UPDATE [dbo].Section
					   SET  UpdateDate = @UpdateDate
						   ,RevisionID = @RevisionID
					       ,ParentID = @ParentID
					       ,DisplayOrder = @DisplayOrder
					       ,Title = @Title
					       ,TextContent = @TextContent
					       ,SectionContentTypeID = @SectionContentTypeID
						   ,IsInternalSection = @IsInternalSection
					       ,DisplayRateCode = @DisplayRateCode
						   ,RevisionUniqueSectionId = @RevisionUniqueSectionId
						   ,IsRdsbRequired = @IsRdsbRequired
						   ,SectionContainsCasbDisclosure = @SectionContainsCasbDisclosure
						   ,SectionContainsNonCompliance = @SectionContainsNonCompliance
						WHERE 
							ID = @Id
				END
			ELSE
				BEGIN
					SET @ErrorMessage =   'The Section with ID ' + CAST(@Id  AS varchar(10)) + ' has been updated and is out of sync with the data in your browser.  Please refresh your data.'
					RAISERROR (
							@ErrorMessage, -- Message text.
						    11, -- Severity,/*Severity Changed to 11*/
							1 -- State,
							)
					RETURN
				END
		END

	IF @@ERROR = 0
		SELECT @Id AS NewId
GO

/*
    File: \Stored Procedures\upsertUserRole.sql
*/
PRINT '### Starting file: \Stored Procedures\upsertUserRole.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertUserLog]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertUserLog];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[upsertUserLog]
(
@NTID [varchar](1000),
@DisplayName [varchar](1000),
@Application [varchar](100)
)
AS 
	/******************************************************************************
	**		 
	**		Name:	[upsertUserLog]
	**		Desc:	Insert/Update User Log Entry
	**			
	**		
	**
	**		Auth: RJ Anzalone
	**		Date: 11/22/2017
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			-------------------------------------------
	**		11/27/17	ranzalon			Remove domain, add application field
	*******************************************************************************/

	SET NOCOUNT ON 
	DECLARE @Today datetime2(7) = GetDate()

	IF EXISTS (SELECT 1 FROM dbo.UserLog WHERE NTID = @NTID AND DisplayName = @DisplayName AND [Application] = @Application)
		UPDATE dbo.UserLog
		SET [LogInUpdateDT] = @Today
		WHERE 
			NTID = @NTID AND
			DisplayName = @DisplayName AND
			[Application] = @Application 		
	ELSE
		INSERT INTO [dbo].[UserLog]
				   ([LogInUpdateDT]
				   ,[NTID]
				   ,[DisplayName]
				   ,[Application])
			 VALUES
				   (@Today, @NTID, @DisplayName, @Application)
GO

/*
    File: \Stored Procedures\verifyRateCodeReplication.sql
*/
PRINT '### Starting file: \Stored Procedures\verifyRateCodeReplication.sql';
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[verifyRateCodeReplication]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[verifyRateCodeReplication];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[verifyRateCodeReplication]
(
	 @RevisionId int
)
AS
	/******************************************************************************
	**		 
	**		Name:	[verifyRateCodeReplication]
	**		Desc:	Verifies the Rate Codes saved in the Replication Table against the Revision Id passed in
	**			
	**		
	**
	**		Auth: twilson3
	**		Date: 5/29/2018
	*******************************************************************************
	**		Change History
	*******************************************************************************
	**		Date:		Author:				Description:
	**		--------	--------			---------------------------------------
	*******************************************************************************/
	SET NOCOUNT ON 
	
	SELECT [FROM] as RateCode, 'From' as ColumnName FROM [dbo].[RateCodeReplication] WHERE [FROM] NOT IN (SELECT RateCode FROM [dbo].[RateCode] WHERE RevisionID = @RevisionId)
	UNION
	SELECT [TO] as RateCode, 'To' as ColumnName FROM [dbo].[RateCodeReplication] WHERE [TO] NOT IN (SELECT RateCode FROM [dbo].[RateCode] WHERE RevisionID = @RevisionId)
	UNION
	SELECT [TO] as RateCode, 'Both To and From' as ColumnName FROM [dbo].[RateCodeReplication] WHERE [TO] IN (SELECT DISTINCT [FROM] FROM [dbo].[RateCodeReplication])
GO

/*
    File: \Table Based Processing\CobraDetailViaTable.sql
*/
PRINT '### Starting file: \Table Based Processing\CobraDetailViaTable.sql';
-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateCobraDetailviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateCobraDetailviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_CobraDetail' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_CobraDetail];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_CobraDetail] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[CobraRateSet] [varchar](50) NULL,
	[CobraCode1ID] [int] NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateCobraDetailviaTableParameter]
(
@CobraDetailParam [dbo].[TT_CobraDetail] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [updateCobraDetailviaTableParameter]
**		Desc: Insert/Update COBRA data into RateCode Table
**
**		Auth: G. Brunworth
**		Date: 2/2018
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		2/27/2018	brunworg			Created.
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDate datetime2 = GETDATE()

SET @UpdateDate = GetDate()
			
			 
UPDATE [dbo].[RateCode]
	SET 
		[UpdateDate] = TT.[UpdateDate],
		[CobraRateSet] = TT.[CobraRateSet],
		[CobraCode1ID] = TT.[CobraCode1ID]
FROM [dbo].[RateCode] RC
	INNER JOIN @CobraDetailParam TT ON 
		RC.ID = TT.ID 
				
IF @@ERROR = 0
	SELECT 
		RC.ID AS ID, 
		RC.UpdateDate AS UpdateDate 
	FROM @CobraDetailParam TT
		INNER JOIN dbo.RateCode RC ON TT.ID = RC.ID
	ORDER BY OrderID
GO


/*
    File: \Table Based Processing\ProPricerRateCodeXrefViaTable.sql
*/
PRINT '### Starting file: \Table Based Processing\ProPricerRateCodeXrefViaTable.sql';
-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProPricerRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProPricerRateCodeXrefviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProPricerRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProPricerRateCodeXrefviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertProPricerRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertProPricerRateCodeXrefviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_ProPricerRateCodeXref' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_ProPricerRateCodeXref];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_ProPricerRateCodeXref] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Description] [varchar](255) NOT NULL,
	[RateCodeExtensionID] [int] NULL,
	[ResourceClassID] [int] NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateProPricerRateCodeXrefviaTableParameter]
(
@ProPricerRateCodeXref [dbo].[TT_ProPricerRateCodeXref] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [updateProPricerRateCodeXrefviaTableParameter]
**		Desc: Update data in ProPricerRateCodeXref Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		8/23/2017	Dusan				Removed RevisionId
**		12/22/2017	brunworg			Added ResourceClass column.
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDate datetime2(7) = GetDate()
			
UPDATE [dbo].[ProPricerRateCodeXref]
SET 
	[UpdateDate] = @UpdateDate,
	[RateCodeID] = TT.RateCodeID,
	[Description] = TT.Description,
	[RateCodeExtensionID] = TT.RateCodeExtensionID,
	[ResourceClassID] = TT.ResourceClassID
FROM [dbo].[ProPricerRateCodeXref] PPX
	INNER JOIN @ProPricerRateCodeXref TT ON 
		PPX.[ID] = TT.[ID]
				
IF @@ERROR = 0
SELECT	PPX.ID AS ID,
		PPX.[UpdateDate]
FROM [dbo].[ProPricerRateCodeXref] PPX
	INNER JOIN @ProPricerRateCodeXref TT ON 
		PPX.[ID] = TT.[ID] 
ORDER BY TT.OrderID
GO

CREATE PROCEDURE [dbo].[insertProPricerRateCodeXrefviaTableParameter]
(
@ProPricerRateCodeXref [dbo].[TT_ProPricerRateCodeXref] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertProPricerRateCodeXrefviaTableParameter]
**		Desc: Insert data in ProPricerRateCodeXref Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		8/23/2017	Dusan				Removed RevisionId
**		12/22/2017	brunworg			Added ResourceClass column.
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDate datetime2 = GETDATE()

/* Declare a @TT_ProPricerXref table to store the incoming table with an additional OrderID for inserting children. */

DECLARE @TT_ProPricerXref TABLE
(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Description] [varchar] (255) NOT NULL,
	[RateCodeExtensionID] [int] NULL,
	[ResourceClassID] [int] NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
)

INSERT INTO @TT_ProPricerXref
SELECT * FROM @ProPricerRateCodeXref

DECLARE @ID [int],
		@RateCodeID [int],
		@Description [varchar](255),
		@RateCodeExtensionID [int],
		@ResourceClassID [int],
		@OrderID [int]

DECLARE @InsertedProPricerXref AS Table (ID int)

WHILE EXISTS (SELECT * FROM @TT_ProPricerXref WHERE ID < 0)
BEGIN
	SELECT TOP 1 
     	@ID = ID,
		@RateCodeID = RateCodeID,
		@Description = Description,
		@RateCodeExtensionID = RateCodeExtensionID,
		@ResourceClassID = ResourceClassID,
		@OrderID = OrderID
	FROM @TT_ProPricerXref
	WHERE ID < 0

	INSERT INTO [dbo].[ProPricerRateCodeXref]
           ([UpdateDate]
		   ,[RateCodeID]
		   ,[Description]
		   ,[RateCodeExtensionID]
		   ,[ResourceClassID]
		   )
     OUTPUT inserted.ID INTO @InsertedProPricerXref
     VALUES
           (@UpdateDate
		   ,@RateCodeID
           ,@Description
           ,@RateCodeExtensionID
 		   ,@ResourceClassID
           ) 
            
	SELECT @ID = ID FROM @InsertedProPricerXref
	
	UPDATE @TT_ProPricerXref
		SET ID = @ID
	WHERE 
		@OrderID = OrderID AND
		ID < 0

END

IF @@ERROR = 0
	SELECT 
		TT.ID AS ID, 
		@UpdateDate AS UpdateDate
	FROM @TT_ProPricerXref TT
		ORDER BY OrderID
GO

CREATE PROCEDURE [dbo].[deleteProPricerRateCodeXrefviaTableParameter]
(
@ProPricerRateCodeXref [dbo].[TT_ProPricerRateCodeXref] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [deleteProPricerRateCodeXrefviaTableParameter]
**		Desc: Delete data in ProPricerRateCodeXref Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
*******************************************************************************/
SET NOCOUNT ON 

DELETE FROM [dbo].[ProPricerRateCodeXref]
WHERE ID IN (SELECT ID FROM @ProPricerRateCodeXref)

GO


/*
    File: \Table Based Processing\RateCodeViaTable.sql
*/
PRINT '### Starting file: \Table Based Processing\RateCodeViaTable.sql';
-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateRateCodeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].updateRateCodeviaTableParameter;
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRateCodeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].deleteRateCodeviaTableParameter;
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertRateCodeviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertRateCodeviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_RateCode' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_RateCode];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_RateCode] AS TABLE(
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
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
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
		[RateTypeID] = TT.[RateTypeID]
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
CREATE PROCEDURE [dbo].[insertRateCodeviaTableParameter]
(
@RateCodeParam [dbo].[TT_RateCode] READONLY
)
AS
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

/*
    File: \Table Based Processing\RateCodeYearViaTable.sql
*/
PRINT '### Starting file: \Table Based Processing\RateCodeYearViaTable.sql';
-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRateCodeYearviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRateCodeYearviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateRateCodeYearviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateRateCodeYearviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertRateCodeYearviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertRateCodeYearviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_RateCodeYear' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_RateCodeYear];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_RateCodeYear] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[Rate] [decimal](18, 6),
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[updateRateCodeYearviaTableParameter]
(
@RateCodeYear [dbo].[TT_RateCodeYear] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [updateRateCodeYearviaTableParameter]
**		Desc: Update data in RateCodeYear Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		8/23/2017	Dusan				Removed RevisionId
**		12/13/2017	brunworg			Increased Rate scale to decimal(18,6).
*******************************************************************************/
SET NOCOUNT ON 

DECLARE @UpdateDate datetime2(7) = GetDate()
			
UPDATE [dbo].[RateCodeYear]
SET 
	[UpdateDate] = @UpdateDate,
	[RateCodeID] = TT.RateCodeID,
	[Year] = TT.Year,
	[Rate] = TT.Rate
FROM [dbo].[RateCodeYear] RCY
	INNER JOIN @RateCodeYear TT ON 
		RCY.[ID] = TT.[ID] 
				
IF @@ERROR = 0
SELECT	RCY.ID AS ID,
		RCY.[UpdateDate]
FROM [dbo].[RateCodeYear] RCY
	INNER JOIN @RateCodeYear TT ON 
		RCY.[RateCodeID] = TT.[RateCodeID] 
ORDER BY TT.OrderID
GO

CREATE PROCEDURE [dbo].[insertRateCodeYearviaTableParameter]
(
@RateCodeYear [dbo].[TT_RateCodeYear] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertRateCodeYearviaTableParameter]
**		Desc: Insert data in RateCodeYear Table.
**
**		Auth: Debra Ray (Originally - Don Canuso)
**		Date: 8/2017
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		8/10/2017	dray				Created.
**		8/23/2017	Dusan				Removed RevisionId
**		10/19/2017	Dusan				Fixed rate being nullable
**		12/13/2017	brunworg			Increased Rate scale to decimal(18,6).
**		7/25/2019	twilson3			BOEJ-4269 Truncate the temp table in between while loop runs
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDate datetime2 = GETDATE()

/* Declare a @TT_RateCodeYear table to store the incoming table with an additional OrderID for inserting children. */

DECLARE @TT_RateCodeYear TABLE
(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[Rate] [decimal](18,6),
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
)

INSERT INTO @TT_RateCodeYear
SELECT * FROM @RateCodeYear

DECLARE @ID [int],
		@RateCodeID [int],
		@Year [int],
		@Rate [decimal](18,6),
		@OrderID [int]

DECLARE @InsertedRateCodeYear AS Table (ID int)

WHILE EXISTS (SELECT * FROM @TT_RateCodeYear WHERE ID < 0)
BEGIN
	SELECT TOP 1 
     	@ID = ID,
		@RateCodeID = RateCodeID,
		@Year = Year,
		@Rate = Rate,
		@OrderID =  OrderID
	FROM @TT_RateCodeYear
	WHERE ID < 0


	INSERT INTO [dbo].[RateCodeYear]
           ([UpdateDate]
		   ,[RateCodeID]
           ,[Year]
           ,[Rate]
		   )
     OUTPUT inserted.ID INTO @InsertedRateCodeYear
     VALUES
           (@UpdateDate
		   ,@RateCodeID
           ,@Year
           ,@Rate
            ) 
            
	SELECT @ID = ID FROM @InsertedRateCodeYear
	
	UPDATE @TT_RateCodeYear
		SET ID = @ID
	WHERE 
		@OrderID = OrderID AND
		ID < 0

	DELETE FROM @InsertedRateCodeYear
END

IF @@ERROR = 0
	SELECT 
		TT.ID AS ID, 
		@UpdateDate AS UpdateDate
	FROM @TT_RateCodeYear TT
		ORDER BY OrderID
GO

/*
    File: \Table Based Processing\RDSBRateCodeXrefViaTable.sql
*/
PRINT '### Starting file: \Table Based Processing\RDSBRateCodeXrefViaTable.sql';
-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRDSBRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRDSBRateCodeXrefviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertRDSBRateCodeXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertRDSBRateCodeXrefviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_RDSBRateCodeXref' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_RDSBRateCodeXref];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_RDSBRateCodeXref] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RDSBDocumentInformationID] [int] NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertRDSBRateCodeXrefviaTableParameter]
(
@RDSBRateCodeXref [dbo].[TT_RDSBRateCodeXref] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertRDSBRateCodeXrefviaTableParameter]
**		Desc: Insert data in RDSBRateCodeXref Table.
**
**		Auth: G. Brunworth
**		Date: 2/2018
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		2/01/2018	brunworg			Created.
**		2/15/2018	brunworg			Added RDSBDocumentInformationID column.
**		2/29/2018	ranzalon			Added removal of previous xrefs
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDate datetime2 = GETDATE()

-- Remove all previous xrefs for the RDSBDocumentInformationID
DELETE FROM [dbo].[RDSBRateCodeXref]
WHERE RDSBDocumentInformationID IN (SELECT DISTINCT RDSBDocumentInformationID FROM @RDSBRateCodeXref)

/* Declare a @TT_RDSBXref table to store the incoming table. */

DECLARE @TT_RDSBXref TABLE
(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RDSBDocumentInformationID] [int] NOT NULL,
	[RateCodeID] [int] NOT NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
)

INSERT INTO @TT_RDSBXref
SELECT * FROM @RDSBRateCodeXref

DECLARE @ID [int],
		@RDSBDocumentInformationID [int],
		@RateCodeID [int],
		@OrderID [int]

DECLARE @InsertedRDSBXref AS Table (ID int)

WHILE EXISTS (SELECT * FROM @TT_RDSBXref WHERE ID < 0)
BEGIN
	SELECT TOP 1 
     	@ID = ID,
		@RDSBDocumentInformationID = RDSBDocumentInformationID,
		@RateCodeID = RateCodeID,		
		@OrderID = OrderID
	FROM @TT_RDSBXref
	WHERE ID < 0

	INSERT INTO [dbo].[RDSBRateCodeXref]
           ([UpdateDate]
		   ,[RDSBDocumentInformationID]
		   ,[RateCodeID]
		   )
     OUTPUT inserted.ID INTO @InsertedRDSBXref
     VALUES
           (@UpdateDate
		   ,@RDSBDocumentInformationID
		   ,@RateCodeID
           ) 
            
	SELECT @ID = ID FROM @InsertedRDSBXref
	
	UPDATE @TT_RDSBXref
		SET ID = @ID
	WHERE 
		@OrderID = OrderID AND
		ID < 0

END

IF @@ERROR = 0
	SELECT 
		TT.ID AS ID, 
		@UpdateDate AS UpdateDate
	FROM @TT_RDSBXref TT
		ORDER BY OrderID
GO


/*
    File: \Table Based Processing\RDSBSectionXrefViaTable.sql
*/
PRINT '### Starting file: \Table Based Processing\RDSBSectionXrefViaTable.sql';
-- Drop SPs 1st
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRDSBSectionXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteRDSBSectionXrefviaTableParameter];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertRDSBSectionXrefviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertRDSBSectionXrefviaTableParameter];
GO

-- Drop types 2nd
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TT_RDSBSectionXref' AND ss.name = N'dbo')
	DROP TYPE [dbo].[TT_RDSBSectionXref];
GO

-- Recreate types 3rd
CREATE TYPE [dbo].[TT_RDSBSectionXref] AS TABLE(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RDSBDocumentInformationID] [int] NOT NULL,
	[SectionID] [int] NOT NULL,
	[OrderID] [int] NOT NULL
);
GO

-- Recreate SPs last
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[insertRDSBSectionXrefviaTableParameter]
(
@RDSBSectionXref [dbo].[TT_RDSBSectionXref] READONLY
)
AS
/******************************************************************************
**		 
**		Name: [insertRDSBSectionXrefviaTableParameter]
**		Desc: Insert data in RDSBSectionXref Table.
**
**		Auth: G. Brunworth
**		Date: 2/2018
*******************************************************************************
**		Change History
*******************************************************************************
**		Date:		Author:				Description:
**		--------	--------			-------------------------------------------
**		2/01/2018	brunworg			Created.
**		2/15/2018	brunworg			Added RDSBDocumentInformationID column.
**		2/29/2018	ranzalon			Added removal of previous xrefs
*******************************************************************************/
SET NOCOUNT ON 
DECLARE @UpdateDate datetime2 = GETDATE()

-- Remove all previous xrefs for the RDSBDocumentInformationID
DELETE FROM [dbo].[RDSBSectionXref]
WHERE RDSBDocumentInformationID IN (SELECT DISTINCT RDSBDocumentInformationID FROM @RDSBSectionXref)

/* Declare a @TT_RDSBXref table to store the incoming table. */

DECLARE @TT_RDSBXref TABLE
(
	[ID] [int] NOT NULL PRIMARY KEY CLUSTERED,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RDSBDocumentInformationID] [int] NOT NULL,
	[SectionID] [int] NOT NULL,
	/* OrderID is automatically added in the code, so it HAS to be last */
	[OrderID] [int] NOT NULL
)

INSERT INTO @TT_RDSBXref
SELECT * FROM @RDSBSectionXref

DECLARE @ID [int],
		@RDSBDocumentInformationID [int],
		@SectionID [int],
		@OrderID [int]

DECLARE @InsertedRDSBXref AS Table (ID int)

WHILE EXISTS (SELECT * FROM @TT_RDSBXref WHERE ID < 0)
BEGIN
	SELECT TOP 1 
     	@ID = ID,
		@RDSBDocumentInformationID = RDSBDocumentInformationID,
		@SectionID = SectionID,		
		@OrderID = OrderID
	FROM @TT_RDSBXref
	WHERE ID < 0

	INSERT INTO [dbo].[RDSBSectionXref]
           ([UpdateDate]
		   ,[RDSBDocumentInformationID]
		   ,[SectionID]
		   )
     OUTPUT inserted.ID INTO @InsertedRDSBXref
     VALUES
           (@UpdateDate
		   ,@RDSBDocumentInformationID
		   ,@SectionID
           ) 
            
	SELECT @ID = ID FROM @InsertedRDSBXref
	
	UPDATE @TT_RDSBXref
		SET ID = @ID
	WHERE 
		@OrderID = OrderID AND
		ID < 0

END

IF @@ERROR = 0
	SELECT 
		TT.ID AS ID, 
		@UpdateDate AS UpdateDate
	FROM @TT_RDSBXref TT
		ORDER BY OrderID
GO


/*
    File: \X_Data Cleanup\DeleteErrorLogs.sql
*/
PRINT '### Starting file: \X_Data Cleanup\DeleteErrorLogs.sql';
-- Clean up Error Logs older than 30 days
DELETE FROM [ELMAH_Error] WHERE TimeUtc < DATEADD(d, -30, getdate());
GO

PRINT '###### SCRIPT FINISHED ######';