SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO
-- DROP SOME OUT-DATED STORED PROCEDURES
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteActivityType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteActivityType];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteActivityTypeMap]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteActivityTypeMap];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetActivityTypeMappings]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[GetActivityTypeMappings];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteProPricerActivityTypeXref]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteProPricerActivityTypeXref];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPricingCodeMappings]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[GetPricingCodeMappings];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertActivityType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertActivityType];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertActivityTypeMap]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertActivityTypeMap];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertProPricerActivityTypeXref]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertProPricerActivityTypeXref];
GO

-- DROP TABLES
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProPricerActivityTypeXref]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[ProPricerActivityTypeXref]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProPricerRateCodeXref]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[ProPricerRateCodeXref]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProPricerBurdenRateMap]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[ProPricerBurdenRateMap]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityTypeMap]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[ActivityTypeMap]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityTypeLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[ActivityTypeLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BusinessAreaLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[BusinessAreaLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityTypeDisplayColumnLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[ActivityTypeDisplayColumnLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProPricerActivityTypeDisplayColumnLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[ProPricerActivityTypeDisplayColumnLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CostVolumeRateCode]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[CostVolumeRateCode]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CostVolume]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[CostVolume]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RateCodeYear]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[RateCodeYear]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RateCode]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[RateCode]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CobraCode1LU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[CobraCode1LU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SectionText]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[SectionText]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SectionTable]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[SectionTable]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SectionAttachment]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[SectionAttachment]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SectionContent]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[SectionContent]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Section]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[Section]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SectionContentTypeLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[SectionContentTypeLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UploadedItem]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[UploadedItem]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PPRD]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[PPRD]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Document]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[Document]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocumentTypeLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[DocumentTypeLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BurdenPoolLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[BurdenPoolLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Revision]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[Revision]
END
/* BurdenTypeLU was renamed to  BurdenElementLU.  Keeping Drop statement just in case it needs to be cleaned up */
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BurdenTypeLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[BurdenTypeLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BurdenElementLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[BurdenElementLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CategoryLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[CategoryLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CobraFiscalYearLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[CobraFiscalYearLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataTypeLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[DataTypeLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RateCodeExtensionLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[RateCodeExtensionLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RateTypeLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[RateTypeLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResourceTypeLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[ResourceTypeLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AreaLU]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[AreaLU]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AreaLocking]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[AreaLocking]
END
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RateYearRange]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[RateYearRange]
END
GO

CREATE TABLE [dbo].[ResourceTypeLU](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](4000) NOT NULL,
 CONSTRAINT [PK_ResourceTypeLU] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[RateTypeLU](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](4000) NULL,
 CONSTRAINT [PK_RateTypeLU] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[RateCodeExtensionLU](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RateCodeExtension] [varchar](50) NULL,
	[Description] [varchar](4000) NULL,
 CONSTRAINT [PK_RateCodeExtensionLU] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CategoryLU](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](4000) NULL,
 CONSTRAINT [PK_CategoryLU] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CobraFiscalYearLU](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UpdateDate] [datetime2](7) NOT NULL,
	[Year] [int] NOT NULL,
	[FiscalYearStartDate] [date] NOT NULL,
 CONSTRAINT [PK_CobraFiscalYearLU] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CobraCode1LU](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](4000) NULL,
 CONSTRAINT [PK_CobraCode1LU] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[SectionContentTypeLU](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Description] [varchar](4000) NULL,
 CONSTRAINT [PK_SectionContentTypeLU] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[BurdenElementLU](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BurdenElement] [varchar](50) NOT NULL,
	[Description] [varchar](4000) NULL,
	[DisplayOrder] [int] NOT NULL,
 CONSTRAINT [PK_BurdenElementLU] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Revision](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UpdateDate] [datetime2](7) NOT NULL,
	[Revision] [varchar](50) NOT NULL,		-- AKA Version Number
	[History] [nvarchar](max) NOT NULL,
	[DateCreated] [datetime2](7) NOT NULL,
	[CreatedBy] [varchar](1000) NOT NULL,
	[DatePublished] [datetime2](7) NULL,
	[PublishedBy] [varchar](1000) NULL,
	[StartYear] [int] NOT NULL,
	[EndYear] [int] NOT NULL,
	[ReleaseNotes] [nvarchar](max) NULL
 CONSTRAINT [PK_Revision] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Revision] ADD  CONSTRAINT [CF_Revision] UNIQUE (Revision)
GO

ALTER TABLE [dbo].[Revision] ADD  CONSTRAINT [DF_Revision_DateCreated]  DEFAULT (getdate()) FOR [DateCreated]
GO

CREATE TABLE [dbo].[BurdenPoolLU](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UpdateDate] [datetime2](7) NOT NULL,
	[BurdenPool] [varchar](50) NOT NULL,
	[Description] [varchar](4000) NULL,
	[IsGaT2ApplicableForMissionSolutions] [bit] NOT NULL,     
	[RevisionID] [int] NOT NULL,
CONSTRAINT [PK_BurdenPoolLU] PRIMARY KEY CLUSTERED 
(
     [ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
CONSTRAINT [uq_Person] UNIQUE NONCLUSTERED 
(
     [BurdenPool] ASC,
     [RevisionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BurdenPoolLU]  WITH CHECK ADD  CONSTRAINT [FK_BurdenPoolLU_Revision] FOREIGN KEY([RevisionID])
REFERENCES [dbo].[Revision] ([ID])
GO

ALTER TABLE [dbo].[BurdenPoolLU] CHECK CONSTRAINT [FK_BurdenPoolLU_Revision]
GO

CREATE TABLE [dbo].[Section] (
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RevisionID] [int] NOT NULL,
	[ParentID] [int] NULL,
	[DisplayOrder] [int] NOT NULL,
	[Title] [varchar](4000) NULL,	
	[TextContent] [nvarchar](max) NOT NULL,
	[SectionContentTypeID] [int] NOT NULL,
	[IsInternalSection] [bit] NOT NULL,
	[DisplayRateCode] [bit] NOT NULL,
	RevisionUniqueSectionId [INT] NOT NULL
 CONSTRAINT [PK_Section] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Section]  WITH CHECK ADD  CONSTRAINT [FK_Section_Revision] FOREIGN KEY([RevisionID])
REFERENCES [dbo].[Revision] ([ID])
GO

ALTER TABLE [dbo].[Section] CHECK CONSTRAINT [FK_Section_Revision]
GO

ALTER TABLE [dbo].[Section]  WITH CHECK ADD  CONSTRAINT [FK_Section_Section] FOREIGN KEY([ParentID])
REFERENCES [dbo].[Section] ([ID])
GO

ALTER TABLE [dbo].[Section] CHECK CONSTRAINT [FK_Section_Section]
GO

ALTER TABLE [dbo].[Section]  WITH CHECK ADD  CONSTRAINT [FK_Section_SectionContentTypeLU] FOREIGN KEY([SectionContentTypeID])
REFERENCES [dbo].[SectionContentTypeLU] ([ID])
GO

ALTER TABLE [dbo].[Section] CHECK CONSTRAINT [FK_Section_SectionContentTypeLU]
GO

ALTER TABLE [dbo].[Section] ADD  CONSTRAINT [DF_Section_IsInternalSection]  DEFAULT ((0)) FOR [IsInternalSection]
GO

ALTER TABLE [dbo].[Section] ADD  CONSTRAINT [DF_Section_DisplayRateCode]  DEFAULT ((0)) FOR [DisplayRateCode]
GO

CREATE TABLE [dbo].[RateCode](
	[ID] [int] IDENTITY(1,1) NOT NULL,
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
	[CobraRateSet] [varchar](50) NULL,
	[CobraCode1ID] [int] NULL,
 CONSTRAINT [PK_RateCode] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RateCode]  WITH CHECK ADD  CONSTRAINT [FK_RateCode_CategoryLU] FOREIGN KEY([CategoryID])
REFERENCES [dbo].[CategoryLU] ([ID])
GO

ALTER TABLE [dbo].[RateCode] CHECK CONSTRAINT [FK_RateCode_CategoryLU]
GO

ALTER TABLE [dbo].[RateCode]  WITH CHECK ADD  CONSTRAINT [FK_RateCode_CobraCode1LU] FOREIGN KEY([CobraCode1ID])
REFERENCES [dbo].[CobraCode1LU] ([ID])
GO

ALTER TABLE [dbo].[RateCode] CHECK CONSTRAINT [FK_RateCode_CobraCode1LU]
GO

ALTER TABLE [dbo].[RateCode]  WITH CHECK ADD  CONSTRAINT [FK_RateCode_Section] FOREIGN KEY([SectionID])
REFERENCES [dbo].[Section] ([ID])
GO

ALTER TABLE [dbo].[RateCode] CHECK CONSTRAINT [FK_RateCode_Section]
GO

ALTER TABLE [dbo].[RateCode]  WITH CHECK ADD  CONSTRAINT [FK_RateCode_RateCode] FOREIGN KEY([ID])
REFERENCES [dbo].[RateCode] ([ID])
GO

ALTER TABLE [dbo].[RateCode] CHECK CONSTRAINT [FK_RateCode_RateCode]
GO

ALTER TABLE [dbo].[RateCode]  WITH CHECK ADD  CONSTRAINT [FK_RateCode_Revision] FOREIGN KEY([RevisionID])
REFERENCES [dbo].[Revision] ([ID])
GO

ALTER TABLE [dbo].[RateCode] CHECK CONSTRAINT [FK_RateCode_Revision]
GO

ALTER TABLE [dbo].[RateCode]  WITH CHECK ADD  CONSTRAINT [FK_RateCode_GovtBurdenPoolLU] FOREIGN KEY([GovernmentBurdenPoolID])
REFERENCES [dbo].[BurdenPoolLU] ([ID])
GO

ALTER TABLE [dbo].[RateCode] CHECK CONSTRAINT [FK_RateCode_GovtBurdenPoolLU]
GO

ALTER TABLE [dbo].[RateCode]  WITH CHECK ADD  CONSTRAINT [FK_RateCode_CommBurdenPoolLU] FOREIGN KEY([CommercialBurdenPoolID])
REFERENCES [dbo].[BurdenPoolLU] ([ID])
GO

ALTER TABLE [dbo].[RateCode] CHECK CONSTRAINT [FK_RateCode_CommBurdenPoolLU]
GO

ALTER TABLE [dbo].[RateCode]  WITH CHECK ADD  CONSTRAINT [FK_RateCode_RateTypeLU] FOREIGN KEY([RateTypeID])
REFERENCES [dbo].[RateTypeLU] ([ID])
GO

ALTER TABLE [dbo].[RateCode] CHECK CONSTRAINT [FK_RateCode_RateTypeLU]
GO

ALTER TABLE [dbo].[RateCode]  WITH CHECK ADD  CONSTRAINT [FK_RateCode_ResourceTypeLU] FOREIGN KEY([ResourceTypeID])
REFERENCES [dbo].[ResourceTypeLU] ([ID])
GO

ALTER TABLE [dbo].[RateCode] CHECK CONSTRAINT [FK_RateCode_ResourceTypeLU]
GO

CREATE TABLE [dbo].[RateCodeYear](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[Rate] [decimal](18, 6) NULL,
 CONSTRAINT [PK_RateCodeYear] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
CONSTRAINT [uq_RateCodeYear] UNIQUE NONCLUSTERED 
(
     [RateCodeID] ASC,
     [Year] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RateCodeYear]  WITH CHECK ADD  CONSTRAINT [FK_RateCodeYear_RateCode] FOREIGN KEY([RateCodeID])
REFERENCES [dbo].[RateCode] ([ID])
GO

ALTER TABLE [dbo].[RateCodeYear] CHECK CONSTRAINT [FK_RateCodeYear_RateCode]
GO

CREATE TABLE [dbo].[ProPricerBurdenRateMap](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UpdateDate] [datetime2](7) NOT NULL,
	[BurdenPoolID] [int] NOT NULL,
	[BurdenElementID] [int] NOT NULL,
	[RateCodeID] [int] NOT NULL,
 CONSTRAINT [PK_ProPricerBurdenRateMap] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ProPricerBurdenRateMap]  WITH CHECK ADD  CONSTRAINT [FK_ProPricerBurdenRateMap_BurdenPoolLU] FOREIGN KEY([BurdenPoolID])
REFERENCES [dbo].[BurdenPoolLU] ([ID])
GO

ALTER TABLE [dbo].[ProPricerBurdenRateMap] CHECK CONSTRAINT [FK_ProPricerBurdenRateMap_BurdenPoolLU]
GO

ALTER TABLE [dbo].[ProPricerBurdenRateMap]  WITH CHECK ADD  CONSTRAINT [FK_ProPricerBurdenRateMap_BurdenElementLU] FOREIGN KEY([BurdenElementID])
REFERENCES [dbo].[BurdenElementLU] ([ID])
GO

ALTER TABLE [dbo].[ProPricerBurdenRateMap] CHECK CONSTRAINT [FK_ProPricerBurdenRateMap_BurdenElementLU]
GO

ALTER TABLE [dbo].[ProPricerBurdenRateMap]  WITH CHECK ADD  CONSTRAINT [FK_ProPricerBurdenRateMap_RateCode] FOREIGN KEY([RateCodeID])
REFERENCES [dbo].[RateCode] ([ID])
GO

ALTER TABLE [dbo].[ProPricerBurdenRateMap] CHECK CONSTRAINT [FK_ProPricerBurdenRateMap_RateCode]
GO

CREATE TABLE [dbo].[ProPricerRateCodeXref](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UpdateDate] [datetime2](7) NOT NULL,
	[RateCodeID] [int] NOT NULL,
	[Description] [varchar](255) NOT NULL,
	[RateCodeExtensionID] [int] NULL,
 CONSTRAINT [PK_ProPricerRateCodeXref] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ProPricerRateCodeXref]  WITH CHECK ADD  CONSTRAINT [FK_ProPricerRateCodeXref_RateCode] FOREIGN KEY([RateCodeID])
REFERENCES [dbo].[RateCode] ([ID])
GO

ALTER TABLE [dbo].[ProPricerRateCodeXref] CHECK CONSTRAINT [FK_ProPricerRateCodeXref_RateCode]
GO

ALTER TABLE [dbo].[ProPricerRateCodeXref]  WITH CHECK ADD  CONSTRAINT [FK_ProPricerRateCodeXref_RateCodeExtensionLU] FOREIGN KEY([RateCodeExtensionID])
REFERENCES [dbo].[RateCodeExtensionLU] ([ID])
GO

ALTER TABLE [dbo].[ProPricerRateCodeXref] CHECK CONSTRAINT [FK_ProPricerRateCodeXref_RateCodeExtensionLU]
GO
