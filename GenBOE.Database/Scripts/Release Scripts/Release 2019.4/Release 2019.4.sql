EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2019.4';
GO

-- One time data cleanup, driven by contract changes in PTM
UPDATE ContractTYpeLU SET IsActive = 0 WHERE ContractType = 'OTA';

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'WorkspaceCreationDate' AND Object_ID = Object_ID('[dbo].[Workspace]'))
BEGIN
	ALTER TABLE [dbo].[Workspace]
	ADD WorkspaceCreationDate datetime2(7) NULL
	CONSTRAINT DF_Workspace_WorkspaceCreationDate DEFAULT GETDATE()
END
GO

/*
		## START ##
		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, Material Tables
*/
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMaterial]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMaterial];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMaterialCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMaterialCustomFieldValue];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMaterialSpreadByMaterialID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMaterialSpreadByMaterialID];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteMaterialTaskElementCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteMaterialTaskElementCustomFieldValue];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertMaterialSpread]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertMaterialSpread];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertMaterial]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertMaterial];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertMaterialCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertMaterialCustomFieldValue];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertMaterialTaskElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertMaterialTaskElement];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertMaterialTaskElementCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertMaterialTaskElementCustomFieldValue];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertODCTypeCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertODCTypeCustomFieldValue];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertODCTaskElementCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertODCTaskElementCustomFieldValue];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteODCTypeCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteODCTypeCustomFieldValue];

GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteODCTaskElementCustomFieldValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteODCTaskElementCustomFieldValue];

GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MaterialSpread]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[MaterialSpread];
	DROP TABLE [dbo].[MaterialCustomFieldValueXREF];
	DROP TABLE [dbo].[MaterialTaskElementCustomFieldValueXREF];
	DROP TABLE [dbo].[Material];
	DROP TABLE [dbo].[ODCTypeCustomFieldValueXREF];
	DROP TABLE [dbo].[ODCTaskElementCustomFieldValueXREF];


	DROP TABLE [version].[MaterialSpread];
	DROP TABLE [version].[MaterialCustomFieldValueXREF];
	DROP TABLE [version].[MaterialTaskElementCustomFieldValueXREF];
	DROP TABLE [version].[Material];
	DROP TABLE [version].[ODCTypeCustomFieldValueXREF];
	DROP TABLE [version].[ODCTaskElementCustomFieldValueXREF];
	
END
GO
/*
		6/25/19		twilson3			BOEJ-3964 - Remove in-use flag, Material Tables
		## END ##
*/

/*
       ## START ##

       05/28/2019	twilson3	BOEJ-4211 DateShift Emails
*/
IF NOT EXISTS (SELECT 1 FROM [dbo].[EmailLU] WHERE EmailID = 48)
BEGIN
	
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (48, 'genBOE: An error has occurred when a BOE date(s) was modified by {0} while performing a {1}', 'During the performance of this date shift an error occurred as a result of the decision to not flow down the date changes.<BR/>{0} has modified Start and /or End Dates for the following BOEs. BOEs that were Awaiting Approval or Approved are now in Draft.<BR/><BR/>Workspace/Proposal: {1}<BR/>{2}<BR/>{3}', 'Date Adjust (emails for errors only)', 0, NULL, 'Other', 'Author(s) on affected BOEs and Approver(s) on Awaiting Approval/Approved BOEs');
	
	UPDATE [dbo].[EmailLU] SET Subject = 'genBOE: BOE Start and End Date has been modified by {0} while performing a {1}'
		where EmailID = 42;
	
END
/*
       05/28/2019	twilson3	BOEJ-4211 DateShift Emails

       ## END ##
*/