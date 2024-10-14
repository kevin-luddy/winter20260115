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
	**      09/03/2024  e347897             PROPH-2280 Added columns to populate Address tables
	**		10/09/2024	twilson3			PROPH-2456 Added columns for missing Section columns
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
				(UpdateDate, RevisionID, ParentID, DisplayOrder, Title, TextContent, SectionContentTypeID, IsInternalSection, DisplayRateCode, RevisionUniqueSectionId, IsRdsbRequired, SectionContainsCasbDisclosure, SectionContainsNonCompliance,
				Office, Agency, LMBA, Name, Street, CityST, Phone, Email, Other, [IncludeInCoversheet], [IsDisclosureStatementAdequate], [NonComplianceNotification])
				OUTPUT Inserted.ParentID, Inserted.Id INTO @SectionMap
				SELECT GETDATE() AS UpdateDate,
								@RevisionID as RevisionID,
								Id AS ParentID, -- Note that we save the old Id in the new ParentID.
								DisplayOrder, Title, TextContent, SectionContentTypeID, IsInternalSection, DisplayRateCode, RevisionUniqueSectionId, IsRdsbRequired, SectionContainsCasbDisclosure, SectionContainsNonCompliance,
								Office, Agency, LMBA, Name, Street, CityST, Phone, Email, Other, -- Address Table 
								[IncludeInCoversheet], [IsDisclosureStatementAdequate], [NonComplianceNotification]
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