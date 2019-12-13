EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2019.5';
GO

/*
		## START ##
		7/12/19		twilson3			BOEJ-4037	System Pro Pricer Export 
*/
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemProPricerExport]') AND type in (N'U'))
BEGIN
	--Create System tables
	CREATE TABLE [dbo].[SystemProPricerExport](
	[SystemProPricerExportID] [int] IDENTITY(1,1) NOT NULL,
	[UpdateDT] [datetime2](7) NOT NULL,
	[ProPricerExportName] [varchar](100) NOT NULL
 CONSTRAINT [PK_SystemProPricerExport] PRIMARY KEY CLUSTERED 
(
	[SystemProPricerExportID] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[SystemProPricerFieldXREF](
	[PFID] [int] IDENTITY(1,1) NOT NULL,
	[SystemProPricerExportID] [int] NOT NULL,
	[ProPricerFieldID] [int] NOT NULL,
	[ProPricerTypeID] [int] NOT NULL,
	[ListOrder] [tinyint] NOT NULL,
 CONSTRAINT [PK_SystemProPricerFieldXREF] PRIMARY KEY CLUSTERED 
(
	[PFID] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[SystemProPricerFieldXREF]  WITH CHECK ADD  CONSTRAINT [FK_SystemProPricerFieldXREF_SystemProPricerExport] FOREIGN KEY([SystemProPricerExportID])
REFERENCES [dbo].[SystemProPricerExport] ([SystemProPricerExportID])

ALTER TABLE [dbo].[SystemProPricerFieldXREF] CHECK CONSTRAINT [FK_SystemProPricerFieldXREF_SystemProPricerExport]

ALTER TABLE [dbo].[SystemProPricerFieldXREF]  WITH CHECK ADD  CONSTRAINT [FK_SystemProPricerFieldXREF_ProPricerFieldLU] FOREIGN KEY([ProPricerFieldID])
REFERENCES [dbo].[ProPricerFieldLU] ([ProPricerFieldID])

ALTER TABLE [dbo].[SystemProPricerFieldXREF] CHECK CONSTRAINT [FK_SystemProPricerFieldXREF_ProPricerFieldLU]

ALTER TABLE [dbo].[SystemProPricerFieldXREF]  WITH CHECK ADD  CONSTRAINT [FK_SystemProPricerFieldXREF_ProPricerTypeLU] FOREIGN KEY([ProPricerTypeID])
REFERENCES [dbo].[ProPricerTypeLU] ([ProPricerTypeID])

ALTER TABLE [dbo].[SystemProPricerFieldXREF] CHECK CONSTRAINT [FK_SystemProPricerFieldXREF_ProPricerTypeLU]

CREATE TABLE [dbo].[SystemProPricerCustomFieldXREF](
	[PCID] [int] IDENTITY(1,1) NOT NULL,
	[SystemProPricerExportID] [int] NOT NULL,
	[CustomField] [varchar](100) NOT NULL,
	[ProPricerTypeID] [int] NOT NULL,
	[ProPricerCustomFieldSelectionID] [int] NOT NULL,
	[ListOrder] [tinyint] NOT NULL,
 CONSTRAINT [PK_SystemProPricerCustomFieldXREF] PRIMARY KEY CLUSTERED 
(
	[PCID] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[SystemProPricerCustomFieldXREF]  WITH CHECK ADD  CONSTRAINT [FK_SystemProPricerCustomFieldXREF_ProPricerCustomFieldSelectionLU] FOREIGN KEY([ProPricerCustomFieldSelectionID])
REFERENCES [dbo].[ProPricerCustomFieldSelectionLU] ([ProPricerCustomFieldSelectionID])

ALTER TABLE [dbo].[SystemProPricerCustomFieldXREF] CHECK CONSTRAINT [FK_SystemProPricerCustomFieldXREF_ProPricerCustomFieldSelectionLU]

ALTER TABLE [dbo].[SystemProPricerCustomFieldXREF]  WITH CHECK ADD  CONSTRAINT [FK_SystemProPricerCustomFieldXREF_SystemProPricerExport] FOREIGN KEY([SystemProPricerExportID])
REFERENCES [dbo].[SystemProPricerExport] ([SystemProPricerExportID])

ALTER TABLE [dbo].[SystemProPricerCustomFieldXREF] CHECK CONSTRAINT [FK_SystemProPricerCustomFieldXREF_SystemProPricerExport]

ALTER TABLE [dbo].[SystemProPricerCustomFieldXREF]  WITH CHECK ADD  CONSTRAINT [FK_SystemProPricerCustomFieldXREF_ProPricerTypeLU] FOREIGN KEY([ProPricerTypeID])
REFERENCES [dbo].[ProPricerTypeLU] ([ProPricerTypeID])

ALTER TABLE [dbo].[SystemProPricerCustomFieldXREF] CHECK CONSTRAINT [FK_SystemProPricerCustomFieldXREF_ProPricerTypeLU]

SET IDENTITY_INSERT [dbo].[SystemProPricerExport] ON

-- copy the data from old tables to new tables
INSERT INTO [dbo].[SystemProPricerExport] ([SystemProPricerExportID], [UpdateDT], [ProPricerExportName])
SELECT [ProPricerExportID], [UpdateDT], [ProPricerExportName] FROM [dbo].[ProPricerExport]
WHERE ProPricerScopeID = 2
-- Scope ID 2 is System

SET IDENTITY_INSERT [dbo].[SystemProPricerExport] OFF

INSERT INTO [dbo].[SystemProPricerFieldXREF] ([SystemProPricerExportID], [ProPricerFieldID], [ProPricerTypeID], [ListOrder])
SELECT xf.[ProPricerExportID], xf.[ProPricerFieldID], xf.[ProPricerTypeID], xf.[ListOrder] FROM [dbo].[ProPricerFieldXREF] xf
INNER JOIN [dbo].[ProPricerExport] p on p.ProPricerExportID = xf.ProPricerExportID
WHERE p.ProPricerScopeID = 2

INSERT INTO [dbo].[SystemProPricerCustomFieldXREF] ([SystemProPricerExportID], [CustomField], [ProPricerTypeID], [ProPricerCustomFieldSelectionID], [ListOrder])
SELECT xf.[ProPricerExportID], cf.CustomFieldName, xf.[ProPricerTypeID], xf.[ProPricerCustomFieldSelectionID], xf.[ListOrder] FROM [dbo].[ProPricerCustomFieldXREF] xf
INNER JOIN [dbo].[ProPricerExport] p on p.ProPricerExportID = xf.ProPricerExportID
INNER JOIN [dbo].[CustomField] cf on cf.CustomFieldID = xf.CustomFieldID
WHERE p.ProPricerScopeID = 2

-- remove the data from old tables

DELETE FROM [dbo].[ProPricerFieldXREF] WHERE [ProPricerExportID] IN (select ProPricerExportID from ProPricerExport WHERE ProPricerScopeID = 2)
DELETE FROM [dbo].[ProPricerCustomFieldXREF] WHERE [ProPricerExportID] IN (select ProPricerExportID from ProPricerExport WHERE ProPricerScopeID = 2)

DELETE FROM [dbo].[ProPricerExport] WHERE ProPricerScopeID = 2

-- drop ProPricerScopeID constraint, column, and table
ALTER TABLE [dbo].[ProPricerExport] DROP CONSTRAINT [FK_ProPricerExport_ProPricerScopeLU]
ALTER TABLE [dbo].[ProPricerExport]	DROP COLUMN ProPricerScopeID
DROP TABLE ProPricerScopeLU
ALTER TABLE [version].[ProPricerExport]	DROP COLUMN ProPricerScopeID

END
GO

/*
       7/12/19		twilson3			BOEJ-4037	System Pro Pricer Export 

       ## END ##
*/
/*
		## START ##
		8/21/19		twilson3			BOEJ-4297	SSRS Report Changes
*/

UPDATE [dbo].[ReportLU] SET ReportName = 'Engineering Offload Detailed Calculation Results' WHERE ReportName = 'Offload Detailed Report'
UPDATE [dbo].[ReportLU] SET ReportName = 'Estimates by CLIN, Activity, and CY' WHERE ReportName = 'Flattened Cost by CLIN, Res, Activity 8 yr. (DCMA)'
UPDATE [dbo].[ReportLU] SET ReportName = 'Estimates by CLIN, Activity, and CY' WHERE ReportName = 'Cost Analysis by CLIN, Activity and CY - 8 Yrs'

/*
       8/21/19		twilson3			BOEJ-4297	SSRS Report Changes

       ## END ##
*/

/*
		## START ##
		9/10/19		twilson3			BOEJ-4342  Fix Project Map values
*/

UPDATE [dbo].[ProjectMap] SET addordelete = 'A' WHERE addordelete = '1'
UPDATE [dbo].[ProjectMap] SET addordelete = 'D' WHERE addordelete = '2'
UPDATE [dbo].[ProjectMap] SET classofcost = 'REC' WHERE addordelete = '1'
UPDATE [dbo].[ProjectMap] SET classofcost = 'NRE' WHERE addordelete = '2'
UPDATE [dbo].[ProjectMap] SET classofcost = 'DNR' WHERE addordelete = '3'
UPDATE [dbo].[ProjectMap] SET classofcost = NULL WHERE addordelete = '0'

/*
       9/10/19		twilson3			BOEJ-4342  Fix Project Map values

       ## END ##
*/