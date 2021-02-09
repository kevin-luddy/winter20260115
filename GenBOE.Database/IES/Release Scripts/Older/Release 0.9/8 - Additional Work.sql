/*
	## START ##
	
	08/23/2017 [twilson3] - BOEJ:1601 Adding a table for Elmah
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ELMAH_Error]') AND type in (N'U'))
BEGIN

	CREATE TABLE [dbo].[ELMAH_Error]
	(
		[ErrorId]     UNIQUEIDENTIFIER NOT NULL,
		[Application] NVARCHAR(60)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[Host]        NVARCHAR(50)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[Type]        NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[Source]      NVARCHAR(60)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[Message]     NVARCHAR(500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[User]        NVARCHAR(50)  COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
		[StatusCode]  INT NOT NULL,
		[TimeUtc]     DATETIME NOT NULL,
		[Sequence]    INT IDENTITY (1, 1) NOT NULL,
		[AllXml]      NTEXT COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
	) 
	ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

	ALTER TABLE [dbo].[ELMAH_Error] WITH NOCHECK ADD 
		CONSTRAINT [PK_ELMAH_Error] PRIMARY KEY NONCLUSTERED ([ErrorId]) ON [PRIMARY] 
	
	ALTER TABLE [dbo].[ELMAH_Error] ADD 
		CONSTRAINT [DF_ELMAH_Error_ErrorId] DEFAULT (NEWID()) FOR [ErrorId]
	
	CREATE NONCLUSTERED INDEX [IX_ELMAH_Error_App_Time_Seq] ON [dbo].[ELMAH_Error] 
	(
		[Application]   ASC,
		[TimeUtc]       DESC,
		[Sequence]      DESC
	) 
	ON [PRIMARY]

END

GO
/*
	08/23/2017 [twilson3] - BOEJ:1601 Adding a table for Elmah

	## END ##
*/

/*
	## START ##
	
	10/24/2017 [twilson3] - BOEJ-2749, BOEJ-2747 Missing indexes
*/

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND name = N'idx_Section_RevisionId') 
BEGIN	
	CREATE NONCLUSTERED INDEX [idx_Section_RevisionId]
	ON [dbo].[Section] ([RevisionId])
	INCLUDE ([ID],[UpdateDate],[ParentID],[DisplayOrder],[Title],[TextContent],[SectionContentTypeID],[IsInternalSection],[DisplayRateCode],[RevisionUniqueSectionId])
END
GO 

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[RateCodeYear]') AND name = N'idx_RateCodeYear_Year') 
BEGIN	
	CREATE NONCLUSTERED INDEX [idx_RateCodeYear_Year]
	ON [dbo].[RateCodeYear] ([Year])
	INCLUDE ([ID],[UpdateDate],[RateCodeID],[Rate])
END
GO
/*
	10/24/2017 [twilson3] - BOEJ-2749, BOEJ-2747 Missing indexes

	## END ##
*/