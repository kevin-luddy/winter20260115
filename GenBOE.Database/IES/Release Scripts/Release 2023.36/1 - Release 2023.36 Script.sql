EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.36';

GO
/**** ALTER TABLE Section to ADD Column IsDisclosureStatementAdequate ****/
ALTER TABLE dbo.Section
ADD IsDisclosureStatementAdequate bit
DEFAULT (0)

GO

GO
/**** ALTER TABLE Section to ADD Column NonComplianceNotification ****/
ALTER TABLE dbo.Section
ADD NonComplianceNotification bit
DEFAULT (0)

GO


/**** ALTER STORED PROCEDURE: upsertSection to include new Columns [IsDisclosureStatementAdequate] AND [NonComplianceNotification] ****/

USE [IES]
GO

/****** Object:  StoredProcedure [dbo].[upsertSection]    Script Date: 11/6/2023 12:52:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[upsertSection]
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
	@IsDisclosureStatementAdequate BIT,
	@SectionContainsNonCompliance BIT,
	@NonComplianceNotification BIT,
	@Office					varchar(100),
	@Agency				    varchar(100),
	@LMBA					varchar(100),
	@Name					varchar(100),
	@Street				    varchar(100),
	@CityST					varchar(100),
	@Phone					varchar(100),
	@Email					varchar(100),
	@Other					varchar(100),
	@IncludeInCoversheet		BIT = 0
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
	**		07/18/2023	May				    PROPH-925 Added address fields into section table
	**		10/9/2023	May				    PROPH-931 Added IncludeInCoversheet field into section table
	**		11/06/2023  Hazrat				PROPH-56 and 57 Added two Questions IsDisclosureStatementAdequate and NonComplianceNotification
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
					   ,[IsDisclosureStatementAdequate]
					   ,[SectionContainsNonCompliance]
					   ,[NonComplianceNotification]
					   ,[Office]
					   ,[Agency]
					   ,[LMBA]
					   ,[Name]
					   ,[Street]
					   ,[CityST]
					   ,[Phone]
					   ,[Email]
					   ,[Other]
					   ,[IncludeInCoversheet]
					   )
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
					   ,@IsDisclosureStatementAdequate
					   ,@SectionContainsNonCompliance
					   ,@NonComplianceNotification
					   ,@Office
					   ,@Agency
					   ,@LMBA
					   ,@Name
					   ,@Street
					   ,@CityST
					   ,@Phone
					   ,@Email
					   ,@Other
					   ,@IncludeInCoversheet)
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
						   ,IsDisclosureStatementAdequate = @IsDisclosureStatementAdequate
						   ,SectionContainsNonCompliance = @SectionContainsNonCompliance
						   ,NonComplianceNotification = @NonComplianceNotification
						   ,Office = @Office
					       ,Agency =@Agency
					       ,LMBA =@LMBA
					       ,[Name] = @Name
						   ,Street =@Street
						   ,CityST =@CityST
						   ,Phone = @Phone
						   ,Email = @Email
						   ,Other = @Other
						   ,IncludeInCoversheet=@IncludeInCoversheet
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