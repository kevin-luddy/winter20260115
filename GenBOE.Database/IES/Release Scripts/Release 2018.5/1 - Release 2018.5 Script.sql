EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.5';
GO

/*
       ## START ##

       5/14/18       ranzalon	BOEJ-3381 - Required in RDSB
*/
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsRdsbRequired' AND Object_ID = Object_ID(N'[dbo].[Section]'))
BEGIN
	ALTER TABLE [dbo].[Section]
	ADD [IsRdsbRequired] BIT NOT NULL DEFAULT 0
END

GO
/*
       5/14/18       ranzalon	BOEJ-3381 - Required in RDSB

       ## END ##
*/

/*
       ## START ##

       5/29/2018     twilson3	BOEJ-3348 Rate Code Replication
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RateCodeReplication]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[RateCodeReplication](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[From] [varchar](50) NOT NULL,
		[To] [varchar](50) NOT NULL UNIQUE,
		[UpdateDate] [datetime2](7) NOT NULL
	 CONSTRAINT [PK_RateCodeReplication] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

/*
       5/29/2018     twilson3	BOEJ-3348 Rate Code Replication
       ## END ##
*/

/*
       ## START ##

       6/4/2018 Dusan Removing unused tables
*/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TempPPBurdenRates]') AND type in (N'U'))
	DROP TABLE TempPPBurdenRates;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TempPPDirectRates]') AND type in (N'U'))
	DROP TABLE TempPPDirectRates;

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TempRates]') AND type in (N'U'))
	DROP TABLE TempRates;

GO
/*
        6/4/2018 Dusan Removing unused tables
		
		## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '2', @AppVersion = '2018.5';
GO