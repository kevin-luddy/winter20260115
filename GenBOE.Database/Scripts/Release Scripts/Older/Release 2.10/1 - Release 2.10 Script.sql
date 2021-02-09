/*
	## START ##

	2/16/2016: Tim's DB changes for Summary Boe
*/

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsSummaryBoe' AND Object_ID = Object_ID('[dbo].[BOE]'))
BEGIN
	ALTER TABLE [dbo].[BOE]
		ADD IsSummaryBoe BIT NOT NULL
		CONSTRAINT DF_BOE_IsSummaryBoe DEFAULT 0;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsSummaryBoe' AND Object_ID = Object_ID('[version].[BOE]'))
BEGIN
	ALTER TABLE [version].[BOE]
		ADD IsSummaryBoe BIT NOT NULL
		CONSTRAINT DF_BOE_IsSummaryBoe DEFAULT 0;
END
GO

IF EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'SummaryBoeSelection' AND Object_ID = Object_ID('[dbo].[BOE]'))
BEGIN
    ALTER TABLE dbo.BOE
        DROP COLUMN SummaryBoeSelection
END
GO

IF EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'SummaryBoeSelection' AND Object_ID = Object_ID('[version].[BOE]'))
BEGIN
    ALTER TABLE [version].BOE
        DROP COLUMN SummaryBoeSelection
END
GO

IF EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'SummaryBoeSelection' AND Object_ID = Object_ID('[dbo].[BOETaskElement]'))
BEGIN
    ALTER TABLE [dbo].BOETaskElement
        DROP COLUMN SummaryBoeSelection
END
GO

IF EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'SummaryBoeSelection' AND Object_ID = Object_ID('[version].[BOETaskElement]'))
BEGIN
    ALTER TABLE [version].BOETaskElement
        DROP COLUMN SummaryBoeSelection
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsSummaryTaskElement' AND Object_ID = Object_ID('[dbo].[BOETaskElement]'))
BEGIN
	ALTER TABLE [dbo].BOETaskElement
		ADD IsSummaryTaskElement BIT NOT NULL
		CONSTRAINT DF_BOETaskElement_IsSummaryBoeTaskElement DEFAULT 0;
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsSummaryTaskElement' AND Object_ID = Object_ID('[version].[BOETaskElement]'))
BEGIN
	ALTER TABLE [version].BOETaskElement
		ADD IsSummaryTaskElement BIT NOT NULL
		CONSTRAINT DF_BOETaskElement_IsSummaryBoeTaskElement DEFAULT 0;
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'SummaryBoeSelections' AND Object_ID = Object_ID('[dbo].[BOETaskElement]'))
BEGIN
	ALTER TABLE [dbo].BOETaskElement
		ADD SummaryBoeSelections VARCHAR(MAX)
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'SummaryBoeSelections' AND Object_ID = Object_ID('[version].[BOETaskElement]'))
BEGIN
	ALTER TABLE [version].BOETaskElement
		ADD SummaryBoeSelections VARCHAR(MAX)
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'SummaryBoeSelection' AND Object_ID = Object_ID('[dbo].[Workspace]'))
BEGIN
	ALTER TABLE [dbo].Workspace
		ADD SummaryBoeSelection int not null default(0)
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'SummaryBoeSelection' AND Object_ID = Object_ID('[version].[Workspace]'))
BEGIN
	ALTER TABLE [version].Workspace
		ADD SummaryBoeSelection int not null default(0)
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'SummaryBoeCustomFieldSelection' AND Object_ID = Object_ID('[dbo].[Workspace]'))
BEGIN
	ALTER TABLE [dbo].Workspace
		ADD SummaryBoeCustomFieldSelection int null default(null)
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'SummaryBoeCustomFieldSelection' AND Object_ID = Object_ID('[version].[Workspace]'))
BEGIN
	ALTER TABLE [version].Workspace
		ADD SummaryBoeCustomFieldSelection int null default(null)
END
GO

/*
	2/16/2016: Tim's DB changes for Summary Boe

	## END ##
*/
/*
## START ##

	2/2/2016 [Matt K]: DB Change for Summary BOE Reports
	
*/

  IF NOT EXISTS (select 1 from [dbo].[ReportLU] where ReportID = 16 and ReportName = 'Summary BOE Discrepancy')
  BEGIN
  insert into [dbo].[ReportLU] (ReportID, ReportName, Description)
  Values (16,'Summary BOE Discrepancy', 'View the Resources which are not included in a Summary BOE')
  END

  /*
	2/2/2016 [Matt K]: DB Change for Summary BOE Reports

	## END ##
*/