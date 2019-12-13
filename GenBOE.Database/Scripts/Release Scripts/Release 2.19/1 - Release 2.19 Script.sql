EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2.19';
GO

/*
	## START ##
	10/19/17 [twilson3] - BOEJ-2569 Custom Field Sorting
*/

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'CustomSorting' AND Object_ID = Object_ID(N'[dbo].[Workspace]'))
BEGIN
	CREATE TABLE [dbo].[CustomSortingLU](
		[ID] [int] NOT NULL,
		[Name] varchar(20) NOT NULL
	 CONSTRAINT [PK_CustomSortingLU] PRIMARY KEY NONCLUSTERED 
	(
		[ID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	INSERT INTO [dbo].[CustomSortingLU] (ID, Name) VALUES (0, 'Description');
	INSERT INTO [dbo].[CustomSortingLU] (ID, Name) VALUES (1, 'ID');

	ALTER TABLE [dbo].[Workspace] ADD [CustomSorting] int NOT NULL default(0);
	ALTER TABLE [version].[Workspace] ADD [CustomSorting] int NOT NULL default(0);
	ALTER TABLE [dbo].[Workspace] ADD [ResourceSorting] int NOT NULL default(0);
	ALTER TABLE [version].[Workspace] ADD [ResourceSorting] int NOT NULL default(0);
	ALTER TABLE [dbo].[Workspace] ADD [PerfOrgSorting] int NOT NULL default(0);
	ALTER TABLE [version].[Workspace] ADD [PerfOrgSorting] int NOT NULL default(0);

	ALTER TABLE [dbo].[Workspace] WITH CHECK ADD CONSTRAINT [FK_CustomSorting] FOREIGN KEY ([CustomSorting]) REFERENCES [dbo].[CustomSortingLU]([ID]);
	ALTER TABLE [dbo].[Workspace] WITH CHECK ADD CONSTRAINT [FK_ResourceSorting] FOREIGN KEY ([ResourceSorting]) REFERENCES [dbo].[CustomSortingLU]([ID]);
	ALTER TABLE [dbo].[Workspace] WITH CHECK ADD CONSTRAINT [FK_PerfOrgSorting] FOREIGN KEY ([PerfOrgSorting]) REFERENCES [dbo].[CustomSortingLU]([ID]);
END
GO
/*
	10/19/17 [twilson3] - BOEJ-2569 Custom Field Sorting
	## END ##
*/
/*
	## START ##
	10/27/17 [twilson3] - BOEJ-2578 Partial Save PBOE/IBOE
*/

ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [Description] [varchar](max) NULL;
ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [BasisAndRationale] [varchar](max) NULL;
ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [ProposalTitle] [varchar](200) NULL; 
ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [ProposalDate] [varchar](10) NULL;
ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [Poc] VARCHAR(65) NULL;
ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [PocPhone] VARCHAR (60) NULL;
ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [Approver] VARCHAR(65) NULL;
ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [ApproverPhone] VARCHAR (60) NULL;
ALTER TABLE [dbo].[BOEFormIBOE] ALTER COLUMN [BusinessArea] [varchar](50) NULL;

ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [Description] [varchar](max) NULL;
ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [BasisAndRationale] [varchar](max) NULL;
ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [ProposalTitle] [varchar](200) NULL; 
ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [ProposalDate] [varchar](10) NULL;
ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [Poc] VARCHAR(65) NULL;
ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [PocPhone] VARCHAR (60) NULL;
ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [Approver] VARCHAR(65) NULL;
ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [ApproverPhone] VARCHAR (60) NULL;
ALTER TABLE [version].[BOEFormIBOE] ALTER COLUMN [BusinessArea] [varchar](50) NULL;

ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [Description] [varchar](max) NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [BasisAndRationale] [varchar](max) NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [ProposalTitle] [varchar](200) NULL; 
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [ProposalDate] [varchar](10) NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [Poc] VARCHAR(65) NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [PocPhone] VARCHAR (60) NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [Approver] VARCHAR(65) NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [ApproverPhone] VARCHAR (60) NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [RFP] [varchar](50) NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [ProposalNumber] [varchar](50) NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [SupplierName] [varchar](50) NULL;
ALTER TABLE [dbo].[BOEFormPBOE] ALTER COLUMN [ValidityDate] [varchar](10) NULL;

ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [Description] [varchar](max) NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [BasisAndRationale] [varchar](max) NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [ProposalTitle] [varchar](200) NULL; 
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [ProposalDate] [varchar](10) NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [Poc] VARCHAR(65) NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [PocPhone] VARCHAR (60) NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [Approver] VARCHAR(65) NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [ApproverPhone] VARCHAR (60) NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [RFP] [varchar](50) NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [ProposalNumber] [varchar](50) NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [SupplierName] [varchar](50) NULL;
ALTER TABLE [version].[BOEFormPBOE] ALTER COLUMN [ValidityDate] [varchar](10) NULL;

/*
	10/27/17 [twilson3] - BOEJ-2578 Partial Save PBOE/IBOE
	## END ##
*/

/*
	## START ##
	11/3/17 [twilson3] - BOEJ-2752 Prod exceptions
*/

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateBOELaborSpreadviaTableParameter]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateBOELaborSpreadviaTableParameter];
GO

/*
	11/3/17 [twilson3] - BOEJ-2752 Prod exceptions
	## END ##
*/

/*
	## START ##
	11/7/17 [twilson3] - BOEJ-2678 New Workbench Offload Report
*/

IF(NOT EXISTS(SELECT 1 FROM [dbo].[ReportLU] WHERE ReportID = 35))
BEGIN
	INSERT INTO [dbo].[ReportLU] (ReportID, ReportName, Description)
	VALUES (35, 'Offload Dollar Workbench Export', 'Comma separated file containing Offload data, for Workbench');
END
GO

/*
	11/7/17 [twilson3] - BOEJ-2678 New Workbench Offload Report
	## END ##
*/

/*
	## START ##
	11/28/17 [pattoncr] - BOEJ-2839 AD utils - NTID uniqueness - Step 2 (Remove Domain)
*/

-- Drop and recreate index (to remove NTDomain reference).
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ETIuser]') AND name = N'UK_ETIUser_DomainNTID')
DROP INDEX [UK_ETIUser_DomainNTID] ON [dbo].[ETIuser] WITH ( ONLINE = OFF )
GO
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ETIuser]') AND name = N'UK_ETIUser_NTID')
DROP INDEX [UK_ETIUser_NTID] ON [dbo].[ETIuser] WITH ( ONLINE = OFF )
GO

