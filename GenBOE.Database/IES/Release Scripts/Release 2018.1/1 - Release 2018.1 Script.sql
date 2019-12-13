/*
	## START ##
	1/2/18 twilson3		BOEJ-2704 File Attachments
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FileAttachment]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[FileAttachment](
		[FileAttachmentId] [int] IDENTITY(1,1) NOT NULL,
		[UpdateDate] [datetime2](7) NOT NULL,
		[Name] varchar(100) NOT NULL,
		[Link] varchar(255) NOT NULL,
		[SectionId] int NULL, 
		[RevisionId] int NOT NULL
	 CONSTRAINT [PK_FileAttachment] PRIMARY KEY NONCLUSTERED 
	(
		[FileAttachmentId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	CREATE CLUSTERED INDEX [IX_FileAttachment_RevisionId] ON [dbo].[FileAttachment] 
	(
		[RevisionId]   ASC
	) 
	ON [PRIMARY]

	ALTER TABLE [dbo].[FileAttachment] WITH CHECK ADD  CONSTRAINT [FK_FileAttachment_SectionId] 
	FOREIGN KEY([SectionId]) REFERENCES [dbo].[Section] ([Id]);

END
GO

/*
	1/2/18 twilson3		BOEJ-2704 File Attachments
	## END ##
*/

/*
	## START ##
	1/4/18 ranzalon		BOEJ-2698 Burden Pool Categorization 
*/

IF NOT EXISTS (
	SELECT * FROM sys.all_columns C
		INNER JOIN sys.tables T on C.object_id = T.object_id
		INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
	WHERE
		T.name = 'BurdenPoolLU' AND
		C.name = 'IsCommercial' AND
		S.name = 'dbo'
)
BEGIN
	ALTER TABLE [dbo].[BurdenPoolLU] ADD [IsCommercial] BIT NOT NULL DEFAULT 0;
END
GO

UPDATE [dbo].[BurdenPoolLU] SET [IsCommercial] = 1 WHERE [BurdenPool] like '%-G';
GO

/*
	1/4/18 ranzalon		BOEJ-2698 Burden Pool Categorization 
	## END ##
*/
/*
	## START ##
	1/9/18 brunworg		BOEJ-2901 - Remove ProPricer Pricing Codes table in PPR&D section 3.7
*/

DELETE dbo.Section
WHERE SectionContentTypeID = (SELECT ID from dbo.SectionContentTypeLU WHERE Description = 'ProPricer Pricing Table Content');
GO

DELETE dbo.SectionContentTypeLU WHERE Description = 'ProPricer Pricing Table Content'
GO

/*
	1/9/18 brunworg		BOEJ-2901 - Remove ProPricer Pricing Codes table in PPR&D section 3.7
	## END ##
*/
/*
	## START ##
	1/15/18 brunworg	BOEJ-2965 - System error when attempting to move a PPR&D section that has an associated file attachment (rename stored procedure)
*/
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[remapRateCodesForSection]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[remapRateCodesForSection];
GO
/*
	## END ##
	1/15/18 brunworg	BOEJ-2965 - System error when attempting to move a PPR&D section that has an associated file attachment (rename stored procedure)
*/
