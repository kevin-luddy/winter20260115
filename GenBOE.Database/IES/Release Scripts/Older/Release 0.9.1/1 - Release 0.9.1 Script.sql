/*
	## START ##
	11/27/17 [ranzalon] - BOEJ-2774/2838 RDM User Log
*/

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserLog]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[UserLog](
		[LogInUpdateDT] [datetime2](7) NOT NULL,
		[NTID] [varchar](1000) NOT NULL,
		[DisplayName] [varchar](1000) NOT NULL,
		[Application] [varchar](100) NOT NULL
	) ON [PRIMARY]
END
GO

/*
	11/27/17 [ranzalon] - BOEJ-2774/2838 RDM User Log
	## END ##
*/