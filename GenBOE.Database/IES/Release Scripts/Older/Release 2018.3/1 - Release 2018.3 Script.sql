EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.3';
GO

/*
       ## START ##

       2/22/18       brunworg             BOEJ-3107 – Remove unused SPs
*/

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRateCode]') AND type in (N'P', N'PC'))
       DROP PROCEDURE [dbo].[deleteRateCode];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteRateCodeYear]') AND type in (N'P', N'PC'))
       DROP PROCEDURE [dbo].[deleteRateCodeYear];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRateCode]') AND type in (N'P', N'PC'))
       DROP PROCEDURE [dbo].[upsertRateCode];
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertRateCodeYear]') AND type in (N'P', N'PC'))
       DROP PROCEDURE [dbo].[upsertRateCodeYear];
GO

/*
       2/22/18       brunworg             BOEJ-3107 – Remove unused SPs

       ## END ##
*/
/*
       ## START ##

       3/22/18       brunworg             BOEJ-3163 - Move configuration of rates into DB
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RateConfig]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[RateConfig](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[RateTarget] [varchar](255) NOT NULL,
		[CategoryID] [int] NULL,
		[Prefix] [varchar](50) NULL,
		[Suffix] [varchar](50) NULL,
		[Precision] [int] NOT NULL,
		[Multiplier] [int] NULL,
	 CONSTRAINT [PK_RateConfig] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[RateConfig]  WITH CHECK ADD  CONSTRAINT [FK_RateConfig_CategoryLU] FOREIGN KEY([CategoryID])
	REFERENCES [dbo].[CategoryLU] ([ID])

	ALTER TABLE [dbo].[RateConfig] CHECK CONSTRAINT [FK_RateConfig_CategoryLU]
END
GO

IF NOT EXISTS(SELECT * FROM [dbo].[RateConfig])
BEGIN
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', null, null, null, 6, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Direct Labor'), '$', null, 2, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'FCCOM'), null, '%', 6, 100);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Fringe'), null, '%', 6, 100);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'G&A'), null, '%', 6, 100);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Labor Escalation Factor'), null, null, 4, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Labor Escalation Percentage'), null, '%', 4, 100);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Non-Labor Escalation Factor'), null, null, 4, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Non-Labor Escalation Percentage'), null, '%', 4, 100);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Overhead'), null, '%', 6, 100);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Staff Month Conversion'), null, null, 0, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Service Center'), '$', null, 2, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Travel Fee'), '$', null, 2, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Travel Mlge'), '$', null, 3, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Travel OTC'), '$', null, 2, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('PPRD', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Travel RC'), '$', null, 2, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', null, null, null, 6, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Direct Labor'), '$', null, 2, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'FCCOM'), null, null, 6, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Fringe'), null, null, 6, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'G&A'), null, null, 6, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Labor Escalation Factor'), null, null, 4, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Labor Escalation Percentage'), null, null, 4, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Non-Labor Escalation Factor'), null, null, 4, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Non-Labor Escalation Percentage'), null, null, 4, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Overhead'), null, null, 6, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Staff Month Conversion'), null, null, 0, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Service Center'), '$', null, 2, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Travel Fee'), '$', null, 2, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Travel Mlge'), '$', null, 3, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Travel OTC'), '$', null, 2, null);
	INSERT INTO [dbo].[RateConfig] (RateTarget, CategoryID, Prefix, Suffix, Precision, Multiplier) values ('Rate', (SELECT ID FROM [dbo].[CategoryLU] WHERE Description = 'Travel RC'), '$', null, 2, null);
END

/*
       3/22/18       brunworg             BOEJ-3163 - Move configuration of rates into DB

       ## END ##
*/

/*
       ## START ##

       3/14/18       twilson3			BOEJ-3208 - (Maintenance) Banner DB work
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Banner]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Banner](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[UpdateDate] [datetime2](7) NOT NULL,
		[SelectedApps] [varchar](255) NOT NULL,
		[StartDate] [datetime2](7) NOT NULL,
		[HoursToShow] [int] NOT NULL,
		[BannerText] [varchar](500) NULL,
		[TurnOffTicker] [bit] NULL
	 CONSTRAINT [PK_Banner] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

END
GO
/*
       3/14/18       twilson3			BOEJ-3208 - (Maintenance) Banner DB work

       ## END ##
*/