CREATE UNIQUE NONCLUSTERED INDEX [UK_ETIUser_NTID] ON [dbo].[ETIuser]
(
	[NTID] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = OFF, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO

-- Drop and recreate index (to remove NTDomain reference).
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[UserLog]') AND name = N'UK_UserLog_DomainNTID')
DROP INDEX [UK_UserLog_DomainNTID] ON [dbo].[UserLog] WITH ( ONLINE = OFF )
GO
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[UserLog]') AND name = N'UK_UserLog_NTID')
DROP INDEX [UK_UserLog_NTID] ON [dbo].[UserLog] WITH ( ONLINE = OFF )
GO

CREATE UNIQUE NONCLUSTERED INDEX [UK_UserLog_NTID] ON [dbo].[UserLog]
(
	[NTID] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = OFF, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO

-- Drop column from ETIGroup.
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'GroupDomain' AND Object_ID = Object_ID(N'[dbo].[ETIGroup]'))
ALTER TABLE [dbo].[ETIGroup] DROP COLUMN GroupDomain
GO

-- Drop column from ETIuser.
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'NTDomain' AND Object_ID = Object_ID(N'[dbo].[ETIuser]'))
ALTER TABLE dbo.[ETIuser] DROP COLUMN NTDomain
GO

-- Drop column from UserAccessReport.
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'NTDomain' AND Object_ID = Object_ID(N'[dbo].[UserAccessReport]'))
ALTER TABLE dbo.[UserAccessReport] DROP COLUMN NTDomain
GO

-- Drop column from UserLog.
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'NTDomain' AND Object_ID = Object_ID(N'[dbo].[UserLog]'))
ALTER TABLE dbo.[UserLog] DROP COLUMN NTDomain
GO

/*
	11/28/17 [pattoncr] - BOEJ-2839 AD utils - NTID uniqueness - Step 2 (Remove Domain)
	## END ##
*/

/*
       ## START ##
       11/29/17 [brunworg] - BOEJ-2830 - Do not allow deletion of a resource if INL forms are using it
*/

-- Remove any bad data. Should be minimal if any
DELETE FROM [dbo].[BOEFormPBOEResourcesXREF] WHERE ResourceId NOT IN (SELECT ResourceId FROM dbo.Resource);
DELETE FROM [dbo].[BOEFormIBOEResourcesXREF] WHERE ResourceId NOT IN (SELECT ResourceId FROM dbo.Resource);

-- Add FK Constraint for BOEFormIBOEResourcesXREF ResourceID field
IF NOT EXISTS(select * from sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FK_BOEFormIBOEResourcesXREF_Resource]'))
ALTER TABLE [dbo].[BOEFormIBOEResourcesXREF] WITH CHECK ADD CONSTRAINT [FK_BOEFormIBOEResourcesXREF_Resource] FOREIGN KEY (ResourceID) REFERENCES [dbo].[Resource]([ResourceID]);
GO

-- Add FK Constraint for BOEFormPBOEResourcesXREF ResourceID field
IF NOT EXISTS(select * from sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FK_BOEFormPBOEResourcesXREF_Resource]'))
ALTER TABLE [dbo].[BOEFormPBOEResourcesXREF] WITH CHECK ADD CONSTRAINT [FK_BOEFormPBOEResourcesXREF_Resource] FOREIGN KEY (ResourceID) REFERENCES [dbo].[Resource]([ResourceID]);
GO

/*
       11/29/17 [brunworg] - BOEJ-2830 - Do not allow deletion of a resource if INL forms are using it
       ## END ##
*/

/*
	## START ##
	
	12/19/17		Dusan - Add a new template for RMS (originally for USS)
*/

SET IDENTITY_INSERT [dbo].[OutputFormatTemplate] ON
IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessId > 2001 AND LineOfBusinessId < 2999) -- RMS only
	AND NOT EXISTS(SELECT 1 FROM [dbo].[OutputFormatTemplate] WHERE TemplateID = 2010) -- does not exist yet
BEGIN
	INSERT INTO [dbo].[OutputFormatTemplate] (TemplateID, UpdateDT, Template, TemplateDescription, IsActive, ParentTemplateID) 
		VALUES (2010, GETDATE(), 'RMS - Portrait - CLIN-WBS with SOW ID', 'RMS - Portrait - CLIN-WBS with SOW ID', 1, 2010)
END
SET IDENTITY_INSERT [dbo].[OutputFormatTemplate] OFF

GO
/*
	12/19/17		Dusan - Add a new template for RMS (originally for USS)
	
	## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '2', @AppVersion = '2.19';
GO
