EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2023.x';
GO

/*
		## START ##
		7/18/23		May SLMX_POLM_PROPH-925 Update Database for Address
*/

USE [IES]
GO

/* 1. New row inside SectionContentTypeLU for new content type "Address"*/
IF NOT EXISTS (SELECT [Description] FROM [dbo].[SectionContentTypeLU] WHERE [Description] = N'Address' )
BEGIN
INSERT INTO [dbo].[SectionContentTypeLU]
           ([Description])
     VALUES
           ('Address')
END


/* 2.Adding new properties/columns into Section Table */
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Office' AND Object_ID = Object_ID(N'[dbo].[Section]'))
BEGIN
	ALTER TABLE [dbo].[Section]
	ADD [Office] VARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Agency' AND Object_ID = Object_ID(N'[dbo].[Section]'))
BEGIN
	ALTER TABLE [dbo].[Section]
	ADD [Agency] VARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LMBA' AND Object_ID = Object_ID(N'[dbo].[Section]'))
BEGIN
	ALTER TABLE [dbo].[Section]
	ADD [LMBA] VARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Name' AND Object_ID = Object_ID(N'[dbo].[Section]'))
BEGIN
	ALTER TABLE [dbo].[Section]
	ADD [Name] VARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Street' AND Object_ID = Object_ID(N'[dbo].[Section]'))
BEGIN
	ALTER TABLE [dbo].[Section]
	ADD [Street] VARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CityST' AND Object_ID = Object_ID(N'[dbo].[Section]'))
BEGIN
	ALTER TABLE [dbo].[Section]
	ADD [CityST] VARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Phone' AND Object_ID = Object_ID(N'[dbo].[Section]'))
BEGIN
	ALTER TABLE [dbo].[Section]
	ADD [Phone] VARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Email' AND Object_ID = Object_ID(N'[dbo].[Section]'))
BEGIN
	ALTER TABLE [dbo].[Section]
	ADD [Email] VARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Other' AND Object_ID = Object_ID(N'[dbo].[Section]'))
BEGIN
	ALTER TABLE [dbo].[Section]
	ADD [Other] VARCHAR(100) NULL;
END
GO

/* 3. Update upsertSection stored procedure to take in new properties*/
/****** Object:  StoredProcedure [dbo].[upsertSection]    Script Date: 7/18/2023 2:58:44 PM ******/
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
	@SectionContainsNonCompliance BIT,
	@Office					varchar(100),
	@Agency				    varchar(100),
	@LMBA					varchar(100),
	@Name					varchar(100),
	@Street				    varchar(100),
	@CityST					varchar(100),
	@Phone					varchar(100),
	@Email					varchar(100),
	@Other					varchar(100)
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
					   ,[SectionContainsNonCompliance]
					   ,[Office]
					   ,[Agency]
					   ,[LMBA]
					   ,[Name]
					   ,[Street]
					   ,[CityST]
					   ,[Phone]
					   ,[Email]
					   ,[Other]
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
					   ,@SectionContainsNonCompliance
					   ,@Office
					   ,@Agency
					   ,@LMBA
					   ,@Name
					   ,@Street
					   ,@CityST
					   ,@Phone
					   ,@Email
					   ,@Other)
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
						   ,Office = @Office
					       ,Agency =@Agency
					       ,LMBA =@LMBA
					       ,[Name] = @Name
						   ,Street =@Street
						   ,CityST =@CityST
						   ,Phone = @Phone
						   ,Email = @Email
						   ,Other = @Other
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
		7/18/23		May SLMX_POLM_PROPH-925 Update Database for Address
       ## END ##
*/