EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.3';
GO

/*
		## START ##
		2/22/18 Dusan - BOEJ-3144 - Increase Performing Org Size from 30 -> 50
*/

ALTER TABLE [dbo].PerformingOrganization ALTER COLUMN PerformingOrganizationDescription VARCHAR(50);
ALTER TABLE [version].PerformingOrganization ALTER COLUMN PerformingOrganizationDescription VARCHAR(50);
GO

/*
		2/22/18 Dusan - BOEJ-3144 - Increase Performing Org Size from 30 -> 50
		## END ##
*/

/*
       ## START ##
       3/6/18       ranzalon             BOEJ-3097 – Open Ended Custom Fields
*/

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsOpenEnded' AND Object_ID = Object_ID(N'[dbo].[CustomField]'))
BEGIN
	ALTER TABLE [dbo].[CustomField]
	ADD [IsOpenEnded] BIT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsOpenEnded' AND Object_ID = Object_ID(N'[version].[CustomField]'))
BEGIN
	ALTER TABLE [version].[CustomField]
	ADD [IsOpenEnded] BIT;
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'CustomFieldValueDescription' AND Object_ID = Object_ID('[dbo].[CustomFieldValue]'))
BEGIN
	ALTER TABLE [dbo].[CustomFieldValue]
	ALTER COLUMN [CustomFieldValueDescription] VARCHAR(250);
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'CustomFieldValueDescription' AND Object_ID = Object_ID('[version].[CustomFieldValue]'))
BEGIN
	ALTER TABLE [version].[CustomFieldValue]
	ALTER COLUMN [CustomFieldValueDescription] VARCHAR(250);
END

/*
       3/6/18       ranzalon             BOEJ-3097 – Open Ended Custom Fields
	   ## END ##
*/

/*
		## START ##
		4/2/2018     twilson3      BOEJ-3244 PTM BOE Data inconsistent
*/
-- These changes are to be executed in SSC only. The way we can tell the environments apart is that SSC has LOBs in the range of 1000's. RMS is 2000+ and ISGS is 0-999
IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessID > 1000 AND LineOfBusinessID < 1999)
BEGIN
	Update [LineOfBusiness] SET [LineOfBusinessName] = 'Advanced Technology Center' WHERE [LineOfBusinessName] = 'Advanced Technology Center (ATC)';
	Update [LineOfBusiness] SET [LineOfBusinessName] = 'Military Space' WHERE [LineOfBusinessName] = 'Military Space (MS)';
	Update [LineOfBusiness] SET [LineOfBusinessName] = 'Mission Solutions' WHERE [LineOfBusinessName] = 'Mission Solutions (MsnSln)';
	Update [LineOfBusiness] SET [LineOfBusinessName] = 'Strategic & Missile Defense' WHERE [LineOfBusinessName] = 'Strategic Missile Defense (SMD)';
	Update [LineOfBusiness] SET [LineOfBusinessName] = 'Special Programs' WHERE [LineOfBusinessName] = 'Special Programs (SP)';
	Update [LineOfBusiness] SET IsActive = 0 WHERE [LineOfBusinessName] IN ('Commercial Ventures (COM)', 'Civil Space (CS)', 'Production Ops (PO)');
	
	IF NOT EXISTS (SELECT 1 FROM [dbo].[ContractTypeLU] WHERE [ContractType] = 'OTA')
	BEGIN
		SET IDENTITY_INSERT [dbo].[LineOfBusiness] ON
		INSERT INTO [LineOfBusiness] ([LineOfBusinessID],[LineOfBusinessName],[LineOfBusinessLongName],[LineOfBusinessURL],[ForesightLineOfBusinessID],[IsActive]) VALUES (1009, 'Commercial/Civil Space', 'Commercial/Civil Space', 'CommCivil', -1, 1);
		SET IDENTITY_INSERT [dbo].[LineOfBusiness] OFF
		INSERT INTO [ContractTypeLU] ([ContractTypeID], [ContractType]) VALUES (1022, 'OTA');
		INSERT INTO [ContractTypeLU] ([ContractTypeID], [ContractType]) VALUES (1023, 'IWTAFCC');
		INSERT INTO [ContractTypeLU] ([ContractTypeID], [ContractType]) VALUES (1024, 'IWTAFWP');
		INSERT INTO [ContractTypeLU] ([ContractTypeID], [ContractType]) VALUES (1025, 'FPIA');
	END
END

/*
		4/2/2018     twilson3      BOEJ-3244 PTM BOE Data inconsistent

		## END ##
*/