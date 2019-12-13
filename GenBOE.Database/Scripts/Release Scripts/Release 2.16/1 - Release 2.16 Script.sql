/*
	## START ##

	6/23/17 [twilson3] -- BOEJ-2357 Remove NGI Output Format Template
*/

UPDATE [OutputFormatTemplate] SET IsActive = 0 WHERE TemplateID = 7

/*
	6/23/17 [twilson3] -- BOEJ-2357 Remove NGI Output Format Template

	## END ##
*/

/*
	## START ##

	7/5/17 [twilson3] -- BOEJ-2359 Store SSRS Xml in DB
*/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReportXmlData]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[ReportXmlData](
		Nonce VARCHAR(40) NOT NULL,
		[Xml] VARCHAR(MAX) NOT NULL,
		UpdateDT Datetime2(7) NOT NULL,
	)
END
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getConfigurationValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getConfigurationValue]

GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Configuration]') AND type in (N'U'))
BEGIN
	DROP Table [dbo].[Configuration]
END
GO
/*
	7/5/17 [twilson3] -- BOEJ-2359 Store SSRS Xml in DB

	## END ##
*/

/*
	## START ##

	7/6/17 [twilson3] -- BOEJ-2372 Recurring/Non-Recurring 
*/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ClassOfCost' AND Object_ID = Object_ID('[dbo].[BOE]'))
BEGIN
	ALTER TABLE [dbo].[BOE] ADD ClassOfCost INT NULL
END

GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ClassOfCost' AND Object_ID = Object_ID('[version].[BOE]'))
BEGIN
	ALTER TABLE [version].[BOE] ADD ClassOfCost INT NULL
END
GO
/*
	7/6/17 [twilson3] -- BOEJ-2372 Recurring/Non-Recurring 

	## END ##
*/

/*
	7/19/17 [Joe] -- Insert ProjectMap company ID to support custom list of ProPricer export fields just for ProjectMap type workspace

	## START ##
*/

IF NOT EXISTS ( SELECT 1 FROM [dbo].[ProPricerCompanyLU] WHERE [ProPricerCompanyID]=4)
BEGIN
	INSERT INTO [dbo].[ProPricerCompanyLU] ([ProPricerCompanyID], [ProPricerCompany])
	VALUES (4, 'ProjectMap');
END

/*
	7/19/17 [Joe] -- Insert ProjectMap company ID to support custom list of ProPricer export fields just for ProjectMap type workspace

	## END ##
*/

/*
	## START ##

	7/19/17 [Joe] - Multiple SQL scripts
	
	- Project Map ProPricer Task and Resource Fields
	- Rename of Project CLIN Cost Summary Report
	- Insert Class Of Cost Field for ProPricer
*/

IF NOT EXISTS ( SELECT 1 FROM [dbo].[ProPricerFieldLU] WHERE [ProPricerFieldID]=57)
BEGIN
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(57,'Task ID',1,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(58,'Activity Name',1,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(59,'Start Date',1,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(60,'End Date',1,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(61,'Quantity (Always 1)',1,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(62,'CAM Name',1,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(63,'WBS Number',1,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(64,'Cost Center',1,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(65,'CLIN',1,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(66,'SOW Number',1,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(67,'Add/Delete',1,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(72,'Category',1,4);


	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(68,'Task ID',2,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(69,'Initial Resource',2,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(70,'Spread Code (Always D)',2,4);
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID]) VALUES(71,'Start Date',2,4);
END
GO

DELETE FROM [dbo].[ReportLU] WHERE [ReportID]=21;
GO

UPDATE [dbo].[ReportLU] 
	SET [ReportName]='Project CLIN Cost Summary', [Description]='Project CLIN cost summary estimate sorted by CLIN, activity, resource, start and end date'
	WHERE [ReportID]=20;
GO

IF NOT EXISTS ( SELECT 1 FROM [dbo].[ProPricerFieldLU] WHERE [ProPricerFieldID]=73)
BEGIN
	INSERT INTO [dbo].[ProPricerFieldLU] ([ProPricerFieldID], [ProPricerField], [ProPricerTypeID], [ProPricerCompanyID])
	VALUES (73, 'Class of Cost', 1, 4);
END
GO

/*
	7/19/17 [Joe] - Multiple SQL scripts
	
	- Project Map ProPricer Task and Resource Fields
	- Rename of Project CLIN Cost Summary Report
	- Insert Class Of Cost Field for ProPricer

	## END ##
*/

/*
	7/19/17 [momeara] -- BOEJ-2412 RMS Feedback (round 3?) 

	## START ##
*/

ALTER TABLE [dbo].[BOE] ALTER COLUMN [Rationale] VARCHAR(MAX);
go

ALTER TABLE [version].[BOE] ALTER COLUMN [Rationale] VARCHAR(MAX);
go

/*
	7/19/17 [momeara] -- BOEJ-2412 RMS Feedback (round 3?) 

	## END ##
*/

/*
	8/16/17 [Dusan] -- BOEJ-2474: Rename Initial Resource to Resource

	## START ##
*/
UPDATE [dbo].[ProPricerFieldLU] SET [ProPricerField] = 'Resource' WHERE [ProPricerFieldID] = 69;
GO
/*
	8/16/17 [Dusan] -- BOEJ-2474: Rename Initial Resource to Resource

	## END ##
*/

EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2.16';
GO